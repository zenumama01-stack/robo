using System.Management.Automation.PSTasks;
using CommonParamSet = System.Management.Automation.Internal.CommonParameters;
using NotNullWhen = System.Diagnostics.CodeAnalysis.NotNullWhenAttribute;
    /// A thin wrapper over a property-getting Callsite, to allow reuse when possible.
    internal struct DynamicPropertyGetter
        // For the wildcard case, lets us know if we can reuse the callsite:
        private string _lastUsedPropertyName;
        public object GetValue(PSObject inputObject, string propertyName)
            Dbg.Assert(!WildcardPattern.ContainsWildcardCharacters(propertyName), "propertyName should be pre-resolved by caller");
            // If wildcards are involved, the resolved property name could potentially
            // be different on every object... but probably not, so we'll attempt to
            // reuse the callsite if possible.
            if (!propertyName.Equals(_lastUsedPropertyName, StringComparison.OrdinalIgnoreCase))
                _lastUsedPropertyName = propertyName;
                _getValueDynamicSite = CallSite<Func<CallSite, object, object>>.Create(
            return _getValueDynamicSite.Target.Invoke(_getValueDynamicSite, inputObject);
    #region Built-in cmdlets that are used by or require direct access to the engine.
    [Cmdlet("ForEach", "Object", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Low,
        DefaultParameterSetName = ForEachObjectCommand.ScriptBlockSet, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096867",
    public sealed class ForEachObjectCommand : PSCmdlet, IDisposable
        private const string ParallelParameterSet = "ParallelParameterSet";
        private const string ScriptBlockSet = "ScriptBlockSet";
        private const string PropertyAndMethodSet = "PropertyAndMethodSet";
        #region Common Parameters
        [Parameter(ValueFromPipeline = true, ParameterSetName = ForEachObjectCommand.ScriptBlockSet)]
        [Parameter(ValueFromPipeline = true, ParameterSetName = ForEachObjectCommand.PropertyAndMethodSet)]
        [Parameter(ValueFromPipeline = true, ParameterSetName = ForEachObjectCommand.ParallelParameterSet)]
        #region ScriptBlockSet
        private readonly List<ScriptBlock> _scripts = new List<ScriptBlock>();
        /// Gets or sets the script block to apply in begin processing.
        [Parameter(ParameterSetName = ForEachObjectCommand.ScriptBlockSet)]
        public ScriptBlock Begin
                _scripts.Insert(0, value);
        /// Gets or sets the script block to apply.
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ForEachObjectCommand.ScriptBlockSet)]
        public ScriptBlock[] Process
                    _scripts.Add(null);
                    _scripts.AddRange(value);
        private ScriptBlock _endScript;
        private bool _setEndScript;
        /// Gets or sets the script block to apply in complete processing.
        public ScriptBlock End
                return _endScript;
                _endScript = value;
                _setEndScript = true;
        /// Gets or sets the remaining script blocks to apply.
        [Parameter(ParameterSetName = ForEachObjectCommand.ScriptBlockSet, ValueFromRemainingArguments = true)]
        public ScriptBlock[] RemainingScripts
        private int _start, _end;
        #endregion ScriptBlockSet
        #region PropertyAndMethodSet
        /// Gets or sets the property or method name.
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ForEachObjectCommand.PropertyAndMethodSet)]
                return _propertyOrMethodName;
                _propertyOrMethodName = value;
        private string _propertyOrMethodName;
        private string _targetString;
        private DynamicPropertyGetter _propGetter;
        /// The arguments passed to a method invocation.
        [Parameter(ParameterSetName = ForEachObjectCommand.PropertyAndMethodSet, ValueFromRemainingArguments = true)]
            get { return _arguments; }
            set { _arguments = value; }
        private object[] _arguments;
        #endregion PropertyAndMethodSet
        #region ParallelParameterSet
        /// Gets or sets a script block to run in parallel for each pipeline object.
        [Parameter(Mandatory = true, ParameterSetName = ForEachObjectCommand.ParallelParameterSet)]
        public ScriptBlock Parallel { get; set; }
        /// Gets or sets the maximum number of concurrently running scriptblocks on separate threads.
        /// The default number is 5.
        [Parameter(ParameterSetName = ForEachObjectCommand.ParallelParameterSet)]
        [ValidateRange(1, Int32.MaxValue)]
        public int ThrottleLimit { get; set; } = 5;
        /// Gets or sets a timeout time in seconds, after which the parallel running scripts will be stopped
        /// The default value is 0, indicating no timeout.
        [ValidateRange(0, (Int32.MaxValue / 1000))]
        public int TimeoutSeconds { get; set; }
        /// Gets or sets a flag that returns a job object immediately for the parallel operation, instead of returning after
        /// all foreach processing is completed.
        public SwitchParameter AsJob { get; set; }
        /// Gets or sets a flag so that a new runspace object is created for each loop iteration, instead of reusing objects
        /// from the runspace pool.
        /// By default, runspaces are reused from a runspace pool.
        public SwitchParameter UseNewRunspace { get; set; }
        /// Execute the begin scriptblock at the start of processing.
        /// <exception cref="ParseException">Could not parse script.</exception>
        /// <exception cref="RuntimeException">See Pipeline.Invoke.</exception>
        /// <exception cref="ParameterBindingException">See Pipeline.Invoke.</exception>
                case ForEachObjectCommand.ScriptBlockSet:
                    InitScriptBlockParameterSet();
                case ForEachObjectCommand.ParallelParameterSet:
                    InitParallelParameterSet();
        /// Execute the processing script blocks on the current pipeline object
        /// which is passed as it's only parameter.
                    ProcessScriptBlockParameterSet();
                case ForEachObjectCommand.PropertyAndMethodSet:
                    ProcessPropertyAndMethodParameterSet();
                    ProcessParallelParameterSet();
        /// Execute the end scriptblock when the pipeline is complete.
                    EndBlockParameterSet();
                    EndParallelParameterSet();
        /// Handle pipeline stop signal.
                    StopParallelProcessing();
        /// Dispose cmdlet instance.
            // Ensure all parallel task objects are disposed
            _taskTimer?.Dispose();
            _taskDataStreamWriter?.Dispose();
            _taskPool?.Dispose();
            _taskCollection?.Dispose();
        #region PSTasks
        private PSTaskPool _taskPool;
        private PSTaskDataStreamWriter _taskDataStreamWriter;
        private Dictionary<string, object> _usingValuesMap;
        private Timer _taskTimer;
        private PSTaskJob _taskJob;
        private PSDataCollection<System.Management.Automation.PSTasks.PSTask> _taskCollection;
        private Exception _taskCollectionException;
        private string _currentLocationPath;
        private void InitParallelParameterSet()
            // The following common parameters are not (yet) supported in this parameter set.
            //  ErrorAction, WarningAction, InformationAction, ProgressAction, PipelineVariable.
            if (MyInvocation.BoundParameters.ContainsKey(nameof(CommonParamSet.ErrorAction)) ||
                MyInvocation.BoundParameters.ContainsKey(nameof(CommonParamSet.WarningAction)) ||
                MyInvocation.BoundParameters.ContainsKey(nameof(CommonParamSet.InformationAction)) ||
                MyInvocation.BoundParameters.ContainsKey(nameof(CommonParamSet.ProgressAction)) ||
                MyInvocation.BoundParameters.ContainsKey(nameof(CommonParamSet.PipelineVariable)))
                            new PSNotSupportedException(InternalCommandStrings.ParallelCommonParametersNotSupported),
                            "ParallelCommonParametersNotSupported",
            // Get the current working directory location, if available.
                _currentLocationPath = SessionState.Internal.CurrentLocation.Path;
            var allowUsingExpression = this.Context.SessionState.LanguageMode != PSLanguageMode.NoLanguage;
            _usingValuesMap = ScriptBlockToPowerShellConverter.GetUsingValuesForEachParallel(
                scriptBlock: Parallel,
                isTrustedInput: allowUsingExpression,
                context: this.Context);
            // Validate using values map, which is a map of '$using:' variables referenced in the script.
            // Script block variables are not allowed since their behavior is undefined outside the runspace
            // in which they were created.
            foreach (object item in _usingValuesMap.Values)
                if (item is ScriptBlock or PSObject { BaseObject: ScriptBlock })
                            new PSArgumentException(InternalCommandStrings.ParallelUsingVariableCannotBeScriptBlock),
                            "ParallelUsingVariableCannotBeScriptBlock",
            if (AsJob)
                // Set up for returning a job object.
                if (MyInvocation.BoundParameters.ContainsKey(nameof(TimeoutSeconds)))
                            new PSArgumentException(InternalCommandStrings.ParallelCannotUseTimeoutWithJob),
                            "ParallelCannotUseTimeoutWithJob",
                _taskJob = new PSTaskJob(
                    Parallel.ToString(),
                    ThrottleLimit,
                    UseNewRunspace);
            // Set up for synchronous processing and data streaming.
            _taskCollection = new PSDataCollection<System.Management.Automation.PSTasks.PSTask>();
            _taskDataStreamWriter = new PSTaskDataStreamWriter(this);
            _taskPool = new PSTaskPool(ThrottleLimit, UseNewRunspace);
            _taskPool.PoolComplete += (sender, args) => _taskDataStreamWriter.Close();
            // Create timeout timer if requested.
            if (TimeoutSeconds != 0)
                _taskTimer = new Timer(
                    callback: (_) => { _taskCollection.Complete(); _taskPool.StopAll(); },
                    dueTime: TimeoutSeconds * 1000,
                    period: Timeout.Infinite);
            // Task collection handler.
            System.Threading.ThreadPool.QueueUserWorkItem(
                    // As piped input are converted to PSTasks and added to the _taskCollection,
                    // transfer the task to the _taskPool on this dedicated thread.
                    // The _taskPool will block this thread when it is full, and allow more tasks to
                    // be added only when a currently running task completes and makes space in the pool.
                    // Continue adding any tasks appearing in _taskCollection until the collection is closed.
                        // This handle will unblock the thread when a new task is available or the _taskCollection
                        // is closed.
                        _taskCollection.WaitHandle.WaitOne();
                        // Task collection open state is volatile.
                        // Record current task collection open state here, to be checked after processing.
                        bool isOpen = _taskCollection.IsOpen;
                            // Read all tasks in the collection.
                            foreach (var task in _taskCollection.ReadAll())
                                // This _taskPool method will block if the pool is full and will unblock
                                // only after a task completes making more space.
                                _taskPool.Add(task);
                            _taskCollection.Complete();
                            _taskCollectionException = ex;
                            _taskDataStreamWriter.Close();
                        // Loop is exited only when task collection is closed and all task
                        // collection tasks are processed.
                        if (!isOpen)
                    // We are done adding tasks and can close the task pool.
                    _taskPool.Close();
        private void ProcessParallelParameterSet()
            // Validate piped InputObject
            if (_inputObject != null &&
                _inputObject.BaseObject is ScriptBlock)
                            new PSArgumentException(InternalCommandStrings.ParallelPipedInputObjectCannotBeScriptBlock),
                            "ParallelPipedInputObjectCannotBeScriptBlock",
                // Add child task job.
                var taskChildJob = new PSTaskChildJob(
                    Parallel,
                    _usingValuesMap,
                    InputObject,
                    _currentLocationPath);
                _taskJob.AddJob(taskChildJob);
            // Write any streaming data
            _taskDataStreamWriter.WriteImmediate();
            // Add to task collection for processing.
            if (_taskCollection.IsOpen)
                    // Create a PSTask based on this piped input and add it to the task collection.
                    // A dedicated thread will add it to the PSTask pool in a performant manner.
                    _taskCollection.Add(
                        new System.Management.Automation.PSTasks.PSTask(
                            _currentLocationPath,
                            _taskDataStreamWriter));
                    // This exception is thrown if the task collection is closed, which should not happen.
                    Dbg.Assert(false, "Should not add to a closed PSTask collection");
        private void EndParallelParameterSet()
                // Start and return parent job object.
                _taskJob.Start();
                JobRepository.Add(_taskJob);
                WriteObject(_taskJob);
            // Close task collection and wait for processing to complete while streaming data.
            _taskDataStreamWriter.WaitAndWrite();
            // Check for an unexpected error from the _taskCollection handler thread and report here.
            var ex = _taskCollectionException;
                var msg = string.Format(CultureInfo.InvariantCulture, InternalCommandStrings.ParallelPipedInputProcessingError, ex);
                        exception: new InvalidOperationException(msg),
                        errorId: "ParallelPipedInputProcessingError",
                        errorCategory: ErrorCategory.InvalidOperation,
                        targetObject: this));
        private void StopParallelProcessing()
            _taskCollection?.Complete();
            _taskPool?.StopAll();
        private void EndBlockParameterSet()
            if (_endScript == null)
            _endScript.InvokeUsingCmdlet(
        private void ProcessPropertyAndMethodParameterSet()
            _targetString = string.Format(CultureInfo.InvariantCulture, InternalCommandStrings.ForEachObjectTarget, GetStringRepresentation(InputObject));
            if (LanguagePrimitives.IsNull(InputObject))
                if (_arguments != null && _arguments.Length > 0)
                    WriteError(GenerateNameParameterError("InputObject", ParserStrings.InvokeMethodOnNull,
                                                          "InvokeMethodOnNull", _inputObject));
                    // should process
                    string propertyAction = string.Format(CultureInfo.InvariantCulture,
                        InternalCommandStrings.ForEachObjectPropertyAction, _propertyOrMethodName);
                    if (ShouldProcess(_targetString, propertyAction))
                            WriteError(GenerateNameParameterError("InputObject", InternalCommandStrings.InputObjectIsNull,
                                                                  "InputObjectIsNull", _inputObject));
                            // we write null out because:
                            // PS C:\> $null | ForEach-object {$_.aa} | ForEach-Object {$_ + 3}
                            // 3
                            // so we also want
                            // PS C:\> $null | ForEach-object aa | ForEach-Object {$_ + 3}
                            // But if we don't write anything to the pipeline when _inputObject is null,
                            // the result 3 will not be generated.
                            WriteObject(null);
            // if args exist, this is explicitly a method invocation
                MethodCallWithArguments();
            // no arg provided
                // if inputObject is of IDictionary, get the value
                if (GetValueFromIDictionaryInput())
                if (WildcardPattern.ContainsWildcardCharacters(_propertyOrMethodName))
                    // get the matched member(s)
                    ReadOnlyPSMemberInfoCollection<PSMemberInfo> members =
                        _inputObject.Members.Match(_propertyOrMethodName, PSMemberTypes.All);
                    Dbg.Assert(members != null, "The return value of Members.Match should never be null");
                    if (members.Count > 1)
                        // write error record: property method ambiguous
                        StringBuilder possibleMatches = new StringBuilder();
                        foreach (PSMemberInfo item in members)
                            possibleMatches.Append(CultureInfo.InvariantCulture, $" {item.Name}");
                        WriteError(GenerateNameParameterError("Name", InternalCommandStrings.AmbiguousPropertyOrMethodName,
                                                              "AmbiguousPropertyOrMethodName", _inputObject,
                                                              _propertyOrMethodName, possibleMatches));
                    if (members.Count == 1)
                        member = members[0];
                    member = _inputObject.Members[_propertyOrMethodName];
                // member is a method
                if (member is PSMethodInfo)
                    // first we check if the member is a ParameterizedProperty
                    PSParameterizedProperty targetParameterizedProperty = member as PSParameterizedProperty;
                    if (targetParameterizedProperty != null)
                            InternalCommandStrings.ForEachObjectPropertyAction, targetParameterizedProperty.Name);
                        // ParameterizedProperty always take parameters, so we output the member.Value directly
                            WriteObject(member.Value);
                    PSMethodInfo targetMethod = member as PSMethodInfo;
                    Dbg.Assert(targetMethod != null, "targetMethod should not be null here.");
                        string methodAction = string.Format(CultureInfo.InvariantCulture,
                            InternalCommandStrings.ForEachObjectMethodActionWithoutArguments, targetMethod.Name);
                        if (ShouldProcess(_targetString, methodAction))
                            if (!BlockMethodInLanguageMode(InputObject))
                                object result = targetMethod.Invoke(Array.Empty<object>());
                                WriteToPipelineWithUnrolling(result);
                        // PipelineStoppedException can be caused by select-object
                        MethodException mex = ex as MethodException;
                        if (mex != null && mex.ErrorRecord != null && mex.ErrorRecord.FullyQualifiedErrorId == "MethodCountCouldNotFindBest")
                            WriteObject(targetMethod.Value);
                            WriteError(new ErrorRecord(ex, "MethodInvocationError", ErrorCategory.InvalidOperation, _inputObject));
                    string resolvedPropertyName = null;
                    bool isBlindDynamicAccess = false;
                        if ((_inputObject.BaseObject is IDynamicMetaObjectProvider) &&
                            !WildcardPattern.ContainsWildcardCharacters(_propertyOrMethodName))
                            // Let's just try a dynamic property access. Note that if it
                            // comes to depending on dynamic access, we are assuming it is a
                            // property; we don't have ETS info to tell us up front if it
                            // even exists or not, let alone if it is a method or something
                            // else.
                            // Note that this is "truly blind"--the name did not show up in
                            // GetDynamicMemberNames(), else it would show up as a dynamic
                            // member.
                            resolvedPropertyName = _propertyOrMethodName;
                            isBlindDynamicAccess = true;
                            errorRecord = GenerateNameParameterError("Name", InternalCommandStrings.PropertyOrMethodNotFound,
                                                                     "PropertyOrMethodNotFound", _inputObject,
                                                                     _propertyOrMethodName);
                        // member is [presumably] a property (note that it could be a
                        // dynamic property, if it shows up in GetDynamicMemberNames())
                        resolvedPropertyName = member.Name;
                    if (!string.IsNullOrEmpty(resolvedPropertyName))
                            InternalCommandStrings.ForEachObjectPropertyAction, resolvedPropertyName);
                                WriteToPipelineWithUnrolling(_propGetter.GetValue(InputObject, resolvedPropertyName));
                            catch (TerminateException) // The debugger is terminating the execution
                            catch (MethodException)
                                // For normal property accesses, we do not generate an error
                                // here. The problem for truly blind dynamic accesses (the
                                // member did not show up in GetDynamicMemberNames) is that
                                // we can't tell the difference between "it failed because
                                // the property does not exist" (let's call this case 1) and
                                // "it failed because accessing it actually threw some
                                // exception" (let's call that case 2).
                                // PowerShell behavior for normal (non-dynamic) properties
                                // is different for these two cases: case 1 gets an error
                                // (which is possible because the ETS tells us up front if
                                // the property exists or not), and case 2 does not. (For
                                // normal properties, this catch block /is/ case 2.)
                                // For IDMOPs, we have the chance to attempt a "blind"
                                // access, but the cost is that we must have the same
                                // response to both cases (because we cannot distinguish
                                // between the two). So we have to make a choice: we can
                                // either swallow ALL errors (including "The property
                                // 'Blarg' does not exist"), or expose them all.
                                // Here, for truly blind dynamic access, we choose to
                                // preserve the behavior of showing "The property 'Blarg'
                                // does not exist" (case 1) errors than to suppress
                                // "FooException thrown when accessing Bloop property" (case
                                // 2) errors.
                                if (isBlindDynamicAccess)
                                    errorRecord = new ErrorRecord(ex,
                                                                  "DynamicPropertyAccessFailed_" + _propertyOrMethodName,
                                    // When the property is not gettable or it throws an exception.
                                    // e.g. when trying to access an assembly's location property, since dynamic assemblies are not backed up by a file,
                                    // an exception will be thrown when accessing its location property. In this case, return null.
                        // PS C:\> "string" | ForEach-Object {$_.aa} | ForEach-Object {$_ + 3}
                        // PS C:\> "string" | ForEach-Object aa | ForEach-Object {$_ + 3}
                        // But if we don't write anything to the pipeline when no member is found,
        private void ProcessScriptBlockParameterSet()
            for (int i = _start; i < _end; i++)
                // Only execute scripts that aren't null. This isn't treated as an error
                // because it allows you to parameterize a command - for example you might allow
                // for actions before and after the main processing script. They could be null
                // by default and therefore ignored then filled in later...
                _scripts[i]?.InvokeUsingCmdlet(
        private void InitScriptBlockParameterSet()
            // Win8: 176403: ScriptCmdlets sets the global WhatIf and Confirm preferences
            // This effects the new W8 foreach-object cmdlet with -whatif and -confirm
            // implemented. -whatif and -confirm needed only for PropertyAndMethodSet
            // parameter set. So erring out in cases where these are used with ScriptBlockSet.
            // Not using MshCommandRuntime, as those variables will be affected by ScriptCmdlet
            // infrastructure (wherein ScriptCmdlet modifies the global preferences).
            Dictionary<string, object> psBoundParameters = this.MyInvocation.BoundParameters;
            if (psBoundParameters != null)
                SwitchParameter whatIf = false;
                SwitchParameter confirm = false;
                object argument;
                if (psBoundParameters.TryGetValue("whatif", out argument))
                    whatIf = (SwitchParameter)argument;
                if (psBoundParameters.TryGetValue("confirm", out argument))
                    confirm = (SwitchParameter)argument;
                if (whatIf || confirm)
                    string message = InternalCommandStrings.NoShouldProcessForScriptBlockSet;
                        "NoShouldProcessForScriptBlockSet",
            // Calculate the start and end indexes for the processRecord script blocks
            _end = _scripts.Count;
            _start = _scripts.Count > 1 ? 1 : 0;
            // and set the end script if it wasn't explicitly set with a named parameter.
            if (!_setEndScript)
                if (_scripts.Count > 2)
                    _end = _scripts.Count - 1;
                    _endScript = _scripts[_end];
            // only process the start script if there is more than one script...
            if (_end < 2)
            if (_scripts[0] == null)
            _scripts[0].InvokeUsingCmdlet(
        /// Do method invocation with arguments.
        private void MethodCallWithArguments()
            // resolve the name
            ReadOnlyPSMemberInfoCollection<PSMemberInfo> methods =
                _inputObject.Members.Match(
                    _propertyOrMethodName,
                    PSMemberTypes.Methods | PSMemberTypes.ParameterizedProperty);
            Dbg.Assert(methods != null, "The return value of Members.Match should never be null.");
            if (methods.Count > 1)
                // write error record: method ambiguous
                foreach (PSMemberInfo item in methods)
                WriteError(GenerateNameParameterError(
                    InternalCommandStrings.AmbiguousMethodName,
                    "AmbiguousMethodName",
                    _inputObject,
                    possibleMatches));
            else if (methods.Count == 0 || methods[0] is not PSMethodInfo)
                // write error record: method no found
                    InternalCommandStrings.MethodNotFound,
                    "MethodNotFound",
                    _propertyOrMethodName));
                PSMethodInfo targetMethod = methods[0] as PSMethodInfo;
                StringBuilder arglist = new StringBuilder(GetStringRepresentation(_arguments[0]));
                for (int i = 1; i < _arguments.Length; i++)
                    arglist.Append(CultureInfo.InvariantCulture, $", {GetStringRepresentation(_arguments[i])}");
                    InternalCommandStrings.ForEachObjectMethodActionWithArguments,
                    targetMethod.Name, arglist);
                            object result = targetMethod.Invoke(_arguments);
        /// Get the string representation of the passed-in object.
        /// <param name="obj">Source object.</param>
        /// <returns>String representation of the source object.</returns>
        private static string GetStringRepresentation(object obj)
            string objInString;
                // The "ToString()" method could throw an exception
                objInString = LanguagePrimitives.IsNull(obj) ? "null" : obj.ToString();
                objInString = null;
            if (string.IsNullOrEmpty(objInString))
                var psobj = obj as PSObject;
                objInString = psobj != null ? psobj.BaseObject.GetType().FullName : obj.GetType().FullName;
            return objInString;
        /// Get the value by taking _propertyOrMethodName as the key, if the
        /// input object is a IDictionary.
        /// <returns>True if success.</returns>
        private bool GetValueFromIDictionaryInput()
            object target = PSObject.Base(_inputObject);
            IDictionary hash = target as IDictionary;
                if (hash != null && hash.Contains(_propertyOrMethodName))
                    string keyAction = string.Format(
                        InternalCommandStrings.ForEachObjectKeyAction,
                    if (ShouldProcess(_targetString, keyAction))
                        object result = hash[_propertyOrMethodName];
                // Ignore invalid operation exception, it can happen if the dictionary
                // has keys that can't be compared to property.
        /// Unroll the object to be output. If it's of type IEnumerator, unroll and output it
        /// by calling WriteOutIEnumerator. If it's not, unroll and output it by calling WriteObject(obj, true)
        private void WriteToPipelineWithUnrolling(object obj)
            IEnumerator objAsEnumerator = LanguagePrimitives.GetEnumerator(obj);
            if (objAsEnumerator != null)
                WriteOutIEnumerator(objAsEnumerator);
        /// Unroll an IEnumerator and output all entries.
        /// <param name="list">Source list.</param>
        private void WriteOutIEnumerator(IEnumerator list)
                while (ParserOps.MoveNext(this.Context, null, list))
                    object val = ParserOps.Current(null, list);
                    if (val != AutomationNull.Value)
                        WriteObject(val);
        /// Check if the language mode is the restrictedLanguageMode before invoking a method.
        /// Write out error message and return true if we are in restrictedLanguageMode.
        /// <param name="inputObject">Source object.</param>
        /// <returns>True if we are in restrictedLanguageMode.</returns>
        private bool BlockMethodInLanguageMode(object inputObject)
            // Cannot invoke a method in RestrictedLanguage mode
            if (Context.LanguageMode == PSLanguageMode.RestrictedLanguage)
                PSInvalidOperationException exception =
                    new PSInvalidOperationException(InternalCommandStrings.NoMethodInvocationInRestrictedLanguageMode);
                WriteError(new ErrorRecord(exception, "NoMethodInvocationInRestrictedLanguageMode", ErrorCategory.InvalidOperation, null));
            // Cannot invoke certain methods in ConstrainedLanguage mode
                object baseObject = PSObject.Base(inputObject);
                var objectType = baseObject.GetType();
                if (!CoreTypes.Contains(objectType))
                            new PSInvalidOperationException(ParserStrings.InvokeMethodConstrainedLanguage);
                        WriteError(new ErrorRecord(exception, "MethodInvocationNotSupportedInConstrainedLanguage", ErrorCategory.InvalidOperation, null));
                        title: InternalCommandStrings.WDACLogTitle,
                        message: StringUtil.Format(InternalCommandStrings.WDACLogMessage, objectType.FullName),
                        fqid: "ForEachObjectCmdletMethodInvocationNotAllowed",
        /// Generate the appropriate error record.
        /// <param name="paraName"></param>
        /// <param name="resourceString"></param>
        /// <param name="target"></param>
        internal static ErrorRecord GenerateNameParameterError(string paraName, string resourceString, string errorId, object target, params object[] args)
            if (args == null || args.Length == 0)
                // Don't format in case the string contains literal curly braces
                message = resourceString;
                message = StringUtil.Format(resourceString, args);
            if (string.IsNullOrEmpty(message))
                Dbg.Assert(false, "Could not load text for error record '" + errorId + "'");
                new PSArgumentException(message, paraName),
    /// Implements a cmdlet that applys a script block
    /// to each element of the pipeline. If the result of that
    /// application is true, then the current pipeline object
    /// is passed on, otherwise it is dropped.
    [Cmdlet("Where", "Object", DefaultParameterSetName = "EqualSet",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096806", RemotingCapability = RemotingCapability.None)]
    public sealed class WhereObjectCommand : PSCmdlet
        private ScriptBlock _script;
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ScriptBlockSet")]
        public ScriptBlock FilterScript
                return _script;
                _script = value;
        /// Gets or sets the property to retrieve value.
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "EqualSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "CaseSensitiveEqualSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "NotEqualSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "CaseSensitiveNotEqualSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "GreaterThanSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "CaseSensitiveGreaterThanSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "LessThanSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "CaseSensitiveLessThanSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "GreaterOrEqualSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "CaseSensitiveGreaterOrEqualSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "LessOrEqualSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "CaseSensitiveLessOrEqualSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "LikeSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "CaseSensitiveLikeSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "NotLikeSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "CaseSensitiveNotLikeSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "MatchSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "CaseSensitiveMatchSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "NotMatchSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "CaseSensitiveNotMatchSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ContainsSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "CaseSensitiveContainsSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "NotContainsSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "CaseSensitiveNotContainsSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "InSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "CaseSensitiveInSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "NotInSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "CaseSensitiveNotInSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "IsSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "IsNotSet")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Not")]
        public string Property
        private object _convertedValue;
        private object _value = true;
        private bool _valueNotSpecified = true;
        [Parameter(Position = 1, ParameterSetName = "EqualSet")]
        [Parameter(Position = 1, ParameterSetName = "CaseSensitiveEqualSet")]
        [Parameter(Position = 1, ParameterSetName = "NotEqualSet")]
        [Parameter(Position = 1, ParameterSetName = "CaseSensitiveNotEqualSet")]
        [Parameter(Position = 1, ParameterSetName = "GreaterThanSet")]
        [Parameter(Position = 1, ParameterSetName = "CaseSensitiveGreaterThanSet")]
        [Parameter(Position = 1, ParameterSetName = "LessThanSet")]
        [Parameter(Position = 1, ParameterSetName = "CaseSensitiveLessThanSet")]
        [Parameter(Position = 1, ParameterSetName = "GreaterOrEqualSet")]
        [Parameter(Position = 1, ParameterSetName = "CaseSensitiveGreaterOrEqualSet")]
        [Parameter(Position = 1, ParameterSetName = "LessOrEqualSet")]
        [Parameter(Position = 1, ParameterSetName = "CaseSensitiveLessOrEqualSet")]
        [Parameter(Position = 1, ParameterSetName = "LikeSet")]
        [Parameter(Position = 1, ParameterSetName = "CaseSensitiveLikeSet")]
        [Parameter(Position = 1, ParameterSetName = "NotLikeSet")]
        [Parameter(Position = 1, ParameterSetName = "CaseSensitiveNotLikeSet")]
        [Parameter(Position = 1, ParameterSetName = "MatchSet")]
        [Parameter(Position = 1, ParameterSetName = "CaseSensitiveMatchSet")]
        [Parameter(Position = 1, ParameterSetName = "NotMatchSet")]
        [Parameter(Position = 1, ParameterSetName = "CaseSensitiveNotMatchSet")]
        [Parameter(Position = 1, ParameterSetName = "ContainsSet")]
        [Parameter(Position = 1, ParameterSetName = "CaseSensitiveContainsSet")]
        [Parameter(Position = 1, ParameterSetName = "NotContainsSet")]
        [Parameter(Position = 1, ParameterSetName = "CaseSensitiveNotContainsSet")]
        [Parameter(Position = 1, ParameterSetName = "InSet")]
        [Parameter(Position = 1, ParameterSetName = "CaseSensitiveInSet")]
        [Parameter(Position = 1, ParameterSetName = "NotInSet")]
        [Parameter(Position = 1, ParameterSetName = "CaseSensitiveNotInSet")]
        [Parameter(Position = 1, ParameterSetName = "IsSet")]
        [Parameter(Position = 1, ParameterSetName = "IsNotSet")]
                return _value;
                _value = value;
                _valueNotSpecified = false;
        #region binary operator parameters
        private TokenKind _binaryOperator = TokenKind.Ieq;
        // set to false if the user specified "-EQ" in the command line.
        // remain to be true if "EqualSet" is chosen by default.
        private bool _forceBooleanEvaluation = true;
        /// Gets or sets binary operator -Equal
        /// It's the default parameter set, so -EQ is not mandatory.
        [Parameter(ParameterSetName = "EqualSet")]
        [Alias("IEQ")]
        public SwitchParameter EQ
                return _binaryOperator == TokenKind.Ieq;
                    _binaryOperator = TokenKind.Ieq;
                    _forceBooleanEvaluation = false;
        /// Gets or sets case sensitive binary operator -ceq.
        [Parameter(Mandatory = true, ParameterSetName = "CaseSensitiveEqualSet")]
        public SwitchParameter CEQ
                return _binaryOperator == TokenKind.Ceq;
                    _binaryOperator = TokenKind.Ceq;
        /// Gets or sets binary operator -NotEqual.
        [Parameter(Mandatory = true, ParameterSetName = "NotEqualSet")]
        [Alias("INE")]
        public SwitchParameter NE
                return _binaryOperator == TokenKind.Ine;
                    _binaryOperator = TokenKind.Ine;
        /// Gets or sets case sensitive binary operator -cne.
        [Parameter(Mandatory = true, ParameterSetName = "CaseSensitiveNotEqualSet")]
        public SwitchParameter CNE
                return _binaryOperator == TokenKind.Cne;
                    _binaryOperator = TokenKind.Cne;
        /// Gets or sets binary operator -GreaterThan.
        [Parameter(Mandatory = true, ParameterSetName = "GreaterThanSet")]
        [Alias("IGT")]
        public SwitchParameter GT
                return _binaryOperator == TokenKind.Igt;
                    _binaryOperator = TokenKind.Igt;
        /// Gets or sets case sensitive binary operator -cgt.
        [Parameter(Mandatory = true, ParameterSetName = "CaseSensitiveGreaterThanSet")]
        public SwitchParameter CGT
                return _binaryOperator == TokenKind.Cgt;
                    _binaryOperator = TokenKind.Cgt;
        /// Gets or sets binary operator -LessThan.
        [Parameter(Mandatory = true, ParameterSetName = "LessThanSet")]
        [Alias("ILT")]
        public SwitchParameter LT
                return _binaryOperator == TokenKind.Ilt;
                    _binaryOperator = TokenKind.Ilt;
        /// Gets or sets case sensitive binary operator -clt.
        [Parameter(Mandatory = true, ParameterSetName = "CaseSensitiveLessThanSet")]
        public SwitchParameter CLT
                return _binaryOperator == TokenKind.Clt;
                    _binaryOperator = TokenKind.Clt;
        /// Gets or sets binary operator -GreaterOrEqual.
        [Parameter(Mandatory = true, ParameterSetName = "GreaterOrEqualSet")]
        [Alias("IGE")]
        public SwitchParameter GE
                return _binaryOperator == TokenKind.Ige;
                    _binaryOperator = TokenKind.Ige;
        /// Gets or sets case sensitive binary operator -cge.
        [Parameter(Mandatory = true, ParameterSetName = "CaseSensitiveGreaterOrEqualSet")]
        public SwitchParameter CGE
                return _binaryOperator == TokenKind.Cge;
                    _binaryOperator = TokenKind.Cge;
        /// Gets or sets binary operator -LessOrEqual.
        [Parameter(Mandatory = true, ParameterSetName = "LessOrEqualSet")]
        [Alias("ILE")]
        public SwitchParameter LE
                return _binaryOperator == TokenKind.Ile;
                    _binaryOperator = TokenKind.Ile;
        /// Gets or sets case sensitive binary operator -cle.
        [Parameter(Mandatory = true, ParameterSetName = "CaseSensitiveLessOrEqualSet")]
        public SwitchParameter CLE
                return _binaryOperator == TokenKind.Cle;
                    _binaryOperator = TokenKind.Cle;
        ///Gets or sets binary operator -Like.
        [Parameter(Mandatory = true, ParameterSetName = "LikeSet")]
        [Alias("ILike")]
        public SwitchParameter Like
                return _binaryOperator == TokenKind.Ilike;
                    _binaryOperator = TokenKind.Ilike;
        /// Gets or sets case sensitive binary operator -clike.
        [Parameter(Mandatory = true, ParameterSetName = "CaseSensitiveLikeSet")]
        public SwitchParameter CLike
                return _binaryOperator == TokenKind.Clike;
                    _binaryOperator = TokenKind.Clike;
        /// Gets or sets binary operator -NotLike.
        [Parameter(Mandatory = true, ParameterSetName = "NotLikeSet")]
        [Alias("INotLike")]
        public SwitchParameter NotLike
                    _binaryOperator = TokenKind.Inotlike;
        /// Gets or sets case sensitive binary operator -cnotlike.
        [Parameter(Mandatory = true, ParameterSetName = "CaseSensitiveNotLikeSet")]
        public SwitchParameter CNotLike
                return _binaryOperator == TokenKind.Cnotlike;
                    _binaryOperator = TokenKind.Cnotlike;
        /// Get or sets binary operator -Match.
        [Parameter(Mandatory = true, ParameterSetName = "MatchSet")]
        [Alias("IMatch")]
        public SwitchParameter Match
                return _binaryOperator == TokenKind.Imatch;
                    _binaryOperator = TokenKind.Imatch;
        /// Gets or sets case sensitive binary operator -cmatch.
        [Parameter(Mandatory = true, ParameterSetName = "CaseSensitiveMatchSet")]
        public SwitchParameter CMatch
                return _binaryOperator == TokenKind.Cmatch;
                    _binaryOperator = TokenKind.Cmatch;
        /// Gets or sets binary operator -NotMatch.
        [Parameter(Mandatory = true, ParameterSetName = "NotMatchSet")]
        [Alias("INotMatch")]
        public SwitchParameter NotMatch
                return _binaryOperator == TokenKind.Inotmatch;
                    _binaryOperator = TokenKind.Inotmatch;
        /// Gets or sets case sensitive binary operator -cnotmatch.
        [Parameter(Mandatory = true, ParameterSetName = "CaseSensitiveNotMatchSet")]
        public SwitchParameter CNotMatch
                return _binaryOperator == TokenKind.Cnotmatch;
                    _binaryOperator = TokenKind.Cnotmatch;
        /// Gets or sets binary operator -Contains.
        [Parameter(Mandatory = true, ParameterSetName = "ContainsSet")]
        [Alias("IContains")]
        public SwitchParameter Contains
                return _binaryOperator == TokenKind.Icontains;
                    _binaryOperator = TokenKind.Icontains;
        /// Gets or sets case sensitive binary operator -ccontains.
        [Parameter(Mandatory = true, ParameterSetName = "CaseSensitiveContainsSet")]
        public SwitchParameter CContains
                return _binaryOperator == TokenKind.Ccontains;
                    _binaryOperator = TokenKind.Ccontains;
        /// Gets or sets binary operator -NotContains.
        [Parameter(Mandatory = true, ParameterSetName = "NotContainsSet")]
        [Alias("INotContains")]
        public SwitchParameter NotContains
                return _binaryOperator == TokenKind.Inotcontains;
                    _binaryOperator = TokenKind.Inotcontains;
        /// Gets or sets case sensitive binary operator -cnotcontains.
        [Parameter(Mandatory = true, ParameterSetName = "CaseSensitiveNotContainsSet")]
        public SwitchParameter CNotContains
                return _binaryOperator == TokenKind.Cnotcontains;
                    _binaryOperator = TokenKind.Cnotcontains;
        /// Gets or sets binary operator -In.
        [Parameter(Mandatory = true, ParameterSetName = "InSet")]
        [Alias("IIn")]
        public SwitchParameter In
                return _binaryOperator == TokenKind.In;
                    _binaryOperator = TokenKind.In;
        /// Gets or sets case sensitive binary operator -cin.
        [Parameter(Mandatory = true, ParameterSetName = "CaseSensitiveInSet")]
        public SwitchParameter CIn
                return _binaryOperator == TokenKind.Cin;
                    _binaryOperator = TokenKind.Cin;
        /// Gets or sets binary operator -NotIn.
        [Parameter(Mandatory = true, ParameterSetName = "NotInSet")]
        [Alias("INotIn")]
        public SwitchParameter NotIn
                return _binaryOperator == TokenKind.Inotin;
                    _binaryOperator = TokenKind.Inotin;
        /// Gets or sets case sensitive binary operator -cnotin.
        [Parameter(Mandatory = true, ParameterSetName = "CaseSensitiveNotInSet")]
        public SwitchParameter CNotIn
                return _binaryOperator == TokenKind.Cnotin;
                    _binaryOperator = TokenKind.Cnotin;
        /// Gets or sets binary operator -Is.
        [Parameter(Mandatory = true, ParameterSetName = "IsSet")]
        public SwitchParameter Is
                return _binaryOperator == TokenKind.Is;
                    _binaryOperator = TokenKind.Is;
        /// Gets or sets binary operator -IsNot.
        [Parameter(Mandatory = true, ParameterSetName = "IsNotSet")]
        public SwitchParameter IsNot
                return _binaryOperator == TokenKind.IsNot;
                    _binaryOperator = TokenKind.IsNot;
        /// Gets or sets binary operator -Not.
        [Parameter(Mandatory = true, ParameterSetName = "Not")]
        public SwitchParameter Not
                return _binaryOperator == TokenKind.Not;
                    _binaryOperator = TokenKind.Not;
        #endregion binary operator parameters
        private readonly CallSite<Func<CallSite, object, bool>> _toBoolSite =
            CallSite<Func<CallSite, object, bool>>.Create(PSConvertBinder.Get(typeof(bool)));
        private Func<object, object, object> _operationDelegate;
        private static Func<object, object, object> GetCallSiteDelegate(ExpressionType expressionType, bool ignoreCase)
            var site = CallSite<Func<CallSite, object, object, object>>.Create(PSBinaryOperationBinder.Get(expressionType, ignoreCase));
            return (x, y) => site.Target.Invoke(site, x, y);
        private static Func<object, object, object> GetCallSiteDelegateBoolean(ExpressionType expressionType, bool ignoreCase)
            // flip 'lval' and 'rval' in the scenario '... | Where-Object property' so as to make it
            // equivalent to '... | Where-Object {$true -eq property}'. Because we want the property to
            // be compared under the bool context. So that '"string" | Where-Object Length' would behave
            // just like '"string" | Where-Object {$_.Length}'.
            var site = CallSite<Func<CallSite, object, object, object>>.Create(binder: PSBinaryOperationBinder.Get(expressionType, ignoreCase));
            return (x, y) => site.Target.Invoke(site, y, x);
        private static Tuple<CallSite<Func<CallSite, object, IEnumerator>>, CallSite<Func<CallSite, object, object, object>>> GetContainsCallSites(bool ignoreCase)
            var enumerableSite = CallSite<Func<CallSite, object, IEnumerator>>.Create(PSEnumerableBinder.Get());
            var equalSite =
                CallSite<Func<CallSite, object, object, object>>.Create(PSBinaryOperationBinder.Get(
                    ExpressionType.Equal, ignoreCase, scalarCompare: true));
            return Tuple.Create(enumerableSite, equalSite);
        private void CheckLanguageMode()
            if (Context.LanguageMode.Equals(PSLanguageMode.RestrictedLanguage))
                    InternalCommandStrings.OperationNotAllowedInRestrictedLanguageMode,
                    _binaryOperator);
                    new PSInvalidOperationException(message);
                ThrowTerminatingError(new ErrorRecord(exception, "OperationNotAllowedInRestrictedLanguageMode", ErrorCategory.InvalidOperation, null));
        private object GetLikeRHSOperand(object operand)
            if (operand is not string val)
                return operand;
            var wildcardOptions = _binaryOperator == TokenKind.Ilike || _binaryOperator == TokenKind.Inotlike
                ? WildcardOptions.IgnoreCase
                : WildcardOptions.None;
            return WildcardPattern.Get(val, wildcardOptions);
            if (_script != null)
            switch (_binaryOperator)
                case TokenKind.Ieq:
                    if (!_forceBooleanEvaluation)
                        _operationDelegate = GetCallSiteDelegate(ExpressionType.Equal, ignoreCase: true);
                        _operationDelegate = GetCallSiteDelegateBoolean(ExpressionType.Equal, ignoreCase: true);
                case TokenKind.Ceq:
                    _operationDelegate = GetCallSiteDelegate(ExpressionType.Equal, ignoreCase: false);
                case TokenKind.Ine:
                    _operationDelegate = GetCallSiteDelegate(ExpressionType.NotEqual, ignoreCase: true);
                case TokenKind.Cne:
                    _operationDelegate = GetCallSiteDelegate(ExpressionType.NotEqual, ignoreCase: false);
                case TokenKind.Igt:
                    _operationDelegate = GetCallSiteDelegate(ExpressionType.GreaterThan, ignoreCase: true);
                case TokenKind.Cgt:
                    _operationDelegate = GetCallSiteDelegate(ExpressionType.GreaterThan, ignoreCase: false);
                case TokenKind.Ilt:
                    _operationDelegate = GetCallSiteDelegate(ExpressionType.LessThan, ignoreCase: true);
                case TokenKind.Clt:
                    _operationDelegate = GetCallSiteDelegate(ExpressionType.LessThan, ignoreCase: false);
                case TokenKind.Ige:
                    _operationDelegate = GetCallSiteDelegate(ExpressionType.GreaterThanOrEqual, ignoreCase: true);
                case TokenKind.Cge:
                    _operationDelegate = GetCallSiteDelegate(ExpressionType.GreaterThanOrEqual, ignoreCase: false);
                case TokenKind.Ile:
                    _operationDelegate = GetCallSiteDelegate(ExpressionType.LessThanOrEqual, ignoreCase: true);
                case TokenKind.Cle:
                    _operationDelegate = GetCallSiteDelegate(ExpressionType.LessThanOrEqual, ignoreCase: false);
                case TokenKind.Ilike:
                    _operationDelegate =
                        (lval, rval) => ParserOps.LikeOperator(Context, PositionUtilities.EmptyExtent, lval, rval, _binaryOperator);
                case TokenKind.Clike:
                case TokenKind.Inotlike:
                case TokenKind.Cnotlike:
                case TokenKind.Imatch:
                    CheckLanguageMode();
                        (lval, rval) => ParserOps.MatchOperator(Context, PositionUtilities.EmptyExtent, lval, rval, notMatch: false, ignoreCase: true);
                case TokenKind.Cmatch:
                        (lval, rval) => ParserOps.MatchOperator(Context, PositionUtilities.EmptyExtent, lval, rval, notMatch: false, ignoreCase: false);
                case TokenKind.Inotmatch:
                        (lval, rval) => ParserOps.MatchOperator(Context, PositionUtilities.EmptyExtent, lval, rval, notMatch: true, ignoreCase: true);
                case TokenKind.Cnotmatch:
                        (lval, rval) => ParserOps.MatchOperator(Context, PositionUtilities.EmptyExtent, lval, rval, notMatch: true, ignoreCase: false);
                    _operationDelegate = GetCallSiteDelegateBoolean(ExpressionType.NotEqual, ignoreCase: true);
                // the second to last parameter in ContainsOperator has flipped semantics compared to others.
                // "true" means "contains" while "false" means "notcontains"
                case TokenKind.Icontains:
                case TokenKind.Inotcontains:
                case TokenKind.In:
                case TokenKind.Inotin:
                        var sites = GetContainsCallSites(ignoreCase: true);
                                    (lval, rval) => ParserOps.ContainsOperatorCompiled(Context, sites.Item1, sites.Item2, lval, rval);
                                    (lval, rval) => !ParserOps.ContainsOperatorCompiled(Context, sites.Item1, sites.Item2, lval, rval);
                                    (lval, rval) => ParserOps.ContainsOperatorCompiled(Context, sites.Item1, sites.Item2, rval, lval);
                                    (lval, rval) => !ParserOps.ContainsOperatorCompiled(Context, sites.Item1, sites.Item2, rval, lval);
                case TokenKind.Ccontains:
                case TokenKind.Cnotcontains:
                case TokenKind.Cin:
                case TokenKind.Cnotin:
                        var sites = GetContainsCallSites(ignoreCase: false);
                case TokenKind.Is:
                    _operationDelegate = (lval, rval) => ParserOps.IsOperator(Context, PositionUtilities.EmptyExtent, lval, rval);
                case TokenKind.IsNot:
                    _operationDelegate = (lval, rval) => ParserOps.IsNotOperator(Context, PositionUtilities.EmptyExtent, lval, rval);
            _convertedValue = _value;
            if (!_valueNotSpecified)
                        _convertedValue = GetLikeRHSOperand(_convertedValue);
                        // users might input [int], [string] as they do when using scripts
                        var strValue = _convertedValue as string;
                        if (strValue != null)
                            var typeLength = strValue.Length;
                            if (typeLength > 2 && strValue[0] == '[' && strValue[typeLength - 1] == ']')
                                _convertedValue = strValue.Substring(1, typeLength - 2);
                            _convertedValue = LanguagePrimitives.ConvertTo<Type>(_convertedValue);
        /// Execute the script block passing in the current pipeline object as
        /// it's only parameter.
            if (_inputObject == AutomationNull.Value)
                object result = _script.DoInvokeReturnAsIs(
                    input: new object[] { _inputObject },
                if (_toBoolSite.Target.Invoke(_toBoolSite, result))
                // Both -Property and -Value need to be specified if the user specifies the binary operation
                if (_valueNotSpecified && ((_binaryOperator != TokenKind.Ieq && _binaryOperator != TokenKind.Not) || !_forceBooleanEvaluation))
                    // The binary operation is specified explicitly by the user and the -Value parameter is
                        ForEachObjectCommand.GenerateNameParameterError(
                            "Value",
                            InternalCommandStrings.ValueNotSpecifiedForWhereObject,
                            "ValueNotSpecifiedForWhereObject",
                            target: null));
                // The binary operation needs to be specified if the user specifies both the -Property and -Value
                if (!_valueNotSpecified && (_binaryOperator == TokenKind.Ieq && _forceBooleanEvaluation))
                    // The -Property and -Value are specified explicitly by the user but the binary operation is not
                            "Operator",
                            InternalCommandStrings.OperatorNotSpecified,
                            "OperatorNotSpecified",
                bool strictModeWithError = false;
                object lvalue = GetValue(ref strictModeWithError);
                if (strictModeWithError)
                    object result = _operationDelegate.Invoke(lvalue, _convertedValue);
                catch (ArgumentException ae)
                        PSTraceSource.NewArgumentException("BinaryOperator", ParserStrings.BadOperatorArgument, _binaryOperator, ae.Message),
                        "BadOperatorArgument",
                        _inputObject);
                        PSTraceSource.NewInvalidOperationException(ParserStrings.OperatorFailed, _binaryOperator, ex.Message),
                        "OperatorFailed",
        /// Get the value based on the given property name.
        private object GetValue(ref bool error)
                            "InputObject",
                            InternalCommandStrings.InputObjectIsNull,
                            "InputObjectIsNull",
                            _property));
                    error = true;
            // If the target is a hash table and it contains the requested key
            // return that, otherwise fall through and see if there is an
            // underlying member corresponding to the key...
                if (hash != null && hash.Contains(_property))
                    return hash[_property];
            ReadOnlyPSMemberInfoCollection<PSMemberInfo> members = GetMatchMembers();
                        InternalCommandStrings.AmbiguousPropertyOrMethodName,
                        "AmbiguousPropertyName",
                        _property,
            else if (members.Count == 0)
                if ((InputObject.BaseObject is IDynamicMetaObjectProvider) &&
                    !WildcardPattern.ContainsWildcardCharacters(_property))
                    // Let's just try a dynamic property access. Note that if it comes to
                    // depending on dynamic access, we are assuming it is a property; we
                    // don't have ETS info to tell us up front if it even exists or not,
                    // let alone if it is a method or something else.
                    // GetDynamicMemberNames(), else it would show up as a dynamic member.
                    resolvedPropertyName = _property;
                else if (Context.IsStrictVersion(2))
                    WriteError(ForEachObjectCommand.GenerateNameParameterError(
                        InternalCommandStrings.PropertyNotFound,
                        "PropertyNotFound",
                resolvedPropertyName = members[0].Name;
                    return _propGetter.GetValue(_inputObject, resolvedPropertyName);
                catch (TerminateException)
                    // For normal property accesses, we do not generate an error here. The problem
                    // for truly blind dynamic accesses (the member did not show up in
                    // GetDynamicMemberNames) is that we can't tell the difference between "it
                    // failed because the property does not exist" (let's call this case
                    // 1) and "it failed because accessing it actually threw some exception" (let's
                    // call that case 2).
                    // PowerShell behavior for normal (non-dynamic) properties is different for
                    // these two cases: case 1 gets an error (if strict mode is on) (which is
                    // possible because the ETS tells us up front if the property exists or not),
                    // and case 2 does not. (For normal properties, this catch block /is/ case 2.)
                    // For IDMOPs, we have the chance to attempt a "blind" access, but the cost is
                    // that we must have the same response to both cases (because we cannot
                    // distinguish between the two). So we have to make a choice: we can either
                    // swallow ALL errors (including "The property 'Blarg' does not exist"), or
                    // expose them all.
                    // Here, for truly blind dynamic access, we choose to preserve the behavior of
                    // showing "The property 'Blarg' does not exist" (case 1) errors than to
                    // suppress "FooException thrown when accessing Bloop property" (case
                    if (isBlindDynamicAccess && Context.IsStrictVersion(2))
                        WriteError(new ErrorRecord(ex,
                                                   "DynamicPropertyAccessFailed_" + _property,
                                                   _inputObject));
                        // When the property is not gettable or it throws an exception
        /// Get the matched PSMembers.
        /// <returns>Matched PSMembers.</returns>
        private ReadOnlyPSMemberInfoCollection<PSMemberInfo> GetMatchMembers()
            if (!WildcardPattern.ContainsWildcardCharacters(_property))
                PSMemberInfoInternalCollection<PSMemberInfo> results = new PSMemberInfoInternalCollection<PSMemberInfo>();
                PSMemberInfo member = _inputObject.Members[_property];
                    results.Add(member);
                return new ReadOnlyPSMemberInfoCollection<PSMemberInfo>(results);
            ReadOnlyPSMemberInfoCollection<PSMemberInfo> members = _inputObject.Members.Match(_property, PSMemberTypes.All);
            Dbg.Assert(members != null, "The return value of Members.Match should never be null.");
    /// Implements a cmdlet that sets the script debugging options.
    [Cmdlet(VerbsCommon.Set, "PSDebug", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096959")]
    public sealed class SetPSDebugCommand : PSCmdlet
        /// Gets or sets the script tracing level.
        [Parameter(ParameterSetName = "on")]
        [ValidateRange(0, 2)]
        public int Trace
                return _trace;
                _trace = value;
        private int _trace = -1;
        /// Gets or sets stepping on and off.
        public SwitchParameter Step
                return (SwitchParameter)_step;
                _step = value;
        private bool? _step;
        /// Gets or sets strict mode on and off.
        public SwitchParameter Strict
                return (SwitchParameter)_strict;
                _strict = value;
        private bool? _strict;
        /// Gets or sets all script debugging features off.
        [Parameter(ParameterSetName = "off")]
        public SwitchParameter Off
                return _off;
                _off = value;
        private bool _off;
            // -off gets processed after the others so it takes precedence...
            if (_off)
                Context.Debugger.DisableTracing();
                Context.EngineSessionState.GlobalScope.StrictModeVersion = null;
                if (_trace >= 0 || _step != null)
                    Context.Debugger.EnableTracing(_trace, _step);
                // Version 0 is the same as off
                if (_strict != null)
                    Context.EngineSessionState.GlobalScope.StrictModeVersion = new Version((bool)_strict ? 1 : 0, 0);
    #region Set-StrictMode
    /// Set-StrictMode causes the interpreter to throw an exception in the following cases:
    /// * Referencing an unassigned variable
    /// * Referencing a non-existent property of an object
    /// * Calling a function as a method (with parentheses and commas)
    /// * Using the variable expansion syntax in a string literal w/o naming a variable, i.e. "${}"
    /// Parameters:
    /// -Version allows the script author to specify which strict mode version to enforce.
    /// -Off turns strict mode off
    /// Note:
    /// Unlike Set-PSDebug -strict, Set-StrictMode is not engine-wide, and only
    /// affects the scope it was defined in.
    [Cmdlet(VerbsCommon.Set, "StrictMode", DefaultParameterSetName = "Version", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096804")]
    public class SetStrictModeCommand : PSCmdlet
        /// Gets or sets strict mode off.
        [Parameter(ParameterSetName = "Off", Mandatory = true)]
        private SwitchParameter _off;
        /// Handle 'latest', which we interpret to be the current version of PowerShell.
        private sealed class ArgumentToPSVersionTransformationAttribute : ArgumentToVersionTransformationAttribute
            protected override bool TryConvertFromString(string versionString, [NotNullWhen(true)] out Version version)
                if (string.Equals("latest", versionString, StringComparison.OrdinalIgnoreCase))
                    version = PSVersionInfo.PSVersion;
                return base.TryConvertFromString(versionString, out version);
        private sealed class ValidateVersionAttribute : ValidateArgumentsAttribute
                Version version = arguments as Version;
                if (!PSVersionInfo.IsValidPSVersion(version))
                    // No conversion succeeded so throw and exception...
                        "InvalidPSVersion",
                        Metadata.ValidateVersionFailure,
        /// Gets or sets strict mode in the current scope.
        [Parameter(ParameterSetName = "Version", Mandatory = true)]
        [ArgumentCompleter(typeof(StrictModeVersionArgumentCompleter))]
        [ArgumentToPSVersionTransformation]
        [ValidateVersion]
        [Alias("v")]
        /// Set the correct version for strict mode checking in the current scope.
            if (_off.IsPresent)
                _version = new Version(0, 0);
            Context.EngineSessionState.CurrentScope.StrictModeVersion = _version;
    /// Provides argument completion for StrictMode Version parameter.
    public class StrictModeVersionArgumentCompleter : IArgumentCompleter
        private static readonly string[] s_strictModeVersions = new string[] { "Latest", "3.0", "2.0", "1.0" };
        /// Returns completion results for version parameter.
                    possibleCompletionValues: s_strictModeVersions);
    #endregion Set-StrictMode
    #endregion Built-in cmdlets that are used by or require direct access to the engine.

    /// An object representing a pre-compiled block of powershell script.
    /// This class track a block of script in a compiled form. It is also
    /// used for direct invocation of the script block.
    /// 1. Overview
    /// Script block comes in two forms,
    /// a. Full form (cmdlet form)
    /// This comes in following format
    ///     begin
    ///         statementlist;
    ///     process
    ///     end
    /// This form is used for running the script in a pipeline like
    /// a cmdlet.
    /// b. Simple form
    ///     statementlist;
    /// 2. Script block execution
    /// For the full form (or cmdlet form) of script block, the script
    /// block itself is part of a pipeline. Its execution is handled through
    /// ScriptCommandProcessor, which involves execution of begin/process/end
    /// blocks like a cmdlet. If a scriptblock in simple form is used in
    /// a pipeline, its execution is done through ScriptCommandProcessor
    /// also, with some of begin/process/end blocks default to be empty.
    /// A script block in simple form can be directly invoked (outside
    /// of a pipeline context). For example,
    ///     {"text"}.Invoke()
    /// A scriptblock can be directly invoked internally or externally through
    /// runspace API.
    /// This class will handle the logic for direct invocation of script blocks.
    public partial class ScriptBlock
        /// Create a script block object based on a script string to be parsed immediately.
        /// <param name="context">Engine context for this script block.</param>
        /// <param name="script">The string to compile.</param>
        internal static ScriptBlock Create(ExecutionContext context, string script)
            ScriptBlock sb = Create(context.Engine.EngineParser, null, script);
            if (context.EngineSessionState != null && context.EngineSessionState.Module != null)
                sb.SessionStateInternal = context.EngineSessionState;
        /// Create a script block based on a script to be parsed when execution
        /// context is provided.
        public static ScriptBlock Create(string script) => Create(
            parser: new Parser(),
            fileContents: script);
        internal static ScriptBlock CreateDelayParsedScriptBlock(string script, bool isProductCode)
            => new ScriptBlock(new CompiledScriptBlockData(script, isProductCode)) { DebuggerHidden = true };
        /// Returns a new scriptblock bound to a module. Any local variables in the
        /// callers context will be copied into the module.
        public ScriptBlock GetNewClosure()
            PSModuleInfo m = new PSModuleInfo(true);
            m.CaptureLocals();
            return m.NewBoundScriptBlock(this);
        /// Returns PowerShell object representing the pipeline contained in this ScriptBlock.
        /// Some ScriptBlocks are too complicated to be converted into a PowerShell object.
        /// For those ScriptBlocks a <see cref="ScriptBlockToPowerShellNotSupportedException"/> is thrown.
        /// ScriptBlock cannot be converted into a PowerShell object if
        /// - It contains more than one statement
        /// - It references variables undeclared in <c>param(...)</c> block
        /// - It uses redirection to a file
        /// - It uses dot sourcing
        /// - Command names can't be resolved (i.e. if an element of a pipeline is another scriptblock)
        /// Declaration of variables in a <c>param(...)</c> block is enforced,
        /// because undeclared variables are assumed to be variables from a remoting server.
        /// Since we need to fully evaluate parameters of commands of a PowerShell object's
        /// we reject all variables references that refer to a variable from a remoting server.
        /// arguments for the ScriptBlock (providing values for variables used within the ScriptBlock);
        /// can be null
        /// PowerShell object representing the pipeline contained in this ScriptBlock
        /// <exception cref="ScriptBlockToPowerShellNotSupportedException">
        /// Thrown when this ScriptBlock cannot be expressed as a PowerShell object.
        /// For example thrown when there is more than one statement, if there
        /// are undeclared variables, if redirection to a file is used.
        /// Thrown when evaluation of command arguments results in an exception.
        /// Might depend on the value of $errorActionPreference variable.
        /// For example trying to translate the following ScriptBlock will result in this exception:
        /// <c>$errorActionPreference = "stop"; $sb = { get-foo $( throw ) }; $sb.GetPowerShell()</c>
        /// Thrown when there is no ExecutionContext associated with this ScriptBlock object.
        public PowerShell GetPowerShell(params object[] args) => GetPowerShellImpl(
            context: LocalPipeline.GetExecutionContextFromTLS(),
            variables: null,
            isTrustedInput: false,
            filterNonUsingVariables: false,
            createLocalScope: null,
        /// Returns PowerShell object representing the pipeline contained in this ScriptBlock,
        /// similar to the GetPowerShell() method. If the 'isTrustedInput' flag parameter is set
        /// to True, then the GetPowerShell() implementation supports extended conversion operations
        /// (such as replacing variable values with their current values) that might otherwise
        /// be unsafe if applied to untrusted input.
        /// <param name="isTrustedInput">
        /// Specifies whether the scriptblock being converted comes from a trusted source.
        /// The default is False.
        public PowerShell GetPowerShell(bool isTrustedInput, params object[] args)
            => GetPowerShellImpl(
                isTrustedInput,
        /// Returns PowerShell object representing the pipeline contained in this ScriptBlock, using variables
        /// supplied in the dictionary.
        /// <param name="variables">
        /// variables to be supplied as context to the ScriptBlock (providing values for variables explicitly
        /// requested by the 'using:' prefix.
        /// Thrown when there is no ExecutionContext associated with this ScriptBlock object and no
        /// variables are supplied.
        public PowerShell GetPowerShell(Dictionary<string, object> variables, params object[] args)
            Dictionary<string, object> suppliedVariables = null;
            if (variables != null)
                suppliedVariables = new Dictionary<string, object>(variables, StringComparer.OrdinalIgnoreCase);
            return GetPowerShellImpl(context, suppliedVariables, false, false, null, args);
        /// <param name="usingVariables">
        /// key-value pairs from the <para>variables</para> that actually get used by the 'using:' prefix variables
        public PowerShell GetPowerShell(
            Dictionary<string, object> variables,
            out Dictionary<string, object> usingVariables,
            => GetPowerShell(variables, out usingVariables, isTrustedInput: false, args);
            bool isTrustedInput,
            PowerShell powershell = GetPowerShellImpl(context, suppliedVariables, isTrustedInput, true, null, args);
            usingVariables = suppliedVariables;
            return powershell;
        internal PowerShell GetPowerShell(
            bool? useLocalScope,
                useLocalScope,
            "Microsoft.Naming",
            "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Steppable",
            Justification = "Review this during API naming")]
            => GetSteppablePipelineImpl(commandOrigin: CommandOrigin.Internal, args: null);
        public SteppablePipeline GetSteppablePipeline(CommandOrigin commandOrigin)
            => GetSteppablePipelineImpl(commandOrigin, args: null);
        public SteppablePipeline GetSteppablePipeline(CommandOrigin commandOrigin, object[] args)
            => GetSteppablePipelineImpl(commandOrigin, args);
        /// Execute this node with the specified arguments. The arguments show
        /// up in the script as $args with $_ being the first argument.
        /// <param name="args">The arguments to this script.</param>
        /// <returns>The object(s) generated during the execution of
        /// the script block returned as a collection of PSObjects.</returns>
        /// <exception cref="RuntimeException">Thrown if a script runtime exceptionexception occurred.</exception>
        /// <exception cref="FlowControlException">An internal (non-public) exception from a flow control statement.</exception>
        public Collection<PSObject> Invoke(params object[] args) =>
            DoInvoke(dollarUnder: AutomationNull.Value, input: AutomationNull.Value, args);
        /// A method that allows a scriptblock to be invoked with additional context in the form of a
        /// set of local functions and variables to be defined in the scriptblock's scope. The list of
        /// variables may include the special variables $input, $_ and $this.
        /// This overload of the function takes a hashtable and converts it to the
        /// required dictionary which makes the API easier to use from within a PowerShell script.
        /// <param name="functionsToDefine">A dictionary of functions to define.</param>
        /// <param name="variablesToDefine">A list of variables to define.</param>
        /// <param name="args">The arguments to the actual scriptblock.</param>
        public Collection<PSObject> InvokeWithContext(
            IDictionary functionsToDefine,
            List<PSVariable> variablesToDefine,
            Dictionary<string, ScriptBlock> functionsToDefineDictionary = null;
                functionsToDefineDictionary = new Dictionary<string, ScriptBlock>();
                foreach (DictionaryEntry pair in functionsToDefine)
                    string functionName = pair.Key as string;
                    if (string.IsNullOrWhiteSpace(functionName))
                            ParserStrings.EmptyFunctionNameInFunctionDefinitionDictionary);
                        e.SetErrorId("EmptyFunctionNameInFunctionDefinitionDictionary");
                    ScriptBlock functionBody = pair.Value as ScriptBlock;
                    // null check for functionBody is done at the lower layer.
                    functionsToDefineDictionary.Add(functionName, functionBody);
            return InvokeWithContext(
                functionsToDefineDictionary,
                variablesToDefine,
            object input = AutomationNull.Value;
            object dollarUnder = AutomationNull.Value;
            object scriptThis = AutomationNull.Value;
            if (variablesToDefine != null)
                // Extract the special variables "this", "input" and "_"
                PSVariable located = variablesToDefine.Find(
                    v => string.Equals(v.Name, "this", StringComparison.OrdinalIgnoreCase));
                if (located != null)
                    scriptThis = located.Value;
                    variablesToDefine.Remove(located);
                located = variablesToDefine.Find(
                    v => string.Equals(v.Name, "_", StringComparison.Ordinal));
                    dollarUnder = located.Value;
                    v => string.Equals(v.Name, "input", StringComparison.OrdinalIgnoreCase));
                    input = located.Value;
            Pipe outputPipe = new Pipe(result);
            InvokeWithPipe(
                functionsToDefine: functionsToDefine,
                variablesToDefine: variablesToDefine,
                errorHandlingBehavior: ErrorHandlingBehavior.WriteToCurrentErrorPipe,
                dollarUnder: dollarUnder,
            return GetWrappedResult(result);
        /// up in the script as $args. This overload return the raw (unwrapped) result
        /// so it can be more efficient.
        /// <param name="args">The arguments to pass to this scriptblock.</param>
        /// <returns>The object(s) generated during the execution of the
        /// script block. They may or may not be wrapped in PSObject. It's up to the caller to check.</returns>
        public object InvokeReturnAsIs(params object[] args)
            => DoInvokeReturnAsIs(
                errorHandlingBehavior: ErrorHandlingBehavior.WriteToExternalErrorPipe,
        internal T InvokeAsMemberFunctionT<T>(object instance, object[] args)
            Pipe pipe = new Pipe(result);
                scriptThis: instance ?? AutomationNull.Value,
                outputPipe: pipe,
                propagateAllExceptionsToTop: true,
            // This is needed only for the case where the
            // method returns [object]. If the argument to 'return'
            // is a pipeline that emits nothing then result.Count will
            // be zero so we catch that and "convert" it to null. Note that
            // the return statement is still required in the method, it
            // just receives nothing from it's argument.
            return (T)result[0];
        internal void InvokeAsMemberFunction(object instance, object[] args)
            Diagnostics.Assert(result.Count == 0, "Code generation ensures we return the correct type");
        /// Return all attributes on a script block.
        public List<Attribute> Attributes { get => GetAttributes(); }
        /// The script file that defined this script block.
        public string File { get => GetFileName(); }
        /// Get/set whether this scriptblock is a filter.
        public bool IsFilter { get => _scriptBlockData.IsFilter; }
        /// Get/set whether this scriptblock is a Configuration.
        public bool IsConfiguration { get => _scriptBlockData.GetIsConfiguration(); }
        /// Get the PSModuleInfo object for the module that defined this
        /// scriptblock.
        public PSModuleInfo Module { get => SessionStateInternal?.Module; }
        /// Return the PSToken object for this function definition...
        public PSToken StartPosition { get => GetStartPosition(); }
        // LanguageMode is a nullable PSLanguageMode enumeration because script blocks
        // need to inherit the language mode from the context in which they are executing.
        // We can't assume FullLanguage by default when there is no context, as there are
        // script blocks (such as the script blocks used in Workflow activities) that are
        // created by the host without a "current language mode" to inherit. They ultimately
        // get their language mode set when they are finally invoked in a constrained
        // language runspace.
        // Script blocks that should always be run under FullLanguage mode (i.e.: set in
        // InitialSessionState, etc.) should explicitly set the LanguageMode to FullLanguage
        // when they are created.
        internal PSLanguageMode? LanguageMode { get; set; }
        internal enum ErrorHandlingBehavior
            WriteToCurrentErrorPipe = 1,
            WriteToExternalErrorPipe = 2,
            SwallowErrors = 3,
        internal ReadOnlyCollection<PSTypeName> OutputType
                List<PSTypeName> result = new List<PSTypeName>();
                foreach (Attribute attribute in Attributes)
                    OutputTypeAttribute outputType = attribute as OutputTypeAttribute;
                    if (outputType != null)
                        result.AddRange(outputType.Type);
                return new ReadOnlyCollection<PSTypeName>(result);
        /// This is a helper function to process script invocation result.
        /// This does normal array reduction in the case of a one-element array.
        internal static object GetRawResult(List<object> result, bool wrapToPSObject)
            switch (result.Count)
                    return wrapToPSObject ? LanguagePrimitives.AsPSObjectOrNull(result[0]) : result[0];
                    object resultArray = result.ToArray();
                    return wrapToPSObject ? LanguagePrimitives.AsPSObjectOrNull(resultArray) : resultArray;
        internal void InvokeUsingCmdlet(
            Cmdlet contextCmdlet,
            ErrorHandlingBehavior errorHandlingBehavior,
            object dollarUnder,
            object input,
            object scriptThis,
            Diagnostics.Assert(contextCmdlet != null, "caller to verify contextCmdlet parameter");
            Pipe outputPipe = ((MshCommandRuntime)contextCmdlet.CommandRuntime).OutputPipe;
            var myInv = context.EngineSessionState.CurrentScope.GetAutomaticVariableValue(AutomaticVariable.MyInvocation);
            InvocationInfo inInfo = myInv == AutomationNull.Value ? null : (InvocationInfo)myInv;
                errorHandlingBehavior,
                dollarUnder,
                scriptThis,
                outputPipe,
                inInfo,
                propagateAllExceptionsToTop: false,
        /// The internal session state object associated with this scriptblock.
        internal SessionStateInternal SessionStateInternal { get; set; }
        /// The session state instance that should be used when evaluating
        /// this scriptblock.
                if (SessionStateInternal == null)
                        SessionStateInternal = context.EngineSessionState.PublicSessionState.Internal;
                return SessionStateInternal?.PublicSessionState;
                SessionStateInternal = value.Internal;
        #region Delegates
        private static readonly ConditionalWeakTable<ScriptBlock, ConcurrentDictionary<Type, Delegate>> s_delegateTable =
            new ConditionalWeakTable<ScriptBlock, ConcurrentDictionary<Type, Delegate>>();
        internal Delegate GetDelegate(Type delegateType)
            => s_delegateTable.GetOrCreateValue(this).GetOrAdd(delegateType, CreateDelegate);
        /// Get the delegate method as a call back.
        internal Delegate CreateDelegate(Type delegateType)
            MethodInfo invokeMethod = delegateType.GetMethod("Invoke");
            ParameterInfo[] parameters = invokeMethod.GetParameters();
            if (invokeMethod.ContainsGenericParameters)
                throw new ScriptBlockToPowerShellNotSupportedException(
                    "CantConvertScriptBlockToOpenGenericType",
                    "AutomationExceptions",
                    delegateType);
            var parameterExprs = new List<ParameterExpression>();
                parameterExprs.Add(Expression.Parameter(parameter.ParameterType));
            bool returnsSomething = !invokeMethod.ReturnType.Equals(typeof(void));
            Expression dollarUnderExpr;
            Expression dollarThisExpr;
            if (parameters.Length == 2 && !returnsSomething)
                // V1 was designed for System.EventHandler and not much else.
                // The first arg (sender) was bound to $this, the second (e or EventArgs) was bound to $_.
                // We do this for backwards compatibility, but we also bind the parameters (or $args) for
                // consistency w/ delegates that take more or fewer parameters.
                dollarUnderExpr = parameterExprs[1].Cast(typeof(object));
                dollarThisExpr = parameterExprs[0].Cast(typeof(object));
                dollarUnderExpr = ExpressionCache.AutomationNullConstant;
                dollarThisExpr = ExpressionCache.AutomationNullConstant;
            Expression call = Expression.Call(
                Expression.Constant(this),
                CachedReflectionInfo.ScriptBlock_InvokeAsDelegateHelper,
                dollarUnderExpr,
                dollarThisExpr,
                Expression.NewArrayInit(typeof(object), parameterExprs.Select(static p => p.Cast(typeof(object)))));
            if (returnsSomething)
                call = DynamicExpression.Dynamic(
                    PSConvertBinder.Get(invokeMethod.ReturnType),
                    invokeMethod.ReturnType,
                    call);
            return Expression.Lambda(delegateType, call, parameterExprs).Compile();
        internal object InvokeAsDelegateHelper(object dollarUnder, object dollarThis, object[] args)
            // Retrieve context and current runspace to ensure that we throw exception, if this is non-default runspace.
            RunspaceBase runspace = (RunspaceBase)context.CurrentRunspace;
            List<object> rawResult = new List<object>();
            Pipe outputPipe = new Pipe(rawResult);
                scriptThis: dollarThis,
            return GetRawResult(rawResult, wrapToPSObject: false);
        /// An attempt was made to use the scriptblock outside the engine.
                string scriptText = this.ToString();
                scriptText = ErrorCategoryInfo.Ellipsize(CultureInfo.CurrentUICulture, scriptText);
                    ParserStrings.ScriptBlockDelegateInvokedFromWrongThread,
                e.SetErrorId("ScriptBlockDelegateInvokedFromWrongThread");
        /// <param name="dollarUnder">
        /// The value of the $_ variable for the script block. If AutomationNull.Value,
        /// the $_ variable is not created.
        /// The value of the $input variable for the script block. If AutomationNull.Value,
        /// the $input variable is not created.
        /// <exception cref="RuntimeException">A script exception occurred.</exception>
        internal Collection<PSObject> DoInvoke(object dollarUnder, object input, object[] args)
        /// This is a helper function to wrap script execution results
        /// in PSObjects.
        private static Collection<PSObject> GetWrappedResult(List<object> result)
            Collection<PSObject> wrappedResult = new Collection<PSObject>();
            for (int i = 0; i < result.Count; i++)
                wrappedResult.Add(LanguagePrimitives.AsPSObjectOrNull(result[i]));
            return wrappedResult;
        /// <param name="errorHandlingBehavior"></param>
        ///   The value of the $_ variable for the script block. If AutomationNull.Value,
        ///   the $_ variable is not created.
        ///   The value of the $input variable for the script block. If AutomationNull.Value,
        ///   the $input variable is not created.
        /// <param name="scriptThis"></param>
        internal object DoInvokeReturnAsIs(
                useLocalScope: useLocalScope,
            return GetRawResult(result, wrapToPSObject: true);
        internal void InvokeWithPipe(
            Pipe outputPipe,
            bool propagateAllExceptionsToTop = false,
            List<PSVariable> variablesToDefine = null,
            object[] args = null)
            bool shouldGenerateEvent = false;
            bool oldPropagateExceptions = false;
            if (SessionStateInternal != null && SessionStateInternal.ExecutionContext != context)
                context = SessionStateInternal.ExecutionContext;
                shouldGenerateEvent = true;
            else if (context == null)
                // This will throw.
                GetContextFromTLS();
                if (propagateAllExceptionsToTop)
                    oldPropagateExceptions = context.PropagateExceptionsToEnclosingStatementBlock;
                    context.PropagateExceptionsToEnclosingStatementBlock = true;
                    var runspace = (RunspaceBase)context.CurrentRunspace;
                        InvokeWithPipeImpl(
                            functionsToDefine,
                        context.PropagateExceptionsToEnclosingStatementBlock = oldPropagateExceptions;
            if (shouldGenerateEvent)
                context.Events.SubscribeEvent(
                    eventName: PSEngineEvent.OnScriptBlockInvoke,
                    sourceIdentifier: PSEngineEvent.OnScriptBlockInvoke,
                    handlerDelegate: new PSEventReceivedEventHandler(OnScriptBlockInvokeEventHandler),
                var scriptBlockInvocationEventArgs = new ScriptBlockInvocationEventArgs(
                    scriptBlock: this,
                context.Events.GenerateEvent(
                    args: new object[1] { scriptBlockInvocationEventArgs },
                scriptBlockInvocationEventArgs.Exception?.Throw();
        /// Handles OnScriptBlockInvoke event, this is called by the event manager.
        private static void OnScriptBlockInvokeEventHandler(object sender, PSEventArgs args)
            var eventArgs = (object)args.SourceEventArgs as ScriptBlockInvocationEventArgs;
            Diagnostics.Assert(eventArgs != null,
                "Event Arguments to OnScriptBlockInvokeEventHandler should not be null");
                ScriptBlock sb = eventArgs.ScriptBlock;
                sb.InvokeWithPipeImpl(
                    eventArgs.UseLocalScope,
                    functionsToDefine: null,
                    variablesToDefine: null,
                    eventArgs.ErrorHandlingBehavior,
                    eventArgs.DollarUnder,
                    eventArgs.Input,
                    eventArgs.ScriptThis,
                    eventArgs.OutputPipe,
                    eventArgs.InvocationInfo,
                    eventArgs.Args);
        internal void SetPSScriptRootAndPSCommandPath(MutableTuple locals, ExecutionContext context)
            var psScriptRoot = string.Empty;
            var psCommandPath = string.Empty;
            if (!string.IsNullOrEmpty(File))
                psScriptRoot = Path.GetDirectoryName(File);
                psCommandPath = File;
            locals.SetAutomaticVariable(AutomaticVariable.PSScriptRoot, psScriptRoot, context);
            locals.SetAutomaticVariable(AutomaticVariable.PSCommandPath, psCommandPath, context);
    /// A steppable pipeline wrapper object...
        Justification = "Consider Name change during API review")]
    public sealed class SteppablePipeline : IDisposable
        internal SteppablePipeline(ExecutionContext context, PipelineProcessor pipeline)
            ArgumentNullException.ThrowIfNull(pipeline);
            _pipeline = pipeline;
        private readonly PipelineProcessor _pipeline;
        private bool _expectInput;
        /// Begin execution of a steppable pipeline. This overload doesn't reroute output and error pipes.
        /// <param name="expectInput"><see langword="true"/> if you plan to write input into this pipe; <see langword="false"/> otherwise.</param>
        public void Begin(bool expectInput) => Begin(expectInput, commandRuntime: (ICommandRuntime)null);
        /// Begin execution of a steppable pipeline, using the command running currently in the specified context to figure
        /// out how to route the output and errors.
        /// <param name="contextToRedirectTo">Context used to figure out how to route the output and errors.</param>
        public void Begin(bool expectInput, EngineIntrinsics contextToRedirectTo)
            ArgumentNullException.ThrowIfNull(contextToRedirectTo);
            ExecutionContext executionContext = contextToRedirectTo.SessionState.Internal.ExecutionContext;
            CommandProcessorBase commandProcessor = executionContext.CurrentCommandProcessor;
            ICommandRuntime crt = commandProcessor?.CommandRuntime;
            Begin(expectInput, crt);
        /// Begin execution of a steppable pipeline, using the calling command to figure
        /// out how to route the output and errors. This is the most effective
        /// way to start stepping.
        /// <param name="command">The command you're calling this from (i.e. instance of PSCmdlet or value of $PSCmdlet variable).</param>
        public void Begin(InternalCommand command)
            if (command is null || command.MyInvocation is null)
                throw new ArgumentNullException(nameof(command));
            Begin(command.MyInvocation.ExpectingInput, command.commandRuntime);
        private void Begin(bool expectInput, ICommandRuntime commandRuntime)
                _pipeline.ExecutionScope = _context.EngineSessionState.CurrentScope;
                _context.PushPipelineProcessor(_pipeline);
                _expectInput = expectInput;
                // Start the pipeline, if the command calling this pipeline is
                // not expecting input (as indicated by it's position in the pipeline
                // then neither should we.
                MshCommandRuntime crt = commandRuntime as MshCommandRuntime;
                if (crt != null)
                    if (crt.OutputPipe != null)
                        _pipeline.LinkPipelineSuccessOutput(crt.OutputPipe);
                    if (crt.ErrorOutputPipe != null)
                        _pipeline.LinkPipelineErrorOutput(crt.ErrorOutputPipe);
                _pipeline.StartStepping(_expectInput);
                // then pop this pipeline...
                _context.PopPipelineProcessor(true);
        /// Process a single input object.
        /// <param name="input">The object to process.</param>
        /// <returns>A collection of 0 or more result objects.</returns>
        public Array Process(object input)
                if (_expectInput)
                    return _pipeline.Step(input);
                    return _pipeline.Step(AutomationNull.Value);
        /// Process a single PSObject. This overload exists to deal with the fact
        /// that the PowerShell runtime will PSBase an object before passing it to
        /// a .NET API call with argument type object.
        /// <param name="input">The input object to process.</param>
        public Array Process(PSObject input)
        /// Process with no input. This is used in the case where
        /// Begin() was called with $false so we won't send any
        /// input to be processed.
        /// <returns>The result of the execution.</returns>
        public Array Process()
        /// End the execution of this steppable pipeline. This will
        /// complete the execution and dispose the results.
        public Array End()
                return _pipeline.DoComplete();
                // then pop this pipeline and dispose it...
        /// Clean resources for script commands of this steppable pipeline.
        /// The way we handle 'Clean' blocks in a steppable pipeline makes sure that:</para>
        /// <para>1. The 'Clean' blocks get to run if any exception is thrown from 'Begin/Process/End'.</para>
        /// <para>2. The 'Clean' blocks get to run if 'End' finished successfully.</para>
        /// pipeline. This method allows a user to do that from the 'Clean' block of the proxy function.</para>
        public void Clean()
            if (_pipeline.Commands is null)
                // The pipeline commands have been disposed. In this case, 'Clean'
                // should have already been called on the pipeline processor.
                _pipeline.DoCleanup();
        /// When this object is disposed, the contained pipeline should also be disposed.
    /// Defines the exception thrown when conversion from ScriptBlock to PowerShell is forbidden
    /// (i.e. when the script block has undeclared variables or more than one statement)
    public class ScriptBlockToPowerShellNotSupportedException : RuntimeException
        public ScriptBlockToPowerShellNotSupportedException()
        public ScriptBlockToPowerShellNotSupportedException(string message)
        public ScriptBlockToPowerShellNotSupportedException(string message, Exception innerException)
        internal ScriptBlockToPowerShellNotSupportedException(
            : base(string.Format(CultureInfo.CurrentCulture, message, arguments), innerException)
            => this.SetErrorId(errorId);
        /// Initializes a new instance of ScriptBlockToPowerShellNotSupportedException with serialization parameters.
        protected ScriptBlockToPowerShellNotSupportedException(SerializationInfo info, StreamingContext context)
    /// Defines Event arguments passed to OnScriptBlockInvocationEventHandler.
    internal sealed class ScriptBlockInvocationEventArgs : EventArgs
        /// Constructs ScriptBlockInvocationEventArgs.
        /// <param name="scriptBlock">The scriptblock to invoke
        /// /// <param name="useLocalScope"></param>
        /// <param name="outputPipe">The output pipe which has the results of the invocation
        /// <param name="invocationInfo">The information about current state of the runspace.</param>
        /// <exception cref="ArgumentNullException">ScriptBlock is null
        internal ScriptBlockInvocationEventArgs(
            ScriptBlock.ErrorHandlingBehavior errorHandlingBehavior,
            OutputPipe = outputPipe;
            UseLocalScope = useLocalScope;
            ErrorHandlingBehavior = errorHandlingBehavior;
            DollarUnder = dollarUnder;
            Input = input;
            ScriptThis = scriptThis;
        internal bool UseLocalScope { get; set; }
        internal ScriptBlock.ErrorHandlingBehavior ErrorHandlingBehavior { get; set; }
        internal object DollarUnder { get; set; }
        internal object Input { get; set; }
        internal object ScriptThis { get; set; }
        internal Pipe OutputPipe { get; set; }
        internal object[] Args { get; set; }
        /// Holds the exception thrown during scriptblock invocation.
        internal ExceptionDispatchInfo Exception { get; set; }

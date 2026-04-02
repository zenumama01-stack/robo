    /// This class contains the execution context that gets passed
    /// around to commands. This is all of the information that lets you get
    /// at session state and the host interfaces.
    internal class ExecutionContext
        /// The events received by this runspace.
        internal PSLocalEventManager Events { get; private set; }
        internal HashSet<string> AutoLoadingModuleInProgress { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        /// The debugger for the interpreter.
        internal ScriptDebugger Debugger
            get { return _debugger; }
        internal ScriptDebugger _debugger;
        internal int _debuggingMode;
        /// Reset or clear the various context managers so the runspace can be reused without contamination.
        internal void ResetManagers()
            _debugger?.ResetDebugger();
            Events?.Dispose();
            Events = new PSLocalEventManager(this);
            this.transactionManager?.Dispose();
            this.transactionManager = new PSTransactionManager();
        /// The tracing mode for the interpreter.
        /// <value>True if tracing is turned on, false if it's turned off.</value>
        internal int PSDebugTraceLevel
                // Pretend that tracing is off if ignoreScriptDebug is true
                return IgnoreScriptDebug ? 0 : _debugTraceLevel;
                _debugTraceLevel = value;
        private int _debugTraceLevel;
        /// The step mode for the interpreter.
        /// <value>True of stepping is turned on, false if it's turned off.</value>
        internal bool PSDebugTraceStep
                return !IgnoreScriptDebug && _debugTraceStep;
                _debugTraceStep = value;
        private bool _debugTraceStep;
        // Helper for generated code to handle running w/ no execution context
        internal static bool IsStrictVersion(ExecutionContext context, int majorVersion)
            context ??= LocalPipeline.GetExecutionContextFromTLS();
            return (context != null) && context.IsStrictVersion(majorVersion);
        /// Check to see a specific version of strict mode is enabled.  The check is always scoped,
        /// even though in version 1 the check was engine wide.
        /// <param name="majorVersion">The version for a strict check about to be performed.</param>
        internal bool IsStrictVersion(int majorVersion)
            SessionStateScope scope = EngineSessionState.CurrentScope;
            while (scope != null)
                // If StrictModeVersion is null, we haven't seen set-strictmode, so check the parent scope.
                if (scope.StrictModeVersion != null)
                    // If StrictModeVersion is not null, just check the major version.  A version of 0
                    // is the same as off to make this a simple check.
                    return (scope.StrictModeVersion.Major >= majorVersion);
                // We shouldn't check global scope if we were in a module.
                if (scope == EngineSessionState.ModuleScope)
                scope = scope.Parent;
            // set-strictmode hasn't been used.
        /// Is true if the current statement in the interpreter should be traced...
        internal bool ShouldTraceStatement
                return !IgnoreScriptDebug && (_debugTraceLevel > 0 || _debugTraceStep);
        /// If true, then a script command processor should rethrow the exit exception instead of
        /// simply capturing it. This is used by the -file option on the console host.
        internal bool ScriptCommandProcessorShouldRethrowExit { get; set; } = false;
        /// If this flag is set to true, script trace output
        /// will not be generated regardless of the state of the
        /// trace flag.
        /// <value>The current state of the IgnoreScriptDebug flag.</value>
        internal bool IgnoreScriptDebug { get; set; } = true;
        /// Gets the automation engine instance.
        internal AutomationEngine Engine { get; private set; }
        internal InitialSessionState InitialSessionState { get; }
        /// Added for Win8: 336382
        /// Contains the name of the previous module that was processed. This
        /// allows you to skip this module when doing a lookup.
        internal string PreviousModuleProcessed { get; set; }
        /// Added for 4980967
        /// Contains the name of the latest module that was imported,
        /// Allows "module\function" to call the function from latest imported module instead of randomly choosing the first module in the moduletable.
        internal Hashtable previousModuleImported { get; set; } = new Hashtable();
        /// Contains the name of the module currently being processed. This
        internal string ModuleBeingProcessed { get; set; }
        /// Authorization manager for this runspace.
        internal AuthorizationManager AuthorizationManager { get; private set; }
        /// Gets the appropriate provider names for the default
        /// providers based on the type of the shell
        /// (single shell or custom shell).
        internal ProviderNames ProviderNames
                _providerNames ??= new SingleShellProviderNames();
                return _providerNames;
        private ProviderNames _providerNames;
        /// The module information for this engine...
        internal ModuleIntrinsics Modules { get; private set; }
        /// Get the shellID for this runspace...
        internal string ShellID
                if (_shellId == null)
                    // Use the ShellID from PSAuthorizationManager before everything else because that's what's used
                    // to check execution policy...
                    if (AuthorizationManager is PSAuthorizationManager && !string.IsNullOrEmpty(AuthorizationManager.ShellId))
                        _shellId = AuthorizationManager.ShellId;
                        // Finally fall back to the default shell id...
                        _shellId = Utils.DefaultPowerShellShellID;
                return _shellId;
        private string _shellId;
        /// Session State with which this instance of engine works.
        internal SessionStateInternal EngineSessionState { get; set; }
        /// The default or top-level session state instance for the
        /// engine.
        internal SessionStateInternal TopLevelSessionState { get; private set; }
        /// Get the SessionState facade for the internal session state APIs.
        internal SessionState SessionState
                return EngineSessionState.PublicSessionState;
        /// Get/set constraints for this execution environment.
        internal PSLanguageMode LanguageMode
                return _languageMode;
                // If we're moving to ConstrainedLanguage, invalidate the binding
                // caches. After that, the binding rules encode the language mode.
                if (value == PSLanguageMode.ConstrainedLanguage)
                    HasRunspaceEverUsedConstrainedLanguageMode = true;
                    // If 'ExecutionContext.HasEverUsedConstrainedLanguage' is already set to True, then we have
                    // already invalidated all cached binders, and binders already started to generate code with
                    // consideration of 'LanguageMode'. In such case, we don't need to invalidate cached binders
                    // again.
                    // Note that when executing script blocks marked as 'FullLanguage' in a 'ConstrainedLanguage'
                    // environment, we will set and Restore 'context.LanguageMode' very often. But we should not
                    // invalidate the cached binders every time we restore to 'ConstrainedLanguage'.
                    if (!ExecutionContext.HasEverUsedConstrainedLanguage)
                        lock (lockObject)
                            // If another thread has already set 'ExecutionContext.HasEverUsedConstrainedLanguage'
                            // while we are waiting on the lock, then nothing needs to be done.
                                PSSetMemberBinder.InvalidateCache();
                                PSInvokeMemberBinder.InvalidateCache();
                                PSConvertBinder.InvalidateCache();
                                PSBinaryOperationBinder.InvalidateCache();
                                PSGetIndexBinder.InvalidateCache();
                                PSSetIndexBinder.InvalidateCache();
                                PSCreateInstanceBinder.InvalidateCache();
                                // Set 'HasEverUsedConstrainedLanguage' at the very end to guarantee other threads to wait until
                                // all invalidation operations are done.
                                UntrustedObjects = new ConditionalWeakTable<object, object>();
                                ExecutionContext.HasEverUsedConstrainedLanguage = true;
                // Conversion caches don't have version info / binding rules, so must be
                // cleared every time.
                LanguagePrimitives.RebuildConversionCache();
                _languageMode = value;
        private PSLanguageMode _languageMode = PSLanguageMode.FullLanguage;
        /// True if this runspace has ever used constrained language mode.
        internal bool HasRunspaceEverUsedConstrainedLanguageMode { get; private set; }
        /// Indicate if a parameter binding is happening that transitions the execution from ConstrainedLanguage
        /// mode to a trusted FullLanguage command.
        internal bool LanguageModeTransitionInParameterBinding { get; set; }
        /// True if we've ever used ConstrainedLanguage. If this is the case, then the binding restrictions
        /// need to also validate against the language mode.
        internal static bool HasEverUsedConstrainedLanguage { get; private set; }
        #region Variable Tracking
        /// Initialized when 'ConstrainedLanguage' is applied.
        /// The objects contained in this table are considered to be untrusted.
        private static ConditionalWeakTable<object, object> UntrustedObjects { get; set; }
        /// Helper for checking if the given value is marked as untrusted.
        internal static bool IsMarkedAsUntrusted(object value)
            var baseValue = PSObject.Base(value);
            if (baseValue != null && baseValue != NullString.Value)
                result = UntrustedObjects.TryGetValue(baseValue, out _);
        /// Helper for marking a value as untrusted.
        internal static void MarkObjectAsUntrusted(object value)
            // If the value is a PSObject, then we mark its base object untrusted
                // It's actually setting a key value pair when the key doesn't exist
                UntrustedObjects.GetValue(baseValue, static key => null);
                    // If it's a PSReference object, we need to also mark the value it's holding on.
                    // This could result in a recursion if psRef.Value points to itself directly or indirectly, so we check if psRef.Value is already
                    // marked before making a recursive call. The additional check adds extra overhead for handling PSReference object, but it should
                    // be rare in practice.
                    var psRef = baseValue as PSReference;
                    if (psRef != null && !IsMarkedAsUntrusted(psRef.Value))
                        MarkObjectAsUntrusted(psRef.Value);
                catch { /* psRef.Value may call PSVariable.Value under the hood, which may throw arbitrary exception */ }
        /// Helper for setting the untrusted value of an assignment to either a 'Global:' variable, or a 'Script:' variable in a module scope.
        /// This method is for tracking assignment to global variables and module script scope varaibles in ConstrainedLanguage mode. Those variables
        /// can go across boundaries between ConstrainedLanguage and FullLanguage, and make it easy for a trusted script to use data from an untrusted
        /// environment. Therefore, in ConstrainedLanguage mode, we need to mark the value objects assigned to those variables as untrusted.
        internal static void MarkObjectAsUntrustedForVariableAssignment(PSVariable variable, SessionStateScope scope, SessionStateInternal sessionState)
            if (scope.Parent == null ||  // If it's the global scope, OR
                (sessionState.Module != null &&  // it's running in a module AND
                 scope.ScriptScope == scope && scope.Parent.Parent == null)) // it's the module's script scope (scope.Parent is global scope and scope.ScriptScope points to itself)
                // We are setting value for either a 'Global:' variable, or a 'Script:' variable within a module in 'ConstrainedLanguage' mode.
                // Global variable may be referenced within trusted script block (scriptBlock.LanguageMode == 'FullLanguage'), and users could
                // also set a 'Script:' variable in a trusted module scope from 'ConstrainedLanguage' environment via '& $mo { $script:<var> }'.
                // So we need to mark the value as untrusted.
                MarkObjectAsUntrusted(variable.Value);
        /// The result object is assumed generated by operating on the original object.
        /// So if the original object is from an untrusted input source, we mark the result object as untrusted.
        internal static void PropagateInputSource(object originalObject, object resultObject, PSLanguageMode currentLanguageMode)
            // The untrusted flag is populated only in FullLanguage mode and ConstrainedLanguage has been used in the process before.
            if (ExecutionContext.HasEverUsedConstrainedLanguage && currentLanguageMode == PSLanguageMode.FullLanguage && IsMarkedAsUntrusted(originalObject))
                MarkObjectAsUntrusted(resultObject);
        /// If true the PowerShell debugger will use FullLanguage mode, otherwise it will use the current language mode.
        internal bool UseFullLanguageModeInDebugger
                return InitialSessionState != null && InitialSessionState.UseFullLanguageModeInDebugger;
        internal static readonly List<string> ModulesWithJobSourceAdapters = new List<string>
                Utils.ScheduledJobModuleName,
        /// Is true if the PSScheduledJob module is loaded for this runspace.
        internal bool IsModuleWithJobSourceAdapterLoaded
        /// Gets the location globber for the session state for
        /// this instance of the runspace.
        internal LocationGlobber LocationGlobber
                _locationGlobber = new LocationGlobber(this.SessionState);
                return _locationGlobber;
        private LocationGlobber _locationGlobber;
        /// The assemblies that have been loaded for this runspace.
        internal Dictionary<string, Assembly> AssemblyCache { get; private set; }
        #region Engine State
        /// The state for current engine that is running.
        internal EngineState EngineState { get; set; } = EngineState.None;
        #region GetSetVariable methods
        /// Get a variable out of session state.
        internal object GetVariableValue(VariablePath path)
            CmdletProviderContext context;
            SessionStateScope scope;
            return EngineSessionState.GetVariableValue(path, out context, out scope);
        /// Get a variable out of session state. This calls GetVariable(name) and returns the
        /// value unless it is null in which case it returns the defaultValue provided by the caller.
        internal object GetVariableValue(VariablePath path, object defaultValue)
            return EngineSessionState.GetVariableValue(path, out _, out _) ?? defaultValue;
        /// Set a variable in session state.
        internal void SetVariable(VariablePath path, object newValue)
            EngineSessionState.SetVariable(path, newValue, true, CommandOrigin.Internal);
        internal T GetEnumPreference<T>(VariablePath preferenceVariablePath, T defaultPref, out bool defaultUsed)
            object val = EngineSessionState.GetVariableValue(preferenceVariablePath, out _, out _);
            if (val is T)
                if (val is ActionPreference actionPreferenceValue)
                    CheckActionPreference(preferenceVariablePath, actionPreferenceValue, defaultPref);
                T convertedResult = (T)val;
                defaultUsed = false;
                return convertedResult;
            defaultUsed = true;
            T result = defaultPref;
                    string valString = val as string;
                    if (valString != null)
                        result = (T)Enum.Parse(typeof(T), valString, true);
                        result = (T)PSObject.Base(val);
                    if (result is ActionPreference actionPreferenceValue)
                catch (InvalidCastException)
                    // default value is used
        private void CheckActionPreference(VariablePath preferenceVariablePath, ActionPreference preference, object defaultValue)
            if (preference == ActionPreference.Suspend)
                // ActionPreference.Suspend is reserved for future use. When it is used, reset
                // the variable to its default.
                string message = StringUtil.Format(ErrorPackage.ReservedActionPreferenceReplacedError, preference, preferenceVariablePath.UserPath, defaultValue);
                EngineSessionState.SetVariable(preferenceVariablePath, defaultValue, true, CommandOrigin.Internal);
                throw new NotSupportedException(message);
        /// Same as GetEnumPreference, but for boolean values.
        /// <param name="preferenceVariablePath"></param>
        /// <param name="defaultPref"></param>
        /// <param name="defaultUsed"></param>
        internal bool GetBooleanPreference(VariablePath preferenceVariablePath, bool defaultPref, out bool defaultUsed)
            if (val is null)
                return defaultPref;
            defaultUsed = !LanguagePrimitives.TryConvertTo(val, out bool converted);
            return defaultUsed ? defaultPref : converted;
        #endregion GetSetVariable methods
        #region HelpSystem
        /// Help system for this engine context.
        internal HelpSystem HelpSystem
            get { return _helpSystem ??= new HelpSystem(this); }
        private HelpSystem _helpSystem;
        #region FormatAndOutput
        internal object FormatInfo { get; set; }
        internal Dictionary<string, ScriptBlock> CustomArgumentCompleters { get; set; }
        internal Dictionary<string, ScriptBlock> NativeArgumentCompleters { get; set; }
        /// Routine to create a command(processor) instance using the factory.
        /// <param name="command">The name of the command to lookup.</param>
        /// <param name="dotSource"></param>
        /// <param name="forCompletion"></param>
        /// <returns>The command processor object.</returns>
        internal CommandProcessorBase CreateCommand(string command, bool dotSource, bool forCompletion = false)
            CommandOrigin commandOrigin = this.EngineSessionState.CurrentScope.ScopeOrigin;
                CommandDiscovery.LookupCommandProcessor(command, commandOrigin, !dotSource, forCompletion);
            // Reset the command origin for script commands... // BUGBUG - dotting can get around command origin checks???
            if (commandProcessor != null && commandProcessor is ScriptCommandProcessorBase)
                commandProcessor.Command.CommandOriginInternal = CommandOrigin.Internal;
            return commandProcessor;
        /// Hold the current command.
        /// <value>Reference to command discovery</value>
        internal CommandProcessorBase CurrentCommandProcessor { get; set; }
        /// Redirect to the CommandDiscovery in the engine.
        internal CommandDiscovery CommandDiscovery
                return Engine.CommandDiscovery;
        /// Interface that should be used for interaction with host.
        internal InternalHost EngineHostInterface
            // set not provided: it's not meaningful to change the host post-construction.
        /// Interface to be used for interaction with internal
        /// host. InternalHost wraps the host supplied
        /// during construction. Use this wrapper to access
        /// functionality specific to InternalHost.
        internal InternalHost InternalHost
            get { return EngineHostInterface; }
        /// Interface to the public API for the engine.
        internal EngineIntrinsics EngineIntrinsics
            get { return _engineIntrinsics ??= new EngineIntrinsics(this); }
        private EngineIntrinsics _engineIntrinsics;
        /// Log context cache.
        internal LogContextCache LogContextCache { get; } = new LogContextCache();
        #region Output pipes
        /// The PipelineWriter provided by the connection object for success output.
        internal PipelineWriter ExternalSuccessOutput { get; set; }
        /// The PipelineWriter provided by the connection object for error output.
        internal PipelineWriter ExternalErrorOutput { get; set; }
        /// The PipelineWriter provided by the connection object for progress output.
        internal PipelineWriter ExternalProgressOutput { get; set; }
        internal class SavedContextData
            private readonly bool _stepScript;
            private readonly bool _ignoreScriptDebug;
            private readonly int _PSDebug;
            private readonly Pipe _shellFunctionErrorOutputPipe;
            public SavedContextData(ExecutionContext context)
                _stepScript = context.PSDebugTraceStep;
                _ignoreScriptDebug = context.IgnoreScriptDebug;
                _PSDebug = context.PSDebugTraceLevel;
                _shellFunctionErrorOutputPipe = context.ShellFunctionErrorOutputPipe;
            public void RestoreContextData(ExecutionContext context)
                context.PSDebugTraceStep = _stepScript;
                context.IgnoreScriptDebug = _ignoreScriptDebug;
                context.PSDebugTraceLevel = _PSDebug;
                context.ShellFunctionErrorOutputPipe = _shellFunctionErrorOutputPipe;
        /// Host uses this to saves context data when entering a nested prompt.
        internal SavedContextData SaveContextData()
            return new SavedContextData(this);
        internal void ResetShellFunctionErrorOutputPipe()
            ShellFunctionErrorOutputPipe = null;
        internal Pipe RedirectErrorPipe(Pipe newPipe)
            Pipe oldPipe = ShellFunctionErrorOutputPipe;
            ShellFunctionErrorOutputPipe = newPipe;
            return oldPipe;
        /// Reset all of the redirection book keeping variables. This routine should be called when starting to
        /// execute a script.
        internal void ResetRedirection()
        /// Function and Script command processors will route their error output to
        /// this pipe if set, unless explicitly routed elsewhere. We also keep track
        /// of the first time this value is set so we can know if it's the default
        /// error output or not.
        internal Pipe ShellFunctionErrorOutputPipe { get; set; }
        /// Supports expression Warning output redirection.
        internal Pipe ExpressionWarningOutputPipe { get; set; }
        /// Supports expression Verbose output redirection.
        internal Pipe ExpressionVerboseOutputPipe { get; set; }
        internal Pipe ExpressionDebugOutputPipe { get; set; }
        /// Supports expression Information output redirection.
        internal Pipe ExpressionInformationOutputPipe { get; set; }
        #endregion Output pipes
        #region Append to $error
        /// Appends the object to $global:error if it's an error record or exception.
        /// ErrorRecord or Exception to be written to $global:error
        /// <exception cref="ExtendedTypeSystemException">
        /// (get-only) An error occurred accessing $ERROR.
        internal void AppendDollarError(object obj)
            ErrorRecord objAsErrorRecord = obj as ErrorRecord;
            if (objAsErrorRecord is null && obj is not Exception)
                Diagnostics.Assert(false, "Object to append was neither an ErrorRecord nor an Exception in ExecutionContext.AppendDollarError");
            if (DollarErrorVariable is not ArrayList arraylist)
                Diagnostics.Assert(false, "$error should be a global constant ArrayList");
            // Don't add the same exception twice...
            if (arraylist.Count > 0)
                // There may be exceptions stored directly in which case
                // the direct comparison will catch them...
                if (arraylist[0] == obj)
                // otherwise check the exception members of the error records...
                ErrorRecord er1 = arraylist[0] as ErrorRecord;
                if (er1 != null && objAsErrorRecord != null && er1.Exception == objAsErrorRecord.Exception)
            const int maxErrorCount = 256;
            int numToErase = arraylist.Count - (maxErrorCount - 1);
            if (numToErase > 0)
                arraylist.RemoveRange(
                    maxErrorCount - 1,
                    numToErase);
            arraylist.Insert(0, obj);
        #region Scope or Commands (in pipeline) Depth Count
        /// Check if the stack would overflow soon, if so, throw ScriptCallDepthException.
        /// <exception cref="ScriptCallDepthException">
        /// If the stack would overflow soon.
        internal static void CheckStackDepth()
                RuntimeHelpers.EnsureSufficientExecutionStack();
            catch (InsufficientExecutionStackException)
                throw new ScriptCallDepthException();
        /// The current connection object.
        private Runspace _currentRunspace;
        // This should be internal, but it need to be friend of remoting dll.
        internal Runspace CurrentRunspace
            get { return _currentRunspace; }
            set { _currentRunspace = value; }
        /// Each pipeline has a stack of pipeline processor. This method
        /// pushes pp in to stack for currently executing pipeline.
        /// <param name="pp"></param>
        internal void PushPipelineProcessor(PipelineProcessor pp)
            if (_currentRunspace == null)
            LocalPipeline lpl = (LocalPipeline)((RunspaceBase)_currentRunspace).GetCurrentlyRunningPipeline();
            if (lpl == null)
            lpl.Stopper.Push(pp);
        /// Each pipeline has a stack of pipeline processor. This method pops the
        /// top item from the stack.
        internal void PopPipelineProcessor(bool fromSteppablePipeline)
            lpl.Stopper.Pop(fromSteppablePipeline);
        /// This flag is checked by parser to stop loops etc.
        internal bool CurrentPipelineStopping
                return lpl.IsStopping;
        /// True means one of these:
        /// 1) there is a trap statement in a dynamically enclosing statement block that might catch an exception.
        /// 2) execution happens inside a PS class and exceptions should be propagated all the way up, even if there is no enclosing try-catch-finally.
        internal bool PropagateExceptionsToEnclosingStatementBlock { get; set; }
        internal RuntimeException CurrentExceptionBeingHandled { get; set; }
        /// Shortcut to get at $?
        /// <value>The current value of $? </value>
        internal bool QuestionMarkVariableValue { get; set; } = true;
        /// Shortcut to get at $error.
        /// <value>The current value of $global:error </value>
        internal object DollarErrorVariable
                CmdletProviderContext context = null;
                object resultItem = null;
                if (!Events.IsExecutingEventAction)
                    resultItem = EngineSessionState.GetVariableValue(
                        SpecialVariables.ErrorVarPath, out context, out scope);
                        SpecialVariables.EventErrorVarPath, out context, out scope);
                return resultItem;
                EngineSessionState.SetVariable(
                    SpecialVariables.ErrorVarPath, value, true, CommandOrigin.Internal);
        internal ActionPreference DebugPreferenceVariable
                bool defaultUsed = false;
                return this.GetEnumPreference(
                    SpecialVariables.DebugPreferenceVarPath,
                    InitialSessionState.DefaultDebugPreference,
                    out defaultUsed);
                this.EngineSessionState.SetVariable(
                    LanguagePrimitives.ConvertTo(value, typeof(ActionPreference), CultureInfo.InvariantCulture),
                    CommandOrigin.Internal);
        internal ActionPreference VerbosePreferenceVariable
                    SpecialVariables.VerbosePreferenceVarPath,
                    InitialSessionState.DefaultVerbosePreference,
        internal ActionPreference ErrorActionPreferenceVariable
                    SpecialVariables.ErrorActionPreferenceVarPath,
                    InitialSessionState.DefaultErrorActionPreference,
        internal ActionPreference WarningActionPreferenceVariable
                    SpecialVariables.WarningPreferenceVarPath,
                    InitialSessionState.DefaultWarningPreference,
        internal ActionPreference InformationActionPreferenceVariable
                    SpecialVariables.InformationPreferenceVarPath,
                    InitialSessionState.DefaultInformationPreference,
        internal object WhatIfPreferenceVariable
                object resultItem = this.EngineSessionState.GetVariableValue(
                    SpecialVariables.WhatIfPreferenceVarPath,
                    out context,
                    out scope);
        internal ConfirmImpact ConfirmPreferenceVariable
                    SpecialVariables.ConfirmPreferenceVarPath,
                    InitialSessionState.DefaultConfirmPreference,
                    LanguagePrimitives.ConvertTo(value, typeof(ConfirmImpact), CultureInfo.InvariantCulture),
        internal void RunspaceClosingNotification()
            EngineSessionState.RunspaceClosingNotification();
            _debugger?.Dispose();
            Events = null;
            this.transactionManager = null;
        /// Gets the type table instance for this engine.
        internal TypeTable TypeTable
                if (_typeTable == null)
                    _typeTable = new TypeTable();
                    _typeTableWeakReference = new WeakReference<TypeTable>(_typeTable);
                return _typeTable;
                _typeTable = value;
                _typeTableWeakReference = (value != null) ? new WeakReference<TypeTable>(value) : null;
        /// Here for PSObject, should probably not be used elsewhere, maybe not even in PSObject.
        internal WeakReference<TypeTable> TypeTableWeakReference
                    var unused = TypeTable;
                return _typeTableWeakReference;
        private TypeTable _typeTable;
        private WeakReference<TypeTable> _typeTableWeakReference;
        /// Gets the format info database for this engine.
                if (_formatDBManager == null)
                    // If no Formatter database has been created, then
                    // create and initialize an empty one.
                    _formatDBManager = new TypeInfoDataBaseManager();
                    _formatDBManager.Update(this.AuthorizationManager, this.EngineHostInterface);
                    if (this.InitialSessionState != null)
                        // Win8:418011: Set DisableFormatTableUpdates only after performing the initial update. Otherwise, formatDBManager will be
                        // in bad state.
                        _formatDBManager.DisableFormatTableUpdates = this.InitialSessionState.DisableFormatUpdates;
                return _formatDBManager;
                _formatDBManager = value;
        private TypeInfoDataBaseManager _formatDBManager;
        /// Gets the TransactionManager instance that controls transactions in the current
        /// instance.
        internal PSTransactionManager TransactionManager
                return transactionManager;
        internal PSTransactionManager transactionManager;
        /// This method is used for assembly loading requests stemmed from 'InitialSessionState' binding and module loading.
        /// <param name="source">Source of the assembly loading request, should be a module name when specified.</param>
        /// <param name="assemblyName">Name of the assembly to be loaded.</param>
        /// <param name="filePath">Path of the assembly to be loaded.</param>
        /// <param name="error">Exception that is caught when the loading fails.</param>
        internal Assembly AddAssembly(string source, string assemblyName, string filePath, out Exception error)
            // Search the cache by the path, and return the assembly if we find it.
            // It's common to have two loading requests for the same assembly when loading a module -- the first time for
            // resolving a binary module path, and the second time for actually processing that module.
            // That's not a problem when all the module assemblies are loaded into the default ALC. But in a scenario where
            // a module tries to hide its nested/root binary modules in a custom ALC, that will become a problem. This is
            // because:
            // in that scenario, the module will usually setup a handler to load the specific assemblies to the custom ALC,
            // and that will be how the first loading request gets served. However, after the module path is resolved with
            // the first loading, the path will be used for the second loading upon real module processing. Since we prefer
            // loading-by-path over loading-by-name in the 'LoadAssembly' call, we will end up loading the same assembly in
            // the default ALC (because we use 'Assembly.LoadFrom' which always loads an assembly to the default ALC) if we
            // do not search in the cache first. That will break the scenario, because the module means to isolate all its
            // dependencies from the default ALC, and it failed to do so.
            // Therefore, we need to search the cache first. The reason we use path as the key is to make sure the request
            // is for exactly the same assembly. The same assembly file should not be loaded into different ALC's by module
            // loading within the same PowerShell session (Runspace).
            // An example module targeting the abovementioned scenario will likely have the following file structure:
            // IsolatedModule
            //  │   IsolatedModule.psd1 (has 'NestedModules = @('Test.Isolated.Init.dll', 'Test.Isolated.Nested.dll')')
            //  │   Test.Isolated.Init.dll (contains the custom ALC and code to setup 'Resolving' handler)
            //  └───Dependencies (folder under module base)
            //         Newtonsoft.Json.dll (version 10.0.0.0 dependency)
            //         Test.Isolated.Nested.dll (nested binary module referencing the particular dependency)
            // In this example, the following events will happen in sequence:
            //  1. PowerShell is able to find 'Test.Isolated.Init.dll' under module base folder, so it will be loaded into
            //     the default ALC as expected and setup the 'Resolving' handler via the 'OnImport' call.
            //  2. PowerShell cannot find 'Test.Isolated.Nested.dll' under the module base folder, so it will call the method
            //     'FixFileName(.., bool canLoadAssembly)' to resolve the path of this binary module.
            //     This particular overload will attempt to load the assembly by name, which will be served by the 'Resolving'
            //     handler that was setup in the step 1. So, the assembly will be loaded into the custom ALC and insert to the
            //     assembly cache.
            //  3. Path of the nested module 'Test.Isolated.Init.dll' now has been resolved by the step 2 (assembly.Location).
            //     Now it's time to actually load this binary module for processing in the method 'LoadBinaryModule', which
            //     will make a call to this method with the resolved assembly file path.
            // At this poin, we will have to query the cache first, instead of calling 'LoadAssembly' directly, to make sure
            // that the assembly instance loaded in the custom ALC in step 2 gets returned back. Otherwise, the same assembly
            // file will be loaded in the default ALC because 'Assembly.LoadFrom' is used in 'LoadAssembly' and that API will
            // always load an assembly file to the default ALC, and that will break this scenario.
            if (TryGetFromAssemblyCache(source, filePath, out Assembly loadedAssembly))
            // Attempt to load the requested assembly, first by path then by name.
            loadedAssembly = LoadAssembly(assemblyName, filePath, out error);
            if (loadedAssembly is not null)
                AddToAssemblyCache(source, loadedAssembly);
        /// Add a loaded assembly to the 'AssemblyCache'.
        /// The <paramref name="source"/> is used as a prefix for the key to make it easy to remove all associated
        /// assemblies from the cache when a module gets unloaded.
        /// <param name="source">The source where the assembly comes from, should be a module name when specified.</param>
        /// <param name="assembly">The assembly we try to cache.</param>
        internal void AddToAssemblyCache(string source, Assembly assembly)
            // Try caching the assembly by its location if possible.
            // When it's a dynamic assembly, we use it's full name. This could happen with 'Import-Module -Assembly'.
            string key = string.IsNullOrEmpty(assembly.Location) ? assembly.FullName : assembly.Location;
            // When the assembly is from a module loading, we prefix the key with the source,
            // so we can remove it from the cache when the module gets unloaded.
            if (!string.IsNullOrEmpty(source))
                // Both 'source' and 'key' are of the string type, so no need to specify 'InvariantCulture'.
                key = $"{source}@{key}";
            AssemblyCache.TryAdd(key, assembly);
        /// Remove all cache entries that are associated with the specified source.
        internal void RemoveFromAssemblyCache(string source)
            if (string.IsNullOrEmpty(source))
            var keysToRemove = new List<string>();
            string prefix = $"{source}@";
            foreach (string key in AssemblyCache.Keys)
                if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    keysToRemove.Add(key);
            foreach (string key in keysToRemove)
                AssemblyCache.Remove(key);
        /// Try to get an assembly from the cache.
        private bool TryGetFromAssemblyCache(string source, string filePath, out Assembly assembly)
            if (string.IsNullOrEmpty(filePath))
            // Both 'source' and 'filePath' are of the string type, so no need to specify 'InvariantCulture'.
            string key = string.IsNullOrEmpty(source) ? filePath : $"{source}@{filePath}";
            return AssemblyCache.TryGetValue(key, out assembly);
        private static Assembly LoadAssembly(string name, string filePath, out Exception error)
            // First we try to load the assembly based on the filename
                    // codeql[cs/dll-injection-remote] - The dll is loaded during the initial state setup, which is expected behavior. This allows users hosting PowerShell to load additional C# types to enable their specific scenarios.
                    loadedAssembly = Assembly.LoadFrom(filePath);
                catch (FileNotFoundException fileNotFound)
                    error = fileNotFound;
                    error = fileLoadException;
                catch (BadImageFormatException badImage)
                    error = badImage;
                    error = securityException;
            // Then we try to load the assembly based on the given name
            if (!string.IsNullOrEmpty(name))
                string fixedName = null;
                // Remove the '.dll' if it's there...
                fixedName = name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                                ? Path.GetFileNameWithoutExtension(name)
                                : name;
                var assemblyString = Utils.IsPowerShellAssembly(fixedName)
                                         ? Utils.GetPowerShellAssemblyStrongName(fixedName)
                                         : fixedName;
                    loadedAssembly = Assembly.Load(new AssemblyName(assemblyString));
            // If the assembly is loaded, we ignore error as it may come from the filepath loading.
            if (loadedAssembly != null)
        /// Report an initialization-time error.
        /// <param name="resourceString">Resource string.</param>
        /// <param name="arguments">Arguments.</param>
        internal void ReportEngineStartupError(string resourceString, params object[] arguments)
                Cmdlet currentRunningModuleCommand;
                if (IsModuleCommandCurrentlyRunning(out currentRunningModuleCommand, out errorId))
                    RuntimeException rte = InterpreterError.NewInterpreterException(null, typeof(RuntimeException), null, errorId, resourceString, arguments);
                    currentRunningModuleCommand.WriteError(new ErrorRecord(rte.ErrorRecord, rte));
                    PSHost host = EngineHostInterface;
                    PSHostUserInterface ui = host.UI;
                    ui.WriteErrorLine(
                        StringUtil.Format(resourceString, arguments));
            catch (Exception) // swallow all exceptions
        /// <param name="error">Error to report.</param>
        internal void ReportEngineStartupError(string error)
                    RuntimeException rte = InterpreterError.NewInterpreterException(null, typeof(RuntimeException), null, errorId, "{0}", error);
                    ui.WriteErrorLine(error);
        internal void ReportEngineStartupError(Exception e)
                    var rte = e as RuntimeException;
                    error = rte != null
                        ? new ErrorRecord(rte.ErrorRecord, rte)
                        : new ErrorRecord(e, errorId, ErrorCategory.OperationStopped, null);
                    currentRunningModuleCommand.WriteError(error);
        /// <param name="errorRecord"></param>
        internal void ReportEngineStartupError(ErrorRecord errorRecord)
                if (IsModuleCommandCurrentlyRunning(out currentRunningModuleCommand, out _))
                    currentRunningModuleCommand.WriteError(errorRecord);
                    ui.WriteErrorLine(errorRecord.ToString());
        private bool IsModuleCommandCurrentlyRunning(out Cmdlet command, out string errorId)
            errorId = null;
            if (this.CurrentCommandProcessor != null)
                CommandInfo cmdletInfo = this.CurrentCommandProcessor.CommandInfo;
                if ((string.Equals(cmdletInfo.Name, "Import-Module", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(cmdletInfo.Name, "Remove-Module", StringComparison.OrdinalIgnoreCase)) &&
                    cmdletInfo.CommandType.Equals(CommandTypes.Cmdlet) &&
                    InitialSessionState.CoreModule.Equals(cmdletInfo.ModuleName, StringComparison.OrdinalIgnoreCase))
                    command = (Cmdlet)this.CurrentCommandProcessor.Command;
                    errorId = string.Equals(cmdletInfo.Name, "Import-Module", StringComparison.OrdinalIgnoreCase)
                                  ? "Module_ImportModuleError"
                                  : "Module_RemoveModuleError";
        /// Constructs an Execution context object for Automation Engine.
        /// <param name="engine">
        /// Engine that hosts this execution context
        /// <param name="hostInterface">
        /// Interface that should be used for interaction with host
        /// <param name="initialSessionState">
        /// InitialSessionState information
        internal ExecutionContext(AutomationEngine engine, PSHost hostInterface, InitialSessionState initialSessionState)
            InitialSessionState = initialSessionState;
            AuthorizationManager = initialSessionState.AuthorizationManager;
            InitializeCommon(engine, hostInterface);
        private void InitializeCommon(AutomationEngine engine, PSHost hostInterface)
            Engine = engine;
            transactionManager = new PSTransactionManager();
            _debugger = new ScriptDebugger(this);
            EngineHostInterface = hostInterface as InternalHost ?? new InternalHost(hostInterface, this);
            // Hook up the assembly cache
            AssemblyCache = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);
            // Initialize the fixed toplevel session state and the current session state
            TopLevelSessionState = EngineSessionState = new SessionStateInternal(this);
            // if authorizationmanager==null, this means the configuration
            // explicitly asked for dummy authorization manager.
            AuthorizationManager ??= new AuthorizationManager(null);
            // Set up the module intrinsics
            Modules = new ModuleIntrinsics(this);
        private static readonly object lockObject = new object();
    /// Enum that defines state of monad engine.
    internal enum EngineState
        /// Engine state is not defined or initialized.
        /// Engine available.
        Available = 1,
        /// Engine service is degraded.
        Degraded = 2,
        /// Engine is out of service.
        OutOfService = 3,
        /// Engine is stopped.
        Stopped = 4

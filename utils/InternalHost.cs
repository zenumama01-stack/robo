namespace System.Management.Automation.Internal.Host
    /// Wraps PSHost instances to provide a shim layer
    /// between InternalCommand and the host-supplied PSHost instance.
    /// This class exists for the purpose of ensuring that an externally-supplied PSHost meets the minimum proper required
    /// implementation, and also to provide a leverage point at which the monad engine can hook the interaction between the engine,
    /// cmdlets, and that external host.
    /// That leverage may be necessary to manage concurrent access between multiple pipelines sharing the same instance of
    /// PSHost.
    internal class InternalHost : PSHost, IHostSupportsInteractiveSession
        /// There should only be one instance of InternalHost per runspace (i.e. per engine), and all engine use of the host
        /// should be through that single instance.  If we ever accidentally create more than one instance of InternalHost per
        /// runspace, then some of the internal state checks that InternalHost makes, like checking the nestedPromptCounter, can
        /// be messed up.
        /// To ensure that this constraint is met, I wanted to make this class a singleton.  However, Hitesh rightly pointed out
        /// that a singleton would be appdomain-global, which would prevent having multiple runspaces per appdomain. So we will
        /// just have to be careful not to create extra instances of InternalHost per runspace.
        internal InternalHost(PSHost externalHost, ExecutionContext executionContext)
            Dbg.Assert(externalHost != null, "must supply an PSHost");
            Dbg.Assert(externalHost is not InternalHost, "try to create an InternalHost from another InternalHost");
            Dbg.Assert(executionContext != null, "must supply an ExecutionContext");
            _externalHostRef = new ObjectRef<PSHost>(externalHost);
            Context = executionContext;
            PSHostUserInterface ui = externalHost.UI;
            _internalUIRef = new ObjectRef<InternalHostUserInterface>(new InternalHostUserInterface(ui, this));
            _zeroGuid = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            _idResult = _zeroGuid;
        /// <exception cref="NotImplementedException">
        ///  when the external host's Name is null or empty.
                if (string.IsNullOrEmpty(_nameResult))
                    _nameResult = _externalHostRef.Value.Name;
                return _nameResult;
        ///  when the external host's Version is null.
                if (_versionResult == null)
                    _versionResult = _externalHostRef.Value.Version;
                return _versionResult;
        ///  when the external host's InstanceId is a zero Guid.
        public override System.Guid InstanceId
                if (_idResult == _zeroGuid)
                    _idResult = _externalHostRef.Value.InstanceId;
                return _idResult;
        public override System.Management.Automation.Host.PSHostUserInterface UI
                return _internalUIRef.Value;
        /// host UI. InternalHostUserInterface wraps the host UI
        /// supplied during construction. Use this wrapper to access
        internal InternalHostUserInterface InternalUI
        ///  when the external host's CurrentCulture is null.
                CultureInfo ci = _externalHostRef.Value.CurrentCulture ?? CultureInfo.InvariantCulture;
        /// If the external host's CurrentUICulture is null.
        public override CultureInfo CurrentUICulture
                CultureInfo ci = _externalHostRef.Value.CurrentUICulture ?? CultureInfo.InstalledUICulture;
        /// <param name="exitCode"></param>
            _externalHostRef.Value.SetShouldExit(exitCode);
        /// <seealso cref="ExitNestedPrompt"/>
            EnterNestedPrompt(null);
        private struct PromptContextData
            public object SavedCurrentlyExecutingCommandVarValue;
            public object SavedPSBoundParametersVarValue;
            public ExecutionContext.SavedContextData SavedContextData;
            public RunspaceAvailability RunspaceAvailability;
            public PSLanguageMode LanguageMode;
        /// Internal proxy for EnterNestedPrompt.
        /// <param name="callingCommand"></param>
        internal void EnterNestedPrompt(InternalCommand callingCommand)
            // Ensure we are in control of the pipeline
            LocalRunspace localRunspace = null;
            // This needs to be in a try / catch, since the LocalRunspace cast
            // tries to verify that the host supports interactive sessions.
            // Tests hosts do not.
            try { localRunspace = this.Runspace as LocalRunspace; }
                Pipeline currentlyRunningPipeline = this.Runspace.GetCurrentlyRunningPipeline();
                if ((currentlyRunningPipeline != null) &&
                    (currentlyRunningPipeline == localRunspace.PulsePipeline))
            // NTRAID#Windows OS Bugs-986407-2004/07/29 When kumarp has done the configuration work in the engine, it
            // should include setting a bit that indicates that the initialization is complete, and code should be
            // added here to throw an exception if this function is called before that bit is set.
            if (NestedPromptCount < 0)
                Dbg.Assert(false, "nested prompt counter should never be negative.");
                    InternalHostStrings.EnterExitNestedPromptOutOfSync);
            // Increment our nesting counter.  When we set the value of the variable, we will replace any existing variable
            // of the same name.  This is good, as any existing value is either 1) ours, and we have claim to replace it, or
            // 2) is a squatter, and we have claim to clobber it.
            ++NestedPromptCount;
            Context.SetVariable(SpecialVariables.NestedPromptCounterVarPath, NestedPromptCount);
            // On entering a subshell, save and reset values of certain bits of session state
            PromptContextData contextData = new PromptContextData();
            contextData.SavedContextData = Context.SaveContextData();
            contextData.SavedCurrentlyExecutingCommandVarValue = Context.GetVariableValue(SpecialVariables.CurrentlyExecutingCommandVarPath);
            contextData.SavedPSBoundParametersVarValue = Context.GetVariableValue(SpecialVariables.PSBoundParametersVarPath);
            contextData.RunspaceAvailability = this.Context.CurrentRunspace.RunspaceAvailability;
            contextData.LanguageMode = Context.LanguageMode;
            PSPropertyInfo commandInfoProperty = null;
            PSPropertyInfo stackTraceProperty = null;
            object oldCommandInfo = null;
            object oldStackTrace = null;
            if (callingCommand != null)
                Dbg.Assert(callingCommand.Context == Context, "I expect that the contexts should match");
                // Populate $CurrentlyExecutingCommand to facilitate debugging.  One of the gotchas is that we are going to want
                // to expose more and more debug info. We could just populate more and more local variables but that is probably
                // a lousy approach as it pollutes the namespace.  A better way to do it is to add NOTES to the variable value
                PSObject newValue = PSObject.AsPSObject(callingCommand);
                commandInfoProperty = newValue.Properties["CommandInfo"];
                if (commandInfoProperty == null)
                    newValue.Properties.Add(new PSNoteProperty("CommandInfo", callingCommand.CommandInfo));
                    oldCommandInfo = commandInfoProperty.Value;
                    commandInfoProperty.Value = callingCommand.CommandInfo;
                stackTraceProperty = newValue.Properties["StackTrace"];
                if (stackTraceProperty == null)
                    newValue.Properties.Add(new PSNoteProperty("StackTrace", new System.Diagnostics.StackTrace()));
                    oldStackTrace = stackTraceProperty.Value;
                    stackTraceProperty.Value = new System.Diagnostics.StackTrace();
                Context.SetVariable(SpecialVariables.CurrentlyExecutingCommandVarPath, newValue);
            _contextStack.Push(contextData);
            Dbg.Assert(_contextStack.Count == NestedPromptCount, "number of saved contexts should equal nesting count");
            Context.PSDebugTraceStep = false;
            Context.PSDebugTraceLevel = 0;
            Context.ResetShellFunctionErrorOutputPipe();
            // Lock down the language in the nested prompt
                Context.LanguageMode = PSLanguageMode.ConstrainedLanguage;
            this.Context.CurrentRunspace.UpdateRunspaceAvailability(RunspaceAvailability.AvailableForNestedCommand, true);
                _externalHostRef.Value.EnterNestedPrompt();
                // So where things really go south is this path; which is possible for hosts (like our ConsoleHost)
                // that don't return from EnterNestedPrompt immediately.
                //      EnterNestedPrompt() starts
                //          ExitNestedPrompt() called
                //          EnterNestedPrompt throws
                ExitNestedPromptHelper();
                if (commandInfoProperty != null)
                    commandInfoProperty.Value = oldCommandInfo;
                if (stackTraceProperty != null)
                    stackTraceProperty.Value = oldStackTrace;
            Dbg.Assert(NestedPromptCount >= 0, "nestedPromptCounter should be greater than or equal to 0");
        private void ExitNestedPromptHelper()
            --NestedPromptCount;
            // restore the saved context
            Dbg.Assert(_contextStack.Count > 0, "ExitNestedPrompt: called without any saved context");
            if (_contextStack.Count > 0)
                PromptContextData pcd = _contextStack.Pop();
                pcd.SavedContextData.RestoreContextData(Context);
                Context.LanguageMode = pcd.LanguageMode;
                Context.SetVariable(SpecialVariables.CurrentlyExecutingCommandVarPath, pcd.SavedCurrentlyExecutingCommandVarValue);
                Context.SetVariable(SpecialVariables.PSBoundParametersVarPath, pcd.SavedPSBoundParametersVarValue);
                this.Context.CurrentRunspace.UpdateRunspaceAvailability(pcd.RunspaceAvailability, true);
        /// <seealso cref="EnterNestedPrompt()"/>
            if (NestedPromptCount == 0)
                _externalHostRef.Value.ExitNestedPrompt();
            ExitNestedPromptException enpe = new ExitNestedPromptException();
            throw enpe;
                PSObject result = _externalHostRef.Value.PrivateData;
        /// <seealso cref="NotifyEndApplication"/>
            _externalHostRef.Value.NotifyBeginApplication();
        /// Called by the engine to notify the host that the execution of a legacy command has completed.
            _externalHostRef.Value.NotifyEndApplication();
        /// This property enables and disables the host debugger if debugging is supported.
        public override bool DebuggerEnabled { get; set; } = true;
        /// Gets the external host as an IHostSupportsInteractiveSession if it implements this interface;
        /// throws an exception otherwise.
        private IHostSupportsInteractiveSession GetIHostSupportsInteractiveSession()
            if (_externalHostRef.Value is not IHostSupportsInteractiveSession host)
            return host;
        /// Called by the engine to notify the host that a runspace push has been requested.
        /// <seealso cref="PopRunspace"/>
        public void PushRunspace(System.Management.Automation.Runspaces.Runspace runspace)
            IHostSupportsInteractiveSession host = GetIHostSupportsInteractiveSession();
            host.PushRunspace(runspace);
        /// Called by the engine to notify the host that a runspace pop has been requested.
        /// <seealso cref="PushRunspace"/>
            host.PopRunspace();
                return host.IsRunspacePushed;
                return host.Runspace;
        /// Checks if the host is in a nested prompt.
        /// <returns>True, if host in nested prompt
        /// false, otherwise.</returns>
        internal bool HostInNestedPrompt()
            if (NestedPromptCount > 0)
        /// Sets the reference to the external host and the internal UI to a temporary
        /// new host and its UI. This exists so that if the PowerShell/Pipeline
        /// object has a different host from the runspace it can set it's host during its
        /// invocation, and then revert it after the invocation is completed.
        /// <seealso cref="RevertHostRef"/> and
        internal void SetHostRef(PSHost psHost)
            _externalHostRef.Override(psHost);
            _internalUIRef.Override(new InternalHostUserInterface(psHost.UI, this));
        /// Reverts the temporary host set by SetHost. If no host was temporarily set, this has no effect.
        /// <seealso cref="SetHostRef"/> and
        internal void RevertHostRef()
            // nothing to revert if Host reference is not set.
            if (!IsHostRefSet)
            _externalHostRef.Revert();
            _internalUIRef.Revert();
        /// Returns true if the external host reference is temporarily set to another host, masking the original host.
        internal bool IsHostRefSet
            get { return _externalHostRef.IsOverridden; }
        internal PSHost ExternalHost
                return _externalHostRef.Value;
        internal int NestedPromptCount { get; private set; }
        // Masked variables.
        private readonly ObjectRef<PSHost> _externalHostRef;
        private readonly ObjectRef<InternalHostUserInterface> _internalUIRef;
        // Private variables.
        private string _nameResult;
        private Version _versionResult;
        private Guid _idResult;
        private readonly Stack<PromptContextData> _contextStack = new Stack<PromptContextData>();
        private readonly Guid _zeroGuid;

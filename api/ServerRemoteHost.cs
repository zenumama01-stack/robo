    /// The ServerRemoteHost class.
    internal class ServerRemoteHost : PSHost, IHostSupportsInteractiveSession
        /// Remote host user interface.
        private readonly ServerRemoteHostUserInterface _remoteHostUserInterface;
        /// Server method executor.
        private readonly ServerMethodExecutor _serverMethodExecutor;
        protected AbstractServerTransportManager _transportManager;
        /// ServerDriverRemoteHost.
        private readonly ServerDriverRemoteHost _serverDriverRemoteHost;
        /// Constructor for ServerRemoteHost.
        internal ServerRemoteHost(
            HostInfo hostInfo,
            AbstractServerTransportManager transportManager,
            ServerDriverRemoteHost serverDriverRemoteHost)
            Dbg.Assert(hostInfo != null, "Expected hostInfo != null");
            // Set host-info and the transport-manager.
            HostInfo = hostInfo;
            _serverDriverRemoteHost = serverDriverRemoteHost;
            // Create the executor for the host methods.
            _serverMethodExecutor = new ServerMethodExecutor(
                clientRunspacePoolId, clientPowerShellId, _transportManager);
            // Use HostInfo to create host-UI as null or non-null based on the client's host-UI.
            _remoteHostUserInterface = hostInfo.IsHostUINull ? null : new ServerRemoteHostUserInterface(this);
        internal ServerMethodExecutor ServerMethodExecutor
            get { return _serverMethodExecutor; }
        /// The user interface.
            get { return _remoteHostUserInterface; }
            get { return "ServerRemoteHost"; }
        /// Version.
            get { return RemotingConstants.HostVersion; }
        /// Instance id.
        public virtual bool IsRunspacePushed
                if (_serverDriverRemoteHost != null)
                    return _serverDriverRemoteHost.IsRunspacePushed;
                    throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetIsRunspacePushed);
        public Runspace Runspace { get; internal set; }
        /// Host info.
        internal HostInfo HostInfo { get; }
        #region Method Overrides
        /// Set should exit.
            _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetShouldExit, new object[] { exitCode });
        /// Enter nested prompt.
            throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.EnterNestedPrompt);
        /// Exit nested prompt.
            throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.ExitNestedPrompt);
        /// Notify begin application.
            // This is called when a native application is executed on the server. It gives the
            // host an opportunity to save state that might be altered by the native application.
            // This call should not be sent to the client because the native application running
            // on the server cannot affect the state of the machine on the client.
        /// Notify end application.
            // See note in NotifyBeginApplication.
        /// Current culture.
        public override CultureInfo CurrentCulture
                // Return the thread's current culture and rely on WinRM to set this
                // correctly based on the client's culture.
        /// Current ui culture.
                // Return the thread's current UI culture and rely on WinRM to set
                // this correctly based on the client's UI culture.
        /// Push runspace.
        public virtual void PushRunspace(Runspace runspace)
                _serverDriverRemoteHost.PushRunspace(runspace);
                throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.PushRunspace);
        /// Pop runspace.
        public virtual void PopRunspace()
            if ((_serverDriverRemoteHost != null) && (_serverDriverRemoteHost.IsRunspacePushed))
                if (_serverDriverRemoteHost.PropagatePop)
                    // Forward the PopRunspace command to client and keep *this* pushed runspace as
                    // the configured JEA restricted session.
                    _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.PopRunspace);
                    _serverDriverRemoteHost.PopRunspace();
    /// The remote host class for the ServerRunspacePoolDriver.
    internal class ServerDriverRemoteHost : ServerRemoteHost
        private RemoteRunspace _pushedRunspace;
        private ServerRemoteDebugger _debugger;
        private bool _hostSupportsPSEdit;
        internal ServerDriverRemoteHost(
            AbstractServerSessionTransportManager transportManager,
            ServerRemoteDebugger debugger)
            : base(clientRunspacePoolId, clientPowerShellId, hostInfo, transportManager, null, null)
            _debugger = debugger;
        /// True if runspace is pushed.
        public override bool IsRunspacePushed
                return (_pushedRunspace != null);
        /// Push runspace to use for remote command execution.
        /// <param name="runspace">RemoteRunspace.</param>
        public override void PushRunspace(Runspace runspace)
            if (_debugger == null)
                throw new PSInvalidOperationException(RemotingErrorIdStrings.ServerDriverRemoteHostNoDebuggerToPush);
            if (_pushedRunspace != null)
                throw new PSInvalidOperationException(RemotingErrorIdStrings.ServerDriverRemoteHostAlreadyPushed);
                throw new PSInvalidOperationException(RemotingErrorIdStrings.ServerDriverRemoteHostNotRemoteRunspace);
            // PSEdit support.  Existence of RemoteSessionOpenFileEvent event indicates host supports PSEdit
            _hostSupportsPSEdit = false;
            PSEventManager localEventManager = Runspace?.Events;
            _hostSupportsPSEdit = localEventManager != null && localEventManager.GetEventSubscribers(HostUtilities.RemoteSessionOpenFileEvent).GetEnumerator().MoveNext();
            if (_hostSupportsPSEdit)
                AddPSEditForRunspace(remoteRunspace);
            _debugger.PushDebugger(runspace.Debugger);
            _pushedRunspace = remoteRunspace;
        public override void PopRunspace()
                _debugger?.PopDebugger();
                    RemovePSEditFromRunspace(_pushedRunspace);
                if (_pushedRunspace.ShouldCloseOnPop)
                    _pushedRunspace.Close();
                _pushedRunspace = null;
        /// Server Debugger.
        internal Debugger ServerDebugger
            set { _debugger = value as ServerRemoteDebugger; }
        /// Pushed runspace or null.
        internal Runspace PushedRunspace
            get { return _pushedRunspace; }
        /// When true will propagate pop call to client after popping runspace from this
        /// host.  Used for OutOfProc remote sessions in a restricted (pushed) remote runspace,
        /// where a pop (exit session) should occur.
        internal bool PropagatePop
        #region PSEdit Support for ISE Host
        private void AddPSEditForRunspace(RemoteRunspace remoteRunspace)
            if (remoteRunspace.Events == null)
            // Add event handler.
            remoteRunspace.Events.ReceivedEvents.PSEventReceived += HandleRemoteSessionForwardedEvent;
            // Add script function.
            using (PowerShell powershell = PowerShell.Create())
                powershell.Runspace = remoteRunspace;
                powershell.AddScript(HostUtilities.CreatePSEditFunction).AddParameter("PSEditFunction", HostUtilities.PSEditFunction);
                    powershell.Invoke();
                catch (RemoteException) { }
        private void RemovePSEditFromRunspace(RemoteRunspace remoteRunspace)
            // It is possible for the popped runspace to be in a bad state after an error.
            if ((remoteRunspace.RunspaceStateInfo.State != RunspaceState.Opened) || (remoteRunspace.RunspaceAvailability != RunspaceAvailability.Available))
            // Remove event handler.
            remoteRunspace.Events.ReceivedEvents.PSEventReceived -= HandleRemoteSessionForwardedEvent;
            // Remove script function.
                powershell.AddScript(HostUtilities.RemovePSEditFunction);
        private void HandleRemoteSessionForwardedEvent(object sender, PSEventArgs args)
            if ((Runspace == null) || (Runspace.Events == null))
            // Forward events from nested pushed session to parent session.
                Runspace.Events.GenerateEvent(
                    sourceIdentifier: args.SourceIdentifier,
                    args: args.SourceArgs,
                    extraData: null);

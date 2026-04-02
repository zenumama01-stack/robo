    /// Remote runspace which will be created on the client side. This
    /// runspace is wrapped on a RunspacePool(1).
    internal class RemoteRunspace : Runspace, IDisposable
        private readonly List<RemotePipeline> _runningPipelines = new List<RemotePipeline>();
        private readonly bool _bSessionStateProxyCallInProgress = false;
        private readonly RunspaceConnectionInfo _connectionInfo;
        private RemoteDebugger _remoteDebugger;
        // the following two variables have been added for supporting
        // the Invoke-Command | Invoke-Command scenario
        private InvokeCommandCommand _currentInvokeCommand = null;
        private long _currentLocalPipelineId = 0;
        protected class RunspaceEventQueueItem
        private bool _bypassRunspaceStateCheck;
        protected bool ByPassRunspaceStateCheck
                return _bypassRunspaceStateCheck;
                _bypassRunspaceStateCheck = value;
        /// Temporary place to remember whether to close this runspace on pop or not.
        /// Used by Start-PSSession.
        internal bool ShouldCloseOnPop { get; set; } = false;
        /// Construct a remote runspace based on the connection information
        /// and the specified host.
        /// <param name="connectionInfo">connection information which identifies
        /// the remote computer</param>
        /// <param name="host">Host on the client.</param>
        /// <param name="name">Friendly name for remote runspace session.</param>
        /// <param name="id">Id for remote runspace.</param>
        internal RemoteRunspace(TypeTable typeTable, RunspaceConnectionInfo connectionInfo, PSHost host, PSPrimitiveDictionary applicationArguments, string name = null, int id = -1)
            PSEtwLog.LogOperationalVerbose(PSEventId.RunspaceConstructor, PSOpcode.Constructor,
                        PSTask.CreateRunspace, PSKeyword.UseAlwaysOperational,
                        InstanceId.ToString());
            OriginalConnectionInfo = connectionInfo.Clone();
            RunspacePool = new RunspacePool(1, 1, typeTable, host, applicationArguments, connectionInfo, name);
            this.PSSessionId = id;
            SetEventHandlers();
        /// Constructs a RemoteRunspace object based on the passed in RunspacePool object,
        /// with a starting state of Disconnected.
        internal RemoteRunspace(RunspacePool runspacePool)
            // The RemoteRunspace object can only be constructed this way with a RunspacePool that
            // is in the disconnected state.
            if (runspacePool.RunspacePoolStateInfo.State != RunspacePoolState.Disconnected
                || runspacePool.ConnectionInfo is not WSManConnectionInfo)
                throw PSTraceSource.NewInvalidOperationException(RunspaceStrings.InvalidRunspacePool);
            RunspacePool = runspacePool;
            // The remote runspace pool object can only have the value one set for min/max pools.
            // This sets the runspace pool object min/max pool values to one.  The PSRP/WSMan stack
            // will fail during connection if the min/max pool values do not match.
            RunspacePool.RemoteRunspacePoolInternal.SetMinRunspaces(1);
            RunspacePool.RemoteRunspacePoolInternal.SetMaxRunspaces(1);
            _connectionInfo = runspacePool.ConnectionInfo.Clone();
            // Update runspace DisconnectedOn and ExpiresOn property from WSManConnectionInfo
            UpdateDisconnectExpiresOn();
            // Initial state must be Disconnected.
            SetRunspaceState(RunspaceState.Disconnected, null);
            // Normal Availability for a disconnected runspace is "None", which means it can be connected.
            // However, we can also have disconnected runspace objects that are *not* available for
            // connection and in this case the Availability is set to "Busy".
            _runspaceAvailability = RunspacePool.RemoteRunspacePoolInternal.AvailableForConnection ?
                Runspaces.RunspaceAvailability.None : Runspaces.RunspaceAvailability.Busy;
                        this.InstanceId.ToString());
        /// Helper function to set event handlers.
        private void SetEventHandlers()
            // RemoteRunspace must have the same instanceID as its contained RunspacePool instance because
            // the PSRP/WinRS layer tracks remote runspace Ids.
            this.InstanceId = RunspacePool.InstanceId;
            _eventManager = new PSRemoteEventManager(_connectionInfo.ComputerName, this.InstanceId);
            RunspacePool.StateChanged += HandleRunspacePoolStateChanged;
            RunspacePool.RemoteRunspacePoolInternal.HostCallReceived += HandleHostCallReceived;
            RunspacePool.RemoteRunspacePoolInternal.URIRedirectionReported += HandleURIDirectionReported;
            RunspacePool.ForwardEvent += HandleRunspacePoolForwardEvent;
            RunspacePool.RemoteRunspacePoolInternal.SessionCreateCompleted += HandleSessionCreateCompleted;
        /// Initialsessionstate information for this runspace.
        public override InitialSessionState InitialSessionState
        /// PS Version running on server.
        internal Version ServerVersion { get; private set; }
                    if (value != _createThreadOptions)
        /// Connection information to this runspace.
        public override RunspaceConnectionInfo OriginalConnectionInfo { get; }
        private PSRemoteEventManager _eventManager;
        /// Gets the execution context for this runspace.
                return false; // nested prompts are not supported on remote runspaces
        /// Gets the client remote session associated with this
        /// <remarks>This member is actually not required
        /// for the product code. However, there are
        /// existing transport manager tests which depend on
        /// the same. Once transport manager is modified,
        /// this needs to be removed</remarks>
        internal ClientRemoteSession ClientRemoteSession
                    return RunspacePool.RemoteRunspacePoolInternal.DataStructureHandler.RemoteSession;
                catch (InvalidRunspacePoolStateException e)
                    throw e.ToInvalidRunspaceStateException();
        /// Gets command information on a currently running remote command.
        /// If no command is running then null is returned.
        internal ConnectCommandInfo RemoteCommand
                if (RunspacePool.RemoteRunspacePoolInternal.ConnectCommands == null)
                Dbg.Assert(RunspacePool.RemoteRunspacePoolInternal.ConnectCommands.Length < 2, "RemoteRunspace should have no more than one remote running command.");
                if (RunspacePool.RemoteRunspacePoolInternal.ConnectCommands.Length > 0)
                    return RunspacePool.RemoteRunspacePoolInternal.ConnectCommands[0];
        /// Gets friendly name for the remote PSSession.
        internal string PSSessionName
            get { return RunspacePool.RemoteRunspacePoolInternal.Name; }
            set { RunspacePool.RemoteRunspacePoolInternal.Name = value; }
        /// Gets the Id value for the remote PSSession.
        internal int PSSessionId { get; set; } = -1;
            get { return RunspacePool.RemoteRunspacePoolInternal.CanDisconnect; }
        /// Returns true if Runspace can be connected.
        internal bool CanConnect
            get { return RunspacePool.RemoteRunspacePoolInternal.AvailableForConnection; }
        /// This is used to indicate a special loopback remote session used for JEA restrictions.
        internal bool IsConfiguredLoopBack
                return _remoteDebugger;
                RunspacePool.BeginOpen(null, null);
                RunspacePool.ThreadOptions = this.ThreadOptions;
                RunspacePool.ApartmentState = this.ApartmentState;
                RunspacePool.Open();
        #endregion Open
        #region Close
                RunspacePool.BeginClose(null, null);
                IAsyncResult result = RunspacePool.BeginClose(null, null);
                // It is possible for the result ASyncResult object to be null if the runspace
                // pool is already being closed from a server initiated close event.
                    RunspacePool.EndClose(result);
        /// Dispose this runspace.
        /// <param name="disposing">True if called from Dispose.</param>
                        // If the WinRM listener has been removed before the runspace is closed, then calling
                        // Close() will cause a PSRemotingTransportException.  We don't want this exception
                        // surfaced.  Most developers don't expect an exception from calling Dispose.
                        // See [Windows 8 Bugs] 968184.
                    // Release RunspacePool event forwarding handlers.
                    _remoteDebugger?.Dispose();
                        RunspacePool.StateChanged -= HandleRunspacePoolStateChanged;
                        RunspacePool.RemoteRunspacePoolInternal.HostCallReceived -= HandleHostCallReceived;
                        RunspacePool.RemoteRunspacePoolInternal.URIRedirectionReported -= HandleURIDirectionReported;
                        RunspacePool.ForwardEvent -= HandleRunspacePoolForwardEvent;
                        RunspacePool.RemoteRunspacePoolInternal.SessionCreateCompleted -= HandleSessionCreateCompleted;
                        _eventManager = null;
                        RunspacePool.Dispose();
                        // _runspacePool = null;
        #endregion Close
        #region Reset Runspace State
        /// <exception cref="PSInvalidOperationException">
        /// Thrown when runspace is not in proper state or availability or if the
        /// reset operation fails in the remote session.
            if (this.RunspaceStateInfo.State != Runspaces.RunspaceState.Opened)
                        RunspaceStrings.RunspaceNotInOpenedState, this.RunspaceStateInfo.State);
                bool success = RunspacePool.RemoteRunspacePoolInternal.ResetRunspaceState();
        internal static Runspace[] GetRemoteRunspaces(RunspaceConnectionInfo connectionInfo, PSHost host, TypeTable typeTable)
            List<Runspace> runspaces = new List<Runspace>();
            RunspacePool[] runspacePools = RemoteRunspacePoolInternal.GetRemoteRunspacePools(connectionInfo, host, typeTable);
            // We don't yet know how many runspaces there are in these runspace pool objects.  This information isn't updated
            // until a Connect() is performed.  But we can use the ConnectCommands list to prune runspace pool objects that
            // clearly have more than one command/runspace.
            foreach (RunspacePool runspacePool in runspacePools)
                if (runspacePool.RemoteRunspacePoolInternal.ConnectCommands.Length < 2)
                    runspaces.Add(new RemoteRunspace(runspacePool));
            return runspaces.ToArray();
        /// Creates a single disconnected remote Runspace object based on connection information and
        /// session / command identifiers.
        /// <param name="connectionInfo">Connection object for target machine.</param>
        /// <param name="sessionId">Session Id to connect to.</param>
        /// <param name="commandId">Optional command Id to connect to.</param>
        /// <param name="host">Optional PSHost.</param>
        /// <param name="typeTable">Optional TypeTable.</param>
        /// <returns>Disconnect remote Runspace object.</returns>
        internal static Runspace GetRemoteRunspace(RunspaceConnectionInfo connectionInfo, Guid sessionId, Guid? commandId, PSHost host, TypeTable typeTable)
            RunspacePool runspacePool = RemoteRunspacePoolInternal.GetRemoteRunspacePool(
                commandId,
                typeTable);
            return new RemoteRunspace(runspacePool);
                throw PSTraceSource.NewInvalidOperationException(RunspaceStrings.DisconnectNotSupportedOnServer);
            UpdatePoolDisconnectOptions();
                RunspacePool.Disconnect();
                RunspacePool.BeginDisconnect(null, null);
            if (!CanConnect)
                throw PSTraceSource.NewInvalidOperationException(RunspaceStrings.CannotConnect);
                RunspacePool.Connect();
                RunspacePool.BeginConnect(null, null);
            if (RemoteCommand == null)
                throw PSTraceSource.NewInvalidOperationException(RunspaceStrings.NoDisconnectedCommand);
            return new RemotePipeline(this);
            return new PowerShell(RemoteCommand, this);
            RunspaceCapability returnCaps = RunspaceCapability.Default;
                returnCaps |= RunspaceCapability.SupportsDisconnect;
            if (_connectionInfo is WSManConnectionInfo)
            if (_connectionInfo is NamedPipeConnectionInfo)
                returnCaps |= RunspaceCapability.NamedPipeTransport;
            else if (_connectionInfo is VMConnectionInfo)
                returnCaps |= RunspaceCapability.VMSocketTransport;
            else if (_connectionInfo is SSHConnectionInfo)
                returnCaps |= RunspaceCapability.SSHTransport;
            else if (_connectionInfo is ContainerConnectionInfo containerConnectionInfo)
                if ((containerConnectionInfo != null) &&
                    (containerConnectionInfo.ContainerProc.RuntimeId == Guid.Empty))
                // Unknown connection info type means a custom connection/transport, which at
                // minimum supports remote runspace capability starting from PowerShell v7.x.
                returnCaps |= RunspaceCapability.CustomTransport;
        /// Update the pool disconnect options so that any changes will be
        /// passed to the server during the disconnect/connect operations.
        private void UpdatePoolDisconnectOptions()
            WSManConnectionInfo runspaceWSManConnectionInfo = RunspacePool.ConnectionInfo as WSManConnectionInfo;
            WSManConnectionInfo wsManConnectionInfo = ConnectionInfo as WSManConnectionInfo;
            Dbg.Assert(runspaceWSManConnectionInfo != null, "Disconnect-Connect feature is currently only supported for WSMan transport");
            Dbg.Assert(wsManConnectionInfo != null, "Disconnect-Connect feature is currently only supported for WSMan transport");
            runspaceWSManConnectionInfo.IdleTimeout = wsManConnectionInfo.IdleTimeout;
            runspaceWSManConnectionInfo.OutputBufferingMode = wsManConnectionInfo.OutputBufferingMode;
        #region Remote Debugging
        /// Remote DebuggerStop event.
        internal event EventHandler<PSEventArgs> RemoteDebuggerStop;
        /// Remote BreakpointUpdated event.
        internal event EventHandler<PSEventArgs> RemoteDebuggerBreakpointUpdated;
        /// <exception cref="PSNotSupportedException">Not supported in remoting
        /// scenarios</exception>
        #region Running Pipeline Management
        internal void AddToRunningPipelineList(RemotePipeline pipeline)
                if (!_bypassRunspaceStateCheck &&
                    _runspaceStateInfo.State != RunspaceState.Opened &&
                    _runspaceStateInfo.State != RunspaceState.Disconnected) // Disconnected runspaces can have running pipelines.
                            StringUtil.Format(RunspaceStrings.RunspaceNotOpenForPipeline,
                                _runspaceStateInfo.State.ToString()
                            _runspaceStateInfo.State,
                    if (this.ConnectionInfo != null)
                        e.Source = this.ConnectionInfo.ComputerName;
                _runningPipelines.Add(pipeline);
        internal void RemoveFromRunningPipelineList(RemotePipeline pipeline)
                Dbg.Assert(_runspaceStateInfo.State != RunspaceState.BeforeOpen,
                _runningPipelines.Remove(pipeline);
        /// Check to see, if there is any other pipeline running in this
        /// runspace. If not, then add this to the list of pipelines.
        /// <param name="pipeline">Pipeline to check and add.</param>
        /// <param name="syncCall">whether this is being called from
        /// a synchronous method call</param>
        internal void DoConcurrentCheckAndAddToRunningPipelines(RemotePipeline pipeline, bool syncCall)
                pipeline.DoConcurrentCheck(syncCall);
        #endregion Running Pipeline Management
        #region SessionState Proxy
            return _sessionStateProxy ??= new RemoteSessionStateProxy(this);
        private RemoteSessionStateProxy _sessionStateProxy = null;
        #endregion SessionState Proxy
        private void HandleRunspacePoolStateChanged(object sender, RunspacePoolStateChangedEventArgs e)
            RunspaceState newState = (RunspaceState)e.RunspacePoolStateInfo.State;
            RunspaceState prevState = SetRunspaceState(newState, e.RunspacePoolStateInfo.Reason);
            switch (newState)
                            // For newly opened remote runspaces, set the debug mode based on the
                            // associated host.  This involves running a remote command and is Ok
                            // since this event is called on a worker thread and not a WinRM callback.
                            SetDebugModeOnOpen();
                        case RunspaceState.Connecting:
                            // Application private data containing server debug state is updated on
                            // a *reconstruct* connect operation when _applicationPrivateData is null.
                            // Pass new information to the debugger.
                                _applicationPrivateData = GetApplicationPrivateData();
                                SetDebugInfo(_applicationPrivateData);
        /// Set debug mode on remote session based on the interactive host
        /// setting, if available.
        private void SetDebugModeOnOpen()
            // Update client remote debugger based on server capabilities.
            bool serverSupportsDebugging = SetDebugInfo(_applicationPrivateData);
            if (!serverSupportsDebugging) { return; }
            // Set server side initial debug mode based on interactive host.
            DebugModes hostDebugMode = DebugModes.Default;
                IHostSupportsInteractiveSession interactiveHost =
                    RunspacePool.RemoteRunspacePoolInternal.Host as IHostSupportsInteractiveSession;
                if (interactiveHost != null &&
                    interactiveHost.Runspace != null &&
                    interactiveHost.Runspace.Debugger != null)
                    hostDebugMode = interactiveHost.Runspace.Debugger.DebugMode;
            if ((hostDebugMode & DebugModes.RemoteScript) == DebugModes.RemoteScript)
                    _remoteDebugger.SetDebugMode(hostDebugMode);
        private bool SetDebugInfo(PSPrimitiveDictionary psApplicationPrivateData)
            DebugModes? debugMode = null;
            bool inDebugger = false;
            int breakpointCount = 0;
            bool breakAll = false;
            UnhandledBreakpointProcessingMode unhandledBreakpointMode = UnhandledBreakpointProcessingMode.Ignore;
            if (psApplicationPrivateData != null)
                if (psApplicationPrivateData.ContainsKey(RemoteDebugger.DebugModeSetting))
                    debugMode = (DebugModes)(int)psApplicationPrivateData[RemoteDebugger.DebugModeSetting];
                if (psApplicationPrivateData.ContainsKey(RemoteDebugger.DebugStopState))
                    inDebugger = (bool)psApplicationPrivateData[RemoteDebugger.DebugStopState];
                if (psApplicationPrivateData.ContainsKey(RemoteDebugger.DebugBreakpointCount))
                    breakpointCount = (int)psApplicationPrivateData[RemoteDebugger.DebugBreakpointCount];
                if (psApplicationPrivateData.ContainsKey(RemoteDebugger.BreakAllSetting))
                    breakAll = (bool)psApplicationPrivateData[RemoteDebugger.BreakAllSetting];
                if (psApplicationPrivateData.ContainsKey(RemoteDebugger.UnhandledBreakpointModeSetting))
                    unhandledBreakpointMode = (UnhandledBreakpointProcessingMode)(int)psApplicationPrivateData[RemoteDebugger.UnhandledBreakpointModeSetting];
                if (psApplicationPrivateData.ContainsKey(PSVersionInfo.PSVersionTableName))
                    var psVersionTable = psApplicationPrivateData[PSVersionInfo.PSVersionTableName] as PSPrimitiveDictionary;
                    if (psVersionTable.ContainsKey(PSVersionInfo.PSVersionName))
                        ServerVersion = PSObject.Base(psVersionTable[PSVersionInfo.PSVersionName]) as Version;
            if (debugMode != null)
                // Server supports remote debugging.  Create Debugger object for
                // this remote runspace.
                Dbg.Assert(_remoteDebugger == null, "Remote runspace should not have a debugger yet.");
                _remoteDebugger = new RemoteDebugger(this);
                // Set initial debugger state.
                _remoteDebugger.SetClientDebugInfo(debugMode, inDebugger, breakpointCount, breakAll, unhandledBreakpointMode, ServerVersion);
        /// Asserts if the current state of the runspace is BeforeOpen.
        private void AssertIfStateIsBeforeOpen()
                if (_runspaceStateInfo.State != RunspaceState.BeforeOpen)
                            StringUtil.Format(RunspaceStrings.CannotOpenAgain,
                                new object[] { _runspaceStateInfo.State.ToString() }
        /// <returns>Previous runspace state.</returns>
        private RunspaceState SetRunspaceState(RunspaceState state, Exception reason)
            RunspaceState prevState;
                prevState = _runspaceStateInfo.State;
                if (state != prevState)
                    PSEtwLog.LogOperationalVerbose(PSEventId.RunspaceStateChange, PSOpcode.Open,
                                state.ToString());
            return prevState;
        private void RaiseRunspaceStateEvents()
        /// Creates a pipeline.
        private Pipeline CoreCreatePipeline(string command, bool addToHistory, bool isNested)
            return new RemotePipeline(this, command, addToHistory, isNested);
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Finishof")]
        private bool WaitForFinishofPipelines()
            RemotePipeline[] runningPipelines;
                runningPipelines = _runningPipelines.ToArray();
                if (_runningPipelines.Count != 0)
                    return (Pipeline)_runningPipelines[_runningPipelines.Count - 1];
        /// Handles any host calls received from the server.
        /// <param name="eventArgs">arguments describing this event, contains
        /// a RemoteHostCall object</param>
                RunspacePool.RemoteRunspacePoolInternal.DataStructureHandler.TransportManager,
                RunspacePool.RemoteRunspacePoolInternal.Host,
                null,       /* error stream */
                null,       /* method executor stream */
                false,      /* is method stream enabled */
                RunspacePool.RemoteRunspacePoolInternal,
                Guid.Empty, /* powershell id */
                // change the runspace's uri to the new URI.
        /// Forward the events from the runspace pool to the current instance.
        private void HandleRunspacePoolForwardEvent(object sender, PSEventArgs e)
            if (e.SourceIdentifier.Equals(RemoteDebugger.RemoteDebuggerStopEvent))
                // Special processing for forwarded remote DebuggerStop event.
                RemoteDebuggerStop.SafeInvoke(this, e);
            else if (e.SourceIdentifier.Equals(RemoteDebugger.RemoteDebuggerBreakpointUpdatedEvent))
                // Special processing for forwarded remote DebuggerBreakpointUpdated event.
                RemoteDebuggerBreakpointUpdated.SafeInvoke(this, e);
                _eventManager.AddForwardedEvent(e);
        /// Updates runspace DisconnectedOn/ExpiresOn based on RS Pool connectionInfo.
        private void UpdateDisconnectExpiresOn()
            WSManConnectionInfo wsmanConnectionInfo = RunspacePool.RemoteRunspacePoolInternal.ConnectionInfo as WSManConnectionInfo;
                this.DisconnectedOn = wsmanConnectionInfo.DisconnectedOn;
                this.ExpiresOn = wsmanConnectionInfo.ExpiresOn;
        /// Determines if another Invoke-Command is executing
        /// in this runspace in the currently running local pipeline
        /// ahead on the specified invoke-command.
        /// <param name="invokeCommand">current invoke-command
        /// instance</param>
        /// <param name="localPipelineId">Local pipeline id.</param>
        /// <returns>True, if another invoke-command is running
        /// before, false otherwise.</returns>
        internal bool IsAnotherInvokeCommandExecuting(InvokeCommandCommand invokeCommand,
            long localPipelineId)
            // the invoke-command's pipeline should be the currently
            // running pipeline. This will ensure that, we do not
            // return true, when one invoke-command is running as a
            // job and another invoke-command is entered at the
            // console prompt
            if (_currentLocalPipelineId != localPipelineId && _currentLocalPipelineId != 0)
                // the local pipeline ids are the same
                // this invoke command is running may be
                // running in the same pipeline as another
                // invoke command
                if (_currentInvokeCommand == null)
                    // this is the first invoke-command, just
                    // set the reference
                    SetCurrentInvokeCommand(invokeCommand, localPipelineId);
                else if (_currentInvokeCommand.Equals(invokeCommand))
                    // the currently active invoke command is the one
                    // specified
                    // the local pipeline id is the same and there
                    // is another invoke command that is active already
        /// Keeps track of the current invoke command executing
        /// within the current local pipeline.
        /// <param name="invokeCommand">reference to invoke command
        /// which is currently being processed</param>
        /// <param name="localPipelineId">The local pipeline id.</param>
        internal void SetCurrentInvokeCommand(InvokeCommandCommand invokeCommand,
            Dbg.Assert(invokeCommand != null, "InvokeCommand instance cannot be null, use ClearInvokeCommand() method to reset current command");
            Dbg.Assert(localPipelineId != 0, "Local pipeline id needs to be supplied - cannot be 0");
            _currentInvokeCommand = invokeCommand;
            _currentLocalPipelineId = localPipelineId;
        /// Clears the current invoke-command reference stored within
        /// this remote runspace.
        internal void ClearInvokeCommand()
            _currentLocalPipelineId = 0;
            _currentInvokeCommand = null;
        /// Aborts any current Opening process.  If runspace is not opening then this has no effect.
        /// This is currently *only* for named pipe connections where a connection
        /// to a process is limited to a single client.
        internal void AbortOpen()
            System.Management.Automation.Remoting.Client.NamedPipeClientSessionTransportManager transportManager =
                RunspacePool.RemoteRunspacePoolInternal.DataStructureHandler.TransportManager as System.Management.Automation.Remoting.Client.NamedPipeClientSessionTransportManager;
            transportManager?.AbortConnect();
        #region Misc Properties / Events
        /// The runspace pool that this remote runspace wraps.
        internal RunspacePool RunspacePool { get; }
        #endregion Misc Properties
        #region Application private data
        /// Remote runspace gets its application private data from the server (when creating the remote runspace pool)
                return RunspacePool.GetApplicationPrivateData();
            Dbg.Assert(false, "RemoteRunspace.SetApplicationPrivateData shouldn't be called - this runspace does not belong to a runspace pool [although it does use a remote runspace pool internally]");
    #region Remote Debugger
    /// RemoteDebugger.
    internal sealed class RemoteDebugger : Debugger, IDisposable
        private readonly RemoteRunspace _runspace;
        private bool _remoteDebugSupported;
        private int _breakpointCount;
        private RemoteDebuggingCapability _remoteDebuggingCapability;
        private bool? _remoteBreakpointManagementIsSupported;
        private volatile bool _handleDebuggerStop;
        private bool _isDebuggerSteppingEnabled;
        private UnhandledBreakpointProcessingMode _unhandledBreakpointMode;
        private bool _detachCommand;
        // Windows impersonation flow
        private WindowsIdentity _identityToPersonate;
        private bool _identityPersonationChecked;
        /// RemoteDebuggerStopEvent.
        public const string RemoteDebuggerStopEvent = "PSInternalRemoteDebuggerStopEvent";
        /// RemoteDebuggerBreakpointUpdatedEvent.
        public const string RemoteDebuggerBreakpointUpdatedEvent = "PSInternalRemoteDebuggerBreakpointUpdatedEvent";
        // Remote debugger settings
        public const string DebugModeSetting = "DebugMode";
        public const string DebugStopState = "DebugStop";
        public const string DebugBreakpointCount = "DebugBreakpointCount";
        public const string BreakAllSetting = "BreakAll";
        public const string UnhandledBreakpointModeSetting = "UnhandledBreakpointMode";
        private RemoteDebugger() { }
        /// <param name="runspace">Associated remote runspace.</param>
        public RemoteDebugger(RemoteRunspace runspace)
            _unhandledBreakpointMode = UnhandledBreakpointProcessingMode.Ignore;
            // Hook up remote debugger forwarded event handlers.
            _runspace.RemoteDebuggerStop += HandleForwardedDebuggerStopEvent;
            _runspace.RemoteDebuggerBreakpointUpdated += HandleForwardedDebuggerBreakpointUpdatedEvent;
        #region Class overrides
        /// Process debugger command.
        /// <param name="command">Debugger PSCommand.</param>
            CheckForValidateState();
            _detachCommand = false;
            // Execute command on server.
            bool executionError = false;
            using (_psDebuggerCommand = GetNestedPowerShell())
                foreach (var cmd in command.Commands)
                    _psDebuggerCommand.AddCommand(cmd);
                            if (item == null) { return; }
                            else if (item.BaseObject is DebuggerCommandResults)
                                results = item.BaseObject as DebuggerCommandResults;
                    _psDebuggerCommand.Invoke(null, internalOutput, null);
                    executionError = true;
                    RemoteException re = e as RemoteException;
                    if ((re != null) && (re.ErrorRecord != null))
                        // Allow the IncompleteParseException to throw so that the console
                        // can handle here strings and continued parsing.
                        if (re.ErrorRecord.CategoryInfo.Reason == nameof(IncompleteParseException))
                            throw new IncompleteParseException(
                                re.ErrorRecord.Exception?.Message,
                                re.ErrorRecord.FullyQualifiedErrorId);
                        // Allow the RemoteException and InvalidRunspacePoolStateException to propagate so that the host can
                        // clean up the debug session.
                        if ((re.ErrorRecord.CategoryInfo.Reason == nameof(InvalidRunspacePoolStateException)) ||
                            (re.ErrorRecord.CategoryInfo.Reason == nameof(RemoteException)))
                            throw new PSRemotingTransportException(
                                (re.ErrorRecord.Exception != null) ? re.ErrorRecord.Exception.Message : string.Empty);
                    // Allow all PSRemotingTransportException and RemoteException errors to propagate as this
                    // indicates a broken debug session.
                    if ((e is PSRemotingTransportException) || (e is RemoteException))
                    output.Add(
            executionError = executionError || _psDebuggerCommand.HadErrors;
            // Special processing when the detach command is run.
            _detachCommand = (!executionError) && (command.Commands.Count > 0) && (command.Commands[0].CommandText.Equals("Detach", StringComparison.OrdinalIgnoreCase));
            if ((ps != null) &&
                (ps.InvocationStateInfo.State == PSInvocationState.Running))
                ps.BeginStop(null, null);
            // This is supported only for PowerShell versions >= 7.0
            CheckRemoteBreakpointManagementSupport(RemoteDebuggingCommands.SetBreakpoint);
            var functionParameters = new Dictionary<string, object>
                { "BreakpointList", breakpoints },
                functionParameters.Add("RunspaceId", runspaceId.Value);
            InvokeRemoteBreakpointFunction<CommandBreakpoint>(RemoteDebuggingCommands.SetBreakpoint, functionParameters);
            CheckRemoteBreakpointManagementSupport(RemoteDebuggingCommands.GetBreakpoint);
                { "Id", id },
            return InvokeRemoteBreakpointFunction<Breakpoint>(RemoteDebuggingCommands.GetBreakpoint, functionParameters);
            var breakpoints = new List<Breakpoint>();
            using (PowerShell ps = GetNestedPowerShell())
                ps.AddCommand(RemoteDebuggingCommands.GetBreakpoint);
                    ps.AddParameter("RunspaceId", runspaceId.Value);
                Collection<PSObject> output = ps.Invoke<PSObject>();
                foreach (var item in output)
                    if (item?.BaseObject is Breakpoint bp)
                    else if (TryGetRemoteDebuggerException(item, out Exception ex))
            return breakpoints;
            Breakpoint breakpoint = new CommandBreakpoint(path, null, command, action);
                { "Breakpoint", breakpoint },
            return InvokeRemoteBreakpointFunction<CommandBreakpoint>(RemoteDebuggingCommands.SetBreakpoint, functionParameters);
            Breakpoint breakpoint = new LineBreakpoint(path, line, column, action);
            return InvokeRemoteBreakpointFunction<LineBreakpoint>(RemoteDebuggingCommands.SetBreakpoint, functionParameters);
            Breakpoint breakpoint = new VariableBreakpoint(path, variableName, accessMode, action);
            return InvokeRemoteBreakpointFunction<VariableBreakpoint>(RemoteDebuggingCommands.SetBreakpoint, functionParameters);
            CheckRemoteBreakpointManagementSupport(RemoteDebuggingCommands.RemoveBreakpoint);
                { "Id", breakpoint.Id },
            return InvokeRemoteBreakpointFunction<bool>(RemoteDebuggingCommands.RemoveBreakpoint, functionParameters);
            CheckRemoteBreakpointManagementSupport(RemoteDebuggingCommands.EnableBreakpoint);
            return InvokeRemoteBreakpointFunction<Breakpoint>(RemoteDebuggingCommands.EnableBreakpoint, functionParameters);
            CheckRemoteBreakpointManagementSupport(RemoteDebuggingCommands.DisableBreakpoint);
            return InvokeRemoteBreakpointFunction<Breakpoint>(RemoteDebuggingCommands.DisableBreakpoint, functionParameters);
            SetRemoteDebug(false, RunspaceAvailability.Busy);
                ps.AddCommand(RemoteDebuggingCommands.SetDebuggerAction).AddParameter("ResumeAction", resumeAction);
                // If an error exception is returned then throw it here.
                if (ps.ErrorBuffer.Count > 0)
                    Exception e = ps.ErrorBuffer[0].Exception;
                    if (e != null) { throw e; }
            DebuggerStopEventArgs rtnArgs = null;
                    ps.AddCommand(RemoteDebuggingCommands.GetDebuggerStopArgs);
                        rtnArgs = item.BaseObject as DebuggerStopEventArgs;
                        if (rtnArgs != null) { break; }
        /// SetDebugMode.
            // Only set debug mode on server if no commands are currently
            // running on remote runspace.
            if ((_runspace.GetCurrentlyRunningPipeline() != null) ||
                (_runspace.RemoteCommand != null))
                ps.SetIsNested(false);
                ps.AddCommand(RemoteDebuggingCommands.SetDebugMode).AddParameter("Mode", mode);
            SetIsActive(_breakpointCount);
            // This is supported only for PowerShell versions >= 5.0
            if (!_remoteDebuggingCapability.IsCommandSupported(RemoteDebuggingCommands.SetDebuggerStepMode))
                // Ensure debugger is in correct mode.
                base.SetDebugMode(DebugModes.LocalScript | DebugModes.RemoteScript);
                // Send Enable-DebuggerStepping virtual command.
                    ps.AddCommand(RemoteDebuggingCommands.SetDebuggerStepMode).AddParameter("Enabled", enabled);
                    _isDebuggerSteppingEnabled = enabled;
                // Don't propagate exceptions.
            get { return _isActive; }
                return _handleDebuggerStop || (_runspace.RunspaceAvailability == RunspaceAvailability.RemoteDebug);
        /// InternalProcessCommand.
                return _unhandledBreakpointMode;
                if (!_remoteDebuggingCapability.IsCommandSupported(RemoteDebuggingCommands.SetUnhandledBreakpointMode))
                SetRemoteDebug(false, (RunspaceAvailability?)null);
                // Send Set-PSUnhandledBreakpointMode virtual command.
                    ps.AddCommand(RemoteDebuggingCommands.SetUnhandledBreakpointMode).AddParameter("UnhandledBreakpointMode", value);
                _unhandledBreakpointMode = value;
            _runspace.RemoteDebuggerStop -= HandleForwardedDebuggerStopEvent;
            _runspace.RemoteDebuggerBreakpointUpdated -= HandleForwardedDebuggerBreakpointUpdatedEvent;
            if (_identityToPersonate != null)
                _identityToPersonate.Dispose();
                _identityToPersonate = null;
        /// Internal method that checks the debug state of
        /// the remote session and raises the DebuggerStop event
        /// if debugger is in stopped state.
        /// This is used internally to help clients get back to
        /// debug state when reconnecting to remote session in debug state.
            DebuggerStopEventArgs stopArgs = GetDebuggerStopArgs();
            if (stopArgs != null)
                ProcessDebuggerStopEvent(stopArgs);
        /// IsRemoteDebug.
        internal bool IsRemoteDebug
        /// Sets client debug info state based on server info.
        /// <param name="debugMode">Debug mode.</param>
        /// <param name="inBreakpoint">Currently in breakpoint.</param>
        /// <param name="breakpointCount">Breakpoint count.</param>
        /// <param name="breakAll">Break All setting.</param>
        /// <param name="unhandledBreakpointMode">UnhandledBreakpointMode.</param>
        /// <param name="serverPSVersion">Server PowerShell version.</param>
        internal void SetClientDebugInfo(
            DebugModes? debugMode,
            bool inBreakpoint,
            int breakpointCount,
            bool breakAll,
            UnhandledBreakpointProcessingMode unhandledBreakpointMode,
            Version serverPSVersion)
                _remoteDebugSupported = true;
                DebugMode = debugMode.Value;
                _remoteDebugSupported = false;
            if (inBreakpoint)
                SetRemoteDebug(true, RunspaceAvailability.RemoteDebug);
            _remoteDebuggingCapability = RemoteDebuggingCapability.CreateDebuggingCapability(serverPSVersion);
            _breakpointCount = breakpointCount;
            _isDebuggerSteppingEnabled = breakAll;
            _unhandledBreakpointMode = unhandledBreakpointMode;
            SetIsActive(breakpointCount);
        /// If a command is stopped while in debug stopped state and it
        /// is the only command running then server is no longer debug stopped.
        internal void OnCommandStopped()
            if (IsRemoteDebug)
                IsRemoteDebug = false;
        /// Gets breakpoint information from the target machine and passes that information
        /// on through the BreakpointUpdated event.
        internal void SendBreakpointUpdatedEvents()
            if (!IsDebuggerBreakpointUpdatedEventSubscribed() ||
                (_breakpointCount == 0))
            PSDataCollection<PSObject> breakpoints = new PSDataCollection<PSObject>();
            // Get breakpoint information by running "Get-PSBreakpoint" PowerShell command.
                if (!this.InBreakpoint)
                    // Can't use nested PowerShell if we are not stopped in a breakpoint.
                ps.AddCommand("Get-PSBreakpoint");
                ps.Invoke(null, breakpoints);
            // Raise BreakpointUpdated event to client for each breakpoint.
            foreach (PSObject obj in breakpoints)
                Breakpoint breakpoint = obj.BaseObject as Breakpoint;
                if (breakpoint != null)
                    RaiseBreakpointUpdatedEvent(
                        new BreakpointUpdatedEventArgs(breakpoint, BreakpointUpdateType.Set, _breakpointCount));
        /// IsDebuggerSteppingEnabled.
            get { return _isDebuggerSteppingEnabled; }
        private static bool TryGetRemoteDebuggerException(
            PSObject item,
            bool haveExceptionType = false;
            foreach (var typeName in item.TypeNames)
                if (typeName.Equals("Deserialized.System.Exception"))
                    haveExceptionType = true;
            if (haveExceptionType)
                var errorMessage = item.Properties["Message"]?.Value ?? string.Empty;
                exception = new RemoteException(
                        RemotingErrorIdStrings.RemoteDebuggerError, item.TypeNames[0], errorMessage));
        // Event handlers
        private void HandleForwardedDebuggerStopEvent(object sender, PSEventArgs e)
            Dbg.Assert(e.SourceArgs.Length == 1, "Forwarded debugger stop event args must always contain one SourceArgs item.");
            DebuggerStopEventArgs args;
            if (e.SourceArgs[0] is PSObject)
                args = ((PSObject)e.SourceArgs[0]).BaseObject as DebuggerStopEventArgs;
                args = e.SourceArgs[0] as DebuggerStopEventArgs;
            ProcessDebuggerStopEvent(args);
        private void ProcessDebuggerStopEvent(DebuggerStopEventArgs args)
            // It is possible to get a stop event raise request while
            // debugger is already in stop mode (after remote runspace debugger
            // reconnect).  In this case ignore the request.
            if (_handleDebuggerStop) { return; }
            // Attempt to process debugger stop event on original thread if it
            // is available (i.e., if it is blocked by EndInvoke).
            PowerShell powershell = _runspace.RunspacePool.RemoteRunspacePoolInternal.GetCurrentRunningPowerShell();
            AsyncResult invokeAsyncResult = powershell?.EndInvokeAsyncResult;
            bool invokedOnBlockedThread = false;
            if ((invokeAsyncResult != null) && (!invokeAsyncResult.IsCompleted))
                invokedOnBlockedThread = invokeAsyncResult.InvokeCallbackOnThread(
                    ProcessDebuggerStopEventProc,
            if (!invokedOnBlockedThread)
                // Otherwise run on worker thread.
                Utils.QueueWorkItemWithImpersonation(
                    _identityToPersonate,
        private void ProcessDebuggerStopEventProc(object state)
            RunspaceAvailability prevAvailability = _runspace.RunspaceAvailability;
            bool restoreAvailability = true;
                _handleDebuggerStop = true;
                // Update runspace availability
                // Raise event and wait for response.
                DebuggerStopEventArgs args = state as DebuggerStopEventArgs;
                    if (IsDebuggerStopEventSubscribed())
                            // Blocking call.
                            base.RaiseDebuggerStopEvent(args);
                            _handleDebuggerStop = false;
                            if (!_detachCommand && !args.SuspendRemote)
                                SetDebuggerAction(args.ResumeAction);
                        // If no debugger is subscribed to the DebuggerStop event then we
                        // allow the server side script execution to remain blocked in debug
                        // stop mode.  The runspace Availability reflects this and the client
                        // must take action (attach debugger or release remote debugger stop
                        // via SetDebuggerAction()).
                        restoreAvailability = false;
                    // Null arguments may indicate that remote runspace was created without
                    // default type table and so arguments cannot be serialized.  In this case
                    // we don't want to block the remote side script execution.
                    SetDebuggerAction(DebuggerResumeAction.Continue);
                // Restore runspace availability.
                if (restoreAvailability && (_runspace.RunspaceAvailability == RunspaceAvailability.RemoteDebug))
                    SetRemoteDebug(false, prevAvailability);
                if (_detachCommand)
        private void HandleForwardedDebuggerBreakpointUpdatedEvent(object sender, PSEventArgs e)
            Dbg.Assert(e.SourceArgs.Length == 1, "Forwarded debugger breakpoint event args must always contain one SourceArgs item.");
            BreakpointUpdatedEventArgs bpArgs = e.SourceArgs[0] as BreakpointUpdatedEventArgs;
            if (bpArgs != null)
                UpdateBreakpointCount(bpArgs.BreakpointCount);
                base.RaiseBreakpointUpdatedEvent(bpArgs);
        private PowerShell GetNestedPowerShell()
            ps.Runspace = _runspace;
            ps.SetIsNested(true);
        private void CheckForValidateState()
            if (!_remoteDebugSupported)
                    // The remote session to which you are connected does not support remote debugging.
                    // You must connect to a remote computer that is running PowerShell 4.0 or greater.
                    RemotingErrorIdStrings.RemoteDebuggingEndpointVersionError,
                    "RemoteDebugger:RemoteDebuggingNotSupported",
            if (_runspace.RunspaceStateInfo.State != RunspaceState.Opened)
                throw new InvalidRunspaceStateException();
            if (!_identityPersonationChecked)
                _identityPersonationChecked = true;
                // Save identity to impersonate.
                Utils.TryGetWindowsImpersonatedIdentity(out _identityToPersonate);
        private void SetRemoteDebug(bool remoteDebug, RunspaceAvailability? availability)
            if (IsRemoteDebug != remoteDebug)
                IsRemoteDebug = remoteDebug;
                _runspace.RunspacePool.RemoteRunspacePoolInternal.IsRemoteDebugStop = remoteDebug;
            if (availability != null)
                RunspaceAvailability newAvailability = availability.Value;
                if ((_runspace.RunspaceAvailability != newAvailability) &&
                    (remoteDebug || (newAvailability != RunspaceAvailability.RemoteDebug)))
                        _runspace.UpdateRunspaceAvailability(newAvailability, true);
        private void UpdateBreakpointCount(int bpCount)
            _breakpointCount = bpCount;
            SetIsActive(bpCount);
        private void SetIsActive(int breakpointCount)
            if ((DebugMode & DebugModes.RemoteScript) == 0)
                // Debugger is always inactive if RemoteScript is not selected.
                if (_isActive) { _isActive = false; }
            if (breakpointCount > 0)
                if (!_isActive) { _isActive = true; }
        private T InvokeRemoteBreakpointFunction<T>(string functionName, Dictionary<string, object> parameters)
                ps.AddCommand(functionName);
                foreach (var parameterName in parameters.Keys)
                    ps.AddParameter(parameterName, parameters[parameterName]);
                // This helper is only used to return a single output item of type T.
                    if (item?.BaseObject is T)
                        return (T)item.BaseObject;
                    if (TryGetRemoteDebuggerException(item, out Exception ex))
        private void CheckRemoteBreakpointManagementSupport(string breakpointCommandNameToCheck)
            _remoteBreakpointManagementIsSupported ??= _remoteDebuggingCapability.IsCommandSupported(breakpointCommandNameToCheck);
            if (!_remoteBreakpointManagementIsSupported.Value)
                        DebuggerStrings.CommandNotSupportedForRemoteUseInServerDebugger,
                        RemoteDebuggingCommands.CleanCommandName(breakpointCommandNameToCheck)));
    #region RemoteSessionStateProxy
    internal class RemoteSessionStateProxy : SessionStateProxy
        internal RemoteSessionStateProxy(RemoteRunspace runspace)
        private Exception _isInNoLanguageModeException = null;
        private Exception _getVariableCommandNotFoundException = null;
        private Exception _setVariableCommandNotFoundException = null;
        public override void SetVariable(string name, object value)
            // Verify the runspace has the Set-Variable command. For performance, throw if we got an error
            // before.
            if (_setVariableCommandNotFoundException != null)
                throw _setVariableCommandNotFoundException;
            // Since these are implemented as pipelines, we don't need to do our own
            // locking of sessionStateCallInProgress like we do with local runspaces.
            Pipeline remotePipeline = _runspace.CreatePipeline();
            Command command = new Command("Microsoft.PowerShell.Utility\\Set-Variable");
            command.Parameters.Add("Name", name);
            command.Parameters.Add("Value", value);
            remotePipeline.Commands.Add(command);
                remotePipeline.Invoke();
            catch (RemoteException e)
                if (string.Equals("CommandNotFoundException", e.ErrorRecord.FullyQualifiedErrorId, StringComparison.OrdinalIgnoreCase))
                    _setVariableCommandNotFoundException = new PSNotSupportedException(RunspaceStrings.NotSupportedOnRestrictedRunspace, e);
                else throw;
            if (remotePipeline.Error.Count > 0)
                // Don't cache these errors, as they are related to the actual variable being set.
                ErrorRecord error = (ErrorRecord)remotePipeline.Error.Read();
                throw new PSNotSupportedException(RunspaceStrings.NotSupportedOnRestrictedRunspace, error.Exception);
        public override object GetVariable(string name)
            // Verify the runspace has the Get-Variable command. For performance, throw if we got an error
            if (_getVariableCommandNotFoundException != null)
                throw _getVariableCommandNotFoundException;
            Command command = new Command("Microsoft.PowerShell.Utility\\Get-Variable");
            System.Collections.ObjectModel.Collection<PSObject> result = null;
                result = remotePipeline.Invoke();
                    _getVariableCommandNotFoundException = new PSNotSupportedException(RunspaceStrings.NotSupportedOnRestrictedRunspace, e);
                if (string.Equals("CommandNotFoundException", error.FullyQualifiedErrorId, StringComparison.OrdinalIgnoreCase))
                    throw new PSInvalidOperationException(error.Exception.Message, error.Exception);
            if (result.Count != 1)
                return result[0].Properties["Value"].Value;
        public override List<string> Applications
                // Verify the runspace has is not in NoLanguage mode. For performance, throw if we got an error
                if (_isInNoLanguageModeException != null)
                    throw _isInNoLanguageModeException;
                remotePipeline.Commands.AddScript("$executionContext.SessionState.Applications");
                    foreach (PSObject application in remotePipeline.Invoke())
                        result.Add(application.BaseObject as string);
                    if (e.ErrorRecord.CategoryInfo.Category == ErrorCategory.ParserError)
                        _isInNoLanguageModeException = new PSNotSupportedException(RunspaceStrings.NotSupportedOnRestrictedRunspace, e);
        public override List<string> Scripts
                remotePipeline.Commands.AddScript("$executionContext.SessionState.Scripts");
        public override DriveManagementIntrinsics Drive
        public override PSLanguageMode LanguageMode
                // Verify the runspace has is not in NoLanguage mode. For performance, return our
                // cached value if we got an error before.
                    return PSLanguageMode.NoLanguage;
                remotePipeline.Commands.AddScript("$executionContext.SessionState.LanguageMode");
                return (PSLanguageMode)LanguagePrimitives.ConvertTo(result[0], typeof(PSLanguageMode), CultureInfo.InvariantCulture);
        public override PSModuleInfo Module
        public override PathIntrinsics Path
        public override CmdletProviderManagementIntrinsics Provider
        public override PSVariableIntrinsics PSVariable
        public override CommandInvocationIntrinsics InvokeCommand
        public override ProviderIntrinsics InvokeProvider

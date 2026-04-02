    /// Class which supports pooling remote powerShell runspaces
    /// on the client.
    internal sealed class RemoteRunspacePoolInternal : RunspacePoolInternal
        /// supplied <paramref name="connectionInfo"/>, <paramref name="minRunspaces"/>
        /// <param name="host">Host associated with this runspacepool.</param>
        /// <param name="connectionInfo">The RunspaceConnectionInfo object
        /// which identifies this runspace pools connection to the server
        /// ConnectionInfo specified is null
        internal RemoteRunspacePoolInternal(int minRunspaces,
            int maxRunspaces, TypeTable typeTable, PSHost host, PSPrimitiveDictionary applicationArguments, RunspaceConnectionInfo connectionInfo, string name = null)
            : base(minRunspaces, maxRunspaces)
            if (connectionInfo == null)
                throw PSTraceSource.NewArgumentNullException("WSManConnectionInfo");
            PSEtwLog.LogOperationalVerbose(PSEventId.RunspacePoolConstructor,
                    PSOpcode.Constructor, PSTask.CreateRunspace,
                    instanceId.ToString(),
                    minPoolSz.ToString(CultureInfo.InvariantCulture),
                    maxPoolSz.ToString(CultureInfo.InvariantCulture));
            _connectionInfo = connectionInfo.Clone();
            ApplicationArguments = applicationArguments;
            AvailableForConnection = false;
            DispatchTable = new DispatchTable<object>();
            _runningPowerShells = new System.Collections.Concurrent.ConcurrentStack<PowerShell>();
            CreateDSHandler(typeTable);
        /// Create a runspacepool object in the disconnected state.
        /// <param name="instanceId">Identifies remote session to connect.</param>
        /// <param name="isDisconnected">Indicates whether the runspacepool is disconnected.</param>
        /// <param name="connectCommands">Array of commands associated with this runspace pool.</param>
        /// <param name="connectionInfo">Connection information for remote server.</param>
        /// <param name="typeTable">TypeTable for object serialization/deserialization.</param>
        internal RemoteRunspacePoolInternal(Guid instanceId, string name, bool isDisconnected,
            ConnectCommandInfo[] connectCommands, RunspaceConnectionInfo connectionInfo, PSHost host, TypeTable typeTable)
            : base(1, 1)
            if (instanceId == Guid.Empty)
                throw PSTraceSource.NewArgumentException(nameof(instanceId));
            if (connectCommands == null)
                throw PSTraceSource.NewArgumentNullException("ConnectCommandInfo[]");
                Dbg.Assert(false, "ConnectionInfo must be WSManConnectionInfo");
            // Create the runspace pool object to have the same instanceId as the remote session.
            this.instanceId = instanceId;
            // This indicates that this is a disconnected remote runspace pool and min/max values
            // are currently unknown. These values will be updated once the object is connected.
            this.minPoolSz = -1;
            this.maxPoolSz = -1;
            ConnectCommands = connectCommands;
            // Create this object in the disconnected state.
            SetRunspacePoolState(new RunspacePoolStateInfo(RunspacePoolState.Disconnected, null));
            AvailableForConnection = isDisconnected;
        /// Helper method to create the dispatchTable and dataStructureHandler objects.
        private void CreateDSHandler(TypeTable typeTable)
            DataStructureHandler = new ClientRunspacePoolDataStructureHandler(this, typeTable);
            DataStructureHandler.RemoteHostCallReceived += HandleRemoteHostCalls;
            DataStructureHandler.StateInfoReceived += HandleStateInfoReceived;
            DataStructureHandler.RSPoolInitInfoReceived += HandleInitInfoReceived;
            DataStructureHandler.ApplicationPrivateDataReceived += HandleApplicationPrivateDataReceived;
            DataStructureHandler.SessionClosing += HandleSessionClosing;
            DataStructureHandler.SessionClosed += HandleSessionClosed;
            DataStructureHandler.SetMaxMinRunspacesResponseReceived += HandleResponseReceived;
            DataStructureHandler.URIRedirectionReported += HandleURIDirectionReported;
            DataStructureHandler.PSEventArgsReceived += HandlePSEventArgsReceived;
            DataStructureHandler.SessionDisconnected += HandleSessionDisconnected;
            DataStructureHandler.SessionReconnected += HandleSessionReconnected;
            DataStructureHandler.SessionRCDisconnecting += HandleSessionRCDisconnecting;
            DataStructureHandler.SessionCreateCompleted += HandleSessionCreateCompleted;
                return _connectionInfo;
        /// The ClientRunspacePoolDataStructureHandler associated with this
        internal ClientRunspacePoolDataStructureHandler DataStructureHandler { get; private set; }
        /// List of CommandConnectInfo objects for each remote running command
        /// associated with this remote runspace pool.
        internal ConnectCommandInfo[] ConnectCommands { get; set; } = null;
        /// Gets and sets the name string for this runspace pool object.
            get { return _friendlyName; }
            set { _friendlyName = value ?? string.Empty; }
        /// Indicates whether this runspace pools viable/available for connection.
        internal bool AvailableForConnection { get; private set; }
        /// Returns robust connection maximum retry time in milliseconds.
        internal int MaxRetryConnectionTime
                return (DataStructureHandler != null) ? DataStructureHandler.MaxRetryConnectionTime : 0;
        public override RunspacePoolAvailability RunspacePoolAvailability
                RunspacePoolAvailability availability;
                if (stateInfo.State == RunspacePoolState.Disconnected)
                    // Set availability for disconnected runspace pool in the
                    // same way it is set for runspaces.
                    availability = (AvailableForConnection) ?
                            RunspacePoolAvailability.None :     // Disconnected runspacepool available for connection.
                            RunspacePoolAvailability.Busy;      // Disconnected runspacepool unavailable for connection.
                    availability = base.RunspacePoolAvailability;
                return availability;
        /// Property to indicate that the debugger associated to this
        /// remote runspace is in debug stop mode.
        internal bool IsRemoteDebugStop
        #region internal Methods
        internal override bool ResetRunspaceState()
            // Version check.  Reset Runspace is supported only on PSRP protocol
            // version 2.3 or greater.
            Version remoteProtocolVersionDeclaredByServer = PSRemotingProtocolVersion;
            if ((remoteProtocolVersionDeclaredByServer == null) ||
                (remoteProtocolVersionDeclaredByServer < RemotingConstants.ProtocolVersion_2_3))
                throw PSTraceSource.NewInvalidOperationException(RunspacePoolStrings.ResetRunspaceStateNotSupportedOnServer);
            long callId = 0;
                callId = DispatchTable.CreateNewCallId();
                DataStructureHandler.SendResetRunspaceStateToServer(callId);
            // This call blocks until the response is received.
            object response = DispatchTable.GetResponse(callId, false);
            return (bool)response;
        internal override bool SetMaxRunspaces(int maxRunspaces)
                if (maxRunspaces < minPoolSz || maxRunspaces == maxPoolSz || stateInfo.State == RunspacePoolState.Closed
                    || stateInfo.State == RunspacePoolState.Closing || stateInfo.State == RunspacePoolState.Broken)
                // if the runspace pool is not opened as yet, or is in Disconnected state.
                // just change the value locally. No need to
                // send a message
                if (stateInfo.State == RunspacePoolState.BeforeOpen ||
                    stateInfo.State == RunspacePoolState.Disconnected)
                // sending the message should be done within the lock
                // to ensure that multiple calls to SetMaxRunspaces
                // will be executed on the server in the order in which
                // they were called in the client
                DataStructureHandler.SendSetMaxRunspacesToServer(maxRunspaces, callId);
            // this call blocks until the response is received
            isSizeIncreased = (bool)response;
            return isSizeIncreased;
        internal override bool SetMinRunspaces(int minRunspaces)
            bool isSizeDecreased = false;
                if ((minRunspaces < 1) || (minRunspaces > maxPoolSz) || (minRunspaces == minPoolSz)
                    || stateInfo.State == RunspacePoolState.Closed || stateInfo.State == RunspacePoolState.Closing ||
                    stateInfo.State == RunspacePoolState.Broken)
                // to ensure that multiple calls to SetMinRunspaces
                DataStructureHandler.SendSetMinRunspacesToServer(minRunspaces, callId);
            isSizeDecreased = (bool)response;
            if (isSizeDecreased)
            return isSizeDecreased;
        /// this method from the remote server.
        /// <returns>The number of runspaces available in the pool.</returns>
        internal override int GetAvailableRunspaces()
            int availableRunspaces = 0;
                // if the runspace pool is opened we need to
                // get the value from the server, else
                // return maxrunspaces
                    // to ensure that multiple calls to GetAvailableRunspaces
                DataStructureHandler.SendGetAvailableRunspacesToServer(callId);
            object response = DispatchTable.GetResponse(callId, 0);
            availableRunspaces = (int)response;
            return availableRunspaces;
        /// The server sent application private data.  Store the data so that user
        /// can get it later.
        /// <param name="eventArgs">Argument describing this event.</param>
        internal void HandleApplicationPrivateDataReceived(object sender,
            RemoteDataEventArgs<PSPrimitiveDictionary> eventArgs)
            this.SetApplicationPrivateData(eventArgs.Data);
        internal void HandleInitInfoReceived(object sender,
                        RemoteDataEventArgs<RunspacePoolInitInfo> eventArgs)
            RunspacePoolStateInfo info = new RunspacePoolStateInfo(RunspacePoolState.Opened, null);
                minPoolSz = eventArgs.Data.MinRunspaces;
                maxPoolSz = eventArgs.Data.MaxRunspaces;
                if (stateInfo.State == RunspacePoolState.Connecting)
                    ResetDisconnectedOnExpiresOn();
                    SetRunspacePoolState(info);
                // Private application data is sent after (post) connect.  We need
                // to wait for application data before raising the state change
                // Connecting -> Opened event.
                ThreadPool.QueueUserWorkItem(WaitAndRaiseConnectEventsProc, info);
        /// The state of the server RunspacePool has changed. Handle
        /// the same and reflect local states accordingly.
        internal void HandleStateInfoReceived(object sender,
            RemoteDataEventArgs<RunspacePoolStateInfo> eventArgs)
            RunspacePoolStateInfo newStateInfo = eventArgs.Data;
            Dbg.Assert(newStateInfo != null, "state information should not be null");
            if (newStateInfo.State == RunspacePoolState.Opened)
                        SetRunspacePoolState(newStateInfo);
                    // this needs to be done outside the lock to avoid a
                    // deadlock scenario
                    SetOpenAsCompleted();
            else if (newStateInfo.State == RunspacePoolState.Closed || newStateInfo.State == RunspacePoolState.Broken)
                bool doClose = false;
                    if (stateInfo.State == RunspacePoolState.Closed || stateInfo.State == RunspacePoolState.Broken)
                        // there is nothing to do here
                    if (stateInfo.State == RunspacePoolState.Opening
                     || stateInfo.State == RunspacePoolState.Opened
                     || stateInfo.State == RunspacePoolState.Closing)
                        doClose = true;
                if (doClose)
                    // if closeAsyncResult is null, BeginClose is not called. That means
                    // we are getting close event from server, in this case release the
                    // local resources
                    if (_closeAsyncResult == null)
                        // Close the local resources.
                        DataStructureHandler.CloseRunspacePoolAsync();
                    // Delay notifying upper layers of finished state change event
                    // until after transport close ack is received (HandleSessionClosed handler).
        /// A host call has been proxied from the server which needs to
        /// be executed.
        internal void HandleRemoteHostCalls(object sender,
            RemoteDataEventArgs<RemoteHostCall> eventArgs)
                HostCallReceived.SafeInvoke(sender, eventArgs);
                RemoteHostCall hostCall = eventArgs.Data;
                if (hostCall.IsVoidMethod)
                    hostCall.ExecuteVoidMethod(host);
                    RemoteHostResponse remoteHostResponse = hostCall.ExecuteNonVoidMethod(host);
                    DataStructureHandler.SendHostResponseToServer(remoteHostResponse);
        internal PSHost Host
        /// Application arguments to use when opening a remote session.
        internal PSPrimitiveDictionary ApplicationArguments { get; }
        /// - calling this method on a remote runspace will block until the data is received from the server.
        /// - unless the runspace is disconnected and data hasn't been received in which case it returns null immediately.
        internal override PSPrimitiveDictionary GetApplicationPrivateData()
            if (this.RunspacePoolStateInfo.State == RunspacePoolState.Disconnected &&
                !_applicationPrivateDataReceived.WaitOne(0))
                // Runspace pool was disconnected before application data was returned.  Application
                // data cannot be returned with the runspace pool disconnected so return null.
        internal void SetApplicationPrivateData(PSPrimitiveDictionary applicationPrivateData)
                if (_applicationPrivateDataReceived.WaitOne(0))
                    return; // ignore server's attempt to set application private data if it has already been set
                _applicationPrivateDataReceived.Set();
                foreach (Runspace runspace in this.runspaceList)
                    runspace.SetApplicationPrivateData(applicationPrivateData);
        internal override void PropagateApplicationPrivateData(Runspace runspace)
        private readonly ManualResetEvent _applicationPrivateDataReceived = new ManualResetEvent(false);
        /// This event is raised, when a host call is for a remote runspace
        /// which this runspace pool wraps.
        /// EventHandler used to report connection URI redirections to the application.
        internal event EventHandler<RemoteDataEventArgs<Uri>> URIRedirectionReported;
        /// Notifies the successful creation of the runspace session.
        internal event EventHandler<CreateCompleteEventArgs> SessionCreateCompleted;
        internal void CreatePowerShellOnServerAndInvoke(ClientRemotePowerShell shell)
            DataStructureHandler.CreatePowerShellOnServerAndInvoke(shell);
            // send any input that may be available
            if (!shell.NoInput)
                shell.SendInput();
        /// Add a ClientPowerShellDataStructureHandler to ClientRunspaceDataStructureHandler list.
        /// <param name="psShellInstanceId">PowerShell Instance Id.</param>
        /// <param name="psDSHandler">ClientPowerShellDataStructureHandler for PowerShell.</param>
        internal void AddRemotePowerShellDSHandler(Guid psShellInstanceId, ClientPowerShellDataStructureHandler psDSHandler)
            DataStructureHandler.AddRemotePowerShellDSHandler(psShellInstanceId, psDSHandler);
        /// Returns true if Runspace supports disconnect.
        internal bool CanDisconnect
                if (remoteProtocolVersionDeclaredByServer != null && DataStructureHandler != null)
                    // Disconnect/Connect support is currently only provided by the WSMan transport
                    // that is running PSRP protocol version 2.2 and greater.
                    return (remoteProtocolVersionDeclaredByServer >= RemotingConstants.ProtocolVersion_2_2 &&
                            DataStructureHandler.EndpointSupportsDisconnect);
        /// Returns the WinRM protocol version object for this runspace
        /// pool connection.
        internal Version PSRemotingProtocolVersion
                Version winRMProtocolVersion = null;
                PSPrimitiveDictionary psPrimitiveDictionary = GetApplicationPrivateData();
                if (psPrimitiveDictionary != null)
                        psPrimitiveDictionary,
                        out winRMProtocolVersion,
                return winRMProtocolVersion;
            PowerShell powershell;
            if (_runningPowerShells.TryPop(out powershell))
        /// Return the current running PowerShell.
        internal PowerShell GetCurrentRunningPowerShell()
            if (_runningPowerShells.TryPeek(out powershell))
        protected override IAsyncResult CoreOpen(bool isAsync, AsyncCallback callback,
            PSEtwLog.LogOperationalVerbose(PSEventId.RunspacePoolOpen, PSOpcode.Open,
                            PSTask.CreateRunspace, PSKeyword.UseAlwaysOperational);
            // Telemetry here - remote session
            ApplicationInsightsTelemetry.SendTelemetryMetric(TelemetryType.RemoteSessionOpen, isAsync.ToString());
            TelemetryAPI.ReportRemoteSessionCreated(_connectionInfo);
            // BUGBUG: the following comment needs to be validated
            // only one thread will reach here, so no need
            // to lock
            RunspacePoolAsyncResult asyncResult = new RunspacePoolAsyncResult(
                    instanceId, callback, asyncState, true);
            _openAsyncResult = asyncResult;
            // send a message using the data structure handler to open the RunspacePool
            // on the remote server
            DataStructureHandler.CreateRunspacePoolAndOpenAsync();
        /// Synchronous open.
            IAsyncResult asyncResult = BeginOpen(null, null);
            EndOpen(asyncResult);
            // close and wait
            IAsyncResult asyncResult = BeginClose(null, null);
            EndClose(asyncResult);
        /// Closes the RunspacePool asynchronously. To get the exceptions
        /// that might have occurred, call EndOpen.
        /// An AsyncCallback to call once the BeginClose completes
        /// with
        /// operation
        public override IAsyncResult BeginClose(AsyncCallback callback, object asyncState)
            bool skipClosing = false;
            RunspacePoolStateInfo copyState = new RunspacePoolStateInfo(RunspacePoolState.BeforeOpen, null);
            RunspacePoolAsyncResult asyncResult = null;
                    (stateInfo.State == RunspacePoolState.Broken))
                    skipClosing = true;
                    asyncResult = new RunspacePoolAsyncResult(instanceId, callback, asyncState, false);
                else if (stateInfo.State == RunspacePoolState.BeforeOpen)
                    copyState = stateInfo = new RunspacePoolStateInfo(RunspacePoolState.Closed, null);
                    _closeAsyncResult = null;
                else if (stateInfo.State == RunspacePoolState.Opened ||
                         stateInfo.State == RunspacePoolState.Opening)
                    copyState = stateInfo = new RunspacePoolStateInfo(RunspacePoolState.Closing, null);
                    _closeAsyncResult = new RunspacePoolAsyncResult(instanceId, callback, asyncState, false);
                    asyncResult = _closeAsyncResult;
                else if (stateInfo.State == RunspacePoolState.Disconnected ||
                         stateInfo.State == RunspacePoolState.Disconnecting ||
                         stateInfo.State == RunspacePoolState.Connecting)
                    // Continue with closing so the PSRP layer is aware that the client side session is
                    // being closed.  This will result in a broken session on the client.
                else if (stateInfo.State == RunspacePoolState.Closing)
                    return _closeAsyncResult;
            // raise the events outside the lock
                RaiseStateChangeEvent(copyState);
            if (!skipClosing)
                // SetRunspacePoolState(new RunspacePoolStateInfo(RunspacePoolState.Closing, null), true);
                // send a message using the data structure handler to close the RunspacePool
                // signal the wait handle
        /// Synchronous disconnect.
            IAsyncResult asyncResult = BeginDisconnect(null, null);
            EndDisconnect(asyncResult);
        /// Asynchronous disconnect.
        /// <param name="callback">AsyncCallback object.</param>
        /// <param name="state">State object.</param>
        public override IAsyncResult BeginDisconnect(AsyncCallback callback, object state)
            if (!CanDisconnect)
                throw PSTraceSource.NewInvalidOperationException(RunspacePoolStrings.DisconnectNotSupportedOnServer);
            RunspacePoolState currentState;
                currentState = stateInfo.State;
                if (currentState == RunspacePoolState.Opened)
                    RunspacePoolStateInfo newStateInfo = new RunspacePoolStateInfo(RunspacePoolState.Disconnecting, null);
            // Raise events outside of lock.
                RaiseStateChangeEvent(this.stateInfo);
                    instanceId, callback, state, false);
                _disconnectAsyncResult = asyncResult;
                DataStructureHandler.DisconnectPoolAsync();
                // Return local reference to async object since the class member can
                // be asynchronously nulled if the session closes suddenly.
                InvalidRunspacePoolStateException invalidStateException = new InvalidRunspacePoolStateException(message,
                throw invalidStateException;
        /// Waits for BeginDisconnect operation to complete.
        /// <param name="asyncResult">IAsyncResult object.</param>
        public override void EndDisconnect(IAsyncResult asyncResult)
        /// Synchronous connect.
            IAsyncResult asyncResult = BeginConnect(null, null);
            EndConnect(asyncResult);
        /// Asynchronous connect.
        /// <param name="callback">ASyncCallback object.</param>
        /// <param name="state">State Object.</param>
        public override IAsyncResult BeginConnect(AsyncCallback callback, object state)
            if (!AvailableForConnection)
                throw PSTraceSource.NewInvalidOperationException(RunspacePoolStrings.CannotConnect);
                if (currentState == RunspacePoolState.Disconnected)
                    RunspacePoolStateInfo newStateInfo = new RunspacePoolStateInfo(RunspacePoolState.Connecting, null);
            raiseEvents = false;
                // Assign to local variable to ensure we always pass a non-null value.
                // The async class members can be nulled out if the session closes suddenly.
                RunspacePoolAsyncResult ret = new RunspacePoolAsyncResult(
                if (_canReconnect)
                    // This indicates a reconnect scenario where this object instance was previously
                    // disconnected.
                    _reconnectAsyncResult = ret;
                    DataStructureHandler.ReconnectPoolAsync();
                    // This indicates a reconstruction scenario where this object was created
                    // in the disconnect state and is being connected for the first time.
                    _openAsyncResult = ret;
                    DataStructureHandler.ConnectPoolAsync();
                string message = StringUtil.Format(RunspacePoolStrings.InvalidRunspacePoolState, RunspacePoolState.Disconnected, stateInfo.State);
                        stateInfo.State, RunspacePoolState.Disconnected);
        /// Waits for BeginConnect to complete.
        public override void EndConnect(IAsyncResult asyncResult)
        /// <returns>Array of PowerShell objects.</returns>
        public override Collection<PowerShell> CreateDisconnectedPowerShells(RunspacePool runspacePool)
            Collection<PowerShell> psCollection = new Collection<PowerShell>();
            if (ConnectCommands == null)
                // Throw error indicating that this runspacepool is not configured for
                // reconstructing commands.
                string msg = StringUtil.Format(RunspacePoolStrings.CannotReconstructCommands, this.Name);
                throw new InvalidRunspacePoolStateException(msg);
            // Get list of all disconnected commands associated with this runspace pool.
            foreach (ConnectCommandInfo connectCmdInfo in ConnectCommands)
                psCollection.Add(new PowerShell(connectCmdInfo, runspacePool));
            return psCollection;
        public override RunspacePoolCapability GetCapabilities()
            RunspacePoolCapability returnCaps = RunspacePoolCapability.Default;
            if (CanDisconnect)
                returnCaps |= RunspacePoolCapability.SupportsDisconnect;
            return returnCaps;
        #region Static methods
        internal static RunspacePool[] GetRemoteRunspacePools(RunspaceConnectionInfo connectionInfo, PSHost host, TypeTable typeTable)
            if (connectionInfo is not WSManConnectionInfo wsmanConnectionInfoParam)
                // Disconnect-Connect currently only supported by WSMan.
            List<RunspacePool> discRunspacePools = new List<RunspacePool>();
            // Enumerate all runspacepools
            Collection<PSObject> runspaceItems = RemoteRunspacePoolEnumeration.GetRemotePools(wsmanConnectionInfoParam);
            foreach (PSObject rsObject in runspaceItems)
                // Create a new WSMan connection info object for each returned runspace pool.
                WSManConnectionInfo wsmanConnectionInfo = wsmanConnectionInfoParam.Copy();
                PSPropertyInfo pspShellId = rsObject.Properties["ShellId"];
                PSPropertyInfo pspState = rsObject.Properties["State"];
                PSPropertyInfo pspName = rsObject.Properties["Name"];
                PSPropertyInfo pspResourceUri = rsObject.Properties["ResourceUri"];
                if (pspShellId == null || pspState == null || pspName == null || pspResourceUri == null)
                string strName = pspName.Value.ToString();
                string strShellUri = pspResourceUri.Value.ToString();
                bool isDisconnected = pspState.Value.ToString().Equals("Disconnected", StringComparison.OrdinalIgnoreCase);
                Guid shellId = Guid.Parse(pspShellId.Value.ToString());
                // Filter returned items for PowerShell sessions.
                if (!strShellUri.StartsWith(WSManNativeApi.ResourceURIPrefix, StringComparison.OrdinalIgnoreCase))
                // Update wsmanconnection information with server settings.
                UpdateWSManConnectionInfo(wsmanConnectionInfo, rsObject);
                // Ensure that EnableNetworkAccess property is always enabled for reconstructed runspaces.
                wsmanConnectionInfo.EnableNetworkAccess = true;
                // Compute runspace DisconnectedOn and ExpiresOn fields.
                    DateTime? disconnectedOn;
                    DateTime? expiresOn;
                    ComputeDisconnectedOnExpiresOn(rsObject, out disconnectedOn, out expiresOn);
                    wsmanConnectionInfo.DisconnectedOn = disconnectedOn;
                    wsmanConnectionInfo.ExpiresOn = expiresOn;
                List<ConnectCommandInfo> connectCmdInfos = new List<ConnectCommandInfo>();
                // Enumerate all commands on runspace pool.
                Collection<PSObject> commandItems;
                    commandItems = RemoteRunspacePoolEnumeration.GetRemoteCommands(shellId, wsmanConnectionInfo);
                catch (CmdletInvocationException e)
                    if (e.InnerException != null && e.InnerException is InvalidOperationException)
                        // If we cannot successfully retrieve command information then this runspace
                        // object we are building is invalid and must be skipped.
                foreach (PSObject cmdObject in commandItems)
                    PSPropertyInfo pspCommandId = cmdObject.Properties["CommandId"];
                    PSPropertyInfo pspCommandLine = cmdObject.Properties["CommandLine"];
                    if (pspCommandId == null)
                        Dbg.Assert(false, "Should not get an empty command Id from a remote runspace pool.");
                    string cmdLine = (pspCommandLine != null) ? pspCommandLine.Value.ToString() : string.Empty;
                    Guid cmdId = Guid.Parse(pspCommandId.Value.ToString());
                    connectCmdInfos.Add(new ConnectCommandInfo(cmdId, cmdLine));
                // At this point we don't know if the runspace pool we want to connect to has just one runspace
                // (a RemoteRunspace/PSSession) or multiple runspaces in its pool.  We do have an array of
                // running command information which will indicate a runspace pool if the count is gt one.
                RunspacePool runspacePool = new RunspacePool(isDisconnected, shellId, strName,
                    connectCmdInfos.ToArray(), wsmanConnectionInfo, host, typeTable);
                discRunspacePools.Add(runspacePool);
            return discRunspacePools.ToArray();
        internal static RunspacePool GetRemoteRunspacePool(RunspaceConnectionInfo connectionInfo, Guid sessionId, Guid? commandId, PSHost host, TypeTable typeTable)
            if (commandId != null)
                connectCmdInfos.Add(new ConnectCommandInfo(commandId.Value, string.Empty));
            return new RunspacePool(true, sessionId, string.Empty, connectCmdInfos.ToArray(), connectionInfo, host, typeTable);
        private static void UpdateWSManConnectionInfo(
            WSManConnectionInfo wsmanConnectionInfo,
            PSObject rsInfoObject)
            PSPropertyInfo pspIdleTimeOut = rsInfoObject.Properties["IdleTimeOut"];
            PSPropertyInfo pspBufferMode = rsInfoObject.Properties["BufferMode"];
            PSPropertyInfo pspResourceUri = rsInfoObject.Properties["ResourceUri"];
            PSPropertyInfo pspLocale = rsInfoObject.Properties["Locale"];
            PSPropertyInfo pspDataLocale = rsInfoObject.Properties["DataLocale"];
            PSPropertyInfo pspCompressionMode = rsInfoObject.Properties["CompressionMode"];
            PSPropertyInfo pspEncoding = rsInfoObject.Properties["Encoding"];
            PSPropertyInfo pspProfile = rsInfoObject.Properties["ProfileLoaded"];
            PSPropertyInfo pspMaxIdleTimeout = rsInfoObject.Properties["MaxIdleTimeout"];
            if (pspIdleTimeOut != null)
                int idleTimeout;
                if (GetTimeIntValue(pspIdleTimeOut.Value as string, out idleTimeout))
                    wsmanConnectionInfo.IdleTimeout = idleTimeout;
            if (pspBufferMode != null)
                string bufferingMode = pspBufferMode.Value as string;
                if (bufferingMode != null)
                    OutputBufferingMode outputBufferingMode;
                    if (Enum.TryParse<OutputBufferingMode>(bufferingMode, out outputBufferingMode))
                        // Update connection info.
                        wsmanConnectionInfo.OutputBufferingMode = outputBufferingMode;
            if (pspResourceUri != null)
                string strShellUri = pspResourceUri.Value as string;
                if (strShellUri != null)
                    wsmanConnectionInfo.ShellUri = strShellUri;
            if (pspLocale != null)
                string localString = pspLocale.Value as string;
                if (localString != null)
                        wsmanConnectionInfo.UICulture = new CultureInfo(localString);
            if (pspDataLocale != null)
                string dataLocalString = pspDataLocale.Value as string;
                if (dataLocalString != null)
                        wsmanConnectionInfo.Culture = new CultureInfo(dataLocalString);
            if (pspCompressionMode != null)
                string compressionModeString = pspCompressionMode.Value as string;
                if (compressionModeString != null)
                    wsmanConnectionInfo.UseCompression = !compressionModeString.Equals("NoCompression", StringComparison.OrdinalIgnoreCase);
            if (pspEncoding != null)
                string encodingString = pspEncoding.Value as string;
                if (encodingString != null)
                    wsmanConnectionInfo.UseUTF16 = encodingString.Equals("UTF16", StringComparison.OrdinalIgnoreCase);
            if (pspProfile != null)
                string machineProfileLoadedString = pspProfile.Value as string;
                if (machineProfileLoadedString != null)
                    wsmanConnectionInfo.NoMachineProfile = !machineProfileLoadedString.Equals("Yes", StringComparison.OrdinalIgnoreCase);
            if (pspMaxIdleTimeout != null)
                int maxIdleTimeout;
                if (GetTimeIntValue(pspMaxIdleTimeout.Value as string, out maxIdleTimeout))
                    wsmanConnectionInfo.MaxIdleTimeout = maxIdleTimeout;
        private static void ComputeDisconnectedOnExpiresOn(
            PSObject rsInfoObject,
            out DateTime? disconnectedOn,
            out DateTime? expiresOn)
            PSPropertyInfo pspShellInactivity = rsInfoObject.Properties["ShellInactivity"];
            if (pspIdleTimeOut != null && pspShellInactivity != null)
                string shellInactivityString = pspShellInactivity.Value as string;
                if ((shellInactivityString != null) &&
                    GetTimeIntValue(pspIdleTimeOut.Value as string, out idleTimeout))
                        TimeSpan shellInactivityTime = Xml.XmlConvert.ToTimeSpan(shellInactivityString);
                        TimeSpan idleTimeoutTime = TimeSpan.FromSeconds(idleTimeout / 1000);
                        if (idleTimeoutTime > shellInactivityTime)
                            disconnectedOn = now.Subtract(shellInactivityTime);
                            expiresOn = disconnectedOn.Value.Add(idleTimeoutTime);
            disconnectedOn = null;
            expiresOn = null;
        private static bool GetTimeIntValue(string timeString, out int value)
            if (timeString != null)
                string timeoutString = timeString.Replace("PT", string.Empty).Replace("S", string.Empty);
                    // Convert time from seconds to milliseconds.
                    int idleTimeout = (int)(Convert.ToDouble(timeoutString, CultureInfo.InvariantCulture) * 1000);
                    if (idleTimeout > 0)
                        value = idleTimeout;
        /// Set the new runspace pool state based on the state of the
        /// server RunspacePool.
        /// <param name="newStateInfo">state information object
        /// describing the state change at the server RunspacePool</param>
        private void SetRunspacePoolState(RunspacePoolStateInfo newStateInfo)
            SetRunspacePoolState(newStateInfo, false);
        /// server RunspacePool and raise events if required.
        /// <param name="raiseEvents">Raise state changed events if true.</param>
        private void SetRunspacePoolState(RunspacePoolStateInfo newStateInfo, bool raiseEvents)
            stateInfo = newStateInfo;
            // Update the availableForConnection variable based on state change.
            AvailableForConnection = (stateInfo.State == RunspacePoolState.Disconnected ||
                                           stateInfo.State == RunspacePoolState.Opened);
                RaiseStateChangeEvent(newStateInfo);
        private void HandleSessionDisconnected(object sender, RemoteDataEventArgs<Exception> eventArgs)
            bool stateChange = false;
                if (stateInfo.State == RunspacePoolState.Disconnecting)
                    UpdateDisconnectedExpiresOn();
                    SetRunspacePoolState(new RunspacePoolStateInfo(RunspacePoolState.Disconnected, eventArgs.Data));
                    stateChange = true;
                // Set boolean indicating this object has previous connection state and so
                // can be reconnected as opposed to the alternative where the connection
                // state has to be reconstructed then connected.
                _canReconnect = true;
            // Do state change work outside of lock.
            if (stateChange)
                SetDisconnectAsCompleted();
        private void SetDisconnectAsCompleted()
            if (_disconnectAsyncResult != null && !_disconnectAsyncResult.IsCompleted)
                _disconnectAsyncResult.SetAsCompleted(stateInfo.Reason);
                _disconnectAsyncResult = null;
        private void HandleSessionReconnected(object sender, RemoteDataEventArgs<Exception> eventArgs)
                    SetRunspacePoolState(new RunspacePoolStateInfo(RunspacePoolState.Opened, null));
                SetReconnectAsCompleted();
        private void SetReconnectAsCompleted()
            if (_reconnectAsyncResult != null && !_reconnectAsyncResult.IsCompleted)
                _reconnectAsyncResult.SetAsCompleted(stateInfo.Reason);
                _reconnectAsyncResult = null;
        /// The session is closing set the state and reason accordingly.
        private void HandleSessionClosing(object sender, RemoteDataEventArgs<Exception> eventArgs)
            // just capture the reason for closing here..handle the session closed event
            // to change state appropriately.
            _closingReason = eventArgs.Data;
        /// The session closed, set the state and reason accordingly.
        private void HandleSessionClosed(object sender, RemoteDataEventArgs<Exception> eventArgs)
            if (eventArgs.Data != null)
            // Set state under lock.
            RunspacePoolState prevState;
            RunspacePoolStateInfo finishedStateInfo;
                prevState = stateInfo.State;
                switch (prevState)
                        // Since RunspacePool is not in closing state, this close is
                        // happening because of data structure handler error. Set the state to broken.
                        SetRunspacePoolState(new RunspacePoolStateInfo(RunspacePoolState.Broken, _closingReason));
                        SetRunspacePoolState(new RunspacePoolStateInfo(RunspacePoolState.Closed, _closingReason));
                finishedStateInfo = new RunspacePoolStateInfo(stateInfo.State, stateInfo.Reason);
            // Raise notification event outside of lock.
                RaiseStateChangeEvent(finishedStateInfo);
                // Don't throw exception on notification thread.
            // Check if we have either an existing disconnect or connect async object
            // and if so make sure they are set to completed since this is a
            // final state for the runspace pool.
            // Ensure an existing Close async object is completed.
            SetCloseAsCompleted();
        /// Set the async result for open as completed.
        private void SetOpenAsCompleted()
            RunspacePoolAsyncResult tempOpenAsyncResult = _openAsyncResult;
            _openAsyncResult = null;
            if (tempOpenAsyncResult != null && !tempOpenAsyncResult.IsCompleted)
                tempOpenAsyncResult.SetAsCompleted(stateInfo.Reason);
        /// Set the async result for close as completed.
        private void SetCloseAsCompleted()
            // abort all pending calls.
            DispatchTable.AbortAllCalls();
            if (_closeAsyncResult != null)
                _closeAsyncResult.SetAsCompleted(stateInfo.Reason);
            // Ensure that openAsyncResult is completed and that
            // any error is thrown on the calling thread.
            // The session can be closed at any time, including
            // during Open processing.
            // Ensure private application data wait is released.
        /// When a response to a SetMaxRunspaces or SetMinRunspaces is received,
        /// from the server, this method sets the response and thereby unblocks
        /// corresponding call.
        /// <param name="sender">Sender of this message, unused.</param>
        /// <param name="eventArgs">Contains response and call id.</param>
        private void HandleResponseReceived(object sender, RemoteDataEventArgs<PSObject> eventArgs)
            PSObject data = eventArgs.Data;
            object response = RemotingDecoder.GetPropertyValue<object>(data, RemoteDataNameStrings.RunspacePoolOperationResponse);
            long callId = RemotingDecoder.GetPropertyValue<long>(data, RemoteDataNameStrings.CallId);
            DispatchTable.SetResponse(callId, response);
        private void HandleURIDirectionReported(object sender, RemoteDataEventArgs<Uri> eventArgs)
            WSManConnectionInfo wsmanConnectionInfo = _connectionInfo as WSManConnectionInfo;
            if (wsmanConnectionInfo != null)
                wsmanConnectionInfo.ConnectionUri = eventArgs.Data;
                URIRedirectionReported.SafeInvoke(this, eventArgs);
        /// When the server sends a PSEventArgs this method will add it to the local event queue.
        private void HandlePSEventArgsReceived(object sender, RemoteDataEventArgs<PSEventArgs> e)
            OnForwardEvent(e.Data);
        /// A session disconnect has been initiated by the WinRM robust connection layer.  Set
        /// internal state to Disconnecting.
        private void HandleSessionRCDisconnecting(object sender, RemoteDataEventArgs<Exception> e)
            Dbg.Assert(this.stateInfo.State == RunspacePoolState.Opened,
                "RC disconnect should only occur for runspace pools in the Opened state.");
                SetRunspacePoolState(new RunspacePoolStateInfo(RunspacePoolState.Disconnecting, e.Data));
        /// The session has been successfully created.
        private void HandleSessionCreateCompleted(object sender, CreateCompleteEventArgs eventArgs)
            // Update connectionInfo with updated information from the transport.
                _connectionInfo.IdleTimeout = eventArgs.ConnectionInfo.IdleTimeout;
                _connectionInfo.MaxIdleTimeout = eventArgs.ConnectionInfo.MaxIdleTimeout;
                    wsmanConnectionInfo.OutputBufferingMode =
                        ((WSManConnectionInfo)eventArgs.ConnectionInfo).OutputBufferingMode;
            // Forward event.
            SessionCreateCompleted.SafeInvoke<CreateCompleteEventArgs>(this, eventArgs);
        private void ResetDisconnectedOnExpiresOn()
            // Reset DisconnectedOn/ExpiresOn
            WSManConnectionInfo wsManConnectionInfo = _connectionInfo as WSManConnectionInfo;
            wsManConnectionInfo?.NullDisconnectedExpiresOn();
        private void UpdateDisconnectedExpiresOn()
            // Set DisconnectedOn/ExpiresOn for disconnected session.
            wsManConnectionInfo?.SetDisconnectedExpiresOnToNow();
        /// Waits for application private data from server before raising
        /// event:  Connecting->Opened state changed event.
        private void WaitAndRaiseConnectEventsProc(object state)
            RunspacePoolStateInfo info = state as RunspacePoolStateInfo;
            Dbg.Assert(info != null, "State -> Event arguments cannot be null.");
            // Wait for private application data to arrive from server.
                _applicationPrivateDataReceived.WaitOne();
            // Raise state changed event.
                RaiseStateChangeEvent(info);
            // Set Opened async object.
        private readonly RunspaceConnectionInfo _connectionInfo;     // connection info with which this
        // runspace is created
        // data structure handler handling
        private RunspacePoolAsyncResult _openAsyncResult; // async result object generated on
        // CoreOpen
        private RunspacePoolAsyncResult _closeAsyncResult; // async result object generated by
        // BeginClose
        private Exception _closingReason;                       // reason for a Closing state transition
        private RunspacePoolAsyncResult _disconnectAsyncResult; // async result object generated on CoreDisconnect
        private RunspacePoolAsyncResult _reconnectAsyncResult;  // async result object generated on CoreReconnect
        private DispatchTable<object> DispatchTable { get; }
        private bool _canReconnect;
        private string _friendlyName = string.Empty;
        private readonly System.Collections.Concurrent.ConcurrentStack<PowerShell> _runningPowerShells;
            // dispose the base class before disposing dataStructure handler.
                DataStructureHandler.Dispose(disposing);
                _applicationPrivateDataReceived.Dispose();
    #region ConnectCommandInfo class
    /// Class defining a remote command to connect to.
    internal class ConnectCommandInfo
        /// Remote command instance Id.
        public Guid CommandId { get; } = Guid.Empty;
        /// Remote command string.
        public string Command { get; } = string.Empty;
        /// Constructs a remote command object.
        /// <param name="cmdId">Command instance Id.</param>
        /// <param name="cmdStr">Command string.</param>
        public ConnectCommandInfo(Guid cmdId, string cmdStr)
            CommandId = cmdId;
            Command = cmdStr;
    #region RemoteRunspacePoolEnumeration class
    /// Enumerates remote runspacepools (Shells) and running commands
    /// using Get-WSManInstance cmdlet.
    internal static class RemoteRunspacePoolEnumeration
        /// Gets an array of XmlElement objects representing all
        /// disconnected runspace pools on the indicated server.
        /// <param name="wsmanConnectionInfo">Specifies the remote server to connect to.</param>]
        /// <returns>Collection of XmlElement objects.</returns>
        internal static Collection<PSObject> GetRemotePools(WSManConnectionInfo wsmanConnectionInfo)
            Collection<PSObject> result;
            using (PowerShell powerShell = PowerShell.Create())
                // Enumerate remote runspaces using the Get-WSManInstance cmdlet.
                powerShell.AddCommand("Get-WSManInstance");
                // Add parameters to enumerate Shells (runspace pools).
                powerShell.AddParameter("ResourceURI", "Shell");
                powerShell.AddParameter("Enumerate", true);
                // Add parameters for server connection.
                powerShell.AddParameter("ComputerName", wsmanConnectionInfo.ComputerName);
                powerShell.AddParameter("Authentication", ConvertPSAuthToWSManAuth(wsmanConnectionInfo.AuthenticationMechanism));
                if (wsmanConnectionInfo.Credential != null)
                    powerShell.AddParameter("Credential", wsmanConnectionInfo.Credential);
                if (wsmanConnectionInfo.CertificateThumbprint != null)
                    powerShell.AddParameter("CertificateThumbprint", wsmanConnectionInfo.CertificateThumbprint);
                if (wsmanConnectionInfo.PortSetting != -1)
                    powerShell.AddParameter("Port", wsmanConnectionInfo.Port);
                if (CheckForSSL(wsmanConnectionInfo))
                    powerShell.AddParameter("UseSSL", true);
                if (!string.IsNullOrEmpty(wsmanConnectionInfo.AppName))
                    // Remove prepended path character.
                    string appName = wsmanConnectionInfo.AppName.TrimStart('/');
                    powerShell.AddParameter("ApplicationName", appName);
                powerShell.AddParameter("SessionOption", GetSessionOptions(wsmanConnectionInfo));
                result = powerShell.Invoke();
        /// Gets an array of XmlElement objects representing each running command
        /// on the specified runspace pool with the shellid Guid.
        /// <param name="shellId">Guid of shellId (runspacepool Id).</param>
        internal static Collection<PSObject> GetRemoteCommands(Guid shellId, WSManConnectionInfo wsmanConnectionInfo)
                // Enumerate remote runspace commands using the Get-WSManInstance cmdlet.
                // Add parameters to enumerate commands.
                string filterStr = string.Create(CultureInfo.InvariantCulture, $"ShellId='{shellId.ToString().ToUpperInvariant()}'");
                powerShell.AddParameter("ResourceURI", @"Shell/Command");
                powerShell.AddParameter("Dialect", "Selector");
                powerShell.AddParameter("Filter", filterStr);
        /// Use the WSMan New-WSManSessionOption cmdlet to create a session options
        /// object used for Get-WSManInstance queries.
        /// <param name="wsmanConnectionInfo">WSManConnectionInfo.</param>
        /// <returns>WSMan session options object.</returns>
        private static object GetSessionOptions(WSManConnectionInfo wsmanConnectionInfo)
                powerShell.AddCommand("New-WSManSessionOption");
                if (wsmanConnectionInfo.ProxyAccessType != ProxyAccessType.None)
                    powerShell.AddParameter("ProxyAccessType", "Proxy" + wsmanConnectionInfo.ProxyAccessType.ToString());
                    powerShell.AddParameter("ProxyAuthentication", wsmanConnectionInfo.ProxyAuthentication.ToString());
                    if (wsmanConnectionInfo.ProxyCredential != null)
                        powerShell.AddParameter("ProxyCredential", wsmanConnectionInfo.ProxyCredential);
                // New-WSManSessionOption uses the SPNPort number here to enable SPN
                // server authentication.  It looks like any value > 0 will enable
                // this.  Since the Port property always returns a valid port value (>0)
                // just pass the WSManConnectionInfo port parameter.
                if (wsmanConnectionInfo.IncludePortInSPN)
                    powerShell.AddParameter("SPNPort", wsmanConnectionInfo.Port);
                powerShell.AddParameter("SkipCACheck", wsmanConnectionInfo.SkipCACheck);
                powerShell.AddParameter("SkipCNCheck", wsmanConnectionInfo.SkipCNCheck);
                powerShell.AddParameter("SkipRevocationCheck", wsmanConnectionInfo.SkipRevocationCheck);
                powerShell.AddParameter("OperationTimeout", wsmanConnectionInfo.OperationTimeout);
                powerShell.AddParameter("NoEncryption", wsmanConnectionInfo.NoEncryption);
                powerShell.AddParameter("UseUTF16", wsmanConnectionInfo.UseUTF16);
            return result[0].BaseObject;
        private static bool CheckForSSL(WSManConnectionInfo wsmanConnectionInfo)
            return (!string.IsNullOrEmpty(wsmanConnectionInfo.Scheme) &&
                    wsmanConnectionInfo.Scheme.Contains(WSManConnectionInfo.HttpsScheme, StringComparison.OrdinalIgnoreCase));
        private static int ConvertPSAuthToWSManAuth(AuthenticationMechanism psAuth)
            int wsmanAuth;
            switch (psAuth)
                case AuthenticationMechanism.Default:
                    wsmanAuth = 0x1;
                case AuthenticationMechanism.Basic:
                    wsmanAuth = 0x8;
                case AuthenticationMechanism.Digest:
                    wsmanAuth = 0x2;
                case AuthenticationMechanism.Credssp:
                    wsmanAuth = 0x80;
                case AuthenticationMechanism.Kerberos:
                    wsmanAuth = 0x10;
                case AuthenticationMechanism.Negotiate:
                    wsmanAuth = 0x4;
            return wsmanAuth;

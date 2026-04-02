    /// Handles all PowerShell data structure handler communication with the
    /// server side RunspacePool.
    internal sealed class ClientRunspacePoolDataStructureHandler : IDisposable
        private bool _reconnecting = false;
        /// Constructor which takes a client runspace pool and creates
        /// an associated ClientRunspacePoolDataStructureHandler.
        /// <param name="clientRunspacePool">Client runspace pool object.</param>
        /// <param name="typeTable">Typetable to use for serialization/deserialization.</param>
        internal ClientRunspacePoolDataStructureHandler(RemoteRunspacePoolInternal clientRunspacePool,
            _clientRunspacePoolId = clientRunspacePool.InstanceId;
            _minRunspaces = clientRunspacePool.GetMinRunspaces();
            _maxRunspaces = clientRunspacePool.GetMaxRunspaces();
            _host = clientRunspacePool.Host;
            _applicationArguments = clientRunspacePool.ApplicationArguments;
            RemoteSession = CreateClientRemoteSession(clientRunspacePool);
            // TODO: Assign remote session name.. should be passed from clientRunspacePool
            _transportManager = RemoteSession.SessionDataStructureHandler.TransportManager;
            _transportManager.TypeTable = typeTable;
            RemoteSession.StateChanged += HandleClientRemoteSessionStateChanged;
            _reconnecting = false;
            _transportManager.RobustConnectionNotification += HandleRobustConnectionNotification;
            _transportManager.CreateCompleted += HandleSessionCreateCompleted;
        #region Data Structure Handler Methods
        /// Create a runspace pool asynchronously (and opens) it
        /// on the server.
        internal void CreateRunspacePoolAndOpenAsync()
            // #1: Connect to remote session
            Dbg.Assert(RemoteSession.SessionDataStructureHandler.StateMachine.State == RemoteSessionState.Idle,
                "State of ClientRemoteSession is expected to be idle before connection is established");
            RemoteSession.CreateAsync();
            // #2: send the message for runspace pool creation
            // this is done in HandleClientRemoteSessionStateChanged
        /// Closes the server runspace pool asynchronously.
        internal void CloseRunspacePoolAsync()
            RemoteSession.CloseAsync();
        /// Suspends connection to a runspace pool asynchronously.
        internal void DisconnectPoolAsync()
            // Prepare running commands for disconnect and start disconnect
            // when ready.
            PrepareForAndStartDisconnect();
        /// Restore connection to a runspace pool asynchronously.
        internal void ReconnectPoolAsync()
            // TODO: Integrate this into state machine
            _reconnecting = true;
            PrepareForConnect();
            RemoteSession.ReconnectAsync();
        /// Creates a connection to an existing remote runspace pool.
        internal void ConnectPoolAsync()
            RemoteSession.ConnectAsync();
        /// Process the data received from the runspace pool
        /// <param name="receivedData">Data received.</param>
        internal void ProcessReceivedData(RemoteDataObject<PSObject> receivedData)
            // verify if this data structure handler is the intended recipient
            if (receivedData.RunspacePoolId != _clientRunspacePoolId)
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.RunspaceIdsDoNotMatch,
                                receivedData.RunspacePoolId, _clientRunspacePoolId);
            // take appropriate action based on the action type
            Dbg.Assert(receivedData.TargetInterface == RemotingTargetInterface.RunspacePool,
                "Target interface is expected to be RunspacePool");
            switch (receivedData.DataType)
                case RemotingDataType.RemoteHostCallUsingRunspaceHost:
                        Dbg.Assert(RemoteHostCallReceived != null,
                            "RemoteRunspacePoolInternal should subscribe to all data structure handler events");
                        RemoteHostCall remoteHostCall = RemoteHostCall.Decode(receivedData.Data);
                        RemoteHostCallReceived.SafeInvoke(this, new RemoteDataEventArgs<RemoteHostCall>(remoteHostCall));
                case RemotingDataType.RunspacePoolInitData:
                        RunspacePoolInitInfo initInfo = RemotingDecoder.GetRunspacePoolInitInfo(receivedData.Data);
                        Dbg.Assert(RSPoolInitInfoReceived != null,
                        RSPoolInitInfoReceived.SafeInvoke(this,
                            new RemoteDataEventArgs<RunspacePoolInitInfo>(initInfo));
                case RemotingDataType.RunspacePoolStateInfo:
                        RunspacePoolStateInfo stateInfo =
                            RemotingDecoder.GetRunspacePoolStateInfo(receivedData.Data);
                        Dbg.Assert(StateInfoReceived != null,
                        StateInfoReceived.SafeInvoke(this,
                            new RemoteDataEventArgs<RunspacePoolStateInfo>(stateInfo));
                        NotifyAssociatedPowerShells(stateInfo);
                case RemotingDataType.ApplicationPrivateData:
                        PSPrimitiveDictionary applicationPrivateData = RemotingDecoder.GetApplicationPrivateData(receivedData.Data);
                        Dbg.Assert(ApplicationPrivateDataReceived != null,
                        ApplicationPrivateDataReceived.SafeInvoke(this,
                            new RemoteDataEventArgs<PSPrimitiveDictionary>(applicationPrivateData));
                case RemotingDataType.RunspacePoolOperationResponse:
                        Dbg.Assert(SetMaxMinRunspacesResponseReceived != null,
                        SetMaxMinRunspacesResponseReceived.SafeInvoke(this, new RemoteDataEventArgs<PSObject>(receivedData.Data));
                case RemotingDataType.PSEventArgs:
                        PSEventArgs psEventArgs = RemotingDecoder.GetPSEventArgs(receivedData.Data);
                        Dbg.Assert(PSEventArgsReceived != null,
                        PSEventArgsReceived.SafeInvoke(this, new RemoteDataEventArgs<PSEventArgs>(psEventArgs));
        /// Creates a PowerShell data structure handler instance associated
        /// with this runspace pool data structure handler.
        /// <param name="shell">Associated powershell.</param>
        /// <returns>PowerShell data structure handler object.</returns>
        internal ClientPowerShellDataStructureHandler CreatePowerShellDataStructureHandler(
            ClientRemotePowerShell shell)
            BaseClientCommandTransportManager clientTransportMgr =
                RemoteSession.SessionDataStructureHandler.CreateClientCommandTransportManager(shell, shell.NoInput);
            return new ClientPowerShellDataStructureHandler(
                clientTransportMgr, _clientRunspacePoolId, shell.InstanceId);
        /// Creates a PowerShell instances on the server, associates it
        /// with this runspace pool and invokes.
        /// <param name="shell">The client remote powershell.</param>
            // add to associated powershell list and send request to server
            lock (_associationSyncObject)
                _associatedPowerShellDSHandlers.Add(shell.InstanceId, shell.DataStructureHandler);
            shell.DataStructureHandler.RemoveAssociation += HandleRemoveAssociation;
            // Find out if this is an invoke and disconnect operation and if so whether the endpoint
            // supports disconnect.  Throw exception if disconnect is not supported.
            bool invokeAndDisconnect = shell.Settings != null && shell.Settings.InvokeAndDisconnect;
            if (invokeAndDisconnect && !EndpointSupportsDisconnect)
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.EndpointDoesNotSupportDisconnect);
            if (RemoteSession == null)
                throw new ObjectDisposedException("ClientRunspacePoolDataStructureHandler");
            shell.DataStructureHandler.Start(RemoteSession.SessionDataStructureHandler.StateMachine, invokeAndDisconnect);
        /// Add a ClientPowerShellDataStructureHandler to association list.
                // Remove old DSHandler and replace with new.
                _associatedPowerShellDSHandlers[psShellInstanceId] = psDSHandler;
            psDSHandler.RemoveAssociation += HandleRemoveAssociation;
        /// Dispatch the message to the associated powershell data structure handler.
        /// <param name="rcvdData">Message received.</param>
        internal void DispatchMessageToPowerShell(RemoteDataObject<PSObject> rcvdData)
            ClientPowerShellDataStructureHandler dsHandler =
                GetAssociatedPowerShellDataStructureHandler(rcvdData.PowerShellId);
            // if a data structure handler does not exist it means
            // the association has been removed -
            // discard messages
            dsHandler?.ProcessReceivedData(rcvdData);
        /// Send the host response to the server.
        /// <param name="hostResponse">Host response object to send.</param>
        internal void SendHostResponseToServer(RemoteHostResponse hostResponse)
            SendDataAsync(hostResponse.Encode(), DataPriorityType.PromptResponse);
        /// Send a message to the server instructing it to reset its runspace state.
        /// <param name="callId">Caller Id.</param>
        internal void SendResetRunspaceStateToServer(long callId)
            RemoteDataObject message =
                RemotingEncoder.GenerateResetRunspaceState(_clientRunspacePoolId, callId);
            SendDataAsync(message);
        /// Sent a message to modify the max runspaces of the runspace pool.
        /// <param name="maxRunspaces">New maxrunspaces to set.</param>
        /// <param name="callId">call id on which the calling method will
        /// be blocked on</param>
        internal void SendSetMaxRunspacesToServer(int maxRunspaces, long callId)
                RemotingEncoder.GenerateSetMaxRunspaces(_clientRunspacePoolId, maxRunspaces, callId);
        /// Send a message to modify the min runspaces of the runspace pool.
        /// <param name="minRunspaces">New minrunspaces to set.</param>
        internal void SendSetMinRunspacesToServer(int minRunspaces, long callId)
                RemotingEncoder.GenerateSetMinRunspaces(_clientRunspacePoolId, minRunspaces, callId);
        /// Send a message to get the available runspaces from the server.
        internal void SendGetAvailableRunspacesToServer(long callId)
            SendDataAsync(RemotingEncoder.GenerateGetAvailableRunspaces(_clientRunspacePoolId, callId));
        #endregion Data Structure Handler Methods
        #region Data Structure Handler events
        /// Event raised when a host call is received.
        internal event EventHandler<RemoteDataEventArgs<RemoteHostCall>> RemoteHostCallReceived;
        /// Event raised when state information is received.
        internal event EventHandler<RemoteDataEventArgs<RunspacePoolStateInfo>> StateInfoReceived;
        /// Event raised when RunspacePoolInitInfo is received. This is the first runspace pool message expected
        /// after connecting to an existing remote runspace pool. RemoteRunspacePoolInternal should use this
        /// notification to set the state of a reconstructed runspace to "Opened State" and use the
        /// minRunspace and MaxRunspaces information to set its state.
        internal event EventHandler<RemoteDataEventArgs<RunspacePoolInitInfo>> RSPoolInitInfoReceived;
        /// Event raised when application private data is received.
        internal event EventHandler<RemoteDataEventArgs<PSPrimitiveDictionary>> ApplicationPrivateDataReceived;
        /// Event raised when a PSEventArgs is received.
        internal event EventHandler<RemoteDataEventArgs<PSEventArgs>> PSEventArgsReceived;
        /// Event raised when the session is closed.
        internal event EventHandler<RemoteDataEventArgs<Exception>> SessionClosed;
        internal event EventHandler<RemoteDataEventArgs<Exception>> SessionDisconnected;
        internal event EventHandler<RemoteDataEventArgs<Exception>> SessionReconnected;
        /// Event raised when the session is closing.
        internal event EventHandler<RemoteDataEventArgs<Exception>> SessionClosing;
        /// Event raised when a response to a SetMaxRunspaces or SetMinRunspaces call
        /// is received.
        internal event EventHandler<RemoteDataEventArgs<PSObject>> SetMaxMinRunspacesResponseReceived;
        /// Indicates that a disconnect has been initiated by the WinRM robust connections layer.
        internal event EventHandler<RemoteDataEventArgs<Exception>> SessionRCDisconnecting;
        /// Notification that session creation has completed.
        #endregion Data Structure Handler events
        /// Send the data specified as a RemoteDataObject asynchronously
        /// to the runspace pool on the remote session.
        /// <param name="data">Data to send.</param>
        /// <remarks>This overload takes a RemoteDataObject and should be
        /// the one used within the code</remarks>
        private void SendDataAsync(RemoteDataObject data)
            _transportManager.DataToBeSentCollection.Add<object>(data);
        /// Send the data asynchronously to runspace pool driver on remote
        /// session with the specified priority.
        /// <param name="data">Data to be sent to server.</param>
        /// <param name="priority">Priority with which to send data.</param>
        internal void SendDataAsync<T>(RemoteDataObject<T> data, DataPriorityType priority)
            _transportManager.DataToBeSentCollection.Add<T>(data, priority);
        /// <param name="data">Data object to send.</param>
        internal void SendDataAsync(PSObject data, DataPriorityType priority)
            RemoteDataObject<PSObject> dataToBeSent = RemoteDataObject<PSObject>.CreateFrom(RemotingDestination.Server,
                RemotingDataType.InvalidDataType, _clientRunspacePoolId, Guid.Empty, data);
            _transportManager.DataToBeSentCollection.Add<PSObject>(dataToBeSent);
        /// Create a client remote session based on the connection info.
        /// <param name="rsPoolInternal">
        /// The RunspacePool object this session should map to.
        private ClientRemoteSessionImpl CreateClientRemoteSession(
                    RemoteRunspacePoolInternal rsPoolInternal)
            ClientRemoteSession.URIDirectionReported uriRedirectionHandler =
                new ClientRemoteSession.URIDirectionReported(HandleURIDirectionReported);
            return new ClientRemoteSessionImpl(rsPoolInternal,
                                               uriRedirectionHandler);
        /// Handler for handling all session events.
        /// <param name="e">Object describing this event.</param>
        private void HandleClientRemoteSessionStateChanged(
                        object sender, RemoteSessionStateEventArgs e)
            // send create runspace request while sending negotiation packet. This will
            // save 1 network call to create a runspace on the server.
            if (e.SessionStateInfo.State == RemoteSessionState.NegotiationSending)
                if (_createRunspaceCalled)
                    // We are doing this check because Established event
                    // is raised more than once
                        // TODO: Put an assert here. NegotiationSending cannot
                        // occur multiple time in v2 remoting.
                    _createRunspaceCalled = true;
                // make client's PSVersionTable available to the server using applicationArguments
                PSPrimitiveDictionary argumentsWithVersionTable =
                    PSPrimitiveDictionary.CloneAndAddPSVersionTable(_applicationArguments);
                // send a message to the server..
                SendDataAsync(RemotingEncoder.GenerateCreateRunspacePool(
                    _clientRunspacePoolId, _minRunspaces, _maxRunspaces, RemoteSession.RemoteRunspacePoolInternal, _host,
                    argumentsWithVersionTable));
            if (e.SessionStateInfo.State == RemoteSessionState.NegotiationSendingOnConnect)
                // send connect message to the server.
                SendDataAsync(RemotingEncoder.GenerateConnectRunspacePool(
                    _clientRunspacePoolId, _minRunspaces, _maxRunspaces));
            else if (e.SessionStateInfo.State == RemoteSessionState.ClosingConnection)
                // use the first reason which caused the error
                Exception reason = _closingReason;
                if (reason == null)
                    reason = e.SessionStateInfo.Reason;
                    _closingReason = reason;
                // close transport managers of the associated commands
                List<ClientPowerShellDataStructureHandler> dsHandlers;
                    dsHandlers = new List<ClientPowerShellDataStructureHandler>(_associatedPowerShellDSHandlers.Values);
                foreach (ClientPowerShellDataStructureHandler dsHandler in dsHandlers)
                    dsHandler.CloseConnectionAsync(_closingReason);
                SessionClosing.SafeInvoke(this, new RemoteDataEventArgs<Exception>(reason));
            else if (e.SessionStateInfo.State == RemoteSessionState.Closed)
                // if there is a reason associated, then most likely the
                // runspace pool has broken, so notify accordingly
                if (reason != null)
                    NotifyAssociatedPowerShells(new RunspacePoolStateInfo(RunspacePoolState.Broken, reason));
                    // notify the associated powershells that this
                    // runspace pool has closed
                    NotifyAssociatedPowerShells(new RunspacePoolStateInfo(RunspacePoolState.Closed, reason));
                SessionClosed.SafeInvoke(this, new RemoteDataEventArgs<Exception>(reason));
            else if (e.SessionStateInfo.State == RemoteSessionState.Connected)
                // write a transfer event here
                PSEtwLog.ReplaceActivityIdForCurrentThread(_clientRunspacePoolId, PSEventId.OperationalTransferEventRunspacePool,
                    PSEventId.AnalyticTransferEventRunspacePool, PSKeyword.Runspace, PSTask.CreateRunspace);
            else if (e.SessionStateInfo.State == RemoteSessionState.Disconnected)
                NotifyAssociatedPowerShells(new RunspacePoolStateInfo(
                    RunspacePoolState.Disconnected,
                    e.SessionStateInfo.Reason));
                SessionDisconnected.SafeInvoke(this, new RemoteDataEventArgs<Exception>(e.SessionStateInfo.Reason));
            else if (_reconnecting && e.SessionStateInfo.State == RemoteSessionState.Established)
                SessionReconnected.SafeInvoke(this, new RemoteDataEventArgs<Exception>(null));
            else if (e.SessionStateInfo.State == RemoteSessionState.RCDisconnecting)
                SessionRCDisconnecting.SafeInvoke(this, new RemoteDataEventArgs<Exception>(null));
                if (e.SessionStateInfo.Reason != null)
                    _closingReason = e.SessionStateInfo.Reason;
        /// Session is reporting that URI is getting redirected.
        /// Report this information to the user by writing a warning message.
        /// <param name="newURI"></param>
        private void HandleURIDirectionReported(Uri newURI)
            URIRedirectionReported.SafeInvoke(this, new RemoteDataEventArgs<Uri>(newURI));
        /// Notifies associated powershell's of the runspace pool state change.
        /// <param name="stateInfo">state information that need to
        /// be notified</param>
        private void NotifyAssociatedPowerShells(RunspacePoolStateInfo stateInfo)
                    dsHandler.ProcessDisconnect(stateInfo);
            // if the runspace pool is broken or closed then set all
            // associated powershells to stopped
            if (stateInfo.State == RunspacePoolState.Broken || stateInfo.State == RunspacePoolState.Closed)
                    _associatedPowerShellDSHandlers.Clear();
                if (stateInfo.State == RunspacePoolState.Broken)
                    // set the state to failed, outside the lock
                        dsHandler.SetStateToFailed(stateInfo.Reason);
                else if (stateInfo.State == RunspacePoolState.Closed)
                        dsHandler.SetStateToStopped(stateInfo.Reason);
        /// Gets the ClientPowerShellDataStructureHandler instance for the specified id.
        /// <param name="clientPowerShellId">Id of the client remote powershell.</param>
        /// <returns>ClientPowerShellDataStructureHandler object.</returns>
        private ClientPowerShellDataStructureHandler GetAssociatedPowerShellDataStructureHandler
            (Guid clientPowerShellId)
            ClientPowerShellDataStructureHandler dsHandler = null;
                bool success = _associatedPowerShellDSHandlers.TryGetValue(clientPowerShellId, out dsHandler);
                    dsHandler = null;
            return dsHandler;
        /// Remove the association of the powershell from the runspace pool.
        /// <param name="e">Unused.</param>
        private void HandleRemoveAssociation(object sender, EventArgs e)
            Dbg.Assert(sender is ClientPowerShellDataStructureHandler, @"sender of the event
                must be ClientPowerShellDataStructureHandler");
                sender as ClientPowerShellDataStructureHandler;
                _associatedPowerShellDSHandlers.Remove(dsHandler.PowerShellId);
            _transportManager.RemoveCommandTransportManager(dsHandler.PowerShellId);
        /// Calls each running command Transport manager PrepareForDisconnect method.
        /// Each transport manager object will raise an event when the command/transport
        /// is ready to be disconnected.  Disconnect will begin when all is ready.
        private void PrepareForAndStartDisconnect()
            bool startDisconnectNow;
                if (_associatedPowerShellDSHandlers.Count == 0)
                    // There are no running commands associated with this runspace pool.
                    startDisconnectNow = true;
                    _preparingForDisconnectList = null;
                    // Delay starting the disconnect operation until all running commands are prepared.
                    startDisconnectNow = false;
                    // Create and fill list of active transportmanager objects to be disconnected.
                    // Add ready-for-disconnect callback handler to DSHandler transportmanager objects.
                    Dbg.Assert(_preparingForDisconnectList == null, "Cannot prepare for disconnect while disconnect is pending.");
                    _preparingForDisconnectList = new List<BaseClientCommandTransportManager>();
                    foreach (ClientPowerShellDataStructureHandler dsHandler in _associatedPowerShellDSHandlers.Values)
                        _preparingForDisconnectList.Add(dsHandler.TransportManager);
                        dsHandler.TransportManager.ReadyForDisconnect += HandleReadyForDisconnect;
            if (startDisconnectNow)
                // Ok to start on this thread.
                StartDisconnectAsync(RemoteSession);
                // Start preparation for disconnect.  The HandleReadyForDisconnect callback will be
                // called when a transportManager is ready for disconnect.
                    dsHandler.TransportManager.PrepareForDisconnect();
        /// Allows each running command to resume processing command input for when
        /// the runspacepool and running commands are connected.
        private void PrepareForConnect()
                dsHandler.TransportManager.ReadyForDisconnect -= HandleReadyForDisconnect;
                dsHandler.TransportManager.PrepareForConnect();
        /// Handler of the transport ReadyForDisconnect event.  When all command
        /// transports are ready for disconnect we can start the disconnect process.
        private void HandleReadyForDisconnect(object sender, EventArgs args)
            if (sender == null)
            BaseClientCommandTransportManager bcmdTM = (BaseClientCommandTransportManager)sender;
                // Ignore extra event calls after disconnect is started.
                if (_preparingForDisconnectList == null)
                _preparingForDisconnectList.Remove(bcmdTM);
                if (_preparingForDisconnectList.Count == 0)
                    // Start the asynchronous disconnect on a worker thread because we don't know
                    // what thread this callback is made from.  If it was made from a transport
                    // callback event then a deadlock may occur when DisconnectAsync is called on
                    // that same thread.
                    ThreadPool.QueueUserWorkItem(new WaitCallback(StartDisconnectAsync));
        /// WaitCallback method to start an asynchronous disconnect.
        private void StartDisconnectAsync(object state)
            var remoteSession = RemoteSession;
                remoteSession?.DisconnectAsync();
                // remoteSession may have already been disposed resulting in unexpected exceptions.
        /// Forwards robust connection notifications to associated PowerShell clients.
                dsHandler.ProcessRobustConnectionNotification(e);
        /// Forwards the session create completion event.
        /// <param name="sender">Transport sender.</param>
        /// <param name="eventArgs">CreateCompleteEventArgs.</param>
        private bool _createRunspaceCalled = false;
        private Exception _closingReason;
        private readonly int _minRunspaces;
        private readonly int _maxRunspaces;
        private readonly PSPrimitiveDictionary _applicationArguments;
        private readonly Dictionary<Guid, ClientPowerShellDataStructureHandler> _associatedPowerShellDSHandlers
            = new Dictionary<Guid, ClientPowerShellDataStructureHandler>();
        // data structure handlers of all ClientRemotePowerShell which are
        // associated with this runspace pool
        private readonly object _associationSyncObject = new object();
        // object to synchronize operations to above
        private readonly BaseClientSessionTransportManager _transportManager;
        // session transport manager associated with this runspace
        private List<BaseClientCommandTransportManager> _preparingForDisconnectList;
        /// The remote session associated with this runspace pool
        /// data structure handler.
        internal ClientRemoteSession RemoteSession { get; private set; }
        /// Transport manager used by this data structure handler.
        internal BaseClientSessionTransportManager TransportManager
                if (RemoteSession != null)
                    return RemoteSession.SessionDataStructureHandler.TransportManager;
        /// Returns robust connection maximum retry time in milliseconds, if supported
        /// by underlying transport manager.
                if (_transportManager != null &&
                    _transportManager is WSManClientSessionTransportManager)
                    return ((WSManClientSessionTransportManager)(_transportManager)).MaxRetryConnectionTime;
        /// Indicates whether the currently connected runspace endpoint supports
        /// disconnect/connect semantics.
        internal bool EndpointSupportsDisconnect
                WSManClientSessionTransportManager wsmanTransportManager = _transportManager as WSManClientSessionTransportManager;
                return wsmanTransportManager != null && wsmanTransportManager.SupportsDisconnect;
        /// Public interface for dispose.
                    ((ClientRemoteSessionImpl)RemoteSession).Dispose();
                    RemoteSession = null;
    /// Base class for ClientPowerShellDataStructureHandler to handle all
    /// references.
    internal sealed class ClientPowerShellDataStructureHandler
        /// This event is raised when the state of associated
        /// powershell is terminal and the runspace pool has
        /// to detach the association.
        internal event EventHandler RemoveAssociation;
        /// This event is raised when a state information object
        /// is received from the server.
        internal event EventHandler<RemoteDataEventArgs<PSInvocationStateInfo>> InvocationStateInfoReceived;
        /// This event is raised when an output object is received
        /// from the server.
        internal event EventHandler<RemoteDataEventArgs<object>> OutputReceived;
        /// This event is raised when an error record is received
        internal event EventHandler<RemoteDataEventArgs<ErrorRecord>> ErrorReceived;
        /// This event is raised when an informational message -
        /// debug, verbose, warning, progress is received from
        /// the server.
        internal event EventHandler<RemoteDataEventArgs<InformationalMessage>> InformationalMessageReceived;
        /// This event is raised when a host call is targeted to the
        /// powershell.
        /// This event is raised when a runspace pool data structure handler notifies an
        /// associated powershell data structure handler that its closed.
        internal event EventHandler<RemoteDataEventArgs<Exception>> ClosedNotificationFromRunspacePool;
        /// Event that is raised when a remote connection is successfully closed. The event is raised
        /// from a WSMan transport thread. Since this thread can hold on to a HTTP
        /// connection, the event handler should complete processing as fast as possible.
        /// Importantly the event handler should not generate any call that results in a
        /// user request like host.ReadLine().
        /// Errors (occurred during connection attempt) are reported through WSManTransportErrorOccured
        /// event.
        /// The eventhandler should make sure not to throw any exceptions.
        internal event EventHandler<EventArgs> CloseCompleted;
        /// associated powershell data structure handler that its broken.
        internal event EventHandler<RemoteDataEventArgs<Exception>> BrokenNotificationFromRunspacePool;
        /// This event is raised when reconnect async operation on the associated powershell/pipeline instance is completed.
        internal event EventHandler<RemoteDataEventArgs<Exception>> ReconnectCompleted;
        /// This event is raised when connect async operation on the associated powershell/pipeline instance is completed.
        internal event EventHandler<RemoteDataEventArgs<Exception>> ConnectCompleted;
        /// This event is raised when a Robust Connection layer notification is available.
        internal event EventHandler<ConnectionStatusEventArgs> RobustConnectionNotification;
        /// Start the command operation.
        internal void Start(ClientRemoteSessionDSHandlerStateMachine stateMachine, bool inDisconnectMode)
            // Add all callbacks to transport manager.
            SetupTransportManager(inDisconnectMode);
            TransportManager.CreateAsync();
        private void HandleDelayStreamRequestProcessed(object sender, EventArgs e)
            // client's request to start pipeline in disconnected mode has been successfully processed
            ProcessDisconnect(null);
        internal void HandleReconnectCompleted(object sender, EventArgs args)
            int currentState = Interlocked.CompareExchange(ref _connectionState, (int)connectionStates.Connected, (int)connectionStates.Reconnecting);
            ReconnectCompleted.SafeInvoke(this, new RemoteDataEventArgs<Exception>(null));
        internal void HandleConnectCompleted(object sender, EventArgs args)
            int currentState = Interlocked.CompareExchange(ref _connectionState, (int)connectionStates.Connected, (int)connectionStates.Connecting);
            ConnectCompleted.SafeInvoke(this, new RemoteDataEventArgs<Exception>(null));
        /// Handler which handles transport errors.
        internal void HandleTransportError(object sender, TransportErrorOccuredEventArgs e)
            // notify associated powershell about the error and close transport manager
            PSInvocationStateInfo stateInfo = new PSInvocationStateInfo(PSInvocationState.Failed, e.Exception);
            InvocationStateInfoReceived.SafeInvoke(this, new RemoteDataEventArgs<PSInvocationStateInfo>(stateInfo));
            // The handler to InvocationStateInfoReceived would have already
            // closed the connection. No need to do it here again
        /// Send a stop powershell message to the server.
        internal void SendStopPowerShellMessage()
            TransportManager.CryptoHelper.CompleteKeyExchange();
            TransportManager.SendStopSignal();
        /// Event that gets raised when stop signal is completed.
        private void OnSignalCompleted(object sender, EventArgs e)
            // Raise stopped event locally...By the time this event
            // is raised, the remote server would have sent state changed info.
            // A bad server may not send appropriate sate info, in which case we
            // fail safely
            PSRemotingDataStructureException exception = new PSRemotingDataStructureException(
                RemotingErrorIdStrings.PipelineStopped);
            InvocationStateInfoReceived.SafeInvoke(this,
                new RemoteDataEventArgs<PSInvocationStateInfo>(
                    new PSInvocationStateInfo(PSInvocationState.Stopped, exception)));
        /// <param name="hostResponse">Host response to send.</param>
            RemoteDataObject<PSObject> dataToBeSent =
                RemoteDataObject<PSObject>.CreateFrom(RemotingDestination.Server,
                RemotingDataType.RemotePowerShellHostResponseData,
                _clientRunspacePoolId,
                _clientPowerShellId,
                hostResponse.Encode());
            TransportManager.DataToBeSentCollection.Add<PSObject>(dataToBeSent,
                DataPriorityType.PromptResponse);
        /// Attach the specified data collection as input
        /// to the remote powershell.
        internal void SendInput(ObjectStreamBase inputstream)
            if (!inputstream.IsOpen && inputstream.Count == 0)
                // there is no input, send an end of input
                // message
                lock (_inputSyncObject)
                    // send input closed information to server
                    SendDataAsync(RemotingEncoder.GeneratePowerShellInputEnd(
                        _clientRunspacePoolId, _clientPowerShellId));
                // its possible that in client input data is written in a thread
                // other than the current thread. Since we want to write input
                // to the server in the order in which it was received, this
                // operation of writing to the server need to be synced
                // Also we need to ensure that all the data currently available
                // for enumeration are written out before any newly added data
                // is written. Hence the lock is made even before the handler is
                // registered
                    inputstream.DataReady += HandleInputDataReady;
                    WriteInput(inputstream);
            if (receivedData.PowerShellId != _clientPowerShellId)
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.PipelineIdsDoNotMatch,
                                receivedData.PowerShellId, _clientPowerShellId);
            // decode the message and take appropriate action
            Dbg.Assert(receivedData.TargetInterface == RemotingTargetInterface.PowerShell,
                "Target interface is expected to be Pipeline");
                case RemotingDataType.PowerShellStateInfo:
                        PSInvocationStateInfo stateInfo =
                            RemotingDecoder.GetPowerShellStateInfo(receivedData.Data);
                        Dbg.Assert(InvocationStateInfoReceived != null,
                            "ClientRemotePowerShell should subscribe to all data structure handler events");
                            new RemoteDataEventArgs<PSInvocationStateInfo>(stateInfo));
                case RemotingDataType.PowerShellOutput:
                        object outputObject =
                            RemotingDecoder.GetPowerShellOutput(receivedData.Data);
                        // since it is possible that powershell can have
                        // strongly typed output, origin information will
                        // not be added in this case. If a remoting cmdlet
                        // is using PowerShell, then it should take care
                        // of adding the origin information
                        Dbg.Assert(OutputReceived != null,
                        OutputReceived.SafeInvoke(this,
                            new RemoteDataEventArgs<object>(outputObject));
                case RemotingDataType.PowerShellErrorRecord:
                            RemotingDecoder.GetPowerShellError(receivedData.Data);
                        // not be added for output. Therefore, origin
                        // information will not be added for error records
                        // as well. If a remoting cmdlet
                        Dbg.Assert(ErrorReceived != null,
                        ErrorReceived.SafeInvoke(this,
                            new RemoteDataEventArgs<ErrorRecord>(errorRecord));
                        DebugRecord record = RemotingDecoder.GetPowerShellDebug(receivedData.Data);
                        InformationalMessageReceived.SafeInvoke(this,
                                new InformationalMessage(record, RemotingDataType.PowerShellDebug)));
                        VerboseRecord record = RemotingDecoder.GetPowerShellVerbose(receivedData.Data);
                                new InformationalMessage(record, RemotingDataType.PowerShellVerbose)));
                        WarningRecord record = RemotingDecoder.GetPowerShellWarning(receivedData.Data);
                                new InformationalMessage(record, RemotingDataType.PowerShellWarning)));
                        ProgressRecord record = RemotingDecoder.GetPowerShellProgress(receivedData.Data);
                                new InformationalMessage(record, RemotingDataType.PowerShellProgress)));
                        InformationRecord record = RemotingDecoder.GetPowerShellInformation(receivedData.Data);
                                new InformationalMessage(record, RemotingDataType.PowerShellInformationStream)));
                case RemotingDataType.RemoteHostCallUsingPowerShellHost:
                        HostCallReceived.SafeInvoke(this, new RemoteDataEventArgs<RemoteHostCall>(remoteHostCall));
                        Dbg.Assert(false, "we should not be encountering this");
        /// Set the state of the associated powershell to stopped.
        /// <param name="reason">reason why this state change
        /// should occur</param>
        /// <remarks>This method is called by the associated
        /// runspace pool data structure handler when the server runspace pool
        /// goes into a closed or broken state</remarks>
        internal void SetStateToFailed(Exception reason)
            Dbg.Assert(BrokenNotificationFromRunspacePool != null,
            BrokenNotificationFromRunspacePool.SafeInvoke(this, new RemoteDataEventArgs<Exception>(reason));
        /// Sets the state of the powershell to stopped.
        /// <param name="reason">reason why the powershell has to be
        /// set to a stopped state.</param>
        internal void SetStateToStopped(Exception reason)
            Dbg.Assert(ClosedNotificationFromRunspacePool != null,
            ClosedNotificationFromRunspacePool.SafeInvoke(this, new RemoteDataEventArgs<Exception>(reason));
        /// Closes transport manager.
        internal void CloseConnectionAsync(Exception sessionCloseReason)
            _sessionClosedReason = sessionCloseReason;
            // wait for the close to complete and then dispose the transport manager
            TransportManager.CloseCompleted += (object source, EventArgs args) =>
                if (CloseCompleted != null)
                    // If the provided event args are empty then call CloseCompleted with
                    // RemoteSessionStateEventArgs containing session closed reason exception.
                    EventArgs closeCompletedEventArgs = (args == EventArgs.Empty) ?
                        new RemoteSessionStateEventArgs(new RemoteSessionStateInfo(RemoteSessionState.Closed, _sessionClosedReason)) :
                        args;
                    CloseCompleted(this, closeCompletedEventArgs);
                TransportManager.Dispose();
            TransportManager.CloseAsync();
        /// Raise a remove association event. This is raised
        /// when the powershell has gone into a terminal state
        /// and the runspace pool need not maintain any further
        /// associations.
        internal void RaiseRemoveAssociationEvent()
            RemoveAssociation.SafeInvoke(this, EventArgs.Empty);
        /// Called from runspace DS handler while disconnecting
        /// This will set the state of the pipeline DS handler to disconnected.
        internal void ProcessDisconnect(RunspacePoolStateInfo rsStateInfo)
            // disconnect may be called on a pipeline that is already disconnected.
                            new PSInvocationStateInfo(PSInvocationState.Disconnected,
                                rsStateInfo?.Reason);
            Interlocked.CompareExchange(ref _connectionState, (int)connectionStates.Disconnected, (int)connectionStates.Connected);
        /// This does not ensure that the corresponding session/runspacepool is in connected stated
        /// It's the caller responsibility to ensure that this is the case
        /// At the protocols layers, this logic is delegated to the transport layer.
        /// WSMan transport ensures that WinRS commands cannot be reconnected when the parent shell is not in connected state.
        internal void ReconnectAsync()
            int currentState = Interlocked.CompareExchange(ref _connectionState, (int)connectionStates.Reconnecting, (int)connectionStates.Disconnected);
            if ((currentState != (int)connectionStates.Disconnected))
                Dbg.Assert(false, "Pipeline DS Handler is in unexpected connection state");
                // TODO: Raise appropriate exception
            TransportManager.ReconnectAsync();
        // Called from session DSHandler. Connects to a remote powershell instance.
            int currentState = Interlocked.CompareExchange(ref _connectionState, (int)connectionStates.Connecting, (int)connectionStates.Disconnected);
            // Connect is called for *reconstruct* connection case and so
            // we need to set up all transport manager callbacks.
            SetupTransportManager(false);
            TransportManager.ConnectAsync();
        /// Called from session DSHandler.  Notify client of robust connection
        internal void ProcessRobustConnectionNotification(
            // Raise event for PowerShell client.
            RobustConnectionNotification.SafeInvoke(this, e);
        /// Default internal constructor.
        /// <param name="clientRunspacePoolId">id of the client
        /// remote runspace pool associated with this data structure handler
        /// <param name="clientPowerShellId">id of the client
        /// powershell associated with this data structure handler</param>
        /// <param name="transportManager">transport manager associated
        /// with this connection</param>
        internal ClientPowerShellDataStructureHandler(BaseClientCommandTransportManager transportManager,
                    Guid clientRunspacePoolId, Guid clientPowerShellId)
            TransportManager = transportManager;
            transportManager.SignalCompleted += OnSignalCompleted;
        /// Client PowerShell Id of the powershell this
        /// data structure handler is associated with.
        internal Guid PowerShellId
                return _clientPowerShellId;
        internal BaseClientCommandTransportManager TransportManager { get; }
        /// to the powershell on server.
            RemoteDataObject<object> dataToBeSent = (RemoteDataObject<object>)data;
            TransportManager.DataToBeSentCollection.Add<object>(dataToBeSent);
        /// Handle data added to input.
        /// <param name="e">Information describing this event.</param>
        private void HandleInputDataReady(object sender, EventArgs e)
            // make sure only one thread calls the WriteInput.
                ObjectStreamBase inputstream = sender as ObjectStreamBase;
        /// <remarks>This method doesn't lock and its the responsibility
        /// of the caller to actually do the locking</remarks>
        private void WriteInput(ObjectStreamBase inputstream)
            Collection<object> inputObjects = inputstream.ObjectReader.NonBlockingRead(Int32.MaxValue);
                SendDataAsync(RemotingEncoder.GeneratePowerShellInput(inputObject,
            if (!inputstream.IsOpen)
                // Write any data written after the NonBlockingRead call above.
                inputObjects = inputstream.ObjectReader.NonBlockingRead(Int32.MaxValue);
                // we are sending input end to the server. Ignore the future
                // DataReady events (A DataReady event is raised while Closing
                // the stream as well)
                inputstream.DataReady -= HandleInputDataReady;
                // stream close: send end of input
        /// Helper method to add transport manager callbacks and set transport
        /// manager disconnected state.
        /// <param name="inDisconnectMode">Boolean.</param>
        private void SetupTransportManager(bool inDisconnectMode)
            TransportManager.WSManTransportErrorOccured += HandleTransportError;
            TransportManager.ReconnectCompleted += HandleReconnectCompleted;
            TransportManager.ConnectCompleted += HandleConnectCompleted;
            TransportManager.DelayStreamRequestProcessed += HandleDelayStreamRequestProcessed;
            TransportManager.startInDisconnectedMode = inDisconnectMode;
        // object for synchronizing input to be sent
        // to server powershell
        private readonly object _inputSyncObject = new object();
        private enum connectionStates
            Connected = 1, Disconnected = 3, Reconnecting = 4, Connecting = 5
        private int _connectionState = (int)connectionStates.Connected;
        // Contains the associated session closed reason exception if any,
        // otherwise is null.
        private Exception _sessionClosedReason;
    internal sealed class InformationalMessage
        internal object Message { get; }
        internal RemotingDataType DataType { get; }
        internal InformationalMessage(object message, RemotingDataType dataType)
            DataType = dataType;

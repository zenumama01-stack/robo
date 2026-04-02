    /// Implements ServerRemoteSessionDataStructureHandler.
    internal sealed class ClientRemoteSessionDSHandlerImpl : ClientRemoteSessionDataStructureHandler, IDisposable
        [TraceSource("CRSDSHdlerImpl", "ClientRemoteSessionDSHandlerImpl")]
        private static readonly PSTraceSource s_trace = PSTraceSource.GetTracer("CRSDSHdlerImpl", "ClientRemoteSessionDSHandlerImpl");
        private const string resBaseName = "remotingerroridstrings";
        private readonly ClientRemoteSessionDSHandlerStateMachine _stateMachine;
        private readonly ClientRemoteSession _session;
        // used for connection redirection.
        private Uri _redirectUri;
        private int _maxUriRedirectionCount;
        private bool _isCloseCalled;
        private readonly PSRemotingCryptoHelper _cryptoHelper;
        private readonly ClientRemoteSession.URIDirectionReported _uriRedirectionHandler;
        internal override BaseClientSessionTransportManager TransportManager
                return _transportManager;
        internal override BaseClientCommandTransportManager CreateClientCommandTransportManager(
            bool noInput)
            BaseClientCommandTransportManager cmdTransportMgr =
                _transportManager.CreateClientCommandTransportManager(_connectionInfo, cmd, noInput);
            // listen to data ready events.
            cmdTransportMgr.DataReceived += DispatchInputQueueData;
            return cmdTransportMgr;
        /// Creates an instance of ClientRemoteSessionDSHandlerImpl.
        internal ClientRemoteSessionDSHandlerImpl(ClientRemoteSession session,
            PSRemotingCryptoHelper cryptoHelper,
            ClientRemoteSession.URIDirectionReported uriRedirectionHandler)
            Dbg.Assert(_maxUriRedirectionCount >= 0, "maxUriRedirectionCount cannot be less than 0.");
            if (session == null)
                throw PSTraceSource.NewArgumentNullException(nameof(session));
            _session = session;
            // Create state machine
            _stateMachine = new ClientRemoteSessionDSHandlerStateMachine();
            _stateMachine.StateChanged += HandleStateChanged;
            _connectionInfo = connectionInfo;
            // Create transport manager
            _cryptoHelper = cryptoHelper;
            _transportManager = _connectionInfo.CreateClientSessionTransportManager(
                _session.RemoteRunspacePoolInternal.InstanceId,
                _session.RemoteRunspacePoolInternal.Name,
                cryptoHelper);
            _transportManager.DataReceived += DispatchInputQueueData;
            _transportManager.WSManTransportErrorOccured += HandleTransportError;
            _transportManager.CloseCompleted += HandleCloseComplete;
            _transportManager.DisconnectCompleted += HandleDisconnectComplete;
            _transportManager.ReconnectCompleted += HandleReconnectComplete;
                // only WSMan transport supports redirection
                // store the uri redirection handler and authmechanism
                // for uri redirection.
                _uriRedirectionHandler = uriRedirectionHandler;
                _maxUriRedirectionCount = wsmanConnectionInfo.MaximumConnectionRedirectionCount;
        #region create
        /// Makes a create call asynchronously.
        internal override void CreateAsync()
            // errors are reported through WSManTransportErrorOccured event on
            // the transport manager.
            _transportManager.CreateCompleted += HandleCreateComplete;
            _transportManager.CreateAsync();
        /// This callback is called on complete of async connect call.
        private void HandleCreateComplete(object sender, EventArgs args)
            // This is a no-op at the moment..as we dont need to inform anything to
            // state machine here..StateMachine must already have reached NegotiationSent
            // state and waiting for Negotiation Received which will happen only from
            // DataReceived event.
        private void HandleConnectComplete(object sender, EventArgs args)
            // No-OP. Once the negotiation messages are exchanged and the session gets into established state,
            // it will take care of spawning the receive operation on the connected session
            // There is however a caveat.
            // A rouge remote server if it does not send the required negotiation data in the Connect Response,
            // then the state machine can never get into the established state and the runspace can never get into a opened state
            // Living with this for now.
        #endregion create
        #region disconnect
        internal override void DisconnectAsync()
            _transportManager.DisconnectAsync();
        private void HandleDisconnectComplete(object sender, EventArgs args)
            // Set statemachine event
            RemoteSessionStateMachineEventArgs disconnectCompletedArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.DisconnectCompleted);
            StateMachine.RaiseEvent(disconnectCompletedArg);
        #endregion disconnect
        #region RobustConnection events
        private void HandleRobustConnectionNotification(object sender, ConnectionStatusEventArgs e)
            RemoteSessionStateMachineEventArgs eventArgument = null;
                    eventArgument = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.RCDisconnectStarted);
                    eventArgument = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.DisconnectCompleted,
                        new RuntimeException(
                                _session.RemoteRunspacePoolInternal.ConnectionInfo.ComputerName)));
                    eventArgument = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.FatalError);
            if (eventArgument != null)
                StateMachine.RaiseEvent(eventArgument);
        #region reconnect
        internal override void ReconnectAsync()
            _transportManager.ReconnectAsync();
        private void HandleReconnectComplete(object sender, EventArgs args)
            RemoteSessionStateMachineEventArgs reconnectCompletedArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.ReconnectCompleted);
            StateMachine.RaiseEvent(reconnectCompletedArg);
        #endregion reconnect
        /// Close the connection asynchronously.
        internal override void CloseConnectionAsync()
                if (_isCloseCalled)
                _transportManager.CloseAsync();
                _isCloseCalled = true;
        private void HandleCloseComplete(object sender, EventArgs args)
            // This event gets raised only when the connection is closed successfully.
            RemoteSessionStateMachineEventArgs closeCompletedArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.CloseCompleted);
            _stateMachine.RaiseEvent(closeCompletedArg);
        /// Sends the negotiation package asynchronously.
        internal override void SendNegotiationAsync(RemoteSessionState sessionState)
            // This state change is made before the call to CreateAsync to ensure the state machine
            // is prepared for a NegotiationReceived response.  Otherwise a race condition can
            // occur when the transport NegotiationReceived arrives too soon, breaking the session.
            // This race condition was observed for OutOfProc transport when reusing the OutOfProc process.
            // this will change StateMachine to NegotiationSent.
            RemoteSessionStateMachineEventArgs negotiationSendCompletedArg =
                new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationSendCompleted);
            _stateMachine.RaiseEvent(negotiationSendCompletedArg);
            if (sessionState == RemoteSessionState.NegotiationSending)
            else if (sessionState == RemoteSessionState.NegotiationSendingOnConnect)
                _transportManager.ConnectCompleted += HandleConnectComplete;
                _transportManager.ConnectAsync();
                Dbg.Assert(false, "SendNegotiationAsync called in unexpected session state");
        internal override event EventHandler<RemoteSessionNegotiationEventArgs> NegotiationReceived;
        #region state change
        /// This event indicates that the connection state has changed.
        internal override event EventHandler<RemoteSessionStateEventArgs> ConnectionStateChanged;
        private void HandleStateChanged(object sender, RemoteSessionStateEventArgs arg)
            // Enqueue session related negotiation packets first
            if ((arg.SessionStateInfo.State == RemoteSessionState.NegotiationSending) || (arg.SessionStateInfo.State == RemoteSessionState.NegotiationSendingOnConnect))
                HandleNegotiationSendingStateChange();
            // this will enable top-layers to enqueue any packets during NegotiationSending and
            // during other states.
            ConnectionStateChanged.SafeInvoke(this, arg);
                SendNegotiationAsync(arg.SessionStateInfo.State);
            // once session is established.. start receiving data (if not already done and only apples to wsmanclientsessionTM)
            if (arg.SessionStateInfo.State == RemoteSessionState.Established)
                WSManClientSessionTransportManager tm = _transportManager as WSManClientSessionTransportManager;
                if (tm != null)
                    tm.AdjustForProtocolVariations(_session.ServerProtocolVersion);
                    tm.StartReceivingData();
            // Close the transport manager only after powershell's close their transports
            // Powershell's close their transport using the ConnectionStateChanged event notification.
                CloseConnectionAsync();
            // process disconnect
            if (arg.SessionStateInfo.State == RemoteSessionState.Disconnecting)
                DisconnectAsync();
            // process reconnect
            if (arg.SessionStateInfo.State == RemoteSessionState.Reconnecting)
                ReconnectAsync();
        /// Clubbing negotiation packet + runspace creation and then doing transportManager.ConnectAsync().
        /// This will save us 2 network calls by doing all the work in one network call.
        private void HandleNegotiationSendingStateChange()
            RemoteSessionCapability clientCapability = _session.Context.ClientCapability;
            Dbg.Assert(clientCapability.RemotingDestination == RemotingDestination.Server, "Expected clientCapability.RemotingDestination == RemotingDestination.Server");
            // Encode and send the negotiation reply
            RemoteDataObject data = RemotingEncoder.GenerateClientSessionCapability(
                                        clientCapability, _session.RemoteRunspacePoolInternal.InstanceId);
            RemoteDataObject<PSObject> dataAsPSObject = RemoteDataObject<PSObject>.CreateFrom(
                data.Destination, data.DataType, data.RunspacePoolId, data.PowerShellId, (PSObject)data.Data);
            _transportManager.DataToBeSentCollection.Add<PSObject>(dataAsPSObject);
        #endregion state change
        internal override ClientRemoteSessionDSHandlerStateMachine StateMachine
                return _stateMachine;
        /// Transport reported an error saying that uri is redirected. This method
        /// will perform the redirection to the new URI by doing the following:
        /// 1. Close the current transport manager to clean resources
        /// 2. Raise a warning that URI is getting redirected.
        /// 3. Using the new URI, ask the same transport manager to redirect
        /// Step 1 is performed here. Step2-3 is performed in another method.
        /// <param name="newURIString"></param>
        /// newURIString is a null reference.
        /// <exception cref="UriFormatException">
        /// uriString is empty.
        /// The scheme specified in uriString is invalid.
        /// uriString contains too many slashes.
        /// The password specified in uriString is invalid.
        /// The host name specified in uriString is invalid.
        private void PerformURIRedirection(string newURIString)
            _redirectUri = new Uri(newURIString);
            // make sure connection is not closed while we are handling the redirection.
                // if connection is closed by the user..no need to redirect
                // clear our current close complete & Error handlers
                _transportManager.CloseCompleted -= HandleCloseComplete;
                _transportManager.WSManTransportErrorOccured -= HandleTransportError;
                // perform other steps only after transport manager is closed.
                _transportManager.CloseCompleted += HandleTransportCloseCompleteForRedirection;
                // Handle errors happened while redirecting differently..We need to reset the
                // original handlers in this case.
                _transportManager.WSManTransportErrorOccured += HandleTransportErrorForRedirection;
                _transportManager.PrepareForRedirection();
        private void HandleTransportCloseCompleteForRedirection(object source, EventArgs args)
            _transportManager.CloseCompleted -= HandleTransportCloseCompleteForRedirection;
            _transportManager.WSManTransportErrorOccured -= HandleTransportErrorForRedirection;
            // reattach the close complete and error handlers
            PerformURIRedirectionStep2(_redirectUri);
        private void HandleTransportErrorForRedirection(object sender, TransportErrorOccuredEventArgs e)
            HandleTransportError(sender, e);
        /// This is step 2 of URI redirection. This is called after the current transport manager
        /// is closed. This is usually called from the close complete callback.
        private void PerformURIRedirectionStep2(System.Uri newURI)
            Dbg.Assert(newURI != null, "Uri cannot be null");
                // raise warning to report the redirection
                _uriRedirectionHandler?.Invoke(newURI);
                // start a new connection
                _transportManager.Redirect(newURI, _connectionInfo);
        #region data handling
            Dbg.Assert(e != null, "HandleTransportError expects non-null eventargs");
            // handle uri redirections
            PSRemotingTransportRedirectException redirectException = e.Exception as PSRemotingTransportRedirectException;
            if ((redirectException != null) && (_maxUriRedirectionCount > 0))
                    // honor max redirection count given by the user.
                    _maxUriRedirectionCount--;
                    PerformURIRedirection(redirectException.RedirectLocation);
                catch (ArgumentNullException argumentException)
                    exception = argumentException;
                catch (UriFormatException uriFormatException)
                    exception = uriFormatException;
                // if we are here, there must be an exception constructing a uri
                    PSRemotingTransportException newException =
                        new PSRemotingTransportException(PSRemotingErrorId.RedirectedURINotWellFormatted, RemotingErrorIdStrings.RedirectedURINotWellFormatted,
                            _session.Context.RemoteAddress.OriginalString,
                            redirectException.RedirectLocation);
                    newException.TransportMessage = e.Exception.TransportMessage;
                    e.Exception = newException;
            RemoteSessionEvent sessionEvent = RemoteSessionEvent.ConnectFailed;
            switch (e.ReportingTransportMethod)
                case TransportMethodEnum.CreateShellEx:
                    sessionEvent = RemoteSessionEvent.ConnectFailed;
                case TransportMethodEnum.SendShellInputEx:
                case TransportMethodEnum.CommandInputEx:
                    sessionEvent = RemoteSessionEvent.SendFailed;
                case TransportMethodEnum.ReceiveShellOutputEx:
                case TransportMethodEnum.ReceiveCommandOutputEx:
                    sessionEvent = RemoteSessionEvent.ReceiveFailed;
                case TransportMethodEnum.CloseShellOperationEx:
                    sessionEvent = RemoteSessionEvent.CloseFailed;
                case TransportMethodEnum.DisconnectShellEx:
                    sessionEvent = RemoteSessionEvent.DisconnectFailed;
                case TransportMethodEnum.ReconnectShellEx:
                    sessionEvent = RemoteSessionEvent.ReconnectFailed;
            RemoteSessionStateMachineEventArgs errorArgs =
                new RemoteSessionStateMachineEventArgs(sessionEvent, e.Exception);
            _stateMachine.RaiseEvent(errorArgs);
        /// Dispatches data when it arrives from the input queue.
        /// <param name="dataArg">
        /// arg which contains the data received from input queue
        internal void DispatchInputQueueData(object sender, RemoteDataEventArgs dataArg)
            if (dataArg == null)
                throw PSTraceSource.NewArgumentNullException(nameof(dataArg));
            RemoteDataObject<PSObject> rcvdData = dataArg.ReceivedData;
            if (rcvdData == null)
                throw PSTraceSource.NewArgumentException(nameof(dataArg));
            RemotingDestination destination = rcvdData.Destination;
            if ((destination & RemotingDestination.Client) != RemotingDestination.Client)
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.RemotingDestinationNotForMe, RemotingDestination.Client, destination);
            RemotingTargetInterface targetInterface = rcvdData.TargetInterface;
            switch (targetInterface)
                case RemotingTargetInterface.Session:
                        // Messages for session can cause statemachine state to change.
                        // These messages are first processed by Sessiondata structure handler and depending
                        // on the type of message, appropriate event is raised in state machine
                        ProcessSessionMessages(dataArg);
                case RemotingTargetInterface.RunspacePool:
                case RemotingTargetInterface.PowerShell:
                    // Non Session messages do not change the state of the statemachine.
                    // However instead of forwarding them to Runspace/pipeline here, an
                    // event is raised in state machine which verified that state is
                    // suitable for accepting these messages. if state is suitable statemachine
                    // will call DoMessageForwading which will forward the messages appropriately
                    RemoteSessionStateMachineEventArgs msgRcvArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.MessageReceived, null);
                    if (StateMachine.CanByPassRaiseEvent(msgRcvArg))
                        ProcessNonSessionMessages(dataArg.ReceivedData);
                        StateMachine.RaiseEvent(msgRcvArg);
        // TODO: If this is not used remove this
        // internal override event EventHandler<RemoteDataEventArgs> DataReceived;
        /// This processes the object received from transport which are
        /// targeted for session.
        /// argument contains the data object
        private void ProcessSessionMessages(RemoteDataEventArgs arg)
            if (arg == null || arg.ReceivedData == null)
            RemoteDataObject<PSObject> rcvdData = arg.ReceivedData;
            Dbg.Assert(targetInterface == RemotingTargetInterface.Session, "targetInterface must be Session");
            RemotingDataType dataType = rcvdData.DataType;
            switch (dataType)
                case RemotingDataType.CloseSession:
                    PSRemotingDataStructureException reasonOfClose = new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerRequestedToCloseSession);
                    RemoteSessionStateMachineEventArgs closeSessionArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close, reasonOfClose);
                    _stateMachine.RaiseEvent(closeSessionArg);
                case RemotingDataType.SessionCapability:
                    RemoteSessionCapability capability = null;
                        capability = RemotingDecoder.GetSessionCapability(rcvdData.Data);
                        // this will happen if expected properties are not
                        // received for session capability
                        throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ClientNotFoundCapabilityProperties,
                            dse.Message, PSVersionInfo.GitCommitId, RemotingConstants.ProtocolVersion);
                    RemoteSessionStateMachineEventArgs capabilityArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationReceived);
                    capabilityArg.RemoteSessionCapability = capability;
                    _stateMachine.RaiseEvent(capabilityArg);
                    RemoteSessionNegotiationEventArgs negotiationArg = new RemoteSessionNegotiationEventArgs(capability);
                    NegotiationReceived.SafeInvoke(this, negotiationArg);
                case RemotingDataType.EncryptedSessionKey:
                        string encryptedSessionKey = RemotingDecoder.GetEncryptedSessionKey(rcvdData.Data);
                        EncryptedSessionKeyReceived.SafeInvoke(this, new RemoteDataEventArgs<string>(encryptedSessionKey));
                case RemotingDataType.PublicKeyRequest:
                        PublicKeyRequestReceived.SafeInvoke(this, new RemoteDataEventArgs<string>(string.Empty));
                        throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ReceivedUnsupportedAction, dataType);
        /// not targeted for session.
        /// <param name="rcvdData">
        /// received data.
        internal void ProcessNonSessionMessages(RemoteDataObject<PSObject> rcvdData)
            // TODO: Consider changing to Dbg.Assert()
                throw PSTraceSource.NewArgumentNullException(nameof(rcvdData));
            Guid clientRunspacePoolId;
            RemoteRunspacePoolInternal runspacePool;
                        "The session remote data is handled my session data structure handler, not here");
                    clientRunspacePoolId = rcvdData.RunspacePoolId;
                    runspacePool = _session.GetRunspacePool(clientRunspacePoolId);
                    if (runspacePool != null)
                        // GETBACK
                        runspacePool.DataStructureHandler.ProcessReceivedData(rcvdData);
                        // The runspace pool may have been removed on the client side,
                        // so, we should just ignore the message.
                        s_trace.WriteLine(@"Client received data for Runspace (id: {0}),
                            but the Runspace cannot be found", clientRunspacePoolId);
                    runspacePool.DataStructureHandler.DispatchMessageToPowerShell(rcvdData);
        #endregion data handling
            _transportManager.Dispose();
        #region Key Exchange
        internal override event EventHandler<RemoteDataEventArgs<string>> EncryptedSessionKeyReceived;
        internal override event EventHandler<RemoteDataEventArgs<string>> PublicKeyRequestReceived;
        /// Send the specified local public key to the remote end.
        /// <param name="localPublicKey">Local public key as a string.</param>
        internal override void SendPublicKeyAsync(string localPublicKey)
            _transportManager.DataToBeSentCollection.Add<object>(
                RemotingEncoder.GenerateMyPublicKey(_session.RemoteRunspacePoolInternal.InstanceId,
                    localPublicKey, RemotingDestination.Server));
        /// Raise the public key received event.
        /// <param name="receivedData">Received data.</param>
        /// <remarks>This method is a hook to be called
        /// from the transport manager</remarks>
        internal override void RaiseKeyExchangeMessageReceived(RemoteDataObject<PSObject> receivedData)
            ProcessSessionMessages(new RemoteDataEventArgs(receivedData));
        #endregion Key Exchange

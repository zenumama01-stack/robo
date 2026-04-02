    /// This class is designed to contains the pertinent information about a Remote Connection,
    /// such as remote computer name, remote user name etc.
    /// It is also used to access remote connection capability and configuration information.
    /// Currently the session is identified by the InstanceId of the runspacePool associated with it
    /// This can change in future if we start supporting multiple runspacePools per session.
    internal class ClientRemoteSessionContext
        /// Remote computer address in URI format.
        internal Uri RemoteAddress { get; set; }
        /// User credential to be used on the remote computer.
        internal PSCredential UserCredential { get; set; }
        /// Capability information for the client side.
        internal RemoteSessionCapability ClientCapability { get; set; }
        /// Capability information received from the server side.
        internal RemoteSessionCapability ServerCapability { get; set; }
        /// This is the shellName which identifies the PowerShell configuration to launch
        /// on remote machine.
        internal string ShellName { get; set; }
        #endregion Public_Properties
    /// This abstract class defines the client view of the remote connection.
    internal abstract class ClientRemoteSession : RemoteSession
        [TraceSource("CRSession", "ClientRemoteSession")]
        private static readonly PSTraceSource s_trace = PSTraceSource.GetTracer("CRSession", "ClientRemoteSession");
        #region Public_Method_API
        /// Client side user calls this function to create a new remote session.
        /// User needs to register event handler to ConnectionEstablished and ConnectionClosed to
        /// monitor the actual connection state.
        public abstract void CreateAsync();
        /// This event handler is raised when the state of session changes.
        public abstract event EventHandler<RemoteSessionStateEventArgs> StateChanged;
        /// Close the connection to the remote computer in an asynchronous manner.
        /// Client side user can register an event handler with ConnectionClosed to monitor
        /// the connection state.
        /// Disconnects the remote session in an asynchronous manner.
        /// Reconnects the remote session in an asynchronous manner.
        public abstract void ReconnectAsync();
        /// Connects to an existing remote session
        #endregion Public_Method_API
        #region Public_Properties
        internal ClientRemoteSessionContext Context { get; } = new ClientRemoteSessionContext();
        #region URI Redirection
        /// Delegate used to report connection URI redirections to the application.
        /// <param name="newURI">
        /// New URI to which the connection is being redirected to.
        internal delegate void URIDirectionReported(Uri newURI);
        /// ServerRemoteSessionDataStructureHandler instance for this session.
        internal ClientRemoteSessionDataStructureHandler SessionDataStructureHandler { get; set; }
        protected Version _serverProtocolVersion;
        /// Protocol version negotiated by the server.
        internal Version ServerProtocolVersion
                return _serverProtocolVersion;
        private RemoteRunspacePoolInternal _remoteRunspacePool;
        /// Remote runspace pool if used, for this session.
                return _remoteRunspacePool;
                Dbg.Assert(_remoteRunspacePool == null, @"RunspacePool should be
                        attached only once to the session");
                _remoteRunspacePool = value;
        /// Get the runspace pool with the matching id.
        /// <param name="clientRunspacePoolId">
        /// Id of the runspace to get
        internal RemoteRunspacePoolInternal GetRunspacePool(Guid clientRunspacePoolId)
            if (_remoteRunspacePool != null)
                if (_remoteRunspacePool.InstanceId.Equals(clientRunspacePoolId))
    /// Remote Session Implementation.
    internal class ClientRemoteSessionImpl : ClientRemoteSession, IDisposable
        [TraceSource("CRSessionImpl", "ClientRemoteSessionImpl")]
        private static readonly PSTraceSource s_trace = PSTraceSource.GetTracer("CRSessionImpl", "ClientRemoteSessionImpl");
        private PSRemotingCryptoHelperClient _cryptoHelper = null;
        /// Creates a new instance of ClientRemoteSessionImpl.
        /// <param name="rsPool">
        /// <param name="uriRedirectionHandler">
        internal ClientRemoteSessionImpl(RemoteRunspacePoolInternal rsPool,
                                       URIDirectionReported uriRedirectionHandler)
            Dbg.Assert(rsPool != null, "RunspacePool cannot be null");
            base.RemoteRunspacePoolInternal = rsPool;
            Context.RemoteAddress = WSManConnectionInfo.ExtractPropertyAsWsManConnectionInfo<Uri>(rsPool.ConnectionInfo,
                "ConnectionUri", null);
            _cryptoHelper = new PSRemotingCryptoHelperClient();
            _cryptoHelper.Session = this;
            Context.ClientCapability = RemoteSessionCapability.CreateClientCapability();
            Context.UserCredential = rsPool.ConnectionInfo.Credential;
            // shellName validation is not performed on the client side.
            // This is recommended by the WinRS team: for the reason that the rules may change in the future.
            Context.ShellName = WSManConnectionInfo.ExtractPropertyAsWsManConnectionInfo<string>(rsPool.ConnectionInfo,
                "ShellUri", string.Empty);
            MySelf = RemotingDestination.Client;
            // Create session data structure handler for this session
            SessionDataStructureHandler = new ClientRemoteSessionDSHandlerImpl(this,
                _cryptoHelper,
                rsPool.ConnectionInfo,
            BaseSessionDataStructureHandler = SessionDataStructureHandler;
            _waitHandleForConfigurationReceived = new ManualResetEvent(false);
            // Register handlers for various ClientSessiondata structure handler events
            SessionDataStructureHandler.NegotiationReceived += HandleNegotiationReceived;
            SessionDataStructureHandler.ConnectionStateChanged += HandleConnectionStateChanged;
            SessionDataStructureHandler.EncryptedSessionKeyReceived += HandleEncryptedSessionKeyReceived;
            SessionDataStructureHandler.PublicKeyRequestReceived += HandlePublicKeyRequestReceived;
        #region connect/close
        /// Creates a Remote Session Asynchronously.
        public override void CreateAsync()
            // Raise a CreateSession event in StateMachine. This start the process of connection and negotiation to a new remote session
            RemoteSessionStateMachineEventArgs startArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.CreateSession);
            SessionDataStructureHandler.StateMachine.RaiseEvent(startArg);
        /// Connects to a existing Remote Session Asynchronously by executing a Connect negotiation algorithm.
            // Raise the connectsession event in statemachine. This start the process of connection and negotiation to an existing remote session
            RemoteSessionStateMachineEventArgs startArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.ConnectSession);
        /// Closes Session Connection Asynchronously.
        /// Caller should register for ConnectionClosed event to get notified
            RemoteSessionStateMachineEventArgs closeArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close);
            SessionDataStructureHandler.StateMachine.RaiseEvent(closeArg);
        /// Temporarily suspends connection to a connected remote session.
            RemoteSessionStateMachineEventArgs startDisconnectArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.DisconnectStart);
            SessionDataStructureHandler.StateMachine.RaiseEvent(startDisconnectArg);
        /// Restores connection to a disconnected remote session. Negotiation has already been performed before.
        public override void ReconnectAsync()
            RemoteSessionStateMachineEventArgs startReconnectArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.ReconnectStart);
            SessionDataStructureHandler.StateMachine.RaiseEvent(startReconnectArg);
        public override event EventHandler<RemoteSessionStateEventArgs> StateChanged;
        /// Handles changes in data structure handler state.
        /// Event argument which contains the new state
        private void HandleConnectionStateChanged(object sender, RemoteSessionStateEventArgs arg)
            using (s_trace.TraceEventHandlers())
                    throw PSTraceSource.NewArgumentNullException(nameof(arg));
                if (arg.SessionStateInfo.State == RemoteSessionState.EstablishedAndKeyReceived) // TODO - Client session would never get into this state... to be removed
                    // send the public key
                    StartKeyExchange();
                if (arg.SessionStateInfo.State == RemoteSessionState.ClosingConnection)
                    // when the connection is being closed we need to
                    // complete the key exchange process to release
                    // the lock under which the key exchange is happening
                    // if we fail to release the lock, then when
                    // transport manager is closing it will try to
                    // acquire the lock again leading to a deadlock
                    CompleteKeyExchange();
                StateChanged.SafeInvoke(this, arg);
        #endregion connect/closed
        #region KeyExchange
        /// Start the key exchange process.
        internal override void StartKeyExchange()
            if (SessionDataStructureHandler.StateMachine.State == RemoteSessionState.Established ||
                SessionDataStructureHandler.StateMachine.State == RemoteSessionState.EstablishedAndKeyRequested)
                // Start the key sending process
                string localPublicKey = null;
                RemoteSessionStateMachineEventArgs eventArgs = null;
                    ret = _cryptoHelper.ExportLocalPublicKey(out localPublicKey);
                catch (PSCryptoException cryptoException)
                    ret = false;
                    exception = cryptoException;
                    // we need to complete the key exchange
                    // since the crypto helper will be waiting on it
                    // exporting local public key failed
                    // set state to Closed
                    eventArgs = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeySendFailed,
                    SessionDataStructureHandler.StateMachine.RaiseEvent(eventArgs);
                    // send using data structure handler
                    eventArgs = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeySent);
                    SessionDataStructureHandler.SendPublicKeyAsync(localPublicKey);
        /// Complete the key exchange process.
        internal override void CompleteKeyExchange()
            _cryptoHelper.CompleteKeyExchange();
        /// Handles an encrypted session key received from the other side.
        /// <param name="eventArgs">arguments that contain the remote
        /// public key</param>
        private void HandleEncryptedSessionKeyReceived(object sender, RemoteDataEventArgs<string> eventArgs)
            if (SessionDataStructureHandler.StateMachine.State == RemoteSessionState.EstablishedAndKeySent)
                string encryptedSessionKey = eventArgs.Data;
                bool ret = _cryptoHelper.ImportEncryptedSessionKey(encryptedSessionKey);
                RemoteSessionStateMachineEventArgs args = null;
                    // importing remote public key failed
                    // set state to closed
                    args = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeyReceiveFailed);
                    SessionDataStructureHandler.StateMachine.RaiseEvent(args);
                // complete the key exchange process
                args = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeyReceived);
        /// Handles a request for public key from the server.
        /// <param name="sender">Send of this event, unused.</param>
        private void HandlePublicKeyRequestReceived(object sender, RemoteDataEventArgs<string> eventArgs)
            if (SessionDataStructureHandler.StateMachine.State == RemoteSessionState.Established)
                RemoteSessionStateMachineEventArgs args =
                    new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeyRequested);
        #endregion KeyExchange
        // TODO:Review Configuration Story
        #region configuration
        private ManualResetEvent _waitHandleForConfigurationReceived;
        #endregion configuration
        #region negotiation
        /// Examines the negotiation packet received from the server.
        /// <param name="arg"></param>
        private void HandleNegotiationReceived(object sender, RemoteSessionNegotiationEventArgs arg)
                if (arg.RemoteSessionCapability == null)
                    throw PSTraceSource.NewArgumentException(nameof(arg));
                Context.ServerCapability = arg.RemoteSessionCapability;
                    // This will throw if there is an error running the algorithm
                    RunClientNegotiationAlgorithm(Context.ServerCapability);
                    RemoteSessionStateMachineEventArgs negotiationCompletedArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationCompleted);
                    SessionDataStructureHandler.StateMachine.RaiseEvent(negotiationCompletedArg);
                catch (PSRemotingDataStructureException dse)
                    RemoteSessionStateMachineEventArgs negotiationFailedArg =
                        new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationFailed,
                            dse);
                    SessionDataStructureHandler.StateMachine.RaiseEvent(negotiationFailedArg);
        /// Verifies the negotiation packet received from the server.
        /// <param name="serverRemoteSessionCapability">
        /// Capabilities of remote session
        /// The method returns true if the capability negotiation is successful.
        /// Otherwise, it returns false.
        /// <exception cref="PSRemotingDataStructureException">
        /// 1. PowerShell client does not support the PSVersion {1} negotiated by the server.
        ///    Make sure the server is compatible with the build {2} of PowerShell.
        /// 2. PowerShell client does not support the SerializationVersion {1} negotiated by the server.
        private bool RunClientNegotiationAlgorithm(RemoteSessionCapability serverRemoteSessionCapability)
            Dbg.Assert(serverRemoteSessionCapability != null, "server capability cache must be non-null");
            // ProtocolVersion check
            Version serverProtocolVersion = serverRemoteSessionCapability.ProtocolVersion;
            _serverProtocolVersion = serverProtocolVersion;
            Version clientProtocolVersion = Context.ClientCapability.ProtocolVersion;
            if (clientProtocolVersion == serverProtocolVersion ||
                serverProtocolVersion == RemotingConstants.ProtocolVersion_2_0 ||
                serverProtocolVersion == RemotingConstants.ProtocolVersion_2_1 ||
                serverProtocolVersion == RemotingConstants.ProtocolVersion_2_2 ||
                serverProtocolVersion == RemotingConstants.ProtocolVersion_2_3)
                // passed negotiation check
                PSRemotingDataStructureException reasonOfFailure =
                    new PSRemotingDataStructureException(RemotingErrorIdStrings.ClientNegotiationFailed,
                        RemoteDataNameStrings.PS_STARTUP_PROTOCOL_VERSION_NAME,
                        serverProtocolVersion,
                        PSVersionInfo.GitCommitId,
                        RemotingConstants.ProtocolVersion);
                throw reasonOfFailure;
            // PSVersion check
            Version serverPSVersion = serverRemoteSessionCapability.PSVersion;
            Version clientPSVersion = Context.ClientCapability.PSVersion;
            if (!clientPSVersion.Equals(serverPSVersion))
                        RemoteDataNameStrings.PSVersion,
                        serverPSVersion.ToString(),
            // Serialization Version check
            Version serverSerVersion = serverRemoteSessionCapability.SerializationVersion;
            Version clientSerVersion = Context.ClientCapability.SerializationVersion;
            if (!clientSerVersion.Equals(serverSerVersion))
                        RemoteDataNameStrings.SerializationVersion,
                        serverSerVersion.ToString(),
        #endregion negotiation
        internal override RemotingDestination MySelf { get; }
        /// Public method for dispose.
                if (_waitHandleForConfigurationReceived != null)
                    _waitHandleForConfigurationReceived.Dispose();
                    _waitHandleForConfigurationReceived = null;
                ((ClientRemoteSessionDSHandlerImpl)SessionDataStructureHandler).Dispose();
                SessionDataStructureHandler = null;
                _cryptoHelper.Dispose();
                _cryptoHelper = null;

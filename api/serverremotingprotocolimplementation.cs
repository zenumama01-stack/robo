    /// This class is an implementation of the abstract class ServerRemoteSessionDataStructureHandler.
    internal class ServerRemoteSessionDSHandlerImpl : ServerRemoteSessionDataStructureHandler
        private readonly ServerRemoteSessionDSHandlerStateMachine _stateMachine;
        internal override AbstractServerSessionTransportManager TransportManager
        /// Constructs a ServerRemoteSession handler using the supplied transport manager. The
        /// supplied transport manager will be used to send and receive data from the remote
        internal ServerRemoteSessionDSHandlerImpl(ServerRemoteSession session,
            Dbg.Assert(session != null, "session cannot be null.");
            _stateMachine = new ServerRemoteSessionDSHandlerStateMachine(session);
            _transportManager.DataReceived += session.DispatchInputQueueData;
        /// Calls the transport layer connect to make a connection to the listener.
            // for the WSMan implementation, this is a no-op..and statemachine is coded accordingly
            // to move to negotiation pending.
        /// This method sends the server side capability negotiation packet to the client.
        internal override void SendNegotiationAsync()
            RemoteSessionCapability serverCapability = _session.Context.ServerCapability;
            RemoteDataObject data = RemotingEncoder.GenerateServerSessionCapability(serverCapability,
            // send data to client..flush is not true as we expect to send state changed
            // information (from runspace creation)
            _transportManager.SendDataToClient<PSObject>(dataToBeSent, false);
        /// This event indicates that the client capability negotiation packet has been received.
        internal override event EventHandler<EventArgs> SessionClosing;
        internal override event EventHandler<RemoteDataEventArgs<string>> PublicKeyReceived;
        /// Send the encrypted session key to the client side.
        /// <param name="encryptedSessionKey">encrypted session key
        /// as a string</param>
        internal override void SendEncryptedSessionKey(string encryptedSessionKey)
            _transportManager.SendDataToClient<object>(RemotingEncoder.GenerateEncryptedSessionKeyResponse(
                Guid.Empty, encryptedSessionKey), true);
        /// Send request to the client for sending a public key.
        internal override void SendRequestForPublicKey()
            _transportManager.SendDataToClient<object>(
                RemotingEncoder.GeneratePublicKeyRequest(Guid.Empty), true);
            RaiseDataReceivedEvent(new RemoteDataEventArgs(receivedData));
        /// This method calls the transport level call to close the connection to the listener.
        /// If the transport call fails.
        internal override void CloseConnectionAsync(Exception reasonForClose)
            // Raise the closing event
            SessionClosing.SafeInvoke(this, EventArgs.Empty);
            _transportManager.Close(reasonForClose);
        /// This event indicates that the client has requested to create a new runspace pool
        /// on the server side.
        internal override event EventHandler<RemoteDataEventArgs> CreateRunspacePoolReceived;
        /// A reference to the FSM object.
        internal override ServerRemoteSessionDSHandlerStateMachine StateMachine
        /// This method is used by the input queue dispatching mechanism.
        /// It examines the data and takes appropriate actions.
        /// The received client data.
        internal override void RaiseDataReceivedEvent(RemoteDataEventArgs dataArg)
                        // At this point, the negotiation is complete, so
                        // need to import the clients public key
                        CreateRunspacePoolReceived.SafeInvoke(this, dataArg);
                    PSRemotingDataStructureException reasonOfClose = new PSRemotingDataStructureException(RemotingErrorIdStrings.ClientRequestedToCloseSession);
                        throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerNotFoundCapabilityProperties,
                    if (NegotiationReceived != null)
                        negotiationArg.RemoteData = rcvdData;
                        string remotePublicKey = RemotingDecoder.GetPublicKey(rcvdData.Data);
                        PublicKeyReceived.SafeInvoke(this, new RemoteDataEventArgs<string>(remotePublicKey));

    /// This abstract class defines the server side data structure handler that a remote connection has
    /// at the remote session level.
    /// There are two other data structure handler levels:
    /// 1) at the runspace level,
    /// 2) at the pipeline level.
    /// This session level data structure handler defines what can be done at the session level.
    internal abstract class ServerRemoteSessionDataStructureHandler : BaseSessionDataStructureHandler
        /// Constructor does no special initialization.
        internal ServerRemoteSessionDataStructureHandler()
        /// Makes a connect call asynchronously.
        /// Send capability negotiation asynchronously.
        internal abstract void SendNegotiationAsync();
        /// This event indicates that a client's capability negotiation packet has been received.
        /// Message describing why the session is closing
        internal abstract void CloseConnectionAsync(Exception reasonForClose);
        /// Event that raised when session datastructure handler is closing.
        internal abstract event EventHandler<EventArgs> SessionClosing;
        /// This event indicates a request for creating a new runspace pool
        /// has been received on the server side.
        internal abstract event EventHandler<RemoteDataEventArgs> CreateRunspacePoolReceived;
        /// A reference to the Finite State Machine.
        internal abstract ServerRemoteSessionDSHandlerStateMachine StateMachine
        internal abstract AbstractServerSessionTransportManager TransportManager
        /// This method is used by the client data dispatching mechanism.
        /// This parameter contains the remote data from the client.
        internal abstract void RaiseDataReceivedEvent(RemoteDataEventArgs arg); // this is the API the Transport calls
        internal abstract event EventHandler<RemoteDataEventArgs<string>> PublicKeyReceived;
        internal abstract void SendRequestForPublicKey();
        internal abstract void SendEncryptedSessionKey(string encryptedSessionKey);

    internal sealed class RemoteSessionNegotiationEventArgs : EventArgs
        internal RemoteSessionNegotiationEventArgs(RemoteSessionCapability remoteSessionCapability)
            Dbg.Assert(remoteSessionCapability != null, "caller should validate the parameter");
            if (remoteSessionCapability == null)
                throw PSTraceSource.NewArgumentNullException(nameof(remoteSessionCapability));
            RemoteSessionCapability = remoteSessionCapability;
        /// Data from network converted to type RemoteSessionCapability.
        internal RemoteSessionCapability RemoteSessionCapability { get; }
        /// Actual data received from the network.
        internal RemoteDataObject<PSObject> RemoteData { get; set; }
    /// This event arg is designed to contain generic data received from the other side of the connection.
    /// It can be used for both the client side and for the server side.
    internal sealed class RemoteDataEventArgs : EventArgs
        internal RemoteDataEventArgs(RemoteDataObject<PSObject> receivedData)
            Dbg.Assert(receivedData != null, "caller should validate the parameter");
            if (receivedData == null)
                throw PSTraceSource.NewArgumentNullException(nameof(receivedData));
            ReceivedData = receivedData;
        /// Received data.
        public RemoteDataObject<PSObject> ReceivedData { get; }
    /// This event arg contains data received and is used to pass information
    /// from a data structure handler to its object.
    /// <typeparam name="T">type of data that's associated</typeparam>
    internal sealed class RemoteDataEventArgs<T> : EventArgs
        /// The data contained within this event.
        internal T Data { get; }
        internal RemoteDataEventArgs(object data)
            // Dbg.Assert(data != null, "data passed should not be null");
            Data = (T)data;
    /// This defines the various states a remote connection can be in.
    internal enum RemoteSessionState
        /// Undefined state.
        UndefinedState = 0,
        /// This is the state a connect start with. When a connection is closed,
        /// the connection will eventually come back to this Idle state.
        Idle = 1,
        /// A connection operation has been initiated.
        Connecting = 2,
        /// A connection operation has completed successfully.
        Connected = 3,
        /// The capability negotiation message is in the process being sent on a create operation.
        NegotiationSending = 4,
        /// The capability negotiation message is in the process being sent on a connect operation.
        NegotiationSendingOnConnect = 5,
        /// The capability negotiation message is sent successfully from a sender point of view.
        NegotiationSent = 6,
        /// A capability negotiation message is received.
        NegotiationReceived = 7,
        /// Used by server to wait for negotiation from client.
        NegotiationPending = 8,
        /// The connection is in the progress of getting closed.
        ClosingConnection = 9,
        /// The connection is closed completely.
        Closed = 10,
        /// The capability negotiation has been successfully completed.
        Established = 11,
        /// Have sent a public key to the remote end,
        /// awaiting a response.
        /// <remarks>Applicable only to client</remarks>
        EstablishedAndKeySent = 12,
        /// Have received a public key from the remote
        /// end, need to send a response.
        /// <remarks>Applicable only to server</remarks>
        EstablishedAndKeyReceived = 13,
        /// For Server - Have sent a request to the remote end to
        /// send a public key
        /// for Client - have received a PK request from server.
        /// <remarks>Applicable to both client and server</remarks>
        EstablishedAndKeyRequested = 14,
        /// Key exchange complete. This can mean
        ///      (a) Sent an encrypted session key to the
        ///          remote end in response to receiving
        ///          a public key - this is for the server
        ///      (b) Received an encrypted session key from
        ///          remote end after sending a public key -
        ///          this is for the client.
        EstablishedAndKeyExchanged = 15,
        Disconnecting = 16,
        Disconnected = 17,
        Reconnecting = 18,
        /// A disconnect operation initiated by the WinRM robust connection
        /// layer and *not* by the user.
        RCDisconnecting = 19,
        /// Number of states.
        MaxState = 20
    /// This defines the internal events that the finite state machine for the connection
    /// uses to take action and perform state transitions.
    internal enum RemoteSessionEvent
        InvalidEvent = 0,
        CreateSession = 1,
        ConnectSession = 2,
        NegotiationSending = 3,
        NegotiationSendingOnConnect = 4,
        NegotiationSendCompleted = 5,
        NegotiationReceived = 6,
        NegotiationCompleted = 7,
        Close = 9,
        CloseCompleted = 10,
        CloseFailed = 11,
        ConnectFailed = 12,
        NegotiationFailed = 13,
        NegotiationTimeout = 14,
        SendFailed = 15,
        ReceiveFailed = 16,
        FatalError = 17,
        MessageReceived = 18,
        KeySent = 19,
        KeySendFailed = 20,
        KeyReceived = 21,
        KeyReceiveFailed = 22,
        KeyRequested = 23,
        KeyRequestFailed = 24,
        DisconnectStart = 25,
        DisconnectCompleted = 26,
        DisconnectFailed = 27,
        ReconnectStart = 28,
        ReconnectCompleted = 29,
        ReconnectFailed = 30,
        RCDisconnectStarted = 31,
        MaxEvent = 32
    /// This is a wrapper class for RemoteSessionState.
    internal class RemoteSessionStateInfo
        internal RemoteSessionStateInfo(RemoteSessionState state)
        internal RemoteSessionStateInfo(RemoteSessionState state, Exception reason)
        internal RemoteSessionStateInfo(RemoteSessionStateInfo sessionStateInfo)
            State = sessionStateInfo.State;
            Reason = sessionStateInfo.Reason;
        /// State of the connection.
        internal RemoteSessionState State { get; }
        /// If the connection is closed, this provides reason why it had happened.
        internal Exception Reason { get; }
    /// This is the event arg that contains the state information.
    internal class RemoteSessionStateEventArgs : EventArgs
        internal RemoteSessionStateEventArgs(RemoteSessionStateInfo remoteSessionStateInfo)
            Dbg.Assert(remoteSessionStateInfo != null, "caller should validate the parameter");
            if (remoteSessionStateInfo == null)
                PSTraceSource.NewArgumentNullException(nameof(remoteSessionStateInfo));
            SessionStateInfo = remoteSessionStateInfo;
        /// State information about the connection.
        public RemoteSessionStateInfo SessionStateInfo { get; }
    internal class RemoteSessionStateMachineEventArgs : EventArgs
        internal RemoteSessionStateMachineEventArgs(RemoteSessionEvent stateEvent)
            : this(stateEvent, null)
        internal RemoteSessionStateMachineEventArgs(RemoteSessionEvent stateEvent, Exception reason)
            StateEvent = stateEvent;
        internal RemoteSessionEvent StateEvent { get; }
        internal RemoteSessionCapability RemoteSessionCapability { get; set; }
    /// Defines the various types of remoting behaviour that a cmdlet may
    /// desire when used in a context that supports ambient / automatic remoting.
    public enum RemotingCapability
        /// In the presence of ambient remoting, this command should
        /// still be run locally.
        /// be run on the target computer using PowerShell Remoting.
        PowerShell,
        /// In the presence of ambient remoting, this command supports
        /// its own form of remoting which can be used instead to target
        /// the remote computer.
        SupportedByCommand,
        /// In the presence of ambient remoting, the command assumes
        /// all responsibility for targeting the remote computer;
        /// PowerShell Remoting is not supported.
        OwnedByCommand
    /// Controls or overrides the remoting behavior, during invocation, of a
    /// command that supports ambient remoting.
    public enum RemotingBehavior
        /// In the presence of ambient remoting, run this command locally.
        /// In the presence of ambient remoting, run this command on the target
        /// computer using PowerShell Remoting.
        /// In the presence of ambient remoting, and a command that declares
        /// 'SupportedByCommand' remoting capability, run this command on the
        /// target computer using the command's custom remoting facilities.
        Custom

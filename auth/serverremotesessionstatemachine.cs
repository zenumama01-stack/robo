    /// This class implements a Finite State Machine (FSM) to control the remote connection on the server side.
    /// There is a similar but not identical FSM on the client side for this connection.
    internal class ServerRemoteSessionDSHandlerStateMachine
        [TraceSource("ServerRemoteSessionDSHandlerStateMachine", "ServerRemoteSessionDSHandlerStateMachine")]
        private static readonly PSTraceSource s_trace = PSTraceSource.GetTracer("ServerRemoteSessionDSHandlerStateMachine", "ServerRemoteSessionDSHandlerStateMachine");
        private readonly ServerRemoteSession _session;
        /// Timer used for key exchange.
        /// This constructor instantiates a FSM object for the server side to control the remote connection.
        /// It initializes the event handling matrix with event handlers.
        /// It sets the initial state of the FSM to be Idle.
        /// This is the remote session object.
        internal ServerRemoteSessionDSHandlerStateMachine(ServerRemoteSession session)
                _stateMachineHandle[i, (int)RemoteSessionEvent.FatalError] += DoFatalError;
                _stateMachineHandle[i, (int)RemoteSessionEvent.CloseFailed] += DoCloseFailed;
                _stateMachineHandle[i, (int)RemoteSessionEvent.CloseCompleted] += DoCloseCompleted;
                _stateMachineHandle[i, (int)RemoteSessionEvent.NegotiationTimeout] += DoNegotiationTimeout;
                _stateMachineHandle[i, (int)RemoteSessionEvent.SendFailed] += DoSendFailed;
                _stateMachineHandle[i, (int)RemoteSessionEvent.ReceiveFailed] += DoReceiveFailed;
                _stateMachineHandle[i, (int)RemoteSessionEvent.ConnectSession] += DoConnect;
            _stateMachineHandle[(int)RemoteSessionState.Idle, (int)RemoteSessionEvent.CreateSession] += DoCreateSession;
            _stateMachineHandle[(int)RemoteSessionState.NegotiationPending, (int)RemoteSessionEvent.NegotiationReceived] += DoNegotiationReceived;
            _stateMachineHandle[(int)RemoteSessionState.NegotiationReceived, (int)RemoteSessionEvent.NegotiationSending] += DoNegotiationSending;
            _stateMachineHandle[(int)RemoteSessionState.NegotiationSending, (int)RemoteSessionEvent.NegotiationSendCompleted] += DoNegotiationCompleted;
            _stateMachineHandle[(int)RemoteSessionState.NegotiationSent, (int)RemoteSessionEvent.NegotiationCompleted] += DoEstablished;
            _stateMachineHandle[(int)RemoteSessionState.NegotiationSent, (int)RemoteSessionEvent.NegotiationPending] += DoNegotiationPending;
            _stateMachineHandle[(int)RemoteSessionState.Established, (int)RemoteSessionEvent.MessageReceived] += DoMessageReceived;
            _stateMachineHandle[(int)RemoteSessionState.NegotiationReceived, (int)RemoteSessionEvent.NegotiationFailed] += DoNegotiationFailed;
            _stateMachineHandle[(int)RemoteSessionState.Connecting, (int)RemoteSessionEvent.ConnectFailed] += DoConnectFailed;
            _stateMachineHandle[(int)RemoteSessionState.Established, (int)RemoteSessionEvent.KeyReceived] += DoKeyExchange; //
            _stateMachineHandle[(int)RemoteSessionState.Established, (int)RemoteSessionEvent.KeyRequested] += DoKeyExchange; //
            _stateMachineHandle[(int)RemoteSessionState.Established, (int)RemoteSessionEvent.KeyReceiveFailed] += DoKeyExchange; //
            _stateMachineHandle[(int)RemoteSessionState.EstablishedAndKeyRequested, (int)RemoteSessionEvent.KeyReceived] += DoKeyExchange; //
            _stateMachineHandle[(int)RemoteSessionState.EstablishedAndKeyRequested, (int)RemoteSessionEvent.KeySent] += DoKeyExchange; //
            _stateMachineHandle[(int)RemoteSessionState.EstablishedAndKeyRequested, (int)RemoteSessionEvent.KeyReceiveFailed] += DoKeyExchange; //
            _stateMachineHandle[(int)RemoteSessionState.EstablishedAndKeyReceived, (int)RemoteSessionEvent.KeySendFailed] += DoKeyExchange;
            _stateMachineHandle[(int)RemoteSessionState.EstablishedAndKeyReceived, (int)RemoteSessionEvent.KeySent] += DoKeyExchange;
            // with connect, a new client can negotiate a key change to a server that has already negotiated key exchange with a previous client
            _stateMachineHandle[(int)RemoteSessionState.EstablishedAndKeyExchanged, (int)RemoteSessionEvent.KeyReceived] += DoKeyExchange; //
            _stateMachineHandle[(int)RemoteSessionState.EstablishedAndKeyExchanged, (int)RemoteSessionEvent.KeyRequested] += DoKeyExchange; //
            _stateMachineHandle[(int)RemoteSessionState.EstablishedAndKeyExchanged, (int)RemoteSessionEvent.KeyReceiveFailed] += DoKeyExchange; //
            // Initially, set state to Idle
                    _state == RemoteSessionState.EstablishedAndKeySent || // server session will never be in this state.. TODO- remove this
                    _state == RemoteSessionState.EstablishedAndKeyReceived ||
                    _state == RemoteSessionState.EstablishedAndKeyExchanged)
        /// <param name="fsmEventArg">
        internal void RaiseEvent(RemoteSessionStateMachineEventArgs fsmEventArg)
            // make sure only one thread is processing events.
                s_trace.WriteLine("Event received : {0}", fsmEventArg.StateEvent);
                _processPendingEventsQueue.Enqueue(fsmEventArg);
                if (_eventsInProcess)
            // currently server state machine doesn't raise events
            // this will allow server state machine to raise events.
            // RaiseStateMachineEvents();
        private void RaiseEventPrivate(RemoteSessionStateMachineEventArgs fsmEventArg)
            if (fsmEventArg == null)
                throw PSTraceSource.NewArgumentNullException(nameof(fsmEventArg));
            EventHandler<RemoteSessionStateMachineEventArgs> handler = _stateMachineHandle[(int)_state, (int)fsmEventArg.StateEvent];
                s_trace.WriteLine("Before calling state machine event handler: state = {0}, event = {1}", _state, fsmEventArg.StateEvent);
                handler(this, fsmEventArg);
                s_trace.WriteLine("After calling state machine event handler: state = {0}, event = {1}", _state, fsmEventArg.StateEvent);
        /// This is the handler for Start event of the FSM. This is the beginning of everything
        /// If the parameter <paramref name="fsmEventArg"/> is null.
        private void DoCreateSession(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
                Dbg.Assert(fsmEventArg.StateEvent == RemoteSessionEvent.CreateSession, "StateEvent must be CreateSession");
                Dbg.Assert(_state == RemoteSessionState.Idle, "DoCreateSession cannot only be called in Idle state");
                DoNegotiationPending(sender, fsmEventArg);
        /// This is the handler for NegotiationPending event.
        /// NegotiationPending state can be in reached in the following cases
        /// 1. From Idle to NegotiationPending (during startup)
        /// 2. From Negotiation(Response)Sent to NegotiationPending.
        private void DoNegotiationPending(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
                Dbg.Assert((_state == RemoteSessionState.Idle) || (_state == RemoteSessionState.NegotiationSent),
                    "DoNegotiationPending can only occur when the state is Idle or NegotiationSent.");
                SetState(RemoteSessionState.NegotiationPending, null);
        /// This is the handler for the NegotiationReceived event.
        /// It sets the new state to be NegotiationReceived.
        /// If the parameter <paramref name="fsmEventArg"/> is not NegotiationReceived event or it does not hold the
        /// client negotiation packet.
        private void DoNegotiationReceived(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
                Dbg.Assert(fsmEventArg.StateEvent == RemoteSessionEvent.NegotiationReceived, "StateEvent must be NegotiationReceived");
                Dbg.Assert(fsmEventArg.RemoteSessionCapability != null, "RemoteSessioncapability must be non-null");
                Dbg.Assert(_state == RemoteSessionState.NegotiationPending, "state must be in NegotiationPending state");
                if (fsmEventArg.StateEvent != RemoteSessionEvent.NegotiationReceived)
                    throw PSTraceSource.NewArgumentException(nameof(fsmEventArg));
                if (fsmEventArg.RemoteSessionCapability == null)
        /// It sets the new state to be NegotiationSending, and sends the server side
        /// negotiation packet by queuing it on the output queue.
        private void DoNegotiationSending(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
            Dbg.Assert(fsmEventArg.StateEvent == RemoteSessionEvent.NegotiationSending, "Event must be NegotiationSending");
            Dbg.Assert(_state == RemoteSessionState.NegotiationReceived, "State must be NegotiationReceived");
            _session.SessionDataStructureHandler.SendNegotiationAsync();
        /// This is the handler for NegotiationSendCompleted event.
        /// It sets the new state to be NegotiationSent.
        private void DoNegotiationCompleted(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
                Dbg.Assert(_state == RemoteSessionState.NegotiationSending, "State must be NegotiationSending");
                Dbg.Assert(fsmEventArg.StateEvent == RemoteSessionEvent.NegotiationSendCompleted, "StateEvent must be NegotiationSendCompleted");
        /// This is the handler for the NegotiationCompleted event.
        /// It sets the new state to be Established. It turns off the negotiation timeout timer.
        private void DoEstablished(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
                Dbg.Assert(_state == RemoteSessionState.NegotiationSent, "State must be NegotiationReceived");
                Dbg.Assert(fsmEventArg.StateEvent == RemoteSessionEvent.NegotiationCompleted, "StateEvent must be NegotiationCompleted");
                if (fsmEventArg.StateEvent != RemoteSessionEvent.NegotiationCompleted)
                if (_state != RemoteSessionState.NegotiationSent)
        /// This is the handler for MessageReceived event. It dispatches the data to various components
        /// that uses the data.
        /// If the parameter <paramref name="fsmEventArg"/> does not contain remote data.
        internal void DoMessageReceived(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
                if (fsmEventArg.RemoteData == null)
                Dbg.Assert(_state == RemoteSessionState.Established ||
                           _state == RemoteSessionState.EstablishedAndKeyExchanged ||
                           _state == RemoteSessionState.EstablishedAndKeySent,  // server session will never be in this state.. TODO- remove this
                           "State must be Established or EstablishedAndKeySent or EstablishedAndKeyReceived or EstablishedAndKeyExchanged");
                RemotingTargetInterface targetInterface = fsmEventArg.RemoteData.TargetInterface;
                RemotingDataType dataType = fsmEventArg.RemoteData.DataType;
                ServerRunspacePoolDriver runspacePoolDriver;
                // string errorMessage = null;
                RemoteDataEventArgs remoteDataForSessionArg = null;
                                    remoteDataForSessionArg = new RemoteDataEventArgs(fsmEventArg.RemoteData);
                                    _session.SessionDataStructureHandler.RaiseDataReceivedEvent(remoteDataForSessionArg);
                        clientRunspacePoolId = fsmEventArg.RemoteData.RunspacePoolId;
                        runspacePoolDriver = _session.GetRunspacePoolDriver(clientRunspacePoolId);
                        if (runspacePoolDriver != null)
                            runspacePoolDriver.DataStructureHandler.ProcessReceivedData(fsmEventArg.RemoteData);
                            s_trace.WriteLine(@"Server received data for Runspace (id: {0}),
                            PSRemotingDataStructureException reasonOfFailure = new
                                PSRemotingDataStructureException(RemotingErrorIdStrings.RunspaceCannotBeFound,
                                    clientRunspacePoolId);
                            RemoteSessionStateMachineEventArgs runspaceNotFoundArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.FatalError, reasonOfFailure);
                            RaiseEvent(runspaceNotFoundArg);
                        runspacePoolDriver.DataStructureHandler.DispatchMessageToPowerShell(fsmEventArg.RemoteData);
                        s_trace.WriteLine("Server received data unknown targetInterface: {0}", targetInterface);
                        PSRemotingDataStructureException reasonOfFailure2 = new PSRemotingDataStructureException(RemotingErrorIdStrings.ReceivedUnsupportedRemotingTargetInterfaceType, targetInterface);
                        RemoteSessionStateMachineEventArgs unknownTargetArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.FatalError, reasonOfFailure2);
                        RaiseEvent(unknownTargetArg);
        /// This is the handler for ConnectFailed event. In this implementation, this should never
        /// happen. This is because the IO channel is stdin/stdout/stderr redirection.
        /// Therefore, the connection is a dummy operation.
        /// If the parameter <paramref name="fsmEventArg"/> does not contain ConnectFailed event.
        private void DoConnectFailed(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
                Dbg.Assert(fsmEventArg.StateEvent == RemoteSessionEvent.ConnectFailed, "StateEvent must be ConnectFailed");
                if (fsmEventArg.StateEvent != RemoteSessionEvent.ConnectFailed)
                Dbg.Assert(_state == RemoteSessionState.Connecting, "session State must be Connecting");
                // This method should not be called in this implementation.
        /// This is the handler for FatalError event. It directly calls the DoClose, which
        /// is the Close event handler.
        /// If the parameter <paramref name="fsmEventArg"/> does not contains FatalError event.
        private void DoFatalError(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
                Dbg.Assert(fsmEventArg.StateEvent == RemoteSessionEvent.FatalError, "StateEvent must be FatalError");
                if (fsmEventArg.StateEvent != RemoteSessionEvent.FatalError)
                DoClose(this, fsmEventArg);
        /// Handle connect event - this is raised when a new client tries to connect to an existing session
        /// No changes to state. Calls into the session to handle any post connect operations.
        /// <param name="fsmEventArg"></param>
        private void DoConnect(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
            Dbg.Assert(_state != RemoteSessionState.Idle, "session should not be in idle state when SessionConnect event is queued");
            if ((_state != RemoteSessionState.Closed) && (_state != RemoteSessionState.ClosingConnection))
                _session.HandlePostConnect();
        /// This is the handler for Close event. It closes the connection.
        private void DoClose(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
                    case RemoteSessionState.EstablishedAndKeySent:  // server session will never be in this state.. TODO- remove this
                    case RemoteSessionState.EstablishedAndKeyReceived:
                    case RemoteSessionState.EstablishedAndKeyExchanged:
                        SetState(RemoteSessionState.ClosingConnection, fsmEventArg.Reason);
                        _session.SessionDataStructureHandler.CloseConnectionAsync(fsmEventArg.Reason);
                        Exception forcedCloseException = new PSRemotingTransportException(fsmEventArg.Reason, RemotingErrorIdStrings.ForceClosed);
                        SetState(RemoteSessionState.Closed, forcedCloseException);
        /// This is the handler for CloseFailed event.
        /// It simply force the new state to be Closed.
        private void DoCloseFailed(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
                Dbg.Assert(fsmEventArg.StateEvent == RemoteSessionEvent.CloseFailed, "StateEvent must be CloseFailed");
                RemoteSessionState stateBeforeTransition = _state;
                SetState(RemoteSessionState.Closed, fsmEventArg.Reason);
        /// This is the handler for CloseCompleted event. It sets the new state to be Closed.
        private void DoCloseCompleted(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
                Dbg.Assert(fsmEventArg.StateEvent == RemoteSessionEvent.CloseCompleted, "StateEvent must be CloseCompleted");
                // Close the session only after changing the state..this way
                // state machine will not process anything.
                _session.Close(fsmEventArg);
        /// This is the handler for NegotiationFailed event.
        /// It raises a Close event to trigger the connection to be shutdown.
        private void DoNegotiationFailed(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
                Dbg.Assert(fsmEventArg.StateEvent == RemoteSessionEvent.NegotiationFailed, "StateEvent must be NegotiationFailed");
                RaiseEventPrivate(closeArg);
        /// This is the handler for NegotiationTimeout event.
        /// If the connection is already Established, it ignores this event.
        /// Otherwise, it raises a Close event to trigger a close of the connection.
        private void DoNegotiationTimeout(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
                Dbg.Assert(fsmEventArg.StateEvent == RemoteSessionEvent.NegotiationTimeout, "StateEvent must be NegotiationTimeout");
        /// This is the handler for SendFailed event.
        /// This is an indication that the wire layer IO is no longer connected. So it raises
        /// a Close event to trigger a connection shutdown.
        private void DoSendFailed(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
                Dbg.Assert(fsmEventArg.StateEvent == RemoteSessionEvent.SendFailed, "StateEvent must be SendFailed");
        /// This is the handler for ReceivedFailed event.
        private void DoReceiveFailed(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
                Dbg.Assert(fsmEventArg.StateEvent == RemoteSessionEvent.ReceiveFailed, "StateEvent must be ReceivedFailed");
        /// This method contains all the logic for handling the state machine
        /// for key exchange. All the different scenarios are covered in this.
        private void DoKeyExchange(object sender, RemoteSessionStateMachineEventArgs eventArgs)
            // There are corner cases with disconnect that can result in client receiving outdated key exchange packets
            // ***TODO*** Deal with this on the client side. Key exchange packets should have additional information
            // that identify the context of negotiation. Just like callId in SetMax and SetMinRunspaces messages
                        // does the server ever start key exchange process??? This may not be required
                        if (_state == RemoteSessionState.EstablishedAndKeyRequested)
                            // reset the timer
                        // the key import would have been done
                        // set state accordingly
                        SetState(RemoteSessionState.EstablishedAndKeyReceived, eventArgs.Reason);
                        // you need to send an encrypted session key to the client
                        _session.SendEncryptedSessionKey();
                        if (_state == RemoteSessionState.EstablishedAndKeyReceived)
                            // key exchange is now complete
                            SetState(RemoteSessionState.EstablishedAndKeyExchanged, eventArgs.Reason);
                        if ((_state == RemoteSessionState.Established) || (_state == RemoteSessionState.EstablishedAndKeyExchanged))
                            // the key has been sent set state accordingly
                            // start the timer
                            _keyExchangeTimer = new Timer(HandleKeyExchangeTimeout, null, BaseTransportManager.ServerDefaultKeepAliveTimeoutMs, Timeout.Infinite);
                case RemoteSessionEvent.KeyReceiveFailed:
                        DoClose(this, eventArgs);
                case RemoteSessionEvent.KeySendFailed:
            Dbg.Assert(_state == RemoteSessionState.EstablishedAndKeyRequested, "timeout should only happen when waiting for a key");
                new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerKeyExchangeFailed);
        /// This method is designed to be a cleanup routine after the connection is closed.
        /// It can also be used for graceful shutdown of the server process, which is not currently
        /// implemented.
        /// Set the FSM state to a new state.
        /// <param name="newState">
        /// The new state.
        /// Optional parameter that can provide additional information. This is currently not used.
            // TODO: else should we close the session here?

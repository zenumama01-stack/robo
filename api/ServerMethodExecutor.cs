    /// Responsible for routing messages from the server, blocking the callers and
    /// then waking them up when there is a response to their message.
    internal class ServerMethodExecutor
        /// Default client pipeline id.
        private const long DefaultClientPipelineId = -1;
        /// Server dispatch table.
        private readonly ServerDispatchTable _serverDispatchTable;
        /// Remote host call data type.
        private readonly RemotingDataType _remoteHostCallDataType;
        private readonly AbstractServerTransportManager _transportManager;
        /// Constructor for ServerMethodExecutor.
        internal ServerMethodExecutor(
            Guid clientRunspacePoolId, Guid clientPowerShellId,
            AbstractServerTransportManager transportManager)
            _remoteHostCallDataType =
                clientPowerShellId == Guid.Empty ? RemotingDataType.RemoteHostCallUsingRunspaceHost : RemotingDataType.RemoteHostCallUsingPowerShellHost;
            _serverDispatchTable = new ServerDispatchTable();
        /// Handle remote host response from client.
        internal void HandleRemoteHostResponseFromClient(RemoteHostResponse remoteHostResponse)
            _serverDispatchTable.SetResponse(remoteHostResponse.CallId, remoteHostResponse);
            _serverDispatchTable.AbortAllCalls();
        internal void ExecuteVoidMethod(RemoteHostMethodId methodId)
            ExecuteVoidMethod(methodId, Array.Empty<object>());
        internal void ExecuteVoidMethod(RemoteHostMethodId methodId, object[] parameters)
            // Use void call ID so that the call is known to not have a return value.
            const long callId = ServerDispatchTable.VoidCallId;
            RemoteHostCall remoteHostCall = new RemoteHostCall(callId, methodId, parameters);
            // Dispatch the call but don't wait for response since the return value is void.
            // TODO: remove redundant data from the RemoteHostCallPacket.
            RemoteDataObject<PSObject> dataToBeSent = RemoteDataObject<PSObject>.CreateFrom(RemotingDestination.Client,
                _remoteHostCallDataType, _clientRunspacePoolId, _clientPowerShellId,
                remoteHostCall.Encode());
            // flush is not used here..since this is a void method and server host
            // does not expect anything from client..so let the transport manager buffer
            // and send as much data as possible.
            _transportManager.SendDataToClient(dataToBeSent, false);
        /// Execute method.
        internal T ExecuteMethod<T>(RemoteHostMethodId methodId)
            return ExecuteMethod<T>(methodId, Array.Empty<object>());
        internal T ExecuteMethod<T>(RemoteHostMethodId methodId, object[] parameters)
            // Create the method call object.
            long callId = _serverDispatchTable.CreateNewCallId();
            // report that execution is pending host response
            _transportManager.SendDataToClient(dataToBeSent, false, true);
            // Wait for response.
            RemoteHostResponse remoteHostResponse = _serverDispatchTable.GetResponse(callId, null);
            // Null means that the response PSObject was not received and there was an error.
                throw RemoteHostExceptions.NewRemoteHostCallFailedException(methodId);
            // Process the response.
            object returnValue = remoteHostResponse.SimulateExecution();
            Dbg.Assert(returnValue is T, "Expected returnValue is T");
            return (T)remoteHostResponse.SimulateExecution();

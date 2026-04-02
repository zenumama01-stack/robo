using System.Management.Automation.Remoting.Client;
namespace System.Management.Automation.Remoting
    /// Executes methods on the client.
    internal sealed class ClientMethodExecutor
        /// Transport manager.
        private readonly BaseClientTransportManager _transportManager;
        /// Client host.
        private readonly PSHost _clientHost;
        /// Client runspace pool id.
        private readonly Guid _clientRunspacePoolId;
        /// Client power shell id.
        private readonly Guid _clientPowerShellId;
        /// Remote host call.
        private readonly RemoteHostCall _remoteHostCall;
        internal RemoteHostCall RemoteHostCall
                return _remoteHostCall;
        /// Constructor for ClientMethodExecutor.
        private ClientMethodExecutor(BaseClientTransportManager transportManager, PSHost clientHost, Guid clientRunspacePoolId, Guid clientPowerShellId, RemoteHostCall remoteHostCall)
            Dbg.Assert(transportManager != null, "Expected transportManager != null");
            Dbg.Assert(remoteHostCall != null, "Expected remoteHostCall != null");
            _transportManager = transportManager;
            _remoteHostCall = remoteHostCall;
            _clientHost = clientHost;
            _clientRunspacePoolId = clientRunspacePoolId;
            _clientPowerShellId = clientPowerShellId;
        /// Create a new ClientMethodExecutor object and then dispatch it.
        internal static void Dispatch(
            BaseClientTransportManager transportManager,
            PSHost clientHost,
            PSDataCollectionStream<ErrorRecord> errorStream,
            ObjectStream methodExecutorStream,
            bool isMethodExecutorStreamEnabled,
            RemoteRunspacePoolInternal runspacePool,
            Guid clientPowerShellId,
            RemoteHostCall remoteHostCall)
            ClientMethodExecutor methodExecutor =
                new ClientMethodExecutor(transportManager, clientHost, runspacePool.InstanceId,
                    clientPowerShellId, remoteHostCall);
            // If the powershell id is not specified, this message is for the runspace pool, execute
            // it immediately and return
            if (clientPowerShellId == Guid.Empty)
                methodExecutor.Execute(errorStream);
            // Check client host to see if SetShouldExit should be allowed
            bool hostAllowSetShouldExit = false;
            if (clientHost != null)
                PSObject hostPrivateData = clientHost.PrivateData as PSObject;
                if (hostPrivateData != null)
                    PSNoteProperty allowSetShouldExit = hostPrivateData.Properties["AllowSetShouldExitFromRemote"] as PSNoteProperty;
                    hostAllowSetShouldExit = allowSetShouldExit != null && allowSetShouldExit.Value is bool && (bool)allowSetShouldExit.Value;
            // Should we kill remote runspace? Check if "SetShouldExit" and if we are in the
            // cmdlet case. In the API case (when we are invoked from an API not a cmdlet) we
            // should not interpret "SetShouldExit" but should pass it on to the host. The
            // variable IsMethodExecutorStreamEnabled is only true in the cmdlet case. In the
            // API case it is false.
            if (remoteHostCall.IsSetShouldExit && isMethodExecutorStreamEnabled && !hostAllowSetShouldExit)
                runspacePool.Close();
            // Cmdlet case: queue up the executor in the pipeline stream.
            if (isMethodExecutorStreamEnabled)
                Dbg.Assert(methodExecutorStream != null, "method executor stream can't be null when enabled");
                methodExecutorStream.Write(methodExecutor);
            // API case: execute it immediately.
        /// Is runspace pushed.
        private static bool IsRunspacePushed(PSHost host)
            if (host is not IHostSupportsInteractiveSession host2)
            // IsRunspacePushed can throw (not implemented exception)
                return host2.IsRunspacePushed;
        /// Execute.
        internal void Execute(PSDataCollectionStream<ErrorRecord> errorStream)
            Action<ErrorRecord> writeErrorAction = null;
            // If error-stream is null or we are in pushed-runspace - then write error directly to console.
            if (errorStream == null || IsRunspacePushed(_clientHost))
                writeErrorAction = (ErrorRecord errorRecord) =>
                        _clientHost.UI?.WriteErrorLine(errorRecord.ToString());
            // Otherwise write it to error-stream.
                writeErrorAction = (ErrorRecord errorRecord) => errorStream.Write(errorRecord);
            this.Execute(writeErrorAction);
        internal void Execute(Cmdlet cmdlet)
            this.Execute(cmdlet.WriteError);
        internal void Execute(Action<ErrorRecord> writeErrorAction)
            if (_remoteHostCall.IsVoidMethod)
                ExecuteVoid(writeErrorAction);
                RemotingDataType remotingDataType =
                    _clientPowerShellId == Guid.Empty ? RemotingDataType.RemoteRunspaceHostResponseData : RemotingDataType.RemotePowerShellHostResponseData;
                RemoteHostResponse remoteHostResponse = _remoteHostCall.ExecuteNonVoidMethod(_clientHost);
                RemoteDataObject<PSObject> dataToBeSent = RemoteDataObject<PSObject>.CreateFrom(
                    RemotingDestination.Server, remotingDataType, _clientRunspacePoolId,
                    _clientPowerShellId, remoteHostResponse.Encode());
                _transportManager.DataToBeSentCollection.Add<PSObject>(dataToBeSent, DataPriorityType.PromptResponse);
        /// Execute void.
        internal void ExecuteVoid(Action<ErrorRecord> writeErrorAction)
                _remoteHostCall.ExecuteVoidMethod(_clientHost);
                // Extract inner exception.
                if (exception.InnerException != null)
                    exception = exception.InnerException;
                // Create an error record and write it to the stream.
                    nameof(PSRemotingErrorId.RemoteHostCallFailed),
                    _remoteHostCall.MethodName);
                writeErrorAction(errorRecord);

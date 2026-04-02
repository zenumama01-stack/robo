// Copyright (c) Microsoft Corporation. All rights reserved.
namespace Microsoft.PowerShell.CustomNamedPipeConnection
    #region NamedPipeClient
    /// This class is based on PowerShell core source code, and handles creating
    /// a client side named pipe object that can connect to a running PowerShell 
    /// process by its process Id.
    internal sealed class NamedPipeClient : IDisposable
        /// Name of the pipe.
        public string PipeName { get; private set; }
        private NamedPipeClient()
        public NamedPipeClient(int procId)
            PipeName = CreateProcessPipeName(
        private static string CreateProcessPipeName(System.Diagnostics.Process proc)
            System.Text.StringBuilder pipeNameBuilder = new System.Text.StringBuilder();
            pipeNameBuilder.Append(@"PSHost.");
            string DefaultAppDomainName;
            if (OperatingSystem.IsWindows())
                DefaultAppDomainName = "DefaultAppDomain";
                pipeNameBuilder.Append(proc.StartTime.ToFileTime().ToString(CultureInfo.InvariantCulture));
                DefaultAppDomainName = "None";
                pipeNameBuilder.Append(proc.StartTime.ToFileTime().ToString("X8").AsSpan(1, 8));
            pipeNameBuilder.Append('.')
                .Append(DefaultAppDomainName)
                _clientPipeStream.Dispose();
        public void AbortConnect()
        private NamedPipeClientStream DoConnect(int timeout)
            throw new TimeoutException(@"Timeout expired before connection could be made to named pipe.");
    #region NamedPipeInfo
    internal sealed class NamedPipeInfo : RunspaceConnectionInfo
        private NamedPipeClient _clientPipe;
        /// Process Id to attach to.
        /// ConnectingTimeout in Milliseconds
        private NamedPipeInfo()
        /// Construct instance.
        public NamedPipeInfo(
            int connectingTimeout)
            _computerName = $"LocalMachine:{ProcessId}";
            _clientPipe = new NamedPipeClient(ProcessId);
        /// ComputerName
        /// Credential
        /// AuthenticationMechanism
        /// CertificateThumbprint
        /// Create shallow copy of NamedPipeInfo object.
            var connectionInfo = new NamedPipeInfo(ProcessId, ConnectingTimeout);
            connectionInfo._clientPipe = _clientPipe;
        /// Create an instance of ClientSessionTransportManager.
        public override BaseClientSessionTransportManager CreateClientSessionTransportManager(
            return new NamedPipeClientSessionTransportMgr(
                connectionInfo: this,
                runspaceId: instanceId,
                cryptoHelper: cryptoHelper);
        /// Attempt to connect to process Id.
        /// If connection fails, is aborted, or times out, an exception is thrown.
        /// <param name="textWriter">Named pipe text stream writer.</param>
        /// <param name="textReader">Named pipe text stream reader.</param>
        /// <exception cref="TimeoutException">Connect attempt times out or is aborted.</exception>
            out StreamWriter textWriter,
            out StreamReader textReader)
            _clientPipe.Connect(
                timeout: ConnectingTimeout > -1 ? ConnectingTimeout : int.MaxValue);
            textWriter = _clientPipe.TextWriter;
            textReader = _clientPipe.TextReader;
        /// Stops a connection attempt, or closes the connection that has been established.
        public void StopConnect()
            _clientPipe?.AbortConnect();
            _clientPipe?.Close();
    #region NamedPipeClientSessionTransportMgr
    internal sealed class NamedPipeClientSessionTransportMgr : ClientSessionTransportManagerBase
        private readonly NamedPipeInfo _connectionInfo;
        private const string _threadName = "NamedPipeCustomTransport Reader Thread";
        internal NamedPipeClientSessionTransportMgr(
            NamedPipeInfo connectionInfo,
                throw new PSArgumentException("connectionInfo");
            _connectionInfo.Connect(
                out StreamWriter pipeTextWriter,
                out StreamReader pipeTextReader);
            SetMessageWriter(pipeTextWriter);
            StartReaderThread(pipeTextReader);
            _connectionInfo.StopConnect();
        private void HandleSSHError(PSRemotingTransportException ex)
                    TransportMethodEnum.CloseShellOperationEx));
                HandleSSHError(new PSRemotingTransportException(
                    $"The SSH client session has ended reader thread with message: {errorMsg}"));
    #region New-NamedPipeSession
    /// Attempts to connect to the specified host computer and returns
    /// a PSSession object representing the remote session.
    [Cmdlet(VerbsCommon.New, "NamedPipeSession")]
    public sealed class NewNamedPipeSessionCommand : PSCmdlet
        private NamedPipeInfo _connectionInfo;
        private ManualResetEvent _openAsync;
        /// Name of host computer to connect to.
        [Parameter(Position=0, Mandatory=true)]
        public int ProcessId { get; set; }
        /// Optional value in seconds that limits the time allowed for a connection to be established.
        [ValidateRange(-1, 86400)]
        public int ConnectingTimeout { get; set; } = Timeout.Infinite;
        /// Optional name for the new PSSession.
            // Convert ConnectingTimeout value from seconds to milliseconds.
            _connectionInfo = new NamedPipeInfo(
                processId: ProcessId,
                connectingTimeout: (ConnectingTimeout == Timeout.Infinite) ? Timeout.Infinite : ConnectingTimeout * 1000);
            _runspace = RunspaceFactory.CreateRunspace(
                connectionInfo: _connectionInfo,
                host: Host,
                typeTable: TypeTable.LoadDefaultTypeFiles(),
                name: Name);
            _openAsync = new ManualResetEvent(false);
            _runspace.StateChanged += HandleRunspaceStateChanged;
                _runspace.OpenAsync();
                _openAsync.WaitOne();
                    PSSession.Create(
                        runspace: _runspace,
                        transportName: "PSNPTest",
                        psCmdlet: this));
                _openAsync.Dispose();
            _connectionInfo?.StopConnect();
        private void HandleRunspaceStateChanged(
                    _runspace.StateChanged -= HandleRunspaceStateChanged;
                    ReleaseWait();
        private void ReleaseWait()
                _openAsync?.Set();

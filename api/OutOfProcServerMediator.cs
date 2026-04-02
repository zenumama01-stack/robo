    internal abstract class OutOfProcessMediatorBase
        #region Protected Data
        protected TextReader originalStdIn;
        protected OutOfProcessTextWriter originalStdOut;
        protected OutOfProcessTextWriter originalStdErr;
        protected OutOfProcessServerSessionTransportManager sessionTM;
        protected OutOfProcessUtils.DataProcessingDelegates callbacks;
        protected static object SyncObject = new object();
        protected object _syncObject = new object();
        protected string _initialCommand;
        protected ManualResetEvent allcmdsClosedEvent;
        // Thread impersonation.
        protected WindowsIdentity _windowsIdentityToImpersonate;
        /// Count of commands in progress.
        protected int _inProgressCommandsCount = 0;
        protected PowerShellTraceSource tracer = PowerShellTraceSourceFactory.GetTraceSource();
        protected bool _exitProcessOnError;
        protected OutOfProcessMediatorBase(bool exitProcessOnError)
            _exitProcessOnError = exitProcessOnError;
            // Set up data handling callbacks.
            callbacks = new OutOfProcessUtils.DataProcessingDelegates();
            callbacks.DataPacketReceived += new OutOfProcessUtils.DataPacketReceived(OnDataPacketReceived);
            callbacks.DataAckPacketReceived += new OutOfProcessUtils.DataAckPacketReceived(OnDataAckPacketReceived);
            callbacks.CommandCreationPacketReceived += new OutOfProcessUtils.CommandCreationPacketReceived(OnCommandCreationPacketReceived);
            callbacks.CommandCreationAckReceived += new OutOfProcessUtils.CommandCreationAckReceived(OnCommandCreationAckReceived);
            callbacks.ClosePacketReceived += new OutOfProcessUtils.ClosePacketReceived(OnClosePacketReceived);
            callbacks.CloseAckPacketReceived += new OutOfProcessUtils.CloseAckPacketReceived(OnCloseAckPacketReceived);
            callbacks.SignalPacketReceived += new OutOfProcessUtils.SignalPacketReceived(OnSignalPacketReceived);
            callbacks.SignalAckPacketReceived += new OutOfProcessUtils.SignalAckPacketReceived(OnSignalAckPacketReceived);
            allcmdsClosedEvent = new ManualResetEvent(true);
        protected void ProcessingThreadStart(object state)
                string data = state as string;
                OutOfProcessUtils.ProcessData(data, callbacks);
                    OutOfProcessUtils.EXITCODE_UNHANDLED_EXCEPTION,
                    e.StackTrace);
                // notify the remote client of any errors and fail gracefully
                if (_exitProcessOnError)
                    originalStdErr.WriteLine(e.Message + e.StackTrace);
                    Environment.Exit(OutOfProcessUtils.EXITCODE_UNHANDLED_EXCEPTION);
        protected void OnDataPacketReceived(byte[] rawData, string stream, Guid psGuid)
            string streamTemp = System.Management.Automation.Remoting.Client.WSManNativeApi.WSMAN_STREAM_ID_STDIN;
                    sessionTM.ProcessRawData(rawData, streamTemp);
                AbstractServerTransportManager cmdTM = null;
                    cmdTM = sessionTM.GetCommandTransportManager(psGuid);
                if (cmdTM != null)
                    // not throwing when there is no associated command as the command might have
                    // legitimately closed while the client is sending data. however the client
                    // should die after timeout as we are not sending an ACK back.
                    cmdTM.ProcessRawData(rawData, streamTemp);
                    // There is no command transport manager to process the input data.
                    // However, we still need to acknowledge to the client that this input data
                    // was received.  This can happen with some cmdlets such as Select-Object -First
                    // where the cmdlet completes before all input data is received.
                    originalStdOut.WriteLine(OutOfProcessUtils.CreateDataAckPacket(psGuid));
        protected void OnDataAckPacketReceived(Guid psGuid)
        protected void OnCommandCreationPacketReceived(Guid psGuid)
                sessionTM.CreateCommandTransportManager(psGuid);
                if (_inProgressCommandsCount == 0)
                    allcmdsClosedEvent.Reset();
                _inProgressCommandsCount++;
                tracer.WriteMessage("OutOfProcessMediator.OnCommandCreationPacketReceived, in progress command count : " + _inProgressCommandsCount + " psGuid : " + psGuid.ToString());
        protected void OnCommandCreationAckReceived(Guid psGuid)
            throw new PSRemotingTransportException(PSRemotingErrorId.IPCUnknownElementReceived, RemotingErrorIdStrings.IPCUnknownElementReceived,
        protected void OnSignalPacketReceived(Guid psGuid)
                throw new PSRemotingTransportException(PSRemotingErrorId.IPCNoSignalForSession, RemotingErrorIdStrings.IPCNoSignalForSession,
                    // dont throw if there is no cmdTM as it might have legitimately closed
                    cmdTM?.Close(null);
                    // Always send ack signal to avoid not responding in client.
                    originalStdOut.WriteLine(OutOfProcessUtils.CreateSignalAckPacket(psGuid));
        protected void OnSignalAckPacketReceived(Guid psGuid)
        protected void OnClosePacketReceived(Guid psGuid)
                tracer.WriteMessage("BEGIN calling close on session transport manager");
                bool waitForAllcmdsClosedEvent = false;
                    if (_inProgressCommandsCount > 0)
                        waitForAllcmdsClosedEvent = true;
                // Wait outside sync lock if required for all cmds to be closed
                if (waitForAllcmdsClosedEvent)
                    allcmdsClosedEvent.WaitOne();
                    tracer.WriteMessage("OnClosePacketReceived, in progress commands count should be zero : " + _inProgressCommandsCount + ", psGuid : " + psGuid.ToString());
                    // it appears that when closing PowerShell ISE, therefore closing OutOfProcServerMediator, there are 2 Close command requests
                    // changing PSRP/IPC at this point is too risky, therefore protecting about this duplication
                    sessionTM?.Close(null);
                    tracer.WriteMessage("END calling close on session transport manager");
                    sessionTM = null;
                tracer.WriteMessage("Closing command with GUID " + psGuid.ToString());
                    tracer.WriteMessage("OnClosePacketReceived, in progress commands count should be greater than zero : " + _inProgressCommandsCount + ", psGuid : " + psGuid.ToString());
                    _inProgressCommandsCount--;
                        allcmdsClosedEvent.Set();
            // send close ack
            originalStdOut.WriteLine(OutOfProcessUtils.CreateCloseAckPacket(psGuid));
        protected void OnCloseAckPacketReceived(Guid psGuid)
        protected OutOfProcessServerSessionTransportManager CreateSessionTransportManager(
            string configurationFile,
            PSRemotingCryptoHelperServer cryptoHelper,
            string workingDirectory)
            PSSenderInfo senderInfo;
            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            PSPrincipal userPrincipal = new PSPrincipal(
                new PSIdentity(string.Empty, true, currentIdentity.Name, null),
                currentIdentity);
            senderInfo = new PSSenderInfo(userPrincipal, "http://localhost");
                new PSIdentity(string.Empty, true, string.Empty, null),
            var tm = new OutOfProcessServerSessionTransportManager(
                originalStdOut,
                originalStdErr,
            ServerRemoteSession.CreateServerRemoteSession(
                configurationProviderId: "Microsoft.PowerShell",
                initializationParameters: string.Empty,
                transportManager: tm,
                initialCommand: _initialCommand,
                configurationName: configurationName,
                configurationFile: configurationFile,
                initialLocation: workingDirectory);
        protected void Start(
            string workingDirectory,
            string configurationFile)
            _initialCommand = initialCommand;
            sessionTM = CreateSessionTransportManager(
                cryptoHelper: cryptoHelper,
                workingDirectory: workingDirectory);
                    string data = originalStdIn.ReadLine();
                        sessionTM ??= CreateSessionTransportManager(
                            // give a chance to runspace/pipelines to close (as it looks like the client died
                            // intermittently)
                            sessionTM.Close(null);
                    // process data in a thread pool thread..this way Runspace, Command
                    // data can be processed concurrently.
                            _windowsIdentityToImpersonate,
                            new WaitCallback(ProcessingThreadStart),
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessingThreadStart), data);
                    originalStdErr.WriteLine(e.Message);
    internal sealed class StdIOProcessMediator : OutOfProcessMediatorBase
        private static StdIOProcessMediator s_singletonInstance;
        /// The mediator will take actions from the StdIn stream and responds to them.
        /// It will replace StdIn,StdOut and StdErr stream with TextWriter.Null. This is
        /// to make sure these streams are totally used by our Mediator.
        /// <param name="combineErrOutStream">Redirects remoting errors to the Out stream.</param>
        private StdIOProcessMediator(bool combineErrOutStream) : base(exitProcessOnError: true)
            // Create input stream reader from Console standard input stream.
            // We don't use the provided Console.In TextReader because it can have
            // an incorrect encoding, e.g., Hyper-V Container console where the
            // TextReader has incorrect default console encoding instead of the actual
            // stream encoding.  This way the stream encoding is determined by the
            // stream BOM as needed.
            originalStdIn = new StreamReader(Console.OpenStandardInput(), true);
            // Remoting errors can optionally be written to stdErr or stdOut with
            // special formatting.
            originalStdOut = new OutOfProcessTextWriter(Console.Out);
            if (combineErrOutStream)
                originalStdErr = new FormattedErrorTextWriter(Console.Out);
                originalStdErr = new OutOfProcessTextWriter(Console.Error);
            // Replacing StdIn, StdOut, StdErr with Null so that no other app messes with the
            // original streams.
            Console.SetIn(TextReader.Null);
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
        /// Starts the out-of-process powershell server instance.
        /// <param name="initialCommand">Specifies the initialization script.</param>
        /// <param name="workingDirectory">Specifies the initial working directory. The working directory is set before the initial command.</param>
        /// <param name="configurationName">Specifies an optional configuration name that configures the endpoint session.</param>
        /// <param name="configurationFile">Specifies an optional path to a configuration (.pssc) file for the session.</param>
        /// <param name="combineErrOutStream">Specifies the option to write remoting errors to stdOut stream, with special formatting.</param>
        internal static void Run(
            bool combineErrOutStream)
                if (s_singletonInstance != null)
                    Dbg.Assert(false, "Run should not be called multiple times");
                s_singletonInstance = new StdIOProcessMediator(combineErrOutStream);
            s_singletonInstance.Start(
                initialCommand: initialCommand,
                cryptoHelper: new PSRemotingCryptoHelperServer(),
                workingDirectory: workingDirectory,
                configurationFile: configurationFile);
    internal sealed class NamedPipeProcessMediator : OutOfProcessMediatorBase
        private static NamedPipeProcessMediator s_singletonInstance;
        private readonly RemoteSessionNamedPipeServer _namedPipeServer;
        internal bool IsDisposed
            get { return _namedPipeServer.IsDisposed; }
        private NamedPipeProcessMediator() : base(false) { }
        private NamedPipeProcessMediator(
            RemoteSessionNamedPipeServer namedPipeServer) : base(false)
            if (namedPipeServer == null)
                throw new PSArgumentNullException(nameof(namedPipeServer));
            _namedPipeServer = namedPipeServer;
            // Create transport reader/writers from named pipe.
            originalStdIn = namedPipeServer.TextReader;
            originalStdOut = new OutOfProcessTextWriter(namedPipeServer.TextWriter);
            originalStdErr = new FormattedErrorTextWriter(namedPipeServer.TextWriter);
            // Flow impersonation as needed.
            Utils.TryGetWindowsImpersonatedIdentity(out _windowsIdentityToImpersonate);
            RemoteSessionNamedPipeServer namedPipeServer)
                if (s_singletonInstance != null && !s_singletonInstance.IsDisposed)
                    Dbg.Assert(false, "Run should not be called multiple times, unless the singleton was disposed.");
                s_singletonInstance = new NamedPipeProcessMediator(namedPipeServer);
                configurationName: namedPipeServer.ConfigurationName,
                configurationFile: null);
    internal sealed class FormattedErrorTextWriter : OutOfProcessTextWriter
        internal FormattedErrorTextWriter(
            TextWriter textWriter) : base(textWriter)
        #region Base class overrides
        // Write error data to stream with 'ErrorPrefix' prefix that will
        // be interpreted by the client.
        public override void WriteLine(string data)
            string dataToWrite = (data != null) ? ErrorPrefix + data : null;
            base.WriteLine(dataToWrite);
    internal sealed class HyperVSocketMediator : OutOfProcessMediatorBase
        private static HyperVSocketMediator s_instance;
        private readonly RemoteSessionHyperVSocketServer _hypervSocketServer;
            get { return _hypervSocketServer.IsDisposed; }
        private HyperVSocketMediator()
            : base(false)
            _hypervSocketServer = new RemoteSessionHyperVSocketServer(false);
            originalStdIn = _hypervSocketServer.TextReader;
            originalStdOut = new OutOfProcessTextWriter(_hypervSocketServer.TextWriter);
            originalStdErr = new HyperVSocketErrorTextWriter(_hypervSocketServer.TextWriter);
        private HyperVSocketMediator(string token,
            DateTimeOffset tokenCreationTime)
            _hypervSocketServer = new RemoteSessionHyperVSocketServer(false, token: token, tokenCreationTime: tokenCreationTime);
                s_instance = new HyperVSocketMediator();
            s_instance.Start(
            string token,
                s_instance = new HyperVSocketMediator(token, tokenCreationTime);
    internal sealed class HyperVSocketErrorTextWriter : OutOfProcessTextWriter
        private const string _errorPrepend = "__HyperVSocketError__:";
        internal static string ErrorPrepend
            get { return _errorPrepend; }
        internal HyperVSocketErrorTextWriter(
            TextWriter textWriter)
            : base(textWriter)
            string dataToWrite = (data != null) ? _errorPrepend + data : null;

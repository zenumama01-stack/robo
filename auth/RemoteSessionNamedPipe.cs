using System.IO.Pipes;
    /// Shared named pipe utilities.
    internal static class NamedPipeUtils
        internal const string NamedPipeNamePrefix = "PSHost.";
        internal const string DefaultAppDomainName = "None";
        // This `CoreFxPipe` prefix is defined by CoreFx
        internal const string NamedPipeNamePrefixSearch = "CoreFxPipe_PSHost*";
        internal const string DefaultAppDomainName = "DefaultAppDomain";
        internal const string NamedPipeNamePrefixSearch = "PSHost*";
        // On non-Windows, .NET named pipes are limited to up to 104 characters
        internal const int MaxNamedPipeNameSize = 104;
        /// Create a pipe name based on process information.
        /// E.g., "PSHost.ProcessStartTime.ProcessId.DefaultAppDomain.ProcessName"
        /// <param name="procId">Process Id.</param>
        /// <returns>Pipe name.</returns>
        internal static string CreateProcessPipeName(
            int procId)
            return CreateProcessPipeName(
                System.Diagnostics.Process.GetProcessById(procId));
        /// <param name="proc">Process object.</param>
            System.Diagnostics.Process proc)
            return CreateProcessPipeName(proc, DefaultAppDomainName);
        /// Create a pipe name based on process Id and appdomain name information.
        /// <param name="appDomainName">Name of process app domain to connect to.</param>
            int procId,
            string appDomainName)
            return CreateProcessPipeName(System.Diagnostics.Process.GetProcessById(procId), appDomainName);
        /// Create a pipe name based on process and appdomain name information.
            System.Diagnostics.Process proc,
            if (proc == null)
                throw new PSArgumentNullException(nameof(proc));
                appDomainName = DefaultAppDomainName;
            System.Text.StringBuilder pipeNameBuilder = new System.Text.StringBuilder(MaxNamedPipeNameSize);
            pipeNameBuilder.Append(NamedPipeNamePrefix)
                // The starttime is there to prevent another process easily guessing the pipe name
                // and squatting on it.
                // There is a limit of 104 characters in total including the temp path to the named pipe file
                // on non-Windows systems, so we'll convert the starttime to hex and just take the first 8 characters.
                .Append(proc.StartTime.ToFileTime().ToString("X8").AsSpan(1, 8))
                .Append(proc.StartTime.ToFileTime().ToString(CultureInfo.InvariantCulture))
                .Append('.')
                .Append(proc.Id.ToString(CultureInfo.InvariantCulture))
                .Append(CleanAppDomainNameForPipeName(appDomainName))
                .Append(proc.ProcessName);
            int charsToTrim = pipeNameBuilder.Length - MaxNamedPipeNameSize;
            if (charsToTrim > 0)
                // TODO: In the case the pipe name is truncated, the user cannot connect to it using the cmdlet
                // unless we add a `-Force` type switch as it attempts to validate the current process name
                // matches the process name in the pipe name
                pipeNameBuilder.Remove(MaxNamedPipeNameSize + 1, charsToTrim);
            return pipeNameBuilder.ToString();
        private static string CleanAppDomainNameForPipeName(string appDomainName)
            // Pipe names cannot contain the ':' character.  Remove unwanted characters.
            return appDomainName.Replace(":", string.Empty).Replace(" ", string.Empty);
        /// Returns the current process AppDomain name.
        /// <returns>AppDomain Name string.</returns>
        internal static string GetCurrentAppDomainName()
#if CORECLR // There is only one AppDomain per application in CoreCLR, which would be the default
            return DefaultAppDomainName;
#else       // Use the AppDomain in which current powershell is running
            return AppDomain.CurrentDomain.IsDefaultAppDomain() ? DefaultAppDomainName : AppDomain.CurrentDomain.FriendlyName;
    /// Native API for Named Pipes.
    internal static class NamedPipeNative
        #region Pipe constants
        // Pipe open modes
        internal const uint PIPE_ACCESS_DUPLEX = 0x00000003;
        internal const uint PIPE_ACCESS_OUTBOUND = 0x00000002;
        internal const uint PIPE_ACCESS_INBOUND = 0x00000001;
        // Pipe modes
        internal const uint PIPE_TYPE_BYTE = 0x00000000;
        internal const uint PIPE_TYPE_MESSAGE = 0x00000004;
        internal const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        internal const uint FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000;
        internal const uint PIPE_WAIT = 0x00000000;
        internal const uint PIPE_NOWAIT = 0x00000001;
        internal const uint PIPE_READMODE_BYTE = 0x00000000;
        internal const uint PIPE_READMODE_MESSAGE = 0x00000002;
        internal const uint PIPE_ACCEPT_REMOTE_CLIENTS = 0x00000000;
        internal const uint PIPE_REJECT_REMOTE_CLIENTS = 0x00000008;
        // Pipe errors
        internal const uint ERROR_FILE_NOT_FOUND = 2;
        internal const uint ERROR_BROKEN_PIPE = 109;
        internal const uint ERROR_PIPE_BUSY = 231;
        internal const uint ERROR_NO_DATA = 232;
        internal const uint ERROR_MORE_DATA = 234;
        internal const uint ERROR_PIPE_CONNECTED = 535;
        internal const uint ERROR_IO_INCOMPLETE = 996;
        internal const uint ERROR_IO_PENDING = 997;
        #region Data structures
        internal class SECURITY_ATTRIBUTES
            /// The size, in bytes, of this structure. Set this value to the size of the SECURITY_ATTRIBUTES structure.
            public int NLength;
            /// A pointer to a security descriptor for the object that controls the sharing of it.
            public IntPtr LPSecurityDescriptor = IntPtr.Zero;
            /// A Boolean value that specifies whether the returned handle is inherited when a new process is created.
            public bool InheritHandle;
            /// Initializes a new instance of the SECURITY_ATTRIBUTES class.
                this.NLength = 12;
        #region Pipe methods
        [DllImport(PinvokeDllNames.CreateNamedPipeDllName, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern SafePipeHandle CreateNamedPipe(
           string lpName,
           uint dwOpenMode,
           uint dwPipeMode,
           uint nMaxInstances,
           uint nOutBufferSize,
           uint nInBufferSize,
           uint nDefaultTimeOut,
           SECURITY_ATTRIBUTES securityAttributes);
        internal static SECURITY_ATTRIBUTES GetSecurityAttributes(GCHandle securityDescriptorPinnedHandle, bool inheritHandle = false)
            SECURITY_ATTRIBUTES securityAttributes = new NamedPipeNative.SECURITY_ATTRIBUTES();
            securityAttributes.InheritHandle = inheritHandle;
            securityAttributes.NLength = (int)Marshal.SizeOf(securityAttributes);
            securityAttributes.LPSecurityDescriptor = securityDescriptorPinnedHandle.AddrOfPinnedObject();
            return securityAttributes;
    /// Event arguments for listener thread end event.
    internal sealed class ListenerEndedEventArgs : EventArgs
        /// Exception reason for listener end event.  Can be null
        /// which indicates listener thread end is not due to an error.
        /// True if listener should be restarted after ending.
        public bool RestartListener { get; }
        private ListenerEndedEventArgs() { }
        /// <param name="reason">Listener end reason.</param>
        /// <param name="restartListener">Restart listener.</param>
        public ListenerEndedEventArgs(
            Exception reason,
            bool restartListener)
            RestartListener = restartListener;
    /// Light wrapper class for BCL NamedPipeServerStream class, that
    /// creates the named pipe server with process named pipe name,
    /// having correct access restrictions, and provides a listener
    /// thread loop.
    public sealed class RemoteSessionNamedPipeServer : IDisposable
        private const string _threadName = "IPC Listener Thread";
        private const int _namedPipeBufferSizeForRemoting = 32768;
        private const int _maxPipePathLengthLinux = 108;
        private const int _maxPipePathLengthMacOS = 104;
        // Singleton server.
        internal static RemoteSessionNamedPipeServer IPCNamedPipeServer;
        internal static bool IPCNamedPipeServerEnabled;
        // Optional custom server.
        private static RemoteSessionNamedPipeServer _customNamedPipeServer;
        // Access mask constant taken from PipeSecurity access rights and is equivalent to
        // PipeAccessRights.FullControl.
        // See: https://msdn.microsoft.com/library/vstudio/bb348408(v=vs.100).aspx
        private const int _pipeAccessMaskFullControl = 0x1f019f;
        /// Returns the Named Pipe stream object.
        internal NamedPipeServerStream Stream { get; }
        /// Returns the Named Pipe name.
        internal string PipeName { get; }
        /// Returns true if listener is currently running.
        internal bool IsListenerRunning { get; private set; }
        /// Name of session configuration.
        /// Accessor for the named pipe reader.
        internal StreamReader TextReader { get; private set; }
        /// Accessor for the named pipe writer.
        internal StreamWriter TextWriter { get; private set; }
        internal bool IsDisposed { get; private set; }
        /// Buffer size for PSRP fragmentor.
        internal static int NamedPipeBufferSizeForRemoting
            get { return _namedPipeBufferSizeForRemoting; }
        /// Event raised when the named pipe server listening thread
        /// ends.
        internal event EventHandler<ListenerEndedEventArgs> ListenerEnded;
        /// Creates a RemoteSessionNamedPipeServer with the current process and AppDomain information.
        /// <returns>RemoteSessionNamedPipeServer.</returns>
        internal static RemoteSessionNamedPipeServer CreateRemoteSessionNamedPipeServer()
            string appDomainName = NamedPipeUtils.GetCurrentAppDomainName();
            return new RemoteSessionNamedPipeServer(NamedPipeUtils.CreateProcessPipeName(
                System.Diagnostics.Process.GetCurrentProcess(), appDomainName));
        /// Constructor.  Creates named pipe server with provided pipe name.
        /// <param name="pipeName">Named Pipe name.</param>
        internal RemoteSessionNamedPipeServer(
            string pipeName)
            if (pipeName == null)
                throw new PSArgumentNullException(nameof(pipeName));
            PipeName = pipeName;
            Stream = CreateNamedPipe(
                serverName: ".",
                namespaceName: "pipe",
                coreName: pipeName,
                securityDesc: GetServerPipeSecurity());
        /// Helper method to create a PowerShell transport named pipe via native API, along
        /// with a returned .Net NamedPipeServerStream object wrapping the named pipe.
        /// <param name="serverName">Named pipe server name.</param>
        /// <param name="namespaceName">Named pipe namespace name.</param>
        /// <param name="coreName">Named pipe core name.</param>
        /// <param name="securityDesc"></param>
        /// <returns>NamedPipeServerStream.</returns>
        private static NamedPipeServerStream CreateNamedPipe(
            string coreName,
            CommonSecurityDescriptor securityDesc)
            if (serverName == null) { throw new PSArgumentNullException(nameof(serverName)); }
            if (namespaceName == null) { throw new PSArgumentNullException(nameof(namespaceName)); }
            if (coreName == null) { throw new PSArgumentNullException(nameof(coreName)); }
            string fullPipeName = @"\\" + serverName + @"\" + namespaceName + @"\" + coreName;
            // Create optional security attributes based on provided PipeSecurity.
            NamedPipeNative.SECURITY_ATTRIBUTES securityAttributes = null;
            GCHandle? securityDescHandle = null;
            if (securityDesc != null)
                byte[] securityDescBuffer = new byte[securityDesc.BinaryLength];
                securityDesc.GetBinaryForm(securityDescBuffer, 0);
                securityDescHandle = GCHandle.Alloc(securityDescBuffer, GCHandleType.Pinned);
                securityAttributes = NamedPipeNative.GetSecurityAttributes(securityDescHandle.Value);
            // Create named pipe.
            SafePipeHandle pipeHandle = NamedPipeNative.CreateNamedPipe(
                fullPipeName,
                NamedPipeNative.PIPE_ACCESS_DUPLEX | NamedPipeNative.FILE_FLAG_FIRST_PIPE_INSTANCE | NamedPipeNative.FILE_FLAG_OVERLAPPED,
                NamedPipeNative.PIPE_TYPE_MESSAGE | NamedPipeNative.PIPE_READMODE_MESSAGE | NamedPipeNative.PIPE_REJECT_REMOTE_CLIENTS,
                _namedPipeBufferSizeForRemoting,
                securityAttributes);
            securityDescHandle?.Free();
            if (pipeHandle.IsInvalid)
                    StringUtil.Format(RemotingErrorIdStrings.CannotCreateNamedPipe, lastError));
            // Create the .Net NamedPipeServerStream wrapper.
                return new NamedPipeServerStream(
                    PipeDirection.InOut,
                    true,                       // IsAsync
                    false,                      // IsConnected
                    pipeHandle);
                pipeHandle.Dispose();
                pipeName: coreName,
                direction: PipeDirection.InOut,
                maxNumberOfServerInstances: 1,
                transmissionMode: PipeTransmissionMode.Byte,
                options: PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly,
                inBufferSize: _namedPipeBufferSizeForRemoting,
                outBufferSize: _namedPipeBufferSizeForRemoting);
        static RemoteSessionNamedPipeServer()
            // Unless opt-out, all PowerShell instances will start with the named-pipe listener created and running.
            IPCNamedPipeServerEnabled = !Utils.GetEnvironmentVariableAsBool(name: "POWERSHELL_DIAGNOSTICS_OPTOUT", defaultValue: false);
            if (IPCNamedPipeServerEnabled)
                CreateIPCNamedPipeServerSingleton();
                CreateProcessExitHandler();
                if (IsDisposed) { return; }
        /// Creates the custom named pipe server with the given pipename.
        /// <param name="pipeName">The name of the pipe to create.</param>
        public static void CreateCustomNamedPipeServer(string pipeName)
                if (_customNamedPipeServer != null && !_customNamedPipeServer.IsDisposed)
                    if (pipeName == _customNamedPipeServer.PipeName)
                        // we shouldn't recreate the server object if we're using the same pipeName
                    // Dispose of the current pipe server so we can create a new one with the new pipeName
                    _customNamedPipeServer.Dispose();
                if (!Platform.IsWindows)
                    int maxNameLength = (Platform.IsLinux ? _maxPipePathLengthLinux : _maxPipePathLengthMacOS) - Path.GetTempPath().Length;
                    if (pipeName.Length > maxNameLength)
                                RemotingErrorIdStrings.CustomPipeNameTooLong,
                                pipeName,
                                pipeName.Length));
                        _customNamedPipeServer = new RemoteSessionNamedPipeServer(pipeName);
                        // Expected when named pipe server for this process already exists.
                        // This can happen if process has multiple AppDomains hosting PowerShell (SMA.dll).
                    // Listener ended callback, used to create listening new pipe server.
                    _customNamedPipeServer.ListenerEnded += OnCustomNamedPipeServerEnded;
                    // Start the pipe server listening thread, and provide client connection callback.
                    _customNamedPipeServer.StartListening(ClientConnectionCallback);
                    _customNamedPipeServer = null;
        /// Starts named pipe server listening thread.  When a client connects this thread
        /// makes a callback to implement the client communication.  When the thread ends
        /// this object is disposed and a new RemoteSessionNamedPipeServer must be created
        /// and a new listening thread started to handle subsequent client connections.
        /// <param name="clientConnectCallback">Connection callback.</param>
        internal void StartListening(
            Action<RemoteSessionNamedPipeServer> clientConnectCallback)
            if (clientConnectCallback == null)
                throw new PSArgumentNullException(nameof(clientConnectCallback));
                if (IsListenerRunning)
                    throw new InvalidOperationException(RemotingErrorIdStrings.NamedPipeAlreadyListening);
                IsListenerRunning = true;
                // Create listener thread.
                Thread listenerThread = new Thread(ProcessListeningThread);
                listenerThread.Name = _threadName;
                listenerThread.IsBackground = true;
                listenerThread.Start(clientConnectCallback);
        internal static CommonSecurityDescriptor GetServerPipeSecurity()
            // Built-in Admin SID
            SecurityIdentifier adminSID = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            DiscretionaryAcl dacl = new DiscretionaryAcl(false, false, 1);
            dacl.AddAccess(
                AccessControlType.Allow,
                adminSID,
                _pipeAccessMaskFullControl,
                InheritanceFlags.None,
                PropagationFlags.None);
            CommonSecurityDescriptor securityDesc = new CommonSecurityDescriptor(
                false, false,
                ControlFlags.DiscretionaryAclPresent | ControlFlags.OwnerDefaulted | ControlFlags.GroupDefaulted,
                null, null, null, dacl);
            // Conditionally add User SID
            bool isAdminElevated = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            if (!isAdminElevated)
                securityDesc.DiscretionaryAcl.AddAccess(
                    WindowsIdentity.GetCurrent().User,
            return securityDesc;
        /// Wait for client connection.
        private void WaitForConnection()
            Stream.WaitForConnection();
        /// Process listening thread.
        /// <param name="state">Client callback delegate.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle")]
        private void ProcessListeningThread(object state)
            string processId = Environment.ProcessId.ToString(CultureInfo.InvariantCulture);
            // Logging.
            _tracer.WriteMessage("RemoteSessionNamedPipeServer", "StartListening", Guid.Empty,
                "Listener thread started on Process {0} in AppDomainName {1}.", processId, appDomainName);
            PSEtwLog.LogOperationalInformation(
                PSEventId.NamedPipeIPC_ServerListenerStarted, PSOpcode.Open, PSTask.NamedPipe,
                processId, appDomainName);
            string userName = string.Empty;
            bool restartListenerThread = true;
            // Wait for connection.
                // Begin listening for a client connect.
                this.WaitForConnection();
                    userName = System.Environment.UserName;
                    userName = WindowsIdentity.GetCurrent().Name;
                    "Client connection started on Process {0} in AppDomainName {1} for User {2}.", processId, appDomainName, userName);
                    PSEventId.NamedPipeIPC_ServerConnect, PSOpcode.Connect, PSTask.NamedPipe,
                    processId, appDomainName, userName);
                // Error during connection handling.  Don't try to restart listening thread.
                    "Unexpected error in listener thread on process {0} in AppDomainName {1}.  Error Message: {2}", processId, appDomainName, errorMessage);
                PSEtwLog.LogOperationalError(PSEventId.NamedPipeIPC_ServerListenerError, PSOpcode.Exception, PSTask.NamedPipe,
                    processId, appDomainName, errorMessage);
            // Start server session on new connection.
                Action<RemoteSessionNamedPipeServer> clientConnectCallback = state as Action<RemoteSessionNamedPipeServer>;
                Dbg.Assert(clientConnectCallback != null, "Client callback should never be null.");
                // Handle a new client connect by making the callback.
                // The callback must handle all exceptions except
                // for a named pipe disposed or disconnected exception
                // which propagates up to the thread listener loop.
                clientConnectCallback(this);
                // Expected connection terminated.
                // Expected from PS transport close/dispose.
                restartListenerThread = false;
                "Client connection ended on process {0} in AppDomainName {1} for User {2}.", processId, appDomainName, userName);
                PSEventId.NamedPipeIPC_ServerDisconnect, PSOpcode.Close, PSTask.NamedPipe,
                // Normal listener exit.
                    "Listener thread ended on process {0} in AppDomainName {1}.", processId, appDomainName);
                PSEtwLog.LogOperationalInformation(PSEventId.NamedPipeIPC_ServerListenerEnded, PSOpcode.Close, PSTask.NamedPipe,
                IsListenerRunning = false;
            // Ensure this named pipe server object is disposed.
            ListenerEnded.SafeInvoke(
                new ListenerEndedEventArgs(ex, restartListenerThread));
        /// Ensures the namedpipe singleton server is running and waits for a client connection.
        /// This is a blocking call that returns after the client connection ends.
        /// This method supports PowerShell running in "NamedPipeServerMode", which is used for
        /// PowerShell Direct Windows Server Container connection and management.
        /// <param name="configurationName">Name of the configuration to use.</param>
        internal static void RunServerMode(string configurationName)
            IPCNamedPipeServerEnabled = true;
            if (IPCNamedPipeServer == null)
                throw new RuntimeException(RemotingErrorIdStrings.NamedPipeServerCannotStart);
            IPCNamedPipeServer.ConfigurationName = configurationName;
            ManualResetEventSlim clientConnectionEnded = new ManualResetEventSlim(false);
            IPCNamedPipeServer.ListenerEnded -= OnIPCNamedPipeServerEnded;
            IPCNamedPipeServer.ListenerEnded += (sender, e) => clientConnectionEnded.Set();
            // Wait for server to service a single client connection.
            clientConnectionEnded.Wait();
            clientConnectionEnded.Dispose();
            IPCNamedPipeServerEnabled = false;
        /// Creates the process named pipe server object singleton and
        /// starts the client listening thread.
        internal static void CreateIPCNamedPipeServerSingleton()
                if (!IPCNamedPipeServerEnabled) { return; }
                if (IPCNamedPipeServer == null || IPCNamedPipeServer.IsDisposed)
                            IPCNamedPipeServer = CreateRemoteSessionNamedPipeServer();
                        IPCNamedPipeServer.ListenerEnded += OnIPCNamedPipeServerEnded;
                        IPCNamedPipeServer.StartListening(ClientConnectionCallback);
                        IPCNamedPipeServer = null;
        private static void CreateProcessExitHandler()
            AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
                RemoteSessionNamedPipeServer namedPipeServer = IPCNamedPipeServer;
                if (namedPipeServer != null)
                        // Terminate the IPC thread.
                        namedPipeServer.Dispose();
                        // Ignore if object already disposed.
                        // Don't throw an exception on the app domain unload event thread.
        private static void OnIPCNamedPipeServerEnded(object sender, ListenerEndedEventArgs args)
            if (args.RestartListener)
        private static void OnCustomNamedPipeServerEnded(object sender, ListenerEndedEventArgs args)
            if (args.RestartListener && sender is RemoteSessionNamedPipeServer server)
                CreateCustomNamedPipeServer(server.PipeName);
        private static void ClientConnectionCallback(RemoteSessionNamedPipeServer pipeServer)
            // Create server mediator object and begin remote session with client.
            NamedPipeProcessMediator.Run(
                pipeServer);
    /// Base class for RemoteSessionNamedPipeClient and ContainerSessionNamedPipeClient.
    internal class NamedPipeClientBase : IDisposable
        private NamedPipeClientStream _clientPipeStream;
        /// Name of pipe.
        public string PipeName
        /// Dispose object.
            if (_clientPipeStream != null)
                try { _clientPipeStream.Dispose(); }
        /// Connect to named pipe server.  This is a blocking call until a
        /// <param name="timeout">Connection attempt timeout in milliseconds.</param>
        public void Connect(
            int timeout)
            // Uses Native API to connect to pipe and return NamedPipeClientStream object.
            _clientPipeStream = DoConnect(timeout);
            TextReader = new StreamReader(_clientPipeStream);
            TextWriter = new StreamWriter(_clientPipeStream);
            _tracer.WriteMessage("NamedPipeClientBase", "Connect", Guid.Empty,
                "Connection started on pipe: {0}", PipeName);
        /// Closes the named pipe.
        public void Close() => _clientPipeStream?.Dispose();
        /// Abort connection attempt.
        public virtual void AbortConnect()
        /// Begin connection attempt.
        protected virtual NamedPipeClientStream DoConnect(int timeout)
    /// Light wrapper class for BCL NamedPipeClientStream class, that
    /// creates the named pipe name and initiates connection to
    /// target named pipe server.
    internal sealed class RemoteSessionNamedPipeClient : NamedPipeClientBase
        private volatile bool _connecting;
        private RemoteSessionNamedPipeClient()
        /// Constructor.  Creates Named Pipe based on process object.
        /// <param name="process">Target process object for pipe.</param>
        /// <param name="appDomainName">AppDomain name or null for default AppDomain.</param>
        public RemoteSessionNamedPipeClient(System.Diagnostics.Process process, string appDomainName)
            : this(NamedPipeUtils.CreateProcessPipeName(process, appDomainName))
        /// Constructor. Creates Named Pipe based on process Id.
        /// <param name="procId">Target process Id for pipe.</param>
        public RemoteSessionNamedPipeClient(int procId, string appDomainName)
            : this(NamedPipeUtils.CreateProcessPipeName(procId, appDomainName))
        /// Constructor. Creates Named Pipe based on name argument.
        internal RemoteSessionNamedPipeClient(
            // Defer creating the .Net NamedPipeClientStream object until we connect.
            // _clientPipeStream == null.
        /// <param name="serverName"></param>
        /// <param name="coreName"></param>
            string coreName)
            PipeName = @"\\" + serverName + @"\" + namespaceName + @"\" + coreName;
        public override void AbortConnect()
            _connecting = false;
        protected override NamedPipeClientStream DoConnect(int timeout)
            // Repeatedly attempt connection to pipe until timeout expires.
            int startTime = Environment.TickCount;
            int elapsedTime = 0;
            _connecting = true;
            NamedPipeClientStream namedPipeClientStream = new NamedPipeClientStream(
                pipeName: PipeName,
                options: PipeOptions.Asynchronous);
            namedPipeClientStream.ConnectAsync(timeout);
                if (!namedPipeClientStream.IsConnected)
                    elapsedTime = unchecked(Environment.TickCount - startTime);
                return namedPipeClientStream;
            } while (_connecting && (elapsedTime < timeout));
            throw new TimeoutException(RemotingErrorIdStrings.ConnectNamedPipeTimeout);
    /// target named pipe server inside Windows Server container.
    internal sealed class ContainerSessionNamedPipeClient : NamedPipeClientBase
        /// Constructor. Creates Named Pipe based on process Id, app domain name and container object root path.
        /// <param name="containerObRoot">Container OB root.</param>
        public ContainerSessionNamedPipeClient(
            string containerObRoot)
            if (string.IsNullOrEmpty(containerObRoot))
                throw new PSArgumentNullException(nameof(containerObRoot));
            // Named pipe inside Windows Server container is under different name space.
            PipeName = containerObRoot + @"\Device\NamedPipe\" +
                NamedPipeUtils.CreateProcessPipeName(procId, appDomainName);
        /// Helper method to open a named pipe via native APIs and return in
        /// .Net NamedPipeClientStream wrapper object.
            // TODO: `CreateFileWithSafePipeHandle` pinvoke below clearly says
            // that the code is only for Windows and we could exclude
            // a lot of code from compilation on Unix.
            throw new NotSupportedException(nameof(DoConnect));
            // WaitNamedPipe API is not supported by Windows Server container now, so we need to repeatedly
            // attempt connection to pipe server until timeout expires.
            nint handle;
                // Get handle to pipe.
                handle = Interop.Windows.CreateFileWithPipeHandle(
                    lpFileName: PipeName,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    FileMode.Open,
                    Interop.Windows.FileAttributes.Overlapped);
                if (handle == nint.Zero || handle == (nint)(-1))
                    int lastError = Marshal.GetLastPInvokeError();
                    if (lastError == Interop.Windows.ERROR_FILE_NOT_FOUND)
                            StringUtil.Format(RemotingErrorIdStrings.CannotConnectContainerNamedPipe, lastError));
            } while (elapsedTime < timeout);
            SafePipeHandle pipeHandle = null;
                pipeHandle = new SafePipeHandle(handle, ownsHandle: true);
                return new NamedPipeClientStream(
                    isAsync: true,
                pipeHandle?.Dispose();

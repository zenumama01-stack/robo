    /// This class implements New-PSSessionOption cmdlet.
    [Cmdlet(VerbsCommon.New, "PSSessionOption", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096488", RemotingCapability = RemotingCapability.None)]
    [OutputType(typeof(PSSessionOption))]
    public sealed class NewPSSessionOptionCommand : PSCmdlet
        #region Parameters (specific to PSSessionOption)
        /// The MaximumRedirection parameter enables the implicit redirection functionality
        /// -1 = no limit
        ///  0 = no redirection.
        public int MaximumRedirection
            get { return _maximumRedirection.Value; }
            set { _maximumRedirection = value; }
        private int? _maximumRedirection;
        /// If false, underlying WSMan infrastructure will compress data sent on the network.
        /// If true, data will not be compressed. Compression improves performance by
        /// reducing the amount of data sent on the network. Compression my require extra
        /// memory consumption and CPU usage. In cases where available memory / CPU is less,
        /// set this property to "true".
        /// By default the value of this property is "false".
        public SwitchParameter NoCompression { get; set; }
        /// If <see langword="true"/> then Operating System won't load the user profile (i.e. registry keys under HKCU) on the remote server
        /// which can result in a faster session creation time.  This option won't have any effect if the remote machine has
        /// already loaded the profile (i.e. in another session).
        public SwitchParameter NoMachineProfile { get; set; }
        /// Culture that the remote session should use.
        /// UI culture that the remote session should use.
        /// Total data (in bytes) that can be received from a remote machine
        /// targeted towards a command. If null, then the size is unlimited.
        /// Default is unlimited data.
        public int MaximumReceivedDataSizePerCommand
            get { return _maxRecvdDataSizePerCommand.Value; }
            set { _maxRecvdDataSizePerCommand = value; }
        private int? _maxRecvdDataSizePerCommand;
        /// Maximum size (in bytes) of a deserialized object received from a remote machine.
        /// If null, then the size is unlimited. Default is unlimited object size.
        public int MaximumReceivedObjectSize
            get { return _maxRecvdObjectSize.Value; }
            set { _maxRecvdObjectSize = value; }
        private int? _maxRecvdObjectSize;
        /// Specifies the output mode on the server when it is in Disconnected mode
        /// and its output data cache becomes full.
        public OutputBufferingMode OutputBufferingMode { get; set; }
        /// Maximum number of times a connection will be re-attempted when a connection fails due to network
        /// issues.
        public int MaxConnectionRetryCount { get; set; }
        public PSPrimitiveDictionary ApplicationArguments { get; set; }
        /// The duration for which PowerShell remoting waits (in milliseconds) before timing
        /// out on a connection to a remote machine. Simply put, the timeout for a remote
        /// runspace creation.
        /// The user would like to tweak this timeout depending on whether
        /// he/she is connecting to a machine in the data center or across a slow WAN.
        [Alias("OpenTimeoutMSec")]
        public int OpenTimeout
                return _openTimeout ?? RunspaceConnectionInfo.DefaultOpenTimeout;
                _openTimeout = value;
        private int? _openTimeout;
        /// The duration for which PowerShell should wait (in milliseconds) before it
        /// times out on cancel operations (close runspace or stop powershell). For
        /// instance, when the user hits ctrl-C, New-PSSession cmdlet tries to call a
        /// stop on all remote runspaces which are in the Opening state. The user
        /// wouldn't mind waiting for 15 seconds, but this should be time bound and of a
        /// shorter duration. A high timeout here like 3 minutes will give the user
        /// a feeling that the PowerShell client is not responding.
        [Alias("CancelTimeoutMSec")]
        public int CancelTimeout
                return _cancelTimeout ?? BaseTransportManager.ClientCloseTimeoutMs;
                _cancelTimeout = value;
        private int? _cancelTimeout;
        /// The duration for which a Runspace on server needs to wait (in milliseconds) before it
        /// declares the client dead and closes itself down.
        /// This is especially important as these values may have to be configured differently
        /// for enterprise administration scenarios.
        [ValidateRange(-1, Int32.MaxValue)]
        [Alias("IdleTimeoutMSec")]
        public int IdleTimeout
                return _idleTimeout ?? RunspaceConnectionInfo.DefaultIdleTimeout;
                _idleTimeout = value;
        private int? _idleTimeout;
        #region Parameters copied from New-WSManSessionOption
        /// By default, ProxyAccessType is None, that means Proxy information (ProxyAccessType,
        /// ProxyAuthenticationMechanism and ProxyCredential)is not passed to WSMan at all.
        public ProxyAccessType ProxyAccessType { get; set; } = ProxyAccessType.None;
        /// - Negotiate: Use the default authentication (as defined by the underlying
        public AuthenticationMechanism ProxyAuthentication { get; set; } = AuthenticationMechanism.Negotiate;
        public PSCredential ProxyCredential { get; set; }
            get { return _skipcacheck; }
            set { _skipcacheck = value; }
        private bool _skipcacheck;
            get { return _skipcncheck; }
            set { _skipcncheck = value; }
        private bool _skipcncheck;
            get { return _skiprevocationcheck; }
            set { _skiprevocationcheck = value; }
        private bool _skiprevocationcheck;
        /// Defines the timeout in milliseconds for the wsman operation.
                return _operationtimeout ?? BaseTransportManager.ClientDefaultOperationTimeoutMs;
                _operationtimeout = value;
        private int? _operationtimeout;
                return _noencryption;
                _noencryption = value;
        private bool _noencryption;
                return _useutf16;
                _useutf16 = value;
        private bool _useutf16;
        /// Uses Service Principal Name (SPN) along with the Port number during authentication.
        public SwitchParameter IncludePortInSPN
            get { return _includePortInSPN; }
            set { _includePortInSPN = value; }
        private bool _includePortInSPN;
            PSSessionOption result = new PSSessionOption();
            // Begin: WSMan specific options
            result.ProxyAccessType = this.ProxyAccessType;
            result.ProxyAuthentication = this.ProxyAuthentication;
            result.ProxyCredential = this.ProxyCredential;
            result.SkipCACheck = this.SkipCACheck;
            result.SkipCNCheck = this.SkipCNCheck;
            result.SkipRevocationCheck = this.SkipRevocationCheck;
            if (_operationtimeout.HasValue)
                result.OperationTimeout = TimeSpan.FromMilliseconds(_operationtimeout.Value);
            result.NoEncryption = this.NoEncryption;
            result.UseUTF16 = this.UseUTF16;
            result.IncludePortInSPN = this.IncludePortInSPN;
            // End: WSMan specific options
            if (_maximumRedirection.HasValue)
                result.MaximumConnectionRedirectionCount = this.MaximumRedirection;
            result.NoCompression = this.NoCompression.IsPresent;
            result.NoMachineProfile = this.NoMachineProfile.IsPresent;
            result.MaximumReceivedDataSizePerCommand = _maxRecvdDataSizePerCommand;
            result.MaximumReceivedObjectSize = _maxRecvdObjectSize;
                result.Culture = this.Culture;
                result.UICulture = this.UICulture;
            if (_openTimeout.HasValue)
                result.OpenTimeout = TimeSpan.FromMilliseconds(_openTimeout.Value);
            if (_cancelTimeout.HasValue)
                result.CancelTimeout = TimeSpan.FromMilliseconds(_cancelTimeout.Value);
            if (_idleTimeout.HasValue)
                result.IdleTimeout = TimeSpan.FromMilliseconds(_idleTimeout.Value);
            result.OutputBufferingMode = OutputBufferingMode;
            result.MaxConnectionRetryCount = MaxConnectionRetryCount;
            if (this.ApplicationArguments != null)
                result.ApplicationArguments = this.ApplicationArguments;
            this.WriteObject(result);

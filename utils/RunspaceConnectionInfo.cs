using WSManAuthenticationMechanism = System.Management.Automation.Remoting.Client.WSManNativeApi.WSManAuthenticationMechanism;
// ReSharper disable CheckNamespace
// ReSharper restore CheckNamespace
    /// Different Authentication Mechanisms supported by New-Runspace command to connect
    /// to remote server.
        /// Use the default authentication (as defined by the underlying protocol)
        /// for establishing a remote connection.
        /// Use Basic authentication for establishing a remote connection.
        Basic = 0x1,
        /// Use Negotiate authentication for establishing a remote connection.
        Negotiate = 0x2,
        /// Allow implicit credentials for Negotiate.
        NegotiateWithImplicitCredential = 0x3,
        /// Use CredSSP authentication for establishing a remote connection.
        Credssp = 0x4,
        /// Use Digest authentication mechanism. Digest authentication operates much
        /// like Basic authentication. However, unlike Basic authentication, Digest authentication
        /// transmits credentials across the network as a hash value, also known as a message digest.
        /// The user name and password cannot be deciphered from the hash value. Conversely, Basic
        /// authentication sends a Base 64 encoded password, essentially in clear text, across the
        /// network.
        Digest = 0x5,
        /// Use Kerberos authentication for establishing a remote connection.
        Kerberos = 0x6,
    /// Specify the type of access mode that should be
    /// used when creating a session configuration.
    public enum PSSessionConfigurationAccessMode
        /// Disable the configuration.
        /// Allow local access.
        Local = 1,
        /// Default allow remote access.
        Remote = 2,
    /// WSManTransportManager supports disconnected PowerShell sessions.
    /// When a remote PS session server is in disconnected state, output
    /// from the running command pipeline is cached on the server.  This
    /// enum determines what the server does when the cache is full.
    public enum OutputBufferingMode
        /// No output buffering mode specified.  Output buffering mode on server will
        /// default to Block if a new session is created, or will retain its current
        /// mode for non-creation scenarios (e.g., disconnect/connect operations).
        /// Command pipeline execution continues, excess output is dropped in FIFO manner.
        Drop = 1,
        /// Command pipeline execution on server is blocked until session is reconnected.
        Block = 2
    /// Class which defines connection path to a remote runspace
    /// that needs to be created. Transport specific connection
    /// paths will be derived from this.
    public abstract class RunspaceConnectionInfo
        /// Name of the computer.
        public abstract string ComputerName { get; set; }
        /// Credential used for the connection.
        public abstract PSCredential Credential { get; set; }
        /// Authentication mechanism to use while connecting to the server.
        public abstract AuthenticationMechanism AuthenticationMechanism { get; set; }
        /// ThumbPrint of a certificate used for connecting to a remote machine.
        /// When this is specified, you dont need to supply credential and authentication
        /// mechanism.
        public abstract string CertificateThumbprint { get; set; }
        public CultureInfo Culture
                return _culture;
                _culture = value;
        private CultureInfo _culture = CultureInfo.CurrentCulture;
        public CultureInfo UICulture
                return _uiCulture;
                _uiCulture = value;
        private CultureInfo _uiCulture = CultureInfo.CurrentUICulture;
        /// The duration (in ms) for which PowerShell remoting waits before timing out on a connection to a remote machine.
        /// The administrator would like to tweak this timeout depending on whether
                return _openTimeout;
                if (this is WSManConnectionInfo && _openTimeout == DefaultTimeout)
                    _openTimeout = DefaultOpenTimeout;
                else if (this is WSManConnectionInfo && _openTimeout == InfiniteTimeout)
                    // this timeout value gets passed to a
                    // timer associated with the session
                    // data structure handler state machine.
                    // The timer constructor will throw an exception
                    // for any value greater than Int32.MaxValue
                    // hence this is the maximum possible limit
                    _openTimeout = Int32.MaxValue;
        private int _openTimeout = DefaultOpenTimeout;
        internal const int DefaultOpenTimeout = 3 * 60 * 1000; // 3 minutes
        internal const int DefaultTimeout = -1;
        internal const int InfiniteTimeout = 0;
        /// The duration (in ms) for which PowerShell should wait before it times out on cancel operations
        /// The administrator wouldn't mind waiting for 15 seconds, but this should be time bound and of a shorter duration.
        /// A high timeout here like 3 minutes will give the administrator a feeling that the PowerShell client is not responding.
        public int CancelTimeout { get; set; } = defaultCancelTimeout;
        internal const int defaultCancelTimeout = BaseTransportManager.ClientCloseTimeoutMs;
        public int OperationTimeout { get; set; } = BaseTransportManager.ClientDefaultOperationTimeoutMs;
        /// The duration (in ms) for which a Runspace on server needs to wait before it declares the client dead and closes itself down.
        public int IdleTimeout { get; set; } = DefaultIdleTimeout;
        internal const int DefaultIdleTimeout = BaseTransportManager.UseServerDefaultIdleTimeout;
        /// The maximum allowed idle timeout duration (in ms) that can be set on a Runspace.  This is a read-only property
        /// that is set once the Runspace is successfully created and opened.
        public int MaxIdleTimeout { get; internal set; } = Int32.MaxValue;
        /// Populates session options from a PSSessionOption instance.
        public virtual void SetSessionOptions(PSSessionOption options)
            ArgumentNullException.ThrowIfNull(options);
            if (options.Culture != null)
                this.Culture = options.Culture;
            if (options.UICulture != null)
                this.UICulture = options.UICulture;
            _openTimeout = TimeSpanToTimeOutMs(options.OpenTimeout);
            CancelTimeout = TimeSpanToTimeOutMs(options.CancelTimeout);
            OperationTimeout = TimeSpanToTimeOutMs(options.OperationTimeout);
            // Special case for idle timeout.  A value of milliseconds == -1
            // (BaseTransportManager.UseServerDefaultIdleTimeout) is allowed for
            // specifying the default value on the server.
            IdleTimeout = (options.IdleTimeout.TotalMilliseconds >= BaseTransportManager.UseServerDefaultIdleTimeout &&
                                options.IdleTimeout.TotalMilliseconds < int.MaxValue)
                                ? (int)(options.IdleTimeout.TotalMilliseconds) : int.MaxValue;
        internal int TimeSpanToTimeOutMs(TimeSpan t)
            if ((t.TotalMilliseconds > int.MaxValue) || (t == TimeSpan.MaxValue) || (t.TotalMilliseconds < 0))
                return (int)(t.TotalMilliseconds);
        /// Validates port number is in range.
        /// <param name="port">Port number to validate.</param>
        internal virtual void ValidatePortInRange(int port)
            if ((port < MinPort || port > MaxPort))
                        RemotingErrorIdStrings.PortIsOutOfRange, port);
                ArgumentException e = new ArgumentException(message);
        /// Creates the appropriate client session transportmanager.
        /// <param name="instanceId">Runspace/Pool instance Id.</param>
        /// <param name="sessionName">Session name.</param>
        /// <param name="cryptoHelper">PSRemotingCryptoHelper.</param>
        public virtual BaseClientSessionTransportManager CreateClientSessionTransportManager(
            string sessionName,
            PSRemotingCryptoHelper cryptoHelper)
        /// Create a copy of the connection info object.
        /// <returns>Copy of the connection info object.</returns>
        public virtual RunspaceConnectionInfo Clone()
        /// Maximum value for port.
        protected const int MaxPort = 0xFFFF;
        /// Minimum value for port.
        protected const int MinPort = 0;
    /// Class which defines path to a remote runspace that
    /// need to be created.
    public sealed class WSManConnectionInfo : RunspaceConnectionInfo
        /// Uri associated with this connection path.
        public Uri ConnectionUri
                return _connectionUri;
                UpdateUri(value);
        public override string ComputerName
                // null or empty value allowed
                ConstructUri(_scheme, value, null, _appName);
        /// Scheme used for connection.
        public string Scheme
                return _scheme;
                ConstructUri(value, _computerName, null, _appName);
        /// Port in which to connect.
                return ConnectionUri.Port;
                ConstructUri(_scheme, _computerName, value, _appName);
        /// AppName which identifies the connection
        /// end point in the machine.
        public string AppName
                ConstructUri(_scheme, _computerName, null, value);
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "System.Management.Automation.Runspaces.WSManConnectionInfo.#ShellUri")]
        public string ShellUri
                return _shellUri;
                _shellUri = ResolveShellUri(value);
        public override AuthenticationMechanism AuthenticationMechanism
                switch (WSManAuthenticationMechanism)
                    case WSManAuthenticationMechanism.WSMAN_FLAG_DEFAULT_AUTHENTICATION:
                        return AuthenticationMechanism.Default;
                    case WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_BASIC:
                        return AuthenticationMechanism.Basic;
                    case WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_CREDSSP:
                        return AuthenticationMechanism.Credssp;
                    case WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_NEGOTIATE:
                        if (AllowImplicitCredentialForNegotiate)
                            return AuthenticationMechanism.NegotiateWithImplicitCredential;
                        return AuthenticationMechanism.Negotiate;
                    case WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_DIGEST:
                        return AuthenticationMechanism.Digest;
                    case WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_KERBEROS:
                        return AuthenticationMechanism.Kerberos;
                        Dbg.Assert(false, "Invalid authentication mechanism detected.");
                        WSManAuthenticationMechanism = WSManAuthenticationMechanism.WSMAN_FLAG_DEFAULT_AUTHENTICATION;
                        WSManAuthenticationMechanism = WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_BASIC;
                        WSManAuthenticationMechanism = WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_NEGOTIATE;
                    case AuthenticationMechanism.NegotiateWithImplicitCredential:
                        AllowImplicitCredentialForNegotiate = true;
                        WSManAuthenticationMechanism = WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_CREDSSP;
                        WSManAuthenticationMechanism = WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_DIGEST;
                        WSManAuthenticationMechanism = WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_KERBEROS;
        /// AuthenticationMechanism converted to WSManAuthenticationMechanism type.
        /// This is internal.
        internal WSManAuthenticationMechanism WSManAuthenticationMechanism { get; private set; } = WSManAuthenticationMechanism.WSMAN_FLAG_DEFAULT_AUTHENTICATION;
        /// Allow default credentials for Negotiate.
        internal bool AllowImplicitCredentialForNegotiate { get; private set; }
        /// Returns the actual port property value and not the ConnectionUri port.
        /// Internal only.
        internal int PortSetting { get; private set; } = -1;
        /// Maximum uri redirection count.
        public int MaximumConnectionRedirectionCount { get; set; }
        internal const int defaultMaximumConnectionRedirectionCount = 5;
        public int? MaximumReceivedObjectSize { get; set; }
        /// If true, underlying WSMan infrastructure will compress data sent on the network.
        /// If false, data will not be compressed. Compression improves performance by
        /// set this property to false.
        /// By default the value of this property is "true".
        public bool UseCompression { get; set; } = true;
        public bool NoMachineProfile { get; set; }
        // BEGIN: Session Options
        /// By default, wsman uses IEConfig - the current user
        ///  under a user context different then the interactive user, the client
                return _proxyCredential;
                if (ProxyAccessType == ProxyAccessType.None)
                    string message = PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.ProxyCredentialWithoutAccess,
                                ProxyAccessType.None);
                _proxyCredential = value;
        // END: Session Options
        public OutputBufferingMode OutputBufferingMode { get; set; } = DefaultOutputBufferingMode;
        /// When true and in loopback scenario (localhost) this enables creation of WSMan
        public bool EnableNetworkAccess { get; set; }
        /// Specifies the maximum number of connection retries if previous connection attempts fail
        /// due to network issues.
        public int MaxConnectionRetryCount { get; set; } = DefaultMaxConnectionRetryCount;
        /// Constructor used to create a WSManConnectionInfo.
        /// <param name="computerName">Computer to connect to.</param>
        /// <param name="scheme">Scheme to be used for connection.</param>
        /// <param name="port">Port to connect to.</param>
        /// <param name="appName">Application end point to connect to.</param>
        /// <param name="shellUri">remote shell to launch
        /// on connection</param>
        /// <param name="credential">credential to be used
        /// for connection</param>
        /// <param name="openTimeout">Timeout in milliseconds for open
        /// call on Runspace to finish</param>
        /// <exception cref="ArgumentException">Invalid
        /// scheme or invalid port is specified</exception>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Scope = "member", Target = "System.Management.Automation.Runspaces.WSManConnectionInfo.#.ctor(System.String,System.String,System.Int32,System.String,System.String,System.Management.Automation.PSCredential,System.Int64,System.Int64)", MessageId = "4#")]
        public WSManConnectionInfo(string scheme, string computerName, int port, string appName, string shellUri, PSCredential credential, int openTimeout)
            Scheme = scheme;
            AppName = appName;
            ShellUri = shellUri;
            OpenTimeout = openTimeout;
        /// <remarks>max server life timeout and open timeout are
        /// default in this case</remarks>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Scope = "member", Target = "System.Management.Automation.Runspaces.WSManConnectionInfo.#.ctor(System.String,System.String,System.Int32,System.String,System.String,System.Management.Automation.PSCredential)", MessageId = "4#")]
        public WSManConnectionInfo(
            string scheme,
            int port,
            string appName,
            string shellUri,
                scheme,
                appName,
                credential,
                DefaultOpenTimeout)
        /// <param name="useSsl"></param>
        /// <param name="appName"></param>
        /// <param name="shellUri"></param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "4#")]
            bool useSsl,
                  useSsl ? DefaultSslScheme : DefaultScheme,
                  credential)
        /// <param name="openTimeout"></param>
            int openTimeout)
                  openTimeout)
        /// Creates a WSManConnectionInfo for the following URI
        /// and with the default credentials, default server
        /// life time and default open timeout
        ///        http://localhost/
        /// The default shellname Microsoft.PowerShell will be
        /// used.
        public WSManConnectionInfo()
            // ConstructUri(DefaultScheme, DefaultComputerName, DefaultPort, DefaultAppName);
            UseDefaultWSManPort = true;
        /// Constructor to create a WSManConnectionInfo with a uri
        /// and explicit credentials - server life time is
        /// default and open timeout is default.
        /// <param name="uri">Uri of remote runspace.</param>
        /// <param name="credential">credentials to use to
        /// connect to the remote runspace</param>
        /// <exception cref="ArgumentException">When an
        /// uri representing an invalid path is specified</exception>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Scope = "member", Target = "System.Management.Automation.Runspaces.WSManConnectionInfo.#.ctor(System.Uri,System.String,System.Management.Automation.PSCredential)", MessageId = "1#")]
        public WSManConnectionInfo(Uri uri, string shellUri, PSCredential credential)
            if (uri == null)
                // if the uri is null..apply wsman default logic for port
                // resolution..BUG 542726
            if (!uri.IsAbsoluteUri)
                throw new NotSupportedException(PSRemotingErrorInvariants.FormatResourceString
                                                    (RemotingErrorIdStrings.RelativeUriForRunspacePathNotSupported));
            // This check is needed to make sure we connect to WSMan app in the
            // default case (when user did not specify any appname) like
            // http://localhost , http://127.0.0.1 etc.
            if (uri.AbsolutePath.Equals("/", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrEmpty(uri.Query) && string.IsNullOrEmpty(uri.Fragment))
                ConstructUri(uri.Scheme,
                             uri.Host,
                             uri.Port,
                             s_defaultAppName);
                ConnectionUri = uri;
        /// Constructor used to create a WSManConnectionInfo. This constructor supports a certificate thumbprint to
        /// be used while connecting to a remote machine instead of credential.
        /// <param name="certificateThumbprint">
        /// A thumb print of the certificate to use while connecting to the remote machine.
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#")]
        public WSManConnectionInfo(Uri uri, string shellUri, string certificateThumbprint)
            : this(uri, shellUri, (PSCredential)null)
            _thumbPrint = certificateThumbprint;
        /// Constructor to create a WSManConnectionInfo with a
        /// uri specified and the default credentials,
        /// default server life time and default open
        /// timeout.
        public WSManConnectionInfo(Uri uri)
            : this(uri, DefaultShellUri, DefaultCredential)
        /// 1. Proxy credential cannot be specified when proxy accesstype is None.
        /// Either specify a valid proxy accesstype other than None or do not specify proxy credential.
        public override void SetSessionOptions(PSSessionOption options)
            if ((options.ProxyAccessType == ProxyAccessType.None) && (options.ProxyCredential != null))
            base.SetSessionOptions(options);
            this.MaximumConnectionRedirectionCount =
                options.MaximumConnectionRedirectionCount >= 0
                        ? options.MaximumConnectionRedirectionCount : int.MaxValue;
            this.MaximumReceivedDataSizePerCommand = options.MaximumReceivedDataSizePerCommand;
            this.MaximumReceivedObjectSize = options.MaximumReceivedObjectSize;
            this.UseCompression = !(options.NoCompression);
            this.NoMachineProfile = options.NoMachineProfile;
            ProxyAccessType = options.ProxyAccessType;
            _proxyAuthentication = options.ProxyAuthentication;
            _proxyCredential = options.ProxyCredential;
            SkipCACheck = options.SkipCACheck;
            SkipCNCheck = options.SkipCNCheck;
            SkipRevocationCheck = options.SkipRevocationCheck;
            NoEncryption = options.NoEncryption;
            UseUTF16 = options.UseUTF16;
            IncludePortInSPN = options.IncludePortInSPN;
            OutputBufferingMode = options.OutputBufferingMode;
            MaxConnectionRetryCount = options.MaxConnectionRetryCount;
        public override RunspaceConnectionInfo Clone()
            return Copy();
        /// Does a shallow copy of the current instance.
        public WSManConnectionInfo Copy()
            WSManConnectionInfo result = new WSManConnectionInfo();
            result._connectionUri = _connectionUri;
            result._computerName = _computerName;
            result._scheme = _scheme;
            result.PortSetting = PortSetting;
            result._appName = _appName;
            result._shellUri = _shellUri;
            result._credential = _credential;
            result.UseDefaultWSManPort = UseDefaultWSManPort;
            result.WSManAuthenticationMechanism = WSManAuthenticationMechanism;
            result.MaximumConnectionRedirectionCount = MaximumConnectionRedirectionCount;
            result.MaximumReceivedDataSizePerCommand = MaximumReceivedDataSizePerCommand;
            result.MaximumReceivedObjectSize = MaximumReceivedObjectSize;
            result.OpenTimeout = this.OpenTimeout;
            result.IdleTimeout = this.IdleTimeout;
            result.MaxIdleTimeout = this.MaxIdleTimeout;
            result.CancelTimeout = this.CancelTimeout;
            result.OperationTimeout = base.OperationTimeout;
            result._thumbPrint = _thumbPrint;
            result.AllowImplicitCredentialForNegotiate = AllowImplicitCredentialForNegotiate;
            result.UseCompression = UseCompression;
            result.NoMachineProfile = NoMachineProfile;
            result._proxyAuthentication = this.ProxyAuthentication;
            result._proxyCredential = this.ProxyCredential;
            result.EnableNetworkAccess = this.EnableNetworkAccess;
            result.UseDefaultWSManPort = this.UseDefaultWSManPort;
            result.DisconnectedOn = this.DisconnectedOn;
            result.ExpiresOn = this.ExpiresOn;
            result.MaxConnectionRetryCount = this.MaxConnectionRetryCount;
        /// String for http scheme.
        public const string HttpScheme = "http";
        /// String for https scheme.
        public const string HttpsScheme = "https";
        /// <param name="cryptoHelper">PSRemotingCryptoHelper instance.</param>
        /// <returns>Instance of WSManClientSessionTransportManager</returns>
        public override BaseClientSessionTransportManager CreateClientSessionTransportManager(Guid instanceId, string sessionName, PSRemotingCryptoHelper cryptoHelper)
            return new WSManClientSessionTransportManager(
                instanceId,
                cryptoHelper,
                sessionName);
        private static string ResolveShellUri(string shell)
            string resolvedShellUri = shell;
            if (string.IsNullOrEmpty(resolvedShellUri))
                resolvedShellUri = DefaultShellUri;
            if (!resolvedShellUri.Contains(WSManNativeApi.ResourceURIPrefix, StringComparison.OrdinalIgnoreCase))
                resolvedShellUri = WSManNativeApi.ResourceURIPrefix + resolvedShellUri;
            return resolvedShellUri;
        /// Converts <paramref name="rsCI"/> to a WSManConnectionInfo. If conversion succeeds extracts
        /// the property..otherwise returns default value.
        /// <param name="rsCI"></param>
        internal static T ExtractPropertyAsWsManConnectionInfo<T>(RunspaceConnectionInfo rsCI,
            string property, T defaultValue)
            if (rsCI is not WSManConnectionInfo wsCI)
            return (T)typeof(WSManConnectionInfo).GetProperty(property, typeof(T)).GetValue(wsCI, null);
        internal void SetConnectionUri(Uri newUri)
            Dbg.Assert(newUri != null, "newUri cannot be null.");
            _connectionUri = newUri;
        /// Constructs a Uri from the supplied parameters.
        /// <param name="scheme"></param>
        /// <param name="port">
        /// Making the port nullable to make sure the UseDefaultWSManPort variable is protected and updated
        /// only when Port is updated. Usages that dont update port, should use null for this parameter.
        internal void ConstructUri(string scheme, string computerName, int? port, string appName)
            // Default scheme is http
            _scheme = scheme;
            if (string.IsNullOrEmpty(_scheme))
                _scheme = DefaultScheme;
            // Valid values for scheme are "http" and "https"
            if (!(_scheme.Equals(HttpScheme, StringComparison.OrdinalIgnoreCase)
                || _scheme.Equals(HttpsScheme, StringComparison.OrdinalIgnoreCase)
                || _scheme.Equals(DefaultScheme, StringComparison.OrdinalIgnoreCase)))
                        RemotingErrorIdStrings.InvalidSchemeValue, _scheme);
            // default host is localhost
            if (string.IsNullOrEmpty(computerName) || string.Equals(computerName, ".", StringComparison.OrdinalIgnoreCase))
                _computerName = DefaultComputerName;
                _computerName = computerName.Trim();
                // According to RFC3513, an Ipv6 address in URI needs to be bracketed.
                IPAddress ipAddress = null;
                bool isIPAddress = IPAddress.TryParse(_computerName, out ipAddress);
                if (isIPAddress && ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
                    if ((_computerName.Length == 0) || (_computerName[0] != '['))
                        _computerName = @"[" + _computerName + @"]";
            PSEtwLog.LogAnalyticVerbose(PSEventId.ComputerName, PSOpcode.Method,
                PSTask.CreateRunspace, PSKeyword.Runspace | PSKeyword.UseAlwaysAnalytic,
                _computerName);
            if (port.HasValue)
                ValidatePortInRange(port.Value);
                // resolve to default ports if required
                if (port.Value == DefaultPort)
                    // this is needed so that the OriginalString on
                    // connection uri is fine
                    PortSetting = -1;
                    PortSetting = port.Value;
                    UseDefaultWSManPort = false;
            // default appname is WSMan
            _appName = appName;
                _appName = s_defaultAppName;
            // construct Uri
            UriBuilder uriBuilder = new UriBuilder(_scheme, _computerName,
                                PortSetting, _appName);
            _connectionUri = uriBuilder.Uri;
        /// Returns connection string without the scheme portion.
        /// <param name="connectionUri">
        /// The uri from which the string will be extracted
        /// <param name="isSSLSpecified">
        /// returns true if https scheme is specified
        /// returns connection string without the scheme portion.
        internal static string GetConnectionString(Uri connectionUri,
            out bool isSSLSpecified)
            isSSLSpecified =
                connectionUri.Scheme.Equals(WSManConnectionInfo.HttpsScheme);
            string result = connectionUri.OriginalString.TrimStart();
            if (isSSLSpecified)
                return result.Substring(WSManConnectionInfo.HttpsScheme.Length + 3);
                return result.Substring(WSManConnectionInfo.HttpScheme.Length + 3);
        private void ValidateSpecifiedAuthentication()
            if ((WSManAuthenticationMechanism != WSManAuthenticationMechanism.WSMAN_FLAG_DEFAULT_AUTHENTICATION)
                && (_thumbPrint != null))
                throw PSTraceSource.NewInvalidOperationException(RemotingErrorIdStrings.NewRunspaceAmbiguousAuthentication,
                      "CertificateThumbPrint", this.AuthenticationMechanism.ToString());
        private void UpdateUri(Uri uri)
            if (uri.OriginalString.LastIndexOf(':') >
                uri.AbsoluteUri.IndexOf("//", StringComparison.Ordinal))
            string appname;
            if (uri.AbsolutePath.Equals("/", StringComparison.Ordinal) &&
                appname = s_defaultAppName;
                                appname);
                _connectionUri = uri;
                _scheme = uri.Scheme;
                _appName = uri.AbsolutePath;
                PortSetting = uri.Port;
                _computerName = uri.Host;
        private string _scheme = HttpScheme;
        private string _computerName = DefaultComputerName;
        private string _appName = s_defaultAppName;
        private Uri _connectionUri = new Uri(LocalHostUriString);          // uri of this connection
        private PSCredential _credential;    // credentials to be used for this connection
        private string _shellUri = DefaultShellUri;            // shell that's specified by the user
        private string _thumbPrint;
        private AuthenticationMechanism _proxyAuthentication;
        private PSCredential _proxyCredential;
        #region constants
        /// Default disconnected server output mode is set to None.  This mode allows the
        /// server to set the buffering mode to Block for new sessions and retain its
        /// current mode during disconnect/connect operations.
        internal const OutputBufferingMode DefaultOutputBufferingMode = OutputBufferingMode.None;
        /// Default maximum connection retry count.
        internal const int DefaultMaxConnectionRetryCount = 5;
#if NOT_APPLY_PORT_DCR
        private static string DEFAULT_SCHEME = HTTP_SCHEME;
        internal static readonly string DEFAULT_SSL_SCHEME = HTTPS_SCHEME;
        private static string DEFAULT_APP_NAME = "wsman";
        /// See below for explanation.
        internal bool UseDefaultWSManPort
        private const string DefaultScheme = HttpScheme;
        private const string DefaultSslScheme = HttpsScheme;
        /// Default appname. This is empty as WSMan configuration has support
        /// for this. Look at
        /// get-item WSMan:\localhost\Client\URLPrefix.
        private static readonly string s_defaultAppName = "/wsman";
        /// Default scheme.
        /// As part of port DCR, WSMan changed the default ports
        /// from 80,443 to 5985,5986 respectively no-SSL,SSL
        /// connections. Since the standards say http,https use
        /// 80,443 as defaults..we came up with new mechanism
        /// to specify scheme as empty. For SSL, WSMan introduced
        /// a new SessionOption. In order to make scheme empty
        /// in the connection string passed to WSMan, we use
        /// this internal boolean.
        internal bool UseDefaultWSManPort { get; set; }
        /// Default port for http scheme.
        private const int DefaultPortHttp = 80;
        /// Default port for https scheme.
        private const int DefaultPortHttps = 443;
        /// This is the default port value which when specified
        /// results in the default port for the scheme to be
        /// assumed.
        private const int DefaultPort = 0;
        /// Default remote host name.
        private const string DefaultComputerName = "localhost";
        /// String that represents the local host Uri.
        private const string LocalHostUriString = "http://localhost/wsman";
        /// Default value for shell.
        private const string DefaultShellUri = WSManNativeApi.ResourceURIPrefix + RemotingConstants.DefaultShellName;
        /// Default credentials - null indicates credentials of
        /// current user.
        private const PSCredential DefaultCredential = null;
        #endregion constants
        #region Internal members
        /// Helper property that returns true when the connection has EnableNetworkAccess set
        /// and the connection is localhost (loopback), i.e., not a network connection.
        internal bool IsLocalhostAndNetworkAccess
                return (EnableNetworkAccess &&                                                              // Interactive token requested
                        (Credential == null &&                                                              // No credential provided
                         (ComputerName.Equals(DefaultComputerName, StringComparison.OrdinalIgnoreCase) ||   // Localhost computer name
                          !ComputerName.Contains('.'))));                                                    // Not FQDN computer name
        /// DisconnectedOn property applies to disconnnected runspaces.
        /// This property is publicly exposed only through Runspace class.
        internal DateTime? DisconnectedOn
        /// ExpiresOn property applies to disconnnected runspaces.
        internal DateTime? ExpiresOn
        /// Helper method to reset DisconnectedOn/ExpiresOn properties to null.
        internal void NullDisconnectedExpiresOn()
            this.DisconnectedOn = null;
            this.ExpiresOn = null;
        /// Helper method to set the DisconnectedOn/ExpiresOn properties based
        /// on current date/time and session idletimeout value.
        internal void SetDisconnectedExpiresOnToNow()
            TimeSpan idleTimeoutTime = TimeSpan.FromSeconds(this.IdleTimeout / 1000);
            this.DisconnectedOn = now;
            this.ExpiresOn = now.Add(idleTimeoutTime);
    /// Class which is used to create an Out-Of-Process Runspace/RunspacePool.
    /// This does not have a dependency on WSMan. *-Job cmdlets use Out-Of-Proc
    /// Runspaces to support background jobs.
    internal sealed class NewProcessConnectionInfo : RunspaceConnectionInfo
        private AuthenticationMechanism _authMechanism;
        /// Script to run while starting the background process.
        public ScriptBlock InitializationScript { get; set; }
        /// On a 64bit machine, specifying true for this will launch a 32 bit process
        /// for the background process.
        public bool RunAs32 { get; set; }
        /// Gets or sets an initial working directory for the powershell background process.
        /// Powershell version to execute the job in.
        public Version PSVersion { get; set; }
        internal PowerShellProcessInstance Process { get; set; }
        /// Name of the computer. Will always be "localhost" to signify local machine.
            get { return "localhost"; }
            set { throw new NotImplementedException(); }
                _authMechanism = AuthenticationMechanism.Default;
        /// Only Default is supported.
                if (value != AuthenticationMechanism.Default)
                    throw PSTraceSource.NewInvalidOperationException(RemotingErrorIdStrings.IPCSupportsOnlyDefaultAuth,
                        value.ToString(), nameof(AuthenticationMechanism.Default));
        /// Will always be empty to signify that this is not supported.
        public NewProcessConnectionInfo Copy()
            NewProcessConnectionInfo result = new NewProcessConnectionInfo(_credential);
            result.AuthenticationMechanism = this.AuthenticationMechanism;
            result.InitializationScript = this.InitializationScript;
            result.WorkingDirectory = this.WorkingDirectory;
            result.RunAs32 = this.RunAs32;
            result.PSVersion = this.PSVersion;
            result.Process = Process;
        /// <param name="cryptoHelper">PSRemotingCryptoHelper object.</param>
        /// <returns>Instance of OutOfProcessClientSessionTransportManager</returns>
            return new OutOfProcessClientSessionTransportManager(
        /// Creates a connection info instance used to create a runspace on a different
        /// process on the local machine.
        internal NewProcessConnectionInfo(PSCredential credential)
            _credential = credential;
    /// Class used to create an Out-Of-Process Runspace/RunspacePool between
    /// two local processes using a named pipe for IPC.
    /// This class does not have a dependency on WSMan and is used to implement
    /// the PowerShell attach-to-process feature.
    public sealed class NamedPipeConnectionInfo : RunspaceConnectionInfo
        private string _appDomainName = string.Empty;
        private const int _defaultOpenTimeout = 60000;      /* 60 seconds. */
        /// Process Id of process to attach to.
        public int ProcessId
        /// Optional application domain name.  If not specified then the
        /// default application domain is used.
                return _appDomainName;
                _appDomainName = value ?? string.Empty;
        /// Gets or sets the custom named pipe name to connect to. This is usually used in conjunction with pwsh -CustomPipeName.
        /// Initializes a new instance of the <see cref="NamedPipeConnectionInfo"/> class.
        public NamedPipeConnectionInfo()
            OpenTimeout = _defaultOpenTimeout;
        /// <param name="processId">Process Id to connect to.</param>
        public NamedPipeConnectionInfo(int processId)
            : this(processId, string.Empty, _defaultOpenTimeout)
        /// <param name="appDomainName">Application domain name to connect to, or default AppDomain if blank.</param>
        public NamedPipeConnectionInfo(int processId, string appDomainName)
            : this(processId, appDomainName, _defaultOpenTimeout)
        /// <param name="appDomainName">Name of application domain to connect to.  Connection is to default application domain if blank.</param>
        /// <param name="openTimeout">Open time out in Milliseconds.</param>
        public NamedPipeConnectionInfo(
            AppDomainName = appDomainName;
        /// <param name="customPipeName">Pipe name to connect to.</param>
        public NamedPipeConnectionInfo(string customPipeName)
            : this(customPipeName, _defaultOpenTimeout)
            string customPipeName,
            if (customPipeName == null)
                throw new PSArgumentNullException(nameof(customPipeName));
            CustomPipeName = customPipeName;
        /// Computer is always localhost.
                _authMechanism = Runspaces.AuthenticationMechanism.Default;
        /// Authentication.
                if (value != Runspaces.AuthenticationMechanism.Default)
        /// CertificateThumbprint.
            NamedPipeConnectionInfo newCopy = new NamedPipeConnectionInfo();
            newCopy._authMechanism = this.AuthenticationMechanism;
            newCopy._credential = this.Credential;
            newCopy.ProcessId = this.ProcessId;
            newCopy._appDomainName = _appDomainName;
            newCopy.OpenTimeout = this.OpenTimeout;
            newCopy.CustomPipeName = this.CustomPipeName;
            return newCopy;
        /// <returns>Instance of NamedPipeClientSessionTransportManager</returns>
            return new NamedPipeClientSessionTransportManager(
    /// Class used to create a connection through an SSH.exe client to a remote host machine.
    /// Connection information includes SSH target (user name and host machine) along with
    /// client key used for key based user authorization.
    public sealed class SSHConnectionInfo : RunspaceConnectionInfo
        /// Default value for subsystem.
        private const string DefaultSubsystem = "powershell";
        /// Default value is infinite timeout.
        private const int DefaultConnectingTimeoutTime = Timeout.Infinite;
        /// Key File Path.
        public string KeyFilePath
        /// Port for connection.
        /// Subsystem to use.
        public string Subsystem
        /// Gets or sets a time in milliseconds after which a connection attempt is terminated.
        /// Default value (-1) never times out and a connection attempt waits indefinitely.
        public int ConnectingTimeout
        /// The SSH options to pass to OpenSSH.
        /// Gets or sets the SSH options to pass to OpenSSH.
        private Hashtable Options
        /// Initializes a new instance of the <see cref="SSHConnectionInfo" /> class.
        private SSHConnectionInfo()
        /// <param name="userName">User Name.</param>
        /// <param name="computerName">Computer Name.</param>
        /// <param name="keyFilePath">Key File Path.</param>
        public SSHConnectionInfo(
            string keyFilePath)
                throw new PSArgumentNullException(nameof(computerName));
            UserName = userName;
            KeyFilePath = keyFilePath;
            Port = 0;
            Subsystem = DefaultSubsystem;
            ConnectingTimeout = DefaultConnectingTimeoutTime;
        /// <param name="port">Port number for connection (default 22).</param>
            string keyFilePath,
            int port) : this(userName, computerName, keyFilePath)
            ValidatePortInRange(port);
        /// <param name="subsystem">Subsystem to use (default 'powershell').</param>
            string subsystem) : this(userName, computerName, keyFilePath, port)
            Subsystem = string.IsNullOrEmpty(subsystem) ? DefaultSubsystem : subsystem;
        /// Initializes a new instance of SSHConnectionInfo.
        /// <param name="userName">Name of user.</param>
        /// <param name="computerName">Name of computer.</param>
        /// <param name="keyFilePath">Path of key file.</param>
        /// <param name="connectingTimeout">Timeout time for terminating connection attempt.</param>
            string subsystem,
            int connectingTimeout) : this(userName, computerName, keyFilePath, port, subsystem)
            ConnectingTimeout = connectingTimeout;
        /// <param name="options">Options for the SSH connection.</param>
            int connectingTimeout,
            Hashtable options) : this(userName, computerName, keyFilePath, port, subsystem, connectingTimeout)
            get { return AuthenticationMechanism.Default; }
            SSHConnectionInfo newCopy = new SSHConnectionInfo();
            newCopy.ComputerName = ComputerName;
            newCopy.UserName = UserName;
            newCopy.KeyFilePath = KeyFilePath;
            newCopy.Port = Port;
            newCopy.Subsystem = Subsystem;
            newCopy.ConnectingTimeout = ConnectingTimeout;
            newCopy.Options = Options;
            return new SSHClientSessionTransportManager(
        /// StartSSHProcess.
        internal int StartSSHProcess(
            out StreamWriter stdInWriterVar,
            out StreamReader stdOutReaderVar,
            out StreamReader stdErrReaderVar)
            string filePath = string.Empty;
            const string sshCommand = "ssh";
            const string sshCommand = "ssh.exe";
            var context = Runspaces.LocalPipeline.GetExecutionContextFromTLS();
                var cmdInfo = CommandDiscovery.LookupCommandInfo(
                    sshCommand,
                if (cmdInfo is ApplicationInfo appInfo)
                    filePath = appInfo.Path;
                // A Runspace may not be present in the TLS in SDK hosted apps
                // or if running in another thread without a Runspace. While
                // 'ProcessStartInfo' can lookup the full path in PATH, it searches
                // the process' working directory first. 'LookupCommandInfo' does
                // not search the process' working directory and we want to keep that
                // behavior. We also get the parent dir of the full path to set as the
                // new WorkingDirectory. So, we do a manual lookup here only in PATH.
                string[] entries = Environment.GetEnvironmentVariable("PATH")?.Split(
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
                foreach (var path in entries)
                    if (!Path.IsPathFullyQualified(path))
                    var sshCommandPath = Path.Combine(path, sshCommand);
                    if (File.Exists(sshCommandPath))
                        filePath = sshCommandPath;
            // Create a local ssh process (client) that connects to a remote sshd process (server) using a 'powershell' subsystem.
            // Local ssh invoked as:
            //   windows:
            //     ssh.exe [-i identity_file] [-l login_name] [-p port] [-o option] -s <destination> <command>
            //   linux|macos:
            //     ssh [-i identity_file] [-l login_name] [-p port] [-o option] -s <destination> <command>
            // where <command> is interpreted as the subsystem due to the -s flag.
            // Remote sshd configured for PowerShell Remoting Protocol (PSRP) over Secure Shell Protocol (SSH)
            // by adding one of the following Subsystem directives to sshd_config on the remote machine:
            //     Subsystem powershell C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe -SSHServerMode -NoLogo -NoProfile
            //     Subsystem powershell C:\Program Files\PowerShell\6\pwsh.exe -SSHServerMode -NoLogo -NoProfile
            //     Subsystem powershell /usr/local/bin/pwsh -SSHServerMode -NoLogo -NoProfile
            // codeql[cs/microsoft/command-line-injection-shell-execution] - This is expected Poweshell behavior where user inputted paths are supported for the context of this method. The user assumes trust for the file path specified, so any file executed in the runspace would be in the user's local system/process or a system they have access to in which case restricted remoting security guidelines should be used.
            ProcessStartInfo startInfo = new(filePath);
            // pass "-i identity_file" command line argument to ssh if KeyFilePath is set
            // if KeyFilePath is not set, then ssh will use IdentityFile / IdentityAgent from ssh_config if defined else none by default
            if (!string.IsNullOrEmpty(this.KeyFilePath))
                if (!File.Exists(this.KeyFilePath))
                    throw new FileNotFoundException(
                        StringUtil.Format(RemotingErrorIdStrings.KeyFileNotFound, this.KeyFilePath));
                startInfo.ArgumentList.Add(string.Create(CultureInfo.InvariantCulture, $@"-i ""{this.KeyFilePath}"""));
            // pass "-l login_name" command line argument to ssh if UserName is set
            // if UserName is not set, then ssh will use User from ssh_config if defined else the environment user by default
            if (!string.IsNullOrEmpty(this.UserName))
                var parts = this.UserName.Split('\\');
                if (parts.Length == 2)
                    // convert DOMAIN\user to user@DOMAIN
                    var domainName = parts[0];
                    var userName = parts[1];
                    startInfo.ArgumentList.Add(string.Create(CultureInfo.InvariantCulture, $@"-l {userName}@{domainName}"));
                    startInfo.ArgumentList.Add(string.Create(CultureInfo.InvariantCulture, $@"-l {this.UserName}"));
            // pass "-p port" command line argument to ssh if Port is set
            // if Port is not set, then ssh will use Port from ssh_config if defined else 22 by default
            if (this.Port != 0)
                startInfo.ArgumentList.Add(string.Create(CultureInfo.InvariantCulture, $@"-p {this.Port}"));
            // pass "-o option=value" command line argument to ssh if options are provided
            if (this.Options != null)
                foreach (DictionaryEntry pair in this.Options)
                    startInfo.ArgumentList.Add(string.Create(CultureInfo.InvariantCulture, $@"-o {pair.Key}={pair.Value}"));
            // pass "-s destination command" command line arguments to ssh where command is the subsystem to invoke on the destination
            // note that ssh expects IPv6 addresses to not be enclosed in square brackets so trim them if present
            startInfo.ArgumentList.Add(string.Create(CultureInfo.InvariantCulture, $@"-s {this.ComputerName.TrimStart('[').TrimEnd(']')} {this.Subsystem}"));
            startInfo.WorkingDirectory = Path.GetDirectoryName(filePath);
            startInfo.CreateNoWindow = true;
            return StartSSHProcessImpl(startInfo, out stdInWriterVar, out stdOutReaderVar, out stdErrReaderVar);
        /// Terminates the SSH process by process Id.
        /// <param name="pid">Process id.</param>
        internal void KillSSHProcess(int pid)
            KillSSHProcessImpl(pid);
        #region SSH Process Creation
        /// Create a process through managed APIs and returns StdIn, StdOut, StdError reader/writers.
        /// This works for Linux platforms and creates the SSH process in its own session which means
        /// Ctrl+C signals will not propagate from parent (PowerShell) process to SSH process so that
        /// PSRP handles them correctly.
        private static int StartSSHProcessImpl(
            System.Diagnostics.ProcessStartInfo startInfo,
            StreamWriter stdInWriter = null;
            StreamReader stdOutReader = null;
            StreamReader stdErrReader = null;
            int pid = StartSSHProcess(
                startInfo,
                ref stdInWriter,
                ref stdOutReader,
                ref stdErrReader);
            stdInWriterVar = stdInWriter;
            stdOutReaderVar = stdOutReader;
            stdErrReaderVar = stdErrReader;
            return pid;
        private static void KillSSHProcessImpl(int pid)
            // killing a zombie might or might not return ESRCH, so we ignore kill's return value
            Platform.NonWindowsKillProcess(pid);
            // block while waiting for process to die
            // shouldn't take long after SIGKILL
            Platform.NonWindowsWaitPid(pid, false);
        #region UNIX Create Process
        // This code is based on GitHub DotNet CoreFx
        // It is specific to launching the SSH process for use in
        // SSH based remoting, and is not intended to be general
        // process creation code.
        private const int StreamBufferSize = 4096;
        private const int SUPPRESS_PROCESS_SIGINT = 0x00000001;
        internal static int StartSSHProcess(
            ProcessStartInfo startInfo,
            ref StreamWriter standardInput,
            ref StreamReader standardOutput,
            ref StreamReader standardError)
            string filename = startInfo.FileName;
            string[] argv = ParseArgv(startInfo);
            string[] envp = CopyEnvVariables(startInfo);
            string cwd = !string.IsNullOrWhiteSpace(startInfo.WorkingDirectory) ? startInfo.WorkingDirectory : null;
            // Invoke the shim fork/execve routine.  It will create pipes for all requested
            // redirects, fork a child process, map the pipe ends onto the appropriate stdin/stdout/stderr
            // descriptors, and execve to execute the requested process.  The shim implementation
            // is used to fork/execve as executing managed code in a forked process is not safe (only
            // the calling thread will transfer, thread IDs aren't stable across the fork, etc.)
            int childPid, stdinFd, stdoutFd, stderrFd;
            CreateProcess(
                filename, argv, envp, cwd,
                startInfo.RedirectStandardInput, startInfo.RedirectStandardOutput, startInfo.RedirectStandardError,
                SUPPRESS_PROCESS_SIGINT,    // Create SSH process to ignore SIGINT signals
                out childPid,
                out stdinFd, out stdoutFd, out stderrFd);
            Debug.Assert(childPid >= 0, "Invalid process id");
            // Configure the parent's ends of the redirection streams.
            // We use UTF8 encoding without BOM by-default(instead of Console encoding as on Windows)
            // as there is no good way to get this information from the native layer
            // and we do not want to take dependency on Console contract.
                Debug.Assert(stdinFd >= 0, "Invalid Fd");
                standardInput = new StreamWriter(
                    OpenStream(stdinFd, FileAccess.Write),
                    StreamBufferSize)
                { AutoFlush = true };
            if (startInfo.RedirectStandardOutput)
                Debug.Assert(stdoutFd >= 0, "Invalid Fd");
                standardOutput = new StreamReader(
                    OpenStream(stdoutFd, FileAccess.Read),
                    startInfo.StandardOutputEncoding ?? Encoding.Default,
                    detectEncodingFromByteOrderMarks: true,
                    StreamBufferSize);
            if (startInfo.RedirectStandardError)
                Debug.Assert(stderrFd >= 0, "Invalid Fd");
                standardError = new StreamReader(
                    OpenStream(stderrFd, FileAccess.Read),
                    startInfo.StandardErrorEncoding ?? Encoding.Default,
            return childPid;
        /// <summary>Opens a stream around the specified file descriptor and with the specified access.</summary>
        /// <param name="fd">The file descriptor.</param>
        /// <param name="access">The access mode.</param>
        /// <returns>The opened stream.</returns>
        private static FileStream OpenStream(int fd, FileAccess access)
            Debug.Assert(fd >= 0, "Invalid Fd");
            return new FileStream(
                new SafeFileHandle((IntPtr)fd, ownsHandle: true),
                access, StreamBufferSize, isAsync: false);
        /// <summary>Copies environment variables from ProcessStartInfo </summary>
        /// <param name="psi">ProcessStartInfo.</param>
        /// <returns>String array of environment key/value pairs.</returns>
        private static string[] CopyEnvVariables(ProcessStartInfo psi)
            var envp = new string[psi.Environment.Count];
            foreach (var pair in psi.Environment)
                envp[index++] = pair.Key + "=" + pair.Value;
            return envp;
        /// <summary>Converts the filename and arguments information from a ProcessStartInfo into an argv array.</summary>
        /// <param name="psi">The ProcessStartInfo.</param>
        /// <returns>The argv array.</returns>
        private static string[] ParseArgv(ProcessStartInfo psi)
            var argvList = new List<string>();
            argvList.Add(psi.FileName);
            var argsToParse = String.Join(' ', psi.ArgumentList).Trim();
            var argsLength = argsToParse.Length;
            for (int i = 0; i < argsLength; )
                var iStart = i;
                switch (argsToParse[i])
                        // Special case for arguments within quotes
                        // Just return argument value within the quotes
                        while ((++i < argsLength) && argsToParse[i] != '"') { }
                        if (iStart < argsLength - 1)
                            iStart++;
                        // Common case for parsing arguments with space character delimiter
                        while ((++i < argsLength) && argsToParse[i] != ' ') { }
                argvList.Add(argsToParse.Substring(iStart, (i - iStart)));
                while ((++i < argsLength) && argsToParse[i] == ' ') { }
            return argvList.ToArray();
        internal static unsafe void CreateProcess(
            string filename, string[] argv, string[] envp, string cwd,
            bool redirectStdin, bool redirectStdout, bool redirectStderr, int creationFlags,
            out int lpChildPid, out int stdinFd, out int stdoutFd, out int stderrFd)
            byte** argvPtr = null, envpPtr = null;
                AllocNullTerminatedArray(argv, ref argvPtr);
                AllocNullTerminatedArray(envp, ref envpPtr);
                int result = ForkAndExecProcess(
                    filename, argvPtr, envpPtr, cwd,
                    redirectStdin ? 1 : 0, redirectStdout ? 1 : 0, redirectStderr ? 1 : 0, creationFlags,
                    out lpChildPid, out stdinFd, out stdoutFd, out stderrFd);
                    // Normally we'd simply make this method return the result of the native
                    // call and allow the caller to use GetLastWin32Error.  However, we need
                    // to free the native arrays after calling the function, and doing so
                    // stomps on the runtime's captured last error.  So we need to access the
                    // error here, and without SetLastWin32Error available, we can't propagate
                    // the error to the caller via the normal GetLastWin32Error mechanism.  We could
                    // return 0 on success or the GetLastWin32Error value on failure, but that's
                    // technically ambiguous, in the case of a failure with a 0 errno.  Simplest
                    // solution then is just to throw here the same exception the Process caller
                    // would have.  This can be revisited if we ever have another call site.
                    throw new Win32Exception();
                FreeArray(envpPtr, envp.Length);
                FreeArray(argvPtr, argv.Length);
        private static unsafe void AllocNullTerminatedArray(string[] arr, ref byte** arrPtr)
            int arrLength = arr.Length + 1; // +1 is for null termination
            // Allocate the unmanaged array to hold each string pointer.
            // It needs to have an extra element to null terminate the array.
            arrPtr = (byte**)Marshal.AllocHGlobal(sizeof(IntPtr) * arrLength);
            Debug.Assert(arrPtr != null, "Invalid array ptr");
            // Zero the memory so that if any of the individual string allocations fails,
            // we can loop through the array to free any that succeeded.
            // The last element will remain null.
            for (int i = 0; i < arrLength; i++)
                arrPtr[i] = null;
            // Now copy each string to unmanaged memory referenced from the array.
            // We need the data to be an unmanaged, null-terminated array of UTF8-encoded bytes.
            for (int i = 0; i < arr.Length; i++)
                byte[] byteArr = System.Text.Encoding.UTF8.GetBytes(arr[i]);
                arrPtr[i] = (byte*)Marshal.AllocHGlobal(byteArr.Length + 1); // +1 for null termination
                Debug.Assert(arrPtr[i] != null, "Invalid array ptr");
                Marshal.Copy(byteArr, 0, (IntPtr)arrPtr[i], byteArr.Length); // copy over the data from the managed byte array
                arrPtr[i][byteArr.Length] = (byte)'\0'; // null terminate
        private static unsafe void FreeArray(byte** arr, int length)
            if (arr != null)
                // Free each element of the array
                    if (arr[i] != null)
                        Marshal.FreeHGlobal((IntPtr)arr[i]);
                        arr[i] = null;
                // And then the array itself
                Marshal.FreeHGlobal((IntPtr)arr);
        [DllImport("libpsl-native", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern unsafe int ForkAndExecProcess(
            string filename, byte** argv, byte** envp, string cwd,
            int redirectStdin, int redirectStdout, int redirectStderr, int creationFlags,
            out int lpChildPid, out int stdinFd, out int stdoutFd, out int stderrFd);
        /// Create a process through native Win32 APIs and return StdIn, StdOut, StdError reader/writers
        /// This needs to be done via Win32 APIs because managed code creates anonymous synchronous pipes
        /// for redirected StdIn/Out and SSH (and PSRP) require asynchronous (overlapped) pipes, which must
        /// be through named pipes.  Managed code for named pipes is unreliable and so this is done via
        /// P-Invoking native APIs.
            Process sshProcess = null;
            // These std pipe handles are bound to managed Reader/Writer objects and returned to the transport
            // manager object, which uses them for PSRP communication.  The lifetime of these handles are then
            // tied to the reader/writer objects which the transport is responsible for disposing (see
            // SSHClientSessionTransportManger and the CloseConnection() method.
            SafePipeHandle stdInPipeServer = null;
            SafePipeHandle stdOutPipeServer = null;
            SafePipeHandle stdErrPipeServer = null;
                sshProcess = CreateProcessWithRedirectedStd(
                    out stdInPipeServer,
                    out stdOutPipeServer,
                    out stdErrPipeServer);
            catch (InvalidOperationException e) { ex = e; }
            catch (FileNotFoundException e) { ex = e; }
            catch (Win32Exception e) { ex = e; }
            if ((ex != null) ||
                (sshProcess == null) ||
                (sshProcess.HasExited))
                    StringUtil.Format(RemotingErrorIdStrings.CannotStartSSHClient, (ex != null) ? ex.Message : string.Empty),
                    ex);
            // Create the std in writer/readers needed for communication with ssh.exe.
            stdInWriterVar = null;
            stdOutReaderVar = null;
            stdErrReaderVar = null;
                stdInWriterVar = new StreamWriter(new NamedPipeServerStream(PipeDirection.Out, true, true, stdInPipeServer));
                stdOutReaderVar = new StreamReader(new NamedPipeServerStream(PipeDirection.In, true, true, stdOutPipeServer));
                stdErrReaderVar = new StreamReader(new NamedPipeServerStream(PipeDirection.In, true, true, stdErrPipeServer));
                if (stdInWriterVar != null) { stdInWriterVar.Dispose(); } else { stdInPipeServer.Dispose(); }
                if (stdOutReaderVar != null) { stdOutReaderVar.Dispose(); } else { stdOutPipeServer.Dispose(); }
                if (stdErrReaderVar != null) { stdErrReaderVar.Dispose(); } else { stdErrPipeServer.Dispose(); }
            return sshProcess.Id;
            using (var sshProcess = Process.GetProcessById(pid))
                if ((sshProcess != null) && (sshProcess.Handle != IntPtr.Zero) && !sshProcess.HasExited)
                    sshProcess.Kill();
        // Process creation flags
        private const int CREATE_NEW_PROCESS_GROUP = 0x00000200;
        private const int CREATE_SUSPENDED = 0x00000004;
        /// CreateProcessWithRedirectedStd.
        private static Process CreateProcessWithRedirectedStd(
            out SafePipeHandle stdInPipeServer,
            out SafePipeHandle stdOutPipeServer,
            out SafePipeHandle stdErrPipeServer)
            // Create named (async) pipes for reading/writing to std.
            stdInPipeServer = null;
            stdOutPipeServer = null;
            stdErrPipeServer = null;
            SafeFileHandle stdInPipeClient = null;
            SafeFileHandle stdOutPipeClient = null;
            SafeFileHandle stdErrPipeClient = null;
            string randomName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
                // Get default pipe security (Admin and current user access)
                var securityDesc = RemoteSessionNamedPipeServer.GetServerPipeSecurity();
                var stdInPipeName = @"\\.\pipe\StdIn" + randomName;
                stdInPipeServer = CreateNamedPipe(stdInPipeName, securityDesc);
                stdInPipeClient = GetNamedPipeHandle(stdInPipeName);
                var stdOutPipeName = @"\\.\pipe\StdOut" + randomName;
                stdOutPipeServer = CreateNamedPipe(stdOutPipeName, securityDesc);
                stdOutPipeClient = GetNamedPipeHandle(stdOutPipeName);
                var stdErrPipeName = @"\\.\pipe\StdErr" + randomName;
                stdErrPipeServer = CreateNamedPipe(stdErrPipeName, securityDesc);
                stdErrPipeClient = GetNamedPipeHandle(stdErrPipeName);
                stdInPipeServer?.Dispose();
                stdInPipeClient?.Dispose();
                stdOutPipeServer?.Dispose();
                stdOutPipeClient?.Dispose();
                stdErrPipeServer?.Dispose();
                stdErrPipeClient?.Dispose();
            // Create process
            PlatformInvokes.STARTUPINFO lpStartupInfo = new PlatformInvokes.STARTUPINFO();
            PlatformInvokes.PROCESS_INFORMATION lpProcessInformation = new PlatformInvokes.PROCESS_INFORMATION();
                // Create process start command line with filename and argument list.
                var cmdLine = string.Format(
                    @"""{0}"" {1}",
                    startInfo.FileName,
                    string.Join(' ', startInfo.ArgumentList));
                lpStartupInfo.hStdInput = stdInPipeClient;
                lpStartupInfo.hStdOutput = stdOutPipeClient;
                lpStartupInfo.hStdError = stdErrPipeClient;
                // Create the new process in its own group, so that Ctrl+C is not sent to ssh.exe.  We want to handle this
                // control signal internally so that it can be passed via PSRP to the remote session.
                creationFlags |= CREATE_NEW_PROCESS_GROUP;
                creationFlags |= CREATE_SUSPENDED;
                PlatformInvokes.SECURITY_ATTRIBUTES lpProcessAttributes = new PlatformInvokes.SECURITY_ATTRIBUTES();
                PlatformInvokes.SECURITY_ATTRIBUTES lpThreadAttributes = new PlatformInvokes.SECURITY_ATTRIBUTES();
                bool success = PlatformInvokes.CreateProcess(
                    cmdLine,
                    lpProcessAttributes,
                    lpThreadAttributes,
                    creationFlags,
                    startInfo.WorkingDirectory,
                    lpStartupInfo,
                    lpProcessInformation);
                // At this point, we should have a suspended process.  Get the .Net Process object, resume the process, and return.
                Process result = Process.GetProcessById(lpProcessInformation.dwProcessId);
                uint returnValue = PlatformInvokes.ResumeThread(lpProcessInformation.hThread);
                if (returnValue == PlatformInvokes.RESUME_THREAD_FAILED)
                lpProcessInformation.Dispose();
        private static SafeFileHandle GetNamedPipeHandle(string pipeName)
            SafeFileHandle sf = File.OpenHandle(pipeName, FileMode.Open, FileAccess.ReadWrite, FileShare.Inheritable, FileOptions.Asynchronous);
        private static SafePipeHandle CreateNamedPipe(
            string pipeName,
                securityAttributes = NamedPipeNative.GetSecurityAttributes(securityDescHandle.Value, true);
            // Create async named pipe.
                NamedPipeNative.PIPE_TYPE_MESSAGE | NamedPipeNative.PIPE_READMODE_MESSAGE,
                32768,
                throw new Win32Exception(lastError);
            return pipeHandle;
    /// The class that contains connection information for a remote session between a local host
    /// and VM. The local host can be a VM in nested scenario.
    public sealed class VMConnectionInfo : RunspaceConnectionInfo
        private const int _defaultOpenTimeout = 20000; /* 20 seconds. */
        /// GUID of the target VM.
        public Guid VMGuid { get; set; }
        /// Configuration name of the VM session.
        /// Will always be null to signify that this is not supported.
        /// Name of the target VM.
        public override string ComputerName { get; set; }
            VMConnectionInfo result = new VMConnectionInfo(Credential, VMGuid, ComputerName, ConfigurationName);
        /// <returns>Instance of VMHyperVSocketClientSessionTransportManager.</returns>
            return new VMHyperVSocketClientSessionTransportManager(
                VMGuid,
                ConfigurationName);
        /// Creates a connection info instance used to create a runspace on target VM.
        internal VMConnectionInfo(
            Guid vmGuid,
            string vmName,
            VMGuid = vmGuid;
            ComputerName = vmName;
            AuthenticationMechanism = AuthenticationMechanism.Default;
    /// The class that contains connection information for a remote session between a local
    /// container host and container.
    /// For Windows Server container, the transport is based on named pipe for now.
    /// For Hyper-V container, the transport is based on Hyper-V socket.
    public sealed class ContainerConnectionInfo : RunspaceConnectionInfo
        /// ContainerProcess class instance.
        internal ContainerProcess ContainerProc { get; set; }
        /// Name of the target container.
            get { return ContainerProc.ContainerId; }
            set { throw new PSNotSupportedException(); }
            ContainerConnectionInfo newCopy = new ContainerConnectionInfo(ContainerProc);
        /// <returns>Instance of ContainerHyperVSocketClientSessionTransportManager</returns>
            if (ContainerProc.RuntimeId != Guid.Empty)
                return new ContainerHyperVSocketClientSessionTransportManager(
                    ContainerProc.RuntimeId);
                return new ContainerNamedPipeClientSessionTransportManager(
        /// Creates a connection info instance used to create a runspace on target container.
        internal ContainerConnectionInfo(
            ContainerProcess containerProc)
            ContainerProc = containerProc;
            Credential = null;
        /// Create ContainerConnectionInfo object based on container id.
        public static ContainerConnectionInfo CreateContainerConnectionInfo(
            string containerId,
            bool runAsAdmin,
            ContainerProcess containerProc = new ContainerProcess(containerId, null, 0, runAsAdmin, configurationName);
            return new ContainerConnectionInfo(containerProc);
        /// Create process inside container.
        public void CreateContainerProcess()
            ContainerProc.CreateContainerProcess();
        /// Terminate process inside container.
        public bool TerminateContainerProcess()
            return ContainerProc.TerminateContainerProcess();
    /// Class used to create/terminate process inside container, which can be either
    /// Windows Server Container or Hyper-V container.
    /// - Windows Server Container does not require Hyper-V.
    /// - Hyper-V container requires Hyper-V and utility VM, which is different from normal VM.
    internal class ContainerProcess
        private const uint NoError = 0;
        private const uint InvalidContainerId = 1;
        private const uint ContainersFeatureNotEnabled = 2;
        private const uint OtherError = 9999;
        private const uint FileNotFoundHResult = 0x80070002;
        // The list of executable to try in order
        private static readonly string[] Executables = new string[] { "pwsh.exe", "powershell.exe" };
        /// Gets or Sets, for Hyper-V container, the Guid of utility VM hosting Hyper-V container.
        /// For Windows Server Container, it is empty.
        public Guid RuntimeId { get; set; }
        /// Gets or sets the OB root of the container.
        public string ContainerObRoot { get; set; }
        /// Gets or sets the ID of the container.
        public string ContainerId { get; set; }
        /// Gets or sets the process ID of the process created in container.
        internal int ProcessId { get; set; }
        /// Gets or sets whether the process in container should be launched as high privileged account
        /// (RunAsAdmin being true) or low privileged account (RunAsAdmin being false).
        internal bool RunAsAdmin { get; set; } = false;
        /// Gets or sets the configuration name of the container session.
        /// Gets or sets whether the process in container has terminated.
        internal bool ProcessTerminated { get; set; } = false;
        /// Gets or sets the error code.
        internal uint ErrorCode { get; set; } = 0;
        /// Gets or sets the error message for other errors.
        internal string ErrorMessage { get; set; } = string.Empty;
        /// Gets or sets the PowerShell executable being used to host the runspace.
        internal string Executable { get; set; } = string.Empty;
        #region Native HCS (i.e., host compute service) methods
        internal struct HCS_PROCESS_INFORMATION
            /// The process id.
            public uint ProcessId;
            /// Reserved.
            public uint Reserved;
            /// If created, standard input handle of the process.
            public IntPtr StdInput;
            /// If created, standard output handle of the process.
            public IntPtr StdOutput;
            /// If created, standard error handle of the process.
            public IntPtr StdError;
        [DllImport(PinvokeDllNames.CreateProcessInComputeSystemDllName, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern uint HcsOpenComputeSystem(
            string id,
            ref IntPtr computeSystem,
            ref string result);
        internal static extern uint HcsGetComputeSystemProperties(
            IntPtr computeSystem,
            string propertyQuery,
            ref string properties,
        internal static extern uint HcsCreateProcess(
            string processParameters,
            ref HCS_PROCESS_INFORMATION processInformation,
            ref IntPtr process,
        internal static extern uint HcsOpenProcess(
        internal static extern uint HcsTerminateProcess(
            IntPtr process,
        /// Creates an instance used for PowerShell Direct for container.
        public ContainerProcess(string containerId, string containerObRoot, int processId, bool runAsAdmin, string configurationName)
            this.ContainerId = containerId;
            this.ContainerObRoot = containerObRoot;
            this.RunAsAdmin = runAsAdmin;
            this.ConfigurationName = configurationName;
            Dbg.Assert(!string.IsNullOrEmpty(containerId), "containerId input cannot be empty.");
            GetContainerProperties();
            RunOnMTAThread(CreateContainerProcessInternal);
            // Report error. More error reporting will come later.
            switch (ErrorCode)
                case NoError:
                case InvalidContainerId:
                    throw new PSInvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.InvalidContainerId,
                                                                            ContainerId));
                case ContainersFeatureNotEnabled:
                    throw new PSInvalidOperationException(RemotingErrorIdStrings.ContainersFeatureNotEnabled);
                // other errors caught with exception
                case OtherError:
                    throw new PSInvalidOperationException(ErrorMessage);
                // other errors caught without exception
                    throw new PSInvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.CannotCreateProcessInContainer,
                                                                            ContainerId,
                                                                            Executable,
                                                                            ErrorCode));
            RunOnMTAThread(TerminateContainerProcessInternal);
            return ProcessTerminated;
        /// Get object root based on given container id.
        public void GetContainerProperties()
            RunOnMTAThread(GetContainerPropertiesInternal);
            // Report error.
        /// Dynamically load the Host Compute interop assemblies and return useful types.
        /// <param name="computeSystemPropertiesType">The HCS.Compute.System.Properties type.</param>
        /// <param name="hostComputeInteropType">The Microsoft.HostCompute.Interop.HostComputeInterop type.</param>
        private static void GetHostComputeInteropTypes(out Type computeSystemPropertiesType, out Type hostComputeInteropType)
            Assembly schemaAssembly = Assembly.Load(new AssemblyName("Microsoft.HyperV.Schema, Version=10.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            // The type name was changed in newer version of Windows so we check for new one first,
            // then fallback to previous type name to support older versions of Windows
            computeSystemPropertiesType = schemaAssembly.GetType("HCS.Compute.System.Properties");
            if (computeSystemPropertiesType == null)
                computeSystemPropertiesType = schemaAssembly.GetType("Microsoft.HyperV.Schema.Compute.System.Properties");
                    throw new PSInvalidOperationException(RemotingErrorIdStrings.CannotGetHostInteropTypes);
            Assembly hostComputeInteropAssembly = Assembly.Load(new AssemblyName("Microsoft.HostCompute.Interop, Version=10.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            hostComputeInteropType = hostComputeInteropAssembly.GetType("Microsoft.HostCompute.Interop.HostComputeInterop");
            if (hostComputeInteropType == null)
        private void CreateContainerProcessInternal()
            uint result;
            string cmd;
            int processId = 0;
            uint error = 0;
            // Check whether the given container id exists.
                IntPtr ComputeSystem = IntPtr.Zero;
                string resultString = string.Empty;
                result = HcsOpenComputeSystem(ContainerId, ref ComputeSystem, ref resultString);
                    processId = 0;
                    error = InvalidContainerId;
                    // Hyper-V container (i.e., RuntimeId is not empty) uses Hyper-V socket transport.
                    // Windows Server container (i.e., RuntimeId is empty) uses named pipe transport for now.
                    // This code executes `pwsh.exe` as it exists in the container which currently is
                    // expected to be PowerShell 6+ as it's inbox in the container.
                    // If `pwsh.exe` does not exist, fall back to `powershell.exe` which is Windows PowerShell.
                    foreach (string executableToTry in Executables)
                        cmd = GetContainerProcessCommand(executableToTry);
                        HCS_PROCESS_INFORMATION ProcessInformation = new HCS_PROCESS_INFORMATION();
                        IntPtr Process = IntPtr.Zero;
                        // Create PowerShell process inside the container.
                        result = HcsCreateProcess(ComputeSystem, cmd, ref ProcessInformation, ref Process, ref resultString);
                            processId = Convert.ToInt32(ProcessInformation.ProcessId);
                            // Reset error to 0 in case this is not the first iteration of the loop.
                            error = 0;
                            // the process was started, exit the loop.
                        else if (result == FileNotFoundHResult)
                            // "The system cannot find the file specified", try the next one
                            // or exit the loop of none are left to try.
                            // Set the process and error information in case we exit the loop.
                            error = result;
                            // the executable was found but did not work
                            // exit the loop with the error state.
                if (e is FileNotFoundException || e is FileLoadException)
                    // The ComputeSystemExists call depends on the existence of microsoft.hostcompute.interop.dll,
                    // which requires Containers feature to be enabled. In case Containers feature is
                    // not enabled, we need to output a corresponding error message to inform user.
                    ProcessId = 0;
                    ErrorCode = ContainersFeatureNotEnabled;
                    ErrorCode = OtherError;
                    ErrorMessage = GetErrorMessageFromException(e);
            ErrorCode = error;
        /// Get Command to launch container process based on instance properties.
        /// <param name="executable">The name of the executable to use in the command.</param>
        /// <returns>The command to launch the container process.</returns>
        private string GetContainerProcessCommand(string executable)
            Executable = executable;
                        @"{{""CommandLine"": ""{0} {1} -NoLogo {2}"",""RestrictedToken"": {3}}}",
                        (RuntimeId != Guid.Empty) ? "-SocketServerMode -NoProfile" : "-NamedPipeServerMode",
                        string.IsNullOrEmpty(ConfigurationName) ? string.Empty : string.Concat("-Config ", ConfigurationName),
                        RunAsAdmin ? "false" : "true");
        /// Terminate the process inside container.
        private void TerminateContainerProcessInternal()
            IntPtr process = IntPtr.Zero;
            ProcessTerminated = false;
            if (HcsOpenComputeSystem(ContainerId, ref ComputeSystem, ref resultString) == 0)
                if (HcsOpenProcess(ComputeSystem, ProcessId, ref process, ref resultString) == 0)
                    if (HcsTerminateProcess(process, ref resultString) == 0)
                        ProcessTerminated = true;
        private void GetContainerPropertiesInternal()
                    Type computeSystemPropertiesType;
                    Type hostComputeInteropType;
                    GetHostComputeInteropTypes(out computeSystemPropertiesType, out hostComputeInteropType);
                    MethodInfo getComputeSystemPropertiesInfo = hostComputeInteropType.GetMethod("HcsGetComputeSystemProperties");
                    var computeSystemPropertiesHandle = getComputeSystemPropertiesInfo.Invoke(null, new object[] { ComputeSystem });
                    // Since Hyper-V changed this from a property to a field, we can optimize for newest Windows to see if it's a field,
                    // otherwise we fall back to old code to be compatible with older versions of Windows
                    var fieldInfo = computeSystemPropertiesType.GetField("RuntimeId");
                        RuntimeId = (Guid)fieldInfo.GetValue(computeSystemPropertiesHandle);
                        var propertyInfo = computeSystemPropertiesType.GetProperty("RuntimeId");
                        RuntimeId = (Guid)propertyInfo.GetValue(computeSystemPropertiesHandle);
                    // Get container object root for Windows Server container.
                    if (RuntimeId == Guid.Empty)
                        var obRootFieldInfo = computeSystemPropertiesType.GetField("ObRoot");
                        if (obRootFieldInfo != null)
                            ContainerObRoot = obRootFieldInfo.GetValue(computeSystemPropertiesHandle) as string;
                            var obRootPropertyInfo = computeSystemPropertiesType.GetProperty("ObRoot");
                            if (obRootPropertyInfo != null)
                                ContainerObRoot = obRootPropertyInfo.GetValue(computeSystemPropertiesHandle) as string;
                        if (ContainerObRoot == null)
                if (e.InnerException != null &&
                    StringComparer.Ordinal.Equals(
                        e.InnerException.GetType().FullName,
                        "Microsoft.HostCompute.Interop.ObjectNotFoundException"))
                    ErrorCode = InvalidContainerId;
        /// Run some tasks on MTA thread if needed.
        private static void RunOnMTAThread(ThreadStart threadProc)
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.MTA)
                threadProc();
                Thread executionThread = new Thread(new ThreadStart(threadProc));
                executionThread.SetApartmentState(ApartmentState.MTA);
                executionThread.Start();
                executionThread.Join();
        /// Get error message from the thrown exception.
        private static string GetErrorMessageFromException(Exception e)
            string errorMessage = e.Message;
                errorMessage += " " + e.InnerException.Message;

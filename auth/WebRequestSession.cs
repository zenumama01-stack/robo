using System.Security.Authentication;
    /// WebRequestSession for holding session infos.
    public class WebRequestSession : IDisposable
        private HttpClient? _client;
        private CookieContainer _cookies;
        private bool _useDefaultCredentials;
        private ICredentials? _credentials;
        private X509CertificateCollection? _certificates;
        private IWebProxy? _proxy;
        private int _maximumRedirection;
        private WebSslProtocol _sslProtocol;
        private bool _allowAutoRedirect;
        private bool _skipCertificateCheck;
        private bool _noProxy;
        private TimeSpan _connectionTimeout;
        private UnixDomainSocketEndPoint? _unixSocket;
        /// Contains true if an existing HttpClient had to be disposed and recreated since the WebSession was last used.
        private bool _disposedClient;
        /// Gets or sets the Header property.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public Dictionary<string, string> Headers { get; set; }
        /// Gets or sets the content Headers when using HttpClient.
        internal Dictionary<string, string> ContentHeaders { get; set; }
        /// Gets or sets the Cookies property.
        public CookieContainer Cookies { get => _cookies; set => SetClassVar(ref _cookies, value); }
        #region Credentials
        /// Gets or sets the UseDefaultCredentials property.
        public bool UseDefaultCredentials { get => _useDefaultCredentials; set => SetStructVar(ref _useDefaultCredentials, value); }
        /// Gets or sets the Credentials property.
        public ICredentials? Credentials { get => _credentials; set => SetClassVar(ref _credentials, value); }
        /// Gets or sets the Certificates property.
        public X509CertificateCollection? Certificates { get => _certificates; set => SetClassVar(ref _certificates, value); }
        #endregion Credentials
        /// Gets or sets the UserAgent property.
        public string UserAgent { get; set; }
        /// Gets or sets the Proxy property.
        public IWebProxy? Proxy
            get => _proxy;
                SetClassVar(ref _proxy, value);
                if (_proxy is not null)
                    NoProxy = false;
        /// Gets or sets the MaximumRedirection property.
        public int MaximumRedirection { get => _maximumRedirection; set => SetStructVar(ref _maximumRedirection, value); }
        /// Gets or sets the count of retries for request failures.
        public int MaximumRetryCount { get; set; }
        /// Gets or sets the interval in seconds between retries.
        public int RetryIntervalInSeconds { get; set; }
        /// Initializes a new instance of the <see cref="WebRequestSession"/> class.
        public WebRequestSession()
            // Build the headers collection
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            ContentHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            // Build the cookie jar
            _cookies = new CookieContainer();
            // Initialize the credential and certificate caches
            _useDefaultCredentials = false;
            _credentials = null;
            _certificates = null;
            // Setup the default UserAgent
            UserAgent = PSUserAgent.UserAgent;
            _proxy = null;
            _maximumRedirection = -1;
            _allowAutoRedirect = true;
        internal WebSslProtocol SslProtocol { set => SetStructVar(ref _sslProtocol, value); }
        internal bool SkipCertificateCheck { set => SetStructVar(ref _skipCertificateCheck, value); }
        internal TimeSpan ConnectionTimeout { set => SetStructVar(ref _connectionTimeout, value); }
        internal UnixDomainSocketEndPoint UnixSocket { set => SetClassVar(ref _unixSocket, value); }
        internal bool NoProxy
                SetStructVar(ref _noProxy, value);
                if (_noProxy)
                    Proxy = null;
        /// Add a X509Certificate to the Certificates collection.
        /// <param name="certificate">The certificate to be added.</param>
        internal void AddCertificate(X509Certificate certificate)
            Certificates ??= new X509CertificateCollection();
            if (!Certificates.Contains(certificate))
                ResetClient();
                Certificates.Add(certificate);
        /// Gets an existing or creates a new HttpClient for this WebRequest session if none currently exists (either because it was never
        /// created, or because changes to the WebSession properties required the existing HttpClient to be disposed).
        /// <param name="suppressHttpClientRedirects">True if the caller does not want the HttpClient to ever handle redirections automatically.</param>
        /// <param name="clientWasReset">Contains true if an existing HttpClient had to be disposed and recreated since the WebSession was last used.</param>
        /// <returns>The HttpClient cached in the WebSession, based on all current settings.</returns>
        internal HttpClient GetHttpClient(bool suppressHttpClientRedirects, out bool clientWasReset)
            // Do not auto redirect if the caller does not want it, or maximum redirections is 0
            SetStructVar(ref _allowAutoRedirect, !(suppressHttpClientRedirects || MaximumRedirection == 0));
            clientWasReset = _disposedClient;
            if (_client is null)
                _client = CreateHttpClient();
                _disposedClient = false;
            return _client;
        private HttpClient CreateHttpClient()
            SocketsHttpHandler handler = new();
            if (_unixSocket is not null)
                handler.ConnectCallback = async (context, token) =>
                    Socket socket = new(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
                    await socket.ConnectAsync(_unixSocket).ConfigureAwait(false);
                    return new NetworkStream(socket, ownsSocket: false);
            handler.CookieContainer = Cookies;
            handler.AutomaticDecompression = DecompressionMethods.All;
            if (Credentials is not null)
                handler.Credentials = Credentials;
            else if (UseDefaultCredentials)
                handler.Credentials = CredentialCache.DefaultCredentials;
                handler.UseProxy = false;
            else if (Proxy is not null)
                handler.Proxy = Proxy;
            if (Certificates is not null)
                handler.SslOptions.ClientCertificates = new X509CertificateCollection(Certificates);
            if (_skipCertificateCheck)
                handler.SslOptions.RemoteCertificateValidationCallback = delegate { return true; };
            handler.AllowAutoRedirect = _allowAutoRedirect;
            if (_allowAutoRedirect && MaximumRedirection > 0)
                handler.MaxAutomaticRedirections = MaximumRedirection;
            handler.SslOptions.EnabledSslProtocols = (SslProtocols)_sslProtocol;
            // Check timeout setting (in seconds)
            return new HttpClient(handler)
                Timeout = _connectionTimeout
        private void SetClassVar<T>(ref T oldValue, T newValue) where T : class?
            if (oldValue != newValue)
                oldValue = newValue;
        private void SetStructVar<T>(ref T oldValue, T newValue) where T : struct
            if (!oldValue.Equals(newValue))
        private void ResetClient()
            if (_client is not null)
                _disposedClient = true;
                _client.Dispose();
                _client = null;
        /// Dispose the WebRequestSession.
        /// <param name="disposing">True when called from Dispose() and false when called from finalizer.</param>
                    _client?.Dispose();

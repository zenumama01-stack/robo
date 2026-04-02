    /// Creates a WSMan Session option hashtable which can be passed into WSMan
    /// cmdlets:
    /// Get-WSManInstance
    /// Set-WSManInstance
    /// Invoke-WSManAction
    /// Connect-WSMan.
    [Cmdlet(VerbsCommon.New, "WSManSessionOption", HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2096845")]
    [OutputType(typeof(SessionOption))]
    public class NewWSManSessionOptionCommand : PSCmdlet
        public ProxyAccessType ProxyAccessType
                return _proxyaccesstype;
                _proxyaccesstype = value;
        private ProxyAccessType _proxyaccesstype;
        /// from.  The available options should be as follows:
        /// - Negotiate: Use the default authentication (ad defined by the underlying
        /// - Basic:  Use basic authentication for establishing a remote connection
        /// - Digest: Use Digest authentication for establishing a remote connection.
        public ProxyAuthentication ProxyAuthentication
                return proxyauthentication;
                proxyauthentication = value;
        private ProxyAuthentication proxyauthentication;
                return _proxycredential;
                _proxycredential = value;
        private PSCredential _proxycredential;
        /// The following is the definition of the input parameter "SkipCACheck".
        /// When connecting over HTTPS, the client does not validate that the server
        /// certificate is signed by a trusted certificate authority (CA). Use only when
        /// the remote computer is trusted by other means, for example, if the remote
        /// computer is part of a network that is physically secure and isolated or the
        /// remote computer is listed as a trusted host in WinRM configuration.
                return skipcacheck;
                skipcacheck = value;
        private bool skipcacheck;
        /// The following is the definition of the input parameter "SkipCNCheck".
        /// Indicates that certificate common name (CN) of the server need not match the
        /// hostname of the server. Used only in remote operations using https. This
        /// option should only be used for trusted machines.
                return skipcncheck;
                skipcncheck = value;
        private bool skipcncheck;
        /// The following is the definition of the input parameter "SkipRevocation".
                return skiprevocationcheck;
                skiprevocationcheck = value;
        private bool skiprevocationcheck;
        /// The following is the definition of the input parameter "SPNPort".
        /// Appends port number to the connection Service Principal Name SPN of the
        /// remote server.
        /// SPN is used when authentication mechanism is Kerberos or Negotiate.
        public int SPNPort
                return spnport;
                spnport = value;
        private int spnport;
        /// The following is the definition of the input parameter "Timeout".
        /// Defines the timeout in ms for the wsman operation.
        [Alias("OperationTimeoutMSec")]
        public int OperationTimeout
                return operationtimeout;
                operationtimeout = value;
        private int operationtimeout;
        /// The following is the definition of the input parameter "UnEncrypted".
        /// Specifies that no encryption will be used when doing remote operations over
        /// http. Unencrypted traffic is not allowed by default and must be enabled in
        /// the local configuration.
                return noencryption;
                noencryption = value;
        private bool noencryption;
        /// The following is the definition of the input parameter "UTF16".
        /// Indicates the request is encoded in UTF16 format rather than UTF8 format;
        /// UTF8 is the default.
        public SwitchParameter UseUTF16
                return useutf16;
                useutf16 = value;
        private bool useutf16;
            if (proxyauthentication.Equals(ProxyAuthentication.Basic) || proxyauthentication.Equals(ProxyAuthentication.Digest))
                if (_proxycredential == null)
                    InvalidOperationException ex = new InvalidOperationException(helper.GetResourceMsgFromResourcetext("NewWSManSessionOptionCred"));
                    ErrorRecord er = new ErrorRecord(ex, "InvalidOperationException", ErrorCategory.InvalidOperation, null);
            if ((_proxycredential != null) && (proxyauthentication == 0))
                InvalidOperationException ex = new InvalidOperationException(helper.GetResourceMsgFromResourcetext("NewWSManSessionOptionAuth"));
            // Creating the Session Object
            SessionOption objSessionOption = new SessionOption();
            objSessionOption.SPNPort = spnport;
            objSessionOption.UseUtf16 = useutf16;
            objSessionOption.SkipCNCheck = skipcncheck;
            objSessionOption.SkipCACheck = skipcacheck;
            objSessionOption.OperationTimeout = operationtimeout;
            objSessionOption.SkipRevocationCheck = skiprevocationcheck;
            // Proxy Settings
            objSessionOption.ProxyAccessType = _proxyaccesstype;
            objSessionOption.ProxyAuthentication = proxyauthentication;
            if (noencryption)
                objSessionOption.UseEncryption = false;
            if (_proxycredential != null)
                NetworkCredential nwCredentials = _proxycredential.GetNetworkCredential();
                objSessionOption.ProxyCredential = nwCredentials;
            WriteObject(objSessionOption);

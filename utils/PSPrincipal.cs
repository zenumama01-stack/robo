 * Contains definition for PSSenderInfo, PSPrincipal, PSIdentity which are
 * used to provide remote user information to different plugin snapins
 * like Exchange.
    /// This class is used in the server side remoting scenarios. This class
    /// holds information about the incoming connection like:
    /// (a) Connecting User information
    /// (b) Connection String used by the user to connect to the server.
    public sealed class PSSenderInfo : ISerializable
        private PSPrimitiveDictionary _applicationArguments;
            PSObject psObject = PSObject.AsPSObject(this);
        /// Deserialization constructor.
        private PSSenderInfo(SerializationInfo info, StreamingContext context)
            string serializedData = null;
                serializedData = info.GetValue("CliXml", typeof(string)) as string;
                // When a workflow is run locally, there won't be PSSenderInfo
            if (serializedData == null)
                PSSenderInfo senderInfo = DeserializingTypeConverter.RehydratePSSenderInfo(result);
                UserInfo = senderInfo.UserInfo;
                ConnectionString = senderInfo.ConnectionString;
                _applicationArguments = senderInfo._applicationArguments;
                // Ignore conversion errors
        /// Constructs PSPrincipal using PSIdentity and a token (used to construct WindowsIdentity)
        /// <param name="userPrincipal">
        /// Connecting User Information
        /// <param name="httpUrl">
        /// httpUrl element (from WSMAN_SENDER_DETAILS struct).
        public PSSenderInfo(PSPrincipal userPrincipal, string httpUrl)
            UserInfo = userPrincipal;
            ConnectionString = httpUrl;
        /// Contains information related to the user connecting to the server.
        public PSPrincipal UserInfo
            // No public set because PSSenderInfo/PSPrincipal is used by PSSessionConfiguration's
            // and usually they dont cache this data internally..so did not want to give
            // cmdlets/scripts a chance to modify these.
        /// Contains the TimeZone information from the client machine.
        public TimeZoneInfo ClientTimeZone => null;
        /// Connection string used by the client to connect to the server. This is
        /// directly taken from WSMAN_SENDER_DETAILS struct (from wsman.h)
        public string ConnectionString
        /// Application arguments (i.e. specified in New-PSSessionOptions -ApplicationArguments)
        public PSPrimitiveDictionary ApplicationArguments
            get { return _applicationArguments; }
            internal set { _applicationArguments = value; }
        /// "ConfigurationName" from the sever remote session.
        public string ConfigurationName { get; internal set; }
    /// Defines the basic functionality of a PSPrincipal object.
    public sealed class PSPrincipal : IPrincipal
        /// Gets the identity of the current user principal.
        public PSIdentity Identity
        /// Gets the WindowsIdentity (if possible) representation of the current Identity.
        /// PSPrincipal can represent any user for example a LiveID user, network user within
        /// a domain etc. This property tries to convert the Identity to WindowsIdentity
        /// using the user token supplied.
        public WindowsIdentity WindowsIdentity
        /// Gets the identity of the current principal.
        IIdentity IPrincipal.Identity
            get { return this.Identity; }
        /// Determines if the current principal belongs to a specified rule.
        /// If we were able to get a WindowsIdentity then this will perform the
        /// check using the WindowsIdentity otherwise this will return false.
        /// <param name="role"></param>
        public bool IsInRole(string role)
            if (WindowsIdentity != null)
                // Get Windows Principal for this identity
                WindowsPrincipal windowsPrincipal = new WindowsPrincipal(WindowsIdentity);
                return windowsPrincipal.IsInRole(role);
        /// Internal overload of IsInRole() taking a WindowsBuiltInRole enum value.
        internal bool IsInRole(WindowsBuiltInRole role)
        /// Constructs PSPrincipal using PSIdentity and a WindowsIdentity.
        /// <param name="identity">
        /// An instance of PSIdentity
        /// <param name="windowsIdentity">
        /// An instance of WindowsIdentity, if psIdentity represents a windows user. This can be
        /// null.
        public PSPrincipal(PSIdentity identity, WindowsIdentity windowsIdentity)
            Identity = identity;
            WindowsIdentity = windowsIdentity;
    /// Defines the basic functionality of a PSIdentity object.
    public sealed class PSIdentity : IIdentity
        /// Gets the type of authentication used.
        /// For a WSMan service authenticated user this will be one of the following:
        ///  WSMAN_DEFAULT_AUTHENTICATION
        ///  WSMAN_NO_AUTHENTICATION
        ///  WSMAN_AUTH_DIGEST
        ///  WSMAN_AUTH_NEGOTIATE
        ///  WSMAN_AUTH_BASIC
        ///  WSMAN_AUTH_KERBEROS
        ///  WSMAN_AUTH_CLIENT_CERTIFICATE
        ///  WSMAN_AUTH_LIVEID.
        public string AuthenticationType { get; }
        /// Gets a value that indicates whether the user has been authenticated.
        public bool IsAuthenticated { get; }
        /// Gets the name of the user.
        /// Gets the certificate details of the user if supported, null otherwise.
        public PSCertificateDetails CertificateDetails { get; }
        /// Constructor used to construct a PSIdentity object.
        /// <param name="authType">
        /// Type of authentication used to authenticate this user.
        ///  WSMAN_AUTH_LIVEID
        /// <param name="isAuthenticated">
        /// true if this user is authenticated.
        /// Name of the user
        /// <param name="cert">
        /// Certificate details if Certificate authentication is used.
        public PSIdentity(string authType, bool isAuthenticated, string userName, PSCertificateDetails cert)
            AuthenticationType = authType;
            IsAuthenticated = isAuthenticated;
            Name = userName;
            CertificateDetails = cert;
    /// Represents the certificate of a user.
    public sealed class PSCertificateDetails
        /// Gets Subject of the certificate.
        public string Subject { get; }
        /// Gets the issuer name of the certificate.
        public string IssuerName { get; }
        /// Gets the issuer thumb print.
        public string IssuerThumbprint { get; }
        /// Constructor used to construct a PSCertificateDetails object.
        /// Subject of the certificate.
        /// <param name="issuerName">
        /// Issuer name of the certificate.
        /// <param name="issuerThumbprint">
        /// Issuer thumb print of the certificate.
        public PSCertificateDetails(string subject, string issuerName, string issuerThumbprint)
            Subject = subject;
            IssuerName = issuerName;
            IssuerThumbprint = issuerThumbprint;

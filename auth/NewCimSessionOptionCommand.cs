    /// Define Protocol type.
    public enum ProtocolType
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        Dcom,
        Wsman
    /// The Cmdlet allows the IT Pro to create a CimSessionOptions object that she/he
    /// can subsequently use to create one or more CimSession connections. The
    /// options object holds the CIM Session information that is less commonly set
    /// and used by the IT Pro, and most commonly defaulted.
    ///
    /// The Cmdlet has two parameter sets, one for WMMan options and one for DCOM
    /// options. Depending on the arguments the Cmdlet will return an instance of
    /// DComSessionOptions or WSManSessionOptions, which derive from
    /// CimSessionOptions.
    [Alias("ncso")]
    [Cmdlet(VerbsCommon.New, "CimSessionOption", DefaultParameterSetName = ProtocolNameParameterSet, HelpUri = "https://go.microsoft.com/fwlink/?LinkId=227969")]
    [OutputType(typeof(CimSessionOptions))]
    public sealed class NewCimSessionOptionCommand : CimBaseCommand
        /// Initializes a new instance of the <see cref="NewCimSessionOptionCommand"/> class.
        public NewCimSessionOptionCommand()
        /// The following is the definition of the input parameter "NoEncryption".
        /// Switch indicating if WSMan can use no encryption in the given CimSession (there are also global client and server WSMan settings - AllowUnencrypted).
        [Parameter(ParameterSetName = WSManParameterSet)]
        public SwitchParameter NoEncryption
                return noEncryption;
                noEncryption = value;
                noEncryptionSet = true;
                base.SetParameter(value, nameNoEncryption);
        private SwitchParameter noEncryption;
        private bool noEncryptionSet = false;
        /// The following is the definition of the input parameter "CertificateCACheck".
        /// Switch indicating if Certificate Authority should be validated.
                   ParameterSetName = WSManParameterSet)]
        public SwitchParameter SkipCACheck
                return skipCACheck;
                skipCACheck = value;
                skipCACheckSet = true;
                base.SetParameter(value, nameSkipCACheck);
        private SwitchParameter skipCACheck;
        private bool skipCACheckSet = false;
        /// The following is the definition of the input parameter "CertificateCNCheck".
        /// Switch indicating if Certificate Name should be validated.
        public SwitchParameter SkipCNCheck
                return skipCNCheck;
                skipCNCheck = value;
                skipCNCheckSet = true;
                base.SetParameter(value, nameSkipCNCheck);
        private SwitchParameter skipCNCheck;
        private bool skipCNCheckSet = false;
        /// The following is the definition of the input parameter "CertRevocationCheck".
        /// Switch indicating if certificate should be revoked.
        public SwitchParameter SkipRevocationCheck
                return skipRevocationCheck;
                skipRevocationCheck = value;
                skipRevocationCheckSet = true;
                base.SetParameter(value, nameSkipRevocationCheck);
        private SwitchParameter skipRevocationCheck;
        private bool skipRevocationCheckSet = false;
        /// The following is the definition of the input parameter "EncodePortInServicePrincipalName".
        /// Switch indicating if to encode Port In Service Principal Name.
        public SwitchParameter EncodePortInServicePrincipalName
                return encodeportinserviceprincipalname;
                encodeportinserviceprincipalname = value;
                encodeportinserviceprincipalnameSet = true;
                base.SetParameter(value, nameEncodePortInServicePrincipalName);
        private SwitchParameter encodeportinserviceprincipalname;
        private bool encodeportinserviceprincipalnameSet = false;
        /// The following is the definition of the input parameter "Encoding".
        /// Defined the message encoding.
        /// The allowed encodings are { Default | Utf8 | Utf16 }. The default value
        /// should be Utf8.
        public PacketEncoding Encoding
                return encoding;
                encoding = value;
                encodingSet = true;
                base.SetParameter(value, nameEncoding);
        private PacketEncoding encoding;
        private bool encodingSet = false;
        /// The following is the definition of the input parameter "HttpPrefix".
        /// This is the HTTP URL on the server on which the WSMan service is listening.
        /// In most cases it is /wsman, which is the default.
        public Uri HttpPrefix
                return httpprefix;
                httpprefix = value;
                base.SetParameter(value, nameHttpPrefix);
        private Uri httpprefix;
        /// The following is the definition of the input parameter "MaxEnvelopeSizeKB".
        /// Sets the limit to the maximum size of the WSMan message envelope.
        public uint MaxEnvelopeSizeKB
                return maxenvelopesizekb;
                maxenvelopesizekb = value;
                maxenvelopesizekbSet = true;
                base.SetParameter(value, nameMaxEnvelopeSizeKB);
        private uint maxenvelopesizekb;
        private bool maxenvelopesizekbSet = false;
        /// The following is the definition of the input parameter "ProxyAuthentication".
        /// Which proxy authentication types to use: Allowed set is:
        public PasswordAuthenticationMechanism ProxyAuthentication
                return proxyAuthentication;
                proxyAuthentication = value;
                proxyauthenticationSet = true;
                base.SetParameter(value, nameProxyAuthentication);
        private PasswordAuthenticationMechanism proxyAuthentication;
        private bool proxyauthenticationSet = false;
        /// The following is the definition of the input parameter "ProxyCertificateThumbprint".
        public string ProxyCertificateThumbprint
                return proxycertificatethumbprint;
                proxycertificatethumbprint = value;
                base.SetParameter(value, nameProxyCertificateThumbprint);
        private string proxycertificatethumbprint;
        /// The following is the definition of the input parameter "ProxyCredential".
        /// Ps Credential used by the proxy server when required by the server.
        public PSCredential ProxyCredential
                return proxycredential;
                proxycredential = value;
                base.SetParameter(value, nameProxyCredential);
        private PSCredential proxycredential;
        /// The following is the definition of the input parameter "ProxyType".
        /// Which proxy type to use: Valid set is:
        ///  { InternetExplorer | WinHttp | Auto | None }
        public ProxyType ProxyType
                return proxytype;
                proxytype = value;
                proxytypeSet = true;
                base.SetParameter(value, nameProxyType);
        private ProxyType proxytype;
        private bool proxytypeSet = false;
        /// The following is the definition of the input parameter "UseSSL".
        /// Switch indicating if Secure Sockets Layer connection should be used.
        public SwitchParameter UseSsl
                return usessl;
                usessl = value;
                usesslSet = true;
                base.SetParameter(value, nameUseSsl);
        private SwitchParameter usessl;
        private bool usesslSet = false;
        /// The following is the definition of the input parameter "Impersonation".
        /// Used to select if, and if so what kind of, impersonation should be used.
        /// Applies only to the DCOM channel.
        [Parameter(ParameterSetName = DcomParameterSet)]
        public ImpersonationType Impersonation
                return impersonation;
                impersonation = value;
                impersonationSet = true;
                base.SetParameter(value, nameImpersonation);
        private ImpersonationType impersonation;
        private bool impersonationSet = false;
        /// The following is the definition of the input parameter "PacketIntegrity".
        /// Switch indicating if the package integrity in DCOM connections should be
        /// checked/enforced.
        public SwitchParameter PacketIntegrity
                return packetintegrity;
                packetintegrity = value;
                packetintegritySet = true;
                base.SetParameter(value, namePacketIntegrity);
        private SwitchParameter packetintegrity;
        private bool packetintegritySet = false;
        /// The following is the definition of the input parameter "PacketPrivacy".
        /// Switch indicating if packet privacy of the packets in DCOM communications
        /// should be checked/enforced.
        public SwitchParameter PacketPrivacy
                return packetprivacy;
                packetprivacy = value;
                packetprivacySet = true;
                base.SetParameter(value, namePacketPrivacy);
        private SwitchParameter packetprivacy;
        private bool packetprivacySet = false;
        /// The following is the definition of the input parameter "Protocol".
            ParameterSetName = ProtocolNameParameterSet)]
        public ProtocolType Protocol
                protocol = value;
                base.SetParameter(value, nameProtocol);
        private ProtocolType protocol;
        /// The following is the definition of the input parameter "UICulture".
        /// Specifies the UI Culture to use. i.e. en-us, ar-sa.
        public CultureInfo UICulture { get; set; }
        /// The following is the definition of the input parameter "Culture".
        /// Specifies the culture to use. i.e. en-us, ar-sa.
        public CultureInfo Culture { get; set; }
            CimSessionOptions options;
                case WSManParameterSet:
                        options = CreateWSMANSessionOptions();
                case DcomParameterSet:
                        options = CreateDComSessionOptions();
                case ProtocolNameParameterSet:
                    switch (Protocol)
                if (this.Culture != null)
                    options.Culture = this.Culture;
                if (this.UICulture != null)
                    options.UICulture = this.UICulture;
                this.WriteObject(options);
        #region helper functions
        /// Create DComSessionOptions.
        internal DComSessionOptions CreateDComSessionOptions()
            DComSessionOptions dcomoptions = new();
            if (this.impersonationSet)
                dcomoptions.Impersonation = this.Impersonation;
                this.impersonationSet = false;
                dcomoptions.Impersonation = ImpersonationType.Impersonate;
            if (this.packetintegritySet)
                dcomoptions.PacketIntegrity = this.packetintegrity;
                this.packetintegritySet = false;
                dcomoptions.PacketIntegrity = true;
            if (this.packetprivacySet)
                dcomoptions.PacketPrivacy = this.PacketPrivacy;
                this.packetprivacySet = false;
                dcomoptions.PacketPrivacy = true;
            return dcomoptions;
        /// Create WSMANSessionOptions.
        internal WSManSessionOptions CreateWSMANSessionOptions()
            WSManSessionOptions wsmanoptions = new();
            if (this.noEncryptionSet)
                wsmanoptions.NoEncryption = true;
                this.noEncryptionSet = false;
                wsmanoptions.NoEncryption = false;
            if (this.skipCACheckSet)
                wsmanoptions.CertCACheck = false;
                this.skipCACheckSet = false;
                wsmanoptions.CertCACheck = true;
            if (this.skipCNCheckSet)
                wsmanoptions.CertCNCheck = false;
                this.skipCNCheckSet = false;
                wsmanoptions.CertCNCheck = true;
            if (this.skipRevocationCheckSet)
                wsmanoptions.CertRevocationCheck = false;
                this.skipRevocationCheckSet = false;
                wsmanoptions.CertRevocationCheck = true;
            if (this.encodeportinserviceprincipalnameSet)
                wsmanoptions.EncodePortInServicePrincipalName = this.EncodePortInServicePrincipalName;
                this.encodeportinserviceprincipalnameSet = false;
                wsmanoptions.EncodePortInServicePrincipalName = false;
            if (this.encodingSet)
                wsmanoptions.PacketEncoding = this.Encoding;
                wsmanoptions.PacketEncoding = PacketEncoding.Utf8;
            if (this.HttpPrefix != null)
                wsmanoptions.HttpUrlPrefix = this.HttpPrefix;
            if (this.maxenvelopesizekbSet)
                wsmanoptions.MaxEnvelopeSize = this.MaxEnvelopeSizeKB;
                wsmanoptions.MaxEnvelopeSize = 0;
            if (!string.IsNullOrWhiteSpace(this.ProxyCertificateThumbprint))
                CimCredential credentials = new(CertificateAuthenticationMechanism.Default, this.ProxyCertificateThumbprint);
                wsmanoptions.AddProxyCredentials(credentials);
            if (this.proxyauthenticationSet)
                this.proxyauthenticationSet = false;
                DebugHelper.WriteLogEx("create credential", 1);
                CimCredential credentials = CreateCimCredentials(this.ProxyCredential, this.ProxyAuthentication, @"New-CimSessionOption", @"ProxyAuthentication");
                if (credentials != null)
                        DebugHelper.WriteLogEx("Add proxy credential", 1);
                        DebugHelper.WriteLogEx(ex.ToString(), 1);
            if (this.proxytypeSet)
                wsmanoptions.ProxyType = this.ProxyType;
                this.proxytypeSet = false;
                wsmanoptions.ProxyType = Options.ProxyType.WinHttp;
            if (this.usesslSet)
                wsmanoptions.UseSsl = this.UseSsl;
                this.usesslSet = false;
                wsmanoptions.UseSsl = false;
            wsmanoptions.DestinationPort = 0;
            return wsmanoptions;
        internal const string nameNoEncryption = "NoEncryption";
        internal const string nameSkipCACheck = "SkipCACheck";
        internal const string nameSkipCNCheck = "SkipCNCheck";
        internal const string nameSkipRevocationCheck = "SkipRevocationCheck";
        internal const string nameEncodePortInServicePrincipalName = "EncodePortInServicePrincipalName";
        internal const string nameEncoding = "Encoding";
        internal const string nameHttpPrefix = "HttpPrefix";
        internal const string nameMaxEnvelopeSizeKB = "MaxEnvelopeSizeKB";
        internal const string nameProxyAuthentication = "ProxyAuthentication";
        internal const string nameProxyCertificateThumbprint = "ProxyCertificateThumbprint";
        internal const string nameProxyCredential = "ProxyCredential";
        internal const string nameProxyType = "ProxyType";
        internal const string nameUseSsl = "UseSsl";
        internal const string nameImpersonation = "Impersonation";
        internal const string namePacketIntegrity = "PacketIntegrity";
        internal const string namePacketPrivacy = "PacketPrivacy";
        internal const string nameProtocol = "Protocol";
                nameNoEncryption, new HashSet<ParameterDefinitionEntry> {
                                    new ParameterDefinitionEntry(CimBaseCommand.WSManParameterSet, false),
                nameSkipCACheck, new HashSet<ParameterDefinitionEntry> {
                nameSkipCNCheck, new HashSet<ParameterDefinitionEntry> {
                nameSkipRevocationCheck, new HashSet<ParameterDefinitionEntry> {
                nameEncodePortInServicePrincipalName, new HashSet<ParameterDefinitionEntry> {
                nameEncoding, new HashSet<ParameterDefinitionEntry> {
                nameHttpPrefix, new HashSet<ParameterDefinitionEntry> {
                nameMaxEnvelopeSizeKB, new HashSet<ParameterDefinitionEntry> {
                nameProxyAuthentication, new HashSet<ParameterDefinitionEntry> {
                nameProxyCertificateThumbprint, new HashSet<ParameterDefinitionEntry> {
                nameProxyCredential, new HashSet<ParameterDefinitionEntry> {
                nameProxyType, new HashSet<ParameterDefinitionEntry> {
                nameUseSsl, new HashSet<ParameterDefinitionEntry> {
                nameImpersonation, new HashSet<ParameterDefinitionEntry> {
                                    new ParameterDefinitionEntry(CimBaseCommand.DcomParameterSet, false),
                namePacketIntegrity, new HashSet<ParameterDefinitionEntry> {
                namePacketPrivacy, new HashSet<ParameterDefinitionEntry> {
                nameProtocol, new HashSet<ParameterDefinitionEntry> {
                                    new ParameterDefinitionEntry(CimBaseCommand.ProtocolNameParameterSet, true),
            {   CimBaseCommand.ProtocolNameParameterSet, new ParameterSetEntry(1, true)     },
            {   CimBaseCommand.DcomParameterSet, new ParameterSetEntry(0)     },
            {   CimBaseCommand.WSManParameterSet, new ParameterSetEntry(0)     },

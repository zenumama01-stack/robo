[assembly: CLSCompliant(true)]
    /// Session option class.
    public sealed class SessionOption
        /// Property.
        public bool SkipCACheck { get; set; }
        public bool SkipCNCheck { get; set; }
        public bool SkipRevocationCheck { get; set; }
        public bool UseEncryption { get; set; } = true;
        public bool UseUtf16 { get; set; }
        public ProxyAuthentication ProxyAuthentication { get; set; }
        public int SPNPort { get; set; }
        public int OperationTimeout { get; set; }
        public NetworkCredential ProxyCredential { get; set; }
        public ProxyAccessType ProxyAccessType { get; set; }
    public enum ProxyAccessType
        ProxyIEConfig = 0,
        ProxyWinHttpConfig = 1,
        ProxyAutoDetect = 2,
        ProxyNoProxyServer = 3
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum ProxyAuthentication
        Negotiate = 1,
        Basic = 2,
        Digest = 4

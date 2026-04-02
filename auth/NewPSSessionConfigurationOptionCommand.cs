    /// Implementing type for WSManConfigurationOption.
    public class WSManConfigurationOption : PSTransportOption
        private const string Token = " {0}='{1}'";
        private const string QuotasToken = "<Quotas {0} />";
        internal const string AttribOutputBufferingMode = "OutputBufferingMode";
        internal static readonly System.Management.Automation.Runspaces.OutputBufferingMode? DefaultOutputBufferingMode = System.Management.Automation.Runspaces.OutputBufferingMode.Block;
        private System.Management.Automation.Runspaces.OutputBufferingMode? _outputBufferingMode = null;
        private const string AttribProcessIdleTimeout = "ProcessIdleTimeoutSec";
        internal static readonly int? DefaultProcessIdleTimeout_ForPSRemoting = 0; // in seconds
        private int? _processIdleTimeoutSec = null;
        internal const string AttribMaxIdleTimeout = "MaxIdleTimeoutms";
        internal static readonly int? DefaultMaxIdleTimeout = int.MaxValue;
        private int? _maxIdleTimeoutSec = null;
        internal const string AttribIdleTimeout = "IdleTimeoutms";
        internal static readonly int? DefaultIdleTimeout = 7200; // 2 hours in seconds
        private int? _idleTimeoutSec = null;
        private const string AttribMaxConcurrentUsers = "MaxConcurrentUsers";
        internal static readonly int? DefaultMaxConcurrentUsers = int.MaxValue;
        private int? _maxConcurrentUsers = null;
        private const string AttribMaxProcessesPerSession = "MaxProcessesPerShell";
        internal static readonly int? DefaultMaxProcessesPerSession = int.MaxValue;
        private int? _maxProcessesPerSession = null;
        private const string AttribMaxMemoryPerSessionMB = "MaxMemoryPerShellMB";
        internal static readonly int? DefaultMaxMemoryPerSessionMB = int.MaxValue;
        private int? _maxMemoryPerSessionMB = null;
        private const string AttribMaxSessions = "MaxShells";
        internal static readonly int? DefaultMaxSessions = int.MaxValue;
        private int? _maxSessions = null;
        private const string AttribMaxSessionsPerUser = "MaxShellsPerUser";
        internal static readonly int? DefaultMaxSessionsPerUser = int.MaxValue;
        private int? _maxSessionsPerUser = null;
        private const string AttribMaxConcurrentCommandsPerSession = "MaxConcurrentCommandsPerShell";
        internal static readonly int? DefaultMaxConcurrentCommandsPerSession = int.MaxValue;
        private int? _maxConcurrentCommandsPerSession = null;
        /// Constructor that instantiates with default values.
        internal WSManConfigurationOption()
        /// Override LoadFromDefaults method.
        /// <param name="keepAssigned">Keep old values.</param>
        protected internal override void LoadFromDefaults(bool keepAssigned)
            if (!keepAssigned || !_outputBufferingMode.HasValue)
                _outputBufferingMode = DefaultOutputBufferingMode;
            if (!keepAssigned || !_processIdleTimeoutSec.HasValue)
                _processIdleTimeoutSec = DefaultProcessIdleTimeout_ForPSRemoting;
            if (!keepAssigned || !_maxIdleTimeoutSec.HasValue)
                _maxIdleTimeoutSec = DefaultMaxIdleTimeout;
            if (!keepAssigned || !_idleTimeoutSec.HasValue)
                _idleTimeoutSec = DefaultIdleTimeout;
            if (!keepAssigned || !_maxConcurrentUsers.HasValue)
                _maxConcurrentUsers = DefaultMaxConcurrentUsers;
            if (!keepAssigned || !_maxProcessesPerSession.HasValue)
                _maxProcessesPerSession = DefaultMaxProcessesPerSession;
            if (!keepAssigned || !_maxMemoryPerSessionMB.HasValue)
                _maxMemoryPerSessionMB = DefaultMaxMemoryPerSessionMB;
            if (!keepAssigned || !_maxSessions.HasValue)
                _maxSessions = DefaultMaxSessions;
            if (!keepAssigned || !_maxSessionsPerUser.HasValue)
                _maxSessionsPerUser = DefaultMaxSessionsPerUser;
            if (!keepAssigned || !_maxConcurrentCommandsPerSession.HasValue)
                _maxConcurrentCommandsPerSession = DefaultMaxConcurrentCommandsPerSession;
        /// ProcessIdleTimeout in Seconds.
        public int? ProcessIdleTimeoutSec
                return _processIdleTimeoutSec;
                _processIdleTimeoutSec = value;
        /// MaxIdleTimeout in Seconds.
        public int? MaxIdleTimeoutSec
                return _maxIdleTimeoutSec;
                _maxIdleTimeoutSec = value;
        /// MaxSessions.
        public int? MaxSessions
                return _maxSessions;
                _maxSessions = value;
        /// MaxConcurrentCommandsPerSession.
        public int? MaxConcurrentCommandsPerSession
                return _maxConcurrentCommandsPerSession;
                _maxConcurrentCommandsPerSession = value;
        /// MaxSessionsPerUser.
        public int? MaxSessionsPerUser
                return _maxSessionsPerUser;
                _maxSessionsPerUser = value;
        /// MaxMemoryPerSessionMB.
        public int? MaxMemoryPerSessionMB
                return _maxMemoryPerSessionMB;
                _maxMemoryPerSessionMB = value;
        /// MaxProcessesPerSession.
        public int? MaxProcessesPerSession
                return _maxProcessesPerSession;
                _maxProcessesPerSession = value;
        /// MaxConcurrentUsers.
        public int? MaxConcurrentUsers
                return _maxConcurrentUsers;
                _maxConcurrentUsers = value;
        /// IdleTimeout in Seconds.
        public int? IdleTimeoutSec
                return _idleTimeoutSec;
                _idleTimeoutSec = value;
        /// OutputBufferingMode.
        public System.Management.Automation.Runspaces.OutputBufferingMode? OutputBufferingMode
                return _outputBufferingMode;
                _outputBufferingMode = value;
        internal override Hashtable ConstructQuotasAsHashtable()
            Hashtable quotas = new Hashtable();
            if (_idleTimeoutSec.HasValue)
                quotas[AttribIdleTimeout] = (1000 * _idleTimeoutSec.Value).ToString(CultureInfo.InvariantCulture);
            if (_maxConcurrentUsers.HasValue)
                quotas[AttribMaxConcurrentUsers] = _maxConcurrentUsers.Value.ToString(CultureInfo.InvariantCulture);
            if (_maxProcessesPerSession.HasValue)
                quotas[AttribMaxProcessesPerSession] = _maxProcessesPerSession.Value.ToString(CultureInfo.InvariantCulture);
            if (_maxMemoryPerSessionMB.HasValue)
                quotas[AttribMaxMemoryPerSessionMB] = _maxMemoryPerSessionMB.Value.ToString(CultureInfo.InvariantCulture);
            if (_maxSessionsPerUser.HasValue)
                quotas[AttribMaxSessionsPerUser] = _maxSessionsPerUser.Value.ToString(CultureInfo.InvariantCulture);
            if (_maxConcurrentCommandsPerSession.HasValue)
                quotas[AttribMaxConcurrentCommandsPerSession] = _maxConcurrentCommandsPerSession.Value.ToString(CultureInfo.InvariantCulture);
            if (_maxSessions.HasValue)
                quotas[AttribMaxSessions] = _maxSessions.Value.ToString(CultureInfo.InvariantCulture);
            if (_maxIdleTimeoutSec.HasValue)
                quotas[AttribMaxIdleTimeout] = (1000 * _maxIdleTimeoutSec.Value).ToString(CultureInfo.InvariantCulture);
            return quotas;
        /// ConstructQuotas.
        internal override string ConstructQuotas()
                sb.Append(string.Format(CultureInfo.InvariantCulture, Token, AttribIdleTimeout, 1000 * _idleTimeoutSec));
                sb.Append(string.Format(CultureInfo.InvariantCulture, Token, AttribMaxConcurrentUsers, _maxConcurrentUsers));
                sb.Append(string.Format(CultureInfo.InvariantCulture, Token, AttribMaxProcessesPerSession, _maxProcessesPerSession));
                sb.Append(string.Format(CultureInfo.InvariantCulture, Token, AttribMaxMemoryPerSessionMB, _maxMemoryPerSessionMB));
                sb.Append(string.Format(CultureInfo.InvariantCulture, Token, AttribMaxSessionsPerUser, _maxSessionsPerUser));
                sb.Append(string.Format(CultureInfo.InvariantCulture, Token, AttribMaxConcurrentCommandsPerSession, _maxConcurrentCommandsPerSession));
                sb.Append(string.Format(CultureInfo.InvariantCulture, Token, AttribMaxSessions, _maxSessions));
                // Special case max int value for unbounded default.
                sb.Append(string.Format(CultureInfo.InvariantCulture, Token, AttribMaxIdleTimeout,
                    (_maxIdleTimeoutSec == int.MaxValue) ? _maxIdleTimeoutSec : (1000 * _maxIdleTimeoutSec)));
            return sb.Length > 0
                ? string.Format(CultureInfo.InvariantCulture, QuotasToken, sb.ToString())
        /// ConstructOptionsXmlAttributes.
        internal override string ConstructOptionsAsXmlAttributes()
            if (_outputBufferingMode.HasValue)
                sb.Append(string.Format(CultureInfo.InvariantCulture, Token, AttribOutputBufferingMode, _outputBufferingMode.ToString()));
            if (_processIdleTimeoutSec.HasValue)
                sb.Append(string.Format(CultureInfo.InvariantCulture, Token, AttribProcessIdleTimeout, _processIdleTimeoutSec));
        internal override Hashtable ConstructOptionsAsHashtable()
            Hashtable table = new Hashtable();
                table[AttribOutputBufferingMode] = _outputBufferingMode.ToString();
                table[AttribProcessIdleTimeout] = _processIdleTimeoutSec;
            return table;
    /// Command to create an object for WSManConfigurationOption.
    [Cmdlet(VerbsCommon.New, "PSTransportOption", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=210608", RemotingCapability = RemotingCapability.None)]
    [OutputType(typeof(WSManConfigurationOption))]
    public sealed class NewPSTransportOptionCommand : PSCmdlet
        private readonly WSManConfigurationOption _option = new WSManConfigurationOption();
        /// MaxIdleTimeoutSec.
        [Parameter(ValueFromPipelineByPropertyName = true), ValidateRange(60, 2147483)]
                return _option.MaxIdleTimeoutSec;
                _option.MaxIdleTimeoutSec = value;
        /// ProcessIdleTimeoutSec.
        [Parameter(ValueFromPipelineByPropertyName = true), ValidateRange(0, 1209600)]
                return _option.ProcessIdleTimeoutSec;
                _option.ProcessIdleTimeoutSec = value;
        [Parameter(ValueFromPipelineByPropertyName = true), ValidateRange(1, int.MaxValue)]
                return _option.MaxSessions;
                _option.MaxSessions = value;
                return _option.MaxConcurrentCommandsPerSession;
                _option.MaxConcurrentCommandsPerSession = value;
                return _option.MaxSessionsPerUser;
                _option.MaxSessionsPerUser = value;
        [Parameter(ValueFromPipelineByPropertyName = true), ValidateRange(5, int.MaxValue)]
                return _option.MaxMemoryPerSessionMB;
                _option.MaxMemoryPerSessionMB = value;
                return _option.MaxProcessesPerSession;
                _option.MaxProcessesPerSession = value;
        [Parameter(ValueFromPipelineByPropertyName = true), ValidateRange(1, 100)]
                return _option.MaxConcurrentUsers;
                _option.MaxConcurrentUsers = value;
        /// IdleTimeoutMs.
                return _option.IdleTimeoutSec;
                _option.IdleTimeoutSec = value;
                return _option.OutputBufferingMode;
                _option.OutputBufferingMode = value;
        /// Overriding the base method.
            this.WriteObject(_option);

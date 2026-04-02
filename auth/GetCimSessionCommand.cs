    /// The command returns zero, one or more CimSession objects that represent
    /// connections with remote computers established from the current PS Session.
    [Alias("gcms")]
    [Cmdlet(VerbsCommon.Get, "CimSession", DefaultParameterSetName = ComputerNameSet, HelpUri = "https://go.microsoft.com/fwlink/?LinkId=227966")]
    [OutputType(typeof(CimSession))]
    public sealed class GetCimSessionCommand : CimBaseCommand
        /// Initializes a new instance of the <see cref="GetCimSessionCommand"/> class.
        public GetCimSessionCommand()
        /// Specifies one or more connections by providing their ComputerName(s). The
        /// Cmdlet then gets CimSession(s) opened with those connections. This parameter
        /// is an alternative to using CimSession(s) that also identifies the remote
        /// computer(s).
        /// This is the only optional parameter of the Cmdlet. If not provided, the
        /// Cmdlet returns all CimSession(s) live/active in the runspace.
        /// If an instance of CimSession is pipelined to Get-CimSession, the
        /// ComputerName property of the instance is bound by name with this parameter.
        [Parameter(Position = 0,
            ParameterSetName = ComputerNameSet)]
                return computername;
                computername = value;
        private string[] computername;
        /// The following is the definition of the input parameter "Id".
        /// Specifies one or more numeric Id(s) for which to get CimSession(s).
            ParameterSetName = SessionIdSet)]
        public uint[] Id
                return id;
                id = value;
                base.SetParameter(value, nameId);
        private uint[] id;
        /// The following is the definition of the input parameter "InstanceID".
        /// Specifies one or Session Instance IDs.
            ParameterSetName = InstanceIdSet)]
        public Guid[] InstanceId
                return instanceid;
                instanceid = value;
                base.SetParameter(value, nameInstanceId);
        private Guid[] instanceid;
        /// The following is the definition of the input parameter "Name".
        /// Specifies one or more session Name(s)  for which to get CimSession(s). The
        /// argument may contain wildcard characters.
            ParameterSetName = NameSet)]
        public string[] Name
                return name;
                name = value;
                base.SetParameter(value, nameName);
        private string[] name;
        #region cmdlet processing methods
            cimGetSession = new CimGetSession();
            cimGetSession.GetCimSession(this);
        /// <see cref="CimGetSession"/> object used to search CimSession from cache.
        private CimGetSession cimGetSession;
        internal const string nameId = "Id";
        internal const string nameInstanceId = "InstanceId";
        internal const string nameName = "Name";
                                    new ParameterDefinitionEntry(CimBaseCommand.ComputerNameSet, false),
                nameId, new HashSet<ParameterDefinitionEntry> {
                                    new ParameterDefinitionEntry(CimBaseCommand.SessionIdSet, true),
                nameInstanceId, new HashSet<ParameterDefinitionEntry> {
                                    new ParameterDefinitionEntry(CimBaseCommand.InstanceIdSet, true),
                nameName, new HashSet<ParameterDefinitionEntry> {
                                    new ParameterDefinitionEntry(CimBaseCommand.NameSet, true),
            {   CimBaseCommand.ComputerNameSet, new ParameterSetEntry(0, true)     },
            {   CimBaseCommand.SessionIdSet, new ParameterSetEntry(1)     },
            {   CimBaseCommand.InstanceIdSet, new ParameterSetEntry(1)     },
            {   CimBaseCommand.NameSet, new ParameterSetEntry(1)     },

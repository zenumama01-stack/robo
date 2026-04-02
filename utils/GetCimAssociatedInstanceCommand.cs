using System.Diagnostics.CodeAnalysis;
    /// The Cmdlet retrieves instances connected to the given instance, which
    /// is called the source instance, via a given association. In an
    /// association each instance has a named role, and the same instance can
    /// participate in an association in different roles. Hence, the Cmdlet
    /// takes SourceRole and AssociatorRole parameters in addition to the
    /// Association parameter.
    [Alias("gcai")]
    [Cmdlet(VerbsCommon.Get,
        GetCimAssociatedInstanceCommand.Noun,
        DefaultParameterSetName = CimBaseCommand.ComputerSetName,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkId=227958")]
    [OutputType(typeof(CimInstance))]
    public class GetCimAssociatedInstanceCommand : CimBaseCommand
        /// Initializes a new instance of the <see cref="GetCimAssociatedInstanceCommand"/> class.
        public GetCimAssociatedInstanceCommand()
            : base(parameters, parameterSets)
        /// The following is the definition of the input parameter "Association".
        /// Specifies the class name of the association to be traversed from the
        /// SourceRole to AssociatorRole.
        [Parameter(
            Position = 1,
            ValueFromPipelineByPropertyName = true)]
        public string Association { get; set; }
        /// The following is the definition of the input parameter "ResultClassName".
        /// Specifies the class name of the result class name, which associated with
        /// the given instance.
        [Parameter]
        public string ResultClassName { get; set; }
        /// The following is the definition of the input parameter "InputObject".
        /// Provides the instance from which the association traversal is to begin.
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true)]
        [Alias(CimBaseCommand.AliasCimInstance)]
        public CimInstance InputObject
                return CimInstance;
                CimInstance = value;
                base.SetParameter(value, nameCimInstance);
        /// Property for internal usage purpose.
        internal CimInstance CimInstance { get; private set; }
        /// The following is the definition of the input parameter "Namespace".
        /// Identifies the Namespace in which the source class, indicated by ClassName,
        /// is registered.
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public string Namespace { get; set; }
        /// The following is the definition of the input parameter "OperationTimeoutSec".
        /// Specifies the operation timeout after which the client operation should be
        /// canceled. The default is the CimSession operation timeout. If this parameter
        /// is specified, then this value takes precedence over the CimSession
        /// OperationTimeout.
        [Alias(AliasOT)]
        public uint OperationTimeoutSec { get; set; }
        /// The following is the definition of the input parameter "ResourceUri".
        /// Define the Resource Uri for which the instances are retrieved.
                return resourceUri;
                this.resourceUri = value;
                base.SetParameter(value, nameResourceUri);
        private Uri resourceUri;
        /// The following is the definition of the input parameter "ComputerName".
        /// Specifies the name of the computer where the source instance is stored and
        /// where the association traversal should begin.
        /// This is an optional parameter and if it is not provided, the default value
        /// will be "localhost".
        [Alias(AliasCN, AliasServerName)]
            ParameterSetName = ComputerSetName)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] ComputerName
                return computerName;
                computerName = value;
                base.SetParameter(value, nameComputerName);
        private string[] computerName;
        /// The following is the definition of the input parameter "CimSession".
        /// Identifies the CimSession which is to be used to retrieve the instances.
            ValueFromPipeline = true,
            ParameterSetName = SessionSetName)]
        public Microsoft.Management.Infrastructure.CimSession[] CimSession
                return cimSession;
                cimSession = value;
                base.SetParameter(value, nameCimSession);
        private Microsoft.Management.Infrastructure.CimSession[] cimSession;
        /// The following is the definition of the input parameter "KeyOnly".
        /// Indicates that only key properties of the retrieved instances should be
        /// returned to the client.
        public SwitchParameter KeyOnly { get; set; }
        #region cmdlet methods
        /// BeginProcessing method.
        protected override void BeginProcessing()
            this.CmdletOperation = new CmdletOperationBase(this);
            this.AtBeginProcess = false;
        /// ProcessRecord method.
        protected override void ProcessRecord()
            base.CheckParameterSet();
            CimGetAssociatedInstance operation = this.GetOperationAgent() ?? this.CreateOperationAgent();
            operation.GetCimAssociatedInstance(this);
            operation.ProcessActions(this.CmdletOperation);
        /// EndProcessing method.
        protected override void EndProcessing()
            CimGetAssociatedInstance operation = this.GetOperationAgent();
            operation?.ProcessRemainActions(this.CmdletOperation);
        /// Get <see cref="CimGetAssociatedInstance"/> object, which is
        /// used to delegate all Get-CimAssociatedInstance operations.
        private CimGetAssociatedInstance GetOperationAgent()
            return this.AsyncOperation as CimGetAssociatedInstance;
        /// Create <see cref="CimGetAssociatedInstance"/> object, which is
        private CimGetAssociatedInstance CreateOperationAgent()
            this.AsyncOperation = new CimGetAssociatedInstance();
            return GetOperationAgent();
        /// Noun of current cmdlet.
        internal const string Noun = @"CimAssociatedInstance";
        #region const string of parameter names
        internal const string nameCimInstance = "InputObject";
        internal const string nameComputerName = "ComputerName";
        internal const string nameCimSession = "CimSession";
        internal const string nameResourceUri = "ResourceUri";
        /// Static parameter definition entries.
        private static readonly Dictionary<string, HashSet<ParameterDefinitionEntry>> parameters = new()
                nameComputerName, new HashSet<ParameterDefinitionEntry> {
                                    new ParameterDefinitionEntry(CimBaseCommand.ComputerSetName, false),
            },
                nameCimSession, new HashSet<ParameterDefinitionEntry> {
                                    new ParameterDefinitionEntry(CimBaseCommand.SessionSetName, true),
                nameCimInstance, new HashSet<ParameterDefinitionEntry> {
                                    new ParameterDefinitionEntry(CimBaseCommand.ComputerSetName, true),
                nameResourceUri, new HashSet<ParameterDefinitionEntry> {
                                    new ParameterDefinitionEntry(CimBaseCommand.SessionSetName, false),
        /// Static parameter set entries.
        private static readonly Dictionary<string, ParameterSetEntry> parameterSets = new()
            {   CimBaseCommand.SessionSetName, new ParameterSetEntry(2, false)     },
            {   CimBaseCommand.ComputerSetName, new ParameterSetEntry(1, true)     },

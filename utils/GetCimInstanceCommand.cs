    /// Returns zero, one or more CIM (dynamic) instances with the properties
    /// specified in the Property parameter, KeysOnly parameter or the Select clause
    /// of the Query parameter.
    [Alias("gcim")]
    [Cmdlet(VerbsCommon.Get, "CimInstance", DefaultParameterSetName = CimBaseCommand.ClassNameComputerSet, HelpUri = "https://go.microsoft.com/fwlink/?LinkId=227961")]
    public class GetCimInstanceCommand : CimBaseCommand
        /// Initializes a new instance of the <see cref="GetCimInstanceCommand"/> class.
        /// Constructor.
        public GetCimInstanceCommand()
            ParameterSetName = CimBaseCommand.CimInstanceSessionSet)]
            ParameterSetName = CimBaseCommand.QuerySessionSet)]
            ParameterSetName = CimBaseCommand.ClassNameSessionSet)]
            ParameterSetName = CimBaseCommand.ResourceUriSessionSet)]
        /// Define the class name for which the instances are retrieved.
        [Parameter(Mandatory = true,
                   ParameterSetName = CimBaseCommand.ClassNameComputerSet)]
        public string ClassName
                return className;
                this.className = value;
                base.SetParameter(value, nameClassName);
        private string className;
                   ParameterSetName = CimBaseCommand.ResourceUriComputerSet)]
            ParameterSetName = CimBaseCommand.CimInstanceComputerSet)]
            ParameterSetName = CimBaseCommand.QueryComputerSet)]
        /// Provides the name of the computer from which to retrieve the instances. The
        /// ComputerName is used to create a temporary CimSession with default parameter
        /// values, which is then used to retrieve the instances.
        [Parameter(ParameterSetName = CimBaseCommand.ClassNameComputerSet)]
        [Parameter(ParameterSetName = CimBaseCommand.ClassNameSessionSet)]
        [Parameter(ParameterSetName = CimBaseCommand.ResourceUriComputerSet)]
        [Parameter(ParameterSetName = CimBaseCommand.ResourceUriSessionSet)]
                return keyOnly;
                keyOnly = value;
                base.SetParameter(value, nameKeyOnly);
        private SwitchParameter keyOnly;
        /// Identifies the Namespace in which the class, indicated by ClassName, is
        /// registered.
        /// Default namespace is 'root\cimv2' if this property is not specified.
        [Parameter(ValueFromPipelineByPropertyName = true,
        public string Namespace
                return nameSpace;
                nameSpace = value;
                base.SetParameter(value, nameNamespace);
        /// <para>The following is the definition of the input parameter "InputObject".
        /// Provides the <see cref="CimInstance"/> that containing the [Key] properties,
        /// based on the key properties to retrieve the <see cref="CimInstance"/>.
        /// User can call New-CimInstance to create the CimInstance with key only
        /// properties, for example:
        /// New-CimInstance -ClassName C -Namespace root\cimv2
        ///  -Property @{CreationClassName="CIM_VirtualComputerSystem";Name="VM3358"}
        ///  -Keys {"CreationClassName", "Name"} -Local
        /// The following is the definition of the input parameter "Query".
        /// Specifies the query string for what instances, and what properties of those
        /// instances, should be retrieve.
        public string Query
                return query;
                query = value;
                base.SetParameter(value, nameQuery);
        private string query;
        /// The following is the definition of the input parameter "QueryDialect".
        /// Specifies the dialect used by the query Engine that interprets the Query
        /// string.
        public string QueryDialect
                return queryDialect;
                queryDialect = value;
                base.SetParameter(value, nameQueryDialect);
        /// The following is the definition of the input parameter "Shallow".
        /// If the switch is set to True, only instance of the class identified by
        /// Namespace + ClassName will be returned. If the switch is not set, instances
        /// of the above class and of all of its descendents will be returned (the
        /// enumeration will cascade the class inheritance hierarchy).
        [Parameter(ParameterSetName = CimBaseCommand.QueryComputerSet)]
        [Parameter(ParameterSetName = CimBaseCommand.QuerySessionSet)]
                return shallow;
                shallow = value;
                base.SetParameter(value, nameShallow);
        private SwitchParameter shallow;
        /// The following is the definition of the input parameter "Filter".
        /// Specifies the where clause of the query.
        public string Filter
                return filter;
                filter = value;
                base.SetParameter(value, nameFilter);
        private string filter;
        /// The following is the definition of the input parameter "Property".
        /// Specifies the selected properties of result instances.
        [Alias("SelectProperties")]
        public string[] Property
                return SelectProperties;
                SelectProperties = value;
                base.SetParameter(value, nameSelectProperties);
        /// Property for internal usage.
        internal string[] SelectProperties { get; private set; }
            this.CheckArgument();
            CimGetInstance cimGetInstance = this.GetOperationAgent() ?? CreateOperationAgent();
            cimGetInstance.GetCimInstance(this);
            cimGetInstance.ProcessActions(this.CmdletOperation);
            CimGetInstance cimGetInstance = this.GetOperationAgent();
            cimGetInstance?.ProcessRemainActions(this.CmdletOperation);
        /// Get <see cref="CimGetInstance"/> object, which is
        /// used to delegate all Get-CimInstance operations, such
        /// as enumerate instances, get instance, query instance.
        private CimGetInstance GetOperationAgent()
            return this.AsyncOperation as CimGetInstance;
        /// Create <see cref="CimGetInstance"/> object, which is
        private CimGetInstance CreateOperationAgent()
            CimGetInstance cimGetInstance = new();
            this.AsyncOperation = cimGetInstance;
            return cimGetInstance;
        /// Check argument value.
        private void CheckArgument()
            switch (this.ParameterSetName)
                    // validate the classname & property
                    this.className = ValidationHelper.ValidateArgumentIsValidName(nameClassName, this.className);
                    this.SelectProperties = ValidationHelper.ValidateArgumentIsValidName(nameSelectProperties, this.SelectProperties);
        internal const string nameClassName = "ClassName";
        internal const string nameFilter = "Filter";
        internal const string nameKeyOnly = "KeyOnly";
        internal const string nameNamespace = "Namespace";
        internal const string nameOperationTimeoutSec = "OperationTimeoutSec";
        internal const string nameQuery = "Query";
        internal const string nameQueryDialect = "QueryDialect";
        internal const string nameSelectProperties = "Property";
        internal const string nameShallow = "Shallow";
                                    new ParameterDefinitionEntry(CimBaseCommand.QuerySessionSet, true),
                                    new ParameterDefinitionEntry(CimBaseCommand.CimInstanceSessionSet, true),
                                    new ParameterDefinitionEntry(CimBaseCommand.ClassNameSessionSet, true),
                                    new ParameterDefinitionEntry(CimBaseCommand.ResourceUriSessionSet, true),
                                    new ParameterDefinitionEntry(CimBaseCommand.ResourceUriComputerSet, true),
                                    new ParameterDefinitionEntry(CimBaseCommand.CimInstanceComputerSet, false),
                                    new ParameterDefinitionEntry(CimBaseCommand.CimInstanceSessionSet, false),
                                    new ParameterDefinitionEntry(CimBaseCommand.QueryComputerSet, false),
                                    new ParameterDefinitionEntry(CimBaseCommand.QuerySessionSet, false),
                nameClassName, new HashSet<ParameterDefinitionEntry> {
                                    new ParameterDefinitionEntry(CimBaseCommand.ClassNameComputerSet, true),
                                    new ParameterDefinitionEntry(CimBaseCommand.ClassNameComputerSet, false),
                                    new ParameterDefinitionEntry(CimBaseCommand.ResourceUriComputerSet, false),
                nameKeyOnly, new HashSet<ParameterDefinitionEntry> {
                                    new ParameterDefinitionEntry(CimBaseCommand.ClassNameSessionSet, false),
                                    new ParameterDefinitionEntry(CimBaseCommand.ResourceUriSessionSet, false),
                nameNamespace, new HashSet<ParameterDefinitionEntry> {
                                    new ParameterDefinitionEntry(CimBaseCommand.CimInstanceComputerSet, true),
                nameQuery, new HashSet<ParameterDefinitionEntry> {
                                    new ParameterDefinitionEntry(CimBaseCommand.QueryComputerSet, true),
                nameQueryDialect, new HashSet<ParameterDefinitionEntry> {
                nameShallow, new HashSet<ParameterDefinitionEntry> {
                nameFilter, new HashSet<ParameterDefinitionEntry> {
                nameSelectProperties, new HashSet<ParameterDefinitionEntry> {
            {   CimBaseCommand.CimInstanceComputerSet, new ParameterSetEntry(1)     },
            {   CimBaseCommand.CimInstanceSessionSet, new ParameterSetEntry(2)     },
            {   CimBaseCommand.ClassNameComputerSet, new ParameterSetEntry(1, true)     },
            {   CimBaseCommand.ClassNameSessionSet, new ParameterSetEntry(2)     },
            {   CimBaseCommand.QueryComputerSet, new ParameterSetEntry(1)     },
            {   CimBaseCommand.ResourceUriSessionSet, new ParameterSetEntry(2)     },
            {   CimBaseCommand.ResourceUriComputerSet, new ParameterSetEntry(1)     },
            {   CimBaseCommand.QuerySessionSet, new ParameterSetEntry(2)     }

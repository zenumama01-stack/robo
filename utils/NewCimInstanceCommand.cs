    /// This Cmdlet creates an instance of a CIM class based on the class
    /// definition, which is an instance factory
    /// If -ClientOnly is not specified, New-CimInstance will create a new instance
    /// on the server, otherwise just create client in-memory instance
    [Alias("ncim")]
    [Cmdlet(VerbsCommon.New, "CimInstance", DefaultParameterSetName = CimBaseCommand.ClassNameComputerSet, SupportsShouldProcess = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkId=227963")]
    public class NewCimInstanceCommand : CimBaseCommand
        /// Initializes a new instance of the <see cref="NewCimInstanceCommand"/> class.
        public NewCimInstanceCommand()
        /// Name of the Class to use to create Instance.
        /// The following is the definition of the input parameter "Key".
        /// Enables the user to specify list of key property name.
        /// Example: -Key {"K1", "K2"}
        public string[] Key
                return key;
                key = value;
                base.SetParameter(value, nameKey);
        private string[] key;
        /// The CimClass is used to create Instance.
        /// Enables the user to specify instances with specific property values.
        /// Example: -Property @{P1="Value1";P2="Value2"}
        [Alias("Arguments")]
        public IDictionary Property { get; set; }
        /// Namespace used to look for the classes under to store the instances.
        /// Default namespace is 'root\cimv2'
        /// Operation Timeout of the cmdlet in seconds. Overrides the value in the Cim
        /// Session.
        /// Identifies the CimSession which is to be used to create the instances.
        /// Provides the name of the computer from which to create the instances.
        /// The following is the definition of the input parameter "ClientOnly".
        /// Indicates to create a client only ciminstance object, NOT on the server.
        [Alias("Local")]
            ParameterSetName = CimBaseCommand.CimClassSessionSet)]
        public SwitchParameter ClientOnly
                return clientOnly;
                clientOnly = value;
                base.SetParameter(value, nameClientOnly);
        private SwitchParameter clientOnly;
            if (this.ClientOnly)
                string conflictParameterName = null;
                if (this.ComputerName != null)
                    conflictParameterName = @"ComputerName";
                else if (this.CimSession != null)
                    conflictParameterName = @"CimSession";
                if (conflictParameterName != null)
                    ThrowConflictParameterWasSet(@"New-CimInstance", conflictParameterName, @"ClientOnly");
            CimNewCimInstance cimNewCimInstance = this.GetOperationAgent() ?? CreateOperationAgent();
            cimNewCimInstance.NewCimInstance(this);
            cimNewCimInstance.ProcessActions(this.CmdletOperation);
            CimNewCimInstance cimNewCimInstance = this.GetOperationAgent();
            cimNewCimInstance?.ProcessRemainActions(this.CmdletOperation);
        private CimNewCimInstance GetOperationAgent()
            return this.AsyncOperation as CimNewCimInstance;
        /// Create <see cref="CimNewCimInstance"/> object, which is
        private CimNewCimInstance CreateOperationAgent()
            CimNewCimInstance cimNewCimInstance = new();
            this.AsyncOperation = cimNewCimInstance;
            return cimNewCimInstance;
        internal const string nameKey = "Key";
        internal const string nameProperty = "Property";
        internal const string nameClientOnly = "ClientOnly";
                nameKey, new HashSet<ParameterDefinitionEntry> {
                nameClientOnly, new HashSet<ParameterDefinitionEntry> {
            {   CimBaseCommand.CimClassSessionSet, new ParameterSetEntry(2)     },
            {   CimBaseCommand.CimClassComputerSet, new ParameterSetEntry(1)     },

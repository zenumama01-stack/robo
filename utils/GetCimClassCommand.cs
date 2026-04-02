    /// Enables the user to enumerate the list of CIM Classes under a specific
    /// Namespace. If no list of classes is given, the Cmdlet returns all
    /// classes in the given namespace.
    /// NOTES: The class instance contains the Namespace properties
    /// Should the class remember what Session it came from? No.
    [Alias("gcls")]
    [Cmdlet(VerbsCommon.Get, GetCimClassCommand.Noun, DefaultParameterSetName = ComputerSetName, HelpUri = "https://go.microsoft.com/fwlink/?LinkId=227959")]
    [OutputType(typeof(CimClass))]
    public class GetCimClassCommand : CimBaseCommand
        /// Initializes a new instance of the <see cref="GetCimClassCommand"/> class.
        public GetCimClassCommand()
        /// Gets or sets flag to retrieve a localized data for WMI class.
        public SwitchParameter Amended { get; set; }
        /// Specifies the Namespace under which to look for the specified class name.
        /// If no class name is specified, the cmdlet should return all classes under
        /// the specified Namespace.
        /// Default namespace is root\cimv2
        /// Enables the user to specify the operation timeout in Seconds. This value
        /// overwrites the value specified by the CimSession Operation timeout.
        /// The following is the definition of the input parameter "Session".
        /// Uses a CimSession context.
        public CimSession[] CimSession
        private CimSession[] cimSession;
        /// <para>The following is the definition of the input parameter "ComputerName".
        /// Provides the name of the computer from which to retrieve the <see cref="CimClass"/>
        /// If no ComputerName is specified the default value is "localhost"
            ValueFromPipelineByPropertyName = true,
        public string MethodName { get; set; }
        public string PropertyName { get; set; }
        public string QualifierName { get; set; }
            CimGetCimClass cimGetCimClass = this.GetOperationAgent() ?? CreateOperationAgent();
            cimGetCimClass.GetCimClass(this);
            cimGetCimClass.ProcessActions(this.CmdletOperation);
            CimGetCimClass cimGetCimClass = this.GetOperationAgent();
            cimGetCimClass?.ProcessRemainActions(this.CmdletOperation);
        /// Get <see cref="CimNewCimInstance"/> object, which is
        /// used to delegate all New-CimInstance operations.
        private CimGetCimClass GetOperationAgent()
            return this.AsyncOperation as CimGetCimClass;
        /// Create <see cref="CimGetCimClass"/> object, which is
        /// used to delegate all Get-CimClass operations.
        private CimGetCimClass CreateOperationAgent()
            CimGetCimClass cimGetCimClass = new();
            this.AsyncOperation = cimGetCimClass;
            return cimGetCimClass;
        internal const string Noun = @"CimClass";
            {   CimBaseCommand.SessionSetName, new ParameterSetEntry(1)     },
            {   CimBaseCommand.ComputerSetName, new ParameterSetEntry(0, true)     },

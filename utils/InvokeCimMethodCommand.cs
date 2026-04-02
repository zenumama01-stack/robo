    /// This cmdlet enables the user to invoke a static method on a CIM class using
    /// the arguments passed as a list of name value pair dictionary.
    [Alias("icim")]
    [Cmdlet(
        "Invoke",
        "CimMethod",
        SupportsShouldProcess = true,
        DefaultParameterSetName = CimBaseCommand.ClassNameComputerSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkId=227965")]
    public class InvokeCimMethodCommand : CimBaseCommand
        /// Initializes a new instance of the <see cref="InvokeCimMethodCommand"/> class.
        public InvokeCimMethodCommand()
        /// Specifies the Class Name, on which to invoke static method.
        [Alias("Class")]
                className = value;
        /// The following is the definition of the input parameter "CimClass".
        /// Specifies the <see cref="CimClass"/> object, on which to invoke static method.
            ParameterSetName = CimClassComputerSet)]
            ParameterSetName = CimClassSessionSet)]
        public CimClass CimClass
                return cimClass;
                cimClass = value;
                base.SetParameter(value, nameCimClass);
        private CimClass cimClass;
        /// Takes a CimInstance object retrieved by a Get-CimInstance call.
        /// Invoke the method against the given instance.
        /// Provides the name of the computer from which to invoke the method. The
            ParameterSetName = CimBaseCommand.CimClassComputerSet)]
        /// The following is the definition of the input parameter "Arguments".
        /// Specifies the parameter arguments for the static method using a name value
        /// pair.
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true)]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IDictionary Arguments { get; set; }
        /// The following is the definition of the input parameter "MethodName".
        /// Name of the Static Method to use.
                   Position = 2,
        [Alias("Name")]
        public string MethodName
                return methodName;
                methodName = value;
                base.SetParameter(value, nameMethodName);
        private string methodName;
        /// Specifies the NameSpace in which the class or instance lives under.
            CimInvokeCimMethod cimInvokeMethod = this.GetOperationAgent() ?? CreateOperationAgent();
            this.CmdletOperation = new CmdletOperationInvokeCimMethod(this, cimInvokeMethod);
            CimInvokeCimMethod cimInvokeMethod = this.GetOperationAgent();
            cimInvokeMethod.InvokeCimMethod(this);
            cimInvokeMethod.ProcessActions(this.CmdletOperation);
            cimInvokeMethod?.ProcessRemainActions(this.CmdletOperation);
        /// Get <see cref="CimInvokeCimMethod"/> object, which is
        /// used to delegate all Invoke-CimMethod operations.
        private CimInvokeCimMethod GetOperationAgent()
            return this.AsyncOperation as CimInvokeCimMethod;
        /// Create <see cref="CimInvokeCimMethod"/> object, which is
        private CimInvokeCimMethod CreateOperationAgent()
            CimInvokeCimMethod cimInvokeMethod = new();
            this.AsyncOperation = cimInvokeMethod;
            return cimInvokeMethod;
                    // validate the classname
        internal const string nameCimClass = "CimClass";
        internal const string nameArguments = "Arguments";
        internal const string nameMethodName = "MethodName";
                nameCimClass, new HashSet<ParameterDefinitionEntry> {
                                    new ParameterDefinitionEntry(CimBaseCommand.CimClassComputerSet, true),
                                    new ParameterDefinitionEntry(CimBaseCommand.CimClassSessionSet, true),
                                    new ParameterDefinitionEntry(CimBaseCommand.CimClassComputerSet, false),
                nameMethodName, new HashSet<ParameterDefinitionEntry> {
            {   CimBaseCommand.ClassNameComputerSet, new ParameterSetEntry(2, true)     },
            {   CimBaseCommand.ResourceUriSessionSet, new ParameterSetEntry(3)     },
            {   CimBaseCommand.ResourceUriComputerSet, new ParameterSetEntry(2)     },
            {   CimBaseCommand.ClassNameSessionSet, new ParameterSetEntry(3)     },
            {   CimBaseCommand.QueryComputerSet, new ParameterSetEntry(2)     },
            {   CimBaseCommand.QuerySessionSet, new ParameterSetEntry(3)     },
            {   CimBaseCommand.CimInstanceComputerSet, new ParameterSetEntry(2)     },
            {   CimBaseCommand.CimInstanceSessionSet, new ParameterSetEntry(3)     },
            {   CimBaseCommand.CimClassComputerSet, new ParameterSetEntry(2)     },
            {   CimBaseCommand.CimClassSessionSet, new ParameterSetEntry(3)     },

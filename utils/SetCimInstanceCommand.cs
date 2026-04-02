    /// Enables the user to Set properties and keys on a specific <see cref="CimInstance"/>
    /// CimInstance must have values of all [KEY] properties.
    [Alias("scim")]
        VerbsCommon.Set,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkId=227962")]
    public class SetCimInstanceCommand : CimBaseCommand
        /// Initializes a new instance of the <see cref="SetCimInstanceCommand"/> class.
        public SetCimInstanceCommand()
        /// CIM session used to set the CIM Instance.
        /// Used to get a CimInstance using Get-CimInstance | Set-CimInstance.
        /// The following is the definition of the input parameter "Property",
        /// defines the value to be changed.
        /// The key properties will be ignored. Any invalid property will cause
        /// termination of the cmdlet execution.
        public IDictionary Property
                return property;
                property = value;
                base.SetParameter(value, nameProperty);
        private IDictionary property;
        /// The following is the definition of the input parameter "PassThru",
        /// indicate whether Set-CimInstance should output modified result instance or not.
        /// True indicates output the result instance, otherwise output nothing as by default
        /// behavior.
        [ValidateNotNull]
        public SwitchParameter PassThru { get; set; }
            CimSetCimInstance cimSetCimInstance = this.GetOperationAgent() ?? CreateOperationAgent();
            this.CmdletOperation = new CmdletOperationSetCimInstance(this, cimSetCimInstance);
            CimSetCimInstance cimSetCimInstance = this.GetOperationAgent();
            cimSetCimInstance.SetCimInstance(this);
            cimSetCimInstance.ProcessActions(this.CmdletOperation);
            cimSetCimInstance?.ProcessRemainActions(this.CmdletOperation);
        /// Get <see cref="CimSetCimInstance"/> object, which is
        /// used to delegate all Set-CimInstance operations.
        private CimSetCimInstance GetOperationAgent()
            return this.AsyncOperation as CimSetCimInstance;
        /// Create <see cref="CimSetCimInstance"/> object, which is
        private CimSetCimInstance CreateOperationAgent()
            CimSetCimInstance cimSetCimInstance = new();
            this.AsyncOperation = cimSetCimInstance;
            return cimSetCimInstance;
                nameProperty, new HashSet<ParameterDefinitionEntry> {

    /// Enables the user to remove a CimInstance.
    [Alias("rcim")]
        VerbsCommon.Remove,
        "CimInstance",
        DefaultParameterSetName = CimBaseCommand.CimInstanceComputerSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkId=227964")]
    public class RemoveCimInstanceCommand : CimBaseCommand
        /// Initializes a new instance of the <see cref="RemoveCimInstanceCommand"/> class.
        public RemoveCimInstanceCommand()
        /// CIM session used to remove the CIM Instance.
        /// The Namespace used to look for the Class instances under.
        /// Used to set the invocation operation time out. This value overrides the
        /// CimSession operation timeout.
        /// Used to get a CimInstance using Get-CimInstance | Remove-CimInstance.
                return querydialect;
                querydialect = value;
        private string querydialect;
            CimRemoveCimInstance cimRemoveInstance = this.GetOperationAgent() ?? CreateOperationAgent();
            this.CmdletOperation = new CmdletOperationRemoveCimInstance(this, cimRemoveInstance);
            CimRemoveCimInstance cimRemoveInstance = this.GetOperationAgent();
            cimRemoveInstance.RemoveCimInstance(this);
            cimRemoveInstance.ProcessActions(this.CmdletOperation);
            cimRemoveInstance?.ProcessRemainActions(this.CmdletOperation);
        /// Get <see cref="CimRemoveCimInstance"/> object, which is
        /// used to delegate all Remove-CimInstance operations.
        private CimRemoveCimInstance GetOperationAgent()
            return this.AsyncOperation as CimRemoveCimInstance;
        /// Create <see cref="CimRemoveCimInstance"/> object, which is
        private CimRemoveCimInstance CreateOperationAgent()
            CimRemoveCimInstance cimRemoveInstance = new();
            this.AsyncOperation = cimRemoveInstance;
            return cimRemoveInstance;
            {   CimBaseCommand.CimInstanceComputerSet, new ParameterSetEntry(1, true)     },
            {   CimBaseCommand.QuerySessionSet, new ParameterSetEntry(2)     },

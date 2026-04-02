/******************************************************************************
 * warning 28750: Banned usage of lstrlen and its variants: lstrlenW is a
 * banned API for improved error handling purposes.
 *****************************************************************************/
    /// This Cmdlet allows the to remove, or terminate, one or more CimSession(s).
    [Alias("rcms")]
    [Cmdlet(VerbsCommon.Remove, "CimSession",
             DefaultParameterSetName = CimSessionSet,
             HelpUri = "https://go.microsoft.com/fwlink/?LinkId=227968")]
    public sealed class RemoveCimSessionCommand : CimBaseCommand
        /// Initializes a new instance of the <see cref="RemoveCimSessionCommand"/> class.
        public RemoveCimSessionCommand()
        /// Specifies one or more CimSession object to be removed from the local PS
        /// session/runspace.
            ParameterSetName = CimSessionSet)]
                return cimsession;
                cimsession = value;
        private CimSession[] cimsession;
        /// Specified one or more computer names for which all CimSession(s)
        /// (connections) should be removed (terminated).</para>
        /// <para>This is the only optional parameter. If no value for this parameter is
        /// provided, all CimSession(s) are terminated.</para>
        /// Specifies the friendly Id(s) of the CimSession(s) that should be removed
        /// (terminated).
        /// The following is the definition of the input parameter "InstanceId".
        /// Specifies one or more automatically generated InstanceId(s) (GUIDs) of the
        /// CimSession(s) that should be removed (terminated).
        /// Specifies one or more of friendly Names of the CimSession(s) that should be
        /// removed (terminated).
            this.cimRemoveSession = new CimRemoveSession();
            this.cimRemoveSession.RemoveCimSession(this);
        /// <see cref="CimRemoveSession"/> object used to remove the session from
        /// session cache.
        private CimRemoveSession cimRemoveSession;
                                    new ParameterDefinitionEntry(CimBaseCommand.CimSessionSet, true),
                                    new ParameterDefinitionEntry(CimBaseCommand.ComputerNameSet, true),
            {   CimBaseCommand.CimSessionSet, new ParameterSetEntry(1, true)     },
            {   CimBaseCommand.ComputerNameSet, new ParameterSetEntry(1)     },

    /// Wrapper of Cmdlet, forward the operation to Cmdlet directly.
    /// This is for unit test purpose, unit test can derive from this class,
    /// to hook up all of the cmdlet related operation and verify the correctness.
    internal class CmdletOperationBase
        /// Wrap the Cmdlet object.
        private readonly Cmdlet cmdlet;
        /// Wrap the Cmdlet methods, for testing purpose.
        /// Test binary can define a child class of CmdletOperationBase.
        /// While Execute method of <seealso cref="CimBaseAction"/> accept the
        /// object of CmdletOperationBase as parameter.
        #region CMDLET methods
        public virtual bool ShouldContinue(string query, string caption)
            return cmdlet.ShouldContinue(query, caption);
        public virtual bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll)
            return cmdlet.ShouldContinue(query, caption, ref yesToAll, ref noToAll);
        public virtual bool ShouldProcess(string target)
            return cmdlet.ShouldProcess(target);
        public virtual bool ShouldProcess(string target, string action)
            return cmdlet.ShouldProcess(target, action);
        public virtual bool ShouldProcess(string verboseDescription, string verboseWarning, string caption)
            return cmdlet.ShouldProcess(verboseDescription, verboseWarning, caption);
        public virtual bool ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason)
            return cmdlet.ShouldProcess(verboseDescription, verboseWarning, caption, out shouldProcessReason);
        public virtual void ThrowTerminatingError(ErrorRecord errorRecord)
            cmdlet.ThrowTerminatingError(errorRecord);
        public virtual void WriteCommandDetail(string text)
            cmdlet.WriteCommandDetail(text);
        public virtual void WriteDebug(string text)
            cmdlet.WriteDebug(text);
        public virtual void WriteError(ErrorRecord errorRecord)
            cmdlet.WriteError(errorRecord);
        public virtual void WriteObject(object sendToPipeline, XOperationContextBase context)
            cmdlet.WriteObject(sendToPipeline);
        public virtual void WriteObject(object sendToPipeline, bool enumerateCollection, XOperationContextBase context)
            cmdlet.WriteObject(sendToPipeline, enumerateCollection);
        public virtual void WriteProgress(ProgressRecord progressRecord)
            cmdlet.WriteProgress(progressRecord);
        public virtual void WriteVerbose(string text)
            cmdlet.WriteVerbose(text);
        public virtual void WriteWarning(string text)
            cmdlet.WriteWarning(text);
        /// Initializes a new instance of the <see cref="CmdletOperationBase"/> class.
        public CmdletOperationBase(Cmdlet cmdlet)
            this.cmdlet = cmdlet;
    #region Class CmdletOperationRemoveCimInstance
    /// Wrapper of Cmdlet, override WriteObject function call since
    /// we need to remove <see cref="CimInstance"/>.
    internal class CmdletOperationRemoveCimInstance : CmdletOperationBase
        /// Initializes a new instance of the <see cref="CmdletOperationRemoveCimInstance"/> class.
        public CmdletOperationRemoveCimInstance(Cmdlet cmdlet,
            CimRemoveCimInstance cimRemoveCimInstance)
            : base(cmdlet)
            ValidationHelper.ValidateNoNullArgument(cimRemoveCimInstance, cimRemoveCimInstanceParameterName);
            this.removeCimInstance = cimRemoveCimInstance;
        /// Object here need to be removed if it is CimInstance
        /// <param name="sendToPipeline"></param>
        public override void WriteObject(object sendToPipeline, XOperationContextBase context)
            if (sendToPipeline is CimInstance)
                DebugHelper.WriteLog(">>>>CmdletOperationRemoveCimInstance::WriteObject", 4);
                this.removeCimInstance.RemoveCimInstance(sendToPipeline as CimInstance, context, this);
                base.WriteObject(sendToPipeline, context);
        public override void WriteObject(object sendToPipeline, bool enumerateCollection, XOperationContextBase context)
                this.WriteObject(sendToPipeline, context);
                base.WriteObject(sendToPipeline, enumerateCollection, context);
        private readonly CimRemoveCimInstance removeCimInstance;
        private const string cimRemoveCimInstanceParameterName = @"cimRemoveCimInstance";
    #region Class CmdletOperationSetCimInstance
    /// we need to set <see cref="CimInstance"/>.
    internal class CmdletOperationSetCimInstance : CmdletOperationBase
        /// Initializes a new instance of the <see cref="CmdletOperationSetCimInstance"/> class.
        public CmdletOperationSetCimInstance(Cmdlet cmdlet,
            CimSetCimInstance theCimSetCimInstance)
            ValidationHelper.ValidateNoNullArgument(theCimSetCimInstance, theCimSetCimInstanceParameterName);
            this.setCimInstance = theCimSetCimInstance;
                if (context is CimSetCimInstanceContext setContext)
                    if (string.Equals(setContext.ParameterSetName, CimBaseCommand.QueryComputerSet, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(setContext.ParameterSetName, CimBaseCommand.QuerySessionSet, StringComparison.OrdinalIgnoreCase))
                        this.setCimInstance.SetCimInstance(sendToPipeline as CimInstance, setContext, this);
                        DebugHelper.WriteLog("Write the cimInstance to pipeline since this CimInstance is returned by SetCimInstance.", 4);
                    DebugHelper.WriteLog("Assert. CimSetCimInstance::SetCimInstance has NULL CimSetCimInstanceContext", 4);
        private readonly CimSetCimInstance setCimInstance;
        private const string theCimSetCimInstanceParameterName = @"theCimSetCimInstance";
    #region Class CmdletOperationInvokeCimMethod
    /// we need to invoke cim method.
    internal class CmdletOperationInvokeCimMethod : CmdletOperationBase
        /// Initializes a new instance of the <see cref="CmdletOperationInvokeCimMethod"/> class.
        public CmdletOperationInvokeCimMethod(Cmdlet cmdlet,
            CimInvokeCimMethod theCimInvokeCimMethod)
            ValidationHelper.ValidateNoNullArgument(theCimInvokeCimMethod, theCimInvokeCimMethodParameterName);
            this.cimInvokeCimMethod = theCimInvokeCimMethod;
                this.cimInvokeCimMethod.InvokeCimMethodOnCimInstance(sendToPipeline as CimInstance, context, this);
        private readonly CimInvokeCimMethod cimInvokeCimMethod;
        private const string theCimInvokeCimMethodParameterName = @"theCimInvokeCimMethod";
    #region Class CmdletOperationTestCimSession
    /// we need to add cim session to global cache.
    internal class CmdletOperationTestCimSession : CmdletOperationBase
        /// Initializes a new instance of the <see cref="CmdletOperationTestCimSession"/> class.
        public CmdletOperationTestCimSession(Cmdlet cmdlet,
            CimNewSession theCimNewSession)
            ValidationHelper.ValidateNoNullArgument(theCimNewSession, theCimNewSessionParameterName);
            this.cimNewSession = theCimNewSession;
        /// Add session object to cache
            if (sendToPipeline is CimSession)
                DebugHelper.WriteLog("Call CimNewSession::AddSessionToCache", 1);
                this.cimNewSession.AddSessionToCache(sendToPipeline as CimSession, context, this);
            else if (sendToPipeline is PSObject)
                DebugHelper.WriteLog("Write PSObject to pipeline", 1);
                // NOTES: May need to output for warning message/verbose message
                DebugHelper.WriteLog("Ignore other type object {0}", 1, sendToPipeline);
        private readonly CimNewSession cimNewSession;
        private const string theCimNewSessionParameterName = @"theCimNewSession";

    /// CIM-specific ObjectModelWrapper.
    public sealed class CimCmdletAdapter :
        SessionBasedCmdletAdapter<CimInstance, CimSession>,
        IDynamicParameters
        #region Special method and parameter names
        internal const string CreateInstance_MethodName = "cim:CreateInstance";
        internal const string ModifyInstance_MethodName = "cim:ModifyInstance";
        internal const string DeleteInstance_MethodName = "cim:DeleteInstance";
        #region Changing Session parameter to CimSession
        /// CimSession to operate on.
        [Alias("Session")]
                return base.Session;
                base.Session = value;
        public override int ThrottleLimit
                if (_throttleLimitIsSetExplicitly)
                    return base.ThrottleLimit;
                return this.CmdletDefinitionContext.DefaultThrottleLimit;
                base.ThrottleLimit = value;
                _throttleLimitIsSetExplicitly = true;
        private bool _throttleLimitIsSetExplicitly;
        #region ObjectModelWrapper overrides
        /// Creates a query builder for CIM OM.
        /// <returns>Query builder for CIM OM.</returns>
        public override QueryBuilder GetQueryBuilder()
            return new CimQuery();
        internal CimCmdletInvocationContext CmdletInvocationContext
                return _cmdletInvocationContext ??= new CimCmdletInvocationContext(
                    this.CmdletDefinitionContext,
                    this.Cmdlet,
                    this.GetDynamicNamespace());
        private CimCmdletInvocationContext _cmdletInvocationContext;
        internal CimCmdletDefinitionContext CmdletDefinitionContext
                _cmdletDefinitionContext ??= new CimCmdletDefinitionContext(
                    this.ClassName,
                    this.ClassVersion,
                    this.ModuleVersion,
                    this.Cmdlet.CommandInfo.CommandMetadata.SupportsShouldProcess,
                    this.PrivateData);
                return _cmdletDefinitionContext;
        private CimCmdletDefinitionContext _cmdletDefinitionContext;
        internal InvocationInfo CmdletInvocationInfo
        #endregion ObjectModelWrapper overrides
        #region SessionBasedCmdletAdapter overrides
        private static long s_jobNumber;
        /// Returns a new job name to use for the parent job that handles throttling of the child jobs that actually perform querying and method invocation.
        /// <returns>Job name.</returns>
        protected override string GenerateParentJobName()
            return "CimJob" + Interlocked.Increment(ref CimCmdletAdapter.s_jobNumber).ToString(CultureInfo.InvariantCulture);
        protected override CimSession DefaultSession
                return this.CmdletInvocationContext.GetDefaultCimSession();
        private CimJobContext CreateJobContext(CimSession session, object targetObject)
            return new CimJobContext(
                this.CmdletInvocationContext,
                session,
                targetObject);
        /// <param name="baseQuery">Query parameters.</param>
        /// <returns><see cref="System.Management.Automation.Job"/> object that performs a query against the wrapped object model.</returns>
        internal override StartableJob CreateQueryJob(CimSession session, QueryBuilder baseQuery)
            if (baseQuery is not CimQuery query)
                throw new ArgumentNullException(nameof(baseQuery));
            TerminatingErrorTracker tracker = TerminatingErrorTracker.GetTracker(this.CmdletInvocationInfo, isStaticCmdlet: false);
            if (tracker.IsSessionTerminated(session))
            if (!IsSupportedSession(session, tracker))
            CimJobContext jobContext = this.CreateJobContext(session, targetObject: null);
            StartableJob queryJob = query.GetQueryJob(jobContext);
        internal override StartableJob CreateInstanceMethodInvocationJob(CimSession session, CimInstance objectInstance, MethodInvocationInfo methodInvocationInfo, bool passThru)
            CimJobContext jobContext = this.CreateJobContext(session, objectInstance);
            Dbg.Assert(objectInstance != null, "Caller should verify objectInstance != null");
            StartableJob result;
            if (methodInvocationInfo.MethodName.Equals(CimCmdletAdapter.DeleteInstance_MethodName, StringComparison.OrdinalIgnoreCase))
                result = new DeleteInstanceJob(
                    methodInvocationInfo);
            else if (methodInvocationInfo.MethodName.Equals(CimCmdletAdapter.ModifyInstance_MethodName, StringComparison.OrdinalIgnoreCase))
                result = new ModifyInstanceJob(
                result = new InstanceMethodInvocationJob(
        private bool IsSupportedSession(CimSession cimSession, TerminatingErrorTracker terminatingErrorTracker)
            bool confirmSwitchSpecified = this.CmdletInvocationInfo.BoundParameters.ContainsKey("Confirm");
            bool whatIfSwitchSpecified = this.CmdletInvocationInfo.BoundParameters.ContainsKey("WhatIf");
            if (confirmSwitchSpecified || whatIfSwitchSpecified)
                if (cimSession.ComputerName != null && (!cimSession.ComputerName.Equals("localhost", StringComparison.OrdinalIgnoreCase)))
                    PSPropertyInfo protocolProperty = PSObject.AsPSObject(cimSession).Properties["Protocol"];
                    if ((protocolProperty != null) &&
                        (protocolProperty.Value != null) &&
                        (protocolProperty.Value.ToString().Equals("DCOM", StringComparison.OrdinalIgnoreCase)))
                        bool sessionWasAlreadyTerminated;
                        terminatingErrorTracker.MarkSessionAsTerminated(cimSession, out sessionWasAlreadyTerminated);
                        if (!sessionWasAlreadyTerminated)
                            string nameOfUnsupportedSwitch;
                            if (confirmSwitchSpecified)
                                nameOfUnsupportedSwitch = "-Confirm";
                                Dbg.Assert(whatIfSwitchSpecified, "Confirm and WhatIf are the only detected settings");
                                nameOfUnsupportedSwitch = "-WhatIf";
                                CmdletizationResources.CimCmdletAdapter_RemoteDcomDoesntSupportExtendedSemantics,
                                cimSession.ComputerName,
                                nameOfUnsupportedSwitch);
                            Exception exception = new NotSupportedException(errorMessage);
                            ErrorRecord errorRecord = new(
                                exception,
                                "NoExtendedSemanticsSupportInRemoteDcomProtocol",
                                ErrorCategory.NotImplemented,
                                cimSession);
                            this.Cmdlet.WriteError(errorRecord);
        /// Creates a <see cref="System.Management.Automation.Job"/> object that invokes a static method
        /// (of the class named by <see cref="Microsoft.PowerShell.Cmdletization.CmdletAdapter&lt;TObjectInstance&gt;.ClassName"/>)
        /// in the wrapped object model.
        internal override StartableJob CreateStaticMethodInvocationJob(CimSession session, MethodInvocationInfo methodInvocationInfo)
            TerminatingErrorTracker tracker = TerminatingErrorTracker.GetTracker(this.CmdletInvocationInfo, isStaticCmdlet: true);
            if (methodInvocationInfo.MethodName.Equals(CimCmdletAdapter.CreateInstance_MethodName, StringComparison.OrdinalIgnoreCase))
                result = new CreateInstanceJob(
                result = new StaticMethodInvocationJob(
        #endregion SessionBasedCmdletAdapter overrides
        #region Session affinity management
        private static readonly ConditionalWeakTable<CimInstance, CimSession> s_cimInstanceToSessionOfOrigin = new();
        internal static void AssociateSessionOfOriginWithInstance(CimInstance cimInstance, CimSession sessionOfOrigin)
            // GetValue adds value to the table, if the key is not present in the table
            s_cimInstanceToSessionOfOrigin.GetValue(cimInstance, _ => sessionOfOrigin);
        internal static CimSession GetSessionOfOriginFromCimInstance(CimInstance instance)
            CimSession result = null;
            if (instance != null)
                s_cimInstanceToSessionOfOrigin.TryGetValue(instance, out result);
        internal override CimSession GetSessionOfOriginFromInstance(CimInstance instance)
            return GetSessionOfOriginFromCimInstance(instance);
        #region Handling of dynamic parameters
        private RuntimeDefinedParameterDictionary _dynamicParameters;
        private const string CimNamespaceParameter = "CimNamespace";
        private string GetDynamicNamespace()
            if (_dynamicParameters == null)
            RuntimeDefinedParameter runtimeParameter;
            if (!_dynamicParameters.TryGetValue(CimNamespaceParameter, out runtimeParameter))
            return runtimeParameter.Value as string;
        object IDynamicParameters.GetDynamicParameters()
                _dynamicParameters = new RuntimeDefinedParameterDictionary();
                if (this.CmdletDefinitionContext.ExposeCimNamespaceParameter)
                    Collection<Attribute> namespaceAttributes = new();
                    namespaceAttributes.Add(new ValidateNotNullOrEmptyAttribute());
                    namespaceAttributes.Add(new ParameterAttribute());
                    RuntimeDefinedParameter namespaceRuntimeParameter = new(
                        CimNamespaceParameter,
                        typeof(string),
                        namespaceAttributes);
                    _dynamicParameters.Add(CimNamespaceParameter, namespaceRuntimeParameter);
            return _dynamicParameters;

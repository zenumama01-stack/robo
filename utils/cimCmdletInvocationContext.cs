    internal sealed class CimCmdletInvocationContext
        internal CimCmdletInvocationContext(
            CimCmdletDefinitionContext cmdletDefinitionContext,
            Cmdlet cmdlet,
            string namespaceOverride)
            this.CmdletDefinitionContext = cmdletDefinitionContext;
            this.NamespaceOverride = namespaceOverride;
            // Cmdlet might have a shorter lifespan than CimCmdletInvocationContext
            // - we need to extract information out of Cmdlet to extend information's lifespan
            this.CmdletInvocationInfo = cmdlet.MyInvocation;
            var runtime = cmdlet.CommandRuntime as MshCommandRuntime;
            Dbg.Assert(runtime != null, "CIM cmdlets should only be run from within PS runtime");
            this.DebugActionPreference = runtime.DebugPreference;
            WarnAboutUnsupportedActionPreferences(
                cmdlet,
                this.DebugActionPreference,
                "Debug",
                inquireMessageGetter: () => CmdletizationResources.CimCmdletAdapter_DebugInquire,
                stopMessageGetter: () => string.Empty);
            this.WarningActionPreference = runtime.WarningPreference;
                this.WarningActionPreference,
                "WarningAction",
                inquireMessageGetter: () => CmdletizationResources.CimCmdletAdapter_WarningInquire,
                stopMessageGetter: () => CmdletizationResources.CimCmdletAdapter_WarningStop);
            this.VerboseActionPreference = runtime.VerbosePreference;
            this.ErrorActionPreference = runtime.ErrorAction;
            this.ShouldProcessOptimization = runtime.CalculatePossibleShouldProcessOptimization();
        private static void WarnAboutUnsupportedActionPreferences(
            ActionPreference effectiveActionPreference,
            string nameOfCommandLineParameter,
            Func<string> inquireMessageGetter,
            Func<string> stopMessageGetter)
            string message;
            switch (effectiveActionPreference)
                    message = stopMessageGetter();
                    message = inquireMessageGetter();
                    return; // we can handle everything that is not Stop or Inquire
            bool actionPreferenceComesFromCommandLineParameter = cmdlet.MyInvocation.BoundParameters.ContainsKey(nameOfCommandLineParameter);
            if (actionPreferenceComesFromCommandLineParameter)
                Exception exception = new ArgumentException(message);
                ErrorRecord errorRecord = new(exception, "ActionPreferenceNotSupportedByCimCmdletAdapter", ErrorCategory.NotImplemented, null);
        public CimCmdletDefinitionContext CmdletDefinitionContext { get; }
        public InvocationInfo CmdletInvocationInfo { get; }
        public MshCommandRuntime.ShouldProcessPossibleOptimization ShouldProcessOptimization { get; }
        public ActionPreference ErrorActionPreference { get; }
        public ActionPreference WarningActionPreference { get; }
        public ActionPreference VerboseActionPreference { get; }
        public ActionPreference DebugActionPreference { get; }
        public string NamespaceOverride { get; }
        public bool IsRunningInBackground
                return this.CmdletInvocationInfo.BoundParameters.ContainsKey("AsJob");
        public bool ShowComputerName
                return this.CmdletInvocationInfo.BoundParameters.ContainsKey("CimSession");
        private readonly Lazy<CimSession> _defaultCimSession = new(CreateDefaultCimSession);
        private static CimSession CreateDefaultCimSession()
            return CimSession.Create(null);
        public CimSession GetDefaultCimSession()
            return _defaultCimSession.Value;

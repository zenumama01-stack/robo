using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Metrics;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
namespace Microsoft.PowerShell.Telemetry
    /// The category of telemetry.
    internal enum TelemetryType
        /// Telemetry of the application type (cmdlet, script, etc).
        ApplicationType,
        /// Send telemetry when we load a module, only module names in the s_knownModules list
        /// will be reported, otherwise it will be "anonymous".
        ModuleLoad,
        /// Send telemetry when we load a module using Windows compatibility feature, only module names in the s_knownModules list
        WinCompatModuleLoad,
        /// Send telemetry for experimental module feature deactivation.
        /// All experimental engine features will be have telemetry.
        ExperimentalEngineFeatureDeactivation,
        /// Send telemetry for experimental module feature activation.
        ExperimentalEngineFeatureActivation,
        /// Send telemetry for an experimental feature when use.
        ExperimentalFeatureUse,
        /// Experimental module features will send telemetry based on the module it is in.
        /// If we send telemetry for the module, we will also do so for any experimental feature
        /// in that module.
        ExperimentalModuleFeatureDeactivation,
        ExperimentalModuleFeatureActivation,
        /// Send telemetry for each PowerShell.Create API.
        PowerShellCreate,
        /// Remote session creation.
        RemoteSessionOpen,
        /// Send telemetry for a stable feature when used.
        /// By making a distinction between this and experimental feature use, it will make
        /// queries much easier.
        FeatureUse,
    /// Set up the telemetry initializer to mask the platform specific names.
    internal class NameObscurerTelemetryInitializer : ITelemetryInitializer
        // Report the platform name information as "na".
        private const string _notavailable = "na";
        /// Initialize properties we are obscuring to "na".
        /// <param name="telemetry">The instance of our telemetry.</param>
        public void Initialize(ITelemetry telemetry)
            telemetry.Context.Cloud.RoleName = _notavailable;
            telemetry.Context.GetInternalContext().NodeName = _notavailable;
            telemetry.Context.Cloud.RoleInstance = _notavailable;
    /// Send up telemetry for startup.
    public static class ApplicationInsightsTelemetry
        // The string for SubsystermRegistration
        internal const string s_subsystemRegistration = "Subsystem.Registration";
        // If this env var is true, yes, or 1, telemetry will NOT be sent.
        private const string _telemetryOptoutEnvVar = "POWERSHELL_TELEMETRY_OPTOUT";
        // PSCoreInsight2 telemetry key
        // private const string _psCoreTelemetryKey = "ee4b2115-d347-47b0-adb6-b19c2c763808"; // Production
        private const string _psCoreTelemetryKey = "d26a5ef4-d608-452c-a6b8-a4a55935f70d"; // V7 Preview 3
        // In the event there is a problem in creating the node identifier file, use the default identifier.
        // This can happen if we are running in a system which has a read-only filesystem.
        private static readonly Guid _defaultNodeIdentifier = new Guid("2f998828-3f4a-4741-bf50-d11c6be42f50");
        // Use "anonymous" as the string to return when you can't report a name
        private const string Anonymous = "anonymous";
        // Use '0.0' as the string for an anonymous module version
        private const string AnonymousVersion = "0.0";
        // Use 'n/a' as the string when there's no tag to report
        private const string NoTag = "n/a";
        // the telemetry failure string
        private const string _telemetryFailure = "TELEMETRY_FAILURE";
        // Telemetry client to be reused when we start sending more telemetry
        private static TelemetryClient s_telemetryClient { get; }
        // the unique identifier for the user, when we start we
        private static string s_uniqueUserIdentifier { get; }
        // the session identifier
        private static string s_sessionId { get; }
        // private semaphore to determine whether we sent the startup telemetry event
        private static int s_startupEventSent = 0;
        /// Use a hashset for quick lookups.
        /// We send telemetry only a known set of modules and tags.
        /// If it's not in the list (initialized in the static constructor), then we report anonymous
        /// or don't report anything (in the case of tags).
        private static readonly HashSet<string> s_knownModules;
        private static readonly HashSet<string> s_knownModuleTags;
        private static readonly HashSet<string> s_knownSubsystemNames;
        private static readonly string s_uuidPath;
        /// <summary>Gets a value indicating whether telemetry can be sent.</summary>
        public static bool CanSendTelemetry { get; private set; } = false;
        /// Initializes static members of the <see cref="ApplicationInsightsTelemetry"/> class.
        /// Static constructor determines whether telemetry is to be sent, and then
        /// sets the telemetry key and set the telemetry delivery mode.
        /// Creates the session ID and initializes the HashSet of known module names.
        /// Gets or constructs the unique identifier.
        static ApplicationInsightsTelemetry()
            // If we can't send telemetry, there's no reason to do any of this
            CanSendTelemetry = !Utils.GetEnvironmentVariableAsBool(name: _telemetryOptoutEnvVar, defaultValue: false)
                && Platform.TryDeriveFromCache("telemetry.uuid", out s_uuidPath);
            if (!CanSendTelemetry)
            s_sessionId = Guid.NewGuid().ToString();
            TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
            configuration.ConnectionString = "InstrumentationKey=" + _psCoreTelemetryKey;
            // Set this to true to reduce latency during development
            configuration.TelemetryChannel.DeveloperMode = false;
            // Be sure to obscure any information about the client node name.
            configuration.TelemetryInitializers.Add(new NameObscurerTelemetryInitializer());
            s_telemetryClient = new TelemetryClient(configuration);
            // use a hashset when looking for module names, it should be quicker than a string comparison
            s_knownModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                "AADRM",
                "activedirectory",
                "adcsadministration",
                "adcsdeployment",
                "addsadministration",
                "addsdeployment",
                "adfs",
                "adrms",
                "adrmsadmin",
                "agpm",
                "AIShell",
                "appbackgroundtask",
                "applocker",
                "appv",
                "appvclient",
                "appvsequencer",
                "appvserver",
                "appx",
                "assignedaccess",
                "Az",
                "Az.Accounts",
                "Az.Advisor",
                "Az.Aks",
                "Az.AlertsManagement",
                "Az.AnalysisServices",
                "Az.ApiManagement",
                "Az.ApplicationInsights",
                "Az.Attestation",
                "Az.Automation",
                "Az.Batch",
                "Az.Billing",
                "Az.Blueprint",
                "Az.Cdn",
                "Az.CognitiveServices",
                "Az.Compute",
                "Az.ContainerInstance",
                "Az.ContainerRegistry",
                "Az.DataBox",
                "Az.DataFactory",
                "Az.DataLakeAnalytics",
                "Az.DataLakeStore",
                "Az.DataMigration",
                "Az.DataShare",
                "Az.DeploymentManager",
                "Az.DeviceProvisioningServices",
                "Az.DevSpaces",
                "Az.DevTestLabs",
                "Az.Dns",
                "Az.EventGrid",
                "Az.EventHub",
                "Az.FrontDoor",
                "Az.GuestConfiguration",
                "Az.HDInsight",
                "Az.HealthcareApis",
                "Az.IotCentral",
                "Az.IotHub",
                "Az.KeyVault",
                "Az.Kusto",
                "Az.LogicApp",
                "Az.MachineLearning",
                "Az.ManagedServiceIdentity",
                "Az.ManagedServices",
                "Az.ManagementPartner",
                "Az.Maps",
                "Az.MarketplaceOrdering",
                "Az.Media",
                "Az.MixedReality",
                "Az.Monitor",
                "Az.NetAppFiles",
                "Az.Network",
                "Az.NotificationHubs",
                "Az.OperationalInsights",
                "Az.Peering",
                "Az.PolicyInsights",
                "Az.PowerBIEmbedded",
                "Az.PrivateDns",
                "Az.RecoveryServices",
                "Az.RedisCache",
                "Az.Relay",
                "Az.Reservations",
                "Az.ResourceGraph",
                "Az.Resources",
                "Az.Search",
                "Az.Security",
                "Az.ServiceBus",
                "Az.ServiceFabric",
                "Az.SignalR",
                "Az.Sql",
                "Az.Storage",
                "Az.StorageSync",
                "Az.StorageTable",
                "Az.StreamAnalytics",
                "Az.Subscription",
                "Az.Tools.Predictor",
                "Az.TrafficManager",
                "Az.Websites",
                "Azs.Azurebridge.Admin",
                "Azs.Backup.Admin",
                "Azs.Commerce.Admin",
                "Azs.Compute.Admin",
                "Azs.Fabric.Admin",
                "Azs.Gallery.Admin",
                "Azs.Infrastructureinsights.Admin",
                "Azs.Keyvault.Admin",
                "Azs.Network.Admin",
                "Azs.Storage.Admin",
                "Azs.Subscriptions",
                "Azs.Subscriptions.Admin",
                "Azs.Update.Admin",
                "AzStorageTable",
                "Azure",
                "Azure.AnalysisServices",
                "Azure.Storage",
                "AzureAD",
                "AzureInformationProtection",
                "AzureRM.Aks",
                "AzureRM.AnalysisServices",
                "AzureRM.ApiManagement",
                "AzureRM.ApplicationInsights",
                "AzureRM.Automation",
                "AzureRM.Backup",
                "AzureRM.Batch",
                "AzureRM.Billing",
                "AzureRM.Cdn",
                "AzureRM.CognitiveServices",
                "AzureRm.Compute",
                "AzureRM.Compute.ManagedService",
                "AzureRM.Consumption",
                "AzureRM.ContainerInstance",
                "AzureRM.ContainerRegistry",
                "AzureRM.DataFactories",
                "AzureRM.DataFactoryV2",
                "AzureRM.DataLakeAnalytics",
                "AzureRM.DataLakeStore",
                "AzureRM.DataMigration",
                "AzureRM.DeploymentManager",
                "AzureRM.DeviceProvisioningServices",
                "AzureRM.DevSpaces",
                "AzureRM.DevTestLabs",
                "AzureRm.Dns",
                "AzureRM.EventGrid",
                "AzureRM.EventHub",
                "AzureRM.FrontDoor",
                "AzureRM.HDInsight",
                "AzureRm.Insights",
                "AzureRM.IotCentral",
                "AzureRM.IotHub",
                "AzureRm.Keyvault",
                "AzureRM.LocationBasedServices",
                "AzureRM.LogicApp",
                "AzureRM.MachineLearning",
                "AzureRM.MachineLearningCompute",
                "AzureRM.ManagedServiceIdentity",
                "AzureRM.ManagementPartner",
                "AzureRM.Maps",
                "AzureRM.MarketplaceOrdering",
                "AzureRM.Media",
                "AzureRM.Network",
                "AzureRM.NotificationHubs",
                "AzureRM.OperationalInsights",
                "AzureRM.PolicyInsights",
                "AzureRM.PowerBIEmbedded",
                "AzureRM.Profile",
                "AzureRM.RecoveryServices",
                "AzureRM.RecoveryServices.Backup",
                "AzureRM.RecoveryServices.SiteRecovery",
                "AzureRM.RedisCache",
                "AzureRM.Relay",
                "AzureRM.Reservations",
                "AzureRM.ResourceGraph",
                "AzureRM.Resources",
                "AzureRM.Scheduler",
                "AzureRM.Search",
                "AzureRM.Security",
                "AzureRM.ServerManagement",
                "AzureRM.ServiceBus",
                "AzureRM.ServiceFabric",
                "AzureRM.SignalR",
                "AzureRM.SiteRecovery",
                "AzureRM.Sql",
                "AzureRm.Storage",
                "AzureRM.StorageSync",
                "AzureRM.StreamAnalytics",
                "AzureRM.Subscription",
                "AzureRM.Subscription.Preview",
                "AzureRM.Tags",
                "AzureRM.TrafficManager",
                "AzureRm.UsageAggregates",
                "AzureRm.Websites",
                "AzureRmStorageTable",
                "bestpractices",
                "bitlocker",
                "bitstransfer",
                "booteventcollector",
                "branchcache",
                "clusterawareupdating",
                "CompatPowerShellGet",
                "configci",
                "ConfigurationManager",
                "CompletionPredictor",
                "DataProtectionManager",
                "dcbqos",
                "deduplication",
                "defender",
                "devicehealthattestation",
                "dfsn",
                "dfsr",
                "dhcpserver",
                "directaccessclient",
                "directaccessclientcomponent",
                "directaccessclientcomponents",
                "dism",
                "dnsclient",
                "dnsserver",
                "ElasticDatabaseJobs",
                "EventTracingManagement",
                "failoverclusters",
                "fileserverresourcemanager",
                "FIMAutomation",
                "GPRegistryPolicy",
                "grouppolicy",
                "hardwarecertification",
                "hcs",
                "hgsattestation",
                "hgsclient",
                "hgsdiagnostics",
                "hgskeyprotection",
                "hgsserver",
                "hnvdiagnostics",
                "hostcomputeservice",
                "hpc",
                "HPC.ACM",
                "HPC.ACM.API.PS",
                "HPCPack2016",
                "hyper-v",
                "IISAdministration",
                "international",
                "ipamserver",
                "iscsi",
                "iscsitarget",
                "ISE",
                "kds",
                "Microsoft.MBAM",
                "Microsoft.MEDV",
                "MgmtSvcAdmin",
                "MgmtSvcConfig",
                "MgmtSvcMySql",
                "MgmtSvcSqlServer",
                "Microsoft.AzureStack.ReadinessChecker",
                "Microsoft.Crm.PowerShell",
                "Microsoft.DiagnosticDataViewer",
                "Microsoft.DirectoryServices.MetadirectoryServices.Config",
                "Microsoft.Dynamics.Nav.Apps.Management",
                "Microsoft.Dynamics.Nav.Apps.Tools",
                "Microsoft.Dynamics.Nav.Ide",
                "Microsoft.Dynamics.Nav.Management",
                "Microsoft.Dynamics.Nav.Model.Tools",
                "Microsoft.Dynamics.Nav.Model.Tools.Crm",
                "Microsoft.EnterpriseManagement.Warehouse.Cmdlets",
                "Microsoft.Medv.Administration.Commands.WorkspacePackager",
                "Microsoft.PowerApps.Checker.PowerShell",
                "Microsoft.PowerShell.Archive",
                "Microsoft.PowerShell.ConsoleGuiTools",
                "Microsoft.PowerShell.Core",
                "Microsoft.PowerShell.Crescendo",
                "Microsoft.PowerShell.LocalAccounts",
                "Microsoft.PowerShell.ODataUtils",
                "Microsoft.PowerShell.Operation.Validation",
                "Microsoft.PowerShell.PSAdapter",
                "Microsoft.PowerShell.PSResourceGet",
                "Microsoft.PowerShell.RemotingTools",
                "Microsoft.PowerShell.SecretManagement",
                "Microsoft.PowerShell.SecretStore",
                "Microsoft.PowerShell.TextUtility",
                "Microsoft.SharePoint.Powershell",
                "Microsoft.SystemCenter.ServiceManagementAutomation",
                "Microsoft.Windows.ServerManager.Migration",
                "Microsoft.Xrm.OnlineManagementAPI",
                "Microsoft.Xrm.Tooling.CrmConnector.PowerShell",
                "Microsoft.Xrm.Tooling.PackageDeployment",
                "Microsoft.Xrm.Tooling.PackageDeployment.Powershell",
                "Microsoft.Xrm.Tooling.Testing",
                "MicrosoftPowerBIMgmt",
                "MicrosoftPowerBIMgmt.Data",
                "MicrosoftPowerBIMgmt.Profile",
                "MicrosoftPowerBIMgmt.Reports",
                "MicrosoftPowerBIMgmt.Workspaces",
                "MicrosoftStaffHub",
                "MicrosoftTeams",
                "MIMPAM",
                "mlSqlPs",
                "MMAgent",
                "MPIO",
                "MsDtc",
                "MSMQ",
                "MSOnline",
                "MSOnlineBackup",
                "WmsCmdlets",
                "WmsCmdlets3",
                "NanoServerImageGenerator",
                "NAVWebClientManagement",
                "NetAdapter",
                "NetConnection",
                "NetEventPacketCapture",
                "Netlbfo",
                "Netldpagent",
                "NetNat",
                "Netqos",
                "NetSecurity",
                "NetSwitchtTeam",
                "Nettcpip",
                "Netwnv",
                "NetworkConnectivity",
                "NetworkConnectivityStatus",
                "NetworkController",
                "NetworkControllerDiagnostics",
                "NetworkloadBalancingClusters",
                "NetworkSwitchManager",
                "NetworkTransition",
                "NFS",
                "NPS",
                "OfficeWebapps",
                "OperationsManager",
                "PackageManagement",
                "PartnerCenter",
                "pcsvdevice",
                "pef",
                "Pester",
                "pkiclient",
                "platformidentifier",
                "pnpdevice",
                "PowerShellEditorServices",
                "PowerShellGet",
                "powershellwebaccess",
                "printmanagement",
                "ProcessMitigations",
                "provisioning",
                "PSDesiredStateConfiguration",
                "PSReadLine",
                "PSScheduledJob",
                "PSScriptAnalyzer",
                "PSWorkflow",
                "PSWorkflowUtility",
                "RemoteAccess",
                "RemoteDesktop",
                "RemoteDesktopServices",
                "ScheduledTasks",
                "Secureboot",
                "ServerCore",
                "ServerManager",
                "ServerManagerTasks",
                "ServerMigrationcmdlets",
                "ServiceFabric",
                "Microsoft.Online.SharePoint.PowerShell",
                "shieldedvmdatafile",
                "shieldedvmprovisioning",
                "shieldedvmtemplate",
                "SkypeOnlineConnector",
                "SkypeForBusinessHybridHealth",
                "smbshare",
                "smbwitness",
                "smisconfig",
                "softwareinventorylogging",
                "SPFAdmin",
                "Microsoft.SharePoint.MigrationTool.PowerShell",
                "sqlps",
                "SqlServer",
                "StartLayout",
                "StartScreen",
                "Storage",
                "StorageDsc",
                "storageqos",
                "Storagereplica",
                "Storagespaces",
                "Syncshare",
                "System.Center.Service.Manager",
                "TLS",
                "TroubleshootingPack",
                "TrustedPlatformModule",
                "UEV",
                "UpdateServices",
                "UserAccessLogging",
                "vamt",
                "VirtualMachineManager",
                "vpnclient",
                "WasPSExt",
                "WDAC",
                "WDS",
                "WebAdministration",
                "WebAdministrationDsc",
                "WebApplicationProxy",
                "WebSites",
                "Whea",
                "WhiteboardAdmin",
                "WindowsDefender",
                "WindowsDefenderDsc",
                "WindowsDeveloperLicense",
                "WindowsDiagnosticData",
                "WindowsErrorReporting",
                "WindowServerRackup",
                "WindowsSearch",
                "WindowsServerBackup",
                "WindowsUpdate",
                "WinGetCommandNotFound",
                "wsscmdlets",
                "wsssetup",
                "wsus",
                "xActiveDirectory",
                "xBitLocker",
                "xDefender",
                "xDhcpServer",
                "xDismFeature",
                "xDnsServer",
                "xHyper-V",
                "xHyper-VBackup",
                "xPSDesiredStateConfiguration",
                "xSmbShare",
                "xSqlPs",
                "xStorage",
                "xWebAdministration",
                "xWindowsUpdate",
            s_knownModuleTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                "CrescendoBuilt",
            s_uniqueUserIdentifier = GetUniqueIdentifier().ToString();
            s_knownSubsystemNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                "Completion",
                "General Feedback",
                "Windows Package Manager - WinGet",
                "Az Predictor"
        /// Send module load telemetry as a metric.
        /// For modules we send the module name (if allowed), and the version.
        /// Some modules (CIM) will continue use the string alternative method.
        /// <param name="telemetryType">The type of telemetry that we'll be sending.</param>
        /// <param name="moduleInfo">The module to report. If it is not allowed, then it is set to 'anonymous'.</param>
        internal static void SendModuleTelemetryMetric(TelemetryType telemetryType, PSModuleInfo moduleInfo)
            // Package up the module name, version, and known tags as a metric.
            // Note that the allowed tags will be a comma separated list which will need to
            // be handled in the telemetry query.
                string allowedModuleName = GetModuleName(moduleInfo.Name);
                string allowedModuleVersion = allowedModuleName == Anonymous ? AnonymousVersion : moduleInfo.Version?.ToString();
                var allowedModuleTags = moduleInfo.Tags.Where(t => s_knownModuleTags.Contains(t)).Distinct();
                string allowedModuleTagString = allowedModuleTags.Any() ? string.Join(',', allowedModuleTags) : NoTag;
                s_telemetryClient.
                    GetMetric(new MetricIdentifier(string.Empty, telemetryType.ToString(), "uuid", "SessionId", "ModuleName", "Version", "Tag")).
                    TrackValue(metricValue: 1.0, s_uniqueUserIdentifier, s_sessionId, allowedModuleName, allowedModuleVersion, allowedModuleTagString);
                // Ignore errors.
        /// <param name="moduleName">The module name to report. If it is not allowed, then it is set to 'anonymous'.</param>
        internal static void SendModuleTelemetryMetric(TelemetryType telemetryType, string moduleName)
                string allowedModuleName = GetModuleName(moduleName);
                s_telemetryClient.GetMetric(telemetryType.ToString(), "uuid", "SessionId", "ModuleName", "Version").TrackValue(metricValue: 1.0, s_uniqueUserIdentifier, s_sessionId, allowedModuleName, AnonymousVersion);
        /// Send telemetry as a metric.
        /// <param name="metricId">The type of telemetry that we'll be sending.</param>
        /// <param name="data">The specific details about the telemetry.</param>
        /// <param name="value">The count of instances for the telemetry payload.</param>
        internal static void SendTelemetryMetric(TelemetryType metricId, string data, double value = 1.0)
            // These should be handled by SendModuleTelemetryMetric.
            Debug.Assert(metricId != TelemetryType.ModuleLoad, "ModuleLoad should be handled by SendModuleTelemetryMetric.");
            Debug.Assert(metricId != TelemetryType.WinCompatModuleLoad, "WinCompatModuleLoad should be handled by SendModuleTelemetryMetric.");
            string metricName = metricId.ToString();
                switch (metricId)
                    case TelemetryType.ApplicationType:
                    case TelemetryType.PowerShellCreate:
                    case TelemetryType.RemoteSessionOpen:
                    case TelemetryType.ExperimentalEngineFeatureActivation:
                    case TelemetryType.ExperimentalEngineFeatureDeactivation:
                    case TelemetryType.ExperimentalFeatureUse:
                    case TelemetryType.FeatureUse:
                        s_telemetryClient.GetMetric(metricName, "uuid", "SessionId", "Detail").TrackValue(metricValue: value, s_uniqueUserIdentifier, s_sessionId, data);
                    case TelemetryType.ExperimentalModuleFeatureActivation:
                    case TelemetryType.ExperimentalModuleFeatureDeactivation:
                        string experimentalFeatureName = GetExperimentalFeatureName(data);
                        s_telemetryClient.GetMetric(metricName, "uuid", "SessionId", "Detail").TrackValue(metricValue: value, s_uniqueUserIdentifier, s_sessionId, experimentalFeatureName);
                // do nothing, telemetry can't be sent
                // don't send the panic telemetry as if we have failed above, it will likely fail here.
        /// Send additional information about an feature as it is used.
        /// <param name="featureName">The name of the feature.</param>
        /// <param name="detail">The details about the feature use.</param>
        /// <param name="value">The value to report when sending the payload.</param>
        internal static void SendUseTelemetry(string featureName, string detail, double value = 1.0)
            // keep payload small
            if (featureName is null || detail is null || featureName.Length > 33 || detail.Length > 33)
            if (string.Compare(featureName, s_subsystemRegistration, true) == 0)
                ApplicationInsightsTelemetry.SendTelemetryMetric(TelemetryType.FeatureUse, string.Join(":", featureName, GetSubsystemName(detail)), value);
                ApplicationInsightsTelemetry.SendTelemetryMetric(TelemetryType.FeatureUse, string.Join(":", featureName, detail), value);
        /// Send additional information about an experimental feature as it is used.
        /// <param name="featureName">The name of the experimental feature.</param>
        /// <param name="detail">The details about the experimental feature use.</param>
        internal static void SendExperimentalUseData(string featureName, string detail)
            ApplicationInsightsTelemetry.SendTelemetryMetric(TelemetryType.ExperimentalFeatureUse, string.Join(":", featureName, detail));
        // Get the experimental feature name. If we can report it, we'll return the name of the feature, otherwise, we'll return "anonymous"
        private static string GetExperimentalFeatureName(string featureNameToValidate)
            // An experimental feature in a module is guaranteed to start with the module name
            // we can strip out the text past the last '.' as the text before that will be the ModuleName
            int lastDotIndex = featureNameToValidate.LastIndexOf('.');
            string moduleName = featureNameToValidate.Substring(0, lastDotIndex);
            if (s_knownModules.Contains(moduleName))
                return featureNameToValidate;
            return Anonymous;
        // Get the module name. If we can report it, we'll return the name, otherwise, we'll return the string "anonymous"
        private static string GetSubsystemName(string subsystemNameToValidate)
            if (s_knownSubsystemNames.Contains(subsystemNameToValidate))
                return subsystemNameToValidate;
        // Get the module name. If we can report it, we'll return the name, otherwise, we'll return anonymous.
        private static string GetModuleName(string moduleNameToValidate)
            if (s_knownModules.Contains(moduleNameToValidate))
                return moduleNameToValidate;
        /// Create the startup payload and send it up.
        /// This is done only once during for the console host.
        /// <param name="mode">The "mode" of the startup.</param>
        /// <param name="parametersUsed">The parameter bitmap used when starting.</param>
        internal static void SendPSCoreStartupTelemetry(string mode, double parametersUsed)
            // Check if we already sent startup telemetry
            if (Interlocked.CompareExchange(ref s_startupEventSent, 1, 0) == 1)
            // This is the payload which reports the startup information of OS and shell details.
            var properties = new Dictionary<string, string>();
            // This is the payload for the parameter data which is sent as a metric.
            var parameters = new Dictionary<string, double>();
            // The variable POWERSHELL_DISTRIBUTION_CHANNEL is set in our docker images and
            // by various other environments. This allows us to track the actual docker OS as
            // OSDescription provides only "linuxkit" which has limited usefulness.
            var channel = Environment.GetEnvironmentVariable("POWERSHELL_DISTRIBUTION_CHANNEL");
            // Construct the payload for the OS and shell details.
            properties.Add("SessionId", s_sessionId);
            properties.Add("UUID", s_uniqueUserIdentifier);
            properties.Add("GitCommitID", PSVersionInfo.GitCommitId);
            properties.Add("OSDescription", RuntimeInformation.OSDescription);
            properties.Add("RuntimeIdentifier", RuntimeInformation.RuntimeIdentifier);
            properties.Add("OSChannel", string.IsNullOrEmpty(channel) ? "unknown" : channel);
            properties.Add("StartMode", string.IsNullOrEmpty(mode) ? "unknown" : mode);
            // Construct the payload for the parameters used.
            parameters.Add("Param", parametersUsed);
                s_telemetryClient.TrackEvent("ConsoleHostStartup", properties, parameters);
                // do nothing, telemetry cannot be sent
        /// Try to read the file and collect the guid.
        /// <param name="telemetryFilePath">The path to the telemetry file.</param>
        /// <param name="id">The newly created id.</param>
        /// The method returns a bool indicating success or failure of creating the id.
        private static bool TryGetIdentifier(string telemetryFilePath, out Guid id)
            if (File.Exists(telemetryFilePath))
                // attempt to read the persisted identifier
                const int GuidSize = 16;
                byte[] buffer = new byte[GuidSize];
                    using (FileStream fs = new FileStream(telemetryFilePath, FileMode.Open, FileAccess.Read))
                        // if the read is invalid, or wrong size, we return it
                        int n = fs.Read(buffer, 0, GuidSize);
                        if (n == GuidSize)
                            // it's possible this could through
                            id = new Guid(buffer);
                            if (id != Guid.Empty)
                    // something went wrong, the file may not exist or not have enough bytes, so return false
            id = Guid.Empty;
        /// Try to create a unique identifier and persist it to the telemetry.uuid file.
        /// <param name="telemetryFilePath">The path to the persisted telemetry.uuid file.</param>
        /// The method node id.
        private static Guid CreateUniqueIdentifierAndFile(string telemetryFilePath)
            // one last attempt to retrieve before creating incase we have a lot of simultaneous entry into the mutex.
            Guid id = Guid.Empty;
            if (TryGetIdentifier(telemetryFilePath, out id))
            // The directory may not exist, so attempt to create it
            // CreateDirectory will simply return the directory if exists
            bool attemptFileCreation = true;
                Directory.CreateDirectory(Path.GetDirectoryName(telemetryFilePath));
                // There was a problem in creating the directory for the file, do not attempt to create the file.
                // We don't send telemetry here because there are valid reasons for the directory to not exist
                // and not be able to be created.
                attemptFileCreation = false;
            // If we were able to create the directory, try to create the file,
            // if this fails we will send telemetry to indicate this and then use the default identifier.
            if (attemptFileCreation)
                    id = Guid.NewGuid();
                    File.WriteAllBytes(telemetryFilePath, id.ToByteArray());
                    // another bit of telemetry to notify us about a problem with saving the unique id.
                    s_telemetryClient.GetMetric(_telemetryFailure, "Detail").TrackValue(1, "saveuuid");
            // all attempts to create an identifier have failed, so use the default node id.
            id = _defaultNodeIdentifier;
        /// Retrieve the unique identifier from the persisted file, if it doesn't exist create it.
        /// Generate a guid which will be used as the UUID.
        /// <returns>A guid which represents the unique identifier.</returns>
        private static Guid GetUniqueIdentifier()
            // Try to get the unique id. If this returns false, we'll
            // create/recreate the telemetry.uuid file to persist for next startup.
            if (TryGetIdentifier(s_uuidPath, out id))
            // Multiple processes may start simultaneously so we need a system wide
            // way to control access to the file in the case (although remote) when we have
            // simultaneous shell starts without the persisted file which attempt to create the file.
                // CreateUniqueIdentifierAndFile shouldn't throw, but the mutex might
                using var m = new Mutex(true, "CreateUniqueUserId");
                m.WaitOne();
                    return CreateUniqueIdentifierAndFile(s_uuidPath);
                    m.ReleaseMutex();
                // Any problem in generating a uuid will result in no telemetry being sent.
                // Try to send the failure in telemetry, but it will have no unique id.
                s_telemetryClient.GetMetric(_telemetryFailure, "Detail").TrackValue(1, "mutex");
            // something bad happened, turn off telemetry since the unique id wasn't set.
            CanSendTelemetry = false;

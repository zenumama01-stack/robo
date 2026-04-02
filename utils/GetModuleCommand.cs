    /// Implements a cmdlet that gets the list of loaded modules...
    [Cmdlet(VerbsCommon.Get, "Module", DefaultParameterSetName = ParameterSet_Loaded,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096696")]
    public sealed class GetModuleCommand : ModuleCmdletBase, IDisposable
        #region Cmdlet parameters
        private const string ParameterSet_Loaded = "Loaded";
        private const string ParameterSet_AvailableLocally = "Available";
        private const string ParameterSet_AvailableInPsrpSession = "PsSession";
        private const string ParameterSet_AvailableInCimSession = "CimSession";
        [Parameter(ParameterSetName = ParameterSet_Loaded, ValueFromPipeline = true, Position = 0)]
        [Parameter(ParameterSetName = ParameterSet_AvailableLocally, ValueFromPipeline = true, Position = 0)]
        [Parameter(ParameterSetName = ParameterSet_AvailableInPsrpSession, ValueFromPipeline = true, Position = 0)]
        [Parameter(ParameterSetName = ParameterSet_AvailableInCimSession, ValueFromPipeline = true, Position = 0)]
            Justification = "Cmdlets use arrays for parameters.")]
        [Parameter(ParameterSetName = ParameterSet_Loaded, ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = ParameterSet_AvailableLocally, ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = ParameterSet_AvailableInPsrpSession, ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = ParameterSet_AvailableInCimSession, ValueFromPipelineByPropertyName = true)]
        public ModuleSpecification[] FullyQualifiedName { get; set; }
        /// If specified, all loaded modules should be returned, otherwise only the visible
        /// modules should be returned.
        [Parameter(ParameterSetName = ParameterSet_Loaded)]
        [Parameter(ParameterSetName = ParameterSet_AvailableLocally)]
        public SwitchParameter All { get; set; }
        /// If specified, then Get-Module will return the set of available modules...
        [Parameter(ParameterSetName = ParameterSet_AvailableLocally, Mandatory = true)]
        [Parameter(ParameterSetName = ParameterSet_AvailableInPsrpSession)]
        [Parameter(ParameterSetName = ParameterSet_AvailableInCimSession)]
        /// If specified, then Get-Module will return the set of available modules which supports the specified PowerShell edition...
        [ArgumentCompleter(typeof(PSEditionArgumentCompleter))]
        public string PSEdition { get; set; }
        /// When set, CompatiblePSEditions checking is disabled for modules in the System32 (Windows PowerShell) module directory.
        public SwitchParameter SkipEditionCheck
            get { return (SwitchParameter)BaseSkipEditionCheck; }
            set { BaseSkipEditionCheck = value; }
        /// If specified, then Get-Module refreshes the internal cmdlet analysis cache.
        public SwitchParameter Refresh { get; set; }
        /// If specified, then Get-Module will attempt to discover PowerShell modules on a remote computer using the specified session.
        [Parameter(ParameterSetName = ParameterSet_AvailableInPsrpSession, Mandatory = true)]
        public PSSession PSSession { get; set; }
        /// If specified, then Get-Module will attempt to discover PS-CIM modules on a remote computer using the specified session.
        [Parameter(ParameterSetName = ParameterSet_AvailableInCimSession, Mandatory = true)]
        public CimSession CimSession { get; set; }
        /// For interoperability with 3rd party CIM servers, user can specify custom resource URI.
        [Parameter(ParameterSetName = ParameterSet_AvailableInCimSession, Mandatory = false)]
        public Uri CimResourceUri { get; set; }
        /// For interoperability with 3rd party CIM servers, user can specify custom namespace.
        public string CimNamespace { get; set; }
        #endregion Cmdlet parameters
        #region Remote discovery
        private IEnumerable<PSModuleInfo> GetAvailableViaPsrpSessionCore(string[] moduleNames, Runspace remoteRunspace)
            Dbg.Assert(remoteRunspace != null, "Caller should verify remoteRunspace != null");
            using (var powerShell = System.Management.Automation.PowerShell.Create())
                powerShell.Runspace = remoteRunspace;
                powerShell.AddParameter("ListAvailable", true);
                if (Refresh.IsPresent)
                    powerShell.AddParameter("Refresh", true);
                if (moduleNames != null)
                    powerShell.AddParameter("Name", moduleNames);
                string errorMessageTemplate = string.Format(
                    Modules.RemoteDiscoveryRemotePsrpCommandFailed,
                    "Get-Module");
                    PSObject outputObject in
                        RemoteDiscoveryHelper.InvokePowerShell(powerShell, this, errorMessageTemplate,
                                                               this.CancellationToken))
                    PSModuleInfo moduleInfo = RemoteDiscoveryHelper.RehydratePSModuleInfo(outputObject);
                    yield return moduleInfo;
        private static PSModuleInfo GetModuleInfoForRemoteModuleWithoutManifest(RemoteDiscoveryHelper.CimModule cimModule)
            return new PSModuleInfo(cimModule.ModuleName, null, null);
        private PSModuleInfo ConvertCimModuleInfoToPSModuleInfo(RemoteDiscoveryHelper.CimModule cimModule,
                                                                string computerName)
                bool containedErrors = false;
                if (cimModule.MainManifest == null)
                    return GetModuleInfoForRemoteModuleWithoutManifest(cimModule);
                string temporaryModuleManifestPath = Path.Combine(
                    RemoteDiscoveryHelper.GetModulePath(cimModule.ModuleName, null, computerName,
                                                        this.Context.CurrentRunspace),
                    Path.GetFileName(cimModule.ModuleName));
                Hashtable mainData = null;
                if (!containedErrors)
                    mainData = RemoteDiscoveryHelper.ConvertCimModuleFileToManifestHashtable(
                        cimModule.MainManifest,
                        temporaryModuleManifestPath,
                        ref containedErrors);
                    if (mainData == null)
                    mainData = RemoteDiscoveryHelper.RewriteManifest(mainData);
                Hashtable localizedData = mainData; // TODO/FIXME - this needs full path support from the provider
                    ImportModuleOptions throwAwayOptions = new ImportModuleOptions();
                    moduleInfo = LoadModuleManifest(
                        null, // scriptInfo
                        mainData,
                        localizedData,
                        0 /* - don't write errors, don't load elements, don't return null on first error */,
                        this.BaseMinimumVersion,
                        this.BaseMaximumVersion,
                        this.BaseRequiredVersion,
                        this.BaseGuid,
                        ref throwAwayOptions,
                if ((moduleInfo == null) || containedErrors)
                    moduleInfo = GetModuleInfoForRemoteModuleWithoutManifest(cimModule);
                ErrorRecord errorRecord = RemoteDiscoveryHelper.GetErrorRecordForProcessingOfCimModule(e, cimModule.ModuleName);
        private IEnumerable<PSModuleInfo> GetAvailableViaCimSessionCore(IEnumerable<string> moduleNames,
                                                                        CimSession cimSession, Uri resourceUri,
                                                                        string cimNamespace)
            IEnumerable<RemoteDiscoveryHelper.CimModule> remoteModules = RemoteDiscoveryHelper.GetCimModules(
                cimSession,
                resourceUri,
                cimNamespace,
                moduleNames,
                true /* onlyManifests */,
                this.CancellationToken);
            IEnumerable<PSModuleInfo> remoteModuleInfos = remoteModules
                .Select(cimModule => this.ConvertCimModuleInfoToPSModuleInfo(cimModule, cimSession.ComputerName))
                .Where(static moduleInfo => moduleInfo != null);
            return remoteModuleInfos;
        #endregion Remote discovery
        #region Cancellation support
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private CancellationToken CancellationToken
            get { return _cancellationTokenSource.Token; }
        private void AssertListAvailableMode()
            if (!this.ListAvailable.IsPresent)
                string errorMessage = Modules.RemoteDiscoveryWorksOnlyInListAvailableMode;
                ArgumentException argumentException = new ArgumentException(errorMessage);
                    "RemoteDiscoveryWorksOnlyInListAvailableMode",
        /// Write out the specified modules...
            // Name and FullyQualifiedName should not be specified at the same time.
            if ((Name != null) && (FullyQualifiedName != null))
                string errMsg = StringUtil.Format(SessionStateStrings.GetContent_TailAndHeadCannotCoexist, "Name", "FullyQualifiedName");
                ErrorRecord error = new ErrorRecord(new InvalidOperationException(errMsg), "NameAndFullyQualifiedNameCannotBeSpecifiedTogether", ErrorCategory.InvalidOperation, null);
            // -SkipEditionCheck only makes sense for -ListAvailable (otherwise the module is already loaded)
            if (SkipEditionCheck && !ListAvailable)
                    new InvalidOperationException(Modules.SkipEditionCheckNotSupportedWithoutListAvailable),
                    nameof(Modules.SkipEditionCheckNotSupportedWithoutListAvailable),
            var strNames = new List<string>();
                strNames.AddRange(Name);
            var moduleSpecTable = new Dictionary<string, ModuleSpecification>(StringComparer.OrdinalIgnoreCase);
            if (FullyQualifiedName != null)
                for (int modSpecIndex = 0; modSpecIndex < FullyQualifiedName.Length; modSpecIndex++)
                    FullyQualifiedName[modSpecIndex] = FullyQualifiedName[modSpecIndex].WithNormalizedName(Context, SessionState.Path.CurrentLocation.Path);
                moduleSpecTable = FullyQualifiedName.ToDictionary(static moduleSpecification => moduleSpecification.Name, StringComparer.OrdinalIgnoreCase);
                strNames.AddRange(FullyQualifiedName.Select(static spec => spec.Name));
            string[] names = strNames.Count > 0 ? strNames.ToArray() : null;
            if (ParameterSetName.Equals(ParameterSet_Loaded, StringComparison.OrdinalIgnoreCase))
                AssertNameDoesNotResolveToAPath(names,
                                                Modules.ModuleDiscoveryForLoadedModulesWorksOnlyForUnQualifiedNames,
                                                "ModuleDiscoveryForLoadedModulesWorksOnlyForUnQualifiedNames");
                GetLoadedModules(names, moduleSpecTable, this.All);
            else if (ParameterSetName.Equals(ParameterSet_AvailableLocally, StringComparison.OrdinalIgnoreCase))
                if (ListAvailable.IsPresent)
                    GetAvailableLocallyModules(names, moduleSpecTable, this.All);
            else if (ParameterSetName.Equals(ParameterSet_AvailableInPsrpSession, StringComparison.OrdinalIgnoreCase))
                AssertListAvailableMode();
                                                Modules.RemoteDiscoveryWorksOnlyForUnQualifiedNames,
                                                "RemoteDiscoveryWorksOnlyForUnQualifiedNames");
                GetAvailableViaPsrpSession(names, moduleSpecTable, this.PSSession);
            else if (ParameterSetName.Equals(ParameterSet_AvailableInCimSession, StringComparison.OrdinalIgnoreCase))
                GetAvailableViaCimSession(names, moduleSpecTable, this.CimSession,
                                          this.CimResourceUri, this.CimNamespace);
                Dbg.Assert(false, "Unrecognized parameter set");
        private void AssertNameDoesNotResolveToAPath(string[] names, string stringFormat, string resourceId)
                foreach (var n in names)
                    if (n.Contains(StringLiterals.DefaultPathSeparator) || n.Contains(StringLiterals.AlternatePathSeparator))
                        string errorMessage = StringUtil.Format(stringFormat, n);
                        var argumentException = new ArgumentException(errorMessage);
                            n);
        private void GetAvailableViaCimSession(IEnumerable<string> names, IDictionary<string, ModuleSpecification> moduleSpecTable,
                                               CimSession cimSession, Uri resourceUri, string cimNamespace)
            IEnumerable<PSModuleInfo> remoteModules = GetAvailableViaCimSessionCore(names, cimSession, resourceUri, cimNamespace);
            foreach (PSModuleInfo remoteModule in FilterModulesForEditionAndSpecification(remoteModules, moduleSpecTable))
                RemoteDiscoveryHelper.AssociatePSModuleInfoWithSession(remoteModule, cimSession, resourceUri,
                                                                       cimNamespace);
                this.WriteObject(remoteModule);
        private void GetAvailableViaPsrpSession(string[] names, IDictionary<string, ModuleSpecification> moduleSpecTable, PSSession session)
            IEnumerable<PSModuleInfo> remoteModules = GetAvailableViaPsrpSessionCore(names, session.Runspace);
                RemoteDiscoveryHelper.AssociatePSModuleInfoWithSession(remoteModule, session);
        private void GetAvailableLocallyModules(string[] names, IDictionary<string, ModuleSpecification> moduleSpecTable, bool all)
            IEnumerable<PSModuleInfo> modules = GetModule(names, all, Refresh);
            foreach (PSModuleInfo module in FilterModulesForEditionAndSpecification(modules, moduleSpecTable))
                var psModule = new PSObject(module);
                psModule.TypeNames.Insert(0, "ModuleInfoGrouping");
                WriteObject(psModule);
        private void GetLoadedModules(string[] names, IDictionary<string, ModuleSpecification> moduleSpecTable, bool all)
            var modulesToWrite = Context.Modules.GetModules(names, all);
            foreach (PSModuleInfo moduleInfo in FilterModulesForEditionAndSpecification(modulesToWrite, moduleSpecTable))
                WriteObject(moduleInfo);
        /// Filter an enumeration of PowerShell modules based on the required PowerShell edition
        /// and the module specification constraints set for each module (if any).
        /// <param name="modules">The modules to filter through.</param>
        /// <param name="moduleSpecificationTable">Module constraints, keyed by module name, to filter modules of that name by.</param>
        /// <returns>All modules from the original input that meet both any module edition and module specification constraints provided.</returns>
        private IEnumerable<PSModuleInfo> FilterModulesForEditionAndSpecification(
            IEnumerable<PSModuleInfo> modules,
            IDictionary<string, ModuleSpecification> moduleSpecificationTable)
            // Edition check only applies to Windows System32 module path
            if (!SkipEditionCheck && ListAvailable && !All)
                modules = modules.Where(static module => module.IsConsideredEditionCompatible);
            if (!string.IsNullOrEmpty(PSEdition))
                modules = modules.Where(module => module.CompatiblePSEditions.Contains(PSEdition, StringComparer.OrdinalIgnoreCase));
            if (moduleSpecificationTable != null && moduleSpecificationTable.Count > 0)
                modules = FilterModulesForSpecificationMatch(modules, moduleSpecificationTable);
        /// Take an enumeration of modules and only return those that match a specification
        /// in the given specification table, or have no corresponding entry in the specification table.
        /// <param name="modules">The modules to filter by specification match.</param>
        /// <param name="moduleSpecificationTable">The specification lookup table to filter the modules on.</param>
        /// <returns>The modules that match their corresponding table entry, or which have no table entry.</returns>
        private static IEnumerable<PSModuleInfo> FilterModulesForSpecificationMatch(
            Dbg.Assert(moduleSpecificationTable != null, $"Caller to verify that {nameof(moduleSpecificationTable)} is not null");
            Dbg.Assert(moduleSpecificationTable.Count != 0, $"Caller to verify that {nameof(moduleSpecificationTable)} is not empty");
            foreach (PSModuleInfo module in modules)
                IEnumerable<ModuleSpecification> candidateModuleSpecs = GetCandidateModuleSpecs(moduleSpecificationTable, module);
                // Modules with table entries only get returned if they match them
                // We skip the name check since modules have already been prefiltered base on the moduleSpec path/name
                foreach (ModuleSpecification moduleSpec in candidateModuleSpecs)
                    if (ModuleIntrinsics.IsModuleMatchingModuleSpec(module, moduleSpec, skipNameCheck: true))
                        yield return module;
        /// Take a dictionary of module specifications and return those that potentially match the module
        /// passed in as a parameter (checks on names and paths).
        /// <param name="moduleSpecTable">The module specifications to filter candidates from.</param>
        /// <param name="module">The module to find candidates for from the module specification table.</param>
        /// <returns>The module specifications matching the module based on name, path and subpath.</returns>
        private static IEnumerable<ModuleSpecification> GetCandidateModuleSpecs(
            IDictionary<string, ModuleSpecification> moduleSpecTable,
            PSModuleInfo module)
            const WildcardOptions options = WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant;
            foreach (ModuleSpecification moduleSpec in moduleSpecTable.Values)
                WildcardPattern namePattern = WildcardPattern.Get(moduleSpec.Name, options);
                if (namePattern.IsMatch(module.Name) || moduleSpec.Name == module.Path || module.Path.Contains(moduleSpec.Name))
                    yield return moduleSpec;
    /// Provides argument completion for PSEdition parameter.
    public class PSEditionArgumentCompleter : IArgumentCompleter
        /// Returns completion results for PSEdition parameter.
                => CompletionHelpers.GetMatchingResults(wordToComplete, possibleCompletionValues: Utils.AllowedEditionValues);

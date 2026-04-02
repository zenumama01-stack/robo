    #region Remove-Module
    [Cmdlet(VerbsCommon.Remove, "Module", SupportsShouldProcess = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096802")]
    public sealed class RemoveModuleCommand : ModuleCmdletBase
        [Parameter(Mandatory = true, ParameterSetName = "name", ValueFromPipeline = true, Position = 0)]
        private string[] _name = Array.Empty<string>();
        [Parameter(Mandatory = true, ParameterSetName = "FullyQualifiedName", ValueFromPipeline = true, Position = 0)]
        [Parameter(Mandatory = true, ParameterSetName = "ModuleInfo", ValueFromPipeline = true, Position = 0)]
        public PSModuleInfo[] ModuleInfo
            get { return _moduleInfo; }
            set { _moduleInfo = value; }
        private PSModuleInfo[] _moduleInfo = Array.Empty<PSModuleInfo>();
        /// If provided, this parameter will allow readonly modules to be removed.
            get { return BaseForce; }
        private int _numberRemoved = 0;  // Maintains a count of the number of modules removed...
        /// Remove the specified modules. Modules can be specified either through a ModuleInfo or a name.
            // This dictionary has the list of modules to be removed.
            // Key - Module specified as a parameter to Remove-Module
            // Values - List of all modules that need to be removed for this key (includes all nested modules of this module)
            Dictionary<PSModuleInfo, List<PSModuleInfo>> modulesToRemove = new Dictionary<PSModuleInfo, List<PSModuleInfo>>();
            foreach (var m in Context.Modules.GetModules(_name, false))
                modulesToRemove.Add(m, new List<PSModuleInfo> { m });
                // TODO:
                // Paths in the module name may fail here because
                // they the wrong directory separator or are relative.
                // Fix with the code below:
                // FullyQualifiedName = FullyQualifiedName.Select(ms => ms.WithNormalizedName(Context, SessionState.Path.CurrentLocation.Path)).ToArray();
                foreach (var m in Context.Modules.GetModules(FullyQualifiedName, false))
            foreach (var m in _moduleInfo)
            // Add any of the child modules of a manifests to the list of modules to remove...
            Dictionary<PSModuleInfo, List<PSModuleInfo>> nestedModules = new Dictionary<PSModuleInfo, List<PSModuleInfo>>();
            foreach (var entry in modulesToRemove)
                var module = entry.Key;
                if (module.NestedModules != null && module.NestedModules.Count > 0)
                    List<PSModuleInfo> nestedModulesWithNoCircularReference = new List<PSModuleInfo>();
                    GetAllNestedModules(module, ref nestedModulesWithNoCircularReference);
                    nestedModules.Add(module, nestedModulesWithNoCircularReference);
            // dont add duplicates to our original modulesToRemove list..so that the
            // evaluation loop below will not duplicate in case of WriteError and WriteWarning.
            // A global list of modules to be removed is maintained for this purpose
            HashSet<PSModuleInfo> globalListOfModules = new HashSet<PSModuleInfo>(new PSModuleInfoComparer());
            if (nestedModules.Count > 0)
                foreach (var entry in nestedModules)
                    List<PSModuleInfo> values = null;
                    if (modulesToRemove.TryGetValue(entry.Key, out values))
                        foreach (var module in entry.Value)
                            if (!globalListOfModules.Contains(module))
                                values.Add(module);
                                globalListOfModules.Add(module);
            // Check the list of modules to remove and exclude those that cannot or should not be removed
            Dictionary<PSModuleInfo, List<PSModuleInfo>> actualModulesToRemove = new Dictionary<PSModuleInfo, List<PSModuleInfo>>();
            // We want to remove the modules starting from the nested modules
            // If we start from the parent module, the nested modules do not get removed and are left orphaned in the parent modules's sessionstate.
                List<PSModuleInfo> moduleList = new List<PSModuleInfo>();
                for (int i = entry.Value.Count - 1; i >= 0; i--)
                    PSModuleInfo module = entry.Value[i];
                    // See if the module is constant...
                    if (module.AccessMode == ModuleAccessMode.Constant)
                        string message = StringUtil.Format(Modules.ModuleIsConstant, module.Name);
                        InvalidOperationException moduleNotRemoved = new InvalidOperationException(message);
                        ErrorRecord er = new ErrorRecord(moduleNotRemoved, "Modules_ModuleIsConstant",
                                                         ErrorCategory.PermissionDenied, module);
                    // See if the module is readonly...
                    if (module.AccessMode == ModuleAccessMode.ReadOnly && !BaseForce)
                        string message = StringUtil.Format(Modules.ModuleIsReadOnly, module.Name);
                        if (InitialSessionState.IsConstantEngineModule(module.Name))
                            ErrorRecord er = new ErrorRecord(moduleNotRemoved, "Modules_ModuleIsReadOnly",
                    if (!ShouldProcess(StringUtil.Format(Modules.ConfirmRemoveModule, module.Name, module.Path)))
                    // If this module provides the current session drive, then we cannot remove it.
                    // Abort this command since we don't want to do a partial removal of a module manifest.
                    if (ModuleProvidesCurrentSessionDrive(module))
                        if (InitialSessionState.IsEngineModule(module.Name))
                                string message = StringUtil.Format(Modules.CoreModuleCannotBeRemoved, module.Name);
                        // Specify the overall module name if there is only one.
                        // Otherwise specify the particular module name.
                        string moduleName = (_name.Length == 1) ? _name[0] : module.Name;
                                Modules.ModuleDriveInUse,
                        throw (invalidOperation);
                    // Add module to remove list.
                actualModulesToRemove[entry.Key] = moduleList;
            // Now remove the modules, first checking the RequiredModules dependencies
            Dictionary<PSModuleInfo, List<PSModuleInfo>> requiredDependencies = GetRequiredDependencies();
            foreach (var entry in actualModulesToRemove)
                        List<PSModuleInfo> requiredBy = null;
                        if (requiredDependencies.TryGetValue(module, out requiredBy))
                            for (int i = requiredBy.Count - 1; i >= 0; i--)
                                if (actualModulesToRemove.ContainsKey(requiredBy[i]))
                                    requiredBy.RemoveAt(i);
                            if (requiredBy.Count > 0)
                                string message = StringUtil.Format(Modules.ModuleIsRequired, module.Name, requiredBy[0].Name);
                                ErrorRecord er = new ErrorRecord(moduleNotRemoved, "Modules_ModuleIsRequired",
                    _numberRemoved++;
                    this.RemoveModule(module, entry.Key.Name);
        private bool ModuleProvidesCurrentSessionDrive(PSModuleInfo module)
                foreach (KeyValuePair<string, List<ProviderInfo>> pList in providers)
                    Dbg.Assert(pList.Value != null, "There should never be a null list of entries in the provider table");
                    foreach (ProviderInfo pInfo in pList.Value)
                        string implTypeAssemblyLocation = pInfo.ImplementingType.Assembly.Location;
                        if (implTypeAssemblyLocation.Equals(module.Path, StringComparison.OrdinalIgnoreCase))
                            foreach (PSDriveInfo dInfo in Context.TopLevelSessionState.GetDrivesForProvider(pInfo.FullName))
                                if (dInfo == SessionState.Drive.Current)
        private static void GetAllNestedModules(PSModuleInfo module, ref List<PSModuleInfo> nestedModulesWithNoCircularReference)
            List<PSModuleInfo> nestedModules = new List<PSModuleInfo>();
                foreach (var nestedModule in module.NestedModules)
                    if (!nestedModulesWithNoCircularReference.Contains(nestedModule))
                        nestedModulesWithNoCircularReference.Add(nestedModule);
                        nestedModules.Add(nestedModule);
                foreach (PSModuleInfo child in nestedModules)
                    GetAllNestedModules(child, ref nestedModulesWithNoCircularReference);
        /// Returns a map from a module to the list of modules that require it.
        private Dictionary<PSModuleInfo, List<PSModuleInfo>> GetRequiredDependencies()
            Dictionary<PSModuleInfo, List<PSModuleInfo>> requiredDependencies = new Dictionary<PSModuleInfo, List<PSModuleInfo>>();
            foreach (PSModuleInfo module in Context.Modules.GetModules(new string[] { "*" }, false))
                foreach (PSModuleInfo requiredModule in module.RequiredModules)
                    List<PSModuleInfo> requiredByList = null;
                    if (!requiredDependencies.TryGetValue(requiredModule, out requiredByList))
                        requiredDependencies.Add(requiredModule, requiredByList = new List<PSModuleInfo>());
                    requiredByList.Add(module);
            return requiredDependencies;
        /// Reports an error if no modules were removed...
            // Write an error record if specific modules were to be removed.
            // By specific, we mean either a name sting with no wildcards or
            // or a PSModuleInfo object. If the removal request only includes patterns
            // then we won't write the error.
            if (_numberRemoved == 0 && !MyInvocation.BoundParameters.ContainsKey("WhatIf"))
                bool hasWildcards = true;
                bool isEngineModule = true;
                foreach (string n in _name)
                    if (!InitialSessionState.IsEngineModule(n))
                        isEngineModule = false;
                    if (!WildcardPattern.ContainsWildcardCharacters(n))
                        hasWildcards = false;
                if (FullyQualifiedName != null && (FullyQualifiedName.Any(static moduleSpec => !InitialSessionState.IsEngineModule(moduleSpec.Name))))
                if (!isEngineModule && (!hasWildcards || _moduleInfo.Length != 0 || (FullyQualifiedName != null && FullyQualifiedName.Length != 0)))
                    string message = StringUtil.Format(Modules.NoModulesRemoved);
                    ErrorRecord er = new ErrorRecord(invalidOp, "Modules_NoModulesRemoved",
                        ErrorCategory.ResourceUnavailable, null);
    #endregion Remove-Module

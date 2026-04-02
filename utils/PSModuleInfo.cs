    /// Class describing a PowerShell module...
    public sealed class PSModuleInfo
        internal const string DynamicModulePrefixString = "__DynamicModule_";
        private static readonly ReadOnlyDictionary<string, TypeDefinitionAst> s_emptyTypeDefinitionDictionary =
            new ReadOnlyDictionary<string, TypeDefinitionAst>(new Dictionary<string, TypeDefinitionAst>(StringComparer.OrdinalIgnoreCase));
        // This dictionary doesn't include ExportedTypes from nested modules.
        private ReadOnlyDictionary<string, TypeDefinitionAst> _exportedTypeDefinitionsNoNested { get; set; }
        private static readonly HashSet<string> s_scriptModuleExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        internal static void SetDefaultDynamicNameAndPath(PSModuleInfo module)
            string gs = Guid.NewGuid().ToString();
            module.Path = gs;
            module.Name = "__DynamicModule_" + gs;
        /// This object describes a PowerShell module...
        /// <param name="path">The absolute path to the module.</param>
        /// <param name="sessionState">The module's sessionstate object - this may be null if the module is a dll.</param>
        internal PSModuleInfo(string path, ExecutionContext context, SessionState sessionState)
            : this(null, path, context, sessionState)
        /// <param name="name">The name to use for the module. If null, get it from the path name.</param>
        /// <param name="languageMode">Language mode for script based modules.</param>
        internal PSModuleInfo(string name, string path, ExecutionContext context, SessionState sessionState, PSLanguageMode? languageMode)
            : this(name, path, context, sessionState)
            LanguageMode = languageMode;
        internal PSModuleInfo(string name, string path, ExecutionContext context, SessionState sessionState)
                string resolvedPath = ModuleCmdletBase.GetResolvedPath(path, context);
                // The resolved path might be null if we're building a dynamic module and the path
                // is just a GUID, not an actual path that can be resolved.
                Path = resolvedPath ?? path;
            if (sessionState != null)
                sessionState.Internal.Module = this;
            // Use the name of basename of the path as the module name if no module name is supplied.
            Name = name ?? ModuleIntrinsics.GetModuleName(Path);
        /// Default constructor to create an empty module info.
        public PSModuleInfo(bool linkToGlobal)
            : this(LocalPipeline.GetExecutionContextFromTLS(), linkToGlobal)
        internal PSModuleInfo(ExecutionContext context, bool linkToGlobal)
                throw new InvalidOperationException("PSModuleInfo");
            SetDefaultDynamicNameAndPath(this);
            SessionState = new SessionState(context, true, linkToGlobal);
            SessionState.Internal.Module = this;
        /// Construct a PSModuleInfo instance initializing it from a scriptblock instead of a script file.
        /// <param name="scriptBlock">The scriptblock to use to initialize the module.</param>
        public PSModuleInfo(ScriptBlock scriptBlock)
            // Get the ExecutionContext from the thread.
            SessionState = new SessionState(context, true, true);
            LanguageMode = scriptBlock.LanguageMode;
            SessionStateInternal oldSessionState = context.EngineSessionState;
                context.EngineSessionState = SessionState.Internal;
                // Set the PSScriptRoot variable...
                context.SetVariable(SpecialVariables.PSScriptRootVarPath, Path);
                scriptBlock = scriptBlock.Clone();
                scriptBlock.SessionState = SessionState;
                Pipe outputPipe = new Pipe { NullPipe = true };
                scriptBlock.InvokeWithPipe(
                    invocationInfo: null
                context.EngineSessionState = oldSessionState;
        /// Specifies the language mode for script based modules.
        internal PSLanguageMode? LanguageMode
        } = PSLanguageMode.FullLanguage;
        /// Set to true when script module automatically exports all functions by default.
        internal bool ModuleAutoExportsAllFunctions { get; set; }
        internal bool ModuleHasPrivateMembers { get; set; }
        /// True if the module had errors during loading.
        internal bool HadErrorsLoading { get; set; }
        /// ToString() implementation which returns the name of the module.
        /// <returns>The name of the module.</returns>
        /// Get/set whether to log Pipeline Execution Detail events.
        public bool LogPipelineExecutionDetails { get; set; } = false;
        /// The name of this module.
        /// Sets the name property of the PSModuleInfo object.
        /// <param name="name">The name to set it to.</param>
        internal void SetName(string name)
        /// The path to the file that defined this module...
        public string Path { get; internal set; } = string.Empty;
        /// If the module is a binary module or a script module that defines
        /// classes, this property if a reference to the assembly, otherwise
        /// it is null.
        public Assembly ImplementingAssembly { get; internal set; }
        /// If this is a script module, then this property will contain
        /// the PowerShell source text that was used to define this module.
        public string Definition
            get { return _definitionExtent == null ? string.Empty : _definitionExtent.Text; }
        internal IScriptExtent _definitionExtent;
        /// A description of this module...
            set { _description = value ?? string.Empty; }
        /// The guid for this module if one was defined in the module manifest.
        public Guid Guid { get; private set; }
        internal void SetGuid(Guid guid)
            Guid = guid;
        /// The HelpInfo for this module if one was defined in the module manifest.
        public string HelpInfoUri { get; private set; }
        internal bool IsWindowsPowerShellCompatModule { get; set; }
        internal void SetHelpInfoUri(string uri)
            HelpInfoUri = uri;
        /// Get the module base directory for this module. For modules loaded via a module
        /// manifest, this will be the directory containing the manifest file rather than
        /// the directory containing the actual module file. This is particularly useful
        /// when loading a GAC'ed assembly.
        public string ModuleBase
                return _moduleBase ??= !string.IsNullOrEmpty(Path) ? IO.Path.GetDirectoryName(Path) : string.Empty;
        internal void SetModuleBase(string moduleBase)
            _moduleBase = moduleBase;
        private string _moduleBase;
        /// This value is set from the PrivateData member in the module manifest.
        /// It allows implementor specific data to be passed to the module
        /// via the manifest file.
                _privateData = value;
                SetPSDataPropertiesFromPrivateData();
        private object _privateData = null;
        private void SetPSDataPropertiesFromPrivateData()
            // Reset the old values of PSData properties.
            _tags.Clear();
            ReleaseNotes = null;
            LicenseUri = null;
            ProjectUri = null;
            IconUri = null;
            if (_privateData is Hashtable hashData && hashData["PSData"] is Hashtable psData)
                var tagsValue = psData["Tags"];
                if (tagsValue is object[] tags && tags.Length > 0)
                    foreach (var tagString in tags.OfType<string>())
                        AddToTags(tagString);
                else if (tagsValue is string tag)
                    AddToTags(tag);
                if (psData["LicenseUri"] is string licenseUri)
                    LicenseUri = GetUriFromString(licenseUri);
                if (psData["ProjectUri"] is string projectUri)
                    ProjectUri = GetUriFromString(projectUri);
                if (psData["IconUri"] is string iconUri)
                    IconUri = GetUriFromString(iconUri);
                ReleaseNotes = psData["ReleaseNotes"] as string;
        private static Uri GetUriFromString(string uriString)
            Uri uri = null;
            if (uriString != null)
                // try creating the Uri object
                // Ignoring the return value from Uri.TryCreate(), as uri value will be null on false or valid uri object on true.
                Uri.TryCreate(uriString, UriKind.Absolute, out uri);
        /// Get the experimental features declared in this module.
        public IEnumerable<ExperimentalFeature> ExperimentalFeatures { get; internal set; } = Utils.EmptyReadOnlyCollection<ExperimentalFeature>();
        /// Tags of this module.
        public IEnumerable<string> Tags
            get { return _tags; }
        private readonly List<string> _tags = new List<string>();
        internal void AddToTags(string tag)
            _tags.Add(tag);
        /// ProjectUri of this module.
        public Uri ProjectUri { get; internal set; }
        /// IconUri of this module.
        public Uri IconUri { get; internal set; }
        /// LicenseUri of this module.
        public Uri LicenseUri { get; internal set; }
        /// ReleaseNotes of this module.
        public string ReleaseNotes { get; internal set; }
        /// Repository SourceLocation of this module.
        public Uri RepositorySourceLocation { get; internal set; }
        /// The version of this module.
        public Version Version { get; private set; } = new Version(0, 0);
        /// Sets the module version.
        /// <param name="version">The version to set...</param>
        internal void SetVersion(Version version)
            Version = version;
        /// True if the module was compiled (i.e. a .DLL) instead of
        /// being in PowerShell script...
        public ModuleType ModuleType { get; private set; } = ModuleType.Script;
        /// This module as being a compiled module...
        internal void SetModuleType(ModuleType moduleType) { ModuleType = moduleType; }
        /// Module Author.
        /// Controls the module access mode...
        public ModuleAccessMode AccessMode
                return _accessMode;
                if (_accessMode == ModuleAccessMode.Constant)
                _accessMode = value;
        private ModuleAccessMode _accessMode = ModuleAccessMode.ReadWrite;
        /// CLR Version.
        /// Company Name.
        /// Copyright.
        /// .NET Framework Version.
        internal Collection<string> DeclaredFunctionExports = null;
        internal Collection<string> DeclaredCmdletExports = null;
        internal Collection<string> DeclaredAliasExports = null;
        internal Collection<string> DeclaredVariableExports = null;
        internal List<string> DetectedFunctionExports = new List<string>();
        internal List<string> DetectedCmdletExports = new List<string>();
        internal Dictionary<string, string> DetectedAliasExports = new Dictionary<string, string>();
        /// Lists the functions exported by this module...
        public Dictionary<string, FunctionInfo> ExportedFunctions
                Dictionary<string, FunctionInfo> exports = new Dictionary<string, FunctionInfo>(StringComparer.OrdinalIgnoreCase);
                // If the module is not binary, it may also have functions...
                if (DeclaredFunctionExports != null)
                    if (DeclaredFunctionExports.Count == 0)
                        return exports;
                    foreach (string fn in DeclaredFunctionExports)
                        FunctionInfo tempFunction = new FunctionInfo(fn, ScriptBlock.EmptyScriptBlock, null) { Module = this };
                        exports[fn] = tempFunction;
                else if (SessionState != null)
                    // If there is no session state object associated with this list,
                    // just return a null list of exports...
                    if (SessionState.Internal.ExportedFunctions != null)
                        foreach (FunctionInfo fi in SessionState.Internal.ExportedFunctions)
                            if (!exports.ContainsKey(fi.Name))
                                exports[ModuleCmdletBase.AddPrefixToCommandName(fi.Name, fi.Prefix)] = fi;
                    foreach (var detectedExport in DetectedFunctionExports)
                        if (!exports.ContainsKey(detectedExport))
                            FunctionInfo tempFunction = new FunctionInfo(detectedExport, ScriptBlock.EmptyScriptBlock, null) { Module = this };
                            exports[detectedExport] = tempFunction;
        private static bool IsScriptModuleFile(string path)
            return ext != null && s_scriptModuleExtensions.Contains(ext);
        /// Lists the types (PowerShell classes, enums, interfaces) exported by this module.
        /// This returns ASTs for types, created in parse time.
        public ReadOnlyDictionary<string, TypeDefinitionAst> GetExportedTypeDefinitions()
            // We cache exported types from this modules, but not from nestedModules,
            // because we may not have NestedModules list populated on the first call.
            // TODO(sevoroby): it may harm perf a little bit. Can we sort it out?
            if (_exportedTypeDefinitionsNoNested == null)
                if (RootModule == null)
                    if (this.Path != null)
                        rootedPath = this.Path;
                    rootedPath = IO.Path.Combine(this.ModuleBase, this.RootModule);
                // ExternalScriptInfo.GetScriptBlockAst() uses a cache layer to avoid re-parsing.
                CreateExportedTypeDefinitions(rootedPath != null && IsScriptModuleFile(rootedPath) && IO.File.Exists(rootedPath) ?
                    (new ExternalScriptInfo(rootedPath, rootedPath)).GetScriptBlockAst() : null);
            var res = new Dictionary<string, TypeDefinitionAst>(StringComparer.OrdinalIgnoreCase);
            foreach (var nestedModule in this.NestedModules)
                if (nestedModule == this)
                    // Circular nested modules could happen with ill-organized module structure.
                    // For example, module folder 'test' has two files: 'test.psd1' and 'test.psm1', and 'test.psd1' has the following content:
                    //    "@{ ModuleVersion = '0.0.1'; RootModule = 'test'; NestedModules = @('test') }"
                    // Then, 'Import-Module test.psd1 -PassThru' will return a ModuleInfo object with circular nested modules.
                foreach (var typePairs in nestedModule.GetExportedTypeDefinitions())
                    res[typePairs.Key] = typePairs.Value;
            foreach (var typePairs in _exportedTypeDefinitionsNoNested)
            return new ReadOnlyDictionary<string, TypeDefinitionAst>(res);
        /// Create ExportedTypeDefinitions from ast.
        /// <param name="moduleContentScriptBlockAsts"></param>
        internal void CreateExportedTypeDefinitions(ScriptBlockAst moduleContentScriptBlockAsts)
            if (moduleContentScriptBlockAsts == null)
                this._exportedTypeDefinitionsNoNested = s_emptyTypeDefinitionDictionary;
                this._exportedTypeDefinitionsNoNested = new ReadOnlyDictionary<string, TypeDefinitionAst>(
                    moduleContentScriptBlockAsts.FindAll(static a => (a is TypeDefinitionAst), false)
                        .OfType<TypeDefinitionAst>()
                        .ToDictionary(static a => a.Name, StringComparer.OrdinalIgnoreCase));
        internal void AddDetectedTypeExports(List<TypeDefinitionAst> typeDefinitions)
                typeDefinitions.ToDictionary(static a => a.Name, StringComparer.OrdinalIgnoreCase));
        /// Prefix.
        /// Add function to the fixed exports list.
        /// <param name="name">The function to add.</param>
        internal void AddDetectedFunctionExport(string name)
            Dbg.Assert(name != null, "AddDetectedFunctionExport should not be called with a null value");
            if (!DetectedFunctionExports.Contains(name))
                DetectedFunctionExports.Add(name);
        public Dictionary<string, CmdletInfo> ExportedCmdlets
                Dictionary<string, CmdletInfo> exports = new Dictionary<string, CmdletInfo>(StringComparer.OrdinalIgnoreCase);
                if (DeclaredCmdletExports != null)
                    if (DeclaredCmdletExports.Count == 0)
                    foreach (string fn in DeclaredCmdletExports)
                        CmdletInfo tempCmdlet = new CmdletInfo(fn, null, null, null, null) { Module = this };
                        exports[fn] = tempCmdlet;
                else if ((CompiledExports != null) && (CompiledExports.Count > 0))
                    foreach (CmdletInfo cmdlet in CompiledExports)
                        exports[cmdlet.Name] = cmdlet;
                    foreach (string detectedExport in DetectedCmdletExports)
                            CmdletInfo tempCmdlet = new CmdletInfo(detectedExport, null, null, null, null) { Module = this };
                            exports[detectedExport] = tempCmdlet;
        /// Add CmdletInfo to the fixed exports list...
        /// <param name="cmdlet">The cmdlet to add...</param>
        internal void AddDetectedCmdletExport(string cmdlet)
            Dbg.Assert(cmdlet != null, "AddDetectedCmdletExport should not be called with a null value");
            if (!DetectedCmdletExports.Contains(cmdlet))
                DetectedCmdletExports.Add(cmdlet);
        /// Gets the aggregated list of visible commands exported from the module. If there are two
        /// commands of different types exported with the same name (e.g. alias 'foo' and cmdlet 'foo') the
        /// combined dictionary will only contain the highest precedence cmdlet (e.g. the alias 'foo' since
        /// aliases shadow cmdlets.
        public Dictionary<string, CommandInfo> ExportedCommands
                Dictionary<string, CommandInfo> exports = new Dictionary<string, CommandInfo>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, CmdletInfo> cmdlets = this.ExportedCmdlets;
                    foreach (var cmdlet in cmdlets)
                        exports[cmdlet.Key] = cmdlet.Value;
                Dictionary<string, FunctionInfo> functions = this.ExportedFunctions;
                if (functions != null)
                    foreach (var function in functions)
                        exports[function.Key] = function.Value;
                Dictionary<string, AliasInfo> aliases = this.ExportedAliases;
                    foreach (var alias in aliases)
                        exports[alias.Key] = alias.Value;
        internal void AddExportedCmdlet(CmdletInfo cmdlet)
            Dbg.Assert(cmdlet != null, "AddExportedCmdlet should not be called with a null value");
            _compiledExports.Add(cmdlet);
        /// Return the merged list of exported cmdlets. This is necessary
        /// because you may have a binary module with nested modules where
        /// some cmdlets come from the module and others come from the nested
        /// module. We need to consolidate the list so it can properly be constrained.
        internal List<CmdletInfo> CompiledExports
                // If this module has a session state instance and there are any
                // exported cmdlets in the session state, migrate them to the
                // module info _compiledCmdlets entry.
                if (SessionState != null && SessionState.Internal.ExportedCmdlets != null &&
                    SessionState.Internal.ExportedCmdlets.Count > 0)
                    foreach (CmdletInfo ci in SessionState.Internal.ExportedCmdlets)
                        _compiledExports.Add(ci);
                    SessionState.Internal.ExportedCmdlets.Clear();
                return _compiledExports;
        private readonly List<CmdletInfo> _compiledExports = new List<CmdletInfo>();
        /// Add AliasInfo to the fixed exports list...
        /// <param name="aliasInfo">The cmdlet to add...</param>
        internal void AddExportedAlias(AliasInfo aliasInfo)
            Dbg.Assert(aliasInfo != null, "AddExportedAlias should not be called with a null value");
            CompiledAliasExports.Add(aliasInfo);
        /// Return the merged list of exported aliases. This is necessary
        /// some aliases come from the module and others come from the nested
        internal List<AliasInfo> CompiledAliasExports { get; } = new List<AliasInfo>();
        /// FileList.
        public IEnumerable<string> FileList
            get { return _fileList; }
        private List<string> _fileList = new List<string>();
        internal void AddToFileList(string file)
            _fileList.Add(file);
        /// Lists the PowerShell editions this module is compatible with. This should
        /// reflect the module manifest the module was loaded with, or if no manifest was given
        /// or the key was not in the manifest, this should be an empty collection. This
        /// property is never null.
        public IEnumerable<string> CompatiblePSEditions
        private readonly List<string> _compatiblePSEditions = new List<string>();
        internal void AddToCompatiblePSEditions(string psEdition)
            _compatiblePSEditions.Add(psEdition);
        internal void AddToCompatiblePSEditions(IEnumerable<string> psEditions)
            _compatiblePSEditions.AddRange(psEditions);
        /// Describes whether the module was considered compatible at load time.
        /// Any module not on the System32 module path should have this as true.
        /// Modules loaded from the System32 module path will have this as true if they
        /// have declared edition compatibility with PowerShell 6+. Currently, this field
        /// is true for all non-psd1 module files, when it should not be. Being able to
        /// load psm1/dll modules from the System32 module path without needing to skip
        /// the edition check is considered a bug and should be fixed.
        internal bool IsConsideredEditionCompatible { get; set; } = true;
        /// ModuleList.
        public IEnumerable<object> ModuleList
        private Collection<object> _moduleList = new Collection<object>();
        internal void AddToModuleList(object m)
            _moduleList.Add(m);
        /// Returns the list of child modules of this module. This will only
        /// be non-empty for module manifests.
        public ReadOnlyCollection<PSModuleInfo> NestedModules
                return _readonlyNestedModules ??= new ReadOnlyCollection<PSModuleInfo>(_nestedModules);
        private ReadOnlyCollection<PSModuleInfo> _readonlyNestedModules;
        /// Add a module to the list of child modules.
        /// <param name="nestedModule">The module to add.</param>
        internal void AddNestedModule(PSModuleInfo nestedModule)
            AddModuleToList(nestedModule, _nestedModules);
        private readonly List<PSModuleInfo> _nestedModules = new List<PSModuleInfo>();
        /// PowerShell Host Name.
        /// PowerShell Host Version.
        /// PowerShell Version.
        /// Processor Architecture.
        /// Scripts to Process.
        public IEnumerable<string> Scripts
        private List<string> _scripts = new List<string>();
        internal void AddScript(string s)
            _scripts.Add(s);
        /// Required Assemblies.
        public IEnumerable<string> RequiredAssemblies
        private Collection<string> _requiredAssemblies = new Collection<string>();
        internal void AddRequiredAssembly(string assembly)
            _requiredAssemblies.Add(assembly);
        /// Returns the list of required modules of this module. This will only
        public ReadOnlyCollection<PSModuleInfo> RequiredModules
                return _readonlyRequiredModules ??= new ReadOnlyCollection<PSModuleInfo>(_requiredModules);
        private ReadOnlyCollection<PSModuleInfo> _readonlyRequiredModules;
        /// Add a module to the list of required modules.
        /// <param name="requiredModule">The module to add.</param>
        internal void AddRequiredModule(PSModuleInfo requiredModule)
            AddModuleToList(requiredModule, _requiredModules);
        private List<PSModuleInfo> _requiredModules = new List<PSModuleInfo>();
        /// Returns the list of required modules specified in the module manifest of this module. This will only
        internal ReadOnlyCollection<ModuleSpecification> RequiredModulesSpecification
                return _readonlyRequiredModulesSpecification ??= new ReadOnlyCollection<ModuleSpecification>(_requiredModulesSpecification);
        private ReadOnlyCollection<ModuleSpecification> _readonlyRequiredModulesSpecification;
        /// Add a module to the list of required modules specification.
        /// <param name="requiredModuleSpecification">The module to add.</param>
        internal void AddRequiredModuleSpecification(ModuleSpecification requiredModuleSpecification)
            _requiredModulesSpecification.Add(requiredModuleSpecification);
        private List<ModuleSpecification> _requiredModulesSpecification = new List<ModuleSpecification>();
        /// Root Module.
        /// This member is used to copy over the RootModule in case the module is a manifest module
        /// This is so that only ModuleInfo for modules with type=Manifest have RootModule populated.
        internal string RootModuleForManifest
        /// Add a module to the list of modules, avoiding adding duplicates.
        private static void AddModuleToList(PSModuleInfo module, List<PSModuleInfo> moduleList)
            Dbg.Assert(module != null, "AddModuleToList should not be called with a null value");
            // Add the module if it isn't already there...
            foreach (PSModuleInfo m in moduleList)
                if (m.Path.Equals(module.Path, StringComparison.OrdinalIgnoreCase))
            moduleList.Add(module);
        internal static readonly string[] _builtinVariables = new string[] { "_", "this", "input", "args", "true", "false", "null",
            "PSDefaultParameterValues", "Error", "PSScriptRoot", "PSCommandPath", "MyInvocation", "ExecutionContext", "StackTrace" };
        /// Lists the variables exported by this module.
        public Dictionary<string, PSVariable> ExportedVariables
                Dictionary<string, PSVariable> exportedVariables = new Dictionary<string, PSVariable>(StringComparer.OrdinalIgnoreCase);
                if ((DeclaredVariableExports != null) && (DeclaredVariableExports.Count > 0))
                    foreach (string fn in DeclaredVariableExports)
                        exportedVariables[fn] = null;
                    // just return a null list of exports. This will be true if the
                    // module is a compiled module.
                    if (SessionState == null || SessionState.Internal.ExportedVariables == null)
                        return exportedVariables;
                    foreach (PSVariable v in SessionState.Internal.ExportedVariables)
                        exportedVariables[v.Name] = v;
        /// Lists the aliases exported by this module.
        public Dictionary<string, AliasInfo> ExportedAliases
                Dictionary<string, AliasInfo> exportedAliases = new Dictionary<string, AliasInfo>(StringComparer.OrdinalIgnoreCase);
                if ((DeclaredAliasExports != null) && (DeclaredAliasExports.Count > 0))
                    foreach (string fn in DeclaredAliasExports)
                        AliasInfo tempAlias = new AliasInfo(fn, null, null) { Module = this };
                        exportedAliases[fn] = tempAlias;
                else if ((CompiledAliasExports != null) && (CompiledAliasExports.Count > 0))
                    foreach (AliasInfo ai in CompiledAliasExports)
                        exportedAliases[ai.Name] = ai;
                    // There is no session state object associated with this list.
                    if (SessionState == null)
                        // Check if we detected any
                        if (DetectedAliasExports.Count > 0)
                            foreach (var pair in DetectedAliasExports)
                                string detectedExport = pair.Key;
                                if (!exportedAliases.ContainsKey(detectedExport))
                                    AliasInfo tempAlias = new AliasInfo(detectedExport, pair.Value, null) { Module = this };
                                    exportedAliases[detectedExport] = tempAlias;
                            return exportedAliases;
                        // We have a session state
                        foreach (AliasInfo ai in SessionState.Internal.ExportedAliases)
        /// Add alias to the detected alias list.
        /// <param name="name">The alias to add.</param>
        /// <param name="value">The command it resolves to.</param>
        internal void AddDetectedAliasExport(string name, string value)
            Dbg.Assert(name != null, "AddDetectedAliasExport should not be called with a null value");
            DetectedAliasExports[name] = value;
        public ReadOnlyCollection<string> ExportedDscResources
                return _declaredDscResourceExports != null
                    ? new ReadOnlyCollection<string>(_declaredDscResourceExports)
                    : Utils.EmptyReadOnlyCollection<string>();
        internal Collection<string> _declaredDscResourceExports = null;
        /// The session state instance associated with this module.
        public SessionState SessionState { get; set; }
        /// Returns a new scriptblock bound to this module instance.
        /// <param name="scriptBlockToBind">The original scriptblock.</param>
        /// <returns>The new bound scriptblock.</returns>
        public ScriptBlock NewBoundScriptBlock(ScriptBlock scriptBlockToBind)
            return NewBoundScriptBlock(scriptBlockToBind, context);
        internal ScriptBlock NewBoundScriptBlock(ScriptBlock scriptBlockToBind, ExecutionContext context)
            if (SessionState == null || context == null)
                throw PSTraceSource.NewInvalidOperationException(Modules.InvalidOperationOnBinaryModule);
            ScriptBlock newsb;
            lock (context.EngineSessionState)
                    newsb = scriptBlockToBind.Clone();
                    newsb.SessionState = SessionState;
            return newsb;
        /// Invoke a scriptblock in the context of this module...
        /// <param name="sb">The scriptblock to invoke.</param>
        /// <param name="args">Arguments to the scriptblock.</param>
        /// <returns>The result of the invocation.</returns>
        public object Invoke(ScriptBlock sb, params object[] args)
            // Temporarily set the scriptblocks session state to be the
            // modules...
            SessionStateInternal oldSessionState = sb.SessionStateInternal;
                sb.SessionStateInternal = SessionState.Internal;
                result = sb.InvokeReturnAsIs(args);
                // and restore the scriptblocks session state...
                sb.SessionStateInternal = oldSessionState;
        /// This routine allows you to get access variable objects in the callers module
        /// or from the toplevel sessionstate if there is no calling module.
        public PSVariable GetVariableFromCallersModule(string variableName)
            ArgumentException.ThrowIfNullOrEmpty(variableName);
            SessionState callersSessionState = null;
            foreach (var sf in context.Debugger.GetCallStack())
                var frameModule = sf.InvocationInfo.MyCommand.Module;
                if (frameModule == null)
                if (frameModule.SessionState != SessionState)
                    callersSessionState = sf.InvocationInfo.MyCommand.Module.SessionState;
            if (callersSessionState != null)
                return callersSessionState.Internal.GetVariable(variableName);
                return context.TopLevelSessionState.GetVariable(variableName);
        /// Copies the local variables in the caller's cope into the module...
        internal void CaptureLocals()
            var tuple = context.EngineSessionState.CurrentScope.LocalsTuple;
            IEnumerable<PSVariable> variables = context.EngineSessionState.CurrentScope.Variables.Values;
                var result = new Dictionary<string, PSVariable>();
                tuple.GetVariableTable(result, false);
                variables = result.Values.Concat(variables);
            foreach (PSVariable v in variables)
                    // Only copy simple mutable variables...
                    if (v.Options == ScopedItemOptions.None && v is not NullVariable)
                        PSVariable newVar = new PSVariable(v.Name, v.Value, v.Options, v.Description);
                        // The variable is already defined/set in the scope, and that means the attributes
                        // have already been checked if it was needed, so we don't do it again.
                        newVar.AddParameterAttributesNoChecks(v.Attributes);
                        SessionState.Internal.NewVariable(newVar, false);
        /// Build a custom object out of this module...
        /// <returns>A custom object.</returns>
        public PSObject AsCustomObject()
            foreach (KeyValuePair<string, FunctionInfo> entry in this.ExportedFunctions)
                FunctionInfo func = entry.Value;
                if (func != null)
                    PSScriptMethod sm = new PSScriptMethod(func.Name, func.ScriptBlock);
                    obj.Members.Add(sm);
            foreach (KeyValuePair<string, PSVariable> entry in this.ExportedVariables)
                    PSVariableProperty sm = new PSVariableProperty(var);
        /// Optional script that is going to be called just before Remove-Module cmdlet removes the module.
        public ScriptBlock OnRemove { get; set; }
        /// The list of Format files imported by this module.
        public ReadOnlyCollection<string> ExportedFormatFiles { get; private set; } = new ReadOnlyCollection<string>(new List<string>());
        internal void SetExportedFormatFiles(ReadOnlyCollection<string> files)
            ExportedFormatFiles = files;
        /// The list of types files imported by this module.
        public ReadOnlyCollection<string> ExportedTypeFiles { get; private set; } = new ReadOnlyCollection<string>(new List<string>());
        internal void SetExportedTypeFiles(ReadOnlyCollection<string> files)
            ExportedTypeFiles = files;
        /// Implements deep copy of a PSModuleInfo instance.
        /// <returns>A new PSModuleInfo instance.</returns>
        public PSModuleInfo Clone()
            PSModuleInfo clone = (PSModuleInfo)this.MemberwiseClone();
            clone._fileList = new List<string>(this.FileList);
            clone._moduleList = new Collection<object>(_moduleList);
            foreach (var n in this.NestedModules)
                clone.AddNestedModule(n);
            clone._readonlyNestedModules = new ReadOnlyCollection<PSModuleInfo>(this.NestedModules);
            clone._readonlyRequiredModules = new ReadOnlyCollection<PSModuleInfo>(this.RequiredModules);
            clone._readonlyRequiredModulesSpecification = new ReadOnlyCollection<ModuleSpecification>(this.RequiredModulesSpecification);
            clone._requiredAssemblies = new Collection<string>(_requiredAssemblies);
            clone._requiredModulesSpecification = new List<ModuleSpecification>();
            clone._requiredModules = new List<PSModuleInfo>();
            foreach (var r in _requiredModules)
                clone.AddRequiredModule(r);
            foreach (var r in _requiredModulesSpecification)
                clone.AddRequiredModuleSpecification(r);
            clone._scripts = new List<string>(this.Scripts);
            clone.SessionState = this.SessionState;
        /// Enables or disables the appdomain module path cache.
        public static bool UseAppDomainLevelModuleCache { get; set; }
        /// Clear out the appdomain-level module path cache.
        public static void ClearAppDomainLevelModulePathCache()
            s_appdomainModulePathCache.Clear();
        /// A method available in debug mode providing access to the module path cache.
        public static object GetAppDomainLevelModuleCache()
            return s_appdomainModulePathCache;
        /// Look up a module in the appdomain wide module path cache.
        /// <param name="moduleName">Module name to look up.</param>
        /// <returns>The path to the matched module.</returns>
        internal static string ResolveUsingAppDomainLevelModuleCache(string moduleName)
            if (s_appdomainModulePathCache.TryGetValue(moduleName, out path))
        /// Add an entry to the appdomain level module path cache. By default, if there already is an entry
        /// it won't be replace. If force is specified, then it will be updated. \
        internal static void AddToAppDomainLevelModuleCache(string moduleName, string path, bool force)
                s_appdomainModulePathCache.AddOrUpdate(moduleName, path, (modulename, oldPath) => path);
                s_appdomainModulePathCache.TryAdd(moduleName, path);
        /// If there is an entry for the named module in the appdomain level module path cache, remove it.
        /// <param name="moduleName">The name of the module to remove from the cache.</param>
        /// <returns>True if the module was remove.</returns>
        internal static bool RemoveFromAppDomainLevelCache(string moduleName)
            string outString;
            return s_appdomainModulePathCache.TryRemove(moduleName, out outString);
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> s_appdomainModulePathCache =
            new System.Collections.Concurrent.ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    /// Indicates the type of a module.
    public enum ModuleType
        /// Indicates that this is a script module (a powershell file with a .PSM1 extension)
        Script = 0,
        /// Indicates that this is compiled .dll containing cmdlet definitions.
        Binary = 1,
        /// Indicates that this module entry was derived from a module manifest and
        /// may have child modules.
        Manifest,
        /// Indicates that this is cmdlets-over-objects module (a powershell file with a .CDXML extension)
        Cim,
    /// Defines the possible access modes for a module...
    public enum ModuleAccessMode
        /// The default access mode for the module.
        ReadWrite = 0,
        /// The module is readonly and can only be removed with -force.
        ReadOnly = 1,
        /// The module cannot be removed.
        Constant = 2
    /// An EqualityComparer to compare 2 PSModuleInfo instances. 2 PSModuleInfos are
    /// considered equal if their Name,Guid and Version are equal.
    internal sealed class PSModuleInfoComparer : IEqualityComparer<PSModuleInfo>
        public bool Equals(PSModuleInfo x, PSModuleInfo y)
            // Check whether the compared objects reference the same data.
            if (object.ReferenceEquals(x, y)) return true;
            // Check whether any of the compared objects is null.
            if (x is null || y is null)
            bool result = string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase) &&
                (x.Guid == y.Guid) && (x.Version == y.Version);
        public int GetHashCode(PSModuleInfo obj)
            unchecked // Overflow is fine, just wrap
                    // picking two different prime numbers to avoid collisions
                    result = 23;
                    if (obj.Name != null)
                        result = result * 17 + obj.Name.GetHashCode();
                    if (obj.Guid != Guid.Empty)
                        result = result * 17 + obj.Guid.GetHashCode();
                    if (obj.Version != null)
                        result = result * 17 + obj.Version.GetHashCode();

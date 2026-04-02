    #region Test-ModuleManifest
    /// This cmdlet takes a module manifest and validates the contents...
    [Cmdlet(VerbsDiagnostic.Test, "ModuleManifest", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096900")]
    public sealed class TestModuleManifestCommand : ModuleCmdletBase
        /// Creates an instance of the Test-ModuleManifest command.
        public TestModuleManifestCommand()
            // Test-ModuleManifest reads a manifest with ModuleCmdletBase.LoadModuleManifest().
            // This will error on an edition-incompatible manifest loaded from the System32 path,
            // unless BaseSkipEditionCheck is true. Since Test-ModuleManifest shouldn't care about
            // module edition (it just tests manifest validity), we always want to set this rather
            // than provide it as a switch on the cmdlet.
            BaseSkipEditionCheck = true;
        /// The output path for the generated file...
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        /// Implements the record processing for this cmdlet.
                if (Context.EngineSessionState.IsProviderLoaded(Context.ProviderNames.FileSystem))
                        SessionState.Path.GetResolvedProviderPathFromPSPath(_path, out provider);
                    filePaths.Add(_path);
                string message = StringUtil.Format(Modules.ModuleNotFound, _path);
                    ErrorCategory.ResourceUnavailable, _path);
                throw InterpreterError.NewInterpreterException(_path, typeof(RuntimeException),
                    null, "FileOpenError", ParserStrings.FileOpenError, provider.FullName);
                throw InterpreterError.NewInterpreterException(filePaths, typeof(RuntimeException),
                    null, "AmbiguousPath", ParserStrings.AmbiguousPath);
            string filePath = filePaths[0];
            ExternalScriptInfo scriptInfo = null;
            string ext = System.IO.Path.GetExtension(filePath);
            if (ext.Equals(StringLiterals.PowerShellDataFileExtension, StringComparison.OrdinalIgnoreCase))
                // Create a script info for loading the file...
                scriptInfo = GetScriptInfoForFile(filePath, out scriptName, false);
                // we should reserve the Context.ModuleBeingProcessed unchanged after loadModuleManifest(), otherwise the module won't be importable next time.
                PSModuleInfo module;
                    ManifestProcessingFlags.WriteErrors | ManifestProcessingFlags.WriteWarnings /* but don't stop on first error and don't load elements */,
                        // Validate file existence
                        if (module.RequiredAssemblies != null)
                            foreach (string requiredAssembliespath in module.RequiredAssemblies)
                                if (!IsValidFilePath(requiredAssembliespath, module, true) && !IsValidGacAssembly(requiredAssembliespath))
                                    string errorMsg = StringUtil.Format(Modules.InvalidRequiredAssembliesInModuleManifest, requiredAssembliespath, filePath);
                                    var errorRecord = new ErrorRecord(new DirectoryNotFoundException(errorMsg), "Modules_InvalidRequiredAssembliesInModuleManifest",
                                            ErrorCategory.ObjectNotFound, _path);
                        if (!HasValidRootModule(module))
                            string errorMsg = StringUtil.Format(Modules.InvalidModuleManifest, module.RootModule, filePath);
                            var errorRecord = new ErrorRecord(new ArgumentException(errorMsg), "Modules_InvalidRootModuleInModuleManifest",
                        bool containerErrors = false;
                        LoadModuleManifestData(scriptInfo, ManifestProcessingFlags.WriteErrors | ManifestProcessingFlags.WriteWarnings, out data, out localizedData, ref containerErrors);
                        ModuleSpecification[] nestedModules;
                        GetScalarFromData(data, scriptInfo.Path, "NestedModules", ManifestProcessingFlags.WriteErrors | ManifestProcessingFlags.WriteWarnings, out nestedModules);
                            foreach (ModuleSpecification nestedModule in nestedModules)
                                if (!IsValidFilePath(nestedModule.Name, module, true)
                                    && !IsValidFilePath(nestedModule.Name + StringLiterals.PowerShellILAssemblyExtension, module, true)
                                    && !IsValidFilePath(nestedModule.Name + StringLiterals.PowerShellNgenAssemblyExtension, module, true)
                                    && !IsValidFilePath(nestedModule.Name + StringLiterals.PowerShellILExecutableExtension, module, true)
                                    && !IsValidFilePath(nestedModule.Name + StringLiterals.PowerShellModuleFileExtension, module, true)
                                    && !IsValidGacAssembly(nestedModule.Name))
                                    Collection<PSModuleInfo> modules = GetModuleIfAvailable(nestedModule);
                                    if (modules.Count == 0)
                                        string errorMsg = StringUtil.Format(Modules.InvalidNestedModuleinModuleManifest, nestedModule.Name, filePath);
                                        var errorRecord = new ErrorRecord(new DirectoryNotFoundException(errorMsg), "Modules_InvalidNestedModuleinModuleManifest",
                        GetScalarFromData(data, scriptInfo.Path, "RequiredModules", ManifestProcessingFlags.WriteErrors | ManifestProcessingFlags.WriteWarnings, out requiredModules);
                                var modules = GetModule(new[] { requiredModule.Name }, all: false, refresh: true);
                                    string errorMsg = StringUtil.Format(Modules.InvalidRequiredModulesinModuleManifest, requiredModule.Name, filePath);
                                    var errorRecord = new ErrorRecord(new DirectoryNotFoundException(errorMsg), "Modules_InvalidRequiredModulesinModuleManifest",
                        string[] fileListPaths;
                        GetScalarFromData(data, scriptInfo.Path, "FileList", ManifestProcessingFlags.WriteErrors | ManifestProcessingFlags.WriteWarnings, out fileListPaths);
                        if (fileListPaths != null)
                            foreach (string fileListPath in fileListPaths)
                                if (!IsValidFilePath(fileListPath, module, true))
                                    string errorMsg = StringUtil.Format(Modules.InvalidFilePathinModuleManifest, fileListPath, filePath);
                                    var errorRecord = new ErrorRecord(new DirectoryNotFoundException(errorMsg), "Modules_InvalidFilePathinModuleManifest",
                        ModuleSpecification[] moduleListModules;
                        GetScalarFromData(data, scriptInfo.Path, "ModuleList", ManifestProcessingFlags.WriteErrors | ManifestProcessingFlags.WriteWarnings, out moduleListModules);
                        if (moduleListModules != null)
                            foreach (ModuleSpecification moduleListModule in moduleListModules)
                                var modules = GetModule(new[] { moduleListModule.Name }, all: false, refresh: true);
                                    string errorMsg = StringUtil.Format(Modules.InvalidModuleListinModuleManifest, moduleListModule.Name, filePath);
                                    var errorRecord = new ErrorRecord(new DirectoryNotFoundException(errorMsg), "Modules_InvalidModuleListinModuleManifest",
                        if (module.CompatiblePSEditions.Any())
                            // The CompatiblePSEditions module manifest key is supported only on PowerShell version '5.1' or higher.
                            // Ensure that PowerShellVersion module manifest key value is '5.1' or higher.
                            var minimumRequiredPowerShellVersion = new Version(5, 1);
                            if ((module.PowerShellVersion == null) || module.PowerShellVersion < minimumRequiredPowerShellVersion)
                                string errorMsg = StringUtil.Format(Modules.InvalidPowerShellVersionInModuleManifest, filePath);
                                var errorRecord = new ErrorRecord(new ArgumentException(errorMsg), "Modules_InvalidPowerShellVersionInModuleManifest", ErrorCategory.InvalidArgument, _path);
                    parent = Directory.GetParent(filePath);
                if (parent != null && Version.TryParse(parent.Name, out version))
                    if (!version.Equals(module.Version))
                        string message = StringUtil.Format(Modules.InvalidModuleManifestVersion, filePath, module.Version.ToString(), parent.FullName);
                        ErrorRecord er = new ErrorRecord(ioe, "Modules_InvalidModuleManifestVersion",
                    WriteVerbose(Modules.ModuleVersionEqualsToVersionFolder);
                string message = StringUtil.Format(Modules.InvalidModuleManifestPath, filePath);
        // All module extensions except ".psd1" are valid RootModule extensions
        private static readonly IReadOnlyList<string> s_validRootModuleExtensions = ModuleIntrinsics.PSModuleExtensions
            .Where(static ext => !string.Equals(ext, StringLiterals.PowerShellDataFileExtension, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        /// Checks whether the RootModule field of a module is valid or not.
        /// Valid root modules are:
        ///  - null
        ///  - Empty string
        ///  - A valid non-psd1 module file (psm1, cdxml, xaml, dll), as name with extension, name without extension, or path.
        /// <param name="module">The module for which we want to check the validity of the root module.</param>
        /// <returns>True if the root module is valid, false otherwise.</returns>
        private bool HasValidRootModule(PSModuleInfo module)
            // Empty/null root modules are allowed
            if (string.IsNullOrEmpty(module.RootModule))
            // GAC assemblies are allowed
            if (IsValidGacAssembly(module.RootModule))
            // Check for extensions
            string rootModuleExt = System.IO.Path.GetExtension(module.RootModule);
            if (!string.IsNullOrEmpty(rootModuleExt))
                // Check that the root module's extension is an allowed one
                if (!s_validRootModuleExtensions.Contains(rootModuleExt, StringComparer.OrdinalIgnoreCase))
                // Check the file path of the full root module
                return IsValidFilePath(module.RootModule, module, verifyPathScope: true);
            // We have no extension, so we need to check all of them
            foreach (string extension in s_validRootModuleExtensions)
                if (IsValidFilePath(module.RootModule + extension, module, verifyPathScope: true))
        /// Check if the given path is valid.
        /// <param name="verifyPathScope"></param>
        private bool IsValidFilePath(string path, PSModuleInfo module, bool verifyPathScope)
                if (!System.IO.Path.IsPathRooted(path))
                    // we assume the relative path is under module scope, otherwise we will throw error anyway.
                    path = System.IO.Path.GetFullPath(module.ModuleBase + System.IO.Path.DirectorySeparatorChar + path);
                // resolve the path so slashes are in the right direction
                CmdletProviderContext cmdContext = new CmdletProviderContext(this);
                Collection<PathInfo> pathInfos = SessionState.Path.GetResolvedPSPathFromPSPath(path, cmdContext);
                if (pathInfos.Count != 1)
                    string message = StringUtil.Format(Modules.InvalidModuleManifestPath, path);
                    ErrorRecord er = new ErrorRecord(ioe, "Modules_InvalidModuleManifestPath", ErrorCategory.InvalidArgument, path);
                // `Path` returns the PSProviderPath which is fully qualified to the provider and the filesystem APIs
                // don't understand this.  Instead `ProviderPath` returns the path that the FileSystemProvider understands.
                path = pathInfos[0].ProviderPath;
                // First, we validate if the path does exist.
                if (!File.Exists(path) && !Directory.Exists(path))
                // Then, we validate if the path is under module scope
                if (verifyPathScope && !System.IO.Path.GetFullPath(path).StartsWith(System.IO.Path.GetFullPath(module.ModuleBase), StringComparison.OrdinalIgnoreCase))
                if (exception is ArgumentException || exception is ArgumentNullException || exception is NotSupportedException || exception is PathTooLongException || exception is ItemNotFoundException)
        /// Check if the given string is a valid gac assembly.
        private static bool IsValidGacAssembly(string assemblyName)
            string gacPath = System.Environment.GetEnvironmentVariable("windir") + "\\Microsoft.NET\\assembly";
            string assemblyFile = assemblyName;
            string ngenAssemblyFile = assemblyName;
            if (!assemblyName.EndsWith(StringLiterals.PowerShellILAssemblyExtension, StringComparison.OrdinalIgnoreCase))
                assemblyFile = assemblyName + StringLiterals.PowerShellILAssemblyExtension;
                ngenAssemblyFile = assemblyName + StringLiterals.PowerShellNgenAssemblyExtension;
                return Directory.EnumerateFiles(gacPath, assemblyFile, SearchOption.AllDirectories).Any()
                    || Directory.EnumerateFiles(gacPath, ngenAssemblyFile, SearchOption.AllDirectories).Any();

    internal static class Constants
        public const string PSModulePathEnvVar = "PSModulePath";
    /// Encapsulates the basic module operations for a PowerShell engine instance...
    public class ModuleIntrinsics
        /// Tracer for module analysis.
        [TraceSource("Modules", "Module loading and analysis")]
        internal static readonly PSTraceSource Tracer = PSTraceSource.GetTracer("Modules", "Module loading and analysis");
        // The %WINDIR%\System32\WindowsPowerShell\v1.0\Modules module path,
        // to load forward compatible Windows PowerShell modules from
        private static readonly string s_windowsPowerShellPSHomeModulePath =
            Path.Combine(System.Environment.SystemDirectory, "WindowsPowerShell", "v1.0", "Modules");
        static ModuleIntrinsics()
            // Initialize the module path.
            SetModulePath();
        internal ModuleIntrinsics(ExecutionContext context)
            ModuleTable = new Dictionary<string, PSModuleInfo>(StringComparer.OrdinalIgnoreCase);
        // Holds the module collection...
        internal Dictionary<string, PSModuleInfo> ModuleTable { get; }
        private const int MaxModuleNestingDepth = 10;
        internal void IncrementModuleNestingDepth(PSCmdlet cmdlet, string path)
            if (++ModuleNestingDepth > MaxModuleNestingDepth)
                string message = StringUtil.Format(Modules.ModuleTooDeeplyNested, path, MaxModuleNestingDepth);
                ErrorRecord er = new ErrorRecord(ioe, "Modules_ModuleTooDeeplyNested",
                    ErrorCategory.InvalidOperation, path);
                cmdlet.ThrowTerminatingError(er);
        internal void DecrementModuleNestingCount()
            --ModuleNestingDepth;
        internal int ModuleNestingDepth { get; private set; }
        /// Create a new module object from a scriptblock specifying the path to set for the module.
        /// <param name="name">The name of the module.</param>
        /// <param name="path">The path where the module is rooted.</param>
        /// <param name="scriptBlock">
        /// ScriptBlock that is executed to initialize the module...
        /// The arguments to pass to the scriptblock used to initialize the module
        /// <param name="ss">The session state instance to use for this module - may be null.</param>
        /// <param name="results">The results produced from evaluating the scriptblock.</param>
        /// <returns>The newly created module info object.</returns>
        internal PSModuleInfo CreateModule(string name, string path, ScriptBlock scriptBlock, SessionState ss, out List<object> results, params object[] arguments)
            return CreateModuleImplementation(name, path, scriptBlock, null, ss, null, out results, arguments);
        /// Create a new module object from a ScriptInfo object.
        /// <param name="scriptInfo">The script info to use to create the module.</param>
        /// <param name="scriptPosition">The position for the command that loaded this module.</param>
        /// <param name="arguments">Optional arguments to pass to the script while executing.</param>
        /// <param name="privateData">The private data to use for this module - may be null.</param>
        /// <returns>The constructed module object.</returns>
        internal PSModuleInfo CreateModule(string path, ExternalScriptInfo scriptInfo, IScriptExtent scriptPosition, SessionState ss, object privateData, params object[] arguments)
            List<object> result;
            return CreateModuleImplementation(ModuleIntrinsics.GetModuleName(path), path, scriptInfo, scriptPosition, ss, privateData, out result, arguments);
        /// Create a new module object from code specifying the path to set for the module.
        /// <param name="path">The path to use for the module root.</param>
        /// <param name="moduleCode">
        /// The code to use to create the module. This can be one of ScriptBlock, string
        /// or ExternalScriptInfo
        /// Arguments to pass to the module scriptblock during evaluation.
        /// The results of the evaluation of the scriptblock.
        /// The position of the caller of this function so you can tell where the call
        /// to Import-Module (or whatever) occurred. This can be null.
        /// <returns>The created module.</returns>
        private PSModuleInfo CreateModuleImplementation(string name, string path, object moduleCode, IScriptExtent scriptPosition, SessionState ss, object privateData, out List<object> result, params object[] arguments)
            ScriptBlock sb;
            // By default the top-level scope in a session state object is the global scope for the instance.
            // For modules, we need to set its global scope to be another scope object and, chain the top
            // level scope for this sessionstate instance to be the parent. The top level scope for this ss is the
            // script scope for the ss.
            // Allocate the session state instance for this module.
            ss ??= new SessionState(_context, true, true);
            // Now set up the module's session state to be the current session state
            PSModuleInfo module = new PSModuleInfo(name, path, _context, ss);
            ss.Internal.Module = module;
            module.PrivateData = privateData;
            bool setExitCode = false;
                _context.EngineSessionState = ss.Internal;
                // Build the scriptblock at this point so the references to the module
                // context are correct...
                ExternalScriptInfo scriptInfo = moduleCode as ExternalScriptInfo;
                    sb = scriptInfo.ScriptBlock;
                    _context.Debugger.RegisterScriptFile(scriptInfo);
                    sb = moduleCode as ScriptBlock;
                    if (sb != null)
                        PSLanguageMode? moduleLanguageMode = sb.LanguageMode;
                        sb = sb.Clone();
                        sb.LanguageMode = moduleLanguageMode;
                        sb.SessionState = ss;
                    else if (moduleCode is string sbText)
                        sb = ScriptBlock.Create(_context, sbText);
                if (sb == null)
                sb.SessionStateInternal = ss.Internal;
                module.LanguageMode = sb.LanguageMode;
                InvocationInfo invocationInfo = new InvocationInfo(scriptInfo, scriptPosition);
                // Save the module string
                module._definitionExtent = sb.Ast.Extent;
                var ast = sb.Ast;
                // The variables set in the interpreted case get set by InvokeWithPipe in the compiled case.
                Diagnostics.Assert(_context.SessionState.Internal.CurrentScope.LocalsTuple == null,
                                    "No locals tuple should have been created yet.");
                List<object> resultList = new List<object>();
                    Pipe outputPipe = new Pipe(resultList);
                    // And run the scriptblock...
                    sb.InvokeWithPipe(
                        args: arguments ?? Array.Empty<object>());
                    exitCode = (int)ee.Argument;
                    setExitCode = true;
            if (setExitCode)
                _context.SetVariable(SpecialVariables.LastExitCodeVarPath, exitCode);
            module.ImplementingAssembly = sb.AssemblyDefiningPSTypes;
            // We force re-population of ExportedTypeDefinitions, now with the actual RuntimeTypes, created above.
            module.CreateExportedTypeDefinitions(sb.Ast as ScriptBlockAst);
        /// Allocate a new dynamic module then return a new scriptblock
        /// bound to the module instance.
        /// <param name="context">Context to use to create bounded script.</param>
        /// <param name="sb">The scriptblock to bind.</param>
        /// <param name="linkToGlobal">Whether it should be linked to the global session state or not.</param>
        /// <returns>A new scriptblock.</returns>
        internal ScriptBlock CreateBoundScriptBlock(ExecutionContext context, ScriptBlock sb, bool linkToGlobal)
            PSModuleInfo module = new PSModuleInfo(context, linkToGlobal);
            return module.NewBoundScriptBlock(sb, context);
        internal List<PSModuleInfo> GetModules(string[] patterns, bool all)
            return GetModuleCore(patterns, all, false);
        internal List<PSModuleInfo> GetExactMatchModules(string moduleName, bool all, bool exactMatch)
            moduleName ??= string.Empty;
            return GetModuleCore(new string[] { moduleName }, all, exactMatch);
        private List<PSModuleInfo> GetModuleCore(string[] patterns, bool all, bool exactMatch)
            string targetModuleName = null;
            List<WildcardPattern> wcpList = new List<WildcardPattern>();
            if (exactMatch)
                Dbg.Assert(patterns.Length == 1, "The 'patterns' should only contain one element when it is for an exact match");
                targetModuleName = patterns[0];
                patterns ??= new string[] { "*" };
                foreach (string pattern in patterns)
                    wcpList.Add(WildcardPattern.Get(pattern, WildcardOptions.IgnoreCase));
            List<PSModuleInfo> modulesMatched = new List<PSModuleInfo>();
            if (all)
                foreach (PSModuleInfo module in ModuleTable.Values)
                    // See if this is the requested module...
                    if ((exactMatch && module.Name.Equals(targetModuleName, StringComparison.OrdinalIgnoreCase)) ||
                        (!exactMatch && SessionStateUtilities.MatchesAnyWildcardPattern(module.Name, wcpList, false)))
                        modulesMatched.Add(module);
                // Create a joint list of local and global modules. Only report a module once.
                // Local modules are reported before global modules...
                Dictionary<string, bool> found = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                foreach (var pair in _context.EngineSessionState.ModuleTable)
                    string path = pair.Key;
                    PSModuleInfo module = pair.Value;
                        found[path] = true;
                if (_context.EngineSessionState != _context.TopLevelSessionState)
                    foreach (var pair in _context.TopLevelSessionState.ModuleTable)
                        if (!found.ContainsKey(path))
            return modulesMatched.OrderBy(static m => m.Name).ToList();
        internal List<PSModuleInfo> GetModules(ModuleSpecification[] fullyQualifiedName, bool all)
                foreach (var moduleSpec in fullyQualifiedName)
                        if (IsModuleMatchingModuleSpec(module, moduleSpec))
        /// Check if a given module info object matches a given module specification.
        /// <param name="moduleInfo">The module info object to check.</param>
        /// <param name="moduleSpec">The module specification to match the module info object against.</param>
        /// <param name="skipNameCheck">True if we should skip the name check on the module specification.</param>
        /// <returns>True if the module info object meets all the constraints on the module specification, false otherwise.</returns>
        internal static bool IsModuleMatchingModuleSpec(
            ModuleSpecification moduleSpec,
            bool skipNameCheck = false)
            return IsModuleMatchingModuleSpec(out ModuleMatchFailure matchFailureReason, moduleInfo, moduleSpec, skipNameCheck);
        /// <param name="matchFailureReason">The constraint that caused the match failure, if any.</param>
            out ModuleMatchFailure matchFailureReason,
            if (moduleSpec == null)
                matchFailureReason = ModuleMatchFailure.NullModuleSpecification;
            return IsModuleMatchingConstraints(
                out matchFailureReason,
                moduleInfo,
                skipNameCheck ? null : moduleSpec.Name,
                moduleSpec.Guid,
                moduleSpec.RequiredVersion,
                moduleSpec.Version,
                moduleSpec.MaximumVersion == null ? null : ModuleCmdletBase.GetMaximumVersion(moduleSpec.MaximumVersion));
        /// Check if a given module info object matches the given constraints.
        /// Constraints given as null are ignored.
        /// <param name="name">The name or normalized absolute path of the expected module.</param>
        /// <param name="guid">The guid of the expected module.</param>
        /// <param name="requiredVersion">The required version of the expected module.</param>
        /// <param name="minimumVersion">The minimum required version of the expected module.</param>
        /// <param name="maximumVersion">The maximum required version of the expected module.</param>
        /// <returns>True if the module info object matches all given constraints, false otherwise.</returns>
        internal static bool IsModuleMatchingConstraints(
            string name = null,
            Guid? guid = null,
            Version requiredVersion = null,
            Version minimumVersion = null,
            Version maximumVersion = null)
                guid,
                requiredVersion,
                minimumVersion,
                maximumVersion);
        /// <param name="matchFailureReason">The reason for the module constraint match failing.</param>
            Guid? guid,
            Version maximumVersion)
            // Define that a null module does not meet any constraints
                matchFailureReason = ModuleMatchFailure.NullModule;
            return AreModuleFieldsMatchingConstraints(
                moduleInfo.Name,
                moduleInfo.Path,
                moduleInfo.Guid,
                moduleInfo.Version,
                maximumVersion
        /// Check that given module fields meet any given constraints.
        /// <param name="moduleName">The name of the module to check.</param>
        /// <param name="modulePath">The path of the module to check.</param>
        /// <param name="moduleGuid">The GUID of the module to check.</param>
        /// <param name="moduleVersion">The version of the module to check.</param>
        /// <param name="requiredName">The name or normalized absolute path the module must have, if any.</param>
        /// <param name="requiredGuid">The GUID the module must have, if any.</param>
        /// <param name="requiredVersion">The exact version the module must have, if any.</param>
        /// <param name="minimumRequiredVersion">The minimum version the module may have, if any.</param>
        /// <param name="maximumRequiredVersion">The maximum version the module may have, if any.</param>
        /// <returns>True if the module parameters match all given constraints, false otherwise.</returns>
        internal static bool AreModuleFieldsMatchingConstraints(
            string moduleName = null,
            string modulePath = null,
            Guid? moduleGuid = null,
            Version moduleVersion = null,
            string requiredName = null,
            Guid? requiredGuid = null,
            Version minimumRequiredVersion = null,
            Version maximumRequiredVersion = null)
                moduleGuid,
                requiredName,
                requiredGuid,
                minimumRequiredVersion,
                maximumRequiredVersion);
        /// <param name="matchFailureReason">The reason the match failed, if any.</param>
            Guid? moduleGuid,
            Version moduleVersion,
            string requiredName,
            Guid? requiredGuid,
            Version minimumRequiredVersion,
            Version maximumRequiredVersion)
            // If a name is required, check that it matches.
            // A required module name may also be an absolute path, so check it against the given module's path as well.
            if (requiredName != null
                && !requiredName.Equals(moduleName, StringComparison.OrdinalIgnoreCase)
                && !MatchesModulePath(modulePath, requiredName))
                matchFailureReason = ModuleMatchFailure.Name;
            // If a GUID is required, check it matches
            if (requiredGuid != null && !requiredGuid.Equals(moduleGuid))
                matchFailureReason = ModuleMatchFailure.Guid;
            // Check the versions
            return IsVersionMatchingConstraints(out matchFailureReason, moduleVersion, requiredVersion, minimumRequiredVersion, maximumRequiredVersion);
        /// Check that a given module version matches the required or minimum/maximum version constraints.
        /// Null constraints are not checked.
        /// <param name="version">The module version to check. Must not be null.</param>
        /// <param name="requiredVersion">The version that the given version must be, if not null.</param>
        /// <param name="minimumVersion">The minimum version that the given version must be greater than or equal to, if not null.</param>
        /// <param name="maximumVersion">The maximum version that the given version must be less then or equal to, if not null.</param>
        /// True if the version matches the required version, or if it is absent, is between the minimum and maximum versions, and false otherwise.
        internal static bool IsVersionMatchingConstraints(
            Version version,
            return IsVersionMatchingConstraints(out ModuleMatchFailure matchFailureReason, version, requiredVersion, minimumVersion, maximumVersion);
        /// <param name="matchFailureReason">The reason why the match failed.</param>
            Dbg.Assert(version != null, $"Caller to verify that {nameof(version)} is not null");
            // If a RequiredVersion is given it overrides other version settings
            if (requiredVersion != null)
                matchFailureReason = ModuleMatchFailure.RequiredVersion;
                return requiredVersion.Equals(version);
            // Check the version is at least the minimum version
            if (minimumVersion != null && version < minimumVersion)
                matchFailureReason = ModuleMatchFailure.MinimumVersion;
            // Check the version is at most the maximum version
            if (maximumVersion != null && version > maximumVersion)
                matchFailureReason = ModuleMatchFailure.MaximumVersion;
            matchFailureReason = ModuleMatchFailure.None;
        /// Checks whether a given module path is the same as
        /// a required path.
        /// <param name="modulePath">The path of the module whose path to check. This must be the path to the module file (.psd1, .psm1, .dll, etc).</param>
        /// <param name="requiredPath">The path of the required module. This may be the module directory path or the file path. Only normalized absolute paths will work for this.</param>
        /// <returns>True if the module path matches the required path, false otherwise.</returns>
        internal static bool MatchesModulePath(string modulePath, string requiredPath)
            Dbg.Assert(requiredPath != null, $"Caller to verify that {nameof(requiredPath)} is not null");
            if (modulePath == null)
            const StringComparison strcmp = StringComparison.Ordinal;
            const StringComparison strcmp = StringComparison.OrdinalIgnoreCase;
            // We must check modulePath (e.g. /path/to/module/module.psd1) against several possibilities:
            // 1. "/path/to/module"                 - Module dir path
            // 2. "/path/to/module/module.psd1"     - Module root file path
            // 3. "/path/to/module/2.1/module.psd1" - Versioned module path
            // If the required module just matches the module path (case 1), we are done
            if (modulePath.Equals(requiredPath, strcmp))
            // At this point we are looking for the module directory (case 2 or 3).
            // We can some allocations here if module path doesn't sit under the required path
            // (the required path may still refer to some nested module though)
            if (!modulePath.StartsWith(requiredPath, strcmp))
            string moduleDirPath = Path.GetDirectoryName(modulePath);
            // The module itself may be in a versioned directory (case 3)
            if (Version.TryParse(Path.GetFileName(moduleDirPath), out _))
                moduleDirPath = Path.GetDirectoryName(moduleDirPath);
            return moduleDirPath.Equals(requiredPath, strcmp);
        /// Takes the name of a module as used in a module specification
        /// and either returns it as a simple name (if it was a simple name)
        /// or a fully qualified, PowerShell-resolved path.
        /// <param name="moduleName">The name or path of the module from the specification.</param>
        /// <param name="basePath">The path to base relative paths off.</param>
        /// <param name="executionContext">The current execution context.</param>
        /// The simple module name if the given one was simple,
        /// otherwise a fully resolved, absolute path to the module.
        /// 2018-11-09 rjmholt:
        /// There are several, possibly inconsistent, path handling mechanisms
        /// in the module cmdlets. After looking through all of them and seeing
        /// they all make some assumptions about their caller I wrote this method.
        /// Hopefully we can find a standard path resolution API to settle on.
        internal static string NormalizeModuleName(
            ExecutionContext executionContext)
            // Check whether the module is a path -- if not, it is a simple name and we just return it.
            if (!IsModuleNamePath(moduleName))
            // Standardize directory separators -- Path.IsPathRooted() will return false for "\path\here" on *nix and for "/path/there" on Windows
            moduleName = moduleName.Replace(StringLiterals.AlternatePathSeparator, StringLiterals.DefaultPathSeparator);
            // Note: Path.IsFullyQualified("\default\root") is false on Windows, but Path.IsPathRooted returns true
            if (!Path.IsPathRooted(moduleName))
                moduleName = Path.Join(basePath, moduleName);
            // Use the PowerShell filesystem provider to fully resolve the path
            // If there is a problem, null could be returned -- so default back to the pre-normalized path
            string normalizedPath = ModuleCmdletBase.GetResolvedPath(moduleName, executionContext)?.TrimEnd(StringLiterals.DefaultPathSeparator);
            // ModuleCmdletBase.GetResolvePath will return null in the unlikely event that it failed.
            // If it does, we return the fully qualified path generated before.
            return normalizedPath ?? Path.GetFullPath(moduleName);
        /// Check if a given module name is a path to a module rather than a simple name.
        /// <param name="moduleName">The module name to check.</param>
        /// <returns>True if the module name is a path, false otherwise.</returns>
        internal static bool IsModuleNamePath(string moduleName)
            return moduleName.Contains(StringLiterals.DefaultPathSeparator)
                || moduleName.Contains(StringLiterals.AlternatePathSeparator)
                || moduleName.Equals("..")
                || moduleName.Equals(".");
        internal static Version GetManifestModuleVersion(string manifestPath)
                        return moduleVersion;
            return new Version(0, 0);
        internal static Guid GetManifestGuid(string manifestPath)
                        PsUtils.ManifestGuidPropertyName);
                object guidValue = dataFileSetting["GUID"];
                if (guidValue != null)
                    Guid guidID;
                    if (LanguagePrimitives.TryConvertTo(guidValue, out guidID))
                        return guidID;
            return new Guid();
        internal static ExperimentalFeature[] GetExperimentalFeature(string manifestPath)
                        PsUtils.ManifestPrivateDataPropertyName);
                object privateData = dataFileSetting["PrivateData"];
                    object expFeatureValue = psData["ExperimentalFeatures"];
                    if (expFeatureValue != null &&
                        LanguagePrimitives.TryConvertTo(expFeatureValue, out Hashtable[] features) &&
                        features.Length > 0)
                        string moduleName = ModuleIntrinsics.GetModuleName(manifestPath);
                        var expFeatureList = new List<ExperimentalFeature>();
                            if (ExperimentalFeature.IsModuleFeatureName(featureName, moduleName))
                                expFeatureList.Add(new ExperimentalFeature(featureName, featureDescription, manifestPath,
                        return expFeatureList.ToArray();
            return Array.Empty<ExperimentalFeature>();
        // The extensions of all of the files that can be processed with Import-Module, put the ni.dll in front of .dll to have higher priority to be loaded.
        internal static readonly string[] PSModuleProcessableExtensions = new string[]
            StringLiterals.PowerShellScriptFileExtension,
            StringLiterals.PowerShellCmdletizationFileExtension,
        // A list of the extensions to check for implicit module loading and discovery, put the ni.dll in front of .dll to have higher priority to be loaded.
        internal static readonly string[] PSModuleExtensions = new string[]
        // A list of the extensions to check for required assemblies.
        internal static readonly string[] ProcessableAssemblyExtensions = new string[]
            StringLiterals.PowerShellILExecutableExtension
        /// Returns true if the extension is one of the module extensions...
        /// <param name="extension">The extension to check.</param>
        /// <returns>True if it was a module extension...</returns>
        internal static bool IsPowerShellModuleExtension(string extension)
            foreach (string ext in PSModuleProcessableExtensions)
                if (extension.Equals(ext, StringComparison.OrdinalIgnoreCase))
        /// Gets the module name from module path.
        /// <param name="path">The path to the module.</param>
        /// <returns>The module name.</returns>
        internal static string GetModuleName(string path)
            string fileName = path == null ? string.Empty : Path.GetFileName(path);
            if (!string.IsNullOrEmpty(ext) && IsPowerShellModuleExtension(ext))
                return fileName.Substring(0, fileName.Length - ext.Length);
                return fileName;
        /// Gets the personal module path.
        /// <returns>Personal module path.</returns>
        internal static string GetPersonalModulePath()
            return Platform.SelectProductNameForDirectory(Platform.XDG_Type.USER_MODULES);
            string myDocumentsPath = InternalTestHooks.SetMyDocumentsSpecialFolderToBlank ? string.Empty : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return string.IsNullOrEmpty(myDocumentsPath) ? null : Path.Combine(myDocumentsPath, Utils.ModuleDirectory);
        /// Gets the PSHome module path, as known as the "system wide module path" in windows powershell.
        /// <returns>The PSHome module path.</returns>
        internal static string GetPSHomeModulePath()
            if (s_psHomeModulePath != null)
                return s_psHomeModulePath;
                // Win8: 584267 Powershell Modules are listed twice in x86, and cannot be removed.
                // This happens because 'ModuleTable' uses Path as the key and x86 WinPS has "SysWOW64" in its $PSHOME.
                // Because of this, the module that is getting loaded during startup (through LocalRunspace) is using
                // "SysWow64" in the key. Later, when 'Import-Module' is called, it loads the module using ""System32"
                // in the key.
                // For the cross-platform PowerShell, a user can choose to install it under "C:\Windows\SysWOW64", and
                // thus it may have the same problem as described above. So we keep this line of code.
                psHome = psHome.ToLowerInvariant().Replace(@"\syswow64\", @"\system32\");
                Interlocked.CompareExchange(ref s_psHomeModulePath, Path.Combine(psHome, "Modules"), null);
        private static string s_psHomeModulePath;
        /// Get the module path that is shared among different users.
        /// It's known as "Program Files" module path in windows powershell.
        internal static string GetSharedModulePath()
            return Platform.SelectProductNameForDirectory(Platform.XDG_Type.SHARED_MODULES);
            string sharedModulePath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (!string.IsNullOrEmpty(sharedModulePath))
                sharedModulePath = Path.Combine(sharedModulePath, Utils.ModuleDirectory);
            return sharedModulePath;
        /// Get the path to the Windows PowerShell module directory under the
        /// System32 directory on Windows (the Windows PowerShell $PSHOME).
        /// <returns>The path of the Windows PowerShell system module directory.</returns>
        internal static string GetWindowsPowerShellPSHomeModulePath()
            if (!string.IsNullOrEmpty(InternalTestHooks.TestWindowsPowerShellPSHomeLocation))
                return InternalTestHooks.TestWindowsPowerShellPSHomeLocation;
            return s_windowsPowerShellPSHomeModulePath;
        /// Combine the PS system-wide module path and the DSC module path
        /// to get the system module paths.
        private static string CombineSystemModulePaths()
            string psHomeModulePath = GetPSHomeModulePath();
            string sharedModulePath = GetSharedModulePath();
            bool isPSHomePathNullOrEmpty = string.IsNullOrEmpty(psHomeModulePath);
            bool isSharedPathNullOrEmpty = string.IsNullOrEmpty(sharedModulePath);
            if (!isPSHomePathNullOrEmpty && !isSharedPathNullOrEmpty)
                return (sharedModulePath + Path.PathSeparator + psHomeModulePath);
            if (!isPSHomePathNullOrEmpty || !isSharedPathNullOrEmpty)
                return isPSHomePathNullOrEmpty ? sharedModulePath : psHomeModulePath;
        internal static string GetExpandedEnvironmentVariable(string name, EnvironmentVariableTarget target)
            string result = Environment.GetEnvironmentVariable(name, target);
            if (!string.IsNullOrEmpty(result))
        /// Adds paths to a 'combined path' string (like %Path% or %PSModulePath%) if they are not already there.
        /// <param name="basePath">Path string (like %Path% or %PSModulePath%).</param>
        /// <param name="pathToAdd">An individual path to add, or multiple paths separated by the path separator character.</param>
        /// <param name="insertPosition">-1 to append to the end; 0 to insert in the beginning of the string; etc...</param>
        /// <returns>Result string.</returns>
        private static string UpdatePath(string basePath, string pathToAdd, ref int insertPosition)
            // we don't support if any of the args are null - parent function should ensure this; empty values are ok
            Dbg.Assert(basePath != null, "basePath should not be null according to contract of the function");
            Dbg.Assert(pathToAdd != null, "pathToAdd should not be null according to contract of the function");
            // The 'pathToAdd' could be a 'combined path' (path-separator-separated).
            string[] newPaths = pathToAdd.Split(
                Path.PathSeparator,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (newPaths.Length is 0)
                // The 'pathToAdd' doesn't really contain any paths to add.
                return basePath;
            var result = new StringBuilder(basePath, capacity: basePath.Length + pathToAdd.Length + newPaths.Length);
            var addedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string[] initialPaths = basePath.Split(
            foreach (string p in initialPaths)
                // Remove the trailing directory separators.
                // Trailing white spaces were already removed by 'StringSplitOptions.TrimEntries'.
                addedPaths.Add(Path.TrimEndingDirectorySeparator(p));
            foreach (string subPathToAdd in newPaths)
                string normalizedPath = Path.TrimEndingDirectorySeparator(subPathToAdd);
                if (addedPaths.Contains(normalizedPath))
                    // The normalized sub path was already added - skip it.
                // The normalized sub path was not found - add it.
                if (insertPosition is -1 || insertPosition >= result.Length)
                    // Append the normalized sub path to the end.
                    if (result.Length > 0 && result[^1] != Path.PathSeparator)
                        result.Append(Path.PathSeparator);
                    result.Append(normalizedPath);
                    // Next insertion should happen at the end.
                    insertPosition = result.Length;
                    // Insert at the requested location.
                    // This is used by the user-specific module path, the shared module path (<Program Files> location), and the PSHome module path.
                    string strToInsert = normalizedPath + Path.PathSeparator;
                    result.Insert(insertPosition, strToInsert);
                    // Next insertion should happen after the just inserted string.
                    insertPosition += strToInsert.Length;
                // Add it to the set.
                addedPaths.Add(normalizedPath);
        /// The available module path scopes.
        public enum PSModulePathScope
            /// <summary>The users module path.</summary>
            User,
            /// <summary>The Builtin module path. This is where PowerShell is installed (PSHOME).</summary>
            Builtin,
            /// <summary>The machine module path. This is the shared location for all users of the system.</summary>
            Machine
        /// Retrieve the current PSModulePath for the specified scope.
        /// <param name="scope">The scope of module path to retrieve. This can be User, Builtin, or Machine.</param>
        /// <returns>The string representing the requested module path type.</returns>
        public static string GetPSModulePath(PSModulePathScope scope)
            if (scope == PSModulePathScope.User)
                return GetPersonalModulePath();
            else if (scope == PSModulePathScope.Builtin)
                return GetPSHomeModulePath();
                return GetSharedModulePath();
        /// Checks the various PSModulePath environment string and returns PSModulePath string as appropriate.
        public static string GetModulePath(string currentProcessModulePath, string hklmMachineModulePath, string hkcuUserModulePath)
            string personalModulePath = GetPersonalModulePath();
            string sharedModulePath = GetSharedModulePath(); // aka <Program Files> location
            string psHomeModulePath = GetPSHomeModulePath(); // $PSHome\Modules location
            // If the variable isn't set, then set it to the default value
            if (string.IsNullOrEmpty(currentProcessModulePath))  // EVT.Process does Not exist - really corner case
                // Handle the default case...
                if (string.IsNullOrEmpty(hkcuUserModulePath)) // EVT.User does Not exist -> set to <SpecialFolder.MyDocuments> location
                    currentProcessModulePath = personalModulePath; // = SpecialFolder.MyDocuments + Utils.ProductNameForDirectory + Utils.ModuleDirectory
                else // EVT.User exists -> set to EVT.User
                    currentProcessModulePath = hkcuUserModulePath; // = EVT.User
                if (string.IsNullOrEmpty(currentProcessModulePath))
                    currentProcessModulePath ??= string.Empty;
                    currentProcessModulePath += Path.PathSeparator;
                if (string.IsNullOrEmpty(hklmMachineModulePath)) // EVT.Machine does Not exist
                    currentProcessModulePath += CombineSystemModulePaths(); // += (SharedModulePath + $PSHome\Modules)
                    currentProcessModulePath += hklmMachineModulePath; // += EVT.Machine
            // EVT.Process exists
            // Now handle the case where the environment variable is already set.
                string personalModulePathToUse = string.IsNullOrEmpty(hkcuUserModulePath) ? personalModulePath : hkcuUserModulePath;
                string systemModulePathToUse = string.IsNullOrEmpty(hklmMachineModulePath) ? psHomeModulePath : hklmMachineModulePath;
                // Maintain order of the paths, but ahead of any existing paths:
                // personalModulePath
                // sharedModulePath
                // systemModulePath
                int insertIndex = 0;
                currentProcessModulePath = UpdatePath(currentProcessModulePath, personalModulePathToUse, ref insertIndex);
                currentProcessModulePath = UpdatePath(currentProcessModulePath, sharedModulePath, ref insertIndex);
                currentProcessModulePath = UpdatePath(currentProcessModulePath, systemModulePathToUse, ref insertIndex);
            return currentProcessModulePath;
        /// Checks if $env:PSModulePath is not set and sets it as appropriate. Note - because these
        /// strings go through the provider, we need to escape any wildcards before passing them
        /// along.
        internal static string GetModulePath()
            string currentModulePath = GetExpandedEnvironmentVariable(Constants.PSModulePathEnvVar, EnvironmentVariableTarget.Process);
            return currentModulePath;
        /// Returns a PSModulePath suitable for Windows PowerShell by removing PowerShell's specific
        /// paths from current PSModulePath.
        /// Returns appropriate PSModulePath for Windows PowerShell.
        internal static string GetWindowsPowerShellModulePath()
            string currentModulePath = GetModulePath();
            if (currentModulePath == null)
            // PowerShell specific paths including if set in powershell.config.json file we want to exclude
            var excludeModulePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                GetPersonalModulePath(),
                GetSharedModulePath(),
                GetPSHomeModulePath(),
                PowerShellConfig.Instance.GetModulePath(ConfigScope.AllUsers),
                PowerShellConfig.Instance.GetModulePath(ConfigScope.CurrentUser)
            var modulePathList = new List<string>();
            foreach (var path in currentModulePath.Split(';', StringSplitOptions.TrimEntries))
                if (!excludeModulePaths.Contains(path))
                    // make sure this module path is Not part of other PS Core installation
                    var possiblePwshDir = Path.GetDirectoryName(path);
                    if (string.IsNullOrEmpty(possiblePwshDir))
                        // i.e. module dir is in the drive root
                        modulePathList.Add(path);
                        if (!File.Exists(Path.Combine(possiblePwshDir, "pwsh.dll")))
            return string.Join(Path.PathSeparator, modulePathList);
        private static string SetModulePath()
            // if the current process and user env vars are the same, it means we need to append the machine one as it's incomplete.
            // Otherwise, the user modified it and we should use the process one.
            if (string.CompareOrdinal(GetExpandedEnvironmentVariable(Constants.PSModulePathEnvVar, EnvironmentVariableTarget.User), currentModulePath) == 0)
                string machineScopeValue = GetExpandedEnvironmentVariable(Constants.PSModulePathEnvVar, EnvironmentVariableTarget.Machine);
                currentModulePath = string.IsNullOrEmpty(currentModulePath)
                    ? machineScopeValue
                    : string.IsNullOrEmpty(machineScopeValue)
                        ? currentModulePath
                        : string.Concat(currentModulePath, Path.PathSeparator, machineScopeValue);
            string allUsersModulePath = PowerShellConfig.Instance.GetModulePath(ConfigScope.AllUsers);
            string personalModulePath = PowerShellConfig.Instance.GetModulePath(ConfigScope.CurrentUser);
            string newModulePathString = GetModulePath(currentModulePath, allUsersModulePath, personalModulePath);
            if (!string.IsNullOrEmpty(newModulePathString))
                Environment.SetEnvironmentVariable(Constants.PSModulePathEnvVar, newModulePathString);
            return newModulePathString;
        /// Get the current module path setting.
        /// <param name="includeSystemModulePath">
        /// Include The system wide module path ($PSHOME\Modules) even if it's not in PSModulePath.
        /// In V3-V5, we prepended this path during module auto-discovery which incorrectly preferred
        /// $PSHOME\Modules over user installed modules that might have a command that overrides
        /// a product-supplied command.
        /// For 5.1, we append $PSHOME\Modules in this case to avoid the rare case where PSModulePath
        /// does not contain the path, but a script depends on previous behavior.
        /// Note that appending is still a potential breaking change, but necessary to update in-box
        /// modules long term - e.g. when open sourcing a module and installing from the gallery.
        /// <returns>The module path as an array of strings.</returns>
        internal static IEnumerable<string> GetModulePath(bool includeSystemModulePath, ExecutionContext context)
            string modulePathString = Environment.GetEnvironmentVariable(Constants.PSModulePathEnvVar) ?? SetModulePath();
            HashSet<string> processedPathSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(modulePathString))
                foreach (string envPath in modulePathString.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
                    var processedPath = ProcessOneModulePath(context, envPath, processedPathSet);
                    if (processedPath != null)
                        yield return processedPath;
            if (includeSystemModulePath)
                var processedPath = ProcessOneModulePath(context, GetPSHomeModulePath(), processedPathSet);
        private static string ProcessOneModulePath(ExecutionContext context, string envPath, HashSet<string> processedPathSet)
            string trimmedenvPath = envPath.Trim();
            bool isUnc = Utils.PathIsUnc(trimmedenvPath);
            if (!isUnc)
                // if the path start with "filesystem::", remove it so we can test for URI and
                // also Directory.Exists (if the file system provider isn't actually loaded.)
                if (trimmedenvPath.StartsWith("filesystem::", StringComparison.OrdinalIgnoreCase))
                    trimmedenvPath = trimmedenvPath.Remove(0, 12 /*"filesystem::".Length*/);
                isUnc = Utils.PathIsUnc(trimmedenvPath);
            // If we have an unc, just return the value as resolving the path is expensive.
            if (isUnc)
                return trimmedenvPath;
            // We prefer using the file system provider to resolve paths so callers can avoid processing
            // duplicates, e.g. the following are all the same:
            //     a\b
            //     a\.\b
            //     a\b\
            // But if the file system provider isn't loaded, we will just check if the directory exists.
                IEnumerable<string> resolvedPaths = null;
                    resolvedPaths = context.SessionState.Path.GetResolvedProviderPathFromPSPath(
                        WildcardPattern.Escape(trimmedenvPath), out provider);
                    // silently skip directories that are not found
                    // silently skip drives that are not found
                    // silently skip invalid path
                    // NotSupportedException is thrown if path contains a colon (":") that is not part of a
                    // volume identifier (for example, "c:\" is Supported but not "c:\temp\Z:\invalidPath")
                if (provider != null && resolvedPaths != null && provider.NameEquals(context.ProviderNames.FileSystem))
                    var result = resolvedPaths.FirstOrDefault();
                    if (processedPathSet.Add(result))
            else if (Directory.Exists(trimmedenvPath))
        private static void SortAndRemoveDuplicates<T>(List<T> input, Func<T, string> keyGetter)
            Dbg.Assert(input is not null, "Caller should verify that input != null");
            input.Sort(
                (T x, T y) =>
                    string kx = keyGetter(x);
                    string ky = keyGetter(y);
                    return string.Compare(kx, ky, StringComparison.OrdinalIgnoreCase);
            string? previousKey = null;
            input.RemoveAll(ShouldRemove);
            bool ShouldRemove(T item)
                string currentKey = keyGetter(item);
                bool match = previousKey is not null
                    && currentKey.Equals(previousKey, StringComparison.OrdinalIgnoreCase);
                previousKey = currentKey;
        /// Mark stuff to be exported from the current environment using the various patterns.
        /// <param name="cmdlet">The cmdlet calling this method.</param>
        /// <param name="sessionState">The session state instance to do the exports on.</param>
        /// <param name="functionPatterns">Patterns describing the functions to export.</param>
        /// <param name="cmdletPatterns">Patterns describing the cmdlets to export.</param>
        /// <param name="aliasPatterns">Patterns describing the aliases to export.</param>
        /// <param name="variablePatterns">Patterns describing the variables to export.</param>
        /// <param name="doNotExportCmdlets">List of Cmdlets that will not be exported, even if they match in cmdletPatterns.</param>
        internal static void ExportModuleMembers(
            List<string> doNotExportCmdlets)
            // If this cmdlet is called, then mark that the export list should be used for exporting
            // module members...
            sessionState.UseExportList = true;
            if (functionPatterns != null)
                sessionState.FunctionsExported = true;
                if (PatternContainsWildcard(functionPatterns))
                    sessionState.FunctionsExportedWithWildcard = true;
                IDictionary<string, FunctionInfo> ft = sessionState.ModuleScope.FunctionTable;
                foreach (KeyValuePair<string, FunctionInfo> entry in ft)
                    // Skip AllScope functions
                    if ((entry.Value.Options & ScopedItemOptions.AllScope) != 0)
                    if (SessionStateUtilities.MatchesAnyWildcardPattern(entry.Key, functionPatterns, false))
                        sessionState.ExportedFunctions.Add(entry.Value);
                        string message = StringUtil.Format(Modules.ExportingFunction, entry.Key);
                SortAndRemoveDuplicates(sessionState.ExportedFunctions, static (FunctionInfo ci) => ci.Name);
            if (cmdletPatterns != null)
                IDictionary<string, List<CmdletInfo>> ft = sessionState.ModuleScope.CmdletTable;
                // Subset the existing cmdlet exports if there are any. This will be the case
                // if we're using ModuleToProcess to import a binary module which has nested modules.
                if (sessionState.Module.CompiledExports.Count > 0)
                    CmdletInfo[] copy = sessionState.Module.CompiledExports.ToArray();
                    sessionState.Module.CompiledExports.Clear();
                    foreach (CmdletInfo element in copy)
                        if (doNotExportCmdlets == null
                            || !doNotExportCmdlets.Exists(cmdletName => string.Equals(element.FullName, cmdletName, StringComparison.OrdinalIgnoreCase)))
                            if (SessionStateUtilities.MatchesAnyWildcardPattern(element.Name, cmdletPatterns, false))
                                string message = StringUtil.Format(Modules.ExportingCmdlet, element.Name);
                                // Copy the cmdlet info, changing the module association to be the current module...
                                CmdletInfo exportedCmdlet = new CmdletInfo(element.Name, element.ImplementingType,
                                    element.HelpFile, null, element.Context)
                                { Module = sessionState.Module };
                                Dbg.Assert(sessionState.Module != null, "sessionState.Module should not be null by the time we're exporting cmdlets");
                                sessionState.Module.CompiledExports.Add(exportedCmdlet);
                // And copy in any cmdlets imported from the nested modules...
                foreach (KeyValuePair<string, List<CmdletInfo>> entry in ft)
                    CmdletInfo cmdletToImport = entry.Value[0];
                        || !doNotExportCmdlets.Exists(cmdletName => string.Equals(cmdletToImport.FullName, cmdletName, StringComparison.OrdinalIgnoreCase)))
                        if (SessionStateUtilities.MatchesAnyWildcardPattern(entry.Key, cmdletPatterns, false))
                            string message = StringUtil.Format(Modules.ExportingCmdlet, entry.Key);
                            CmdletInfo exportedCmdlet = new CmdletInfo(cmdletToImport.Name, cmdletToImport.ImplementingType,
                                cmdletToImport.HelpFile, null, cmdletToImport.Context)
                SortAndRemoveDuplicates(sessionState.Module.CompiledExports, static (CmdletInfo ci) => ci.Name);
            if (variablePatterns != null)
                IDictionary<string, PSVariable> vt = sessionState.ModuleScope.Variables;
                foreach (KeyValuePair<string, PSVariable> entry in vt)
                    // The magic variables are always private as are all-scope variables...
                    if (entry.Value.IsAllScope || Array.IndexOf(PSModuleInfo._builtinVariables, entry.Key) != -1)
                    if (SessionStateUtilities.MatchesAnyWildcardPattern(entry.Key, variablePatterns, false))
                        string message = StringUtil.Format(Modules.ExportingVariable, entry.Key);
                        sessionState.ExportedVariables.Add(entry.Value);
                SortAndRemoveDuplicates(sessionState.ExportedVariables, static (PSVariable v) => v.Name);
            if (aliasPatterns != null)
                IEnumerable<AliasInfo> mai = sessionState.ModuleScope.AliasTable;
                // Subset the existing alias exports if there are any. This will be the case
                if (sessionState.Module.CompiledAliasExports.Count > 0)
                    AliasInfo[] copy = sessionState.Module.CompiledAliasExports.ToArray();
                    foreach (var element in copy)
                        if (SessionStateUtilities.MatchesAnyWildcardPattern(element.Name, aliasPatterns, false))
                            string message = StringUtil.Format(Modules.ExportingAlias, element.Name);
                            sessionState.ExportedAliases.Add(NewAliasInfo(element, sessionState));
                foreach (AliasInfo entry in mai)
                    // Skip allscope items...
                    if ((entry.Options & ScopedItemOptions.AllScope) != 0)
                    if (SessionStateUtilities.MatchesAnyWildcardPattern(entry.Name, aliasPatterns, false))
                        string message = StringUtil.Format(Modules.ExportingAlias, entry.Name);
                        sessionState.ExportedAliases.Add(NewAliasInfo(entry, sessionState));
                SortAndRemoveDuplicates(sessionState.ExportedAliases, static (AliasInfo ci) => ci.Name);
        /// Checks pattern list for wildcard characters.
        /// <param name="list">Pattern list.</param>
        /// <returns>True if pattern contains '*'.</returns>
        internal static bool PatternContainsWildcard(List<WildcardPattern> list)
                foreach (var item in list)
                    if (WildcardPattern.ContainsWildcardCharacters(item.Pattern))
        private static AliasInfo NewAliasInfo(AliasInfo alias, SessionStateInternal sessionState)
            Dbg.Assert(alias != null, "alias should not be null");
            Dbg.Assert(sessionState != null, "sessionState should not be null");
            Dbg.Assert(sessionState.Module != null, "sessionState.Module should not be null by the time we're exporting aliases");
            // Copy the alias info, changing the module association to be the current module...
            var aliasCopy = new AliasInfo(alias.Name, alias.Definition, alias.Context, alias.Options)
                Module = sessionState.Module
            return aliasCopy;
    /// Enumeration of reasons for a failure to match a module by constraints.
    internal enum ModuleMatchFailure
        /// <summary>Match did not fail.</summary>
        /// <summary>Match failed because the module was null.</summary>
        NullModule,
        /// <summary>Module name did not match.</summary>
        /// <summary>Module GUID did not match.</summary>
        Guid,
        /// <summary>Module version did not match the required version.</summary>
        RequiredVersion,
        /// <summary>Module version was lower than the minimum version.</summary>
        MinimumVersion,
        /// <summary>Module version was greater than the maximum version.</summary>
        MaximumVersion,
        /// <summary>The module specification passed in was null.</summary>
        NullModuleSpecification,
    /// Used by Modules/Snapins to provide a hook to the engine for startup initialization
    /// w.r.t compiled assembly loading.
    public interface IModuleAssemblyInitializer
        /// Gets called when assembly is loaded.
        void OnImport();
    /// Used by modules to provide a hook to the engine for cleanup on removal
    /// w.r.t. compiled assembly being removed.
    public interface IModuleAssemblyCleanup
        /// Gets called when the binary module is unloaded.
        void OnRemove(PSModuleInfo psModuleInfo);

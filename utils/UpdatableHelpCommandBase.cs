    /// The base class of all updatable help system cmdlets (Update-Help, Save-Help)
    public class UpdatableHelpCommandBase : PSCmdlet
        internal const string PathParameterSetName = "Path";
        internal const string LiteralPathParameterSetName = "LiteralPath";
        internal UpdatableHelpCommandType _commandType;
        internal UpdatableHelpSystem _helpSystem;
        internal bool _stopping;
        internal int activityId;
        private readonly Dictionary<string, UpdatableHelpExceptionContext> _exceptions;
        /// Specifies the languages to update.
        public CultureInfo[] UICulture
                CultureInfo[] result = null;
                if (_language != null)
                    result = new CultureInfo[_language.Length];
                    for (int index = 0; index < _language.Length; index++)
                        result[index] = new CultureInfo(_language[index]);
                _language = new string[value.Length];
                for (int index = 0; index < value.Length; index++)
                    _language[index] = value[index].Name;
        internal string[] _language;
            set { _credential = value; }
        internal PSCredential _credential;
        /// Directs System.Net.WebClient whether or not to use default credentials.
        public SwitchParameter UseDefaultCredentials
                return _useDefaultCredentials;
                _useDefaultCredentials = value;
        private bool _useDefaultCredentials = false;
        /// Forces the operation to complete.
        internal bool _force;
        /// Sets the scope to which help is saved.
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public UpdateHelpScope Scope
        /// Handles help system progress events.
        private void HandleProgressChanged(object sender, UpdatableHelpProgressEventArgs e)
            Debug.Assert(e.CommandType == UpdatableHelpCommandType.UpdateHelpCommand
                || e.CommandType == UpdatableHelpCommandType.SaveHelpCommand);
            string activity = (e.CommandType == UpdatableHelpCommandType.UpdateHelpCommand) ?
                HelpDisplayStrings.UpdateProgressActivityForModule : HelpDisplayStrings.SaveProgressActivityForModule;
            ProgressRecord progress = new ProgressRecord(activityId, StringUtil.Format(activity, e.ModuleName), e.ProgressStatus);
            progress.PercentComplete = e.ProgressPercent;
        private static readonly Dictionary<string, string> s_metadataCache;
        /// Static constructor
        /// NOTE: HelpInfoUri for core PowerShell modules are needed since they get loaded as snapins in a Remoting Endpoint.
        /// When we moved to modules in V3, we were not able to make this change as it was a risky change to make at that time.
        static UpdatableHelpCommandBase()
            s_metadataCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            // NOTE: The HelpInfoUri must be updated with each release.
            s_metadataCache.Add("Microsoft.PowerShell.Diagnostics", "https://aka.ms/powershell75-help");
            s_metadataCache.Add("Microsoft.PowerShell.Core", "https://aka.ms/powershell75-help");
            s_metadataCache.Add("Microsoft.PowerShell.Utility", "https://aka.ms/powershell75-help");
            s_metadataCache.Add("Microsoft.PowerShell.Host", "https://aka.ms/powershell75-help");
            s_metadataCache.Add("Microsoft.PowerShell.Management", "https://aka.ms/powershell75-help");
            s_metadataCache.Add("Microsoft.PowerShell.Security", "https://aka.ms/powershell75-help");
            s_metadataCache.Add("Microsoft.WSMan.Management", "https://aka.ms/powershell75-help");
        /// Checks if a module is a system module, a module is a system module
        /// if it exists in the metadata cache.
        /// <param name="module">Module name.</param>
        /// <returns>True if system module, false if not.</returns>
        internal static bool IsSystemModule(string module)
            return s_metadataCache.ContainsKey(module);
        /// <param name="commandType">Command type.</param>
        internal UpdatableHelpCommandBase(UpdatableHelpCommandType commandType)
            _commandType = commandType;
            _helpSystem = new UpdatableHelpSystem(this, _useDefaultCredentials);
            _exceptions = new Dictionary<string, UpdatableHelpExceptionContext>();
            _helpSystem.OnProgressChanged += HandleProgressChanged;
            activityId = Random.Shared.Next();
        private void ProcessSingleModuleObject(PSModuleInfo module, ExecutionContext context, Dictionary<Tuple<string, Version>, UpdatableHelpModuleInfo> helpModules, bool noErrors)
            if (InitialSessionState.IsEngineModule(module.Name) && !InitialSessionState.IsNestedEngineModule(module.Name))
                WriteDebug(StringUtil.Format("Found engine module: {0}, {1}.", module.Name, module.Guid));
                var keyTuple = new Tuple<string, Version>(module.Name, module.Version);
                if (!helpModules.ContainsKey(keyTuple))
                    helpModules.Add(keyTuple, new UpdatableHelpModuleInfo(module.Name, module.Guid,
                        Utils.GetApplicationBase(context.ShellID), s_metadataCache[module.Name]));
            else if (InitialSessionState.IsNestedEngineModule(module.Name))
            if (string.IsNullOrEmpty(module.HelpInfoUri))
                if (!noErrors)
                    ProcessException(module.Name, null, new UpdatableHelpSystemException(
                        "HelpInfoUriNotFound", StringUtil.Format(HelpDisplayStrings.HelpInfoUriNotFound),
                        ErrorCategory.NotSpecified, new Uri("HelpInfoUri", UriKind.Relative), null));
            if (!(module.HelpInfoUri.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || module.HelpInfoUri.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
                        "InvalidHelpInfoUriFormat", StringUtil.Format(HelpDisplayStrings.InvalidHelpInfoUriFormat, module.HelpInfoUri),
            var keyTuple2 = new Tuple<string, Version>(module.Name, module.Version);
            if (!helpModules.ContainsKey(keyTuple2))
                helpModules.Add(keyTuple2, new UpdatableHelpModuleInfo(module.Name, module.Guid, module.ModuleBase, module.HelpInfoUri));
        /// Gets a list of modules from the given pattern.
        /// <param name="pattern">Pattern to search.</param>
        /// <param name="fullyQualifiedName">Module Specification.</param>
        /// <param name="noErrors">Do not generate errors for modules without HelpInfoUri.</param>
        /// <returns>A list of modules.</returns>
        private Dictionary<Tuple<string, Version>, UpdatableHelpModuleInfo> GetModuleInfo(ExecutionContext context, string pattern, ModuleSpecification fullyQualifiedName, bool noErrors)
            List<PSModuleInfo> modules = null;
            string moduleNamePattern = null;
            if (pattern != null)
                moduleNamePattern = pattern;
                modules = Utils.GetModules(pattern, context);
            else if (fullyQualifiedName != null)
                moduleNamePattern = fullyQualifiedName.Name;
                modules = Utils.GetModules(fullyQualifiedName, context);
            var helpModules = new Dictionary<Tuple<string, Version>, UpdatableHelpModuleInfo>();
                    ProcessSingleModuleObject(module, context, helpModules, noErrors);
            IEnumerable<WildcardPattern> patternList = SessionStateUtilities.CreateWildcardsFromStrings(
                globPatterns: new[] { moduleNamePattern },
                options: WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant);
            foreach (KeyValuePair<string, string> name in s_metadataCache)
                if (SessionStateUtilities.MatchesAnyWildcardPattern(name.Key, patternList, true))
                    // For core snapin, there are no GUIDs. So, we need to construct the HelpInfo slightly differently
                    if (!name.Key.Equals(InitialSessionState.CoreSnapin, StringComparison.OrdinalIgnoreCase))
                        var keyTuple = new Tuple<string, Version>(name.Key, new Version("1.0"));
                            List<PSModuleInfo> availableModules = Utils.GetModules(name.Key, context);
                            if (availableModules != null)
                                foreach (PSModuleInfo module in availableModules)
                                    keyTuple = new Tuple<string, Version>(module.Name, module.Version);
                                        helpModules.Add(keyTuple, new UpdatableHelpModuleInfo(module.Name,
                                            module.Guid, Utils.GetApplicationBase(context.ShellID), s_metadataCache[module.Name]));
                        var keyTuple2 = new Tuple<string, Version>(name.Key, new Version("1.0"));
                            helpModules.Add(keyTuple2,
                                            new UpdatableHelpModuleInfo(name.Key, Guid.Empty,
                                                                        Utils.GetApplicationBase(context.ShellID),
                                                                        name.Value));
            return helpModules;
        /// Handles Ctrl+C.
            _helpSystem.CancelDownload();
        /// End processing.
            foreach (UpdatableHelpExceptionContext exception in _exceptions.Values)
                UpdatableHelpExceptionContext e = exception;
                if ((exception.Exception.FullyQualifiedErrorId == "HelpCultureNotSupported") &&
                    ((exception.Cultures != null && exception.Cultures.Count > 1) ||
                    (exception.Modules != null && exception.Modules.Count > 1)))
                    // Win8: 744749 Rewriting the error message only in the case where either
                    // multiple cultures or multiple modules are involved.
                    e = new UpdatableHelpExceptionContext(new UpdatableHelpSystemException(
                        "HelpCultureNotSupported", StringUtil.Format(HelpDisplayStrings.CannotMatchUICulturePattern,
                        string.Join(", ", exception.Cultures)),
                        ErrorCategory.InvalidArgument, exception.Cultures, null));
                    e.Modules = exception.Modules;
                    e.Cultures = exception.Cultures;
                WriteError(e.CreateErrorRecord(_commandType));
                LogContext context = MshLog.GetLogContext(Context, MyInvocation);
                context.Severity = "Error";
                PSEtwLog.LogOperationalError(PSEventId.Pipeline_Detail, PSOpcode.Exception, PSTask.ExecutePipeline,
                    context, e.GetExceptionMessage(_commandType));
        /// Main cmdlet logic for processing module names or fully qualified module names.
        /// <param name="moduleNames">Module names given by the user.</param>
        /// <param name="fullyQualifiedNames">FullyQualifiedNames.</param>
        internal void Process(IEnumerable<string> moduleNames, IEnumerable<ModuleSpecification> fullyQualifiedNames)
            _helpSystem.UseDefaultCredentials = _useDefaultCredentials;
                foreach (string name in moduleNames)
                    ProcessModuleWithGlobbing(name);
            else if (fullyQualifiedNames != null)
                    ProcessModuleWithGlobbing(fullyQualifiedName);
                foreach (KeyValuePair<Tuple<string, Version>, UpdatableHelpModuleInfo> module in GetModuleInfo("*", null, true))
                    ProcessModule(module.Value);
        /// Processing module objects for Save-Help.
        /// <param name="modules">Module objects given by the user.</param>
        internal void Process(IEnumerable<PSModuleInfo> modules)
            if (modules == null || !modules.Any())
                ProcessSingleModuleObject(module, Context, helpModules, false);
            foreach (KeyValuePair<Tuple<string, Version>, UpdatableHelpModuleInfo> helpModule in helpModules)
                ProcessModule(helpModule.Value);
        /// Processes a module with potential globbing.
        /// <param name="name">Module name with globbing.</param>
        private void ProcessModuleWithGlobbing(string name)
                PSArgumentException e = new PSArgumentException(StringUtil.Format(HelpDisplayStrings.ModuleNameNullOrEmpty));
            foreach (KeyValuePair<Tuple<string, Version>, UpdatableHelpModuleInfo> module in GetModuleInfo(name, null, false))
        /// Processes a ModuleSpecification with potential globbing.
        /// <param name="fullyQualifiedName">ModuleSpecification.</param>
        private void ProcessModuleWithGlobbing(ModuleSpecification fullyQualifiedName)
            foreach (KeyValuePair<Tuple<string, Version>, UpdatableHelpModuleInfo> module in GetModuleInfo(null, fullyQualifiedName, false))
        /// Processes a single module with multiple cultures.
        private void ProcessModule(UpdatableHelpModuleInfo module)
            _helpSystem.CurrentModule = module.ModuleName;
            if (this is UpdateHelpCommand && !Directory.Exists(module.ModuleBase))
                ProcessException(module.ModuleName, null,
                    new UpdatableHelpSystemException("ModuleBaseMustExist",
                        StringUtil.Format(HelpDisplayStrings.ModuleBaseMustExist),
                        ErrorCategory.InvalidOperation, null, null));
            // Win8: 572882 When the system locale is English and the UI is JPN,
            // running "update-help" still downs English help content.
            var cultures = _language ?? _helpSystem.GetCurrentUICulture();
            UpdatableHelpSystemException implicitCultureNotSupported = null;
            foreach (string culture in cultures)
                bool installed = true;
                    ProcessModuleWithCulture(module, culture);
                    ProcessException(module.ModuleName, culture, new UpdatableHelpSystemException("FailedToCopyFile",
                        e.Message, ErrorCategory.InvalidOperation, null, e));
                    ProcessException(module.ModuleName, culture, new UpdatableHelpSystemException("AccessIsDenied",
                        e.Message, ErrorCategory.PermissionDenied, null, e));
                catch (WebException e)
                    if (e.InnerException != null && e.InnerException is UnauthorizedAccessException)
                            e.InnerException.Message, ErrorCategory.PermissionDenied, null, e));
                        ProcessException(module.ModuleName, culture, e);
                catch (UpdatableHelpSystemException e)
                    if (e.FullyQualifiedErrorId == "HelpCultureNotSupported"
                            || e.FullyQualifiedErrorId == "UnableToRetrieveHelpInfoXml")
                            // Display the error message only if we are not using the fallback chain
                            // Hold first exception, it will be displayed if fallback chain fails
                            WriteVerbose(StringUtil.Format(HelpDisplayStrings.HelpCultureNotSupportedFallback, e.Message));
                            implicitCultureNotSupported ??= e;
                    if (_helpSystem.Errors.Count != 0)
                        foreach (Exception error in _helpSystem.Errors)
                            ProcessException(module.ModuleName, culture, error);
                        _helpSystem.Errors.Clear();
                // If -UICulture is not specified, we only install
                // one culture from the fallback chain
                if (_language == null && installed)
            // If the exception is not null and did not return early, then all of the fallback chain failed
            if (implicitCultureNotSupported != null)
                ProcessException(module.ModuleName, cultures.First(), implicitCultureNotSupported);
        internal virtual bool ProcessModuleWithCulture(UpdatableHelpModuleInfo module, string culture)
        #region Common methods
        /// Gets a list of modules from the given pattern or ModuleSpecification.
        /// <param name="pattern">Pattern to match.</param>
        /// <param name="noErrors">Skip errors.</param>
        internal Dictionary<Tuple<string, Version>, UpdatableHelpModuleInfo> GetModuleInfo(string pattern, ModuleSpecification fullyQualifiedName, bool noErrors)
            Dictionary<Tuple<string, Version>, UpdatableHelpModuleInfo> modules = GetModuleInfo(Context, pattern, fullyQualifiedName, noErrors);
            if (modules.Count == 0 && _exceptions.Count == 0 && !noErrors)
                var errorMessage = fullyQualifiedName != null ? StringUtil.Format(HelpDisplayStrings.ModuleNotFoundWithFullyQualifiedName, fullyQualifiedName)
                                                              : StringUtil.Format(HelpDisplayStrings.CannotMatchModulePattern, pattern);
                ErrorRecord errorRecord = new ErrorRecord(new Exception(errorMessage),
                    "ModuleNotFound", ErrorCategory.InvalidArgument, pattern);
        /// Checks if it is necessary to update help.
        /// <param name="module">ModuleInfo.</param>
        /// <param name="currentHelpInfo">Current HelpInfo.xml.</param>
        /// <param name="newHelpInfo">New HelpInfo.xml.</param>
        /// <param name="culture">Current culture.</param>
        /// <param name="force">Force update.</param>
        /// <returns>True if it is necessary to update help, false if not.</returns>
        internal bool IsUpdateNecessary(UpdatableHelpModuleInfo module, UpdatableHelpInfo currentHelpInfo,
            UpdatableHelpInfo newHelpInfo, CultureInfo culture, bool force)
            Debug.Assert(module != null);
                    StringUtil.Format(HelpDisplayStrings.UnableToRetrieveHelpInfoXml, culture.Name), ErrorCategory.ResourceUnavailable,
            // Culture check
            if (!newHelpInfo.IsCultureSupported(culture.Name))
                throw new UpdatableHelpSystemException("HelpCultureNotSupported",
                    StringUtil.Format(HelpDisplayStrings.HelpCultureNotSupported,
                    culture.Name, newHelpInfo.GetSupportedCultures()), ErrorCategory.InvalidOperation, null, null);
            // Version check
            if (!force && currentHelpInfo != null && !currentHelpInfo.IsNewerVersion(newHelpInfo, culture))
        /// Checks if the user has attempted to update more than once per day per module.
        /// <param name="path">Path to help info.</param>
        /// <param name="filename">Help info file name.</param>
        /// <param name="time">Current time (UTC).</param>
        /// <param name="force">If -Force is specified.</param>
        /// <returns>True if we are okay to update, false if not.</returns>
        internal bool CheckOncePerDayPerModule(string moduleName, string path, string filename, DateTime time, bool force)
            // Update if -Force is specified
            string helpInfoFilePath = SessionState.Path.Combine(path, filename);
            // No HelpInfo.xml
            if (!File.Exists(helpInfoFilePath))
            DateTime lastModified = File.GetLastWriteTimeUtc(helpInfoFilePath);
            TimeSpan difference = time - lastModified;
            if (difference.Days >= 1)
            if (_commandType == UpdatableHelpCommandType.UpdateHelpCommand)
                WriteVerbose(StringUtil.Format(HelpDisplayStrings.UseForceToUpdateHelp, moduleName));
            else if (_commandType == UpdatableHelpCommandType.SaveHelpCommand)
                WriteVerbose(StringUtil.Format(HelpDisplayStrings.UseForceToSaveHelp, moduleName));
        /// Resolves a given path to a list of directories.
        /// <param name="recurse">Resolve recursively?</param>
        /// <param name="isLiteralPath">Treat the path / start path as a literal path?</param>///
        /// <returns>A list of directories.</returns>
        internal IEnumerable<string> ResolvePath(string path, bool recurse, bool isLiteralPath)
                string newPath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(path);
                if (!Directory.Exists(newPath))
                resolvedPaths.Add(newPath);
                Collection<PathInfo> resolvedPathInfos = SessionState.Path.GetResolvedPSPathFromPSPath(path);
                foreach (PathInfo resolvedPath in resolvedPathInfos)
                    ValidatePathProvider(resolvedPath);
                    resolvedPaths.Add(resolvedPath.ProviderPath);
                    foreach (string innerResolvedPath in RecursiveResolvePathHelper(resolvedPath))
                        yield return innerResolvedPath;
                    // Win8: 566738
                    CmdletProviderContext context = new CmdletProviderContext(this.Context);
                    // resolvedPath is already resolved..so no need to expand wildcards anymore
                    if (isLiteralPath || InvokeProvider.Item.IsContainer(resolvedPath, context))
                        yield return resolvedPath;
        /// Resolves a given path to a list of directories recursively.
        private static IEnumerable<string> RecursiveResolvePathHelper(string path)
            if (System.IO.Directory.Exists(path))
                yield return path;
                foreach (string subDirectory in Directory.EnumerateDirectories(path))
                    foreach (string subDirectory2 in RecursiveResolvePathHelper(subDirectory))
                        yield return subDirectory2;
        /// Validates the provider of the path, only FileSystem provider is accepted.
        /// <param name="path">Path to validate.</param>
        internal void ValidatePathProvider(PathInfo path)
            if (path.Provider == null || path.Provider.Name != FileSystemProvider.ProviderName)
                throw new PSArgumentException(StringUtil.Format(HelpDisplayStrings.ProviderIsNotFileSystem,
                    path.Path));
        #region Logging
        /// Logs a command message.
        /// <param name="message">Message to log.</param>
        internal void LogMessage(string message)
            List<string> details = new List<string>() { message };
            PSEtwLog.LogPipelineExecutionDetailEvent(MshLog.GetLogContext(Context, Context.CurrentCommandProcessor.Command.MyInvocation), details);
        #region Exception processing
        /// Processes an exception for help cmdlets.
        /// <param name="culture">Culture info.</param>
        /// <param name="e">Exception to check.</param>
        internal void ProcessException(string moduleName, string culture, Exception e)
            UpdatableHelpSystemException except = null;
            if (e is UpdatableHelpSystemException)
                except = (UpdatableHelpSystemException)e;
            else if (e is WebException)
                except = new UpdatableHelpSystemException("UnableToConnect",
                    StringUtil.Format(HelpDisplayStrings.UnableToConnect), ErrorCategory.InvalidOperation, null, e);
            else if (e is PSArgumentException)
                except = new UpdatableHelpSystemException("InvalidArgument",
                    e.Message, ErrorCategory.InvalidArgument, null, e);
                except = new UpdatableHelpSystemException("UnknownErrorId",
                    e.Message, ErrorCategory.InvalidOperation, null, e);
            if (!_exceptions.ContainsKey(except.FullyQualifiedErrorId))
                _exceptions.Add(except.FullyQualifiedErrorId, new UpdatableHelpExceptionContext(except));
            _exceptions[except.FullyQualifiedErrorId].Modules.Add(moduleName);
            if (culture != null)
                _exceptions[except.FullyQualifiedErrorId].Cultures.Add(culture);
    /// Scope to which the help should be saved.
    public enum UpdateHelpScope
        /// Save the help content to the user directory.
        CurrentUser,
        /// Save the help content to the module directory. This is the default behavior.
        AllUsers

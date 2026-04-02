    internal class CommandSearcher : IEnumerable<CommandInfo>, IEnumerator<CommandInfo>
        /// to a command using a standard algorithm.
        /// <param name="commandName">The name of the command to look for.</param>
        /// <param name="options">Determines which types of commands glob resolution of the name will take place on.</param>
        /// <param name="commandTypes">The types of commands to look for.</param>
        /// <param name="context">The execution context for this engine instance.</param>
        /// <param name="fuzzyMatcher">The fuzzy matcher to use for fuzzy searching.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="context"/> is null.</exception>
        /// <exception cref="PSArgumentException">If <paramref name="commandName"/> is null or empty.</exception>
        internal CommandSearcher(
            SearchResolutionOptions options,
            FuzzyMatcher? fuzzyMatcher = null)
            Diagnostics.Assert(context != null, "caller to verify context is not null");
            Diagnostics.Assert(!string.IsNullOrEmpty(commandName), "caller to verify commandName is valid");
            _commandName = commandName;
            _commandResolutionOptions = options;
            _commandTypes = commandTypes;
            // Initialize the enumerators
            this.Reset();
        IEnumerator<CommandInfo> IEnumerable<CommandInfo>.GetEnumerator()
        /// Moves the enumerator to the next command match. Public for IEnumerable.
            _currentMatch = null;
            if (_currentState == SearchState.SearchingAliases)
                _currentMatch = SearchForAliases();
                // Why don't we check IsVisible on other scoped items?
                if (_currentMatch != null && SessionState.IsVisible(_commandOrigin, _currentMatch))
                // Make sure Current doesn't return an alias that isn't visible
                // Advance the state
                _currentState = SearchState.SearchingFunctions;
            if (_currentState == SearchState.SearchingFunctions)
                _currentMatch = SearchForFunctions();
                // Return the alias info only if it is visible. If not, then skip to the next
                // stage of command resolution...
                if (_currentMatch != null)
                _currentState = SearchState.SearchingCmdlets;
            if (_currentState == SearchState.SearchingCmdlets)
                _currentMatch = SearchForCmdlets();
                _currentState = SearchState.StartSearchingForExternalCommands;
            if (_currentState == SearchState.StartSearchingForExternalCommands)
                if ((_commandTypes & (CommandTypes.Application | CommandTypes.ExternalScript)) == 0)
                    // Since we are not requiring any path lookup in this search, just return false now
                    // because all the remaining searches do path lookup.
                // For security reasons, if the command is coming from outside the runspace and it looks like a path,
                // we want to pre-check that path before doing any probing of the network or drives
                if (_commandOrigin == CommandOrigin.Runspace && _commandName.IndexOfAny(Utils.Separators.DirectoryOrDrive) >= 0)
                    bool allowed = false;
                    // Ok - it looks like it might be a path, so we're going to check to see if the command is prefixed
                    // by any of the allowed paths. If so, then we allow the search to proceed...
                    // If either the Applications or Script lists contain just '*' the command is allowed
                    // at this point.
                    if ((_context.EngineSessionState.Applications.Count == 1 &&
                        _context.EngineSessionState.Applications[0].Equals("*", StringComparison.OrdinalIgnoreCase)) ||
                        (_context.EngineSessionState.Scripts.Count == 1 &&
                        _context.EngineSessionState.Scripts[0].Equals("*", StringComparison.OrdinalIgnoreCase)))
                        allowed = true;
                        // Ok, see if it's in the applications list
                        foreach (string path in _context.EngineSessionState.Applications)
                            if (checkPath(path, _commandName))
                        // If it wasn't in the applications list, see it's in the script list
                        if (!allowed)
                            foreach (string path in _context.EngineSessionState.Scripts)
                _currentState = SearchState.PowerShellPathResolution;
                _currentMatch = ProcessBuiltinScriptState();
                    // Set the current state to QualifiedFileSystemPath since
                    // we want to skip the qualified file system path search
                    // in the case where we found a PowerShell qualified path.
                    _currentState = SearchState.QualifiedFileSystemPath;
            if (_currentState == SearchState.PowerShellPathResolution)
                _currentMatch = ProcessPathResolutionState();
            // Search using CommandPathSearch
            if (_currentState == SearchState.QualifiedFileSystemPath ||
                    _currentState == SearchState.PathSearch)
                _currentMatch = ProcessQualifiedFileSystemState();
            if (_currentState == SearchState.PathSearch)
                _currentState = SearchState.PowerShellRelativePath;
                _currentMatch = ProcessPathSearchState();
        private CommandInfo? SearchForAliases()
            CommandInfo? currentMatch = null;
            if (_context.EngineSessionState != null &&
                (_commandTypes & CommandTypes.Alias) != 0)
                currentMatch = GetNextAlias();
        private CommandInfo? SearchForFunctions()
                (_commandTypes & (CommandTypes.Function | CommandTypes.Filter | CommandTypes.Configuration)) != 0)
                currentMatch = GetNextFunction();
        private CommandInfo? SearchForCmdlets()
            if ((_commandTypes & CommandTypes.Cmdlet) != 0)
                currentMatch = GetNextCmdlet();
        private CommandInfo? ProcessBuiltinScriptState()
            // Check to see if the path is qualified
                _context.EngineSessionState.ProviderCount > 0 &&
                IsQualifiedPSPath(_commandName))
                currentMatch = GetNextFromPath();
        private CommandInfo? ProcessPathResolutionState()
                // Check to see if the path is a file system path that
                // is rooted.  If so that is the next match
                if (Path.IsPathRooted(_commandName) &&
                    File.Exists(_commandName))
                        currentMatch = GetInfoFromPath(_commandName);
                    catch (FileLoadException)
                    catch (MetadataException)
                // If the path contains illegal characters that
                // weren't caught by the other APIs, IsPathRooted
                // will throw an exception.
                // For example, looking for a command called
                // `abcdef
                // The `a will be translated into the beep control character
                // which is not a legal file system character, though
                // Path.InvalidPathChars does not contain it as an invalid
                // character.
        private CommandInfo? ProcessQualifiedFileSystemState()
                setupPathSearcher();
                _currentState = SearchState.NoMoreMatches;
            _currentState = SearchState.PathSearch;
            if (_canDoPathLookup)
                    // the previous call to setupPathSearcher ensures _pathSearcher != null
                    while (currentMatch == null && _pathSearcher!.MoveNext())
                        currentMatch = GetInfoFromPath(((IEnumerator<string>)_pathSearcher).Current);
                    // The enumerator may throw if there are no more matches
        private CommandInfo? ProcessPathSearchState()
            string? path = DoPowerShellRelativePathLookup();
            if (!string.IsNullOrEmpty(path))
                currentMatch = GetInfoFromPath(path);
        /// Gets the CommandInfo representing the current command match.
        CommandInfo IEnumerator<CommandInfo>.Current
                if ((_currentState == SearchState.SearchingAliases && _currentMatch == null) ||
                    _currentState == SearchState.NoMoreMatches ||
                    _currentMatch == null)
                return _currentMatch;
                return ((IEnumerator<CommandInfo>)this).Current;
            if (_pathSearcher != null)
                _pathSearcher.Dispose();
                _pathSearcher = null;
        /// Gets the next command info using the command name as a path.
        /// A CommandInfo for the next command if it exists as a path, or null otherwise.
        private CommandInfo? GetNextFromPath()
            CommandInfo? result = null;
                    "The name appears to be a qualified path: {0}",
                    _commandName);
                    "Trying to resolve the path as an PSPath");
                // Find the match if it is.
                // Try literal path resolution if it is set to run first
                if (_commandResolutionOptions.HasFlag(SearchResolutionOptions.ResolveLiteralThenPathPatterns))
                    var path = GetNextLiteralPathThatExistsAndHandleExceptions(_commandName, out _);
                        return GetInfoFromPath(path);
                Collection<string> resolvedPaths = new Collection<string>();
                if (WildcardPattern.ContainsWildcardCharacters(_commandName))
                    resolvedPaths = GetNextFromPathUsingWildcards(_commandName, out _);
                // Try literal path resolution if wildcards are enable first and wildcard search failed
                if (!_commandResolutionOptions.HasFlag(SearchResolutionOptions.ResolveLiteralThenPathPatterns) &&
                    resolvedPaths.Count == 0)
                    string? path = GetNextLiteralPathThatExistsAndHandleExceptions(_commandName, out _);
                        "The path resolved to more than one result so this path cannot be used.");
                // If the path was resolved, and it exists
                if (resolvedPaths.Count == 1 &&
                    File.Exists(resolvedPaths[0]))
                    string path = resolvedPaths[0];
                        "Path resolved to: {0}",
                    result = GetInfoFromPath(path);
        /// Gets the next path using WildCards.
        /// The command to search for.
        /// <param name="provider">The provider that the command was found in.</param>
        /// A collection of full paths to the commands which were found.
        private Collection<string> GetNextFromPathUsingWildcards(string? command, out ProviderInfo? provider)
                return _context.LocationGlobber.GetGlobbedProviderPathsFromMonadPath(path: command, allowNonexistingPaths: false, provider: out provider, providerInstance: out _);
                    "The path could not be found: {0}",
                    command);
                    "A drive could not be found for the path: {0}",
                    "A provider could not be found for the path: {0}",
                    "The path specified a home directory, but the provider home directory was not set. {0}",
            catch (ProviderInvocationException providerException)
                    "The provider associated with the path '{0}' encountered an error: {1}",
                    providerException.Message);
                    "The provider associated with the path '{0}' does not implement ContainerCmdletProvider",
            provider = null;
            return new Collection<string>();
        private static bool checkPath(string path, string commandName)
            return path.StartsWith(commandName, StringComparison.OrdinalIgnoreCase);
        /// Gets the appropriate CommandInfo instance given the specified path.
        /// The path to create the CommandInfo for.
        /// An instance of the appropriate CommandInfo derivative given the specified path.
        /// <exception cref="FileLoadException">
        /// The <paramref name="path"/> refers to a cmdlet, or cmdletprovider
        /// and it could not be loaded as an XML document.
        /// <exception cref="FormatException">
        /// that does not adhere to the appropriate file format for its extension.
        /// If <paramref name="path"/> refers to a cmdlet file that
        /// contains invalid metadata.
        private CommandInfo? GetInfoFromPath(string path)
                    CommandDiscovery.discoveryTracer.TraceError("The path does not exist: {0}", path);
                // Now create the appropriate CommandInfo using the extension
                string? extension = null;
                    extension = Path.GetExtension(path);
                    // weren't caught by the other APIs, GetExtension
                    // which is not a legal file system character.
                if (extension == null)
                if (string.Equals(extension, StringLiterals.PowerShellScriptFileExtension, StringComparison.OrdinalIgnoreCase))
                    if ((_commandTypes & CommandTypes.ExternalScript) != 0)
                        string scriptName = Path.GetFileName(path);
                            "Command Found: path ({0}) is a script with name: {1}",
                            scriptName);
                        // The path is to a PowerShell script
                        result = new ExternalScriptInfo(scriptName, path, _context);
                if ((_commandTypes & CommandTypes.Application) != 0)
                    // Anything else is treated like an application
                    string appName = Path.GetFileName(path);
                        "Command Found: path ({0}) is an application with name: {1}",
                        appName);
                    result = new ApplicationInfo(appName, path, _context);
            // Verify that this script is not untrusted, if we aren't constrained.
            if (ShouldSkipCommandResolutionForConstrainedLanguage(result, _context))
        /// Gets the next matching alias.
        /// A CommandInfo representing the next matching alias if found, otherwise null.
        private CommandInfo? GetNextAlias()
            if ((_commandResolutionOptions & SearchResolutionOptions.ResolveAliasPatterns) != 0)
                if (_matchingAlias == null)
                    // Generate the enumerator of matching alias names
                    Collection<AliasInfo> matchingAliases = new Collection<AliasInfo>();
                    WildcardPattern aliasMatcher =
                            _commandName,
                    foreach (KeyValuePair<string, AliasInfo> aliasEntry in _context.EngineSessionState.GetAliasTable())
                        if (aliasMatcher.IsMatch(aliasEntry.Key) ||
                            (_fuzzyMatcher is not null && _fuzzyMatcher.IsFuzzyMatch(aliasEntry.Key, _commandName)))
                            matchingAliases.Add(aliasEntry.Value);
                    // Process alias from modules
                    AliasInfo? c = GetAliasFromModules(_commandName);
                    if (c != null)
                        matchingAliases.Add(c);
                    _matchingAlias = matchingAliases.GetEnumerator();
                if (!_matchingAlias.MoveNext())
                    _matchingAlias = null;
                    result = _matchingAlias.Current;
                result = _context.EngineSessionState.GetAlias(_commandName) ?? GetAliasFromModules(_commandName);
            // Verify that this alias was not created by an untrusted constrained language,
            // if we aren't constrained.
                    "Alias found: {0}  {1}",
                    result.Name,
                    result.Definition);
        /// Gets the next matching function.
        /// A CommandInfo representing the next matching function if found, otherwise null.
        private CommandInfo? GetNextFunction()
            if (_commandResolutionOptions.HasFlag(SearchResolutionOptions.ResolveFunctionPatterns))
                if (_matchingFunctionEnumerator == null)
                    Collection<CommandInfo?> matchingFunction = new Collection<CommandInfo?>();
                    // Generate the enumerator of matching function names
                    WildcardPattern functionMatcher =
                    foreach ((string functionName, FunctionInfo functionInfo) in _context.EngineSessionState.GetFunctionTable())
                        if (functionMatcher.IsMatch(functionName) ||
                            (_fuzzyMatcher is not null && _fuzzyMatcher.IsFuzzyMatch(functionName, _commandName)))
                            matchingFunction.Add(functionInfo);
                        else if (_commandResolutionOptions.HasFlag(SearchResolutionOptions.UseAbbreviationExpansion))
                            if (_commandName.Equals(ModuleUtils.AbbreviateName(functionName), StringComparison.OrdinalIgnoreCase))
                    // Process functions from modules
                    CommandInfo? cmdInfo = GetFunctionFromModules(_commandName);
                    if (cmdInfo != null)
                        matchingFunction.Add(cmdInfo);
                    _matchingFunctionEnumerator = matchingFunction.GetEnumerator();
                if (!_matchingFunctionEnumerator.MoveNext())
                    _matchingFunctionEnumerator = null;
                    result = _matchingFunctionEnumerator.Current;
                result = GetFunction(_commandName);
            // Verify that this function was not created by an untrusted constrained language,
        // Don't return commands to the user if that might result in:
        //     - Trusted commands calling untrusted functions that the user has overridden
        //     - Debug prompts calling internal functions that are likely to have code injection
        private static bool ShouldSkipCommandResolutionForConstrainedLanguage(CommandInfo? result, ExecutionContext executionContext)
            // Don't return untrusted commands to trusted functions.
            if (result.DefiningLanguageMode == PSLanguageMode.ConstrainedLanguage && executionContext.LanguageMode == PSLanguageMode.FullLanguage)
                // This audit log message is to inform the user that an expected command will not be available because it is not trusted
                // when the machine is in policy enforcement mode.
                    context: executionContext,
                    title: CommandBaseStrings.SearcherWDACLogTitle,
                    message: StringUtil.Format(CommandBaseStrings.SearcherWDACLogMessage, result.Name, result.ModuleName ?? string.Empty),
                    fqid: "CommandSearchFailureForUntrustedCommand");
            // Don't allow invocation of trusted functions from debug breakpoints.
            // They were probably defined within a trusted script, and could be
            // susceptible to injection attacks. However, we do allow execution
            // of functions defined in the global scope (i.e.: "more",) as those
            // are intended to be exposed explicitly to users.
            if ((result is FunctionInfo) &&
                (executionContext.LanguageMode == PSLanguageMode.ConstrainedLanguage) &&
                (result.DefiningLanguageMode == PSLanguageMode.FullLanguage) &&
                (executionContext.Debugger != null) &&
                (executionContext.Debugger.InBreakpoint) &&
                (!(executionContext.TopLevelSessionState.GetFunctionTableAtScope("GLOBAL").ContainsKey(result.Name))))
        private AliasInfo? GetAliasFromModules(string command)
            AliasInfo? result = null;
            if (command.IndexOf('\\') > 0)
                // See if it's a module qualified alias...
                PSSnapinQualifiedName? qualifiedName = PSSnapinQualifiedName.GetInstance(command);
                if (qualifiedName != null && !string.IsNullOrEmpty(qualifiedName.PSSnapInName))
                    PSModuleInfo? module = GetImportedModuleByName(qualifiedName.PSSnapInName);
                    module?.ExportedAliases.TryGetValue(qualifiedName.ShortName, out result);
        private CommandInfo? GetFunctionFromModules(string command)
            FunctionInfo? result = null;
                // See if it's a module qualified function call...
                    module?.ExportedFunctions.TryGetValue(qualifiedName.ShortName, out result);
        private PSModuleInfo? GetImportedModuleByName(string moduleName)
            PSModuleInfo? module = null;
            List<PSModuleInfo> modules = _context.Modules.GetModules(new string[] { moduleName }, false);
            if (modules != null && modules.Count > 0)
                foreach (PSModuleInfo m in modules)
                    if (_context.previousModuleImported.ContainsKey(m.Name) && ((string?)_context.previousModuleImported[m.Name] == m.Path))
                        module = m;
                module ??= modules[0];
            return module;
        /// Gets the FunctionInfo or FilterInfo for the specified function name.
        /// <param name="function">
        /// The name of the function/filter to retrieve.
        /// A FunctionInfo if the function name exists and is a function, a FilterInfo if
        /// the filter name exists and is a filter, or null otherwise.
        private CommandInfo? GetFunction(string function)
            CommandInfo? result = _context.EngineSessionState.GetFunction(function);
                var formatString = result switch
                    FilterInfo => "Filter found: {0}",
                    ConfigurationInfo => "Configuration found: {0}",
                    _ => "Function found: {0}",
                CommandDiscovery.discoveryTracer.WriteLine(formatString, function);
                result = GetFunctionFromModules(function);
        /// Gets the next cmdlet from the collection of matching cmdlets.
        /// If the collection doesn't exist yet it is created and the
        /// enumerator is moved to the first item in the collection.
        /// A CmdletInfo for the next matching Cmdlet or null if there are
        /// no more matches.
        private CmdletInfo? GetNextCmdlet()
            CmdletInfo? result = null;
            bool useAbbreviationExpansion = _commandResolutionOptions.HasFlag(SearchResolutionOptions.UseAbbreviationExpansion);
            if (_matchingCmdlet == null)
                if (_commandResolutionOptions.HasFlag(SearchResolutionOptions.CommandNameIsPattern) || useAbbreviationExpansion)
                    Collection<CmdletInfo> matchingCmdletInfo = new Collection<CmdletInfo>();
                    PSSnapinQualifiedName? PSSnapinQualifiedCommandName =
                        PSSnapinQualifiedName.GetInstance(_commandName);
                    if (!useAbbreviationExpansion && PSSnapinQualifiedCommandName == null)
                    string? moduleName = PSSnapinQualifiedCommandName?.PSSnapInName;
                    var cmdletShortName = PSSnapinQualifiedCommandName?.ShortName;
                    WildcardPattern? cmdletMatcher = cmdletShortName != null
                        ? WildcardPattern.Get(cmdletShortName, WildcardOptions.IgnoreCase)
                    SessionStateInternal ss = _context.EngineSessionState;
                    foreach (List<CmdletInfo> cmdletList in ss.GetCmdletTable().Values)
                        foreach (CmdletInfo cmdlet in cmdletList)
                            if ((cmdletMatcher is not null && cmdletMatcher.IsMatch(cmdlet.Name)) ||
                                (_fuzzyMatcher is not null && _fuzzyMatcher.IsFuzzyMatch(cmdlet.Name, _commandName)))
                                if (string.IsNullOrEmpty(moduleName) || moduleName.Equals(cmdlet.ModuleName, StringComparison.OrdinalIgnoreCase))
                                    // If PSSnapin is specified, make sure they match
                                    matchingCmdletInfo.Add(cmdlet);
                            else if (useAbbreviationExpansion)
                                if (_commandName.Equals(ModuleUtils.AbbreviateName(cmdlet.Name), StringComparison.OrdinalIgnoreCase))
                    _matchingCmdlet = matchingCmdletInfo.GetEnumerator();
                    _matchingCmdlet = _context.CommandDiscovery.GetCmdletInfo(_commandName,
                        _commandResolutionOptions.HasFlag(SearchResolutionOptions.SearchAllScopes));
            if (!_matchingCmdlet.MoveNext())
                _matchingCmdlet = null;
                result = _matchingCmdlet.Current;
            return traceResult(result);
        private IEnumerator<CmdletInfo>? _matchingCmdlet;
        [return: NotNullIfNotNull("result")]
        private static CmdletInfo? traceResult(CmdletInfo? result)
                    "Cmdlet found: {0}  {1}",
                    result.ImplementingType);
        private string? DoPowerShellRelativePathLookup()
            string? result = null;
                _context.EngineSessionState.ProviderCount > 0 && _commandName.Length != 0)
                // NTRAID#Windows OS Bugs-1009294-2004/02/04-JeffJon
                // This is really slow.  Maybe since we are only allowing FS paths right
                // now we should use the file system APIs to verify the existence of the file.
                // Since the path to the command was not found using the PATH variable,
                // maybe it is relative to the current location. Try resolving the
                // path.
                // Relative Path:       ".\command.exe"
                // Home Path:           "~\command.exe"
                // Drive Relative Path: "\Users\User\AppData\Local\Temp\command.exe"
                char firstChar = _commandName[0];
                if (firstChar == '.' || firstChar == '~' || firstChar == '\\')
                    using (CommandDiscovery.discoveryTracer.TraceScope(
                        "{0} appears to be a relative path. Trying to resolve relative path",
                        _commandName))
                        result = ResolvePSPath(_commandName);
        /// Resolves the given path as an PSPath and ensures that it was resolved
        /// by the FileSystemProvider.
        /// The path that was resolved. Null if the path couldn't be resolved or was
        /// not resolved by the FileSystemProvider.
        private string? ResolvePSPath(string? path)
                ProviderInfo? provider = null;
                    // Cannot return early as this code path only expects
                    // The file system provider and the final check for that
                    // must verify this before we return.
                    resolvedPath = GetNextLiteralPathThatExists(path, out provider);
                if (WildcardPattern.ContainsWildcardCharacters(path) &&
                    ((resolvedPath == null) || (provider == null)))
                    // Let PowerShell resolve relative path with wildcards.
                    Collection<string> resolvedPaths = GetNextFromPathUsingWildcards(path, out provider);
                    if (resolvedPaths.Count == 0)
                        resolvedPath = null;
                           "The relative path with wildcard did not resolve to valid path. {0}",
                    else if (resolvedPaths.Count > 1)
                        "The relative path with wildcard resolved to multiple paths. {0}",
                        resolvedPath = resolvedPaths[0];
                // Try literal path resolution if wildcards are enabled first and wildcard search failed
                // Verify the path was resolved to a file system path
                if (provider != null && provider.NameEquals(_context.ProviderNames.FileSystem))
                    result = resolvedPath;
                        "The relative path was resolved to: {0}",
                        result);
                    // The path was not to the file system
                    "The home path was not specified for the provider. {0}",
                    "While resolving the path, \"{0}\", an error was encountered by the provider: {1}",
                    "The path does not exist: {0}",
                    "The drive does not exist: {0}",
                    driveNotFound.ItemName);
        /// Gets the next literal path.
        /// Filtering to ones that exist for the filesystem.
        /// Handles Exceptions
        /// Full path to the command.
        private string? GetNextLiteralPathThatExistsAndHandleExceptions(string command, out ProviderInfo? provider)
                return GetNextLiteralPathThatExists(command, out provider);
                // This can be because we think a scope or a url is a drive
                // and need to continue searching.
                // Although, scope does not work through get-command
        private string? GetNextLiteralPathThatExists(string? command, out ProviderInfo? provider)
            string resolvedPath = _context.LocationGlobber.GetProviderPath(command, out provider);
            if (provider.NameEquals(_context.ProviderNames.FileSystem)
                && !File.Exists(resolvedPath)
                && !Directory.Exists(resolvedPath))
            return resolvedPath;
        /// Creates a collection of patterns used to find the command.
        /// The name of the command to search for.
        /// <param name="commandDiscovery">Get names for command discovery.</param>
        /// A collection of the patterns used to find the command.
        /// The patterns are as follows:
        ///     1. [commandName].cmdlet
        ///     2. [commandName].ps1
        ///     3..x
        ///         foreach (extension in PATHEXT)
        ///             [commandName].[extension]
        ///     x+1. [commandName]
        /// If <paramref name="name"/> contains one or more of the
        /// invalid characters defined in InvalidPathChars.
        internal LookupPathCollection ConstructSearchPatternsFromName(string name, bool commandDiscovery = false)
            var result = new LookupPathCollection();
            // First check to see if the commandName has an extension, if so
            // look for that first
            bool commandNameAddedFirst = Path.HasExtension(name);
            if (commandNameAddedFirst)
                result.Add(name);
            // Add the extensions for script, module and data files in that order...
            if (_commandTypes.HasFlag(CommandTypes.ExternalScript))
                result.Add(name + StringLiterals.PowerShellScriptFileExtension);
                if (!commandDiscovery)
                    // psd1 and psm1 are not executable, so don't add them
                    result.Add(name + StringLiterals.PowerShellModuleFileExtension);
                    result.Add(name + StringLiterals.PowerShellDataFileExtension);
            if (_commandTypes.HasFlag(CommandTypes.Application))
                // Now add each extension from the PATHEXT environment variable
                foreach (string extension in CommandDiscovery.PathExtensions)
                    result.Add(name + extension);
            // Now add the commandName by itself if it wasn't added as the first pattern
            if (!commandNameAddedFirst)
        /// Determines if the given command name is a qualified PowerShell path.
        /// True if the command name is either a provider-qualified or PowerShell drive-qualified
        /// path. False otherwise.
        private static bool IsQualifiedPSPath(string commandName)
                !string.IsNullOrEmpty(commandName),
                "The caller should have verified the commandName");
                LocationGlobber.IsAbsolutePath(commandName) ||
                LocationGlobber.IsProviderQualifiedPath(commandName) ||
                LocationGlobber.IsHomePath(commandName) ||
                LocationGlobber.IsProviderDirectPath(commandName);
        private enum CanDoPathLookupResult
            Yes,
            PathIsRooted,
            WildcardCharacters,
            DirectorySeparator,
            IllegalCharacters
        /// Determines if the command name has any path special
        /// characters which would require resolution. If so,
        /// path lookup will not succeed.
        /// <param name="possiblePath">
        /// The command name (or possible path) to look for the special characters.
        /// True if the command name does not contain any special
        /// characters.  False otherwise.
        private static CanDoPathLookupResult CanDoPathLookup(string possiblePath)
            CanDoPathLookupResult result = CanDoPathLookupResult.Yes;
                // If the command name contains any wildcard characters
                // we can't do the path lookup
                if (WildcardPattern.ContainsWildcardCharacters(possiblePath))
                    result = CanDoPathLookupResult.WildcardCharacters;
                    if (Path.IsPathRooted(possiblePath))
                        result = CanDoPathLookupResult.PathIsRooted;
                    result = CanDoPathLookupResult.IllegalCharacters;
                // If the command contains any path separators, we can't
                // do the path lookup
                if (possiblePath.AsSpan().IndexOfAny('\\', '/') != -1)
                    result = CanDoPathLookupResult.DirectorySeparator;
                // If the command contains any invalid path characters, we can't
                if (PathUtils.ContainsInvalidPathChars(possiblePath))
        private string _commandName;
        /// Determines which command types will be globbed.
        private readonly SearchResolutionOptions _commandResolutionOptions;
        /// Determines which types of commands to look for.
        private CommandTypes _commandTypes = CommandTypes.All;
        /// The enumerator that uses the Path to
        /// search for commands.
        private CommandPathSearch? _pathSearcher;
        /// The execution context instance for the current engine...
        /// A routine to initialize the path searcher...
        /// If the commandName used to construct this object
        /// contains one or more of the invalid characters defined
        /// in InvalidPathChars.
        private void setupPathSearcher()
            // If it's already set up, just return...
            // We are never going to look for non-executable commands in CommandSearcher.
            // Even though file types like .DOC, .LOG,.TXT, etc. can be opened / invoked, users think of these as files, not applications.
            // So I don't think we should show applications with the additional extensions at all.
            // Applications should only include files whose extensions are in the PATHEXT list and these would only be returned with the All parameter.
            if ((_commandResolutionOptions & SearchResolutionOptions.CommandNameIsPattern) != 0)
                _canDoPathLookup = true;
                _canDoPathLookupResult = CanDoPathLookupResult.Yes;
                _pathSearcher =
                    new CommandPathSearch(
                        _context.CommandDiscovery.GetLookupDirectoryPaths(),
                        _context,
                        acceptableCommandNames: null,
                        _fuzzyMatcher);
                _canDoPathLookupResult = CanDoPathLookup(_commandName);
                if (_canDoPathLookupResult == CanDoPathLookupResult.Yes)
                    _commandName = _commandName.TrimEnd(Utils.Separators.PathSearchTrimEnd);
                            ConstructSearchPatternsFromName(_commandName, commandDiscovery: true),
                            fuzzyMatcher: null);
                else if (_canDoPathLookupResult == CanDoPathLookupResult.PathIsRooted)
                    string? directory = Path.GetDirectoryName(_commandName);
                    var directoryCollection = new LookupPathCollection { directory };
                        "The path is rooted, so only doing the lookup in the specified directory: {0}",
                        directory);
                    string fileName = Path.GetFileName(_commandName);
                    if (!string.IsNullOrEmpty(fileName))
                        fileName = fileName.TrimEnd(Utils.Separators.PathSearchTrimEnd);
                                directoryCollection,
                                ConstructSearchPatternsFromName(fileName, commandDiscovery: true),
                        _canDoPathLookup = false;
                else if (_canDoPathLookupResult == CanDoPathLookupResult.DirectorySeparator)
                    // We must try to resolve the path as an PSPath or else we can't do
                    // path lookup for relative paths.
                    directory = ResolvePSPath(directory);
                        "The path is relative, so only doing the lookup in the specified directory: {0}",
                    if (directory == null)
        /// Resets the enumerator to before the first command match, public for IEnumerable.
            // If this is a command coming from outside the runspace and there are no
            // permitted scripts or applications,
            // remove them from the set of things to search for...
            if (_commandOrigin == CommandOrigin.Runspace)
                if (_context.EngineSessionState.Applications.Count == 0)
                    _commandTypes &= ~CommandTypes.Application;
                if (_context.EngineSessionState.Scripts.Count == 0)
                    _commandTypes &= ~CommandTypes.ExternalScript;
            _pathSearcher?.Reset();
            _currentState = SearchState.SearchingAliases;
        internal CommandOrigin CommandOrigin
            get { return _commandOrigin; }
            set { _commandOrigin = value; }
        private CommandOrigin _commandOrigin = CommandOrigin.Internal;
        /// An enumerator of the matching aliases.
        private IEnumerator<AliasInfo>? _matchingAlias;
        /// An enumerator of the matching functions.
        private IEnumerator<CommandInfo?>? _matchingFunctionEnumerator;
        /// The CommandInfo that references the command that matches the pattern.
        private CommandInfo? _currentMatch;
        private bool _canDoPathLookup;
        private CanDoPathLookupResult _canDoPathLookupResult = CanDoPathLookupResult.Yes;
        /// The current state of the enumerator.
        private SearchState _currentState = SearchState.SearchingAliases;
        private enum SearchState
            // the searcher has been reset or has not been advanced since being created.
            SearchingAliases,
            // the searcher has finished alias resolution and is now searching for functions.
            SearchingFunctions,
            // the searcher has finished function resolution and is now searching for cmdlets
            SearchingCmdlets,
            // the search has finished builtin script resolution and is now searching for external commands
            StartSearchingForExternalCommands,
            // the searcher has moved to
            PowerShellPathResolution,
            // the searcher has moved to a qualified file system path
            QualifiedFileSystemPath,
            // the searcher has moved to using a CommandPathSearch object
            // for resolution
            PathSearch,
            // with get prepended to the command name for resolution
            GetPathSearch,
            // the searcher has moved to resolving the command as a
            // relative PowerShell path
            PowerShellRelativePath,
            // No more matches can be found
            NoMoreMatches,
    /// Determines which types of commands should be globbed using the specified
    /// pattern. Any flag that is not specified will only match if exact.
    internal enum SearchResolutionOptions
        ResolveAliasPatterns = 0x01,
        ResolveFunctionPatterns = 0x02,
        CommandNameIsPattern = 0x04,
        SearchAllScopes = 0x08,
        /// Enable searching for cmdlets/functions by abbreviation expansion.
        UseAbbreviationExpansion = 0x10,
        /// Enable resolving wildcard in paths.
        ResolveLiteralThenPathPatterns = 0x20

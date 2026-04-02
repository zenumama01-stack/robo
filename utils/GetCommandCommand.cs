    /// The get-command cmdlet.  It uses the command discovery APIs to find one or more
    /// commands of the given name.  It returns an instance of CommandInfo for each
    /// command that is found.
    [Cmdlet(VerbsCommon.Get, "Command", DefaultParameterSetName = "CmdletSet", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096579")]
    [OutputType(typeof(AliasInfo), typeof(ApplicationInfo), typeof(FunctionInfo),
                typeof(CmdletInfo), typeof(ExternalScriptInfo), typeof(FilterInfo),
                typeof(string), typeof(PSObject))]
    public sealed class GetCommandCommand : PSCmdlet
        #region Definitions of cmdlet parameters
            ParameterSetName = "AllCommandSet")]
                _nameContainsWildcard = false;
                    foreach (string commandName in value)
                        if (WildcardPattern.ContainsWildcardCharacters(commandName))
                            _nameContainsWildcard = true;
        private bool _nameContainsWildcard;
        /// Gets or sets the verb parameter to the cmdlet.
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "CmdletSet")]
                return _verbs;
                _verbs = value;
                _verbPatterns = null;
        private string[] _verbs = Array.Empty<string>();
        /// Gets or sets the noun parameter to the cmdlet.
        [ArgumentCompleter(typeof(NounArgumentCompleter))]
        public string[] Noun
                return _nouns;
                _nouns = value;
                _nounPatterns = null;
        private string[] _nouns = Array.Empty<string>();
        /// Gets or sets the PSSnapin/Module parameter to the cmdlet.
                return _modules;
                _modules = value;
                _modulePatterns = null;
                _isModuleSpecified = true;
        private string[] _modules = Array.Empty<string>();
        private bool _isModuleSpecified = false;
        /// Gets or sets the ExcludeModule parameter to the cmdlet.
        public string[] ExcludeModule
                return _excludedModules;
                _excludedModules = value;
                _excludedModulePatterns = null;
        private string[] _excludedModules = Array.Empty<string>();
                _isFullyQualifiedModuleSpecified = true;
        private bool _isFullyQualifiedModuleSpecified = false;
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "AllCommandSet")]
                _isCommandTypeSpecified = true;
        private CommandTypes _commandType = CommandTypes.All;
        private bool _isCommandTypeSpecified = false;
        /// The parameter representing the total number of commands that will
        /// be returned. If negative, all matching commands that are found will
        /// be returned.
        public int TotalCount { get; set; } = -1;
        /// The parameter that determines if the CommandInfo or the string
        /// definition of the command is output.
        public SwitchParameter Syntax
                return _usage;
                _usage = value;
        private bool _usage;
        /// This parameter causes the output to be packaged into ShowCommandInfo PSObject types
        /// needed to display GUI command information.
        public SwitchParameter ShowCommandInfo { get; set; }
        [Parameter(Position = 1, ValueFromRemainingArguments = true)]
        /// The parameter that determines if additional matching commands should be returned.
        /// (Additional matching functions and aliases are returned from module tables)
        public SwitchParameter All
            get { return _all; }
            set { _all = value; }
        private bool _all;
        /// The parameter that determines if additional matching commands from available modules should be returned.
        /// If set to true, only those commands currently in the session are returned.
        public SwitchParameter ListImported
                return _listImported;
                _listImported = value;
        private bool _listImported;
        /// The parameter that filters commands returned to only include commands that have a parameter with a name that matches one of the ParameterName's arguments.
        public string[] ParameterName
                return _parameterNames;
                _parameterNames = value;
                _parameterNameWildcards = SessionStateUtilities.CreateWildcardsFromStrings(
                    _parameterNames,
        private Collection<WildcardPattern> _parameterNameWildcards;
        private string[] _parameterNames;
        private HashSet<string> _matchedParameterNames;
        /// The parameter that filters commands returned to only include commands that have a parameter of a type that matches one of the ParameterType's arguments.
        public PSTypeName[] ParameterType
                return _parameterTypes;
                // if '...CimInstance#Win32_Process' is specified, then exclude '...CimInstance'
                List<PSTypeName> filteredParameterTypes = new List<PSTypeName>(value.Length);
                for (int i = 0; i < value.Length; i++)
                    PSTypeName ptn = value[i];
                    if (value.Any(otherPtn => otherPtn.Name.StartsWith(ptn.Name + "#", StringComparison.OrdinalIgnoreCase)))
                    if ((i != 0) && (ptn.Type != null) && (ptn.Type.Equals(typeof(object))))
                    filteredParameterTypes.Add(ptn);
                _parameterTypes = filteredParameterTypes.ToArray();
        private PSTypeName[] _parameterTypes;
        /// Gets or sets the parameter that enables using fuzzy matching.
        [Parameter(ParameterSetName = "AllCommandSet")]
        public SwitchParameter UseFuzzyMatching { get; set; }
        /// Gets or sets the minimum fuzzy matching distance.
        public uint FuzzyMinimumDistance { get; set; } = 5;
        private FuzzyMatcher _fuzzyMatcher;
        private List<CommandScore> _commandScores;
        /// Gets or sets the parameter that determines if return cmdlets based on abbreviation expansion.
        /// This means it matches cmdlets where the uppercase characters for the noun match
        /// the given characters.  i.e., g-sgc would match Get-SomeGreatCmdlet.
        public SwitchParameter UseAbbreviationExpansion { get; set; }
        #endregion Definitions of cmdlet parameters
        /// Begin Processing.
            _timer.Start();
            if (UseFuzzyMatching)
                _fuzzyMatcher = new FuzzyMatcher(FuzzyMinimumDistance);
                _commandScores = new List<CommandScore>();
            if (ShowCommandInfo.IsPresent && Syntax.IsPresent)
                        new PSArgumentException(DiscoveryExceptions.GetCommandShowCommandInfoParamError),
                        "GetCommandCannotSpecifySyntaxAndShowCommandInfoTogether",
        /// Method that implements get-command.
            _commandsWritten.Clear();
            if (_isModuleSpecified && _isFullyQualifiedModuleSpecified)
                string errMsg = string.Format(CultureInfo.InvariantCulture, SessionStateStrings.GetContent_TailAndHeadCannotCoexist, "Module", "FullyQualifiedModule");
                ErrorRecord error = new ErrorRecord(new InvalidOperationException(errMsg), "ModuleAndFullyQualifiedModuleCannotBeSpecifiedTogether", ErrorCategory.InvalidOperation, null);
            // Initialize the module patterns
            _modulePatterns ??= SessionStateUtilities.CreateWildcardsFromStrings(Module, WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant);
            _excludedModulePatterns ??= SessionStateUtilities.CreateWildcardsFromStrings(ExcludeModule, WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant);
                case "CmdletSet":
                    AccumulateMatchingCmdlets();
                case "AllCommandSet":
                    AccumulateMatchingCommands();
                        "Only the valid parameter set names should be used");
        /// Writes out the accumulated matching commands.
            // We do not show the pithy aliases (not of the format Verb-Noun) and applications by default.
            // We will show them only if the Name, All and totalCount are not specified.
            if ((this.Name == null) && (!_all) && TotalCount == -1 && !UseFuzzyMatching)
                CommandTypes commandTypesToIgnore = 0;
                if (((this.CommandType & CommandTypes.Alias) != CommandTypes.Alias) || (!_isCommandTypeSpecified))
                    commandTypesToIgnore |= CommandTypes.Alias;
                if (((_commandType & CommandTypes.Application) != CommandTypes.Application) ||
                    (!_isCommandTypeSpecified))
                    commandTypesToIgnore |= CommandTypes.Application;
                _accumulatedResults =
                    _accumulatedResults.Where(
                        commandInfo =>
                        (((commandInfo.CommandType & commandTypesToIgnore) == 0) ||
                         (commandInfo.Name.IndexOf('-') > 0))).ToList();
            // report not-found errors for ParameterName and ParameterType if needed
            if ((_matchedParameterNames != null) && (ParameterName != null))
                foreach (string requestedParameterName in ParameterName)
                    if (WildcardPattern.ContainsWildcardCharacters(requestedParameterName))
                    if (_matchedParameterNames.Contains(requestedParameterName))
                        DiscoveryExceptions.CommandParameterNotFound,
                        requestedParameterName);
                    var exception = new ArgumentException(errorMessage, requestedParameterName);
                    var errorRecord = new ErrorRecord(exception, "CommandParameterNotFound",
                                                      ErrorCategory.ObjectNotFound, requestedParameterName);
            // Only sort if they didn't fully specify a name)
            if ((_names == null) || (_nameContainsWildcard))
                // Use the stable sorting to sort the result list
                _accumulatedResults = _accumulatedResults.Order(new CommandInfoComparer()).ToList();
            OutputResultsHelper(_accumulatedResults);
            object pssenderInfo = Context.GetVariableValue(SpecialVariables.PSSenderInfoVarPath);
            if ((pssenderInfo != null) && (pssenderInfo is System.Management.Automation.Remoting.PSSenderInfo))
                // Win8: 593295. Exchange has around 1000 cmdlets. During Import-PSSession,
                // Get-Command  | select-object ..,HelpURI,... is run. HelpURI is a script property
                // which in turn runs Get-Help. Get-Help loads the help content and caches it in the process.
                // This caching is using around 190 MB. During V3, we have implemented HelpURI attribute
                // and this should solve it. In V2, we dont have this attribute and hence 3rd parties
                // run into the same issue. The fix here is to reset help cache whenever get-command is run on
                // a remote endpoint. In the worst case, this will affect get-help to run a little longer
                // after get-command is run..but that should be OK because get-help is used mainly for
                // document reading purposes and not in production.
                Context.HelpSystem.ResetHelpProviders();
        private void OutputResultsHelper(IEnumerable<CommandInfo> results)
                _commandScores = _commandScores.OrderBy(static x => x.Score).ToList();
                results = _commandScores.Select(static x => x.Command);
            foreach (CommandInfo result in results)
                if (SessionState.IsVisible(origin, result))
                    // If the -syntax flag was specified, write the definition as a string
                    // otherwise just return the object...
                    if (Syntax)
                        if (!string.IsNullOrEmpty(result.Syntax))
                            PSObject syntax = GetSyntaxObject(result);
                            WriteObject(syntax);
                        if (ShowCommandInfo.IsPresent)
                            // Write output as ShowCommandCommandInfo object.
                                ConvertToShowCommandInfo(result));
                                PSObject obj = new PSObject(result);
                                obj.Properties.Add(new PSNoteProperty("Score", _commandScores[count].Score));
                count += 1;
            _timer.Stop();
            // No telemetry here - capturing the name of a command which we are not familiar with
            // may be confidential customer information
            // We want telemetry on commands people look for but don't exist - this should give us an idea
            // what sort of commands people expect but either don't exist, or maybe should be installed by default.
            // The StartsWith is to avoid logging telemetry when suggestion mode checks the
            // current directory for scripts/exes in the current directory and '.' is not in the path.
            if (count == 0 && Name != null && Name.Length > 0 && !Name[0].StartsWith(".\\", StringComparison.OrdinalIgnoreCase))
                Telemetry.Internal.TelemetryAPI.ReportGetCommandFailed(Name, _timer.ElapsedMilliseconds);
        /// Creates the syntax output based on if the command is an alias, script, application or command.
        /// CommandInfo object containing the syntax to be output.
        /// Syntax string cast as a PSObject for outputting.
        private PSObject GetSyntaxObject(CommandInfo command)
            PSObject syntax = PSObject.AsPSObject(command.Syntax);
            // This is checking if the command name that's been passed in is one that was specified by a user,
            // if not then we have to assume they specified an alias or a wildcard and do some extra formatting for those,
            // if it is then just go with the default formatting.
            // So if a user runs Get-Command -Name del -Syntax the code will find del and the command it resolves to as Remove-Item
            // and attempt to return that, but as the user specified del we want to fiddle with the output a bit to make it clear
            // that's an alias but still give the Remove-Item syntax.
            if (this.Name != null && !Array.Exists(this.Name, name => name.Equals(command.Name, StringComparison.InvariantCultureIgnoreCase)))
                string aliasName = _nameContainsWildcard ? command.Name : this.Name[0];
                IDictionary<string, AliasInfo> aliasTable = SessionState.Internal.GetAliasTable();
                    if ((Array.Exists(this.Name, name => name.Equals(tableEntry.Key, StringComparison.InvariantCultureIgnoreCase)) &&
                        tableEntry.Value.Definition == command.Name) ||
                        (_nameContainsWildcard && tableEntry.Value.Definition == command.Name))
                        aliasName = tableEntry.Key;
                string replacedSyntax = string.Empty;
                switch (command)
                    case ExternalScriptInfo externalScript:
                        replacedSyntax = string.Format(
                            "{0} (alias) -> {1}{2}{3}",
                            string.Format("{0}{1}", externalScript.Path, Environment.NewLine),
                            Environment.NewLine,
                            command.Syntax.Replace(command.Name, aliasName));
                    case ApplicationInfo app:
                        replacedSyntax = app.Path;
                        if (aliasName.Equals(command.Name))
                            replacedSyntax = command.Syntax;
                                command.Name,
                syntax = PSObject.AsPSObject(replacedSyntax);
            syntax.IsHelpObject = true;
            return syntax;
        /// The comparer to sort CommandInfo objects in the result list.
        private sealed class CommandInfoComparer : IComparer<CommandInfo>
            /// Compare two CommandInfo objects first by their command types, and if they
            /// are with the same command type, then we compare their names.
            /// <param name="x"></param>
            /// <param name="y"></param>
            public int Compare(CommandInfo x, CommandInfo y)
                if ((int)x.CommandType < (int)y.CommandType)
                else if ((int)x.CommandType > (int)y.CommandType)
                    return string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
        private void AccumulateMatchingCmdlets()
            _commandType = CommandTypes.Cmdlet | CommandTypes.Function | CommandTypes.Filter | CommandTypes.Alias | CommandTypes.Configuration;
            Collection<string> commandNames = new Collection<string>();
            commandNames.Add("*");
            AccumulateMatchingCommands(commandNames);
        private bool IsNounVerbMatch(CommandInfo command)
                _verbPatterns ??= SessionStateUtilities.CreateWildcardsFromStrings(Verb, WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant);
                _nounPatterns ??= SessionStateUtilities.CreateWildcardsFromStrings(Noun, WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant);
                if (!string.IsNullOrEmpty(command.ModuleName))
                    if (_excludedModulePatterns is not null
                        && _excludedModulePatterns.Count > 0
                        && SessionStateUtilities.MatchesAnyWildcardPattern(command.ModuleName, _excludedModulePatterns, true))
                    if (_isFullyQualifiedModuleSpecified)
                        if (!_moduleSpecifications.Any(
                                moduleSpecification =>
                                ModuleIntrinsics.IsModuleMatchingModuleSpec(command.Module, moduleSpecification)))
                    else if (!SessionStateUtilities.MatchesAnyWildcardPattern(command.ModuleName, _modulePatterns, true))
                    if (_modulePatterns.Count > 0 || _moduleSpecifications.Length > 0)
                        // Its not a match if we are filtering on a PSSnapin/Module name but the cmdlet doesn't have one.
                // Get the noun and verb to check...
                string verb;
                string noun;
                CmdletInfo cmdlet = command as CmdletInfo;
                if (cmdlet != null)
                    verb = cmdlet.Verb;
                    noun = cmdlet.Noun;
                    if (!CmdletInfo.SplitCmdletName(command.Name, out verb, out noun))
                if (!SessionStateUtilities.MatchesAnyWildcardPattern(verb, _verbPatterns, true))
                if (!SessionStateUtilities.MatchesAnyWildcardPattern(noun, _nounPatterns, true))
        /// Writes out the commands for the AllCommandSet using the specified CommandType.
        private void AccumulateMatchingCommands()
            Collection<string> commandNames =
                SessionStateUtilities.ConvertArrayToCollection<string>(this.Name);
            if (commandNames.Count == 0)
        private void AccumulateMatchingCommands(IEnumerable<string> commandNames)
            // First set the search options
            SearchResolutionOptions options = SearchResolutionOptions.None;
            if (All)
                options = SearchResolutionOptions.SearchAllScopes;
            if (UseAbbreviationExpansion)
                options |= SearchResolutionOptions.UseAbbreviationExpansion;
            if ((this.CommandType & CommandTypes.Alias) != 0)
                options |= SearchResolutionOptions.ResolveAliasPatterns;
            if ((this.CommandType & (CommandTypes.Function | CommandTypes.Filter | CommandTypes.Configuration)) != 0)
                options |= SearchResolutionOptions.ResolveFunctionPatterns;
            foreach (string commandName in commandNames)
                    // Determine if the command name is module-qualified, and search
                    // available modules for the command.
                    string plainCommandName = Utils.ParseCommandName(commandName, out moduleName);
                    bool isModuleQualified = (moduleName != null);
                    // If they've specified a module name, we can do some smarter filtering.
                    // Otherwise, we have to filter everything.
                    if ((this.Module.Length == 1) && (!WildcardPattern.ContainsWildcardCharacters(this.Module[0])))
                        moduleName = this.Module[0];
                    bool isPattern = WildcardPattern.ContainsWildcardCharacters(plainCommandName) || UseAbbreviationExpansion || UseFuzzyMatching;
                    if (isPattern)
                        options |= SearchResolutionOptions.CommandNameIsPattern;
                    // Try to initially find the command in the available commands
                    bool isDuplicate;
                    bool resultFound = FindCommandForName(options, commandName, isPattern, true, ref count, out isDuplicate);
                    // If we didn't find the command, or if it had a wildcard, also see if it
                    // is in an available module
                    if (!resultFound || isPattern)
                        // If the command name had no wildcards or was module-qualified,
                        // import the module so that we can return the fully structured data.
                        // This uses the same code path as module auto-loading.
                        if ((!isPattern) || (!string.IsNullOrEmpty(moduleName)))
                            string tempCommandName = commandName;
                            if ((!isModuleQualified) && (!string.IsNullOrEmpty(moduleName)))
                                tempCommandName = moduleName + "\\" + commandName;
                                CommandDiscovery.LookupCommandInfo(tempCommandName, this.MyInvocation.CommandOrigin, this.Context);
                                // Ignore, LookupCommandInfo doesn't handle wildcards.
                            resultFound = FindCommandForName(options, commandName, isPattern, false, ref count, out isDuplicate);
                        // Show additional commands from available modules only if ListImported is not specified
                        else if (!ListImported)
                            if (TotalCount < 0 || count < TotalCount)
                                IEnumerable<CommandInfo> commands;
                                    foreach (var commandScore in ModuleUtils.GetFuzzyMatchingCommands(
                                        plainCommandName,
                                        MyInvocation.CommandOrigin,
                                        _fuzzyMatcher,
                                        rediscoverImportedModules: true,
                                        moduleVersionRequired: _isFullyQualifiedModuleSpecified))
                                        _commandScores.Add(commandScore);
                                    commands = _commandScores.Select(static x => x.Command);
                                    commands = ModuleUtils.GetMatchingCommands(
                                        moduleVersionRequired: _isFullyQualifiedModuleSpecified,
                                        useAbbreviationExpansion: UseAbbreviationExpansion);
                                foreach (CommandInfo command in commands)
                                    // Cannot pass in "command" by ref (foreach iteration variable)
                                    CommandInfo current = command;
                                    if (IsCommandMatch(ref current, out isDuplicate) && (!IsCommandInResult(current)) && IsParameterMatch(current))
                                        _accumulatedResults.Add(current);
                                        // Make sure we don't exceed the TotalCount parameter
                                        if (TotalCount >= 0 && count >= TotalCount)
                    // If we are trying to match a single specific command name (no glob characters)
                    // then we need to write an error if we didn't find it.
                    if (!isDuplicate)
                        if (!resultFound && !isPattern)
                catch (CommandNotFoundException exception)
                            exception.ErrorRecord,
                            exception));
        private bool FindCommandForName(SearchResolutionOptions options, string commandName, bool isPattern, bool emitErrors, ref int currentCount, out bool isDuplicate)
            var searcher = new CommandSearcher(
                options,
                CommandType,
            isDuplicate = false;
                catch (ArgumentException argumentException)
                    if (emitErrors)
                        WriteError(new ErrorRecord(argumentException, "GetCommandInvalidArgument", ErrorCategory.SyntaxError, null));
                        WriteError(new ErrorRecord(pathTooLong, "GetCommandInvalidArgument", ErrorCategory.SyntaxError, null));
                        WriteError(new ErrorRecord(fileLoadException, "GetCommandFileLoadError", ErrorCategory.ReadError, null));
                        WriteError(new ErrorRecord(metadataException, "GetCommandMetadataError", ErrorCategory.MetadataError, null));
                        WriteError(new ErrorRecord(formatException, "GetCommandBadFileFormat", ErrorCategory.InvalidData, null));
                CommandInfo current = ((IEnumerator<CommandInfo>)searcher).Current;
                // skip private commands as early as possible
                // (i.e. before setting "result found" flag and before trying to use ArgumentList parameter)
                // see bugs Windows 7: #520498 and #520470
                CommandOrigin origin = this.MyInvocation.CommandOrigin;
                if (!SessionState.IsVisible(origin, current))
                bool tempResultFound = IsCommandMatch(ref current, out isDuplicate);
                if (tempResultFound && (!IsCommandInResult(current)))
                    if (IsParameterMatch(current))
                        ++currentCount;
                        if (TotalCount >= 0 && currentCount > TotalCount)
                            if (_fuzzyMatcher.IsFuzzyMatch(current.Name, commandName, out int score))
                                _commandScores.Add(new CommandScore(current, score));
                            // Don't iterate the enumerator any more. If -arguments was specified, then we stop at the first match
                    // Only for this case, the loop should exit
                    // Get-Command Foo
                    if (isPattern || All || TotalCount != -1 || _isCommandTypeSpecified || _isModuleSpecified || _isFullyQualifiedModuleSpecified)
                // Get additional matching commands from module tables.
                foreach (CommandInfo command in GetMatchingCommandsFromModules(commandName))
                    CommandInfo c = command;
                    bool tempResultFound = IsCommandMatch(ref c, out isDuplicate);
                    if (tempResultFound)
                        if (!IsCommandInResult(command) && IsParameterMatch(c))
                            _accumulatedResults.Add(c);
            return resultFound;
        /// Determines if the specific command information has already been
        /// written out based on the path or definition.
        /// The command information to check for duplication.
        /// true if the command has already been written out.
        private bool IsDuplicate(CommandInfo info)
            string key = null;
                ApplicationInfo appInfo = info as ApplicationInfo;
                if (appInfo != null)
                    key = appInfo.Path;
                CmdletInfo cmdletInfo = info as CmdletInfo;
                if (cmdletInfo != null)
                    key = cmdletInfo.FullName;
                ScriptInfo scriptInfo = info as ScriptInfo;
                    key = scriptInfo.Definition;
                ExternalScriptInfo externalScriptInfo = info as ExternalScriptInfo;
                if (externalScriptInfo != null)
                    key = externalScriptInfo.Path;
                if (_commandsWritten.ContainsKey(key))
                    _commandsWritten.Add(key, info);
        private bool IsParameterMatch(CommandInfo commandInfo)
            if ((this.ParameterName == null) && (this.ParameterType == null))
            _matchedParameterNames ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            IEnumerable<ParameterMetadata> commandParameters = null;
                IDictionary<string, ParameterMetadata> tmp = commandInfo.Parameters;
                if (tmp != null)
                    commandParameters = tmp.Values;
                // ignore all exceptions when getting parameter metadata (i.e. parse exceptions, dangling alias exceptions)
                // and proceed as if there was no parameter metadata
            if (commandParameters == null)
                // do not match commands which have not been imported yet / for which we don't have parameter metadata yet
                bool foundMatchingParameter = false;
                foreach (ParameterMetadata parameterMetadata in commandParameters)
                    if (IsParameterMatch(parameterMetadata))
                        foundMatchingParameter = true;
                        // not breaking out of the loop early, to ensure that _matchedParameterNames gets populated for all command parameters
                return foundMatchingParameter;
        private bool IsParameterMatch(ParameterMetadata parameterMetadata)
            // ParameterName matching
            bool nameIsDirectlyMatching = SessionStateUtilities.MatchesAnyWildcardPattern(parameterMetadata.Name, _parameterNameWildcards, true);
            bool oneOfAliasesIsMatching = false;
            foreach (string alias in parameterMetadata.Aliases ?? Enumerable.Empty<string>())
                if (SessionStateUtilities.MatchesAnyWildcardPattern(alias, _parameterNameWildcards, true))
                    _matchedParameterNames.Add(alias);
                    oneOfAliasesIsMatching = true;
                    // don't want to break out of the loop early (need to fully populate _matchedParameterNames hashset)
            bool nameIsMatching = nameIsDirectlyMatching || oneOfAliasesIsMatching;
            if (nameIsMatching)
                _matchedParameterNames.Add(parameterMetadata.Name);
            // ParameterType matching
            bool typeIsMatching;
            if ((_parameterTypes == null) || (_parameterTypes.Length == 0))
                typeIsMatching = true;
                typeIsMatching = false;
                if (_parameterTypes != null &&
                    _parameterTypes.Length > 0)
                    typeIsMatching |= _parameterTypes.Any(parameterMetadata.IsMatchingType);
            return nameIsMatching && typeIsMatching;
        private bool IsCommandMatch(ref CommandInfo current, out bool isDuplicate)
            bool isCommandMatch = false;
            // Be sure we haven't already found this command before
            if (!IsDuplicate(current))
                if ((current.CommandType & this.CommandType) != 0)
                    isCommandMatch = true;
                // If the command in question is a cmdlet or (a function/filter/configuration/alias and we are filtering on nouns or verbs),
                // then do the verb/noun check
                if (current.CommandType == CommandTypes.Cmdlet ||
                    ((_verbs.Length > 0 || _nouns.Length > 0) &&
                     (current.CommandType == CommandTypes.Function ||
                      current.CommandType == CommandTypes.Filter ||
                      current.CommandType == CommandTypes.Configuration ||
                      current.CommandType == CommandTypes.Alias)))
                    if (!IsNounVerbMatch(current))
                        isCommandMatch = false;
                        && SessionStateUtilities.MatchesAnyWildcardPattern(current.ModuleName, _excludedModulePatterns, true))
                        bool foundModuleMatch = false;
                        foreach (var moduleSpecification in _moduleSpecifications)
                            if (ModuleIntrinsics.IsModuleMatchingModuleSpec(current.Module, moduleSpecification))
                                foundModuleMatch = true;
                        if (!foundModuleMatch)
                    else if (_modulePatterns != null && _modulePatterns.Count > 0)
                        if (!SessionStateUtilities.MatchesAnyWildcardPattern(current.ModuleName, _modulePatterns, true))
                if (isCommandMatch)
                    if (Syntax.IsPresent && current is AliasInfo ai)
                        // If the matching command was an alias, then use the resolved command
                        // instead of the alias...
                        current = ai.ResolvedCommand ?? CommandDiscovery.LookupCommandInfo(
                            ai.UnresolvedCommandName,
                            this.MyInvocation.CommandOrigin,
                        // there are situations where both ResolvedCommand and UnresolvedCommandName
                        // are both null (often due to multiple versions of modules with aliases)
                        // therefore we need to exit early.
                        if (current == null)
                    if (ArgumentList != null
                        && current is not CmdletInfo
                        && current is not IScriptCommandInfo)
                        // If current is not a cmdlet or script, we need to throw a terminating error.
                                    "ArgumentList",
                                    DiscoveryExceptions.CommandArgsOnlyForSingleCmdlet),
                                "CommandArgsOnlyForSingleCmdlet",
                                current));
                    // If the command implements dynamic parameters
                    // then we must make a copy of the CommandInfo which merges the
                    // dynamic parameter metadata with the statically defined parameter
                    // metadata
                        // We can ignore some errors that occur when checking if
                        // the command implements dynamic parameters.
                        needCopy = current.ImplementsDynamicParameters;
                        // Ignore execution policies in get-command, those will get
                        // raised when trying to run the real command
                        // Ignore parse/runtime exceptions.  Again, they will get
                        // raised again if the script is actually run.
                            CommandInfo newCurrent = current.CreateGetCommandCopy(ArgumentList);
                                // We need to prepopulate the parameter metadata in the CmdletInfo to
                                // ensure there are no errors. Getting the ParameterSets property
                                // triggers the parameter metadata to be generated
                                ReadOnlyCollection<CommandParameterSetInfo> parameterSets =
                                    newCurrent.ParameterSets;
                            current = newCurrent;
                            // A metadata exception can be thrown if the dynamic parameters duplicates a parameter
                            // of the cmdlet.
                            WriteError(new ErrorRecord(metadataException, "GetCommandMetadataError",
                                                       ErrorCategory.MetadataError, current));
                        catch (ParameterBindingException parameterBindingException)
                            // if the exception is thrown when retrieving dynamic parameters, ignore it and
                            // the static parameter info will be used.
                            if (!parameterBindingException.ErrorRecord.FullyQualifiedErrorId.StartsWith(
                                "GetDynamicParametersException", StringComparison.Ordinal))
                isDuplicate = true;
            return isCommandMatch;
        /// Gets matching commands from the module tables.
        /// The commandname to look for
        /// IEnumerable of CommandInfo objects
        private IEnumerable<CommandInfo> GetMatchingCommandsFromModules(string commandName)
            WildcardPattern matcher = WildcardPattern.Get(
            // Use ModuleTableKeys list in reverse order
            for (int i = Context.EngineSessionState.ModuleTableKeys.Count - 1; i >= 0; i--)
                PSModuleInfo module = null;
                if (!Context.EngineSessionState.ModuleTable.TryGetValue(Context.EngineSessionState.ModuleTableKeys[i], out module))
                    Dbg.Assert(false, "ModuleTableKeys should be in sync with ModuleTable");
                    bool isModuleMatch = false;
                    if (!_isFullyQualifiedModuleSpecified)
                        isModuleMatch = SessionStateUtilities.MatchesAnyWildcardPattern(module.Name, _modulePatterns, true);
                    else if (_moduleSpecifications.Any(moduleSpecification => ModuleIntrinsics.IsModuleMatchingModuleSpec(module, moduleSpecification)))
                        isModuleMatch = true;
                    if (isModuleMatch)
                        if (module.SessionState != null)
                            // Look in function table
                                foreach ((string functionName, FunctionInfo functionInfo) in module.SessionState.Internal.GetFunctionTable())
                                    if (matcher.IsMatch(functionName) && functionInfo.IsImported)
                                        // make sure function doesn't come from the current module's nested module
                                        if (functionInfo.Module.Path.Equals(module.Path, StringComparison.OrdinalIgnoreCase))
                                            yield return functionInfo;
                            // Look in alias table
                                foreach (var alias in module.SessionState.Internal.GetAliasTable())
                                    if (matcher.IsMatch(alias.Key) && alias.Value.IsImported)
                                        // make sure alias doesn't come from the current module's nested module
                                        if (alias.Value.Module.Path.Equals(module.Path, StringComparison.OrdinalIgnoreCase))
                                            yield return alias.Value;
        /// added to the result from CommandSearcher.
        /// true if the command is present in the result.
        private bool IsCommandInResult(CommandInfo command)
            bool isPresent = false;
            if (command.Module is not null)
                foreach (CommandInfo commandInfo in _accumulatedResults)
                    if (commandInfo.Module is null || commandInfo.CommandType != command.CommandType)
                    // We do reference equal comparison if both command are imported. If either one is not imported, we compare the module path
                    if ((!commandInfo.IsImported || !command.IsImported || !commandInfo.Module.Equals(command.Module))
                        && ((commandInfo.IsImported && command.IsImported) || !commandInfo.Module.Path.Equals(command.Module.Path, StringComparison.OrdinalIgnoreCase)))
                    // If the command has been imported with a prefix, then just checking the names for duplication will not be enough.
                    // Hence, an additional check is done with the prefix information
                    if (commandInfo.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase)
                        || ModuleCmdletBase.RemovePrefixFromCommandName(commandInfo.Name, commandInfo.Prefix).Equals(command.Name, StringComparison.OrdinalIgnoreCase))
                        isPresent = true;
            return isPresent;
        #region Members
        private readonly Dictionary<string, CommandInfo> _commandsWritten =
            new Dictionary<string, CommandInfo>(StringComparer.OrdinalIgnoreCase);
        private List<CommandInfo> _accumulatedResults = new List<CommandInfo>();
        // These members are the collection of wildcard patterns for the "CmdletSet"
        private Collection<WildcardPattern> _verbPatterns;
        private Collection<WildcardPattern> _nounPatterns;
        private Collection<WildcardPattern> _modulePatterns;
        private Collection<WildcardPattern> _excludedModulePatterns;
        private Stopwatch _timer = new Stopwatch();
        #region ShowCommandInfo support
        // Converts to PSObject containing ShowCommand information.
        private static PSObject ConvertToShowCommandInfo(CommandInfo cmdInfo)
            PSObject showCommandInfo = new PSObject();
            showCommandInfo.Properties.Add(new PSNoteProperty("Name", cmdInfo.Name));
            showCommandInfo.Properties.Add(new PSNoteProperty("ModuleName", cmdInfo.ModuleName));
            showCommandInfo.Properties.Add(new PSNoteProperty("Module", GetModuleInfo(cmdInfo)));
            showCommandInfo.Properties.Add(new PSNoteProperty("CommandType", cmdInfo.CommandType));
            showCommandInfo.Properties.Add(new PSNoteProperty("Definition", cmdInfo.Definition));
            showCommandInfo.Properties.Add(new PSNoteProperty("ParameterSets", GetParameterSets(cmdInfo)));
            return showCommandInfo;
        private static PSObject GetModuleInfo(CommandInfo cmdInfo)
            PSObject moduleInfo = new PSObject();
            string moduleName = (cmdInfo.Module != null) ? cmdInfo.Module.Name : string.Empty;
            moduleInfo.Properties.Add(new PSNoteProperty("Name", moduleName));
            return moduleInfo;
        private static PSObject[] GetParameterSets(CommandInfo cmdInfo)
            ReadOnlyCollection<CommandParameterSetInfo> parameterSets = null;
                if (cmdInfo.ParameterSets != null)
                    parameterSets = cmdInfo.ParameterSets;
            catch (PSNotSupportedException) { }
            if (parameterSets == null)
                return Array.Empty<PSObject>();
            List<PSObject> returnParameterSets = new List<PSObject>(cmdInfo.ParameterSets.Count);
            foreach (CommandParameterSetInfo parameterSetInfo in parameterSets)
                PSObject parameterSetObj = new PSObject();
                parameterSetObj.Properties.Add(new PSNoteProperty("Name", parameterSetInfo.Name));
                parameterSetObj.Properties.Add(new PSNoteProperty("IsDefault", parameterSetInfo.IsDefault));
                parameterSetObj.Properties.Add(new PSNoteProperty("Parameters", GetParameterInfo(parameterSetInfo.Parameters)));
                returnParameterSets.Add(parameterSetObj);
            return returnParameterSets.ToArray();
        private static PSObject[] GetParameterInfo(ReadOnlyCollection<CommandParameterInfo> parameters)
            List<PSObject> parameterObjs = new List<PSObject>(parameters.Count);
            foreach (CommandParameterInfo parameter in parameters)
                PSObject parameterObj = new PSObject();
                parameterObj.Properties.Add(new PSNoteProperty("Name", parameter.Name));
                parameterObj.Properties.Add(new PSNoteProperty("IsMandatory", parameter.IsMandatory));
                parameterObj.Properties.Add(new PSNoteProperty("ValueFromPipeline", parameter.ValueFromPipeline));
                parameterObj.Properties.Add(new PSNoteProperty("Position", parameter.Position));
                parameterObj.Properties.Add(new PSNoteProperty("ParameterType", GetParameterType(parameter.ParameterType)));
                bool hasParameterSet = false;
                IList<string> validValues = new List<string>();
                var validateSetAttribute = parameter.Attributes.OfType<ValidateSetAttribute>().LastOrDefault();
                    hasParameterSet = true;
                    validValues = validateSetAttribute.ValidValues;
                parameterObj.Properties.Add(new PSNoteProperty("HasParameterSet", hasParameterSet));
                parameterObj.Properties.Add(new PSNoteProperty("ValidParamSetValues", validValues));
                parameterObjs.Add(parameterObj);
            return parameterObjs.ToArray();
        private static PSObject GetParameterType(Type parameterType)
            PSObject returnParameterType = new PSObject();
            bool isEnum = parameterType.IsEnum;
            bool isArray = parameterType.IsArray;
            returnParameterType.Properties.Add(new PSNoteProperty("FullName", parameterType.FullName));
            returnParameterType.Properties.Add(new PSNoteProperty("IsEnum", isEnum));
            returnParameterType.Properties.Add(new PSNoteProperty("IsArray", isArray));
            ArrayList enumValues = (isEnum) ?
                new ArrayList(Enum.GetValues(parameterType)) : new ArrayList();
            returnParameterType.Properties.Add(new PSNoteProperty("EnumValues", enumValues));
            bool hasFlagAttribute = (isArray) && ((parameterType.GetCustomAttributes(typeof(FlagsAttribute), true)).Length > 0);
            returnParameterType.Properties.Add(new PSNoteProperty("HasFlagAttribute", hasFlagAttribute));
            // Recurse into array elements.
            object elementType = (isArray) ?
                GetParameterType(parameterType.GetElementType()) : null;
            returnParameterType.Properties.Add(new PSNoteProperty("ElementType", elementType));
            bool implementsDictionary = (!isEnum && !isArray && (parameterType is IDictionary));
            returnParameterType.Properties.Add(new PSNoteProperty("ImplementsDictionary", implementsDictionary));
            return returnParameterType;
    /// Provides argument completion for Noun parameter.
    public class NounArgumentCompleter : IArgumentCompleter
        /// Returns completion results for Noun parameter.
        /// <returns>List of completion results.</returns>
            IDictionary fakeBoundParameters) => CompletionHelpers.GetMatchingResults(
                possibleCompletionValues: GetCommandNouns(fakeBoundParameters));
        /// Get sorted set of command nouns using Get-Command.
        /// <returns>Sorted set of command nouns.</returns>
        private static SortedSet<string> GetCommandNouns(IDictionary fakeBoundParameters)
            Collection<CommandInfo> commands = CompletionCompleters.GetCommandInfo(fakeBoundParameters, "Module", "Verb");
            SortedSet<string> nouns = new(StringComparer.OrdinalIgnoreCase);
                string commandName = command.Name;
                int dashIndex = commandName.IndexOf('-');
                if (dashIndex != -1)
                    string noun = commandName.Substring(dashIndex + 1);
                    nouns.Add(noun);
            return nouns;

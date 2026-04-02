    /// The implementation of the "import-alias" cmdlet.
    [Cmdlet(VerbsData.Import, "Alias", SupportsShouldProcess = true, DefaultParameterSetName = "ByPath", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097125")]
    public class ImportAliasCommand : PSCmdlet
        #region Statics
        private const string LiteralPathParameterSetName = "ByLiteralPath";
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPath")]
        /// The literal path from which to import the aliases.
        [Parameter(Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = LiteralPathParameterSetName)]
        /// If set to true and an existing alias of the same name exists
        /// and is ReadOnly, the alias will be overwritten.
            Collection<AliasInfo> importedAliases = GetAliasesFromFile(this.ParameterSetName.Equals(LiteralPathParameterSetName,
                StringComparison.OrdinalIgnoreCase));
            foreach (AliasInfo alias in importedAliases)
                // If not force, then see if the alias already exists
                // NTRAID#Windows Out Of Band Releases-906910-2006/03/17-JonN
                string action = AliasCommandStrings.ImportAliasAction;
                string target = StringUtil.Format(AliasCommandStrings.ImportAliasTarget, alias.Name, alias.Definition);
                    AliasInfo existingAlias = null;
                    if (string.IsNullOrEmpty(Scope))
                        existingAlias = SessionState.Internal.GetAlias(alias.Name);
                        existingAlias = SessionState.Internal.GetAliasAtScope(alias.Name, Scope);
                    if (existingAlias != null)
                        // Write an error for aliases that aren't visible...
                            SessionState.ThrowIfNotVisible(origin, existingAlias);
                            // Only report the error once...
                        // Since the alias already exists, write an error.
                        SessionStateException aliasExists =
                                SessionStateCategory.Alias,
                                "AliasAlreadyExists",
                                SessionStateStrings.AliasAlreadyExists,
                                ErrorCategory.ResourceExists);
                                aliasExists.ErrorRecord,
                                aliasExists));
                    if (VerifyShadowingExistingCommandsAndWriteError(alias.Name))
                // Set the alias in the specified scope or the
                // current scope.
                AliasInfo result = null;
                        result = SessionState.Internal.SetAliasItem(alias, Force, MyInvocation.CommandOrigin);
                        result = SessionState.Internal.SetAliasItemAtScope(alias, Scope, Force, MyInvocation.CommandOrigin);
                catch (PSArgumentOutOfRangeException argOutOfRange)
                            argOutOfRange.ErrorRecord,
                            argOutOfRange));
                // Write the alias to the pipeline if PassThru was specified
                if (PassThru && result != null)
        private Dictionary<string, CommandTypes> _existingCommands;
        private Dictionary<string, CommandTypes> ExistingCommands
                    _existingCommands = new Dictionary<string, CommandTypes>(StringComparer.OrdinalIgnoreCase);
                        CommandTypes.All ^ CommandTypes.Alias,
                        _existingCommands[commandInfo.Name] = commandInfo.CommandType;
                    // Also add commands from the analysis cache
                    foreach (CommandInfo commandInfo in System.Management.Automation.Internal.ModuleUtils.GetMatchingCommands("*", this.Context, this.MyInvocation.CommandOrigin))
                        if (!_existingCommands.ContainsKey(commandInfo.Name))
        private bool VerifyShadowingExistingCommandsAndWriteError(string aliasName)
            CommandSearcher searcher = new(aliasName, SearchResolutionOptions.None, CommandTypes.All ^ CommandTypes.Alias, this.Context);
            foreach (string expandedCommandName in searcher.ConstructSearchPatternsFromName(aliasName))
                CommandTypes commandTypeOfExistingCommand;
                if (this.ExistingCommands.TryGetValue(expandedCommandName, out commandTypeOfExistingCommand))
                            SessionStateStrings.AliasWithCommandNameAlreadyExists,
                            commandTypeOfExistingCommand);
        private Collection<AliasInfo> GetAliasesFromFile(bool isLiteralPath)
            Collection<AliasInfo> result = new();
            string filePath = null;
            using (StreamReader reader = OpenFile(out filePath, isLiteralPath))
                CSVHelper csvHelper = new(',');
                long lineNumber = 0;
                while ((line = reader.ReadLine()) != null)
                    ++lineNumber;
                    // Ignore blank lines
                    if (line.Length == 0)
                    // Ignore lines that only contain whitespace
                    if (OnlyContainsWhitespace(line))
                    // Ignore comment lines
                    if (line[0] == '#')
                    Collection<string> values = csvHelper.ParseCsv(line);
                    if (values.Count != 4)
                        string message = StringUtil.Format(AliasCommandStrings.ImportAliasFileInvalidFormat, filePath, lineNumber);
                        FormatException formatException =
                            new(message);
                                formatException,
                                "ImportAliasFileFormatError",
                    ScopedItemOptions options = ScopedItemOptions.None;
                        options = (ScopedItemOptions)Enum.Parse(typeof(ScopedItemOptions), values[3], true);
                        string message = StringUtil.Format(AliasCommandStrings.ImportAliasOptionsError, filePath, lineNumber);
                                "ImportAliasOptionsError",
                    AliasInfo newAlias =
                            values[0],
                            values[1],
                            Context,
                            options);
                    if (!string.IsNullOrEmpty(values[2]))
                        newAlias.Description = values[2];
                    result.Add(newAlias);
                reader.Dispose();
        private StreamReader OpenFile(out string filePath, bool isLiteralPath)
            StreamReader result = null;
            filePath = null;
            Collection<string> paths = null;
            if (isLiteralPath)
                paths = new Collection<string>();
                PSDriveInfo drive;
                paths.Add(SessionState.Path.GetUnresolvedProviderPathFromPSPath(this.Path, out provider, out drive));
                // first resolve the path
                paths = SessionState.Path.GetResolvedProviderPathFromPSPath(this.Path, out provider);
            // We can only export aliases to the file system
            if (!provider.NameEquals(this.Context.ProviderNames.FileSystem))
                        AliasCommandStrings.ImportAliasFromFileSystemOnly,
                        provider.FullName);
            // We can only write to a single file at a time.
            if (paths.Count != 1)
                        AliasCommandStrings.ImportAliasPathResolvedToMultiple,
            filePath = paths[0];
                FileStream file = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                result = new StreamReader(file);
                ThrowFileOpenError(ioException, filePath);
            catch (SecurityException securityException)
                ThrowFileOpenError(securityException, filePath);
            catch (UnauthorizedAccessException unauthorizedAccessException)
                ThrowFileOpenError(unauthorizedAccessException, filePath);
                StringUtil.Format(AliasCommandStrings.ImportAliasFileOpenFailed, pathWithError, e.Message);
        private static bool OnlyContainsWhitespace(string line)
            foreach (char c in line)
                if (char.IsWhiteSpace(c) && c != '\n' && c != '\r')

    internal static class ModuleUtils
        // These are documented members FILE_ATTRIBUTE, they just have not yet been
        // added to System.IO.FileAttributes yet.
        private const int FILE_ATTRIBUTE_RECALL_ON_DATA_ACCESS = 0x400000;
        private const int FILE_ATTRIBUTE_RECALL_ON_OPEN = 0x40000;
        // Default option for local file system enumeration:
        //  - Ignore files/directories when access is denied;
        //  - Search top directory only.
        private static readonly System.IO.EnumerationOptions s_defaultEnumerationOptions =
                                        new System.IO.EnumerationOptions() { AttributesToSkip = FileAttributesToSkip };
        private static readonly FileAttributes FileAttributesToSkip;
        // Default option for UNC path enumeration. Same as above plus a large buffer size.
        // For network shares, a large buffer may result in better performance as more results can be batched over the wire.
        // The buffer size 16K is recommended in the comment of the 'BufferSize' property:
        //    "A "large" buffer, for example, would be 16K. Typical is 4K."
        private static readonly System.IO.EnumerationOptions s_uncPathEnumerationOptions =
                                        new System.IO.EnumerationOptions() { AttributesToSkip = FileAttributesToSkip, BufferSize = 16384 };
        private static readonly string EnCulturePath = Path.DirectorySeparatorChar + "en";
        private static readonly string EnUsCulturePath = Path.DirectorySeparatorChar + "en-us";
        static ModuleUtils()
            FileAttributesToSkip = FileAttributes.Hidden
                // Skip OneDrive files/directories that are not fully on disk.
                | FileAttributes.Offline
                | (FileAttributes)FILE_ATTRIBUTE_RECALL_ON_DATA_ACCESS
                | (FileAttributes)FILE_ATTRIBUTE_RECALL_ON_OPEN;
        /// Check if a directory is likely a localized resources folder.
        /// <param name="dir">Directory to check if it is a possible resource folder.</param>
        /// <returns>True if the directory name matches a culture.</returns>
        internal static bool IsPossibleResourceDirectory(string dir)
            // Assume locale directories do not contain modules.
            if (dir.EndsWith(EnCulturePath, StringComparison.OrdinalIgnoreCase) ||
                dir.EndsWith(EnUsCulturePath, StringComparison.OrdinalIgnoreCase))
            dir = Path.GetFileName(dir);
            // Use some simple pattern matching to avoid the call into GetCultureInfo when we know it will fail (and throw).
            if ((dir.Length == 2 && char.IsLetter(dir[0]) && char.IsLetter(dir[1]))
                (dir.Length == 5 && char.IsLetter(dir[0]) && char.IsLetter(dir[1]) && (dir[2] == '-') && char.IsLetter(dir[3]) && char.IsLetter(dir[4])))
                    // This might not throw on invalid culture still
                    // 4096 is considered the unknown locale - so assume that could be a module
                    var cultureInfo = new CultureInfo(dir);
                    return cultureInfo.LCID != 4096;
        /// Get all module files by searching the given directory recursively.
        /// All sub-directories that could be a module folder will be searched.
        internal static IEnumerable<string> GetAllAvailableModuleFiles(string topDirectoryToCheck)
            if (!Directory.Exists(topDirectoryToCheck)) { yield break; }
            var options = Utils.PathIsUnc(topDirectoryToCheck) ? s_uncPathEnumerationOptions : s_defaultEnumerationOptions;
            Queue<string> directoriesToCheck = new Queue<string>();
            directoriesToCheck.Enqueue(topDirectoryToCheck);
            bool firstSubDirs = true;
            while (directoriesToCheck.Count > 0)
                string directoryToCheck = directoriesToCheck.Dequeue();
                    foreach (string toAdd in Directory.EnumerateDirectories(directoryToCheck, "*", options))
                        if (firstSubDirs || !IsPossibleResourceDirectory(toAdd))
                            directoriesToCheck.Enqueue(toAdd);
                catch (IOException) { }
                firstSubDirs = false;
                foreach (string moduleFile in Directory.EnumerateFiles(directoryToCheck, "*", options))
                    foreach (string ext in ModuleIntrinsics.PSModuleExtensions)
                        if (moduleFile.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                            break; // one file can have only one extension
        /// Check if the CompatiblePSEditions field of a given module
        /// declares compatibility with the running PowerShell edition.
        /// <param name="moduleManifestPath">The path to the module manifest being checked.</param>
        /// <param name="compatiblePSEditions">The value of the CompatiblePSEditions field of the module manifest.</param>
        /// <returns>True if the module is compatible with the running PowerShell edition, false otherwise.</returns>
        internal static bool IsPSEditionCompatible(
            IEnumerable<string> compatiblePSEditions)
            if (!IsOnSystem32ModulePath(moduleManifestPath))
            return Utils.IsPSEditionSupported(compatiblePSEditions);
        internal static IEnumerable<string> GetDefaultAvailableModuleFiles(bool isForAutoDiscovery, ExecutionContext context)
            HashSet<string> uniqueModuleFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string directory in ModuleIntrinsics.GetModulePath(isForAutoDiscovery, context))
                var needWriteProgressCompleted = false;
                ProgressRecord analysisProgress = null;
                // Write a progress message for UNC paths, so that users know what is happening
                    if ((context.CurrentCommandProcessor != null) && Utils.PathIsUnc(directory))
                        analysisProgress = new ProgressRecord(0,
                            Modules.DeterminingAvailableModules,
                            string.Format(CultureInfo.InvariantCulture, Modules.SearchingUncShare, directory))
                            RecordType = ProgressRecordType.Processing
                        needWriteProgressCompleted = true;
                    // This may be called when we are not allowed to write progress,
                    // So eat the invalid operation
                    foreach (string moduleFile in ModuleUtils.GetDefaultAvailableModuleFiles(directory))
                        if (uniqueModuleFiles.Add(moduleFile))
                    if (needWriteProgressCompleted)
        /// Get a list of module files from the given directory without recursively searching all sub-directories.
        /// This method assumes the given directory is a module folder or a version sub-directory of a module folder.
        internal static List<string> GetModuleFilesFromAbsolutePath(string directory)
            string fileName = Path.GetFileName(directory);
            // If the given directory doesn't exist or it's the root folder, then return an empty list.
            if (!Directory.Exists(directory) || string.IsNullOrEmpty(fileName)) { return result; }
            // If the user give the module path including version, the module name could be the parent folder name.
            if (Version.TryParse(fileName, out Version ver))
                string parentDirPath = Path.GetDirectoryName(directory);
                string parentDirName = Path.GetFileName(parentDirPath);
                // If the parent directory is NOT a root folder, then it could be the module folder.
                if (!string.IsNullOrEmpty(parentDirName))
                    string manifestPath = Path.Combine(directory, parentDirName);
                    manifestPath += StringLiterals.PowerShellDataFileExtension;
                    if (File.Exists(manifestPath) && ver.Equals(ModuleIntrinsics.GetManifestModuleVersion(manifestPath)))
                        result.Add(manifestPath);
            // If we reach here, then use the given directory as the module folder.
            foreach (Version version in GetModuleVersionSubfolders(directory))
                string manifestPath = Path.Combine(directory, version.ToString(), fileName);
                if (File.Exists(manifestPath) && version.Equals(ModuleIntrinsics.GetManifestModuleVersion(manifestPath)))
                string moduleFile = Path.Combine(directory, fileName) + ext;
                if (File.Exists(moduleFile))
                    result.Add(moduleFile);
                    // when finding the default modules we stop when the first
                    // match is hit - searching in order .psd1, .psm1, .dll,
                    // if a file is found but is not readable then it is an error.
        /// Get a list of the available module files from the given directory.
        /// Search all module folders under the specified directory, but do not search sub-directories under a module folder.
        internal static IEnumerable<string> GetDefaultAvailableModuleFiles(string topDirectoryToCheck)
            List<Version> versionDirectories = new List<Version>();
            LinkedList<string> directoriesToCheck = new LinkedList<string>();
            directoriesToCheck.AddLast(topDirectoryToCheck);
                versionDirectories.Clear();
                string[] subdirectories;
                string directoryToCheck = directoriesToCheck.First.Value;
                directoriesToCheck.RemoveFirst();
                    subdirectories = Directory.GetDirectories(directoryToCheck, "*", options);
                    ProcessPossibleVersionSubdirectories(subdirectories, versionDirectories);
                catch (IOException) { subdirectories = Array.Empty<string>(); }
                catch (UnauthorizedAccessException) { subdirectories = Array.Empty<string>(); }
                bool isModuleDirectory = false;
                string proposedModuleName = Path.GetFileName(directoryToCheck);
                foreach (Version version in versionDirectories)
                    string manifestPath = Path.Combine(directoryToCheck, version.ToString(), proposedModuleName);
                        if (HasSkippedFileAttribute(manifestPath))
                        isModuleDirectory = true;
                        yield return manifestPath;
                if (!isModuleDirectory)
                        string moduleFile = Path.Combine(directoryToCheck, proposedModuleName) + ext;
                            if (HasSkippedFileAttribute(moduleFile))
                            // match is hit - searching in order .psd1, .psm1, .dll, .exe
                            // if a file is found but is not readable then it is an
                    foreach (var subdirectory in subdirectories)
                        if (subdirectory.EndsWith("Microsoft.PowerShell.Management", StringComparison.OrdinalIgnoreCase) ||
                            subdirectory.EndsWith("Microsoft.PowerShell.Utility", StringComparison.OrdinalIgnoreCase))
                            directoriesToCheck.AddFirst(subdirectory);
                            directoriesToCheck.AddLast(subdirectory);
        /// Gets the list of versions under the specified module base path in descending sorted order.
        /// <param name="moduleBase">Module base path.</param>
        /// <returns>Sorted list of versions.</returns>
        internal static List<Version> GetModuleVersionSubfolders(string moduleBase)
            var versionFolders = new List<Version>();
            if (!string.IsNullOrWhiteSpace(moduleBase) && Directory.Exists(moduleBase))
                var options = Utils.PathIsUnc(moduleBase) ? s_uncPathEnumerationOptions : s_defaultEnumerationOptions;
                IEnumerable<string> subdirectories = Directory.EnumerateDirectories(moduleBase, "*", options);
                ProcessPossibleVersionSubdirectories(subdirectories, versionFolders);
            return versionFolders;
        private static bool HasSkippedFileAttribute(string path)
                FileAttributes attributes = File.GetAttributes(path);
                if ((attributes & FileAttributesToSkip) is not 0)
                // Ignore failures so that we keep the current behavior of failing
                // later in the search.
        private static void ProcessPossibleVersionSubdirectories(IEnumerable<string> subdirectories, List<Version> versionFolders)
            foreach (string subdir in subdirectories)
                string subdirName = Path.GetFileName(subdir);
                if (Version.TryParse(subdirName, out Version version))
                    versionFolders.Add(version);
            if (versionFolders.Count > 1)
                versionFolders.Sort(static (x, y) => y.CompareTo(x));
        internal static bool IsModuleInVersionSubdirectory(string modulePath, out Version version)
            string folderName = Path.GetDirectoryName(modulePath);
            if (folderName != null)
                folderName = Path.GetFileName(folderName);
                return Version.TryParse(folderName, out version);
        internal static bool IsOnSystem32ModulePath(string path)
            Dbg.Assert(!string.IsNullOrEmpty(path), $"Caller to verify that {nameof(path)} is not null or empty");
            string windowsPowerShellPSHomePath = ModuleIntrinsics.GetWindowsPowerShellPSHomeModulePath();
            return path.StartsWith(windowsPowerShellPSHomePath, StringComparison.OrdinalIgnoreCase);
        /// Gets a list of fuzzy matching commands and their scores.
        /// <param name="pattern">Command pattern.</param>
        /// <param name="commandOrigin">Command origin.</param>
        /// <param name="fuzzyMatcher">Fuzzy matcher to use.</param>
        /// <param name="rediscoverImportedModules">If true, rediscovers imported modules.</param>
        /// <param name="moduleVersionRequired">Specific module version to be required.</param>
        /// <returns>IEnumerable tuple containing the CommandInfo and the match score.</returns>
        internal static IEnumerable<CommandScore> GetFuzzyMatchingCommands(string pattern, ExecutionContext context, CommandOrigin commandOrigin, FuzzyMatcher fuzzyMatcher, bool rediscoverImportedModules = false, bool moduleVersionRequired = false)
            foreach (CommandInfo command in GetMatchingCommands(pattern, context, commandOrigin, rediscoverImportedModules, moduleVersionRequired, fuzzyMatcher: fuzzyMatcher))
                if (fuzzyMatcher.IsFuzzyMatch(command.Name, pattern, out int score))
                    yield return new CommandScore(command, score);
        /// Gets a list of matching commands.
        /// <param name="fuzzyMatcher">Fuzzy matcher for fuzzy searching.</param>
        /// <param name="useAbbreviationExpansion">Use abbreviation expansion for matching.</param>
        /// <returns>Returns matching CommandInfo IEnumerable.</returns>
        internal static IEnumerable<CommandInfo> GetMatchingCommands(string pattern, ExecutionContext context, CommandOrigin commandOrigin, bool rediscoverImportedModules = false, bool moduleVersionRequired = false, FuzzyMatcher fuzzyMatcher = null, bool useAbbreviationExpansion = false)
            // Otherwise, if it had wildcards, just return the "AvailableCommand"
            // type of command info.
            WildcardPattern commandPattern = WildcardPattern.Get(pattern, WildcardOptions.IgnoreCase);
            PSModuleAutoLoadingPreference moduleAutoLoadingPreference = CommandDiscovery.GetCommandDiscoveryPreference(context, SpecialVariables.PSModuleAutoLoadingPreferenceVarPath, "PSModuleAutoLoadingPreference");
            if ((moduleAutoLoadingPreference != PSModuleAutoLoadingPreference.None) &&
                ((commandOrigin == CommandOrigin.Internal) || ((cmdletInfo != null) && (cmdletInfo.Visibility == SessionStateEntryVisibility.Public))))
                foreach (string modulePath in GetDefaultAvailableModuleFiles(isForAutoDiscovery: false, context))
                    // Skip modules that have already been loaded so that we don't expose private commands.
                    List<PSModuleInfo> modules = context.Modules.GetExactMatchModules(moduleName, all: false, exactMatch: true);
                    PSModuleInfo tempModuleInfo = null;
                    if (modules.Count != 0)
                        // 1. We continue to the next module path if we don't want to re-discover those imported modules
                        // 2. If we want to re-discover the imported modules, but one or more commands from the module were made private,
                        //    then we don't do re-discovery
                        if (!rediscoverImportedModules || modules.Exists(static module => module.ModuleHasPrivateMembers))
                        if (modules.Count == 1)
                            PSModuleInfo psModule = modules[0];
                            tempModuleInfo = new PSModuleInfo(psModule.Name, psModule.Path, context: null, sessionState: null);
                            tempModuleInfo.SetModuleBase(psModule.ModuleBase);
                            foreach (KeyValuePair<string, CommandInfo> entry in psModule.ExportedCommands)
                                if (commandPattern.IsMatch(entry.Value.Name) ||
                                    (fuzzyMatcher is not null && fuzzyMatcher.IsFuzzyMatch(entry.Value.Name, pattern)) ||
                                    (useAbbreviationExpansion && string.Equals(pattern, AbbreviateName(entry.Value.Name), StringComparison.OrdinalIgnoreCase)))
                                    CommandInfo current = null;
                                    switch (entry.Value.CommandType)
                                            current = new AliasInfo(entry.Value.Name, definition: null, context);
                                            current = new FunctionInfo(entry.Value.Name, ScriptBlock.EmptyScriptBlock, context);
                                            current = new FilterInfo(entry.Value.Name, ScriptBlock.EmptyScriptBlock, context);
                                            current = new ConfigurationInfo(entry.Value.Name, ScriptBlock.EmptyScriptBlock, context);
                                            current = new CmdletInfo(entry.Value.Name, implementingType: null, helpFile: null, PSSnapin: null, context);
                                            Dbg.Assert(false, "cannot be hit");
                                    current.Module = tempModuleInfo;
                                    yield return current;
                    string moduleShortName = Path.GetFileNameWithoutExtension(modulePath);
                    IDictionary<string, CommandTypes> exportedCommands = AnalysisCache.GetExportedCommands(modulePath, testOnly: false, context);
                    tempModuleInfo = new PSModuleInfo(moduleShortName, modulePath, sessionState: null, context: null);
                    if (InitialSessionState.IsEngineModule(moduleShortName))
                        tempModuleInfo.SetModuleBase(Utils.DefaultPowerShellAppBase);
                    // moduleVersionRequired is bypassed by FullyQualifiedModule from calling method. This is the only place where guid will be involved.
                    if (moduleVersionRequired && modulePath.EndsWith(StringLiterals.PowerShellDataFileExtension, StringComparison.OrdinalIgnoreCase))
                        tempModuleInfo.SetVersion(ModuleIntrinsics.GetManifestModuleVersion(modulePath));
                        tempModuleInfo.SetGuid(ModuleIntrinsics.GetManifestGuid(modulePath));
                    foreach (KeyValuePair<string, CommandTypes> pair in exportedCommands)
                        CommandTypes commandTypes = pair.Value;
                        if (commandPattern.IsMatch(commandName) ||
                            (fuzzyMatcher is not null && fuzzyMatcher.IsFuzzyMatch(commandName, pattern)) ||
                            (useAbbreviationExpansion && string.Equals(pattern, AbbreviateName(commandName), StringComparison.OrdinalIgnoreCase)))
                            bool shouldExportCommand = true;
                            // Verify that we don't already have it represented in the initial session state.
                            if ((context.InitialSessionState != null) && (commandOrigin == CommandOrigin.Runspace))
                                foreach (SessionStateCommandEntry commandEntry in context.InitialSessionState.Commands[commandName])
                                    string moduleCompareName = null;
                                    if (commandEntry.Module != null)
                                        moduleCompareName = commandEntry.Module.Name;
                                    else if (commandEntry.PSSnapIn != null)
                                        moduleCompareName = commandEntry.PSSnapIn.Name;
                                    if (string.Equals(moduleShortName, moduleCompareName, StringComparison.OrdinalIgnoreCase))
                                        if (commandEntry.Visibility == SessionStateEntryVisibility.Private)
                                            shouldExportCommand = false;
                            if (shouldExportCommand)
                                if ((commandTypes & CommandTypes.Alias) == CommandTypes.Alias)
                                    yield return new AliasInfo(commandName, null, context)
                                        Module = tempModuleInfo
                                if ((commandTypes & CommandTypes.Cmdlet) == CommandTypes.Cmdlet)
                                    yield return new CmdletInfo(commandName, implementingType: null, helpFile: null, PSSnapin: null, context: context)
                                if ((commandTypes & CommandTypes.Function) == CommandTypes.Function)
                                    yield return new FunctionInfo(commandName, ScriptBlock.EmptyScriptBlock, context)
                                if ((commandTypes & CommandTypes.Configuration) == CommandTypes.Configuration)
                                    yield return new ConfigurationInfo(commandName, ScriptBlock.EmptyScriptBlock, context)
        /// Returns abbreviated version of a command name.
        /// <param name="commandName">Name of the command to transform.</param>
        /// <returns>Abbreviated version of the command name.</returns>
        internal static string AbbreviateName(string commandName)
            // Use default size of 6 which represents expected average abbreviation length
            StringBuilder abbreviation = new StringBuilder(6);
                if (char.IsUpper(c) || c == '-')
                    abbreviation.Append(c);
            return abbreviation.ToString();
    internal struct CommandScore
        public CommandScore(CommandInfo command, int score)
            Command = command;
            Score = score;
        public CommandInfo Command;
        public int Score;

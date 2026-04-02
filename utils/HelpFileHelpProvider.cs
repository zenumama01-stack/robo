    /// Class HelpFileHelpProvider implement the help provider for help.txt kinds of
    /// help contents.
    /// Help File help information are stored in '.help.txt' files. These files are
    /// located in the Monad / CustomShell Path as well as in the Application Base
    /// of PSSnapIns.
    internal class HelpFileHelpProvider : HelpProviderWithCache
        internal HelpFileHelpProvider(HelpSystem helpSystem) : base(helpSystem)
        /// Name of the provider.
        /// <value>Name of the provider</value>
                return "HelpFile Help Provider";
        /// Help category of the provider.
        /// <value>Help category of the provider</value>
            string helpFileName = helpRequest.Target + ".help.txt";
            Collection<string> filesMatched = MUIFileSearcher.SearchFiles(helpFileName, GetExtendedSearchPaths());
            Diagnostics.Assert(filesMatched != null, "Files collection should not be null.");
            var matchedFilesToRemove = FilterToLatestModuleVersion(filesMatched);
            foreach (string file in filesMatched)
                if (matchedFilesToRemove.Contains(file))
                // Check whether the file is already loaded
                if (!_helpFiles.ContainsKey(file))
                        LoadHelpFile(file);
                        ReportHelpFileError(ioException, helpRequest.Target, file);
                        ReportHelpFileError(securityException, helpRequest.Target, file);
                HelpInfo helpInfo = GetCache(file);
        private Collection<string> FilterToLatestModuleVersion(Collection<string> filesMatched)
            Collection<string> matchedFilesToRemove = new Collection<string>();
            if (filesMatched.Count > 1)
                // Dictionary<<ModuleName,fileName>, <Version, helpFileFullName>>
                Dictionary<Tuple<string, string>, Tuple<string, Version>> modulesAndVersion = new Dictionary<Tuple<string, string>, Tuple<string, Version>>();
                HashSet<string> filesProcessed = new HashSet<string>();
                var allPSModulePaths = ModuleIntrinsics.GetModulePath(false, this.HelpSystem.ExecutionContext);
                foreach (string fileFullName in filesMatched)
                    // Use the filename as a check if we need to process further.
                    // Single module can have multiple .help.txt files.
                    var fileName = Path.GetFileName(fileFullName);
                    foreach (string psModulePath in allPSModulePaths)
                        Version moduleVersionFromPath = null;
                        GetModuleNameAndVersion(psModulePath, fileFullName, out moduleName, out moduleVersionFromPath);
                        // Skip modules whose root we cannot determine or which do not have versions.
                        if (moduleVersionFromPath != null && moduleName != null)
                            Tuple<string, Version> moduleVersion = null;
                            Tuple<string, string> key = new Tuple<string, string>(moduleName, fileName);
                            if (modulesAndVersion.TryGetValue(key, out moduleVersion))
                                // Consider for further processing only if the help file name is same.
                                if (filesProcessed.Contains(fileName))
                                    if (moduleVersionFromPath > moduleVersion.Item2)
                                        modulesAndVersion[key] = new Tuple<string, Version>(fileFullName, moduleVersionFromPath);
                                        // Remove the old file since we found a newer version.
                                        matchedFilesToRemove.Add(moduleVersion.Item1);
                                        // Remove the new file as higher version item is already in dictionary.
                                        matchedFilesToRemove.Add(fileFullName);
                                // Add the module to the dictionary as it was not processes earlier.
                                modulesAndVersion.Add(new Tuple<string, string>(moduleName, fileName),
                                                      new Tuple<string, Version>(fileFullName, moduleVersionFromPath));
                    filesProcessed.Add(fileName);
            // Deduplicate by filename to compensate for two sources, currentuser scope and allusers scope.
            // This is done after the version check filtering to ensure we do not remove later version files.
            HashSet<string> fileNameHash = new HashSet<string>();
            foreach (var file in filesMatched)
                string fileName = Path.GetFileName(file);
                if (!fileNameHash.Add(fileName))
                    // If the file need to be removed, add it to matchedFilesToRemove, if not already present.
                    if (!matchedFilesToRemove.Contains(file))
                        matchedFilesToRemove.Add(file);
            return matchedFilesToRemove;
            if ((!searchOnlyContent) && (!WildcardPattern.ContainsWildcardCharacters(target)))
                // Search all the about conceptual topics. This pattern
                // makes about topics discoverable without actually
                // using the word "about_" as in "get-help while".
                pattern = "*" + pattern + "*";
            if (searchOnlyContent)
                if (!WildcardPattern.ContainsWildcardCharacters(helpRequest.Target))
                    searchTarget = "*" + searchTarget + "*";
                // search all about_* topics
                pattern = "*";
            pattern += ".help.txt";
            Collection<string> files = MUIFileSearcher.SearchFiles(pattern, GetExtendedSearchPaths());
            var matchedFilesToRemove = FilterToLatestModuleVersion(files);
            if (files == null)
                HelpFileHelpInfo helpInfo = GetCache(file) as HelpFileHelpInfo;
                        if (!helpInfo.MatchPatternInContent(wildCardPattern))
        private static void GetModuleNameAndVersion(string psmodulePathRoot, string filePath, out string moduleName, out Version moduleVersion)
            if (filePath.StartsWith(psmodulePathRoot, StringComparison.OrdinalIgnoreCase))
                var moduleRootSubPath = filePath.Remove(0, psmodulePathRoot.Length);
                var pathParts = moduleRootSubPath.Split(Utils.Separators.Directory, StringSplitOptions.RemoveEmptyEntries);
                moduleName = pathParts[0];
                var potentialVersion = pathParts[1];
                Version result;
                if (Version.TryParse(potentialVersion, out result))
                    moduleVersion = result;
        /// Load help file based on the file path.
        /// <param name="path">File path to load help from.</param>
        /// <returns>Help info object loaded from the file.</returns>
        private HelpInfo LoadHelpFile(string path)
            // Bug906435: Get-help for special devices throws an exception
            // There might be situations where path does not end with .help.txt extension
            // The assumption that path ends with .help.txt is broken under special
            // conditions when user uses "get-help" with device names like "prn","com1" etc.
            // First check whether path ends with .help.txt.
            // If path does not end with ".help.txt" return.
            if (!path.EndsWith(".help.txt", StringComparison.OrdinalIgnoreCase))
            string name = fileName.Substring(0, fileName.Length - 9 /* ".help.txt".Length */);
            HelpInfo helpInfo = GetCache(path);
            string helpText = null;
            using (TextReader tr = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
                helpText = tr.ReadToEnd();
            _helpFiles[path] = 0;
            helpInfo = HelpFileHelpInfo.GetHelpInfo(name, helpText, path);
            AddCache(path, helpInfo);
        /// Gets the extended search paths for about_topics help. To be able to get about_topics help from unloaded modules,
        /// we will add $pshome and the folders under PS module paths to the collection of paths to search.
        /// <returns>A collection of string representing locations.</returns>
        internal Collection<string> GetExtendedSearchPaths()
            Collection<string> searchPaths = GetSearchPaths();
            // Add $pshome at the top of the list
            string defaultShellSearchPath = GetDefaultShellSearchPath();
            int index = searchPaths.IndexOf(defaultShellSearchPath);
            if (index != 0)
                    searchPaths.RemoveAt(index);
                searchPaths.Insert(0, defaultShellSearchPath);
            // Add the CurrentUser help path.
            // Add modules that are not loaded. Since using 'get-module -listavailable' is very expensive,
            // we load all the directories (which are not empty) under the module path.
            foreach (string psModulePath in ModuleIntrinsics.GetModulePath(false, this.HelpSystem.ExecutionContext))
                if (Directory.Exists(psModulePath))
                        // Get all the directories under the module path
                        // * and SearchOption.AllDirectories gets all the version directories.
                        IEnumerable<string> directories = Directory.EnumerateDirectories(psModulePath, "*", SearchOption.AllDirectories);
                        var possibleModuleDirectories = directories.Where(static directory => !ModuleUtils.IsPossibleResourceDirectory(directory));
                        foreach (string directory in possibleModuleDirectories)
                            // Add only directories that are not empty
                            if (Directory.EnumerateFiles(directory).Any())
                                if (!searchPaths.Contains(directory))
                                    searchPaths.Add(directory);
                    // Absorb any exception related to enumerating directories
                    catch (System.ArgumentException) { }
                    catch (System.UnauthorizedAccessException) { }
            return searchPaths;

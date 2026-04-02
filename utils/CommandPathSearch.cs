    /// Used to enumerate the commands on the system that match the specified
    /// command name.
    internal class CommandPathSearch : IEnumerable<string>, IEnumerator<string>
        [TraceSource("CommandSearch", "CommandSearch")]
        private static readonly PSTraceSource s_tracer = PSTraceSource.GetTracer("CommandSearch", "CommandSearch");
        /// Constructs a command searching enumerator that resolves the location
        /// of a command using the PATH environment variable.
        /// The command name to search for in the path.
        /// <param name="lookupPaths">
        /// The paths to directories in which to lookup the command.
        /// Ex.null: paths from PATH environment variable.
        /// The execution context for the current engine instance.
        /// <param name="acceptableCommandNames">
        /// The patterns to search for in the paths.
        /// <param name="fuzzyMatcher">
        /// The fuzzy matcher to use for fuzzy searching.
        internal CommandPathSearch(
            LookupPathCollection lookupPaths,
            Collection<string>? acceptableCommandNames,
            FuzzyMatcher? fuzzyMatcher)
            _fuzzyMatcher = fuzzyMatcher;
            string[] commandPatterns;
            if (acceptableCommandNames != null)
                // The name passed in is not a pattern. To minimize enumerating the file system, we
                // turn the command name into a pattern and then match against extensions in PATHEXT.
                // The old code would enumerate the file system many more times, once per possible extension.
                    commandPatterns = new[] { commandName + ".*" };
                    // Porting note: on non-Windows platforms, we want to always allow just 'commandName'
                    // as an acceptable command name. However, we also want to allow commands to be
                    // called with the .ps1 extension, so that 'script.ps1' can be called by 'script'.
                    commandPatterns = new[] { commandName, commandName + ".ps1" };
                _postProcessEnumeratedFiles = CheckAgainstAcceptableCommandNames;
                _acceptableCommandNames = acceptableCommandNames;
                commandPatterns = new[] { commandName };
                _postProcessEnumeratedFiles = JustCheckExtensions;
            // Note, discovery must be set before resolving the current directory
            _patterns = commandPatterns;
            _lookupPaths = lookupPaths;
            ResolveCurrentDirectoryInLookupPaths();
            _orderedPathExt = CommandDiscovery.PathExtensionsWithPs1Prepended;
            // The same as in this.Reset()
            _lookupPathsEnumerator = _lookupPaths.GetEnumerator();
            _patternEnumerator = _patterns.GetEnumerator();
            _currentDirectoryResults = Array.Empty<string>();
            _currentDirectoryResultsEnumerator = _currentDirectoryResults.GetEnumerator();
            _justReset = true;
        /// Ensures that all the paths in the lookupPaths member are absolute
        /// file system paths.
        private void ResolveCurrentDirectoryInLookupPaths()
            var indexesToRemove = new SortedDictionary<int, int>();
            int removalListCount = 0;
            string fileSystemProviderName = _context.ProviderNames.FileSystem;
            SessionStateInternal sessionState = _context.EngineSessionState;
            // Only use the directory if it gets resolved by the FileSystemProvider
            bool isCurrentDriveValid =
                sessionState.CurrentDrive != null &&
                sessionState.CurrentDrive.Provider.NameEquals(fileSystemProviderName) &&
                sessionState.IsProviderLoaded(fileSystemProviderName);
            string? environmentCurrentDirectory = null;
                environmentCurrentDirectory = Directory.GetCurrentDirectory();
                // This can happen if the current working directory is deleted by another process on non-Windows
                // In this case, we'll just ignore it and continue on with the current directory as null
            LocationGlobber pathResolver = _context.LocationGlobber;
            // Loop through the relative paths and resolve them
            foreach (int index in _lookupPaths.IndexOfRelativePath())
                string? resolvedDirectory = null;
                string? resolvedPath = null;
                CommandDiscovery.discoveryTracer.WriteLine(
                    "Lookup directory \"{0}\" appears to be a relative path. Attempting resolution...",
                    _lookupPaths[index]);
                if (isCurrentDriveValid)
                        resolvedPath =
                            pathResolver.GetProviderPath(
                                _lookupPaths[index],
                    catch (ProviderInvocationException providerInvocationException)
                            "The relative path '{0}', could not be resolved because the provider threw an exception: '{1}'",
                            providerInvocationException.Message);
                            "The relative path '{0}', could not resolve a home directory for the provider",
                    // Note, if the directory resolves to multiple paths, only the first is used.
                    if (!string.IsNullOrEmpty(resolvedPath))
                        CommandDiscovery.discoveryTracer.TraceError(
                            "The relative path resolved to: {0}",
                        resolvedDirectory = resolvedPath;
                            "The relative path was not a file system path. {0}",
                    CommandDiscovery.discoveryTracer.TraceWarning(
                        "The current drive is not set, using the process current directory: {0}",
                        environmentCurrentDirectory);
                    resolvedDirectory = environmentCurrentDirectory;
                // If we successfully resolved the path, make sure it is unique. Remove
                // any duplicates found after the first occurrence of the path.
                if (resolvedDirectory != null)
                    int existingIndex = _lookupPaths.IndexOf(resolvedDirectory);
                    if (existingIndex != -1)
                        if (existingIndex > index)
                            // The relative path index is less than the explicit path,
                            // so remove the explicit path.
                            indexesToRemove.Add(removalListCount++, existingIndex);
                            _lookupPaths[index] = resolvedDirectory;
                            // The explicit path index is less than the relative path
                            // index, so remove the relative path.
                            indexesToRemove.Add(removalListCount++, index);
                        // Change the relative path to the resolved path.
                    // The directory couldn't be resolved so remove it from the
                    // lookup paths.
            // Now remove all the duplicates starting from the back of the collection.
            // As each element is removed, elements that follow are moved up to occupy
            // the emptied index.
            for (int removeIndex = indexesToRemove.Count; removeIndex > 0; --removeIndex)
                int indexToRemove = indexesToRemove[removeIndex - 1];
                _lookupPaths.RemoveAt(indexToRemove);
        /// Gets an instance of a command enumerator.
        /// An instance of this class as IEnumerator.
        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        /// Moves the enumerator to the next command match.
        /// true if there was another command that matches, false otherwise.
        public bool MoveNext()
            if (_justReset)
                _justReset = false;
                if (!_patternEnumerator.MoveNext())
                    s_tracer.TraceError("No patterns were specified");
                if (!_lookupPathsEnumerator.MoveNext())
                    s_tracer.TraceError("No lookup paths were specified");
                GetNewDirectoryResults(_patternEnumerator.Current, _lookupPathsEnumerator.Current);
            while (true) // while lookupPathsEnumerator is valid
                while (true) // while patternEnumerator is valid
                    // Try moving to the next path in the current results
                    if (!_currentDirectoryResultsEnumerator.MoveNext())
                        s_tracer.WriteLine("Current directory results are invalid");
                        // Since a path was not found in the current result,
                        // advance the pattern and try again
                            s_tracer.WriteLine("Current patterns exhausted in current directory: {0}", _lookupPathsEnumerator.Current);
                        // Get the results of the next pattern
                        s_tracer.WriteLine("Next path found: {0}", _currentDirectoryResultsEnumerator.Current);
                    // Since we have reset the results, loop again to find the next result.
                // Since the path was not found in the current results, and all patterns were exhausted,
                // advance the path and continue
                    s_tracer.WriteLine("All lookup paths exhausted, no more matches can be found");
                // Reset the pattern enumerator and get new results using the new lookup path
                    s_tracer.WriteLine("All patterns exhausted, no more matches can be found");
        /// Resets the enumerator to before the first command match.
        public void Reset()
            _lookupPathsEnumerator.Dispose();
            _patternEnumerator.Dispose();
            _currentDirectoryResultsEnumerator.Dispose();
        /// Gets the path to the current command match.
        /// The enumerator is positioned before the first element of
        /// the collection or after the last element.
        string IEnumerator<string>.Current
                if (_currentDirectoryResults == null)
                return _currentDirectoryResultsEnumerator.Current;
        object IEnumerator.Current
                return ((IEnumerator<string>)this).Current;
        /// Required by the IEnumerator generic interface.
        /// Resets the searcher.
        /// Gets the matching files in the specified directories and resets
        /// the currentDirectoryResultsEnumerator to this new set of results.
        /// The pattern used to find the matching files in the specified directory.
        /// <param name="directory">
        /// The path to the directory to find the files in.
        private void GetNewDirectoryResults(string pattern, string directory)
            IEnumerable<string>? result = null;
                CommandDiscovery.discoveryTracer.WriteLine("Looking for {0} in {1}", pattern, directory);
                // Get the matching files in the directory
                if (Directory.Exists(directory))
                    // Win8 bug 92113: Directory.GetFiles() regressed in NET4
                    // Directory.GetFiles(directory, ".") used to return null with CLR 2.
                    // but with CLR4 it started acting like "*". This is a appcompat bug in CLR4
                    // but they cannot fix it as CLR4 is already RTMd by the time this was reported.
                    // If they revert it, it will become a CLR4 appcompat issue. So, using the workaround
                    // to forcefully use null if pattern is "."
                    if (pattern.Length != 1 || pattern[0] != '.')
                        if (_fuzzyMatcher is not null)
                            var files = new List<string>();
                            var matchingFiles = Directory.EnumerateFiles(directory);
                            foreach (string file in matchingFiles)
                                if (_fuzzyMatcher.IsFuzzyMatch(Path.GetFileName(file), pattern))
                            result = _postProcessEnumeratedFiles != null
                                ? _postProcessEnumeratedFiles(files.ToArray())
                                : files;
                            var matchingFiles = Directory.EnumerateFiles(directory, pattern);
                                ? _postProcessEnumeratedFiles(matchingFiles.ToArray())
                                : matchingFiles;
                // The pattern contained illegal file system characters
                // A directory specified in the lookup path was not
                // accessible
            _currentDirectoryResults = result ?? Array.Empty<string>();
        private IEnumerable<string>? CheckAgainstAcceptableCommandNames(string[] fileNames)
            var baseNames = fileNames.Select(Path.GetFileName).ToArray();
            // Result must be ordered by PATHEXT order of precedence.
            // acceptableCommandNames is in this order, so
            // Porting note: allow files with executable bit on non-Windows platforms
            Collection<string>? result = null;
            if (baseNames.Length > 0 && _acceptableCommandNames != null)
                foreach (var name in _acceptableCommandNames)
                    for (int i = 0; i < baseNames.Length; i++)
                        if (name.Equals(baseNames[i], StringComparison.OrdinalIgnoreCase)
                            || (!Platform.IsWindows && Platform.NonWindowsIsExecutable(name)))
                            result ??= new Collection<string>();
                            result.Add(fileNames[i]);
        private IEnumerable<string>? JustCheckExtensions(string[] fileNames)
            // Warning: pretty duplicated code
            foreach (var allowedExt in _orderedPathExt)
                foreach (var fileName in fileNames)
                    if (fileName.EndsWith(allowedExt, StringComparison.OrdinalIgnoreCase)
                        || (!Platform.IsWindows && Platform.NonWindowsIsExecutable(fileName)))
                        result.Add(fileName);
        /// The directory paths in which to look for commands.
        /// This is derived from the PATH environment variable.
        private readonly LookupPathCollection _lookupPaths;
        /// The enumerator for the lookup paths.
        private IEnumerator<string> _lookupPathsEnumerator;
        /// The list of results matching the pattern in the current
        /// path lookup directory. Resets to null.
        private IEnumerable<string> _currentDirectoryResults;
        /// The enumerator for the list of results.
        private IEnumerator<string> _currentDirectoryResultsEnumerator;
        /// The command name to search for.
        private readonly IEnumerable<string> _patterns;
        /// The enumerator for the patterns.
        private IEnumerator<string> _patternEnumerator;
        /// A reference to the execution context for this runspace.
        /// When reset is called, this gets set to true. Once MoveNext
        /// is called, this gets set to false.
        private bool _justReset;
        /// If not null, called with the enumerated files for further processing.
        private readonly Func<string[], IEnumerable<string>?> _postProcessEnumeratedFiles;
        private readonly string[] _orderedPathExt;
        private readonly Collection<string>? _acceptableCommandNames;
        private readonly FuzzyMatcher? _fuzzyMatcher;

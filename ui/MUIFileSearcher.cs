    internal sealed class MUIFileSearcher
        /// Constructor. It is private so that MUIFileSearcher is used only internal for this class.
        /// To access functionality in this class, static api should be used.
        /// <param name="searchPaths"></param>
        /// <param name="searchMode"></param>
        private MUIFileSearcher(string target, Collection<string> searchPaths, SearchMode searchMode)
            SearchPaths = searchPaths;
            SearchMode = searchMode;
        /// A constructor to make searchMode optional.
        private MUIFileSearcher(string target, Collection<string> searchPaths)
            : this(target, searchPaths, SearchMode.Unique)
        #region Basic Properties
        /// Search target. It can be
        ///     1. a file name
        ///     2. a search pattern
        /// It can also include a path, in that case,
        ///     1. the path will be searched first for the existence of the files.
        internal string Target { get; } = null;
        /// Search path as provided by user.
        internal Collection<string> SearchPaths { get; } = null;
        /// Search mode for this file search.
        internal SearchMode SearchMode { get; } = SearchMode.Unique;
        private static readonly System.IO.EnumerationOptions _enumerationOptions = new()
            IgnoreInaccessible = false,
            AttributesToSkip = 0,
            MatchType = MatchType.Win32,
        private Collection<string> _result = null;
        /// Result of the search.
        internal Collection<string> Result
                if (_result == null)
                    _result = new Collection<string>();
                    // SearchForFiles will fill the result collection.
                    SearchForFiles();
                return _result;
        #region File Search
        /// _uniqueMatches is used to track matches already found during the search process.
        /// This is useful for ignoring duplicates in the case of unique search.
        private readonly Hashtable _uniqueMatches = new Hashtable(StringComparer.OrdinalIgnoreCase);
        /// Search for files using the target, searchPaths member of this class.
        private void SearchForFiles()
            if (string.IsNullOrEmpty(this.Target))
            string pattern = Path.GetFileName(this.Target);
            Collection<string> normalizedSearchPaths = NormalizeSearchPaths(this.Target, this.SearchPaths);
            foreach (string directory in normalizedSearchPaths)
                SearchForFiles(pattern, directory);
                if (this.SearchMode == SearchMode.First && this.Result.Count > 0)
        private void AddFiles(string muiDirectory, string directory, string pattern)
                foreach (string file in Directory.EnumerateFiles(muiDirectory, pattern, _enumerationOptions))
                    string path = Path.Combine(muiDirectory, file);
                    switch (this.SearchMode)
                        case SearchMode.All:
                            _result.Add(path);
                        case SearchMode.Unique:
                            // Construct a Unique filename for this directory.
                            // Remember the file may belong to one of the sub-culture
                            // directories. In this case we should not be returning
                            // same files that are residing in 2 or more sub-culture
                            // directories.
                            string leafFileName = Path.GetFileName(file);
                            string uniqueToDirectory = Path.Combine(directory, leafFileName);
                            if (!_result.Contains(path) && !_uniqueMatches.Contains(uniqueToDirectory))
                                _uniqueMatches[uniqueToDirectory] = true;
                        case SearchMode.First:
        /// Search for files of a particular pattern under a particular directory.
        /// This will do MUI search in which appropriate language directories are
        /// searched in order.
        private void SearchForFiles(string pattern, string directory)
            List<string> cultureNameList = new List<string>();
            while (culture != null && !string.IsNullOrEmpty(culture.Name))
                cultureNameList.Add(culture.Name);
            cultureNameList.Add(string.Empty);
            // Add en-US and en as fallback languages
            if (!cultureNameList.Contains("en-US"))
                cultureNameList.Add("en-US");
            if (!cultureNameList.Contains("en"))
                cultureNameList.Add("en");
            foreach (string name in cultureNameList)
                string muiDirectory = Path.Combine(directory, name);
                AddFiles(muiDirectory, directory, pattern);
        /// A help file is located in 3 steps
        ///     1. If file itself contains a path itself, try to locate the file
        ///        from path. LocateFile will fail if this file doesn't exist.
        ///     2. Try to locate the file from searchPaths. Normally the searchPaths will
        ///        contain the cmdlet/provider assembly directory if currently we are searching
        ///        help for cmdlet and providers.
        ///     3. Try to locate the file in the default PowerShell installation directory.
        private static Collection<string> NormalizeSearchPaths(string target, Collection<string> searchPaths)
            // step 1: if target has path attached, directly locate
            //         file from there.
            if (!string.IsNullOrEmpty(target) && !string.IsNullOrEmpty(Path.GetDirectoryName(target)))
                string directory = Path.GetDirectoryName(target);
                    result.Add(Path.GetFullPath(directory));
                // user specifically wanted to search in a particular directory
                // so return..
            // step 2: add directories specified in to search path.
            if (searchPaths != null)
                foreach (string directory in searchPaths)
                    if (!result.Contains(directory) && Directory.Exists(directory))
                        result.Add(directory);
            // step 3: locate the file in the default PowerShell installation directory.
            string defaultPSPath = Utils.GetApplicationBase(Utils.DefaultPowerShellShellID);
            if (defaultPSPath != null &&
                !result.Contains(defaultPSPath) &&
                Directory.Exists(defaultPSPath))
                result.Add(defaultPSPath);
        #region Static API's
        /// Search for files in default search paths.
        internal static Collection<string> SearchFiles(string pattern)
            return SearchFiles(pattern, new Collection<string>());
        /// Search for files in specified search paths.
        internal static Collection<string> SearchFiles(string pattern, Collection<string> searchPaths)
            MUIFileSearcher searcher = new MUIFileSearcher(pattern, searchPaths);
            return searcher.Result;
        /// Locate a file in default search paths.
        /// <param name="file"></param>
        internal static string LocateFile(string file)
            return LocateFile(file, new Collection<string>());
        /// Get the file in different search paths corresponding to current culture.
        /// The file name to search is the filename part of path parameter. (Normally path
        /// parameter should contain only the filename part).
        /// <param name="file">This is the path to the file. If it has a path, we need to search under that path first.</param>
        /// <param name="searchPaths">Additional search paths.</param>
        internal static string LocateFile(string file, Collection<string> searchPaths)
            MUIFileSearcher searcher = new MUIFileSearcher(file, searchPaths, SearchMode.First);
            if (searcher.Result == null || searcher.Result.Count == 0)
            return searcher.Result[0];
    /// This enum defines different search mode for the MUIFileSearcher.
    internal enum SearchMode
        // return the first match
        First,
        // return all matches, with duplicates allowed
        // return all matches, with duplicates ignored
        Unique

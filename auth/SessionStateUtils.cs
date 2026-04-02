    /// This class holds the integer constants used in Session State.
    internal static class SessionStateConstants
        /// The default maximum for the number of variables.
        internal const int DefaultVariableCapacity = 4096;
        /// Max # of variables allowed in a scope in Session State.
        internal const int MaxVariablesCapacity = 32768;
        /// Min # of variables allows in a scope in Session State.
        internal const int MinVariablesCapacity = 1024;
        /// The default maximum for the number of aliases.
        internal const int DefaultAliasCapacity = 4096;
        /// Max # of aliases allowed in a scope in Session State.
        internal const int MaxAliasCapacity = 32768;
        /// Min # of aliases allowed in a scope in Session State.
        internal const int MinAliasCapacity = 1024;
        /// The default maximum for the number of functions.
        internal const int DefaultFunctionCapacity = 4096;
        /// Max # of functions allowed in a scope in Session State.
        internal const int MaxFunctionCapacity = 32768;
        /// Min # of functions allowed in a scope in Session State.
        internal const int MinFunctionCapacity = 1024;
        /// The default maximum for the number of drives.
        internal const int DefaultDriveCapacity = 4096;
        /// Max # of drives allowed in a scope in Session State.
        internal const int MaxDriveCapacity = 32768;
        /// Min # of drives allowed in a scope in Session State.
        internal const int MinDriveCapacity = 1024;
        /// The default maximum for the number of errors.
        internal const int DefaultErrorCapacity = 256;
        /// Max # of errors allowed in a scope in Session State.
        internal const int MaxErrorCapacity = 32768;
        /// Min # of errors allowed in a scope in Session State.
        internal const int MinErrorCapacity = 256;
        /// The default capacity for a Dictionary store.
        internal const int DefaultDictionaryCapacity = 100;
        /// Default load factor on a hash table.
        internal const float DefaultHashTableLoadFactor = 0.25F;
    /// This class has static methods that are used in Session State.
    internal static class SessionStateUtilities
        /// Converts the specified array into a collection of the specified type.
        /// <param name="array">
        /// The array to be converted.
        /// A collection of the elements that were in the array.
        internal static Collection<T> ConvertArrayToCollection<T>(T[] array)
                foreach (T element in array)
        /// Compares the elements in the specified collection with value specified. If
        /// the string comparer is specified it is used for the comparison, else the
        /// .Equals method is used.
        /// The collection to check for the value.
        /// The value to check for.
        /// <param name="comparer">
        /// If specified the comparer will be used instead of .Equals.
        /// true if the value is contained in the collection or false otherwise.
        /// If <paramref name="collection"/> is null.
        internal static bool CollectionContainsValue(IEnumerable collection, object value, IComparer comparer)
            ArgumentNullException.ThrowIfNull(collection);
            foreach (object item in collection)
                if (comparer != null)
                    if (comparer.Compare(item, value) == 0)
                    if (item.Equals(value))
        /// Constructs a collection of WildcardPatterns for the specified
        /// string collection.
        /// <param name="globPatterns">
        /// The string patterns to construct the WildcardPatterns for.
        /// The options to create the WildcardPatterns with.
        /// A collection of WildcardPatterns that represent the string patterns
        /// that were passed.
        internal static Collection<WildcardPattern> CreateWildcardsFromStrings(
            IEnumerable<string> globPatterns,
            WildcardOptions options)
            Collection<WildcardPattern> result = new Collection<WildcardPattern>();
            if (globPatterns != null)
                // Loop through the patterns and construct a wildcard pattern for each one
                foreach (string pattern in globPatterns)
                    if (!string.IsNullOrEmpty(pattern))
                                options));
        /// Determines if the specified text matches any of the patterns.
        /// The text to check against the wildcard pattern.
        /// <param name="patterns">
        /// An array of wildcard patterns. If the array is empty or null the text is deemed
        /// to be a match.
        /// <param name="defaultValue">
        /// The default value that should be returned if <paramref name="patterns"/>
        /// is empty or null.
        /// True if the text matches any of the patterns OR if patterns is null or empty and defaultValue is True.
        internal static bool MatchesAnyWildcardPattern(
            string text,
            IEnumerable<WildcardPattern> patterns,
            bool defaultValue)
            bool patternsNonEmpty = false;
            if (patterns != null)
                // Loop through each of the patterns until a match is found
                foreach (WildcardPattern pattern in patterns)
                    patternsNonEmpty = true;
                    if (pattern.IsMatch(text))
            if (!patternsNonEmpty)
                // Since no pattern was specified return the default value
                result = defaultValue;
        /// Converts an OpenMode enum value to a FileMode.
        /// <param name="openMode">
        /// The OpenMode value to be converted.
        /// The FileMode representation of the OpenMode.
        internal static FileMode GetFileModeFromOpenMode(OpenMode openMode)
            FileMode result = FileMode.Create;
            switch (openMode)
                case OpenMode.Add:
                    result = FileMode.Append;
                case OpenMode.New:
                    result = FileMode.CreateNew;
                case OpenMode.Overwrite:
                    result = FileMode.Create;
    /// The enum used by commands to allow the user to specify how
    /// a file (or other item) should be opened.
    public enum OpenMode
        /// This opens the file for appending (similar to FileMode.Append)
        Add,
        /// The file must be created new. If the file exists it is an error (similar to FileMode.CreateNew)
        New,
        /// Creates a new file, if the file already exists it is overwritten (similar to FileMode.Create)
        Overwrite

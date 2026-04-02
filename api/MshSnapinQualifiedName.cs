    /// A class representing a name that is qualified by the PSSnapin name.
    internal sealed class PSSnapinQualifiedName
        private PSSnapinQualifiedName(string[] splitName)
            Dbg.Assert(splitName != null, "splitName should not be null");
            Dbg.Assert(splitName.Length == 1 || splitName.Length == 2, "splitName should contain 1 or 2 elements");
            if (splitName.Length == 1)
                _shortName = splitName[0];
            else if (splitName.Length == 2)
                if (!string.IsNullOrEmpty(splitName[0]))
                    _psSnapinName = splitName[0];
                _shortName = splitName[1];
                // Since the provider name contained multiple slashes it is
                // a bad format.
                throw PSTraceSource.NewArgumentException("name");
            // Now set the full name
            if (!string.IsNullOrEmpty(_psSnapinName))
                _fullName =
                        _psSnapinName,
                        _shortName);
                _fullName = _shortName;
        /// Gets an instance of the Name class.
        /// An instance of the Name class.
        internal static PSSnapinQualifiedName? GetInstance(string? name)
            string[] splitName = name.Split('\\');
            if (splitName.Length == 0 || splitName.Length > 2)
            var result = new PSSnapinQualifiedName(splitName);
            // If the shortname is empty, then return null...
            if (string.IsNullOrEmpty(result.ShortName))
        /// Gets the command's full name.
        private readonly string _fullName;
        /// Gets the command's PSSnapin name.
        internal string? PSSnapInName
                return _psSnapinName;
        private readonly string? _psSnapinName;
        /// Gets the command's short name.
        internal string ShortName
                return _shortName;
        private readonly string _shortName;
        /// The full name.
        /// A string representing the full name.

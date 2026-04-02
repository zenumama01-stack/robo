    internal enum VariablePathFlags
        Local = 0x01,
        Script = 0x02,
        Global = 0x04,
        Private = 0x08,
        Variable = 0x10,
        Function = 0x20,
        DriveQualified = 0x40,
        Unqualified = 0x80,
        // If any of these bits are set, the path does not represent an unscoped variable.
        UnscopedVariableMask = Local | Script | Global | Private | Function | DriveQualified,
    /// A variable path that you can query the scope and drive of the variable reference.
    public class VariablePath
        /// Stores the path that was passed to the constructor.
        private string _userPath;
        /// The name of the variable without any scope or drive.
        private string _unqualifiedPath;
        /// Store flags about the path, such as private/global/local/etc.
        private VariablePathFlags _flags = VariablePathFlags.None;
        /// Private constructor for CloneAndSetLocal().
        private VariablePath()
        /// Constructs a variable path.
        /// <param name="path">The path to parse.</param>
        public VariablePath(string path)
            : this(path, VariablePathFlags.None)
        /// Constructs a scoped item lookup path.
        /// <param name="knownFlags">
        /// These flags for anything known about the path (such as, is it a function) before
        /// being scanned.
        internal VariablePath(string path, VariablePathFlags knownFlags)
            _userPath = path;
            _flags = knownFlags;
            string candidateScope = null;
            string candidateScopeUpper = null;
            VariablePathFlags candidateFlags = VariablePathFlags.Unqualified;
            int currentCharIndex = 0;
            int lastScannedColon = -1;
        scanScope:
            switch (path[0])
                    candidateScope = "lobal";
                    candidateScopeUpper = "LOBAL";
                    candidateFlags = VariablePathFlags.Global;
                    candidateScope = "ocal";
                    candidateScopeUpper = "OCAL";
                    candidateFlags = VariablePathFlags.Local;
                    candidateScope = "rivate";
                    candidateScopeUpper = "RIVATE";
                    candidateFlags = VariablePathFlags.Private;
                    candidateScope = "cript";
                    candidateScopeUpper = "CRIPT";
                    candidateFlags = VariablePathFlags.Script;
                case 'v':
                    if (knownFlags == VariablePathFlags.None)
                        // If we see 'variable:', our namespaceId will be empty, and
                        // we'll also need to scan for the scope again.
                        candidateScope = "ariable";
                        candidateScopeUpper = "ARIABLE";
                        candidateFlags = VariablePathFlags.Variable;
            if (candidateScope != null)
                currentCharIndex += 1; // First character already matched.
                for (j = 0; currentCharIndex < path.Length && j < candidateScope.Length; ++j, ++currentCharIndex)
                    if (path[currentCharIndex] != candidateScope[j] && path[currentCharIndex] != candidateScopeUpper[j])
                if (j == candidateScope.Length &&
                    currentCharIndex < path.Length &&
                    path[currentCharIndex] == ':')
                    if (_flags == VariablePathFlags.None)
                        _flags = VariablePathFlags.Variable;
                    _flags |= candidateFlags;
                    lastScannedColon = currentCharIndex;
                    currentCharIndex += 1;
                    // If saw 'variable:', we need to look for a scope after 'variable:'.
                    if (candidateFlags == VariablePathFlags.Variable)
                        knownFlags = VariablePathFlags.Variable;
                        candidateScope = candidateScopeUpper = null;
                        candidateFlags = VariablePathFlags.None;
                        goto scanScope;
                lastScannedColon = path.IndexOf(':', currentCharIndex);
                // No colon, or a colon as the first character means we have
                // a simple variable, otherwise it's a drive.
                if (lastScannedColon > 0)
                    _flags = VariablePathFlags.DriveQualified;
            if (lastScannedColon == -1)
                _unqualifiedPath = _userPath;
                _unqualifiedPath = _userPath.Substring(lastScannedColon + 1);
                _flags = VariablePathFlags.Unqualified | VariablePathFlags.Variable;
        internal VariablePath CloneAndSetLocal()
            Debug.Assert(IsUnscopedVariable, "Special method to clone, input must be unqualified");
            VariablePath result = new VariablePath();
            result._userPath = _userPath;
            result._unqualifiedPath = _unqualifiedPath;
            result._flags = VariablePathFlags.Local | VariablePathFlags.Variable;
        #region data accessors
        /// Gets the full path including any possibly specified scope and/or drive name.
        public string UserPath { get { return _userPath; } }
        /// Returns true if the path explicitly specifies 'global:'.
        public bool IsGlobal { get { return (_flags & VariablePathFlags.Global) != 0; } }
        /// Returns true if the path explicitly specifies 'local:'.
        public bool IsLocal { get { return (_flags & VariablePathFlags.Local) != 0; } }
        /// Returns true if the path explicitly specifies 'private:'.
        public bool IsPrivate { get { return (_flags & VariablePathFlags.Private) != 0; } }
        /// Returns true if the path explicitly specifies 'script:'.
        public bool IsScript { get { return (_flags & VariablePathFlags.Script) != 0; } }
        /// Returns true if the path specifies no drive or scope qualifiers.
        public bool IsUnqualified { get { return (_flags & VariablePathFlags.Unqualified) != 0; } }
        /// Returns true if the path specifies a variable path with no scope qualifiers.
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Unscoped")]
        public bool IsUnscopedVariable { get { return ((_flags & VariablePathFlags.UnscopedVariableMask) == 0); } }
        /// Returns true if the path defines a variable.
        public bool IsVariable { get { return (_flags & VariablePathFlags.Variable) != 0; } }
        /// Returns true if the path defines a function.
        internal bool IsFunction { get { return (_flags & VariablePathFlags.Function) != 0; } }
        /// Returns true if the path specifies a drive other than the variable drive.
        public bool IsDriveQualified { get { return (_flags & VariablePathFlags.DriveQualified) != 0; } }
        /// The drive name, or null if the path is for a variable.
        /// It may also be null for some functions (specifically if this is a FunctionScopedItemLookupPath.)
        public string DriveName
                if (!IsDriveQualified)
                // The drive name is asked for infrequently.  Lots of VariablePath
                // objects are created, so rather than allocate an extra string that will
                // always be null, just compute the drive name on demand.
                return _userPath.Substring(0, _userPath.IndexOf(':'));
        /// Gets the namespace specific string.
        internal string UnqualifiedPath
            get { return _unqualifiedPath; }
        /// Return the drive qualified name, if any drive specified, otherwise the simple variable name.
        internal string QualifiedName
            get { return IsDriveQualified ? _userPath : _unqualifiedPath; }
        #endregion data accessors
        /// Helpful for debugging.
            return _userPath;
    internal class FunctionLookupPath : VariablePath
        internal FunctionLookupPath(string path)
            : base(path, VariablePathFlags.Function | VariablePathFlags.Unqualified)

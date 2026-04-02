    /// Enumerates the items matching a particular name in the scopes specified using
    /// the appropriate scoping lookup rules.
    /// The type of items that the derived class returns.
    internal abstract class ScopedItemSearcher<T> : IEnumerator<T>, IEnumerable<T>
        /// Constructs a scoped item searcher.
        /// The state of the engine instance to enumerate through the scopes.
        /// <param name="lookupPath">
        /// The parsed name of the item to lookup.
        /// If <paramref name="sessionState"/> or <paramref name="lookupPath"/>
        internal ScopedItemSearcher(
            SessionStateInternal sessionState,
            VariablePath lookupPath)
            if (lookupPath == null)
                throw PSTraceSource.NewArgumentNullException(nameof(lookupPath));
            this.sessionState = sessionState;
            _lookupPath = lookupPath;
            InitializeScopeEnumerator();
        #region IEnumerable/IEnumerator members
        /// Gets the current object as an IEnumerator.
        /// The current object as an IEnumerator.
        System.Collections.Generic.IEnumerator<T> System.Collections.Generic.IEnumerable<T>.GetEnumerator()
        /// Moves the enumerator to the next matching scoped item.
        /// True if another matching scoped item was found, or false otherwise.
            if (!_isInitialized)
            // Enumerate the scopes until a matching scoped item is found
            while (_scopeEnumerable.MoveNext())
                T newCurrentItem;
                if (TryGetNewScopeItem(((IEnumerator<SessionStateScope>)_scopeEnumerable).Current, out newCurrentItem))
                    _currentScope = ((IEnumerator<SessionStateScope>)_scopeEnumerable).Current;
                    _current = newCurrentItem;
                if (_isSingleScopeLookup)
        /// Gets the current scoped item.
        public object Current
            _current = default(T);
            _scopeEnumerable.Dispose();
            _scopeEnumerable = null;
            _isInitialized = false;
        /// Derived classes override this method to return their
        /// particular type of scoped item.
        /// The scope to look the item up in.
        /// The name of the item to retrieve.
        /// <param name="newCurrentItem">
        /// The scope item that the derived class should return.
        /// True if the scope item was found or false otherwise.
        protected abstract bool GetScopeItem(
            SessionStateScope scope,
            VariablePath name,
            out T newCurrentItem);
        #endregion IEnumerable/IEnumerator members
        /// Gets the lookup scope that the Current item was found in.
        internal SessionStateScope CurrentLookupScope
            get { return _currentScope; }
        private SessionStateScope _currentScope;
        /// Gets the scope in which the search begins.
        internal SessionStateScope InitialScope
            get { return _initialScope; }
        private SessionStateScope _initialScope;
        private bool TryGetNewScopeItem(
            SessionStateScope lookupScope,
            out T newCurrentItem)
            bool result = GetScopeItem(
                lookupScope,
                _lookupPath,
                out newCurrentItem);
        private void InitializeScopeEnumerator()
            // Define the lookup scope and if we have to do single
            // level or dynamic lookup based on the lookup variable
            _initialScope = sessionState.CurrentScope;
            if (_lookupPath.IsGlobal)
                _initialScope = sessionState.GlobalScope;
                _isSingleScopeLookup = true;
            else if (_lookupPath.IsLocal ||
                     _lookupPath.IsPrivate)
            else if (_lookupPath.IsScript)
                _initialScope = sessionState.ScriptScope;
            _scopeEnumerable =
                 new SessionStateScopeEnumerator(_initialScope);
        protected SessionStateInternal sessionState;
        private readonly VariablePath _lookupPath;
        private SessionStateScopeEnumerator _scopeEnumerable;
        private bool _isSingleScopeLookup;
        private bool _isInitialized;
    /// The scope searcher for variables.
    internal class VariableScopeItemSearcher : ScopedItemSearcher<PSVariable>
        public VariableScopeItemSearcher(
            VariablePath lookupPath,
            CommandOrigin origin) : base(sessionState, lookupPath)
            _origin = origin;
        private readonly CommandOrigin _origin;
        /// <param name="variable">
        protected override bool GetScopeItem(
            out PSVariable variable)
            Diagnostics.Assert(name is not FunctionLookupPath,
                "name was scanned incorrect if we get here and it is a FunctionLookupPath");
            variable = scope.GetVariable(name.QualifiedName, _origin);
            // If the variable is private and the lookup scope
            // isn't the current scope, claim that the variable
            // doesn't exist so that the lookup continues.
            if (variable == null ||
                (variable.IsPrivate &&
                 scope != sessionState.CurrentScope))
    /// The scope searcher for aliases.
    internal class AliasScopeItemSearcher : ScopedItemSearcher<AliasInfo>
        public AliasScopeItemSearcher(
            VariablePath lookupPath) : base(sessionState, lookupPath)
        /// <param name="alias">
            out AliasInfo alias)
            alias = scope.GetAlias(name.QualifiedName);
            // If the alias is private and the lookup scope
            // isn't the current scope, claim that the alias
            if (alias == null ||
                ((alias.Options & ScopedItemOptions.Private) != 0 &&
    /// The scope searcher for functions.
    internal class FunctionScopeItemSearcher : ScopedItemSearcher<FunctionInfo>
        public FunctionScopeItemSearcher(
        /// <param name="script">
            VariablePath path,
            out FunctionInfo script)
            Diagnostics.Assert(path is FunctionLookupPath,
                "name was scanned incorrect if we get here and it is not a FunctionLookupPath");
            _name = path.IsFunction ? path.UnqualifiedPath : path.QualifiedName;
            script = scope.GetFunction(_name);
            if (script != null)
                bool isPrivate;
                FilterInfo filterInfo = script as FilterInfo;
                if (filterInfo != null)
                    isPrivate = (filterInfo.Options & ScopedItemOptions.Private) != 0;
                    isPrivate = (script.Options & ScopedItemOptions.Private) != 0;
                // If the function is private and the lookup scope
                // isn't the current scope, claim that the function
                if (isPrivate &&
                    scope != sessionState.CurrentScope)
                    // Now check the visibility of the variable...
                    SessionState.ThrowIfNotVisible(_origin, script);
        internal string Name
    /// The scope searcher for drives.
    internal class DriveScopeItemSearcher : ScopedItemSearcher<PSDriveInfo>
        public DriveScopeItemSearcher(
            drive = scope.GetDrive(name.DriveName);

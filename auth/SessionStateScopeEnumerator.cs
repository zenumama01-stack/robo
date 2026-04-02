    internal sealed class SessionStateScopeEnumerator : IEnumerator<SessionStateScope>, IEnumerable<SessionStateScope>
        /// Constructs an enumerator for enumerating through the session state scopes
        /// using the appropriate scoping rules (default to dynamic scoping).
        ///   The starting scope to start the enumeration from.
        internal SessionStateScopeEnumerator(SessionStateScope scope)
            Diagnostics.Assert(scope != null, "Caller to verify scope argument");
            _initialScope = scope;
        /// Uses the proper scoping rules to get the next scope to do the lookup in.
        /// True if the enumerator was advanced to the next scope, or false otherwise.
            // On the first call to MoveNext the enumerator should be before
            // the first scope in the lookup and then advance to the first
            // scope in the lookup
            _currentEnumeratedScope = _currentEnumeratedScope == null ? _initialScope : _currentEnumeratedScope.Parent;
            // If the current scope is the global scope there is nowhere else
            // to do the lookup, so return false.
            return (_currentEnumeratedScope != null);
        /// Sets the enumerator to before the first scope.
            _currentEnumeratedScope = null;
        /// Gets the current lookup scope.
        /// The enumerator is positioned before the first element of the
        /// collection or after the last element.
        SessionStateScope IEnumerator<SessionStateScope>.Current
                if (_currentEnumeratedScope == null)
                return _currentEnumeratedScope;
                return ((IEnumerator<SessionStateScope>)this).Current;
        /// Gets the IEnumerator for this class.
        /// The IEnumerator interface for this class.
        System.Collections.Generic.IEnumerator<SessionStateScope> System.Collections.Generic.IEnumerable<SessionStateScope>.GetEnumerator()
        private readonly SessionStateScope _initialScope;
        private SessionStateScope _currentEnumeratedScope;

        /// The current scope. It is either the global scope or
        /// a nested scope within the global scope. The current
        /// scope is implied or can be accessed using $local in
        /// the shell.
        /// Cmdlet parameter name to return in the error message instead of "scopeID".
        internal const string ScopeParameterName = "Scope";
        /// Given a scope identifier, returns the proper session state scope.
        /// "global", "local", or "private, or a numeric ID of a relative scope
        /// The scope identified by the scope ID or the current scope if the
        /// scope ID is not defined as a special or numeric scope identifier.
        internal SessionStateScope GetScopeByID(string scopeID)
            SessionStateScope result = _currentScope;
                        scopeID,
                        StringLiterals.Global,
                    result = GlobalScope;
                else if (string.Equals(
                            StringLiterals.Local,
                    result = _currentScope;
                            StringLiterals.Private,
                            StringLiterals.Script,
                    // Get the current script scope from the stack.
                    result = _currentScope.ScriptScope;
                    // Since the scope is not any of the special scopes
                    // try parsing it as an ID
                        int scopeNumericID = Int32.Parse(scopeID, System.Globalization.CultureInfo.CurrentCulture);
                        if (scopeNumericID < 0)
                            throw PSTraceSource.NewArgumentOutOfRangeException(ScopeParameterName, scopeID);
                        result = GetScopeByID(scopeNumericID) ?? _currentScope;
                        throw PSTraceSource.NewArgumentException(ScopeParameterName, AutomationExceptions.InvalidScopeIdArgument, ScopeParameterName);
        /// Given a scope ID, walks the scope list to the appropriate scope and returns it.
        /// The numeric indexer to the scope relative to the current scope.
        /// The scope at the index specified.  The index is relative to the current
        /// scope.
        internal SessionStateScope GetScopeByID(int scopeID)
            SessionStateScope processingScope = _currentScope;
            int originalID = scopeID;
            while (scopeID > 0 && processingScope != null)
                processingScope = processingScope.Parent;
                scopeID--;
            if (processingScope == null && scopeID >= 0)
                ArgumentOutOfRangeException outOfRange =
                        ScopeParameterName,
                        originalID,
                        SessionStateStrings.ScopeIDExceedsAvailableScopes,
                        originalID);
                throw outOfRange;
            return processingScope;
        /// The global scope of session state.  Can be accessed
        /// using $global in the shell.
        internal SessionStateScope GlobalScope { get; }
        /// The module scope of a session state. This is only used internally
        /// by the engine. There is no module scope qualifier.
        internal SessionStateScope ModuleScope { get; }
        /// Gets the session state current scope.
        internal SessionStateScope CurrentScope
                return _currentScope;
                    value != null,
                    "A null scope should never be set");
                // This code is ifdef'd for DEBUG because it may pose a significant
                // performance hit and is only really required to validate our internal
                // code. There is no way anyone outside the Monad codebase can cause
                // these error conditions to be hit.
                // Need to make sure the new scope is in the global scope lineage
                SessionStateScope scope = value;
                bool inGlobalScopeLineage = false;
                        inGlobalScopeLineage = true;
                    inGlobalScopeLineage,
                    "The scope specified to be set in CurrentScope is not in the global scope lineage. All scopes must originate from the global scope.");
                _currentScope = value;
        /// Gets the session state current script scope.
        internal SessionStateScope ScriptScope { get { return _currentScope.ScriptScope; } }
        /// Creates a new scope in the scope tree and assigns the parent
        /// and child scopes appropriately.
        /// <param name="isScriptScope">
        /// If true, the new scope is pushed on to the script scope stack and
        /// can be referenced using $script:
        /// A new SessionStateScope which is a child of the current scope.
        internal SessionStateScope NewScope(bool isScriptScope)
                _currentScope != null,
                "The currentScope should always be set.");
            // Create the new child scope.
            SessionStateScope newScope = new SessionStateScope(_currentScope);
            if (isScriptScope)
                newScope.ScriptScope = newScope;
            return newScope;
        /// Removes the current scope from the scope tree and
        /// changes the current scope to the parent scope.
        /// The scope to cleanup and remove.
        /// The global scope cannot be removed.
        internal void RemoveScope(SessionStateScope scope)
                            SessionStateCategory.Scope,
                            "GlobalScopeCannotRemove",
                            SessionStateStrings.GlobalScopeCannotRemove);
            // Give the provider a chance to cleanup the drive data associated
            // with drives in this scope
            foreach (PSDriveInfo drive in scope.Drives)
                // Call CanRemoveDrive to give the provider a chance to cleanup
                // but ignore the return value and exceptions
                    CanRemoveDrive(drive, context);
                catch (Exception) // Catch-all OK, 3rd party callout.
                    // Ignore all exceptions from the provider as we are
                    // going to force the removal anyway
            scope.RemoveAllDrives();
            // If the scope being removed is the current scope,
            // then it must be removed from the tree.
            if (scope == _currentScope && _currentScope.Parent != null)
                _currentScope = _currentScope.Parent;
            scope.Parent = null;

        #region aliases
        /// Add a new alias entry to this session state object...
        /// <param name="scopeID">
        /// A scope identifier that is either one of the "special" scopes like
        /// "global", "script", "local", or "private, or a numeric ID of a relative scope
        /// to the current scope.
        internal void AddSessionStateEntry(SessionStateAliasEntry entry, string scopeID)
            AliasInfo alias = new AliasInfo(entry.Name, entry.Definition, this.ExecutionContext, entry.Options)
                Module = entry.Module,
                Description = entry.Description
            // Create alias in the global scope...
            this.SetAliasItemAtScope(alias, scopeID, true, CommandOrigin.Internal);
        /// Gets an IEnumerable for the alias table.
        internal IDictionary<string, AliasInfo> GetAliasTable()
            // On 7.0 version we have 132 aliases so we set a larger number to reduce re-allocations.
            const int InitialAliasCount = 150;
            Dictionary<string, AliasInfo> result =
                new Dictionary<string, AliasInfo>(InitialAliasCount, StringComparer.OrdinalIgnoreCase);
                new SessionStateScopeEnumerator(_currentScope);
                foreach (AliasInfo entry in scope.AliasTable)
                    if (!result.ContainsKey(entry.Name))
                        // Make sure the alias isn't private or if it is that the current
                        // scope is the same scope the alias was retrieved from.
                        if ((entry.Options & ScopedItemOptions.Private) == 0 ||
                            scope == _currentScope)
                            result.Add(entry.Name, entry);
        /// Gets an IEnumerable for the alias table for a given scope.
        /// If <paramref name="scopeID"/> is less than zero, or not
        internal IDictionary<string, AliasInfo> GetAliasTableAtScope(string scopeID)
                new Dictionary<string, AliasInfo>(StringComparer.OrdinalIgnoreCase);
            SessionStateScope scope = GetScopeByID(scopeID);
        /// List of aliases to export from this session state object...
        internal List<AliasInfo> ExportedAliases { get; } = new List<AliasInfo>();
        /// Gets the value of the specified alias from the alias table.
        /// <param name="aliasName">
        /// The name of the alias value to retrieve.
        /// The origin of the command calling this API.
        /// The AliasInfo representing the alias.
        internal AliasInfo GetAlias(string aliasName, CommandOrigin origin)
            // Use the scope enumerator to find the alias using the
            // appropriate scoping rules
                result = scope.GetAlias(aliasName);
                    SessionState.ThrowIfNotVisible(origin, result);
                    if ((result.Options & ScopedItemOptions.Private) != 0 &&
                        scope != _currentScope)
        internal AliasInfo GetAlias(string aliasName)
            return GetAlias(aliasName, CommandOrigin.Internal);
        internal AliasInfo GetAliasAtScope(string aliasName, string scopeID)
            if (result != null &&
                (result.Options & ScopedItemOptions.Private) != 0 &&
        /// Sets the alias with specified name to the specified value in the current scope.
        /// The name of the alias to set.
        /// The value to set the alias to.
        /// THe origin of the caller of this API
        /// The resulting AliasInfo for the alias that was set.
        /// If <paramref name="aliasName"/> or <paramref name="value"/> is null or empty.
        /// If the alias is read-only or constant.
        internal AliasInfo SetAliasValue(string aliasName, string value, bool force, CommandOrigin origin)
                throw PSTraceSource.NewArgumentException(nameof(aliasName));
            AliasInfo info = _currentScope.SetAliasValue(aliasName, value, this.ExecutionContext, force, origin);
        /// BUGBUG: this overload only exists for the test suites. They should be cleaned up
        /// and this overload removed.
        internal AliasInfo SetAliasValue(string aliasName, string value, bool force)
            return SetAliasValue(aliasName, value, force, CommandOrigin.Internal);
        /// The options to set on the alias.
        /// The origin of the caller of this API
        internal AliasInfo SetAliasValue(
            string aliasName,
            ScopedItemOptions options,
            CommandOrigin origin)
            AliasInfo info = _currentScope.SetAliasValue(aliasName, value, options, this.ExecutionContext, force, origin);
        /// BUGBUG: this api only exists for the test suites. They should be fixed and it should be removed.
            return SetAliasValue(aliasName, value, options, force, CommandOrigin.Internal);
        /// If true, the alias will be set even if there is an existing ReadOnly
        /// alias.
        /// Specifies the origin of the command setting the alias.
        /// If <paramref name="alias"/> is null.
        internal AliasInfo SetAliasItem(AliasInfo alias, bool force, CommandOrigin origin)
            if (alias == null)
                throw PSTraceSource.NewArgumentNullException(nameof(alias));
            AliasInfo info = _currentScope.SetAliasItem(alias, force, origin);
        /// Specifies the command origin of the calling command.
        internal AliasInfo SetAliasItemAtScope(AliasInfo alias, string scopeID, bool force, CommandOrigin origin)
            // If the "private" scope was specified, make sure the options contain
            // the Private flag
            if (string.Equals(scopeID, StringLiterals.Private, StringComparison.OrdinalIgnoreCase))
                alias.Options |= ScopedItemOptions.Private;
            AliasInfo info = scope.SetAliasItem(alias, force, origin);
        internal AliasInfo SetAliasItemAtScope(AliasInfo alias, string scopeID, bool force)
            return SetAliasItemAtScope(alias, scopeID, force, CommandOrigin.Internal);
        /// Removes the specified alias.
        /// The name of the alias to remove.
        /// If true the alias will be removed even if its ReadOnly.
        /// If <paramref name="aliasName"/> is null or empty.
        /// If the alias is constant.
        internal void RemoveAlias(string aliasName, bool force)
            // Use the scope enumerator to find an existing function
                AliasInfo alias =
                    scope.GetAlias(aliasName);
                if (alias != null)
                    if ((alias.Options & ScopedItemOptions.Private) != 0 &&
                        alias = null;
                        scope.RemoveAlias(aliasName, force);
        /// Gets the aliases by command name (used by metadata-driven help)
        internal IEnumerable<string> GetAliasesByCommandName(string command)
                foreach (string alias in scope.GetAliasesByCommandName(command))
                    yield return alias;
        #endregion aliases

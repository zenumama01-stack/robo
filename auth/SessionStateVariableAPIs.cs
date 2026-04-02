        #region variables
        /// Add an new SessionStateVariable entry to this session state object...
        internal void AddSessionStateEntry(SessionStateVariableEntry entry)
            PSVariable v = new PSVariable(entry.Name, entry.Value,
                    entry.Options, entry.Attributes, entry.Description);
            v.Visibility = entry.Visibility;
            this.SetVariableAtScope(v, "global", true, CommandOrigin.Internal);
        /// Get a variable out of session state. This interface supports
        /// the scope specifiers like "global:example"
        /// name of variable to look up
        /// Origin of the command making this request.
        /// The specified variable.
            VariablePath variablePath = new VariablePath(name, VariablePathFlags.Variable | VariablePathFlags.Unqualified);
            PSVariable resultItem = GetVariableItem(variablePath, out scope, origin);
            return GetVariable(name, CommandOrigin.Internal);
        /// the "namespace:name" syntax so you can do things like
        /// "env:PATH" or "global:example"
        /// The value of the specified variable.
        /// If the <paramref name="name"/> refers to a provider that could not be found.
        /// If the <paramref name="name"/> refers to a drive that could not be found.
        /// If the provider that the <paramref name="name"/> refers to does
        internal object GetVariableValue(string name)
            VariablePath variablePath = new VariablePath(name);
            object resultItem = GetVariableValue(variablePath, out context, out scope);
        /// value to return if you can't find Name or it returns null.
        internal object GetVariableValue(string name, object defaultValue)
            object returnObject = GetVariableValue(name) ?? defaultValue;
        /// Looks up the specified variable and returns the context under which
        /// the variable was found as well as the variable itself.
        /// <param name="variablePath">
        /// The VariablePath helper for the variable.
        /// The scope the variable was found in. Null if the variable wasn't found.
        /// Returns the context under which the variable was found. The context will
        /// have the drive data already set. This will be null if the variable was
        /// not found.
        /// The variable if it was found or null if it was not.
        /// The <paramref name="variablePath"/> is first parsed to see if it contains a drive
        /// specifier or special scope.  If a special scope is found ("LOCAL" or "GLOBAL")
        /// then only that scope is searched for the variable. If any other drive specifier
        /// is found the lookup goes in the following order.
        ///     - current scope
        ///     - each consecutive parent scope until the variable is found.
        ///     - global scope
        /// If <paramref name="variablePath"/> is null.
        /// If the <paramref name="variablePath"/> refers to a provider that could not be found.
        /// If the <paramref name="variablePath"/> refers to a drive that could not be found.
        /// If the provider that the <paramref name="variablePath"/> refers to does
        internal object GetVariableValue(
            VariablePath variablePath,
            out CmdletProviderContext context,
            out SessionStateScope scope)
            scope = null;
            if (variablePath.IsVariable)
                PSVariable variable = GetVariableItem(variablePath, out scope);
                    result = variable.Value;
                result = GetVariableValueFromProvider(variablePath, out context, out scope, _currentScope.ScopeOrigin);
#pragma warning disable 0162
        internal object GetVariableValueFromProvider(
            out SessionStateScope scope,
            if (variablePath == null)
                throw PSTraceSource.NewArgumentNullException(nameof(variablePath));
                !variablePath.IsVariable,
                "This method can only be used to retrieve provider content");
            DriveScopeItemSearcher searcher =
                new DriveScopeItemSearcher(
                    variablePath);
                PSDriveInfo drive = ((IEnumerator<PSDriveInfo>)searcher).Current;
                // Create a new CmdletProviderContext and set the drive data
                context = new CmdletProviderContext(this.ExecutionContext, origin);
#if true
                // PSVariable get/set is the get/set of content in the provider
                        GetContentReader(new string[] { variablePath.QualifiedName }, context);
                // If the item is not found we just return null like the normal
                // variable semantics.
                    // First get the provider for the path.
                    ProviderInfo providerInfo = null;
                    _ = this.Globber.GetProviderPath(variablePath.QualifiedName, out providerInfo);
                        "ProviderCannotBeUsedAsVariable",
                        SessionStateStrings.ProviderCannotBeUsedAsVariable,
                        providerInfo,
                        variablePath.QualifiedName,
                        notImplemented,
                if (readers == null || readers.Count == 0)
                    // The drive was found but the path was wrong or something so return null.
                    // We don't want to continue searching if the provider didn't support content
                    // or the path wasn't found.
                if (readers.Count > 1)
                    // Since more than one path was resolved, this is an error.
                    // Before throwing exception. Close the readers to avoid sharing violation.
                    foreach (IContentReader r in readers)
                        r.Close();
                    PSArgumentException argException =
                            SessionStateStrings.VariablePathResolvedToMultiple,
                            variablePath.QualifiedName);
                        "ProviderVariableSyntaxInvalid",
                        SessionStateStrings.ProviderVariableSyntaxInvalid,
                        argException);
                IContentReader reader = readers[0];
                    // Read all the content
                    IList resultList = reader.Read(-1);
                    if (resultList != null)
                        if (resultList.Count == 0)
                        else if (resultList.Count == 1)
                            result = resultList[0];
                            result = resultList;
                catch (Exception e) // Third-party callout, catch-all OK
                        new ProviderInvocationException(
                    reader.Close();
                        GetItem(variablePath.LookupPath.ToString(), context);
                    Collection<PSObject> items = context.GetAccumulatedObjects ();
                    if (items != null &&
                        items.Count > 0)
                        result = items[0];
                        if (!items[0].basObjectIsEmpty)
                            result = items[0].BaseObject;
                            DictionaryEntry entry = (DictionaryEntry)result;
                            result = entry.Value;
                            // Since DictionaryEntry is a value type we have to
                            // try the cast and catch the exception to determine
                            // if it is a DictionaryEntry type.
#pragma warning restore 0162
        /// Origin of the command requesting this variable
        /// then only that scope is searched for the variable.
        internal PSVariable GetVariableItem(
            Dbg.Diagnostics.Assert(variablePath.IsVariable, "Can't get variable w/ non-variable path");
            VariableScopeItemSearcher searcher =
                new VariableScopeItemSearcher(this, variablePath, origin);
            PSVariable result = null;
                result = ((IEnumerator<PSVariable>)searcher).Current;
            return GetVariableItem(variablePath, out scope, CommandOrigin.Internal);
        /// The ID of the scope to lookup the variable in.
        internal PSVariable GetVariableAtScope(string name, string scopeID)
            SessionStateScope lookupScope = null;
            // The lookup scope from above is ignored and the scope is retrieved by
            // ID.
            lookupScope = GetScopeByID(scopeID);
            PSVariable resultItem = null;
                resultItem = lookupScope.GetVariable(variablePath.QualifiedName);
        internal object GetVariableValueAtScope(string name, string scopeID)
                PSDriveInfo drive = lookupScope.GetDrive(variablePath.DriveName);
                        // Any errors should have been written to the error pipeline.
                        foreach (IContentReader closeReader in readers)
                            closeReader.Close();
                                resultItem = null;
                                resultItem = resultList[0];
                                resultItem = resultList;
                        GetItem (variablePath.LookupPath.ToString (), context);
                        Collection<PSObject> results = context.GetAccumulatedObjects ();
                        if (results != null &
                            results.Count > 0)
                            // Only return the first value. If the caller wants globbing
                            // they need to call the GetItem method directly.
                            if (!results[0].basObjectIsEmpty)
                                resultItem = results[0].BaseObject;
                                resultItem = results[0];
            // If we get a PSVariable or DictionaryEntry returned then we have to
            // grab the value from it and return that instead.
            if (resultItem != null)
                PSVariable variable = resultItem as PSVariable;
                    resultItem = variable.Value;
                        DictionaryEntry entry = (DictionaryEntry)resultItem;
                        resultItem = entry.Value;
            var scopeEnumerator = new SessionStateScopeEnumerator(CurrentScope);
            object result = AutomationNull.Value;
            foreach (var scope in scopeEnumerator)
                result = scope.GetAutomaticVariableValue(variable);
                if (result != AutomationNull.Value)
        /// Set a variable in session state. This interface supports
        /// "$env:PATH = 'c:\windows'" or "$global:example = 13"
        /// The name of the item to set.
        /// The new value of the item being set.
        /// The origin of the caller of this API...
        internal void SetVariableValue(string name, object newValue, CommandOrigin origin)
            SetVariable(variablePath, newValue, true, origin);
        /// BUGBUG: this overload exists because a lot of tests in the
        /// testsuite use it. Those tests should eventually be fixed and this overload
        /// should be removed.
        internal void SetVariableValue(string name, object newValue)
            SetVariableValue(name, newValue, CommandOrigin.Internal);
        /// the scope specifiers like "$global:example = 13"
        /// The variable to be set.
        /// If true, the variable is set even if it is ReadOnly.
        /// A PSVariable object if <paramref name="variablePath"/> refers to a variable.
        /// An PSObject if <paramref name="variablePath"/> refers to a provider path.
        /// If <paramref name="variable"/> is null.
        internal object SetVariable(PSVariable variable, bool force, CommandOrigin origin)
            if (variable == null || string.IsNullOrEmpty(variable.Name))
                throw PSTraceSource.NewArgumentException(nameof(variable));
            VariablePath variablePath = new VariablePath(variable.Name, VariablePathFlags.Variable | VariablePathFlags.Unqualified);
            return SetVariable(variablePath, variable, false, force, origin);
        /// Set a variable using a pre-parsed variablePath object instead of a string.
        /// A pre-parsed variable path object for the variable in question.
        /// The value to set.
        internal object SetVariable(
            object newValue,
            bool asValue,
            return SetVariable(variablePath, newValue, asValue, false, origin);
                // Make sure to set the variable in the appropriate scope
                if (variablePath.IsLocal || variablePath.IsUnscopedVariable)
                    scope = _currentScope;
                else if (variablePath.IsScript)
                    scope = _currentScope.ScriptScope;
                else if (variablePath.IsGlobal)
                    scope = GlobalScope;
                else if (variablePath.IsPrivate)
                PSVariable varResult =
                    scope.SetVariable(
                        newValue,
                        asValue,
                // If the name is scoped as private we need to mark the
                // variable as private
                if (variablePath.IsPrivate && varResult != null)
                    varResult.Options |= ScopedItemOptions.Private;
                result = varResult;
                // Use GetVariable to get the correct context for the set operation.
                // NTRAID#Windows OS Bugs-896768-2004/07/06-JeffJon
                // There is probably a more efficient way to do this.
                GetVariableValue(variablePath, out context, out scope);
                            CmdletProviderContext clearContentContext = new CmdletProviderContext(context);
                            // First clear the content if it is supported.
                            ClearContent(new string[] { variablePath.QualifiedName }, clearContentContext);
                            GetContentWriter(
                                new string[] { variablePath.QualifiedName },
                            ClearContent(new string[] { variablePath.QualifiedName }, false, false);
                                new string[] { variablePath.QualifiedName }, false, false);
                if (writers == null || writers.Count == 0)
                if (writers.Count > 1)
                    foreach (IContentWriter w in writers)
                        w.Close();
                IContentWriter writer = writers[0];
                IList content = newValue as IList ?? new object[] { newValue };
                    writer.Write(content);
                    writer.Close();
                        SetItem (variablePath.LookupPath.ToString (), newValue, context);
                        Collection<PSObject> setItemResult =
                            SetItem (variablePath.LookupPath.ToString (), newValue);
                        if (setItemResult != null &&
                            setItemResult.Count > 0)
                            result = setItemResult[0];
        /// The variable to set
        /// The ID of the scope to do the lookup in. The ID is either a zero based index
        /// of the scope tree with the current scope being zero, its parent scope
        /// being 1 and so on, or "global", "local", "private", or "script"
        /// If <paramref name="variable"/> is null or its name is null or empty.
        /// A PSVariable object if <paramref name="variable"/> refers to a variable.
        /// An PSObject if <paramref name="variable"/> refers to a provider path.
        internal object SetVariableAtScope(PSVariable variable, string scopeID, bool force, CommandOrigin origin)
            SessionStateScope lookupScope = GetScopeByID(scopeID);
                lookupScope.SetVariable(
                    variable.Name,
                    variable,
        #region NewVariable
        /// Creates a new variable.
        /// The variable to create
        /// If true, the variable is created even if it is ReadOnly.
        /// A PSVariable representing the variable that was created.
        internal object NewVariable(PSVariable variable, bool force)
                this.CurrentScope.NewVariable(
        /// Creates a new variable in the specified scope.
        internal object NewVariableAtScope(PSVariable variable, string scopeID, bool force)
                lookupScope.NewVariable(
        #endregion NewVariable
        internal void RemoveVariable(string name)
            RemoveVariable(name, false);
                if (GetVariableItem(variablePath, out scope) != null)
                    scope.RemoveVariable(variablePath.QualifiedName, force);
                RemoveItem(new string[] { variablePath.QualifiedName }, false, context);
        /// The variable to remove.
        internal void RemoveVariable(PSVariable variable)
            RemoveVariable(variable, false);
        internal void RemoveVariable(PSVariable variable, bool force)
            VariablePath variablePath = new VariablePath(variable.Name);
        /// Remove a variable from session state. This interface supports
        /// name of variable to remove
        /// If <paramref name="name"/> refers to an MSH path (not a variable)
        /// and the provider throws an exception.
        internal void RemoveVariableAtScope(string name, string scopeID)
            RemoveVariableAtScope(name, scopeID, false);
        internal void RemoveVariableAtScope(string name, string scopeID, bool force)
                lookupScope.RemoveVariable(variablePath.QualifiedName, force);
        /// Remove a variable from session state.
        /// The variable to remove
        internal void RemoveVariableAtScope(PSVariable variable, string scopeID)
            RemoveVariableAtScope(variable, scopeID, false);
        internal void RemoveVariableAtScope(PSVariable variable, string scopeID, bool force)
            // The lookup scope is retrieved by ID.
        /// Gets a flattened view of the variables that are visible using
        /// the current scope as a reference and filtering the variables in
        /// An IDictionary representing the visible variables.
        internal IDictionary<string, PSVariable> GetVariableTable()
            Dictionary<string, PSVariable> result =
                new Dictionary<string, PSVariable>(StringComparer.OrdinalIgnoreCase);
                GetScopeVariableTable(scope, result, includePrivate: scope == _currentScope);
        private static void GetScopeVariableTable(SessionStateScope scope, Dictionary<string, PSVariable> result, bool includePrivate)
            foreach (KeyValuePair<string, PSVariable> entry in scope.Variables)
                    // Also check to ensure that the variable isn't private
                    // and in a different scope
                    PSVariable var = entry.Value;
                    if (!var.IsPrivate || includePrivate)
                        result.Add(entry.Key, var);
            foreach (var dottedScope in scope.DottedScopes)
                dottedScope.GetVariableTable(result, includePrivate);
            scope.LocalsTuple?.GetVariableTable(result, includePrivate);
        internal IDictionary<string, PSVariable> GetVariableTableAtScope(string scopeID)
            var result = new Dictionary<string, PSVariable>(StringComparer.OrdinalIgnoreCase);
            GetScopeVariableTable(GetScopeByID(scopeID), result, includePrivate: true);
        /// List of variables to export from this session state object...
        internal List<PSVariable> ExportedVariables { get; } = new List<PSVariable>();

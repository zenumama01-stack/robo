    /// A SessionStateScope defines the scope of visibility for a set
    /// of virtual drives and their data.
    internal sealed class SessionStateScope
        /// Constructor for a session state scope.
        /// <param name="parentScope">
        /// The parent of this scope.  It can be null for the global scope.
        internal SessionStateScope(SessionStateScope parentScope)
            ScopeOrigin = CommandOrigin.Internal;
            Parent = parentScope;
            if (parentScope != null)
                // Now copy the script: scope stack from the parent
                _scriptScope = parentScope.ScriptScope;
                _scriptScope = this;
        /// Gets the parent scope of this scope.  May be null
        /// for the global scope.
        internal SessionStateScope Parent { get; set; }
        /// Defines the origin of the command that resulted in this scope
        /// being created.
        internal CommandOrigin ScopeOrigin { get; set; }
        /// The script scope for this scope. It may reference itself but may not
        /// be a null reference.
        /// If <paramref name="value"/> is null when setting the property.
        internal SessionStateScope ScriptScope
                return _scriptScope;
                Diagnostics.Assert(value != null, "Caller to verify scope is not null");
                _scriptScope = value;
        private SessionStateScope _scriptScope;
        /// The version of strict mode for the interpreter.
        /// <value>Which version of strict mode is active for this scope and it's children.</value>
        internal Version StrictModeVersion { get; set; }
        /// Some local variables are stored in this tuple (for non-global scope, any variable assigned to,
        /// or parameters, or some predefined locals.)
        internal MutableTuple LocalsTuple { get; set; }
        /// When dotting a script, no new scope is created.  Automatic variables must go somewhere, so rather than store
        /// them in the scope they are dotted into, we just store them in a tuple like any other local variable so we
        /// can skip saving and restoring them as the scopes change, instead it's a simple push/pop of this stack.
        /// This works because in a dotted script block, the only locals in the tuple are the automatic variables, all
        /// other variables use the variable apis to find the variable and get/set it.
        internal Stack<MutableTuple> DottedScopes { get { return _dottedScopes; } }
        private readonly Stack<MutableTuple> _dottedScopes = new Stack<MutableTuple>();
        /// Adds a new drive to the scope's drive collection.
        /// <param name="newDrive">
        /// This method assumes the drive has already been verified and
        /// the provider has already been notified.
        /// If <paramref name="newDrive"/> is null.
        /// If a drive of the same name already exists in this scope.
        internal void NewDrive(PSDriveInfo newDrive)
                throw PSTraceSource.NewArgumentNullException(nameof(newDrive));
            // Ensure that multiple threads do not try to modify the
            // drive data at the same time.
            var driveInfos = GetDrives();
            if (driveInfos.ContainsKey(newDrive.Name))
                SessionStateException e =
                        newDrive.Name,
                        SessionStateCategory.Drive,
                        "DriveAlreadyExists",
                        SessionStateStrings.DriveAlreadyExists,
            if (!newDrive.IsAutoMounted)
                driveInfos.Add(newDrive.Name, newDrive);
                var automountedDrives = GetAutomountedDrives();
                if (!automountedDrives.ContainsKey(newDrive.Name))
                    automountedDrives.Add(newDrive.Name, newDrive);
        /// Removes the specified drive from this scope.
        /// This method assumes that the drive has already been validated for removal
        /// by the provider.
        internal void RemoveDrive(PSDriveInfo drive)
            if (_drives == null)
            if (!driveInfos.Remove(drive.Name))
                // Check to see if it is in the automounted drive collection.
                PSDriveInfo automountedDrive;
                if (automountedDrives.TryGetValue(drive.Name, out automountedDrive))
                    automountedDrive.IsAutoMountedManuallyRemoved = true;
                    // Remove ths persisted from the list of automounted drives.
                    if (drive.IsNetworkDrive)
                        automountedDrives.Remove(drive.Name);
        /// Removes all the drives from the scope.
        internal void RemoveAllDrives()
            GetDrives().Clear();
            GetAutomountedDrives().Clear();
        /// Retrieves the drive of the specified name.
        /// The name of the drive to retrieve.
        /// An instance of a PSDriveInfo object with the specified name if one
        /// exists in this scope or null if one does not exist.
            if (!driveInfos.TryGetValue(name, out result))
                // The caller needs to determine what to do with
                // manually removed drives.
                GetAutomountedDrives().TryGetValue(name, out result);
        /// Gets an IEnumerable for the drives in this scope.
        internal IEnumerable<PSDriveInfo> Drives
                Collection<PSDriveInfo> result = new Collection<PSDriveInfo>();
                foreach (PSDriveInfo drive in GetDrives().Values)
                    result.Add(drive);
                // Now add automounted drives that have not been manually
                // removed by the user.
                foreach (PSDriveInfo drive in GetAutomountedDrives().Values)
                    if (!drive.IsAutoMountedManuallyRemoved)
        #region Variables
        /// Gets an IDictionary for the variables in this scope.
        internal IDictionary<string, PSVariable> Variables { get { return GetPrivateVariables(); } }
        /// Gets the specified variable from the variable table.
        /// The name of the variable to retrieve.
        /// The origin of the command trying to retrieve this variable...
        /// The PSVariable representing the variable specified.
        internal PSVariable GetVariable(string name, CommandOrigin origin)
            PSVariable result;
            TryGetVariable(name, origin, false, out result);
        internal PSVariable GetVariable(string name)
            return GetVariable(name, ScopeOrigin);
        /// Looks up a variable, returns true and the variable if found and is visible, throws if the found variable is not visible,
        /// and returns false if there is no variable with the given name in the current scope.
        /// <param name="origin">The command origin (where the scope was created), used to decide if the variable is visible.</param>
        /// <param name="fromNewOrSet">True if looking up the variable as part of a new or set variable operation.</param>
        /// <param name="variable">The variable, if one is found in scope.</param>
        /// <exception cref="SessionStateException">Thrown if the variable is not visible based on CommandOrigin.</exception>
        /// <returns>True if there is a variable in scope, false otherwise.</returns>
        internal bool TryGetVariable(string name, CommandOrigin origin, bool fromNewOrSet, out PSVariable variable)
            Diagnostics.Assert(name != null, "The caller should verify the name");
            if (TryGetLocalVariableFromTuple(name, fromNewOrSet, out variable))
                SessionState.ThrowIfNotVisible(origin, variable);
            if (GetPrivateVariables().TryGetValue(name, out variable))
        /// <param name="variable"></param>
        internal object GetAutomaticVariableValue(AutomaticVariable variable)
            int index = (int)variable;
            foreach (var dottedScope in _dottedScopes)
                if (dottedScope.IsValueSet(index))
                    return dottedScope.GetValue(index);
            // LocalsTuple should not be null, but the test infrastructure creates scopes
            // and doesn't set LocalsTuple
            if (LocalsTuple != null && LocalsTuple.IsValueSet(index))
                return LocalsTuple.GetValue(index);
        /// Sets a variable to the given value.
        /// The name of the variable to set.
        /// The value for the variable
        /// <param name="asValue">
        /// If true, sets the variable value to newValue. If false, newValue must
        /// be a PSVariable object and the item will be set rather than the value.
        /// If true, the variable will be set even if it is readonly.
        /// Which SessionState this variable belongs to.
        /// <param name="fastPath">
        /// If true and the variable is being set in the global scope,
        /// then all of the normal variable lookup stuff is bypassed and
        /// the variable is added directly to the dictionary.
        /// The PSVariable representing the variable that was set.
        /// If the variable is read-only or constant.
        internal PSVariable SetVariable(string name, object value, bool asValue, bool force, SessionStateInternal sessionState, CommandOrigin origin = CommandOrigin.Internal, bool fastPath = false)
            PSVariable variable;
            PSVariable variableToSet = value as PSVariable;
            // Set the variable directly in the table, bypassing all of the checks. This
            // can only be used for global scope otherwise the slow path is used.
            if (fastPath)
                if (Parent != null)
                    throw new NotImplementedException("fastPath");
                variable = new PSVariable(name, variableToSet.Value, variableToSet.Options, variableToSet.Attributes) { Description = variableToSet.Description };
                GetPrivateVariables()[name] = variable;
                return variable;
            bool varExists = TryGetVariable(name, origin, true, out variable);
            // Initialize the private variable dictionary if it's not yet
                GetPrivateVariables();
            if (!asValue && variableToSet != null)
                if (varExists)
                    // First check the variable to ensure that it
                    // is not constant or readonly
                    if (variable == null || variable.IsConstant || (!force && variable.IsReadOnly))
                                    "VariableNotWritable",
                                    SessionStateStrings.VariableNotWritable);
                    if (variable is LocalVariable
                        && (variableToSet.Attributes.Count > 0 || variableToSet.Options != variable.Options))
                                    "VariableNotWritableRare",
                                    SessionStateStrings.VariableNotWritableRare);
                    if (variable.IsReadOnly && force)
                        _variables.Remove(name);
                        varExists = false;
                        // Since the variable already exists, copy
                        // the value, options, description, and attributes
                        // to it.
                        variable.Attributes.Clear();
                        variable.Value = variableToSet.Value;
                        variable.Options = variableToSet.Options;
                        variable.Description = variableToSet.Description;
                        foreach (Attribute attr in variableToSet.Attributes)
                            variable.Attributes.Add(attr);
                    // Since the variable doesn't exist, use the new Variable
                    variable = variableToSet;
            else if (variable != null)
                variable = (LocalsTuple?.TrySetVariable(name, value)) ?? new PSVariable(name, value);
                CheckVariableChangeInConstrainedLanguage(variable);
            _variables[name] = variable;
            variable.SessionState = sessionState;
        /// Sets a variable to scope without any checks.
        /// This is intended to be used only for global scope.
        /// <param name="variableToSet">PSVariable to set.</param>
        /// <param name="sessionState">SessionState for variable.</param>
        internal void SetVariableForce(PSVariable variableToSet, SessionStateInternal sessionState)
                throw new NotImplementedException("SetVariableForce");
            variableToSet.SessionState = sessionState;
            GetPrivateVariables()[variableToSet.Name] = variableToSet;
        /// <param name="newVariable">
        /// The new variable to create.
        internal PSVariable NewVariable(PSVariable newVariable, bool force, SessionStateInternal sessionState)
            bool varExists = TryGetVariable(newVariable.Name, ScopeOrigin, true, out variable);
                                newVariable.Name,
                if (variable is LocalVariable)
                // If the new and old variable are the same then don't bother
                // doing the assignment and marking as "removed".
                // This can happen when a module variable is imported twice.
                if (!ReferenceEquals(newVariable, variable))
                    // Mark the old variable as removed...
                    variable.WasRemoved = true;
                    variable = newVariable;
            _variables[variable.Name] = variable;
        /// Removes a variable from the variable table.
        /// The name of the variable to remove.
        /// If true, the variable will be removed even if its ReadOnly.
        /// if the variable is constant.
        internal void RemoveVariable(string name, bool force)
                name != null,
                "The caller should verify the name");
            PSVariable variable = GetVariable(name);
            if (variable.IsConstant || (variable.IsReadOnly && !force))
                            "VariableNotRemovable",
                            SessionStateStrings.VariableNotRemovable);
                            "VariableNotRemovableRare",
                            SessionStateStrings.VariableNotRemovableRare);
            // Finally mark the variable itself has having been removed so
            // anyone holding a reference to it can be aware of this.
        internal bool TrySetLocalParameterValue(string name, object value)
                if (dottedScope.TrySetParameter(name, value))
            return LocalsTuple != null && LocalsTuple.TrySetParameter(name, value);
        /// For most scopes (global scope being the notable exception), most variables are known ahead of
        /// time and stored in a tuple.  The names of those variables are stored separately, this method
        /// determines if variable name is active in this scope, and if so, returns a wrapper around the
        /// tuple to access the property in the tuple for the given variable.
        internal bool TryGetLocalVariableFromTuple(string name, bool fromNewOrSet, out PSVariable result)
                if (dottedScope.TryGetLocalVariable(name, fromNewOrSet, out result))
            return LocalsTuple != null && LocalsTuple.TryGetLocalVariable(name, fromNewOrSet, out result);
        #endregion variables
        #region Aliases
        /// Gets an IEnumerable for the aliases in this scope.
        internal IEnumerable<AliasInfo> AliasTable
                return GetAliases().Values;
        /// Gets the specified alias from the alias table.
        /// The name of the alias to retrieve.
        /// The string representing the value of the alias specified.
        internal AliasInfo GetAlias(string name)
            AliasInfo result;
            GetAliases().TryGetValue(name, out result);
        /// Sets an alias to the given value.
        /// The value for the alias
        /// The execution context for this engine instance.
        /// The string representing the value that was set.
        /// if the alias is read-only or constant.
        internal AliasInfo SetAliasValue(string name, string value, ExecutionContext context, bool force, CommandOrigin origin)
            var aliasInfos = GetAliases();
            AliasInfo aliasInfo;
            if (!aliasInfos.TryGetValue(name, out aliasInfo))
                aliasInfos[name] = new AliasInfo(name, value, context);
                // Make sure the alias isn't constant or readonly
                if ((aliasInfo.Options & ScopedItemOptions.Constant) != 0 ||
                    (!force && (aliasInfo.Options & ScopedItemOptions.ReadOnly) != 0))
                SessionState.ThrowIfNotVisible(origin, aliasInfo);
                RemoveAliasFromCache(aliasInfo.Name, aliasInfo.Definition);
                    aliasInfos.Remove(name);
                    aliasInfo = new AliasInfo(name, value, context);
                    aliasInfos[name] = aliasInfo;
                    aliasInfo.SetDefinition(value, false);
            AddAliasToCache(name, value);
            return aliasInfos[name];
                result = new AliasInfo(name, value, context, options);
                aliasInfos[name] = result;
                // Ensure we are not trying to set the alias to constant as this can only be
                // done at creation time.
                if ((options & ScopedItemOptions.Constant) != 0)
                if ((options & ScopedItemOptions.AllScope) == 0 &&
                    (aliasInfo.Options & ScopedItemOptions.AllScope) != 0)
                    result = aliasInfo;
                    aliasInfo.Options = options;
        /// <param name="aliasToSet">
        /// The information about the alias to be set
        internal AliasInfo SetAliasItem(AliasInfo aliasToSet, bool force, CommandOrigin origin = CommandOrigin.Internal)
                aliasToSet != null,
                "The caller should verify the aliasToSet");
            if (aliasInfos.TryGetValue(aliasToSet.Name, out aliasInfo))
                // An existing alias cannot be set if it is ReadOnly or Constant unless
                // force is specified, in which case an existing ReadOnly alias can
                // be set.
                    ((aliasInfo.Options & ScopedItemOptions.ReadOnly) != 0 && !force))
                                aliasToSet.Name,
                if ((aliasToSet.Options & ScopedItemOptions.AllScope) == 0 &&
            aliasInfos[aliasToSet.Name] = aliasToSet;
            AddAliasToCache(aliasToSet.Name, aliasToSet.Definition);
            return aliasToSet;
        /// Removes a alias from the alias table.
        /// If true, the alias will be removed even if it is ReadOnly.
        internal void RemoveAlias(string name, bool force)
            if (aliasInfos.TryGetValue(name, out aliasInfo))
                                "AliasNotRemovable",
                                SessionStateStrings.AliasNotRemovable);
        /// Gets an IEnumerable for the functions in this scope.
        internal Dictionary<string, FunctionInfo> FunctionTable
                return GetFunctions();
        /// Gets the specified function from the function table.
        /// The name of the function to retrieve.
        /// A FunctionInfo that is either a FilterInfo or FunctionInfo representing the
        /// function or filter.
            FunctionInfo result;
            GetFunctions().TryGetValue(name, out result);
        /// Sets an function to the given function declaration.
        /// The script block that represents the code for the function.
        /// The execution context for the function/filter.
            return SetFunction(name, function, null, ScopedItemOptions.Unspecified, force, origin, context);
        /// The original function (if any) from which the scriptblock was derived.
            return SetFunction(name, function, originalFunction, ScopedItemOptions.Unspecified, force, origin, context);
        /// The options that should be applied to the function.
            return SetFunction(name, function, originalFunction, options, force, origin, context, null);
            return SetFunction(name, function, originalFunction, options, force, origin, context, helpFile, CreateFunction);
        /// <param name="functionFactory">
        /// Function to create the FunctionInfo.
            Func<string, ScriptBlock, FunctionInfo, ScopedItemOptions, ExecutionContext, string, FunctionInfo> functionFactory)
            Dictionary<string, FunctionInfo> functionInfos = GetFunctions();
            // Functions are equal only if they have the same name and if they come from the same module (if any).
            // If the function is not associated with a module then the info 'ModuleName' property is set to empty string.
            // If the new function has the same name of an existing function, but different module names, then the
            // existing table function is replaced with the new function.
            if (!functionInfos.TryGetValue(name, out FunctionInfo existingValue) ||
                (originalFunction != null &&
                    !existingValue.ModuleName.Equals(originalFunction.ModuleName, StringComparison.OrdinalIgnoreCase)))
                // Add new function info to function table and return.
                result = functionFactory(name, function, originalFunction, options, context, helpFile);
                functionInfos[name] = result;
                if (IsFunctionOptionSet(result, ScopedItemOptions.AllScope))
                    GetAllScopeFunctions()[name] = result;
            // Update the existing function.
            // Make sure the function isn't constant or readonly.
            SessionState.ThrowIfNotVisible(origin, existingValue);
            if (IsFunctionOptionSet(existingValue, ScopedItemOptions.Constant) ||
                (!force && IsFunctionOptionSet(existingValue, ScopedItemOptions.ReadOnly)))
                            "FunctionNotWritable",
                            SessionStateStrings.FunctionNotWritable);
            // Ensure we are not trying to set the function to constant as this can only be
            // Ensure we are not trying to remove the AllScope option.
                IsFunctionOptionSet(existingValue, ScopedItemOptions.AllScope))
            FunctionInfo existingFunction = existingValue;
            // If the function type changes (i.e.: function to workflow or back)
            // then we need to replace what was there.
            FunctionInfo newValue = functionFactory(name, function, originalFunction, options, context, helpFile);
            bool changesFunctionType = existingFunction.GetType() != newValue.GetType();
            // Since the options are set after the script block, we have to
            // forcefully apply the script block if the options will be
            // set to not being ReadOnly.
            if (changesFunctionType ||
                ((existingFunction.Options & ScopedItemOptions.ReadOnly) != 0 && force))
                result = newValue;
                functionInfos[name] = newValue;
                bool applyForce = force || (options & ScopedItemOptions.ReadOnly) == 0;
                existingFunction.Update(newValue, applyForce, options, helpFile);
                result = existingFunction;
            var functionInfos = GetFunctions();
            FunctionInfo function;
            if (functionInfos.TryGetValue(name, out function))
                if (IsFunctionOptionSet(function, ScopedItemOptions.Constant) ||
                    (!force && IsFunctionOptionSet(function, ScopedItemOptions.ReadOnly)))
                                "FunctionNotRemovable",
                                SessionStateStrings.FunctionNotRemovable);
                if (IsFunctionOptionSet(function, ScopedItemOptions.AllScope))
                    GetAllScopeFunctions().Remove(name);
            functionInfos.Remove(name);
        #endregion functions
        #region Cmdlets
        /// Gets an IEnumerable for the cmdlets in this scope.
        internal Dictionary<string, List<CmdletInfo>> CmdletTable
                return _cmdlets;
        /// Gets the specified cmdlet from the cmdlet table.
        /// The name of the cmdlet to retrieve.
        /// A CmdletInfo representing this cmdlet
        internal CmdletInfo GetCmdlet(string name)
            if (_cmdlets.TryGetValue(name, out cmdlets))
                if (cmdlets != null && cmdlets.Count > 0)
                    result = cmdlets[0];
        /// Adds a cmdlet to the cmdlet cache.
        /// The cmdlet that should be added.
        /// The execution context for the cmdlet.
        /// A CmdletInfo representing the cmdlet
        /// If the cmdlet is read-only or constant.
        internal CmdletInfo AddCmdletToCache(
            CmdletInfo cmdlet,
            bool throwNotSupported = false;
                if (!_cmdlets.TryGetValue(name, out cmdlets))
                    cmdlets = new List<CmdletInfo>();
                    cmdlets.Add(cmdlet);
                    _cmdlets.Add(name, cmdlets);
                    if ((cmdlet.Options & ScopedItemOptions.AllScope) != 0)
                        _allScopeCmdlets[name].Insert(0, cmdlet);
                    if (!string.IsNullOrEmpty(cmdlet.ModuleName))
                        // Need to be sure that the existing cmdlet doesn't have the same snapin name
                        foreach (CmdletInfo cmdletInfo in cmdlets)
                            if (string.Equals(cmdlet.FullName, cmdletInfo.FullName,
                                if (cmdlet.ImplementingType == cmdletInfo.ImplementingType)
                                    // It is already added in the cache. Do not add it again
                                // Otherwise it's an error...
                                throwNotSupported = true;
                        // If there's no module name, then see if there is a cmdlet that matches the type
                                // It's already in the cache so don't need to add it again...
                    // Insert the cmdlet if a duplicate doesn't already exist
                    if (!throwNotSupported)
                        cmdlets.Insert(0, cmdlet);
            if (throwNotSupported)
                PSNotSupportedException notSupported =
                        DiscoveryExceptions.DuplicateCmdletName,
                        cmdlet.Name);
                throw notSupported;
            return _cmdlets[name][0];
        /// Removes a cmdlet from the cmdlet table.
        /// The index at which to remove the cmdlet
        /// If index is -1, remove all cmdlets with that name
        /// If the cmdlet is constant.
                CmdletInfo tempCmdlet = cmdlets[index];
                if ((tempCmdlet.Options & ScopedItemOptions.AllScope) != 0)
                    _allScopeCmdlets[name].RemoveAt(index);
                cmdlets.RemoveAt(index);
                // Remove the entry is the list is now empty
                if (cmdlets.Count == 0)
                    // Remove the key
                    _cmdlets.Remove(name);
        /// The key for the cmdlet entry to remove.
        /// If true, the cmdlet entry is removed even if it is ReadOnly.
        #endregion Cmdlets
        #region Types
        private Language.TypeResolutionState _typeResolutionState;
        internal Language.TypeResolutionState TypeResolutionState
                if (_typeResolutionState != null)
                    return _typeResolutionState;
                return Parent != null ? Parent.TypeResolutionState : Language.TypeResolutionState.UsingSystem;
                _typeResolutionState = value;
        internal IDictionary<string, Type> TypeTable { get; private set; }
        internal void AddType(string name, Type type)
            TypeTable ??= new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            TypeTable[name] = type;
        internal Type LookupType(string name)
            if (TypeTable == null)
            Type result;
            TypeTable.TryGetValue(name, out result);
        #endregion Types
        private static bool IsFunctionOptionSet(FunctionInfo function, ScopedItemOptions options)
            return (function.Options & options) != 0;
        private static FunctionInfo CreateFunction(string name, ScriptBlock function, FunctionInfo originalFunction,
            ScopedItemOptions options, ExecutionContext context, string helpFile)
            FunctionInfo newValue = null;
            if (options == ScopedItemOptions.Unspecified)
                options = ScopedItemOptions.None;
            // First use the copy constructors
            if (originalFunction is FilterInfo)
                newValue = new FilterInfo(name, (FilterInfo)originalFunction);
            else if (originalFunction is ConfigurationInfo)
                newValue = new ConfigurationInfo(name, (ConfigurationInfo)originalFunction);
            else if (originalFunction != null)
                newValue = new FunctionInfo(name, originalFunction);
            // Then use the creation constructors - workflows don't get here because the workflow info
            // is created during compilation.
            else if (function.IsFilter)
                newValue = new FilterInfo(name, function, options, context, helpFile);
            else if (function.IsConfiguration)
                newValue = new ConfigurationInfo(name, function, options, context, helpFile, function.IsMetaConfiguration());
                newValue = new FunctionInfo(name, function, options, context, helpFile);
        /// Contains the virtual drives for this scope.
        // Initializing all of the session state items every time we create a new scope causes a measurable
        // performance degradation, so we use lazy initialization for all of them.
        private Dictionary<string, PSDriveInfo> GetDrives()
            return _drives ??= new Dictionary<string, PSDriveInfo>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, PSDriveInfo> _drives;
        /// Contains the drives that have been automounted by the system.
        private Dictionary<string, PSDriveInfo> GetAutomountedDrives()
            return _automountedDrives ??= new Dictionary<string, PSDriveInfo>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, PSDriveInfo> _automountedDrives;
        private Dictionary<string, PSVariable> _variables;
        private Dictionary<string, PSVariable> GetPrivateVariables()
                // Create the variables collection with the default parameters
                _variables = new Dictionary<string, PSVariable>(StringComparer.OrdinalIgnoreCase);
                // Create the default variables in the global scope.
                // If the variable must propagate to each new scope,
                // the AllScope option must be set.
                AddSessionStateScopeDefaultVariables();
        /// Add the built-in variables defined by the session state scope.
        internal void AddSessionStateScopeDefaultVariables()
            if (Parent == null)
                // Create the default variables that are in every scope
                // These variables will automatically propagate to new
                // scopes since they are marked AllScope.
                _variables.Add(s_nullVar.Name, s_nullVar);
                _variables.Add(s_falseVar.Name, s_falseVar);
                _variables.Add(s_trueVar.Name, s_trueVar);
                // Propagate all variables that are marked AllScope.
                foreach (PSVariable variable in Parent.GetPrivateVariables().Values)
                    if (variable.IsAllScope)
                        _variables.Add(variable.Name, variable);
        /// A collection of the aliases defined for the session.
        private Dictionary<string, AliasInfo> GetAliases()
            if (_alias == null)
                // Create the alias table
                _alias = new Dictionary<string, AliasInfo>(StringComparer.OrdinalIgnoreCase);
                    // Propagate all aliases that are marked AllScope
                    foreach (AliasInfo newAlias in Parent.GetAliases().Values)
                        if ((newAlias.Options & ScopedItemOptions.AllScope) != 0)
                            _alias.Add(newAlias.Name, newAlias);
            return _alias;
        private Dictionary<string, AliasInfo> _alias;
        /// A collection of the functions defined in this scope...
        private Dictionary<string, FunctionInfo> GetFunctions()
            if (_functions == null)
                // Create the functions table
                _functions = new Dictionary<string, FunctionInfo>(StringComparer.OrdinalIgnoreCase);
                if (Parent != null && Parent._allScopeFunctions != null)
                    // Propagate all functions that are marked AllScope
                    foreach (FunctionInfo newFunc in Parent._allScopeFunctions.Values)
                        _functions.Add(newFunc.Name, newFunc);
            return _functions;
        private Dictionary<string, FunctionInfo> _functions;
        /// All entries in this table should also be in the normal function
        /// table. The entries in this table are automatically propagated
        /// to new scopes.
        private Dictionary<string, FunctionInfo> GetAllScopeFunctions()
            if (_allScopeFunctions == null)
                    return Parent._allScopeFunctions;
                // Create the "AllScope" functions table
                _allScopeFunctions = new Dictionary<string, FunctionInfo>(StringComparer.OrdinalIgnoreCase);
            return _allScopeFunctions;
        private Dictionary<string, FunctionInfo> _allScopeFunctions;
        // The value for the cmdlet cache is a list of CmdletInfo objects because of the following reason
        // Import-Module Mod1 -Cmdlet foo
        // Import-Module Mod2 -Cmdlet foo
        // Remove-Module Mod2
        // foo
        // The command "foo" from Mod1 is invoked.
        // If we do not maintain a list, we break this behavior as we would have over-written Mod1\foo with Mod2\foo and then Mod2 is removed, we have nothing.
        private readonly Dictionary<string, List<CmdletInfo>> _cmdlets = new Dictionary<string, List<CmdletInfo>>(StringComparer.OrdinalIgnoreCase);
        /// All entries in this table should also be in the normal cmdlet
        private readonly Dictionary<string, List<CmdletInfo>> _allScopeCmdlets = new Dictionary<string, List<CmdletInfo>>(StringComparer.OrdinalIgnoreCase);
        /// The variable that represents $true in the language.
        /// We don't need a new reference in each scope since it
        /// is ScopedItemOptions.Constant.
        private static readonly PSVariable s_trueVar =
            new PSVariable(
                StringLiterals.True,
                "Boolean True");
        /// The variable that represents $false in the language.
        private static readonly PSVariable s_falseVar =
                StringLiterals.False,
                "Boolean False");
        /// The variable that represents $null in the language.
        private static readonly NullVariable s_nullVar =
            new NullVariable();
        #region Alias mapping
        private readonly Dictionary<string, List<string>> _commandsToAliasesCache = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            List<string> commandsToAliases;
            if (_commandsToAliasesCache.TryGetValue(command, out commandsToAliases))
                foreach (string str in commandsToAliases)
                    yield return str;
        /// <param name="alias"></param>
        private void AddAliasToCache(string alias, string value)
            List<string> existingAliases;
            if (!_commandsToAliasesCache.TryGetValue(value, out existingAliases))
                list.Add(alias);
                _commandsToAliasesCache.Add(value, list);
                if (!existingAliases.Contains(alias, StringComparer.OrdinalIgnoreCase))
                    existingAliases.Add(alias);
        private void RemoveAliasFromCache(string alias, string value)
            List<string> list;
            if (!_commandsToAliasesCache.TryGetValue(value, out list))
            if (list.Count <= 1)
                _commandsToAliasesCache.Remove(value);
                string itemToRemove = list.Find(item => item.Equals(alias, StringComparison.OrdinalIgnoreCase));
                if (itemToRemove != null)
                    list.Remove(itemToRemove);
        private void CheckVariableChangeInConstrainedLanguage(PSVariable variable)
                if (variable.Options.HasFlag(ScopedItemOptions.AllScope))
                        // Don't let people set AllScope variables in ConstrainedLanguage, as they can be used to
                        // interfere with the session state of trusted commands.
                        throw new PSNotSupportedException();
                        title: SessionStateStrings.WDACSessionStateVarLogTitle,
                        message: StringUtil.Format(SessionStateStrings.WDACSessionStateVarLogMessage, variable.Name),
                        fqid: "AllScopeVariableNotAllowed",
                ExecutionContext.MarkObjectAsUntrustedForVariableAssignment(variable, this, context.EngineSessionState);

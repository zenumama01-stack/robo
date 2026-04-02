    /// Exposes the APIs to manipulate variables in the Runspace.
    public sealed class PSVariableIntrinsics
        private PSVariableIntrinsics()
        /// Constructs a facade for the specified session.
        /// The session for which the facade wraps.
        internal PSVariableIntrinsics(SessionStateInternal sessionState)
                throw PSTraceSource.NewArgumentException(nameof(sessionState));
        /// Gets the specified variable from session state.
        /// The name of the variable to get. The name can contain drive and/or
        /// scope specifiers like "ENV:path" or "global:myvar".
        public PSVariable Get(string name)
            // Null is returned whenever the requested variable is string.Empty.
            // As per Powershell V1 implementation:
            // 1. If the requested variable exists in the session scope, the variable value is returned.
            // 2. If the requested variable is not null and does not exist in the session scope, then a null value is returned to the pipeline.
            // 3. If the requested variable is null then an NewArgumentNullException is thrown.
            // PowerShell V3 has the similar experience.
            if (name != null && name.Equals(string.Empty))
            return _sessionState.GetVariable(name);
        /// Gets the specified variable from session state in the specified scope.
        /// If the variable doesn't exist in the specified scope no additional lookup
        /// will be done.
        /// The ID of the scope to do the lookup in.
        internal PSVariable GetAtScope(string name, string scope)
            return _sessionState.GetVariableAtScope(name, scope);
        /// Gets the specified variable value from session state.
            return _sessionState.GetVariableValue(name);
        /// Gets the specified variable from session state. If the variable
        /// is not found the default value is returned.
        /// The default value returned if the variable could not be found.
        /// The value of the specified variable or the default value if the variable
        public object GetValue(string name, object defaultValue)
            return _sessionState.GetVariableValue(name) ?? defaultValue;
        internal object GetValueAtScope(string name, string scope)
            return _sessionState.GetVariableValueAtScope(name, scope);
        /// Sets the variable to the specified value.
        /// The name of the variable to be set. The name can contain drive and/or
        public void Set(string name, object value)
            _sessionState.SetVariableValue(name, value, CommandOrigin.Internal);
        /// Sets the variable.
        public void Set(PSVariable variable)
            _sessionState.SetVariable(variable, false, CommandOrigin.Internal);
        /// Removes the specified variable from session state.
        /// The name of the variable to be removed. The name can contain drive and/or
        public void Remove(string name)
            _sessionState.RemoveVariable(name);
        /// The variable to be removed. It is removed based on the name of the variable.
        public void Remove(PSVariable variable)
            _sessionState.RemoveVariable(variable);
        /// Removes the specified variable from the specified scope.
        /// The ID of the scope to do the lookup in. The ID is a zero based index
        /// being 1 and so on.
        internal void RemoveAtScope(string name, string scope)
            _sessionState.RemoveVariableAtScope(name, scope);
        internal void RemoveAtScope(PSVariable variable, string scope)
            _sessionState.RemoveVariableAtScope(variable, scope);

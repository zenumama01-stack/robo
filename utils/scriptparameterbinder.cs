    /// The parameter binder for shell functions.
    internal class ScriptParameterBinder : ParameterBinderBase
        /// Constructs a ScriptParameterBinder with the specified context.
        /// The script block representing the code being run
        /// The invocation information about the code that is being run.
        /// The context under which the shell function is executing.
        /// The command instance that represents the script in a pipeline. May be null.
        /// <param name="localScope">
        /// If binding in a new local scope, the scope to set variables in.  If dotting, the value is null.
        internal ScriptParameterBinder(
            ScriptBlock script,
            SessionStateScope localScope) : base(invocationInfo, context, command)
            Diagnostics.Assert(script != null, "caller to verify script is not null.");
            this.Script = script;
            this.LocalScope = localScope;
        /// <exception cref="Exception">See SessionStateInternal.GetVariableValue.</exception>
            if (Script.RuntimeDefinedParameters.TryGetValue(name, out runtimeDefinedParameter))
                return GetDefaultScriptParameterValue(runtimeDefinedParameter);
        /// Binds the parameters to local variables in the function scope.
            if (value == AutomationNull.Value || value == UnboundParameter.Value)
            Diagnostics.Assert(name != null, "The caller should verify that name is not null");
            var varPath = new VariablePath(name, VariablePathFlags.Variable);
            // If the parameter was allocated in the LocalsTuple, we can avoid creating a PSVariable,
            if (LocalScope != null
                && varPath.IsAnyLocal()
                && LocalScope.TrySetLocalParameterValue(varPath.UnqualifiedPath, CopyMutableValues(value)))
            // Otherwise we'll fall through and enter a new PSVariable in the current scope.  This
            // is what normally happens when dotting (though the above may succeed if a parameter name
            // was an automatic variable like $PSBoundParameters.
            // First we need to make a variable instance and apply
            // any attributes from the script.
            PSVariable variable = new PSVariable(varPath.UnqualifiedPath, value,
                                                 varPath.IsPrivate ? ScopedItemOptions.Private : ScopedItemOptions.None);
            Context.EngineSessionState.SetVariable(varPath, variable, false, CommandOrigin.Internal);
                // The attributes have already been checked and conversions run, so it is wrong
                // to do so again.
                variable.AddParameterAttributesNoChecks(runtimeDefinedParameter.Attributes);
        /// Return the default value of a script parameter, evaluating the parse tree if necessary.
        internal object GetDefaultScriptParameterValue(RuntimeDefinedParameter parameter, IDictionary implicitUsingParameters = null)
            object result = parameter.Value;
            var compiledDefault = result as Compiler.DefaultValueExpressionWrapper;
            if (compiledDefault != null)
                result = compiledDefault.GetValue(Context, Script.SessionStateInternal, implicitUsingParameters);
        /// The script that is being bound to.
        internal ScriptBlock Script { get; }
        internal SessionStateScope LocalScope { get; set; }

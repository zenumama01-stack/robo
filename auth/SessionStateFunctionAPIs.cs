        #region Functions
        /// Add an new SessionState function entry to this session state object...
        internal void AddSessionStateEntry(SessionStateFunctionEntry entry)
            ScriptBlock sb = entry.ScriptBlock.Clone();
            FunctionInfo fn = this.SetFunction(entry.Name, sb, null, entry.Options, false, CommandOrigin.Internal, this.ExecutionContext, entry.HelpFile, true);
            fn.Visibility = entry.Visibility;
            fn.Module = entry.Module;
            fn.ScriptBlock.LanguageMode = entry.ScriptBlock.LanguageMode ?? PSLanguageMode.FullLanguage;
        /// Gets a flattened view of the functions that are visible using
        /// the current scope as a reference and filtering the functions in
        /// the other scopes based on the scoping rules.
        /// An IDictionary representing the visible functions.
        internal IDictionary<string, FunctionInfo> GetFunctionTable()
            Dictionary<string, FunctionInfo> result =
                new Dictionary<string, FunctionInfo>(StringComparer.OrdinalIgnoreCase);
                foreach (FunctionInfo entry in scope.FunctionTable.Values)
        /// Gets an IEnumerable for the function table for a given scope.
        internal IDictionary<string, FunctionInfo> GetFunctionTableAtScope(string scopeID)
                // Make sure the function/filter isn't private or if it is that the current
        internal List<FunctionInfo> ExportedFunctions { get; } = new List<FunctionInfo>();
        internal bool UseExportList { get; set; } = false;
        /// Set to true when module functions are being explicitly exported using Export-ModuleMember.
        internal bool FunctionsExported { get; set; }
        /// Set to true when any processed module functions are being explicitly exported using '*' wildcard.
        internal bool FunctionsExportedWithWildcard
                return _functionsExportedWithWildcard;
                Dbg.Assert((value), "This property should never be set/reset to false");
                    _functionsExportedWithWildcard = value;
        private bool _functionsExportedWithWildcard;
        /// Set to true if module loading is performed under a manifest that explicitly exports functions (no wildcards)
        internal bool ManifestWithExplicitFunctionExport { get; set; }
        /// Get a functions out of session state.
        /// name of function to look up
        /// Origin of the command that called this API...
        /// The value of the specified function.
        internal FunctionInfo GetFunction(string name, CommandOrigin origin)
            FunctionInfo result = null;
            FunctionLookupPath lookupPath = new FunctionLookupPath(name);
            FunctionScopeItemSearcher searcher =
                new FunctionScopeItemSearcher(this, lookupPath, origin);
            if (searcher.MoveNext())
                result = ((IEnumerator<FunctionInfo>)searcher).Current;
            return (IsFunctionVisibleInDebugger(result, origin)) ? result : null;
        private bool IsFunctionVisibleInDebugger(FunctionInfo fnInfo, CommandOrigin origin)
            // Ensure the returned function item is not exposed across language boundaries when in
            // a debugger breakpoint or nested prompt.
            // A debugger breakpoint/nested prompt has access to all current scoped functions.
            // This includes both running commands from the prompt or via a debugger Action scriptblock.
            // Early out.
            // Always allow built-in functions needed for command line debugging.
            if (this.ExecutionContext.LanguageMode == PSLanguageMode.FullLanguage ||
                (fnInfo == null) ||
                (fnInfo.Name.Equals("prompt", StringComparison.OrdinalIgnoreCase)) ||
                (fnInfo.Name.Equals("TabExpansion2", StringComparison.OrdinalIgnoreCase)) ||
                (fnInfo.Name.Equals("Clear-Host", StringComparison.Ordinal)))
            // Check both InNestedPrompt and Debugger.InBreakpoint to ensure we don't miss a case.
            // Function is not visible if function and context language modes are different.
            var runspace = this.ExecutionContext.CurrentRunspace;
            if ((runspace != null) &&
                (runspace.InNestedPrompt || (runspace.Debugger?.InBreakpoint == true)) &&
                (fnInfo.DefiningLanguageMode.HasValue && (fnInfo.DefiningLanguageMode != this.ExecutionContext.LanguageMode)))
        internal FunctionInfo GetFunction(string name)
            return GetFunction(name, CommandOrigin.Internal);
        private static IEnumerable<string> GetFunctionAliases(IParameterMetadataProvider ipmp)
            if (ipmp == null || ipmp.Body.ParamBlock == null)
            var attributes = ipmp.Body.ParamBlock.Attributes;
            foreach (var attributeAst in attributes)
                var attributeType = attributeAst.TypeName.GetReflectionAttributeType();
                if (attributeType == typeof(AliasAttribute))
                    var cvv = new ConstantValueVisitor { AttributeArgument = true };
                    for (int i = 0; i < attributeAst.PositionalArguments.Count; i++)
                        yield return Compiler.s_attrArgToStringConverter.Target(Compiler.s_attrArgToStringConverter,
                            attributeAst.PositionalArguments[i].Accept(cvv));
        /// Set a function in the current scope of session state.
        /// The name of the function to set.
        /// The new value of the function being set.
        /// Origin of the caller of this API
        /// If the function is read-only or constant.
        internal FunctionInfo SetFunctionRaw(
            ScriptBlock function,
            string originalName = name;
            FunctionLookupPath path = new FunctionLookupPath(name);
            name = path.UnqualifiedPath;
                SessionStateException exception =
                    new SessionStateException(
                        originalName,
                        "ScopedFunctionMustHaveName",
                        SessionStateStrings.ScopedFunctionMustHaveName,
            if (path.IsPrivate)
                options |= ScopedItemOptions.Private;
                new FunctionScopeItemSearcher(
                    origin);
            var functionInfo = searcher.InitialScope.SetFunction(name, function, null, options, false, origin, ExecutionContext);
            foreach (var aliasName in GetFunctionAliases(function.Ast as IParameterMetadataProvider))
                searcher.InitialScope.SetAliasValue(aliasName, name, ExecutionContext, false, origin);
            return functionInfo;
        /// <param name="originalFunction">
        /// The original function (if any) from which the ScriptBlock is derived.
        /// The options to set on the function.
        /// If true, the function will be set even if its ReadOnly.
        internal FunctionInfo SetFunction(
            FunctionInfo originalFunction,
            return SetFunction(name, function, originalFunction, options, force, origin, ExecutionContext, null);
            return SetFunction(name, function, originalFunction, options, force, origin, ExecutionContext, helpFile, false);
            return SetFunction(name, function, originalFunction, options, force, origin, context, helpFile, false);
        /// <param name="isPreValidated">
        /// Set to true if it is a regular function (meaning, we do not need to check if the script contains JobDefinition Attribute and then process it)
            bool isPreValidated)
            return searcher.InitialScope.SetFunction(name, function, originalFunction, options, force, origin, context, helpFile);
        /// The origin of the caller
        /// If <paramref name="function"/> is not a <see cref="FilterInfo">FilterInfo</see>
        /// or <see cref="FunctionInfo">FunctionInfo</see>
            SessionStateScope scope = searcher.InitialScope;
                scope = searcher.CurrentLookupScope;
                name = searcher.Name;
                    // Need to add the Private flag
                    FunctionInfo existingFunction = scope.GetFunction(name);
                    options |= existingFunction.Options;
                    result = scope.SetFunction(name, function, originalFunction, options, force, origin, ExecutionContext);
                    result = scope.SetFunction(name, function, force, origin, ExecutionContext);
        /// BUGBUG: this overload is preserved because a lot of tests use reflection to
        /// call it. The tests should be fixed and this API eventually removed.
        internal FunctionInfo SetFunction(string name, ScriptBlock function, bool force)
            return SetFunction(name, function, null, force, CommandOrigin.Internal);
        /// Removes a function from the function table.
        /// The name of the function to remove.
        /// If true, the function is removed even if it is ReadOnly.
        internal void RemoveFunction(string name, bool force, CommandOrigin origin)
            scope.RemoveFunction(name, force);
        internal void RemoveFunction(string name, bool force)
            RemoveFunction(name, force, CommandOrigin.Internal);
        /// Removes a function from the function table
        /// if the function was imported from the given module.
        /// BUGBUG: This is only used by the implicit remoting functions...
        /// <param name="module">
        /// Module the function might be imported from.
        internal void RemoveFunction(string name, PSModuleInfo module)
            Dbg.Assert(module != null, "Caller should verify that module parameter is not null");
            FunctionInfo func = GetFunction(name) as FunctionInfo;
            if (func != null && func.ScriptBlock != null
                && func.ScriptBlock.File != null
                && func.ScriptBlock.File.Equals(module.Path, StringComparison.OrdinalIgnoreCase))
                RemoveFunction(name, true);
        #endregion Functions

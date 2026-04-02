    /// Provides information about a function that is stored in session state.
    public class FunctionInfo : CommandInfo, IScriptCommandInfo
        /// Creates an instance of the FunctionInfo class with the specified name and ScriptBlock.
        /// The name of the function.
        /// The ScriptBlock for the function
        /// The execution context for the function.
        /// If <paramref name="function"/> is null.
        internal FunctionInfo(string name, ScriptBlock function, ExecutionContext context) : this(name, function, context, null)
        /// The name of the help file associated with the function.
        internal FunctionInfo(string name, ScriptBlock function, ExecutionContext context, string helpFile) : base(name, CommandTypes.Function, context)
            if (function == null)
                throw PSTraceSource.NewArgumentNullException(nameof(function));
            _scriptBlock = function;
            CmdletInfo.SplitCmdletName(name, out _verb, out _noun);
            this.Module = function.Module;
            _helpFile = helpFile;
        internal FunctionInfo(string name, ScriptBlock function, ScopedItemOptions options, ExecutionContext context) : this(name, function, options, context, null)
        internal FunctionInfo(string name, ScriptBlock function, ScopedItemOptions options, ExecutionContext context, string helpFile)
            : this(name, function, context, helpFile)
        internal FunctionInfo(FunctionInfo other)
            CopyFieldsFromOther(other);
        private void CopyFieldsFromOther(FunctionInfo other)
            _scriptBlock = other._scriptBlock;
            _description = other._description;
            _options = other._options;
            _helpFile = other._helpFile;
        internal FunctionInfo(string name, FunctionInfo other)
            FunctionInfo copy = new FunctionInfo(this) { IsGetCommandCopy = true, Arguments = arguments };
            get { return HelpCategory.Function; }
        /// Gets the ScriptBlock which is the implementation of the function.
            get { return _scriptBlock; }
        /// Updates a function.
        /// <param name="newFunction">
        /// The script block that the function should represent.
        /// If true, the script block will be applied even if the filter is ReadOnly.
        /// Any options to set on the new function, null if none.
        /// If <paramref name="newFunction"/> is null.
        internal void Update(ScriptBlock newFunction, bool force, ScopedItemOptions options)
            Update(newFunction, force, options, null);
            this.DefiningLanguageMode = newFunction.LanguageMode;
        protected internal virtual void Update(FunctionInfo newFunction, bool force, ScopedItemOptions options, string helpFile)
            Update(newFunction.ScriptBlock, force, options, helpFile);
        /// The helpfile for this function.
        internal void Update(ScriptBlock newFunction, bool force, ScopedItemOptions options, string helpFile)
            if (newFunction == null)
                throw PSTraceSource.NewArgumentNullException("function");
                            SessionStateCategory.Function,
                            "FunctionIsConstant",
                            SessionStateStrings.FunctionIsConstant);
                            "FunctionIsReadOnly",
                            SessionStateStrings.FunctionIsReadOnly);
            _scriptBlock = newFunction;
            this.Module = newFunction.Module;
            _commandMetadata = null;
            this._parameterSets = null;
            this.ExternalCommandMetadata = null;
            if (options != ScopedItemOptions.Unspecified)
                this.Options = options;
        /// Returns <see langword="true"/> if this function uses cmdlet binding mode for its parameters; otherwise returns <see langword="false"/>.
        public bool CmdletBinding
                return this.ScriptBlock.UsesCmdletBinding;
        /// Returns <see langword="null"/> if this function doesn't use cmdlet parameter binding or if the default parameter set wasn't specified.
                return this.CmdletBinding ? this.CommandMetadata.DefaultParameterSetName : null;
        /// Gets the definition of the function which is the
        /// ToString() of the ScriptBlock that implements the function.
        public override string Definition { get { return _scriptBlock.ToString(); } }
        /// Gets or sets the scope options for the function.
        /// If the trying to set a function that is constant or
                return CopiedCommand == null ? _options : ((FunctionInfo)CopiedCommand).Options;
                    // Check to see if the function is constant, if so
                    if ((value & ScopedItemOptions.Constant) != 0)
                        // user is trying to set the function to constant after
                        // creating the function. Do not allow this (as per spec).
                                    "FunctionCannotBeMadeConstant",
                                    SessionStateStrings.FunctionCannotBeMadeConstant);
                    // Ensure we are not trying to remove the AllScope option
                    if ((value & ScopedItemOptions.AllScope) == 0 &&
                                    "FunctionAllScopeOptionCannotBeRemoved",
                                    SessionStateStrings.FunctionAllScopeOptionCannotBeRemoved);
                    ((FunctionInfo)CopiedCommand).Options = value;
        /// Gets or sets the description associated with the function.
                return CopiedCommand == null ? _description : ((FunctionInfo)CopiedCommand).Description;
                    _description = value;
                    ((FunctionInfo)CopiedCommand).Description = value;
        private string _description = null;
        /// Gets the verb of the function.
        private string _verb = string.Empty;
        /// Gets the noun of the function.
        private string _noun = string.Empty;
        /// Gets the help file path for the function.
                return _helpFile;
                _helpFile = value;
        private string _helpFile = string.Empty;
            get { return ScriptBlock.HasDynamicParameters; }
        /// The command metadata for the function or filter.

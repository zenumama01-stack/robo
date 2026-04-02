    /// Provides information about a mapping between a command name and a real command.
    public class AliasInfo : CommandInfo
        /// Creates an instance of the AliasInfo class with the specified name and referenced command.
        /// <param name="definition">
        /// The token that the alias refers to.
        /// The execution context for this engine, used to lookup the current session state.
        /// If <paramref name="definition"/> is null or empty.
        /// If <paramref name="context"/> is null.
        internal AliasInfo(string name, string definition, ExecutionContext context) : base(name, CommandTypes.Alias)
            _definition = definition;
            this.Context = context;
                this.Module = context.SessionState.Internal.Module;
        /// The execution context for this engine instance, used to look up session state.
        /// The options to set on the alias. Note, Constant can only be set at creation time.
        internal AliasInfo(
            string definition,
            ScopedItemOptions options) : base(name, CommandTypes.Alias)
            _options = options;
        internal override HelpCategory HelpCategory
            get { return HelpCategory.Alias; }
        /// Gets the command information for the command that is immediately referenced by this alias.
        public CommandInfo ReferencedCommand
                // Need to lookup the referenced command every time
                // to ensure we get the latest session state information
                CommandInfo referencedCommand = null;
                if ((_definition != null) && (Context != null))
                    CommandSearcher commandSearcher =
                        new CommandSearcher(
                            _definition,
                            SearchResolutionOptions.None,
                            Context);
                    if (commandSearcher.MoveNext())
                        System.Collections.Generic.IEnumerator<CommandInfo> ie = commandSearcher;
                        referencedCommand = ie.Current;
                        // referencedCommand = commandSearcher.Current;
                return referencedCommand;
        /// Gets the command information for the command that
        /// the alias eventually resolves to.
        /// An alias may reference another alias. This property follows the reference
        /// chain of aliases to its end.
        /// <!--
        /// If the command didn't resolve to anything but aliases, the UnresolvedCommandName
        /// property contains the last name the resolution succeeded in finding.
        /// -->
        public CommandInfo ResolvedCommand
                // Need to lookup the resolved command every time to ensure
                // we use the latest session state information
                CommandInfo result = null;
                if (_definition != null)
                    List<string> cyclePrevention = new List<string>();
                    cyclePrevention.Add(Name);
                    string commandNameToResolve = _definition;
                    result = ReferencedCommand;
                    while (result != null && result.CommandType == CommandTypes.Alias)
                        result = ((AliasInfo)result).ReferencedCommand;
                        if (result is AliasInfo)
                            // Check for the cycle by checking for the alias name
                            // in the cyclePrevention dictionary
                            if (SessionStateUtilities.CollectionContainsValue(cyclePrevention, result.Name, StringComparer.OrdinalIgnoreCase))
                            cyclePrevention.Add(result.Name);
                            commandNameToResolve = result.Definition;
                        // Since we couldn't resolve the command that the alias
                        // points to, remember the definition so that we can
                        // provide better error reporting.
                        UnresolvedCommandName = commandNameToResolve;
        /// Gets the name of the command to which the alias refers.
        public override string Definition
                return _definition;
        private string _definition = string.Empty;
        /// Sets the new definition for the alias.
        /// The new definition for the alias.
        /// <param name="force">
        /// If true, the value will be set even if the alias is ReadOnly.
        /// <exception cref="SessionStateUnauthorizedAccessException">
        /// If the alias is readonly or constant.
        internal void SetDefinition(string definition, bool force)
            // Check to see if the variable is writable
            if ((_options & ScopedItemOptions.Constant) != 0 ||
                (!force && (_options & ScopedItemOptions.ReadOnly) != 0))
                SessionStateUnauthorizedAccessException e =
                    new SessionStateUnauthorizedAccessException(
                            "AliasNotWritable",
                            SessionStateStrings.AliasNotWritable);
        /// Gets or sets the scope options for the alias.
        /// <exception cref="System.Management.Automation.SessionStateUnauthorizedAccessException">
        /// If the trying to set an alias that is constant or
        ///     if the value trying to be set is ScopedItemOptions.Constant
        public ScopedItemOptions Options
                SetOptions(value, false);
        /// Sets the options for the alias and allows changes ReadOnly options only if force is specified.
        /// <param name="newOptions">
        /// The new options value.
        /// If true the change to the options will happen even if the existing options are read-only.
        internal void SetOptions(ScopedItemOptions newOptions, bool force)
            // Check to see if the variable is constant, if so
            // throw an exception because the options cannot be changed.
            if ((_options & ScopedItemOptions.Constant) != 0)
                            "AliasIsConstant",
                            SessionStateStrings.AliasIsConstant);
            // Check to see if the variable is readonly, if so
            if (!force && (_options & ScopedItemOptions.ReadOnly) != 0)
                            "AliasIsReadOnly",
                            SessionStateStrings.AliasIsReadOnly);
            // Now check to see if the caller is trying to set
            // the options to constant. This is only allowed at
            // variable creation
            if ((newOptions & ScopedItemOptions.Constant) != 0)
                // user is trying to set the variable to constant after
                // creating the variable. Do not allow this (as per spec).
                            "AliasCannotBeMadeConstant",
                            SessionStateStrings.AliasCannotBeMadeConstant);
            if ((newOptions & ScopedItemOptions.AllScope) == 0 &&
                (_options & ScopedItemOptions.AllScope) != 0)
                // user is trying to remove the AllScope option from the alias.
                // Do not allow this (as per spec).
                            "AliasAllScopeOptionCannotBeRemoved",
                            SessionStateStrings.AliasAllScopeOptionCannotBeRemoved);
            _options = newOptions;
        private ScopedItemOptions _options = ScopedItemOptions.None;
        /// Gets or sets the description for the alias.
        /// If ResolvedCommand returns null, this property will
        /// return the name of the command that could not be resolved.
        /// If ResolvedCommand has not yet been called or was able
        /// to resolve the command, this property will return null.
        internal string UnresolvedCommandName { get; private set; }
        /// The objects output from an alias are the objects output from the resolved
        /// command.  If we can't resolve the command, assume nothing is output - so use void.
        public override ReadOnlyCollection<PSTypeName> OutputType
                CommandInfo resolvedCommand = this.ResolvedCommand;
                if (resolvedCommand != null)
                    return resolvedCommand.OutputType;

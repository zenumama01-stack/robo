    /// Base class for all variable commands.
    /// Because -Scope is defined in VariableCommandBase, all derived commands must implement -Scope.
    public abstract class VariableCommandBase : PSCmdlet
        /// Selects active scope to work with; used for all variable commands.
        /// The Include parameter for all the variable commands.
        protected string[] IncludeFilters
                return _include;
                _include = value;
        private string[] _include = Array.Empty<string>();
        /// The Exclude parameter for all the variable commands.
        protected string[] ExcludeFilters
                return _exclude;
                _exclude = value;
        private string[] _exclude = Array.Empty<string>();
        #region helpers
        /// Gets the matching variable for the specified name, using the
        /// Include, Exclude, and Scope parameters defined in the base class.
        /// The name or pattern of the variables to retrieve.
        /// <param name="lookupScope">
        /// The scope to do the lookup in. If null or empty the normal scoping rules apply.
        /// <param name="wasFiltered">
        /// True is returned if a variable exists of the given name but was filtered
        /// out via globbing, include, or exclude.
        /// <param name="quiet">
        /// If true, don't report errors when trying to access private variables.
        /// A collection of the variables matching the name, include, and exclude
        /// pattern in the specified scope.
        internal List<PSVariable> GetMatchingVariables(string name, string lookupScope, out bool wasFiltered, bool quiet)
            wasFiltered = false;
            List<PSVariable> result = new();
                name = "*";
            bool nameContainsWildcard = WildcardPattern.ContainsWildcardCharacters(name);
            // Now create the filters
            WildcardPattern nameFilter =
            Collection<WildcardPattern> includeFilters =
                    _include,
            Collection<WildcardPattern> excludeFilters =
                    _exclude,
            if (!nameContainsWildcard)
                // Filter the name here against the include and exclude so that
                // we can report if the name was filtered vs. there being no
                // variable existing of that name.
                bool isIncludeMatch =
                    SessionStateUtilities.MatchesAnyWildcardPattern(
                        includeFilters,
                bool isExcludeMatch =
                        excludeFilters,
                        false);
                if (!isIncludeMatch || isExcludeMatch)
                    wasFiltered = true;
            // First get the appropriate view of the variables. If no scope
            // is specified, flatten all scopes to produce a currently active
            // view.
            IDictionary<string, PSVariable> variableTable = null;
            if (string.IsNullOrEmpty(lookupScope))
                variableTable = SessionState.Internal.GetVariableTable();
                variableTable = SessionState.Internal.GetVariableTableAtScope(lookupScope);
            foreach (KeyValuePair<string, PSVariable> entry in variableTable)
                bool isNameMatch = nameFilter.IsMatch(entry.Key);
                        entry.Key,
                if (isNameMatch)
                    if (isIncludeMatch && !isExcludeMatch)
                        // See if the variable is visible
                        if (!SessionState.IsVisible(origin, entry.Value))
                            // In quiet mode, don't report private variable accesses unless they are specific matches...
                            if (quiet || nameContainsWildcard)
                                // Generate an error for elements that aren't visible...
                                    SessionState.ThrowIfNotVisible(origin, entry.Value);
                        result.Add(entry.Value);
                    if (nameContainsWildcard)
        #endregion helpers
    /// Implements get-variable command.
    [Cmdlet(VerbsCommon.Get, "Variable", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096711")]
    [OutputType(typeof(PSVariable))]
    public class GetVariableCommand : VariableCommandBase
        /// Name of the PSVariable.
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
                return _name;
                value ??= new string[] { "*" };
        /// Output only the value(s) of the requested variable(s).
        public SwitchParameter ValueOnly
                return _valueOnly;
                _valueOnly = value;
        private bool _valueOnly;
                return IncludeFilters;
                IncludeFilters = value;
                return ExcludeFilters;
                ExcludeFilters = value;
        /// Implements ProcessRecord() method for get-variable's command.
            foreach (string varName in _name)
                bool wasFiltered = false;
                List<PSVariable> matchingVariables =
                    GetMatchingVariables(varName, Scope, out wasFiltered, /*quiet*/ false);
                matchingVariables.Sort(
                    static (PSVariable left, PSVariable right) => StringComparer.CurrentCultureIgnoreCase.Compare(left.Name, right.Name));
                bool matchFound = false;
                foreach (PSVariable matchingVariable in matchingVariables)
                    matchFound = true;
                    if (_valueOnly)
                        WriteObject(matchingVariable.Value);
                        WriteObject(matchingVariable);
                if (!matchFound && !wasFiltered)
                            varName,
                            "VariableNotFound",
                            SessionStateStrings.VariableNotFound);
    /// Class implementing new-variable command.
    [Cmdlet(VerbsCommon.New, "Variable", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Low,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097121")]
    public sealed class NewVariableCommand : VariableCommandBase
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = true)]
        /// Value of the PSVariable.
        /// Description of the variable.
        /// The options for the variable to specify if the variable should
        /// be ReadOnly, Constant, and/or Private.
        public ScopedItemOptions Option { get; set; } = ScopedItemOptions.None;
        /// Specifies the visibility of the new variable...
        public SessionStateEntryVisibility Visibility
                return (SessionStateEntryVisibility)_visibility;
                _visibility = value;
        private SessionStateEntryVisibility? _visibility;
        /// Force the operation to make the best attempt at setting the variable.
        /// The variable object should be passed down the pipeline.
        /// Add objects received on the pipeline to an ArrayList of values, to
        /// take the place of the Value parameter if none was specified on the
        /// command line.
            // If Force is not specified, see if the variable already exists
            // in the specified scope. If the scope isn't specified, then
            // check to see if it exists in the current scope.
                PSVariable varFound = null;
                    varFound =
                        SessionState.PSVariable.GetAtScope(Name, "local");
                        SessionState.PSVariable.GetAtScope(Name, Scope);
                if (varFound != null)
                    SessionStateException sessionStateException =
                            SessionStateCategory.Variable,
                            "VariableAlreadyExists",
                            SessionStateStrings.VariableAlreadyExists,
            // Since the variable doesn't exist or -Force was specified,
            // Call should process to validate the set with the user.
            string action = VariableCommandStrings.NewVariableAction;
            string target = StringUtil.Format(VariableCommandStrings.NewVariableTarget, Name, Value);
                PSVariable newVariable = new(Name, Value, Option);
                if (_visibility != null)
                    newVariable.Visibility = (SessionStateEntryVisibility)_visibility;
                    newVariable.Description = Description;
                        SessionState.Internal.NewVariable(newVariable, Force);
                        SessionState.Internal.NewVariableAtScope(newVariable, Scope, Force);
                    WriteObject(newVariable);
    /// This class implements set-variable command.
    [Cmdlet(VerbsCommon.Set, "Variable", SupportsShouldProcess = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096624")]
    public sealed class SetVariableCommand : VariableCommandBase
        /// Name of the PSVariable(s) to set.
        public object Value { get; set; } = AutomationNull.Value;
        public ScopedItemOptions Option
                return (ScopedItemOptions)_options;
                _options = value;
        private ScopedItemOptions? _options;
        /// Sets the visibility of the variable...
        /// Gets whether we will append to the variable if it exists.
        private bool _nameIsFormalParameter;
        private bool _valueIsFormalParameter;
        /// Checks to see if the name and value parameters were bound as formal parameters.
                _nameIsFormalParameter = true;
            if (Value != AutomationNull.Value)
                _valueIsFormalParameter = true;
            if (Append)
                // create the list here and add to it if it has a value
                // but if they have more than one name, produce an error
                if (Name.Length != 1)
                    ErrorRecord appendVariableError = new ErrorRecord(new InvalidOperationException(), "SetVariableAppend", ErrorCategory.InvalidOperation, Name);
                    appendVariableError.ErrorDetails = new ErrorDetails("SetVariableAppend");
                    appendVariableError.ErrorDetails.RecommendedAction = VariableCommandStrings.UseSingleVariable;
                    ThrowTerminatingError(appendVariableError);
                _valueList = new List<object>();
                var currentValue = Context.SessionState.PSVariable.Get(Name[0]);
                if (currentValue is not null)
                    if (currentValue.Value is IList<object> ilist)
                        _valueList.AddRange(ilist);
                        _valueList.Add(currentValue.Value);
        /// If name and value are both specified as a formal parameters, then
        /// just ignore the incoming objects in ProcessRecord.
        /// If name is a formal parameter but the value is coming from the pipeline,
        /// then accumulate the values in the valueList and set the variable during
        /// EndProcessing().
        /// If name is not a formal parameter, then set
        /// the variable each time ProcessRecord is called.
            if (_nameIsFormalParameter && _valueIsFormalParameter)
                        _valueList ??= new List<object>();
                        _valueList.Add(Value);
            if (_nameIsFormalParameter && !_valueIsFormalParameter)
                SetVariable(Name, Value);
        private List<object> _valueList;
        /// Sets the variable if the name was specified as a formal parameter
        /// but the value came from the pipeline.
            if (_nameIsFormalParameter)
                if (_valueIsFormalParameter)
                        SetVariable(Name, _valueList);
                    if (_valueList != null)
                        if (_valueList.Count == 1)
                            SetVariable(Name, _valueList[0]);
                        else if (_valueList.Count == 0)
                            SetVariable(Name, AutomationNull.Value);
                            SetVariable(Name, _valueList.ToArray());
        /// Sets the variables of the given names to the specified value.
        /// <param name="varNames">
        /// The name(s) of the variables to set.
        /// <param name="varValue">
        /// The value to set the variable to.
        private void SetVariable(string[] varNames, object varValue)
            foreach (string varName in varNames)
                // First look for existing variables to set.
                List<PSVariable> matchingVariables = new();
                    // We really only need to find matches if the scope was specified.
                    // If the scope wasn't specified then we need to create the
                    // variable in the local scope.
                    matchingVariables =
                        GetMatchingVariables(varName, Scope, out wasFiltered, /* quiet */ false);
                    // Since the scope wasn't specified, it doesn't matter if there
                    // is a variable in another scope, it only matters if there is a
                        GetMatchingVariables(
                            System.Management.Automation.StringLiterals.Local,
                            out wasFiltered,
                // We only want to create the variable if we are not filtering
                // the name.
                if (matchingVariables.Count == 0 &&
                    !wasFiltered)
                        ScopedItemOptions newOptions = ScopedItemOptions.None;
                        if (!string.IsNullOrEmpty(Scope) &&
                            string.Equals("private", Scope, StringComparison.OrdinalIgnoreCase))
                            newOptions = ScopedItemOptions.Private;
                        if (_options != null)
                            newOptions |= (ScopedItemOptions)_options;
                        object newVarValue = varValue;
                        if (newVarValue == AutomationNull.Value)
                            newVarValue = null;
                        PSVariable varToSet =
                                newVarValue,
                                newOptions);
                        Description ??= string.Empty;
                        varToSet.Description = Description;
                        // If visibility was specified, set it on the variable
                            varToSet.Visibility = Visibility;
                        string action = VariableCommandStrings.SetVariableAction;
                        string target = StringUtil.Format(VariableCommandStrings.SetVariableTarget, varName, newVarValue);
                                    SessionState.Internal.SetVariable(varToSet, Force, origin);
                                    SessionState.Internal.SetVariableAtScope(varToSet, Scope, Force, origin);
                            if (_passThru && result != null)
                        string target = StringUtil.Format(VariableCommandStrings.SetVariableTarget, matchingVariable.Name, varValue);
                                // Since the variable existed in the specified scope, or
                                // in the local scope if no scope was specified, use
                                // the reference returned to set the variable properties.
                                // If we want to force setting over a readonly variable
                                // we have to temporarily mark the variable writable.
                                bool wasReadOnly = false;
                                if (Force &&
                                    (matchingVariable.Options & ScopedItemOptions.ReadOnly) != 0)
                                    matchingVariable.SetOptions(matchingVariable.Options & ~ScopedItemOptions.ReadOnly, true);
                                    wasReadOnly = true;
                                // Now change the value, options, or description
                                // and set the variable
                                if (varValue != AutomationNull.Value)
                                    matchingVariable.Value = varValue;
                                        // In 'ConstrainedLanguage' we want to monitor untrusted values assigned to 'Global:' variables
                                        // and 'Script:' variables, because they may be set from 'ConstrainedLanguage' environment and
                                        // referenced within trusted script block, and thus result in security issues.
                                        // Here we are setting the value of an existing variable and don't know what scope this variable
                                        // is from, so we mark the value as untrusted, regardless of the scope.
                                        ExecutionContext.MarkObjectAsUntrusted(matchingVariable.Value);
                                    matchingVariable.Description = Description;
                                    matchingVariable.Options = (ScopedItemOptions)_options;
                                    if (wasReadOnly)
                                        matchingVariable.SetOptions(matchingVariable.Options | ScopedItemOptions.ReadOnly, true);
                                    matchingVariable.Visibility = Visibility;
                                result = matchingVariable;
    /// The Remove-Variable cmdlet implementation.
    [Cmdlet(VerbsCommon.Remove, "Variable", SupportsShouldProcess = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097123")]
    public sealed class RemoveVariableCommand : VariableCommandBase
        /// If true, the variable is removed even if it is ReadOnly.
        /// Removes the matching variables from the specified scope.
            // Removal of variables only happens in the local scope if the
            // scope wasn't explicitly specified by the user.
            Scope ??= "local";
            foreach (string varName in Name)
                if (matchingVariables.Count == 0 && !wasFiltered)
                    // Since the variable wasn't found and no glob
                    // characters were specified, write an error.
                    string action = VariableCommandStrings.RemoveVariableAction;
                    string target = StringUtil.Format(VariableCommandStrings.RemoveVariableTarget, matchingVariable.Name);
                                SessionState.Internal.RemoveVariable(matchingVariable, _force);
                                SessionState.Internal.RemoveVariableAtScope(matchingVariable, Scope, _force);
    [Cmdlet(VerbsCommon.Clear, "Variable", SupportsShouldProcess = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096923")]
    public sealed class ClearVariableCommand : VariableCommandBase
        /// Force the operation to make the best attempt at clearing the variable.
        /// The implementation of the Clear-Variable command.
                    string action = VariableCommandStrings.ClearVariableAction;
                    string target = StringUtil.Format(VariableCommandStrings.ClearVariableTarget, matchingVariable.Name);
                        PSVariable result = matchingVariable;
                            if (_force &&
                                // Remove the ReadOnly bit to set the value and then reapply
                                result = ClearValue(matchingVariable);
        /// Clears the value of the variable using the PSVariable instance if the scope
        /// was specified or using standard variable lookup if the scope was not specified.
        /// <param name="matchingVariable">
        /// The variable that matched the name parameter(s).
        private PSVariable ClearValue(PSVariable matchingVariable)
            if (Scope != null)
                matchingVariable.Value = null;
                SessionState.PSVariable.Set(matchingVariable.Name, null);
                result = SessionState.PSVariable.Get(matchingVariable.Name);

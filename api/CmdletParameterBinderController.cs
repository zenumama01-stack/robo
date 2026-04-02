    /// This is the interface between the CommandProcessor and the various
    /// parameter binders required to bind parameters to a cmdlet.
    internal class CmdletParameterBinderController : ParameterBinderController
        [TraceSource("ParameterBinderController", "Controls the interaction between the command processor and the parameter binder(s).")]
        private static readonly PSTraceSource s_tracer = PSTraceSource.GetTracer("ParameterBinderController", "Controls the interaction between the command processor and the parameter binder(s).");
        /// Initializes the cmdlet parameter binder controller for
        /// the specified cmdlet and engine context.
        /// The cmdlet that the parameters will be bound to.
        /// <param name="commandMetadata">
        /// The metadata about the cmdlet.
        /// <param name="parameterBinder">
        /// The default parameter binder to use.
        internal CmdletParameterBinderController(
            CommandMetadata commandMetadata,
            ParameterBinderBase parameterBinder)
                cmdlet.MyInvocation,
                cmdlet.Context,
                parameterBinder)
            if (commandMetadata == null)
                throw PSTraceSource.NewArgumentNullException(nameof(commandMetadata));
            this.Command = cmdlet;
            _commandRuntime = (MshCommandRuntime)cmdlet.CommandRuntime;
            _commandMetadata = commandMetadata;
            // Add the static parameter metadata to the bindable parameters
            // And add them to the unbound parameters list
            if (commandMetadata.ImplementsDynamicParameters)
                // ReplaceMetadata makes a copy for us, so we can use that collection as is.
                this.UnboundParameters = this.BindableParameters.ReplaceMetadata(commandMetadata.StaticCommandParameterMetadata);
                _bindableParameters = commandMetadata.StaticCommandParameterMetadata;
                // Must make a copy of the list because we'll modify it.
                this.UnboundParameters = new List<MergedCompiledCommandParameter>(_bindableParameters.BindableParameters.Values);
        #region helper_methods
        /// Binds the specified command-line parameters to the target.
        /// <param name="arguments">
        /// Parameters to the command.
        /// <exception cref="ParameterBindingException">
        /// If any parameters fail to bind,
        /// or
        /// If any mandatory parameters are missing.
        /// If there is an error generating the metadata for dynamic parameters.
        internal void BindCommandLineParameters(Collection<CommandParameterInternal> arguments)
            s_tracer.WriteLine("Argument count: {0}", arguments.Count);
            BindCommandLineParametersNoValidation(arguments);
            // Is pipeline input expected?
            bool isPipelineInputExpected = !(_commandRuntime.IsClosed && _commandRuntime.InputPipe.Empty);
            int validParameterSetCount;
            if (!isPipelineInputExpected)
                // Since pipeline input is not expected, ensure that we have a single
                // parameter set and that all the mandatory
                // parameters for the working parameter set are specified, or prompt
                validParameterSetCount = ValidateParameterSets(false, true);
                // Use ValidateParameterSets to get the number of valid parameter
                // sets.
                // NTRAID#Windows Out Of Band Releases-2005/11/07-923917-JonN
                validParameterSetCount = ValidateParameterSets(true, false);
            // If the parameter set is determined and the default parameters are not used
            // we try the default parameter binding again because it may contain some mandatory
            // parameters
            if (validParameterSetCount == 1 && !DefaultParameterBindingInUse)
                ApplyDefaultParameterBinding("Mandatory Checking", false);
            // If there are multiple valid parameter sets and we are expecting pipeline inputs,
            // we should filter out those parameter sets that cannot take pipeline inputs anymore.
            if (validParameterSetCount > 1 && isPipelineInputExpected)
                uint filteredValidParameterSetFlags = FilterParameterSetsTakingNoPipelineInput();
                if (filteredValidParameterSetFlags != _currentParameterSetFlag)
                    _currentParameterSetFlag = filteredValidParameterSetFlags;
                    // The valid parameter set flag is narrowed down, we get the new validParameterSetCount
            using (ParameterBinderBase.bindingTracer.TraceScope(
                "MANDATORY PARAMETER CHECK on cmdlet [{0}]",
                _commandMetadata.Name))
                    // The missingMandatoryParameters out parameter is used for error reporting when binding from the pipeline.
                    // We're not binding from the pipeline here, and if a mandatory non-pipeline parameter is missing, it will
                    // be prompted for, or an exception will be raised, so we can ignore the missingMandatoryParameters out parameter.
                    Collection<MergedCompiledCommandParameter> missingMandatoryParameters;
                    // We shouldn't prompt for mandatory parameters if this command is private.
                    bool promptForMandatoryParameters = (Command.CommandInfo.Visibility == SessionStateEntryVisibility.Public);
                    HandleUnboundMandatoryParameters(validParameterSetCount, true, promptForMandatoryParameters, isPipelineInputExpected, out missingMandatoryParameters);
                    if (DefaultParameterBinder is ScriptParameterBinder)
                        BindUnboundScriptParameters();
                catch (ParameterBindingException pbex)
                    if (!DefaultParameterBindingInUse)
                    ThrowElaboratedBindingException(pbex);
            // If there is no more expected input, ensure there is a single
            // parameter set selected
                VerifyParameterSetSelected();
            // Set the prepipeline parameter set flags so that they can be restored
            // between each pipeline object.
            _prePipelineProcessingParameterSetFlags = _currentParameterSetFlag;
        /// Binds the unbound arguments to parameters but does not
        /// perform mandatory parameter validation or parameter set validation.
        internal void BindCommandLineParametersNoValidation(Collection<CommandParameterInternal> arguments)
            var psCompiledScriptCmdlet = this.Command as PSScriptCmdlet;
            psCompiledScriptCmdlet?.PrepareForBinding(this.CommandLineParameters);
            InitUnboundArguments(arguments);
            CommandMetadata cmdletMetadata = _commandMetadata;
            // Clear the warningSet at the beginning.
            _warningSet.Clear();
            // Parse $PSDefaultParameterValues to get all valid <parameter, value> pairs
            _allDefaultParameterValuePairs = this.GetDefaultParameterValuePairs(true);
            // Set to false at the beginning
            DefaultParameterBindingInUse = false;
            // Clear the bound default parameters at the beginning
            BoundDefaultParameters.Clear();
            // Reparse the arguments based on the merged metadata
            ReparseUnboundArguments();
                "BIND NAMED cmd line args [{0}]",
                // Bind the actual arguments
                UnboundArguments = BindNamedParameters(_currentParameterSetFlag, this.UnboundArguments);
            ParameterBindingException reportedBindingException;
            ParameterBindingException currentBindingException;
                "BIND POSITIONAL cmd line args [{0}]",
                // Now that we know the parameter set, bind the positional parameters
                UnboundArguments =
                    BindPositionalParameters(
                        UnboundArguments,
                        _currentParameterSetFlag,
                        cmdletMetadata.DefaultParameterSetFlag,
                        out currentBindingException);
                reportedBindingException = currentBindingException;
            // Try applying the default parameter binding after POSITIONAL BIND so that the default parameter
            // values can influence the parameter set selection earlier than the default parameter set.
            ApplyDefaultParameterBinding("POSITIONAL BIND", false);
            // We need to make sure there is at least one valid parameter set. Its
            // OK to allow more than one as long as one of them takes pipeline input.
            // NTRAID#Windows Out Of Band Releases-2006/02/14-928660-JonN
            // Pipeline input fails to bind to pipeline enabled parameter
            // second parameter changed from true to false
            ValidateParameterSets(true, false);
            // Always get the dynamic parameters as there may be mandatory parameters there
            // Now try binding the dynamic parameters
            HandleCommandLineDynamicParameters(out currentBindingException);
            // Try binding the default parameters again. After dynamic binding, new parameter metadata are
            // included, so it's possible a previously unsuccessful binding will succeed.
            ApplyDefaultParameterBinding("DYNAMIC BIND", true);
            // If this generated an exception (but we didn't have one from the non-dynamic
            // parameters, report on this one.
            reportedBindingException ??= currentBindingException;
            // If the cmdlet implements a ValueFromRemainingArguments parameter (VarArgs)
            // bind the unbound arguments to that parameter.
            HandleRemainingArguments();
            VerifyArgumentsProcessed(reportedBindingException);
        /// Process all valid parameter sets, and filter out those that don't take any pipeline input.
        /// The new valid parameter set flags
        private uint FilterParameterSetsTakingNoPipelineInput()
            uint parameterSetsTakingPipeInput = 0;
            bool findPipeParameterInAllSets = false;
            foreach (KeyValuePair<MergedCompiledCommandParameter, DelayedScriptBlockArgument> entry in _delayBindScriptBlocks)
                parameterSetsTakingPipeInput |= entry.Key.Parameter.ParameterSetFlags;
            foreach (MergedCompiledCommandParameter parameter in UnboundParameters)
                // If a parameter doesn't take pipeline input at all, we can skip it
                if (!parameter.Parameter.IsPipelineParameterInSomeParameterSet)
                var matchingParameterSetMetadata =
                    parameter.Parameter.GetMatchingParameterSetData(_currentParameterSetFlag);
                foreach (ParameterSetSpecificMetadata parameterSetMetadata in matchingParameterSetMetadata)
                    if (parameterSetMetadata.ValueFromPipeline || parameterSetMetadata.ValueFromPipelineByPropertyName)
                        if (parameterSetMetadata.ParameterSetFlag == 0 && parameterSetMetadata.IsInAllSets)
                            // The parameter takes pipeline input and is in all sets, we don't change the _currentParameterSetFlag
                            parameterSetsTakingPipeInput = 0;
                            findPipeParameterInAllSets = true;
                            parameterSetsTakingPipeInput |= parameterSetMetadata.ParameterSetFlag;
                if (findPipeParameterInAllSets)
            // If parameterSetsTakingPipeInput is 0, then no parameter set from the _currentParameterSetFlag can take piped objects.
            // Then we just leave what it was, and the pipeline binding deal with the error later
            if (parameterSetsTakingPipeInput != 0)
                return _currentParameterSetFlag & parameterSetsTakingPipeInput;
                return _currentParameterSetFlag;
        /// Apply the binding for the default parameter defined by the user.
        /// <param name="bindingStage">
        /// Dictate which binding stage this default binding happens
        /// <param name="isDynamic">
        /// Special operation needed if the default binding happens at the dynamic binding stage
        private void ApplyDefaultParameterBinding(string bindingStage, bool isDynamic)
            if (!_useDefaultParameterBinding)
            if (isDynamic)
                // Get user defined default parameter value pairs again, so that the
                // dynamic parameter value pairs could be involved.
                _allDefaultParameterValuePairs = GetDefaultParameterValuePairs(false);
            Dictionary<MergedCompiledCommandParameter, object> qualifiedParameterValuePairs = GetQualifiedParameterValuePairs(_currentParameterSetFlag, _allDefaultParameterValuePairs);
            if (qualifiedParameterValuePairs != null)
                    "BIND DEFAULT <parameter, value> pairs after [{0}] for [{1}]",
                    bindingStage, _commandMetadata.Name))
                    isSuccess = BindDefaultParameters(_currentParameterSetFlag, qualifiedParameterValuePairs);
                    if (isSuccess && !DefaultParameterBindingInUse)
                        DefaultParameterBindingInUse = true;
                s_tracer.WriteLine("BIND DEFAULT after [{0}] result [{1}]", bindingStage, isSuccess);
        /// Bind the default parameter value pairs.
        /// <param name="validParameterSetFlag">ValidParameterSetFlag.</param>
        /// <param name="defaultParameterValues">Default value pairs.</param>
        /// true if there is at least one default parameter bound successfully
        /// false if there is no default parameter bound successfully
        private bool BindDefaultParameters(uint validParameterSetFlag, Dictionary<MergedCompiledCommandParameter, object> defaultParameterValues)
            foreach (var pair in defaultParameterValues)
                MergedCompiledCommandParameter parameter = pair.Key;
                object argumentValue = pair.Value;
                string parameterName = parameter.Parameter.Name;
                    ScriptBlock scriptBlockArg = argumentValue as ScriptBlock;
                    if (scriptBlockArg != null)
                        // Get the current binding state, and pass it to the ScriptBlock as the argument
                        // The 'arg' includes HashSet properties 'BoundParameters', 'BoundPositionalParameters',
                        // 'BoundDefaultParameters', and 'LastBindingStage'. So the user can set value
                        // to a parameter depending on the current binding state.
                        PSObject arg = WrapBindingState();
                        Collection<PSObject> results = scriptBlockArg.Invoke(arg);
                        if (results == null || results.Count == 0)
                        else if (results.Count == 1)
                            argumentValue = results[0];
                            argumentValue = results;
                    CommandParameterInternal bindableArgument =
                           /*parameterAst*/null, parameterName, "-" + parameterName + ":",
                           /*argumentAst*/null, argumentValue, false);
                    bool bindResult =
                            BindParameter(
                                validParameterSetFlag,
                                bindableArgument,
                                parameter,
                                ParameterBindingFlags.ShouldCoerceType | ParameterBindingFlags.DelayBindScriptBlock);
                    if (bindResult && !ret)
                        ret = true;
                    if (bindResult)
                        BoundDefaultParameters.Add(parameterName);
                catch (ParameterBindingException ex)
                    // We don't want the failures in default binding affect the command line binding,
                    // so we write out a warning and ignore this binding failure
                    if (!_warningSet.Contains(_commandMetadata.Name + Separator + parameterName))
                            ParameterBinderStrings.FailToBindDefaultParameter,
                            LanguagePrimitives.IsNull(argumentValue) ? "null" : argumentValue.ToString(),
                            parameterName, ex.Message);
                        _commandRuntime.WriteWarning(message);
                        _warningSet.Add(_commandMetadata.Name + Separator + parameterName);
        /// Wrap up current binding state to provide more information to the user.
        private PSObject WrapBindingState()
            HashSet<string> boundParameterNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> boundPositionalParameterNames =
                this.DefaultParameterBinder.CommandLineParameters.CopyBoundPositionalParameters();
            HashSet<string> boundDefaultParameterNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string paramName in BoundParameters.Keys)
                boundParameterNames.Add(paramName);
            foreach (string paramName in BoundDefaultParameters)
                boundDefaultParameterNames.Add(paramName);
            PSObject result = new PSObject();
            result.Properties.Add(new PSNoteProperty("BoundParameters", boundParameterNames));
            result.Properties.Add(new PSNoteProperty("BoundPositionalParameters", boundPositionalParameterNames));
            result.Properties.Add(new PSNoteProperty("BoundDefaultParameters", boundDefaultParameterNames));
        /// Get all qualified default parameter value pairs based on the
        /// given currentParameterSetFlag.
        /// <param name="currentParameterSetFlag"></param>
        /// <param name="availableParameterValuePairs"></param>
        /// <returns>Null if no qualified pair found.</returns>
        private Dictionary<MergedCompiledCommandParameter, object> GetQualifiedParameterValuePairs(
            uint currentParameterSetFlag,
            Dictionary<MergedCompiledCommandParameter, object> availableParameterValuePairs)
            if (availableParameterValuePairs == null)
            Dictionary<MergedCompiledCommandParameter, object> result = new Dictionary<MergedCompiledCommandParameter, object>();
            uint possibleParameterFlag = uint.MaxValue;
            foreach (var pair in availableParameterValuePairs)
                MergedCompiledCommandParameter param = pair.Key;
                if ((param.Parameter.ParameterSetFlags & currentParameterSetFlag) == 0 && !param.Parameter.IsInAllSets)
                if (BoundArguments.ContainsKey(param.Parameter.Name))
                // check if this param's set conflicts with other possible params.
                if (param.Parameter.ParameterSetFlags != 0)
                    possibleParameterFlag &= param.Parameter.ParameterSetFlags;
                    if (possibleParameterFlag == 0)
                result.Add(param, pair.Value);
            if (result.Count > 0)
        /// Get the aliases of the current cmdlet.
        private List<string> GetAliasOfCurrentCmdlet()
            var results = Context.SessionState.Internal.GetAliasesByCommandName(_commandMetadata.Name).ToList();
            return results.Count > 0 ? results : null;
        /// Check if the passed-in aliasName matches an alias name in _aliasList.
        /// <param name="aliasName"></param>
        private bool MatchAnyAlias(string aliasName)
            if (_aliasList == null)
            WildcardPattern aliasPattern = WildcardPattern.Get(aliasName, WildcardOptions.IgnoreCase);
            foreach (string alias in _aliasList)
                if (aliasPattern.IsMatch(alias))
        internal IDictionary DefaultParameterValues { get; set; }
        /// Get all available default parameter value pairs.
        /// <returns>Return the available parameter value pairs. Otherwise return null.</returns>
        private Dictionary<MergedCompiledCommandParameter, object> GetDefaultParameterValuePairs(bool needToGetAlias)
            if (DefaultParameterValues == null)
                _useDefaultParameterBinding = false;
            var availablePairs = new Dictionary<MergedCompiledCommandParameter, object>();
            if (needToGetAlias && DefaultParameterValues.Count > 0)
                // Get all aliases of the current cmdlet
                _aliasList = GetAliasOfCurrentCmdlet();
            // Set flag to true by default
            _useDefaultParameterBinding = true;
            string currentCmdletName = _commandMetadata.Name;
            IDictionary<string, MergedCompiledCommandParameter> bindableParameters = BindableParameters.BindableParameters;
            IDictionary<string, MergedCompiledCommandParameter> bindableAlias = BindableParameters.AliasedParameters;
            // Contains parameters that are set with different values by settings in $PSDefaultParameterValues.
            // We should ignore those settings and write out a warning
            var parametersToRemove = new HashSet<MergedCompiledCommandParameter>();
            var wildcardDefault = new Dictionary<string, object>();
            // Contains keys that are in bad format. For every bad format key, we should write out a warning message
            // the first time we encounter it, and remove it from the $PSDefaultParameterValues
            var keysToRemove = new List<object>();
            foreach (DictionaryEntry entry in DefaultParameterValues)
                if (entry.Key is not string key)
                key = key.Trim();
                string cmdletName = null;
                string parameterName = null;
                // The key is not in valid format
                if (!DefaultParameterDictionary.CheckKeyIsValid(key, ref cmdletName, ref parameterName))
                    if (key.Equals("Disabled", StringComparison.OrdinalIgnoreCase) &&
                        LanguagePrimitives.IsTrue(entry.Value))
                    // Write out a warning message if the key is not 'Disabled'
                    if (!key.Equals("Disabled", StringComparison.OrdinalIgnoreCase))
                        keysToRemove.Add(entry.Key);
                Diagnostics.Assert(cmdletName != null && parameterName != null, "The cmdletName and parameterName should be set in CheckKeyIsValid");
                if (WildcardPattern.ContainsWildcardCharacters(key))
                    wildcardDefault.Add(cmdletName + Separator + parameterName, entry.Value);
                // Continue to process this entry only if the specified cmdletName is the name
                // of the current cmdlet, or is an alias name of the current cmdlet.
                if (!cmdletName.Equals(currentCmdletName, StringComparison.OrdinalIgnoreCase) && !MatchAnyAlias(cmdletName))
                GetDefaultParameterValuePairsHelper(
                    cmdletName, parameterName, entry.Value,
                    bindableParameters, bindableAlias,
                    availablePairs, parametersToRemove);
            foreach (KeyValuePair<string, object> wildcard in wildcardDefault)
                string key = wildcard.Key;
                string cmdletName = key.Substring(0, key.IndexOf(Separator, StringComparison.OrdinalIgnoreCase));
                string parameterName = key.Substring(key.IndexOf(Separator, StringComparison.OrdinalIgnoreCase) + Separator.Length);
                WildcardPattern cmdletPattern = WildcardPattern.Get(cmdletName, WildcardOptions.IgnoreCase);
                // Continue to process this entry only if the cmdletName matches the name of the current
                // cmdlet, or matches an alias name of the current cmdlet
                if (!cmdletPattern.IsMatch(currentCmdletName) && !MatchAnyAlias(cmdletName))
                if (!WildcardPattern.ContainsWildcardCharacters(parameterName))
                        cmdletName, parameterName, wildcard.Value,
                WildcardPattern parameterPattern = MemberMatch.GetNamePattern(parameterName);
                var matches = new List<MergedCompiledCommandParameter>();
                foreach (KeyValuePair<string, MergedCompiledCommandParameter> entry in bindableParameters)
                    if (parameterPattern.IsMatch(entry.Key))
                        matches.Add(entry.Value);
                foreach (KeyValuePair<string, MergedCompiledCommandParameter> entry in bindableAlias)
                if (matches.Count > 1)
                    // The parameterPattern matches more than one parameters, so we write out a warning message and ignore this setting
                    if (!_warningSet.Contains(cmdletName + Separator + parameterName))
                        _commandRuntime.WriteWarning(
                            string.Format(CultureInfo.InvariantCulture, ParameterBinderStrings.MultipleParametersMatched, parameterName));
                        _warningSet.Add(cmdletName + Separator + parameterName);
                if (matches.Count == 1)
                    if (!availablePairs.ContainsKey(matches[0]))
                        availablePairs.Add(matches[0], wildcard.Value);
                    if (!wildcard.Value.Equals(availablePairs[matches[0]]))
                                string.Format(CultureInfo.InvariantCulture, ParameterBinderStrings.DifferentValuesAssignedToSingleParameter, parameterName));
                        parametersToRemove.Add(matches[0]);
            if (keysToRemove.Count > 0)
                var keysInError = new StringBuilder();
                foreach (object badFormatKey in keysToRemove)
                    if (DefaultParameterValues.Contains(badFormatKey))
                        DefaultParameterValues.Remove(badFormatKey);
                    keysInError.Append(badFormatKey.ToString() + ", ");
                keysInError.Remove(keysInError.Length - 2, 2);
                var multipleKeys = keysToRemove.Count > 1;
                string formatString = multipleKeys
                                            ? ParameterBinderStrings.MultipleKeysInBadFormat
                                            : ParameterBinderStrings.SingleKeyInBadFormat;
                    string.Format(CultureInfo.InvariantCulture, formatString, keysInError));
            foreach (MergedCompiledCommandParameter param in parametersToRemove)
                availablePairs.Remove(param);
            if (availablePairs.Count > 0)
                return availablePairs;
        /// A helper method for GetDefaultParameterValuePairs.
        /// <param name="cmdletName"></param>
        /// <param name="paramValue"></param>
        /// <param name="bindableParameters"></param>
        /// <param name="bindableAlias"></param>
        /// <param name="parametersToRemove"></param>
        private void GetDefaultParameterValuePairsHelper(
            string cmdletName, string paramName, object paramValue,
            IDictionary<string, MergedCompiledCommandParameter> bindableParameters,
            IDictionary<string, MergedCompiledCommandParameter> bindableAlias,
            Dictionary<MergedCompiledCommandParameter, object> result,
            HashSet<MergedCompiledCommandParameter> parametersToRemove)
            // No exception should be thrown if we cannot find a match for the 'paramName',
            // because the 'paramName' could be a dynamic parameter name, and this dynamic parameter
            // hasn't been introduced at the current stage.
            bool writeWarning = false;
            MergedCompiledCommandParameter matchParameter;
            object resultObject;
            if (bindableParameters.TryGetValue(paramName, out matchParameter))
                if (!result.TryGetValue(matchParameter, out resultObject))
                    result.Add(matchParameter, paramValue);
                if (!paramValue.Equals(resultObject))
                    writeWarning = true;
                    parametersToRemove.Add(matchParameter);
                if (bindableAlias.TryGetValue(paramName, out matchParameter))
            if (writeWarning && !_warningSet.Contains(cmdletName + Separator + paramName))
                    string.Format(CultureInfo.InvariantCulture, ParameterBinderStrings.DifferentValuesAssignedToSingleParameter, paramName));
                _warningSet.Add(cmdletName + Separator + paramName);
        /// Verify if all arguments from the command line are bound.
        /// <param name="originalBindingException">
        /// Previous binding exceptions that possibly causes the failure
        private void VerifyArgumentsProcessed(ParameterBindingException originalBindingException)
            // Now verify that all the arguments that were passed in were processed.
            if (UnboundArguments.Count > 0)
                ParameterBindingException bindingException;
                CommandParameterInternal parameter = UnboundArguments[0];
                // Get the argument type that was specified
                Type specifiedType = null;
                object argumentValue = parameter.ArgumentValue;
                if (argumentValue != null && argumentValue != UnboundParameter.Value)
                    specifiedType = argumentValue.GetType();
                if (parameter.ParameterNameSpecified)
                    bindingException =
                        new ParameterBindingException(
                            this.Command.MyInvocation,
                            GetParameterErrorExtent(parameter),
                            parameter.ParameterName,
                            specifiedType,
                            ParameterBinderStrings.NamedParameterNotFound,
                            "NamedParameterNotFound");
                    // If this was a positional parameter, and we have the original exception,
                    // report on the original error
                    if (originalBindingException != null)
                        bindingException = originalBindingException;
                    // Otherwise, give a generic error.
                        string argument = StringLiterals.DollarNull;
                        if (parameter.ArgumentValue != null)
                                argument = parameter.ArgumentValue.ToString();
                                    new ParameterBindingArgumentTransformationException(
                                        this.InvocationInfo,
                                        parameter.ArgumentValue.GetType(),
                                        ParameterBinderStrings.ParameterArgumentTransformationErrorMessageOnly,
                                        "ParameterArgumentTransformationErrorMessageOnly",
                                    throw bindingException;
                                    ThrowElaboratedBindingException(bindingException);
                                argument,
                                ParameterBinderStrings.PositionalParameterNotFound,
                                "PositionalParameterNotFound");
        /// Verifies that a single parameter set is selected and throws an exception if
        /// one of there are multiple and one of them is not the default parameter set.
        private void VerifyParameterSetSelected()
            // Now verify that a parameter set has been selected if any parameter sets
            // were defined.
            if (this.BindableParameters.ParameterSetCount > 1)
                if (_currentParameterSetFlag == uint.MaxValue)
                    if ((_currentParameterSetFlag &
                         _commandMetadata.DefaultParameterSetFlag) != 0 &&
                         _commandMetadata.DefaultParameterSetFlag != uint.MaxValue)
                        ParameterBinderBase.bindingTracer.WriteLine(
                            "{0} valid parameter sets, using the DEFAULT PARAMETER SET: [{0}]",
                            this.BindableParameters.ParameterSetCount.ToString(),
                            _commandMetadata.DefaultParameterSetName);
                        _currentParameterSetFlag =
                            _commandMetadata.DefaultParameterSetFlag;
                        ParameterBinderBase.bindingTracer.TraceError(
                            "ERROR: {0} valid parameter sets, but NOT DEFAULT PARAMETER SET.",
                            this.BindableParameters.ParameterSetCount);
                        // Throw an exception for ambiguous parameter set
                        ThrowAmbiguousParameterSetException(_currentParameterSetFlag, BindableParameters);
        /// Restores the specified parameter to the original value.
        /// <param name="argumentToBind">
        /// The argument containing the value to restore.
        /// The metadata for the parameter to restore.
        /// True if the parameter was restored correctly, or false otherwise.
        private bool RestoreParameter(CommandParameterInternal argumentToBind, MergedCompiledCommandParameter parameter)
            switch (parameter.BinderAssociation)
                case ParameterBinderAssociation.DeclaredFormalParameters:
                    DefaultParameterBinder.BindParameter(argumentToBind.ParameterName, argumentToBind.ArgumentValue, parameter.Parameter);
                case ParameterBinderAssociation.CommonParameters:
                    CommonParametersBinder.BindParameter(argumentToBind.ParameterName, argumentToBind.ArgumentValue, parameter.Parameter);
                case ParameterBinderAssociation.ShouldProcessParameters:
                        _commandMetadata.SupportsShouldProcess,
                        "The metadata for the ShouldProcessParameters should only be available if the command supports ShouldProcess");
                    ShouldProcessParametersBinder.BindParameter(argumentToBind.ParameterName, argumentToBind.ArgumentValue, parameter.Parameter);
                case ParameterBinderAssociation.PagingParameters:
                        _commandMetadata.SupportsPaging,
                        "The metadata for the PagingParameters should only be available if the command supports paging");
                    PagingParametersBinder.BindParameter(argumentToBind.ParameterName, argumentToBind.ArgumentValue, parameter.Parameter);
                case ParameterBinderAssociation.TransactionParameters:
                        _commandMetadata.SupportsTransactions,
                        "The metadata for the TransactionParameters should only be available if the command supports Transactions");
                    TransactionParametersBinder.BindParameter(argumentToBind.ParameterName, argumentToBind.ArgumentValue, parameter.Parameter);
                case ParameterBinderAssociation.DynamicParameters:
                        _commandMetadata.ImplementsDynamicParameters,
                        "The metadata for the dynamic parameters should only be available if the command supports IDynamicParameters");
                    _dynamicParameterBinder?.BindParameter(argumentToBind.ParameterName, argumentToBind.ArgumentValue, parameter.Parameter);
        /// Validate the given named parameter against the specified parameter set,
        /// and then bind the argument to the parameter.
        protected override void BindNamedParameter(
            uint parameterSets,
            CommandParameterInternal argument,
            MergedCompiledCommandParameter parameter)
            if ((parameter.Parameter.ParameterSetFlags & parameterSets) == 0 &&
                !parameter.Parameter.IsInAllSets)
                string parameterSetName = BindableParameters.GetParameterSetName(parameterSets);
                ParameterBindingException bindingException =
                        errorPosition: null,
                        argument.ParameterName,
                        parameterType: null,
                        typeSpecified: null,
                        ParameterBinderStrings.ParameterNotInParameterSet,
                        "ParameterNotInParameterSet",
                // Might be caused by default parameter binding
                BindParameter(parameterSets, argument, parameter,
        /// Determines if a ScriptBlock can be bound directly to the type of the specified parameter.
        /// The metadata of the parameter to check the type of.
        /// true if the parameter type is Object, ScriptBlock, derived from ScriptBlock, a
        /// collection of ScriptBlocks, a collection of Objects, or a collection of types derived from
        /// ScriptBlock.
        private static bool IsParameterScriptBlockBindable(MergedCompiledCommandParameter parameter)
            Type parameterType = parameter.Parameter.Type;
            do // false loop
                if (parameterType == typeof(object))
                if (parameterType == typeof(ScriptBlock))
                if (parameterType.IsSubclassOf(typeof(ScriptBlock)))
                ParameterCollectionTypeInformation parameterCollectionTypeInfo = parameter.Parameter.CollectionTypeInformation;
                if (parameterCollectionTypeInfo.ParameterCollectionType != ParameterCollectionType.NotCollection)
                    if (parameterCollectionTypeInfo.ElementType == typeof(object))
                    if (parameterCollectionTypeInfo.ElementType == typeof(ScriptBlock))
                    if (parameterCollectionTypeInfo.ElementType.IsSubclassOf(typeof(ScriptBlock)))
            s_tracer.WriteLine("IsParameterScriptBlockBindable: result = {0}", result);
        /// Binds the specified argument to the specified parameter using the appropriate
        /// parameter binder. If the argument is of type ScriptBlock and the parameter takes
        /// pipeline input, then the ScriptBlock is saved off in the delay-bind ScriptBlock
        /// container for further processing of pipeline input and is not bound as the argument
        /// to the parameter.
        /// <param name="parameterSets">
        /// The parameter set used to bind the arguments.
        /// <param name="argument">
        /// The argument to be bound.
        /// The metadata for the parameter to bind the argument to.
        /// <param name="flags">
        /// Flags for type coercion, validation, and script block binding.
        /// ParameterBindingFlags.DelayBindScriptBlock:
        /// If set, arguments that are of type ScriptBlock where the parameter is not of type ScriptBlock,
        /// Object, or PSObject will be stored for execution during pipeline input and not bound as
        /// an argument to the parameter.
        /// True if the parameter was successfully bound. False if <paramref name="flags"/>
        /// has the flag <see cref="ParameterBindingFlags.ShouldCoerceType"/> set and the type does not match the parameter type.
        internal override bool BindParameter(
            MergedCompiledCommandParameter parameter,
            ParameterBindingFlags flags)
            // Now we need to check to see if the argument value is
            // a ScriptBlock.  If it is and the parameter type is
            // not ScriptBlock and not Object, then we need to delay
            // binding until a pipeline object is provided to invoke
            // the ScriptBlock.
            // Note: we haven't yet determined that only a single parameter
            // set is valid, so we have to take a best guess on pipeline input
            // based on the current valid parameter sets.
            bool continueWithBinding = true;
            if ((flags & ParameterBindingFlags.DelayBindScriptBlock) != 0 &&
                parameter.Parameter.DoesParameterSetTakePipelineInput(parameterSets) &&
                argument.ArgumentSpecified)
                object argumentValue = argument.ArgumentValue;
                if ((argumentValue is ScriptBlock || argumentValue is DelayedScriptBlockArgument) &&
                    !IsParameterScriptBlockBindable(parameter))
                    // Now check to see if the command expects to have pipeline input.
                    // If not, we should throw an exception now to inform the
                    // user with more information than they would get if it was
                    // considered an unbound mandatory parameter.
                    if (_commandRuntime.IsClosed && _commandRuntime.InputPipe.Empty)
                                    ErrorCategory.MetadataError,
                                    GetErrorExtent(argument),
                                    parameter.Parameter.Name,
                                    parameter.Parameter.Type,
                                    ParameterBinderStrings.ScriptBlockArgumentNoInput,
                                    "ScriptBlockArgumentNoInput");
                        "Adding ScriptBlock to delay-bind list for parameter '{0}'",
                        parameter.Parameter.Name);
                    // We need to delay binding of this argument to the parameter
                    DelayedScriptBlockArgument delayedArg = argumentValue as DelayedScriptBlockArgument ??
                                                            new DelayedScriptBlockArgument { _argument = argument, _parameterBinder = this };
                    if (!_delayBindScriptBlocks.ContainsKey(parameter))
                        _delayBindScriptBlocks.Add(parameter, delayedArg);
                    // We treat the parameter as bound, but really the
                    // script block gets run for each pipeline object and
                    // the result is bound.
                    if (parameter.Parameter.ParameterSetFlags != 0)
                        _currentParameterSetFlag &= parameter.Parameter.ParameterSetFlags;
                    UnboundParameters.Remove(parameter);
                    BoundParameters[parameter.Parameter.Name] = parameter;
                    BoundArguments[parameter.Parameter.Name] = argument;
                    if (DefaultParameterBinder.RecordBoundParameters &&
                        !DefaultParameterBinder.CommandLineParameters.ContainsKey(parameter.Parameter.Name))
                        DefaultParameterBinder.CommandLineParameters.Add(parameter.Parameter.Name, delayedArg);
                    continueWithBinding = false;
            if (continueWithBinding)
                    result = BindParameter(argument, parameter, flags);
                    bool rethrow = true;
                    if ((flags & ParameterBindingFlags.ShouldCoerceType) == 0)
                        // Attributes are used to do type coercion and result in various exceptions.
                        // We assume that if we aren't trying to do type coercion, we should avoid
                        // propagating type conversion exceptions.
                        while (e != null)
                            if (e is PSInvalidCastException)
                                rethrow = false;
                            e = e.InnerException;
                    if (rethrow)
        /// parameter binder.
        /// Flags for type coercion and validation.
        private bool BindParameter(
                        DefaultParameterBinder.BindParameter(
                            parameter.Parameter,
                            flags);
                        CommonParametersBinder.BindParameter(
                        ShouldProcessParametersBinder.BindParameter(
                        PagingParametersBinder.BindParameter(
                        "The metadata for the TransactionsParameters should only be available if the command supports transactions");
                        TransactionParametersBinder.BindParameter(
                    if (_dynamicParameterBinder != null)
                            _dynamicParameterBinder.BindParameter(
            if (result && ((flags & ParameterBindingFlags.IsDefaultValue) == 0))
                // Update the current valid parameter set flags
                if (!BoundParameters.ContainsKey(parameter.Parameter.Name))
                    BoundParameters.Add(parameter.Parameter.Name, parameter);
                if (!BoundArguments.ContainsKey(parameter.Parameter.Name))
                    BoundArguments.Add(parameter.Parameter.Name, argument);
                if (parameter.Parameter.ObsoleteAttribute != null &&
                    (flags & ParameterBindingFlags.IsDefaultValue) == 0 &&
                    !BoundObsoleteParameterNames.Contains(parameter.Parameter.Name))
                    string obsoleteWarning = string.Format(
                        ParameterBinderStrings.UseOfDeprecatedParameterWarning,
                        parameter.Parameter.ObsoleteAttribute.Message);
                    var warningRecord = new WarningRecord(ParameterBinderBase.FQIDParameterObsolete, obsoleteWarning);
                    BoundObsoleteParameterNames.Add(parameter.Parameter.Name);
                    ObsoleteParameterWarningList ??= new List<WarningRecord>();
                    ObsoleteParameterWarningList.Add(warningRecord);
        /// Binds the remaining arguments to an unbound ValueFromRemainingArguments parameter (Varargs)
        /// If there was an error binding the arguments to the parameters.
        private void HandleRemainingArguments()
                // Find the parameters that take the remaining args, if there are more
                // than one and the parameter set has not been defined, this is an error
                MergedCompiledCommandParameter varargsParameter = null;
                    ParameterSetSpecificMetadata parameterSetData = parameter.Parameter.GetParameterSetData(_currentParameterSetFlag);
                    if (parameterSetData == null)
                    // If the parameter takes the remaining arguments, bind them.
                    if (parameterSetData.ValueFromRemainingArguments)
                        if (varargsParameter != null)
                                        ParameterBinderStrings.AmbiguousParameterSet,
                                        "AmbiguousParameterSet");
                            // Might be caused by the default parameter binding
                        varargsParameter = parameter;
                        "BIND REMAININGARGUMENTS cmd line args to param: [{0}]",
                        varargsParameter.Parameter.Name))
                        // Accumulate the unbound arguments in to an list and then bind it to the parameter
                        List<object> valueFromRemainingArguments = new List<object>();
                        foreach (CommandParameterInternal argument in UnboundArguments)
                            if (argument.ParameterNameSpecified)
                                Diagnostics.Assert(!string.IsNullOrEmpty(argument.ParameterText), "Don't add a null argument");
                                valueFromRemainingArguments.Add(argument.ParameterText);
                            if (argument.ArgumentSpecified)
                                if (argumentValue != AutomationNull.Value && argumentValue != UnboundParameter.Value)
                                    valueFromRemainingArguments.Add(argumentValue);
                        // If there are multiple arguments, it's not clear how best to represent the extent as the extent
                        // may be disjoint, as in 'echo a -verbose b', we have 'a' and 'b' in UnboundArguments.
                        var argumentAst = UnboundArguments.Count == 1 ? UnboundArguments[0].ArgumentAst : null;
                        var cpi = CommandParameterInternal.CreateParameterWithArgument(
                            /*parameterAst*/null, varargsParameter.Parameter.Name, "-" + varargsParameter.Parameter.Name + ":",
                            argumentAst, valueFromRemainingArguments, false);
                        // To make all of the following work similarly (the first is handled elsewhere, but second and third are
                        // handled here):
                        //     Set-ClusterOwnerNode -Owners foo,bar
                        //     Set-ClusterOwnerNode foo bar
                        //     Set-ClusterOwnerNode foo,bar
                        // we unwrap our List, but only if there is a single argument which is a collection.
                        if (valueFromRemainingArguments.Count == 1 && LanguagePrimitives.IsObjectEnumerable(valueFromRemainingArguments[0]))
                            cpi.SetArgumentValue(UnboundArguments[0].ArgumentAst, valueFromRemainingArguments[0]);
                            BindParameter(cpi, varargsParameter, ParameterBindingFlags.ShouldCoerceType);
                        UnboundArguments.Clear();
        /// Determines if the cmdlet supports dynamic parameters. If it does,
        /// the dynamic parameter bindable object is retrieved and the unbound
        /// arguments are bound to it.
        /// <param name="outgoingBindingException">
        /// Returns the underlying parameter binding exception if any was generated.
        /// If there was an error compiling the parameter metadata.
        private void HandleCommandLineDynamicParameters(out ParameterBindingException outgoingBindingException)
            outgoingBindingException = null;
            if (_commandMetadata.ImplementsDynamicParameters)
                    "BIND cmd line args to DYNAMIC parameters."))
                    s_tracer.WriteLine("The Cmdlet supports the dynamic parameter interface");
                    IDynamicParameters dynamicParameterCmdlet = this.Command as IDynamicParameters;
                    if (dynamicParameterCmdlet != null)
                        if (_dynamicParameterBinder == null)
                            s_tracer.WriteLine("Getting the bindable object from the Cmdlet");
                            // Now get the dynamic parameter bindable object.
                            object dynamicParamBindableObject;
                                dynamicParamBindableObject = dynamicParameterCmdlet.GetDynamicParameters();
                            catch (Exception e) // Catch-all OK, this is a third-party callout
                                if (e is ProviderInvocationException)
                                        ParameterBinderStrings.GetDynamicParametersException,
                                        "GetDynamicParametersException",
                                // This exception is caused because failure happens when retrieving the dynamic parameters,
                                // this is not caused by introducing the default parameter binding.
                            if (dynamicParamBindableObject != null)
                                    "DYNAMIC parameter object: [{0}]",
                                    dynamicParamBindableObject.GetType());
                                s_tracer.WriteLine("Creating a new parameter binder for the dynamic parameter object");
                                InternalParameterMetadata dynamicParameterMetadata;
                                RuntimeDefinedParameterDictionary runtimeParamDictionary = dynamicParamBindableObject as RuntimeDefinedParameterDictionary;
                                if (runtimeParamDictionary != null)
                                    // Generate the type metadata for the runtime-defined parameters
                                    dynamicParameterMetadata =
                                        InternalParameterMetadata.Get(runtimeParamDictionary, true, true);
                                    _dynamicParameterBinder =
                                        new RuntimeDefinedParameterBinder(
                                            runtimeParamDictionary,
                                            this.Command,
                                            this.CommandLineParameters);
                                    // Generate the type metadata or retrieve it from the cache
                                        InternalParameterMetadata.Get(dynamicParamBindableObject.GetType(), Context, true);
                                    // Create the parameter binder for the dynamic parameter object
                                        new ReflectionParameterBinder(
                                            dynamicParamBindableObject,
                                // Now merge the metadata with other metadata for the command
                                var dynamicParams =
                                    BindableParameters.AddMetadataForBinder(
                                        dynamicParameterMetadata,
                                        ParameterBinderAssociation.DynamicParameters);
                                foreach (var param in dynamicParams)
                                    UnboundParameters.Add(param);
                                // Now set the parameter set flags for the new type metadata.
                                _commandMetadata.DefaultParameterSetFlag =
                                    this.BindableParameters.GenerateParameterSetMappingFromMetadata(_commandMetadata.DefaultParameterSetName);
                            s_tracer.WriteLine("No dynamic parameter object was returned from the Cmdlet");
                                    "BIND NAMED args to DYNAMIC parameters"))
                                // Try to bind the unbound arguments as static parameters to the
                                // dynamic parameter object.
                                UnboundArguments = BindNamedParameters(_currentParameterSetFlag, UnboundArguments);
                                    "BIND POSITIONAL args to DYNAMIC parameters"))
                                    _commandMetadata.DefaultParameterSetFlag,
                                    out outgoingBindingException);
        /// This method determines if the unbound mandatory parameters take pipeline input or
        /// if we can use the default parameter set.  If all the unbound mandatory parameters
        /// take pipeline input and the default parameter set is valid, then the default parameter
        /// set is set as the current parameter set and processing can continue.  If there are
        /// more than one valid parameter sets and the unbound mandatory parameters are not
        /// consistent across parameter sets or there is no default parameter set then a
        /// ParameterBindingException is thrown with an errorId of AmbiguousParameterSet.
        /// <param name="validParameterSetCount">
        /// The number of valid parameter sets.
        /// <param name="isPipelineInputExpected">
        /// True if the pipeline is open to receive input.
        /// If there are multiple valid parameter sets and the missing mandatory parameters are
        /// not consistent across parameter sets, or there is no default parameter set.
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode", Justification = "Consider Simplifying it.")]
        private Collection<MergedCompiledCommandParameter> GetMissingMandatoryParameters(
            int validParameterSetCount,
            bool isPipelineInputExpected)
            Collection<MergedCompiledCommandParameter> result = new Collection<MergedCompiledCommandParameter>();
            uint defaultParameterSet = _commandMetadata.DefaultParameterSetFlag;
            uint commandMandatorySets = 0;
            Dictionary<uint, ParameterSetPromptingData> promptingData = new Dictionary<uint, ParameterSetPromptingData>();
            bool missingAMandatoryParameter = false;
            bool missingAMandatoryParameterInAllSet = false;
            // See if any of the unbound parameters are mandatory
                // If a parameter is never mandatory, we can skip lots of work here.
                if (!parameter.Parameter.IsMandatoryInSomeParameterSet)
                var matchingParameterSetMetadata = parameter.Parameter.GetMatchingParameterSetData(_currentParameterSetFlag);
                uint parameterMandatorySets = 0;
                bool thisParameterMissing = false;
                    uint newMandatoryParameterSetFlag = NewParameterSetPromptingData(promptingData, parameter, parameterSetMetadata, defaultParameterSet, isPipelineInputExpected);
                    if (newMandatoryParameterSetFlag != 0)
                        missingAMandatoryParameter = true;
                        thisParameterMissing = true;
                        if (newMandatoryParameterSetFlag != uint.MaxValue)
                            parameterMandatorySets |= (_currentParameterSetFlag & newMandatoryParameterSetFlag);
                            commandMandatorySets |= (_currentParameterSetFlag & parameterMandatorySets);
                            missingAMandatoryParameterInAllSet = true;
                // We are not expecting pipeline input
                    // The parameter is mandatory so we need to prompt for it
                    if (thisParameterMissing)
                        result.Add(parameter);
                    // The parameter was not mandatory in any parameter set
            if (missingAMandatoryParameter && isPipelineInputExpected)
                if (commandMandatorySets == 0)
                    commandMandatorySets = _currentParameterSetFlag;
                if (missingAMandatoryParameterInAllSet)
                    uint availableParameterSetFlags = this.BindableParameters.AllParameterSetFlags;
                    if (availableParameterSetFlags == 0)
                        availableParameterSetFlags = uint.MaxValue;
                    commandMandatorySets = (_currentParameterSetFlag & availableParameterSetFlags);
                // First we need to see if there are multiple valid parameter sets, and if one is
                // the default parameter set, and it is not missing any mandatory parameters, then
                // use the default parameter set.
                if (validParameterSetCount > 1 &&
                    defaultParameterSet != 0 &&
                    (defaultParameterSet & commandMandatorySets) == 0 &&
                    (defaultParameterSet & _currentParameterSetFlag) != 0)
                    // If no other set takes pipeline input, then latch on to the default set
                    uint setThatTakesPipelineInput = 0;
                    foreach (ParameterSetPromptingData promptingSetData in promptingData.Values)
                        if ((promptingSetData.ParameterSet & _currentParameterSetFlag) != 0 &&
                            (promptingSetData.ParameterSet & defaultParameterSet) == 0 &&
                            !promptingSetData.IsAllSet)
                            if (promptingSetData.PipelineableMandatoryParameters.Count > 0)
                                setThatTakesPipelineInput = promptingSetData.ParameterSet;
                    if (setThatTakesPipelineInput == 0)
                        // Old algorithm starts
                        // // latch on to the default parameter set
                        // commandMandatorySets = defaultParameterSet;
                        // _currentParameterSetFlag = defaultParameterSet;
                        // Command.SetParameterSetName(CurrentParameterSetName);
                        // Old algorithm ends
                        // At this point, we have the following information:
                        //  1. There are unbound mandatory parameter(s)
                        //  2. No unbound mandatory parameter is in AllSet
                        //  3. All unbound mandatory parameters don't take pipeline input
                        //  4. Default parameter set is valid
                        //  5. Default parameter set doesn't contain unbound mandatory parameters
                        // We ignore those parameter sets that contain unbound mandatory parameters, but leave
                        // all other parameter sets remain valid. The other parameter sets contains the default
                        // parameter set and have one characteristic: NONE of them contain unbound mandatory parameters
                        // Comparing to the old algorithm, we keep more possible parameter sets here, but
                        // we need to prioritize the default parameter set for pipeline binding, so as NOT to
                        // make breaking changes. This is to handle the following scenario:
                        //                               Old Algorithm              New Algorithm (without prioritizing default)      New Algorithm (with prioritizing default)
                        //  Remaining Parameter Sets       A(default)               A(default), B                                     A(default), B
                        //        Pipeline parameter       P1(string)               A: P1(string); B: P2(System.DateTime)             A: P1(string); B: P2(System.DateTime)
                        //   Pipeline parameter type       P1:By Value              P1:By Value; P2:By Value                          P1:By Value; P2:By Value
                        //            Pipeline input       $a (System.DateTime)     $a (System.DateTime)                              $a (System.DateTime)
                        //   Pipeline binding result       P1 --> $a.ToString()     P2 --> $a                                         P1 --> $a.ToString()
                        //     Pipeline binding type       ByValueWithCoercion      ByValueWithoutCoercion                            ByValueWithCoercion
                        commandMandatorySets = _currentParameterSetFlag & (~commandMandatorySets);
                        _currentParameterSetFlag = commandMandatorySets;
                        if (_currentParameterSetFlag == defaultParameterSet)
                            Command.SetParameterSetName(CurrentParameterSetName);
                            _parameterSetToBePrioritizedInPipelineBinding = defaultParameterSet;
                // We need to analyze the prompting data that was gathered to determine what parameter
                // set to use, which parameters need prompting for, and which parameters take pipeline input.
                int commandMandatorySetsCount = ValidParameterSetCount(commandMandatorySets);
                if (commandMandatorySetsCount == 0)
                else if (commandMandatorySetsCount == 1)
                    // Since we have only one valid parameter set, add all
                        if ((promptingSetData.ParameterSet & commandMandatorySets) != 0 ||
                            promptingSetData.IsAllSet)
                            foreach (MergedCompiledCommandParameter mandatoryParameter in promptingSetData.NonpipelineableMandatoryParameters.Keys)
                                result.Add(mandatoryParameter);
                else if (_parameterSetToBePrioritizedInPipelineBinding == 0)
                    // We have more than one valid parameter set.  Need to figure out which one to
                    // use.
                    // First we need to process the default parameter set if it can fill its parameters
                    // from the pipeline.
                    bool latchOnToDefault = false;
                    if (defaultParameterSet != 0 && (commandMandatorySets & defaultParameterSet) != 0)
                        // Determine if another set could be satisfied by pipeline input - that is, it
                        // has mandatory pipeline input parameters but no mandatory command-line only parameters.
                        bool anotherSetTakesPipelineInput = false;
                        foreach (ParameterSetPromptingData paramPromptingData in promptingData.Values)
                            if (!paramPromptingData.IsAllSet &&
                                !paramPromptingData.IsDefaultSet &&
                                paramPromptingData.PipelineableMandatoryParameters.Count > 0 &&
                                paramPromptingData.NonpipelineableMandatoryParameters.Count == 0)
                                anotherSetTakesPipelineInput = true;
                        // Determine if another set takes pipeline input by property name
                        bool anotherSetTakesPipelineInputByPropertyName = false;
                                paramPromptingData.PipelineableMandatoryByPropertyNameParameters.Count > 0)
                                anotherSetTakesPipelineInputByPropertyName = true;
                        // See if we should pick the default set if it can bind strongly to the incoming objects
                        ParameterSetPromptingData defaultSetPromptingData;
                        if (promptingData.TryGetValue(defaultParameterSet, out defaultSetPromptingData))
                            bool defaultSetTakesPipelineInput = defaultSetPromptingData.PipelineableMandatoryParameters.Count > 0;
                            bool defaultSetTakesPipelineInputByPropertyName = defaultSetPromptingData.PipelineableMandatoryByPropertyNameParameters.Count > 0;
                            if (defaultSetTakesPipelineInputByPropertyName && !anotherSetTakesPipelineInputByPropertyName)
                                latchOnToDefault = true;
                            else if (defaultSetTakesPipelineInput && !anotherSetTakesPipelineInput)
                        if (!latchOnToDefault)
                            // If only the all set takes pipeline input then latch on to the
                            // default set
                            if (!anotherSetTakesPipelineInput)
                            // Need to see if there are nonpipelineable mandatory parameters in the
                            // all set.
                            ParameterSetPromptingData allSetPromptingData;
                            if (promptingData.TryGetValue(uint.MaxValue, out allSetPromptingData))
                                if (allSetPromptingData.NonpipelineableMandatoryParameters.Count > 0)
                        if (latchOnToDefault)
                            // latch on to the default parameter set
                            commandMandatorySets = defaultParameterSet;
                            _currentParameterSetFlag = defaultParameterSet;
                            // Add all missing mandatory parameters that don't take pipeline input
                        // When we select a mandatory set to latch on, we should try to preserve other parameter sets that contain no mandatory parameters or contain only common mandatory parameters
                        // as much as possible, so as to support the binding for the following scenarios:
                        // (1) Scenario 1:
                        // Valid parameter sets when it comes to the mandatory checking: A, B
                        // Mandatory parameters in A, B:
                        // Set      Nonpipelineable-Mandatory-InSet         Pipelineable-Mandatory-InSet       Common-Nonpipelineable-Mandatory       Common-Pipelineable-Mandatory
                        // A        N/A                                     N/A                                N/A                                    AllParam (of type DateTime)
                        // B        N/A                                     ParamB (of type TimeSpan)          N/A                                    AllParam (of type DateTime)
                        // Piped-in object: Get-Date
                        // (2) Scenario 2:
                        // Valid parameter sets when it comes to the mandatory checking: A, B, C, Default
                        // Mandatory parameters in A, B, C and Default:
                        // C        N/A                                     N/A                                N/A                                    AllParam (of type DateTime)
                        // Default  N/A                                     N/A                                N/A                                    AllParam (of type DateTime)
                        // Before the fix, the mandatory checking will resolve the parameter set to be B in both scenario 1 and 2, which will fail in the subsequent pipeline binding.
                        // After the fix, the parameter set "A" in the scenario 1 and the set "A", "C", "Default" in the scenario 2 will be preserved, and the subsequent pipeline binding will succeed.
                        // (3) Scenario 3:
                        // Valid parameter sets when it comes to the mandatory checking: A, B, C
                        // Mandatory parameters in A, B and C:
                        // Set      Nonpipelineable-Mandatory-InSet         Pipelineable-Mandatory-InSet       Pipelineable-Nonmandatory-InSet       Common-Nonpipelineable-Mandatory       Common-Pipelineable-Mandatory       Common-Pipelineable-Nonmandatory
                        // A        N/A                                     ParamA (of type TimeSpan)          N/A                                   N/A                                    N/A                                 N/A
                        // B        ParamB-1                                N/A                                ParamB-2 (of type string[])           N/A                                    N/A                                 N/A
                        // C        N/A                                     N/A                                ParamC (of type DateTime)             N/A                                    N/A                                 N/A
                        // (4) Scenario 4:
                        // A        N/A                                     ParamA (of type TimeSpan)          N/A                                   N/A                                    N/A                                 AllParam (of type DateTime)
                        // B        ParamB-1                                N/A                                ParamB-2 (of type string[])           N/A                                    N/A                                 AllParam (of type DateTime)
                        // C        N/A                                     N/A                                N/A                                   N/A                                    N/A                                 AllParam (of type DateTime)
                        // Default  N/A                                     N/A                                N/A                                   N/A                                    N/A                                 AllParam (of type DateTime)
                        // Before the fix, the mandatory checking will resolve the parameter set to be A in both scenario 3 and 4, which will fail in the subsequent pipeline binding.
                        // After the fix, the parameter set "C" in the scenario 1 and the set "C" and "Default" in the scenario 2 will be preserved, and the subsequent pipeline binding will succeed.
                        // Examples:
                        // (1) Scenario 1
                        // Function Get-Cmdlet
                        //       [CmdletBinding()]
                        //       param(
                        //          [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
                        //          [System.DateTime]
                        //          $Date,
                        //          [Parameter(ParameterSetName="computer")]
                        //          [Parameter(ParameterSetName="session")]
                        //          $ComputerName,
                        //          [Parameter(ParameterSetName="session", Mandatory=$true, ValueFromPipeline=$true)]
                        //          [System.TimeSpan]
                        //          $TimeSpan
                        //       )
                        //      Process
                        //      {
                        //         Write-Output $PsCmdlet.ParameterSetName
                        //      }
                        // PS:\> Get-Date | Get-Cmdlet
                        // PS:\> computer
                        // (2) Scenario 2
                        //       [CmdletBinding(DefaultParameterSetName="computer")]
                        //          [Parameter(ParameterSetName="new")]
                        //          $NewName,
                        // (3) Scenario 3
                        //        [CmdletBinding()]
                        //        param(
                        //           [Parameter(ParameterSetName="network", Mandatory=$true, ValueFromPipeline=$true)]
                        //           [TimeSpan]
                        //           $network,
                        //           [Parameter(ParameterSetName="computer", ValueFromPipelineByPropertyName=$true)]
                        //           [string[]]
                        //           $ComputerName,
                        //           [Parameter(ParameterSetName="computer", Mandatory=$true)]
                        //           [switch]
                        //           $DisableComputer,
                        //           [Parameter(ParameterSetName="session", ValueFromPipeline=$true)]
                        //           [DateTime]
                        //           $Date
                        //        )
                        //        Process
                        //           Write-Output $PsCmdlet.ParameterSetName
                        // PS:\> session
                        // (4) Scenario 4
                        //       [CmdletBinding(DefaultParameterSetName="server")]
                        //          [Parameter(ParameterSetName="network", Mandatory=$true, ValueFromPipeline=$true)]
                        //          [TimeSpan]
                        //          $network,
                        //          [Parameter(ParameterSetName="computer", ValueFromPipelineByPropertyName=$true)]
                        //          [string[]]
                        //          [Parameter(ParameterSetName="computer", Mandatory=$true)]
                        //          [switch]
                        //          $DisableComputer,
                        //          [Parameter(ParameterSetName="server")]
                        //          [string]
                        //          $Param,
                        //          [Parameter(ValueFromPipeline=$true)]
                        //          [DateTime]
                        //          $Date
                        // PS:\> server
                        uint setThatTakesPipelineInputByValue = 0;
                        uint setThatTakesPipelineInputByPropertyName = 0;
                        // Find the single set that takes pipeline input by value
                        bool foundSetThatTakesPipelineInputByValue = false;
                        bool foundMultipleSetsThatTakesPipelineInputByValue = false;
                            if ((promptingSetData.ParameterSet & commandMandatorySets) != 0 &&
                                if (promptingSetData.PipelineableMandatoryByValueParameters.Count > 0)
                                    if (foundSetThatTakesPipelineInputByValue)
                                        foundMultipleSetsThatTakesPipelineInputByValue = true;
                                        setThatTakesPipelineInputByValue = 0;
                                    setThatTakesPipelineInputByValue = promptingSetData.ParameterSet;
                                    foundSetThatTakesPipelineInputByValue = true;
                        // Find the single set that takes pipeline input by property name
                        bool foundSetThatTakesPipelineInputByPropertyName = false;
                        bool foundMultipleSetsThatTakesPipelineInputByPropertyName = false;
                                if (promptingSetData.PipelineableMandatoryByPropertyNameParameters.Count > 0)
                                    if (foundSetThatTakesPipelineInputByPropertyName)
                                        foundMultipleSetsThatTakesPipelineInputByPropertyName = true;
                                        setThatTakesPipelineInputByPropertyName = 0;
                                    setThatTakesPipelineInputByPropertyName = promptingSetData.ParameterSet;
                                    foundSetThatTakesPipelineInputByPropertyName = true;
                        // If we have one or the other, we can latch onto that set without difficulty
                        uint uniqueSetThatTakesPipelineInput = 0;
                        if (foundSetThatTakesPipelineInputByValue && foundSetThatTakesPipelineInputByPropertyName &&
                            (setThatTakesPipelineInputByValue == setThatTakesPipelineInputByPropertyName))
                            uniqueSetThatTakesPipelineInput = setThatTakesPipelineInputByValue;
                        if (foundSetThatTakesPipelineInputByValue ^ foundSetThatTakesPipelineInputByPropertyName)
                            uniqueSetThatTakesPipelineInput = foundSetThatTakesPipelineInputByValue ?
                                setThatTakesPipelineInputByValue : setThatTakesPipelineInputByPropertyName;
                        if (uniqueSetThatTakesPipelineInput != 0)
                            // latch on to the set that takes pipeline input
                            commandMandatorySets = uniqueSetThatTakesPipelineInput;
                            uint otherMandatorySetsToBeIgnored = 0;
                            bool chosenMandatorySetContainsNonpipelineableMandatoryParameters = false;
                                    if (!promptingSetData.IsAllSet)
                                        chosenMandatorySetContainsNonpipelineableMandatoryParameters =
                                            promptingSetData.NonpipelineableMandatoryParameters.Count > 0;
                                    otherMandatorySetsToBeIgnored |= promptingSetData.ParameterSet;
                            // Preserve potential parameter sets as much as possible
                            PreservePotentialParameterSets(uniqueSetThatTakesPipelineInput,
                                                           otherMandatorySetsToBeIgnored,
                                                           chosenMandatorySetContainsNonpipelineableMandatoryParameters);
                            // Now if any valid parameter sets have nonpipelineable mandatory parameters we have
                            // an error
                            bool foundMissingParameters = false;
                            uint setsThatContainNonpipelineableMandatoryParameter = 0;
                                    if (promptingSetData.NonpipelineableMandatoryParameters.Count > 0)
                                        foundMissingParameters = true;
                                            setsThatContainNonpipelineableMandatoryParameter |= promptingSetData.ParameterSet;
                            if (foundMissingParameters)
                                // As a last-ditch effort, bind to the set that takes pipeline input by value
                                if (setThatTakesPipelineInputByValue != 0)
                                    commandMandatorySets = setThatTakesPipelineInputByValue;
                                    PreservePotentialParameterSets(setThatTakesPipelineInputByValue,
                                    if ((!foundMultipleSetsThatTakesPipelineInputByValue) &&
                                       (!foundMultipleSetsThatTakesPipelineInputByPropertyName))
                                    // Remove the data set that contains non-pipelineable mandatory parameters, since we are not
                                    // prompting for them and they will not be bound later.
                                    // If no data set left, throw ambiguous parameter set exception
                                    if (setsThatContainNonpipelineableMandatoryParameter != 0)
                                        IgnoreOtherMandatoryParameterSets(setsThatContainNonpipelineableMandatoryParameter);
                                        if (_currentParameterSetFlag == 0)
                                        if (ValidParameterSetCount(_currentParameterSetFlag) == 1)
        /// Preserve potential parameter sets as much as possible.
        /// <param name="chosenMandatorySet">The mandatory set we choose to latch on.</param>
        /// <param name="otherMandatorySetsToBeIgnored">Other mandatory parameter sets to be ignored.</param>
        /// <param name="chosenSetContainsNonpipelineableMandatoryParameters">Indicate if the chosen mandatory set contains any non-pipelineable mandatory parameters.</param>
        private void PreservePotentialParameterSets(uint chosenMandatorySet, uint otherMandatorySetsToBeIgnored, bool chosenSetContainsNonpipelineableMandatoryParameters)
            // If the chosen set contains nonpipelineable mandatory parameters, then we set it as the only valid parameter set since we will prompt for those mandatory parameters
            if (chosenSetContainsNonpipelineableMandatoryParameters)
                _currentParameterSetFlag = chosenMandatorySet;
                // Otherwise, we additionally preserve those valid parameter sets that contain no mandatory parameter, or contain only the common mandatory parameters
                IgnoreOtherMandatoryParameterSets(otherMandatorySetsToBeIgnored);
                if (_currentParameterSetFlag != chosenMandatorySet)
                    _parameterSetToBePrioritizedInPipelineBinding = chosenMandatorySet;
        /// Update _currentParameterSetFlag to ignore the specified mandatory sets.
        /// This method is used only when we try to preserve parameter sets during the mandatory parameter checking.
        /// In cases where this method is used, there must be at least one parameter set declared.
        /// <param name="otherMandatorySetsToBeIgnored">The mandatory parameter sets to be ignored.</param>
        private void IgnoreOtherMandatoryParameterSets(uint otherMandatorySetsToBeIgnored)
            if (otherMandatorySetsToBeIgnored == 0)
                // We cannot update the _currentParameterSetFlag to remove some parameter sets directly when it's AllSet as that will get it to an incorrect state.
                uint availableParameterSets = this.BindableParameters.AllParameterSetFlags;
                Diagnostics.Assert(availableParameterSets != 0, "At least one parameter set must be declared");
                _currentParameterSetFlag = availableParameterSets & (~otherMandatorySetsToBeIgnored);
                _currentParameterSetFlag &= (~otherMandatorySetsToBeIgnored);
        private static uint NewParameterSetPromptingData(
            Dictionary<uint, ParameterSetPromptingData> promptingData,
            ParameterSetSpecificMetadata parameterSetMetadata,
            uint defaultParameterSet,
            bool pipelineInputExpected)
            uint parameterSetFlag = parameterSetMetadata.ParameterSetFlag;
            if (parameterSetFlag == 0)
                parameterSetFlag = uint.MaxValue;
            bool isDefaultSet = (defaultParameterSet != 0) && ((defaultParameterSet & parameterSetFlag) != 0);
            bool isMandatory = false;
            if (parameterSetMetadata.IsMandatory)
                parameterMandatorySets |= parameterSetFlag;
                isMandatory = true;
            bool isPipelineable = false;
            if (pipelineInputExpected)
                    isPipelineable = true;
            if (isMandatory)
                ParameterSetPromptingData promptingDataForSet;
                if (!promptingData.TryGetValue(parameterSetFlag, out promptingDataForSet))
                    promptingDataForSet = new ParameterSetPromptingData(parameterSetFlag, isDefaultSet);
                    promptingData.Add(parameterSetFlag, promptingDataForSet);
                if (isPipelineable)
                    promptingDataForSet.PipelineableMandatoryParameters[parameter] = parameterSetMetadata;
                    if (parameterSetMetadata.ValueFromPipeline)
                        promptingDataForSet.PipelineableMandatoryByValueParameters[parameter] = parameterSetMetadata;
                    if (parameterSetMetadata.ValueFromPipelineByPropertyName)
                        promptingDataForSet.PipelineableMandatoryByPropertyNameParameters[parameter] = parameterSetMetadata;
                    promptingDataForSet.NonpipelineableMandatoryParameters[parameter] = parameterSetMetadata;
            return parameterMandatorySets;
        /// Ensures that only one parameter set is valid or throws an appropriate exception.
        /// <param name="prePipelineInput">
        /// If true, it is acceptable to have multiple valid parameter sets as long as one
        /// of those parameter sets take pipeline input.
        /// <param name="setDefault">
        /// If true, the default parameter set will be selected if there is more than
        /// one valid parameter set and one is the default set.
        /// If false, the count of valid parameter sets will be returned but no error
        /// will occur and the default parameter set will not be used.
        /// If the more than one or zero parameter sets were resolved from the named
        /// parameters.
        private int ValidateParameterSets(bool prePipelineInput, bool setDefault)
            // Compute how many parameter sets are still valid
            int validParameterSetCount = ValidParameterSetCount(_currentParameterSetFlag);
            if (validParameterSetCount == 0 && _currentParameterSetFlag != uint.MaxValue)
            else if (validParameterSetCount > 1)
                uint defaultParameterSetFlag = _commandMetadata.DefaultParameterSetFlag;
                bool hasDefaultSetDefined = defaultParameterSetFlag != 0;
                bool validSetIsAllSet = _currentParameterSetFlag == uint.MaxValue;
                bool validSetIsDefault = _currentParameterSetFlag == defaultParameterSetFlag;
                // If no default parameter set is defined and the valid set is the "all" set
                // then use the all set.
                if (validSetIsAllSet && !hasDefaultSetDefined)
                    // The current parameter set flags are valid.
                    // Note: this is the same as having a single valid parameter set flag.
                    validParameterSetCount = 1;
                // If the valid parameter set is the default parameter set, or if the default
                // parameter set has been defined and one of the valid parameter sets is
                // the default parameter set, then use the default parameter set.
                else if (!prePipelineInput &&
                    validSetIsDefault ||
                    (hasDefaultSetDefined && (_currentParameterSetFlag & defaultParameterSetFlag) != 0))
                    // Set currentParameterSetName regardless of setDefault
                    string currentParameterSetName = BindableParameters.GetParameterSetName(defaultParameterSetFlag);
                    Command.SetParameterSetName(currentParameterSetName);
                    if (setDefault)
                        _currentParameterSetFlag = _commandMetadata.DefaultParameterSetFlag;
                // There are multiple valid parameter sets but at least one parameter set takes
                // pipeline input
                else if (prePipelineInput &&
                    AtLeastOneUnboundValidParameterSetTakesPipelineInput(_currentParameterSetFlag))
                    // We haven't fixated on a valid parameter set yet, but will wait for pipeline input to
                    // determine which parameter set to use.
                    int resolvedParameterSetCount = ResolveParameterSetAmbiguityBasedOnMandatoryParameters();
                    if (resolvedParameterSetCount != 1)
                    validParameterSetCount = resolvedParameterSetCount;
            else // validParameterSetCount == 1
                // If the valid parameter set is the "all" set, and a default set was defined,
                // then set the current parameter set to the default set.
                    // Since this is the "all" set, default the parameter set count to the
                    // number of parameter sets that were defined for the cmdlet or 1 if
                    // none were defined.
                    validParameterSetCount =
                        (this.BindableParameters.ParameterSetCount > 0) ?
                            this.BindableParameters.ParameterSetCount : 1;
                    if (prePipelineInput &&
                        // Don't fixate on the default parameter set yet. Wait until after
                        // we have processed pipeline input.
                    else if (_commandMetadata.DefaultParameterSetFlag != 0)
            return validParameterSetCount;
        private int ResolveParameterSetAmbiguityBasedOnMandatoryParameters()
            return ResolveParameterSetAmbiguityBasedOnMandatoryParameters(this.BoundParameters, this.UnboundParameters, this.BindableParameters, ref _currentParameterSetFlag, Command);
        internal static int ResolveParameterSetAmbiguityBasedOnMandatoryParameters(
            Dictionary<string, MergedCompiledCommandParameter> boundParameters,
            ICollection<MergedCompiledCommandParameter> unboundParameters,
            MergedCommandParameterMetadata bindableParameters,
            ref uint _currentParameterSetFlag,
            Cmdlet command
            uint remainingParameterSetsWithNoMandatoryUnboundParameters = _currentParameterSetFlag;
            IEnumerable<ParameterSetSpecificMetadata> allParameterSetMetadatas = boundParameters.Values
                .Concat(unboundParameters)
                .SelectMany(static p => p.Parameter.ParameterSetData.Values);
            uint allParameterSetFlags = 0;
            foreach (ParameterSetSpecificMetadata parameterSetMetadata in allParameterSetMetadatas)
                allParameterSetFlags |= parameterSetMetadata.ParameterSetFlag;
            remainingParameterSetsWithNoMandatoryUnboundParameters &= allParameterSetFlags;
                ValidParameterSetCount(remainingParameterSetsWithNoMandatoryUnboundParameters) > 1,
                "This method should only be called when there is an ambiguity wrt parameter sets");
            IEnumerable<ParameterSetSpecificMetadata> parameterSetMetadatasForUnboundMandatoryParameters = unboundParameters
                .SelectMany(static p => p.Parameter.ParameterSetData.Values)
                .Where(static p => p.IsMandatory);
            foreach (ParameterSetSpecificMetadata parameterSetMetadata in parameterSetMetadatasForUnboundMandatoryParameters)
                remainingParameterSetsWithNoMandatoryUnboundParameters &= (~parameterSetMetadata.ParameterSetFlag);
            int finalParameterSetCount = ValidParameterSetCount(remainingParameterSetsWithNoMandatoryUnboundParameters);
            if (finalParameterSetCount == 1)
                _currentParameterSetFlag = remainingParameterSetsWithNoMandatoryUnboundParameters;
                    string currentParameterSetName = bindableParameters.GetParameterSetName(_currentParameterSetFlag);
                    command.SetParameterSetName(currentParameterSetName);
                return finalParameterSetCount;
        private void ThrowAmbiguousParameterSetException(uint parameterSetFlags, MergedCommandParameterMetadata bindableParameters)
            // Trace the parameter sets still active
            uint currentParameterSet = 1;
            while (parameterSetFlags != 0)
                uint currentParameterSetActive = parameterSetFlags & 0x1;
                if (currentParameterSetActive == 1)
                    string parameterSetName = bindableParameters.GetParameterSetName(currentParameterSet);
                    if (!string.IsNullOrEmpty(parameterSetName))
                        ParameterBinderBase.bindingTracer.WriteLine("Remaining valid parameter set: {0}", parameterSetName);
                parameterSetFlags >>= 1;
                currentParameterSet <<= 1;
        /// Determines if there are any unbound parameters that take pipeline input
        /// for the specified parameter sets.
        /// <param name="validParameterSetFlags">
        /// The parameter sets that should be checked for each unbound parameter to see
        /// if it accepts pipeline input.
        /// True if there is at least one parameter that takes pipeline input for the
        /// specified parameter sets, or false otherwise.
        private bool AtLeastOneUnboundValidParameterSetTakesPipelineInput(uint validParameterSetFlags)
            // Loop through all the unbound parameters to see if there are any
            // that take pipeline input for the specified parameter sets.
                if (parameter.Parameter.DoesParameterSetTakePipelineInput(validParameterSetFlags))
        /// Checks for unbound mandatory parameters. If any are found, an exception is thrown.
        /// <param name="missingMandatoryParameters">
        /// Returns the missing mandatory parameters, if any.
        /// True if there are no unbound mandatory parameters. False if there are unbound mandatory parameters.
        internal bool HandleUnboundMandatoryParameters(out Collection<MergedCompiledCommandParameter> missingMandatoryParameters)
            return HandleUnboundMandatoryParameters(
                ValidParameterSetCount(_currentParameterSetFlag),
                out missingMandatoryParameters);
        /// Checks for unbound mandatory parameters. If any are found and promptForMandatory is true,
        /// the user will be prompted for the missing mandatory parameters.
        /// <param name="processMissingMandatory">
        /// If true, unbound mandatory parameters will be processed via user prompting (if allowed by promptForMandatory).
        /// If false, unbound mandatory parameters will cause false to be returned.
        /// <param name="promptForMandatory">
        /// If true, unbound mandatory parameters will cause the user to be prompted. If false, unbound
        /// mandatory parameters will cause an exception to be thrown.
        /// If true, then only parameters that don't take pipeline input will be prompted for.
        /// If false, any mandatory parameter that has not been specified will be prompted for.
        /// True if there are no unbound mandatory parameters. False if there are unbound mandatory parameters
        /// and promptForMandatory if false.
        /// If prompting didn't result in a value for the parameter (only when <paramref name="promptForMandatory"/> is true.)
        internal bool HandleUnboundMandatoryParameters(
            bool processMissingMandatory,
            bool promptForMandatory,
            bool isPipelineInputExpected,
            out Collection<MergedCompiledCommandParameter> missingMandatoryParameters)
            missingMandatoryParameters = GetMissingMandatoryParameters(validParameterSetCount, isPipelineInputExpected);
            if (missingMandatoryParameters.Count > 0)
                if (processMissingMandatory)
                    // If the host interface wasn't specified or we were instructed not to prmopt, then throw
                    // an exception instead
                    if ((Context.EngineHostInterface == null) || (!promptForMandatory))
                            Context.EngineHostInterface != null,
                            "The EngineHostInterface should never be null");
                            "ERROR: host does not support prompting for missing mandatory parameters");
                        string missingParameters = BuildMissingParamsString(missingMandatoryParameters);
                                missingParameters,
                                ParameterBinderStrings.MissingMandatoryParameter,
                                "MissingMandatoryParameter");
                    // Create a collection to store the prompt descriptions of unbound mandatory parameters
                    Collection<FieldDescription> fieldDescriptionList = CreatePromptDataStructures(missingMandatoryParameters);
                    Dictionary<string, PSObject> parameters =
                        PromptForMissingMandatoryParameters(
                            fieldDescriptionList,
                            missingMandatoryParameters);
                        "BIND PROMPTED mandatory parameter args"))
                        // Now bind any parameters that were retrieved.
                        foreach (KeyValuePair<string, PSObject> entry in parameters)
                            var argument =
                                /*parameterAst*/null, entry.Key, "-" + entry.Key + ":",
                                /*argumentAst*/null, entry.Value,
                            // Ignore the result since any failure should cause an exception
                                BindParameter(argument, ParameterBindingFlags.ShouldCoerceType | ParameterBindingFlags.ThrowOnParameterNotFound);
                                "Any error in binding the parameter with type coercion should result in an exception");
        private Dictionary<string, PSObject> PromptForMissingMandatoryParameters(
            Collection<FieldDescription> fieldDescriptionList,
            Collection<MergedCompiledCommandParameter> missingMandatoryParameters)
            Dictionary<string, PSObject> parameters = null;
            // Prompt
                    "PROMPTING for missing mandatory parameters using the host");
                string msg = ParameterBinderStrings.PromptMessage;
                InvocationInfo invoInfo = Command.MyInvocation;
                string caption = StringUtil.Format(ParameterBinderStrings.PromptCaption,
                    invoInfo.MyCommand.Name,
                    invoInfo.PipelinePosition);
                parameters = Context.EngineHostInterface.UI.Prompt(caption, msg, fieldDescriptionList);
            catch (NotImplementedException notImplemented)
                error = notImplemented;
            catch (HostException hostException)
                error = hostException;
            catch (PSInvalidOperationException invalidOperation)
                error = invalidOperation;
            if ((parameters == null) || (parameters.Count == 0))
                    "ERROR: still missing mandatory parameters after PROMPTING");
        internal static string BuildMissingParamsString(Collection<MergedCompiledCommandParameter> missingMandatoryParameters)
            StringBuilder missingParameters = new StringBuilder();
            foreach (MergedCompiledCommandParameter missingParameter in missingMandatoryParameters)
                missingParameters.Append(CultureInfo.InvariantCulture, $" {missingParameter.Parameter.Name}");
            return missingParameters.ToString();
        private Collection<FieldDescription> CreatePromptDataStructures(
            StringBuilder usedHotKeys = new StringBuilder();
            Collection<FieldDescription> fieldDescriptionList = new Collection<FieldDescription>();
            foreach (MergedCompiledCommandParameter parameter in missingMandatoryParameters)
                ParameterSetSpecificMetadata parameterSetMetadata =
                    parameter.Parameter.GetParameterSetData(_currentParameterSetFlag);
                FieldDescription fDesc = new FieldDescription(parameter.Parameter.Name);
                string helpInfo = null;
                    helpInfo = parameterSetMetadata.GetHelpMessage(Command);
                if (!string.IsNullOrEmpty(helpInfo))
                    fDesc.HelpMessage = helpInfo;
                fDesc.SetParameterType(parameter.Parameter.Type);
                fDesc.Label = BuildLabel(parameter.Parameter.Name, usedHotKeys);
                foreach (ValidateArgumentsAttribute vaAttr in parameter.Parameter.ValidationAttributes)
                    fDesc.Attributes.Add(vaAttr);
                foreach (ArgumentTransformationAttribute arAttr in parameter.Parameter.ArgumentTransformationAttributes)
                    fDesc.Attributes.Add(arAttr);
                fDesc.IsMandatory = true;
                fieldDescriptionList.Add(fDesc);
            return fieldDescriptionList;
        /// Creates a label with a Hotkey from <paramref name="parameterName"/>. The Hotkey is
        /// <paramref name="parameterName"/>'s first capital character not in <paramref name="usedHotKeys"/>.
        /// If <paramref name="parameterName"/> does not have any capital character, the first lower
        ///  case character is used. The Hotkey is preceded by an ampersand in the label.
        /// The parameter name from which the Hotkey is created
        /// <param name="usedHotKeys">
        /// A list of used HotKeys
        /// A label made from parameterName with a HotKey indicated by an ampersand
        private static string BuildLabel(string parameterName, StringBuilder usedHotKeys)
            Diagnostics.Assert(!string.IsNullOrEmpty(parameterName), "parameterName is not set");
            const char hotKeyPrefix = '&';
            bool built = false;
            StringBuilder label = new StringBuilder(parameterName);
            string usedHotKeysStr = usedHotKeys.ToString();
            for (int i = 0; i < parameterName.Length; i++)
                // try Upper case
                if (char.IsUpper(parameterName[i]) && usedHotKeysStr.Contains(parameterName[i]))
                    label.Insert(i, hotKeyPrefix);
                    usedHotKeys.Append(parameterName[i]);
                    built = true;
            if (!built)
                // try Lower case
                    if (char.IsLower(parameterName[i]) && usedHotKeysStr.Contains(parameterName[i]))
                // try non-letters
                    if (!char.IsLetter(parameterName[i]) && usedHotKeysStr.Contains(parameterName[i]))
                // use first char
                label.Insert(0, hotKeyPrefix);
            return label.ToString();
        /// Gets the parameter set name for the current parameter set.
        internal string CurrentParameterSetName
                string currentParameterSetName = BindableParameters.GetParameterSetName(_currentParameterSetFlag);
                s_tracer.WriteLine("CurrentParameterSetName = {0}", currentParameterSetName);
                return currentParameterSetName;
        /// Binds the specified object or its properties to parameters
        /// that accept pipeline input.
        /// <param name="inputToOperateOn">
        /// The pipeline object to bind.
        /// True if the pipeline input was bound successfully or there was nothing
        /// to bind, or false if there was an error.
        internal bool BindPipelineParameters(PSObject inputToOperateOn)
                    "BIND PIPELINE object to parameters: [{0}]",
                    // First run any of the delay bind ScriptBlocks and bind the
                    // result to the appropriate parameter.
                    bool thereWasSomethingToBind;
                    bool invokeScriptResult = InvokeAndBindDelayBindScriptBlock(inputToOperateOn, out thereWasSomethingToBind);
                    bool continueBindingAfterScriptBlockProcessing = !thereWasSomethingToBind || invokeScriptResult;
                    bool bindPipelineParametersResult = false;
                    if (continueBindingAfterScriptBlockProcessing)
                        // If any of the parameters in the parameter set which are not yet bound
                        // accept pipeline input, process the input object and bind to those
                        bindPipelineParametersResult = BindPipelineParametersPrivate(inputToOperateOn);
                    // We are successful at binding the pipeline input if there was a ScriptBlock to
                    // run and it ran successfully or if we successfully bound a parameter based on
                    // the pipeline input.
                    result = (thereWasSomethingToBind && invokeScriptResult) || bindPipelineParametersResult;
            catch (ParameterBindingException)
                // Reset the default values
                // This prevents the last pipeline object from being bound during EndProcessing
                // if it failed some post binding verification step.
                this.RestoreDefaultParameterValues(ParametersBoundThroughPipelineInput);
                // Let the parameter binding errors propagate out
                // Now make sure we have latched on to a single parameter set.
        /// Binds the pipeline parameters using the specified input and parameter set.
        /// The pipeline input to be bound to the parameters.
        /// If argument transformation fails.
        /// The argument could not be coerced to the appropriate type for the parameter.
        /// The parameter argument transformation, prerequisite, or validation failed.
        /// If the binding to the parameter fails.
        /// If there is a failure resetting values prior to binding from the pipeline
        /// The algorithm for binding the pipeline object is as follows. If any
        /// step is successful true gets returned immediately.
        /// - If parameter supports ValueFromPipeline
        ///     - attempt to bind input value without type coercion
        /// - If parameter supports ValueFromPipelineByPropertyName
        ///     - attempt to bind the value of the property with the matching name without type coercion
        /// Now see if we have a single valid parameter set and reset the validParameterSets flags as
        /// necessary. If there are still multiple valid parameter sets, then we need to use TypeDistance
        /// to determine which parameters to do type coercion binding on.
        ///     - attempt to bind input value using type coercion
        /// - If parameter support ValueFromPipelineByPropertyName
        ///     - attempt to bind the vlue of the property with the matching name using type coercion
        private bool BindPipelineParametersPrivate(PSObject inputToOperateOn)
            if (ParameterBinderBase.bindingTracer.IsEnabled)
                ConsolidatedString dontuseInternalTypeNames;
                    "PIPELINE object TYPE = [{0}]",
                    inputToOperateOn == null || inputToOperateOn == AutomationNull.Value
                        ? "null"
                        : ((dontuseInternalTypeNames = inputToOperateOn.InternalTypeNames).Count > 0 && dontuseInternalTypeNames[0] != null)
                              ? dontuseInternalTypeNames[0]
                              : inputToOperateOn.BaseObject.GetType().FullName);
                ParameterBinderBase.bindingTracer.WriteLine("RESTORING pipeline parameter's original values");
            // Now clear the parameter names from the previous pipeline input
            ParametersBoundThroughPipelineInput.Clear();
            // Now restore the parameter set flags
            _currentParameterSetFlag = _prePipelineProcessingParameterSetFlags;
            uint validParameterSets = _currentParameterSetFlag;
            bool needToPrioritizeOneSpecificParameterSet = _parameterSetToBePrioritizedInPipelineBinding != 0;
            int steps = needToPrioritizeOneSpecificParameterSet ? 2 : 1;
            if (needToPrioritizeOneSpecificParameterSet)
                // _parameterSetToBePrioritizedInPipelineBinding is set, so we are certain that the specified parameter set must be valid,
                // and it's not the only valid parameter set.
                Diagnostics.Assert((_currentParameterSetFlag & _parameterSetToBePrioritizedInPipelineBinding) != 0, "_parameterSetToBePrioritizedInPipelineBinding should be valid if it's set");
                validParameterSets = _parameterSetToBePrioritizedInPipelineBinding;
            for (int i = 0; i < steps; i++)
                for (CurrentlyBinding currentlyBinding = CurrentlyBinding.ValueFromPipelineNoCoercion; currentlyBinding <= CurrentlyBinding.ValueFromPipelineByPropertyNameWithCoercion; ++currentlyBinding)
                    // The parameterBoundForCurrentlyBindingState will be true as long as there is one parameter gets bound, even if it belongs to AllSet
                    bool parameterBoundForCurrentlyBindingState =
                        BindUnboundParametersForBindingState(
                            inputToOperateOn,
                            currentlyBinding,
                            validParameterSets);
                    if (parameterBoundForCurrentlyBindingState)
                        // Now validate the parameter sets again and update the valid sets.
                        // No need to validate the parameter sets and update the valid sets when dealing with the prioritized parameter set,
                        // this is because the prioritized parameter set is a single set, and when binding succeeds, _currentParameterSetFlag
                        // must be equal to the specific prioritized parameter set.
                        if (!needToPrioritizeOneSpecificParameterSet || i == 1)
                            ValidateParameterSets(true, true);
                            validParameterSets = _currentParameterSetFlag;
                // Update the validParameterSets after the binding attempt for the prioritized parameter set
                if (needToPrioritizeOneSpecificParameterSet && i == 0)
                    // If the prioritized set can be bound successfully, there is no need to do the second round binding
                    if (_currentParameterSetFlag == _parameterSetToBePrioritizedInPipelineBinding)
                    validParameterSets = _currentParameterSetFlag & (~_parameterSetToBePrioritizedInPipelineBinding);
            // Now make sure we only have one valid parameter set
            // Note, this will throw if we have more than one.
            ValidateParameterSets(false, true);
                ApplyDefaultParameterBinding("PIPELINE BIND", false);
        private bool BindUnboundParametersForBindingState(
            PSObject inputToOperateOn,
            CurrentlyBinding currentlyBinding,
            uint validParameterSets)
            bool aParameterWasBound = false;
            // First check to see if the default parameter set has been defined and if it
            // is still valid.
            if (defaultParameterSetFlag != 0 && (validParameterSets & defaultParameterSetFlag) != 0)
                // Since we have a default parameter set and it is still valid, give preference to the
                // parameters in the default set.
                aParameterWasBound =
                    BindUnboundParametersForBindingStateInParameterSet(
                        defaultParameterSetFlag);
                if (!aParameterWasBound)
                    validParameterSets &= ~(defaultParameterSetFlag);
                // Since nothing was bound for the default parameter set, try all
                // the other parameter sets that are still valid.
            s_tracer.WriteLine("aParameterWasBound = {0}", aParameterWasBound);
            return aParameterWasBound;
        private bool BindUnboundParametersForBindingStateInParameterSet(
            // For all unbound parameters in the parameter set, see if we can bind
            // from the input object directly from pipeline without type coercion.
            // We loop the unbound parameters in reversed order, so that we can move
            // items from the unboundParameters collection to the boundParameters
            // collection as we process, without the need to make a copy of the
            // unboundParameters collection.
            // We used to make a copy of UnboundParameters and loop from the head of the
            // list. Now we are processing the unbound parameters from the end of the list.
            // This change should NOT be a breaking change. The 'validParameterSets' in
            // this method never changes, so no matter we start from the head or the end of
            // the list, every unbound parameter in the list that takes pipeline input and
            // satisfy the 'validParameterSets' will be bound. If parameters from more than
            // one sets got bound, then "parameter set cannot be resolved" error will be thrown,
            // which is expected.
            for (int i = UnboundParameters.Count - 1; i >= 0; i--)
                var parameter = UnboundParameters[i];
                // if the parameter is never a pipeline parameter, don't consider it
                // if the parameter is not in the specified parameter set, don't consider it
                if ((validParameterSets & parameter.Parameter.ParameterSetFlags) == 0 &&
                // Get the appropriate parameter set data
                var parameterSetData = parameter.Parameter.GetMatchingParameterSetData(validParameterSets);
                bool bindResult = false;
                foreach (ParameterSetSpecificMetadata parameterSetMetadata in parameterSetData)
                    // In the first phase we try to bind the value from the pipeline without
                    // type coercion
                    if (currentlyBinding == CurrentlyBinding.ValueFromPipelineNoCoercion &&
                        parameterSetMetadata.ValueFromPipeline)
                        bindResult = BindValueFromPipeline(inputToOperateOn, parameter, ParameterBindingFlags.None);
                    // In the next phase we try binding the value from the pipeline by matching
                    // the property name
                    else if (currentlyBinding == CurrentlyBinding.ValueFromPipelineByPropertyNameNoCoercion &&
                        parameterSetMetadata.ValueFromPipelineByPropertyName &&
                        inputToOperateOn != null)
                        bindResult = BindValueFromPipelineByPropertyName(inputToOperateOn, parameter, ParameterBindingFlags.None);
                    // The third step is to attempt to bind the value from the pipeline with
                    // type coercion.
                    else if (currentlyBinding == CurrentlyBinding.ValueFromPipelineWithCoercion &&
                        bindResult = BindValueFromPipeline(inputToOperateOn, parameter, ParameterBindingFlags.ShouldCoerceType);
                    // The final step is to attempt to bind the value from the pipeline by matching
                    else if (currentlyBinding == CurrentlyBinding.ValueFromPipelineByPropertyNameWithCoercion &&
                        bindResult = BindValueFromPipelineByPropertyName(inputToOperateOn, parameter, ParameterBindingFlags.ShouldCoerceType);
                        aParameterWasBound = true;
        private bool BindValueFromPipeline(
            // Attempt binding the value from the pipeline
            // without type coercion
                ((flags & ParameterBindingFlags.ShouldCoerceType) != 0) ?
                    "Parameter [{0}] PIPELINE INPUT ValueFromPipeline WITH COERCION" :
                    "Parameter [{0}] PIPELINE INPUT ValueFromPipeline NO COERCION",
            ParameterBindingException parameterBindingException = null;
                bindResult = BindPipelineParameter(inputToOperateOn, parameter, flags);
            catch (ParameterBindingArgumentTransformationException e)
                PSInvalidCastException invalidCast;
                if (e.InnerException is ArgumentTransformationMetadataException)
                    invalidCast = e.InnerException.InnerException as PSInvalidCastException;
                    invalidCast = e.InnerException as PSInvalidCastException;
                if (invalidCast == null)
                    parameterBindingException = e;
                // Just ignore and continue;
                bindResult = false;
            catch (ParameterBindingValidationException e)
            catch (ParameterBindingParameterDefaultValueException e)
            if (parameterBindingException != null)
                    throw parameterBindingException;
                    ThrowElaboratedBindingException(parameterBindingException);
            return bindResult;
        private bool BindValueFromPipelineByPropertyName(
                    "Parameter [{0}] PIPELINE INPUT ValueFromPipelineByPropertyName WITH COERCION" :
                    "Parameter [{0}] PIPELINE INPUT ValueFromPipelineByPropertyName NO COERCION",
            PSMemberInfo member = inputToOperateOn.Properties[parameter.Parameter.Name];
                // Since a member matching the name of the parameter wasn't found,
                // check the aliases.
                foreach (string alias in parameter.Parameter.Aliases)
                    member = inputToOperateOn.Properties[alias];
                    if (member != null)
                    bindResult =
                        BindPipelineParameter(
                            member.Value,
        /// Used for defining the state of the binding state machine.
        private enum CurrentlyBinding
            ValueFromPipelineNoCoercion = 0,
            ValueFromPipelineByPropertyNameNoCoercion = 1,
            ValueFromPipelineWithCoercion = 2,
            ValueFromPipelineByPropertyNameWithCoercion = 3
        /// Invokes any delay bind script blocks and binds the resulting value
        /// to the appropriate parameter.
        /// The input to the script block.
        /// <param name="thereWasSomethingToBind">
        /// Returns True if there was a ScriptBlock to invoke and bind, or false if there
        /// are no ScriptBlocks to invoke.
        /// True if the binding succeeds, or false otherwise.
        /// if <paramref name="inputToOperateOn"/> is null.
        /// If execution of the script block throws an exception or if it doesn't produce
        /// any output.
        private bool InvokeAndBindDelayBindScriptBlock(PSObject inputToOperateOn, out bool thereWasSomethingToBind)
            thereWasSomethingToBind = false;
            // NOTE: we are not doing backup and restore of default parameter
            // values here.  It is not needed because each script block will be
            // invoked and each delay bind parameter bound for each pipeline object.
            // This is unlike normal pipeline object processing which may bind
            // different parameters depending on the type of the incoming pipeline
            // object.
            // Loop through each of the delay bind script blocks and invoke them.
            // Bind the result to the associated parameter
            foreach (KeyValuePair<MergedCompiledCommandParameter, DelayedScriptBlockArgument> delayedScriptBlock in _delayBindScriptBlocks)
                thereWasSomethingToBind = true;
                CommandParameterInternal argument = delayedScriptBlock.Value._argument;
                MergedCompiledCommandParameter parameter = delayedScriptBlock.Key;
                ScriptBlock script = argument.ArgumentValue as ScriptBlock;
                    script != null,
                    "An argument should only be put in the delayBindScriptBlocks collection if it is a ScriptBlock");
                Collection<PSObject> output = null;
                    "Invoking delay-bind ScriptBlock"))
                    if (delayedScriptBlock.Value._parameterBinder == this)
                            output = script.DoInvoke(inputToOperateOn, inputToOperateOn, Array.Empty<object>());
                            delayedScriptBlock.Value._evaluatedArgument = output;
                        catch (RuntimeException runtimeException)
                            error = runtimeException;
                        output = delayedScriptBlock.Value._evaluatedArgument;
                            ParameterBinderStrings.ScriptBlockArgumentInvocationFailed,
                            "ScriptBlockArgumentInvocationFailed",
                            error.Message);
                if (output == null || output.Count == 0)
                            ParameterBinderStrings.ScriptBlockArgumentNoOutput,
                            "ScriptBlockArgumentNoOutput");
                // Check the output.  If it is only a single value, just pass the single value,
                // if not, pass in the whole collection.
                object newValue = output;
                if (output.Count == 1)
                    newValue = output[0];
                // Create a new CommandParameterInternal for the output of the script block.
                var newArgument = CommandParameterInternal.CreateParameterWithArgument(
                    argument.ParameterAst, argument.ParameterName, "-" + argument.ParameterName + ":",
                    argument.ArgumentAst, newValue,
                if (!BindParameter(newArgument, parameter, ParameterBindingFlags.ShouldCoerceType))
        /// Determines the number of valid parameter sets based on the valid parameter
        /// set flags.
        /// <param name="parameterSetFlags">
        /// The valid parameter set flags.
        /// The number of valid parameter sets in the parameterSetFlags.
        private static int ValidParameterSetCount(uint parameterSetFlags)
            if (parameterSetFlags == uint.MaxValue)
                result = 1;
                    result += (int)(parameterSetFlags & 0x1);
        #endregion helper_methods
        /// This method gets a backup of the default value of a parameter.
        /// Derived classes may override this method to get the default parameter
        /// value in a different way.
        /// The name of the parameter to get the default value of.
        /// The value of the parameter specified by name.
        /// <exception cref="ParameterBindingParameterDefaultValueException">
        /// If the parameter binder encounters an error getting the default value.
        internal object GetDefaultParameterValue(string name)
            MergedCompiledCommandParameter matchingParameter =
                BindableParameters.GetMatchingParameter(
                switch (matchingParameter.BinderAssociation)
                        result = DefaultParameterBinder.GetDefaultParameterValue(name);
                        result = CommonParametersBinder.GetDefaultParameterValue(name);
                        result = ShouldProcessParametersBinder.GetDefaultParameterValue(name);
                            result = _dynamicParameterBinder.GetDefaultParameterValue(name);
            catch (GetValueException getValueException)
                ParameterBindingParameterDefaultValueException bindingError =
                    new ParameterBindingParameterDefaultValueException(
                        getValueException,
                        "ParameterBinderStrings",
                        "GetDefaultValueFailed",
                        getValueException.Message);
                throw bindingError;
        /// Gets or sets the command that this parameter binder controller
        /// will bind parameters to.
        internal Cmdlet Command { get; }
        #region DefaultParameterBindingStructures
        /// The separator used in GetDefaultParameterValuePairs function.
        private const string Separator = ":::";
        // Hold all aliases of the current cmdlet
        private List<string> _aliasList;
        // Method GetDefaultParameterValuePairs() will be invoked twice, one time before the Named Bind,
        // one time after Dynamic Bind. We don't want the same warning message to be written out twice.
        // Put the key(in case the key format is invalid), or cmdletName+separator+parameterName(in case
        // setting resolves to multiple parameters or multiple different values are assigned to the same
        // parameter) in warningSet when the corresponding warnings are written out, so they won't get
        // written out the second time GetDefaultParameterValuePairs() is called.
        private readonly HashSet<string> _warningSet = new HashSet<string>();
        // Hold all user defined default parameter values
        private Dictionary<MergedCompiledCommandParameter, object> _allDefaultParameterValuePairs;
        private bool _useDefaultParameterBinding = true;
        #endregion DefaultParameterBindingStructures
        private uint _parameterSetToBePrioritizedInPipelineBinding = 0;
        /// The cmdlet metadata.
        private readonly CommandMetadata _commandMetadata;
        /// THe command runtime object for this cmdlet.
        private readonly MshCommandRuntime _commandRuntime;
        /// Keep the obsolete parameter warnings generated from parameter binding.
        internal List<WarningRecord> ObsoleteParameterWarningList { get; private set; }
        /// Keep names of the parameters for which we have generated obsolete warning messages.
        private HashSet<string> BoundObsoleteParameterNames
                return _boundObsoleteParameterNames ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> _boundObsoleteParameterNames;
        /// The parameter binder for the dynamic parameters. Currently this
        /// can be either a ReflectionParameterBinder or a RuntimeDefinedParameterBinder.
        private ParameterBinderBase _dynamicParameterBinder;
        /// The parameter binder for the ShouldProcess parameters.
        internal ReflectionParameterBinder ShouldProcessParametersBinder
                if (_shouldProcessParameterBinder == null)
                    // Construct a new instance of the should process parameters object
                    ShouldProcessParameters shouldProcessParameters = new ShouldProcessParameters(_commandRuntime);
                    // Create reflection binder for this object
                    _shouldProcessParameterBinder =
                            shouldProcessParameters,
                return _shouldProcessParameterBinder;
        private ReflectionParameterBinder _shouldProcessParameterBinder;
        /// The parameter binder for the Paging parameters.
        internal ReflectionParameterBinder PagingParametersBinder
                if (_pagingParameterBinder == null)
                    PagingParameters pagingParameters = new PagingParameters(_commandRuntime);
                    _pagingParameterBinder =
                            pagingParameters,
                return _pagingParameterBinder;
        private ReflectionParameterBinder _pagingParameterBinder;
        /// The parameter binder for the Transactions parameters.
        internal ReflectionParameterBinder TransactionParametersBinder
                if (_transactionParameterBinder == null)
                    // Construct a new instance of the transactions parameters object
                    TransactionParameters transactionParameters = new TransactionParameters(_commandRuntime);
                    _transactionParameterBinder =
                            transactionParameters,
                return _transactionParameterBinder;
        private ReflectionParameterBinder _transactionParameterBinder;
        /// The parameter binder for the CommonParameters.
        internal ReflectionParameterBinder CommonParametersBinder
                if (_commonParametersBinder == null)
                    // Construct a new instance of the user feedback parameters object
                    CommonParameters commonParameters = new CommonParameters(_commandRuntime);
                    _commonParametersBinder =
                            commonParameters,
                return _commonParametersBinder;
        private ReflectionParameterBinder _commonParametersBinder;
        private sealed class DelayedScriptBlockArgument
            // Remember the parameter binder so we know when to invoke the script block
            // and when to use the evaluated argument.
            internal CmdletParameterBinderController _parameterBinder;
            internal CommandParameterInternal _argument;
            internal Collection<PSObject> _evaluatedArgument;
                return _argument.ArgumentValue.ToString();
        /// This dictionary is used to contain the arguments that were passed in as ScriptBlocks
        /// but the parameter isn't a ScriptBlock. So we have to wait to bind the parameter
        /// until there is a pipeline object available to invoke the ScriptBlock with.
        private readonly Dictionary<MergedCompiledCommandParameter, DelayedScriptBlockArgument> _delayBindScriptBlocks =
            new Dictionary<MergedCompiledCommandParameter, DelayedScriptBlockArgument>();
        /// A collection of the default values of the parameters.
        private readonly Dictionary<string, CommandParameterInternal> _defaultParameterValues =
            new Dictionary<string, CommandParameterInternal>(StringComparer.OrdinalIgnoreCase);
        /// Binds the specified value to the specified parameter.
        /// <param name="parameterValue">
        /// The value to bind to the parameter
        /// The parameter to bind the value to.
        /// Parameter binding flags for type coercion and validation.
        /// specifies no coercion and the type does not match the parameter type.
        private bool BindPipelineParameter(
            object parameterValue,
            if (parameterValue != AutomationNull.Value)
                s_tracer.WriteLine("Adding PipelineParameter name={0}; value={1}",
                                 parameter.Parameter.Name, parameterValue ?? "null");
                // Backup the default value
                BackupDefaultParameter(parameter);
                // Now bind the new value
                CommandParameterInternal param = CommandParameterInternal.CreateParameterWithArgument(
                    /*parameterAst*/null, parameter.Parameter.Name, "-" + parameter.Parameter.Name + ":",
                flags &= ~ParameterBindingFlags.DelayBindScriptBlock;
                result = BindParameter(_currentParameterSetFlag, param, parameter, flags);
                    // Now make sure to remember that the default value needs to be restored
                    // if we get another pipeline object
                    ParametersBoundThroughPipelineInput.Add(parameter);
        protected override void SaveDefaultScriptParameterValue(string name, object value)
            _defaultParameterValues.Add(name,
                    /*parameterAst*/null, name, "-" + name + ":",
                    /*argumentAst*/null, value,
        /// Backs up the specified parameter value by calling the GetDefaultParameterValue
        /// abstract method.
        /// This method is called when binding a parameter value that came from a pipeline
        private void BackupDefaultParameter(MergedCompiledCommandParameter parameter)
            if (!_defaultParameterValues.ContainsKey(parameter.Parameter.Name))
                object defaultParameterValue = GetDefaultParameterValue(parameter.Parameter.Name);
                _defaultParameterValues.Add(
                        /*argumentAst*/null, defaultParameterValue,
        /// Replaces the values of the parameters with their initial value for the
        /// parameters specified.
        /// <param name="parameters">
        /// The parameters that should have their default values restored.
        /// If <paramref name="parameters"/> is null.
        private void RestoreDefaultParameterValues(IEnumerable<MergedCompiledCommandParameter> parameters)
                throw PSTraceSource.NewArgumentNullException(nameof(parameters));
            // Get all the matching arguments from the defaultParameterValues collection
            // and bind those that had parameters that were bound via pipeline input
            foreach (MergedCompiledCommandParameter parameter in parameters)
                CommandParameterInternal argumentToBind = null;
                // If the argument was found then bind it to the parameter
                // and manage the bound and unbound parameter list
                if (_defaultParameterValues.TryGetValue(parameter.Parameter.Name, out argumentToBind))
                    // Don't go through the normal binding routine to run data generation,
                    // type coercion, validation, or prerequisites since we know the
                    // type is already correct, and we don't want data generation to
                    // run when resetting the default value.
                        // We shouldn't have to coerce the type here so its
                        // faster to pass false
                        bool bindResult = RestoreParameter(argumentToBind, parameter);
                            bindResult,
                            "Restoring the default value should not require type coercion");
                    catch (SetValueException setValueException)
                        error = setValueException;
                        Type specifiedType = argumentToBind.ArgumentValue?.GetType();
                                GetErrorExtent(argumentToBind),
                                ParameterBinderStrings.ParameterBindingFailed,
                                "ParameterBindingFailed",
                    // Since the parameter was returned to its original value,
                    // ensure that it is not in the boundParameters list but
                    // is in the unboundParameters list
                    BoundParameters.Remove(parameter.Parameter.Name);
                    if (!UnboundParameters.Contains(parameter))
                        UnboundParameters.Add(parameter);
                    BoundArguments.Remove(parameter.Parameter.Name);
                    // Since the parameter was not reset, ensure that the parameter
                    // is in the bound parameters list and not in the unbound
                    // parameters list
                    // Ensure the parameter is not in the unboundParameters list
    /// A versionable hashtable, so the caching of UserInput -> ParameterBindingResult will work.
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "DefaultParameterDictionary will only be used for $PSDefaultParameterValues.")]
    public sealed class DefaultParameterDictionary : Hashtable
        private bool _isChanged;
        /// Check to see if the hashtable has been changed since last check.
        /// <returns>True for changed; false for not changed.</returns>
        public bool ChangeSinceLastCheck()
            bool ret = _isChanged;
            _isChanged = false;
        public DefaultParameterDictionary()
            : base(StringComparer.OrdinalIgnoreCase)
            _isChanged = true;
        /// Constructor takes a hash table.
        /// Check for the keys' formats and make it versionable
        /// <param name="dictionary">A hashtable instance.</param>
        public DefaultParameterDictionary(IDictionary dictionary)
            if (dictionary == null)
                throw PSTraceSource.NewArgumentNullException(nameof(dictionary));
            var keysInBadFormat = new List<object>();
                var entryKey = entry.Key as string;
                if (entryKey != null)
                    string key = entryKey.Trim();
                    bool isSpecialKey = false; // The key is 'Disabled'
                    // The key is not with valid format
                    if (!CheckKeyIsValid(key, ref cmdletName, ref parameterName))
                        isSpecialKey = key.Equals("Disabled", StringComparison.OrdinalIgnoreCase);
                        if (!isSpecialKey)
                            keysInBadFormat.Add(entryKey);
                    Diagnostics.Assert(isSpecialKey || (cmdletName != null && parameterName != null), "The cmdletName and parameterName should be set in CheckKeyIsValid");
                    if (keysInBadFormat.Count == 0 && !base.ContainsKey(key))
                        base.Add(key, entry.Value);
                    keysInBadFormat.Add(entry.Key);
            foreach (object badFormatKey in keysInBadFormat)
            if (keysInError.Length > 0)
                string resourceString = keysInBadFormat.Count > 1
                throw PSTraceSource.NewInvalidOperationException(resourceString, keysInError);
        /// Override Contains.
        public override bool Contains(object key)
            return this.ContainsKey(key);
        /// Override ContainsKey.
        public override bool ContainsKey(object key)
            if (key == null)
                throw PSTraceSource.NewArgumentNullException(nameof(key));
            if (key is not string strKey)
            string keyAfterTrim = strKey.Trim();
            return base.ContainsKey(keyAfterTrim);
        /// Override the Add to check for key's format and make it versionable.
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public override void Add(object key, object value)
            AddImpl(key, value, isSelfIndexing: false);
        /// Actual implementation for Add.
        private void AddImpl(object key, object value, bool isSelfIndexing)
                throw PSTraceSource.NewArgumentException(nameof(key), ParameterBinderStrings.StringValueKeyExpected, key, key.GetType().FullName);
            if (base.ContainsKey(keyAfterTrim))
                if (isSelfIndexing)
                    base[keyAfterTrim] = value;
                throw PSTraceSource.NewArgumentException(nameof(key), ParameterBinderStrings.KeyAlreadyAdded, key);
            if (!CheckKeyIsValid(keyAfterTrim, ref cmdletName, ref parameterName))
                if (!keyAfterTrim.Equals("Disabled", StringComparison.OrdinalIgnoreCase))
                    throw PSTraceSource.NewInvalidOperationException(ParameterBinderStrings.SingleKeyInBadFormat, key);
            base.Add(keyAfterTrim, value);
        /// Override the indexing to check for key's format and make it versionable.
        public override object this[object key]
                return base[keyAfterTrim];
                AddImpl(key, value, isSelfIndexing: true);
        /// Override the Remove to make it versionable.
        public override void Remove(object key)
                base.Remove(keyAfterTrim);
        /// Override the Clear to make it versionable.
        public override void Clear()
            base.Clear();
        #region KeyValidation
        /// Check if the key is in valid format. If it is, get the cmdlet name and parameter name.
        /// <returns>Return true if the key is valid, false if not.</returns>
        internal static bool CheckKeyIsValid(string key, ref string cmdletName, ref string parameterName)
            if (key == string.Empty)
            // The index returned should point to the separator or a character that is before the separator
            int index = GetValueToken(0, key, ref cmdletName, true);
            // The index returned should point to the first non-whitespace character, and it should be the separator
            index = SkipWhiteSpace(index, key);
            if (index == -1 || key[index] != ':')
            // The index returned should point to the first non-whitespace character after the separator
            index = SkipWhiteSpace(index + 1, key);
            // The index returned should point to the last character in key
            index = GetValueToken(index, key, ref parameterName, false);
            if (index == -1 || index != key.Length)
        /// Get the cmdlet name and the parameter name.
        /// <param name="index">Point to a non-whitespace character.</param>
        /// <param name="key">The key to iterate over.</param>
        /// <param name="getCmdletName">Specify whether to get the cmdlet name or parameter name.</param>
        /// For cmdletName:
        /// When the name is enclosed by quotes, the index returned should be the index of the character right after the second quote;
        /// When the name is not enclosed by quotes, the index returned should be the index of the separator;
        /// For parameterName:
        /// When the name is enclosed by quotes, the index returned should be the index of the second quote plus 1 (the length of the key if the key is in a valid format);
        /// When the name is not enclosed by quotes, the index returned should be the length of the key.
        private static int GetValueToken(int index, string key, ref string name, bool getCmdletName)
            char quoteChar = '\0';
            if (key[index].IsSingleQuote() || key[index].IsDoubleQuote())
                quoteChar = key[index];
            StringBuilder builder = new StringBuilder(string.Empty);
            for (; index < key.Length; index++)
                if (quoteChar != '\0')
                    if ((quoteChar.IsSingleQuote() && key[index].IsSingleQuote()) ||
                        (quoteChar.IsDoubleQuote() && key[index].IsDoubleQuote()))
                        name = builder.ToString().Trim();
                        // Make the index point to the character right after the quote
                        return name.Length == 0 ? -1 : index + 1;
                    builder.Append(key[index]);
                if (getCmdletName)
                    if (key[index] != ':')
                    return name.Length == 0 ? -1 : index;
            if (!getCmdletName && quoteChar == '\0')
                Diagnostics.Assert(name.Length > 0, "name should not be empty at this point");
                return index;
        /// Skip whitespace characters.
        /// <param name="index">Start index.</param>
        /// <param name="key">The string to iterate over.</param>
        /// Return -1 if we reach the end of the key, otherwise return the index of the first
        /// non-whitespace character we encounter.
        private static int SkipWhiteSpace(int index, string key)
                if (key[index].IsWhitespace() || key[index] == '\r' || key[index] == '\n')
        #endregion KeyValidation

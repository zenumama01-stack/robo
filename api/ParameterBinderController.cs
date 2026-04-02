    /// The base class for the parameter binder controllers. This class and
    /// its derived classes control the interaction between the command processor
    /// and the parameter binder(s). It holds the state of the arguments and parameters.
    [DebuggerDisplay("InvocationInfo = {InvocationInfo}")]
    internal abstract class ParameterBinderController
        /// Constructs a parameter binder controller for the specified command
        /// in the specified engine context.
        ///     The invocation information about the code being run.
        ///     The engine context in which the command is being run.
        ///     The default parameter binder for the command.
        internal ParameterBinderController(InvocationInfo invocationInfo, ExecutionContext context, ParameterBinderBase parameterBinder)
            Diagnostics.Assert(invocationInfo != null, "Caller to verify invocationInfo is not null.");
            Diagnostics.Assert(parameterBinder != null, "Caller to verify parameterBinder is not null.");
            Diagnostics.Assert(context != null, "call to verify context is not null.");
            this.DefaultParameterBinder = parameterBinder;
            InvocationInfo = invocationInfo;
        /// The engine context the command is running in.
        /// Gets the parameter binder for the command.
        internal ParameterBinderBase DefaultParameterBinder { get; }
        internal InvocationInfo InvocationInfo { get; }
        /// All the metadata associated with any of the parameters that
        /// are available from the command.
        internal MergedCommandParameterMetadata BindableParameters
            get { return _bindableParameters; }
        protected MergedCommandParameterMetadata _bindableParameters = new MergedCommandParameterMetadata();
        /// A list of the unbound parameters for the command.
        protected List<MergedCompiledCommandParameter> UnboundParameters { get; set; }
        /// A collection of the bound parameters for the command. The collection is
        /// indexed based on the name of the parameter.
        protected Dictionary<string, MergedCompiledCommandParameter> BoundParameters { get; } = new Dictionary<string, MergedCompiledCommandParameter>(StringComparer.OrdinalIgnoreCase);
            get { return this.DefaultParameterBinder.CommandLineParameters; }
        /// Set true if the default parameter binding is in use.
        protected bool DefaultParameterBindingInUse { get; set; } = false;
        // Set true if the default parameter values are applied
        /// A collection of bound default parameters.
        protected Collection<string> BoundDefaultParameters { get; } = new Collection<string>();
        // Keep record of the bound default parameters
        /// A collection of the unbound arguments.
        protected Collection<CommandParameterInternal> UnboundArguments { get; set; } = new Collection<CommandParameterInternal>();
        internal void ClearUnboundArguments()
        /// A collection of the arguments that have been bound.
        protected Dictionary<string, CommandParameterInternal> BoundArguments { get; } = new Dictionary<string, CommandParameterInternal>(StringComparer.OrdinalIgnoreCase);
        /// Reparses the unbound arguments using the parameter metadata of the
        /// specified parameter binder as the parsing guide.
        /// If a parameter token is not matched with an argument and its not a bool or
        /// SwitchParameter.
        /// Or
        /// The name of the argument matches more than one parameter.
        protected void ReparseUnboundArguments()
            Collection<CommandParameterInternal> result = new Collection<CommandParameterInternal>();
            for (int index = 0; index < UnboundArguments.Count; ++index)
                CommandParameterInternal argument = UnboundArguments[index];
                // If the parameter name is not specified, or if it is specified _and_ there is an
                // argument, we have nothing to reparse for this argument.
                if (!argument.ParameterNameSpecified || argument.ArgumentSpecified)
                    result.Add(argument);
                Diagnostics.Assert(argument.ParameterNameSpecified && !argument.ArgumentSpecified,
                    "At this point, we only process parameters with no arguments");
                // Now check the argument name with the binder.
                string parameterName = argument.ParameterName;
                    _bindableParameters.GetMatchingParameter(
                        new InvocationInfo(this.InvocationInfo.MyCommand, argument.ParameterExtent));
                if (matchingParameter == null)
                    // Since we couldn't find a match, just add the argument as it was
                    // and continue
                // Now that we know we have a single match for the parameter name,
                // see if we can figure out what the argument value for the parameter is.
                // If its a bool or switch parameter, then set the value to true and continue
                if (IsSwitchAndSetValue(parameterName, argument, matchingParameter.Parameter))
                // Since it's not a bool or a SwitchParameter we need to check the next
                if (UnboundArguments.Count - 1 > index)
                    CommandParameterInternal nextArgument = UnboundArguments[index + 1];
                    // Since the argument appears to be a valid parameter, check the
                    // next argument to see if it is the value for that parameter
                    if (nextArgument.ParameterNameSpecified)
                        // Since we have a valid parameter we need to see if the next argument is
                        // an argument value for that parameter or a parameter itself.
                        MergedCompiledCommandParameter nextMatchingParameter =
                                nextArgument.ParameterName,
                                new InvocationInfo(this.InvocationInfo.MyCommand, nextArgument.ParameterExtent));
                        if ((nextMatchingParameter != null) || nextArgument.ParameterAndArgumentSpecified)
                            // Since the next argument is a valid parameter that means the current
                            // argument doesn't have a value
                            // It is an error to have an argument that is a parameter name
                            // but doesn't have a value
                                    matchingParameter.Parameter.Name,
                                    matchingParameter.Parameter.Type,
                        argument.ParameterName = matchingParameter.Parameter.Name;
                        argument.SetArgumentValue(nextArgument.ArgumentAst, nextArgument.ParameterText);
                    // The next argument appears to be the value for this parameter. Set the value,
                    // increment the index and continue
                    argument.SetArgumentValue(nextArgument.ArgumentAst, nextArgument.ArgumentValue);
                    // It is an error to have a argument that is a parameter name
            UnboundArguments = result;
        protected void InitUnboundArguments(Collection<CommandParameterInternal> arguments)
            // Add the passed in arguments to the unboundArguments collection
            Collection<CommandParameterInternal> paramsFromSplatting = null;
            foreach (CommandParameterInternal argument in arguments)
                if (argument.FromHashtableSplatting)
                    paramsFromSplatting ??= new Collection<CommandParameterInternal>();
                    paramsFromSplatting.Add(argument);
                    UnboundArguments.Add(argument);
            // Move the arguments from hashtable splatting to the end of the unbound args list, so that
            // the explicitly specified named arguments can supersede those from a hashtable splatting.
            if (paramsFromSplatting != null)
                foreach (CommandParameterInternal argument in paramsFromSplatting)
        private static bool IsSwitchAndSetValue(
            string argumentName,
            CompiledCommandParameter matchingParameter)
            if (matchingParameter.Type == typeof(SwitchParameter))
                argument.ParameterName = argumentName;
                argument.SetArgumentValue(null, SwitchParameter.Present);
        /// The argument looks like a parameter if it is a string
        /// and starts with a dash.
        /// <param name="arg">
        /// The argument to check.
        /// True if the argument is a string and starts with a dash,
        /// or false otherwise.
        internal static bool ArgumentLooksLikeParameter(string arg)
                result = arg[0].IsDash();
        /// Reparses the arguments specified in the object[] and generates CommandParameterInternal instances
        /// based on whether the arguments look like parameters. The CommandParameterInternal instances then
        /// get added to the specified command processor.
        /// <param name="commandProcessor">
        /// The command processor instance to add the reparsed parameters to.
        /// The arguments that require reparsing.
        internal static void AddArgumentsToCommandProcessor(CommandProcessorBase commandProcessor, object[] arguments)
            if ((arguments != null) && (arguments.Length > 0))
                PSBoundParametersDictionary boundParameters = arguments[0] as PSBoundParametersDictionary;
                if ((boundParameters != null) && (arguments.Length == 1))
                    // If they are supplying a dictionary of parameters, use those directly
                    foreach (KeyValuePair<string, object> boundParameter in boundParameters)
                            /*parameterAst*/null, boundParameter.Key, boundParameter.Key,
                            /*argumentAst*/null, boundParameter.Value, false);
                        commandProcessor.AddParameter(param);
                    // Otherwise, we need to parse them ourselves
                    for (int argIndex = 0; argIndex < arguments.Length; ++argIndex)
                        CommandParameterInternal param;
                        string paramText = arguments[argIndex] as string;
                        if (ArgumentLooksLikeParameter(paramText))
                            // The argument looks like a parameter.
                            // Create a parameter with argument if the paramText is like this: -Path:c:\windows
                            // Combine it with the next argument if there is an argument, and the parameter ends in ':'.
                            var colonIndex = paramText.IndexOf(':');
                            if (colonIndex != -1 && colonIndex != paramText.Length - 1)
                                param = CommandParameterInternal.CreateParameterWithArgument(
                                    /*parameterAst*/null, paramText.Substring(1, colonIndex - 1), paramText,
                                    /*argumentAst*/null, paramText.AsSpan(colonIndex + 1).Trim().ToString(),
                            else if (argIndex == arguments.Length - 1 || paramText[paramText.Length - 1] != ':')
                                param = CommandParameterInternal.CreateParameter(
                                    paramText.Substring(1), paramText);
                                    /*parameterAst*/null, paramText.Substring(1, paramText.Length - 2), paramText,
                                    /*argumentAst*/null, arguments[argIndex + 1],
                                argIndex++;
                            param = CommandParameterInternal.CreateArgument(arguments[argIndex]);
        /// Bind the argument to the specified parameter.
        /// The flags for type coercion, validation, and script block binding.
        /// True if the parameter was successfully bound. False if <paramref name="flags"/> does not have the
        /// flag <see>ParameterBindingFlags.ShouldCoerceType</see> and the type does not match the parameter type.
        /// The parameter has already been bound.
                    (flags & ParameterBindingFlags.ThrowOnParameterNotFound) != 0,
            if (matchingParameter != null)
                // Now check to make sure it hasn't already been
                // bound by looking in the boundParameters collection
                if (BoundParameters.ContainsKey(matchingParameter.Parameter.Name))
                            ParameterBinderStrings.ParameterAlreadyBound,
                            nameof(ParameterBinderStrings.ParameterAlreadyBound));
                result = BindParameter(_currentParameterSetFlag, argument, matchingParameter, flags);
        /// Derived classes need to define the binding of multiple arguments.
        /// The arguments to be bound.
        /// The arguments which are still not bound.
        internal virtual Collection<CommandParameterInternal> BindParameters(Collection<CommandParameterInternal> parameters)
        /// Flags for type coercion and validation of the arguments.
        /// specifies no type coercion and the type does not match the parameter type.
        /// If <paramref name="parameter"/> or <paramref name="argument"/> is null.
                        this.DefaultParameterBinder.BindParameter(
                        "Only the formal parameters are available for this type of command");
        /// This is used by <see cref="BindNamedParameters"/> to validate and bind a given named parameter.
        protected virtual void BindNamedParameter(
            BindParameter(parameterSets, argument, parameter, ParameterBindingFlags.ShouldCoerceType);
        /// Bind the named parameters from the specified argument collection,
        /// for only the parameters in the specified parameter set.
        /// The arguments that should be attempted to bind to the parameters of the specified parameter binder.
        /// if multiple parameters are found matching the name.
        /// if no match could be found.
        protected Collection<CommandParameterInternal> BindNamedParameters(uint parameterSets, Collection<CommandParameterInternal> arguments)
            HashSet<string> boundExplicitNamedParams = null;
                if (!argument.ParameterNameSpecified)
                // We don't want to throw an exception yet because the parameter might be a positional argument,
                // or in case of a cmdlet or an advanced function, it might match up to a dynamic parameter.
                MergedCompiledCommandParameter parameter =
                        name: argument.ParameterName,
                        throwOnParameterNotFound: false,
                        tryExactMatching: true,
                        invocationInfo: new InvocationInfo(this.InvocationInfo.MyCommand, argument.ParameterExtent));
                // If the parameter is not in the specified parameter set, throw a binding exception
                    string formalParamName = parameter.Parameter.Name;
                        boundExplicitNamedParams ??= new HashSet<string>(
                            BoundParameters.Keys,
                        if (boundExplicitNamedParams.Contains(formalParamName))
                            // This named parameter from splatting is also explicitly specified by the user,
                            // which was successfully bound, so we ignore the one from splatting because it
                            // is superseded by the explicit one. For example:
                            //   $splat = @{ Path = $path1 }
                            //   dir @splat -Path $path2
                    if (BoundParameters.ContainsKey(formalParamName))
                    BindNamedParameter(parameterSets, argument, parameter);
                else if (argument.ParameterName.Equals(Parser.VERBATIM_PARAMETERNAME, StringComparison.Ordinal))
                    // We sometimes send a magic parameter from a remote machine with the values referenced via
                    // a using expression ($using:x).  We then access these values via PSBoundParameters, so
                    // "bind" them here.
                    DefaultParameterBinder.CommandLineParameters.SetImplicitUsingParameters(argument.ArgumentValue);
        /// Binds the unbound arguments to positional parameters.
        /// <param name="unboundArguments">
        /// The unbound arguments to attempt to bind as positional arguments.
        /// <param name="validParameterSets">
        /// The current parameter set flags that are valid.
        /// <param name="defaultParameterSet">
        /// The parameter set to use to disambiguate parameters that have the same position
        /// The remaining arguments that have not been bound.
        /// It is assumed that the unboundArguments parameter has already been processed
        /// for this parameter binder. All named parameters have been paired with their
        /// values. Any arguments that don't have a name are considered positional and
        /// will be processed in this method.
        /// If multiple parameters were found for the same position in the specified
        /// parameter set.
        internal Collection<CommandParameterInternal> BindPositionalParameters(
            Collection<CommandParameterInternal> unboundArguments,
            uint validParameterSets,
            out ParameterBindingException outgoingBindingException
            if (unboundArguments.Count > 0)
                // Create a new collection to iterate over so that we can remove
                // unbound arguments while binding them.
                List<CommandParameterInternal> unboundArgumentsCollection = new List<CommandParameterInternal>(unboundArguments);
                // Get a sorted dictionary of the positional parameters with the position
                // as the key
                SortedDictionary<int, Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter>> positionalParameterDictionary;
                    positionalParameterDictionary =
                        EvaluateUnboundPositionalParameters(UnboundParameters, _currentParameterSetFlag);
                    // The parameter set declaration is ambiguous so
                    // throw an exception.
                            ParameterBinderStrings.AmbiguousPositionalParameterNoName,
                            "AmbiguousPositionalParameterNoName");
                    // This exception is thrown because the binder found two positional parameters
                    // from the same parameter set with the same position defined. This is not caused
                    // by introducing the default parameter binding.
                if (positionalParameterDictionary.Count > 0)
                    int unboundArgumentsIndex = 0;
                    foreach (Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter> nextPositionalParameters in positionalParameterDictionary.Values)
                        // Only continue if there are parameters at the specified position. Parameters
                        // can be removed as the parameter set gets narrowed down.
                        if (nextPositionalParameters.Count == 0)
                        CommandParameterInternal argument = GetNextPositionalArgument(
                            unboundArgumentsCollection,
                            ref unboundArgumentsIndex);
                        // Bind first to defaultParameterSet without type coercion, then to
                        // other sets without type coercion, then to the defaultParameterSet with
                        // type coercion and finally to the other sets with type coercion.
                        if (defaultParameterSet != 0 && (validParameterSets & defaultParameterSet) != 0)
                            // Favor the default parameter set.
                            // First try without type coercion
                                BindPositionalParametersInSet(
                                    defaultParameterSet,
                                    nextPositionalParameters,
                                    ParameterBindingFlags.DelayBindScriptBlock,
                            // Try the non-default parameter sets
                            // without type coercion.
                                    validParameterSets,
                            // Now try the default parameter set with type coercion
                                        ParameterBindingFlags.ShouldCoerceType | ParameterBindingFlags.DelayBindScriptBlock,
                            // with type coercion.
                            // Add the unprocessed argument to the results and continue
                            // Update the parameter sets if necessary
                            if (validParameterSets != _currentParameterSetFlag)
                                UpdatePositionalDictionary(positionalParameterDictionary, validParameterSets);
                    // Now for any arguments that were not processed, add them to
                    // the result
                    for (int index = unboundArgumentsIndex; index < unboundArgumentsCollection.Count; ++index)
                        result.Add(unboundArgumentsCollection[index]);
                    // Since no positional parameters were found, add the arguments
                    // to the result
                    result = unboundArguments;
        /// This method only updates the collections contained in the dictionary, not the dictionary
        /// itself to contain only the parameters that are in the specified parameter set.
        /// <param name="positionalParameterDictionary">
        /// The sorted dictionary of positional parameters.
        /// Valid parameter sets
        internal static void UpdatePositionalDictionary(
            SortedDictionary<int, Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter>> positionalParameterDictionary,
            foreach (Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter> parameterCollection in positionalParameterDictionary.Values)
                Collection<MergedCompiledCommandParameter> paramToRemove = new Collection<MergedCompiledCommandParameter>();
                foreach (PositionalCommandParameter positionalParameter in parameterCollection.Values)
                    Collection<ParameterSetSpecificMetadata> parameterSetData = positionalParameter.ParameterSetData;
                    for (int index = parameterSetData.Count - 1; index >= 0; --index)
                        if ((parameterSetData[index].ParameterSetFlag & validParameterSets) == 0 &&
                            !parameterSetData[index].IsInAllSets)
                            // The parameter is not in the valid parameter sets so remove it from the collection.
                            parameterSetData.RemoveAt(index);
                    if (parameterSetData.Count == 0)
                        paramToRemove.Add(positionalParameter.Parameter);
                // Now remove all the parameters that no longer have parameter set data
                foreach (MergedCompiledCommandParameter removeParam in paramToRemove)
                    parameterCollection.Remove(removeParam);
        private bool BindPositionalParametersInSet(
            Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter> nextPositionalParameters,
            out ParameterBindingException bindingException
            bindingException = null;
            foreach (PositionalCommandParameter parameter in nextPositionalParameters.Values)
                foreach (ParameterSetSpecificMetadata parameterSetData in parameter.ParameterSetData)
                    if ((validParameterSets & parameterSetData.ParameterSetFlag) == 0 &&
                        !parameterSetData.IsInAllSets)
                    string parameterName = parameter.Parameter.Parameter.Name;
                    ParameterBindingException parameterBindingExceptionToThrown = null;
                                argument.ArgumentAst, argument.ArgumentValue,
                    catch (ParameterBindingArgumentTransformationException pbex)
                        parameterBindingExceptionToThrown = pbex;
                    catch (ParameterBindingValidationException pbex)
                        if (pbex.SwallowException)
                            // Just ignore and continue
                            bindingException = pbex;
                    catch (ParameterBindingParameterDefaultValueException pbex)
                    catch (ParameterBindingException e)
                        bindingException = e;
                    if (parameterBindingExceptionToThrown != null)
                            throw parameterBindingExceptionToThrown;
                            ThrowElaboratedBindingException(parameterBindingExceptionToThrown);
                        this.CommandLineParameters.MarkAsBoundPositionally(parameterName);
        /// Generate elaborated binding exception so that the user will know the default binding might cause the failure.
        /// <param name="pbex"></param>
        protected void ThrowElaboratedBindingException(ParameterBindingException pbex)
            if (pbex == null)
                throw PSTraceSource.NewArgumentNullException(nameof(pbex));
            Diagnostics.Assert(pbex.ErrorRecord != null, "ErrorRecord should not be null in a ParameterBindingException");
            // Original error message
            string oldMsg = pbex.Message;
            // Default parameters get bound so far
            StringBuilder defaultParamsGetBound = new StringBuilder();
                defaultParamsGetBound.Append(CultureInfo.InvariantCulture, $" -{paramName}");
            string resourceString = ParameterBinderStrings.DefaultBindingErrorElaborationSingle;
            if (BoundDefaultParameters.Count > 1)
                resourceString = ParameterBinderStrings.DefaultBindingErrorElaborationMultiple;
            ParameterBindingException newBindingException =
                    pbex.InnerException,
                    pbex,
                    oldMsg, defaultParamsGetBound);
            throw newBindingException;
        private static CommandParameterInternal GetNextPositionalArgument(
            List<CommandParameterInternal> unboundArgumentsCollection,
            Collection<CommandParameterInternal> nonPositionalArguments,
            ref int unboundArgumentsIndex)
            // Find the next positional argument
            // An argument without a name is considered to be positional since
            // we are assuming the unboundArguments have been reparsed using
            // the merged metadata from this parameter binder controller.
            CommandParameterInternal result = null;
            while (unboundArgumentsIndex < unboundArgumentsCollection.Count)
                CommandParameterInternal argument = unboundArgumentsCollection[unboundArgumentsIndex++];
                    result = argument;
                nonPositionalArguments.Add(argument);
                // Now check to see if the next argument needs to be consumed as well.
                if (unboundArgumentsCollection.Count - 1 >= unboundArgumentsIndex)
                    argument = unboundArgumentsCollection[unboundArgumentsIndex];
                        // Since the next argument doesn't appear to be a parameter name
                        // consume it as well.
                        unboundArgumentsIndex++;
        /// Gets the unbound positional parameters in a sorted dictionary in the order of their
        /// positions.
        /// The sorted dictionary of MergedCompiledCommandParameter metadata with the position
        /// as the key.
        internal static SortedDictionary<int, Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter>> EvaluateUnboundPositionalParameters(
            ICollection<MergedCompiledCommandParameter> unboundParameters, uint validParameterSetFlag)
            SortedDictionary<int, Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter>> result =
                new SortedDictionary<int, Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter>>();
            if (unboundParameters.Count > 0)
                // Loop through the unbound parameters and find a parameter in the specified parameter set
                // that has a position greater than or equal to the positionalParameterIndex
                foreach (MergedCompiledCommandParameter parameter in unboundParameters)
                    bool isInParameterSet = (parameter.Parameter.ParameterSetFlags & validParameterSetFlag) != 0 || parameter.Parameter.IsInAllSets;
                    if (isInParameterSet)
                        var parameterSetDataCollection = parameter.Parameter.GetMatchingParameterSetData(validParameterSetFlag);
                        foreach (ParameterSetSpecificMetadata parameterSetData in parameterSetDataCollection)
                            // Skip ValueFromRemainingArguments parameters
                            // Check the position in the parameter set
                            int positionInParameterSet = parameterSetData.Position;
                            if (positionInParameterSet == int.MinValue)
                                // The parameter is not positional so go to the next one
                            AddNewPosition(result, positionInParameterSet, parameter, parameterSetData);
        private static void AddNewPosition(
            SortedDictionary<int, Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter>> result,
            int positionInParameterSet,
            ParameterSetSpecificMetadata parameterSetData)
            Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter> positionalCommandParameters;
            if (result.TryGetValue(positionInParameterSet, out positionalCommandParameters))
                // Check to see if any of the other parameters in this position are in the same parameter set.
                if (ContainsPositionalParameterInSet(positionalCommandParameters, parameter, parameterSetData.ParameterSetFlag))
                    // Multiple parameters were found with the same
                    // position. This means the parameter set is ambiguous.
                    // positional parameter could not be resolved
                    // We throw InvalidOperationException, which the
                    // caller will catch and throw a more
                    // appropriate exception.
                PositionalCommandParameter positionalCommandParameter;
                if (!positionalCommandParameters.TryGetValue(parameter, out positionalCommandParameter))
                    positionalCommandParameter = new PositionalCommandParameter(parameter);
                    positionalCommandParameters.Add(parameter, positionalCommandParameter);
                positionalCommandParameter.ParameterSetData.Add(parameterSetData);
                Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter> newPositionDictionary =
                    new Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter>();
                PositionalCommandParameter newPositionalParameter = new PositionalCommandParameter(parameter);
                newPositionalParameter.ParameterSetData.Add(parameterSetData);
                newPositionDictionary.Add(parameter, newPositionalParameter);
                result.Add(positionInParameterSet, newPositionDictionary);
        private static bool ContainsPositionalParameterInSet(
            Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter> positionalCommandParameters,
            uint parameterSet)
            foreach (KeyValuePair<MergedCompiledCommandParameter, PositionalCommandParameter> pair in positionalCommandParameters)
                // It's OK to have the same parameter
                if (pair.Key == parameter)
                foreach (ParameterSetSpecificMetadata parameterSetData in pair.Value.ParameterSetData)
                    if ((parameterSetData.ParameterSetFlag & parameterSet) != 0 ||
                        parameterSetData.ParameterSetFlag == parameterSet)
        /// Keeps track of the parameters that get bound through pipeline input, so that their
        /// previous values can be restored before the next pipeline input comes.
        internal Collection<MergedCompiledCommandParameter> ParametersBoundThroughPipelineInput { get; } = new Collection<MergedCompiledCommandParameter>();
        /// For any unbound parameters, this method checks to see if the
        /// parameter has a default value specified, and evaluates the expression
        /// (if the expression is not constant) and binds the result to the parameter.
        /// If not, we bind null to the parameter (which may go through type coercion).
        internal void BindUnboundScriptParameters()
                BindUnboundScriptParameterWithDefaultValue(parameter);
        /// If the parameter binder might use the value more than once, this it can save the value to avoid
        /// re-evaluating complicated expressions.
        protected virtual void SaveDefaultScriptParameterValue(string name, object value)
            // By default, parameter binders don't need to remember the value, the exception being the cmdlet parameter binder.
        /// Bind the default value for an unbound parameter to script (used by both the script binder
        /// and the cmdlet binder).
        internal void BindUnboundScriptParameterWithDefaultValue(MergedCompiledCommandParameter parameter)
            ScriptParameterBinder spb = (ScriptParameterBinder)this.DefaultParameterBinder;
            ScriptBlock script = spb.Script;
            RuntimeDefinedParameter runtimeDefinedParameter;
            if (script.RuntimeDefinedParameters.TryGetValue(parameter.Parameter.Name, out runtimeDefinedParameter))
                bool oldRecordParameters = spb.RecordBoundParameters;
                    spb.RecordBoundParameters = false;
                    // We may pass a magic parameter from the remote end with the values for the using expressions.
                    // In this case, we want to use those values to evaluate the default value. e.g. param($a = $using:date)
                    System.Collections.IDictionary implicitUsingParameters = null;
                    if (DefaultParameterBinder.CommandLineParameters != null)
                        implicitUsingParameters = DefaultParameterBinder.CommandLineParameters.GetImplicitUsingParameters();
                    object result = spb.GetDefaultScriptParameterValue(runtimeDefinedParameter, implicitUsingParameters);
                    SaveDefaultScriptParameterValue(parameter.Parameter.Name, result);
                    CommandParameterInternal argument = CommandParameterInternal.CreateParameterWithArgument(
                        /*argumentAst*/null, result,
                    ParameterBindingFlags flags = ParameterBindingFlags.IsDefaultValue;
                    // Only coerce explicit values.  We default to null, which isn't always convertible.
                    if (runtimeDefinedParameter.IsSet)
                        flags |= ParameterBindingFlags.ShouldCoerceType;
                    BindParameter(uint.MaxValue, argument, parameter, flags);
                    spb.RecordBoundParameters = oldRecordParameters;
        internal uint _currentParameterSetFlag = uint.MaxValue;
        internal uint _prePipelineProcessingParameterSetFlags = uint.MaxValue;

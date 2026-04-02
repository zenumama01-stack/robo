    internal class MergedCommandParameterMetadata
        /// Replaces any existing metadata in this object with the metadata specified.
        /// Note that this method should NOT be called after a MergedCommandParameterMetadata
        /// instance is made read only by calling MakeReadOnly(). This is because MakeReadOnly()
        /// will turn 'bindableParameters', 'aliasedParameters' and 'parameterSetMap' into
        /// ReadOnlyDictionary and ReadOnlyCollection.
        /// <param name="metadata">
        /// The metadata to replace in this object.
        /// A list of the merged parameter metadata that was added.
        internal List<MergedCompiledCommandParameter> ReplaceMetadata(MergedCommandParameterMetadata metadata)
            var result = new List<MergedCompiledCommandParameter>();
            // Replace bindable parameters
            _bindableParameters.Clear();
            foreach (KeyValuePair<string, MergedCompiledCommandParameter> entry in metadata.BindableParameters)
                _bindableParameters.Add(entry.Key, entry.Value);
            _aliasedParameters.Clear();
            foreach (KeyValuePair<string, MergedCompiledCommandParameter> entry in metadata.AliasedParameters)
                _aliasedParameters.Add(entry.Key, entry.Value);
            // Replace additional meta info
            _defaultParameterSetName = metadata._defaultParameterSetName;
            _nextAvailableParameterSetIndex = metadata._nextAvailableParameterSetIndex;
            _parameterSetMap.Clear();
            var parameterSetMapInList = (List<string>)_parameterSetMap;
            parameterSetMapInList.AddRange(metadata._parameterSetMap);
            Diagnostics.Assert(ParameterSetCount == _nextAvailableParameterSetIndex,
                "After replacement with the metadata of the new parameters, ParameterSetCount should be equal to nextAvailableParameterSetIndex");
        /// Merges the specified metadata with the other metadata already defined
        /// in this object.
        /// <param name="parameterMetadata">
        /// The compiled metadata for the type to be merged.
        /// <param name="binderAssociation">
        /// The type of binder that the CommandProcessor will use to bind
        /// the parameters for <paramref name="parameterMetadata"/>
        /// A collection of the merged parameter metadata that was added.
        /// If a parameter name or alias described in the <paramref name="parameterMetadata"/> already
        internal Collection<MergedCompiledCommandParameter> AddMetadataForBinder(
            InternalParameterMetadata parameterMetadata,
            ParameterBinderAssociation binderAssociation)
            if (parameterMetadata == null)
                throw PSTraceSource.NewArgumentNullException(nameof(parameterMetadata));
            Collection<MergedCompiledCommandParameter> result =
                new Collection<MergedCompiledCommandParameter>();
            // Merge in the bindable parameters
            foreach (KeyValuePair<string, CompiledCommandParameter> bindableParameter in parameterMetadata.BindableParameters)
                if (_bindableParameters.ContainsKey(bindableParameter.Key))
                    MetadataException exception =
                            "ParameterNameAlreadyExistsForCommand",
                            Metadata.ParameterNameAlreadyExistsForCommand,
                            bindableParameter.Key);
                // NTRAID#Windows Out Of Band Releases-926371-2005/12/27-JonN
                if (_aliasedParameters.ContainsKey(bindableParameter.Key))
                            "ParameterNameConflictsWithAlias",
                            Metadata.ParameterNameConflictsWithAlias,
                            bindableParameter.Key,
                            RetrieveParameterNameForAlias(bindableParameter.Key, _aliasedParameters));
                MergedCompiledCommandParameter mergedParameter =
                    new MergedCompiledCommandParameter(bindableParameter.Value, binderAssociation);
                _bindableParameters.Add(bindableParameter.Key, mergedParameter);
                result.Add(mergedParameter);
                // Merge in the aliases
                foreach (string aliasName in bindableParameter.Value.Aliases)
                    if (_aliasedParameters.ContainsKey(aliasName))
                                "AliasParameterNameAlreadyExistsForCommand",
                                Metadata.AliasParameterNameAlreadyExistsForCommand,
                                aliasName);
                    if (_bindableParameters.ContainsKey(aliasName))
                                RetrieveParameterNameForAlias(aliasName, _bindableParameters),
                                bindableParameter.Value.Name);
                    _aliasedParameters.Add(aliasName, mergedParameter);
        /// The next available parameter set bit. This number increments but the parameter
        /// set bit is really 1 shifted left this number of times. This number also acts
        /// as the index for the parameter set map.
        private uint _nextAvailableParameterSetIndex;
        /// The maximum number of parameter sets allowed. Limit is set by the use
        /// of a uint bitmask to store which parameter sets a parameter is included in.
        /// See <see cref="ParameterSetSpecificMetadata.ParameterSetFlag"/>.
        private const uint MaxParameterSetCount = 32;
        /// Gets the number of parameter sets that were declared for the command.
        internal int ParameterSetCount
                return _parameterSetMap.Count;
        /// Gets a bit-field representing all valid parameter sets.
        internal uint AllParameterSetFlags
                return (1u << ParameterSetCount) - 1;
        /// This is the parameter set map. The index is the number of times 1 gets shifted
        /// left to specify the bit field marker for the parameter set.
        /// The value is the parameter set name.
        /// New parameter sets are added at the nextAvailableParameterSetIndex.
        private IList<string> _parameterSetMap = new List<string>();
        /// The name of the default parameter set.
        private string _defaultParameterSetName;
        /// Adds the parameter set name to the parameter set map and returns the
        /// index. If the parameter set name was already in the map, the index to
        /// the existing parameter set name is returned.
        /// <param name="parameterSetName">
        /// The name of the parameter set to add.
        /// The index of the parameter set name. If the name didn't already exist the
        /// name gets added and the new index is returned. If the name already exists
        /// the index of the existing name is returned.
        /// The nextAvailableParameterSetIndex is incremented if the parameter set name
        /// is added.
        /// If more than uint.MaxValue parameter-sets are defined for the command.
        private int AddParameterSetToMap(string parameterSetName)
                index = _parameterSetMap.IndexOf(parameterSetName);
                // A parameter set name should only be added once
                    if (_nextAvailableParameterSetIndex >= MaxParameterSetCount)
                        // Don't let the parameter set index overflow
                        ParsingMetadataException parsingException =
                            new ParsingMetadataException(
                                "ParsingTooManyParameterSets",
                                Metadata.ParsingTooManyParameterSets);
                        throw parsingException;
                    _parameterSetMap.Add(parameterSetName);
                        index == _nextAvailableParameterSetIndex,
                        "AddParameterSetToMap should always add the parameter set name to the map at the nextAvailableParameterSetIndex");
                    _nextAvailableParameterSetIndex++;
        /// Loops through all the parameters and retrieves the parameter set names.  In the process
        /// it generates a mapping of parameter set names to the bits in the bit-field and sets
        /// the parameter set flags for the parameter.
        /// <param name="defaultParameterSetName">
        /// The default parameter set name.
        /// The bit flag for the default parameter set.
        internal uint GenerateParameterSetMappingFromMetadata(string defaultParameterSetName)
            // First clear the parameter set map
            _nextAvailableParameterSetIndex = 0;
            uint defaultParameterSetFlag = 0;
            if (!string.IsNullOrEmpty(defaultParameterSetName))
                // Add the default parameter set to the parameter set map
                int index = AddParameterSetToMap(defaultParameterSetName);
                defaultParameterSetFlag = (uint)1 << index;
            // Loop through all the parameters and then each parameter set for each parameter
            foreach (MergedCompiledCommandParameter parameter in BindableParameters.Values)
                // For each parameter we need to generate a bit-field for the parameter sets
                // that the parameter is a part of.
                uint parameterSetBitField = 0;
                foreach (var keyValuePair in parameter.Parameter.ParameterSetData)
                    var parameterSetName = keyValuePair.Key;
                    var parameterSetData = keyValuePair.Value;
                    if (string.Equals(parameterSetName, ParameterAttribute.AllParameterSets, StringComparison.OrdinalIgnoreCase))
                        // Don't add the parameter set name but assign the bit field zero and then mark the bool
                        parameterSetData.ParameterSetFlag = 0;
                        parameterSetData.IsInAllSets = true;
                        parameter.Parameter.IsInAllSets = true;
                        // Add the parameter set name and/or get the index in the map
                        int index = AddParameterSetToMap(parameterSetName);
                            index >= 0,
                            "AddParameterSetToMap should always be able to add the parameter set name, if not it should throw");
                        // Calculate the bit for this parameter set
                        uint parameterSetBit = (uint)1 << index;
                        // Add the bit to the bit-field
                        parameterSetBitField |= parameterSetBit;
                        // Add the bit to the parameter set specific data
                        parameterSetData.ParameterSetFlag = parameterSetBit;
                // Set the bit field in the parameter
                parameter.Parameter.ParameterSetFlags = parameterSetBitField;
            return defaultParameterSetFlag;
        /// Gets the parameter set name for the specified parameter set.
        /// <param name="parameterSet">
        /// The parameter set to get the name for.
        /// The name of the specified parameter set.
        internal string GetParameterSetName(uint parameterSet)
            string result = _defaultParameterSetName;
            if (string.IsNullOrEmpty(result))
                result = ParameterAttribute.AllParameterSets;
            if (parameterSet != uint.MaxValue && parameterSet != 0)
                // Count the number of right shifts it takes to hit the parameter set
                // This is the index into the parameter set map.
                while (((parameterSet >> index) & 0x1) == 0)
                    ++index;
                // Now check to see if there are any remaining sets passed this bit.
                // If so return string.Empty
                if (((parameterSet >> (index + 1)) & 0x1) == 0)
                    // Ensure that the bit found was within the map, if not return an empty string
                    if (index < _parameterSetMap.Count)
                        result = _parameterSetMap[index];
        /// Helper function to retrieve the name of the parameter
        /// which defined an alias.
        /// <param name="dict"></param>
        private static string RetrieveParameterNameForAlias(
            string key,
            IDictionary<string, MergedCompiledCommandParameter> dict)
            MergedCompiledCommandParameter mergedParam = dict[key];
            if (mergedParam != null)
                CompiledCommandParameter compiledParam = mergedParam.Parameter;
                if (compiledParam != null)
                    if (!string.IsNullOrEmpty(compiledParam.Name))
                        return compiledParam.Name;
        /// Gets the parameters by matching its name.
        /// <param name="throwOnParameterNotFound">
        /// If true and a matching parameter is not found, an exception will be
        /// throw. If false and a matching parameter is not found, null is returned.
        /// <param name="tryExactMatching">
        /// If true we do exact matching, otherwise we do not.
        /// <param name="invocationInfo">
        /// The invocation information about the code being run.
        /// The a collection of the metadata associated with the parameters that
        /// match the specified name. If no matches were found, an empty collection
        /// is returned.
        internal MergedCompiledCommandParameter GetMatchingParameter(
            bool throwOnParameterNotFound,
            bool tryExactMatching,
            Collection<MergedCompiledCommandParameter> matchingParameters =
            // Skip the leading '-' if present
            if (name.Length > 0 && CharExtensions.IsDash(name[0]))
                name = name.Substring(1);
            // First try to match the bindable parameters
            foreach (string parameterName in _bindableParameters.Keys)
                if (CultureInfo.InvariantCulture.CompareInfo.IsPrefix(parameterName, name, CompareOptions.IgnoreCase))
                    // If it is an exact match then only return the exact match
                    // as the result
                    if (tryExactMatching && string.Equals(parameterName, name, StringComparison.OrdinalIgnoreCase))
                        return _bindableParameters[parameterName];
                        matchingParameters.Add(_bindableParameters[parameterName]);
            // Now check the aliases
            foreach (string parameterName in _aliasedParameters.Keys)
                        return _aliasedParameters[parameterName];
                        if (!matchingParameters.Contains(_aliasedParameters[parameterName]))
                            matchingParameters.Add(_aliasedParameters[parameterName]);
            if (matchingParameters.Count > 1)
                // Prefer parameters in the cmdlet over common parameters
                Collection<MergedCompiledCommandParameter> filteredParameters =
                foreach (MergedCompiledCommandParameter matchingParameter in matchingParameters)
                    if ((matchingParameter.BinderAssociation == ParameterBinderAssociation.DeclaredFormalParameters) ||
                        (matchingParameter.BinderAssociation == ParameterBinderAssociation.DynamicParameters))
                        filteredParameters.Add(matchingParameter);
                if (tryExactMatching && filteredParameters.Count == 1)
                    matchingParameters = filteredParameters;
                        possibleMatches.Append(" -");
                        possibleMatches.Append(matchingParameter.Parameter.Name);
                    ParameterBindingException exception =
                            ParameterBinderStrings.AmbiguousParameter,
                            "AmbiguousParameter",
                            possibleMatches);
            else if (matchingParameters.Count == 0)
                if (throwOnParameterNotFound)
            MergedCompiledCommandParameter result = null;
            if (matchingParameters.Count > 0)
                result = matchingParameters[0];
        /// Gets a collection of all the parameters that are allowed in the parameter set.
        /// The bit representing the parameter set from which the parameters should be retrieved.
        /// A collection of all the parameters in the specified parameter set.
        internal Collection<MergedCompiledCommandParameter> GetParametersInParameterSet(uint parameterSetFlag)
                if ((parameterSetFlag & parameter.Parameter.ParameterSetFlags) != 0 ||
                    parameter.Parameter.IsInAllSets)
        /// Gets a dictionary of the compiled parameter metadata for this Type.
        /// The dictionary keys are the names of the parameters and
        /// the values are the compiled parameter metadata.
        internal IDictionary<string, MergedCompiledCommandParameter> BindableParameters { get { return _bindableParameters; } }
        private IDictionary<string, MergedCompiledCommandParameter> _bindableParameters =
            new Dictionary<string, MergedCompiledCommandParameter>(StringComparer.OrdinalIgnoreCase);
        /// Gets a dictionary of the parameters that have been aliased to other names. The key is
        /// the alias name and the value is the MergedCompiledCommandParameter metadata.
        internal IDictionary<string, MergedCompiledCommandParameter> AliasedParameters { get { return _aliasedParameters; } }
        private IDictionary<string, MergedCompiledCommandParameter> _aliasedParameters =
        internal void MakeReadOnly()
            _bindableParameters = new ReadOnlyDictionary<string, MergedCompiledCommandParameter>(_bindableParameters);
            _aliasedParameters = new ReadOnlyDictionary<string, MergedCompiledCommandParameter>(_aliasedParameters);
            _parameterSetMap = new ReadOnlyCollection<string>(_parameterSetMap);
        internal void ResetReadOnly()
            _bindableParameters = new Dictionary<string, MergedCompiledCommandParameter>(_bindableParameters, StringComparer.OrdinalIgnoreCase);
            _aliasedParameters = new Dictionary<string, MergedCompiledCommandParameter>(_aliasedParameters, StringComparer.OrdinalIgnoreCase);
    /// Makes an association between a CompiledCommandParameter and the type
    /// of the parameter binder used to bind the parameter.
    internal class MergedCompiledCommandParameter
        /// Constructs an association between the CompiledCommandParameter and the
        /// binder that should be used to bind it.
        /// The metadata for a parameter.
        /// The type of binder that should be used to bind the parameter.
        internal MergedCompiledCommandParameter(
                CompiledCommandParameter parameter,
            Diagnostics.Assert(parameter != null, "caller to verify parameter is not null");
            this.Parameter = parameter;
            this.BinderAssociation = binderAssociation;
        /// Gets the compiled command parameter for the association.
        internal CompiledCommandParameter Parameter { get; }
        /// Gets the type of binder that the compiled command parameter should be bound with.
        internal ParameterBinderAssociation BinderAssociation { get; }
            return Parameter.ToString();
    /// This enum is used in the MergedCompiledCommandParameter class
    /// to associate a particular CompiledCommandParameter with the
    /// appropriate ParameterBinder.
    internal enum ParameterBinderAssociation
        /// The parameter was declared as a formal parameter in the command type.
        DeclaredFormalParameters,
        /// The parameter was declared as a dynamic parameter for the command.
        DynamicParameters,
        /// The parameter is a common parameter found in the CommonParameters class.
        CommonParameters,
        /// The parameter is a ShouldProcess parameter found in the ShouldProcessParameters class.
        ShouldProcessParameters,
        /// The parameter is a transactions parameter found in the TransactionParameters class.
        TransactionParameters,
        /// The parameter is a Paging parameter found in the PagingParameters class.
        PagingParameters,

    /// This class holds the data for missing mandatory parameters for each parameter set as we
    /// are trying to process which parameter set to use based on the missing mandatory parameters.
    internal class ParameterSetPromptingData
        internal ParameterSetPromptingData(uint parameterSet, bool isDefaultSet)
            ParameterSet = parameterSet;
            IsDefaultSet = isDefaultSet;
        /// True if this parameter set represents the default parameter set.
        internal bool IsDefaultSet { get; }
        /// The parameter set this data represents.
        internal uint ParameterSet { get; } = 0;
        /// True if the parameter set represents parameters in all the parameter sets.
        internal bool IsAllSet
            get { return ParameterSet == uint.MaxValue; }
        /// Gets the parameters that take pipeline input and are mandatory in this parameter set.
        internal Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata> PipelineableMandatoryParameters
        { get; } = new Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata>();
        /// Gets the parameters that take pipeline input by value, and are mandatory in this parameter set.
        internal Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata> PipelineableMandatoryByValueParameters
        /// Gets the parameters that take pipeline input by property name, and are mandatory in this parameter set.
        internal Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata> PipelineableMandatoryByPropertyNameParameters
        /// Gets the parameters that do not take pipeline input and are mandatory in this parameter set.
        internal Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata> NonpipelineableMandatoryParameters

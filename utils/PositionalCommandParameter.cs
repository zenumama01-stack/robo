    internal class PositionalCommandParameter
        /// Constructs a container for the merged parameter metadata and
        /// parameter set specific metadata for a positional parameter.
        internal PositionalCommandParameter(MergedCompiledCommandParameter parameter)
            Parameter = parameter;
        internal MergedCompiledCommandParameter Parameter { get; }
        internal Collection<ParameterSetSpecificMetadata> ParameterSetData { get; } = new Collection<ParameterSetSpecificMetadata>();

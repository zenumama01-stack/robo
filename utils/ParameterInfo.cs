    /// Provides information about a cmdlet parameter for a particular parameter set.
    public class CommandParameterInfo
        /// Constructs the parameter info using the specified aliases, attributes, and
        /// parameter set metadata.
        /// The parameter metadata to retrieve the parameter information from.
        /// The parameter set flag to get the parameter information from.
        /// If <paramref name="parameter"/> is null.
        internal CommandParameterInfo(
            uint parameterSetFlag)
            Name = parameter.Name;
            ParameterType = parameter.Type;
            IsDynamic = parameter.IsDynamic;
            Aliases = new ReadOnlyCollection<string>(parameter.Aliases);
            SetAttributes(parameter.CompiledAttributes);
            SetParameterSetData(parameter.GetParameterSetData(parameterSetFlag));
        public string Name { get; } = string.Empty;
        public Type ParameterType { get; }
        /// Gets whether or not the parameter is a dynamic parameter.
        public bool IsMandatory { get; private set; }
        /// Gets whether or not the parameter is mandatory.
        /// True if the parameter is mandatory, or false otherwise.
        public bool IsDynamic { get; }
        public int Position { get; private set; } = int.MinValue;
        public bool ValueFromPipeline { get; private set; }
        /// Gets whether the parameter can take values from a property inn the incoming
        /// pipeline object with the same name as the parameter.
        public bool ValueFromPipelineByPropertyName { get; private set; }
        /// Gets whether the parameter will take any argument that isn't bound to another parameter.
        public bool ValueFromRemainingArguments { get; private set; }
        /// Gets the help message for this parameter.
        public string HelpMessage { get; private set; } = string.Empty;
        /// Gets the aliases by which this parameter can be referenced.
        public ReadOnlyCollection<string> Aliases { get; }
        /// Gets the attributes that are specified on the parameter.
        public ReadOnlyCollection<Attribute> Attributes { get; private set; }
        private void SetAttributes(IList<Attribute> attributeMetadata)
                attributeMetadata != null,
                "The compiled attribute collection should never be null");
            Collection<Attribute> processedAttributes = new Collection<Attribute>();
            foreach (var attribute in attributeMetadata)
                processedAttributes.Add(attribute);
            Attributes = new ReadOnlyCollection<Attribute>(processedAttributes);
        private void SetParameterSetData(ParameterSetSpecificMetadata parameterMetadata)
            IsMandatory = parameterMetadata.IsMandatory;
            Position = parameterMetadata.Position;
            ValueFromPipeline = parameterMetadata.valueFromPipeline;
            ValueFromPipelineByPropertyName = parameterMetadata.valueFromPipelineByPropertyName;
            ValueFromRemainingArguments = parameterMetadata.ValueFromRemainingArguments;
            HelpMessage = parameterMetadata.HelpMessage;

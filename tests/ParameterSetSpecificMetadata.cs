    internal class ParameterSetSpecificMetadata
        /// Constructs an instance of the ParameterSetSpecificMetadata using the instance of the attribute
        /// that is specified.
        /// The attribute to be compiled.
        internal ParameterSetSpecificMetadata(ParameterAttribute attribute)
            _attribute = attribute;
            IsMandatory = attribute.Mandatory;
            Position = attribute.Position;
            ValueFromRemainingArguments = attribute.ValueFromRemainingArguments;
            this.valueFromPipeline = attribute.ValueFromPipeline;
            this.valueFromPipelineByPropertyName = attribute.ValueFromPipelineByPropertyName;
            HelpMessage = attribute.HelpMessage;
            HelpMessageBaseName = attribute.HelpMessageBaseName;
            HelpMessageResourceId = attribute.HelpMessageResourceId;
        internal ParameterSetSpecificMetadata(
            bool isMandatory,
            int position,
            bool valueFromRemainingArguments,
            bool valueFromPipeline,
            bool valueFromPipelineByPropertyName,
            string helpMessageBaseName,
            string helpMessageResourceId,
            string helpMessage)
            IsMandatory = isMandatory;
            Position = position;
            ValueFromRemainingArguments = valueFromRemainingArguments;
            this.valueFromPipeline = valueFromPipeline;
            this.valueFromPipelineByPropertyName = valueFromPipelineByPropertyName;
            HelpMessageBaseName = helpMessageBaseName;
            HelpMessageResourceId = helpMessageResourceId;
            HelpMessage = helpMessage;
        /// Returns true if the parameter is mandatory for this parameterset, false otherwise.
        /// If the parameter is allowed to be positional for this parameter set, this returns
        /// the position it is allowed to be in. If it is not positional, this returns int.MinValue.
        internal int Position { get; } = int.MinValue;
        /// Returns true if the parameter is positional for this parameter set, or false otherwise.
        internal bool IsPositional
                return Position != int.MinValue;
        /// Returns true if this parameter takes all the remaining unbound arguments that were specified,
        internal bool ValueFromRemainingArguments { get; }
        internal bool valueFromPipeline;
        /// Specifies that this parameter can take values from the incoming pipeline object.
        internal bool ValueFromPipeline
                return valueFromPipeline;
        internal bool valueFromPipelineByPropertyName;
        /// Specifies that this parameter can take values from a property un the incoming
        internal bool ValueFromPipelineByPropertyName
                return valueFromPipelineByPropertyName;
        /// A short description for this parameter, suitable for presentation as a tool tip.
        internal string HelpMessage { get; }
        /// The base name of the resource for a help message.
        internal string HelpMessageBaseName { get; }
        /// The Id of the resource for a help message.
        internal string HelpMessageResourceId { get; } = null;
        /// Gets or sets the value that tells whether this parameter set
        /// data is for the "all" parameter set.
        /// Gets the parameter set flag that represents the parameter set
        /// that this data is valid for.
        internal uint ParameterSetFlag { get; set; }
        /// If HelpMessageBaseName and HelpMessageResourceId are set, the help info is
        /// loaded from the resource indicated by HelpMessageBaseName and HelpMessageResourceId.
        /// If that fails and HelpMessage is set, the help info is set to HelpMessage; otherwise,
        /// the exception that is thrown when loading the resource is thrown.
        /// If both HelpMessageBaseName and HelpMessageResourceId are not set, the help info is
        /// set to HelpMessage.
        /// Help info about the parameter
        /// If the value of the specified resource is not a string and
        ///     HelpMessage is not set.
        /// If only one of HelpMessageBaseName and HelpMessageResourceId is set
        ///     OR if no usable resources have been found, and
        ///     there are no neutral culture resources and HelpMessage is not set.
        internal string GetHelpMessage(Cmdlet cmdlet)
            bool isHelpMsgSet = !string.IsNullOrEmpty(HelpMessage);
            bool isHelpMsgBaseNameSet = !string.IsNullOrEmpty(HelpMessageBaseName);
            bool isHelpMsgResIdSet = !string.IsNullOrEmpty(HelpMessageResourceId);
            if (isHelpMsgBaseNameSet ^ isHelpMsgResIdSet)
                throw PSTraceSource.NewArgumentException(isHelpMsgBaseNameSet ? "HelpMessageResourceId" : "HelpMessageBaseName");
            if (isHelpMsgBaseNameSet && isHelpMsgResIdSet)
                    helpInfo = cmdlet.GetResourceString(HelpMessageBaseName, HelpMessageResourceId);
                    if (isHelpMsgSet)
                        helpInfo = HelpMessage;
            else if (isHelpMsgSet)
            return helpInfo;
        private readonly ParameterAttribute _attribute;

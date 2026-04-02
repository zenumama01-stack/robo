    /// Implementation for the Format-Custom command. It just calls the formatting engine on complex shape.
    [Cmdlet(VerbsCommon.Format, "Custom", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096929")]
    public class FormatCustomCommand : OuterFormatShapeCommandBase
        /// Initializes a new instance of the <see cref="FormatCustomCommand"/> class
        public FormatCustomCommand()
            this.implementation = new InnerFormatShapeCommand(FormatShape.Complex);
        /// Positional parameter for properties, property sets and table sets.
        /// specified on the command line.
        /// The parameter is optional, since the defaults
        /// will be determined using property sets, etc.
            get { return _props; }
            set { _props = value; }
        private object[] _props;
        /// Gets or sets the properties to exclude from formatting.
        public int Depth
            get { return _depth; }
            set { _depth = value; }
        private int _depth = ComplexSpecificParameters.maxDepthAllowable;
        internal override FormattingCommandLineParameters GetCommandLineParameters()
            FormattingCommandLineParameters parameters = new();
            // Check View conflicts first (before any auto-expansion)
            if (!string.IsNullOrEmpty(this.View))
                // View cannot be used with Property or ExcludeProperty
                if ((_props is not null && _props.Length != 0) || (ExcludeProperty is not null && ExcludeProperty.Length != 0))
                    ReportCannotSpecifyViewAndProperty();
                parameters.viewName = this.View;
            if (_props != null)
                ParameterProcessor processor = new(new FormatObjectParameterDefinition());
                parameters.mshParameterList = processor.ProcessParameters(_props, invocationContext);
            if (ExcludeProperty is not null)
                parameters.excludePropertyFilter = new PSPropertyExpressionFilter(ExcludeProperty);
                if (_props is null || _props.Length == 0)
                    parameters.mshParameterList = processor.ProcessParameters(new object[] { "*" }, invocationContext);
            parameters.groupByParameter = this.ProcessGroupByParameter();
            parameters.forceFormattingAlsoOnOutOfBand = this.Force;
            if (this.showErrorsAsMessages.HasValue)
                parameters.showErrorsAsMessages = this.showErrorsAsMessages;
            if (this.showErrorsInFormattedOutput.HasValue)
                parameters.showErrorsInFormattedOutput = this.showErrorsInFormattedOutput;
            parameters.expansion = ProcessExpandParameter();
            ComplexSpecificParameters csp = new();
            csp.maxDepth = _depth;
            parameters.shapeParameters = csp;
            return parameters;

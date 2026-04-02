    /// Implementation for the Format-Wide command.
    [Cmdlet(VerbsCommon.Format, "Wide", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096930")]
    public class FormatWideCommand : OuterFormatShapeCommandBase
        /// Initializes a new instance of the <see cref="FormatWideCommand"/> class
        public FormatWideCommand()
            this.implementation = new InnerFormatShapeCommand(FormatShape.Wide);
        /// Positional parameter for properties, property sets and table sets specified on the command line.
        /// The parameter is optional, since the defaults will be determined using property sets, etc.
        public object Property
            get { return _prop; }
            set { _prop = value; }
        private object _prop;
        /// Gets or sets a value indicating whether to autosize the output.
        public SwitchParameter AutoSize
            get => _autosize.GetValueOrDefault();
            set => _autosize = value;
        private bool? _autosize = null;
        /// Optional, non positional parameter.
        public int Column
            get => _column.GetValueOrDefault(-1);
            set => _column = value;
        private int? _column = null;
                if (_prop is not null || (ExcludeProperty is not null && ExcludeProperty.Length != 0))
            if (_prop != null)
                ParameterProcessor processor = new(new FormatWideParameterDefinition());
                parameters.mshParameterList = processor.ProcessParameters(new object[] { _prop }, invocationContext);
                if (_prop is null)
            // we cannot specify -column and -autosize, they are mutually exclusive
            if (AutoSize && _column.HasValue)
                // the user specified -autosize:true AND a column number
                string msg = StringUtil.Format(FormatAndOut_format_xxx.CannotSpecifyAutosizeAndColumnsError);
                    new InvalidDataException(),
                    "FormatCannotSpecifyAutosizeAndColumns",
            if (_autosize.HasValue)
                parameters.autosize = _autosize.Value;
            WideSpecificParameters wideSpecific = new();
            parameters.shapeParameters = wideSpecific;
            if (_column.HasValue)
                wideSpecific.columns = _column.Value;

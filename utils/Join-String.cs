namespace Microsoft.PowerShell.Commands.Utility
    /// Join-Object implementation.
    [Cmdlet(VerbsCommon.Join, "String", RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "default")]
    public sealed class JoinStringCommand : PSCmdlet
        /// <summary>A bigger default to not get re-allocations in common use cases.</summary>
        private const int DefaultOutputStringCapacity = 256;
        private readonly StringBuilder _outputBuilder = new(DefaultOutputStringCapacity);
        private CultureInfo _cultureInfo = CultureInfo.InvariantCulture;
        private string _separator;
        private char _quoteChar;
        private bool _firstInputObject = true;
        /// Gets or sets the property name or script block to use as the value to join.
        [ArgumentCompleter(typeof(PropertyNameCompleter))]
        public PSPropertyExpression Property { get; set; }
        /// Gets or sets the delimiter to join the output with.
        [ArgumentCompleter(typeof(SeparatorArgumentCompleter))]
        public string Separator
            get => _separator ?? LanguagePrimitives.ConvertTo<string>(GetVariableValue("OFS"));
            set => _separator = value;
        /// Gets or sets text to include before the joined input text.
        [Alias("op")]
        public string OutputPrefix { get; set; }
        /// Gets or sets text to include after the joined input text.
        [Alias("os")]
        public string OutputSuffix { get; set; }
        /// Gets or sets if the output items should we wrapped in single quotes.
        [Parameter(ParameterSetName = "SingleQuote")]
        public SwitchParameter SingleQuote { get; set; }
        /// Gets or sets if the output items should we wrapped in double quotes.
        [Parameter(ParameterSetName = "DoubleQuote")]
        public SwitchParameter DoubleQuote { get; set; }
        /// Gets or sets a format string that is applied to each input object.
        [Parameter(ParameterSetName = "Format")]
        [ArgumentCompleter(typeof(FormatStringArgumentCompleter))]
        public string FormatString { get; set; }
        /// Gets or sets if the current culture should be used with formatting instead of the invariant culture.
        /// Gets or sets the input object to join into text.
        /// <inheritdoc/>
            _quoteChar = SingleQuote ? '\'' : DoubleQuote ? '"' : char.MinValue;
            _outputBuilder.Append(OutputPrefix);
            if (UseCulture)
                _cultureInfo = CultureInfo.CurrentCulture;
                    if (inputObject != null && inputObject != AutomationNull.Value)
                        var inputValue = Property == null
                                            ? inputObject
                                            : Property.GetValues(inputObject, false, true).FirstOrDefault()?.Result;
                        // conversion to string always succeeds.
                        if (!LanguagePrimitives.TryConvertTo<string>(inputValue, _cultureInfo, out var stringValue))
                            throw new PSInvalidCastException("InvalidCastFromAnyTypeToString", ExtendedTypeSystem.InvalidCastCannotRetrieveString, null);
                        if (_firstInputObject)
                            _firstInputObject = false;
                            _outputBuilder.Append(Separator);
                        if (_quoteChar != char.MinValue)
                            _outputBuilder.Append(_quoteChar);
                            _outputBuilder.Append(stringValue);
                        else if (string.IsNullOrEmpty(FormatString))
                            _outputBuilder.AppendFormat(_cultureInfo, FormatString, inputValue);
            _outputBuilder.Append(OutputSuffix);
            WriteObject(_outputBuilder.ToString());
    /// Provides completion for the Separator parameter of the Join-String cmdlet.
    public sealed class SeparatorArgumentCompleter : IArgumentCompleter
        private const string NewLineText =
        "`n";
        "`r`n";
        private static readonly CompletionHelpers.CompletionDisplayInfoMapper SeparatorDisplayInfoMapper = separator => separator switch
            "," => (
                ToolTip: TabCompletionStrings.SeparatorCommaToolTip,
                ListItemText: "Comma"),
            ", " => (
                ToolTip: TabCompletionStrings.SeparatorCommaSpaceToolTip,
                ListItemText: "Comma-Space"),
            ";" => (
                ToolTip: TabCompletionStrings.SeparatorSemiColonToolTip,
                ListItemText: "Semi-Colon"),
            "; " => (
                ToolTip: TabCompletionStrings.SeparatorSemiColonSpaceToolTip,
                ListItemText: "Semi-Colon-Space"),
            "-" => (
                ToolTip: TabCompletionStrings.SeparatorDashToolTip,
                ListItemText: "Dash"),
            " " => (
                ToolTip: TabCompletionStrings.SeparatorSpaceToolTip,
                ListItemText: "Space"),
            NewLineText => (
                ToolTip: StringUtil.Format(TabCompletionStrings.SeparatorNewlineToolTip, NewLineText),
                ListItemText: "Newline"),
                ToolTip: separator,
                ListItemText: separator),
        private static readonly IReadOnlyList<string> s_separatorValues = new List<string>(capacity: 7)
            ",",
            ", ",
            ";",
            "; ",
            NewLineText,
            "-",
            " ",
        /// Returns completion results for Separator parameter.
                    possibleCompletionValues: s_separatorValues,
                    displayInfoMapper: SeparatorDisplayInfoMapper,
                    resultType: CompletionResultType.ParameterValue);
    /// Provides completion for the FormatString parameter of the Join-String cmdlet.
    public sealed class FormatStringArgumentCompleter : IArgumentCompleter
        private static readonly IReadOnlyList<string> s_formatStringValues = new List<string>(capacity: 4)
            "[{0}]",
            "{0:N2}",
            "`n    `${0}",
            "`n    [string] `${0}",
            "`r`n    `${0}",
            "`r`n    [string] `${0}",
        /// Returns completion results for FormatString parameter.
                    possibleCompletionValues: s_formatStringValues,
                    matchStrategy: CompletionHelpers.WildcardPatternEscapeMatch);

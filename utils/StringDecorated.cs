    /// Extensions to String type to calculate and render decorated content.
    public class StringDecorated
        private readonly bool _isDecorated;
        private readonly string _text;
        private string? _plaintextcontent;
        private string PlainText
                _plaintextcontent ??= ValueStringDecorated.AnsiRegex.Replace(_text, string.Empty);
                return _plaintextcontent;
        /// Initializes a new instance of the <see cref="StringDecorated"/> class.
        /// <param name="text">The input string.</param>
        public StringDecorated(string text)
            _text = text;
            _isDecorated = text.Contains(ValueStringDecorated.ESC);
        /// Gets a value indicating whether the string contains decoration.
        /// <returns>Boolean if the string contains decoration.</returns>
        public bool IsDecorated => _isDecorated;
        /// Gets the length of content sans escape sequences.
        /// <returns>Length of content sans escape sequences.</returns>
        public int ContentLength => PlainText.Length;
        /// Render the decorarted string using automatic output rendering.
        /// <returns>Rendered string based on automatic output rendering.</returns>
        public override string ToString() => ToString(
            PSStyle.Instance.OutputRendering == OutputRendering.PlainText
                ? OutputRendering.PlainText
                : OutputRendering.Ansi);
        /// Return string representation of content depending on output rendering mode.
        /// <param name="outputRendering">Specify how to render the text content.</param>
        /// <returns>Rendered string based on outputRendering.</returns>
        public string ToString(OutputRendering outputRendering)
            if (outputRendering == OutputRendering.Host)
                throw new ArgumentException(StringDecoratedStrings.RequireExplicitRendering);
            if (!_isDecorated)
                return _text;
            return outputRendering == OutputRendering.PlainText ? PlainText : _text;
    internal struct ValueStringDecorated
        internal const char ESC = '\x1b';
        private Dictionary<int, int>? _vtRanges;
                _plaintextcontent ??= AnsiRegex.Replace(_text, string.Empty);
        // graphics/color mode ESC[1;2;...m
        private const string GraphicsRegex = @"(\x1b\[\d*(;\d+)*m)";
        // CSI escape sequences
        private const string CsiRegex = @"(\x1b\[\?\d+[hl])";
        // Hyperlink escape sequences. Note: '.*?' makes '.*' do non-greedy match.
        private const string HyperlinkRegex = @"(\x1b\]8;;.*?\x1b\\)";
        // replace regex with .NET 6 API once available
        internal static readonly Regex AnsiRegex = new Regex($"{GraphicsRegex}|{CsiRegex}|{HyperlinkRegex}", RegexOptions.Compiled);
        /// Get the ranges of all escape sequences in the text.
        /// A dictionary with the key being the starting index of an escape sequence,
        /// and the value being the length of the escape sequence.
        internal Dictionary<int, int>? EscapeSequenceRanges
                if (_isDecorated && _vtRanges is null)
                    _vtRanges = new Dictionary<int, int>();
                    foreach (Match match in AnsiRegex.Matches(_text))
                        _vtRanges.Add(match.Index, match.Length);
                return _vtRanges;
        /// Initializes a new instance of the <see cref="ValueStringDecorated"/> struct.
        public ValueStringDecorated(string text)
            _isDecorated = text.Contains(ESC);
            _plaintextcontent = _isDecorated ? null : text;
            _vtRanges = null;

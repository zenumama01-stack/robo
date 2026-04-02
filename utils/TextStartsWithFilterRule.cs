    /// The TextStartsWithFilterRule class evaluates a string item to
    /// check if it starts with the rule's value.
    public class TextStartsWithFilterRule : TextFilterRule
        private static readonly string TextStartsWithCharactersRegexPattern = "^{0}";
        private static readonly string TextStartsWithWordsRegexPattern = TextStartsWithCharactersRegexPattern + WordBoundaryRegexPattern;
        /// Initializes a new instance of the <see cref="TextStartsWithFilterRule"/> class.
        public TextStartsWithFilterRule()
            this.DisplayName = UICultureResources.FilterRule_TextStartsWith;
        public TextStartsWithFilterRule(TextStartsWithFilterRule source)
        /// Determines if data starts with Value.
        /// Returns true is data starts with Value, false otherwise.
            return this.ExactMatchEvaluate(data, TextStartsWithCharactersRegexPattern, TextStartsWithWordsRegexPattern);

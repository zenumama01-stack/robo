    /// The TextEndsWithFilterRule class evaluates a string item to
    /// check if it ends with the rule's value.
    public class TextEndsWithFilterRule : TextFilterRule
        private static readonly string TextEndsWithCharactersRegexPattern = "{0}$";
        private static readonly string TextEndsWithWordsRegexPattern = WordBoundaryRegexPattern + TextEndsWithCharactersRegexPattern;
        /// Initializes a new instance of the <see cref="TextEndsWithFilterRule"/> class.
        public TextEndsWithFilterRule()
            this.DisplayName = UICultureResources.FilterRule_TextEndsWith;
        public TextEndsWithFilterRule(TextEndsWithFilterRule source)
        /// Determines if data ends with Value.
        /// The value to compare with.
        /// Returns true is data ends with Value, false otherwise.
            return this.ExactMatchEvaluate(data, TextEndsWithCharactersRegexPattern, TextEndsWithWordsRegexPattern);

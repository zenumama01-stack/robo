    /// The TextEqualsFilterRule class evaluates a string item to
    public class TextEqualsFilterRule : TextFilterRule
        private static readonly string TextEqualsCharactersRegexPattern = "^{0}$";
        /// Initializes a new instance of the <see cref="TextEqualsFilterRule"/> class.
        public TextEqualsFilterRule()
        public TextEqualsFilterRule(TextEqualsFilterRule source)
        /// Determines if data is equal to Value.
        /// Returns true is data equals Value, false otherwise.
            return this.ExactMatchEvaluate(data, TextEqualsCharactersRegexPattern, TextEqualsCharactersRegexPattern);

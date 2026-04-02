    /// The TextContainsFilterRule class evaluates a string item to
    /// check if it is contains the rule's value within it.
    public class TextContainsFilterRule : TextFilterRule
        /// Initializes a new instance of the <see cref="TextContainsFilterRule"/> class.
        public TextContainsFilterRule()
            this.DisplayName = UICultureResources.FilterRule_Contains;
        public TextContainsFilterRule(TextContainsFilterRule source)
        /// Determines if Value is contained within data.
        /// The data to compare with.
        /// Returns true if data contains Value, false otherwise.
            // True "text contains": \\
            return this.ExactMatchEvaluate(data, TextContainsCharactersRegexPattern, TextContainsWordsRegexPattern);

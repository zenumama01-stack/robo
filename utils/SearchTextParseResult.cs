    /// Represents the result of search text parsing.
    public class SearchTextParseResult
        /// Initializes a new instance of <see cref="SearchTextParseResult"/> with the specified <see cref="FilterRule"/>.
        /// <param name="rule">The rule that resulted from parsing the search text.</param>
        public SearchTextParseResult(FilterRule rule)
            this.FilterRule = rule;
        /// Gets the rule that resulted from parsing the search text.
        public FilterRule FilterRule

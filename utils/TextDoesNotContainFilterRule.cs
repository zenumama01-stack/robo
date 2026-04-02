    /// The TextDoesNotContainFilterRule class evaluates a string item to
    /// check if it is does not contain the rule's value within it.
    public class TextDoesNotContainFilterRule : TextContainsFilterRule
        /// Initializes a new instance of the <see cref="TextDoesNotContainFilterRule"/> class.
        public TextDoesNotContainFilterRule()
            this.DisplayName = UICultureResources.FilterRule_DoesNotContain;
        public TextDoesNotContainFilterRule(TextDoesNotContainFilterRule source)
        /// Determines if Value is not contained within data.
        /// Returns true if data does not contain Value, false otherwise.

    /// The FilterExpressionOperandNode class is responsible for holding a
    /// FilterRule within the FilterExpression tree.
    public class FilterExpressionOperandNode : FilterExpressionNode
        /// The FilterRule to evaluate.
        public FilterRule Rule
            protected set;
        /// Initializes a new instance of the FilterExpressionOperandNode
        /// The FilterRule to hold for evaluation.
        public FilterExpressionOperandNode(FilterRule rule)
            this.Rule = rule;
        /// Evaluates the item against the contained FilterRule.
        /// The item to pass to the contained FilterRule.
        /// Returns true if the contained FilterRule evaluates to
        /// true, false otherwise.
            Debug.Assert(this.Rule != null, "rule is not null");
            return this.Rule.Evaluate(item);

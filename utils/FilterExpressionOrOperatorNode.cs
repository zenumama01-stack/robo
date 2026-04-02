    /// The FilterExpressionOrOperatorNode class is responsible for containing children
    /// FilterExpressionNodes which will be OR'ed together during evaluation.
    public class FilterExpressionOrOperatorNode : FilterExpressionNode
        /// Initializes a new instance of the FilterExpressionOrOperatorNode
        public FilterExpressionOrOperatorNode()
        /// FilterExpressionOrOperatorNode's Children collection.
        public FilterExpressionOrOperatorNode(IEnumerable<FilterExpressionNode> children)
        /// the OR'ed result of their results.
        /// True if any FilterExpressionNode child evaluates to true,
                if (node.Evaluate(item))

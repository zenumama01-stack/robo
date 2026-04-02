    /// The FilterExpressionAndOperatorNode class is responsible for containing children
    /// FilterExpressionNodes which will be AND'ed together during evaluation.
    public class FilterExpressionAndOperatorNode : FilterExpressionNode
        private List<FilterExpressionNode> children = new List<FilterExpressionNode>();
        /// Gets a collection FilterExpressionNode children used during evaluation.
        public ICollection<FilterExpressionNode> Children
                return this.children;
        #region Ctor
        /// Initializes a new instance of the FilterExpressionAndOperatorNode
        public FilterExpressionAndOperatorNode()
        /// <param name="children">
        /// A collection of children which will be added to the
        /// FilterExpressionAndOperatorNode's Children collection.
        public FilterExpressionAndOperatorNode(IEnumerable<FilterExpressionNode> children)
            ArgumentNullException.ThrowIfNull(children);
            this.children.AddRange(children);
        #endregion Ctor
        /// Evaluates the children FilterExpressionNodes and returns
        /// the AND'ed result of their results.
        /// The item to evaluate against.
        /// True if all FilterExpressionNode children evaluate to true,
        /// false otherwise.
        public override bool Evaluate(object item)
            if (this.Children.Count == 0)
            foreach (FilterExpressionNode node in this.Children)
                if (!node.Evaluate(item))

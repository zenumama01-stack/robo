    /// The IFilterExpressionProvider interface defines the contract between
    /// providers of FilterExpressions and consumers thereof.
    public interface IFilterExpressionProvider
        /// Gets a FilterExpression representing the current
        /// relational organization of FilterRules for this provider.
        FilterExpressionNode FilterExpression
        bool HasFilterExpression
        /// Raised when the FilterExpression of this provider
        /// has changed.
        event EventHandler FilterExpressionChanged;

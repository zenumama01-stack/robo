    /// The IsNotEmptyFilterRule evaluates an item to determine whether it
    public class IsNotEmptyFilterRule : IsEmptyFilterRule
        /// Initializes a new instance of the <see cref="IsNotEmptyFilterRule"/> class.
        public IsNotEmptyFilterRule()
            this.DisplayName = UICultureResources.FilterRule_IsNotEmpty;
        public IsNotEmptyFilterRule(IsNotEmptyFilterRule source)
        /// Gets a values indicating whether the supplied item is not empty.
        /// Returns false if the item is null or if the item is a string
        /// composed of whitespace. True otherwise.
            return !base.Evaluate(item);

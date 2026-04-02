    /// The IsGreaterThanFilterRule class evaluates an IComparable item to
    /// check if it is greater than its value.
    public class IsGreaterThanFilterRule<T> : SingleValueComparableValueFilterRule<T> where T : IComparable
        /// Initializes a new instance of the <see cref="IsGreaterThanFilterRule{T}"/> class.
        public IsGreaterThanFilterRule()
            this.DisplayName = UICultureResources.FilterRule_GreaterThanOrEqual;
        public IsGreaterThanFilterRule(IsGreaterThanFilterRule<T> source)
        /// Determines if item is greater than Value.
        /// Returns true if data is greater than Value.
            return result <= 0;

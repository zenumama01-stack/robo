    /// The IsLessThanFilterRule class evaluates an IComparable item to
    /// check if it is less than the rule's value.
    public class IsLessThanFilterRule<T> : SingleValueComparableValueFilterRule<T> where T : IComparable
        /// Initializes a new instance of the <see cref="IsLessThanFilterRule{T}"/> class.
        public IsLessThanFilterRule()
            this.DisplayName = UICultureResources.FilterRule_LessThanOrEqual;
        public IsLessThanFilterRule(IsLessThanFilterRule<T> source)
        /// Determines if item is less than Value.
        /// Returns true if data is less than Value.
        protected override bool Evaluate(T item)
            int result = CustomTypeComparer.Compare<T>(this.Value.GetCastValue(), item);
            return result >= 0;

    /// The EqualsFilterRule class evaluates an IComparable item to
    /// check if it is equal to the rule's value.
    public class EqualsFilterRule<T> : SingleValueComparableValueFilterRule<T> where T : IComparable
        /// Initializes a new instance of the <see cref="EqualsFilterRule{T}"/> class.
        public EqualsFilterRule()
            this.DisplayName = UICultureResources.FilterRule_Equals;
        public EqualsFilterRule(EqualsFilterRule<T> source)
        /// Determines if item is equal to Value.
        /// Returns true if data is equal to Value.
            Debug.Assert(this.IsValid, "isValid");
            int result = CustomTypeComparer.Compare<T>(this.Value.GetCastValue(), data);
            return result == 0;

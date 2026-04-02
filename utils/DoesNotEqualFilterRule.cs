    /// The DoesNotEqualFilterRule class evaluates an IComparable item to
    /// check if it is not equal to the rule's value.
    public class DoesNotEqualFilterRule<T> : EqualsFilterRule<T> where T : IComparable
        /// Initializes a new instance of the <see cref="DoesNotEqualFilterRule{T}"/> class.
        public DoesNotEqualFilterRule()
            this.DisplayName = UICultureResources.FilterRule_DoesNotEqual;
            this.DefaultNullValueEvaluation = true;
        public DoesNotEqualFilterRule(DoesNotEqualFilterRule<T> source)
        /// Determines if item is not equal to Value.
        /// The data to compare against.
        /// Returns true if data is not equal to Value, false otherwise.
        protected override bool Evaluate(T data)
            return !base.Evaluate(data);

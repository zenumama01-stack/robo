    /// The ComparableValueFilterRule provides support for derived classes
    /// that evaluate against IComparable values.
    public abstract class ComparableValueFilterRule<T> : FilterRule where T : IComparable
        /// Initializes a new instance of the <see cref="ComparableValueFilterRule{T}"/> class.
        protected ComparableValueFilterRule()
        protected ComparableValueFilterRule(ComparableValueFilterRule<T> source)
            this.DefaultNullValueEvaluation = source.DefaultNullValueEvaluation;
        /// Gets or sets a value indicating whether null objects passed to Evaluate will
        /// evaluate to true or false.
        protected bool DefaultNullValueEvaluation
        /// Determines if item matches a derived classes criteria.
        /// The item to match evaluate.
        /// Returns true if the item matches, false otherwise.
                return this.DefaultNullValueEvaluation;
            T castItem;
            if (!FilterUtilities.TryCastItem<T>(item, out castItem))
            return this.Evaluate(castItem);
        /// <param name="data">
        protected abstract bool Evaluate(T data);

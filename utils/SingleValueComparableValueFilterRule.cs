    /// The SingleValueComparableValueFilterRule provides support for derived classes
    /// that take a single input and evaluate against IComparable values.
    /// <typeparam name="T">The generic parameter.</typeparam>
    public abstract class SingleValueComparableValueFilterRule<T> : ComparableValueFilterRule<T> where T : IComparable
        /// Gets a value that holds user input.
        public ValidatingValue<T> Value
                return this.Value.IsValid;
        /// Initializes a new instance of the <see cref="SingleValueComparableValueFilterRule{T}"/> class.
        protected SingleValueComparableValueFilterRule()
            this.Value = new ValidatingValue<T>();
            this.Value.PropertyChanged += this.Value_PropertyChanged;
        protected SingleValueComparableValueFilterRule(SingleValueComparableValueFilterRule<T> source)
            this.Value = (ValidatingValue<T>)source.Value.DeepClone();

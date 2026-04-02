    /// The base class for all filtering rules.
    public abstract class FilterRule : IEvaluate, IDeepCloneable
        /// Gets a value indicating whether the FilterRule can be
        /// evaluated in its current state.
        public virtual bool IsValid
        /// Gets a display friendly name for the FilterRule.
        public string DisplayName
        /// Initializes a new instance of the <see cref="FilterRule"/> class.
        protected FilterRule()
        protected FilterRule(FilterRule source)
            this.DisplayName = source.DisplayName;
        public object DeepClone()
            return Activator.CreateInstance(this.GetType(), new object[] { this });
        /// Gets a value indicating whether the supplied item meets the
        /// criteria specified by this rule.
        /// <param name="item">The item to evaluate.</param>
        /// <returns>Returns true if the item meets the criteria. False otherwise.</returns>
        #region EvaluationResultInvalidated
        /// Occurs when the values of this rule changes.
        public event EventHandler EvaluationResultInvalidated;
        /// Fires <see cref="EvaluationResultInvalidated"/>.
        protected void NotifyEvaluationResultInvalidated()
            var eh = this.EvaluationResultInvalidated;

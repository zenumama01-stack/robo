using System.Runtime.Serialization;
    /// The IsBetweenFilterRule class evaluates an item to see if it is between
    /// the StartValue and EndValue of the rule.
    public class IsBetweenFilterRule<T> : ComparableValueFilterRule<T> where T : IComparable
        public override bool IsValid
                return this.StartValue.IsValid && this.EndValue.IsValid;
        /// Gets the start value for the range.
        public ValidatingValue<T> StartValue
        /// Gets the end value for the range.
        public ValidatingValue<T> EndValue
        /// Initializes a new instance of the <see cref="IsBetweenFilterRule{T}"/> class.
        public IsBetweenFilterRule()
            this.DisplayName = UICultureResources.FilterRule_IsBetween;
            this.StartValue = new ValidatingValue<T>();
            this.StartValue.PropertyChanged += this.Value_PropertyChanged;
            this.EndValue = new ValidatingValue<T>();
            this.EndValue.PropertyChanged += this.Value_PropertyChanged;
        public IsBetweenFilterRule(IsBetweenFilterRule<T> source)
            this.StartValue = (ValidatingValue<T>)source.StartValue.DeepClone();
            this.EndValue = (ValidatingValue<T>)source.EndValue.DeepClone();
        /// Evaluates data and determines if it is between
        /// StartValue and EndValue.
        /// The data to evaluate.
        /// Returns true if data is between StartValue and EndValue,
            Debug.Assert(this.IsValid, "is valid");
            int startValueComparedToData = CustomTypeComparer.Compare<T>(this.StartValue.GetCastValue(), data);
            int endValueComparedToData = CustomTypeComparer.Compare<T>(this.EndValue.GetCastValue(), data);
            bool isBetweenForward = startValueComparedToData < 0 && endValueComparedToData > 0;
            bool isBetweenBackwards = endValueComparedToData < 0 && startValueComparedToData > 0;
            return isBetweenForward || isBetweenBackwards;
        #region Value Change Handlers
        private void Value_PropertyChanged(object sender, PropertyChangedEventArgs e)
            if (e.PropertyName == "Value")
                this.NotifyEvaluationResultInvalidated();
        #endregion Value Change Handlers

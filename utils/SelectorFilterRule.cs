    /// The SelectorFilterRule represents a rule composed of other rules.
    public class SelectorFilterRule : FilterRule
        /// Gets a value indicating whether the rule can be evaluated.
                return this.AvailableRules.IsValid && this.AvailableRules.SelectedValue.IsValid;
        /// Gets the collection of available rules.
        public ValidatingSelectorValue<FilterRule> AvailableRules
        /// Initializes a new instance of the <see cref="SelectorFilterRule"/> class.
        public SelectorFilterRule()
            this.AvailableRules = new ValidatingSelectorValue<FilterRule>();
            this.AvailableRules.SelectedValueChanged += this.AvailableRules_SelectedValueChanged;
        public SelectorFilterRule(SelectorFilterRule source)
            this.AvailableRules = (ValidatingSelectorValue<FilterRule>)source.AvailableRules.DeepClone();
            this.AvailableRules.SelectedValue.EvaluationResultInvalidated += this.SelectedValue_EvaluationResultInvalidated;
            return this.AvailableRules.SelectedValue.Evaluate(item);
        /// Called when the SelectedValue within AvailableRules changes.
        /// The old FilterRule.
        /// The new FilterRule.
        protected void OnSelectedValueChanged(FilterRule oldValue, FilterRule newValue)
            FilterRuleCustomizationFactory.FactoryInstance.ClearValues(newValue);
            FilterRuleCustomizationFactory.FactoryInstance.TransferValues(oldValue, newValue);
            FilterRuleCustomizationFactory.FactoryInstance.ClearValues(oldValue);
            oldValue.EvaluationResultInvalidated -= this.SelectedValue_EvaluationResultInvalidated;
            newValue.EvaluationResultInvalidated += this.SelectedValue_EvaluationResultInvalidated;
        private void SelectedValue_EvaluationResultInvalidated(object sender, EventArgs e)
        private void AvailableRules_SelectedValueChanged(object sender, PropertyChangedEventArgs<FilterRule> e)
            this.OnSelectedValueChanged(e.OldValue, e.NewValue);

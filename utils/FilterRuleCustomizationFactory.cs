    /// The FilterRuleCustomizationFactory class provides a central location
    /// a return an abstract factory which creates the standard settings and rules
    /// used by the builtin FilterRules.
    public abstract class FilterRuleCustomizationFactory
        private static FilterRuleCustomizationFactory factoryInstance;
        /// Gets or sets a factory instance which is used by builtin
        /// filter rules.
        public static FilterRuleCustomizationFactory FactoryInstance
                Debug.Assert(factoryInstance != null, "factoryInstance not null");
                return factoryInstance;
                factoryInstance = value;
        /// Initializes the static state of the DataErrorInfoValidationRuleFactory class.
        static FilterRuleCustomizationFactory()
            FactoryInstance = new DefaultFilterRuleCustomizationFactory();
        public abstract IPropertyValueGetter PropertyValueGetter
        /// for type T.
        public abstract ICollection<FilterRule> CreateDefaultFilterRulesForPropertyValueSelectorFilterRule<T>() where T : IComparable;
        public abstract void TransferValues(FilterRule oldRule, FilterRule newRule);
        public abstract void ClearValues(FilterRule rule);
        public abstract string GetErrorMessageForInvalidValue(string value, Type typeToParseTo);

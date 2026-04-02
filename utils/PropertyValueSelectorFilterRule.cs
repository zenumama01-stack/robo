    /// The PropertyValueSelectorFilterRule class supports filtering against a
    /// property of an object. Based on the type of the property a collection of
    /// filter rules are available to be used.
    public class PropertyValueSelectorFilterRule<T> : SelectorFilterRule where T : IComparable
        /// Gets the name of the property on the item to evaluate which holds
        /// the real value which should be evaluated.
        public string PropertyName
        /// Creates a new PropertyValueSelectorFilterRule instance.
        /// <param name="propertyDisplayName">
        /// The display friendly representation of the property name.
        public PropertyValueSelectorFilterRule(string propertyName, string propertyDisplayName)
            : this(propertyName, propertyDisplayName, FilterRuleCustomizationFactory.FactoryInstance.CreateDefaultFilterRulesForPropertyValueSelectorFilterRule<T>())
            // Empty
        /// The propertyName on the item to evaluate which holds the real
        /// value which should be evaluated.
        /// The display friendly representation of the propertyName.
        /// <param name="rules">
        /// The collection of available rules.
        public PropertyValueSelectorFilterRule(string propertyName, string propertyDisplayName, IEnumerable<FilterRule> rules)
            this.PropertyName = propertyName;
            this.DisplayName = propertyDisplayName;
            foreach (FilterRule rule in rules)
                if (rule == null)
                    throw new ArgumentException("A value within rules is null", "rules");
                this.AvailableRules.AvailableValues.Add(rule);
            this.AvailableRules.DisplayNameConverter = new FilterRuleToDisplayNameConverter();
        /// Initializes a new instance of the <see cref="PropertyValueSelectorFilterRule{T}"/> class.
        public PropertyValueSelectorFilterRule(PropertyValueSelectorFilterRule<T> source)
            this.PropertyName = source.PropertyName;
        /// Evaluates whether the item is inclusive.
        /// Returns true if the item matches the filtering criteria, false otherwise.
            T propertyValue;
            if (!this.TryGetPropertyValue(item, out propertyValue))
            return this.AvailableRules.SelectedValue.Evaluate(propertyValue);
        private bool TryGetPropertyValue(object item, out T propertyValue)
            propertyValue = default(T);
            Debug.Assert(item != null, "item not null");
            return FilterRuleCustomizationFactory.FactoryInstance.PropertyValueGetter.TryGetPropertyValue<T>(this.PropertyName, item, out propertyValue);

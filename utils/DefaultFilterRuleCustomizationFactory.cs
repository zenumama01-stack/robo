    /// The BuiltinDataErrorInfoValidationRuleFactory creates default settings for the
    /// builtin FilterRules.
    public class DefaultFilterRuleCustomizationFactory : FilterRuleCustomizationFactory
        private IPropertyValueGetter propertyValueGetter;
        /// Gets or sets a <see cref="IPropertyValueGetter"/> that can retrieve the values of properties on a given object.
        public override IPropertyValueGetter PropertyValueGetter
                if (this.propertyValueGetter == null)
                    this.propertyValueGetter = new PropertyValueGetter();
                return this.propertyValueGetter;
                this.propertyValueGetter = value;
        /// Returns a collection containing the default rules used by a PropertyValueSelectorFilterRule
        /// for type t.
        /// The type used to determine what rules to include.
        /// Returns a collection of FilterRules.
        public override ICollection<FilterRule> CreateDefaultFilterRulesForPropertyValueSelectorFilterRule<T>()
            Collection<FilterRule> rules = new Collection<FilterRule>();
            Type t = typeof(T);
            if (t == typeof(string))
                rules.Add(new TextContainsFilterRule());
                rules.Add(new TextDoesNotContainFilterRule());
                rules.Add(new TextStartsWithFilterRule());
                rules.Add(new TextEqualsFilterRule());
                rules.Add(new TextDoesNotEqualFilterRule());
                rules.Add(new TextEndsWithFilterRule());
                rules.Add(new IsEmptyFilterRule());
                rules.Add(new IsNotEmptyFilterRule());
            else if (t == typeof(bool))
                rules.Add(new EqualsFilterRule<T>());
            else if (t.IsEnum)
                rules.Add(new DoesNotEqualFilterRule<T>());
                rules.Add(new IsLessThanFilterRule<T>());
                rules.Add(new IsGreaterThanFilterRule<T>());
                rules.Add(new IsBetweenFilterRule<T>());
            return rules;
        /// Transfers the values from the old rule into the new rule.
        /// <param name="oldRule">
        /// The old filter rule.
        /// <param name="newRule">
        /// The new filter rule.
        public override void TransferValues(FilterRule oldRule, FilterRule newRule)
            ArgumentNullException.ThrowIfNull(oldRule);
            ArgumentNullException.ThrowIfNull(newRule);
            if (this.TryTransferValuesAsSingleValueComparableValueFilterRule(oldRule, newRule))
        /// Clears the values from the filter rule.
        /// <param name="rule">
        /// The rule to clear.
        public override void ClearValues(FilterRule rule)
            ArgumentNullException.ThrowIfNull(rule);
            if (this.TryClearValueFromSingleValueComparableValueFilterRule(rule))
            if (this.TryClearIsBetweenFilterRule(rule))
        /// Get an error message to display to a user when they
        /// provide a string value that cannot be parsed to type
        /// typeToParseTo.
        /// <param name="value">
        /// The value entered by the user.
        /// <param name="typeToParseTo">
        /// The desired type to parse value to.
        /// An error message to a user to explain how they can
        /// enter a valid value.
        public override string GetErrorMessageForInvalidValue(string value, Type typeToParseTo)
            ArgumentNullException.ThrowIfNull(typeToParseTo);
            bool isNumericType = typeToParseTo == typeof(byte)
                || typeToParseTo == typeof(sbyte)
                || typeToParseTo == typeof(short)
                || typeToParseTo == typeof(ushort)
                || typeToParseTo == typeof(int)
                || typeToParseTo == typeof(uint)
                || typeToParseTo == typeof(long)
                || typeToParseTo == typeof(ulong)
                || typeToParseTo == typeof(Single)
                || typeToParseTo == typeof(double);
            if (isNumericType)
                return string.Format(CultureInfo.CurrentCulture, UICultureResources.ErrorMessageForUnparsableNumericType);
            if (typeToParseTo == typeof(DateTime))
                return string.Format(CultureInfo.CurrentCulture, UICultureResources.ErrorMessageForUnparsableDateTimeType, CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
            return string.Format(CultureInfo.CurrentCulture, UICultureResources.ErrorTextBoxTypeConversionErrorText, typeToParseTo.Name);
        #region Helpers
        private bool TryGetGenericParameterForComparableValueFilterRule(FilterRule rule, out Type genericParameter)
            genericParameter = null;
            TextFilterRule textRule = rule as TextFilterRule;
            if (textRule != null)
                genericParameter = typeof(string);
            Type ruleType = rule.GetType();
            if (!ruleType.IsGenericType)
            genericParameter = ruleType.GetGenericArguments()[0];
        private object GetValueFromValidatingValue(FilterRule rule, string propertyName)
            Debug.Assert(rule != null && !string.IsNullOrEmpty(propertyName), "rule and propertyname are not null");
            // NOTE: This isn't needed but OACR is complaining
            PropertyInfo property = ruleType.GetProperty(propertyName);
            object validatingValue = property.GetValue(rule, null);
            property = property.PropertyType.GetProperty("Value");
            return property.GetValue(validatingValue, null);
        private void SetValueOnValidatingValue(FilterRule rule, string propertyName, object value)
            property.SetValue(validatingValue, value, null);
        #endregion Helpers
        #region SingleValueComparableValueFilterRule
        private bool TryTransferValuesAsSingleValueComparableValueFilterRule(FilterRule oldRule, FilterRule newRule)
            Debug.Assert(oldRule != null && newRule != null, "oldrule and newrule are not null");
            bool areCorrectType = this.IsSingleValueComparableValueFilterRule(oldRule) && this.IsSingleValueComparableValueFilterRule(newRule);
            if (!areCorrectType)
            object value = this.GetValueFromValidatingValue(oldRule, "Value");
            this.SetValueOnValidatingValue(newRule, "Value", value);
        private bool TryClearValueFromSingleValueComparableValueFilterRule(FilterRule rule)
            Debug.Assert(rule != null, "rule is not null");
            if (!this.IsSingleValueComparableValueFilterRule(rule))
            this.SetValueOnValidatingValue(rule, "Value", null);
        private bool IsSingleValueComparableValueFilterRule(FilterRule rule)
            Type genericParameter;
            if (!this.TryGetGenericParameterForComparableValueFilterRule(rule, out genericParameter))
            Type baseGenericType = typeof(SingleValueComparableValueFilterRule<>);
            Type baseType = baseGenericType.MakeGenericType(genericParameter);
            return baseType.Equals(ruleType) || ruleType.IsSubclassOf(baseType);
        #endregion SingleValueComparableValueFilterRule
        #region IsBetweenFilterRule
        private bool TryClearIsBetweenFilterRule(FilterRule rule)
            if (!this.IsIsBetweenFilterRule(rule))
            this.SetValueOnValidatingValue(rule, "StartValue", null);
            this.SetValueOnValidatingValue(rule, "EndValue", null);
        private bool IsIsBetweenFilterRule(FilterRule rule)
            Type baseGenericType = typeof(IsBetweenFilterRule<>);
        #endregion IsBetweenFilterRule

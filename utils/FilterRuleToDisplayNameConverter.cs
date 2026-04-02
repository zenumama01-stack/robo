    /// The FilterRuleToDisplayNameConverter is responsible for converting
    /// a FilterRule value to its DisplayName.
    public class FilterRuleToDisplayNameConverter : IValueConverter
        /// Converts a FilterRule value to its DisplayName.
        /// A FilterRule.
        /// <param name="targetType">
        /// Type of String.
        /// <param name="parameter">
        /// <param name="culture">
        /// The display name of the FilterRule.
            FilterRule rule = value as FilterRule;
                throw new ArgumentException("value of type FilterRule expected.", "value");
            return rule.DisplayName;
        /// The method is not used.
        /// The method does not return a value.

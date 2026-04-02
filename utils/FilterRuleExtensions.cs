    /// The FilterRuleExtensions class provides extension methods
    /// for FilterRule classes.
    public static class FilterRuleExtensions
        /// Creates a deep copy of a FilterRule.
        /// The FilterRule to clone.
        /// Returns a deep copy of the passed in rule.
        public static FilterRule DeepCopy(this FilterRule rule)
            return (FilterRule)rule.DeepClone();

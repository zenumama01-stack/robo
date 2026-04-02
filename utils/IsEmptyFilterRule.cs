    /// The IsEmptyFilterRule evaluates an item to determine whether it
    /// is empty or not.
    public class IsEmptyFilterRule : FilterRule
        /// Initializes a new instance of the <see cref="IsEmptyFilterRule"/> class.
        public IsEmptyFilterRule()
            this.DisplayName = UICultureResources.FilterRule_IsEmpty;
        public IsEmptyFilterRule(IsEmptyFilterRule source)
        /// Gets a values indicating whether the supplied item is empty.
        /// Returns true if the item is null or if the item is a string
        /// composed of whitespace. False otherwise.
            Type type = item.GetType();
            if (typeof(string) == type)
                return ((string)item).Trim().Length == 0;

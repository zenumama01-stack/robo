    /// Provides a way to create a custom rule in order to check the validity of user input.
    public abstract class DataErrorInfoValidationRule : IDeepCloneable
        /// When overridden in a derived class, performs validation checks on a value.
        /// The value to check.
        /// The culture to use in this rule.
        /// A DataErrorInfoValidationResult object.
        public abstract DataErrorInfoValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo);

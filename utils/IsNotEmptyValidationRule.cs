    /// The IsNotEmptyValidationRule checks a value to see if a value is not empty.
    public class IsNotEmptyValidationRule : DataErrorInfoValidationRule
        private static readonly DataErrorInfoValidationResult EmptyValueResult = new DataErrorInfoValidationResult(false, null, string.Empty);
        /// Determines if value is not empty.
        /// The value to validate.
        /// <param name="cultureInfo">
        /// The culture info to use while validating.
        /// Returns true if the value is not empty, false otherwise.
        public override DataErrorInfoValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
                return EmptyValueResult;
            Type t = value.GetType();
            if (typeof(string) == t)
                return IsStringNotEmpty((string)value) ? DataErrorInfoValidationResult.ValidResult : EmptyValueResult;
            // Instance is stateless.
            // return this;
            return new IsNotEmptyValidationRule();
        internal static bool IsStringNotEmpty(string value)
            return !(string.IsNullOrEmpty(value) || value.Trim().Length == 0);

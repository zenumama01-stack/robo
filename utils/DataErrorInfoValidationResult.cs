    /// The DataErrorInfoValidationResult supports reporting validation result
    /// data needed for the IDataErrorInfo interface.
    public class DataErrorInfoValidationResult : ValidationResult
        /// Gets a value indicating whether the error should
        /// be presented to the user.
        public bool IsUserVisible
        /// Gets a value used to communicate what the error is.
        public string ErrorMessage
        private static readonly DataErrorInfoValidationResult valid = new DataErrorInfoValidationResult(true, null, string.Empty);
        /// Geta an instance of DataErrorInfoValidationResult that corresponds
        /// to a valid result.
        public static new DataErrorInfoValidationResult ValidResult
                return valid;
        /// Initializes a new instance of the DataErrorInfoValidationResult class.
        /// <param name="isValid">
        /// Indicates whether the value checked against the
        /// DataErrorInfoValidationResult is valid
        /// <param name="errorContent">
        /// Information about the invalidity.
        /// <param name="errorMessage">
        /// The error message to display to the user. If the result is invalid
        /// and the error message is empty (""), the result will be treated as
        /// invalid but no error will be presented to the user.
        public DataErrorInfoValidationResult(bool isValid, object errorContent, string errorMessage)
            : base(isValid, errorContent)
            this.IsUserVisible = !string.IsNullOrEmpty(errorMessage);
            this.ErrorMessage = errorMessage ?? string.Empty;

    /// The exception that is thrown when there is no help category matching
    /// a specific input string.
    public class HelpCategoryInvalidException : ArgumentException, IContainsErrorRecord
        /// Initializes a new instance of the HelpCategoryInvalidException class.
        /// <param name="helpCategory">The name of help category that is invalid.</param>
        public HelpCategoryInvalidException(string helpCategory)
            _helpCategory = helpCategory;
            CreateErrorRecord();
        public HelpCategoryInvalidException()
        /// <param name="innerException">The inner exception of this exception.</param>
        public HelpCategoryInvalidException(string helpCategory, Exception innerException)
                  (innerException != null) ? innerException.Message : string.Empty,
        /// Creates an internal error record based on helpCategory.
        private void CreateErrorRecord()
            _errorRecord = new ErrorRecord(new ParentContainsErrorRecordException(this), "HelpCategoryInvalid", ErrorCategory.InvalidArgument, null);
            _errorRecord.ErrorDetails = new ErrorDetails(typeof(HelpCategoryInvalidException).Assembly, "HelpErrors", "HelpCategoryInvalid", _helpCategory);
        /// Gets ErrorRecord embedded in this exception.
        /// <value>ErrorRecord instance</value>
        private readonly string _helpCategory = System.Management.Automation.HelpCategory.None.ToString();
        /// Gets name of the help category that is invalid.
        /// <value>Name of the help category.</value>
        public string HelpCategory
                return _helpCategory;
        /// Gets exception message for this exception.
        /// <value>Error message.</value>
                if (_errorRecord != null)
                    return _errorRecord.ToString();
                return base.Message;
        protected HelpCategoryInvalidException(SerializationInfo info,

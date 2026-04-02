    /// The exception that is thrown when there is no help found for a topic.
    public class HelpNotFoundException : SystemException, IContainsErrorRecord
        /// Initializes a new instance of the HelpNotFoundException class with the give help topic.
        /// <param name="helpTopic">The help topic for which help is not found.</param>
        public HelpNotFoundException(string helpTopic)
            _helpTopic = helpTopic;
        /// Initializes a new instance of the HelpNotFoundException class.
        public HelpNotFoundException()
        /// Initializes a new instance of the HelpNotFoundException class with the given help topic
        /// and associated exception.
        public HelpNotFoundException(string helpTopic, Exception innerException)
        /// Creates an internal error record based on helpTopic.
        /// The ErrorRecord created will be stored in the _errorRecord member.
            string errMessage = string.Format(HelpErrors.HelpNotFound, _helpTopic);
            // Don't do ParentContainsErrorRecordException(this), as this causes recursion, and creates a
            // segmentation fault on Linux
            _errorRecord = new ErrorRecord(new ParentContainsErrorRecordException(errMessage), "HelpNotFound", ErrorCategory.ResourceUnavailable, null);
            _errorRecord.ErrorDetails = new ErrorDetails(typeof(HelpNotFoundException).Assembly, "HelpErrors", "HelpNotFound", _helpTopic);
        /// <value>ErrorRecord instance.</value>
        private readonly string _helpTopic = string.Empty;
        /// Gets help topic for which help is not found.
        /// <value>Help topic.</value>
        public string HelpTopic
                return _helpTopic;
        protected HelpNotFoundException(SerializationInfo info,

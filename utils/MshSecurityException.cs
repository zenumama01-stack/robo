    /// This is a wrapper for exception class SecurityException.
    public class PSSecurityException : RuntimeException
        /// Recommended constructor for class PSSecurityException.
        public PSSecurityException()
            _errorRecord = new ErrorRecord(
                "UnauthorizedAccess",
            _errorRecord.ErrorDetails = new ErrorDetails(SessionStateStrings.CanNotRun);
            _message = _errorRecord.ErrorDetails.Message;
        /// Serialization constructor for class PSSecurityException.
        protected PSSecurityException(SerializationInfo info,
        /// Constructor for class PSSecurityException.
        public PSSecurityException(string message)
            _errorRecord.ErrorDetails = new ErrorDetails(message);
        public PSSecurityException(string message,
        /// Gets the ErrorRecord information for this exception.
        public override ErrorRecord ErrorRecord
        /// Exception.Message is get-only, but you can effectively
        /// set it in a subclass by overriding this virtual property.
        public override string Message
        private readonly string _message;

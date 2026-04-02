    /// <see cref="System.InvalidOperationException"/>
    public class PSInvalidOperationException
            : InvalidOperationException, IContainsErrorRecord
        /// Initializes a new instance of the PSInvalidOperationException class.
        public PSInvalidOperationException()
        /// Initializes a new instance of the PSInvalidOperationException class
        protected PSInvalidOperationException(SerializationInfo info,
        public PSInvalidOperationException(string message)
        public PSInvalidOperationException(string message,
        internal PSInvalidOperationException(string message, Exception innerException, string errorId, ErrorCategory errorCategory, object target)
            _errorCategory = errorCategory;
                    _target);
        private string _errorId = "InvalidOperation";
        internal void SetErrorId(string errorId)
        private readonly ErrorCategory _errorCategory = ErrorCategory.InvalidOperation;
        private readonly object _target = null;

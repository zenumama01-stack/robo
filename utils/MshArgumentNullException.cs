    /// <see cref="System.ArgumentNullException"/>
    public class PSArgumentNullException
            : ArgumentNullException, IContainsErrorRecord
        /// Initializes a new instance of the PSArgumentNullException class.
        public PSArgumentNullException()
        /// Per MSDN, the parameter is paramName and not message.
        public PSArgumentNullException(string paramName)
            : base(paramName)
        public PSArgumentNullException(string message, Exception innerException)
        /// ArgumentNullException has this ctor form and we imitate it here.
        public PSArgumentNullException(string paramName, string message)
            : base(paramName, message)
        /// Initializes a new instance of the PSArgumentNullException class
        protected PSArgumentNullException(SerializationInfo info,
        private readonly string _errorId = "ArgumentNull";

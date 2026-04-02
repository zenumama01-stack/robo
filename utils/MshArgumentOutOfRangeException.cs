    /// <see cref="System.ArgumentOutOfRangeException"/>
    public class PSArgumentOutOfRangeException
            : ArgumentOutOfRangeException, IContainsErrorRecord
        /// Constructor for class PSArgumentOutOfRangeException.
        public PSArgumentOutOfRangeException()
        /// Initializes a new instance of the PSArgumentOutOfRangeException class.
        public PSArgumentOutOfRangeException(string paramName)
        /// <param name="actualValue"></param>
        /// ArgumentOutOfRangeException has this ctor form and we imitate it here.
        public PSArgumentOutOfRangeException(string paramName, object actualValue, string message)
                : base(paramName, actualValue, message)
        /// Initializes a new instance of the PSArgumentOutOfRangeException class
        protected PSArgumentOutOfRangeException(SerializationInfo info,
        public PSArgumentOutOfRangeException(string message,
        private readonly string _errorId = "ArgumentOutOfRange";

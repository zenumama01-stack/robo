    /// <see cref="System.NotSupportedException"/>
    public class PSNotSupportedException
            : NotSupportedException, IContainsErrorRecord
        /// Initializes a new instance of the PSNotSupportedException class.
        public PSNotSupportedException()
        /// Initializes a new instance of the PSNotSupportedException class
        protected PSNotSupportedException(SerializationInfo info,
        public PSNotSupportedException(string message)
        public PSNotSupportedException(string message,
        private readonly string _errorId = "NotSupported";

    /// <see cref="System.NotImplementedException"/>
    public class PSNotImplementedException
            : NotImplementedException, IContainsErrorRecord
        /// Initializes a new instance of the PSNotImplementedException class.
        public PSNotImplementedException()
        /// Initializes a new instance of the PSNotImplementedException class
        protected PSNotImplementedException(SerializationInfo info,
        public PSNotImplementedException(string message)
        public PSNotImplementedException(string message,
        private readonly string _errorId = "NotImplemented";

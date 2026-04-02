    /// <see cref="System.ObjectDisposedException"/>
    public class PSObjectDisposedException
            : ObjectDisposedException, IContainsErrorRecord
        /// Initializes a new instance of the PSObjectDisposedException class.
        /// <param name="objectName"></param>
        /// Per MSDN, the parameter is objectName and not message.
        /// Also note that there is no parameterless constructor.
        public PSObjectDisposedException(string objectName)
            : base(objectName)
        public PSObjectDisposedException(string objectName, string message)
                : base(objectName, message)
        public PSObjectDisposedException(string message, Exception innerException)
        /// Initializes a new instance of the PSObjectDisposedException class
        protected PSObjectDisposedException(SerializationInfo info, StreamingContext context) : base(info, context)
        private readonly string _errorId = "ObjectDisposed";

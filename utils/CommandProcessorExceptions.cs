    /// Defines the exception that is thrown if a native command fails.
    public class ApplicationFailedException : RuntimeException
        private const string errorIdString = "NativeCommandFailed";
        /// Initializes a new instance of the ApplicationFailedException class and defines the serialization information,
        /// and streaming context.
        /// <param name="info">The serialization information to use when initializing this object.</param>
        /// <param name="context">The streaming context to use when initializing this object.</param>
        protected ApplicationFailedException(SerializationInfo info,
        /// Initializes a new instance of the class ApplicationFailedException.
        public ApplicationFailedException() : base()
            base.SetErrorId(errorIdString);
            base.SetErrorCategory(ErrorCategory.ResourceUnavailable);
        /// Initializes a new instance of the ApplicationFailedException class and defines the error message.
        /// <param name="message">The error message to use when initializing this object.</param>
        public ApplicationFailedException(string message) : base(message)
        /// Initializes a new instance of the ApplicationFailedException class and defines the error message and
        /// errorID.
        /// <param name="errorId">The errorId to use when initializing this object.</param>
        internal ApplicationFailedException(string message, string errorId) : base(message)
            base.SetErrorId(errorId);
        /// Initializes a new instance of the ApplicationFailedException class and defines the error message,
        /// error ID and inner exception.
        /// <param name="innerException">The inner exception to use when initializing this object.</param>
        internal ApplicationFailedException(string message, string errorId, Exception innerException)
        /// inner exception.
        public ApplicationFailedException(string message,

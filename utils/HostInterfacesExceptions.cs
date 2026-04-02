    /// Defines the exception thrown when the Host cannot complete an operation
    /// such as checking whether there is any input available.
    class HostException : RuntimeException
        /// Initializes a new instance of the HostException class.
        HostException()
            : base(StringUtil.Format(HostInterfaceExceptionsStrings.DefaultCtorMessageTemplate, typeof(HostException).FullName))
        /// Initializes a new instance of the HostException class and defines the error message.
        HostException(string message)
        /// Initializes a new instance of the HostException class and defines the error message and
        /// The exception that is the cause of the current exception. If the <paramref name="innerException"/>
        /// parameter is not a null reference, the current exception is raised in a catch
        /// block that handles the inner exception.
        HostException(string message, Exception innerException)
        /// Initializes a new instance of the HostException class and defines the error message,
        /// inner exception, the error ID, and the error category.
        /// The string that should uniquely identifies the situation where the exception is thrown.
        /// The string should not contain white space.
        /// The ErrorCategory into which this exception situation falls
        /// Intentionally public, third-party hosts can call this
        HostException(
            ErrorCategory errorCategory)
            SetErrorCategory(errorCategory);
        /// Initializes a new instance of the HostException class and defines the SerializationInfo
        /// and the StreamingContext.
        /// The object that holds the serialized object data.
        /// The contextual information about the source or destination.
        HostException(SerializationInfo info, StreamingContext context)
            SetErrorId(typeof(HostException).FullName);
    /// Defines the exception thrown when an error occurs from prompting for a command parameter.
    class PromptingException : HostException
        /// Initializes a new instance of the PromptingException class.
        PromptingException()
            : base(StringUtil.Format(HostInterfaceExceptionsStrings.DefaultCtorMessageTemplate, typeof(PromptingException).FullName))
        /// Initializes a new instance of the PromptingException class and defines the error message.
        PromptingException(string message)
        /// Initializes a new instance of the PromptingException class and defines the error message and
        PromptingException(string message, Exception innerException)
        /// Initializes a new instance of the PromptingException class and defines the error message,
        PromptingException(
            : base(message, innerException, errorId, errorCategory)
        PromptingException(SerializationInfo info, StreamingContext context)
            SetErrorId(typeof(PromptingException).FullName);

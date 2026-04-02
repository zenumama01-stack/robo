    /// Defines the exception thrown for all Extended type system related errors.
    public class ExtendedTypeSystemException : RuntimeException
        /// Initializes a new instance of ExtendedTypeSystemException with the message set
        /// to typeof(ExtendedTypeSystemException).FullName.
        public ExtendedTypeSystemException()
            : base(typeof(ExtendedTypeSystemException).FullName)
        /// Initializes a new instance of ExtendedTypeSystemException setting the message.
        /// <param name="message">The exception's message.</param>
        public ExtendedTypeSystemException(string message)
        /// Initializes a new instance of ExtendedTypeSystemException setting the message and innerException.
        /// <param name="innerException">The exception's inner exception.</param>
        public ExtendedTypeSystemException(string message, Exception innerException)
        /// Recommended constructor for the class.
        /// <param name="errorId">String that uniquely identifies each thrown Exception.</param>
        /// <param name="innerException">The inner exception, null for none.</param>
        /// <param name="arguments">Arguments to the resource string.</param>
        internal ExtendedTypeSystemException(
            params object[] arguments)
                  StringUtil.Format(resourceString, arguments),
                  innerException)
        /// Initializes a new instance of ExtendedTypeSystemException with serialization parameters.
        protected ExtendedTypeSystemException(SerializationInfo info, StreamingContext context)
            : base(info, context)
    /// Defines the exception thrown for Method related errors.
    public class MethodException : ExtendedTypeSystemException
        internal const string MethodArgumentCountExceptionMsg = "MethodArgumentCountException";
        internal const string MethodAmbiguousExceptionMsg = "MethodAmbiguousException";
        internal const string MethodArgumentConversionExceptionMsg = "MethodArgumentConversionException";
        internal const string NonRefArgumentToRefParameterMsg = "NonRefArgumentToRefParameter";
        internal const string RefArgumentToNonRefParameterMsg = "RefArgumentToNonRefParameter";
        /// Initializes a new instance of MethodException with the message set
        /// to typeof(MethodException).FullName.
        public MethodException()
            : base(typeof(MethodException).FullName)
        /// Initializes a new instance of MethodException setting the message.
        public MethodException(string message)
        /// Initializes a new instance of MethodException setting the message and innerException.
        public MethodException(string message, Exception innerException)
        /// <param name="innerException">The inner exception.</param>
        internal MethodException(
            : base(errorId, innerException, resourceString, arguments)
        /// Initializes a new instance of MethodException with serialization parameters.
        protected MethodException(SerializationInfo info, StreamingContext context)
    /// Defines the exception thrown for Method invocation exceptions.
    public class MethodInvocationException : MethodException
        internal const string MethodInvocationExceptionMsg = "MethodInvocationException";
        internal const string CopyToInvocationExceptionMsg = "CopyToInvocationException";
        internal const string WMIMethodInvocationException = "WMIMethodInvocationException";
        /// Initializes a new instance of MethodInvocationException with the message set
        /// to typeof(MethodInvocationException).FullName.
        public MethodInvocationException()
            : base(typeof(MethodInvocationException).FullName)
        /// Initializes a new instance of MethodInvocationException setting the message.
        public MethodInvocationException(string message)
        /// Initializes a new instance of MethodInvocationException setting the message and innerException.
        public MethodInvocationException(string message, Exception innerException)
        internal MethodInvocationException(
        /// Initializes a new instance of MethodInvocationException with serialization parameters.
        protected MethodInvocationException(SerializationInfo info, StreamingContext context)
    /// Defines the exception thrown for errors getting the value of properties.
    public class GetValueException : ExtendedTypeSystemException
        internal const string GetWithoutGetterExceptionMsg = "GetWithoutGetterException";
        internal const string WriteOnlyProperty = "WriteOnlyProperty";
        /// Initializes a new instance of GetValueException with the message set
        /// to typeof(GetValueException).FullName.
        public GetValueException()
            : base(typeof(GetValueException).FullName)
        /// Initializes a new instance of GetValueException setting the message.
        public GetValueException(string message)
        /// Initializes a new instance of GetValueException setting the message and innerException.
        public GetValueException(string message, Exception innerException)
        internal GetValueException(
        /// Initializes a new instance of GetValueException with serialization parameters.
        protected GetValueException(SerializationInfo info, StreamingContext context)
    public class PropertyNotFoundException : ExtendedTypeSystemException
        public PropertyNotFoundException()
            : base(typeof(PropertyNotFoundException).FullName)
        public PropertyNotFoundException(string message)
        public PropertyNotFoundException(string message, Exception innerException)
        internal PropertyNotFoundException(
        protected PropertyNotFoundException(SerializationInfo info, StreamingContext context)
    /// Defines the exception thrown for exceptions thrown by property getters.
    public class GetValueInvocationException : GetValueException
        internal const string ExceptionWhenGettingMsg = "ExceptionWhenGetting";
        /// Initializes a new instance of GetValueInvocationException with the message set
        /// to typeof(GetValueInvocationException).FullName.
        public GetValueInvocationException()
            : base(typeof(GetValueInvocationException).FullName)
        /// Initializes a new instance of GetValueInvocationException setting the message.
        public GetValueInvocationException(string message)
        /// Initializes a new instance of GetValueInvocationException setting the message and innerException.
        public GetValueInvocationException(string message, Exception innerException)
        internal GetValueInvocationException(
        /// Initializes a new instance of GetValueInvocationException with serialization parameters.
        protected GetValueInvocationException(SerializationInfo info, StreamingContext context)
    /// Defines the exception thrown for errors setting the value of properties.
    public class SetValueException : ExtendedTypeSystemException
        /// Initializes a new instance of SetValueException with the message set
        /// to typeof(SetValueException).FullName.
        public SetValueException()
            : base(typeof(SetValueException).FullName)
        /// Initializes a new instance of SetValueException setting the message.
        public SetValueException(string message)
        /// Initializes a new instance of SetValueException setting the message and innerException.
        public SetValueException(string message, Exception innerException)
        internal SetValueException(
        /// Initializes a new instance of SetValueException with serialization parameters.
        protected SetValueException(SerializationInfo info, StreamingContext context)
    /// Defines the exception thrown for exceptions thrown by property setters.
    public class SetValueInvocationException : SetValueException
        /// Initializes a new instance of SetValueInvocationException with the message set
        /// to typeof(SetValueInvocationException).FullName.
        public SetValueInvocationException()
            : base(typeof(SetValueInvocationException).FullName)
        /// Initializes a new instance of SetValueInvocationException setting the message.
        public SetValueInvocationException(string message)
        /// Initializes a new instance of SetValueInvocationException setting the message and innerException.
        public SetValueInvocationException(string message, Exception innerException)
        internal SetValueInvocationException(
        /// Initializes a new instance of SetValueInvocationException with serialization parameters.
        protected SetValueInvocationException(SerializationInfo info, StreamingContext context)
    /// Defines the exception thrown for type conversion errors.
    public class PSInvalidCastException : InvalidCastException, IContainsErrorRecord
        /// Initializes a new instance of PSInvalidCastException with serialization parameters.
        protected PSInvalidCastException(SerializationInfo info, StreamingContext context)
        /// Initializes a new instance of PSInvalidCastException with the message set
        /// to typeof(PSInvalidCastException).FullName.
        public PSInvalidCastException()
            : base(typeof(PSInvalidCastException).FullName)
        /// Initializes a new instance of PSInvalidCastException setting the message.
        public PSInvalidCastException(string message)
        /// Initializes a new instance of PSInvalidCastException setting the message and innerException.
        public PSInvalidCastException(string message, Exception innerException)
        internal PSInvalidCastException(string errorId, string message, Exception innerException)
        internal PSInvalidCastException(
            : this(
                errorId, StringUtil.Format(resourceString, arguments),
        /// Gets the ErrorRecord associated with this exception.
                _errorRecord ??= new ErrorRecord(
                    new ParentContainsErrorRecordException(this),
                    _errorId,
                return _errorRecord;
        private readonly string _errorId = "PSInvalidCastException";

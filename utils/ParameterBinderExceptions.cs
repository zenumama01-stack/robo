    /// The exception thrown if the specified value can not be bound parameter of a command.
    public class ParameterBindingException : RuntimeException
        #region Preferred constructors
        /// Constructs a ParameterBindingException.
        /// The category for the error.
        /// The information about the command that encountered the error.
        /// InvocationInfo.MyCommand.Name == {0}
        /// The position for the command or parameter that caused the error.
        /// If position is null, the one from the InvocationInfo is used.
        /// token.LineNumber == {4}
        /// token.OffsetInLine == {5}
        /// The parameter on which binding caused the error.
        /// parameterName == {1}
        /// The Type the parameter was expecting.
        /// parameterType == {2}
        /// <param name="typeSpecified">
        /// The Type that was attempted to be bound to the parameter.
        /// typeSpecified == {3}
        /// The format string for the exception message.
        /// The error ID.
        /// Additional arguments to pass to the format string.
        /// starts at {6}
        /// If <paramref name="resourceString"/> or <paramref name="errorId"/>
        /// is null or empty.
        internal ParameterBindingException(
            : base(errorCategory, invocationInfo, errorPosition, errorId, null, null)
                throw PSTraceSource.NewArgumentException(nameof(resourceString));
                throw PSTraceSource.NewArgumentException(nameof(errorId));
                _commandName = invocationInfo.MyCommand.Name;
            _typeSpecified = typeSpecified;
            if ((errorPosition == null) && (_invocationInfo != null))
                errorPosition = invocationInfo.ScriptPosition;
                _line = errorPosition.StartLineNumber;
                _offset = errorPosition.StartColumnNumber;
            _resourceString = resourceString;
        /// If <paramref name="invocationInfo"/> is null.
            : base(errorCategory, invocationInfo, errorPosition, errorId, null, innerException)
                throw PSTraceSource.NewArgumentNullException(nameof(invocationInfo));
            errorPosition ??= invocationInfo.ScriptPosition;
            ParameterBindingException pbex,
            : base(string.Empty, innerException)
            _invocationInfo = pbex.CommandInvocation;
                _commandName = _invocationInfo.MyCommand.Name;
                errorPosition = _invocationInfo.ScriptPosition;
            _line = pbex.Line;
            _offset = pbex.Offset;
            _parameterName = pbex.ParameterName;
            _parameterType = pbex.ParameterType;
            _typeSpecified = pbex.TypeSpecified;
            _errorId = pbex.ErrorId;
            base.SetErrorCategory(pbex.ErrorRecord._category);
            base.SetErrorId(_errorId);
                base.ErrorRecord.SetInvocationInfo(new InvocationInfo(_invocationInfo.MyCommand, errorPosition));
        #endregion Preferred constructors
        /// Constructors a ParameterBindingException using serialized data.
        protected ParameterBindingException(
        #region Do Not Use
        /// DO NOT USE!!!
        public ParameterBindingException() : base() { }
        /// Constructors a ParameterBindingException.
        /// Message to be included in exception.
        public ParameterBindingException(string message) : base(message) { _message = message; }
        /// Message to be included in the exception.
        /// exception that led to this exception
        public ParameterBindingException(
        { _message = message; }
        #endregion Do Not Use
            get { return _message ??= BuildMessage(); }
        /// Gets the name of the parameter that the parameter binding
        /// error was encountered on.
                return _parameterName;
        private readonly string _parameterName = string.Empty;
        /// Gets the type the parameter is expecting.
        /// Gets the Type that was specified as the parameter value.
        public Type TypeSpecified
                return _typeSpecified;
        private readonly Type _typeSpecified;
        /// Gets the errorId of this ParameterBindingException.
        public string ErrorId
                return _errorId;
        /// Gets the line in the script at which the error occurred.
        public Int64 Line
                return _line;
        private readonly Int64 _line = Int64.MinValue;
        /// Gets the offset on the line in the script at which the error occurred.
        public Int64 Offset
        private readonly Int64 _offset = Int64.MinValue;
        /// Gets the invocation information about the command.
        public InvocationInfo CommandInvocation
        private readonly string _resourceString;
        private readonly object[] _args = Array.Empty<object>();
        private string BuildMessage()
            object[] messageArgs = Array.Empty<object>();
                messageArgs = new object[_args.Length + 6];
                messageArgs[0] = _commandName;
                messageArgs[1] = _parameterName;
                messageArgs[2] = _parameterType;
                messageArgs[3] = _typeSpecified;
                messageArgs[4] = _line;
                messageArgs[5] = _offset;
                _args.CopyTo(messageArgs, 6);
            if (!string.IsNullOrEmpty(_resourceString))
                result = StringUtil.Format(_resourceString, messageArgs);
    internal class ParameterBindingValidationException : ParameterBindingException
        /// Constructs a ParameterBindingValidationException.
        internal ParameterBindingValidationException(
                args)
        /// If <paramref name="resourceBaseName"/> or <paramref name="errorIdAndResourceId"/>
            if (innerException is ValidationMetadataException validationException && validationException.SwallowException)
                _swallowException = true;
        /// Constructs a ParameterBindingValidationException from serialized data.
        protected ParameterBindingValidationException(
        #region Property
        /// Make the positional binding ignore this validation exception when it's set to true.
        #endregion Property
    internal class ParameterBindingArgumentTransformationException : ParameterBindingException
        /// Constructs a ParameterBindingArgumentTransformationException.
        internal ParameterBindingArgumentTransformationException(
        /// Constructs a ParameterBindingArgumentTransformationException using serialized data.
        protected ParameterBindingArgumentTransformationException(
    internal class ParameterBindingParameterDefaultValueException : ParameterBindingException
        /// Constructs a ParameterBindingParameterDefaultValueException.
        internal ParameterBindingParameterDefaultValueException(
        /// Constructs a ParameterBindingParameterDefaultValueException using serialized data.
        protected ParameterBindingParameterDefaultValueException(

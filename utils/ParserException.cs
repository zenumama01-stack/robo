    /// Defines the exception thrown when a syntax error occurs while parsing PowerShell script text.
    public class ParseException : RuntimeException
        private const string errorIdString = "Parse";
        private readonly ParseError[] _errors;
        /// The list of parser errors.
        public ParseError[] Errors
        /// Initializes a new instance of the ParseException class and defines the serialization information,
        protected ParseException(SerializationInfo info,
        /// Initializes a new instance of the class ParseException.
        public ParseException() : base()
            base.SetErrorCategory(ErrorCategory.ParserError);
        /// Initializes a new instance of the ParseException class and defines the error message.
        public ParseException(string message) : base(message)
        /// Initializes a new instance of the ParseException class and defines the error message and
        internal ParseException(string message, string errorId) : base(message)
        /// Initializes a new instance of the ParseException class and defines the error message,
        internal ParseException(string message, string errorId, Exception innerException)
        public ParseException(string message,
        /// Initializes a new instance of the ParseException class with a collection of error messages.
        /// <param name="errors">The collection of error messages.</param>
        public ParseException(ParseError[] errors)
            if (errors is null || errors.Length == 0)
                throw new ArgumentNullException(nameof(errors));
            // Arbitrarily choose the first error message for the ErrorId.
            base.SetErrorId(_errors[0].ErrorId);
            if (errors[0].Extent != null)
                this.ErrorRecord.SetInvocationInfo(new InvocationInfo(null, errors[0].Extent));
        /// The error message to display.
                if (_errors == null)
                // Report at most the first 10 errors
                var errorsToReport = (_errors.Length > 10)
                    ? _errors.Take(10).Select(static e => e.ToString()).Append(ParserStrings.TooManyErrors)
                    : _errors.Select(static e => e.ToString());
                return string.Join(Environment.NewLine + Environment.NewLine, errorsToReport);
    /// Defines the exception thrown when a incomplete parse error occurs while parsing PowerShell script text.
    /// This is a variation on a parsing error that indicates that the parse was incomplete
    /// rather than irrecoverably wrong. A host can catch this exception and then prompt for additional
    /// input to complete the parse.
    public class IncompleteParseException
            : ParseException
        private const string errorIdString = "IncompleteParse";
        /// Initializes a new instance of the IncompleteParseException class and defines the serialization information,
        protected IncompleteParseException(SerializationInfo info,
        /// Initializes a new instance of the class IncompleteParseException.
        public IncompleteParseException() : base()
            // Error category is set in base constructor
        /// Initializes a new instance of the IncompleteParseException class and defines the error message.
        public IncompleteParseException(string message) : base(message)
        /// Initializes a new instance of the IncompleteParseException class and defines the error message and
        internal IncompleteParseException(string message, string errorId) : base(message, errorId)
        /// Initializes a new instance of the IncompleteParseException class and defines the error message,
        internal IncompleteParseException(string message, string errorId, Exception innerException)
            : base(message, errorId, innerException)
        public IncompleteParseException(string message,

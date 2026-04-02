    /// Defines exception thrown when a PSSnapin was not able to load into current runspace.
    /// Implementation of PSConsoleLoadException requires it to
    ///     1. Implement IContainsErrorRecord,
    ///     2. ISerializable
    /// Basic information for this exception includes,
    ///     1. PSSnapin name
    ///     2. Inner exception.
    public class PSConsoleLoadException : SystemException, IContainsErrorRecord
        /// Initiate an instance of PSConsoleLoadException.
        public PSConsoleLoadException() : base()
        /// <param name="message">Error message.</param>
        public PSConsoleLoadException(string message)
        public PSConsoleLoadException(string message, Exception innerException)
        /// Gets error record embedded in this exception.
        /// This property is required as part of IErrorRecordContainer
        /// Create the internal error record.
            if (PSSnapInExceptions != null)
                foreach (PSSnapInException e in PSSnapInExceptions)
                    sb.Append(e.Message);
            _errorRecord = new ErrorRecord(new ParentContainsErrorRecordException(this), "ConsoleLoadFailure", ErrorCategory.ResourceUnavailable, null);
        private readonly Collection<PSSnapInException> _PSSnapInExceptions = new Collection<PSSnapInException>();
        internal Collection<PSSnapInException> PSSnapInExceptions
                return _PSSnapInExceptions;
        /// Gets message for this exception.

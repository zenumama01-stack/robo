    /// Implementation of PSSnapInException requires it to
    public class PSSnapInException : RuntimeException
        /// Initiate an instance of PSSnapInException.
        /// <param name="PSSnapin">PSSnapin for the exception.</param>
        /// <param name="message">Message with load failure detail.</param>
        internal PSSnapInException(string PSSnapin, string message)
            _reason = message;
        /// <param name="warning">Whether this is just a warning for PSSnapin load.</param>
        internal PSSnapInException(string PSSnapin, string message, bool warning)
            _warning = warning;
        /// <param name="exception">Exception for PSSnapin load failure.</param>
        internal PSSnapInException(string PSSnapin, string message, Exception exception)
            : base(message, exception)
        public PSSnapInException() : base()
        public PSSnapInException(string message)
        public PSSnapInException(string message, Exception innerException)
            // if _PSSnapin or _reason is empty, this exception is created using default
            // constructor. Don't create the error record since there is
            // no useful information anyway.
            if (!string.IsNullOrEmpty(_PSSnapin) && !string.IsNullOrEmpty(_reason))
                Assembly currentAssembly = typeof(PSSnapInException).Assembly;
                if (_warning)
                    _errorRecord = new ErrorRecord(new ParentContainsErrorRecordException(this), "PSSnapInLoadWarning", ErrorCategory.ResourceUnavailable, null);
                    _errorRecord.ErrorDetails = new ErrorDetails(string.Format(ConsoleInfoErrorStrings.PSSnapInLoadWarning, _PSSnapin, _reason));
                    _errorRecord = new ErrorRecord(new ParentContainsErrorRecordException(this), "PSSnapInLoadFailure", ErrorCategory.ResourceUnavailable, null);
                    _errorRecord.ErrorDetails = new ErrorDetails(string.Format(ConsoleInfoErrorStrings.PSSnapInLoadFailure, _PSSnapin, _reason));
        private readonly bool _warning = false;
        private bool _isErrorRecordOriginallyNull;
                if (_errorRecord == null)
                    _isErrorRecordOriginallyNull = true;
                        "PSSnapInException",
        private readonly string _PSSnapin = string.Empty;
        private readonly string _reason = string.Empty;
                if (_errorRecord != null && !_isErrorRecordOriginallyNull)
        /// Initiate a PSSnapInException instance.
        protected PSSnapInException(SerializationInfo info,

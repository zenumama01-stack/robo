    /// RuntimeException is the base class for exceptions likely to occur
    /// while a PowerShell command is running.
    /// PowerShell scripts can trap RuntimeException using the
    /// "trap (exceptionclass) {handler}" script construct.
    public class RuntimeException
            : SystemException, IContainsErrorRecord
        /// Initializes a new instance of the RuntimeException class.
        public RuntimeException()
        /// Initializes a new instance of the RuntimeException class
        protected RuntimeException(SerializationInfo info,
        public RuntimeException(string message)
        public RuntimeException(string message,
        /// starting with an already populated error record.
        internal RuntimeException(ErrorCategory errorCategory,
            SetErrorId(errorIdAndResourceId);
            if ((errorPosition == null) && (invocationInfo != null))
                _targetObject);
            _errorRecord.SetInvocationInfo(new InvocationInfo(invocationInfo.MyCommand, errorPosition));
        #region ErrorRecord
        // If RuntimeException subclasses need to do more than change
        // the ErrorId, ErrorCategory and TargetObject, they can access
        // the ErrorRecord property and make changes directly.  However,
        // not that calling SetErrorId, SetErrorCategory or SetTargetObject
        // will clean the cached ErrorRecord and erase any other changes,
        // so the ErrorId etc. should be set first.
        public virtual ErrorRecord ErrorRecord
        private string _errorId = "RuntimeException";
        private ErrorCategory _errorCategory = ErrorCategory.NotSpecified;
        private object _targetObject = null;
        /// Subclasses can use this method to set the ErrorId.
        /// Note that this will clear the cached ErrorRecord, so be sure
        /// to change this before writing to ErrorRecord.ErrorDetails
        /// or the like.
        /// <param name="errorId">Per ErrorRecord constructors.</param>
            if (_errorId != errorId)
                _errorRecord = null;
        /// Subclasses can use this method to set the ErrorCategory.
        /// per ErrorRecord.CategoryInfo.Category
        internal void SetErrorCategory(ErrorCategory errorCategory)
            if (_errorCategory != errorCategory)
        /// Subclasses can use this method to set or update the TargetObject.
        /// This convenience function doesn't clobber the error record if it
        /// already exists...
        /// per ErrorRecord.TargetObject
        internal void SetTargetObject(object targetObject)
            _targetObject = targetObject;
            _errorRecord?.SetTargetObject(targetObject);
        #endregion ErrorRecord
        internal static string RetrieveMessage(ErrorRecord errorRecord)
            if (errorRecord.ErrorDetails != null &&
                !string.IsNullOrEmpty(errorRecord.ErrorDetails.Message))
                return errorRecord.ErrorDetails.Message;
            if (errorRecord.Exception == null)
            return errorRecord.Exception.Message;
        internal static string RetrieveMessage(Exception e)
            if (e is not IContainsErrorRecord icer)
                return e.Message;
            ErrorRecord er = icer.ErrorRecord;
            ErrorDetails ed = er.ErrorDetails;
            if (ed == null)
            string detailsMessage = ed.Message;
            return (string.IsNullOrEmpty(detailsMessage)) ? e.Message : detailsMessage;
        internal static Exception RetrieveException(ErrorRecord errorRecord)
        public bool WasThrownFromThrowStatement
                return _thrownByThrowStatement;
                _thrownByThrowStatement = value;
                if (_errorRecord?.Exception is RuntimeException exception)
                    exception.WasThrownFromThrowStatement = value;
        private bool _thrownByThrowStatement;
        internal bool WasRethrown { get; set; }
        /// Fix for BUG: Windows Out Of Band Releases: 906263 and 906264
        /// The interpreter prompt CommandBaseStrings:InquireHalt
        /// should be suppressed when this flag is set.  This will be set
        /// when this prompt has already occurred and Break was chosen,
        /// or for ActionPreferenceStopException in all cases.
        internal bool SuppressPromptInInterpreter
            get { return _suppressPromptInInterpreter; }
            set { _suppressPromptInInterpreter = value; }
        private bool _suppressPromptInInterpreter;
        private Token _errorToken;
        internal Token ErrorToken
                return _errorToken;
                _errorToken = value;

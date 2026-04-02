    /// An exception that wraps all exceptions that are thrown by providers. This allows
    /// callers of the provider APIs to be able to catch a single exception no matter
    /// what any of the various providers may have thrown.
    public class ProviderInvocationException : RuntimeException
        /// Constructs a ProviderInvocationException.
        public ProviderInvocationException() : base()
        /// Constructs a ProviderInvocationException using serialized data.
        protected ProviderInvocationException(
        /// Constructs a ProviderInvocationException with a message.
        /// The message for the exception.
        public ProviderInvocationException(string message)
        /// Constructs a ProviderInvocationException with provider information and an inner exception.
        /// Information about the provider to be used in formatting the message.
        /// The inner exception for this exception.
        internal ProviderInvocationException(ProviderInfo provider, Exception innerException)
            : base(RuntimeException.RetrieveMessage(innerException), innerException)
            _message = base.Message;
            _providerInfo = provider;
                    "ErrorRecordNotSpecified",
        /// Constructs a ProviderInvocationException with provider information and an
        /// Detailed error information
        internal ProviderInvocationException(ProviderInfo provider, ErrorRecord errorRecord)
            : base(RuntimeException.RetrieveMessage(errorRecord),
                    RuntimeException.RetrieveException(errorRecord))
        /// Constructs a ProviderInvocationException with a message
        /// and inner exception.
        public ProviderInvocationException(string message, Exception innerException)
        /// This string is the message template string.
        /// The provider information used to format into the message.
        /// The path that was being processed when the exception occurred.
        /// The exception that was thrown by the provider.
        internal ProviderInvocationException(
            : this(errorId, resourceStr, provider, path, innerException, true)
        /// Constructor to make it easy to wrap a provider exception.
        /// This is the message template string
        /// <param name="useInnerExceptionMessage">
        /// If true, the message from the inner exception will be used if the exception contains
        /// an ErrorRecord. If false, the error message retrieved using the errorId will be used.
            bool useInnerExceptionMessage)
                RetrieveMessage(errorId, resourceStr, provider, path, innerException),
            Exception errorRecordException = null;
            if (useInnerExceptionMessage)
                errorRecordException = innerException;
                errorRecordException = new ParentContainsErrorRecordException(this);
                _errorRecord = new ErrorRecord(icer.ErrorRecord, errorRecordException);
                    errorRecordException,
        /// Gets the provider information of the provider that threw an exception.
        public ProviderInfo ProviderInfo { get { return _providerInfo; } }
        internal ProviderInfo _providerInfo;
        /// Gets the error record.
                    "ProviderInvocationException",
        #region Private/Internal
        private static string RetrieveMessage(
                "ProviderInvocationException.RetrieveMessage needs innerException");
                "ProviderInvocationException.RetrieveMessage needs errorId");
                return RuntimeException.RetrieveMessage(innerException);
                "ProviderInvocationException.RetrieveMessage needs provider");
            string format = resourceStr;
            if (string.IsNullOrEmpty(format))
                "ProviderInvocationException.RetrieveMessage bad errorId " + errorId);
                        RuntimeException.RetrieveMessage(innerException));
            get { return (string.IsNullOrEmpty(_message)) ? base.Message : _message; }
        private readonly string _message /* = null */;
        #endregion Private/Internal
    /// Categories of session state objects, used by SessionStateException.
    public enum SessionStateCategory
        /// Used when an exception is thrown accessing a variable.
        Variable = 0,
        /// Used when an exception is thrown accessing an alias.
        Alias = 1,
        /// Used when an exception is thrown accessing a function.
        Function = 2,
        /// Used when an exception is thrown accessing a filter.
        Filter = 3,
        /// Used when an exception is thrown accessing a drive.
        Drive = 4,
        /// Used when an exception is thrown accessing a Cmdlet Provider.
        CmdletProvider = 5,
        /// Used when an exception is thrown manipulating the PowerShell language scopes.
        Scope = 6,
        /// Used when generically accessing any type of command...
        Command = 7,
        /// Other resources not covered by the previous categories...
        Resource = 8,
        /// Used when an exception is thrown accessing a cmdlet.
        Cmdlet = 9,
    /// SessionStateException represents an error working with
    /// session state objects: variables, aliases, functions, filters,
    /// drives, or providers.
    public class SessionStateException : RuntimeException
        /// Constructs a SessionStateException.
        /// <param name="itemName">Name of session state object.</param>
        /// <param name="sessionStateCategory">Category of session state object.</param>
        /// <param name="resourceStr">This string is the message template string.</param>
        /// SessionStateStrings.txt.
        /// <param name="errorCategory">ErrorRecord.CategoryInfo.Category.</param>
        /// Additional insertion strings used to construct the message.
        /// Note that itemName is always the first insertion string.
        internal SessionStateException(
            string itemName,
            SessionStateCategory sessionStateCategory,
            : base(BuildMessage(itemName, resourceStr, messageArgs))
            _itemName = itemName;
            _sessionStateCategory = sessionStateCategory;
        public SessionStateException()
        public SessionStateException(string message)
        /// The exception that caused the error.
        public SessionStateException(string message,
        /// Constructs a SessionStateException using serialized data.
        protected SessionStateException(SerializationInfo info,
        /// Gets the error record information for this exception.
                    _itemName);
        /// Gets the name of session state object the error occurred on.
        public string ItemName
            get { return _itemName; }
        private readonly string _itemName = string.Empty;
        /// Gets the category of session state object the error occurred on.
        public SessionStateCategory SessionStateCategory
            get { return _sessionStateCategory; }
        private readonly SessionStateCategory _sessionStateCategory = SessionStateCategory.Variable;
        private readonly string _errorId = "SessionStateException";
        private readonly ErrorCategory _errorCategory = ErrorCategory.InvalidArgument;
                a[0] = itemName;
    /// SessionStateUnauthorizedAccessException occurs when
    /// a change to a session state object cannot be completed
    /// because the object is read-only or constant, or because
    /// an object which is declared constant cannot be removed
    /// or made non-constant.
    public class SessionStateUnauthorizedAccessException : SessionStateException
        /// Constructs a SessionStateUnauthorizedAccessException.
        /// <param name="itemName">
        /// The name of the session state object the error occurred on.
        /// <param name="sessionStateCategory">
        /// The category of session state object.
        internal SessionStateUnauthorizedAccessException(
            string resourceStr
            : base(itemName, sessionStateCategory,
                    errorIdAndResourceId, resourceStr, ErrorCategory.WriteError)
        /// Constructs a SessionStateUnauthorizedAccessException using serialized data.
        protected SessionStateUnauthorizedAccessException(
        public SessionStateUnauthorizedAccessException()
        /// The message used by the exception.
        public SessionStateUnauthorizedAccessException(string message)
        public SessionStateUnauthorizedAccessException(string message,
    /// ProviderNotFoundException occurs when no provider can be found
    /// with the specified name.
    public class ProviderNotFoundException : SessionStateException
        /// Constructs a ProviderNotFoundException.
        /// The name of provider that could not be found.
        /// The category of session state object
        /// This string is the message template string
        /// Additional arguments to build the message from.
        internal ProviderNotFoundException(
                sessionStateCategory,
                errorIdAndResourceId,
                messageArgs)
        public ProviderNotFoundException()
        /// The messaged used by the exception.
        public ProviderNotFoundException(string message)
        public ProviderNotFoundException(string message,
    /// ProviderNameAmbiguousException occurs when more than one provider exists
    /// for a given name and the request did not contain the PSSnapin name qualifier.
    public class ProviderNameAmbiguousException : ProviderNotFoundException
        /// Constructs a ProviderNameAmbiguousException.
        /// The name of provider that was ambiguous.
        /// <param name="possibleMatches">
        /// The provider information for the providers that match the specified
        /// name.
        internal ProviderNameAmbiguousException(
            Collection<ProviderInfo> possibleMatches,
            _possibleMatches = new ReadOnlyCollection<ProviderInfo>(possibleMatches);
        public ProviderNameAmbiguousException()
        public ProviderNameAmbiguousException(string message)
        public ProviderNameAmbiguousException(string message,
        /// Constructs a ProviderNameAmbiguousException using serialized data.
        protected ProviderNameAmbiguousException(
        /// Gets the information of the providers which might match the specified
        /// provider name.
        public ReadOnlyCollection<ProviderInfo> PossibleMatches
                return _possibleMatches;
        private readonly ReadOnlyCollection<ProviderInfo> _possibleMatches;
    /// DriveNotFoundException occurs when no drive can be found
    public class DriveNotFoundException : SessionStateException
        /// Constructs a DriveNotFoundException.
        /// The name of the drive that could not be found.
        internal DriveNotFoundException(
            : base(itemName, SessionStateCategory.Drive,
                    errorIdAndResourceId, resourceStr, ErrorCategory.ObjectNotFound)
        public DriveNotFoundException()
        /// The message that will be used by the exception.
        public DriveNotFoundException(string message)
        public DriveNotFoundException(string message,
        /// Constructs a DriveNotFoundException using serialized data.
        protected DriveNotFoundException(
    /// ItemNotFoundException occurs when the path contained no wildcard characters
    /// and an item at that path could not be found.
    public class ItemNotFoundException : SessionStateException
        /// Constructs a ItemNotFoundException.
        /// The path that was not found.
        internal ItemNotFoundException(
            : base(path, SessionStateCategory.Drive,
        public ItemNotFoundException()
        public ItemNotFoundException(string message)
        public ItemNotFoundException(string message,
        /// Constructs a ItemNotFoundException using serialized data.
        protected ItemNotFoundException(

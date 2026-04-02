    /// This exception is thrown when a command cannot be found.
    public class CommandNotFoundException : RuntimeException
        /// Constructs a CommandNotFoundException. This is the recommended constructor.
        /// The name of the command that could not be found.
        /// The inner exception.
        /// This string is message template string
        /// <param name="errorIdAndResourceId">
        /// This string is the ErrorId passed to the ErrorRecord, and is also
        /// the resourceId used to look up the message template string in
        /// DiscoveryExceptions.txt.
        /// <param name="messageArgs">
        /// Additional arguments to format into the message.
        internal CommandNotFoundException(
            string errorIdAndResourceId,
            params object[] messageArgs)
            : base(BuildMessage(commandName, resourceStr, messageArgs), innerException)
            _errorId = errorIdAndResourceId;
        /// Constructs a CommandNotFoundException.
        public CommandNotFoundException() : base() { }
        public CommandNotFoundException(string message) : base(message) { }
        public CommandNotFoundException(string message, Exception innerException) : base(message, innerException) { }
        /// Serialization constructor for class CommandNotFoundException.
        /// serialization information
        /// streaming context
        protected CommandNotFoundException(SerializationInfo info,
                    _errorCategory,
        /// Gets the name of the command that could not be found.
            get { return _commandName; }
            set { _commandName = value; }
        private string _commandName = string.Empty;
        private readonly string _errorId = "CommandNotFoundException";
        private readonly ErrorCategory _errorCategory = ErrorCategory.ObjectNotFound;
        private static string BuildMessage(
            params object[] messageArgs
            object[] a;
            if (messageArgs != null && messageArgs.Length > 0)
                a = new object[messageArgs.Length + 1];
                a[0] = commandName;
                messageArgs.CopyTo(a, 1);
                a = new object[1];
            return StringUtil.Format(resourceStr, a);
    /// Defines the exception thrown when a script's requirements to run specified by the #requires
    /// statements are not met.
    public class ScriptRequiresException : RuntimeException
        /// Constructs an ScriptRequiresException. Recommended constructor for the class for
        /// #requires -shellId MyShellId.
        /// The name of the script containing the #requires statement.
        /// <param name="requiresShellId">
        /// The ID of the shell that is incompatible with the current shell.
        /// <param name="requiresShellPath">
        /// The path to the shell specified in the #requires -shellId statement.
        /// The error id for this exception.
        internal ScriptRequiresException(
            string requiresShellId,
            string requiresShellPath,
            : base(BuildMessage(commandName, requiresShellId, requiresShellPath, true))
            Diagnostics.Assert(!string.IsNullOrEmpty(commandName), "commandName is null or empty when constructing ScriptRequiresException");
            Diagnostics.Assert(!string.IsNullOrEmpty(errorId), "errorId is null or empty when constructing ScriptRequiresException");
            _requiresShellId = requiresShellId;
            _requiresShellPath = requiresShellPath;
            this.SetErrorId(errorId);
            this.SetTargetObject(commandName);
            this.SetErrorCategory(ErrorCategory.ResourceUnavailable);
        /// #requires -version N.
        /// <param name="requiresPSVersion">
        /// The Msh version that the script requires.
        /// <param name="currentPSVersion">
        /// The current Msh version
            Version requiresPSVersion,
            string currentPSVersion,
            : base(BuildMessage(commandName, requiresPSVersion.ToString(), currentPSVersion, false))
            Diagnostics.Assert(requiresPSVersion != null, "requiresPSVersion is null or empty when constructing ScriptRequiresException");
            _requiresPSVersion = requiresPSVersion;
        /// Constructs an ScriptRequiresException. Recommended constructor for the class for the
        /// #requires -PSSnapin MyPSSnapIn statement.
        /// <param name="missingItems">
        /// The missing snap-ins/modules that the script requires.
        /// /// <param name="forSnapins">
        /// Indicates whether the error message needs to be constructed for missing snap-ins/ missing modules.
            Collection<string> missingItems,
            bool forSnapins)
            : this(commandName, missingItems, errorId, forSnapins, null)
        /// The error Record for this exception.
            bool forSnapins,
            : base(BuildMessage(commandName, missingItems, forSnapins), null, errorRecord)
            Diagnostics.Assert(missingItems != null && missingItems.Count > 0, "missingItems is null or empty when constructing ScriptRequiresException");
            _missingPSSnapIns = new ReadOnlyCollection<string>(missingItems);
        /// #requires -RunAsAdministrator statement.
            : base(BuildMessage(commandName))
            this.SetErrorCategory(ErrorCategory.PermissionDenied);
        /// Constructs an PSVersionNotCompatibleException.
        public ScriptRequiresException() : base() { }
        public ScriptRequiresException(string message) : base(message) { }
        /// The exception that led to this exception.
        public ScriptRequiresException(string message, Exception innerException) : base(message, innerException) { }
        /// Constructs an PSVersionNotCompatibleException using serialized data.
        protected ScriptRequiresException(SerializationInfo info,
        /// Gets the name of the script that contained the #requires statement.
        private readonly string _commandName = string.Empty;
        /// Gets the PSVersion that the script requires.
        public Version RequiresPSVersion
            get { return _requiresPSVersion; }
        private readonly Version _requiresPSVersion;
        /// Gets the missing snap-ins that the script requires.
        public ReadOnlyCollection<string> MissingPSSnapIns
            get { return _missingPSSnapIns; }
        private readonly ReadOnlyCollection<string> _missingPSSnapIns = new ReadOnlyCollection<string>(Array.Empty<string>());
        /// Gets or sets the ID of the shell.
        public string RequiresShellId
            get { return _requiresShellId; }
        private readonly string _requiresShellId;
        /// Gets or sets the path to the incompatible shell.
        public string RequiresShellPath
            get { return _requiresShellPath; }
        private readonly string _requiresShellPath;
            if (missingItems == null)
                throw PSTraceSource.NewArgumentNullException(nameof(missingItems));
            foreach (string missingItem in missingItems)
                sb.Append(missingItem).Append(", ");
            if (sb.Length > 1)
            if (forSnapins)
                    DiscoveryExceptions.RequiresMissingPSSnapIns,
                    sb.ToString());
                    DiscoveryExceptions.RequiresMissingModules,
            string first,
            string second,
            bool forShellId)
            if (forShellId)
                if (string.IsNullOrEmpty(first))
                    resourceStr = DiscoveryExceptions.RequiresShellIDInvalidForSingleShell;
                    resourceStr = string.IsNullOrEmpty(second)
                            ? DiscoveryExceptions.RequiresInterpreterNotCompatibleNoPath
                            : DiscoveryExceptions.RequiresInterpreterNotCompatible;
                resourceStr = DiscoveryExceptions.RequiresPSVersionNotCompatible;
            return StringUtil.Format(resourceStr, commandName, first, second);
        private static string BuildMessage(string commandName)
            return StringUtil.Format(DiscoveryExceptions.RequiresElevation, commandName);

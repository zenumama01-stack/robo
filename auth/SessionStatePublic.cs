    /// Exposes the APIs to manipulate the providers, Runspace data, and location to the Cmdlet base class.
    public sealed class SessionState
        /// An instance of SessionState that the APIs should work against.
        internal SessionState(SessionStateInternal sessionState)
        /// An instance of ExecutionContext whose EngineSessionState represents the parent session state.
        /// <param name="createAsChild">
        /// True if the session state should be created as a child session state.
        /// <param name="linkToGlobal">
        /// True if the session state should be linked to the global scope.
        internal SessionState(ExecutionContext context, bool createAsChild, bool linkToGlobal)
                throw new InvalidOperationException("ExecutionContext");
            if (createAsChild)
                _sessionState = new SessionStateInternal(context.EngineSessionState, linkToGlobal, context);
                _sessionState = new SessionStateInternal(context);
            _sessionState.PublicSessionState = this;
        /// Construct a new session state object...
        public SessionState()
            if (ecFromTLS == null)
            _sessionState = new SessionStateInternal(ecFromTLS);
        /// Gets the APIs to access drives.
        public DriveManagementIntrinsics Drive
            get { return _drive ??= new DriveManagementIntrinsics(_sessionState); }
        /// Gets the APIs to access providers.
        public CmdletProviderManagementIntrinsics Provider
            get { return _provider ??= new CmdletProviderManagementIntrinsics(_sessionState); }
        /// Gets the APIs to access paths and location.
        public PathIntrinsics Path
            get { return _path ??= new PathIntrinsics(_sessionState); }
        /// Gets the APIs to access variables in session state.
        public PSVariableIntrinsics PSVariable
            get { return _variable ??= new PSVariableIntrinsics(_sessionState); }
        public PSLanguageMode LanguageMode
            get { return _sessionState.LanguageMode; }
            set { _sessionState.LanguageMode = value; }
        public bool UseFullLanguageModeInDebugger
            get { return _sessionState.UseFullLanguageModeInDebugger; }
        /// Public proxy for the list of scripts that are allowed to be run. If the name "*"
        public List<string> Scripts
            get { return _sessionState.Scripts; }
        /// Public proxy for the list of applications that are allowed to be run. If the name "*"
        public List<string> Applications
            get { return _sessionState.Applications; }
        /// The module associated with this session state instance...
            get { return _sessionState.Module; }
        /// The provider intrinsics for this session state instance.
            get { return _sessionState.InvokeProvider; }
        /// The command invocation intrinsics for this session state instance.
            get { return _sessionState.ExecutionContext.EngineIntrinsics.InvokeCommand; }
        /// Utility to check the visibility of an object based on the current
        /// command origin. If the object implements IHasSessionStateEntryVisibility
        /// then the check will be made. If the check fails, then an exception will be thrown...
        /// <param name="origin">The command origin value to check against...</param>
        /// <param name="valueToCheck">The object to check.</param>
        public static void ThrowIfNotVisible(CommandOrigin origin, object valueToCheck)
            SessionStateException exception;
            if (!IsVisible(origin, valueToCheck))
                PSVariable sv = valueToCheck as PSVariable;
                if (sv != null)
                    exception =
                           sv.Name,
                           "VariableIsPrivate",
                           SessionStateStrings.VariableIsPrivate,
                CommandInfo cinfo = valueToCheck as CommandInfo;
                if (cinfo != null)
                    string commandName = cinfo.Name;
                    if (commandName != null)
                        // If we have a name, use it in the error message
                                SessionStateCategory.Command,
                                "NamedCommandIsPrivate",
                                SessionStateStrings.NamedCommandIsPrivate,
                                "CommandIsPrivate",
                                SessionStateStrings.CommandIsPrivate,
                // Catch all error for other types of resources...
                        SessionStateCategory.Resource,
                        "ResourceIsPrivate",
                        SessionStateStrings.ResourceIsPrivate,
        /// Checks the visibility of an object based on the command origin argument.
        /// <param name="origin">The origin to check against.</param>
        /// <returns>Returns true if the object is visible, false otherwise.</returns>
        public static bool IsVisible(CommandOrigin origin, object valueToCheck)
            if (origin == CommandOrigin.Internal)
            IHasSessionStateEntryVisibility obj = valueToCheck as IHasSessionStateEntryVisibility;
                return (obj.Visibility == SessionStateEntryVisibility.Public);
        /// <param name="variable">The variable to check.</param>
        public static bool IsVisible(CommandOrigin origin, PSVariable variable)
                throw PSTraceSource.NewArgumentNullException(nameof(variable));
            return (variable.Visibility == SessionStateEntryVisibility.Public);
        /// <param name="commandInfo">The command to check.</param>
        public static bool IsVisible(CommandOrigin origin, CommandInfo commandInfo)
            return (commandInfo.Visibility == SessionStateEntryVisibility.Public);
        /// Gets a reference to the "real" session state object instead of the facade.
        internal SessionStateInternal Internal
            get { return _sessionState; }
        private DriveManagementIntrinsics _drive;
        private CmdletProviderManagementIntrinsics _provider;
        private PathIntrinsics _path;
        private PSVariableIntrinsics _variable;
    /// This enum defines the visibility of execution environment elements...
    public enum SessionStateEntryVisibility
        /// Entries are visible to requests from outside the runspace.
        Public = 0,
        /// Entries are not visible to requests from outside the runspace.
        Private = 1
    internal interface IHasSessionStateEntryVisibility
        SessionStateEntryVisibility Visibility { get; set; }
    /// This enum defines what subset of the PowerShell language is permitted when
    /// calling into this execution environment.
    public enum PSLanguageMode
        /// All PowerShell language elements are available.
        FullLanguage = 0,
        /// A subset of language elements are available to external requests.
        RestrictedLanguage = 1,
        /// Commands containing script text to evaluate are not allowed. You can only
        /// call commands using the Runspace APIs when in this mode.
        NoLanguage = 2,
        /// Exposes a subset of the PowerShell language that limits itself to core PowerShell
        /// types, does not support method invocation (except on those types), and does not
        /// support property setters (except on those types).
        ConstrainedLanguage = 3

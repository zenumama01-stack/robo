    /// This enum defines the dispatch origin of a command.
    public enum CommandOrigin
        /// The command was submitted via a runspace.
        Runspace,
        /// The command was dispatched by the engine as a result of
        /// a dispatch request from an already running command.
        Internal
    /// Defines the base class for an authorization manager of a Runspace.
    /// An authorization manager helps a host control and restrict the
    /// execution of commands.  For each of the command types listed in
    /// the <see cref="System.Management.Automation.CommandTypes"/>
    /// enumeration, the engine requests permission from the AuthorizationManager
    /// to run the command.
    /// Extending this class requires that you override the ShouldRun method with
    /// the logic specific to your needs.  The base class gives permission to run
    /// every command.  The default
    /// Microsoft.PowerShell.PSAuthorizationManager
    /// provides a customized and much more complete authorization policy.
    public class AuthorizationManager
        /// Creates an instance of authorization manager using specified shellID.
        /// <param name="shellId">
        public AuthorizationManager(string shellId)
            ShellId = shellId;
        private readonly object _policyCheckLock = new object();
        #region methods to use internally
        /// Determine if we should run the specified file.
        /// <param name="commandInfo">Info on entity to be run.</param>
        /// <param name="origin">The dispatch origin of a command.</param>
        /// <param name="host">Allows access to the host.</param>
        /// This method throws SecurityException in case running is not allowed.
        /// If the derived security manager threw an exception or returned
        /// false with a reason.
        internal void ShouldRunInternal(CommandInfo commandInfo,
                                        CommandOrigin origin,
            // TODO:PSL this is a workaround since the exception below
            // hides the internal issue of what's going on in terms of
            // execution policy.
            // On non-Windows platform Set/Get-ExecutionPolicy throw
            // PlatformNotSupportedException
            // If we are debugging, let the unit tests swap the file from beneath us
            if (commandInfo.CommandType == CommandTypes.ExternalScript)
                while (Environment.GetEnvironmentVariable("PSCommandDiscoveryPreDelay") != null) { System.Threading.Thread.Sleep(100); }
            bool defaultCatch = false;
            Exception authorizationManagerException = null;
                lock (_policyCheckLock)
                    result = this.ShouldRun(commandInfo, origin, host, out authorizationManagerException);
                    while (Environment.GetEnvironmentVariable("PSCommandDiscoveryPostDelay") != null) { System.Threading.Thread.Sleep(100); }
                authorizationManagerException = e;
                defaultCatch = true;
                if (authorizationManagerException != null)
                    if (authorizationManagerException is PSSecurityException)
                        throw authorizationManagerException;
                        string message = authorizationManagerException.Message;
                        if (defaultCatch)
                            message = AuthorizationManagerBase.AuthorizationManagerDefaultFailureReason;
                        PSSecurityException securityException = new PSSecurityException(message, authorizationManagerException);
                        throw securityException;
                    throw new PSSecurityException(AuthorizationManagerBase.AuthorizationManagerDefaultFailureReason);
        /// Get the shell ID from the authorization manager...
        internal string ShellId { get; }
        #endregion methods to use internally
        #region methods for derived class to override
        /// Determines if the host should run the command a specified by the CommandInfo parameter.
        /// The default implementation gives permission to run every command.
        /// <param name="commandInfo">Information about the command to be run.</param>
        /// <param name="origin">The origin of the command.</param>
        /// <param name="host">The host running the command.</param>
        /// <param name="reason">The reason for preventing execution, if applicable.</param>
        /// <returns>True if the host should run the command.  False otherwise.</returns>
        protected internal virtual bool ShouldRun(CommandInfo commandInfo,
                                                  out Exception reason)
            Dbg.Diagnostics.Assert(commandInfo != null, "caller should validate the parameter");
            reason = null;
        #endregion methods for derived class to override

// System.Management.Automation is the namespace which contains the types and
// methods pertaining to the Microsoft Command Shell
    /// Defines the implementation of the 'Get-ExecutionPolicy' cmdlet.
    /// This cmdlet gets the effective execution policy of the shell.
    /// In priority-order (highest priority first,) these come from:
    ///    - Machine-wide Group Policy
    ///    - Current-user Group Policy
    ///    - Current session preference
    ///    - Current user machine preference
    ///    - Local machine preference.
    [Cmdlet(VerbsCommon.Get, "ExecutionPolicy", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096594")]
    [OutputType(typeof(ExecutionPolicy))]
    public class GetExecutionPolicyCommand : PSCmdlet
        /// Gets or sets the scope of the execution policy.
        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public ExecutionPolicyScope Scope
            get { return _executionPolicyScope; }
            set { _executionPolicyScope = value; _scopeSpecified = true; }
        private ExecutionPolicyScope _executionPolicyScope = ExecutionPolicyScope.LocalMachine;
        private bool _scopeSpecified = false;
        /// Gets or sets the List parameter, which lists all scopes and their execution
        /// policies.
        public SwitchParameter List
            get { return _list; }
            set { _list = value; }
        private bool _list;
        /// Outputs the execution policy.
            if (_list && _scopeSpecified)
                string message = ExecutionPolicyCommands.ListAndScopeSpecified;
                    "ListAndScopeSpecified",
            string shellId = base.Context.ShellID;
            if (_list)
                foreach (ExecutionPolicyScope scope in SecuritySupport.ExecutionPolicyScopePreferences)
                    PSObject outputObject = new();
                    ExecutionPolicy policy = SecuritySupport.GetExecutionPolicy(shellId, scope);
                    PSNoteProperty inputNote = new("Scope", scope);
                    outputObject.Properties.Add(inputNote);
                    inputNote = new PSNoteProperty(
                            "ExecutionPolicy", policy);
            else if (_scopeSpecified)
                WriteObject(SecuritySupport.GetExecutionPolicy(shellId, _executionPolicyScope));
                WriteObject(SecuritySupport.GetExecutionPolicy(shellId));
    /// Defines the implementation of the 'Set-ExecutionPolicy' cmdlet.
    /// This cmdlet sets the local preference for the execution policy of the
    /// shell.
    /// The execution policy may be overridden by settings in Group Policy.
    /// If the Group Policy setting overrides the desired behaviour, the Cmdlet
    /// generates a terminating error.
    [Cmdlet(VerbsCommon.Set, "ExecutionPolicy", SupportsShouldProcess = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096612")]
    public class SetExecutionPolicyCommand : PSCmdlet
        /// Gets or sets the execution policy that the user requests.
        public ExecutionPolicy ExecutionPolicy
            get { return _executionPolicy; }
            set { _executionPolicy = value; }
        private ExecutionPolicy _executionPolicy;
        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true)]
            set { _executionPolicyScope = value; }
        /// Specifies whether to force the execution policy change.
        /// Sets the execution policy (validation).
            // Verify they've specified a valid scope
            if ((_executionPolicyScope == ExecutionPolicyScope.UserPolicy) ||
                (_executionPolicyScope == ExecutionPolicyScope.MachinePolicy))
                string message = ExecutionPolicyCommands.CantSetGroupPolicy;
                    "CantSetGroupPolicy",
        /// Set the desired execution policy.
            string executionPolicy = SecuritySupport.GetExecutionPolicy(ExecutionPolicy);
            if (ShouldProcessPolicyChange(executionPolicy))
                    SecuritySupport.SetExecutionPolicy(_executionPolicyScope, ExecutionPolicy, shellId);
                catch (UnauthorizedAccessException exception)
                    OnAccessDeniedError(exception);
                catch (System.Security.SecurityException exception)
                // Ensure it is now the effective execution policy
                if (ExecutionPolicy != ExecutionPolicy.Undefined)
                    string effectiveExecutionPolicy = SecuritySupport.GetExecutionPolicy(shellId).ToString();
                    if (!string.Equals(effectiveExecutionPolicy, executionPolicy, StringComparison.OrdinalIgnoreCase))
                        string message = StringUtil.Format(ExecutionPolicyCommands.ExecutionPolicyOverridden, effectiveExecutionPolicy);
                        string recommendedAction = ExecutionPolicyCommands.ExecutionPolicyOverriddenRecommendedAction;
                            new System.Security.SecurityException(),
                            "ExecutionPolicyOverride",
                        errorRecord.ErrorDetails.RecommendedAction = recommendedAction;
                PSEtwLog.LogSettingsEvent(MshLog.GetLogContext(Context, MyInvocation),
                    EtwLoggingStrings.ExecutionPolicyName, executionPolicy, null);
        // Determine if we should process this policy change
#if CORECLR // Seems that we cannot find if the cmdlet is executed interactive or through a script on CoreCLR
        private bool ShouldProcessPolicyChange(string localPreference)
            return ShouldProcess(localPreference);
            if (ShouldProcess(localPreference))
                // command line. In that case, give a warning.
                    // We don't give this warning if we're in a script, or
                    // if we don't have a window handle
                    // (i.e.: PowerShell -command Set-ExecutionPolicy Unrestricted)
                    if (IsProcessInteractive())
                        string query = ExecutionPolicyCommands.SetExecutionPolicyQuery;
                        string caption = ExecutionPolicyCommands.SetExecutionPolicyCaption;
                            bool yesToAllNoToAllDefault = false;
                            if (!ShouldContinue(query, caption, true, ref yesToAllNoToAllDefault, ref yesToAllNoToAllDefault))
                            // Host is non-interactive. This should
                            // return false, but must return true due
                            // to backward compatibility.
                            // Host doesn't implement ShouldContinue. This should
        private bool IsProcessInteractive()
            // CommandOrigin != Runspace means it is in a script
            if (MyInvocation.CommandOrigin != CommandOrigin.Runspace)
            // If we don't own the window handle, we've been invoked
            // from another process that just calls "PowerShell -Command"
            if (System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle == IntPtr.Zero)
            // If the window has been idle for less than a second,
            // they're probably still calling "PowerShell -Command"
            // but from Start-Process, or the StartProcess API
                System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                TimeSpan timeSinceStart = DateTime.Now - currentProcess.StartTime;
                TimeSpan idleTime = timeSinceStart - currentProcess.TotalProcessorTime;
                if (idleTime.TotalSeconds > 1)
                // Don't have access to the properties
        // Throw terminating error when the access to the registry is denied
        private void OnAccessDeniedError(Exception exception)
            string message = StringUtil.Format(ExecutionPolicyCommands.SetExecutionPolicyAccessDeniedError, exception.Message);
                exception.GetType().FullName,

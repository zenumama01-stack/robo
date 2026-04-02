    internal class PowerShellExecutionHelper
        // Creates a new PowerShellExecutionHelper with the PowerShell instance that will be used to execute the tab expansion commands
        // Used by the ISE
        internal PowerShellExecutionHelper(PowerShell powershell)
            CurrentPowerShell = powershell;
        // Gets and sets a flag set to false at the beginning of each tab completion and
        // set to true if a pipeline is stopped to indicate all commands should return empty matches
        internal bool CancelTabCompletion { get; set; }
        // Gets and sets the PowerShell instance used to run command completion commands
        internal PowerShell CurrentPowerShell { get; set; }
        // Returns true if this instance is currently executing a command
        internal bool IsRunning => CurrentPowerShell.InvocationStateInfo.State == PSInvocationState.Running;
        // Returns true if the command executed by this instance was stopped
        internal bool IsStopped => CurrentPowerShell.InvocationStateInfo.State == PSInvocationState.Stopped;
        #region Command Execution
        internal bool ExecuteCommandAndGetResultAsBool()
            Collection<PSObject> streamResults = ExecuteCurrentPowerShell(out exceptionThrown);
            if (exceptionThrown != null || streamResults == null || streamResults.Count == 0)
            return (streamResults.Count > 1) || (LanguagePrimitives.IsTrue(streamResults[0]));
        internal Collection<PSObject> ExecuteCurrentPowerShell(out Exception exceptionThrown, IEnumerable input = null)
            return ExecuteCurrentPowerShell(out exceptionThrown, out _, input);
        internal Collection<PSObject> ExecuteCurrentPowerShell(out Exception exceptionThrown, out bool hadErrors, IEnumerable input = null)
            // This flag indicates a previous call to this method had its pipeline cancelled
            if (CancelTabCompletion)
                hadErrors = false;
                results = CurrentPowerShell.Invoke(input);
                // If this pipeline has been stopped lets set a flag to cancel all future tab completion calls
                // until the next completion
                if (IsStopped)
                    CancelTabCompletion = true;
                hadErrors = CurrentPowerShell.HadErrors;
                CurrentPowerShell.Commands.Clear();
        #endregion Command Execution
        /// Converts an object to a string safely...
        /// <param name="obj">The object to convert.</param>
        /// <returns>The result of the conversion...</returns>
        internal static string SafeToString(object obj)
                if (obj is PSObject pso)
                    if (baseObject != null && baseObject is not PSCustomObject)
                        result = baseObject.ToString();
                        result = pso.ToString();
                    result = obj.ToString();
                // We swallow all exceptions from command completion because we don't want the shell to crash
        /// Converts an object to a string adn, if the string is not empty, adds it to the list.
        /// <param name="list">The list to update.</param>
        /// <param name="obj">The object to convert to a string...</param>
        internal static void SafeAddToStringList(List<string> list, object obj)
            string result = SafeToString(obj);
                list.Add(result);
    internal static class PowerShellExtensionHelpers
        internal static PowerShell AddCommandWithPreferenceSetting(this PowerShellExecutionHelper helper,
            string command, Type type = null)
            return helper.CurrentPowerShell.AddCommandWithPreferenceSetting(command, type);
        internal static PowerShell AddCommandWithPreferenceSetting(this PowerShell powershell, string command, Type type = null)
            Diagnostics.Assert(powershell != null, "the passed-in powershell cannot be null");
            Diagnostics.Assert(!string.IsNullOrWhiteSpace(command),
                "the passed-in command name should not be null or whitespaces");
                var cmdletInfo = new CmdletInfo(command, type);
                powershell.AddCommand(cmdletInfo);
                powershell.AddCommand(command);

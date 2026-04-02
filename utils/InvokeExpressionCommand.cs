    /// Class implementing Invoke-Expression.
    [Cmdlet(VerbsLifecycle.Invoke, "Expression", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097030")]
    class
    InvokeExpressionCommand : PSCmdlet
        /// Command to execute.
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public string Command { get; set; }
        /// For each record, execute it, and push the results into the success stream.
            Diagnostics.Assert(Command != null, "Command is null");
            ScriptBlock myScriptBlock = InvokeCommand.NewScriptBlock(Command);
            // If the runspace has ever been in ConstrainedLanguage, lock down this
            // invocation as well - it is too easy for the command to be negatively influenced
            // by malicious input (such as ReadOnly + Constant variables)
            if (Context.HasRunspaceEverUsedConstrainedLanguageMode)
                myScriptBlock.LanguageMode = PSLanguageMode.ConstrainedLanguage;
            if (SystemPolicy.GetSystemLockdownPolicy() == SystemEnforcementMode.Audit)
                    title: UtilityCommonStrings.IEXWDACLogTitle,
                    message: UtilityCommonStrings.IEXWDACLogMessage,
                    fqid: "InvokeExpressionCmdletConstrained",
            myScriptBlock.InvokeUsingCmdlet(
                dollarUnder: AutomationNull.Value,

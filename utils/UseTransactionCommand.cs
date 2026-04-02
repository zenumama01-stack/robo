    [Cmdlet(VerbsOther.Use, "Transaction", SupportsTransactions = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=135271")]
    public class UseTransactionCommand : PSCmdlet
        /// This parameter specifies the script block to run in the current
        /// PowerShell transaction.
        public ScriptBlock TransactedScript
                return _transactedScript;
                _transactedScript = value;
        private ScriptBlock _transactedScript;
            using (CurrentPSTransaction)
                    var emptyArray = Array.Empty<object>();
                    _transactedScript.InvokeUsingCmdlet(
                        contextCmdlet: this,
                        useLocalScope: false,
                        errorHandlingBehavior: ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe,
                        dollarUnder: null,
                        input: emptyArray,
                        scriptThis: AutomationNull.Value,
                        args: emptyArray);
                    // Catch-all OK. This is a third-party call-out.
                    ErrorRecord errorRecord = new ErrorRecord(e, "TRANSACTED_SCRIPT_EXCEPTION", ErrorCategory.NotSpecified, null);
                    // The "transaction timed out" exception is
                    // exceedingly obtuse. We clarify things here.
                    bool isTimeoutException = false;
                    Exception tempException = e;
                    while (tempException != null)
                        if (tempException is System.TimeoutException)
                            isTimeoutException = true;
                        tempException = tempException.InnerException;
                    if (isTimeoutException)
                        errorRecord = new ErrorRecord(
                            new InvalidOperationException(
                                TransactionResources.TransactionTimedOut),
                            "TRANSACTION_TIMEOUT",

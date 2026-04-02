    /// A command that gets the active transaction.
    [Cmdlet(VerbsCommon.Get, "Transaction", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=135220")]
    [OutputType(typeof(PSTransaction))]
    public class GetTransactionCommand : PSCmdlet
        /// Creates a new transaction.
            WriteObject(this.Context.TransactionManager.GetCurrent());

    /// A command that rolls back a transaction.
    [Cmdlet(VerbsCommon.Undo, "Transaction", SupportsShouldProcess = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=135268")]
    public class UndoTransactionCommand : PSCmdlet
        /// Rolls the current transaction back.
            // Rollback the transaction
                NavigationResources.RollbackAction))
                this.Context.TransactionManager.Rollback();

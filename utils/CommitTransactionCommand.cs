    /// A command that commits a transaction.
    [Cmdlet(VerbsLifecycle.Complete, "Transaction", SupportsShouldProcess = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=135200")]
    public class CompleteTransactionCommand : PSCmdlet
        /// Commits the current transaction.
            // Commit the transaction
            if (ShouldProcess(
                NavigationResources.TransactionResource,
                NavigationResources.CommitAction))
                this.Context.TransactionManager.Commit();

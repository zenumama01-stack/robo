    /// A command that begins a transaction.
    [Cmdlet(VerbsLifecycle.Start, "Transaction", SupportsShouldProcess = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=135262")]
    public class StartTransactionCommand : PSCmdlet
        /// The time, in minutes, before this transaction is rolled back
        /// automatically.
        [Alias("TimeoutMins")]
                return (int)_timeout.TotalMinutes;
                // The transactions constructor treats a timeout of
                // zero as infinite. So we fudge it to be a bit longer.
                if (value == 0)
                    _timeout = TimeSpan.FromTicks(1);
                    _timeout = TimeSpan.FromMinutes(value);
        private TimeSpan _timeout = TimeSpan.MinValue;
        /// Gets or sets the flag to determine if this transaction can
        /// be committed or rolled back independently of other transactions.
        public SwitchParameter Independent
            get { return _independent; }
            set { _independent = value; }
        private SwitchParameter _independent;
        /// Gets or sets the rollback preference for this transaction.
        public RollbackSeverity RollbackPreference
            get { return _rollbackPreference; }
            set { _rollbackPreference = value; }
        private RollbackSeverity _rollbackPreference = RollbackSeverity.Error;
                NavigationResources.CreateAction))
                // Set the default timeout
                if (!_timeoutSpecified)
                    // See if we're being invoked directly at the
                    // command line. In that case, set the timeout to infinite.
                    if (MyInvocation.CommandOrigin == CommandOrigin.Runspace)
                        _timeout = TimeSpan.MaxValue;
                        _timeout = TimeSpan.FromMinutes(30);
                // Create the new transaction
                if (_independent)
                    this.Context.TransactionManager.CreateNew(_rollbackPreference, _timeout);
                    this.Context.TransactionManager.CreateOrJoin(_rollbackPreference, _timeout);

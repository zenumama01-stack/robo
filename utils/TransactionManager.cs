    /// The status of a PowerShell transaction.
    public enum PSTransactionStatus
        /// The transaction has been rolled back.
        RolledBack = 0,
        /// The transaction has been committed.
        Committed = 1,
        /// The transaction is currently active.
        Active = 2
    /// Represents an active transaction.
    public sealed class PSTransaction : IDisposable
        /// Initializes a new instance of the PSTransaction class.
        internal PSTransaction(RollbackSeverity rollbackPreference, TimeSpan timeout)
            _transaction = new CommittableTransaction(timeout);
            RollbackPreference = rollbackPreference;
            _subscriberCount = 1;
        /// Initializes a new instance of the PSTransaction class using a CommittableTransaction.
        internal PSTransaction(CommittableTransaction transaction, RollbackSeverity severity)
            _transaction = transaction;
            RollbackPreference = severity;
        private CommittableTransaction _transaction;
        /// Gets the rollback preference for this transaction.
        public RollbackSeverity RollbackPreference { get; }
        /// Gets the number of subscribers to this transaction.
        public int SubscriberCount
                // Verify the transaction hasn't been rolled back beneath us
                if (this.IsRolledBack)
                    this.SubscriberCount = 0;
                return _subscriberCount;
            set { _subscriberCount = value; }
        private int _subscriberCount;
        /// Returns the status of this transaction.
        public PSTransactionStatus Status
                if (IsRolledBack)
                    return PSTransactionStatus.RolledBack;
                else if (IsCommitted)
                    return PSTransactionStatus.Committed;
                    return PSTransactionStatus.Active;
        /// Activates the transaction held by this PSTransaction.
        internal void Activate()
            Transaction.Current = _transaction;
        /// Commits the transaction held by this PSTransaction.
        internal void Commit()
            _transaction.Commit();
            IsCommitted = true;
        /// Rolls back the transaction held by this PSTransaction.
        internal void Rollback()
            _transaction.Rollback();
            _isRolledBack = true;
        /// Determines whether this PSTransaction has been
        /// rolled back or not.
        internal bool IsRolledBack
                // Check if it's been aborted underneath us
                    (!_isRolledBack) &&
                    (_transaction != null) &&
                    (_transaction.TransactionInformation.Status == TransactionStatus.Aborted))
                return _isRolledBack;
                _isRolledBack = value;
        private bool _isRolledBack = false;
        /// Determines whether this PSTransaction
        /// has been committed or not.
        internal bool IsCommitted { get; set; } = false;
        /// Destructor for the PSTransaction class.
        ~PSTransaction()
        /// Disposes the PSTransaction object.
        /// Disposes the PSTransaction object, which disposes the
        /// underlying transaction.
                if (_transaction != null)
                    _transaction.Dispose();
    /// Supports the transaction management infrastructure for the PowerShell engine.
        /// Initializes a new instance of the PSTransactionManager class.
        internal PSTransactionContext(PSTransactionManager transactionManager)
            _transactionManager = transactionManager;
            transactionManager.SetActive();
        private PSTransactionManager _transactionManager;
        /// Destructor for the PSTransactionManager class.
        ~PSTransactionContext()
        /// Disposes the PSTransactionContext object.
        /// Disposes the PSTransactionContext object, which resets the
        /// active PSTransaction.
                _transactionManager.ResetActive();
        internal PSTransactionManager()
            _transactionStack = new Stack<PSTransaction>();
            _transactionStack.Push(null);
            if (s_engineProtectionEnabled && (Transaction.Current != null))
                return new System.Transactions.TransactionScope(
                    System.Transactions.TransactionScopeOption.Suppress);
        /// Called by the transaction manager to enable engine
        /// protection the first time a transaction is activated.
        /// Engine protection APIs remain protected from this point on.
        internal static void EnableEngineProtection()
            s_engineProtectionEnabled = true;
        private static bool s_engineProtectionEnabled = false;
                PSTransaction currentTransaction = _transactionStack.Peek();
                if (currentTransaction == null)
                    string error = TransactionStrings.NoTransactionActive;
                    // This is not an expected condition, and is just protective
                    // coding.
                return currentTransaction.RollbackPreference;
        /// Creates a new Transaction if none are active. Otherwise, increments
        /// the subscriber count for the active transaction.
        internal void CreateOrJoin()
            CreateOrJoin(RollbackSeverity.Error, TimeSpan.FromMinutes(1));
        internal void CreateOrJoin(RollbackSeverity rollbackPreference, TimeSpan timeout)
            // There is a transaction on the stack
            if (currentTransaction != null)
                // If you are already in a transaction that has been aborted, or committed,
                // create it.
                if (currentTransaction.IsRolledBack || currentTransaction.IsCommitted)
                    // Clean up the "used" one
                    _transactionStack.Pop().Dispose();
                    // And add a new one to the stack
                    _transactionStack.Push(new PSTransaction(rollbackPreference, timeout));
                    // This is a usable one. Add a subscriber to it.
                    currentTransaction.SubscriberCount++;
                // Add a new transaction to the stack
        /// Creates a new Transaction that should be managed independently of
        /// any parent transactions.
        internal void CreateNew()
            CreateNew(RollbackSeverity.Error, TimeSpan.FromMinutes(1));
        internal void CreateNew(RollbackSeverity rollbackPreference, TimeSpan timeout)
        /// Completes the current transaction. If only one subscriber is active, this
        /// commits the transaction. Otherwise, it reduces the subscriber count by one.
            // Should not be able to commit a transaction that is not active
                string error = TransactionStrings.NoTransactionActiveForCommit;
            // If you are already in a transaction that has been aborted
            if (currentTransaction.IsRolledBack)
                string error = TransactionStrings.TransactionRolledBackForCommit;
                throw new TransactionAbortedException(error);
            // If you are already in a transaction that has been committed
            if (currentTransaction.IsCommitted)
                string error = TransactionStrings.CommittedTransactionForCommit;
            if (currentTransaction.SubscriberCount == 1)
                currentTransaction.Commit();
                currentTransaction.SubscriberCount = 0;
                currentTransaction.SubscriberCount--;
            // Now that we've committed, go back to the last available transaction
            while ((_transactionStack.Count > 2) &&
                (_transactionStack.Peek().IsRolledBack || _transactionStack.Peek().IsCommitted))
            Rollback(false);
            // Should not be able to roll back a transaction that is not active
                string error = TransactionStrings.NoTransactionActiveForRollback;
                if (!suppressErrors)
                    // Otherwise, you should not be able to roll it back.
                    string error = TransactionStrings.TransactionRolledBackForRollback;
            // See if they've already committed the transaction
                    string error = TransactionStrings.CommittedTransactionForRollback;
            // Roll back the transaction if it hasn't been rolled back
            currentTransaction.Rollback();
            // Now that we've rolled back, go back to the last available transaction
        /// Sets the base transaction; any transactions created thereafter will be nested to this instance.
        internal void SetBaseTransaction(CommittableTransaction transaction, RollbackSeverity severity)
            if (this.HasTransaction)
                throw new InvalidOperationException(TransactionStrings.BaseTransactionMustBeFirst);
            // If there is a "used" transaction at the top of the stack, clean it up
            while (_transactionStack.Peek() != null &&
            _baseTransaction = new PSTransaction(transaction, severity);
            _transactionStack.Push(_baseTransaction);
        /// Removes the transaction added by SetBaseTransaction.
        internal void ClearBaseTransaction()
            if (_baseTransaction == null)
                throw new InvalidOperationException(TransactionStrings.BaseTransactionNotSet);
            if (_transactionStack.Peek() != _baseTransaction)
                throw new InvalidOperationException(TransactionStrings.BaseTransactionNotActive);
            _baseTransaction = null;
        private Stack<PSTransaction> _transactionStack;
        private PSTransaction _baseTransaction;
        /// Returns the current engine transaction.
        internal PSTransaction GetCurrent()
            return _transactionStack.Peek();
        /// Activates the current transaction, both in the engine, and in the Ambient.
        internal void SetActive()
            PSTransactionManager.EnableEngineProtection();
            // Should not be able to activate a transaction that is not active
                string error = TransactionStrings.NoTransactionForActivation;
            // If you are already in a transaction that has been aborted, you should
            // not be able to activate it.
                string error = TransactionStrings.NoTransactionForActivationBecauseRollback;
            _previousActiveTransaction = Transaction.Current;
            currentTransaction.Activate();
        private Transaction _previousActiveTransaction;
        /// Deactivates the current transaction in the engine, and restores the
        /// ambient transaction.
        internal void ResetActive()
            // Even if you are in a transaction that has been aborted, you
            // should still be able to restore the current transaction.
            Transaction.Current = _previousActiveTransaction;
            _previousActiveTransaction = null;
                if ((currentTransaction != null) &&
                    (!currentTransaction.IsCommitted) &&
                    (!currentTransaction.IsRolledBack))
                    return currentTransaction.IsCommitted;
                    return currentTransaction.IsRolledBack;
        ~PSTransactionManager()
        /// Disposes the PSTransactionManager object.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "baseTransaction", Justification = "baseTransaction should not be disposed since we do not own it - it belongs to the caller")]
                ResetActive();
                while (_transactionStack.Peek() != null)
                    PSTransaction currentTransaction = _transactionStack.Pop();
                    if (currentTransaction != _baseTransaction)
                        currentTransaction.Dispose();

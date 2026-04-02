using System.Transactions;
namespace Microsoft.PowerShell.Commands.Management
    /// Represents a string that can be used in transactions.
    public class TransactedString : IEnlistmentNotification
        private StringBuilder _value;
        private StringBuilder _temporaryValue;
        private Transaction _enlistedTransaction = null;
        /// Constructor for the TransactedString class.
        public TransactedString() : this(string.Empty)
        /// The initial value of the transacted string.
        public TransactedString(string value)
            _value = new StringBuilder(value);
            _temporaryValue = null;
        /// Make the transacted changes permanent.
        void IEnlistmentNotification.Commit(Enlistment enlistment)
            _value = new StringBuilder(_temporaryValue.ToString());
            _enlistedTransaction = null;
            enlistment.Done();
        /// Discard the transacted changes.
        void IEnlistmentNotification.Rollback(Enlistment enlistment)
        void IEnlistmentNotification.InDoubt(Enlistment enlistment)
        void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
            preparingEnlistment.Prepared();
        /// Append text to the transacted string.
        /// The text to append.
        public void Append(string text)
            ValidateTransactionOrEnlist();
            if (_enlistedTransaction != null)
                _temporaryValue.Append(text);
                _value.Append(text);
        /// Remove text from the transacted string.
        /// The position in the string from which to start removing.
        /// <param name="length">
        /// The length of text to remove.
        public void Remove(int startIndex, int length)
                _temporaryValue.Remove(startIndex, length);
                _value.Remove(startIndex, length);
        /// Gets the length of the transacted string. If this is
        /// called within the transaction, it returns the length of
        /// the transacted value. Otherwise, it returns the length of
        /// the original value.
        public int Length
                // If we're not in a transaction, or we are in a different transaction than the one we
                // enlisted to, return the publicly visible state.
                    (Transaction.Current == null) ||
                    (_enlistedTransaction != Transaction.Current))
                    return _value.Length;
                    return _temporaryValue.Length;
        /// Gets the System.String that represents the transacted
        /// transacted string. If this is called within the
        /// transaction, it returns the transacted value.
        /// Otherwise, it returns the original value.
                return _value.ToString();
                return _temporaryValue.ToString();
        private void ValidateTransactionOrEnlist()
            // We're in a transaction
            if (Transaction.Current != null)
                // We haven't yet been called inside of a transaction. So enlist
                // in the transaction, and store our save point
                if (_enlistedTransaction == null)
                    Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                    _enlistedTransaction = Transaction.Current;
                    _temporaryValue = new StringBuilder(_value.ToString());
                // We're already enlisted in a transaction
                    // And we're in that transaction
                    if (Transaction.Current != _enlistedTransaction)
                        throw new InvalidOperationException("Cannot modify string. It has been modified by another transaction.");
            // We're not in a transaction
                // If we're not subscribed to a transaction, modify the underlying value

    /// An interface designed to provide updates about an asynchronous operation.
    /// If the UI is data bound to the properties in this interface then INotifyPropertyChanged should
    /// be implemented by the type implementing IAsyncProgress so the UI can get notification of the properties
    /// being changed.
    public interface IAsyncProgress
        /// Gets a value indicating whether the async operation is currently running.
        bool OperationInProgress
        /// Gets a the error for the async operation.  This field is only valid if
        /// OperationInProgress is false.  null indicates there was no error.
        Exception OperationError

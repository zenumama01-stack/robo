using Microsoft.Management.Infrastructure.Options;
    /// Base action class, implemented to write results to pipeline.
    internal abstract class CimBaseAction
        /// Initializes a new instance of the <see cref="CimBaseAction"/> class.
        protected CimBaseAction()
        /// Execute the write operation to given cmdlet object
        /// <param name="cmdlet">
        /// cmdlet wrapper object, to which write result.
        /// <see cref="CmdletOperationBase"/> for details.
        public virtual void Execute(CmdletOperationBase cmdlet)
        /// <see cref="XOperationContextBase"/> object that related to current action.
        /// It may used by action, such as <see cref="CimWriteResultObject"/>,
        /// since later on action may require namespace, and proxy object to reuse
        /// <see cref="CimSession"/>, <see cref="CimOperationOptions"/> object.
        protected XOperationContextBase Context { get; set; }
    /// Synchronous action class, implemented to write results to pipeline
    /// and block current thread until the action is completed.
    internal class CimSyncAction : CimBaseAction, IDisposable
        /// Initializes a new instance of the <see cref="CimSyncAction"/> class.
        public CimSyncAction()
            this.completeEvent = new ManualResetEventSlim(false);
            this.responseType = CimResponseType.None;
        /// Block current thread until action completed
        /// <returns>Response from user.</returns>
        public virtual CimResponseType GetResponse()
            this.Block();
            return responseType;
        /// Set the response result.
        internal CimResponseType ResponseType
            set { this.responseType = value; }
        /// Call this method when the action is completed or
        /// the operation is terminated.
        internal virtual void OnComplete()
            this.completeEvent.Set();
        /// Block current thread.
        protected virtual void Block()
            this.completeEvent.Wait();
            this.completeEvent.Dispose();
        #region members
        /// Action completed event.
        private readonly ManualResetEventSlim completeEvent;
        /// Response result.
        protected CimResponseType responseType;
        #region IDisposable interface
        /// IDisposable interface.
        private bool _disposed;
            // Check to see if Dispose has already been called.
            if (!this._disposed)
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                    // Dispose managed resources.
                    this.completeEvent?.Dispose();
                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                // Note disposing has been done.
                _disposed = true;

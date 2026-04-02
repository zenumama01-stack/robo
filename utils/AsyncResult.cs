    /// Base class for AsyncResult objects that are returned by various
    /// Async operations supported by RunspacePool , PowerShell types.
    internal class AsyncResult : IAsyncResult
        private ManualResetEvent _completedWaitHandle;
        // exception occurred in the async thread.
        // user supplied state object
        // Invoke on thread (remote debugging support).
        private AutoResetEvent _invokeOnThreadEvent;
        private WaitCallback _invokeCallback;
        private object _invokeCallbackState;
        /// <param name="ownerId">
        /// Instance Id of the object creating this instance
        /// <param name="callback">
        /// A AsyncCallback to call once the async operation completes.
        /// <param name="state">
        /// A user supplied state object
        internal AsyncResult(Guid ownerId, AsyncCallback callback, object state)
            Dbg.Assert(ownerId != Guid.Empty, "ownerId cannot be empty");
            OwnerId = ownerId;
            Callback = callback;
            AsyncState = state;
        #region IAsync Overrides
        /// This always returns false.
        public bool CompletedSynchronously
        /// Gets an indication whether the asynchronous operation has completed.
        public bool IsCompleted { get; private set; }
        /// This is not supported and returns null.
        public object AsyncState { get; }
        /// Gets a System.Threading.WaitHandle that is used to wait for an asynchronous
        /// operation to complete.
        public WaitHandle AsyncWaitHandle
                if (_completedWaitHandle == null)
                    lock (SyncObject)
                        _completedWaitHandle ??= new ManualResetEvent(IsCompleted);
                return _completedWaitHandle;
        #region properties / methods
        /// Instance Id of the object owning this async result.
        internal Guid OwnerId { get; }
        /// Gets the exception that occurred while processing the
        /// async operation.
        /// User supplied callback.
        internal AsyncCallback Callback { get; }
        /// SyncObject.
        internal object SyncObject { get; } = new object();
        /// Marks the async operation as completed.
        /// Exception occurred. null if no exception occurred
        internal void SetAsCompleted(Exception exception)
            // Dbg.Assert(!isCompleted, "AsynResult already completed");
            if (IsCompleted)
                    IsCompleted = true;
                    // release the threads waiting on this operation.
                    SignalWaitHandle();
            // call the user supplied callback
            Callback?.Invoke(this);
        /// Release the asyncResult without calling the callback.
        internal void Release()
            if (!IsCompleted)
        /// Signal wait handle of this async result.
        internal void SignalWaitHandle()
                _completedWaitHandle?.Set();
        /// Wait for the operation to complete and throw the exception if any.
        internal void EndInvoke()
            _invokeOnThreadEvent = new AutoResetEvent(false);
            // Start the thread wait loop.
            WaitHandle[] waitHandles = new WaitHandle[2] { AsyncWaitHandle, _invokeOnThreadEvent };
            bool waiting = true;
            while (waiting)
                int waitIndex = WaitHandle.WaitAny(waitHandles);
                if (waitIndex == 0)
                    waiting = false;
                    // Invoke callback on thread.
                        _invokeCallback(_invokeCallbackState);
            AsyncWaitHandle.Dispose();
            _completedWaitHandle = null;  // Allow early GC
            _invokeOnThreadEvent.Dispose();
            _invokeOnThreadEvent = null;  // Allow early GC
            // Operation is done: if an exception occurred, throw it
                throw Exception;
        /// Use blocked thread to invoke callback delegate.
        /// <param name="callback">Callback delegate.</param>
        /// <param name="state">Callback state.</param>
        internal bool InvokeCallbackOnThread(WaitCallback callback, object state)
            if (callback == null)
                throw new PSArgumentNullException(nameof(callback));
            _invokeCallback = callback;
            _invokeCallbackState = state;
            // Signal thread to run callback.
            if (_invokeOnThreadEvent != null)
                _invokeOnThreadEvent.Set();

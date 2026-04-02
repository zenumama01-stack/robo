    ///     An object that can be used to execute a method on a threadpool thread while correctly
    ///     managing system state, such as flowing ETW activities from the current thread to the
    ///     threadpool thread.
    public interface IBackgroundDispatcher
        ///     Works the same as <see cref="ThreadPool.QueueUserWorkItem(WaitCallback)" />, except that it
        ///     also manages system state correctly.
        bool QueueUserWorkItem(WaitCallback callback);
        ///     Works the same as <see cref="ThreadPool.QueueUserWorkItem(WaitCallback, object)" />, except that it
        bool QueueUserWorkItem(WaitCallback callback, object state);
        ///     Works the same as BeginInvoke would for any other delegate, except that it also manages system state correctly.
        IAsyncResult BeginInvoke(WaitCallback callback, object state, AsyncCallback completionCallback, object asyncState);
        ///     Works the same as EndInvoke would for any other delegate, except that it also manages system state correctly.
        void EndInvoke(IAsyncResult asyncResult);
    ///     A simple implementation of <see cref="IBackgroundDispatcher" />.
    public class BackgroundDispatcher :
        IBackgroundDispatcher
        private readonly IMethodInvoker _etwActivityMethodInvoker;
        private readonly WaitCallback _invokerWaitCallback;
        #region Creation/Cleanup
        ///     Creates a <see cref="BackgroundDispatcher" /> that uses an <see cref="EtwEventCorrelator" />
        ///     for activity creation and correlation.
        /// <param name="transferProvider">The <see cref="EventProvider" /> to use when logging transfer events
        ///     during activity correlation.</param>
        /// <param name="transferEvent">The <see cref="EventDescriptor" /> to use when logging transfer events
        public BackgroundDispatcher(EventProvider transferProvider, EventDescriptor transferEvent)
            : this(new EtwActivityReverterMethodInvoker(new EtwEventCorrelator(transferProvider, transferEvent)))
        // internal for unit testing only.  Otherwise, would be private.
        internal BackgroundDispatcher(IMethodInvoker etwActivityMethodInvoker)
            ArgumentNullException.ThrowIfNull(etwActivityMethodInvoker);
            _etwActivityMethodInvoker = etwActivityMethodInvoker;
            _invokerWaitCallback = DoInvoker;
        #region Instance Utilities
        private void DoInvoker(object invokerArgs)
            var invokerArgsArray = (object[])invokerArgs;
            _etwActivityMethodInvoker.Invoker.DynamicInvoke(invokerArgsArray);
        #region Instance Access
        ///     Implements <see cref="IBackgroundDispatcher.QueueUserWorkItem(WaitCallback)" />.
        public bool QueueUserWorkItem(WaitCallback callback)
            return QueueUserWorkItem(callback, null);
        ///     Implements <see cref="IBackgroundDispatcher.QueueUserWorkItem(WaitCallback, object)" />.
        public bool QueueUserWorkItem(WaitCallback callback, object state)
            var invokerArgs = _etwActivityMethodInvoker.CreateInvokerArgs(callback, new object[] { state });
            var result = ThreadPool.QueueUserWorkItem(_invokerWaitCallback, invokerArgs);
        ///     Implements <see cref="IBackgroundDispatcher.BeginInvoke(WaitCallback, object, AsyncCallback, object)" />.
        public IAsyncResult BeginInvoke(WaitCallback callback, object state, AsyncCallback completionCallback, object asyncState)
            var result = _invokerWaitCallback.BeginInvoke(invokerArgs, completionCallback, asyncState);
        ///     Implements <see cref="IBackgroundDispatcher.EndInvoke(IAsyncResult)" />.
        public void EndInvoke(IAsyncResult asyncResult)
            _invokerWaitCallback.EndInvoke(asyncResult);

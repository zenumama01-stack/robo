    #region OperationState
    /// Defines the different states of the operation.
    internal enum OperationState
        /// Start operation completed successfully.
        StartComplete = 0,
        /// Stop operation completed successfully.
        StopComplete = 1,
    /// Class describing event args which a helper class
    /// implementing IThrottleOperation need to throw.
    internal sealed class OperationStateEventArgs : EventArgs
        /// Operation state.
        internal OperationState OperationState { get; set; }
        /// The original event which actually resulted in this
        /// event being raised.
        internal EventArgs BaseEvent { get; set; }
    #endregion OperationState
    #region IThrottleOperation
    /// Interface which needs to be implemented by a class which wants to
    /// submit operations to the throttle manager.
    /// <remarks>Any synchronization that needs to be performed between
    /// StartOperation and StopOperation in the class that implements this
    /// interface should take care of handling the same. For instance,
    /// say New-Runspace class internally uses a class A which implements
    /// the IThrottleOperation interface. StartOperation of this
    /// class opens a runspace asynchronously on a remote machine. Stop
    /// operation is supposed to cancel the opening of this runspace. Any
    /// synchronization/cleanup issues should be handled by class A.
    internal abstract class IThrottleOperation
        /// This method should handle the actual operation which need to be
        /// controlled and performed. Examples of this can be Opening remote
        /// runspace, invoking expression in a remote runspace, etc. Once
        /// an event is successfully received as a result of this function,
        /// the handler has to ensure that it raises an OperationComplete
        /// event with StartComplete or StopComplete for the throttle manager
        /// to handle.
        internal abstract void StartOperation();
        /// This method should handle the situation when a stop signal is sent
        /// for this operation. For instance, when trying to open a set of
        /// remote runspaces, the user might hit ctrl-C. In which case, the
        /// pending runspaces to be opened will actually be signalled through
        /// this method to stop operation and return back. This method also
        /// needs to be asynchronous. Once an event is successfully received
        /// as a result of this function, the handler has to ensure that it
        /// raises an OperationComplete event with StopComplete for the
        /// throttle manager to handle. It is important that this function
        /// does not raise a StartComplete which will then result in the
        /// ThrottleComplete event not being raised by the throttle manager.
        internal abstract void StopOperation();
        /// Event which will be triggered when the operation is complete. It is
        /// assumed that all the operations performed by StartOperation and
        /// StopOperation are asynchronous. The submitter of operations may
        /// subscribe to this event to know when it's complete (or it can handle
        /// the synchronization with its scheduler) and the throttle
        /// manager will subscribe to this event to know that it's complete
        /// and to start the operation on the next item.
        internal abstract event EventHandler<OperationStateEventArgs> OperationComplete;
        /// This Property indicates whether an operation has been stopped.
        /// In the initial implementation of ThrottleManager stopping
        /// individual operations was not supported. When the support
        /// for stopping individual operations was added, there was
        /// the following problem - if an operation is not there in
        /// the pending queue and in the startOperationQueue as well,
        /// then the following two scenarios are possible
        ///      (a) Operation was started and start completed
        ///      (b) Operation was started and stopped and both completed
        /// This property has been added in order to disambiguate between
        /// these two cases. When this property is set, StopOperation
        /// need not be called on the operation (this can be when the
        /// operation has stop completed or stop has been called and is
        /// pending)
        internal bool IgnoreStop
                return _ignoreStop;
                _ignoreStop = true;
        private bool _ignoreStop = false;
        /// When true enables runspace debugging for operations involving runspaces.
        internal bool RunspaceDebuggingEnabled
        /// When true configures runspace debugging to stop at first opportunity.
        internal bool RunspaceDebugStepInEnabled
        /// Event raised when operation runspace enters a debugger stopped state.
        internal event EventHandler<StartRunspaceDebugProcessingEventArgs> RunspaceDebugStop;
        /// RaiseRunspaceDebugStopEvent.
        internal void RaiseRunspaceDebugStopEvent(System.Management.Automation.Runspaces.Runspace runspace)
            RunspaceDebugStop.SafeInvoke(this, new StartRunspaceDebugProcessingEventArgs(runspace));
    #endregion IThrottleOperation
    #region ThrottleManager
    /// Class which handles the throttling operations. This class is singleton and therefore
    /// when used either across cmdlets or at the infrastructure level it will ensure that
    /// there aren't more operations by way of accumulation than what is intended by design.
    /// This class contains a queue of items, each of which has the
    /// <see cref="System.Management.Automation.Remoting.IThrottleOperation">
    /// IThrottleOperation</see> interface implemented. To begin with
    /// THROTTLE_LIMIT number of items will be taken from the queue and the operations on
    /// them will be executed. Subsequently, as and when operations complete, new items from
    /// the queue will be taken and their operations executed.
    /// Whenever a consumer submits or adds operations, the methods will start as much
    /// operations from the queue as permitted based on the throttle limit. Also the event
    /// handler will start an operation once a previous event is completed.
    /// The queue used is a generic queue of type IThrottleOperations, as it will offer better
    /// performance.
    /// <remarks>Throttle limit is currently set to 50. This value may be modified later based
    /// on a figure that we may arrive at out of experience.</remarks>
    internal class ThrottleManager : IDisposable
        #region Public (internal) Properties
        /// Allows the consumer to override the default throttle limit.
        internal int ThrottleLimit
                return _throttleLimit;
                if (value > 0 && value <= s_THROTTLE_LIMIT_MAX)
                    _throttleLimit = value;
        private int _throttleLimit = s_DEFAULT_THROTTLE_LIMIT;
        #endregion Public (internal) Properties
        #region Public (internal) Methods
        /// Submit a list of operations that need to be throttled.
        /// <param name="operations">List of operations to be throttled.</param>
        /// <remarks>Once the operations are added to the queue, the method will
        /// start operations from the queue
        internal void SubmitOperations(List<IThrottleOperation> operations)
                // operations can be submitted only until submitComplete
                // is not set to true (happens when EndSubmitOperations is called)
                if (!_submitComplete)
                    // add items to the queue
                        Dbg.Assert(operation != null,
                            "Operation submitComplete to throttle manager cannot be null");
                        _operationsQueue.Add(operation);
            // schedule operations here if possible
            StartOperationsFromQueue();
        /// Add a single operation to the queue.
        /// <param name="operation">Operation to be added.</param>
        internal void AddOperation(IThrottleOperation operation)
            // add item to the queue
            // start operations from queue if possible
        /// Stop throttling operations.
        /// <remarks>Calling this method will also affect other cmdlets which
        /// could have potentially submitComplete operations for processing
        /// <returns>Number of objects cleared from queue without being
        /// stopped.</returns>
            // if stopping is already in progress, make it a no op
            bool needToReturn = false;
                    needToReturn = true;
            if (needToReturn)
                RaiseThrottleManagerEvents();
            IThrottleOperation[] startOperationsInProcessArray;
                // no more submissions possible once stopped
                _submitComplete = true;
                // Clear all pending operations in queue so that they are not
                // scheduled when a stop operation completes
                _operationsQueue.Clear();
                // Make a copy of the in process queue so as to stop all
                // operations in progress
                startOperationsInProcessArray =
                        new IThrottleOperation[_startOperationQueue.Count];
                _startOperationQueue.CopyTo(startOperationsInProcessArray);
                // stop all operations in process (using the copy)
                foreach (IThrottleOperation operation in startOperationsInProcessArray)
                    // When iterating through the array of operations in process
                    // it is quite possible that a runspace gets to the open state
                    // before stop is actually called on it. In that case, the
                    // OperationCompleteHandler will remove it from the
                    // operationsInProcess queue. Now when the runspace is closed
                    // the same handler will try removing it again and so there will
                    // be an exception. Hence adding it a second time before stop
                    // will ensure that the operation is available in the queue for
                    // removal. In case the stop succeeds before start succeeds then
                    // both will get removed (it goes without saying that there cannot
                    // be a situation where start succeeds after stop succeeded)
                    _stopOperationQueue.Add(operation);
                    operation.IgnoreStop = true;
                operation.StopOperation();
            // Raise event as it can be that at this point, all operations are
            // complete
        /// Stop the specified operation.
        /// <param name="operation">Operation which needs to be stopped.</param>
        internal void StopOperation(IThrottleOperation operation)
            // StopOperation is being called a second time
            // or the stop operation has already completed
            // - in either case just return
            if (operation.IgnoreStop)
            // If the operation has not yet been started, then
            // remove it from the pending queue
            if (_operationsQueue.IndexOf(operation) != -1)
                        _operationsQueue.Remove(operation);
            // The operation has already started, then add it
            // to the inprocess queue and call stop. Refer to
            // comment in StopAllOperations() as to why this is
            // being added a second time
            // stop the operation outside of the lock
        /// Signals that no more operations can be submitComplete
        /// for throttling.
        internal void EndSubmitOperations()
        #endregion Public (internal) Methods
        #region Public (internal) Events
        /// Event raised when throttling all operations is complete.
        internal event EventHandler<EventArgs> ThrottleComplete;
        #endregion Public (internal) Events
        public ThrottleManager()
            _operationsQueue = new List<IThrottleOperation>();
            _startOperationQueue = new List<IThrottleOperation>();
            _stopOperationQueue = new List<IThrottleOperation>();
        /// Handler which handles state change for the object which implements
        /// the <see cref="System.Management.Automation.Remoting.IThrottleOperation"/>
        /// interface.
        /// <param name="source">Sender of the event.</param>
        /// <param name="stateEventArgs">Event information object which describes the event
        /// which triggered this method</param>
        private void OperationCompleteHandler(object source, OperationStateEventArgs stateEventArgs)
            // An item has completed operation. If it's a start operation which completed
            // remove the instance from the startOperationqueue. If it's a stop operation
            // which completed, then remove the instance from both queues
                IThrottleOperation operation = source as IThrottleOperation;
                Dbg.Assert(operation != null, "Source of event should not be null");
                if (stateEventArgs.OperationState == OperationState.StartComplete)
                    // A stop operation can be initiated before a start operation completes.
                    // A stop operation handler cleans up an outstanding start operation.
                    // So it is possible that a start operation complete callback will find the
                    // operation removed from the queue by an earlier stop operation complete.
                    index = _startOperationQueue.IndexOf(operation);
                        _startOperationQueue.RemoveAt(index);
                    // for a stop operation, the same operation object would have been
                    // added to the stopOperationQueue as well. So we need to
                    // remove both the instances.
                    index = _stopOperationQueue.IndexOf(operation);
                        _stopOperationQueue.RemoveAt(index);
                    // if an operation signals a stopcomplete, it can mean
                    // that the operation has completed. In this case, we
                    // need to set the isStopped to true
            // It's possible that all operations are completed at this point
            // and submit is complete. So raise event
            // Do necessary things for starting operation for the next item in the queue
            StartOneOperationFromQueue();
        /// Method used to start the operation on one item in the queue.
        private void StartOneOperationFromQueue()
            IThrottleOperation operation = null;
                if (_operationsQueue.Count > 0)
                    operation = _operationsQueue[0];
                    _operationsQueue.RemoveAt(0);
                    operation.OperationComplete += OperationCompleteHandler;
                    _startOperationQueue.Add(operation);
            operation?.StartOperation();
        /// Start operations to the limit possible from the queue.
        private void StartOperationsFromQueue()
            int operationsInProcessCount = 0;
            int operationsQueueCount = 0;
                operationsInProcessCount = _startOperationQueue.Count;
                operationsQueueCount = _operationsQueue.Count;
            int remainingCap = _throttleLimit - operationsInProcessCount;
            if (remainingCap > 0)
                int numOperations = (remainingCap > operationsQueueCount) ? operationsQueueCount : remainingCap;
                for (int i = 0; i < numOperations; i++)
        /// Raise the throttle manager events once the conditions are met.
        private void RaiseThrottleManagerEvents()
            bool readyToRaise = false;
                // if submit is complete, there are no operations in progress and
                // the pending queue is empty, then raise events
                if (_submitComplete &&
                    _startOperationQueue.Count == 0 &&
                    _stopOperationQueue.Count == 0 &&
                    _operationsQueue.Count == 0)
                    readyToRaise = true;
            if (readyToRaise)
                ThrottleComplete.SafeInvoke(this, EventArgs.Empty);
        /// Default throttle limit - the maximum number of operations
        /// to be processed at a time.
        private static readonly int s_DEFAULT_THROTTLE_LIMIT = 32;
        /// Maximum value that the throttle limit can be set to.
        private static readonly int s_THROTTLE_LIMIT_MAX = int.MaxValue;
        /// All pending operations.
        private readonly List<IThrottleOperation> _operationsQueue;
        /// List of items on which a StartOperation has
        /// been called.
        private readonly List<IThrottleOperation> _startOperationQueue;
        /// List of items on which a StopOperation has
        private readonly List<IThrottleOperation> _stopOperationQueue;
        /// Object used to synchronize access to the queues.
        private bool _submitComplete = false;                    // to check if operations have been submitComplete
        private bool _stopping = false;                      // if stop is in process
        /// Dispose method of IDisposable. Any cmdlet that uses
        /// the throttle manager needs to call this method from its
        /// Dispose method.
        /// Internal dispose method which does the actual dispose
        /// operations and finalize suppressions.
        /// <param name="disposing">If method is called from
        /// disposing of destructor</param>
                StopAllOperations();
    #endregion ThrottleManager
    #region Helper Class for Testing
#if !CORECLR // Skip The Helper Class for Testing (Thread.Abort() Not In CoreCLR)
    internal class Operation : IThrottleOperation
        private ThreadStart workerThreadDelegate;
        private Thread workerThreadStart;
        private Thread workerThreadStop;
        public bool Done { get; set; }
        public int SleepTime { get; set; } = 100;
        private void WorkerThreadMethodStart()
            Thread.Sleep(SleepTime);
            Done = true;
        private void WorkerThreadMethodStop()
            workerThreadStart.Abort();
        internal Operation()
            Done = false;
            workerThreadDelegate = new ThreadStart(WorkerThreadMethodStart);
            workerThreadStart = new Thread(workerThreadDelegate);
            workerThreadDelegate = new ThreadStart(WorkerThreadMethodStop);
            workerThreadStop = new Thread(workerThreadDelegate);
            workerThreadStart.Start();
            workerThreadStop.Start();
        internal event EventHandler<EventArgs> InternalEvent = null;
        internal event EventHandler<EventArgs> EventHandler
                bool firstEntry = (InternalEvent == null);
                InternalEvent += value;
                    OperationComplete += new EventHandler<OperationStateEventArgs>(Operation_OperationComplete);
                InternalEvent -= value;
        private void Operation_OperationComplete(object sender, OperationStateEventArgs e)
            InternalEvent.SafeInvoke(sender, e);
        internal static void SubmitOperations(List<object> operations, ThrottleManager throttleManager)
            List<IThrottleOperation> newOperations = new List<IThrottleOperation>();
            foreach (object operation in operations)
                newOperations.Add((IThrottleOperation)operation);
            throttleManager.SubmitOperations(newOperations);
        internal static void AddOperation(object operation, ThrottleManager throttleManager)
            throttleManager.AddOperation((IThrottleOperation)operation);
    #endregion Helper Class for Testing

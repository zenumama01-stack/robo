namespace System.Management.Automation.Runspaces.Internal
    /// Class which supports pooling local powerShell runspaces.
    internal class RunspacePoolInternal : IDisposable
        protected int maxPoolSz;
        protected int minPoolSz;
        // we need total active runspaces to avoid lock() statements everywhere
        protected int totalRunspaces;
        protected List<Runspace> runspaceList = new List<Runspace>(); // info of all the runspaces in the pool.
        protected Stack<Runspace> pool; // stack of runspaces that are available.
        protected Queue<GetRunspaceAsyncResult> runspaceRequestQueue; // request queue.
        // let requesters request on the runspaceRequestQueue..internally
        // pool services on this queue.
        protected Queue<GetRunspaceAsyncResult> ultimateRequestQueue;
        protected RunspacePoolStateInfo stateInfo;
        protected InitialSessionState _initialSessionState;
        protected PSHost host;
        protected Guid instanceId;
        protected bool isServicingRequests;
        protected object syncObject = new object();
        private static readonly TimeSpan s_defaultCleanupPeriod = new TimeSpan(0, 15, 0);   // 15 minutes.
        private TimeSpan _cleanupInterval;
        private readonly Timer _cleanupTimer;
        /// supplied <paramref name="configuration"/>, <paramref name="minRunspaces"/>
        /// and <paramref name="maxRunspaces"/>
        public RunspacePoolInternal(int minRunspaces,
            : this(minRunspaces, maxRunspaces)
            this.host = host;
            pool = new Stack<Runspace>();
            runspaceRequestQueue = new Queue<GetRunspaceAsyncResult>();
            ultimateRequestQueue = new Queue<GetRunspaceAsyncResult>();
            _initialSessionState = InitialSessionState.CreateDefault();
        /// InitialSessionState to use when creating a new Runspace.
            _initialSessionState = initialSessionState.Clone();
            ThreadOptions = initialSessionState.ThreadOptions;
        /// Constructor for doing common initialization between
        /// this class and its derivatives.
        protected RunspacePoolInternal(int minRunspaces, int maxRunspaces)
            if (maxRunspaces < 1)
                throw PSTraceSource.NewArgumentException(nameof(maxRunspaces), RunspacePoolStrings.MaxPoolLessThan1);
            if (minRunspaces < 1)
                throw PSTraceSource.NewArgumentException(nameof(minRunspaces), RunspacePoolStrings.MinPoolLessThan1);
            if (minRunspaces > maxRunspaces)
                throw PSTraceSource.NewArgumentException(nameof(minRunspaces), RunspacePoolStrings.MinPoolGreaterThanMaxPool);
            maxPoolSz = maxRunspaces;
            minPoolSz = minRunspaces;
            stateInfo = new RunspacePoolStateInfo(RunspacePoolState.BeforeOpen, null);
            instanceId = Guid.NewGuid();
            PSEtwLog.SetActivityIdForCurrentThread(instanceId);
            _cleanupInterval = s_defaultCleanupPeriod;
            _cleanupTimer = new Timer(new TimerCallback(CleanupCallback), null, Timeout.Infinite, Timeout.Infinite);
        internal RunspacePoolInternal() { }
                return instanceId;
                return _isDisposed;
                return stateInfo;
        internal virtual PSPrimitiveDictionary GetApplicationPrivateData()
                lock (this.syncObject)
        internal virtual void PropagateApplicationPrivateData(Runspace runspace)
            runspace.SetApplicationPrivateData(this.GetApplicationPrivateData());
                return _initialSessionState;
        /// The connection associated with this runspace pool.
        public virtual RunspaceConnectionInfo ConnectionInfo
                return _cleanupInterval;
                    _cleanupInterval = value;
        public virtual RunspacePoolAvailability RunspacePoolAvailability
                return (stateInfo.State == RunspacePoolState.Opened) ?
                    RunspacePoolAvailability.Available :
                    RunspacePoolAvailability.None;
        public event EventHandler<RunspacePoolStateChangedEventArgs> StateChanged;
        public event EventHandler<PSEventArgs> ForwardEvent;
        internal event EventHandler<RunspaceCreatedEventArgs> RunspaceCreated;
        #region Disconnect-Connect Methods
        /// Synchronously disconnect runspace pool.
        public virtual void Disconnect()
            throw PSTraceSource.NewInvalidOperationException(RunspacePoolStrings.RunspaceDisconnectConnectNotSupported);
        /// Asynchronously disconnect runspace pool.
        public virtual IAsyncResult BeginDisconnect(AsyncCallback callback, object state)
        /// Wait for BeginDisconnect to complete.
        public virtual void EndDisconnect(IAsyncResult asyncResult)
        /// Synchronously connect runspace pool.
        public virtual void Connect()
        /// Asynchronously connect runspace pool.
        public virtual IAsyncResult BeginConnect(AsyncCallback callback, object state)
        /// Wait for BeginConnect to complete.
        public virtual void EndConnect(IAsyncResult asyncResult)
        public virtual Collection<PowerShell> CreateDisconnectedPowerShells(RunspacePool runspacePool)
        public virtual RunspacePoolCapability GetCapabilities()
            return RunspacePoolCapability.Default;
        /// Resets the runspace state on a runspace pool with a single
        /// This is currently supported *only* for remote runspaces.
        internal virtual bool ResetRunspaceState()
        internal virtual bool SetMaxRunspaces(int maxRunspaces)
            bool isSizeIncreased = false;
            lock (pool)
                if (maxRunspaces < this.minPoolSz)
                if (maxRunspaces > this.maxPoolSz)
                    isSizeIncreased = true;
                    // since maxrunspaces limit is decreased
                    // destroy unwanted runspaces from the top
                    // of the pool.
                    while (pool.Count > maxRunspaces)
                        Runspace rsToDestroy = pool.Pop();
                        DestroyRunspace(rsToDestroy);
            // pool size is incremented.. check if we can release
            // some requests.
            if (isSizeIncreased)
                EnqueueCheckAndStartRequestServicingThread(null, false);
            return maxPoolSz;
        internal virtual bool SetMinRunspaces(int minRunspaces)
                if ((minRunspaces < 1) || (minRunspaces > this.maxPoolSz))
            return minPoolSz;
        /// If the RunspacePool failed or has been closed
        internal virtual int GetAvailableRunspaces()
            // Dont allow state changes while we get the count
            lock (syncObject)
                if (stateInfo.State == RunspacePoolState.Opened)
                    // Win8: 169492 RunspacePool can report that there are negative runspaces available.
                    // totalRunspaces represents all the runspaces that were ever created by ths RunspacePool
                    // pool.Count represents the runspaces that are currently available
                    // maxPoolSz represents the total capacity w.r.t runspaces for this RunspacePool
                    // Once the RunspacePool allocates a runspace to a consumer, RunspacePool cannot reclaim the
                    // runspace until the consumer released the runspace back to the pool. A SetMaxRunspaces()
                    // call can arrive before the runspace is released..It is bad to make SetMaxRunspaces()
                    // wait for the consumers to release runspaces, so we let SetMaxRunspaces() go by changing
                    // maxPoolSz. Because of this there may be cases where maxPoolSz - totalRunspaces will become
                    // less than 0.
                    int unUsedCapacity = (maxPoolSz - totalRunspaces) < 0 ? 0 : (maxPoolSz - totalRunspaces);
                    return (pool.Count + unUsedCapacity);
                else if (stateInfo.State == RunspacePoolState.Disconnected)
                    throw new InvalidOperationException(RunspacePoolStrings.CannotWhileDisconnected);
                else if (stateInfo.State != RunspacePoolState.BeforeOpen && stateInfo.State != RunspacePoolState.Opening)
                    throw new InvalidOperationException(HostInterfaceExceptionsStrings.RunspacePoolNotOpened);
        public virtual void Open()
            CoreOpen(false, null, null);
            return CoreOpen(true, callback, state);
            RunspacePoolAsyncResult rsAsyncResult = asyncResult as RunspacePoolAsyncResult;
            if ((rsAsyncResult == null) ||
                (rsAsyncResult.OwnerId != instanceId) ||
                (!rsAsyncResult.IsAssociatedWithAsyncOpen))
                                                         RunspacePoolStrings.AsyncResultNotOwned,
                                                         "IAsyncResult",
                                                         "BeginOpen");
            rsAsyncResult.EndInvoke();
            CoreClose(false, null, null);
        public virtual IAsyncResult BeginClose(AsyncCallback callback, object state)
            return CoreClose(true, callback, state);
        /// TODO: Behavior if EndClose is called multiple times.
        public virtual void EndClose(IAsyncResult asyncResult)
                (rsAsyncResult.IsAssociatedWithAsyncOpen))
                                                         "BeginClose");
        /// Gets a Runspace from the pool. If no free runspace is available
        /// and if max pool size is not reached, a new runspace is created.
        /// Otherwise this will block a runspace is released and available.
        /// An opened Runspace.
        /// Cannot perform operation because RunspacePool is
        /// not in the opened state.
        public Runspace GetRunspace()
            AssertPoolIsOpen();
            // Get the runspace asynchronously.
            GetRunspaceAsyncResult asyncResult = (GetRunspaceAsyncResult)BeginGetRunspace(null, null);
            // Wait for async operation to complete.
            // throw the exception that occurred while
            // processing the async operation
            if (asyncResult.Exception != null)
                throw asyncResult.Exception;
            return asyncResult.Runspace;
        /// Runspool is not in Opened state.
        public void ReleaseRunspace(Runspace runspace)
                throw PSTraceSource.NewArgumentNullException(nameof(runspace));
            bool isRunspaceReleased = false;
            bool destroyRunspace = false;
            // check if the runspace is owned by the pool
            lock (runspaceList)
                if (!runspaceList.Contains(runspace))
                    throw PSTraceSource.NewInvalidOperationException(RunspacePoolStrings.RunspaceNotBelongsToPool);
            // Release this runspace only if it is in valid state and is
            // owned by this pool.
                    if (pool.Count < maxPoolSz)
                        isRunspaceReleased = true;
                        pool.Push(runspace);
                        // this runspace is not going to be pooled as maxPoolSz is reduced.
                        // so release the runspace and destroy it.
                        destroyRunspace = true;
            if (destroyRunspace)
                // Destroying a runspace might be costly.
                // so doing this outside of the lock.
                DestroyRunspace(runspace);
            // it is important to release lock on Pool so that
            // other threads can service requests.
            if (isRunspaceReleased)
                // service any pending runspace requests.
        /// Dispose off the current runspace pool.
        /// true to release all the internal resources.
                    _cleanupTimer.Dispose();
                    _initialSessionState = null;
                    host = null;
        /// The value of this property is propagated to all the Runspaces in this pool;
        /// it determines whether a new thread is create when a pipeline is executed.
        internal PSThreadOptions ThreadOptions { get; set; } = PSThreadOptions.Default;
        /// The value of this property is propagated to all the Runspaces in this pool.
        internal ApartmentState ApartmentState { get; set; } = Runspace.DefaultApartmentState;
            GetRunspaceAsyncResult asyncResult = new GetRunspaceAsyncResult(this.InstanceId,
                callback, state);
            // Enqueue and start servicing thread in one go..saving multiple locks.
            EnqueueCheckAndStartRequestServicingThread(asyncResult, true);
            return asyncResult;
            GetRunspaceAsyncResult grsAsyncResult =
                asyncResult as GetRunspaceAsyncResult;
            if ((grsAsyncResult == null) || (grsAsyncResult.OwnerId != instanceId))
                                                         "BeginGetRunspace");
            grsAsyncResult.IsActive = false;
        /// TODO: Behavior if EndGetRunspace is called multiple times.
            grsAsyncResult.EndInvoke();
            return grsAsyncResult.Runspace;
        /// Opens the runspacepool synchronously / asynchronously.
        /// Runspace pool must be opened before it can be used.
        /// <param name="isAsync">
        /// true to open asynchronously
        /// <param name="asyncState">
        /// asyncResult object to monitor status of the async
        /// open operation. This is returned only if <paramref name="isAsync"/>
        /// is true.
        /// Cannot open RunspacePool because RunspacePool is not in
        /// the BeforeOpen state.
        /// <exception cref="OutOfMemoryException">
        /// There is not enough memory available to start this asynchronously.
        protected virtual IAsyncResult CoreOpen(bool isAsync, AsyncCallback callback,
            object asyncState)
                AssertIfStateIsBeforeOpen();
                stateInfo = new RunspacePoolStateInfo(RunspacePoolState.Opening, null);
            // only one thread will reach here, so no
            // need to lock.
            RaiseStateChangeEvent(stateInfo);
            if (isAsync)
                AsyncResult asyncResult = new RunspacePoolAsyncResult(instanceId, callback, asyncState, true);
                // Open pool in another thread
                ThreadPool.QueueUserWorkItem(new WaitCallback(OpenThreadProc), asyncResult);
            // open the runspace synchronously
            OpenHelper();
        /// Creates a Runspace + opens it synchronously and
        /// pushes it into the stack.
        /// Caller to make sure this is thread safe.
        protected void OpenHelper()
                PSEtwLog.SetActivityIdForCurrentThread(this.InstanceId);
                // Create a Runspace and store it in the pool
                // for future use. This will validate whether
                // a runspace can be created + opened successfully
                Runspace rs = CreateRunspace();
                pool.Push(rs);
                SetStateToBroken(exception);
                // rethrow the exception
            bool shouldRaiseEvents = false;
            // RunspacePool might be closed while we are still opening
            // we should not change state from closed to opened..
                if (stateInfo.State == RunspacePoolState.Opening)
                    // Change state to opened and notify the user.
                    stateInfo = new RunspacePoolStateInfo(RunspacePoolState.Opened, null);
                    shouldRaiseEvents = true;
            if (shouldRaiseEvents)
        private void SetStateToBroken(Exception reason)
                if ((stateInfo.State == RunspacePoolState.Opening) ||
                    (stateInfo.State == RunspacePoolState.Opened) ||
                    (stateInfo.State == RunspacePoolState.Disconnecting) ||
                    (stateInfo.State == RunspacePoolState.Disconnected) ||
                    (stateInfo.State == RunspacePoolState.Connecting))
                    stateInfo = new RunspacePoolStateInfo(RunspacePoolState.Broken, null);
                RunspacePoolStateInfo stateInfo = new RunspacePoolStateInfo(this.stateInfo.State,
                    reason);
        /// Starting point for asynchronous thread.
        /// asyncResult object
        protected void OpenThreadProc(object o)
            Dbg.Assert(o is AsyncResult, "OpenThreadProc expects AsyncResult");
            // Since this is an internal method, we can safely cast the
            // object to AsyncResult object.
            AsyncResult asyncObject = (AsyncResult)o;
                asyncObject.SetAsCompleted(exception);
        /// Closes the runspacepool synchronously / asynchronously.
        /// true to close asynchronously
        private IAsyncResult CoreClose(bool isAsync, AsyncCallback callback, object asyncState)
                if ((stateInfo.State == RunspacePoolState.Closed) ||
                    (stateInfo.State == RunspacePoolState.Broken) ||
                    (stateInfo.State == RunspacePoolState.Closing) ||
                    (stateInfo.State == RunspacePoolState.Disconnected))
                        RunspacePoolAsyncResult asyncResult = new RunspacePoolAsyncResult(instanceId, callback, asyncState, false);
                        asyncResult.SetAsCompleted(null);
                stateInfo = new RunspacePoolStateInfo(RunspacePoolState.Closing, null);
            // only one thread will reach here.
                ThreadPool.QueueUserWorkItem(new WaitCallback(CloseThreadProc), asyncResult);
            CloseHelper();
        private void CloseHelper()
                InternalClearAllResources();
                stateInfo = new RunspacePoolStateInfo(RunspacePoolState.Closed, null);
        private void CloseThreadProc(object o)
            Dbg.Assert(o is AsyncResult, "CloseThreadProc expects AsyncResult");
        /// Raise state changed event based on the StateInfo
        /// <param name="stateInfo">State information object.</param>
        protected void RaiseStateChangeEvent(RunspacePoolStateInfo stateInfo)
            StateChanged.SafeInvoke(this,
                new RunspacePoolStateChangedEventArgs(stateInfo));
        /// Checks if the Pool is open to honour requests.
        /// If not throws an exception.
                if (stateInfo.State != RunspacePoolState.Opened)
                    string message = StringUtil.Format(RunspacePoolStrings.InvalidRunspacePoolState, RunspacePoolState.Opened, stateInfo.State);
                    throw new InvalidRunspacePoolStateException(message,
                                     stateInfo.State, RunspacePoolState.Opened);
        /// Creates a new Runspace and initializes it by calling Open()
        /// TODO: Exceptions thrown here need to be documented.
        protected Runspace CreateRunspace()
            Dbg.Assert(_initialSessionState != null, "_initialSessionState should not be null");
            // TODO: exceptions thrown here need to be documented
            // runspace.Open() did not document all the exceptions.
            Runspace result = RunspaceFactory.CreateRunspaceFromSessionStateNoClone(host, _initialSessionState);
            result.ThreadOptions = this.ThreadOptions == PSThreadOptions.Default ? PSThreadOptions.ReuseThread : this.ThreadOptions;
            result.ApartmentState = this.ApartmentState;
            this.PropagateApplicationPrivateData(result);
            result.Open();
            // Enforce the system lockdown policy if one is defined.
            Utils.EnforceSystemLockDownLanguageMode(result.ExecutionContext);
            result.Events.ForwardEvent += OnRunspaceForwardEvent; // this must be done after open since open initializes the ExecutionContext
                runspaceList.Add(result);
                totalRunspaces = runspaceList.Count;
            // Start/Reset the cleanup timer to release idle runspaces in the pool.
                _cleanupTimer.Change(CleanupInterval, CleanupInterval);
            // raise the RunspaceCreated event and let callers handle it.
            RunspaceCreated.SafeInvoke(this, new RunspaceCreatedEventArgs(result));
        /// Cleans/Closes the runspace.
        /// Runspace to be closed/cleaned
        protected void DestroyRunspace(Runspace runspace)
            Dbg.Assert(runspace != null, "Runspace cannot be null");
            runspace.Events.ForwardEvent -= OnRunspaceForwardEvent; // this must be done after open since open initializes the ExecutionContext
            runspace.Close();
                runspaceList.Remove(runspace);
        /// Cleans the pool closing the runspaces that are idle.
        /// This method is called as part of a timer callback.
        /// This method will make sure at least minPoolSz number
        /// of Runspaces are active.
        protected void CleanupCallback(object state)
            Dbg.Assert((this.stateInfo.State != RunspacePoolState.Disconnected &&
                        this.stateInfo.State != RunspacePoolState.Disconnecting &&
                        this.stateInfo.State != RunspacePoolState.Connecting),
                       "Local RunspacePool cannot be in disconnect/connect states");
            bool isCleanupTimerChanged = false;
            // Clean up the pool only if more runspaces
            // than minimum requested are present.
            while (totalRunspaces > minPoolSz)
                // if the pool is closing just return..
                if (this.stateInfo.State == RunspacePoolState.Closing)
                // This is getting run on a threadpool thread
                // it is ok to take the hit on locking and unlocking.
                // this will release request threads depending
                // on thread scheduling
                Runspace runspaceToDestroy = null;
                    if (pool.Count == 0)
                        break; // break from while
                    runspaceToDestroy = pool.Pop();
                // Stop the clean up timer only when we are about to clean runspaces.
                // It will be restarted when a new runspace is created.
                if (!isCleanupTimerChanged)
                        _cleanupTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        isCleanupTimerChanged = true;
                // destroy runspace outside of the lock
                DestroyRunspace(runspaceToDestroy);
        /// Close all the runspaces in the pool.
        private void InternalClearAllResources()
            Exception invalidStateException = new InvalidRunspacePoolStateException(message,
            GetRunspaceAsyncResult runspaceRequester;
            // clear the request queue first..this way waiting threads
            // are immediately notified.
            lock (runspaceRequestQueue)
                while (runspaceRequestQueue.Count > 0)
                    runspaceRequester = runspaceRequestQueue.Dequeue();
                    runspaceRequester.SetAsCompleted(invalidStateException);
            lock (ultimateRequestQueue)
                while (ultimateRequestQueue.Count > 0)
                    runspaceRequester = ultimateRequestQueue.Dequeue();
            // close all the runspaces
            List<Runspace> runspaceListCopy = new List<Runspace>();
                runspaceListCopy.AddRange(runspaceList);
                runspaceList.Clear();
            // Start from the most recent runspace.
            for (int index = runspaceListCopy.Count - 1; index >= 0; index--)
                // close runspaces suppress exceptions
                    // this will release pipelines executing in the
                    // runspace.
                    runspaceListCopy[index].Close();
                    runspaceListCopy[index].Dispose();
                pool.Clear();
            // dont release pool/runspacelist/runspaceRequestQueue/ultimateRequestQueue as they
            // might be accessed in lock() statements from another thread.
        /// If <paramref name="requestToEnqueue"/> is not null, enqueues the request.
        /// Checks if a thread pool thread is queued to service pending requests
        /// for runspace. If a thread is not queued, queues one.
        /// <param name="requestToEnqueue">
        /// Used by calling threads to queue a request before checking and starting
        /// servicing thread.
        /// <param name="useCallingThread">
        /// uses calling thread to assign available runspaces (if any) to runspace
        /// requesters.
        protected void EnqueueCheckAndStartRequestServicingThread(GetRunspaceAsyncResult requestToEnqueue,
            bool useCallingThread)
            bool shouldStartServicingInSameThread = false;
                if (requestToEnqueue != null)
                    runspaceRequestQueue.Enqueue(requestToEnqueue);
                // if a thread is already servicing requests..just return.
                if (isServicingRequests)
                if ((runspaceRequestQueue.Count + ultimateRequestQueue.Count) > 0)
                    // we have requests pending..check if a runspace is available to
                    // service the requests.
                        if ((pool.Count > 0) || (totalRunspaces < maxPoolSz))
                            isServicingRequests = true;
                            if ((useCallingThread) && (ultimateRequestQueue.Count == 0))
                                shouldStartServicingInSameThread = true;
                                // release a async result object using a thread pool thread.
                                // this way the calling thread will not block.
                                ThreadPool.QueueUserWorkItem(new WaitCallback(ServicePendingRequests), false);
            // only one thread will be here if any..
            // This will allow us to release lock.
            if (shouldStartServicingInSameThread)
                ServicePendingRequests(true);
        /// Releases any readers in the reader queue waiting for
        /// <param name="useCallingThreadState">
        /// This is of type object..because this method is called from a ThreadPool
        /// Thread.
        /// true, if calling thread should be used to assign a runspace.
        protected void ServicePendingRequests(object useCallingThreadState)
            // Check if the pool is closed or closing..if so return.
            if ((stateInfo.State == RunspacePoolState.Closed) || (stateInfo.State == RunspacePoolState.Closing))
            bool useCallingThread = (bool)useCallingThreadState;
            GetRunspaceAsyncResult runspaceRequester = null;
                            Runspace result;
                                if (pool.Count > 0)
                                    result = pool.Pop();
                                else if (totalRunspaces >= maxPoolSz)
                                    // no runspace is available..
                                    // TODO: how to handle exceptions if runspace
                                    // creation fails.
                                    // Create a new runspace..since the max limit is
                                    // not reached.
                                    result = CreateRunspace();
                            // Dequeue a runspace request
                            // if the runspace is not active send the runspace back to
                            // the pool and process other requests
                            if (!runspaceRequester.IsActive)
                                    pool.Push(result);
                                // release the runspace requester
                                runspaceRequester.Release();
                            // release readers waiting for runspace on a thread pool
                            runspaceRequester.Runspace = result;
                            // release the async operation on a thread pool thread.
                            if (useCallingThread)
                                // call DoComplete outside of the lock..as the
                                // DoComplete handler may handle the runspace
                                // in the same thread thereby blocking future
                                // servicing requests.
                                goto endOuterWhile;
                                ThreadPool.QueueUserWorkItem(new WaitCallback(runspaceRequester.DoComplete));
                        if (runspaceRequestQueue.Count == 0)
                        // copy requests from one queue to another and start
                        // processing the other queue
                            ultimateRequestQueue.Enqueue(runspaceRequestQueue.Dequeue());
            endOuterWhile:;
                    isServicingRequests = false;
                    // check if any new runspace request has arrived..
            if ((useCallingThread) && (runspaceRequester != null))
                // call DoComplete outside of the lock and finally..as the
                // DoComplete handler may handle the runspace in the same
                // thread thereby blocking future servicing requests.
                runspaceRequester.DoComplete(null);
        /// Throws an exception if the runspace state is not
        /// BeforeOpen.
        protected void AssertIfStateIsBeforeOpen()
            if (stateInfo.State != RunspacePoolState.BeforeOpen)
                // Call fails if RunspacePoolState is not BeforeOpen.
                InvalidRunspacePoolStateException e =
                    new InvalidRunspacePoolStateException
                        StringUtil.Format(RunspacePoolStrings.CannotOpenAgain,
                            new object[] { stateInfo.State.ToString() }
                        stateInfo.State,
                        RunspacePoolState.BeforeOpen
            this.ForwardEvent?.Invoke(this, e);
        /// Forward runspace events to the pool's event queue.
        private void OnRunspaceForwardEvent(object sender, PSEventArgs e)
            if (e.ForwardEvent)
                OnForwardEvent(e);

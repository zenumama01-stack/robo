    /// Runspaces is base class for different kind of Runspaces.
    /// <remarks>There should be a class derived from it for each type of
    /// Runspace. Types of Runspace which we support are Local, X-AppDomain,
    /// X-Process and X-Machine.</remarks>
    internal abstract class RunspaceBase : Runspace
        /// Initialize powershell AssemblyLoadContext and register the 'Resolving' event, if it's not done already.
        /// If powershell is hosted by a native host such as DSC, then PS ALC may be initialized via 'SetPowerShellAssemblyLoadContext' before loading S.M.A.
        /// We do this both here and during the initialization of the 'ClrFacade' type.
        /// This is because we want to make sure the assembly/library resolvers are:
        ///  1. registered before any script/cmdlet can run.
        ///  2. registered before 'ClrFacade' gets used for assembly related operations.
        /// The 'ClrFacade' type may be used without a Runspace created, for example, by calling type conversion methods in the 'LanguagePrimitive' type.
        /// And at the mean time, script or cmdlet may run without the 'ClrFacade' type initialized.
        /// That's why we attempt to create the singleton of 'PowerShellAssemblyLoadContext' at both places.
        static RunspaceBase()
            if (PowerShellAssemblyLoadContext.Instance is null)
                PowerShellAssemblyLoadContext.InitializeSingleton(string.Empty, throwOnReentry: false);
        /// Construct an instance of an Runspace using a custom
        /// implementation of PSHost.
        /// <param name="host">The explicit PSHost implementation.</param>
        /// Host is null.
        /// host is null.
        protected RunspaceBase(PSHost host)
            InitialSessionState = InitialSessionState.CreateDefault();
            Host = host;
        /// configuration information for this runspace instance.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OK to call ThreadOptions")]
        protected RunspaceBase(PSHost host, InitialSessionState initialSessionState)
            InitialSessionState = initialSessionState.Clone();
            this.ThreadOptions = initialSessionState.ThreadOptions;
            this.ApartmentState = initialSessionState.ApartmentState;
        /// The explicit PSHost implementation
        /// <param name="suppressClone">
        /// If true, don't make a copy of the initial session state object.
        protected RunspaceBase(PSHost host, InitialSessionState initialSessionState, bool suppressClone)
            if (suppressClone)
        /// The host implemented PSHost interface.
        protected PSHost Host { get; }
        public override InitialSessionState InitialSessionState { get; }
        public override Version Version { get; } = PSVersionInfo.PSVersion;
        private RunspaceStateInfo _runspaceStateInfo = new RunspaceStateInfo(RunspaceState.BeforeOpen);
        public override RunspaceStateInfo RunspaceStateInfo
                lock (SyncRoot)
                    // Do not return internal state.
                    return _runspaceStateInfo.Clone();
        public override RunspaceAvailability RunspaceAvailability
            get { return _runspaceAvailability; }
            protected set { _runspaceAvailability = value; }
        private RunspaceAvailability _runspaceAvailability = RunspaceAvailability.None;
        /// Object used for synchronization.
        protected internal object SyncRoot { get; } = new object();
        /// Information about the computer where this runspace is created.
        public override RunspaceConnectionInfo ConnectionInfo
                // null refers to local case for path
        /// Original Connection Info that the user passed.
        public override RunspaceConnectionInfo OriginalConnectionInfo
        #region Open
        /// Open the runspace synchronously.
        public override void Open()
            CoreOpen(true);
        public override void OpenAsync()
            CoreOpen(false);
        /// Opens the runspace.
        /// <param name="syncCall">If true runspace is opened synchronously
        /// else runspaces is opened asynchronously
        private void CoreOpen(bool syncCall)
                RunspaceEventSource.Log.OpenRunspaceStart();
                // Call fails if RunspaceState is not BeforeOpen.
                if (RunspaceState != RunspaceState.BeforeOpen)
                    InvalidRunspaceStateException e =
                        new InvalidRunspaceStateException
                            StringUtil.Format(RunspaceStrings.CannotOpenAgain, new object[] { RunspaceState.ToString() }),
                            RunspaceState,
                            RunspaceState.BeforeOpen
                SetRunspaceState(RunspaceState.Opening);
            // Raise event outside the lock
            RaiseRunspaceStateEvents();
            OpenHelper(syncCall);
                RunspaceEventSource.Log.OpenRunspaceStop();
            // We report startup telemetry when opening the runspace - because this is the first time
            // we are really using PowerShell. This isn't the cleanest place though, because
            // sometimes there are many runspaces created - the callee ensures telemetry is only
            // reported once. Note that if the host implements IHostProvidesTelemetryData, we rely
            // on the host calling ReportStartupTelemetry.
            if (this.Host is not IHostProvidesTelemetryData)
                TelemetryAPI.ReportStartupTelemetry(null);
        /// Derived class's open implementation.
        protected abstract void OpenHelper(bool syncCall);
        #endregion open
        #region close
            CoreClose(true);
        public override void CloseAsync()
            CoreClose(false);
        /// Close the runspace.
        /// <param name="syncCall">If true runspace is closed synchronously
        /// else runspaces is closed asynchronously
        /// If SessionStateProxy has some method call in progress
        private void CoreClose(bool syncCall)
            bool alreadyClosing = false;
                if (RunspaceState == RunspaceState.Closed ||
                    RunspaceState == RunspaceState.Broken)
                else if (RunspaceState == RunspaceState.BeforeOpen)
                    SetRunspaceState(RunspaceState.Closing, null);
                    SetRunspaceState(RunspaceState.Closed, null);
                else if (RunspaceState == RunspaceState.Opening)
                    // Wait till the runspace is opened - This is set in DoOpenHelper()
                    // Release the lock before we wait
                    Monitor.Exit(SyncRoot);
                        RunspaceOpening.Wait();
                        // Acquire the lock before we carry on with the rest operations
                        Monitor.Enter(SyncRoot);
                if (_bSessionStateProxyCallInProgress)
                    throw PSTraceSource.NewInvalidOperationException(RunspaceStrings.RunspaceCloseInvalidWhileSessionStateProxy);
                if (RunspaceState == RunspaceState.Closing)
                    alreadyClosing = true;
                    if (RunspaceState != RunspaceState.Opened)
                                StringUtil.Format(RunspaceStrings.RunspaceNotInOpenedState, RunspaceState.ToString()),
                                RunspaceState.Opened
                    SetRunspaceState(RunspaceState.Closing);
            if (alreadyClosing)
                // Already closing is set to true if Runspace is already
                // in closing. In this case wait for runspace to close.
                // This can happen in two scenarios:
                // 1) User calls Runspace.Close from two threads.
                // 2) In remoting, some error from data structure handler layer can start
                // runspace closure. At the same time, user can call
                // remove runspace.
                if (syncCall)
                    WaitForFinishofPipelines();
            // Raise Event outside the lock
            // Call the derived class implementation to do the actual work
            CloseHelper(syncCall);
        /// Derived class's close implementation.
        protected abstract void CloseHelper(bool syncCall);
        #endregion close
        #region Disconnect-Connect
        public override void Disconnect()
            // Disconnect operation is not supported on local runspaces.
            throw new InvalidRunspaceStateException(
                            RunspaceStrings.DisconnectNotSupported);
        public override void DisconnectAsync()
        /// Connects a runspace to its remote counterpart synchronously.
        public override void Connect()
            // Connect operation is not supported on local runspaces.
                            RunspaceStrings.ConnectNotSupported);
        public override void ConnectAsync()
        /// Creates a pipeline object in the Disconnected state.
        /// <returns>Pipeline.</returns>
        public override Pipeline CreateDisconnectedPipeline()
            // Disconnect-Connect is not supported on local runspaces.
                            RunspaceStrings.DisconnectConnectNotSupported);
        /// Creates a powershell object in the Disconnected state.
        public override PowerShell CreateDisconnectedPowerShell()
        public override RunspaceCapability GetCapabilities()
            return RunspaceCapability.Default;
        #region CreatePipeline
        public override Pipeline CreatePipeline()
            return CoreCreatePipeline(null, false, false);
        /// A pipeline pre-filled with Commands specified in commandString.
        public override Pipeline CreatePipeline(string command)
            return CoreCreatePipeline(command, false, false);
        public override Pipeline CreatePipeline(string command, bool addToHistory)
            return CoreCreatePipeline(command, addToHistory, false);
        public override Pipeline CreateNestedPipeline()
            return CoreCreatePipeline(null, false, true);
        public override Pipeline CreateNestedPipeline(string command, bool addToHistory)
            return CoreCreatePipeline(command, addToHistory, true);
        /// <param name="command">A valid command string or string.Empty.</param>
        /// <param name="isNested">True for nested pipeline.</param>
        protected abstract Pipeline CoreCreatePipeline(string command, bool addToHistory, bool isNested);
        #endregion CreatePipeline
        #region state change event
        public override event EventHandler<RunspaceStateEventArgs> StateChanged;
        public override event EventHandler<RunspaceAvailabilityEventArgs> AvailabilityChanged;
        internal override bool HasAvailabilityChangedSubscribers
            get { return this.AvailabilityChanged != null; }
        protected override void OnAvailabilityChanged(RunspaceAvailabilityEventArgs e)
            EventHandler<RunspaceAvailabilityEventArgs> eh = this.AvailabilityChanged;
                    eh(this, e);
        /// Retrieve the current state of the runspace.
        /// <see cref="RunspaceState"/>
        protected RunspaceState RunspaceState
                return _runspaceStateInfo.State;
        /// This is queue of all the state change event which have occurred for
        /// this runspace. RaiseRunspaceStateEvents raises event for each
        /// item in this queue. We don't raise events from with SetRunspaceState
        /// because SetRunspaceState is often called from with in the a lock.
        /// Raising event with in a lock introduces chances of deadlock in GUI
        /// applications.
        private Queue<RunspaceEventQueueItem> _runspaceEventQueue = new Queue<RunspaceEventQueueItem>();
        private sealed class RunspaceEventQueueItem
            public RunspaceEventQueueItem(RunspaceStateInfo runspaceStateInfo, RunspaceAvailability currentAvailability, RunspaceAvailability newAvailability)
                this.RunspaceStateInfo = runspaceStateInfo;
                this.CurrentRunspaceAvailability = currentAvailability;
                this.NewRunspaceAvailability = newAvailability;
            public RunspaceStateInfo RunspaceStateInfo;
            public RunspaceAvailability CurrentRunspaceAvailability;
            public RunspaceAvailability NewRunspaceAvailability;
        // This is to notify once runspace has been opened (RunspaceState.Opened)
        internal ManualResetEventSlim RunspaceOpening = new ManualResetEventSlim(false);
        /// Set the new runspace state.
        /// <param name="state">The new state.</param>
        /// <param name="reason">An exception indicating the state change is the
        /// result of an error, otherwise; null.
        /// Sets the internal runspace state information member variable. It also
        /// adds RunspaceStateInfo to a queue.
        /// RaiseRunspaceStateEvents raises event for each item in this queue.
        protected void SetRunspaceState(RunspaceState state, Exception reason)
                if (state != RunspaceState)
                    _runspaceStateInfo = new RunspaceStateInfo(state, reason);
                    // Add _runspaceStateInfo to _runspaceEventQueue.
                    // RaiseRunspaceStateEvents will raise event for each item
                    // in this queue.
                    // Note:We are doing clone here instead of passing the member
                    // _runspaceStateInfo because we donot want outside
                    // to change our runspace state.
                    RunspaceAvailability previousAvailability = _runspaceAvailability;
                    this.UpdateRunspaceAvailability(_runspaceStateInfo.State, false);
                    _runspaceEventQueue.Enqueue(
                        new RunspaceEventQueueItem(
                            _runspaceStateInfo.Clone(),
                            previousAvailability,
                            _runspaceAvailability));
        /// Set the current runspace state - no error.
        protected void SetRunspaceState(RunspaceState state)
            this.SetRunspaceState(state, null);
        /// Raises events for changes in runspace state.
        protected void RaiseRunspaceStateEvents()
            Queue<RunspaceEventQueueItem> tempEventQueue = null;
            EventHandler<RunspaceStateEventArgs> stateChanged = null;
            bool hasAvailabilityChangedSubscribers = false;
                stateChanged = this.StateChanged;
                hasAvailabilityChangedSubscribers = this.HasAvailabilityChangedSubscribers;
                if (stateChanged != null || hasAvailabilityChangedSubscribers)
                    tempEventQueue = _runspaceEventQueue;
                    _runspaceEventQueue = new Queue<RunspaceEventQueueItem>();
                    // Clear the events if there are no EventHandlers. This
                    // ensures that events do not get called for state
                    // changes prior to their registration.
                    _runspaceEventQueue.Clear();
            if (tempEventQueue != null)
                while (tempEventQueue.Count > 0)
                    RunspaceEventQueueItem queueItem = tempEventQueue.Dequeue();
                    if (hasAvailabilityChangedSubscribers && queueItem.NewRunspaceAvailability != queueItem.CurrentRunspaceAvailability)
                        this.OnAvailabilityChanged(new RunspaceAvailabilityEventArgs(queueItem.NewRunspaceAvailability));
                    // Exception raised by events are not error condition for runspace
                    if (stateChanged != null)
                            stateChanged(this, new RunspaceStateEventArgs(queueItem.RunspaceStateInfo));
        #endregion state change event
        #region running pipeline management
        /// In RemoteRunspace, it is required to invoke pipeline
        /// as part of open call (i.e. while state is Opening).
        /// If this property is true, runspace state check is
        /// not performed in AddToRunningPipelineList call.
        protected bool ByPassRunspaceStateCheck { get; set; }
        private readonly object _pipelineListLock = new object();
        /// List of pipeline which are currently executing in this runspace.
        protected List<Pipeline> RunningPipelines { get; } = new List<Pipeline>();
        /// Add the pipeline to list of pipelines in execution.
        /// <param name="pipeline">Pipeline to add to the
        /// list of pipelines in execution</param>
        /// Thrown if the runspace is not in the Opened state.
        /// <see cref="RunspaceState"/>.
        /// <exception cref="ArgumentNullException">Thrown if
        /// <paramref name="pipeline"/> is null.
        internal void AddToRunningPipelineList(PipelineBase pipeline)
            Dbg.Assert(pipeline != null, "caller should validate the parameter");
            lock (_pipelineListLock)
                if (!ByPassRunspaceStateCheck && RunspaceState != RunspaceState.Opened)
                            StringUtil.Format(RunspaceStrings.RunspaceNotOpenForPipeline, RunspaceState.ToString()),
                // Add the pipeline to list of Executing pipeline.
                // Note:_runningPipelines is always accessed with the lock so
                // there is no need to create a synchronized version of list
                RunningPipelines.Add(pipeline);
                _currentlyRunningPipeline = pipeline;
        /// Remove the pipeline from list of pipelines in execution.
        /// <param name="pipeline">Pipeline to remove from the
        /// Thrown if <paramref name="pipeline"/> is null.
        internal void RemoveFromRunningPipelineList(PipelineBase pipeline)
                Dbg.Assert(RunspaceState != RunspaceState.BeforeOpen,
                             "Runspace should not be before open when pipeline is running");
                // Remove the pipeline to list of Executing pipeline.
                RunningPipelines.Remove(pipeline);
                // Update the running pipeline
                if (RunningPipelines.Count == 0)
                    _currentlyRunningPipeline = null;
                    _currentlyRunningPipeline = RunningPipelines[RunningPipelines.Count - 1];
                pipeline.PipelineFinishedEvent.Set();
        /// Waits till all the pipelines running in the runspace have
        /// finished execution.
        internal bool WaitForFinishofPipelines()
            // Take a snapshot of list of active pipelines.
            // Note:Before we enter to this CloseHelper routine
            // CoreClose has already set the state of Runspace
            // to closing. So no new pipelines can be executed on this
            // runspace and so no new pipelines will be added to
            // _runningPipelines. However we still need to lock because
            // running pipelines can be removed from this.
            PipelineBase[] runningPipelines;
                runningPipelines = RunningPipelines.Cast<PipelineBase>().ToArray();
            if (runningPipelines.Length > 0)
                WaitHandle[] waitHandles = new WaitHandle[runningPipelines.Length];
                for (int i = 0; i < runningPipelines.Length; i++)
                    waitHandles[i] = runningPipelines[i].PipelineFinishedEvent;
                // WaitAll for multiple handles on a STA (single-thread apartment) thread is not supported as WaitAll will prevent the message pump to run
                if (runningPipelines.Length > 1 && Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
                    // We use a worker thread to wait for all handles, and the STA thread can just wait on the worker thread -- the worker
                    // threads from the ThreadPool are MTA.
                    using (ManualResetEvent waitAllIsDone = new ManualResetEvent(false))
                        Tuple<WaitHandle[], ManualResetEvent> stateInfo = new Tuple<WaitHandle[], ManualResetEvent>(waitHandles, waitAllIsDone);
                            (object state) =>
                                var tuple = (Tuple<WaitHandle[], ManualResetEvent>)state;
                                WaitHandle.WaitAll(tuple.Item1);
                                tuple.Item2.Set();
                            stateInfo);
                        return waitAllIsDone.WaitOne();
                return WaitHandle.WaitAll(waitHandles);
        /// Stops all the running pipelines.
        protected void StopPipelines()
                // Start from the most recent pipeline.
                for (int i = runningPipelines.Length - 1; i >= 0; i--)
                    runningPipelines[i].Stop();
        internal bool CanRunActionInCurrentPipeline()
                // If we have no running pipeline, or if the currently running pipeline is
                // the same as the current thread, then execute the action.
                var pipelineRunning = _currentlyRunningPipeline as PipelineBase;
                return pipelineRunning == null ||
                    Thread.CurrentThread == pipelineRunning.NestedPipelineExecutionThread;
        /// Gets the currently executing pipeline.
        /// <remarks>Internal because it is needed by invoke-history</remarks>
        internal override Pipeline GetCurrentlyRunningPipeline()
            return _currentlyRunningPipeline;
        private Pipeline _currentlyRunningPipeline = null;
        /// This method stops all the pipelines which are nested
        /// under specified pipeline.
        /// <param name="pipeline"></param>
        internal void StopNestedPipelines(Pipeline pipeline)
            List<Pipeline> nestedPipelines = null;
                // first check if this pipeline is in the list of running
                // pipelines. It is possible that pipeline has already
                // completed.
                if (!RunningPipelines.Contains(pipeline))
                // If this pipeline is currently running pipeline,
                // then it does not have nested pipelines
                if (GetCurrentlyRunningPipeline() == pipeline)
                // Build list of nested pipelines
                nestedPipelines = new List<Pipeline>();
                for (int i = RunningPipelines.Count - 1; i >= 0; i--)
                    if (RunningPipelines[i] == pipeline)
                    nestedPipelines.Add(RunningPipelines[i]);
            foreach (Pipeline np in nestedPipelines)
                    np.Stop();
                catch (InvalidPipelineStateException)
        DoConcurrentCheckAndAddToRunningPipelines(PipelineBase pipeline, bool syncCall)
            // Concurrency check should be done under runspace lock
                    throw PSTraceSource.NewInvalidOperationException(RunspaceStrings.NoPipelineWhenSessionStateProxyInProgress);
                // Delegate to pipeline to do check if it is fine to invoke if another
                // pipeline is running.
                pipeline.DoConcurrentCheck(syncCall, SyncRoot, true);
                // Finally add to the list of running pipelines.
                AddToRunningPipelineList(pipeline);
        internal void Pulse()
            // If we don't already have a pipeline running, pulse the engine.
            bool pipelineCreated = false;
            if (GetCurrentlyRunningPipeline() == null)
                        // This is a pipeline that does the least amount possible.
                        // It evaluates a constant, and results in the execution of only two parse tree nodes.
                        // We don't need to void it, as we aren't using the results. In addition, voiding
                        // (as opposed to ignoring) is 1.6x slower.
                            PulsePipeline = (PipelineBase)CreatePipeline("0");
                            PulsePipeline.IsPulsePipeline = true;
                            pipelineCreated = true;
                            // Ignore. The runspace is closing. The event was not processed,
                            // but this should not crash PowerShell.
            // Invoke pipeline outside the runspace lock.
            // A concurrency check will be made on the runspace before this
            // pipeline is invoked.
            if (pipelineCreated)
                    PulsePipeline.Invoke();
                    // Ignore. A pipeline was created between the time
                    // we checked for it, and when we invoked the pipeline.
                    // This is unlikely, but taking a lock on the runspace
                    // means that OUR invoke will not be able to run.
        internal PipelineBase PulsePipeline { get; private set; }
        #endregion running pipeline management
        #region session state proxy
        // Note: When SessionStateProxy calls are in progress,
        // pipeline cannot be invoked. Also when pipeline is in
        // progress, SessionStateProxy calls cannot be made.
        private bool _bSessionStateProxyCallInProgress;
        /// This method ensures that SessionStateProxy call is allowed and if
        /// allowed it sets a variable to disallow further SessionStateProxy or
        /// pipeline calls.
        private void DoConcurrentCheckAndMarkSessionStateProxyCallInProgress()
                    throw PSTraceSource.NewInvalidOperationException(RunspaceStrings.AnotherSessionStateProxyInProgress);
                Pipeline runningPipeline = GetCurrentlyRunningPipeline();
                if (runningPipeline != null)
                    // Detect if we're running an engine pulse, or we're running a nested pipeline
                    // from an engine pulse
                    if (runningPipeline == PulsePipeline ||
                        (runningPipeline.IsNested && PulsePipeline != null))
                        // If so, wait and try again
                        // Release the lock before we wait for the pulse pipelines
                        DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
                        throw PSTraceSource.NewInvalidOperationException(RunspaceStrings.NoSessionStateProxyWhenPipelineInProgress);
                // Now we can invoke session state proxy
                _bSessionStateProxyCallInProgress = true;
        /// SetVariable implementation. This class does the necessary checks to ensure
        /// that no pipeline or other SessionStateProxy calls are in progress.
        /// It delegates to derived class worker method for actual operation.
        internal void SetVariable(string name, object value)
                DoSetVariable(name, value);
                    _bSessionStateProxyCallInProgress = false;
        /// GetVariable implementation. This class does the necessary checks to ensure
        internal object GetVariable(string name)
                return DoGetVariable(name);
        /// Applications implementation. This class does the necessary checks to ensure
        internal List<string> Applications
                    return DoApplications;
        /// Scripts implementation. This class does the necessary checks to ensure
        internal List<string> Scripts
                    return DoScripts;
        /// Protected methods to be implemented by derived class.
        /// This does the actual work of getting scripts.
        internal DriveManagementIntrinsics Drive
                    return DoDrive;
                return DoLanguageMode;
                DoLanguageMode = value;
        internal PSModuleInfo Module
                    return DoModule;
        internal PathIntrinsics PathIntrinsics
                    return DoPath;
        internal CmdletProviderManagementIntrinsics Provider
                    return DoProvider;
        internal PSVariableIntrinsics PSVariable
                    return DoPSVariable;
        internal CommandInvocationIntrinsics InvokeCommand
                    return DoInvokeCommand;
                    return DoInvokeProvider;
        /// This does the actual work of setting variable.
        /// <param name="name">Name of the variable to set.</param>
        /// <param name="value">The value to set it to.</param>
        protected abstract void DoSetVariable(string name, object value);
        /// This does the actual work of getting variable.
        protected abstract object DoGetVariable(string name);
        /// This does the actual work of getting applications.
        protected abstract List<string> DoApplications { get; }
        protected abstract List<string> DoScripts { get; }
        protected abstract DriveManagementIntrinsics DoDrive { get; }
        protected abstract PSLanguageMode DoLanguageMode { get; set; }
        protected abstract PSModuleInfo DoModule { get; }
        protected abstract PathIntrinsics DoPath { get; }
        protected abstract CmdletProviderManagementIntrinsics DoProvider { get; }
        protected abstract PSVariableIntrinsics DoPSVariable { get; }
        protected abstract CommandInvocationIntrinsics DoInvokeCommand { get; }
        protected abstract ProviderIntrinsics DoInvokeProvider { get; }
        private SessionStateProxy _sessionStateProxy;
        /// Returns SessionState proxy object.
        internal override SessionStateProxy GetSessionStateProxy()
            return _sessionStateProxy ??= new SessionStateProxy(this);
        #endregion session state proxy

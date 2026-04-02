    /// This class has common base implementation for Pipeline class.
    /// LocalPipeline and RemotePipeline classes derives from it.
    internal abstract class PipelineBase : Pipeline
        /// Create a pipeline initialized with a command string.
        /// <param name="runspace">The associated Runspace/></param>
        /// Command is null and add to history is true
        protected PipelineBase(Runspace runspace, string command, bool addToHistory, bool isNested)
            : base(runspace)
            Initialize(runspace, command, addToHistory, isNested);
            // Initialize streams
            InputStream = new ObjectStream();
            OutputStream = new ObjectStream();
            ErrorStream = new ObjectStream();
        /// The command to invoke.
        /// If true, add the command to history.
        /// 1. InformationalBuffers is null
        protected PipelineBase(Runspace runspace,
            : base(runspace, command)
            Dbg.Assert(inputStream != null, "Caller Should validate inputstream parameter");
            Dbg.Assert(outputStream != null, "Caller Should validate outputStream parameter");
            Dbg.Assert(errorStream != null, "Caller Should validate errorStream parameter");
            Dbg.Assert(infoBuffers != null, "Caller Should validate informationalBuffers parameter");
            // Since we are constructing this pipeline using a commandcollection we dont need
            // to add cmd to CommandCollection again (Initialize does this).. because of this
            // I am handling history here..
            Initialize(runspace, null, false, isNested);
                // get command text for history..
                string cmdText = command.GetCommandStringForHistory();
                HistoryString = cmdText;
                AddToHistory = addToHistory;
            InputStream = inputStream;
            OutputStream = outputStream;
            ErrorStream = errorStream;
            InformationalBuffers = infoBuffers;
        /// The copy constructor's intent is to support the scenario
        /// where a host needs to run the same set of commands multiple
        /// times.  This is accomplished via creating a master pipeline
        /// then cloning it and executing the cloned copy.
        protected PipelineBase(PipelineBase pipeline)
            : this(pipeline.Runspace, null, false, pipeline.IsNested)
            if (pipeline == null)
                throw PSTraceSource.NewArgumentNullException(nameof(pipeline));
            if (pipeline._disposed)
            AddToHistory = pipeline.AddToHistory;
            HistoryString = pipeline.HistoryString;
            foreach (Command command in pipeline.Commands)
                // Attach the cloned Command to this pipeline.
                Commands.Add(clone);
        /// Access the runspace this pipeline is created on.
        public override Runspace Runspace
        /// This internal method doesn't do the _disposed check.
        internal Runspace GetRunspace()
        private bool _isNested;
        /// Is this pipeline nested.
        public override bool IsNested
                return _isNested;
        /// Is this a pulse pipeline (created by the EventManager)
        internal bool IsPulsePipeline { get; set; }
        private PipelineStateInfo _pipelineStateInfo = new PipelineStateInfo(PipelineState.NotStarted);
        /// Info about current state of the pipeline.
        public override PipelineStateInfo PipelineStateInfo
                    // Note:We do not return internal state.
                    return _pipelineStateInfo.Clone();
        // 913921-2005/07/08 ObjectWriter can be retrieved on a closed stream
        /// Access the input writer for this pipeline.
        public override PipelineWriter Input
                return InputStream.ObjectWriter;
        /// Access the output reader for this pipeline.
        public override PipelineReader<PSObject> Output
                return OutputStream.PSObjectReader;
        /// Access the error output reader for this pipeline.
        public override PipelineReader<object> Error
                return _errorStream.ObjectReader;
        /// Is this pipeline a child pipeline?
        internal override bool IsChild { get; set; }
        public override void Stop()
            CoreStop(true);
        public override void StopAsync()
            CoreStop(false);
        private void CoreStop(bool syncCall)
            // Is pipeline already in stopping state.
            bool alreadyStopping = false;
                switch (PipelineState)
                    case PipelineState.NotStarted:
                        SetPipelineState(PipelineState.Stopping);
                        SetPipelineState(PipelineState.Stopped);
                    // If pipeline execution has failed or completed or
                    // stopped, return silently.
                    // If pipeline is in Stopping state, ignore the second
                    // stop.
                        alreadyStopping = true;
            // If pipeline is already in stopping state. Wait for pipeline
            // to finish. We do need to raise any events here as no
            // change of state has occurred.
            if (alreadyStopping)
            // Raise the event outside the lock
            // A pipeline can be stopped before it is started. See NotStarted
            // case in above switch statement. This is done to allow stoping a pipeline
            // in another thread before it has been started.
                if (PipelineState == PipelineState.Stopped)
                    // Note:if we have reached here, Stopped state was set
                    // in PipelineState.NotStarted case above. Only other
                    // way Stopped can be set when this method calls
                    // StopHelper below
            // Start stop operation in derived class
            ImplementStop(syncCall);
        /// Stop execution of pipeline.
        /// <param name="syncCall">If false, call is asynchronous.</param>
        protected abstract void ImplementStop(bool syncCall);
        /// Invoke the pipeline, synchronously, returning the results as an
        /// array of objects.
        /// <remarks>Caller of synchronous exectute should not close
        /// On Synchronous Invoke if output is throttled and no one is reading from
        /// output pipe, Execution will block after buffer is full.
        public override Collection<PSObject> Invoke(IEnumerable input)
            CoreInvoke(input, true);
            // Wait for pipeline to finish execution
            if (SyncInvokeCall)
                // Raise the pipeline completion events. These events are set in
                // pipeline execution thread. However for Synchronous execution
                // we raise the event in the main thread.
            if (PipelineStateInfo.State == PipelineState.Stopped)
            else if (PipelineStateInfo.State == PipelineState.Failed && PipelineStateInfo.Reason != null)
                // If this is an error pipe for a hosting applicationand we are logging,
                // then log the error.
                    this.Runspace.ExecutionContext.InternalHost.UI.TranscribeResult(this.Runspace, PipelineStateInfo.Reason.Message);
                throw PipelineStateInfo.Reason;
            // Execution completed successfully
            return Output.NonBlockingRead(Int32.MaxValue);
        public override void InvokeAsync()
            CoreInvoke(null, false);
        /// This parameter is true if Invoke is called.
        /// It is false if InvokeAsync is called.
        protected bool SyncInvokeCall { get; private set; }
        /// <param name="input">input to provide to pipeline. Input is
        /// used only for synchronous execution</param>
        /// <param name="syncCall">True if this method is called from
        /// synchronous invoke else false</param>
        /// 3) Attempt is made to invoke a nested pipeline directly. Nested
        private void CoreInvoke(IEnumerable input, bool syncCall)
                if (Commands == null || Commands.Count == 0)
                            RunspaceStrings.NoCommandInPipeline);
                if (PipelineState != PipelineState.NotStarted)
                    InvalidPipelineStateException e =
                        new InvalidPipelineStateException
                            StringUtil.Format(RunspaceStrings.PipelineReInvokeNotAllowed),
                            PipelineState,
                            PipelineState.NotStarted
                if (syncCall
                    && InputStream is not PSDataCollectionStream<PSObject>
                    && InputStream is not PSDataCollectionStream<object>)
                    // Method is called from synchronous invoke.
                        // TO-DO-Add a test make sure that ObjectDisposed
                        // exception is thrown
                        // Write input data in to inputStream and close the input
                        // pipe. If Input stream is already closed an
                        // ObjectDisposed exception will be thrown
                        foreach (object temp in input)
                            InputStream.Write(temp);
                SyncInvokeCall = syncCall;
                // Create event which will be signalled when pipeline execution
                // is completed/failed/stopped.
                // Note:Runspace.Close waits for all the running pipeline
                // to finish.  This Event must be created before pipeline is
                // added to list of running pipelines. This avoids the race condition
                // where Close is called after pipeline is added to list of
                // running pipeline but before event is created.
                PipelineFinishedEvent = new ManualResetEvent(false);
                // 1) Do the check to ensure that pipeline no other
                // 2) Runspace object maintains a list of pipelines in
                // execution. Add this pipeline to the list.
                RunspaceBase.DoConcurrentCheckAndAddToRunningPipelines(this, syncCall);
                // Note: Set PipelineState to Running only after adding pipeline to list
                // of pipelines in execution. AddForExecution checks that runspace is in
                // state where pipeline can be run.
                // StartPipelineExecution raises this event. See Windows Bug 1160481 for
                // more details.
                SetPipelineState(PipelineState.Running);
                // Let the derived class start the pipeline execution.
                StartPipelineExecution();
                // If we fail in any of the above three steps, set the correct states.
                RunspaceBase.RemoveFromRunningPipelineList(this);
                SetPipelineState(PipelineState.Failed, exception);
                // Note: we are not raising the events in this case. However this is
                // fine as user is getting the exception.
        internal override void InvokeAsyncAndDisconnect()
        /// Starts execution of pipeline.
        protected abstract void StartPipelineExecution();
        #region concurrent pipeline check
        private bool _performNestedCheck = true;
        /// For nested pipeline, system checks that Execute is called from
        /// currently executing pipeline.
        /// If PerformNestedCheck is false, this check is bypassed. This
        /// is set to true by remote provider. In remote provider case all
        /// the checks are done by the client proxy.
        internal bool PerformNestedCheck
                _performNestedCheck = value;
        /// This is the thread on which NestedPipeline can be executed.
        /// In case of LocalPipeline, this is the thread of execution
        /// of LocalPipeline. In case of RemotePipeline, this is thread
        /// on which EnterNestedPrompt is called.
        /// RemotePipeline proxy should set it on at the beginning of
        /// EnterNestedPrompt and clear it on return.
        internal Thread NestedPipelineExecutionThread { get; set; }
        /// Check if anyother pipeline is executing.
        /// In case of nested pipeline, checks that it is called
        /// from currently executing pipeline's thread.
        /// <param name="syncCall">True if method is called from Invoke, false
        /// if called from InvokeAsync</param>
        /// <param name="syncObject">The sync object on which the lock is acquired.</param>
        /// <param name="isInLock">True if the method is invoked in a critical section.</param>
        internal void DoConcurrentCheck(bool syncCall, object syncObject, bool isInLock)
            PipelineBase currentPipeline = (PipelineBase)RunspaceBase.GetCurrentlyRunningPipeline();
                    // Detect if we're running a pulse pipeline, or we're running a nested pipeline
                    // in a pulse pipeline
                    if (currentPipeline == RunspaceBase.PulsePipeline ||
                        (currentPipeline.IsNested && RunspaceBase.PulsePipeline != null))
                        if (isInLock)
                            // If the method is invoked in the lock statement, release the
                            // lock before wait on the pulse pipeline
                            Monitor.Exit(syncObject);
                            RunspaceBase.WaitForFinishofPipelines();
                                // If the method is invoked in the lock statement, acquire the
                                // lock before we carry on with the rest operations
                                Monitor.Enter(syncObject);
                        DoConcurrentCheck(syncCall, syncObject, isInLock);
                if (_performNestedCheck)
                    if (!syncCall)
                                RunspaceStrings.NestedPipelineInvokeAsync);
                        if (this.IsChild)
                            // OK it's not really a nested pipeline but a call with RunspaceMode=UseCurrentRunspace
                            // This shouldn't fail so we'll clear the IsNested and IsChild flags and then return
                            // That way executions proceeds but everything gets clean up at the end when the pipeline completes
                            this.IsChild = false;
                            _isNested = false;
                                RunspaceStrings.NestedPipelineNoParentPipeline);
                    Dbg.Assert(currentPipeline.NestedPipelineExecutionThread != null, "Current pipeline should always have NestedPipelineExecutionThread set");
                    Thread th = Thread.CurrentThread;
                    if (!currentPipeline.NestedPipelineExecutionThread.Equals(th))
        #endregion concurrent pipeline check
        #region Connect
        public override Collection<PSObject> Connect()
            // Connect semantics not supported on local (non-remoting) pipelines.
            throw PSTraceSource.NewNotSupportedException(PipelineStrings.ConnectNotSupported);
        public override event EventHandler<PipelineStateEventArgs> StateChanged = null;
        /// Current state of the pipeline.
        protected PipelineState PipelineState
                return _pipelineStateInfo.State;
        /// This returns true if pipeline state is Completed, Failed or Stopped.
        protected bool IsPipelineFinished()
            return (PipelineState == PipelineState.Completed ||
                    PipelineState == PipelineState.Failed ||
                    PipelineState == PipelineState.Stopped);
        /// this pipeline. RaisePipelineStateEvents raises event for each
        /// item in this queue. We don't raise the event with in SetPipelineState
        /// because often SetPipelineState is called with in a lock.
        /// Raising event in lock introduces chances of deadlock in GUI applications.
        private Queue<ExecutionEventQueueItem> _executionEventQueue = new Queue<ExecutionEventQueueItem>();
        private sealed class ExecutionEventQueueItem
            public ExecutionEventQueueItem(PipelineStateInfo pipelineStateInfo, RunspaceAvailability currentAvailability, RunspaceAvailability newAvailability)
                this.PipelineStateInfo = pipelineStateInfo;
            public PipelineStateInfo PipelineStateInfo;
        /// Sets the new execution state.
        /// <param name="reason">
        /// An exception indicating that state change is the result of an error,
        /// otherwise; null.
        /// Sets the internal execution state information member variable. It
        /// also adds PipelineStateInfo to a queue. RaisePipelineStateEvents
        /// raises event for each item in this queue.
        protected void SetPipelineState(PipelineState state, Exception reason)
                if (state != PipelineState)
                    _pipelineStateInfo = new PipelineStateInfo(state, reason);
                    // Add _pipelineStateInfo to _executionEventQueue.
                    // RaisePipelineStateEvents will raise event for each item
                    // _pipelineStateInfo because we donot want outside
                    // to change pipeline state.
                    RunspaceAvailability previousAvailability = _runspace.RunspaceAvailability;
                    _runspace.UpdateRunspaceAvailability(_pipelineStateInfo.State, false);
                    _executionEventQueue.Enqueue(
                        new ExecutionEventQueueItem(
                            _pipelineStateInfo.Clone(),
                            _runspace.RunspaceAvailability));
        /// Set the new execution state.
        protected void SetPipelineState(PipelineState state)
            SetPipelineState(state, null);
        /// Raises events for changes in execution state.
        protected void RaisePipelineStateEvents()
            Queue<ExecutionEventQueueItem> tempEventQueue = null;
            EventHandler<PipelineStateEventArgs> stateChanged = null;
            bool runspaceHasAvailabilityChangedSubscribers = false;
                runspaceHasAvailabilityChangedSubscribers = _runspace.HasAvailabilityChangedSubscribers;
                if (stateChanged != null || runspaceHasAvailabilityChangedSubscribers)
                    tempEventQueue = _executionEventQueue;
                    _executionEventQueue = new Queue<ExecutionEventQueueItem>();
                    _executionEventQueue.Clear();
                    ExecutionEventQueueItem queueItem = tempEventQueue.Dequeue();
                    if (runspaceHasAvailabilityChangedSubscribers && queueItem.NewRunspaceAvailability != queueItem.CurrentRunspaceAvailability)
                        _runspace.RaiseAvailabilityChangedEvent(queueItem.NewRunspaceAvailability);
                    // this is shipped as part of V1. So disabling the warning here.
                    // Exception raised in the eventhandler are not error in pipeline.
                    // silently ignore them.
                            stateChanged(this, new PipelineStateEventArgs(queueItem.PipelineStateInfo));
        /// ManualResetEvent which is signaled when pipeline execution is
        /// completed/failed/stopped.
        internal ManualResetEvent PipelineFinishedEvent { get; private set; }
        #region streams
        /// OutputStream from PipelineProcessor. Host will read on
        /// ObjectReader of this stream. PipelineProcessor will write to
        /// ObjectWriter of this stream.
        protected ObjectStreamBase OutputStream { get; }
        private ObjectStreamBase _errorStream;
        /// ErrorStream from PipelineProcessor. Host will read on
        protected ObjectStreamBase ErrorStream
                return _errorStream;
                Dbg.Assert(value != null, "ErrorStream cannot be null");
                _errorStream = value;
                _errorStream.DataReady += OnErrorStreamDataReady;
        // Winblue: 26115. This handler is used to populate Pipeline.HadErrors.
        private void OnErrorStreamDataReady(object sender, EventArgs e)
            if (_errorStream.Count > 0)
                // unsubscribe from further event notifications as
                // this notification is suffice to say there is an
                // error.
                _errorStream.DataReady -= OnErrorStreamDataReady;
        /// Informational Buffers that represent verbose, debug, progress,
        /// warning emanating from the command execution.
        /// Informational buffers are introduced after 1.0. This can be
        /// null if executing command as part of 1.0 hosting interfaces.
        protected PSInformationalBuffers InformationalBuffers { get; }
        /// Stream for providing input to PipelineProcessor. Host will write on
        /// ObjectWriter of this stream. PipelineProcessor will read from
        /// ObjectReader of this stream.
        protected ObjectStreamBase InputStream { get; }
        #endregion streams
        #region history
        // History information is internal so that Pipeline serialization code
        // can access it.
        /// If true, this pipeline is added in history.
        internal bool AddToHistory { get; set; }
        /// String which is added in the history.
        /// <remarks>This needs to be internal so that it can be replaced
        /// by invoke-cmd to place correct string in history.</remarks>
        internal string HistoryString { get; set; }
        #endregion history
        /// Initialized the current pipeline instance with the supplied data.
        /// 1. addToHistory is true and command is null.
        private void Initialize(Runspace runspace, string command, bool addToHistory, bool isNested)
            Dbg.Assert(runspace != null, "caller should validate the parameter");
            if (addToHistory && command == null)
                Commands.Add(new Command(command, true, false));
                HistoryString = command;
        private RunspaceBase RunspaceBase
                return (RunspaceBase)Runspace;

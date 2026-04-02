    internal class RemotePipeline : Pipeline
        private PowerShell _powershell;
        private readonly bool _addToHistory;
        private bool _isSteppable;
        private string _historyString;
        private readonly CommandCollection _commands = new CommandCollection();
        private readonly ConnectCommandInfo _connectCmdInfo = null;
        private readonly bool _performNestedCheck = true;
        /// Private constructor that does most of the work constructing a remote pipeline object.
        /// <param name="runspace">RemoteRunspace object.</param>
        /// <param name="addToHistory">AddToHistory.</param>
        /// <param name="isNested">IsNested.</param>
        private RemotePipeline(RemoteRunspace runspace, bool addToHistory, bool isNested)
            _addToHistory = addToHistory;
            _isSteppable = false;
            _computerName = ((RemoteRunspace)_runspace).ConnectionInfo.ComputerName;
            _runspaceId = _runspace.InstanceId;
            _inputCollection = new PSDataCollection<object>();
            _inputCollection.ReleaseOnEnumeration = true;
            _inputStream = new PSDataCollectionStream<object>(Guid.Empty, _inputCollection);
            _outputCollection = new PSDataCollection<PSObject>();
            _outputStream = new PSDataCollectionStream<PSObject>(Guid.Empty, _outputCollection);
            _errorCollection = new PSDataCollection<ErrorRecord>();
            _errorStream = new PSDataCollectionStream<ErrorRecord>(Guid.Empty, _errorCollection);
            // Create object stream for method executor objects.
            MethodExecutorStream = new ObjectStream();
            IsMethodExecutorStreamEnabled = false;
            SetCommandCollection(_commands);
        /// Constructs a remote pipeline for the specified runspace and
        /// specified command.
        /// <param name="runspace">Runspace in which to create the pipeline.</param>
        /// <param name="command">Command as a string, to be used in pipeline creation.</param>
        /// <param name="addToHistory">Whether to add the command to the runspaces history.</param>
        /// <param name="isNested">Whether this pipeline is nested.</param>
        internal RemotePipeline(RemoteRunspace runspace, string command, bool addToHistory, bool isNested)
            : this(runspace, addToHistory, isNested)
                _commands.Add(new Command(command, true));
            // initialize the underlying powershell object
            _powershell = new PowerShell(_inputStream, _outputStream, _errorStream,
                ((RemoteRunspace)_runspace).RunspacePool);
            _powershell.SetIsNested(isNested);
            _powershell.InvocationStateChanged += HandleInvocationStateChanged;
        /// Constructs a remote pipeline object associated with a remote running
        /// command but in a disconnected state.
        /// <param name="runspace">Remote runspace associated with running command.</param>
        internal RemotePipeline(RemoteRunspace runspace)
            : this(runspace, false, false)
            if (runspace.RemoteCommand == null)
                throw new InvalidOperationException(PipelineStrings.InvalidRemoteCommand);
            _connectCmdInfo = runspace.RemoteCommand;
            _commands.Add(_connectCmdInfo.Command);
            // Beginning state will be disconnected.
            SetPipelineState(PipelineState.Disconnected, null);
            // Create the underlying powershell object.
            _powershell = new PowerShell(_connectCmdInfo, _inputStream, _outputStream, _errorStream,
        /// Creates a cloned pipeline from the specified one.
        /// <param name="pipeline">Pipeline to clone from.</param>
        /// <remarks>This constructor is private because this will
        /// only be called from the copy method</remarks>
        private RemotePipeline(RemotePipeline pipeline)
                (RemoteRunspace)pipeline.Runspace,
                command: null,
                addToHistory: false,
                pipeline.IsNested)
            _isSteppable = pipeline._isSteppable;
            // the above comment copied from RemotePipelineBase which
            // originally copied it from PipelineBase
            _addToHistory = pipeline._addToHistory;
            _historyString = pipeline._historyString;
        /// Override for creating a copy of pipeline.
        /// Pipeline object which is copy of this pipeline
            return (Pipeline)new RemotePipeline(this);
        /// Internal method to set the value of IsNested. This is called
        /// by serializer.
        /// Internal method to set the value of IsSteppable. This is called
        /// during DoConcurrentCheck.
        internal void SetIsSteppable(bool isSteppable)
            _isSteppable = isSteppable;
                return _inputStream.ObjectWriter;
                return _outputStream.GetPSObjectReaderForPipeline(_computerName, _runspaceId);
                return _errorStream.GetObjectReaderForPipeline(_computerName, _runspaceId);
        internal string HistoryString
                return _historyString;
                _historyString = value;
        /// Whether the pipeline needs to be added to history of the runspace.
        public bool AddToHistory
                return _addToHistory;
        // Stream and Collection go together...a stream wraps
        // a corresponding collection to support
        // streaming behavior of the pipeline.
        private readonly PSDataCollection<PSObject> _outputCollection;
        private readonly PSDataCollectionStream<PSObject> _outputStream;
        private readonly PSDataCollection<ErrorRecord> _errorCollection;
        private readonly PSDataCollectionStream<ErrorRecord> _errorStream;
        private readonly PSDataCollection<object> _inputCollection;
        private readonly PSDataCollectionStream<object> _inputStream;
        protected PSDataCollectionStream<object> InputStream
                return _inputStream;
        #region Invoke
            InitPowerShell(false);
            CoreInvokeAsync();
            // Initialize PowerShell invocation with "InvokeAndDisconnect" setting.
            InitPowerShell(false, true);
        public override Collection<PSObject> Invoke(System.Collections.IEnumerable input)
                this.InputStream.Close();
            InitPowerShell(true);
            Collection<PSObject> results;
                results = _powershell.Invoke(input);
                        StringUtil.Format(RunspaceStrings.RunspaceNotOpenForPipeline, _runspace.RunspaceStateInfo.State.ToString()),
                        _runspace.RunspaceStateInfo.State,
        #endregion Invoke
            InitPowerShellForConnect(true);
                results = _powershell.Connect();
                        StringUtil.Format(RunspaceStrings.RunspaceNotOpenForPipelineConnect, _runspace.RunspaceStateInfo.State.ToString()),
            // PowerShell object will return empty results if it was provided an alternative object to
            // collect output in.  Check to see if the output was collected in a member variable.
                if (_outputCollection != null && _outputCollection.Count > 0)
                    results = new Collection<PSObject>(_outputCollection);
            InitPowerShellForConnect(false);
                _powershell.ConnectAsync();
        #region Stop
        /// Stop the pipeline synchronously.
            bool isAlreadyStopping = false;
            if (CanStopPipeline(out isAlreadyStopping))
                // A pipeline can be stopped before it is started.so protecting against that
                    IAsyncResult asyncresult = null;
                        asyncresult = _powershell.BeginStop(null, null);
                        throw PSTraceSource.NewObjectDisposedException("Pipeline");
                    asyncresult.AsyncWaitHandle.WaitOne();
            // Waits until pipeline completes stop as this is a sync call.
        /// Stop the pipeline asynchronously.
        /// This method calls the BeginStop on the underlying
        /// powershell and so any exception will be
        /// thrown on the same thread.
            bool isAlreadyStopping;
                    _powershell.BeginStop(null, null);
        /// Verifies if the pipeline is in a state where it can be stopped.
        private bool CanStopPipeline(out bool isAlreadyStopping)
            bool returnResult = false;
            isAlreadyStopping = false;
                // SetPipelineState does not raise events..
                // so locking is ok here.
                switch (_pipelineStateInfo.State)
                        SetPipelineState(PipelineState.Stopping, null);
                        SetPipelineState(PipelineState.Stopped, null);
                        returnResult = false;
                        isAlreadyStopping = true;
                        returnResult = true;
            return returnResult;
        #endregion Stop
        /// Disposes the pipeline.
        /// <param name="disposing">True, when called on Dispose().</param>
                    // wait for the pipeline to stop..this will block
                    // if the pipeline is already stopping.
                    // _pipelineFinishedEvent.Close();
                        _powershell = null;
                    _inputCollection.Dispose();
                    _outputCollection.Dispose();
                    _errorCollection.Dispose();
                    MethodExecutorStream.Dispose();
                    PipelineFinishedEvent.Dispose();
        private void CoreInvokeAsync()
                _powershell.BeginInvoke();
        private void HandleInvocationStateChanged(object sender, PSInvocationStateChangedEventArgs e)
            SetPipelineState((PipelineState)e.InvocationStateInfo.State, e.InvocationStateInfo.Reason);
        private void SetPipelineState(PipelineState state, Exception reason)
            PipelineState copyState = state;
            PipelineStateInfo copyStateInfo = null;
                            if (state == PipelineState.Running)
                            if (state == PipelineState.Running || state == PipelineState.Stopping)
                                copyState = PipelineState.Stopped;
                _pipelineStateInfo = new PipelineStateInfo(copyState, reason);
                copyStateInfo = _pipelineStateInfo;
                Guid? cmdInstanceId = (_powershell != null) ? _powershell.InstanceId : (Guid?)null;
                _runspace.UpdateRunspaceAvailability(_pipelineStateInfo.State, false, cmdInstanceId);
            // using the copyStateInfo here as this piece of code is
            // outside of lock and _pipelineStateInfo might get changed
            // by two threads running concurrently..so its value is
            // not guaranteed to be the same for this entire method call.
            // copyStateInfo is a local variable.
            if (copyStateInfo.State == PipelineState.Completed ||
                copyStateInfo.State == PipelineState.Failed ||
                copyStateInfo.State == PipelineState.Stopped)
        /// Initializes the underlying PowerShell object after verifying
        /// if the pipeline is in a state where it can be invoked.
        /// If invokeAndDisconnect is true then the remote PowerShell
        /// command will be immediately disconnected after it begins
        /// <param name="syncCall">True if called from a sync call.</param>
        /// <param name="invokeAndDisconnect">Invoke and Disconnect.</param>
        private void InitPowerShell(bool syncCall, bool invokeAndDisconnect = false)
            if (_pipelineStateInfo.State != PipelineState.NotStarted)
                        _pipelineStateInfo.State,
            ((RemoteRunspace)_runspace).DoConcurrentCheckAndAddToRunningPipelines(this, syncCall);
            PSInvocationSettings settings = new PSInvocationSettings();
            settings.AddToHistory = _addToHistory;
            settings.InvokeAndDisconnect = invokeAndDisconnect;
            _powershell.InitForRemotePipeline(_commands, _inputStream, _outputStream, _errorStream, settings, RedirectShellErrorOutputPipe);
            _powershell.RemotePowerShell.HostCallReceived += HandleHostCallReceived;
        /// Initializes the underlying PowerShell object after verifying that it is
        /// in a state where it can connect to the remote command.
        /// <param name="syncCall"></param>
        private void InitPowerShellForConnect(bool syncCall)
            if (_pipelineStateInfo.State != PipelineState.Disconnected)
                throw new InvalidPipelineStateException(StringUtil.Format(PipelineStrings.PipelineNotDisconnected),
                                                        PipelineState.Disconnected);
            // The connect may be from the same Pipeline that disconnected and in this case
            // the Pipeline state already exists.  Or this could be a new Pipeline object
            // (connect reconstruction case) and new state is created.
            // Check to see if this pipeline already exists in the runspace.
            RemotePipeline currentPipeline = (RemotePipeline)((RemoteRunspace)_runspace).GetCurrentlyRunningPipeline();
            if (!ReferenceEquals(currentPipeline, this))
            // Initialize the PowerShell object if it hasn't been initialized before.
            if ((_powershell.RemotePowerShell) == null || !_powershell.RemotePowerShell.Initialized)
                _powershell.InitForRemotePipelineConnect(_inputStream, _outputStream, _errorStream, settings, RedirectShellErrorOutputPipe);
        /// Handle host call received.
        /// <param name="eventArgs">Arguments describing the host call to invoke.</param>
            ClientMethodExecutor.Dispatch(
                _powershell.RemotePowerShell.DataStructureHandler.TransportManager,
                ((RemoteRunspace)_runspace).RunspacePool.RemoteRunspacePoolInternal.Host,
                MethodExecutorStream,
                IsMethodExecutorStreamEnabled,
                ((RemoteRunspace)_runspace).RunspacePool.RemoteRunspacePoolInternal,
                _powershell.InstanceId,
                eventArgs.Data);
        /// Does the cleanup necessary on pipeline completion.
            if (_outputStream.IsOpen)
                    _outputCollection.Complete();
            if (_errorStream.IsOpen)
                    _errorCollection.Complete();
            if (_inputStream.IsOpen)
                    _inputCollection.Complete();
                ((RemoteRunspace)_runspace).RemoveFromRunningPipelineList(this);
                PipelineFinishedEvent.Set();
        internal ManualResetEvent PipelineFinishedEvent { get; }
        /// Is method executor stream enabled.
        internal bool IsMethodExecutorStreamEnabled { get; set; }
        /// Method executor stream.
        internal ObjectStream MethodExecutorStream { get; }
        internal void DoConcurrentCheck(bool syncCall)
            RemotePipeline currentPipeline =
                (RemotePipeline)((RemoteRunspace)_runspace).GetCurrentlyRunningPipeline();
            if (!_isNested)
                if (currentPipeline == null &&
                    ((RemoteRunspace)_runspace).RunspaceAvailability != RunspaceAvailability.Busy &&
                    ((RemoteRunspace)_runspace).RunspaceAvailability != RunspaceAvailability.RemoteDebug)
                    // We can add a new pipeline to the runspace only if it is
                    // available (not busy).
                    ((RemoteRunspace)_runspace).RemoteCommand != null &&
                    _connectCmdInfo != null &&
                    Guid.Equals(((RemoteRunspace)_runspace).RemoteCommand.CommandId, _connectCmdInfo.CommandId))
                    // Connect case.  We can add a pipeline to a busy runspace when
                    // that pipeline represents the same command as is currently
                    // running.
                if (currentPipeline != null &&
                         ReferenceEquals(currentPipeline, this))
                    // Reconnect case.  We can add a pipeline to a busy runspace when the
                    // pipeline is the same (reconnecting).
                if (!_isSteppable)
                    if (_isSteppable)
        /// The underlying powershell object on which this remote pipeline
        /// is created.
                return _powershell;
        /// Sets the history string to the specified string.
        /// <param name="historyString">New history string to set to.</param>
            _powershell.HistoryString = historyString;
        internal override void SuspendIncomingData()
            _powershell.SuspendIncomingData();
        internal override void ResumeIncomingData()
            _powershell.ResumeIncomingData();
        internal override void DrainIncomingData()
            _powershell.WaitForServicingComplete();

namespace System.Management.Automation.PSTasks
    #region PSTask
    /// Class to encapsulate synchronous running scripts in parallel.
    internal sealed class PSTask : PSTaskBase
        private readonly PSTaskDataStreamWriter _dataStreamWriter;
        /// Initializes a new instance of the <see cref="PSTask"/> class.
        /// <param name="scriptBlock">Script block to run in task.</param>
        /// <param name="usingValuesMap">Using values passed into script block.</param>
        /// <param name="dollarUnderbar">Dollar underbar variable value.</param>
        /// <param name="currentLocationPath">Current working directory.</param>
        /// <param name="dataStreamWriter">Cmdlet data stream writer.</param>
        public PSTask(
            Dictionary<string, object> usingValuesMap,
            object dollarUnderbar,
            string currentLocationPath,
            PSTaskDataStreamWriter dataStreamWriter)
                usingValuesMap,
                dollarUnderbar,
                currentLocationPath)
            _dataStreamWriter = dataStreamWriter;
        /// Initialize PowerShell object.
        protected override void InitializePowershell()
            // Writer data stream handlers
            _output.DataAdded += (sender, args) => HandleOutputData();
            _powershell.Streams.Error.DataAdded += (sender, args) => HandleErrorData();
            _powershell.Streams.Warning.DataAdded += (sender, args) => HandleWarningData();
            _powershell.Streams.Verbose.DataAdded += (sender, args) => HandleVerboseData();
            _powershell.Streams.Debug.DataAdded += (sender, args) => HandleDebugData();
            _powershell.Streams.Progress.DataAdded += (sender, args) => HandleProgressData();
            _powershell.Streams.Information.DataAdded += (sender, args) => HandleInformationData();
            // State change handler
            _powershell.InvocationStateChanged += (sender, args) => HandleStateChanged(args);
        #region Writer data stream handlers
        private void HandleOutputData()
            foreach (var item in _output.ReadAll())
                _dataStreamWriter.Add(
                    new PSStreamObject(PSStreamObjectType.Output, item));
        private void HandleErrorData()
            foreach (var item in _powershell.Streams.Error.ReadAll())
                    new PSStreamObject(PSStreamObjectType.Error, item));
        private void HandleWarningData()
            foreach (var item in _powershell.Streams.Warning.ReadAll())
                    new PSStreamObject(PSStreamObjectType.Warning, item.Message));
        private void HandleVerboseData()
            foreach (var item in _powershell.Streams.Verbose.ReadAll())
                    new PSStreamObject(PSStreamObjectType.Verbose, item.Message));
        private void HandleDebugData()
            foreach (var item in _powershell.Streams.Debug.ReadAll())
                    new PSStreamObject(PSStreamObjectType.Debug, item.Message));
        private void HandleInformationData()
            foreach (var item in _powershell.Streams.Information.ReadAll())
                    new PSStreamObject(PSStreamObjectType.Information, item));
        private void HandleProgressData()
            foreach (var item in _powershell.Streams.Progress.ReadAll())
                    new PSStreamObject(PSStreamObjectType.Progress, item));
        #region Event handlers
        private void HandleStateChanged(PSInvocationStateChangedEventArgs stateChangeInfo)
            if (_dataStreamWriter != null)
                // Treat any terminating exception as a non-terminating error record
                var newStateInfo = stateChangeInfo.InvocationStateInfo;
                if (newStateInfo.Reason != null)
                        newStateInfo.Reason,
                        "PSTaskException",
                        new PSStreamObject(PSStreamObjectType.Error, errorRecord));
            RaiseStateChangedEvent(stateChangeInfo);
    /// Class to encapsulate asynchronous running scripts in parallel as jobs.
    internal sealed class PSJobTask : PSTaskBase
        /// Initializes a new instance of the <see cref="PSJobTask"/> class.
        /// <param name="scriptBlock">Script block to run.</param>
        /// <param name="usingValuesMap">Using variable values passed to script block.</param>
        /// <param name="dollarUnderbar">Dollar underbar variable value for script block.</param>
        /// <param name="job">Job object associated with task.</param>
        public PSJobTask(
            Job job) : base(
            // Job data stream handlers
            _output.DataAdded += (sender, args) => HandleJobOutputData();
            _powershell.Streams.Error.DataAdded += (sender, args) => HandleJobErrorData();
            _powershell.Streams.Warning.DataAdded += (sender, args) => HandleJobWarningData();
            _powershell.Streams.Verbose.DataAdded += (sender, args) => HandleJobVerboseData();
            _powershell.Streams.Debug.DataAdded += (sender, args) => HandleJobDebugData();
            _powershell.Streams.Information.DataAdded += (sender, args) => HandleJobInformationData();
        #region Job data stream handlers
        private void HandleJobOutputData()
                _job.Output.Add(item);
                _job.Results.Add(
        private void HandleJobErrorData()
                _job.Error.Add(item);
        private void HandleJobWarningData()
                _job.Warning.Add(item);
        private void HandleJobVerboseData()
                _job.Verbose.Add(item);
        private void HandleJobDebugData()
                _job.Debug.Add(item);
        private void HandleJobInformationData()
                _job.Information.Add(item);
        /// Gets Debugger.
        public Debugger Debugger
            get => _powershell.Runspace.Debugger;
    /// Base class to encapsulate running a PowerShell script concurrently in a cmdlet or job context.
    internal abstract class PSTaskBase : IDisposable
        private readonly ScriptBlock _scriptBlockToRun;
        private readonly Dictionary<string, object> _usingValuesMap;
        private readonly object _dollarUnderbar;
        private readonly int _id;
        private readonly string _currentLocationPath;
        protected PowerShell _powershell;
        protected PSDataCollection<PSObject> _output;
        public const string RunspaceName = "PSTask";
        private static int s_taskId;
        /// Event that fires when the task running state changes.
        public event EventHandler<PSInvocationStateChangedEventArgs> StateChanged;
        internal void RaiseStateChangedEvent(PSInvocationStateChangedEventArgs args)
            StateChanged.SafeInvoke(this, args);
        /// Gets current running state of the task.
        public PSInvocationState State
                PowerShell ps = _powershell;
                if (ps != null)
                    return ps.InvocationStateInfo.State;
                return PSInvocationState.NotStarted;
        /// Gets Task Id.
        public int Id { get => _id; }
        /// Gets Task Runspace.
        public Runspace Runspace { get => _runspace; }
        private PSTaskBase()
            _id = Interlocked.Increment(ref s_taskId);
        /// Initializes a new instance of the <see cref="PSTaskBase"/> class.
        protected PSTaskBase(
            string currentLocationPath) : this()
            _scriptBlockToRun = scriptBlock;
            _usingValuesMap = usingValuesMap;
            _currentLocationPath = currentLocationPath;
        protected abstract void InitializePowershell();
        /// Dispose PSTaskBase instance.
            _output.Dispose();
        /// Start task.
        /// <param name="runspace">Runspace used to run task.</param>
        public void Start(Runspace runspace)
                Dbg.Assert(false, "A PSTask can be started only once.");
            Dbg.Assert(runspace != null, "Task runspace cannot be null.");
            // If available, set current working directory on the runspace.
            // Temporarily set the newly created runspace as the thread default runspace for any needed module loading.
            if (_currentLocationPath != null)
                var oldDefaultRunspace = Runspace.DefaultRunspace;
                    Runspace.DefaultRunspace = runspace;
                    var context = new CmdletProviderContext(runspace.ExecutionContext)
                        // _currentLocationPath denotes the current path as-is, and should not be attempted expanded.
                        SuppressWildcardExpansion = true
                    runspace.ExecutionContext.SessionState.Internal.SetLocation(_currentLocationPath, context);
                    // Allow task to run if current drive is not available.
                    Runspace.DefaultRunspace = oldDefaultRunspace;
            // Create the PowerShell command pipeline for the provided script block
            // The script will run on the provided Runspace in a new thread by default
            _powershell = PowerShell.Create(runspace);
            // Initialize PowerShell object data streams and event handlers
            _output = new PSDataCollection<PSObject>();
            InitializePowershell();
            // Start the script running in a new thread
            _powershell.AddScript(_scriptBlockToRun.ToString());
            _powershell.Commands.Commands[0].DollarUnderbar = _dollarUnderbar;
            if (_usingValuesMap != null && _usingValuesMap.Count > 0)
                _powershell.AddParameter(Parser.VERBATIM_ARGUMENT, _usingValuesMap);
            _powershell.BeginInvoke<object, PSObject>(input: null, output: _output);
        /// Signals the running task to stop.
        public void SignalStop() => _powershell?.BeginStop(null, null);
    #region PSTaskDataStreamWriter
    /// Class that handles writing task data stream objects to a cmdlet.
    internal sealed class PSTaskDataStreamWriter : IDisposable
        private readonly PSDataCollection<PSStreamObject> _dataStream;
        private readonly int _cmdletThreadId;
        /// Gets wait-able handle that signals when new data has been added to
        /// the data stream collection.
        /// <returns>Data added wait handle.</returns>
        internal WaitHandle DataAddedWaitHandle
            get => _dataStream.WaitHandle;
        private PSTaskDataStreamWriter() { }
        /// Initializes a new instance of the <see cref="PSTaskDataStreamWriter"/> class.
        /// <param name="psCmdlet">Parent cmdlet.</param>
        public PSTaskDataStreamWriter(PSCmdlet psCmdlet)
            _cmdlet = psCmdlet;
            _cmdletThreadId = Environment.CurrentManagedThreadId;
            _dataStream = new PSDataCollection<PSStreamObject>();
        /// Add data stream object to the writer.
        /// <param name="streamObject">Data stream object to write.</param>
        public void Add(PSStreamObject streamObject)
            _dataStream.Add(streamObject);
        /// Write all objects in data stream collection to the cmdlet data stream.
        public void WriteImmediate()
            CheckCmdletThread();
            foreach (var item in _dataStream.ReadAll())
                item.WriteStreamObject(cmdlet: _cmdlet, overrideInquire: true);
        /// Waits for data stream objects to be added to the collection, and writes them
        /// to the cmdlet data stream.
        /// This method returns only after the writer has been closed.
        public void WaitAndWrite()
                _dataStream.WaitHandle.WaitOne();
                WriteImmediate();
                if (!_dataStream.IsOpen)
        /// Closes the stream writer.
            _dataStream.Complete();
        private void CheckCmdletThread()
            if (Environment.CurrentManagedThreadId != _cmdletThreadId)
                throw new PSInvalidOperationException(InternalCommandStrings.PSTaskStreamWriterWrongThread);
        /// Dispose the stream writer.
            _dataStream.Dispose();
    #region PSTaskPool
    /// Pool for running PSTasks, with limit of total number of running tasks at a time.
    internal sealed class PSTaskPool : IDisposable
        private readonly ManualResetEvent _addAvailable;
        private readonly int _sizeLimit;
        private readonly ManualResetEvent _stopAll;
        private readonly Dictionary<int, PSTaskBase> _taskPool;
        private readonly ConcurrentQueue<Runspace> _runspacePool;
        private readonly ConcurrentDictionary<int, Runspace> _activeRunspaces;
        private readonly WaitHandle[] _waitHandles;
        private readonly bool _useRunspacePool;
        private bool _isOpen;
        private int _createdRunspaceCount;
        private const int AddAvailable = 0;
        private const int Stop = 1;
        private PSTaskPool() { }
        /// Initializes a new instance of the <see cref="PSTaskPool"/> class.
        /// <param name="size">Total number of allowed running objects in pool at one time.</param>
        /// <param name="useNewRunspace">When true, a new runspace object is created for the task instead of reusing one from the pool.</param>
        public PSTaskPool(
            int size,
            bool useNewRunspace)
            _sizeLimit = size;
            _useRunspacePool = !useNewRunspace;
            _isOpen = true;
            _addAvailable = new ManualResetEvent(true);
            _stopAll = new ManualResetEvent(false);
            _waitHandles = new WaitHandle[]
                _addAvailable,      // index 0
                _stopAll,           // index 1
            _taskPool = new Dictionary<int, PSTaskBase>(size);
            _activeRunspaces = new ConcurrentDictionary<int, Runspace>();
            if (_useRunspacePool)
                _runspacePool = new ConcurrentQueue<Runspace>();
        /// Event that fires when pool is closed and drained of all tasks.
        public event EventHandler<EventArgs> PoolComplete;
        /// Gets a value indicating whether a pool is currently open for accepting tasks.
            get => _isOpen;
        /// Gets a value of the count of total runspaces allocated.
        public int AllocatedRunspaceCount
            get => _createdRunspaceCount;
        /// Dispose task pool.
            _addAvailable.Dispose();
            _stopAll.Dispose();
            DisposeRunspaces();
        /// Dispose runspaces.
        internal void DisposeRunspaces()
            foreach (var item in _activeRunspaces)
                item.Value.Dispose();
            _activeRunspaces.Clear();
        /// Method to add a task to the pool.
        /// If the pool is full, then this method blocks until space is available.
        /// This method is not multi-thread safe and assumes only one thread waits and adds tasks.
        /// <param name="task">Task to be added to pool.</param>
        /// <returns>True when task is successfully added.</returns>
        public bool Add(PSTaskBase task)
            // Block until either space is available, or a stop is commanded
            var index = WaitHandle.WaitAny(_waitHandles);
            switch (index)
                case AddAvailable:
                    var runspace = GetRunspace(task.Id);
                    task.StateChanged += HandleTaskStateChangedDelegate;
                        _taskPool.Add(task.Id, task);
                        if (_taskPool.Count == _sizeLimit)
                            _addAvailable.Reset();
                        task.Start(runspace);
                case Stop:
        /// Add child job task to task pool.
        /// <param name="childJob">Child job to be added to pool.</param>
        /// <returns>True when child job is successfully added.</returns>
        public bool Add(PSTaskChildJob childJob)
            return Add(childJob.Task);
        /// Signals all running tasks to stop and closes pool for any new tasks.
        public void StopAll()
            // Accept no more input
            _stopAll.Set();
            // Stop all running tasks
            PSTaskBase[] tasksToStop;
                tasksToStop = new PSTaskBase[_taskPool.Values.Count];
                _taskPool.Values.CopyTo(tasksToStop, 0);
            foreach (var task in tasksToStop)
                task.Dispose();
            // Dispose all active runspaces
            _stopping = false;
        /// Closes the pool and prevents any new tasks from being added.
            CheckForComplete();
        private void HandleTaskStateChangedDelegate(object sender, PSInvocationStateChangedEventArgs args) => HandleTaskStateChanged(sender, args);
        private void HandleTaskStateChanged(object sender, PSInvocationStateChangedEventArgs args)
            var task = sender as PSTaskBase;
            Dbg.Assert(task != null, "State changed sender must always be PSTaskBase");
            var stateInfo = args.InvocationStateInfo;
            switch (stateInfo.State)
                // Look for completed state and remove
                    ReturnRunspace(task);
                        _taskPool.Remove(task.Id);
                        if (_taskPool.Count == (_sizeLimit - 1))
                            _addAvailable.Set();
                    task.StateChanged -= HandleTaskStateChangedDelegate;
                    if (!_stopping || stateInfo.State != PSInvocationState.Stopped)
                        // StopAll disposes tasks.
        private void CheckForComplete()
            bool isTaskPoolComplete;
                isTaskPoolComplete = !_isOpen && _taskPool.Count == 0;
            if (isTaskPoolComplete)
                    PoolComplete.SafeInvoke(
                        new EventArgs());
                    Dbg.Assert(false, "Exceptions should not be thrown on event thread");
        private Runspace GetRunspace(int taskId)
            var runspaceName = string.Create(CultureInfo.InvariantCulture, $"{PSTask.RunspaceName}:{taskId}");
            if (_useRunspacePool && _runspacePool.TryDequeue(out Runspace runspace))
                if (runspace.RunspaceStateInfo.State == RunspaceState.Opened &&
                    runspace.RunspaceAvailability == RunspaceAvailability.Available)
                        runspace.ResetRunspaceState();
                        runspace.Name = runspaceName;
                        // If the runspace cannot be reset for any reason, remove it.
                RemoveActiveRunspace(runspace);
            // Create and initialize a new Runspace
            var iss = InitialSessionState.CreateDefault2();
                    iss.LanguageMode = PSLanguageMode.ConstrainedLanguage;
                    // In audit mode, CL restrictions are not enforced and instead audit
                    // log entries are created.
                    iss.LanguageMode = PSLanguageMode.FullLanguage;
            runspace = RunspaceFactory.CreateRunspace(iss);
            _activeRunspaces.TryAdd(runspace.Id, runspace);
            _createdRunspaceCount++;
        private void ReturnRunspace(PSTaskBase task)
            var runspace = task.Runspace;
            if (_useRunspacePool &&
                runspace.RunspaceStateInfo.State == RunspaceState.Opened &&
                _runspacePool.Enqueue(runspace);
        private void RemoveActiveRunspace(Runspace runspace)
            _activeRunspaces.TryRemove(runspace.Id, out Runspace _);
    #region PSTaskJobs
    /// Job for running ForEach-Object parallel task child jobs asynchronously.
    public sealed class PSTaskJob : Job
        private readonly PSTaskPool _taskPool;
        private bool _stopSignaled;
            get => _taskPool.AllocatedRunspaceCount;
        private PSTaskJob() { }
        /// Initializes a new instance of the <see cref="PSTaskJob"/> class.
        /// <param name="command">Job command text.</param>
        /// <param name="throttleLimit">Pool size limit for task job.</param>
        internal PSTaskJob(
            string command,
            int throttleLimit,
            bool useNewRunspace) : base(command, string.Empty)
            _taskPool = new PSTaskPool(throttleLimit, useNewRunspace);
            PSJobTypeName = nameof(PSTaskJob);
            _taskPool.PoolComplete += (sender, args) => HandleTaskPoolComplete(sender, args);
        /// Gets Location.
            get => "PowerShell";
        /// Gets HasMoreData.
                foreach (var childJob in ChildJobs)
                    if (childJob.HasMoreData)
        /// Gets StatusMessage.
            get => string.Empty;
        /// Stops running job.
            _stopSignaled = true;
            SetJobState(JobState.Stopping);
            _taskPool.StopAll();
        /// Disposes task job.
        /// <param name="disposing">Indicates disposing action.</param>
                _taskPool.Dispose();
        /// Add a child job to the collection.
        /// <param name="childJob">Child job to add.</param>
        internal bool AddJob(PSTaskChildJob childJob)
        /// Closes this parent job to adding more child jobs and starts
        /// the child jobs running with the provided throttle limit.
            // Submit jobs to the task pool, blocking when throttle limit is reached.
            // This thread will end once all jobs reach a finished state by either running
            // to completion, terminating with error, or stopped.
                        _taskPool.Add((PSTaskChildJob)childJob);
        private void HandleTaskPoolComplete(object sender, EventArgs args)
                if (_stopSignaled)
                    SetJobState(JobState.Stopped, new PipelineStoppedException());
                // Final state will be 'Complete', only if all child jobs completed successfully.
                JobState finalState = JobState.Completed;
                    if (childJob.JobStateInfo.State != JobState.Completed)
                        finalState = JobState.Failed;
                SetJobState(finalState);
                // Release job task pool runspace resources.
                (sender as PSTaskPool).DisposeRunspaces();
    /// PSTaskChildJob debugger wrapper.
    internal sealed class PSTaskChildDebugger : Debugger
        private readonly Debugger _wrappedDebugger;
        private readonly string _jobName;
        private PSTaskChildDebugger() { }
        /// Initializes a new instance of the <see cref="PSTaskChildDebugger"/> class.
        /// <param name="debugger">Script debugger associated with task.</param>
        /// <param name="jobName">Job name for associated task.</param>
        public PSTaskChildDebugger(
            Debugger debugger,
            string jobName)
            _wrappedDebugger = debugger;
            _jobName = jobName ?? string.Empty;
            // Create handlers for wrapped debugger events.
        #region Debugger overrides
        /// <param name="output">PowerShell output.</param>
        /// <returns>Debugger command results.</returns>
        public override DebuggerCommandResults ProcessCommand(
            PSCommand command,
            PSDataCollection<PSObject> output)
            // Special handling for the prompt command.
            if (command.Commands[0].CommandText.Trim().Equals("prompt", StringComparison.OrdinalIgnoreCase))
        /// <param name="breakpoints">List of breakpoints.</param>
        /// <returns>The breakpoint with the specified id.</returns>
        /// <returns>Debugger stop eventArgs.</returns>
        /// Sets the parent debugger, breakpoints, and other debugging context information.
        /// <param name="host">PowerShell host.</param>
        public override void SetParent(
            // For now always enable step mode debugging.
        /// <param name="mode">Debugger mode to set.</param>
        /// <returns>Enumerable call stack.</returns>
            return _wrappedDebugger.GetCallStack();
        /// <param name="enabled">True to enable debugger step mode.</param>
        /// Gets boolean indicating when debugger is stopped at a breakpoint.
            get => _wrappedDebugger.InBreakpoint;
        private void HandleDebuggerStop(object sender, DebuggerStopEventArgs e)
        private void HandleBreakpointUpdated(object sender, BreakpointUpdatedEventArgs e)
        private DebuggerCommandResults HandlePromptCommand(PSDataCollection<PSObject> output)
            // [DBG]: [JobName]: PS C:\>>
            string promptScript = "'[DBG]: '" + " + " + "'[" + CodeGeneration.EscapeSingleQuotedStringContent(_jobName) + "]: '" + " + " + @"""PS $($executionContext.SessionState.Path.CurrentLocation)>> """;
            _wrappedDebugger.ProcessCommand(promptCommand, output);
    /// Task child job that wraps asynchronously running tasks.
    internal sealed class PSTaskChildJob : Job, IJobDebugger
        private readonly PSJobTask _task;
        private PSTaskChildDebugger _jobDebuggerWrapper;
        private PSTaskChildJob() { }
        /// Initializes a new instance of the <see cref="PSTaskChildJob"/> class.
        public PSTaskChildJob(
            string currentLocationPath)
            : base(scriptBlock.ToString(), string.Empty)
            PSJobTypeName = nameof(PSTaskChildJob);
            _task = new PSJobTask(scriptBlock, usingValuesMap, dollarUnderbar, currentLocationPath, this);
            _task.StateChanged += (sender, args) => HandleTaskStateChange(sender, args);
        /// Gets child job task.
        internal PSTaskBase Task
            get => _task;
            get => this.Output.Count > 0 ||
                   this.Error.Count > 0 ||
                   this.Progress.Count > 0 ||
                   this.Verbose.Count > 0 ||
                   this.Debug.Count > 0 ||
                   this.Warning.Count > 0 ||
                   this.Information.Count > 0;
            _task.SignalStop();
        #region IJobDebugger
        /// Gets Job Debugger.
                _jobDebuggerWrapper ??= new PSTaskChildDebugger(
                    _task.Debugger,
                return _jobDebuggerWrapper;
        /// Gets or sets a value indicating whether IAsync.
        public bool IsAsync { get; set; }
        private void HandleTaskStateChange(object sender, PSInvocationStateChangedEventArgs args)
                    SetJobState(JobState.Stopped, stateInfo.Reason);
                    SetJobState(JobState.Failed, stateInfo.Reason);
                    SetJobState(JobState.Completed, stateInfo.Reason);

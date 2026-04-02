// Stops compiler from warning about unknown warnings
    /// Enumeration for job status values. Indicates the status
    /// of the result object.
    public enum JobState
        /// Execution of command in job not started.
        /// Execution of command in progress.
        /// Execution of command completed in all
        /// computernames/runspaces.
        Completed = 2,
        /// An error was encountered when trying to executed
        /// command in one or more computernames/runspaces.
        Failed = 3,
        /// Command execution is cancelled (stopped) in one or more
        Stopped = 4,
        /// Command execution is blocked (on user input host calls etc)
        Blocked = 5,
        /// The job has been suspended.
        Suspended = 6,
        /// The job is a remote job and has been disconnected from the server.
        /// Suspend is in progress.
        Suspending = 8,
        /// Stop is in progress.
        Stopping = 9,
        /// Script execution is halted in a debugger stop.
        AtBreakpoint = 10
    /// Defines exception which is thrown when state of the PSJob is different
    public class InvalidJobStateException : SystemException
        /// Creates a new instance of InvalidPSJobStateException class.
        public InvalidJobStateException()
            PSRemotingErrorInvariants.FormatResourceString
                RemotingErrorIdStrings.InvalidJobStateGeneral
        public InvalidJobStateException(string message)
        public InvalidJobStateException(string message, Exception innerException)
        /// Creates a new instance of InvalidJobStateException class.
        /// <param name="currentState">
        /// The Job State at the time of the error.
        /// <param name="actionMessage">
        /// An additional message that gives more information about the error. Used
        /// for context after a generalized error message.
        public InvalidJobStateException(JobState currentState, string actionMessage)
                RemotingErrorIdStrings.InvalidJobStateSpecific, currentState, actionMessage
        /// Initializes a new instance of the InvalidPSJobStateException and defines value of
        internal InvalidJobStateException(JobState currentState)
        /// Initializes a new instance of the InvalidPSJobStateException
        InvalidJobStateException(SerializationInfo info, StreamingContext context)
        /// Gets CurrentState of the Job.
        public JobState CurrentState
        /// State of job when exception was thrown.
        private readonly JobState _currState = 0;
    /// Type which has information about JobState and Exception
    /// ,if any, associated with JobState.
    public sealed class JobStateInfo
        public JobStateInfo(JobState state)
        public JobStateInfo(JobState state, Exception reason)
        /// <param name="jobStateInfo">Source information.</param>
        /// ArgumentNullException when <paramref name="jobStateInfo"/> is null.
        internal JobStateInfo(JobStateInfo jobStateInfo)
            State = jobStateInfo.State;
            Reason = jobStateInfo.Reason;
        /// The state of the job.
        /// This value indicates the state of the job .
        public JobState State { get; }
        internal JobStateInfo Clone()
            return new JobStateInfo(this);
    /// Event arguments passed to JobStateEvent handlers
    /// <see cref="Job.StateChanged"/> event.
    public sealed class JobStateEventArgs : EventArgs
        /// Constructor of JobStateEventArgs.
        /// <param name="jobStateInfo">The current state of the job.</param>
        public JobStateEventArgs(JobStateInfo jobStateInfo)
            : this(jobStateInfo, null)
        /// <param name="previousJobStateInfo">The previous state of the job.</param>
        public JobStateEventArgs(JobStateInfo jobStateInfo, JobStateInfo previousJobStateInfo)
            if (jobStateInfo == null)
                throw PSTraceSource.NewArgumentNullException(nameof(jobStateInfo));
            JobStateInfo = jobStateInfo;
            PreviousJobStateInfo = previousJobStateInfo;
        /// Info about the current state of the job.
        public JobStateInfo JobStateInfo { get; }
        /// Info about the previous state of the job.
        public JobStateInfo PreviousJobStateInfo { get; }
    /// Object that must be created by PowerShell to allow reuse of an ID for a job.
    /// Also allows setting of the Instance Id so that jobs may be recreated.
    public sealed class JobIdentifier
        internal JobIdentifier(int id, Guid instanceId)
                PSTraceSource.NewArgumentException(nameof(id), RemotingErrorIdStrings.JobSessionIdLessThanOne, id);
            InstanceId = instanceId;
        internal int Id { get; }
    /// Interface to expose a job debugger.
    public interface IJobDebugger
        /// Job Debugger.
        Debugger? Debugger
        /// True if job is running asynchronously.
        bool IsAsync
    /// Represents a command running in background. A job object can internally
    /// contain many child job objects.
    public abstract class Job : IDisposable
        protected Job()
            Id = System.Threading.Interlocked.Increment(ref s_jobIdSeed);
        /// Creates an instance of this class.
        /// <param name="command">Command invoked by this job object.</param>
        protected Job(string command)
            _name = AutoGenerateJobName();
        /// <param name="name">Friendly name for the job object.</param>
        protected Job(string command, string name)
            : this(command)
        /// <param name="childJobs">Child jobs of this job object.</param>
        protected Job(string command, string name, IList<Job> childJobs)
            : this(command, name)
            _childJobs = childJobs;
        /// <param name="token">Id and InstanceId pair to be used for this job object.</param>
        /// <remarks>The JobIdentifier is a token that must be issued by PowerShell to allow
        /// reuse of the Id. This is the only way to set either Id or instance Id.</remarks>
        protected Job(string command, string name, JobIdentifier token)
            if (token == null)
                throw PSTraceSource.NewArgumentNullException(nameof(token), RemotingErrorIdStrings.JobIdentifierNull);
            if (token.Id > s_jobIdSeed)
                throw PSTraceSource.NewArgumentException(nameof(token), RemotingErrorIdStrings.JobIdNotYetAssigned, token.Id);
            Id = token.Id;
            InstanceId = token.InstanceId;
        /// <param name="instanceId">InstanceId to be used for this job object.</param>
        /// <remarks>The InstanceId may need to be set to maintain job identity across
        /// instances of the process.</remarks>
        protected Job(string command, string name, Guid instanceId)
        internal static string GetCommandTextFromInvocationInfo(InvocationInfo invocationInfo)
            IScriptExtent scriptExtent = invocationInfo.ScriptPosition;
            if ((scriptExtent != null) && (scriptExtent.StartScriptPosition != null) && !string.IsNullOrWhiteSpace(scriptExtent.StartScriptPosition.Line))
                Dbg.Assert(scriptExtent.StartScriptPosition.ColumnNumber > 0, "Column numbers start at 1");
                Dbg.Assert(scriptExtent.StartScriptPosition.ColumnNumber <= scriptExtent.StartScriptPosition.Line.Length, "Column numbers are not greater than the length of a line");
                return scriptExtent.StartScriptPosition.Line.AsSpan(scriptExtent.StartScriptPosition.ColumnNumber - 1).Trim().ToString();
            return invocationInfo.InvocationName;
        private ManualResetEvent _finished = new ManualResetEvent(false);
        private IList<Job> _childJobs;
        internal readonly object syncObject = new object();   // object used for synchronization
        // ISSUE: Should Result be public property
        private PSDataCollection<PSStreamObject> _results = new PSDataCollection<PSStreamObject>();
        private bool _resultsOwner = true;
        private PSDataCollection<ErrorRecord> _error = new PSDataCollection<ErrorRecord>();
        private bool _errorOwner = true;
        private PSDataCollection<ProgressRecord> _progress = new PSDataCollection<ProgressRecord>();
        private bool _progressOwner = true;
        private PSDataCollection<VerboseRecord> _verbose = new PSDataCollection<VerboseRecord>();
        private bool _verboseOwner = true;
        private PSDataCollection<WarningRecord> _warning = new PSDataCollection<WarningRecord>();
        private bool _warningOwner = true;
        private PSDataCollection<DebugRecord> _debug = new PSDataCollection<DebugRecord>();
        private bool _debugOwner = true;
        private PSDataCollection<InformationRecord> _information = new PSDataCollection<InformationRecord>();
        private bool _informationOwner = true;
        private PSDataCollection<PSObject> _output = new PSDataCollection<PSObject>();
        private bool _outputOwner = true;
        /// Static variable which is incremented to generate id.
        private static int s_jobIdSeed = 0;
        private string _jobTypeName = string.Empty;
        #region Job Properties
        /// Command Invoked by this Job.
        /// Status of the command execution.
        public JobStateInfo JobStateInfo { get; private set; } = new JobStateInfo(JobState.NotStarted);
        /// Wait Handle which is signaled when job is finished.
        /// This is set when state of the job is set to Completed,
        /// Stopped or Failed.
        public WaitHandle Finished
                    if (_finished != null)
                        return _finished;
                        // Damage control mode:
                        // Somebody is trying to get Finished handle for an already disposed Job instance.
                        // Return an already triggered handle (disposed job is finished by definition).
                        // The newly created handle will not be disposed in a deterministic manner
                        // and in some circumstances can be mistaken for a handle leak.
                        return new ManualResetEvent(true);
        /// Unique identifier for this job.
        public Guid InstanceId { get; } = Guid.NewGuid();
        /// Short identifier for this result which will be
        /// recycled and used within a process.
        /// Name for identifying this job object.
        /// List of child jobs contained within this job.
        public IList<Job> ChildJobs
                if (_childJobs == null)
                        _childJobs ??= new List<Job>();
                return _childJobs;
        /// Success status of the command execution.
        public abstract string StatusMessage { get; }
        /// Indicates that more data is available in this
        /// result object for reading.
        public abstract bool HasMoreData { get; }
        /// Time job was started.
        public DateTime? PSBeginTime { get; protected set; } = null;
        /// Time job stopped.
        public DateTime? PSEndTime { get; protected set; } = null;
        /// Job type name.
        public string PSJobTypeName
                return _jobTypeName;
                _jobTypeName = value ?? this.GetType().ToString();
        #region results
        /// Result objects from this job. If this object is not a
        /// leaf node (with no children), then this will
        /// aggregate the results from all child jobs.
        internal PSDataCollection<PSStreamObject> Results
                return _results;
                    throw PSTraceSource.NewArgumentNullException("Results");
                    _resultsOwner = false;
                    _results = value;
        /// Indicates if a particular Job type uses the
        /// internal results collection.
        internal bool UsesResultsCollection { get; set; }
        /// Suppresses forwarding of job output into a cmdlet (like Receive-Job).
        /// This flag modifies functionality of <see cref="WriteObject"/> method, so that it doesnt add output-processing to <see cref="Results"/> collection.
        internal bool SuppressOutputForwarding { get; set; }
        internal virtual void WriteObject(object outputObject)
            PSObject pso = (outputObject == null) ? null : PSObject.AsPSObject(outputObject);
            this.Output.Add(pso);
            if (!SuppressOutputForwarding)
                this.Results.Add(new PSStreamObject(PSStreamObjectType.Output, pso));
        /// Allows propagating of terminating exceptions from remote "throw" statement
        /// (normally / by default all remote errors are transformed into non-terminating errors.
        internal bool PropagateThrows { get; set; }
        private void WriteError(Cmdlet cmdlet, ErrorRecord errorRecord)
            if (this.PropagateThrows)
                Exception e = GetExceptionFromErrorRecord(errorRecord);
        private static Exception GetExceptionFromErrorRecord(ErrorRecord errorRecord)
            if (errorRecord.Exception is not RuntimeException runtimeException)
            if (runtimeException is not RemoteException remoteException)
            PSPropertyInfo wasThrownFromThrow =
                remoteException.SerializedRemoteException.Properties["WasThrownFromThrowStatement"];
            if (wasThrownFromThrow == null || !((bool)wasThrownFromThrow.Value))
            runtimeException.WasThrownFromThrowStatement = true;
            return runtimeException;
        internal virtual void WriteError(ErrorRecord errorRecord)
            Error.Add(errorRecord);
            if (PropagateThrows)
                Exception exception = GetExceptionFromErrorRecord(errorRecord);
                    Results.Add(new PSStreamObject(PSStreamObjectType.Exception, exception));
            Results.Add(new PSStreamObject(PSStreamObjectType.Error, errorRecord));
        internal void WriteError(ErrorRecord errorRecord, out Exception exceptionThrownOnCmdletThread)
            this.InvokeCmdletMethodAndWaitForResults<object>(
                (Cmdlet cmdlet) =>
                    this.WriteError(cmdlet, errorRecord);
                out exceptionThrownOnCmdletThread);
        internal virtual void WriteWarning(string message)
            this.Warning.Add(new WarningRecord(message));
            this.Results.Add(new PSStreamObject(PSStreamObjectType.Warning, message));
        internal virtual void WriteVerbose(string message)
            this.Verbose.Add(new VerboseRecord(message));
            this.Results.Add(new PSStreamObject(PSStreamObjectType.Verbose, message));
        internal virtual void WriteDebug(string message)
            this.Debug.Add(new DebugRecord(message));
            this.Results.Add(new PSStreamObject(PSStreamObjectType.Debug, message));
        internal virtual void WriteProgress(ProgressRecord progressRecord)
            if ((progressRecord.ParentActivityId == (-1)) && (_parentActivityId != null))
                progressRecord = new ProgressRecord(progressRecord) { ParentActivityId = _parentActivityId.Value };
            Progress.Add(progressRecord);
            Results.Add(new PSStreamObject(PSStreamObjectType.Progress, progressRecord));
        internal virtual void WriteInformation(InformationRecord informationRecord)
            Information.Add(informationRecord);
            Results.Add(new PSStreamObject(PSStreamObjectType.Information, informationRecord));
        private Lazy<int> _parentActivityId;
        internal void SetParentActivityIdGetter(Func<int> parentActivityIdGetter)
            Dbg.Assert(parentActivityIdGetter != null, "Caller should verify parentActivityIdGetter != null");
            _parentActivityId = new Lazy<int>(parentActivityIdGetter);
        internal bool ShouldContinue(string query, string caption)
            return this.ShouldContinue(query, caption, out exceptionThrownOnCmdletThread);
        internal bool ShouldContinue(string query, string caption, out Exception exceptionThrownOnCmdletThread)
            bool methodResult = InvokeCmdletMethodAndWaitForResults(
                cmdlet => cmdlet.ShouldContinue(query, caption),
            return methodResult;
        internal virtual void NonblockingShouldProcess(
            InvokeCmdletMethodAndIgnoreResults(
                    ShouldProcessReason throwAwayProcessReason;
                    cmdlet.ShouldProcess(
                        out throwAwayProcessReason);
        internal virtual bool ShouldProcess(
            out ShouldProcessReason shouldProcessReason,
            out Exception exceptionThrownOnCmdletThread)
            ShouldProcessReason closureSafeShouldProcessReason = ShouldProcessReason.None;
                cmdlet => cmdlet.ShouldProcess(
                    out closureSafeShouldProcessReason),
            shouldProcessReason = closureSafeShouldProcessReason;
        private void InvokeCmdletMethodAndIgnoreResults(Action<Cmdlet> invokeCmdletMethod)
            object resultsLock = new object();
            CmdletMethodInvoker<object> methodInvoker = new CmdletMethodInvoker<object>
                Action = (Cmdlet cmdlet) => { invokeCmdletMethod(cmdlet); return null; },
                Finished = null,
                SyncObject = resultsLock
            Results.Add(new PSStreamObject(PSStreamObjectType.BlockingError, methodInvoker));
        private T InvokeCmdletMethodAndWaitForResults<T>(Func<Cmdlet, T> invokeCmdletMethodAndReturnResult, out Exception exceptionThrownOnCmdletThread)
            Dbg.Assert(invokeCmdletMethodAndReturnResult != null, "Caller should verify invokeCmdletMethodAndReturnResult != null");
            T methodResult = default(T);
            Exception closureSafeExceptionThrownOnCmdletThread = null;
            using (var gotResultEvent = new ManualResetEventSlim(false))
                EventHandler<JobStateEventArgs> stateChangedEventHandler =
                    (object sender, JobStateEventArgs eventArgs) =>
                        if (IsFinishedState(eventArgs.JobStateInfo.State) || eventArgs.JobStateInfo.State == JobState.Stopping)
                            lock (resultsLock)
                                closureSafeExceptionThrownOnCmdletThread = new OperationCanceledException();
                            gotResultEvent.Set();
                this.StateChanged += stateChangedEventHandler;
                Interlocked.MemoryBarrier();
                    stateChangedEventHandler(null, new JobStateEventArgs(this.JobStateInfo));
                    if (!gotResultEvent.IsSet)
                        this.SetJobState(JobState.Blocked);
                        // addition to results column happens here
                        CmdletMethodInvoker<T> methodInvoker = new CmdletMethodInvoker<T>
                            Action = invokeCmdletMethodAndReturnResult,
                            Finished = gotResultEvent,
                        PSStreamObjectType objectType = PSStreamObjectType.ShouldMethod;
                        if (typeof(T) == typeof(object))
                            objectType = PSStreamObjectType.BlockingError;
                        Results.Add(new PSStreamObject(objectType, methodInvoker));
                        gotResultEvent.Wait();
                            if (closureSafeExceptionThrownOnCmdletThread == null) // stateChangedEventHandler didn't set the results?  = ok to clobber results?
                                closureSafeExceptionThrownOnCmdletThread = methodInvoker.ExceptionThrownOnCmdletThread;
                                methodResult = methodInvoker.MethodResult;
                    this.StateChanged -= stateChangedEventHandler;
                exceptionThrownOnCmdletThread = closureSafeExceptionThrownOnCmdletThread;
        internal virtual void ForwardAvailableResultsToCmdlet(Cmdlet cmdlet)
            foreach (PSStreamObject obj in Results.ReadAll())
                obj.WriteStreamObject(cmdlet);
        internal virtual void ForwardAllResultsToCmdlet(Cmdlet cmdlet)
            foreach (PSStreamObject obj in this.Results)
        /// This method is introduce for delaying the loading of streams
        /// for a particular job.
        protected virtual void DoLoadJobStreams()
        /// Unloads job streams information. Enables jobs to
        /// clear stream information from memory.
        protected virtual void DoUnloadJobStreams()
        /// Load the required job streams.
        public void LoadJobStreams()
            if (_jobStreamsLoaded)
                _jobStreamsLoaded = true;
                DoLoadJobStreams();
                // third party call-out for platform API
                // Therefore it is fine to eat the exception
                // here
                using (PowerShellTraceSource tracer = PowerShellTraceSourceFactory.GetTraceSource())
                    tracer.TraceException(e);
        private bool _jobStreamsLoaded;
        /// Unload the required job streams.
        public void UnloadJobStreams()
            if (!_jobStreamsLoaded) return;
                _jobStreamsLoaded = false;
                DoUnloadJobStreams();
        /// Gets or sets the output buffer. Output of job is written
        /// into this buffer.
        public PSDataCollection<PSObject> Output
                LoadJobStreams(); // for delayed loading
                return _output;
                    throw PSTraceSource.NewArgumentNullException("Output");
                    _outputOwner = false;
                    _output = value;
        /// Gets or sets the error buffer. Errors of job are written
                    _errorOwner = false;
                    _error = value;
        /// Gets or sets the progress buffer. Progress of job is written
                return _progress;
                    _progressOwner = false;
                    _progress = value;
        /// Gets or sets the verbose buffer. Verbose output of job is written to
        /// this stream.
                return _verbose;
                    _verboseOwner = false;
                    _verbose = value;
        /// Gets or sets the debug buffer. Debug output of Job is written
        /// to this buffer.
                return _debug;
                    _debugOwner = false;
                    _debug = value;
        /// Gets or sets the warning buffer. Warnings of job are written to
        /// this buffer.
                return _warning;
                    _warningOwner = false;
                    _warning = value;
        /// Gets or sets the information buffer. Information records of job are written to
                return _information;
                    _informationOwner = false;
                    _information = value;
        public abstract string Location { get; }
        #endregion results
        #region Connect/Disconnect
        /// Returns boolean indicating whether the underlying
        /// transport for the job (or child jobs) supports
        /// connect/disconnect semantics.
        internal virtual bool CanDisconnect
        /// Returns runspaces associated with the Job, including
        /// child jobs.
        /// <returns>IEnumerable of RemoteRunspaces.</returns>
        internal virtual IEnumerable<RemoteRunspace> GetRunspaces()
        #endregion Job Properties
        #region Job State and State Change Event
        /// Event raised when state of the job changes.
        public event EventHandler<JobStateEventArgs> StateChanged;
        /// Sets Job State.
        /// New State of Job
        protected void SetJobState(JobState state)
            SetJobState(state, null);
        /// Reason associated with the state.
        internal void SetJobState(JobState state, Exception reason)
                bool alldone = false;
                JobStateInfo previousState = JobStateInfo;
                    JobStateInfo = new JobStateInfo(state, reason);
                    if (state == JobState.Running)
                        // BeginTime is set only once.
                        if (PSBeginTime == null)
                            PSBeginTime = DateTime.Now;
                    else if (IsFinishedState(state))
                        alldone = true;
                        // EndTime is set only once.
                        if (PSEndTime == null)
                            PSEndTime = DateTime.Now;
                if (alldone)
                    CloseAllStreams();
                    if (_processingOutput)
                            // Still marked as processing output.  Send final state changed.
                            HandleOutputProcessingStateChanged(this, new OutputProcessingStateEventArgs(false));
                // Exception raised in the eventhandler are not error in job.
                    tracer.WriteMessage("Job", "SetJobState", Guid.Empty, this, "Invoking StateChanged event", null);
                    StateChanged.SafeInvoke(this, new JobStateEventArgs(JobStateInfo.Clone(), previousState));
                    tracer.WriteMessage("Job", "SetJobState", Guid.Empty, this,
                                        "Some Job StateChange event handler threw an unhandled exception.", null);
                    tracer.TraceException(exception);
                // finished needs to be set after StateChanged event
                // has been raised
                        _finished?.Set();
        #endregion Job State and State Change Event
        #region Job Public Methods
        /// Stop this job object. If job contains child job, this should
        /// stop child job objects also.
        public abstract void StopJob();
        #endregion Job Public Methods
        /// Returns the items in results collection
        /// after clearing up all the internal
        /// <returns>Collection of stream objects.</returns>
        internal Collection<PSStreamObject> ReadAll()
            Output.Clear();
            Debug.Clear();
            Warning.Clear();
            Verbose.Clear();
            Progress.Clear();
            return Results.ReadAll();
        /// Helper function to check if job is finished.
        internal bool IsFinishedState(JobState state)
                return (state == JobState.Completed || state == JobState.Failed || state == JobState.Stopped);
        internal bool IsPersistentState(JobState state)
                return (IsFinishedState(state) || state == JobState.Disconnected || state == JobState.Suspended);
        /// Checks if the current instance can accept changes like
        /// changing one of the properties like Output, Error etc.
        private void AssertChangesAreAccepted()
                if (JobStateInfo.State == JobState.Running)
                    throw new InvalidJobStateException(JobState.Running);
        /// Automatically generate a job name if the user
        /// does not supply one.
        /// <returns>Auto generated job name.</returns>
        /// <remarks>Since the user can script/program against the
        /// job name, the auto generated name will not be
        /// localizable</remarks>
        protected string AutoGenerateJobName()
            return "Job" + Id.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
        internal void AssertNotDisposed()
                throw PSTraceSource.NewObjectDisposedException("PSJob");
        /// A helper method to close all the streams.
        internal void CloseAllStreams()
            // The Complete() method includes raising public notification events that third parties can
            // handle and potentially throw exceptions on the notification thread.  We don't want to
            // propagate those exceptions because it prevents this thread from completing its processing.
            if (_resultsOwner) { try { _results.Complete(); } catch (Exception e) { TraceException(e); } }
            if (_outputOwner) { try { _output.Complete(); } catch (Exception e) { TraceException(e); } }
            if (_errorOwner) { try { _error.Complete(); } catch (Exception e) { TraceException(e); } }
            if (_progressOwner) { try { _progress.Complete(); } catch (Exception e) { TraceException(e); } }
            if (_verboseOwner) { try { _verbose.Complete(); } catch (Exception e) { TraceException(e); } }
            if (_warningOwner) { try { _warning.Complete(); } catch (Exception e) { TraceException(e); } }
            if (_debugOwner) { try { _debug.Complete(); } catch (Exception e) { TraceException(e); } }
            if (_informationOwner) { try { _information.Complete(); } catch (Exception e) { TraceException(e); } }
        private static void TraceException(Exception e)
        /// Gets the job for the specified location.
        /// <param name="location">Location to filter on.</param>
        /// <returns>Collection of jobs.</returns>
        internal List<Job> GetJobsForLocation(string location)
            List<Job> returnJobList = new List<Job>();
                if (string.Equals(job.Location, location, StringComparison.OrdinalIgnoreCase))
                    returnJobList.Add(job);
            return returnJobList;
        #endregion Private/Internal Methods
        /// Dispose all managed resources. This will suppress finalizer on the object from getting called by
        /// calling System.GC.SuppressFinalize(this).
            // To prevent derived types with finalizers from having to re-implement System.IDisposable to call it,
            // unsealed types without finalizers should still call SuppressFinalize.
            System.GC.SuppressFinalize(this);
                    // release the WaitHandle
                            _finished.Dispose();
                            _finished = null;
                    // Only dispose the collections if we've created them...
                    if (_resultsOwner) _results.Dispose();
                    if (_outputOwner) _output.Dispose();
                    if (_errorOwner) _error.Dispose();
                    if (_debugOwner) _debug.Dispose();
                    if (_informationOwner) _information.Dispose();
                    if (_verboseOwner) _verbose.Dispose();
                    if (_warningOwner) _warning.Dispose();
                    if (_progressOwner) _progress.Dispose();
        #region MonitorOutputProcessing
        internal event EventHandler<OutputProcessingStateEventArgs> OutputProcessingStateChanged;
        private bool _processingOutput;
        /// MonitorOutputProcessing.
        internal bool MonitorOutputProcessing
        internal void SetMonitorOutputProcessing(IOutputProcessingState outputProcessingState)
            if (outputProcessingState != null)
                outputProcessingState.OutputProcessingStateChanged += HandleOutputProcessingStateChanged;
        internal void RemoveMonitorOutputProcessing(IOutputProcessingState outputProcessingState)
                outputProcessingState.OutputProcessingStateChanged -= HandleOutputProcessingStateChanged;
            _processingOutput = e.ProcessingOutput;
            OutputProcessingStateChanged.SafeInvoke<OutputProcessingStateEventArgs>(this, e);
    /// Top level job object for remoting. This contains multiple child job
    /// objects. Each child job object invokes command on one remote machine.
    /// Not removing the prefix "PS" as this signifies powershell specific remoting job
    internal class PSRemotingJob : Job
        /// Internal constructor for initializing PSRemotingJob using
        /// computer names.
        /// <param name="computerNames">names of computers for
        /// which the job object is being created</param>
        /// <param name="computerNameHelpers">list of helper objects
        /// corresponding to the computer names
        /// <param name="remoteCommand">remote command corresponding to this
        /// job object</param>
        /// <param name="name"> a friendly name for the job object
        internal PSRemotingJob(string[] computerNames,
                        List<IThrottleOperation> computerNameHelpers, string remoteCommand, string name)
            : this(computerNames, computerNameHelpers, remoteCommand, 0, name)
        /// Internal constructor for initializing job using
        /// PSSession objects.
        /// <param name="remoteRunspaceInfos">array of runspace info
        /// objects on which the remote command is executed</param>
        /// <param name="runspaceHelpers">List of helper objects for the
        /// runspaces</param>
        /// <param name="remoteCommand"> remote command corresponding to this
        /// <param name="name">a friendly name for the job object
        internal PSRemotingJob(PSSession[] remoteRunspaceInfos,
                        List<IThrottleOperation> runspaceHelpers, string remoteCommand, string name)
            : this(remoteRunspaceInfos, runspaceHelpers, remoteCommand, 0, name)
        /// which the result object is being created</param>
        /// result object</param>
        /// <param name="throttleLimit">Throttle limit to use.</param>
        /// <param name="name">A friendly name for the job object.</param>
                        List<IThrottleOperation> computerNameHelpers, string remoteCommand,
                            int throttleLimit, string name)
            : base(remoteCommand, name)
            // Create child jobs for each object in the list
            foreach (ExecutionCmdletHelperComputerName helper in computerNameHelpers)
                // Create Child Job and Register for its StateChanged Event
                PSRemotingChildJob childJob = new PSRemotingChildJob(remoteCommand,
                                            helper, _throttleManager);
                childJob.StateChanged += HandleChildJobStateChanged;
                childJob.JobUnblocked += HandleJobUnblocked;
                // Add this job to list of childjobs
            CommonInit(throttleLimit, computerNameHelpers);
                        List<IThrottleOperation> runspaceHelpers, string remoteCommand,
            for (int i = 0; i < remoteRunspaceInfos.Length; i++)
                ExecutionCmdletHelperRunspace helper = (ExecutionCmdletHelperRunspace)runspaceHelpers[i];
                // Create Child Job object and Register for its state changed event
                PSRemotingChildJob job = new PSRemotingChildJob(remoteCommand,
                job.StateChanged += HandleChildJobStateChanged;
                job.JobUnblocked += HandleJobUnblocked;
                // Add the child job to list of child jobs
            CommonInit(throttleLimit, runspaceHelpers);
        /// Creates a job object and child jobs for each disconnected pipeline/runspace
        /// provided in the list of ExecutionCmdletHelperRunspace items.  The runspace
        /// object must have a remote running command that can be connected to.
        /// Use Connect() method to transition to the connected state.
        /// <param name="helpers">List of DisconnectedJobOperation objects with disconnected pipelines.</param>
        /// <param name="throttleLimit">Throttle limit value.</param>
        /// <param name="name">Job name.</param>
        /// <param name="aggregateResults">Aggregate results.</param>
        internal PSRemotingJob(List<IThrottleOperation> helpers,
                               int throttleLimit, string name, bool aggregateResults)
            : base(string.Empty, name)
            // All pipeline objects must be in "disconnected" state and associated to running
            // remote commands.  Once the jobs are connected they can be stopped using the
            // ExecutionCmdletHelperRunspace object and ThrottleManager.
            foreach (ExecutionCmdletHelper helper in helpers)
                PSRemotingChildJob job = new PSRemotingChildJob(helper, _throttleManager, aggregateResults);
            // close all the streams.
            // Set status to "disconnected".
            SetJobState(JobState.Disconnected);
            // Submit the disconnected operation helpers to the throttle manager
            _throttleManager.SubmitOperations(helpers);
        protected PSRemotingJob() { }
        private void CommonInit(int throttleLimit, List<IThrottleOperation> helpers)
        #endregion Internal Constructors
        /// Get entity result for the specified computer.
        /// <param name="computerName">computername for which entity
        /// result is required</param>
        /// <returns>Entity result.</returns>
        internal List<Job> GetJobsForComputer(string computerName)
            foreach (Job j in ChildJobs)
                if (j is not PSRemotingChildJob child) continue;
                if (string.Equals(child.Runspace.ConnectionInfo.ComputerName, computerName,
                    returnJobList.Add(child);
        /// Get entity result for the specified runspace.
        /// <param name="runspace">runspace for which entity
        internal List<Job> GetJobsForRunspace(PSSession runspace)
                if (child.Runspace.InstanceId.Equals(runspace.InstanceId))
        /// Get entity result for the specified helper object.
        /// <param name="operation">helper for which entity
        internal List<Job> GetJobsForOperation(IThrottleOperation operation)
            ExecutionCmdletHelper helper = operation as ExecutionCmdletHelper;
                if (child.Helper.Equals(helper))
        #region Connection Support
        /// Connect all child jobs if they are in a disconnected state.
        internal void ConnectJobs()
            // Create connect operation objects for each child job object to connect.
            List<IThrottleOperation> connectJobOperations = new List<IThrottleOperation>();
            foreach (PSRemotingChildJob childJob in ChildJobs)
                if (childJob.JobStateInfo.State == JobState.Disconnected)
                    connectJobOperations.Add(new ConnectJobOperation(childJob));
            if (connectJobOperations.Count == 0)
            // Submit the connect job operation.
            // Return only after the connect operation completes.
            SubmitAndWaitForConnect(connectJobOperations);
        /// Connect a single child job associated with the provided runspace.
        /// <param name="runspaceInstanceId">Runspace instance id for child job.</param>
        internal void ConnectJob(Guid runspaceInstanceId)
            PSRemotingChildJob childJob = FindDisconnectedChildJob(runspaceInstanceId);
        private static void SubmitAndWaitForConnect(List<IThrottleOperation> connectJobOperations)
            using (ThrottleManager connectThrottleManager = new ThrottleManager())
                using (ManualResetEvent connectResult = new ManualResetEvent(false))
                    EventHandler<EventArgs> throttleCompleteEventHandler =
                        (object sender, EventArgs eventArgs) => connectResult.Set();
                    connectThrottleManager.ThrottleComplete += throttleCompleteEventHandler;
                        connectThrottleManager.ThrottleLimit = 0;
                        connectThrottleManager.SubmitOperations(connectJobOperations);
                        connectThrottleManager.EndSubmitOperations();
                        connectResult.WaitOne();
                        connectThrottleManager.ThrottleComplete -= throttleCompleteEventHandler;
        /// Simple throttle operation class for connecting jobs.
        private sealed class ConnectJobOperation : IThrottleOperation
            private readonly PSRemotingChildJob _psRemoteChildJob;
            internal ConnectJobOperation(PSRemotingChildJob job)
                _psRemoteChildJob = job;
                _psRemoteChildJob.StateChanged += ChildJobStateChangedHandler;
                bool startedSuccessfully = true;
                    _psRemoteChildJob.ConnectAsync();
                catch (InvalidJobStateException e)
                    startedSuccessfully = false;
                    string msg = StringUtil.Format(RemotingErrorIdStrings.JobConnectFailed, _psRemoteChildJob.Name);
                    Exception reason = new RuntimeException(msg, e);
                    ErrorRecord errorRecord = new ErrorRecord(reason, "PSJobConnectFailed", ErrorCategory.InvalidOperation, _psRemoteChildJob);
                    _psRemoteChildJob.WriteError(errorRecord);
                if (!startedSuccessfully)
                    RemoveEventCallback();
                    SendStartComplete();
                // Cannot stop a connect attempt.
                operationStateEventArgs.OperationState = OperationState.StopComplete;
            private void ChildJobStateChangedHandler(object sender, JobStateEventArgs eArgs)
                if (eArgs.JobStateInfo.State == JobState.Disconnected)
            private void SendStartComplete()
                operationStateEventArgs.OperationState = OperationState.StartComplete;
            private void RemoveEventCallback()
                _psRemoteChildJob.StateChanged -= ChildJobStateChangedHandler;
        /// Finds the disconnected child job associated with this runspace and returns
        /// the PowerShell object that is remotely executing the command.
        /// <param name="runspaceInstanceId">Runspace instance Id.</param>
        /// <returns>Associated PowerShell object.</returns>
        internal PowerShell GetAssociatedPowerShellObject(Guid runspaceInstanceId)
                ps = childJob.GetPowerShell();
            return ps;
        /// Helper method to find a disconnected child job associated with
        /// a given runspace.
        /// <param name="runspaceInstanceId">Runspace Id.</param>
        /// <returns>PSRemotingChildJob object.</returns>
        private PSRemotingChildJob FindDisconnectedChildJob(Guid runspaceInstanceId)
            PSRemotingChildJob rtnJob = null;
            foreach (PSRemotingChildJob childJob in this.ChildJobs)
                if ((childJob.Runspace.InstanceId.Equals(runspaceInstanceId)) &&
                    (childJob.JobStateInfo.State == JobState.Disconnected))
                    rtnJob = childJob;
            return rtnJob;
        /// Internal method to stop a job without first connecting it if it is in
        /// a disconnected state.  This supports Receive-PSSession where it abandons
        /// a currently running/disconnected job when user selects -OutTarget Host.
        internal void InternalStopJob()
            if (_isDisposed || _stopIsCalled || IsFinishedState(JobStateInfo.State))
        private bool _moreData = true;
                // moreData is initially set to true, and it
                // will remain so until the async result
                // object has completed execution.
                if (_moreData && IsFinishedState(JobStateInfo.State))
            // If the job is in a disconnected state then try to connect it
            // so that it can be stopped on the server.
            if (JobStateInfo.State == JobState.Disconnected)
                bool ConnectSuccessful;
                    ConnectJobs();
                    ConnectSuccessful = true;
                    ConnectSuccessful = false;
                if (!ConnectSuccessful && this.Error.IsOpen)
                    string msg = StringUtil.Format(RemotingErrorIdStrings.StopJobNotConnected, this.Name);
                    Exception reason = new RuntimeException(msg);
                    ErrorRecord errorRecord = new ErrorRecord(reason, "StopJobCannotConnectToServer",
            InternalStopJob();
        /// Used by Invoke-Command cmdlet to show/hide computername property value.
        /// Format and Output has capability to understand RemoteObjects and this property lets
        /// Format and Output decide whether to show/hide computername.
        internal bool HideComputerName
                return _hideComputerName;
                _hideComputerName = value;
                foreach (Job job in this.ChildJobs)
                    PSRemotingChildJob rJob = job as PSRemotingChildJob;
                    if (rJob != null)
                        rJob.HideComputerName = value;
        private bool _hideComputerName = true;
            //        bool localErrors = false; // if local errors are present
            //        bool setFinished = false;    // if finished needs to be set
            //        statusMessage = "OK";
            //        lock (syncObject)
            //            if (finishedCount == ChildJobs.Count)
            //                // ISSUE: Change this code to look in to child jobs for exception
            //                if (errors.Count > 0)
            //                    statusMessage = "LocalErrors";
            //                    localErrors = true;
            //                // check for status of remote command
            //                for (int i = 0; i < ChildJobs.Count; i++)
            //                    PSRemotingChildJob childJob = ChildJobs[i] as PSRemotingChildJob;
            //                    if (childJob == null) continue;
            //                    if (childJob.ContainsErrors)
            //                        if (localErrors)
            //                        {
            //                            statusMessage = "LocalAndRemoteErrors";
            //                        }
            //                        else
            //                            statusMessage = "RemoteErrors";
            //                        break;
            //                setFinished = true;
        #region finish logic
        // This variable is set to true if at least one child job failed.
        // count of number of child jobs which have finished
        // count of number of child jobs which are blocked
        // count of child jobs that are in disconnected state.
        private int _disconnectedChildJobsCount = 0;
        // count of child jobs that are in Debug halted state.
        private int _debugChildJobsCount = 0;
            // Update object state to reflect disconnect state related changes in child jobs.
            CheckDisconnectedAndUpdateState(e.JobStateInfo.State, e.PreviousJobStateInfo.State);
            // Handle transition of child job to Debug halt state.
            if (e.JobStateInfo.State == JobState.AtBreakpoint)
                lock (_syncObject) { _debugChildJobsCount++; }
                // If any child jobs are Debug halted, we set state to Debug.
                SetJobState(JobState.AtBreakpoint);
            // Handle transition of child job back to running state.
            if ((e.JobStateInfo.State == JobState.Running) &&
                (e.PreviousJobStateInfo.State == JobState.AtBreakpoint))
                int totalDebugCount;
                lock (_syncObject) { totalDebugCount = --_debugChildJobsCount; }
                if (totalDebugCount == 0)
            if (!IsFinishedState(e.JobStateInfo.State))
                if (_finishedChildJobsCount + _disconnectedChildJobsCount
                    == ChildJobs.Count)
                if (_disconnectedChildJobsCount > 0)
                else if (_atleastOneChildJobFailed)
                else if (_stopIsCalled)
        /// Updates the parent job state based on state of all child jobs.
        /// <param name="newState">New child job state.</param>
        /// <param name="prevState">Previous child job state.</param>
        private void CheckDisconnectedAndUpdateState(JobState newState, JobState prevState)
            // Do all logic inside a lock to ensure it is atomic against
            // multiple job thread state changes.
                if (newState == JobState.Disconnected)
                    ++(_disconnectedChildJobsCount);
                    // If previous state was Blocked then we need to decrement the count
                    // since it is now Disconnected.
                    if (prevState == JobState.Blocked)
                        --_blockedChildJobsCount;
                    // If all unfinished and unblocked child jobs are disconnected then this
                    // parent job becomes disconnected.
                    if ((_disconnectedChildJobsCount +
                         _finishedChildJobsCount +
                         _blockedChildJobsCount) == ChildJobs.Count)
                        SetJobState(JobState.Disconnected, null);
                    if (prevState == JobState.Disconnected)
                        --(_disconnectedChildJobsCount);
                    if ((newState == JobState.Running) && (JobStateInfo.State == JobState.Disconnected))
                        // Note that SetJobState() takes a lock so it is unnecessary to do
                        // this under a lock here.
        #endregion finish logic
            if (ChildJobs.Count > 0)
                foreach (PSRemotingChildJob job in ChildJobs)
        internal override bool CanDisconnect
                // If one child job can disconnect then all of them can since
                // all child jobs use the same remote runspace transport.
                return (ChildJobs.Count > 0) && ChildJobs[0].CanDisconnect;
        internal override IEnumerable<RemoteRunspace> GetRunspaces()
                runspaces.Add(job.Runspace as RemoteRunspace);
        private readonly ThrottleManager _throttleManager = new ThrottleManager();
        private readonly object _syncObject = new object();           // sync object
    #region DisconnectedJobOperation class
    /// Simple throttle operation class for PSRemoting jobs created in the
    /// disconnected state.
    internal class DisconnectedJobOperation : ExecutionCmdletHelper
        internal DisconnectedJobOperation(Pipeline pipeline)
            this.pipeline = pipeline;
            this.pipeline.StateChanged += HandlePipelineStateChanged;
            // This is a no-op since disconnected jobs (pipelines) have
            // already been started.
            if (pipeline.PipelineStateInfo.State == PipelineState.Running ||
                pipeline.PipelineStateInfo.State == PipelineState.Disconnected ||
                pipeline.PipelineStateInfo.State == PipelineState.NotStarted)
                // If the pipeline state has reached Complete/Failed/Stopped
                // by the time control reaches here, then this operation
                // becomes a no-op. However, an OperationComplete would have
                // already been raised from the handler.
                // Will have to raise OperationComplete from here,
                // else ThrottleManager will have
                SendStopComplete();
        private void HandlePipelineStateChanged(object sender, PipelineStateEventArgs stateEventArgs)
            PipelineStateInfo stateInfo = stateEventArgs.PipelineStateInfo;
            SendStopComplete(stateEventArgs);
        private void SendStopComplete(EventArgs eventArgs = null)
            operationStateEventArgs.BaseEvent = eventArgs;
    /// Class for RemotingChildJob object. This job object invokes command
    /// on one remote machine.
    /// TODO: I am not sure whether to change this internal to just RemotingChildJob.
    internal class PSRemotingChildJob : Job, IJobDebugger
        /// Creates an instance of PSRemotingChildJob.
        /// <param name="remoteCommand">Command invoked by this job object.</param>
        /// <param name="helper"></param>
        /// <param name="throttleManager"></param>
        internal PSRemotingChildJob(string remoteCommand, ExecutionCmdletHelper helper, ThrottleManager throttleManager)
            : base(remoteCommand)
            Dbg.Assert(helper.Pipeline is RemotePipeline, "Pipeline passed should be a remote pipeline");
            Helper = helper;
            Runspace = helper.Pipeline.Runspace;
            _remotePipeline = helper.Pipeline as RemotePipeline;
            RemoteRunspace remoteRS = Runspace as RemoteRunspace;
            if ((remoteRS != null) && (remoteRS.RunspaceStateInfo.State == RunspaceState.BeforeOpen))
                remoteRS.URIRedirectionReported += HandleURIDirectionReported;
            AggregateResultsFromHelper(helper);
            Runspace.AvailabilityChanged += HandleRunspaceAvailabilityChanged;
            RegisterThrottleComplete(throttleManager);
        /// Constructs a disconnected child job that is able to connect to a remote
        /// runspace/command on a server.  The ExecutionCmdletHelperRunspace must
        /// contain a remote pipeline object in a disconnected state.  In addition
        /// the pipeline runspace must be associated with a valid running remote
        /// command that can be connected to.
        /// <param name="helper">ExecutionCmdletHelper object containing runspace and pipeline objects.</param>
        /// <param name="throttleManager">ThrottleManger object.</param>
        internal PSRemotingChildJob(ExecutionCmdletHelper helper, ThrottleManager throttleManager, bool aggregateResults = false)
            Dbg.Assert((helper.Pipeline is RemotePipeline), "Helper pipeline object should be a remote pipeline");
            Dbg.Assert((helper.Pipeline.PipelineStateInfo.State == PipelineState.Disconnected), "Remote pipeline object must be in Disconnected state.");
            if (aggregateResults)
                _remotePipeline.StateChanged += HandlePipelineStateChanged;
                _remotePipeline.Output.DataReady += HandleOutputReady;
                _remotePipeline.Error.DataReady += HandleErrorReady;
            IThrottleOperation operation = helper as IThrottleOperation;
            operation.OperationComplete += HandleOperationComplete;
        protected PSRemotingChildJob()
        #endregion Internal Constructor
        /// Connects the remote pipeline that this job represents.
        internal void ConnectAsync()
            if (JobStateInfo.State != JobState.Disconnected)
                throw new InvalidJobStateException(JobStateInfo.State);
            _remotePipeline.ConnectAsync();
        // bool isStopCalled = false;
            _throttleManager.StopOperation(Helper);
        /// Status Message associated with the Job.
                // ISSUE implement this.
                return (Runspace != null) ? Runspace.ConnectionInfo.ComputerName : string.Empty;
        /// Helper associated with this entity.
        internal ExecutionCmdletHelper Helper { get; } = null;
        /// Property that indicates this disconnected child job was
        /// previously in the Blocked state.
        internal bool DisconnectedAndBlocked { get; private set; } = false;
                return remoteRS != null && remoteRS.CanDisconnect;
                if (_jobDebugger == null)
                    lock (this.SyncObject)
                        if ((_jobDebugger == null) &&
                            (Runspace.Debugger != null))
                            _jobDebugger = new RemotingJobDebugger(Runspace.Debugger, Runspace, this.Name);
                return _jobDebugger;
        /// True if job is synchronous and can be debugged.
        public bool IsAsync
            get { return _isAsync; }
            set { _isAsync = true; }
        /// Handler which will handle output ready events of the
        /// pipeline. The output objects are queued on to the
        /// internal stream.
        /// <param name="sender">the pipeline reader which raised
        /// this event</param>
        /// <param name="eventArgs">Information describing the ready event.</param>
        private void HandleOutputReady(object sender, EventArgs eventArgs)
            PSDataCollectionPipelineReader<PSObject, PSObject> reader =
                    sender as PSDataCollectionPipelineReader<PSObject, PSObject>;
            Collection<PSObject> output = reader.NonBlockingRead();
            foreach (PSObject dataObject in output)
                // attach origin information only if it doesn't exist
                // in case of a second-hop scenario, the origin information
                // will already be added at the second hop machine
                if (dataObject != null)
                    // if the server has already added some properties, which we do not
                    // want to trust, we simply replace them with the server's
                    // identity we know of
                    if (dataObject.Properties[RemotingConstants.ComputerNameNoteProperty] != null)
                        dataObject.Properties.Remove(RemotingConstants.ComputerNameNoteProperty);
                    if (dataObject.Properties[RemotingConstants.RunspaceIdNoteProperty] != null)
                        dataObject.Properties.Remove(RemotingConstants.RunspaceIdNoteProperty);
                    dataObject.Properties.Add(new PSNoteProperty(RemotingConstants.ComputerNameNoteProperty, reader.ComputerName));
                    dataObject.Properties.Add(new PSNoteProperty(RemotingConstants.RunspaceIdNoteProperty, reader.RunspaceId));
                    // PSShowComputerName is present for all the objects (from remoting)..this is to allow PSComputerName to be selected.
                    // Ex: Invoke-Command localhost,blah { gps } | select PSComputerName should work.
                    if (dataObject.Properties[RemotingConstants.ShowComputerNameNoteProperty] == null)
                        PSNoteProperty showComputerNameNP = new PSNoteProperty(RemotingConstants.ShowComputerNameNoteProperty, !_hideComputerName);
                        dataObject.Properties.Add(showComputerNameNP);
                this.WriteObject(dataObject);
        /// Handler which will handle error ready events of the
        /// pipeline. The error records are queued on to the
        private void HandleErrorReady(object sender, EventArgs eventArgs)
            PSDataCollectionPipelineReader<ErrorRecord, object> reader =
                sender as PSDataCollectionPipelineReader<ErrorRecord, object>;
            Collection<object> error = reader.NonBlockingRead();
            foreach (object errorData in error)
                ErrorRecord er = errorData as ErrorRecord;
                if (er != null)
                    OriginInfo originInfo = new OriginInfo(reader.ComputerName, reader.RunspaceId);
                    RemotingErrorRecord errorRecord =
                        new RemotingErrorRecord(er, originInfo);
                    // ISSUE: Add an Assert for ErrorRecord.
                    // Add to the PSRemotingChild jobs streams
        /// When the client remote session reports a URI redirection, this method will report the
        /// message to the user as a Warning using Host method calls.
        protected void HandleURIDirectionReported(object sender, RemoteDataEventArgs<Uri> eventArgs)
            string message = StringUtil.Format(RemotingErrorIdStrings.URIRedirectWarningToHost, eventArgs.Data.OriginalString);
        /// Handle method executor stream events.
        /// <param name="eventArgs">The event args.</param>
        private void HandleHostCalls(object sender, EventArgs eventArgs)
            ObjectStream hostCallsStream = sender as ObjectStream;
            if (hostCallsStream != null)
                Collection<object> hostCallMethodExecutors =
                    hostCallsStream.NonBlockingRead(hostCallsStream.Count);
                    foreach (ClientMethodExecutor hostCallMethodExecutor in hostCallMethodExecutors)
                        Results.Add(new PSStreamObject(PSStreamObjectType.MethodExecutor, hostCallMethodExecutor));
                        // if the call id of the underlying remote host call is not ServerDispatchTable.VoidCallId
                        // then the call is waiting on user input. Change state to Blocked
                        if (hostCallMethodExecutor.RemoteHostCall.CallId != ServerDispatchTable.VoidCallId)
        /// Handle changes in pipeline states.
        protected virtual void HandlePipelineStateChanged(object sender, PipelineStateEventArgs e)
            if ((Runspace != null) && (e.PipelineStateInfo.State != PipelineState.Running))
                // since we got state changed event..we dont need to listen on
                // URI redirections anymore
                ((RemoteRunspace)Runspace).URIRedirectionReported -= HandleURIDirectionReported;
            PipelineState state = e.PipelineStateInfo.State;
                    if (DisconnectedAndBlocked)
                        DisconnectedAndBlocked = false;
                        SetJobState(JobState.Blocked);
                    DisconnectedAndBlocked = (JobStateInfo.State == JobState.Blocked);
            // Question: Why is the DoFinish() call on terminal pipeline state deleted
            // Answer: Because in the runspace case, when pipeline reaches a terminal state
            // OperationComplete will be raised and DoFinish() is called on OperationComplete
            // In the computer name case, once pipeline reaches a terminal state, runspace is
            // closed which will result in an OperationComplete event
            // Question: Why do we register for HandleThrottleComplete when we have already
            // registered for PipelineStateChangedEvent?
            // Answer: Because ThrottleManager at a given time can have some pipelines which are
            // still not started. If TM.Stop() is called, then it simply discards those pipelines and
            // PipelineStateChangedEvent is not called for them. For such jobs, we depend on
            // HandleThrottleComplete to mark the finish of job.
            // Question: So it is possible in some cases DoFinish can be called twice.
            // Answer: Yes: One from PipelineStateChangedEvent and Another here. But
            // DoFinish has logic to check if it has been already called and second call
            // becomes noOp.
            DoFinish();
        /// Handle the operation complete event.
        /// <param name="stateEventArgs">Operation complete event args.</param>
        protected virtual void HandleOperationComplete(object sender, OperationStateEventArgs stateEventArgs)
            // Question:Why are we registering for OperationComplete if we already
            // registering for StateChangedEvent and ThrottleComplete event
            // Answer:Because in case of computer, if Runspace.Open it self fails,
            // no pipeline is created and no pipeline state changed event is raised.
            // We can wait for throttle complete, but it is raised only when all the
            // operations are completed and this means that status of job is not updated
            // until Operation Complete.
            ExecutionCmdletHelper helper = sender as ExecutionCmdletHelper;
            Dbg.Assert(helper != null, "Sender of OperationComplete has to be ExecutionCmdletHelper");
            DeterminedAndSetJobState(helper);
        private bool _doFinishCalled = false;
        /// This method marks the completion state for Job. Also if job failed, it processes the
        /// reason of failure.
        protected virtual void DoFinish()
            if (_doFinishCalled)
                _doFinishCalled = true;
            DeterminedAndSetJobState(Helper);
            DoCleanupOnFinished();
        /// This is the pretty formated error record associated with the reason of failure.
        private ErrorRecord _failureErrorRecord;
        /// This is set if Job state is Failed and Reason has a exception.
        internal ErrorRecord FailureErrorRecord
                return _failureErrorRecord;
        /// Process the exceptions to decide reason for job failure.
        /// <param name="failureException"></param>
        /// <param name="failureErrorRecord"></param>
        protected void ProcessJobFailure(ExecutionCmdletHelper helper, out Exception failureException,
                            out ErrorRecord failureErrorRecord)
            //      There are three errors possible
            //      1. The remote runspace is in (or went into) a
            //         broken state. This information is available
            //         in the runspace state information
            //      2. The remote pipeline failed because of an
            //         exception. This information is available
            //         in the pipeline state information
            //      3. Runspace.OpenAsync or Pipeline.InvokeAsync threw exception
            //         They are in Helper.InternalException
            Dbg.Assert(helper != null, "helper is null");
            RemotePipeline pipeline = helper.Pipeline as RemotePipeline;
            Dbg.Assert(pipeline != null, "pipeline is null");
            RemoteRunspace runspace = pipeline.GetRunspace() as RemoteRunspace;
            Dbg.Assert(runspace != null, "runspace is null");
            failureException = null;
            failureErrorRecord = null;
            if (helper.InternalException != null)
                string errorId = "RemotePipelineExecutionFailed";
                failureException = helper.InternalException;
                if ((failureException is InvalidRunspaceStateException) || (failureException is InvalidRunspacePoolStateException))
                    errorId = "InvalidSessionState";
                    if (!string.IsNullOrEmpty(failureException.Source))
                        errorId = string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{errorId},{failureException.Source}");
                failureErrorRecord = new ErrorRecord(helper.InternalException,
                       errorId, ErrorCategory.OperationStopped,
                            helper);
            // there is a failure reason available in the runspace
            else if ((runspace.RunspaceStateInfo.State == RunspaceState.Broken) ||
                     (runspace.RunspaceStateInfo.Reason != null))
                failureException = runspace.RunspaceStateInfo.Reason;
                object targetObject = runspace.ConnectionInfo.ComputerName;
                string errorDetails = null;
                // set the transport message in the error detail so that
                // the user can directly get to see the message without
                // having to mine through the error record details
                PSRemotingTransportException transException =
                            failureException as PSRemotingTransportException;
                string fullyQualifiedErrorId =
                    System.Management.Automation.Remoting.Client.WSManTransportManagerUtils.GetFQEIDFromTransportError(
                        (transException != null) ? transException.ErrorCode : 0,
                        "PSSessionStateBroken");
                if (transException != null)
                    errorDetails = "[" + runspace.ConnectionInfo.ComputerName + "] ";
                    if (transException.ErrorCode ==
                        Remoting.Client.WSManNativeApi.ERROR_WSMAN_REDIRECT_REQUESTED)
                        // Handling a special case for redirection..we should talk about
                        // AllowRedirection parameter and WSManMaxRedirectionCount preference
                        // variables
                        string message = PSRemotingErrorInvariants.FormatResourceString(
                            RemotingErrorIdStrings.URIRedirectionReported,
                            transException.Message,
                            "MaximumConnectionRedirectionCount",
                            "AllowRedirection");
                        errorDetails += message;
                    else if (!string.IsNullOrEmpty(transException.Message))
                        errorDetails += transException.Message;
                    else if (!string.IsNullOrEmpty(transException.TransportMessage))
                        errorDetails += transException.TransportMessage;
                failureException ??= new RuntimeException(
                    PSRemotingErrorInvariants.FormatResourceString(
                        RemotingErrorIdStrings.RemoteRunspaceOpenUnknownState,
                        runspace.RunspaceStateInfo.State));
                failureErrorRecord = new ErrorRecord(failureException, targetObject,
                                fullyQualifiedErrorId, ErrorCategory.OpenError,
                                null, null, null, null, null, errorDetails, null);
            else if (pipeline.PipelineStateInfo.State == PipelineState.Failed
                || (pipeline.PipelineStateInfo.State == PipelineState.Stopped
                    && pipeline.PipelineStateInfo.Reason != null
                    && pipeline.PipelineStateInfo.Reason is not PipelineStoppedException))
                // Pipeline stopped state is also an error condition if the associated exception is not 'PipelineStoppedException'.
                failureException = pipeline.PipelineStateInfo.Reason;
                if (failureException != null)
                    RemoteException rException = failureException as RemoteException;
                    if (rException != null)
                        errorRecord = rException.ErrorRecord;
                        // A RemoteException will hide a PipelineStoppedException, which should be ignored.
                        if (errorRecord != null &&
                            errorRecord.FullyQualifiedErrorId.Equals("PipelineStopped", StringComparison.OrdinalIgnoreCase))
                            // PipelineStoppedException should not be reported as error.
                        // at this point, there may be no failure reason available in
                        // the runspace because the remoting protocol
                        // layer may not have yet assigned it to the runspace
                        // in such a case, the remoting protocol layer would have
                        // assigned an exception in the client end to the pipeline
                        // create an error record from it and write it out
                        errorRecord = new ErrorRecord(pipeline.PipelineStateInfo.Reason,
                                                        "JobFailure", ErrorCategory.OperationStopped,
                    string computerName = ((RemoteRunspace)pipeline.GetRunspace()).ConnectionInfo.ComputerName;
                    Guid runspaceId = pipeline.GetRunspace().InstanceId;
                    OriginInfo originInfo = new OriginInfo(computerName, runspaceId);
                    failureErrorRecord = new RemotingErrorRecord(errorRecord, originInfo);
        private bool _cleanupDone = false;
        /// Cleanup after state changes to finished.
        protected virtual void DoCleanupOnFinished()
            bool doCleanup = false;
            if (!_cleanupDone)
                        _cleanupDone = true;
                        doCleanup = true;
            if (!doCleanup) return;
            StopAggregateResultsFromHelper(Helper);
            Runspace.AvailabilityChanged -= HandleRunspaceAvailabilityChanged;
            IThrottleOperation operation = Helper as IThrottleOperation;
            operation.OperationComplete -= HandleOperationComplete;
            UnregisterThrottleComplete(_throttleManager);
            _throttleManager = null;
        /// Aggregates results from the pipeline associated
        /// with the specified helper.
        /// <param name="helper">helper whose pipeline results
        /// need to be aggregated</param>
        protected void AggregateResultsFromHelper(ExecutionCmdletHelper helper)
            // Get the pipeline associated with this helper and register for appropriate events
            Pipeline pipeline = helper.Pipeline;
            pipeline.Output.DataReady += HandleOutputReady;
            pipeline.Error.DataReady += HandleErrorReady;
            pipeline.StateChanged += HandlePipelineStateChanged;
            // Register handler for method executor object stream.
            Dbg.Assert(pipeline is RemotePipeline, "pipeline is RemotePipeline");
            RemotePipeline remotePipeline = pipeline as RemotePipeline;
            remotePipeline.MethodExecutorStream.DataReady += HandleHostCalls;
            remotePipeline.PowerShell.Streams.Progress.DataAdded += HandleProgressAdded;
            remotePipeline.PowerShell.Streams.Warning.DataAdded += HandleWarningAdded;
            remotePipeline.PowerShell.Streams.Verbose.DataAdded += HandleVerboseAdded;
            remotePipeline.PowerShell.Streams.Debug.DataAdded += HandleDebugAdded;
            remotePipeline.PowerShell.Streams.Information.DataAdded += HandleInformationAdded;
            // Enable method executor stream so that host methods are queued up
            // on it instead of being executed asynchronously when they arrive.
            remotePipeline.IsMethodExecutorStreamEnabled = true;
        /// If the pipeline is not null, returns the pipeline's PowerShell
        /// If it is null, then returns the PowerShell with the specified
        /// instance Id.
        /// <param name="pipeline">Remote pipeline.</param>
        /// <param name="instanceId">Instance as described in event args.</param>
        /// <returns>PowerShell instance.</returns>
        private PowerShell GetPipelinePowerShell(RemotePipeline pipeline, Guid instanceId)
                return pipeline.PowerShell;
            return GetPowerShell(instanceId);
        /// When a debug message is raised in the underlying PowerShell
        /// add it to the jobs debug stream.
        /// <param name="sender">Unused.</param>
        private void HandleDebugAdded(object sender, DataAddedEventArgs eventArgs)
            int index = eventArgs.Index;
            PowerShell powershell = GetPipelinePowerShell(_remotePipeline, eventArgs.PowerShellInstanceId);
            if (powershell != null)
                this.Debug.Add(powershell.Streams.Debug[index]);
        /// When a verbose message is raised in the underlying PowerShell
        /// add it to the jobs verbose stream.
        private void HandleVerboseAdded(object sender, DataAddedEventArgs eventArgs)
                this.Verbose.Add(powershell.Streams.Verbose[index]);
        /// When a warning message is raised in the underlying PowerShell
        /// add it to the jobs warning stream.
        private void HandleWarningAdded(object sender, DataAddedEventArgs eventArgs)
                WarningRecord warningRecord = powershell.Streams.Warning[index];
                this.Warning.Add(warningRecord);
                this.Results.Add(new PSStreamObject(PSStreamObjectType.WarningRecord, warningRecord));
        /// When a progress message is raised in the underlying PowerShell
        /// add it to the jobs progress tream.
        private void HandleProgressAdded(object sender, DataAddedEventArgs eventArgs)
                this.Progress.Add(powershell.Streams.Progress[index]);
        /// When a Information message is raised in the underlying PowerShell
        /// add it to the jobs Information stream.
        private void HandleInformationAdded(object sender, DataAddedEventArgs eventArgs)
                InformationRecord informationRecord = powershell.Streams.Information[index];
                this.Information.Add(informationRecord);
                // Host output is handled by the hosting APIs directly, so we need to add a tag that it was
                // forwarded so that it is not written twice.
                //  For all other Information records, forward them.
                if (informationRecord.Tags.Contains("PSHOST"))
                    informationRecord.Tags.Add("FORWARDED");
                this.Results.Add(new PSStreamObject(PSStreamObjectType.Information, informationRecord));
        /// Stops collecting results from the pipeline associated with
        /// the specified helper.
        /// <param name="helper">helper class whose pipeline results
        /// aggregation has to be stopped</param>
        protected void StopAggregateResultsFromHelper(ExecutionCmdletHelper helper)
            RemoveAggreateCallbacksFromHelper(helper);
            pipeline.Dispose();
            pipeline = null;
        /// Removes aggregate callbacks from pipeline so that a new job object can
        /// be created and can add its own callbacks.
        /// This is to support Invoke-Command auto-disconnect where a new PSRemoting
        /// job must be created to pass back to user for connection.
        /// <param name="helper">Helper class.</param>
        protected void RemoveAggreateCallbacksFromHelper(ExecutionCmdletHelper helper)
            // Remove old data output callbacks from pipeline so new callbacks can be added.
            pipeline.Output.DataReady -= HandleOutputReady;
            pipeline.Error.DataReady -= HandleErrorReady;
            pipeline.StateChanged -= HandlePipelineStateChanged;
            // Remove old data aggregation and host calls.
            remotePipeline.MethodExecutorStream.DataReady -= HandleHostCalls;
            if (remotePipeline.PowerShell != null)
                remotePipeline.PowerShell.Streams.Progress.DataAdded -= HandleProgressAdded;
                remotePipeline.PowerShell.Streams.Warning.DataAdded -= HandleWarningAdded;
                remotePipeline.PowerShell.Streams.Verbose.DataAdded -= HandleVerboseAdded;
                remotePipeline.PowerShell.Streams.Debug.DataAdded -= HandleDebugAdded;
                remotePipeline.PowerShell.Streams.Information.DataAdded -= HandleInformationAdded;
                remotePipeline.IsMethodExecutorStreamEnabled = false;
        /// Register for throttle complete from the specified
        /// throttlemanager.
        protected void RegisterThrottleComplete(ThrottleManager throttleManager)
            throttleManager.ThrottleComplete += HandleThrottleComplete;
        /// Unregister for throttle complete from the specified
        /// throttle manager.
        protected void UnregisterThrottleComplete(ThrottleManager throttleManager)
            throttleManager.ThrottleComplete -= HandleThrottleComplete;
        /// Determine the current state of the job based on the underlying
        /// pipeline state and set the state accordingly.
        protected void DeterminedAndSetJobState(ExecutionCmdletHelper helper)
            Exception failureException;
            // Process the reason in case of failure.
            ProcessJobFailure(helper, out failureException, out _failureErrorRecord);
                SetJobState(JobState.Failed, failureException);
                // Get the state of the pipeline
                PipelineState state = helper.Pipeline.PipelineStateInfo.State;
                if (state == PipelineState.NotStarted)
                    // This is a case in which pipeline was not started and TM.Stop was
                    // called. See comment in HandleThrottleComplete
                else if (state == PipelineState.Completed)
            Dbg.Assert(JobStateInfo.State == JobState.Blocked,
                "Current state of job must be blocked before it can be unblocked");
            Dbg.Assert(JobUnblocked != null, "Parent job must register for JobUnblocked event from all child jobs");
        /// Returns the PowerShell for the specified instance id.
        /// <param name="instanceId">Instance id of powershell.</param>
        /// <returns>Powershell instance.</returns>
        internal virtual PowerShell GetPowerShell(Guid instanceId)
            // this should be called only in the derived implementation
        /// Returns the PowerShell object associated with this remote child job.
        /// <returns>PowerShell object.</returns>
        internal PowerShell GetPowerShell()
            if (_remotePipeline != null)
                ps = _remotePipeline.PowerShell;
        /// Monitor runspace availability and if it goes to RemoteDebug then set
        /// job state to Debug.  Set back to Running when availability goes back to
        /// Busy (indicating the script/command is running again).
        /// <param name="e">RunspaceAvailabilityEventArgs.</param>
            RunspaceAvailability prevAvailability = _prevRunspaceAvailability;
            _prevRunspaceAvailability = e.RunspaceAvailability;
            if (e.RunspaceAvailability == RunspaceAvailability.RemoteDebug)
            else if ((prevAvailability == RunspaceAvailability.RemoteDebug) &&
                     (e.RunspaceAvailability == RunspaceAvailability.Busy))
        // helper associated with this job object
        private readonly RemotePipeline _remotePipeline = null;
        // object used for synchronization
        protected object SyncObject = new object();
        private volatile Debugger _jobDebugger;
        private bool _isAsync = true;
        private RunspaceAvailability _prevRunspaceAvailability = RunspaceAvailability.None;
    /// This is a debugger wrapper class used to allow debugging of
    /// remoting jobs that implement the IJobDebugger interface.
    internal sealed class RemotingJobDebugger : Debugger
        private readonly Runspace _runspace;
        private RemotingJobDebugger() { }
        /// <param name="debugger">Debugger to wrap.</param>
        /// <param name="runspace">Remote runspace.</param>
        /// <param name="jobName">Name of associated job.</param>
        public RemotingJobDebugger(
        /// <param name="breakpoints">Breakpoints to set.</param>
        /// <returns>A breakpoint with the specified id.</returns>
        /// CheckStateAndRaiseStopEvent.
            remoteDebugger?.CheckStateAndRaiseStopEvent();
            get { return _wrappedDebugger.InBreakpoint; }
            Pipeline remoteRunningCmd = null;
        private Pipeline DrainAndBlockRemoteOutput()
        private static void RestoreRemoteOutput(Pipeline runningCmd) => runningCmd?.ResumeIncomingData();
    /// This job is used for running as a job the results from multiple
    /// pipelines. This is used in synchronous Invoke-Expression execution.
    /// TODO: I am not sure whether to change this internal to just InvokeExpressionSyncJob.
    internal class PSInvokeExpressionSyncJob : PSRemotingChildJob
        private readonly List<ExecutionCmdletHelper> _helpers = new List<ExecutionCmdletHelper>();
        private readonly ThrottleManager _throttleManager;
        private readonly Dictionary<Guid, PowerShell> _powershells = new Dictionary<Guid, PowerShell>();
        private int _pipelineFinishedCount;
        private int _pipelineDisconnectedCount;
        /// Construct an invoke-expression sync job.
        /// <param name="operations">List of operations to use.</param>
        /// <param name="throttleManager">throttle manager to use for
        /// this job</param>
        internal PSInvokeExpressionSyncJob(List<IThrottleOperation> operations, ThrottleManager throttleManager)
            Results.AddRef();
            RegisterThrottleComplete(_throttleManager);
            foreach (IThrottleOperation operation in operations)
                RemoteRunspace remoteRS = helper.Pipeline.Runspace as RemoteRunspace;
                if (remoteRS != null)
                    remoteRS.StateChanged += HandleRunspaceStateChanged;
                    if (remoteRS.RunspaceStateInfo.State == RunspaceState.BeforeOpen)
                _helpers.Add(helper);
                Dbg.Assert(helper.Pipeline is RemotePipeline, "Only remote pipeline can be used in InvokeExpressionSyncJob");
                _powershells.Add(pipeline.PowerShell.InstanceId, pipeline.PowerShell);
        /// Clean up once job is finished.
        protected override void DoCleanupOnFinished()
            foreach (ExecutionCmdletHelper helper in _helpers)
                // cleanup remote runspace related handlers
                RemoteRunspace remoteRS = helper.PipelineRunspace as RemoteRunspace;
                    remoteRS.StateChanged -= HandleRunspaceStateChanged;
                    remoteRS.URIRedirectionReported -= HandleURIDirectionReported;
                StopAggregateResultsFromHelper(helper);
            // throttleManager = null;
            Results.DecrementRef();
        /// <param name="disposing">True if called by Dispose().</param>
        /// Handles operation complete from the operations. Adds an error record
        /// to results whenever an error is encountered.
        /// <param name="stateEventArgs">Arguments describing this event, unused.</param>
        protected override void HandleOperationComplete(object sender, OperationStateEventArgs stateEventArgs)
            ErrorRecord failureErrorRecord;
            ProcessJobFailure(helper, out failureException, out failureErrorRecord);
            if (failureErrorRecord != null)
                this.WriteError(failureErrorRecord);
        protected override void HandlePipelineStateChanged(object sender, PipelineStateEventArgs e)
                    CheckForAndSetDisconnectedState(state);
        /// Checks for a condition where all pipelines are either finished
        /// or disconnected and at least one pipeline is disconnected.
        /// In this case the Job state is set to Disconnected.
        private void CheckForAndSetDisconnectedState(PipelineState pipelineState)
            bool setJobStateToDisconnected;
                if (IsTerminalState())
                        _pipelineFinishedCount += 1;
                        _pipelineDisconnectedCount += 1;
                setJobStateToDisconnected = ((_pipelineFinishedCount + _pipelineDisconnectedCount) == _helpers.Count &&
                                              _pipelineDisconnectedCount > 0);
            if (setJobStateToDisconnected)
                // Job cannot finish with pipelines in disconnected state.
                // Set Job state to Disconnected.
        /// Used to stop all operations.
        protected override void DoFinish()
            if (_helpers.Count == 0 && this.JobStateInfo.State == JobState.NotStarted)
        /// Returns the PowerShell instance for the specified id.
        /// <param name="instanceId">Instance id of PowerShell.</param>
        internal override PowerShell GetPowerShell(Guid instanceId)
            PowerShell powershell = null;
            _powershells.TryGetValue(instanceId, out powershell);
        #endregion Protected Methods
        /// Used to unregister URI Redirection handler.
            RemoteRunspace remoteRS = sender as RemoteRunspace;
            // remote runspace must be connected (or connection failed)
            // we dont need URI redirection any more..so clear it
                if (e.RunspaceStateInfo.State != RunspaceState.Opening)
                    if (e.RunspaceStateInfo.State != RunspaceState.Opened)
        /// Submits the operations created in the constructor for invocation.
        internal void StartOperations(List<IThrottleOperation> operations)
            _throttleManager.SubmitOperations(operations);
        /// Determines if the job is in a terminal state.
        /// <returns>True, if job in terminal state
        internal bool IsTerminalState()
            return (IsFinishedState(this.JobStateInfo.State) ||
                    this.JobStateInfo.State == JobState.Disconnected);
        /// Returns a collection of all powershells for this job.
        /// <returns>Collection of PowerShell objects.</returns>
        internal Collection<PowerShell> GetPowerShells()
            Collection<PowerShell> powershellsToReturn = new Collection<PowerShell>();
            foreach (PowerShell ps in _powershells.Values)
                powershellsToReturn.Add(ps);
            return powershellsToReturn;
        /// Returns a disconnected remoting job object that contains all
        /// remote pipelines/runspaces that are in the Disconnected state.
        internal PSRemotingJob CreateDisconnectedRemotingJob()
            List<IThrottleOperation> disconnectedJobHelpers = new List<IThrottleOperation>();
            foreach (var helper in _helpers)
                if (helper.Pipeline.PipelineStateInfo.State == PipelineState.Disconnected)
                    // Remove data callbacks from the old helper.
                    // Create new helper used to create the new Disconnected PSRemoting job.
                    disconnectedJobHelpers.Add(new DisconnectedJobOperation(helper.Pipeline));
            if (disconnectedJobHelpers.Count == 0)
            return new PSRemotingJob(disconnectedJobHelpers, 0, Name, true);
    #region OutputProcessingState class
    internal class OutputProcessingStateEventArgs : EventArgs
        internal bool ProcessingOutput { get; }
        internal OutputProcessingStateEventArgs(bool processingOutput)
            ProcessingOutput = processingOutput;
    internal interface IOutputProcessingState
        event EventHandler<OutputProcessingStateEventArgs>? OutputProcessingStateChanged;

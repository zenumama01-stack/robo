    #region PowerShell v3 Job Extensions
    /// New base class for a job that provides extended state
    /// management functionality on the job. Since the existing
    /// Job class is an abstract class and there are existing
    /// implementations of the same, it is required to have a
    /// new class that will have the extended functionality. This
    /// is to ensure that backwards compatibility is maintained
    /// However, this class will derive from the existing Job
    /// class. The option of deprecating the existing class was
    /// considered as well. In order to maintain backwards
    /// compatibility of PowerShell job cmdlets they will have
    /// to work with the old interface and hence deprecating
    /// the Job class did not add any benefit rather than
    /// deriving from the same.
    /// <remarks>The following are some of the notes about
    /// why the asynchronous operations are provided this way
    /// in this class. There are two possible options in which
    /// asynchronous support can be provided:
    ///     1. Classical pattern (Begin and End)
    ///     2. Event based pattern
    /// Although the PowerShell API uses the classical pattern
    /// and we would like the Job API and PowerShell API to be
    /// as close as possible, the classical pattern is inherently
    /// complex to use.</remarks>
    public abstract class Job2 : Job
        /// These are the parameters that can be used by a job
        /// implementation when they want to specify parameters
        /// to start a job.
        private List<CommandParameterCollection> _parameters;
        /// Object that will be used for thread synchronization.
        private readonly object _syncobject = new object();
        private const int StartJobOperation = 1;
        private const int StopJobOperation = 2;
        private const int SuspendJobOperation = 3;
        private const int ResumeJobOperation = 4;
        private const int UnblockJobOperation = 5;
        private readonly PowerShellTraceSource _tracer = PowerShellTraceSourceFactory.GetTraceSource();
        /// Parameters to be used to start a job.
        /// This is a property because CommandParameterCollection
        /// does not have a public constructor. Hence the
        /// infrastructure creates an instance and provides
        /// it for the implementations to use.
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<CommandParameterCollection> StartParameters
                    lock (_syncobject)
                        _parameters ??= new List<CommandParameterCollection>();
        protected object SyncRoot
            get { return syncObject; }
        /// Default no argument constructor.
        protected Job2() : base() { }
        /// Constructor which will initialize the job
        /// with the associated command string.
        /// <param name="command">string representation
        /// of the command the job is running</param>
        protected Job2(string command) : base(command) { }
        protected Job2(string command, string name)
            : base(command, name)
        protected Job2(string command, string name, IList<Job> childJobs)
            : base(command, name, childJobs)
        /// <param name="token">JobIdentifier token used to assign Id and InstanceId.</param>
        protected Job2(string command, string name, JobIdentifier token)
            : base(command, name, token)
        /// <param name="name">Friendly name for the job.</param>
        /// <param name="instanceId">Instance ID to allow job identification across sessions.</param>
        protected Job2(string command, string name, Guid instanceId)
            : base(command, name, instanceId)
        /// There is an internal method in Job which is not made
        /// public. In order to make this available to someone
        /// implementing a job it has to be added here. If the
        /// original method is made public it has changes of
        /// colliding with some implementation which may have
        /// added that method.
        /// <param name="state">State of the job.</param>
        /// <param name="reason">exception associated with the
        /// job entering this state</param>
        protected new void SetJobState(JobState state, Exception reason)
            base.SetJobState(state, reason);
        #region State Management
        /// Start a job. The job will be started with the parameters
        /// specified in StartParameters.
        /// <remarks>It is redundant to have a method named StartJob
        /// on a job class. However, this is done so as to avoid
        /// an FxCop violation "CA1716:IdentifiersShouldNotMatchKeywords"
        /// Stop and Resume are reserved keyworks in C# and hence cannot
        /// be used as method names. Therefore to be consistent it has
        /// been decided to use *Job in the name of the methods</remarks>
        public abstract void StartJob();
        /// Start a job asynchronously.
        public abstract void StartJobAsync();
        /// Event to be raise when the start job activity is completed.
        /// This event should not be raised for
        /// synchronous operation.
        public event EventHandler<AsyncCompletedEventArgs> StartJobCompleted;
        /// Method which can be extended or called by derived
        /// classes to raise the event when start of
        /// the job is completed.
        /// <param name="eventArgs">arguments describing
        /// an exception that is associated with the event</param>
        protected virtual void OnStartJobCompleted(AsyncCompletedEventArgs eventArgs)
            RaiseCompletedHandler(StartJobOperation, eventArgs);
        /// classes to raise the event when stopping a
        /// job is completed.
        /// <param name="eventArgs">argument describing
        protected virtual void OnStopJobCompleted(AsyncCompletedEventArgs eventArgs)
            RaiseCompletedHandler(StopJobOperation, eventArgs);
        /// classes to raise the event when suspending a
        protected virtual void OnSuspendJobCompleted(AsyncCompletedEventArgs eventArgs)
            RaiseCompletedHandler(SuspendJobOperation, eventArgs);
        /// classes to raise the event when resuming a
        /// suspended job is completed.
        protected virtual void OnResumeJobCompleted(AsyncCompletedEventArgs eventArgs)
            RaiseCompletedHandler(ResumeJobOperation, eventArgs);
        /// classes to raise the event when unblocking a
        /// blocked job is completed.
        protected virtual void OnUnblockJobCompleted(AsyncCompletedEventArgs eventArgs)
            RaiseCompletedHandler(UnblockJobOperation, eventArgs);
        /// Raises the appropriate event based on the operation
        /// and the associated event arguments.
        /// <param name="operation">operation for which the event
        /// needs to be raised</param>
        private void RaiseCompletedHandler(int operation, AsyncCompletedEventArgs eventArgs)
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<AsyncCompletedEventArgs> handler = null;
                case StartJobOperation:
                        handler = StartJobCompleted;
                case StopJobOperation:
                        handler = StopJobCompleted;
                case SuspendJobOperation:
                        handler = SuspendJobCompleted;
                case ResumeJobOperation:
                        handler = ResumeJobCompleted;
                case UnblockJobOperation:
                        handler = UnblockJobCompleted;
                        Dbg.Assert(false, "this condition should not be hit, check the value of operation that you passed");
                handler?.Invoke(this, eventArgs);
                // errors in the handlers are not errors in the operation
                // silently ignore them
                _tracer.TraceException(exception);
        /// Stop a job asynchronously.
        public abstract void StopJobAsync();
        /// Event to be raised when the asynchronous stopping of a job
        /// is completed.This event should not be raised for
        public event EventHandler<AsyncCompletedEventArgs> StopJobCompleted;
        /// Suspend a job.
        public abstract void SuspendJob();
        /// Asynchronously suspend a job.
        public abstract void SuspendJobAsync();
        /// This event should be raised whenever the asynchronous suspend of
        /// a job is completed. This event should not be raised for
        public event EventHandler<AsyncCompletedEventArgs> SuspendJobCompleted;
        /// Resume a suspended job.
        public abstract void ResumeJob();
        /// Resume a suspended job asynchronously.
        public abstract void ResumeJobAsync();
        /// This event should be raised whenever the asynchronous resume of
        /// a suspended job is completed. This event should not be raised for
        public event EventHandler<AsyncCompletedEventArgs> ResumeJobCompleted;
        /// Unblock a blocked job.
        public abstract void UnblockJob();
        /// Unblock a blocked job asynchronously.
        public abstract void UnblockJobAsync();
        /// StopJob.
        /// <param name="reason"></param>
        public abstract void StopJob(bool force, string reason);
        /// StopJobAsync.
        public abstract void StopJobAsync(bool force, string reason);
        /// SuspendJob.
        public abstract void SuspendJob(bool force, string reason);
        /// SuspendJobAsync.
        public abstract void SuspendJobAsync(bool force, string reason);
        /// This event should be raised whenever the asynchronous unblock
        /// of a blocked job is completed. This event should not be raised for
        public event EventHandler<AsyncCompletedEventArgs> UnblockJobCompleted;
        #endregion State Management
    /// Specifies the various thread options that can be used
    /// for the ThreadBasedJob.
    public enum JobThreadOptions
        /// Use the default behavior, which is to use a
        /// ThreadPoolThread.
        /// Use a thread pool thread.
        UseThreadPoolThread = 1,
        /// Create a new thread everything and reuse.
        UseNewThread = 2,
    /*/// <summary>
    /// This job will provide asynchronous behavior by running
    /// the user specified script block in a separate process.
    /// There will be options for running the scriptblock
    /// in a new process or an existing process.
    /// <remarks>Jobs for the out-of-process activity manager
    /// can be implemented using this interface</remarks>
    public abstract class ProcessBasedJob : Job2
        public override void Start()
        public override void StartAsync()
    /// Top level container job.
    public sealed class ContainerParentJob : Job2
        private const string TraceClassName = "ContainerParentJob";
        private int _isDisposed = 0;
        private const int DisposedTrue = 1;
        private const int DisposedFalse = 0;
        // count of number of child jobs which are suspended
        private int _suspendedChildJobsCount = 0;
        // count of number of child jobs which are suspending
        private int _suspendingChildJobsCount = 0;
        // count of number of child jobs which failed
        private int _failedChildJobsCount = 0;
        // count of number of child jobs which stopped
        private int _stoppedChildJobsCount = 0;
        private readonly PSDataCollection<ErrorRecord> _executionError = new PSDataCollection<ErrorRecord>();
        private PSEventManager _eventManager;
        internal PSEventManager EventManager
                return _eventManager;
                _tracer.WriteMessage("Setting event manager for Job ", InstanceId);
                _eventManager = value;
        private ManualResetEvent _jobRunning;
        private ManualResetEvent JobRunning
                if (_jobRunning == null)
                            // this assert is required so that a wait handle
                            // is not created after the object is disposed
                            // which will result in a leak
                            _jobRunning = new ManualResetEvent(false);
                return _jobRunning;
        private ManualResetEvent _jobSuspendedOrAborted;
        private ManualResetEvent JobSuspendedOrAborted
                if (_jobSuspendedOrAborted == null)
                            _jobSuspendedOrAborted = new ManualResetEvent(false);
                return _jobSuspendedOrAborted;
        /// Create a container parent job with the
        /// specified command string and name.
        /// <param name="name">Friendly name for display.</param>
        public ContainerParentJob(string command, string name)
            StateChanged += HandleMyStateChanged;
        /// specified command string.
        public ContainerParentJob(string command)
        /// <param name="jobId">JobIdentifier token that allows reuse of an Id and Instance Id.</param>
        public ContainerParentJob(string command, string name, JobIdentifier jobId)
            : base(command, name, jobId)
        public ContainerParentJob(string command, string name, Guid instanceId)
        /// <param name="jobType">Job type name.</param>
        public ContainerParentJob(string command, string name, JobIdentifier jobId, string jobType)
            PSJobTypeName = jobType;
        public ContainerParentJob(string command, string name, Guid instanceId, string jobType)
        /// Create a container parent job with the specified command, name,
        /// job type strings.
        public ContainerParentJob(string command, string name, string jobType)
        internal PSDataCollection<ErrorRecord> ExecutionError { get { return _executionError; } }
        /// Add a child job to the parent job.
        /// <exception cref="ObjectDisposedException">Thrown if the job is disposed.</exception>
        /// <exception cref="ArgumentNullException">Thrown if child being added is null.</exception>
        public void AddChildJob(Job2 childJob)
            ArgumentNullException.ThrowIfNull(childJob);
            _tracer.WriteMessage(TraceClassName, "AddChildJob", Guid.Empty, childJob, "Adding Child to Parent with InstanceId : ", InstanceId.ToString());
            JobStateInfo childJobStateInfo;
            lock (childJob.syncObject)
                // Store job's state and subscribe to State Changed event. Locking here will
                // ensure that the jobstateinfo we get is the state before any state changed events are handled by ContainerParentJob.
                childJobStateInfo = childJob.JobStateInfo;
            ParentJobStateCalculation(new JobStateEventArgs(childJobStateInfo, new JobStateInfo(JobState.NotStarted)));
                return ConstructStatusMessage();
        /// Starts all jobs.
        /// <exception cref="ObjectDisposedException">Thrown if job is disposed.</exception>
        public override void StartJob()
            _tracer.WriteMessage(TraceClassName, "StartJob", Guid.Empty, this, "Entering method", null);
            s_structuredTracer.BeginContainerParentJobExecution(InstanceId);
            // If parent contains no child jobs then this method will not respond.  Throw error in this case.
            if (ChildJobs.Count == 0)
                throw PSTraceSource.NewInvalidOperationException(RemotingErrorIdStrings.JobActionInvalidWithNoChildJobs);
            foreach (Job2 job in this.ChildJobs)
                if (job == null) throw PSTraceSource.NewInvalidOperationException(RemotingErrorIdStrings.JobActionInvalidWithNullChild);
            // If there is only one child job, call the synchronous method on the child to avoid use of another thread.
            // If there are multiple, we can run them in parallel using the asynchronous versions.
            if (ChildJobs.Count == 1)
                Job2 child = ChildJobs[0] as Job2;
                Dbg.Assert(child != null, "Job is null after initial null check");
                    _tracer.WriteMessage(TraceClassName, "StartJob", Guid.Empty, this,
                        "Single child job synchronously, child InstanceId: {0}", child.InstanceId.ToString());
                    child.StartJob();
                    JobRunning.WaitOne();
                    // These exceptions are thrown by third party code. Adding them here to the collection
                    // of execution errors to present consistent behavior of the object.
                    ExecutionError.Add(new ErrorRecord(e, "ContainerParentJobStartError",
                                                       ErrorCategory.InvalidResult, child));
                        "Single child job threw exception, child InstanceId: {0}", child.InstanceId.ToString());
                    _tracer.TraceException(e);
            var completed = new AutoResetEvent(false);
            // Count of StartJobCompleted events from children.
            var startedChildJobsCount = 0;
            EventHandler<AsyncCompletedEventArgs> eventHandler = (object sender, AsyncCompletedEventArgs e) =>
                var childJob = sender as Job2;
                Dbg.Assert(childJob != null,
                           "StartJobCompleted only available on Job2");
                _tracer.WriteMessage(TraceClassName, "StartJob-Handler", Guid.Empty, this,
                    "Finished starting child job asynchronously, child InstanceId: {0}", childJob.InstanceId.ToString());
                if (e.Error != null)
                    ExecutionError.Add(
                        new ErrorRecord(e.Error,
                                        "ContainerParentJobStartError",
                                        childJob));
                        "Child job asynchronously had error, child InstanceId: {0}", childJob.InstanceId.ToString());
                    _tracer.TraceException(e.Error);
                Interlocked.Increment(ref startedChildJobsCount);
                if (startedChildJobsCount == ChildJobs.Count)
                        "Finished starting all child jobs asynchronously", null);
                    completed.Set();
            foreach (Job2 job in ChildJobs)
                Dbg.Assert(job != null, "Job is null after initial null check");
                job.StartJobCompleted += eventHandler;
                    "Child job asynchronously, child InstanceId: {0}", job.InstanceId.ToString());
                // This child job is created to run synchronously and so can be debugged.  Set
                // the IJobDebugger.IsAsync accordingly.
                ScriptDebugger.SetDebugJobAsync(job as IJobDebugger, false);
                job.StartJobAsync();
            completed.WaitOne();
                job.StartJobCompleted -= eventHandler;
            if (ExecutionError.Count > 0)
                // Check to see expected behavior if one child job fails to start.
            if (ExecutionError.Count == 1)
                throw ExecutionError[0];
            } */
            _tracer.WriteMessage(TraceClassName, "StartJob", Guid.Empty, this, "Exiting method", null);
        private static readonly Tracer s_structuredTracer = new Tracer();
        /// Starts all child jobs asynchronously.
        /// When all child jobs are started, StartJobCompleted event is raised.
        public override void StartJobAsync()
            if (_isDisposed == DisposedTrue)
                OnStartJobCompleted(new AsyncCompletedEventArgs(new ObjectDisposedException(TraceClassName), false, null));
            _tracer.WriteMessage(TraceClassName, "StartJobAsync", Guid.Empty, this, "Entering method", null);
            EventHandler<AsyncCompletedEventArgs> eventHandler = null;
            eventHandler = (sender, e) =>
                 Dbg.Assert(childJob != null, "StartJobCompleted only available on Job2");
                 _tracer.WriteMessage(TraceClassName, "StartJobAsync-Handler", Guid.Empty, this,
                     ExecutionError.Add(new ErrorRecord(e.Error, "ContainerParentJobStartAsyncError",
                         ErrorCategory.InvalidResult, childJob));
                 Dbg.Assert(eventHandler != null, "Event handler magically disappeared");
                 childJob.StartJobCompleted -= eventHandler;
                     // There may be multiple exceptions raised. They
                     // are stored in the Error stream of this job object, which is otherwise
                     // unused.
                     OnStartJobCompleted(new AsyncCompletedEventArgs(null, false, null));
                _tracer.WriteMessage(TraceClassName, "StartJobAsync", Guid.Empty, this,
            _tracer.WriteMessage(TraceClassName, "StartJobAsync", Guid.Empty, this, "Exiting method", null);
        /// Resume all jobs.
        public override void ResumeJob()
            _tracer.WriteMessage(TraceClassName, "ResumeJob", Guid.Empty, this, "Entering method", null);
                    _tracer.WriteMessage(TraceClassName, "ResumeJob", Guid.Empty, this,
                    child.ResumeJob();
                    ExecutionError.Add(new ErrorRecord(e, "ContainerParentJobResumeError",
            // Count of ResumeJobCompleted events from children.
            var resumedChildJobsCount = 0;
                eventHandler = (object sender, AsyncCompletedEventArgs e) =>
                                                Dbg.Assert(childJob != null, "ResumeJobCompleted only available on Job2");
                                                _tracer.WriteMessage(TraceClassName, "ResumeJob-Handler", Guid.Empty, this,
                                                    "Finished resuming child job asynchronously, child InstanceId: {0}", job.InstanceId.ToString());
                                                    ExecutionError.Add(new ErrorRecord(e.Error, "ContainerParentJobResumeError",
                                                        ErrorCategory.InvalidResult, job));
                                                        "Child job asynchronously had error, child InstanceId: {0}", job.InstanceId.ToString());
                                                Interlocked.Increment(ref resumedChildJobsCount);
                                                if (resumedChildJobsCount == ChildJobs.Count)
                                                        "Finished resuming all child jobs asynchronously", null);
                job.ResumeJobCompleted += eventHandler;
                job.ResumeJobAsync();
                job.ResumeJobCompleted -= eventHandler;
            _tracer.WriteMessage(TraceClassName, "ResumeJob", Guid.Empty, this, "Exiting method", null);
            // Errors are taken from the Error collection by the cmdlet for ContainerParentJob.
        /// Resume all jobs asynchronously.
        public override void ResumeJobAsync()
                OnResumeJobCompleted(new AsyncCompletedEventArgs(new ObjectDisposedException(TraceClassName), false, null));
            _tracer.WriteMessage(TraceClassName, "ResumeJobAsync", Guid.Empty, this, "Entering method", null);
                                            _tracer.WriteMessage(TraceClassName, "ResumeJobAsync-Handler", Guid.Empty, this,
                                                ExecutionError.Add(new ErrorRecord(e.Error, "ContainerParentJobResumeAsyncError",
                                            childJob.ResumeJobCompleted -= eventHandler;
                                                OnResumeJobCompleted(new AsyncCompletedEventArgs(null, false, null));
                _tracer.WriteMessage(TraceClassName, "ResumeJobAsync", Guid.Empty, this,
            _tracer.WriteMessage(TraceClassName, "ResumeJobAsync", Guid.Empty, this, "Exiting method", null);
        /// Suspends all jobs.
        public override void SuspendJob()
            SuspendJobInternal(null, null);
        /// Suspends all jobs forcefully.
        /// <param name="force">Force flag for suspending forcefully.</param>
        /// <param name="reason">Reason for doing forceful suspend.</param>
        public override void SuspendJob(bool force, string reason)
            SuspendJobInternal(force, reason);
        /// Suspends all jobs asynchronously.
        /// When all jobs have been suspended, SuspendJobCompleted is raised.
        public override void SuspendJobAsync()
            SuspendJobAsyncInternal(null, null);
        /// Suspends all jobs asynchronously with force flag.
        public override void SuspendJobAsync(bool force, string reason)
            SuspendJobAsyncInternal(force, reason);
        /// Stop all child jobs.
            StopJobInternal(null, null);
        /// Stops all child jobs asynchronously.
        /// Once all child jobs are stopped, StopJobCompleted event is raised.
        public override void StopJobAsync()
            StopJobAsyncInternal(null, null);
        public override void StopJob(bool force, string reason)
            StopJobInternal(force, reason);
        public override void StopJobAsync(bool force, string reason)
            StopJobAsyncInternal(force, reason);
        /// Unblock all child jobs.
        public override void UnblockJob()
            _tracer.WriteMessage(TraceClassName, "UnblockJob", Guid.Empty, this, "Entering method", null);
                    _tracer.WriteMessage(TraceClassName, "UnblockJob", Guid.Empty, this,
                    child.UnblockJob();
                    ExecutionError.Add(new ErrorRecord(e, "ContainerParentJobUnblockError",
            // count of UnblockJobCompleted events from children.
            int unblockedChildJobsCount = 0;
                                            Dbg.Assert(childJob != null, "UnblockJobCompleted only available on Job2");
                                            _tracer.WriteMessage(TraceClassName, "UnblockJob-Handler", Guid.Empty, this,
                                                "Finished unblock child job asynchronously, child InstanceId: {0}", job.InstanceId.ToString());
                                                ExecutionError.Add(new ErrorRecord(e.Error, "ContainerParentJobUnblockError",
                                            Interlocked.Increment(ref unblockedChildJobsCount);
                                            if (unblockedChildJobsCount == ChildJobs.Count)
                                                    "Finished unblock all child jobs asynchronously", null);
                job.UnblockJobCompleted += eventHandler;
                job.UnblockJobAsync();
                job.UnblockJobCompleted -= eventHandler;
            _tracer.WriteMessage(TraceClassName, "UnblockJob", Guid.Empty, this, "Exiting method", null);
        /// Unblock all child jobs asynchronously.
        /// Once all child jobs are unblocked, UnblockJobCompleted event is raised.
        public override void UnblockJobAsync()
                OnUnblockJobCompleted(new AsyncCompletedEventArgs(new ObjectDisposedException(TraceClassName), false, null));
            _tracer.WriteMessage(TraceClassName, "UnblockJobAsync", Guid.Empty, this, "Entering method", null);
                                            _tracer.WriteMessage(TraceClassName, "UnblockJobAsync-Handler", Guid.Empty, this,
                                            childJob.UnblockJobCompleted -= eventHandler;
                                                // State change is handled elsewhere.
                                                OnUnblockJobCompleted(new AsyncCompletedEventArgs(null, false, null));
                _tracer.WriteMessage(TraceClassName, "UnblockJobAsync", Guid.Empty, this,
            _tracer.WriteMessage(TraceClassName, "UnblockJobAsync", Guid.Empty, this, "Exiting method", null);
        /// Internal synchronous SuspendJob, calls appropriate version if Force is specified.
        private void SuspendJobInternal(bool? force, string reason)
            _tracer.WriteMessage(TraceClassName, "SuspendJob", Guid.Empty, this, "Entering method", null);
                    _tracer.WriteMessage(TraceClassName, "SuspendJob", Guid.Empty, this,
                                         "Single child job synchronously, child InstanceId: {0} force: {1}", child.InstanceId.ToString(), force.ToString());
                    if (force.HasValue)
                        child.SuspendJob(force.Value, reason);
                        child.SuspendJob();
                    JobSuspendedOrAborted.WaitOne();
                    ExecutionError.Add(new ErrorRecord(e, "ContainerParentJobSuspendError",
                        "Single child job threw exception, child InstanceId: {0} force: {1}", child.InstanceId.ToString(), force.ToString());
            AutoResetEvent completed = new AutoResetEvent(false);
            var suspendedChildJobsCount = 0;
                                "SuspendJobCompleted only available on Job2");
                    _tracer.WriteMessage(TraceClassName, "SuspendJob-Handler", Guid.Empty, this,
                        "Finished suspending child job asynchronously, child InstanceId: {0} force: {1}", job.InstanceId.ToString(), force.ToString());
                        ExecutionError.Add(new ErrorRecord(e.Error, "ContainerParentJobSuspendError",
                            "Child job asynchronously had error, child InstanceId: {0} force: {1}", job.InstanceId.ToString(), force.ToString());
                    Interlocked.Increment(ref suspendedChildJobsCount);
                    if (suspendedChildJobsCount == ChildJobs.Count)
                            "Finished suspending all child jobs asynchronously", null);
                job.SuspendJobCompleted += eventHandler;
                    "Child job asynchronously, child InstanceId: {0} force: {1}", job.InstanceId.ToString(), force.ToString());
                    job.SuspendJobAsync(force.Value, reason);
                    job.SuspendJobAsync();
                job.SuspendJobCompleted -= eventHandler;
            _tracer.WriteMessage(TraceClassName, "SuspendJob", Guid.Empty, this, "Exiting method", null);
        /// Internal SuspendJobAsync. Calls appropriate method if Force is specified.
        private void SuspendJobAsyncInternal(bool? force, string reason)
                OnSuspendJobCompleted(new AsyncCompletedEventArgs(new ObjectDisposedException(TraceClassName), false, null));
            _tracer.WriteMessage(TraceClassName, "SuspendJobAsync", Guid.Empty, this, "Entering method", null);
            // Count of SuspendJobCompleted events from children.
                    Dbg.Assert(childJob != null, "SuspendJobCompleted only available on Job2");
                    _tracer.WriteMessage(TraceClassName, "SuspendJobAsync-Handler", Guid.Empty, this,
                        ExecutionError.Add(new ErrorRecord(e.Error, "ContainerParentJobSuspendAsyncError",
                    childJob.SuspendJobCompleted -= eventHandler;
                        OnSuspendJobCompleted(new AsyncCompletedEventArgs(null, false, null));
                _tracer.WriteMessage(TraceClassName, "SuspendJobAsync", Guid.Empty, this,
            _tracer.WriteMessage(TraceClassName, "SuspendJobAsync", Guid.Empty, this, "Exiting method", null);
        private void StopJobInternal(bool? force, string reason)
            _tracer.WriteMessage(TraceClassName, "StopJob", Guid.Empty, this, "Entering method", null);
                    _tracer.WriteMessage(TraceClassName, "StopJob", Guid.Empty, this,
                        child.StopJob(force.Value, reason);
                        child.StopJob();
                    ExecutionError.Add(new ErrorRecord(e, "ContainerParentJobStopError",
            // Count of StopJobCompleted events from children.
            var stoppedChildJobsCount = 0;
                                "StopJobCompleted only available on Job2");
                    _tracer.WriteMessage(TraceClassName, "StopJob-Handler", Guid.Empty, this,
                        "Finished stopping child job asynchronously, child InstanceId: {0}", job.InstanceId.ToString());
                        ExecutionError.Add(new ErrorRecord(e.Error, "ContainerParentJobStopError",
                    Interlocked.Increment(ref stoppedChildJobsCount);
                    if (stoppedChildJobsCount == ChildJobs.Count)
                        "Finished stopping all child jobs asynchronously", null);
                job.StopJobCompleted += eventHandler;
                    job.StopJobAsync(force.Value, reason);
                    job.StopJobAsync();
                job.StopJobCompleted -= eventHandler;
            _tracer.WriteMessage(TraceClassName, "StopJob", Guid.Empty, this, "Exiting method", null);
        private void StopJobAsyncInternal(bool? force, string reason)
                OnStopJobCompleted(new AsyncCompletedEventArgs(new ObjectDisposedException(TraceClassName), false, null));
            _tracer.WriteMessage(TraceClassName, "StopJobAsync", Guid.Empty, this, "Entering method", null);
            // count of StopJobCompleted events from children.
            int stoppedChildJobsCount = 0;
                    Dbg.Assert(childJob != null, "StopJobCompleted only available on Job2");
                    _tracer.WriteMessage(TraceClassName, "StopJobAsync-Handler", Guid.Empty, this,
                        ExecutionError.Add(new ErrorRecord(e.Error, "ContainerParentJobStopAsyncError",
                    childJob.StopJobCompleted -= eventHandler;
                        OnStopJobCompleted(new AsyncCompletedEventArgs(null, false, null));
                _tracer.WriteMessage(TraceClassName, "StopJobAsync", Guid.Empty, this,
            _tracer.WriteMessage(TraceClassName, "StopJobAsync", Guid.Empty, this, "Exiting method", null);
        private void HandleMyStateChanged(object sender, JobStateEventArgs e)
            _tracer.WriteMessage(TraceClassName, "HandleMyStateChanged", Guid.Empty, this,
                                 "NewState: {0}; OldState: {1}", e.JobStateInfo.State.ToString(),
                                 e.PreviousJobStateInfo.State.ToString());
            switch (e.JobStateInfo.State)
                case JobState.Running:
                            JobRunning.Set();
                            // Do not create the event if it doesn't already exist. Suspend may never be called.
                            if (_jobSuspendedOrAborted != null)
                                JobSuspendedOrAborted.Reset();
                case JobState.Suspended:
                            JobSuspendedOrAborted.Set();
                            JobRunning.Reset();
                case JobState.Failed:
                case JobState.Completed:
                case JobState.Stopped:
                            // Do not reset JobRunning when the state is terminal.
                            // No thread should wait on a job transitioning again to
                            // JobState.Running.
            ParentJobStateCalculation(e);
        private void ParentJobStateCalculation(JobStateEventArgs e)
            JobState computedState;
            if (ComputeJobStateFromChildJobStates("ContainerParentJob", e, ref _blockedChildJobsCount, ref _suspendedChildJobsCount, ref _suspendingChildJobsCount,
                    ref _finishedChildJobsCount, ref _failedChildJobsCount, ref _stoppedChildJobsCount, ChildJobs.Count, out computedState))
                if (computedState != JobStateInfo.State)
                    if (JobStateInfo.State == JobState.NotStarted && computedState == JobState.Running)
                    if (!IsFinishedState(JobStateInfo.State) && IsPersistentState(computedState))
                    SetJobState(computedState);
                    s_structuredTracer.EndContainerParentJobExecution(InstanceId);
        /// <param name="traceClassName"></param>
        /// <param name="blockedChildJobsCount"></param>
        /// <param name="suspendedChildJobsCount"></param>
        /// <param name="suspendingChildJobsCount"></param>
        /// <param name="finishedChildJobsCount"></param>
        /// <param name="stoppedChildJobsCount"></param>
        /// <param name="childJobsCount"></param>
        /// <param name="computedJobState"></param>
        /// <param name="failedChildJobsCount"></param>
        /// <returns>True if the job state needs to be modified, false otherwise.</returns>
        internal static bool ComputeJobStateFromChildJobStates(string traceClassName, JobStateEventArgs e,
            ref int blockedChildJobsCount, ref int suspendedChildJobsCount, ref int suspendingChildJobsCount, ref int finishedChildJobsCount,
                ref int failedChildJobsCount, ref int stoppedChildJobsCount, int childJobsCount,
                    out JobState computedJobState)
            computedJobState = JobState.NotStarted;
                    Interlocked.Increment(ref blockedChildJobsCount);
                    tracer.WriteMessage(traceClassName, ": JobState is Blocked, at least one child job is blocked.");
                    computedJobState = JobState.Blocked;
                if (e.PreviousJobStateInfo.State == JobState.Blocked)
                    // check if any of the child jobs were unblocked
                    // in which case we need to check if the parent
                    // job needs to be unblocked as well
                    Interlocked.Decrement(ref blockedChildJobsCount);
                    if (blockedChildJobsCount == 0)
                        tracer.WriteMessage(traceClassName, ": JobState is unblocked, all child jobs are unblocked.");
                        computedJobState = JobState.Running;
                if (e.PreviousJobStateInfo.State == JobState.Suspended)
                    // decrement count of suspended child jobs
                    // needed to determine when all incomplete child jobs are suspended for parent job state.
                    Interlocked.Decrement(ref suspendedChildJobsCount);
                if (e.PreviousJobStateInfo.State == JobState.Suspending)
                    // decrement count of suspending child jobs
                    Interlocked.Decrement(ref suspendingChildJobsCount);
                if (e.JobStateInfo.State == JobState.Suspended)
                    // increment count of suspended child jobs.
                    // We know that at least one child is suspended. If all jobs are either complete or suspended, set the state.
                    if (suspendedChildJobsCount + finishedChildJobsCount == childJobsCount)
                        tracer.WriteMessage(traceClassName, ": JobState is suspended, all child jobs are suspended.");
                        computedJobState = JobState.Suspended;
                    // Job state should continue to be running unless:
                    // at least one child is suspended
                    // AND
                    // all child jobs are either suspended or finished.
                if (e.JobStateInfo.State == JobState.Suspending)
                    // increment count of suspending child jobs.
                    Interlocked.Increment(ref suspendingChildJobsCount);
                    if (suspendedChildJobsCount + finishedChildJobsCount + suspendingChildJobsCount == childJobsCount)
                        tracer.WriteMessage(traceClassName, ": JobState is suspending, all child jobs are in suspending state.");
                        computedJobState = JobState.Suspending;
                    // at least one child is suspended, suspending
                // State will be Running once at least one child is running.
                if ((e.JobStateInfo.State != JobState.Completed && e.JobStateInfo.State != JobState.Failed) && e.JobStateInfo.State != JobState.Stopped)
                    if (e.JobStateInfo.State == JobState.Running)
                    // if the job state is Suspended, we have already returned.
                    // if the job state is NotStarted, do not set the state.
                    // if the job state is blocked, we have already returned.
                    // we can set it right now and
                    Interlocked.Increment(ref failedChildJobsCount);
                // If stop has not been called, but a child has been stopped, the parent should
                // reflect the stopped state.
                if (e.JobStateInfo.State == JobState.Stopped)
                int finishedChildJobsCountNew = Interlocked.Increment(ref finishedChildJobsCount);
                if (finishedChildJobsCountNew == childJobsCount)
                    // else completed);
                    if (failedChildJobsCount > 0)
                        tracer.WriteMessage(traceClassName, ": JobState is failed, at least one child job failed.");
                        computedJobState = JobState.Failed;
                    if (stoppedChildJobsCount > 0)
                        tracer.WriteMessage(traceClassName, ": JobState is stopped, stop is called.");
                        computedJobState = JobState.Stopped;
                    tracer.WriteMessage(traceClassName, ": JobState is completed.");
                    computedJobState = JobState.Completed;
                // If not all jobs are finished, one child job may be suspended, even though this job did not finish.
                // At this point, we know finishedChildJobsCountNew != childJobsCount
                if (suspendedChildJobsCount + finishedChildJobsCountNew == childJobsCount)
                // If not all jobs are finished, one child job may be suspending, even though this job did not finish.
                // At this point, we know finishedChildJobsCountNew != childJobsCount and finishChildJobsCount + suspendedChilJobsCout != childJobsCount
                if (suspendingChildJobsCount + suspendedChildJobsCount + finishedChildJobsCountNew == childJobsCount)
            if (!disposing) return;
            if (Interlocked.CompareExchange(ref _isDisposed, DisposedTrue, DisposedFalse) == DisposedTrue) return;
                UnregisterAllJobEvents();
                _executionError.Dispose();
                StateChanged -= HandleMyStateChanged;
                    _tracer.WriteMessage("Disposing child job with id : " + job.Id);
                _jobRunning?.Dispose();
                _jobSuspendedOrAborted?.Dispose();
                base.Dispose(true);
            if (ChildJobs == null || ChildJobs.Count == 0)
            string location = ChildJobs.Select(static (job) => job.Location).Aggregate((s1, s2) => s1 + ',' + s2);
        private string ConstructStatusMessage()
                if (!string.IsNullOrEmpty(ChildJobs[i].StatusMessage))
                    sb.Append(ChildJobs[i].StatusMessage);
                if (i < (ChildJobs.Count - 1))
        private void UnregisterJobEvent(Job job)
            string sourceIdentifier = job.InstanceId + ":StateChanged";
            _tracer.WriteMessage("Unregistering StateChanged event for job ", job.InstanceId);
            foreach (PSEventSubscriber subscriber in
                EventManager.Subscribers.Where(subscriber => string.Equals(subscriber.SourceIdentifier, sourceIdentifier, StringComparison.OrdinalIgnoreCase)))
                EventManager.UnsubscribeEvent(subscriber);
        private void UnregisterAllJobEvents()
            if (EventManager == null)
                _tracer.WriteMessage("No events subscribed, skipping event unregistrations");
            foreach (var job in ChildJobs)
                UnregisterJobEvent(job);
            UnregisterJobEvent(this);
            _tracer.WriteMessage("Setting event manager to null");
            EventManager = null;
    /// Container exception for jobs that can map errors and exceptions
    /// to specific lines in their input.
    public class JobFailedException : SystemException
        /// Creates a new JobFailedException.
        public JobFailedException()
        /// <param name="message">The message of the exception.</param>
        public JobFailedException(string message)
        /// <param name="innerException">The actual exception that caused this error.</param>
        public JobFailedException(string message, Exception innerException)
        /// <param name="displayScriptPosition">A ScriptExtent that describes where this error originated from.</param>
        public JobFailedException(Exception innerException, ScriptExtent displayScriptPosition)
            _reason = innerException;
            _displayScriptPosition = displayScriptPosition;
        /// <param name="serializationInfo">Serialization info.</param>
        /// <param name="streamingContext">Streaming context.</param>
        protected JobFailedException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        /// The actual exception that caused this error.
        public Exception Reason { get { return _reason; } }
        private readonly Exception _reason;
        /// The user-focused location from where this error originated.
        public ScriptExtent DisplayScriptPosition { get { return _displayScriptPosition; } }
        private readonly ScriptExtent _displayScriptPosition;
        /// Returns the reason for this exception.
                return Reason.Message;
    #endregion PowerShell v3 Job Extensions

    internal abstract class StartableJob : Job
        internal StartableJob(string commandName, string jobName)
            : base(commandName, jobName)
        internal abstract void StartJob();
    /// A job that can throttle execution of child jobs.
    internal sealed class ThrottlingJob : Job
                    List<Job> childJobsToDispose;
                        Dbg.Assert(this.IsFinishedState(this.JobStateInfo.State), "ThrottlingJob should be completed before removing and disposing child jobs");
                        childJobsToDispose = new List<Job>(this.ChildJobs);
                        this.ChildJobs.Clear();
                    foreach (Job childJob in childJobsToDispose)
                        childJob.Dispose();
                    _jobResultsThrottlingSemaphore?.Dispose();
        private readonly DateTime _progressStartTime = DateTime.UtcNow;
        private readonly int _progressActivityId;
        private readonly object _progressLock = new object();
        private DateTime _progressReportLastTime = DateTime.MinValue;
        internal int GetProgressActivityId()
            lock (_progressLock)
                if (_progressReportLastTime.Equals(DateTime.MinValue))
                        this.ReportProgress(minimizeFrequentUpdates: false);
                        Dbg.Assert(_progressReportLastTime > DateTime.MinValue, "Progress was reported (lastTimeProgressWasReported)");
                        // return "no parent activity id" if this ThrottlingJob has already finished
                return _progressActivityId;
        private void ReportProgress(bool minimizeFrequentUpdates)
                if (minimizeFrequentUpdates)
                    if ((now - _progressStartTime) < TimeSpan.FromSeconds(1))
                    if ((!_progressReportLastTime.Equals(DateTime.MinValue)) &&
                        (now - _progressReportLastTime < TimeSpan.FromMilliseconds(200)))
                _progressReportLastTime = now;
                double workCompleted;
                double totalWork;
                int percentComplete;
                    totalWork = _countOfAllChildJobs;
                    workCompleted = this.CountOfFinishedChildJobs;
                if (totalWork >= 1.0)
                    percentComplete = (int)(100.0 * workCompleted / totalWork);
                    percentComplete = -1;
                percentComplete = Math.Max(-1, Math.Min(100, percentComplete));
                var progressRecord = new ProgressRecord(
                    activityId: _progressActivityId,
                    activity: this.Command,
                    statusDescription: this.StatusMessage);
                if (this.IsThrottlingJobCompleted)
                    progressRecord.RecordType = ProgressRecordType.Completed;
                    progressRecord.PercentComplete = 100;
                    progressRecord.SecondsRemaining = 0;
                    progressRecord.RecordType = ProgressRecordType.Processing;
                    progressRecord.PercentComplete = percentComplete;
                    int? secondsRemaining = null;
                    if (percentComplete >= 0)
                        secondsRemaining = ProgressRecord.GetSecondsRemaining(_progressStartTime, (double)percentComplete / 100.0);
        /// Flags of child jobs of a <see cref="ThrottlingJob"/>
        internal enum ChildJobFlags
            /// Child job doesn't have any special properties.
            /// Child job can call <see cref="ThrottlingJob.AddChildJobWithoutBlocking"/> method
            /// or <see cref="ThrottlingJob.AddChildJobAndPotentiallyBlock(StartableJob, ChildJobFlags)"/>
            /// or <see cref="ThrottlingJob.AddChildJobAndPotentiallyBlock(Cmdlet, StartableJob, ChildJobFlags)"/>
            /// method
            /// of the <see cref="ThrottlingJob"/> instance it belongs to.
            CreatesChildJobs = 0x1,
        private bool _ownerWontSubmitNewChildJobs = false;
        private readonly HashSet<Guid> _setOfChildJobsThatCanAddMoreChildJobs = new HashSet<Guid>();
        private bool IsEndOfChildJobs
                    return _isStopping || (_ownerWontSubmitNewChildJobs && _setOfChildJobsThatCanAddMoreChildJobs.Count == 0);
        private bool IsThrottlingJobCompleted
                    return this.IsEndOfChildJobs && (_countOfAllChildJobs <= this.CountOfFinishedChildJobs);
        private readonly bool _cmdletMode;
        private int _countOfAllChildJobs;
        private int _countOfBlockedChildJobs;
        private int _countOfFailedChildJobs;
        private int _countOfStoppedChildJobs;
        private int _countOfSuccessfullyCompletedChildJobs;
        private int CountOfFinishedChildJobs
                    return _countOfFailedChildJobs + _countOfStoppedChildJobs + _countOfSuccessfullyCompletedChildJobs;
        private int CountOfRunningOrReadyToRunChildJobs
                    return _countOfAllChildJobs - this.CountOfFinishedChildJobs;
        /// Creates a new <see cref="ThrottlingJob"/> object.
        /// <param name="jobName">Friendly name for the job object.</param>
        /// <param name="jobTypeName">Name describing job type.</param>
        /// <param name="maximumConcurrentChildJobs">
        /// The maximum number of child jobs that can be running at any given point in time.
        /// Passing 0 requests to turn off throttling (i.e. allow unlimited number of child jobs to run)
        /// <param name="cmdletMode">
        /// <see langword="true"/> if this <see cref="ThrottlingJob"/> is used from a cmdlet invoked without -AsJob switch.
        /// <see langword="false"/> if this <see cref="ThrottlingJob"/> is used from a cmdlet invoked with -AsJob switch.
        /// If <paramref name="cmdletMode"/> is <see langword="true"/>, then
        /// memory can be managed more aggressively (for example ChildJobs can be discarded as soon as they complete)
        /// because the <see cref="ThrottlingJob"/> is not exposed to the end user.
        internal ThrottlingJob(string command, string jobName, string jobTypeName, int maximumConcurrentChildJobs, bool cmdletMode)
            : base(command, jobName)
            this.Results.BlockingEnumerator = true;
            _cmdletMode = cmdletMode;
            this.PSJobTypeName = jobTypeName;
            if (_cmdletMode)
                _jobResultsThrottlingSemaphore = new SemaphoreSlim(ForwardingHelper.AggregationQueueMaxCapacity);
            _progressActivityId = new Random(this.GetHashCode()).Next();
            this.SetupThrottlingQueue(maximumConcurrentChildJobs);
        internal void AddChildJobAndPotentiallyBlock(
            StartableJob childJob,
            ChildJobFlags flags)
            using (var jobGotEnqueued = new ManualResetEventSlim(initialState: false))
                this.AddChildJobWithoutBlocking(childJob, flags, jobGotEnqueued.Set);
                jobGotEnqueued.Wait();
            using (var forwardingCancellation = new CancellationTokenSource())
                this.AddChildJobWithoutBlocking(childJob, flags, forwardingCancellation.Cancel);
                this.ForwardAllResultsToCmdlet(cmdlet, forwardingCancellation.Token);
        private bool _alreadyDisabledFlowControlForPendingJobsQueue = false;
        internal void DisableFlowControlForPendingJobsQueue()
            if (!_cmdletMode || _alreadyDisabledFlowControlForPendingJobsQueue)
            _alreadyDisabledFlowControlForPendingJobsQueue = true;
                _maxReadyToRunJobs = int.MaxValue;
                while (_actionsForUnblockingChildAdditions.Count > 0)
                    Action a = _actionsForUnblockingChildAdditions.Dequeue();
                    a?.Invoke();
        private bool _alreadyDisabledFlowControlForPendingCmdletActionsQueue = false;
        internal void DisableFlowControlForPendingCmdletActionsQueue()
            if (!_cmdletMode || _alreadyDisabledFlowControlForPendingCmdletActionsQueue)
            _alreadyDisabledFlowControlForPendingCmdletActionsQueue = true;
            long slotsToRelease = (long)(int.MaxValue / 2) - (long)(_jobResultsThrottlingSemaphore.CurrentCount);
            if ((slotsToRelease > 0) && (slotsToRelease < int.MaxValue))
                _jobResultsThrottlingSemaphore.Release((int)slotsToRelease);
        /// Adds and starts a child job.
        /// <param name="flags">Flags of the child job.</param>
        /// <param name="jobEnqueuedAction">Action to run after enqueuing the job.</param>
        /// Thrown when the child job is not in the <see cref="JobState.NotStarted"/> state.
        /// (because this can lead to race conditions - the child job can finish before the parent job has a chance to register for child job events)
        internal void AddChildJobWithoutBlocking(StartableJob childJob, ChildJobFlags flags, Action jobEnqueuedAction = null)
            if (childJob.JobStateInfo.State != JobState.NotStarted)
                throw new ArgumentException(RemotingErrorIdStrings.ThrottlingJobChildAlreadyRunning, nameof(childJob));
            this.AssertNotDisposed();
            JobStateInfo newJobStateInfo = null;
                if (this.IsEndOfChildJobs)
                    throw new InvalidOperationException(RemotingErrorIdStrings.ThrottlingJobChildAddedAfterEndOfChildJobs);
                if (_countOfAllChildJobs == 0)
                    newJobStateInfo = new JobStateInfo(JobState.Running);
                if ((ChildJobFlags.CreatesChildJobs & flags) == ChildJobFlags.CreatesChildJobs)
                    _setOfChildJobsThatCanAddMoreChildJobs.Add(childJob.InstanceId);
                this.ChildJobs.Add(childJob);
                _childJobLocations.Add(childJob.Location);
                _countOfAllChildJobs++;
                this.WriteWarningAboutHighUsageOfFlowControlBuffers(this.CountOfRunningOrReadyToRunChildJobs);
                if (this.CountOfRunningOrReadyToRunChildJobs > _maxReadyToRunJobs)
                    _actionsForUnblockingChildAdditions.Enqueue(jobEnqueuedAction);
                    jobEnqueuedAction?.Invoke();
            if (newJobStateInfo != null)
                this.SetJobState(newJobStateInfo.State, newJobStateInfo.Reason);
            this.ChildJobAdded.SafeInvoke(this, new ThrottlingJobChildAddedEventArgs(childJob));
            childJob.SetParentActivityIdGetter(this.GetProgressActivityId);
            childJob.StateChanged += this.childJob_StateChanged;
                childJob.Results.DataAdded += childJob_ResultsAdded;
            this.EnqueueReadyToRunChildJob(childJob);
            this.ReportProgress(minimizeFrequentUpdates: true);
        private void childJob_ResultsAdded(object sender, DataAddedEventArgs e)
            Dbg.Assert(_jobResultsThrottlingSemaphore != null, "JobResultsThrottlingSemaphore should be non-null if childJob_ResultsAdded handled is registered");
                long jobResultsUpdatedCount = Interlocked.Increment(ref _jobResultsCurrentCount);
                this.WriteWarningAboutHighUsageOfFlowControlBuffers(jobResultsUpdatedCount);
                _jobResultsThrottlingSemaphore.Wait(_cancellationTokenSource.Token);
        private readonly object _alreadyWroteFlowControlBuffersHighMemoryUsageWarningLock = new object();
        private bool _alreadyWroteFlowControlBuffersHighMemoryUsageWarning;
        private const long FlowControlBuffersHighMemoryUsageThreshold = 30000;
        private void WriteWarningAboutHighUsageOfFlowControlBuffers(long currentCount)
            if (!_cmdletMode)
            if (currentCount < FlowControlBuffersHighMemoryUsageThreshold)
            lock (_alreadyWroteFlowControlBuffersHighMemoryUsageWarningLock)
                if (_alreadyWroteFlowControlBuffersHighMemoryUsageWarning)
                _alreadyWroteFlowControlBuffersHighMemoryUsageWarning = true;
                RemotingErrorIdStrings.ThrottlingJobFlowControlMemoryWarning,
                this.Command);
        internal event EventHandler<ThrottlingJobChildAddedEventArgs> ChildJobAdded;
        private int _maximumConcurrentChildJobs;
        private int _extraCapacityForRunningQueryJobs;
        private int _extraCapacityForRunningAllJobs;
        private bool _inBoostModeToPreventQueryJobDeadlock;
        private Queue<StartableJob> _readyToRunQueryJobs;
        private Queue<StartableJob> _readyToRunRegularJobs;
        private Queue<Action> _actionsForUnblockingChildAdditions;
        private int _maxReadyToRunJobs;
        private readonly SemaphoreSlim _jobResultsThrottlingSemaphore;
        private long _jobResultsCurrentCount;
        private static readonly int s_maximumReadyToRunJobs = 10000;
        private void SetupThrottlingQueue(int maximumConcurrentChildJobs)
            _maximumConcurrentChildJobs = maximumConcurrentChildJobs > 0 ? maximumConcurrentChildJobs : int.MaxValue;
                _maxReadyToRunJobs = s_maximumReadyToRunJobs;
            _extraCapacityForRunningAllJobs = _maximumConcurrentChildJobs;
            _extraCapacityForRunningQueryJobs = Math.Max(1, _extraCapacityForRunningAllJobs / 2);
            _inBoostModeToPreventQueryJobDeadlock = false;
            _readyToRunQueryJobs = new Queue<StartableJob>();
            _readyToRunRegularJobs = new Queue<StartableJob>();
            _actionsForUnblockingChildAdditions = new Queue<Action>();
        private void StartChildJobIfPossible()
            StartableJob readyToRunChildJob = null;
                    if ((_readyToRunQueryJobs.Count > 0) &&
                        (_extraCapacityForRunningQueryJobs > 0) &&
                        (_extraCapacityForRunningAllJobs > 0))
                        _extraCapacityForRunningQueryJobs--;
                        _extraCapacityForRunningAllJobs--;
                        readyToRunChildJob = _readyToRunQueryJobs.Dequeue();
                    if ((_readyToRunRegularJobs.Count > 0) &&
                        readyToRunChildJob = _readyToRunRegularJobs.Dequeue();
            readyToRunChildJob?.StartJob();
        private void EnqueueReadyToRunChildJob(StartableJob childJob)
                bool isQueryJob = _setOfChildJobsThatCanAddMoreChildJobs.Contains(childJob.InstanceId);
                if (isQueryJob &&
                    !_inBoostModeToPreventQueryJobDeadlock &&
                    (_maximumConcurrentChildJobs == 1))
                    _inBoostModeToPreventQueryJobDeadlock = true;
                    _extraCapacityForRunningAllJobs++;
                if (isQueryJob)
                    _readyToRunQueryJobs.Enqueue(childJob);
                    _readyToRunRegularJobs.Enqueue(childJob);
            StartChildJobIfPossible();
        private void MakeRoomForRunningOtherJobs(Job completedChildJob)
                bool isQueryJob = _setOfChildJobsThatCanAddMoreChildJobs.Contains(completedChildJob.InstanceId);
                    _setOfChildJobsThatCanAddMoreChildJobs.Remove(completedChildJob.InstanceId);
                    _extraCapacityForRunningQueryJobs++;
                    if (_inBoostModeToPreventQueryJobDeadlock && (_setOfChildJobsThatCanAddMoreChildJobs.Count == 0))
        private void FigureOutIfThrottlingJobIsCompleted()
            JobStateInfo finalJobStateInfo = null;
                if (this.IsThrottlingJobCompleted && !IsFinishedState(this.JobStateInfo.State))
                        finalJobStateInfo = new JobStateInfo(JobState.Stopped, null);
                    else if (_countOfFailedChildJobs > 0)
                        finalJobStateInfo = new JobStateInfo(JobState.Failed, null);
                    else if (_countOfStoppedChildJobs > 0)
                        finalJobStateInfo = new JobStateInfo(JobState.Completed);
            if (finalJobStateInfo != null)
                this.SetJobState(finalJobStateInfo.State, finalJobStateInfo.Reason);
        /// Notifies this <see cref="ThrottlingJob"/> object that no more child jobs will be added.
        internal void EndOfChildJobs()
                _ownerWontSubmitNewChildJobs = true;
            this.FigureOutIfThrottlingJobIsCompleted();
        /// Stop this job object and all the <see cref="System.Management.Automation.Job.ChildJobs"/>.
            List<Job> childJobsToStop = null;
                if (!(_isStopping || this.IsThrottlingJobCompleted))
                    childJobsToStop = this.GetChildJobsSnapshot();
            if (childJobsToStop != null)
                foreach (Job childJob in childJobsToStop)
                    if (!childJob.IsFinishedState(childJob.JobStateInfo.State))
                        childJob.StopJob();
        private void childJob_StateChanged(object sender, JobStateEventArgs e)
            Dbg.Assert(sender != null, "Only our internal implementation of Job should raise this event and it should make sure that sender != null");
            Dbg.Assert(sender is Job, "Only our internal implementation of Job should raise this event and it should make sure that sender is Job");
            var childJob = (Job)sender;
            if ((e.PreviousJobStateInfo.State == JobState.Blocked) && (e.JobStateInfo.State != JobState.Blocked))
                bool parentJobGotUnblocked = false;
                    _countOfBlockedChildJobs--;
                    if (_countOfBlockedChildJobs == 0)
                        parentJobGotUnblocked = true;
                if (parentJobGotUnblocked)
                // intermediate states
                case JobState.Blocked:
                        _countOfBlockedChildJobs++;
                // 3 finished states
                    childJob.StateChanged -= childJob_StateChanged;
                    this.MakeRoomForRunningOtherJobs(childJob);
                            _countOfFailedChildJobs++;
                        else if (e.JobStateInfo.State == JobState.Stopped)
                            _countOfStoppedChildJobs++;
                        else if (e.JobStateInfo.State == JobState.Completed)
                            _countOfSuccessfullyCompletedChildJobs++;
                        if (_actionsForUnblockingChildAdditions.Count > 0)
                            foreach (PSStreamObject streamObject in childJob.Results.ReadAll())
                                this.Results.Add(streamObject);
                            this.ChildJobs.Remove(childJob);
                            _setOfChildJobsThatCanAddMoreChildJobs.Remove(childJob.InstanceId);
                    this.ReportProgress(minimizeFrequentUpdates: !this.IsThrottlingJobCompleted);
        private List<Job> GetChildJobsSnapshot()
                return new List<Job>(this.ChildJobs);
        /// <see langword="true"/> if any of the child jobs have more data OR if <see cref="EndOfChildJobs"/> have not been called yet;
                return this.GetChildJobsSnapshot().Any(static childJob => childJob.HasMoreData) || (this.Results.Count != 0);
        /// Comma-separated list of locations of <see cref="System.Management.Automation.Job.ChildJobs"/>.
                    return string.Join(", ", _childJobLocations);
        private readonly HashSet<string> _childJobLocations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                int completedChildJobs;
                int totalChildJobs;
                    completedChildJobs = this.CountOfFinishedChildJobs;
                    totalChildJobs = _countOfAllChildJobs;
                string totalChildJobsString = totalChildJobs.ToString(CultureInfo.CurrentCulture);
                if (!this.IsEndOfChildJobs)
                    totalChildJobsString += "+";
                    RemotingErrorIdStrings.ThrottlingJobStatusMessage,
                    completedChildJobs,
                    totalChildJobsString);
        #region Forwarding results to a cmdlet
        internal override void ForwardAvailableResultsToCmdlet(Cmdlet cmdlet)
            base.ForwardAvailableResultsToCmdlet(cmdlet);
            foreach (Job childJob in this.GetChildJobsSnapshot())
                childJob.ForwardAvailableResultsToCmdlet(cmdlet);
        private sealed class ForwardingHelper : IDisposable
            // This is higher than 1000 used in
            //      RxExtensionMethods+ToEnumerableObserver<T>.BlockingCollectionCapacity
            // and in
            //      RemoteDiscoveryHelper.BlockingCollectionCapacity
            // It needs to be higher, because the high value is used as an attempt to workaround the fact that
            // WSMan will timeout if an OnNext call blocks for more than X minutes.
            internal static readonly int AggregationQueueMaxCapacity = 10000;
            private readonly ThrottlingJob _throttlingJob;
            private readonly object _myLock;
            private readonly BlockingCollection<PSStreamObject> _aggregatedResults;
            private readonly HashSet<Job> _monitoredJobs;
            private ForwardingHelper(ThrottlingJob throttlingJob)
                _throttlingJob = throttlingJob;
                _myLock = new object();
                _monitoredJobs = new HashSet<Job>();
                _aggregatedResults = new BlockingCollection<PSStreamObject>();
            private void StartMonitoringJob(Job job)
                    if (_disposed || _stoppedMonitoringAllJobs)
                    if (_monitoredJobs.Contains(job))
                    _monitoredJobs.Add(job);
                    job.Results.DataAdded += this.MonitoredJobResults_DataAdded;
                    job.StateChanged += MonitoredJob_StateChanged;
                this.AggregateJobResults(job.Results);
                this.CheckIfMonitoredJobIsComplete(job);
            private void StopMonitoringJob(Job job)
                        job.Results.DataAdded -= this.MonitoredJobResults_DataAdded;
                        job.StateChanged -= this.MonitoredJob_StateChanged;
                        _monitoredJobs.Remove(job);
            private void AggregateJobResults(PSDataCollection<PSStreamObject> resultsCollection)
                    // try not to remove results from a job, unless it seems safe ...
                    if (_disposed || _stoppedMonitoringAllJobs || _aggregatedResults.IsAddingCompleted || _cancellationTokenSource.IsCancellationRequested)
                // ... and after removing the results via ReadAll, we have to make sure that we don't drop them ...
                foreach (var result in resultsCollection.ReadAll())
                    bool successfullyAggregatedResult = false;
                            if (!(_disposed || _stoppedMonitoringAllJobs || _aggregatedResults.IsAddingCompleted || _cancellationTokenSource.IsCancellationRequested))
                                _aggregatedResults.Add(result, _cancellationTokenSource.Token);
                                successfullyAggregatedResult = true;
                    catch (Exception) // BlockingCollection.Add can throw undocumented exceptions - we cannot just catch InvalidOperationException
                    // ... so if _aggregatedResults is not accepting new results, we will store them in the throttling job
                    if (!successfullyAggregatedResult)
                        this.StopMonitoringJob(_throttlingJob);
                            _throttlingJob.Results.Add(result);
                            Dbg.Assert(false, "ThrottlingJob.Results was already closed when trying to preserve results aggregated by ForwardingHelper");
            private void CancelForwarding()
                    Dbg.Assert(!_disposed, "CancelForwarding should be unregistered before ForwardingHelper gets disposed");
                    _aggregatedResults.CompleteAdding();
            private void CheckIfMonitoredJobIsComplete(Job job)
                CheckIfMonitoredJobIsComplete(job, job.JobStateInfo.State);
            private void CheckIfMonitoredJobIsComplete(Job job, JobState jobState)
                if (job.IsFinishedState(jobState))
                        this.StopMonitoringJob(job);
            private void CheckIfThrottlingJobIsComplete()
                if (_throttlingJob.IsThrottlingJobCompleted)
                    List<PSDataCollection<PSStreamObject>> resultsToAggregate = new List<PSDataCollection<PSStreamObject>>();
                        foreach (Job registeredJob in _monitoredJobs)
                            resultsToAggregate.Add(registeredJob.Results);
                        foreach (Job throttledJob in _throttlingJob.GetChildJobsSnapshot())
                            resultsToAggregate.Add(throttledJob.Results);
                        resultsToAggregate.Add(_throttlingJob.Results);
                    foreach (PSDataCollection<PSStreamObject> resultToAggregate in resultsToAggregate)
                        this.AggregateJobResults(resultToAggregate);
                        if (!_disposed && !_aggregatedResults.IsAddingCompleted)
            private void MonitoredJobResults_DataAdded(object sender, DataAddedEventArgs e)
                var resultsCollection = (PSDataCollection<PSStreamObject>)sender;
                this.AggregateJobResults(resultsCollection);
            private void MonitoredJob_StateChanged(object sender, JobStateEventArgs e)
                var job = (Job)sender;
                this.CheckIfMonitoredJobIsComplete(job, e.JobStateInfo.State);
            private void ThrottlingJob_ChildJobAdded(object sender, ThrottlingJobChildAddedEventArgs e)
                this.StartMonitoringJob(e.AddedChildJob);
            private void ThrottlingJob_StateChanged(object sender, JobStateEventArgs e)
                this.CheckIfThrottlingJobIsComplete();
            private void AttemptToPreserveAggregatedResults()
                    Dbg.Assert(!_disposed, "AttemptToPreserveAggregatedResults should be called before disposing ForwardingHelper");
                    Dbg.Assert(_stoppedMonitoringAllJobs, "Caller should guarantee no-more-results before calling AttemptToPreserveAggregatedResults (1)");
                    Dbg.Assert(_aggregatedResults.IsAddingCompleted, "Caller should guarantee no-more-results before calling AttemptToPreserveAggregatedResults (2)");
                bool isThrottlingJobFinished = false;
                foreach (var aggregatedButNotYetProcessedResult in _aggregatedResults)
                    if (!isThrottlingJobFinished)
                            _throttlingJob.Results.Add(aggregatedButNotYetProcessedResult);
                            isThrottlingJobFinished = _throttlingJob.IsFinishedState(_throttlingJob.JobStateInfo.State);
                            Dbg.Assert(isThrottlingJobFinished, "Buffers should not be closed before throttling job is stopped");
            // CDXML_CLIXML_TEST testability hook
            private static readonly bool s_isCliXmlTestabilityHookActive = GetIsCliXmlTestabilityHookActive();
            internal static void ProcessCliXmlTestabilityHook(PSStreamObject streamObject)
                if (!s_isCliXmlTestabilityHookActive)
                if (streamObject.ObjectType != PSStreamObjectType.Output)
                if (streamObject.Value == null)
                if (!(PSObject.AsPSObject(streamObject.Value).BaseObject.GetType().Name.Equals("CimInstance")))
                string serializedForm = PSSerializer.Serialize(streamObject.Value, depth: 1);
                streamObject.Value = PSObject.AsPSObject(deserializedObject).BaseObject;
            private void ForwardResults(Cmdlet cmdlet)
                    foreach (var result in _aggregatedResults.GetConsumingEnumerable(_throttlingJob._cancellationTokenSource.Token))
                            ProcessCliXmlTestabilityHook(result);
                                result.WriteStreamObject(cmdlet);
                                if (_throttlingJob._cmdletMode)
                                    Dbg.Assert(_throttlingJob._jobResultsThrottlingSemaphore != null, "JobResultsThrottlingSemaphore should be present in cmdlet mode");
                                    Interlocked.Decrement(ref _throttlingJob._jobResultsCurrentCount);
                                    _throttlingJob._jobResultsThrottlingSemaphore.Release();
                    this.StopMonitoringAllJobs();
                    this.AttemptToPreserveAggregatedResults();
            private bool _stoppedMonitoringAllJobs;
            private void StopMonitoringAllJobs()
                    _stoppedMonitoringAllJobs = true;
                    List<Job> snapshotOfCurrentlyMonitoredJobs = _monitoredJobs.ToList();
                    foreach (Job monitoredJob in snapshotOfCurrentlyMonitoredJobs)
                        this.StopMonitoringJob(monitoredJob);
                    Dbg.Assert(_monitoredJobs.Count == 0, "No monitored jobs should be left after ForwardingHelper is disposed");
                    _aggregatedResults.Dispose();
            public static void ForwardAllResultsToCmdlet(ThrottlingJob throttlingJob, Cmdlet cmdlet, CancellationToken? cancellationToken)
                using (var helper = new ForwardingHelper(throttlingJob))
                        throttlingJob.ChildJobAdded += helper.ThrottlingJob_ChildJobAdded;
                            throttlingJob.StateChanged += helper.ThrottlingJob_StateChanged;
                            IDisposable cancellationTokenRegistration = null;
                            if (cancellationToken.HasValue)
                                cancellationTokenRegistration = cancellationToken.Value.Register(helper.CancelForwarding);
                                            helper.StartMonitoringJob(throttlingJob);
                                            foreach (Job childJob in throttlingJob.GetChildJobsSnapshot())
                                                helper.StartMonitoringJob(childJob);
                                            helper.CheckIfThrottlingJobIsComplete();
                                helper.ForwardResults(cmdlet);
                                cancellationTokenRegistration?.Dispose();
                            throttlingJob.StateChanged -= helper.ThrottlingJob_StateChanged;
                        throttlingJob.ChildJobAdded -= helper.ThrottlingJob_ChildJobAdded;
        internal override void ForwardAllResultsToCmdlet(Cmdlet cmdlet)
            this.ForwardAllResultsToCmdlet(cmdlet, cancellationToken: null);
        private void ForwardAllResultsToCmdlet(Cmdlet cmdlet, CancellationToken? cancellationToken)
            ForwardingHelper.ForwardAllResultsToCmdlet(this, cmdlet, cancellationToken);
        #endregion Forwarding results to a cmdlet
    internal class ThrottlingJobChildAddedEventArgs : EventArgs
        internal Job AddedChildJob { get; }
        internal ThrottlingJobChildAddedEventArgs(Job addedChildJob)
            Dbg.Assert(addedChildJob != null, "Caller should verify addedChildJob != null");
            AddedChildJob = addedChildJob;

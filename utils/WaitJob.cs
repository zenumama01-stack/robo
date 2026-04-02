    /// This cmdlet waits for job to complete.
    [Cmdlet(VerbsLifecycle.Wait, "Job", DefaultParameterSetName = JobCmdletBase.SessionIdParameterSet, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096902")]
    public class WaitJobCommand : JobCmdletBase, IDisposable
        public Job[] Job { get; set; }
        /// Complete the cmdlet when any of the job is completed, instead of waiting for all of them to be completed.
        /// Forces the cmdlet to wait for Finished states (Completed, Failed, Stopped) instead of
        /// persistent states, which also include Suspended and Disconnected.
        public override string[] Command { get; set; }
        #region Coordinating how different events (timeout, stopprocessing, job finished, job blocked) affect what happens in EndProcessing
        private readonly object _endProcessingActionLock = new object();
        private Action _endProcessingAction;
        private readonly ManualResetEventSlim _endProcessingActionIsReady = new ManualResetEventSlim(false);
        private void SetEndProcessingAction(Action endProcessingAction)
            Dbg.Assert(endProcessingAction != null, "Caller should verify endProcessingAction != null");
            lock (_endProcessingActionLock)
                if (_endProcessingAction == null)
                    Dbg.Assert(!_endProcessingActionIsReady.IsSet, "This line should execute only once");
                    _endProcessingAction = endProcessingAction;
                    _endProcessingActionIsReady.Set();
        private void InvokeEndProcessingAction()
            _endProcessingActionIsReady.Wait();
            Action endProcessingAction;
                endProcessingAction = _endProcessingAction;
            // Invoke action outside lock.
            endProcessingAction?.Invoke();
        private void CleanUpEndProcessing()
            _endProcessingActionIsReady.Dispose();
        #region Support for triggering EndProcessing when jobs are finished or blocked
        private readonly HashSet<Job> _finishedJobs = new HashSet<Job>();
        private readonly HashSet<Job> _blockedJobs = new HashSet<Job>();
        private readonly List<Job> _jobsToWaitFor = new List<Job>();
        private readonly object _jobTrackingLock = new object();
        private void HandleJobStateChangedEvent(object source, JobStateEventArgs eventArgs)
            Dbg.Assert(source is Job, "Caller should verify source is Job");
            Dbg.Assert(eventArgs != null, "Caller should verify eventArgs != null");
            var job = (Job)source;
            lock (_jobTrackingLock)
                Dbg.Assert(_blockedJobs.All(j => !_finishedJobs.Contains(j)), "Job cannot be in *both* _blockedJobs and _finishedJobs");
                if (eventArgs.JobStateInfo.State == JobState.Blocked)
                    _blockedJobs.Add(job);
                    _blockedJobs.Remove(job);
                // Treat jobs in Disconnected state as finished jobs since the user
                // will have to reconnect the job before more information can be
                // obtained.
                // Suspended jobs require a Resume-Job call. Both of these states are persistent
                // without user interaction.
                // Wait should wait until a job is in a persistent state, OR if the force parameter
                // is specified, until the job is in a finished state, which is a subset of
                // persistent states.
                if (!Force && job.IsPersistentState(eventArgs.JobStateInfo.State) || (Force && job.IsFinishedState(eventArgs.JobStateInfo.State)))
                    if (!job.IsFinishedState(eventArgs.JobStateInfo.State))
                        _warnNotTerminal = true;
                    _finishedJobs.Add(job);
                    _finishedJobs.Remove(job);
                if (this.Any.IsPresent)
                    if (_finishedJobs.Count > 0)
                        this.SetEndProcessingAction(this.EndProcessingOutputSingleFinishedJob);
                    else if (_blockedJobs.Count == _jobsToWaitFor.Count)
                        this.SetEndProcessingAction(this.EndProcessingBlockedJobsError);
                    if (_finishedJobs.Count == _jobsToWaitFor.Count)
                        this.SetEndProcessingAction(this.EndProcessingOutputAllFinishedJobs);
                    else if (_blockedJobs.Count > 0)
        private void AddJobsThatNeedJobChangesTracking(IEnumerable<Job> jobsToAdd)
            Dbg.Assert(jobsToAdd != null, "Caller should verify jobs != null");
                _jobsToWaitFor.AddRange(jobsToAdd);
        private void StartJobChangesTracking()
                if (_jobsToWaitFor.Count == 0)
                    this.SetEndProcessingAction(this.EndProcessingDoNothing);
                foreach (Job job in _jobsToWaitFor)
                    job.StateChanged += this.HandleJobStateChangedEvent;
                    this.HandleJobStateChangedEvent(job, new JobStateEventArgs(job.JobStateInfo));
        private void CleanUpJobChangesTracking()
                    job.StateChanged -= this.HandleJobStateChangedEvent;
        private List<Job> GetFinishedJobs()
            List<Job> jobsToOutput;
                jobsToOutput = _jobsToWaitFor.Where(j => ((!Force && j.IsPersistentState(j.JobStateInfo.State)) || (Force && j.IsFinishedState(j.JobStateInfo.State)))).ToList();
            return jobsToOutput;
        private Job GetOneBlockedJob()
                return _jobsToWaitFor.Find(static j => j.JobStateInfo.State == JobState.Blocked);
        #region Support for triggering EndProcessing when timing out
        private readonly object _timerLock = new object();
        private void StartTimeoutTracking(int timeoutInSeconds)
            if (timeoutInSeconds == 0)
            else if (timeoutInSeconds > 0)
                lock (_timerLock)
                    _timer = new Timer((_) => this.SetEndProcessingAction(this.EndProcessingDoNothing), null, timeoutInSeconds * 1000, System.Threading.Timeout.Infinite);
        private void CleanUpTimeoutTracking()
                if (_timer != null)
                    _timer.Dispose();
                    _timer = null;
        /// Cancel the Wait-Job cmdlet.
        /// In this method, we initialize the timer if timeout parameter is specified.
            this.StartTimeoutTracking(_timeoutInSeconds);
        /// This method just collects the Jobs which will be waited on in the EndProcessing method.
            // List of jobs to wait
            List<Job> matches;
                    matches = FindJobsMatchingByName(true, false, true, false);
                    matches = FindJobsMatchingByInstanceId(true, false, true, false);
                    matches = FindJobsMatchingBySessionId(true, false, true, false);
                    matches = FindJobsMatchingByState(false);
                    matches = FindJobsMatchingByFilter(false);
                    matches = CopyJobsToList(this.Job, false, false);
            this.AddJobsThatNeedJobChangesTracking(matches);
        /// Wait on the collected Jobs.
            this.StartJobChangesTracking();
            this.InvokeEndProcessingAction();
            if (_warnNotTerminal)
                WriteWarning(RemotingErrorIdStrings.JobSuspendedDisconnectedWaitWithForce);
        private void EndProcessingOutputSingleFinishedJob()
            Job finishedJob = this.GetFinishedJobs().FirstOrDefault();
            if (finishedJob != null)
                this.WriteObject(finishedJob);
        private void EndProcessingOutputAllFinishedJobs()
            IEnumerable<Job> finishedJobs = this.GetFinishedJobs();
            foreach (Job finishedJob in finishedJobs)
        private void EndProcessingBlockedJobsError()
            string message = RemotingErrorIdStrings.JobBlockedSoWaitJobCannotContinue;
                "BlockedJobsDeadlockWithWaitJob",
                ErrorCategory.DeadlockDetected,
                this.GetOneBlockedJob());
        private void EndProcessingDoNothing()
                lock (_disposableLock)
                        this.CleanUpTimeoutTracking();
                        this.CleanUpJobChangesTracking();
                        this.CleanUpEndProcessing(); // <- has to be last
        private readonly object _disposableLock = new object();
        private bool _warnNotTerminal = false;

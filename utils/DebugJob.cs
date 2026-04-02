    /// This cmdlet takes a Job object and checks to see if it is debuggable.  If it
    /// is debuggable then it breaks into the job debugger in step mode.  If it is not
    /// debuggable then it is treated as a parent job and each child job is checked if
    /// it is debuggable and if it is will break into its job debugger in step mode.
    /// For multiple debuggable child jobs, each job execution will be halted and the
    /// debugger will step to each job execution point sequentially.
    /// When a job is debugged its output data is written to host and the executing job
    /// script will break into the host debugger, in step mode, at the next stoppable
    /// execution point.
    [Cmdlet(VerbsDiagnostic.Debug, "Job", SupportsShouldProcess = true, DefaultParameterSetName = DebugJobCommand.JobParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkId=330208")]
    public sealed class DebugJobCommand : PSCmdlet
        private const string JobParameterSet = "JobParameterSet";
        private const string JobNameParameterSet = "JobNameParameterSet";
        private const string JobIdParameterSet = "JobIdParameterSet";
        private const string JobInstanceIdParameterSet = "JobInstanceIdParameterSet";
        private Job _job;
        private Debugger _debugger;
        private PSDataCollection<PSStreamObject> _debugCollection;
        /// The Job object to be debugged.
                   ParameterSetName = DebugJobCommand.JobParameterSet)]
        public Job Job
        /// The Job object name to be debugged.
                   ParameterSetName = DebugJobCommand.JobNameParameterSet)]
        /// The Job object Id to be debugged.
                   ParameterSetName = DebugJobCommand.JobIdParameterSet)]
        /// The Job object InstanceId to be debugged.
                   ParameterSetName = DebugJobCommand.JobInstanceIdParameterSet)]
                case DebugJobCommand.JobParameterSet:
                    _job = Job;
                case DebugJobCommand.JobNameParameterSet:
                    _job = GetJobByName(Name);
                case DebugJobCommand.JobIdParameterSet:
                    _job = GetJobById(Id);
                case DebugJobCommand.JobInstanceIdParameterSet:
                    _job = GetJobByInstanceId(InstanceId);
            if (!ShouldProcess(_job.Name, VerbsDiagnostic.Debug))
            Runspace runspace = LocalRunspace.DefaultRunspace;
                        new PSInvalidOperationException(RemotingErrorIdStrings.CannotDebugJobNoHostDebugger),
                        "DebugJobNoHostDebugger",
            if ((runspace.Debugger.DebugMode == DebugModes.Default) || (runspace.Debugger.DebugMode == DebugModes.None))
                        new PSInvalidOperationException(RemotingErrorIdStrings.CannotDebugJobInvalidDebuggerMode),
                        "DebugJobWrongDebugMode",
                        new PSInvalidOperationException(RemotingErrorIdStrings.CannotDebugJobNoHostUI),
                        "DebugJobNoHostAvailable",
            if (!CheckForDebuggableJob())
                        new PSInvalidOperationException(DebuggerStrings.NoDebuggableJobsFound),
                        "DebugJobNoDebuggableJobsFound",
            // Set up host script debugger to debug the job.
            _debugger = runspace.Debugger;
            _debugger.DebugJob(_job, breakAll: BreakAll);
            // Blocking call.  Send job output to host UI while debugging and wait for Job completion.
            WaitAndReceiveJobOutput();
            // Cancel job debugging.
            Debugger debugger = _debugger;
            if ((debugger != null) && (_job != null))
                debugger.StopDebugJob(_job);
            PSDataCollection<PSStreamObject> debugCollection = _debugCollection;
        /// Check for debuggable job.  Job must implement IJobDebugger and also
        /// must be running or in Debug stopped state.
        private bool CheckForDebuggableJob()
            // Check passed in job object.
            bool debuggableJobFound = GetJobDebuggable(_job);
            if (!debuggableJobFound)
                // Assume passed in job is a container job and check child jobs.
                foreach (var cJob in _job.ChildJobs)
                    debuggableJobFound = GetJobDebuggable(cJob);
                    if (debuggableJobFound)
            return debuggableJobFound;
        private static bool GetJobDebuggable(Job job)
            if (job is IJobDebugger)
                return ((job.JobStateInfo.State == JobState.Running) ||
                        (job.JobStateInfo.State == JobState.AtBreakpoint));
        private void WaitAndReceiveJobOutput()
            _debugCollection = new PSDataCollection<PSStreamObject>();
            _debugCollection.BlockingEnumerator = true;
                AddEventHandlers();
                // This call blocks (blocking enumerator) until the job completes
                // or this command is cancelled.
                foreach (var streamItem in _debugCollection)
                    streamItem?.WriteStreamObject(this);
                // Terminate job on exception.
                if (!_job.IsFinishedState(_job.JobStateInfo.State))
                RemoveEventHandlers();
                _debugCollection = null;
        private void HandleJobStateChangedEvent(object sender, JobStateEventArgs stateChangedArgs)
            if (job.IsFinishedState(stateChangedArgs.JobStateInfo.State))
                _debugCollection.Complete();
        private void HandleResultsDataAdding(object sender, DataAddingEventArgs dataAddingArgs)
            if (_debugCollection.IsOpen)
                PSStreamObject streamObject = dataAddingArgs.ItemAdded as PSStreamObject;
                if (streamObject != null)
                        _debugCollection.Add(streamObject);
        private void AddEventHandlers()
            _job.StateChanged += HandleJobStateChangedEvent;
            if (_job.ChildJobs.Count == 0)
                // No child jobs, monitor this job's results collection.
                _job.Results.DataAdding += HandleResultsDataAdding;
                // Monitor each child job's results collections.
                foreach (var childJob in _job.ChildJobs)
                    childJob.Results.DataAdding += HandleResultsDataAdding;
        private void RemoveEventHandlers()
            _job.StateChanged -= HandleJobStateChangedEvent;
                // Remove single job DataAdding event handler.
                _job.Results.DataAdding -= HandleResultsDataAdding;
                // Remove each child job's DataAdding event handler.
                    childJob.Results.DataAdding -= HandleResultsDataAdding;
        private Job GetJobByName(string name)
            // Search jobs in job repository.
            List<Job> jobs1 = new List<Job>();
                WildcardPattern.Get(name, WildcardOptions.IgnoreCase | WildcardOptions.Compiled);
            foreach (Job job in JobRepository.Jobs)
                if (pattern.IsMatch(job.Name))
                    jobs1.Add(job);
            // Search jobs in job manager.
            List<Job2> jobs2 = JobManager.GetJobsByName(name, this, false, false, false, null);
            int jobCount = jobs1.Count + jobs2.Count;
            if (jobCount == 1)
                return (jobs1.Count > 0) ? jobs1[0] : jobs2[0];
            if (jobCount > 1)
                        new PSInvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.FoundMultipleJobsWithName, name)),
                        "DebugJobFoundMultipleJobsWithName",
                    new PSInvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.CannotFindJobWithName, name)),
                    "DebugJobCannotFindJobWithName",
        private Job GetJobById(int id)
                if (job.Id == id)
            Job job2 = JobManager.GetJobById(id, this, false, false, false);
            if ((jobs1.Count == 0) && (job2 == null))
                        new PSInvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.CannotFindJobWithId, id)),
                        "DebugJobCannotFindJobWithId",
            if ((jobs1.Count > 1) ||
                (jobs1.Count == 1) && (job2 != null))
                        new PSInvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.FoundMultipleJobsWithId, id)),
                        "DebugJobFoundMultipleJobsWithId",
            return (jobs1.Count > 0) ? jobs1[0] : job2;
        private Job GetJobByInstanceId(Guid instanceId)
                if (job.InstanceId == instanceId)
            Job2 job2 = JobManager.GetJobByInstanceId(instanceId, this, false, false, false);
            if (job2 != null)
                return job2;
                    new PSInvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.CannotFindJobWithInstanceId, instanceId)),
                    "DebugJobCannotFindJobWithInstanceId",

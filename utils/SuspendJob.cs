    /// This cmdlet suspends the jobs that are Job2. Errors are added for each Job that is not Job2.
    [Cmdlet(VerbsLifecycle.Suspend, "Job", SupportsShouldProcess = true, DefaultParameterSetName = JobCmdletBase.SessionIdParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=210613")]
    public class SuspendJobCommand : JobCmdletBase, IDisposable
        /// If state of the job is running , this will forcefully suspend it.
        [Parameter(ParameterSetName = RemoveJobCommand.StateParameterSet)]
        private bool _wait = false;
        /// Suspend the Job.
            // List of jobs to suspend
            List<Job> jobsToSuspend = null;
                        jobsToSuspend = FindJobsMatchingByName(true, false, true, false);
                        jobsToSuspend = FindJobsMatchingByInstanceId(true, false, true, false);
                        jobsToSuspend = FindJobsMatchingBySessionId(true, false, true, false);
                        jobsToSuspend = FindJobsMatchingByState(false);
                        jobsToSuspend = FindJobsMatchingByFilter(false);
                        jobsToSuspend = CopyJobsToList(_jobs, false, false);
            _allJobsToSuspend.AddRange(jobsToSuspend);
            foreach (Job job in jobsToSuspend)
                // If the job is not Job2, the suspend operation is not supported.
                            PSTraceSource.NewNotSupportedException(RemotingErrorIdStrings.JobSuspendNotSupported, job.Id),
                            "Job2OperationNotSupportedOnJob", ErrorCategory.InvalidType, (object)job));
                if (ShouldProcess(targetString, VerbsLifecycle.Suspend))
                        _cleanUpActions.Add(job2, HandleSuspendJobCompleted);
                        if (job2.IsFinishedState(job2.JobStateInfo.State) || job2.JobStateInfo.State == JobState.Stopping)
                        if (job2.JobStateInfo.State == JobState.Suspending || job2.JobStateInfo.State == JobState.Suspended)
                        job2.StateChanged += noWait_Job2_StateChanged;
                    job2.SuspendJobCompleted += HandleSuspendJobCompleted;
                    // there could be possibility that the job gets completed before or after the
                    // subscribing to nowait_job2_statechanged event so checking it again.
                    if (!_wait && (job2.IsFinishedState(job2.JobStateInfo.State) || job2.JobStateInfo.State == JobState.Suspending || job2.JobStateInfo.State == JobState.Suspended))
                        this.ProcessExecutionErrorsAndReleaseWaitHandle(job2);
                    job2.SuspendJobAsync(_force, RemotingErrorIdStrings.ForceSuspendJob);
        private readonly List<Job> _allJobsToSuspend = new List<Job>();
        private void noWait_Job2_StateChanged(object sender, JobStateEventArgs e)
                case JobState.Suspending:
                    this.ProcessExecutionErrorsAndReleaseWaitHandle(job);
        private void HandleSuspendJobCompleted(object sender, AsyncCompletedEventArgs eventArgs)
        private void ProcessExecutionErrorsAndReleaseWaitHandle(Job job)
                    // there could be a possibility of race condition where this function is getting called twice
                    // so if job doesn't present in the _pendingJobs then just return
            if (!_wait)
                job.StateChanged -= noWait_Job2_StateChanged;
                    job2.SuspendJobCompleted -= HandleSuspendJobCompleted;
                        parentJob.ExecutionError.Where(static e => e.FullyQualifiedErrorId == "ContainerParentJobSuspendAsyncError")
                WriteWarning(RemotingErrorIdStrings.SuspendJobInvalidJobState);
            foreach (var j in _allJobsToSuspend)
                pair.Key.SuspendJobCompleted -= pair.Value;

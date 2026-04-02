    /// This cmdlet resumes the jobs that are Job2. Errors are added for each Job that is not Job2.
    [Cmdlet(VerbsLifecycle.Resume, "Job", SupportsShouldProcess = true, DefaultParameterSetName = JobCmdletBase.SessionIdParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=210611")]
    public class ResumeJobCommand : JobCmdletBase, IDisposable
        /// suspended.
                   ParameterSetName = JobParameterSet)]
        /// Specifies whether to delay returning from the cmdlet until all jobs reach a running state.
        /// This could take significant time due to workflow throttling.
        [Parameter(ParameterSetName = ParameterAttribute.AllParameterSets)]
        /// Resume the Job.
            // List of jobs to resume
            List<Job> jobsToResume = null;
                        jobsToResume = FindJobsMatchingByName(true, false, true, false);
                        jobsToResume = FindJobsMatchingByInstanceId(true, false, true, false);
                        jobsToResume = FindJobsMatchingBySessionId(true, false, true, false);
                        jobsToResume = FindJobsMatchingByState(false);
                        jobsToResume = FindJobsMatchingByFilter(false);
                        jobsToResume = CopyJobsToList(_jobs, false, false);
            _allJobsToResume.AddRange(jobsToResume);
            // Blue: 151804 When resuming a single suspended workflow job, Resume-job cmdlet doesn't wait for the job to be in running state
            // Setting Wait to true so that this cmdlet will wait for the running job state.
            if (_allJobsToResume.Count == 1)
                Wait = true;
            foreach (Job job in jobsToResume)
                var job2 = job as Job2;
                // If the job is not Job2, the resume operation is not supported.
                if (job2 == null)
                    WriteError(new ErrorRecord(PSTraceSource.NewNotSupportedException(RemotingErrorIdStrings.JobResumeNotSupported, job.Id), "Job2OperationNotSupportedOnJob", ErrorCategory.InvalidType, (object)job));
                string targetString = PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.RemovePSJobWhatIfTarget, job.Command, job.Id);
                if (ShouldProcess(targetString, VerbsLifecycle.Resume))
                    _cleanUpActions.Add(job2, HandleResumeJobCompleted);
                    job2.ResumeJobCompleted += HandleResumeJobCompleted;
                        if (!_pendingJobs.Contains(job2.InstanceId))
                    job2.ResumeJobAsync();
        private bool _warnInvalidState = false;
        private readonly List<ErrorRecord> _errorsToWrite = new List<ErrorRecord>();
        private readonly List<Job> _allJobsToResume = new List<Job>();
        private void HandleResumeJobCompleted(object sender, AsyncCompletedEventArgs eventArgs)
            if (eventArgs.Error != null && eventArgs.Error is InvalidJobStateException)
                _warnInvalidState = true;
            var parentJob = job as ContainerParentJob;
            if (parentJob != null && parentJob.ExecutionError.Count > 0)
                    var e in
                        parentJob.ExecutionError.Where(static e => e.FullyQualifiedErrorId == "ContainerParentJobResumeAsyncError")
                    if (e.Exception is InvalidJobStateException)
                        // if any errors were invalid job state exceptions, warn the user.
                        // This is to support Get-Job | Resume-Job scenarios when many jobs
                        // are Completed, etc.
                        _errorsToWrite.Add(e);
                parentJob.ExecutionError.Clear();
            bool jobsPending = false;
                    jobsPending = true;
            if (Wait && jobsPending)
            if (_warnInvalidState)
                WriteWarning(RemotingErrorIdStrings.ResumeJobInvalidJobState);
            foreach (var e in _errorsToWrite)
                WriteError(e);
            foreach (var j in _allJobsToResume)
                WriteObject(j);
                pair.Key.ResumeJobCompleted -= pair.Value;

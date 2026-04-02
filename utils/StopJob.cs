    /// This cmdlet stops the asynchronously invoked remote operations.
    [Cmdlet(VerbsLifecycle.Stop, "Job", SupportsShouldProcess = true, DefaultParameterSetName = JobCmdletBase.SessionIdParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096795")]
    public class StopJobCommand : JobCmdletBase, IDisposable
        /// Pass the Job object through the pipeline.
        /// Stop the Job.
            // List of jobs to stop
            List<Job> jobsToStop = null;
                        jobsToStop = FindJobsMatchingByName(true, false, true, false);
                        jobsToStop = FindJobsMatchingByInstanceId(true, false, true, false);
                        jobsToStop = FindJobsMatchingBySessionId(true, false, true, false);
                        jobsToStop = FindJobsMatchingByState(false);
                        jobsToStop = FindJobsMatchingByFilter(false);
                        jobsToStop = CopyJobsToList(_jobs, false, false);
            _allJobsToStop.AddRange(jobsToStop);
            foreach (Job job in jobsToStop)
                if (this.Stopping)
                string targetString =
                    PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.RemovePSJobWhatIfTarget,
                                                                   job.Command, job.Id);
                if (ShouldProcess(targetString, VerbsLifecycle.Stop))
            foreach (var e in _errorsToWrite) WriteError(e);
                foreach (var job in _allJobsToStop) WriteObject(job);
            if (eventArgs.Error != null)
                _errorsToWrite.Add(new ErrorRecord(eventArgs.Error, "StopJobError", ErrorCategory.ReadError, job));
                        parentJob.ExecutionError.Where(
                            e => e.FullyQualifiedErrorId == "ContainerParentJobStopAsyncError"))
        private readonly List<Job> _allJobsToStop = new List<Job>();

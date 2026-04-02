    /// Cmdlet to get available list of results.
    [Cmdlet(VerbsCommon.Get, "Job", DefaultParameterSetName = JobCmdletBase.SessionIdParameterSet, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096582")]
    [OutputType(typeof(Job))]
    public class GetJobCommand : JobCmdletBase
        /// IncludeChildJob parameter.
        [Parameter(ParameterSetName = JobCmdletBase.SessionIdParameterSet)]
        [Parameter(ParameterSetName = JobCmdletBase.InstanceIdParameterSet)]
        [Parameter(ParameterSetName = JobCmdletBase.NameParameterSet)]
        [Parameter(ParameterSetName = JobCmdletBase.StateParameterSet)]
        [Parameter(ParameterSetName = JobCmdletBase.CommandParameterSet)]
        public SwitchParameter IncludeChildJob { get; set; }
        /// ChildJobState parameter.
        public JobState ChildJobState { get; set; }
        /// HasMoreData parameter.
        public bool HasMoreData { get; set; }
        /// Before time filter.
        public DateTime Before { get; set; }
        /// After time filter.
        public DateTime After { get; set; }
        /// Newest returned count.
        public int Newest { get; set; }
        /// SessionId for which job
        /// need to be obtained.
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0,
                  ParameterSetName = JobCmdletBase.SessionIdParameterSet)]
        public override int[] Id
                return base.Id;
                base.Id = value;
        /// Extract result objects corresponding to the specified
        /// names or expressions.
            List<Job> jobList = FindJobs();
            jobList.Sort(static (x, y) => x != null ? x.Id.CompareTo(y != null ? y.Id : 1) : -1);
            WriteObject(jobList, true);
        #region Protected Members
        /// Helper method to find jobs based on parameter set.
        /// <returns>Matching jobs.</returns>
        protected List<Job> FindJobs()
            List<Job> jobList = new List<Job>();
                        jobList.AddRange(FindJobsMatchingByName(true, false, true, false));
                case InstanceIdParameterSet:
                        jobList.AddRange(FindJobsMatchingByInstanceId(true, false, true, false));
                case SessionIdParameterSet:
                            jobList.AddRange(FindJobsMatchingBySessionId(true, false, true, false));
                            // Get-Job with no filter.
                            jobList.AddRange(JobRepository.Jobs);
                            jobList.AddRange(JobManager.GetJobs(this, true, false, null));
                case CommandParameterSet:
                        jobList.AddRange(FindJobsMatchingByCommand(false));
                case StateParameterSet:
                        jobList.AddRange(FindJobsMatchingByState(false));
                case FilterParameterSet:
                        jobList.AddRange(FindJobsMatchingByFilter(false));
            jobList.AddRange(FindChildJobs(jobList));
            jobList = ApplyHasMoreDataFiltering(jobList);
            return ApplyTimeFiltering(jobList);
        /// Filter jobs based on HasMoreData.
        /// <param name="jobList"></param>
        /// <returns>Return the list of jobs after applying HasMoreData filter.</returns>
        private List<Job> ApplyHasMoreDataFiltering(List<Job> jobList)
            bool hasMoreDataParameter = MyInvocation.BoundParameters.ContainsKey(nameof(HasMoreData));
            if (!hasMoreDataParameter)
                return jobList;
            List<Job> matches = new List<Job>();
            foreach (Job job in jobList)
                if (job.HasMoreData == HasMoreData)
                    matches.Add(job);
            return matches;
        /// Find the all child jobs with specified ChildJobState in the job list.
        /// <returns>Returns job list including all child jobs with ChildJobState or all if IncludeChildJob is specified.</returns>
        private List<Job> FindChildJobs(List<Job> jobList)
            bool childJobStateParameter = MyInvocation.BoundParameters.ContainsKey(nameof(ChildJobState));
            bool includeChildJobParameter = MyInvocation.BoundParameters.ContainsKey(nameof(IncludeChildJob));
            if (!childJobStateParameter && !includeChildJobParameter)
            // add all child jobs if ChildJobState is not specified
            if (!childJobStateParameter && includeChildJobParameter)
                    if (job.ChildJobs != null && job.ChildJobs.Count > 0)
                        matches.AddRange(job.ChildJobs);
                        if (childJob.JobStateInfo.State != ChildJobState)
                        matches.Add(childJob);
        /// Applys the appropriate time filter to each job in the job list.
        /// Only Job2 type jobs can be time filtered so older Job types are skipped.
        private List<Job> ApplyTimeFiltering(List<Job> jobList)
            bool beforeParameter = MyInvocation.BoundParameters.ContainsKey(nameof(Before));
            bool afterParameter = MyInvocation.BoundParameters.ContainsKey(nameof(After));
            bool newestParameter = MyInvocation.BoundParameters.ContainsKey(nameof(Newest));
            if (!beforeParameter && !afterParameter && !newestParameter)
            // Apply filtering.
            List<Job> filteredJobs;
            if (beforeParameter || afterParameter)
                filteredJobs = new List<Job>();
                    if (job.PSEndTime == DateTime.MinValue)
                        // Skip invalid dates.
                    if (beforeParameter && afterParameter)
                        if (job.PSEndTime < Before &&
                            job.PSEndTime > After)
                            filteredJobs.Add(job);
                    else if ((beforeParameter &&
                              job.PSEndTime < Before) ||
                             (afterParameter &&
                              job.PSEndTime > After))
                filteredJobs = jobList;
            if (!newestParameter ||
                filteredJobs.Count == 0)
                return filteredJobs;
            // Apply Newest count.
            // Sort filtered jobs
            filteredJobs.Sort((firstJob, secondJob) =>
                    if (firstJob.PSEndTime > secondJob.PSEndTime)
                    else if (firstJob.PSEndTime < secondJob.PSEndTime)
            List<Job> newestJobs = new List<Job>();
            foreach (Job job in filteredJobs)
                if (++count > Newest)
                if (!newestJobs.Contains(job))
                    newestJobs.Add(job);
            return newestJobs;

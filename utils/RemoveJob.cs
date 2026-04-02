    /// This is the base class for job cmdlet and contains some helper functions.
    public class JobCmdletBase : PSRemotingCmdlet
        // Parametersets used by job cmdlets
        internal const string JobParameterSet = "JobParameterSet";
        internal const string InstanceIdParameterSet = "InstanceIdParameterSet";
        internal const string SessionIdParameterSet = "SessionIdParameterSet";
        internal const string NameParameterSet = "NameParameterSet";
        internal const string StateParameterSet = "StateParameterSet";
        internal const string CommandParameterSet = "CommandParameterSet";
        internal const string FilterParameterSet = "FilterParameterSet";
        // common parameter names
        internal const string JobParameter = "Job";
        internal const string InstanceIdParameter = "InstanceId";
        internal const string SessionIdParameter = "SessionId";
        internal const string NameParameter = "Name";
        internal const string StateParameter = "State";
        internal const string CommandParameter = "Command";
        internal const string FilterParameter = "Filter";
        #region Job Matches
        /// Find the jobs in repository which match matching the specified names.
        /// <param name="writeobject">if true, method writes the object instead of returning it
        /// in list (an empty list is returned).</param>
        /// <param name="writeErrorOnNoMatch">Write error if no match is found.</param>
        /// <param name="checkIfJobCanBeRemoved">Check if this job can be removed.</param>
        /// <param name="recurse">Recurse and check in child jobs.</param>
        /// <returns>List of matching jobs.</returns>
        internal List<Job> FindJobsMatchingByName(
            bool checkIfJobCanBeRemoved)
                // search all jobs in repository.
                jobFound = FindJobsMatchingByNameHelper(matches, JobRepository.Jobs, name,
                                    duplicateDetector, recurse, writeobject, checkIfJobCanBeRemoved);
                // search all jobs in JobManager
                List<Job2> jobs2 = JobManager.GetJobsByName(name, this, false, writeobject, recurse, null);
                bool job2Found = (jobs2 != null) && (jobs2.Count > 0);
                if (job2Found)
                    foreach (Job2 job2 in jobs2)
                        if (CheckIfJob2CanBeRemoved(checkIfJobCanBeRemoved, NameParameter, job2,
                            RemotingErrorIdStrings.JobWithSpecifiedNameNotCompleted, job2.Id, job2.Name))
                            matches.Add(job2);
                jobFound = jobFound || job2Found;
                // if a match is not found, write an error)
                if (jobFound || !writeErrorOnNoMatch || WildcardPattern.ContainsWildcardCharacters(name))
                Exception ex = PSTraceSource.NewArgumentException(NameParameter, RemotingErrorIdStrings.JobWithSpecifiedNameNotFound, name);
                WriteError(new ErrorRecord(ex, "JobWithSpecifiedNameNotFound", ErrorCategory.ObjectNotFound, name));
        private bool CheckIfJob2CanBeRemoved(bool checkForRemove, string parameterName, Job2 job2, string resourceString, params object[] args)
            if (checkForRemove)
                if (job2.IsFinishedState(job2.JobStateInfo.State))
                string message = PSRemotingErrorInvariants.FormatResourceString(resourceString, args);
                Exception ex = new ArgumentException(message, parameterName);
                WriteError(new ErrorRecord(ex, "JobObjectNotFinishedCannotBeRemoved", ErrorCategory.InvalidOperation, job2));
        private bool FindJobsMatchingByNameHelper(List<Job> matches, IList<Job> jobsToSearch, string name,
                        Hashtable duplicateDetector, bool recurse, bool writeobject, bool checkIfJobCanBeRemoved)
            Dbg.Assert(!string.IsNullOrEmpty(name), "Caller should ensure that name is not null or empty");
            foreach (Job job in jobsToSearch)
                // check if this job has already been searched
                if (duplicateDetector.ContainsKey(job.Id))
                // check if the job is available in any of the
                // top level jobs
                // if (string.Equals(job.Name, name, StringComparison.OrdinalIgnoreCase))
                    if (!checkIfJobCanBeRemoved || CheckJobCanBeRemoved(job, NameParameter, RemotingErrorIdStrings.JobWithSpecifiedNameNotCompleted, job.Id, job.Name))
                // check if the job is available in any of the childjobs
                if (job.ChildJobs != null && job.ChildJobs.Count > 0 && recurse)
                    bool jobFoundinChildJobs = FindJobsMatchingByNameHelper(matches, job.ChildJobs, name,
                    if (jobFoundinChildJobs)
        /// Find the jobs in repository which match the specified instanceid.
        /// <param name="recurse">Look in all child jobs.</param>
        internal List<Job> FindJobsMatchingByInstanceId(bool recurse, bool writeobject, bool writeErrorOnNoMatch, bool checkIfJobCanBeRemoved)
            if (_instanceIds == null)
            foreach (Guid id in _instanceIds)
                // search all jobs in Job repository
                bool jobFound = FindJobsMatchingByInstanceIdHelper(matches, JobRepository.Jobs, id,
                // TODO: optimize this to not search JobManager since matching by InstanceId is unique
                Job2 job2 = JobManager.GetJobByInstanceId(id, this, false, writeobject, recurse);
                bool job2Found = job2 != null;
                    if (CheckIfJob2CanBeRemoved(checkIfJobCanBeRemoved, InstanceIdParameter, job2,
                        RemotingErrorIdStrings.JobWithSpecifiedInstanceIdNotCompleted, job2.Id, job2.InstanceId))
                if (jobFound || !writeErrorOnNoMatch)
                Exception ex = PSTraceSource.NewArgumentException(InstanceIdParameter,
                                                                  RemotingErrorIdStrings.JobWithSpecifiedInstanceIdNotFound,
                                                                  id);
                WriteError(new ErrorRecord(ex, "JobWithSpecifiedInstanceIdNotFound", ErrorCategory.ObjectNotFound, id));
        private bool FindJobsMatchingByInstanceIdHelper(List<Job> matches, IList<Job> jobsToSearch, Guid instanceId,
            // Most likely users will ask for top level jobs.
            // So in order to be more efficient, first look
            // into the top level jobs and only if a match is
            // not found in the top level jobs, recurse. This
            // will ensure that we get a pretty quick hit when
            // the job tree is more than 2 levels deep
            // check if job is found in top level item
                    if (!checkIfJobCanBeRemoved || CheckJobCanBeRemoved(job, InstanceIdParameter, RemotingErrorIdStrings.JobWithSpecifiedInstanceIdNotCompleted, job.Id, job.InstanceId))
                        // instance id is unique, so once a match is found
                        // you can break
            // check if a match is found in the child jobs
            if (!jobFound && recurse)
                        jobFound = FindJobsMatchingByInstanceIdHelper(matches, job.ChildJobs, instanceId,
                        if (jobFound)
        /// Find the jobs in repository which match the specified session ids.
        /// <param name="recurse">Look in child jobs as well.</param>
        internal List<Job> FindJobsMatchingBySessionId(bool recurse, bool writeobject, bool writeErrorOnNoMatch, bool checkIfJobCanBeRemoved)
            if (_sessionIds == null)
            foreach (int id in _sessionIds)
                // check jobs in job repository
                bool jobFound = FindJobsMatchingBySessionIdHelper(matches, JobRepository.Jobs, id,
                // check jobs in job manager
                Job2 job2 = JobManager.GetJobById(id, this, false, writeobject, recurse);
                    if (CheckIfJob2CanBeRemoved(checkIfJobCanBeRemoved, SessionIdParameter, job2,
                        RemotingErrorIdStrings.JobWithSpecifiedSessionIdNotCompleted, job2.Id))
                Exception ex = PSTraceSource.NewArgumentException(SessionIdParameter, RemotingErrorIdStrings.JobWithSpecifiedSessionIdNotFound, id);
                WriteError(new ErrorRecord(ex, "JobWithSpecifiedSessionNotFound", ErrorCategory.ObjectNotFound, id));
        private bool FindJobsMatchingBySessionIdHelper(List<Job> matches, IList<Job> jobsToSearch, int sessionId,
            // check if there is a match in the top level jobs
                if (job.Id == sessionId)
                    if (!checkIfJobCanBeRemoved || CheckJobCanBeRemoved(job, SessionIdParameter, RemotingErrorIdStrings.JobWithSpecifiedSessionIdNotCompleted, job.Id))
                        // session id will be unique for every session, so
                        // can break after the first match
            // check if there is a match found in the child jobs
                        jobFound = FindJobsMatchingBySessionIdHelper(matches, job.ChildJobs, sessionId,
        /// Find the jobs in repository which match the specified command.
        internal List<Job> FindJobsMatchingByCommand(
            bool writeobject)
            List<Job> jobs = new List<Job>();
            jobs.AddRange(JobRepository.Jobs);
            foreach (string command in _commands)
                List<Job2> jobs2 = JobManager.GetJobsByCommand(command, this, false, false, false, null);
                if (jobs2 != null)
                        jobs.Add(job2);
                    WildcardPattern commandPattern = WildcardPattern.Get(command, WildcardOptions.IgnoreCase);
                    string jobCommand = job.Command.Trim();
                    // Win8: 469830
                    // Win7 code does not have commandPattern.IsMatch. We added wildcard support for Command parameterset
                    // in Win8 which breaks scenarios where the actual command has wildcards.)
                    if (jobCommand.Equals(command.Trim(), StringComparison.OrdinalIgnoreCase) || commandPattern.IsMatch(jobCommand))
        /// Find the jobs in repository which match the specified state.
        internal List<Job> FindJobsMatchingByState(
            List<Job2> jobs2 = JobManager.GetJobsByState(_jobstate, this, false, false, false, null);
                if (job.JobStateInfo.State != _jobstate)
        /// Find the jobs which match the specified filter.
        /// <param name="writeobject"></param>
        internal List<Job> FindJobsMatchingByFilter(bool writeobject)
            // add Jobs from JobRepository -- only job property based filters are supported.
            FindJobsMatchingByFilterHelper(jobs, JobRepository.Jobs);
            var filterDictionary = new Dictionary<string, object>();
            foreach (string item in _filter.Keys)
                filterDictionary.Add(item, _filter[item]);
            List<Job2> jobs2 = JobManager.GetJobsByFilter(filterDictionary, this, false, false, true);
        /// Used to find the v2 jobs that match a given filter.
        /// <param name="matches"></param>
        /// <param name="jobsToSearch"></param>
        private static bool FindJobsMatchingByFilterHelper(List<Job> matches, List<Job> jobsToSearch)
            // check that filter only has job properties
            // if so, filter on one at a time using helpers.
        /// Copies the jobs to list.
        /// <param name="checkIfJobCanBeRemoved">If true, only jobs which can be removed will be checked.</param>
        internal List<Job> CopyJobsToList(Job[] jobs, bool writeobject, bool checkIfJobCanBeRemoved)
                if (!checkIfJobCanBeRemoved || CheckJobCanBeRemoved(job, "Job", RemotingErrorIdStrings.JobWithSpecifiedSessionIdNotCompleted, job.Id))
        /// Checks that this job object can be removed. If not, writes an error record.
        /// <param name="job">Job object to be removed.</param>
        /// <param name="parameterName">Name of the parameter which is associated with this job object.
        /// <param name="resourceString">Resource String in case of error.</param>
        /// <param name="list">Parameters for resource message.</param>
        /// <returns>True if object should be removed, else false.</returns>
        private bool CheckJobCanBeRemoved(Job job, string parameterName, string resourceString, params object[] list)
            if (job.IsFinishedState(job.JobStateInfo.State))
            string message = PSRemotingErrorInvariants.FormatResourceString(resourceString, list);
            WriteError(new ErrorRecord(ex, "JobObjectNotFinishedCannotBeRemoved", ErrorCategory.InvalidOperation, job));
        #endregion JobMatches
        /// Name of the jobs to retrieve.
                  ParameterSetName = JobCmdletBase.NameParameterSet)]
        /// InstanceIds for which job
                   ParameterSetName = JobCmdletBase.InstanceIdParameterSet)]
                return _instanceIds;
        private Guid[] _instanceIds;
        public virtual int[] Id
                return _sessionIds;
                _sessionIds = value;
        private int[] _sessionIds;
        /// All the job objects having this state.
                   Position = 0, ValueFromPipelineByPropertyName = true,
            ParameterSetName = RemoveJobCommand.StateParameterSet)]
        public virtual JobState State
                return _jobstate;
                _jobstate = value;
        private JobState _jobstate;
        /// All the job objects having this command.
            ParameterSetName = RemoveJobCommand.CommandParameterSet)]
        public virtual string[] Command
                _commands = value;
        private string[] _commands;
        /// All the job objects matching the values in filter.
            ParameterSetName = RemoveJobCommand.FilterParameterSet)]
        public virtual Hashtable Filter
            get { return _filter; }
            set { _filter = value; }
        private Hashtable _filter;
        /// All remoting cmdlets other than Start-PSJob should
        /// continue to work even if PowerShell remoting is not
        /// enabled. This is because jobs are based out of APIs
        /// and there can be other job implementations like
        /// eventing or WMI which are not based on PowerShell
            CommandDiscovery.AutoloadModulesWithJobSourceAdapters(this.Context, this.CommandOrigin);
            // intentionally left blank to avoid
            // check being performed in base.BeginProcessing()
    /// This cmdlet removes the Job object from the runspace
    /// wide Job repository.
    /// Once the Job object is removed, it will not be available
    /// through get-psjob command.
    [Cmdlet(VerbsCommon.Remove, "Job", SupportsShouldProcess = true, DefaultParameterSetName = JobCmdletBase.SessionIdParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096868")]
    [OutputType(typeof(Job), ParameterSetName = new string[] { JobCmdletBase.JobParameterSet })]
    public class RemoveJobCommand : JobCmdletBase, IDisposable
        /// Specifies the Jobs objects which need to be
                   ParameterSetName = RemoveJobCommand.JobParameterSet)]
        /// If state of the job is running or notstarted, this will forcefully stop it.
        [Parameter(ParameterSetName = RemoveJobCommand.InstanceIdParameterSet)]
        [Parameter(ParameterSetName = RemoveJobCommand.JobParameterSet)]
        [Parameter(ParameterSetName = RemoveJobCommand.NameParameterSet)]
        [Parameter(ParameterSetName = RemoveJobCommand.SessionIdParameterSet)]
        [Parameter(ParameterSetName = RemoveJobCommand.FilterParameterSet)]
        [Alias("F")]
        /// Gets the job object as per the parameter and removes it.
            List<Job> listOfJobsToRemove = null;
                        listOfJobsToRemove = FindJobsMatchingByName(false, false, true, !_force);
                        listOfJobsToRemove = FindJobsMatchingByInstanceId(true, false, true, !_force);
                        listOfJobsToRemove = FindJobsMatchingBySessionId(true, false, true, !_force);
                        listOfJobsToRemove = FindJobsMatchingByCommand(false);
                        listOfJobsToRemove = FindJobsMatchingByState(false);
                        listOfJobsToRemove = FindJobsMatchingByFilter(false);
                        listOfJobsToRemove = CopyJobsToList(_jobs, false, !_force);
            // Now actually remove the jobs
            foreach (Job job in listOfJobsToRemove)
                string message = GetMessage(RemotingErrorIdStrings.StopPSJobWhatIfTarget, job.Command, job.Id);
                if (!ShouldProcess(message, VerbsCommon.Remove))
                if (!job.IsFinishedState(job.JobStateInfo.State))
                    // if it is a Job2, then async is supported
                    // stop the job asynchronously
                        _cleanUpActions.Add(job2, HandleStopJobCompleted);
                        job2.StopJobCompleted += HandleStopJobCompleted;
                            if (!job2.IsFinishedState(job2.JobStateInfo.State) &&
                                !_pendingJobs.Contains(job2.InstanceId))
                                _pendingJobs.Add(job2.InstanceId);
                        job2.StopJobAsync();
                        job.StopJob();
                        RemoveJobAndDispose(job, false);
                    RemoveJobAndDispose(job, job2 != null);
        /// Wait for all the stop jobs to be completed.
            bool haveToWait = false;
                _needToCheckForWaitingJobs = true;
                if (_pendingJobs.Count > 0)
                    haveToWait = true;
            if (haveToWait)
                _waitForJobs.WaitOne();
        /// Release waiting for jobs.
            _waitForJobs.Set();
        private void RemoveJobAndDispose(Job job, bool jobIsJob2)
                bool job2TypeFound = false;
                if (jobIsJob2)
                    job2TypeFound = JobManager.RemoveJob(job as Job2, this, true, false);
                if (!job2TypeFound)
                                        RemotingErrorIdStrings.CannotRemoveJob);
                ArgumentException ex2 = new ArgumentException(message, ex);
                WriteError(new ErrorRecord(ex2, "CannotRemoveJob", ErrorCategory.InvalidOperation, job));
        private void HandleStopJobCompleted(object sender, AsyncCompletedEventArgs eventArgs)
            RemoveJobAndDispose(job, true);
            bool releaseWait = false;
                if (_pendingJobs.Contains(job.InstanceId))
                    _pendingJobs.Remove(job.InstanceId);
                if (_needToCheckForWaitingJobs && _pendingJobs.Count == 0)
                    releaseWait = true;
            // end processing has been called
            // set waithandle if this is the last one
            if (releaseWait)
        private readonly HashSet<Guid> _pendingJobs = new HashSet<Guid>();
        private readonly ManualResetEvent _waitForJobs = new ManualResetEvent(false);
        private readonly Dictionary<Job2, EventHandler<AsyncCompletedEventArgs>> _cleanUpActions =
            new Dictionary<Job2, EventHandler<AsyncCompletedEventArgs>>();
        private bool _needToCheckForWaitingJobs;
            foreach (var pair in _cleanUpActions)
                pair.Key.StopJobCompleted -= pair.Value;
            _waitForJobs.Dispose();

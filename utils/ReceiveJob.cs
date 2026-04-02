    /// Cmdlet used for receiving results from job object.
    /// This cmdlet is intended to have a slightly different behavior
    /// in the following two cases:
    ///          1. The job object to receive results from is a PSRemotingJob
    ///               In this case, the cmdlet can use two additional
    ///               parameters to filter results - ComputerName and Runspace
    ///               The parameters help filter out results for a specified
    ///               computer or runspace from the job object
    ///               $job = Start-PSJob -Command 'get-process' -ComputerName server1, server2
    ///               Receive-PSJob -Job $job -ComputerName server1
    ///               $job = Start-PSJob -Command 'get-process' -Session $r1, $r2
    ///               Receive-PSJob -Job $job -Session $r1
    ///         2. The job object to receive results is a PSJob (or derivative
    ///            other than PSRemotingJob)
    ///              In this case, the user cannot will use the location parameter
    ///              to do any filtering and will not have ComputerName and Runspace
    ///              parameters
    ///              $job = Get-WMIObject '....' -AsJob
    ///              Receive-PSJob -Job $job -Location "Server2"
    ///              The following will result in an error:
    ///              Receive-PSJob -Job $job -ComputerName "Server2"
    ///              The parameter ComputerName cannot be used with jobs which are
    ///              not PSRemotingJob.
    [Cmdlet(VerbsCommunications.Receive, "Job", DefaultParameterSetName = ReceiveJobCommand.LocationParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096965", RemotingCapability = RemotingCapability.SupportedByCommand)]
    public class ReceiveJobCommand : JobCmdletBase, IDisposable
        /// Job object from which specific results need to
        /// be extracted.
                   ParameterSetName = ReceiveJobCommand.ComputerNameParameterSet)]
                   ParameterSetName = ReceiveJobCommand.SessionParameterSet)]
                   ParameterSetName = ReceiveJobCommand.LocationParameterSet)]
        public Job[] Job
                return _jobs;
                _jobs = value;
        private Job[] _jobs;
        /// Name of the computer for which the results needs to be
                   ParameterSetName = ReceiveJobCommand.ComputerNameParameterSet,
        /// Locations for which the results needs to be returned.
        /// This will cater to all kinds of jobs and not only
        /// remoting jobs.
        [Parameter(ParameterSetName = ReceiveJobCommand.LocationParameterSet,
        public string[] Location
                return _locations;
                _locations = value;
        private string[] _locations;
        /// Runspaces for which the results needs to be
                   ParameterSetName = ReceiveJobCommand.SessionParameterSet,
        public PSSession[] Session
                return _remoteRunspaceInfos;
                _remoteRunspaceInfos = value;
        private PSSession[] _remoteRunspaceInfos;
        /// If the results need to be not removed from the store
        /// after being written. Default is results are removed.
        public SwitchParameter Keep
                return !_flush;
                _flush = !value;
                ValidateWait();
        private bool _flush = true;
        public SwitchParameter NoRecurse
                return !_recurse;
                _recurse = !value;
        private bool _recurse = true;
        { get; set; }
        public override JobState State
                return JobState.NotStarted;
        public override Hashtable Filter
        public override string[] Command
        protected const string LocationParameterSet = "Location";
        public SwitchParameter Wait
                return _wait;
                _wait = value;
        public SwitchParameter AutoRemoveJob
                return _autoRemoveJob;
                _autoRemoveJob = value;
        public SwitchParameter WriteEvents
                return _writeStateChangedEvents;
                _writeStateChangedEvents = value;
        public SwitchParameter WriteJobInResults
                return _outputJobFirst;
                _outputJobFirst = value;
        private bool _autoRemoveJob;
        private bool _writeStateChangedEvents;
        private bool _wait;
        private readonly ReaderWriterLockSlim _resultsReaderWriterLock = new ReaderWriterLockSlim();
        private readonly ManualResetEvent _writeExistingData = new ManualResetEvent(true);
        private readonly PSDataCollection<PSStreamObject> _results = new PSDataCollection<PSStreamObject>();
        private bool _holdingResultsRef;
        private readonly List<Job> _jobsBeingAggregated = new List<Job>();
        private readonly List<Guid> _jobsSpecifiedInParameters = new List<Guid>();
        private bool _outputJobFirst;
        private OutputProcessingState _outputProcessingNotification;
        private const string ClassNameTrace = "ReceiveJobCommand";
            ValidateAutoRemove();
            ValidateWriteJobInResults();
            ValidateWriteEvents();
            ValidateForce();
        /// Retrieve the results for the specified computers or
        /// runspaces.
            bool checkForRecurse = false;
            List<Job> jobsToWrite = new List<Job>();
                        foreach (Job job in _jobs)
                            PSRemotingJob remoteJob =
                                        job as PSRemotingJob;
                            if (remoteJob == null)
                                string message = GetMessage(RemotingErrorIdStrings.RunspaceParamNotSupported);
                                WriteError(new ErrorRecord(new ArgumentException(message),
                                    "RunspaceParameterNotSupported", ErrorCategory.InvalidArgument,
                                        job));
                            // Runspace parameter is supported only on PSRemotingJob objects
                            foreach (PSSession remoteRunspaceInfo in _remoteRunspaceInfos)
                                // get the required child jobs
                                List<Job> childJobs = remoteJob.GetJobsForRunspace(remoteRunspaceInfo);
                                jobsToWrite.AddRange(childJobs);
                                // WriteResultsForJobsInCollection(childJobs, false);
                            // the job can either be a remoting job or another one
                            // ComputerName parameter can only be used with remoting jobs
                                string message = GetMessage(RemotingErrorIdStrings.ComputerNameParamNotSupported);
                                    "ComputerNameParameterNotSupported", ErrorCategory.InvalidArgument,
                            string[] resolvedComputernames = null;
                            ResolveComputerNames(_computerNames, out resolvedComputernames);
                            foreach (string resolvedComputerName in resolvedComputernames)
                                // get the required child Job objects
                                List<Job> childJobs = remoteJob.GetJobsForComputer(resolvedComputerName);
                case "Location":
                        if (_locations == null)
                            // WriteAll();
                            jobsToWrite.AddRange(_jobs);
                            checkForRecurse = true;
                                foreach (string location in _locations)
                                    List<Job> childJobs = job.GetJobsForLocation(location);
                case ReceiveJobCommand.InstanceIdParameterSet:
                        List<Job> jobs = FindJobsMatchingByInstanceId(true, false, true, false);
                        jobsToWrite.AddRange(jobs);
                        // WriteResultsForJobsInCollection(jobs, true);
                case ReceiveJobCommand.SessionIdParameterSet:
                        List<Job> jobs = FindJobsMatchingBySessionId(true, false, true, false);
                case ReceiveJobCommand.NameParameterSet:
                        List<Job> jobs = FindJobsMatchingByName(true, false, true, false);
            // if block has been specified and the cmdlet has not been
            // stopped, we continue to write recursively, until there
            // is no more data to write
            if (_wait)
                _writeExistingData.Reset();
                // if writejobresults is specified we will write only the top level jobs
                // this is because that is what the proxy requires. Anything else being
                // written is useless and will only add weight to the serialization
                WriteJobsIfRequired(jobsToWrite);
                // Make a note of the jobs specified by the user (does not include child jobs)
                // for the purpose of removal. Only the parent jobs should have remove called.
                foreach (var job in jobsToWrite)
                    _jobsSpecifiedInParameters.Add(job.InstanceId);
                    if (_isDisposed || _isStopping) return;
                    // Check to see that we only AddRef once. ProcessRecord is called
                    // once per job on the pipeline.
                    if (!_holdingResultsRef)
                        _tracer.WriteMessage(ClassNameTrace, "ProcessRecord", Guid.Empty, (Job)null, "Adding Ref to results collection",
                        _results.AddRef();
                        _holdingResultsRef = true;
                _tracer.WriteMessage(ClassNameTrace, "ProcessRecord", Guid.Empty, (Job)null, "BEGIN Register for jobs");
                WriteResultsForJobsInCollection(jobsToWrite, checkForRecurse, true);
                _tracer.WriteMessage(ClassNameTrace, "ProcessRecord", Guid.Empty, (Job)null, "END Register for jobs");
                    if (_jobsBeingAggregated.Count == 0 && _holdingResultsRef)
                        _tracer.WriteMessage(ClassNameTrace, "ProcessRecord", Guid.Empty, (Job)null,
                                             "Removing Ref to results collection", null);
                        _results.DecrementRef();
                        _holdingResultsRef = false;
                _tracer.WriteMessage(ClassNameTrace, "ProcessRecord", Guid.Empty, (Job)null, "BEGIN Write existing job data");
                WriteResultsForJobsInCollection(jobsToWrite, checkForRecurse, false);
                _tracer.WriteMessage(ClassNameTrace, "ProcessRecord", Guid.Empty, (Job)null, "END Write existing job data");
                _writeExistingData.Set();
        /// StopProcessing - when the command is stopped,
        /// unregister all the event handlers from the jobs
        /// and decrement reference for results.
            _tracer.WriteMessage(ClassNameTrace, "StopProcessing", Guid.Empty, (Job)null, "Entered Stop Processing",
            Job[] aggregatedJobs = new Job[_jobsBeingAggregated.Count];
            for (int i = 0; i < _jobsBeingAggregated.Count; i++)
                aggregatedJobs[i] = _jobsBeingAggregated[i];
            foreach (Job job in aggregatedJobs)
                StopAggregateResultsFromJob(job);
            _resultsReaderWriterLock.EnterWriteLock();
                _results.Complete();
                SetOutputProcessingState(false);
                _resultsReaderWriterLock.ExitWriteLock();
            _tracer.WriteMessage(ClassNameTrace, "StopProcessing", Guid.Empty, (Job)null, "Exiting Stop Processing",
        /// If we are not stopping, continue writing output
        /// as and when they are available.
                    int totalCount = 0;
                    foreach (PSStreamObject result in _results)
                        if (_isStopping) break;
                        SetOutputProcessingState(true);
                        result.WriteStreamObject(this, true, true);
                        if (++totalCount == _results.Count)
                    _eventArgsWritten.Clear();
                        result.WriteStreamObject(this, false, true);
                if (_isDisposed) return;
                if (_jobsBeingAggregated != null)
                    foreach (var job in _jobsBeingAggregated)
                        if (job.MonitorOutputProcessing)
                            job.RemoveMonitorOutputProcessing(_outputProcessingNotification);
                        if (job.UsesResultsCollection)
                            job.Results.DataAdded -= ResultsAdded;
                            job.Output.DataAdded -= Output_DataAdded;
                            job.Error.DataAdded -= Error_DataAdded;
                            job.Progress.DataAdded -= Progress_DataAdded;
                            job.Verbose.DataAdded -= Verbose_DataAdded;
                            job.Warning.DataAdded -= Warning_DataAdded;
                            job.Debug.DataAdded -= Debug_DataAdded;
                            job.Information.DataAdded -= Information_DataAdded;
                _resultsReaderWriterLock.Dispose();
                _results.Clear();
                _results.Dispose();
                _writeExistingData.Dispose();
        private static void DoUnblockJob(Job job)
            // we should not do anything for a parent job
            // the assumption is parent job states are
            // computed and so unblocking the child state
            // should be able to handle this
            if (job.ChildJobs.Count != 0) return;
            // we have a better way of handling blocked state logic
            // for remoting jobs, so use that if job is a remoting
            // job
            PSRemotingChildJob remotingChildJob = job as PSRemotingChildJob;
            if (remotingChildJob != null)
                remotingChildJob.UnblockJob();
                // for all other job types, simply set the job state
                // to running, the handling of the parent jobs state
                // should be taken care of by the job implementation
                job.SetJobState(JobState.Running, null);
        /// Write the results from this Job object. This does not write from the
        /// child jobs of this job object.
        /// <param name="job">Job object from which to write the results from
        private void WriteJobResults(Job job)
            if (job == null) return;
            // Q: Why do we need to unblock the job, before getting
            // the results
            // A: The job can get into a terminal state and we do
            // not want to set it to running at that point. Also, if
            // we do not explicitly signal that the job is unblocked
            // then the parent job cannot be unblocked. This is because
            // the parent job does not maintain a list of jobs which
            // are blocked but just simply a count (to keep things
            // light weight)
            // check if the state of the job is blocked, if so unblock it
            // Skip disconnected jobs that were in Blocked state before
            // the disconnect, since we cannot process host data until the
            // job is re-connected.
            if (job.JobStateInfo.State == JobState.Disconnected)
                if (remotingChildJob != null && remotingChildJob.DisconnectedAndBlocked)
            // TODO: Fix Unblock() handling by Job2
            if (job.JobStateInfo.State == JobState.Blocked)
                DoUnblockJob(job);
            // for the jobs that PowerShell writes, there is a
            // results collection internally used. This collection
            // can be used to write results. For all other jobs
            // results need to be written from the other collections
            // There is a bug in V2 that only remoting jobs work
            // with Receive-Job. This is being fixed
            if (job is not Job2 && job.UsesResultsCollection)
                // extract results and handle them
                Collection<PSStreamObject> results = ReadAll<PSStreamObject>(job.Results);
                    foreach (var psStreamObject in results)
                        psStreamObject.WriteStreamObject(this, job.Results.SourceId);
                        psStreamObject.WriteStreamObject(this);
                Collection<PSObject> output = ReadAll<PSObject>(job.Output);
                foreach (PSObject o in output)
                    if (o == null) continue;
                Collection<ErrorRecord> errorRecords = ReadAll<ErrorRecord>(job.Error);
                foreach (ErrorRecord e in errorRecords)
                    if (e == null) continue;
                    MshCommandRuntime mshCommandRuntime = CommandRuntime as MshCommandRuntime;
                        e.PreserveInvocationInfoOnce = true;
                        mshCommandRuntime.WriteError(e, true);
                Collection<VerboseRecord> verboseRecords = ReadAll(job.Verbose);
                foreach (VerboseRecord v in verboseRecords)
                    if (v == null) continue;
                    mshCommandRuntime?.WriteVerbose(v, true);
                Collection<DebugRecord> debugRecords = ReadAll(job.Debug);
                foreach (DebugRecord d in debugRecords)
                    if (d == null) continue;
                    mshCommandRuntime?.WriteDebug(d, true);
                Collection<WarningRecord> warningRecords = ReadAll(job.Warning);
                foreach (WarningRecord w in warningRecords)
                    if (w == null) continue;
                    mshCommandRuntime?.WriteWarning(w, true);
                Collection<ProgressRecord> progressRecords = ReadAll(job.Progress);
                foreach (ProgressRecord p in progressRecords)
                    if (p == null) continue;
                    mshCommandRuntime?.WriteProgress(p, true);
                Collection<InformationRecord> informationRecords = ReadAll(job.Information);
                foreach (InformationRecord p in informationRecords)
                    mshCommandRuntime?.WriteInformation(p, true);
            if (job.JobStateInfo.State != JobState.Failed) return;
            WriteReasonError(job);
        private void WriteReasonError(Job job)
            // Write better error for the remoting case and generic error for the other case
            PSRemotingChildJob child = job as PSRemotingChildJob;
            if (child != null && child.FailureErrorRecord != null)
                _results.Add(new PSStreamObject(PSStreamObjectType.Error, child.FailureErrorRecord, child.InstanceId));
            else if (job.JobStateInfo.Reason != null)
                Exception baseReason = job.JobStateInfo.Reason;
                Exception resultReason = baseReason;
                // If it was generated by a job that gave location information, unpack the
                // base exception.
                JobFailedException exceptionWithLocation = baseReason as JobFailedException;
                if (exceptionWithLocation != null)
                    resultReason = exceptionWithLocation.Reason;
                ErrorRecord errorRecord = new ErrorRecord(resultReason, "JobStateFailed", ErrorCategory.InvalidResult, null);
                // If it was generated by a job that gave location information, set the
                // location information.
                if ((exceptionWithLocation != null) && (exceptionWithLocation.DisplayScriptPosition != null))
                    if (errorRecord.InvocationInfo == null)
                        errorRecord.SetInvocationInfo(new InvocationInfo(null, null));
                    errorRecord.InvocationInfo.DisplayScriptPosition = exceptionWithLocation.DisplayScriptPosition;
                _results.Add(new PSStreamObject(PSStreamObjectType.Error, errorRecord, job.InstanceId));
        /// Returns all the results from supplied PSDataCollection.
        /// <param name="psDataCollection">Data collection to read from.</param>
        /// <returns>Collection with copy of data.</returns>
        private Collection<T> ReadAll<T>(PSDataCollection<T> psDataCollection)
            if (_flush)
                return psDataCollection.ReadAll();
            T[] array = new T[psDataCollection.Count];
            psDataCollection.CopyTo(array, 0);
            Collection<T> collection = new Collection<T>();
            foreach (T t in array)
                collection.Add(t);
        /// Write the results from this Job object. It also writes the
        /// results from its child objects recursively.
        /// <param name="duplicate">Hashtable used for duplicate detection.</param>
        /// <param name="job">Job whose results are written.</param>
        /// <param name="registerInsteadOfWrite"></param>
        private void WriteJobResultsRecursivelyHelper(Hashtable duplicate, Job job, bool registerInsteadOfWrite)
            // Check if this object is already visited. If not, add it to the cache
            if (duplicate.ContainsKey(job))
            duplicate.Add(job, job);
            // Write the results of child jobs
            IList<Job> childJobs = job.ChildJobs;
            foreach (Job childjob in childJobs)
                WriteJobResultsRecursivelyHelper(duplicate, childjob, registerInsteadOfWrite);
            if (registerInsteadOfWrite)
                // at any point there will be only one thread which will have
                // access to an entry corresponding to a job
                // this is because of the way the synchronization happens
                // with the pipeline thread and event handler thread using
                // _writeExistingData
                _eventArgsWritten[job.InstanceId] = false;
                // register the job for future updates
                AggregateResultsFromJob(job);
                // Write the results of this job
                WriteJobResults(job);
                WriteJobStateInformationIfRequired(job);
        /// Writes the job objects if required by the cmdlet.
        /// <param name="jobsToWrite">Collection of jobs to write.</param>
        /// <remarks>this method is intended to be called only from
        /// ProcessRecord. When any changes are made ensure that this
        /// contract is not broken</remarks>
        private void WriteJobsIfRequired(IEnumerable<Job> jobsToWrite)
            if (!_outputJobFirst) return;
                _tracer.WriteMessage("ReceiveJobCommand", "WriteJobsIfRequired", Guid.Empty, job, "Writing job object as output", null);
        /// <remarks>this method should always be called before
        /// writeExistingData is set in ProcessRecord</remarks>
        private void AggregateResultsFromJob(Job job)
            if ((!Force && job.IsPersistentState(job.JobStateInfo.State)) || (Force && job.IsFinishedState(job.JobStateInfo.State))) return;
            job.StateChanged += HandleJobStateChanged;
            // Check after the state changed event has been subscribed to avoid a race
            // with the job state. StopAggregate is called from the state changed handler, so-
            // this could cause the job to never be removed from _jobsBeingAggregated
            // and therefore the _results ref to never be decremented.
            if ((!Force && job.IsPersistentState(job.JobStateInfo.State)) || (Force && job.IsFinishedState(job.JobStateInfo.State)))
            _tracer.WriteMessage(ClassNameTrace, "AggregateResultsFromJob", Guid.Empty, job,
                                 "BEGIN Adding job for aggregation", null);
            // at this point, we can be sure that any job added to this
            // collection will have a state changed event to a finished state.
            _jobsBeingAggregated.Add(job);
            // Tag the output collection so that the instance ID can be added to the output objects when streaming.
                job.Results.SourceId = job.InstanceId;
                job.Results.DataAdded += ResultsAdded;
                job.Output.SourceId = job.InstanceId;
                job.Error.SourceId = job.InstanceId;
                job.Progress.SourceId = job.InstanceId;
                job.Verbose.SourceId = job.InstanceId;
                job.Warning.SourceId = job.InstanceId;
                job.Debug.SourceId = job.InstanceId;
                job.Information.SourceId = job.InstanceId;
                job.Output.DataAdded += Output_DataAdded;
                job.Error.DataAdded += Error_DataAdded;
                job.Progress.DataAdded += Progress_DataAdded;
                job.Verbose.DataAdded += Verbose_DataAdded;
                job.Warning.DataAdded += Warning_DataAdded;
                job.Debug.DataAdded += Debug_DataAdded;
                job.Information.DataAdded += Information_DataAdded;
                if (_outputProcessingNotification == null)
                        _outputProcessingNotification ??= new OutputProcessingState();
                job.SetMonitorOutputProcessing(_outputProcessingNotification);
                                 "END Adding job for aggregation", null);
        private void ResultsAdded(object sender, DataAddedEventArgs e)
            _writeExistingData.WaitOne();
            PSDataCollection<PSStreamObject> results = sender as PSDataCollection<PSStreamObject>;
            Dbg.Assert(results != null, "PSDataCollection is raising an inappropriate event");
            PSStreamObject record = GetData(results, e.Index);
            if (record != null)
                record.Id = results.SourceId;
                _results.Add(record);
            Dbg.Assert(job != null, "Job state info cannot be raised with reference to job");
            // waiting for existing data to be written ensures two things
            // 1. that no aggregation for a job is in progress
            // 2. the state information is written in the correct order
            //    as per the contract
            _tracer.WriteMessage(ClassNameTrace, "HandleJobStateChanged", Guid.Empty, job,
                                 "BEGIN wait for write existing data", null);
            if (e.JobStateInfo.State != JobState.Running)
                                 "END wait for write existing data", null);
                if (!_jobsBeingAggregated.Contains(job))
                                         "Returning because job is not in _jobsBeingAggregated", null);
            // Stop wait if:
            // Force is specified and Job is in a Finished state (Completed, Failed, Stopped)
            // Force is not specified and Job is in a persistent state (Suspended or
            // Disconnected as well as above)
            // (logic inverted for return)
            if (!(!Force && job.IsPersistentState(e.JobStateInfo.State)) && !(Force && job.IsFinishedState(e.JobStateInfo.State)))
                                     "Returning because job state does not meet wait requirements (continue aggregating)");
            // Write an error record with JobStateFailed ID if there is a JobStateInfo.Reason.
            WriteJobStateInformationIfRequired(job, e);
        private void Progress_DataAdded(object sender, DataAddedEventArgs e)
            _resultsReaderWriterLock.EnterReadLock();
                if (!_results.IsOpen) return;
                PSDataCollection<ProgressRecord> progressRecords = sender as PSDataCollection<ProgressRecord>;
                Diagnostics.Assert(progressRecords != null, "PSDataCollection is raising an inappropriate event");
                ProgressRecord record = GetData(progressRecords, e.Index);
                    _results.Add(new PSStreamObject(PSStreamObjectType.Progress, record, progressRecords.SourceId));
                _resultsReaderWriterLock.ExitReadLock();
                if (!_results.IsOpen)
                PSDataCollection<ErrorRecord> errorRecords = sender as PSDataCollection<ErrorRecord>;
                Diagnostics.Assert(errorRecords != null, "PSDataCollection is raising an inappropriate event");
                ErrorRecord errorRecord = GetData(errorRecords, e.Index);
                    // error records are already tagged, skip tagging
                    _results.Add(new PSStreamObject(PSStreamObjectType.Error, errorRecord, Guid.Empty));
        private void Debug_DataAdded(object sender, DataAddedEventArgs e)
                PSDataCollection<DebugRecord> debugRecords = sender as PSDataCollection<DebugRecord>;
                Diagnostics.Assert(debugRecords != null, "PSDataCollection is raising an inappropriate event");
                DebugRecord record = GetData(debugRecords, e.Index);
                    // debug records are already tagged, skip tagging
                    _results.Add(new PSStreamObject(PSStreamObjectType.Debug, record.Message, Guid.Empty));
        private void Warning_DataAdded(object sender, DataAddedEventArgs e)
                PSDataCollection<WarningRecord> warningRecords = sender as PSDataCollection<WarningRecord>;
                Diagnostics.Assert(warningRecords != null, "PSDataCollection is raising an inappropriate event");
                WarningRecord record = GetData(warningRecords, e.Index);
                    // warning records are already tagged, skip tagging
                    _results.Add(new PSStreamObject(PSStreamObjectType.Warning, record.Message, Guid.Empty));
        private void Verbose_DataAdded(object sender, DataAddedEventArgs e)
                PSDataCollection<VerboseRecord> verboseRecords = sender as PSDataCollection<VerboseRecord>;
                Dbg.Assert(verboseRecords != null, "PSDataCollection is raising an inappropriate event");
                VerboseRecord record = GetData(verboseRecords, e.Index);
                    // verbose records are already tagged, skip tagging
                    _results.Add(new PSStreamObject(PSStreamObjectType.Verbose, record.Message, Guid.Empty));
        private void Information_DataAdded(object sender, DataAddedEventArgs e)
                PSDataCollection<InformationRecord> informationRecords = sender as PSDataCollection<InformationRecord>;
                Dbg.Assert(informationRecords != null, "PSDataCollection is raising an inappropriate event");
                InformationRecord record = GetData(informationRecords, e.Index);
                    // information records are already tagged, skip tagging
                    _results.Add(new PSStreamObject(PSStreamObjectType.Information, record, Guid.Empty));
                PSDataCollection<PSObject> output = sender as PSDataCollection<PSObject>;
                Dbg.Assert(output != null, "PSDataCollection is raising an inappropriate event");
                PSObject obj = GetData(output, e.Index);
                    // output objects are already tagged, skip tagging
                    _results.Add(new PSStreamObject(PSStreamObjectType.Output, obj, Guid.Empty));
        private T GetData<T>(PSDataCollection<T> collection, int index)
                Collection<T> data = collection.ReadAndRemove(1);
                if (data.Count > 0)
                    Dbg.Assert(data.Count == 1, "DataAdded should be raised for each object added");
                    return data[0];
                // it is possible that when there was a wait
                // the data got written
            return collection[index];
        private void StopAggregateResultsFromJob(Job job)
                _tracer.WriteMessage(ClassNameTrace, "StopAggregateResultsFromJob", Guid.Empty, job,
                                     "Removing job from aggregation", null);
                _jobsBeingAggregated.Remove(job);
                    _tracer.WriteMessage(ClassNameTrace, "StopAggregateResultsFromJob", Guid.Empty, (Job)null,
        private void AutoRemoveJobIfRequired(Job job)
            if (!_autoRemoveJob) return;
            if (!_jobsSpecifiedInParameters.Contains(job.InstanceId)) return;
            if (!job.IsFinishedState(job.JobStateInfo.State)) return;
            // Only finished jobs that were specified by the user on the cmdline
            // (computed in processRecord) should reach this point.
            if (job.HasMoreData)
                _tracer.WriteMessage(ClassNameTrace, "AutoRemoveJobIfRequired", Guid.Empty, job,
                                     "Job has data and is being removed.");
            Job2 job2 = job as Job2;
                    JobManager.RemoveJob(job2, this, false, true);
                    // JobSourceAdapters are third party code, and could
                    // throw any exception type. Catch generic exception, but transfer the error
                    // so that it's written to the user.
                    AddRemoveErrorToResults(job2, ex);
                    JobRepository.Remove(job);
                    AddRemoveErrorToResults(job, ex);
        private void AddRemoveErrorToResults(Job job, Exception ex)
            var ex2 = new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.CannotRemoveJob), ex);
            var removeError = new ErrorRecord(ex2, "ReceiveJobAutoRemovalError", ErrorCategory.InvalidOperation, job);
            _results.Add(new PSStreamObject(PSStreamObjectType.Error, removeError));
        private void WriteJobResultsRecursively(Job job, bool registerInsteadOfWrite)
            WriteJobResultsRecursivelyHelper(duplicateDetector, job, registerInsteadOfWrite);
            duplicateDetector.Clear();
        /// <param name="jobs"></param>
        /// <param name="checkForRecurse"></param>
        private void WriteResultsForJobsInCollection(List<Job> jobs, bool checkForRecurse, bool registerInsteadOfWrite)
            foreach (Job job in jobs)
                if (checkForRecurse && _recurse)
                    WriteJobResultsRecursively(job, registerInsteadOfWrite);
        private readonly Dictionary<Guid, bool> _eventArgsWritten = new Dictionary<Guid, bool>();
        private void WriteJobStateInformation(Job job, JobStateEventArgs args = null)
            bool eventWritten;
            _eventArgsWritten.TryGetValue(job.InstanceId, out eventWritten);
            if (eventWritten)
                _tracer.WriteMessage(ClassNameTrace, "WriteJobStateInformation", Guid.Empty, job,
                                     "State information already written, skipping another write", null);
            JobStateEventArgs eventArgs = args ?? new JobStateEventArgs(job.JobStateInfo);
            _eventArgsWritten[job.InstanceId] = true;
            _tracer.WriteMessage(ClassNameTrace, "WriteJobStateInformation", Guid.Empty, job, "Writing job state changed event args", null);
            PSObject obj = new PSObject(eventArgs);
            obj.Properties.Add(new PSNoteProperty(RemotingConstants.EventObject, true));
            _results.Add(new PSStreamObject(PSStreamObjectType.Output, obj, job.InstanceId));
        private void WriteJobStateInformationIfRequired(Job job, JobStateEventArgs args = null)
            if (_writeStateChangedEvents && job.IsPersistentState(job.JobStateInfo.State))
                WriteJobStateInformation(job, args);
            AutoRemoveJobIfRequired(job);
        private void ValidateWait()
            if (_wait && !_flush)
                throw PSTraceSource.NewInvalidOperationException(RemotingErrorIdStrings.BlockCannotBeUsedWithKeep);
        private void ValidateWriteEvents()
            if (_writeStateChangedEvents && !_wait)
                throw PSTraceSource.NewInvalidOperationException(RemotingErrorIdStrings.WriteEventsCannotBeUsedWithoutWait);
        private void ValidateAutoRemove()
            if (_autoRemoveJob && !_wait)
                throw PSTraceSource.NewInvalidOperationException(RemotingErrorIdStrings.AutoRemoveCannotBeUsedWithoutWait);
        private void ValidateForce()
            if (Force && !_wait)
                throw PSTraceSource.NewInvalidOperationException(RemotingErrorIdStrings.ForceCannotBeUsedWithoutWait);
        private void ValidateWriteJobInResults()
            if (_outputJobFirst && !_wait)
                throw PSTraceSource.NewInvalidOperationException(RemotingErrorIdStrings.WriteJobInResultsCannotBeUsedWithoutWait);
        #region OutputProcessingState
        private void SetOutputProcessingState(bool processingOutput)
            bool stateChanged;
                stateChanged = (processingOutput != _processingOutput);
                if (stateChanged)
                    _processingOutput = processingOutput;
            if (_outputProcessingNotification != null && stateChanged)
                _outputProcessingNotification.RaiseOutputProcessingStateChangedEvent(processingOutput);
    internal class OutputProcessingState : IOutputProcessingState
        public event EventHandler<OutputProcessingStateEventArgs> OutputProcessingStateChanged;
        internal void RaiseOutputProcessingStateChangedEvent(bool processingOutput)
                OutputProcessingStateChanged.SafeInvoke<OutputProcessingStateEventArgs>(
                    new OutputProcessingStateEventArgs(processingOutput));

    /// Manager for JobSourceAdapters for invocation and management of specific Job types.
    public sealed class JobManager
        /// Collection of registered JobSourceAdapters.
        private readonly Dictionary<string, JobSourceAdapter> _sourceAdapters =
            new Dictionary<string, JobSourceAdapter>();
        /// Collection of job IDs that are valid for reuse.
        private static readonly Dictionary<Guid, KeyValuePair<int, string>> s_jobIdsForReuse = new Dictionary<Guid, KeyValuePair<int, string>>();
        private static readonly object s_syncObject = new object();
        /// Creates a JobManager instance.
        internal JobManager()
        /// Returns true if the type is already registered.
        /// <param name="typeName">Type to check.</param>
        /// <returns>Whether the type is registered already.</returns>
        public bool IsRegistered(string typeName)
                return _sourceAdapters.ContainsKey(typeName);
        /// Adds a new JobSourceAdapter to the JobManager instance.
        /// After addition, creating a NewJob with a JobDefinition
        /// indicating the JobSourceAdapter derivative type will function.
        /// <param name="jobSourceAdapterType">The derivative JobSourceAdapter type to
        /// register.</param>
        /// <exception cref="InvalidOperationException">Throws when there is no public
        /// default constructor on the type.</exception>
        internal void RegisterJobSourceAdapter(Type jobSourceAdapterType)
            Dbg.Assert(typeof(JobSourceAdapter).IsAssignableFrom(jobSourceAdapterType), "BaseType of any type being registered with the JobManager should be JobSourceAdapter.");
            Dbg.Assert(jobSourceAdapterType != typeof(JobSourceAdapter), "JobSourceAdapter abstract type itself should never be registered.");
            Dbg.Assert(jobSourceAdapterType != null, "JobSourceAdapterType should never be called with null value.");
            object instance = null;
            ConstructorInfo constructor = jobSourceAdapterType.GetConstructor(Type.EmptyTypes);
            if (!constructor.IsPublic)
                string message = string.Format(CultureInfo.CurrentCulture,
                                                RemotingErrorIdStrings.JobManagerRegistrationConstructorError,
                                                jobSourceAdapterType.FullName);
                instance = constructor.Invoke(null);
            catch (MemberAccessException exception)
            catch (TargetInvocationException exception)
            catch (TargetParameterCountException exception)
            catch (NotSupportedException exception)
            catch (SecurityException exception)
                    _sourceAdapters.Add(jobSourceAdapterType.Name, (JobSourceAdapter)instance);
        /// Returns a token that allows a job to be constructed with a specific id and instanceId.
        /// The original job must have been saved using "SaveJobIdForReconstruction" in the JobSourceAdapter.
        /// <param name="instanceId">The instance id desired.</param>
        /// <param name="typeName">The requesting type name for JobSourceAdapter implementation.</param>
        /// <returns>Token for job creation.</returns>
        internal static JobIdentifier GetJobIdentifier(Guid instanceId, string typeName)
                KeyValuePair<int, string> keyValuePair;
                if (s_jobIdsForReuse.TryGetValue(instanceId, out keyValuePair) && keyValuePair.Value.Equals(typeName))
                    return new JobIdentifier(keyValuePair.Key, instanceId);
        /// Saves the Id information for a job so that it can be constructed at a later time by a JobSourceAdapter
        /// with the same type.
        /// <param name="instanceId">The instance id to save.</param>
        /// <param name="id">The session specific id to save.</param>
        /// <param name="typeName">The type name for the JobSourceAdapter implementation doing the save.</param>
        internal static void SaveJobId(Guid instanceId, int id, string typeName)
                if (s_jobIdsForReuse.ContainsKey(instanceId))
                s_jobIdsForReuse.Add(instanceId, new KeyValuePair<int, string>(id, typeName));
        #region NewJob
        /// Creates a new job of the appropriate type given by JobDefinition passed in.
        /// <param name="definition">JobDefinition defining the command.</param>
        /// <returns>Job2 object of the appropriate type specified by the definition.</returns>
        /// <exception cref="InvalidOperationException">If JobSourceAdapter type specified
        /// in definition is not registered.</exception>
        /// <exception cref="Exception">JobSourceAdapter implementation exception thrown on error.
        public Job2 NewJob(JobDefinition definition)
            ArgumentNullException.ThrowIfNull(definition);
            JobSourceAdapter sourceAdapter = GetJobSourceAdapter(definition);
            Job2 newJob;
                newJob = sourceAdapter.NewJob(definition);
                // Since we are calling into 3rd party code
                // catching Exception is allowed. In all
                // other cases the appropriate exception
                // needs to be caught.
                // sourceAdapter.NewJob returned unknown error.
            return newJob;
        /// <param name="specification">JobInvocationInfo defining the command.</param>
        public Job2 NewJob(JobInvocationInfo specification)
            ArgumentNullException.ThrowIfNull(specification);
            if (specification.Definition == null)
                throw new ArgumentException(RemotingErrorIdStrings.NewJobSpecificationError, nameof(specification));
            JobSourceAdapter sourceAdapter = GetJobSourceAdapter(specification.Definition);
            Job2 newJob = null;
                newJob = sourceAdapter.NewJob(specification);
        #endregion NewJob
        #region Persist Job
        /// Saves the job to a persisted store.
        /// <param name="job">Job2 type job to persist.</param>
        /// <param name="definition">Job definition containing source adapter information.</param>
        public void PersistJob(Job2 job, JobDefinition definition)
            if (job == null)
                throw new PSArgumentNullException(nameof(job));
            if (definition == null)
                throw new PSArgumentNullException(nameof(definition));
                sourceAdapter.PersistJob(job);
        /// Helper method, finds source adapter if registered, otherwise throws
        /// an InvalidOperationException.
        /// <param name="adapterTypeName">The name of the JobSourceAdapter derivative desired.</param>
        /// <returns>The JobSourceAdapter instance.</returns>
        /// is not found.</exception>
        private JobSourceAdapter AssertAndReturnJobSourceAdapter(string adapterTypeName)
            JobSourceAdapter adapter;
                if (!_sourceAdapters.TryGetValue(adapterTypeName, out adapter))
                    throw new InvalidOperationException(RemotingErrorIdStrings.JobSourceAdapterNotFound);
        /// Helper method to find and return the job source adapter if currently loaded or
        /// otherwise load the associated module and the requested source adapter.
        /// <param name="definition">JobDefinition supplies the JobSourceAdapter information.</param>
        /// <returns>JobSourceAdapter.</returns>
        private JobSourceAdapter GetJobSourceAdapter(JobDefinition definition)
            string adapterTypeName;
            if (!string.IsNullOrEmpty(definition.JobSourceAdapterTypeName))
                adapterTypeName = definition.JobSourceAdapterTypeName;
            else if (definition.JobSourceAdapterType != null)
                adapterTypeName = definition.JobSourceAdapterType.Name;
            bool adapterFound = false;
                adapterFound = _sourceAdapters.TryGetValue(adapterTypeName, out adapter);
            if (!adapterFound)
                if (!string.IsNullOrEmpty(definition.ModuleName))
                    // Attempt to load the module.
                        InitialSessionState iss = InitialSessionState.CreateDefault2();
                        iss.Commands.Clear();
                        iss.Commands.Add(new SessionStateCmdletEntry("Import-Module", typeof(Microsoft.PowerShell.Commands.ImportModuleCommand), null));
                        using (PowerShell powerShell = PowerShell.Create(iss))
                            powerShell.AddParameter("Name", definition.ModuleName);
                            if (powerShell.ErrorBuffer.Count > 0)
                                ex = powerShell.ErrorBuffer[0].Exception;
                        ex = e;
                    catch (ScriptCallDepthException e)
                        throw new InvalidOperationException(RemotingErrorIdStrings.JobSourceAdapterNotFound, ex);
                    // Now try getting the job source adapter again.
                    adapter = AssertAndReturnJobSourceAdapter(adapterTypeName);
        #region GetJobs
        /// Get list of all jobs.
        /// <param name="cmdlet">Cmdlet requesting this, for error processing.</param>
        /// <param name="writeErrorOnException"></param>
        /// <param name="writeObject"></param>
        /// <param name="jobSourceAdapterTypes">Job source adapter type names.</param>
        /// <exception cref="Exception">If cmdlet parameter is null, throws exception on error from
        /// JobSourceAdapter implementation.</exception>
        internal List<Job2> GetJobs(
            bool writeErrorOnException,
            bool writeObject,
            string[] jobSourceAdapterTypes)
            return GetFilteredJobs(null, FilterType.None, cmdlet, writeErrorOnException, writeObject, false, jobSourceAdapterTypes);
        /// Get list of jobs that matches the specified names.
        /// <param name="name">Names to match, can support
        ///   wildcard if the store supports.</param>
        /// <returns>Collection of jobs that match the specified
        /// criteria.</returns>
        internal List<Job2> GetJobsByName(
            return GetFilteredJobs(name, FilterType.Name, cmdlet, writeErrorOnException, writeObject, recurse, jobSourceAdapterTypes);
        /// Get list of jobs that run the specified command.
        /// <param name="command">Command to match.</param>
        internal List<Job2> GetJobsByCommand(
            return GetFilteredJobs(command, FilterType.Command, cmdlet, writeErrorOnException, writeObject, recurse, jobSourceAdapterTypes);
        /// Get list of jobs that are in the specified state.
        /// <param name="state">State to match.</param>
        /// <returns>Collection of jobs with the specified
        /// state.</returns>
        internal List<Job2> GetJobsByState(
            JobState state,
            return GetFilteredJobs(state, FilterType.State, cmdlet, writeErrorOnException, writeObject, recurse, jobSourceAdapterTypes);
        /// Get list of jobs based on the adapter specific
        /// filter parameters.
        /// <param name="filter">Dictionary containing name value
        ///   pairs for adapter specific filters.</param>
        /// <returns>Collection of jobs that match the
        /// specified criteria.</returns>
        internal List<Job2> GetJobsByFilter(Dictionary<string, object> filter, Cmdlet cmdlet, bool writeErrorOnException, bool writeObject, bool recurse)
            return GetFilteredJobs(filter, FilterType.Filter, cmdlet, writeErrorOnException, writeObject, recurse, null);
        /// Get a filtered list of jobs based on adapter name.
        /// <param name="id">Job id.</param>
        /// <param name="name">Adapter name.</param>
        internal bool IsJobFromAdapter(Guid id, string name)
                foreach (JobSourceAdapter sourceAdapter in _sourceAdapters.Values)
                    if (sourceAdapter.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                        return (sourceAdapter.GetJobByInstanceId(id, false) != null);
        /// Get a filtered list of jobs based on filter type.
        /// <param name="filter">Object to use for filtering.</param>
        /// <param name="filterType">Type of filter, specifies which "get" from
        ///   JobSourceAdapter to call, and dictates the type for filter.</param>
        /// <returns>Filtered list of jobs.</returns>
        private List<Job2> GetFilteredJobs(
            object filter,
            FilterType filterType,
            Diagnostics.Assert(cmdlet != null, "Cmdlet should be passed to JobManager");
            List<Job2> allJobs = new List<Job2>();
                    List<Job2> jobs = null;
                    // Filter search based on job source adapter types if provided.
                    if (!CheckTypeNames(sourceAdapter, jobSourceAdapterTypes))
                        jobs = CallJobFilter(sourceAdapter, filter, filterType, recurse);
                        // sourceAdapter.GetJobsByFilter() threw unknown exception.
                        WriteErrorOrWarning(writeErrorOnException, cmdlet, exception, "JobSourceAdapterGetJobsError", sourceAdapter);
                    if (jobs == null)
                    allJobs.AddRange(jobs);
            if (writeObject)
                foreach (Job2 job in allJobs)
                    cmdlet.WriteObject(job);
            return allJobs;
        /// Compare sourceAdapter name with the provided source adapter type
        /// name list.
        /// <param name="sourceAdapter"></param>
        /// <param name="jobSourceAdapterTypes"></param>
        private static bool CheckTypeNames(JobSourceAdapter sourceAdapter, string[] jobSourceAdapterTypes)
            // If no type names were specified then allow all adapter types.
            if (jobSourceAdapterTypes == null ||
                jobSourceAdapterTypes.Length == 0)
            string sourceAdapterName = GetAdapterName(sourceAdapter);
            Diagnostics.Assert(sourceAdapterName != null, "Source adapter should have name or type.");
            // Look for name match allowing wildcards.
            foreach (string typeName in jobSourceAdapterTypes)
                WildcardPattern typeNamePattern = WildcardPattern.Get(typeName, WildcardOptions.IgnoreCase);
                if (typeNamePattern.IsMatch(sourceAdapterName))
        private static string GetAdapterName(JobSourceAdapter sourceAdapter)
            return (!string.IsNullOrEmpty(sourceAdapter.Name) ?
                sourceAdapter.Name :
                sourceAdapter.GetType().ToString());
        /// Gets a filtered list of jobs from the given JobSourceAdapter.
        /// <param name="sourceAdapter">JobSourceAdapter to query.</param>
        /// <param name="filter">Filter object.</param>
        /// <param name="filterType">Filter type.</param>
        /// <returns>List of jobs from sourceAdapter filtered on filterType.</returns>
        /// <exception cref="Exception">Throws exception on error from JobSourceAdapter
        /// implementation.</exception>
        private static List<Job2> CallJobFilter(JobSourceAdapter sourceAdapter, object filter, FilterType filterType, bool recurse)
            List<Job2> jobs = new List<Job2>();
            IList<Job2> matches;
            switch (filterType)
                case FilterType.Command:
                    matches = sourceAdapter.GetJobsByCommand((string)filter, recurse);
                case FilterType.Filter:
                    matches = sourceAdapter.GetJobsByFilter((Dictionary<string, object>)filter, recurse);
                case FilterType.Name:
                    matches = sourceAdapter.GetJobsByName((string)filter, recurse);
                case FilterType.State:
                    matches = sourceAdapter.GetJobsByState((JobState)filter, recurse);
                case FilterType.None:
                    matches = sourceAdapter.GetJobs();
            if (matches != null)
                jobs.AddRange(matches);
            return jobs;
        /// Get job specified by the session specific id provided.
        /// <param name="id">Session specific job id.</param>
        /// <returns>Job that match the specified criteria.</returns>
        internal Job2 GetJobById(int id, Cmdlet cmdlet, bool writeErrorOnException, bool writeObject, bool recurse)
            return GetJobThroughId<int>(Guid.Empty, id, cmdlet, writeErrorOnException, writeObject, recurse);
        /// Get job that has the specified id.
        /// <param name="instanceId">Guid to match.</param>
        /// <returns>Job with the specified guid.</returns>
        internal Job2 GetJobByInstanceId(Guid instanceId, Cmdlet cmdlet, bool writeErrorOnException, bool writeObject, bool recurse)
            return GetJobThroughId<Guid>(instanceId, 0, cmdlet, writeErrorOnException, writeObject, recurse);
        private Job2 GetJobThroughId<T>(Guid guid, int id, Cmdlet cmdlet, bool writeErrorOnException, bool writeObject, bool recurse)
            Diagnostics.Assert(cmdlet != null, "Cmdlet should always be passed to JobManager");
            Job2 job = null;
                        if (typeof(T) == typeof(Guid))
                            Diagnostics.Assert(id == 0, "id must be zero when invoked with guid");
                            job = sourceAdapter.GetJobByInstanceId(guid, recurse);
                        else if (typeof(T) == typeof(int))
                            Diagnostics.Assert(guid == Guid.Empty, "Guid must be empty when used with int");
                            job = sourceAdapter.GetJobBySessionId(id, recurse);
                        // sourceAdapter.GetJobByInstanceId threw unknown exception.
                        WriteErrorOrWarning(writeErrorOnException, cmdlet, exception, "JobSourceAdapterGetJobByInstanceIdError", sourceAdapter);
                    return job;
        /// Gets or creates a Job2 object with the given definition name, path
        /// and definition type if specified, that can be run via the StartJob()
        /// <param name="definitionName">Job definition name.</param>
        /// <param name="definitionPath">Job definition file path.</param>
        /// <param name="definitionType">JobSourceAdapter type that contains the job definition.</param>
        /// <param name="cmdlet">Cmdlet making call.</param>
        /// <param name="writeErrorOnException">Whether to write jobsourceadapter errors.</param>
        /// <returns>List of matching Job2 objects.</returns>
        internal List<Job2> GetJobToStart(
            string definitionName,
            string definitionPath,
            string definitionType,
            bool writeErrorOnException)
            WildcardPattern typeNamePattern = (definitionType != null) ?
                WildcardPattern.Get(definitionType, WildcardOptions.IgnoreCase) : null;
                        if (typeNamePattern != null)
                            if (!typeNamePattern.IsMatch(sourceAdapterName))
                        Job2 job = sourceAdapter.NewJob(definitionName, definitionPath);
                            jobs.Add(job);
                            // Adapter type found, can quit.
        private static void WriteErrorOrWarning(bool writeErrorOnException, Cmdlet cmdlet, Exception exception, string identifier, JobSourceAdapter sourceAdapter)
                if (writeErrorOnException)
                    cmdlet.WriteError(new ErrorRecord(exception, identifier, ErrorCategory.OpenError, sourceAdapter));
                    // Write a warning
                                                   RemotingErrorIdStrings.JobSourceAdapterError,
                                                   exception.Message,
                                                   sourceAdapter.Name);
                // if this call is not made from a cmdlet thread or if
                // the cmdlet is closed this will thrown an exception
                // it is fine to eat that exception
        /// Returns a List of adapter names currently loaded.
        /// <param name="adapterTypeNames">Adapter names to filter on.</param>
        /// <returns>List of names.</returns>
        internal List<string> GetLoadedAdapterNames(string[] adapterTypeNames)
            List<string> adapterNames = new List<string>();
                    if (CheckTypeNames(sourceAdapter, adapterTypeNames))
                        adapterNames.Add(GetAdapterName(sourceAdapter));
            return adapterNames;
        #endregion GetJobs
        #region RemoveJob
        /// Remove a job from the appropriate store.
        /// <param name="sessionJobId">Session specific Job ID to remove.</param>
        internal void RemoveJob(int sessionJobId, Cmdlet cmdlet, bool writeErrorOnException)
            Job2 job = GetJobById(sessionJobId, cmdlet, writeErrorOnException, false, false);
            RemoveJob(job, cmdlet, false);
        /// <param name="job">Job object to remove.</param>
        /// <param name="throwExceptions">If true, will throw all JobSourceAdapter exceptions to caller.
        /// This is needed if RemoveJob is being called from an event handler in Receive-Job.</param>
        /// <returns>True if job is found.</returns>
        internal bool RemoveJob(Job2 job, Cmdlet cmdlet, bool writeErrorOnException, bool throwExceptions = false)
            bool jobFound = false;
                    Job2 foundJob = null;
                        foundJob = sourceAdapter.GetJobByInstanceId(job.InstanceId, true);
                        // sourceAdapter.GetJobByInstanceId() threw unknown exception.
                        if (throwExceptions)
                        WriteErrorOrWarning(writeErrorOnException, cmdlet, exception, "JobSourceAdapterGetJobError", sourceAdapter);
                    if (foundJob == null)
                    jobFound = true;
                    RemoveJobIdForReuse(foundJob);
                        sourceAdapter.RemoveJob(job);
                        // sourceAdapter.RemoveJob() threw unknown exception.
                        WriteErrorOrWarning(writeErrorOnException, cmdlet, exception, "JobSourceAdapterRemoveJobError", sourceAdapter);
            if (!jobFound && throwExceptions)
                var message = PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.ItemNotFoundInRepository,
                            "Job repository", job.InstanceId.ToString());
            return jobFound;
        private void RemoveJobIdForReuse(Job job)
            Hashtable duplicateDetector = new Hashtable();
            duplicateDetector.Add(job.Id, job.Id);
            RemoveJobIdForReuseHelper(duplicateDetector, job);
        private void RemoveJobIdForReuseHelper(Hashtable duplicateDetector, Job job)
                s_jobIdsForReuse.Remove(job.InstanceId);
            foreach (Job child in job.ChildJobs)
                if (duplicateDetector.ContainsKey(child.Id))
                duplicateDetector.Add(child.Id, child.Id);
                RemoveJobIdForReuse(child);
        #endregion RemoveJob
        /// Filters available for GetJob, used internally to centralize Exception handling.
        private enum FilterType
            /// Use no filter.
            /// Filter on command (string).
            /// Filter on custom dictionary (dictionary(string, object)).
            /// Filter on name (string).
            /// Filter on job state (JobState).
            State

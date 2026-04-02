    /// Contains the definition of a job which is defined in a
    /// job store.
    /// <remarks>The actual implementation of this class will
    /// happen in M2</remarks>
    public class JobDefinition : ISerializable
        /// A friendly Name for this definition.
        /// The type that derives from JobSourceAdapter
        /// that contains the logic for invocation and
        /// management of this type of job.
        public Type JobSourceAdapterType { get; }
        private string _moduleName;
        /// Module name for the module containing
        /// the source adapter implementation.
            get { return _moduleName; }
            set { _moduleName = value; }
        private string _jobSourceAdapterTypeName;
        /// Job source adapter type name.
        public string JobSourceAdapterTypeName
            get { return _jobSourceAdapterTypeName; }
            set { _jobSourceAdapterTypeName = value; }
        /// Name of the job that needs to be loaded
        /// from the specified module.
        private Guid _instanceId;
        /// Unique Guid for this job definition.
                return _instanceId;
                _instanceId = value;
        /// Save this definition to the specified
        /// file on disk.
        /// <param name="stream">Stream to save to.</param>
        public virtual void Save(Stream stream)
        /// Load this definition from the specified
        public virtual void Load(Stream stream)
        /// Returns information about this job like
        /// name, definition, parameters etc.
        public CommandInfo CommandInfo
        /// Public constructor for testing.
        /// <param name="jobSourceAdapterType">Type of adapter to use to create a job.</param>
        /// <param name="command">The command string.</param>
        /// <param name="name">The job name.</param>
        public JobDefinition(Type jobSourceAdapterType, string command, string name)
            JobSourceAdapterType = jobSourceAdapterType;
            if (jobSourceAdapterType != null)
                _jobSourceAdapterTypeName = jobSourceAdapterType.Name;
            _instanceId = Guid.NewGuid();
        protected JobDefinition(SerializationInfo info, StreamingContext context)
    /// Class that helps define the parameters to
    /// be passed to a job so that the job can be
    /// instantiated without having to specify
    /// the parameters explicitly. Helps in
    /// passing job parameters to disk.
    /// <remarks>This class is not required if
    /// CommandParameterCollection adds a public
    /// constructor.The actual implementation of
    /// this class will happen in M2</remarks>
    public class JobInvocationInfo : ISerializable
        /// Friendly name associated with this specification.
                    throw new PSArgumentNullException("value");
        private string _command;
        /// Command string to execute.
        public string Command
                return _command ?? _definition.Command;
        private JobDefinition _definition;
        /// Definition associated with the job.
        public JobDefinition Definition
                _definition = value;
        /// Parameters associated with this specification.
        public List<CommandParameterCollection> Parameters
            get { return _parameters ??= new List<CommandParameterCollection>(); }
        /// Unique identifies for this specification.
        /// Save this specification to a file.
        /// Load this specification from a file.
        /// <param name="stream">Stream to load from.</param>
        protected JobInvocationInfo()
        /// Create a new job definition with a single set of parameters.
        /// <param name="definition">The job definition.</param>
        /// <param name="parameters">The parameter collection to use.</param>
        public JobInvocationInfo(JobDefinition definition, Dictionary<string, object> parameters)
            var convertedCollection = ConvertDictionaryToParameterCollection(parameters);
            if (convertedCollection != null)
                Parameters.Add(convertedCollection);
        /// Create a new job definition with a multiple sets of parameters. This allows
        /// different parameters for different machines.
        /// <param name="parameterCollectionList">Collection of sets of parameters to use for the child jobs.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "This is forced by the interaction of PowerShell and Workflow.")]
        public JobInvocationInfo(JobDefinition definition, IEnumerable<Dictionary<string, object>> parameterCollectionList)
            if (parameterCollectionList == null) return;
            foreach (var parameterCollection in parameterCollectionList)
                if (parameterCollection == null) continue;
                CommandParameterCollection convertedCollection = ConvertDictionaryToParameterCollection(parameterCollection);
        /// <param name="definition"></param>
        public JobInvocationInfo(JobDefinition definition, CommandParameterCollection parameters)
            Parameters.Add(parameters ?? new CommandParameterCollection());
        public JobInvocationInfo(JobDefinition definition, IEnumerable<CommandParameterCollection> parameters)
            if (parameters == null) return;
                Parameters.Add(parameter);
        protected JobInvocationInfo(SerializationInfo info, StreamingContext context)
        /// Utility function to turn a dictionary of name/value pairs into a parameter collection.
        /// <param name="parameters">The dictionary to convert.</param>
        /// <returns>The converted collection.</returns>
        private static CommandParameterCollection ConvertDictionaryToParameterCollection(IEnumerable<KeyValuePair<string, object>> parameters)
            CommandParameterCollection paramCollection = new CommandParameterCollection();
            foreach (CommandParameter paramItem in
                parameters.Select(static param => new CommandParameter(param.Key, param.Value)))
                paramCollection.Add(paramItem);
            return paramCollection;
    /// Abstract class for a job store which will
    /// contain the jobs of a specific type.
    public abstract class JobSourceAdapter
        /// Name for this store.
        /// Get a token that allows for construction of a job with a previously assigned
        /// Id and InstanceId. This is only possible if this JobSourceAdapter is the
        /// creator of the original job.
        /// The original job must have been saved using "SaveJobIdForReconstruction"
        /// <param name="instanceId">Instance Id of the job to recreate.</param>
        /// <returns>JobIdentifier to be used in job construction.</returns>
        protected JobIdentifier RetrieveJobIdForReuse(Guid instanceId)
            return JobManager.GetJobIdentifier(instanceId, this.GetType().Name);
        /// Saves the Id information for a job so that it can be constructed at a later time.
        /// This will only allow this job source adapter type to recreate the job.
        /// <param name="job">The job whose id information to store.</param>
        /// <param name="recurse">Recurse to save child job Ids.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Only jobs that derive from Job2 should have reusable IDs.")]
        public void StoreJobIdForReuse(Job2 job, bool recurse)
                PSTraceSource.NewArgumentNullException(nameof(job), RemotingErrorIdStrings.JobSourceAdapterCannotSaveNullJob);
            JobManager.SaveJobId(job.InstanceId, job.Id, this.GetType().Name);
            if (recurse && job.ChildJobs != null && job.ChildJobs.Count > 0)
                duplicateDetector.Add(job.InstanceId, job.InstanceId);
                    if (child is not Job2 childJob) continue;
                    StoreJobIdForReuseHelper(duplicateDetector, childJob, true);
        private void StoreJobIdForReuseHelper(Hashtable duplicateDetector, Job2 job, bool recurse)
            if (duplicateDetector.ContainsKey(job.InstanceId)) return;
            if (!recurse || job.ChildJobs == null) return;
                StoreJobIdForReuseHelper(duplicateDetector, childJob, recurse);
        /// Create a new job with the specified definition.
        /// <param name="definition">Job definition to use.</param>
        /// <returns>Job object.</returns>
            return NewJob(new JobInvocationInfo(definition, new Dictionary<string, object>()));
        /// Creates a new job with the definition as specified by
        /// the provided definition name and path.  If path is null
        /// then a default location will be used to find the job
        /// definition by name.
        /// <returns>Job2 object.</returns>
        public virtual Job2 NewJob(string definitionName, string definitionPath)
        /// Create a new job with the specified JobSpecification.
        /// <param name="specification">Specification.</param>
        public abstract Job2 NewJob(JobInvocationInfo specification);
        /// Get the list of jobs that are currently available in this
        /// <returns>Collection of job objects.</returns>
        public abstract IList<Job2> GetJobs();
        /// <param name="name">names to match, can support
        ///   wildcard if the store supports</param>
        public abstract IList<Job2> GetJobsByName(string name, bool recurse);
        public abstract IList<Job2> GetJobsByCommand(string command, bool recurse);
        /// Get list of jobs that has the specified id.
        public abstract Job2 GetJobByInstanceId(Guid instanceId, bool recurse);
        /// Get job that has specific session id.
        /// <param name="id">Id to match.</param>
        /// <returns>Job with the specified id.</returns>
        public abstract Job2 GetJobBySessionId(int id, bool recurse);
        public abstract IList<Job2> GetJobsByState(JobState state, bool recurse);
        /// <param name="filter">dictionary containing name value
        ///   pairs for adapter specific filters</param>
        public abstract IList<Job2> GetJobsByFilter(Dictionary<string, object> filter, bool recurse);
        /// Remove a job from the store.
        public abstract void RemoveJob(Job2 job);
        public virtual void PersistJob(Job2 job)
            // Implemented only if job needs to be told when to persist.

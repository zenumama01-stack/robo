    /// Class which has list of job objects currently active in the system.
    public abstract class Repository<T> where T : class
        /// Add an item to the repository.
        /// <param name="item">Object to add.</param>
            ArgumentNullException.ThrowIfNull(item, _identifier);
                Guid instanceId = GetKey(item);
                if (!_repository.ContainsKey(instanceId))
                    _repository.Add(instanceId, item);
                    throw new ArgumentException(_identifier);
        /// Remove the specified item from the repository.
        /// <param name="item">Object to remove.</param>
        public void Remove(T item)
                if (!_repository.Remove(instanceId))
                        PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.ItemNotFoundInRepository,
                            "Job repository", instanceId.ToString());
        public List<T> GetItems()
            return Items;
        /// Get a key for the specified item.
        /// <param name="item">Item for which the key is required.</param>
        /// <returns>Returns a key.</returns>
        protected abstract Guid GetKey(T item);
        protected Repository(string identifier)
            _identifier = identifier;
        /// Creates a repository with the specified values.
        internal List<T> Items
                    return new List<T>(_repository.Values);
        /// Gets the specified Item.
        public T GetItem(Guid instanceId)
                _repository.TryGetValue(instanceId, out result);
        /// Gets the Repository dictionary.
        internal Dictionary<Guid, T> Dictionary
            get { return _repository; }
        private readonly Dictionary<Guid, T> _repository = new Dictionary<Guid, T>();
        private readonly object _syncObject = new object();      // object for synchronization
        private readonly string _identifier;
    public class JobRepository : Repository<Job>
        /// Returns the list of available job objects.
        public List<Job> Jobs
        /// Returns the Job whose InstanceId matches the parameter.
        /// The matching Job. Null if no match is found.
        public Job GetJob(Guid instanceId)
            return GetItem(instanceId);
        internal JobRepository() : base("job")
        /// Returns the instance id of the job as key.
        /// <param name="item">Job for which a key is required.</param>
        /// <returns>Returns jobs guid.</returns>
        protected override Guid GetKey(Job item)
                return item.InstanceId;

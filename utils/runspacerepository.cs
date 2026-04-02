    /// Repository of remote runspaces available in a local runspace.
    public class RunspaceRepository : Repository<PSSession>
        /// Collection of runspaces available.
        public List<PSSession> Runspaces
        internal RunspaceRepository() : base("runspace")
        /// Gets a key for the specified item.
        protected override Guid GetKey(PSSession item)
        /// Adds the PSSession item to the repository if it doesn't already
        /// exist or replaces the existing one.
        /// <param name="item">PSSession object.</param>
        internal void AddOrReplace(PSSession item)
            this.Dictionary[GetKey(item)] = item;

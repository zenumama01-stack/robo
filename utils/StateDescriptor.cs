    /// Base proxy class for other classes which wish to have save and restore functionality.
    /// <typeparam name="T">There are no restrictions on T.</typeparam>
    public abstract class StateDescriptor<T>
        private Guid id;
        private string name;
        /// Creates a new instances of the StateDescriptor class and creates a new GUID.
        protected StateDescriptor()
            this.Id = Guid.NewGuid();
        /// Constructor overload to provide name.
        /// <param name="name">The friendly name for the StateDescriptor.</param>
        protected StateDescriptor(string name)
            : this()
            this.Name = name;
        /// Gets the global unique identification number.
        public Guid Id
                return this.id;
            protected set
                this.id = value;
        /// Gets or sets the friendly display name.
        public string Name
                return this.name;
                this.name = value;
        /// Saves a snapshot of the subject's current state.
        /// <param name="subject">The object whose state will be saved.</param>
        public abstract void SaveState(T subject);
        /// Restores the state of subject to the saved state.
        /// <param name="subject">The object whose state will be restored.</param>
        public abstract void RestoreState(T subject);

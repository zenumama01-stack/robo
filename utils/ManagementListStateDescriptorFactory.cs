    /// Defines a factory which returns ManagementListStateDescriptors.
    public class ManagementListStateDescriptorFactory : IStateDescriptorFactory<ManagementList>
        /// Factory method that creates a ManagementListStateDescriptor.
        /// <returns>A new ManagementListStateDescriptor.</returns>
        public StateDescriptor<ManagementList> Create()
            return new ManagementListStateDescriptor();

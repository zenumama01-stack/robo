    /// Defines an interface for a factory that creates
    /// StateDescriptors.
    /// <typeparam name="T">The type T used by the StateDescriptor.</typeparam>
    public interface IStateDescriptorFactory<T>
        /// Creates a new StateDescriptor based upon custom
        /// logic.
        /// <returns>A new StateDescriptor.</returns>
        StateDescriptor<T> Create();

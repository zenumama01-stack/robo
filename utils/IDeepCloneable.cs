    /// Defines a generalized method for creating a deep copy of an instance.
    internal interface IDeepCloneable
        /// Creates a deep copy of the current instance.
        /// <returns>A new object that is a deep copy of the current instance.</returns>
        object DeepClone();

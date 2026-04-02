    /// The FilterStatus enum is used to classify the current status a <see cref="FilterEvaluator" /> is in.
    public enum FilterStatus
        /// A FilterStatus of NotApplied indicates that the filter is currently
        /// not applied.
        NotApplied = 0,
        /// A FilterStatus of InProgress indicates that the filter is being
        /// applied but is not done.
        InProgress = 1,
        /// A FilterStatus of Applied indicates that the filter has been
        /// applied.
        Applied = 2

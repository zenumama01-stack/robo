    /// Defines type which has information about RunspacePoolState
    /// and exception associated with that state.
    /// <remarks>This class is created so that a state change along
    /// with its reason can be transported from the server to the
    /// client in case of RemoteRunspacePool</remarks>
    public sealed class RunspacePoolStateInfo
        /// State of the runspace pool when this event occurred.
        public RunspacePoolState State { get; }
        /// Exception associated with that state.
        /// Constructor for creating the state info.
        /// <param name="state">State.</param>
        /// <param name="reason">exception that resulted in this
        /// state change. Can be null</param>
        public RunspacePoolStateInfo(RunspacePoolState state, Exception reason)

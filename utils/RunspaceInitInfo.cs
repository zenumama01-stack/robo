    /// Class that encapsulates the information carried by the RunspaceInitInfo PSRP message.
    internal class RunspacePoolInitInfo
        /// Min Runspaces setting on the server runspace pool.
        internal int MinRunspaces { get; }
        /// Max Runspaces setting on the server runspace pool.
        internal int MaxRunspaces { get; }
        /// <param name="minRS"></param>
        /// <param name="maxRS"></param>
        internal RunspacePoolInitInfo(int minRS, int maxRS)
            MinRunspaces = minRS;
            MaxRunspaces = maxRS;

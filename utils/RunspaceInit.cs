    internal sealed partial
    class LocalRunspace : RunspaceBase
        /// Initialize default values of preference vars.
        private void InitializeDefaults()
            SessionStateInternal ss = _engine.Context.EngineSessionState;
            Dbg.Assert(ss != null, "SessionState should not be null");
            // Add the variables that must always be there...

    /// This is a singleton object that is used to indicate a void return result.
    /// It's a singleton class. Sealed to prevent subclassing. Any operation that
    /// returns no actual value should return this object AutomationNull.Value.
    /// Anything that evaluates a PowerShell expression should be prepared to deal
    /// with receiving this result and discarding it. When received in an
    /// evaluation where a value is required, it should be replaced with null.
    public static class AutomationNull
        #region private_members
        // Private member for Value.
        #endregion private_members
        #region public_property
        /// Returns the singleton instance of this object.
        public static PSObject Value { get; } = new PSObject();
        #endregion public_property

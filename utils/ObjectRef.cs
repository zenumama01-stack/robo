    /// The purpose of this class is to hide an object (mask it) and replace it
    /// with a substitute temporarily. This is used in pushing and popping
    /// runspaces. It is also used to temporarily set a PowerShell object's host as
    /// the Runspace object's host when the PowerShell object is executed.
    internal class ObjectRef<T> where T : class
        /// New value.
        private T _newValue;
        /// Old value.
        private readonly T _oldValue;
        internal T OldValue
                return _oldValue;
                if (_newValue == null)
                    return _newValue;
        /// Is overridden.
        internal bool IsOverridden
                return _newValue != null;
        /// Constructor for ObjectRef.
        internal ObjectRef(T oldValue)
            Dbg.Assert(oldValue != null, "Expected oldValue != null");
            _oldValue = oldValue;
        internal void Override(T newValue)
            Dbg.Assert(newValue != null, "Expected newValue != null");
            _newValue = newValue;
            _newValue = null;

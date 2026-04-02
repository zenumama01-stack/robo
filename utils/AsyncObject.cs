    /// Blocks caller trying to get the value of an object of type T
    /// until the value is set. After the set all future gets are
    /// unblocked.
    internal class AsyncObject<T> where T : class
        /// Value.
        private T _value;
        /// Value was set.
        private readonly ManualResetEvent _valueWasSet;
                bool result = _valueWasSet.WaitOne();
                _valueWasSet.Set();
        /// Constructor for AsyncObject.
        internal AsyncObject()
            _valueWasSet = new ManualResetEvent(false);

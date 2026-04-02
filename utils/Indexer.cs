    /// Provides an enumerator for iterating through a multi-dimensional array.
    /// This is needed to encode multi-dimensional arrays in remote host methods.
    internal class Indexer : IEnumerable, IEnumerator
        /// Current.
        private readonly int[] _current;
        /// Lengths.
        private readonly int[] _lengths;
        /// Constructor for Indexer.
        internal Indexer(int[] lengths)
            Dbg.Assert(lengths != null, "Expected lengths != null");
            _lengths = lengths;
            Dbg.Assert(CheckLengthsNonNegative(lengths), "Expected CheckLengthsNonNegative(lengths)");
            _current = new int[lengths.Length];
        /// Check lengths non negative.
        private static bool CheckLengthsNonNegative(int[] lengths)
            for (int i = 0; i < lengths.Length; ++i)
                if (lengths[i] < 0)
        /// Get enumerator.
        /// Reset.
            for (int i = 0; i < _current.Length; ++i)
                _current[i] = 0;
            // Set last value to -1 so that MoveNext makes this 0,0,...,0.
            if (_current.Length > 0)
                _current[_current.Length - 1] = -1;
        /// Move next.
            for (int i = _lengths.Length - 1; i >= 0; --i)
                // See if we can increment this dimension.
                if (_current[i] < _lengths[i] - 1)
                    _current[i]++;
                // Otherwise reset i and try to increment the next one.

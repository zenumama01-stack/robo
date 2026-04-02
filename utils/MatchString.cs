    /// Context information about a match.
    public sealed class MatchInfoContext : ICloneable
        internal MatchInfoContext()
        /// Gets or sets the lines found before a match.
        public string[] PreContext { get; set; }
        /// Gets or sets the lines found after a match.
        public string[] PostContext { get; set; }
        /// Gets or sets the lines found before a match. Does not include
        /// overlapping context and thus can be used to
        /// display contiguous match regions.
        public string[] DisplayPreContext { get; set; }
        /// Gets or sets the lines found after a match. Does not include
        public string[] DisplayPostContext { get; set; }
        /// Produce a deep copy of this object.
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
            return new MatchInfoContext()
                PreContext = (string[])PreContext?.Clone(),
                PostContext = (string[])PostContext?.Clone(),
                DisplayPreContext = (string[])DisplayPreContext?.Clone(),
                DisplayPostContext = (string[])DisplayPostContext?.Clone()
    /// The object returned by select-string representing the result of a match.
    public class MatchInfo
        private static readonly string s_inputStream = "InputStream";
        /// Gets or sets a value indicating whether the match was done ignoring case.
        /// <value>True if case was ignored.</value>
        public bool IgnoreCase { get; set; }
        /// Gets or sets the number of the matching line.
        /// <value>The number of the matching line.</value>
        public ulong LineNumber { get; set; }
        /// Gets or sets the text of the matching line.
        /// <value>The text of the matching line.</value>
        public string Line { get; set; } = string.Empty;
        /// Gets or sets a value indicating whether the matched portion of the string is highlighted.
        /// <value>Whether the matched portion of the string is highlighted with the negative VT sequence.</value>
        private readonly bool _emphasize;
        /// Stores the starting index of each match within the line.
        private readonly IReadOnlyList<int> _matchIndexes;
        /// Stores the length of each match within the line.
        private readonly IReadOnlyList<int> _matchLengths;
        /// Initializes a new instance of the <see cref="MatchInfo"/> class with emphasis disabled.
        public MatchInfo()
            this._emphasize = false;
        /// Initializes a new instance of the <see cref="MatchInfo"/> class with emphasized matched text.
        /// Used when virtual terminal sequences are supported.
        /// <param name="matchIndexes">Sets the matchIndexes.</param>
        /// <param name="matchLengths">Sets the matchLengths.</param>
        public MatchInfo(IReadOnlyList<int> matchIndexes, IReadOnlyList<int> matchLengths)
            this._emphasize = true;
            this._matchIndexes = matchIndexes;
            this._matchLengths = matchLengths;
        /// Gets the base name of the file containing the matching line.
        /// It will be the string "InputStream" if the object came from the input stream.
        /// This is a readonly property calculated from the path <see cref="Path"/>.
        /// <value>The file name.</value>
        public string Filename
                if (!_pathSet)
                    return s_inputStream;
                return _filename ??= System.IO.Path.GetFileName(_path);
        private string _filename;
        /// Gets or sets the full path of the file containing the matching line.
        /// It will be "InputStream" if the object came from the input stream.
        /// <value>The path name.</value>
            get => _pathSet ? _path : s_inputStream;
                _pathSet = true;
        private string _path = s_inputStream;
        private bool _pathSet;
        /// Gets or sets the pattern that was used in the match.
        /// <value>The pattern string.</value>
        public string Pattern { get; set; }
        /// Gets or sets context for the match, or null if -context was not specified.
        public MatchInfoContext Context { get; set; }
        /// Returns the path of the matching file truncated relative to the <paramref name="directory"/> parameter.
        /// For example, if the matching path was c:\foo\bar\baz.c and the directory argument was c:\foo
        /// the routine would return bar\baz.c .
        /// <param name="directory">The directory base the truncation on.</param>
        /// <returns>The relative path that was produced.</returns>
        public string RelativePath(string directory)
                return this.Path;
            string relPath = _path;
            if (!string.IsNullOrEmpty(directory))
                if (relPath.StartsWith(directory, StringComparison.OrdinalIgnoreCase))
                    int offset = directory.Length;
                    if (offset < relPath.Length)
                        if (directory[offset - 1] == '\\' || directory[offset - 1] == '/')
                            relPath = relPath.Substring(offset);
                        else if (relPath[offset] == '\\' || relPath[offset] == '/')
                            relPath = relPath.Substring(offset + 1);
            return relPath;
        private const string MatchFormat = "{0}{1}:{2}:{3}";
        private const string SimpleFormat = "{0}{1}";
        // Prefixes used by formatting: Match and Context prefixes
        // are used when context-tracking is enabled, otherwise
        // the empty prefix is used.
        private const string MatchPrefix = "> ";
        private const string ContextPrefix = "  ";
        private const string EmptyPrefix = "";
        /// Returns the string representation of this object. The format
        /// depends on whether a path has been set for this object or not.
        /// If the path component is set, as would be the case when matching
        /// in a file, ToString() would return the path, line number and line text.
        /// If path is not set, then just the line text is presented.
        /// <returns>The string representation of the match object.</returns>
            return ToString(null);
        /// Returns the string representation of the match object same format as ToString()
        /// but trims the path to be relative to the <paramref name="directory"/> argument.
        /// <param name="directory">Directory to use as the root when calculating the relative path.</param>
        public string ToString(string directory)
            return ToString(directory, Line);
        /// Returns the string representation of the match object with the matched line passed
        /// in as <paramref name="line"/> and trims the path to be relative to
        /// the<paramref name="directory"/> argument.
        /// <param name="line">Line that the match occurs in.</param>
        private string ToString(string directory, string line)
            string displayPath = (directory != null) ? RelativePath(directory) : _path;
            // Just return a single line if the user didn't
            // enable context-tracking.
            if (Context == null)
                return FormatLine(line, this.LineNumber, displayPath, EmptyPrefix);
            // Otherwise, render the full context.
            List<string> lines = new(Context.DisplayPreContext.Length + Context.DisplayPostContext.Length + 1);
            ulong displayLineNumber = this.LineNumber - (ulong)Context.DisplayPreContext.Length;
            foreach (string contextLine in Context.DisplayPreContext)
                lines.Add(FormatLine(contextLine, displayLineNumber++, displayPath, ContextPrefix));
            lines.Add(FormatLine(line, displayLineNumber++, displayPath, MatchPrefix));
            foreach (string contextLine in Context.DisplayPostContext)
            return string.Join(System.Environment.NewLine, lines.ToArray());
        /// and inverts the color of the matched text if virtual terminal is supported.
        /// <returns>The string representation of the match object with matched text inverted.</returns>
        public string ToEmphasizedString(string directory)
            if (!_emphasize)
                return ToString(directory);
            return ToString(directory, EmphasizeLine());
        /// Surrounds the matched text with virtual terminal sequences to invert it's color. Used in ToEmphasizedString.
        /// <returns>The matched line with matched text inverted.</returns>
        private string EmphasizeLine()
            string invertColorsVT100 = PSStyle.Instance.Reverse;
            string resetVT100 = PSStyle.Instance.Reset;
            char[] chars = new char[(_matchIndexes.Count * (invertColorsVT100.Length + resetVT100.Length)) + Line.Length];
            int lineIndex = 0;
            int charsIndex = 0;
            for (int i = 0; i < _matchIndexes.Count; i++)
                // Adds characters before match
                Line.CopyTo(lineIndex, chars, charsIndex, _matchIndexes[i] - lineIndex);
                charsIndex += _matchIndexes[i] - lineIndex;
                lineIndex = _matchIndexes[i];
                // Adds opening vt sequence
                invertColorsVT100.CopyTo(0, chars, charsIndex, invertColorsVT100.Length);
                charsIndex += invertColorsVT100.Length;
                // Adds characters being emphasized
                Line.CopyTo(lineIndex, chars, charsIndex, _matchLengths[i]);
                lineIndex += _matchLengths[i];
                charsIndex += _matchLengths[i];
                // Adds closing vt sequence
                resetVT100.CopyTo(0, chars, charsIndex, resetVT100.Length);
                charsIndex += resetVT100.Length;
            // Adds remaining characters in line
            Line.CopyTo(lineIndex, chars, charsIndex, Line.Length - lineIndex);
        /// <param name="lineStr">The line to format.</param>
        /// <param name="displayLineNumber">The line number to display.</param>
        /// <param name="displayPath">The file path, formatted for display.</param>
        /// <param name="prefix">The match prefix.</param>
        /// <returns>The formatted line as a string.</returns>
        private string FormatLine(string lineStr, ulong displayLineNumber, string displayPath, string prefix)
            return _pathSet
                       ? StringUtil.Format(MatchFormat, prefix, displayPath, displayLineNumber, lineStr)
                       : StringUtil.Format(SimpleFormat, prefix, lineStr);
        /// Gets or sets a list of all Regex matches on the matching line.
        public Match[] Matches { get; set; } = Array.Empty<Match>();
        /// Create a deep copy of this MatchInfo instance.
        internal MatchInfo Clone()
            // Just do a shallow copy and then deep-copy the
            // fields that need it.
            MatchInfo clone = (MatchInfo)this.MemberwiseClone();
            if (clone.Context != null)
                clone.Context = (MatchInfoContext)clone.Context.Clone();
            // Regex match objects are immutable, so we can get away
            // with just copying the array.
            clone.Matches = (Match[])clone.Matches.Clone();
            return clone;
    /// A cmdlet to search through strings and files for particular patterns.
    [Cmdlet(VerbsCommon.Select, "String", DefaultParameterSetName = ParameterSetFile, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097119")]
    [OutputType(typeof(bool), typeof(MatchInfo), ParameterSetName = new[] { ParameterSetFile, ParameterSetObject, ParameterSetLiteralFile })]
    [OutputType(typeof(string), ParameterSetName = new[] { ParameterSetFileRaw, ParameterSetObjectRaw, ParameterSetLiteralFileRaw })]
    public sealed class SelectStringCommand : PSCmdlet
        private const string ParameterSetFile = "File";
        private const string ParameterSetFileRaw = "FileRaw";
        private const string ParameterSetObject = "Object";
        private const string ParameterSetObjectRaw = "ObjectRaw";
        private const string ParameterSetLiteralFile = "LiteralFile";
        private const string ParameterSetLiteralFileRaw = "LiteralFileRaw";
        /// A generic circular buffer.
        /// <typeparam name="T">The type of items that are buffered.</typeparam>
        private sealed class CircularBuffer<T> : ICollection<T>
            // Ring of items
            private readonly T[] _items;
            // Current length, as opposed to the total capacity
            // Current start of the list. Starts at 0, but may
            // move forwards or wrap around back to 0 due to
            // rotation.
            private int _firstIndex;
            /// Initializes a new instance of the <see cref="CircularBuffer{T}"/> class.
            /// <param name="capacity">The maximum capacity of the buffer.</param>
            /// <exception cref="ArgumentOutOfRangeException">If <paramref name="capacity"/> is negative.</exception>
            public CircularBuffer(int capacity)
                ArgumentOutOfRangeException.ThrowIfNegative(capacity);
                _items = new T[capacity];
            /// Gets the maximum capacity of the buffer. If more items
            /// are added than the buffer has capacity for, then
            /// older items will be removed from the buffer with
            /// a first-in, first-out policy.
            public int Capacity => _items.Length;
            /// Whether or not the buffer is at capacity.
            public bool IsFull => Count == Capacity;
            /// Convert from a 0-based index to a buffer index which
            /// has been properly offset and wrapped.
            /// <param name="zeroBasedIndex">The index to wrap.</param>
            /// <exception cref="ArgumentOutOfRangeException">If <paramref name="zeroBasedIndex"/> is out of range.</exception>
            /// The actual index that <paramref name="zeroBasedIndex"/>
            /// maps to.
            private int WrapIndex(int zeroBasedIndex)
                if (Capacity == 0 || zeroBasedIndex < 0)
                    throw new ArgumentOutOfRangeException(nameof(zeroBasedIndex));
                return (zeroBasedIndex + _firstIndex) % Capacity;
            #region IEnumerable<T> implementation.
            public IEnumerator<T> GetEnumerator()
                    yield return _items[WrapIndex(i)];
            IEnumerator IEnumerable.GetEnumerator()
                return (IEnumerator)GetEnumerator();
            #region ICollection<T> implementation
            public int Count { get; private set; }
            public bool IsReadOnly => false;
            /// Adds an item to the buffer. If the buffer is already
            /// full, the oldest item in the list will be removed,
            /// and the new item added at the logical end of the list.
            /// <param name="item">The item to add.</param>
            public void Add(T item)
                if (Capacity == 0)
                int itemIndex;
                if (IsFull)
                    itemIndex = _firstIndex;
                    _firstIndex = (_firstIndex + 1) % Capacity;
                    itemIndex = _firstIndex + Count;
                _items[itemIndex] = item;
            public void Clear()
                _firstIndex = 0;
                Count = 0;
            public bool Contains(T item)
            public void CopyTo(T[] array, int arrayIndex)
                ArgumentNullException.ThrowIfNull(array);
                ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
                if (Count > (array.Length - arrayIndex))
                    throw new ArgumentException("arrayIndex");
                // Iterate through the buffer in correct order.
                foreach (T item in this)
                    array[arrayIndex++] = item;
            public bool Remove(T item)
            /// Create an array of the items in the buffer. Items
            /// will be in the same order they were added.
            /// <returns>The new array.</returns>
            public T[] ToArray()
                T[] result = new T[Count];
                CopyTo(result, 0);
            /// Access an item in the buffer. Indexing is based off
            /// of the order items were added, rather than any
            /// internal ordering the buffer may be maintaining.
            /// <param name="index">The index of the item to access.</param>
            /// <returns>The buffered item at index <paramref name="index"/>.</returns>
            public T this[int index]
                    if (!(index >= 0 && index < Count))
                        throw new ArgumentOutOfRangeException(nameof(index));
                    return _items[WrapIndex(index)];
        /// An interface to a context tracking algorithm.
        private interface IContextTracker
            /// Gets matches with completed context information
            /// that are ready to be emitted into the pipeline.
            IList<MatchInfo> EmitQueue { get; }
            /// Track a non-matching line for context.
            /// <param name="line">The line to track.</param>
            void TrackLine(string line);
            /// Track a matching line.
            /// <param name="match">The line to track.</param>
            void TrackMatch(MatchInfo match);
            /// Track having reached the end of the file,
            /// giving the tracker a chance to process matches with
            /// incomplete context information.
            void TrackEOF();
        /// A state machine to track display context for each match.
        private sealed class DisplayContextTracker : IContextTracker
            private enum ContextState
                InitialState,
                CollectPre,
                CollectPost,
            private ContextState _contextState = ContextState.InitialState;
            private readonly int _preContext;
            private readonly int _postContext;
            // The context leading up to the match.
            private readonly CircularBuffer<string> _collectedPreContext;
            // The context after the match.
            private readonly List<string> _collectedPostContext;
            // Current match info we are tracking postcontext for.
            // At any given time, if set, this value will not be
            // in the emitQueue but will be the next to be added.
            private MatchInfo _matchInfo = null;
            /// Initializes a new instance of the <see cref="DisplayContextTracker"/> class.
            /// <param name="preContext">How much preContext to collect at most.</param>
            /// <param name="postContext">How much postContext to collect at most.</param>
            public DisplayContextTracker(int preContext, int postContext)
                _preContext = preContext;
                _postContext = postContext;
                _collectedPreContext = new CircularBuffer<string>(preContext);
                _collectedPostContext = new List<string>(postContext);
                _emitQueue = new List<MatchInfo>();
                Reset();
            #region IContextTracker implementation
            public IList<MatchInfo> EmitQueue => _emitQueue;
            private readonly List<MatchInfo> _emitQueue;
            // Track non-matching line
            public void TrackLine(string line)
                switch (_contextState)
                    case ContextState.InitialState:
                    case ContextState.CollectPre:
                        _collectedPreContext.Add(line);
                    case ContextState.CollectPost:
                        // We're not done collecting post-context.
                        _collectedPostContext.Add(line);
                        if (_collectedPostContext.Count >= _postContext)
                            // Now we're done.
                            UpdateQueue();
            // Track matching line
            public void TrackMatch(MatchInfo match)
                // Update the queue in case we were in the middle
                // of collecting postcontext for an older match...
                if (_contextState == ContextState.CollectPost)
                // Update the current matchInfo.
                _matchInfo = match;
                // If postContext is set, then we need to hold
                // onto the match for a while and gather context.
                // Otherwise, immediately move the match onto the queue
                // and let UpdateQueue update our state instead.
                if (_postContext > 0)
                    _contextState = ContextState.CollectPost;
            // Track having reached the end of the file.
            public void TrackEOF()
                // If we're in the middle of collecting postcontext, we
                // already have a match and it's okay to queue it up
                // early since there are no more lines to track context
                // for.
            /// Moves matchInfo, if set, to the emitQueue and
            /// resets the tracking state.
            private void UpdateQueue()
                if (_matchInfo != null)
                    _emitQueue.Add(_matchInfo);
                    if (_matchInfo.Context != null)
                        _matchInfo.Context.DisplayPreContext = _collectedPreContext.ToArray();
                        _matchInfo.Context.DisplayPostContext = _collectedPostContext.ToArray();
            // Reset tracking state. Does not reset the emit queue.
            private void Reset()
                _contextState = (_preContext > 0)
                               ? ContextState.CollectPre
                               : ContextState.InitialState;
                _collectedPreContext.Clear();
                _collectedPostContext.Clear();
                _matchInfo = null;
        /// A class to track logical context for each match.
        /// The difference between logical and display context is
        /// that logical context includes as many context lines
        /// as possible for a given match, up to the specified
        /// limit, including context lines which overlap between
        /// matches and other matching lines themselves. Display
        /// context, on the other hand, is designed to display
        /// a possibly-continuous set of matches by excluding
        /// overlapping context (lines will only appear once)
        /// and other matching lines (since they will appear
        /// as their own match entries.).
        private sealed class LogicalContextTracker : IContextTracker
            // A union: string | MatchInfo. Needed since
            // context lines could be either proper matches
            // or non-matching lines.
            private sealed class ContextEntry
                public readonly string Line;
                public readonly MatchInfo Match;
                public ContextEntry(string line)
                    Line = line;
                public ContextEntry(MatchInfo match)
                    Match = match;
                public override string ToString() => Match?.Line ?? Line;
            // Whether or not early entries found
            // while still filling up the context buffer
            // have been added to the emit queue.
            // Used by UpdateQueue.
            private bool _hasProcessedPreEntries;
            // A circular buffer tracking both precontext and postcontext.
            // Essentially, the buffer is separated into regions:
            // | prectxt region  (older entries, length = precontext)  |
            // | match region    (length = 1)                          |
            // | postctxt region (newer entries, length = postcontext) |
            // When context entries containing a match reach the "middle"
            // (the position between the pre/post context regions)
            // of this buffer, and the buffer is full, we will know
            // enough context to populate the Context properties of the
            // match. At that point, we will add the match object
            // to the emit queue.
            private readonly CircularBuffer<ContextEntry> _collectedContext;
            /// Initializes a new instance of the <see cref="LogicalContextTracker"/> class.
            public LogicalContextTracker(int preContext, int postContext)
                _collectedContext = new CircularBuffer<ContextEntry>(preContext + postContext + 1);
                ContextEntry entry = new(line);
                _collectedContext.Add(entry);
                ContextEntry entry = new(match);
                // If the buffer is already full,
                // check for any matches with incomplete
                // postcontext and add them to the emit queue.
                // These matches can be identified by being past
                // the "middle" of the context buffer (still in
                // the postcontext region.
                // If the buffer isn't full, then nothing will have
                // ever been emitted and everything is still waiting
                // on postcontext. So process the whole buffer.
                int startIndex = _collectedContext.IsFull ? _preContext + 1 : 0;
                EmitAllInRange(startIndex, _collectedContext.Count - 1);
            /// Add all matches found in the specified range
            /// to the emit queue, collecting as much context
            /// as possible up to the limits specified in the constructor.
            /// The range is inclusive; the entries at
            /// startIndex and endIndex will both be checked.
            /// <param name="startIndex">The beginning of the match range.</param>
            /// <param name="endIndex">The ending of the match range.</param>
            private void EmitAllInRange(int startIndex, int endIndex)
                for (int i = startIndex; i <= endIndex; i++)
                    MatchInfo match = _collectedContext[i].Match;
                    if (match != null)
                        int preStart = Math.Max(i - _preContext, 0);
                        int postLength = Math.Min(_postContext, _collectedContext.Count - i - 1);
                        Emit(match, preStart, i - preStart, i + 1, postLength);
            /// Add match(es) found in the match region to the
            /// emit queue. Should be called every time an entry
            /// is added to the context buffer.
                // Are we at capacity and thus have enough postcontext?
                // Is there a match in the "middle" of the buffer
                // that we know the pre/post context for?
                // If this is the first time we've reached full capacity,
                // hasProcessedPreEntries will not be set, and we
                // should go through the entire context, because it might
                // have entries that never collected enough
                // precontext. Otherwise, we should just look at the
                // middle region.
                if (_collectedContext.IsFull)
                    if (_hasProcessedPreEntries)
                        // Only process a potential match with exactly
                        // enough pre and post-context.
                        EmitAllInRange(_preContext, _preContext);
                        // Some of our early entries may not
                        // have enough precontext. Process them too.
                        EmitAllInRange(0, _preContext);
                        _hasProcessedPreEntries = true;
            /// Collects context from the specified ranges. Populates
            /// the specified match with the collected context
            /// and adds it to the emit queue.
            /// Context ranges must be within the bounds of the context buffer.
            /// <param name="match">The match to operate on.</param>
            /// <param name="preStartIndex">The start index of the preContext range.</param>
            /// <param name="preLength">The length of the preContext range.</param>
            /// <param name="postStartIndex">The start index of the postContext range.</param>
            /// <param name="postLength">The length of the postContext range.</param>
            private void Emit(MatchInfo match, int preStartIndex, int preLength, int postStartIndex, int postLength)
                if (match.Context != null)
                    match.Context.PreContext = CopyContext(preStartIndex, preLength);
                    match.Context.PostContext = CopyContext(postStartIndex, postLength);
                _emitQueue.Add(match);
            /// Collects context from the specified ranges.
            /// The range must be within the bounds of the context buffer.
            /// <param name="startIndex">The index to start at.</param>
            /// <param name="length">The length of the range.</param>
            /// <returns>String representation of the collected context at the specified range.</returns>
            private string[] CopyContext(int startIndex, int length)
                string[] result = new string[length];
                for (int i = 0; i < length; i++)
                    result[i] = _collectedContext[startIndex + i].ToString();
        /// A class to track both logical and display contexts.
        private sealed class ContextTracker : IContextTracker
            private readonly IContextTracker _displayTracker;
            private readonly IContextTracker _logicalTracker;
            /// Initializes a new instance of the <see cref="ContextTracker"/> class.
            public ContextTracker(int preContext, int postContext)
                _displayTracker = new DisplayContextTracker(preContext, postContext);
                _logicalTracker = new LogicalContextTracker(preContext, postContext);
                EmitQueue = new List<MatchInfo>();
            public IList<MatchInfo> EmitQueue { get; }
                _displayTracker.TrackLine(line);
                _logicalTracker.TrackLine(line);
                _displayTracker.TrackMatch(match);
                _logicalTracker.TrackMatch(match);
                _displayTracker.TrackEOF();
                _logicalTracker.TrackEOF();
            /// Update the emit queue based on the wrapped trackers.
                // Look for completed matches in the logical
                // tracker's queue. Since the logical tracker
                // will try to collect as much context as
                // possible, the display tracker will have either
                // already finished collecting its context for the
                // match or will have completed it at the same
                // time as the logical tracker, so we can
                // be sure the matches will have both logical
                // and display context already populated.
                foreach (MatchInfo match in _logicalTracker.EmitQueue)
                    EmitQueue.Add(match);
                _logicalTracker.EmitQueue.Clear();
                _displayTracker.EmitQueue.Clear();
        /// ContextTracker that does not work for the case when pre- and post context is 0.
        private sealed class NoContextTracker : IContextTracker
            private readonly IList<MatchInfo> _matches = new List<MatchInfo>(1);
            IList<MatchInfo> IContextTracker.EmitQueue => _matches;
            void IContextTracker.TrackLine(string line)
            void IContextTracker.TrackMatch(MatchInfo match) => _matches.Add(match);
            void IContextTracker.TrackEOF()
        /// Gets or sets a culture name.
        [ValidateSet(typeof(ValidateMatchStringCultureNamesGenerator))]
        public string Culture
                switch (_stringComparison)
                    case StringComparison.Ordinal:
                    case StringComparison.OrdinalIgnoreCase:
                            return OrdinalCultureName;
                    case StringComparison.InvariantCulture:
                    case StringComparison.InvariantCultureIgnoreCase:
                            return InvariantCultureName;
                    case StringComparison.CurrentCulture:
                    case StringComparison.CurrentCultureIgnoreCase:
                            return CurrentCultureName;
                return _cultureName;
                _cultureName = value;
                InitCulture();
        internal const string OrdinalCultureName = "Ordinal";
        internal const string InvariantCultureName = "Invariant";
        internal const string CurrentCultureName = "Current";
        private string _cultureName = CultureInfo.CurrentCulture.Name;
        private StringComparison _stringComparison = StringComparison.CurrentCultureIgnoreCase;
        private CompareOptions _compareOptions = CompareOptions.IgnoreCase;
        private delegate int CultureInfoIndexOf(string source, string value, int startIndex, int count, CompareOptions options);
        private CultureInfoIndexOf _cultureInfoIndexOf = CultureInfo.CurrentCulture.CompareInfo.IndexOf;
        private void InitCulture()
            _stringComparison = default;
            switch (_cultureName)
                case OrdinalCultureName:
                        _stringComparison = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                        _compareOptions = CaseSensitive ? CompareOptions.Ordinal : CompareOptions.OrdinalIgnoreCase;
                        _cultureInfoIndexOf = CultureInfo.InvariantCulture.CompareInfo.IndexOf;
                case InvariantCultureName:
                        _stringComparison = CaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
                        _compareOptions = CaseSensitive ? CompareOptions.None : CompareOptions.IgnoreCase;
                case CurrentCultureName:
                        _stringComparison = CaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
                        _cultureInfoIndexOf = CultureInfo.CurrentCulture.CompareInfo.IndexOf;
                        var _cultureInfo = CultureInfo.GetCultureInfo(_cultureName);
                        _cultureInfoIndexOf = _cultureInfo.CompareInfo.IndexOf;
        /// Gets or sets the current pipeline object.
        [Parameter(ValueFromPipeline = true, Mandatory = true, ParameterSetName = ParameterSetObject)]
        [Parameter(ValueFromPipeline = true, Mandatory = true, ParameterSetName = ParameterSetObjectRaw)]
            get => _inputObject;
            set => _inputObject = LanguagePrimitives.IsNull(value) ? PSObject.AsPSObject(string.Empty) : value;
        private PSObject _inputObject = AutomationNull.Value;
        /// Gets or sets the patterns to find.
        public string[] Pattern { get; set; }
        private Regex[] _regexPattern;
        /// Gets or sets files to read from.
        /// Globbing is done on these.
        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = ParameterSetFile)]
        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = ParameterSetFileRaw)]
        [FileinfoToString]
        /// Gets or sets literal files to read from.
        /// Globbing is not done on these.
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = ParameterSetLiteralFile)]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = ParameterSetLiteralFileRaw)]
            get => Path;
        /// Gets or sets a value indicating if only string values containing matched lines should be returned.
        /// If not (default) return MatchInfo (or bool objects, when Quiet is passed).
        [Parameter(Mandatory = true, ParameterSetName = ParameterSetObjectRaw)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSetFileRaw)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSetLiteralFileRaw)]
        public SwitchParameter Raw { get; set; }
        /// Gets or sets a value indicating if a pattern string should be matched literally.
        /// If not (default) search using pattern as a Regular Expression.
        public SwitchParameter SimpleMatch { get; set; }
        /// Gets or sets a value indicating if the search is case sensitive.If true, then do case-sensitive searches.
        public SwitchParameter CaseSensitive { get; set; }
        /// Gets or sets a value indicating if the cmdlet will stop processing at the first successful match and
        /// return true.  If both List and Quiet parameters are given, an exception is thrown.
        [Parameter(ParameterSetName = ParameterSetObject)]
        [Parameter(ParameterSetName = ParameterSetFile)]
        [Parameter(ParameterSetName = ParameterSetLiteralFile)]
        /// Gets or sets a value indicating if matching files should be listed.
        /// This is the Unix functionality this switch is intended to mimic;
        /// the actual action of this option is to stop after the first match
        /// is found and returned from any particular file.
        /// Gets or sets a value indicating if highlighting should be disabled.
        public SwitchParameter NoEmphasis { get; set; }
        /// Gets or sets files to include. Files matching
        /// one of these (if specified) are included.
        /// <exception cref="WildcardPatternException">Invalid wildcard pattern was specified.</exception>
            get => _includeStrings;
                // null check is not needed (because of ValidateNotNullOrEmpty),
                // but we have to include it to silence OACR
                _includeStrings = value ?? throw PSTraceSource.NewArgumentNullException(nameof(value));
                _include = new WildcardPattern[_includeStrings.Length];
                for (int i = 0; i < _includeStrings.Length; i++)
                    _include[i] = WildcardPattern.Get(_includeStrings[i], WildcardOptions.IgnoreCase);
        private string[] _includeStrings;
        private WildcardPattern[] _include;
        /// Gets or sets files to exclude. Files matching
            get => _excludeStrings;
                _excludeStrings = value ?? throw PSTraceSource.NewArgumentNullException("value");
                _exclude = new WildcardPattern[_excludeStrings.Length];
                for (int i = 0; i < _excludeStrings.Length; i++)
                    _exclude[i] = WildcardPattern.Get(_excludeStrings[i], WildcardOptions.IgnoreCase);
        private string[] _excludeStrings;
        private WildcardPattern[] _exclude;
        /// Gets or sets a value indicating whether to only show lines which do not match.
        /// Equivalent to grep -v/findstr -v.
        public SwitchParameter NotMatch { get; set; }
        /// Gets or sets a value indicating whether the Matches property of MatchInfo should be set
        /// to the result of calling System.Text.RegularExpressions.Regex.Matches() on
        /// the corresponding line.
        /// Has no effect if -SimpleMatch is also specified.
        public SwitchParameter AllMatches { get; set; }
        /// Gets or sets the text encoding to process each file as.
        /// Gets or sets the number of context lines to collect. If set to a
        /// single integer value N, collects N lines each of pre-
        /// and post- context. If set to a 2-tuple B,A, collects B
        /// lines of pre- and A lines of post- context.
        /// If set to a list with more than 2 elements, the
        /// excess elements are ignored.
        [ValidateCount(1, 2)]
        public new int[] Context
            get => _context;
                _context = value ?? throw PSTraceSource.NewArgumentNullException("value");
                if (_context.Length == 1)
                    _preContext = _context[0];
                    _postContext = _context[0];
                else if (_context.Length >= 2)
                    _postContext = _context[1];
        private int[] _context;
        private int _preContext = 0;
        private int _postContext = 0;
        // When we are in Raw mode or pre- and postcontext are zero, use the _noContextTracker, since we will not be needing trackedLines.
        private IContextTracker GetContextTracker() => (Raw || (_preContext == 0 && _postContext == 0))
            ? _noContextTracker
            : new ContextTracker(_preContext, _postContext);
        // This context tracker is only used for strings which are piped
        // directly into the cmdlet. File processing doesn't need
        // to track state between calls to ProcessRecord, and so
        // allocates its own tracker. The reason we can't
        // use a single global tracker for both is that in the case of
        // a mixed list of strings and FileInfo, the context tracker
        // would get reset after each file.
        private IContextTracker _globalContextTracker;
        private IContextTracker _noContextTracker;
        /// This is used to handle the case were we're done processing input objects.
        /// If true, process record will just return.
        private bool _doneProcessing;
        private ulong _inputRecordNumber;
        /// Read command line parameters.
            if (this.MyInvocation.BoundParameters.ContainsKey(nameof(Culture)) && !this.MyInvocation.BoundParameters.ContainsKey(nameof(SimpleMatch)))
                InvalidOperationException exception = new(MatchStringStrings.CannotSpecifyCultureWithoutSimpleMatch);
                ErrorRecord errorRecord = new(exception, "CannotSpecifyCultureWithoutSimpleMatch", ErrorCategory.InvalidData, null);
            string suppressVt = Environment.GetEnvironmentVariable("__SuppressAnsiEscapeSequences");
            if (!string.IsNullOrEmpty(suppressVt))
                NoEmphasis = true;
            if (!SimpleMatch)
                RegexOptions regexOptions = CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                _regexPattern = new Regex[Pattern.Length];
                for (int i = 0; i < Pattern.Length; i++)
                        _regexPattern[i] = new Regex(Pattern[i], regexOptions);
                        this.ThrowTerminatingError(BuildErrorRecord(MatchStringStrings.InvalidRegex, Pattern[i], e.Message, "InvalidRegex", e));
            _noContextTracker = new NoContextTracker();
            _globalContextTracker = GetContextTracker();
        private readonly List<string> _inputObjectFileList = new(1) { string.Empty };
        /// Process the input.
        /// <exception cref="ArgumentException">Regular expression parsing error, path error.</exception>
        /// <exception cref="FileNotFoundException">A file cannot be found.</exception>
        /// <exception cref="DirectoryNotFoundException">A file cannot be found.</exception>
            if (_doneProcessing)
            // We may only have directories when we have resolved wildcards
            var expandedPathsMaybeDirectory = false;
            List<string> expandedPaths = null;
                expandedPaths = ResolveFilePaths(Path, _isLiteralPath);
                if (expandedPaths == null)
                expandedPathsMaybeDirectory = true;
                if (_inputObject.BaseObject is FileInfo fileInfo)
                    _inputObjectFileList[0] = fileInfo.FullName;
                    expandedPaths = _inputObjectFileList;
            if (expandedPaths != null)
                foreach (var filename in expandedPaths)
                    if (expandedPathsMaybeDirectory && Directory.Exists(filename))
                    var foundMatch = ProcessFile(filename);
                    if (Quiet && foundMatch)
                // No results in any files.
                if (Quiet)
                    var res = List ? null : Boxed.False;
                    WriteObject(res);
                // Set the line number in the matched object to be the record number
                _inputRecordNumber++;
                bool matched;
                MatchInfo result;
                MatchInfo matchInfo = null;
                if (_inputObject.BaseObject is string line)
                    matched = DoMatch(line, out result);
                    matchInfo = _inputObject.BaseObject as MatchInfo;
                    object objectToCheck = matchInfo ?? (object)_inputObject;
                    matched = DoMatch(objectToCheck, out result, out line);
                if (matched)
                    // Don't re-write the line number if it was already set...
                    if (matchInfo == null)
                        result.LineNumber = _inputRecordNumber;
                    // doMatch will have already set the pattern and line text...
                    _globalContextTracker.TrackMatch(result);
                    _globalContextTracker.TrackLine(line);
                // Emit any queued up objects...
                if (FlushTrackerQueue(_globalContextTracker))
                    // If we're in quiet mode, go ahead and stop processing
                    // now.
                        _doneProcessing = true;
        /// Process a file which was either specified on the
        /// command line or passed in as a FileInfo object.
        /// <param name="filename">The file to process.</param>
        /// <returns>True if a match was found; otherwise false.</returns>
        private bool ProcessFile(string filename)
            var contextTracker = GetContextTracker();
            // Read the file one line at a time...
                // see if the file is one the include exclude list...
                if (!MeetsIncludeExcludeCriteria(filename))
                using (FileStream fs = new(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (StreamReader sr = new(fs, Encoding))
                        string line;
                        ulong lineNo = 0;
                        // Read and display lines from the file until the end of
                        // the file is reached.
                        while ((line = sr.ReadLine()) != null)
                            lineNo++;
                            if (DoMatch(line, out MatchInfo result))
                                result.Path = filename;
                                result.LineNumber = lineNo;
                                contextTracker.TrackMatch(result);
                                contextTracker.TrackLine(line);
                            // Flush queue of matches to emit.
                            if (contextTracker.EmitQueue.Count > 0)
                                // If -list or -quiet was specified, we only want to emit the first match
                                // for each file so record the object to emit and stop processing
                                // this file. It's done this way so the file is closed before emitting
                                // the result so the downstream cmdlet can actually manipulate the file
                                // that was found.
                                if (Quiet || List)
                                FlushTrackerQueue(contextTracker);
                // Check for any remaining matches. This could be caused
                // by breaking out of the loop early for quiet or list
                // mode, or by reaching EOF before we collected all
                // our postcontext.
                contextTracker.TrackEOF();
                if (FlushTrackerQueue(contextTracker))
            catch (System.NotSupportedException nse)
                WriteError(BuildErrorRecord(MatchStringStrings.FileReadError, filename, nse.Message, "ProcessingFile", nse));
            catch (System.IO.IOException ioe)
                WriteError(BuildErrorRecord(MatchStringStrings.FileReadError, filename, ioe.Message, "ProcessingFile", ioe));
            catch (System.Security.SecurityException se)
                WriteError(BuildErrorRecord(MatchStringStrings.FileReadError, filename, se.Message, "ProcessingFile", se));
            catch (System.UnauthorizedAccessException uae)
                WriteError(BuildErrorRecord(MatchStringStrings.FileReadError, filename, uae.Message, "ProcessingFile", uae));
            return foundMatch;
        /// Emit any objects which have been queued up, and clear the queue.
        /// <param name="contextTracker">The context tracker to operate on.</param>
        /// <returns>Whether or not any objects were emitted.</returns>
        private bool FlushTrackerQueue(IContextTracker contextTracker)
            // Do we even have any matches to emit?
            if (contextTracker.EmitQueue.Count < 1)
            if (Raw)
                foreach (MatchInfo match in contextTracker.EmitQueue)
                    WriteObject(match.Line);
            else if (Quiet && !List)
                WriteObject(true);
            else if (List)
                WriteObject(contextTracker.EmitQueue[0]);
                    WriteObject(match);
            contextTracker.EmitQueue.Clear();
        /// Complete processing. Emits any objects which have been queued up
        /// due to -context tracking.
            // Check for a leftover match that was still tracking context.
            _globalContextTracker.TrackEOF();
            if (!_doneProcessing)
                FlushTrackerQueue(_globalContextTracker);
        private bool DoMatch(string operandString, out MatchInfo matchResult)
            return DoMatchWorker(operandString, null, out matchResult);
        private bool DoMatch(object operand, out MatchInfo matchResult, out string operandString)
            MatchInfo matchInfo = operand as MatchInfo;
            if (matchInfo != null)
                // We're operating in filter mode. Match
                // against the provided MatchInfo's line.
                // If the user has specified context tracking,
                // inform them that it is not allowed in filter
                // mode and disable it. Also, reset the global
                // context tracker used for processing pipeline
                // objects to use the new settings.
                operandString = matchInfo.Line;
                if (_preContext > 0 || _postContext > 0)
                    _preContext = 0;
                    _postContext = 0;
                    _globalContextTracker = new ContextTracker(_preContext, _postContext);
                    WarnFilterContext();
                operandString = (string)LanguagePrimitives.ConvertTo(operand, typeof(string), CultureInfo.InvariantCulture);
            return DoMatchWorker(operandString, matchInfo, out matchResult);
        /// Check the operand and see if it matches, if this.quiet is not set, then
        /// return a partially populated MatchInfo object with Line, Pattern, IgnoreCase set.
        /// <param name="operandString">The result of converting operand to a string.</param>
        /// <param name="matchInfo">The input object in filter mode.</param>
        /// <param name="matchResult">The match info object - this will be null if this.quiet is set.</param>
        /// <returns>True if the input object matched.</returns>
        private bool DoMatchWorker(string operandString, MatchInfo matchInfo, out MatchInfo matchResult)
            bool gotMatch = false;
            Match[] matches = null;
            int patternIndex = 0;
            matchResult = null;
            List<int> indexes = null;
            List<int> lengths = null;
            bool shouldEmphasize = !NoEmphasis && Host.UI.SupportsVirtualTerminal;
            // If Emphasize is set and VT is supported,
            // the lengths and starting indexes of regex matches
            // need to be passed in to the matchInfo object.
            if (shouldEmphasize)
                indexes = new List<int>();
                lengths = new List<int>();
                while (patternIndex < Pattern.Length)
                    Regex r = _regexPattern[patternIndex];
                    // Only honor allMatches if notMatch is not set,
                    // since it's a fairly expensive operation and
                    // notMatch takes precedent over allMatch.
                    if (AllMatches && !NotMatch)
                        MatchCollection mc = r.Matches(operandString);
                        if (mc.Count > 0)
                            matches = new Match[mc.Count];
                            ((ICollection)mc).CopyTo(matches, 0);
                                    indexes.Add(match.Index);
                                    lengths.Add(match.Length);
                            gotMatch = true;
                        Match match = r.Match(operandString);
                        gotMatch = match.Success;
                        if (match.Success)
                            matches = new Match[] { match };
                    if (gotMatch)
                    patternIndex++;
                    string pat = Pattern[patternIndex];
                    int index = _cultureInfoIndexOf(operandString, pat, 0, operandString.Length, _compareOptions);
                    if (index >= 0)
                            indexes.Add(index);
                            lengths.Add(pat.Length);
            if (NotMatch)
                gotMatch = !gotMatch;
                // If notMatch was specified with multiple
                // patterns, then *none* of the patterns
                // matched and any pattern could be picked
                // to report in MatchInfo. However, that also
                // means that patternIndex will have been
                // incremented past the end of the pattern array.
                // So reset it to select the first pattern.
                patternIndex = 0;
                // if we were passed a MatchInfo object as the operand,
                // we're operating in filter mode.
                    // If the original MatchInfo was tracking context,
                    // we need to copy it and disable display context,
                    // since we can't guarantee it will be displayed
                    // correctly when filtered.
                    if (matchInfo.Context != null)
                        matchResult = matchInfo.Clone();
                        matchResult.Context.DisplayPreContext = Array.Empty<string>();
                        matchResult.Context.DisplayPostContext = Array.Empty<string>();
                        // Otherwise, just pass the object as is.
                        matchResult = matchInfo;
                // otherwise construct and populate a new MatchInfo object
                matchResult = shouldEmphasize
                    ? new MatchInfo(indexes, lengths)
                    : new MatchInfo();
                matchResult.IgnoreCase = !CaseSensitive;
                matchResult.Line = operandString;
                matchResult.Pattern = Pattern[patternIndex];
                    matchResult.Context = new MatchInfoContext();
                // Matches should be an empty list, rather than null,
                // in the cases of notMatch and simpleMatch.
                matchResult.Matches = matches ?? Array.Empty<Match>();
        /// Get a list or resolved file paths.
        /// <param name="filePaths">The filePaths to resolve.</param>
        /// <param name="isLiteralPath">True if the wildcard resolution should not be attempted.</param>
        /// <returns>The resolved (absolute) paths.</returns>
        private List<string> ResolveFilePaths(string[] filePaths, bool isLiteralPath)
            List<string> allPaths = new();
            foreach (string path in filePaths)
                Collection<string> resolvedPaths;
                ProviderInfo provider;
                    resolvedPaths = new Collection<string>();
                    string resolvedPath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(path, out provider, out _);
                    resolvedPaths.Add(resolvedPath);
                    resolvedPaths = GetResolvedProviderPathFromPSPath(path, out provider);
                if (!provider.NameEquals(base.Context.ProviderNames.FileSystem))
                    // "The current provider ({0}) cannot open a file"
                    WriteError(BuildErrorRecord(MatchStringStrings.FileOpenError, provider.FullName, "ProcessingFile", null));
                allPaths.AddRange(resolvedPaths);
            return allPaths;
        private static ErrorRecord BuildErrorRecord(string messageId, string argument, string errorId, Exception innerException)
            return BuildErrorRecord(messageId, new object[] { argument }, errorId, innerException);
        private static ErrorRecord BuildErrorRecord(string messageId, string arg0, string arg1, string errorId, Exception innerException)
            return BuildErrorRecord(messageId, new object[] { arg0, arg1 }, errorId, innerException);
        private static ErrorRecord BuildErrorRecord(string messageId, object[] arguments, string errorId, Exception innerException)
            string fmtedMsg = StringUtil.Format(messageId, arguments);
            ArgumentException e = new(fmtedMsg, innerException);
        private void WarnFilterContext()
            string msg = MatchStringStrings.FilterContextWarning;
            WriteWarning(msg);
        /// Magic class that works around the limitations on ToString() for FileInfo.
        private sealed class FileinfoToStringAttribute : ArgumentTransformationAttribute
                object result = inputData;
                if (result is PSObject mso)
                    result = mso.BaseObject;
                FileInfo fileInfo;
                // Handle an array of elements...
                if (result is IList argList)
                    object[] resultList = new object[argList.Count];
                    for (int i = 0; i < argList.Count; i++)
                        object element = argList[i];
                        mso = element as PSObject;
                        if (mso != null)
                            element = mso.BaseObject;
                        fileInfo = element as FileInfo;
                        resultList[i] = fileInfo?.FullName ?? element;
                    return resultList;
                // Handle the singleton case...
                fileInfo = result as FileInfo;
                if (fileInfo != null)
                    return fileInfo.FullName;
        /// Check whether the supplied name meets the include/exclude criteria.
        /// That is - it's on the include list if there is one and not on
        /// the exclude list if there was one of those.
        /// <param name="filename">The filename to test.</param>
        /// <returns>True if the filename is acceptable.</returns>
        private bool MeetsIncludeExcludeCriteria(string filename)
            // see if the file is on the include list...
            if (_include != null)
                foreach (WildcardPattern patternItem in _include)
                    if (patternItem.IsMatch(filename))
            if (!ok)
            // now see if it's on the exclude list...
            if (_exclude != null)
                foreach (WildcardPattern patternItem in _exclude)
                        ok = false;
    public class ValidateMatchStringCultureNamesGenerator : IValidateSetValuesGenerator
            var result = new List<string>(cultures.Length + 3);
            result.Add(SelectStringCommand.OrdinalCultureName);
            result.Add(SelectStringCommand.InvariantCultureName);
            result.Add(SelectStringCommand.CurrentCultureName);

// interfaces for host interaction
    /// Base class providing support for string manipulation.
    /// This class is a tear off class provided by the LineOutput class.
    internal class DisplayCells
        /// Calculate the buffer cell length of the given string.
        /// <param name="str">String that may contain VT escape sequences.</param>
        /// <returns>Number of buffer cells the string needs to take.</returns>
        internal int Length(string str)
            return Length(str, 0);
        /// <param name="offset">
        /// When the string doesn't contain VT sequences, it's the starting index.
        /// When the string contains VT sequences, it means starting from the 'n-th' char that doesn't belong to a escape sequence.</param>
        internal virtual int Length(string str, int offset)
            var valueStrDec = new ValueStringDecorated(str);
                str = valueStrDec.ToString(OutputRendering.PlainText);
            for (; offset < str.Length; offset++)
                length += CharLengthInBufferCells(str[offset]);
        /// Calculate the buffer cell length of the given character.
        /// <param name="character"></param>
        /// <returns>Number of buffer cells the character needs to take.</returns>
        internal virtual int Length(char character)
            return CharLengthInBufferCells(character);
        /// Truncate from the tail of the string.
        /// <param name="displayCells">Number of buffer cells to fit in.</param>
        /// <returns>Number of non-escape-sequence characters from head of the string that can fit in the space.</returns>
        internal int TruncateTail(string str, int displayCells)
            return TruncateTail(str, offset: 0, displayCells);
        internal int TruncateTail(string str, int offset, int displayCells)
            return GetFitLength(str, offset, displayCells, startFromHead: true);
        /// Truncate from the head of the string.
        /// <returns>Number of non-escape-sequence characters from head of the string that should be skipped.</returns>
        internal int TruncateHead(string str, int displayCells)
            int tailCount = GetFitLength(str, offset: 0, displayCells, startFromHead: false);
            return str.Length - tailCount;
        protected static int CharLengthInBufferCells(char c)
        /// Given a string and a number of display cells, it computes how many
        /// characters would fit starting from the beginning or end of the string.
        /// <param name="str">String to be displayed, which doesn't contain any VT sequences.</param>
        /// <param name="offset">Offset inside the string.</param>
        /// <param name="displayCells">Number of display cells.</param>
        /// <param name="startFromHead">If true compute from the head (i.e. k++) else from the tail (i.e. k--).</param>
        /// <returns>Number of characters that would fit.</returns>
        protected int GetFitLength(string str, int offset, int displayCells, bool startFromHead)
            int filledDisplayCellsCount = 0; // number of cells that are filled in
            int charactersAdded = 0; // number of characters that fit
            int currCharDisplayLen; // scratch variable
            int k = startFromHead ? offset : str.Length - 1;
            int kFinal = startFromHead ? str.Length - 1 : offset;
                if ((startFromHead && k > kFinal) || (!startFromHead && k < kFinal))
                // compute the cell number for the current character
                currCharDisplayLen = this.Length(str[k]);
                if (filledDisplayCellsCount + currCharDisplayLen > displayCells)
                    // if we added this character it would not fit, we cannot continue
                // keep adding, we fit
                filledDisplayCellsCount += currCharDisplayLen;
                charactersAdded++;
                // check if we fit exactly
                if (filledDisplayCellsCount == displayCells)
                    // exact fit, we cannot add more
                k = startFromHead ? (k + 1) : (k - 1);
            return charactersAdded;
    /// Base class providing information about the screen device capabilities
    /// and used to write the output strings to the text output device.
    /// Each device supported will have to derive from it.
    /// Examples of supported devices are:
    /// *   Screen Layout: it layers on top of Console and RawConsole
    /// *   File: it layers on top of a TextWriter
    /// *   In Memory text stream: it layers on top of an in memory buffer
    /// *   Printer: it layers on top of a memory buffer then sent to a printer device
    /// Assumptions:
    /// - Fixed pitch font: layout done in terms of character cells
    /// - character cell layout not affected by bold, reverse screen, color, etc.
    /// - returned values might change from call to call if the specific underlying
    ///   implementation allows window resizing.
    internal abstract class LineOutput
        /// Whether the device requires full buffering of formatting
        /// objects before any processing.
        internal virtual bool RequiresBuffering { get { return false; } }
        /// Delegate the implementor of ExecuteBufferPlayBack should
        /// call to cause the playback to happen when ready to execute.
        internal delegate void DoPlayBackCall();
        /// If RequiresBuffering = true, this call will be made to
        /// start the playback.
        internal virtual void ExecuteBufferPlayBack(DoPlayBackCall playback) { }
        /// The number of columns the current device has.
        internal abstract int ColumnNumber { get; }
        /// The number of rows the current device has.
        internal abstract int RowNumber { get; }
        /// <param name="s">
        ///     string to be written to the device
        internal abstract void WriteLine(string s);
        /// Write a line of string as raw text to the output device, with no change to the string.
        internal virtual void WriteRawText(string s) => WriteLine(s);
        internal WriteStreamType WriteStream
        /// Handle the stop processing signal.
        /// Set a flag that will be checked during operations.
        internal void StopProcessing()
        private bool _isStopping;
        internal void CheckStopProcessing()
            if (!_isStopping)
        /// Return an instance of the display helper tear off.
        internal virtual DisplayCells DisplayCells
                // just return the default singleton implementation
                return _displayCellsDefault;
        /// Singleton used for the default implementation.
        /// NOTE: derived classes may chose to provide a different
        /// implementation by overriding.
        protected static DisplayCells _displayCellsDefault = new DisplayCells();
    /// Helper class to provide line breaking (based on device width)
    /// and embedded newline processing
    /// It needs to be provided with two callbacks for line processing.
    internal class WriteLineHelper
        #region callbacks
        /// Delegate definition.
        internal delegate void WriteCallback(string s);
        /// Instance of the delegate previously defined
        /// for line that has EXACTLY this.ncols characters.
        private readonly WriteCallback _writeCall = null;
        /// for generic line, less that this.ncols characters.
        private readonly WriteCallback _writeLineCall = null;
        private readonly bool _lineWrap;
        /// Construct an instance, given the two callbacks
        /// NOTE: if the underlying device treats the two cases as the
        /// same, the same delegate can be passed twice.
        /// <param name="lineWrap">True if we require line wrapping.</param>
        /// <param name="wlc">Delegate for WriteLine(), must ben non null.</param>
        /// <param name="wc">Delegate for Write(), if null, use the first parameter.</param>
        /// <param name="displayCells">Helper object for manipulating strings.</param>
        internal WriteLineHelper(bool lineWrap, WriteCallback wlc, WriteCallback wc, DisplayCells displayCells)
            if (wlc == null)
                throw PSTraceSource.NewArgumentNullException(nameof(wlc));
            if (displayCells == null)
                throw PSTraceSource.NewArgumentNullException(nameof(displayCells));
            _displayCells = displayCells;
            _writeLineCall = wlc;
            _writeCall = wc ?? wlc;
            _lineWrap = lineWrap;
        /// Main entry point to process a line.
        /// <param name="s">String to process.</param>
        /// <param name="cols">Width of the device.</param>
        internal void WriteLine(string s, int cols)
            WriteLineInternal(s, cols);
        /// Internal helper, needed because it might make recursive calls to itself.
        /// <param name="val">String to process.</param>
        private void WriteLineInternal(string val, int cols)
                _writeLineCall(val);
            // If the output is being redirected, then we don't break val
            if (!_lineWrap)
                _writeCall(val);
            // check for line breaks
            List<string> lines = StringManipulationHelper.SplitLines(val);
            // process the substrings as separate lines
                // compute the display length of the string
                int displayLength = _displayCells.Length(lines[k]);
                if (displayLength < cols)
                    // NOTE: this is the case where where System.Console.WriteLine() would work just fine
                    _writeLineCall(lines[k]);
                if (displayLength == cols)
                    // NOTE: this is the corner case where System.Console.WriteLine() cannot be called
                    _writeCall(lines[k]);
                string s = lines[k];
                    // the string is still too long to fit, write the first cols characters
                    // and go back for more wraparound
                    int headCount = _displayCells.TruncateTail(s, cols);
                    WriteLineInternal(s.VtSubstring(0, headCount), cols);
                    // chop off the first fieldWidth characters, already printed
                    s = s.VtSubstring(headCount);
                    if (_displayCells.Length(s) <= cols)
                        // if we fit, print the tail of the string and we are done
        private readonly DisplayCells _displayCells;
    /// Implementation of the ILineOutput interface accepting an instance of a
    /// TextWriter abstract class.
    internal sealed class TextWriterLineOutput : LineOutput
        #region ILineOutput methods
        /// Get the columns on the screen
        /// for files, it is settable at creation time.
                return _columns;
        /// Get the # of rows on the screen: for files
        /// we return -1, meaning infinite.
        /// Write a line by delegating to the writer underneath.
            WriteRawText(PSHostUserInterface.GetOutputString(s, isHost: false));
            if (_suppressNewline)
                _writer.Write(s);
                _writer.WriteLine(s);
        /// Initialization of the object. It must be called before
        /// attempting any operation.
        /// <param name="writer">TextWriter to write to.</param>
        /// <param name="columns">Max columns widths for the text.</param>
        internal TextWriterLineOutput(TextWriter writer, int columns)
            _columns = columns;
        /// <param name="suppressNewline">False to add a newline to the end of the output string, true if not.</param>
        internal TextWriterLineOutput(TextWriter writer, int columns, bool suppressNewline)
            : this(writer, columns)
            _suppressNewline = suppressNewline;
        private readonly int _columns = 0;
        private readonly TextWriter _writer = null;
        private readonly bool _suppressNewline = false;
    /// TextWriter to generate data for the Monad pipeline in a streaming fashion:
    /// the provided callback will be called each time a line is written.
    internal class StreamingTextWriter : TextWriter
        [TraceSource("StreamingTextWriter", "StreamingTextWriter")]
        private static readonly PSTraceSource s_tracer = PSTraceSource.GetTracer("StreamingTextWriter", "StreamingTextWriter");
        /// Create an instance by passing a delegate.
        /// <param name="writeCall">Delegate to write to.</param>
        /// <param name="culture">Culture for this TextWriter.</param>
        internal StreamingTextWriter(WriteLineCallback writeCall, CultureInfo culture)
            : base(culture)
            if (writeCall == null)
                throw PSTraceSource.NewArgumentNullException(nameof(writeCall));
            _writeCall = writeCall;
        #region TextWriter overrides
        public override Encoding Encoding { get { return new UnicodeEncoding(); } }
        public override void WriteLine(string s)
            _writeCall(s);
        internal delegate void WriteLineCallback(string s);
        /// Instance of the delegate previously defined.
        private readonly WriteLineCallback _writeCall = null;

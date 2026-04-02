    /// ConsoleHostUserInterface implements console-mode user interface for powershell.
    internal partial class ConsoleHostUserInterface : System.Management.Automation.Host.PSHostUserInterface
        /// This is the char that is echoed to the console when the input is masked. This not localizable.
        private const char PrintToken = '*';
        /// Command completion implementation object.
        private PowerShell _commandCompletionPowerShell;
        /// This is a test hook for programmatically reading and writing ConsoleHost I/O.
        private static readonly PSHostUserInterface s_h = null;
        /// Return true if the console supports a VT100 like virtual terminal.
        public override bool SupportsVirtualTerminal { get; }
        /// Constructs an instance.
        /// <param name="parent"></param>
        internal ConsoleHostUserInterface(ConsoleHost parent)
            Dbg.Assert(parent != null, "parent may not be null");
            _rawui = new ConsoleHostRawUserInterface(this);
            SupportsVirtualTerminal = true;
            _isInteractiveTestToolListening = false;
            // check if TERM env var is set
            // `dumb` means explicitly don't use VT
            // `xterm-mono` and `xtermm` means support VT, but emit plaintext
            switch (Environment.GetEnvironmentVariable("TERM"))
                case "dumb":
                    SupportsVirtualTerminal = false;
                case "xterm-mono":
                case "xtermm":
                    PSStyle.Instance.OutputRendering = OutputRendering.PlainText;
            // widely supported by CLI tools via https://no-color.org/
            if (Environment.GetEnvironmentVariable("NO_COLOR") != null)
            if (SupportsVirtualTerminal)
                SupportsVirtualTerminal = TryTurnOnVirtualTerminal();
        internal bool TryTurnOnVirtualTerminal()
                // Turn on virtual terminal if possible.
                // This might throw - not sure how exactly (no console), but if it does, we shouldn't fail to start.
                var outputMode = ConsoleControl.GetMode(outputHandle);
                if (outputMode.HasFlag(ConsoleControl.ConsoleModes.VirtualTerminal))
                outputMode |= ConsoleControl.ConsoleModes.VirtualTerminal;
                if (ConsoleControl.NativeMethods.SetConsoleMode(outputHandle.DangerousGetHandle(), (uint)outputMode))
                    // We only know if vt100 is supported if the previous call actually set the new flag, older
                    // systems ignore the setting.
                    outputMode = ConsoleControl.GetMode(outputHandle);
                    return outputMode.HasFlag(ConsoleControl.ConsoleModes.VirtualTerminal);
                // Do nothing if failed
        /// Supplies an implementation of PSHostRawUserInterface that provides low-level console mode UI facilities.
        public override PSHostRawUserInterface RawUI
                Dbg.Assert(_rawui != null, "rawui should have been created by ctor");
                // no locking because this is read-only, and allocated in the ctor.
                return _rawui;
        // deadcode; but could be needed in the future.
        ///// <summary>
        ///// gets the PSHost instance that uses this ConsoleHostUserInterface instance
        ///// </summary>
        ///// <value></value>
        ///// <exception/>
        // internal
        // PSHost
        // Parent
        //    get
        //    {
        //        using (tracer.TraceProperty())
        //        {
        //            // no locking because this is read-only and set in the ctor.
        //            return parent;
        //        }
        //    }
        /// True if command completion is currently running.
        internal bool IsCommandCompletionRunning
                return _commandCompletionPowerShell != null &&
                       _commandCompletionPowerShell.InvocationStateInfo.State == PSInvocationState.Running;
        /// True if the Read* functions should read from the stdin stream instead of from the win32 console.
        internal bool ReadFromStdin { get; set; }
        /// True if the host shouldn't write out prompts.
        internal bool NoPrompt { get; set; }
        #region Line-oriented interaction
        ///    Win32's ReadConsole fails
            HandleThrowOnReadAndPrompt();
            // call our internal version such that it does not end input on a tab
            return ReadLine(false, string.Empty, out _, true, true);
        ///    Win32's ReadConsole failed
        /// If Ctrl-C is entered by user
            // we lock here so that multiple threads won't interleave the various reads and writes here.
            lock (_instanceLock)
                result = ReadLineSafe(true, PrintToken);
            SecureString secureResult = result as SecureString;
            System.Management.Automation.Diagnostics.Assert(secureResult != null, "ReadLineSafe did not return a SecureString");
            return secureResult;
        /// Implementation based on NT CredUI's GetPasswdStr.
        /// Use Win32.ReadConsole to construct a SecureString. The advantage of ReadConsole over ReadKey is
        /// Alt-ddd where d is {0-9} is allowed.
        /// It also manages the cursor as keys are entered and "backspaced". However, it is possible that
        /// while this method is running, the console buffer contents could change. Then, its cursor mgmt
        /// will likely be messed up.
        /// Secondary implementation for Unix based on Console.ReadKey(), where
        /// the advantage is portability through abstraction. Does not support
        /// arrow key movement, but supports backspace.
        /// <param name="isSecureString">
        /// True to specify reading a SecureString; false reading a string
        /// <param name="printToken">
        /// string for output echo
        private object ReadLineSafe(bool isSecureString, char? printToken)
            // Don't lock (instanceLock) in here -- the caller needs to do that...
            PreRead();
            string printTokenString = printToken.HasValue ?
                printToken.ToString() :
                null;
            SecureString secureResult = new SecureString();
            StringBuilder result = new StringBuilder();
            bool treatControlCAsInput = Console.TreatControlCAsInput;
            bool isModeChanged = true; // assume ConsoleMode is changed so that if ReadLineSetMode
            // fails to return the value correctly, the original mode is
            // restored.
                Console.TreatControlCAsInput = true;
                // Ensure that we're in the proper line-input mode.
                const ConsoleControl.ConsoleModes DesiredMode =
                    ConsoleControl.ConsoleModes.Extended |
                    ConsoleControl.ConsoleModes.QuickEdit;
                ConsoleControl.ConsoleModes m = originalMode;
                bool shouldUnsetEchoInput = shouldUnsetMode(ConsoleControl.ConsoleModes.EchoInput, ref m);
                bool shouldUnsetLineInput = shouldUnsetMode(ConsoleControl.ConsoleModes.LineInput, ref m);
                bool shouldUnsetMouseInput = shouldUnsetMode(ConsoleControl.ConsoleModes.MouseInput, ref m);
                bool shouldUnsetProcessInput = shouldUnsetMode(ConsoleControl.ConsoleModes.ProcessedInput, ref m);
                if ((m & DesiredMode) != DesiredMode ||
                    shouldUnsetMouseInput ||
                    shouldUnsetEchoInput ||
                    shouldUnsetLineInput ||
                    shouldUnsetProcessInput)
                    m |= DesiredMode;
                    ConsoleControl.SetMode(handle, m);
                    isModeChanged = false;
                _rawui.ClearKeyCache();
                Coordinates originalCursorPos = _rawui.CursorPosition;
                // read one char at a time so that we don't
                // end up having a immutable string holding the
                // secret in memory.
                const int CharactersToRead = 1;
                Span<char> inputBuffer = stackalloc char[CharactersToRead + 1];
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                    string key = ConsoleControl.ReadConsole(handle, initialContentLength: 0, inputBuffer, charactersToRead: CharactersToRead, endOnTab: false, out _);
                    // Handle Ctrl-C ending input
                    if (keyInfo.Key == ConsoleKey.C && keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
                    if (string.IsNullOrEmpty(key) || key[0] == (char)3)
                    if (keyInfo.Key == ConsoleKey.Enter)
                    if (key[0] == (char)13)
                        // we are done if user presses ENTER key
                    if (keyInfo.Key == ConsoleKey.Backspace)
                    if (key[0] == (char)8)
                        // for backspace, remove last char appended
                        if (isSecureString && secureResult.Length > 0)
                            secureResult.RemoveAt(secureResult.Length - 1);
                            WriteBackSpace(originalCursorPos);
                        else if (result.Length > 0)
                            result.Remove(result.Length - 1, 1);
                    else if (char.IsControl(keyInfo.KeyChar))
                        // deny list control characters
                        // append the char to our string
                        if (isSecureString)
                            secureResult.AppendChar(keyInfo.KeyChar);
                            secureResult.AppendChar(key[0]);
                            result.Append(keyInfo.KeyChar);
                            result.Append(key);
                        if (!string.IsNullOrEmpty(printTokenString))
                            WritePrintToken(printTokenString, ref originalCursorPos);
                // ReadKey() failed so we stop
                Console.TreatControlCAsInput = treatControlCAsInput;
                if (isModeChanged)
            WriteLineToConsole();
            PostRead(result.ToString());
        /// Handle writing print token with proper cursor adjustment for ReadLineSafe.
        /// token output for each char input. It must be a one-char string
        /// <param name="originalCursorPosition">
        /// it is the cursor position where ReadLineSafe begins
        private void WritePrintToken(
            string printToken,
            ref Coordinates originalCursorPosition)
            Dbg.Assert(!string.IsNullOrEmpty(printToken),
                "Calling WritePrintToken with printToken being null or empty");
            Dbg.Assert(printToken.Length == 1,
                "Calling WritePrintToken with printToken's Length being " + printToken.Length);
            Size consoleBufferSize = _rawui.BufferSize;
            Coordinates currentCursorPosition = _rawui.CursorPosition;
            // if the cursor is currently at the lower right corner, this write will cause the screen buffer to
            // scroll up. So, it is necessary to adjust the original cursor position one row up.
            if (currentCursorPosition.Y >= consoleBufferSize.Height - 1 && // last row
                currentCursorPosition.X >= consoleBufferSize.Width - 1)  // last column
                if (originalCursorPosition.Y > 0)
                    originalCursorPosition.Y--;
            WriteToConsole(printToken, false);
        /// Handle backspace with proper cursor adjustment for ReadLineSafe.
        private void WriteBackSpace(Coordinates originalCursorPosition)
            Coordinates cursorPosition = _rawui.CursorPosition;
            if (cursorPosition == originalCursorPosition)
                // at originalCursorPosition, don't move
            if (cursorPosition.X == 0)
                if (cursorPosition.Y <= originalCursorPosition.Y)
                // BufferSize.Width is 1 larger than cursor position
                cursorPosition.X = _rawui.BufferSize.Width - 1;
                cursorPosition.Y--;
                BlankAtCursor(cursorPosition);
            else if (cursorPosition.X > 0)
                cursorPosition.X--;
            // do nothing if cursorPosition.X is left of screen
        /// Blank out at and move rawui.CursorPosition to <paramref name="cursorPosition"/>
        /// <param name="cursorPosition">Position to blank out.</param>
        private void BlankAtCursor(Coordinates cursorPosition)
            _rawui.CursorPosition = cursorPosition;
            WriteToConsole(" ", true);
        /// If <paramref name="m"/> is set on <paramref name="flagToUnset"/>, unset it and return true;
        /// otherwise return false.
        /// <param name="flagToUnset">
        /// a flag in ConsoleControl.ConsoleModes to be unset in <paramref name="m"/>
        /// <param name="m">
        /// true if <paramref name="m"/> is set on <paramref name="flagToUnset"/>
        /// false otherwise
        private static bool shouldUnsetMode(
            ConsoleControl.ConsoleModes flagToUnset,
            ref ConsoleControl.ConsoleModes m)
            if ((m & flagToUnset) > 0)
                m &= ~flagToUnset;
        #region WriteToConsole
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void WriteToConsole(char c, bool transcribeResult)
            ReadOnlySpan<char> value = [c];
            WriteToConsole(value, transcribeResult);
        internal void WriteToConsole(ReadOnlySpan<char> value, bool transcribeResult)
            WriteToConsole(value, transcribeResult, newLine: false);
        private void WriteToConsole(ReadOnlySpan<char> value, bool transcribeResult, bool newLine)
            // Ensure that we're in the proper line-output mode.  We don't lock here as it does not matter if we
            // attempt to set the mode from multiple threads at once.
            ConsoleControl.ConsoleModes m = ConsoleControl.GetMode(handle);
                ConsoleControl.ConsoleModes.ProcessedOutput
                | ConsoleControl.ConsoleModes.WrapEndOfLine;
            if ((m & DesiredMode) != DesiredMode)
            PreWrite();
            // This is atomic, so we don't lock here...
            ConsoleControl.WriteConsole(handle, value, newLine);
            ConsoleOutWriteHelper(value, newLine);
            if (_isInteractiveTestToolListening && Console.IsOutputRedirected)
            if (transcribeResult)
                PostWrite(value, newLine);
                PostWrite();
        private void WriteToConsole(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string text, bool newLine = false)
            // Sync access so that we don't conflict on color settings if called from multiple threads.
                ConsoleColor fg = RawUI.ForegroundColor;
                ConsoleColor bg = RawUI.BackgroundColor;
                RawUI.ForegroundColor = foregroundColor;
                RawUI.BackgroundColor = backgroundColor;
                    WriteToConsole(text, transcribeResult: true, newLine);
                    RawUI.ForegroundColor = fg;
                    RawUI.BackgroundColor = bg;
        private static void ConsoleOutWriteHelper(ReadOnlySpan<char> value, bool newLine)
                Console.Out.WriteLine(value);
                Console.Out.Write(value);
        internal void WriteLineToConsole(ReadOnlySpan<char> value, bool transcribeResult)
            WriteToConsole(value, transcribeResult, newLine: true);
        private void WriteLineToConsole(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string text)
            WriteToConsole(foregroundColor, backgroundColor, text, newLine: true);
        private void WriteLineToConsole(string text)
            WriteLineToConsole(text, transcribeResult: true);
        private void WriteLineToConsole()
            WriteToConsole(Environment.NewLine, transcribeResult: true, newLine: false);
        #endregion WriteToConsole
        ///    Win32's GetConsoleMode fails
        ///    Win32's SetConsoleMode fails
        ///    Win32's WriteConsole fails
                WriteImpl(value, newLine: false);
        // The WriteImpl() method should always be called within a lock on _instanceLock
        // to ensure thread safety and prevent issues in multi-threaded scenarios.
        private void WriteImpl(string value, bool newLine)
            if (string.IsNullOrEmpty(value) && !newLine)
            // If the test hook is set, write to it and continue.
            if (s_h != null)
                    s_h.WriteLine(value);
                    s_h.Write(value);
            TextWriter writer = Console.IsOutputRedirected ? Console.Out : _parent.ConsoleTextWriter;
            value = GetOutputString(value, SupportsVirtualTerminal);
            if (_parent.IsRunningAsync)
                Dbg.Assert(writer == _parent.OutputSerializer.textWriter, "writers should be the same");
                _parent.OutputSerializer.Serialize(value);
                    _parent.OutputSerializer.Serialize(Environment.NewLine);
                    writer.WriteLine(value);
                    writer.Write(value);
        ///    Win32's CreateFile fails
            Write(foregroundColor, backgroundColor, value, newLine: false);
        public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
            Write(foregroundColor, backgroundColor, value, newLine: true);
        private void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value, bool newLine)
                    this.WriteImpl(value, newLine);
                this.WriteImpl(value, newLine: true);
        public override void WriteLine()
                this.WriteImpl(Environment.NewLine, newLine: false);
        #region Word Wrapping
        /// This is a poor-man's word-wrapping routine.  It breaks a single string into segments small enough to fit within a
        /// given number of cells.  A break is determined by the last occurrence of whitespace that allows all prior characters
        /// on a line to be written within a given number of cells.  If there is no whitespace found within that span, then the
        /// largest span that will fit in the bounds is used.
        /// The problem is complicated by the fact that a single character may consume more than one cell.  Conceptually, this
        /// is the same case as placing an upper bound on the length of a line while also having a strlen function that
        /// arbitrarily considers the length of any single character to be 1 or greater.
        /// <param name="text">
        /// Text to be emitted.
        /// Each tab character in the text is replaced with a space in the results.
        /// <param name="maxWidthInBufferCells">
        /// Max width, in buffer cells, of a single line.  Note that a single character may consume more than one cell.  The
        /// number of cells consumed is determined by calling ConsoleHostRawUserInterface.LengthInBufferCells.
        /// A list of strings representing the text broken into "lines" each of which are guaranteed not to exceed
        /// maxWidthInBufferCells.
        internal List<string> WrapText(string text, int maxWidthInBufferCells)
            List<string> result = new List<string>();
            List<Word> words = ChopTextIntoWords(text, maxWidthInBufferCells);
            if (words.Count < 1)
            IEnumerator<Word> e = words.GetEnumerator();
            bool valid = false;
            int cellCounter = 0;
            StringBuilder line = new StringBuilder();
            string l = null;
                valid = e.MoveNext();
                if (!valid)
                    if (line.Length > 0)
                        l = line.ToString();
                        Dbg.Assert(RawUI.LengthInBufferCells(l) <= maxWidthInBufferCells, "line is too long");
                        result.Add(l);
                if ((e.Current.Flags & WordFlags.IsNewline) > 0)
                    // skip the newline "words"
                    line = new StringBuilder();
                    cellCounter = 0;
                // will the word fit?
                if (cellCounter + e.Current.CellCount <= maxWidthInBufferCells)
                    // yes, add it to the line.
                    line.Append(e.Current.Text);
                    cellCounter += e.Current.CellCount;
                    // no: too long.  Either start a new line, or pick off as much whitespace as we need.
                    if ((e.Current.Flags & WordFlags.IsWhitespace) == 0)
                        line = new StringBuilder(e.Current.Text);
                        cellCounter = e.Current.CellCount;
                    // chop the whitespace into bits.
                    int w = maxWidthInBufferCells - cellCounter;
                    Dbg.Assert(w < e.Current.CellCount, "width remaining should be less than size of word");
                    line.Append(e.Current.Text.AsSpan(0, w));
                    Dbg.Assert(RawUI.LengthInBufferCells(l) == maxWidthInBufferCells, "line should exactly fit");
                    string remaining = e.Current.Text.Substring(w);
                    line = new StringBuilder(remaining);
                    cellCounter = RawUI.LengthInBufferCells(remaining);
            } while (valid);
        /// Struct used by WrapText.
        internal enum WordFlags
            IsWhitespace = 0x01,
            IsNewline = 0x02
        internal struct Word
            internal int CellCount;
            internal string Text;
            internal WordFlags Flags;
        /// Chops text into "words," where a word is defined to be a sequence of whitespace characters, or a sequence of
        /// non-whitespace characters, each sequence being no longer than a given maximum.  Therefore, in the text "this is a
        /// string" there are 7 words: 4 sequences of non-whitespace characters and 3 sequences of whitespace characters.
        /// Whitespace is considered to be spaces or tabs.  Each tab character is replaced with a single space.
        /// The text to be chopped up.
        /// The maximum number of buffer cells that each word may consume.
        /// A list of words, in the same order they appear in the source text.
        /// This can be made faster by, instead of creating little strings for each word, creating indices of the start and end
        /// range of a word.  That would reduce the string allocations.
        internal List<Word> ChopTextIntoWords(string text, int maxWidthInBufferCells)
            List<Word> result = new List<Word>();
            if (maxWidthInBufferCells < 1)
            text = text.Replace('\t', ' ');
            result = new List<Word>();
            // a "word" is a span of characters delimited by whitespace.  Contiguous whitespace, too, is a word.
            int startIndex = 0;
            int wordEnd = 0;
            bool inWs = false;
            while (wordEnd < text.Length)
                if (text[wordEnd] == '\n')
                    if (startIndex < wordEnd)
                        // the span up to this point needs to be saved off
                        AddWord(text, startIndex, wordEnd, maxWidthInBufferCells, inWs, ref result);
                    // add a nl word
                    Word w = new Word();
                    w.Flags = WordFlags.IsNewline;
                    result.Add(w);
                    // skip the nl
                    ++wordEnd;
                    startIndex = wordEnd;
                    inWs = false;
                else if (text[wordEnd] == ' ')
                    if (!inWs)
                        // span from startIndex..(wordEnd - 1) is a word
                    inWs = true;
                    // not whitespace
                    if (inWs)
            if (startIndex != wordEnd)
                AddWord(text, startIndex, text.Length, maxWidthInBufferCells, inWs, ref result);
        /// Helper for ChopTextIntoWords.  Takes a span of characters in a string and adds it to the word list, further
        /// subdividing the span as needed so that each subdivision fits within the limit.
        /// The string of characters in which the span is to be extracted.
        /// <param name="startIndex">
        /// index into text of the start of the word to be added.
        /// <param name="endIndex">
        /// index of the char after the last char to be included in the word.
        /// <param name="isWhitespace">
        /// true if the span is whitespace, false if not.
        /// <param name="result">
        /// The list into which the words will be added.
        internal void AddWord(string text, int startIndex, int endIndex,
            int maxWidthInBufferCells, bool isWhitespace, ref List<Word> result)
            Dbg.Assert(endIndex >= startIndex, "startIndex must be before endIndex");
            Dbg.Assert(endIndex >= 0, "endIndex must be positive");
            Dbg.Assert(startIndex >= 0, "startIndex must be positive");
            Dbg.Assert(startIndex < text.Length, "startIndex must be within the string");
            Dbg.Assert(endIndex <= text.Length, "endIndex must be within the string");
            while (startIndex < endIndex)
                int i = Math.Min(endIndex, startIndex + maxWidthInBufferCells);
                if (isWhitespace)
                    w.Flags = WordFlags.IsWhitespace;
                    w.Text = text.Substring(startIndex, i - startIndex);
                    w.CellCount = RawUI.LengthInBufferCells(w.Text);
                    if (w.CellCount <= maxWidthInBufferCells)
                        // the segment from start..i fits
                        // The segment does not fit, back off a tad until it does
                Dbg.Assert(RawUI.LengthInBufferCells(w.Text) <= maxWidthInBufferCells, "word should not exceed max");
                startIndex = i;
        internal string WrapToCurrentWindowWidth(string text)
            // we leave a 1-cell margin on the end because if the very last character butts up against the
            // edge of the screen buffer, then the console will wrap the line.
            List<string> lines = WrapText(text, RawUI.WindowSize.Width - 1);
            foreach (string s in lines)
                if (++count != lines.Count)
        #endregion Word Wrapping
            // We should write debug to error stream only if debug is redirected.)
            if (_parent.ErrorFormat == Serialization.DataFormat.XML)
                _parent.ErrorSerializer.Serialize(message, "debug");
                    WriteLine(GetFormatStyleString(FormatStyle.Debug) + StringUtil.Format(ConsoleHostUserInterfaceStrings.DebugFormatString, message) + PSStyle.Instance.Reset);
                    WriteLine(
                        DebugForegroundColor,
                        DebugBackgroundColor,
                        StringUtil.Format(ConsoleHostUserInterfaceStrings.DebugFormatString, message));
        public override void WriteInformation(InformationRecord record)
            // We should write information to error stream only if redirected.)
                _parent.ErrorSerializer.Serialize(record, "information");
                // Do nothing. The information stream is not visible by default
            // NTRAID#Windows OS Bugs-1061752-2004/12/15-sburns should read a skin setting here...)
                _parent.ErrorSerializer.Serialize(message, "verbose");
                    WriteLine(GetFormatStyleString(FormatStyle.Verbose) + StringUtil.Format(ConsoleHostUserInterfaceStrings.VerboseFormatString, message) + PSStyle.Instance.Reset);
                        VerboseForegroundColor,
                        VerboseBackgroundColor,
                        StringUtil.Format(ConsoleHostUserInterfaceStrings.VerboseFormatString, message));
                _parent.ErrorSerializer.Serialize(message, "warning");
                    WriteLine(GetFormatStyleString(FormatStyle.Warning) + StringUtil.Format(ConsoleHostUserInterfaceStrings.WarningFormatString, message) + PSStyle.Instance.Reset);
                        WarningForegroundColor,
                        WarningBackgroundColor,
                        StringUtil.Format(ConsoleHostUserInterfaceStrings.WarningFormatString, message));
        /// Invoked by CommandBase.WriteProgress to display a progress record.
            Dbg.Assert(record != null, "WriteProgress called with null ProgressRecord");
                PSObject obj = new PSObject();
                obj.Properties.Add(new PSNoteProperty("SourceId", sourceId));
                obj.Properties.Add(new PSNoteProperty("Record", record));
                _parent.ErrorSerializer.Serialize(obj, "progress");
            else if (Console.IsOutputRedirected)
                // Do not write progress bar when the stdout is redirected.
                // We allow only one thread at a time to update the progress state.)
                    HandleIncomingProgressRecord(sourceId, record);
            TextWriter writer = (!Console.IsErrorRedirected || _parent.IsInteractive)
                ? _parent.ConsoleTextWriter
                : Console.Error;
                Dbg.Assert(writer == _parent.ErrorSerializer.textWriter, "writers should be the same");
                _parent.ErrorSerializer.Serialize(value + Environment.NewLine);
                if (writer == _parent.ConsoleTextWriter)
                        WriteLine(value);
                        WriteLine(ErrorForegroundColor, ErrorBackgroundColor, value);
                    Console.Error.WriteLine(value);
        public ConsoleColor FormatAccentColor { get; set; } = ConsoleColor.Green;
        public ConsoleColor ErrorAccentColor { get; set; } = ConsoleColor.Cyan;
        public ConsoleColor ErrorForegroundColor { get; set; } = ConsoleColor.Red;
        public ConsoleColor ErrorBackgroundColor { get; set; } = Console.BackgroundColor;
        public ConsoleColor WarningForegroundColor { get; set; } = ConsoleColor.Yellow;
        public ConsoleColor WarningBackgroundColor { get; set; } = Console.BackgroundColor;
        public ConsoleColor DebugForegroundColor { get; set; } = ConsoleColor.Yellow;
        public ConsoleColor DebugBackgroundColor { get; set; } = Console.BackgroundColor;
        public ConsoleColor VerboseForegroundColor { get; set; } = ConsoleColor.Yellow;
        public ConsoleColor VerboseBackgroundColor { get; set; } = Console.BackgroundColor;
        public ConsoleColor ProgressForegroundColor { get; set; } = ConsoleColor.Black;
        public ConsoleColor ProgressBackgroundColor { get; set; } = ConsoleColor.Yellow;
        #endregion Line-oriented interaction
        #region implementation
        // We use System.Environment.NewLine because we are platform-agnostic
        internal static readonly string Crlf = System.Environment.NewLine;
        private const string Tab = "\x0009";
        internal enum ReadLineResult
            endedOnEnter = 0,
            endedOnTab = 1,
            endedOnShiftTab = 2,
            endedOnBreak = 3
        private const int MaxInputLineLength = 1024;
        /// Reads a line of input from the console.  Returns when the user hits enter, a break key, a break event occurs.  In
        /// the case that stdin has been redirected, reads from the stdin stream instead of the console.
        /// true to end input when the user hits the tab or shift-tab keys, false to only end on the enter key (or a break
        /// event). Ignored if not reading from the console device.
        /// <param name="initialContent">
        /// The initial contents of the input buffer.  Nice if you want to have a default result. Ignored if not reading from the
        /// console device.
        /// Receives an enum value indicating how input was ended.
        /// <param name="calledFromPipeline">
        /// TBD
        /// <param name="transcribeResult">
        /// true to include the results in any transcription that might be happening.
        /// The string read from either the console or the stdin stream.  null if:
        /// - stdin was read and EOF was reached on the stream, or
        /// - the console was read, and input was terminated with Ctrl-C, Ctrl-Break, or Close.
        internal string ReadLine(bool endOnTab, string initialContent, out ReadLineResult result, bool calledFromPipeline, bool transcribeResult)
            result = ReadLineResult.endedOnEnter;
            // If the test hook is set, read from it.
                return s_h.ReadLine();
            string restOfLine = null;
            string s = ReadFromStdin
                ? ReadLineFromFile(initialContent)
                : ReadLineFromConsole(endOnTab, initialContent, calledFromPipeline, ref restOfLine, ref result);
                PostRead(s);
                PostRead();
            if (restOfLine != null)
                s += restOfLine;
        private string ReadLineFromFile(string initialContent)
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(initialContent))
                sb.Append(initialContent);
            var consoleIn = _parent.ConsoleIn.Value;
                var inC = consoleIn.Read();
                if (inC == -1)
                    // EOF - we return null which tells our caller to exit
                    // but only if we don't have any input, we could have
                    // input and then stdin was closed, but never saw a newline.
                    return sb.Length == 0 ? null : sb.ToString();
                var c = unchecked((char)inC);
                if (!NoPrompt)
                    Console.Out.Write(c);
                if (c == '\r')
                    // Treat as newline, but consume \n if there is one.
                    if (consoleIn.Peek() == '\n')
                        consoleIn.Read();
                // If NoPrompt is true, we are in a sort of server mode where we shouldn't
                // do anything like edit the command line - every character is part of the input.
                if (c == '\b' && !NoPrompt)
                    sb.Remove(sb.Length - 1, 1);
        private string ReadLineFromConsole(bool endOnTab, string initialContent, bool calledFromPipeline, ref string restOfLine, ref ReadLineResult result)
                ConsoleControl.ConsoleModes.LineInput
                | ConsoleControl.ConsoleModes.EchoInput
                | ConsoleControl.ConsoleModes.ProcessedInput;
            if ((m & DesiredMode) != DesiredMode || (m & ConsoleControl.ConsoleModes.MouseInput) > 0)
                m &= ~ConsoleControl.ConsoleModes.MouseInput;
            // If more characters are typed than you asked, then the next call to ReadConsole will return the
            // additional characters beyond those you requested.
            // If input is terminated with a tab key, then the buffer returned will have a tab (ascii 0x9) at the
            // position where the tab key was hit.  If the user has arrowed backward over existing input in the line
            // buffer, the tab will overwrite whatever character was in that position. That character will be lost in
            // the input buffer, but since we echo each character the user types, it's still in the active screen buffer
            // and we can read the console output to get that character.
            // If input is terminated with an enter key, then the buffer returned will have ascii 0x0D and 0x0A
            // (Carriage Return and Line Feed) as the last two characters of the buffer.
            // If input is terminated with a break key (Ctrl-C, Ctrl-Break, Close, etc.), then the buffer will be
            // the empty string.
            // For Unix systems, we implement a basic readline loop around Console.ReadKey(), that
            // supports backspace, arrow keys, Ctrl-C, and Ctrl-D. This readline is only used for
            // interactive prompts (like Read-Host), otherwise it is assumed that PSReadLine is
            // available. Therefore this explicitly does not support history or tab completion.
                ConsoleKeyInfo keyInfo;
                string s = string.Empty;
                int cursorLeft = Console.CursorLeft;
                int cursorCurrent = cursorLeft;
                bool insertMode = true;
            uint keyState = 0;
            Span<char> inputBuffer = stackalloc char[MaxInputLineLength + 1];
            if (initialContent.Length > 0)
                initialContent.AsSpan().CopyTo(inputBuffer);
                    keyInfo = Console.ReadKey(true);
                s += ConsoleControl.ReadConsole(handle, initialContent.Length, inputBuffer, MaxInputLineLength, endOnTab, out keyState);
                Dbg.Assert(s != null, "s should never be null");
                if (s.Length == 0)
                    result = ReadLineResult.endedOnBreak;
                    s = null;
                    if (calledFromPipeline)
                        // make sure that the pipeline that called us is stopped
                if (s.EndsWith(Environment.NewLine, StringComparison.Ordinal))
                        // We're intercepting characters, so we need to echo the newline
                        Console.Out.WriteLine();
                    s = s.Remove(s.Length - Environment.NewLine.Length);
                    if (keyInfo.Key == ConsoleKey.Tab)
                        // This is unsupported
                int i = s.IndexOf(Tab, StringComparison.Ordinal);
                if (endOnTab && i != -1)
                    // then the tab we found is the completion character.  bit 0x10 is set if the shift key was down
                    // when the key was hit.
                    if ((keyState & 0x10) == 0)
                        result = ReadLineResult.endedOnTab;
                    else if ((keyState & 0x10) > 0)
                        result = ReadLineResult.endedOnShiftTab;
                        // do nothing: leave the result state as it was. This is the circumstance when we've have to
                        // do more than one iteration and the input ended on a tab or shift-tab, or the user hit
                        // enter, or the user hit ctrl-c
                    // also clean up the screen -- if the cursor was positioned somewhere before the last character
                    // in the input buffer, then the characters from the tab to the end of the buffer need to be
                    // erased.
                    int leftover = RawUI.LengthInBufferCells(s.Substring(i + 1));
                    if (leftover > 0)
                        Coordinates c = RawUI.CursorPosition;
                        // before cleaning up the screen, read the active screen buffer to retrieve the character that
                        // is overridden by the tab
                        char charUnderCursor = GetCharacterUnderCursor(c);
                        Write(StringUtil.Padding(leftover));
                        RawUI.CursorPosition = c;
                        restOfLine = s[i] + (charUnderCursor + s.Substring(i + 1));
                        restOfLine += s[i];
                    s = s.Remove(i);
                            int length = s.Length;
                            s = s.Remove(index - 1, 1);
                            index--;
                            cursorCurrent = Console.CursorLeft;
                            Console.CursorLeft = cursorLeft;
                            Console.Out.Write(s.PadRight(length));
                            Console.CursorLeft = cursorCurrent - 1;
                    if (keyInfo.Key == ConsoleKey.Delete
                        || (keyInfo.Key == ConsoleKey.D && keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control)))
                        if (index < s.Length)
                            s = s.Remove(index, 1);
                            Console.CursorLeft = cursorCurrent;
                    if (keyInfo.Key == ConsoleKey.LeftArrow
                        || (keyInfo.Key == ConsoleKey.B && keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control)))
                        if (Console.CursorLeft > cursorLeft)
                            Console.CursorLeft--;
                    if (keyInfo.Key == ConsoleKey.RightArrow
                        || (keyInfo.Key == ConsoleKey.F && keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control)))
                        if (Console.CursorLeft < cursorLeft + s.Length)
                            Console.CursorLeft++;
                    if (keyInfo.Key == ConsoleKey.UpArrow
                        || keyInfo.Key == ConsoleKey.DownArrow
                        || keyInfo.Key == ConsoleKey.PageUp
                        || keyInfo.Key == ConsoleKey.PageDown)
                        // Arrow/Page Up/down is unimplemented, so fail gracefully
                    if (keyInfo.Key == ConsoleKey.Home
                        || (keyInfo.Key == ConsoleKey.A && keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control)))
                    if (keyInfo.Key == ConsoleKey.End
                        || (keyInfo.Key == ConsoleKey.E && keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control)))
                        Console.CursorLeft = cursorLeft + s.Length;
                        index = s.Length;
                    if (keyInfo.Key == ConsoleKey.Escape)
                        s = string.Empty;
                    if (keyInfo.Key == ConsoleKey.Insert)
                        // Toggle insert/overwrite mode
                        insertMode = !insertMode;
                    if (char.IsControl(keyInfo.KeyChar))
                    // Handle case where terminal gets reset and the index is outside of the buffer
                    if (index > s.Length)
                    // Modify string
                    if (!insertMode && index < s.Length) // then overwrite mode
                    s = s.Insert(index, keyInfo.KeyChar.ToString());
                    // Redisplay string
                    Console.Out.Write(s);
                    Console.CursorLeft = cursorCurrent + 1;
                       (s == null && result == ReadLineResult.endedOnBreak)
                       || (s != null && result != ReadLineResult.endedOnBreak),
                       "s should only be null if input ended with a break");
        /// Get the character at the cursor when the user types 'tab' in the middle of line.
        /// <param name="cursorPosition">The cursor position where 'tab' is hit.</param>
        private char GetCharacterUnderCursor(Coordinates cursorPosition)
            Rectangle region = new Rectangle(0, cursorPosition.Y, RawUI.BufferSize.Width - 1, cursorPosition.Y);
            BufferCell[,] content = RawUI.GetBufferContents(region);
            for (int index = 0, column = 0; column <= cursorPosition.X; index++)
                BufferCell cell = content[0, index];
                if (cell.BufferCellType == BufferCellType.Complete || cell.BufferCellType == BufferCellType.Leading)
                    if (column == cursorPosition.X)
                        return cell.Character;
                    column += ConsoleControl.LengthInBufferCells(cell.Character);
            Dbg.Assert(false, "the character at the cursor should be retrieved, never gets to here");
            return '\0';
        /// Strip nulls from a string.
        /// <param name="input">The string to process.</param>
        /// <returns>The string with any '\0' characters removed.</returns>
        private static string RemoveNulls(string input)
            if (!input.Contains('\0'))
            StringBuilder sb = new StringBuilder(input.Length);
            foreach (char c in input)
                if (c != '\0')
        /// Reads a line, and completes the input for the user if they hit tab.
        /// <param name="exec">
        /// The Executor instance on which to run any pipelines that are needed to find matches
        /// null on a break event
        /// the completed line otherwise
        internal string ReadLineWithTabCompletion(Executor exec)
            string input = null;
            string lastInput = string.Empty;
            ReadLineResult rlResult = ReadLineResult.endedOnEnter;
            string lastCompletion = string.Empty;
            Size screenBufferSize = RawUI.BufferSize;
            // Save the cursor position at the end of the prompt string so that we can restore it later to write the
            // completed input.
            Coordinates endOfPromptCursorPos = RawUI.CursorPosition;
            CommandCompletion commandCompletion = null;
            string completionInput = null;
                if (TryInvokeUserDefinedReadLine(out input))
                input = ReadLine(true, lastInput, out rlResult, false, false);
                if (input == null)
                if (rlResult == ReadLineResult.endedOnEnter)
#if UNIX // Portable code only ends on enter (or no input), so tab is not processed
                throw new PlatformNotSupportedException("This readline state is unsupported in portable code!");
                Coordinates endOfInputCursorPos = RawUI.CursorPosition;
                string completedInput = null;
                if (rlResult == ReadLineResult.endedOnTab || rlResult == ReadLineResult.endedOnShiftTab)
                    int tabIndex = input.IndexOf(Tab, StringComparison.Ordinal);
                    Dbg.Assert(tabIndex != -1, "tab should appear in the input");
                    string restOfLine = string.Empty;
                    int leftover = input.Length - tabIndex - 1;
                        // We are reading from the console (not redirected, b/c we don't end on tab when redirected)
                        // If the cursor is at the end of a line, there is actually a space character at the cursor's position and when we type tab
                        // at the end of a line, that space character is replaced by the tab. But when we type tab at the middle of a line, the space
                        // character at the end is preserved, we should remove that space character because it's not provided by the user.
                        input = input.Remove(input.Length - 1);
                        restOfLine = input.Substring(tabIndex + 1);
                    input = input.Remove(tabIndex);
                    if (input != lastCompletion || commandCompletion == null)
                        completionInput = input;
                        commandCompletion = GetNewCompletionResults(input);
                    var completionResult = commandCompletion.GetNextResult(rlResult == ReadLineResult.endedOnTab);
                    if (completionResult != null)
                        completedInput = string.Concat(completionInput.AsSpan(0, commandCompletion.ReplacementIndex), completionResult.CompletionText);
                        completedInput = completionInput;
                    if (restOfLine != string.Empty)
                        completedInput += restOfLine;
                    if (completedInput.Length > (MaxInputLineLength - 2))
                        completedInput = completedInput.Substring(0, MaxInputLineLength - 2);
                    // Remove any nulls from the string...
                    completedInput = RemoveNulls(completedInput);
                    // adjust the saved cursor position if the buffer scrolled as the user was typing (i.e. the user
                    // typed past the end of the buffer).
                    int linesOfInput = (endOfPromptCursorPos.X + input.Length) / screenBufferSize.Width;
                    endOfPromptCursorPos.Y = endOfInputCursorPos.Y - linesOfInput;
                    // replace the displayed input with the new input
                        RawUI.CursorPosition = endOfPromptCursorPos;
                    catch (PSArgumentOutOfRangeException)
                        // If we go a range exception, it's because
                        // there's no room in the buffer for the completed
                        // line so we'll just pretend that there was no match...
                    // When the string is written to the console, a space character is actually appended to the string
                    // and the cursor will flash at the position of that space character.
                    WriteToConsole(completedInput, false);
                    Coordinates endOfCompletionCursorPos = RawUI.CursorPosition;
                    // adjust the starting cursor position if the screen buffer has scrolled as a result of writing the
                    // completed input (i.e. writing the completed input ran past the end of the buffer).
                    int linesOfCompletedInput = (endOfPromptCursorPos.X + completedInput.Length) / screenBufferSize.Width;
                    endOfPromptCursorPos.Y = endOfCompletionCursorPos.Y - linesOfCompletedInput;
                    // blank out any "leftover" old input.  That's everything between the cursor position at the time
                    // the user hit tab up to the current cursor position after writing the completed text.
                    int deltaInput =
                        (endOfInputCursorPos.Y * screenBufferSize.Width + endOfInputCursorPos.X)
                        - (endOfCompletionCursorPos.Y * screenBufferSize.Width + endOfCompletionCursorPos.X);
                    if (deltaInput > 0)
                        ConsoleControl.FillConsoleOutputCharacter(handle, ' ', deltaInput, endOfCompletionCursorPos);
                        lastCompletion = completedInput.Remove(completedInput.Length - restOfLine.Length);
                        SendLeftArrows(restOfLine.Length);
                        lastCompletion = completedInput;
                    lastInput = completedInput;
            // Since we did not transcribe any call to ReadLine, transcribe the results here.
            if (_parent.IsTranscribing)
                // Reads always terminate with the enter key, so add that.
                _parent.WriteLineToTranscript(input);
        private static void SendLeftArrows(int length)
            var inputs = new ConsoleControl.INPUT[length * 2];
                var down = new ConsoleControl.INPUT();
                down.Type = (uint)ConsoleControl.InputType.Keyboard;
                down.Data.Keyboard = new ConsoleControl.KeyboardInput();
                down.Data.Keyboard.Vk = (ushort)ConsoleControl.VirtualKeyCode.Left;
                down.Data.Keyboard.Scan = 0;
                down.Data.Keyboard.Flags = 0;
                down.Data.Keyboard.Time = 0;
                down.Data.Keyboard.ExtraInfo = IntPtr.Zero;
                var up = new ConsoleControl.INPUT();
                up.Type = (uint)ConsoleControl.InputType.Keyboard;
                up.Data.Keyboard = new ConsoleControl.KeyboardInput();
                up.Data.Keyboard.Vk = (ushort)ConsoleControl.VirtualKeyCode.Left;
                up.Data.Keyboard.Scan = 0;
                up.Data.Keyboard.Flags = (uint)ConsoleControl.KeyboardFlag.KeyUp;
                up.Data.Keyboard.Time = 0;
                up.Data.Keyboard.ExtraInfo = IntPtr.Zero;
                inputs[2 * i] = down;
                inputs[2 * i + 1] = up;
            ConsoleControl.MimicKeyPress(inputs);
        private CommandCompletion GetNewCompletionResults(string input)
                var runspace = _parent.Runspace;
                var debugger = runspace.Debugger;
                if ((debugger != null) && debugger.InBreakpoint)
                    // If in debug stop mode do command completion though debugger process command.
                        return CommandCompletion.CompleteInputInDebugger(input, input.Length, null, debugger);
                if (runspace is LocalRunspace &&
                    runspace.ExecutionContext.EngineHostInterface.NestedPromptCount > 0)
                    _commandCompletionPowerShell = PowerShell.Create(RunspaceMode.CurrentRunspace);
                    _commandCompletionPowerShell = PowerShell.Create();
                    _commandCompletionPowerShell.SetIsNested(_parent.IsNested);
                    _commandCompletionPowerShell.Runspace = runspace;
                return CommandCompletion.CompleteInput(input, input.Length, null, _commandCompletionPowerShell);
                _commandCompletionPowerShell = null;
        private const string CustomReadlineCommand = "PSConsoleHostReadLine";
        private bool TryInvokeUserDefinedReadLine(out string input)
            // We're using GetCommands instead of GetCommand so we don't auto-load a module should the command exist, but isn't loaded.
            // The idea is that if someone hasn't defined the command (say because they started -noprofile), we shouldn't auto-load
            var runspace = _parent.LocalRunspace;
            if (runspace != null &&
                runspace.Engine.Context.EngineIntrinsics.InvokeCommand.GetCommands(CustomReadlineCommand,
                    CommandTypes.Function | CommandTypes.Cmdlet, nameIsPattern: false).Any())
                    PowerShell ps;
                    if ((runspace.ExecutionContext.EngineHostInterface.NestedPromptCount > 0) &&
                        (Runspace.DefaultRunspace != null))
                        ps = PowerShell.Create(RunspaceMode.CurrentRunspace);
                        ps = PowerShell.Create();
                        ps.Runspace = runspace;
                    var result = ps.AddCommand(CustomReadlineCommand).Invoke();
                    if (result.Count == 1)
                        input = PSObject.Base(result[0]) as string;
            input = null;
        #endregion implementation
        // used to serialize access to instance data
        private readonly object _instanceLock = new object();
        // If this is true, class throws on read or prompt method which require
        // access to console.
                _throwOnReadAndPrompt = value;
        private bool _throwOnReadAndPrompt;
        internal void HandleThrowOnReadAndPrompt()
            if (_throwOnReadAndPrompt)
                throw PSTraceSource.NewInvalidOperationException(ConsoleHostUserInterfaceStrings.ReadFailsOnNonInteractiveFlag);
        // this is a test hook for the ConsoleInteractiveTestTool, which sets this field to true.
        private readonly bool _isInteractiveTestToolListening;
        // This instance data is "read-only" and need not have access serialized.
        private readonly ConsoleHostRawUserInterface _rawui;
        [TraceSource("ConsoleHostUserInterface", "Console host's subclass of S.M.A.Host.Console")]
        private static readonly PSTraceSource s_tracer = PSTraceSource.GetTracer("ConsoleHostUserInterface", "Console host's subclass of S.M.A.Host.Console");

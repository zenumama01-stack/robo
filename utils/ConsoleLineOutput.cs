    /// Tear off class.
    internal class DisplayCellsHost : DisplayCells
        internal DisplayCellsHost(PSHostRawUserInterface rawUserInterface)
            _rawUserInterface = rawUserInterface;
        internal override int Length(string str, int offset)
            if (offset < 0 || offset >= str.Length)
                throw PSTraceSource.NewArgumentException(nameof(offset));
                    length += _rawUserInterface.LengthInBufferCells(str[offset]);
                // thrown when external host rawui is not implemented, in which case
                // we will fallback to the default value.
                return base.Length(str, offset);
        internal override int Length(char character)
                return _rawUserInterface.LengthInBufferCells(character);
                return base.Length(character);
        private readonly PSHostRawUserInterface _rawUserInterface;
    /// Implementation of the LineOutput interface on top of Console and RawConsole.
    internal sealed class ConsoleLineOutput : LineOutput
        [TraceSource("ConsoleLineOutput", "ConsoleLineOutput")]
        internal static readonly PSTraceSource tracer = PSTraceSource.GetTracer("ConsoleLineOutput", "ConsoleLineOutput");
        /// The default buffer cell calculation already works for the PowerShell console host and Visual studio code host.
        private static readonly HashSet<string> s_psHost = new(StringComparer.Ordinal) { "ConsoleHost", "Visual Studio Code Host" };
        /// The # of columns is just the width of the screen buffer (not the
        /// width of the window)
                PSHostRawUserInterface raw = _console.RawUI;
                // IMPORTANT NOTE: we subtract one because
                // we want to make sure the console's last column
                // is never considered written. This causes the writing
                // logic to always call WriteLine(), making sure a CR
                // is inserted.
                    return _forceNewLine ? raw.BufferSize.Width - 1 : raw.BufferSize.Width;
                return _forceNewLine ? _fallbackRawConsoleColumnNumber - 1 : _fallbackRawConsoleColumnNumber;
        /// The # of rows is the # of rows visible in the window (and not the # of
        /// rows in the screen buffer)
                    return raw.WindowSize.Height;
                return _fallbackRawConsoleRowNumber;
            // delegate the action to the helper,
            // that will properly break the string into
            // screen lines
            _writeLineHelper.WriteLine(s, this.ColumnNumber);
        internal override DisplayCells DisplayCells
                if (_displayCellsHost != null)
                    return _displayCellsHost;
                // fall back if we do not have a Msh host specific instance
        /// Constructor for the ConsoleLineOutput.
        /// <param name="host">PSHostUserInterface to wrap.</param>
        /// <param name="paging">True if we require prompting for page breaks.</param>
        /// <param name="errorContext">Error context to throw exceptions.</param>
        internal ConsoleLineOutput(PSHost host, bool paging, TerminatingErrorContext errorContext)
            if (host == null)
                throw PSTraceSource.NewArgumentNullException(nameof(host));
            if (errorContext == null)
                throw PSTraceSource.NewArgumentNullException(nameof(errorContext));
            _console = host.UI;
            if (paging)
                tracer.WriteLine("paging is needed");
                // If we need to do paging, instantiate a prompt handler that will take care of the screen interaction
                string promptString = StringUtil.Format(FormatAndOut_out_xxx.ConsoleLineOutput_PagingPrompt);
                _prompt = new PromptHandler(promptString, this);
            if (!s_psHost.Contains(host.Name) && _console.RawUI is not null)
                // set only if we have a valid raw interface
                tracer.WriteLine("there is a valid raw interface");
                _displayCellsHost = new DisplayCellsHost(_console.RawUI);
            // instantiate the helper to do the line processing when ILineOutput.WriteXXX() is called
            WriteLineHelper.WriteCallback wl = new WriteLineHelper.WriteCallback(this.OnWriteLine);
            WriteLineHelper.WriteCallback w = new WriteLineHelper.WriteCallback(this.OnWrite);
            if (_forceNewLine)
                _writeLineHelper = new WriteLineHelper(/*lineWrap*/false, wl, null, this.DisplayCells);
                _writeLineHelper = new WriteLineHelper(/*lineWrap*/false, wl, w, this.DisplayCells);
        /// Callback to be called when ILineOutput.WriteLine() is called by WriteLineHelper.
            // Do any default transcription.
            _console.TranscribeResult(s);
            switch (this.WriteStream)
                case WriteStreamType.Error:
                    _console.WriteErrorLine(s);
                case WriteStreamType.Warning:
                    _console.WriteWarningLine(s);
                case WriteStreamType.Verbose:
                    _console.WriteVerboseLine(s);
                case WriteStreamType.Debug:
                    _console.WriteDebugLine(s);
                    // If the host is in "transcribe only"
                    // mode (due to an implicitly added call to Out-Default -Transcribe),
                    // then don't call the actual host API.
                    if (!_console.TranscribeOnly)
                        _console.WriteLine(s);
            LineWrittenEvent();
        /// Callback to be called when ILineOutput.Write() is called by WriteLineHelper
                    _console.Write(s);
        /// Called when a line was written to console.
        private void LineWrittenEvent()
            // check to avoid reentrancy from the prompt handler
            // writing during the PromptUser() call
            if (_disableLineWrittenEvent)
            // if there is no prompting, we are done
            if (_prompt == null)
            // increment the count of lines written to the screen
            _linesWritten++;
            // check if we need to put out a prompt
            if (this.NeedToPrompt)
                // put out the prompt
                _disableLineWrittenEvent = true;
                PromptHandler.PromptResponse response = _prompt.PromptUser(_console);
                _disableLineWrittenEvent = false;
                switch (response)
                    case PromptHandler.PromptResponse.NextPage:
                            // reset the counter, since we are starting a new page
                            _linesWritten = 0;
                    case PromptHandler.PromptResponse.NextLine:
                            // roll back the counter by one, since we allow one more line
                            _linesWritten--;
                    case PromptHandler.PromptResponse.Quit:
                        // 1021203-2005/05/09-JonN
                        // HaltCommandException will cause the command
                        // to stop, but not be reported as an error.
                        throw new HaltCommandException();
        /// Check if we need to put out a prompt.
        /// <value>true if we need to prompt</value>
        private bool NeedToPrompt
                // NOTE: we recompute all the time to take into account screen resizing
                int rawRowNumber = this.RowNumber;
                if (rawRowNumber <= 0)
                    // something is wrong, there is no real estate, we suppress prompting
                // the prompt will occupy some lines, so we need to subtract them form the total
                // screen line count
                int computedPromptLines = _prompt.ComputePromptLines(this.DisplayCells, this.ColumnNumber);
                int availableLines = this.RowNumber - computedPromptLines;
                if (availableLines <= 0)
                    tracer.WriteLine("No available Lines; suppress prompting");
                return _linesWritten >= availableLines;
        /// Object to manage prompting.
        private sealed class PromptHandler
            /// Prompt handler with the given prompt.
            /// <param name="s">Prompt string to be used.</param>
            /// <param name="cmdlet">The Cmdlet using this prompt handler.</param>
            internal PromptHandler(string s, ConsoleLineOutput cmdlet)
                _promptString = s;
                _callingCmdlet = cmdlet;
            /// Determine how many rows the prompt should take.
            /// <param name="cols">Current number of columns on the screen.</param>
            /// <param name="displayCells">String manipulation helper.</param>
            internal int ComputePromptLines(DisplayCells displayCells, int cols)
                // split the prompt string into lines
                _actualPrompt = StringManipulationHelper.GenerateLines(displayCells, _promptString, cols, cols);
                return _actualPrompt.Count;
            /// Options returned by the PromptUser() call.
            internal enum PromptResponse
                NextPage,
                NextLine,
                Quit
            /// Do the actual prompting.
            /// <param name="console">PSHostUserInterface instance to prompt to.</param>
            internal PromptResponse PromptUser(PSHostUserInterface console)
                // NOTE: assume the values passed to ComputePromptLines are still valid
                // write out the prompt line(s). The last one will not have a new line
                // at the end because we leave the prompt at the end of the line
                for (int k = 0; k < _actualPrompt.Count; k++)
                    if (k < (_actualPrompt.Count - 1))
                        console.WriteLine(_actualPrompt[k]); // intermediate line(s)
                        console.Write(_actualPrompt[k]); // last line
                    _callingCmdlet.CheckStopProcessing();
                    KeyInfo ki = console.RawUI.ReadKey(ReadKeyOptions.IncludeKeyUp | ReadKeyOptions.NoEcho);
                    char key = ki.Character;
                    if (key == 'q' || key == 'Q')
                        // need to move to the next line since we accepted input, add a newline
                        console.WriteLine();
                        return PromptResponse.Quit;
                    else if (key == ' ')
                        return PromptResponse.NextPage;
                    else if (key == '\r')
                        return PromptResponse.NextLine;
            /// Cached string(s) valid during a sequence of ComputePromptLines()/PromptUser()
            private StringCollection _actualPrompt;
            /// Prompt string as passed at initialization.
            private readonly string _promptString;
            /// The cmdlet that uses this prompt helper.
            private readonly ConsoleLineOutput _callingCmdlet = null;
        /// Flag to force new lines in CMD.EXE by limiting the
        /// usable width to N-1 (e.g. 80-1) and forcing a call
        /// to WriteLine()
        private readonly bool _forceNewLine = true;
        /// Use this if IRawConsole is null;
        private readonly int _fallbackRawConsoleColumnNumber = 80;
        private readonly int _fallbackRawConsoleRowNumber = 40;
        /// Handler to prompt the user for page breaks
        /// if this handler is not null, we have prompting.
        private readonly PromptHandler _prompt = null;
        /// Counter for the # of lines written when prompting is on.
        private long _linesWritten = 0;
        /// Flag to avoid reentrancy on prompting.
        private bool _disableLineWrittenEvent = false;
        /// Reference to the PSHostUserInterface interface we use.
        private readonly PSHostUserInterface _console = null;
        /// Msh host specific string manipulation helper.
        private readonly DisplayCells _displayCellsHost;
        /// Reference to error context to throw Msh exceptions.
        private readonly TerminatingErrorContext _errorContext = null;

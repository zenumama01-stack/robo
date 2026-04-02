    /// Implementation for the out-string command.
    [Cmdlet(VerbsData.Out, "String", DefaultParameterSetName = "NoNewLineFormatting", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097024", RemotingCapability = RemotingCapability.None)]
    public class OutStringCommand : FrontEndCommandBase
        /// Optional, non positional parameter to specify the streaming behavior.
        /// FALSE: accumulate all the data, then write a single string.
        /// TRUE: write one line at the time.
        [Parameter(ParameterSetName = "StreamFormatting")]
        public SwitchParameter Stream
            get { return _stream; }
            set { _stream = value; }
        private bool _stream;
        [Parameter(ParameterSetName = "NoNewLineFormatting")]
            get { return _noNewLine; }
            set { _noNewLine = value; }
        private bool _noNewLine = false;
        /// Initializes a new instance of the <see cref="OutStringCommand"/> class
        public OutStringCommand()
            // set up the LineOutput interface
        /// by creating one on top of a stream.
            // set up the streaming text writer
            StreamingTextWriter.WriteLineCallback callback = new(this.OnWriteLine);
            _writer = new StreamingTextWriter(callback, Host.CurrentCulture);
            // use it to create and initialize the Line Output writer
            TextWriterLineOutput twlo = new(_writer, computedWidth);
            // finally have the LineOutput interface extracted
        /// Callback to add lines to the buffer or to write them to the output stream.
            if (_stream)
                this.WriteObject(s);
                if (_noNewLine)
                    _buffer.Append(s);
                    _buffer.AppendLine(s);
            // close the writer
            _writer.Dispose();
            if (!_stream)
                this.WriteObject(_buffer.ToString());
        /// Writer used by the LineOutput.
        private StreamingTextWriter _writer = null;
        /// Buffer used when buffering until the end.
        private readonly StringBuilder _buffer = new();

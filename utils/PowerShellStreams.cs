    /// Define all the output streams and one input stream for a workflow.
    public sealed class PowerShellStreams<TInput, TOutput> : IDisposable
        /// Input stream for incoming objects.
        private PSDataCollection<TInput> _inputStream;
        /// Output stream for returned objects.
        private PSDataCollection<TOutput> _outputStream;
        /// Error stream for error messages.
        private PSDataCollection<ErrorRecord> _errorStream;
        /// Warning stream for warning messages.
        private PSDataCollection<WarningRecord> _warningStream;
        /// Progress stream for progress messages.
        private PSDataCollection<ProgressRecord> _progressStream;
        /// Verbose stream for verbose messages.
        private PSDataCollection<VerboseRecord> _verboseStream;
        /// Debug stream for debug messages.
        private PSDataCollection<DebugRecord> _debugStream;
        /// Information stream for Information messages.
        private PSDataCollection<InformationRecord> _informationStream;
        /// If the object is already disposed or not.
        /// Private object for thread-safe execution.
        private readonly object _syncLock = new object();
        public PowerShellStreams()
            _inputStream = null;
            _outputStream = null;
            _errorStream = null;
            _warningStream = null;
            _progressStream = null;
            _verboseStream = null;
            _debugStream = null;
            _informationStream = null;
            _disposed = false;
        public PowerShellStreams(PSDataCollection<TInput> pipelineInput)
            // Populate the input collection if there is any...
            _inputStream = pipelineInput ?? new PSDataCollection<TInput>();
            _inputStream.Complete();
            _outputStream = new PSDataCollection<TOutput>();
            _errorStream = new PSDataCollection<ErrorRecord>();
            _warningStream = new PSDataCollection<WarningRecord>();
            _progressStream = new PSDataCollection<ProgressRecord>();
            _verboseStream = new PSDataCollection<VerboseRecord>();
            _debugStream = new PSDataCollection<DebugRecord>();
            _informationStream = new PSDataCollection<InformationRecord>();
        /// Dispose implementation.
        /// Protected virtual implementation of Dispose.
            lock (_syncLock)
                        _inputStream.Dispose();
                        _outputStream.Dispose();
                        _errorStream.Dispose();
                        _warningStream.Dispose();
                        _progressStream.Dispose();
                        _verboseStream.Dispose();
                        _debugStream.Dispose();
                        _informationStream.Dispose();
        /// Gets input stream.
        public PSDataCollection<TInput> InputStream
            get { return _inputStream; }
            set { _inputStream = value; }
        /// Gets output stream.
        public PSDataCollection<TOutput> OutputStream
            get { return _outputStream; }
            set { _outputStream = value; }
        /// Gets error stream.
        public PSDataCollection<ErrorRecord> ErrorStream
            get { return _errorStream; }
            set { _errorStream = value; }
        /// Gets warning stream.
        public PSDataCollection<WarningRecord> WarningStream
            get { return _warningStream; }
            set { _warningStream = value; }
        /// Gets progress stream.
        public PSDataCollection<ProgressRecord> ProgressStream
            get { return _progressStream; }
            set { _progressStream = value; }
        /// Gets verbose stream.
        public PSDataCollection<VerboseRecord> VerboseStream
            get { return _verboseStream; }
            set { _verboseStream = value; }
        /// Get debug stream.
        public PSDataCollection<DebugRecord> DebugStream
            get { return _debugStream; }
            set { _debugStream = value; }
        /// Gets Information stream.
        public PSDataCollection<InformationRecord> InformationStream
            get { return _informationStream; }
            set { _informationStream = value; }
        /// Marking all the streams as completed so that no further data can be added and
        /// jobs will know that there is no more data coming in.
        public void CloseAll()
                        _outputStream.Complete();
                        _errorStream.Complete();
                        _warningStream.Complete();
                        _progressStream.Complete();
                        _verboseStream.Complete();
                        _debugStream.Complete();
                        _informationStream.Complete();

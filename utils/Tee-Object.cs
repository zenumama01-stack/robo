    /// Class for Tee-object implementation.
    [Cmdlet("Tee", "Object", DefaultParameterSetName = "File", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097034")]
    public sealed class TeeObjectCommand : PSCmdlet, IDisposable
        /// Object to process.
        /// FilePath parameter.
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "File")]
        public string FilePath
            get { return _fileName; }
            set { _fileName = value; }
        /// Literal FilePath parameter.
        [Parameter(Mandatory = true, ParameterSetName = "LiteralFile")]
        /// Append switch.
        [Parameter(ParameterSetName = "File")]
            get { return _append; }
            set { _append = value; }
        /// Gets or sets the Encoding.
        [Parameter(ParameterSetName = "LiteralFile")]
        public Encoding Encoding { get; set; } = Encoding.Default;
        /// Variable parameter.
        [Parameter(Mandatory = true, ParameterSetName = "Variable")]
        public string Variable
            get { return _variable; }
            set { _variable = value; }
        private string _variable;
            _commandWrapper = new CommandWrapper();
            if (string.Equals(ParameterSetName, "File", StringComparison.OrdinalIgnoreCase))
                _commandWrapper.Initialize(Context, "out-file", typeof(OutFileCommand));
                _commandWrapper.AddNamedParameter("filepath", _fileName);
                _commandWrapper.AddNamedParameter("append", _append);
                _commandWrapper.AddNamedParameter("encoding", Encoding);
            else if (string.Equals(ParameterSetName, "LiteralFile", StringComparison.OrdinalIgnoreCase))
                _commandWrapper.AddNamedParameter("LiteralPath", _fileName);
                // variable parameter set
                _commandWrapper.Initialize(Context, "set-variable", typeof(SetVariableCommand));
                _commandWrapper.AddNamedParameter("name", _variable);
                // Can't use set-var's passthru because it writes the var object to the pipeline, we want just
                // the values to be written
            _commandWrapper.Process(_inputObject);
            _commandWrapper.ShutDown();
            if (!_alreadyDisposed)
                _alreadyDisposed = true;
                if (_commandWrapper != null)
                    _commandWrapper.Dispose();
                    _commandWrapper = null;
        private CommandWrapper _commandWrapper;
        private bool _alreadyDisposed;

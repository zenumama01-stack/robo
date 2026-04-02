    internal static class InputFileOpenModeConversion
        internal static FileMode Convert(OpenMode openMode)
            return SessionStateUtilities.GetFileModeFromOpenMode(openMode);
    /// Implementation for the out-file command.
    [Cmdlet(VerbsData.Out, "File", SupportsShouldProcess = true, DefaultParameterSetName = "ByPath", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096621")]
    public class OutFileCommand : FrontEndCommandBase
        /// Initializes a new instance of the <see cref="OutFileCommand"/> class
        public OutFileCommand()
            this.implementation = new OutputManagerInner();
            get { return _noclobber; }
            set { _noclobber = value; }
        /// Optional, number of columns to use when writing to device.
        [ValidateRange(2, int.MaxValue)]
        public int Width
            get { return (_width != null) ? _width.Value : 0; }
            set { _width = value; }
        private int? _width = null;
                return _suppressNewline;
                _suppressNewline = value;
        private bool _suppressNewline = false;
            // set up the Screen Host interface
            OutputManagerInner outInner = (OutputManagerInner)this.implementation;
            // NOTICE: if any exception is thrown from here to the end of the method, the
            // cleanup code will be called in IDisposable.Dispose()
            outInner.LineOutput = InstantiateLineOutputInterface();
            if (_sw == null)
            // finally call the base class for general hookup
        /// One-time initialization: acquire a screen host interface
        /// by creating one on top of a file.
        /// NOTICE: we assume that at this time the file name is
        /// available in the CRO. JonN recommends: file name has to be
        /// a MANDATORY parameter on the command line.
        private LineOutput InstantiateLineOutputInterface()
            string action = StringUtil.Format(FormatAndOut_out_xxx.OutFile_Action);
            if (ShouldProcess(FilePath, action))
                    FilePath,
            // compute the # of columns available
            int computedWidth = int.MaxValue;
            if (_width != null)
                // use the value from the command line
                computedWidth = _width.Value;
            // use the stream writer to create and initialize the Line Output writer
            TextWriterLineOutput twlo = new(_sw, computedWidth, _suppressNewline);
            // finally have the ILineOutput interface extracted
            return (LineOutput)twlo;
        /// Execution entry point.
            _processRecordExecuted = true;
            // NOTICE: if any exception is thrown, the
            _sw.Flush();
            // When the Out-File is used in a redirection pipelineProcessor,
            // its ProcessRecord method may not be called when nothing is written to the
            // output pipe, for example:
            //     Write-Error error > test.txt
            // In this case, the EndProcess method should return immediately as if it's
            // never been called. The cleanup work will be done in IDisposable.Dispose()
            if (!_processRecordExecuted)
        /// InternalDispose.
        protected override void InternalDispose()
            base.InternalDispose();
                _readOnlyFileInfo = null;
        /// Indicate whether the ProcessRecord method was executed.
        /// When the Out-File is used in a redirection pipelineProcessor,
        /// its ProcessRecord method may not be called when nothing is written to the
        /// output pipe, for example:
        ///     Write-Error error > test.txt
        /// In this case, the EndProcess method should return immediately as if it's
        /// never been called.
        private bool _processRecordExecuted = false;

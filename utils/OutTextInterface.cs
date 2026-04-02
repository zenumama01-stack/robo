    /// Implementation for the out-lineoutput command
    /// it provides a wrapper for the OutCommandInner class,
    /// which is the general purpose output command.
    [Cmdlet(VerbsData.Out, "LineOutput")]
    public class OutLineOutputCommand : FrontEndCommandBase
        /// Command line switch for ILineOutput communication channel.
        public object LineOutput
            get { return _lineOutput; }
            set { _lineOutput = value; }
        private object _lineOutput = null;
        public OutLineOutputCommand()
            this.implementation = new OutCommandInner();
            if (_lineOutput == null)
                ProcessNullLineOutput();
            LineOutput lo = _lineOutput as LineOutput;
            if (lo == null)
                ProcessWrongTypeLineOutput(_lineOutput);
            ((OutCommandInner)this.implementation).LineOutput = lo;
        private void ProcessNullLineOutput()
            string msg = StringUtil.Format(FormatAndOut_out_xxx.OutLineOutput_NullLineOutputParameter);
                PSTraceSource.NewArgumentNullException("LineOutput"),
                "OutLineOutputNullLineOutputParameter",
        private void ProcessWrongTypeLineOutput(object obj)
            string msg = StringUtil.Format(FormatAndOut_out_xxx.OutLineOutput_InvalidLineOutputParameterType,
                obj.GetType().FullName,
                typeof(LineOutput).FullName);
                new InvalidCastException(),
                "OutLineOutputInvalidLineOutputParameterType",

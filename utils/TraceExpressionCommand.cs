    /// A cmdlet that traces the specified categories and flags for the duration of the
    /// specified expression.
    [Cmdlet(VerbsDiagnostic.Trace, "Command", DefaultParameterSetName = "expressionSet", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097136")]
    public class TraceCommandCommand : TraceListenerCommandBase, IDisposable
        /// The parameter for the expression that should be traced.
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = "expressionSet")]
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = "commandSet")]
        /// When set, this parameter is the arguments to pass to the command specified by
        /// the -Command parameter.
        [Parameter(ParameterSetName = "commandSet", ValueFromRemainingArguments = true)]
        /// If this parameter is specified the Msh Host trace listener will be added.
        private Collection<PSTraceSource> _matchingSources;
        /// Gets the PSTraceSource instances that match the names specified.
            Collection<PSTraceSource> preconfiguredSources = null;
            _matchingSources = ConfigureTraceSource(base.NameInternal, false, out preconfiguredSources);
            TurnOnTracing(_matchingSources, false);
            TurnOnTracing(preconfiguredSources, true);
            // Now that tracing has been configured, move all the sources into a
            // single collection
            foreach (PSTraceSource preconfiguredSource in preconfiguredSources)
                _matchingSources.Add(preconfiguredSource);
            if (ParameterSetName == "commandSet")
                // Create the CommandProcessor and add it to a pipeline
                CommandProcessorBase commandProcessor =
                    this.Context.CommandDiscovery.LookupCommandProcessor(Command, CommandOrigin.Runspace, false);
                // Add the parameters that were specified
                ParameterBinderController.AddArgumentsToCommandProcessor(commandProcessor, ArgumentList);
                _pipeline = new PipelineProcessor();
                _pipeline.Add(commandProcessor);
                // Hook up the success and error pipelines to this cmdlet's WriteObject and
                // WriteError methods
                _pipeline.ExternalErrorOutput = new TracePipelineWriter(this, true, _matchingSources);
                _pipeline.ExternalSuccessOutput = new TracePipelineWriter(this, false, _matchingSources);
            ResetTracing(_matchingSources);
        /// Executes the expression.
        /// Note, this was taken from apply-expression.
                case "expressionSet":
                    result = RunExpression();
                case "commandSet":
                    result = StepCommand();
            if (!LanguagePrimitives.IsNull(result))
        /// Finishes running the command if specified and then sets the
        /// tracing options and listeners back to their original values.
            if (_pipeline != null)
                Array results = _pipeline.SynchronousExecuteEnumerate(AutomationNull.Value);
        /// Ensures that the sub-pipeline we created gets stopped as well.
        protected override void StopProcessing() => _pipeline?.Stop();
        private object RunExpression()
            return Expression.DoInvokeReturnAsIs(
                dollarUnder: InputObject,
                input: new object[] { InputObject },
                args: Array.Empty<object>());
        private object StepCommand()
            if (InputObject != AutomationNull.Value)
                _pipeline.Step(InputObject);
        private PipelineProcessor _pipeline;
        /// Resets the TraceSource flags back to their original value and restores
        /// the original TraceListeners.
                // Reset the flags for the trace switch back to the original value
                ClearStoredState();
                _matchingSources = null;
                    _pipeline.Dispose();
                    _pipeline = null;
                // If there are any file streams, close those as well.
                if (this.FileStreams != null)
                    foreach (FileStream fileStream in this.FileStreams)
                        fileStream.Flush();
                        fileStream.Dispose();
    /// This class acts a pipe redirector for the sub-pipeline created by the Trace-Command
    /// cmdlet.  It gets attached to the sub-pipelines success or error pipeline and redirects
    /// all objects written to these pipelines to trace-command pipeline.
    internal sealed class TracePipelineWriter : PipelineWriter
        internal TracePipelineWriter(
            TraceListenerCommandBase cmdlet,
            bool writeError,
            Collection<PSTraceSource> matchingSources)
            ArgumentNullException.ThrowIfNull(matchingSources);
            _writeError = writeError;
            _matchingSources = matchingSources;
        /// Get the wait handle signaled when buffer space is available in the underlying stream.
        public override WaitHandle WaitHandle
        /// Check if the stream is open for further writes.
        /// <value>true if the underlying stream is open, otherwise; false.</value>
        /// Attempting to write to the underlying stream if IsOpen is false throws
        /// an <see cref="ObjectDisposedException"/>.
        public override bool IsOpen
                return _isOpen;
        /// Returns the number of objects in the underlying stream.
        public override int Count
            get { return 0; }
        /// Get the capacity of the stream.
        /// The capacity of the stream.
        /// The capacity is the number of objects that stream may contain at one time.  Once this
        /// limit is reached, attempts to write into the stream block until buffer space
        /// becomes available.
        public override int MaxCapacity
            get { return int.MaxValue; }
        /// Close the stream.
        /// Causes subsequent calls to IsOpen to return false and calls to
        /// a write operation to throw an ObjectDisposedException.
        /// All calls to Close() after the first call are silently ignored.
        /// <exception cref="ObjectDisposedException">
        /// The stream is already disposed.
        public override void Close()
            if (_isOpen)
                Flush();
                _isOpen = false;
        /// Flush the data from the stream.  Closed streams may be flushed,
        /// but disposed streams may not.
        /// The underlying stream is disposed.
        /// Write a single object into the underlying stream.
        /// <param name="obj">The object to add to the stream.</param>
        /// One, if the write was successful, otherwise;
        /// zero if the stream was closed before the object could be written,
        /// or if the object was AutomationNull.Value.
        /// The underlying stream is closed.
        public override int Write(object obj)
            _cmdlet.ResetTracing(_matchingSources);
            if (_writeError)
                ErrorRecord errorRecord = ConvertToErrorRecord(obj);
                    _cmdlet.WriteError(errorRecord);
                _cmdlet.WriteObject(obj);
            _cmdlet.TurnOnTracing(_matchingSources, false);
        /// Write objects to the underlying stream.
        /// <param name="obj">Object or enumeration to read from.</param>
        /// <param name="enumerateCollection">
        /// If enumerateCollection is true, and <paramref name="obj"/>
        /// is an enumeration according to LanguagePrimitives.GetEnumerable,
        /// the objects in the enumeration will be unrolled and
        /// written separately.  Otherwise, <paramref name="obj"/>
        /// will be written as a single object.
        /// <returns>The number of objects written.</returns>
        /// <paramref name="obj"/> contains AutomationNull.Value
        public override int Write(object obj, bool enumerateCollection)
            int numWritten = 0;
                if (enumerateCollection)
                    foreach (object o in LanguagePrimitives.GetEnumerable(obj))
                        ErrorRecord errorRecord = ConvertToErrorRecord(o);
                            numWritten++;
                _cmdlet.WriteObject(obj, enumerateCollection);
            return numWritten;
        private static ErrorRecord ConvertToErrorRecord(object obj)
            ErrorRecord result = null;
            if (obj is PSObject mshobj)
                object baseObject = mshobj.BaseObject;
                if (baseObject is not PSCustomObject)
                    obj = baseObject;
            if (obj is ErrorRecord errorRecordResult)
                result = errorRecordResult;
        private readonly TraceListenerCommandBase _cmdlet;
        private readonly bool _writeError;
        private bool _isOpen = true;
        private readonly Collection<PSTraceSource> _matchingSources = new();

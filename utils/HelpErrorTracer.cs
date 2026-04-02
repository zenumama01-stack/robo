    /// HelpErrorTracer is a class to help tracing errors happened during loading
    /// help content for a help topic.
    /// This class tracks help context information like help topic, help category
    /// and help file, which are usually not available when an error happens at
    /// down level.
    /// Following is how this class can be used.
    ///     using(HelpErrorTracer.Trace(helpTopic, helpCategory, helpFile))
    ///         InsideFunctionCall();
    /// At this moment, a TraceFrame instance, which is disposable, will be created.
    /// In inside function calls and the calls down on the call stack, error can
    /// be traced by calling,
    ///     HelpErrorTracer.TraceError(errorRecord)
    /// At this moment, the errorRecord will be temporarily stored with in TraceFrame instance.
    /// When the TraceFrame instance is disposed, all errorRecords stored will be
    /// dumped into HelpSystem.LastErrors with context information attached.
    internal class HelpErrorTracer
        /// TraceFrame class track basic context information for current help activity.
        /// TraceFrame instance exists in a scope governed by using statement. It is possible
        /// that a new TraceFrame instance will be created in the scope of another TraceFrame
        /// instance. The scopes of various live TraceFrame instances form a stack which is
        /// similar to call stacks of normal C# functions. This is why we call this class
        /// a "TraceFrame"
        /// TraceFrame itself implements IDisposable interface to guarantee a chance to
        /// write errors into system error pool when execution gets out of its scope. During
        /// disposal time, errorRecords accumulated will be written to system error pool
        /// together with error context information collected at instance creation.
        internal sealed class TraceFrame : IDisposable
            // Following are help context information
            private readonly string _helpFile = string.Empty;
            // ErrorRecords accumulated during the help content loading.
            private readonly Collection<ErrorRecord> _errors = new Collection<ErrorRecord>();
            private readonly HelpErrorTracer _helpTracer;
            /// Constructor. Here help context information will be collected.
            /// <param name="helpTracer"></param>
            internal TraceFrame(HelpErrorTracer helpTracer, string helpFile)
                _helpTracer = helpTracer;
            /// This is a interface for code in trace frame scope to add errorRecord into
            /// accumulative error pool.
            internal void TraceError(ErrorRecord errorRecord)
                if (_helpTracer.HelpSystem.VerboseHelpErrors)
                    _errors.Add(errorRecord);
            /// This is a interface for code in trace frame scope to add errorRecord's into
            /// <param name="errorRecords"></param>
            internal void TraceErrors(Collection<ErrorRecord> errorRecords)
                    foreach (ErrorRecord errorRecord in errorRecords)
            /// This is where we dump ErrorRecord's accumulated to help system error pool
            /// together with some context information.
                if (_helpTracer.HelpSystem.VerboseHelpErrors && _errors.Count > 0)
                    ErrorRecord errorRecord = new ErrorRecord(new ParentContainsErrorRecordException("Help Load Error"), "HelpLoadError", ErrorCategory.SyntaxError, null);
                    errorRecord.ErrorDetails = new ErrorDetails(typeof(HelpErrorTracer).Assembly, "HelpErrors", "HelpLoadError", _helpFile, _errors.Count);
                    _helpTracer.HelpSystem.LastErrors.Add(errorRecord);
                    foreach (ErrorRecord error in _errors)
                        _helpTracer.HelpSystem.LastErrors.Add(error);
                _helpTracer.PopFrame(this);
        internal HelpSystem HelpSystem { get; }
        internal HelpErrorTracer(HelpSystem helpSystem)
            if (helpSystem == null)
                throw PSTraceSource.NewArgumentNullException("HelpSystem");
            HelpSystem = helpSystem;
        /// This tracks all live TraceFrame objects, which forms a stack.
        private readonly List<TraceFrame> _traceFrames = new List<TraceFrame>();
        /// This is the API to use for starting a help trace scope.
        internal IDisposable Trace(string helpFile)
            TraceFrame traceFrame = new TraceFrame(this, helpFile);
            _traceFrames.Add(traceFrame);
            return traceFrame;
        /// This is the api function used for adding errorRecords to TraceFrame's error
            if (_traceFrames.Count == 0)
            TraceFrame traceFrame = _traceFrames[_traceFrames.Count - 1];
            traceFrame.TraceError(errorRecord);
            traceFrame.TraceErrors(errorRecords);
        internal void PopFrame(TraceFrame traceFrame)
            TraceFrame lastFrame = _traceFrames[_traceFrames.Count - 1];
            if (lastFrame == traceFrame)
                _traceFrames.RemoveAt(_traceFrames.Count - 1);
        /// Track whether help error tracer is turned on.
        internal bool IsOn
                return (_traceFrames.Count > 0 && this.HelpSystem.VerboseHelpErrors);

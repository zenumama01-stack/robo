using System.Management.Automation.Internal.Host;
    /// This subclass of TraceListener allows for the trace output
    /// coming from a System.Management.Automation.TraceSwitch
    /// to be passed to the Msh host's RawUI methods.
    /// This trace listener cannot be specified in the app.config file.
    /// It must be added through the add-tracelistener cmdlet.
    internal sealed class PSHostTraceListener
        : System.Diagnostics.TraceListener
        #region TraceListener constructors and disposer
        /// Initializes a new instance of the <see cref="PSHostTraceListener"/> class.
        internal PSHostTraceListener(PSCmdlet cmdlet)
            : base(string.Empty)
            if (cmdlet == null)
                throw new PSArgumentNullException(nameof(cmdlet));
                cmdlet.Host.UI is InternalHostUserInterface,
                "The internal host must be available to trace");
            _ui = cmdlet.Host.UI as InternalHostUserInterface;
        ~PSHostTraceListener()
        /// Closes the TraceListenerDialog so that it no longer receives trace output.
        /// True if the TraceListener is being disposed, false otherwise.
        #endregion TraceListener constructors and disposer
        /// Sends the given output string to the host for processing.
        /// <param name="output">
        /// The trace output to be written.
        public override void Write(string output)
                _cachedWrite.Append(output);
                // Catch and ignore all exceptions while tracing
                // We don't want tracing to bring down the process.
        private readonly StringBuilder _cachedWrite = new();
        public override void WriteLine(string output)
                _cachedWrite.Insert(0, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff "));
                _ui.WriteDebugLine(_cachedWrite.ToString());
                _cachedWrite.Remove(0, _cachedWrite.Length);
        /// The host interface to write the debug line to.
        private readonly InternalHostUserInterface _ui;

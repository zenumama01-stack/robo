    /// Defines a class which allows simple execution of commands from CLR languages.
    public class RunspaceInvoke : IDisposable
        /// Runspace on which commands are invoked.
        /// Create a RunspaceInvoke for invoking commands. This uses
        /// a runspace with default PSSnapins.
        public RunspaceInvoke()
            _runspace = RunspaceFactory.CreateRunspace();
            _runspace.Open();
                Runspace.DefaultRunspace = _runspace;
        /// Create RunspaceInvoke for invoking command in specified
        /// <remarks>Runspace must be opened state</remarks>
        public RunspaceInvoke(Runspace runspace)
                throw PSTraceSource.NewArgumentNullException("runspace");
        #region invoke
        /// Invoke the specified script.
        /// <param name="script">PowerShell script to invoke.</param>
        /// <returns>Output of invocation.</returns>
        public Collection<PSObject> Invoke(string script)
            return Invoke(script, null);
        /// Invoke the specified script and passes specified input to the script.
        /// <param name="input">Input to script.</param>
        public Collection<PSObject> Invoke(string script, IEnumerable input)
            if (_disposed == true)
                throw PSTraceSource.NewArgumentNullException("script");
            Pipeline p = _runspace.CreatePipeline(script);
            return p.Invoke(input);
        /// <param name="errors">This gets errors from script.</param>
        /// <paramref name="errors"/> is the non-terminating error stream
        /// from the command.
        public Collection<PSObject> Invoke(string script, IEnumerable input, out IList errors)
            Collection<PSObject> output = p.Invoke(input);
            // 2004/06/30-JonN was ReadAll() which was non-blocking
            errors = p.Error.NonBlockingRead();
        #endregion invoke
        /// Dispose underlying Runspace.
            if (_disposed == false)
                    _runspace.Close();

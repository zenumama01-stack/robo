    /// Facade class to provide context information to process
    /// exceptions.
    internal sealed class TerminatingErrorContext
        internal TerminatingErrorContext(PSCmdlet command)
                throw PSTraceSource.NewArgumentNullException(nameof(command));
            _command = command;
        internal void ThrowTerminatingError(ErrorRecord errorRecord)
            _command.ThrowTerminatingError(errorRecord);
        private readonly PSCmdlet _command;
    /// Helper class to invoke a command in a secondary pipeline.
    /// NOTE: this implementation does not return any error messages
    /// that invoked pipelines might generate.
    internal sealed class CommandWrapper : IDisposable
        /// Initialize the command before executing.
        /// <param name="execContext">ExecutionContext used to create sub pipeline.</param>
        /// <param name="nameOfCommand">Name of the command to run.</param>
        /// <param name="typeOfCommand">Type of the command to run.</param>
        internal void Initialize(ExecutionContext execContext, string nameOfCommand, Type typeOfCommand)
            _context = execContext;
            _commandName = nameOfCommand;
            _commandType = typeOfCommand;
        /// Add a parameter to the command invocation.
        /// It needs to be called before any execution takes place.
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="parameterValue">Value of the parameter.</param>
        internal void AddNamedParameter(string parameterName, object parameterValue)
            _commandParameterList.Add(
                CommandParameterInternal.CreateParameterWithArgument(
                    /*parameterAst*/null, parameterName, null,
                    /*argumentAst*/null, parameterValue,
                    false));
        /// Send an object to the pipeline.
        /// <param name="o">Object to process.</param>
        /// <returns>Array of objects out of the success pipeline.</returns>
        internal Array Process(object o)
            if (_pp == null)
                // if this is the first call, we need to initialize the
                // pipeline underneath
                DelayedInternalInitialize();
            return _pp.Step(o);
        /// Shut down the pipeline.
        internal Array ShutDown()
                // if Process() never got called, no sub pipeline
                // ever got created, hence we just return an empty array
                return Array.Empty<object>();
            PipelineProcessor ppTemp = _pp;
            _pp = null;
            return ppTemp.SynchronousExecuteEnumerate(AutomationNull.Value);
        private void DelayedInternalInitialize()
            _pp = new PipelineProcessor();
            CmdletInfo cmdletInfo = new CmdletInfo(_commandName, _commandType, null, null, _context);
            CommandProcessor cp = new CommandProcessor(cmdletInfo, _context);
            foreach (CommandParameterInternal par in _commandParameterList)
                cp.AddParameter(par);
            _pp.Add(cp);
        /// Just dispose the pipeline processor.
            _pp.Dispose();
        private PipelineProcessor _pp = null;
        private string _commandName = null;
        private Type _commandType;
        private readonly List<CommandParameterInternal> _commandParameterList = new List<CommandParameterInternal>();
        private ExecutionContext _context = null;
    /// Base class for the command-let's we expose
    /// it contains a reference to the implementation
    /// class it wraps.
    public abstract class FrontEndCommandBase : PSCmdlet, IDisposable
        /// Hook up the calls from the implementation object
        /// and then call the implementation's Begin()
            Diagnostics.Assert(this.implementation != null, "this.implementation is null");
            this.implementation.OuterCmdletCall = new ImplementationCommandBase.OuterCmdletCallback(this.OuterCmdletCall);
            this.implementation.InputObjectCall = new ImplementationCommandBase.InputObjectCallback(this.InputObjectCall);
            this.implementation.WriteObjectCall = new ImplementationCommandBase.WriteObjectCallback(this.WriteObjectCall);
            this.implementation.CreateTerminatingErrorContext();
            implementation.BeginProcessing();
        /// Call the implementation.
            implementation.ProcessRecord();
            implementation.EndProcessing();
            implementation.StopProcessing();
        /// Callback for the implementation to obtain a reference to the Cmdlet object.
        /// <returns>Cmdlet reference.</returns>
        protected virtual PSCmdlet OuterCmdletCall()
        /// Callback for the implementation to get the current pipeline object.
        /// <returns>Current object from the pipeline.</returns>
        protected virtual PSObject InputObjectCall()
            // just bind to the input object parameter
            return this.InputObject;
        /// Callback for the implementation to write objects.
        /// <param name="value">Object to be written.</param>
        protected virtual void WriteObjectCall(object value)
            // just call Monad API
            this.WriteObject(value);
        /// Reference to the implementation command that this class
        /// is wrapping.
        internal ImplementationCommandBase implementation = null;
        #region IDisposable Implementation
        /// Default implementation just delegates to internal helper.
        /// <remarks>This method calls GC.SuppressFinalize</remarks>
        /// Dispose pattern implementation.
                InternalDispose();
        /// Do-nothing implementation: derived classes will override as see fit.
        protected virtual void InternalDispose()
            if (this.implementation == null)
            this.implementation.Dispose();
            this.implementation = null;
    /// Implementation class to be called by the outer command
    /// In order to properly work, the callbacks have to be properly set by the outer command.
    internal class ImplementationCommandBase : IDisposable
        /// Inner version of CommandBase.BeginProcessing()
        internal virtual void BeginProcessing()
        /// Inner version of CommandBase.ProcessRecord()
        internal virtual void ProcessRecord()
        /// Inner version of CommandBase.EndProcessing()
        internal virtual void EndProcessing()
        /// Inner version of CommandBase.StopProcessing()
        internal virtual void StopProcessing()
        /// Retrieve the current input pipeline object.
        internal virtual PSObject ReadObject()
            // delegate to the front end object
            System.Diagnostics.Debug.Assert(this.InputObjectCall != null, "this.InputObjectCall is null");
            return this.InputObjectCall();
        /// Write an object to the pipeline.
        /// <param name="o">Object to write to the pipeline.</param>
        internal virtual void WriteObject(object o)
            System.Diagnostics.Debug.Assert(this.WriteObjectCall != null, "this.WriteObjectCall is null");
            this.WriteObjectCall(o);
        // callback methods to get to the outer Monad Cmdlet
        /// Get a hold of the Monad outer Cmdlet.
        internal virtual PSCmdlet OuterCmdlet()
            System.Diagnostics.Debug.Assert(this.OuterCmdletCall != null, "this.OuterCmdletCall is null");
            return this.OuterCmdletCall();
        protected TerminatingErrorContext TerminatingErrorContext { get; private set; }
        internal void CreateTerminatingErrorContext()
            TerminatingErrorContext = new TerminatingErrorContext(this.OuterCmdlet());
        /// Delegate definition to get to the outer command-let.
        internal delegate PSCmdlet OuterCmdletCallback();
        /// Callback to get to the outer command-let.
        internal OuterCmdletCallback OuterCmdletCall;
        // callback to the methods to get an object and write an object
        /// Delegate definition to get to the current pipeline input object.
        internal delegate PSObject InputObjectCallback();
        /// Delegate definition to write object.
        internal delegate void WriteObjectCallback(object o);
        /// Callback to read object.
        internal InputObjectCallback InputObjectCall;
        /// Callback to write object.
        internal WriteObjectCallback WriteObjectCall;

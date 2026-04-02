using System.Management.Automation.Runspaces.Internal;
    /// Defines exception which is thrown when state of the PowerShell is different
    /// from the expected state.
    public class InvalidPowerShellStateException : SystemException
        /// Creates a new instance of InvalidPowershellStateException class.
        public InvalidPowerShellStateException()
        (StringUtil.Format(PowerShellStrings.InvalidPowerShellStateGeneral))
        public InvalidPowerShellStateException(string message)
        public InvalidPowerShellStateException(string message, Exception innerException)
        /// Initializes a new instance of the InvalidPowerShellStateException and defines value of
        /// CurrentState.
        /// <param name="currentState">Current state of powershell.</param>
        internal InvalidPowerShellStateException(PSInvocationState currentState)
            _currState = currentState;
        // No need to implement GetObjectData
        /// Initializes a new instance of the InvalidPowerShellStateException
        /// class with serialized data.
        InvalidPowerShellStateException(SerializationInfo info, StreamingContext context)
        /// Gets CurrentState of the powershell.
        public PSInvocationState CurrentState
                return _currState;
        /// State of powershell when exception was thrown.
        private readonly PSInvocationState _currState = 0;
    #region PSInvocationState, PSInvocationStateInfo, PSInvocationStateChangedEventArgs
    /// Enumerated type defining the state of the PowerShell.
    public enum PSInvocationState
        /// PowerShell has not been started.
        /// PowerShell is executing.
        /// PowerShell is stoping execution.
        /// PowerShell is completed due to a stop request.
        /// PowerShell has completed executing a command.
        /// PowerShell completed abnormally due to an error.
        /// PowerShell is in disconnected state.
    /// Enumerated type defining runspace modes for nested pipeline.
    public enum RunspaceMode
        /// Use current runspace from the current thread of execution.
        CurrentRunspace = 0,
        /// Create new runspace.
        NewRunspace = 1
    /// Type which has information about InvocationState and Exception
    /// associated with InvocationState.
    public sealed class PSInvocationStateInfo
        internal PSInvocationStateInfo(PSInvocationState state, Exception reason)
            _executionState = state;
            _exceptionReason = reason;
        /// Construct from PipelineStateInfo.
        /// <param name="pipelineStateInfo"></param>
        internal PSInvocationStateInfo(PipelineStateInfo pipelineStateInfo)
            _executionState = (PSInvocationState)((int)pipelineStateInfo.State);
            _exceptionReason = pipelineStateInfo.Reason;
        /// The state of the PowerShell instance.
                return _executionState;
        public Exception Reason
                return _exceptionReason;
        /// Clone the current instance.
        /// A copy of the current instance.
        internal PSInvocationStateInfo Clone()
            return new PSInvocationStateInfo(
                _executionState,
                _exceptionReason
        /// The current execution state.
        private readonly PSInvocationState _executionState;
        /// Non-null exception if the execution state change was due to an error.
        private readonly Exception _exceptionReason;
    /// Event arguments passed to PowerShell state change handlers
    /// <see cref="PowerShell.InvocationStateChanged"/> event.
    public sealed class PSInvocationStateChangedEventArgs : EventArgs
        /// Constructs PSInvocationStateChangedEventArgs from PSInvocationStateInfo.
        /// <param name="psStateInfo">
        /// state to raise the event with.
        internal PSInvocationStateChangedEventArgs(PSInvocationStateInfo psStateInfo)
            Dbg.Assert(psStateInfo != null, "caller should validate the parameter");
            InvocationStateInfo = psStateInfo;
        /// Information about current state of a PowerShell Instance.
        public PSInvocationStateInfo InvocationStateInfo { get; }
    /// Settings to control command invocation.
    public sealed class PSInvocationSettings
        private PSHost _host;
        // the following are used to flow the identity to pipeline execution thread
        // Invokes a remote command and immediately disconnects, if transport layer
        // supports this operation.
        public PSInvocationSettings()
            this.ApartmentState = ApartmentState.Unknown;
            _host = null;
            RemoteStreamOptions = 0;
            AddToHistory = false;
            ErrorActionPreference = null;
        /// ApartmentState of the thread in which the command
        public ApartmentState ApartmentState { get; set; }
        /// Host to use with the Runspace when the command is
        /// executed.
                    throw PSTraceSource.NewArgumentNullException("Host");
                _host = value;
        /// Options for the Error, Warning, Verbose and Debug streams during remote calls.
        public RemoteStreamOptions RemoteStreamOptions { get; set; }
        /// Boolean which tells if the command is added to the history of the
        /// Runspace the command is executing in. By default this is false.
        public bool AddToHistory { get; set; }
        /// Determines how errors should be handled during batch command execution.
        public ActionPreference? ErrorActionPreference { get; set; }
        /// Used by Powershell remoting infrastructure to flow identity from calling thread to
        /// Pipeline Execution Thread.
        /// Scenario: In the IIS hosting model, the calling thread is impersonated with a different
        /// identity than the process identity. However Pipeline Execution Thread always inherits
        /// process's identity and this will create problems related to security. In the IIS hosting
        /// model, we should honor calling threads identity.
        public bool FlowImpersonationPolicy { get; set; }
        internal System.Security.Principal.WindowsIdentity WindowsIdentityToImpersonate { get; set; }
        /// When true, allows an unhandled flow control exceptions to
        /// propagate to a caller invoking the PowerShell object.
        public bool ExposeFlowControlExceptions
        /// Invokes a remote command and immediately disconnects, if the transport
        /// layer supports this operation.
        internal bool InvokeAndDisconnect { get; set; }
    /// Batch execution context.
    internal class BatchInvocationContext
        private readonly AutoResetEvent _completionEvent;
        internal BatchInvocationContext(PSCommand command, PSDataCollection<PSObject> output)
            Output = output;
            _completionEvent = new AutoResetEvent(false);
        /// Invocation output.
        internal PSDataCollection<PSObject> Output { get; }
        /// Command to invoke.
        internal PSCommand Command { get; }
        /// Waits for the completion event.
            _completionEvent.WaitOne();
        /// Signals the completion event.
        internal void Signal()
            _completionEvent.Set();
    /// These flags control whether InvocationInfo is added to items in the Error, Warning, Verbose and Debug
    /// streams during remote calls.
    public enum RemoteStreamOptions
        /// If this flag is set, ErrorRecord will include an instance of InvocationInfo on remote calls.
        AddInvocationInfoToErrorRecord = 0x01,
        /// If this flag is set, WarningRecord will include an instance of InvocationInfo on remote calls.
        AddInvocationInfoToWarningRecord = 0x02,
        /// If this flag is set, DebugRecord will include an instance of InvocationInfo on remote calls.
        AddInvocationInfoToDebugRecord = 0x04,
        /// If this flag is set, VerboseRecord will include an instance of InvocationInfo on remote calls.
        AddInvocationInfoToVerboseRecord = 0x08,
        /// If this flag is set, ErrorRecord, WarningRecord, DebugRecord, and VerboseRecord will include an instance of InvocationInfo on remote calls.
        AddInvocationInfo = AddInvocationInfoToErrorRecord
                          | AddInvocationInfoToWarningRecord
                          | AddInvocationInfoToDebugRecord
                          | AddInvocationInfoToVerboseRecord
    #region PowerShell AsyncResult
    /// Internal Async result type used by BeginInvoke() and BeginStop() overloads.
    internal sealed class PowerShellAsyncResult : AsyncResult
        #region Private Data / Properties
        // used to track if this AsyncResult is created by a BeginInvoke operation or
        // a BeginStop operation.
        /// True if AsyncResult monitors Async BeginInvoke().
        internal bool IsAssociatedWithAsyncInvoke { get; }
        /// The output buffer for the asynchronous invoke.
        /// Instance Id of the Powershell object creating this instance
        /// Callback to call when the async operation completes.
        /// A user supplied state to call the "callback" with.
        /// The output buffer to return from EndInvoke.
        /// <param name="isCalledFromBeginInvoke">
        /// true if AsyncResult monitors BeginInvoke.
        internal PowerShellAsyncResult(Guid ownerId, AsyncCallback callback, object state, PSDataCollection<PSObject> output,
            bool isCalledFromBeginInvoke)
            : base(ownerId, callback, state)
            IsAssociatedWithAsyncInvoke = isCalledFromBeginInvoke;
    /// Represents a PowerShell command or script to execute against a
    /// Runspace(Pool) if provided, otherwise execute using a default
    /// Runspace. Provides access to different result buffers
    /// like output, error, debug, verbose, progress, warning, and information.
    /// Provides a simple interface to execute a powershell command:
    ///    Powershell.Create().AddScript("get-process").Invoke();
    /// The above statement creates a local runspace using default
    /// configuration, executes the command and then closes the runspace.
    /// Using RunspacePool property, the caller can provide the runspace
    /// where the command / script is executed.
    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "PowerShell is a valid type in SMAR namespace.")]
    public sealed class PowerShell : IDisposable
        private PSCommand _psCommand;
        // worker object which does the invoke
        private Worker _worker;
        private PowerShellAsyncResult _invokeAsyncResult;
        private PowerShellAsyncResult _stopAsyncResult;
        private PowerShellAsyncResult _batchAsyncResult;
        private PSInvocationSettings _batchInvocationSettings;
        private PSCommand _backupPSCommand;
        private object _rsConnection;
        private PSDataCollection<ErrorRecord> _errorBuffer;
        // client remote powershell if the powershell
        // is executed with a remote runspace pool
        private ConnectCommandInfo _connectCmdInfo;
        private bool _commandInvokedSynchronously = false;
        private bool _isBatching = false;
        private bool _stopBatchExecution = false;
        // Delegates for asynchronous invocation/termination of PowerShell commands
        private readonly Func<IAsyncResult, PSDataCollection<PSObject>> _endInvokeMethod;
        private readonly Action<IAsyncResult> _endStopMethod;
        #region Internal Constructors
        /// Constructs PowerShell.
        /// A PSCommand.
        /// <param name="extraCommands">
        /// A list of extra commands to run
        /// <param name="rsConnection">
        /// A Runspace or RunspacePool to refer while invoking the command.
        /// This can be null in which case a new runspace is created
        /// whenever Invoke* method is called.
        private PowerShell(PSCommand command, Collection<PSCommand> extraCommands, object rsConnection)
            Dbg.Assert(command != null, "command must not be null");
            ExtraCommands = extraCommands ?? new Collection<PSCommand>();
            RunningExtraCommands = false;
            _psCommand = command;
            _psCommand.Owner = this;
            RemoteRunspace remoteRunspace = rsConnection as RemoteRunspace;
            _rsConnection = remoteRunspace != null ? remoteRunspace.RunspacePool : rsConnection;
            InvocationStateInfo = new PSInvocationStateInfo(PSInvocationState.NotStarted, null);
            OutputBuffer = null;
            OutputBufferOwner = true;
            _errorBuffer = new PSDataCollection<ErrorRecord>();
            ErrorBufferOwner = true;
            InformationalBuffers = new PSInformationalBuffers(InstanceId);
            Streams = new PSDataStreams(this);
            _endInvokeMethod = EndInvoke;
            _endStopMethod = EndStop;
            ApplicationInsightsTelemetry.SendTelemetryMetric(TelemetryType.PowerShellCreate, "create");
        /// Constructs a PowerShell instance in the disconnected start state with
        /// the provided remote command connect information and runspace(pool) objects.
        /// <param name="connectCmdInfo">Remote command connect information.</param>
        /// <param name="rsConnection">Remote Runspace or RunspacePool object.</param>
        internal PowerShell(ConnectCommandInfo connectCmdInfo, object rsConnection)
            : this(new PSCommand(), null, rsConnection)
            ExtraCommands = new Collection<PSCommand>();
            AddCommand(connectCmdInfo.Command);
            _connectCmdInfo = connectCmdInfo;
            // The command ID is passed to the PSRP layer through the PowerShell instanceID.
            InstanceId = _connectCmdInfo.CommandId;
            InvocationStateInfo = new PSInvocationStateInfo(PSInvocationState.Disconnected, null);
            if (rsConnection is RemoteRunspace)
                _runspace = rsConnection as Runspace;
                _runspacePool = ((RemoteRunspace)rsConnection).RunspacePool;
            else if (rsConnection is RunspacePool)
                _runspacePool = (RunspacePool)rsConnection;
            Dbg.Assert(_runspacePool != null, "Invalid rsConnection parameter>");
            RemotePowerShell = new ClientRemotePowerShell(this, _runspacePool.RemoteRunspacePoolInternal);
        /// <param name="inputstream"></param>
        /// <param name="outputstream"></param>
        /// <param name="errorstream"></param>
        /// <param name="runspacePool"></param>
        internal PowerShell(ObjectStreamBase inputstream,
            ObjectStreamBase outputstream, ObjectStreamBase errorstream, RunspacePool runspacePool)
            _rsConnection = runspacePool;
            PSDataCollectionStream<PSObject> outputdatastream = (PSDataCollectionStream<PSObject>)outputstream;
            OutputBuffer = outputdatastream.ObjectStore;
            PSDataCollectionStream<ErrorRecord> errordatastream = (PSDataCollectionStream<ErrorRecord>)errorstream;
            _errorBuffer = errordatastream.ObjectStore;
            if (runspacePool != null && runspacePool.RemoteRunspacePoolInternal != null)
                RemotePowerShell = new ClientRemotePowerShell(this, runspacePool.RemoteRunspacePoolInternal);
        /// Creates a PowerShell object in the disconnected start state and with a ConnectCommandInfo object
        /// parameter that specifies what remote command to associate with this PowerShell when it is connected.
        /// <param name="connectCmdInfo"></param>
        internal PowerShell(ConnectCommandInfo connectCmdInfo, ObjectStreamBase inputstream, ObjectStreamBase outputstream,
            ObjectStreamBase errorstream, RunspacePool runspacePool)
            : this(inputstream, outputstream, errorstream, runspacePool)
            _psCommand = new PSCommand();
            _runspacePool = runspacePool;
        /// Sets the command collection in this powershell.
        /// <remarks>This method will be called by RemotePipeline
        /// before it begins execution. This method is used to set
        /// the command collection of the remote pipeline as the
        /// command collection of the underlying powershell</remarks>
        internal void InitForRemotePipeline(CommandCollection command, ObjectStreamBase inputstream,
            ObjectStreamBase outputstream, ObjectStreamBase errorstream, PSInvocationSettings settings, bool redirectShellErrorOutputPipe)
            Dbg.Assert(command != null, "A command collection need to be specified");
            _psCommand = new PSCommand(command[0]);
            for (int i = 1; i < command.Count; i++)
                AddCommand(command[i]);
            RedirectShellErrorOutputPipe = redirectShellErrorOutputPipe;
            // create the client remote powershell for remoting
            // communications
            RemotePowerShell ??= new ClientRemotePowerShell(this, ((RunspacePool)_rsConnection).RemoteRunspacePoolInternal);
            // If we get here, we don't call 'Invoke' or any of it's friends on 'this', instead we serialize 'this' in PowerShell.ToPSObjectForRemoting.
            // Without the following two steps, we'll be missing the 'ExtraCommands' on the serialized instance of 'this'.
            // This is the last possible chance to call set up for batching as we will indirectly call ToPSObjectForRemoting
            // in the call to ClientRemotePowerShell.Initialize (which happens just below.)
            DetermineIsBatching();
            if (_isBatching)
                SetupAsyncBatchExecution();
            RemotePowerShell.Initialize(inputstream, outputstream,
                errorstream, InformationalBuffers, settings);
        /// Initialize PowerShell object for connection to remote command.
        /// <param name="inputstream">Input stream.</param>
        /// <param name="outputstream">Output stream.</param>
        /// <param name="errorstream">Error stream.</param>
        /// <param name="settings">Settings information.</param>
        /// <param name="redirectShellErrorOutputPipe">Redirect error output.</param>
        internal void InitForRemotePipelineConnect(ObjectStreamBase inputstream, ObjectStreamBase outputstream,
            ObjectStreamBase errorstream, PSInvocationSettings settings, bool redirectShellErrorOutputPipe)
            // The remotePowerShell and DSHandler cannot be initialized with a disconnected runspace.
            // Make sure the associated runspace is valid and connected.
            CheckRunspacePoolAndConnect();
            if (InvocationStateInfo.State != PSInvocationState.Disconnected)
                throw new InvalidPowerShellStateException(InvocationStateInfo.State);
            if (!RemotePowerShell.Initialized)
                RemotePowerShell.Initialize(inputstream, outputstream, errorstream, InformationalBuffers, settings);
        #region Construction Factory
        /// Constructs an empty PowerShell instance; a script or command must be added before invoking this instance.
        /// An instance of PowerShell.
        public static PowerShell Create()
            return new PowerShell(new PSCommand(), null, null);
        /// <param name="runspace">Runspace mode.</param>
        /// <returns>An instance of PowerShell.</returns>
        public static PowerShell Create(RunspaceMode runspace)
            PowerShell result = null;
            switch (runspace)
                case RunspaceMode.CurrentRunspace:
                        throw new InvalidOperationException(PowerShellStrings.NoDefaultRunspaceForPSCreate);
                    result = new PowerShell(new PSCommand(), null, Runspace.DefaultRunspace);
                    result.IsChild = true;
                    result.IsNested = true;
                    result.IsRunspaceOwner = false;
                    result._runspace = Runspace.DefaultRunspace;
                case RunspaceMode.NewRunspace:
                    result = new PowerShell(new PSCommand(), null, null);
        /// <param name="initialSessionState">InitialSessionState with which to create the runspace.</param>
        public static PowerShell Create(InitialSessionState initialSessionState)
            PowerShell result = Create();
            result.Runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            result.Runspace.Open();
        /// Constructs an empty PowerShell instance and associates it with the provided
        /// Runspace; a script or command must be added before invoking this instance.
        /// <param name="runspace">Runspace in which to invoke commands.</param>
        /// The required Runspace argument is accepted no matter what state it is in.
        /// Leaving Runspace state management to the caller allows them to open their
        /// runspace in whatever manner is most appropriate for their application
        /// (in another thread while this instance of the PowerShell class is being
        /// instantiated, for example).
        public static PowerShell Create(Runspace runspace)
            result.Runspace = runspace;
        /// Creates a nested powershell within the current instance.
        /// Nested PowerShell is used to do simple operations like checking state
        /// of a variable while another command is using the runspace.
        /// Nested PowerShell should be invoked from the same thread as the parent
        /// PowerShell invocation thread. So effectively the parent Powershell
        /// invocation thread is blocked until nested invoke() operation is
        /// complete.
        /// Implement PSHost.EnterNestedPrompt to perform invoke() operation on the
        /// nested powershell.
        /// 1. State of powershell instance is not valid to create a nested powershell instance.
        /// Nested PowerShell should be created only for a running powershell instance.
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "ps", Justification = "ps represents PowerShell and is used at many places.")]
        public PowerShell CreateNestedPowerShell()
            if ((_worker != null) && (_worker.CurrentlyRunningPipeline != null))
                PowerShell result = new PowerShell(new PSCommand(),
                    null, _worker.CurrentlyRunningPipeline.Runspace);
            throw PSTraceSource.NewInvalidOperationException(PowerShellStrings.InvalidStateCreateNested);
        /// Method needed when deserializing PowerShell object coming from a RemoteDataObject.
        /// <param name="isNested">Indicates if PowerShell object is nested.</param>
        /// <param name="psCommand">Commands that the PowerShell pipeline is built of.</param>
        /// <param name="extraCommands">Extra commands to run.</param>
        private static PowerShell Create(bool isNested, PSCommand psCommand, Collection<PSCommand> extraCommands)
            PowerShell powerShell = new PowerShell(psCommand, extraCommands, null);
            powerShell.IsNested = isNested;
        /// For example, to construct a command string "Get-Process | Sort-Object",
        ///         PowerShell shell = PowerShell.Create()
        ///             .AddCommand("Get-Process")
        ///             .AddCommand("Sort-Object");
        /// A PowerShell instance with <paramref name="cmdlet"/> added.
        /// Object is disposed.
        public PowerShell AddCommand(string cmdlet)
                AssertChangesAreAccepted();
                _psCommand.AddCommand(cmdlet);
        ///             .AddCommand("Get-Process", true)
        ///             .AddCommand("Sort-Object", true);
        public PowerShell AddCommand(string cmdlet, bool useLocalScope)
                _psCommand.AddCommand(cmdlet, useLocalScope);
        /// For example, to construct a command string "Get-Process | ForEach-Object { $_.Name }"
        ///             .AddScript("Get-Process | ForEach-Object { $_.Name }");
        /// A string representing a script.
        /// A PowerShell instance with <paramref name="command"/> added.
        public PowerShell AddScript(string script)
                _psCommand.AddScript(script);
        ///             .AddScript("Get-Process | ForEach-Object { $_.Name }", true);
        public PowerShell AddScript(string script, bool useLocalScope)
                _psCommand.AddScript(script, useLocalScope);
        internal PowerShell AddCommand(Command command)
                _psCommand.AddCommand(command);
        /// CommandInfo object for the command to add.
        /// <param name="commandInfo">The CommandInfo object for the command to add.</param>
        /// A PSCommand instance with the command added.
        public PowerShell AddCommand(CommandInfo commandInfo)
            Command cmd = new Command(commandInfo);
            _psCommand.AddCommand(cmd);
        /// For example, to construct a command string "Get-Process | Select-Object -Property Name"
        ///             .AddCommand("Select-Object").AddParameter("Property", "Name");
        /// A PowerShell instance with <paramref name="parameterName"/> added
        public PowerShell AddParameter(string parameterName, object value)
                if (_psCommand.Commands.Count == 0)
                    throw PSTraceSource.NewInvalidOperationException(PowerShellStrings.ParameterRequiresCommand);
                _psCommand.AddParameter(parameterName, value);
        ///         PSCommand command = new PSCommand("get-process").
        ///                                     AddCommand("sort-object").AddParameter("descending");
        public PowerShell AddParameter(string parameterName)
                _psCommand.AddParameter(parameterName);
        internal PowerShell AddParameter(CommandParameter parameter)
                _psCommand.AddParameter(parameter);
        /// Adds a set of parameters to the last added command.
        /// List of parameters.
        /// <exception cref="PSArgumentNullException">
        /// The function was given a null argument.
        public PowerShell AddParameters(IList parameters)
                foreach (object p in parameters)
                    _psCommand.AddParameter(null, p);
        /// Dictionary of parameters. Each key-value pair corresponds to a parameter name and its value. Keys must strings.
        /// One of the dictionary keys is not a string.
        public PowerShell AddParameters(IDictionary parameters)
                foreach (DictionaryEntry entry in parameters)
                    if (entry.Key is not string parameterName)
                        throw PSTraceSource.NewArgumentException(nameof(parameters), PowerShellStrings.KeyMustBeString);
                    _psCommand.AddParameter(parameterName, entry.Value);
        /// For example, to construct a command string "Get-Process | Select-Object Name"
        ///             .AddCommand("Select-Object").AddArgument("Name");
        public PowerShell AddArgument(object value)
                _psCommand.AddArgument(value);
        public PowerShell AddStatement()
                // for PowerShell.Create().AddStatement().AddCommand("Get-Process");
                // we reduce it to PowerShell.Create().AddCommand("Get-Process");
                _psCommand.Commands[_psCommand.Commands.Count - 1].IsEndOfStatement = true;
        /// Gets or sets current powershell command line.
        /// Cannot set to a null value.
        public PSCommand Commands
                return _psCommand;
                    throw PSTraceSource.NewArgumentNullException("Command");
                    _psCommand = value.Clone();
        /// Streams generated by PowerShell invocations.
        public PSDataStreams Streams { get; }
        /// Gets or sets the error buffer. Powershell invocation writes
        /// the error data into this buffer.
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "We want to allow callers to change the backing store.")]
        internal PSDataCollection<ErrorRecord> ErrorBuffer
                return _errorBuffer;
                    throw PSTraceSource.NewArgumentNullException("Error");
                    _errorBuffer = value;
                    ErrorBufferOwner = false;
        /// Gets or sets the progress buffer. Powershell invocation writes
        /// the progress data into this buffer. Can be null.
        internal PSDataCollection<ProgressRecord> ProgressBuffer
                return InformationalBuffers.Progress;
                    throw PSTraceSource.NewArgumentNullException("Progress");
                    InformationalBuffers.Progress = value;
        /// Gets or sets the verbose buffer. Powershell invocation writes
        /// the verbose data into this buffer.  Can be null.
        internal PSDataCollection<VerboseRecord> VerboseBuffer
                return InformationalBuffers.Verbose;
                    throw PSTraceSource.NewArgumentNullException("Verbose");
                    InformationalBuffers.Verbose = value;
        /// Gets or sets the debug buffer. Powershell invocation writes
        /// the debug data into this buffer.  Can be null.
        internal PSDataCollection<DebugRecord> DebugBuffer
                return InformationalBuffers.Debug;
                    throw PSTraceSource.NewArgumentNullException("Debug");
                    InformationalBuffers.Debug = value;
        /// Gets or sets the warning buffer. Powershell invocation writes
        /// the warning data into this buffer. Can be null.
        internal PSDataCollection<WarningRecord> WarningBuffer
                return InformationalBuffers.Warning;
                    throw PSTraceSource.NewArgumentNullException("Warning");
                    InformationalBuffers.Warning = value;
        /// Gets or sets the information buffer. Powershell invocation writes
        /// the information data into this buffer. Can be null.
        internal PSDataCollection<InformationRecord> InformationBuffer
                return InformationalBuffers.Information;
                    throw PSTraceSource.NewArgumentNullException("Information");
                    InformationalBuffers.Information = value;
        /// Gets the informational buffers.
        internal PSInformationalBuffers InformationalBuffers { get; }
        /// (see the comment in Pipeline.RedirectShellErrorOutputPipe for an
        /// explanation of why this flag is needed)
        internal bool RedirectShellErrorOutputPipe { get; set; } = true;
        /// Get unique id for this instance of runspace pool. It is primarily used
        public Guid InstanceId { get; private set; }
        /// Gets the execution state of the current PowerShell instance.
        public PSInvocationStateInfo InvocationStateInfo { get; private set; }
        /// Gets the property which indicates if this PowerShell instance
        /// is nested.
        public bool IsNested { get; private set; }
        /// is a child instance.
        internal bool IsChild { get; private set; } = false;
        /// If an error occurred while executing the pipeline, this will be set to true.
        public bool HadErrors { get; private set; }
            HadErrors = status;
        /// Access to the EndInvoke AsyncResult object.  Used by remote
        /// debugging to invoke debugger commands on command thread.
        internal AsyncResult EndInvokeAsyncResult
        /// Event raised when PowerShell Execution State Changes.
        public event EventHandler<PSInvocationStateChangedEventArgs> InvocationStateChanged;
        /// This event gets fired when a Runspace from the RunspacePool is assigned to this PowerShell
        /// instance to invoke the commands.
        internal event EventHandler<PSEventArgs<Runspace>> RunspaceAssigned;
        /// Sets an associated Runspace for this PowerShell instance.
        /// This property and RunspacePool are mutually exclusive; setting one of them resets the other to null
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Runspace", Justification = "Runspace is a well-known term in PowerShell.")]
                if (_runspace == null && _runspacePool == null) // create a runspace only if neither a runspace nor a runspace pool have been set
                        if (_runspace == null && _runspacePool == null)
                            SetRunspace(RunspaceFactory.CreateRunspace(), true);
                            this.Runspace.Open();
                return _runspace;
                    if (_runspace != null && IsRunspaceOwner)
                        _runspace.Dispose();
                        IsRunspaceOwner = false;
                    SetRunspace(value, false);
        /// Internal method to set the Runspace property.
        private void SetRunspace(Runspace runspace, bool owner)
            RemoteRunspace remoteRunspace = runspace as RemoteRunspace;
            if (remoteRunspace == null)
                _rsConnection = runspace;
                _rsConnection = remoteRunspace.RunspacePool;
                if (RemotePowerShell != null)
                    RemotePowerShell.Clear();
                    RemotePowerShell.Dispose();
                RemotePowerShell = new ClientRemotePowerShell(this, remoteRunspace.RunspacePool.RemoteRunspacePoolInternal);
            IsRunspaceOwner = owner;
            _runspacePool = null;
        private Runspace _runspace = null;
        /// Sets an associated RunspacePool for this PowerShell instance.
        /// A Runspace from this pool is used whenever Invoke* method
        /// is called.
        /// This property and Runspace are mutually exclusive; setting one of them resets the other to null
        public RunspacePool RunspacePool
                return _runspacePool;
                        _rsConnection = value;
                        _runspacePool = value;
                        if (_runspacePool.IsRemote)
                            RemotePowerShell = new
                                ClientRemotePowerShell(this, _runspacePool.RemoteRunspacePoolInternal);
        private RunspacePool _runspacePool = null;
        /// Gets the associated Runspace or RunspacePool for this PowerShell
        /// instance. If this is null, PowerShell instance is not associated
        /// with any runspace.
        internal object GetRunspaceConnection()
            return _rsConnection;
        #region Connect Support
        /// Synchronously connects to a running command on a remote server.
        /// <returns>Command output as a PSDataCollection.</returns>
        public Collection<PSObject> Connect()
            // Regardless of how the command was originally invoked, set this member
            // variable to indicate the command is now running synchronously.
            _commandInvokedSynchronously = true;
            IAsyncResult asyncResult = ConnectAsync();
            PowerShellAsyncResult psAsyncResult = asyncResult as PowerShellAsyncResult;
            // Wait for command to complete.  If an exception was thrown during command
            // execution (such as disconnect occurring during this synchronous execution)
            // then the exception will be thrown here.
            EndInvokeAsyncResult = psAsyncResult;
            psAsyncResult.EndInvoke();
            EndInvokeAsyncResult = null;
            if (psAsyncResult.Output != null)
                results = psAsyncResult.Output.ReadAll();
                // Return empty collection.
                results = new Collection<PSObject>();
        /// Asynchronously connects to a running command on a remote server.
        /// The returned IAsyncResult object can be used with EndInvoke() method
        /// to wait on command and/or get command returned data.
        /// <returns>IAsyncResult.</returns>
        public IAsyncResult ConnectAsync()
            return ConnectAsync(null, null, null);
        /// <param name="output">The output buffer to return from EndInvoke.</param>
        /// <param name="invocationCallback">An AsyncCallback to be called once the previous invocation has completed.</param>
        /// <param name="state">A user supplied state to call the <paramref name="invocationCallback"/> with.</param>
        public IAsyncResult ConnectAsync(
            PSDataCollection<PSObject> output,
            AsyncCallback invocationCallback,
            // Ensure this is a command invoked on a remote runspace(pool) and connect the
            // runspace if it is currently disconnected.
            if (_connectCmdInfo != null)
                // This is a reconstruct/connect scenario and we create new state.
                PSDataCollection<PSObject> streamToUse = OutputBuffer;
                // The remotePowerShell may have been initialized by InitForRemotePipelineConnect()
                    // Empty input stream.
                    ObjectStreamBase inputStream = new ObjectStream();
                    inputStream.Close();
                    // Output stream.
                        // Use the supplied output buffer.
                        OutputBuffer = output;
                        OutputBufferOwner = false;
                    else if (OutputBuffer == null)
                        OutputBuffer = new PSDataCollection<PSObject>();
                    streamToUse = OutputBuffer;
                    ObjectStreamBase outputStream = new PSDataCollectionStream<PSObject>(InstanceId, streamToUse);
                    RemotePowerShell.Initialize(inputStream, outputStream,
                                                     new PSDataCollectionStream<ErrorRecord>(InstanceId, _errorBuffer),
                                                     InformationalBuffers, null);
                Dbg.Assert((_invokeAsyncResult == null), "Async result should be null in the reconstruct scenario.");
                _invokeAsyncResult = new PowerShellAsyncResult(InstanceId, invocationCallback, state, streamToUse, true);
                // If this is not a reconstruct scenario then this must be a PowerShell object that was
                // previously disconnected, and all state should be valid.
                Dbg.Assert((_invokeAsyncResult != null && RemotePowerShell.Initialized),
                            "AsyncResult and RemotePowerShell objects must be valid here.");
                if (output != null ||
                    invocationCallback != null ||
                    _invokeAsyncResult.IsCompleted)
                    // A new async object is needed.
                    PSDataCollection<PSObject> streamToUse;
                        streamToUse = output;
                    else if (_invokeAsyncResult.Output == null ||
                             !_invokeAsyncResult.Output.IsOpen)
                        streamToUse = _invokeAsyncResult.Output;
                        OutputBuffer = streamToUse;
                    _invokeAsyncResult = new PowerShellAsyncResult(
                        InstanceId,
                        invocationCallback ?? _invokeAsyncResult.Callback,
                        (invocationCallback != null) ? state : _invokeAsyncResult.AsyncState,
                        streamToUse,
                // Perform the connect operation to the remote server through the PSRP layer.
                // If this.connectCmdInfo is null then a connection will be attempted using current state.
                // If this.connectCmdInfo is non-null then a connection will be attempted with this info.
                RemotePowerShell.ConnectAsync(_connectCmdInfo);
                // allow GC collection
                _invokeAsyncResult = null;
                SetStateChanged(new PSInvocationStateInfo(PSInvocationState.Failed, exception));
                // re-throw the exception
                InvalidRunspacePoolStateException poolException = exception as InvalidRunspacePoolStateException;
                if (poolException != null && _runspace != null) // the pool exception was actually thrown by a runspace
                    throw poolException.ToInvalidRunspaceStateException();
            return _invokeAsyncResult;
        /// Checks that the current runspace associated with this PowerShell is a remote runspace,
        /// and if it is in Disconnected state then to connect it.
        private void CheckRunspacePoolAndConnect()
            RemoteRunspacePoolInternal remoteRunspacePoolInternal = null;
            if (_rsConnection is RemoteRunspace)
                remoteRunspacePoolInternal = (_rsConnection as RemoteRunspace).RunspacePool.RemoteRunspacePoolInternal;
            else if (_rsConnection is RunspacePool)
                remoteRunspacePoolInternal = (_rsConnection as RunspacePool).RemoteRunspacePoolInternal;
            if (remoteRunspacePoolInternal == null)
                throw new InvalidOperationException(PowerShellStrings.CannotConnect);
            // Connect runspace if needed.
            if (remoteRunspacePoolInternal.RunspacePoolStateInfo.State == RunspacePoolState.Disconnected)
                remoteRunspacePoolInternal.Connect();
            // Make sure runspace is in valid state for connection.
            if (remoteRunspacePoolInternal.RunspacePoolStateInfo.State != RunspacePoolState.Opened)
                throw new InvalidRunspacePoolStateException(RunspacePoolStrings.InvalidRunspacePoolState,
                                    remoteRunspacePoolInternal.RunspacePoolStateInfo.State, RunspacePoolState.Opened);
        #region Script Debugger Support
        /// This method allows the script debugger first crack at evaluating the
        /// command in case it is a debugger command, otherwise the command is
        /// evaluated by PowerShell.
        /// If the debugger evaluated a command then DebuggerCommand.ResumeAction
        /// value will be set appropriately.
        /// <param name="input">Input.</param>
        /// <param name="settings">PS invocation settings.</param>
        /// <param name="invokeMustRun">True if PowerShell Invoke must run regardless
        /// of whether debugger handles the command.
        internal void InvokeWithDebugger(
            IEnumerable<object> input,
            IList<PSObject> output,
            PSInvocationSettings settings,
            bool invokeMustRun)
            Debugger debugger = _runspace.Debugger;
            bool addToHistory = true;
            if (debugger != null &&
                Commands.Commands.Count > 0)
                Command cmd = this.Commands.Commands[0];
                DebuggerCommand dbgCommandResult = debugger.InternalProcessCommand(
                    cmd.CommandText, output);
                if (dbgCommandResult.ResumeAction != null ||
                    dbgCommandResult.ExecutedByDebugger)
                    output.Add(new PSObject(dbgCommandResult));
                    Commands.Commands.Clear();
                    addToHistory = false;
                else if (!dbgCommandResult.Command.Equals(cmd.CommandText, StringComparison.OrdinalIgnoreCase))
                    // Script debugger will replace commands, e.g., "k" -> "Get-PSCallStack".
                    Commands.Commands[0] = new Command(dbgCommandResult.Command, false, true, true);
                    // Report that these replaced commands are executed by the debugger.
                    DebuggerCommand dbgCommand = new DebuggerCommand(dbgCommandResult.Command, null, false, true);
                    output.Add(new PSObject(dbgCommand));
            if (addToHistory && (Commands.Commands.Count > 0))
                addToHistory = DebuggerUtils.ShouldAddCommandToHistory(
                    Commands.Commands[0].CommandText);
            // Remote PowerShell Invoke must always run Invoke so that the
            // command can complete.
            if (Commands.Commands.Count == 0 &&
                invokeMustRun)
                Commands.Commands.AddScript(string.Empty);
            if (Commands.Commands.Count > 0)
                    settings ??= new PSInvocationSettings();
                    settings.AddToHistory = true;
                Invoke<PSObject>(input, output, settings);
        #region Invoke Overloads
        /// Invoke the <see cref="Command"/> synchronously and return
        /// the output PSObject collection.
        /// collection of PSObjects.
        /// Cannot perform the operation because the command is
        /// already started.Stop the command and try the operation again.
        /// No commands are specified.
        /// PowerShell.Invoke can throw a variety of exceptions derived
            return Invoke(null, null);
        /// Input to the command
        /// Collection of PSObjects representing output.
        public Collection<PSObject> Invoke(IEnumerable input)
            return Invoke(input, null);
        /// <param name="settings">
        /// Invocation Settings
        public Collection<PSObject> Invoke(IEnumerable input, PSInvocationSettings settings)
            Collection<PSObject> result = new Collection<PSObject>();
            PSDataCollection<PSObject> listToWriteTo = new PSDataCollection<PSObject>(result);
            CoreInvoke<PSObject>(input, listToWriteTo, settings);
        /// the output.
        /// Type of output object(s) expected from the command invocation.
        public Collection<T> Invoke<T>()
            // We should bind all the results to this instance except
            // for output.
            Invoke<T>(null, result, null);
        public Collection<T> Invoke<T>(IEnumerable input)
            Invoke<T>(input, result, null);
        public Collection<T> Invoke<T>(IEnumerable input, PSInvocationSettings settings)
            Invoke<T>(input, result, settings);
        /// Invoke the <see cref="Command"/> synchronously and collect
        /// output data into the buffer <paramref name="output"/>
        /// A collection supplied by the user where output is collected.
        /// <paramref name="output"/> cannot be null.
        public void Invoke<T>(IEnumerable input, IList<T> output)
            Invoke<T>(input, output, null);
        /// Invocation Settings to use.
        public void Invoke<T>(IEnumerable input, IList<T> output, PSInvocationSettings settings)
                throw PSTraceSource.NewArgumentNullException(nameof(output));
            // use the above collection as the data store.
            PSDataCollection<T> listToWriteTo = new PSDataCollection<T>(output);
            CoreInvoke<T>(input, listToWriteTo, settings);
        /// Invoke the <see cref="Command"/> synchronously and stream
        /// <typeparam name="TInput">
        /// Type of input object(s) expected from the command invocation.
        /// <typeparam name="TOutput">
        /// Output of the command.
        public void Invoke<TInput, TOutput>(PSDataCollection<TInput> input, PSDataCollection<TOutput> output, PSInvocationSettings settings)
            CoreInvoke<TInput, TOutput>(input, output, settings);
        /// Invoke the <see cref="Command"/> asynchronously.
        /// Use EndInvoke() to obtain the output of the command.
        /// Cannot perform the operation because the command is already started.
        /// Stop the command and try the operation again.
        /// No command is added.
        public IAsyncResult BeginInvoke()
            return BeginInvoke<object>(null, null, null, null);
        /// When invoked using BeginInvoke, invocation doesn't
        /// finish until Input is closed. Caller of BeginInvoke must
        /// close the input buffer after all input has been written to
        /// input buffer. Input buffer is closed by calling
        /// Close() method.
        /// If you want this command to execute as a standalone cmdlet
        /// be sure to call Close() before calling BeginInvoke().  Otherwise,
        /// the command will be executed as though it had external input.
        /// If you observe that the command isn't doing anything,
        /// this may be the reason.
        /// Type of the input buffer
        /// Input to the command. See remarks for more details.
        public IAsyncResult BeginInvoke<T>(PSDataCollection<T> input)
            return BeginInvoke<T>(input, null, null, null);
        /// Invocation Settings.
        /// An AsyncCallback to call once the BeginInvoke completes.
        /// Note: when using this API in script, don't pass in a delegate that is cast from a script block.
        /// The callback could be invoked from a thread without a default Runspace and a delegate cast from
        /// a script block would fail in that case.
        /// A user supplied state to call the <paramref name="callback"/>
        /// with.
        public IAsyncResult BeginInvoke<T>(PSDataCollection<T> input, PSInvocationSettings settings, AsyncCallback callback, object state)
            if (OutputBuffer != null)
                if (_isBatching || ExtraCommands.Count != 0)
                    return BeginBatchInvoke<T, PSObject>(input, OutputBuffer, settings, callback, state);
                return CoreInvokeAsync<T, PSObject>(input, OutputBuffer, settings, callback, state, null);
                return CoreInvokeAsync<T, PSObject>(input, OutputBuffer, settings, callback, state, OutputBuffer);
        /// When this method is used EndInvoke() returns a null buffer.
        /// Type of input object(s) for the command invocation.
        /// A buffer supplied by the user where output is collected.
        public IAsyncResult BeginInvoke<TInput, TOutput>(PSDataCollection<TInput> input, PSDataCollection<TOutput> output)
            return BeginInvoke<TInput, TOutput>(input, output, null, null, null);
        /// Invoke the <see cref="Command"/> asynchronously and collect
        /// output data into the buffer <paramref name="output"/>.
        public IAsyncResult BeginInvoke<TInput, TOutput>(PSDataCollection<TInput> input, PSDataCollection<TOutput> output, PSInvocationSettings settings, AsyncCallback callback, object state)
                return BeginBatchInvoke<TInput, TOutput>(input, output, settings, callback, state);
            return CoreInvokeAsync<TInput, TOutput>(input, output, settings, callback, state, null);
        /// Invoke a PowerShell command asynchronously.
        /// Use await to wait for the command to complete and obtain the output of the command.
        /// The output buffer created to hold the results of the asynchronous invoke.
        /// The running PowerShell pipeline was stopped.
        /// This occurs when <see cref="PowerShell.Stop"/> or <see cref="PowerShell.StopAsync(AsyncCallback, object)"/> is called.
        public Task<PSDataCollection<PSObject>> InvokeAsync()
            => Task<PSDataCollection<PSObject>>.Factory.FromAsync(BeginInvoke(), _endInvokeMethod);
        /// When invoked using InvokeAsync, invocation doesn't
        /// finish until Input is closed. Caller of InvokeAsync must
        /// </para><para>
        /// be sure to call Close() before calling InvokeAsync().  Otherwise,
        /// Type of the input buffer.
        public Task<PSDataCollection<PSObject>> InvokeAsync<T>(PSDataCollection<T> input)
            => Task<PSDataCollection<PSObject>>.Factory.FromAsync(BeginInvoke<T>(input), _endInvokeMethod);
        /// An AsyncCallback to call once the command is invoked.
        public Task<PSDataCollection<PSObject>> InvokeAsync<T>(PSDataCollection<T> input, PSInvocationSettings settings, AsyncCallback callback, object state)
            => Task<PSDataCollection<PSObject>>.Factory.FromAsync(BeginInvoke<T>(input, settings, callback, state), _endInvokeMethod);
        /// The output buffer created to hold the results of the asynchronous invoke,
        /// or null if the caller provided their own buffer.
        /// To collect partial output in this scenario,
        /// supply a <see cref="System.Management.Automation.PSDataCollection{T}" /> for the <paramref name="output"/> parameter,
        /// and either add a handler for the <see cref="System.Management.Automation.PSDataCollection{T}.DataAdding"/> event
        /// or catch the exception and enumerate the object supplied for <paramref name="output"/>.
        public Task<PSDataCollection<PSObject>> InvokeAsync<TInput, TOutput>(PSDataCollection<TInput> input, PSDataCollection<TOutput> output)
            => Task<PSDataCollection<PSObject>>.Factory.FromAsync(BeginInvoke<TInput, TOutput>(input, output), _endInvokeMethod);
        /// Invoke a PowerShell command asynchronously and collect
        /// or catch the exception and use object supplied for <paramref name="output"/>.
        public Task<PSDataCollection<PSObject>> InvokeAsync<TInput, TOutput>(PSDataCollection<TInput> input, PSDataCollection<TOutput> output, PSInvocationSettings settings, AsyncCallback callback, object state)
            => Task<PSDataCollection<PSObject>>.Factory.FromAsync(BeginInvoke<TInput, TOutput>(input, output, settings, callback, state), _endInvokeMethod);
        /// Begins a batch execution.
        private IAsyncResult BeginBatchInvoke<TInput, TOutput>(PSDataCollection<TInput> input, PSDataCollection<TOutput> output, PSInvocationSettings settings, AsyncCallback callback, object state)
            if ((object)output is not PSDataCollection<PSObject> asyncOutput)
            RunspacePool pool = _rsConnection as RunspacePool;
            if ((pool != null) && (pool.IsRemote))
                // Server supports batch invocation, in this case, we just send everything to the server and return immediately
                if (ServerSupportsBatchInvocation())
                        return CoreInvokeAsync<TInput, TOutput>(input, output, settings, callback, state, asyncOutput);
                            EndAsyncBatchExecution();
            // Non-remoting case or server does not support batching
            // In this case we execute the cmdlets one by one
            RunningExtraCommands = true;
            _batchInvocationSettings = settings;
            _batchAsyncResult = new PowerShellAsyncResult(InstanceId, callback, state, asyncOutput, true);
            CoreInvokeAsync<TInput, TOutput>(input, output, settings, new AsyncCallback(BatchInvocationCallback), state, asyncOutput);
            return _batchAsyncResult;
        /// Batch invocation callback.
        /// <param name="state"></param>
        private void BatchInvocationWorkItem(object state)
            Debug.Assert(ExtraCommands.Count != 0, "This callback is for batch invocation only");
            BatchInvocationContext context = state as BatchInvocationContext;
            Debug.Assert(context != null, "Context should never be null");
            PSCommand backupCommand = _psCommand;
                _psCommand = context.Command;
                // Last element
                if (_psCommand == ExtraCommands[ExtraCommands.Count - 1])
                    IAsyncResult cmdResult = CoreInvokeAsync<object, PSObject>(null, context.Output, _batchInvocationSettings,
                        null, _batchAsyncResult.AsyncState, context.Output);
                    EndInvoke(cmdResult);
                catch (ActionPreferenceStopException e)
                    // We need to honor the current error action preference here
                    _stopBatchExecution = true;
                    _batchAsyncResult.SetAsCompleted(e);
                    // Stop if necessarily
                    if ((_batchInvocationSettings != null) && _batchInvocationSettings.ErrorActionPreference == ActionPreference.Stop)
                        AppendExceptionToErrorStream(e);
                        _batchAsyncResult.SetAsCompleted(null);
                    // If we get here, then ErrorActionPreference is either Continue,
                    // SilentlyContinue, or Inquire (Continue), so we just continue....
                    if (_batchInvocationSettings == null)
                        ActionPreference preference = (ActionPreference)Runspace.SessionStateProxy.GetVariable("ErrorActionPreference");
                    else if (_batchInvocationSettings.ErrorActionPreference != ActionPreference.Ignore)
                    // Let it continue
                _psCommand = backupCommand;
                context.Signal();
        private void BatchInvocationCallback(IAsyncResult result)
            PSDataCollection<PSObject> objs = null;
                objs = EndInvoke(result) ?? _batchAsyncResult.Output;
                DoRemainingBatchCommands(objs);
            catch (PipelineStoppedException e)
                // PowerShell throws the pipeline stopped exception.
                ActionPreference preference;
                if (_batchInvocationSettings != null)
                    preference = _batchInvocationSettings.ErrorActionPreference ?? ActionPreference.Continue;
                    preference = (Runspace != null) ?
                        (ActionPreference)Runspace.SessionStateProxy.GetVariable("ErrorActionPreference")
                        : ActionPreference.Continue;
                objs ??= _batchAsyncResult.Output;
        /// Executes remaining batch commands.
        private void DoRemainingBatchCommands(PSDataCollection<PSObject> objs)
            if (ExtraCommands.Count > 1)
                for (int i = 1; i < ExtraCommands.Count; i++)
                    if (_stopBatchExecution)
                    BatchInvocationContext context = new BatchInvocationContext(ExtraCommands[i], objs);
                    // Queue a batch work item here.
                    // Calling CoreInvokeAsync / CoreInvoke here directly doesn't work and causes the thread to not respond.
                    ThreadPool.QueueUserWorkItem(new WaitCallback(BatchInvocationWorkItem), context);
                    context.Wait();
        private void DetermineIsBatching()
            foreach (Command command in _psCommand.Commands)
                if (command.IsEndOfStatement)
                    _isBatching = true;
            _isBatching = false;
        /// Prepare for async batch execution.
        private void SetupAsyncBatchExecution()
            Debug.Assert(_isBatching);
            _backupPSCommand = _psCommand.Clone();
            ExtraCommands.Clear();
            PSCommand currentPipe = new PSCommand();
            currentPipe.Owner = this;
                    currentPipe.Commands.Add(command);
                    ExtraCommands.Add(currentPipe);
                    currentPipe = new PSCommand();
            if (currentPipe.Commands.Count != 0)
            _psCommand = ExtraCommands[0];
        /// Ends an async batch execution.
        private void EndAsyncBatchExecution()
            _psCommand = _backupPSCommand;
        /// Appends an exception to the error stream.
        private void AppendExceptionToErrorStream(Exception e)
            IContainsErrorRecord er = e as IContainsErrorRecord;
            if (er != null && er.ErrorRecord != null)
                this.Streams.Error.Add(er.ErrorRecord);
                this.Streams.Error.Add(new ErrorRecord(e,
                    "InvalidOperation", ErrorCategory.InvalidOperation, null));
        /// Waits for the pending asynchronous BeginInvoke to complete.
        /// <param name="asyncResult">
        /// Instance of IAsyncResult returned by BeginInvoke.
        /// The output buffer created to hold the results of the asynchronous invoke, or null if the caller provided their own buffer.
        /// asyncResult is a null reference.
        /// asyncResult object was not created by calling BeginInvoke
        /// on this PowerShell instance.
        /// supply a <see cref="System.Management.Automation.PSDataCollection{T}" /> to <see cref="PowerShell.BeginInvoke"/> for the <paramref name="output"/> parameter
        /// or catch the exception and enumerate the object supplied.
        public PSDataCollection<PSObject> EndInvoke(IAsyncResult asyncResult)
                if (asyncResult == null)
                    throw PSTraceSource.NewArgumentNullException(nameof(asyncResult));
                if ((psAsyncResult == null) ||
                    (psAsyncResult.OwnerId != InstanceId) ||
                    (!psAsyncResult.IsAssociatedWithAsyncInvoke))
                    throw PSTraceSource.NewArgumentException(nameof(asyncResult),
                        PowerShellStrings.AsyncResultNotOwned, "IAsyncResult", "BeginInvoke");
                // PowerShell no longer owns the output buffer when it is passed back to the caller.
                ResetOutputBufferAsNeeded();
                return psAsyncResult.Output;
            catch (InvalidRunspacePoolStateException exception)
                if (_runspace != null) // the pool exception was actually thrown by a runspace
                    throw exception.ToInvalidRunspaceStateException();
        #region Stop Overloads
        /// Stop the currently running command synchronously.
        /// When used with <see cref="PowerShell.Invoke()"/>, that call will return a partial result.
        /// When used with <see cref="PowerShell.InvokeAsync"/>, that call will throw a <see cref="System.Management.Automation.PipelineStoppedException"/>.
                IAsyncResult asyncResult = CoreStop(true, null, null);
                // This is a sync call..Wait for the stop operation to complete.
                asyncResult.AsyncWaitHandle.WaitOne();
                // PowerShell no longer owns the output buffer when the pipeline is stopped by caller.
                // If it's already disposed, then the client doesn't need to know.
        /// Stop the currently running command asynchronously. If the command is not started,
        /// the state of PowerShell instance is changed to Stopped and corresponding events
        /// will be raised.
        /// The returned IAsyncResult object can be used to wait for the stop operation
        /// to complete.
        /// A AsyncCallback to call once the BeginStop completes.
        public IAsyncResult BeginStop(AsyncCallback callback, object state)
            return CoreStop(false, callback, state);
        /// Waits for the pending asynchronous BeginStop to complete.
        /// Instance of IAsyncResult returned by BeginStop.
        /// asyncResult object was not created by calling BeginStop
        public void EndStop(IAsyncResult asyncResult)
                (psAsyncResult.IsAssociatedWithAsyncInvoke))
                    PowerShellStrings.AsyncResultNotOwned, "IAsyncResult", "BeginStop");
        /// Stop a PowerShell command asynchronously.
        /// Use await to wait for the command to stop.
        /// If the command is not started, the state of the PowerShell instance
        /// is changed to Stopped and corresponding events will be raised.
        public Task StopAsync(AsyncCallback callback, object state)
            => Task.Factory.FromAsync(BeginStop(callback, state), _endStopMethod);
        #region Event Handlers
        /// Handler for state changed events for the currently running pipeline.
        /// Source of the event.
        /// <param name="stateEventArgs">
        /// Pipeline State.
        private void PipelineStateChanged(object source, PipelineStateEventArgs stateEventArgs)
            // we need to process the pipeline event.
            PSInvocationStateInfo targetStateInfo = new PSInvocationStateInfo(stateEventArgs.PipelineStateInfo);
            SetStateChanged(targetStateInfo);
                // if already disposed return
            // Stop the currently running command outside of the lock
            if (InvocationStateInfo.State == PSInvocationState.Running ||
                InvocationStateInfo.State == PSInvocationState.Stopping)
            if (OutputBuffer != null && OutputBufferOwner)
                OutputBuffer.Dispose();
            if (_errorBuffer != null && ErrorBufferOwner)
                _errorBuffer.Dispose();
            if (IsRunspaceOwner)
            RemotePowerShell?.Dispose();
            _stopAsyncResult = null;
        #region Internal / Private Methods / Properties
        /// Indicates if this PowerShell object is the owner of the
        /// runspace or RunspacePool assigned to this object.
        public bool IsRunspaceOwner { get; internal set; } = false;
        internal bool ErrorBufferOwner { get; set; } = true;
        internal bool OutputBufferOwner { get; set; } = true;
        /// OutputBuffer.
        internal PSDataCollection<PSObject> OutputBuffer { get; private set; }
        /// Reset the output buffer to null if it's owned by the current powershell instance.
        private void ResetOutputBufferAsNeeded()
            if (OutputBufferOwner)
        /// Get a steppable pipeline object.
        /// <returns>A steppable pipeline object.</returns>
        /// <exception cref="InvalidOperationException">An attempt was made to use the scriptblock outside of the engine.</exception>
        public SteppablePipeline GetSteppablePipeline()
            ExecutionContext context = GetContextFromTLS();
            SteppablePipeline spl = GetSteppablePipeline(context, CommandOrigin.Internal);
            return spl;
        /// Returns the current execution context from TLS, or raises an exception if it is null.
        internal ExecutionContext GetContextFromTLS()
            // If ExecutionContext from TLS is null then we are not in powershell engine thread.
                string scriptText = this.Commands.Commands.Count > 0 ? this.Commands.Commands[0].CommandText : null;
                PSInvalidOperationException e = null;
                if (scriptText != null)
                    scriptText = ErrorCategoryInfo.Ellipsize(System.Globalization.CultureInfo.CurrentUICulture, scriptText);
                    e = PSTraceSource.NewInvalidOperationException(
                        PowerShellStrings.CommandInvokedFromWrongThreadWithCommand,
                        scriptText);
                        PowerShellStrings.CommandInvokedFromWrongThreadWithoutCommand);
                e.SetErrorId("CommandInvokedFromWrongThread");
        /// Gets the steppable pipeline from the powershell object.
        /// <param name="context">Engine execution context.</param>
        /// <returns>Steppable pipeline object.</returns>
        private SteppablePipeline GetSteppablePipeline(ExecutionContext context, CommandOrigin commandOrigin)
            // Check for an empty pipeline
            if (Commands.Commands.Count == 0)
                foreach (Command cmd in Commands.Commands)
                    CommandProcessorBase commandProcessorBase =
                            cmd.CreateCommandProcessor
                                Runspace.DefaultRunspace.ExecutionContext,
                                IsNested ? CommandOrigin.Internal : CommandOrigin.Runspace
                    commandProcessorBase.RedirectShellErrorOutputPipe = RedirectShellErrorOutputPipe;
            return new SteppablePipeline(context, pipelineProcessor);
        internal bool IsGetCommandMetadataSpecialPipeline { get; set; }
        /// Checks if the command is running.
        private bool IsCommandRunning()
            if (InvocationStateInfo.State == PSInvocationState.Running)
        /// Checks if the current state is Disconnected.
        private bool IsDisconnected()
            return (InvocationStateInfo.State == PSInvocationState.Disconnected);
        /// Checks if the command is already running.
        /// If the command is already running, throws an
        private void AssertExecutionNotStarted()
            if (IsCommandRunning())
                string message = StringUtil.Format(PowerShellStrings.ExecutionAlreadyStarted);
            if (IsDisconnected())
                string message = StringUtil.Format(PowerShellStrings.ExecutionDisconnected);
            if (InvocationStateInfo.State == PSInvocationState.Stopping)
                string message = StringUtil.Format(PowerShellStrings.ExecutionStopping);
        /// Checks if the current powershell instance can accept changes like
        /// changing one of the properties like Output, Command etc.
        /// If changes are not allowed, throws an exception.
        internal void AssertChangesAreAccepted()
                if (IsCommandRunning() || IsDisconnected())
        /// Checks if the current powershell instance is disposed.
        /// If disposed, throws ObjectDisposedException.
        private void AssertNotDisposed()
                throw PSTraceSource.NewObjectDisposedException("PowerShell");
        /// Clear the internal elements.
        private void InternalClearSuppressExceptions()
                _worker?.InternalClearSuppressExceptions();
        /// Raise the execution state change event handlers.
        /// <param name="stateInfo">
        /// State Information
        private void RaiseStateChangeEvent(PSInvocationStateInfo stateInfo)
            // First update the runspace availability.
            // The Pipeline class takes care of updating local runspaces.
            // Don't update for RemoteRunspace and nested PowerShell since this is used
            // only internally by the remote debugger.
            RemoteRunspace remoteRunspace = _runspace as RemoteRunspace;
            if (remoteRunspace != null && !this.IsNested)
                _runspace.UpdateRunspaceAvailability(InvocationStateInfo.State, true, InstanceId);
            if (stateInfo.State == PSInvocationState.Running)
                AddToRemoteRunspaceRunningList();
            else if (stateInfo.State == PSInvocationState.Completed || stateInfo.State == PSInvocationState.Stopped ||
                     stateInfo.State == PSInvocationState.Failed)
                RemoveFromRemoteRunspaceRunningList();
            InvocationStateChanged.SafeInvoke(this, new PSInvocationStateChangedEventArgs(stateInfo));
        /// Sets the state of this powershell instance.
        /// <param name="stateInfo">The state info to set.</param>
        internal void SetStateChanged(PSInvocationStateInfo stateInfo)
            PSInvocationStateInfo copyStateInfo = stateInfo;
            PSInvocationState previousState;
            // copy pipeline HasdErrors property to PowerShell instance...
            if (_worker != null && _worker.CurrentlyRunningPipeline != null)
                SetHadErrors(_worker.CurrentlyRunningPipeline.HadErrors);
            // win281312: Usig temporary variables to avoid thread
            // synchronization issues between Dispose and transition
            // to Terminal States (Completed/Failed/Stopped)
            PowerShellAsyncResult tempInvokeAsyncResult;
            PowerShellAsyncResult tempStopAsyncResult;
                previousState = InvocationStateInfo.State;
                // Check the current state and see if we need to process this pipeline event.
                switch (InvocationStateInfo.State)
                        // if the current state is already completed..then no need to process state
                        // change requests. This will happen if another thread calls BeginStop
                        // We are in stopping state and we should not honor Running state
                        // here.
                        if (stateInfo.State == PSInvocationState.Running ||
                            stateInfo.State == PSInvocationState.Stopping)
                        else if (stateInfo.State == PSInvocationState.Completed ||
                            copyStateInfo = new PSInvocationStateInfo(PSInvocationState.Stopped, stateInfo.Reason);
                tempInvokeAsyncResult = _invokeAsyncResult;
                tempStopAsyncResult = _stopAsyncResult;
                InvocationStateInfo = copyStateInfo;
            bool isExceptionOccured = false;
                    CloseInputBufferOnReconnection(previousState);
                    RaiseStateChangeEvent(InvocationStateInfo.Clone());
                    // Clear Internal data
                    InternalClearSuppressExceptions();
                    // Ensure remote receive queue is not blocked.
                        ResumeIncomingData();
                        if (RunningExtraCommands)
                            tempInvokeAsyncResult?.SetAsCompleted(InvocationStateInfo.Reason);
                        tempStopAsyncResult?.SetAsCompleted(null);
                        // need to release asyncresults if there is an
                        // exception from the eventhandlers.
                        isExceptionOccured = true;
                        // takes care exception occurred with invokeAsyncResult
                        if (isExceptionOccured && (tempStopAsyncResult != null))
                            tempStopAsyncResult.Release();
                        // If this command was disconnected and was also invoked synchronously then
                        // we throw an exception on the calling thread.
                        if (_commandInvokedSynchronously && (tempInvokeAsyncResult != null))
                            tempInvokeAsyncResult.SetAsCompleted(new RuntimeException(PowerShellStrings.DiscOnSyncCommand));
                        // This object can be disconnected even if "BeginStop" was called if it is a remote object
                        // and robust connections is retrying a failed network connection.
                        // In this case release the stop wait handle to prevent not responding.
                        // Only raise the Disconnected state changed event if the PowerShell state
                        // actually transitions to Disconnected from some other state.  This condition
                        // can happen when the corresponding runspace disconnects/connects multiple
                        // times with the command remaining in Disconnected state.
                        if (previousState != PSInvocationState.Disconnected)
                    // Make sure the connect command information is null when going to Disconnected state.
                    // This parameter is used to determine reconnect/reconstruct scenarios.  Setting to null
                    // means we have a reconnect scenario.
                    _connectCmdInfo = null;
        /// Helper function to close the input buffer after command is reconnected.
        /// <param name="previousState">Previous state.</param>
        private void CloseInputBufferOnReconnection(PSInvocationState previousState)
            // If the previous state was disconnected and we are now running (reconnected),
            // and we reconnected synchronously with pending input, then we need to close
            // the input buffer to allow the remote command to complete.  Otherwise the
            // synchronous Connect() method will wait indefinitely for the command to complete.
            if (previousState == PSInvocationState.Disconnected &&
                _commandInvokedSynchronously &&
                RemotePowerShell.InputStream != null &&
                RemotePowerShell.InputStream.IsOpen &&
                RemotePowerShell.InputStream.Count > 0)
                RemotePowerShell.InputStream.Close();
        /// Clear the internal reference to remote powershell.
        internal void ClearRemotePowerShell()
                RemotePowerShell?.Clear();
        /// Sets if the pipeline is nested, typically used by the remoting infrastructure.
        /// <param name="isNested"></param>
        internal void SetIsNested(bool isNested)
            IsNested = isNested;
        /// Performs the actual synchronous command invocation. The caller
        /// should check if it safe to call this method.
        /// Type of objects to return.
        /// Input to the command.
        /// output from the command
        /// Invocation settings.
        private void CoreInvoke<TOutput>(IEnumerable input, PSDataCollection<TOutput> output, PSInvocationSettings settings)
            PSDataCollection<object> inputBuffer = null;
                inputBuffer = new PSDataCollection<object>();
                foreach (object o in input)
                    inputBuffer.Add(o);
                inputBuffer.Complete();
            CoreInvoke(inputBuffer, output, settings);
        /// Core invocation helper method.
        /// <typeparam name="TInput">input type</typeparam>
        /// <typeparam name="TOutput">output type</typeparam>
        /// <param name="input">Input objects.</param>
        /// <param name="output">Output object.</param>
        /// <param name="settings">Invocation settings.</param>
        private void CoreInvokeHelper<TInput, TOutput>(PSDataCollection<TInput> input, PSDataCollection<TOutput> output, PSInvocationSettings settings)
            // Prepare the environment...non-remoting case.
            Prepare<TInput, TOutput>(input, output, settings, true);
                // Invoke in the same thread as the calling thread.
                Runspace rsToUse = null;
                if (!IsNested)
                    if (pool != null)
                        VerifyThreadSettings(settings, pool.ApartmentState, pool.ThreadOptions, false);
                        // getting the runspace asynchronously so that Stop can be supported from a different
                        // thread.
                        _worker.GetRunspaceAsyncResult = pool.BeginGetRunspace(null, null);
                        _worker.GetRunspaceAsyncResult.AsyncWaitHandle.WaitOne();
                        rsToUse = pool.EndGetRunspace(_worker.GetRunspaceAsyncResult);
                        rsToUse = _rsConnection as Runspace;
                        if (rsToUse != null)
                            VerifyThreadSettings(settings, rsToUse.ApartmentState, rsToUse.ThreadOptions, false);
                            if (rsToUse.RunspaceStateInfo.State != RunspaceState.Opened)
                                string message = StringUtil.Format(PowerShellStrings.InvalidRunspaceState, RunspaceState.Opened, rsToUse.RunspaceStateInfo.State);
                                InvalidRunspaceStateException e = new InvalidRunspaceStateException(message,
                                        rsToUse.RunspaceStateInfo.State,
                    // perform the work in the current thread
                    _worker.CreateRunspaceIfNeededAndDoWork(rsToUse, true);
                    Dbg.Assert(rsToUse != null,
                        "Nested PowerShell can only work on a Runspace");
                    // Perform work on the current thread. Nested Pipeline
                    // should be invoked from the same thread that the parent
                    // pipeline is executing in.
                    _worker.ConstructPipelineAndDoWork(rsToUse, true);
        /// Core invocation helper method for remoting.
        private void CoreInvokeRemoteHelper<TInput, TOutput>(PSDataCollection<TInput> input, PSDataCollection<TOutput> output, PSInvocationSettings settings)
            // For remote calls..use the infrastructure built in CoreInvokeAsync..
            IAsyncResult asyncResult = CoreInvokeAsync<TInput, TOutput>(input, output, settings,
                null, null, null);
            if ((InvocationStateInfo.State == PSInvocationState.Failed) &&
                        (InvocationStateInfo.Reason != null))
                throw InvocationStateInfo.Reason;
        /// Core invocation method.
        private void CoreInvoke<TInput, TOutput>(PSDataCollection<TInput> input, PSDataCollection<TOutput> output, PSInvocationSettings settings)
            bool isRemote = false;
            SetHadErrors(false);
                        CoreInvokeRemoteHelper(input, output, settings);
                isRemote = true;
                    foreach (PSCommand command in ExtraCommands)
                        if (_psCommand != ExtraCommands[ExtraCommands.Count - 1])
                            if (isRemote)
                                CoreInvokeHelper(input, output, settings);
                            if ((settings != null) && settings.ErrorActionPreference == ActionPreference.Stop)
                            // Ignore the exception if necessary.
                            if ((settings != null) && settings.ErrorActionPreference == ActionPreference.Ignore)
        /// Performs the actual asynchronous command invocation.
        /// <typeparam name="TInput">Type of the input buffer</typeparam>
        /// <typeparam name="TOutput">Type of the output buffer</typeparam>
        /// input can be null
        /// A AsyncCallback to call once the BeginInvoke completes.
        /// <param name="asyncResultOutput">
        /// The output buffer to attach to the IAsyncResult returned by this method
        /// BeginInvoke is called on nested powershell. Nested
        /// Powershell cannot be executed Asynchronously.
        private IAsyncResult CoreInvokeAsync<TInput, TOutput>(PSDataCollection<TInput> input,
            PSDataCollection<TOutput> output, PSInvocationSettings settings,
            AsyncCallback callback, object state, PSDataCollection<PSObject> asyncResultOutput)
            // We dont need to create worker if pool is remote
            Prepare<TInput, TOutput>(input, output, settings, (pool == null || !pool.IsRemote));
            _invokeAsyncResult = new PowerShellAsyncResult(InstanceId, callback, state, asyncResultOutput, true);
                // IsNested is true for the icm | % { icm } scenario
                if (!IsNested || (pool != null && pool.IsRemote))
                        VerifyThreadSettings(settings, pool.ApartmentState, pool.ThreadOptions, pool.IsRemote);
                        pool.AssertPoolIsOpen();
                        // for executing in a remote runspace pool case
                        if (pool.IsRemote)
                            _worker = null;
                                // for remoting case, when the state is set to
                                // Running, the message should have been sent
                                // to the server. In order to ensure the same
                                // all of the following are placed inside the
                                // lock
                                //    1. set the state to Running
                                //    2. create remotePowerShell
                                //    3. Send message to server
                                // set the execution state to running.. so changes
                                // to the current instance of powershell
                                // are blocked.
                                AssertExecutionNotStarted();
                                InvocationStateInfo = new PSInvocationStateInfo(PSInvocationState.Running, null);
                                ObjectStreamBase inputStream = null;
                                    inputStream = new PSDataCollectionStream<TInput>(InstanceId, input);
                                    if (inputStream == null)
                                        inputStream = new ObjectStream();
                                    RemotePowerShell.Initialize(
                                        inputStream, new PSDataCollectionStream<TOutput>(InstanceId, output),
                                                    InformationalBuffers, settings);
                                    if (inputStream != null)
                                        RemotePowerShell.InputStream = inputStream;
                                        RemotePowerShell.OutputStream =
                                            new PSDataCollectionStream<TOutput>(InstanceId, output);
                                pool.RemoteRunspacePoolInternal.CreatePowerShellOnServerAndInvoke(RemotePowerShell);
                            _worker.GetRunspaceAsyncResult = pool.BeginGetRunspace(
                                    new AsyncCallback(_worker.RunspaceAvailableCallback), null);
                        LocalRunspace rs = _rsConnection as LocalRunspace;
                        if (rs != null)
                            VerifyThreadSettings(settings, rs.ApartmentState, rs.ThreadOptions, false);
                            if (rs.RunspaceStateInfo.State != RunspaceState.Opened)
                                string message = StringUtil.Format(PowerShellStrings.InvalidRunspaceState, RunspaceState.Opened, rs.RunspaceStateInfo.State);
                                        rs.RunspaceStateInfo.State,
                            _worker.CreateRunspaceIfNeededAndDoWork(rs, false);
                            // create a new runspace and perform invoke..
                            ThreadPool.QueueUserWorkItem(
                                new WaitCallback(_worker.CreateRunspaceIfNeededAndDoWork),
                                _rsConnection);
                    // Nested PowerShell
                    throw PSTraceSource.NewInvalidOperationException(PowerShellStrings.NestedPowerShellInvokeAsync);
        // Apartment thread state does not apply to non-Windows platforms.
        /// Verifies the settings for ThreadOptions and ApartmentState.
        private static void VerifyThreadSettings(PSInvocationSettings settings, ApartmentState runspaceApartmentState, PSThreadOptions runspaceThreadOptions, bool isRemote)
            if (settings != null && settings.ApartmentState != ApartmentState.Unknown)
                apartmentState = settings.ApartmentState;
                apartmentState = runspaceApartmentState;
            if (runspaceThreadOptions == PSThreadOptions.ReuseThread)
                if (apartmentState != runspaceApartmentState)
                    throw new InvalidOperationException(PowerShellStrings.ApartmentStateMismatch);
            else if (runspaceThreadOptions == PSThreadOptions.UseCurrentThread)
                if (!isRemote) // on remote calls this check needs to be done by the server
                    if (apartmentState != ApartmentState.Unknown && apartmentState != Thread.CurrentThread.GetApartmentState())
                        throw new InvalidOperationException(PowerShellStrings.ApartmentStateMismatchCurrentThread);
        /// <typeparam name="TInput">Type for the input collection</typeparam>
        /// <typeparam name="TOutput">Type for the output collection</typeparam>
        /// <param name="shouldCreateWorker"></param>
        private void Prepare<TInput, TOutput>(PSDataCollection<TInput> input, PSDataCollection<TOutput> output, PSInvocationSettings settings, bool shouldCreateWorker)
            Dbg.Assert(output != null, "Output cannot be null");
                if ((_psCommand == null) || (_psCommand.Commands == null) || (_psCommand.Commands.Count == 0))
                    throw PSTraceSource.NewInvalidOperationException(PowerShellStrings.NoCommandToInvoke);
                // If execution has already started this will throw
                if (shouldCreateWorker)
                    // update settings for impersonation policy
                    if ((settings != null) && (settings.FlowImpersonationPolicy))
                        // get the identity of the thread.
                        // false behavior: If the thread is impersonating the WindowsIdentity for the
                        // thread is returned. If the thread is not impersonating, the WindowsIdentity of
                        // the process is returned.
                        settings.WindowsIdentityToImpersonate =
                            System.Security.Principal.WindowsIdentity.GetCurrent(false);
                    // Create the streams and handoff these to the pipeline
                    // this way pipeline will not waste resources creating
                    // the same.
                    ObjectStreamBase inputStream;
                    ObjectStreamBase outputStream = new PSDataCollectionStream<TOutput>(InstanceId, output);
                    _worker = new Worker(inputStream, outputStream, settings, this);
            // Only one thread will be running after this point
            // so no need to lock.
                // Raise the state change events outside of the lock
                // send a cloned copy..this way the handler will
                // not see changes happening to the instance's execution state.
        /// Called by both Sync Stop and Async Stop.
        /// If isSyncCall is false, then an IAsyncResult object is returned which
        /// can be passed back to the user.
        /// <param name="isSyncCall">
        /// true if pipeline to be stopped synchronously,
        /// false otherewise.
        /// Valid for asynchronous stop
        private IAsyncResult CoreStop(bool isSyncCall, AsyncCallback callback, object state)
            bool isRunning = false;
            bool isDisconnected = false;
            Queue<PSInvocationStateInfo> events = new Queue<PSInvocationStateInfo>();
            // Acquire lock as we are going to change state here..
                // BUGBUG: remote powershell appears to handle state change's differently
                // Need to speak with remoting dev and resolve this.
                        // Stopped is called before operation started..we need to change
                        // state to stopping and then to stopped... so that future stops
                        // dont affect the state.
                        InvocationStateInfo = new PSInvocationStateInfo(PSInvocationState.Stopping,
                        events.Enqueue(new PSInvocationStateInfo(PSInvocationState.Stopped,
                        _stopAsyncResult = new PowerShellAsyncResult(InstanceId, callback, state, null, false);
                        _stopAsyncResult.SetAsCompleted(null);
                        return _stopAsyncResult;
                        // Create new stop sync object if none exists.  Otherwise return existing.
                        if (_stopAsyncResult == null)
                        isRunning = true;
                        // Stopping a disconnected command results in a failed state.
                        InvocationStateInfo = new PSInvocationStateInfo(PSInvocationState.Failed, null);
                        isDisconnected = true;
            // If in the Disconnected state then stopping simply cuts loose the PowerShell object
            // so that a new one can be connected.  The state is set to Failed since the command
            // cannot complete with this object.
            if (isDisconnected)
                // Since object is stopped, allow result wait to end.
                _invokeAsyncResult?.SetAsCompleted(null);
                // Raise event for failed state change.
            // Ensure the runspace is not blocking in a debug stop.
            ReleaseDebugger();
            bool shouldRunStopHelper = false;
            if (pool != null && pool.IsRemote)
                if ((RemotePowerShell != null) && RemotePowerShell.Initialized)
                    RemotePowerShell.StopAsync();
                    if (isSyncCall)
                        _stopAsyncResult.AsyncWaitHandle.WaitOne();
                    shouldRunStopHelper = true;
            else if (isRunning)
                _worker.Stop(isSyncCall);
            if (shouldRunStopHelper)
                    StopHelper(events);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(StopThreadProc), events);
        private void ReleaseDebugger()
            LocalRunspace localRunspace = _runspace as LocalRunspace;
            localRunspace?.ReleaseDebugger();
        /// If there is no worker assigned yet, we need to emulate stop here.
        /// In Asynchronous stop case, we need to send event change notifications
        /// from a different thread.
        private void StopHelper(object state)
            Queue<PSInvocationStateInfo> events = state as Queue<PSInvocationStateInfo>;
            Dbg.Assert(events != null,
                "StopImplementation expects a Queue<PSInvocationStateInfo> as parameter");
            // Raise the events outside of the lock..this way 3rd party callback
            // cannot hold our lock.
            while (events.Count > 0)
                PSInvocationStateInfo targetStateInfo = events.Dequeue();
            // Clear internal resources
        private void StopThreadProc(object state)
            // variable to keep track of exceptions.
                StopHelper(state);
                // report non-severe exceptions to the user via the
                // asyncresult object
        /// The client remote powershell associated with this
        /// powershell object.
        internal ClientRemotePowerShell RemotePowerShell { get; private set; }
        /// The history string to be used for displaying
        /// the history.
        public string HistoryString { get; set; }
        /// Extra commands to run in a single invocation.
        internal Collection<PSCommand> ExtraCommands { get; }
        /// Currently running extra commands.
        internal bool RunningExtraCommands { get; private set; }
        private bool ServerSupportsBatchInvocation()
            if (_runspace != null)
                return _runspace.RunspaceStateInfo.State != RunspaceState.BeforeOpen &&
                       _runspace.GetRemoteProtocolVersion() >= RemotingConstants.ProtocolVersion_2_2;
            return remoteRunspacePoolInternal != null &&
                   remoteRunspacePoolInternal.PSRemotingProtocolVersion >= RemotingConstants.ProtocolVersion_2_2;
        /// Helper method to add running remote PowerShell to the remote runspace list.
        private void AddToRemoteRunspaceRunningList()
                _runspace.PushRunningPowerShell(this);
                RemoteRunspacePoolInternal remoteRunspacePoolInternal = GetRemoteRunspacePoolInternal();
                remoteRunspacePoolInternal?.PushRunningPowerShell(this);
        /// Helper method to remove running remote PowerShell from the remote runspacelist.
        private void RemoveFromRemoteRunspaceRunningList()
                _runspace.PopRunningPowerShell();
                remoteRunspacePoolInternal?.PopRunningPowerShell();
        private RemoteRunspacePoolInternal GetRemoteRunspacePoolInternal()
            RunspacePool runspacePool = _rsConnection as RunspacePool;
            return runspacePool?.RemoteRunspacePoolInternal;
        #region Worker
        /// AsyncResult object used to monitor pipeline creation and invocation.
        /// This is needed as a Runspace may not be available in the RunspacePool.
        private sealed class Worker
            private readonly ObjectStreamBase _inputStream;
            private readonly ObjectStreamBase _outputStream;
            private readonly ObjectStreamBase _errorStream;
            private readonly PSInvocationSettings _settings;
            private bool _isNotActive;
            private readonly PowerShell _shell;
            /// <param name="inputStream"></param>
            /// <param name="outputStream"></param>
            /// <param name="shell"></param>
            internal Worker(ObjectStreamBase inputStream,
                PowerShell shell)
                _inputStream = inputStream;
                _outputStream = outputStream;
                _errorStream = new PSDataCollectionStream<ErrorRecord>(shell.InstanceId, shell._errorBuffer);
                _settings = settings;
                _shell = shell;
            /// Sets the async result object that monitors a
            /// BeginGetRunspace async operation on the
            /// RunspacePool.
            internal IAsyncResult GetRunspaceAsyncResult { get; set; }
            /// Gets the currently running pipeline.
            internal Pipeline CurrentlyRunningPipeline { get; private set; }
            /// This method gets invoked from a ThreadPool thread.
            internal void CreateRunspaceIfNeededAndDoWork(object state)
                Runspace rsToUse = state as Runspace;
                CreateRunspaceIfNeededAndDoWork(rsToUse, false);
            /// This method gets invoked when PowerShell is not associated
            /// with a RunspacePool.
            /// <param name="rsToUse">
            /// User supplied Runspace if any.
            /// <param name="isSync">
            /// true if Invoke() should be used to invoke pipeline
            /// false if InvokeAsync() should be used.
            /// All exceptions are caught and reported via a
            /// PipelineStateChanged event.
            internal void CreateRunspaceIfNeededAndDoWork(Runspace rsToUse, bool isSync)
                    // Set the host for this local runspace if user specified one.
                    LocalRunspace rs = rsToUse as LocalRunspace;
                    if (rs == null)
                        lock (_shell._syncObject)
                            if (_shell._runspace != null)
                                rsToUse = _shell._runspace;
                                Runspace runspace = null;
                                if ((_settings != null) && (_settings.Host != null))
                                    runspace = RunspaceFactory.CreateRunspace(_settings.Host);
                                    runspace = RunspaceFactory.CreateRunspace();
                                _shell.SetRunspace(runspace, true);
                                rsToUse = (LocalRunspace)runspace;
                                rsToUse.Open();
                    ConstructPipelineAndDoWork(rsToUse, isSync);
                    // PipelineStateChangedEvent is not raised
                    // if there is an exception calling BeginInvoke
                    // So raise the event here and notify the caller.
                        if (_isNotActive)
                        _isNotActive = true;
                    _shell.PipelineStateChanged(this,
                           new PipelineStateEventArgs(
                               new PipelineStateInfo(PipelineState.Failed,
                                   e)));
                    if (isSync)
            /// This method gets called from a ThreadPool thread.
            /// This method gets called from a RunspacePool thread when a
            /// Runspace is available.
            /// AsyncResult object which monitors the asyncOperation.
            internal void RunspaceAvailableCallback(IAsyncResult asyncResult)
                    RunspacePool pool = _shell._rsConnection as RunspacePool;
                    Dbg.Assert(pool != null, "RunspaceConnection must be a runspace pool");
                    // get the runspace..this will throw if there is an exception
                    // occurred while getting the runspace.
                    Runspace pooledRunspace = pool.EndGetRunspace(asyncResult);
                    bool isPipelineCreated = ConstructPipelineAndDoWork(pooledRunspace, false);
                    if (!isPipelineCreated)
                        pool.ReleaseRunspace(pooledRunspace);
            /// Constructs a pipeline from the supplied runspace and invokes
            /// pipeline either synchronously or asynchronously identified by
            /// <paramref name="performSyncInvoke"/>.
            /// <param name="rs">
            /// Runspace to create pipeline. Cannot be null.
            /// <param name="performSyncInvoke">
            /// if true, Invoke() is called
            /// BeginInvoke() otherwise.
            /// 1.BeginInvoke is called on nested powershell. Nested
            /// true if the pipeline is created/invoked successfully.
            internal bool ConstructPipelineAndDoWork(Runspace rs, bool performSyncInvoke)
                Dbg.Assert(rs != null, "Runspace cannot be null in ConstructPipelineAndDoWork");
                _shell.RunspaceAssigned.SafeInvoke(this, new PSEventArgs<Runspace>(rs));
                // lock is needed until a pipeline is created to
                // make stop() cleanly release resources.
                LocalRunspace lrs = rs as LocalRunspace;
                    if (lrs != null)
                        LocalPipeline localPipeline = new LocalPipeline(
                            lrs,
                            _shell.Commands.Commands,
                            (_settings != null && _settings.AddToHistory),
                            _shell.IsNested,
                            _inputStream,
                            _outputStream,
                            _errorStream,
                            _shell.InformationalBuffers);
                        localPipeline.IsChild = _shell.IsChild;
                        if (!string.IsNullOrEmpty(_shell.HistoryString))
                            localPipeline.SetHistoryString(_shell.HistoryString);
                        localPipeline.RedirectShellErrorOutputPipe = _shell.RedirectShellErrorOutputPipe;
                        CurrentlyRunningPipeline = localPipeline;
                        // register for pipeline state changed events within a lock...so that if
                        // stop is called before invoke, we can listen to state transition and
                        // take appropriate action.
                        CurrentlyRunningPipeline.StateChanged += _shell.PipelineStateChanged;
                // Set pipeline specific settings
                CurrentlyRunningPipeline.InvocationSettings = _settings;
                Dbg.Assert(lrs != null, "LocalRunspace cannot be null here");
                if (performSyncInvoke)
                    CurrentlyRunningPipeline.Invoke();
                    CurrentlyRunningPipeline.InvokeAsync();
            /// Stops the async operation.
            /// <param name="isSyncCall"></param>
            internal void Stop(bool isSyncCall)
                    if (CurrentlyRunningPipeline != null)
                            CurrentlyRunningPipeline.Stop();
                            CurrentlyRunningPipeline.StopAsync();
                    if (GetRunspaceAsyncResult != null)
                        pool.CancelGetRunspace(GetRunspaceAsyncResult);
                // Pipeline is not yet associated with PowerShell..so emulate stop
                // locally
                events.Enqueue(new PSInvocationStateInfo(PSInvocationState.Stopped, null));
                    _shell.StopHelper(events);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(_shell.StopThreadProc), events);
            /// Internal clear is called when the invoke operation
            /// is completed or failed or stopped.
            internal void InternalClearSuppressExceptions()
                    if ((_settings != null) && (_settings.WindowsIdentityToImpersonate != null))
                        _settings.WindowsIdentityToImpersonate.Dispose();
                        _settings.WindowsIdentityToImpersonate = null;
                    _inputStream.Close();
                    _outputStream.Close();
                    _errorStream.Close();
                    if (CurrentlyRunningPipeline == null)
                    // Detach state changed handler so that runspace.close
                    // and pipeline.dispose will not change powershell instances state
                    CurrentlyRunningPipeline.StateChanged -= _shell.PipelineStateChanged;
                    if ((GetRunspaceAsyncResult == null) && (_shell._rsConnection == null))
                        // user did not supply a runspace..Invoke* method created
                        // a new runspace..so close it.
                        CurrentlyRunningPipeline.Runspace.Close();
                        pool?.ReleaseRunspace(CurrentlyRunningPipeline.Runspace);
                    CurrentlyRunningPipeline.Dispose();
                catch (InvalidRunspacePoolStateException)
                CurrentlyRunningPipeline = null;
            internal void GetSettings(out bool addToHistory, out bool noInput, out uint apartmentState)
                addToHistory = _settings.AddToHistory;
                noInput = false;
                apartmentState = (uint)_settings.ApartmentState;
        /// Creates a PowerShell object from a PSObject property bag.
        /// <param name="powerShellAsPSObject">PSObject to rehydrate.</param>
        /// PowerShell rehydrated from a PSObject property bag
        internal static PowerShell FromPSObjectForRemoting(PSObject powerShellAsPSObject)
            if (powerShellAsPSObject == null)
                throw PSTraceSource.NewArgumentNullException(nameof(powerShellAsPSObject));
            Collection<PSCommand> extraCommands = null;
            ReadOnlyPSMemberInfoCollection<PSPropertyInfo> properties = powerShellAsPSObject.Properties.Match(RemoteDataNameStrings.ExtraCommands);
            if (properties.Count > 0)
                extraCommands = new Collection<PSCommand>();
                foreach (PSObject extraCommandsAsPSObject in RemotingDecoder.EnumerateListProperty<PSObject>(powerShellAsPSObject, RemoteDataNameStrings.ExtraCommands))
                    PSCommand cmd = null;
                    foreach (PSObject extraCommand in RemotingDecoder.EnumerateListProperty<PSObject>(extraCommandsAsPSObject, RemoteDataNameStrings.Commands))
                        System.Management.Automation.Runspaces.Command command =
                            System.Management.Automation.Runspaces.Command.FromPSObjectForRemoting(extraCommand);
                        if (cmd == null)
                            cmd = new PSCommand(command);
                            cmd.AddCommand(command);
                    extraCommands.Add(cmd);
            PSCommand psCommand = null;
            foreach (PSObject commandAsPSObject in RemotingDecoder.EnumerateListProperty<PSObject>(powerShellAsPSObject, RemoteDataNameStrings.Commands))
                    System.Management.Automation.Runspaces.Command.FromPSObjectForRemoting(commandAsPSObject);
                if (psCommand == null)
                    psCommand = new PSCommand(command);
                    psCommand.AddCommand(command);
            bool isNested = RemotingDecoder.GetPropertyValue<bool>(powerShellAsPSObject, RemoteDataNameStrings.IsNested);
            PowerShell shell = PowerShell.Create(isNested, psCommand, extraCommands);
            shell.HistoryString = RemotingDecoder.GetPropertyValue<string>(powerShellAsPSObject, RemoteDataNameStrings.HistoryString);
            shell.RedirectShellErrorOutputPipe = RemotingDecoder.GetPropertyValue<bool>(powerShellAsPSObject, RemoteDataNameStrings.RedirectShellErrorOutputPipe);
            return shell;
            PSObject powerShellAsPSObject = RemotingEncoder.CreateEmptyPSObject();
            Version psRPVersion = RemotingEncoder.GetPSRemotingProtocolVersion(_rsConnection as RunspacePool);
            // Check if the server supports batch invocation
                if (ExtraCommands.Count > 0)
                    List<PSObject> extraCommandsAsListOfPSObjects = new List<PSObject>(ExtraCommands.Count);
                    foreach (PSCommand extraCommand in ExtraCommands)
                        PSObject obj = RemotingEncoder.CreateEmptyPSObject();
                        obj.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.Commands, CommandsAsListOfPSObjects(extraCommand.Commands, psRPVersion)));
                        extraCommandsAsListOfPSObjects.Add(obj);
                    powerShellAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.ExtraCommands, extraCommandsAsListOfPSObjects));
            List<PSObject> commandsAsListOfPSObjects = CommandsAsListOfPSObjects(Commands.Commands, psRPVersion);
            powerShellAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.Commands, commandsAsListOfPSObjects));
            powerShellAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.IsNested, this.IsNested));
            powerShellAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.HistoryString, HistoryString));
            powerShellAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.RedirectShellErrorOutputPipe, this.RedirectShellErrorOutputPipe));
            return powerShellAsPSObject;
        private static List<PSObject> CommandsAsListOfPSObjects(CommandCollection commands, Version psRPVersion)
            List<PSObject> commandsAsListOfPSObjects = new List<PSObject>(commands.Count);
                commandsAsListOfPSObjects.Add(command.ToPSObjectForRemoting(psRPVersion));
            return commandsAsListOfPSObjects;
        /// Suspends data arriving from remote session.
        internal void SuspendIncomingData()
            if (RemotePowerShell == null)
            RemotePowerShell.DataStructureHandler?.TransportManager.SuspendQueue(true);
        /// Resumes data arriving from remote session.
        internal void ResumeIncomingData()
            RemotePowerShell.DataStructureHandler?.TransportManager.ResumeQueue();
        /// Blocking call that waits until the *current remote* data
        /// queue at the transport manager is empty.  This affects only
        /// the current queue until it is empty.
        internal void WaitForServicingComplete()
            if (RemotePowerShell.DataStructureHandler != null)
                while (++count < 2 &&
                       RemotePowerShell.DataStructureHandler.TransportManager.IsServicing)
                    // Try waiting for 50 ms, then continue.
                    Threading.Thread.Sleep(50);
        internal CimInstance AsPSPowerShellPipeline()
            CimInstance c = InternalMISerializer.CreateCimInstance("PS_PowerShellPipeline");
            CimProperty instanceIdProperty = InternalMISerializer.CreateCimProperty("InstanceId",
                                                                                    this.InstanceId.ToString(),
            c.CimInstanceProperties.Add(instanceIdProperty);
            CimProperty isNestedProperty = InternalMISerializer.CreateCimProperty("IsNested",
                                                                                  this.IsNested,
            c.CimInstanceProperties.Add(isNestedProperty);
            bool addToHistoryValue = false, noInputValue = false;
            uint apartmentStateValue = 0;
            if (_worker != null)
                _worker.GetSettings(out addToHistoryValue, out noInputValue, out apartmentStateValue);
            CimProperty addToHistoryProperty = InternalMISerializer.CreateCimProperty("AddToHistory",
                                                                                      addToHistoryValue,
            c.CimInstanceProperties.Add(addToHistoryProperty);
            CimProperty noInputProperty = InternalMISerializer.CreateCimProperty("NoInput",
                                                                                 noInputValue,
            c.CimInstanceProperties.Add(noInputProperty);
            CimProperty apartmentStateProperty = InternalMISerializer.CreateCimProperty("ApartmentState",
                                                                                        apartmentStateValue,
                                                                                        Microsoft.Management.Infrastructure.CimType.UInt32);
            c.CimInstanceProperties.Add(apartmentStateProperty);
            if (this.Commands.Commands.Count > 0)
                List<CimInstance> commandInstances = new List<CimInstance>();
                foreach (var command in this.Commands.Commands)
                    commandInstances.Add(command.ToCimInstance());
                CimProperty commandsProperty = InternalMISerializer.CreateCimProperty("Commands",
                                                                                      commandInstances.ToArray(),
                c.CimInstanceProperties.Add(commandsProperty);
    public sealed class PSDataStreams
        /// PSDataStreams is the public interface to access the *Buffer properties in the PowerShell class.
        internal PSDataStreams(PowerShell powershell)
            _powershell = powershell;
        public PSDataCollection<ErrorRecord> Error
                return _powershell.ErrorBuffer;
                _powershell.ErrorBuffer = value;
        public PSDataCollection<ProgressRecord> Progress
                return _powershell.ProgressBuffer;
                _powershell.ProgressBuffer = value;
        public PSDataCollection<VerboseRecord> Verbose
                return _powershell.VerboseBuffer;
                _powershell.VerboseBuffer = value;
        public PSDataCollection<DebugRecord> Debug
                return _powershell.DebugBuffer;
                _powershell.DebugBuffer = value;
        public PSDataCollection<WarningRecord> Warning
                return _powershell.WarningBuffer;
                _powershell.WarningBuffer = value;
        public PSDataCollection<InformationRecord> Information
                return _powershell.InformationBuffer;
                _powershell.InformationBuffer = value;
        /// Removes all items from all the data streams.
        public void ClearStreams()
            this.Error.Clear();
            this.Progress.Clear();
            this.Verbose.Clear();
            this.Information.Clear();
            this.Debug.Clear();
            this.Warning.Clear();
        private readonly PowerShell _powershell;
    /// Helper class for making sure Ctrl-C stops an active powershell invocation.
    ///     powerShell = PowerShell.Create();
    ///     powerShell.AddCommand("Start-Sleep");
    ///     powerShell.AddParameter("Seconds", 10);
    ///     powerShell.Runspace = remoteRunspace;
    ///     Collection&lt;PSObject&gt; result;
    ///     using (new PowerShellStopper(context, powerShell))
    ///         result = powerShell.Invoke();
    internal class PowerShellStopper : IDisposable
        private readonly PipelineBase _pipeline;
        private readonly PowerShell _powerShell;
        private EventHandler<PipelineStateEventArgs> _eventHandler;
        internal PowerShellStopper(ExecutionContext context, PowerShell powerShell)
            ArgumentNullException.ThrowIfNull(powerShell);
            _powerShell = powerShell;
            if ((context.CurrentCommandProcessor != null) &&
                (context.CurrentCommandProcessor.CommandRuntime != null) &&
                (context.CurrentCommandProcessor.CommandRuntime.PipelineProcessor != null) &&
                (context.CurrentCommandProcessor.CommandRuntime.PipelineProcessor.LocalPipeline != null))
                _eventHandler = new EventHandler<PipelineStateEventArgs>(LocalPipeline_StateChanged);
                _pipeline = context.CurrentCommandProcessor.CommandRuntime.PipelineProcessor.LocalPipeline;
                _pipeline.StateChanged += _eventHandler;
        private void LocalPipeline_StateChanged(object sender, PipelineStateEventArgs e)
            if ((e.PipelineStateInfo.State == PipelineState.Stopping) &&
                (_powerShell.InvocationStateInfo.State == PSInvocationState.Running))
                _powerShell.Stop();
                if (_eventHandler != null)
                    _pipeline.StateChanged -= _eventHandler;
                    _eventHandler = null;

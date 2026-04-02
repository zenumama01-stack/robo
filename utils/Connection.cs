    #region Exceptions
    /// Exception thrown when state of the runspace is different from
    /// expected state of runspace.
    public class InvalidRunspaceStateException : SystemException
        /// Initializes a new instance of InvalidRunspaceStateException.
        public InvalidRunspaceStateException()
        : base
            StringUtil.Format(RunspaceStrings.InvalidRunspaceStateGeneral)
        /// Initializes a new instance of InvalidRunspaceStateException with a specified error message.
        public InvalidRunspaceStateException(string message)
        /// Initializes a new instance of the InvalidRunspaceStateException class
        public InvalidRunspaceStateException(string message, Exception innerException)
        /// Initializes a new instance of the InvalidRunspaceStateException
        /// with a specified error message and current and expected state.
        /// <param name="currentState">Current state of runspace.</param>
        /// <param name="expectedState">Expected states of runspace.</param>
        internal InvalidRunspaceStateException
            RunspaceState currentState,
            RunspaceState expectedState
            _expectedState = expectedState;
            _currentState = currentState;
        // 2005/04/20-JonN No need to implement GetObjectData
        // if all fields are static or [NonSerialized]
        /// Initializes a new instance of the <see cref="InvalidRunspaceStateException"/>
        protected InvalidRunspaceStateException(SerializationInfo info, StreamingContext context)
        /// Access CurrentState of the runspace.
        /// <remarks>This is the state of the runspace when exception was thrown.
        public RunspaceState CurrentState
                return _currentState;
                _currentState = value;
        /// Expected state of runspace by the operation which has thrown this exception.
        public RunspaceState ExpectedState
                return _expectedState;
                _expectedState = value;
        /// State of the runspace when exception was thrown.
        private RunspaceState _currentState = 0;
        /// States of the runspace expected in method which throws this exception.
        private RunspaceState _expectedState = 0;
    #endregion Exceptions
    #region Runspace state
    /// Defines various states of runspace.
    public enum RunspaceState
        /// Beginning state upon creation.
        BeforeOpen = 0,
        /// A runspace is being established.
        Opening = 1,
        /// The runspace is established and valid.
        Opened = 2,
        /// The runspace is closed or has not been established.
        Closed = 3,
        /// The runspace is being closed.
        Closing = 4,
        /// The runspace has been disconnected abnormally.
        Broken = 5,
        /// The runspace is being disconnected.
        Disconnecting = 6,
        /// The runspace is disconnected.
        Disconnected = 7,
        /// The runspace is Connecting.
        Connecting = 8
    /// These options control whether a new thread is created when a command is executed within a runspace.
    public enum PSThreadOptions
        /// Use the default options: UseNewThread for local Runspace, ReuseThread for local RunspacePool, server settings for remote Runspace and RunspacePool.
        /// Creates a new thread for each invocation.
        UseNewThread = 1,
        /// Creates a new thread for the first invocation and then re-uses
        /// that thread in subsequent invocations.
        ReuseThread = 2,
        /// Doesn't create a new thread; the execution occurs on the thread
        /// that calls Invoke. This option is not valid for asynchronous calls.
        UseCurrentThread = 3
    /// Defines type which has information about RunspaceState and
    /// Exception associated with RunspaceState.
    public sealed class RunspaceStateInfo
        /// Constructor for state changes not resulting from an error.
        /// <param name="state">The state of the runspace.</param>
        internal RunspaceStateInfo(RunspaceState state)
            : this(state, null)
        /// Constructor for state changes with an optional error.
        /// <param name="state">The state of runspace.</param>
        /// <param name="reason">A non-null exception if the state change was
        /// caused by an error, otherwise; null.
        internal RunspaceStateInfo(RunspaceState state, Exception reason)
            State = state;
            Reason = reason;
        /// Copy constructor to support cloning.
        /// <param name="runspaceStateInfo">The source
        /// RunspaceStateInfo
        internal RunspaceStateInfo(RunspaceStateInfo runspaceStateInfo)
            State = runspaceStateInfo.State;
            Reason = runspaceStateInfo.Reason;
        /// The state of the runspace.
        public RunspaceState State { get; }
        /// The reason for the state change, if caused by an error.
        /// The value of this property is non-null if the state
        /// changed due to an error. Otherwise, the value of this
        /// property is null.
        public Exception Reason { get; }
        /// Override for ToString()
            return State.ToString();
        /// Clones current object.
        /// <returns>Cloned object.</returns>
        internal RunspaceStateInfo Clone()
            return new RunspaceStateInfo(this);
    /// Defines Event arguments passed to RunspaceStateEvent handler
    /// <see cref="Runspace.StateChanged"/> event.
    public sealed class RunspaceStateEventArgs : EventArgs
        /// Constructs RunspaceStateEventArgs using RunspaceStateInfo.
        /// <param name="runspaceStateInfo">The information about
        /// current state of the runspace.</param>
        /// <exception cref="ArgumentNullException">RunspaceStateInfo is null
        internal RunspaceStateEventArgs(RunspaceStateInfo runspaceStateInfo)
            if (runspaceStateInfo == null)
                throw PSTraceSource.NewArgumentNullException(nameof(runspaceStateInfo));
            RunspaceStateInfo = runspaceStateInfo;
        /// Information about state of the runspace.
        /// This value indicates the state of the runspace after the
        /// change.
        public RunspaceStateInfo RunspaceStateInfo { get; }
    /// Enum to indicate whether a Runspace is busy or available.
    public enum RunspaceAvailability
        /// The Runspace is not been in the Opened state.
        /// The Runspace is available to execute commands.
        Available,
        /// The Runspace is available to execute nested commands.
        AvailableForNestedCommand,
        /// The Runspace is busy executing a command.
        Busy,
        /// Applies only to remote runspace case.  The remote runspace
        /// is currently in a Debugger Stop mode and requires a debugger
        /// SetDebuggerAction() call to continue.
        RemoteDebug
    /// Defines the event arguments passed to the AvailabilityChanged <see cref="Runspace.AvailabilityChanged"/> event.
    public sealed class RunspaceAvailabilityEventArgs : EventArgs
        internal RunspaceAvailabilityEventArgs(RunspaceAvailability runspaceAvailability)
            RunspaceAvailability = runspaceAvailability;
        /// Whether the Runspace is available to execute commands.
        public RunspaceAvailability RunspaceAvailability { get; }
    #endregion Runspace state
    #region Runspace capabilities
    /// Defines runspace capabilities.
    public enum RunspaceCapability
        /// Legacy capabilities for WinRM only, from Win7 timeframe.
        Default = 0x0,
        /// Runspace and remoting layer supports disconnect/connect feature.
        SupportsDisconnect = 0x1,
        /// Runspace is based on a named pipe transport.
        NamedPipeTransport = 0x2,
        /// Runspace is based on a VM socket transport.
        VMSocketTransport = 0x4,
        /// Runspace is based on SSH transport.
        SSHTransport = 0x8,
        /// Runspace is based on open custom connection/transport support.
        CustomTransport = 0x100
    /// Public interface to PowerShell Runtime. Provides APIs for creating pipelines,
    /// access session state etc.
    public abstract class Runspace : IDisposable
        private static int s_globalId;
        private readonly Stack<PowerShell> _runningPowerShells;
        private PowerShell _baseRunningPowerShell;
        /// Explicit default constructor.
        internal Runspace()
            // Create the default Runspace Id and friendly name.
            Id = System.Threading.Interlocked.Increment(ref s_globalId);
            Name = "Runspace" + Id.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            _runningPowerShells = new Stack<PowerShell>();
            // Keep track of this runspace until it is disposed.
            lock (s_syncObject)
                s_runspaceDictionary.Add(Id, new WeakReference<Runspace>(this));
        static Runspace()
            s_syncObject = new object();
            s_runspaceDictionary = new SortedDictionary<int, WeakReference<Runspace>>();
            s_globalId = 0;
        /// Used to store Runspace reference on per thread basis. Used by
        /// various PowerShell engine features to get access to TypeTable
        private static Runspace t_threadSpecificDefaultRunspace = null;
        /// Gets and sets the default Runspace used to evaluate scripts.
        /// <remarks>The Runspace used to set this property should not be shared between different threads.</remarks>
        public static Runspace DefaultRunspace
                return t_threadSpecificDefaultRunspace;
                if (value == null || !value.RunspaceIsRemote)
                    t_threadSpecificDefaultRunspace = value;
                    throw new InvalidOperationException(RunspaceStrings.RunspaceNotLocal);
        /// A PrimaryRunspace is a runspace that persists for the entire lifetime of the PowerShell session. It is only
        /// closed or disposed when the session is ending.  So when the PrimaryRunspace is closing it will trigger on-exit
        /// cleanup that includes closing any other local runspaces left open, and will allow the process to exit.
        internal static Runspace PrimaryRunspace
                return s_primaryRunspace;
                var result = Interlocked.CompareExchange<Runspace>(ref s_primaryRunspace, value, null);
                    throw new PSInvalidOperationException(RunspaceStrings.PrimaryRunspaceAlreadySet);
        private static Runspace s_primaryRunspace;
        /// Returns true if Runspace.DefaultRunspace can be used to
        /// create an instance of the PowerShell class with
        /// 'UseCurrentRunspace = true'.
        public static bool CanUseDefaultRunspace
            // can use default runspace in a thread safe manner only if
            // 1. we have a default runspace
            // 2. we recognize the type of current runspace and current pipeline
            // 3. the pipeline executes on the same thread as this method
            // we don't return "true" for
            // 2. no currently executing pipeline
            // to avoid a race condition where a pipeline is started
            // after this property getter did all the checks
                RunspaceBase runspace = Runspace.DefaultRunspace as RunspaceBase;
                    Pipeline currentPipeline = runspace.GetCurrentlyRunningPipeline();
                    LocalPipeline localPipeline = currentPipeline as LocalPipeline;
                    if ((localPipeline != null) && (localPipeline.NestedPipelineExecutionThread != null))
                            (localPipeline.NestedPipelineExecutionThread.ManagedThreadId
                            == Environment.CurrentManagedThreadId);
        internal const ApartmentState DefaultApartmentState = ApartmentState.Unknown;
        /// ApartmentState of the thread used to execute commands within this Runspace.
        /// Any updates to the value of this property must be done before the Runspace is opened
        /// <exception cref="InvalidRunspaceStateException">
        /// An attempt to change this property was made after opening the Runspace
        public ApartmentState ApartmentState
                return this.apartmentState;
                if (this.RunspaceStateInfo.State != RunspaceState.BeforeOpen)
                    throw new InvalidRunspaceStateException(StringUtil.Format(RunspaceStrings.ChangePropertyAfterOpen));
                this.apartmentState = value;
        private ApartmentState apartmentState = Runspace.DefaultApartmentState;
        /// This property determines whether a new thread is create for each invocation.
        /// The thread options cannot be changed to the requested value
        public abstract PSThreadOptions ThreadOptions
        /// Return version of this runspace.
        public abstract Version Version
        /// Return whether the Runspace is Remote
        /// We can determine this by whether the runspace is an implementation of LocalRunspace
        /// or infer it from whether the ConnectionInfo property is null
        /// If it happens to be an instance of a LocalRunspace, but has a non-null ConnectionInfo
        /// we declare it to be remote.
        public bool RunspaceIsRemote
                return this is not LocalRunspace && ConnectionInfo != null;
        /// Retrieve information about current state of the runspace.
        public abstract RunspaceStateInfo RunspaceStateInfo
        /// Gets the current availability of the Runspace.
        public abstract RunspaceAvailability RunspaceAvailability
        /// InitialSessionState information for this runspace.
        public abstract InitialSessionState InitialSessionState
        /// Get unique id for this instance of runspace. It is primarily used
        /// for logging purposes.
            // This id is also used to identify proxy and remote runspace objects.
            // We need to set this when reconstructing a remote runspace to connect
            // to an existing remote runspace.
        } = Guid.NewGuid();
        /// Gets execution context.
        /// <exception cref="InvalidRunspaceStateException">Runspace is not opened.
        internal ExecutionContext ExecutionContext
                return GetExecutionContext;
        /// Skip user profile on engine initialization.
        internal bool SkipUserProfile { get; set; } = false;
        /// Connection information for remote Runspaces, null for local Runspaces.
        public abstract RunspaceConnectionInfo ConnectionInfo { get; }
        /// ConnectionInfo originally supplied by the user.
        public abstract RunspaceConnectionInfo OriginalConnectionInfo { get; }
        /// Manager for JobSourceAdapters registered in this runspace.
        public abstract JobManager JobManager { get; }
        /// DisconnectedOn property applies to remote runspaces that have
        /// been disconnected.
        public DateTime? DisconnectedOn
        /// ExpiresOn property applies to remote runspaces that have been
        /// disconnected.
        public DateTime? ExpiresOn
        /// Gets and sets a friendly name for the Runspace.
        /// Gets the Runspace Id.
        /// Gets and sets a boolean indicating whether the runspace has a
        /// debugger attached with <c>Debug-Runspace</c>.
        public bool IsRemoteDebuggerAttached { get; internal set; }
        /// Returns protocol version that the remote server uses for PS remoting.
        internal Version GetRemoteProtocolVersion()
            Version remoteProtocolVersionDeclaredByServer;
            bool isServerDeclarationValid = PSPrimitiveDictionary.TryPathGet(
                this.GetApplicationPrivateData(),
                out remoteProtocolVersionDeclaredByServer,
                PSVersionInfo.PSVersionTableName,
                PSVersionInfo.PSRemotingProtocolVersionName);
            if (isServerDeclarationValid)
                return remoteProtocolVersionDeclaredByServer;
                return RemotingConstants.ProtocolVersion;
        /// Engine activity id (for ETW tracing)
        internal Guid EngineActivityId { get; set; } = Guid.Empty;
        /// Returns a read only runspace dictionary.
        internal static ReadOnlyDictionary<int, WeakReference<Runspace>> RunspaceDictionary
                    return new ReadOnlyDictionary<int, WeakReference<Runspace>>(new Dictionary<int, WeakReference<Runspace>>(s_runspaceDictionary));
        private static readonly SortedDictionary<int, WeakReference<Runspace>> s_runspaceDictionary;
        private static readonly object s_syncObject;
        /// Returns a read only list of runspaces.
        internal static IReadOnlyList<Runspace> RunspaceList
                List<Runspace> runspaceList = new List<Runspace>();
                    foreach (var item in s_runspaceDictionary.Values)
                        if (item.TryGetTarget(out runspace))
                            runspaceList.Add(runspace);
                return new ReadOnlyCollection<Runspace>(runspaceList);
        #region events
        /// Event raised when RunspaceState changes.
        public abstract event EventHandler<RunspaceStateEventArgs> StateChanged;
        /// Event raised when the availability of the Runspace changes.
        public abstract event EventHandler<RunspaceAvailabilityEventArgs> AvailabilityChanged;
        /// Returns true if there are any subscribers to the AvailabilityChanged event.
        internal abstract bool HasAvailabilityChangedSubscribers
        /// Raises the AvailabilityChanged event.
        protected abstract void OnAvailabilityChanged(RunspaceAvailabilityEventArgs e);
        /// Used to raise the AvailabilityChanged event when the state of the currently executing pipeline changes.
        /// The possible pipeline states are
        ///     NotStarted
        ///     Running
        ///     Disconnected
        ///     Stopping
        ///     Stopped
        ///     Completed
        ///     Failed
        internal void UpdateRunspaceAvailability(PipelineState pipelineState, bool raiseEvent, Guid? cmdInstanceId = null)
            RunspaceAvailability oldAvailability = this.RunspaceAvailability;
            switch (oldAvailability)
                // Because of disconnect/connect support runspace availability can now transition
                // in and out of "None" state.
                case RunspaceAvailability.None:
                    switch (pipelineState)
                        case PipelineState.Running:
                            this.RunspaceAvailability = RunspaceAvailability.Busy;
                            // Otherwise no change.
                case RunspaceAvailability.Available:
                        case PipelineState.Disconnected:
                            this.RunspaceAvailability = Runspaces.RunspaceAvailability.None;
                case RunspaceAvailability.AvailableForNestedCommand:
                        case PipelineState.Completed: // a nested pipeline caused the host to exit nested prompt
                            this.RunspaceAvailability = (this.InNestedPrompt || (_runningPowerShells.Count > 1)) ?
                                RunspaceAvailability.AvailableForNestedCommand : RunspaceAvailability.Available;
                            break; // no change in the availability
                case RunspaceAvailability.Busy:
                case RunspaceAvailability.RemoteDebug:
                            if (oldAvailability == Runspaces.RunspaceAvailability.RemoteDebug)
                                this.RunspaceAvailability = RunspaceAvailability.RemoteDebug;
                                this.RunspaceAvailability = RunspaceAvailability.None;
                        case PipelineState.Stopping:
                        case PipelineState.Completed:
                        case PipelineState.Stopped:
                        case PipelineState.Failed:
                            if (this.InNestedPrompt
                                || (this is not RemoteRunspace && this.Debugger.InBreakpoint))
                                this.RunspaceAvailability = RunspaceAvailability.AvailableForNestedCommand;
                                RemoteRunspace remoteRunspace = this as RemoteRunspace;
                                RemoteDebugger remoteDebugger = (remoteRunspace != null) ? remoteRunspace.Debugger as RemoteDebugger : null;
                                Internal.ConnectCommandInfo remoteCommand = remoteRunspace?.RemoteCommand;
                                if (((pipelineState == PipelineState.Completed) || (pipelineState == PipelineState.Failed) ||
                                    ((pipelineState == PipelineState.Stopped) && (this.RunspaceStateInfo.State == RunspaceState.Opened)))
                                    && (remoteCommand != null) && (cmdInstanceId != null) && (remoteCommand.CommandId == cmdInstanceId))
                                    // Completed, Failed, and Stopped with Runspace.Opened states are command finish states and we know
                                    // that the command is finished on the server.
                                    // Setting ConnectCommands to null indicates that the runspace is free to run other
                                    // commands.
                                    remoteRunspace.RunspacePool.RemoteRunspacePoolInternal.ConnectCommands = null;
                                    remoteCommand = null;
                                    if ((remoteDebugger != null) && (pipelineState == PipelineState.Stopped))
                                        // Notify remote debugger of a stop in case the stop occurred while command was in debug stop.
                                        remoteDebugger.OnCommandStopped();
                                Pipeline currentPipeline = this.GetCurrentlyRunningPipeline();
                                RemotePipeline remotePipeline = currentPipeline as RemotePipeline;
                                Guid? pipeLineCmdInstance = (remotePipeline != null && remotePipeline.PowerShell != null) ? remotePipeline.PowerShell.InstanceId : (Guid?)null;
                                if (currentPipeline == null)
                                    // A runspace is available:
                                    //  - if there is no currently running pipeline
                                    //    and for remote runspaces:
                                    //    - if there is no remote command associated with it.
                                    //    - if the remote runspace pool is marked as available for connection.
                                    if (remoteCommand == null)
                                            this.RunspaceAvailability =
                                                remoteRunspace.RunspacePool.RemoteRunspacePoolInternal.AvailableForConnection ?
                                                RunspaceAvailability.Available : Runspaces.RunspaceAvailability.Busy;
                                            this.RunspaceAvailability = RunspaceAvailability.Available;
                                else if ((cmdInstanceId != null) && (pipeLineCmdInstance != null) && (cmdInstanceId == pipeLineCmdInstance))
                                else // a nested pipeline completed, but the parent pipeline is still running
                                        this.RunspaceAvailability = Runspaces.RunspaceAvailability.RemoteDebug;
                                    else if ((currentPipeline.PipelineStateInfo.State == PipelineState.Running) || (_runningPowerShells.Count > 1))
                                        // Either the current pipeline is running or there are other nested commands to run in the Runspace.
                        case PipelineState.Running: // this can happen if a nested pipeline is created without entering a nested prompt
                    Diagnostics.Assert(false, "Invalid RunspaceAvailability");
            if (raiseEvent && this.RunspaceAvailability != oldAvailability)
                OnAvailabilityChanged(new RunspaceAvailabilityEventArgs(this.RunspaceAvailability));
        /// Used to update the runspace availability when the state of the currently executing PowerShell instance changes.
        /// The possible invocation states are
        internal void UpdateRunspaceAvailability(PSInvocationState invocationState, bool raiseEvent, Guid cmdInstanceId)
            switch (invocationState)
                case PSInvocationState.NotStarted:
                    UpdateRunspaceAvailability(PipelineState.NotStarted, raiseEvent, cmdInstanceId);
                case PSInvocationState.Running:
                    UpdateRunspaceAvailability(PipelineState.Running, raiseEvent, cmdInstanceId);
                case PSInvocationState.Completed:
                    UpdateRunspaceAvailability(PipelineState.Completed, raiseEvent, cmdInstanceId);
                case PSInvocationState.Failed:
                    UpdateRunspaceAvailability(PipelineState.Failed, raiseEvent, cmdInstanceId);
                case PSInvocationState.Stopping:
                    UpdateRunspaceAvailability(PipelineState.Stopping, raiseEvent, cmdInstanceId);
                case PSInvocationState.Stopped:
                    UpdateRunspaceAvailability(PipelineState.Stopped, raiseEvent, cmdInstanceId);
                case PSInvocationState.Disconnected:
                    UpdateRunspaceAvailability(PipelineState.Disconnected, raiseEvent, cmdInstanceId);
                    Diagnostics.Assert(false, "Invalid PSInvocationState");
        /// Used to update the runspace availability event when the state of the runspace changes.
        /// The possible runspace states are:
        ///     BeforeOpen
        ///     Opening
        ///     Opened
        ///     Closed
        ///     Closing
        ///     Broken
        protected void UpdateRunspaceAvailability(RunspaceState runspaceState, bool raiseEvent)
            Internal.ConnectCommandInfo remoteCommand = null;
            bool remoteDebug = false;
                remoteCommand = remoteRunspace.RemoteCommand;
                RemoteDebugger remoteDebugger = remoteRunspace.Debugger as RemoteDebugger;
                remoteDebug = (remoteDebugger != null) && remoteDebugger.IsRemoteDebug;
                    switch (runspaceState)
                            if (remoteDebug)
                                this.RunspaceAvailability = (remoteCommand == null && GetCurrentlyRunningPipeline() == null) ?
                                    RunspaceAvailability.Available : RunspaceAvailability.Busy;
        /// Used to update the runspace availability from Enter/ExitNestedPrompt and the debugger.
        internal void UpdateRunspaceAvailability(RunspaceAvailability availability, bool raiseEvent)
            this.RunspaceAvailability = availability;
        internal void RaiseAvailabilityChangedEvent(RunspaceAvailability availability)
            OnAvailabilityChanged(new RunspaceAvailabilityEventArgs(availability));
        #endregion events
        #region Public static methods
        /// Queries the server for disconnected runspaces and creates an array of runspace
        /// objects associated with each disconnected runspace on the server.  Each
        /// runspace object in the returned array is in the Disconnected state and can be
        /// connected to the server by calling the Connect() method on the runspace.
        /// <param name="connectionInfo">Connection object for the target server.</param>
        /// <returns>Array of Runspace objects each in the Disconnected state.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Runspaces")]
        public static Runspace[] GetRunspaces(RunspaceConnectionInfo connectionInfo)
            return GetRunspaces(connectionInfo, null, null);
        /// <param name="host">Client host object.</param>
        public static Runspace[] GetRunspaces(RunspaceConnectionInfo connectionInfo, PSHost host)
            return GetRunspaces(connectionInfo, host, null);
        /// <param name="typeTable">TypeTable object.</param>
        public static Runspace[] GetRunspaces(RunspaceConnectionInfo connectionInfo, PSHost host, TypeTable typeTable)
            return RemoteRunspace.GetRemoteRunspaces(connectionInfo, host, typeTable);
        /// Returns a single disconnected Runspace object targeted to the remote computer and remote
        /// session as specified by the connection, session Id, and command Id parameters.
        /// <param name="sessionId">Id of a disconnected remote session on the target server.</param>
        /// <param name="commandId">Optional Id of a disconnected command running in the disconnected remote session on the target server.</param>
        /// <param name="host">Optional client host object.</param>
        /// <param name="typeTable">Optional TypeTable object.</param>
        /// <returns>Disconnected runspace corresponding to the provided session Id.</returns>
        public static Runspace GetRunspace(RunspaceConnectionInfo connectionInfo, Guid sessionId, Guid? commandId, PSHost host, TypeTable typeTable)
            return RemoteRunspace.GetRemoteRunspace(connectionInfo, sessionId, commandId, host, typeTable);
        #region public Disconnect-Connect methods
        /// Disconnects the runspace synchronously.
        /// Disconnects the remote runspace and any running command from the server
        /// machine.  Any data generated by the running command on the server is
        /// cached on the server machine.  This runspace object goes to the disconnected
        /// state.  This object can be reconnected to the server by calling the
        /// Connect() method.
        /// If the remote runspace on the server remains disconnected for the IdleTimeout
        /// value (as defined in the WSManConnectionInfo object) then it is closed and
        /// torn down on the server.
        /// RunspaceState is not Opened.
        public abstract void Disconnect();
        /// Disconnects the runspace asynchronously.
        public abstract void DisconnectAsync();
        /// Connects the runspace to its remote counterpart synchronously.
        /// Connects the runspace object to its corresponding runspace on the target
        /// server machine.  The target server machine is identified by the connection
        /// object passed in during construction.  The remote runspace is identified
        /// by the internal runspace Guid value.
        /// RunspaceState is not Disconnected.
        public abstract void Connect();
        /// Connects a runspace to its remote counterpart asynchronously.
        public abstract void ConnectAsync();
        /// Creates a PipeLine object in the disconnected state for the currently disconnected
        /// remote running command associated with this runspace.
        /// <returns>Pipeline object in disconnected state.</returns>
        public abstract Pipeline CreateDisconnectedPipeline();
        /// Creates a PowerShell object in the disconnected state for the currently disconnected
        /// <returns>PowerShell object in disconnected state.</returns>
        public abstract PowerShell CreateDisconnectedPowerShell();
        /// Returns Runspace capabilities.
        /// <returns>RunspaceCapability.</returns>
        public abstract RunspaceCapability GetCapabilities();
        /// Opens the runspace synchronously. Runspace must be opened before it can be used.
        /// RunspaceState is not BeforeOpen
        public abstract void Open();
        /// Open the runspace Asynchronously.
        public abstract void OpenAsync();
        /// Close the runspace synchronously.
        /// Attempts to execute pipelines after a call to close will fail.
        /// RunspaceState is BeforeOpen or Opening
        public abstract void Close();
        /// Close the runspace Asynchronously.
        /// Attempts to execute pipelines after a call to
        /// close will fail.
        public abstract void CloseAsync();
        /// Create an empty pipeline.
        /// <returns>An empty pipeline.</returns>
        public abstract Pipeline CreatePipeline();
        /// Creates a pipeline for specified command string.
        /// <param name="command">A valid command string.</param>
        /// A pipeline pre-filled with a <see cref="Command"/> object for specified command parameter.
        /// command is null
        public abstract Pipeline CreatePipeline(string command);
        /// Create a pipeline from a command string.
        /// <param name="addToHistory">If true command is added to history.</param>
        public abstract Pipeline CreatePipeline(string command, bool addToHistory);
        /// Creates a nested pipeline.
        /// Nested pipelines are needed for nested prompt scenario. Nested
        /// prompt requires that we execute new pipelines( child pipelines)
        /// while current pipeline (lets call it parent pipeline) is blocked.
        public abstract Pipeline CreateNestedPipeline();
        /// A pipeline pre-filled with Command specified in commandString.
        public abstract Pipeline CreateNestedPipeline(string command, bool addToHistory);
        /// Returns the currently executing pipeline,  or null if no pipeline is executing.
        internal abstract Pipeline GetCurrentlyRunningPipeline();
        /// Private data to be used by applications built on top of PowerShell.
        /// Local runspace is created with application private data set to an empty <see cref="PSPrimitiveDictionary"/>.
        /// Remote runspace gets its application private data from the server (set when creating a remote runspace pool)
        /// Calling this method on a remote runspace will block until the data is received from the server.
        /// The server will send application private data before reaching <see cref="RunspacePoolState.Opened"/> state.
        /// Runspaces that are part of a <see cref="RunspacePool"/> inherit application private data from the pool.
        public abstract PSPrimitiveDictionary GetApplicationPrivateData();
        /// A method that runspace pools can use to propagate application private data into runspaces.
        /// <param name="applicationPrivateData"></param>
        internal abstract void SetApplicationPrivateData(PSPrimitiveDictionary applicationPrivateData);
        /// Push a running PowerShell onto the stack.
        /// <param name="ps">PowerShell.</param>
        internal void PushRunningPowerShell(PowerShell ps)
            Dbg.Assert(ps != null, "Caller should not pass in null reference.");
                _runningPowerShells.Push(ps);
                if (_runningPowerShells.Count == 1)
                    _baseRunningPowerShell = ps;
        /// Pop the currently running PowerShell from stack.
        /// <returns>PowerShell.</returns>
        internal PowerShell PopRunningPowerShell()
                int count = _runningPowerShells.Count;
                    if (count == 1)
                        _baseRunningPowerShell = null;
                    return _runningPowerShells.Pop();
        internal PowerShell GetCurrentBasePowerShell()
            return _baseRunningPowerShell;
        #region SessionStateProxy
        /// Gets session state proxy.
        public SessionStateProxy SessionStateProxy
                return GetSessionStateProxy();
        internal abstract SessionStateProxy GetSessionStateProxy();
        #endregion SessionStateProxy
        /// Disposes this runspace instance. Dispose will close the runspace if not closed already.
        /// Protected dispose which can be overridden by derived classes.
                s_runspaceDictionary.Remove(Id);
        /// Gets the execution context.
        internal abstract ExecutionContext GetExecutionContext
        /// Returns true if the internal host is in a nested prompt.
        internal abstract bool InNestedPrompt
        /// Gets the debugger.
        public virtual Debugger Debugger
                var context = GetExecutionContext;
                return context?.Debugger;
        /// InternalDebugger.
        internal Debugger InternalDebugger
        /// Gets the event manager.
        public abstract PSEventManager Events
#if !CORECLR // Transaction Not Supported On CSS
        /// Sets the base transaction for the runspace; any transactions created on this runspace will be nested to this instance.
        /// <param name="transaction">The base transaction</param>
        /// <remarks>This overload uses RollbackSeverity.Error; i.e. the transaction will be rolled back automatically on a non-terminating error or worse</remarks>
        public void SetBaseTransaction(System.Transactions.CommittableTransaction transaction)
            this.ExecutionContext.TransactionManager.SetBaseTransaction(transaction, RollbackSeverity.Error);
        /// <param name="severity">The severity of error that causes PowerShell to automatically rollback the transaction</param>
        public void SetBaseTransaction(System.Transactions.CommittableTransaction transaction, RollbackSeverity severity)
            this.ExecutionContext.TransactionManager.SetBaseTransaction(transaction, severity);
        /// Clears the transaction set by SetBaseTransaction()
        public void ClearBaseTransaction()
            this.ExecutionContext.TransactionManager.ClearBaseTransaction();
        /// Resets the variable table for the runspace to the default state.
        public virtual void ResetRunspaceState()
            throw new NotImplementedException("ResetRunspaceState");
        // Used for pipeline id generation.
        private long _pipelineIdSeed;
        // Generate pipeline id unique to this runspace
        internal long GeneratePipelineId()
            return System.Threading.Interlocked.Increment(ref _pipelineIdSeed);
    /// This class provides subset of functionality provided by
    /// session state.
    public class SessionStateProxy
        internal SessionStateProxy()
        private readonly RunspaceBase _runspace;
        internal SessionStateProxy(RunspaceBase runspace)
            Dbg.Assert(runspace != null, "Caller should validate the parameter");
        /// name is null
        /// Runspace is not open.
        /// Another SessionStateProxy call or another pipeline is in progress.
        public virtual void SetVariable(string name, object value)
            _runspace.SetVariable(name, value);
        public virtual object GetVariable(string name)
            if (name.Equals(string.Empty))
            return _runspace.GetVariable(name);
        /// Get the list of applications out of session state.
        public virtual List<string> Applications
                return _runspace.Applications;
        /// Get the list of scripts out of session state.
        public virtual List<string> Scripts
                return _runspace.Scripts;
        /// Get the APIs to access drives out of session state.
        public virtual DriveManagementIntrinsics Drive
            get { return _runspace.Drive; }
        /// Get/Set the language mode out of session state.
        public virtual PSLanguageMode LanguageMode
            get { return _runspace.LanguageMode; }
            set { _runspace.LanguageMode = value; }
        /// Get the module info out of session state.
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Shipped this way in V2 before becoming virtual.")]
        public virtual PSModuleInfo Module
            get { return _runspace.Module; }
        /// Get the APIs to access paths and locations out of session state.
        public virtual PathIntrinsics Path
            get { return _runspace.PathIntrinsics; }
        /// Get the APIs to access a provider out of session state.
        public virtual CmdletProviderManagementIntrinsics Provider
            get { return _runspace.Provider; }
        /// Get the APIs to access variables out of session state.
        public virtual PSVariableIntrinsics PSVariable
            get { return _runspace.PSVariable; }
        /// Get the APIs to build script blocks and execute script out of session state.
        public virtual CommandInvocationIntrinsics InvokeCommand
            get { return _runspace.InvokeCommand; }
        /// Gets the instance of the provider interface APIs out of session state.
        public virtual ProviderIntrinsics InvokeProvider
            get { return _runspace.InvokeProvider; }

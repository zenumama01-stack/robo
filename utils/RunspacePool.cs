using PSHost = System.Management.Automation.Host.PSHost;
    /// Exception thrown when state of the runspace pool is different from
    /// expected state of runspace pool.
    public class InvalidRunspacePoolStateException : SystemException
        /// Creates a new instance of InvalidRunspacePoolStateException class.
        public InvalidRunspacePoolStateException()
            StringUtil.Format(RunspacePoolStrings.InvalidRunspacePoolStateGeneral)
        public InvalidRunspacePoolStateException(string message)
        public InvalidRunspacePoolStateException(string message, Exception innerException)
        /// Initializes a new instance of the InvalidRunspacePoolStateException
        /// <param name="currentState">Current state of runspace pool.</param>
        /// <param name="expectedState">Expected state of the runspace pool.</param>
        internal InvalidRunspacePoolStateException
            RunspacePoolState currentState,
            RunspacePoolState expectedState
        /// The <see cref="SerializationInfo"/> that holds
        /// the serialized object data about the exception being thrown.
        /// The <see cref="StreamingContext"/> that contains
        /// contextual information about the source or destination.
        InvalidRunspacePoolStateException(SerializationInfo info, StreamingContext context)
        /// Access CurrentState of the runspace pool.
        /// This is the state of the runspace pool when exception was thrown.
        public RunspacePoolState CurrentState
        /// Expected state of runspace pool by the operation which has thrown
        /// this exception.
        public RunspacePoolState ExpectedState
        /// Converts the current to an InvalidRunspaceStateException.
        internal InvalidRunspaceStateException ToInvalidRunspaceStateException()
            InvalidRunspaceStateException exception = new InvalidRunspaceStateException(
                RunspaceStrings.InvalidRunspaceStateGeneral,
            exception.CurrentState = RunspacePoolStateToRunspaceState(this.CurrentState);
            exception.ExpectedState = RunspacePoolStateToRunspaceState(this.ExpectedState);
        /// Converts a RunspacePoolState to a RunspaceState.
        private static RunspaceState RunspacePoolStateToRunspaceState(RunspacePoolState state)
                case RunspacePoolState.BeforeOpen:
                    return RunspaceState.BeforeOpen;
                case RunspacePoolState.Opening:
                    return RunspaceState.Opening;
                case RunspacePoolState.Opened:
                    return RunspaceState.Opened;
                case RunspacePoolState.Closed:
                    return RunspaceState.Closed;
                case RunspacePoolState.Closing:
                    return RunspaceState.Closing;
                case RunspacePoolState.Broken:
                    return RunspaceState.Broken;
                case RunspacePoolState.Disconnecting:
                    return RunspaceState.Disconnecting;
                case RunspacePoolState.Disconnected:
                    return RunspaceState.Disconnected;
                case RunspacePoolState.Connecting:
                    return RunspaceState.Connecting;
                    Diagnostics.Assert(false, "Unexpected RunspacePoolState");
        /// State of the runspace pool when exception was thrown.
        private readonly RunspacePoolState _currentState = 0;
        /// State of the runspace pool expected in method which throws this exception.
        private readonly RunspacePoolState _expectedState = 0;
    #region State
    /// Defines various states of a runspace pool.
    public enum RunspacePoolState
        /// A RunspacePool is being created.
        /// The RunspacePool is created and valid.
        /// The RunspacePool is closed.
        /// The RunspacePool is being closed.
        /// The RunspacePool has been disconnected abnormally.
        /// The RunspacePool is being disconnected.
        /// The RunspacePool has been disconnected.
        /// The RunspacePool is being connected.
        Connecting = 8,
    /// Event arguments passed to runspacepool state change handlers
    /// <see cref="RunspacePool.StateChanged"/> event.
    public sealed class RunspacePoolStateChangedEventArgs : EventArgs
        internal RunspacePoolStateChangedEventArgs(RunspacePoolState state)
            RunspacePoolStateInfo = new RunspacePoolStateInfo(state, null);
        /// <param name="stateInfo"></param>
        internal RunspacePoolStateChangedEventArgs(RunspacePoolStateInfo stateInfo)
            RunspacePoolStateInfo = stateInfo;
        /// Gets the stateinfo of RunspacePool when this event occurred.
        public RunspacePoolStateInfo RunspacePoolStateInfo { get; }
    /// Event arguments passed to RunspaceCreated event of RunspacePool.
    internal sealed class RunspaceCreatedEventArgs : EventArgs
        internal RunspaceCreatedEventArgs(Runspace runspace)
        internal Runspace Runspace { get; }
    #region RunspacePool Availability
    /// Defines runspace pool availability.
    public enum RunspacePoolAvailability
        /// RunspacePool is not in the Opened state.
        /// RunspacePool is Opened and available to accept commands.
        /// RunspacePool on the server is connected to another
        /// client and is not available to this client for connection
        /// or running commands.
        Busy = 2
    #region RunspacePool Capabilities
    public enum RunspacePoolCapability
        /// No additional capabilities beyond a default runspace.
        /// Runspacepool and remoting layer supports disconnect/connect feature.
        SupportsDisconnect = 0x1
    #region AsyncResult
    /// Encapsulated the AsyncResult for pool's Open/Close async operations.
    internal sealed class RunspacePoolAsyncResult : AsyncResult
        /// Instance Id of the pool creating this instance
        /// <param name="isCalledFromOpenAsync">
        /// true if AsyncResult monitors Async Open.
        internal RunspacePoolAsyncResult(Guid ownerId, AsyncCallback callback, object state,
            bool isCalledFromOpenAsync)
            IsAssociatedWithAsyncOpen = isCalledFromOpenAsync;
        /// True if AsyncResult monitors Async Open.
        internal bool IsAssociatedWithAsyncOpen { get; }
    /// Encapsulated the results of a RunspacePool.BeginGetRunspace method.
    internal sealed class GetRunspaceAsyncResult : AsyncResult
        private bool _isActive;
        internal GetRunspaceAsyncResult(Guid ownerId, AsyncCallback callback, object state)
            _isActive = true;
        /// Gets the runspace that is assigned to the async operation.
        /// This can be null if the async Get operation is not completed.
        internal Runspace Runspace { get; set; }
        /// Gets or sets a value indicating whether this operation
        /// is active or not.
        internal bool IsActive
                    return _isActive;
                    _isActive = value;
        /// Marks the async operation as completed and releases
        /// waiting threads.
        /// This is not used
        /// This method is called from a thread pool thread to release
        /// the async operation.
        internal void DoComplete(object state)
            SetAsCompleted(null);
    #region RunspacePool
    /// Public interface which supports pooling PowerShell Runspaces.
    public sealed class RunspacePool : IDisposable
        private readonly RunspacePoolInternal _internalPool;
        private event EventHandler<RunspacePoolStateChangedEventArgs> InternalStateChanged = null;
        private event EventHandler<PSEventArgs> InternalForwardEvent = null;
        private event EventHandler<RunspaceCreatedEventArgs> InternalRunspaceCreated = null;
        /// Constructor which creates a RunspacePool using the
        /// supplied <paramref name="configuration"/>,
        internal RunspacePool(int minRunspaces, int maxRunspaces, PSHost host)
            // Currently we support only Local Runspace Pool..
            // this needs to be changed once remote runspace pool
            // is implemented
            _internalPool = new RunspacePoolInternal(minRunspaces, maxRunspaces, host);
        /// supplied <paramref name="initialSessionState"/>,
        /// InitialSessionState object to use when creating a new Runspace.
        /// initialSessionState is null.
        internal RunspacePool(int minRunspaces, int maxRunspaces,
            _internalPool = new RunspacePoolInternal(minRunspaces,
        /// Construct a runspace pool object.
        /// <param name="minRunspaces">Min runspaces.</param>
        /// <param name="maxRunspaces">Max runspaces.</param>
        /// <param name="typeTable">TypeTable.</param>
        /// <param name="applicationArguments">App arguments.</param>
        /// <param name="connectionInfo">Connection information.</param>
        /// <param name="name">Session name.</param>
        internal RunspacePool(
            int minRunspaces,
            int maxRunspaces,
            TypeTable typeTable,
            PSPrimitiveDictionary applicationArguments,
            RunspaceConnectionInfo connectionInfo,
            string name = null)
            _internalPool = new RemoteRunspacePoolInternal(
                minRunspaces,
                maxRunspaces,
                typeTable,
                applicationArguments,
                connectionInfo,
            IsRemote = true;
        /// Creates a runspace pool object in a disconnected state that is
        /// ready to connect to a remote runspace pool session specified by
        /// the instanceId parameter.
        /// <param name="isDisconnected">Indicates whether the shell/runspace pool is disconnected.</param>
        /// <param name="instanceId">Identifies a remote runspace pool session to connect to.</param>
        /// <param name="name">Friendly name for runspace pool.</param>
        /// <param name="connectCommands">Runspace pool running commands information.</param>
        /// <param name="connectionInfo">Connection information of remote server.</param>
        /// <param name="host">PSHost object.</param>
        /// <param name="typeTable">TypeTable used for serialization/deserialization of remote objects.</param>
            bool isDisconnected,
            ConnectCommandInfo[] connectCommands,
            TypeTable typeTable)
            // Disconnect-Connect semantics are currently only supported in WSMan transport.
            if (connectionInfo is not WSManConnectionInfo)
            _internalPool = new RemoteRunspacePoolInternal(instanceId, name, isDisconnected, connectCommands,
                connectionInfo, host, typeTable);
                return _internalPool.InstanceId;
        /// Gets a boolean which describes if the runspace pool is disposed.
                return _internalPool.IsDisposed;
        /// Gets State of the current runspace pool.
        public RunspacePoolStateInfo RunspacePoolStateInfo
                return _internalPool.RunspacePoolStateInfo;
        /// Gets the InitialSessionState object that this pool uses
        /// to create the runspaces.
        public InitialSessionState InitialSessionState
                return _internalPool.InitialSessionState;
        /// Connection information for remote RunspacePools, null for local RunspacePools.
        public RunspaceConnectionInfo ConnectionInfo
                return _internalPool.ConnectionInfo;
        /// Specifies how often unused runspaces are disposed.
        public TimeSpan CleanupInterval
            get { return _internalPool.CleanupInterval; }
            set { _internalPool.CleanupInterval = value; }
        /// Returns runspace pool availability.
        public RunspacePoolAvailability RunspacePoolAvailability
            get { return _internalPool.RunspacePoolAvailability; }
        /// Event raised when RunspacePoolState changes.
        public event EventHandler<RunspacePoolStateChangedEventArgs> StateChanged
                    bool firstEntry = (InternalStateChanged == null);
                    InternalStateChanged += value;
                    if (firstEntry)
                        // call any event handlers on this object, replacing the
                        // internalPool sender with 'this' since receivers
                        // are expecting a RunspacePool.
                        _internalPool.StateChanged += OnStateChanged;
                    InternalStateChanged -= value;
                    if (InternalStateChanged == null)
                        _internalPool.StateChanged -= OnStateChanged;
        /// Handle internal Pool state changed events.
        private void OnStateChanged(object source, RunspacePoolStateChangedEventArgs args)
            if (ConnectionInfo is NewProcessConnectionInfo)
                NewProcessConnectionInfo connectionInfo = ConnectionInfo as NewProcessConnectionInfo;
                if (connectionInfo.Process != null &&
                    (args.RunspacePoolStateInfo.State == RunspacePoolState.Opened ||
                     args.RunspacePoolStateInfo.State == RunspacePoolState.Broken))
                    connectionInfo.Process.RunspacePool = this;
            // call any event handlers on this, replacing the
            // are expecting a RunspacePool
            InternalStateChanged.SafeInvoke(this, args);
        /// Event raised when one of the runspaces in the pool forwards an event to this instance.
        internal event EventHandler<PSEventArgs> ForwardEvent
                    bool firstEntry = InternalForwardEvent == null;
                    InternalForwardEvent += value;
                        _internalPool.ForwardEvent += OnInternalPoolForwardEvent;
                    InternalForwardEvent -= value;
                    if (InternalForwardEvent == null)
                        _internalPool.ForwardEvent -= OnInternalPoolForwardEvent;
        /// Pass thru of the ForwardEvent event from the internal pool.
        private void OnInternalPoolForwardEvent(object sender, PSEventArgs e)
            OnEventForwarded(e);
        private void OnEventForwarded(PSEventArgs e)
            InternalForwardEvent?.Invoke(this, e);
        /// Event raised when a new Runspace is created by the pool.
        internal event EventHandler<RunspaceCreatedEventArgs> RunspaceCreated
                    bool firstEntry = (InternalRunspaceCreated == null);
                    InternalRunspaceCreated += value;
                        _internalPool.RunspaceCreated += OnRunspaceCreated;
                    InternalRunspaceCreated -= value;
                    if (InternalRunspaceCreated == null)
                        _internalPool.RunspaceCreated -= OnRunspaceCreated;
        /// Handle internal Pool RunspaceCreated events.
        private void OnRunspaceCreated(object source, RunspaceCreatedEventArgs args)
            InternalRunspaceCreated.SafeInvoke(this, args);
        #region Public static methods.
        /// Queries the server for disconnected runspace pools and creates an array of runspace
        /// pool objects associated with each disconnected runspace pool on the server.  Each
        /// runspace pool object in the returned array is in the Disconnected state and can be
        /// connected to the server by calling the Connect() method on the runspace pool.
        /// <returns>Array of RunspacePool objects each in the Disconnected state.</returns>
        public static RunspacePool[] GetRunspacePools(RunspaceConnectionInfo connectionInfo)
            return GetRunspacePools(connectionInfo, null, null);
        public static RunspacePool[] GetRunspacePools(RunspaceConnectionInfo connectionInfo, PSHost host)
            return GetRunspacePools(connectionInfo, host, null);
        public static RunspacePool[] GetRunspacePools(RunspaceConnectionInfo connectionInfo, PSHost host, TypeTable typeTable)
            return RemoteRunspacePoolInternal.GetRemoteRunspacePools(connectionInfo, host, typeTable);
        #region Public Disconnect-Connect API
        /// Disconnects the runspace pool synchronously.  Runspace pool must be in Opened state.
        public void Disconnect()
            _internalPool.Disconnect();
        /// Disconnects the runspace pool asynchronously.  Runspace pool must be in Opened state.
        /// <param name="callback">An AsyncCallback to call once the BeginClose completes.</param>
        /// <param name="state">A user supplied state to call the callback with.</param>
        public IAsyncResult BeginDisconnect(AsyncCallback callback, object state)
            return _internalPool.BeginDisconnect(callback, state);
        /// Waits for the pending asynchronous BeginDisconnect to complete.
        /// <param name="asyncResult">Asynchronous call result object.</param>
        public void EndDisconnect(IAsyncResult asyncResult)
            _internalPool.EndDisconnect(asyncResult);
        /// Connects the runspace pool synchronously.  Runspace pool must be in disconnected state.
        public void Connect()
            _internalPool.Connect();
        /// Connects the runspace pool asynchronously.  Runspace pool must be in disconnected state.
        /// <param name="callback"></param>
        public IAsyncResult BeginConnect(AsyncCallback callback, object state)
            return _internalPool.BeginConnect(callback, state);
        /// Waits for the pending asynchronous BeginConnect to complete.
        public void EndConnect(IAsyncResult asyncResult)
            _internalPool.EndConnect(asyncResult);
        /// Creates an array of PowerShell objects that are in the Disconnected state for
        /// all currently disconnected running commands associated with this runspace pool.
        public Collection<PowerShell> CreateDisconnectedPowerShells()
            return _internalPool.CreateDisconnectedPowerShells(this);
        /// Returns RunspacePool capabilities.
        /// <returns>RunspacePoolCapability.</returns>
        public RunspacePoolCapability GetCapabilities()
            return _internalPool.GetCapabilities();
        /// Sets the maximum number of Runspaces that can be active concurrently
        /// in the pool. All requests above that number remain queued until
        /// runspaces become available.
        /// The maximum number of runspaces in the pool.
        /// true if the change is successful; otherwise, false.
        /// You cannot set the number of runspaces to a number smaller than
        /// the minimum runspaces.
        public bool SetMaxRunspaces(int maxRunspaces)
            return _internalPool.SetMaxRunspaces(maxRunspaces);
        /// Retrieves the maximum number of runspaces the pool maintains.
        /// The maximum number of runspaces in the pool
        public int GetMaxRunspaces()
            return _internalPool.GetMaxRunspaces();
        /// Sets the minimum number of Runspaces that the pool maintains
        /// in anticipation of new requests.
        /// The minimum number of runspaces in the pool.
        /// You cannot set the number of idle runspaces to a number smaller than
        /// 1 or greater than maximum number of active runspaces.
        public bool SetMinRunspaces(int minRunspaces)
            return _internalPool.SetMinRunspaces(minRunspaces);
        /// Retrieves the minimum number of runspaces the pool maintains.
        /// The minimum number of runspaces in the pool
        public int GetMinRunspaces()
            return _internalPool.GetMinRunspaces();
        /// Retrieves the number of runspaces available at the time of calling
        /// this method.
        /// The number of available runspace in the pool.
        public int GetAvailableRunspaces()
            return _internalPool.GetAvailableRunspaces();
        /// Opens the runspacepool synchronously. RunspacePool must
        /// be opened before it can be used.
        /// <exception cref="InvalidRunspacePoolStateException">
        /// RunspacePoolState is not BeforeOpen
        public void Open()
            _internalPool.Open();
        /// Opens the RunspacePool asynchronously. RunspacePool must
        /// To get the exceptions that might have occurred, call
        /// EndOpen.
        /// A AsyncCallback to call once the BeginOpen completes.
        /// An AsyncResult object to monitor the state of the async
        /// operation.
        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
            return _internalPool.BeginOpen(callback, state);
        /// Waits for the pending asynchronous BeginOpen to complete.
        /// asyncResult object was not created by calling BeginOpen
        /// on this runspacepool instance.
        /// RunspacePoolState is not BeforeOpen.
        /// TODO: Behavior if EndOpen is called multiple times.
        public void EndOpen(IAsyncResult asyncResult)
            _internalPool.EndOpen(asyncResult);
        /// Closes the RunspacePool and cleans all the internal
        /// resources. This will close all the runspaces in the
        /// runspacepool and release all the async operations
        /// waiting for a runspace. If the pool is already closed
        /// or broken or closing this will just return.
        /// Cannot close the RunspacePool because RunspacePool is
        /// in Closing state.
            _internalPool.Close();
        /// Closes the RunspacePool asynchronously and cleans all the internal
        /// A AsyncCallback to call once the BeginClose completes.
        public IAsyncResult BeginClose(AsyncCallback callback, object state)
            return _internalPool.BeginClose(callback, state);
        /// Waits for the pending asynchronous BeginClose to complete.
        /// asyncResult object was not created by calling BeginClose
        public void EndClose(IAsyncResult asyncResult)
            _internalPool.EndClose(asyncResult);
            _internalPool.Dispose();
        /// Remote runspace pool gets its application private data from the server (when creating the remote runspace pool)
        /// Calling this method on a remote runspace pool will block until the data is received from the server.
        public PSPrimitiveDictionary GetApplicationPrivateData()
            return _internalPool.GetApplicationPrivateData();
        #region Internal API
        /// This property determines whether a new thread is created for each invocation.
        /// Any updates to the value of this property must be done before the RunspacePool is opened
        /// An attempt to change this property was made after opening the RunspacePool
        public PSThreadOptions ThreadOptions
                return _internalPool.ThreadOptions;
                if (this.RunspacePoolStateInfo.State != RunspacePoolState.BeforeOpen)
                    throw new InvalidRunspacePoolStateException(RunspacePoolStrings.ChangePropertyAfterOpen);
                _internalPool.ThreadOptions = value;
        /// ApartmentState of the thread used to execute commands within this RunspacePool.
                return _internalPool.ApartmentState;
                _internalPool.ApartmentState = value;
        /// Gets Runspace asynchronously from the runspace pool. The caller
        /// will get notified with the runspace using <paramref name="callback"/>
        /// A AsyncCallback to call once the runspace is available.
        /// An IAsyncResult object to track the status of the Async operation.
        internal IAsyncResult BeginGetRunspace(
            AsyncCallback callback, object state)
            return _internalPool.BeginGetRunspace(callback, state);
        /// Cancels the pending asynchronous BeginGetRunspace operation.
        internal void CancelGetRunspace(IAsyncResult asyncResult)
            _internalPool.CancelGetRunspace(asyncResult);
        /// Waits for the pending asynchronous BeginGetRunspace to complete.
        /// asyncResult object was not created by calling BeginGetRunspace
        internal Runspace EndGetRunspace(IAsyncResult asyncResult)
            return _internalPool.EndGetRunspace(asyncResult);
        /// Releases a Runspace to the pool. If pool is closed, this
        /// will be a no-op.
        /// Runspace to release to the pool.
        /// <paramref name="runspace"/> is null.
        /// Cannot release the runspace to this pool as the runspace
        /// doesn't belong to this pool.
        /// Only opened runspaces can be released back to the pool.
        internal void ReleaseRunspace(Runspace runspace)
            _internalPool.ReleaseRunspace(runspace);
        /// Indicates whether the RunspacePool is a remote one.
        internal bool IsRemote { get; } = false;
        /// RemoteRunspacePoolInternal associated with this
        /// runspace pool.
        internal RemoteRunspacePoolInternal RemoteRunspacePoolInternal
                if (_internalPool is RemoteRunspacePoolInternal)
                    return (RemoteRunspacePoolInternal)_internalPool;
        internal void AssertPoolIsOpen()
            _internalPool.AssertPoolIsOpen();

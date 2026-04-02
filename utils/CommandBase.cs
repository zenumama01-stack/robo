    /// Defines members used by Cmdlets.
    /// All Cmdlets must derive from
    /// <see cref="System.Management.Automation.Cmdlet"/>.
    /// Only use <see cref="System.Management.Automation.Internal.InternalCommand"/>
    /// as a subclass of
    /// Do not attempt to create instances of
    /// <see cref="System.Management.Automation.Internal.InternalCommand"/>
    /// independently, or to derive other classes than
    /// <see cref="System.Management.Automation.Cmdlet"/> from
    /// <see cref="System.Management.Automation.Internal.InternalCommand"/>.
    /// <seealso cref="System.Management.Automation.Cmdlet"/>
    /// These are the Cmdlet members which are also used by other
    /// non-public command types.
    /// Ideally this would be an internal class, but C# does not support
    /// public classes deriving from internal classes.
    [DebuggerDisplay("Command = {_commandInfo}")]
    public abstract class InternalCommand
        internal ICommandRuntime commandRuntime;
        /// Initializes the new instance of Cmdlet class.
        /// The only constructor is internal, so outside users cannot create
        /// an instance of this class.
        internal InternalCommand()
            this.CommandInfo = null;
        #region internal_members
        /// Allows you to access the calling token for this command invocation...
        internal IScriptExtent InvocationExtent { get; set; }
        private InvocationInfo _myInvocation = null;
        /// Return the invocation data object for this command.
        /// <value>The invocation object for this command.</value>
        internal InvocationInfo MyInvocation
            get { return _myInvocation ??= new InvocationInfo(this); }
        /// Represents the current pipeline object under consideration.
        internal PSObject currentObjectInPipeline = AutomationNull.Value;
        /// Gets or sets the current pipeline object under consideration.
        internal PSObject CurrentPipelineObject
                return currentObjectInPipeline;
                currentObjectInPipeline = value;
        /// Internal helper. Interface that should be used for interaction with host.
        internal PSHost PSHostInternal
            get { return _CBhost; }
        private PSHost _CBhost;
        /// Internal helper to get to SessionState.
        internal SessionState InternalState
        private SessionState _state;
        /// Internal helper. Indicates whether stop has been requested on this command.
        internal bool IsStopping
                MshCommandRuntime mcr = this.commandRuntime as MshCommandRuntime;
                return (mcr != null && mcr.IsStopping);
        /// Gets the CancellationToken that is signaled when the pipeline is stopping.
        internal CancellationToken StopToken => commandRuntime is MshCommandRuntime mcr
            ? mcr.PipelineProcessor.PipelineStopToken
            : default;
        /// The information about the command.
        private CommandInfo _commandInfo;
        /// Gets or sets the command information for the command.
        internal CommandInfo CommandInfo
            get { return _commandInfo; }
            set { _commandInfo = value; }
        #endregion internal_members
        #region public_properties
        /// Gets or sets the execution context.
        /// may not be set to null
        internal ExecutionContext Context
                return _context;
                    throw PSTraceSource.NewArgumentNullException("Context");
                _context = value;
                Diagnostics.Assert(_context.EngineHostInterface is InternalHost, "context.EngineHostInterface is not an InternalHost");
                _CBhost = (InternalHost)_context.EngineHostInterface;
                // Construct the session state API set from the new context
                _state = new SessionState(_context.EngineSessionState);
        private ExecutionContext _context;
        /// This property tells you if you were being invoked inside the runspace or
        /// if it was an external request.
        public CommandOrigin CommandOrigin
            get { return CommandOriginInternal; }
        internal CommandOrigin CommandOriginInternal = CommandOrigin.Internal;
        #endregion public_properties
        #region Override
        /// When overridden in the derived class, performs initialization
        /// of command execution.
        internal virtual void DoBeginProcessing()
        /// When overridden in the derived class, performs execution
        /// of the command.
        internal virtual void DoProcessRecord()
        /// When overridden in the derived class, performs clean-up
        /// after the command execution.
        internal virtual void DoEndProcessing()
        /// running code within the command. It should interrupt BeginProcessing,
        /// ProcessRecord, and EndProcessing.
        internal virtual void DoStopProcessing()
        /// When overridden in the derived class, performs clean-up after the command execution.
        internal virtual void DoCleanResource()
        #endregion Override
        /// Throws if the pipeline is stopping.
        /// <exception cref="System.Management.Automation.PipelineStoppedException"></exception>
        internal void ThrowIfStopping()
            if (IsStopping)
        #region Dispose
        /// IDisposable implementation
        /// When the command is complete, release the associated members.
        /// Using InternalDispose instead of Dispose pattern because this
        /// interface was shipped in PowerShell V1 and 3rd cmdlets indirectly
        /// derive from this interface. If we depend on Dispose() and 3rd
        /// party cmdlets do not call base.Dispose (which is the case), we
        /// will still end up having this leak.
        internal void InternalDispose(bool isDisposing)
            _myInvocation = null;
            _state = null;
            _commandInfo = null;
            _context = null;
    #region NativeArgumentPassingStyle
    /// Defines the different native command argument parsing options.
    public enum NativeArgumentPassingStyle
        /// <summary>Use legacy argument parsing via ProcessStartInfo.Arguments.</summary>
        Legacy = 0,
        /// <summary>Use new style argument passing via ProcessStartInfo.ArgumentList.</summary>
        Standard = 1,
        /// Use specific to Windows passing style which is Legacy for selected files on Windows, but
        /// Standard for everything else. This is the default behavior for Windows.
        Windows = 2
    #endregion NativeArgumentPassingStyle
    #region ErrorView
    /// Defines the potential ErrorView options.
    public enum ErrorView
        /// <summary>Existing all red multi-line output.</summary>
        NormalView = 0,
        /// <summary>Only show category information.</summary>
        CategoryView = 1,
        /// <summary>Concise shows more information on the context of the error or just the message if not a script or parser error.</summary>
        ConciseView = 2,
        /// <summary>Detailed will leverage Get-Error to get much more detailed information for the error.</summary>
        DetailedView = 3,
    #endregion ErrorView
    #region ActionPreference
    /// Defines the Action Preference options.  These options determine
    /// what will happen when a particular type of event occurs.
    /// For example, setting shell variable ErrorActionPreference to "Stop"
    /// will cause the command to stop when an otherwise non-terminating
    /// error occurs.
    public enum ActionPreference
        /// <summary>Ignore this event and continue</summary>
        SilentlyContinue = 0,
        /// <summary>Stop the command</summary>
        Stop = 1,
        /// <summary>Handle this event as normal and continue</summary>
        Continue = 2,
        /// <summary>Ask whether to stop or continue</summary>
        Inquire = 3,
        /// <summary>Ignore the event completely (not even logging it to the target stream)</summary>
        Ignore = 4,
        /// <summary>Reserved for future use.</summary>
        Suspend = 5,
        /// <summary>Enter the debugger.</summary>
        Break = 6,
    } // enum ActionPreference
    #endregion ActionPreference
    #region ConfirmImpact
    /// Defines the ConfirmImpact levels.  These levels describe
    /// the "destructiveness" of an action, and thus the degree of
    /// important that the user confirm the action.
    /// For example, setting the read-only flag on a file might be Low,
    /// and reformatting a disk might be High.
    /// These levels are also used in $ConfirmPreference to describe
    /// which operations should be confirmed.  Operations with ConfirmImpact
    /// equal to or greater than $ConfirmPreference are confirmed.
    /// Operations with ConfirmImpact.None are never confirmed, and
    /// no operations are confirmed when $ConfirmPreference is ConfirmImpact.None
    /// (except when explicitly requested with -Confirm).
        /// <summary>There is never any need to confirm this action.</summary>
        /// This action only needs to be confirmed when the
        /// user has requested that low-impact changes must be confirmed.
        /// This action should be confirmed in most scenarios where
        /// confirmation is requested.
        /// This action is potentially highly "destructive" and should be
        /// confirmed by default unless otherwise specified.
    #endregion ConfirmImpact
    /// Defines members and overrides used by Cmdlets.
    /// All Cmdlets must derive from <see cref="System.Management.Automation.Cmdlet"/>.
    /// There are two ways to create a Cmdlet: by deriving from the Cmdlet base class, and by
    /// deriving from the PSCmdlet base class.  The Cmdlet base class is the primary means by
    /// which users create their own Cmdlets.  Extending this class provides support for the most
    /// common functionality, including object output and record processing.
    /// If your Cmdlet requires access to the PowerShell Runtime (for example, variables in the session state,
    /// access to the host, or information about the current Cmdlet Providers,) then you should instead
    /// derive from the PSCmdlet base class.
    /// The public members defined by the PSCmdlet class are not designed to be overridden; instead, they
    /// provided access to different aspects of the PowerShell runtime.
    /// In both cases, users should first develop and implement an object model to accomplish their
    /// task, extending the Cmdlet or PSCmdlet classes only as a thin management layer.
    /// <seealso cref="System.Management.Automation.Internal.InternalCommand"/>
    public abstract partial class PSCmdlet : Cmdlet
        private ProviderIntrinsics _invokeProvider = null;
        /// Gets the host interaction APIs.
        public PSHost Host
                using (PSTransactionManager.GetEngineProtectionScope())
                    return PSHostInternal;
        /// Gets the instance of session state for the current runspace.
        public SessionState SessionState
                    return this.InternalState;
        /// Gets the event manager for the current runspace.
        public PSEventManager Events
                    return this.Context.Events;
        /// Repository for jobs.
        public JobRepository JobRepository
                    return ((LocalRunspace)this.Context.CurrentRunspace).JobRepository;
        /// Manager for JobSourceAdapters registered.
        public JobManager JobManager
                    return ((LocalRunspace)this.Context.CurrentRunspace).JobManager;
        /// Repository for runspaces.
        internal RunspaceRepository RunspaceRepository
                return ((LocalRunspace)this.Context.CurrentRunspace).RunspaceRepository;
        /// Gets the instance of the provider interface APIs for the current runspace.
        public ProviderIntrinsics InvokeProvider
                    return _invokeProvider ??= new ProviderIntrinsics(this);
        #region Provider wrappers
        /// <Content contentref="System.Management.Automation.PathIntrinsics.CurrentProviderLocation" />
        public PathInfo CurrentProviderLocation(string providerId)
                if (providerId == null)
                    throw PSTraceSource.NewArgumentNullException(nameof(providerId));
                PathInfo result = SessionState.Path.CurrentProviderLocation(providerId);
                Diagnostics.Assert(result != null, "DataStoreAdapterCollection.GetNamespaceCurrentLocation() should " + "throw an exception, not return null");
        /// <Content contentref="System.Management.Automation.PathIntrinsics.GetUnresolvedProviderPathFromPSPath" />
        public string GetUnresolvedProviderPathFromPSPath(string path)
                return SessionState.Path.GetUnresolvedProviderPathFromPSPath(path);
        /// <Content contentref="System.Management.Automation.PathIntrinsics.GetResolvedProviderPathFromPSPath" />
        public Collection<string> GetResolvedProviderPathFromPSPath(string path, out ProviderInfo provider)
                return SessionState.Path.GetResolvedProviderPathFromPSPath(path, out provider);
        #endregion Provider wrappers
        /// Initializes the new instance of PSCmdlet class.
        /// Only subclasses of <see cref="System.Management.Automation.Cmdlet"/>
        /// can be created.
        protected PSCmdlet()
        #region public_methods
        #region PSVariable APIs
        /// <Content contentref="System.Management.Automation.VariableIntrinsics.GetValue" />
        public object GetVariableValue(string name)
                return this.SessionState.PSVariable.GetValue(name);
        public object GetVariableValue(string name, object defaultValue)
                return this.SessionState.PSVariable.GetValue(name, defaultValue);
        #endregion PSVariable APIs
        #region Parameter methods
        #endregion Parameter methods
        #endregion public_methods

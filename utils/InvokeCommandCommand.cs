    /// This cmdlet executes a specified script block on one or more
    /// remote machines. The expression or command, as they will be
    /// interchangeably called, need to be contained in a script
    /// block. This is to ensure two things:
    ///       1. The expression that the user has entered is
    ///          syntactically correct (its compiled)
    ///       2. The scriptblock can be converted to a powershell
    ///          object before transmitting it to the remote end
    ///          so that it can be run on constrained runspaces in
    ///          the no language mode
    /// In general, the command script block is executed as if
    /// the user had typed it at the command line. The output of the
    /// command is the output of the cmdlet. However, since
    /// invoke-command is a cmdlet, it will unravel its output:
    ///     - if the command outputs an empty array, invoke-command
    ///     will output $null
    ///     - if the command outputs a single-element array, invoke-command
    ///     will output that single element.
    ///     Additionally, the command will be run on a remote system.
    /// This cmdlet can be called in the following different ways:
    /// Execute a command in a remote machine by specifying the command
    /// and machine name
    ///     invoke-command -Command {get-process} -computername "server1"
    /// Execute a command in a set of remote machines by specifying the
    /// command and the list of machines
    ///     $servers = 1..10 | ForEach-Object {"Server${_}"}
    ///     invoke-command -command {get-process} -computername $servers
    /// Create a new runspace and use it to execute a command on a remote machine
    ///     $runspace = New-PSSession -computername "Server1"
    ///     $credential = get-credential "user01"
    ///     invoke-command -command {get-process} -Session $runspace -credential $credential
    /// complete uri for the machines
    ///     $uri = "http://hostedservices.microsoft.com/someservice"
    ///     invoke-command -command { get-mail } - uri $uri
    /// Create a collection of runspaces and use it to execute a command on a set
    /// of remote machines
    ///     $serveruris = 1..8 | ForEach-Object {"http://Server${_}/"}
    ///     $runspaces = New-PSSession -URI $serveruris
    ///     invoke-command -command {get-process} -Session $runspaces
    /// The cmdlet can also be invoked in the asynchronous mode.
    ///     invoke-command -command {get-process} -computername $servers -asjob
    /// When the -AsJob switch is used, the cmdlet will emit an PSJob Object.
    /// The user can then use the other job cmdlets to work with this object
    /// Note there are two types of errors:
    ///     1. Remote invocation errors
    ///     2. Local errors.
    /// Both types of errors will be available when the user invokes
    /// a receive operation.
    /// The PSJob object has its own throttling mechanism.
    /// The result object will be stored in a global cache. If a user wants to
    /// retrieve data from the result object the user should be able to do so
    /// using the Receive-PSJob cmdlet
    /// The following needs to be noted about exception/error reporting in this
    /// cmdlet:
    ///     The exception objects that are thrown by underlying layers will be
    ///     written as errors, to avoid stopping the entire cmdlet in case of
    ///     multi-computername or multi-Session usage (for consistency, this
    ///     is true even when done using one computername or runspace)
    /// Only one expression may be executed at a time in any single runspace.
    /// Attempts to invoke an expression on a runspace that is already executing
    /// an expression shall return an error with ErrorCategory ResourceNotAvailable
    /// and notify the user that the runspace is currently busy.
    /// Some additional notes:
    /// - invoke-command issues a single scriptblock to the computer or
    /// runspace. If a runspace is specified and a command is already running
    /// in that runspace, then the second command will fail
    /// - The files necessary to execute the command (cmdlets, scripts, data
    /// files, etc) must be present on the remote system; the cmdlet is not
    /// responsible for copying them over
    /// - The entire input stream is collected and sent to the remote system
    /// before execution of the command begins (no input streaming)
    /// - Input shall be available as $input.  Remote Runspaces must reference
    /// $input explicitly (input will not automatically be available)
    /// - Output from the command streams back to the client as it is
    /// available
    /// - Ctrl-C and pause/resume are supported; the client will send a
    /// message to the remote powershell instance.
    /// - By default if no -credential is specified, the host will impersonate
    /// the current user on the client when executing the command
    /// - The standard output of invoke-command is the output of the
    /// last element of the remote pipeline, with some extra properties added
    /// - If -Shell is not specified, then the value of the environment
    /// variable DEFAULTREMOTESHELLNAME is used. If this is not set, then
    /// "Microsoft.PowerShell" is used.
    [Cmdlet(VerbsLifecycle.Invoke, "Command", DefaultParameterSetName = InvokeCommandCommand.InProcParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096789", RemotingCapability = RemotingCapability.OwnedByCommand)]
    public class InvokeCommandCommand : PSExecutionCmdlet, IDisposable
                   ParameterSetName = InvokeCommandCommand.SessionParameterSet)]
                   ParameterSetName = InvokeCommandCommand.FilePathSessionParameterSet)]
        public override PSSession[] Session
        /// This parameter represents the address(es) of the remote
        /// computer(s). The following formats are supported:
        ///      (a) Computer name
        ///      (b) IPv4 address : 132.3.4.5
        ///      (c) IPv6 address: 3ffe:8311:ffff:f70f:0:5efe:172.30.162.18.
                   ParameterSetName = InvokeCommandCommand.ComputerNameParameterSet)]
                   ParameterSetName = InvokeCommandCommand.FilePathComputerNameParameterSet)]
        public override string[] ComputerName
                return base.ComputerName;
                base.ComputerName = value;
                   ParameterSetName = InvokeCommandCommand.UriParameterSet)]
                   ParameterSetName = InvokeCommandCommand.FilePathUriParameterSet)]
        [Parameter(ValueFromPipelineByPropertyName = true, Mandatory = true,
                   ParameterSetName = InvokeCommandCommand.VMIdParameterSet)]
                   ParameterSetName = InvokeCommandCommand.VMNameParameterSet)]
                   ParameterSetName = InvokeCommandCommand.FilePathVMIdParameterSet)]
                   ParameterSetName = InvokeCommandCommand.FilePathVMNameParameterSet)]
        public override PSCredential Credential
                return base.Credential;
                base.Credential = value;
        [Parameter(ParameterSetName = InvokeCommandCommand.ComputerNameParameterSet)]
        [Parameter(ParameterSetName = InvokeCommandCommand.FilePathComputerNameParameterSet)]
        [Parameter(ParameterSetName = InvokeCommandCommand.SSHHostParameterSet)]
        public override int Port
                return base.Port;
                base.Port = value;
        public override SwitchParameter UseSSL
                return base.UseSSL;
                base.UseSSL = value;
        /// For WSMan session:
        /// For VM/Container sessions:
        /// If this parameter is not specified then no configuration is used.
                   ParameterSetName = InvokeCommandCommand.ContainerIdParameterSet)]
                   ParameterSetName = InvokeCommandCommand.FilePathContainerIdParameterSet)]
        public override string ConfigurationName
                return base.ConfigurationName;
                base.ConfigurationName = value;
        public override string ApplicationName
                return base.ApplicationName;
                base.ApplicationName = value;
        [Parameter(ParameterSetName = InvokeCommandCommand.SessionParameterSet)]
        [Parameter(ParameterSetName = InvokeCommandCommand.UriParameterSet)]
        [Parameter(ParameterSetName = InvokeCommandCommand.FilePathSessionParameterSet)]
        [Parameter(ParameterSetName = InvokeCommandCommand.FilePathUriParameterSet)]
        [Parameter(ParameterSetName = InvokeCommandCommand.VMIdParameterSet)]
        [Parameter(ParameterSetName = InvokeCommandCommand.VMNameParameterSet)]
        [Parameter(ParameterSetName = InvokeCommandCommand.ContainerIdParameterSet)]
        [Parameter(ParameterSetName = InvokeCommandCommand.FilePathVMIdParameterSet)]
        [Parameter(ParameterSetName = InvokeCommandCommand.FilePathVMNameParameterSet)]
        [Parameter(ParameterSetName = InvokeCommandCommand.FilePathContainerIdParameterSet)]
        /// connect to and create runspace for.
        public override Uri[] ConnectionUri
                return base.ConnectionUri;
                base.ConnectionUri = value;
        /// Specifies if the cmdlet needs to be run asynchronously.
        [Parameter(ParameterSetName = InvokeCommandCommand.SSHHostHashParameterSet)]
        [Parameter(ParameterSetName = InvokeCommandCommand.FilePathSSHHostParameterSet)]
        [Parameter(ParameterSetName = InvokeCommandCommand.FilePathSSHHostHashParameterSet)]
                return _asjob;
                _asjob = value;
        private bool _asjob = false;
        /// Specifies that after the command is invoked on a remote computer the
        /// remote session should be disconnected.
        [Alias("Disconnected")]
        public SwitchParameter InDisconnectedSession
            get { return InvokeAndDisconnect; }
            set { InvokeAndDisconnect = value; }
        /// Specifies the name of the returned session when the InDisconnectedSession switch
        public string[] SessionName
            get { return DisconnectedSessionName; }
            set { DisconnectedSessionName = value; }
        /// Hide/Show computername of the remote objects.
        [Alias("HCN")]
        public SwitchParameter HideComputerName
            get { return _hideComputerName; }
            set { _hideComputerName = value; }
        private bool _hideComputerName;
        /// Friendly name for the job object if AsJob is used.
        public string JobName
                    _asjob = true;
        /// The script block that the user has specified in the
        /// cmdlet. This will be converted to a powershell before
        /// its actually sent to the remote end.
                   ParameterSetName = InvokeCommandCommand.InProcParameterSet)]
                   ParameterSetName = InvokeCommandCommand.SSHHostParameterSet)]
                   ParameterSetName = InvokeCommandCommand.SSHHostHashParameterSet)]
        [Alias("Command")]
        public override ScriptBlock ScriptBlock
                return base.ScriptBlock;
                base.ScriptBlock = value;
        /// When executing a scriptblock in the current session, tell the cmdlet not to create a new scope.
        [Parameter(ParameterSetName = InvokeCommandCommand.InProcParameterSet)]
        public SwitchParameter NoNewScope { get; set; }
                   ParameterSetName = FilePathComputerNameParameterSet)]
                   ParameterSetName = FilePathSessionParameterSet)]
                   ParameterSetName = FilePathUriParameterSet)]
                   ParameterSetName = FilePathVMIdParameterSet)]
                   ParameterSetName = FilePathVMNameParameterSet)]
                   ParameterSetName = FilePathContainerIdParameterSet)]
                   ParameterSetName = FilePathSSHHostParameterSet)]
                   ParameterSetName = FilePathSSHHostHashParameterSet)]
        public override string FilePath
                return base.FilePath;
                base.FilePath = value;
        public override SwitchParameter AllowRedirection
                return base.AllowRedirection;
                base.AllowRedirection = value;
        /// Extended Session Options for controlling the session creation. Use
        /// "New-WSManSessionOption" cmdlet to supply value for this parameter.
        public override PSSessionOption SessionOption
                return base.SessionOption;
                base.SessionOption = value;
        /// Authentication mechanism to authenticate the user.
                return base.Authentication;
                base.Authentication = value;
        /// When set and in loopback scenario (localhost) this enables creation of WSMan
        /// host process with the user interactive token, allowing PowerShell script network access,
        /// i.e., allows going off box.  When this property is true and a PSSession is disconnected,
        /// reconnection is allowed only if reconnecting from a PowerShell session on the same box.
        public override SwitchParameter EnableNetworkAccess
            get { return base.EnableNetworkAccess; }
            set { base.EnableNetworkAccess = value; }
        /// When set, PowerShell process inside container will be launched with
        /// high privileged account.
        /// Otherwise (default case), PowerShell process inside container will be launched
        /// with low privileged account.
        public override SwitchParameter RunAsAdministrator
            get { return base.RunAsAdministrator; }
            set { base.RunAsAdministrator = value; }
        #region SSH Parameters
        /// Host name for an SSH remote connection.
            ParameterSetName = InvokeCommandCommand.FilePathSSHHostParameterSet)]
        public override string[] HostName
            get { return base.HostName; }
            set { base.HostName = value; }
        /// User Name.
        public override string UserName
            get { return base.UserName; }
            set { base.UserName = value; }
        /// Key Path.
        [Alias("IdentityFilePath")]
        public override string KeyFilePath
            get { return base.KeyFilePath; }
            set { base.KeyFilePath = value; }
        /// Gets and sets a value for the SSH subsystem to use for the remote connection.
        public override string Subsystem
            get { return base.Subsystem; }
            set { base.Subsystem = value; }
        /// Gets and sets a value in milliseconds that limits the time allowed for an SSH connection to be established.
        public override int ConnectingTimeout
            get { return base.ConnectingTimeout; }
            set { base.ConnectingTimeout = value; }
        /// This parameter specifies that SSH is used to establish the remote
        /// connection and act as the remoting transport.  By default WinRM is used
        /// as the remoting transport.  Using the SSH transport requires that SSH is
        /// installed and PowerShell remoting is enabled on both client and remote machines.
        [Parameter(ParameterSetName = PSRemotingBaseCmdlet.SSHHostParameterSet)]
        [ValidateSet("true")]
        public override SwitchParameter SSHTransport
            get { return base.SSHTransport; }
            set { base.SSHTransport = value; }
        /// Hashtable array containing SSH connection parameters for each remote target
        ///   ComputerName  (Alias: HostName)           (required)
        ///   UserName                                  (optional)
        ///   KeyFilePath   (Alias: IdentityFilePath)   (optional)
        [Parameter(ParameterSetName = PSRemotingBaseCmdlet.SSHHostHashParameterSet, Mandatory = true)]
        [Parameter(ParameterSetName = InvokeCommandCommand.FilePathSSHHostHashParameterSet, Mandatory = true)]
        public override Hashtable[] SSHConnection
        /// Hashtable containing options to be passed to OpenSSH.
        public override Hashtable Options
                base.Options = value;
        #region Remote Debug Parameters
        /// When selected this parameter causes a debugger Step-Into action for each running remote session.
        public virtual SwitchParameter RemoteDebug
        /// Creates the helper classes for the specified
            if (this.InvokeAndDisconnect && _asjob)
                // The -AsJob and -InDisconnectedSession parameter switches are mutually exclusive.
                throw new InvalidOperationException(RemotingErrorIdStrings.AsJobAndDisconnectedError);
            if (MyInvocation.BoundParameters.ContainsKey(nameof(SessionName)) && !this.InvokeAndDisconnect)
                throw new InvalidOperationException(RemotingErrorIdStrings.SessionNameWithoutInvokeDisconnected);
            // Adjust RemoteDebug value based on current state
            var hostDebugger = GetHostDebugger();
            if (hostDebugger == null)
                // Do not allow RemoteDebug if there is no host debugger available.  Otherwise script will not respond indefinitely.
                RemoteDebug = false;
            else if (hostDebugger.IsDebuggerSteppingEnabled)
                // If host debugger is in step-in mode then always make RemoteDebug true
                RemoteDebug = true;
            // Checking session's availability and reporting errors in early stage, unless '-AsJob' is specified.
            // When '-AsJob' is specified, Invoke-Command should return a job object without throwing error, even
            // if the session is not in available state -- this is the PSv3 behavior and we should not break it.
            if (!_asjob && (ParameterSetName.Equals(InvokeCommandCommand.SessionParameterSet) ||
                ParameterSetName.Equals(InvokeCommandCommand.FilePathSessionParameterSet)))
                long localPipelineId =
                    ((LocalRunspace)this.Context.CurrentRunspace).GetCurrentlyRunningPipeline().InstanceId;
                // Check for sessions in invalid state for running commands.
                List<PSSession> availableSessions = new List<PSSession>();
                foreach (var session in Session)
                    if (session.Runspace.RunspaceStateInfo.State != RunspaceState.Opened)
                        // Session not in Opened state.
                        string msg = StringUtil.Format(RemotingErrorIdStrings.ICMInvalidSessionState,
                            session.Name, session.InstanceId, session.ComputerName, session.Runspace.RunspaceStateInfo.State);
                            new InvalidRunspaceStateException(msg),
                            "InvokeCommandCommandInvalidSessionState",
                            session));
                    else if (session.Runspace.RunspaceAvailability != RunspaceAvailability.Available)
                        // Check to see if this is a steppable pipeline case.
                        RemoteRunspace remoteRunspace = session.Runspace as RemoteRunspace;
                        if ((remoteRunspace != null) &&
                            (remoteRunspace.RunspaceAvailability == RunspaceAvailability.Busy) &&
                            (remoteRunspace.IsAnotherInvokeCommandExecuting(this, localPipelineId)))
                            // Valid steppable pipeline session.
                            availableSessions.Add(session);
                            // Session not Available.
                            string msg = StringUtil.Format(RemotingErrorIdStrings.ICMInvalidSessionAvailability,
                                session.Name, session.InstanceId, session.ComputerName, session.Runspace.RunspaceAvailability);
                                "InvokeCommandCommandInvalidSessionAvailability",
                if (availableSessions.Count == 0)
                    throw new PSInvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.ICMNoValidRunspaces));
                if (availableSessions.Count < Session.Length)
                    Session = availableSessions.ToArray();
            if (ParameterSetName.Equals(InvokeCommandCommand.InProcParameterSet))
                if (FilePath != null)
                    ScriptBlock = GetScriptBlockFromFile(FilePath, false);
                if (this.MyInvocation.ExpectingInput)
                    if (!ScriptBlock.IsUsingDollarInput())
                            _steppablePipeline = ScriptBlock.GetSteppablePipeline(CommandOrigin.Internal, ArgumentList);
                            _steppablePipeline.Begin(this);
                            // ignore exception and don't do any streaming if can't convert to steppable pipeline
            if (string.IsNullOrEmpty(ConfigurationName))
                if ((ParameterSetName == InvokeCommandCommand.ComputerNameParameterSet) ||
                    (ParameterSetName == InvokeCommandCommand.UriParameterSet) ||
                    (ParameterSetName == InvokeCommandCommand.FilePathComputerNameParameterSet) ||
                    (ParameterSetName == InvokeCommandCommand.FilePathUriParameterSet))
                    // set to default value for WSMan session
                    ConfigurationName = ResolveShell(null);
                    // convert null to string.Empty for VM/Container session
                    ConfigurationName = string.Empty;
            // create collection of input writers here
            foreach (IThrottleOperation operation in Operations)
                _inputWriters.Add(((ExecutionCmdletHelper)operation).Pipeline.Input);
            // we need to verify, if this Invoke-Command is the first
            // instance within the current local pipeline. If not, then
            // we need to collect all the data and run the invoke-command
            // when the remote runspace is free
            // We also need to worry about it only in the case of
            // runspace parameter set - for all else we will never hit
            // this scenario
            if (ParameterSetName.Equals(InvokeCommandCommand.SessionParameterSet))
                foreach (PSSession runspaceInfo in Session)
                    RemoteRunspace remoteRunspace = (RemoteRunspace)runspaceInfo.Runspace;
                    if (remoteRunspace.IsAnotherInvokeCommandExecuting(this, localPipelineId))
                        // Use remote steppable pipeline only for non-input piping case.
                        // Win8 Bug:898011 - We are restricting remote steppable pipeline because
                        // of this bug in Win8 where not responding can occur during data piping.
                        // We are reverting to Win7 behavior for {icm | icm} and {proxycommand | proxycommand}
                        // cases. For ICM | % ICM case, we are using remote steppable pipeline.
                        if ((MyInvocation != null) && (MyInvocation.PipelinePosition == 1) && !MyInvocation.ExpectingInput)
                            PSPrimitiveDictionary table = (object)runspaceInfo.ApplicationPrivateData[PSVersionInfo.PSVersionTableName] as PSPrimitiveDictionary;
                                Version version = (object)table[PSVersionInfo.PSRemotingProtocolVersionName] as Version;
                                    // In order to support foreach remoting properly ( icm | % { icm } ), the server must
                                    // be using protocol version 2.2. Otherwise, we skip this and assume the old behavior.
                                    if (version >= RemotingConstants.ProtocolVersion_2_2)
                                        // Suppress collection behavior
                                        _needToCollect = false;
                                        _needToStartSteppablePipelineOnServer = true;
                        // Either version table is null or the server is not version 2.2 and beyond, we need to collect
                        _needToCollect = true;
                        _needToStartSteppablePipelineOnServer = false;
            if (_needToStartSteppablePipelineOnServer)
                    if (operation is not ExecutionCmdletHelperRunspace ecHelper)
                        // either all the operations will be of type ExecutionCmdletHelperRunspace
                        // or not...there is no mix.
                    ecHelper.ShouldUseSteppablePipelineOnServer = true;
                // RemoteRunspace must be holding this InvokeCommand..So release
                // this at dispose time
                _clearInvokeCommandOnRunspace = true;
            // check if we need to propagate terminating errors
            DetermineThrowStatementBehavior();
        /// The expression will be executed in the remote computer if a
        /// remote runspace parameter or computer name or uri is specified.
        /// 1. Identify if the command belongs to the same pipeline
        /// 2. If so, use the same GUID to create Pipeline/PowerShell
            // we should create the pipeline on first instance
            // and if there are no invoke-commands running
            // ahead in the pipeline
            if (!_pipelineinvoked && !_needToCollect)
                _pipelineinvoked = true;
                if (InputObject == AutomationNull.Value)
                    CloseAllInputStreams();
                    _inputStreamClosed = true;
                if (!ParameterSetName.Equals(InProcParameterSet))
                    // at this point there is nothing to do for
                    // inproc case. The script block is executed
                    // in EndProcessing
                    if (!_asjob)
                        CreateAndRunSyncJob();
                            case InvokeCommandCommand.ComputerNameParameterSet:
                            case InvokeCommandCommand.FilePathComputerNameParameterSet:
                            case InvokeCommandCommand.VMIdParameterSet:
                            case InvokeCommandCommand.VMNameParameterSet:
                            case InvokeCommandCommand.ContainerIdParameterSet:
                            case InvokeCommandCommand.FilePathVMIdParameterSet:
                            case InvokeCommandCommand.FilePathVMNameParameterSet:
                            case InvokeCommandCommand.FilePathContainerIdParameterSet:
                            case InvokeCommandCommand.SSHHostParameterSet:
                            case InvokeCommandCommand.FilePathSSHHostParameterSet:
                            case InvokeCommandCommand.SSHHostHashParameterSet:
                            case InvokeCommandCommand.FilePathSSHHostHashParameterSet:
                                    if (ResolvedComputerNames.Length != 0 && Operations.Count > 0)
                                        PSRemotingJob job = new PSRemotingJob(ResolvedComputerNames, Operations,
                                                ScriptBlock.ToString(), ThrottleLimit, _name);
                                        job.PSJobTypeName = RemoteJobType;
                                        job.HideComputerName = _hideComputerName;
                                        this.JobRepository.Add(job);
                                        WriteObject(job);
                            case InvokeCommandCommand.SessionParameterSet:
                            case InvokeCommandCommand.FilePathSessionParameterSet:
                                    PSRemotingJob job = new PSRemotingJob(Session, Operations,
                            case InvokeCommandCommand.UriParameterSet:
                            case InvokeCommandCommand.FilePathUriParameterSet:
                                    if (Operations.Count > 0)
                                        string[] locations = new string[ConnectionUri.Length];
                                        for (int i = 0; i < locations.Length; i++)
                                            locations[i] = ConnectionUri[i].ToString();
                                        PSRemotingJob job = new PSRemotingJob(locations, Operations,
            if (InputObject != AutomationNull.Value && !_inputStreamClosed)
                if ((ParameterSetName.Equals(InvokeCommandCommand.InProcParameterSet) && (_steppablePipeline == null)) ||
                    _needToCollect)
                    _input.Add(InputObject);
                else if (ParameterSetName.Equals(InvokeCommandCommand.InProcParameterSet) && (_steppablePipeline != null))
                    _steppablePipeline.Process(InputObject);
                    WriteInput(InputObject);
                    // if not a job write out the results available thus far
                        WriteJobResults(true);
        /// InvokeAsync would have been called in ProcessRecord. Wait here
        /// for all the results to become available.
            // close the input stream on all the pipelines
            if (!_needToCollect)
                    if (_steppablePipeline != null)
                        _steppablePipeline.End();
                        ScriptBlock.InvokeUsingCmdlet(
                            useLocalScope: !NoNewScope,
                            input: _input,
                            args: ArgumentList);
                    // runspace and computername parameter sets
                    if (_job != null)
                        // The job/command is disconnected immediately after it is invoked.  The command
                        // will continue to run on the server but we don't wait and return immediately.
                        if (InvokeAndDisconnect)
                            // Wait for the Job disconnect to complete.
                            WaitForDisconnectAndDisposeJob();
                        // Wait for job results and for job to complete.
                        // The Job may auto-disconnect in which case it may be
                        // converted to "asJob" so that it isn't disposed and can
                        // be connected to later.
                        WriteJobResults(false);
                        // Dispose job object if it is not returned to the user.
                        // The _asjob field can change dynamically and needs to be checked before the job 
                        // object is disposed. For example, if remote sessions are disconnected abruptly
                        // via WinRM, a disconnected job object is created to facilitate a reconnect.
                        // If the job object is disposed here, then a session reconnect cannot happen.
                            _job.Dispose();
                        // We no longer need to call ClearInvokeCommandOnRunspaces() here because
                        // this command might finish before the foreach block finishes. previously,
                        // icm | icm was implemented so that the first icm always finishes before
                        // the second icm runs, this is not the case with the new implementation
                        if (_needToCollect && ParameterSetName.Equals(InvokeCommandCommand.SessionParameterSet))
                            // if job was null, then its because the invoke-command
                            // was collecting or ProcessRecord() was not called.
                            // If we are collecting, then
                            // we would have collected until this point
                            // so now start the execution with the collected
                            // input
                            Dbg.Assert(_needToCollect, "InvokeCommand should have collected input before this");
                            Dbg.Assert(ParameterSetName.Equals(InvokeCommandCommand.SessionParameterSet), "Collecting and invoking should happen only in case of Runspace parameter set");
                            // loop through and write all input
                            foreach (object inputValue in _input)
                                WriteInput(inputValue);
                            // This calls waits for the job to return and then writes the results.
        /// This method is called when the user sends a stop signal to the
        /// cmdlet. The cmdlet will not exit until it has completed
        /// executing the command on all the runspaces. However, when a stop
        /// signal is sent, execution needs to be stopped on the pipelines
        /// corresponding to these runspaces.
        /// <remarks>This is called from a separate thread so need to worry
        /// about concurrency issues
            // Ensure that any runspace debug processing is ended
            if (hostDebugger != null)
                    hostDebugger.CancelDebuggerProcessing();
            if (!ParameterSetName.Equals(InvokeCommandCommand.InProcParameterSet))
                    // stop all operations in the job
                    // we need to check is job is not null, since
                    // StopProcessing() may be called even before the
                    // job is created
                    bool stopjob = false;
                    lock (_jobSyncObject)
                            stopjob = true;
                            // StopProcessing() has already been called
                            // the job should not be created anymore
                            _nojob = true;
                    if (stopjob)
                    // clear the need to collect flag
        private Debugger GetHostDebugger()
            Debugger hostDebugger = null;
                System.Management.Automation.Internal.Host.InternalHost chost =
                    this.Host as System.Management.Automation.Internal.Host.InternalHost;
                hostDebugger = chost.Runspace.Debugger;
            return hostDebugger;
        /// Handle event from the throttle manager indicating that all
        /// operations are complete.
            _throttleManager.ThrottleComplete -= HandleThrottleComplete;
        /// Clears the internal invoke command instance on all
        /// remote runspaces.
        private void ClearInvokeCommandOnRunspaces()
                    remoteRunspace.ClearInvokeCommand();
        /// Sets the throttle limit, creates the invoke expression
        /// sync job and executes the same.
        private void CreateAndRunSyncJob()
                if (!_nojob)
                    _throttleManager.ThrottleComplete += HandleThrottleComplete;
                    Dbg.Assert(_disconnectComplete == null, "disconnectComplete event should only be used once.");
                    _disconnectComplete = new ManualResetEvent(false);
                    _job = new PSInvokeExpressionSyncJob(Operations, _throttleManager);
                    _job.HideComputerName = _hideComputerName;
                    // Add robust connection retry notification handler.
                    AddConnectionRetryHandler(_job);
                    // Enable all Invoke-Command synchronous jobs for remote debugging (in case Wait-Debugger or
                    // or line breakpoints are set in script).
                    foreach (var operation in Operations)
                        operation.RunspaceDebuggingEnabled = true;
                        operation.RunspaceDebugStepInEnabled = RemoteDebug;
                        operation.RunspaceDebugStop += HandleRunspaceDebugStop;
                    _job.StartOperations(Operations);
        private void HandleRunspaceDebugStop(object sender, StartRunspaceDebugProcessingEventArgs args)
            var operation = sender as IThrottleOperation;
            operation.RunspaceDebugStop -= HandleRunspaceDebugStop;
            hostDebugger?.QueueRunspaceForDebug(args.Runspace);
        private void HandleJobStateChanged(object sender, JobStateEventArgs e)
            JobState state = e.JobStateInfo.State;
            if (state == JobState.Disconnected ||
                state == JobState.Completed ||
                state == JobState.Stopped ||
                state == JobState.Failed)
                RemoveConnectionRetryHandler(sender as PSInvokeExpressionSyncJob);
                // Signal that this job has been disconnected, or has ended.
                    _disconnectComplete?.Set();
        private void AddConnectionRetryHandler(PSInvokeExpressionSyncJob job)
            Collection<System.Management.Automation.PowerShell> powershells = job.GetPowerShells();
            foreach (var ps in powershells)
                if (ps.RemotePowerShell != null)
                    ps.RemotePowerShell.RCConnectionNotification += RCConnectionNotificationHandler;
        private void RemoveConnectionRetryHandler(PSInvokeExpressionSyncJob job)
            // Ensure progress bar is removed.
            StopProgressBar(0);
                    ps.RemotePowerShell.RCConnectionNotification -= RCConnectionNotificationHandler;
        private void RCConnectionNotificationHandler(object sender, PSConnectionRetryStatusEventArgs e)
            // Update the progress bar.
        /// Waits for the disconnectComplete event and then disposes the job
        private void WaitForDisconnectAndDisposeJob()
            if (_disconnectComplete != null)
                _disconnectComplete.WaitOne();
                // Create disconnected PSSession objects for each powershell and write to output.
                List<PSSession> discSessions = GetDisconnectedSessions(_job);
                foreach (PSSession session in discSessions)
                    WriteObject(session);
                // Check to see if the disconnect was successful.  If not write any errors there may be.
                if (_job.Error.Count > 0)
                    WriteStreamObjectsFromCollection(_job.ReadAll());
        /// Creates a disconnected session for each disconnected PowerShell object in
        /// PSInvokeExpressionSyncJob.
        private List<PSSession> GetDisconnectedSessions(PSInvokeExpressionSyncJob job)
            List<PSSession> discSessions = new List<PSSession>();
            foreach (System.Management.Automation.PowerShell ps in powershells)
                // Get the command information from the PowerShell object.
                string commandText = (ps.Commands != null && ps.Commands.Commands.Count > 0) ?
                    ps.Commands.Commands[0].CommandText : string.Empty;
                ConnectCommandInfo cmdInfo = new ConnectCommandInfo(ps.InstanceId, commandText);
                // Get the old RunspacePool object that the command was initially run on.
                RunspacePool oldRunspacePool = null;
                if (ps.RunspacePool != null)
                    oldRunspacePool = ps.RunspacePool;
                    object rsConnection = ps.GetRunspaceConnection();
                    RunspacePool rsPool = rsConnection as RunspacePool;
                    if (rsPool != null)
                        oldRunspacePool = rsPool;
                        RemoteRunspace remoteRs = rsConnection as RemoteRunspace;
                        if (remoteRs != null)
                            oldRunspacePool = remoteRs.RunspacePool;
                // Create a new disconnected PSSession object and return to the user.
                // The user can use this object to connect to the command on the server
                // and retrieve data.
                if (oldRunspacePool != null)
                    if (oldRunspacePool.RunspacePoolStateInfo.State != RunspacePoolState.Disconnected)
                        // InvokeAndDisconnect starts the command and immediately disconnects the command,
                        // but we need to disconnect the associated runspace/pool here.
                        if (InvokeAndDisconnect && oldRunspacePool.RunspacePoolStateInfo.State == RunspacePoolState.Opened)
                            oldRunspacePool.Disconnect();
                            // Skip runspace pools that have not been disconnected.
                    // Auto-generate a session name if one was not provided.
                    string sessionName = oldRunspacePool.RemoteRunspacePoolInternal.Name;
                    if (string.IsNullOrEmpty(sessionName))
                        sessionName = PSSession.GenerateRunspaceName(out id);
                    RunspacePool runspacePool = new RunspacePool(
                                                        oldRunspacePool.RemoteRunspacePoolInternal.InstanceId,
                                                        new ConnectCommandInfo[1] { cmdInfo },
                                                        oldRunspacePool.RemoteRunspacePoolInternal.ConnectionInfo,
                                                        this.Context.TypeTable);
                    runspacePool.RemoteRunspacePoolInternal.IsRemoteDebugStop = oldRunspacePool.RemoteRunspacePoolInternal.IsRemoteDebugStop;
                    RemoteRunspace remoteRunspace = new RemoteRunspace(runspacePool);
                    discSessions.Add(new PSSession(remoteRunspace));
            return discSessions;
        /// Writes an input value to the pipeline.
        /// <param name="inputValue">Input value to write.</param>
        private void WriteInput(object inputValue)
            // when there are no input writers, there is no
            // point either accumulating or trying to write data
            // so throw an exception in that case
            if (_inputWriters.Count == 0)
            List<PipelineWriter> removeCollection = new List<PipelineWriter>();
            foreach (PipelineWriter writer in _inputWriters)
                    writer.Write(inputValue);
                    removeCollection.Add(writer);
            foreach (PipelineWriter writer in removeCollection)
                _inputWriters.Remove(writer);
        /// Writes the results in the job object.
        /// <param name="nonblocking">Write in a non-blocking manner.</param>
        private void WriteJobResults(bool nonblocking)
            if (_job == null)
                PipelineStoppedException caughtPipelineStoppedException = null;
                _job.PropagateThrows = _propagateErrors;
                    if (!nonblocking)
                        // we need to wait until results arrive
                        // before we attempt to read. This will
                        // ensure that the thread blocks. Else
                        // the thread will spin leading to a CPU
                        // usage spike
                            // An auto-disconnect can occur and we need to detect
                            // this condition along with a job results signal.
                            WaitHandle.WaitAny(new WaitHandle[] {
                                                    _disconnectComplete,
                                                    _job.Results.WaitHandle });
                            _job.Results.WaitHandle.WaitOne();
                    catch (System.Management.Automation.PipelineStoppedException pse)
                        caughtPipelineStoppedException = pse;
                    if (nonblocking)
                } while (!_job.IsTerminalState());
                if (caughtPipelineStoppedException != null)
                    HandlePipelinesStopped();
                    throw caughtPipelineStoppedException;
                if (_job.JobStateInfo.State == JobState.Disconnected)
                    if (ParameterSetName == InvokeCommandCommand.SessionParameterSet ||
                        ParameterSetName == InvokeCommandCommand.FilePathSessionParameterSet)
                        // Create a PSRemoting job we can add to the job repository and that
                        // a user can reconnect to (via Receive-PSSession).
                        PSRemotingJob rtnJob = _job.CreateDisconnectedRemotingJob();
                        if (rtnJob != null)
                            rtnJob.PSJobTypeName = RemoteJobType;
                            // Don't let the job object be disposed or stopped since
                            // we want to be able to reconnect to the disconnected
                            // pipelines.
                            // Write warnings to user about each disconnect.
                            foreach (var cjob in rtnJob.ChildJobs)
                                PSRemotingChildJob childJob = cjob as PSRemotingChildJob;
                                    // Get session for this job.
                                    PSSession session = GetPSSession(childJob.Runspace.InstanceId);
                                        // Write network failed, auto-disconnect error
                                        WriteNetworkFailedError(session);
                                        // Session disconnected message.
                                            StringUtil.Format(RemotingErrorIdStrings.RCDisconnectSession,
                                                session.Name, session.InstanceId, session.ComputerName));
                            if (rtnJob.ChildJobs.Count > 0)
                                JobRepository.Add(rtnJob);
                                // Inform the user that a new Job object was created and added to the repository
                                // to support later reconnection.
                                    StringUtil.Format(RemotingErrorIdStrings.RCDisconnectedJob, rtnJob.Name));
                    else if (ParameterSetName == InvokeCommandCommand.ComputerNameParameterSet ||
                             ParameterSetName == InvokeCommandCommand.FilePathComputerNameParameterSet)
                        // Create disconnected sessions for each PowerShell in job that was disconnected,
                        // and add them to the local repository.
                            // Add to session repository.
                            // Session created message.
                                StringUtil.Format(RemotingErrorIdStrings.RCDisconnectSessionCreated,
                                    session.Name, session.InstanceId));
                    // Allow Invoke-Command to end even though not all remote pipelines
                    // finished.
                    HandleThrottleComplete(null, null);
        private void WriteNetworkFailedError(PSSession session)
            RuntimeException reason = new RuntimeException(
                StringUtil.Format(RemotingErrorIdStrings.RCAutoDisconnectingError, session.ComputerName));
            WriteError(new ErrorRecord(reason,
                ErrorCategory.OperationTimeout, session));
        private PSSession GetPSSession(Guid runspaceId)
            foreach (PSSession session in Session)
                if (session.Runspace.InstanceId == runspaceId)
                    return session;
        private void HandlePipelinesStopped()
            // Emit warning for cases where commands were stopped during connection retry attempts.
            bool retryCanceled = false;
            Collection<System.Management.Automation.PowerShell> powershells = _job.GetPowerShells();
                if (ps.RemotePowerShell != null &&
                    ps.RemotePowerShell.ConnectionRetryStatus != PSConnectionRetryStatus.None &&
                    ps.RemotePowerShell.ConnectionRetryStatus != PSConnectionRetryStatus.ConnectionRetrySucceeded &&
                    ps.RemotePowerShell.ConnectionRetryStatus != PSConnectionRetryStatus.AutoDisconnectSucceeded)
                    retryCanceled = true;
            if (retryCanceled &&
                this.Host != null)
                // Write warning directly to host since pipeline has been stopped.
                this.Host.UI.WriteWarningLine(RemotingErrorIdStrings.StopCommandOnRetry);
                this.Host);
        /// Writes the stream objects in the specified collection.
        /// <param name="results">Collection to read from.</param>
        private void WriteStreamObjectsFromCollection(IEnumerable<PSStreamObject> results)
            foreach (var result in results)
                    PreProcessStreamObject(result);
                    result.WriteStreamObject(this);
        /// Determine if we have to throw for a
        /// "throw" statement from scripts
        ///  This means that the local pipeline will be terminated as well.
        /// This is valid when only one pipeline is
        /// existing. Which means, there can be only one of the following:
        ///     1. A single computer name
        ///     2. A single session
        ///     3. A single uri
        /// It can be used in conjunction with a filepath or a script block parameter
        /// It doesn't take effect with the -AsJob parameter
        private void DetermineThrowStatementBehavior()
                // in proc parameter set - just return
                if (ParameterSetName.Equals(InvokeCommandCommand.ComputerNameParameterSet) ||
                    ParameterSetName.Equals(InvokeCommandCommand.FilePathComputerNameParameterSet))
                    if (ComputerName.Length == 1)
                        _propagateErrors = true;
                else if (ParameterSetName.Equals(InvokeCommandCommand.SessionParameterSet) ||
                         ParameterSetName.Equals(InvokeCommandCommand.FilePathSessionParameterSet))
                    if (Session.Length == 1)
                else if (ParameterSetName.Equals(InvokeCommandCommand.UriParameterSet) ||
                         ParameterSetName.Equals(InvokeCommandCommand.FilePathUriParameterSet))
                    if (ConnectionUri.Length == 1)
        /// Process the stream object before writing it in the specified collection.
        /// <param name="streamObject">Stream object to process.</param>
        private static void PreProcessStreamObject(PSStreamObject streamObject)
            ErrorRecord errorRecord = streamObject.Value as ErrorRecord;
            // In case of PSDirectException, we should output the precise error message
            // in inner exception instead of the generic one in outer exception.
            if ((errorRecord != null) &&
                (errorRecord.Exception != null) &&
                (errorRecord.Exception.InnerException != null))
                PSDirectException ex = errorRecord.Exception.InnerException as PSDirectException;
                    streamObject.Value = new ErrorRecord(errorRecord.Exception.InnerException,
                                                         errorRecord.CategoryInfo.Category,
                                                         errorRecord.TargetObject);
        // throttle manager for handling all throttling operations
        private ManualResetEvent _disconnectComplete;
        // the initial state is true because when no
        // operations actually take place as in case of a
        // parameter binding exception, then Dispose is
        // called. Since Dispose waits on this handler
        // it is set to true initially and is Reset() in
        // BeginProcessing()
        private PSInvokeExpressionSyncJob _job;
        // used for streaming behavior for local invocations
        private SteppablePipeline _steppablePipeline;
        private bool _pipelineinvoked = false;    // if pipeline has been invoked
        private bool _inputStreamClosed = false;
        private const string InProcParameterSet = "InProcess";
        private readonly PSDataCollection<object> _input = new PSDataCollection<object>();
        private bool _needToCollect = false;
        private bool _needToStartSteppablePipelineOnServer = false;
        private bool _clearInvokeCommandOnRunspace = false;
        private readonly List<PipelineWriter> _inputWriters = new List<PipelineWriter>();
        private readonly object _jobSyncObject = new object();
        private bool _nojob = false;
        private readonly Guid _instanceId = Guid.NewGuid();
        private bool _propagateErrors = false;
        internal static readonly string RemoteJobType = "RemoteJob";
        /// Dispose the cmdlet.
        /// Internal dispose method which does the actual disposing.
        /// <param name="disposing">Whether called from dispose or finalize.</param>
                // this call fixes bug Windows 7 #278836
                // by making sure the server is stopped even if it is waiting
                // for further input from this Invoke-Command cmdlet
                // wait for all operations to complete
                    // job will be null in the "InProcess" case
                    _job?.Dispose();
                // clear the invoke command references we have stored
                if (_clearInvokeCommandOnRunspace)
                    ClearInvokeCommandOnRunspaces();
                _input.Dispose();
                        _disconnectComplete.Dispose();
                        _disconnectComplete = null;
    #region RobustConnectionProgress class
    /// Encapsulates the Robust Connection retry progress bar.
    internal class RobustConnectionProgress
        private System.Management.Automation.Host.PSHost _psHost;
        private readonly string _activity;
        private int _secondsTotal;
        private int _secondsRemaining;
        private ProgressRecord _progressRecord;
        private long _sourceId;
        private bool _progressIsRunning;
        private Timer _updateTimer;
        public RobustConnectionProgress()
            _activity = RemotingErrorIdStrings.RCProgressActivity;
        /// Starts progress bar.
        /// <param name="secondsTotal"></param>
        /// <param name="psHost"></param>
        public void StartProgress(
            int secondsTotal,
            System.Management.Automation.Host.PSHost psHost)
            if (psHost == null)
            if (secondsTotal < 1)
            ArgumentException.ThrowIfNullOrEmpty(computerName);
                if (_progressIsRunning)
                _progressIsRunning = true;
                _sourceId = sourceId;
                _secondsTotal = secondsTotal;
                _secondsRemaining = secondsTotal;
                _psHost = psHost;
                _status = StringUtil.Format(RemotingErrorIdStrings.RCProgressStatus, computerName);
                _progressRecord = new ProgressRecord(0, _activity, _status);
                // Create timer to fire every second to update progress bar.
                _updateTimer = new Timer(new TimerCallback(UpdateCallback), null, TimeSpan.Zero, new TimeSpan(0, 0, 1));
        /// Stops progress bar.
        public void StopProgress(
                if ((sourceId == _sourceId || sourceId == 0) &&
                    _progressIsRunning)
                    RemoveProgressBar();
        private void UpdateCallback(object state)
                if (!_progressIsRunning)
                if (_secondsRemaining > 0)
                    // Update progress bar.
                    _progressRecord.PercentComplete =
                        ((_secondsTotal - _secondsRemaining) * 100) / _secondsTotal;
                    _progressRecord.SecondsRemaining = _secondsRemaining--;
                    _progressRecord.RecordType = ProgressRecordType.Processing;
                    _psHost.UI.WriteProgress(0, _progressRecord);
                    // Remove progress bar.
        private void RemoveProgressBar()
            _progressIsRunning = false;
            _progressRecord.RecordType = ProgressRecordType.Completed;
            // Remove timer.
            _updateTimer.Dispose();
            _updateTimer = null;

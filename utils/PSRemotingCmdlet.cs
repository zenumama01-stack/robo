    /// This class defines most of the common functionality used
    /// across remoting cmdlets.
    /// It contains tons of utility functions which are used all
    /// across the remoting cmdlets.
    public abstract class PSRemotingCmdlet : PSCmdlet
            if (!SkipWinRMCheck)
        #region Utility functions
        /// Handle the object obtained from an ObjectStream's reader
        /// based on its type.
        internal void WriteStreamObject(Action<Cmdlet> action)
            action(this);
        /// Resolve all the machine names provided. Basically, if a machine
        /// name is '.' assume localhost.
        /// <param name="computerNames">Array of computer names to resolve.</param>
        /// <param name="resolvedComputerNames">Resolved array of machine names.</param>
        protected void ResolveComputerNames(string[] computerNames, out string[] resolvedComputerNames)
            if (computerNames == null)
                resolvedComputerNames = new string[1];
                resolvedComputerNames[0] = ResolveComputerName(".");
            else if (computerNames.Length == 0)
                resolvedComputerNames = Array.Empty<string>();
                resolvedComputerNames = new string[computerNames.Length];
                for (int i = 0; i < resolvedComputerNames.Length; i++)
                    resolvedComputerNames[i] = ResolveComputerName(computerNames[i]);
        /// Resolves a computer name. If its null or empty
        /// its assumed to be localhost.
        /// <param name="computerName">Computer name to resolve.</param>
        /// <returns>Resolved computer name.</returns>
        protected string ResolveComputerName(string computerName)
            Diagnostics.Assert(computerName != null, "Null ComputerName");
            if (string.Equals(computerName, ".", StringComparison.OrdinalIgnoreCase))
                // tracer.WriteEvent(ref PSEventDescriptors.PS_EVENT_HOSTNAMERESOLVE);
                // tracer.Dispose();
                // tracer.OperationalChannel.WriteVerbose(PSEventId.HostNameResolve, PSOpcode.Method, PSTask.CreateRunspace);
                return s_LOCALHOST;
        /// Load the resource corresponding to the specified errorId and
        /// return the message as a string.
        /// <param name="resourceString">resource String which holds the message
        /// <returns>Error message loaded from appropriate resource cache.</returns>
        internal string GetMessage(string resourceString)
            string message = GetMessage(resourceString, null);
        internal string GetMessage(string resourceString, params object[] args)
        #endregion Utility functions
        private static readonly string s_LOCALHOST = "localhost";
        // private PSETWTracer tracer = PSETWTracer.GetETWTracer(PSKeyword.Cmdlets);
        /// Computername parameter set.
        protected const string ComputerNameParameterSet = "ComputerName";
        /// Computername with session instance ID parameter set.
        protected const string ComputerInstanceIdParameterSet = "ComputerInstanceId";
        /// Container ID parameter set.
        protected const string ContainerIdParameterSet = "ContainerId";
        /// VM guid parameter set.
        protected const string VMIdParameterSet = "VMId";
        /// VM name parameter set.
        protected const string VMNameParameterSet = "VMName";
        /// SSH host parameter set.
        protected const string SSHHostParameterSet = "SSHHost";
        /// SSH host parmeter set supporting hash connection parameters.
        protected const string SSHHostHashParameterSet = "SSHHostHashParam";
        /// Runspace parameter set.
        protected const string SessionParameterSet = "Session";
        /// Parameter set to use Windows PowerShell.
        protected const string UseWindowsPowerShellParameterSet = "UseWindowsPowerShellParameterSet";
        /// Default shellname.
        protected const string DefaultPowerShellRemoteShellName = WSManNativeApi.ResourceURIPrefix + "Microsoft.PowerShell";
        /// Default application name for the connection uri.
        protected const string DefaultPowerShellRemoteShellAppName = "WSMan";
        #endregion Protected Members
        /// Skip checking for WinRM.
        internal bool SkipWinRMCheck { get; set; } = false;
        /// Determines the shellname to use based on the following order:
        ///     1. ShellName parameter specified
        ///     2. DEFAULTREMOTESHELLNAME variable set
        ///     3. PowerShell.
        /// <returns>The shell to launch in the remote machine.</returns>
        protected string ResolveShell(string shell)
            string resolvedShell;
            if (!string.IsNullOrEmpty(shell))
                resolvedShell = shell;
                resolvedShell = (string)SessionState.Internal.ExecutionContext.GetVariableValue(
                    SpecialVariables.PSSessionConfigurationNameVarPath, DefaultPowerShellRemoteShellName);
            return resolvedShell;
        /// Determines the appname to be used based on the following order:
        ///     1. AppName parameter specified
        ///     2. DEFAULTREMOTEAPPNAME variable set
        ///     3. WSMan.
        /// <param name="appName">Application name to resolve.</param>
        /// <returns>Resolved appname.</returns>
        protected string ResolveAppName(string appName)
            string resolvedAppName;
            if (!string.IsNullOrEmpty(appName))
                resolvedAppName = appName;
                resolvedAppName = (string)SessionState.Internal.ExecutionContext.GetVariableValue(
                    SpecialVariables.PSSessionApplicationNameVarPath,
                    DefaultPowerShellRemoteShellAppName);
            return resolvedAppName;
    /// Contains SSH connection information.
    internal struct SSHConnection
        public string ComputerName;
        public string KeyFilePath;
        public int Port;
        public string Subsystem;
        public int ConnectingTimeout;
        public Hashtable Options;
    /// Base class for any cmdlet which takes a -Session parameter
    /// or a -ComputerName parameter (along with its other associated
    /// parameters). The following cmdlets currently fall under this
    /// category:
    ///     1. New-PSSession
    ///     2. Invoke-Expression
    ///     3. Start-PSJob.
    public abstract class PSRemotingBaseCmdlet : PSRemotingCmdlet
        /// State of virtual machine. This is the same as VMState in
        /// \vm\ux\powershell\objects\common\Types.cs.
        internal enum VMState
            /// Other. Corresponds to CIM_EnabledLogicalElement.EnabledState = Other.
            /// Running. Corresponds to CIM_EnabledLogicalElement.EnabledState = Enabled.
            Running = 2,
            /// Off. Corresponds to CIM_EnabledLogicalElement.EnabledState = Disabled.
            Off = 3,
            /// Stopping. Corresponds to CIM_EnabledLogicalElement.EnabledState = ShuttingDown.
            Stopping = 4,
            /// Saved. Corresponds to CIM_EnabledLogicalElement.EnabledState = Enabled but offline.
            Saved = 6,
            /// Paused. Corresponds to CIM_EnabledLogicalElement.EnabledState = Quiesce.
            Paused = 9,
            /// Starting. EnabledStateStarting. State transition from PowerOff or Saved to Running.
            Starting = 10,
            /// Reset. Corresponds to CIM_EnabledLogicalElement.EnabledState = Reset.
            Reset = 11,
            /// Saving. Corresponds to EnabledStateSaving.
            Saving = 32773,
            /// Pausing. Corresponds to EnabledStatePausing.
            Pausing = 32776,
            /// Resuming. Corresponds to EnabledStateResuming.
            Resuming = 32777,
            /// FastSaved. EnabledStateFastSuspend.
            FastSaved = 32779,
            /// FastSaving. EnabledStateFastSuspending.
            FastSaving = 32780,
            /// ForceShutdown. Used to force a graceful shutdown of the virtual machine.
            ForceShutdown = 32781,
            /// ForceReboot. Used to force a graceful reboot of the virtual machine.
            ForceReboot = 32782,
            /// RunningCritical. Critical states.
            RunningCritical,
            /// OffCritical. Critical states.
            OffCritical,
            /// StoppingCritical. Critical states.
            StoppingCritical,
            /// SavedCritical. Critical states.
            SavedCritical,
            /// PausedCritical. Critical states.
            PausedCritical,
            /// StartingCritical. Critical states.
            StartingCritical,
            /// ResetCritical. Critical states.
            ResetCritical,
            /// SavingCritical. Critical states.
            SavingCritical,
            /// PausingCritical. Critical states.
            PausingCritical,
            /// ResumingCritical. Critical states.
            ResumingCritical,
            /// FastSavedCritical. Critical states.
            FastSavedCritical,
            /// FastSavingCritical. Critical states.
            FastSavingCritical,
        /// Get the State property from Get-VM result.
        /// <param name="value">The raw PSObject as returned by Get-VM.</param>
        /// <returns>The VMState value of the State property if present and parsable, otherwise null.</returns>
        internal VMState? GetVMStateProperty(PSObject value)
            object? rawState = value.Properties["State"].Value;
            if (rawState is Enum enumState)
                // If the Hyper-V module was directly importable we have the VMState enum
                // value which we can just cast to our VMState type.
                return (VMState)enumState;
            else if (rawState is string stringState && Enum.TryParse(stringState, true, out VMState result))
                // If the Hyper-V module was imported through implicit remoting on old
                // Windows versions we get a string back which we will try and parse
                // as the enum label.
            // Unknown scenario, this should not happen.
                RemotingErrorIdStrings.HyperVFailedToGetStateUnknownType,
                rawState?.GetType()?.FullName ?? "null");
        // PSETWTracer tracer = PSETWTracer.GetETWTracer(PSKeyword.Runspace);
                   ParameterSetName = PSRemotingBaseCmdlet.SessionParameterSet)]
        public virtual PSSession[] Session { get; set; }
                   ParameterSetName = PSRemotingBaseCmdlet.ComputerNameParameterSet)]
        public virtual string[] ComputerName { get; set; }
        /// Computer names after they have been resolved
        /// (null, empty string, "." resolves to localhost)
        /// <remarks>If Null or empty string is specified, then localhost is assumed.
        /// The ResolveComputerNames will include this.
        protected string[] ResolvedComputerNames { get; set; }
        /// Guid of target virtual machine.
            Justification = "This is by spec.")]
                   ParameterSetName = PSRemotingBaseCmdlet.VMIdParameterSet)]
        [Alias("VMGuid")]
        public virtual Guid[] VMId { get; set; }
        /// Name of target virtual machine.
                   ParameterSetName = PSRemotingBaseCmdlet.VMNameParameterSet)]
        public virtual string[] VMName { get; set; }
                   ParameterSetName = PSRemotingBaseCmdlet.UriParameterSet)]
                return _pscredential;
                _pscredential = value;
                ValidateSpecifiedAuthentication(Credential, CertificateThumbprint, Authentication);
        private PSCredential _pscredential;
        /// ID of target container.
                   ParameterSetName = PSRemotingBaseCmdlet.ContainerIdParameterSet)]
        public virtual string[] ContainerId { get; set; }
        [Parameter(ParameterSetName = PSRemotingBaseCmdlet.ContainerIdParameterSet)]
        public virtual SwitchParameter RunAsAdministrator { get; set; }
        [Parameter(ParameterSetName = PSRemotingBaseCmdlet.ComputerNameParameterSet)]
        public virtual int Port { get; set; }
        public virtual SwitchParameter UseSSL { get; set; }
        public virtual string ApplicationName
        [Parameter(ParameterSetName = PSRemotingBaseCmdlet.SessionParameterSet)]
        [Parameter(ParameterSetName = PSRemotingBaseCmdlet.UriParameterSet)]
        [Parameter(ParameterSetName = PSRemotingBaseCmdlet.VMIdParameterSet)]
        [Parameter(ParameterSetName = PSRemotingBaseCmdlet.VMNameParameterSet)]
        public virtual int ThrottleLimit { get; set; } = 0;
        public virtual Uri[] ConnectionUri { get; set; }
        public virtual SwitchParameter AllowRedirection
        public virtual PSSessionOption SessionOption
                if (_sessionOption == null)
                    object tmp = this.SessionState.PSVariable.GetValue(DEFAULT_SESSION_OPTION);
                    if (tmp == null || !LanguagePrimitives.TryConvertTo<PSSessionOption>(tmp, out _sessionOption))
                        _sessionOption = new PSSessionOption();
                return _sessionOption;
                _sessionOption = value;
        internal const string DEFAULT_SESSION_OPTION = "PSSessionOption";
        // Quota related variables.
                return _authMechanism;
                _authMechanism = value;
                // Validate if a user can specify this authentication.
        private AuthenticationMechanism _authMechanism = AuthenticationMechanism.Default;
        [Parameter(ParameterSetName = NewPSSessionCommand.ComputerNameParameterSet)]
        [Parameter(ParameterSetName = NewPSSessionCommand.UriParameterSet)]
                return _thumbPrint;
                _thumbPrint = value;
        private string _thumbPrint = null;
        #region SSHHostParameters
            ParameterSetName = PSRemotingBaseCmdlet.SSHHostParameterSet)]
        public virtual string[] HostName
        /// SSH User Name.
        public virtual string UserName
        /// SSH Key File Path.
        public virtual string KeyFilePath
        /// Gets or sets a value for the SSH subsystem to use for the remote connection.
        public virtual string Subsystem { get; set; }
        /// Gets or sets a value in milliseconds that limits the time allowed for an SSH connection to be established.
        /// Default timeout value is infinite.
        public virtual int ConnectingTimeout { get; set; } = Timeout.Infinite;
        public virtual SwitchParameter SSHTransport
        public virtual Hashtable[] SSHConnection
        /// Gets or sets the Hashtable containing options to be passed to OpenSSH.
        public virtual Hashtable Options { get; set; }
        #region Internal Static Methods
        internal static void ValidateSpecifiedAuthentication(PSCredential credential, string thumbprint, AuthenticationMechanism authentication)
            if ((credential != null) && (thumbprint != null))
                    RemotingErrorIdStrings.NewRunspaceAmbiguousAuthentication,
                        "CertificateThumbPrint", "Credential");
            if ((authentication != AuthenticationMechanism.Default) && (thumbprint != null))
            if ((authentication == AuthenticationMechanism.NegotiateWithImplicitCredential) &&
                (credential != null))
                    "Credential", authentication.ToString());
        #region SSH Connection Strings
        private const string ComputerNameParameter = "ComputerName";
        private const string HostNameAlias = "HostName";
        private const string UserNameParameter = "UserName";
        private const string KeyFilePathParameter = "KeyFilePath";
        private const string IdentityFilePathAlias = "IdentityFilePath";
        private const string PortParameter = "Port";
        private const string SubsystemParameter = "Subsystem";
        private const string ConnectingTimeoutParameter = "ConnectingTimeout";
        private const string OptionsParameter = "Options";
        /// Parse a hostname used with SSH Transport to get embedded
        /// username and/or port.
        /// <param name="hostname">Host name to parse.</param>
        /// <param name="host">Resolved target host.</param>
        /// <param name="userName">Resolved target user name.</param>
        /// <param name="port">Resolved target port.</param>
        protected void ParseSshHostName(string hostname, out string host, out string userName, out int port)
            host = hostname;
            userName = this.UserName;
            port = this.Port;
                Uri uri = new System.Uri("ssh://" + hostname);
                host = ResolveComputerName(uri.Host);
                ValidateComputerName(new string[] { host });
                if (uri.UserInfo != string.Empty)
                    userName = uri.UserInfo;
                if (uri.Port != -1)
                    port = uri.Port;
            catch (UriFormatException)
                    new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(
                        RemotingErrorIdStrings.InvalidComputerName)), "PSSessionInvalidComputerName",
                            ErrorCategory.InvalidArgument, hostname));
        /// Parse the Connection parameter HashTable array.
        /// <returns>Array of SSHConnection objects.</returns>
        internal SSHConnection[] ParseSSHConnectionHashTable()
            List<SSHConnection> connections = new();
            foreach (var item in this.SSHConnection)
                if (item.ContainsKey(ComputerNameParameter) && item.ContainsKey(HostNameAlias))
                    throw new PSArgumentException(RemotingErrorIdStrings.SSHConnectionDuplicateHostName);
                if (item.ContainsKey(KeyFilePathParameter) && item.ContainsKey(IdentityFilePathAlias))
                    throw new PSArgumentException(RemotingErrorIdStrings.SSHConnectionDuplicateKeyPath);
                SSHConnection connectionInfo = new();
                foreach (var key in item.Keys)
                    string paramName = key as string;
                        throw new PSArgumentException(RemotingErrorIdStrings.InvalidSSHConnectionParameter);
                    if (paramName.Equals(ComputerNameParameter, StringComparison.OrdinalIgnoreCase) || paramName.Equals(HostNameAlias, StringComparison.OrdinalIgnoreCase))
                        var resolvedComputerName = ResolveComputerName(GetSSHConnectionStringParameter(item[paramName]));
                        ParseSshHostName(resolvedComputerName, out string host, out string userName, out int port);
                        connectionInfo.ComputerName = host;
                        if (userName != string.Empty)
                            connectionInfo.UserName = userName;
                        if (port != -1)
                            connectionInfo.Port = port;
                    else if (paramName.Equals(UserNameParameter, StringComparison.OrdinalIgnoreCase))
                        connectionInfo.UserName = GetSSHConnectionStringParameter(item[paramName]);
                    else if (paramName.Equals(KeyFilePathParameter, StringComparison.OrdinalIgnoreCase) || paramName.Equals(IdentityFilePathAlias, StringComparison.OrdinalIgnoreCase))
                        connectionInfo.KeyFilePath = GetSSHConnectionStringParameter(item[paramName]);
                    else if (paramName.Equals(PortParameter, StringComparison.OrdinalIgnoreCase))
                        connectionInfo.Port = GetSSHConnectionIntParameter(item[paramName]);
                    else if (paramName.Equals(SubsystemParameter, StringComparison.OrdinalIgnoreCase))
                        connectionInfo.Subsystem = GetSSHConnectionStringParameter(item[paramName]);
                    else if (paramName.Equals(ConnectingTimeoutParameter, StringComparison.OrdinalIgnoreCase))
                        connectionInfo.ConnectingTimeout = GetSSHConnectionIntParameter(item[paramName]);
                    else if (paramName.Equals(OptionsParameter, StringComparison.OrdinalIgnoreCase))
                        connectionInfo.Options = item[paramName] as Hashtable;
                        throw new PSArgumentException(
                            StringUtil.Format(RemotingErrorIdStrings.UnknownSSHConnectionParameter, paramName));
                if (string.IsNullOrEmpty(connectionInfo.ComputerName))
                    throw new PSArgumentException(RemotingErrorIdStrings.MissingRequiredSSHParameter);
                connections.Add(connectionInfo);
            return connections.ToArray();
        /// Validate the PSSession objects specified and write
        /// appropriate error records.
        /// <remarks>This function will lead in terminating errors when any of
        /// the validations fail</remarks>
        protected void ValidateRemoteRunspacesSpecified()
            Dbg.Assert(Session != null && Session.Length != 0,
                    "Remote Runspaces specified must not be null or empty");
            // Check if there are duplicates in the specified PSSession objects
            if (RemotingCommandUtil.HasRepeatingRunspaces(Session))
                ThrowTerminatingError(new ErrorRecord(new ArgumentException(
                    GetMessage(RemotingErrorIdStrings.RemoteRunspaceInfoHasDuplicates)),
                        nameof(PSRemotingErrorId.RemoteRunspaceInfoHasDuplicates),
                            ErrorCategory.InvalidArgument, Session));
            // BUGBUG: The following is a bogus check
            // Check if the number of PSSession objects specified is greater
            // than the maximum allowable range
            if (RemotingCommandUtil.ExceedMaximumAllowableRunspaces(Session))
                    GetMessage(RemotingErrorIdStrings.RemoteRunspaceInfoLimitExceeded)),
                        nameof(PSRemotingErrorId.RemoteRunspaceInfoLimitExceeded),
        /// Updates connection info with the data read from cmdlet's parameters and
        /// sessions variables.
        /// The following data is updated:
        /// 1. MaxURIRedirectionCount
        /// 2. MaxRecvdDataSizePerSession
        /// 3. MaxRecvdDataSizePerCommand
        /// 4. MaxRecvdObjectSize.
        internal void UpdateConnectionInfo(WSManConnectionInfo connectionInfo)
            Dbg.Assert(connectionInfo != null, "connectionInfo cannot be null.");
            connectionInfo.SetSessionOptions(this.SessionOption);
            if (!ParameterSetName.Equals(PSRemotingBaseCmdlet.UriParameterSet, StringComparison.OrdinalIgnoreCase))
        /// Uri parameter set.
        protected const string UriParameterSet = "Uri";
        /// Validates computer names to check if none of them
        /// happen to be a Uri. If so this throws an error.
        /// <param name="computerNames">collection of computer
        /// names to validate</param>
        protected void ValidateComputerName(string[] computerNames)
                UriHostNameType nametype = Uri.CheckHostName(computerName);
                if (!(nametype == UriHostNameType.Dns || nametype == UriHostNameType.IPv4 ||
                    nametype == UriHostNameType.IPv6))
                                ErrorCategory.InvalidArgument, computerNames));
        /// Validates parameter value and returns as string.
        /// <param name="param">Parameter value to be validated.</param>
        /// <returns>Parameter value as string.</returns>
        private static string GetSSHConnectionStringParameter(object param)
            string paramValue;
                paramValue = LanguagePrimitives.ConvertTo<string>(param);
                throw new PSArgumentException(e.Message, e);
            if (!string.IsNullOrEmpty(paramValue))
                return paramValue;
        /// Validates parameter value and returns as integer.
        /// <returns>Parameter value as integer.</returns>
        private static int GetSSHConnectionIntParameter(object param)
            if (param == null)
                return LanguagePrimitives.ConvertTo<int>(param);
        /// Resolves shellname and appname.
            // Validate KeyFilePath parameter.
            if ((ParameterSetName == PSRemotingBaseCmdlet.SSHHostParameterSet) &&
                (this.KeyFilePath != null))
                // Resolve the key file path when set.
                this.KeyFilePath = PathResolver.ResolveProviderAndPath(this.KeyFilePath, true, this, false, RemotingErrorIdStrings.FilePathNotFromFileSystemProvider);
            // Validate IdleTimeout parameter.
            int idleTimeout = (int)SessionOption.IdleTimeout.TotalMilliseconds;
                idleTimeout < BaseTransportManager.MinimumIdleTimeout)
                    StringUtil.Format(RemotingErrorIdStrings.InvalidIdleTimeoutOption,
                    idleTimeout / 1000, BaseTransportManager.MinimumIdleTimeout / 1000));
            if (string.IsNullOrEmpty(_appName))
                _appName = ResolveAppName(null);
    /// Base class for any cmdlet which has to execute a pipeline. The
    /// following cmdlets currently fall under this category:
    ///     1. Invoke-Expression
    ///     2. Start-PSJob.
    public abstract class PSExecutionCmdlet : PSRemotingBaseCmdlet
        /// VM guid file path parameter set.
        protected const string FilePathVMIdParameterSet = "FilePathVMId";
        /// VM name file path parameter set.
        protected const string FilePathVMNameParameterSet = "FilePathVMName";
        /// Container ID file path parameter set.
        protected const string FilePathContainerIdParameterSet = "FilePathContainerId";
        /// SSH Host file path parameter set.
        protected const string FilePathSSHHostParameterSet = "FilePathSSHHost";
        /// SSH Host file path parameter set with HashTable connection parameter.
        protected const string FilePathSSHHostHashParameterSet = "FilePathSSHHostHash";
        /// Input object which gets assigned to $input when executed
        /// on the remote machine. This is the only parameter in
        /// this cmdlet which will bind with a ValueFromPipeline=true.
        public virtual PSObject InputObject { get; set; } = AutomationNull.Value;
        /// Command to execute specified as a string. This can be a single
        /// cmdlet, an expression or anything that can be internally
        /// converted into a ScriptBlock.
        public virtual ScriptBlock ScriptBlock
        /// The file containing the script that the user has specified in the
        public virtual string FilePath
                return _filePath;
                _filePath = value;
        private string _filePath;
        /// True if FilePath should be processed as a literal path.
        protected bool IsLiteralPath { get; set; }
        /// Arguments that are passed to this scriptblock.
        public virtual object[] ArgumentList
                return _args;
                _args = value;
        private object[] _args;
        /// Indicates that if a job/command is invoked remotely the connection should be severed
        /// right have invocation of job/command.
        protected bool InvokeAndDisconnect { get; set; } = false;
        /// Session names optionally provided for Disconnected parameter.
        protected string[] DisconnectedSessionName { get; set; }
        public virtual SwitchParameter EnableNetworkAccess { get; set; }
        public override Guid[] VMId { get; set; }
        public override string[] VMName { get; set; }
        public override string[] ContainerId { get; set; }
        public virtual string ConfigurationName { get; set; }
        /// Creates helper objects with the command for the specified
        /// remote computer names.
        protected virtual void CreateHelpersForSpecifiedComputerNames()
            ValidateComputerName(ResolvedComputerNames);
            // create helper objects for computer names
            for (int i = 0; i < ResolvedComputerNames.Length; i++)
                    connectionInfo.ComputerName = ResolvedComputerNames[i];
                    connectionInfo.EnableNetworkAccess = EnableNetworkAccess;
                    // Use the provided session name or create one for this remote runspace so that
                    // it can be easily identified if it becomes disconnected and is queried on the server.
                    int rsId = PSSession.GenerateRunspaceId();
                    string rsName = (DisconnectedSessionName != null && DisconnectedSessionName.Length > i) ?
                        DisconnectedSessionName[i] : PSSession.GenerateRunspaceName(out rsId);
                    remoteRunspace = new RemoteRunspace(Utils.GetTypeTableFromExecutionContextTLS(), connectionInfo,
                        this.Host, this.SessionOption.ApplicationArguments, rsName, rsId);
                    remoteRunspace.Events.ReceivedEvents.PSEventReceived += OnRunspacePSEventReceived;
                catch (UriFormatException uriException)
                    ErrorRecord errorRecord = new ErrorRecord(uriException, "CreateRemoteRunspaceFailed",
                            ErrorCategory.InvalidArgument, ResolvedComputerNames[i]);
                Pipeline pipeline = CreatePipeline(remoteRunspace);
                IThrottleOperation operation =
                    new ExecutionCmdletHelperComputerName(remoteRunspace, pipeline, InvokeAndDisconnect);
                Operations.Add(operation);
        /// Creates helper objects for SSH remoting computer names
        /// remoting.
        protected void CreateHelpersForSpecifiedSSHComputerNames()
            foreach (string computerName in ResolvedComputerNames)
                ParseSshHostName(computerName, out string host, out string userName, out int port);
                var sshConnectionInfo = new SSHConnectionInfo(userName, host, KeyFilePath, port, Subsystem, ConnectingTimeout, Options);
                var typeTable = TypeTable.LoadDefaultTypeFiles();
                var remoteRunspace = RunspaceFactory.CreateRunspace(sshConnectionInfo, Host, typeTable) as RemoteRunspace;
                var pipeline = CreatePipeline(remoteRunspace);
                var operation = new ExecutionCmdletHelperComputerName(remoteRunspace, pipeline);
        /// Creates helper objects for SSH remoting from HashTable parameters.
        protected void CreateHelpersForSpecifiedSSHHashComputerNames()
            var sshConnections = ParseSSHConnectionHashTable();
            foreach (var sshConnection in sshConnections)
                var sshConnectionInfo = new SSHConnectionInfo(
                    sshConnection.UserName,
                    sshConnection.ComputerName,
                    sshConnection.KeyFilePath,
                    sshConnection.Port,
                    sshConnection.Subsystem,
                    sshConnection.ConnectingTimeout);
                var remoteRunspace = RunspaceFactory.CreateRunspace(sshConnectionInfo, this.Host, typeTable) as RemoteRunspace;
        /// Creates helper objects with the specified command for
        /// the specified remote runspaceinfo objects.
        protected void CreateHelpersForSpecifiedRunspaces()
            RemoteRunspace[] remoteRunspaces;
            Pipeline[] pipelines;
            // extract RemoteRunspace out of the PSSession objects
            int length = Session.Length;
            remoteRunspaces = new RemoteRunspace[length];
                remoteRunspaces[i] = (RemoteRunspace)Session[i].Runspace;
            // create the set of pipelines from the RemoteRunspace objects and
            // create IREHelperRunspace helper class to create operations
            pipelines = new Pipeline[length];
                pipelines[i] = CreatePipeline(remoteRunspaces[i]);
                // create the operation object
                IThrottleOperation operation = new ExecutionCmdletHelperRunspace(pipelines[i]);
        /// remote connection uris.
        protected void CreateHelpersForSpecifiedUris()
            for (int i = 0; i < ConnectionUri.Length; i++)
                    connectionInfo.ConnectionUri = ConnectionUri[i];
                    remoteRunspace = (RemoteRunspace)RunspaceFactory.CreateRunspace(connectionInfo, this.Host,
                        Utils.GetTypeTableFromExecutionContextTLS(),
                        this.SessionOption.ApplicationArguments);
                    Dbg.Assert(remoteRunspace != null,
                            "RemoteRunspace object created using URI is null");
                    WriteErrorCreateRemoteRunspaceFailed(e, ConnectionUri[i]);
        /// VM GUIDs or VM names.
        protected virtual void CreateHelpersForSpecifiedVMSession()
            int inputArraySize;
            string command;
            bool[] vmIsRunning;
            if ((ParameterSetName == PSExecutionCmdlet.VMIdParameterSet) ||
                (ParameterSetName == PSExecutionCmdlet.FilePathVMIdParameterSet))
                inputArraySize = this.VMId.Length;
                this.VMName = new string[inputArraySize];
                vmIsRunning = new bool[inputArraySize];
                for (index = 0; index < inputArraySize; index++)
                    vmIsRunning[index] = false;
                    command = "Get-VM -Id $args[0]";
                        results = this.InvokeCommand.InvokeScript(
                            command, false, PipelineResultTypes.None, null, this.VMId[index]);
                                new ArgumentException(RemotingErrorIdStrings.HyperVModuleNotAvailable),
                                nameof(PSRemotingErrorId.HyperVModuleNotAvailable),
                                ErrorCategory.NotInstalled,
                    if (results.Count != 1)
                        this.VMName[index] = string.Empty;
                        this.VMName[index] = (string)results[0].Properties["VMName"].Value;
                        if (GetVMStateProperty(results[0]) == VMState.Running)
                            vmIsRunning[index] = true;
                Dbg.Assert((ParameterSetName == PSExecutionCmdlet.VMNameParameterSet) ||
                           (ParameterSetName == PSExecutionCmdlet.FilePathVMNameParameterSet),
                           "Expected ParameterSetName == VMName or FilePathVMName");
                inputArraySize = this.VMName.Length;
                this.VMId = new Guid[inputArraySize];
                    command = "Get-VM -Name $args";
                            command, false, PipelineResultTypes.None, null, this.VMName[index]);
                        this.VMId[index] = Guid.Empty;
                        this.VMId[index] = (Guid)results[0].Properties["VMId"].Value;
            ResolvedComputerNames = this.VMName;
            for (index = 0; index < ResolvedComputerNames.Length; index++)
                if ((this.VMId[index] == Guid.Empty) &&
                    ((ParameterSetName == PSExecutionCmdlet.VMNameParameterSet) ||
                     (ParameterSetName == PSExecutionCmdlet.FilePathVMNameParameterSet)))
                            new ArgumentException(GetMessage(RemotingErrorIdStrings.InvalidVMNameNotSingle,
                                                             this.VMName[index])),
                            nameof(PSRemotingErrorId.InvalidVMNameNotSingle),
                else if ((this.VMName[index] == string.Empty) &&
                         ((ParameterSetName == PSExecutionCmdlet.VMIdParameterSet) ||
                          (ParameterSetName == PSExecutionCmdlet.FilePathVMIdParameterSet)))
                            new ArgumentException(GetMessage(RemotingErrorIdStrings.InvalidVMIdNotSingle,
                                                             this.VMId[index].ToString(null))),
                            nameof(PSRemotingErrorId.InvalidVMIdNotSingle),
                else if (!vmIsRunning[index])
                            new ArgumentException(GetMessage(RemotingErrorIdStrings.InvalidVMState,
                            nameof(PSRemotingErrorId.InvalidVMState),
                // create helper objects for VM GUIDs or names
                VMConnectionInfo connectionInfo;
                    connectionInfo = new VMConnectionInfo(this.Credential, this.VMId[index], this.VMName[index], this.ConfigurationName);
                    remoteRunspace = new RemoteRunspace(Utils.GetTypeTableFromExecutionContextTLS(),
                        connectionInfo, this.Host, null, null, -1);
                    ErrorRecord errorRecord = new ErrorRecord(e,
                        "CreateRemoteRunspaceForVMFailed",
                    new ExecutionCmdletHelperComputerName(remoteRunspace, pipeline, false);
        /// container IDs or names.
        protected virtual void CreateHelpersForSpecifiedContainerSession()
            List<string> resolvedNameList = new List<string>();
            Dbg.Assert((ParameterSetName == PSExecutionCmdlet.ContainerIdParameterSet) ||
                       (ParameterSetName == PSExecutionCmdlet.FilePathContainerIdParameterSet),
                       "Expected ParameterSetName == ContainerId or FilePathContainerId");
            foreach (var input in ContainerId)
                // Create helper objects for container ID or name.
                ContainerConnectionInfo connectionInfo = null;
                    // Hyper-V container uses Hype-V socket as transport.
                    // Windows Server container uses named pipe as transport.
                    connectionInfo = ContainerConnectionInfo.CreateContainerConnectionInfo(input, RunAsAdministrator.IsPresent, this.ConfigurationName);
                    resolvedNameList.Add(connectionInfo.ComputerName);
                    connectionInfo.CreateContainerProcess();
                        "CreateRemoteRunspaceForContainerFailed",
            ResolvedComputerNames = resolvedNameList.ToArray();
        /// Creates a pipeline from the powershell.
        /// <param name="remoteRunspace">Runspace on which to create the pipeline.</param>
        /// <returns>A pipeline.</returns>
        internal Pipeline CreatePipeline(RemoteRunspace remoteRunspace)
            // The fix to WinBlue#475223 changed how UsingExpression is handled on the client/server sides, if the remote end is PSv5
            // or later, we send the dictionary-form using values to the remote end. If the remote end is PSv3 or PSv4, then we send
            // the array-form using values if all UsingExpressions are in the same scope, otherwise, we handle the UsingExpression as
            // if the remote end is PSv2.
            string serverPsVersion = GetRemoteServerPsVersion(remoteRunspace);
            System.Management.Automation.PowerShell powershellToUse = GetPowerShellForPSv3OrLater(serverPsVersion);
            Pipeline pipeline =
                remoteRunspace.CreatePipeline(powershellToUse.Commands.Commands[0].CommandText, true);
            foreach (Command command in powershellToUse.Commands.Commands)
            pipeline.RedirectShellErrorOutputPipe = true;
        /// Check the powershell version of the remote server.
        private static string GetRemoteServerPsVersion(RemoteRunspace remoteRunspace)
            if (remoteRunspace.ConnectionInfo is not WSManConnectionInfo)
                // All transport types except for WSManConnectionInfo work with 5.1 or later.
                return PSv5OrLater;
            PSPrimitiveDictionary psApplicationPrivateData = remoteRunspace.GetApplicationPrivateData();
            if (psApplicationPrivateData == null)
                // The remote runspace is not opened yet, or it's disconnected before the private data is retrieved.
                // In this case we cannot validate if the remote server is running PSv5 or later, so for safety purpose,
                // we will handle the $using expressions as if the remote server is PSv3Orv4.
                return PSv3Orv4;
                psApplicationPrivateData,
                out Version serverPsVersion,
                PSVersionInfo.PSVersionName);
            // PSv5 server will return 5.0 whereas older versions will always be 2.0. As we don't care about v2
            // anymore we can use a simple ternary check here to differenciate v5 using behaviour vs v3/4.
            return serverPsVersion != null && serverPsVersion.Major >= 5 ? PSv5OrLater : PSv3Orv4;
        /// Adds forwarded events to the local queue.
        internal void OnRunspacePSEventReceived(object sender, PSEventArgs e) => this.Events?.AddForwardedEvent(e);
        #region Protected Members / Methods
        /// List of operations.
        internal List<IThrottleOperation> Operations { get; } = new List<IThrottleOperation>();
        /// Closes the input streams on all the pipelines.
        protected void CloseAllInputStreams()
                ExecutionCmdletHelper helper = (ExecutionCmdletHelper)operation;
                helper.Pipeline.Input.Close();
        /// Writes an error record specifying that creation of remote runspace
        /// failed.
        /// <param name="e">exception which is causing this error record
        /// to be written</param>
        /// <param name="uri">Uri which caused this exception.</param>
        private void WriteErrorCreateRemoteRunspaceFailed(Exception e, Uri uri)
            Dbg.Assert(e is UriFormatException || e is InvalidOperationException ||
                       e is ArgumentException,
                       "Exception has to be of type UriFormatException or InvalidOperationException or ArgumentException");
            ErrorRecord errorRecord = new ErrorRecord(e, "CreateRemoteRunspaceFailed",
                ErrorCategory.InvalidArgument, uri);
        /// FilePathComputername parameter set.
        protected const string FilePathComputerNameParameterSet = "FilePathComputerName";
        /// LiteralFilePathComputername parameter set.
        protected const string LiteralFilePathComputerNameParameterSet = "LiteralFilePathComputerName";
        /// FilePathRunspace parameter set.
        protected const string FilePathSessionParameterSet = "FilePathRunspace";
        /// FilePathUri parameter set.
        protected const string FilePathUriParameterSet = "FilePathUri";
        /// PS version of the remote server.
        private const string PSv5OrLater = "PSv5OrLater";
        private const string PSv3Orv4 = "PSv3Orv4";
        private System.Management.Automation.PowerShell _powershellV2;
        private System.Management.Automation.PowerShell _powershellV3;
        /// Reads content of file and converts it to a scriptblock.
        /// <param name="isLiteralPath"></param>
        protected ScriptBlock GetScriptBlockFromFile(string filePath, bool isLiteralPath)
            // Make sure filepath doesn't contain wildcards
            if ((!isLiteralPath) && WildcardPattern.ContainsWildcardCharacters(filePath))
                throw new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.WildCardErrorFilePathParameter), nameof(filePath));
                throw new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.FilePathShouldPS1Extension), nameof(filePath));
            // Resolve file path
            string resolvedPath = PathResolver.ResolveProviderAndPath(filePath, isLiteralPath, this, false, RemotingErrorIdStrings.FilePathNotFromFileSystemProvider);
            // read content of file
            ExternalScriptInfo scriptInfo = new ExternalScriptInfo(filePath, resolvedPath, this.Context);
            if (!filePath.EndsWith(".psd1", StringComparison.OrdinalIgnoreCase))
                this.Context.AuthorizationManager.ShouldRunInternal(scriptInfo, CommandOrigin.Internal, this.Context.EngineHostInterface);
            return scriptInfo.ScriptBlock;
        #endregion Protected Members / Methods
                (ParameterSetName == PSExecutionCmdlet.VMNameParameterSet) ||
                (ParameterSetName == PSExecutionCmdlet.ContainerIdParameterSet) ||
                (ParameterSetName == PSExecutionCmdlet.FilePathVMIdParameterSet) ||
                (ParameterSetName == PSExecutionCmdlet.FilePathVMNameParameterSet) ||
                (ParameterSetName == PSExecutionCmdlet.FilePathContainerIdParameterSet))
                SkipWinRMCheck = true;
            if (_filePath != null)
                _scriptBlock = GetScriptBlockFromFile(_filePath, IsLiteralPath);
                case PSExecutionCmdlet.FilePathComputerNameParameterSet:
                case PSExecutionCmdlet.LiteralFilePathComputerNameParameterSet:
                case PSExecutionCmdlet.ComputerNameParameterSet:
                        string[] resolvedComputerNames = null;
                        ResolveComputerNames(ComputerName, out resolvedComputerNames);
                        ResolvedComputerNames = resolvedComputerNames;
                        CreateHelpersForSpecifiedComputerNames();
                case PSExecutionCmdlet.SSHHostParameterSet:
                case PSExecutionCmdlet.FilePathSSHHostParameterSet:
                        ResolveComputerNames(HostName, out resolvedComputerNames);
                        CreateHelpersForSpecifiedSSHComputerNames();
                case PSExecutionCmdlet.SSHHostHashParameterSet:
                case PSExecutionCmdlet.FilePathSSHHostHashParameterSet:
                        CreateHelpersForSpecifiedSSHHashComputerNames();
                case PSExecutionCmdlet.FilePathSessionParameterSet:
                case PSExecutionCmdlet.SessionParameterSet:
                        ValidateRemoteRunspacesSpecified();
                        CreateHelpersForSpecifiedRunspaces();
                case PSExecutionCmdlet.FilePathUriParameterSet:
                case PSExecutionCmdlet.UriParameterSet:
                        CreateHelpersForSpecifiedUris();
                case PSExecutionCmdlet.VMIdParameterSet:
                case PSExecutionCmdlet.VMNameParameterSet:
                case PSExecutionCmdlet.FilePathVMIdParameterSet:
                case PSExecutionCmdlet.FilePathVMNameParameterSet:
                        CreateHelpersForSpecifiedVMSession();
                case PSExecutionCmdlet.ContainerIdParameterSet:
                case PSExecutionCmdlet.FilePathContainerIdParameterSet:
                        CreateHelpersForSpecifiedContainerSession();
        #region "Get PowerShell instance"
        /// Get the PowerShell instance for the PSv2 remote end
        /// Generate the PowerShell instance by using the text of the scriptblock.
        /// PSv2 doesn't understand the '$using' prefix. To make UsingExpression work on PSv2 remote end, we will have to
        /// alter the script, and send the altered script to the remote end. Since the script is altered, when there is an
        /// error, the error message will show the altered script, and that could be confusing to the user. So if the remote
        /// server is PSv3 or later version, we will use a different approach to handle UsingExpression so that we can keep
        /// the script unchanged.
        /// However, on PSv3 and PSv4 remote server, it's not well supported if UsingExpressions are used in different scopes (fixed in PSv5).
        /// If the remote end is PSv3 or PSv4, and there are UsingExpressions in different scopes, then we have to revert back to the approach
        /// used for PSv2 remote server.
        private System.Management.Automation.PowerShell GetPowerShellForPSv2()
            if (_powershellV2 != null) { return _powershellV2; }
            // Try to convert the scriptblock to powershell commands.
            _powershellV2 = ConvertToPowerShell();
            if (_powershellV2 != null)
                // Look for EndOfStatement tokens.
                foreach (var command in _powershellV2.Commands.Commands)
                        // PSv2 cannot process this.  Revert to sending script.
                        _powershellV2 = null;
            List<string> newParameterNames;
            List<object> newParameterValues;
            string scriptTextAdaptedForPSv2 = GetConvertedScript(out newParameterNames, out newParameterValues);
            _powershellV2 = System.Management.Automation.PowerShell.Create().AddScript(scriptTextAdaptedForPSv2);
            if (_args != null)
                foreach (object arg in _args)
                    _powershellV2.AddArgument(arg);
            if (newParameterNames != null)
                Dbg.Assert(newParameterValues != null && newParameterNames.Count == newParameterValues.Count, "We should get the value for each using variable");
                for (int i = 0; i < newParameterNames.Count; i++)
                    _powershellV2.AddParameter(newParameterNames[i], newParameterValues[i]);
            return _powershellV2;
        /// Get the PowerShell instance for the PSv3 (or later) remote end
        /// In PSv3 and PSv4, if the remote server is PSv3 or later, we generate an object array that contains the value of each using expression in
        /// the parsing order, and then pass the array to the remote end as a special argument. On the remote end, the using expressions will be indexed
        /// in the same parsing order during the variable analysis process, and the index is used to get the value of the corresponding using expression
        /// from the special array. There is a limitation in that approach -- $using cannot be used in different scopes with Invoke-Command/Start-Job
        /// (see WinBlue#475223), because the variable analysis process can only index using expressions within the same scope (this is by design), and a
        /// using expression from a different scope may be assigned with an index that conflicts with other using expressions.
        /// To fix the limitation described above, we changed to pass a dictionary with key/value pairs for the using expressions on the client side. The key
        /// is an unique base64 encoded string generated based on the text of the using expression. On the remote end, it can always get the unique key of a
        /// using expression because the text passed to the server side is the same, and thus the value of the using expression can be retrieved from the special
        /// dictionary. With this approach, $using in different scopes can be supported for Invoke-Command/Start-Job.
        /// This fix involved changes on the server side, so the fix will work only if the remote end is PSv5 or later. In order to avoid possible breaking
        /// change in 'PSv5 client - PSv3 server' and 'PSv5 client - PSv4 server' scenarios, we should keep sending the array-form using values if the remote
        /// end is PSv3 or PSv4 as long as no UsingExpression is in a different scope. If the remote end is PSv3 or PSv4 and we do have UsingExpressions
        /// in different scopes, then we will revert back to the approach we use to handle UsingExpression for PSv2 remote server.
        private System.Management.Automation.PowerShell GetPowerShellForPSv3OrLater(string serverPsVersion)
            if (_powershellV3 != null) { return _powershellV3; }
            _powershellV3 = ConvertToPowerShell();
            // Using expressions can be a variable, as well as property and / or array references. E.g.
            // icm { echo $using:a }
            // icm { echo $using:a[3] }
            // icm { echo $using:a.Length }
            // Semantic checks on the using statement have already validated that there are no arbitrary expressions,
            // so we'll allow these expressions in everything but NoLanguage mode.
            bool allowUsingExpressions = Context.SessionState.LanguageMode != PSLanguageMode.NoLanguage;
            object[] usingValuesInArray = null;
            IDictionary usingValuesInDict = null;
            // Value of 'serverPsVersion' should be either 'PSv3Orv4' or 'PSv5OrLater'
            if (serverPsVersion == PSv3Orv4)
                usingValuesInArray = ScriptBlockToPowerShellConverter.GetUsingValuesAsArray(_scriptBlock, allowUsingExpressions, Context, null);
                if (usingValuesInArray == null)
                    // 'usingValuesInArray' will be null only if there are UsingExpressions used in different scopes.
                    // PSv3 and PSv4 remote server cannot handle this, so we revert back to the approach we use for PSv2 remote end.
                    return GetPowerShellForPSv2();
                // Remote server is PSv5 or later version
                usingValuesInDict = ScriptBlockToPowerShellConverter.GetUsingValuesAsDictionary(_scriptBlock, allowUsingExpressions, Context, null);
            string textOfScriptBlock = this.MyInvocation.ExpectingInput
                ? _scriptBlock.GetWithInputHandlingForInvokeCommand()
                : _scriptBlock.ToString();
            _powershellV3 = System.Management.Automation.PowerShell.Create().AddScript(textOfScriptBlock);
                    _powershellV3.AddArgument(arg);
            if (usingValuesInDict != null && usingValuesInDict.Count > 0)
                _powershellV3.AddParameter(Parser.VERBATIM_ARGUMENT, usingValuesInDict);
            else if (usingValuesInArray != null && usingValuesInArray.Length > 0)
                _powershellV3.AddParameter(Parser.VERBATIM_ARGUMENT, usingValuesInArray);
            return _powershellV3;
        private System.Management.Automation.PowerShell ConvertToPowerShell()
            System.Management.Automation.PowerShell powershell = null;
                bool isTrustedInput = Context.LanguageMode == PSLanguageMode.FullLanguage;
                powershell = _scriptBlock.GetPowerShell(isTrustedInput, _args);
                // conversion failed, we need to send the script to the remote end.
                // since the PowerShell instance would be different according to the PSVersion of the remote end,
                // we generate it when we know which version we are talking to.
        #endregion "Get PowerShell instance"
        #region "UsingExpression Utilities"
        /// Get the converted script for a remote PSv2 end.
        /// <param name="newParameterNames">
        /// The new parameter names that we added to the param block
        /// <param name="newParameterValues">
        /// The new parameter values that need to be added to the powershell instance
        private string GetConvertedScript(out List<string> newParameterNames, out List<object> newParameterValues)
            newParameterNames = null; newParameterValues = null;
            string textOfScriptBlock = null;
            // Scan for the using variables
            List<VariableExpressionAst> usingVariables = GetUsingVariables(_scriptBlock);
            if (usingVariables == null || usingVariables.Count == 0)
                // No using variable is used, then we don't change the script
                textOfScriptBlock = this.MyInvocation.ExpectingInput
                newParameterNames = new List<string>();
                var paramNamesWithDollarSign = new List<string>();
                var paramUsingVars = new List<VariableExpressionAst>();
                var nameHashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var varAst in usingVariables)
                    string paramName = UsingExpressionAst.UsingPrefix + varName;
                    string paramNameWithDollar = "$" + paramName;
                    if (!nameHashSet.Contains(paramNameWithDollar))
                        newParameterNames.Add(paramName);
                        paramNamesWithDollarSign.Add(paramNameWithDollar);
                        paramUsingVars.Add(varAst);
                        nameHashSet.Add(paramNameWithDollar);
                // Retrieve the value for each using variable
                newParameterValues = GetUsingVariableValues(paramUsingVars);
                // Generate the wrapped script
                string additionalNewParams = string.Join(", ", paramNamesWithDollarSign);
                    ? _scriptBlock.GetWithInputHandlingForInvokeCommandWithUsingExpression(Tuple.Create(usingVariables, additionalNewParams))
                    : _scriptBlock.ToStringWithDollarUsingHandling(Tuple.Create(usingVariables, additionalNewParams));
            return textOfScriptBlock;
        /// Get the values for the using variables that are passed in.
        /// <param name="paramUsingVars"></param>
        private List<object> GetUsingVariableValues(List<VariableExpressionAst> paramUsingVars)
            var values = new List<object>(paramUsingVars.Count);
            VariableExpressionAst currentVarAst = null;
            Version oldStrictVersion = Context.EngineSessionState.CurrentScope.StrictModeVersion;
                Context.EngineSessionState.CurrentScope.StrictModeVersion = PSVersionInfo.PSVersion;
                // GetExpressionValue ensures that it only does variable access when supplied a VariableExpressionAst.
                // So, this is still safe to use in ConstrainedLanguage and will not result in arbitrary code
                bool allowVariableAccess = Context.SessionState.LanguageMode != PSLanguageMode.NoLanguage;
                foreach (var varAst in paramUsingVars)
                    currentVarAst = varAst;
                    object value = Compiler.GetExpressionValue(varAst, allowVariableAccess, Context);
                    values.Add(value);
                if (rte.ErrorRecord.FullyQualifiedErrorId.Equals("VariableIsUndefined", StringComparison.Ordinal))
                        currentVarAst.Extent, "UsingVariableIsUndefined", AutomationExceptions.UsingVariableIsUndefined, rte.ErrorRecord.TargetObject);
                Context.EngineSessionState.CurrentScope.StrictModeVersion = oldStrictVersion;
        /// Get all Using expressions that we care about.
        /// <param name="localScriptBlock"></param>
        /// <returns>A list of UsingExpressionAsts ordered by the StartOffset.</returns>
        private static List<VariableExpressionAst> GetUsingVariables(ScriptBlock localScriptBlock)
            ArgumentNullException.ThrowIfNull(localScriptBlock, "Caller needs to make sure the parameter value is not null");
            var allUsingExprs = UsingExpressionAstSearcher.FindAllUsingExpressions(localScriptBlock.Ast);
            return allUsingExprs.Select(static usingExpr => UsingExpressionAst.ExtractUsingVariable((UsingExpressionAst)usingExpr)).ToList();
        #endregion "UsingExpression Utilities"
    /// Base class for any cmdlet which operates on a runspace. The
    ///     1. Get-PSSession
    ///     2. Remove-PSSession
    ///     3. Disconnect-PSSession
    ///     4. Connect-PSSession.
    public abstract class PSRunspaceCmdlet : PSRemotingCmdlet
        /// ContainerIdInstanceId parameter set: container id + session instance id.
        protected const string ContainerIdInstanceIdParameterSet = "ContainerIdInstanceId";
        /// VMIdInstanceId parameter set: vm id + session instance id.
        protected const string VMIdInstanceIdParameterSet = "VMIdInstanceId";
        /// VMNameInstanceId parameter set: vm name + session instance id.
        protected const string VMNameInstanceIdParameterSet = "VMNameInstanceId";
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true,
                   ParameterSetName = PSRunspaceCmdlet.InstanceIdParameterSet)]
        public virtual Guid[] InstanceId
                return _remoteRunspaceIds;
                _remoteRunspaceIds = value;
        private Guid[] _remoteRunspaceIds;
        /// Session Id of the remoterunspace info object.
                   ParameterSetName = PSRunspaceCmdlet.IdParameterSet)]
                   ParameterSetName = PSRunspaceCmdlet.NameParameterSet)]
        public virtual string[] Name
        /// Name of the computer for which the runspace needs to be
                   ParameterSetName = PSRunspaceCmdlet.ComputerNameParameterSet)]
        public virtual string[] ComputerName
                return _computerNames;
                _computerNames = value;
        private string[] _computerNames;
                   ParameterSetName = PSRunspaceCmdlet.ContainerIdParameterSet)]
                   ParameterSetName = PSRunspaceCmdlet.ContainerIdInstanceIdParameterSet)]
                   ParameterSetName = PSRunspaceCmdlet.VMIdParameterSet)]
                   ParameterSetName = PSRunspaceCmdlet.VMIdInstanceIdParameterSet)]
                   ParameterSetName = PSRunspaceCmdlet.VMNameParameterSet)]
                   ParameterSetName = PSRunspaceCmdlet.VMNameInstanceIdParameterSet)]
        #region Private / Protected Methods
        /// Gets the matching runspaces based on the parameterset.
        /// <param name="writeErrorOnNoMatch">write an error record when
        /// no matches are found</param>
        /// <param name="writeobject">if true write the object down
        /// the pipeline</param>
        /// <returns>List of matching runspaces.</returns>
        protected Dictionary<Guid, PSSession> GetMatchingRunspaces(bool writeobject,
            bool writeErrorOnNoMatch)
            return GetMatchingRunspaces(writeobject, writeErrorOnNoMatch, SessionFilterState.All, null);
        /// <param name="filterState">Runspace state filter value.</param>
        /// <param name="configurationName">Runspace configuration name filter value.</param>
            bool writeErrorOnNoMatch,
            SessionFilterState filterState,
            string configurationName)
                case PSRunspaceCmdlet.ComputerNameParameterSet:
                        return GetMatchingRunspacesByComputerName(writeobject, writeErrorOnNoMatch);
                case PSRunspaceCmdlet.InstanceIdParameterSet:
                        return GetMatchingRunspacesByRunspaceId(writeobject, writeErrorOnNoMatch);
                case PSRunspaceCmdlet.NameParameterSet:
                        return GetMatchingRunspacesByName(writeobject, writeErrorOnNoMatch);
                case PSRunspaceCmdlet.IdParameterSet:
                        return GetMatchingRunspacesBySessionId(writeobject, writeErrorOnNoMatch);
                // writeErrorOnNoMatch should always be false for container/vm id/name inputs
                // in Get-PSSession/Remove-PSSession cmdlets
                // container id + optional session name
                case PSRunspaceCmdlet.ContainerIdParameterSet:
                        return GetMatchingRunspacesByVMNameContainerId(writeobject, filterState, configurationName, true);
                // container id + session instanceid
                case PSRunspaceCmdlet.ContainerIdInstanceIdParameterSet:
                        return GetMatchingRunspacesByVMNameContainerIdSessionInstanceId(writeobject, filterState, configurationName, true);
                // vm Guid + optional session name
                case PSRunspaceCmdlet.VMIdParameterSet:
                        return GetMatchingRunspacesByVMId(writeobject, filterState, configurationName);
                // vm Guid + session instanceid
                case PSRunspaceCmdlet.VMIdInstanceIdParameterSet:
                        return GetMatchingRunspacesByVMIdSessionInstanceId(writeobject, filterState, configurationName);
                // vm name + optional session name
                case PSRunspaceCmdlet.VMNameParameterSet:
                        return GetMatchingRunspacesByVMNameContainerId(writeobject, filterState, configurationName, false);
                // vm name + session instanceid
                case PSRunspaceCmdlet.VMNameInstanceIdParameterSet:
                        return GetMatchingRunspacesByVMNameContainerIdSessionInstanceId(writeobject, filterState, configurationName, false);
        internal Dictionary<Guid, PSSession> GetAllRunspaces(bool writeobject,
            Dictionary<Guid, PSSession> matches = new Dictionary<Guid, PSSession>();
            List<PSSession> remoteRunspaceInfos = this.RunspaceRepository.Runspaces;
            foreach (PSSession remoteRunspaceInfo in remoteRunspaceInfos)
                // return all remote runspace info objects
                if (writeobject)
                    WriteObject(remoteRunspaceInfo);
                    matches.Add(remoteRunspaceInfo.InstanceId, remoteRunspaceInfo);
        /// Gets the matching runspaces by computernames.
        private Dictionary<Guid, PSSession> GetMatchingRunspacesByComputerName(bool writeobject,
            if (_computerNames == null || _computerNames.Length == 0)
                return GetAllRunspaces(writeobject, writeErrorOnNoMatch);
            // Loop through all computer-name patterns and runspaces to find matches.
            foreach (string computerName in _computerNames)
                WildcardPattern computerNamePattern = WildcardPattern.Get(computerName, WildcardOptions.IgnoreCase);
                // Match the computer-name patterns against all the runspaces and remember the matches.
                    if (computerNamePattern.IsMatch(remoteRunspaceInfo.ComputerName))
                                // if match already found ignore
                // If no match found write an error record.
                if (!found && writeErrorOnNoMatch)
                    WriteInvalidArgumentError(PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedComputer, RemotingErrorIdStrings.RemoteRunspaceNotAvailableForSpecifiedComputer,
                        computerName);
        /// Gets the matching runspaces based on name.
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "writeobject")]
        protected Dictionary<Guid, PSSession> GetMatchingRunspacesByName(bool writeobject,
            foreach (string name in _names)
                    if (namePattern.IsMatch(remoteRunspaceInfo.Name))
                if (!found && writeErrorOnNoMatch && !WildcardPattern.ContainsWildcardCharacters(name))
                    WriteInvalidArgumentError(PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedName, RemotingErrorIdStrings.RemoteRunspaceNotAvailableForSpecifiedName,
        /// Gets the matching runspaces based on the runspaces instance id.
        protected Dictionary<Guid, PSSession> GetMatchingRunspacesByRunspaceId(bool writeobject,
            foreach (Guid remoteRunspaceId in _remoteRunspaceIds)
                    if (remoteRunspaceId.Equals(remoteRunspaceInfo.InstanceId))
                    WriteInvalidArgumentError(PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedRunspaceId, RemotingErrorIdStrings.RemoteRunspaceNotAvailableForSpecifiedRunspaceId,
                        remoteRunspaceId);
        /// Gets the matching runspaces based on the session id (the
        /// short integer id which is unique for a runspace)
        private Dictionary<Guid, PSSession> GetMatchingRunspacesBySessionId(bool writeobject,
            foreach (int sessionId in Id)
                    if (sessionId == remoteRunspaceInfo.Id)
                    WriteInvalidArgumentError(PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedSessionId, RemotingErrorIdStrings.RemoteRunspaceNotAvailableForSpecifiedSessionId,
                        sessionId);
        /// Gets the matching runspaces by vm name or container id with optional session name.
        /// <param name="writeobject">If true write the object down the pipeline.</param>
        /// <param name="isContainer">If true the target is a container instead of virtual machine.</param>
        private Dictionary<Guid, PSSession> GetMatchingRunspacesByVMNameContainerId(bool writeobject,
            bool isContainer)
            string[] inputNames;
            TargetMachineType computerType;
            bool supportWildChar;
            string[] sessionNames = { "*" };
            WildcardPattern configurationNamePattern =
                string.IsNullOrEmpty(configurationName) ? null : WildcardPattern.Get(configurationName, WildcardOptions.IgnoreCase);
            // vm name support wild characters, while container id does not.
            // vm id does not apply in this method, which does not support wild characters either.
                inputNames = ContainerId;
                computerType = TargetMachineType.Container;
                supportWildChar = false;
                inputNames = VMName;
                computerType = TargetMachineType.VirtualMachine;
                supportWildChar = true;
            // When "-name" is not set, we use "*" that means matching all.
                sessionNames = Name;
            foreach (string inputName in inputNames)
                WildcardPattern inputNamePattern = WildcardPattern.Get(inputName, WildcardOptions.IgnoreCase);
                foreach (string sessionName in sessionNames)
                    WildcardPattern sessionNamePattern =
                        string.IsNullOrEmpty(sessionName) ? null : WildcardPattern.Get(sessionName, WildcardOptions.IgnoreCase);
                    var matchingRunspaceInfos = remoteRunspaceInfos
                        .Where<PSSession>(session => (supportWildChar ? inputNamePattern.IsMatch(session.VMName)
                                                                      : inputName.Equals(session.ContainerId)) &&
                                                     (sessionNamePattern == null || sessionNamePattern.IsMatch(session.Name)) &&
                                                     QueryRunspaces.TestRunspaceState(session.Runspace, filterState) &&
                                                     (configurationNamePattern == null || configurationNamePattern.IsMatch(session.ConfigurationName)) &&
                                                     (session.ComputerType == computerType))
                        .ToList<PSSession>();
                    WriteOrAddMatches(matchingRunspaceInfos, writeobject, ref matches);
        /// Gets the matching runspaces by vm name or container id with session instanceid.
        private Dictionary<Guid, PSSession> GetMatchingRunspacesByVMNameContainerIdSessionInstanceId(bool writeobject,
                foreach (Guid sessionInstanceId in InstanceId)
                                                     sessionInstanceId.Equals(session.InstanceId) &&
        /// Gets the matching runspaces by vm guid and optional session name.
        private Dictionary<Guid, PSSession> GetMatchingRunspacesByVMId(bool writeobject,
            // When "-name" is not set, we use "*" that means matching all .
            foreach (Guid vmId in VMId)
                        .Where<PSSession>(session => vmId.Equals(session.VMId) &&
                                                     (session.ComputerType == TargetMachineType.VirtualMachine))
        /// Gets the matching runspaces by vm guid and session instanceid.
        private Dictionary<Guid, PSSession> GetMatchingRunspacesByVMIdSessionInstanceId(bool writeobject,
        /// Write the matching runspace objects down the pipeline, or add to the list.
        /// <param name="matchingRunspaceInfos">The matching runspaces.</param>
        /// <param name="writeobject">If true write the object down the pipeline. Otherwise, add to the list.</param>
        /// <param name="matches">The list we add the matching runspaces to.</param>
        private void WriteOrAddMatches(List<PSSession> matchingRunspaceInfos,
            bool writeobject,
            ref Dictionary<Guid, PSSession> matches)
            foreach (PSSession remoteRunspaceInfo in matchingRunspaceInfos)
        /// Write invalid argument error.
        private void WriteInvalidArgumentError(PSRemotingErrorId errorId, string resourceString, object errorArgument)
            string message = GetMessage(resourceString, errorArgument);
            WriteError(new ErrorRecord(new ArgumentException(message), errorId.ToString(),
                ErrorCategory.InvalidArgument, errorArgument));
        #endregion Private / Protected Methods
        /// Runspace Id parameter set.
        protected const string InstanceIdParameterSet = "InstanceId";
        /// Session id parameter set.
        protected const string IdParameterSet = "Id";
        /// Name parameter set.
        protected const string NameParameterSet = "Name";
    /// Base class for both the helpers. This is an abstract class
    internal abstract class ExecutionCmdletHelper : IThrottleOperation
        /// Pipeline associated with this operation.
        internal Pipeline Pipeline
        protected Pipeline pipeline;
        protected Exception internalException;
        /// Internal access to Runspace and Computer helper runspace.
        internal Runspace PipelineRunspace
        #region Runspace Debug
        internal void ConfigureRunspaceDebugging(Runspace runspace)
            if (!RunspaceDebuggingEnabled || (runspace == null) || (runspace.Debugger == null)) { return; }
            runspace.Debugger.DebuggerStop += HandleDebuggerStop;
            // Configure runspace debugger to preserve unhandled stops (wait for debugger attach)
            runspace.Debugger.UnhandledBreakpointMode = UnhandledBreakpointProcessingMode.Wait;
            if (RunspaceDebugStepInEnabled)
                // Configure runspace debugger to run script in step mode
                    runspace.Debugger.SetDebuggerStepMode(true);
        internal void CleanupRunspaceDebugging(Runspace runspace)
            if ((runspace == null) || (runspace.Debugger == null)) { return; }
            runspace.Debugger.DebuggerStop -= HandleDebuggerStop;
        private void HandleDebuggerStop(object sender, DebuggerStopEventArgs args)
            PipelineRunspace.Debugger.DebuggerStop -= HandleDebuggerStop;
            // Forward event
            RaiseRunspaceDebugStopEvent(PipelineRunspace);
            // Signal remote session to remain stopped in debuger
            args.SuspendRemote = true;
    /// Contains a pipeline and calls InvokeAsync on the pipeline
    /// on StartOperation. On StopOperation it calls StopAsync.
    /// The handler sends a StopComplete message in OperationComplete
    /// for both the functions. This is because, there is only a
    /// single state of the pipeline which raises an event on
    /// a method call. There are no separate events raised as
    /// part of method calls.
    internal class ExecutionCmdletHelperRunspace : ExecutionCmdletHelper
        /// Indicates whether or not the server should be using the steppable pipeline.
        internal bool ShouldUseSteppablePipelineOnServer;
        /// <param name="pipeline">Pipeline object associated with this operation.</param>
        internal ExecutionCmdletHelperRunspace(Pipeline pipeline)
            PipelineRunspace = pipeline.Runspace;
        /// Invokes the pipeline asynchronously.
            ConfigureRunspaceDebugging(PipelineRunspace);
                if (ShouldUseSteppablePipelineOnServer && pipeline is RemotePipeline rPipeline)
                    rPipeline.SetIsNested(true);
                    rPipeline.SetIsSteppable(true);
                pipeline.InvokeAsync();
            catch (InvalidRunspaceStateException e)
            catch (InvalidPipelineStateException e)
        /// Closes the pipeline asynchronously.
                // already been raised from the handler
                // will have to raise OperationComplete from here,
        /// Handles the state changed events for the pipeline. This is registered in both
        /// StartOperation and StopOperation. Here nothing more is done excepting raising
        /// the OperationComplete event appropriately which will be handled by the cmdlet.
        /// <param name="sender">Source of this event.</param>
        /// <param name="stateEventArgs">object describing state information about the
        /// pipeline</param>
            RaiseOperationCompleteEvent(stateEventArgs);
        /// Raise an OperationComplete Event. The base event will be
        /// null in this case.
            RaiseOperationCompleteEvent(null);
        /// Raise an operation complete event.
        /// <param name="baseEventArgs">The event args which actually
        /// raises this operation complete</param>
        private void RaiseOperationCompleteEvent(EventArgs baseEventArgs)
            CleanupRunspaceDebugging(PipelineRunspace);
                // Dispose the pipeline object and release data and remoting resources.
                // Pipeline object remains to provide information on final state and any errors incurred.
                    OperationState.StopComplete;
            operationStateEventArgs.BaseEvent = baseEventArgs;
            OperationComplete?.SafeInvoke(this, operationStateEventArgs);
    /// This helper class contains a runspace and
    /// an associated pipeline. On StartOperation it calls
    /// OpenAsync on the runspace. In the handler for runspace,
    /// when the runspace is successfully opened it calls
    /// InvokeAsync on the pipeline. StartOperation
    /// is assumed complete when both the operations complete.
    /// StopOperation will call StopAsync first on the pipeline
    /// and then close the associated runspace. StopOperation
    /// is considered complete when both these operations
    /// complete. The handler sends a StopComplete message in
    /// OperationComplete for both the calls.
    internal class ExecutionCmdletHelperComputerName : ExecutionCmdletHelper
        /// Determines if the command should be invoked and then disconnect the
        /// remote runspace from the client.
        private readonly bool _invokeAndDisconnect;
        /// The remote runspace created using the computer name
        /// parameter set details.
        internal RemoteRunspace RemoteRunspace { get; private set; }
        /// <param name="remoteRunspace">RemoteRunspace that is associated
        /// with this operation</param>
        /// <param name="pipeline">Pipeline created from the remote runspace.</param>
        /// <param name="invokeAndDisconnect">Indicates if pipeline should be disconnected after invoking command.</param>
        internal ExecutionCmdletHelperComputerName(RemoteRunspace remoteRunspace, Pipeline pipeline, bool invokeAndDisconnect = false)
                    "RemoteRunspace reference cannot be null");
            PipelineRunspace = remoteRunspace;
            _invokeAndDisconnect = invokeAndDisconnect;
            RemoteRunspace = remoteRunspace;
            remoteRunspace.StateChanged += HandleRunspaceStateChanged;
            Dbg.Assert(pipeline != null,
                    "Pipeline cannot be null or empty");
        /// Call OpenAsync() on the RemoteRunspace.
                RemoteRunspace.OpenAsync();
            catch (PSRemotingTransportException e)
        /// StopAsync on the pipeline.
            bool needToStop = false; // indicates whether to call StopAsync
                needToStop = true;
            if (needToStop)
                // raise an OperationComplete event here. Else the
                // throttle manager will not respond as it will be waiting for
                // this StopOperation to complete
        /// Handles the state changed event for runspace operations.
        /// <param name="sender">Sender of this information.</param>
        /// <param name="stateEventArgs">Object describing this event.</param>
        private void HandleRunspaceStateChanged(object sender,
                RunspaceStateEventArgs stateEventArgs)
            RunspaceState state = stateEventArgs.RunspaceStateInfo.State;
                        ConfigureRunspaceDebugging(RemoteRunspace);
                        // if successfully opened
                        // Call InvokeAsync() on the pipeline
                            if (_invokeAndDisconnect)
                                pipeline.InvokeAsyncAndDisconnect();
                            RemoteRunspace.CloseAsync();
                        // raise a OperationComplete event with
                        // StopComplete message
                        if (stateEventArgs.RunspaceStateInfo.Reason != null)
        /// Handles the state changed event for the pipeline.
        private void HandlePipelineStateChanged(object sender,
                        PipelineStateEventArgs stateEventArgs)
            PipelineState state = stateEventArgs.PipelineStateInfo.State;
                    RemoteRunspace?.CloseAsync();
            if (RemoteRunspace != null)
                // Dispose of the runspace object.
                RemoteRunspace.Dispose();
                RemoteRunspace = null;
    #region Path Resolver
    /// A helper class to resolve the path.
    internal static class PathResolver
        /// Resolves the specified path and verifies the path belongs to
        /// FileSystemProvider.
        /// <param name="path">Path to resolve.</param>
        /// <param name="isLiteralPath">True if wildcard expansion should be suppressed for this path.</param>
        /// <param name="cmdlet">reference to calling cmdlet. This will be used for
        /// for writing errors</param>
        /// <param name="allowNonexistingPaths"></param>
        /// <param name="resourceString">Resource string for error when path is not from filesystem provider.</param>
        /// <returns>A fully qualified string representing filename.</returns>
        internal static string ResolveProviderAndPath(string path, bool isLiteralPath, PSCmdlet cmdlet, bool allowNonexistingPaths, string resourceString)
            // First resolve path
            PathInfo resolvedPath = ResolvePath(path, isLiteralPath, allowNonexistingPaths, cmdlet);
            if (resolvedPath.Provider.ImplementingType == typeof(FileSystemProvider))
                return resolvedPath.ProviderPath;
            throw PSTraceSource.NewInvalidOperationException(resourceString, resolvedPath.Provider.Name);
        /// Resolves the specified path to PathInfo objects.
        /// <param name="pathToResolve">
        /// The path to be resolved. Each path may contain glob characters.
        /// <param name="isLiteralPath">
        /// True if wildcard expansion should be suppressed for pathToResolve.
        /// Calling cmdlet
        /// A string representing the resolved path.
        private static PathInfo ResolvePath(
            string pathToResolve,
            bool isLiteralPath,
            PSCmdlet cmdlet)
            // Construct cmdletprovidercontext
            CmdletProviderContext cmdContext = new CmdletProviderContext(cmdlet);
            cmdContext.SuppressWildcardExpansion = isLiteralPath;
            Collection<PathInfo> results = new Collection<PathInfo>();
                    cmdlet.SessionState.Path.GetResolvedPSPathFromPSPath(
                        pathToResolve,
                        cmdContext);
                cmdlet.ThrowTerminatingError(
                if (allowNonexistingPaths)
                    System.Management.Automation.PSDriveInfo drive = null;
                        cmdlet.SessionState.Path.GetUnresolvedProviderPathFromPSPath(
                            cmdContext,
                            cmdlet.SessionState);
            else // if (results.Count > 1)
                Exception e = PSTraceSource.NewNotSupportedException();
                    new ErrorRecord(e,
                    "NotSupported",
                    results));
    #region QueryRunspaces
    internal class QueryRunspaces
        internal QueryRunspaces()
            _stopProcessing = false;
        /// Queries all remote computers specified in collection of WSManConnectionInfo objects
        /// and returns disconnected PSSession objects ready for connection to server.
        /// Returned sessions can be matched to Guids or Names.
        /// <param name="connectionInfos">Collection of WSManConnectionInfo objects.</param>
        /// <param name="host">Host for PSSession objects.</param>
        /// <param name="stream">Out stream object.</param>
        /// <param name="runspaceRepository">Runspace repository.</param>
        /// <param name="throttleLimit">Throttle limit.</param>
        /// <param name="matchIds">Array of session Guids to match to.</param>
        /// <param name="matchNames">Array of session Names to match to.</param>
        /// <param name="configurationName">Configuration name to match to.</param>
        internal Collection<PSSession> GetDisconnectedSessions(Collection<WSManConnectionInfo> connectionInfos, PSHost host,
                                                               ObjectStream stream, RunspaceRepository runspaceRepository,
                                                               int throttleLimit, SessionFilterState filterState,
                                                               Guid[] matchIds, string[] matchNames, string configurationName)
            Collection<PSSession> filteredPSSessions = new Collection<PSSession>();
            // Create a query operation for each connection information object.
            foreach (WSManConnectionInfo connectionInfo in connectionInfos)
                Runspace[] runspaces = null;
                    runspaces = Runspace.GetRunspaces(connectionInfo, host, BuiltInTypesTable);
                catch (System.Management.Automation.RuntimeException e)
                    if (e.InnerException is InvalidOperationException)
                        // The Get-WSManInstance cmdlet used to query remote computers for runspaces will throw
                        // an Invalid Operation (inner) exception if the connectInfo object is invalid, including
                        // invalid computer names.
                        // We don't want to propagate the exception so just write error here.
                        if (stream.ObjectWriter != null && stream.ObjectWriter.IsOpen)
                            string msg = StringUtil.Format(RemotingErrorIdStrings.QueryForRunspacesFailed, connectionInfo.ComputerName, ExtractMessage(e.InnerException, out errorCode));
                            string FQEID = WSManTransportManagerUtils.GetFQEIDFromTransportError(errorCode, "RemotePSSessionQueryFailed");
                            Exception reason = new RuntimeException(msg, e.InnerException);
                            ErrorRecord errorRecord = new ErrorRecord(reason, FQEID, ErrorCategory.InvalidOperation, connectionInfo);
                            stream.ObjectWriter.Write((Action<Cmdlet>)(cmdlet => cmdlet.WriteError(errorRecord)));
                if (_stopProcessing)
                // Add all runspaces meeting filter criteria to collection.
                if (runspaces != null)
                    // Convert configuration name into shell Uri for comparison.
                    string shellUri = null;
                    if (!string.IsNullOrEmpty(configurationName))
                        shellUri = configurationName.Contains(WSManNativeApi.ResourceURIPrefix, StringComparison.OrdinalIgnoreCase)
                            ? configurationName
                            : WSManNativeApi.ResourceURIPrefix + configurationName;
                        // Filter returned runspaces by ConfigurationName if provided.
                        if (shellUri != null)
                            // Compare with returned shell Uri in connection info.
                            WSManConnectionInfo wsmanConnectionInfo = runspace.ConnectionInfo as WSManConnectionInfo;
                            if (wsmanConnectionInfo != null &&
                                !shellUri.Equals(wsmanConnectionInfo.ShellUri, StringComparison.OrdinalIgnoreCase))
                        // Check the repository for an existing viable PSSession for
                        // this runspace (based on instanceId).  Use the existing
                        // local runspace instead of the one returned from the server
                        // query.
                        PSSession existingPSSession = null;
                        if (runspaceRepository != null)
                            existingPSSession = runspaceRepository.GetItem(runspace.InstanceId);
                        if (existingPSSession != null &&
                            UseExistingRunspace(existingPSSession.Runspace, runspace))
                            if (TestRunspaceState(existingPSSession.Runspace, filterState))
                                filteredPSSessions.Add(existingPSSession);
                        else if (TestRunspaceState(runspace, filterState))
                            filteredPSSessions.Add(new PSSession(runspace as RemoteRunspace));
            // Return only PSSessions that match provided Ids or Names.
            if ((matchIds != null) && (filteredPSSessions.Count > 0))
                Collection<PSSession> matchIdsSessions = new Collection<PSSession>();
                foreach (Guid id in matchIds)
                    foreach (PSSession psSession in filteredPSSessions)
                        if (psSession.Runspace.InstanceId.Equals(id))
                            matchIdsSessions.Add(psSession);
                    if (!matchFound && stream.ObjectWriter != null && stream.ObjectWriter.IsOpen)
                        string msg = StringUtil.Format(RemotingErrorIdStrings.SessionIdMatchFailed, id);
                        ErrorRecord errorRecord = new ErrorRecord(reason, "PSSessionIdMatchFail", ErrorCategory.InvalidOperation, id);
                // Return all found sessions.
                return matchIdsSessions;
            else if ((matchNames != null) && (filteredPSSessions.Count > 0))
                Collection<PSSession> matchNamesSessions = new Collection<PSSession>();
                foreach (string name in matchNames)
                        if (namePattern.IsMatch(((RemoteRunspace)psSession.Runspace).RunspacePool.RemoteRunspacePoolInternal.Name))
                            matchNamesSessions.Add(psSession);
                        string msg = StringUtil.Format(RemotingErrorIdStrings.SessionNameMatchFailed, name);
                        ErrorRecord errorRecord = new ErrorRecord(reason, "PSSessionNameMatchFail", ErrorCategory.InvalidOperation, name);
                return matchNamesSessions;
                // Return all collected sessions.
                return filteredPSSessions;
        /// Returns true if the existing runspace should be returned to the user
        /// a.  If the existing runspace is not broken
        /// b.  If the queried runspace is not connected to a different user.
        /// <param name="existingRunspace"></param>
        /// <param name="queriedrunspace"></param>
        private static bool UseExistingRunspace(
            Runspace existingRunspace,
            Runspace queriedrunspace)
            Dbg.Assert(existingRunspace != null, "Invalid parameter.");
            Dbg.Assert(queriedrunspace != null, "Invalid parameter.");
            if (existingRunspace.RunspaceStateInfo.State == RunspaceState.Broken)
            if (existingRunspace.RunspaceStateInfo.State == RunspaceState.Disconnected &&
                queriedrunspace.RunspaceAvailability == RunspaceAvailability.Busy)
            // Update existing runspace to have latest DisconnectedOn/ExpiresOn data.
            existingRunspace.DisconnectedOn = queriedrunspace.DisconnectedOn;
            existingRunspace.ExpiresOn = queriedrunspace.ExpiresOn;
        /// Returns Exception message.  If message is WSMan Xml then
        /// the WSMan message and error code is extracted and returned.
        /// <param name="e">Exception.</param>
        /// <param name="errorCode">Returned WSMan error code.</param>
        /// <returns>WSMan message.</returns>
        internal static string ExtractMessage(
            out int errorCode)
            errorCode = 0;
            if (e == null ||
                e.Message == null)
            string rtnMsg = null;
                System.Xml.XmlReaderSettings xmlReaderSettings = InternalDeserializer.XmlReaderSettingsForUntrustedXmlDocument.Clone();
                xmlReaderSettings.MaxCharactersInDocument = 4096;
                xmlReaderSettings.MaxCharactersFromEntities = 1024;
                xmlReaderSettings.DtdProcessing = System.Xml.DtdProcessing.Prohibit;
                using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(
                        new System.IO.StringReader(e.Message), xmlReaderSettings))
                    while (reader.Read())
                        if (reader.NodeType == System.Xml.XmlNodeType.Element)
                            if (reader.LocalName.Equals("Message", StringComparison.OrdinalIgnoreCase))
                                rtnMsg = reader.ReadElementContentAsString();
                            else if (reader.LocalName.Equals("WSManFault", StringComparison.OrdinalIgnoreCase))
                                string errorCodeString = reader.GetAttribute("Code");
                                if (errorCodeString != null)
                                        // WinRM returns both signed and unsigned 32 bit string values.  Convert to signed 32 bit integer.
                                        Int64 eCode = Convert.ToInt64(errorCodeString, System.Globalization.NumberFormatInfo.InvariantInfo);
                                            errorCode = (int)eCode;
            return rtnMsg ?? e.Message;
        /// Discontinue all remote server query operations.
        internal void StopAllOperations()
            _stopProcessing = true;
        /// Compares the runspace filter state with the runspace state.
        /// <param name="runspace">Runspace object to test.</param>
        /// <param name="filterState">Filter state to compare.</param>
        /// <returns>Result of test.</returns>
        public static bool TestRunspaceState(Runspace runspace, SessionFilterState filterState)
            switch (filterState)
                case SessionFilterState.All:
                case SessionFilterState.Opened:
                    result = (runspace.RunspaceStateInfo.State == RunspaceState.Opened);
                case SessionFilterState.Closed:
                    result = (runspace.RunspaceStateInfo.State == RunspaceState.Closed);
                case SessionFilterState.Disconnected:
                    result = (runspace.RunspaceStateInfo.State == RunspaceState.Disconnected);
                case SessionFilterState.Broken:
                    result = (runspace.RunspaceStateInfo.State == RunspaceState.Broken);
                    Dbg.Assert(false, "Invalid SessionFilterState value.");
        /// Returns the default type table for built-in PowerShell types.
        internal static TypeTable BuiltInTypesTable
                if (s_TypeTable == null)
                    lock (s_SyncObject)
                        s_TypeTable ??= TypeTable.LoadDefaultTypeFiles();
                return s_TypeTable;
        private bool _stopProcessing;
        private static readonly object s_SyncObject = new object();
        private static TypeTable s_TypeTable;
    #region SessionFilterState Enum
    /// Runspace states that can be used as filters for querying remote runspaces.
    public enum SessionFilterState
        /// Return runspaces in any state.
        /// Return runspaces in Opened state.
        Opened = 1,
        /// Return runspaces in Disconnected state.
        Disconnected = 2,
        /// Return runspaces in Closed state.
        /// Return runspaces in Broken state.
        Broken = 4
    /// IMPORTANT: proxy configuration is supported for HTTPS only; for HTTP, the direct
    /// connection to the server is used.
        /// ProxyAccessType is not specified. That means Proxy information (ProxyAccessType, ProxyAuthenticationMechanism
        /// and ProxyCredential)is not passed to WSMan at all.
        /// Use the Internet Explorer proxy configuration for the current user.
        ///  Internet Explorer proxy settings for the current active network connection.
        ///  This option requires the user profile to be loaded, so the option can
        ///  be directly used when called within a process that is running under
        ///  an interactive user account identity; if the client application is running
        ///  under a user context different than the interactive user, the client
        ///  application has to explicitly load the user profile prior to using this option.
        IEConfig = 1,
        /// Proxy settings configured for WinHTTP, using the ProxyCfg.exe utility.
        WinHttpConfig = 2,
        /// Force autodetection of proxy.
        AutoDetect = 4,
        /// Do not use a proxy server - resolves all host names locally.
        NoProxyServer = 8
    /// Options for a remote PSSession.
    public sealed class PSSessionOption
        /// Creates a new instance of <see cref="PSSessionOption"/>
        public PSSessionOption()
        /// The MaximumConnectionRedirectionCount parameter enables the implicit redirection functionality.
        public int MaximumConnectionRedirectionCount { get; set; } = WSManConnectionInfo.defaultMaximumConnectionRedirectionCount;
        public bool NoCompression { get; set; } = false;
        public bool NoMachineProfile { get; set; } = false;
        /// - Digest: Use Digest authentication for establishing a remote connection
        /// Default is Negotiate.
        public AuthenticationMechanism ProxyAuthentication
                return _proxyAuthentication;
                        _proxyAuthentication = value;
                        string message = PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.ProxyAmbiguousAuthentication,
                            nameof(AuthenticationMechanism.Basic),
                            nameof(AuthenticationMechanism.Negotiate),
                            nameof(AuthenticationMechanism.Digest));
        private AuthenticationMechanism _proxyAuthentication = AuthenticationMechanism.Negotiate;
        /// The duration for which PowerShell remoting waits before timing out
        /// for any operation. The user would like to tweak this timeout
        /// depending on whether he/she is connecting to a machine in the data
        /// center or across a slow WAN.
        /// Default: 3*60*1000 == 3minutes.
        public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromMilliseconds(BaseTransportManager.ClientDefaultOperationTimeoutMs);
        public bool NoEncryption { get; set; }
        public bool UseUTF16 { get; set; }
        public bool IncludePortInSPN { get; set; }
        /// Determines how server in disconnected state deals with cached output
        /// data when the cache becomes filled.
        /// Default value is 'block mode' where command execution is blocked after
        /// the server side data cache becomes filled.
        public OutputBufferingMode OutputBufferingMode { get; set; } = WSManConnectionInfo.DefaultOutputBufferingMode;
        /// Number of times a connection will be re-attempted when a connection fails due to network
        public int MaxConnectionRetryCount { get; set; } = WSManConnectionInfo.DefaultMaxConnectionRetryCount;
        public int? MaximumReceivedDataSizePerCommand { get; set; }
        /// If null, then the size is unlimited. Default is 200MB object size.
        public int? MaximumReceivedObjectSize { get; set; } = 200 << 20;
        /// The duration for which PowerShell remoting waits before timing out on a connection to a remote machine.
        /// Simply put, the timeout for a remote runspace creation.
        /// Default: 3 * 60 * 1000 = 3 minutes.
        public TimeSpan OpenTimeout { get; set; } = TimeSpan.FromMilliseconds(RunspaceConnectionInfo.DefaultOpenTimeout);
        /// The duration for which PowerShell should wait before it times out on cancel operations
        /// (close runspace or stop powershell). For instance, when the user hits ctrl-C,
        /// New-PSSession cmdlet tries to call a stop on all remote runspaces which are in the Opening state.
        /// The user wouldn't mind waiting for 15 seconds, but this should be time bound and of a shorter duration.
        /// A high timeout here like 3 minutes will give the user a feeling that the PowerShell client is not responding.
        /// Default: 60 * 1000 = 1 minute.
        public TimeSpan CancelTimeout { get; set; } = TimeSpan.FromMilliseconds(RunspaceConnectionInfo.defaultCancelTimeout);
        /// The duration for which a Runspace on server needs to wait before it declares the client dead and closes itself down.
        /// This is especially important as these values may have to be configured differently for enterprise administration
        /// and exchange scenarios.
        /// Default: -1 -> Use current server value for IdleTimeout.
        public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromMilliseconds(RunspaceConnectionInfo.DefaultIdleTimeout);

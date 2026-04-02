    /// This cmdlet connects PS sessions (RemoteRunspaces) that are in the Disconnected
    /// state and returns those PS session objects in the Opened state.  One or more
    /// session objects can be specified for connecting, or a remote computer name can
    /// be specified and in this case all disconnected remote runspaces found on the
    /// remote computer will be connected and PSSession objects created on the local
    /// The cmdlet can be used in the following ways:
    /// Connect a PS session object:
    /// > $session = New-PSSession serverName
    /// > Disconnect-PSSession $session
    /// > Connect-PSSession $session
    /// Connect a PS session by name:
    /// > Connect-PSSession $session.Name
    /// Connect a PS session by Id:
    /// > Connect-PSSession $session.Id
    /// Connect a collection of PS session:
    /// > Get-PSSession | Connect-PSSession
    /// Connect all disconnected PS sessions on a remote computer
    /// > Connect-PSSession serverName.
    [Cmdlet(VerbsCommunications.Connect, "PSSession", SupportsShouldProcess = true, DefaultParameterSetName = ConnectPSSessionCommand.NameParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096694", RemotingCapability = RemotingCapability.OwnedByCommand)]
    [OutputType(typeof(PSSession))]
    public class ConnectPSSessionCommand : PSRunspaceCmdlet, IDisposable
        private const string ComputerNameGuidParameterSet = "ComputerNameGuid";
        private const string ConnectionUriParameterSet = "ConnectionUri";
        private const string ConnectionUriGuidParameterSet = "ConnectionUriGuid";
        /// The PSSession object or objects to be connected.
                   ParameterSetName = ConnectPSSessionCommand.SessionParameterSet)]
        public PSSession[] Session { get; set; }
        /// Computer names to connect to.
                   ParameterSetName = ConnectPSSessionCommand.ComputerNameParameterSet,
                   Mandatory = true)]
        [Parameter(ParameterSetName = ConnectPSSessionCommand.ComputerNameGuidParameterSet,
        public override string[] ComputerName { get; set; }
        /// This parameters specifies the appname which identifies the connection
        /// end point on the remote machine. If this parameter is not specified
        /// then the value specified in DEFAULTREMOTEAPPNAME will be used. If that's
        /// not specified as well, then "WSMAN" will be used.
                   ParameterSetName = ConnectPSSessionCommand.ComputerNameParameterSet)]
                   ParameterSetName = ConnectPSSessionCommand.ComputerNameGuidParameterSet)]
                return _appName;
                _appName = ResolveAppName(value);
        private string _appName;
        /// If this parameter is not specified then the value specified in
        /// the environment variable DEFAULTREMOTESHELLNAME will be used. If
        /// this is not set as well, then Microsoft.PowerShell is used.
                   ParameterSetName = ConnectPSSessionCommand.ConnectionUriParameterSet)]
                   ParameterSetName = ConnectPSSessionCommand.ConnectionUriGuidParameterSet)]
        public string ConfigurationName
                return _shell;
                _shell = ResolveShell(value);
        private string _shell;
        /// A complete URI(s) specified for the remote computer and shell to
        /// connect to and create a runspace for.
        [Alias("URI", "CU")]
        public Uri[] ConnectionUri { get; set; }
        /// The AllowRedirection parameter enables the implicit redirection functionality.
        [Parameter(ParameterSetName = ConnectPSSessionCommand.ConnectionUriParameterSet)]
        [Parameter(ParameterSetName = ConnectPSSessionCommand.ConnectionUriGuidParameterSet)]
        public SwitchParameter AllowRedirection
            get { return _allowRedirection; }
            set { _allowRedirection = value; }
        private bool _allowRedirection = false;
        /// RemoteRunspaceId to retrieve corresponding PSSession
        [Parameter(ParameterSetName = ConnectPSSessionCommand.ConnectionUriGuidParameterSet,
        [Parameter(ParameterSetName = ConnectPSSessionCommand.InstanceIdParameterSet,
        public override Guid[] InstanceId
            get { return base.InstanceId; }
            set { base.InstanceId = value; }
        /// Name of the remote runspaceinfo object.
        [Parameter(ParameterSetName = ConnectPSSessionCommand.NameParameterSet,
        [Parameter(ParameterSetName = ConnectPSSessionCommand.ComputerNameParameterSet)]
        public override string[] Name
            get { return base.Name; }
            set { base.Name = value; }
        /// Specifies the credentials of the user to impersonate in the
        /// remote machine. If this parameter is not specified then the
        /// credentials of the current user process will be assumed.
        [Parameter(ParameterSetName = ConnectPSSessionCommand.ComputerNameGuidParameterSet)]
                return _psCredential;
                _psCredential = value;
                PSRemotingBaseCmdlet.ValidateSpecifiedAuthentication(Credential, CertificateThumbprint, Authentication);
        private PSCredential _psCredential;
        /// Use basic authentication to authenticate the user.
                return _authentication;
                _authentication = value;
        private AuthenticationMechanism _authentication;
                return _thumbprint;
                _thumbprint = value;
        private string _thumbprint;
        /// Port specifies the alternate port to be used in case the
        /// default ports are not used for the transport mechanism
        /// (port 80 for http and port 443 for useSSL)
        /// Currently this is being accepted as a parameter. But in future
        /// support will be added to make this a part of a policy setting.
        /// When a policy setting is in place this parameter can be used
        /// to override the policy setting
        [ValidateRange((int)1, (int)UInt16.MaxValue)]
        /// This parameter suggests that the transport scheme to be used for
        /// remote connections is useSSL instead of the default http.Since
        /// there are only two possible transport schemes that are possible
        /// at this point, a SwitchParameter is being used to switch between
        /// the two.
        public SwitchParameter UseSSL { get; set; }
        /// Extended session options.  Used in this cmdlet to set server disconnect options.
        public PSSessionOption SessionOption { get; set; }
        /// Allows the user of the cmdlet to specify a throttling value
        /// for throttling the number of remote operations that can
        /// be executed simultaneously.
        [Parameter(ParameterSetName = ConnectPSSessionCommand.SessionParameterSet)]
        [Parameter(ParameterSetName = ConnectPSSessionCommand.IdParameterSet)]
        [Parameter(ParameterSetName = ConnectPSSessionCommand.NameParameterSet)]
        [Parameter(ParameterSetName = ConnectPSSessionCommand.InstanceIdParameterSet)]
        public int ThrottleLimit { get; set; } = 0;
        /// Overriding to suppress this parameter.
        public override string[] ContainerId
        public override Guid[] VMId
        public override string[] VMName
        /// Set up the ThrottleManager for runspace connect processing.
            _throttleManager.ThrottleLimit = ThrottleLimit;
            _throttleManager.ThrottleComplete += HandleThrottleConnectComplete;
        /// Perform runspace connect processing on all input.
            Collection<PSSession> psSessions;
                if (ParameterSetName == ConnectPSSessionCommand.ComputerNameParameterSet ||
                    ParameterSetName == ConnectPSSessionCommand.ComputerNameGuidParameterSet ||
                    ParameterSetName == ConnectPSSessionCommand.ConnectionUriParameterSet ||
                    ParameterSetName == ConnectPSSessionCommand.ConnectionUriGuidParameterSet)
                    // Query remote computers for disconnected sessions.
                    psSessions = QueryForDisconnectedSessions();
                    // Collect provided disconnected sessions.
                    psSessions = CollectDisconnectedSessions();
            catch (PSRemotingDataStructureException)
                // Allow cmdlet to end and then re-throw exception.
                _operationsComplete.Set();
            catch (RemoteException)
            ConnectSessions(psSessions);
        /// End processing clean up.
            // Wait for all connect operations to complete.
            _operationsComplete.WaitOne();
            // If there are failed connect operations due to stale
            // session state then perform the query retry here.
            if (_failedSessions.Count > 0)
                RetryFailedSessions();
            // Read all objects in the stream pipeline.
            while (_stream.ObjectReader.Count > 0)
                object streamObject = _stream.ObjectReader.Read();
                WriteStreamObject((Action<Cmdlet>)streamObject);
            _stream.ObjectWriter.Close();
            // Add all successfully connected sessions to local repository.
            foreach (PSSession psSession in _allSessions)
                if (psSession.Runspace.RunspaceStateInfo.State == RunspaceState.Opened)
                    // Make sure that this session is included in the PSSession repository.
                    // If it already exists then replace it because we want the latest/connected session in the repository.
                    this.RunspaceRepository.AddOrReplace(psSession);
        /// User has signaled a stop for this cmdlet.
            // Close the output stream for any further writes.
            // Stop any remote server queries that may be running.
            _queryRunspaces.StopAllOperations();
            // Signal the ThrottleManager to stop any further
            // PSSession connect processing.
            // Signal the Retry throttle manager in case it is running.
            _retryThrottleManager.StopAllOperations();
        #region ConnectRunspaceOperation Class
        /// Throttle class to perform a remoterunspace connect operation.
        private sealed class ConnectRunspaceOperation : IThrottleOperation
            private PSSession _session;
            private PSSession _oldSession;
            private readonly ObjectStream _writeStream;
            private readonly Collection<PSSession> _retryList;
            private readonly QueryRunspaces _queryRunspaces;
            private static readonly object s_LockObject = new object();
            internal ConnectRunspaceOperation(
                PSSession session,
                ObjectStream stream,
                QueryRunspaces queryRunspaces,
                Collection<PSSession> retryList)
                _writeStream = stream;
                _host = host;
                _queryRunspaces = queryRunspaces;
                _retryList = retryList;
                _session.Runspace.StateChanged += StateCallBackHandler;
                    if (_queryRunspaces != null)
                        PSSession newSession = QueryForSession(_session);
                        if (newSession != null)
                            _session.Runspace.StateChanged -= StateCallBackHandler;
                            _oldSession = _session;
                            _session = newSession;
                            _session.Runspace.ConnectAsync();
                    WriteConnectFailed(ex, _session);
                    // We are done at this point.  Notify throttle manager.
                _queryRunspaces?.StopAllOperations();
            internal PSSession QueryForSession(PSSession session)
                Collection<WSManConnectionInfo> wsManConnectionInfos = new Collection<WSManConnectionInfo>();
                wsManConnectionInfos.Add(session.Runspace.ConnectionInfo as WSManConnectionInfo);
                Collection<PSSession> sessions = null;
                    sessions = _queryRunspaces.GetDisconnectedSessions(wsManConnectionInfos, _host, _writeStream, null,
                        0, SessionFilterState.Disconnected, new Guid[] { session.InstanceId }, null, null);
                    WriteConnectFailed(ex, session);
                if (sessions.Count != 1)
                    ex = new RuntimeException(StringUtil.Format(RemotingErrorIdStrings.CannotFindSessionForConnect,
                        session.Name, session.ComputerName));
                return sessions[0];
            private void StateCallBackHandler(object sender, RunspaceStateEventArgs eArgs)
                if (eArgs.RunspaceStateInfo.State == RunspaceState.Connecting ||
                    eArgs.RunspaceStateInfo.State == RunspaceState.Disconnecting ||
                    eArgs.RunspaceStateInfo.State == RunspaceState.Disconnected)
                Dbg.Assert(eArgs.RunspaceStateInfo.State != RunspaceState.BeforeOpen, "Can't reconnect a session that hasn't been previously Opened");
                Dbg.Assert(eArgs.RunspaceStateInfo.State != RunspaceState.Opening, "Can't reconnect a session that hasn't been previously Opened");
                if (eArgs.RunspaceStateInfo.State == RunspaceState.Opened)
                    // Connect operation succeeded, write the PSSession object.
                    WriteConnectedPSSession();
                    // Check to see if failure is due to stale PSSession error and
                    // add to retry list if this is the case.
                    bool writeError = true;
                    if (_queryRunspaces == null)
                        PSRemotingTransportException transportException = eArgs.RunspaceStateInfo.Reason as PSRemotingTransportException;
                            transportException.ErrorCode == WSManNativeApi.ERROR_WSMAN_INUSE_CANNOT_RECONNECT)
                            lock (s_LockObject)
                                _retryList.Add(_session);
                        // Connect operation failed, write error.
                        WriteConnectFailed(eArgs.RunspaceStateInfo.Reason, _session);
            private void SendStopComplete()
            private void WriteConnectedPSSession()
                // Use temporary variable because we need to preserve _session class variable
                // for later clean up.
                PSSession outSession = _session;
                        // Pass back the original session if possible.
                        if (_oldSession != null &&
                            _oldSession.InsertRunspace(_session.Runspace as RemoteRunspace))
                            outSession = _oldSession;
                            _retryList.Add(_oldSession);
                if (_writeStream.ObjectWriter.IsOpen)
                    // This code is based on ThrottleManager infrastructure
                    // and this particular method may be called on a thread that
                    // is different from Pipeline Execution Thread. Hence using
                    // a delegate to perform the WriteObject.
                    Action<Cmdlet> outputWriter = (Cmdlet cmdlet) => cmdlet.WriteObject(outSession);
                    _writeStream.ObjectWriter.Write(outputWriter);
            private void WriteConnectFailed(
                PSSession session)
                    string FQEID = "PSSessionConnectFailed";
                    Exception reason;
                    if (e != null && !string.IsNullOrEmpty(e.Message))
                        // Update fully qualified error Id if we have a transport error.
                        PSRemotingTransportException transportException = e as PSRemotingTransportException;
                        if (transportException != null)
                            FQEID = WSManTransportManagerUtils.GetFQEIDFromTransportError(transportException.ErrorCode, FQEID);
                        reason = new RuntimeException(
                            StringUtil.Format(RemotingErrorIdStrings.RunspaceConnectFailedWithMessage, session.Name, e.Message),
                            StringUtil.Format(RemotingErrorIdStrings.RunspaceConnectFailed, session.Name,
                                session.Runspace.RunspaceStateInfo.State.ToString()), null);
                    ErrorRecord errorRecord = new ErrorRecord(reason, FQEID, ErrorCategory.InvalidOperation, null);
                    Action<Cmdlet> errorWriter = (Cmdlet cmdlet) => cmdlet.WriteError(errorRecord);
                    _writeStream.ObjectWriter.Write(errorWriter);
        /// Enum indicating an override on which parameter is used to filter
        /// local sessions.
        private enum OverrideParameter
            /// No override.
            /// Use the Name parameter as a filter.
            Name = 1,
            /// Use the InstanceId parameter as a filter.
            InstanceId = 2
        /// Retrieves a collection of disconnected PSSession objects queried from
        /// remote computers.
        /// <returns>Collection of disconnected PSSession objects.</returns>
        private Collection<PSSession> QueryForDisconnectedSessions()
            Collection<WSManConnectionInfo> connectionInfos = GetConnectionObjects();
            Collection<PSSession> psSessions = _queryRunspaces.GetDisconnectedSessions(connectionInfos, this.Host, _stream,
                                                    this.RunspaceRepository, ThrottleLimit,
                                                    SessionFilterState.Disconnected, this.InstanceId, this.Name, ConfigurationName);
            // Write any error output from stream object.
            Collection<object> streamObjects = _stream.ObjectReader.NonBlockingRead();
            foreach (object streamObject in streamObjects)
            return psSessions;
        /// Creates a collection of PSSession objects based on cmdlet parameters.
        /// <param name="overrideParam">OverrideParameter.</param>
        /// <returns>Collection of PSSession objects in disconnected state.</returns>
        private Collection<PSSession> CollectDisconnectedSessions(OverrideParameter overrideParam = OverrideParameter.None)
            Collection<PSSession> psSessions = new Collection<PSSession>();
            // Get all remote runspaces to disconnect.
            if (ParameterSetName == DisconnectPSSessionCommand.SessionParameterSet)
                if (Session != null)
                    foreach (PSSession psSession in Session)
                        psSessions.Add(psSession);
                Dictionary<Guid, PSSession> entries = null;
                switch (overrideParam)
                    case OverrideParameter.None:
                        entries = GetMatchingRunspaces(false, true);
                    case OverrideParameter.Name:
                        entries = GetMatchingRunspacesByName(false, true);
                    case OverrideParameter.InstanceId:
                        entries = GetMatchingRunspacesByRunspaceId(false, true);
                if (entries != null)
                    foreach (PSSession psSession in entries.Values)
        /// Connect all disconnected sessions.
        private void ConnectSessions(Collection<PSSession> psSessions)
            List<IThrottleOperation> connectOperations = new List<IThrottleOperation>();
            // Create a disconnect operation for each runspace to disconnect.
            foreach (PSSession psSession in psSessions)
                if (ShouldProcess(psSession.Name, VerbsCommunications.Connect))
                    if (psSession.ComputerType != TargetMachineType.RemoteMachine)
                        // PS session disconnection is not supported for VM/Container sessions.
                        string msg = StringUtil.Format(RemotingErrorIdStrings.RunspaceCannotBeConnectedForVMContainerSession,
                            psSession.Name, psSession.ComputerName, psSession.ComputerType);
                        Exception reason = new PSNotSupportedException(msg);
                        ErrorRecord errorRecord = new ErrorRecord(reason, "CannotConnectVMContainerSession", ErrorCategory.InvalidOperation, psSession);
                    else if (psSession.Runspace.RunspaceStateInfo.State == RunspaceState.Disconnected &&
                        psSession.Runspace.RunspaceAvailability == RunspaceAvailability.None)
                        // Can only connect sessions that are in Disconnected state.
                        // Update session connection information based on cmdlet parameters.
                        UpdateConnectionInfo(psSession.Runspace.ConnectionInfo as WSManConnectionInfo);
                        ConnectRunspaceOperation connectOperation = new ConnectRunspaceOperation(
                            psSession,
                            _stream,
                            this.Host,
                            _failedSessions);
                        connectOperations.Add(connectOperation);
                    else if (psSession.Runspace.RunspaceStateInfo.State != RunspaceState.Opened)
                        // Write error record if runspace is not already in the Opened state.
                        string msg = StringUtil.Format(RemotingErrorIdStrings.RunspaceCannotBeConnected, psSession.Name);
                        ErrorRecord errorRecord = new ErrorRecord(reason, "PSSessionConnectFailed", ErrorCategory.InvalidOperation, psSession);
                        // Session is already connected.  Write to output.
                        WriteObject(psSession);
                _allSessions.Add(psSession);
            if (connectOperations.Count > 0)
                // Make sure operations are not set as complete while processing input.
                _operationsComplete.Reset();
                // Submit list of connect operations.
                _throttleManager.SubmitOperations(connectOperations);
                // Write any output now.
        /// Handles the connect throttling complete event from the ThrottleManager.
        /// <param name="sender">Sender.</param>
        /// <param name="eventArgs">EventArgs.</param>
        private void HandleThrottleConnectComplete(object sender, EventArgs eventArgs)
        private Collection<WSManConnectionInfo> GetConnectionObjects()
            Collection<WSManConnectionInfo> connectionInfos = new Collection<WSManConnectionInfo>();
                ParameterSetName == ConnectPSSessionCommand.ComputerNameGuidParameterSet)
                string scheme = UseSSL.IsPresent ? WSManConnectionInfo.HttpsScheme : WSManConnectionInfo.HttpScheme;
                    WSManConnectionInfo connectionInfo = new WSManConnectionInfo();
                    connectionInfo.Scheme = scheme;
                    connectionInfo.ComputerName = ResolveComputerName(computerName);
                    connectionInfo.AppName = ApplicationName;
                    connectionInfo.ShellUri = ConfigurationName;
                    connectionInfo.Port = Port;
                    if (CertificateThumbprint != null)
                        connectionInfo.CertificateThumbprint = CertificateThumbprint;
                        connectionInfo.Credential = Credential;
                    connectionInfo.AuthenticationMechanism = Authentication;
                    UpdateConnectionInfo(connectionInfo);
                    connectionInfos.Add(connectionInfo);
            else if (ParameterSetName == ConnectPSSessionCommand.ConnectionUriParameterSet ||
                foreach (var connectionUri in ConnectionUri)
                    connectionInfo.ConnectionUri = connectionUri;
            return connectionInfos;
        /// Updates connection info with the data read from cmdlet's parameters.
        /// <param name="connectionInfo"></param>
        private void UpdateConnectionInfo(WSManConnectionInfo connectionInfo)
            if (ParameterSetName != ConnectPSSessionCommand.ConnectionUriParameterSet &&
                ParameterSetName != ConnectPSSessionCommand.ConnectionUriGuidParameterSet)
                // uri redirection is supported only with URI parameter set
                connectionInfo.MaximumConnectionRedirectionCount = 0;
            if (!_allowRedirection)
                // uri redirection required explicit user consent
            // Update the connectionInfo object with passed in session options.
            if (SessionOption != null)
                connectionInfo.SetSessionOptions(SessionOption);
        private void RetryFailedSessions()
            using (ManualResetEvent retrysComplete = new ManualResetEvent(false))
                Collection<PSSession> connectedSessions = new Collection<PSSession>();
                List<IThrottleOperation> retryConnectionOperations = new List<IThrottleOperation>();
                _retryThrottleManager.ThrottleLimit = ThrottleLimit;
                _retryThrottleManager.ThrottleComplete += (sender, eventArgs) =>
                            retrysComplete.Set();
                foreach (var session in _failedSessions)
                    retryConnectionOperations.Add(new ConnectRunspaceOperation(
                        new QueryRunspaces(),
                        connectedSessions));
                _retryThrottleManager.SubmitOperations(retryConnectionOperations);
                _retryThrottleManager.EndSubmitOperations();
                retrysComplete.WaitOne();
                // Add or replace all successfully connected sessions to the local repository.
                foreach (var session in connectedSessions)
                    this.RunspaceRepository.AddOrReplace(session);
        /// Dispose method of IDisposable. Gets called in the following cases:
        ///     1. Pipeline explicitly calls dispose on cmdlets
        ///     2. Called by the garbage collector.
        /// Internal dispose method which does the actual
        /// dispose operations and finalize suppressions.
        /// <param name="disposing">Whether method is called
        /// from Dispose or destructor</param>
                _operationsComplete.Dispose();
                _throttleManager.ThrottleComplete -= HandleThrottleConnectComplete;
                _retryThrottleManager.Dispose();
                _stream.Dispose();
        // Collection of PSSessions to be connected.
        private readonly Collection<PSSession> _allSessions = new Collection<PSSession>();
        // Object used to perform network disconnect operations in a limited manner.
        // Event indicating that all disconnect operations through the ThrottleManager
        // are complete.
        private readonly ManualResetEvent _operationsComplete = new ManualResetEvent(true);
        // Object used for querying remote runspaces.
        private readonly QueryRunspaces _queryRunspaces = new QueryRunspaces();
        // Object to collect output data from multiple threads.
        private readonly ObjectStream _stream = new ObjectStream();
        // Support for connection retry on failure.
        private readonly ThrottleManager _retryThrottleManager = new ThrottleManager();
        private readonly Collection<PSSession> _failedSessions = new Collection<PSSession>();

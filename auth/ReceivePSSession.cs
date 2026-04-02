    /// This cmdlet connects a running command associated with a PS session and then
    /// directs the command output either:
    /// a) To Host.  This is the synchronous mode of the cmdlet which won't return
    ///    until the running command completes and all output data is received on
    ///    the client.
    /// b) To a job object.  This is the asynchronous mode of the cmdlet which will
    ///    return immediately providing the job object that is collecting the
    ///    running command output data.
    /// The running command becomes disconnected when the associated runspace is
    /// disconnected (via the Disconnect-PSSession cmdlet).
    /// The associated runspace object must be in the Opened state (connected) before
    /// the running command can be connected.  If the associated runspace object is
    /// in the disconnected state, it will first be connected before the running
    /// command is connected.
    /// The user can specify how command output data is returned by using the public
    /// OutTarget enumeration (Host, Job).
    /// The default actions of this cmdlet is to always direct output to host unless
    /// a job object already exists on the client that is associated with the running
    /// command.  In this case the existing job object is connected to the running
    /// command and returned.
    /// Receive PS session data by session object
    /// > $job1 = Invoke-Command $session { [script] } -asjob
    /// > Receive-PSSession $session    // command output continues collecting at job object.
    /// Receive PS session data by session Id
    /// > Receive-PSSession $session.Id
    /// Receive PS session data by session instance Id
    /// > Receive-PSSession $session.InstanceId
    /// Receive PS session data by session Name.  Direct output to job
    /// > Receive-PSSession $session.Name
    /// Receive a running command from a computer.
    /// > $job = Receive-PSSession -ComputerName ServerOne -Name SessionName -OutTarget Job.
    [Cmdlet(VerbsCommunications.Receive, "PSSession", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Low,
        DefaultParameterSetName = ReceivePSSessionCommand.SessionParameterSet, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096800",
        RemotingCapability = RemotingCapability.OwnedByCommand)]
    public class ReceivePSSessionCommand : PSRemotingCmdlet
        private const string NameParameterSet = "SessionName";
        private const string ComputerSessionNameParameterSet = "ComputerSessionName";   // Computer name and session Name.
        private const string ConnectionUriSessionNameParameterSet = "ConnectionUriSessionName";
        private const string ConnectionUriInstanceIdParameterSet = "ConnectionUriInstanceId";
        /// The PSSession object to receive data from.
                   ParameterSetName = ReceivePSSessionCommand.SessionParameterSet)]
        /// Session Id of PSSession object to receive data from.
                   ParameterSetName = ReceivePSSessionCommand.IdParameterSet)]
        /// Computer name to receive session data from.
                   ParameterSetName = ReceivePSSessionCommand.ComputerSessionNameParameterSet)]
                   ParameterSetName = ReceivePSSessionCommand.ComputerInstanceIdParameterSet)]
                   ParameterSetName = ReceivePSSessionCommand.ConnectionUriSessionNameParameterSet)]
                   ParameterSetName = ReceivePSSessionCommand.ConnectionUriInstanceIdParameterSet)]
        public Uri ConnectionUri { get; set; }
        [Parameter(ParameterSetName = ReceivePSSessionCommand.ConnectionUriSessionNameParameterSet)]
        [Parameter(ParameterSetName = ReceivePSSessionCommand.ConnectionUriInstanceIdParameterSet)]
        /// Instance Id of PSSession object to receive data from.
                   ParameterSetName = ReceivePSSessionCommand.InstanceIdParameterSet)]
        /// Name of PSSession object to receive data from.
                   ParameterSetName = ReceivePSSessionCommand.NameParameterSet)]
        /// Determines how running command output is returned on client.
        [Parameter(ParameterSetName = ReceivePSSessionCommand.SessionParameterSet)]
        [Parameter(ParameterSetName = ReceivePSSessionCommand.IdParameterSet)]
        [Parameter(ParameterSetName = ReceivePSSessionCommand.InstanceIdParameterSet)]
        [Parameter(ParameterSetName = ReceivePSSessionCommand.NameParameterSet)]
        [Parameter(ParameterSetName = ReceivePSSessionCommand.ComputerInstanceIdParameterSet)]
        [Parameter(ParameterSetName = ReceivePSSessionCommand.ComputerSessionNameParameterSet)]
        public OutTarget OutTarget { get; set; } = OutTarget.Default;
        /// Provides job name when job is created for returned data.
        public string JobName { get; set; } = string.Empty;
        /// Session options.
        /// Process input.
            if (ParameterSetName == ReceivePSSessionCommand.ComputerSessionNameParameterSet ||
                ParameterSetName == ReceivePSSessionCommand.ConnectionUriSessionNameParameterSet)
                QueryForAndConnectCommands(Name, Guid.Empty);
            else if (ParameterSetName == ReceivePSSessionCommand.ComputerInstanceIdParameterSet ||
                     ParameterSetName == ReceivePSSessionCommand.ConnectionUriInstanceIdParameterSet)
                QueryForAndConnectCommands(string.Empty, InstanceId);
                GetAndConnectSessionCommand();
            RemotePipeline tmpPipeline;
            Job tmpJob;
                tmpPipeline = _remotePipeline;
                tmpJob = _job;
            tmpPipeline?.StopAsync();
            tmpJob?.StopJob();
        /// Queries the remote computer for the specified session, creates a disconnected
        /// session object, connects the runspace/command and collects command data.
        /// Command output is either returned (OutTarget.Host) or collected
        /// in a job object that is returned (OutTarget.Job).
        /// <param name="name">Name of session to find.</param>
        /// <param name="instanceId">Instance Id of session to find.</param>
        private void QueryForAndConnectCommands(string name, Guid instanceId)
            WSManConnectionInfo connectionInfo = GetConnectionObject();
            // Retrieve all disconnected runspaces on the remote computer.
            Runspace[] runspaces;
                runspaces = Runspace.GetRunspaces(connectionInfo, this.Host, QueryRunspaces.BuiltInTypesTable);
                string msg = StringUtil.Format(RemotingErrorIdStrings.QueryForRunspacesFailed, connectionInfo.ComputerName,
                    QueryRunspaces.ExtractMessage(e.InnerException, out errorCode));
                string FQEID = WSManTransportManagerUtils.GetFQEIDFromTransportError(errorCode, "ReceivePSSessionQueryForSessionFailed");
            if (!string.IsNullOrEmpty(ConfigurationName))
                shellUri = ConfigurationName.Contains(WSManNativeApi.ResourceURIPrefix, StringComparison.OrdinalIgnoreCase)
                    ? ConfigurationName
                    : WSManNativeApi.ResourceURIPrefix + ConfigurationName;
            // Connect selected runspace/command and direct command output to host
            // or job objects.
                // Find specified session.
                if (!string.IsNullOrEmpty(name) &&
                    string.Equals(name, ((RemoteRunspace)runspace).RunspacePool.RemoteRunspacePoolInternal.Name, StringComparison.OrdinalIgnoreCase))
                    // Selected by friendly name.
                else if (instanceId.Equals(runspace.InstanceId))
                    // Selected by instance Id (note that session/runspace/runspacepool instanceIds are identical.)
                if (haveMatch &&
                    ShouldProcess(((RemoteRunspace)runspace).PSSessionName, VerbsCommunications.Receive))
                    // Check the local repository for an existing viable session.
                    PSSession locSession = this.RunspaceRepository.GetItem(runspace.InstanceId);
                    // Connect the session here.  If it fails (connectedSession == null) revert to the
                    // reconstruct method.
                    Exception ex;
                    PSSession connectedSession = ConnectSession(locSession, out ex);
                    if (connectedSession != null)
                        // Make sure that this connected session is included in the PSSession repository.
                        this.RunspaceRepository.AddOrReplace(connectedSession);
                        // Since we have a local runspace we will do a *reconnect* operation and will
                        // need the corresponding job object.
                        PSRemotingJob job = FindJobForSession(connectedSession);
                        if (this.OutTarget == OutTarget.Host)
                            ConnectSessionToHost(connectedSession, job);
                            // Connection to Job is default option.
                            ConnectSessionToJob(connectedSession, job);
                        // Otherwise create a new session from the queried runspace object.
                        // This will be a *reconstruct* operation.
                        // Create and connect session.
                        PSSession newSession = new PSSession(runspace as RemoteRunspace);
                        connectedSession = ConnectSession(newSession, out ex);
                            // Try to reuse the existing local repository PSSession object.
                            if (locSession != null)
                                connectedSession = locSession.InsertRunspace(connectedSession.Runspace as RemoteRunspace) ? locSession : connectedSession;
                            if (this.OutTarget == OutTarget.Job)
                                ConnectSessionToJob(connectedSession);
                                // Connection to Host is default option.
                                ConnectSessionToHost(connectedSession);
                            string message = StringUtil.Format(RemotingErrorIdStrings.RunspaceCannotBeConnected, newSession.Name);
                            WriteError(new ErrorRecord(new ArgumentException(message, ex), "ReceivePSSessionCannotConnectSession",
                                       ErrorCategory.InvalidOperation, newSession));
        private WSManConnectionInfo GetConnectionObject()
                ParameterSetName == ReceivePSSessionCommand.ComputerInstanceIdParameterSet)
                // Create the WSManConnectionInfo object for the specified computer name.
                connectionInfo.ComputerName = ResolveComputerName(ComputerName);
            return connectionInfo;
            if (ParameterSetName != ReceivePSSessionCommand.ConnectionUriInstanceIdParameterSet &&
                ParameterSetName != ReceivePSSessionCommand.ConnectionUriSessionNameParameterSet)
        /// Gets the PSSession object to connect based on Id, Name, etc.
        /// Connects the running command associated with the PSSession runspace object.
        private void GetAndConnectSessionCommand()
            PSSession session = null;
            if (ParameterSetName == ReceivePSSessionCommand.SessionParameterSet)
                session = Session;
            else if (ParameterSetName == ReceivePSSessionCommand.IdParameterSet)
                session = GetSessionById(Id);
                    WriteInvalidArgumentError(PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedSessionId,
                                              RemotingErrorIdStrings.RemoteRunspaceNotAvailableForSpecifiedSessionId,
                                              Id);
            else if (ParameterSetName == ReceivePSSessionCommand.NameParameterSet)
                session = GetSessionByName(Name);
                    WriteInvalidArgumentError(PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedName,
                                              RemotingErrorIdStrings.RemoteRunspaceNotAvailableForSpecifiedName,
            else if (ParameterSetName == ReceivePSSessionCommand.InstanceIdParameterSet)
                session = GetSessionByInstanceId(InstanceId);
                    WriteInvalidArgumentError(PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedRunspaceId,
                                              RemotingErrorIdStrings.RemoteRunspaceNotAvailableForSpecifiedRunspaceId,
                                              InstanceId);
                Dbg.Assert(false, "Invalid Parameter Set");
            if (session.ComputerType != TargetMachineType.RemoteMachine)
                string msg = StringUtil.Format(RemotingErrorIdStrings.RunspaceCannotBeReceivedForVMContainerSession,
                    session.Name, session.ComputerName, session.ComputerType);
                ErrorRecord errorRecord = new ErrorRecord(reason, "CannotReceiveVMContainerSession", ErrorCategory.InvalidOperation, session);
            if (ShouldProcess(session.Name, VerbsCommunications.Receive))
                if (ConnectSession(session, out ex) == null)
                    // Unable to connect runspace.  If this was a *reconnect* runspace then try
                    // obtaining a connectable runspace directly from the server and do a
                    // *reconstruct* connect.
                    PSSession oldSession = session;
                    session = TryGetSessionFromServer(oldSession);
                        // No luck.  Return error.
                        string message = StringUtil.Format(RemotingErrorIdStrings.RunspaceCannotBeConnected, oldSession.Name);
                                   ErrorCategory.InvalidOperation, oldSession));
                // Look to see if there exists a job associated with this runspace.
                // If so then we use this job object, unless the user explicitly specifies
                // output to host.
                PSRemotingJob job = FindJobForSession(session);
                    // Default is to route data to job.
                    if (OutTarget == OutTarget.Host)
                        // This performs a *reconstruct* connection scenario where a new
                        // pipeline object is created and connected.
                        ConnectSessionToHost(session, job);
                        // This preforms a *reconnect* scenario where the existing job
                        // and runspace objects are reconnected.
                        ConnectSessionToJob(session, job);
                    // Default is to route data to host.
                    if (OutTarget == OutTarget.Job)
                        // This performs a *reconstruct* connection scenario where new
                        // pipeline/job objects are created and connected.
                        ConnectSessionToJob(session);
                        ConnectSessionToHost(session);
                // Make sure that if this session is successfully connected that it is included
                // in the PSSession repository.  If it already exists then replace it because we
                // want the latest/connected session in the repository.
                if (session.Runspace.RunspaceStateInfo.State != RunspaceState.Disconnected)
        private bool CheckForDebugMode(PSSession session, bool monitorAvailabilityChange)
                DisconnectAndStopRunningCmds(remoteRunspace);
                WriteDebugStopWarning();
            if (monitorAvailabilityChange)
                // Monitor runspace availability transition to RemoteDebug
                remoteRunspace.AvailabilityChanged += HandleRunspaceAvailabilityChanged;
            if ((e.RunspaceAvailability == RunspaceAvailability.RemoteDebug))
                RemoteRunspace remoteRunspace = sender as RemoteRunspace;
                remoteRunspace.AvailabilityChanged -= HandleRunspaceAvailabilityChanged;
        private void DisconnectAndStopRunningCmds(RemoteRunspace remoteRunspace)
            // Disconnect runspace to stop command from running and to allow reconnect
            // via the Enter-PSSession cmdlet.
            if (remoteRunspace.RunspaceStateInfo.State == RunspaceState.Opened)
                Job job;
                ManualResetEvent stopPipelineReceive;
                    job = _job;
                    stopPipelineReceive = _stopPipelineReceive;
                remoteRunspace.Disconnect();
                    stopPipelineReceive?.Set();
                job?.StopJob();
        private void WriteDebugStopWarning()
                GetMessage(RemotingErrorIdStrings.ReceivePSSessionInDebugMode));
            WriteObject(string.Empty);
        /// Connects session, retrieves command output data and writes to host.
        /// <param name="session">PSSession object.</param>
        /// <param name="job">Job object associated with session.</param>
        private void ConnectSessionToHost(PSSession session, PSRemotingJob job = null)
            Dbg.Assert(remoteRunspace != null, "PS sessions can only contain RemoteRunspace type.");
                // If we have a job object associated with the session then this means
                // the user explicitly chose to connect and return data synchronously.
                // Reconnect the job object and stream data to host.
                lock (_syncObject) { _job = job; _stopPipelineReceive = new ManualResetEvent(false); }
                using (_stopPipelineReceive)
                using (job)
                    Job childJob = job.ChildJobs[0];
                    job.ConnectJobs();
                    if (CheckForDebugMode(session, true))
                        // Retrieve and display results from child job as they become
                        int index = WaitHandle.WaitAny(new WaitHandle[] {
                            _stopPipelineReceive,
                            childJob.Results.WaitHandle });
                        foreach (var result in childJob.ReadAll())
                            result?.WriteStreamObject(this);
                    while (!job.IsFinishedState(job.JobStateInfo.State));
                lock (_syncObject) { _job = null; _stopPipelineReceive = null; }
            // Otherwise this must be a new disconnected session object that has a running command
            // associated with it.
            if (remoteRunspace.RemoteCommand == null)
                // There is no associated running command for this runspace, so we cannot proceed.
                // Check to see if session is in debug mode.
                CheckForDebugMode(session, false);
            // Create a RemotePipeline object for this command and attempt to connect.
                _remotePipeline = (RemotePipeline)session.Runspace.CreateDisconnectedPipeline();
                _stopPipelineReceive = new ManualResetEvent(false);
                using (_remotePipeline)
                    // Connect to remote running command.
                    ManualResetEvent pipelineConnectedEvent = new ManualResetEvent(false);
                    using (pipelineConnectedEvent)
                        _remotePipeline.StateChanged += (sender, args) =>
                                if (pipelineConnectedEvent != null &&
                                    (args.PipelineStateInfo.State == PipelineState.Running ||
                                     args.PipelineStateInfo.State == PipelineState.Stopped ||
                                     args.PipelineStateInfo.State == PipelineState.Failed))
                                    pipelineConnectedEvent.Set();
                        pipelineConnectedEvent.WaitOne();
                    pipelineConnectedEvent = null;
                    // Wait for remote command to complete, while writing any available data.
                    while (!_remotePipeline.Output.EndOfPipeline)
                            _remotePipeline.Output.WaitHandle });
                        while (_remotePipeline.Output.Count > 0)
                            PSObject psObject = _remotePipeline.Output.Read();
                            WriteRemoteObject(psObject, session);
                    // Write pipeline object errors.
                    if (_remotePipeline.Error.Count > 0)
                        while (!_remotePipeline.Error.EndOfPipeline)
                            object errorObj = _remotePipeline.Error.Read();
                            if (errorObj is Collection<ErrorRecord>)
                                Collection<ErrorRecord> errorCollection = (Collection<ErrorRecord>)errorObj;
                                foreach (ErrorRecord errorRecord in errorCollection)
                            else if (errorObj is ErrorRecord)
                                WriteError((ErrorRecord)errorObj);
                                Dbg.Assert(false, "Objects in pipeline Error collection must be ErrorRecord type.");
                    // Wait for pipeline to finish.
                    int wIndex = WaitHandle.WaitAny(new WaitHandle[] {
                            _remotePipeline.PipelineFinishedEvent });
                    if (wIndex == 0)
                    // Set the runspace RemoteCommand to null.  It is not needed anymore and it
                    // allows the runspace to become available after pipeline completes.
                    // Check for any terminating errors to report.
                    if (_remotePipeline.PipelineStateInfo.State == PipelineState.Failed)
                        Exception reason = _remotePipeline.PipelineStateInfo.Reason;
                        if (reason != null && !string.IsNullOrEmpty(reason.Message))
                            msg = StringUtil.Format(RemotingErrorIdStrings.PipelineFailedWithReason, reason.Message);
                            msg = RemotingErrorIdStrings.PipelineFailedWithoutReason;
                        ErrorRecord errorRecord = new ErrorRecord(new RuntimeException(msg, reason),
                                                            "ReceivePSSessionPipelineFailed",
                                                            _remotePipeline
            lock (_syncObject) { _remotePipeline = null; _stopPipelineReceive = null; }
        /// Helper method to append computer name and session GUID
        /// note properties to the PSObject before it is written.
        /// <param name="psObject">PSObject.</param>
        /// <param name="session">PSSession.</param>
        private void WriteRemoteObject(
            PSObject psObject,
            // Add note properties for this session if they don't already exist.
            if (psObject.Properties[RemotingConstants.ComputerNameNoteProperty] == null)
                psObject.Properties.Add(new PSNoteProperty(RemotingConstants.ComputerNameNoteProperty, session.ComputerName));
            if (psObject.Properties[RemotingConstants.RunspaceIdNoteProperty] == null)
                psObject.Properties.Add(new PSNoteProperty(RemotingConstants.RunspaceIdNoteProperty, session.InstanceId));
            if (psObject.Properties[RemotingConstants.ShowComputerNameNoteProperty] == null)
                psObject.Properties.Add(new PSNoteProperty(RemotingConstants.ShowComputerNameNoteProperty, true));
            WriteObject(psObject);
        /// Connects session, collects command output data in a job object.
        /// If a PSRemotingJob object is passed in then that job will be
        /// (re)connected.  Otherwise a new job object will be created that
        /// will be connected to the session's running command.
        /// <param name="job">Job object to connect to.</param>
        private void ConnectSessionToJob(PSSession session, PSRemotingJob job = null)
            // Otherwise create a new job object in the disconnected state for this
            // session and then connect it.
            bool newJobCreated = false;
                // The PSRemoting job object uses helper objects to track remote command execution.
                List<IThrottleOperation> helpers = new List<IThrottleOperation>();
                // Create the remote pipeline object that will represent the running command
                // on the server machine.  This object will be in the disconnected state.
                Pipeline remotePipeline = session.Runspace.CreateDisconnectedPipeline();
                // Create a disconnected runspace helper for this remote command.
                helpers.Add(new DisconnectedJobOperation(remotePipeline));
                // Create the job object in a disconnected state.  Note that the job name
                // will be autogenerated.
                job = new PSRemotingJob(helpers, 0, JobName, false);
                job.PSJobTypeName = InvokeCommandCommand.RemoteJobType;
                job.HideComputerName = false;
                newJobCreated = true;
                // Connect the job to the remote command running on the server.
                job.ConnectJob(session.Runspace.InstanceId);
                // Add the created job to the store if it was connected successfully.
                if (newJobCreated)
                    JobRepository.Add(job);
            // Write the job object to output.
        /// Helper method to connect the runspace.  If the session/runspace can't
        /// be connected or fails to be connected then a null PSSessionobject is
        /// <param name="session">Session to connect.</param>
        /// <param name="ex">Optional exception object.</param>
        /// <returns>Connected session or null.</returns>
        private static PSSession ConnectSession(PSSession session, out Exception ex)
            ex = null;
            if (session == null ||
                (session.Runspace.RunspaceStateInfo.State != RunspaceState.Opened &&
                 session.Runspace.RunspaceStateInfo.State != RunspaceState.Disconnected))
            else if (session.Runspace.RunspaceStateInfo.State == RunspaceState.Opened)
                session.Runspace.Connect();
            return (ex == null) ? session : null;
        /// Helper method to attempt to retrieve a disconnected runspace object
        /// from the server, based on the provided session object.
        /// <param name="session">PSSession session object.</param>
        /// <returns>PSSession disconnected runspace object.</returns>
        private PSSession TryGetSessionFromServer(PSSession session)
            if (session.Runspace is not RemoteRunspace remoteRunspace)
            Runspace[] runspaces = Runspace.GetRunspaces(session.Runspace.ConnectionInfo, this.Host, QueryRunspaces.BuiltInTypesTable);
                if (runspace.InstanceId == session.Runspace.InstanceId)
                    remoteRunspace = runspace as RemoteRunspace;
                // Try inserting connected runspace into existing PSSession.
                session = session.InsertRunspace(remoteRunspace) ? session : new PSSession(remoteRunspace);
        /// Helper method to search the local PS client job repository
        /// for a job associated with the provided session.
        /// <returns>Associated job object from the job repository.</returns>
        private PSRemotingJob FindJobForSession(PSSession session)
            PSRemotingJob job = null;
            RemoteRunspace remoteSessionRunspace = session.Runspace as RemoteRunspace;
            if (remoteSessionRunspace == null ||
                remoteSessionRunspace.RemoteCommand != null)
                // The provided session is created for *reconstruction* and we
                // cannot connect a previous job even if one exists.  A new job
                // will have to be created.
            foreach (Job repJob in this.JobRepository.Jobs)
                if (repJob is PSRemotingJob)
                    foreach (PSRemotingChildJob childJob in repJob.ChildJobs)
                        if (childJob.Runspace.InstanceId.Equals(session.InstanceId) &&
                            job = (PSRemotingJob)repJob;
        /// Searches runspace repository for session by Id.
        /// <returns>PSSession object.</returns>
        private PSSession GetSessionById(int id)
            foreach (PSSession session in this.RunspaceRepository.Runspaces)
                if (session.Id == id)
        /// Searches runspace repository for session by Name.
        /// <param name="name">Name to match.</param>
        private PSSession GetSessionByName(string name)
                if (namePattern.IsMatch(session.Name))
        /// Searches runspace repository for session by InstanceId.
        /// <param name="instanceId">InstanceId to match.</param>
        private PSSession GetSessionByInstanceId(Guid instanceId)
                if (instanceId.Equals(session.InstanceId))
        private RemotePipeline _remotePipeline;
        private ManualResetEvent _stopPipelineReceive;
    #region OutTarget Enum
    /// Output modes available to the Receive-PSSession cmdlet.
    public enum OutTarget
        /// Default mode.  If.
        /// Synchronous mode.  Receive-PSSession output data goes to host (returned by cmdlet object).
        Host = 1,
        /// Asynchronous mode.  Receive-PSSession output data goes to returned job object.
        Job = 2

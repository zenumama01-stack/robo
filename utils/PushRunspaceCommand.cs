    /// Enter-PSSession cmdlet.
    [Cmdlet(VerbsCommon.Enter, "PSSession", DefaultParameterSetName = "ComputerName",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096695", RemotingCapability = RemotingCapability.OwnedByCommand)]
    public class EnterPSSessionCommand : PSRemotingBaseCmdlet
        private const string InstanceIdParameterSet = "InstanceId";
        /// Disable ThrottleLimit parameter inherited from base class.
        public new int ThrottleLimit { get { return 0; } set { } }
        private ObjectStream _stream;
        private RemoteRunspace _tempRunspace;
        #region SSH Parameter Set
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true,
        public new string HostName { get; set; }
        /// Computer name parameter.
            ValueFromPipelineByPropertyName = true, ParameterSetName = ComputerNameParameterSet)]
        public new string ComputerName { get; set; }
        /// Runspace parameter.
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true, ParameterSetName = SessionParameterSet)]
        public new PSSession Session { get; set; }
        /// ConnectionUri parameter.
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true,
            ParameterSetName = UriParameterSet)]
        public new Uri ConnectionUri { get; set; }
        /// RemoteRunspaceId of the remote runspace info object.
        ParameterSetName = InstanceIdParameterSet)]
        public Guid InstanceId { get; set; }
        /// SessionId of the remote runspace info object.
             ParameterSetName = IdParameterSet)]
        /// Name of the remote runspace info object.
        ParameterSetName = NameParameterSet)]
        [Parameter(ParameterSetName = ComputerNameParameterSet)]
        [Parameter(ParameterSetName = UriParameterSet)]
        public SwitchParameter EnableNetworkAccess { get; set; }
        /// Virtual machine ID.
            ValueFromPipelineByPropertyName = true, ParameterSetName = VMIdParameterSet)]
        public new Guid VMId { get; set; }
        /// Virtual machine name.
            ValueFromPipelineByPropertyName = true, ParameterSetName = VMNameParameterSet)]
        public new string VMName { get; set; }
        /// virtual machine. If this parameter is not specified then the
        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true,
                   ParameterSetName = VMIdParameterSet)]
                   ParameterSetName = VMNameParameterSet)]
            get { return base.Credential; }
            set { base.Credential = value; }
        /// The Id of the target container.
            ValueFromPipelineByPropertyName = true, ParameterSetName = ContainerIdParameterSet)]
        public new string ContainerId { get; set; }
        /// For WSMan sessions:
                   ParameterSetName = EnterPSSessionCommand.ComputerNameParameterSet)]
                   ParameterSetName = EnterPSSessionCommand.UriParameterSet)]
                   ParameterSetName = EnterPSSessionCommand.ContainerIdParameterSet)]
                   ParameterSetName = EnterPSSessionCommand.VMIdParameterSet)]
                   ParameterSetName = EnterPSSessionCommand.VMNameParameterSet)]
        #region Suppress PSRemotingBaseCmdlet SSH hash parameter set
        /// Suppress SSHConnection parameter set.
                if ((ParameterSetName == EnterPSSessionCommand.ComputerNameParameterSet) ||
                    (ParameterSetName == EnterPSSessionCommand.UriParameterSet))
            // Push the remote runspace on the local host.
            // for the console host and Graphical PowerShell host
            // we want to skip pushing into the runspace if
            // the host is in a nested prompt
            if (!IsParameterSetForVM() &&
                !IsParameterSetForContainer() &&
                !IsParameterSetForVMContainerSession() &&
                chost != null && chost.HostInNestedPrompt())
                    new InvalidOperationException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.HostInNestedPrompt)),
                    "HostInNestedPrompt", ErrorCategory.InvalidOperation, chost));
            /*Microsoft.Windows.PowerShell.Gui.Internal.GPSHost ghost = this.Host as Microsoft.Windows.PowerShell.Gui.Internal.GPSHost;
            if (ghost != null && ghost.HostInNestedPrompt())
                    new InvalidOperationException(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.HostInNestedPrompt)),
                    "HostInNestedPrompt", ErrorCategory.InvalidOperation, wpshost));
            // Get the remote runspace.
                case ComputerNameParameterSet:
                    remoteRunspace = CreateRunspaceWhenComputerNameParameterSpecified();
                case UriParameterSet:
                    remoteRunspace = CreateRunspaceWhenUriParameterSpecified();
                case SessionParameterSet:
                    remoteRunspace = (RemoteRunspace)Session.Runspace;
                    remoteRunspace = GetRunspaceMatchingRunspaceId(this.InstanceId);
                case IdParameterSet:
                    remoteRunspace = GetRunspaceMatchingSessionId(this.Id);
                    remoteRunspace = GetRunspaceMatchingName(this.Name);
                case VMIdParameterSet:
                case VMNameParameterSet:
                    remoteRunspace = GetRunspaceForVMSession();
                case ContainerIdParameterSet:
                    remoteRunspace = GetRunspaceForContainerSession();
                case SSHHostParameterSet:
                    remoteRunspace = GetRunspaceForSSHSession();
            // If runspace is null then the error record has already been written and we can exit.
            // If the runspace is in a disconnected state try to connect.
            bool runspaceConnected = false;
            if (remoteRunspace.RunspaceStateInfo.State == RunspaceState.Disconnected)
                if (!remoteRunspace.CanConnect)
                    string message = StringUtil.Format(RemotingErrorIdStrings.SessionNotAvailableForConnection);
                            new RuntimeException(message), "EnterPSSessionCannotConnectDisconnectedSession",
                                ErrorCategory.InvalidOperation, remoteRunspace));
                // Connect the runspace.
                    remoteRunspace.Connect();
                    runspaceConnected = true;
                catch (System.Management.Automation.Remoting.PSRemotingTransportException e)
                    string message = StringUtil.Format(RemotingErrorIdStrings.SessionConnectFailed);
                            new RuntimeException(message, ex), "EnterPSSessionConnectSessionFailed",
            // Verify that the runspace is open.
            if (remoteRunspace.RunspaceStateInfo.State != RunspaceState.Opened)
                if (ParameterSetName == SessionParameterSet)
                    string sessionName = (Session != null) ? Session.Name : string.Empty;
                            new ArgumentException(GetMessage(RemotingErrorIdStrings.EnterPSSessionBrokenSession,
                                sessionName, remoteRunspace.ConnectionInfo.ComputerName, remoteRunspace.InstanceId)),
                            nameof(PSRemotingErrorId.PushedRunspaceMustBeOpen),
                            new ArgumentException(GetMessage(RemotingErrorIdStrings.PushedRunspaceMustBeOpen)),
                if (host.Runspace != null)
                    debugger = host.Runspace.Debugger;
            bool supportRunningCommand = ((debugger != null) && ((debugger.DebugMode & DebugModes.RemoteScript) == DebugModes.RemoteScript));
            if (remoteRunspace.RunspaceAvailability != RunspaceAvailability.Available)
                // Session has running command.
                if (!supportRunningCommand)
                    // Host does not support remote debug and cannot connect to running command.
                    if (runspaceConnected)
                        // If we succeeded connecting the session (runspace) but it is already running a command,
                        // emit an error for this case because since it is disconnected this session object will
                        // never complete and the user must use *reconstruct* scenario to retrieve data.
                        string message = StringUtil.Format(RemotingErrorIdStrings.EnterPSSessionDisconnected,
                                                        remoteRunspace.PSSessionName);
                                new RuntimeException(message), "EnterPSSessionConnectSessionNotAvailable",
                                    ErrorCategory.InvalidOperation, Session));
                        // Leave session in original disconnected state.
                        remoteRunspace.DisconnectAsync();
                        // If the remote runspace is currently not available then let user know that this command
                        // will not complete until it becomes available.
                        WriteWarning(GetMessage(RunspaceStrings.RunspaceNotReady));
                    // Running commands supported.
                    // Warn user that they are entering a session that is running a command and output may
                    // be going to a job object.
                    Job job = FindJobForRunspace(remoteRunspace.InstanceId);
                        msg = StringUtil.Format(
                            RunspaceStrings.RunningCmdWithJob,
                            (!string.IsNullOrEmpty(job.Name)) ? job.Name : string.Empty);
                        if (remoteRunspace.RunspaceAvailability == RunspaceAvailability.RemoteDebug)
                                RunspaceStrings.RunningCmdDebugStop);
                                RunspaceStrings.RunningCmdWithoutJob);
            // Make sure any PSSession object passed in is saved in the local runspace repository.
                this.RunspaceRepository.AddOrReplace(Session);
            // prepare runspace for prompt
            SetRunspacePrompt(remoteRunspace);
                host.PushRunspace(remoteRunspace);
                // A third-party host can throw any exception here..we should
                // clean the runspace created in this case.
                if ((remoteRunspace != null) && (remoteRunspace.ShouldCloseOnPop))
                    remoteRunspace.Close();
                // rethrow the exception after cleanup.
        /// This method will until the runspace is opened and warnings if any
        /// are reported.
            if (_stream != null)
                    // Keep reading objects until end of stream is encountered
                    _stream.ObjectReader.WaitHandle.WaitOne();
                    if (!_stream.ObjectReader.EndOfPipeline)
            var remoteRunspace = _tempRunspace;
                    remoteRunspace.CloseAsync();
        /// Create temporary remote runspace.
        private RemoteRunspace CreateTemporaryRemoteRunspace(PSHost host, WSManConnectionInfo connectionInfo)
            // Create and open the runspace.
            int rsId;
            string rsName = PSSession.GenerateRunspaceName(out rsId);
            RemoteRunspace remoteRunspace = new RemoteRunspace(
                this.SessionOption.ApplicationArguments,
                rsName,
                rsId);
            Dbg.Assert(remoteRunspace != null, "Expected remoteRunspace != null");
            remoteRunspace.URIRedirectionReported += HandleURIDirectionReported;
            _stream = new ObjectStream();
                // Mark this temporary runspace so that it closes on pop.
                // unregister uri redirection handler
                remoteRunspace.URIRedirectionReported -= HandleURIDirectionReported;
                // close the internal object stream after runspace is opened
                // Runspace.Open() might throw exceptions..this will make sure
                // the stream is always closed.
                // make sure we dispose the temporary runspace if something bad happens
                    remoteRunspace.Dispose();
                    remoteRunspace = null;
        /// Write error create remote runspace failed.
        private void WriteErrorCreateRemoteRunspaceFailed(Exception exception, object argument)
                        exception as PSRemotingTransportException;
            if ((transException != null) &&
                (transException.ErrorCode ==
                    System.Management.Automation.Remoting.Client.WSManNativeApi.ERROR_WSMAN_REDIRECT_REQUESTED))
                errorDetails = message;
            ErrorRecord errorRecord = new ErrorRecord(exception, argument,
                "CreateRemoteRunspaceFailed",
            Action<Cmdlet> streamObject = (Cmdlet cmdlet) => cmdlet.WriteWarning(message);
            _stream.Write(streamObject);
        /// Create runspace when computer name parameter specified.
        private RemoteRunspace CreateRunspaceWhenComputerNameParameterSpecified()
            string resolvedComputerName = ResolveComputerName(ComputerName);
                WSManConnectionInfo connectionInfo = null;
                connectionInfo = new WSManConnectionInfo();
                connectionInfo.ComputerName = resolvedComputerName;
                remoteRunspace = CreateTemporaryRemoteRunspace(this.Host, connectionInfo);
                WriteErrorCreateRemoteRunspaceFailed(e, resolvedComputerName);
        /// Create runspace when uri parameter specified.
        private RemoteRunspace CreateRunspaceWhenUriParameterSpecified()
                connectionInfo.ConnectionUri = ConnectionUri;
                WriteErrorCreateRemoteRunspaceFailed(e, ConnectionUri);
        /// Get runspace matching condition.
        private RemoteRunspace GetRunspaceMatchingCondition(
            Predicate<PSSession> condition,
            PSRemotingErrorId tooFew,
            PSRemotingErrorId tooMany,
            string tooFewResourceString,
            string tooManyResourceString,
            object errorArgument)
            // Find matches.
            List<PSSession> matches = this.RunspaceRepository.Runspaces.FindAll(condition);
            // Validate.
                WriteInvalidArgumentError(tooFew, tooFewResourceString, errorArgument);
            else if (matches.Count > 1)
                WriteInvalidArgumentError(tooMany, tooManyResourceString, errorArgument);
                remoteRunspace = (RemoteRunspace)matches[0].Runspace;
        /// Get runspace matching runspace id.
        private RemoteRunspace GetRunspaceMatchingRunspaceId(Guid remoteRunspaceId)
            return GetRunspaceMatchingCondition(
                condition: info => info.InstanceId == remoteRunspaceId,
                tooFew: PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedRunspaceId,
                tooMany: PSRemotingErrorId.RemoteRunspaceHasMultipleMatchesForSpecifiedRunspaceId,
                tooFewResourceString: RemotingErrorIdStrings.RemoteRunspaceNotAvailableForSpecifiedRunspaceId,
                tooManyResourceString: RemotingErrorIdStrings.RemoteRunspaceHasMultipleMatchesForSpecifiedRunspaceId,
                errorArgument: remoteRunspaceId);
        /// Get runspace matching session id.
        private RemoteRunspace GetRunspaceMatchingSessionId(int sessionId)
                condition: info => info.Id == sessionId,
                tooFew: PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedSessionId,
                tooMany: PSRemotingErrorId.RemoteRunspaceHasMultipleMatchesForSpecifiedSessionId,
                tooFewResourceString: RemotingErrorIdStrings.RemoteRunspaceNotAvailableForSpecifiedSessionId,
                tooManyResourceString: RemotingErrorIdStrings.RemoteRunspaceHasMultipleMatchesForSpecifiedSessionId,
                errorArgument: sessionId);
        /// Get runspace matching name.
        private RemoteRunspace GetRunspaceMatchingName(string name)
                condition: info => info.Name.Equals(name, StringComparison.OrdinalIgnoreCase),
                tooFew: PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedName,
                tooMany: PSRemotingErrorId.RemoteRunspaceHasMultipleMatchesForSpecifiedName,
                tooFewResourceString: RemotingErrorIdStrings.RemoteRunspaceNotAvailableForSpecifiedName,
                tooManyResourceString: RemotingErrorIdStrings.RemoteRunspaceHasMultipleMatchesForSpecifiedName,
                errorArgument: name);
        private Job FindJobForRunspace(Guid id)
            foreach (var repJob in this.JobRepository.Jobs)
                foreach (Job childJob in repJob.ChildJobs)
                    PSRemotingChildJob remotingChildJob = childJob as PSRemotingChildJob;
                    if (remotingChildJob != null &&
                        remotingChildJob.Runspace != null &&
                        remotingChildJob.JobStateInfo.State == JobState.Running &&
                        remotingChildJob.Runspace.InstanceId.Equals(id))
                        return repJob;
        private bool IsParameterSetForVM()
            return ((ParameterSetName == VMIdParameterSet) ||
                    (ParameterSetName == VMNameParameterSet));
        private bool IsParameterSetForContainer()
            return (ParameterSetName == ContainerIdParameterSet);
        /// Whether the input is a session object or property that corresponds to
        /// VM or container.
        private bool IsParameterSetForVMContainerSession()
                    if (this.Session != null)
                        remoteRunspace = (RemoteRunspace)this.Session.Runspace;
                (remoteRunspace.ConnectionInfo != null))
                if ((remoteRunspace.ConnectionInfo is VMConnectionInfo) ||
                    (remoteRunspace.ConnectionInfo is ContainerConnectionInfo))
        /// Create runspace for VM session.
        private RemoteRunspace GetRunspaceForVMSession()
            if (ParameterSetName == VMIdParameterSet)
                        command, false, PipelineResultTypes.None, null, this.VMId);
                            new ArgumentException(RemotingErrorIdStrings.InvalidVMId),
                            nameof(PSRemotingErrorId.InvalidVMId),
                this.VMName = (string)results[0].Properties["VMName"].Value;
                Dbg.Assert(ParameterSetName == VMNameParameterSet, "Expected ParameterSetName == VMName");
                        command, false, PipelineResultTypes.None, null, this.VMName);
                            new ArgumentException(RemotingErrorIdStrings.InvalidVMNameNoVM),
                            nameof(PSRemotingErrorId.InvalidVMNameNoVM),
                else if (results.Count > 1)
                            new ArgumentException(RemotingErrorIdStrings.InvalidVMNameMultipleVM),
                            nameof(PSRemotingErrorId.InvalidVMNameMultipleVM),
                this.VMId = (Guid)results[0].Properties["VMId"].Value;
            // VM should be in running state.
            if (GetVMStateProperty(results[0]) != VMState.Running)
                                                         this.VMName)),
                connectionInfo = new VMConnectionInfo(this.Credential, this.VMId, this.VMName, this.ConfigurationName);
                remoteRunspace = CreateTemporaryRemoteRunspaceForPowerShellDirect(this.Host, connectionInfo);
            catch (PSRemotingDataStructureException e)
                if ((e.InnerException != null) && (e.InnerException is PSDirectException))
                    errorRecord = new ErrorRecord(e.InnerException,
                    errorRecord = new ErrorRecord(e,
        private static RemoteRunspace CreateTemporaryRemoteRunspaceForPowerShellDirect(PSHost host, RunspaceConnectionInfo connectionInfo)
            RemoteRunspace remoteRunspace = RunspaceFactory.CreateRunspace(connectionInfo, host, typeTable) as RemoteRunspace;
            remoteRunspace.Name = "PowerShellDirectAttach";
                // Make sure we dispose the temporary runspace if something bad happens.
        /// Set prompt for VM/Container sessions.
        private void SetRunspacePrompt(RemoteRunspace remoteRunspace)
            if (IsParameterSetForVM() ||
                IsParameterSetForContainer() ||
                IsParameterSetForVMContainerSession())
                string targetName = string.Empty;
                        targetName = this.VMName;
                        targetName = (this.ContainerId.Length <= 15) ? this.ContainerId
                                                                     : this.ContainerId.Remove(14) + PSObjectHelper.Ellipsis;
                        targetName = (this.Session != null) ? this.Session.ComputerName : string.Empty;
                            targetName = remoteRunspace.ConnectionInfo.ComputerName;
                        Dbg.Assert(false, "Unrecognized parameter set.");
                string promptFn = StringUtil.Format(RemotingErrorIdStrings.EnterVMSessionPrompt,
                    @"PS $($executionContext.SessionState.Path.CurrentLocation)> "" }");
                    ps.Runspace = remoteRunspace;
        /// Create runspace for container session.
        private RemoteRunspace GetRunspaceForContainerSession()
                Dbg.Assert(!string.IsNullOrEmpty(ContainerId), "ContainerId has to be set.");
                connectionInfo = ContainerConnectionInfo.CreateContainerConnectionInfo(ContainerId, RunAsAdministrator.IsPresent, this.ConfigurationName);
        /// Create remote runspace for SSH session.
        private RemoteRunspace GetRunspaceForSSHSession()
            ParseSshHostName(HostName, out string host, out string userName, out int port);
            // Use the class _tempRunspace field while the runspace is being opened so that StopProcessing can be handled at that time.
            // This is only needed for SSH sessions where a Ctrl+C during an SSH password prompt can abort the session before a connection
            // is established.
            _tempRunspace = RunspaceFactory.CreateRunspace(sshConnectionInfo, this.Host, typeTable) as RemoteRunspace;
            _tempRunspace.Open();
            _tempRunspace.ShouldCloseOnPop = true;
            _tempRunspace = null;
        internal static RemotePipeline ConnectRunningPipeline(RemoteRunspace remoteRunspace)
            RemotePipeline cmd = null;
            if (remoteRunspace.RemoteCommand != null)
                // Reconstruct scenario.
                // Newly connected pipeline object is added to the RemoteRunspace running
                // pipeline list.
                cmd = new RemotePipeline(remoteRunspace);
                // Reconnect scenario.
                cmd = remoteRunspace.GetCurrentlyRunningPipeline() as RemotePipeline;
            // Connect the runspace pipeline so that debugging and output data from
            // remote server can continue.
                cmd.PipelineStateInfo.State == PipelineState.Disconnected)
                using (ManualResetEvent connected = new ManualResetEvent(false))
                    cmd.StateChanged += (sender, args) =>
                        if (args.PipelineStateInfo.State != PipelineState.Disconnected)
                                connected.Set();
                    cmd.ConnectAsync();
                    connected.WaitOne();
            return cmd;
        internal static void ContinueCommand(RemoteRunspace remoteRunspace, Pipeline cmd, PSHost host, bool inDebugMode, System.Management.Automation.ExecutionContext context)
            RemotePipeline remotePipeline = cmd as RemotePipeline;
            if (remotePipeline != null)
                        Host = host
                    PSDataCollection<PSObject> input = new PSDataCollection<PSObject>();
                    CommandInfo commandInfo = new CmdletInfo("Out-Default", typeof(OutDefaultCommand), null, null, context);
                    Command outDefaultCommand = new Command(commandInfo);
                    ps.AddCommand(outDefaultCommand);
                    IAsyncResult async = ps.BeginInvoke<PSObject>(input, settings, null, null);
                        // Update client with breakpoint information from pushed runspace.
                        // Information will be passed to the client via the Debugger.BreakpointUpdated event.
                        remoteDebugger.SendBreakpointUpdatedEvents();
                        if (!inDebugMode)
                            // Enter debug mode if remote runspace is in debug stop mode.
                    // Wait for debugged cmd to complete.
                    while (!remotePipeline.Output.EndOfPipeline)
                        remotePipeline.Output.WaitHandle.WaitOne();
                        while (remotePipeline.Output.Count > 0)
                            input.Add(remotePipeline.Output.Read());
                    input.Complete();

    /// This cmdlet disconnects PS sessions (RemoteRunspaces) that are in the Opened state
    /// and returns the PS session objects in the Disconnected state.  While the PS
    /// sessions are in the disconnected state no commands can be invoked on them and
    /// any existing remote running commands will not return any data.
    /// The PS sessions can be reconnected by using the Connect-PSSession cmdlet.
    /// Disconnect a PS session object:
    /// Disconnect a PS session by name:
    /// > Disconnect-PSSession -Name $session.Name
    /// Disconnect a PS session by Id:
    /// > Disconnect-PSSession -Id $session.Id
    /// Disconnect a collection of PS sessions:
    /// > Get-PSSession | Disconnect-PSSession.
    [Cmdlet(VerbsCommunications.Disconnect, "PSSession", SupportsShouldProcess = true, DefaultParameterSetName = DisconnectPSSessionCommand.SessionParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096576", RemotingCapability = RemotingCapability.OwnedByCommand)]
    public class DisconnectPSSessionCommand : PSRunspaceCmdlet, IDisposable
        /// The PSSession object or objects to be disconnected.
                   ParameterSetName = DisconnectPSSessionCommand.SessionParameterSet)]
        /// Idle Timeout session option in seconds.  Used in this cmdlet to set server disconnect idletimeout option.
        [Parameter(ParameterSetName = DisconnectPSSessionCommand.SessionParameterSet)]
        [Parameter(ParameterSetName = PSRunspaceCmdlet.NameParameterSet)]
        [Parameter(ParameterSetName = PSRunspaceCmdlet.IdParameterSet)]
        [Parameter(ParameterSetName = PSRunspaceCmdlet.InstanceIdParameterSet)]
        public int IdleTimeoutSec
            get { return this.PSSessionOption.IdleTimeout.Seconds; }
            set { this.PSSessionOption.IdleTimeout = TimeSpan.FromSeconds(value); }
        /// Output buffering mode session option.  Used in this cmdlet to set server disconnect OutputBufferingMode option.
        public OutputBufferingMode OutputBufferingMode
            get { return this.PSSessionOption.OutputBufferingMode; }
            set { this.PSSessionOption.OutputBufferingMode = value; }
        /// Disconnect-PSSession does not support ComputerName parameter set.
        /// This may change for later versions.
        private PSSessionOption PSSessionOption
                // no need to lock as the cmdlet parameters will not be assigned
                // from multiple threads.
                return _sessionOption ??= new PSSessionOption();
        private PSSessionOption _sessionOption;
        /// Set up the ThrottleManager for runspace disconnect processing.
            _throttleManager.ThrottleComplete += HandleThrottleDisconnectComplete;
        /// Perform runspace disconnect processing on all input.
            Dictionary<Guid, PSSession> psSessions;
            List<IThrottleOperation> disconnectOperations = new List<IThrottleOperation>();
                    if (Session == null || Session.Length == 0)
                    psSessions = new Dictionary<Guid, PSSession>();
                        psSessions.Add(psSession.InstanceId, psSession);
                    psSessions = GetMatchingRunspaces(false, true);
                // Look for local sessions that have the EnableNetworkAccess property set and
                // return a string containing all of the session names.  Emit a warning for
                // these sessions.
                string cnNames = GetLocalhostWithNetworkAccessEnabled(psSessions);
                if (!string.IsNullOrEmpty(cnNames))
                    WriteWarning(
                        StringUtil.Format(RemotingErrorIdStrings.EnableNetworkAccessWarning, cnNames));
                foreach (PSSession psSession in psSessions.Values)
                    if (ShouldProcess(psSession.Name, VerbsCommunications.Disconnect))
                            // Write error record.
                            string msg = StringUtil.Format(RemotingErrorIdStrings.RunspaceCannotBeDisconnectedForVMContainerSession,
                            ErrorRecord errorRecord = new ErrorRecord(reason, "CannotDisconnectVMContainerSession", ErrorCategory.InvalidOperation, psSession);
                        // Can only disconnect an Opened runspace.
                            if (_sessionOption != null)
                                psSession.Runspace.ConnectionInfo.SetSessionOptions(_sessionOption);
                            // Validate the ConnectionInfo IdleTimeout value against the MaxIdleTimeout
                            // value returned by the server and the hard coded minimum allowed value.
                            if (!ValidateIdleTimeout(psSession))
                            DisconnectRunspaceOperation disconnectOperation = new DisconnectRunspaceOperation(psSession, _stream);
                            disconnectOperations.Add(disconnectOperation);
                        else if (psSession.Runspace.RunspaceStateInfo.State != RunspaceState.Disconnected)
                            string msg = StringUtil.Format(RemotingErrorIdStrings.RunspaceCannotBeDisconnected, psSession.Name);
                            ErrorRecord errorRecord = new ErrorRecord(reason, "CannotDisconnectSessionWhenNotOpened", ErrorCategory.InvalidOperation, psSession);
                            // Session is already disconnected.  Write to output.
            if (disconnectOperations.Count > 0)
                // Submit list of disconnect operations.
                _throttleManager.SubmitOperations(disconnectOperations);
            // Wait for all disconnect operations to complete.
            while (!_stream.ObjectReader.EndOfPipeline)
            // Signal the ThrottleManager to stop any further processing
            // of PSSessions.
        private void HandleThrottleDisconnectComplete(object sender, EventArgs eventArgs)
        private bool ValidateIdleTimeout(PSSession session)
            int idleTimeout = session.Runspace.ConnectionInfo.IdleTimeout;
            int maxIdleTimeout = session.Runspace.ConnectionInfo.MaxIdleTimeout;
            const int minIdleTimeout = BaseTransportManager.MinimumIdleTimeout;
            if (idleTimeout != BaseTransportManager.UseServerDefaultIdleTimeout &&
                (idleTimeout > maxIdleTimeout || idleTimeout < minIdleTimeout))
                string msg = StringUtil.Format(RemotingErrorIdStrings.CannotDisconnectSessionWithInvalidIdleTimeout,
                    session.Name, idleTimeout / 1000, maxIdleTimeout / 1000, minIdleTimeout / 1000);
                ErrorRecord errorRecord = new ErrorRecord(new RuntimeException(msg),
                    "CannotDisconnectSessionWithInvalidIdleTimeout", ErrorCategory.InvalidArgument, session);
        private static string GetLocalhostWithNetworkAccessEnabled(Dictionary<Guid, PSSession> psSessions)
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
                WSManConnectionInfo wsManConnectionInfo = psSession.Runspace.ConnectionInfo as WSManConnectionInfo;
                if ((wsManConnectionInfo != null) && (wsManConnectionInfo.IsLocalhostAndNetworkAccess))
                    sb.Append(psSession.Name + ", ");
        #region Private Classes
        /// Throttle class to perform a remoterunspace disconnect operation.
        private sealed class DisconnectRunspaceOperation : IThrottleOperation
            private readonly PSSession _remoteSession;
            internal DisconnectRunspaceOperation(PSSession session, ObjectStream stream)
                _remoteSession = session;
                _remoteSession.Runspace.StateChanged += StateCallBackHandler;
                    _remoteSession.Runspace.DisconnectAsync();
                    WriteDisconnectFailed(e);
                    _remoteSession.Runspace.StateChanged -= StateCallBackHandler;
                // Cannot stop a disconnect attempt.
                if (eArgs.RunspaceStateInfo.State == RunspaceState.Disconnecting)
                if (eArgs.RunspaceStateInfo.State == RunspaceState.Disconnected)
                    // If disconnect succeeded then write the PSSession object.
                    WriteDisconnectedPSSession();
                    // Write error if disconnect did not succeed.
                    WriteDisconnectFailed();
                // Notify throttle manager that the start is complete.
            private void WriteDisconnectedPSSession()
                    Action<Cmdlet> outputWriter = (Cmdlet cmdlet) => cmdlet.WriteObject(_remoteSession);
            private void WriteDisconnectFailed(Exception e = null)
                    if (e != null && !string.IsNullOrWhiteSpace(e.Message))
                        msg = StringUtil.Format(RemotingErrorIdStrings.RunspaceDisconnectFailedWithReason, _remoteSession.InstanceId, e.Message);
                        msg = StringUtil.Format(RemotingErrorIdStrings.RunspaceDisconnectFailed, _remoteSession.InstanceId);
                    ErrorRecord errorRecord = new ErrorRecord(reason, "PSSessionDisconnectFailed", ErrorCategory.InvalidOperation, _remoteSession);
                _throttleManager.ThrottleComplete -= HandleThrottleDisconnectComplete;
        // Output data stream.

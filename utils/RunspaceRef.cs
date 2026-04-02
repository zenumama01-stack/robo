    /// Provides a reference to a runspace that can be used to temporarily
    /// push a remote runspace on top of a local runspace. This is
    /// primary used by Start-PSSession. The purpose of this class is to hide
    /// the CreatePipeline method and force it to be used as defined in this
    internal class RunspaceRef
        /// Runspace ref.
        private readonly ObjectRef<Runspace> _runspaceRef;
        private bool _stopInvoke;
        private readonly object _localSyncObject;
        private static readonly RobustConnectionProgress s_RCProgress = new RobustConnectionProgress();
        /// Constructor for RunspaceRef.
        internal RunspaceRef(Runspace runspace)
            Dbg.Assert(runspace != null, "Expected runspace != null");
            _runspaceRef = new ObjectRef<Runspace>(runspace);
            _stopInvoke = false;
            _localSyncObject = new object();
        /// Revert.
        internal void Revert()
            lock (_localSyncObject)
                _stopInvoke = true;
        internal Runspace Runspace
                return _runspaceRef.Value;
        internal Runspace OldRunspace
            get { return _runspaceRef.OldValue; }
        /// Is runspace overridden.
        internal bool IsRunspaceOverridden
                return _runspaceRef.IsOverridden;
        /// Parse ps command using script block.
        private PSCommand ParsePsCommandUsingScriptBlock(string line, bool? useLocalScope)
                // Extract execution context from local runspace.
                Runspace localRunspace = _runspaceRef.OldValue;
                ExecutionContext context = localRunspace.ExecutionContext;
                // This is trusted input as long as we're in FullLanguage mode
                // and if we are not in a loopback configuration mode, in which case we always force remote script commands
                // to be parsed and evaluated on the remote session (not in the current local session).
                RemoteRunspace remoteRunspace = _runspaceRef.Value as RemoteRunspace;
                bool isConfiguredLoopback = remoteRunspace != null && remoteRunspace.IsConfiguredLoopBack;
                bool inFullLanguage = context.LanguageMode == PSLanguageMode.FullLanguage;
                if (context.LanguageMode == PSLanguageMode.ConstrainedLanguage
                    && SystemPolicy.GetSystemLockdownPolicy() == SystemEnforcementMode.Audit)
                    // In audit mode, report but don't enforce.
                    inFullLanguage = true;
                        title: RemotingErrorIdStrings.WDACGetPowerShellLogTitle,
                        message: RemotingErrorIdStrings.WDACGetPowerShellLogMessage,
                        fqid: "GetPowerShellMayFail",
                bool isTrustedInput = !isConfiguredLoopback && inFullLanguage;
                // Create PowerShell from ScriptBlock.
                ScriptBlock scriptBlock = ScriptBlock.Create(context, line);
                PowerShell powerShell = scriptBlock.GetPowerShell(context, isTrustedInput, useLocalScope, null);
                return powerShell.Commands;
            catch (ScriptBlockToPowerShellNotSupportedException)
            // If parsing failed return null.
        /// Create ps command.
        internal PSCommand CreatePsCommand(string line, bool isScript, bool? useNewScope)
            // Fall-back to traditional approach if runspace is not pushed.
            if (!this.IsRunspaceOverridden)
                return CreatePsCommandNotOverridden(line, isScript, useNewScope);
            // Try to parse commands as script-block.
            PSCommand psCommand = ParsePsCommandUsingScriptBlock(line, useNewScope);
            // If that didn't work fall back to traditional approach.
            // Otherwise return the psCommandCollection we got.
            return psCommand;
        /// Creates the PSCommand when the runspace is not overridden.
        private static PSCommand CreatePsCommandNotOverridden(string line, bool isScript, bool? useNewScope)
            if (isScript)
                if (useNewScope.HasValue)
                    command.AddScript(line, useNewScope.Value);
                    command.AddScript(line);
                    command.AddCommand(line, useNewScope.Value);
                    command.AddCommand(line);
        /// Create pipeline.
        internal Pipeline CreatePipeline(string line, bool addToHistory, bool useNestedPipelines)
            // This method allows input commands to work against no-language runspaces. If a runspace
            // is pushed, it tries to parse the line using a ScriptBlock object. If a runspace is not
            // pushed, or if the parsing fails, in these cases it reverts to calling CreatePipeline
            // using the unparsed line.
            Pipeline pipeline = null;
            // In Start-PSSession scenario try to create a pipeline by parsing the line as a script block.
            if (this.IsRunspaceOverridden)
                // Win8: exit should work to escape from the restrictive session
                if ((_runspaceRef.Value is RemoteRunspace) &&
                    (!string.IsNullOrEmpty(line) && string.Equals(line.Trim(), "exit", StringComparison.OrdinalIgnoreCase)))
                    line = "Exit-PSSession";
                PSCommand psCommand = ParsePsCommandUsingScriptBlock(line, null);
                if (psCommand != null)
                    pipeline = useNestedPipelines ?
                        _runspaceRef.Value.CreateNestedPipeline(psCommand.Commands[0].CommandText, addToHistory) :
                        _runspaceRef.Value.CreatePipeline(psCommand.Commands[0].CommandText, addToHistory);
                    pipeline.Commands.Clear();
                    foreach (Command command in psCommand.Commands)
                        pipeline.Commands.Add(command);
            // If that didn't work out fall-back to the traditional approach.
            pipeline ??= useNestedPipelines ?
                _runspaceRef.Value.CreateNestedPipeline(line, addToHistory) :
                _runspaceRef.Value.CreatePipeline(line, addToHistory);
            // Add robust connection callback if this is a pushed runspace.
            if (this.IsRunspaceOverridden && remotePipeline != null)
                PowerShell shell = remotePipeline.PowerShell;
                if (shell.RemotePowerShell != null)
                    shell.RemotePowerShell.RCConnectionNotification += HandleRCConnectionNotification;
                // Add callback to write robust connection errors from stream.
                shell.ErrorBuffer.DataAdded += (sender, eventArgs) =>
                    PSDataCollection<ErrorRecord> erBuffer = sender as PSDataCollection<ErrorRecord>;
                    if (remoteRunspace != null && erBuffer != null &&
                        remoteRunspace.RunspacePool.RemoteRunspacePoolInternal.Host != null)
                        Collection<ErrorRecord> erRecords = erBuffer.ReadAll();
                        foreach (var er in erRecords)
                            remoteRunspace.RunspacePool.RemoteRunspacePoolInternal.Host.UI.WriteErrorLine(er.ToString());
            pipeline.SetHistoryString(line);
            return pipeline;
            return _runspaceRef.Value.CreatePipeline();
        /// Create nested pipeline.
        internal Pipeline CreateNestedPipeline()
            return _runspaceRef.Value.CreateNestedPipeline();
        /// Override.
        internal void Override(RemoteRunspace remoteRunspace)
            bool isRunspacePushed = false;
            Override(remoteRunspace, null, out isRunspacePushed);
        /// Override inside a safe lock.
        /// <param name="remoteRunspace">Runspace to override.</param>
        /// <param name="syncObject">Object to use in synchronization.</param>
        /// <param name="isRunspacePushed">Set is runspace pushed.</param>
        internal void Override(RemoteRunspace remoteRunspace, object syncObject, out bool isRunspacePushed)
                if (syncObject != null)
                        _runspaceRef.Override(remoteRunspace);
                        isRunspacePushed = true;
                if ((remoteRunspace.GetCurrentlyRunningPipeline() != null))
                    // Don't execute command if pushed runspace is already running one.
                    powerShell.AddParameter("Name", new string[] { "Out-Default", "Exit-PSSession" });
                    powerShell.Runspace = _runspaceRef.Value;
                    bool isReleaseCandidateBackcompatibilityMode = _runspaceRef.Value.GetRemoteProtocolVersion() == RemotingConstants.ProtocolVersion_2_0;
                    int expectedNumberOfResults = isReleaseCandidateBackcompatibilityMode ? 2 : 3;
                    powerShell.RemotePowerShell.HostCallReceived += HandleHostCall;
                    IAsyncResult asyncResult = powerShell.BeginInvoke();
                    PSDataCollection<PSObject> results = new PSDataCollection<PSObject>();
                    while (!_stopInvoke)
                        asyncResult.AsyncWaitHandle.WaitOne(1000);
                        if (asyncResult.IsCompleted)
                            results = powerShell.EndInvoke(asyncResult);
                    if (powerShell.Streams.Error.Count > 0 || results.Count < expectedNumberOfResults)
                        throw RemoteHostExceptions.NewRemoteRunspaceDoesNotSupportPushRunspaceException();
                isRunspacePushed = false;
        private void HandleHostCall(object sender, RemoteDataEventArgs<RemoteHostCall> eventArgs)
            ClientRemotePowerShell.ExitHandler(sender, eventArgs);
        #region Robust Connection Support
        private void HandleRCConnectionNotification(object sender, PSConnectionRetryStatusEventArgs e)
                case PSConnectionRetryStatus.NetworkFailureDetected:
                    StartProgressBar(sender.GetHashCode(), e.ComputerName, (e.MaxRetryConnectionTime / 1000));
                case PSConnectionRetryStatus.AutoDisconnectStarting:
                case PSConnectionRetryStatus.ConnectionRetrySucceeded:
                    StopProgressBar(sender.GetHashCode());
                case PSConnectionRetryStatus.AutoDisconnectSucceeded:
                case PSConnectionRetryStatus.InternalErrorAbort:
                    WriteRCFailedError();
        private void WriteRCFailedError()
            if (remoteRunspace != null &&
                remoteRunspace.RunspacePool.RemoteRunspacePoolInternal.Host.UI.WriteErrorLine(
                    StringUtil.Format(RemotingErrorIdStrings.RCAutoDisconnectingError,
                    remoteRunspace.ConnectionInfo.ComputerName));
        private void StartProgressBar(
            long sourceId,
            int totalSeconds)
                s_RCProgress.StartProgress(
                    sourceId,
                    totalSeconds,
                    remoteRunspace.RunspacePool.RemoteRunspacePoolInternal.Host);
        private static void StopProgressBar(
            long sourceId)
            s_RCProgress.StopProgress(sourceId);

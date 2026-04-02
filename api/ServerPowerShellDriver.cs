    /// This class wraps a PowerShell object. It is used to function
    /// as a server side powershell.
    internal class ServerPowerShellDriver
        private bool _extraPowerShellAlreadyScheduled;
        // extra PowerShell at the server to be run after localPowerShell
        private readonly PowerShell _extraPowerShell;
        // output buffer for the local PowerShell that is associated with this powershell driver
        // associated with this powershell data structure handler object to handle all communications with the client
        private readonly PSDataCollection<PSObject> _localPowerShellOutput;
        // if the remaining data has been sent to the client before sending state information
        private readonly bool[] _datasent = new bool[2];
        // sync object for synchronizing sending data to client
        // there is no input when this driver was created
        private readonly bool _noInput;
        // the server remote host instance
        // associated with this powershell
        private readonly ServerRemoteHost _remoteHost;
        // apartment state for this powershell
        private readonly ApartmentState apartmentState;
        // Handles nested invocation of PS drivers.
        private readonly IRSPDriverInvoke _psDriverInvoker;
        /// Default constructor for creating ServerPowerShellDrivers.
        /// <param name="powershell">Decoded powershell object.</param>
        /// <param name="extraPowerShell">Extra pipeline to be run after <paramref name="powershell"/> completes.</param>
        /// <param name="noInput">Whether there is input for this powershell.</param>
        /// <param name="clientPowerShellId">The client powershell id.</param>
        /// <param name="clientRunspacePoolId">The client runspacepool id.</param>
        /// <param name="runspacePoolDriver">runspace pool driver
        /// which is creating this powershell driver</param>
        /// <param name="apartmentState">Apartment state for this powershell.</param>
        /// <param name="hostInfo">host info using which the host for
        /// this powershell will be constructed</param>
        /// <param name="streamOptions">Serialization options for the streams in this powershell.</param>
        /// true if the command is to be added to history list of the runspace. false, otherwise.
        /// If not null, this Runspace will be used to invoke Powershell.
        /// If null, the RunspacePool pointed by <paramref name="runspacePoolDriver"/> will be used.
        internal ServerPowerShellDriver(PowerShell powershell, PowerShell extraPowerShell, bool noInput, Guid clientPowerShellId,
            Guid clientRunspacePoolId, ServerRunspacePoolDriver runspacePoolDriver,
            ApartmentState apartmentState, HostInfo hostInfo, RemoteStreamOptions streamOptions,
            bool addToHistory, Runspace rsToUse)
            : this(powershell, extraPowerShell, noInput, clientPowerShellId, clientRunspacePoolId, runspacePoolDriver,
                   apartmentState, hostInfo, streamOptions, addToHistory, rsToUse, null)
        /// If not null, this is used as another source of output sent to the client.
            bool addToHistory, Runspace rsToUse, PSDataCollection<PSObject> output)
            InstanceId = clientPowerShellId;
            RunspacePoolId = clientRunspacePoolId;
            RemoteStreamOptions = streamOptions;
            this.apartmentState = apartmentState;
            LocalPowerShell = powershell;
            _extraPowerShell = extraPowerShell;
            _localPowerShellOutput = new PSDataCollection<PSObject>();
            _noInput = noInput;
            _psDriverInvoker = runspacePoolDriver;
            DataStructureHandler = runspacePoolDriver.DataStructureHandler.CreatePowerShellDataStructureHandler(clientPowerShellId, clientRunspacePoolId, RemoteStreamOptions, LocalPowerShell);
            _remoteHost = DataStructureHandler.GetHostAssociatedWithPowerShell(hostInfo, runspacePoolDriver.ServerRemoteHost);
            if (!noInput)
                InputCollection = new PSDataCollection<object>();
                InputCollection.ReleaseOnEnumeration = true;
                InputCollection.IdleEvent += HandleIdleEvent;
            RegisterPipelineOutputEventHandlers(_localPowerShellOutput);
            if (LocalPowerShell != null)
                RegisterPowerShellEventHandlers(LocalPowerShell);
                _datasent[0] = false;
            if (extraPowerShell != null)
                RegisterPowerShellEventHandlers(extraPowerShell);
                _datasent[1] = false;
            RegisterDataStructureHandlerEventHandlers(DataStructureHandler);
            // set the runspace pool and invoke this powershell
                LocalPowerShell.Runspace = rsToUse;
                    extraPowerShell.Runspace = rsToUse;
                LocalPowerShell.RunspacePool = runspacePoolDriver.RunspacePool;
                    extraPowerShell.RunspacePool = runspacePoolDriver.RunspacePool;
                output.DataAdded += (sender, args) =>
                        if (_localPowerShellOutput.IsOpen)
                            var items = output.ReadAll();
                            foreach (var item in items)
                                _localPowerShellOutput.Add(item);
        /// Input collection sync object.
        internal PSDataCollection<object> InputCollection { get; }
        /// Local PowerShell instance.
        internal PowerShell LocalPowerShell { get; }
        /// Instance id by which this powershell driver is
        /// identified. This is the same as the id of the
        /// powershell on the client side.
        internal Guid InstanceId { get; }
        /// Serialization options for the streams in this powershell.
        internal RemoteStreamOptions RemoteStreamOptions { get; }
        /// Id of the runspace pool driver which created
        /// this object. This is the same as the id of
        /// the runspace pool at the client side which
        /// is associated with the powershell on the
        /// client side.
        /// ServerPowerShellDataStructureHandler associated with this
        /// powershell driver.
        internal ServerPowerShellDataStructureHandler DataStructureHandler { get; }
        private PSInvocationSettings PrepInvoke(bool startMainPowerShell)
            if (startMainPowerShell)
                // prepare transport manager for sending and receiving data.
                DataStructureHandler.Prepare();
            settings.ApartmentState = apartmentState;
            settings.Host = _remoteHost;
            // Flow the impersonation policy to pipeline execution thread
            // only if the current thread is impersonated (Delegation is
            // also a kind of impersonation).
                WindowsIdentity currentThreadIdentity = WindowsIdentity.GetCurrent();
                switch (currentThreadIdentity.ImpersonationLevel)
                    case TokenImpersonationLevel.Impersonation:
                    case TokenImpersonationLevel.Delegation:
                        settings.FlowImpersonationPolicy = true;
                        settings.FlowImpersonationPolicy = false;
        private IAsyncResult Start(bool startMainPowerShell)
            PSInvocationSettings settings = PrepInvoke(startMainPowerShell);
                return LocalPowerShell.BeginInvoke<object, PSObject>(InputCollection, _localPowerShellOutput, settings, null, null);
                return _extraPowerShell.BeginInvoke<object, PSObject>(InputCollection, _localPowerShellOutput, settings, null, null);
        /// Invokes the powershell asynchronously.
        internal IAsyncResult Start()
            return Start(true);
        /// Runs no command but allows the PowerShell object on the client
        /// to complete.  This is used for running "virtual" remote debug
        /// commands that sets debugger state but doesn't run any command
        /// on the server runspace.
        /// <param name="output">The output from preprocessing that we want to send to the client.</param>
        internal void RunNoOpCommand(IReadOnlyCollection<object> output)
                            LocalPowerShell.SetStateChanged(
                                new PSInvocationStateInfo(
                                    PSInvocationState.Running, null));
                                    _localPowerShellOutput.Add(PSObject.AsPSObject(item));
                                    PSInvocationState.Completed, null));
        /// Invokes the Main PowerShell object synchronously.
        internal void InvokeMain()
            PSInvocationSettings settings = PrepInvoke(true);
                LocalPowerShell.InvokeWithDebugger(InputCollection, _localPowerShellOutput, settings, true);
                // Since this is being invoked asynchronously on a single pipeline thread
                // any invoke failures (such as possible debugger failures) need to be
                // passed back to client or the original client invoke request will not respond.
                string failedCommand = LocalPowerShell.Commands.Commands[0].CommandText;
                LocalPowerShell.Commands.Clear();
                string msg = StringUtil.Format(
                    RemotingErrorIdStrings.ServerSideNestedCommandInvokeFailed,
                    failedCommand ?? string.Empty,
                    ex.Message ?? string.Empty);
                LocalPowerShell.AddCommand("Write-Error").AddArgument(msg);
                LocalPowerShell.Invoke();
        private void RegisterPowerShellEventHandlers(PowerShell powerShell)
            powerShell.InvocationStateChanged += HandlePowerShellInvocationStateChanged;
            powerShell.Streams.Error.DataAdded += HandleErrorDataAdded;
            powerShell.Streams.Debug.DataAdded += HandleDebugAdded;
            powerShell.Streams.Verbose.DataAdded += HandleVerboseAdded;
            powerShell.Streams.Warning.DataAdded += HandleWarningAdded;
            powerShell.Streams.Progress.DataAdded += HandleProgressAdded;
            powerShell.Streams.Information.DataAdded += HandleInformationAdded;
        private void UnregisterPowerShellEventHandlers(PowerShell powerShell)
            powerShell.InvocationStateChanged -= HandlePowerShellInvocationStateChanged;
            powerShell.Streams.Error.DataAdded -= HandleErrorDataAdded;
            powerShell.Streams.Debug.DataAdded -= HandleDebugAdded;
            powerShell.Streams.Verbose.DataAdded -= HandleVerboseAdded;
            powerShell.Streams.Warning.DataAdded -= HandleWarningAdded;
            powerShell.Streams.Progress.DataAdded -= HandleProgressAdded;
            powerShell.Streams.Information.DataAdded -= HandleInformationAdded;
        private void RegisterDataStructureHandlerEventHandlers(ServerPowerShellDataStructureHandler dsHandler)
            dsHandler.InputEndReceived += HandleInputEndReceived;
            dsHandler.InputReceived += HandleInputReceived;
            dsHandler.StopPowerShellReceived += HandleStopReceived;
            dsHandler.HostResponseReceived += HandleHostResponseReceived;
            dsHandler.OnSessionConnected += HandleSessionConnected;
        private void UnregisterDataStructureHandlerEventHandlers(ServerPowerShellDataStructureHandler dsHandler)
            dsHandler.InputEndReceived -= HandleInputEndReceived;
            dsHandler.InputReceived -= HandleInputReceived;
            dsHandler.StopPowerShellReceived -= HandleStopReceived;
            dsHandler.HostResponseReceived -= HandleHostResponseReceived;
            dsHandler.OnSessionConnected -= HandleSessionConnected;
        private void RegisterPipelineOutputEventHandlers(PSDataCollection<PSObject> pipelineOutput)
            pipelineOutput.DataAdded += HandleOutputDataAdded;
        private void UnregisterPipelineOutputEventHandlers(PSDataCollection<PSObject> pipelineOutput)
            pipelineOutput.DataAdded -= HandleOutputDataAdded;
        /// Handle state changed information from PowerShell
        /// and send it to the client.
        /// <param name="eventArgs">arguments describing state changed
        /// information for this powershell</param>
        private void HandlePowerShellInvocationStateChanged(object sender,
            PSInvocationStateChangedEventArgs eventArgs)
            PSInvocationState state = eventArgs.InvocationStateInfo.State;
                        if (LocalPowerShell.RunningExtraCommands)
                            // If completed successfully then allow extra commands to run.
                            if (state == PSInvocationState.Completed)
                            // For failed or stopped state, extra commands cannot run and
                            // we allow this command invocation to finish.
                        // send the remaining data before sending in
                        // state information. This is required because
                        // the client side runspace pool will remove
                        // the association with the client side powershell
                        // once the powershell reaches a terminal state.
                        // If the association is removed, then any data
                        // sent to the powershell will be discarded by
                        // the runspace pool data structure handler on the client side
                        SendRemainingData();
                        if (state == PSInvocationState.Completed &&
                            (_extraPowerShell != null) &&
                            !_extraPowerShellAlreadyScheduled)
                            _extraPowerShellAlreadyScheduled = true;
                            Start(false);
                            DataStructureHandler.RaiseRemoveAssociationEvent();
                            // send the state change notification to the client
                            DataStructureHandler.SendStateChangedInformationToClient(
                                eventArgs.InvocationStateInfo);
                            UnregisterPowerShellEventHandlers(LocalPowerShell);
                            if (_extraPowerShell != null)
                                UnregisterPowerShellEventHandlers(_extraPowerShell);
                            UnregisterDataStructureHandlerEventHandlers(DataStructureHandler);
                            UnregisterPipelineOutputEventHandlers(_localPowerShellOutput);
                            // BUGBUG: currently the local powershell cannot
                            // be disposed as raising the events is
                            // not done towards the end. Need to fix
                            // powershell in order to get this enabled
                            // localPowerShell.Dispose();
                        // abort all pending host calls
                        _remoteHost.ServerMethodExecutor.AbortAllCalls();
        /// Handles DataAdded event from the Output of the powershell.
        /// <param name="e">Arguments describing this event.</param>
        private void HandleOutputDataAdded(object sender, DataAddedEventArgs e)
            int index = e.Index;
                int indexIntoDataSent = (!_extraPowerShellAlreadyScheduled) ? 0 : 1;
                if (!_datasent[indexIntoDataSent])
                    PSObject data = _localPowerShellOutput[index];
                    // once send the output is removed so that the same
                    // is not sent again by SendRemainingData() method
                    _localPowerShellOutput.RemoveAt(index);
                    // send the output data to the client
                    DataStructureHandler.SendOutputDataToClient(data);
        /// Handles DataAdded event from Error of the PowerShell.
        private void HandleErrorDataAdded(object sender, DataAddedEventArgs e)
                if ((indexIntoDataSent == 0) && (!_datasent[indexIntoDataSent]))
                    ErrorRecord errorRecord = LocalPowerShell.Streams.Error[index];
                    // once send the error record is removed so that the same
                    LocalPowerShell.Streams.Error.RemoveAt(index);
                    // send the error record to the client
                    DataStructureHandler.SendErrorRecordToClient(errorRecord);
        /// Handles DataAdded event from Progress of PowerShell.
                    ProgressRecord data = LocalPowerShell.Streams.Progress[index];
                    // once the debug message is sent, it is removed so that
                    // the same is not sent again by SendRemainingData() method
                    LocalPowerShell.Streams.Progress.RemoveAt(index);
                    DataStructureHandler.SendProgressRecordToClient(data);
        /// Handles DataAdded event from Warning of PowerShell.
                    WarningRecord data = LocalPowerShell.Streams.Warning[index];
                    LocalPowerShell.Streams.Warning.RemoveAt(index);
                    DataStructureHandler.SendWarningRecordToClient(data);
        /// Handles DataAdded from Verbose of PowerShell.
        /// <param name="eventArgs">Sender of this information.</param>
                    VerboseRecord data = LocalPowerShell.Streams.Verbose[index];
                    LocalPowerShell.Streams.Verbose.RemoveAt(index);
                    DataStructureHandler.SendVerboseRecordToClient(data);
        /// Handles DataAdded from Debug of PowerShell.
                    DebugRecord data = LocalPowerShell.Streams.Debug[index];
                    LocalPowerShell.Streams.Debug.RemoveAt(index);
                    DataStructureHandler.SendDebugRecordToClient(data);
        /// Handles DataAdded from Information of PowerShell.
                    InformationRecord data = LocalPowerShell.Streams.Information[index];
                    // once the Information message is sent, it is removed so that
                    LocalPowerShell.Streams.Information.RemoveAt(index);
                    DataStructureHandler.SendInformationRecordToClient(data);
        /// Send the remaining output and error information to
        /// client.
        /// <remarks>This method should be called before
        /// sending the state information. The client will
        /// remove the association between a powershell and
        /// runspace pool if it receives any of the terminal
        /// states. Hence all the remaining data should be
        /// sent before this happens. Else the data will be
        /// discarded</remarks>
        private void SendRemainingData()
                _datasent[indexIntoDataSent] = true;
                // BUGBUG: change this code to use enumerator
                // blocked on bug #108824, to be fixed by Kriscv
                for (int i = 0; i < _localPowerShellOutput.Count; i++)
                    PSObject data = _localPowerShellOutput[i];
                _localPowerShellOutput.Clear();
                // foreach (ErrorRecord errorRecord in localPowerShell.Error)
                for (int i = 0; i < LocalPowerShell.Streams.Error.Count; i++)
                    ErrorRecord errorRecord = LocalPowerShell.Streams.Error[i];
                LocalPowerShell.Streams.Error.Clear();
                    // reset to original state so other pipelines can stream.
        /// Stop the local powershell.
        /// <param name="eventArgs">Unused.</param>
        private void HandleStopReceived(object sender, EventArgs eventArgs)
                if (LocalPowerShell.InvocationStateInfo.State == PSInvocationState.Stopped ||
                    LocalPowerShell.InvocationStateInfo.State == PSInvocationState.Completed ||
                    LocalPowerShell.InvocationStateInfo.State == PSInvocationState.Failed ||
                    LocalPowerShell.InvocationStateInfo.State == PSInvocationState.Stopping)
                    // Ensure that the local PowerShell command is not stopped in debug mode.
                    bool handledByDebugger = false;
                    if (!LocalPowerShell.IsNested &&
                        _psDriverInvoker != null)
                        handledByDebugger = _psDriverInvoker.HandleStopSignal();
                    if (!handledByDebugger)
                        LocalPowerShell.Stop();
                    if (_extraPowerShell.InvocationStateInfo.State == PSInvocationState.Stopped ||
                        _extraPowerShell.InvocationStateInfo.State == PSInvocationState.Completed ||
                        _extraPowerShell.InvocationStateInfo.State == PSInvocationState.Failed ||
                        _extraPowerShell.InvocationStateInfo.State == PSInvocationState.Stopping)
                        _extraPowerShell.Stop();
        /// Add input to the local powershell's input collection.
        private void HandleInputReceived(object sender, RemoteDataEventArgs<object> eventArgs)
            // This can be called in pushed runspace scenarios for error reporting (pipeline stopped).
            // Ignore for noInput.
            if (!_noInput && (InputCollection != null))
                InputCollection.Add(eventArgs.Data);
        /// Close the input collection of the local powershell.
        private void HandleInputEndReceived(object sender, EventArgs eventArgs)
                InputCollection.Complete();
        private void HandleSessionConnected(object sender, EventArgs eventArgs)
            // Close input if its active. no need to synchronize as input stream would have already been processed
            // when connect call came into PS plugin
            // TODO: Post an ETW event
            InputCollection?.Complete();
        /// Handle a host message response received.
        private void HandleHostResponseReceived(object sender, RemoteDataEventArgs<RemoteHostResponse> eventArgs)
            _remoteHost.ServerMethodExecutor.HandleRemoteHostResponseFromClient(eventArgs.Data);
        /// Handles the PSDataCollection idle event.
        private void HandleIdleEvent(object sender, EventArgs args)
            Runspace rs = DataStructureHandler.RunspaceUsedToInvokePowerShell;
                PSLocalEventManager events = (object)rs.Events as PSLocalEventManager;
                if (events != null)
                    foreach (PSEventSubscriber subscriber in events.Subscribers)
                        // Use the synchronous version
                        events.DrainPendingActions(subscriber);

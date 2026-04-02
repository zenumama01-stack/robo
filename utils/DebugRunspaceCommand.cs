    /// This cmdlet takes a Runspace object and checks to see if it is debuggable (i.e, if
    /// it is running a script or is currently stopped in the debugger.
    /// If it is debuggable then it breaks into the Runspace debugger in step mode.
    [SuppressMessage("Microsoft.PowerShell", "PS1012:CallShouldProcessOnlyIfDeclaringSupport")]
    [Cmdlet(VerbsDiagnostic.Debug, "Runspace", SupportsShouldProcess = true, DefaultParameterSetName = DebugRunspaceCommand.RunspaceParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2096917")]
    public sealed class DebugRunspaceCommand : PSCmdlet
        #region Strings
        private const string RunspaceParameterSet = "RunspaceParameterSet";
        private const string NameParameterSet = "NameParameterSet";
        private const string IdParameterSet = "IdParameterSet";
        private const string InstanceIdParameterSet = "InstanceIdParameterSet";
        #region Private members
        private Runspace _runspace;
        private System.Management.Automation.Debugger _debugger;
        private PSDataCollection<PSStreamObject> _debugBlockingCollection;
        private PSDataCollection<PSStreamObject> _debugAccumulateCollection;
        private Pipeline _runningPipeline;
        private System.Management.Automation.PowerShell _runningPowerShell;
        // Debugging to persist until Ctrl+C or Debugger 'Exit' stops cmdlet.
        private bool _debugging;
        private readonly ManualResetEventSlim _newRunningScriptEvent = new(true);
        private RunspaceAvailability _previousRunspaceAvailability = RunspaceAvailability.None;
        /// The Runspace to be debugged.
                   ParameterSetName = DebugRunspaceCommand.RunspaceParameterSet)]
        public Runspace Runspace
        /// The name of a Runspace to be debugged.
                   ParameterSetName = DebugRunspaceCommand.NameParameterSet)]
        /// The Id of a Runspace to be debugged.
                   ParameterSetName = DebugRunspaceCommand.IdParameterSet)]
        /// The InstanceId of a Runspace to be debugged.
                   ParameterSetName = DebugRunspaceCommand.InstanceIdParameterSet)]
        public Guid InstanceId
        /// Gets or sets a flag that tells PowerShell to automatically perform a BreakAll when the debugger is attached to the remote target.
        public SwitchParameter BreakAll { get; set; }
        /// End processing.  Do work.
            if (ParameterSetName == DebugRunspaceCommand.RunspaceParameterSet)
                _runspace = Runspace;
                IReadOnlyList<Runspace> runspaces = null;
                    case DebugRunspaceCommand.NameParameterSet:
                        runspaces = GetRunspaceUtils.GetRunspacesByName(new string[] { Name });
                    case DebugRunspaceCommand.IdParameterSet:
                        runspaces = GetRunspaceUtils.GetRunspacesById(new int[] { Id });
                    case DebugRunspaceCommand.InstanceIdParameterSet:
                        runspaces = GetRunspaceUtils.GetRunspacesByInstanceId(new Guid[] { InstanceId });
                if (runspaces.Count > 1)
                            new PSArgumentException(Debugger.RunspaceDebuggingTooManyRunspacesFound),
                            "DebugRunspaceTooManyRunspaceFound",
                            this)
                if (runspaces.Count == 1)
                    _runspace = runspaces[0];
            if (_runspace == null)
                        new PSArgumentNullException(Debugger.RunspaceDebuggingNoRunspaceFound),
                        "DebugRunspaceNoRunspaceFound",
            Runspace defaultRunspace = LocalRunspace.DefaultRunspace;
            if (defaultRunspace == null || defaultRunspace.Debugger == null)
                        new PSInvalidOperationException(Debugger.RunspaceDebuggingNoHostRunspaceOrDebugger),
                        "DebugRunspaceNoHostDebugger",
            if (_runspace == defaultRunspace)
                        new PSInvalidOperationException(Debugger.RunspaceDebuggingCannotDebugDefaultRunspace),
                        "DebugRunspaceCannotDebugHostRunspace",
            if (this.Host == null || this.Host.UI == null)
                        new PSInvalidOperationException(Debugger.RunspaceDebuggingNoHost),
                        "DebugRunspaceNoHostAvailable",
            if (!ShouldProcess(_runspace.Name, VerbsDiagnostic.Debug))
            _debugger = defaultRunspace.Debugger;
                PrepareRunspace(_runspace);
                // Blocking call.  Send runspace/command output to host UI while debugging and wait for runspace/command completion.
                WaitAndReceiveRunspaceOutput();
                RestoreRunspace(_runspace);
        /// Stop processing.
            _debugging = false;
            // Cancel runspace debugging.
            System.Management.Automation.Debugger debugger = _debugger;
            if ((debugger != null) && (_runspace != null))
                debugger.StopDebugRunspace(_runspace);
            // Unblock the data collection.
            PSDataCollection<PSStreamObject> debugCollection = _debugBlockingCollection;
            debugCollection?.Complete();
            // Unblock any new command wait.
            _newRunningScriptEvent.Set();
        private void WaitAndReceiveRunspaceOutput()
            _debugging = true;
                HostWriteLine(string.Format(CultureInfo.InvariantCulture, Debugger.RunspaceDebuggingStarted, _runspace.Name));
                HostWriteLine(Debugger.RunspaceDebuggingEndSession);
                HostWriteLine(string.Empty);
                _runspace.AvailabilityChanged += HandleRunspaceAvailabilityChanged;
                _debugger.NestedDebuggingCancelledEvent += HandleDebuggerNestedDebuggingCancelledEvent;
                // Make sure host debugger has debugging turned on.
                _debugger.SetDebugMode(DebugModes.LocalScript | DebugModes.RemoteScript);
                // Set up host script debugger to debug the runspace.
                _debugger.DebugRunspace(_runspace, breakAll: BreakAll);
                _runspace.IsRemoteDebuggerAttached = true;
                _runspace.Events?.GenerateEvent(
                    PSEngineEvent.OnDebugAttach,
                    sender: null,
                    args: Array.Empty<object>(),
                    extraData: null,
                    processInCurrentThread: true,
                    waitForCompletionInCurrentThread: false);
                while (_debugging)
                    // Wait for running script.
                    _newRunningScriptEvent.Wait();
                    if (!_debugging)
                    AddDataEventHandlers();
                        // Block cmdlet during debugging until either the command finishes
                        // or the user terminates the debugging session.
                        foreach (var streamItem in _debugBlockingCollection)
                            streamItem.WriteStreamObject(this);
                        RemoveDataEventHandlers();
                    if (_debugging &&
                        (!_runspace.InNestedPrompt))
                        HostWriteLine(Debugger.RunspaceDebuggingScriptCompleted);
                    _newRunningScriptEvent.Reset();
                _runspace.AvailabilityChanged -= HandleRunspaceAvailabilityChanged;
                _debugger.NestedDebuggingCancelledEvent -= HandleDebuggerNestedDebuggingCancelledEvent;
                _runspace.IsRemoteDebuggerAttached = false;
                _debugger.StopDebugRunspace(_runspace);
                _newRunningScriptEvent.Dispose();
        private void HostWriteLine(string line)
            if ((this.Host != null) && (this.Host.UI != null))
                    if (this.Host.UI.RawUI != null)
                        this.Host.UI.WriteLine(ConsoleColor.Yellow, this.Host.UI.RawUI.BackgroundColor, line);
                        this.Host.UI.WriteLine(line);
                catch (System.Management.Automation.Host.HostException) { }
        private void AddDataEventHandlers()
            // Create new collection objects.
            _debugBlockingCollection?.Dispose();
            _debugAccumulateCollection?.Dispose();
            _debugBlockingCollection = new PSDataCollection<PSStreamObject>();
            _debugBlockingCollection.BlockingEnumerator = true;
            _debugAccumulateCollection = new PSDataCollection<PSStreamObject>();
            _runningPowerShell = _runspace.GetCurrentBasePowerShell();
            if (_runningPowerShell != null)
                if (_runningPowerShell.OutputBuffer != null)
                    _runningPowerShell.OutputBuffer.DataAdding += HandlePowerShellOutputBufferDataAdding;
                if (_runningPowerShell.ErrorBuffer != null)
                    _runningPowerShell.ErrorBuffer.DataAdding += HandlePowerShellErrorBufferDataAdding;
                _runningPipeline = _runspace.GetCurrentlyRunningPipeline();
                if (_runningPipeline != null)
                    if (_runningPipeline.Output != null)
                        _runningPipeline.Output.DataReady += HandlePipelineOutputDataReady;
                    if (_runningPipeline.Error != null)
                        _runningPipeline.Error.DataReady += HandlePipelineErrorDataReady;
        private void RemoveDataEventHandlers()
                    _runningPowerShell.OutputBuffer.DataAdding -= HandlePowerShellOutputBufferDataAdding;
                    _runningPowerShell.ErrorBuffer.DataAdding -= HandlePowerShellErrorBufferDataAdding;
                _runningPowerShell = null;
            else if (_runningPipeline != null)
                    _runningPipeline.Output.DataReady -= HandlePipelineOutputDataReady;
                    _runningPipeline.Error.DataReady -= HandlePipelineErrorDataReady;
                _runningPipeline = null;
        private void HandleRunspaceAvailabilityChanged(object sender, RunspaceAvailabilityEventArgs e)
            // Ignore nested commands.
            if (sender is LocalRunspace localRunspace)
                var basePowerShell = localRunspace.GetCurrentBasePowerShell();
                if ((basePowerShell != null) && (basePowerShell.IsNested))
            RunspaceAvailability prevAvailability = _previousRunspaceAvailability;
            _previousRunspaceAvailability = e.RunspaceAvailability;
            if ((e.RunspaceAvailability == RunspaceAvailability.Available) || (e.RunspaceAvailability == RunspaceAvailability.None))
                _debugBlockingCollection.Complete();
            else if ((e.RunspaceAvailability == RunspaceAvailability.Busy) &&
                     ((prevAvailability == RunspaceAvailability.Available) || (prevAvailability == RunspaceAvailability.None)))
        private void HandleDebuggerNestedDebuggingCancelledEvent(object sender, EventArgs e)
            StopProcessing();
        private void HandlePipelineOutputDataReady(object sender, EventArgs e)
            if (sender is PipelineReader<PSObject> reader && reader.IsOpen)
                WritePipelineCollection(reader.NonBlockingRead(), PSStreamObjectType.Output);
        private void HandlePipelineErrorDataReady(object sender, EventArgs e)
            if (sender is PipelineReader<object> reader && reader.IsOpen)
                WritePipelineCollection(reader.NonBlockingRead(), PSStreamObjectType.Error);
        private void WritePipelineCollection<T>(Collection<T> collection, PSStreamObjectType psStreamType)
            foreach (var item in collection)
                    AddToDebugBlockingCollection(new PSStreamObject(psStreamType, item));
        private void HandlePowerShellOutputBufferDataAdding(object sender, DataAddingEventArgs e)
            if (e.ItemAdded != null)
                HandlePowerShellPStreamItem(new PSStreamObject(PSStreamObjectType.Output, e.ItemAdded));
        private void HandlePowerShellErrorBufferDataAdding(object sender, DataAddingEventArgs e)
                HandlePowerShellPStreamItem(new PSStreamObject(PSStreamObjectType.Error, e.ItemAdded));
        private void HandlePowerShellPStreamItem(PSStreamObject streamItem)
            if (!_debugger.InBreakpoint)
                // First write any accumulated items.
                foreach (var item in _debugAccumulateCollection.ReadAll())
                    AddToDebugBlockingCollection(item);
                // Handle new item.
                if ((_debugBlockingCollection != null) && (_debugBlockingCollection.IsOpen))
                    AddToDebugBlockingCollection(streamItem);
            else if (_debugAccumulateCollection.IsOpen)
                // Add to accumulator if debugger is stopped in breakpoint.
                _debugAccumulateCollection.Add(streamItem);
        private void AddToDebugBlockingCollection(PSStreamObject streamItem)
            if (!_debugBlockingCollection.IsOpen)
            if (streamItem != null)
                    _debugBlockingCollection.Add(streamItem);
                catch (PSInvalidOperationException) { }
        private void PrepareRunspace(Runspace runspace)
            SetLocalMode(runspace.Debugger, true);
            EnableHostDebugger(runspace, false);
        private void RestoreRunspace(Runspace runspace)
            SetLocalMode(runspace.Debugger, false);
            EnableHostDebugger(runspace, true);
        private void EnableHostDebugger(Runspace runspace, bool enabled)
            // Only enable and disable the host's runspace if we are in process attach mode.
            if (_debugger is ServerRemoteDebugger)
                if ((runspace is LocalRunspace localRunspace) && (localRunspace.ExecutionContext != null) && (localRunspace.ExecutionContext.EngineHostInterface != null))
                        localRunspace.ExecutionContext.EngineHostInterface.DebuggerEnabled = enabled;
                    catch (PSNotImplementedException) { }
        private static void SetLocalMode(System.Management.Automation.Debugger debugger, bool localMode)
            if (debugger is ServerRemoteDebugger remoteDebugger)
                remoteDebugger.LocalDebugMode = localMode;

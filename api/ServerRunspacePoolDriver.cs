    /// Interface exposing driver single thread invoke enter/exit
    /// nested pipeline.
    internal interface IRSPDriverInvoke
        void EnterNestedPipeline();
        void ExitNestedPipeline();
        bool HandleStopSignal();
    /// This class wraps a RunspacePoolInternal object. It is used to function
    /// as a server side runspacepool.
    internal class ServerRunspacePoolDriver : IRSPDriverInvoke
        // local runspace pool at the server
        // Optional initial location of the PowerShell session
        private readonly string _initialLocation;
        // Script to run after a RunspacePool/Runspace is created in this session.
        private readonly ConfigurationDataFromXML _configData;
        // application private data to send back to the client in when we get into "opened" state
        // the client runspacepool's guid that is
        // associated with this runspace pool driver
        // data structure handler object to handle all communications
        // with the client
        // powershell's associated with this runspace pool
        private readonly Dictionary<Guid, ServerPowerShellDriver> _associatedShells
            = new Dictionary<Guid, ServerPowerShellDriver>();
        // remote host associated with this runspacepool
        private readonly ServerDriverRemoteHost _remoteHost;
        // server capability reported to the client during negotiation (not the actual capability)
        private readonly RemoteSessionCapability _serverCapability;
        private Runspace _rsToUseForSteppablePipeline;
        // steppable pipeline event subscribers exist per-session
        private readonly ServerSteppablePipelineSubscriber _eventSubscriber = new ServerSteppablePipelineSubscriber();
        private PSDataCollection<object> _inputCollection; // PowerShell driver input collection
        // Object to invoke nested PowerShell drivers on single pipeline worker thread.
        private readonly PowerShellDriverInvoker _driverNestedInvoker;
        // Remote wrapper for script debugger.
        private ServerRemoteDebugger _serverRemoteDebugger;
        // Version of PowerShell client.
        private readonly Version _clientPSVersion;
        // Optional endpoint configuration name.
        // Used in OutOfProc scenarios that do not support PSSession endpoint configuration.
        // Results in a configured remote runspace pushed onto driver host.
        /// Event that get raised when the RunspacePool is closed.
        internal EventHandler<EventArgs> Closed;
        /// Initializes a new instance of the runspace pool driver.
        /// <param name="clientRunspacePoolId">Client runspace pool id to associate.</param>
        /// <param name="transportManager">Transport manager associated with this
        /// runspace pool driver.</param>
        /// <param name="maxRunspaces">Maximum runspaces to open.</param>
        /// <param name="minRunspaces">Minimum runspaces to open.</param>
        /// <param name="threadOptions">Threading options for the runspaces in the pool.</param>
        /// <param name="apartmentState">Apartment state for the runspaces in the pool.</param>
        /// <param name="hostInfo">Host information about client side host.</param>
        /// <param name="configData">Contains:
        /// 1. Script to run after a RunspacePool/Runspace is created in this session.
        /// For RunspacePool case, every newly created Runspace (in the pool) will run
        /// this script.
        /// 2. ThreadOptions for RunspacePool/Runspace
        /// 3. ThreadApartment for RunspacePool/Runspace
        /// <param name="initialSessionState">Configuration of the runspace.</param>
        /// <param name="isAdministrator">True if the driver is being created by an administrator.</param>
        /// <param name="serverCapability">Server capability reported to the client during negotiation (not the actual capability).</param>
        /// <param name="psClientVersion">Client PowerShell version.</param>
        /// <param name="configurationName">Optional endpoint configuration name to create a pushed configured runspace.</param>
        /// <param name="initialLocation">Optional initial location of the powershell.</param>
        internal ServerRunspacePoolDriver(
            PSThreadOptions threadOptions,
            ApartmentState apartmentState,
            PSPrimitiveDictionary applicationPrivateData,
            ConfigurationDataFromXML configData,
            bool isAdministrator,
            RemoteSessionCapability serverCapability,
            Version psClientVersion,
            string initialLocation)
            Dbg.Assert(configData != null, "ConfigurationData cannot be null");
            _serverCapability = serverCapability;
            _clientPSVersion = psClientVersion;
            _initialLocation = initialLocation;
            // Create a new server host and associate for host call
            // integration
            _remoteHost = new ServerDriverRemoteHost(
                clientRunspacePoolId, Guid.Empty, hostInfo, transportManager, null);
            _configData = configData;
            RunspacePool = RunspaceFactory.CreateRunspacePool(
                  minRunspaces, maxRunspaces, initialSessionState, _remoteHost);
            // Set ThreadOptions for this RunspacePool
            // The default server settings is to make new commands execute in the calling thread...this saves
            // thread switching time and thread pool pressure on the service.
            // Users can override the server settings only if they are administrators
            PSThreadOptions serverThreadOptions = configData.ShellThreadOptions ?? PSThreadOptions.UseCurrentThread;
            if (threadOptions == PSThreadOptions.Default || threadOptions == serverThreadOptions)
                RunspacePool.ThreadOptions = serverThreadOptions;
                if (!isAdministrator)
                    throw new InvalidOperationException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.MustBeAdminToOverrideThreadOptions));
                RunspacePool.ThreadOptions = threadOptions;
            // Set Thread ApartmentState for this RunspacePool
            ApartmentState serverApartmentState = configData.ShellThreadApartmentState ?? Runspace.DefaultApartmentState;
            if (apartmentState == ApartmentState.Unknown || apartmentState == serverApartmentState)
                RunspacePool.ApartmentState = serverApartmentState;
                RunspacePool.ApartmentState = apartmentState;
            // If we have a runspace pool with a single runspace then we can run nested pipelines on
            // on it in a single pipeline invoke thread.
            if (maxRunspaces == 1 &&
                (RunspacePool.ThreadOptions == PSThreadOptions.Default ||
                 RunspacePool.ThreadOptions == PSThreadOptions.UseCurrentThread))
                _driverNestedInvoker = new PowerShellDriverInvoker();
            InstanceId = clientRunspacePoolId;
            DataStructureHandler = new ServerRunspacePoolDataStructureHandler(this, transportManager);
            // handle the StateChanged event of the runspace pool
            // listen for events on the runspace pool
            RunspacePool.RunspaceCreated += HandleRunspaceCreated;
            // register for all the events from the data structure handler
            DataStructureHandler.CreateAndInvokePowerShell += HandleCreateAndInvokePowerShell;
            DataStructureHandler.GetCommandMetadata += HandleGetCommandMetadata;
            DataStructureHandler.HostResponseReceived += HandleHostResponseReceived;
            DataStructureHandler.SetMaxRunspacesReceived += HandleSetMaxRunspacesReceived;
            DataStructureHandler.SetMinRunspacesReceived += HandleSetMinRunspacesReceived;
            DataStructureHandler.GetAvailableRunspacesReceived += HandleGetAvailableRunspacesReceived;
            DataStructureHandler.ResetRunspaceState += HandleResetRunspaceState;
        /// Data structure handler for communicating with client.
        internal ServerRunspacePoolDataStructureHandler DataStructureHandler { get; }
        /// The server host associated with the runspace pool.
        internal ServerRemoteHost ServerRemoteHost
            get { return _remoteHost; }
        /// The client runspacepool id.
        /// The local runspace pool associated with
        /// this driver.
        internal RunspacePool RunspacePool { get; private set; }
        /// Start the RunspacePoolDriver. This will open the
        /// underlying RunspacePool.
            // open the runspace pool
        /// Send application private data to client
        /// will be called during runspace creation
        /// and each time a new client connects to the server session.
        internal void SendApplicationPrivateDataToClient()
            // Include Debug mode information.
            if (_serverRemoteDebugger != null)
                // Current debug mode.
                DebugModes debugMode = _serverRemoteDebugger.DebugMode;
                if (_applicationPrivateData.ContainsKey(RemoteDebugger.DebugModeSetting))
                    _applicationPrivateData[RemoteDebugger.DebugModeSetting] = (int)debugMode;
                    _applicationPrivateData.Add(RemoteDebugger.DebugModeSetting, (int)debugMode);
                // Current debug state.
                bool inBreakpoint = _serverRemoteDebugger.InBreakpoint;
                if (_applicationPrivateData.ContainsKey(RemoteDebugger.DebugStopState))
                    _applicationPrivateData[RemoteDebugger.DebugStopState] = inBreakpoint;
                    _applicationPrivateData.Add(RemoteDebugger.DebugStopState, inBreakpoint);
                // Current debug breakpoint count.
                int breakpointCount = _serverRemoteDebugger.GetBreakpointCount();
                if (_applicationPrivateData.ContainsKey(RemoteDebugger.DebugBreakpointCount))
                    _applicationPrivateData[RemoteDebugger.DebugBreakpointCount] = breakpointCount;
                    _applicationPrivateData.Add(RemoteDebugger.DebugBreakpointCount, breakpointCount);
                // Current debugger BreakAll option setting.
                bool breakAll = _serverRemoteDebugger.IsDebuggerSteppingEnabled;
                if (_applicationPrivateData.ContainsKey(RemoteDebugger.BreakAllSetting))
                    _applicationPrivateData[RemoteDebugger.BreakAllSetting] = breakAll;
                    _applicationPrivateData.Add(RemoteDebugger.BreakAllSetting, breakAll);
                // Current debugger PreserveUnhandledBreakpoints setting.
                UnhandledBreakpointProcessingMode bpMode = _serverRemoteDebugger.UnhandledBreakpointMode;
                if (_applicationPrivateData.ContainsKey(RemoteDebugger.UnhandledBreakpointModeSetting))
                    _applicationPrivateData[RemoteDebugger.UnhandledBreakpointModeSetting] = (int)bpMode;
                    _applicationPrivateData.Add(RemoteDebugger.UnhandledBreakpointModeSetting, (int)bpMode);
            DataStructureHandler.SendApplicationPrivateDataToClient(_applicationPrivateData, _serverCapability);
        /// Dispose the runspace pool driver and release all its resources.
                if ((_remoteHost != null) && (_remoteHost.IsRunspacePushed))
                    Runspace runspaceToDispose = _remoteHost.PushedRunspace;
                    _remoteHost.PopRunspace();
                    runspaceToDispose?.Dispose();
                DisposeRemoteDebugger();
                RunspacePool.Close();
                RunspacePool = null;
                if (_rsToUseForSteppablePipeline != null)
                    _rsToUseForSteppablePipeline.Close();
                    _rsToUseForSteppablePipeline.Dispose();
                    _rsToUseForSteppablePipeline = null;
                Closed.SafeInvoke(this, EventArgs.Empty);
        #region IRSPDriverInvoke interface methods
        /// This method blocks the current thread execution and starts a
        /// new Invoker pump that will handle invoking client side nested commands.
        /// This method returns after ExitNestedPipeline is called.
        public void EnterNestedPipeline()
            if (_driverNestedInvoker == null)
                throw new PSNotSupportedException(RemotingErrorIdStrings.NestedPipelineNotSupported);
            _driverNestedInvoker.PushInvoker();
        /// Removes current nested command Invoker pump and allows parent command
        /// to continue running.
        public void ExitNestedPipeline()
            _driverNestedInvoker.PopInvoker();
        /// If script execution is currently in debugger stopped mode, this will
        /// release the debugger and terminate script execution, or if processing
        /// a debug command will stop the debug command.
        /// This is used to implement the remote stop signal and ensures a command
        /// will stop even when in debug stop mode.
        public bool HandleStopSignal()
                return _serverRemoteDebugger.HandleStopSignal();
        /// RunspaceCreated eventhandler. This is used to set TypeTable for TransportManager.
        /// TransportManager needs TypeTable for Serializing/Deserializing objects.
        private void HandleRunspaceCreatedForTypeTable(object sender, RunspaceCreatedEventArgs args)
            DataStructureHandler.TypeTable = args.Runspace.ExecutionContext.TypeTable;
            _rsToUseForSteppablePipeline = args.Runspace;
            SetupRemoteDebugger(_rsToUseForSteppablePipeline);
            if (!string.IsNullOrEmpty(_configurationName))
                // Client is requesting a configured session.
                // Create a configured remote runspace and push onto host stack.
                if ((_remoteHost != null) && !(_remoteHost.IsRunspacePushed))
                    // Let exceptions propagate.
                    RemoteRunspace remoteRunspace = HostUtilities.CreateConfiguredRunspace(_configurationName, _remoteHost);
                    _remoteHost.PropagatePop = true;
                    _remoteHost.PushRunspace(remoteRunspace);
        private void SetupRemoteDebugger(Runspace runspace)
            CmdletInfo cmdletInfo = runspace.ExecutionContext.SessionState.InvokeCommand.GetCmdlet(ServerRemoteDebugger.SetPSBreakCommandText);
            if (cmdletInfo == null)
                if ((runspace.ExecutionContext.LanguageMode != PSLanguageMode.FullLanguage) &&
                    (!runspace.ExecutionContext.UseFullLanguageModeInDebugger))
                if (cmdletInfo.Visibility != SessionStateEntryVisibility.Public)
            // Remote debugger is created only when client version is PSVersion (4.0)
            // or greater, and remote session supports debugging.
            if ((_driverNestedInvoker != null) &&
                (_clientPSVersion != null && _clientPSVersion.Major >= 4) &&
                (runspace != null && runspace.Debugger != null))
                _serverRemoteDebugger = new ServerRemoteDebugger(this, runspace, runspace.Debugger);
                _remoteHost.ServerDebugger = _serverRemoteDebugger;
        private void DisposeRemoteDebugger() => _serverRemoteDebugger?.Dispose();
        /// Invokes a script.
        /// <param name="cmdToRun"></param>
        private PSDataCollection<PSObject> InvokeScript(Command cmdToRun, RunspaceCreatedEventArgs args)
            Debug.Assert(cmdToRun != null, "cmdToRun shouldn't be null");
            // Don't invoke initialization script as trusted (CommandOrigin == Internal) if the system is in lock down mode.
            cmdToRun.CommandOrigin = (SystemPolicy.GetSystemLockdownPolicy() == SystemEnforcementMode.Enforce) ? CommandOrigin.Runspace : CommandOrigin.Internal;
            cmdToRun.MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
            PowerShell powershell = PowerShell.Create();
            powershell.AddCommand(cmdToRun).AddCommand("out-default");
            return InvokePowerShell(powershell, args);
        /// Invokes a PowerShell instance.
        private PSDataCollection<PSObject> InvokePowerShell(PowerShell powershell, RunspaceCreatedEventArgs args)
            Debug.Assert(powershell != null, "powershell shouldn't be null");
            // run the startup script on the runspace's host
            HostInfo hostInfo = _remoteHost.HostInfo;
            ServerPowerShellDriver driver = new ServerPowerShellDriver(
                powershell,
                this.InstanceId,
                args.Runspace.ApartmentState,
                hostInfo,
                RemoteStreamOptions.AddInvocationInfo,
                args.Runspace);
            IAsyncResult asyncResult = driver.Start();
            // if there was an exception running the script..this may throw..this will
            // result in the runspace getting closed/broken.
            PSDataCollection<PSObject> results = powershell.EndInvoke(asyncResult);
            ArrayList errorList = (ArrayList)powershell.Runspace.GetExecutionContext.DollarErrorVariable;
                string exceptionThrown;
                    exceptionThrown = lastErrorRecord.ToString();
                        exceptionThrown = lastException.Message ?? string.Empty;
                        exceptionThrown = string.Empty;
                throw PSTraceSource.NewInvalidOperationException(RemotingErrorIdStrings.StartupScriptThrewTerminatingError, exceptionThrown);
        /// Raised by RunspacePool whenever a new runspace is created. This is used
        /// by the driver to run startup script as well as set personal folder
        /// as the current working directory.
        /// Runspace that was created by the RunspacePool.
        private void HandleRunspaceCreated(object sender, RunspaceCreatedEventArgs args)
            this.ServerRemoteHost.Runspace = args.Runspace;
            // If the system lockdown policy says "Enforce", do so (unless it's in the
            // more restrictive NoLanguage mode)
            Utils.EnforceSystemLockDownLanguageMode(args.Runspace.ExecutionContext);
            // Set the current location to MyDocuments folder for this runspace.
            // This used to be set to the Personal folder but was changed to MyDocuments folder for
            // compatibility with PowerShell on Nano Server for PowerShell V5.
            // This is needed because in the remoting scenario, Environment.CurrentDirectory
            // always points to System Folder (%windir%\system32) irrespective of the
            // user as %HOMEDRIVE% and %HOMEPATH% are not available for the logon process.
            // Doing this here than AutomationEngine as I dont want to introduce a dependency
            // on Remoting in PowerShell engine
                string personalfolder = Platform.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                args.Runspace.ExecutionContext.EngineSessionState.SetLocation(personalfolder);
                // SetLocation API can call 3rd party code and so there is no telling what exception may be thrown.
                // Setting location is not critical and is expected not to work with some account types, so we want
                // to ignore all but critical errors.
            if (!string.IsNullOrWhiteSpace(_initialLocation))
                var setLocationCommand = new Command("Set-Location");
                setLocationCommand.Parameters.Add(new CommandParameter("LiteralPath", _initialLocation));
                InvokeScript(setLocationCommand, args);
            // Run startup scripts
            InvokeStartupScripts(args);
            // Now that the server side runspace is set up allow the secondary handler to run.
            HandleRunspaceCreatedForTypeTable(sender, args);
        private void InvokeStartupScripts(RunspaceCreatedEventArgs args)
            Command cmdToRun = null;
            if (!string.IsNullOrEmpty(_configData.StartupScript))
                // build the startup script..merge output / error.
                cmdToRun = new Command(_configData.StartupScript, false, false);
            else if (!string.IsNullOrEmpty(_configData.InitializationScriptForOutOfProcessRunspace))
                cmdToRun = new Command(_configData.InitializationScriptForOutOfProcessRunspace, true, false);
            if (cmdToRun != null)
                InvokeScript(cmdToRun, args);
                // if startup script set $PSApplicationPrivateData, then use that value as ApplicationPrivateData
                // instead of using results from PSSessionConfiguration.GetApplicationPrivateData()
                if (RunspacePool.RunspacePoolStateInfo.State == RunspacePoolState.Opening)
                    object privateDataVariable = args.Runspace.SessionStateProxy.PSVariable.GetValue("global:PSApplicationPrivateData");
                    if (privateDataVariable != null)
                        _applicationPrivateData = (PSPrimitiveDictionary)LanguagePrimitives.ConvertTo(
                            privateDataVariable,
                            typeof(PSPrimitiveDictionary),
        /// Handler to the runspace pool state changed events.
        /// <param name="sender">Sender of this events.</param>
        /// <param name="eventArgs">arguments which describe the
        /// RunspacePool's StateChanged event</param>
        private void HandleRunspacePoolStateChanged(object sender,
                            RunspacePoolStateChangedEventArgs eventArgs)
            RunspacePoolState state = eventArgs.RunspacePoolStateInfo.State;
            Exception reason = eventArgs.RunspacePoolStateInfo.Reason;
                        DataStructureHandler.SendStateInfoToClient(new RunspacePoolStateInfo(state, reason));
                        SendApplicationPrivateDataToClient();
        /// Handler to the runspace pool psevents.
                DataStructureHandler.SendPSEventArgsToClient(e);
        /// Handle the invocation of powershell.
        /// <param name="_">Sender of this event, unused.</param>
        private void HandleCreateAndInvokePowerShell(object _, RemoteDataEventArgs<RemoteDataObject<PSObject>> eventArgs)
            RemoteDataObject<PSObject> data = eventArgs.Data;
            // it is sufficient to just construct the powershell
            // driver, the local powershell on server side is
            // invoked from within the driver
            HostInfo hostInfo = RemotingDecoder.GetHostInfo(data.Data);
            ApartmentState apartmentState = RemotingDecoder.GetApartmentState(data.Data);
            RemoteStreamOptions streamOptions = RemotingDecoder.GetRemoteStreamOptions(data.Data);
            PowerShell powershell = RemotingDecoder.GetPowerShell(data.Data);
            bool noInput = RemotingDecoder.GetNoInput(data.Data);
            bool addToHistory = RemotingDecoder.GetAddToHistory(data.Data);
            bool isNested = false;
            // The server would've dropped the protocol version of an older client was connecting
            if (_serverCapability.ProtocolVersion >= RemotingConstants.ProtocolVersion_2_2)
                isNested = RemotingDecoder.GetIsNested(data.Data);
            // Perform pre-processing of command for over the wire debugging commands.
                DebuggerCommandArgument commandArgument;
                bool terminateImmediate = false;
                Collection<object> preProcessOutput = new Collection<object>();
                    var result = PreProcessDebuggerCommand(powershell.Commands, _serverRemoteDebugger, preProcessOutput, out commandArgument);
                        case PreProcessCommandResult.SetDebuggerAction:
                            // Run this directly on the debugger and terminate the remote command.
                            _serverRemoteDebugger.SetDebuggerAction(commandArgument.ResumeAction.Value);
                            terminateImmediate = true;
                        case PreProcessCommandResult.SetDebugMode:
                            // Set debug mode directly and terminate remote command.
                            _serverRemoteDebugger.SetDebugMode(commandArgument.Mode.Value);
                        case PreProcessCommandResult.SetDebuggerStepMode:
                            // Enable debugger and set to step action, then terminate remote command.
                            _serverRemoteDebugger.SetDebuggerStepMode(commandArgument.DebuggerStepEnabled.Value);
                        case PreProcessCommandResult.SetPreserveUnhandledBreakpointMode:
                            _serverRemoteDebugger.UnhandledBreakpointMode = commandArgument.UnhandledBreakpointMode.Value;
                        case PreProcessCommandResult.ValidNotProcessed:
                        case PreProcessCommandResult.BreakpointManagement:
                    preProcessOutput.Add(
                        PSObject.AsPSObject(ex));
                // If we don't want to run or queue a command to run in the server session then
                // terminate the command here by making it a No Op.
                if (terminateImmediate)
                    ServerPowerShellDriver noOpDriver = new ServerPowerShellDriver(
                        noInput,
                        data.PowerShellId,
                        data.RunspacePoolId,
                        apartmentState,
                        streamOptions,
                        addToHistory,
                    noOpDriver.RunNoOpCommand(preProcessOutput);
            if (_remoteHost.IsRunspacePushed)
                // If we have a pushed runspace then execute there.
                // Ensure debugger is enabled to the original mode it was set to.
                _serverRemoteDebugger?.CheckDebuggerState();
                StartPowerShellCommandOnPushedRunspace(
                    addToHistory);
            else if (isNested)
                if (RunspacePool.GetMaxRunspaces() == 1)
                    if (_driverNestedInvoker != null && _driverNestedInvoker.IsActive)
                        if (!_driverNestedInvoker.IsAvailable)
                            // A nested command is already running.
                                StringUtil.Format(RemotingErrorIdStrings.CannotInvokeNestedCommandNestedCommandRunning));
                        // Handle as nested pipeline invocation.
                        powershell.SetIsNested(true);
                        // Always invoke PowerShell commands on pipeline worker thread
                        // for single runspace case, to support nested invocation requests (debugging scenario).
                        ServerPowerShellDriver srdriver = new ServerPowerShellDriver(
                            _rsToUseForSteppablePipeline);
                        _inputCollection = srdriver.InputCollection;
                        _driverNestedInvoker.InvokeDriverAsync(srdriver);
                    else if (_serverRemoteDebugger != null &&
                             _serverRemoteDebugger.InBreakpoint &&
                             _serverRemoteDebugger.IsPushed)
                        _serverRemoteDebugger.StartPowerShellCommand(
                            _remoteHost,
                    else if (powershell.Commands.Commands.Count == 1 &&
                             !powershell.Commands.Commands[0].IsScript &&
                             (powershell.Commands.Commands[0].CommandText.Contains("Get-PSDebuggerStopArgs", StringComparison.OrdinalIgnoreCase) ||
                              powershell.Commands.Commands[0].CommandText.Contains("Set-PSDebuggerAction", StringComparison.OrdinalIgnoreCase)))
                        // We do not want to invoke debugger commands in the steppable pipeline.
                        // Consider adding IsSteppable message to PSRP to handle this.
                        // This will be caught on the client.
                    ServerPowerShellDataStructureHandler psHandler = DataStructureHandler.GetPowerShellDataStructureHandler();
                    if (psHandler != null)
                        // Have steppable invocation request.
                        powershell.SetIsNested(false);
                        // Execute command concurrently
                        ServerSteppablePipelineDriver spDriver = new ServerSteppablePipelineDriver(
                            _rsToUseForSteppablePipeline,
                            _eventSubscriber,
                            _inputCollection);
                        spDriver.Start();
                // Allow command to run as non-nested and non-stepping.
            // Invoke command normally.  Ensure debugger is enabled to the
            // original mode it was set to.
            // Invoke PowerShell on driver runspace pool.
            _inputCollection = driver.InputCollection;
            driver.Start();
        private bool? _initialSessionStateIncludesGetCommandWithListImportedSwitch;
        private readonly object _initialSessionStateIncludesGetCommandWithListImportedSwitchLock = new object();
        private bool DoesInitialSessionStateIncludeGetCommandWithListImportedSwitch()
            if (!_initialSessionStateIncludesGetCommandWithListImportedSwitch.HasValue)
                lock (_initialSessionStateIncludesGetCommandWithListImportedSwitchLock)
                        InitialSessionState iss = this.RunspacePool.InitialSessionState;
                        if (iss != null)
                            IEnumerable<SessionStateCommandEntry> publicGetCommandEntries = iss
                                .Commands["Get-Command"]
                                .Where(static entry => entry.Visibility == SessionStateEntryVisibility.Public);
                            SessionStateFunctionEntry getCommandProxy = publicGetCommandEntries.OfType<SessionStateFunctionEntry>().FirstOrDefault();
                            if (getCommandProxy != null)
                                if (getCommandProxy.ScriptBlock.ParameterMetadata.BindableParameters.ContainsKey("ListImported"))
                                SessionStateCmdletEntry getCommandCmdlet = publicGetCommandEntries.OfType<SessionStateCmdletEntry>().FirstOrDefault();
                                if ((getCommandCmdlet != null) && (getCommandCmdlet.ImplementingType.Equals(typeof(Microsoft.PowerShell.Commands.GetCommandCommand))))
                        _initialSessionStateIncludesGetCommandWithListImportedSwitch = newValue;
            return _initialSessionStateIncludesGetCommandWithListImportedSwitch.Value;
        /// Handle the invocation of command discovery pipeline.
        private void HandleGetCommandMetadata(object sender, RemoteDataEventArgs<RemoteDataObject<PSObject>> eventArgs)
            PowerShell countingPipeline = RemotingDecoder.GetCommandDiscoveryPipeline(data.Data);
            if (this.DoesInitialSessionStateIncludeGetCommandWithListImportedSwitch())
                countingPipeline.AddParameter("ListImported", true);
            countingPipeline
                .AddParameter("ErrorAction", "SilentlyContinue")
                .AddCommand("Measure-Object")
                .AddParameter("Property", "Count");
            PowerShell mainPipeline = RemotingDecoder.GetCommandDiscoveryPipeline(data.Data);
                mainPipeline.AddParameter("ListImported", true);
            mainPipeline
                .AddParameter("Property", new string[] {
                    "Name", "Namespace", "HelpUri", "CommandType", "ResolvedCommandName", "OutputType", "Parameters" });
            HostInfo useRunspaceHost = new HostInfo(null);
            useRunspaceHost.UseRunspaceHost = true;
                    countingPipeline,
                    mainPipeline,
                    useRunspaceHost,
                // Run on usual driver.
                    true /* no input */,
                    ApartmentState.Unknown,
                    0 /* stream options */,
                    false /* addToHistory */,
                    null /* use default rsPool runspace */);
        /// Handles host responses.
        private void HandleHostResponseReceived(object sender,
            RemoteDataEventArgs<RemoteHostResponse> eventArgs)
            _remoteHost.ServerMethodExecutor.HandleRemoteHostResponseFromClient((eventArgs.Data));
        /// Sets the maximum runspace of the runspace pool and sends a response back.
        /// <param name="eventArgs">contains information about the new maxRunspaces
        /// and the callId at the client</param>
        private void HandleSetMaxRunspacesReceived(object sender, RemoteDataEventArgs<PSObject> eventArgs)
            int maxRunspaces = (int)((PSNoteProperty)data.Properties[RemoteDataNameStrings.MaxRunspaces]).Value;
            long callId = (long)((PSNoteProperty)data.Properties[RemoteDataNameStrings.CallId]).Value;
            bool response = RunspacePool.SetMaxRunspaces(maxRunspaces);
            DataStructureHandler.SendResponseToClient(callId, response);
        /// Sets the minimum runspace of the runspace pool and sends a response back.
        /// <param name="eventArgs">contains information about the new minRunspaces
        private void HandleSetMinRunspacesReceived(object sender, RemoteDataEventArgs<PSObject> eventArgs)
            int minRunspaces = (int)((PSNoteProperty)data.Properties[RemoteDataNameStrings.MinRunspaces]).Value;
            bool response = RunspacePool.SetMinRunspaces(minRunspaces);
        /// Gets the available runspaces from the server and sends it across
        /// to the client.
        /// <param name="eventArgs">Contains information on the callid.</param>
        private void HandleGetAvailableRunspacesReceived(object sender, RemoteDataEventArgs<PSObject> eventArgs)
            int availableRunspaces = RunspacePool.GetAvailableRunspaces();
            DataStructureHandler.SendResponseToClient(callId, availableRunspaces);
        /// Forces a state reset on a single runspace pool.
        private void HandleResetRunspaceState(object sender, RemoteDataEventArgs<PSObject> eventArgs)
            long callId = (long)((PSNoteProperty)(eventArgs.Data).Properties[RemoteDataNameStrings.CallId]).Value;
            bool response = ResetRunspaceState();
        /// Resets the single runspace in the runspace pool.
        private bool ResetRunspaceState()
            LocalRunspace runspaceToReset = _rsToUseForSteppablePipeline as LocalRunspace;
            if ((runspaceToReset == null) || (RunspacePool.GetMaxRunspaces() > 1))
                // Local runspace state reset.
                runspaceToReset.ResetRunspaceState();
        /// Starts the PowerShell command on the currently pushed Runspace.
        /// <param name="powershell">PowerShell command or script.</param>
        /// <param name="extraPowerShell">PowerShell command to run after first completes.</param>
        /// <param name="powershellId">PowerShell Id.</param>
        /// <param name="runspacePoolId">RunspacePool Id.</param>
        /// <param name="hostInfo">Host Info.</param>
        /// <param name="streamOptions">Remote stream options.</param>
        /// <param name="noInput">False when input is provided.</param>
        /// <param name="addToHistory">Add to history.</param>
        private void StartPowerShellCommandOnPushedRunspace(
            PowerShell powershell,
            PowerShell extraPowerShell,
            Guid powershellId,
            RemoteStreamOptions streamOptions,
            bool addToHistory)
            Runspace runspace = _remoteHost.PushedRunspace;
                extraPowerShell,
                powershellId,
                ApartmentState.MTA,
                runspace);
                // Pop runspace on error.
        #region Remote Debugger Command Helpers
        /// Debugger command pre processing result type.
        private enum PreProcessCommandResult
            /// No debugger pre-processing. "Get" commands use this so that the
            /// data they retrieve can be sent back to the caller.
            /// This is a valid debugger command but was not processed because
            /// the debugger state was not correct.
            ValidNotProcessed,
            SetDebuggerAction,
            SetDebugMode,
            /// SetDebuggerStepMode.
            SetDebuggerStepMode,
            /// SetPreserveUnhandledBreakpointMode.
            SetPreserveUnhandledBreakpointMode,
            /// The PreProcessCommandResult used for managing breakpoints.
            BreakpointManagement,
        private sealed class DebuggerCommandArgument
            public DebugModes? Mode { get; set; }
            public DebuggerResumeAction? ResumeAction { get; set; }
            public bool? DebuggerStepEnabled { get; set; }
            public UnhandledBreakpointProcessingMode? UnhandledBreakpointMode { get; set; }
        /// Pre-processor for debugger commands.
        /// Parses special debugger commands and converts to equivalent script for remote execution as needed.
        /// <param name="commands">The PSCommand.</param>
        /// <param name="serverRemoteDebugger">The debugger that can be used to invoke debug operations via API.</param>
        /// <param name="preProcessOutput">A Collection that can be used to send output to the client.</param>
        /// <param name="commandArgument">Command argument.</param>
        /// <returns>PreProcessCommandResult type if preprocessing occurred.</returns>
        private static PreProcessCommandResult PreProcessDebuggerCommand(
            PSCommand commands,
            ServerRemoteDebugger serverRemoteDebugger,
            Collection<object> preProcessOutput,
            out DebuggerCommandArgument commandArgument)
            commandArgument = new DebuggerCommandArgument();
            PreProcessCommandResult result = PreProcessCommandResult.None;
            if (commands.Commands.Count == 0 || commands.Commands[0].IsScript)
            var command = commands.Commands[0];
            string commandText = command.CommandText;
            if (commandText.Equals(RemoteDebuggingCommands.GetDebuggerStopArgs, StringComparison.OrdinalIgnoreCase))
                // __Get-PSDebuggerStopArgs private virtual command.
                // No input parameters.
                // Returns DebuggerStopEventArgs object.
                // Evaluate this command only if the debugger is activated.
                if (!serverRemoteDebugger.IsActive)
                    return PreProcessCommandResult.ValidNotProcessed;
                ReplaceVirtualCommandWithScript(commands, "$host.Runspace.Debugger.GetDebuggerStopArgs()");
            else if (commandText.Equals(RemoteDebuggingCommands.SetDebuggerAction, StringComparison.OrdinalIgnoreCase))
                // __Set-PSDebuggerAction private virtual command.
                // DebuggerResumeAction enum input parameter.
                // Returns void.
                if ((command.Parameters == null) || (command.Parameters.Count == 0) ||
                    (!command.Parameters[0].Name.Equals("ResumeAction", StringComparison.OrdinalIgnoreCase)))
                    throw new PSArgumentException("ResumeAction");
                DebuggerResumeAction? resumeAction = null;
                PSObject resumeObject = command.Parameters[0].Value as PSObject;
                if (resumeObject != null)
                        resumeAction = (DebuggerResumeAction)resumeObject.BaseObject;
                        // Do nothing.
                commandArgument.ResumeAction = resumeAction ?? throw new PSArgumentException("ResumeAction");
                result = PreProcessCommandResult.SetDebuggerAction;
            else if (commandText.Equals(RemoteDebuggingCommands.SetDebugMode, StringComparison.OrdinalIgnoreCase))
                // __Set-PSDebugMode private virtual command.
                // DebugModes enum input parameter.
                    (!command.Parameters[0].Name.Equals("Mode", StringComparison.OrdinalIgnoreCase)))
                    throw new PSArgumentException("Mode");
                DebugModes? mode = null;
                PSObject modeObject = command.Parameters[0].Value as PSObject;
                if (modeObject != null)
                        mode = (DebugModes)modeObject.BaseObject;
                commandArgument.Mode = mode ?? throw new PSArgumentException("Mode");
                result = PreProcessCommandResult.SetDebugMode;
            else if (commandText.Equals(RemoteDebuggingCommands.SetDebuggerStepMode, StringComparison.OrdinalIgnoreCase))
                // __Set-PSDebuggerStepMode private virtual command.
                // Boolean Enabled input parameter.
                   (!command.Parameters[0].Name.Equals("Enabled", StringComparison.OrdinalIgnoreCase)))
                    throw new PSArgumentException("Enabled");
                bool enabled = (bool)command.Parameters[0].Value;
                commandArgument.DebuggerStepEnabled = enabled;
                result = PreProcessCommandResult.SetDebuggerStepMode;
            else if (commandText.Equals(RemoteDebuggingCommands.SetUnhandledBreakpointMode, StringComparison.OrdinalIgnoreCase))
                // __Set-PSUnhandledBreakpointMode private virtual command.
                // UnhandledBreakpointMode input parameter.
                   (!command.Parameters[0].Name.Equals("UnhandledBreakpointMode", StringComparison.OrdinalIgnoreCase)))
                    throw new PSArgumentException("UnhandledBreakpointMode");
                UnhandledBreakpointProcessingMode? mode = null;
                        mode = (UnhandledBreakpointProcessingMode)modeObject.BaseObject;
                commandArgument.UnhandledBreakpointMode = mode ?? throw new PSArgumentException("Mode");
                result = PreProcessCommandResult.SetPreserveUnhandledBreakpointMode;
            else if (commandText.Equals(RemoteDebuggingCommands.GetBreakpoint, StringComparison.OrdinalIgnoreCase))
                // __Get-PSBreakpoint private virtual command.
                // Input parameters:
                // [-Id <int>]
                // Returns Breakpoint object(s).
                TryGetParameter<int?>(command, "RunspaceId", out int? runspaceId);
                if (TryGetParameter<int>(command, "Id", out int breakpointId))
                    preProcessOutput.Add(serverRemoteDebugger.GetBreakpoint(breakpointId, runspaceId));
                    foreach (Breakpoint breakpoint in serverRemoteDebugger.GetBreakpoints(runspaceId))
                        preProcessOutput.Add(breakpoint);
                result = PreProcessCommandResult.BreakpointManagement;
            else if (commandText.Equals(RemoteDebuggingCommands.SetBreakpoint, StringComparison.OrdinalIgnoreCase))
                // __Set-PSBreakpoint private virtual command.
                // -Breakpoint <Breakpoint> or -BreakpointList <IEnumerable<Breakpoint>>
                // [-RunspaceId <int?>]
                TryGetParameter<Breakpoint>(command, "Breakpoint", out Breakpoint breakpoint);
                TryGetParameter<ArrayList>(command, "BreakpointList", out ArrayList breakpoints);
                if (breakpoint == null && breakpoints == null)
                    throw new PSArgumentException(DebuggerStrings.BreakpointOrBreakpointListNotSpecified);
                commands.Clear();
                // Any collection comes through remoting as an ArrayList of Objects so we convert each object
                // into a breakpoint and add it to the list.
                var bps = new List<Breakpoint>();
                if (breakpoints != null)
                    foreach (object obj in breakpoints)
                        if (!LanguagePrimitives.TryConvertTo<Breakpoint>(obj, out Breakpoint bp))
                            throw new PSArgumentException(DebuggerStrings.BreakpointListContainedANonBreakpoint);
                        bps.Add(bp);
                    bps.Add(breakpoint);
                serverRemoteDebugger.SetBreakpoints(bps, runspaceId);
                foreach (var bp in bps)
                    preProcessOutput.Add(bp);
            else if (commandText.Equals(RemoteDebuggingCommands.RemoveBreakpoint, StringComparison.OrdinalIgnoreCase))
                // __Remove-PSBreakpoint private virtual command.
                // -Id <int>
                // Returns bool.
                int breakpointId = GetParameter<int>(command, "Id");
                Breakpoint breakpoint = serverRemoteDebugger.GetBreakpoint(breakpointId, runspaceId);
                    breakpoint != null && serverRemoteDebugger.RemoveBreakpoint(breakpoint, runspaceId));
            else if (commandText.Equals(RemoteDebuggingCommands.EnableBreakpoint, StringComparison.OrdinalIgnoreCase))
                // __Enable-PSBreakpoint private virtual command.
                // Returns Breakpoint object.
                Breakpoint bp = serverRemoteDebugger.GetBreakpoint(breakpointId, runspaceId);
                    preProcessOutput.Add(serverRemoteDebugger.EnableBreakpoint(bp, runspaceId));
            else if (commandText.Equals(RemoteDebuggingCommands.DisableBreakpoint, StringComparison.OrdinalIgnoreCase))
                // __Disable-PSBreakpoint private virtual command.
                    preProcessOutput.Add(serverRemoteDebugger.DisableBreakpoint(bp, runspaceId));
        private static void ReplaceVirtualCommandWithScript(PSCommand commands, string script)
            ScriptBlock scriptBlock = ScriptBlock.Create(script);
            commands.AddCommand("Invoke-Command")
                    .AddParameter("ScriptBlock", scriptBlock)
                    .AddParameter("NoNewScope", true);
        private static T GetParameter<T>(Command command, string parameterName)
            if (command.Parameters?.Count == 0)
                throw new PSArgumentException(parameterName);
                if (string.Equals(param.Name, parameterName, StringComparison.OrdinalIgnoreCase))
                    return LanguagePrimitives.ConvertTo<T>(param.Value);
        private static bool TryGetParameter<T>(Command command, string parameterName, out T value)
                value = GetParameter<T>(command, parameterName);
            catch (Exception ex) when (
                ex is PSArgumentException ||
                ex is InvalidCastException ||
                ex is PSInvalidCastException)
                value = default(T);
        /// Helper class to run ServerPowerShellDriver objects on a single thread.  This is
        /// needed to support nested pipeline execution and remote debugging.
        private sealed class PowerShellDriverInvoker
            private readonly ConcurrentStack<InvokePump> _invokePumpStack;
            public PowerShellDriverInvoker()
                _invokePumpStack = new ConcurrentStack<InvokePump>();
            /// IsActive.
            public bool IsActive
                get { return !_invokePumpStack.IsEmpty; }
            /// True if thread is ready to invoke a PowerShell driver.
            public bool IsAvailable
                    InvokePump pump;
                    if (!_invokePumpStack.TryPeek(out pump))
                        pump = null;
                    return (pump != null) && !(pump.IsBusy);
            /// Submit a driver object to be invoked.
            /// <param name="driver">ServerPowerShellDriver.</param>
            public void InvokeDriverAsync(ServerPowerShellDriver driver)
                InvokePump currentPump;
                if (!_invokePumpStack.TryPeek(out currentPump))
                    throw new PSInvalidOperationException(RemotingErrorIdStrings.PowerShellInvokerInvalidState);
                currentPump.Dispatch(driver);
            /// Blocking call that creates a new pump object and pumps
            /// driver invokes until stopped via a PopInvoker call.
            public void PushInvoker()
                InvokePump newPump = new InvokePump();
                _invokePumpStack.Push(newPump);
                // Blocking call while new driver invocations are handled on
                // new pump.
                newPump.Start();
            /// Stops the current driver invoker and restores the previous
            /// invoker object on the stack, if any, to handle driver invocations.
            public void PopInvoker()
                InvokePump oldPump;
                if (_invokePumpStack.TryPop(out oldPump))
                    oldPump.Stop();
                    throw new PSInvalidOperationException(RemotingErrorIdStrings.CannotExitNestedPipeline);
            #region Private classes
            /// Class that queues and invokes ServerPowerShellDriver objects
            /// in sequence.
            private sealed class InvokePump
                private readonly Queue<ServerPowerShellDriver> _driverInvokeQueue;
                private readonly ManualResetEvent _processDrivers;
                private bool _stopPump;
                public InvokePump()
                    _driverInvokeQueue = new Queue<ServerPowerShellDriver>();
                    _processDrivers = new ManualResetEvent(false);
                            _processDrivers.WaitOne();
                            // Synchronously invoke one ServerPowerShellDriver at a time.
                            ServerPowerShellDriver driver = null;
                                if (_stopPump)
                                if (_driverInvokeQueue.Count > 0)
                                    driver = _driverInvokeQueue.Dequeue();
                                if (_driverInvokeQueue.Count == 0)
                                    _processDrivers.Reset();
                            if (driver != null)
                                    IsBusy = true;
                                    driver.InvokeMain();
                                    IsBusy = false;
                        _processDrivers.Dispose();
                public void Dispatch(ServerPowerShellDriver driver)
                    CheckDisposed();
                        _driverInvokeQueue.Enqueue(driver);
                        _processDrivers.Set();
                        _stopPump = true;
                public bool IsBusy { get; private set; }
                private void CheckDisposed()
                        throw new ObjectDisposedException("InvokePump");
    /// This class wraps the script debugger for a ServerRunspacePoolDriver runspace.
    internal sealed class ServerRemoteDebugger : Debugger, IDisposable
        private readonly IRSPDriverInvoke _driverInvoker;
        private readonly ObjectRef<Debugger> _wrappedDebugger;
        private bool _inDebugMode;
        private ManualResetEventSlim _nestedDebugStopCompleteEvent;
        private bool _nestedDebugging;
        private ManualResetEventSlim _processCommandCompleteEvent;
        private ThreadCommandProcessing _threadCommandProcessing;
        private bool _raiseStopEventLocally;
        internal const string SetPSBreakCommandText = "Set-PSBreakpoint";
        private ServerRemoteDebugger() { }
        /// <param name="driverInvoker"></param>
        /// <param name="debugger"></param>
        internal ServerRemoteDebugger(
            IRSPDriverInvoke driverInvoker,
            if (driverInvoker == null)
                throw new PSArgumentNullException(nameof(driverInvoker));
            _driverInvoker = driverInvoker;
            _wrappedDebugger = new ObjectRef<Debugger>(debugger);
            SetDebuggerCallbacks();
            _runspace.Name = "RemoteHost";
            _runspace.InternalDebugger = this;
            get { return _inDebugMode; }
            _wrappedDebugger.Value.SetBreakpoints(breakpoints, runspaceId);
            _wrappedDebugger.Value.GetBreakpoint(id, runspaceId);
            _wrappedDebugger.Value.GetBreakpoints(runspaceId);
            _wrappedDebugger.Value.SetCommandBreakpoint(command, action, path, runspaceId);
            _wrappedDebugger.Value.SetLineBreakpoint(path, line, column, action, runspaceId);
            _wrappedDebugger.Value.SetVariableBreakpoint(variableName, accessMode, action, path, runspaceId);
            _wrappedDebugger.Value.RemoveBreakpoint(breakpoint, runspaceId);
            _wrappedDebugger.Value.EnableBreakpoint(breakpoint, runspaceId);
            _wrappedDebugger.Value.DisableBreakpoint(breakpoint, runspaceId);
        /// Exits debugger mode with the provided resume action.
            if (!_inDebugMode)
                    StringUtil.Format(DebuggerStrings.CannotSetRemoteDebuggerAction));
            ExitDebugMode(resumeAction);
        /// Returns debugger stop event args if in debugger stop state.
            return _wrappedDebugger.Value.GetDebuggerStopArgs();
        /// <param name="command">Command.</param>
            if (LocalDebugMode)
                return _wrappedDebugger.Value.ProcessCommand(command, output);
            if (!InBreakpoint || (_threadCommandProcessing != null))
                    StringUtil.Format(DebuggerStrings.CannotProcessDebuggerCommandNotStopped));
            _processCommandCompleteEvent ??= new ManualResetEventSlim(false);
            _threadCommandProcessing = new ThreadCommandProcessing(command, output, _wrappedDebugger.Value, _processCommandCompleteEvent);
                return _threadCommandProcessing.Invoke(_nestedDebugStopCompleteEvent);
                _threadCommandProcessing = null;
                _wrappedDebugger.Value.StopProcessCommand();
            ThreadCommandProcessing threadCommandProcessing = _threadCommandProcessing;
            threadCommandProcessing?.Stop();
            _wrappedDebugger.Value.SetDebugMode(mode);
                return (InBreakpoint || _wrappedDebugger.Value.IsActive || _wrappedDebugger.Value.InBreakpoint);
            // Enable both the wrapper and wrapped debuggers for debugging before setting step mode.
            const DebugModes mode = DebugModes.LocalScript | DebugModes.RemoteScript;
            _wrappedDebugger.Value.SetDebuggerStepMode(enabled);
            return _wrappedDebugger.Value.InternalProcessCommand(command, output);
        internal override void DebugJob(Job job, bool breakAll) =>
            _wrappedDebugger.Value.DebugJob(job, breakAll);
            _wrappedDebugger.Value.StopDebugJob(job);
            _wrappedDebugger.Value.DebugRunspace(runspace, breakAll);
            _wrappedDebugger.Value.StopDebugRunspace(runspace);
                return _wrappedDebugger.Value.IsPushed;
                return _wrappedDebugger.Value.IsRemote;
                return _wrappedDebugger.Value.IsDebuggerSteppingEnabled;
                return _wrappedDebugger.Value.UnhandledBreakpointMode;
                _wrappedDebugger.Value.UnhandledBreakpointMode = value;
                if (value == UnhandledBreakpointProcessingMode.Ignore &&
                    _inDebugMode)
                    // Release debugger stop hold.
        /// IsPendingDebugStopEvent.
            get { return _wrappedDebugger.Value.IsPendingDebugStopEvent; }
        /// ReleaseSavedDebugStop.
            _wrappedDebugger.Value.ReleaseSavedDebugStop();
            return _wrappedDebugger.Value.GetCallStack();
            _wrappedDebugger.Value.Break(triggerObject);
            RemoveDebuggerCallbacks();
            if (_inDebugMode)
                ExitDebugMode(DebuggerResumeAction.Stop);
            _nestedDebugStopCompleteEvent?.Dispose();
            _processCommandCompleteEvent?.Dispose();
        private sealed class ThreadCommandProcessing
            // Members
            private readonly ManualResetEventSlim _commandCompleteEvent;
            private readonly PSCommand _command;
            private readonly PSDataCollection<PSObject> _output;
            private DebuggerCommandResults _results;
            private Exception _exception;
            // Constructors
            private ThreadCommandProcessing() { }
            public ThreadCommandProcessing(
                ManualResetEventSlim processCommandCompleteEvent)
                _output = output;
                _commandCompleteEvent = processCommandCompleteEvent;
            public DebuggerCommandResults Invoke(ManualResetEventSlim startInvokeEvent)
                // Get impersonation information to flow if any.
                // Signal thread to process command.
                Dbg.Assert(!_commandCompleteEvent.IsSet, "Command complete event shoulds always be non-signaled here.");
                Dbg.Assert(!startInvokeEvent.IsSet, "The event should always be in non-signaled state here.");
                startInvokeEvent.Set();
                // Wait for completion.
                _commandCompleteEvent.Wait();
                _commandCompleteEvent.Reset();
                    _identityToImpersonate.Dispose();
                // Propagate exception.
                // Return command processing results.
                Debugger debugger = _wrappedDebugger;
                debugger?.StopProcessCommand();
            internal void DoInvoke()
                        _results = WindowsIdentity.RunImpersonated(
                            () => _wrappedDebugger.ProcessCommand(_command, _output));
                    _results = _wrappedDebugger.ProcessCommand(_command, _output);
                    _exception = e;
                    _commandCompleteEvent.Set();
        /// Add Debugger suspend execution callback.
        private void SetDebuggerCallbacks()
            if (_runspace != null &&
                _runspace.ExecutionContext != null &&
                _wrappedDebugger.Value != null)
                SubscribeWrappedDebugger(_wrappedDebugger.Value);
                // Register debugger events for remote forwarding.
                var eventManager = _runspace.ExecutionContext.Events;
                if (!eventManager.GetEventSubscribers(RemoteDebugger.RemoteDebuggerStopEvent).GetEnumerator().MoveNext())
                    eventManager.SubscribeEvent(
                        eventName: null,
                        sourceIdentifier: RemoteDebugger.RemoteDebuggerStopEvent,
                        action: null,
                        forwardEvent: true);
                if (!eventManager.GetEventSubscribers(RemoteDebugger.RemoteDebuggerBreakpointUpdatedEvent).GetEnumerator().MoveNext())
                        sourceIdentifier: RemoteDebugger.RemoteDebuggerBreakpointUpdatedEvent,
        /// Remove the suspend execution callback.
        private void RemoveDebuggerCallbacks()
                UnsubscribeWrappedDebugger(_wrappedDebugger.Value);
                // Unregister debugger events for remote forwarding.
                foreach (var subscriber in eventManager.GetEventSubscribers(RemoteDebugger.RemoteDebuggerStopEvent))
                    eventManager.UnsubscribeEvent(subscriber);
                foreach (var subscriber in eventManager.GetEventSubscribers(RemoteDebugger.RemoteDebuggerBreakpointUpdatedEvent))
            // Ignore if we are in restricted mode.
            if (!IsDebuggingSupported())
                // Forward event locally.
            if ((DebugMode & DebugModes.RemoteScript) != DebugModes.RemoteScript)
            PSHost contextHost = null;
                // Save current context remote host.
                contextHost = _runspace.ExecutionContext.InternalHost.ExternalHost;
                // Forward event to remote client.
                Dbg.Assert(_runspace != null, "Runspace cannot be null.");
                _runspace.ExecutionContext.Events.GenerateEvent(
                    args: new object[] { e },
                // Start the debug mode.  This is a blocking call and will return only
                // after ExitDebugMode() is called.
                EnterDebugMode(_wrappedDebugger.Value.IsPushed);
                // Restore original context remote host.
                _runspace.ExecutionContext.InternalHost.SetHostRef(contextHost);
        /// HandleBreakpointUpdated.
        private void HandleNestedDebuggingCancelEvent(object sender, EventArgs e)
            // Forward cancel event from wrapped debugger.
            // Release debugger.
        /// Sends a DebuggerStop event to the client and enters a nested pipeline.
        private void EnterDebugMode(bool isNestedStop)
            _inDebugMode = true;
                _runspace.ExecutionContext.SetVariable(SpecialVariables.NestedPromptCounterVarPath, 1);
                if (isNestedStop)
                    // Blocking call for nested debugger execution (Debug-Runspace) stop events.
                    // The root debugger never makes two EnterDebugMode calls without an ExitDebugMode.
                    _nestedDebugStopCompleteEvent ??= new ManualResetEventSlim(false);
                    _nestedDebugging = true;
                    OnEnterDebugMode(_nestedDebugStopCompleteEvent);
                    // Process all client commands as nested until nested pipeline is exited at
                    // which point this call returns.
                    _driverInvoker.EnterNestedPipeline();
                _inDebugMode = false;
                _nestedDebugging = false;
            // Check to see if we should re-raise the stop event locally.
            if (_raiseStopEventLocally)
                _raiseStopEventLocally = false;
                LocalDebugMode = true;
                HandleDebuggerStop(this, _debuggerStopEventArgs);
        /// Blocks DebuggerStop event thread until exit debug mode is
        /// received from the client.
        private void OnEnterDebugMode(ManualResetEventSlim debugModeCompletedEvent)
            Dbg.Assert(!debugModeCompletedEvent.IsSet, "Event should always be non-signaled here.");
                debugModeCompletedEvent.Wait();
                debugModeCompletedEvent.Reset();
                if (_threadCommandProcessing != null)
                    // Process command.
                    _threadCommandProcessing.DoInvoke();
                    // No command to process.  Exit debug mode.
        /// Exits the server side nested pipeline.
                if (_nestedDebugging)
                    // Release nested debugger.
                    _nestedDebugStopCompleteEvent.Set();
                    // Release EnterDebugMode blocking call.
                    _driverInvoker.ExitNestedPipeline();
                _runspace.ExecutionContext.SetVariable(SpecialVariables.NestedPromptCounterVarPath, 0);
        private void SubscribeWrappedDebugger(Debugger wrappedDebugger)
            wrappedDebugger.DebuggerStop += HandleDebuggerStop;
            wrappedDebugger.BreakpointUpdated += HandleBreakpointUpdated;
            wrappedDebugger.NestedDebuggingCancelledEvent += HandleNestedDebuggingCancelEvent;
        private void UnsubscribeWrappedDebugger(Debugger wrappedDebugger)
            wrappedDebugger.DebuggerStop -= HandleDebuggerStop;
            wrappedDebugger.BreakpointUpdated -= HandleBreakpointUpdated;
            wrappedDebugger.NestedDebuggingCancelledEvent -= HandleNestedDebuggingCancelEvent;
        private bool IsDebuggingSupported()
            // Restriction only occurs on a (non-pushed) local runspace.
                CmdletInfo cmdletInfo = localRunspace.ExecutionContext.EngineSessionState.GetCmdlet(SetPSBreakCommandText);
                if ((cmdletInfo != null) && (cmdletInfo.Visibility != SessionStateEntryVisibility.Public))
        /// HandleStopSignal.
        /// <returns>True if stop signal is handled.</returns>
        internal bool HandleStopSignal()
            // If in pushed mode then stop any running command.
            if (IsPushed && (_threadCommandProcessing != null))
                StopProcessCommand();
            // Set debug mode to "None" so that current command can stop and not
            // potentially not respond in a debugger stop.  Use RestoreDebugger() to
            // restore debugger to original mode.
            _wrappedDebugger.Value.SetDebugMode(DebugModes.None);
            if (InBreakpoint)
        // Sets the wrapped debugger to the same mode as the wrapper
        // server remote debugger, enabling it if remote debugging is enabled.
        internal void CheckDebuggerState()
            if ((_wrappedDebugger.Value.DebugMode == DebugModes.None &&
                (DebugMode & DebugModes.RemoteScript) == DebugModes.RemoteScript))
                _wrappedDebugger.Value.SetDebugMode(DebugMode);
        internal void StartPowerShellCommand(
            ServerRunspacePoolDriver runspacePoolDriver,
            ServerRemoteHost remoteHost,
            // For nested debugger command processing, invoke command on new local runspace since
            // the root script debugger runspace is unavailable (it is running a PS script).
            Runspace runspace = (remoteHost != null) ?
                RunspaceFactory.CreateRunspace(remoteHost) : RunspaceFactory.CreateRunspace();
                powershell.InvocationStateChanged += HandlePowerShellInvocationStateChanged;
                const string script = @"
                    param ($Debugger, $Commands, $output)
                    trap { throw $_ }
                    $Debugger.ProcessCommand($Commands, $output)
                PSCommand Commands = new PSCommand(powershell.Commands);
                powershell.AddScript(script).AddParameter("Debugger", this).AddParameter("Commands", Commands).AddParameter("output", output);
                    runspacePoolDriver,
                    runspace,
        private void HandlePowerShellInvocationStateChanged(object sender, PSInvocationStateChangedEventArgs e)
            if (e.InvocationStateInfo.State == PSInvocationState.Completed ||
                e.InvocationStateInfo.State == PSInvocationState.Stopped ||
                e.InvocationStateInfo.State == PSInvocationState.Failed)
                PowerShell powershell = sender as PowerShell;
                powershell.InvocationStateChanged -= HandlePowerShellInvocationStateChanged;
                Runspace runspace = powershell.GetRunspaceConnection() as Runspace;
        internal int GetBreakpointCount()
            ScriptDebugger scriptDebugger = _wrappedDebugger.Value as ScriptDebugger;
            if (scriptDebugger != null)
                return scriptDebugger.GetBreakpoints().Count;
        internal void PushDebugger(Debugger debugger)
            if (debugger.Equals(this))
                throw new PSInvalidOperationException(DebuggerStrings.RemoteServerDebuggerCannotPushSelf);
            if (_wrappedDebugger.IsOverridden)
                throw new PSInvalidOperationException(DebuggerStrings.RemoteServerDebuggerAlreadyPushed);
            // Swap wrapped debugger.
            _wrappedDebugger.Override(debugger);
        internal void PopDebugger()
            if (!_wrappedDebugger.IsOverridden)
            _wrappedDebugger.Revert();
        internal void ReleaseAndRaiseDebugStopLocal()
                // Release debugger stop and signal to re-raise locally.
                _raiseStopEventLocally = true;
        /// When true, this debugger is being used for local debugging (not remote debugging)
        /// via the Debug-Runspace cmdlet.
        internal bool LocalDebugMode

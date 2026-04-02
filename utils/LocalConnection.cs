using System.Diagnostics.CodeAnalysis; // for fxcop
    /// Runspace class for local runspace.
    internal sealed partial class LocalRunspace : RunspaceBase
        /// Construct an instance of an Runspace using a custom implementation
        /// of PSHost.
        /// configuration information for this minshell.
        /// If true, don't make a copy of the initial session state object
        internal LocalRunspace(PSHost host, InitialSessionState initialSessionState, bool suppressClone)
            : base(host, initialSessionState, suppressClone)
        internal LocalRunspace(PSHost host, InitialSessionState initialSessionState)
            : base(host, initialSessionState)
        /// Local runspace pool is created with application private data set to an empty <see cref="PSPrimitiveDictionary"/>.
        public override PSPrimitiveDictionary GetApplicationPrivateData()
            // if we didn't get applicationPrivateData from a runspace pool,
            // then we create a new one
            if (_applicationPrivateData == null)
                lock (this.SyncRoot)
                    _applicationPrivateData ??= new PSPrimitiveDictionary();
            return _applicationPrivateData;
        internal override void SetApplicationPrivateData(PSPrimitiveDictionary applicationPrivateData)
            _applicationPrivateData = applicationPrivateData;
        private PSPrimitiveDictionary _applicationPrivateData;
        public override PSEventManager Events
                System.Management.Automation.ExecutionContext context = this.GetExecutionContext;
                return context.Events;
        public override PSThreadOptions ThreadOptions
                return _createThreadOptions;
                    if (value == _createThreadOptions)
                        if (!IsValidThreadOptionsConfiguration(value))
                            throw new InvalidOperationException(StringUtil.Format(RunspaceStrings.InvalidThreadOptionsChange));
                    _createThreadOptions = value;
        private bool IsValidThreadOptionsConfiguration(PSThreadOptions options)
            // If the runspace is already opened, we only allow changing options when:
            //  - The new value is ReuseThread, and
            //  - The apartment state is not STA
            return options == PSThreadOptions.ReuseThread && this.ApartmentState != ApartmentState.STA;
        private PSThreadOptions _createThreadOptions = PSThreadOptions.Default;
        /// Resets the runspace state to allow for fast reuse. Not all of the runspace
        /// elements are reset. The goal is to minimize the chance of the user taking
        /// accidental dependencies on prior runspace state.
        public override void ResetRunspaceState()
            PSInvalidOperationException invalidOperation = null;
            if (this.InitialSessionState == null)
                invalidOperation = PSTraceSource.NewInvalidOperationException();
            else if (this.RunspaceState != Runspaces.RunspaceState.Opened)
                invalidOperation = PSTraceSource.NewInvalidOperationException(
                        RunspaceStrings.RunspaceNotInOpenedState, this.RunspaceState);
            else if (this.RunspaceAvailability != Runspaces.RunspaceAvailability.Available)
                        RunspaceStrings.ConcurrentInvokeNotAllowed);
            if (invalidOperation != null)
                invalidOperation.Source = "ResetRunspaceState";
            this.InitialSessionState.ResetRunspaceState(this.ExecutionContext);
            // Finally, reset history for this runspace. This needs to be done
            // last to so that changes to the default MaximumHistorySize will be picked up.
            _history = new History(this.ExecutionContext);
        #region protected_methods
        /// <param name="command">A valid command string. Can be null.</param>
        protected override Pipeline CoreCreatePipeline(string command, bool addToHistory, bool isNested)
                throw PSTraceSource.NewObjectDisposedException("runspace");
            return (Pipeline)new LocalPipeline(this, command, addToHistory, isNested);
        #endregion protected_methods
        #region protected_properties
        internal override ExecutionContext GetExecutionContext
                if (_engine == null)
                    return _engine.Context;
        internal override bool InNestedPrompt
                return context.InternalHost.HostInNestedPrompt() || InInternalNestedPrompt;
        /// Allows internal nested commands to be run as "HostInNestedPrompt" so that CreatePipelineProcessor() does
        /// not set CommandOrigin to Internal as it normally does by default.  This then allows cmdlets like Invoke-History
        /// to replay history command lines in the current runspace with the same language mode context as the host.
        internal bool InInternalNestedPrompt
        #endregion protected_properties
        #region internal_properties
        /// Gets history manager for this runspace.
        internal History History
                return _history;
        /// Gets transcription data for this runspace.
        internal TranscriptionData TranscriptionData
                return _transcriptionData;
        private TranscriptionData _transcriptionData = null;
        private JobRepository _jobRepository;
        /// List of jobs in this runspace.
        internal JobRepository JobRepository
                return _jobRepository;
        private JobManager _jobManager;
        public override JobManager JobManager
                return _jobManager;
        private RunspaceRepository _runspaceRepository;
        /// List of remote runspaces in this runspace.
                return _runspaceRepository;
        #endregion internal_properties
        #region Debugger
        /// Debugger.
        public override Debugger Debugger
                return InternalDebugger ?? base.Debugger;
        private static readonly string s_debugPreferenceCachePath = Path.Combine(Platform.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WindowsPowerShell", "DebugPreference.clixml");
        private static readonly object s_debugPreferenceLockObject = new object();
        /// DebugPreference serves as a property bag to keep
        /// track of all process specific debug preferences.
        public class DebugPreference
            public string[] AppDomainNames;
        /// CreateDebugPerfStruct is a helper method to populate DebugPreference.
        /// <param name="AppDomainNames">App Domain Names.</param>
        /// <returns>DebugPreference.</returns>
        private static DebugPreference CreateDebugPreference(string[] AppDomainNames)
            DebugPreference DebugPreference = new DebugPreference();
            DebugPreference.AppDomainNames = AppDomainNames;
            return DebugPreference;
        /// SetDebugPreference is a helper method used to enable and disable debug preference.
        /// <param name="enable">Indicates if the debug preference has to be enabled or disabled.</param>
        internal static void SetDebugPreference(string processName, List<string> appDomainName, bool enable)
            lock (s_debugPreferenceLockObject)
                bool iscacheUpdated = false;
                Hashtable debugPreferenceCache = null;
                string[] appDomainNames = null;
                    appDomainNames = appDomainName.ToArray();
                if (!File.Exists(LocalRunspace.s_debugPreferenceCachePath))
                        DebugPreference DebugPreference = CreateDebugPreference(appDomainNames);
                        debugPreferenceCache = new Hashtable();
                        debugPreferenceCache.Add(processName, DebugPreference);
                        iscacheUpdated = true;
                    debugPreferenceCache = GetDebugPreferenceCache(null);
                    if (debugPreferenceCache != null)
                            // Debug preference is set to enable.
                            // If the cache does not contain the process name, then we just update the cache.
                            if (!debugPreferenceCache.ContainsKey(processName))
                                // In this case, the cache contains the process name, hence we check the list of
                                // app domains for which the debug preference is set to enable.
                                DebugPreference processDebugPreference = GetProcessSpecificDebugPreference(debugPreferenceCache[processName]);
                                // processDebugPreference would point to null if debug preference is enabled for all app domains.
                                // If processDebugPreference is not null then it means that user has selected specific
                                // appdomins for which the debug preference has to be enabled.
                                if (processDebugPreference != null)
                                    List<string> cachedAppDomainNames = null;
                                    if (processDebugPreference.AppDomainNames != null && processDebugPreference.AppDomainNames.Length > 0)
                                        cachedAppDomainNames = new List<string>(processDebugPreference.AppDomainNames);
                                            if (!cachedAppDomainNames.Contains(currentAppDomainName, StringComparer.OrdinalIgnoreCase))
                                                cachedAppDomainNames.Add(currentAppDomainName);
                                    if (iscacheUpdated)
                                        DebugPreference DebugPreference = CreateDebugPreference(cachedAppDomainNames.ToArray());
                                        debugPreferenceCache[processName] = DebugPreference;
                            // Debug preference is set to disable.
                            if (debugPreferenceCache.ContainsKey(processName))
                                if (appDomainName == null)
                                    debugPreferenceCache.Remove(processName);
                                                if (cachedAppDomainNames.Contains(currentAppDomainName, StringComparer.OrdinalIgnoreCase))
                                                    // remove requested appdomains debug preference details.
                                                    cachedAppDomainNames.Remove(currentAppDomainName);
                        // For whatever reason, cache is corrupted. Hence override the cache content.
                        ps.AddCommand("Export-Clixml").AddParameter("Path", LocalRunspace.s_debugPreferenceCachePath).AddParameter("InputObject", debugPreferenceCache);
        /// GetDebugPreferenceCache is a helper method used to fetch
        /// the debug preference cache contents as a Hashtable.
        /// <returns>If the Debug preference is persisted then a hashtable containing
        /// the debug preference is returned or else Null is returned.</returns>
        private static Hashtable GetDebugPreferenceCache(Runspace runspace)
                ps.AddCommand("Import-Clixml").AddParameter("Path", LocalRunspace.s_debugPreferenceCachePath);
                Collection<PSObject> psObjects = ps.Invoke();
                if (psObjects != null && psObjects.Count == 1)
                    debugPreferenceCache = psObjects[0].BaseObject as Hashtable;
            return debugPreferenceCache;
        /// GetProcessSpecificDebugPreference is a helper method used to fetch persisted process specific debug preference.
        /// <param name="debugPreference"></param>
        private static DebugPreference GetProcessSpecificDebugPreference(object debugPreference)
            DebugPreference processDebugPreference = null;
            if (debugPreference != null)
                PSObject debugPreferencePsObject = debugPreference as PSObject;
                if (debugPreferencePsObject != null)
                    processDebugPreference = LanguagePrimitives.ConvertTo<DebugPreference>(debugPreferencePsObject);
            return processDebugPreference;
        /// Open the runspace.
        /// <param name="syncCall">
        /// parameter which control if Open is done synchronously or asynchronously
        protected override void OpenHelper(bool syncCall)
                // Open runspace synchronously
                DoOpenHelper();
                // Open runspace in another thread
                Thread asyncThread = new Thread(new ThreadStart(this.OpenThreadProc));
                asyncThread.Start();
        /// Start method for asynchronous open.
        private void OpenThreadProc()
                // This exception is reported by raising RunspaceState
                // change event.
        /// Helper function used for opening a runspace.
        private void DoOpenHelper()
            Dbg.Assert(InitialSessionState != null, "InitialSessionState should not be null");
            bool startLifeCycleEventWritten = false;
            s_runspaceInitTracer.WriteLine("begin open runspace");
                _transcriptionData = new TranscriptionData();
                // All ISS-based configuration of the engine itself is done by AutomationEngine,
                // which calls InitialSessionState.Bind(). Anything that doesn't
                // require an active and open runspace should be done in ISS.Bind()
                _engine = new AutomationEngine(Host, InitialSessionState);
                _engine.Context.CurrentRunspace = this;
                // Log engine for start of engine life
                MshLog.LogEngineLifecycleEvent(_engine.Context, EngineState.Available);
                startLifeCycleEventWritten = true;
                _history = new History(_engine.Context);
                _jobRepository = new JobRepository();
                _jobManager = new JobManager();
                _runspaceRepository = new RunspaceRepository();
                s_runspaceInitTracer.WriteLine("initializing built-in aliases and variable information");
                InitializeDefaults();
                s_runspaceInitTracer.WriteLine("Runspace open failed");
                // Log engine health event
                LogEngineHealthEvent(exception);
                // Log engine for end of engine life
                if (startLifeCycleEventWritten)
                    Dbg.Assert(_engine.Context != null, "if startLifeCycleEventWritten is true, ExecutionContext must be present");
                    MshLog.LogEngineLifecycleEvent(_engine.Context, EngineState.Stopped);
                // Open failed. Set the RunspaceState to Broken.
                SetRunspaceState(RunspaceState.Broken, exception);
                // Raise the event
                // Rethrow the exception. For asynchronous execution,
                // OpenThreadProc will catch it. For synchronous execution
                // caller of open will catch it.
            SetRunspaceState(RunspaceState.Opened);
            RunspaceOpening.Set();
            s_runspaceInitTracer.WriteLine("runspace opened successfully");
            // Now do initial state configuration that requires an active runspace
            Exception initError = InitialSessionState.BindRunspace(this, s_runspaceInitTracer);
                LogEngineHealthEvent(initError);
                Debug.Assert(_engine.Context != null,
                            "if startLifeCycleEventWritten is true, ExecutionContext must be present");
                SetRunspaceState(RunspaceState.Broken, initError);
                // Throw the exception. For asynchronous execution,
                throw initError;
            TelemetryAPI.ReportLocalSessionCreated(InitialSessionState, TranscriptionData);
        /// Logs engine health event.
        internal void LogEngineHealthEvent(Exception exception)
            LogEngineHealthEvent(
                Severity.Error,
        internal void LogEngineHealthEvent(Exception exception,
                             Severity severity,
                             Dictionary<string, string> additionalInfo)
            Dbg.Assert(exception != null, "Caller should validate the parameter");
            LogContext logContext = new LogContext();
            logContext.EngineVersion = Version.ToString();
            logContext.HostId = Host.InstanceId.ToString();
            logContext.HostName = Host.Name;
            logContext.HostVersion = Host.Version.ToString();
            logContext.RunspaceId = InstanceId.ToString();
            logContext.Severity = severity.ToString();
            logContext.ShellId = Utils.DefaultPowerShellShellID;
                logContext,
                additionalInfo);
        /// Returns the thread that must be used to execute pipelines when CreateThreadOptions is ReuseThread.
        /// The pipeline calls this function after ensuring there is a single thread in the pipeline, so no locking is necessary
        internal PipelineThread GetPipelineThread()
            _pipelineThread ??= new PipelineThread(this.ApartmentState);
            return _pipelineThread;
        private PipelineThread _pipelineThread = null;
        protected override void CloseHelper(bool syncCall)
                // Do close synchronously
                DoCloseHelper();
                // Do close asynchronously
                Thread asyncThread = new Thread(new ThreadStart(this.CloseThreadProc));
        /// Start method for asynchronous close.
        private void CloseThreadProc()
        /// Attempts to create/execute pipelines after a call to
        private void DoCloseHelper()
            var isPrimaryRunspace = (Runspace.PrimaryRunspace == this);
            var haveOpenRunspaces = false;
            foreach (Runspace runspace in RunspaceList)
                if (runspace.RunspaceStateInfo.State == RunspaceState.Opened)
                    haveOpenRunspaces = true;
            // When closing the primary runspace, ensure all other local runspaces are closed.
            var closeAllOpenRunspaces = isPrimaryRunspace && haveOpenRunspaces;
            // Stop all transcriptions and un-initialize AMSI if we're the last runspace to exit or we are exiting the primary runspace.
            if (!haveOpenRunspaces)
                ExecutionContext executionContext = this.GetExecutionContext;
                    PSHostUserInterface hostUI = executionContext.EngineHostInterface.UI;
                    hostUI?.StopAllTranscribing();
                AmsiUtils.Uninitialize();
            // Generate the shutdown event
            Events?.GenerateEvent(PSEngineEvent.Exiting, null, Array.Empty<object>(), null, true, false);
            // Stop all running pipelines
            // Note:Do not perform the Cancel in lock. Reason is
            // Pipeline executes in separate thread, say threadP.
            // When pipeline is canceled/failed/completed in
            // Pipeline.ExecuteThreadProc it removes the pipeline
            // from the list of running pipelines. threadP will need
            // lock to remove the pipelines from the list of running pipelines
            // And we will deadlock.
            // Note:It is possible that one or more pipelines in the list
            // of active pipelines have completed before we call cancel.
            // That is fine since Pipeline.Cancel handles that( It ignores
            // the cancel request if pipeline execution has already
            // completed/failed/canceled.
            StopPipelines();
            // Disconnect all disconnectable jobs in the job repository.
            StopOrDisconnectAllJobs();
            // Close or disconnect all the remote runspaces available in the
            // runspace repository.
            CloseOrDisconnectAllRemoteRunspaces(() =>
                    List<RemoteRunspace> runspaces = new List<RemoteRunspace>();
                    foreach (PSSession psSession in this.RunspaceRepository.Runspaces)
                        runspaces.Add(psSession.Runspace as RemoteRunspace);
                    return runspaces;
            // Notify Engine components that runspace is closing.
            _engine.Context.RunspaceClosingNotification();
            // Log engine lifecycle event.
            // All pipelines have been canceled. Close the runspace.
            _engine = null;
            SetRunspaceState(RunspaceState.Closed);
            // Raise Event
            if (closeAllOpenRunspaces)
                        runspace.Dispose();
            // Report telemetry if we have no more open runspaces.
            bool allRunspacesClosed = true;
            bool hostProvidesExitTelemetry = false;
            foreach (var r in Runspace.RunspaceList)
                if (r.RunspaceStateInfo.State != RunspaceState.Closed)
                    allRunspacesClosed = false;
                var localRunspace = r as LocalRunspace;
                if (localRunspace != null && localRunspace.Host is IHostProvidesTelemetryData)
                    hostProvidesExitTelemetry = true;
            if (allRunspacesClosed && !hostProvidesExitTelemetry)
                TelemetryAPI.ReportExitTelemetry(null);
        /// Closes or disconnects all the remote runspaces passed in by the getRunspace
        /// function.  If a remote runspace supports disconnect then it will be disconnected
        /// rather than closed.
        private static void CloseOrDisconnectAllRemoteRunspaces(Func<List<RemoteRunspace>> getRunspaces)
            List<RemoteRunspace> runspaces = getRunspaces();
            if (runspaces.Count == 0)
            // whether the close of all remoterunspaces completed
            using (ManualResetEvent remoteRunspaceCloseCompleted = new ManualResetEvent(false))
                ThrottleManager throttleManager = new ThrottleManager();
                throttleManager.ThrottleComplete += (object sender, EventArgs e) => remoteRunspaceCloseCompleted.Set();
                foreach (RemoteRunspace remoteRunspace in runspaces)
                    IThrottleOperation operation = new CloseOrDisconnectRunspaceOperationHelper(remoteRunspace);
                throttleManager.EndSubmitOperations();
                remoteRunspaceCloseCompleted.WaitOne();
        /// Disconnects all disconnectable jobs listed in the JobRepository.
        private void StopOrDisconnectAllJobs()
            if (JobRepository.Jobs.Count == 0)
            List<RemoteRunspace> disconnectRunspaces = new List<RemoteRunspace>();
            using (ManualResetEvent jobsStopCompleted = new ManualResetEvent(false))
                throttleManager.ThrottleComplete += (object sender, EventArgs e) => jobsStopCompleted.Set();
                foreach (Job job in this.JobRepository.Jobs)
                    // Only stop or disconnect PowerShell jobs.
                    if (job is not PSRemotingJob)
                    if (!job.CanDisconnect)
                        // If the job cannot be disconnected then add it to
                        // the stop list.
                        throttleManager.AddOperation(new StopJobOperationHelper(job));
                    else if (job.JobStateInfo.State == JobState.Running)
                        // Otherwise add disconnectable runspaces to list so that
                        // they can be disconnected.
                        IEnumerable<RemoteRunspace> jobRunspaces = job.GetRunspaces();
                        if (jobRunspaces != null)
                            disconnectRunspaces.AddRange(jobRunspaces);
                // Stop jobs.
                jobsStopCompleted.WaitOne();
            // Disconnect all disconnectable job runspaces found.
            CloseOrDisconnectAllRemoteRunspaces(() => disconnectRunspaces);
        internal void ReleaseDebugger()
            Debugger debugger = Debugger;
                    if (debugger.UnhandledBreakpointMode == UnhandledBreakpointProcessingMode.Wait)
                        // Sets the mode and also releases a held debug stop.
        protected override void DoSetVariable(string name, object value)
            _engine.Context.EngineSessionState.SetVariableValue(name, value, CommandOrigin.Internal);
        protected override object DoGetVariable(string name)
            return _engine.Context.EngineSessionState.GetVariableValue(name);
        protected override List<string> DoApplications
                return _engine.Context.EngineSessionState.Applications;
        protected override List<string> DoScripts
                return _engine.Context.EngineSessionState.Scripts;
        protected override DriveManagementIntrinsics DoDrive
                return _engine.Context.SessionState.Drive;
        protected override PSLanguageMode DoLanguageMode
                return _engine.Context.SessionState.LanguageMode;
                _engine.Context.SessionState.LanguageMode = value;
        protected override PSModuleInfo DoModule
                return _engine.Context.EngineSessionState.Module;
        protected override PathIntrinsics DoPath
                return _engine.Context.SessionState.Path;
        protected override CmdletProviderManagementIntrinsics DoProvider
                return _engine.Context.SessionState.Provider;
        protected override PSVariableIntrinsics DoPSVariable
                return _engine.Context.SessionState.PSVariable;
        protected override CommandInvocationIntrinsics DoInvokeCommand
                return _engine.Context.EngineIntrinsics.InvokeCommand;
        protected override ProviderIntrinsics DoInvokeProvider
                return _engine.Context.EngineIntrinsics.InvokeProvider;
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "pipelineThread", Justification = "pipelineThread is disposed in Close()")]
                    _history = null;
                    _transcriptionData = null;
                    _jobManager = null;
                    _jobRepository = null;
                    _runspaceRepository = null;
                    if (RunspaceOpening != null)
                        RunspaceOpening.Dispose();
                        RunspaceOpening = null;
                    // Dispose the event manager
                    if (this.ExecutionContext != null && this.ExecutionContext.Events != null)
                            this.ExecutionContext.Events.Dispose();
            // Do not put cleanup activities in here, as they aren't
            // captured in CloseAsync() case. Instead, put them in
            // DoCloseHelper()
            base.Close(); // call base.Close() first to make it stop the pipeline
            _pipelineThread?.Close();
        /// AutomationEngine instance for this runspace.
        private AutomationEngine _engine;
        internal AutomationEngine Engine
                return _engine;
        /// Manages history for this runspace.
        [TraceSource("RunspaceInit", "Initialization code for Runspace")]
            PSTraceSource.GetTracer("RunspaceInit", "Initialization code for Runspace", false);
        /// This ensures all processes have a server/listener.
        private static readonly RemoteSessionNamedPipeServer s_IPCNamedPipeServer = RemoteSessionNamedPipeServer.IPCNamedPipeServer;
    #region Helper Class
    /// Helper class to stop a running job.
    internal sealed class StopJobOperationHelper : IThrottleOperation
        private readonly Job _job;
        /// Internal constructor.
        /// <param name="job">Job object to stop.</param>
        internal StopJobOperationHelper(Job job)
            _job = job;
            _job.StateChanged += HandleJobStateChanged;
        /// Handles the Job state change event.
        /// <param name="sender">Originator of event, unused.</param>
        /// <param name="eventArgs">Event arguments containing Job state.</param>
        private void HandleJobStateChanged(object sender, JobStateEventArgs eventArgs)
            if (_job.IsFinishedState(_job.JobStateInfo.State))
                // We are done when the job is in the finished state.
                RaiseOperationCompleteEvent();
        /// Override method to start the operation.
                // The job is already in the finished state and so cannot be stopped.
                // Otherwise stop the job.
                _job.StopJob();
        /// Override method to stop the operation.  Not used, stop operation must
        /// run to completion.
        /// Event to signal ThrottleManager when the operation is complete.
        /// Raise the OperationComplete event.
        private void RaiseOperationCompleteEvent()
            _job.StateChanged -= HandleJobStateChanged;
            OperationStateEventArgs operationStateArgs = new OperationStateEventArgs();
            operationStateArgs.OperationState = OperationState.StartComplete;
            operationStateArgs.BaseEvent = EventArgs.Empty;
            OperationComplete.SafeInvoke(this, operationStateArgs);
    /// Helper class to disconnect a runspace if the runspace supports disconnect
    /// semantics or otherwise close the runspace.
    internal sealed class CloseOrDisconnectRunspaceOperationHelper : IThrottleOperation
        private readonly RemoteRunspace _remoteRunspace;
        /// <param name="remoteRunspace"></param>
        internal CloseOrDisconnectRunspaceOperationHelper(RemoteRunspace remoteRunspace)
            _remoteRunspace = remoteRunspace;
            _remoteRunspace.StateChanged += HandleRunspaceStateChanged;
        /// Handle the runspace state changed event.
        /// <param name="sender">Sender of this information, unused.</param>
        /// <param name="eventArgs">Runspace event args.</param>
        private void HandleRunspaceStateChanged(object sender, RunspaceStateEventArgs eventArgs)
            switch (eventArgs.RunspaceStateInfo.State)
                case RunspaceState.BeforeOpen:
                case RunspaceState.Disconnecting:
            // remoteRunspace.Dispose();
            // remoteRunspace = null;
        /// Start the operation of closing the runspace.
            if (_remoteRunspace.RunspaceStateInfo.State == RunspaceState.Closed ||
                _remoteRunspace.RunspaceStateInfo.State == RunspaceState.Broken ||
                _remoteRunspace.RunspaceStateInfo.State == RunspaceState.Disconnected)
                // If the runspace is currently in a disconnected state then leave it
                // as is.
                // in this case, calling a close won't raise any events. Simply raise
                // the OperationCompleted event. After the if check, but before we
                // get to this point if the state was changed, then the StateChanged
                // event handler will anyway raise the event and so we are fine
                // If the runspace supports disconnect semantics and is running a command,
                // then disconnect it rather than closing it.
                if (_remoteRunspace.CanDisconnect &&
                    _remoteRunspace.GetCurrentlyRunningPipeline() != null)
                    _remoteRunspace.DisconnectAsync();
                    _remoteRunspace.CloseAsync();
        /// There is no scenario where we are going to cancel this close
        /// Hence this method is intentionally empty.
        /// Event raised when the required operation is complete.
        /// Raise the operation completed event.
            _remoteRunspace.StateChanged -= HandleRunspaceStateChanged;
            OperationStateEventArgs operationStateEventArgs =
                    new OperationStateEventArgs();
            operationStateEventArgs.OperationState =
                    OperationState.StartComplete;
            operationStateEventArgs.BaseEvent = EventArgs.Empty;
    /// Defines the exception thrown an error loading modules occurs while opening the runspace. It
    /// contains a list of all of the module errors that have occurred.
    public class RunspaceOpenModuleLoadException : RuntimeException
        /// Initializes a new instance of ScriptBlockToPowerShellNotSupportedException
        /// with the message set to typeof(ScriptBlockToPowerShellNotSupportedException).FullName.
        public RunspaceOpenModuleLoadException()
            : base(typeof(ScriptBlockToPowerShellNotSupportedException).FullName)
        /// Initializes a new instance of ScriptBlockToPowerShellNotSupportedException setting the message.
        public RunspaceOpenModuleLoadException(string message)
        /// Initializes a new instance of ScriptBlockToPowerShellNotSupportedException setting the message and innerException.
        public RunspaceOpenModuleLoadException(string message, Exception innerException)
        /// <param name="moduleName">The name of the module that cause the error.</param>
        /// <param name="errors">The collection of errors that occurred during module processing.</param>
        internal RunspaceOpenModuleLoadException(
            PSDataCollection<ErrorRecord> errors)
            : base(StringUtil.Format(RunspaceStrings.ErrorLoadingModulesOnRunspaceOpen, moduleName,
                (errors != null && errors.Count > 0 && errors[0] != null) ? errors[0].ToString() : string.Empty), null)
            _errors = errors;
            this.SetErrorId("ErrorLoadingModulesOnRunspaceOpen");
            this.SetErrorCategory(ErrorCategory.OpenError);
        /// The collection of error records generated while loading the modules.
        public PSDataCollection<ErrorRecord> ErrorRecords
            get { return _errors; }
        private readonly PSDataCollection<ErrorRecord> _errors;
        /// Initializes a new instance of RunspaceOpenModuleLoadException with serialization parameters.
        protected RunspaceOpenModuleLoadException(SerializationInfo info, StreamingContext context)
    #endregion Helper Class

    /// This file implements the upper layer of the Monad Logging architecture.
    /// Lower layer of Msh Log architecture is implemented in LogProvider.cs file.
    /// Logging Api is made up of following five sets
    ///   1. Engine Health Event
    ///   2. Engine Lifecycle Event
    ///   3. Command Lifecycle Event
    ///   4. Provider Lifecycle Event
    ///   5. Settings Event
    /// Msh Log Engine provides features in following areas,
    ///   1. Loading and managing logging providers. Based on some "Provider Catalog", engine will try to
    ///      load providers. First provider that is successfully loaded will be used for low level logging.
    ///      If no providers can be loaded, a dummy provider will be used, which will essentially do nothing.
    ///   2. Implementation of logging api functions. These api functions is implemented by calling corresponding
    ///      functions in provider interface.
    ///   3. Sequence Id Generation. Unique id are generated in this class. These id's will be attached to events.
    ///   4. Monad engine state management. Engine state is stored in ExecutionContext class but managed here.
    ///      Later on, this feature may be moved to engine itself (where it should belongs to) when sophisticated
    ///      engine state model is established.
    ///   5. Logging policy support. Events are logged or not logged based on logging policy settings (which is stored
    ///      in session state of the engine.
    /// MshLog class is defined as a static class. This essentially make the logging api to be a static api.
    /// We want to provide sufficient synchronization for static functions calls.
    /// This is not needed for now because of following two reasons,
    ///     a. Currently, only one monad engine can be running in one process. So logically only one
    ///        event will be log at a time.
    ///     b. Even in the case of multiple events are logged, underlining logging media should
    ///        provide synchronization.
    internal static class MshLog
        /// A static dictionary to keep track of log providers for different shellId's.
        /// The value of this dictionary is never empty. A value of type DummyProvider means
        /// no logging.
        private static readonly ConcurrentDictionary<string, Collection<LogProvider>> s_logProviders =
            new ConcurrentDictionary<string, Collection<LogProvider>>();
        private const string _crimsonLogProviderAssemblyName = "MshCrimsonLog";
        private const string _crimsonLogProviderTypeName = "System.Management.Automation.Logging.CrimsonLogProvider";
        private static readonly Collection<string> s_ignoredCommands = new Collection<string>();
        static MshLog()
            s_ignoredCommands.Add("Out-Lineoutput");
            s_ignoredCommands.Add("Format-Default");
        /// Currently initialization is done in following sequence
        ///    a. Try to load CrimsonLogProvider (in the case of Longhorn)
        ///    b. If a fails, use the DummyLogProvider instead. (in low-level OS)
        /// In the longer turn, we may need to use a "Provider Catalog" for
        /// log provider loading.
        private static IEnumerable<LogProvider> GetLogProvider(string shellId)
            return s_logProviders.GetOrAdd(shellId, CreateLogProvider);
        /// Get Log Provider based on Execution Context.
        private static IEnumerable<LogProvider> GetLogProvider(ExecutionContext executionContext)
            if (executionContext == null)
                throw PSTraceSource.NewArgumentNullException(nameof(executionContext));
            string shellId = executionContext.ShellID;
            return GetLogProvider(shellId);
        /// Get Log Provider based on Log Context.
        private static IEnumerable<LogProvider> GetLogProvider(LogContext logContext)
            System.Diagnostics.Debug.Assert(logContext != null);
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(logContext.ShellId));
            return GetLogProvider(logContext.ShellId);
        /// Create a log provider based on a shell Id.
        private static Collection<LogProvider> CreateLogProvider(string shellId)
            Collection<LogProvider> providers = new Collection<LogProvider>();
            // Porting note: Linux does not support ETW
                LogProvider sysLogProvider = new PSSysLogProvider();
                providers.Add(sysLogProvider);
                LogProvider etwLogProvider = new PSEtwLogProvider();
                providers.Add(etwLogProvider);
                return providers;
                // This exception will happen if we try to create an event source
                // (corresponding to the current running minishell)
                // when running as non-admin user. In that case, we will default
                // to dummy log.
            providers.Add(new DummyLogProvider());
        /// This will set the current log provider to be dummy log.
        internal static void SetDummyLog(string shellId)
            Collection<LogProvider> providers = new Collection<LogProvider> { new DummyLogProvider() };
            s_logProviders.AddOrUpdate(shellId, providers, (key, value) => providers);
        #region Engine Health Event Logging Api
        /// LogEngineHealthEvent: Log an engine health event. If engine state is changed, a engine
        /// lifecycle event will be logged also.
        /// This is the basic form of EngineHealthEvent logging api, in which all parameters are provided.
        /// Variant form of this function is defined below, which will make parameters additionalInfo
        /// and newEngineState optional.
        /// <param name="executionContext">Execution context for the engine that is running.</param>
        /// <param name="eventId">EventId for the event to be logged.</param>
        /// <param name="exception">Exception associated with this event.</param>
        /// <param name="severity">Severity of this event.</param>
        /// <param name="additionalInfo">Additional information for this event.</param>
        /// <param name="newEngineState">New engine state.</param>
        internal static void LogEngineHealthEvent(ExecutionContext executionContext,
                                                int eventId,
                                                Dictionary<string, string> additionalInfo,
                                                EngineState newEngineState)
                PSTraceSource.NewArgumentNullException(nameof(executionContext));
                PSTraceSource.NewArgumentNullException(nameof(exception));
            InvocationInfo invocationInfo = null;
            if (exception is IContainsErrorRecord icer && icer.ErrorRecord != null)
                invocationInfo = icer.ErrorRecord.InvocationInfo;
            foreach (LogProvider provider in GetLogProvider(executionContext))
                if (NeedToLogEngineHealthEvent(provider, executionContext))
                    provider.LogEngineHealthEvent(GetLogContext(executionContext, invocationInfo, severity), eventId, exception, additionalInfo);
            if (newEngineState != EngineState.None)
                LogEngineLifecycleEvent(executionContext, newEngineState, invocationInfo);
        /// This is a variation of LogEngineHealthEvent api to make additionalInfo and newEngineState
        /// optional.
        /// <param name="severity"></param>
                                                Severity severity)
            LogEngineHealthEvent(executionContext, eventId, exception, severity, null);
        /// This is a variation of LogEngineHealthEvent api to make eventid, additionalInfo and newEngineState
        /// A default event id for engine health event will be used.
            LogEngineHealthEvent(executionContext, 100, exception, severity, null);
        /// This is a variation of LogEngineHealthEvent api to make newEngineState
            LogEngineHealthEvent(executionContext, eventId, exception, severity, additionalInfo, EngineState.None);
        /// This is a variation of LogEngineHealthEvent api to make additionalInfo
        /// <param name="newEngineState"></param>
            LogEngineHealthEvent(executionContext, eventId, exception, severity, null, newEngineState);
        /// LogEngineHealthEvent: This is an API for logging engine health event while execution context
        /// is not available. In this case, caller of this API will directly construct LogContext
        /// This API is currently used only by runspace before engine start.
        /// <param name="logContext">LogContext to be.</param>
        internal static void LogEngineHealthEvent(LogContext logContext,
                                                Dictionary<string, string> additionalInfo
            if (logContext == null)
                PSTraceSource.NewArgumentNullException(nameof(logContext));
            // Here execution context doesn't exist, we will have to log this event regardless.
            // Don't check NeedToLogEngineHealthEvent here.
            foreach (LogProvider provider in GetLogProvider(logContext))
                provider.LogEngineHealthEvent(logContext, eventId, exception, additionalInfo);
        #region Engine Lifecycle Event Logging Api
        /// LogEngineLifecycleEvent: Log an engine lifecycle event.
        /// This is the basic form of EngineLifecycleEvent logging api, in which all parameters are provided.
        /// Variant form of this function is defined below, which will make parameter additionalInfo
        /// <param name="executionContext">Execution context for current engine instance.</param>
        /// <param name="engineState">New engine state.</param>
        /// <param name="invocationInfo">InvocationInfo for current command that is running.</param>
        internal static void LogEngineLifecycleEvent(ExecutionContext executionContext,
                                                EngineState engineState,
            EngineState previousState = GetEngineState(executionContext);
            if (engineState == previousState)
                if (NeedToLogEngineLifecycleEvent(provider, executionContext))
                    provider.LogEngineLifecycleEvent(GetLogContext(executionContext, invocationInfo), engineState, previousState);
            SetEngineState(executionContext, engineState);
        /// This is a variation of basic LogEngineLifeCycleEvent api which makes invocationInfo
        /// <param name="engineState"></param>
                                                EngineState engineState)
            LogEngineLifecycleEvent(executionContext, engineState, null);
        #region Command Health Event Logging Api
        /// LogProviderHealthEvent: Log a command health event.
        internal static void LogCommandHealthEvent(ExecutionContext executionContext,
                                                Severity severity
                if (NeedToLogCommandHealthEvent(provider, executionContext))
                    provider.LogCommandHealthEvent(GetLogContext(executionContext, invocationInfo, severity), exception);
        #region Command Lifecycle Event Logging Api
        /// LogCommandLifecycleEvent: Log a command lifecycle event.
        /// This is the only form of CommandLifecycleEvent logging api.
        /// <param name="executionContext">Execution Context for the current running engine.</param>
        /// <param name="commandState">New command state.</param>
        /// <param name="invocationInfo">Invocation data for current command that is running.</param>
        internal static void LogCommandLifecycleEvent(ExecutionContext executionContext,
                                                CommandState commandState,
                PSTraceSource.NewArgumentNullException(nameof(invocationInfo));
            if (s_ignoredCommands.Contains(invocationInfo.MyCommand.Name))
            LogContext logContext = null;
                if (NeedToLogCommandLifecycleEvent(provider, executionContext))
                    provider.LogCommandLifecycleEvent(
                        () => logContext ??= GetLogContext(executionContext, invocationInfo),
                        commandState);
        /// This is a form of CommandLifecycleEvent which takes a commandName instead
        /// of invocationInfo. It is likely that invocationInfo is not available if
        /// the command failed security check.
        /// <param name="commandName">Current command that is running.</param>
                                logContext = GetLogContext(executionContext, null);
                                logContext.CommandName = commandName;
                            return logContext;
                        }, commandState);
        #region Pipeline Execution Detail Event Logging Api
        /// LogPipelineExecutionDetailEvent: Log a pipeline execution detail event.
        /// <param name="detail">Detail to be logged for this pipeline execution detail.</param>
        internal static void LogPipelineExecutionDetailEvent(ExecutionContext executionContext,
                                                            List<string> detail,
                if (NeedToLogPipelineExecutionDetailEvent(provider, executionContext))
                    provider.LogPipelineExecutionDetailEvent(GetLogContext(executionContext, invocationInfo), detail);
        /// This is a form of PipelineExecutionDetailEvent which takes a scriptName and commandLine
        /// instead of invocationInfo. This will save the need to fill in the commandName for
        /// this event.
        /// <param name="scriptName">Script that is currently running.</param>
        /// <param name="commandLine">Command line that is currently running.</param>
                                                            string commandLine)
            LogContext logContext = GetLogContext(executionContext, null);
            logContext.CommandLine = commandLine;
            logContext.ScriptName = scriptName;
                    provider.LogPipelineExecutionDetailEvent(logContext, detail);
        #region Provider Health Event Logging Api
        /// LogProviderHealthEvent: Log a Provider health event.
        /// <param name="providerName">Name of the provider.</param>
        internal static void LogProviderHealthEvent(ExecutionContext executionContext,
                if (NeedToLogProviderHealthEvent(provider, executionContext))
                    provider.LogProviderHealthEvent(GetLogContext(executionContext, invocationInfo, severity), providerName, exception);
        #region Provider Lifecycle Event Logging Api
        /// LogProviderLifecycleEvent: Log a provider lifecycle event.
        /// This is the only form of ProviderLifecycleEvent logging api.
        /// <param name="executionContext">Execution Context for current engine that is running.</param>
        /// <param name="providerName">Provider name.</param>
        /// <param name="providerState">New provider state.</param>
        internal static void LogProviderLifecycleEvent(ExecutionContext executionContext,
                                                     ProviderState providerState)
                if (NeedToLogProviderLifecycleEvent(provider, executionContext))
                    provider.LogProviderLifecycleEvent(GetLogContext(executionContext, null), providerName, providerState);
        #region Settings Event Logging Api
        /// LogSettingsEvent: Log a settings event
        /// This is the basic form of LoggingSettingsEvent API. Variation of this function defined
        /// below will make parameter invocationInfo optional.
        /// <param name="executionContext">Execution context for current running engine.</param>
        /// <param name="variableName">Variable name.</param>
        /// <param name="newValue">New value for the variable.</param>
        /// <param name="previousValue">Previous value for the variable.</param>
        /// <param name="invocationInfo">Invocation data for the command that is currently running.</param>
        internal static void LogSettingsEvent(ExecutionContext executionContext,
                                            string newValue,
                                            string previousValue,
                if (NeedToLogSettingsEvent(provider, executionContext))
                    provider.LogSettingsEvent(GetLogContext(executionContext, invocationInfo), variableName, newValue, previousValue);
        /// This is a variation of basic LogSettingsEvent to make "invocationInfo" optional.
        /// <param name="newValue"></param>
                                            string previousValue)
            LogSettingsEvent(executionContext, variableName, newValue, previousValue, null);
        #region Helper Functions
        /// Get current engine state for the engine instance corresponding to executionContext
        /// passed in.
        /// Engine state is stored in ExecutionContext.
        private static EngineState GetEngineState(ExecutionContext executionContext)
            return executionContext.EngineState;
        /// Set current engine state for the engine instance corresponding to executionContext
        private static void SetEngineState(ExecutionContext executionContext, EngineState engineState)
            executionContext.EngineState = engineState;
        /// Generate LogContext structure based on executionContext and invocationInfo passed in.
        /// LogContext structure is used in log provider interface.
        internal static LogContext GetLogContext(ExecutionContext executionContext, InvocationInfo invocationInfo)
            return GetLogContext(executionContext, invocationInfo, Severity.Informational);
        private static LogContext GetLogContext(ExecutionContext executionContext, InvocationInfo invocationInfo, Severity severity)
            logContext.ExecutionContext = executionContext;
            logContext.ShellId = shellId;
            if (executionContext.EngineHostInterface != null)
                logContext.HostName = executionContext.EngineHostInterface.Name;
                logContext.HostVersion = executionContext.EngineHostInterface.Version.ToString();
                logContext.HostId = (string)executionContext.EngineHostInterface.InstanceId.ToString();
            logContext.HostApplication = string.Join(' ', Environment.GetCommandLineArgs());
            if (executionContext.CurrentRunspace != null)
                logContext.EngineVersion = executionContext.CurrentRunspace.Version.ToString();
                logContext.RunspaceId = executionContext.CurrentRunspace.InstanceId.ToString();
                Pipeline currentPipeline = ((RunspaceBase)executionContext.CurrentRunspace).GetCurrentlyRunningPipeline();
                if (currentPipeline != null)
                    logContext.PipelineId = currentPipeline.InstanceId.ToString(CultureInfo.CurrentCulture);
            logContext.SequenceNumber = NextSequenceNumber;
                if (executionContext.LogContextCache.User == null)
                    logContext.User = Environment.UserDomainName + "\\" + Environment.UserName;
                    executionContext.LogContextCache.User = logContext.User;
                    logContext.User = executionContext.LogContextCache.User;
                logContext.User = Logging.UnknownUserName;
            if (executionContext.SessionState.PSVariable.GetValue("PSSenderInfo") is System.Management.Automation.Remoting.PSSenderInfo psSenderInfo)
                logContext.ConnectedUser = psSenderInfo.UserInfo.Identity.Name;
            logContext.Time = DateTime.Now.ToString(CultureInfo.CurrentCulture);
            logContext.ScriptName = invocationInfo.ScriptName;
            logContext.CommandLine = invocationInfo.Line;
                logContext.CommandName = invocationInfo.MyCommand.Name;
                logContext.CommandType = invocationInfo.MyCommand.CommandType.ToString();
                switch (invocationInfo.MyCommand.CommandType)
                        logContext.CommandPath = ((ApplicationInfo)invocationInfo.MyCommand).Path;
                        logContext.CommandPath = ((ExternalScriptInfo)invocationInfo.MyCommand).Path;
        #region Logging Policy
        /// NeedToLogEngineHealthEvent: check whether logging engine health event is necessary.
        ///     Whether to log engine event is controled by session variable "LogEngineHealthEvent"
        ///     The default value for this is true (?).
        /// Reading a session variable from execution context for
        /// every single logging call may be expensive. We may need to use a different
        /// approach for this:
        ///     a. ExecutionContext will cache the value for variable "LogEngineHealthEvent"
        ///     b. If this variable is changed, a notification function will change the cached
        ///        value in engine correspondently.
        /// This applies to other logging preference variable also.
        /// <param name="logProvider"></param>
        private static bool NeedToLogEngineHealthEvent(LogProvider logProvider, ExecutionContext executionContext)
            if (!logProvider.UseLoggingVariables())
            return LanguagePrimitives.IsTrue(executionContext.GetVariableValue(SpecialVariables.LogEngineHealthEventVarPath, true));
        /// NeedToLogEngineLifecycleEvent: check whether logging engine lifecycle event is necessary.
        ///     Whether to log engine lifecycle event is controled by session variable "LogEngineLifecycleEvent"
        ///     The default value for this is false (?).
        private static bool NeedToLogEngineLifecycleEvent(LogProvider logProvider, ExecutionContext executionContext)
            return LanguagePrimitives.IsTrue(executionContext.GetVariableValue(SpecialVariables.LogEngineLifecycleEventVarPath, true));
        /// NeedToLogCommandHealthEvent: check whether logging command health event is necessary.
        ///     Whether to log command health event is controled by session variable "LogCommandHealthEvent"
        private static bool NeedToLogCommandHealthEvent(LogProvider logProvider, ExecutionContext executionContext)
            return LanguagePrimitives.IsTrue(executionContext.GetVariableValue(SpecialVariables.LogCommandHealthEventVarPath, false));
        /// NeedToLogCommandLifecycleEvent: check whether logging command event is necessary.
        ///     Whether to log command lifecycle event is controled by session variable "LogCommandLifecycleEvent"
        private static bool NeedToLogCommandLifecycleEvent(LogProvider logProvider, ExecutionContext executionContext)
            return LanguagePrimitives.IsTrue(executionContext.GetVariableValue(SpecialVariables.LogCommandLifecycleEventVarPath, false));
        /// NeedToLogPipelineExecutionDetailEvent: check whether logging pipeline execution detail event is necessary.
        /// Whether to log command lifecycle event is controled by PSSnapin set up.
        /// Should we use session variable "LogPipelineExecutionEvent" to control this also?
        /// Currently we return true always since pipeline processor already check for whether to log
        /// logic from PSSnapin already. This may need to be changed.
        private static bool NeedToLogPipelineExecutionDetailEvent(LogProvider logProvider, ExecutionContext executionContext)
            // return LanguagePrimitives.IsTrue(executionContext.GetVariable("LogPipelineExecutionDetailEvent", false));
        /// NeedToLogProviderHealthEvent: check whether logging Provider health event is necessary.
        ///     Whether to log Provider health event is controled by session variable "LogProviderHealthEvent"
        ///     The default value for this is true.
        private static bool NeedToLogProviderHealthEvent(LogProvider logProvider, ExecutionContext executionContext)
            return LanguagePrimitives.IsTrue(executionContext.GetVariableValue(SpecialVariables.LogProviderHealthEventVarPath, true));
        /// NeedToLogProviderLifecycleEvent: check whether logging Provider lifecycle event is necessary.
        ///     Whether to log Provider lifecycle event is controled by session variable "LogProviderLifecycleEvent"
        private static bool NeedToLogProviderLifecycleEvent(LogProvider logProvider, ExecutionContext executionContext)
            return LanguagePrimitives.IsTrue(executionContext.GetVariableValue(SpecialVariables.LogProviderLifecycleEventVarPath, true));
        /// NeedToLogSettingsEvent: check whether logging settings event is necessary.
        ///     Whether to log settings event is controled by session variable "LogSettingsEvent"
        private static bool NeedToLogSettingsEvent(LogProvider logProvider, ExecutionContext executionContext)
            return LanguagePrimitives.IsTrue(executionContext.GetVariableValue(SpecialVariables.LogSettingsEventVarPath, true));
        #region Sequence Id Generator
        private static int s_nextSequenceNumber = 0;
        /// Generate next sequence id to be attached to current event.
        private static string NextSequenceNumber
                return Convert.ToString(Interlocked.Increment(ref s_nextSequenceNumber), CultureInfo.CurrentCulture);
        #region EventId Constants
        // General health issues.
        internal const int EVENT_ID_GENERAL_HEALTH_ISSUE = 100;
        // Dependency. resource not available
        internal const int EVENT_ID_RESOURCE_NOT_AVAILABLE = 101;
        // Connectivity. network connection failure
        internal const int EVENT_ID_NETWORK_CONNECTIVITY_ISSUE = 102;
        // Settings. fail to set some configuration settings
        internal const int EVENT_ID_CONFIGURATION_FAILURE = 103;
        // Performance. system is experiencing some performance issues
        internal const int EVENT_ID_PERFORMANCE_ISSUE = 104;
        // Security: system is experiencing some security issues
        internal const int EVENT_ID_SECURITY_ISSUE = 105;
        // Workload. system is overloaded.
        internal const int EVENT_ID_SYSTEM_OVERLOADED = 106;
        // Beta 1 only -- Unexpected Exception
        internal const int EVENT_ID_UNEXPECTED_EXCEPTION = 195;
        #endregion EventId Constants
    internal class LogContextCache
        internal string User { get; set; } = null;
    #region Command State and Provider State
    /// Severity of the event.
    internal enum Severity
        /// Undefined severity.
        /// Critical event causing engine not to work.
        Critical,
        /// Error causing engine partially work.
        /// Problem that may not cause an immediate problem.
        /// Informational.
        Informational
    /// Enum for command states.
    internal enum CommandState
        Started = 0,
        Stopped = 1,
        Terminated = 2
    /// Enum for provider states.
    internal enum ProviderState

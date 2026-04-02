    // pragma warning disable 16001,16003
    public enum PowerShellTraceEvent : int
        /// None. (Should not be used)
        /// HostNameResolve.
        /// SchemeResolve.
        /// ShellResolve.
        /// RunspaceConstructor.
        /// RunspacePoolConstructor.
        /// RunspacePoolOpen.
        /// OperationalTransferEventRunspacePool.
        /// RunspacePort.
        RunspacePort = 0x2F01,
        /// AppName.
        /// ComputerName.
        /// Scheme.
        /// TestAnalytic.
        /// WSManConnectionInfoDump.
        /// AnalyticTransferEventRunspacePool.
        /// TransportReceivedObject.
        /// AppDomainUnhandledExceptionAnalytic.
        AppDomainUnhandledExceptionAnalytic = 0x8007,
        /// TransportErrorAnalytic.
        TransportErrorAnalytic = 0x8008,
        /// AppDomainUnhandledException.
        /// TransportError.
        /// WSManCreateShell.
        /// WSManCreateShellCallbackReceived.
        /// WSManCloseShell.
        /// WSManCloseShellCallbackReceived.
        /// WSManSendShellInputExtended.
        WSManSendShellInputExtended = 0x8015,
        /// WSManSendShellInputExCallbackReceived.
        WSManSendShellInputExtendedCallbackReceived = 0x8016,
        /// WSManReceiveShellOutputExtended.
        WSManReceiveShellOutputExtended = 0x8017,
        /// WSManReceiveShellOutputExCallbackReceived.
        WSManReceiveShellOutputExtendedCallbackReceived = 0x8018,
        /// WSManCreateCommand.
        /// WSManCreateCommandCallbackReceived.
        /// WSManCloseCommand.
        /// WSManCloseCommandCallbackReceived.
        /// WSManSignal.
        /// WSManSignalCallbackReceived.
        /// UriRedirection.
        UriRedirection = 0x8025,
        /// ServerSendData.
        /// ServerCreateRemoteSession.
        /// ReportContext.
        /// ReportOperationComplete.
        /// ServerCreateCommandSession.
        /// ServerStopCommand.
        /// ServerReceivedData.
        /// ServerClientReceiveRequest.
        /// ServerCloseOperation.
        /// LoadingPSCustomShellAssembly.
        /// LoadingPSCustomShellType.
        /// ReceivedRemotingFragment.
        /// SentRemotingFragment.
        /// WSManPluginShutdown.
        /// SerializerWorkflowLoadSuccess.
        SerializerWorkflowLoadSuccess = 0x7001,
        /// SerializerWorkflowLoadFailure.
        SerializerWorkflowLoadFailure = 0x7002,
        /// SerializerDepthOverride.
        SerializerDepthOverride = 0x7003,
        /// SerializerModeOverride.
        SerializerModeOverride = 0x7004,
        /// SerializerScriptPropertyWithoutRunspace.
        SerializerScriptPropertyWithoutRunspace = 0x7005,
        /// SerializerPropertyGetterFailed.
        SerializerPropertyGetterFailed = 0x7006,
        /// SerializerEnumerationFailed.
        SerializerEnumerationFailed = 0x7007,
        /// SerializerToStringFailed.
        SerializerToStringFailed = 0x7008,
        /// SerializerMaxDepthWhenSerializing.
        SerializerMaxDepthWhenSerializing = 0x700A,
        /// SerializerXmlExceptionWhenDeserializing.
        SerializerXmlExceptionWhenDeserializing = 0x700B,
        /// SerializerSpecificPropertyMissing.
        SerializerSpecificPropertyMissing = 0x700C,
        // Start: PerformanceTrack related events
        /// PerformanceTrackConsoleStartupStart.
        PerformanceTrackConsoleStartupStart = 0xA001,
        /// PerformanceTrackConsoleStartupStop.
        PerformanceTrackConsoleStartupStop = 0xA002,
        ErrorRecord = 0xB001,
        Exception = 0xB002,
        /// PowerShellObject.
        PowerShellObject = 0xB003,
        /// Job.
        Job = 0xB004,
        /// Writing a simple trace message from code.
        TraceMessage = 0xB005,
        /// Trace the WSManConnectionInfo used for this connection.
        TraceWSManConnectionInfo = 0xB006,
        /// Writing a simple trace message from code with 2
        /// strings.
        TraceMessage2 = 0xC001,
        TraceMessageGuid = 0xC002,
    // pragma warning disable 16001
    public enum PowerShellTraceChannel
        /// None (No channel selected, should not be used)
        /// Operational Channel.
        /// Analytic Channel.
        Analytic = 0x11,
        /// Debug Channel.
        Debug = 0x12,
    // pragma warning restore 16001
    public enum PowerShellTraceLevel
        /// LogAlways.
        LogAlways = 0,
        /// Critical.
        Critical = 1,
        /// Error.
        /// Warning.
        Informational = 4,
        /// Verbose.
        /// Debug.
        Debug = 20,
    public enum PowerShellTraceOperationCode
        /// None.  (Should not be used)
        /// Open.
        Open = 10,
        /// Close.
        Close = 11,
        Connect = 12,
        /// Disconnect.
        Disconnect = 13,
        /// Negotiate.
        Negotiate = 14,
        Create = 15,
        Constructor = 16,
        Dispose = 17,
        /// EventHandler.
        EventHandler = 18,
        Exception = 19,
        /// Method.
        Method = 20,
        /// Send.
        Send = 21,
        /// Receive.
        Receive = 22,
        /// WorkflowLoad.
        WorkflowLoad = 23,
        /// SerializationSettings.
        SerializationSettings = 24,
        /// WinInfo.
        WinInfo,
        /// WinStart.
        WinStart,
        /// WinStop.
        WinStop,
        /// WinDCStart.
        WinDCStart,
        /// WinDCStop.
        WinDCStop,
        /// WinExtension.
        WinExtension,
        /// WinReply.
        WinReply,
        /// WinResume.
        WinResume,
        /// WinSuspend.
        WinSuspend,
    /// Defines Tasks.
    /// BaseChannelWriter is the abstract base class defines event specific methods that are used to write a trace.
    /// The default implementation does not write any message to any trace channel.
    public abstract class BaseChannelWriter : IDisposable
            if (!disposed)
        /// TraceError.
        public virtual bool TraceError(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
        /// TraceWarning.
        public virtual bool TraceWarning(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
        /// TraceInformational.
        public virtual bool TraceInformational(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
        /// TraceVerbose.
        public virtual bool TraceVerbose(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
        /// TraceDebug.
        public virtual bool TraceDebug(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
        /// TraceLogAlways.
        public virtual bool TraceLogAlways(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
        /// TraceCritical.
        public virtual bool TraceCritical(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
        public virtual PowerShellTraceKeywords Keywords
                return PowerShellTraceKeywords.None;
                PowerShellTraceKeywords powerShellTraceKeywords = value;
    /// NullWriter is the implementation of BaseChannelWriter.
    /// This implementation does not write to any trace logs.
    /// This class is singleton and exposes its only instance
    /// through the static Instance property.
    public sealed class NullWriter : BaseChannelWriter
        /// Static Instance property.
        public static BaseChannelWriter Instance { get; } = new NullWriter();
        private NullWriter()
    /// ChannelWrite is the concrete implementation of IChannelWrite.  It writes all the traces to the specified traceChannel.
    /// TraceChannel is specified in the constructor.
    /// It always uses PowerShell event provider Id.
    public sealed class PowerShellChannelWriter : BaseChannelWriter
        private readonly PowerShellTraceChannel _traceChannel;
         * Making the provider static to reduce the number of buffers needed to 1.
         * */
        private static readonly EventProvider _provider = new EventProvider(PSEtwLogProvider.ProviderGuid);
        private PowerShellTraceKeywords _keywords;
        public override PowerShellTraceKeywords Keywords
                _keywords = value;
        internal PowerShellChannelWriter(PowerShellTraceChannel traceChannel, PowerShellTraceKeywords keywords)
            _traceChannel = traceChannel;
        private bool Trace(PowerShellTraceEvent traceEvent, PowerShellTraceLevel level, PowerShellTraceOperationCode operationCode,
            PowerShellTraceTask task, params object[] args)
            EventDescriptor ed = new EventDescriptor((int)traceEvent, 1, (byte)_traceChannel, (byte)level,
                                                     (byte)operationCode, (int)task, (long)_keywords);
             * Not using locks because the _provider is thread safe itself.
             **/
                    if (args[i] == null)
                        args[i] = string.Empty;
            return _provider.WriteEvent(in ed, args);
        public override bool TraceError(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
            return Trace(traceEvent, PowerShellTraceLevel.Error, operationCode, task, args);
        public override bool TraceWarning(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
            return Trace(traceEvent, PowerShellTraceLevel.Warning, operationCode, task, args);
        public override bool TraceInformational(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
            return Trace(traceEvent, PowerShellTraceLevel.Informational, operationCode, task, args);
        public override bool TraceVerbose(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
            return Trace(traceEvent, PowerShellTraceLevel.Verbose, operationCode, task, args);
        public override bool TraceDebug(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
            // TODO: There is some error thrown by the custom debug level
            // hence Informational is being used
        public override bool TraceLogAlways(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
            return Trace(traceEvent, PowerShellTraceLevel.LogAlways, operationCode, task, args);
        public override bool TraceCritical(PowerShellTraceEvent traceEvent, PowerShellTraceOperationCode operationCode, PowerShellTraceTask task, params object[] args)
            return Trace(traceEvent, PowerShellTraceLevel.Critical, operationCode, task, args);
    /// TraceSource class gives access to the actual TraceWriter channels.
    /// Three channels are pre-defined 1) Debug 2) Analytic and 3) Operations
    /// This class also has strongly types methods that are used for easy tracing.
            if (IsEtwSupported)
                DebugChannel = new PowerShellChannelWriter(PowerShellTraceChannel.Debug,
                                                           keywords | PowerShellTraceKeywords.UseAlwaysDebug);
                AnalyticChannel = new PowerShellChannelWriter(PowerShellTraceChannel.Analytic,
                                                              keywords | PowerShellTraceKeywords.UseAlwaysAnalytic);
                OperationalChannel = new PowerShellChannelWriter(PowerShellTraceChannel.Operational,
                                                                keywords | PowerShellTraceKeywords.UseAlwaysOperational);
                this.Task = task;
                this.Keywords = keywords;
                DebugChannel = NullWriter.Instance;
                AnalyticChannel = NullWriter.Instance;
                OperationalChannel = NullWriter.Instance;
                DebugChannel.Dispose();
                AnalyticChannel.Dispose();
                OperationalChannel.Dispose();
        /// Keywords that were set through constructor when object was instantiated.
        public PowerShellTraceKeywords Keywords { get; } = PowerShellTraceKeywords.None;
        /// Task that was set through constructor.
        public PowerShellTraceTask Task { get; set; } = PowerShellTraceTask.None;
        private static bool IsEtwSupported
                return Environment.OSVersion.Version.Major >= 6;
        /// TraceErrorRecord.
        public bool TraceErrorRecord(ErrorRecord errorRecord)
                Exception exception = errorRecord.Exception;
                string innerException = "None";
                    innerException = exception.InnerException.Message;
                ErrorCategoryInfo cinfo = errorRecord.CategoryInfo;
                string message = "None";
                    message = errorRecord.ErrorDetails.Message;
                return DebugChannel.TraceError(PowerShellTraceEvent.ErrorRecord,
                                               PowerShellTraceOperationCode.Exception, PowerShellTraceTask.None,
                                               cinfo.Category.ToString(), cinfo.Reason, cinfo.TargetName,
                                               exception.Message, exception.StackTrace, innerException);
                                               "NULL errorRecord");
        /// TraceException.
                return DebugChannel.TraceError(PowerShellTraceEvent.Exception,
                                           "NULL exception");
        /// TracePowerShellObject.
        public bool TracePowerShellObject(PSObject powerShellObject)
            return this.DebugChannel.TraceDebug(PowerShellTraceEvent.PowerShellObject,
                                                PowerShellTraceOperationCode.Method, PowerShellTraceTask.None);
        /// TraceJob.
        public bool TraceJob(Job job)
                return DebugChannel.TraceDebug(PowerShellTraceEvent.Job,
                                               PowerShellTraceOperationCode.Method, PowerShellTraceTask.None,
                                               job.Id.ToString(CultureInfo.InvariantCulture), job.InstanceId.ToString(), job.Name,
                                               job.Location, job.JobStateInfo.State.ToString(),
                                               job.Command);
                                               string.Empty, string.Empty, "NULL job");
            return DebugChannel.TraceInformational(PowerShellTraceEvent.TraceMessage,
                                            PowerShellTraceOperationCode.None,
                                            PowerShellTraceTask.None, message);
            return DebugChannel.TraceInformational(PowerShellTraceEvent.TraceMessage2,
                                            PowerShellTraceTask.None, message1, message2);
            return DebugChannel.TraceInformational(PowerShellTraceEvent.TraceMessageGuid,
                                            PowerShellTraceTask.None, message, instanceId);
            PSEtwLog.LogAnalyticVerbose(PSEventId.Engine_Trace,
                                        PSOpcode.Method, PSTask.None,
                                        PSKeyword.UseAlwaysAnalytic,
                                        className, methodName, workflowId.ToString(),
                                        parameters == null ? message : StringUtil.Format(message, parameters),
                                        string.Empty, // Job
                                        string.Empty, // Activity name
                                        string.Empty, // Activity GUID
                    sb.AppendLine(StringUtil.Format(EtwLoggingStrings.JobName, job.Name));
                    sb.AppendLine(StringUtil.Format(EtwLoggingStrings.JobId, job.Id.ToString(CultureInfo.InvariantCulture)));
                    sb.AppendLine(StringUtil.Format(EtwLoggingStrings.JobInstanceId, job.InstanceId.ToString()));
                    sb.AppendLine(StringUtil.Format(EtwLoggingStrings.JobLocation, job.Location));
                    sb.AppendLine(StringUtil.Format(EtwLoggingStrings.JobState, job.JobStateInfo.State.ToString()));
                    sb.AppendLine(StringUtil.Format(EtwLoggingStrings.JobCommand, job.Command));
                    // Exception in 3rd party code should never cause a crash due to tracing. The
                    // Implementation of the property getters could throw.
                    TraceException(e);
                    // If an exception is thrown, make sure the message is not partially formed.
                    sb.AppendLine(StringUtil.Format(EtwLoggingStrings.JobName, EtwLoggingStrings.NullJobName));
                                        sb.ToString(), // Job
        /// Writes operational scheduled job start message.
        public void WriteScheduledJobStartEvent(params object[] args)
            PSEtwLog.LogOperationalInformation(PSEventId.ScheduledJob_Start,
                                               PSOpcode.Method,
                                               PSTask.ScheduledJob,
        /// Writes operational scheduled job completed message.
        public void WriteScheduledJobCompleteEvent(params object[] args)
            PSEtwLog.LogOperationalInformation(PSEventId.ScheduledJob_Complete,
        /// Writes operational scheduled job error message.
        public void WriteScheduledJobErrorEvent(params object[] args)
            PSEtwLog.LogOperationalError(PSEventId.ScheduledJob_Error,
        /// Writes operational ISE execute script message.
        public void WriteISEExecuteScriptEvent(params object[] args)
            PSEtwLog.LogOperationalInformation(PSEventId.ISEExecuteScript,
                                               PSTask.ISEOperation,
        /// Writes operational ISE execute selection message.
        public void WriteISEExecuteSelectionEvent(params object[] args)
            PSEtwLog.LogOperationalInformation(PSEventId.ISEExecuteSelection,
        /// Writes operational ISE stop command message.
        public void WriteISEStopCommandEvent(params object[] args)
            PSEtwLog.LogOperationalInformation(PSEventId.ISEStopCommand,
        /// Writes operational ISE resume debugger message.
        public void WriteISEResumeDebuggerEvent(params object[] args)
            PSEtwLog.LogOperationalInformation(PSEventId.ISEResumeDebugger,
        /// Writes operational ISE stop debugger message.
        public void WriteISEStopDebuggerEvent(params object[] args)
            PSEtwLog.LogOperationalInformation(PSEventId.ISEStopDebugger,
        /// Writes operational ISE debugger step into message.
        public void WriteISEDebuggerStepIntoEvent(params object[] args)
            PSEtwLog.LogOperationalInformation(PSEventId.ISEDebuggerStepInto,
        /// Writes operational ISE debugger step over message.
        public void WriteISEDebuggerStepOverEvent(params object[] args)
            PSEtwLog.LogOperationalInformation(PSEventId.ISEDebuggerStepOver,
        /// Writes operational ISE debugger step out message.
        public void WriteISEDebuggerStepOutEvent(params object[] args)
            PSEtwLog.LogOperationalInformation(PSEventId.ISEDebuggerStepOut,
        /// Writes operational ISE enable all breakpoints message.
        public void WriteISEEnableAllBreakpointsEvent(params object[] args)
            PSEtwLog.LogOperationalInformation(PSEventId.ISEEnableAllBreakpoints,
        /// Writes operational ISE disable all breakpoints message.
        public void WriteISEDisableAllBreakpointsEvent(params object[] args)
            PSEtwLog.LogOperationalInformation(PSEventId.ISEDisableAllBreakpoints,
        /// Writes operational ISE remove all breakpoints message.
        public void WriteISERemoveAllBreakpointsEvent(params object[] args)
            PSEtwLog.LogOperationalInformation(PSEventId.ISERemoveAllBreakpoints,
        /// Writes operational ISE set breakpoint message.
        public void WriteISESetBreakpointEvent(params object[] args)
            PSEtwLog.LogOperationalInformation(PSEventId.ISESetBreakpoint,
        /// Writes operational ISE remove breakpoint message.
        public void WriteISERemoveBreakpointEvent(params object[] args)
            PSEtwLog.LogOperationalInformation(PSEventId.ISERemoveBreakpoint,
        /// Writes operational ISE enable breakpoint message.
        public void WriteISEEnableBreakpointEvent(params object[] args)
            PSEtwLog.LogOperationalInformation(PSEventId.ISEEnableBreakpoint,
        /// Writes operational ISE disable breakpoint message.
        public void WriteISEDisableBreakpointEvent(params object[] args)
            PSEtwLog.LogOperationalInformation(PSEventId.ISEDisableBreakpoint,
        /// Writes operational ISE hit breakpoint message.
        public void WriteISEHitBreakpointEvent(params object[] args)
            PSEtwLog.LogOperationalInformation(PSEventId.ISEHitBreakpoint,
        /// <param name="activityName"></param>
        public void WriteMessage(string className, string methodName, Guid workflowId, string activityName, Guid activityId, string message, params string[] parameters)
                                        activityName,
                                        activityId.ToString(),
        public bool TraceWSManConnectionInfo(WSManConnectionInfo connectionInfo)
        /// Gives access to Debug channel writer.
        public BaseChannelWriter DebugChannel { get; }
        /// Gives access to analytical channel writer.
        public BaseChannelWriter AnalyticChannel { get; }
        /// Gives access to operational channel writer.
        public BaseChannelWriter OperationalChannel { get; }
    // pragma warning restore 16001,16003

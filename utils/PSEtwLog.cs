    /// ETW logging API.
    internal static class PSEtwLog
        private static readonly PSSysLogProvider provider;
        private static readonly PSEtwLogProvider provider;
        static PSEtwLog()
            provider = new PSSysLogProvider();
            provider = new PSEtwLogProvider();
        internal static void LogConsoleStartup()
            PSEtwLog.LogOperationalInformation(PSEventId.Perftrack_ConsoleStartupStart, PSOpcode.WinStart,
        internal static void LogEngineHealthEvent(LogContext logContext, int eventId, Exception exception, Dictionary<string, string> additionalInfo)
        internal static void LogEngineLifecycleEvent(LogContext logContext, EngineState newState, EngineState previousState)
            provider.LogEngineLifecycleEvent(logContext, newState, previousState);
        internal static void LogCommandHealthEvent(LogContext logContext, Exception exception)
            provider.LogCommandHealthEvent(logContext, exception);
        internal static void LogCommandLifecycleEvent(LogContext logContext, CommandState newState)
            provider.LogCommandLifecycleEvent(() => logContext, newState);
        internal static void LogPipelineExecutionDetailEvent(LogContext logContext, List<string> pipelineExecutionDetail)
            provider.LogPipelineExecutionDetailEvent(logContext, pipelineExecutionDetail);
        internal static void LogProviderHealthEvent(LogContext logContext, string providerName, Exception exception)
            provider.LogProviderHealthEvent(logContext, providerName, exception);
        internal static void LogProviderLifecycleEvent(LogContext logContext, string providerName, ProviderState newState)
            provider.LogProviderLifecycleEvent(logContext, providerName, newState);
        internal static void LogAmsiUtilStateEvent(string state, string context)
            provider.LogAmsiUtilStateEvent(state, context);
        internal static void LogWDACQueryEvent(
            provider.LogWDACQueryEvent(queryName, fileName ?? string.Empty, querySuccess, queryResult);
        internal static void LogWDACAuditEvent(
            provider.LogWDACAuditEvent(title, message, fqid);
        internal static void LogSettingsEvent(LogContext logContext, string variableName, string value, string previousValue)
            provider.LogSettingsEvent(logContext, variableName, value, previousValue);
        /// Logs information to the operational channel.
        /// <param name="opcode"></param>
        /// <param name="task"></param>
        internal static void LogOperationalInformation(PSEventId id, PSOpcode opcode, PSTask task, PSKeyword keyword, params object[] args)
            provider.WriteEvent(id, PSChannel.Operational, opcode, PSLevel.Informational, task, keyword, args);
        internal static void LogOperationalWarning(PSEventId id, PSOpcode opcode, PSTask task, PSKeyword keyword, params object[] args)
            provider.WriteEvent(id, PSChannel.Operational, opcode, PSLevel.Warning, task, keyword, args);
        /// Logs Verbose to the operational channel.
        internal static void LogOperationalVerbose(PSEventId id, PSOpcode opcode, PSTask task, PSKeyword keyword, params object[] args)
            provider.WriteEvent(id, PSChannel.Operational, opcode, PSLevel.Verbose, task, keyword, args);
        /// Logs error message to the analytic channel.
        internal static void LogAnalyticError(PSEventId id, PSOpcode opcode, PSTask task, PSKeyword keyword, params object[] args)
            provider.WriteEvent(id, PSChannel.Analytic, opcode, PSLevel.Error, task, keyword, args);
        /// Logs warning message to the analytic channel.
        internal static void LogAnalyticWarning(PSEventId id, PSOpcode opcode, PSTask task, PSKeyword keyword, params object[] args)
            provider.WriteEvent(id, PSChannel.Analytic, opcode, PSLevel.Warning, task, keyword, args);
        /// Logs remoting fragment data to verbose channel.
        /// <param name="objectId"></param>
        /// <param name="fragmentId"></param>
        /// <param name="isStartFragment"></param>
        /// <param name="isEndFragment"></param>
        /// <param name="fragmentLength"></param>
        /// <param name="fragmentData"></param>
        internal static void LogAnalyticVerbose(PSEventId id, PSOpcode opcode, PSTask task, PSKeyword keyword,
            Int64 objectId,
            Int64 fragmentId,
            int isStartFragment,
            int isEndFragment,
            UInt32 fragmentLength,
            PSETWBinaryBlob fragmentData)
            if (provider.IsEnabled(PSLevel.Verbose, keyword))
                string payLoadData = Convert.ToHexString(fragmentData.blob, fragmentData.offset, fragmentData.length);
                payLoadData = string.Create(CultureInfo.InvariantCulture, $"0x{payLoadData}");
                provider.WriteEvent(id, PSChannel.Analytic, opcode, PSLevel.Verbose, task, keyword,
                                    objectId, fragmentId, isStartFragment, isEndFragment, fragmentLength,
                                    payLoadData);
        /// Logs verbose message to the analytic channel.
        internal static void LogAnalyticVerbose(PSEventId id, PSOpcode opcode, PSTask task, PSKeyword keyword, params object[] args)
            provider.WriteEvent(id, PSChannel.Analytic, opcode, PSLevel.Verbose, task, keyword, args);
        /// Logs informational message to the analytic channel.
        internal static void LogAnalyticInformational(PSEventId id, PSOpcode opcode, PSTask task, PSKeyword keyword, params object[] args)
            provider.WriteEvent(id, PSChannel.Analytic, opcode, PSLevel.Informational, task, keyword, args);
        /// Logs error message to operation channel.
        internal static void LogOperationalError(PSEventId id, PSOpcode opcode, PSTask task, PSKeyword keyword, params object[] args)
            provider.WriteEvent(id, PSChannel.Operational, opcode, PSLevel.Error, task, keyword, args);
        /// Logs error message to the operational channel.
        /// <param name="payLoad"></param>
        internal static void LogOperationalError(PSEventId id, PSOpcode opcode, PSTask task, LogContext logContext, string payLoad)
            provider.WriteEvent(id, PSChannel.Operational, opcode, task, logContext, payLoad);
        internal static void SetActivityIdForCurrentThread(Guid newActivityId)
            provider.SetActivityIdForCurrentThread(newActivityId);
        internal static void ReplaceActivityIdForCurrentThread(Guid newActivityId,
            PSEventId eventForOperationalChannel, PSEventId eventForAnalyticChannel, PSKeyword keyword, PSTask task)
            // set the new activity id
            // Once the activity id is set, write the transfer event
            WriteTransferEvent(newActivityId, eventForOperationalChannel, eventForAnalyticChannel, keyword, task);
        /// Writes a transfer event mapping current activity id
        /// with a related activity id
        /// This function writes a transfer event for both the
        /// operational and analytic channels.
        /// <param name="relatedActivityId"></param>
        /// <param name="eventForOperationalChannel"></param>
        /// <param name="eventForAnalyticChannel"></param>
        internal static void WriteTransferEvent(Guid relatedActivityId, PSEventId eventForOperationalChannel,
                            PSEventId eventForAnalyticChannel, PSKeyword keyword, PSTask task)
            provider.WriteEvent(eventForOperationalChannel, PSChannel.Operational, PSOpcode.Method, PSLevel.Informational, task,
                PSKeyword.UseAlwaysOperational);
            provider.WriteEvent(eventForAnalyticChannel, PSChannel.Analytic, PSOpcode.Method, PSLevel.Informational, task,
                PSKeyword.UseAlwaysAnalytic);
        /// Writes a transfer event.
        /// <param name="parentActivityId"></param>
        internal static void WriteTransferEvent(Guid parentActivityId)
            provider.WriteTransferEvent(parentActivityId);

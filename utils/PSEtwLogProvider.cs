    /// ETW log provider implementation.
    internal class PSEtwLogProvider : LogProvider
        private static readonly EventProvider etwProvider;
        internal static readonly Guid ProviderGuid = new Guid("F90714A8-5509-434A-BF6D-B1624C8A19A2");
        private static readonly EventDescriptor _xferEventDescriptor = new EventDescriptor(0x1f05, 0x1, 0x11, 0x5, 0x14, 0x0, (long)0x4000000000000000);
        static PSEtwLogProvider()
            etwProvider = new EventProvider(ProviderGuid);
        /// Determines whether any session is requesting the specified event from the provider.
        /// <param name="level"></param>
        /// <param name="keywords"></param>
        /// Typically, a provider does not call this method to determine whether a session requested the specified event;
        /// the provider simply writes the event, and ETW determines whether the event is logged to a session. A provider
        /// may want to call this function if the provider needs to perform extra work to generate the event. In this case,
        ///  calling this function first to determine if a session requested the event or not, may save resources and time.
        internal bool IsEnabled(PSLevel level, PSKeyword keywords)
            return etwProvider.IsEnabled((byte)level, (long)keywords);
            StringBuilder payload = new StringBuilder();
            AppendException(payload, exception);
            payload.AppendLine();
            AppendAdditionalInfo(payload, additionalInfo);
            WriteEvent(PSEventId.Engine_Health, PSChannel.Operational, PSOpcode.Exception, PSTask.ExecutePipeline, logContext, payload.ToString());
            if (IsEnabled(PSLevel.Informational, PSKeyword.Cmdlets | PSKeyword.UseAlwaysAnalytic))
                payload.AppendLine(StringUtil.Format(EtwLoggingStrings.EngineStateChange, previousState.ToString(), newState.ToString()));
                PSTask task = PSTask.EngineStart;
                if (newState == EngineState.Stopped ||
                    newState == EngineState.OutOfService ||
                    newState == EngineState.None ||
                    newState == EngineState.Degraded)
                    task = PSTask.EngineStop;
                WriteEvent(PSEventId.Engine_Lifecycle, PSChannel.Analytic, PSOpcode.Method, task, logContext, payload.ToString());
            WriteEvent(PSEventId.Command_Health, PSChannel.Operational, PSOpcode.Exception, PSTask.ExecutePipeline, logContext, payload.ToString());
                LogContext logContext = getLogContext();
                if (logContext.CommandType != null)
                    if (logContext.CommandType.Equals(StringLiterals.Script, StringComparison.OrdinalIgnoreCase))
                        payload.AppendLine(StringUtil.Format(EtwLoggingStrings.ScriptStateChange, newState.ToString()));
                        if (newState == CommandState.Stopped ||
                            newState == CommandState.Terminated)
                            // When state is stopped or terminated only log the CommandName
                            payload.AppendLine(StringUtil.Format(EtwLoggingStrings.CommandStateChange, logContext, newState.ToString()));
                            // When state is Start log the CommandLine which has arguments for completeness.
                            payload.AppendLine(StringUtil.Format(EtwLoggingStrings.CommandStateChange, logContext.CommandLine, newState.ToString()));
                PSTask task = PSTask.CommandStart;
                    task = PSTask.CommandStop;
                WriteEvent(PSEventId.Command_Lifecycle, PSChannel.Analytic, PSOpcode.Method, task, logContext, payload.ToString());
            if (pipelineExecutionDetail != null)
                foreach (string detail in pipelineExecutionDetail)
                    payload.AppendLine(detail);
            WriteEvent(PSEventId.Pipeline_Detail, PSChannel.Operational, PSOpcode.Method, PSTask.ExecutePipeline, logContext, payload.ToString());
            Dictionary<string, string> additionalInfo = new Dictionary<string, string>();
            additionalInfo.Add(EtwLoggingStrings.ProviderNameString, providerName);
            WriteEvent(PSEventId.Provider_Health, PSChannel.Operational, PSOpcode.Exception, PSTask.ExecutePipeline, logContext, payload.ToString());
            WriteEvent(PSEventId.Amsi_Init, PSChannel.Analytic, PSOpcode.Method, PSLevel.Informational, PSTask.Amsi, (PSKeyword)0x0, state, context);
            WriteEvent(PSEventId.WDAC_Query, PSChannel.Analytic, PSOpcode.Method, PSLevel.Informational, PSTask.WDAC, (PSKeyword)0x0, queryName, fileName, querySuccess, queryResult);
            WriteEvent(PSEventId.WDAC_Audit, PSChannel.Operational, PSOpcode.Method, PSLevel.Informational, PSTask.WDACAudit, (PSKeyword)0x0, title, message, fqid);
                payload.AppendLine(StringUtil.Format(EtwLoggingStrings.ProviderStateChange, providerName, newState.ToString()));
                PSTask task = PSTask.ProviderStart;
                if (newState == ProviderState.Stopped)
                    task = PSTask.ProviderStop;
                WriteEvent(PSEventId.Provider_Lifecycle, PSChannel.Analytic, PSOpcode.Method, task, logContext, payload.ToString());
                if (previousValue == null)
                    payload.AppendLine(StringUtil.Format(EtwLoggingStrings.SettingChangeNoPrevious, variableName, value));
                    payload.AppendLine(StringUtil.Format(EtwLoggingStrings.SettingChange, variableName, previousValue, value));
                WriteEvent(PSEventId.Settings, PSChannel.Analytic, PSOpcode.Method, PSTask.ExecutePipeline, logContext, payload.ToString());
        /// The ETW provider does not use logging variables.
        internal override bool UseLoggingVariables()
        /// Writes a single event.
        /// <param name="id">Event id.</param>
        /// <param name="logContext">Log context.</param>
        internal void WriteEvent(PSEventId id, PSChannel channel, PSOpcode opcode, PSTask task, LogContext logContext, string payLoad)
            WriteEvent(id, channel, opcode, GetPSLevelFromSeverity(logContext.Severity), task, (PSKeyword)0x0,
                LogContextToString(logContext), GetPSLogUserData(logContext.ExecutionContext), payLoad);
        /// Writes an event.
        internal void WriteEvent(PSEventId id, PSChannel channel, PSOpcode opcode, PSLevel level, PSTask task, PSKeyword keyword, params object[] args)
            long longKeyword = 0x00;
            if (keyword == PSKeyword.UseAlwaysAnalytic ||
                keyword == PSKeyword.UseAlwaysOperational)
                longKeyword = 0x00;
                longKeyword = (long)keyword;
            EventDescriptor desc = new EventDescriptor((int)id, (byte)PSEventVersion.One, (byte)channel,
                (byte)level, (byte)opcode, (int)task, longKeyword);
            etwProvider.WriteEvent(in desc, args);
        /// Writes an activity transfer event.
        internal void WriteTransferEvent(Guid parentActivityId)
            etwProvider.WriteTransferEvent(in _xferEventDescriptor, parentActivityId, EtwActivity.GetActivityId(), parentActivityId);
        /// <param name="newActivityId"></param>
        internal void SetActivityIdForCurrentThread(Guid newActivityId)
            Guid result = newActivityId;
            EventProvider.SetActivityId(ref result);

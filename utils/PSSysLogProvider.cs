    /// SysLog LogProvider implementation.
    internal class PSSysLogProvider : LogProvider
        private static readonly SysLogProvider s_provider;
        // by default, do not include channel bits
        internal const PSKeyword DefaultKeywords = (PSKeyword)(0x00FFFFFFFFFFFFFF);
        // the default enabled channel(s)
        internal const PSChannel DefaultChannels = PSChannel.Operational;
        static PSSysLogProvider()
            s_provider = new SysLogProvider(PowerShellConfig.Instance.GetSysLogIdentity(),
                                            PowerShellConfig.Instance.GetLogLevel(),
                                            PowerShellConfig.Instance.GetLogKeywords(),
                                            PowerShellConfig.Instance.GetLogChannels());
        /// Defines a thread local StringBuilder for building event payload strings.
        /// NOTE: do not access this field directly, use the PayloadBuilder
        /// property to ensure correct thread initialization; otherwise, a null reference can occur.
        private static StringBuilder t_payloadBuilder;
        private static StringBuilder PayloadBuilder
                // NOTE: Thread static fields must be explicitly initialized for each thread.
                t_payloadBuilder ??= new StringBuilder(200);
                return t_payloadBuilder;
            return s_provider.IsEnabled(level, keywords);
            StringBuilder payload = PayloadBuilder;
            payload.Clear();
        /// <param name="state">This the action performed in AmsiUtil class, like init, scan, etc</param>
        /// <param name="context">The amsiContext handled - Session pair</param>
            WriteEvent(PSEventId.WDAC_Audit, PSChannel.Operational, PSOpcode.Method, PSLevel.Informational, PSTask.WDAC, (PSKeyword)0x0, title, message, fqid);
                        payload.AppendLine(StringUtil.Format(EtwLoggingStrings.CommandStateChange, logContext.CommandName, newState.ToString()));
        /// The SysLog provider does not use logging variables.
            s_provider.Log(id, channel, task, opcode, GetPSLevelFromSeverity(logContext.Severity), DefaultKeywords,
                           LogContextToString(logContext),
                           GetPSLogUserData(logContext.ExecutionContext),
                           payLoad);
            s_provider.Log(id, channel, task, opcode, level, keyword, args);
            s_provider.LogTransfer(parentActivityId);
        /// Sets the activity id for the current thread.
        /// <param name="newActivityId">The GUID identifying the activity.</param>
            s_provider.SetActivity(newActivityId);

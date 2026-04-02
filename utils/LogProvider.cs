    /// Monad Logging in general is a two layer architecture. At the upper layer are the
    /// Msh Log Engine and Logging Api. At the lower layer is the Provider Interface
    /// and Log Providers. This architecture is adopted to achieve independency between
    /// Monad logging and logging details of different logging technology.
    /// This file implements the lower layer of the Monad Logging architecture.
    /// Upper layer of Msh Log architecture is implemented in MshLog.cs file.
    /// This class defines the provider interface to be implemented by each providers.
    /// Provider Interface.
    /// Corresponding to 5 categories of logging api interface, provider interface provides
    /// functions for logging
    ///     a. EngineHealthEvent
    ///     b. EngineLifecycleEvent
    ///     c. CommandLifecycleEvent
    ///     d. ProviderLifecycleEvent
    ///     e. SettingsEvent.
    internal abstract class LogProvider
        internal LogProvider()
        #region Provider api
        /// Provider interface function for logging health event.
        /// <param name="logContext"></param>
        /// <param name="eventId"></param>
        /// <param name="additionalInfo"></param>
        internal abstract void LogEngineHealthEvent(LogContext logContext, int eventId, Exception exception, Dictionary<string, string> additionalInfo);
        /// Provider interface function for logging engine lifecycle event.
        /// <param name="previousState"></param>
        internal abstract void LogEngineLifecycleEvent(LogContext logContext, EngineState newState, EngineState previousState);
        /// Provider interface function for logging command health event.
        internal abstract void LogCommandHealthEvent(LogContext logContext, Exception exception);
        /// Provider interface function for logging command lifecycle event.
        /// <param name="getLogContext"></param>
        internal abstract void LogCommandLifecycleEvent(Func<LogContext> getLogContext, CommandState newState);
        /// Provider interface function for logging pipeline execution detail.
        /// <param name="pipelineExecutionDetail"></param>
        internal abstract void LogPipelineExecutionDetailEvent(LogContext logContext, List<string> pipelineExecutionDetail);
        /// Provider interface function for logging provider health event.
        internal abstract void LogProviderHealthEvent(LogContext logContext, string providerName, Exception exception);
        /// Provider interface function for logging provider lifecycle event.
        internal abstract void LogProviderLifecycleEvent(LogContext logContext, string providerName, ProviderState newState);
        /// Provider interface function for logging settings event.
        /// <param name="previousValue"></param>
        internal abstract void LogSettingsEvent(LogContext logContext, string variableName, string value, string previousValue);
        /// Provider interface function for logging AmsiUtil State event.
        /// <param name="state">This the action performed in AmsiUtil class, like init, scan, etc.</param>
        /// <param name="context">The amsiContext handled - Session pair.</param>
        internal abstract void LogAmsiUtilStateEvent(string state, string context);
        /// Provider interface function for logging WDAC query event.
        /// <param name="queryName">Name of the WDAC query.</param>
        /// <param name="fileName">Name of script file for policy query. Can be null value.</param>
        /// <param name="querySuccess">Query call succeed code.</param>
        /// <param name="queryResult">Result code of WDAC query.</param>
        internal abstract void LogWDACQueryEvent(
            string queryName,
            int querySuccess,
            int queryResult);
        /// Provider interface function for logging WDAC audit event.
        /// <param name="title">Title of WDAC audit event.</param>
        /// <param name="message">WDAC audit event message.</param>
        /// <param name="fqid">FullyQualifiedId of WDAC audit event.</param>
        internal abstract void LogWDACAuditEvent(
            string fqid);
        /// True if the log provider needs to use logging variables.
        internal virtual bool UseLoggingVariables()
        #region Shared utilities
        private static class Strings
            // The strings are stored in a different class to defer loading the resources until as late
            // as possible, e.g. if logging is never on, these strings won't be loaded.
            internal static readonly string LogContextSeverity = EtwLoggingStrings.LogContextSeverity;
            internal static readonly string LogContextHostName = EtwLoggingStrings.LogContextHostName;
            internal static readonly string LogContextHostVersion = EtwLoggingStrings.LogContextHostVersion;
            internal static readonly string LogContextHostId = EtwLoggingStrings.LogContextHostId;
            internal static readonly string LogContextHostApplication = EtwLoggingStrings.LogContextHostApplication;
            internal static readonly string LogContextEngineVersion = EtwLoggingStrings.LogContextEngineVersion;
            internal static readonly string LogContextRunspaceId = EtwLoggingStrings.LogContextRunspaceId;
            internal static readonly string LogContextPipelineId = EtwLoggingStrings.LogContextPipelineId;
            internal static readonly string LogContextCommandName = EtwLoggingStrings.LogContextCommandName;
            internal static readonly string LogContextCommandType = EtwLoggingStrings.LogContextCommandType;
            internal static readonly string LogContextScriptName = EtwLoggingStrings.LogContextScriptName;
            internal static readonly string LogContextCommandPath = EtwLoggingStrings.LogContextCommandPath;
            internal static readonly string LogContextSequenceNumber = EtwLoggingStrings.LogContextSequenceNumber;
            internal static readonly string LogContextUser = EtwLoggingStrings.LogContextUser;
            internal static readonly string LogContextConnectedUser = EtwLoggingStrings.LogContextConnectedUser;
            internal static readonly string LogContextTime = EtwLoggingStrings.LogContextTime;
            internal static readonly string LogContextShellId = EtwLoggingStrings.LogContextShellId;
        /// Gets PSLogUserData from execution context.
        protected static string GetPSLogUserData(ExecutionContext context)
            object logData = context.GetVariableValue(SpecialVariables.PSLogUserDataPath);
            if (logData == null)
            return logData.ToString();
        /// Appends exception information.
        /// <param name="sb">String builder.</param>
        /// <param name="except">Exception.</param>
        protected static void AppendException(StringBuilder sb, Exception except)
            sb.AppendLine(StringUtil.Format(EtwLoggingStrings.ErrorRecordMessage, except.Message));
            if (except is IContainsErrorRecord ier)
                ErrorRecord er = ier.ErrorRecord;
                    sb.AppendLine(StringUtil.Format(EtwLoggingStrings.ErrorRecordId, er.FullyQualifiedErrorId));
                    ErrorDetails details = er.ErrorDetails;
                    if (details != null)
                        sb.AppendLine(StringUtil.Format(EtwLoggingStrings.ErrorRecordRecommendedAction, details.RecommendedAction));
        /// Appends additional information.
        /// <param name="additionalInfo">Additional information.</param>
        protected static void AppendAdditionalInfo(StringBuilder sb, Dictionary<string, string> additionalInfo)
            if (additionalInfo != null)
                foreach (KeyValuePair<string, string> value in additionalInfo)
                    sb.AppendLine(StringUtil.Format("{0} = {1}", value.Key, value.Value));
        /// Gets PSLevel from severity.
        /// <param name="severity">Error severity.</param>
        /// <returns>PS log level.</returns>
        protected static PSLevel GetPSLevelFromSeverity(string severity)
            switch (severity)
                case "Critical":
                case "Error":
                    return PSLevel.Error;
                case "Warning":
                    return PSLevel.Warning;
                    return PSLevel.Informational;
        // Estimate an approximate size to use for the StringBuilder in LogContextToString
        // Estimated length of all Strings.* values
        // Rough estimate of values
        // max path for Command path
        private const int LogContextInitialSize = 30 * 16 + 13 * 20 + 255;
        /// Converts log context to string.
        /// <param name="context">Log context.</param>
        protected static string LogContextToString(LogContext context)
            StringBuilder sb = new StringBuilder(LogContextInitialSize);
            sb.Append(Strings.LogContextSeverity);
            sb.AppendLine(context.Severity);
            sb.Append(Strings.LogContextHostName);
            sb.AppendLine(context.HostName);
            sb.Append(Strings.LogContextHostVersion);
            sb.AppendLine(context.HostVersion);
            sb.Append(Strings.LogContextHostId);
            sb.AppendLine(context.HostId);
            sb.Append(Strings.LogContextHostApplication);
            sb.AppendLine(context.HostApplication);
            sb.Append(Strings.LogContextEngineVersion);
            sb.AppendLine(context.EngineVersion);
            sb.Append(Strings.LogContextRunspaceId);
            sb.AppendLine(context.RunspaceId);
            sb.Append(Strings.LogContextPipelineId);
            sb.AppendLine(context.PipelineId);
            sb.Append(Strings.LogContextCommandName);
            sb.AppendLine(context.CommandName);
            sb.Append(Strings.LogContextCommandType);
            sb.AppendLine(context.CommandType);
            sb.Append(Strings.LogContextScriptName);
            sb.AppendLine(context.ScriptName);
            sb.Append(Strings.LogContextCommandPath);
            sb.AppendLine(context.CommandPath);
            sb.Append(Strings.LogContextSequenceNumber);
            sb.AppendLine(context.SequenceNumber);
            sb.Append(Strings.LogContextUser);
            sb.AppendLine(context.User);
            sb.Append(Strings.LogContextConnectedUser);
            sb.AppendLine(context.ConnectedUser);
            sb.Append(Strings.LogContextShellId);
            sb.AppendLine(context.ShellId);
    internal class DummyLogProvider : LogProvider
        internal DummyLogProvider()
        /// DummyLogProvider does nothing to Logging EngineHealthEvent.
        internal override void LogEngineHealthEvent(LogContext logContext, int eventId, Exception exception, Dictionary<string, string> additionalInfo)
        /// DummyLogProvider does nothing to Logging EngineLifecycleEvent.
        internal override void LogEngineLifecycleEvent(LogContext logContext, EngineState newState, EngineState previousState)
        internal override void LogCommandHealthEvent(LogContext logContext, Exception exception)
        /// DummyLogProvider does nothing to Logging CommandLifecycleEvent.
        internal override void LogCommandLifecycleEvent(Func<LogContext> getLogContext, CommandState newState)
        /// DummyLogProvider does nothing to Logging PipelineExecutionDetailEvent.
        internal override void LogPipelineExecutionDetailEvent(LogContext logContext, List<string> pipelineExecutionDetail)
        internal override void LogProviderHealthEvent(LogContext logContext, string providerName, Exception exception)
        /// DummyLogProvider does nothing to Logging ProviderLifecycleEvent.
        internal override void LogProviderLifecycleEvent(LogContext logContext, string providerName, ProviderState newState)
        /// DummyLogProvider does nothing to Logging SettingsEvent.
        internal override void LogSettingsEvent(LogContext logContext, string variableName, string value, string previousValue)
        internal override void LogAmsiUtilStateEvent(string state, string context)
        internal override void LogWDACQueryEvent(
            int queryResult)
        internal override void LogWDACAuditEvent(
            string fqid)

using Microsoft.Management.Infrastructure;
using Dbg = System.Management.Automation.Diagnostics;
namespace Microsoft.PowerShell.Cmdletization.Cim
    /// Represents an error during execution of a CIM job.
    public class CimJobException : SystemException, IContainsErrorRecord
        #region Standard constructors and methods required for all exceptions
        /// Initializes a new instance of the <see cref="CimJobException"/> class.
        public CimJobException() : this(null, null)
        /// Initializes a new instance of the <see cref="CimJobException"/> class with a specified error message.
        /// <param name="message">The message that describes the error.</param>
        public CimJobException(string message) : this(message, null)
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public CimJobException(string message, Exception inner) : base(message, inner)
            InitializeErrorRecord(null, "CimJob_ExternalError", ErrorCategory.NotSpecified);
        /// Initializes a new instance of the <see cref="CimJobException"/> class with serialized data.
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        [Obsolete("Legacy serialization support is deprecated since .NET 8", DiagnosticId = "SYSLIB0051")]
        protected CimJobException(
            SerializationInfo info,
            StreamingContext context)
        internal static CimJobException CreateFromCimException(
            string jobDescription,
            CimJobContext jobContext,
            CimException cimException)
            Dbg.Assert(!string.IsNullOrEmpty(jobDescription), "Caller should verify jobDescription != null");
            Dbg.Assert(jobContext != null, "Caller should verify jobContext != null");
            Dbg.Assert(cimException != null, "Caller should verify cimException != null");
            string message = BuildErrorMessage(jobDescription, jobContext, cimException.Message);
            CimJobException cimJobException = new(message, cimException);
            cimJobException.InitializeErrorRecord(jobContext, cimException);
            return cimJobException;
        internal static CimJobException CreateFromAnyException(
            Exception inner)
            Dbg.Assert(inner != null, "Caller should verify inner != null");
                return CreateFromCimException(jobDescription, jobContext, cimException);
            string message = BuildErrorMessage(jobDescription, jobContext, inner.Message);
            CimJobException cimJobException = new(message, inner);
                cimJobException.InitializeErrorRecord(
                    jobContext,
                    errorId: "CimJob_" + containsErrorRecord.ErrorRecord.FullyQualifiedErrorId,
                    errorCategory: containsErrorRecord.ErrorRecord.CategoryInfo.Category);
                    errorId: "CimJob_" + inner.GetType().Name,
                    errorCategory: ErrorCategory.NotSpecified);
        internal static CimJobException CreateWithFullControl(
            string message,
            Exception inner = null)
            Dbg.Assert(!string.IsNullOrEmpty(message), "Caller should verify message != null");
            Dbg.Assert(!string.IsNullOrEmpty(errorId), "Caller should verify errorId != null");
            CimJobException cimJobException = new(jobContext.PrependComputerNameToMessage(message), inner);
            cimJobException.InitializeErrorRecord(jobContext, errorId, errorCategory);
        internal static CimJobException CreateWithoutJobContext(
            cimJobException.InitializeErrorRecord(null, errorId, errorCategory);
        internal static CimJobException CreateFromMethodErrorCode(string jobDescription, CimJobContext jobContext, string methodName, string errorCodeFromMethod)
            string rawErrorMessage = string.Format(
                CmdletizationResources.CimJob_ErrorCodeFromMethod,
                errorCodeFromMethod);
            string errorMessage = BuildErrorMessage(jobDescription, jobContext, rawErrorMessage);
            CimJobException cje = new(errorMessage);
            cje.InitializeErrorRecord(jobContext, "CimJob_" + methodName + "_" + errorCodeFromMethod, ErrorCategory.InvalidResult);
            return cje;
        private static string BuildErrorMessage(string jobDescription, CimJobContext jobContext, string errorMessage)
            Dbg.Assert(!string.IsNullOrEmpty(errorMessage), "Caller should verify !string.IsNullOrEmpty(errorMessage)");
            if (string.IsNullOrEmpty(jobDescription))
                return jobContext.PrependComputerNameToMessage(errorMessage);
                string errorMessageWithJobDescription = string.Format(
                    CmdletizationResources.CimJob_GenericCimFailure,
                    errorMessage,
                    jobDescription);
                return jobContext.PrependComputerNameToMessage(errorMessageWithJobDescription);
        private void InitializeErrorRecordCore(CimJobContext jobContext, Exception exception, string errorId, ErrorCategory errorCategory)
                targetObject: jobContext?.TargetObject);
            if (jobContext != null)
                    jobContext.Session.ComputerName,
                _errorRecord = new System.Management.Automation.Runspaces.RemotingErrorRecord(
                _errorRecord.SetInvocationInfo(jobContext.CmdletInvocationInfo);
                _errorRecord.PreserveInvocationInfoOnce = true;
                _errorRecord = coreErrorRecord;
        private void InitializeErrorRecord(CimJobContext jobContext, string errorId, ErrorCategory errorCategory)
            InitializeErrorRecordCore(
                jobContext: jobContext,
                exception: this,
                errorCategory: errorCategory);
        private void InitializeErrorRecord(CimJobContext jobContext, CimException cimException)
                errorCategory: ConvertCimExceptionToErrorCategory(cimException));
                _errorRecord.CategoryInfo.TargetName = cimException.ErrorSource;
                _errorRecord.CategoryInfo.TargetType = jobContext?.CmdletizationClassName;
        private static ErrorCategory ConvertCimExceptionToErrorCategory(CimException cimException)
        private static ErrorCategory ConvertCimNativeErrorCodeToErrorCategory(NativeErrorCode nativeErrorCode)
                case NativeErrorCode.Ok:
        private static ErrorCategory ConvertCimErrorToErrorCategory(CimInstance cimError)
            CimProperty errorCategoryProperty = cimError.CimInstanceProperties["Error_Category"];
        /// <see cref="ErrorRecord"/> which provides additional information about the error.
        public ErrorRecord ErrorRecord
            get { return _errorRecord; }
        private ErrorRecord _errorRecord;
        internal bool IsTerminatingError
                if ((this.InnerException is not CimException cimException) || (cimException.ErrorData == null))
                CimProperty perceivedSeverityProperty = cimException.ErrorData.CimInstanceProperties["PerceivedSeverity"];
                if ((perceivedSeverityProperty == null) || (perceivedSeverityProperty.CimType != CimType.UInt16) || (perceivedSeverityProperty.Value == null))
                ushort perceivedSeverityValue = (ushort)perceivedSeverityProperty.Value;
                if (perceivedSeverityValue != 7)
                    /* from CIM Schema: Interop\CIM_Error.mof:
                         "7 - Fatal/NonRecoverable should be used to indicate an "
                          "error occurred, but it\'s too late to take remedial "
                          "action. \n"

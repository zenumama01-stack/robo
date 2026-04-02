    #region class ErrorToErrorRecord
    /// Convert error or exception to <see cref="System.Management.Automation.ErrorRecord"/>
    internal sealed class ErrorToErrorRecord
        /// Convert ErrorRecord from exception object, <see cref="Exception"/>
        /// can be either <see cref="CimException"/> or general <see cref="Exception"/>.
        /// <param name="inner"></param>
        /// <param name="context">The context starting the operation, which generated the error.</param>
        /// <param name="cimResultContext">The CimResultContext used to provide ErrorSource, etc. info.</param>
        internal static ErrorRecord ErrorRecordFromAnyException(
            InvocationContext context,
            Exception inner,
            Debug.Assert(inner != null, "Caller should verify inner != null");
            if (inner is CimException cimException)
                return CreateFromCimException(context, cimException, cimResultContext);
            if (inner is IContainsErrorRecord containsErrorRecord)
                return InitializeErrorRecord(context,
                    exception: inner,
                    errorId: "CimCmdlet_" + containsErrorRecord.ErrorRecord.FullyQualifiedErrorId,
                    errorCategory: containsErrorRecord.ErrorRecord.CategoryInfo.Category,
                    cimResultContext: cimResultContext);
                    errorId: "CimCmdlet_" + inner.GetType().Name,
                    errorCategory: ErrorCategory.NotSpecified,
        #region Helper functions
        /// Create <see cref="ErrorRecord"/> from <see cref="CimException"/> object.
        /// <param name="cimException"></param>
        internal static ErrorRecord CreateFromCimException(
            CimException cimException,
            Debug.Assert(cimException != null, "Caller should verify cimException != null");
            return InitializeErrorRecord(context, cimException, cimResultContext);
        /// Create <see cref="ErrorRecord"/> from <see cref="Exception"/> object.
        /// <param name="exception"></param>
        /// <param name="errorId"></param>
        /// <param name="errorCategory"></param>
        internal static ErrorRecord InitializeErrorRecord(
            Exception exception,
            string errorId,
            ErrorCategory errorCategory,
            return InitializeErrorRecordCore(
                context,
                exception: exception,
                errorId: errorId,
                errorCategory: errorCategory,
            ErrorRecord errorRecord = InitializeErrorRecordCore(
                exception: cimException,
                errorId: cimException.MessageId ?? "MiClientApiError_" + cimException.NativeErrorCode,
                errorCategory: ConvertCimExceptionToErrorCategory(cimException),
            if (cimException.ErrorData != null)
                errorRecord.CategoryInfo.TargetName = cimException.ErrorSource;
            return errorRecord;
        internal static ErrorRecord InitializeErrorRecordCore(
            object theTargetObject = null;
            if (cimResultContext != null)
                theTargetObject = cimResultContext.ErrorSource;
            if (theTargetObject == null)
                if (context != null)
                    if (context.TargetCimInstance != null)
                        theTargetObject = context.TargetCimInstance;
            ErrorRecord coreErrorRecord = new(
                targetObject: theTargetObject);
            if (context == null)
                return coreErrorRecord;
            System.Management.Automation.Remoting.OriginInfo originInfo = new(
                context.ComputerName,
                Guid.Empty);
            ErrorRecord errorRecord = new System.Management.Automation.Runspaces.RemotingErrorRecord(
                coreErrorRecord,
                originInfo);
            DebugHelper.WriteLogEx("Created RemotingErrorRecord.", 0);
        /// Convert <see cref="CimException"/> to <see cref="ErrorCategory"/>.
        internal static ErrorCategory ConvertCimExceptionToErrorCategory(CimException cimException)
            ErrorCategory result = ErrorCategory.NotSpecified;
                result = ConvertCimErrorToErrorCategory(cimException.ErrorData);
            if (result == ErrorCategory.NotSpecified)
                result = ConvertCimNativeErrorCodeToErrorCategory(cimException.NativeErrorCode);
        /// Convert <see cref="NativeErrorCode"/> to <see cref="ErrorCategory"/>.
        /// <param name="nativeErrorCode"></param>
        internal static ErrorCategory ConvertCimNativeErrorCodeToErrorCategory(NativeErrorCode nativeErrorCode)
            switch (nativeErrorCode)
                case NativeErrorCode.Failed:
                    return ErrorCategory.NotSpecified;
                case NativeErrorCode.AccessDenied:
                    return ErrorCategory.PermissionDenied;
                case NativeErrorCode.InvalidNamespace:
                    return ErrorCategory.MetadataError;
                case NativeErrorCode.InvalidParameter:
                    return ErrorCategory.InvalidArgument;
                case NativeErrorCode.InvalidClass:
                case NativeErrorCode.NotFound:
                    return ErrorCategory.ObjectNotFound;
                case NativeErrorCode.NotSupported:
                    return ErrorCategory.NotImplemented;
                case NativeErrorCode.ClassHasChildren:
                case NativeErrorCode.ClassHasInstances:
                case NativeErrorCode.InvalidSuperClass:
                case NativeErrorCode.AlreadyExists:
                    return ErrorCategory.ResourceExists;
                case NativeErrorCode.NoSuchProperty:
                case NativeErrorCode.TypeMismatch:
                    return ErrorCategory.InvalidType;
                case NativeErrorCode.QueryLanguageNotSupported:
                case NativeErrorCode.InvalidQuery:
                case NativeErrorCode.MethodNotAvailable:
                case NativeErrorCode.MethodNotFound:
                case NativeErrorCode.NamespaceNotEmpty:
                case NativeErrorCode.InvalidEnumerationContext:
                case NativeErrorCode.InvalidOperationTimeout:
                case NativeErrorCode.PullHasBeenAbandoned:
                    return ErrorCategory.OperationStopped;
                case NativeErrorCode.PullCannotBeAbandoned:
                    return ErrorCategory.CloseError;
                case NativeErrorCode.FilteredEnumerationNotSupported:
                case NativeErrorCode.ContinuationOnErrorNotSupported:
                case NativeErrorCode.ServerLimitsExceeded:
                    return ErrorCategory.ResourceBusy;
                case NativeErrorCode.ServerIsShuttingDown:
                    return ErrorCategory.ResourceUnavailable;
        /// Convert <see cref="cimError"/> to <see cref="ErrorCategory"/>.
        /// <param name="cimError"></param>
        internal static ErrorCategory ConvertCimErrorToErrorCategory(CimInstance cimError)
            if (cimError == null)
            CimProperty errorCategoryProperty = cimError.CimInstanceProperties[@"Error_Category"];
            if (errorCategoryProperty == null)
            ErrorCategory errorCategoryValue;
            if (!LanguagePrimitives.TryConvertTo<ErrorCategory>(errorCategoryProperty.Value, CultureInfo.InvariantCulture, out errorCategoryValue))
            return errorCategoryValue;
    /// Write error to pipeline
    internal sealed class CimWriteError : CimSyncAction
        /// Initializes a new instance of the <see cref="CimWriteError"/> class
        /// with the specified <see cref="CimInstance"/>.
        public CimWriteError(CimInstance error, InvocationContext context)
            this.Error = error;
            this.CimInvocationContext = context;
        /// with the specified <see cref="Exception"/>.
        public CimWriteError(Exception exception, InvocationContext context, CimResultContext cimResultContext)
            this.Exception = exception;
            this.ResultContext = cimResultContext;
            Debug.Assert(cmdlet != null, "Caller should verify that cmdlet != null");
                Exception errorException = (Error != null) ? new CimException(Error) : this.Exception;
                // PS engine takes care of handling error action
                cmdlet.WriteError(ErrorToErrorRecord.ErrorRecordFromAnyException(this.CimInvocationContext, errorException, this.ResultContext));
                // if user wants to continue, we will get here
        /// Error instance
        internal CimInstance Error { get; }
        /// Exception object
        internal Exception Exception { get; }
        internal InvocationContext CimInvocationContext { get; }
        internal CimResultContext ResultContext { get; }

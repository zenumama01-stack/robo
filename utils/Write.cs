    #region WriteDebugCommand
    /// This class implements Write-Debug command.
    [Cmdlet(VerbsCommunications.Write, "Debug", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097132", RemotingCapability = RemotingCapability.None)]
    public sealed class WriteDebugCommand : PSCmdlet
        /// Message to be sent and processed if debug mode is on.
        [Alias("Msg")]
        /// This method implements the ProcessRecord method for Write-Debug command.
            // The write-debug command must use the script's InvocationInfo rather than its own,
            // so we create the DebugRecord here and fill it up with the appropriate InvocationInfo;
            // then, we call the command runtime directly and pass this record to WriteDebug().
            if (this.CommandRuntime is MshCommandRuntime mshCommandRuntime)
                DebugRecord record = new(Message);
                mshCommandRuntime.WriteDebug(record);
                WriteDebug(Message);
    #endregion WriteDebugCommand
    #region WriteVerboseCommand
    /// This class implements Write-Verbose command.
    [Cmdlet(VerbsCommunications.Write, "Verbose", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097043", RemotingCapability = RemotingCapability.None)]
    public sealed class WriteVerboseCommand : PSCmdlet
        /// Message to be sent if verbose messages are being shown.
        /// This method implements the ProcessRecord method for Write-verbose command.
            // The write-verbose command must use the script's InvocationInfo rather than its own,
            // so we create the VerboseRecord here and fill it up with the appropriate InvocationInfo;
            // then, we call the command runtime directly and pass this record to WriteVerbose().
                VerboseRecord record = new(Message);
                mshCommandRuntime.WriteVerbose(record);
                WriteVerbose(Message);
    #endregion WriteVerboseCommand
    #region WriteWarningCommand
    /// This class implements Write-Warning command.
    [Cmdlet(VerbsCommunications.Write, "Warning", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097044", RemotingCapability = RemotingCapability.None)]
    public sealed class WriteWarningCommand : PSCmdlet
        /// Message to be sent if warning messages are being shown.
        /// This method implements the ProcessRecord method for Write-Warning command.
            // The write-warning command must use the script's InvocationInfo rather than its own,
            // so we create the WarningRecord here and fill it up with the appropriate InvocationInfo;
            // then, we call the command runtime directly and pass this record to WriteWarning().
                WriteWarning(Message);
    #endregion WriteWarningCommand
    #region WriteInformationCommand
    /// This class implements Write-Information command.
    [Cmdlet(VerbsCommunications.Write, "Information", HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2097040", RemotingCapability = RemotingCapability.None)]
    public sealed class WriteInformationCommand : PSCmdlet
        /// Object to be sent to the Information stream.
        [Alias("Msg", "Message")]
        public object MessageData { get; set; }
        /// Any tags to be associated with this information.
        public string[] Tags { get; set; }
        /// This method implements the processing of the Write-Information command.
            if (Tags != null)
                foreach (string tag in Tags)
                    if (tag.StartsWith("PS", StringComparison.OrdinalIgnoreCase))
                        ErrorRecord er = new(
                            new InvalidOperationException(StringUtil.Format(UtilityCommonStrings.PSPrefixReservedInInformationTag, tag)),
                            "PSPrefixReservedInInformationTag", ErrorCategory.InvalidArgument, tag);
        /// This method implements the ProcessRecord method for Write-Information command.
            WriteInformation(MessageData, Tags);
    #endregion WriteInformationCommand
    #region WriteOrThrowErrorCommand
    /// This class implements the Write-Error command.
    public class WriteOrThrowErrorCommand : PSCmdlet
        /// ErrorRecord.Exception -- if not specified, ErrorRecord.Exception is System.Exception.
        [Parameter(Position = 0, ParameterSetName = "WithException", Mandatory = true)]
        public Exception Exception { get; set; }
        /// If Exception is specified, this is ErrorRecord.ErrorDetails.Message;
        /// otherwise, the Exception is System.Exception, and this is Exception.Message.
        [Parameter(Position = 0, ParameterSetName = "NoException", Mandatory = true, ValueFromPipeline = true)]
        [Parameter(ParameterSetName = "WithException")]
        [Parameter(Position = 0, ParameterSetName = "ErrorRecord", Mandatory = true)]
        public ErrorRecord ErrorRecord { get; set; }
        /// ErrorRecord.CategoryInfo.Category.
        [Parameter(ParameterSetName = "NoException")]
        public ErrorCategory Category { get; set; } = ErrorCategory.NotSpecified;
        /// ErrorRecord.ErrorId.
        public string ErrorId { get; set; } = string.Empty;
        /// ErrorRecord.TargetObject.
        public object TargetObject { get; set; }
        /// ErrorRecord.ErrorDetails.RecommendedAction.
        public string RecommendedAction { get; set; } = string.Empty;
        /* 2005/01/25 removing throw-error
        /// If true, this is throw-error.  Otherwise, this is write-error.
        internal bool _terminating = false;
        /// ErrorRecord.CategoryInfo.Activity.
        [Alias("Activity")]
        public string CategoryActivity { get; set; } = string.Empty;
        /// ErrorRecord.CategoryInfo.Reason.
        [Alias("Reason")]
        public string CategoryReason { get; set; } = string.Empty;
        /// ErrorRecord.CategoryInfo.TargetName.
        [Alias("TargetName")]
        public string CategoryTargetName { get; set; } = string.Empty;
        /// ErrorRecord.CategoryInfo.TargetType.
        [Alias("TargetType")]
        public string CategoryTargetType { get; set; } = string.Empty;
        /// Write an error to the output pipe, or throw a terminating error.
            ErrorRecord errorRecord = this.ErrorRecord;
            if (errorRecord != null)
                // copy constructor
                errorRecord = new ErrorRecord(errorRecord, null);
                Exception e = this.Exception;
                string msg = Message;
                e ??= new WriteErrorException(msg);
                string errid = ErrorId;
                if (string.IsNullOrEmpty(errid))
                    errid = e.GetType().FullName;
                    errid,
                    Category,
                    TargetObject
                if (this.Exception != null && !string.IsNullOrEmpty(msg))
                    errorRecord.ErrorDetails = new ErrorDetails(msg);
            string recact = RecommendedAction;
            if (!string.IsNullOrEmpty(recact))
                errorRecord.ErrorDetails ??= new ErrorDetails(errorRecord.ToString());
                errorRecord.ErrorDetails.RecommendedAction = recact;
            if (!string.IsNullOrEmpty(CategoryActivity))
                errorRecord.CategoryInfo.Activity = CategoryActivity;
            if (!string.IsNullOrEmpty(CategoryReason))
                errorRecord.CategoryInfo.Reason = CategoryReason;
            if (!string.IsNullOrEmpty(CategoryTargetName))
                errorRecord.CategoryInfo.TargetName = CategoryTargetName;
            if (!string.IsNullOrEmpty(CategoryTargetType))
                errorRecord.CategoryInfo.TargetType = CategoryTargetType;
            if (_terminating)
            // 2005/07/14-913791 "write-error output is confusing and misleading"
            // set InvocationInfo to the script not the command
            if (GetVariableValue(SpecialVariables.MyInvocation) is InvocationInfo myInvocation)
                errorRecord.SetInvocationInfo(myInvocation);
                    errorRecord.CategoryInfo.Activity = "Write-Error";
    /// This class implements Write-Error command.
    [Cmdlet(VerbsCommunications.Write, "Error", DefaultParameterSetName = "NoException",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097039", RemotingCapability = RemotingCapability.None)]
    public sealed class WriteErrorCommand : WriteOrThrowErrorCommand
        /// Initializes a new instance of the <see cref="WriteErrorCommand"/> class.
        public WriteErrorCommand()
        [Cmdlet("Throw", "Error", DefaultParameterSetName = "NoException")]
        public sealed class ThrowErrorCommand : WriteOrThrowErrorCommand
            public ThrowErrorCommand()
                using (tracer.TraceConstructor(this))
                    _terminating = true;
    #endregion WriteOrThrowErrorCommand
    #region WriteErrorException
    /// The write-error cmdlet uses WriteErrorException
    /// when the user only specifies a string and not
    /// an Exception or ErrorRecord.
    public class WriteErrorException : SystemException
        #region ctor
        /// Initializes a new instance of the <see cref="WriteErrorException"/> class.
        public WriteErrorException()
            : base(StringUtil.Format(WriteErrorStrings.WriteErrorException))
        public WriteErrorException(string message)
        public WriteErrorException(string message,
                                          Exception innerException)
        #endregion ctor
        /// Initializes a new instance of the <see cref="WriteErrorException"/> class for serialization.
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Streaming context.</param>
        protected WriteErrorException(SerializationInfo info,
    #endregion WriteErrorException

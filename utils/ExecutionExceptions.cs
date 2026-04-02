    #region CmdletInvocationException
    /// Indicates that a cmdlet hit a terminating error.
    /// InnerException is the error which the cmdlet hit.
    public class CmdletInvocationException : RuntimeException
        /// Instantiates a new instance of the CmdletInvocationException class.
        internal CmdletInvocationException(ErrorRecord errorRecord)
            : base(RetrieveMessage(errorRecord), RetrieveException(errorRecord))
                // 2005/04/13-JonN Can't do this in an unsealed class: HelpLink = errorRecord.Exception.HelpLink;
                // Exception.Source is set by Throw
                // Source = errorRecord.Exception.Source;
        /// <param name="innerException">Wrapped exception.</param>
        /// identity of cmdlet, null is unknown
        internal CmdletInvocationException(Exception innerException,
            : base(RetrieveMessage(innerException), innerException)
            ArgumentNullException.ThrowIfNull(innerException);
            // invocationInfo may be null
            if (innerException is IContainsErrorRecord icer && icer.ErrorRecord != null)
                _errorRecord = new ErrorRecord(icer.ErrorRecord, innerException);
                // When no ErrorId is specified by a thrown exception,
                //  we use innerException.GetType().FullName.
                    innerException.GetType().FullName,
            _errorRecord.SetInvocationInfo(invocationInfo);
            // 2005/04/13-JonN Can't do this in an unsealed class: HelpLink = innerException.HelpLink;
            // Source = innerException.Source;
        public CmdletInvocationException()
        public CmdletInvocationException(string message)
        public CmdletInvocationException(string message,
        /// Initializes a new instance of the CmdletInvocationException class
        protected CmdletInvocationException(SerializationInfo info,
        /// The error reported by the cmdlet.
                    "CmdletInvocationException",
        private ErrorRecord _errorRecord = null;
    #endregion CmdletInvocationException
    #region CmdletProviderInvocationException
    /// Indicates that a cmdlet hit a terminating error of type
    /// <see cref="System.Management.Automation.ProviderInvocationException"/>.
    /// This is generally reported from the standard provider navigation cmdlets
    /// such as get-childitem.
    public class CmdletProviderInvocationException : CmdletInvocationException
        /// Instantiates a new instance of the CmdletProviderInvocationException class.
        /// <param name="myInvocation">
        internal CmdletProviderInvocationException(
                    ProviderInvocationException innerException,
                    InvocationInfo myInvocation)
            : base(GetInnerException(innerException), myInvocation)
            _providerInvocationException = innerException;
        public CmdletProviderInvocationException()
        /// Initializes a new instance of the CmdletProviderInvocationException class
        protected CmdletProviderInvocationException(SerializationInfo info,
        public CmdletProviderInvocationException(string message)
        public CmdletProviderInvocationException(string message,
            _providerInvocationException = innerException as ProviderInvocationException;
        /// InnerException as ProviderInvocationException.
        /// <value>ProviderInvocationException</value>
        public ProviderInvocationException ProviderInvocationException
                return _providerInvocationException;
        private readonly ProviderInvocationException _providerInvocationException;
        /// This is the ProviderInfo associated with the provider which
        /// generated the error.
        public ProviderInfo ProviderInfo
                return _providerInvocationException?.ProviderInfo;
        private static Exception GetInnerException(Exception e)
            return e?.InnerException;
    #endregion CmdletProviderInvocationException
    #region PipelineStoppedException
    /// Indicates that the pipeline has already been stopped.
    /// When reported as the result of a command, PipelineStoppedException
    /// indicates that the command was stopped asynchronously, either by the
    /// user hitting CTRL-C, or by a call to
    /// <see cref="System.Management.Automation.Runspaces.Pipeline.Stop"/>.
    /// When a cmdlet or provider sees this exception thrown from a PowerShell API such as
    ///     WriteObject(object)
    /// this means that the command was already stopped.  The cmdlet or provider
    /// should clean up and return.
    /// Catching this exception is optional; if the cmdlet or providers chooses not to
    /// handle PipelineStoppedException and instead allow it to propagate to the
    /// PowerShell Engine's call to ProcessRecord, the PowerShell Engine will handle it properly.
    public class PipelineStoppedException : RuntimeException
        /// Instantiates a new instance of the PipelineStoppedException class.
        public PipelineStoppedException()
            : base(GetErrorText.PipelineStoppedException)
            SetErrorId("PipelineStopped");
            SetErrorCategory(ErrorCategory.OperationStopped);
        /// Initializes a new instance of the PipelineStoppedException class
        protected PipelineStoppedException(SerializationInfo info,
        public PipelineStoppedException(string message)
        public PipelineStoppedException(string message,
    #endregion PipelineStoppedException
    #region PipelineClosedException
    /// PipelineClosedException occurs when someone tries to write
    /// to an asynchronous pipeline source and the pipeline has already
    /// been stopped.
    /// <seealso cref="System.Management.Automation.Runspaces.Pipeline.Input"/>
    public class PipelineClosedException : RuntimeException
        /// Instantiates a new instance of the PipelineClosedException class.
        public PipelineClosedException()
        public PipelineClosedException(string message)
        public PipelineClosedException(string message,
        /// Initializes a new instance of the PipelineClosedException class
        protected PipelineClosedException(SerializationInfo info,
    #endregion PipelineClosedException
    #region ActionPreferenceStopException
    /// ActionPreferenceStopException indicates that the command stopped due
    /// to the ActionPreference.Stop or Inquire policy.
    /// For example, if $WarningPreference is "Stop", the command will fail with
    /// this error if a cmdlet calls WriteWarning.
    public class ActionPreferenceStopException : RuntimeException
        /// Instantiates a new instance of the ActionPreferenceStopException class.
        public ActionPreferenceStopException()
            : this(GetErrorText.ActionPreferenceStop)
        /// Non-terminating error which triggered the Stop
        internal ActionPreferenceStopException(ErrorRecord error)
            : this(RetrieveMessage(error))
            ArgumentNullException.ThrowIfNull(error);
            _errorRecord = error;
        internal ActionPreferenceStopException(InvocationInfo invocationInfo, string message)
            : this(message)
            base.ErrorRecord.SetInvocationInfo(invocationInfo);
        internal ActionPreferenceStopException(InvocationInfo invocationInfo,
            : this(invocationInfo, message)
        /// Initializes a new instance of the ActionPreferenceStopException class
        protected ActionPreferenceStopException(SerializationInfo info,
        public ActionPreferenceStopException(string message)
            SetErrorId("ActionPreferenceStop");
            // fix for BUG: Windows Out Of Band Releases: 906263 and 906264
            this.SuppressPromptInInterpreter = true;
        public ActionPreferenceStopException(string message,
        /// See <see cref="System.Management.Automation.IContainsErrorRecord"/>
        /// <value>ErrorRecord</value>
        /// If this error results from a non-terminating error being promoted to
        /// terminating due to -ErrorAction or $ErrorActionPreference, this is
        /// the non-terminating error.
            get { return _errorRecord ?? base.ErrorRecord; }
        private readonly ErrorRecord _errorRecord = null;
    #endregion ActionPreferenceStopException
    #region ParentContainsErrorRecordException
    /// ParentContainsErrorRecordException is the exception contained by the ErrorRecord
    /// which is associated with a PowerShell engine custom exception through
    /// the IContainsErrorRecord interface.
    /// We use this exception class
    /// so that there is not a recursive "containment" relationship
    /// between the PowerShell engine exception and its ErrorRecord.
    public class ParentContainsErrorRecordException : SystemException
        /// Instantiates a new instance of the ParentContainsErrorRecordException class.
        /// Note that this sets the Message and not the InnerException.
        /// I leave this non-standard constructor form public.
        // BUGBUG : We should check whether wrapperException is not null.
        // Please remove the #pragma warning when this is fixed.
        public ParentContainsErrorRecordException(Exception wrapperException)
            _wrapperException = wrapperException;
        public ParentContainsErrorRecordException(string message)
        public ParentContainsErrorRecordException()
        public ParentContainsErrorRecordException(string message,
        /// Initializes a new instance of the ParentContainsErrorRecordException class
        /// <exception cref="NotImplementedException">Always.</exception>
        protected ParentContainsErrorRecordException(
            SerializationInfo info, StreamingContext context)
        /// Gets the message for the exception.
                return _message ??= (_wrapperException != null) ? _wrapperException.Message : string.Empty;
        private readonly Exception _wrapperException;
    #endregion ParentContainsErrorRecordException
    #region RedirectedException
    /// Indicates that a success object was written and success-to-error ("1>&amp;2")
    /// has been specified.
    /// The redirected object is available as
    /// <see cref="System.Management.Automation.ErrorRecord.TargetObject"/>
    /// in the ErrorRecord which contains this exception.
    public class RedirectedException : RuntimeException
        /// Instantiates a new instance of the RedirectedException class.
        public RedirectedException()
            SetErrorId("RedirectedException");
        public RedirectedException(string message)
        public RedirectedException(string message,
        /// Initializes a new instance of the RedirectedException class
        protected RedirectedException(SerializationInfo info,
    #endregion RedirectedException
    #region ScriptCallDepthException
    /// ScriptCallDepthException occurs when the number of
    /// session state objects of this type in this scope
    /// exceeds the configured maximum.
    /// When one PowerShell command or script calls another, this creates an additional
    /// scope.  Some script expressions also create a scope.  PowerShell imposes a maximum
    /// call depth to prevent stack overflows.  The maximum call depth is configurable
    /// but generally high enough that scripts which are not deeply recursive
    /// should not have a problem.
    public class ScriptCallDepthException : SystemException, IContainsErrorRecord
        /// Instantiates a new instance of the ScriptCallDepthException class.
        public ScriptCallDepthException()
            : base(GetErrorText.ScriptCallDepthException)
        public ScriptCallDepthException(string message)
        public ScriptCallDepthException(string message,
        /// Initializes a new instance of the ScriptCallDepthException class
        protected ScriptCallDepthException(SerializationInfo info,
        /// TargetObject is the offending call depth
                    "CallDepthOverflow",
                    CallDepth);
        /// Always 0 - depth is not tracked as there is no hard coded maximum.
        public int CallDepth
    #endregion ScriptCallDepthException
    #region PipelineDepthException
    /// PipelineDepthException occurs when the number of
    /// commands participating in a pipeline (object streaming)
    public class PipelineDepthException : SystemException, IContainsErrorRecord
        /// Instantiates a new instance of the PipelineDepthException class.
        public PipelineDepthException()
            : base(GetErrorText.PipelineDepthException)
        public PipelineDepthException(string message)
        public PipelineDepthException(string message,
        /// Initializes a new instance of the PipelineDepthException class
        protected PipelineDepthException(SerializationInfo info,
    #region HaltCommandException
    /// A cmdlet/provider should throw HaltCommandException
    /// when it wants to terminate the running command without
    /// this being considered an error.
    /// For example, "more" will throw HaltCommandException if the user hits "q".
    /// Only throw HaltCommandException from your implementation of ProcessRecord etc.
    /// Note that HaltCommandException does not define IContainsErrorRecord.
    /// This is because it is not reported to the user.
    public class HaltCommandException : SystemException
        /// Instantiates a new instance of the HaltCommandException class.
        public HaltCommandException()
            : base(StringUtil.Format(AutomationExceptions.HaltCommandException))
        public HaltCommandException(string message)
        public HaltCommandException(string message,
        /// Initializes a new instance of the HaltCommandException class
        protected HaltCommandException(SerializationInfo info,
    #endregion HaltCommandException

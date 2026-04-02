using System.Management.Automation.Remoting.Internal;
    /// Base class for all child jobs that wrap CIM operations.
    internal abstract class CimChildJobBase<T> :
        StartableJob,
        IObserver<T>
        private static long s_globalJobNumberCounter;
        private readonly long _myJobNumber = Interlocked.Increment(ref s_globalJobNumberCounter);
        internal CimJobContext JobContext
                return _jobContext;
        private readonly CimJobContext _jobContext;
        internal CimChildJobBase(CimJobContext jobContext)
            : base(Job.GetCommandTextFromInvocationInfo(jobContext.CmdletInvocationInfo), " " /* temporary name - reset below */)
            _jobContext = jobContext;
            this.PSJobTypeName = CIMJobType;
            this.Name = this.GetType().Name + _myJobNumber.ToString(CultureInfo.InvariantCulture);
            UsesResultsCollection = true;
            lock (s_globalRandom)
                _random = new Random(s_globalRandom.Next());
            _jobSpecificCustomOptions = new Lazy<CimCustomOptionsDictionary>(this.CalculateJobSpecificCustomOptions);
        private readonly CimSensitiveValueConverter _cimSensitiveValueConverter = new();
        internal CimSensitiveValueConverter CimSensitiveValueConverter { get { return _cimSensitiveValueConverter; } }
        internal abstract IObservable<T> GetCimOperation();
        public abstract void OnNext(T item);
        // copied from sdpublic\sdk\inc\wsmerror.h
        private enum WsManErrorCode : uint
            ERROR_WSMAN_QUOTA_MAX_SHELLS = 0x803381A5,
            ERROR_WSMAN_QUOTA_MAX_OPERATIONS = 0x803381A6,
            ERROR_WSMAN_QUOTA_USER = 0x803381A7,
            ERROR_WSMAN_QUOTA_SYSTEM = 0x803381A8,
            ERROR_WSMAN_QUOTA_MAX_SHELLUSERS = 0x803381AB,
            ERROR_WSMAN_QUOTA_MAX_SHELLS_PPQ = 0x803381E4,
            ERROR_WSMAN_QUOTA_MAX_USERS_PPQ = 0x803381E5,
            ERROR_WSMAN_QUOTA_MAX_PLUGINSHELLS_PPQ = 0x803381E6,
            ERROR_WSMAN_QUOTA_MAX_PLUGINOPERATIONS_PPQ = 0x803381E7,
            ERROR_WSMAN_QUOTA_MAX_OPERATIONS_USER_PPQ = 0x803381E8,
            ERROR_WSMAN_QUOTA_MAX_COMMANDS_PER_SHELL_PPQ = 0x803381E9,
            ERROR_WSMAN_QUOTA_MIN_REQUIREMENT_NOT_AVAILABLE_PPQ = 0x803381EA,
        private static bool IsWsManQuotaReached(Exception exception)
            if (exception is not CimException cimException)
            if (cimException.NativeErrorCode != NativeErrorCode.ServerLimitsExceeded)
            CimInstance cimError = cimException.ErrorData;
            CimProperty errorCodeProperty = cimError.CimInstanceProperties["error_Code"];
            if (errorCodeProperty == null)
            if (errorCodeProperty.CimType != CimType.UInt32)
            WsManErrorCode wsManErrorCode = (WsManErrorCode)(uint)(errorCodeProperty.Value);
            switch (wsManErrorCode) // error codes that should result in sleep-and-retry are based on an email from Ryan
                case WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_SHELLS:
                case WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_OPERATIONS:
                case WsManErrorCode.ERROR_WSMAN_QUOTA_USER:
                case WsManErrorCode.ERROR_WSMAN_QUOTA_SYSTEM:
                case WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_SHELLUSERS:
                case WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_SHELLS_PPQ:
                case WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_USERS_PPQ:
                case WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_PLUGINSHELLS_PPQ:
                case WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_PLUGINOPERATIONS_PPQ:
                case WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_OPERATIONS_USER_PPQ:
                case WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_COMMANDS_PER_SHELL_PPQ:
                case WsManErrorCode.ERROR_WSMAN_QUOTA_MIN_REQUIREMENT_NOT_AVAILABLE_PPQ:
        public virtual void OnError(Exception exception)
                        if (IsWsManQuotaReached(exception))
                            this.SleepAndRetry();
                        var cje = CimJobException.CreateFromAnyException(this.GetDescription(), this.JobContext, exception);
                        this.ReportJobFailure(cje);
        private static readonly Random s_globalRandom = new();
        private readonly Random _random;
        private int _sleepAndRetryDelayRangeMs = 1000;
        private int _sleepAndRetryExtraDelayMs = 0;
        private const int MaxRetryDelayMs = 15 * 1000;
        private const int MinRetryDelayMs = 100;
        private Timer _sleepAndRetryTimer;
        private void SleepAndRetry_OnWakeup(object state)
                        lock (_jobStateLock)
                            if (_sleepAndRetryTimer != null)
                                _sleepAndRetryTimer.Dispose();
                                _sleepAndRetryTimer = null;
                            if (_jobWasStopped)
                                this.SetCompletedJobState(JobState.Stopped, null);
                        this.StartJob();
        private void SleepAndRetry()
            int tmpRandomDelay = _random.Next(0, _sleepAndRetryDelayRangeMs);
            int delay = MinRetryDelayMs + _sleepAndRetryExtraDelayMs + tmpRandomDelay;
            _sleepAndRetryExtraDelayMs = _sleepAndRetryDelayRangeMs - tmpRandomDelay;
            if (_sleepAndRetryDelayRangeMs < MaxRetryDelayMs)
                _sleepAndRetryDelayRangeMs *= 2;
            string verboseMessage = string.Format(
                CmdletizationResources.CimJob_SleepAndRetryVerboseMessage,
                this.JobContext.CmdletInvocationInfo.InvocationName,
                this.JobContext.Session.ComputerName ?? "localhost",
                delay / 1000.0);
            this.WriteVerbose(verboseMessage);
                    Dbg.Assert(_sleepAndRetryTimer == null, "There should be only 1 active _sleepAndRetryTimer");
                    _sleepAndRetryTimer = new Timer(
                        state: null,
                        dueTime: delay,
                        period: Timeout.Infinite,
                        callback: SleepAndRetry_OnWakeup);
        /// Indicates a location where this job is running.
        public override string Location
                // this.JobContext is set in the constructor of CimChildJobBase,
                // but the constructor of Job wants to access Location property
                // before CimChildJobBase is fully initialized
                if (this.JobContext == null)
                string location = this.JobContext.Session.ComputerName ?? Environment.MachineName;
                return location;
        /// Status message associated with the Job.
        public override string StatusMessage
                return this.JobStateInfo.State.ToString();
        /// Indicates if job has more data available.
        public override bool HasMoreData
                return (Results.IsOpen || Results.Count > 0);
        internal void WriteVerboseStartOfCimOperation()
            if (this.JobContext.CmdletInvocationContext.CmdletDefinitionContext.ClientSideWriteVerbose)
                    CmdletizationResources.CimJob_VerboseExecutionMessage,
                    this.GetDescription());
        internal override void StartJob()
                Dbg.Assert(!_alreadyReachedCompletedState, "Job shouldn't reach completed state, before ThrottlingJob has a chance to register for job-completed/failed events");
                TerminatingErrorTracker tracker = TerminatingErrorTracker.GetTracker(this.JobContext.CmdletInvocationInfo);
                if (tracker.IsSessionTerminated(this.JobContext.Session))
                    this.SetCompletedJobState(JobState.Failed, new OperationCanceledException());
                if (!_jobWasStarted)
                    _jobWasStarted = true;
                    this.SetJobState(JobState.Running);
            // This invocation can block (i.e. by calling Job.ShouldProcess) and wait for pipeline thread to unblock it
            // Therefore we have to do the invocation outside of the pipeline thread.
            ThreadPool.QueueUserWorkItem(delegate
                this.ExceptionSafeWrapper(delegate
                    IObservable<T> observable = this.GetCimOperation();
                    observable?.Subscribe(this);
        internal string GetDescription()
                return this.Description;
        internal abstract string Description { get; }
        internal abstract string FailSafeDescription { get; }
        internal void ExceptionSafeWrapper(Action action)
                    Dbg.Assert(action != null, "Caller should verify action != null");
                    action();
                catch (CimJobException e)
                    this.ReportJobFailure(e);
                catch (PSInvalidCastException e)
                    var cje = CimJobException.CreateFromCimException(this.GetDescription(), this.JobContext, e);
                catch (PSInvalidOperationException)
                        bool everythingIsOk = false;
                            everythingIsOk = true;
                        if (_alreadyReachedCompletedState && _jobHadErrors)
                        if (!everythingIsOk)
                            Dbg.Assert(false, "PSInvalidOperationException should only happen in certain job states");
                var cje = CimJobException.CreateFromAnyException(this.GetDescription(), this.JobContext, e);
        #region Operation options
        internal virtual string GetProviderVersionExpectedByJob()
            return this.JobContext.CmdletInvocationContext.CmdletDefinitionContext.CmdletizationClassVersion;
        internal CimOperationOptions CreateOperationOptions()
            var operationOptions = new CimOperationOptions(mustUnderstand: false)
                CancellationToken = _cancellationTokenSource.Token,
                WriteProgress = this.WriteProgressCallback,
                WriteMessage = this.WriteMessageCallback,
                WriteError = this.WriteErrorCallback,
                PromptUser = this.PromptUserCallback,
            operationOptions.SetOption("__MI_OPERATIONOPTIONS_IMPROVEDPERF_STREAMING", 1);
            operationOptions.Flags |= this.JobContext.CmdletInvocationContext.CmdletDefinitionContext.SchemaConformanceLevel;
            if (this.JobContext.CmdletInvocationContext.CmdletDefinitionContext.ResourceUri != null)
                operationOptions.ResourceUri = this.JobContext.CmdletInvocationContext.CmdletDefinitionContext.ResourceUri;
            if ((
                  (_jobContext.WarningActionPreference == ActionPreference.SilentlyContinue) ||
                  (_jobContext.WarningActionPreference == ActionPreference.Ignore)
                ) && (!_jobContext.IsRunningInBackground))
                operationOptions.DisableChannel((uint)MessageChannel.Warning);
                operationOptions.EnableChannel((uint)MessageChannel.Warning);
                  (_jobContext.VerboseActionPreference == ActionPreference.SilentlyContinue) ||
                  (_jobContext.VerboseActionPreference == ActionPreference.Ignore)
                operationOptions.DisableChannel((uint)MessageChannel.Verbose);
                operationOptions.EnableChannel((uint)MessageChannel.Verbose);
                  (_jobContext.DebugActionPreference == ActionPreference.SilentlyContinue) ||
                  (_jobContext.DebugActionPreference == ActionPreference.Ignore)
                operationOptions.DisableChannel((uint)MessageChannel.Debug);
                operationOptions.EnableChannel((uint)MessageChannel.Debug);
            switch (this.JobContext.ShouldProcessOptimization)
                case MshCommandRuntime.ShouldProcessPossibleOptimization.AutoNo_CanCallShouldProcessAsynchronously:
                    operationOptions.SetPromptUserRegularMode(CimCallbackMode.Report, automaticConfirmation: false);
                case MshCommandRuntime.ShouldProcessPossibleOptimization.AutoYes_CanCallShouldProcessAsynchronously:
                    operationOptions.SetPromptUserRegularMode(CimCallbackMode.Report, automaticConfirmation: true);
                case MshCommandRuntime.ShouldProcessPossibleOptimization.AutoYes_CanSkipShouldProcessCall:
                    operationOptions.SetPromptUserRegularMode(CimCallbackMode.Ignore, automaticConfirmation: true);
                case MshCommandRuntime.ShouldProcessPossibleOptimization.NoOptimizationPossible:
                    operationOptions.PromptUserMode = CimCallbackMode.Inquire;
            switch (this.JobContext.ErrorActionPreference)
                case ActionPreference.Continue:
                case ActionPreference.SilentlyContinue:
                case ActionPreference.Ignore:
                    operationOptions.WriteErrorMode = CimCallbackMode.Report;
                case ActionPreference.Stop:
                case ActionPreference.Inquire:
                    operationOptions.WriteErrorMode = CimCallbackMode.Inquire;
            if (!string.IsNullOrWhiteSpace(this.GetProviderVersionExpectedByJob()))
                CimOperationOptionsHelper.SetCustomOption(
                    operationOptions,
                    "MI_OPERATIONOPTIONS_PROVIDERVERSION",
                    this.GetProviderVersionExpectedByJob(),
                    CimSensitiveValueConverter);
            if (this.JobContext.CmdletizationModuleVersion != null)
                    "MI_OPERATIONOPTIONS_POWERSHELL_MODULEVERSION",
                    this.JobContext.CmdletizationModuleVersion,
                "MI_OPERATIONOPTIONS_POWERSHELL_CMDLETNAME",
                this.JobContext.CmdletInvocationInfo.MyCommand.Name,
            if (!string.IsNullOrWhiteSpace(this.JobContext.Session.ComputerName))
                    "MI_OPERATIONOPTIONS_POWERSHELL_COMPUTERNAME",
            CimCustomOptionsDictionary jobSpecificCustomOptions = this.GetJobSpecificCustomOptions();
            jobSpecificCustomOptions?.Apply(operationOptions, CimSensitiveValueConverter);
            return operationOptions;
        private readonly Lazy<CimCustomOptionsDictionary> _jobSpecificCustomOptions;
        internal abstract CimCustomOptionsDictionary CalculateJobSpecificCustomOptions();
        private CimCustomOptionsDictionary GetJobSpecificCustomOptions()
            return _jobSpecificCustomOptions.Value;
        #region Controlling job state
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        /// Stops this job.
        public override void StopJob()
                if (_jobWasStopped || _alreadyReachedCompletedState)
                _jobWasStopped = true;
                else if (_sleepAndRetryTimer != null)
                    this.SetJobState(JobState.Stopping);
            _cancellationTokenSource.Cancel();
        private readonly object _jobStateLock = new();
        private bool _jobHadErrors;
        private bool _jobWasStarted;
        private bool _jobWasStopped;
        private bool _alreadyReachedCompletedState;
        internal bool JobHadErrors
                    return _jobHadErrors;
        internal void ReportJobFailure(IContainsErrorRecord exception)
            TerminatingErrorTracker terminatingErrorTracker = TerminatingErrorTracker.GetTracker(this.JobContext.CmdletInvocationInfo);
            bool sessionWasAlreadyTerminated = false;
            bool isThisTerminatingError = false;
            Exception brokenSessionException = null;
                if (!_jobWasStopped)
                    brokenSessionException = terminatingErrorTracker.GetExceptionIfBrokenSession(
                        this.JobContext.Session,
                        this.JobContext.CmdletInvocationContext.CmdletDefinitionContext.SkipTestConnection,
                        out sessionWasAlreadyTerminated);
            if (brokenSessionException != null)
                string brokenSessionMessage = string.Format(
                    CmdletizationResources.CimJob_BrokenSession,
                    brokenSessionException.Message);
                exception = CimJobException.CreateWithFullControl(
                    brokenSessionMessage,
                    "CimJob_BrokenCimSession",
                    ErrorCategory.ResourceUnavailable,
                    brokenSessionException);
                isThisTerminatingError = true;
                if ((exception is CimJobException cje) && (cje.IsTerminatingError))
                    terminatingErrorTracker.MarkSessionAsTerminated(this.JobContext.Session, out sessionWasAlreadyTerminated);
            bool writeError = !sessionWasAlreadyTerminated;
            if (writeError)
                        writeError = false;
            ErrorRecord errorRecord = exception.ErrorRecord;
            errorRecord.SetInvocationInfo(this.JobContext.CmdletInvocationInfo);
            errorRecord.PreserveInvocationInfoOnce = true;
                    if (!_alreadyReachedCompletedState)
                        if (isThisTerminatingError)
                            this.Error.Add(errorRecord);
                            CmdletMethodInvoker<bool> methodInvoker = terminatingErrorTracker.GetErrorReportingDelegate(errorRecord);
                            this.Results.Add(new PSStreamObject(PSStreamObjectType.ShouldMethod, methodInvoker));
                            this.WriteError(errorRecord);
            this.SetCompletedJobState(JobState.Failed, errorRecord.Exception);
        internal override void WriteWarning(string message)
            message = this.JobContext.PrependComputerNameToMessage(message);
            base.WriteWarning(message);
        internal override void WriteVerbose(string message)
            base.WriteVerbose(message);
        internal override void WriteDebug(string message)
            base.WriteDebug(message);
        internal void SetCompletedJobState(JobState state, Exception reason)
                if (_alreadyReachedCompletedState)
                _alreadyReachedCompletedState = true;
                if ((state == JobState.Failed) || (reason != null))
                    _jobHadErrors = true;
                    state = JobState.Stopped;
                else if (_jobHadErrors)
                    state = JobState.Failed;
            this.FinishProgressReporting();
            this.SetJobState(state, reason);
            this.CloseAllStreams();
        #region Support for progress reporting
        private readonly ConcurrentDictionary<int, ProgressRecord> _activityIdToLastProgressRecord = new();
        internal override void WriteProgress(ProgressRecord progressRecord)
            progressRecord.Activity = this.JobContext.PrependComputerNameToMessage(progressRecord.Activity);
            _activityIdToLastProgressRecord.AddOrUpdate(
                progressRecord.ActivityId,
                progressRecord,
                (activityId, oldProgressRecord) => progressRecord);
            base.WriteProgress(progressRecord);
        internal void FinishProgressReporting()
            foreach (ProgressRecord lastProgressRecord in _activityIdToLastProgressRecord.Values)
                if (lastProgressRecord.RecordType != ProgressRecordType.Completed)
                    var newProgressRecord = new ProgressRecord(lastProgressRecord.ActivityId, lastProgressRecord.Activity, lastProgressRecord.StatusDescription);
                    newProgressRecord.RecordType = ProgressRecordType.Completed;
                    newProgressRecord.PercentComplete = 100;
                    newProgressRecord.SecondsRemaining = 0;
                    this.WriteProgress(newProgressRecord);
        #region Handling extended semantics callbacks
        private void WriteProgressCallback(string activity, string currentOperation, string statusDescription, uint percentageCompleted, uint secondsRemaining)
            if (string.IsNullOrEmpty(activity))
                activity = this.GetDescription();
            if (string.IsNullOrEmpty(statusDescription))
                statusDescription = this.StatusMessage;
            int signedSecondsRemaining;
            if (secondsRemaining == uint.MaxValue)
                signedSecondsRemaining = -1;
            else if (secondsRemaining <= int.MaxValue)
                signedSecondsRemaining = (int)secondsRemaining;
                signedSecondsRemaining = int.MaxValue;
            int signedPercentageComplete;
            if (percentageCompleted == uint.MaxValue)
                signedPercentageComplete = -1;
            else if (percentageCompleted <= 100)
                signedPercentageComplete = (int)percentageCompleted;
                signedPercentageComplete = 100;
            var progressRecord = new ProgressRecord(unchecked((int)(_myJobNumber % int.MaxValue)), activity, statusDescription)
                CurrentOperation = currentOperation,
                PercentComplete = signedPercentageComplete,
                SecondsRemaining = signedSecondsRemaining,
                RecordType = ProgressRecordType.Processing,
                        this.WriteProgress(progressRecord);
        private enum MessageChannel
            Warning = 0,
            Verbose = 1,
            Debug = 2,
        private void WriteMessageCallback(uint channel, string message)
                        switch ((MessageChannel)channel)
                            case MessageChannel.Warning:
                                this.WriteWarning(message);
                            case MessageChannel.Verbose:
                                this.WriteVerbose(message);
                            case MessageChannel.Debug:
                                this.WriteDebug(message);
                                Dbg.Assert(false, "We shouldn't get messages in channels that we didn't register for");
        private CimResponseType BlockingWriteError(ErrorRecord errorRecord)
            Exception exceptionThrownOnCmdletThread = null;
                        this.WriteError(errorRecord, out exceptionThrownOnCmdletThread);
            return (exceptionThrownOnCmdletThread != null)
                       ? CimResponseType.NoToAll
                       : CimResponseType.Yes;
        private CimResponseType WriteErrorCallback(CimInstance cimError)
            var cimException = new CimException(cimError);
            var jobException = CimJobException.CreateFromCimException(this.GetDescription(), this.JobContext, cimException);
            var errorRecord = jobException.ErrorRecord;
                    return this.BlockingWriteError(errorRecord);
                    return CimResponseType.Yes;
        private bool _userWasPromptedForContinuationOfProcessing;
        private bool _userRespondedYesToAtLeastOneShouldProcess;
        internal bool DidUserSuppressTheOperation
                bool didUserSuppressTheOperation = _userWasPromptedForContinuationOfProcessing && (!_userRespondedYesToAtLeastOneShouldProcess);
                return didUserSuppressTheOperation;
        internal CimResponseType ShouldProcess(string target, string action)
            string verboseDescription = StringUtil.Format(CommandBaseStrings.ShouldProcessMessage,
                action,
                target,
            return ShouldProcess(verboseDescription, null, null);
        internal CimResponseType ShouldProcess(
            string verboseDescription,
            string verboseWarning,
            string caption)
            if (this.JobContext.IsRunningInBackground)
                return CimResponseType.YesToAll;
            if (this.JobContext.ShouldProcessOptimization == MshCommandRuntime.ShouldProcessPossibleOptimization.AutoNo_CanCallShouldProcessAsynchronously)
                this.NonblockingShouldProcess(verboseDescription, verboseWarning, caption);
                return CimResponseType.No;
            if (this.JobContext.ShouldProcessOptimization == MshCommandRuntime.ShouldProcessPossibleOptimization.AutoYes_CanCallShouldProcessAsynchronously)
                (this.JobContext.ShouldProcessOptimization != MshCommandRuntime.ShouldProcessPossibleOptimization.AutoYes_CanSkipShouldProcessCall) ||
                (this.JobContext.CmdletInvocationContext.CmdletDefinitionContext.ClientSideShouldProcess),
                "MI layer should not call us when AutoYes_CanSkipShouldProcessCall optimization is in effect");
            Exception exceptionThrownOnCmdletThread;
            ShouldProcessReason shouldProcessReason;
            bool shouldProcessResponse = this.ShouldProcess(verboseDescription, verboseWarning, caption, out shouldProcessReason, out exceptionThrownOnCmdletThread);
            if (exceptionThrownOnCmdletThread != null)
            else if (shouldProcessResponse)
        private CimResponseType PromptUserCallback(string message, CimPromptType promptType)
            CimResponseType result = CimResponseType.No;
            _userWasPromptedForContinuationOfProcessing = true;
            switch (promptType)
                                if (this.ShouldContinue(message, null, out exceptionThrownOnCmdletThread))
                                    result = CimResponseType.Yes;
                                    result = CimResponseType.No;
                                result = this.ShouldProcess(message, null, null);
                    Dbg.Assert(false, "Unrecognized CimPromptType");
                result = CimResponseType.NoToAll;
            if ((result == CimResponseType.Yes) || (result == CimResponseType.YesToAll))
                _userRespondedYesToAtLeastOneShouldProcess = true;
        internal static bool IsShowComputerNameMarkerPresent(CimInstance cimInstance)
            PSObject pso = PSObject.AsPSObject(cimInstance);
            if (pso.InstanceMembers[RemotingConstants.ShowComputerNameNoteProperty] is not PSPropertyInfo psShowComputerNameProperty)
            return true.Equals(psShowComputerNameProperty.Value);
        internal static void AddShowComputerNameMarker(PSObject pso)
            if (pso.InstanceMembers[RemotingConstants.ShowComputerNameNoteProperty] is PSPropertyInfo psShowComputerNameProperty)
                psShowComputerNameProperty.Value = true;
                psShowComputerNameProperty = new PSNoteProperty(RemotingConstants.ShowComputerNameNoteProperty, true);
                pso.InstanceMembers.Add(psShowComputerNameProperty);
            PSObject pso = null;
            if (outputObject is PSObject)
                pso = PSObject.AsPSObject(outputObject);
                cimInstance = pso.BaseObject as CimInstance;
                cimInstance = outputObject as CimInstance;
            if (cimInstance != null)
                CimCustomOptionsDictionary.AssociateCimInstanceWithCustomOptions(cimInstance, this.GetJobSpecificCustomOptions());
            if (this.JobContext.ShowComputerName)
                pso ??= PSObject.AsPSObject(outputObject);
                if (cimInstance == null)
                    pso.Properties.Add(new PSNoteProperty(RemotingConstants.ComputerNameNoteProperty, this.JobContext.Session.ComputerName));
                bool isCompleted;
                    isCompleted = _alreadyReachedCompletedState;
                if (!isCompleted)
                    this.StopJob();
                    this.Finished.WaitOne();
                _cimSensitiveValueConverter.Dispose();
                _cancellationTokenSource.Dispose();

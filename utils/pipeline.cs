    /// Loads InternalCommand objects and executes them.
    /// The PipelineProcessor class is not thread-safe, so methods such as
    /// AddCommand and SynchronousExecute should not be called
    /// simultaneously.  While SynchronousExecute is running, it may access
    /// ExternalInput, ExternalSuccessOutput and ExternalErrorOutput, and
    /// those objects are thread-safe.
    internal class PipelineProcessor : IDisposable
        private readonly CancellationTokenSource _pipelineStopTokenSource = new CancellationTokenSource();
        private List<CommandProcessorBase> _commands = new List<CommandProcessorBase>();
        private List<PipelineProcessor> _redirectionPipes;
        private PipelineReader<object> _externalInputPipe;
        private PipelineWriter _externalSuccessOutput;
        private PipelineWriter _externalErrorOutput;
        private bool _executionStarted = false;
        private SessionStateScope _executionScope;
        private ExceptionDispatchInfo _firstTerminatingError = null;
        private bool _linkedSuccessOutput = false;
        private bool _linkedErrorOutput = false;
        private NativeCommandProcessor _lastNativeCommand;
        private bool _haveReportedNativePipeUsage;
#if !CORECLR // Impersonation Not Supported On CSS
        // This is the security context when the pipeline was allocated
        internal System.Security.SecurityContext SecurityContext =
            System.Security.SecurityContext.Capture();
        /// When the command is complete, PipelineProcessor will be
        /// disposed.
        /// This is only public because it implements an interface method.
        /// The class itself is internal.
        /// We use the standard IDispose pattern.
                DisposeCommands();
                _localPipeline = null;
                _externalSuccessOutput = null;
                _externalErrorOutput = null;
                _executionScope = null;
                _eventLogBuffer = null;
                _pipelineStopTokenSource.Dispose();
                SecurityContext.Dispose();
                SecurityContext = null;
        #region Execution Logging
        private bool _executionFailed = false;
        internal List<CommandProcessorBase> Commands
            get { return _commands; }
        internal bool ExecutionFailed
                return _executionFailed;
                _executionFailed = value;
        internal CancellationToken PipelineStopToken => _pipelineStopTokenSource.Token;
        internal void LogExecutionInfo(InvocationInfo invocationInfo, string text)
            string message = StringUtil.Format(PipelineStrings.PipelineExecutionInformation, GetCommand(invocationInfo), text);
            Log(message, invocationInfo, PipelineExecutionStatus.Started);
        internal void LogExecutionComplete(InvocationInfo invocationInfo, string text)
            Log(message, invocationInfo, PipelineExecutionStatus.Complete);
        internal void LogPipelineComplete()
            Log(null, null, PipelineExecutionStatus.PipelineComplete);
        internal void LogExecutionParameterBinding(InvocationInfo invocationInfo, string parameterName, string parameterValue)
            string message = StringUtil.Format(PipelineStrings.PipelineExecutionParameterBinding, GetCommand(invocationInfo), parameterName, parameterValue);
            Log(message, invocationInfo, PipelineExecutionStatus.ParameterBinding);
        internal void LogExecutionError(InvocationInfo invocationInfo, ErrorRecord errorRecord)
            string message = StringUtil.Format(PipelineStrings.PipelineExecutionNonTerminatingError, GetCommand(invocationInfo), errorRecord.ToString());
            Log(message, invocationInfo, PipelineExecutionStatus.Error);
        private bool _terminatingErrorLogged = false;
        internal void LogExecutionException(Exception exception)
            _executionFailed = true;
            // Only log one terminating error for pipeline execution.
            if (_terminatingErrorLogged)
            _terminatingErrorLogged = true;
            string message = StringUtil.Format(PipelineStrings.PipelineExecutionTerminatingError, GetCommand(exception), exception.Message);
            Log(message, null, PipelineExecutionStatus.Error);
        private static string GetCommand(InvocationInfo invocationInfo)
            if (invocationInfo.MyCommand != null)
                return invocationInfo.MyCommand.Name;
        private static string GetCommand(Exception exception)
            IContainsErrorRecord icer = exception as IContainsErrorRecord;
            if (icer != null && icer.ErrorRecord != null)
                return GetCommand(icer.ErrorRecord.InvocationInfo);
        private void Log(string logElement, InvocationInfo invocation, PipelineExecutionStatus pipelineExecutionStatus)
            System.Management.Automation.Host.PSHostUserInterface hostInterface = null;
            if (this.LocalPipeline != null)
                hostInterface = this.LocalPipeline.Runspace.GetExecutionContext.EngineHostInterface.UI;
            // Acknowledge command completion
            if (hostInterface != null)
                if (pipelineExecutionStatus == PipelineExecutionStatus.Complete)
                    hostInterface.TranscribeCommandComplete(invocation);
                else if (pipelineExecutionStatus == PipelineExecutionStatus.PipelineComplete)
                    hostInterface.TranscribePipelineComplete();
            // Log the cmdlet invocation execution details if we didn't have an associated script line with it.
            if ((invocation == null) || string.IsNullOrEmpty(invocation.Line))
                hostInterface?.TranscribeCommand(logElement, invocation);
            if (_needToLog && !string.IsNullOrEmpty(logElement))
                _eventLogBuffer ??= new List<string>();
                _eventLogBuffer.Add(logElement);
        private void LogToEventLog()
            // We check to see if there is anything in the buffer before we flush it.
            // Flushing the empty buffer causes a measurable performance degradation.
            if (_commands?.Count > 0 && _eventLogBuffer?.Count > 0)
                InternalCommand firstCmd = _commands[0].Command;
                MshLog.LogPipelineExecutionDetailEvent(
                    firstCmd.Context,
                    _eventLogBuffer,
                    firstCmd.MyInvocation);
            // Clear the log buffer after writing the event.
            _eventLogBuffer?.Clear();
        private bool _needToLog = false;
        private List<string> _eventLogBuffer;
        /// Add a single InternalCommand to the end of the pipeline.
        /// <returns>Results from last pipeline stage.</returns>
        /// see AddCommand
        /// <exception cref="ObjectDisposedException"></exception>
        internal int Add(CommandProcessorBase commandProcessor)
            if (commandProcessor is NativeCommandProcessor nativeCommand)
                if (_lastNativeCommand is not null)
                    // Only report experimental feature usage once per pipeline.
                    if (!_haveReportedNativePipeUsage)
                        ApplicationInsightsTelemetry.SendExperimentalUseData("PSNativeCommandPreserveBytePipe", "p");
                        _haveReportedNativePipeUsage = true;
                    _lastNativeCommand.DownStreamNativeCommand = nativeCommand;
                    nativeCommand.UpstreamIsNativeCommand = true;
                _lastNativeCommand = nativeCommand;
                _lastNativeCommand = null;
            commandProcessor.CommandRuntime.PipelineProcessor = this;
            return AddCommand(commandProcessor, _commands.Count, readErrorQueue: false);
        internal void AddRedirectionPipe(PipelineProcessor pipelineProcessor)
            if (pipelineProcessor is null)
                throw PSTraceSource.NewArgumentNullException(nameof(pipelineProcessor));
            _redirectionPipes ??= new List<PipelineProcessor>();
            _redirectionPipes.Add(pipelineProcessor);
        // 2004/02/28-JSnover (from spec review) ReadFromErrorQueue
        //   should be an int or enum to allow for more queues
        // 2005/03/08-JonN: This is an internal API
        /// Add a command to the pipeline.
        /// <param name="commandProcessor"></param>
        /// <param name="readFromCommand">Reference number of command from which to read, 0 for none.</param>
        /// <param name="readErrorQueue">Read from error queue of command readFromCommand.</param>
        /// <returns>Reference number of this command for use in readFromCommand.</returns>
        /// FirstCommandCannotHaveInput: <paramref name="readFromCommand"/> must be zero
        ///   for the first command in the pipe
        /// InvalidCommandNumber: there is no command numbered <paramref name="readFromCommand"/>
        ///   A command can only read from earlier commands; this prevents circular queues
        /// ExecutionAlreadyStarted: pipeline has already started or completed
        /// PipeAlreadyTaken: the downstream pipe of command <paramref name="readFromCommand"/>
        ///   is already taken
        private int AddCommand(CommandProcessorBase commandProcessor, int readFromCommand, bool readErrorQueue)
            if (commandProcessor == null)
                throw PSTraceSource.NewArgumentNullException(nameof(commandProcessor));
                // "_commands == null"
                throw PSTraceSource.NewObjectDisposedException("PipelineProcessor");
            if (_executionStarted)
                    PipelineStrings.ExecutionAlreadyStarted);
            if (commandProcessor.AddedToPipelineAlready)
                    PipelineStrings.CommandProcessorAlreadyUsed);
            if (_commands.Count == 0)
                if (readFromCommand != 0)
                    // "First command cannot have input"
                        nameof(readFromCommand),
                        PipelineStrings.FirstCommandCannotHaveInput);
                commandProcessor.AddedToPipelineAlready = true;
            // 2003/08/11-JonN Subsequent commands must have predecessor
            else if (readFromCommand > _commands.Count || readFromCommand <= 0)
                // "invalid command number"
                    PipelineStrings.InvalidCommandNumber);
                var prevcommandProcessor = _commands[readFromCommand - 1] as CommandProcessorBase;
                ValidateCommandProcessorNotNull(prevcommandProcessor, errorMessage: null);
                Pipe UpstreamPipe = (readErrorQueue)
                    ? prevcommandProcessor.CommandRuntime.ErrorOutputPipe
                    : prevcommandProcessor.CommandRuntime.OutputPipe;
                if (UpstreamPipe == null)
                if (UpstreamPipe.DownstreamCmdlet != null)
                        PipelineStrings.PipeAlreadyTaken);
                commandProcessor.CommandRuntime.InputPipe = UpstreamPipe;
                UpstreamPipe.DownstreamCmdlet = commandProcessor;
                // 2004/09/14-JonN This code could be moved to SynchronousExecute
                //  if this setting needed to bind at a later time
                //  than AddCommand.
                if (commandProcessor.CommandRuntime.MergeUnclaimedPreviousErrorResults)
                    for (int i = 0; i < _commands.Count; i++)
                        prevcommandProcessor = _commands[i];
                        // check whether the error output is already claimed
                        if (prevcommandProcessor.CommandRuntime.ErrorOutputPipe.DownstreamCmdlet != null)
                        if (prevcommandProcessor.CommandRuntime.ErrorOutputPipe.ExternalWriter != null)
                        // Set the upstream cmdlet's error output to go down
                        // the same pipe as the downstream cmdlet's input
                        prevcommandProcessor.CommandRuntime.ErrorOutputPipe = UpstreamPipe;
            _commands.Add(commandProcessor);
            // We will log event(s) about the pipeline execution details if any command in the pipeline requests that.
            _needToLog |= commandProcessor.CommandRuntime.LogPipelineExecutionDetail;
            // We give the Command a pointer back to the
            // PipelineProcessor so that it can check whether the
            // command has been stopped.
            return _commands.Count;
        /// Execute the accumulated commands and clear the pipeline.
        /// SynchronousExecute does not return until all commands have
        /// completed.  There is no asynchronous variant; instead, once the
        /// pipeline is set up, the caller can spawn a thread and call
        /// SynchronousExecute from that thread.  This does not mean that
        /// PipelineProcessor is thread-safe; once SynchronousExecute is
        /// running, PipelineProcessor should not be accessed through any
        /// other means. This variant of the routine looks at it's input
        /// object to see if it's enumerable or not.
        /// Input objects for first stage. If this is AutomationNull.Value, the
        /// first cmdlet is the beginning of the pipeline.
        /// Results from last pipeline stage.  This will be empty if
        /// ExternalSuccessOutput is set.
        /// PipelineExecuteRequiresAtLeastOneCommand
        /// A cmdlet encountered a terminating error
        /// The pipeline was stopped asynchronously
        /// <exception cref="ActionPreferenceStopException">
        /// The ActionPreference.Stop or ActionPreference.Inquire policy
        /// triggered a terminating error.
        /// An error occurred clearing the error variable.
        /// <exception cref="HaltCommandException">
        /// HaltCommandException will cause the command
        /// to stop, but should not be reported as an error.
        internal Array SynchronousExecuteEnumerate(object input)
            bool pipelineSucceeded = false;
            ExceptionDispatchInfo toRethrowInfo = null;
            CommandProcessorBase commandRequestingUpstreamCommandsToStop = null;
                        // If the caller specified an input object array, we run assuming there is an incoming "stream"
                        // of objects. This will prevent the one default call to ProcessRecord on the first command.
                        Start(incomingStream: input != AutomationNull.Value);
                        // Start has already validated firstcommandProcessor
                        CommandProcessorBase firstCommandProcessor = _commands[0];
                        // Add any input to the first command.
                        if (ExternalInput is not null)
                            firstCommandProcessor.CommandRuntime.InputPipe.ExternalReader = ExternalInput;
                        Inject(input, enumerate: true);
                        if (_firstTerminatingError?.SourceException is StopUpstreamCommandsException exception)
                            _firstTerminatingError = null;
                            commandRequestingUpstreamCommandsToStop = exception.RequestingCommandProcessor;
                    DoCompleteCore(commandRequestingUpstreamCommandsToStop);
                    pipelineSucceeded = true;
                    // Clean up resources for script commands, no matter the pipeline succeeded or not.
                    // This method catches and handles all exceptions inside, so it will never throw.
                    Clean();
                if (pipelineSucceeded)
                    // Now, we are sure all 'commandProcessors' hosted by the current 'pipelineProcessor' are done execution,
                    // so if there are any redirection 'pipelineProcessors' associated with any of those 'commandProcessors',
                    // they must have successfully executed 'StartStepping' and 'Step', and thus we should call 'DoComplete'
                    // on them for completeness.
                    if (_redirectionPipes is not null)
                        foreach (PipelineProcessor redirectPipelineProcessor in _redirectionPipes)
                            // The 'Clean' block for each 'commandProcessor' might still write to a pipe that is associated
                            // with the redirection 'pipelineProcessor' (e.g. a redirected error pipe), which would trigger
                            // the call to 'pipelineProcessor.Step'.
                            // It's possible (though very unlikely) that the call to 'pipelineProcessor.Step' failed with an
                            // exception, and in such case, the 'pipelineProcessor' would have been disposed, and therefore
                            // the call to 'DoComplete' will simply return, because '_commands' was already set to null.
                            redirectPipelineProcessor.DoCompleteCore(null);
                    // The 'Clean' blocks write nothing to the output pipe, so the results won't be affected by them.
                    return RetrieveResults();
                toRethrowInfo = GetFirstError(e);
            // By rethrowing the exception outside of the handler, we allow the CLR on X64/IA64 to free from
            // the stack the exception records related to this exception.
            // The only reason we should get here is if an exception should be rethrown.
            Diagnostics.Assert(toRethrowInfo != null, "Alternate protocol path failure");
            toRethrowInfo.Throw();
            // UNREACHABLE
        private ExceptionDispatchInfo GetFirstError(RuntimeException e)
            // The error we want to report is the first terminating error which occurred during pipeline execution,
            // regardless of whether other errors occurred afterward.
            var firstError = _firstTerminatingError ?? ExceptionDispatchInfo.Capture(e);
            LogExecutionException(firstError.SourceException);
            return firstError;
        private void ThrowFirstErrorIfExisting(bool logException)
            if (_firstTerminatingError != null)
                if (logException)
                    LogExecutionException(_firstTerminatingError.SourceException);
                _firstTerminatingError.Throw();
        private void DoCompleteCore(CommandProcessorBase commandRequestingUpstreamCommandsToStop)
            if (_commands is null)
                // This could happen to a redirection pipeline, either for an expression (e.g. 1 > a.txt)
                // or for a command (e.g. command > a.txt).
                // An exception may be thrown from the call to 'StartStepping' or 'Step' on the pipeline,
                // which causes the pipeline commands to be disposed.
            // Call DoComplete() for all the commands, which will internally call Complete()
            MshCommandRuntime lastCommandRuntime = null;
                CommandProcessorBase commandProcessor = _commands[i];
                if (commandProcessor is null)
                    // An internal error that should not happen.
                if (object.ReferenceEquals(commandRequestingUpstreamCommandsToStop, commandProcessor))
                    // Do not call DoComplete/EndProcessing on the command that initiated stopping.
                    commandRequestingUpstreamCommandsToStop = null;
                if (commandRequestingUpstreamCommandsToStop is not null)
                    // Do not call DoComplete/EndProcessing on commands that were stopped upstream.
                    commandProcessor.DoComplete();
                EtwActivity.SetActivityId(commandProcessor.PipelineActivityId);
                // Log a command stopped event
                MshLog.LogCommandLifecycleEvent(
                    commandProcessor.Command.Context,
                    CommandState.Stopped,
                    commandProcessor.Command.MyInvocation);
                // Log the execution of a command (not script chunks, as they are not commands in and of themselves).
                if (commandProcessor.CommandInfo.CommandType != CommandTypes.Script)
                    LogExecutionComplete(commandProcessor.Command.MyInvocation, commandProcessor.CommandInfo.Name);
                lastCommandRuntime = commandProcessor.CommandRuntime;
            // Log the pipeline completion.
            if (lastCommandRuntime is not null)
                // Only log the pipeline completion if this wasn't a nested pipeline, as
                // pipeline state in transcription is associated with the toplevel pipeline
                if (LocalPipeline is null || !LocalPipeline.IsNested)
                    lastCommandRuntime.PipelineProcessor.LogPipelineComplete();
            // If a terminating error occurred, report it now.
            // This pipeline could have been stopped asynchronously, by 'Ctrl+c' manually or
            // 'PowerShell.Stop' programatically. We need to check and see if that's the case.
            // An example:
            // - 'Start-Sleep' is running in this pipeline, and 'pipelineProcessor.Stop' gets
            //   called on a different thread, which sets a 'PipelineStoppedException' object
            //   to '_firstTerminatingError' and runs 'StopProcessing' on 'Start-Sleep'.
            // - The 'StopProcessing' will cause 'Start-Sleep' to return from 'ProcessRecord'
            //   call, and thus the pipeline execution will move forward to run 'DoComplete'
            //   for the 'Start-Sleep' command and thus the code flow will reach here.
            // For this given example, we need to check '_firstTerminatingError' and throw out
            // the 'PipelineStoppedException' if the pipeline was indeed being stopped.
            ThrowFirstErrorIfExisting(logException: true);
        /// Clean up resources for script commands in this pipeline processor.
        /// Exception from a 'Clean' block is not allowed to propagate up and terminate the pipeline
        /// so that other 'Clean' blocks can run without being affected. Therefore, this method will
        /// catch and handle all exceptions inside, and it will never throw.
        private void Clean()
            if (!_executionStarted || _commands is null)
                // Simply return if the pipeline execution wasn't even started, or the commands of
                // the pipeline have already been disposed.
            // So far, if '_firstTerminatingError' is not null, then it must be a terminating error
            // thrown from one of 'Begin/Process/End' blocks. There can be terminating error thrown
            // from 'Clean' block as well, which needs to be handled in this method.
            // In order to capture the subsequent first terminating error thrown from 'Clean', we
            // need to forget the previous '_firstTerminatingError' value before calling 'DoClean'
            // on each command processor, so we have to save the old value here and restore later.
            ExceptionDispatchInfo oldFirstTerminatingError = _firstTerminatingError;
            // Suspend a stopping pipeline by setting 'IsStopping' to false and restore it afterwards.
            bool oldIsStopping = ExceptionHandlingOps.SuspendStoppingPipelineImpl(LocalPipeline);
                foreach (CommandProcessorBase commandProcessor in _commands)
                    if (commandProcessor is null || !commandProcessor.HasCleanBlock)
                        // Forget the terminating error we saw before, so a terminating error thrown
                        // from the subsequent 'Clean' block can be recorded and handled properly.
                        commandProcessor.DoCleanup();
                        // Retrieve and report the terminating error that was thrown in the 'Clean' block.
                        ExceptionDispatchInfo firstError = GetFirstError(e);
                        commandProcessor.ReportCleanupError(firstError.SourceException);
                        // Theoretically, only 'RuntimeException' could be thrown out, but we catch
                        // all and log them here just to be safe.
                        // Skip special flow control exceptions and log others.
                        if (ex is not FlowControlException && ex is not HaltCommandException)
                            MshLog.LogCommandHealthEvent(commandProcessor.Context, ex, Severity.Warning);
                _firstTerminatingError = oldFirstTerminatingError;
                ExceptionHandlingOps.RestoreStoppingPipelineImpl(LocalPipeline, oldIsStopping);
        /// Clean up resources for the script commands of a steppable pipeline.
        /// The way we handle 'Clean' blocks in 'StartStepping', 'Step', and 'DoComplete' makes sure that:
        ///  1. The 'Clean' blocks get to run if any exception is thrown from the pipeline execution.
        ///  2. The 'Clean' blocks get to run if the pipeline runs to the end successfully.
        /// However, this is not enough for a steppable pipeline, because the function, where the steppable
        /// pipeline gets used, may fail (think about a proxy function). And that may lead to the situation
        /// where "no exception was thrown from the steppable pipeline" but "the steppable pipeline didn't
        /// run to the end". In that case, 'Clean' won't run unless it's triggered explicitly on the steppable
        /// pipeline. This method is how we will expose this functionality to 'SteppablePipeline'.
        /// Implements DoComplete as a stand-alone function for completing
        /// the execution of a steppable pipeline.
        /// <returns>The results of the execution.</returns>
        internal Array DoComplete()
            if (!_executionStarted)
                    PipelineStrings.PipelineNotStarted);
                ExceptionDispatchInfo toRethrowInfo;
                    DoCompleteCore(null);
                // By rethrowing the exception outside of the handler, we allow the CLR on X64/IA64 to free from the stack
                // the exception records related to this exception.
                // The only reason we should get here is an exception should be rethrown.
        /// This routine starts the stepping process. It is optional to call this but can be useful
        /// if you want the begin clauses of the pipeline to be run even when there may not be any
        /// input to process as is the case for I/O redirection into a file. We still want the file
        /// opened, even if there was nothing to write to it.
        /// <param name="expectInput">True if you want to write to this pipeline.</param>
        internal void StartStepping(bool expectInput)
            bool startSucceeded = false;
                Start(expectInput);
                startSucceeded = true;
                // Check if this pipeline is being stopped asynchronously.
                ThrowFirstErrorIfExisting(logException: false);
                if (!startSucceeded && e is PipelineStoppedException)
                    // When a terminating error happens during command execution, PowerShell will first save it
                    // to '_firstTerminatingError', and then throw a 'PipelineStoppedException' to tear down the
                    // pipeline. So when the caught exception here is 'PipelineStoppedException', it may not be
                    // the actual original terminating error.
                    // In this case, we want to report the first terminating error which occurred during pipeline
                    // execution, regardless of whether other errors occurred afterward.
        /// Request that the pipeline execution should stop.  Unlike other
        /// methods of PipelineProcessor, this method can be called
            // Only call StopProcessing if the pipeline is being stopped
            // for the first time
            if (!RecordFailure(new PipelineStoppedException(), command: null))
            // Retain copy of _commands in case Dispose() is called
            List<CommandProcessorBase> commands = _commands;
            if (commands is null)
            _pipelineStopTokenSource.Cancel();
            // Call StopProcessing() for all the commands.
            foreach (CommandProcessorBase commandProcessor in commands)
                    commandProcessor.Command.DoStopProcessing();
                    // We swallow exceptions which occur during StopProcessing.
        #region private_methods
        /// Partially execute the pipeline, and retrieve the output
        /// after the input objects have been entered into the pipe.
        /// Array of input objects for first stage
        /// The pipeline has already been stopped, or a cmdlet encountered
        /// a terminating error
        /// or a terminating error occurred.
        internal Array Step(object input)
            bool injectSucceeded = false;
                Start(true);
                Inject(input, enumerate: false);
                injectSucceeded = true;
                if (!injectSucceeded && e is PipelineStoppedException)
        /// Prepares the pipeline for execution.
        /// <param name="incomingStream">
        /// Input objects are expected, so do not close the first command.
        /// This will prevent the one default call to ProcessRecord
        /// on the first command.
        /// Start must always be called in a context where terminating errors will
        /// be caught and result in DisposeCommands.
        private void Start(bool incomingStream)
            // Every call to Step or SynchronousExecute will call Start.
            if (_commands == null || _commands.Count == 0)
                    PipelineStrings.PipelineExecuteRequiresAtLeastOneCommand);
            CommandProcessorBase firstcommandProcessor = _commands[0];
            ValidateCommandProcessorNotNull(firstcommandProcessor, PipelineStrings.PipelineExecuteRequiresAtLeastOneCommand);
            // Set the execution scope using the current scope
            _executionScope ??= firstcommandProcessor.Context.EngineSessionState.CurrentScope;
            // add ExternalSuccessOutput to the last command
            CommandProcessorBase LastCommandProcessor = _commands[_commands.Count - 1];
            ValidateCommandProcessorNotNull(LastCommandProcessor, errorMessage: null);
            if (ExternalSuccessOutput != null)
                LastCommandProcessor.CommandRuntime.OutputPipe.ExternalWriter = ExternalSuccessOutput;
            // add ExternalErrorOutput to all commands whose error
            // output is not yet claimed
            SetExternalErrorOutput();
            if (ExternalInput == null && !incomingStream)
                // no upstream cmdlet from the first command
                firstcommandProcessor.CommandRuntime.IsClosed = true;
            // We want the value of PSDefaultParameterValues before possibly changing to the commands scopes.
            // This ensures we use the value from the caller's scope, not the callee's scope.
            IDictionary psDefaultParameterValues =
                firstcommandProcessor.Context.GetVariableValue(SpecialVariables.PSDefaultParameterValuesVarPath, false) as IDictionary;
            _executionStarted = true;
            // Allocate the pipeline iteration array; note that the pipeline position for
            // each command starts at 1 so we need to allocate _commands.Count + 1 items.
            int[] pipelineIterationInfo = new int[_commands.Count + 1];
            // Prepare all commands from Engine's side, and make sure they are all valid
                    // "null command " + i
                // Generate new Activity Id for the thread
                Guid pipelineActivityId = EtwActivity.CreateActivityId();
                EtwActivity.SetActivityId(pipelineActivityId);
                commandProcessor.PipelineActivityId = pipelineActivityId;
                // Log a command started event
                    commandProcessor.Context,
                    CommandState.Started,
                Microsoft.PowerShell.Telemetry.Internal.TelemetryAPI.TraceExecutedCommand(commandProcessor.Command.CommandInfo, commandProcessor.Command.CommandOrigin);
                // Log the execution of a command (not script chunks, as they are not commands in and of themselves)
                    LogExecutionInfo(commandProcessor.Command.MyInvocation, commandProcessor.CommandInfo.Name);
                InvocationInfo myInfo = commandProcessor.Command.MyInvocation;
                myInfo.PipelinePosition = i + 1;
                myInfo.PipelineLength = _commands.Count;
                myInfo.PipelineIterationInfo = pipelineIterationInfo;
                myInfo.ExpectingInput = commandProcessor.IsPipelineInputExpected();
                commandProcessor.DoPrepare(psDefaultParameterValues);
            // Clear ErrorVariable as appropriate
            SetupParameterVariables();
            // Prepare all commands from Command's side.
            // Note that DoPrepare() and DoBegin() should NOT be combined
            // in a single for loop.
            // Reason: Encoding of commandline parameters happen
            // as part of DoPrepare(). If they are combined,
            // the first command's DoBegin() will be called before
            // the next command's DoPrepare(). Since BeginProcessing()
            // can write objects to the downstream commandlet,
            // it will end up calling DoExecute() (from Pipe.Add())
            // before DoPrepare.
                commandProcessor.DoBegin();
        /// Add ExternalErrorOutput to all commands whose error output is not yet claimed.
        private void SetExternalErrorOutput()
            if (ExternalErrorOutput != null)
                    Pipe errorPipe = commandProcessor.CommandRuntime.ErrorOutputPipe;
                    // check whether a cmdlet is consuming the error pipe
                    if (!errorPipe.IsRedirected)
                        errorPipe.ExternalWriter = ExternalErrorOutput;
        /// Clear ErrorVariable as appropriate.
        private void SetupParameterVariables()
                ValidateCommandProcessorNotNull(commandProcessor, errorMessage: null);
                commandProcessor.CommandRuntime.SetupOutVariable();
                commandProcessor.CommandRuntime.SetupErrorVariable();
                commandProcessor.CommandRuntime.SetupWarningVariable();
                commandProcessor.CommandRuntime.SetupPipelineVariable();
                commandProcessor.CommandRuntime.SetupInformationVariable();
        private static void ValidateCommandProcessorNotNull(CommandProcessorBase commandProcessor, string errorMessage)
            if (commandProcessor?.CommandRuntime is null)
                throw errorMessage is null
                    ? PSTraceSource.NewInvalidOperationException()
                    : PSTraceSource.NewInvalidOperationException(errorMessage, Array.Empty<object>());
        /// Partially execute the pipeline.  The output remains in
        /// the pipes.
        /// <param name="enumerate">If true, unravel the input otherwise pass as one object.</param>
        /// Exception if any cmdlet throws a [terminating] exception
        /// Inject must always be called in a context where terminating errors will
        private void Inject(object input, bool enumerate)
            if (input != AutomationNull.Value)
                if (enumerate)
                    IEnumerator enumerator = LanguagePrimitives.GetEnumerator(input);
                        firstcommandProcessor.CommandRuntime.InputPipe = new Pipe(enumerator);
                        firstcommandProcessor.CommandRuntime.InputPipe.Add(input);
            // Do not set ExternalInput until SynchronousExecute is called
            // Execute the first command - In the streamlet model, Execute of the first command will
            // automatically call the downstream command incase if there are any objects in the pipe.
            firstcommandProcessor.DoExecute();
        /// Retrieve results from the pipeline.
        /// ExternalSuccessOutput is set or if this pipeline has been linked.
        private Array RetrieveResults()
                // This could happen to an expression redirection pipeline (e.g. 1 > a.txt).
            // If the error queue has been linked, it's up to the link to
            // deal with the output. Don't do anything here...
            if (!_linkedErrorOutput)
                    Pipe ErrorPipe = commandProcessor.CommandRuntime.ErrorOutputPipe;
                    if (ErrorPipe.DownstreamCmdlet == null && !ErrorPipe.Empty)
                        // Clear the error pipe if it's not empty and will not be consumed.
                        ErrorPipe.Clear();
            // If the success queue has been linked, it's up to the link to
            if (_linkedSuccessOutput)
            Array results = LastCommandProcessor.CommandRuntime.GetResultsAsArray();
            // Do not return the same results more than once
            LastCommandProcessor.CommandRuntime.OutputPipe.Clear();
            return results is null ? MshCommandRuntime.StaticEmptyArray : results;
        /// Links this pipeline to a pre-existing Pipe object. This allows nested pipes
        /// to write into the parent pipeline. It does this by resetting the terminal
        /// pipeline object.
        /// <param name="pipeToUse">The pipeline to write success objects to.</param>
        internal void LinkPipelineSuccessOutput(Pipe pipeToUse)
            Dbg.Assert(pipeToUse != null, "Caller should verify pipeToUse != null");
            LastCommandProcessor.CommandRuntime.OutputPipe = pipeToUse;
            _linkedSuccessOutput = true;
        internal void LinkPipelineErrorOutput(Pipe pipeToUse)
                if (commandProcessor.CommandRuntime.ErrorOutputPipe.DownstreamCmdlet == null)
                    commandProcessor.CommandRuntime.ErrorOutputPipe = pipeToUse;
            _linkedErrorOutput = true;
        /// When the command is complete, Command should be disposed.
        /// Exceptions occurring while disposing commands are recorded
        /// but not passed through.
        private void DisposeCommands()
            // Note that this is not in a lock.
            // We do not make Dispose() wait until StopProcessing() has completed.
            if (_commands is null && _redirectionPipes is null)
                // Commands were already disposed.
            LogToEventLog();
            if (_commands is not null)
                    // If Dispose throws an exception, record it as a pipeline failure and continue disposing cmdlets.
                        // Only cmdlets can have variables defined via the common parameters.
                        // We handle the cleanup of those variables only if we need to.
                        if (commandProcessor is CommandProcessor)
                            if (commandProcessor.Command is not PSScriptCmdlet)
                                // For script cmdlets, the variable lists were already removed when exiting a scope.
                                // So we only need to take care of binary cmdlets here.
                                commandProcessor.CommandRuntime.RemoveVariableListsInPipe();
                            // Remove the pipeline variable if we need to.
                            commandProcessor.CommandRuntime.RemovePipelineVariable();
                        commandProcessor.Dispose();
                        // The only vaguely plausible reason for a failure here is an exception in 'Command.Dispose'.
                        // As such, this should be covered by the overall exemption.
                        InvocationInfo myInvocation = commandProcessor.Command?.MyInvocation;
                            e = new CmdletProviderInvocationException(pie, myInvocation);
                            e = new CmdletInvocationException(e, myInvocation);
                            MshLog.LogCommandHealthEvent(commandProcessor.Command.Context, e, Severity.Warning);
                        RecordFailure(e, commandProcessor.Command);
            _commands = null;
            // Now dispose any pipes that were used for redirection...
                foreach (PipelineProcessor redirPipe in _redirectionPipes)
                    if (redirPipe is null)
                    // Clean resources for script commands.
                    // It is possible (though very unlikely) that the call to 'Step' on the redirection pipeline failed.
                    // In such a case, 'Clean' would have run and the 'pipelineProcessor' would have been disposed.
                    // Therefore, calling 'Clean' again will simply return, because '_commands' was already set to null.
                    redirPipe.Clean();
                    // The complicated logic of disposing the commands is taken care
                    // of through recursion, this routine should not be getting any
                    // exceptions...
                        redirPipe.Dispose();
            _redirectionPipes = null;
        private readonly object _stopReasonLock = new object();
        /// Makes an internal note of the exception, but only if this is
        /// the first error.
        /// <param name="e">Error which terminated the pipeline.</param>
        /// <param name="command">Command against which to log SecondFailure.</param>
        /// <returns>True if-and-only-if the pipeline was not already stopped.</returns>
        internal bool RecordFailure(Exception e, InternalCommand command)
            bool wasStopping = false;
            lock (_stopReasonLock)
                if (_firstTerminatingError == null)
                    _firstTerminatingError = ExceptionDispatchInfo.Capture(e);
                // Error Architecture: Log/trace second and subsequent RecordFailure.
                // Note that the pipeline could have been stopped asynchronously before hitting the error,
                // therefore we check whether '_firstTerminatingError' is 'PipelineStoppedException'.
                else if (_firstTerminatingError.SourceException is not PipelineStoppedException
                    && command?.Context != null)
                    Exception ex = e;
                    while ((ex is TargetInvocationException || ex is CmdletInvocationException)
                            && (ex.InnerException != null))
                        ex = ex.InnerException;
                    if (ex is not PipelineStoppedException)
                        string message = StringUtil.Format(PipelineStrings.SecondFailure,
                            _firstTerminatingError.GetType().Name,
                            _firstTerminatingError.SourceException.StackTrace,
                            ex.GetType().Name,
                            ex.StackTrace
                            command.Context,
                            new InvalidOperationException(message, ex),
                wasStopping = _stopping;
            return !wasStopping;
        internal void ForgetFailure()
        // Only this InternalCommand from this Thread is allowed to call
        // WriteObject/WriteError
        internal InternalCommand _permittedToWrite = null;
        internal bool _permittedToWriteToPipeline = false;
        internal System.Threading.Thread _permittedToWriteThread = null;
        #endregion private_methods
        /// ExternalInput allows the caller to specify an asynchronous source for
        /// the input to the first command in the pipeline.  Note that if
        /// ExternalInput is specified, SynchronousExecute will not return
        /// until the ExternalInput is closed.
        /// It is the responsibility of the caller to ensure that the object
        /// reader is closed, usually by another thread.
        internal PipelineReader<object> ExternalInput
                return _externalInputPipe;
                _externalInputPipe = value;
        /// ExternalSuccessOutput provides asynchronous access to the
        /// success output of the last command in the pipeline.  Note that
        /// if ExternalSuccessOutput is specified, the result array return value
        /// to SynchronousExecute will always be empty.  PipelineProcessor will
        /// close ExternalSuccessOutput when the pipeline is finished.
        internal PipelineWriter ExternalSuccessOutput
                return _externalSuccessOutput;
                _externalSuccessOutput = value;
        /// ExternalErrorOutput provides asynchronous access to the combined
        /// error output of all commands in the pipeline except what is routed
        /// to other commands in the pipeline.  Note that if
        /// ExternalErrorOutput is specified, the errorResults return parameter to
        /// SynchronousExecute will always be empty.  PipelineProcessor will
        /// close ExternalErrorOutput when the pipeline is finished.
        internal PipelineWriter ExternalErrorOutput
                return _externalErrorOutput;
                _externalErrorOutput = value;
        /// Indicates whether this PipelineProcessor has already started.
        /// If so, some properties can no longer be changed.
        internal bool ExecutionStarted
            get { return _executionStarted; }
        /// Indicates whether stop has been requested on this PipelineProcessor.
        internal bool Stopping
            get { return _localPipeline != null && _localPipeline.IsStopping; }
        private LocalPipeline _localPipeline;
        internal LocalPipeline LocalPipeline
            get { return _localPipeline; }
            set { _localPipeline = value; }
        internal bool TopLevel { get; set; } = false;
        /// The scope the pipeline should execute in.
        internal SessionStateScope ExecutionScope
                return _executionScope;
                // This needs to be settable so that a steppable pipeline
                // can be stepped in the context of the caller, not where
                // it was created...
                _executionScope = value;
        internal enum PipelineExecutionStatus
            ParameterBinding,
            Complete,
            PipelineComplete
    /// Defines exception which is thrown when state of the pipeline is different
    /// from expected state.
    public class InvalidPipelineStateException : SystemException
        /// Initializes a new instance of the InvalidPipelineStateException class.
        public InvalidPipelineStateException()
            : base(StringUtil.Format(RunspaceStrings.InvalidPipelineStateStateGeneral))
        /// Initializes a new instance of the InvalidPipelineStateException class
        /// with a specified error message.
        /// The error message that explains the reason for the exception.
        public InvalidPipelineStateException(string message)
        /// with a specified error message and a reference to the inner exception that is the cause of this exception.
        public InvalidPipelineStateException(string message, Exception innerException)
        /// Initializes a new instance of the InvalidPipelineStateException and defines value of
        /// CurrentState and ExpectedState.
        /// <param name="message">The error message that explains the reason for the exception.
        /// <param name="currentState">Current state of pipeline.</param>
        /// <param name="expectedState">Expected state of pipeline.</param>
        internal InvalidPipelineStateException(string message, PipelineState currentState, PipelineState expectedState)
        /// Initializes a new instance of the <see cref="InvalidPipelineStateException"/>
        private InvalidPipelineStateException(SerializationInfo info, StreamingContext context)
        /// Gets CurrentState of the pipeline.
        public PipelineState CurrentState
            get { return _currentState; }
        /// Gets ExpectedState of the pipeline.
        public PipelineState ExpectedState
            get { return _expectedState; }
        /// State of pipeline when exception was thrown.
        private readonly PipelineState _currentState = 0;
        /// States of the pipeline expected in method which throws this exception.
        private readonly PipelineState _expectedState = 0;
    #region PipelineState
    /// Enumerated type defining the state of the Pipeline.
    public enum PipelineState
        /// The pipeline has not been started.
        /// The pipeline is executing.
        /// The pipeline is stoping execution.
        /// The pipeline is completed due to a stop request.
        /// The pipeline has completed.
        /// The pipeline completed abnormally due to an error.
        /// The pipeline is disconnected from remote running command.
        Disconnected = 6
    /// Type which has information about PipelineState and Exception
    /// associated with PipelineState.
    public sealed class PipelineStateInfo
        /// <param name="state">Execution state.</param>
        internal PipelineStateInfo(PipelineState state)
        /// caused by an error,otherwise; null.
        internal PipelineStateInfo(PipelineState state, Exception reason)
        /// <param name="pipelineStateInfo">Source information.</param>
        /// ArgumentNullException when <paramref name="pipelineStateInfo"/> is null.
        internal PipelineStateInfo(PipelineStateInfo pipelineStateInfo)
            Dbg.Assert(pipelineStateInfo != null, "caller should validate the parameter");
            State = pipelineStateInfo.State;
            Reason = pipelineStateInfo.Reason;
        /// This value indicates the state of the pipeline after the change.
        public PipelineState State { get; }
        /// Clones this object.
        internal PipelineStateInfo Clone()
            return new PipelineStateInfo(this);
    /// Event arguments passed to PipelineStateEvent handlers
    /// <see cref="Pipeline.StateChanged"/> event.
    public sealed class PipelineStateEventArgs : EventArgs
        /// Constructor PipelineStateEventArgs from PipelineStateInfo.
        /// <param name="pipelineStateInfo">The current state of the
        /// pipeline.</param>
        internal PipelineStateEventArgs(PipelineStateInfo pipelineStateInfo)
            PipelineStateInfo = pipelineStateInfo;
        /// Info about current state of pipeline.
        public PipelineStateInfo PipelineStateInfo { get; }
    #endregion ExecutionState
    /// Defines a class which can be used to invoke a pipeline of commands.
    public abstract class Pipeline : IDisposable
        internal Pipeline(Runspace runspace)
            : this(runspace, new CommandCollection())
        /// Constructor to initialize both Runspace and Command to invoke.
        /// Caller should make sure that "command" is not null.
        /// Runspace to use for the command invocation.
        /// command to Invoke.
        internal Pipeline(Runspace runspace, CommandCollection command)
                PSTraceSource.NewArgumentNullException(nameof(runspace));
            // This constructor is used only internally.
            // Caller should make sure the input is valid
            Dbg.Assert(command != null, "Command cannot be null");
            InstanceId = runspace.GeneratePipelineId();
            Commands = command;
            // Reset the AMSI session so that it is re-initialized
            // when the next script block is parsed.
            AmsiUtils.CloseSession();
        /// Gets the runspace this pipeline is created on.
        public abstract Runspace Runspace { get; }
        /// Gets the property which indicates if this pipeline is nested.
        public abstract bool IsNested { get; }
        /// Gets the property which indicates if this pipeline is a child pipeline.
        /// IsChild flag makes it possible for the pipeline to differentiate between
        /// a true v1 nested pipeline and the cmdlets calling cmdlets case. See bug
        /// 211462.
        internal virtual bool IsChild
            set { }
        /// Gets input writer for this pipeline.
        /// When the caller calls Input.Write(), the caller writes to the
        /// input of the pipeline.  Thus, <paramref name="Input"/>
        /// is a PipelineWriter or "thing which can be written to".
        /// Note:Input must be closed after Pipeline.InvokeAsync for InvokeAsync to
        /// finish.
        public abstract PipelineWriter Input { get; }
        /// Gets the output reader for this pipeline.
        /// When the caller calls Output.Read(), the caller reads from the
        /// output of the pipeline.  Thus, <paramref name="Output"/>
        /// is a PipelineReader or "thing which can be read from".
        public abstract PipelineReader<PSObject> Output { get; }
        /// Gets the error output reader for this pipeline.
        /// When the caller calls Error.Read(), the caller reads from the
        /// output of the pipeline.  Thus, <paramref name="Error"/>
        /// This is the non-terminating error stream from the command.
        /// In this release, the objects read from this PipelineReader
        /// are PSObjects wrapping ErrorRecords.
        public abstract PipelineReader<object> Error { get; }
        /// Gets Info about current state of the pipeline.
        public abstract PipelineStateInfo PipelineStateInfo { get; }
        /// True if pipeline execution encountered and error.
        /// It will always be true if _reason is non-null
        /// since an exception occurred. For other error types,
        /// It has to be set manually.
        public virtual bool HadErrors
            get { return _hadErrors; }
        private bool _hadErrors;
        internal void SetHadErrors(bool status)
            _hadErrors = _hadErrors || status;
        /// Gets the unique identifier for this pipeline. This identifier is unique with in
        /// the scope of Runspace.
        public long InstanceId { get; }
        /// Gets the collection of commands for this pipeline.
        public CommandCollection Commands { get; private set; }
        /// If this property is true, SessionState is updated for this
        /// pipeline state.
        public bool SetPipelineSessionState { get; set; } = true;
        /// Settings for the pipeline invocation thread.
        internal PSInvocationSettings InvocationSettings { get; set; }
        /// If this flag is true, the commands in this Pipeline will redirect the global error output pipe
        /// (ExecutionContext.ShellFunctionErrorOutputPipe) to the command's error output pipe.
        /// When the global error output pipe is not set, $ErrorActionPreference is not checked and all
        /// errors are treated as terminating errors.
        /// On V1, the global error output pipe is redirected to the command's error output pipe only when
        /// it has already been redirected. The command-line host achieves this redirection by merging the
        /// error output into the output pipe so it checks $ErrorActionPreference all right. However, when
        /// the Pipeline class is used programmatically the global error output pipe is not set and the first
        /// error terminates the pipeline.
        /// This flag is used to force the redirection. By default it is false to maintain compatibility with
        /// V1, but the V2 hosting interface (PowerShell class) sets this flag to true to ensure the global
        /// error output pipe is always set and $ErrorActionPreference is checked when invoking the Pipeline.
        /// Event raised when Pipeline's state changes.
        public abstract event EventHandler<PipelineStateEventArgs> StateChanged;
        /// Invoke the pipeline, synchronously, returning the results as an array of
        /// <remarks>If using synchronous invoke, do not close
        /// input objectWriter. Synchronous invoke will always close the input
        /// objectWriter.
        /// No command is added to pipeline
        /// <exception cref="InvalidPipelineStateException">
        /// PipelineState is not NotStarted.
        /// 1) A pipeline is already executing. Pipeline cannot execute
        /// concurrently.
        /// 2) Attempt is made to invoke a nested pipeline directly. Nested
        /// pipeline must be invoked from a running pipeline.
        /// RunspaceState is not Open
        /// Pipeline already disposed
        /// The script recursed too deeply into script functions.
        /// There is a fixed limit on the depth of recursion.
        /// A CLR security violation occurred.  Typically, this happens
        /// because the current CLR permissions do not allow adequate
        /// reflection access to a cmdlet assembly.
        /// Pipeline.Invoke can throw a variety of exceptions derived
        /// from RuntimeException. The most likely of these exceptions
        /// are listed below.
        /// One of more parameters or parameter values specified for
        /// a cmdlet are not valid, or mandatory parameters for a cmdlet
        /// were not specified.
        /// A cmdlet generated a terminating error.
        /// <exception cref="CmdletProviderInvocationException">
        /// A provider generated a terminating error.
        /// The pipeline was terminated asynchronously.
        public Collection<PSObject> Invoke()
            return Invoke(null);
        /// Invoke the pipeline, synchronously, returning the results as an array of objects.
        /// <param name="input">an array of input objects to pass to the pipeline.
        /// Array may be empty but may not be null</param>
        /// <returns>An array of zero or more result objects.</returns>
        /// <remarks>If using synchronous exectute, do not close
        public abstract Collection<PSObject> Invoke(IEnumerable input);
        /// Invoke the pipeline asynchronously.
        /// 1) Results are returned through the <see cref="Pipeline.Output"/> reader.
        /// 2) When pipeline is invoked using InvokeAsync, invocation doesn't
        /// finish until Input to pipeline is closed. Caller of InvokeAsync must close
        /// the input pipe after all input has been written to input pipe. Input pipe
        /// is closed by calling Pipeline.Input.Close();
        /// If you want this pipeline to execute as a standalone command
        /// (that is, using command-line parameters only),
        /// be sure to call Pipeline.Input.Close() before calling
        /// InvokeAsync().  Otherwise, the command will be executed
        /// as though it had external input.  If you observe that the
        /// command isn't doing anything, this may be the reason.
        /// 2) InvokeAsync is called on nested pipeline. Nested pipeline
        /// cannot be executed Asynchronously.
        public abstract void InvokeAsync();
        /// Synchronous call to stop the running pipeline.
        public abstract void Stop();
        /// Asynchronous call to stop the running pipeline.
        public abstract void StopAsync();
        public abstract Pipeline Copy();
        /// Connects synchronously to a running command on a remote server.
        /// The pipeline object must be in the disconnected state.
        /// <returns>A collection of result objects.</returns>
        public abstract Collection<PSObject> Connect();
        /// Connects asynchronously to a running command on a remote server.
        /// Sets the command collection.
        /// <param name="commands">Command collection to set.</param>
        /// <remarks>called by ClientRemotePipeline</remarks>
        internal void SetCommandCollection(CommandCollection commands)
            Commands = commands;
        /// Sets the history string to the one that is specified.
        /// <param name="historyString">History string to set.</param>
        internal abstract void SetHistoryString(string historyString);
        /// Invokes a remote command and immediately disconnects if
        /// transport layer supports it.
        internal abstract void InvokeAsyncAndDisconnect();
        #region Remote data drain/block methods
        /// Blocks data arriving from remote session.
        internal virtual void SuspendIncomingData()
        /// Resumes data arrive from remote session.
        internal virtual void ResumeIncomingData()
        /// Blocking call that waits until the current remote data
        /// queue is empty.
        internal virtual void DrainIncomingData()
        /// Disposes the pipeline. If pipeline is running, dispose first
        /// stops the pipeline.
            Dispose(!IsChild);
        protected virtual

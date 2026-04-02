    /// The base class for all command processor classes. It provides
    /// abstract methods to execute a command.
    internal abstract class CommandProcessorBase : IDisposable
        internal CommandProcessorBase()
        /// Initializes the base command processor class with the command metadata.
        /// The metadata about the command to run.
        internal CommandProcessorBase(CommandInfo commandInfo)
            if (commandInfo is IScriptCommandInfo scriptCommand)
                ExperimentalAttribute expAttribute = scriptCommand.ScriptBlock.ExperimentalAttribute;
                if (expAttribute != null && expAttribute.ToHide)
                    string errorTemplate = expAttribute.ExperimentAction == ExperimentAction.Hide
                        ? DiscoveryExceptions.ScriptDisabledWhenFeatureOn
                        : DiscoveryExceptions.ScriptDisabledWhenFeatureOff;
                    string errorMsg = StringUtil.Format(errorTemplate, expAttribute.ExperimentName);
                        "ScriptCommandDisabled",
                        commandInfo);
                    throw new CmdletInvocationException(errorRecord);
                HasCleanBlock = scriptCommand.ScriptBlock.HasCleanBlock;
            CommandInfo = commandInfo;
        private InternalCommand _command;
        // Marker of whether BeginProcessing() has already run,
        // also used by CommandProcessor.
        internal bool RanBeginAlready;
        // Marker of whether this command has already been added to
        // a PipelineProcessor. It is an error to add the same command
        // more than once.
        internal bool AddedToPipelineAlready
            get { return _addedToPipelineAlready; }
            set { _addedToPipelineAlready = value; }
        internal bool _addedToPipelineAlready;
        /// Gets the CommandInfo for the command this command processor represents.
        internal CommandInfo CommandInfo { get; set; }
        /// Gets whether the command has a 'Clean' block defined.
        internal bool HasCleanBlock { get; }
        /// This indicates whether this command processor is created from
        /// a script file.
        /// Script command processor created from a script file is special
        /// in following two perspectives,
        ///     1. New scope created needs to be a 'script' scope in the
        ///        sense that it needs to handle $script: variables.
        ///        For normal functions or scriptblocks, script scope
        ///        variables are not supported.
        ///     2. ExitException will be handled by setting lastExitCode.
        ///        For normal functions or scriptblocks, exit command will
        ///        kill current powershell session.
        public bool FromScriptFile { get { return _fromScriptFile; } }
        protected bool _fromScriptFile = false;
        /// If this flag is true, the commands in this Pipeline will redirect
        /// the global error output pipe to the command's error output pipe.
        /// (See the comment in Pipeline.RedirectShellErrorOutputPipe for an
        /// explanation of why this flag is needed).
        internal bool RedirectShellErrorOutputPipe { get; set; } = false;
        /// Gets or sets the command object.
        internal InternalCommand Command
                return _command;
                // The command runtime needs to be set up...
                    value.commandRuntime = this.commandRuntime;
                        value.CommandInfo = _command.CommandInfo;
                    // Set the execution context for the command it's currently
                    // null and our context has already been set up.
                    if (value.Context == null && _context != null)
                        value.Context = _context;
                _command = value;
        internal virtual ObsoleteAttribute ObsoleteAttribute
        // Full Qualified ID for the obsolete command warning
        private const string FQIDCommandObsolete = "CommandObsolete";
        /// The command runtime used for this instance of a command processor.
        protected MshCommandRuntime commandRuntime;
        internal MshCommandRuntime CommandRuntime
            get { return commandRuntime; }
            set { commandRuntime = value; }
        /// For commands that use the scope stack, if this flag is
        /// true, don't create a new scope when running this command.
        internal bool UseLocalScope
            get { return _useLocalScope; }
            set { _useLocalScope = value; }
        protected bool _useLocalScope;
        /// Ensures that the provided script block is compatible with the current language mode - to
        /// be used when a script block is being dotted.
        /// <param name="scriptBlock">The script block being dotted.</param>
        /// <param name="context">The current execution context.</param>
        /// <param name="invocationInfo">The invocation info about the command.</param>
        protected static void ValidateCompatibleLanguageMode(
            ScriptBlock scriptBlock,
            // If we are in a constrained language mode (Core or Restricted), block it.
            // We are currently restricting in one direction:
            //    - Can't dot something from a more permissive mode, since that would probably expose
            //      functions that were never designed to handle untrusted data.
            // This function won't be called for NoLanguage mode so the only direction checked is trusted
            // (FullLanguage mode) script running in a constrained/restricted session.
            var languageMode = context.LanguageMode;
            if (scriptBlock.LanguageMode.HasValue &&
                scriptBlock.LanguageMode != languageMode &&
                (languageMode == PSLanguageMode.RestrictedLanguage ||
                 languageMode == PSLanguageMode.ConstrainedLanguage))
                // Finally check if script block is really just PowerShell commands plus parameters.
                // If so then it is safe to dot source across language mode boundaries.
                bool isSafeToDotSource = false;
                    scriptBlock.GetPowerShell();
                    isSafeToDotSource = true;
                if (!isSafeToDotSource)
                            new NotSupportedException(DiscoveryExceptions.DotSourceNotSupported),
                            "DotSourceNotSupported",
                        errorRecord.SetInvocationInfo(invocationInfo);
                    string scriptBlockId = scriptBlock.GetFileName() ?? string.Empty;
                        title: CommandBaseStrings.WDACLogTitle,
                        message: StringUtil.Format(CommandBaseStrings.WDACLogMessage, scriptBlockId, scriptBlock.LanguageMode, languageMode),
                        fqid: "ScriptBlockDotSourceNotAllowed",
        /// The execution context used by the system.
        protected ExecutionContext _context;
            get { return _context; }
            set { _context = value; }
        /// Etw activity for this pipeline.
        internal Guid PipelineActivityId { get; set; } = Guid.Empty;
        #region handling of -? parameter
        internal virtual bool IsHelpRequested(out string helpTarget, out HelpCategory helpCategory)
            // by default we don't handle "-?" parameter at all
            // (we want to do the checks only for cmdlets - this method is overridden in CommandProcessor)
            helpTarget = null;
            helpCategory = HelpCategory.None;
        /// Creates a command processor for "get-help [helpTarget]".
        /// <param name="context">Context for the command processor.</param>
        /// <param name="helpTarget">Help target.</param>
        /// <param name="helpCategory">Help category.</param>
        /// <returns>Command processor for "get-help [helpTarget]".</returns>
        internal static CommandProcessorBase CreateGetHelpCommandProcessor(
            string helpTarget,
            HelpCategory helpCategory)
            if (string.IsNullOrEmpty(helpTarget))
                throw PSTraceSource.NewArgumentNullException(nameof(helpTarget));
            CommandProcessorBase helpCommandProcessor = context.CreateCommand("get-help", false);
                /*parameterAst*/null, "Name", "-Name:",
                /*argumentAst*/null, helpTarget,
            helpCommandProcessor.AddParameter(cpi);
            cpi = CommandParameterInternal.CreateParameterWithArgument(
                /*parameterAst*/null, "Category", "-Category:",
                /*argumentAst*/null, helpCategory.ToString(),
            return helpCommandProcessor;
        /// Tells whether pipeline input is expected or not.
        /// <returns>A bool indicating whether pipeline input is expected.</returns>
        internal bool IsPipelineInputExpected()
            return commandRuntime.IsPipelineInputExpected;
        /// If you want this command to execute in other than the default session
        /// state, use this API to get and set that session state instance...
        internal SessionStateInternal CommandSessionState { get; set; }
        /// Gets or sets the session state scope for this command processor object.
        protected internal SessionStateScope CommandScope { get; protected set; }
        protected virtual void OnSetCurrentScope()
        protected virtual void OnRestorePreviousScope()
        /// This method sets the current session state scope to the execution scope for the pipeline
        /// that was stored in the pipeline manager when it was first invoked.
        internal void SetCurrentScopeToExecutionScope()
            // Make sure we have a session state instance for this command.
            // If one hasn't been explicitly set, then use the session state
            // available on the engine execution context...
            CommandSessionState ??= Context.EngineSessionState;
            // Store off the current scope
            _previousScope = CommandSessionState.CurrentScope;
            _previousCommandSessionState = Context.EngineSessionState;
            Context.EngineSessionState = CommandSessionState;
            // Set the current scope to the pipeline execution scope
            CommandSessionState.CurrentScope = CommandScope;
            OnSetCurrentScope();
        /// Restores the current session state scope to the scope which was active when SetCurrentScopeToExecutionScope
        /// was called.
        internal void RestorePreviousScope()
            OnRestorePreviousScope();
            Context.EngineSessionState = _previousCommandSessionState;
            // Restore the scope but use the same session state instance we
            // got it from because the command may have changed the execution context
            // session state...
            CommandSessionState.CurrentScope = _previousScope;
        private SessionStateScope _previousScope;
        private SessionStateInternal _previousCommandSessionState;
        /// A collection of arguments that have been added by the parser or
        /// host interfaces. These will be sent to the parameter binder controller
        /// for processing.
        internal Collection<CommandParameterInternal> arguments = new Collection<CommandParameterInternal>();
        /// Adds an unbound parameter.
        /// The parameter to add to the unbound arguments list
        internal void AddParameter(CommandParameterInternal parameter)
            Diagnostics.Assert(parameter != null, "Caller to verify parameter argument");
            arguments.Add(parameter);
        /// Prepares the command for execution.
        /// This should be called once before ProcessRecord().
        internal abstract void Prepare(IDictionary psDefaultParameterValues);
        /// Write warning message for an obsolete command.
        /// <param name="obsoleteAttr"></param>
        private void HandleObsoleteCommand(ObsoleteAttribute obsoleteAttr)
            string commandName =
                string.IsNullOrEmpty(CommandInfo.Name)
                    ? "script block"
                    : string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                    CommandBaseStrings.ObsoleteCommand, CommandInfo.Name);
            string warningMsg = string.Format(
                CommandBaseStrings.UseOfDeprecatedCommandWarning,
                commandName, obsoleteAttr.Message);
            // We ignore the IsError setting because we don't want to break people when obsoleting a command
            using (this.CommandRuntime.AllowThisCommandToWrite(false))
                this.CommandRuntime.WriteWarning(new WarningRecord(FQIDCommandObsolete, warningMsg));
        /// Sets the execution scope for the pipeline and then calls the Prepare
        /// abstract method which gets overridden by derived classes.
        internal void DoPrepare(IDictionary psDefaultParameterValues)
            CommandProcessorBase oldCurrentCommandProcessor = _context.CurrentCommandProcessor;
                Context.CurrentCommandProcessor = this;
                SetCurrentScopeToExecutionScope();
                Prepare(psDefaultParameterValues);
                // Check obsolete attribute after Prepare so that -WarningAction will be respected for cmdlets
                if (ObsoleteAttribute != null)
                    // Obsolete command is rare. Put the IF here to avoid method call overhead
                    HandleObsoleteCommand(ObsoleteAttribute);
            catch (InvalidComObjectException e)
                // This type of exception could be thrown from parameter binding.
                string msg = StringUtil.Format(ParserStrings.InvalidComObjectException, e.Message);
                var newEx = new RuntimeException(msg, e);
                newEx.SetErrorId("InvalidComObjectException");
                throw newEx;
                RestorePreviousScope();
        /// Called once before ProcessRecord(). Internally it calls
        /// BeginProcessing() of the InternalCommand.
        internal virtual void DoBegin()
            // Note that DoPrepare() and DoBegin() should NOT be combined.
            // Reason: Encoding of commandline parameters happen as part
            // of DoPrepare(). If they are combined, the first command's
            // DoBegin() will be called before the next command's
            // DoPrepare(). Since BeginProcessing() can write objects
            // to the downstream commandlet, it will end up calling
            // DoExecute() (from Pipe.Add()) before DoPrepare.
            if (!RanBeginAlready)
                    if (RedirectShellErrorOutputPipe || _context.ShellFunctionErrorOutputPipe is not null)
                        _context.ShellFunctionErrorOutputPipe = commandRuntime.ErrorOutputPipe;
                    _context.CurrentCommandProcessor = this;
                    using (ParameterBinderBase.bindingTracer.TraceScope("CALLING BeginProcessing"))
                            Context.Debugger.CheckCommand(Command.MyInvocation);
                    _context.CurrentCommandProcessor = oldCurrentCommandProcessor;
        /// This calls the command.  It assumes that DoPrepare() has already been called.
        internal abstract void ProcessRecord();
        /// This method sets the execution scope to the
        /// appropriate scope for the pipeline and then calls
        /// the ProcessRecord abstract method that derived command processors
        /// override.
        internal void DoExecute()
            ExecutionContext.CheckStackDepth();
                ProcessRecord();
        /// Called once after ProcessRecord().
        /// Internally it calls EndProcessing() of the InternalCommand.
        /// A terminating error occurred, or the pipeline was otherwise stopped.
        internal virtual void Complete()
            // Call ProcessRecord once from complete. Don't call DoExecute...
                using (ParameterBinderBase.bindingTracer.TraceScope("CALLING EndProcessing"))
                    this.Command.DoEndProcessing();
                // This cmdlet threw an exception, wrap it as needed and bubble it up.
        /// Calls the virtual Complete method after setting the appropriate session state scope.
        internal void DoComplete()
                Complete();
        protected virtual void CleanResource()
                using (commandRuntime.AllowThisCommandToWrite(permittedToWriteToPipeline: true))
                using (ParameterBinderBase.bindingTracer.TraceScope("CALLING CleanResource"))
                    Command.DoCleanResource();
            catch (HaltCommandException)
            catch (FlowControlException)
        internal void DoCleanup()
            // The property 'PropagateExceptionsToEnclosingStatementBlock' controls whether a general exception
            // (an exception thrown from a .NET method invocation, or an expression like '1/0') will be turned
            // into a terminating error, which will be propagated up and thus stop the rest of the running script.
            // It is usually used by TryStatement and TrapStatement, which makes the general exception catch-able.
            // For the 'Clean' block, we don't want to bubble up the general exception when the command is enclosed
            // in a TryStatement or has TrapStatement accompanying, because no exception can escape from 'Clean' and
            // thus it's pointless to bubble up the general exception in this case.
            // Therefore we set this property to 'false' here to mask off the previous setting that could be from a
            // TryStatement or TrapStatement. Example:
            //   PS:1> function b { end {} clean { 1/0; Write-Host 'clean' } }
            //   PS:2> b
            //   RuntimeException: Attempted to divide by zero.
            //   clean
            //   ## Note that, outer 'try/trap' doesn't affect the general exception happens in 'Clean' block.
            //   ## so its behavior is consistent regardless of whether the command is enclosed by 'try/catch' or not.
            //   PS:3> try { b } catch { 'outer catch' }
            // Be noted that, this doesn't affect the TryStatement/TrapStatement within the 'Clean' block. Example:
            //   ## 'try/trap' within 'Clean' block makes the general exception catch-able.
            //   PS:3> function a { end {} clean { try { 1/0; Write-Host 'clean' } catch { Write-Host "caught: $_" } } }
            //   PS:4> a
            //   caught: Attempted to divide by zero.
            bool oldExceptionPropagationState = _context.PropagateExceptionsToEnclosingStatementBlock;
            _context.PropagateExceptionsToEnclosingStatementBlock = false;
                CleanResource();
                _context.PropagateExceptionsToEnclosingStatementBlock = oldExceptionPropagationState;
        internal void ReportCleanupError(Exception exception)
            var error = exception is IContainsErrorRecord icer
                ? icer.ErrorRecord
                : new ErrorRecord(exception, "Clean.ReportException", ErrorCategory.NotSpecified, targetObject: null);
            PSObject errorWrap = PSObject.AsPSObject(error);
            errorWrap.WriteStream = WriteStreamType.Error;
            var errorPipe = commandRuntime.ErrorMergeTo == MshCommandRuntime.MergeDataStream.Output
                ? commandRuntime.OutputPipe
                : commandRuntime.ErrorOutputPipe;
            errorPipe.Add(errorWrap);
            _context.QuestionMarkVariableValue = false;
            if (CommandInfo != null)
                return CommandInfo.ToString();
            return "<NullCommandInfo>"; // does not require localization
        /// True if Read() has not be called, false otherwise.
        /// Entry point used by the engine to reads the input pipeline object
        /// and binds the parameters.
        /// This default implementation reads the next pipeline object and sets
        /// it as the CurrentPipelineObject in the InternalCommand.
        /// Does not throw.
        /// True if read succeeds.
        internal virtual bool Read()
            Command.CurrentPipelineObject = LanguagePrimitives.AsPSObjectOrNull(inputObject);
        /// Wraps the exception which occurred during cmdlet invocation,
        /// stores that as the exception to be returned from
        /// PipelineProcessor.SynchronousExecute, and writes it to
        /// the error variable.
        /// The exception to wrap in a CmdletInvocationException or
        /// CmdletProviderInvocationException.
        /// Always returns PipelineStoppedException.  The caller should
        /// throw this exception.
        /// Almost all exceptions which occur during pipeline invocation
        /// are wrapped in CmdletInvocationException before they are stored
        /// in the pipeline.  However, there are several exceptions:
        /// AccessViolationException, StackOverflowException:
        /// These are considered to be such severe errors that we
        /// FailFast the process immediately.
        /// ProviderInvocationException: In this case, we assume that the
        /// cmdlet is get-item or the like, a thin wrapper around the
        /// provider API.  We discard the original ProviderInvocationException
        /// and re-wrap its InnerException (the real error) in
        /// CmdletProviderInvocationException. This makes it easier to reach
        /// the real error.
        /// CmdletInvocationException, ActionPreferenceStopException:
        /// This indicates that the cmdlet itself ran a command which failed.
        /// We could go ahead and wrap the original exception in multiple
        /// layers of CmdletInvocationException, but this makes it difficult
        /// for the caller to access the root problem, plus the serialization
        /// layer might not communicate properties beyond some fixed depth.
        /// Instead, we choose to not re-wrap the exception.
        /// PipelineStoppedException: This could mean one of two things.
        /// It usually means that this pipeline has already stopped,
        /// in which case the pipeline already stores the original error.
        /// It could also mean that the cmdlet ran a command which was
        /// stopped by CTRL-C etc, in which case we choose not to
        /// re-wrap the exception as with CmdletInvocationException.
        internal PipelineStoppedException ManageInvocationException(Exception e)
                if (Command != null)
                        if (e is ProviderInvocationException pie)
                            // If a ProviderInvocationException occurred, discard the ProviderInvocationException
                            // and re-wrap it in CmdletProviderInvocationException.
                            e = new CmdletProviderInvocationException(pie, Command.MyInvocation);
                        // HaltCommandException will cause the command to stop, but not be reported as an error.
                        // FlowControlException should not be wrapped.
                        if (e is PipelineStoppedException
                            || e is CmdletInvocationException
                            || e is ActionPreferenceStopException
                            || e is HaltCommandException
                            || e is FlowControlException
                            || e is ScriptCallDepthException)
                            // do nothing; do not rewrap these exceptions
                        RuntimeException rte = e as RuntimeException;
                        if (rte != null && rte.WasThrownFromThrowStatement)
                            // do not rewrap a script based throw
                        // wrap all other exceptions
                        e = new CmdletInvocationException(e, Command.MyInvocation);
                    // commandRuntime.ManageException will always throw PipelineStoppedException
                    // Otherwise, just return this exception...
                    // If this exception happened in a transacted cmdlet,
                    // rollback the transaction
                    if (commandRuntime.UseTransaction)
                                    TransactionStrings.TransactionTimedOut),
                            errorRecord.SetInvocationInfo(Command.MyInvocation);
                            e = new CmdletInvocationException(errorRecord);
                        // Rollback the transaction in the case of errors.
                            _context.TransactionManager.HasTransaction
                            _context.TransactionManager.RollbackPreference != RollbackSeverity.Never
                            Context.TransactionManager.Rollback(true);
                    return (PipelineStoppedException)this.commandRuntime.ManageException(e);
                // Upstream cmdlets see only that execution stopped
                // This should only happen if Command is null
                return new PipelineStoppedException();
                // this method should not throw exceptions; warn about any violations on checked builds and re-throw
                Diagnostics.Assert(false, "This method should not throw exceptions!");
        /// Stores the exception to be returned from
        /// The exception which occurred during script execution
        /// ManageScriptException throws PipelineStoppedException if-and-only-if
        /// the exception is a RuntimeException, otherwise it returns.
        /// This allows the caller to rethrow unexpected exceptions.
        internal void ManageScriptException(RuntimeException e)
            if (Command != null && commandRuntime.PipelineProcessor != null)
                commandRuntime.PipelineProcessor.RecordFailure(e, Command);
                // An explicit throw is written to $error as an ErrorRecord, so we
                // skip adding what is more or less a duplicate.
                if (e is not PipelineStoppedException && !e.WasThrownFromThrowStatement)
                    commandRuntime.AppendErrorToVariables(e);
        /// Sometimes we shouldn't be rethrow the exception we previously caught,
        /// such as when the exception is handled by a trap.
        internal void ForgetScriptException()
                commandRuntime.PipelineProcessor.ForgetFailure();
        #endregion methods
        #region IDispose
        // 2004/03/05-JonN BrucePay has suggested that the IDispose
        // implementations in PipelineProcessor and CommandProcessor can be
        // removed.
        /// When the command is complete, the CommandProcessorBase should be disposed.
        /// This enables cmdlets to reliably release file handles etc.
        /// without waiting for garbage collection.
        /// <remarks>We use the standard IDispose pattern</remarks>
                    // Clean up the PS drives that are associated with this local scope.
                    // This operation may be needed at multiple stages depending on whether the 'clean' block is declared:
                    //  1. when there is a 'clean' block, it needs to be done only after 'clean' block runs, because the scope
                    //     needs to be preserved until the 'clean' block finish execution.
                    //  2. when there is no 'clean' block, it needs to be done when
                    //      (1) there is any exception thrown from 'DoPrepare()', 'DoBegin()', 'DoExecute()', or 'DoComplete';
                    //      (2) OR, the command runs to the end successfully;
                    // Doing this cleanup at those multiple stages is cumbersome. Since we will always dispose the command in
                    // the end, doing this cleanup here will cover all the above cases.
                    CommandSessionState.RemoveScope(CommandScope);
                if (Command is IDisposable id)
                    id.Dispose();
        #endregion IDispose

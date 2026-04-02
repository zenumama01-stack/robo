    /// Pipeline class to be used for LocalRunspace.
    internal sealed class LocalPipeline : PipelineBase
        // Each OS platform uses different default stack size for threads:
        //      - Windows 2 MB
        //      - Linux   8 Mb
        //      - MacOs   512 KB
        // We should use the same stack size for pipeline threads on all platforms to get predictable behavior.
        // The stack size we use for pipeline threads is 10MB, which is inherited from Windows PowerShell.
        internal const int DefaultPipelineStackSize = 10_000_000;
        /// Create a Pipeline with an existing command string.
        /// <param name="runspace">The LocalRunspace to associate with this
        /// pipeline.
        /// <param name="command">The command string to parse.</param>
        /// <param name="addToHistory">If true, add pipeline to history.</param>
        internal LocalPipeline(LocalRunspace runspace, string command, bool addToHistory, bool isNested)
            : base((Runspace)runspace, command, addToHistory, isNested)
            _stopper = new PipelineStopper(this);
            InitStreams();
        /// Caller should validate all the parameters.
        /// The LocalRunspace to associate with this pipeline.
        /// The command to execute.
        /// <param name="addToHistory">
        /// If true, add the command(s) to the history list of the runspace.
        /// <param name="isNested">
        /// If true, mark this pipeline as a nested pipeline.
        /// <param name="inputStream">
        /// Stream to use for reading input objects.
        /// <param name="errorStream">
        /// Stream to use for writing error objects.
        /// <param name="outputStream">
        /// Stream to use for writing output objects.
        /// <param name="infoBuffers">
        /// Buffers used to write progress, verbose, debug, warning, information
        /// information of an invocation.
        internal LocalPipeline(LocalRunspace runspace,
            CommandCollection command,
            bool isNested,
            ObjectStreamBase inputStream,
            ObjectStreamBase outputStream,
            ObjectStreamBase errorStream,
            PSInformationalBuffers infoBuffers)
            : base(runspace, command, addToHistory, isNested, inputStream, outputStream, errorStream, infoBuffers)
        /// <param name="pipeline">The source pipeline.</param>
        internal LocalPipeline(LocalPipeline pipeline)
            : base((PipelineBase)(pipeline))
        /// Creates a new <see cref="Pipeline"/> that is a copy of the current instance.
        /// <returns>A new <see cref="Pipeline"/> that is a copy of this instance.</returns>
        public override Pipeline Copy()
                throw PSTraceSource.NewObjectDisposedException("pipeline");
            return (Pipeline)new LocalPipeline(this);
        /// Invoke the pipeline asynchronously with input.
        /// Results are returned through the <see cref="Pipeline.Output"/> reader.
        protected override void StartPipelineExecution()
            // Note:This method is called from within a lock by parent class. There
            // is no need to lock further.
            // Use input stream in two cases:
            // 1)inputStream is open. In this case PipelineProcessor
            // will call Invoke only if at least one object is added
            // to inputStream.
            // 2)inputStream is closed but there are objects in the stream.
            // NTRAID#Windows Out Of Band Releases-925566-2005/12/09-JonN
            // Remember this here, in the synchronous thread,
            // to avoid timing dependencies in the pipeline thread.
            _useExternalInput = (InputStream.IsOpen || InputStream.Count > 0);
            PSThreadOptions memberOptions = this.IsNested ? PSThreadOptions.UseCurrentThread : this.LocalRunspace.ThreadOptions;
            // Use thread proc that supports impersonation flow for new thread start.
            ThreadStart invokeThreadProcDelegate = InvokeThreadProcImpersonate;
            _identityToImpersonate = null;
            // If impersonation identity flow is requested, then get current thread impersonation, if any.
            if ((InvocationSettings != null) && InvocationSettings.FlowImpersonationPolicy)
                Utils.TryGetWindowsImpersonatedIdentity(out _identityToImpersonate);
            // UNIX does not support thread impersonation flow.
            ThreadStart invokeThreadProcDelegate = InvokeThreadProc;
            switch (memberOptions)
                case PSThreadOptions.Default:
                case PSThreadOptions.UseNewThread:
                        // Start execution of pipeline in another thread,
                        // and support impersonation flow as needed (Windows only).
                        Thread invokeThread = new Thread(new ThreadStart(invokeThreadProcDelegate), DefaultPipelineStackSize);
                        SetupInvokeThread(invokeThread, true);
                        ApartmentState apartmentState;
                        if (InvocationSettings != null && InvocationSettings.ApartmentState != ApartmentState.Unknown)
                            apartmentState = InvocationSettings.ApartmentState; // set the user-defined apartmentstate.
                            apartmentState = this.LocalRunspace.ApartmentState; // use the Runspace apartment state
                        if (apartmentState != ApartmentState.Unknown && Platform.IsStaSupported)
                            invokeThread.SetApartmentState(apartmentState);
                        invokeThread.Start();
                case PSThreadOptions.ReuseThread:
                        if (this.IsNested)
                            // If this a nested pipeline we are already in the appropriate thread so we just execute the pipeline here.
                            // Impersonation flow (Windows only) is not needed when using existing thread.
                            SetupInvokeThread(Thread.CurrentThread, true);
                            InvokeThreadProc();
                            // Otherwise we execute the pipeline in the Runspace's thread,
                            // and support information flow on new thread as needed (Windows only).
                            PipelineThread invokeThread = this.LocalRunspace.GetPipelineThread();
                            SetupInvokeThread(invokeThread.Worker, true);
                            invokeThread.Start(invokeThreadProcDelegate);
                case PSThreadOptions.UseCurrentThread:
                        Thread oldNestedPipelineThread = NestedPipelineExecutionThread;
                        CultureInfo oldCurrentCulture = CultureInfo.CurrentCulture;
                        CultureInfo oldCurrentUICulture = CultureInfo.CurrentUICulture;
                            // Prepare invoke thread.
                            SetupInvokeThread(Thread.CurrentThread, false);
                            NestedPipelineExecutionThread = oldNestedPipelineThread;
                            Thread.CurrentThread.CurrentCulture = oldCurrentCulture;
                            Thread.CurrentThread.CurrentUICulture = oldCurrentUICulture;
                    Debug.Fail(string.Empty);
        /// Prepares the invoke thread for execution.
        private void SetupInvokeThread(Thread invokeThread, bool changeName)
            NestedPipelineExecutionThread = invokeThread;
#if !CORECLR // No Thread.CurrentCulture In CoreCLR
            invokeThread.CurrentCulture = this.LocalRunspace.ExecutionContext.EngineHostInterface.CurrentCulture;
            invokeThread.CurrentUICulture = this.LocalRunspace.ExecutionContext.EngineHostInterface.CurrentUICulture;
            if ((invokeThread.Name == null) && changeName) // setup the invoke thread only once
                invokeThread.Name = "Pipeline Execution Thread";
        /// Helper method for asynchronous invoke
        /// <returns>Unhandled FlowControl exception if InvocationSettings.ExposeFlowControlExceptions is true.</returns>
        private FlowControlException InvokeHelper()
            FlowControlException flowControlException = null;
            PipelineProcessor pipelineProcessor = null;
                // Raise the event for Pipeline.Running
                RaisePipelineStateEvents();
                // Add this pipeline to history
                RecordPipelineStartTime();
                // Add automatic transcription when it's NOT a pulse pipeline, but don't transcribe nested commands.
                if (!IsPulsePipeline && (AddToHistory || !IsNested))
                    foreach (Command command in Commands)
                        if (command.IsScript)
                            // Transcribe scripts, unless they are the pulse pipeline.
                            Runspace.GetExecutionContext.EngineHostInterface.UI.TranscribeCommand(command.CommandText, invocation: null);
                    if (Runspace.GetExecutionContext.EngineHostInterface.UI.IsTranscribing)
                        bool needToAddOutDefault = true;
                        Command lastCommand = Commands[Commands.Count - 1];
                        // Don't need to add Out-Default if the pipeline already has it, or we've got a pipeline evaluating
                        // the PSConsoleHostReadLine or the TabExpansion2 commands.
                        if (string.Equals("Out-Default", lastCommand.CommandText, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals("PSConsoleHostReadLine", lastCommand.CommandText, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals("TabExpansion2", lastCommand.CommandText, StringComparison.OrdinalIgnoreCase) ||
                            (lastCommand.CommandInfo is CmdletInfo cmdlet && cmdlet.ImplementingType == typeof(OutDefaultCommand)))
                            needToAddOutDefault = false;
                        if (needToAddOutDefault)
                            var outDefaultCommand = new Command(
                                    "Out-Default",
                                    typeof(OutDefaultCommand),
                                    helpFile: null,
                                    PSSnapin: null,
                                    context: null));
                            outDefaultCommand.Parameters.Add(new CommandParameter("Transcript", true));
                            outDefaultCommand.Parameters.Add(new CommandParameter("OutVariable", null));
                            Commands.Add(outDefaultCommand);
                    // Create PipelineProcessor to invoke this pipeline
                    pipelineProcessor = CreatePipelineProcessor();
                    if (this.SetPipelineSessionState)
                        SetHadErrors(true);
                        Runspace.ExecutionContext.AppendDollarError(ex);
                // Supply input stream to PipelineProcessor
                if (_useExternalInput)
                    pipelineProcessor.ExternalInput = InputStream.ObjectReader;
                pipelineProcessor.ExternalSuccessOutput = OutputStream.ObjectWriter;
                pipelineProcessor.ExternalErrorOutput = ErrorStream.ObjectWriter;
                // Set Informational Buffers on the host only if this is not a child.
                // Do not overwrite parent's informational buffers.
                if (!this.IsChild)
                    LocalRunspace.ExecutionContext.InternalHost.InternalUI.SetInformationalMessageBuffers(InformationalBuffers);
                bool oldQuestionMarkValue = true;
                bool savedIgnoreScriptDebug = this.LocalRunspace.ExecutionContext.IgnoreScriptDebug;
                // preserve the trap behaviour state variable...
                bool oldTrapState = this.LocalRunspace.ExecutionContext.PropagateExceptionsToEnclosingStatementBlock;
                this.LocalRunspace.ExecutionContext.PropagateExceptionsToEnclosingStatementBlock = false;
                    // Add this pipeline to stopper
                    _stopper.Push(pipelineProcessor);
                    // Preserve the last value of $? across non-interactive commands.
                    if (!AddToHistory)
                        oldQuestionMarkValue = this.LocalRunspace.ExecutionContext.QuestionMarkVariableValue;
                        this.LocalRunspace.ExecutionContext.IgnoreScriptDebug = true;
                        this.LocalRunspace.ExecutionContext.IgnoreScriptDebug = false;
                    // Reset the redirection only if the pipeline is neither nested nor is a pulse pipeline (created by EventManager)
                    if (!this.IsNested && !this.IsPulsePipeline)
                        this.LocalRunspace.ExecutionContext.ResetRedirection();
                    // Invoke the pipeline.
                    // Note:Since we are using pipes for output, return array is
                    // be empty.
                        pipelineProcessor.SynchronousExecuteEnumerate(AutomationNull.Value);
                        SetHadErrors(pipelineProcessor.ExecutionFailed);
                        // The 'exit' command was run so tell the host to exit.
                        // Use the finally clause to make sure that the call is actually made.
                        // We'll default the exit code to 1 instead or zero so that if, for some
                        // reason, we can't get the real error code, we'll indicate a failure.
                        int exitCode = 1;
                        if (IsNested)
                            // set the global LASTEXITCODE to the value passed by exit <code>
                                this.LocalRunspace.ExecutionContext.SetVariable(SpecialVariables.LastExitCodeVarPath, exitCode);
                                    this.LocalRunspace.ExecutionContext.EngineHostInterface.ExitNestedPrompt();
                                    // Already at the top level so we just want to ignore this exception...
                                if ((InvocationSettings != null) && (InvocationSettings.ExposeFlowControlExceptions))
                                    flowControlException = ee;
                                this.LocalRunspace.ExecutionContext.EngineHostInterface.SetShouldExit(exitCode);
                        if ((InvocationSettings != null) && (InvocationSettings.ExposeFlowControlExceptions) &&
                            ((e is BreakException) || (e is ContinueException) || (e is TerminateException)))
                            // Save FlowControl exception for return to caller.
                            flowControlException = e;
                        // Otherwise discard this type of exception generated by the debugger or from an unhandled break, continue or return.
                        // Indicate that there were errors then rethrow...
                    if (pipelineProcessor != null && pipelineProcessor.Commands != null)
                        for (int i = 0; i < pipelineProcessor.Commands.Count; i++)
                            CommandProcessorBase commandProcessor = pipelineProcessor.Commands[i];
                            // Log a command terminated event
                    PSLocalEventManager eventManager = LocalRunspace.Events as PSLocalEventManager;
                    eventManager?.ProcessPendingActions();
                    // restore the trap state...
                    this.LocalRunspace.ExecutionContext.PropagateExceptionsToEnclosingStatementBlock = oldTrapState;
                    // clean the buffers on InternalHost only if this is not a child.
                    // Do not clear parent's informational buffers.
                    if (!IsChild)
                        LocalRunspace.ExecutionContext.InternalHost.InternalUI.SetInformationalMessageBuffers(null);
                    // Pop the pipeline processor from stopper.
                    _stopper.Pop(false);
                        this.LocalRunspace.ExecutionContext.QuestionMarkVariableValue = oldQuestionMarkValue;
                    // Restore the IgnoreScriptDebug value.
                    this.LocalRunspace.ExecutionContext.IgnoreScriptDebug = savedIgnoreScriptDebug;
                // Discard this type of exception generated by the debugger or from an unhandled break, continue or return.
                // 2004/02/26-JonN added IDisposable to PipelineProcessor
                if (pipelineProcessor != null)
                    pipelineProcessor.Dispose();
                    pipelineProcessor = null;
            return flowControlException;
        /// Invokes the InvokeThreadProc() method on new thread, and flows calling thread
        /// impersonation as needed.
        private void InvokeThreadProcImpersonate()
            if (_identityToImpersonate != null)
                    _identityToImpersonate.AccessToken,
                    () => InvokeThreadProc());
        /// Start thread method for asynchronous pipeline execution.
        private void InvokeThreadProc()
            bool incompleteParseException = false;
            Runspace previousDefaultRunspace = Runspace.DefaultRunspace;
                // Set up pipeline internal host if it is available.
                if (InvocationSettings != null && InvocationSettings.Host != null)
                    InternalHost internalHost = InvocationSettings.Host as InternalHost;
                    if (internalHost != null) // if we are given an internal host, use the external host
                        LocalRunspace.ExecutionContext.InternalHost.SetHostRef(internalHost.ExternalHost);
                        LocalRunspace.ExecutionContext.InternalHost.SetHostRef(InvocationSettings.Host);
                if (LocalRunspace.ExecutionContext.InternalHost.ExternalHost.ShouldSetThreadUILanguageToZero)
                    //  BUG: 610329. Pipeline execution happens in a new thread. For
                    //  Console applications SetThreadUILanguage(0) must be called
                    //  inorder for the native MUI loader to load the resources correctly.
                    //  ConsoleHost already does this in its entry point..but the same
                    //  call is not performed in the Pipeline execution threads causing
                    //  cmdlets that load native resources show unreadable messages on
                    //  the console.
                    Microsoft.PowerShell.NativeCultureResolver.SetThreadUILanguage(0);
                // Put Execution Context In TLS
                Runspace.DefaultRunspace = this.LocalRunspace;
                FlowControlException flowControlException = InvokeHelper();
                if (flowControlException != null)
                    // Let pipeline propagate the BreakException.
                    SetPipelineState(Runspaces.PipelineState.Failed, flowControlException);
                    // Invoke finished successfully. Set state to Completed.
                    SetPipelineState(PipelineState.Completed);
            catch (PipelineStoppedException ex)
                SetPipelineState(PipelineState.Stopped, ex);
                incompleteParseException = ex is IncompleteParseException;
                SetPipelineState(PipelineState.Failed, ex);
            catch (ScriptCallDepthException ex)
                // Remove pipeline specific host if it was set.
                // Win8:464422 Revert the host only if this pipeline invocation changed it
                // with 464422 a nested pipeline reverts the host, although the nested pipeline did not set it.
                if ((InvocationSettings != null && InvocationSettings.Host != null) &&
                    (LocalRunspace.ExecutionContext.InternalHost.IsHostRefSet))
                    LocalRunspace.ExecutionContext.InternalHost.RevertHostRef();
                // Remove Execution Context From TLS
                Runspace.DefaultRunspace = previousDefaultRunspace;
                // If incomplete parse exception is hit, we should not add to history.
                // This is ensure that in case of multiline commands, command is in the
                // history only once.
                if (!incompleteParseException)
                        // do not update the history if we are in the debugger and the history is locked, since that may go into a deadlock
                        bool skipIfLocked = LocalRunspace.ExecutionContext.Debugger.InBreakpoint;
                        if (_historyIdForThisPipeline == -1)
                            AddHistoryEntry(skipIfLocked);
                            UpdateHistoryEntryAddedByAddHistoryCmdlet(skipIfLocked);
                    // Updating the history may trigger variable breakpoints; the debugger may throw a TerminateException to
                    // indicate that the user wants to interrupt the variable access.
                // IsChild makes it possible for LocalPipeline to differentiate
                // between a true v1 nested pipeline and the "Cmdlets Calling Cmdlets" case.
                // Close the output stream if it is not closed.
                if (OutputStream.IsOpen && !IsChild)
                        OutputStream.Close();
                // Close the error stream if it is not closed.
                if (ErrorStream.IsOpen && !IsChild)
                        ErrorStream.Close();
                // Close the input stream if it is not closed.
                if (InputStream.IsOpen && !IsChild)
                        InputStream.Close();
                // Clear stream links from ExecutionContext
                ClearStreams();
                // Runspace object maintains a list of pipelines in execution.
                // Remove this pipeline from the list. This method also calls the
                // pipeline finished event.
                LocalRunspace.RemoveFromRunningPipelineList(this);
                // If async call raise the event here. For sync invoke call,
                // thread on which invoke is called will raise the event.
                if (!SyncInvokeCall)
                    // This should be called after signaling PipelineFinishedEvent and
                    // RemoveFromRunningPipelineList. If it is done before, and in the
                    // Event, Runspace.Close is called which waits for pipeline to close.
                    // We will have deadlock
        #region stop
        /// Stop the running pipeline.
        /// <param name="syncCall">If true pipeline is stopped synchronously
        /// else asynchronously.</param>
        protected override void ImplementStop(bool syncCall)
                StopHelper();
                Thread stopThread = new Thread(new ThreadStart(this.StopThreadProc));
                stopThread.Start();
        /// Start method for asynchronous Stop.
        private void StopThreadProc()
        private readonly PipelineStopper _stopper;
        /// Gets PipelineStopper object which maintains stack of PipelineProcessor
        /// for this pipeline.
        internal PipelineStopper Stopper
                return _stopper;
        /// Helper method for Stop functionality.
        private void StopHelper()
            // Ensure that any saved debugger stop is released
            LocalRunspace.ReleaseDebugger();
            // first stop all child pipelines of this pipeline
            LocalRunspace.StopNestedPipelines(this);
            // close the input pipe if it hasn't been closed.
            // This would release the pipeline thread if it is
            // waiting for input.
            if (InputStream.IsOpen)
            _stopper.Stop();
            // Wait for pipeline to finish
            PipelineFinishedEvent.WaitOne();
        /// Returns true if pipeline is stopping.
                return _stopper.IsStopping;
        #endregion stop
        /// Creates a PipelineProcessor object from LocalPipeline object.
        /// <returns>Created PipelineProcessor object.</returns>
        private PipelineProcessor CreatePipelineProcessor()
            CommandCollection commands = Commands;
            if (commands == null || commands.Count == 0)
                throw PSTraceSource.NewInvalidOperationException(RunspaceStrings.NoCommandInPipeline);
            PipelineProcessor pipelineProcessor = new PipelineProcessor();
            pipelineProcessor.TopLevel = true;
            bool failed = false;
                foreach (Command command in commands)
                    // If CommandInfo is null, proceed with CommandDiscovery to resolve the command name
                    if (command.CommandInfo == null)
                            CommandOrigin commandOrigin = command.CommandOrigin;
                            // Do not set command origin to internal if this is a script debugger originated command (which always
                            // runs nested commands).  This prevents the script debugger command line from seeing private commands.
                            if (IsNested &&
                                !LocalRunspace.InNestedPrompt &&
                                !((LocalRunspace.Debugger != null) && (LocalRunspace.Debugger.InBreakpoint)))
                                commandOrigin = CommandOrigin.Internal;
                            commandProcessorBase =
                                command.CreateCommandProcessor
                                        LocalRunspace.ExecutionContext,
                                        AddToHistory,
                                        commandOrigin
                            // If we had an error creating a command processor and we are logging, then
                            // log the attempted command invocation anyways.
                            if (this.Runspace.GetExecutionContext.EngineHostInterface.UI.IsTranscribing)
                                // Don't need to log script commands, as they were already logged during pipeline
                                // setup
                                if (!command.IsScript)
                                    this.Runspace.ExecutionContext.InternalHost.UI.TranscribeCommand(command.CommandText, null);
                        commandProcessorBase = CreateCommandProcessBase(command);
                        commandProcessorBase.Command.CommandOriginInternal = CommandOrigin.Internal;
                        commandProcessorBase.Command.MyInvocation.InvocationName = command.CommandInfo.Name;
                        if (command.Parameters != null)
                            foreach (CommandParameter publicParameter in command.Parameters)
                                CommandParameterInternal internalParameter = CommandParameter.ToCommandParameterInternal(publicParameter, false);
                    commandProcessorBase.RedirectShellErrorOutputPipe = this.RedirectShellErrorOutputPipe;
                    pipelineProcessor.Add(commandProcessorBase);
                return pipelineProcessor;
                failed = true;
                throw new RuntimeException(PipelineStrings.CannotCreatePipeline, e);
                if (failed)
                    this.SetHadErrors(true);
        /// Resolves command.CommandInfo to an appropriate CommandProcessorBase implementation.
        /// <param name="command">Command to resolve.</param>
        private CommandProcessorBase CreateCommandProcessBase(Command command)
            CommandInfo commandInfo = command.CommandInfo;
                commandInfo = ((AliasInfo)commandInfo).ReferencedCommand;
            CmdletInfo cmdletInfo = commandInfo as CmdletInfo;
                return new CommandProcessor(cmdletInfo, LocalRunspace.ExecutionContext);
            IScriptCommandInfo functionInfo = commandInfo as IScriptCommandInfo;
            if (functionInfo != null)
                return new CommandProcessor(functionInfo, LocalRunspace.ExecutionContext,
                    useLocalScope: false, fromScriptFile: false, sessionState: LocalRunspace.ExecutionContext.EngineSessionState);
            ApplicationInfo applicationInfo = commandInfo as ApplicationInfo;
            if (applicationInfo != null)
                return new NativeCommandProcessor(applicationInfo, LocalRunspace.ExecutionContext);
        /// This method initializes streams and backs up their original states.
        /// This should be only called from constructors.
        private void InitStreams()
            if (LocalRunspace.ExecutionContext != null)
                _oldExternalErrorOutput = LocalRunspace.ExecutionContext.ExternalErrorOutput;
                _oldExternalSuccessOutput = LocalRunspace.ExecutionContext.ExternalSuccessOutput;
                LocalRunspace.ExecutionContext.ExternalErrorOutput = ErrorStream.ObjectWriter;
                LocalRunspace.ExecutionContext.ExternalSuccessOutput = OutputStream.ObjectWriter;
        /// This method sets streams to their original states from execution context.
        /// This is done when Pipeline is completed/failed/stopped ie., termination state.
        private void ClearStreams()
                LocalRunspace.ExecutionContext.ExternalErrorOutput = _oldExternalErrorOutput;
                LocalRunspace.ExecutionContext.ExternalSuccessOutput = _oldExternalSuccessOutput;
        // History object for this pipeline
        private DateTime _pipelineStartTime;
        /// Adds an entry in history for this pipeline.
        private void RecordPipelineStartTime()
            _pipelineStartTime = DateTime.Now;
        /// Add HistoryEntry for this pipeline. Use this function when writing
        /// history at the end of pipeline.
        private void AddHistoryEntry(bool skipIfLocked)
            // History id is greater than zero if entry was added to history
            if (AddToHistory)
                LocalRunspace.History.AddEntry(InstanceId, HistoryString, PipelineState, _pipelineStartTime, DateTime.Now, skipIfLocked);
        private long _historyIdForThisPipeline = -1;
        /// This method is called Add-History cmdlet to add history entry.
        /// In general history entry for current pipeline is added at the
        /// end of pipeline execution.
        /// However when add-history cmdlet is executed, history entry
        /// needs to be added before add-history adds additional entries
        /// in to history.
        void AddHistoryEntryFromAddHistoryCmdlet()
            // This method can be called by multiple times during a single
            // pipeline execution. For ex: a script can execute add-history
            // command multiple times. However we should add entry only
            // once.
            if (_historyIdForThisPipeline != -1)
                _historyIdForThisPipeline = LocalRunspace.History.AddEntry(InstanceId, HistoryString, PipelineState, _pipelineStartTime, DateTime.Now, false);
        /// Add-history cmdlet adds history entry for the pipeline in its
        /// begin processing. This method is called to update the end execution
        /// time and status of pipeline.
        void UpdateHistoryEntryAddedByAddHistoryCmdlet(bool skipIfLocked)
            if (AddToHistory && _historyIdForThisPipeline != -1)
                LocalRunspace.History.UpdateEntry(_historyIdForThisPipeline, PipelineState, DateTime.Now, skipIfLocked);
        /// Sets the history string to the specified one.
        /// <param name="historyString">History string to set to.</param>
        internal override void SetHistoryString(string historyString)
            HistoryString = historyString;
        #region TLS
        /// Gets the execution context in the thread local storage of current
        /// thread.
        /// ExecutionContext, if it available in TLS
        /// Null, if ExecutionContext is not available in TLS
        internal static ExecutionContext GetExecutionContextFromTLS()
            Runspace runspace = Runspace.DefaultRunspace;
            return runspace.ExecutionContext;
        #endregion TLS
        /// Holds reference to LocalRunspace to which this pipeline is
        /// associated with.
        private LocalRunspace LocalRunspace
                return (LocalRunspace)Runspace;
        private bool _useExternalInput;
        private PipelineWriter _oldExternalErrorOutput;
        private PipelineWriter _oldExternalSuccessOutput;
        private WindowsIdentity _identityToImpersonate;
        Dispose(bool disposing)
                        Stop();
    /// Helper class that holds the thread used to execute pipelines when CreateThreadOptions.ReuseThread is used.
    internal class PipelineThread : IDisposable
        /// Creates the worker thread and waits for it to be ready.
        internal PipelineThread(ApartmentState apartmentState)
            _worker = new Thread(WorkerProc, LocalPipeline.DefaultPipelineStackSize);
            _workItem = null;
            _workItemReady = new AutoResetEvent(false);
            _closed = false;
                _worker.SetApartmentState(apartmentState);
        /// Returns the worker thread.
        internal Thread Worker
                return _worker;
        /// Posts an item to the worker thread and wait for its completion.
        internal void Start(ThreadStart workItem)
            if (_closed)
            _workItem = workItem;
            _workItemReady.Set();
            if (_worker.ThreadState == System.Threading.ThreadState.Unstarted)
                _worker.Start();
        /// Shortcut for dispose.
        internal void Close()
        /// Implementation of the worker thread.
        private void WorkerProc()
            while (!_closed)
                _workItemReady.WaitOne();
                if (!_closed)
                    _workItem();
        /// Releases the worker thread.
            _closed = true;
            if (_worker.ThreadState != System.Threading.ThreadState.Unstarted && Thread.CurrentThread != _worker)
                _worker.Join();
            _workItemReady.Dispose();
        /// Finalizes an instance of the <see cref="PipelineThread"/> class.
        ~PipelineThread()
        private readonly Thread _worker;
        private ThreadStart _workItem;
        private readonly AutoResetEvent _workItemReady;
        private bool _closed;
    /// This is helper class for stopping a running pipeline. This
    /// class maintains a stack of currently active pipeline processors.
    /// To stop a pipeline, stop is called on each pipeline processor
    /// in the stack.
    internal class PipelineStopper
        /// Stack of current executing pipeline processor.
        private readonly Stack<PipelineProcessor> _stack = new Stack<PipelineProcessor>();
        private readonly LocalPipeline _localPipeline;
        internal PipelineStopper(LocalPipeline localPipeline)
            _localPipeline = localPipeline;
        /// This is set true when stop is called.
        private bool _stopping;
                return _stopping;
                _stopping = value;
        /// Push item in to PipelineProcessor stack.
        internal void Push(PipelineProcessor item)
                throw PSTraceSource.NewArgumentNullException(nameof(item));
                _stack.Push(item);
            item.LocalPipeline = _localPipeline;
        /// Pop top item from PipelineProcessor stack.
        internal void Pop(bool fromSteppablePipeline)
                // If we are stopping, Stop will pop the entire stack, so
                // we shouldn't do any popping or some PipelineProcessor won't
                // be notified that it is being stopped.
                if (_stack.Count > 0)
                    PipelineProcessor oldPipe = _stack.Pop();
                    if (fromSteppablePipeline && oldPipe.ExecutionFailed && _stack.Count > 0)
                        _stack.Peek().ExecutionFailed = true;
                    // If this is the last pipeline processor on the stack, then propagate it's execution status
                    if (_stack.Count == 1 && _localPipeline != null)
                        _localPipeline.SetHadErrors(oldPipe.ExecutionFailed);
            PipelineProcessor[] copyStack;
                copyStack = _stack.ToArray();
            // Propagate error from the toplevel operation through to enclosing the LocalPipeline.
            if (copyStack.Length > 0)
                PipelineProcessor topLevel = copyStack[copyStack.Length - 1];
                if (topLevel != null && _localPipeline != null)
                    _localPipeline.SetHadErrors(topLevel.ExecutionFailed);
            // Note: after _stopping is set to true, nothing can be pushed/popped
            // from stack and it is safe to call stop on PipelineProcessors in stack
            // outside the lock
            // Note: you want to do below loop outside the lock so that
            // pipeline execution thread doesn't get blocked on Push and Pop.
            // Note: A copy of the stack is made because we "unstop" a stopped
            // pipeline to execute finally blocks.  We don't want to stop pipelines
            // in the finally block though.
            foreach (PipelineProcessor pp in copyStack)
                pp.Stop();

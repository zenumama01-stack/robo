    #region Event Args
    /// Possible actions for the debugger after hitting a breakpoint/step.
    public enum DebuggerResumeAction
        /// Continue running until the next breakpoint, or the end of the script.
        Continue = 0,
        /// Step to next statement, going into functions, scripts, etc.
        StepInto = 1,
        /// Step to next statement, going over functions, scripts, etc.
        StepOut = 2,
        /// Step to next statement after the current function, script, etc.
        StepOver = 3,
        /// Stop executing the script.
        Stop = 4,
    /// Arguments for the DebuggerStop event.
    public class DebuggerStopEventArgs : EventArgs
        /// Initializes the DebuggerStopEventArgs.
        internal DebuggerStopEventArgs(InvocationInfo invocationInfo, List<Breakpoint> breakpoints)
            this.InvocationInfo = invocationInfo;
            this.Breakpoints = new ReadOnlyCollection<Breakpoint>(breakpoints);
            this.ResumeAction = DebuggerResumeAction.Continue;
        /// <param name="invocationInfo"></param>
        /// <param name="breakpoints"></param>
        /// <param name="resumeAction"></param>
        public DebuggerStopEventArgs(
            Collection<Breakpoint> breakpoints,
            DebuggerResumeAction resumeAction)
            this.ResumeAction = resumeAction;
        /// Invocation info of the code being executed.
        public InvocationInfo InvocationInfo { get; internal set; }
        /// The breakpoint(s) hit.
        /// Note there may be more than one breakpoint on the same object (line, variable, command). A single event is
        /// raised for all these breakpoints.
        public ReadOnlyCollection<Breakpoint> Breakpoints { get; }
        /// This property must be set in the event handler to indicate the debugger what it should do next.
        /// The default action is DebuggerAction.Continue.
        /// DebuggerAction.StepToLine is only valid when debugging an script.
        public DebuggerResumeAction ResumeAction { get; set; }
        /// This property is used internally for remote debug stops only.  It is used to signal the remote debugger proxy
        /// that it should *not* send a resume action to the remote debugger.  This is used by runspace debug processing to
        /// leave pending runspace debug sessions suspended until a debugger is attached.
        internal bool SuspendRemote { get; set; }
    /// Kinds of breakpoint updates.
    public enum BreakpointUpdateType
        /// A breakpoint was set.
        Set = 0,
        /// A breakpoint was removed.
        Removed = 1,
        /// A breakpoint was enabled.
        Enabled = 2,
        /// A breakpoint was disabled.
        Disabled = 3
    /// Arguments for the BreakpointUpdated event.
    public class BreakpointUpdatedEventArgs : EventArgs
        /// Initializes the BreakpointUpdatedEventArgs.
        internal BreakpointUpdatedEventArgs(Breakpoint breakpoint, BreakpointUpdateType updateType, int breakpointCount)
            this.Breakpoint = breakpoint;
            this.UpdateType = updateType;
            this.BreakpointCount = breakpointCount;
        /// Gets the breakpoint that was updated.
        public Breakpoint Breakpoint { get; }
        /// Gets the type of update.
        public BreakpointUpdateType UpdateType { get; }
        /// Gets the current breakpoint count.
        public int BreakpointCount { get; }
    #region PSJobStartEventArgs
    /// Arguments for the script job start callback event.
    public sealed class PSJobStartEventArgs : EventArgs
        /// Job to be started.
        public Job Job { get; }
        /// Job debugger.
        public Debugger Debugger { get; }
        /// Job is run asynchronously.
        public bool IsAsync { get; }
        /// <param name="job">Started job.</param>
        /// <param name="debugger">Debugger.</param>
        /// <param name="isAsync">Job started asynchronously.</param>
        public PSJobStartEventArgs(Job job, Debugger debugger, bool isAsync)
            this.Job = job;
            this.Debugger = debugger;
            this.IsAsync = isAsync;
    #region Runspace Debug Processing
    /// StartRunspaceDebugProcessing event arguments.
    public sealed class StartRunspaceDebugProcessingEventArgs : EventArgs
        /// <summary> The runspace to process </summary>
        public Runspace Runspace { get; }
        /// When set to true this will cause PowerShell to process this runspace debug session through its
        /// script debugger.  To use the default processing return from this event call after setting
        /// this property to true.
        public bool UseDefaultProcessing
        public StartRunspaceDebugProcessingEventArgs(Runspace runspace)
            if (runspace == null) { throw new PSArgumentNullException(nameof(runspace)); }
            Runspace = runspace;
    /// ProcessRunspaceDebugEnd event arguments.
    public sealed class ProcessRunspaceDebugEndEventArgs : EventArgs
        /// The runspace where internal debug processing has ended.
        /// <param name="runspace"></param>
        public ProcessRunspaceDebugEndEventArgs(Runspace runspace)
    /// Defines debugging mode.
    public enum DebugModes
        /// PowerShell script debugging is disabled.
        /// Default setting for original PowerShell script debugging.
        /// Compatible with PowerShell Versions 2 and 3.
        /// PowerShell script debugging.
        LocalScript = 0x2,
        /// PowerShell remote script debugging.
        RemoteScript = 0x4
    /// Defines unhandled breakpoint processing behavior.
    internal enum UnhandledBreakpointProcessingMode
        /// Ignore unhandled breakpoint events.
        Ignore = 1,
        /// Wait on unhandled breakpoint events until a handler is available.
        Wait
    #region Debugger base class
    /// Base class for all PowerShell debuggers.
    public abstract class Debugger
        /// Event raised when the debugger hits a breakpoint or a step.
        public event EventHandler<DebuggerStopEventArgs> DebuggerStop;
        /// Event raised when a breakpoint is updated.
        public event EventHandler<BreakpointUpdatedEventArgs> BreakpointUpdated;
        /// Event raised when nested debugging is cancelled.
        internal event EventHandler<EventArgs> NestedDebuggingCancelledEvent;
        #region Runspace Debug Processing Events
        /// Event raised when a runspace debugger needs breakpoint processing.
        public event EventHandler<StartRunspaceDebugProcessingEventArgs> StartRunspaceDebugProcessing;
        /// Event raised when a runspace debugger is finished being processed.
        public event EventHandler<ProcessRunspaceDebugEndEventArgs> RunspaceDebugProcessingCompleted;
        /// Event raised to indicate that the debugging session is over and runspace debuggers queued for
        /// processing should be released.
        public event EventHandler<EventArgs> CancelRunspaceDebugProcessing;
        /// True when the debugger is stopped.
        protected bool DebuggerStopped
        /// IsPushed.
        internal virtual bool IsPushed
        /// IsRemote.
        internal virtual bool IsRemote
        /// Returns true if the debugger is preserving a DebuggerStopEvent
        /// event.  Use ReleaseSavedDebugStop() to allow event to process.
        internal virtual bool IsPendingDebugStopEvent
            get { throw new PSNotImplementedException(); }
        /// Returns true if debugger has been set to stepInto mode.
        internal virtual bool IsDebuggerSteppingEnabled
        /// Returns true if there is a handler for debugger stops.
        internal bool IsDebugHandlerSubscribed
            get { return (DebuggerStop != null); }
        /// UnhandledBreakpointMode.
        internal virtual UnhandledBreakpointProcessingMode UnhandledBreakpointMode
            set { throw new PSNotImplementedException(); }
        /// DebuggerMode.
        public DebugModes DebugMode { get; protected set; } = DebugModes.Default;
        /// Returns true if debugger has breakpoints set and
        /// is currently active.
        public virtual bool IsActive
        /// InstanceId.
        public virtual Guid InstanceId
            get { return s_instanceId; }
        /// True when debugger is stopped at a breakpoint.
        public virtual bool InBreakpoint
            get { return DebuggerStopped; }
        /// RaiseDebuggerStopEvent.
        /// <param name="args">DebuggerStopEventArgs.</param>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        protected void RaiseDebuggerStopEvent(DebuggerStopEventArgs args)
                DebuggerStopped = true;
                DebuggerStop.SafeInvoke<DebuggerStopEventArgs>(this, args);
                DebuggerStopped = false;
        /// IsDebuggerStopEventSubscribed.
        /// <returns>True if event subscription exists.</returns>
        protected bool IsDebuggerStopEventSubscribed()
            return (DebuggerStop != null);
        /// RaiseBreakpointUpdatedEvent.
        /// <param name="args">BreakpointUpdatedEventArgs.</param>
        protected void RaiseBreakpointUpdatedEvent(BreakpointUpdatedEventArgs args)
            BreakpointUpdated.SafeInvoke<BreakpointUpdatedEventArgs>(this, args);
        /// IsDebuggerBreakpointUpdatedEventSubscribed.
        protected bool IsDebuggerBreakpointUpdatedEventSubscribed()
            return (BreakpointUpdated != null);
        protected void RaiseStartRunspaceDebugProcessingEvent(StartRunspaceDebugProcessingEventArgs args)
            if (args == null) { throw new PSArgumentNullException(nameof(args)); }
            StartRunspaceDebugProcessing.SafeInvoke<StartRunspaceDebugProcessingEventArgs>(this, args);
        protected void RaiseRunspaceProcessingCompletedEvent(ProcessRunspaceDebugEndEventArgs args)
            RunspaceDebugProcessingCompleted.SafeInvoke<ProcessRunspaceDebugEndEventArgs>(this, args);
        protected bool IsStartRunspaceDebugProcessingEventSubscribed()
            return (StartRunspaceDebugProcessing != null);
        protected void RaiseCancelRunspaceDebugProcessingEvent()
            CancelRunspaceDebugProcessing.SafeInvoke<EventArgs>(this, null);
        /// Evaluates provided command either as a debugger specific command
        /// or a PowerShell command.
        /// <param name="command">PowerShell command.</param>
        /// <param name="output">Output.</param>
        /// <returns>DebuggerCommandResults.</returns>
        public abstract DebuggerCommandResults ProcessCommand(PSCommand command, PSDataCollection<PSObject> output);
        /// Sets the debugger resume action.
        /// <param name="resumeAction">DebuggerResumeAction.</param>
        public abstract void SetDebuggerAction(DebuggerResumeAction resumeAction);
        /// Stops a running command.
        public abstract void StopProcessCommand();
        /// Returns current debugger stop event arguments if debugger is in
        /// debug stop state.  Otherwise returns null.
        /// <returns>DebuggerStopEventArgs.</returns>
        public abstract DebuggerStopEventArgs GetDebuggerStopArgs();
        /// Sets the parent debugger, breakpoints and other debugging context information.
        /// <param name="parent">Parent debugger.</param>
        /// <param name="breakPoints">List of breakpoints.</param>
        /// <param name="startAction">Debugger mode.</param>
        /// <param name="host">Host.</param>
        /// <param name="path">Current path.</param>
        public virtual void SetParent(
            Debugger parent,
            IEnumerable<Breakpoint> breakPoints,
            DebuggerResumeAction? startAction,
            PathInfo path)
            throw new PSNotImplementedException();
        /// Sets the debugger mode.
        public virtual void SetDebugMode(DebugModes mode)
            this.DebugMode = mode;
        /// Returns IEnumerable of CallStackFrame objects.
        public virtual IEnumerable<CallStackFrame> GetCallStack()
            return new Collection<CallStackFrame>();
        /// Get a breakpoint by id in the current runspace, primarily for Enable/Disable/Remove-PSBreakpoint cmdlets.
        /// <param name="id">Id of the breakpoint you want.</param>
        public Breakpoint GetBreakpoint(int id) =>
            GetBreakpoint(id, runspaceId: null);
        /// Get a breakpoint by id, primarily for Enable/Disable/Remove-PSBreakpoint cmdlets.
        /// <param name="runspaceId">The runspace id of the runspace you want to interact with. A null value will use the current runspace.</param>
        public virtual Breakpoint GetBreakpoint(int id, int? runspaceId) =>
        /// Adds the provided set of breakpoints to the debugger, in the current runspace.
        /// <param name="breakpoints">Breakpoints.</param>
        public void SetBreakpoints(IEnumerable<Breakpoint> breakpoints) =>
            SetBreakpoints(breakpoints, runspaceId: null);
        /// Adds the provided set of breakpoints to the debugger.
        /// <param name="runspaceId">The runspace id of the runspace you want to interact with, null being the current runspace.</param>
        public virtual void SetBreakpoints(IEnumerable<Breakpoint> breakpoints, int? runspaceId) =>
        /// Returns breakpoints in the current runspace, primarily for the Get-PSBreakpoint cmdlet.
        public List<Breakpoint> GetBreakpoints() =>
            GetBreakpoints(runspaceId: null);
        /// Returns breakpoints primarily for the Get-PSBreakpoint cmdlet.
        public virtual List<Breakpoint> GetBreakpoints(int? runspaceId) =>
        /// Sets a command breakpoint in the current runspace in the debugger.
        /// <param name="command">The name of the command that will trigger the breakpoint. This value may not be null.</param>
        /// <param name="action">The action to take when the breakpoint is hit. If null, PowerShell will break into the debugger when the breakpoint is hit.</param>
        /// <param name="path">The path to the script file where the breakpoint may be hit. If null, the breakpoint may be hit anywhere the command is invoked.</param>
        /// <returns>The command breakpoint that was set.</returns>
        public CommandBreakpoint SetCommandBreakpoint(string command, ScriptBlock action, string path) =>
            SetCommandBreakpoint(command, action, path, runspaceId: null);
        /// Sets a command breakpoint in the debugger.
        /// <param name="runspaceId">The runspace id of the runspace you want to interact with. A value of null will use the current runspace.</param>
        public virtual CommandBreakpoint SetCommandBreakpoint(string command, ScriptBlock action, string path, int? runspaceId) =>
        /// Sets a line breakpoint in the current runspace in the debugger.
        /// <param name="path">The path to the script file where the breakpoint may be hit. This value may not be null.</param>
        /// <param name="line">The line in the script file where the breakpoint may be hit. This value must be greater than or equal to 1.</param>
        /// <param name="column">The column in the script file where the breakpoint may be hit. If 0, the breakpoint will trigger on any statement on the line.</param>
        /// <returns>The line breakpoint that was set.</returns>
        public LineBreakpoint SetLineBreakpoint(string path, int line, int column, ScriptBlock action) =>
            SetLineBreakpoint(path, line, column, action, runspaceId: null);
        /// Sets a line breakpoint in the debugger.
        public virtual LineBreakpoint SetLineBreakpoint(string path, int line, int column, ScriptBlock action, int? runspaceId) =>
        /// Sets a variable breakpoint in the current runspace in the debugger.
        /// <param name="variableName">The name of the variable that will trigger the breakpoint. This value may not be null.</param>
        /// <param name="accessMode">The variable access mode that will trigger the breakpoint.</param>
        /// <param name="path">The path to the script file where the breakpoint may be hit. If null, the breakpoint may be hit anywhere the variable is accessed using the specified access mode.</param>
        /// <returns>The variable breakpoint that was set.</returns>
        public VariableBreakpoint SetVariableBreakpoint(string variableName, VariableAccessMode accessMode, ScriptBlock action, string path) =>
            SetVariableBreakpoint(variableName, accessMode, action, path, runspaceId: null);
        /// Sets a variable breakpoint in the debugger.
        public virtual VariableBreakpoint SetVariableBreakpoint(string variableName, VariableAccessMode accessMode, ScriptBlock action, string path, int? runspaceId) =>
        /// Removes a breakpoint from the debugger in the current runspace.
        /// <param name="breakpoint">The breakpoint to remove from the debugger. This value may not be null.</param>
        /// <returns>True if the breakpoint was removed from the debugger; false otherwise.</returns>
        public bool RemoveBreakpoint(Breakpoint breakpoint) =>
            RemoveBreakpoint(breakpoint, runspaceId: null);
        /// Removes a breakpoint from the debugger.
        public virtual bool RemoveBreakpoint(Breakpoint breakpoint, int? runspaceId) =>
        /// Enables a breakpoint in the debugger in the current runspace.
        /// <param name="breakpoint">The breakpoint to enable in the debugger. This value may not be null.</param>
        /// <returns>The updated breakpoint if it was found; null if the breakpoint was not found in the debugger.</returns>
        public Breakpoint EnableBreakpoint(Breakpoint breakpoint) =>
            EnableBreakpoint(breakpoint, runspaceId: null);
        /// Enables a breakpoint in the debugger.
        public virtual Breakpoint EnableBreakpoint(Breakpoint breakpoint, int? runspaceId) =>
        /// Disables a breakpoint in the debugger in the current runspace.
        public Breakpoint DisableBreakpoint(Breakpoint breakpoint) =>
            DisableBreakpoint(breakpoint, runspaceId: null);
        /// Disables a breakpoint in the debugger.
        public virtual Breakpoint DisableBreakpoint(Breakpoint breakpoint, int? runspaceId) =>
        /// Resets the command processor source information so that it is
        /// updated with latest information on the next debug stop.
        public virtual void ResetCommandProcessorSource()
        /// Sets debugger stepping mode.
        /// <param name="enabled">True if stepping is to be enabled.</param>
        public virtual void SetDebuggerStepMode(bool enabled)
        /// Breaks into the debugger.
        /// <param name="triggerObject">The object that triggered the breakpoint, if there is one.</param>
        internal virtual void Break(object triggerObject = null)
        /// Returns script position message of current execution stack item.
        /// This is used for WDAC audit mode logging for script information enhancement.
        /// <returns>Script position message string.</returns>
        internal virtual string GetCurrentScriptPosition()
        /// Passes the debugger command to the internal script debugger command processor.  This
        /// is used internally to handle debugger commands such as list, help, etc.
        /// <param name="command">Command string.</param>
        /// <param name="output">Output collection.</param>
        /// <returns>DebuggerCommand containing information on whether and how the command was processed.</returns>
        internal virtual DebuggerCommand InternalProcessCommand(string command, IList<PSObject> output)
        /// Creates a source list based on root script debugger source information if available, with
        /// the current source line highlighted.  This is used internally for nested runspace debugging
        /// where the runspace command is run in context of a parent script.
        /// <param name="lineNum">Current source line.</param>
        /// <returns>True if source listed successfully.</returns>
        internal virtual bool InternalProcessListCommand(int lineNum, IList<PSObject> output)
        /// Sets up debugger to debug provided job or its child jobs.
        /// <param name="job">
        /// Job object that is either a debuggable job or a container of
        /// debuggable child jobs.
        /// <param name="breakAll">
        /// If true, the debugger automatically invokes a break all when it
        /// attaches to the job.
        internal virtual void DebugJob(Job job, bool breakAll) =>
        /// Removes job from debugger job list and pops the its
        /// debugger from the active debugger stack.
        /// <param name="job">Job.</param>
        internal virtual void StopDebugJob(Job job)
        /// GetActiveDebuggerCallStack.
        /// <returns>Array of stack frame objects of active debugger.</returns>
        internal virtual CallStackFrame[] GetActiveDebuggerCallStack()
        /// Method to add the provided runspace information to the debugger
        /// for monitoring of debugger events.  This is used to implement nested
        /// debugging of runspaces.
        /// <param name="args">PSEntityCreatedRunspaceEventArgs.</param>
        internal virtual void StartMonitoringRunspace(PSMonitorRunspaceInfo args)
        /// Method to end the monitoring of a runspace for debugging events.
        internal virtual void EndMonitoringRunspace(PSMonitorRunspaceInfo args)
        /// If a debug stop event is currently pending then this method will release
        /// the event to continue processing.
        internal virtual void ReleaseSavedDebugStop()
        /// Sets up debugger to debug provided Runspace in a nested debug session.
        /// <param name="runspace">
        /// The runspace to debug.
        /// attaches to the runspace.
        internal virtual void DebugRunspace(Runspace runspace, bool breakAll) =>
        /// Removes the provided Runspace from the nested "active" debugger state.
        internal virtual void StopDebugRunspace(Runspace runspace)
        /// Raises the NestedDebuggingCancelledEvent event.
        internal void RaiseNestedDebuggingCancelEvent()
            // Raise event on worker thread.
            Threading.ThreadPool.QueueUserWorkItem(
                (state) =>
                        NestedDebuggingCancelledEvent.SafeInvoke<EventArgs>(this, null);
        #region Runspace Debug Processing Methods
        /// Adds the provided Runspace object to the runspace debugger processing queue.
        /// The queue will then raise the StartRunspaceDebugProcessing events for each runspace to allow
        /// a host script debugger implementation to provide an active debugging session.
        /// <param name="runspace">Runspace to debug.</param>
        internal virtual void QueueRunspaceForDebug(Runspace runspace)
        /// Causes the CancelRunspaceDebugProcessing event to be raised which notifies subscribers that current debugging
        /// sessions should be cancelled.
        public virtual void CancelDebuggerProcessing()
        internal const string CannotProcessCommandNotStopped = "Debugger:CannotProcessCommandNotStopped";
        internal const string CannotEnableDebuggerSteppingInvalidMode = "Debugger:CannotEnableDebuggerSteppingInvalidMode";
        private static readonly Guid s_instanceId = new Guid();
    #region ScriptDebugger class
    /// Holds the debugging information for a Monad Shell session.
    internal sealed class ScriptDebugger : Debugger, IDisposable
        internal ScriptDebugger(ExecutionContext context)
            _inBreakpoint = false;
            _idToBreakpoint = new ConcurrentDictionary<int, Breakpoint>();
            // The string key is function context file path. The int key is sequencePoint index.
            _pendingBreakpoints = new ConcurrentDictionary<string, ConcurrentDictionary<int, LineBreakpoint>>(StringComparer.OrdinalIgnoreCase);
            _boundBreakpoints = new ConcurrentDictionary<string, Tuple<WeakReference, ConcurrentDictionary<int, LineBreakpoint>>>(StringComparer.OrdinalIgnoreCase);
            _commandBreakpoints = new ConcurrentDictionary<int, CommandBreakpoint>();
            _variableBreakpoints = new ConcurrentDictionary<string, ConcurrentDictionary<int, VariableBreakpoint>>(StringComparer.OrdinalIgnoreCase);
            _steppingMode = SteppingMode.None;
            _callStack = new CallStackList { _callStackList = new List<CallStackInfo>() };
            _runningJobs = new Dictionary<Guid, PSJobStartEventArgs>();
            _activeDebuggers = new ConcurrentStack<Debugger>();
            _debuggerStopEventArgs = new ConcurrentStack<DebuggerStopEventArgs>();
            _syncObject = new object();
            _syncActiveDebuggerStopObject = new object();
            _runningRunspaces = new Dictionary<Guid, PSMonitorRunspaceInfo>();
        static ScriptDebugger()
            s_processDebugPromptMatch = StringUtil.Format(@"""[{0}:", DebuggerStrings.NestedRunspaceDebuggerPromptProcessName);
        public override bool InBreakpoint
                if (_inBreakpoint)
                    return _inBreakpoint;
                Debugger activeDebugger;
                if (_activeDebuggers.TryPeek(out activeDebugger))
                    return activeDebugger.InBreakpoint;
        internal override bool IsPushed
            get { return (!_activeDebuggers.IsEmpty); }
        internal override bool IsPendingDebugStopEvent
                return ((_preserveDebugStopEvent != null) && !_preserveDebugStopEvent.IsSet);
        internal override bool IsDebuggerSteppingEnabled
                return ((_context._debuggingMode == (int)InternalDebugMode.Enabled) &&
                        (_currentDebuggerAction == DebuggerResumeAction.StepInto) &&
                        (_steppingMode != SteppingMode.None));
        private bool? _isLocalSession;
        private bool IsLocalSession
                // Remote debug sessions always have a ServerRemoteHost.  Otherwise it is a local session.
                _isLocalSession ??= !((_context.InternalHost.ExternalHost != null) &&
                    (_context.InternalHost.ExternalHost is System.Management.Automation.Remoting.ServerRemoteHost));
                return _isLocalSession.Value;
        /// Gets or sets the object that triggered the current breakpoint.
        private object TriggerObject { get; set; }
        #region Reset Debugger
        /// Resets debugger to initial state.
        internal void ResetDebugger()
            SetDebugMode(DebugModes.None);
            SetInternalDebugMode(InternalDebugMode.Disabled);
            _idToBreakpoint.Clear();
            _pendingBreakpoints.Clear();
            _boundBreakpoints.Clear();
            _commandBreakpoints.Clear();
            _variableBreakpoints.Clear();
            s_emptyBreakpointList.Clear();
            _callStack.Clear();
            _overOrOutFrame = null;
            _commandProcessor = new DebuggerCommandProcessor();
            _currentInvocationInfo = null;
            _psDebuggerCommand = null;
            _savedIgnoreScriptDebug = false;
            _isLocalSession = null;
            _nestedDebuggerStop = false;
            _debuggerStopEventArgs.Clear();
            _lastActiveDebuggerAction = DebuggerResumeAction.Continue;
            _currentDebuggerAction = DebuggerResumeAction.Continue;
            _previousDebuggerAction = DebuggerResumeAction.Continue;
            _nestedRunningFrame = null;
            _processingOutputCount = 0;
            _preserveUnhandledDebugStopEvent = false;
            ClearRunningJobList();
            ClearRunningRunspaceList();
            _activeDebuggers.Clear();
            ReleaseSavedDebugStop();
            SetDebugMode(DebugModes.Default);
        #region Call stack management
        // Called from generated code on entering the script function, called once for each dynamicparam, begin, or end
        // block, and once for each object written to the pipeline.  Also called when entering a trap.
        internal void EnterScriptFunction(FunctionContext functionContext)
            Diagnostics.Assert(functionContext._executionContext == _context, "Wrong debugger is being used.");
            var invocationInfo = (InvocationInfo)functionContext._localsTuple.GetAutomaticVariable(AutomaticVariable.MyInvocation);
            var newCallStackInfo = new CallStackInfo
                InvocationInfo = invocationInfo,
                File = functionContext._file,
                DebuggerStepThrough = functionContext._debuggerStepThrough,
                FunctionContext = functionContext,
                IsFrameHidden = functionContext._debuggerHidden,
            _callStack.Add(newCallStackInfo);
            if (_context._debuggingMode > 0)
                var scriptCommandInfo = invocationInfo.MyCommand as ExternalScriptInfo;
                    RegisterScriptFile(scriptCommandInfo);
                bool checkLineBp = CheckCommand(invocationInfo);
                SetupBreakpoints(functionContext);
                if (functionContext._debuggerStepThrough && _overOrOutFrame == null && _steppingMode == SteppingMode.StepIn)
                    // Treat like step out, but only if we're not already stepping out
                    ResumeExecution(DebuggerResumeAction.StepOut);
                if (checkLineBp)
                    OnSequencePointHit(functionContext);
                if (_context.PSDebugTraceLevel > 1 && !functionContext._debuggerStepThrough && !functionContext._debuggerHidden)
                    TraceScriptFunctionEntry(functionContext);
        private void SetupBreakpoints(FunctionContext functionContext)
            var scriptDebugData = _mapScriptToBreakpoints.GetValue(functionContext._sequencePoints,
                                                                   _ => Tuple.Create(new Dictionary<int, List<LineBreakpoint>>(),
                                                                                     new BitArray(functionContext._sequencePoints.Length)));
            functionContext._boundBreakpoints = scriptDebugData.Item1;
            functionContext._breakPoints = scriptDebugData.Item2;
            SetPendingBreakpoints(functionContext);
        // Called after exiting the script function, called once for each dynamicparam, begin, or end
        // block, and once for each object written to the pipeline.  Also called when leaving a trap.
        internal void ExitScriptFunction()
            // If it's stepping over to exit the current frame, we need to clear the _overOrOutFrame,
            // so that we will stop at the next statement in the outer frame.
            if (_callStack.Last() == _overOrOutFrame)
            _callStack.RemoveAt(_callStack.Count - 1);
            // Don't disable step mode if the user enabled runspace debugging (UnhandledBreakpointMode == Wait)
            if ((_callStack.Count == 0) && (UnhandledBreakpointMode != UnhandledBreakpointProcessingMode.Wait))
                // If we've popped the last entry, don't step into anything else (like prompt, suggestions, etc.)
        internal void RegisterScriptFile(ExternalScriptInfo scriptCommandInfo)
            RegisterScriptFile(scriptCommandInfo.Path, scriptCommandInfo.ScriptContents);
        internal void RegisterScriptFile(string path, string scriptContents)
            Tuple<WeakReference, ConcurrentDictionary<int, LineBreakpoint>> boundBreakpoints;
            if (!_boundBreakpoints.TryGetValue(path, out boundBreakpoints))
                _boundBreakpoints[path] = Tuple.Create(new WeakReference(scriptContents), new ConcurrentDictionary<int, LineBreakpoint>());
                // If script contents have changed, or if the file got collected, we must rebind the breakpoints.
                string oldScriptContents;
                boundBreakpoints.Item1.TryGetTarget(out oldScriptContents);
                if (oldScriptContents == null || !oldScriptContents.Equals(scriptContents, StringComparison.Ordinal))
                    UnbindBoundBreakpoints(boundBreakpoints.Item2.Values.ToList());
        #endregion Call stack management
        #region setting breakpoints
        internal void AddBreakpointCommon(Breakpoint breakpoint)
            if (_context._debuggingMode == 0)
                SetInternalDebugMode(InternalDebugMode.Enabled);
            _idToBreakpoint[breakpoint.Id] = breakpoint;
            OnBreakpointUpdated(new BreakpointUpdatedEventArgs(breakpoint, BreakpointUpdateType.Set, _idToBreakpoint.Count));
        private CommandBreakpoint AddCommandBreakpoint(CommandBreakpoint breakpoint)
            AddBreakpointCommon(breakpoint);
            _commandBreakpoints[breakpoint.Id] = breakpoint;
            return breakpoint;
        private LineBreakpoint AddLineBreakpoint(LineBreakpoint breakpoint)
            AddPendingBreakpoint(breakpoint);
        private void AddPendingBreakpoint(LineBreakpoint breakpoint)
            _pendingBreakpoints.AddOrUpdate(
                breakpoint.Script,
                new ConcurrentDictionary<int, LineBreakpoint> { [breakpoint.Id] = breakpoint },
                (_, dictionary) => { dictionary.TryAdd(breakpoint.Id, breakpoint); return dictionary; });
        private void AddNewBreakpoint(Breakpoint breakpoint)
            LineBreakpoint lineBreakpoint = breakpoint as LineBreakpoint;
            if (lineBreakpoint != null)
                AddLineBreakpoint(lineBreakpoint);
            CommandBreakpoint cmdBreakpoint = breakpoint as CommandBreakpoint;
            if (cmdBreakpoint != null)
                AddCommandBreakpoint(cmdBreakpoint);
            VariableBreakpoint varBreakpoint = breakpoint as VariableBreakpoint;
            if (varBreakpoint != null)
                AddVariableBreakpoint(varBreakpoint);
        internal VariableBreakpoint AddVariableBreakpoint(VariableBreakpoint breakpoint)
            if (!_variableBreakpoints.TryGetValue(breakpoint.Variable, out ConcurrentDictionary<int, VariableBreakpoint> breakpoints))
                breakpoints = new ConcurrentDictionary<int, VariableBreakpoint>();
                _variableBreakpoints[breakpoint.Variable] = breakpoints;
            breakpoints[breakpoint.Id] = breakpoint;
        private void UpdateBreakpoints(FunctionContext functionContext)
            if (functionContext._breakPoints == null)
                // This should be rare - setting a breakpoint inside a script, but debugger hadn't started.
                // Check pending breakpoints to see if any apply to this script.
                if (string.IsNullOrEmpty(functionContext._file))
                if (_pendingBreakpoints.TryGetValue(functionContext._file, out var dictionary) && !dictionary.IsEmpty)
        /// Raises the BreakpointUpdated event.
        private void OnBreakpointUpdated(BreakpointUpdatedEventArgs e)
            RaiseBreakpointUpdatedEvent(e);
        #endregion setting breakpoints
        #region removing breakpoints
        internal bool RemoveVariableBreakpoint(VariableBreakpoint breakpoint) =>
            _variableBreakpoints[breakpoint.Variable].Remove(breakpoint.Id, out _);
        internal bool RemoveCommandBreakpoint(CommandBreakpoint breakpoint) =>
            _commandBreakpoints.Remove(breakpoint.Id, out _);
        internal bool RemoveLineBreakpoint(LineBreakpoint breakpoint)
            if (_pendingBreakpoints.TryGetValue(breakpoint.Script, out var dictionary))
                removed = dictionary.Remove(breakpoint.Id, out _);
            Tuple<WeakReference, ConcurrentDictionary<int, LineBreakpoint>> value;
            if (_boundBreakpoints.TryGetValue(breakpoint.Script, out value))
                removed = value.Item2.Remove(breakpoint.Id, out _);
            return removed;
        #endregion removing breakpoints
        #region finding breakpoints
        // The bit array is used to detect if a breakpoint is set or not for a given scriptblock.  This bit array
        // is checked when hitting sequence points.  Enabling/disabling a line breakpoint is as simple as flipping
        // the bit.
        private readonly ConditionalWeakTable<IScriptExtent[], Tuple<Dictionary<int, List<LineBreakpoint>>, BitArray>> _mapScriptToBreakpoints =
            new ConditionalWeakTable<IScriptExtent[], Tuple<Dictionary<int, List<LineBreakpoint>>, BitArray>>();
        /// Checks for command breakpoints.
        internal bool CheckCommand(InvocationInfo invocationInfo)
            var functionContext = _callStack.LastFunctionContext();
            if (functionContext != null && functionContext._debuggerHidden)
                // Never stop in DebuggerHidden scripts, don't even call the actions on breakpoints.
            List<Breakpoint> breakpoints =
                _commandBreakpoints.Values.Where(bp => bp.Enabled && bp.Trigger(invocationInfo)).ToList<Breakpoint>();
            bool checkLineBp = true;
            if (breakpoints.Count > 0)
                breakpoints = TriggerBreakpoints(breakpoints);
                    var breakInvocationInfo =
                        functionContext != null
                            ? new InvocationInfo(invocationInfo.MyCommand, functionContext.CurrentPosition)
                    OnDebuggerStop(breakInvocationInfo, breakpoints);
                    checkLineBp = false;
            return checkLineBp;
        internal void CheckVariableRead(string variableName)
            var breakpointsToTrigger = GetVariableBreakpointsToTrigger(variableName, read: true);
            if (breakpointsToTrigger != null && breakpointsToTrigger.Count > 0)
                TriggerVariableBreakpoints(breakpointsToTrigger);
        internal void CheckVariableWrite(string variableName)
            var breakpointsToTrigger = GetVariableBreakpointsToTrigger(variableName, read: false);
        private List<VariableBreakpoint> GetVariableBreakpointsToTrigger(string variableName, bool read)
            Diagnostics.Assert(_context._debuggingMode == 1, "breakpoints only hit when debugging mode is 1");
                // Never stop in DebuggerHidden scripts, don't even call the action on any breakpoint.
                ConcurrentDictionary<int, VariableBreakpoint> breakpoints;
                if (!_variableBreakpoints.TryGetValue(variableName, out breakpoints))
                    // $PSItem is an alias for $_.  We don't use PSItem internally, but a user might
                    // have set a bp on $PSItem, so look for that if appropriate.
                    if (SpecialVariables.IsUnderbar(variableName))
                        _variableBreakpoints.TryGetValue(SpecialVariables.PSItem, out breakpoints);
                if (breakpoints == null)
                var callStackInfo = _callStack.Last();
                var currentScriptFile = callStackInfo?.File;
                return breakpoints.Values.Where(bp => bp.Trigger(currentScriptFile, read: read)).ToList();
        internal void TriggerVariableBreakpoints(List<VariableBreakpoint> breakpoints)
            var invocationInfo = functionContext != null ? new InvocationInfo(null, functionContext.CurrentPosition, _context) : null;
            OnDebuggerStop(invocationInfo, breakpoints.ToList<Breakpoint>());
        // Return the line breakpoints bound in a specific script block (used when a sequence point
        // is hit, to find which breakpoints are set on that sequence point.)
        internal Dictionary<int, List<LineBreakpoint>> GetBoundBreakpoints(IScriptExtent[] sequencePoints)
            Tuple<Dictionary<int, List<LineBreakpoint>>, BitArray> tuple;
            if (_mapScriptToBreakpoints.TryGetValue(sequencePoints, out tuple))
        #endregion finding breakpoints
        #region triggering breakpoints
        private List<Breakpoint> TriggerBreakpoints(List<Breakpoint> breakpoints)
            Diagnostics.Assert(_context._debuggingMode == 1, "breakpoints only hit when debugging mode == 1");
            List<Breakpoint> breaks = new List<Breakpoint>();
                SetInternalDebugMode(InternalDebugMode.InScriptStop);
                foreach (Breakpoint breakpoint in breakpoints)
                    if (breakpoint.Enabled)
                            // Ensure that code being processed during breakpoint triggers
                            // act like they are broken into the debugger.
                            _inBreakpoint = true;
                            if (breakpoint.Trigger() == Breakpoint.BreakpointAction.Break)
                                breaks.Add(breakpoint);
            return breaks;
        internal void OnSequencePointHit(FunctionContext functionContext)
            // TraceLine uses ColumnNumber and expects it to be 1 based. For
            // extents added by the engine and not user code the value can be
            // set to 0 causing an exception. This skips those types of extents
            // as tracing them wouldn't be useful for the end user anyway.
            if (_context.ShouldTraceStatement &&
                !_callStack.Last().IsFrameHidden &&
                !functionContext._debuggerStepThrough &&
                functionContext.CurrentPosition is not EmptyScriptExtent &&
                (functionContext.CurrentPosition is InternalScriptExtent ||
                   functionContext.CurrentPosition.StartColumnNumber > 0))
                TraceLine(functionContext.CurrentPosition);
            // If a nested debugger received a stop debug command then all debugging
            // should stop.
            if (_nestedDebuggerStop)
                ResumeExecution(DebuggerResumeAction.Stop);
            UpdateBreakpoints(functionContext);
            if (_steppingMode == SteppingMode.StepIn &&
                (_overOrOutFrame == null || _callStack.Last() == _overOrOutFrame))
                if (!_callStack.Last().IsFrameHidden)
                    StopOnSequencePoint(functionContext, s_emptyBreakpointList);
                else if (_overOrOutFrame == null)
                if (functionContext._breakPoints[functionContext._currentSequencePointIndex])
                    if (functionContext._boundBreakpoints.TryGetValue(functionContext._currentSequencePointIndex, out var sequencePointBreakpoints))
                        var enabledBreakpoints = new List<Breakpoint>();
                        foreach (Breakpoint breakpoint in sequencePointBreakpoints)
                                enabledBreakpoints.Add(breakpoint);
                        if (enabledBreakpoints.Count > 0)
                            enabledBreakpoints = TriggerBreakpoints(enabledBreakpoints);
                                StopOnSequencePoint(functionContext, enabledBreakpoints);
        #endregion triggering breakpoints
        [DebuggerDisplay("{FunctionContext.CurrentPosition}")]
        private sealed class CallStackInfo
            internal InvocationInfo InvocationInfo { get; set; }
            internal string File { get; set; }
            internal bool DebuggerStepThrough { get; set; }
            internal FunctionContext FunctionContext { get; set; }
            /// The frame is hidden due to the <see cref="DebuggerHiddenAttribute"/> attribute.
            /// No breakpoints will be set and no stepping in/through.
            internal bool IsFrameHidden { get; set; }
            internal bool TopFrameAtBreakpoint { get; set; }
        private struct CallStackList
            internal List<CallStackInfo> _callStackList;
            internal void Add(CallStackInfo item)
                lock (_callStackList)
                    _callStackList.Add(item);
            internal void RemoveAt(int index)
                    _callStackList.RemoveAt(index);
            internal CallStackInfo this[int index]
                        return ((index > -1) && (index < _callStackList.Count)) ? _callStackList[index] : null;
            internal CallStackInfo Last()
                    return (_callStackList.Count > 0) ? _callStackList[_callStackList.Count - 1] : null;
            internal FunctionContext LastFunctionContext()
                var last = Last();
                return last?.FunctionContext;
            internal bool Any()
                    return _callStackList.Count > 0;
                        return _callStackList.Count;
            internal CallStackInfo[] ToArray()
                    return _callStackList.ToArray();
                    _callStackList.Clear();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, LineBreakpoint>> _pendingBreakpoints;
        private readonly ConcurrentDictionary<string, Tuple<WeakReference, ConcurrentDictionary<int, LineBreakpoint>>> _boundBreakpoints;
        private readonly ConcurrentDictionary<int, CommandBreakpoint> _commandBreakpoints;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, VariableBreakpoint>> _variableBreakpoints;
        private readonly ConcurrentDictionary<int, Breakpoint> _idToBreakpoint;
        private SteppingMode _steppingMode;
        private CallStackInfo _overOrOutFrame;
        private CallStackList _callStack;
        private static readonly List<Breakpoint> s_emptyBreakpointList = new List<Breakpoint>();
        private DebuggerCommandProcessor _commandProcessor = new DebuggerCommandProcessor();
        private InvocationInfo _currentInvocationInfo;
        private bool _inBreakpoint;
        private PowerShell _psDebuggerCommand;
        // Job debugger integration.
        private bool _nestedDebuggerStop;
        private readonly Dictionary<Guid, PSJobStartEventArgs> _runningJobs;
        private readonly ConcurrentStack<Debugger> _activeDebuggers;
        private readonly ConcurrentStack<DebuggerStopEventArgs> _debuggerStopEventArgs;
        private DebuggerResumeAction _lastActiveDebuggerAction;
        private DebuggerResumeAction _currentDebuggerAction;
        private DebuggerResumeAction _previousDebuggerAction;
        private CallStackInfo _nestedRunningFrame;
        private readonly object _syncObject;
        private readonly object _syncActiveDebuggerStopObject;
        private int _processingOutputCount;
        private ManualResetEventSlim _processingOutputCompleteEvent = new ManualResetEventSlim(true);
        // Runspace debugger integration.
        private readonly Dictionary<Guid, PSMonitorRunspaceInfo> _runningRunspaces;
        private const int _jobCallStackOffset = 2;
        private const int _runspaceCallStackOffset = 1;
        private bool _preserveUnhandledDebugStopEvent;
        private ManualResetEventSlim _preserveDebugStopEvent;
        // Process runspace debugger
        private readonly Lazy<ConcurrentQueue<StartRunspaceDebugProcessingEventArgs>> _runspaceDebugQueue = new Lazy<ConcurrentQueue<StartRunspaceDebugProcessingEventArgs>>();
        private volatile int _processingRunspaceDebugQueue;
        private ManualResetEventSlim _runspaceDebugCompleteEvent;
        // System is locked down when true. Used to disable debugger on lock down.
        private bool? _isSystemLockedDown;
        private static readonly string s_processDebugPromptMatch;
        /// Raises the DebuggerStop event.
        private void OnDebuggerStop(InvocationInfo invocationInfo, List<Breakpoint> breakpoints)
            Diagnostics.Assert(breakpoints != null, "The list of breakpoints should not be null");
            LocalRunspace localRunspace = _context.CurrentRunspace as LocalRunspace;
            Diagnostics.Assert(localRunspace != null, "Debugging is only supported on local runspaces");
            if (localRunspace.PulsePipeline != null && localRunspace.PulsePipeline == localRunspace.GetCurrentlyRunningPipeline())
                _context.EngineHostInterface.UI.WriteWarningLine(
                    breakpoints.Count > 0
                        ? string.Format(CultureInfo.CurrentCulture, DebuggerStrings.WarningBreakpointWillNotBeHit,
                                        breakpoints[0])
                        : new InvalidOperationException().Message);
            _currentInvocationInfo = invocationInfo;
            // Optionally wait for a debug stop event subscriber if requested.
            if (!WaitForDebugStopSubscriber())
                // No subscriber.  Ignore this debug stop event.
            bool oldQuestionMarkVariableValue = _context.QuestionMarkVariableValue;
            _context.SetVariable(SpecialVariables.PSDebugContextVarPath, new PSDebugContext(invocationInfo, breakpoints, TriggerObject));
            FunctionInfo defaultPromptInfo = null;
            string originalPromptString = null;
            bool hadDefaultPrompt = false;
                Collection<PSObject> items = _context.SessionState.InvokeProvider.Item.Get("function:\\prompt");
                if ((items != null) && (items.Count > 0))
                    defaultPromptInfo = items[0].BaseObject as FunctionInfo;
                    originalPromptString = defaultPromptInfo.Definition as string;
                    if (originalPromptString.Equals(InitialSessionState.DefaultPromptFunctionText, StringComparison.OrdinalIgnoreCase) ||
                        originalPromptString.Trim().StartsWith(s_processDebugPromptMatch, StringComparison.OrdinalIgnoreCase))
                        hadDefaultPrompt = true;
                // Ignore, it means they don't have the default prompt
            // Change the context language mode before updating the prompt script.
            // This way the new prompt scriptblock will pick up the current context language mode.
            PSLanguageMode? originalLanguageMode = null;
            if (_context.UseFullLanguageModeInDebugger &&
                (_context.LanguageMode != PSLanguageMode.FullLanguage))
                originalLanguageMode = _context.LanguageMode;
                _context.LanguageMode = PSLanguageMode.FullLanguage;
            // Update the prompt to the debug prompt
            if (hadDefaultPrompt)
                int index = originalPromptString.IndexOf('"', StringComparison.OrdinalIgnoreCase);
                    // Fix up prompt.
                    string debugPrompt = string.Concat("\"[DBG]: ", originalPromptString.AsSpan(index, originalPromptString.Length - index));
                    defaultPromptInfo.Update(
                        ScriptBlock.Create(debugPrompt), true, ScopedItemOptions.Unspecified);
                    hadDefaultPrompt = false;
            RunspaceAvailability previousAvailability = _context.CurrentRunspace.RunspaceAvailability;
            _context.CurrentRunspace.UpdateRunspaceAvailability(
                _context.CurrentRunspace.GetCurrentlyRunningPipeline() != null
                    ? RunspaceAvailability.AvailableForNestedCommand
                    : RunspaceAvailability.Available,
            Diagnostics.Assert(_context._debuggingMode == 1, "Should only be stopping when debugger is on.");
                if (_callStack.Any())
                    // Get-PSCallStack shouldn't report any frames above this frame, so mark it.  One alternative
                    // to marking the frame would be to not push new frames while debugging, but that limits our
                    // ability to give a full callstack if there are errors during eval.
                    _callStack.Last().TopFrameAtBreakpoint = true;
                // Reset list lines.
                _commandProcessor.Reset();
                // Save a copy of the stop arguments.
                DebuggerStopEventArgs copyArgs = new DebuggerStopEventArgs(invocationInfo, breakpoints);
                _debuggerStopEventArgs.Push(copyArgs);
                // Blocking call to raise debugger stop event.
                DebuggerStopEventArgs e = new DebuggerStopEventArgs(invocationInfo, breakpoints);
                RaiseDebuggerStopEvent(e);
                ResumeExecution(e.ResumeAction);
                    _callStack.Last().TopFrameAtBreakpoint = false;
                _context.CurrentRunspace.UpdateRunspaceAvailability(previousAvailability, true);
                if (originalLanguageMode.HasValue)
                    _context.LanguageMode = originalLanguageMode.Value;
                _context.EngineSessionState.RemoveVariable(SpecialVariables.PSDebugContext);
                    // Restore the prompt if they had our default
                        ScriptBlock.Create(originalPromptString), true, ScopedItemOptions.Unspecified);
                DebuggerStopEventArgs oldArgs;
                _debuggerStopEventArgs.TryPop(out oldArgs);
                _context.QuestionMarkVariableValue = oldQuestionMarkVariableValue;
        /// Resumes execution after a breakpoint/step event has been handled.
        private void ResumeExecution(DebuggerResumeAction action)
            _previousDebuggerAction = _currentDebuggerAction;
            _currentDebuggerAction = action;
                case DebuggerResumeAction.StepInto:
                    _steppingMode = SteppingMode.StepIn;
                case DebuggerResumeAction.StepOut:
                    if (_callStack.Count > 1)
                        // When we pop to the parent frame, we'll clear _overOrOutFrame (so OnSequencePointHit
                        // will stop) and continue with a step.
                        _overOrOutFrame = _callStack[_callStack.Count - 2];
                        // Stepping out of the top frame is just like continue (allow hitting
                        // breakpoints in the current frame, but otherwise just go.)
                        goto case DebuggerResumeAction.Continue;
                case DebuggerResumeAction.StepOver:
                    _overOrOutFrame = _callStack.Last();
                case DebuggerResumeAction.Continue:
                    // nothing to do, just continue
                case DebuggerResumeAction.Stop:
                    throw new TerminateException();
                    Debug.Fail("Received an unknown action: " + action);
        /// Blocking call that blocks until a release occurs via ReleaseSavedDebugStop().
        /// <returns>True if there is a DebuggerStop event subscriber.</returns>
        private bool WaitForDebugStopSubscriber()
            if (!IsDebuggerStopEventSubscribed())
                if (_preserveUnhandledDebugStopEvent)
                    // Lazily create the event object.
                    _preserveDebugStopEvent ??= new ManualResetEventSlim(true);
                    // Set the event handle to non-signaled.
                    if (!_preserveDebugStopEvent.IsSet)
                        Diagnostics.Assert(false, "The _preserveDebugStop event handle should always be in the signaled state at this point.");
                    _preserveDebugStopEvent.Reset();
                    // Wait indefinitely for a signal event.
                    _preserveDebugStopEvent.Wait();
                    return IsDebuggerStopEventSubscribed();
        private enum SteppingMode
            StepIn,
        // When a script file changes, we need to rebind all breakpoints in that script.
        private void UnbindBoundBreakpoints(List<LineBreakpoint> boundBreakpoints)
            foreach (var breakpoint in boundBreakpoints)
                // Also remove unbound breakpoints from the script to breakpoint map.
                Tuple<Dictionary<int, List<LineBreakpoint>>, BitArray> lineBreakTuple;
                if (_mapScriptToBreakpoints.TryGetValue(breakpoint.SequencePoints, out lineBreakTuple))
                    if (lineBreakTuple.Item1.TryGetValue(breakpoint.SequencePointIndex, out var lineBreakList))
                        lineBreakList.Remove(breakpoint);
                breakpoint.SequencePoints = null;
                breakpoint.SequencePointIndex = -1;
                breakpoint.BreakpointBitArray = null;
            boundBreakpoints.Clear();
        private void SetPendingBreakpoints(FunctionContext functionContext)
            var currentScriptFile = functionContext._file;
            // If we're not in a file, we can't have any line breakpoints.
            if (currentScriptFile == null)
            if (!_pendingBreakpoints.TryGetValue(currentScriptFile, out var breakpoints) || breakpoints.IsEmpty)
            // Normally we register a script file when the script is run or the module is imported,
            // but if there weren't any breakpoints when the script was run and the script was dotted,
            // we will end up here with pending breakpoints, but we won't have cached the list of
            // breakpoints in the script.
            RegisterScriptFile(currentScriptFile, functionContext.CurrentPosition.StartScriptPosition.GetFullScript());
            if (!_mapScriptToBreakpoints.TryGetValue(functionContext._sequencePoints, out tuple))
                Diagnostics.Assert(false, "If the script block is still alive, the entry should not be collected.");
            Diagnostics.Assert(tuple.Item1 == functionContext._boundBreakpoints, "What's up?");
            foreach ((int breakpointId, LineBreakpoint breakpoint) in breakpoints)
                bool bound = false;
                if (breakpoint.TrySetBreakpoint(currentScriptFile, functionContext))
                    bound = true;
                    if (tuple.Item1.TryGetValue(breakpoint.SequencePointIndex, out var list))
                        list.Add(breakpoint);
                        tuple.Item1.Add(breakpoint.SequencePointIndex, new List<LineBreakpoint> { breakpoint });
                    // We need to keep track of any breakpoints that are bound in each script because they may
                    // need to be rebound if the script changes.
                    var boundBreakpoints = _boundBreakpoints[currentScriptFile].Item2;
                    boundBreakpoints[breakpoint.Id] = breakpoint;
                if (bound)
                    breakpoints.TryRemove(breakpointId, out _);
            // Here could check if all breakpoints for the current functionContext were bound, but because there is no atomic
            // api for conditional removal we either need to lock, or do some trickery that has possibility of race conditions.
            // Instead we keep the item in the dictionary with 0 breakpoint count. This should not be a big issue,
            // because it is single entry per file that had breakpoints, so there won't be thousands of files in a session.
        private void StopOnSequencePoint(FunctionContext functionContext, List<Breakpoint> breakpoints)
            if (functionContext._debuggerHidden)
                // Never stop in a DebuggerHidden scriptblock.
            if (functionContext._sequencePoints.Length == 1 &&
                functionContext._scriptBlock != null &&
                object.ReferenceEquals(functionContext._sequencePoints[0], functionContext._scriptBlock.Ast.Extent))
                // If the script is empty or only defines functions, we used the script block extent as a sequence point, but that
                // was only intended for error reporting, it was not meant to be hit as a breakpoint.
            var invocationInfo = new InvocationInfo(null, functionContext.CurrentPosition, _context);
            OnDebuggerStop(invocationInfo, breakpoints);
        private enum InternalDebugMode
            InPushedStop = -2,
            InScriptStop = -1,
            Enabled = 1
        /// Sets the internal Execution context debug mode given the
        /// current DebugMode setting.
        /// <param name="mode">Internal debug mode.</param>
        private void SetInternalDebugMode(InternalDebugMode mode)
                // Disable script debugger when in system lock down mode
                if (IsSystemLockedDown)
                    if (_context._debuggingMode != (int)InternalDebugMode.Disabled)
                        _context._debuggingMode = (int)InternalDebugMode.Disabled;
                    case InternalDebugMode.InPushedStop:
                    case InternalDebugMode.InScriptStop:
                    case InternalDebugMode.Disabled:
                        _context._debuggingMode = (int)mode;
                    case InternalDebugMode.Enabled:
                        _context._debuggingMode = CanEnableDebugger ?
                            (int)InternalDebugMode.Enabled : (int)InternalDebugMode.Disabled;
        private bool CanEnableDebugger
                // The debugger can be enabled if we are not in DebugMode.None and if we are
                // not in a local session set only to RemoteScript.
                return !((DebugMode == DebugModes.RemoteScript) && IsLocalSession) && (DebugMode != DebugModes.None);
        private bool CanDisableDebugger
                // The debugger can be disabled if there are no breakpoints
                // left and if we are not currently stepping in the debugger.
                return _idToBreakpoint.IsEmpty &&
                       _currentDebuggerAction != DebuggerResumeAction.StepInto &&
                       _currentDebuggerAction != DebuggerResumeAction.StepOver &&
                       _currentDebuggerAction != DebuggerResumeAction.StepOut;
        private bool IsSystemLockedDown
                if (_isSystemLockedDown == null)
                        _isSystemLockedDown ??= (System.Management.Automation.Security.SystemPolicy.GetSystemLockdownPolicy() ==
                            System.Management.Automation.Security.SystemEnforcementMode.Enforce);
                return _isSystemLockedDown.Value;
        private void CheckForBreakpointSupport()
                // Local script debugging is not supported in locked down mode
        #region Enable debug stepping
        private enum EnableNestedType
            NestedJob = 0x1,
            NestedRunspace = 0x2
        private void EnableDebuggerStepping(EnableNestedType nestedType)
            if (DebugMode == DebugModes.None)
                    DebuggerStrings.CannotEnableDebuggerSteppingInvalidMode,
                    Debugger.CannotEnableDebuggerSteppingInvalidMode,
                    // Set active debugger to StepInto mode.
                    activeDebugger.SetDebugMode(DebugModes.LocalScript | DebugModes.RemoteScript);
                    activeDebugger.SetDebuggerStepMode(true);
                    // Set script debugger to StepInto mode.
                    ResumeExecution(DebuggerResumeAction.StepInto);
            // Look for any runspaces with debuggers and set to setp mode.
            if ((nestedType & EnableNestedType.NestedRunspace) == EnableNestedType.NestedRunspace)
                SetRunspaceListToStep(true);
        /// Restores debugger back to non-step mode.
        private void DisableDebuggerStepping()
            if (!IsDebuggerSteppingEnabled) { return; }
                ResumeExecution(DebuggerResumeAction.Continue);
                RestoreInternalDebugMode();
                    activeDebugger.SetDebuggerStepMode(false);
            SetRunningJobListToStep(false);
            SetRunspaceListToStep(false);
        private void RestoreInternalDebugMode()
            InternalDebugMode restoreMode = ((DebugMode != DebugModes.None) && (!_idToBreakpoint.IsEmpty)) ? InternalDebugMode.Enabled : InternalDebugMode.Disabled;
            SetInternalDebugMode(restoreMode);
        #region Debugger Overrides
        /// Set ScriptDebugger action.
        public override void SetDebuggerAction(DebuggerResumeAction resumeAction)
            throw new PSNotSupportedException(
                StringUtil.Format(DebuggerStrings.CannotSetDebuggerAction));
        /// GetDebuggerStopped.
        public override DebuggerStopEventArgs GetDebuggerStopArgs()
            DebuggerStopEventArgs rtnArgs;
            if (_debuggerStopEventArgs.TryPeek(out rtnArgs))
                return rtnArgs;
        /// ProcessCommand.
        public override DebuggerCommandResults ProcessCommand(PSCommand command, PSDataCollection<PSObject> output)
                throw new PSArgumentNullException(nameof(command));
            if (output == null)
                throw new PSArgumentNullException(nameof(output));
            if (!DebuggerStopped)
                    DebuggerStrings.CannotProcessDebuggerCommandNotStopped,
                    Debugger.CannotProcessCommandNotStopped,
            // Allow an active pushed debugger to process commands
            DebuggerCommandResults results = ProcessCommandForActiveDebugger(command, output);
            // Otherwise let root script debugger handle it.
            if (_context.CurrentRunspace is not LocalRunspace localRunspace)
                using (_psDebuggerCommand = PowerShell.Create())
                    if (localRunspace.GetCurrentlyRunningPipeline() != null)
                        _psDebuggerCommand.SetIsNested(true);
                    _psDebuggerCommand.Runspace = localRunspace;
                    _psDebuggerCommand.Commands = command;
                    foreach (var cmd in _psDebuggerCommand.Commands.Commands)
                        cmd.MergeMyResults(PipelineResultTypes.All, PipelineResultTypes.Output);
                    PSDataCollection<PSObject> internalOutput = new PSDataCollection<PSObject>();
                    internalOutput.DataAdded += (sender, args) =>
                            foreach (var item in internalOutput.ReadAll())
                                if (item == null) { continue; }
                                DebuggerCommand dbgCmd = item.BaseObject as DebuggerCommand;
                                if (dbgCmd != null)
                                    bool executedByDebugger = (dbgCmd.ResumeAction != null || dbgCmd.ExecutedByDebugger);
                                    results = new DebuggerCommandResults(dbgCmd.ResumeAction, executedByDebugger);
                                    output.Add(item);
                    // Allow any exceptions to propagate.
                    _psDebuggerCommand.InvokeWithDebugger(null, internalOutput, null, false);
            return results ?? new DebuggerCommandResults(null, false);
        /// StopProcessCommand.
        public override void StopProcessCommand()
            // If we have a pushed debugger then stop that command.
            if (StopCommandForActiveDebugger())
            PowerShell ps = _psDebuggerCommand;
            ps?.BeginStop(null, null);
        /// Set debug mode.
        /// <param name="mode"></param>
        public override void SetDebugMode(DebugModes mode)
                // Restrict local script debugger mode when in system lock down.
                // DebugModes enum flags provide a combination of values.  To disable local script debugging
                // we have to disallow 'LocalScript' and 'Default' flags and only allow 'None' or 'RemoteScript'
                // flags exclusively.  This allows only no debugging 'None' or remote debugging 'RemoteScript'.
                if (IsSystemLockedDown && (mode != DebugModes.None) && (mode != DebugModes.RemoteScript))
                    mode = DebugModes.RemoteScript;
                base.SetDebugMode(mode);
                if (!CanEnableDebugger)
                else if ((!_idToBreakpoint.IsEmpty) && (_context._debuggingMode == 0))
                    // Set internal debugger to active.
        /// Returns current call stack.
        /// <returns>IEnumerable of CallStackFrame objects.</returns>
        public override IEnumerable<CallStackFrame> GetCallStack()
            CallStackInfo[] callStack = _callStack.ToArray();
            if (callStack.Length > 0)
                int startingIndex = callStack.Length - 1;
                for (int i = startingIndex; i >= 0; i--)
                    if (callStack[i].TopFrameAtBreakpoint)
                        startingIndex = i;
                    var funcContext = callStack[i].FunctionContext;
                    yield return new CallStackFrame(funcContext, callStack[i].InvocationInfo);
        /// True when debugger is active with breakpoints.
        public override bool IsActive
                int debuggerState = _context._debuggingMode;
                return (debuggerState != 0);
        public override void ResetCommandProcessorSource()
        public override void SetDebuggerStepMode(bool enabled)
            if (enabled)
                EnableDebuggerStepping(EnableNestedType.NestedJob | EnableNestedType.NestedRunspace);
                DisableDebuggerStepping();
        internal override void Break(object triggerObject = null)
            if (!IsDebugHandlerSubscribed &&
                (UnhandledBreakpointMode == UnhandledBreakpointProcessingMode.Ignore))
                // No debugger attached and runspace debugging is not enabled.  Enable runspace debugging here
                // so that this command is effective.
                UnhandledBreakpointMode = UnhandledBreakpointProcessingMode.Wait;
            // Store the triggerObject so that we can add it to PSDebugContext
            TriggerObject = triggerObject;
            // Set debugger to step mode so that a break can occur.
            SetDebuggerStepMode(true);
            // If the debugger is enabled and we are not in a breakpoint, trigger an immediate break in the current location
                using (IEnumerator<CallStackFrame> enumerator = GetCallStack().GetEnumerator())
                        OnSequencePointHit(enumerator.Current.FunctionContext);
        internal override string GetCurrentScriptPosition()
                    var functionContext = enumerator.Current.FunctionContext;
                    if (functionContext is not null)
                        var invocationInfo = new InvocationInfo(commandInfo: null, functionContext.CurrentPosition, _context);
                        return $"\n{invocationInfo.PositionMessage}";
        internal override DebuggerCommand InternalProcessCommand(string command, IList<PSObject> output)
                return new DebuggerCommand(command, null, false, false);
            // "Exit" command should always result with "Continue" behavior for legacy compatibility.
            if (command.Equals("exit", StringComparison.OrdinalIgnoreCase))
                return new DebuggerCommand(command, DebuggerResumeAction.Continue, false, true);
            return _commandProcessor.ProcessCommand(null, command, _currentInvocationInfo, output);
        internal override bool InternalProcessListCommand(int lineNum, IList<PSObject> output)
            if (!DebuggerStopped || (_currentInvocationInfo == null)) { return false; }
            // Create an Invocation object that has full source script from script debugger plus
            // line information provided from caller.
            string fullScript = _currentInvocationInfo.GetFullScript();
            ScriptPosition startScriptPosition = new ScriptPosition(
                _currentInvocationInfo.ScriptName,
                lineNum,
                _currentInvocationInfo.ScriptPosition.StartScriptPosition.Offset,
                _currentInvocationInfo.Line,
                fullScript);
            ScriptPosition endScriptPosition = new ScriptPosition(
            InvocationInfo tempInvocationInfo = InvocationInfo.Create(
                _currentInvocationInfo.MyCommand,
                new ScriptExtent(
                    startScriptPosition,
                    endScriptPosition)
            _commandProcessor.ProcessListCommand(tempInvocationInfo, output);
        internal override bool IsRemote
                    return (activeDebugger is RemotingJobDebugger);
        /// Array of stack frame objects of active debugger if any,
        /// otherwise null.
        /// <returns>CallStackFrame[].</returns>
        internal override CallStackFrame[] GetActiveDebuggerCallStack()
                return activeDebugger.GetCallStack().ToArray();
        /// Sets how the debugger deals with breakpoint events that are not handled.
        ///  Ignore - This is the default behavior and ignores any breakpoint event
        ///           if there is no handler.  Releases any preserved event.
        ///  Wait   - This mode preserves a breakpoint event until a handler is
        ///           subscribed.
        internal override UnhandledBreakpointProcessingMode UnhandledBreakpointMode
                return (_preserveUnhandledDebugStopEvent) ? UnhandledBreakpointProcessingMode.Wait : UnhandledBreakpointProcessingMode.Ignore;
                    case UnhandledBreakpointProcessingMode.Wait:
                        _preserveUnhandledDebugStopEvent = true;
                    case UnhandledBreakpointProcessingMode.Ignore:
        #region Breakpoints
        /// <param name="breakpoints">The breakpoints to set.</param>
        public override void SetBreakpoints(IEnumerable<Breakpoint> breakpoints, int? runspaceId)
            if (runspaceId.HasValue)
                GetRunspaceDebugger(runspaceId.Value).SetBreakpoints(breakpoints);
            foreach (Breakpoint bp in breakpoints)
                switch (bp)
                    case CommandBreakpoint commandBreakpoint:
                        AddCommandBreakpoint(commandBreakpoint);
                    case LineBreakpoint lineBreakpoint:
                    case VariableBreakpoint variableBreakpoint:
                        AddVariableBreakpoint(variableBreakpoint);
        public override Breakpoint GetBreakpoint(int id, int? runspaceId)
                return GetRunspaceDebugger(runspaceId.Value).GetBreakpoint(id);
            _idToBreakpoint.TryGetValue(id, out Breakpoint breakpoint);
        public override List<Breakpoint> GetBreakpoints(int? runspaceId)
                return GetRunspaceDebugger(runspaceId.Value).GetBreakpoints();
            return (from bp in _idToBreakpoint.Values orderby bp.Id select bp).ToList();
        public override CommandBreakpoint SetCommandBreakpoint(string command, ScriptBlock action, string path, int? runspaceId)
                return GetRunspaceDebugger(runspaceId.Value).SetCommandBreakpoint(command, action, path);
            Diagnostics.Assert(!string.IsNullOrEmpty(command), "Caller to verify command is not null or empty.");
            CheckForBreakpointSupport();
            return AddCommandBreakpoint(new CommandBreakpoint(path, pattern, command, action));
        /// <returns>A LineBreakpoint</returns>
        public override LineBreakpoint SetLineBreakpoint(string path, int line, int column, ScriptBlock action, int? runspaceId)
                return GetRunspaceDebugger(runspaceId.Value).SetLineBreakpoint(path, line, column, action);
            Diagnostics.Assert(path != null, "Caller to verify path is not null.");
            Diagnostics.Assert(line > 0, "Caller to verify line is greater than 0.");
            return AddLineBreakpoint(new LineBreakpoint(path, line, column, action));
        /// <returns>A VariableBreakpoint that was set.</returns>
        public override VariableBreakpoint SetVariableBreakpoint(string variableName, VariableAccessMode accessMode, ScriptBlock action, string path, int? runspaceId)
                return GetRunspaceDebugger(runspaceId.Value).SetVariableBreakpoint(variableName, accessMode, action, path);
            Diagnostics.Assert(!string.IsNullOrEmpty(variableName), "Caller to verify variableName is not null or empty.");
            return AddVariableBreakpoint(new VariableBreakpoint(path, variableName, accessMode, action));
        /// This is the implementation of the Remove-PSBreakpoint cmdlet.
        /// <param name="breakpoint">Id of the breakpoint you want.</param>
        public override bool RemoveBreakpoint(Breakpoint breakpoint, int? runspaceId)
                return GetRunspaceDebugger(runspaceId.Value).RemoveBreakpoint(breakpoint);
            Diagnostics.Assert(breakpoint != null, "Caller to verify the breakpoint is not null.");
            if (_idToBreakpoint.Remove(breakpoint.Id, out _))
                breakpoint.RemoveSelf(this);
                if (CanDisableDebugger)
                OnBreakpointUpdated(new BreakpointUpdatedEventArgs(breakpoint, BreakpointUpdateType.Removed, _idToBreakpoint.Count));
        /// This is the implementation of the Enable-PSBreakpoint cmdlet.
        public override Breakpoint EnableBreakpoint(Breakpoint breakpoint, int? runspaceId)
                return GetRunspaceDebugger(runspaceId.Value).EnableBreakpoint(breakpoint);
            if (_idToBreakpoint.TryGetValue(breakpoint.Id, out _))
                breakpoint.SetEnabled(true);
                OnBreakpointUpdated(new BreakpointUpdatedEventArgs(breakpoint, BreakpointUpdateType.Enabled, _idToBreakpoint.Count));
        /// This is the implementation of the Disable-PSBreakpoint cmdlet.
        public override Breakpoint DisableBreakpoint(Breakpoint breakpoint, int? runspaceId)
                return GetRunspaceDebugger(runspaceId.Value).DisableBreakpoint(breakpoint);
                breakpoint.SetEnabled(false);
                OnBreakpointUpdated(new BreakpointUpdatedEventArgs(breakpoint, BreakpointUpdateType.Disabled, _idToBreakpoint.Count));
        private static Debugger GetRunspaceDebugger(int runspaceId)
            if (!Runspace.RunspaceDictionary.TryGetValue(runspaceId, out WeakReference<Runspace> wr))
                throw new PSArgumentException(string.Format(DebuggerStrings.InvalidRunspaceId, runspaceId));
            if (!wr.TryGetTarget(out Runspace rs))
                throw new PSArgumentException(DebuggerStrings.UnableToGetRunspace);
            return rs.Debugger;
        #endregion Breakpoints
        #region Job Debugging
        internal override void DebugJob(Job job, bool breakAll)
            if (job == null) { throw new PSArgumentNullException(nameof(job)); }
                if (_context._debuggingMode < 0)
                    throw new RuntimeException(DebuggerStrings.CannotStartJobDebuggingDebuggerBusy);
            // If a debuggable job was passed in then add it to the
            // job running list.
            bool jobsAdded = TryAddDebugJob(job, breakAll);
            if (!jobsAdded)
                // Otherwise treat as parent Job and iterate over child jobs.
                foreach (Job childJob in job.ChildJobs)
                    if (TryAddDebugJob(childJob, breakAll) && !jobsAdded)
                        jobsAdded = true;
                throw new PSInvalidOperationException(DebuggerStrings.NoDebuggableJobsFound);
        private bool TryAddDebugJob(Job job, bool breakAll)
            IJobDebugger debuggableJob = job as IJobDebugger;
            if ((debuggableJob != null) && (debuggableJob.Debugger != null) &&
                ((job.JobStateInfo.State == JobState.Running) || (job.JobStateInfo.State == JobState.AtBreakpoint)))
                // Check to see if job is already stopped in debugger.
                bool jobDebugAlreadyStopped = debuggableJob.Debugger.InBreakpoint;
                // Add to running job list with debugger set to step into.
                SetDebugJobAsync(debuggableJob, false);
                AddToJobRunningList(
                    new PSJobStartEventArgs(job, debuggableJob.Debugger, false),
                    breakAll ? DebuggerResumeAction.StepInto : DebuggerResumeAction.Continue);
                // Raise debug stop event if job is already in stopped state.
                if (jobDebugAlreadyStopped)
                    RemotingJobDebugger remoteJobDebugger = debuggableJob.Debugger as RemotingJobDebugger;
                    if (remoteJobDebugger != null)
                        remoteJobDebugger.CheckStateAndRaiseStopEvent();
                        Diagnostics.Assert(false, "Should never get debugger stopped job that is not of RemotingJobDebugger type.");
        /// Removes job from debugger job list and pops its
        internal override void StopDebugJob(Job job)
            // Parameter validation.
            RemoveFromRunningJobList(job);
            SetDebugJobAsync(job as IJobDebugger, true);
            foreach (var cJob in job.ChildJobs)
                RemoveFromRunningJobList(cJob);
                SetDebugJobAsync(cJob as IJobDebugger, true);
        /// Helper method to set a IJobDebugger job CanDebug property.
        /// <param name="debuggableJob">IJobDebugger.</param>
        /// <param name="isAsync">Boolean.</param>
        internal static void SetDebugJobAsync(IJobDebugger debuggableJob, bool isAsync)
            if (debuggableJob != null)
                debuggableJob.IsAsync = isAsync;
        #region Runspace Debugging
        /// Runspace to debug.
        /// When true, this command will invoke a BreakAll when the debugger is
        /// first attached.
        internal override void DebugRunspace(Runspace runspace, bool breakAll)
                throw new PSArgumentNullException(nameof(runspace));
                    string.Format(CultureInfo.InvariantCulture, DebuggerStrings.RunspaceDebuggingInvalidRunspaceState, runspace.RunspaceStateInfo.State)
                    throw new PSInvalidOperationException(DebuggerStrings.RunspaceDebuggingDebuggerBusy);
            if (runspace.Debugger == null)
                    string.Format(CultureInfo.InvariantCulture, DebuggerStrings.RunspaceDebuggingNoRunspaceDebugger, runspace.Name));
            if (runspace.Debugger.DebugMode == DebugModes.None)
                throw new PSInvalidOperationException(DebuggerStrings.RunspaceDebuggingDebuggerIsOff);
            AddToRunningRunspaceList(new PSStandaloneMonitorRunspaceInfo(runspace));
            if (!runspace.Debugger.InBreakpoint && breakAll)
                EnableDebuggerStepping(EnableNestedType.NestedRunspace);
        internal override void StopDebugRunspace(Runspace runspace)
            RemoveFromRunningRunspaceList(runspace);
        internal override void QueueRunspaceForDebug(Runspace runspace)
            runspace.StateChanged += RunspaceStateChangedHandler;
            runspace.AvailabilityChanged += RunspaceAvailabilityChangedHandler;
            _runspaceDebugQueue.Value.Enqueue(new StartRunspaceDebugProcessingEventArgs(runspace));
            StartRunspaceForDebugQueueProcessing();
        /// Causes the CancelRunspaceDebugProcessing event to be raised which notifies subscribers that these debugging
        public override void CancelDebuggerProcessing()
            // Empty runspace debugger processing queue and then notify any subscribers.
            ReleaseInternalRunspaceDebugProcessing(null, true);
                RaiseCancelRunspaceDebugProcessingEvent();
        private void ReleaseInternalRunspaceDebugProcessing(object sender, bool emptyQueue = false)
            Runspace runspace = sender as Runspace;
                runspace.StateChanged -= RunspaceStateChangedHandler;
                runspace.AvailabilityChanged -= RunspaceAvailabilityChangedHandler;
            if (emptyQueue && _runspaceDebugQueue.IsValueCreated)
                StartRunspaceDebugProcessingEventArgs args;
                while (_runspaceDebugQueue.Value.TryDequeue(out args))
                    args.Runspace.StateChanged -= RunspaceStateChangedHandler;
                    args.Runspace.AvailabilityChanged -= RunspaceAvailabilityChangedHandler;
                        args.Runspace.Debugger.UnhandledBreakpointMode = UnhandledBreakpointProcessingMode.Ignore;
                    catch (Exception) { }
            if (_runspaceDebugCompleteEvent != null)
                    _runspaceDebugCompleteEvent.Set();
        private void RunspaceStateChangedHandler(object sender, RunspaceStateEventArgs args)
            switch (args.RunspaceStateInfo.State)
                    ReleaseInternalRunspaceDebugProcessing(sender);
        private void RunspaceAvailabilityChangedHandler(object sender, RunspaceAvailabilityEventArgs args)
            if (args.RunspaceAvailability == RunspaceAvailability.Available)
        #region Job debugger integration
        private void AddToJobRunningList(PSJobStartEventArgs jobArgs, DebuggerResumeAction startAction)
            bool newJob = false;
                jobArgs.Job.StateChanged += HandleJobStateChanged;
                if (jobArgs.Job.IsPersistentState(jobArgs.Job.JobStateInfo.State))
                    jobArgs.Job.StateChanged -= HandleJobStateChanged;
                if (!_runningJobs.ContainsKey(jobArgs.Job.InstanceId))
                    // For now ignore WF jobs started asynchronously from script.
                    if (jobArgs.IsAsync) { return; }
                    // Turn on output processing monitoring on workflow job so that
                    // the debug stop events can coordinate with end of output processing.
                    jobArgs.Job.OutputProcessingStateChanged += HandleOutputProcessingStateChanged;
                    jobArgs.Job.MonitorOutputProcessing = true;
                    _runningJobs.Add(jobArgs.Job.InstanceId, jobArgs);
                    jobArgs.Debugger.DebuggerStop += HandleMonitorRunningJobsDebuggerStop;
                    newJob = true;
            if (newJob)
                jobArgs.Debugger.SetParent(
                    _idToBreakpoint.Values.ToArray<Breakpoint>(),
                    startAction,
                    _context.EngineHostInterface.ExternalHost,
                    _context.SessionState.Path.CurrentLocation);
                // If job already in collection then make sure start action is set.
                // Note that this covers the case where Debug-Job was performed on
                // an async job, which then becomes sync, the user continues execution
                // and then wants to break (step mode) into the debugger *again*.
                jobArgs.Debugger.SetDebuggerStepMode(true);
        private void SetRunningJobListToStep(bool enableStepping)
            PSJobStartEventArgs[] runningJobs;
                runningJobs = _runningJobs.Values.ToArray();
            foreach (var item in runningJobs)
                    item.Debugger.SetDebuggerStepMode(enableStepping);
        private void SetRunspaceListToStep(bool enableStepping)
            PSMonitorRunspaceInfo[] runspaceList;
                runspaceList = _runningRunspaces.Values.ToArray();
            foreach (var item in runspaceList)
                    Debugger nestedDebugger = item.NestedDebugger;
                    nestedDebugger?.SetDebuggerStepMode(enableStepping);
        private void RemoveFromRunningJobList(Job job)
            job.StateChanged -= HandleJobStateChanged;
            job.OutputProcessingStateChanged -= HandleOutputProcessingStateChanged;
            PSJobStartEventArgs jobArgs = null;
                if (_runningJobs.TryGetValue(job.InstanceId, out jobArgs))
                    jobArgs.Debugger.DebuggerStop -= HandleMonitorRunningJobsDebuggerStop;
                    _runningJobs.Remove(job.InstanceId);
            if (jobArgs != null)
                // Pop from active debugger stack.
                lock (_syncActiveDebuggerStopObject)
                        if (activeDebugger.Equals(jobArgs.Debugger))
                            PopActiveDebugger();
        private void ClearRunningJobList()
            PSJobStartEventArgs[] runningJobs = null;
                if (_runningJobs.Count > 0)
                    runningJobs = new PSJobStartEventArgs[_runningJobs.Values.Count];
                    _runningJobs.Values.CopyTo(runningJobs, 0);
            if (runningJobs != null)
                    RemoveFromRunningJobList(item.Job);
        private bool PushActiveDebugger(Debugger debugger, int callstackOffset)
            // Don't push active debugger if script debugger disabled debugging.
            if (_context._debuggingMode == -1) { return false; }
            // Disable script debugging while another debugger is running.
            // -1 - Indicates script debugging is disabled from script debugger.
            // -2 - Indicates script debugging is disabled from pushed active debugger.
            SetInternalDebugMode(InternalDebugMode.InPushedStop);
            // Save running calling frame.
            _nestedRunningFrame = _callStack[_callStack.Count - callstackOffset];
            // Make active debugger.
            _activeDebuggers.Push(debugger);
        private Debugger PopActiveDebugger()
            Debugger poppedDebugger = null;
            if (_activeDebuggers.TryPop(out poppedDebugger))
                int runningJobCount;
                    runningJobCount = _runningJobs.Count;
                if (runningJobCount == 0)
                    // If we are back to the root debugger and are in step mode, ensure
                    // that the root debugger is in step mode to continue stepping.
                    switch (_lastActiveDebuggerAction)
                            // Set script debugger to step mode after the WF running
                            // script completes.
                            _overOrOutFrame = _nestedRunningFrame;
                            _nestedDebuggerStop = true;
                    // Allow script debugger to continue in debugging mode.
                    _currentDebuggerAction = _lastActiveDebuggerAction;
            return poppedDebugger;
        private void HandleActiveJobDebuggerStop(object sender, DebuggerStopEventArgs args)
            // If we are debugging nested runspaces then ignore job debugger stops
            if (_runningRunspaces.Count > 0) { return; }
            // Forward active debugger event.
                // Save copy of arguments.
                DebuggerStopEventArgs copyArgs = new DebuggerStopEventArgs(
                    args.InvocationInfo,
                    new Collection<Breakpoint>(args.Breakpoints),
                    args.ResumeAction);
                CallStackInfo savedCallStackItem = null;
                    // Wait for up to 5 seconds for output processing to complete.
                    _processingOutputCompleteEvent.Wait(5000);
                    // Fix up call stack representing this WF call.
                    savedCallStackItem = FixUpCallStack();
                    // Blocking call that raises stop event.
                    RaiseDebuggerStopEvent(args);
                    _lastActiveDebuggerAction = args.ResumeAction;
                    RestoreCallStack(savedCallStackItem);
                    _debuggerStopEventArgs.TryPop(out copyArgs);
        private CallStackInfo FixUpCallStack()
            // Remove the top level call stack item, which is
            // the PS script that starts the workflow.  The workflow
            // debugger will add its call stack in its GetCallStack()
            // override.
            int count = _callStack.Count;
            CallStackInfo item = null;
            if (count > 1)
                item = _callStack.Last();
                _callStack.RemoveAt(count - 1);
        private void RestoreCallStack(CallStackInfo item)
                _callStack.Add(item);
        private void HandleMonitorRunningJobsDebuggerStop(object sender, DebuggerStopEventArgs args)
            if (!IsJobDebuggingMode())
                // Ignore job debugger stop.
                args.ResumeAction = DebuggerResumeAction.Continue;
            Debugger senderDebugger = sender as Debugger;
            bool pushSucceeded = false;
                Debugger activeDebugger = null;
                    if (activeDebugger.Equals(senderDebugger))
                        HandleActiveJobDebuggerStop(sender, args);
                    if (IsRunningWFJobsDebugger(activeDebugger))
                        // Replace current job active debugger by first popping.
                pushSucceeded = PushActiveDebugger(senderDebugger, _jobCallStackOffset);
            // Handle debugger stop outside lock.
            if (pushSucceeded)
                // Forward the debug stop event.
        private bool IsJobDebuggingMode()
            return ((((DebugMode & DebugModes.LocalScript) == DebugModes.LocalScript) && IsLocalSession) ||
                    (((DebugMode & DebugModes.RemoteScript) == DebugModes.RemoteScript) && !IsLocalSession));
        private bool IsRunningWFJobsDebugger(Debugger debugger)
                foreach (var item in _runningJobs.Values)
                    if (item.Debugger.Equals(debugger))
        private void HandleJobStateChanged(object sender, JobStateEventArgs args)
            Job job = sender as Job;
            if (job.IsPersistentState(args.JobStateInfo.State))
        private void HandleOutputProcessingStateChanged(object sender, OutputProcessingStateEventArgs e)
                if (e.ProcessingOutput)
                    if (++_processingOutputCount == 1)
                        _processingOutputCompleteEvent.Reset();
                else if (_processingOutputCount > 0)
                    if (--_processingOutputCount == 0)
                        _processingOutputCompleteEvent.Set();
        private DebuggerCommandResults ProcessCommandForActiveDebugger(PSCommand command, PSDataCollection<PSObject> output)
            // Check for debugger "detach" command which is only applicable to nested debugging.
            bool detachCommand = ((command.Commands.Count > 0) &&
                                  ((command.Commands[0].CommandText.Equals("Detach", StringComparison.OrdinalIgnoreCase)) ||
                                   (command.Commands[0].CommandText.Equals("d", StringComparison.OrdinalIgnoreCase))));
                if (detachCommand)
                    // Exit command means to cancel the nested debugger session.  This needs to be done by the
                    // owner of the session so we raise an event and release the debugger stop.
                    UnhandledBreakpointMode = UnhandledBreakpointProcessingMode.Ignore;
                    RaiseNestedDebuggingCancelEvent();
                    return new DebuggerCommandResults(DebuggerResumeAction.Continue, true);
                else if ((command.Commands.Count > 0) &&
                         (command.Commands[0].CommandText.IndexOf(".EnterNestedPrompt()", StringComparison.OrdinalIgnoreCase) > 0))
                    // Prevent a host EnterNestedPrompt() call from occurring in an active debugger.
                    // Host nested prompt makes no sense in this case and can cause host to stop responding depending on host implementation.
                // Get current debugger stop breakpoint info.
                DebuggerStopEventArgs stopArgs;
                if (_debuggerStopEventArgs.TryPeek(out stopArgs))
                    string commandText = command.Commands[0].CommandText;
                    // Check to see if this is a resume command that we handle here.
                    DebuggerCommand dbgCommand = _commandProcessor.ProcessBasicCommand(commandText);
                    if (dbgCommand != null &&
                        dbgCommand.ResumeAction != null)
                        _lastActiveDebuggerAction = dbgCommand.ResumeAction.Value;
                        return new DebuggerCommandResults(dbgCommand.ResumeAction, true);
                return activeDebugger.ProcessCommand(command, output);
                // Detach command only applies to nested debugging.  So if there isn't any active debugger then emit error.
                throw new PSInvalidOperationException(DebuggerStrings.InvalidDetachCommand);
        private bool StopCommandForActiveDebugger()
                activeDebugger.StopProcessCommand();
        #region Runspace debugger integration
        internal override void StartMonitoringRunspace(PSMonitorRunspaceInfo runspaceInfo)
            if (runspaceInfo == null || runspaceInfo.Runspace == null) { return; }
            if ((runspaceInfo.Runspace.Debugger != null) &&
                runspaceInfo.Runspace.Debugger.Equals(this))
                Debug.Fail("Nested debugger cannot be the root debugger.");
            DebuggerResumeAction startAction = (_currentDebuggerAction == DebuggerResumeAction.StepInto) ?
                DebuggerResumeAction.StepInto : DebuggerResumeAction.Continue;
            AddToRunningRunspaceList(runspaceInfo.Copy());
        internal override void EndMonitoringRunspace(PSMonitorRunspaceInfo runspaceInfo)
            RemoveFromRunningRunspaceList(runspaceInfo.Runspace);
        internal override void ReleaseSavedDebugStop()
            if (IsPendingDebugStopEvent)
                _preserveDebugStopEvent.Set();
        private void AddToRunningRunspaceList(PSMonitorRunspaceInfo args)
            Runspace runspace = args.Runspace;
            runspace.StateChanged += HandleRunspaceStateChanged;
            RunspaceState rsState = runspace.RunspaceStateInfo.State;
            if (rsState == RunspaceState.Broken ||
                rsState == RunspaceState.Closed ||
                rsState == RunspaceState.Disconnected)
                runspace.StateChanged -= HandleRunspaceStateChanged;
                if (!_runningRunspaces.ContainsKey(runspace.InstanceId))
                    _runningRunspaces.Add(runspace.InstanceId, args);
            // It is possible for the debugger to be non-null at this point if a runspace
            // is being reused.
            SetUpDebuggerOnRunspace(runspace);
        private void RemoveFromRunningRunspaceList(Runspace runspace)
            // Remove from running list.
            PSMonitorRunspaceInfo runspaceInfo = null;
                if (_runningRunspaces.TryGetValue(runspace.InstanceId, out runspaceInfo))
                    _runningRunspaces.Remove(runspace.InstanceId);
            // Clean up nested debugger.
            NestedRunspaceDebugger nestedDebugger = runspaceInfo?.NestedDebugger;
            if (nestedDebugger != null)
                nestedDebugger.DebuggerStop -= HandleMonitorRunningRSDebuggerStop;
                nestedDebugger.Dispose();
                // If current active debugger, then pop.
                        if (activeDebugger.Equals(nestedDebugger))
        private void ClearRunningRunspaceList()
            PSMonitorRunspaceInfo[] runningRunspaces = null;
                if (_runningRunspaces.Count > 0)
                    runningRunspaces = new PSMonitorRunspaceInfo[_runningRunspaces.Count];
                    _runningRunspaces.Values.CopyTo(runningRunspaces, 0);
            if (runningRunspaces != null)
                foreach (var item in runningRunspaces)
                    RemoveFromRunningRunspaceList(item.Runspace);
        private void HandleRunspaceStateChanged(object sender, RunspaceStateEventArgs e)
            bool remove = false;
                // Detect transition to Opened state.
                    remove = !SetUpDebuggerOnRunspace(runspace);
                // Detect any transition to a finished runspace.
                    remove = true;
            if (remove)
        private void HandleMonitorRunningRSDebuggerStop(object sender, DebuggerStopEventArgs args)
            if (sender == null || args == null) { return; }
                    // Replace current runspace debugger by first popping the old debugger.
                    if (IsRunningRSDebugger(activeDebugger))
                // Get nested debugger runspace info.
                if (senderDebugger is not NestedRunspaceDebugger nestedDebugger) { return; }
                PSMonitorRunspaceType runspaceType = nestedDebugger.RunspaceType;
                // Fix up invocation info script extents for embedded nested debuggers where the script source is
                // from the parent.
                args.InvocationInfo = nestedDebugger.FixupInvocationInfo(args.InvocationInfo);
                // Finally push the runspace debugger.
                pushSucceeded = PushActiveDebugger(senderDebugger, _runspaceCallStackOffset);
                // This method will always pop the debugger after debugger stop completes.
                HandleActiveRunspaceDebuggerStop(sender, args);
        private void HandleActiveRunspaceDebuggerStop(object sender, DebuggerStopEventArgs args)
                // Blocking call that raises the stop event.
                // Catch all external user generated exceptions thrown on event thread.
        private bool IsRunningRSDebugger(Debugger debugger)
                foreach (var item in _runningRunspaces.Values)
                    if (item.Runspace.Debugger.Equals(debugger))
        private bool SetUpDebuggerOnRunspace(Runspace runspace)
                _runningRunspaces.TryGetValue(runspace.InstanceId, out runspaceInfo);
            // Create nested debugger wrapper if it is not already created and if
            // the runspace debugger is available.
            if ((runspace.Debugger != null) &&
                (runspaceInfo != null) &&
                (runspaceInfo.NestedDebugger == null))
                    NestedRunspaceDebugger nestedDebugger = runspaceInfo.CreateDebugger(this);
                    runspaceInfo.NestedDebugger = nestedDebugger;
                    nestedDebugger.DebuggerStop += HandleMonitorRunningRSDebuggerStop;
                    if (((_lastActiveDebuggerAction == DebuggerResumeAction.StepInto) || (_currentDebuggerAction == DebuggerResumeAction.StepInto)) &&
                        !nestedDebugger.IsActive)
                        nestedDebugger.SetDebuggerStepMode(true);
                    // If the nested debugger has a pending (saved) debug stop then
                    // release it here now that we have the debug stop handler added.
                    // Note that the DebuggerStop event is raised on the original execution
                    // thread in the debugger (not this thread).
                    nestedDebugger.CheckStateAndRaiseStopEvent();
                catch (InvalidRunspaceStateException) { }
        private void StartRunspaceForDebugQueueProcessing()
            int startThread = Interlocked.CompareExchange(ref _processingRunspaceDebugQueue, 1, 0);
            if (startThread == 0)
                var thread = new System.Threading.Thread(
                    new ThreadStart(DebuggerQueueThreadProc));
        private void DebuggerQueueThreadProc()
            StartRunspaceDebugProcessingEventArgs runspaceDebugProcessArgs;
            while (_runspaceDebugQueue.Value.TryDequeue(out runspaceDebugProcessArgs))
                if (IsStartRunspaceDebugProcessingEventSubscribed())
                        RaiseStartRunspaceDebugProcessingEvent(runspaceDebugProcessArgs);
                    // If there are no ProcessDebugger event subscribers then default to handling internally.
                    runspaceDebugProcessArgs.UseDefaultProcessing = true;
                // Check for internal handling request.
                if (runspaceDebugProcessArgs.UseDefaultProcessing)
                        ProcessRunspaceDebugInternally(runspaceDebugProcessArgs.Runspace);
            Interlocked.CompareExchange(ref _processingRunspaceDebugQueue, 0, 1);
            if (!_runspaceDebugQueue.Value.IsEmpty)
        private void ProcessRunspaceDebugInternally(Runspace runspace)
            WaitForReadyDebug();
            DebugRunspace(runspace, breakAll: true);
            // Block this event thread until debugging has ended.
            WaitForDebugComplete();
            // Ensure runspace debugger is not stopped in break mode.
            if (runspace.Debugger.InBreakpoint)
                    runspace.Debugger.UnhandledBreakpointMode = UnhandledBreakpointProcessingMode.Ignore;
            StopDebugRunspace(runspace);
            // If we return to local script execution in step mode then ensure the debugger is enabled.
            if ((_steppingMode == SteppingMode.StepIn) && (_currentDebuggerAction != DebuggerResumeAction.Stop) && (_context._debuggingMode == 0))
            RaiseRunspaceProcessingCompletedEvent(
                new ProcessRunspaceDebugEndEventArgs(runspace));
        private void WaitForReadyDebug()
            // Wait up to ten seconds
            System.Threading.Thread.Sleep(500);
            bool debugReady = false;
                System.Threading.Thread.Sleep(250);
                debugReady = IsDebuggerReady();
            } while (!debugReady && (count++ < 40));
            if (!debugReady) { throw new PSInvalidOperationException(); }
        private bool IsDebuggerReady()
            return (!this.IsPushed && !this.InBreakpoint && (this._context._debuggingMode > -1) && (this._context.InternalHost.NestedPromptCount == 0));
        private void WaitForDebugComplete()
            if (_runspaceDebugCompleteEvent == null)
                _runspaceDebugCompleteEvent = new ManualResetEventSlim(false);
                _runspaceDebugCompleteEvent.Reset();
            _runspaceDebugCompleteEvent.Wait();
        /// Dispose.
            // Ensure all job event handlers are removed.
                Job job = item.Job;
                if (job != null)
            _processingOutputCompleteEvent.Dispose();
            _processingOutputCompleteEvent = null;
            if (_preserveDebugStopEvent != null)
                _preserveDebugStopEvent.Dispose();
                _preserveDebugStopEvent = null;
                _runspaceDebugCompleteEvent.Dispose();
                _runspaceDebugCompleteEvent = null;
        #region Tracing
        internal void EnableTracing(int traceLevel, bool? step)
            // Enable might actually be disabling depending on the arguments.
            if (traceLevel < 1 && (step == null || !(bool)step))
                DisableTracing();
            _savedIgnoreScriptDebug = _context.IgnoreScriptDebug;
            _context.IgnoreScriptDebug = false;
            _context.PSDebugTraceLevel = traceLevel;
            if (step != null)
                _context.PSDebugTraceStep = (bool)step;
        internal void DisableTracing()
            _context.IgnoreScriptDebug = _savedIgnoreScriptDebug;
            _context.PSDebugTraceLevel = 0;
            _context.PSDebugTraceStep = false;
        private bool _savedIgnoreScriptDebug = false;
        internal void Trace(string messageId, string resourceString, params object[] args)
            ActionPreference pref = ActionPreference.Continue;
                message = "Could not load text for msh script tracing message id '" + messageId + "'";
                Diagnostics.Assert(false, message);
            ((InternalHostUserInterface)_context.EngineHostInterface.UI).WriteDebugLine(message, ref pref);
        internal void TraceLine(IScriptExtent extent)
            string msg = PositionUtilities.BriefMessage(extent.StartScriptPosition);
            InternalHostUserInterface ui = (InternalHostUserInterface)_context.EngineHostInterface.UI;
            ActionPreference pref = _context.PSDebugTraceStep ?
                ActionPreference.Inquire : ActionPreference.Continue;
            ui.WriteDebugLine(msg, ref pref);
            if (pref == ActionPreference.Continue)
        internal void TraceScriptFunctionEntry(FunctionContext functionContext)
            var methodName = functionContext._functionName;
                Trace("TraceEnteringFunction", ParserStrings.TraceEnteringFunction, methodName);
                Trace("TraceEnteringFunctionDefinedInFile", ParserStrings.TraceEnteringFunctionDefinedInFile, methodName, functionContext._file);
        internal void TraceVariableSet(string varName, object value)
            // Don't trace into debugger hidden or debugger step through unless the trace level > 2.
            if (_callStack.Any() && _context.PSDebugTraceLevel <= 2)
                // Skip trace messages in hidden/step through frames.
                var frame = _callStack.Last();
                if (frame.IsFrameHidden || frame.DebuggerStepThrough)
            // If the value is an IEnumerator, we don't attempt to get its string format via 'ToStringParser' method,
            // because 'ToStringParser' would iterate through the enumerator to get the individual elements, which will
            // make irreversible changes to the enumerator.
            bool isValueAnIEnumerator = PSObject.Base(value) is IEnumerator;
            string valAsString = isValueAnIEnumerator ? nameof(IEnumerator) : PSObject.ToStringParser(_context, value);
            int msgLength = 60 - varName.Length;
            if (valAsString.Length > msgLength)
                valAsString = valAsString.Substring(0, msgLength) + PSObjectHelper.Ellipsis;
            Trace("TraceVariableAssignment", ParserStrings.TraceVariableAssignment, varName, valAsString);
        #endregion Tracing
    #region NestedRunspaceDebugger
    /// Base class for nested runspace debugger wrapper.
    internal abstract class NestedRunspaceDebugger : Debugger, IDisposable
        protected Runspace _runspace;
        protected Debugger _wrappedDebugger;
        /// Type of runspace being monitored for debugging.
        public PSMonitorRunspaceType RunspaceType { get; }
        /// Unique parent debugger identifier for monitored runspace.
        public Guid ParentDebuggerId
        /// Creates an instance of NestedRunspaceDebugger.
        /// <param name="runspaceType">Runspace type.</param>
        /// <param name="parentDebuggerId">Debugger Id of parent.</param>
        protected NestedRunspaceDebugger(
            Runspace runspace,
            PSMonitorRunspaceType runspaceType,
            Guid parentDebuggerId)
            if (runspace == null || runspace.Debugger == null)
            _runspace = runspace;
            _wrappedDebugger = runspace.Debugger;
            base.SetDebugMode(_wrappedDebugger.DebugMode);
            RunspaceType = runspaceType;
            ParentDebuggerId = parentDebuggerId;
            // Handlers for wrapped debugger events.
            _wrappedDebugger.BreakpointUpdated += HandleBreakpointUpdated;
            _wrappedDebugger.DebuggerStop += HandleDebuggerStop;
        public override void SetBreakpoints(IEnumerable<Breakpoint> breakpoints, int? runspaceId) =>
            _wrappedDebugger.SetBreakpoints(breakpoints, runspaceId);
        /// Process debugger or PowerShell command/script.
            if (_isDisposed) { return new DebuggerCommandResults(null, false); }
            // Preprocess debugger commands.
            string cmd = command.Commands[0].CommandText.Trim();
            if (cmd.Equals("prompt", StringComparison.OrdinalIgnoreCase))
                return HandlePromptCommand(output);
            if (cmd.Equals("k", StringComparison.OrdinalIgnoreCase) ||
                cmd.StartsWith("Get-PSCallStack", StringComparison.OrdinalIgnoreCase))
                return HandleCallStack(output);
            if (cmd.Equals("l", StringComparison.OrdinalIgnoreCase) ||
                cmd.Equals("list", StringComparison.OrdinalIgnoreCase))
                if (HandleListCommand(output))
                    return new DebuggerCommandResults(null, true);
            return _wrappedDebugger.ProcessCommand(command, output);
        /// Get a breakpoint by id.
        public override Breakpoint GetBreakpoint(int id, int? runspaceId) =>
            _wrappedDebugger.GetBreakpoint(id, runspaceId);
        /// Returns breakpoints on a runspace.
        /// <returns>A list of breakpoints in a runspace.</returns>
        public override List<Breakpoint> GetBreakpoints(int? runspaceId) =>
            _wrappedDebugger.GetBreakpoints(runspaceId);
        public override CommandBreakpoint SetCommandBreakpoint(string command, ScriptBlock action, string path, int? runspaceId) =>
            _wrappedDebugger.SetCommandBreakpoint(command, action, path, runspaceId);
        public override LineBreakpoint SetLineBreakpoint(string path, int line, int column, ScriptBlock action, int? runspaceId) =>
            _wrappedDebugger.SetLineBreakpoint(path, line, column, action, runspaceId);
        public override VariableBreakpoint SetVariableBreakpoint(string variableName, VariableAccessMode accessMode, ScriptBlock action, string path, int? runspaceId) =>
            _wrappedDebugger.SetVariableBreakpoint(variableName, accessMode, action, path, runspaceId);
        public override bool RemoveBreakpoint(Breakpoint breakpoint, int? runspaceId) =>
            _wrappedDebugger.RemoveBreakpoint(breakpoint, runspaceId);
        public override Breakpoint EnableBreakpoint(Breakpoint breakpoint, int? runspaceId) =>
            _wrappedDebugger.EnableBreakpoint(breakpoint, runspaceId);
        public override Breakpoint DisableBreakpoint(Breakpoint breakpoint, int? runspaceId) =>
            _wrappedDebugger.DisableBreakpoint(breakpoint, runspaceId);
        /// SetDebuggerAction.
        /// <param name="resumeAction">Debugger resume action.</param>
            _wrappedDebugger.SetDebuggerAction(resumeAction);
        /// Stops running command.
            _wrappedDebugger.StopProcessCommand();
            return _wrappedDebugger.GetDebuggerStopArgs();
        /// <param name="mode">Debug mode.</param>
            _wrappedDebugger.SetDebugMode(mode);
            _wrappedDebugger.SetDebuggerStepMode(enabled);
        /// Returns true if debugger is active.
            get { return _wrappedDebugger.IsActive; }
            _wrappedDebugger.Break(triggerObject);
        public virtual void Dispose()
            if (_wrappedDebugger != null)
                _wrappedDebugger.BreakpointUpdated -= HandleBreakpointUpdated;
                _wrappedDebugger.DebuggerStop -= HandleDebuggerStop;
            _wrappedDebugger = null;
            _runspace = null;
            // Call GC.SuppressFinalize since this is an unsealed type, in case derived types
            // have finalizers.
        protected virtual void HandleDebuggerStop(object sender, DebuggerStopEventArgs e)
            this.RaiseDebuggerStopEvent(e);
        protected virtual void HandleBreakpointUpdated(object sender, BreakpointUpdatedEventArgs e)
            this.RaiseBreakpointUpdatedEvent(e);
        protected virtual DebuggerCommandResults HandlePromptCommand(PSDataCollection<PSObject> output)
            // Nested debugged runspace prompt should look like:
            // [ComputerName]: [DBG]: [Process:<id>]: [RunspaceName]: PS C:\>
            string computerName = _runspace.ConnectionInfo?.ComputerName;
            const string processPartPattern = "{0}[{1}:{2}]:{3}";
            string processPart = StringUtil.Format(processPartPattern,
                @"""",
                DebuggerStrings.NestedRunspaceDebuggerPromptProcessName,
                @"$($PID)",
                @"""");
            const string locationPart = @"""PS $($executionContext.SessionState.Path.CurrentLocation)> """;
            string promptScript = "'[DBG]: '" + " + " + processPart + " + " + "' [" + CodeGeneration.EscapeSingleQuotedStringContent(_runspace.Name) + "]: '" + " + " + locationPart;
            // Get the command prompt from the wrapped debugger.
            PSCommand promptCommand = new PSCommand();
            promptCommand.AddScript(promptScript);
            PSDataCollection<PSObject> promptOutput = new PSDataCollection<PSObject>();
            _wrappedDebugger.ProcessCommand(promptCommand, promptOutput);
            string promptString = (promptOutput.Count == 1) ? promptOutput[0].BaseObject as string : string.Empty;
            var nestedPromptString = new System.Text.StringBuilder();
            // For remote runspaces display computer name in prompt.
            if (!string.IsNullOrEmpty(computerName))
                nestedPromptString.Append("[" + computerName + "]:");
            nestedPromptString.Append(promptString);
            // Fix up for non-remote runspaces since the runspace is not in a nested prompt
            // but the root runspace is.
            if (string.IsNullOrEmpty(computerName))
                nestedPromptString.Insert(nestedPromptString.Length - 1, ">");
            output.Add(nestedPromptString.ToString());
        protected virtual DebuggerCommandResults HandleCallStack(PSDataCollection<PSObject> output)
        protected virtual bool HandleListCommand(PSDataCollection<PSObject> output)
        /// Attempts to fix up the debugger stop invocation information so that
        /// the correct stack and source can be displayed in the debugger, for
        /// cases where the debugged runspace is called inside a parent script,
        /// such as with script Invoke-Command cases.
        /// <param name="debugStopInvocationInfo"></param>
        /// <returns>InvocationInfo.</returns>
        internal virtual InvocationInfo FixupInvocationInfo(InvocationInfo debugStopInvocationInfo)
            // Default is no fix up.
            return debugStopInvocationInfo;
        internal bool IsSameDebugger(Debugger testDebugger)
            return _wrappedDebugger.Equals(testDebugger);
        /// Checks to see if the runspace debugger is in a preserved debug
        /// stop state, and if so then allows the debugger stop event to
        /// continue processing and raise the event.
        internal void CheckStateAndRaiseStopEvent()
            RemoteDebugger remoteDebugger = _wrappedDebugger as RemoteDebugger;
            if (remoteDebugger != null)
                // Have remote debugger raise existing debugger stop event.
                remoteDebugger.CheckStateAndRaiseStopEvent();
            else if (this._wrappedDebugger.IsPendingDebugStopEvent)
                // Release local debugger preserved debugger stop event.
                this._wrappedDebugger.ReleaseSavedDebugStop();
                // If this is a remote server debugger then we want to convert the pending remote
                // debugger stop to a local debugger stop event for this Debug-Runspace to handle.
                ServerRemoteDebugger serverRemoteDebugger = this._wrappedDebugger as ServerRemoteDebugger;
                serverRemoteDebugger?.ReleaseAndRaiseDebugStopLocal();
        /// Gets the callstack of the nested runspace.
        internal PSDataCollection<PSObject> GetRSCallStack()
            // Get call stack from wrapped debugger
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-PSCallStack");
            PSDataCollection<PSObject> callStackOutput = new PSDataCollection<PSObject>();
            _wrappedDebugger.ProcessCommand(cmd, callStackOutput);
            return callStackOutput;
    /// Wrapper class for runspace debugger where it is running in no known
    /// embedding scenario and is assumed to be running independently of
    /// any other running script.
    internal sealed class StandaloneRunspaceDebugger : NestedRunspaceDebugger
        public StandaloneRunspaceDebugger(
            Runspace runspace)
            : base(runspace, PSMonitorRunspaceType.Standalone, Guid.Empty)
        protected override DebuggerCommandResults HandleCallStack(PSDataCollection<PSObject> output)
            PSDataCollection<PSObject> callStackOutput = GetRSCallStack();
            // Display call stack info as formatted.
                ps.AddCommand("Out-String").AddParameter("Stream", true);
                ps.Invoke(callStackOutput, output);
        protected override void HandleDebuggerStop(object sender, DebuggerStopEventArgs e)
            object runningCmd = null;
                runningCmd = DrainAndBlockRemoteOutput();
                RestoreRemoteOutput(runningCmd);
        private object DrainAndBlockRemoteOutput()
            // We do this only for remote runspaces.
            if (_runspace is not RemoteRunspace remoteRunspace) { return null; }
            var runningPowerShell = remoteRunspace.GetCurrentBasePowerShell();
            if (runningPowerShell != null)
                runningPowerShell.WaitForServicingComplete();
                runningPowerShell.SuspendIncomingData();
                return runningPowerShell;
                var runningPipe = remoteRunspace.GetCurrentlyRunningPipeline();
                if (runningPipe != null)
                    runningPipe.DrainIncomingData();
                    runningPipe.SuspendIncomingData();
                    return runningPipe;
        private static void RestoreRemoteOutput(object runningCmd)
            if (runningCmd == null) { return; }
            var runningPowerShell = runningCmd as PowerShell;
                runningPowerShell.ResumeIncomingData();
                var runningPipe = runningCmd as Pipeline;
                runningPipe?.ResumeIncomingData();
    /// Wrapper class for runspace debugger where the runspace is being used in an
    /// embedded scenario such as Invoke-Command command inside script.
    internal sealed class EmbeddedRunspaceDebugger : NestedRunspaceDebugger
        private PowerShell _command;
        private Debugger _rootDebugger;
        private ScriptBlockAst _parentScriptBlockAst;
        private DebuggerStopEventArgs _sendDebuggerArgs;
        /// Constructor for runspaces executing from script.
        /// <param name="rootDebugger">Root debugger.</param>
        /// <param name="runspaceType">Runspace to monitor type.</param>
        /// <param name="parentDebuggerId">Parent debugger Id.</param>
        public EmbeddedRunspaceDebugger(
            PowerShell command,
            Debugger rootDebugger,
            : base(runspace, runspaceType, parentDebuggerId)
            if (rootDebugger == null)
                throw new PSArgumentNullException(nameof(rootDebugger));
            _rootDebugger = rootDebugger;
            _sendDebuggerArgs = new DebuggerStopEventArgs(
                e.InvocationInfo,
                new Collection<Breakpoint>(e.Breakpoints),
                e.ResumeAction);
            object remoteRunningCmd = null;
                // For remote debugging drain/block output channel.
                remoteRunningCmd = DrainAndBlockRemoteOutput();
                this.RaiseDebuggerStopEvent(_sendDebuggerArgs);
                RestoreRemoteOutput(remoteRunningCmd);
                // Return user determined resume action.
                e.ResumeAction = _sendDebuggerArgs.ResumeAction;
            // First get call stack from wrapped debugger
            // Next get call stack from parent debugger.
            PSDataCollection<CallStackFrame> callStack = _rootDebugger.GetCallStack().ToArray();
            // Combine call stack info.
            foreach (var item in callStack)
                callStackOutput.Add(new PSObject(item));
        protected override bool HandleListCommand(PSDataCollection<PSObject> output)
            if ((_sendDebuggerArgs != null) && (_sendDebuggerArgs.InvocationInfo != null))
                return _rootDebugger.InternalProcessListCommand(_sendDebuggerArgs.InvocationInfo.ScriptLineNumber, output);
        /// <param name="debugStopInvocationInfo">Invocation information from debugger stop.</param>
        internal override InvocationInfo FixupInvocationInfo(InvocationInfo debugStopInvocationInfo)
            if (debugStopInvocationInfo == null) { return null; }
            // Check to see if this nested debug stop is called from within
            // a known parent source.
            int dbStopLineNumber = debugStopInvocationInfo.ScriptLineNumber;
            CallStackFrame topItem = null;
            var parentActiveStack = _rootDebugger.GetActiveDebuggerCallStack();
            if ((parentActiveStack != null) && (parentActiveStack.Length > 0))
                topItem = parentActiveStack[0];
                var parentStack = _rootDebugger.GetCallStack().ToArray();
                if ((parentStack != null) && (parentStack.Length > 0))
                    topItem = parentStack[0];
                    dbStopLineNumber--;
            InvocationInfo debugInvocationInfo = CreateInvocationInfoFromParent(
                topItem,
                dbStopLineNumber,
                debugStopInvocationInfo.ScriptPosition.StartColumnNumber,
                debugStopInvocationInfo.ScriptPosition.EndColumnNumber);
            return debugInvocationInfo ?? debugStopInvocationInfo;
        public override void Dispose()
            base.Dispose();
            _rootDebugger = null;
            _parentScriptBlockAst = null;
            _sendDebuggerArgs = null;
        private InvocationInfo CreateInvocationInfoFromParent(
            CallStackFrame parentStackFrame,
            int debugLineNumber,
            int debugStartColNumber,
            int debugEndColNumber)
            if (parentStackFrame == null) { return null; }
            // Attempt to find parent script file create script block with Ast to
            // find correct line and offset adjustments.
            if ((_parentScriptBlockAst == null) &&
                !string.IsNullOrEmpty(parentStackFrame.ScriptName) &&
                System.IO.File.Exists(parentStackFrame.ScriptName))
                _parentScriptBlockAst = Parser.ParseInput(
                    System.IO.File.ReadAllText(parentStackFrame.ScriptName),
                    out tokens, out errors);
            if (_parentScriptBlockAst != null)
                int callingLineNumber = parentStackFrame.ScriptLineNumber;
                StatementAst debugStatement = null;
                StatementAst callingStatement = _parentScriptBlockAst.Find(
                    ast => ast is StatementAst && (ast.Extent.StartLineNumber == callingLineNumber), true) as StatementAst;
                if (callingStatement != null)
                    // Find first statement in calling statement.
                    StatementAst firstStatement = callingStatement.Find(
                        ast => ast is StatementAst && ast.Extent.StartLineNumber > callingLineNumber, true) as StatementAst;
                    if (firstStatement != null)
                        int adjustedLineNumber = firstStatement.Extent.StartLineNumber + debugLineNumber - 1;
                        debugStatement = callingStatement.Find(
                            ast => ast is StatementAst && ast.Extent.StartLineNumber == adjustedLineNumber, true) as StatementAst;
                if (debugStatement != null)
                    int endColNum = debugStartColNumber + (debugEndColNumber - debugStartColNumber) - 2;
                    string statementExtentText = FixUpStatementExtent(debugStatement.Extent.StartColumnNumber - 1, debugStatement.Extent.Text);
                    ScriptPosition scriptStartPosition = new ScriptPosition(
                        parentStackFrame.ScriptName,
                        debugStatement.Extent.StartLineNumber,
                        debugStartColNumber,
                        statementExtentText);
                    ScriptPosition scriptEndPosition = new ScriptPosition(
                        debugStatement.Extent.EndLineNumber,
                        endColNum,
                    return InvocationInfo.Create(
                        parentStackFrame.InvocationInfo.MyCommand,
                            scriptStartPosition,
                            scriptEndPosition)
        private static string FixUpStatementExtent(int startColNum, string stateExtentText)
            Text.StringBuilder sb = new Text.StringBuilder();
            sb.Append(' ', startColNum);
            sb.Append(stateExtentText);
            // We only do this for remote runspaces.
            if (_runspace is not RemoteRunspace) { return null; }
                    _command.WaitForServicingComplete();
                    _command.SuspendIncomingData();
                Pipeline runningCmd = _runspace.GetCurrentlyRunningPipeline();
                if (runningCmd != null)
                    runningCmd.DrainIncomingData();
                    runningCmd.SuspendIncomingData();
                    return runningCmd;
            PowerShell command = runningCmd as PowerShell;
                command.ResumeIncomingData();
                Pipeline pipelineCommand = runningCmd as Pipeline;
                pipelineCommand?.ResumeIncomingData();
    #region DebuggerCommandResults
    /// Command results returned from Debugger.ProcessCommand.
    public sealed class DebuggerCommandResults
        /// Resume action.
        public DebuggerResumeAction? ResumeAction
        /// True if debugger evaluated command.  Otherwise evaluation was
        /// performed by PowerShell.
        public bool EvaluatedByDebugger { get; }
        private DebuggerCommandResults()
        /// <param name="resumeAction">Resume action.</param>
        /// <param name="evaluatedByDebugger">True if evaluated by debugger.</param>
        public DebuggerCommandResults(
            DebuggerResumeAction? resumeAction,
            bool evaluatedByDebugger)
            ResumeAction = resumeAction;
            EvaluatedByDebugger = evaluatedByDebugger;
    #region DebuggerCommandProcessor
    /// This class is used to pre-process the command read by the host when it is in debug mode; its
    /// main intention is to implement the debugger commands ("s", "c", "o", etc)
    internal class DebuggerCommandProcessor
        // debugger commands
        private const string ContinueCommand = "continue";
        private const string ContinueShortcut = "c";
        private const string GetStackTraceShortcut = "k";
        private const string HelpCommand = "h";
        private const string HelpShortcut = "?";
        private const string ListCommand = "list";
        private const string ListShortcut = "l";
        private const string StepCommand = "stepInto";
        private const string StepShortcut = "s";
        private const string StepOutCommand = "stepOut";
        private const string StepOutShortcut = "o";
        private const string StepOverCommand = "stepOver";
        private const string StepOverShortcut = "v";
        private const string StopCommand = "quit";
        private const string StopShortcut = "q";
        private const string DetachCommand = "detach";
        private const string DetachShortcut = "d";
        // default line count for the list command
        private const int DefaultListLineCount = 16;
        // table of debugger commands
        private readonly Dictionary<string, DebuggerCommand> _commandTable;
        // the Help command
        private readonly DebuggerCommand _helpCommand;
        // the List command
        private readonly DebuggerCommand _listCommand;
        // last command processed
        private DebuggerCommand _lastCommand;
        // the source script split into lines
        private string[] _lines;
        // last line displayed by the list command
        private int _lastLineDisplayed;
        private const string Crlf = "\x000D\x000A";
        /// Creates the table of debugger commands.
        public DebuggerCommandProcessor()
            _commandTable = new Dictionary<string, DebuggerCommand>(StringComparer.OrdinalIgnoreCase);
            _commandTable[StepCommand] = _commandTable[StepShortcut] = new DebuggerCommand(StepCommand, DebuggerResumeAction.StepInto, repeatOnEnter: true, executedByDebugger: false);
            _commandTable[StepOutCommand] = _commandTable[StepOutShortcut] = new DebuggerCommand(StepOutCommand, DebuggerResumeAction.StepOut, repeatOnEnter: false, executedByDebugger: false);
            _commandTable[StepOverCommand] = _commandTable[StepOverShortcut] = new DebuggerCommand(StepOverCommand, DebuggerResumeAction.StepOver, repeatOnEnter: true, executedByDebugger: false);
            _commandTable[ContinueCommand] = _commandTable[ContinueShortcut] = new DebuggerCommand(ContinueCommand, DebuggerResumeAction.Continue, repeatOnEnter: false, executedByDebugger: false);
            _commandTable[StopCommand] = _commandTable[StopShortcut] = new DebuggerCommand(StopCommand, DebuggerResumeAction.Stop, repeatOnEnter: false, executedByDebugger: false);
            _commandTable[GetStackTraceShortcut] = new DebuggerCommand("get-pscallstack", null, repeatOnEnter: false, executedByDebugger: false);
            _commandTable[HelpCommand] = _commandTable[HelpShortcut] = _helpCommand = new DebuggerCommand(HelpCommand, null, repeatOnEnter: false, executedByDebugger: true);
            _commandTable[ListCommand] = _commandTable[ListShortcut] = _listCommand = new DebuggerCommand(ListCommand, null, repeatOnEnter: true, executedByDebugger: true);
            _commandTable[string.Empty] = new DebuggerCommand(string.Empty, null, repeatOnEnter: false, executedByDebugger: true);
        /// Resets any state in the command processor.
            _lines = null;
        /// Process the command read by the host and returns the DebuggerResumeAction or the command
        /// that the host should execute (see comments in the DebuggerCommand class above).
        public DebuggerCommand ProcessCommand(PSHost host, string command, InvocationInfo invocationInfo)
            return _lastCommand = DoProcessCommand(host, command, invocationInfo, null);
        /// <param name="output"></param>
        public DebuggerCommand ProcessCommand(PSHost host, string command, InvocationInfo invocationInfo, IList<PSObject> output)
            DebuggerCommand dbgCommand = DoProcessCommand(host, command, invocationInfo, output);
            if (dbgCommand.ExecutedByDebugger || (dbgCommand.ResumeAction != null)) { _lastCommand = dbgCommand; }
            return dbgCommand;
        /// Process list command with provided line number.
        /// <param name="invocationInfo">Current InvocationInfo.</param>
        public void ProcessListCommand(InvocationInfo invocationInfo, IList<PSObject> output)
            DoProcessCommand(null, "list", invocationInfo, output);
        /// Looks up string command and if it is a debugger command returns the
        /// corresponding DebuggerCommand object.
        /// <param name="command">String command.</param>
        /// <returns>DebuggerCommand or null.</returns>
        public DebuggerCommand ProcessBasicCommand(string command)
            if (command.Length == 0 && _lastCommand != null && _lastCommand.RepeatOnEnter)
                return _lastCommand;
            DebuggerCommand debuggerCommand;
            if (_commandTable.TryGetValue(command, out debuggerCommand))
                if (debuggerCommand.ExecutedByDebugger || (debuggerCommand.ResumeAction != null)) { _lastCommand = debuggerCommand; }
                return debuggerCommand;
        /// Helper for ProcessCommand.
        private DebuggerCommand DoProcessCommand(PSHost host, string command, InvocationInfo invocationInfo, IList<PSObject> output)
            // check for <enter>
                if (_lastCommand == _listCommand)
                    if (_lines != null && _lastLineDisplayed < _lines.Length)
                        DisplayScript(host, output, invocationInfo, _lastLineDisplayed + 1, DefaultListLineCount);
                    return _listCommand;
                command = _lastCommand.Command;
            // Check for the list command using a regular expression
            Regex listCommandRegex = new Regex(@"^l(ist)?(\s+(?<start>\S+))?(\s+(?<count>\S+))?$", RegexOptions.IgnoreCase);
            Match match = listCommandRegex.Match(command);
                DisplayScript(host, output, invocationInfo, match);
            // Check for the rest of the debugger commands
            DebuggerCommand debuggerCommand = null;
                // Check for the help command
                if (debuggerCommand == _helpCommand)
                    DisplayHelp(host, output);
            // Else return the same command
        /// Displays the help text for the debugger commands.
        private static void DisplayHelp(PSHost host, IList<PSObject> output)
            WriteLine(string.Empty, host, output);
            WriteLine(StringUtil.Format(DebuggerStrings.StepHelp, StepShortcut, StepCommand), host, output);
            WriteLine(StringUtil.Format(DebuggerStrings.StepOverHelp, StepOverShortcut, StepOverCommand), host, output);
            WriteLine(StringUtil.Format(DebuggerStrings.StepOutHelp, StepOutShortcut, StepOutCommand), host, output);
            WriteLine(StringUtil.Format(DebuggerStrings.ContinueHelp, ContinueShortcut, ContinueCommand), host, output);
            WriteLine(StringUtil.Format(DebuggerStrings.StopHelp, StopShortcut, StopCommand), host, output);
            WriteLine(StringUtil.Format(DebuggerStrings.DetachHelp, DetachShortcut, DetachCommand), host, output);
            WriteLine(StringUtil.Format(DebuggerStrings.GetStackTraceHelp, GetStackTraceShortcut), host, output);
            WriteLine(StringUtil.Format(DebuggerStrings.ListHelp, ListShortcut, ListCommand), host, output);
            WriteLine(StringUtil.Format(DebuggerStrings.AdditionalListHelp1), host, output);
            WriteLine(StringUtil.Format(DebuggerStrings.AdditionalListHelp2), host, output);
            WriteLine(StringUtil.Format(DebuggerStrings.AdditionalListHelp3), host, output);
            WriteLine(StringUtil.Format(DebuggerStrings.EnterHelp, StepCommand, StepOverCommand, ListCommand), host, output);
            WriteLine(StringUtil.Format(DebuggerStrings.HelpCommandHelp, HelpShortcut, HelpCommand), host, output);
            WriteLine("\n", host, output);
            WriteLine(StringUtil.Format(DebuggerStrings.PromptHelp), host, output);
        /// Executes the list command.
        private void DisplayScript(PSHost host, IList<PSObject> output, InvocationInfo invocationInfo, Match match)
            if (invocationInfo == null) { return; }
            // Get the source code for the script
            if (_lines == null)
                string scriptText = invocationInfo.GetFullScript();
                if (string.IsNullOrEmpty(scriptText))
                    WriteErrorLine(StringUtil.Format(DebuggerStrings.NoSourceCode), host, output);
                _lines = scriptText.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            // Get the starting line
            int start = Math.Max(invocationInfo.ScriptLineNumber - 5, 1);
            if (match.Groups["start"].Value.Length > 0)
                    start = int.Parse(match.Groups["start"].Value, CultureInfo.CurrentCulture.NumberFormat);
                    WriteErrorLine(StringUtil.Format(DebuggerStrings.BadStartFormat, _lines.Length), host, output);
                if (start <= 0 || start > _lines.Length)
            // Get the line count
            int count = DefaultListLineCount;
            if (match.Groups["count"].Value.Length > 0)
                    count = int.Parse(match.Groups["count"].Value, CultureInfo.CurrentCulture.NumberFormat);
                    WriteErrorLine(StringUtil.Format(DebuggerStrings.BadCountFormat, _lines.Length), host, output);
                // Limit requested line count to maximum number of existing lines
                count = (count > _lines.Length) ? _lines.Length : count;
                if (count <= 0)
                    WriteErrorLine(DebuggerStrings.BadCountFormat, host, output);
            // Execute the command
            DisplayScript(host, output, invocationInfo, start, count);
        private void DisplayScript(PSHost host, IList<PSObject> output, InvocationInfo invocationInfo, int start, int count)
            WriteCR(host, output);
            for (int lineNumber = start; lineNumber <= _lines.Length && lineNumber < start + count; lineNumber++)
                    lineNumber == invocationInfo.ScriptLineNumber
                        ? string.Format(CultureInfo.CurrentCulture, "{0,5}:* {1}", lineNumber, _lines[lineNumber - 1])
                        : string.Format(CultureInfo.CurrentCulture, "{0,5}:  {1}", lineNumber, _lines[lineNumber - 1]),
                _lastLineDisplayed = lineNumber;
        private static void WriteLine(string line, PSHost host, IList<PSObject> output)
            host?.UI.WriteLine(line);
            output?.Add(new PSObject(line));
        private static void WriteCR(PSHost host, IList<PSObject> output)
            host?.UI.WriteLine();
            output?.Add(new PSObject(Crlf));
        private static void WriteErrorLine(string error, PSHost host, IList<PSObject> output)
            host?.UI.WriteErrorLine(error);
            output?.Add(
                new PSObject(
                        new RuntimeException(error),
                        "DebuggerError",
                        null)));
    /// Class used to hold the output of the DebuggerCommandProcessor.
    internal class DebuggerCommand
        public DebuggerCommand(string command, DebuggerResumeAction? action, bool repeatOnEnter, bool executedByDebugger)
            ResumeAction = action;
            RepeatOnEnter = repeatOnEnter;
            ExecutedByDebugger = executedByDebugger;
        /// If ResumeAction is not null it indicates that the host must exit the debugger
        /// and resume execution of the suspended pipeline; the debugger will use the
        /// value of this property to decide how to resume the pipeline (i.e. step into,
        /// step-over, continue, etc)
        public DebuggerResumeAction? ResumeAction { get; }
        /// When ResumeAction is null, this property indicates the command that the
        /// host should pass to the PowerShell engine.
        /// If true, the host should repeat this command if the next command in an empty line (enter)
        public bool RepeatOnEnter { get; }
        /// If true, the command was executed by the debugger and the host should ignore the command.
        public bool ExecutedByDebugger { get; }
    #region PSDebugContext class
    /// This class exposes the information about the debugger that is available via $PSDebugContext.
    public class PSDebugContext
        /// Initializes a new instance of the <see cref="PSDebugContext"/> class.
        /// <param name="invocationInfo">The invocation information for the current command.</param>
        /// <param name="breakpoints">The breakpoint(s) that caused the script to break in the debugger.</param>
        public PSDebugContext(InvocationInfo invocationInfo, List<Breakpoint> breakpoints)
            : this(invocationInfo, breakpoints, triggerObject: null)
        /// <param name="triggerObject">The object that caused the script to break in the debugger.</param>
        public PSDebugContext(InvocationInfo invocationInfo, List<Breakpoint> breakpoints, object triggerObject)
                throw new PSArgumentNullException(nameof(breakpoints));
            this.Breakpoints = breakpoints.ToArray();
            this.Trigger = triggerObject;
        /// InvocationInfo of the command currently being executed.
        public InvocationInfo InvocationInfo { get; }
        /// If not empty, indicates that the execution was suspended because one or more breakpoints
        /// were hit. Otherwise, the execution was suspended as part of a step operation.
        public Breakpoint[] Breakpoints { get; }
        /// Gets the object that triggered the current dynamic breakpoint.
        public object Trigger { get; }
    #region CallStackFrame class
    /// A call stack item returned by the Get-PSCallStack cmdlet.
    public sealed class CallStackFrame
        /// <param name="invocationInfo">Invocation Info.</param>
        public CallStackFrame(InvocationInfo invocationInfo)
            : this(null, invocationInfo)
        /// <param name="functionContext">Function context.</param>
        internal CallStackFrame(FunctionContext functionContext, InvocationInfo invocationInfo)
                throw new PSArgumentNullException(nameof(invocationInfo));
            if (functionContext != null)
                FunctionContext = functionContext;
                this.Position = functionContext.CurrentPosition;
                // WF functions do not have functionContext.  Use InvocationInfo.
                this.Position = invocationInfo.ScriptPosition;
                FunctionContext = new FunctionContext();
                FunctionContext._functionName = invocationInfo.ScriptName;
        /// File name of the current location, or null if the frame is not associated to a script.
            get { return Position.File; }
        /// Line number of the current location, or 0 if the frame is not associated to a script.
            get { return Position.StartLineNumber; }
        /// The InvocationInfo of the command.
        /// The position information for the current position in the frame.  Null if the frame is not
        /// associated with a script.
        public IScriptExtent Position { get; }
        /// The name of the function associated with this frame.
        public string FunctionName { get { return FunctionContext._functionName; } }
        internal FunctionContext FunctionContext { get; }
        /// Returns a formatted string containing the ScriptName and ScriptLineNumber.
        public string GetScriptLocation()
            if (string.IsNullOrEmpty(this.ScriptName))
                return DebuggerStrings.NoFile;
            return StringUtil.Format(DebuggerStrings.LocationFormat, Path.GetFileName(this.ScriptName), this.ScriptLineNumber);
        /// Return a dictionary with the names and values of variables that are "local"
        /// to the frame.
        public Dictionary<string, PSVariable> GetFrameVariables()
            if (FunctionContext._executionContext == null) { return result; }
            var scope = FunctionContext._executionContext.EngineSessionState.CurrentScope;
                if (scope.LocalsTuple == FunctionContext._localsTuple)
                    // We can ignore any dotted scopes.
                if (scope.DottedScopes != null && scope.DottedScopes.Any(s => s == FunctionContext._localsTuple))
                    var dottedScopes = scope.DottedScopes.ToArray();
                    // Skip dotted scopes above the current scope
                    for (i = 0; i < dottedScopes.Length; ++i)
                        if (dottedScopes[i] == FunctionContext._localsTuple)
                    for (; i < dottedScopes.Length; ++i)
                        dottedScopes[i].GetVariableTable(result, true);
            FunctionContext._localsTuple.GetVariableTable(result, true);
        /// ToString override.
            return StringUtil.Format(DebuggerStrings.StackTraceFormat, FunctionName,
                                     ScriptName ?? DebuggerStrings.NoFile, ScriptLineNumber);
    #region DebuggerUtils
    /// Debugger Utilities class.
    public static class DebuggerUtils
        private static readonly SortedSet<string> s_noHistoryCommandNames = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
            "prompt",
            "Set-PSDebuggerAction",
            "Get-PSDebuggerStopArgs",
            "Set-PSDebugMode",
            "TabExpansion2"
        /// Helper method to determine if command should be added to debugger
        /// history.
        /// <returns>True if command can be added to history.</returns>
        public static bool ShouldAddCommandToHistory(string command)
            lock (s_noHistoryCommandNames)
                return !(s_noHistoryCommandNames.Contains(command, StringComparer.OrdinalIgnoreCase));
        /// Start monitoring a runspace on the target debugger.
        /// <param name="debugger">Target debugger.</param>
        /// <param name="runspaceInfo">PSMonitorRunspaceInfo.</param>
        public static void StartMonitoringRunspace(Debugger debugger, PSMonitorRunspaceInfo runspaceInfo)
                throw new PSArgumentNullException(nameof(debugger));
            if (runspaceInfo == null)
                throw new PSArgumentNullException(nameof(runspaceInfo));
            debugger.StartMonitoringRunspace(runspaceInfo);
        /// End monitoring a runspace on the target debugger.
        public static void EndMonitoringRunspace(Debugger debugger, PSMonitorRunspaceInfo runspaceInfo)
            debugger.EndMonitoringRunspace(runspaceInfo);
    #region PSMonitorRunspaceEvent
    /// PSMonitorRunspaceEvent.
    public enum PSMonitorRunspaceType
        /// Standalone runspace.
        Standalone = 0,
        /// Runspace from remote Invoke-Command script.
        InvokeCommand,
    /// Runspace information for monitoring runspaces for debugging.
    public abstract class PSMonitorRunspaceInfo
        /// Created Runspace.
        /// Type of runspace for monitoring.
        /// Nested debugger wrapper for runspace debugger.
        internal NestedRunspaceDebugger NestedDebugger { get; set; }
        private PSMonitorRunspaceInfo() { }
        protected PSMonitorRunspaceInfo(
            PSMonitorRunspaceType runspaceType)
        /// Returns a copy of this object.
        internal abstract PSMonitorRunspaceInfo Copy();
        /// Creates an instance of a NestedRunspaceDebugger.
        /// <param name="rootDebugger">Root debugger or null.</param>
        /// <returns>NestedRunspaceDebugger.</returns>
        internal abstract NestedRunspaceDebugger CreateDebugger(Debugger rootDebugger);
    /// Standalone runspace information for monitoring runspaces for debugging.
    public sealed class PSStandaloneMonitorRunspaceInfo : PSMonitorRunspaceInfo
        /// Creates instance of PSStandaloneMonitorRunspaceInfo.
        /// <param name="runspace">Runspace to monitor.</param>
        public PSStandaloneMonitorRunspaceInfo(
            : base(runspace, PSMonitorRunspaceType.Standalone)
        internal override PSMonitorRunspaceInfo Copy()
            return new PSStandaloneMonitorRunspaceInfo(Runspace);
        /// <returns>NestedRunspaceDebugger wrapper.</returns>
        internal override NestedRunspaceDebugger CreateDebugger(Debugger rootDebugger)
            return new StandaloneRunspaceDebugger(Runspace);
    /// Embedded runspaces running in context of a parent script, used for monitoring
    /// runspace debugging.
    public sealed class PSEmbeddedMonitorRunspaceInfo : PSMonitorRunspaceInfo
        /// PowerShell command to run.  Can be null.
        public PowerShell Command { get; }
        /// Unique parent debugger identifier.
        public Guid ParentDebuggerId { get; private set; }
        /// Creates instance of PSEmbeddedMonitorRunspaceInfo.
        /// <param name="runspaceType">Type of runspace.</param>
        /// <param name="command">Running command.</param>
        /// <param name="parentDebuggerId">Unique parent debugger id or null.</param>
        public PSEmbeddedMonitorRunspaceInfo(
            : base(runspace, runspaceType)
            return new PSEmbeddedMonitorRunspaceInfo(
                RunspaceType,
                ParentDebuggerId);
            return new EmbeddedRunspaceDebugger(
                rootDebugger,

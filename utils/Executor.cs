    /// Executor wraps a Pipeline instance, and provides helper methods for executing commands in that pipeline. It is used to
    /// provide bookkeeping and structure to the use of pipeline in such a way that they can be interrupted and cancelled by a
    /// break event handler, and to track nesting of pipelines (which happens with interrupted input loops (aka subshells) and
    /// use of tab-completion in prompts). The bookkeeping is necessary because the break handler is static and global, and
    /// there is no means for tying a break handler to an instance of an object.
    /// The class' instance methods manage a single pipeline.  The class' static methods track the outstanding instances to
    /// ensure that only one instance is 'active' (and therefore cancellable) at a time.
    internal class Executor
        internal enum ExecutionOptions
            None = 0x0,
            AddOutputter = 0x01,
            AddToHistory = 0x02,
            ReadInputObjects = 0x04
        /// <param name="parent">
        /// A reference to the parent ConsoleHost that created this instance.
        /// <param name="useNestedPipelines">
        /// true if the executor is supposed to use nested pipelines; false if not.
        /// <param name="isPromptFunctionExecutor">
        /// True if the instance will be used to execute the prompt function, which will delay stopping the pipeline by some
        /// milliseconds.  This will prevent us from stopping the pipeline so quickly that, when the user leans on the ctrl-c
        /// key, the prompt "stops working" (because it is being stopped faster than it can run to completion).
        internal Executor(ConsoleHost parent, bool useNestedPipelines, bool isPromptFunctionExecutor)
            Dbg.Assert(parent != null, "parent should not be null");
            this.useNestedPipelines = useNestedPipelines;
            _isPromptFunctionExecutor = isPromptFunctionExecutor;
        #region async
        // called on the pipeline thread
        private void OutputObjectStreamHandler(object sender, EventArgs e)
            // e is just an empty instance of EventArgs, so we ignore it. sender is the PipelineReader that raised it's
            // DataReady event that calls this handler, which is the PipelineReader for the Output object stream.
            PipelineReader<PSObject> reader = (PipelineReader<PSObject>)sender;
            // We use NonBlockingRead instead of Read, as Read would block if the reader has no objects.  While it would be
            // inconsistent for this method to be called when there are no objects, since it will be called synchronously on
            // the pipeline thread, blocking in this call until an object is streamed would deadlock the pipeline. So we
            // prefer to take no chance of blocking.
            Collection<PSObject> objects = reader.NonBlockingRead();
            foreach (PSObject obj in objects)
                _parent.OutputSerializer.Serialize(obj);
        private void ErrorObjectStreamHandler(object sender, EventArgs e)
            // DataReady event that calls this handler, which is the PipelineReader for the Error object stream.
            PipelineReader<object> reader = (PipelineReader<object>)sender;
            Collection<object> objects = reader.NonBlockingRead();
            foreach (object obj in objects)
                _parent.ErrorSerializer.Serialize(obj);
        /// This method handles failures in executing the pipeline asynchronously.
        /// <param name="ex"></param>
        private void AsyncPipelineFailureHandler(Exception ex)
            if (ex is IContainsErrorRecord cer)
                er = cer.ErrorRecord;
                // The exception inside the error record is ParentContainsErrorRecordException, which
                // doesn't have a stack trace. Replace it with the top level exception.
                er = new ErrorRecord(er, ex);
            er ??= new ErrorRecord(ex, "ConsoleHostAsyncPipelineFailure", ErrorCategory.NotSpecified, null);
            _parent.ErrorSerializer.Serialize(er);
        private sealed class PipelineFinishedWaitHandle
            internal PipelineFinishedWaitHandle(Pipeline p)
                p.StateChanged += PipelineStateChangedHandler;
            internal void Wait()
                _eventHandle.WaitOne();
            private void PipelineStateChangedHandler(object sender, PipelineStateEventArgs e)
                    e.PipelineStateInfo.State == PipelineState.Completed
                    || e.PipelineStateInfo.State == PipelineState.Failed
                    || e.PipelineStateInfo.State == PipelineState.Stopped)
                    _eventHandle.Set();
            private readonly System.Threading.ManualResetEvent _eventHandle = new System.Threading.ManualResetEvent(false);
        internal void ExecuteCommandAsync(string command, out Exception exceptionThrown, ExecutionOptions options)
            Dbg.Assert(!useNestedPipelines, "can't async invoke a nested pipeline");
            bool addToHistory = (options & ExecutionOptions.AddToHistory) > 0;
            Pipeline tempPipeline = _parent.RunspaceRef.CreatePipeline(command, addToHistory, false);
            ExecuteCommandAsyncHelper(tempPipeline, out exceptionThrown, options);
        /// Executes a pipeline in the console when we are running asnyc.
        /// <param name="tempPipeline">
        /// The pipeline to execute.
        /// <param name="exceptionThrown">
        /// Any exception thrown trying to run the pipeline.
        /// <param name="options">
        /// The options to use to execute the pipeline.
        internal void ExecuteCommandAsyncHelper(Pipeline tempPipeline, out Exception exceptionThrown, ExecutionOptions options)
            Dbg.Assert(!_isPromptFunctionExecutor, "should not async invoke the prompt");
            exceptionThrown = null;
            Executor oldCurrent = CurrentExecutor;
            CurrentExecutor = this;
            lock (_instanceStateLock)
                Dbg.Assert(_pipeline == null, "no other pipeline should exist");
                _pipeline = tempPipeline;
                if ((options & ExecutionOptions.AddOutputter) > 0 && _parent.OutputFormat == Serialization.DataFormat.Text)
                    // Tell the script command to merge it's output and error streams
                    if (tempPipeline.Commands.Count == 1)
                        tempPipeline.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
                    // then add out-default to the pipeline to render everything...
                    Command outDefault = new Command("Out-Default", /* isScript */false, /* useLocalScope */ true);
                    tempPipeline.Commands.Add(outDefault);
                tempPipeline.Output.DataReady += OutputObjectStreamHandler;
                tempPipeline.Error.DataReady += ErrorObjectStreamHandler;
                PipelineFinishedWaitHandle pipelineWaiter = new PipelineFinishedWaitHandle(tempPipeline);
                // close the input pipeline so the command will do something
                // if we are not reading input
                if ((options & Executor.ExecutionOptions.ReadInputObjects) == 0)
                    tempPipeline.Input.Close();
                tempPipeline.InvokeAsync();
                if ((options & ExecutionOptions.ReadInputObjects) > 0 && Console.IsInputRedirected)
                    // read input objects from stdin
                    WrappedDeserializer des = new WrappedDeserializer(_parent.InputFormat, "Input", _parent.ConsoleIn.Value);
                    while (!des.AtEnd)
                        object o = des.Deserialize();
                            tempPipeline.Input.Write(o);
                        catch (PipelineClosedException)
                            // This Exception can occur when the input is closed. This can happen
                            // for various reasons. For example: The command in the pipeline is invalid and
                            // command discovery throws an exception, which closes the pipeline and
                            // hence the Input pipe.
                    des.End();
                pipelineWaiter.Wait();
                // report error if pipeline failed
                if (tempPipeline.PipelineStateInfo.State == PipelineState.Failed && tempPipeline.PipelineStateInfo.Reason != null)
                    if (_parent.OutputFormat == Serialization.DataFormat.Text)
                        // Report the exception using normal error reporting
                        exceptionThrown = tempPipeline.PipelineStateInfo.Reason;
                        // serialize the error record
                        AsyncPipelineFailureHandler(tempPipeline.PipelineStateInfo.Reason);
                exceptionThrown = e;
                // Once we have the results, or an exception is thrown, we throw away the pipeline.
                _parent.ui.ResetProgress();
                CurrentExecutor = oldCurrent;
        #endregion async
        internal Pipeline CreatePipeline()
            if (useNestedPipelines)
                return _parent.RunspaceRef.CreateNestedPipeline();
                return _parent.RunspaceRef.CreatePipeline();
        internal Pipeline CreatePipeline(string command, bool addToHistory)
            return _parent.RunspaceRef.CreatePipeline(command, addToHistory, useNestedPipelines);
        /// All calls to the Runspace to execute a command line must be done with this function, which properly synchronizes
        /// access to the running pipeline between the main thread and the break handler thread. This synchronization is
        /// necessary so that executions can be aborted with Ctrl-C (including evaluation of the prompt and collection of
        /// command-completion candidates).
        /// On any given Executor instance, ExecuteCommand should be called at most once at a time by any one thread. It is NOT
        /// reentrant.
        /// <param name="command">
        /// The command line to be executed. Must be non-null.
        /// Receives the Exception thrown by the execution of the command, if any. If no exception is thrown, then set to null.
        /// Can be tested to see if the execution was successful or not.
        /// Options to govern the execution
        /// The object stream resulting from the execution. May be null.
        internal Collection<PSObject> ExecuteCommand(string command, out Exception exceptionThrown, ExecutionOptions options)
            Pipeline tempPipeline = CreatePipeline(command, (options & ExecutionOptions.AddToHistory) > 0);
            return ExecuteCommandHelper(tempPipeline, out exceptionThrown, options);
        private static Command GetOutDefaultCommand(bool endOfStatement)
            return new Command(command: "Out-Default",
                               isScript: false,
                               useLocalScope: true,
                               mergeUnclaimedPreviousErrorResults: true)
                IsEndOfStatement = endOfStatement
        internal Collection<PSObject> ExecuteCommandHelper(Pipeline tempPipeline, out Exception exceptionThrown, ExecutionOptions options)
            Dbg.Assert(tempPipeline != null, "command should have a value");
            Collection<PSObject> results = null;
            if ((options & ExecutionOptions.AddOutputter) > 0)
                if (tempPipeline.Commands.Count < 2)
                        // Tell the script command to merge it's output and error streams.
                    // Add Out-Default to the pipeline to render.
                    tempPipeline.Commands.Add(GetOutDefaultCommand(endOfStatement: false));
                    // For multiple commands/scripts we need to insert Out-Default at the end of each statement.
                    CommandCollection executeCommands = new CommandCollection();
                    foreach (var cmd in tempPipeline.Commands)
                        executeCommands.Add(cmd);
                        if (cmd.IsEndOfStatement)
                            // End of statement needs to pipe to Out-Default.
                            cmd.IsEndOfStatement = false;
                            executeCommands.Add(GetOutDefaultCommand(endOfStatement: true));
                    var lastCmd = executeCommands.Last();
                    if (!((lastCmd.CommandText != null) &&
                          (lastCmd.CommandText.Equals("Out-Default", StringComparison.OrdinalIgnoreCase)))
                        // Ensure pipeline output goes to Out-Default.
                        executeCommands.Add(GetOutDefaultCommand(endOfStatement: false));
                    tempPipeline.Commands.Clear();
                    foreach (var cmd in executeCommands)
                        tempPipeline.Commands.Add(cmd);
                // blocks until all results are retrieved.
                results = tempPipeline.Invoke();
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Needed by ProfileTests as mentioned in bug 140572")]
        internal Collection<PSObject> ExecuteCommand(string command)
            Collection<PSObject> result = null;
                result = ExecuteCommand(command, out e, ExecutionOptions.None);
        /// Executes a command (by calling this.ExecuteCommand), and coerces the first result object to a string. Any Exception
        /// thrown in the course of execution is returned through the exceptionThrown parameter.
        /// The command to execute. May be any valid monad command.
        /// Receives the Exception thrown by the execution of the command, if any. Set to null if no exception is thrown.
        /// The string representation of the first result object returned, or null if an exception was thrown or no objects were
        /// returned by the command.
        internal string ExecuteCommandAndGetResultAsString(string command, out Exception exceptionThrown)
                Collection<PSObject> streamResults = ExecuteCommand(command, out exceptionThrown, ExecutionOptions.None);
                if (exceptionThrown != null)
                if (streamResults == null || streamResults.Count == 0)
                // we got back one or more objects. Pick off the first result.
                if (streamResults[0] == null)
                // And convert the base object into a string. We can't use the proxied
                // ToString() on the PSObject because there is no default runspace
                // available.
                if (streamResults[0] is PSObject msho)
                    result = msho.BaseObject.ToString();
                    result = streamResults[0].ToString();
        /// Executes a command (by calling this.ExecuteCommand), and coerces the first result object to a bool. Any Exception
        /// thrown in the course of execution is caught and ignored.
        /// The Nullable`bool representation of the first result object returned, or null if an exception was thrown or no
        /// objects were returned by the command.
        internal bool? ExecuteCommandAndGetResultAsBool(string command)
            bool? result = ExecuteCommandAndGetResultAsBool(command, out _);
        internal bool? ExecuteCommandAndGetResultAsBool(string command, out Exception exceptionThrown)
            bool? result = null;
                // we got back one or more objects.
                result = (streamResults.Count > 1) || (LanguagePrimitives.IsTrue(streamResults[0]));
        /// Cancels execution of the current instance. Does nothing if the current instance is not running. Called in
        /// response to a break handler, by the static Executor.Cancel method.
        private void Cancel()
            // if there's a pipeline running, stop it.
                if (_pipeline != null && !_cancelled)
                    _cancelled = true;
                    if (_isPromptFunctionExecutor)
                        System.Threading.Thread.Sleep(100);
                    _pipeline.Stop();
            if (_pipeline is RemotePipeline remotePipeline)
                // Waits until queued data is handled.
                remotePipeline.DrainIncomingData();
                // Blocks any new data.
                remotePipeline.SuspendIncomingData();
            RemotePipeline remotePipeline = _pipeline as RemotePipeline;
            // Resumes data flow.
            remotePipeline?.ResumeIncomingData();
        /// Resets the instance to its post-ctor state. Does not cancel execution.
                _cancelled = false;
        /// Makes the given instance the "current" instance, that is, the instance that will receive a Cancel call if the break
        /// handler is triggered and calls the static Cancel method.
        /// The instance to make current. Null is allowed.
        /// Here are some state-transition cases to illustrate the use of CurrentExecutor
        /// null is current
        /// p1.ExecuteCommand
        ///     set p1 as current
        ///     promptforparams
        ///         tab complete
        ///             p2.ExecuteCommand
        ///                 set p2 as current
        ///                 p2.Execute completes
        ///                 restore old current to p1
        ///     p1.Execute completes
        ///     restore null as current
        /// Here's another case:
        ///     ShouldProcess - suspend
        ///         EnterNestedPrompt
        ///             set null as current so that break does not exit the subshell
        ///             evaluate prompt
        ///                 p2.ExecuteCommand
        ///                    set p2 as current
        ///                    Execute completes
        ///                    restore null as current
        ///            nested loop exit
        ///            restore p1 as current
        /// Summary:
        /// ExecuteCommand always saves/sets/restores CurrentExecutor
        /// Host.EnterNestedPrompt always saves/clears/restores CurrentExecutor
        internal static Executor CurrentExecutor
                Executor result = null;
                lock (s_staticStateLock)
                    result = s_currentExecutor;
                    // null is acceptable.
                    s_currentExecutor = value;
        /// Cancels the execution of the current instance (the instance last passed to PushCurrentExecutor), if any. Does
        /// nothing if no instance is Current.
        internal static void CancelCurrentExecutor()
            Executor temp = null;
                temp = s_currentExecutor;
            temp?.Cancel();
        // These statics are threadsafe, as there can be only one instance of ConsoleHost in a process at a time, and access
        // to currentExecutor is guarded by staticStateLock, and static initializers are run by the CLR at program init time.
        private static Executor s_currentExecutor;
        private static readonly object s_staticStateLock = new object();
        private Pipeline _pipeline;
        private bool _cancelled;
        internal bool useNestedPipelines;
        private readonly object _instanceStateLock = new object();
        private readonly bool _isPromptFunctionExecutor;

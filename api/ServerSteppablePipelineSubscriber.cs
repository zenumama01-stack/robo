// Warning: Events StartSteppablePipeline and RunProcessRecord are never used
// They are actually used by the event manager in some dynamically generated IL
#pragma warning disable 0067
    /// Event handler argument.
    internal class ServerSteppablePipelineDriverEventArg : EventArgs
        internal ServerSteppablePipelineDriver SteppableDriver;
        internal ServerSteppablePipelineDriverEventArg(ServerSteppablePipelineDriver driver)
            this.SteppableDriver = driver;
    /// Steppable pipeline driver event handler class.
    internal class ServerSteppablePipelineSubscriber
        private bool _initialized = false;
        private PSLocalEventManager _eventManager;
        private PSEventSubscriber _startSubscriber;
        private PSEventSubscriber _processSubscriber;
        internal void SubscribeEvents(ServerSteppablePipelineDriver driver)
                if (!_initialized)
                    _eventManager = (object)driver.LocalPowerShell.Runspace.Events as PSLocalEventManager;
                    if (_eventManager != null)
                        _startSubscriber = _eventManager.SubscribeEvent(this, "StartSteppablePipeline", Guid.NewGuid().ToString(), null,
                            new PSEventReceivedEventHandler(this.HandleStartEvent), true, false, true);
                        _processSubscriber = _eventManager.SubscribeEvent(this, "RunProcessRecord", Guid.NewGuid().ToString(), null,
                            new PSEventReceivedEventHandler(this.HandleProcessRecord), true, false, true);
                    _initialized = true;
        #region Events and Handlers
        public event EventHandler<EventArgs> StartSteppablePipeline;
        public event EventHandler<EventArgs> RunProcessRecord;
        /// Handles the start pipeline event, this is called by the event manager.
        private void HandleStartEvent(object sender, PSEventArgs args)
            ServerSteppablePipelineDriverEventArg driverArg = (object)args.SourceEventArgs as ServerSteppablePipelineDriverEventArg;
            ServerSteppablePipelineDriver driver = driverArg.SteppableDriver;
            Exception exceptionOccurred = null;
                using (ExecutionContextForStepping ctxt =
                    ExecutionContextForStepping.PrepareExecutionContext(
                        driver.LocalPowerShell.GetContextFromTLS(),
                        driver.LocalPowerShell.InformationalBuffers,
                        driver.RemoteHost))
                    driver.SteppablePipeline = driver.LocalPowerShell.GetSteppablePipeline();
                    driver.SteppablePipeline.Begin(!driver.NoInput);
                if (driver.NoInput)
                    driver.HandleInputEndReceived(this, EventArgs.Empty);
                // We need to catch this so that we can set the pipeline execution;
                // state to "failed" and send the exception as an error to the user.
                // Otherwise, the event manager will swallow this exception and
                // cause the client to not respond.
                exceptionOccurred = e;
            if (exceptionOccurred != null)
                driver.SetState(PSInvocationState.Failed, exceptionOccurred);
        /// Handles process record event.
        private void HandleProcessRecord(object sender, PSEventArgs args)
            lock (driver.SyncObject)
                // Make sure start event handler was called
                if (driver.SteppablePipeline == null)
                // make sure only one thread does the processing
                if (driver.ProcessingInput)
                driver.ProcessingInput = true;
                driver.Pulsed = false;
            bool shouldDoComplete = false;
                    bool isProcessCalled = false;
                        if (driver.PipelineState != PSInvocationState.Running)
                            driver.SetState(driver.PipelineState, null);
                        if (!driver.InputEnumerator.MoveNext())
                            shouldDoComplete = true;
                            if (!driver.NoInput || isProcessCalled)
                                // if there is noInput then we
                                // need to call process at least once
                        isProcessCalled = true;
                        Array output;
                            output = driver.SteppablePipeline.Process();
                            output = driver.SteppablePipeline.Process(driver.InputEnumerator.Current);
                        foreach (object o in output)
                            driver.DataStructureHandler.SendOutputDataToClient(PSObject.AsPSObject(o));
                            driver.TotalObjectsProcessed++;
                            if (driver.TotalObjectsProcessed >= driver.Input.Count)
                    driver.ProcessingInput = false;
                    driver.CheckAndPulseForProcessing(false);
                // Check if should perform stop
                if (driver.PipelineState == PSInvocationState.Stopping)
                    driver.PerformStop();
            if (shouldDoComplete)
                        Array output = driver.SteppablePipeline.End();
                        driver.SetState(PSInvocationState.Completed, null);
        /// Fires the start event.
        /// <param name="driver">Steppable pipeline driver.</param>
        internal void FireStartSteppablePipeline(ServerSteppablePipelineDriver driver)
                _eventManager?.GenerateEvent(_startSubscriber.SourceIdentifier, this,
                    new object[1] { new ServerSteppablePipelineDriverEventArg(driver) }, null, true, false);
        /// Fires the process record event.
        internal void FireHandleProcessRecord(ServerSteppablePipelineDriver driver)
                _eventManager?.GenerateEvent(_processSubscriber.SourceIdentifier, this,

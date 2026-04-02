    /// Execution context used for stepping.
    internal sealed class ExecutionContextForStepping : IDisposable
        private readonly ExecutionContext _executionContext;
        private PSInformationalBuffers _originalInformationalBuffers;
        private PSHost _originalHost;
        private ExecutionContextForStepping(ExecutionContext ctxt)
            Dbg.Assert(ctxt != null, "ExecutionContext cannot be null.");
            _executionContext = ctxt;
        internal static ExecutionContextForStepping PrepareExecutionContext(
            ExecutionContext ctxt,
            PSInformationalBuffers newBuffers,
            PSHost newHost)
            ExecutionContextForStepping result = new ExecutionContextForStepping(ctxt);
            result._originalInformationalBuffers
                = ctxt.InternalHost.InternalUI.GetInformationalMessageBuffers();
            result._originalHost = ctxt.InternalHost.ExternalHost;
            ctxt.InternalHost.InternalUI.SetInformationalMessageBuffers(newBuffers);
            ctxt.InternalHost.SetHostRef(newHost);
        // Summary:
        //     Performs application-defined tasks associated with freeing, releasing, or
        //     resetting unmanaged resources.
            _executionContext.InternalHost.InternalUI.SetInformationalMessageBuffers(_originalInformationalBuffers);
            _executionContext.InternalHost.SetHostRef(_originalHost);
    internal class ServerSteppablePipelineDriver
        // that is associated with this
        // powershell driver
        // data structure handler object to handle all
        // communications with the client
        // private bool datasent = false;          // if the remaining data has been sent
        // to the client before sending state
        // information
        // data to client
        // was created
        private readonly ApartmentState apartmentState;  // apartment state for this powershell
        // pipeline that runs the actual command.
        private readonly ServerSteppablePipelineSubscriber _eventSubscriber;
        private readonly PSDataCollection<object> _powershellInput; // input collection of the PowerShell pipeline
        /// Default constructor for creating ServerSteppablePipelineDriver...Used by server to concurrently
        /// run 2 pipelines.
        /// Steppable pipeline event subscriber
        /// <param name="powershellInput">Input collection of the PowerShell pipeline.</param>
        internal ServerSteppablePipelineDriver(PowerShell powershell, bool noInput, Guid clientPowerShellId,
            bool addToHistory, Runspace rsToUse, ServerSteppablePipelineSubscriber eventSubscriber, PSDataCollection<object> powershellInput)
            NoInput = noInput;
            _eventSubscriber = eventSubscriber;
            _powershellInput = powershellInput;
            Input = new PSDataCollection<object>();
            InputEnumerator = Input.GetEnumerator();
            Input.ReleaseOnEnumeration = true;
            DataStructureHandler = runspacePoolDriver.DataStructureHandler.CreatePowerShellDataStructureHandler(clientPowerShellId, clientRunspacePoolId, RemoteStreamOptions, null);
            RemoteHost = DataStructureHandler.GetHostAssociatedWithPowerShell(hostInfo, runspacePoolDriver.ServerRemoteHost);
            // subscribe to various data structure handler events
            DataStructureHandler.InputEndReceived += HandleInputEndReceived;
            DataStructureHandler.InputReceived += HandleInputReceived;
            DataStructureHandler.StopPowerShellReceived += HandleStopReceived;
            DataStructureHandler.OnSessionConnected += HandleSessionConnected;
                throw PSTraceSource.NewInvalidOperationException(RemotingErrorIdStrings.NestedPipelineMissingRunspace);
            // else, set the runspace pool and invoke this powershell
            eventSubscriber.SubscribeEvents(this);
            PipelineState = PSInvocationState.NotStarted;
        internal ServerRemoteHost RemoteHost { get; }
        /// Pipeline invocation state.
        internal PSInvocationState PipelineState { get; private set; }
        /// Checks if the steppable pipeline has input.
        internal bool NoInput { get; }
        /// Steppablepipeline object.
        internal SteppablePipeline SteppablePipeline { get; set; }
        /// Synchronization object.
        /// Processing input.
        internal bool ProcessingInput { get; set; }
        /// Input enumerator.
        internal IEnumerator<object> InputEnumerator { get; }
        /// Input collection.
        internal PSDataCollection<object> Input { get; }
        /// Is the pipeline pulsed.
        internal bool Pulsed { get; set; }
        /// Total objects processed.
        internal int TotalObjectsProcessed { get; set; }
        /// Starts the exectution.
            PipelineState = PSInvocationState.Running;
            _eventSubscriber.FireStartSteppablePipeline(this);
            _powershellInput?.Pulse();
        #region DataStructure related event handling / processing
        internal void HandleInputEndReceived(object sender, EventArgs eventArgs)
            Input.Complete();
            CheckAndPulseForProcessing(true);
            Input?.Complete();
        internal void HandleHostResponseReceived(object sender, RemoteDataEventArgs<RemoteHostResponse> eventArgs)
            RemoteHost.ServerMethodExecutor.HandleRemoteHostResponseFromClient(eventArgs.Data);
                PipelineState = PSInvocationState.Stopping;
            PerformStop();
            Dbg.Assert(!NoInput, "Input data should not be received for powershells created with no input");
            if (Input != null)
                    Input.Add(eventArgs.Data);
                CheckAndPulseForProcessing(false);
        /// Checks if there is any pending input that needs processing. If so, triggers RunProcessRecord
        /// event. The pipeline execution thread catches this and calls us back when the pipeline is
        /// <param name="complete"></param>
        internal void CheckAndPulseForProcessing(bool complete)
            if (complete)
                _eventSubscriber.FireHandleProcessRecord(this);
            else if (!Pulsed)
                bool shouldPulse = false;
                    if (Pulsed)
                    if (!ProcessingInput && ((Input.Count > TotalObjectsProcessed)))
                        shouldPulse = true;
                        Pulsed = true;
                if (shouldPulse && (PipelineState == PSInvocationState.Running))
        /// Performs the stop operation.
            bool shouldPerformStop = false;
                if (!ProcessingInput && (PipelineState == PSInvocationState.Stopping))
                    shouldPerformStop = true;
            if (shouldPerformStop)
                SetState(PSInvocationState.Stopped, new PipelineStoppedException());
        /// Changes state and sends message to the client as needed.
        /// <param name="newState"></param>
        internal void SetState(PSInvocationState newState, Exception reason)
            PSInvocationState copyState = PSInvocationState.NotStarted;
                                    copyState = newState;
                                    // NotStarted -> Running..we dont send
                                    // state back to client.
                                    copyState = PSInvocationState.Stopped;
                PipelineState = copyState;
                    new PSInvocationStateInfo(copyState, reason));
            if (PipelineState == PSInvocationState.Completed
                || PipelineState == PSInvocationState.Stopped
                || PipelineState == PSInvocationState.Failed)
                // Remove itself from the runspace pool

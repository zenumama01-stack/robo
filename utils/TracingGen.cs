        /// Critical level.
        public const byte LevelCritical = 1;
        /// Error level.
        public const byte LevelError = 2;
        /// Warning level.
        public const byte LevelWarning = 3;
        /// Informational level.
        public const byte LevelInformational = 4;
        /// Verbose level.
        public const byte LevelVerbose = 5;
        /// Keyword all.
        public const long KeywordAll = 0xFFFFFFFF;
        private static readonly EventDescriptor WriteTransferEventEvent;
        private static readonly EventDescriptor DebugMessageEvent;
        private static readonly EventDescriptor M3PAbortingWorkflowExecutionEvent;
        private static readonly EventDescriptor M3PActivityExecutionFinishedEvent;
        private static readonly EventDescriptor M3PActivityExecutionQueuedEvent;
        private static readonly EventDescriptor M3PActivityExecutionStartedEvent;
        private static readonly EventDescriptor M3PBeginContainerParentJobExecutionEvent;
        private static readonly EventDescriptor M3PBeginCreateNewJobEvent;
        private static readonly EventDescriptor M3PBeginJobLogicEvent;
        private static readonly EventDescriptor M3PBeginProxyChildJobEventHandlerEvent;
        private static readonly EventDescriptor M3PBeginProxyJobEventHandlerEvent;
        private static readonly EventDescriptor M3PBeginProxyJobExecutionEvent;
        private static readonly EventDescriptor M3PBeginRunGarbageCollectionEvent;
        private static readonly EventDescriptor M3PBeginStartWorkflowApplicationEvent;
        private static readonly EventDescriptor M3PBeginWorkflowExecutionEvent;
        private static readonly EventDescriptor M3PCancellingWorkflowExecutionEvent;
        private static readonly EventDescriptor M3PChildWorkflowJobAdditionEvent;
        private static readonly EventDescriptor M3PEndContainerParentJobExecutionEvent;
        private static readonly EventDescriptor M3PEndCreateNewJobEvent;
        private static readonly EventDescriptor M3PEndJobLogicEvent;
        private static readonly EventDescriptor M3PEndpointDisabledEvent;
        private static readonly EventDescriptor M3PEndpointEnabledEvent;
        private static readonly EventDescriptor M3PEndpointModifiedEvent;
        private static readonly EventDescriptor M3PEndpointRegisteredEvent;
        private static readonly EventDescriptor M3PEndpointUnregisteredEvent;
        private static readonly EventDescriptor M3PEndProxyChildJobEventHandlerEvent;
        private static readonly EventDescriptor M3PEndProxyJobEventHandlerEvent;
        private static readonly EventDescriptor M3PEndProxyJobExecutionEvent;
        private static readonly EventDescriptor M3PEndRunGarbageCollectionEvent;
        private static readonly EventDescriptor M3PEndStartWorkflowApplicationEvent;
        private static readonly EventDescriptor M3PEndWorkflowExecutionEvent;
        private static readonly EventDescriptor M3PErrorImportingWorkflowFromXamlEvent;
        private static readonly EventDescriptor M3PForcedWorkflowShutdownErrorEvent;
        private static readonly EventDescriptor M3PForcedWorkflowShutdownFinishedEvent;
        private static readonly EventDescriptor M3PForcedWorkflowShutdownStartedEvent;
        private static readonly EventDescriptor M3PImportedWorkflowFromXamlEvent;
        private static readonly EventDescriptor M3PImportingWorkflowFromXamlEvent;
        private static readonly EventDescriptor M3PJobCreationCompleteEvent;
        private static readonly EventDescriptor M3PJobErrorEvent;
        private static readonly EventDescriptor M3PJobRemovedEvent;
        private static readonly EventDescriptor M3PJobRemoveErrorEvent;
        private static readonly EventDescriptor M3PJobStateChangedEvent;
        private static readonly EventDescriptor M3PLoadingWorkflowForExecutionEvent;
        private static readonly EventDescriptor M3POutOfProcessRunspaceStartedEvent;
        private static readonly EventDescriptor M3PParameterSplattingWasPerformedEvent;
        private static readonly EventDescriptor M3PParentJobCreatedEvent;
        private static readonly EventDescriptor M3PPersistenceStoreMaxSizeReachedEvent;
        private static readonly EventDescriptor M3PPersistingWorkflowEvent;
        private static readonly EventDescriptor M3PProxyJobRemoteJobAssociationEvent;
        private static readonly EventDescriptor M3PRemoveJobStartedEvent;
        private static readonly EventDescriptor M3PRunspaceAvailabilityChangedEvent;
        private static readonly EventDescriptor M3PRunspaceStateChangedEvent;
        private static readonly EventDescriptor M3PTrackingGuidContainerParentJobCorrelationEvent;
        private static readonly EventDescriptor M3PUnloadingWorkflowEvent;
        private static readonly EventDescriptor M3PWorkflowActivityExecutionFailedEvent;
        private static readonly EventDescriptor M3PWorkflowActivityValidatedEvent;
        private static readonly EventDescriptor M3PWorkflowActivityValidationFailedEvent;
        private static readonly EventDescriptor M3PWorkflowCleanupPerformedEvent;
        private static readonly EventDescriptor M3PWorkflowDeletedFromDiskEvent;
        private static readonly EventDescriptor M3PWorkflowEngineStartedEvent;
        private static readonly EventDescriptor M3PWorkflowExecutionAbortedEvent;
        private static readonly EventDescriptor M3PWorkflowExecutionCancelledEvent;
        private static readonly EventDescriptor M3PWorkflowExecutionErrorEvent;
        private static readonly EventDescriptor M3PWorkflowExecutionFinishedEvent;
        private static readonly EventDescriptor M3PWorkflowExecutionStartedEvent;
        private static readonly EventDescriptor M3PWorkflowJobCreatedEvent;
        private static readonly EventDescriptor M3PWorkflowLoadedForExecutionEvent;
        private static readonly EventDescriptor M3PWorkflowLoadedFromDiskEvent;
        private static readonly EventDescriptor M3PWorkflowManagerCheckpointEvent;
        private static readonly EventDescriptor M3PWorkflowPersistedEvent;
        private static readonly EventDescriptor M3PWorkflowPluginRequestedToShutdownEvent;
        private static readonly EventDescriptor M3PWorkflowPluginRestartedEvent;
        private static readonly EventDescriptor M3PWorkflowPluginStartedEvent;
        private static readonly EventDescriptor M3PWorkflowQuotaViolatedEvent;
        private static readonly EventDescriptor M3PWorkflowResumedEvent;
        private static readonly EventDescriptor M3PWorkflowResumingEvent;
        private static readonly EventDescriptor M3PWorkflowRunspacePoolCreatedEvent;
        private static readonly EventDescriptor M3PWorkflowStateChangedEvent;
        private static readonly EventDescriptor M3PWorkflowUnloadedEvent;
        private static readonly EventDescriptor M3PWorkflowValidationErrorEvent;
        private static readonly EventDescriptor M3PWorkflowValidationFinishedEvent;
        private static readonly EventDescriptor M3PWorkflowValidationStartedEvent;
        static Tracer()
                WriteTransferEventEvent = new EventDescriptor(0x1f05, 0x1, 0x11, 0x5, 0x14, 0x0, (long)0x4000000000000000);
                DebugMessageEvent = new EventDescriptor(0xc000, 0x1, 0x12, 0x4, 0x0, 0x0, (long)0x2000000000000000);
                M3PAbortingWorkflowExecutionEvent = new EventDescriptor(0xb038, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PActivityExecutionFinishedEvent = new EventDescriptor(0xb03f, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PActivityExecutionQueuedEvent = new EventDescriptor(0xb017, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PActivityExecutionStartedEvent = new EventDescriptor(0xb018, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PBeginContainerParentJobExecutionEvent = new EventDescriptor(0xb50c, 0x1, 0x12, 0x4, 0x0, 0x0, (long)0x2000000000000000);
                M3PBeginCreateNewJobEvent = new EventDescriptor(0xb503, 0x1, 0x12, 0x4, 0x0, 0x0, (long)0x2000000000000000);
                M3PBeginJobLogicEvent = new EventDescriptor(0xb506, 0x1, 0x12, 0x4, 0x0, 0x0, (long)0x2000000000000000);
                M3PBeginProxyChildJobEventHandlerEvent = new EventDescriptor(0xb512, 0x1, 0x12, 0x4, 0x0, 0x0, (long)0x2000000000000000);
                M3PBeginProxyJobEventHandlerEvent = new EventDescriptor(0xb510, 0x1, 0x12, 0x4, 0x0, 0x0, (long)0x2000000000000000);
                M3PBeginProxyJobExecutionEvent = new EventDescriptor(0xb50e, 0x1, 0x12, 0x4, 0x0, 0x0, (long)0x2000000000000000);
                M3PBeginRunGarbageCollectionEvent = new EventDescriptor(0xb514, 0x1, 0x12, 0x4, 0x0, 0x0, (long)0x2000000000000000);
                M3PBeginStartWorkflowApplicationEvent = new EventDescriptor(0xb501, 0x1, 0x12, 0x4, 0x0, 0x0, (long)0x2000000000000000);
                M3PBeginWorkflowExecutionEvent = new EventDescriptor(0xb508, 0x1, 0x12, 0x4, 0x0, 0x0, (long)0x2000000000000000);
                M3PCancellingWorkflowExecutionEvent = new EventDescriptor(0xb037, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PChildWorkflowJobAdditionEvent = new EventDescriptor(0xb50a, 0x1, 0x12, 0x4, 0x0, 0x0, (long)0x2000000000000000);
                M3PEndContainerParentJobExecutionEvent = new EventDescriptor(0xb50d, 0x1, 0x12, 0x4, 0x0, 0x0, (long)0x2000000000000000);
                M3PEndCreateNewJobEvent = new EventDescriptor(0xb504, 0x1, 0x12, 0x4, 0x0, 0x0, (long)0x2000000000000000);
                M3PEndJobLogicEvent = new EventDescriptor(0xb507, 0x1, 0x12, 0x4, 0x0, 0x0, (long)0x2000000000000000);
                M3PEndpointDisabledEvent = new EventDescriptor(0xb044, 0x1, 0x11, 0x5, 0x14, 0x9, (long)0x4000000000000200);
                M3PEndpointEnabledEvent = new EventDescriptor(0xb045, 0x1, 0x11, 0x5, 0x14, 0x9, (long)0x4000000000000200);
                M3PEndpointModifiedEvent = new EventDescriptor(0xb042, 0x1, 0x11, 0x5, 0x14, 0x9, (long)0x4000000000000200);
                M3PEndpointRegisteredEvent = new EventDescriptor(0xb041, 0x1, 0x11, 0x5, 0x14, 0x9, (long)0x4000000000000200);
                M3PEndpointUnregisteredEvent = new EventDescriptor(0xb043, 0x1, 0x11, 0x5, 0x14, 0x9, (long)0x4000000000000200);
                M3PEndProxyChildJobEventHandlerEvent = new EventDescriptor(0xb513, 0x1, 0x12, 0x4, 0x0, 0x0, (long)0x2000000000000000);
                M3PEndProxyJobEventHandlerEvent = new EventDescriptor(0xb511, 0x1, 0x12, 0x4, 0x0, 0x0, (long)0x2000000000000000);
                M3PEndProxyJobExecutionEvent = new EventDescriptor(0xb50f, 0x1, 0x12, 0x4, 0x0, 0x0, (long)0x2000000000000000);
                M3PEndRunGarbageCollectionEvent = new EventDescriptor(0xb515, 0x1, 0x12, 0x4, 0x0, 0x0, (long)0x2000000000000000);
                M3PEndStartWorkflowApplicationEvent = new EventDescriptor(0xb502, 0x1, 0x12, 0x4, 0x0, 0x0, (long)0x2000000000000000);
                M3PEndWorkflowExecutionEvent = new EventDescriptor(0xb509, 0x1, 0x12, 0x4, 0x0, 0x0, (long)0x2000000000000000);
                M3PErrorImportingWorkflowFromXamlEvent = new EventDescriptor(0xb01b, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PForcedWorkflowShutdownErrorEvent = new EventDescriptor(0xb03c, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PForcedWorkflowShutdownFinishedEvent = new EventDescriptor(0xb03b, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PForcedWorkflowShutdownStartedEvent = new EventDescriptor(0xb03a, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PImportedWorkflowFromXamlEvent = new EventDescriptor(0xb01a, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PImportingWorkflowFromXamlEvent = new EventDescriptor(0xb019, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PJobCreationCompleteEvent = new EventDescriptor(0xb032, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PJobErrorEvent = new EventDescriptor(0xb02e, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PJobRemovedEvent = new EventDescriptor(0xb033, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PJobRemoveErrorEvent = new EventDescriptor(0xb034, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PJobStateChangedEvent = new EventDescriptor(0xb02d, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PLoadingWorkflowForExecutionEvent = new EventDescriptor(0xb035, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3POutOfProcessRunspaceStartedEvent = new EventDescriptor(0xb046, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PParameterSplattingWasPerformedEvent = new EventDescriptor(0xb047, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PParentJobCreatedEvent = new EventDescriptor(0xb031, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PPersistenceStoreMaxSizeReachedEvent = new EventDescriptor(0xb516, 0x1, 0x10, 0x3, 0x0, 0x0, (long)0x8000000000000000);
                M3PPersistingWorkflowEvent = new EventDescriptor(0xb03d, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PProxyJobRemoteJobAssociationEvent = new EventDescriptor(0xb50b, 0x1, 0x12, 0x4, 0x0, 0x0, (long)0x2000000000000000);
                M3PRemoveJobStartedEvent = new EventDescriptor(0xb02c, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PRunspaceAvailabilityChangedEvent = new EventDescriptor(0xb022, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PRunspaceStateChangedEvent = new EventDescriptor(0xb023, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PTrackingGuidContainerParentJobCorrelationEvent = new EventDescriptor(0xb505, 0x1, 0x12, 0x4, 0x0, 0x0, (long)0x2000000000000000);
                M3PUnloadingWorkflowEvent = new EventDescriptor(0xb039, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PWorkflowActivityExecutionFailedEvent = new EventDescriptor(0xb021, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PWorkflowActivityValidatedEvent = new EventDescriptor(0xb01f, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PWorkflowActivityValidationFailedEvent = new EventDescriptor(0xb020, 0x1, 0x11, 0x5, 0x14, 0x8, (long)0x4000000000000200);
                M3PWorkflowCleanupPerformedEvent = new EventDescriptor(0xb028, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PWorkflowDeletedFromDiskEvent = new EventDescriptor(0xb02a, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PWorkflowEngineStartedEvent = new EventDescriptor(0xb048, 0x1, 0x11, 0x5, 0x14, 0x5, (long)0x4000000000000200);
                M3PWorkflowExecutionAbortedEvent = new EventDescriptor(0xb027, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PWorkflowExecutionCancelledEvent = new EventDescriptor(0xb026, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PWorkflowExecutionErrorEvent = new EventDescriptor(0xb040, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PWorkflowExecutionFinishedEvent = new EventDescriptor(0xb036, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PWorkflowExecutionStartedEvent = new EventDescriptor(0xb008, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PWorkflowJobCreatedEvent = new EventDescriptor(0xb030, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PWorkflowLoadedForExecutionEvent = new EventDescriptor(0xb024, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PWorkflowLoadedFromDiskEvent = new EventDescriptor(0xb029, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PWorkflowManagerCheckpointEvent = new EventDescriptor(0xb049, 0x1, 0x12, 0x4, 0x0, 0x0, (long)0x2000000000000200);
                M3PWorkflowPersistedEvent = new EventDescriptor(0xb03e, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PWorkflowPluginRequestedToShutdownEvent = new EventDescriptor(0xb010, 0x1, 0x11, 0x5, 0x14, 0x5, (long)0x4000000000000200);
                M3PWorkflowPluginRestartedEvent = new EventDescriptor(0xb011, 0x1, 0x11, 0x5, 0x14, 0x5, (long)0x4000000000000200);
                M3PWorkflowPluginStartedEvent = new EventDescriptor(0xb007, 0x1, 0x11, 0x5, 0x14, 0x5, (long)0x4000000000000200);
                M3PWorkflowQuotaViolatedEvent = new EventDescriptor(0xb013, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PWorkflowResumedEvent = new EventDescriptor(0xb014, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PWorkflowResumingEvent = new EventDescriptor(0xb012, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PWorkflowRunspacePoolCreatedEvent = new EventDescriptor(0xb016, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PWorkflowStateChangedEvent = new EventDescriptor(0xb009, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PWorkflowUnloadedEvent = new EventDescriptor(0xb025, 0x1, 0x11, 0x5, 0x14, 0x6, (long)0x4000000000000200);
                M3PWorkflowValidationErrorEvent = new EventDescriptor(0xb01e, 0x1, 0x11, 0x5, 0x14, 0x8, (long)0x4000000000000200);
                M3PWorkflowValidationFinishedEvent = new EventDescriptor(0xb01d, 0x1, 0x11, 0x5, 0x14, 0x8, (long)0x4000000000000200);
                M3PWorkflowValidationStartedEvent = new EventDescriptor(0xb01c, 0x1, 0x11, 0x5, 0x14, 0x8, (long)0x4000000000000200);
        public Tracer() : base() { }
        /// Transfer Event.
        protected override EventDescriptor TransferEvent
                return WriteTransferEventEvent;
        /// WriteTransferEvent (EventId: 0x1f05/7941).
        [EtwEvent(0x1f05)]
        public void WriteTransferEvent(Guid currentActivityId, Guid parentActivityId)
            WriteEvent(WriteTransferEventEvent, currentActivityId, parentActivityId);
        /// DebugMessage (EventId: 0xc000/49152).
        public void DebugMessage(string message)
            WriteEvent(DebugMessageEvent, message);
        /// AbortingWorkflowExecution (EventId: 0xb038/45112).
        [EtwEvent(0xb038)]
        public void AbortingWorkflowExecution(Guid workflowId, string reason)
            WriteEvent(M3PAbortingWorkflowExecutionEvent, workflowId, reason);
        /// ActivityExecutionFinished (EventId: 0xb03f/45119).
        [EtwEvent(0xb03f)]
        public void ActivityExecutionFinished(string activityName)
            WriteEvent(M3PActivityExecutionFinishedEvent, activityName);
        /// ActivityExecutionQueued (EventId: 0xb017/45079).
        [EtwEvent(0xb017)]
        public void ActivityExecutionQueued(Guid workflowId, string activityName)
            WriteEvent(M3PActivityExecutionQueuedEvent, workflowId, activityName);
        /// ActivityExecutionStarted (EventId: 0xb018/45080).
        [EtwEvent(0xb018)]
        public void ActivityExecutionStarted(string activityName, string activityTypeName)
            WriteEvent(M3PActivityExecutionStartedEvent, activityName, activityTypeName);
        /// BeginContainerParentJobExecution (EventId: 0xb50c/46348).
        [EtwEvent(0xb50c)]
            WriteEvent(M3PBeginContainerParentJobExecutionEvent, containerParentJobInstanceId);
        /// BeginCreateNewJob (EventId: 0xb503/46339).
        [EtwEvent(0xb503)]
        public void BeginCreateNewJob(Guid trackingId)
            WriteEvent(M3PBeginCreateNewJobEvent, trackingId);
        /// BeginJobLogic (EventId: 0xb506/46342).
        [EtwEvent(0xb506)]
        public void BeginJobLogic(Guid workflowJobJobInstanceId)
            WriteEvent(M3PBeginJobLogicEvent, workflowJobJobInstanceId);
        /// BeginProxyChildJobEventHandler (EventId: 0xb512/46354).
        [EtwEvent(0xb512)]
            WriteEvent(M3PBeginProxyChildJobEventHandlerEvent, proxyChildJobInstanceId);
        /// BeginProxyJobEventHandler (EventId: 0xb510/46352).
        [EtwEvent(0xb510)]
            WriteEvent(M3PBeginProxyJobEventHandlerEvent, proxyJobInstanceId);
        /// BeginProxyJobExecution (EventId: 0xb50e/46350).
        [EtwEvent(0xb50e)]
            WriteEvent(M3PBeginProxyJobExecutionEvent, proxyJobInstanceId);
        /// BeginRunGarbageCollection (EventId: 0xb514/46356).
        [EtwEvent(0xb514)]
        public void BeginRunGarbageCollection()
            WriteEvent(M3PBeginRunGarbageCollectionEvent);
        /// BeginStartWorkflowApplication (EventId: 0xb501/46337).
        [EtwEvent(0xb501)]
        public void BeginStartWorkflowApplication(Guid trackingId)
            WriteEvent(M3PBeginStartWorkflowApplicationEvent, trackingId);
        /// BeginWorkflowExecution (EventId: 0xb508/46344).
        [EtwEvent(0xb508)]
        public void BeginWorkflowExecution(Guid workflowJobJobInstanceId)
            WriteEvent(M3PBeginWorkflowExecutionEvent, workflowJobJobInstanceId);
        /// CancellingWorkflowExecution (EventId: 0xb037/45111).
        [EtwEvent(0xb037)]
        public void CancellingWorkflowExecution(Guid workflowId)
            WriteEvent(M3PCancellingWorkflowExecutionEvent, workflowId);
        /// ChildWorkflowJobAddition (EventId: 0xb50a/46346).
        [EtwEvent(0xb50a)]
        public void ChildWorkflowJobAddition(Guid workflowJobInstanceId, Guid containerParentJobInstanceId)
            WriteEvent(M3PChildWorkflowJobAdditionEvent, workflowJobInstanceId, containerParentJobInstanceId);
        /// EndContainerParentJobExecution (EventId: 0xb50d/46349).
        [EtwEvent(0xb50d)]
            WriteEvent(M3PEndContainerParentJobExecutionEvent, containerParentJobInstanceId);
        /// EndCreateNewJob (EventId: 0xb504/46340).
        [EtwEvent(0xb504)]
        public void EndCreateNewJob(Guid trackingId)
            WriteEvent(M3PEndCreateNewJobEvent, trackingId);
        /// EndJobLogic (EventId: 0xb507/46343).
        [EtwEvent(0xb507)]
        public void EndJobLogic(Guid workflowJobJobInstanceId)
            WriteEvent(M3PEndJobLogicEvent, workflowJobJobInstanceId);
        /// EndpointDisabled (EventId: 0xb044/45124).
        [EtwEvent(0xb044)]
            WriteEvent(M3PEndpointDisabledEvent, endpointName, disabledBy);
        /// EndpointEnabled (EventId: 0xb045/45125).
        [EtwEvent(0xb045)]
            WriteEvent(M3PEndpointEnabledEvent, endpointName, enabledBy);
        /// EndpointModified (EventId: 0xb042/45122).
        [EtwEvent(0xb042)]
            WriteEvent(M3PEndpointModifiedEvent, endpointName, modifiedBy);
        /// EndpointRegistered (EventId: 0xb041/45121).
        [EtwEvent(0xb041)]
        public void EndpointRegistered(string endpointName, string registeredBy)
            WriteEvent(M3PEndpointRegisteredEvent, endpointName, registeredBy);
        /// EndpointUnregistered (EventId: 0xb043/45123).
        [EtwEvent(0xb043)]
            WriteEvent(M3PEndpointUnregisteredEvent, endpointName, unregisteredBy);
        /// EndProxyChildJobEventHandler (EventId: 0xb513/46355).
        [EtwEvent(0xb513)]
        public void EndProxyChildJobEventHandler(Guid proxyChildJobInstanceId)
            WriteEvent(M3PEndProxyChildJobEventHandlerEvent, proxyChildJobInstanceId);
        /// EndProxyJobEventHandler (EventId: 0xb511/46353).
        [EtwEvent(0xb511)]
            WriteEvent(M3PEndProxyJobEventHandlerEvent, proxyJobInstanceId);
        /// EndProxyJobExecution (EventId: 0xb50f/46351).
        [EtwEvent(0xb50f)]
            WriteEvent(M3PEndProxyJobExecutionEvent, proxyJobInstanceId);
        /// EndRunGarbageCollection (EventId: 0xb515/46357).
        [EtwEvent(0xb515)]
        public void EndRunGarbageCollection()
            WriteEvent(M3PEndRunGarbageCollectionEvent);
        /// EndStartWorkflowApplication (EventId: 0xb502/46338).
        [EtwEvent(0xb502)]
        public void EndStartWorkflowApplication(Guid trackingId)
            WriteEvent(M3PEndStartWorkflowApplicationEvent, trackingId);
        /// EndWorkflowExecution (EventId: 0xb509/46345).
        [EtwEvent(0xb509)]
        public void EndWorkflowExecution(Guid workflowJobJobInstanceId)
            WriteEvent(M3PEndWorkflowExecutionEvent, workflowJobJobInstanceId);
        /// ErrorImportingWorkflowFromXaml (EventId: 0xb01b/45083).
        [EtwEvent(0xb01b)]
        public void ErrorImportingWorkflowFromXaml(Guid workflowId, string errorDescription)
            WriteEvent(M3PErrorImportingWorkflowFromXamlEvent, workflowId, errorDescription);
        /// ForcedWorkflowShutdownError (EventId: 0xb03c/45116).
        [EtwEvent(0xb03c)]
        public void ForcedWorkflowShutdownError(Guid workflowId, string errorDescription)
            WriteEvent(M3PForcedWorkflowShutdownErrorEvent, workflowId, errorDescription);
        /// ForcedWorkflowShutdownFinished (EventId: 0xb03b/45115).
        [EtwEvent(0xb03b)]
        public void ForcedWorkflowShutdownFinished(Guid workflowId)
            WriteEvent(M3PForcedWorkflowShutdownFinishedEvent, workflowId);
        /// ForcedWorkflowShutdownStarted (EventId: 0xb03a/45114).
        [EtwEvent(0xb03a)]
        public void ForcedWorkflowShutdownStarted(Guid workflowId)
            WriteEvent(M3PForcedWorkflowShutdownStartedEvent, workflowId);
        /// ImportedWorkflowFromXaml (EventId: 0xb01a/45082).
        [EtwEvent(0xb01a)]
        public void ImportedWorkflowFromXaml(Guid workflowId, string xamlFile)
            WriteEvent(M3PImportedWorkflowFromXamlEvent, workflowId, xamlFile);
        /// ImportingWorkflowFromXaml (EventId: 0xb019/45081).
        [EtwEvent(0xb019)]
        public void ImportingWorkflowFromXaml(Guid workflowId, string xamlFile)
            WriteEvent(M3PImportingWorkflowFromXamlEvent, workflowId, xamlFile);
        /// JobCreationComplete (EventId: 0xb032/45106).
        [EtwEvent(0xb032)]
        public void JobCreationComplete(Guid jobId, Guid workflowId)
            WriteEvent(M3PJobCreationCompleteEvent, jobId, workflowId);
        /// JobError (EventId: 0xb02e/45102).
        [EtwEvent(0xb02e)]
        public void JobError(int jobId, Guid workflowId, string errorDescription)
            WriteEvent(M3PJobErrorEvent, jobId, workflowId, errorDescription);
        /// JobRemoved (EventId: 0xb033/45107).
        [EtwEvent(0xb033)]
        public void JobRemoved(Guid parentJobId, Guid childJobId, Guid workflowId)
            WriteEvent(M3PJobRemovedEvent, parentJobId, childJobId, workflowId);
        /// JobRemoveError (EventId: 0xb034/45108).
        [EtwEvent(0xb034)]
        public void JobRemoveError(Guid parentJobId, Guid childJobId, Guid workflowId, string error)
            WriteEvent(M3PJobRemoveErrorEvent, parentJobId, childJobId, workflowId, error);
        /// JobStateChanged (EventId: 0xb02d/45101).
        [EtwEvent(0xb02d)]
        public void JobStateChanged(int jobId, Guid workflowId, string newState, string oldState)
            WriteEvent(M3PJobStateChangedEvent, jobId, workflowId, newState, oldState);
        /// LoadingWorkflowForExecution (EventId: 0xb035/45109).
        [EtwEvent(0xb035)]
        public void LoadingWorkflowForExecution(Guid workflowId)
            WriteEvent(M3PLoadingWorkflowForExecutionEvent, workflowId);
        /// OutOfProcessRunspaceStarted (EventId: 0xb046/45126).
        [EtwEvent(0xb046)]
        public void OutOfProcessRunspaceStarted(string command)
            WriteEvent(M3POutOfProcessRunspaceStartedEvent, command);
        /// ParameterSplattingWasPerformed (EventId: 0xb047/45127).
        [EtwEvent(0xb047)]
        public void ParameterSplattingWasPerformed(string parameters, string computers)
            WriteEvent(M3PParameterSplattingWasPerformedEvent, parameters, computers);
        /// ParentJobCreated (EventId: 0xb031/45105).
        [EtwEvent(0xb031)]
        public void ParentJobCreated(Guid jobId)
            WriteEvent(M3PParentJobCreatedEvent, jobId);
        /// PersistenceStoreMaxSizeReached (EventId: 0xb516/46358).
        [EtwEvent(0xb516)]
        public void PersistenceStoreMaxSizeReached()
            WriteEvent(M3PPersistenceStoreMaxSizeReachedEvent);
        /// PersistingWorkflow (EventId: 0xb03d/45117).
        [EtwEvent(0xb03d)]
        public void PersistingWorkflow(Guid workflowId, string persistPath)
            WriteEvent(M3PPersistingWorkflowEvent, workflowId, persistPath);
        /// ProxyJobRemoteJobAssociation (EventId: 0xb50b/46347).
        [EtwEvent(0xb50b)]
            WriteEvent(M3PProxyJobRemoteJobAssociationEvent, proxyJobInstanceId, containerParentJobInstanceId);
        /// RemoveJobStarted (EventId: 0xb02c/45100).
        [EtwEvent(0xb02c)]
        public void RemoveJobStarted(Guid jobId)
            WriteEvent(M3PRemoveJobStartedEvent, jobId);
        /// RunspaceAvailabilityChanged (EventId: 0xb022/45090).
        [EtwEvent(0xb022)]
        public void RunspaceAvailabilityChanged(string runspaceId, string availability)
            WriteEvent(M3PRunspaceAvailabilityChangedEvent, runspaceId, availability);
        /// RunspaceStateChanged (EventId: 0xb023/45091).
        [EtwEvent(0xb023)]
        public void RunspaceStateChanged(string runspaceId, string newState, string oldState)
            WriteEvent(M3PRunspaceStateChangedEvent, runspaceId, newState, oldState);
        /// TrackingGuidContainerParentJobCorrelation (EventId: 0xb505/46341).
        [EtwEvent(0xb505)]
        public void TrackingGuidContainerParentJobCorrelation(Guid trackingId, Guid containerParentJobInstanceId)
            WriteEvent(M3PTrackingGuidContainerParentJobCorrelationEvent, trackingId, containerParentJobInstanceId);
        /// UnloadingWorkflow (EventId: 0xb039/45113).
        [EtwEvent(0xb039)]
        public void UnloadingWorkflow(Guid workflowId)
            WriteEvent(M3PUnloadingWorkflowEvent, workflowId);
        /// WorkflowActivityExecutionFailed (EventId: 0xb021/45089).
        [EtwEvent(0xb021)]
        public void WorkflowActivityExecutionFailed(Guid workflowId, string activityName, string failureDescription)
            WriteEvent(M3PWorkflowActivityExecutionFailedEvent, workflowId, activityName, failureDescription);
        /// WorkflowActivityValidated (EventId: 0xb01f/45087).
        [EtwEvent(0xb01f)]
        public void WorkflowActivityValidated(Guid workflowId, string activityDisplayName, string activityType)
            WriteEvent(M3PWorkflowActivityValidatedEvent, workflowId, activityDisplayName, activityType);
        /// WorkflowActivityValidationFailed (EventId: 0xb020/45088).
        [EtwEvent(0xb020)]
        public void WorkflowActivityValidationFailed(Guid workflowId, string activityDisplayName, string activityType)
            WriteEvent(M3PWorkflowActivityValidationFailedEvent, workflowId, activityDisplayName, activityType);
        /// WorkflowCleanupPerformed (EventId: 0xb028/45096).
        [EtwEvent(0xb028)]
        public void WorkflowCleanupPerformed(Guid workflowId)
            WriteEvent(M3PWorkflowCleanupPerformedEvent, workflowId);
        /// WorkflowDeletedFromDisk (EventId: 0xb02a/45098).
        [EtwEvent(0xb02a)]
        public void WorkflowDeletedFromDisk(Guid workflowId, string path)
            WriteEvent(M3PWorkflowDeletedFromDiskEvent, workflowId, path);
        /// WorkflowEngineStarted (EventId: 0xb048/45128).
        [EtwEvent(0xb048)]
        public void WorkflowEngineStarted(string endpointName)
            WriteEvent(M3PWorkflowEngineStartedEvent, endpointName);
        /// WorkflowExecutionAborted (EventId: 0xb027/45095).
        [EtwEvent(0xb027)]
        public void WorkflowExecutionAborted(Guid workflowId)
            WriteEvent(M3PWorkflowExecutionAbortedEvent, workflowId);
        /// WorkflowExecutionCancelled (EventId: 0xb026/45094).
        [EtwEvent(0xb026)]
        public void WorkflowExecutionCancelled(Guid workflowId)
            WriteEvent(M3PWorkflowExecutionCancelledEvent, workflowId);
        /// WorkflowExecutionError (EventId: 0xb040/45120).
        [EtwEvent(0xb040)]
        public void WorkflowExecutionError(Guid workflowId, string errorDescription)
            WriteEvent(M3PWorkflowExecutionErrorEvent, workflowId, errorDescription);
        /// WorkflowExecutionFinished (EventId: 0xb036/45110).
        [EtwEvent(0xb036)]
        public void WorkflowExecutionFinished(Guid workflowId)
            WriteEvent(M3PWorkflowExecutionFinishedEvent, workflowId);
        /// WorkflowExecutionStarted (EventId: 0xb008/45064).
        [EtwEvent(0xb008)]
        public void WorkflowExecutionStarted(Guid workflowId, string managedNodes)
            WriteEvent(M3PWorkflowExecutionStartedEvent, workflowId, managedNodes);
        /// WorkflowJobCreated (EventId: 0xb030/45104).
        [EtwEvent(0xb030)]
        public void WorkflowJobCreated(Guid parentJobId, Guid childJobId, Guid childWorkflowId)
            WriteEvent(M3PWorkflowJobCreatedEvent, parentJobId, childJobId, childWorkflowId);
        /// WorkflowLoadedForExecution (EventId: 0xb024/45092).
        [EtwEvent(0xb024)]
        public void WorkflowLoadedForExecution(Guid workflowId)
            WriteEvent(M3PWorkflowLoadedForExecutionEvent, workflowId);
        /// WorkflowLoadedFromDisk (EventId: 0xb029/45097).
        [EtwEvent(0xb029)]
        public void WorkflowLoadedFromDisk(Guid workflowId, string path)
            WriteEvent(M3PWorkflowLoadedFromDiskEvent, workflowId, path);
        /// WorkflowManagerCheckpoint (EventId: 0xb049/45129).
        [EtwEvent(0xb049)]
        public void WorkflowManagerCheckpoint(string checkpointPath, string configProviderId, string userName, string path)
            WriteEvent(M3PWorkflowManagerCheckpointEvent, checkpointPath, configProviderId, userName, path);
        /// WorkflowPersisted (EventId: 0xb03e/45118).
        [EtwEvent(0xb03e)]
        public void WorkflowPersisted(Guid workflowId)
            WriteEvent(M3PWorkflowPersistedEvent, workflowId);
        /// WorkflowPluginRequestedToShutdown (EventId: 0xb010/45072).
        [EtwEvent(0xb010)]
        public void WorkflowPluginRequestedToShutdown(string endpointName)
            WriteEvent(M3PWorkflowPluginRequestedToShutdownEvent, endpointName);
        /// WorkflowPluginRestarted (EventId: 0xb011/45073).
        [EtwEvent(0xb011)]
        public void WorkflowPluginRestarted(string endpointName)
            WriteEvent(M3PWorkflowPluginRestartedEvent, endpointName);
        /// WorkflowPluginStarted (EventId: 0xb007/45063).
        [EtwEvent(0xb007)]
        public void WorkflowPluginStarted(string endpointName, string user, string hostingMode, string protocol, string configuration)
            WriteEvent(M3PWorkflowPluginStartedEvent, endpointName, user, hostingMode, protocol, configuration);
        /// WorkflowQuotaViolated (EventId: 0xb013/45075).
        [EtwEvent(0xb013)]
        public void WorkflowQuotaViolated(string endpointName, string configName, string allowedValue, string valueInQuestion)
            WriteEvent(M3PWorkflowQuotaViolatedEvent, endpointName, configName, allowedValue, valueInQuestion);
        /// WorkflowResumed (EventId: 0xb014/45076).
        [EtwEvent(0xb014)]
        public void WorkflowResumed(Guid workflowId)
            WriteEvent(M3PWorkflowResumedEvent, workflowId);
        /// WorkflowResuming (EventId: 0xb012/45074).
        [EtwEvent(0xb012)]
        public void WorkflowResuming(Guid workflowId)
            WriteEvent(M3PWorkflowResumingEvent, workflowId);
        /// WorkflowRunspacePoolCreated (EventId: 0xb016/45078).
        [EtwEvent(0xb016)]
        public void WorkflowRunspacePoolCreated(Guid workflowId, string managedNode)
            WriteEvent(M3PWorkflowRunspacePoolCreatedEvent, workflowId, managedNode);
        /// WorkflowStateChanged (EventId: 0xb009/45065).
        [EtwEvent(0xb009)]
        public void WorkflowStateChanged(Guid workflowId, string newState, string oldState)
            WriteEvent(M3PWorkflowStateChangedEvent, workflowId, newState, oldState);
        /// WorkflowUnloaded (EventId: 0xb025/45093).
        [EtwEvent(0xb025)]
        public void WorkflowUnloaded(Guid workflowId)
            WriteEvent(M3PWorkflowUnloadedEvent, workflowId);
        /// WorkflowValidationError (EventId: 0xb01e/45086).
        [EtwEvent(0xb01e)]
        public void WorkflowValidationError(Guid workflowId)
            WriteEvent(M3PWorkflowValidationErrorEvent, workflowId);
        /// WorkflowValidationFinished (EventId: 0xb01d/45085).
        [EtwEvent(0xb01d)]
        public void WorkflowValidationFinished(Guid workflowId)
            WriteEvent(M3PWorkflowValidationFinishedEvent, workflowId);
        /// WorkflowValidationStarted (EventId: 0xb01c/45084).
        [EtwEvent(0xb01c)]
        public void WorkflowValidationStarted(Guid workflowId)
            WriteEvent(M3PWorkflowValidationStartedEvent, workflowId);
// This code was generated on 02/01/2012 19:52:32

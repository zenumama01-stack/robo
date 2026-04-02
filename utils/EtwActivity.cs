    /// Attribute to represent an EtwEvent.
    [AttributeUsage(AttributeTargets.Method)]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public sealed class EtwEvent : Attribute
        public EtwEvent(long eventId)
            this.EventId = eventId;
        /// EventId.
        public long EventId { get; }
    /// Delegates that defines a call back with no parameter.
    public delegate void CallbackNoParameter();
    /// Delegates that defines a call back with one parameter (state)
    public delegate void CallbackWithState(object state);
    /// Delegates that defines a call back with two parameters; state and ElapsedEventArgs.
    /// It will be used in System.Timers.Timer scenarios.
    public delegate void CallbackWithStateAndArgs(object state, System.Timers.ElapsedEventArgs args);
    /// ETW events argument class.
    public class EtwEventArgs : EventArgs
        /// <summary> Gets Event descriptor </summary>
        public EventDescriptor Descriptor
        /// <summary> Gets whether the event is successfully written </summary>
        public bool Success { get; }
        /// <summary> Gets payload in the event </summary>
        public object[] Payload { get; }
        /// Creates a new instance of EtwEventArgs class.
        /// <param name="descriptor">Event descriptor.</param>
        /// <param name="success">Indicate whether the event is successfully written.</param>
        /// <param name="payload">Event payload.</param>
        public EtwEventArgs(EventDescriptor descriptor, bool success, object[] payload)
            this.Descriptor = descriptor;
            this.Payload = payload;
            this.Success = success;
    /// This the abstract base class of all activity classes that represent an end-to-end scenario.
        /// This is a helper class that is used to wrap many multi-threading scenarios
        /// and makes correlation event to be logged easily.
        private sealed class CorrelatedCallback
            private readonly CallbackNoParameter callbackNoParam;
            private readonly CallbackWithState callbackWithState;
            private readonly AsyncCallback asyncCallback;
            /// ParentActivityId.
            private readonly Guid parentActivityId;
            private readonly EtwActivity tracer;
            /// EtwCorrelator Constructor.
            /// <param name="tracer"></param>
            public CorrelatedCallback(EtwActivity tracer, CallbackNoParameter callback)
                ArgumentNullException.ThrowIfNull(callback);
                ArgumentNullException.ThrowIfNull(tracer);
                this.tracer = tracer;
                this.parentActivityId = EtwActivity.GetActivityId();
                this.callbackNoParam = callback;
            public CorrelatedCallback(EtwActivity tracer, CallbackWithState callback)
                this.callbackWithState = callback;
            public CorrelatedCallback(EtwActivity tracer, AsyncCallback callback)
                this.asyncCallback = callback;
            /// It is to be used in System.Timers.Timer scenarios.
            private readonly CallbackWithStateAndArgs callbackWithStateAndArgs;
            public CorrelatedCallback(EtwActivity tracer, CallbackWithStateAndArgs callback)
                this.callbackWithStateAndArgs = callback;
            /// This is the wrapper on the actual callback.
            public void Callback(object state, System.Timers.ElapsedEventArgs args)
                Debug.Assert(callbackWithStateAndArgs != null, "callback is NULL.  There MUST always ba a valid callback!");
                Correlate();
                this.callbackWithStateAndArgs(state, args);
            /// Correlate.
            private void Correlate()
                tracer.CorrelateWithActivity(this.parentActivityId);
            public void Callback()
                Debug.Assert(callbackNoParam != null, "callback is NULL.  There MUST always ba a valid callback");
                this.callbackNoParam();
            public void Callback(object state)
                Debug.Assert(callbackWithState != null, "callback is NULL.  There MUST always ba a valid callback!");
                this.callbackWithState(state);
            public void Callback(IAsyncResult asyncResult)
                Debug.Assert(asyncCallback != null, "callback is NULL.  There MUST always ba a valid callback!");
                this.asyncCallback(asyncResult);
        private static readonly Dictionary<Guid, EventProvider> providers = new Dictionary<Guid, EventProvider>();
        private static readonly object syncLock = new object();
        private static readonly EventDescriptor _WriteTransferEvent = new EventDescriptor(0x1f05, 0x1, 0x11, 0x5, 0x14, 0x0, (long)0x4000000000000000);
        private EventProvider currentProvider;
        /// Event handler for the class.
        public static event EventHandler<EtwEventArgs> EventWritten;
        /// Sets the activityId provided in the current thread.
        /// If current thread already has the same activityId it does
        /// nothing.
        /// <returns>True when provided activity was set, false if current activity
        /// was found to be same and set was not needed.</returns>
            if (GetActivityId() != activityId)
                EventProvider.SetActivityId(ref activityId);
        /// Creates a new ActivityId that can be used to set in the thread's context.
            return EventProvider.CreateActivityId();
        /// Returns the ActivityId set in current thread.
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
            Guid activityId = Guid.Empty;
            Interop.Windows.GetEventActivityIdControl(ref activityId);
            return activityId;
        protected EtwActivity()
        /// CorrelateWithActivity (EventId: 0x1f05/7941)
        /// This method also sets a new activity id in current thread.
        /// And then correlates the new id with parentActivityId.
        public void CorrelateWithActivity(Guid parentActivityId)
            EventProvider provider = GetProvider();
            if (!provider.IsEnabled())
            Guid activityId = CreateActivityId();
            SetActivityId(activityId);
            if (parentActivityId != Guid.Empty)
                EventDescriptor transferEvent = TransferEvent;
                provider.WriteTransferEvent(in transferEvent, parentActivityId, activityId, parentActivityId);
        /// IsEnabled.
        public bool IsEnabled
                return GetProvider().IsEnabled();
        /// Checks whether a provider matching certain levels and keyword is enabled.
        /// <param name="levels">Levels to check.</param>
        /// <param name="keywords">Keywords to check.</param>
        /// <returns>True, if any ETW listener is enabled else false.</returns>
        public bool IsProviderEnabled(byte levels, long keywords)
            return GetProvider().IsEnabled(levels, keywords);
        /// Correlates parent activity id set in the thread with a new activity id
        /// If parent activity id is not, it just sets a new activity in the current thread. And does not write the Transfer event.
        public void Correlate()
            Guid parentActivity = GetActivityId();
            CorrelateWithActivity(parentActivity);
        /// Wraps a callback with no params.
        public CallbackNoParameter Correlate(CallbackNoParameter callback)
            return new CorrelatedCallback(this, callback).Callback;
        /// Wraps a callback with one object param.
        public CallbackWithState Correlate(CallbackWithState callback)
        /// Wraps a AsyncCallback with IAsyncResult param.
        public AsyncCallback Correlate(AsyncCallback callback)
        /// Wraps a callback with one object param and one ElapsedEventArgs object
        /// This is menat to be used in System.Timers.Timer scenarios.
        public CallbackWithStateAndArgs Correlate(CallbackWithStateAndArgs callback)
        /// The provider where the tracing messages will be written to.
        protected virtual Guid ProviderId
                return PSEtwLogProvider.ProviderGuid;
        /// The event that is defined to be used to log transfer event.
        /// The derived class must override this property if they don't
        /// want to use the PowerShell's transfer event.
        protected virtual EventDescriptor TransferEvent
                return _WriteTransferEvent;
        /// This is the main method that write the messages to the trace.
        /// All derived classes must use this method to write to the provider log.
        /// <param name="ed">EventDescriptor.</param>
        /// <param name="payload">Payload.</param>
        protected void WriteEvent(EventDescriptor ed, params object[] payload)
            if (payload != null)
                for (int i = 0; i < payload.Length; i++)
                    if (payload[i] == null)
                        payload[i] = string.Empty;
            bool success = provider.WriteEvent(in ed, payload);
            EventWritten?.Invoke(this, new EtwEventArgs(ed, success, payload));
        private EventProvider GetProvider()
            if (currentProvider != null)
                return currentProvider;
            lock (syncLock)
                if (!providers.TryGetValue(ProviderId, out currentProvider))
                    currentProvider = new EventProvider(ProviderId);
                    providers[ProviderId] = currentProvider;

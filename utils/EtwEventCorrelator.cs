    ///     An object that can be used to manage the ETW activity ID of the current thread.
    public interface IEtwEventCorrelator
        ///     Gets or sets the ETW activity ID of the current thread.
        ///     <para>This method should only be used for advanced scenarios
        ///         or diagnostics.  Prefer using <see cref="StartActivity()"/>
        ///         or <see cref="StartActivity(Guid)"/> instead.</para>
        Guid CurrentActivityId { get; set; }
        ///     Creates and sets a new activity ID for the current thread, optionally correlating
        ///     the new activity with another activity.
        /// <param name="relatedActivityId">The ID of an existing activity to be correlated with the
        ///     new activity or <see cref="Guid.Empty"/> if correlation is not desired.</param>
        /// <returns>An object which can be used to revert the activity ID of the current thread once
        ///     the new activity yields control of the current thread.</returns>
        IEtwActivityReverter StartActivity(Guid relatedActivityId);
        ///     Creates and sets a new activity ID for the current thread.  If the current thread
        ///     has an existing activity ID, it will be correlated with the new activity ID.
        IEtwActivityReverter StartActivity();
    ///     A simple implementation of <see cref="IEtwEventCorrelator"/>.
    public class EtwEventCorrelator :
        IEtwEventCorrelator
        private readonly EventProvider _transferProvider;
        private readonly EventDescriptor _transferEvent;
        ///     Creates an <see cref="EtwEventCorrelator"/>.
        /// <param name="transferProvider">The <see cref="EventProvider"/> to use when logging transfer events
        /// <param name="transferEvent">The <see cref="EventDescriptor"/> to use when logging transfer events
        public EtwEventCorrelator(EventProvider transferProvider, EventDescriptor transferEvent)
            ArgumentNullException.ThrowIfNull(transferProvider);
            _transferProvider = transferProvider;
            _transferEvent = transferEvent;
        ///     Implements <see cref="IEtwEventCorrelator.CurrentActivityId"/>.
        public Guid CurrentActivityId
                return EtwActivity.GetActivityId();
                EventProvider.SetActivityId(ref value);
        ///     Implements <see cref="IEtwEventCorrelator.StartActivity(Guid)"/>.
        public IEtwActivityReverter StartActivity(Guid relatedActivityId)
            var retActivity = new EtwActivityReverter(this, CurrentActivityId);
            CurrentActivityId = EventProvider.CreateActivityId();
            if (relatedActivityId != Guid.Empty)
                var tempTransferEvent = _transferEvent;
                _transferProvider.WriteTransferEvent(in tempTransferEvent, relatedActivityId);
            return retActivity;
        ///     Implements <see cref="IEtwEventCorrelator.StartActivity()"/>.
        public IEtwActivityReverter StartActivity()
            return StartActivity(CurrentActivityId);

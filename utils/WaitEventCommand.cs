    /// Waits for a given event to arrive.
    [Cmdlet(VerbsLifecycle.Wait, "Event", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097042")]
    public class WaitEventCommand : PSCmdlet
        /// If timeout is specified, the cmdlet will only wait for this number of seconds.
        /// Value of -1 means never timeout.
                return _timeoutInSeconds;
                _timeoutInSeconds = value;
        private int _timeoutInSeconds = -1; // -1: infinite, this default is to wait for as long as it takes.
        private readonly AutoResetEvent _eventArrived = new(false);
        private PSEventArgs _receivedEvent = null;
        private readonly object _receivedEventLock = new();
        /// Wait for the event to arrive.
            // Subscribe to notification of events received
            Events.ReceivedEvents.PSEventReceived += ReceivedEvents_PSEventReceived;
            bool received = false;
            // Scan the queue to see if it's already arrived
            ScanEventQueue();
            // And wait for our event handler (or Control-C processor) to give us control
            PSLocalEventManager eventManager = (PSLocalEventManager)Events;
            while (!received)
                if (_timeoutInSeconds >= 0)
                    if ((DateTime.UtcNow - startTime).TotalSeconds > _timeoutInSeconds)
                received = _eventArrived.WaitOne(200);
                eventManager.ProcessPendingActions();
            // Unsubscribe, and write the event information we received
            Events.ReceivedEvents.PSEventReceived -= ReceivedEvents_PSEventReceived;
            if (_receivedEvent != null)
                WriteObject(_receivedEvent);
        /// Handle Control-C.
            _eventArrived.Set();
        private void ReceivedEvents_PSEventReceived(object sender, PSEventArgs e)
            // If they want to wait on just any event
            if (_sourceIdentifier == null)
                NotifyEvent(e);
            // They are waiting on a specific one
        // Go through all the received events. If one matches the subscription identifier,
        // break.
        private void ScanEventQueue()
                foreach (PSEventArgs eventArg in Events.ReceivedEvents)
                    if ((_matchPattern == null) || (_matchPattern.IsMatch(eventArg.SourceIdentifier)))
                        NotifyEvent(eventArg);
        // Notify that an event has arrived
        private void NotifyEvent(PSEventArgs e)
            if (_receivedEvent == null)
                lock (_receivedEventLock)
                        _receivedEvent = e;

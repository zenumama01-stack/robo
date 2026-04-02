    /// Unregisters from an event on an object.
    [Cmdlet(VerbsLifecycle.Unregister, "Event", SupportsShouldProcess = true, DefaultParameterSetName = "BySource", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097037")]
    public class UnregisterEventCommand : PSCmdlet
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = "BySource")]
        /// Flag that determines if we should include subscriptions used to support other subscriptions.
        private bool _foundMatch = false;
        /// Unsubscribe from the event.
            foreach (PSEventSubscriber subscriber in Events.Subscribers)
                // If the event identifier matches, remove the subscription
                    ((_sourceIdentifier != null) && _matchPattern.IsMatch(subscriber.SourceIdentifier)) ||
                    ((SubscriptionId >= 0) && (subscriber.SubscriptionId == SubscriptionId))
                    // If this is a support event but they aren't explicitly
                    // looking for them, continue.
                    _foundMatch = true;
                            EventingStrings.EventSubscription,
                            subscriber.SourceIdentifier),
                        EventingStrings.Unsubscribe))
                        Events.UnsubscribeEvent(subscriber);
               (!_foundMatch))
                            EventingStrings.EventSubscriptionNotFound, _sourceIdentifier)),
            else if ((SubscriptionId >= 0) &&
                            EventingStrings.EventSubscriptionNotFound, SubscriptionId)),
                    "INVALID_SUBSCRIPTION_IDENTIFIER",

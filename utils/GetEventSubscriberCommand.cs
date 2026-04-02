    /// Lists all event subscribers.
    [Cmdlet(VerbsCommon.Get, "EventSubscriber", DefaultParameterSetName = "BySource", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096607")]
    [OutputType(typeof(PSEventSubscriber))]
    public class GetEventSubscriberCommand : PSCmdlet
        public int SubscriptionId { get; set; } = -1;
        /// Also show supporting events.
        /// Get the subscribers.
            List<PSEventSubscriber> subscribers = new(Events.Subscribers);
            foreach (PSEventSubscriber subscriber in subscribers)
                   (!_matchPattern.IsMatch(subscriber.SourceIdentifier)))
                // If they specified a subscription identifier and we don't match, continue
                if ((SubscriptionId >= 0) &&
                    (subscriber.SubscriptionId != SubscriptionId))
                // Don't display support events by default
                if (subscriber.SupportEvent && (!Force))
                WriteObject(subscriber);
                bool lookingForId = (SubscriptionId >= 0);
                        error = EventingStrings.EventSubscriptionSourceNotFound;
                        identifier = SubscriptionId;
                        error = EventingStrings.EventSubscriptionNotFound;

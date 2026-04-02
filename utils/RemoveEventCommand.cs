    /// Removes an event from the event queue.
    [Cmdlet(VerbsCommon.Remove, "Event", SupportsShouldProcess = true, DefaultParameterSetName = "BySource", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096715")]
    public class RemoveEventCommand : PSCmdlet
        /// A source identifier for this event subscription.
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "BySource")]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByIdentifier")]
                return _eventIdentifier;
                _eventIdentifier = value;
        private int _eventIdentifier = -1;
        /// Remove the event from the queue.
                PSEventArgsCollection currentEvents = Events.ReceivedEvents;
                for (int eventCounter = currentEvents.Count; eventCounter > 0; eventCounter--)
                    PSEventArgs currentEvent = currentEvents[eventCounter - 1];
                       (!_matchPattern.IsMatch(currentEvent.SourceIdentifier)))
                    // If they specified a TimeGenerated and we don't match, continue
                    if ((_eventIdentifier >= 0) &&
                        (currentEvent.EventIdentifier != _eventIdentifier))
                            EventingStrings.EventResource,
                            currentEvent.SourceIdentifier),
                        EventingStrings.Remove))
                        currentEvents.RemoveAt(eventCounter - 1);
               (!WildcardPattern.ContainsWildcardCharacters(_sourceIdentifier)) &&
               (!foundMatch))
                            EventingStrings.SourceIdentifierNotFound, _sourceIdentifier)),
            else if ((_eventIdentifier >= 0) && (!foundMatch))
                            EventingStrings.EventIdentifierNotFound, _eventIdentifier)),
                    "INVALID_EVENT_IDENTIFIER",

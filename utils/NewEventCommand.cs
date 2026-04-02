    /// Generates a new event notification.
    [Cmdlet(VerbsCommon.New, "Event", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096708")]
    public class NewEventCommand : PSCmdlet
        /// Adds an event to the event queue.
        /// Data relating to this event.
        public PSObject Sender
                return _sender;
                _sender = value;
        private PSObject _sender = null;
        public PSObject[] EventArguments
                return _eventArguments;
                if (_eventArguments != null)
                    _eventArguments = value;
        private PSObject[] _eventArguments = Array.Empty<PSObject>();
        public PSObject MessageData
                return _messageData;
                _messageData = value;
        private PSObject _messageData = null;
        /// Add the event to the event queue.
            object[] baseEventArgs = null;
            // Get the BaseObject from the event arguments
                baseEventArgs = new object[_eventArguments.Length];
                int loopCounter = 0;
                foreach (PSObject eventArg in _eventArguments)
                    if (eventArg != null)
                        baseEventArgs[loopCounter] = eventArg.BaseObject;
                    loopCounter++;
            object messageSender = null;
            if (_sender != null)
                messageSender = _sender.BaseObject;
            // And then generate the event
            WriteObject(Events.GenerateEvent(_sourceIdentifier, messageSender, baseEventArgs, _messageData, true, false));

    [Cmdlet(VerbsLifecycle.Register, "ObjectEvent", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096714")]
    [OutputType(typeof(PSEventJob))]
    public class RegisterObjectEventCommand : ObjectEventRegistrationBase
        /// The object on which to subscribe.
        private PSObject _inputObject = null;
        /// The event name to subscribe.
        public string EventName
                return _eventName;
                _eventName = value;
        private string _eventName = null;

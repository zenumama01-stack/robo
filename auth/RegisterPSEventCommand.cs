    /// Registers for an event coming from the engine.
    [Cmdlet(VerbsLifecycle.Register, "EngineEvent", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097128")]
    public class RegisterEngineEventCommand : ObjectEventRegistrationBase
        /// Parameter for an identifier for this event subscription.
        [Parameter(Mandatory = true, Position = 100)]
        public new string SourceIdentifier
                return base.SourceIdentifier;
                base.SourceIdentifier = value;
            // If it's not a forwarded event, the user must specify
            // an action
                (Action == null) &&
                (!(bool)Forward)
                    new ArgumentException(EventingStrings.ActionMandatoryForLocal),
                    "ACTION_MANDATORY_FOR_LOCAL",

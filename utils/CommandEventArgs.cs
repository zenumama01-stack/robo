    /// Arguments for the event triggered when something happens at the cmdlet level.
    public class CommandEventArgs : EventArgs
        /// the command targeted by the event.
        private CommandViewModel command;
        /// Initializes a new instance of the CommandEventArgs class.
        /// <param name="command">The command targeted by the event.</param>
        public CommandEventArgs(CommandViewModel command)
            this.command = command;
        /// Gets the command targeted by the event.
        public CommandViewModel Command
            get { return this.command; }

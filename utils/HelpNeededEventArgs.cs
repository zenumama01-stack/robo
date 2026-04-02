    /// Arguments for the event triggered when it is necessary to display help for a command.
    public class HelpNeededEventArgs : EventArgs
        /// the name for the command needing help.
        private string commandName;
        /// Initializes a new instance of the HelpNeededEventArgs class.
        /// <param name="commandName">The name for the command needing help.</param>
        public HelpNeededEventArgs(string commandName)
            this.commandName = commandName;
        /// Gets the name for the command needing help.
        public string CommandName
            get { return this.commandName; }

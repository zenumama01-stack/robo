    /// The base class for the SetAliasCommand and NewAliasCommand.
    public class WriteAliasCommandBase : PSCmdlet
        /// The Value parameter for the command.
        public string Value { get; set; }
        /// The description for the alias.
        public string Description { get; set; } = string.Empty;
        /// The Option parameter allows the alias to be set to
        /// ReadOnly (for existing aliases) and/or Constant (only
        /// for new aliases).
        /// The scope parameter for the command determines which scope the alias is set in.

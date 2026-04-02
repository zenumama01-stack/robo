    /// The Enable-LocalUser cmdlet enables local user accounts. When a user account
    /// is disabled, the user is not permitted to log on. When a user account is
    /// enabled, the user is permitted to log on normally.
    [Cmdlet(VerbsLifecycle.Enable, "LocalUser",
            HelpUri = "https://go.microsoft.com/fwlink/?LinkId=717985")]
    [Alias("elu")]
    public class EnableLocalUserCommand : Cmdlet
        private const Enabling enabling = Enabling.Enable;
        /// Specifies the of the local user accounts to enable in the local Security
        /// Specifies the local user accounts to enable in the local Security Accounts
        /// Manager.
        /// Specifies the LocalUser accounts to enable by
            return ShouldProcess(target, Strings.ActionEnableUser);

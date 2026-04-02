    /// The Disable-LocalUser cmdlet disables local user accounts. When a user
    /// account is disabled, the user is not permitted to log on. When a user
    /// account is enabled, the user is permitted to log on normally.
    [Cmdlet(VerbsLifecycle.Disable, "LocalUser",
            HelpUri = "https://go.microsoft.com/fwlink/?LinkId=717986")]
    [Alias("dlu")]
    public class DisableLocalUserCommand : Cmdlet
        private const Enabling enabling = Enabling.Disable;
        /// Specifies the of the local user accounts to disable in the local Security
        /// Accounts Manager.
                   ParameterSetName = "InputObject")]
        public Microsoft.PowerShell.Commands.LocalUser[] InputObject
            get { return this.inputobject; }
            set { this.inputobject = value; }
        private Microsoft.PowerShell.Commands.LocalUser[] inputobject;
        /// Specifies the names of the local user accounts to disable in the local
        /// Security Accounts Manager.
        /// Specifies the LocalUser accounts to disable by
        /// System.Security.Principal.SecurityIdentifier.
        public System.Security.Principal.SecurityIdentifier[] SID
        private System.Security.Principal.SecurityIdentifier[] sid;
                ProcessUsers();
                ProcessNames();
                ProcessSids();
        /// Process users requested by -Name.
        /// All arguments to -Name will be treated as names,
        /// even if a name looks like a SID.
        private void ProcessNames()
                foreach (var name in Name)
                        if (CheckShouldProcess(name))
                            sam.EnableLocalUser(sam.GetLocalUser(name), enabling);
        /// Process users requested by -SID.
        private void ProcessSids()
            if (SID != null)
                foreach (var sid in SID)
                        if (CheckShouldProcess(sid.ToString()))
                            sam.EnableLocalUser(sid, enabling);
        /// Process users requested by -InputObject.
        private void ProcessUsers()
                foreach (var user in InputObject)
                        if (CheckShouldProcess(user.Name))
                            sam.EnableLocalUser(user, enabling);
        private bool CheckShouldProcess(string target)
            return ShouldProcess(target, Strings.ActionDisableUser);

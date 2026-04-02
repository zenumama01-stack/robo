    /// The Rename-LocalUser cmdlet renames a local user account in the Security
    [Cmdlet(VerbsCommon.Rename, "LocalUser",
            HelpUri = "https://go.microsoft.com/fwlink/?LinkID=717983")]
    [Alias("rnlu")]
    public class RenameLocalUserCommand : Cmdlet
        /// Specifies the of the local user account to rename in the local Security
        public Microsoft.PowerShell.Commands.LocalUser InputObject
        private Microsoft.PowerShell.Commands.LocalUser inputobject;
        /// Specifies the local user account to be renamed in the local Security
        /// Specifies the new name for the local user account in the Security Accounts
        /// Specifies the local user to rename.
                ProcessUser();
        /// Process user requested by -Name.
                        sam.RenameLocalUser(sam.GetLocalUser(Name), NewName);
        /// Process user requested by -SID.
                        sam.RenameLocalUser(SID, NewName);
        private void ProcessUser()
                        sam.RenameLocalUser(InputObject, NewName);
        /// Determine if a user should be processed.
        /// <param name="userName">
        /// Name of the user to rename.
        /// New name for the user.
        /// True if the user should be processed, false otherwise.
        private bool CheckShouldProcess(string userName, string newName)
            string msg = StringUtil.Format(Strings.ActionRenameUser, newName);
            return ShouldProcess(userName, msg);

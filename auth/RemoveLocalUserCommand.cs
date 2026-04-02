    /// The Remove-LocalUser cmdlet deletes a user account from the Windows Security
    [Cmdlet(VerbsCommon.Remove, "LocalUser",
            HelpUri = "https://go.microsoft.com/fwlink/?LinkId=717982")]
    [Alias("rlu")]
    public class RemoveLocalUserCommand : Cmdlet
        /// Specifies the of the local user accounts to remove in the local Security
            get { return this.inputobject;}
        /// Specifies the user accounts to be deleted from the local Security Accounts
        /// Specifies the local user accounts to remove by
                            sam.RemoveLocalUser(sam.GetLocalUser(name));
                            sam.RemoveLocalUser(sid);
        /// Process users given through -InputObject.
                            sam.RemoveLocalUser(user);
            return ShouldProcess(target, Strings.ActionRemoveUser);

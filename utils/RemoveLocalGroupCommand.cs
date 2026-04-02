    /// The Remove-LocalGroup cmdlet deletes a security group from the Windows
    /// Security Accounts manager.
    [Cmdlet(VerbsCommon.Remove, "LocalGroup",
            HelpUri = "https://go.microsoft.com/fwlink/?LinkId=717975")]
    [Alias("rlg")]
    public class RemoveLocalGroupCommand : Cmdlet
        /// Specifies security groups from the local Security Accounts Manager.
        public Microsoft.PowerShell.Commands.LocalGroup[] InputObject
        private Microsoft.PowerShell.Commands.LocalGroup[] inputobject;
        /// Specifies the local groups to be deleted from the local Security Accounts
        /// Specifies the LocalGroup accounts to remove by
                ProcessGroups();
                            sam.RemoveLocalGroup(sam.GetLocalGroup(name));
                            sam.RemoveLocalGroup(sid);
        /// Process groups given through -InputObject.
        private void ProcessGroups()
                foreach (var group in InputObject)
                        if (CheckShouldProcess(group.Name))
                            sam.RemoveLocalGroup(group);
            return ShouldProcess(target, Strings.ActionRemoveGroup);

    /// The Rename-LocalGroup cmdlet renames a local security group in the Security
    [Cmdlet(VerbsCommon.Rename, "LocalGroup",
            HelpUri = "https://go.microsoft.com/fwlink/?LinkId=717978")]
    [Alias("rnlg")]
    public class RenameLocalGroupCommand : Cmdlet
        /// Specifies the of the local group account to rename in the local Security
        public Microsoft.PowerShell.Commands.LocalGroup InputObject
        private Microsoft.PowerShell.Commands.LocalGroup inputobject;
        /// Specifies the local group to be renamed in the local Security Accounts
        /// The following is the definition of the input parameter "NewName".
        /// Specifies the new name for the local security group in the Security Accounts
        public string NewName
            get { return this.newname;}
            set { this.newname = value; }
        private string newname;
                ProcessGroup();
                ProcessName();
                ProcessSid();
        /// Process group requested by -Name.
        /// Arguments to -Name will be treated as names,
        private void ProcessName()
                    if (CheckShouldProcess(Name, NewName))
                        sam.RenameLocalGroup(sam.GetLocalGroup(Name), NewName);
        /// Process group requested by -SID.
        private void ProcessSid()
                    if (CheckShouldProcess(SID.ToString(), NewName))
                        sam.RenameLocalGroup(SID, NewName);
        /// Process group given through -InputObject.
        private void ProcessGroup()
                    if (CheckShouldProcess(InputObject.Name, NewName))
                        sam.RenameLocalGroup(InputObject, NewName);
        /// Determine if a group should be processed.
        /// Name of the group to rename.
        /// New name for the group.
        /// True if the group should be processed, false otherwise.
        private bool CheckShouldProcess(string groupName, string newName)
            string msg = StringUtil.Format(Strings.ActionRenameGroup, newName);

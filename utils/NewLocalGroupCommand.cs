    /// The New-LocalGroup Cmdlet can be used to create a new local security group
    /// in the Windows Security Accounts Manager.
    [Cmdlet(VerbsCommon.New, "LocalGroup",
            HelpUri ="https://go.microsoft.com/fwlink/?LinkId=717990")]
    [Alias("nlg")]
    public class NewLocalGroupCommand : Cmdlet
        /// A descriptive comment.
            get { return this.description;}
            set { this.description = value; }
        private string description;
        /// The group name for the local security group.
        [ValidateLength(1, 256)]
                if (CheckShouldProcess(Name))
                    var group = sam.CreateLocalGroup(new LocalGroup
                                                            Description = Description,
                                                            Name = Name
            return ShouldProcess(target, Strings.ActionNewGroup);

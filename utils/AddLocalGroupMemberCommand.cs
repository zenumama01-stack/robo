    /// The Add-LocalGroupMember cmdlet adds one or more users or groups to a local
    /// group.
    [Cmdlet(VerbsCommon.Add, "LocalGroupMember",
            HelpUri = "https://go.microsoft.com/fwlink/?LinkId=717987")]
    [Alias("algm")]
    public class AddLocalGroupMemberCommand : PSCmdlet
        private Sam sam = null;
        #region Parameter Properties
        /// The following is the definition of the input parameter "Group".
        /// Specifies a security group from the local Security Accounts Manager.
                   ParameterSetName = "Group")]
        public Microsoft.PowerShell.Commands.LocalGroup Group
            get { return this.group;}
            set { this.group = value; }
        private Microsoft.PowerShell.Commands.LocalGroup group;
        /// The following is the definition of the input parameter "Member".
        /// Specifies one or more users or groups to add to this local group. You can
        /// identify users or groups by specifying their names or SIDs, or by passing
        /// Microsoft.PowerShell.Commands.LocalPrincipal objects.
        public Microsoft.PowerShell.Commands.LocalPrincipal[] Member
            get { return this.member;}
            set { this.member = value; }
        private Microsoft.PowerShell.Commands.LocalPrincipal[] member;
                   ParameterSetName = "Default")]
            get { return this.name;}
            set { this.name = value; }
        /// The following is the definition of the input parameter "SID".
                   ParameterSetName = "SecurityIdentifier")]
        public System.Security.Principal.SecurityIdentifier SID
            get { return this.sid;}
            set { this.sid = value; }
        private System.Security.Principal.SecurityIdentifier sid;
        #endregion Parameter Properties
            sam = new Sam();
                if (Group != null)
                    ProcessGroup(Group);
                else if (Name != null)
                    ProcessName(Name);
                else if (SID != null)
                    ProcessSid(SID);
            catch (GroupNotFoundException ex)
                WriteError(ex.MakeErrorRecord());
            if (sam != null)
                sam.Dispose();
                sam = null;
        /// Creates a list of <see cref="LocalPrincipal"/> objects
        /// ready to be processed by the cmdlet.
        /// Name or SID (as a string) of the group we'll be adding to.
        /// This string is used primarily for specifying the target
        /// in WhatIf scenarios.
        /// LocalPrincipal object to be processed
        /// A LocalPrincipal Object to be added to the group
        /// LocalPrincipal objects in the Member parameter may not be complete,
        /// particularly those created from a name or a SID string given to the
        /// Member cmdlet parameter. The object returned from this method contains
        /// , at the very least, a valid SID.
        /// Any Member objects provided by name or SID string will be looked up
        /// to ensure that such an object exists. If an object is not found,
        /// an error message is displayed by PowerShell and null will be returned
        /// This method also handles the WhatIf scenario. If the Cmdlet's
        /// <b>ShouldProcess</b> method returns false on any Member object,
        /// that object will not be included in the returned List.
        private LocalPrincipal MakePrincipal(string groupId, LocalPrincipal member)
            LocalPrincipal principal = null;
            // if the member has a SID, we can use it directly
            if (member.SID != null)
                principal = member;
            else    // otherwise it must have been constructed by name
                SecurityIdentifier sid = this.TrySid(member.Name);
                if (sid != null)
                    member.SID = sid;
                        principal = sam.LookupAccount(member.Name);
            if (CheckShouldProcess(principal, groupId))
                return principal;
        /// Determine if a principal should be processed.
        /// Just a wrapper around Cmdlet.ShouldProcess, with localized string
        /// formatting.
        /// <param name="principal">Name of the principal to be added.</param>
        /// <param name="groupName">
        /// Name of the group to which the members will be added.
        /// True if the principal should be processed, false otherwise.
        private bool CheckShouldProcess(LocalPrincipal principal, string groupName)
            if (principal == null)
            string msg = StringUtil.Format(Strings.ActionAddGroupMember, principal.ToString());
            return ShouldProcess(groupName, msg);
        /// A <see cref="LocalGroup"/> object representing the group to which
        /// the members will be added.
        private void ProcessGroup(LocalGroup group)
            string groupId = group.Name ?? group.SID.ToString();
            foreach (var member in this.Member)
                LocalPrincipal principal = MakePrincipal(groupId, member);
                if (principal != null)
                    var ex = sam.AddLocalGroupMember(group, principal);
        /// Add members to a group specified by name.
        /// The name of the group to which the members will be added.
        private void ProcessName(string name)
            ProcessGroup(sam.GetLocalGroup(name));
        /// Add members to a group specified by SID.
        /// A <see cref="SecurityIdentifier"/> object identifying the group
        /// to which the members will be added.
        private void ProcessSid(SecurityIdentifier groupSid)
                LocalPrincipal principal = MakePrincipal(groupSid.ToString(), member);
                    var ex = sam.AddLocalGroupMember(groupSid, principal);

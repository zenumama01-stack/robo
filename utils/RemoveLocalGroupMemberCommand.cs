    /// The Remove-LocalGroupMember cmdlet removes one or more members (users or
    /// groups) from a local security group.
    [Cmdlet(VerbsCommon.Remove, "LocalGroupMember",
            HelpUri = "https://go.microsoft.com/fwlink/?LinkId=717989")]
    [Alias("rlgm")]
    public class RemoveLocalGroupMemberCommand : PSCmdlet
        /// Specifies one or more users or groups to remove from this local group. You can
        /// Name or SID (as a string) of the group we'll be removing from.
        /// LocalPrincipal object processed and ready to be removed
        /// LocalPrincipal object in the Member parameter may not be complete,
        /// Member cmdlet parameter. The object returned from this method contains at the very least, contain a valid SID.
        /// Any Member object provided by name or SID string will be looked up
        /// an error message is displayed by PowerShell and null will be returned from this method
        /// <b>ShouldProcess</b> method returns false on any Member object
        /// <param name="principal">Name of the principal to be removed.</param>
        /// Name of the group from which the members will be removed.
            string msg = StringUtil.Format(Strings.ActionRemoveGroupMember, principal.ToString());
        /// A <see cref="LocalGroup"/> object representing the group from which
        /// the members will be removed.
                    var ex = sam.RemoveLocalGroupMember(group, principal);
        /// Remove members from a group specified by name.
        /// The name of the group from which the members will be removed.
        /// Remove members from a group specified by SID.
        /// from which the members will be removed.
                    var ex = sam.RemoveLocalGroupMember(groupSid, principal);

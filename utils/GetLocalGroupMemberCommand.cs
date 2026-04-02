    /// The Get-LocalGroupMember cmdlet gets the members of a local group.
    [Cmdlet(VerbsCommon.Get, "LocalGroupMember",
            HelpUri = "https://go.microsoft.com/fwlink/?LinkId=717988")]
    [Alias("glgm")]
    public class GetLocalGroupMemberCommand : Cmdlet
        /// The security group from the local Security Accounts Manager.
        /// Specifies the name of the user or group that is a member of this group. If
        /// this parameter is not specified, all members of the specified group are
        /// returned. This accepts a name, SID, or wildcard string.
        public string Member
        private string member;
                IEnumerable<LocalPrincipal> principals = null;
                    principals = ProcessGroup(Group);
                    principals = ProcessName(Name);
                    principals = ProcessSid(SID);
                if (principals != null)
                    WriteObject(principals, true);
        private IEnumerable<LocalPrincipal> ProcessesMembership(IEnumerable<LocalPrincipal> membership)
            List<LocalPrincipal> rv;
            // if no members are specified, return all of them
            if (Member == null)
                // return membership;
                rv = new List<LocalPrincipal>(membership);
                // var rv = new List<LocalPrincipal>();
                rv = new List<LocalPrincipal>();
                if (WildcardPattern.ContainsWildcardCharacters(Member))
                    var pattern = new WildcardPattern(Member, WildcardOptions.Compiled
                    foreach (var m in membership)
                        if (pattern.IsMatch(sam.StripMachineName(m.Name)))
                            rv.Add(m);
                    var sid = this.TrySid(Member);
                            if (m.SID == sid)
                            if (sam.StripMachineName(m.Name).Equals(Member, StringComparison.CurrentCultureIgnoreCase))
                    if (rv.Count == 0)
                        var ex = new PrincipalNotFoundException(member, member);
            // sort the resulting principals by mane
            rv.Sort(static (p1, p2) => string.Compare(p1.Name, p2.Name, StringComparison.CurrentCultureIgnoreCase));
        private IEnumerable<LocalPrincipal> ProcessGroup(LocalGroup group)
            return ProcessesMembership(sam.GetLocalGroupMembers(group));
        private IEnumerable<LocalPrincipal> ProcessName(string name)
            return ProcessGroup(sam.GetLocalGroup(name));
        private IEnumerable<LocalPrincipal> ProcessSid(SecurityIdentifier groupSid)
            return ProcessesMembership(sam.GetLocalGroupMembers(groupSid));

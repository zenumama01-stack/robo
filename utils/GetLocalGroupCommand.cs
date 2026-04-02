    /// The Get-LocalGroup cmdlet gets local groups from the Windows Security
    /// Accounts manager.
    [Cmdlet(VerbsCommon.Get, "LocalGroup",
            HelpUri = "https://go.microsoft.com/fwlink/?LinkId=717974")]
    [Alias("glg")]
    public class GetLocalGroupCommand : Cmdlet
        /// Specifies the local groups to get from the local Security Accounts Manager.
        /// Specifies a local group from the local Security Accounts Manager.
            if (Name == null && SID == null)
                foreach (var group in sam.GetAllLocalGroups())
                    WriteObject(group);
        /// Process groups requested by -Name.
        /// Groups may be specified using wildcards.
                            var pattern = new WildcardPattern(name, WildcardOptions.Compiled
                                                                | WildcardOptions.IgnoreCase);
                            foreach (var group in sam.GetMatchingLocalGroups(n => pattern.IsMatch(n)))
                            WriteObject(sam.GetLocalGroup(name));
        /// Process groups requested by -SID.
                        WriteObject(sam.GetLocalGroup(sid));

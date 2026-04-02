    /// The Get-LocalUser cmdlet gets local user accounts from the Windows Security
    /// Accounts Manager. This includes local accounts that have been connected to a
    /// Microsoft account.
    [Cmdlet(VerbsCommon.Get, "LocalUser",
            HelpUri = "https://go.microsoft.com/fwlink/?LinkId=717980")]
    [Alias("glu")]
    public class GetLocalUserCommand : Cmdlet
        /// Specifies the local user accounts to get from the local Security Accounts
        /// Manager. This accepts a name or wildcard string.
        /// Specifies a user from the local Security Accounts Manager.
            get { return this.sid; }
                foreach (var user in sam.GetAllLocalUsers())
                    WriteObject(user);
        /// Users may be specified using wildcards.
                foreach (var nm in Name)
                        if (WildcardPattern.ContainsWildcardCharacters(nm))
                            var pattern = new WildcardPattern(nm, WildcardOptions.Compiled
                            foreach (var user in sam.GetMatchingLocalUsers(n => pattern.IsMatch(n)))
                            WriteObject(sam.GetLocalUser(nm));
                foreach (var s in SID)
                        WriteObject(sam.GetLocalUser(s));

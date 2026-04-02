    /// The implementation of the "set-alias" cmdlet.
    [Cmdlet(VerbsCommon.Set, "Alias", SupportsShouldProcess = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096625")]
    public class SetAliasCommand : WriteAliasCommandBase
            AliasInfo aliasToSet =
            aliasToSet.Description = Description;
            string action = AliasCommandStrings.SetAliasAction;
            string target = StringUtil.Format(AliasCommandStrings.SetAliasTarget, Name, Value);
                        result = SessionState.Internal.SetAliasItem(aliasToSet, Force, MyInvocation.CommandOrigin);
                        result = SessionState.Internal.SetAliasItemAtScope(aliasToSet, Scope, Force, MyInvocation.CommandOrigin);

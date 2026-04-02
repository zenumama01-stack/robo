    /// The implementation of the "new-alias" cmdlet.
    [Cmdlet(VerbsCommon.New, "Alias", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Low,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097022")]
    public class NewAliasCommand : WriteAliasCommandBase
                    existingAlias = SessionState.Internal.GetAlias(Name);
                    existingAlias = SessionState.Internal.GetAliasAtScope(Name, Scope);
                    // Throw if alias exists and is private...
                    SessionState.ThrowIfNotVisible(this.CommandOrigin, existingAlias);
            // Create the alias info
                    Value,
                    Option);
            newAlias.Description = Description;
                AliasCommandStrings.NewAliasAction;
                    StringUtil.Format(AliasCommandStrings.NewAliasTarget, Name, Value);
            if (ShouldProcess(target, action))
                        result = SessionState.Internal.SetAliasItem(newAlias, Force, MyInvocation.CommandOrigin);
                        result = SessionState.Internal.SetAliasItemAtScope(newAlias, Scope, Force, MyInvocation.CommandOrigin);

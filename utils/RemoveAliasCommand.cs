    /// The implementation of the "Remove-Alias" cmdlet.
    [Cmdlet(VerbsCommon.Remove, "Alias", DefaultParameterSetName = "Default", HelpUri = "https://go.microsoft.com/fwlink/?linkid=2097127")]
    [Alias("ral")]
    public class RemoveAliasCommand : PSCmdlet
        /// The alias name to remove.
        /// The scope parameter for the command determines which scope the alias is removed from.
        /// and is ReadOnly, it will still be deleted.
            foreach (string aliasName in Name)
                    existingAlias = SessionState.Internal.GetAlias(aliasName);
                    existingAlias = SessionState.Internal.GetAliasAtScope(aliasName, Scope);
                    SessionState.Internal.RemoveAlias(aliasName, Force);
                    ItemNotFoundException notAliasFound = new(StringUtil.Format(AliasCommandStrings.NoAliasFound, "name", aliasName));
                    ErrorRecord error = new(notAliasFound, "ItemNotFoundException", ErrorCategory.ObjectNotFound, aliasName);

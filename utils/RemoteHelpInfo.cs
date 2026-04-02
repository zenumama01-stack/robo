    internal class RemoteHelpInfo : BaseCommandHelpInfo
        private readonly PSObject _deserializedRemoteHelp;
        internal RemoteHelpInfo(
            RemoteRunspace remoteRunspace,
            string localCommandName,
            string remoteHelpTopic,
            string remoteHelpCategory,
            HelpCategory localHelpCategory) : base(localHelpCategory)
            Dbg.Assert(remoteRunspace != null, "Caller should verify arguments");
                powerShell.AddCommand("Get-Help");
                powerShell.AddParameter("Name", remoteHelpTopic);
                if (!string.IsNullOrEmpty(remoteHelpCategory))
                    powerShell.AddParameter("Category", remoteHelpCategory);
                Collection<PSObject> helpResults;
                using (new PowerShellStopper(context, powerShell))
                    helpResults = powerShell.Invoke();
                if ((helpResults == null) || (helpResults.Count == 0))
                    throw new Microsoft.PowerShell.Commands.HelpNotFoundException(remoteHelpTopic);
                Dbg.Assert(helpResults.Count == 1, "Remote help should return exactly one result");
                _deserializedRemoteHelp = helpResults[0];
                _deserializedRemoteHelp.Methods.Remove("ToString");
                // Win8: bug9457: Remote proxy command's name can be changed locally using -Prefix
                // parameter of the Import-PSSession cmdlet. To give better user experience for
                // get-help (on par with get-command), it was decided to use the local command name
                PSPropertyInfo nameInfo = _deserializedRemoteHelp.Properties["Name"];
                if (nameInfo != null)
                    nameInfo.Value = localCommandName;
                if (commandDetails != null)
                    nameInfo = commandDetails.Properties["Name"];
                        commandDetails.InstanceMembers.Add(new PSNoteProperty("Name", localCommandName));
                return _deserializedRemoteHelp;
        private string GetHelpProperty(string propertyName)
            PSPropertyInfo property = _deserializedRemoteHelp.Properties[propertyName];
            return property.Value as string;
                return this.GetHelpProperty("Component");
                return this.GetHelpProperty("Functionality");
                return this.GetHelpProperty("Role");

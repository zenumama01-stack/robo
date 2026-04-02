    /// Class BaseCommandHelpInfo provides common functionality for
    /// extracting information from FullHelp property.
    internal abstract class BaseCommandHelpInfo : HelpInfo
        internal BaseCommandHelpInfo(HelpCategory helpCategory)
            HelpCategory = helpCategory;
        #region Basic Help Properties
        internal PSObject Details
                if (this.FullHelp == null)
                if (this.FullHelp.Properties["Details"] == null ||
                    this.FullHelp.Properties["Details"].Value == null)
                return PSObject.AsPSObject(this.FullHelp.Properties["Details"].Value);
        /// Name of command.
        /// <value>Name of command</value>
                PSObject commandDetails = this.Details;
                if (commandDetails == null)
                if (commandDetails.Properties["Name"] == null ||
                    commandDetails.Properties["Name"].Value == null)
                string name = commandDetails.Properties["Name"].Value.ToString();
                return name.Trim();
        /// Synopsis for this command help.
        /// <value>Synopsis for this command help</value>
        internal override string Synopsis
                if (commandDetails.Properties["Description"] == null ||
                    commandDetails.Properties["Description"].Value == null)
                object[] synopsisItems = (object[])LanguagePrimitives.ConvertTo(
                    commandDetails.Properties["Description"].Value,
                    typeof(object[]),
                if (synopsisItems == null || synopsisItems.Length == 0)
                PSObject firstSynopsisItem = synopsisItems[0] == null ? null : PSObject.AsPSObject(synopsisItems[0]);
                if (firstSynopsisItem == null ||
                    firstSynopsisItem.Properties["Text"] == null ||
                    firstSynopsisItem.Properties["Text"].Value == null)
                string synopsis = firstSynopsisItem.Properties["Text"].Value.ToString();
                if (synopsis == null)
                return synopsis.Trim();
        /// Help category for this command help, which is constantly HelpCategory.Command.
        /// <value>Help category for this command help</value>
        internal override HelpCategory HelpCategory { get; }
        /// Returns the Uri used by get-help cmdlet to show help
        /// online. Returns only the first uri found under
        /// RelatedLinks.
        /// Null if no Uri is specified by the helpinfo or a
        /// valid Uri.
        /// Specified Uri is not valid.
        internal override Uri GetUriForOnlineHelp()
            Uri result = null;
            UriFormatException uriFormatException = null;
                result = GetUriFromCommandPSObject(this.FullHelp);
            catch (UriFormatException urie)
                uriFormatException = urie;
            // else get uri from CommandInfo HelpUri attribute
            result = this.LookupUriFromCommandInfo();
            else if (uriFormatException != null)
                throw uriFormatException;
            return base.GetUriForOnlineHelp();
        internal Uri LookupUriFromCommandInfo()
            CommandTypes cmdTypesToLookFor = CommandTypes.Cmdlet;
            switch (this.HelpCategory)
                case Automation.HelpCategory.Cmdlet:
                    cmdTypesToLookFor = CommandTypes.Cmdlet;
                case Automation.HelpCategory.Function:
                    cmdTypesToLookFor = CommandTypes.Function;
                case Automation.HelpCategory.ScriptCommand:
                    cmdTypesToLookFor = CommandTypes.Script;
                case Automation.HelpCategory.ExternalScript:
                    cmdTypesToLookFor = CommandTypes.ExternalScript;
                case Automation.HelpCategory.Filter:
                    cmdTypesToLookFor = CommandTypes.Filter;
                case Automation.HelpCategory.Configuration:
                    cmdTypesToLookFor = CommandTypes.Configuration;
            string commandName = this.Name;
            if (this.FullHelp.Properties["ModuleName"] != null)
                PSNoteProperty moduleNameNP = this.FullHelp.Properties["ModuleName"] as PSNoteProperty;
                if (moduleNameNP != null)
                    LanguagePrimitives.TryConvertTo<string>(moduleNameNP.Value, CultureInfo.InvariantCulture,
                                                            out moduleName);
            string commandToSearch = commandName;
                commandToSearch = string.Create(CultureInfo.InvariantCulture, $"{moduleName}\\{commandName}");
                CommandInfo cmdInfo = null;
                if (cmdTypesToLookFor == CommandTypes.Cmdlet)
                    cmdInfo = context.SessionState.InvokeCommand.GetCmdlet(commandToSearch);
                    cmdInfo = context.SessionState.InvokeCommand.GetCommands(commandToSearch, cmdTypesToLookFor, false).FirstOrDefault();
                if ((cmdInfo == null) || (cmdInfo.CommandMetadata == null))
                string uriString = cmdInfo.CommandMetadata.HelpUri;
                if (!string.IsNullOrEmpty(uriString))
                    if (!System.Uri.IsWellFormedUriString(uriString, UriKind.RelativeOrAbsolute))
                        // WinBlue: 545315 Online help links are broken with localized help
                        // Example: https://go.microsoft.com/fwlink/?LinkID=113324 (moglicherwei se auf Englisch)
                        // Split the string based on <s> (space). We decided to go with this approach as
                        // UX localization authors use spaces. Correctly extracting only the wellformed URI
                        // is out-of-scope for this fix.
                        string[] tempUriSplitArray = uriString.Split(' ');
                        uriString = tempUriSplitArray[0];
                        return new System.Uri(uriString);
                        // return only the first Uri (ignore other uris)
                        throw PSTraceSource.NewInvalidOperationException(HelpErrors.InvalidURI,
                                                                         cmdInfo.CommandMetadata.HelpUri);
        internal static Uri GetUriFromCommandPSObject(PSObject commandFullHelp)
            // this object knows Maml format...
            // So retrieve Uri information as per the format..
            if ((commandFullHelp == null) ||
                (commandFullHelp.Properties["relatedLinks"] == null) ||
                (commandFullHelp.Properties["relatedLinks"].Value == null))
                // return the default..
            PSObject relatedLinks = PSObject.AsPSObject(commandFullHelp.Properties["relatedLinks"].Value);
            if (relatedLinks.Properties["navigationLink"] == null)
            object[] navigationLinks = (object[])LanguagePrimitives.ConvertTo(
                relatedLinks.Properties["navigationLink"].Value,
            foreach (object navigationLinkAsObject in navigationLinks)
                if (navigationLinkAsObject == null)
                PSObject navigationLink = PSObject.AsPSObject(navigationLinkAsObject);
                PSNoteProperty uriNP = navigationLink.Properties["uri"] as PSNoteProperty;
                if (uriNP != null)
                    string uriString = string.Empty;
                    LanguagePrimitives.TryConvertTo<string>(uriNP.Value, CultureInfo.InvariantCulture, out uriString);
                            throw PSTraceSource.NewInvalidOperationException(HelpErrors.InvalidURI, uriString);
        /// Returns true if help content in help info matches the
        /// pattern contained in <paramref name="pattern"/>.
        /// The underlying code will usually run pattern.IsMatch() on
        /// content it wants to search.
        /// Cmdlet help info looks for pattern in Synopsis and
        /// DetailedDescription.
        internal override bool MatchPatternInContent(WildcardPattern pattern)
            Dbg.Assert(pattern != null, "pattern cannot be null");
            string synopsis = Synopsis;
            string detailedDescription = DetailedDescription;
            synopsis ??= string.Empty;
            detailedDescription ??= string.Empty;
            return pattern.IsMatch(synopsis) || pattern.IsMatch(detailedDescription);
        /// Returns help information for a parameter(s) identified by pattern.
        /// <param name="pattern">Pattern to search for parameters.</param>
        /// <returns>A collection of parameters that match pattern.</returns>
        internal override PSObject[] GetParameter(string pattern)
            // So retrieve parameter information as per the format..
            if ((this.FullHelp == null) ||
                (this.FullHelp.Properties["parameters"] == null) ||
                (this.FullHelp.Properties["parameters"].Value == null))
                return base.GetParameter(pattern);
            PSObject prmts = PSObject.AsPSObject(this.FullHelp.Properties["parameters"].Value);
            if (prmts.Properties["parameter"] == null)
            // The Maml format simplifies array fields containing only one object
            // by transforming them into the objects themselves. To ensure the consistency
            // of the help command result we change it back into an array.
            var param = prmts.Properties["parameter"].Value;
            PSObject[] paramAsPSObjArray = new PSObject[1];
            if (param is PSObject paramPSObj)
                paramAsPSObjArray[0] = paramPSObj;
            PSObject[] prmtArray = (PSObject[])LanguagePrimitives.ConvertTo(
                paramAsPSObjArray[0] != null ? paramAsPSObjArray : param,
                typeof(PSObject[]),
                return prmtArray;
            List<PSObject> returnList = new List<PSObject>();
            foreach (PSObject prmtr in prmtArray)
                if ((prmtr.Properties["name"] == null) || (prmtr.Properties["name"].Value == null))
                string prmName = prmtr.Properties["name"].Value.ToString();
                if (matcher.IsMatch(prmName))
                    returnList.Add(prmtr);
            return returnList.ToArray();
        #region Cmdlet Help specific Properties
        /// Detailed Description string of this cmdlet help info.
        internal string DetailedDescription
                if (this.FullHelp.Properties["Description"] == null ||
                    this.FullHelp.Properties["Description"].Value == null)
                object[] descriptionItems = (object[])LanguagePrimitives.ConvertTo(
                    this.FullHelp.Properties["Description"].Value,
                if (descriptionItems == null || descriptionItems.Length == 0)
                // I think every cmdlet description should at least have 400 characters...
                // so starting with this assumption..I did an average of all the cmdlet
                // help content available at the time of writing this code and came up
                // with this number.
                StringBuilder result = new StringBuilder(400);
                foreach (object descriptionItem in descriptionItems)
                    if (descriptionItem == null)
                    PSObject descriptionObject = PSObject.AsPSObject(descriptionItem);
                    if ((descriptionObject == null) ||
                        (descriptionObject.Properties["Text"] == null) ||
                        (descriptionObject.Properties["Text"].Value == null))
                    string text = descriptionObject.Properties["Text"].Value.ToString();
                    result.Append(text);
                    result.Append(Environment.NewLine);
                return result.ToString().Trim();

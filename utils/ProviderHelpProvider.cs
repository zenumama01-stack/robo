    /// Class ProviderHelpProvider implement the help provider for commands.
    /// Provider Help information are stored in 'help.xml' files. Location of these files
    internal class ProviderHelpProvider : HelpProviderWithCache
        internal ProviderHelpProvider(HelpSystem helpSystem) : base(helpSystem)
        /// Name of this help provider.
        /// <value>Name of this help provider.</value>
                return "Provider Help Provider";
        /// Help category of this provider.
        /// <value>Help category of this provider</value>
        /// Do exact match help based on the target.
            Collection<ProviderInfo> matchingProviders = null;
                matchingProviders = _sessionState.Provider.Get(helpRequest.Target);
                // We distinguish two cases here,
                //      a. If the "Provider" is the only category to search for in this case,
                //         an error will be written.
                //      b. Otherwise, no errors will be written since in end user's mind,
                //         he may mean to search for provider help.
                if (this.HelpSystem.LastHelpCategory == HelpCategory.Provider)
                    ErrorRecord errorRecord = new ErrorRecord(e, "ProviderLoadError", ErrorCategory.ResourceUnavailable, null);
                    errorRecord.ErrorDetails = new ErrorDetails(typeof(ProviderHelpProvider).Assembly, "HelpErrors", "ProviderLoadError", helpRequest.Target, e.Message);
            if (matchingProviders != null)
                foreach (ProviderInfo providerInfo in matchingProviders)
                        LoadHelpFile(providerInfo);
                        ReportHelpFileError(ioException, helpRequest.Target, providerInfo.HelpFile);
                        ReportHelpFileError(securityException, helpRequest.Target, providerInfo.HelpFile);
                        ReportHelpFileError(xmlException, helpRequest.Target, providerInfo.HelpFile);
                    HelpInfo helpInfo = GetCache(providerInfo.PSSnapInName + "\\" + providerInfo.Name);
        private static string GetProviderAssemblyPath(ProviderInfo providerInfo)
            if (providerInfo.ImplementingType == null)
            return Path.GetDirectoryName(providerInfo.ImplementingType.Assembly.Location);
        /// (Which should not happen unless some provider is pointing
        /// Load help file provided.
        /// This will load providerHelpInfo from help file into help cache.
        /// <param name="providerInfo">ProviderInfo for which to locate help.</param>
        private void LoadHelpFile(ProviderInfo providerInfo)
            string helpFile = providerInfo.HelpFile;
            if (string.IsNullOrEmpty(helpFile) || _helpFiles.Contains(helpFile))
            PSSnapInInfo mshSnapInInfo = providerInfo.PSSnapIn;
            // 1. If PSSnapInInfo exists, then always look in the application base
            //    of the mshsnapin
                helpFileToLoad = Path.Combine(mshSnapInInfo.ApplicationBase, helpFile);
            else if ((providerInfo.Module != null) && (!string.IsNullOrEmpty(providerInfo.Module.Path)))
                helpFileToLoad = Path.Combine(providerInfo.Module.ModuleBase, helpFile);
                searchPaths.Add(GetProviderAssemblyPath(providerInfo));
                throw new FileNotFoundException(helpFile);
                new FileInfo(location),
                    if (node.NodeType == XmlNodeType.Element && string.Equals(node.Name, "helpItems", StringComparison.OrdinalIgnoreCase))
            using (this.HelpSystem.Trace(location))
                        if (node.NodeType == XmlNodeType.Element && string.Equals(node.Name, "providerHelp", StringComparison.OrdinalIgnoreCase))
                            HelpInfo helpInfo = ProviderHelpInfo.Load(node);
                                // Add snapin qualified type name for this command..
                                        $"ProviderHelpInfo#{providerInfo.PSSnapInName}#{helpInfo.Name}"));
                                if (!string.IsNullOrEmpty(providerInfo.PSSnapInName))
                                    helpInfo.FullHelp.Properties.Add(new PSNoteProperty("PSSnapIn", providerInfo.PSSnapIn));
                                            $"ProviderHelpInfo#{providerInfo.PSSnapInName}"));
                                AddCache(providerInfo.PSSnapInName + "\\" + helpInfo.Name, helpInfo);
        /// Search for provider help based on a search target.
            bool decoratedSearch = !WildcardPattern.ContainsWildcardCharacters(target);
                // search in all providers
            PSSnapinQualifiedName snapinQualifiedNameForPattern =
                PSSnapinQualifiedName.GetInstance(pattern);
            if (snapinQualifiedNameForPattern == null)
            foreach (ProviderInfo providerInfo in _sessionState.Provider.GetAll())
                if (providerInfo.IsMatch(pattern))
                        if (!decoratedSearch)
                            ReportHelpFileError(ioException, providerInfo.Name, providerInfo.HelpFile);
                            ReportHelpFileError(securityException, providerInfo.Name, providerInfo.HelpFile);
                            ReportHelpFileError(xmlException, providerInfo.Name, providerInfo.HelpFile);
                            // ignore help objects that do not have pattern in its help
                            // content.
            ProviderCommandHelpInfo providerCommandHelpInfo = new ProviderCommandHelpInfo(
                helpInfo, helpRequest.ProviderContext);
            yield return providerCommandHelpInfo;
        /// Process a helpInfo forwarded from other providers (normally commandHelpProvider)
        /// For command help info, this will
        ///     1. check whether provider-specific commandlet help exists.
        ///     2. merge found provider-specific help with commandlet help provided.
        /// <param name="helpInfo">HelpInfo forwarded in.</param>
        /// <returns>The help info object after processing.</returns>
        override internal HelpInfo ProcessForwardedHelp(HelpInfo helpInfo, HelpRequest helpRequest)
            if (helpInfo.HelpCategory != HelpCategory.Command)
            string providerName = helpRequest.Provider;
                providerName = this._sessionState.Path.CurrentLocation.Provider.Name;
            HelpRequest providerHelpRequest = helpRequest.Clone();
            providerHelpRequest.Target = providerName;
            ProviderHelpInfo providerHelpInfo = (ProviderHelpInfo)this.ExactMatchHelp(providerHelpRequest);
            if (providerHelpInfo == null)
            CommandHelpInfo commandHelpInfo = (CommandHelpInfo)helpInfo;
            CommandHelpInfo result = commandHelpInfo.MergeProviderSpecificHelp(providerHelpInfo.GetCmdletHelp(commandHelpInfo.Name), providerHelpInfo.GetDynamicParameterHelp(helpRequest.DynamicParameters));
            // Reset ForwardHelpCategory for the helpinfo to be returned so that it will not be forwarded back again.
            result.ForwardHelpCategory = HelpCategory.None;

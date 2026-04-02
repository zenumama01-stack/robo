    /// The ProviderCommandHelpInfo class.
    internal class ProviderCommandHelpInfo : HelpInfo
        /// Help info.
        private readonly HelpInfo _helpInfo;
        /// Constructor for ProviderCommandHelpInfo.
        internal ProviderCommandHelpInfo(HelpInfo genericHelpInfo, ProviderContext providerContext)
            Dbg.Assert(genericHelpInfo != null, "Expected genericHelpInfo != null");
            Dbg.Assert(providerContext != null, "Expected providerContext != null");
            // This should be set to None to prevent infinite forwarding.
            this.ForwardHelpCategory = HelpCategory.None;
            // Now pick which help we should show.
            MamlCommandHelpInfo providerSpecificHelpInfo =
                providerContext.GetProviderSpecificHelpInfo(genericHelpInfo.Name);
            if (providerSpecificHelpInfo == null)
                _helpInfo = genericHelpInfo;
                providerSpecificHelpInfo.OverrideProviderSpecificHelpWithGenericHelp(genericHelpInfo);
                _helpInfo = providerSpecificHelpInfo;
        /// Get parameter.
            return _helpInfo.GetParameter(pattern);
            return _helpInfo.GetUriForOnlineHelp();
        /// The Name property.
                return _helpInfo.Name;
        /// The Synopsis property.
                return _helpInfo.Synopsis;
        /// The HelpCategory property.
                return _helpInfo.HelpCategory;
        /// The FullHelp property.
                return _helpInfo.FullHelp;
        /// The Component property.
                return _helpInfo.Component;
        /// The Role property.
                return _helpInfo.Role;
        /// The Functionality property.
                return _helpInfo.Functionality;

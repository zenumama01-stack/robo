    /// Class DefaultHelpProvider implement the help provider for commands.
    /// can be found from CommandDiscovery.
    internal class DefaultHelpProvider : HelpFileHelpProvider
        /// Constructor for HelpProvider.
        internal DefaultHelpProvider(HelpSystem helpSystem)
            : base(helpSystem)
                return "Default Help Provider";
                return HelpCategory.DefaultHelp;
            HelpRequest defaultHelpRequest = helpRequest.Clone();
            defaultHelpRequest.Target = "default";
            return base.ExactMatchHelp(defaultHelpRequest);

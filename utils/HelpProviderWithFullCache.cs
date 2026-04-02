    /// Class HelpProviderWithFullCache provides a pseudo implementation of HelpProvider
    /// at which results are fully cached in a hashtable after initial cache load.
    /// This class is different from HelpProviderWithCache class in the sense that
    /// help contents for this provider can be loaded once and be used for later
    /// search. So logically class derived from this class only need to provide
    /// a way to load and initialize help cache.
    internal abstract class HelpProviderWithFullCache : HelpProviderWithCache
        /// Constructor for HelpProviderWithFullCache.
        internal HelpProviderWithFullCache(HelpSystem helpSystem) : base(helpSystem)
        /// Exact match help for a target. This function will be sealed right here
        /// since this is no need for children class to override this member.
        internal sealed override IEnumerable<HelpInfo> ExactMatchHelp(HelpRequest helpRequest)
                LoadCache();
            this.CacheFullyLoaded = true;
            return base.ExactMatchHelp(helpRequest);
        /// Do exact match help for a target. This member is sealed right here since
        /// children class don't need to override this member.
        internal sealed override void DoExactMatchHelp(HelpRequest helpRequest)
        /// Search help for a target. This function will be sealed right here
        internal sealed override IEnumerable<HelpInfo> SearchHelp(HelpRequest helpRequest, bool searchOnlyContent)
            return base.SearchHelp(helpRequest, searchOnlyContent);
        /// Do search help. This function will be sealed right here
        internal sealed override IEnumerable<HelpInfo> DoSearchHelp(HelpRequest helpRequest)
        /// Load cache for later searching for help.
        /// This is the only member child class need to override for help search purpose.
        /// This function will be called only once (usually this happens at the first time when
        /// end user request some help in the target help category).
        internal virtual void LoadCache()

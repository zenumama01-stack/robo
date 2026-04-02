    /// Class HelpProviderWithCache provides a pseudo implementation of HelpProvider
    /// at which results are cached in a hashtable so that later retrieval can be
    /// faster.
    internal abstract class HelpProviderWithCache : HelpProvider
        /// Constructor for HelpProviderWithCache.
        internal HelpProviderWithCache(HelpSystem helpSystem) : base(helpSystem)
        /// _helpCache is a hashtable to stores helpInfo.
        /// This hashtable is made case-insensitive so that helpInfo can be retrieved case insensitively.
        private readonly Hashtable _helpCache = new Hashtable(StringComparer.OrdinalIgnoreCase);
        /// Exact match help for a target.
        /// <returns>The HelpInfo found. Null if nothing is found.</returns>
            if (!this.HasCustomMatch)
                if (_helpCache.Contains(target))
                    yield return (HelpInfo)_helpCache[target];
                foreach (string key in _helpCache.Keys)
                    if (CustomMatch(target, key))
                        yield return (HelpInfo)_helpCache[key];
            if (!this.CacheFullyLoaded)
                DoExactMatchHelp(helpRequest);
        /// This is for child class to indicate that it has implemented
        /// a custom way of match.
        protected bool HasCustomMatch { get; set; } = false;
        /// This is for implementing custom match algorithm.
        /// <param name="target">Target to search.</param>
        /// <param name="key">Key used in cache table.</param>
        protected virtual bool CustomMatch(string target, string key)
            return target == key;
        /// Do exact match help for a target.
        /// Derived class can choose to either override ExactMatchHelp method to DoExactMatchHelp method.
        /// If ExactMatchHelp is overridden, initial cache checking will be disabled by default.
        /// If DoExactMatchHelp is overridden, cache check will be done first in ExactMatchHelp before the
        /// logic in DoExactMatchHelp is in place.
        internal virtual void DoExactMatchHelp(HelpRequest helpRequest)
        /// Search help for a target.
            string wildcardpattern = GetWildCardPattern(target);
            HelpRequest searchHelpRequest = helpRequest.Clone();
            searchHelpRequest.Target = wildcardpattern;
                IEnumerable<HelpInfo> result = DoSearchHelp(searchHelpRequest);
                    foreach (HelpInfo helpInfoToReturn in result)
                WildcardPattern helpMatcher = WildcardPattern.Get(wildcardpattern, WildcardOptions.IgnoreCase);
                    if ((!searchOnlyContent && helpMatcher.IsMatch(key)) ||
                        (searchOnlyContent && ((HelpInfo)_helpCache[key]).MatchPatternInContent(helpMatcher)))
                        if (helpRequest.MaxResults > 0 && countOfHelpInfoObjectsFound >= helpRequest.MaxResults)
        /// Create a wildcard pattern based on a target.
        /// Here we provide the default implementation of this, covering following
        /// two cases
        ///     a. if target has wildcard pattern, return as it is.
        ///     b. if target doesn't have wildcard pattern, postfix it with *
        /// Child class of this one may choose to override this function.
        /// <param name="target">Target string.</param>
        /// <returns>Wild card pattern created.</returns>
        internal virtual string GetWildCardPattern(string target)
            if (WildcardPattern.ContainsWildcardCharacters(target))
            return "*" + target + "*";
        /// Do search help. This is for child class to override.
        /// Child class can choose to override SearchHelp of DoSearchHelp depending on
        /// whether it want to reuse the logic in SearchHelp for this class.
        internal virtual IEnumerable<HelpInfo> DoSearchHelp(HelpRequest helpRequest)
        /// Add an help entry to cache.
        /// <param name="target">The key of the help entry.</param>
        /// <param name="helpInfo">HelpInfo object as the value of the help entry.</param>
        internal void AddCache(string target, HelpInfo helpInfo)
            _helpCache[target] = helpInfo;
        /// Get help entry from cache.
        /// <param name="target">The key for the help entry to retrieve.</param>
        /// <returns>The HelpInfo in cache corresponding the key specified.</returns>
        internal HelpInfo GetCache(string target)
            return (HelpInfo)_helpCache[target];
        /// Is cached fully loaded?
        /// If cache is fully loaded, search/exactmatch Help can short cut the logic
        /// in various help providers to get help directly from cache.
        /// This indicator is usually set by help providers derived from this class.
        protected internal bool CacheFullyLoaded { get; set; } = false;
            _helpCache.Clear();
            CacheFullyLoaded = false;

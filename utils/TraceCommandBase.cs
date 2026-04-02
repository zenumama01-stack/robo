    /// A base class for cmdlets that has helper methods for globbing trace source instances.
    public class TraceCommandBase : PSCmdlet
        /// Gets the matching PSTraceSource instances for the specified patterns.
        /// <param name="patternsToMatch">
        /// The patterns used to match the PSTraceSource name.
        /// <param name="writeErrorIfMatchNotFound">
        /// If true and the pattern does not contain wildcard patterns and no
        /// match is found, then WriteError will be called.
        /// A collection of the matching PSTraceSource instances.
        internal Collection<PSTraceSource> GetMatchingTraceSource(
            string[] patternsToMatch,
            bool writeErrorIfMatchNotFound)
            Collection<string> ignored = null;
            return GetMatchingTraceSource(patternsToMatch, writeErrorIfMatchNotFound, out ignored);
        /// <param name="notMatched">
        /// The patterns for which a match was not found.
            bool writeErrorIfMatchNotFound,
            out Collection<string> notMatched)
            notMatched = new Collection<string>();
            Collection<PSTraceSource> results = new();
            foreach (string patternToMatch in patternsToMatch)
                if (string.IsNullOrEmpty(patternToMatch))
                    notMatched.Add(patternToMatch);
                WildcardPattern pattern =
                        patternToMatch,
                Dictionary<string, PSTraceSource> traceCatalog = PSTraceSource.TraceCatalog;
                foreach (PSTraceSource source in traceCatalog.Values)
                    // Try matching by full name
                    if (pattern.IsMatch(source.FullName))
                        results.Add(source);
                    // Try matching by the short name.
                    else if (pattern.IsMatch(source.Name))
                if (!matchFound)
                    // Only write an error if no match was found, the pattern doesn't
                    // contain wildcard characters, and caller wants us to.
                    if (writeErrorIfMatchNotFound &&
                        !WildcardPattern.ContainsWildcardCharacters(patternToMatch))
                                "TraceSourceNotFound",
                                SessionStateStrings.TraceSourceNotFound);
                        ErrorRecord errorRecord = new(itemNotFound.ErrorRecord, itemNotFound);

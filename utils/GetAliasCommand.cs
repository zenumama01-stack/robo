    /// The implementation of the "get-alias" cmdlet.
    [Cmdlet(VerbsCommon.Get, "Alias", DefaultParameterSetName = "Default", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096702")]
    public class GetAliasCommand : PSCmdlet
        [Parameter(ParameterSetName = "Default", Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        /// The Exclude parameter for the command.
            get { return _excludes; }
            set { _excludes = value ?? Array.Empty<string>(); }
        private string[] _excludes = Array.Empty<string>();
        /// Parameter definition to retrieve aliases based on their definitions.
        [Parameter(ParameterSetName = "Definition")]
        public string[] Definition { get; set; }
            if (ParameterSetName.Equals("Definition"))
                foreach (string defn in Definition)
                    WriteMatches(defn, "Definition");
                    WriteMatches(aliasName, "Default");
        private void WriteMatches(string value, string parametersetname)
            // get the command origin
            string displayString = "name";
            bool matchfound = false;
            bool ContainsWildcard = WildcardPattern.ContainsWildcardCharacters(value);
            WildcardPattern wcPattern = WildcardPattern.Get(value, WildcardOptions.IgnoreCase);
            // excluding patter for Default paramset.
            Collection<WildcardPattern> excludePatterns =
                      SessionStateUtilities.CreateWildcardsFromStrings(
                          _excludes,
            List<AliasInfo> results = new();
                if (parametersetname.Equals("Definition", StringComparison.OrdinalIgnoreCase))
                    displayString = "definition";
                    if (!wcPattern.IsMatch(tableEntry.Value.Definition))
                    if (SessionStateUtilities.MatchesAnyWildcardPattern(tableEntry.Value.Definition, excludePatterns, false))
                    if (!wcPattern.IsMatch(tableEntry.Key))
                    // excludes pattern
                    if (SessionStateUtilities.MatchesAnyWildcardPattern(tableEntry.Key, excludePatterns, false))
                if (ContainsWildcard)
                    // Only write the command if it is visible to the requestor
                        matchfound = true;
                        results.Add(tableEntry.Value);
                    // For specifically named elements, generate an error for elements that aren't visible...
                        SessionState.ThrowIfNotVisible(origin, tableEntry.Value);
                        // Even though it resulted in an error, a result was found
                        // so we don't want to generate the nothing found error
                        // at the end...
            results.Sort(
                static (AliasInfo left, AliasInfo right) => StringComparer.CurrentCultureIgnoreCase.Compare(left.Name, right.Name));
            foreach (AliasInfo alias in results)
                this.WriteObject(alias);
            if (!matchfound && !ContainsWildcard && (excludePatterns == null || excludePatterns.Count == 0))
                // tat doesn't exist and they are not globbing.
                ItemNotFoundException itemNotFound = new(StringUtil.Format(AliasCommandStrings.NoAliasFound, displayString, value));
                ErrorRecord er = new(itemNotFound, "ItemNotFoundException", ErrorCategory.ObjectNotFound, value);

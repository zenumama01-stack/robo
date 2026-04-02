    /// Implements Get-ExperimentalFeature cmdlet.
    [Cmdlet(VerbsCommon.Get, "ExperimentalFeature", HelpUri = "https://go.microsoft.com/fwlink/?linkid=2096786")]
    [OutputType(typeof(ExperimentalFeature))]
    public class GetExperimentalFeatureCommand : PSCmdlet
        /// Get and set the feature names.
        [Parameter(ValueFromPipeline = true, Position = 0)]
            IEnumerable<WildcardPattern> namePatterns = SessionStateUtilities.CreateWildcardsFromStrings(Name, wildcardOptions);
            foreach (ExperimentalFeature feature in GetAvailableExperimentalFeatures(namePatterns).OrderBy(GetSortingString))
                WriteObject(feature);
        /// Construct the string for sorting experimental feature records.
        /// Engine features come before module features.
        /// Within engine features and module features, features are ordered by name.
        private static (int, string) GetSortingString(ExperimentalFeature feature)
            return ExperimentalFeature.EngineSource.Equals(feature.Source, StringComparison.OrdinalIgnoreCase)
                        ? (0, feature.Name)
                        : (1, feature.Name);
        /// Get available experimental features based on the specified name patterns.
        internal IEnumerable<ExperimentalFeature> GetAvailableExperimentalFeatures(IEnumerable<WildcardPattern> namePatterns)
            foreach (ExperimentalFeature feature in ExperimentalFeature.EngineExperimentalFeatures)
                if (SessionStateUtilities.MatchesAnyWildcardPattern(feature.Name, namePatterns, defaultValue: true))
                    yield return feature;
            foreach (string moduleFile in GetValidModuleFiles(moduleNamesToFind: null))
                ExperimentalFeature[] features = ModuleIntrinsics.GetExperimentalFeature(moduleFile);
                foreach (var feature in features)
        /// Get valid module files from module paths.
        private IEnumerable<string> GetValidModuleFiles(HashSet<string> moduleNamesToFind)
            var modulePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string path in ModuleIntrinsics.GetModulePath(includeSystemModulePath: false, Context))
                string uniquePath = path.TrimEnd(Utils.Separators.Directory);
                if (!modulePaths.Add(uniquePath))
                foreach (string moduleFile in ModuleUtils.GetDefaultAvailableModuleFiles(uniquePath))
                    // We only care about module manifest files because that's where experimental features are declared.
                    if (!moduleFile.EndsWith(StringLiterals.PowerShellDataFileExtension, StringComparison.OrdinalIgnoreCase))
                    if (moduleNamesToFind != null)
                        string currentModuleName = ModuleIntrinsics.GetModuleName(moduleFile);
                        if (!moduleNamesToFind.Contains(currentModuleName))
                    yield return moduleFile;

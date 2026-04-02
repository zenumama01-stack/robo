    /// Base class for Enable/Disable-ExperimentalFeature cmdlet.
    public class EnableDisableExperimentalFeatureCommandBase : PSCmdlet
        /// Gets or sets the feature names.
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, Mandatory = true)]
        [ArgumentCompleter(typeof(ExperimentalFeatureNameCompleter))]
        /// Gets or sets the scope of persistence of updating the PowerShell configuration json.
        public ConfigScope Scope { get; set; } = ConfigScope.CurrentUser;
            WriteWarning(ExperimentalFeatureStrings.ExperimentalFeaturePending);
    /// Implements Enable-ExperimentalFeature cmdlet.
    [Cmdlet(VerbsLifecycle.Enable, "ExperimentalFeature", SupportsShouldProcess = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2046964")]
    public class EnableExperimentalFeatureCommand : EnableDisableExperimentalFeatureCommandBase
        /// ProcessRecord method of this cmdlet.
            ExperimentalFeatureConfigHelper.UpdateConfig(this, Name, Scope, enable: true);
    [Cmdlet(VerbsLifecycle.Disable, "ExperimentalFeature", SupportsShouldProcess = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2046963")]
    public class DisableExperimentalFeatureCommand : EnableDisableExperimentalFeatureCommandBase
            ExperimentalFeatureConfigHelper.UpdateConfig(this, Name, Scope, enable: false);
    internal static class ExperimentalFeatureConfigHelper
        internal static void UpdateConfig(PSCmdlet cmdlet, string[] name, ConfigScope scope, bool enable)
            IEnumerable<WildcardPattern> namePatterns = SessionStateUtilities.CreateWildcardsFromStrings(name, WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant);
            GetExperimentalFeatureCommand getExperimentalFeatureCommand = new GetExperimentalFeatureCommand();
            getExperimentalFeatureCommand.Context = cmdlet.Context;
            bool foundFeature = false;
            foreach (ExperimentalFeature feature in getExperimentalFeatureCommand.GetAvailableExperimentalFeatures(namePatterns))
                foundFeature = true;
                if (!cmdlet.ShouldProcess(feature.Name))
                PowerShellConfig.Instance.SetExperimentalFeatures(scope, feature.Name, enable);
            if (!foundFeature)
                string errMsg = string.Format(CultureInfo.InvariantCulture, ExperimentalFeatureStrings.ExperimentalFeatureNameNotFound, name);
                cmdlet.WriteError(new ErrorRecord(new ItemNotFoundException(errMsg), "ItemNotFoundException", ErrorCategory.ObjectNotFound, name));
    /// Provides argument completion for ExperimentalFeature names.
    public class ExperimentalFeatureNameCompleter : IArgumentCompleter
        /// Returns completion results for experimental feature names used as arguments to experimental feature cmdlets.
            SortedSet<string> expirmentalFeatures = new(StringComparer.OrdinalIgnoreCase);
            foreach (ExperimentalFeature feature in GetExperimentalFeatures())
                expirmentalFeatures.Add(feature.Name);
            return CompletionHelpers.GetMatchingResults(wordToComplete, expirmentalFeatures);
        private static Collection<ExperimentalFeature> GetExperimentalFeatures()
            ps.AddCommand("Get-ExperimentalFeature");
            return ps.Invoke<ExperimentalFeature>();

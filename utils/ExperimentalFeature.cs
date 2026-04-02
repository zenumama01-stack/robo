    /// Support experimental features in PowerShell.
    public class ExperimentalFeature
        #region Const Members
        internal const string EngineSource = "PSEngine";
        internal const string PSSerializeJSONLongEnumAsNumber = nameof(PSSerializeJSONLongEnumAsNumber);
        internal const string PSProfileDSCResource = "PSProfileDSCResource";
        #region Instance Members
        /// Name of an experimental feature.
        /// Description of an experimental feature.
        /// Source of an experimental feature.
        /// Indicate whether the feature is enabled.
        public bool Enabled { get; private set; }
        /// Initializes a new instance of the <see cref="ExperimentalFeature"/> class.
        /// <param name="name">The name of the experimental feature.</param>
        /// <param name="description">A description of the experimental feature.</param>
        /// <param name="source">The source where the experimental feature is defined.</param>
        /// <param name="isEnabled">Indicate whether the experimental feature is enabled.</param>
        internal ExperimentalFeature(string name, string description, string source, bool isEnabled)
            Enabled = isEnabled;
        /// This is a private constructor only for declaring new experimental features within this type.
        private ExperimentalFeature(string name, string description)
            : this(name, description, source: EngineSource, isEnabled: false)
        #region Static Members
        /// All available engine experimental features.
        internal static readonly ReadOnlyCollection<ExperimentalFeature> EngineExperimentalFeatures;
        /// A dictionary of all available engine experimental features. Feature name is the key.
        internal static readonly ReadOnlyDictionary<string, ExperimentalFeature> EngineExperimentalFeatureMap;
        /// Experimental feature names that are enabled in the config file.
        internal static readonly ReadOnlyBag<string> EnabledExperimentalFeatureNames;
        /// Type initializer. Initialize the engine experimental feature list.
        static ExperimentalFeature()
            // Initialize the readonly collection 'EngineExperimentalFeatures'.
            var engineFeatures = new ExperimentalFeature[] {
                /* Register engine experimental features here. Follow the same pattern as the example:
                new ExperimentalFeature(
                    name: "PSFileSystemProviderV2",
                    description: "Replace the old FileSystemProvider with cleaner design and faster code"),
                    name: "PSLoadAssemblyFromNativeCode",
                    description: "Expose an API to allow assembly loading from native code"),
                    name: PSSerializeJSONLongEnumAsNumber,
                    description: "Serialize enums based on long or ulong as an numeric value rather than the string representation when using ConvertTo-Json."
                    name: PSProfileDSCResource,
                    description: "DSC v3 resources for managing PowerShell profile."
            EngineExperimentalFeatures = new ReadOnlyCollection<ExperimentalFeature>(engineFeatures);
            // Initialize the readonly dictionary 'EngineExperimentalFeatureMap'.
            var engineExpFeatureMap = engineFeatures.ToDictionary(static f => f.Name, StringComparer.OrdinalIgnoreCase);
            EngineExperimentalFeatureMap = new ReadOnlyDictionary<string, ExperimentalFeature>(engineExpFeatureMap);
            // Initialize the readonly hashset 'EnabledExperimentalFeatureNames'.
            // The initialization of 'EnabledExperimentalFeatureNames' is deliberately made in the type initializer so that:
            //   1. 'EnabledExperimentalFeatureNames' can be declared as readonly;
            //   2. No need to deal with initialization from multiple threads;
            //   3. We don't need to decide where/when to read the config file for the enabled experimental features,
            //      instead, it will be done when the type is used for the first time, which is always earlier than
            //      any experimental features take effect.
            string[] enabledFeatures = Array.Empty<string>();
                enabledFeatures = PowerShellConfig.Instance.GetExperimentalFeatures();
            catch (Exception e) when (LogException(e)) { }
            EnabledExperimentalFeatureNames = ProcessEnabledFeatures(enabledFeatures);
        /// We need to notify which features were not enabled.
        private static void SendTelemetryForDeactivatedFeatures(ReadOnlyBag<string> enabledFeatures)
            foreach (var feature in EngineExperimentalFeatures)
                if (!enabledFeatures.Contains(feature.Name))
                    ApplicationInsightsTelemetry.SendTelemetryMetric(TelemetryType.ExperimentalEngineFeatureDeactivation, feature.Name);
        /// Process the array of enabled feature names retrieved from configuration.
        /// Ignore invalid feature names and unavailable engine feature names, and
        /// return an ReadOnlyBag of the valid enabled feature names.
        private static ReadOnlyBag<string> ProcessEnabledFeatures(string[] enabledFeatures)
            if (enabledFeatures.Length == 0)
                return ReadOnlyBag<string>.Empty;
            var list = new List<string>(enabledFeatures.Length);
            foreach (string name in enabledFeatures)
                if (IsModuleFeatureName(name))
                    list.Add(name);
                    ApplicationInsightsTelemetry.SendTelemetryMetric(TelemetryType.ExperimentalModuleFeatureActivation, name);
                else if (IsEngineFeatureName(name))
                    if (EngineExperimentalFeatureMap.TryGetValue(name, out ExperimentalFeature feature))
                        feature.Enabled = true;
                        ApplicationInsightsTelemetry.SendTelemetryMetric(TelemetryType.ExperimentalEngineFeatureActivation, name);
                        string message = StringUtil.Format(Logging.EngineExperimentalFeatureNotFound, name);
                        LogError(PSEventId.ExperimentalFeature_InvalidName, name, message);
                    string message = StringUtil.Format(Logging.InvalidExperimentalFeatureName, name);
            ReadOnlyBag<string> features = new(new HashSet<string>(list, StringComparer.OrdinalIgnoreCase));
            SendTelemetryForDeactivatedFeatures(features);
        /// Log the exception without rewinding the stack.
        private static bool LogException(Exception e)
            LogError(PSEventId.ExperimentalFeature_ReadConfig_Error, e.GetType().FullName, e.Message, e.StackTrace);
        /// Log an error message.
        private static void LogError(PSEventId eventId, params object[] args)
            PSEtwLog.LogOperationalError(eventId, PSOpcode.Constructor, PSTask.ExperimentalFeature, PSKeyword.UseAlwaysOperational, args);
        /// Check if the name follows the engine experimental feature name convention.
        /// Convention: prefix 'PS' to the feature name -- 'PSFeatureName'.
        internal static bool IsEngineFeatureName(string featureName)
            return featureName.Length > 2 && !featureName.Contains('.') && featureName.StartsWith("PS", StringComparison.Ordinal);
        /// Check if the name follows the module experimental feature name convention.
        /// Convention: prefix the module name to the feature name -- 'ModuleName.FeatureName'.
        /// <param name="featureName">The feature name to check.</param>
        /// <param name="moduleName">When specified, we check if the feature name matches the module name.</param>
        internal static bool IsModuleFeatureName(string featureName, string moduleName = null)
            // Feature names cannot start with a dot
            if (featureName.StartsWith('.'))
            // Feature names must contain a dot, but not at the end
            int lastDotIndex = featureName.LastIndexOf('.');
            if (lastDotIndex == -1 || lastDotIndex == featureName.Length - 1)
            // If the module name is given, it must match the prefix of the feature name (up to the last dot).
            var moduleNamePart = featureName.AsSpan(0, lastDotIndex);
            return moduleNamePart.Equals(moduleName.AsSpan(), StringComparison.OrdinalIgnoreCase);
        /// Determine the action to take for the specified experiment name and action.
        internal static ExperimentAction GetActionToTake(string experimentName, ExperimentAction experimentAction)
            if (experimentName == null || experimentAction == ExperimentAction.None)
                // If either the experiment name or action is not defined, then return 'Show' by default.
                // This could happen to 'ParameterAttribute' when no experimental related field is declared.
                return ExperimentAction.Show;
            ExperimentAction action = experimentAction;
            if (!IsEnabled(experimentName))
                action = (action == ExperimentAction.Hide) ? ExperimentAction.Show : ExperimentAction.Hide;
            return action;
        /// Check if the specified experimental feature has been enabled.
        public static bool IsEnabled(string featureName)
            return EnabledExperimentalFeatureNames.Contains(featureName);
    /// Indicates the action to take on the cmdlet/parameter that has the attribute declared.
    public enum ExperimentAction
        /// Represent an undefined action, used as the default value.
        /// Hide the cmdlet/parameter when the corresponding experimental feature is enabled.
        Hide = 1,
        /// Show the cmdlet/parameter when the corresponding experimental feature is enabled.
        Show = 2
    /// The attribute that applies to cmdlet/function/parameter to define what the engine should do with it.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ExperimentalAttribute : ParsingBaseAttribute
        /// Get name of the experimental feature this attribute is associated with.
        /// Get action for engine to take when the experimental feature is enabled.
        /// Initializes a new instance of the ExperimentalAttribute class.
        public ExperimentalAttribute(string experimentName, ExperimentAction experimentAction)
            ValidateArguments(experimentName, experimentAction);
        /// Initialize an instance that represents the none-value.
        private ExperimentalAttribute() { }
        /// An instance that represents the none-value.
        internal static readonly ExperimentalAttribute None = new ExperimentalAttribute();
        /// Validate arguments for the constructor.
        internal static void ValidateArguments(string experimentName, ExperimentAction experimentAction)
            if (string.IsNullOrEmpty(experimentName))
                const string paramName = nameof(experimentName);
                throw PSTraceSource.NewArgumentNullException(paramName, Metadata.ArgumentNullOrEmpty, paramName);
            if (experimentAction == ExperimentAction.None)
                const string paramName = nameof(experimentAction);
                const string invalidMember = nameof(ExperimentAction.None);
                string validMembers = StringUtil.Format("{0}, {1}", ExperimentAction.Hide, ExperimentAction.Show);
                throw PSTraceSource.NewArgumentException(paramName, Metadata.InvalidEnumArgument, invalidMember, paramName, validMembers);
        /// Get effective action to take at run time.
        private ExperimentAction _effectiveAction = ExperimentAction.None;

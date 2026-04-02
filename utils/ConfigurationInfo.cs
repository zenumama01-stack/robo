    /// Provides information about a configuration that is stored in session state.
    public class ConfigurationInfo : FunctionInfo
        /// Creates an instance of the ConfigurationInfo class with the specified name and ScriptBlock.
        /// The name of the configuration.
        /// <param name="configuration">
        /// The ScriptBlock for the configuration
        /// The ExecutionContext for the configuration.
        /// If <paramref name="configuration"/> is null.
        internal ConfigurationInfo(string name, ScriptBlock configuration, ExecutionContext context) : this(name, configuration, context, null)
        /// The help file for the configuration.
        internal ConfigurationInfo(string name, ScriptBlock configuration, ExecutionContext context, string helpFile)
            : base(name, configuration, context, helpFile)
            SetCommandType(CommandTypes.Configuration);
        /// The options to set on the function. Note, Constant can only be set at creation time.
        /// The execution context for the configuration.
        internal ConfigurationInfo(string name, ScriptBlock configuration, ScopedItemOptions options, ExecutionContext context) : this(name, configuration, options, context, null)
        /// <param name="isMetaConfig">The configuration is a meta configuration.</param>
        internal ConfigurationInfo(string name, ScriptBlock configuration, ScopedItemOptions options, ExecutionContext context, string helpFile, bool isMetaConfig)
            : base(name, configuration, options, context, helpFile)
            IsMetaConfiguration = isMetaConfig;
        internal ConfigurationInfo(string name, ScriptBlock configuration, ScopedItemOptions options, ExecutionContext context, string helpFile)
            : this(name, configuration, options, context, helpFile, false)
        internal ConfigurationInfo(ConfigurationInfo other)
        internal ConfigurationInfo(string name, ConfigurationInfo other)
            : base(name, other)
            var copy = new ConfigurationInfo(this) { IsGetCommandCopy = true, Arguments = arguments };
            get { return HelpCategory.Configuration; }
        /// Indication whether the configuration is a meta-configuration.
        public bool IsMetaConfiguration
        { get; internal set; }

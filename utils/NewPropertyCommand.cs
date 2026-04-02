    /// A command to create a new property on an object.
    [Cmdlet(VerbsCommon.New, "ItemProperty", DefaultParameterSetName = "Path", SupportsShouldProcess = true, SupportsTransactions = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096813")]
    public class NewItemPropertyCommand : ItemPropertyCommandBase
        [Parameter(Position = 0, ParameterSetName = "Path", Mandatory = true)]
        /// The type of the property to create on the item.
        [ArgumentCompleter(typeof(PropertyTypeArgumentCompleter))]
        public string PropertyType { get; set; }
        /// The value of the property to create on the item.
                return InvokeProvider.Property.NewPropertyDynamicParameters(Path[0], Name, PropertyType, Value, context);
            return InvokeProvider.Property.NewPropertyDynamicParameters(".", Name, PropertyType, Value, context);
                    InvokeProvider.Property.New(path, Name, PropertyType, Value, CmdletProviderContext);
    /// Provides argument completion for PropertyType parameter.
    public class PropertyTypeArgumentCompleter : IArgumentCompleter
        private static readonly CompletionHelpers.CompletionDisplayInfoMapper RegistryPropertyTypeDisplayInfoMapper = registryPropertyType => registryPropertyType switch
            "String" => (
                ToolTip: TabCompletionStrings.RegistryStringToolTip,
                ListItemText: "String"),
            "ExpandString" => (
                ToolTip: TabCompletionStrings.RegistryExpandStringToolTip,
                ListItemText: "ExpandString"),
            "Binary" => (
                ToolTip: TabCompletionStrings.RegistryBinaryToolTip,
                ListItemText: "Binary"),
            "DWord" => (
                ToolTip: TabCompletionStrings.RegistryDWordToolTip,
                ListItemText: "DWord"),
            "MultiString" => (
                ToolTip: TabCompletionStrings.RegistryMultiStringToolTip,
                ListItemText: "MultiString"),
            "QWord" => (
                ToolTip: TabCompletionStrings.RegistryQWordToolTip,
                ListItemText: "QWord"),
            _ => (
                ToolTip: TabCompletionStrings.RegistryUnknownToolTip,
                ListItemText: "Unknown"),
        private static readonly IReadOnlyList<string> s_RegistryPropertyTypes = new List<string>(capacity: 7)
            "String",
            "ExpandString",
            "Binary",
            "DWord",
            "MultiString",
            "QWord",
            "Unknown"
        /// Returns completion results for PropertyType parameter.
        /// <param name="commandName">The command name.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="wordToComplete">The word to complete.</param>
        /// <param name="commandAst">The command AST.</param>
        /// <param name="fakeBoundParameters">The fake bound parameters.</param>
        /// <returns>List of Completion Results.</returns>
        public IEnumerable<CompletionResult> CompleteArgument(
            string commandName,
            string wordToComplete,
            CommandAst commandAst,
            IDictionary fakeBoundParameters)
                => IsRegistryProvider(fakeBoundParameters)
                    ? CompletionHelpers.GetMatchingResults(
                        wordToComplete,
                        possibleCompletionValues: s_RegistryPropertyTypes,
                        displayInfoMapper: RegistryPropertyTypeDisplayInfoMapper,
                        resultType: CompletionResultType.ParameterValue)
                    : [];
        /// Checks if parameter paths are from Registry provider.
        /// <returns>Boolean indicating if paths are from Registry Provider.</returns>
        private static bool IsRegistryProvider(IDictionary fakeBoundParameters)
            Collection<PathInfo> paths;
            if (fakeBoundParameters.Contains("Path"))
                paths = ResolvePath(fakeBoundParameters["Path"], isLiteralPath: false);
            else if (fakeBoundParameters.Contains("LiteralPath"))
                paths = ResolvePath(fakeBoundParameters["LiteralPath"], isLiteralPath: true);
                paths = ResolvePath(@".\", isLiteralPath: false);
            return paths.Count > 0 && paths[0].Provider.NameEquals("Registry");
        /// Resolve path or literal path using Resolve-Path.
        /// <param name="path">The path to resolve.</param>
        /// <param name="isLiteralPath">Specifies if path is literal path.</param>
        /// <returns>Collection of Pathinfo objects.</returns>
        private static Collection<PathInfo> ResolvePath(object path, bool isLiteralPath)
            using var ps = System.Management.Automation.PowerShell.Create(RunspaceMode.CurrentRunspace);
            ps.AddCommand("Microsoft.PowerShell.Management\\Resolve-Path");
            ps.AddParameter(isLiteralPath ? "LiteralPath" : "Path", path);
            Collection<PathInfo> output = ps.Invoke<PathInfo>();

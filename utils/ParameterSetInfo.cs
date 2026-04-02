    /// The information about a parameter set and its parameters for a cmdlet.
    public class CommandParameterSetInfo
        /// Constructs the parameter set information using the specified parameter name,
        /// and type metadata.
        /// The formal name of the parameter.
        /// <param name="isDefaultParameterSet">
        /// True if the parameter set is the default parameter set, or false otherwise.
        /// The bit that specifies the parameter set in the type metadata.
        /// The type metadata about the cmdlet.
        /// If <paramref name="parameterMetadata"/> is null.
        internal CommandParameterSetInfo(
            bool isDefaultParameterSet,
            uint parameterSetFlag,
            MergedCommandParameterMetadata parameterMetadata)
            IsDefault = true;
            Name = string.Empty;
            this.IsDefault = isDefaultParameterSet;
            Initialize(parameterMetadata, parameterSetFlag);
        public ReadOnlyCollection<CommandParameterInfo> Parameters { get; private set; }
        /// Gets the synopsis for the cmdlet as a string.
            Text.StringBuilder result = new Text.StringBuilder();
            GenerateParametersInDisplayOrder(
                parameter => AppendFormatCommandParameterInfo(parameter, result),
                (string str) =>
                    if (result.Length > 0)
                        result.Append(' ');
                    result.Append('[');
                    result.Append(str);
                    result.Append(']');
        /// GenerateParameters parameters in display order
        /// ie., Positional followed by
        ///      Named Mandatory (in alpha numeric) followed by
        ///      Named (in alpha numeric).
        /// Callers use <paramref name="parameterAction"/> and
        /// <paramref name="commonParameterAction"/> to handle
        /// syntax generation etc.
        /// <param name="parameterAction"></param>
        /// <param name="commonParameterAction"></param>
        internal void GenerateParametersInDisplayOrder(
            Action<CommandParameterInfo> parameterAction,
            Action<string> commonParameterAction)
            // First figure out the positions
            List<CommandParameterInfo> sortedPositionalParameters = new List<CommandParameterInfo>();
            List<CommandParameterInfo> namedMandatoryParameters = new List<CommandParameterInfo>();
            List<CommandParameterInfo> namedParameters = new List<CommandParameterInfo>();
            foreach (CommandParameterInfo parameter in Parameters)
                if (parameter.Position == int.MinValue)
                    // The parameter is a named parameter
                    if (parameter.IsMandatory)
                        namedMandatoryParameters.Add(parameter);
                        namedParameters.Add(parameter);
                    // The parameter is positional so add it at the correct
                    // index (note we have to pad the list if the position is
                    // higher than the list count since we don't have any requirements
                    // that positional parameters start at zero and are consecutive.
                    if (parameter.Position >= sortedPositionalParameters.Count)
                        for (int fillerIndex = sortedPositionalParameters.Count;
                             fillerIndex <= parameter.Position;
                             ++fillerIndex)
                            sortedPositionalParameters.Add(null);
                    sortedPositionalParameters[parameter.Position] = parameter;
            foreach (CommandParameterInfo parameter in sortedPositionalParameters)
                parameterAction(parameter);
            // Now convert the named mandatory parameters into a string
            foreach (CommandParameterInfo parameter in namedMandatoryParameters)
            List<CommandParameterInfo> commonParameters = new List<CommandParameterInfo>();
            // Now convert the named parameters into a string
            foreach (CommandParameterInfo parameter in namedParameters)
                // Hold off common parameters
                bool isCommon = Cmdlet.CommonParameters.Contains(parameter.Name, StringComparer.OrdinalIgnoreCase);
                if (!isCommon)
                    commonParameters.Add(parameter);
            // If all common parameters are present, group them together
            if (commonParameters.Count == Cmdlet.CommonParameters.Count)
                commonParameterAction(HelpDisplayStrings.CommonParameters);
            // Else, convert to string as before
                foreach (CommandParameterInfo parameter in commonParameters)
        private static void AppendFormatCommandParameterInfo(CommandParameterInfo parameter, Text.StringBuilder result)
                // Add a space between parameters
            if (parameter.ParameterType == typeof(SwitchParameter))
                result.AppendFormat(CultureInfo.InvariantCulture, parameter.IsMandatory ? "-{0}" : "[-{0}]", parameter.Name);
                string parameterTypeString = GetParameterTypeString(parameter.ParameterType, parameter.Attributes);
                        parameter.Position != int.MinValue ? "[-{0}] <{1}>" : "-{0} <{1}>",
                        parameterTypeString);
                        parameter.Position != int.MinValue ? "[[-{0}] <{1}>]" : "[-{0} <{1}>]",
        internal static string GetParameterTypeString(Type type, IEnumerable<Attribute> attributes)
            string parameterTypeString;
            PSTypeNameAttribute typeName;
            if (attributes != null && (typeName = attributes.OfType<PSTypeNameAttribute>().FirstOrDefault()) != null)
                // If we have a PSTypeName specified on the class, we assume it has a more useful type than the actual
                // parameter type.  This is a reasonable assumption, the parameter binder does honor this attribute.
                // This typename might be long, e.g.:
                //     Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Process
                //     System.Management.ManagementObject#root\cimv2\Win32_Process
                // To shorten this, we will drop the namespaces, both on the .Net side and the CIM/WMI side:
                //     CimInstance#Win32_Process
                // If our regex doesn't match, we'll just use the full name.
                var match = Regex.Match(typeName.PSTypeName, "(.*\\.)?(?<NetTypeName>.*)#(.*[/\\\\])?(?<CimClassName>.*)");
                    parameterTypeString = match.Groups["NetTypeName"].Value + "#" + match.Groups["CimClassName"].Value;
                    parameterTypeString = typeName.PSTypeName;
                    // Drop the namespace from the typename, if any.
                    var lastDotIndex = parameterTypeString.LastIndexOf('.');
                    if (lastDotIndex != -1 && lastDotIndex + 1 < parameterTypeString.Length)
                        parameterTypeString = parameterTypeString.Substring(lastDotIndex + 1);
                // If the type is really an array, but the typename didn't include [], then add it.
                if (type.IsArray && !parameterTypeString.Contains("[]", StringComparison.Ordinal))
                    var t = type;
                    while (t.IsArray)
                        parameterTypeString += "[]";
                        t = t.GetElementType();
                Type parameterType = Nullable.GetUnderlyingType(type) ?? type;
                parameterTypeString = ToStringCodeMethods.Type(parameterType, true);
            return parameterTypeString;
        private void Initialize(MergedCommandParameterMetadata parameterMetadata, uint parameterSetFlag)
                parameterMetadata != null,
                "The parameterMetadata should never be null");
            Collection<CommandParameterInfo> processedParameters =
                new Collection<CommandParameterInfo>();
            // Get the parameters in the parameter set
            Collection<MergedCompiledCommandParameter> compiledParameters =
                parameterMetadata.GetParametersInParameterSet(parameterSetFlag);
            foreach (MergedCompiledCommandParameter parameter in compiledParameters)
                    processedParameters.Add(
                        new CommandParameterInfo(parameter.Parameter, parameterSetFlag));
            Parameters = new ReadOnlyCollection<CommandParameterInfo>(processedParameters);

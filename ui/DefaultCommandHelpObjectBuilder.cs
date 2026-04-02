namespace System.Management.Automation.Help
    /// Positional parameter comparer.
    internal class PositionalParameterComparer : IComparer
            CommandParameterInfo a = x as CommandParameterInfo;
            CommandParameterInfo b = y as CommandParameterInfo;
            Debug.Assert(a != null && b != null);
            return (a.Position - b.Position);
    /// The help object builder class attempts to create a full HelpInfo object from
    /// a CmdletInfo object. This is used to generate the default UX when no help content
    /// is present in the box. This class mimics the exact same structure as that of a MAML
    /// node, so that the default UX does not introduce regressions.
    internal static class DefaultCommandHelpObjectBuilder
        internal static readonly string TypeNameForDefaultHelp = "ExtendedCmdletHelpInfo";
        /// Generates a HelpInfo PSObject from a CmdletInfo object.
        /// <param name="input">Command info.</param>
        /// <returns>HelpInfo PSObject.</returns>
        internal static PSObject GetPSObjectFromCmdletInfo(CommandInfo input)
            // Create a copy of commandInfo for GetCommandCommand so that we can generate parameter
            // sets based on Dynamic Parameters (+ optional arguments)
            CommandInfo commandInfo = input.CreateGetCommandCopy(null);
            obj.TypeNames.Clear();
            obj.TypeNames.Add(string.Create(CultureInfo.InvariantCulture, $"{DefaultCommandHelpObjectBuilder.TypeNameForDefaultHelp}#{commandInfo.ModuleName}#command"));
            obj.TypeNames.Add(string.Create(CultureInfo.InvariantCulture, $"{DefaultCommandHelpObjectBuilder.TypeNameForDefaultHelp}#{commandInfo.ModuleName}"));
            obj.TypeNames.Add(DefaultCommandHelpObjectBuilder.TypeNameForDefaultHelp);
            obj.TypeNames.Add("CmdletHelpInfo");
            obj.TypeNames.Add("HelpInfo");
                bool common = false;
                if (cmdletInfo.Parameters != null)
                    common = HasCommonParameters(cmdletInfo.Parameters);
                obj.Properties.Add(new PSNoteProperty("CommonParameters", common));
                AddDetailsProperties(obj, cmdletInfo.Name, cmdletInfo.Noun, cmdletInfo.Verb, TypeNameForDefaultHelp);
                AddSyntaxProperties(obj, cmdletInfo.Name, cmdletInfo.ParameterSets, common, TypeNameForDefaultHelp);
                AddParametersProperties(obj, cmdletInfo.Parameters, common, TypeNameForDefaultHelp);
                AddInputTypesProperties(obj, cmdletInfo.Parameters);
                AddRelatedLinksProperties(obj, commandInfo.CommandMetadata.HelpUri);
                    AddOutputTypesProperties(obj, cmdletInfo.OutputType);
                    AddOutputTypesProperties(obj, new ReadOnlyCollection<PSTypeName>(new List<PSTypeName>()));
                AddAliasesProperties(obj, cmdletInfo.Name, cmdletInfo.Context);
                if (HasHelpInfoUri(cmdletInfo.Module, cmdletInfo.ModuleName))
                    AddRemarksProperties(obj, cmdletInfo.Name, cmdletInfo.CommandMetadata.HelpUri);
                    obj.Properties.Add(new PSNoteProperty("remarks", HelpDisplayStrings.None));
                obj.Properties.Add(new PSNoteProperty("PSSnapIn", cmdletInfo.PSSnapIn));
            else if (commandInfo is FunctionInfo funcInfo)
                bool common = HasCommonParameters(funcInfo.Parameters);
                AddDetailsProperties(obj, funcInfo.Name, string.Empty, string.Empty, TypeNameForDefaultHelp);
                AddSyntaxProperties(obj, funcInfo.Name, funcInfo.ParameterSets, common, TypeNameForDefaultHelp);
                AddParametersProperties(obj, funcInfo.Parameters, common, TypeNameForDefaultHelp);
                AddInputTypesProperties(obj, funcInfo.Parameters);
                AddRelatedLinksProperties(obj, funcInfo.CommandMetadata.HelpUri);
                    AddOutputTypesProperties(obj, funcInfo.OutputType);
                AddAliasesProperties(obj, funcInfo.Name, funcInfo.Context);
                if (HasHelpInfoUri(funcInfo.Module, funcInfo.ModuleName))
                    AddRemarksProperties(obj, funcInfo.Name, funcInfo.CommandMetadata.HelpUri);
            obj.Properties.Add(new PSNoteProperty("alertSet", null));
            obj.Properties.Add(new PSNoteProperty("description", null));
            obj.Properties.Add(new PSNoteProperty("examples", null));
            obj.Properties.Add(new PSNoteProperty("Synopsis", commandInfo.Syntax));
            obj.Properties.Add(new PSNoteProperty("ModuleName", commandInfo.ModuleName));
            obj.Properties.Add(new PSNoteProperty("nonTerminatingErrors", string.Empty));
            obj.Properties.Add(new PSNoteProperty("xmlns:command", "http://schemas.microsoft.com/maml/dev/command/2004/10"));
            obj.Properties.Add(new PSNoteProperty("xmlns:dev", "http://schemas.microsoft.com/maml/dev/2004/10"));
            obj.Properties.Add(new PSNoteProperty("xmlns:maml", "http://schemas.microsoft.com/maml/2004/10"));
        /// Adds the details properties.
        /// <param name="obj">HelpInfo object.</param>
        /// <param name="name">Command name.</param>
        /// <param name="noun">Command noun.</param>
        /// <param name="verb">Command verb.</param>
        /// <param name="typeNameForHelp">Type name for help.</param>
        /// <param name="synopsis">Synopsis.</param>
        internal static void AddDetailsProperties(PSObject obj, string name, string noun, string verb, string typeNameForHelp,
            string synopsis = null)
            mshObject.TypeNames.Clear();
            mshObject.TypeNames.Add(string.Create(CultureInfo.InvariantCulture, $"{typeNameForHelp}#details"));
            mshObject.Properties.Add(new PSNoteProperty("name", name));
            mshObject.Properties.Add(new PSNoteProperty("noun", noun));
            mshObject.Properties.Add(new PSNoteProperty("verb", verb));
            // add synopsis
            if (!string.IsNullOrEmpty(synopsis))
                PSObject descriptionObject = new PSObject();
                descriptionObject.TypeNames.Clear();
                descriptionObject.TypeNames.Add("MamlParaTextItem");
                descriptionObject.Properties.Add(new PSNoteProperty("Text", synopsis));
                mshObject.Properties.Add(new PSNoteProperty("Description", descriptionObject));
            obj.Properties.Add(new PSNoteProperty("details", mshObject));
        /// Adds the syntax properties.
        /// <param name="cmdletName">Command name.</param>
        /// <param name="parameterSets">Parameter sets.</param>
        /// <param name="common">Common parameters.</param>
        internal static void AddSyntaxProperties(PSObject obj, string cmdletName, ReadOnlyCollection<CommandParameterSetInfo> parameterSets, bool common, string typeNameForHelp)
            mshObject.TypeNames.Add(string.Create(CultureInfo.InvariantCulture, $"{typeNameForHelp}#syntax"));
            AddSyntaxItemProperties(mshObject, cmdletName, parameterSets, common, typeNameForHelp);
            obj.Properties.Add(new PSNoteProperty("Syntax", mshObject));
        /// Add the syntax item properties.
        /// <param name="cmdletName">Cmdlet name, you can't get this from parameterSets.</param>
        /// <param name="parameterSets">A collection of parameter sets.</param>
        private static void AddSyntaxItemProperties(PSObject obj, string cmdletName, ReadOnlyCollection<CommandParameterSetInfo> parameterSets, bool common, string typeNameForHelp)
            ArrayList mshObjects = new ArrayList();
            foreach (CommandParameterSetInfo parameterSet in parameterSets)
                mshObject.TypeNames.Add(string.Create(CultureInfo.InvariantCulture, $"{typeNameForHelp}#syntaxItem"));
                mshObject.Properties.Add(new PSNoteProperty("name", cmdletName));
                mshObject.Properties.Add(new PSNoteProperty("CommonParameters", common));
                Collection<CommandParameterInfo> parameters = new Collection<CommandParameterInfo>();
                // GenerateParameters parameters in display order
                // ie., Positional followed by
                //      Named Mandatory (in alpha numeric) followed by
                //      Named (in alpha numeric)
                parameterSet.GenerateParametersInDisplayOrder(parameters.Add, delegate { });
                AddSyntaxParametersProperties(mshObject, parameters, common, parameterSet.Name);
                mshObjects.Add(mshObject);
            obj.Properties.Add(new PSNoteProperty("syntaxItem", mshObjects.ToArray()));
        /// Add the syntax parameters properties (these parameters are used to create the syntax section)
        /// a collection of parameters in display order
        ///      Named (in alpha numeric)
        /// <param name="parameterSetName">Name of the parameter set for which the syntax is generated.</param>
        private static void AddSyntaxParametersProperties(PSObject obj, IEnumerable<CommandParameterInfo> parameters,
            bool common, string parameterSetName)
                if (common && Cmdlet.CommonParameters.Contains(parameter.Name))
                mshObject.TypeNames.Add(string.Create(CultureInfo.InvariantCulture, $"{DefaultCommandHelpObjectBuilder.TypeNameForDefaultHelp}#parameter"));
                Collection<Attribute> attributes = new Collection<Attribute>(parameter.Attributes);
                AddParameterProperties(mshObject, parameter.Name, new Collection<string>(parameter.Aliases),
                    parameter.IsDynamic, parameter.ParameterType, attributes, parameterSetName);
                Collection<ValidateSetAttribute> validateSet = GetValidateSetAttribute(attributes);
                List<string> names = new List<string>();
                foreach (ValidateSetAttribute set in validateSet)
                    foreach (string value in set.ValidValues)
                        names.Add(value);
                if (names.Count != 0)
                    AddParameterValueGroupProperties(mshObject, names.ToArray());
                    if (parameter.ParameterType.IsEnum && (Enum.GetNames(parameter.ParameterType) != null))
                        AddParameterValueGroupProperties(mshObject, Enum.GetNames(parameter.ParameterType));
                    else if (parameter.ParameterType.IsArray)
                        if (parameter.ParameterType.GetElementType().IsEnum &&
                            Enum.GetNames(parameter.ParameterType.GetElementType()) != null)
                            AddParameterValueGroupProperties(mshObject, Enum.GetNames(parameter.ParameterType.GetElementType()));
                    else if (parameter.ParameterType.IsGenericType)
                        Type[] types = parameter.ParameterType.GetGenericArguments();
                        if (types.Length != 0)
                            Type type = types[0];
                            if (type.IsEnum && (Enum.GetNames(type) != null))
                                AddParameterValueGroupProperties(mshObject, Enum.GetNames(type));
                                if (type.GetElementType().IsEnum &&
                                    Enum.GetNames(type.GetElementType()) != null)
                                    AddParameterValueGroupProperties(mshObject, Enum.GetNames(type.GetElementType()));
            obj.Properties.Add(new PSNoteProperty("parameter", mshObjects.ToArray()));
        /// Adds a parameter value group (for enums)
        /// <param name="obj">Object.</param>
        /// <param name="values">Parameter group values.</param>
        private static void AddParameterValueGroupProperties(PSObject obj, string[] values)
            PSObject paramValueGroup = new PSObject();
            paramValueGroup.TypeNames.Clear();
            paramValueGroup.TypeNames.Add(string.Create(CultureInfo.InvariantCulture, $"{DefaultCommandHelpObjectBuilder.TypeNameForDefaultHelp}#parameterValueGroup"));
            ArrayList paramValue = new ArrayList(values);
            paramValueGroup.Properties.Add(new PSNoteProperty("parameterValue", paramValue.ToArray()));
            obj.Properties.Add(new PSNoteProperty("parameterValueGroup", paramValueGroup));
        /// Add the parameters properties (these parameters are used to create the parameters section)
        /// <param name="parameters">Parameters.</param>
        internal static void AddParametersProperties(PSObject obj, Dictionary<string, ParameterMetadata> parameters, bool common, string typeNameForHelp)
            PSObject paramsObject = new PSObject();
            paramsObject.TypeNames.Clear();
            paramsObject.TypeNames.Add(string.Create(CultureInfo.InvariantCulture, $"{typeNameForHelp}#parameters"));
            ArrayList paramObjects = new ArrayList();
            ArrayList sortedParameters = new ArrayList();
                    sortedParameters.Add(parameter.Key);
            sortedParameters.Sort(StringComparer.Ordinal);
            foreach (string parameter in sortedParameters)
                if (common && Cmdlet.CommonParameters.Contains(parameter))
                PSObject paramObject = new PSObject();
                paramObject.TypeNames.Clear();
                paramObject.TypeNames.Add(string.Create(CultureInfo.InvariantCulture, $"{DefaultCommandHelpObjectBuilder.TypeNameForDefaultHelp}#parameter"));
                AddParameterProperties(paramObject, parameter, parameters[parameter].Aliases,
                    parameters[parameter].IsDynamic, parameters[parameter].ParameterType, parameters[parameter].Attributes);
                paramObjects.Add(paramObject);
            paramsObject.Properties.Add(new PSNoteProperty("parameter", paramObjects.ToArray()));
            obj.Properties.Add(new PSNoteProperty("parameters", paramsObject));
        /// Adds the parameter properties.
        /// <param name="aliases">Parameter aliases.</param>
        /// <param name="dynamic">Is dynamic parameter?</param>
        /// <param name="type">Parameter type.</param>
        /// <param name="attributes">Parameter attributes.</param>
        private static void AddParameterProperties(PSObject obj, string name, Collection<string> aliases, bool dynamic,
            Type type, Collection<Attribute> attributes, string parameterSetName = null)
            Collection<ParameterAttribute> attribs = GetParameterAttribute(attributes);
            obj.Properties.Add(new PSNoteProperty("name", name));
            if (attribs.Count == 0)
                obj.Properties.Add(new PSNoteProperty("required", string.Empty));
                obj.Properties.Add(new PSNoteProperty("pipelineInput", string.Empty));
                obj.Properties.Add(new PSNoteProperty("isDynamic", string.Empty));
                obj.Properties.Add(new PSNoteProperty("parameterSetName", string.Empty));
                obj.Properties.Add(new PSNoteProperty("description", string.Empty));
                obj.Properties.Add(new PSNoteProperty("position", string.Empty));
                obj.Properties.Add(new PSNoteProperty("aliases", string.Empty));
                obj.Properties.Add(new PSNoteProperty("globbing", string.Empty));
                ParameterAttribute paramAttribute = attribs[0];
                    foreach (var attrib in attribs)
                        if (string.Equals(attrib.ParameterSetName, parameterSetName, StringComparison.OrdinalIgnoreCase))
                            paramAttribute = attrib;
                obj.Properties.Add(new PSNoteProperty("required", CultureInfo.CurrentCulture.TextInfo.ToLower(paramAttribute.Mandatory.ToString())));
                obj.Properties.Add(new PSNoteProperty("pipelineInput", GetPipelineInputString(paramAttribute)));
                obj.Properties.Add(new PSNoteProperty("isDynamic", CultureInfo.CurrentCulture.TextInfo.ToLower(dynamic.ToString())));
                AddParameterGlobbingProperties(obj, attributes);
                if (paramAttribute.ParameterSetName.Equals(ParameterAttribute.AllParameterSets, StringComparison.OrdinalIgnoreCase))
                    obj.Properties.Add(new PSNoteProperty("parameterSetName", StringUtil.Format(HelpDisplayStrings.AllParameterSetsName)));
                    for (int i = 0; i < attribs.Count; i++)
                        sb.Append(attribs[i].ParameterSetName);
                        if (i != (attribs.Count - 1))
                    obj.Properties.Add(new PSNoteProperty("parameterSetName", sb.ToString()));
                if (paramAttribute.HelpMessage != null)
                    sb.AppendLine(paramAttribute.HelpMessage);
                    obj.Properties.Add(new PSNoteProperty("description", sb.ToString()));
                // We do not show switch parameters in the syntax section
                // (i.e. [-Syntax] not [-Syntax <SwitchParameter>]
                if (type != typeof(SwitchParameter))
                    AddParameterValueProperties(obj, type, attributes);
                AddParameterTypeProperties(obj, type, attributes);
                if (paramAttribute.Position == int.MinValue)
                    obj.Properties.Add(new PSNoteProperty("position",
                        StringUtil.Format(HelpDisplayStrings.NamedParameter)));
                        paramAttribute.Position.ToString(CultureInfo.InvariantCulture)));
                if (aliases.Count == 0)
                    obj.Properties.Add(new PSNoteProperty("aliases", StringUtil.Format(
                        HelpDisplayStrings.None)));
                    for (int i = 0; i < aliases.Count; i++)
                        sb.Append(aliases[i]);
                        if (i != (aliases.Count - 1))
                    obj.Properties.Add(new PSNoteProperty("aliases", sb.ToString()));
        /// Adds the globbing properties.
        /// <param name="attributes">The attributes of the parameter (needed to look for PSTypeName).</param>
        private static void AddParameterGlobbingProperties(PSObject obj, IEnumerable<Attribute> attributes)
            bool globbing = false;
                if (attrib is SupportsWildcardsAttribute)
                    globbing = true;
            obj.Properties.Add(new PSNoteProperty("globbing", CultureInfo.CurrentCulture.TextInfo.ToLower(globbing.ToString())));
        /// Adds the parameterType properties.
        /// <param name="parameterType">The type of a parameter.</param>
        private static void AddParameterTypeProperties(PSObject obj, Type parameterType, IEnumerable<Attribute> attributes)
            mshObject.TypeNames.Add(string.Create(CultureInfo.InvariantCulture, $"{DefaultCommandHelpObjectBuilder.TypeNameForDefaultHelp}#type"));
            var parameterTypeString = CommandParameterSetInfo.GetParameterTypeString(parameterType, attributes);
            mshObject.Properties.Add(new PSNoteProperty("name", parameterTypeString));
            obj.Properties.Add(new PSNoteProperty("type", mshObject));
        /// Adds the parameterValue properties.
        private static void AddParameterValueProperties(PSObject obj, Type parameterType, IEnumerable<Attribute> attributes)
            PSObject mshObject;
            if (parameterType != null)
                Type type = Nullable.GetUnderlyingType(parameterType) ?? parameterType;
                mshObject = new PSObject(parameterTypeString);
                mshObject.Properties.Add(new PSNoteProperty("variableLength", parameterType.IsArray));
                mshObject = new PSObject("System.Object");
                mshObject.Properties.Add(new PSNoteProperty("variableLength",
                    StringUtil.Format(HelpDisplayStrings.FalseShort)));
            mshObject.Properties.Add(new PSNoteProperty("required", "true"));
            obj.Properties.Add(new PSNoteProperty("parameterValue", mshObject));
        /// Adds the InputTypes properties.
        /// <param name="parameters">Command parameters.</param>
        internal static void AddInputTypesProperties(PSObject obj, Dictionary<string, ParameterMetadata> parameters)
            Collection<string> inputs = new Collection<string>();
                    Collection<ParameterAttribute> attribs = GetParameterAttribute(parameter.Value.Attributes);
                    foreach (ParameterAttribute attrib in attribs)
                        if (attrib.ValueFromPipeline || attrib.ValueFromPipelineByPropertyName)
                            if (!inputs.Contains(parameter.Value.ParameterType.FullName))
                                inputs.Add(parameter.Value.ParameterType.FullName);
            if (inputs.Count == 0)
                inputs.Add(StringUtil.Format(HelpDisplayStrings.None));
            foreach (string input in inputs)
            PSObject inputTypesObj = new PSObject();
            inputTypesObj.TypeNames.Clear();
            inputTypesObj.TypeNames.Add(string.Create(CultureInfo.InvariantCulture, $"{DefaultCommandHelpObjectBuilder.TypeNameForDefaultHelp}#inputTypes"));
            PSObject inputTypeObj = new PSObject();
            inputTypeObj.TypeNames.Clear();
            inputTypeObj.TypeNames.Add(string.Create(CultureInfo.InvariantCulture, $"{DefaultCommandHelpObjectBuilder.TypeNameForDefaultHelp}#inputType"));
            PSObject typeObj = new PSObject();
            typeObj.TypeNames.Clear();
            typeObj.TypeNames.Add(string.Create(CultureInfo.InvariantCulture, $"{DefaultCommandHelpObjectBuilder.TypeNameForDefaultHelp}#type"));
            typeObj.Properties.Add(new PSNoteProperty("name", sb.ToString()));
            inputTypeObj.Properties.Add(new PSNoteProperty("type", typeObj));
            inputTypesObj.Properties.Add(new PSNoteProperty("inputType", inputTypeObj));
            obj.Properties.Add(new PSNoteProperty("inputTypes", inputTypesObj));
        /// Adds the OutputTypes properties.
        /// <param name="outputTypes">Output types.</param>
        private static void AddOutputTypesProperties(PSObject obj, ReadOnlyCollection<PSTypeName> outputTypes)
            PSObject returnValuesObj = new PSObject();
            returnValuesObj.TypeNames.Clear();
            returnValuesObj.TypeNames.Add(string.Create(CultureInfo.InvariantCulture, $"{DefaultCommandHelpObjectBuilder.TypeNameForDefaultHelp}#returnValues"));
            PSObject returnValueObj = new PSObject();
            returnValueObj.TypeNames.Clear();
            returnValueObj.TypeNames.Add(string.Create(CultureInfo.InvariantCulture, $"{DefaultCommandHelpObjectBuilder.TypeNameForDefaultHelp}#returnValue"));
            if (outputTypes.Count == 0)
                typeObj.Properties.Add(new PSNoteProperty("name", "System.Object"));
                foreach (PSTypeName outputType in outputTypes)
                    sb.AppendLine(outputType.Name);
            returnValueObj.Properties.Add(new PSNoteProperty("type", typeObj));
            returnValuesObj.Properties.Add(new PSNoteProperty("returnValue", returnValueObj));
            obj.Properties.Add(new PSNoteProperty("returnValues", returnValuesObj));
        /// Adds the aliases properties.
        private static void AddAliasesProperties(PSObject obj, string name, ExecutionContext context)
                foreach (string alias in context.SessionState.Internal.GetAliasesByCommandName(name))
                    sb.AppendLine(alias);
                sb.AppendLine(StringUtil.Format(HelpDisplayStrings.None));
        /// Adds the remarks properties.
        /// <param name="helpUri"></param>
        private static void AddRemarksProperties(PSObject obj, string cmdletName, string helpUri)
            if (string.IsNullOrEmpty(helpUri))
                obj.Properties.Add(new PSNoteProperty("remarks", StringUtil.Format(HelpDisplayStrings.GetLatestHelpContentWithoutHelpUri, cmdletName)));
                obj.Properties.Add(new PSNoteProperty("remarks", StringUtil.Format(HelpDisplayStrings.GetLatestHelpContent, cmdletName, helpUri)));
        /// Adds the related links properties.
        /// <param name="relatedLink"></param>
        internal static void AddRelatedLinksProperties(PSObject obj, string relatedLink)
            if (!string.IsNullOrEmpty(relatedLink))
                PSObject navigationLinkObj = new PSObject();
                navigationLinkObj.TypeNames.Clear();
                navigationLinkObj.TypeNames.Add(string.Create(CultureInfo.InvariantCulture, $"{DefaultCommandHelpObjectBuilder.TypeNameForDefaultHelp}#navigationLinks"));
                navigationLinkObj.Properties.Add(new PSNoteProperty("uri", relatedLink));
                List<PSObject> navigationLinkValues = new List<PSObject> { navigationLinkObj };
                // check if obj already has relatedLinks property
                PSNoteProperty relatedLinksPO = obj.Properties["relatedLinks"] as PSNoteProperty;
                if ((relatedLinksPO != null) && (relatedLinksPO.Value != null))
                    PSObject relatedLinksValue = PSObject.AsPSObject(relatedLinksPO.Value);
                    PSNoteProperty navigationLinkPO = relatedLinksValue.Properties["navigationLink"] as PSNoteProperty;
                    if ((navigationLinkPO != null) && (navigationLinkPO.Value != null))
                        PSObject navigationLinkValue = navigationLinkPO.Value as PSObject;
                        if (navigationLinkValue != null)
                            navigationLinkValues.Add(navigationLinkValue);
                            PSObject[] navigationLinkValueArray = navigationLinkPO.Value as PSObject[];
                            if (navigationLinkValueArray != null)
                                foreach (var psObject in navigationLinkValueArray)
                                    navigationLinkValues.Add(psObject);
                PSObject relatedLinksObj = new PSObject();
                relatedLinksObj.TypeNames.Clear();
                relatedLinksObj.TypeNames.Add(string.Create(CultureInfo.InvariantCulture, $"{DefaultCommandHelpObjectBuilder.TypeNameForDefaultHelp}#relatedLinks"));
                relatedLinksObj.Properties.Add(new PSNoteProperty("navigationLink", navigationLinkValues.ToArray()));
                obj.Properties.Add(new PSNoteProperty("relatedLinks", relatedLinksObj));
        /// Gets the parameter attribute from parameter metadata.
        /// <returns>Collection of parameter attributes.</returns>
        private static Collection<ParameterAttribute> GetParameterAttribute(Collection<Attribute> attributes)
            Collection<ParameterAttribute> paramAttributes = new Collection<ParameterAttribute>();
                ParameterAttribute paramAttribute = (object)attribute as ParameterAttribute;
                if (paramAttribute != null)
                    paramAttributes.Add(paramAttribute);
            return paramAttributes;
        /// Gets the validate set attribute from parameter metadata.
        private static Collection<ValidateSetAttribute> GetValidateSetAttribute(Collection<Attribute> attributes)
            Collection<ValidateSetAttribute> validateSetAttributes = new Collection<ValidateSetAttribute>();
                ValidateSetAttribute validateSetAttribute = (object)attribute as ValidateSetAttribute;
                    validateSetAttributes.Add(validateSetAttribute);
            return validateSetAttributes;
        /// Gets the pipeline input type.
        /// <param name="paramAttrib">Parameter attribute.</param>
        /// <returns>Pipeline input type.</returns>
        private static string GetPipelineInputString(ParameterAttribute paramAttrib)
            Debug.Assert(paramAttrib != null);
            ArrayList values = new ArrayList();
            if (paramAttrib.ValueFromPipeline)
                values.Add(StringUtil.Format(HelpDisplayStrings.PipelineByValue));
            if (paramAttrib.ValueFromPipelineByPropertyName)
                values.Add(StringUtil.Format(HelpDisplayStrings.PipelineByPropertyName));
            if (paramAttrib.ValueFromRemainingArguments)
                values.Add(StringUtil.Format(HelpDisplayStrings.PipelineFromRemainingArguments));
                return StringUtil.Format(HelpDisplayStrings.FalseShort);
            sb.Append(StringUtil.Format(HelpDisplayStrings.TrueShort));
            sb.Append(" (");
            for (int i = 0; i < values.Count; i++)
                sb.Append((string)values[i]);
                if (i != (values.Count - 1))
        /// Checks if a set of parameters contains any of the common parameters.
        /// <param name="parameters">Parameters to check.</param>
        /// <returns>True if it contains common parameters, false otherwise.</returns>
        internal static bool HasCommonParameters(Dictionary<string, ParameterMetadata> parameters)
            Collection<string> commonParams = new Collection<string>();
                if (Cmdlet.CommonParameters.Contains(parameter.Value.Name))
                    commonParams.Add(parameter.Value.Name);
            return (commonParams.Count == Cmdlet.CommonParameters.Count);
        /// Checks if the module contains HelpInfoUri.
        private static bool HasHelpInfoUri(PSModuleInfo module, string moduleName)
            // The core module is really a SnapIn, so module will be null
            if (!string.IsNullOrEmpty(moduleName) && moduleName.Equals(InitialSessionState.CoreModule, StringComparison.OrdinalIgnoreCase))
            return !string.IsNullOrEmpty(module.HelpInfoUri);

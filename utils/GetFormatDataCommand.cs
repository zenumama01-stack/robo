    /// Gets formatting information from the loading format information database.
    /// <remarks>Currently supports only table controls
    [Cmdlet(VerbsCommon.Get, "FormatData", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096614")]
    [OutputType(typeof(System.Management.Automation.ExtendedTypeDefinition))]
    public class GetFormatDataCommand : PSCmdlet
        private string[] _typename;
        private WildcardPattern[] _filter = new WildcardPattern[1];
        public string[] TypeName
                return _typename;
                _typename = value;
                if (_typename == null)
                    _filter = Array.Empty<WildcardPattern>();
                    _filter = new WildcardPattern[_typename.Length];
                        _filter[i] = WildcardPattern.Get(_typename[i],
        /// When specified, helps control whether or not to send richer formatting data
        /// that was not supported by earlier versions of PowerShell.
        public Version PowerShellVersion { get; set; }
        /// Set the default filter.
            if (_filter[0] == null)
                _filter[0] = WildcardPattern.Get("*", WildcardOptions.None);
        private static Dictionary<string, List<string>> GetTypeGroupMap(IEnumerable<TypeGroupDefinition> groupDefinitions)
            var typeGroupMap = new Dictionary<string, List<string>>();
            foreach (TypeGroupDefinition typeGroup in groupDefinitions)
                // The format system actually allows you to define multiple SelectionSets with the same name, but only the
                // first type group will take effect. So we skip the rest groups that have the same name.
                if (!typeGroupMap.ContainsKey(typeGroup.name))
                    var typesInGroup = typeGroup.typeReferenceList.ConvertAll(static typeReference => typeReference.name);
                    typeGroupMap.Add(typeGroup.name, typesInGroup);
            return typeGroupMap;
        /// Takes out the content from the database and writes them out.
            // Remoting detection:
            //   * Automatic variable $PSSenderInfo is defined in true remoting contexts as well as in background jobs.
            //   * $PSSenderInfo.ApplicationArguments.PSVersionTable.PSVersion contains the client version, as a [version] instance.
            //      Note: Even though $PSVersionTable.PSVersion is of type [semver] in PowerShell 6+, it is of type [version] here,
            //            presumably because only the latter type deserializes type-faithfully.
            var clientVersion = PowerShellVersion;
            PSSenderInfo remotingClientInfo = GetVariableValue("PSSenderInfo") as PSSenderInfo;
            if (clientVersion == null && remotingClientInfo != null)
                clientVersion = PSObject.Base((PSObject.Base(remotingClientInfo.ApplicationArguments["PSVersionTable"]) as PSPrimitiveDictionary)?["PSVersion"]) as Version;
            // During remoting, remain compatible with v5.0- clients by default.
            // Passing a -PowerShellVersion argument allows overriding the client version.
            bool writeOldWay =
                (remotingClientInfo != null && clientVersion == null)  // To be safe: Remoting client version could unexpectedly not be determined.
                (clientVersion != null
                    &&
                    (clientVersion.Major < 5
                    (clientVersion.Major == 5 && clientVersion.Minor < 1)));
            TypeInfoDataBase db = this.Context.FormatDBManager.Database;
            List<ViewDefinition> viewdefinitions = db.viewDefinitionsSection.viewDefinitionList;
            Dictionary<string, List<string>> typeGroupMap = GetTypeGroupMap(db.typeGroupSection.typeGroupDefinitionList);
            var typedefs = new Dictionary<ConsolidatedString, List<FormatViewDefinition>>(ConsolidatedString.EqualityComparer);
            foreach (ViewDefinition definition in viewdefinitions)
                this.WriteVerbose(string.Format(CultureInfo.CurrentCulture, GetFormatDataStrings.ProcessViewDefinition, definition.name));
                if (definition.isHelpFormatter)
                var consolidatedTypeName = CreateConsolidatedTypeName(definition, typeGroupMap);
                if (!ShouldGenerateView(consolidatedTypeName))
                PSControl control;
                if (definition.mainControl is TableControlBody tableControlBody)
                    control = new TableControl(tableControlBody, definition);
                    if (definition.mainControl is ListControlBody listControlBody)
                        control = new ListControl(listControlBody, definition);
                        if (definition.mainControl is WideControlBody wideControlBody)
                            control = new WideControl(wideControlBody, definition);
                            if (writeOldWay)
                                // Alignment was added to WideControl in V2, but wasn't
                                // used.  It was removed in V5, but old PowerShell clients
                                // expect this property or fail to rehydrate the remote object.
                                var psobj = new PSObject(control);
                                psobj.Properties.Add(new PSNoteProperty("Alignment", 0));
                            var complexControlBody = (ComplexControlBody)definition.mainControl;
                            control = new CustomControl(complexControlBody, definition);
                // Older version of PowerShell do not know about something in the control, so
                // don't return it.
                if (writeOldWay && !control.CompatibleWithOldPowerShell())
                var formatdef = new FormatViewDefinition(definition.name, control, definition.InstanceId);
                List<FormatViewDefinition> viewList;
                if (!typedefs.TryGetValue(consolidatedTypeName, out viewList))
                    viewList = new List<FormatViewDefinition>();
                    typedefs.Add(consolidatedTypeName, viewList);
                viewList.Add(formatdef);
            // write out all the available type definitions
            foreach (var pair in typedefs)
                var typeNames = pair.Key;
                    foreach (var typeName in typeNames)
                        var etd = new ExtendedTypeDefinition(typeName, pair.Value);
                        WriteObject(etd);
                    var etd = new ExtendedTypeDefinition(typeNames[0], pair.Value);
                    for (int i = 1; i < typeNames.Count; i++)
                        etd.TypeNames.Add(typeNames[i]);
        private static ConsolidatedString CreateConsolidatedTypeName(ViewDefinition definition, Dictionary<string, List<string>> typeGroupMap)
            // Create our "consolidated string" typename which is used as a dictionary key
            var reflist = definition.appliesTo.referenceList;
            var consolidatedTypeName = new ConsolidatedString(ConsolidatedString.Empty);
            foreach (TypeOrGroupReference item in reflist)
                // If it's a TypeGroup, we need to look that up and add it's members
                if (item is TypeGroupReference)
                    List<string> typesInGroup;
                    if (typeGroupMap.TryGetValue(item.name, out typesInGroup))
                        foreach (string typeName in typesInGroup)
                            consolidatedTypeName.Add(typeName);
                    consolidatedTypeName.Add(item.name);
            return consolidatedTypeName;
        private bool ShouldGenerateView(ConsolidatedString consolidatedTypeName)
                foreach (var typeName in consolidatedTypeName)
                    if (pattern.IsMatch(typeName))

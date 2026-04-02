    /// Class MamlCommandHelpInfo keeps track of help information to be returned by
    internal class MamlCommandHelpInfo : BaseCommandHelpInfo
        /// Constructor for custom HelpInfo object construction
        /// This is used by the CommandHelpProvider class to generate the
        /// default help UX when no help content is present.
        internal MamlCommandHelpInfo(PSObject helpObject, HelpCategory helpCategory)
            : base(helpCategory)
            this.ForwardHelpCategory = HelpCategory.Provider;
            this.AddCommonHelpProperties();
            // set user defined data
            if (helpObject.Properties["Component"] != null)
                _component = helpObject.Properties["Component"].Value as string;
            if (helpObject.Properties["Role"] != null)
                _role = helpObject.Properties["Role"].Value as string;
            if (helpObject.Properties["Functionality"] != null)
                _functionality = helpObject.Properties["Functionality"].Value as string;
        /// Constructor for MamlCommandHelpInfo. This constructor will call the corresponding
        /// constructor in CommandHelpInfo so that xmlNode will be converted a mamlNode.
        /// This constructor is intentionally made private so that the only way to create
        /// MamlCommandHelpInfo is through static function
        ///     Load(XmlNode node)
        /// where some sanity check is done.
        private MamlCommandHelpInfo(XmlNode xmlNode, HelpCategory helpCategory) : base(helpCategory)
            // The type name hierarchy for mshObject doesn't necessary
            // reflect the hierarchy in source code. From display's point of
            // view MamlCommandHelpInfo is derived from HelpInfo.
                _fullHelpObject.TypeNames.Add("DscResourceHelpInfo");
                _fullHelpObject.TypeNames.Add("MamlCommandHelpInfo");
        /// Override the FullHelp PSObject of this provider-specific HelpInfo with generic help.
        internal void OverrideProviderSpecificHelpWithGenericHelp(HelpInfo genericHelpInfo)
            PSObject genericHelpMaml = genericHelpInfo.FullHelp;
            MamlUtil.OverrideName(_fullHelpObject, genericHelpMaml);
            MamlUtil.OverridePSTypeNames(_fullHelpObject, genericHelpMaml);
            MamlUtil.PrependSyntax(_fullHelpObject, genericHelpMaml);
            MamlUtil.PrependDetailedDescription(_fullHelpObject, genericHelpMaml);
            MamlUtil.OverrideParameters(_fullHelpObject, genericHelpMaml);
            MamlUtil.PrependNotes(_fullHelpObject, genericHelpMaml);
            MamlUtil.AddCommonProperties(_fullHelpObject, genericHelpMaml);
        /// <value>Full help object for this help item.</value>
        /// Examples string of this cmdlet help info.
        private string Examples
                return ExtractTextForHelpProperty(this.FullHelp, "Examples");
        /// Parameters string of this cmdlet help info.
        private string Parameters
                return ExtractTextForHelpProperty(this.FullHelp, "Parameters");
        /// Notes string of this cmdlet help info.
        private string Notes
                return ExtractTextForHelpProperty(this.FullHelp, "alertset");
        #region Component, Role, Features
        // Component, Role, Functionality are required by exchange for filtering
        // help contents to be returned from help system.
        // Following is how this is going to work,
        //    1. Each command will optionally include component, role and functionality
        //       information. This information is discovered from help content
        //       from xml tags <component>, <role>, <functionality> respectively
        //       as part of command metadata.
        //    2. From command line, end user can request help for commands for
        //       particular component, role and functionality using parameters like
        //       -component, -role, -functionality.
        //    3. At runtime, help engine will match against component/role/functionality
        //       criteria before returning help results.
        private string _component = null;
        /// Component for this command.
        internal override string Component
                return _component;
        private string _role = null;
        /// Role for this command.
        internal override string Role
                return _role;
        private string _functionality = null;
        /// Functionality for this command.
        internal override string Functionality
                return _functionality;
        internal void SetAdditionalDataFromHelpComment(string component, string functionality, string role)
            _component = component;
            _functionality = functionality;
            _role = role;
            // component,role,functionality is part of common help..
            // Update these properties as we have new data now..
            this.UpdateUserDefinedDataProperties();
        /// Add user-defined command help data to command help.
        /// <param name="userDefinedData">User defined data object.</param>
        internal void AddUserDefinedData(UserDefinedHelpData userDefinedData)
            if (userDefinedData == null)
            if (userDefinedData.Properties.TryGetValue("component", out propertyValue))
                _component = propertyValue;
            if (userDefinedData.Properties.TryGetValue("role", out propertyValue))
                _role = propertyValue;
            if (userDefinedData.Properties.TryGetValue("functionality", out propertyValue))
                _functionality = propertyValue;
        /// Create a MamlCommandHelpInfo object from an XmlNode.
        internal static MamlCommandHelpInfo Load(XmlNode xmlNode, HelpCategory helpCategory)
            MamlCommandHelpInfo mamlCommandHelpInfo = new MamlCommandHelpInfo(xmlNode, helpCategory);
            if (string.IsNullOrEmpty(mamlCommandHelpInfo.Name))
            mamlCommandHelpInfo.AddCommonHelpProperties();
            return mamlCommandHelpInfo;
        #region Provider specific help
        /// Merge the provider specific help with current command help.
        /// The cmdletHelp and dynamicParameterHelp is normally retrieved from ProviderHelpProvider.
        /// A new MamlCommandHelpInfo is created to avoid polluting the provider help cache.
        /// <param name="cmdletHelp">Provider-specific cmdletHelp to merge into current MamlCommandHelpInfo object.</param>
        /// <param name="dynamicParameterHelp">Provider-specific dynamic parameter help to merge into current MamlCommandHelpInfo object.</param>
        /// <returns>Merged command help info object.</returns>
        internal MamlCommandHelpInfo MergeProviderSpecificHelp(PSObject cmdletHelp, PSObject[] dynamicParameterHelp)
            if (this._fullHelpObject == null)
            MamlCommandHelpInfo result = (MamlCommandHelpInfo)this.MemberwiseClone();
            // We will need to use a deep clone of _fullHelpObject
            // to avoid _fullHelpObject being get terminated.
            result._fullHelpObject = this._fullHelpObject.Copy();
            if (cmdletHelp != null)
                result._fullHelpObject.Properties.Add(new PSNoteProperty("PS_Cmdlet", cmdletHelp));
            if (dynamicParameterHelp != null)
                result._fullHelpObject.Properties.Add(new PSNoteProperty("PS_DynamicParameters", dynamicParameterHelp));
        /// Extracts text for a given property from the full help object.
        /// <param name="psObject">FullHelp object.</param>
        /// Name of the property for which text needs to be extracted.
        private static string ExtractTextForHelpProperty(PSObject psObject, string propertyName)
            if (psObject.Properties[propertyName] == null ||
                psObject.Properties[propertyName].Value == null)
            return ExtractText(PSObject.AsPSObject(psObject.Properties[propertyName].Value));
        /// Given a PSObject, this method will traverse through the objects properties,
        /// extracts content from properties that are of type System.String, appends them
        /// together and returns.
        private static string ExtractText(PSObject psObject)
                string typeNameOfValue = propertyInfo.TypeNameOfValue;
                switch (typeNameOfValue.ToLowerInvariant())
                    case "system.boolean":
                    case "system.int32":
                    case "system.object":
                    case "system.object[]":
                    case "system.string":
                        result.Append((string)LanguagePrimitives.ConvertTo(propertyInfo.Value,
                            typeof(string), CultureInfo.InvariantCulture));
                    case "system.management.automation.psobject[]":
                        PSObject[] items = (PSObject[])LanguagePrimitives.ConvertTo(
                                propertyInfo.Value,
                        foreach (PSObject item in items)
                            result.Append(ExtractText(item));
                    case "system.management.automation.psobject":
                        result.Append(ExtractText(PSObject.AsPSObject(propertyInfo.Value)));
            System.Management.Automation.Diagnostics.Assert(pattern != null, "pattern cannot be null");
            if ((!string.IsNullOrEmpty(synopsis)) && (pattern.IsMatch(synopsis)))
            if ((!string.IsNullOrEmpty(detailedDescription)) && (pattern.IsMatch(detailedDescription)))
            string examples = Examples;
            if ((!string.IsNullOrEmpty(examples)) && (pattern.IsMatch(examples)))
            string notes = Notes;
            if ((!string.IsNullOrEmpty(notes)) && (pattern.IsMatch(notes)))
            string parameters = Parameters;
            if ((!string.IsNullOrEmpty(parameters)) && (pattern.IsMatch(parameters)))
        internal MamlCommandHelpInfo Copy()
            MamlCommandHelpInfo result = new MamlCommandHelpInfo(_fullHelpObject.Copy(), this.HelpCategory);
        internal MamlCommandHelpInfo Copy(HelpCategory newCategoryToUse)
            MamlCommandHelpInfo result = new MamlCommandHelpInfo(_fullHelpObject.Copy(), newCategoryToUse);

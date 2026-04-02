    /// Class ProviderHelpInfo keeps track of help information to be returned by
    internal sealed class ProviderHelpInfo : HelpInfo
        private ProviderHelpInfo(XmlNode xmlNode)
            _fullHelpObject.TypeNames.Add("ProviderHelpInfo");
        #region Basic Help Properties / Methods
        /// Name of the provider for which this provider help info is for.
                if (_fullHelpObject == null)
                if (_fullHelpObject.Properties["Name"] == null)
                if (_fullHelpObject.Properties["Name"].Value == null)
                string name = _fullHelpObject.Properties["Name"].Value.ToString();
        /// Synopsis in the provider help info.
        /// <value>Synopsis in the provider help info</value>
                if (_fullHelpObject.Properties["Synopsis"] == null)
                if (_fullHelpObject.Properties["Synopsis"].Value == null)
                string synopsis = _fullHelpObject.Properties["Synopsis"].Value.ToString();
        /// Detailed description in the provider help info.
        /// <value>Detailed description in the provider help info</value>
                if (this.FullHelp.Properties["DetailedDescription"] == null ||
                    this.FullHelp.Properties["DetailedDescription"].Value == null)
                IList descriptionItems = FullHelp.Properties["DetailedDescription"].Value as IList;
                if (descriptionItems == null || descriptionItems.Count == 0)
                // I think every provider description should at least have 400 characters...
                // so starting with this assumption..I did an average of all the help content
                // available at the time of writing this code and came up with this number.
                Text.StringBuilder result = new Text.StringBuilder(400);
        /// Help category for this provider help info, which is constantly HelpCategory.Provider.
        /// <value>Help category for this provider help info</value>
                return HelpCategory.Provider;
        /// Full help object for this provider help info.
        /// <value>Full help object for this provider help info</value>
        /// Provider help info looks for pattern in Synopsis and
            Diagnostics.Assert(pattern != null, "pattern cannot be null");
        #region Cmdlet Help and Dynamic Parameter Help
        private Hashtable _cmdletHelps;
        /// Return the provider-specific cmdlet help based on input cmdletName.
        /// <param name="cmdletName">CmdletName on which to get provider-specific help.</param>
        /// <returns>An mshObject that contains provider-specific commandlet help.</returns>
        internal PSObject GetCmdletHelp(string cmdletName)
            LoadCmdletHelps();
            if (_cmdletHelps == null)
            return (PSObject)_cmdletHelps[cmdletName];
        /// Load provider-specific commandlet helps from xmlNode stored in _fullHelpObject.
        /// Result will be stored in a hashtable.
        private void LoadCmdletHelps()
            if (_cmdletHelps != null)
            _cmdletHelps = new Hashtable();
            if (_fullHelpObject.Properties["Cmdlets"] == null)
            PSObject cmdlets = (PSObject)_fullHelpObject.Properties["Cmdlets"].Value;
            if (cmdlets.Properties["Cmdlet"] == null ||
                cmdlets.Properties["Cmdlet"].Value == null)
            if (cmdlets.Properties["Cmdlet"].Value.GetType().Equals(typeof(PSObject[])))
                PSObject[] cmdletHelpItems = (PSObject[])cmdlets.Properties["Cmdlet"].Value;
                for (int i = 0; i < cmdletHelpItems.Length; i++)
                    if (cmdletHelpItems[i].Properties["Name"] == null
                        || cmdletHelpItems[i].Properties["Name"].Value == null)
                    string name = ((PSObject)cmdletHelpItems[i].Properties["Name"].Value).ToString();
                    _cmdletHelps[name] = cmdletHelpItems[i];
            else if (cmdlets.Properties["Cmdlet"].Value.GetType().Equals(typeof(PSObject[])))
                PSObject cmdletHelpItem = (PSObject)cmdlets.Properties["Cmdlet"].Value;
                string name = ((PSObject)cmdletHelpItem.Properties["Name"].Value).ToString();
                _cmdletHelps[name] = cmdletHelpItem;
        private Hashtable _dynamicParameterHelps;
        /// Return the provider-specific dynamic parameter help based on input parameter name.
        /// <param name="parameters">An array of parameters to retrieve help.</param>
        /// <returns>An array of mshObject that contains the parameter help.</returns>
        internal PSObject[] GetDynamicParameterHelp(string[] parameters)
            if (parameters == null || parameters.Length == 0)
            LoadDynamicParameterHelps();
            if (_dynamicParameterHelps == null)
                PSObject entry = (PSObject)_dynamicParameterHelps[parameters[i].ToLower()];
            return (PSObject[])result.ToArray(typeof(PSObject));
        /// Load provider-specific dynamic parameter helps from xmlNode stored in _fullHelpObject.
        private void LoadDynamicParameterHelps()
            if (_dynamicParameterHelps != null)
            _dynamicParameterHelps = new Hashtable();
            if (_fullHelpObject.Properties["DynamicParameters"] == null)
            PSObject dynamicParameters = (PSObject)_fullHelpObject.Properties["DynamicParameters"].Value;
            if (dynamicParameters == null)
            if (dynamicParameters.Properties["DynamicParameter"] == null
                || dynamicParameters.Properties["DynamicParameter"].Value == null)
            if (dynamicParameters.Properties["DynamicParameter"].Value.GetType().Equals(typeof(PSObject[])))
                PSObject[] dynamicParameterHelpItems = (PSObject[])dynamicParameters.Properties["DynamicParameter"].Value;
                for (int i = 0; i < dynamicParameterHelpItems.Length; i++)
                    if (dynamicParameterHelpItems[i].Properties["Name"] == null
                        || dynamicParameterHelpItems[i].Properties["Name"].Value == null)
                    string name = ((PSObject)dynamicParameterHelpItems[i].Properties["Name"].Value).ToString();
                    _dynamicParameterHelps[name] = dynamicParameterHelpItems[i];
            else if (dynamicParameters.Properties["DynamicParameter"].Value.GetType().Equals(typeof(PSObject[])))
                PSObject dynamicParameterHelpItem = (PSObject)dynamicParameters.Properties["DynamicParameter"].Value;
                string name = ((PSObject)dynamicParameterHelpItem.Properties["Name"].Value).ToString();
                _dynamicParameterHelps[name] = dynamicParameterHelpItem;
        #region Load Help
        /// Create providerHelpInfo from an xmlNode.
        /// <param name="xmlNode">Xml node that contains the provider help info.</param>
        /// <returns>The providerHelpInfo object created.</returns>
        internal static ProviderHelpInfo Load(XmlNode xmlNode)
            ProviderHelpInfo providerHelpInfo = new ProviderHelpInfo(xmlNode);
            if (string.IsNullOrEmpty(providerHelpInfo.Name))
            providerHelpInfo.AddCommonHelpProperties();
            return providerHelpInfo;

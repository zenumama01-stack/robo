    /// Updatable help system internal representation of the PSModuleInfo class.
    internal class UpdatableHelpModuleInfo
        internal static readonly string HelpContentZipName = "HelpContent.zip";
        internal static readonly string HelpContentZipName = "HelpContent.cab";
        internal static readonly string HelpIntoXmlName = "HelpInfo.xml";
        /// <param name="guid">Module GUID.</param>
        /// <param name="path">Module path.</param>
        /// <param name="uri">HelpInfo URI.</param>
        internal UpdatableHelpModuleInfo(string name, Guid guid, string path, string uri)
            Debug.Assert(!string.IsNullOrEmpty(name));
            Debug.Assert(!string.IsNullOrEmpty(path));
            Debug.Assert(!string.IsNullOrEmpty(uri));
            ModuleName = name;
            _moduleGuid = guid;
            ModuleBase = path;
        /// Module name.
        internal string ModuleName { get; }
        /// Module GUID.
        internal Guid ModuleGuid
                return _moduleGuid;
        /// Module path.
        internal string ModuleBase { get; }
        /// HelpInfo URI.
        internal string HelpInfoUri { get; }
        /// Gets the combined HelpContent.zip name.
        /// <returns>HelpContent name.</returns>
        internal string GetHelpContentName(CultureInfo culture)
            return ModuleName + "_" + _moduleGuid.ToString() + "_" + culture.Name + "_" + HelpContentZipName;
        /// Gets the combined HelpInfo.xml name.
        /// <returns>HelpInfo name.</returns>
        internal string GetHelpInfoName()
            return ModuleName + "_" + _moduleGuid.ToString() + "_" + HelpIntoXmlName;

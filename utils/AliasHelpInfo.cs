    /// Stores help information related to Alias Commands.
    internal sealed class AliasHelpInfo : HelpInfo
        /// Initializes a new instance of the AliasHelpInfo class.
        /// The constructor is private. The only way to create an
        /// AliasHelpInfo object is through static method <see cref="GetHelpInfo"/>
        private AliasHelpInfo(AliasInfo aliasInfo)
            _fullHelpObject = new PSObject();
            string name = (aliasInfo.ResolvedCommand == null) ? aliasInfo.UnresolvedCommandName : aliasInfo.ResolvedCommand.Name;
            this.ForwardTarget = name;
            // A Cmdlet/Function/Script etc can have alias.
            this.ForwardHelpCategory = HelpCategory.Cmdlet |
                HelpCategory.Function | HelpCategory.ExternalScript | HelpCategory.ScriptCommand | HelpCategory.Filter;
            if (!string.IsNullOrEmpty(aliasInfo.Name))
                Name = aliasInfo.Name.Trim();
                Synopsis = name.Trim();
            _fullHelpObject.TypeNames.Clear();
            _fullHelpObject.TypeNames.Add(string.Create(Globalization.CultureInfo.InvariantCulture, $"AliasHelpInfo#{Name}"));
            _fullHelpObject.TypeNames.Add("AliasHelpInfo");
            _fullHelpObject.TypeNames.Add("HelpInfo");
        /// Returns the name of alias help.
        /// <value>Name of alias help.</value>
        internal override string Name { get; } = string.Empty;
        /// Returns synopsis of alias help.
        /// <value>Synopsis of alias help.</value>
        internal override string Synopsis { get; } = string.Empty;
        /// Help category for alias help. This is always HelpCategory.Alias.
        /// <value>Help category for alias help</value>
                return HelpCategory.Alias;
        private readonly PSObject _fullHelpObject;
        /// Returns full help object for alias help.
        /// <value>Full help object of alias help.</value>
        internal override PSObject FullHelp
                return _fullHelpObject;
        /// Creates an AliasHelpInfo instance based on an AliasInfo object.
        /// This is the only way to create AliasHelpInfo object from outside this class.
        /// <param name="aliasInfo">AliasInfo object for which to create AliasHelpInfo object.</param>
        /// <returns>AliasHelpInfo object.</returns>
        internal static AliasHelpInfo GetHelpInfo(AliasInfo aliasInfo)
            if (aliasInfo == null)
            if (aliasInfo.ResolvedCommand == null && aliasInfo.UnresolvedCommandName == null)
            AliasHelpInfo aliasHelpInfo = new AliasHelpInfo(aliasInfo);
            if (string.IsNullOrEmpty(aliasHelpInfo.Name))
            aliasHelpInfo.AddCommonHelpProperties();
            return aliasHelpInfo;

    /// Class HelpInfo keeps track of help information to be returned by help system.
    /// HelpInfo includes information in following aspect,
    ///     a. Name: the target name for help
    ///     b. Category: what category the help belongs to
    /// This class will be derived to track help info for different help categories like,
    ///     AliasHelpInfo
    ///     CommandHelpInfo
    ///     ProviderHelpInfo
    /// In general, there will be a specific helpInfo child class for each kind of help provider.
    internal abstract class HelpInfo
        /// Constructor for HelpInfo.
        internal HelpInfo()
        /// Name for help info.
        /// <value>Name for help info</value>
        internal abstract string Name
        /// Synopsis for help info.
        /// <value>Synopsis for help info</value>
        internal abstract string Synopsis
        /// Component for help info.
        /// <value>Component for help info</value>
        internal virtual string Component
        /// Role for help info.
        /// <value>Role for help ino</value>
        internal virtual string Role
        /// Functionality for help info.
        /// <value>Functionality for help info</value>
        internal virtual string Functionality
        /// Help category for help info.
        /// <value>Help category for help info</value>
        internal abstract HelpCategory HelpCategory
        /// Forward help category for this help info.
        /// If this is not HelpCategory.None, then some other help provider
        /// (as specified in the HelpCategory bit pattern) need
        /// to process this helpInfo before it can be returned to end user.
        /// <value>Help category to forward this helpInfo to</value>
        internal HelpCategory ForwardHelpCategory { get; set; } = HelpCategory.None;
        /// Target object in forward-help-provider that should process this HelpInfo.
        /// This will serve as auxiliary information to be passed to forward help provider.
        /// In the case of AliasHelpInfo, for example, it needs to be forwarded to
        /// CommandHelpProvider to fill in detailed helpInfo. In that case, ForwardHelpCategory
        /// will be HelpCategory.Command and the help target is the cmdlet name that matches this
        /// <value>forward target object name</value>
        internal string ForwardTarget { get; set; } = string.Empty;
        /// Full help object for this help item.
        /// <value>Full help object for this help item</value>
        internal abstract PSObject FullHelp
        /// Short help object for this help item.
        /// <value>Short help object for this help item</value>
        internal PSObject ShortHelp
                PSObject shortHelpObject = new PSObject(this.FullHelp);
                shortHelpObject.TypeNames.Clear();
                shortHelpObject.TypeNames.Add("HelpInfoShort");
                return shortHelpObject;
        /// The base method returns an empty list.
        internal virtual PSObject[] GetParameter(string pattern)
        /// online.
        internal virtual Uri GetUriForOnlineHelp()
        internal virtual bool MatchPatternInContent(WildcardPattern pattern)
            // this is base class implementation..derived classes can choose
            // what is best to them.
        /// Add common help properties to the helpObject which is in PSObject format.
        /// Intrinsic help properties include properties like,
        ///     Name,
        ///     Synopsis
        ///     HelpCategory
        /// Since help object from different help category has different format, it is
        /// needed that we generate these basic information uniformly in the help object
        /// This function is normally called at the end of each child class constructor.
        protected void AddCommonHelpProperties()
            if (this.FullHelp.Properties["Name"] == null)
                this.FullHelp.Properties.Add(new PSNoteProperty("Name", this.Name));
            if (this.FullHelp.Properties["Category"] == null)
                this.FullHelp.Properties.Add(new PSNoteProperty("Category", this.HelpCategory.ToString()));
            if (this.FullHelp.Properties["Synopsis"] == null)
                this.FullHelp.Properties.Add(new PSNoteProperty("Synopsis", this.Synopsis));
            if (this.FullHelp.Properties["Component"] == null)
                this.FullHelp.Properties.Add(new PSNoteProperty("Component", this.Component));
            if (this.FullHelp.Properties["Role"] == null)
                this.FullHelp.Properties.Add(new PSNoteProperty("Role", this.Role));
            if (this.FullHelp.Properties["Functionality"] == null)
                this.FullHelp.Properties.Add(new PSNoteProperty("Functionality", this.Functionality));
        /// Update common help user-defined properties of the help object which is in PSObject format.
        /// Call this function to update Mshobject after it is created.
        /// This function wont create new properties.This will update only user-defined properties created in
        /// <paramref name="AddCommonHelpProperties"/>
        protected void UpdateUserDefinedDataProperties()
            this.FullHelp.Properties.Remove("Component");
            this.FullHelp.Properties.Remove("Role");
            this.FullHelp.Properties.Remove("Functionality");
        /// This is for tracking the set of errors happened during the parsing of
        /// of this helpinfo.
        internal Collection<ErrorRecord> Errors { get; set; }

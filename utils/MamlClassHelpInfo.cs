    /// Class MamlClassHelpInfo keeps track of help information to be returned by
    /// class help provider.
    internal class MamlClassHelpInfo : HelpInfo
        /// Constructor for custom HelpInfo object creation.
        /// <param name="helpObject"></param>
        internal MamlClassHelpInfo(PSObject helpObject, HelpCategory helpCategory)
            _fullHelpObject = helpObject;
        /// Convert a XMLNode to HelpInfo object.
        /// <param name="xmlNode"></param>
        private MamlClassHelpInfo(XmlNode xmlNode, HelpCategory helpCategory)
            MamlNode mamlNode = new MamlNode(xmlNode);
            _fullHelpObject = mamlNode.PSObject;
            this.Errors = mamlNode.Errors;
            _fullHelpObject.TypeNames.Add("PSClassHelpInfo");
        /// PSObject representation on help.
        /// Create a MamlClassHelpInfo object from an XmlNode.
        /// <param name="xmlNode">XmlNode that contains help info.</param>
        /// <param name="helpCategory">Help category this maml object fits into.</param>
        /// <returns>MamlCommandHelpInfo object created.</returns>
        internal static MamlClassHelpInfo Load(XmlNode xmlNode, HelpCategory helpCategory)
            MamlClassHelpInfo mamlClassHelpInfo = new MamlClassHelpInfo(xmlNode, helpCategory);
            if (string.IsNullOrEmpty(mamlClassHelpInfo.Name))
            mamlClassHelpInfo.AddCommonHelpProperties();
            return mamlClassHelpInfo;
        #region Helper Methods and Overloads
        /// Clone the help info object.
        /// <returns>MamlClassHelpInfo object.</returns>
        internal MamlClassHelpInfo Copy()
            MamlClassHelpInfo result = new MamlClassHelpInfo(_fullHelpObject.Copy(), this.HelpCategory);
        /// Clone the help object with a new category.
        /// <param name="newCategoryToUse"></param>
        /// <returns>MamlClassHelpInfo.</returns>
        internal MamlClassHelpInfo Copy(HelpCategory newCategoryToUse)
            MamlClassHelpInfo result = new MamlClassHelpInfo(_fullHelpObject.Copy(), newCategoryToUse);
            result.FullHelp.Properties["Category"].Value = newCategoryToUse.ToString();
                string tempName = string.Empty;
                var title = _fullHelpObject.Properties["title"];
                if (title != null && title.Value != null)
                    tempName = title.Value.ToString();
                return tempName;
                string tempSynopsis = string.Empty;
                var intro = _fullHelpObject.Properties["introduction"];
                if (intro != null && intro.Value != null)
                    tempSynopsis = intro.Value.ToString();
                return tempSynopsis;
            get { return _fullHelpObject; }

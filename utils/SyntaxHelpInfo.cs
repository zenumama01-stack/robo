    internal sealed class SyntaxHelpInfo : BaseCommandHelpInfo
        /// Constructor for SyntaxHelpInfo.
        private SyntaxHelpInfo(string name, string text, HelpCategory category)
            : base(category)
            Synopsis = text;
        /// <param name="category">Help category.</param>
        /// <returns>SyntaxHelpInfo object created based on information provided.</returns>
        internal static SyntaxHelpInfo GetHelpInfo(string name, string text, HelpCategory category)
            SyntaxHelpInfo syntaxHelpInfo = new SyntaxHelpInfo(name, text, category);
            if (string.IsNullOrEmpty(syntaxHelpInfo.Name))
            syntaxHelpInfo.AddCommonHelpProperties();
            return syntaxHelpInfo;

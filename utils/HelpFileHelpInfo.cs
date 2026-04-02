    /// Class HelpFileHelpInfo keeps track of help information to be returned by
    /// command help provider.
    internal sealed class HelpFileHelpInfo : HelpInfo
        /// Constructor for HelpFileHelpInfo.
        /// This is made private intentionally so that the only way to create object of this type
        /// is through
        ///     GetHelpInfo(string name, string text, string filename)
        /// <param name="name">Help topic name.</param>
        /// <param name="text">Help text.</param>
        /// <param name="filename">File name that contains the help text.</param>
        private HelpFileHelpInfo(string name, string text, string filename)
            FullHelp = PSObject.AsPSObject(text);
            // Take the 5th line as synopsis. This may not be true if
            // format of help file is changed later on.
            _synopsis = GetLine(text, 5);
            if (_synopsis != null)
                _synopsis = _synopsis.Trim();
                // make sure _synopsis is never null
                _synopsis = string.Empty;
            _filename = filename;
        /// Name for the help info.
        /// <value>Name for the help info</value>
        private readonly string _filename = string.Empty;
        private readonly string _synopsis = string.Empty;
        /// Synopsis for the help info.
        /// <value>Synopsis for the help info</value>
                return _synopsis;
        /// Help category for the help info.
        /// <value>Help category for the help info</value>
                return HelpCategory.HelpFile;
        /// Full help object for this help info.
        /// <value>Full help object for this help info</value>
        internal override PSObject FullHelp { get; }
        /// Get help info based on name, text and filename.
        /// <returns>HelpFileHelpInfo object created based on information provided.</returns>
        internal static HelpFileHelpInfo GetHelpInfo(string name, string text, string filename)
            HelpFileHelpInfo helpfileHelpInfo = new HelpFileHelpInfo(name, text, filename);
            if (string.IsNullOrEmpty(helpfileHelpInfo.Name))
            helpfileHelpInfo.AddCommonHelpProperties();
            return helpfileHelpInfo;
        /// Get the text corresponding to a line in input text.
        /// <param name="text">Text to get the line for.</param>
        /// <param name="line">Line number.</param>
        /// <returns>The part of string in text that is in specified line.</returns>
        private static string GetLine(string text, int line)
            StringReader reader = new StringReader(text);
            for (int i = 0; i < line; i++)
                result = reader.ReadLine();
            Diagnostics.Assert(pattern != null, "pattern cannot be null.");
            string helpContent = string.Empty;
            LanguagePrimitives.TryConvertTo<string>(FullHelp, out helpContent);
            return pattern.IsMatch(helpContent);

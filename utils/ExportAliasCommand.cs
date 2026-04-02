    /// The formats that export-alias supports.
    public enum ExportAliasFormat
        /// Aliases will be exported to a CSV file.
        Csv,
        /// Aliases will be exported as a script.
        Script
    /// The implementation of the "export-alias" cmdlet.
    [Cmdlet(VerbsData.Export, "Alias", SupportsShouldProcess = true, DefaultParameterSetName = "ByPath", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096597")]
    [OutputType(typeof(AliasInfo))]
    public class ExportAliasCommand : PSCmdlet
        /// The Path of the file to export the aliases to.
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ByPath")]
            set { _path = value ?? "."; }
        private string _path = ".";
        /// The literal path of the file to export the aliases to.
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByLiteralPath")]
                    _path = ".";
        /// The Name parameter for the command.
            get { return _names; }
            set { _names = value ?? new string[] { "*" }; }
        private string[] _names = new string[] { "*" };
        /// If set to true, the alias that is set is passed to the pipeline.
                return _passThru;
                _passThru = value;
        /// Parameter that determines the format of the file created.
        public ExportAliasFormat As { get; set; } = ExportAliasFormat.Csv;
        /// Property that sets append parameter.
        public SwitchParameter Append
                return _append;
                _append = value;
        private bool _append;
        /// Property that sets force parameter.
        /// Property that prevents file overwrite.
        public SwitchParameter NoClobber
                return _noclobber;
                _noclobber = value;
        private bool _noclobber;
        /// The description that gets added to the file as a comment.
        public string Description { get; set; }
        /// The scope parameter for the command determines
        /// which scope the aliases are retrieved from.
        /// The main processing loop of the command.
            // First get the alias table (from the proper scope if necessary)
            IDictionary<string, AliasInfo> aliasTable = null;
            if (!string.IsNullOrEmpty(Scope))
                // This can throw PSArgumentException and PSArgumentOutOfRangeException
                // but just let them go as this is terminal for the pipeline and the
                // exceptions are already properly adorned with an ErrorRecord.
                aliasTable = SessionState.Internal.GetAliasTableAtScope(Scope);
                aliasTable = SessionState.Internal.GetAliasTable();
            foreach (string aliasName in _names)
                bool resultFound = false;
                // Create the name pattern
                WildcardPattern namePattern =
                        aliasName,
                // Now loop through the table and write out any aliases that
                // match the name and don't match the exclude filters and are
                // visible to the caller...
                CommandOrigin origin = MyInvocation.CommandOrigin;
                foreach (KeyValuePair<string, AliasInfo> tableEntry in aliasTable)
                    if (!namePattern.IsMatch(tableEntry.Key))
                    if (SessionState.IsVisible(origin, tableEntry.Value))
                        resultFound = true;
                        _matchingAliases.Add(tableEntry.Value);
                if (!resultFound &&
                    !WildcardPattern.ContainsWildcardCharacters(aliasName))
                    // Need to write an error if the user tries to get an alias
                    // that doesn't exist and they are not globbing.
                    ItemNotFoundException itemNotFound =
                            "AliasNotFound",
                            SessionStateStrings.AliasNotFound);
        /// Writes the aliases to the file.
            StreamWriter writer = null;
            FileInfo readOnlyFileInfo = null;
                if (ShouldProcess(Path))
                    writer = OpenFile(out readOnlyFileInfo);
                if (writer != null)
                    WriteHeader(writer);
                // Now write out the aliases
                foreach (AliasInfo alias in _matchingAliases)
                    string line = null;
                    if (this.As == ExportAliasFormat.Csv)
                        line = GetAliasLine(alias, "\"{0}\",\"{1}\",\"{2}\",\"{3}\"");
                        line = GetAliasLine(alias, "set-alias -Name:\"{0}\" -Value:\"{1}\" -Description:\"{2}\" -Option:\"{3}\"");
                    writer?.WriteLine(line);
                        WriteObject(alias);
                writer?.Dispose();
                if (readOnlyFileInfo != null)
                    readOnlyFileInfo.Attributes |= FileAttributes.ReadOnly;
        /// Holds all the matching aliases for writing to the file.
        private readonly Collection<AliasInfo> _matchingAliases = new();
        private static string GetAliasLine(AliasInfo alias, string formatString)
            // Using the invariant culture here because we don't want the
            // file to vary based on locale.
            string result =
                    System.Globalization.CultureInfo.InvariantCulture,
                    alias.Name,
                    alias.Definition,
                    alias.Description,
                    alias.Options);
        private void WriteHeader(StreamWriter writer)
            WriteFormattedResourceString(writer, AliasCommandStrings.ExportAliasHeaderTitle);
            string user = Environment.UserName;
            WriteFormattedResourceString(writer, AliasCommandStrings.ExportAliasHeaderUser, user);
            DateTime now = DateTime.Now;
            WriteFormattedResourceString(writer, AliasCommandStrings.ExportAliasHeaderDate, now);
            string machine = Environment.MachineName;
            WriteFormattedResourceString(writer, AliasCommandStrings.ExportAliasHeaderMachine, machine);
            // Now write the description if there is one
                // First we need to break up the description on newlines and add a
                // # for each line.
                Description = Description.Replace("\n", "\n# ");
                // Now write out the description
                writer.WriteLine("#");
                writer.Write("# ");
                writer.WriteLine(Description);
        private static void WriteFormattedResourceString(
            StreamWriter writer,
            params object[] args)
            string line = StringUtil.Format(resourceId, args);
        /// Open the file to which aliases should be exported.
        /// <param name="readOnlyFileInfo">
        /// If not null, this is the file whose read-only attribute
        /// was cleared (due to the -Force parameter).  The attribute
        /// should be reset.
        private StreamWriter OpenFile(out FileInfo readOnlyFileInfo)
            StreamWriter result = null;
            FileStream file = null;
            readOnlyFileInfo = null;
                EncodingConversion.Unicode,
                false, // defaultEncoding
                out file,
                out result,
                out readOnlyFileInfo,
                _isLiteralPath
        private void ThrowFileOpenError(Exception e, string pathWithError)
            string message = StringUtil.Format(AliasCommandStrings.ExportAliasFileOpenFailed, pathWithError, e.Message);
                "FileOpenFailure",
                ErrorCategory.OpenError,
                pathWithError);
            errorRecord.ErrorDetails = new ErrorDetails(message);

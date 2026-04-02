    /// A command to resolve PowerShell paths containing glob characters to
    /// PowerShell paths that match the glob strings.
    [Cmdlet(VerbsCommon.Split, "Path", DefaultParameterSetName = "ParentSet", SupportsTransactions = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097149")]
    [OutputType(typeof(string), ParameterSetName = new[] { leafSet,
                                                           leafBaseSet,
                                                           extensionSet,
                                                           noQualifierSet,
                                                           parentSet,
                                                           qualifierSet,
                                                           literalPathSet})]
    [OutputType(typeof(bool), ParameterSetName = new[] { isAbsoluteSet })]
    public class SplitPathCommand : CoreCommandWithCredentialsBase
        /// The parameter set name to get the parent path.
        private const string parentSet = "ParentSet";
        /// The parameter set name to get the leaf name.
        private const string leafSet = "LeafSet";
        /// The parameter set name to get the leaf base name.
        private const string leafBaseSet = "LeafBaseSet";
        /// The parameter set name to get the extension.
        private const string extensionSet = "ExtensionSet";
        /// The parameter set name to get the qualifier set.
        private const string qualifierSet = "QualifierSet";
        /// The parameter set name to get the noqualifier set.
        private const string noQualifierSet = "NoQualifierSet";
        /// The parameter set name to get the IsAbsolute set.
        private const string isAbsoluteSet = "IsAbsoluteSet";
        /// The parameter set name to get the LiteralPath set.
        private const string literalPathSet = "LiteralPathSet";
        [Parameter(Position = 0, ParameterSetName = parentSet, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Parameter(Position = 0, ParameterSetName = leafSet, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Parameter(Position = 0, ParameterSetName = leafBaseSet, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Parameter(Position = 0, ParameterSetName = extensionSet, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Parameter(Position = 0, ParameterSetName = qualifierSet, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Parameter(Position = 0, ParameterSetName = noQualifierSet, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Parameter(Position = 0, ParameterSetName = isAbsoluteSet, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = "LiteralPathSet", Mandatory = true, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true)]
        /// Determines if the qualifier should be returned.
        /// <value>
        /// If true the qualifier of the path will be returned.
        /// The qualifier is the drive or provider that is qualifying
        /// the PowerShell path.
        /// </value>
        [Parameter(ParameterSetName = qualifierSet, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public SwitchParameter Qualifier { get; set; }
        [Parameter(ParameterSetName = noQualifierSet, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public SwitchParameter NoQualifier { get; set; }
        /// Determines if the parent path should be returned.
        /// If true the parent of the path will be returned.
        [Parameter(ParameterSetName = parentSet, Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public SwitchParameter Parent { get; set; } = true;
        /// Determines if the leaf name should be returned.
        /// If true the leaf name of the path will be returned.
        [Parameter(ParameterSetName = leafSet, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public SwitchParameter Leaf { get; set; }
        /// Determines if the leaf base name (name without extension) should be returned.
        /// If true the leaf base name of the path will be returned.
        [Parameter(ParameterSetName = leafBaseSet, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public SwitchParameter LeafBase { get; set; }
        /// Determines if the extension should be returned.
        /// If true the extension of the path will be returned.
        [Parameter(ParameterSetName = extensionSet, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public SwitchParameter Extension { get; set; }
        /// Determines if the path should be resolved before being parsed.
        /// Determines if the path is an absolute path.
        [Parameter(ParameterSetName = isAbsoluteSet, Mandatory = true)]
        public SwitchParameter IsAbsolute { get; set; }
            StringCollection pathsToParse = new();
                    // resolve the paths and then parse each one.
                    Collection<PathInfo> resolvedPaths;
                            SessionState.Path.GetResolvedPSPathFromPSPath(path, currentContext);
                    foreach (PathInfo resolvedPath in resolvedPaths)
                            if (InvokeProvider.Item.Exists(resolvedPath.Path, currentContext))
                                pathsToParse.Add(resolvedPath.Path);
                pathsToParse.AddRange(Path);
            // Now parse each path
            for (int index = 0; index < pathsToParse.Count; ++index)
                string result = null;
                // Check switch parameters in order of specificity
                if (IsAbsolute)
                    string ignored;
                    bool isPathAbsolute =
                        SessionState.Path.IsPSAbsolute(pathsToParse[index], out ignored);
                    WriteObject(isPathAbsolute);
                else if (Qualifier)
                    int separatorIndex = pathsToParse[index].IndexOf(':');
                    if (separatorIndex < 0)
                        FormatException e =
                                StringUtil.Format(NavigationResources.ParsePathFormatError, pathsToParse[index]));
                                "ParsePathFormatError", // RENAME
                                pathsToParse[index]));
                        // Check to see if it is provider or drive qualified
                        if (SessionState.Path.IsProviderQualified(pathsToParse[index]))
                            // The plus 2 is for the length of the provider separator
                            // which is "::"
                            result =
                                pathsToParse[index].Substring(
                                    separatorIndex + 2);
                                    separatorIndex + 1);
                else if (Leaf || LeafBase || Extension)
                            SessionState.Path.ParseChildName(
                                pathsToParse[index],
                                true);
                        if (LeafBase)
                            result = System.IO.Path.GetFileNameWithoutExtension(result);
                        else if (Extension)
                            result = System.IO.Path.GetExtension(result);
                    catch (PSNotSupportedException)
                        // Since getting the leaf part of a path is not supported,
                        // the provider must be a container, item, or drive
                        // provider.  Since the paths for these types of
                        // providers can't be split, asking for the leaf
                        // is asking for the specified path back.
                        result = pathsToParse[index];
                else if (NoQualifier)
                    result = RemoveQualifier(pathsToParse[index]);
                    // None of the switch parameters are true: default to -Parent behavior
                            SessionState.Path.ParseParent(
                        // Since getting the parent path is not supported,
                        // providers can't be split, asking for the parent
                        // is asking for an empty string.
                        result = string.Empty;
        /// Removes either the drive or provider qualifier or both from the path.
        /// The path to strip the provider qualifier from.
        /// The path without the qualifier.
        private string RemoveQualifier(string path)
                path != null,
                "Path should be verified by the caller");
            string result = path;
            if (SessionState.Path.IsProviderQualified(path))
                int index = path.IndexOf("::", StringComparison.Ordinal);
                    // remove the qualifier
                    result = path.Substring(index + 2);
                string driveName = string.Empty;
                if (SessionState.Path.IsPSAbsolute(path, out driveName))
                    var driveNameLength = driveName.Length;
                    if (path.Length > (driveNameLength + 1) && path[driveNameLength] == ':')
                        // Remove the drive name and colon
                        result = path.Substring(driveNameLength + 1);

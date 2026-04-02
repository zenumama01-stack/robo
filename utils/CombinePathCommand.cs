using Dbg = System.Management.Automation;
    /// A command that adds the parent and child parts of a path together
    /// with the appropriate path separator.
    [Cmdlet(VerbsCommon.Join, "Path", SupportsTransactions = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096811")]
    [OutputType(typeof(string))]
    public class JoinPathCommand : CoreCommandWithCredentialsBase
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        /// Gets or sets the childPath parameter to the command.
        [AllowNull]
        [AllowEmptyString]
        public string[] ChildPath { get; set; }
        /// Gets or sets additional childPaths to the command.
        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, ValueFromRemainingArguments = true)]
        public string[] AdditionalChildPath { get; set; } = Array.Empty<string>();
        /// Determines if the path should be resolved after being joined.
        public SwitchParameter Resolve { get; set; }
        /// Gets or sets the extension to use for the resulting path.
        /// If not specified, the original extension (if any) is preserved.
        /// Behavior:
        /// - If the path has an existing extension, it will be replaced with the specified extension.
        /// - If the path does not have an extension, the specified extension will be added.
        /// - If an empty string is provided, any existing extension will be removed.
        /// - A leading dot in the extension is optional; if omitted, one will be added automatically.
        public string Extension { get; set; }
        /// Parses the specified path and returns the portion determined by the
        /// boolean parameters.
            Dbg.Diagnostics.Assert(
                Path != null,
                "Since Path is a mandatory parameter, paths should never be null");
            string combinedChildPath = string.Empty;
            if (this.ChildPath != null)
                foreach (string childPath in this.ChildPath)
                    combinedChildPath = SessionState.Path.Combine(combinedChildPath, childPath, CmdletProviderContext);
            // join the ChildPath elements
            if (AdditionalChildPath != null)
                foreach (string childPath in AdditionalChildPath)
                // First join the path elements
                string joinedPath = null;
                    joinedPath =
                        SessionState.Path.Combine(path, combinedChildPath, CmdletProviderContext);
                // If Extension parameter is present it is not null due to [ValidateNotNull].
                if (Extension is not null)
                    joinedPath = System.IO.Path.ChangeExtension(joinedPath, Extension.Length == 0 ? null : Extension);
                if (Resolve)
                    // Resolve the paths. The default API (GetResolvedPSPathFromPSPath)
                    // does not allow non-existing paths.
                    Collection<PathInfo> resolvedPaths = null;
                        resolvedPaths =
                            SessionState.Path.GetResolvedPSPathFromPSPath(joinedPath, CmdletProviderContext);
                    for (int index = 0; index < resolvedPaths.Count; ++index)
                            if (resolvedPaths[index] != null)
                                WriteObject(resolvedPaths[index].Path);
                    if (joinedPath != null)
                        WriteObject(joinedPath);

    /// This class implements Import-PowerShellDataFile command.
    [Cmdlet(VerbsData.Import, "PowerShellDataFile", DefaultParameterSetName = "ByPath",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=623621", RemotingCapability = RemotingCapability.None)]
    public class ImportPowerShellDataFileCommand : PSCmdlet
        private bool _isLiteralPath;
        /// Path specified, using globbing to resolve.
        /// Specifies a path to one or more locations, without globbing.
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ByLiteralPath", ValueFromPipelineByPropertyName = true)]
            get { return _isLiteralPath ? Path : null; }
            set { _isLiteralPath = true; Path = value; }
        /// Gets or sets switch that determines if built-in limits are applied to the data.
        public SwitchParameter SkipLimitCheck { get; set; }
        /// For each path, resolve it, parse it and write all hashtables to the output stream.
            foreach (var path in Path)
                var resolved = PathUtils.ResolveFilePath(path, this, _isLiteralPath);
                if (!string.IsNullOrEmpty(resolved) && System.IO.File.Exists(resolved))
                    var ast = Parser.ParseFile(resolved, out tokens, out errors);
                    if (errors.Length > 0)
                        WriteInvalidDataFileError(resolved, "CouldNotParseAsPowerShellDataFile");
                        var data = ast.Find(static a => a is HashtableAst, false);
                            WriteObject(data.SafeGetValue(SkipLimitCheck));
                            WriteInvalidDataFileError(resolved, "CouldNotParseAsPowerShellDataFileNoHashtableRoot");
                    WritePathNotFoundError(path);
        private void WritePathNotFoundError(string path)
            const string errorId = "PathNotFound";
            const ErrorCategory errorCategory = ErrorCategory.InvalidArgument;
            var errorMessage = string.Format(UtilityCommonStrings.PathDoesNotExist, path);
            var exception = new ArgumentException(errorMessage);
            var errorRecord = new ErrorRecord(exception, errorId, errorCategory, path);
        private void WriteInvalidDataFileError(string resolvedPath, string errorId)
            const ErrorCategory errorCategory = ErrorCategory.InvalidData;
            var errorMessage = string.Format(UtilityCommonStrings.CouldNotParseAsPowerShellDataFile, resolvedPath);
            var exception = new InvalidOperationException(errorMessage);
            var errorRecord = new ErrorRecord(exception, errorId, errorCategory, resolvedPath);

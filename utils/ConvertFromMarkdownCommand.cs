using Microsoft.PowerShell.MarkdownRender;
    /// Converts a Markdown string to a MarkdownInfo object.
    /// The conversion can be done into a HTML text or VT100 encoding string.
        VerbsData.ConvertFrom, "Markdown",
        DefaultParameterSetName = PathParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?linkid=2006503")]
    [OutputType(typeof(Microsoft.PowerShell.MarkdownRender.MarkdownInfo))]
    public class ConvertFromMarkdownCommand : PSCmdlet
        /// Gets or sets path to the file to convert from Markdown to MarkdownInfo.
        [Parameter(ParameterSetName = PathParameterSet, Mandatory = true, Position = 0)]
        /// Gets or sets the path to the file to convert from Markdown to MarkdownInfo.
        [Parameter(ParameterSetName = LiteralPathParameterSet, Mandatory = true)]
        public string[] LiteralPath { get; set; }
        /// Gets or sets the InputObject of type System.IO.FileInfo or string with content to convert from Markdown to MarkdownInfo.
        [Parameter(ParameterSetName = InputObjParamSet, Mandatory = true, ValueFromPipeline = true)]
        /// Gets or sets if the Markdown document should be converted to a VT100 encoded string.
        public SwitchParameter AsVT100EncodedString { get; set; }
        private const string PathParameterSet = "PathParamSet";
        private const string LiteralPathParameterSet = "LiteralParamSet";
        private const string InputObjParamSet = "InputObjParamSet";
        private MarkdownConversionType _conversionType = MarkdownConversionType.HTML;
        private PSMarkdownOptionInfo _mdOption = null;
        /// Read the PSMarkdownOptionInfo set in SessionState.
            _mdOption = PSMarkdownOptionInfoCache.Get(this.CommandInfo);
            bool? supportsVT100 = this.Host?.UI.SupportsVirtualTerminal;
            // supportsVT100 == null if the host is null.
            // supportsVT100 == false if host does not support VT100.
            if (supportsVT100 != true)
                _mdOption.EnableVT100Encoding = false;
            if (AsVT100EncodedString)
                _conversionType = MarkdownConversionType.VT100;
        /// Override ProcessRecord.
                case InputObjParamSet:
                    object baseObj = InputObject.BaseObject;
                    if (baseObj is FileInfo fileInfo)
                        WriteObject(
                            MarkdownConverter.Convert(
                                ReadContentFromFile(fileInfo.FullName)?.Result,
                                _conversionType,
                                _mdOption));
                    else if (baseObj is string inpObj)
                        WriteObject(MarkdownConverter.Convert(inpObj, _conversionType, _mdOption));
                        string errorMessage = StringUtil.Format(ConvertMarkdownStrings.InvalidInputObjectType, baseObj.GetType());
                            new InvalidDataException(errorMessage),
                            "InvalidInputObject",
                            InputObject);
                    ConvertEachFile(Path, _conversionType, isLiteral: false, optionInfo: _mdOption);
                    ConvertEachFile(LiteralPath, _conversionType, isLiteral: true, optionInfo: _mdOption);
        private void ConvertEachFile(IEnumerable<string> paths, MarkdownConversionType conversionType, bool isLiteral, PSMarkdownOptionInfo optionInfo)
            foreach (var path in paths)
                var resolvedPaths = ResolvePath(path, isLiteral);
                foreach (var resolvedPath in resolvedPaths)
                                ReadContentFromFile(resolvedPath)?.Result,
                                conversionType,
                                optionInfo));
        private async Task<string> ReadContentFromFile(string filePath)
                using (StreamReader reader = new(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
                    string mdContent = await reader.ReadToEndAsync();
                    return mdContent;
            catch (FileNotFoundException fnfe)
                    fnfe,
                    "FileNotFound",
                    filePath);
            catch (SecurityException se)
                    se,
                    "FileSecurityError",
                    ErrorCategory.SecurityError,
            catch (UnauthorizedAccessException uae)
                    uae,
                    "FileUnauthorizedAccess",
        private List<string> ResolvePath(string path, bool isLiteral)
                if (isLiteral)
                    resolvedPaths.Add(Context.SessionState.Path.GetUnresolvedProviderPathFromPSPath(path, out provider, out drive));
                    resolvedPaths.AddRange(Context.SessionState.Path.GetResolvedProviderPathFromPSPath(path, out provider));
            catch (ItemNotFoundException infe)
                    infe,
            if (!provider.Name.Equals("FileSystem", StringComparison.OrdinalIgnoreCase))
                string errorMessage = StringUtil.Format(ConvertMarkdownStrings.FileSystemPathsOnly, path);
                    new ArgumentException(),
                    "OnlyFileSystemPathsSupported",
            return resolvedPaths;

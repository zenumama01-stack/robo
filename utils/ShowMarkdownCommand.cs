    /// Show the VT100EncodedString or Html property of on console or show.
    /// VT100EncodedString will be displayed on console.
    /// Html will be displayed in default browser.
        VerbsCommon.Show, "Markdown",
        DefaultParameterSetName = "Path",
        HelpUri = "https://go.microsoft.com/fwlink/?linkid=2102329")]
    public class ShowMarkdownCommand : PSCmdlet
        /// Gets or sets InputObject of type Microsoft.PowerShell.MarkdownRender.MarkdownInfo to display.
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        /// Gets or sets path to markdown file(s) to display.
        [Parameter(Position = 0, Mandatory = true,
                   ValueFromPipelineByPropertyName = true, ParameterSetName = "Path")]
        /// Gets or sets the literal path parameter to markdown files(s) to display.
            get { return Path; }
            set { Path = value; }
        /// Gets or sets the switch to view Html in default browser.
        public SwitchParameter UseBrowser { get; set; }
        private System.Management.Automation.PowerShell _powerShell;
        /// Override BeginProcessing.
            _powerShell = System.Management.Automation.PowerShell.Create(RunspaceMode.CurrentRunspace);
                case "InputObject":
                    if (InputObject.BaseObject is MarkdownInfo markdownInfo)
                        ProcessMarkdownInfo(markdownInfo);
                        ConvertFromMarkdown("InputObject", InputObject.BaseObject);
                case "Path":
                case "LiteralPath":
                    ConvertFromMarkdown(ParameterSetName, Path);
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ConvertMarkdownStrings.InvalidParameterSet, ParameterSetName));
        /// Process markdown as path.
        /// <param name="parameter">Name of parameter to pass to `ConvertFrom-Markdown`.</param>
        /// <param name="input">Value of parameter.</param>
        private void ConvertFromMarkdown(string parameter, object input)
            _powerShell.AddCommand("Microsoft.PowerShell.Utility\\ConvertFrom-Markdown").AddParameter(parameter, input);
            if (!UseBrowser)
                _powerShell.AddParameter("AsVT100EncodedString");
            Collection<MarkdownInfo> output = _powerShell.Invoke<MarkdownInfo>();
            if (_powerShell.HadErrors)
                foreach (ErrorRecord errorRecord in _powerShell.Streams.Error)
            foreach (MarkdownInfo result in output)
                ProcessMarkdownInfo(result);
        /// Process markdown as input objects.
        /// <param name="markdownInfo">Markdown object to process.</param>
        private void ProcessMarkdownInfo(MarkdownInfo markdownInfo)
            if (UseBrowser)
                var html = markdownInfo.Html;
                if (!string.IsNullOrEmpty(html))
                    string tmpFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".html");
                        using (var writer = new StreamWriter(new FileStream(tmpFilePath, FileMode.Create, FileAccess.Write, FileShare.Write)))
                            writer.Write(html);
                            "ErrorWritingTempFile",
                            tmpFilePath);
                    if (InternalTestHooks.ShowMarkdownOutputBypass)
                        WriteObject(html);
                        startInfo.FileName = tmpFilePath;
                        startInfo.UseShellExecute = true;
                        Process.Start(startInfo);
                            "ErrorLaunchingDefaultApplication",
                            targetObject: null);
                    string errorMessage = StringUtil.Format(ConvertMarkdownStrings.MarkdownInfoInvalid, "Html");
                        "HtmlIsNullOrEmpty",
                        html);
                var vt100String = markdownInfo.VT100EncodedString;
                if (!string.IsNullOrEmpty(vt100String))
                    WriteObject(vt100String);
                    string errorMessage = StringUtil.Format(ConvertMarkdownStrings.MarkdownInfoInvalid, "VT100EncodedString");
                        "VT100EncodedStringIsNullOrEmpty",
                        vt100String);
            _powerShell?.Dispose();

    /// Defines the implementation of the 'Set-Clipboard' cmdlet.
    /// This cmdlet gets the content from system clipboard.
    [Cmdlet(VerbsCommon.Set, "Clipboard", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2109826")]
    [Alias("scb")]
    public class SetClipboardCommand : PSCmdlet
        private readonly List<string> _contentList = new();
        /// Property that sets clipboard content.
        [System.Management.Automation.AllowNull]
        public string[] Value { get; set; }
        /// Property that sets append parameter. This will allow to append clipboard without clear it.
        public SwitchParameter Append { get; set; }
        /// Gets or sets if the values sent down the pipeline.
        /// Gets or sets whether to use OSC52 escape sequence to set the clipboard of host instead of target.
        [Alias("ToLocalhost")]
        public SwitchParameter AsOSC52 { get; set; }
        /// This method implements the BeginProcessing method for Set-Clipboard command.
            _contentList.Clear();
        /// This method implements the ProcessRecord method for Set-Clipboard command.
            if (Value != null)
                _contentList.AddRange(Value);
                    WriteObject(Value);
        /// This method implements the EndProcessing method for Set-Clipboard command.
            SetClipboardContent(_contentList, Append);
        /// Set the clipboard content.
        /// <param name="contentList">The content to store into the clipboard.</param>
        /// <param name="append">If true, appends to clipboard instead of overwriting.</param>
        private void SetClipboardContent(List<string> contentList, bool append)
            string setClipboardShouldProcessTarget;
            if ((contentList == null || contentList.Count == 0) && !append)
                setClipboardShouldProcessTarget = string.Format(CultureInfo.InvariantCulture, ClipboardResources.ClipboardCleared);
                if (ShouldProcess(setClipboardShouldProcessTarget, "Set-Clipboard"))
                    Clipboard.SetText(string.Empty);
            StringBuilder content = new();
            if (append)
                content.AppendLine(Clipboard.GetText());
            if (contentList != null)
                content.Append(string.Join(Environment.NewLine, contentList.ToArray(), 0, contentList.Count));
            string verboseString = null;
                verboseString = contentList[0];
                if (verboseString.Length >= 20)
                    verboseString = verboseString.Substring(0, 20);
                    verboseString += " ...";
                setClipboardShouldProcessTarget = string.Format(CultureInfo.InvariantCulture, ClipboardResources.AppendClipboardContent, verboseString);
                setClipboardShouldProcessTarget = string.Format(CultureInfo.InvariantCulture, ClipboardResources.SetClipboardContent, verboseString);
                SetClipboardContent(content.ToString());
        /// <param name="content">The content to store into the clipboard.</param>
        private void SetClipboardContent(string content)
            if (!AsOSC52)
                Clipboard.SetText(content);
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var encoded = System.Convert.ToBase64String(bytes);
            var osc = $"\u001B]52;;{encoded}\u0007";
            var message = new HostInformationMessage { Message = osc, NoNewLine = true };
            WriteInformation(message, new string[] { "PSHOST" });

using System.Security.Cryptography.Pkcs;
    /// Defines the implementation of the 'Protect-CmsMessage' cmdlet.
    /// This cmdlet generates a new encrypted CMS message given the
    /// recipient and content supplied.
    [Cmdlet(VerbsSecurity.Protect, "CmsMessage", HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2096826", DefaultParameterSetName = "ByContent")]
    public sealed class ProtectCmsMessageCommand : PSCmdlet
        /// Gets or sets the recipient of the CMS Message.
        public CmsMessageRecipient[] To
        /// Gets or sets the content of the CMS Message.
        [Parameter(Position = 1, Mandatory = true, ValueFromPipeline = true, ParameterSetName = "ByContent")]
        public PSObject Content
        private readonly PSDataCollection<PSObject> _inputObjects = new();
        /// Gets or sets the content of the CMS Message by path.
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = "ByPath")]
        /// Gets or sets the content of the CMS Message by literal path.
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = "ByLiteralPath")]
        private string _resolvedPath = null;
        /// Emits the protected message to a file path.
        public string OutFile
        private string _resolvedOutFile = null;
        /// Validate / convert arguments.
            // Validate Path
            if (!string.IsNullOrEmpty(Path))
                Collection<string> resolvedPaths = GetResolvedProviderPathFromPSPath(Path, out provider);
                // Ensure the path is a single path from the file system provider
                if ((resolvedPaths.Count > 1) ||
                    (!string.Equals(provider.Name, "FileSystem", StringComparison.OrdinalIgnoreCase)))
                                CmsCommands.FilePathMustBeFileSystemPath,
                                Path)),
                        "FilePathMustBeFileSystemPath",
                        provider);
                _resolvedPath = resolvedPaths[0];
            if (!string.IsNullOrEmpty(LiteralPath))
                // Validate that the path exists
                SessionState.InvokeProvider.Item.Get(new string[] { LiteralPath }, false, true);
                _resolvedPath = LiteralPath;
            // Validate OutFile
            if (!string.IsNullOrEmpty(OutFile))
                _resolvedOutFile = GetUnresolvedProviderPathFromPSPath(OutFile);
        /// For each input object, the command encrypts
        /// and exports the object.
            if (string.Equals("ByContent", this.ParameterSetName, StringComparison.OrdinalIgnoreCase))
                _inputObjects.Add(Content);
        /// Encrypts and outputs the message.
            byte[] contentBytes = null;
                StringBuilder outputString = new();
                Collection<PSObject> output = System.Management.Automation.PowerShell.Create()
                    .AddCommand("Microsoft.PowerShell.Utility\\Out-String")
                    .AddParameter("Stream")
                    .Invoke(_inputObjects);
                foreach (PSObject outputObject in output)
                    if (outputString.Length > 0)
                        outputString.AppendLine();
                    outputString.Append(outputObject);
                contentBytes = System.Text.Encoding.UTF8.GetBytes(outputString.ToString());
                contentBytes = System.IO.File.ReadAllBytes(_resolvedPath);
            ErrorRecord terminatingError = null;
            string encodedContent = CmsUtils.Encrypt(contentBytes, To, this.SessionState, out terminatingError);
            if (terminatingError != null)
                ThrowTerminatingError(terminatingError);
            if (string.IsNullOrEmpty(_resolvedOutFile))
                WriteObject(encodedContent);
                System.IO.File.WriteAllText(_resolvedOutFile, encodedContent);
    /// Defines the implementation of the 'Get-CmsMessage' cmdlet.
    /// This cmdlet retrieves information about an encrypted CMS
    /// message.
    [Cmdlet(VerbsCommon.Get, "CmsMessage", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096598")]
    [OutputType(typeof(EnvelopedCms))]
    public sealed class GetCmsMessageCommand : PSCmdlet
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ParameterSetName = "ByContent")]
        public string Content
        private readonly StringBuilder _contentBuffer = new();
        /// Gets or sets the CMS Message by path.
        /// Gets or sets the CMS Message by literal path.
        /// For each input object, the command gets the information
        /// about the protected message and exports the object.
                if (_contentBuffer.Length > 0)
                    _contentBuffer.Append(System.Environment.NewLine);
                _contentBuffer.Append(Content);
        /// Gets the CMS Message object.
            string actualContent = null;
            // Read in the content
                actualContent = _contentBuffer.ToString();
                actualContent = System.IO.File.ReadAllText(_resolvedPath);
            // Extract out the bytes and Base64 decode them
            byte[] contentBytes = CmsUtils.RemoveAsciiArmor(actualContent, CmsUtils.BEGIN_CMS_SIGIL, CmsUtils.END_CMS_SIGIL, out int _, out int _);
            if (contentBytes == null)
                    new ArgumentException(CmsCommands.InputContainedNoEncryptedContent),
                    "InputContainedNoEncryptedContent", ErrorCategory.ObjectNotFound, null);
            EnvelopedCms cms = new();
            cms.Decode(contentBytes);
            PSObject result = new(cms);
            List<object> recipients = new();
            foreach (RecipientInfo recipient in cms.RecipientInfos)
                recipients.Add(recipient.RecipientIdentifier.Value);
            result.Properties.Add(
                new PSNoteProperty("Recipients", recipients));
                new PSNoteProperty("Content", actualContent));
    /// Defines the implementation of the 'Unprotect-CmsMessage' cmdlet.
    /// This cmdlet retrieves the clear text content of an encrypted CMS
    [Cmdlet(VerbsSecurity.Unprotect, "CmsMessage", HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2096701", DefaultParameterSetName = "ByWinEvent")]
    public sealed class UnprotectCmsMessageCommand : PSCmdlet
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByContent")]
        /// Gets or sets the Windows Event Log Message with contents to be decrypted.
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ParameterSetName = "ByWinEvent")]
        [PSTypeName("System.Diagnostics.Eventing.Reader.EventLogRecord")]
        public PSObject EventLogRecord
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "ByPath")]
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "ByLiteralPath")]
        /// Determines whether to include the decrypted content in its original context,
        /// rather than just output the decrypted content itself.
        public SwitchParameter IncludeContext
            // If we're process by content, collect it.
            // If we're processing event log records, decrypt those inline.
            if (string.Equals("ByWinEvent", this.ParameterSetName, StringComparison.OrdinalIgnoreCase))
                string actualContent = EventLogRecord.Properties["Message"].Value.ToString();
                string decrypted = Decrypt(actualContent);
                if (!IncludeContext)
                    WriteObject(decrypted);
                    EventLogRecord.Properties["Message"].Value = decrypted;
                    WriteObject(EventLogRecord);
        private string Decrypt(string actualContent)
            int startIndex, endIndex;
            byte[] messageBytes = CmsUtils.RemoveAsciiArmor(actualContent, CmsUtils.BEGIN_CMS_SIGIL, CmsUtils.END_CMS_SIGIL, out startIndex, out endIndex);
            if ((messageBytes == null) && (!IncludeContext))
                            CmsCommands.InputContainedNoEncryptedContentIncludeContext,
                            "-IncludeContext")),
                    "InputContainedNoEncryptedContentIncludeContext",
            // Capture the pre and post context, if there was any
            string preContext = null;
            string postContext = null;
            if (IncludeContext)
                if (startIndex > -1)
                    preContext = actualContent.Substring(0, startIndex);
                if (endIndex > -1)
                    postContext = actualContent.Substring(endIndex);
            X509Certificate2Collection certificates = new();
            if ((To != null) && (To.Length > 0))
                foreach (CmsMessageRecipient recipient in To)
                    recipient.Resolve(this.SessionState, ResolutionPurpose.Decryption, out error);
                    foreach (X509Certificate2 certificate in recipient.Certificates)
                        certificates.Add(certificate);
            string resultString = actualContent;
            if (messageBytes != null)
                cms.Decode(messageBytes);
                cms.Decrypt(certificates);
                resultString = System.Text.Encoding.UTF8.GetString(cms.ContentInfo.Content);
                if (preContext != null)
                    resultString = preContext + resultString;
                if (postContext != null)
                    resultString += postContext;
            return resultString;

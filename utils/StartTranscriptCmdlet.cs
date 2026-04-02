    /// Implements the start-transcript cmdlet.
    [Cmdlet(VerbsLifecycle.Start, "Transcript", SupportsShouldProcess = true, DefaultParameterSetName = "ByPath", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096485")]
    public sealed class StartTranscriptCommand : PSCmdlet
        /// The name of the file in which to write the transcript. If not provided, the file indicated by the variable
        /// $TRANSCRIPT is used.  If neither the filename is supplied or $TRANSCRIPT is not set, the filename shall be $HOME/My
        /// Documents/PowerShell_transcript.YYYYMMDDmmss.txt.
        [Parameter(Position = 0, ParameterSetName = "ByPath")]
                return _outFilename;
                _isFilenameSet = true;
                _outFilename = value;
        /// The literal name of the file in which to write the transcript.
        [Parameter(Position = 0, ParameterSetName = "ByLiteralPath")]
        [Parameter(Position = 0, ParameterSetName = "ByOutputDirectory")]
        public string OutputDirectory
                return _shouldAppend;
                _shouldAppend = value;
        /// Property that sets force parameter.  This will reset the read-only
        /// attribute on an existing file.
        /// The read-only attribute will not be replaced when the transcript is done.
        /// Whether to include command invocation time headers between commands.
        public SwitchParameter IncludeInvocationHeader
        /// Gets or sets whether to use minimal transcript header.
        public SwitchParameter UseMinimalHeader
        /// Starts the transcription.
            // If they haven't specified a path, figure out the correct output path.
            if (!_isFilenameSet)
                // read the filename from $TRANSCRIPT
                object value = this.GetVariableValue("global:TRANSCRIPT", null);
                // $TRANSCRIPT is not set, so create a file name (the default: $HOME/My Documents/PowerShell_transcript.YYYYMMDDmmss.txt)
                    // If they've specified an output directory, use it. Otherwise, use "My Documents"
                    if (OutputDirectory != null)
                        _outFilename = System.Management.Automation.Host.PSHostUserInterface.GetTranscriptPath(OutputDirectory, false);
                        _outFilename = System.Management.Automation.Host.PSHostUserInterface.GetTranscriptPath();
                    _outFilename = (string)PSObject.Base(value);
            // Normalize outFilename here in case it is a relative path
                string effectiveFilePath = ResolveFilePath(Path, _isLiteralPath);
                if (!ShouldProcess(effectiveFilePath))
                if (System.IO.File.Exists(effectiveFilePath))
                    if (NoClobber && !Append)
                        string message = StringUtil.Format(TranscriptStrings.TranscriptFileExistsNoClobber,
                            effectiveFilePath,
                            "NoClobber"); // prevents localization
                        Exception uae = new UnauthorizedAccessException(message);
                            uae, "NoClobber", ErrorCategory.ResourceExists, effectiveFilePath);
                        // NOTE: this call will throw
                    System.IO.FileInfo fInfo = new System.IO.FileInfo(effectiveFilePath);
                            // Note that we will not clear the ReadOnly flag later
                                TranscriptStrings.TranscriptFileReadOnly,
                                effectiveFilePath);
                            Exception innerException = new ArgumentException(errorMessage);
                            ThrowTerminatingError(new ErrorRecord(innerException, "FileReadOnly", ErrorCategory.InvalidArgument, effectiveFilePath));
                    // If they didn't specify -Append, empty the file
                    if (!_shouldAppend)
                        System.IO.File.WriteAllText(effectiveFilePath, string.Empty);
                System.Management.Automation.Remoting.PSSenderInfo psSenderInfo =
                    this.SessionState.PSVariable.GetValue("PSSenderInfo") as System.Management.Automation.Remoting.PSSenderInfo;
                Host.UI.StartTranscribing(effectiveFilePath, psSenderInfo, IncludeInvocationHeader.ToBool(), UseMinimalHeader.IsPresent);
                // ch.StartTranscribing(effectiveFilePath, Append);
                // NTRAID#Windows Out Of Band Releases-931008-2006/03/21
                // Previous behavior was to write this even if ShouldProcess
                // returned false.  Why would we want that?
                PSObject outputObject = new PSObject(
                    StringUtil.Format(TranscriptStrings.TranscriptionStarted, Path));
                outputObject.Properties.Add(new PSNoteProperty("Path", Path));
                WriteObject(outputObject);
                    Host.UI.StopTranscribing();
                    TranscriptStrings.CannotStartTranscription,
                    PSTraceSource.NewInvalidOperationException(e, errorMessage),
                    "CannotStartTranscription", ErrorCategory.InvalidOperation, null);
        /// resolve a user provided file name or path (including globbing characters)
        /// to a fully qualified file path, using the file system provider
        private string ResolveFilePath(string filePath, bool isLiteralPath)
            string path = null;
                    path = SessionState.Path.GetUnresolvedProviderPathFromPSPath(filePath);
                    Collection<string> filePaths =
                        SessionState.Path.GetResolvedProviderPathFromPSPath(filePath, out provider);
                        ReportWrongProviderType(provider.FullName);
                    if (filePaths.Count > 1)
                        ReportMultipleFilesNotSupported();
                    path = filePaths[0];
                path = null;
                CmdletProviderContext cmdletProviderContext = new CmdletProviderContext(this);
                path =
                        filePath, cmdletProviderContext, out provider, out drive);
                cmdletProviderContext.ThrowFirstErrorOrDoNothing();
        private void ReportWrongProviderType(string providerId)
                PSTraceSource.NewInvalidOperationException(TranscriptStrings.ReadWriteFileNotFileSystemProvider, providerId),
                "ReadWriteFileNotFileSystemProvider",
        private void ReportMultipleFilesNotSupported()
                PSTraceSource.NewInvalidOperationException(TranscriptStrings.MultipleFilesNotSupported),
                "MultipleFilesNotSupported",
        private bool _shouldAppend;
        private string _outFilename;
        private bool _isFilenameSet;

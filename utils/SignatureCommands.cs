    /// Defines the base class from which all signature commands
    public abstract class SignatureCommandsBase : PSCmdlet
        /// Gets or sets the path to the file for which to get or set the
        /// digital signature.
        /// Gets or sets the literal path to the file for which to get or set the
        /// Gets or sets the digital signature to be written to
        /// the output pipeline.
        protected Signature Signature
            get { return _signature; }
            set { _signature = value; }
        private Signature _signature;
        /// Gets or sets the file type of the byte array containing the content with
        [Parameter(Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByContent")]
        public string[] SourcePathOrExtension
                return _sourcePathOrExtension;
                _sourcePathOrExtension = value;
        private string[] _sourcePathOrExtension;
        /// File contents as a byte array.
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByContent")]
        public byte[] Content
        private byte[] _content;
        /// Initializes a new instance of the SignatureCommandsBase class,
        protected SignatureCommandsBase(string name) : base()
        // hide default ctor
        private SignatureCommandsBase() : base() { }
        /// For each input object, the command gets or
        /// sets the digital signature on the object, and
            if (Content == null)
                    "GetSignatureCommand: Param binder did not bind path");
                                    SignatureCommands.FileNotFound,
                                    "SignatureCommandsBaseFileNotFound", p));
                    if (paths.Count == 0)
                    bool foundFile = false;
                        if (!System.IO.Directory.Exists(path))
                            foundFile = true;
                            string resolvedFilePath = SecurityUtils.GetFilePathOfExistingFile(this, path);
                            if (resolvedFilePath == null)
                                WriteError(SecurityUtils.CreateFileNotFoundErrorRecord(
                                    "SignatureCommandsBaseFileNotFound",
                                if ((Signature = PerformAction(resolvedFilePath)) != null)
                                    WriteObject(Signature);
                    if (!foundFile)
                            SignatureCommands.CannotRetrieveFromContainer,
                            "SignatureCommandsBaseCannotRetrieveFromContainer"));
                foreach (string sourcePathOrExtension in SourcePathOrExtension)
                    if ((Signature = PerformAction(sourcePathOrExtension, Content)) != null)
        /// Performs the action (ie: get signature, or set signature)
        /// on the specified file.
        /// <param name="filePath">
        /// The name of the file on which to perform the action.
        protected abstract Signature PerformAction(string filePath);
        /// on the specified contents.
        /// <param name="fileName">
        /// The filename used for type if content is specified.
        /// The file contents on which to perform the action.
        protected abstract Signature PerformAction(string fileName, byte[] content);
    /// Defines the implementation of the 'get-AuthenticodeSignature' cmdlet.
    /// This cmdlet extracts the digital signature from the given file.
    [Cmdlet(VerbsCommon.Get, "AuthenticodeSignature", DefaultParameterSetName = "ByPath", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096823")]
    [OutputType(typeof(Signature))]
    public sealed class GetAuthenticodeSignatureCommand : SignatureCommandsBase
        /// Initializes a new instance of the GetSignatureCommand class.
        public GetAuthenticodeSignatureCommand() : base("Get-AuthenticodeSignature") { }
        /// Gets the signature from the specified file.
        /// The signature on the specified file.
        protected override Signature PerformAction(string filePath)
            return SignatureHelper.GetSignature(filePath, null);
        /// Gets the signature from the specified file contents.
        /// <param name="sourcePathOrExtension">The file type associated with the contents.</param>
        /// The contents of the file on which to perform the action.
        /// The signature on the specified file contents.
        protected override Signature PerformAction(string sourcePathOrExtension, byte[] content)
            return SignatureHelper.GetSignature(sourcePathOrExtension, content);
    /// Defines the implementation of the 'set-AuthenticodeSignature' cmdlet.
    /// This cmdlet sets the digital signature on a given file.
    [Cmdlet(VerbsCommon.Set, "AuthenticodeSignature", SupportsShouldProcess = true, DefaultParameterSetName = "ByPath",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096919")]
    public sealed class SetAuthenticodeSignatureCommand : SignatureCommandsBase
        /// Initializes a new instance of the SetAuthenticodeSignatureCommand class.
        public SetAuthenticodeSignatureCommand() : base("set-AuthenticodeSignature") { }
        /// Gets or sets the certificate with which to sign the
        /// file.
        public X509Certificate2 Certificate
                return _certificate;
                _certificate = value;
        private X509Certificate2 _certificate;
        /// Gets or sets the additional certificates to
        /// include in the digital signature.
        /// Use 'signer' to include only the signer's certificate.
        /// Use 'notroot' to include all certificates in the certificate
        ///    chain, except for the root authority.
        /// Use 'all' to include all certificates in the certificate chain.
        /// Defaults to 'notroot'.
        [ValidateSet("signer", "notroot", "all")]
        public string IncludeChain
                return _includeChain;
                _includeChain = value;
        private string _includeChain = "notroot";
        /// Gets or sets the Url of the time stamping server.
        /// The time stamping server certifies the exact time
        /// that the certificate was added to the file.
        public string TimestampServer
                return _timestampServer;
                value ??= string.Empty;
                _timestampServer = value;
        private string _timestampServer = string.Empty;
        /// Gets or sets the hash algorithm used for signing.
        /// This string value must represent the name of a Cryptographic Algorithm
        /// Identifier supported by Windows.
        public string HashAlgorithm
                return _hashAlgorithm;
                _hashAlgorithm = value;
        private string _hashAlgorithm = "SHA256";
        /// Sets the digital signature on the specified file.
            SigningOption option = GetSigningOption(IncludeChain);
            if (Certificate == null)
                throw PSTraceSource.NewArgumentNullException("certificate");
            // if the cert is not good for signing, we cannot
            // process any more files. Exit the command.
            if (!SecuritySupport.CertIsGoodForSigning(Certificate))
                Exception e = PSTraceSource.NewArgumentException(
                        "certificate",
                        SignatureCommands.CertNotGoodForSigning);
            if (!ShouldProcess(filePath))
                if (this.Force)
                        FileInfo fInfo = new(filePath);
                                // remember to reset the read-only attribute later
                                readOnlyFileInfo = fInfo;
                    // These are the known exceptions for File.Load and StreamWriter.ctor
                            "ForceArgumentException",
                            "ForceIOException",
                            "ForceUnauthorizedAccessException",
                            "ForceNotSupportedException",
                    catch (System.Security.SecurityException e)
                            "ForceSecurityException",
                // ProcessRecord() code in base class has already
                // ascertained that filePath really represents an existing
                // file. Thus we can safely call GetFileSize() below.
                if (SecurityUtils.GetFileSize(filePath) < 4)
                    // Note that the message param comes first
                        UtilsStrings.FileSmallerThan4Bytes, filePath);
                    PSArgumentException e = new(message, nameof(filePath));
                    ErrorRecord er = SecurityUtils.CreateInvalidArgumentErrorRecord(
                            "SignatureCommandsBaseFileSmallerThan4Bytes"
                return SignatureHelper.SignFile(option,
                                                TimestampServer,
                                                _hashAlgorithm);
        private struct SigningOptionInfo
            internal SigningOption option;
            internal string optionName;
            internal SigningOptionInfo(SigningOption o, string n)
                option = o;
                optionName = n;
        /// Association between SigningOption.* values and the
        /// corresponding string names.
        private static readonly SigningOptionInfo[] s_sigOptionInfo =
            new SigningOptionInfo(SigningOption.AddOnlyCertificate, "signer"),
            new SigningOptionInfo(SigningOption.AddFullCertificateChainExceptRoot, "notroot"),
            new SigningOptionInfo(SigningOption.AddFullCertificateChain, "all")
        /// Get SigningOption value corresponding to a string name.
        /// <param name="optionName">Name of option.</param>
        /// <returns>SigningOption.</returns>
        private static SigningOption GetSigningOption(string optionName)
            foreach (SigningOptionInfo si in s_sigOptionInfo)
                if (string.Equals(optionName, si.optionName,
                    return si.option;
            return SigningOption.AddFullCertificateChainExceptRoot;

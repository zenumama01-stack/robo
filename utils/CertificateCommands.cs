    /// Defines the implementation of the get-pfxcertificate cmdlet.
    [Cmdlet(VerbsCommon.Get, "PfxCertificate", DefaultParameterSetName = "ByPath", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096918")]
    [OutputType(typeof(X509Certificate2))]
    public sealed class GetPfxCertificateCommand : PSCmdlet
        /// certificate.
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Mandatory = true, ParameterSetName = "ByPath")]
        public string[] FilePath
        [Parameter(ValueFromPipelineByPropertyName = true, Mandatory = true, ParameterSetName = "ByLiteralPath")]
        /// Gets or sets the password for unlocking the certificate.
        public SecureString Password { get; set; }
        /// Do not prompt for password if not given.
        public SwitchParameter NoPromptForPassword { get; set; }
        // list of files that were not found
        private readonly List<string> _filesNotFound = new();
        /// Initializes a new instance of the GetPfxCertificateCommand
        public GetPfxCertificateCommand() : base()
        /// corresponding certificate.
            // property to be a mandatory parameter
            Dbg.Assert((FilePath != null) && (FilePath.Length > 0),
                       "GetCertificateCommand: Param binder did not bind path");
            X509Certificate2 cert = null;
            foreach (string p in FilePath)
                List<string> paths = new();
                // Expand wildcard characters
                    paths.Add(SessionState.Path.GetUnresolvedProviderPathFromPSPath(p));
                        _filesNotFound.Add(p);
                foreach (string resolvedPath in paths)
                    string resolvedProviderPath =
                        SecurityUtils.GetFilePathOfExistingFile(this, resolvedPath);
                    if (resolvedProviderPath == null)
                        if (Password == null && !NoPromptForPassword.IsPresent)
                                cert = GetCertFromPfxFile(resolvedProviderPath, null);
                                WriteObject(cert);
                            catch (CryptographicException)
                                Password = SecurityUtils.PromptForSecureString(
                                    Host.UI,
                                    CertificateCommands.GetPfxCertPasswordPrompt);
                            cert = GetCertFromPfxFile(resolvedProviderPath, Password);
                        catch (CryptographicException e)
                                "GetPfxCertificateUnknownCryptoError",
            if (_filesNotFound.Count > 0)
                if (_filesNotFound.Count == FilePath.Length)
                        SecurityUtils.CreateFileNotFoundErrorRecord(
                            CertificateCommands.NoneOfTheFilesFound,
                            "GetPfxCertCommandNoneOfTheFilesFound");
                    // we found some files but not others.
                    // Write error for each missing file
                    foreach (string f in _filesNotFound)
                                CertificateCommands.FileNotFound,
                                "GetPfxCertCommandFileNotFound",
                                f
        private static X509Certificate2 GetCertFromPfxFile(string path, SecureString password)
            // No overload found in X509CertificateLoader that takes SecureString
            var cert = new X509Certificate2(path, password, X509KeyStorageFlags.DefaultKeySet);
            return cert;

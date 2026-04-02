    /// Defines the base class from which all catalog commands are derived.
    public abstract class CatalogCommandsBase : PSCmdlet
        /// Path of folder/file to generate or validate the catalog file.
        public string CatalogFilePath
                return catalogFilePath;
                catalogFilePath = value;
        private string catalogFilePath;
        [Parameter(Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPath")]
                path = value;
        private string[] path;
        // name of this command
        private readonly string commandName;
        /// Initializes a new instance of the CatalogCommandsBase class,
        /// using the given command name.
        /// The name of the command.
        protected CatalogCommandsBase(string name) : base()
            commandName = name;
        private CatalogCommandsBase() : base() { }
        /// For each input object, the command either generate the Catalog or
        /// Validates the existing Catalog.
            // this cannot happen as we have specified the Path
            // property to be mandatory parameter
            Dbg.Assert((CatalogFilePath != null) && (CatalogFilePath.Length > 0),
                       "CatalogCommands: Param binder did not bind catalogFilePath");
            Collection<string> paths = new();
                    foreach (PathInfo tempPath in SessionState.Path.GetResolvedPSPathFromPSPath(p))
                        if (ShouldProcess("Including path " + tempPath.ProviderPath, string.Empty, string.Empty))
                            paths.Add(tempPath.ProviderPath);
            string drive = null;
            // resolve catalog destination Path
            if (!SessionState.Path.IsPSAbsolute(catalogFilePath, out drive) && !System.IO.Path.IsPathRooted(catalogFilePath))
                catalogFilePath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(catalogFilePath);
            if (ShouldProcess(catalogFilePath))
                PerformAction(paths, catalogFilePath);
        /// Performs the action i.e. Generate or Validate the Windows Catalog File.
        /// The name of the Folder or file on which to perform the action.
        /// <param name="catalogFilePath">
        /// Path to Catalog
        protected abstract void PerformAction(Collection<string> path, string catalogFilePath);
    /// Defines the implementation of the 'New-FileCatalog' cmdlet.
    /// This cmdlet generates the catalog for File or Folder.
    [Cmdlet(VerbsCommon.New, "FileCatalog", SupportsShouldProcess = true, DefaultParameterSetName = "ByPath",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2096596")]
    public sealed class NewFileCatalogCommand : CatalogCommandsBase
        /// Initializes a new instance of the New-FileCatalog class.
        public NewFileCatalogCommand() : base("New-FileCatalog") { }
        /// Catalog version.
        public int CatalogVersion
                return catalogVersion;
                catalogVersion = value;
        // Based on the Catalog version we will decide which hashing Algorithm to use
        private int catalogVersion = 2;
        /// Generate the Catalog for the Path.
        /// File or Folder Path
        /// True if able to Create Catalog or else False
        protected override void PerformAction(Collection<string> path, string catalogFilePath)
            if (path.Count == 0)
                // if user has not provided the path use current directory to generate catalog
                path.Add(SessionState.Path.CurrentFileSystemLocation.Path);
            FileInfo catalogFileInfo = new(catalogFilePath);
            // If Path points to the expected cat file make sure
            // parent Directory exists other wise CryptoAPI fails to create a .cat file
            if (catalogFileInfo.Extension.Equals(".cat", StringComparison.Ordinal))
                System.IO.Directory.CreateDirectory(catalogFileInfo.Directory.FullName);
                // This only creates Directory if it does not exists, Append a default name
                System.IO.Directory.CreateDirectory(catalogFilePath);
                catalogFilePath = System.IO.Path.Combine(catalogFilePath, "catalog.cat");
            FileInfo catalogFile = CatalogHelper.GenerateCatalog(this, path, catalogFilePath, catalogVersion);
            if (catalogFile != null)
                WriteObject(catalogFile);
    /// Defines the implementation of the 'Test-FileCatalog' cmdlet.
    /// This cmdlet validates the Integrity of catalog.
    [Cmdlet(VerbsDiagnostic.Test, "FileCatalog", SupportsShouldProcess = true, DefaultParameterSetName = "ByPath",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2096921")]
    [OutputType(typeof(CatalogValidationStatus))]
    [OutputType(typeof(CatalogInformation))]
    public sealed class TestFileCatalogCommand : CatalogCommandsBase
        public TestFileCatalogCommand() : base("Test-FileCatalog") { }
        public SwitchParameter Detailed
            get { return detailed; }
            set { detailed = value; }
        private bool detailed = false;
        /// Patterns used to exclude files from DiskPaths and Catalog.
        public string[] FilesToSkip
                return filesToSkip;
                filesToSkip = value;
                this.excludedPatterns = new WildcardPattern[filesToSkip.Length];
                for (int i = 0; i < filesToSkip.Length; i++)
                    this.excludedPatterns[i] = WildcardPattern.Get(filesToSkip[i], WildcardOptions.IgnoreCase);
        private string[] filesToSkip = null;
        internal WildcardPattern[] excludedPatterns = null;
        /// Validate the Integrity of given Catalog.
        /// True if able to Validate the Catalog and its not tampered or else False
                // if user has not provided the path use the path of catalog file itself.
                path.Add(new FileInfo(catalogFilePath).Directory.FullName);
            CatalogInformation catalogInfo = CatalogHelper.ValidateCatalog(this, path, catalogFilePath, excludedPatterns);
            if (detailed)
                WriteObject(catalogInfo);
                WriteObject(catalogInfo.Status);

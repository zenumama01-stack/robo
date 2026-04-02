    /// Provides information for applications that are not directly executable by PowerShell.
    /// An application is any file that is executable by Windows either directly or through
    /// file associations excluding any .ps1 files or cmdlets.
    public class ApplicationInfo : CommandInfo
        /// Creates an instance of the ApplicationInfo class with the specified name, and path.
        /// The name of the application.
        /// The path to the application executable
        /// THe engine execution context for this command...
        /// If <paramref name="path"/> or <paramref name="name"/> is null or empty
        /// or contains one or more of the invalid
        /// characters defined in InvalidPathChars.
        internal ApplicationInfo(string name, string path, ExecutionContext context) : base(name, CommandTypes.Application)
                throw PSTraceSource.NewArgumentNullException(nameof(context));
            Extension = System.IO.Path.GetExtension(path);
            _context = context;
        private readonly ExecutionContext _context;
        /// Gets the path for the application file.
        public string Path { get; } = string.Empty;
        /// Gets the extension of the application file.
        public string Extension { get; } = string.Empty;
        /// Gets the path of the application file.
        /// Gets the source of this command.
        public override string Source
            get { return this.Definition; }
        /// Gets the source version.
        public override Version Version
                if (_version == null)
                    FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Path);
                    _version = new Version(versionInfo.ProductMajorPart, versionInfo.ProductMinorPart, versionInfo.ProductBuildPart, versionInfo.ProductPrivatePart);
        /// Determine the visibility for this script...
        public override SessionStateEntryVisibility Visibility
                return _context.EngineSessionState.CheckApplicationVisibility(Path);
                throw PSTraceSource.NewNotImplementedException();
        /// An application could return nothing, but commonly it returns a string.
                if (_outputType == null)
                    List<PSTypeName> l = new List<PSTypeName>();
                    l.Add(new PSTypeName(typeof(string)));
                    _outputType = new ReadOnlyCollection<PSTypeName>(l);
                return _outputType;
        private ReadOnlyCollection<PSTypeName> _outputType = null;

using System.Security.Permissions;
    /// This exception is used by Formattable constructor to indicate errors
    /// occurred during construction time.
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "FormatTable")]
    public class FormatTableLoadException : RuntimeException
        private readonly Collection<string> _errors;
        /// This is the default constructor.
        public FormatTableLoadException()
            SetDefaultErrorRecord();
        /// This constructor takes a localized error message.
        /// A localized error message.
        public FormatTableLoadException(string message)
        /// This constructor takes a localized message and an inner exception.
        /// Localized error message.
        /// Inner exception.
        public FormatTableLoadException(string message, Exception innerException)
        /// This constructor takes a collection of errors occurred during construction
        /// time.
        /// <param name="loadErrors">
        /// The errors that occurred.
        internal FormatTableLoadException(ConcurrentBag<string> loadErrors)
            : base(StringUtil.Format(FormatAndOutXmlLoadingStrings.FormatTableLoadErrors))
            _errors = new Collection<string>(loadErrors.ToArray());
        /// This constructor is required by serialization.
        protected FormatTableLoadException(SerializationInfo info, StreamingContext context)
        /// Set the default ErrorRecord.
        protected void SetDefaultErrorRecord()
            SetErrorCategory(ErrorCategory.InvalidData);
            SetErrorId(typeof(FormatTableLoadException).FullName);
        /// The specific Formattable load errors.
        public Collection<string> Errors
                return _errors;
    /// A class that keeps the information from format.ps1xml files in a cache table.
    public sealed class FormatTable
        #region Private Data
        private readonly TypeInfoDataBaseManager _formatDBMgr;
        internal FormatTable()
            _formatDBMgr = new TypeInfoDataBaseManager();
        /// Constructor that creates a FormatTable from a set of format files.
        /// <param name="formatFiles">
        /// Format files to load for format information.
        /// 1. Path {0} is not fully qualified. Specify a fully qualified type file path.
        /// <exception cref="FormatTableLoadException">
        /// 1. There were errors loading Formattable. Look in the Errors property to
        /// get detailed error messages.
        public FormatTable(IEnumerable<string> formatFiles) : this(formatFiles, null, null)
        /// Append the formatData to the list of formatting configurations, and update the
        /// entire formatting database.
        /// <param name="formatData">
        /// The formatData is of type 'ExtendedTypeDefinition'. It defines the View configuration
        /// including TableControl, ListControl, and WideControl.
        public void AppendFormatData(IEnumerable<ExtendedTypeDefinition> formatData)
            if (formatData == null)
                throw PSTraceSource.NewArgumentNullException(nameof(formatData));
            _formatDBMgr.AddFormatData(formatData, false);
        /// Prepend the formatData to the list of formatting configurations, and update the
        public void PrependFormatData(IEnumerable<ExtendedTypeDefinition> formatData)
            _formatDBMgr.AddFormatData(formatData, true);
        /// <param name="authorizationManager">
        /// Authorization manager to perform signature checks before reading ps1xml files (or null of no checks are needed)
        /// <param name="host">
        /// Host passed to <paramref name="authorizationManager"/>.  Can be null if no interactive questions should be asked.
        internal FormatTable(IEnumerable<string> formatFiles, AuthorizationManager authorizationManager, PSHost host)
            if (formatFiles == null)
                throw PSTraceSource.NewArgumentNullException(nameof(formatFiles));
            _formatDBMgr = new TypeInfoDataBaseManager(formatFiles, true, authorizationManager, host);
        #region Internal Methods / Properties
        internal TypeInfoDataBaseManager FormatDBManager
            get { return _formatDBMgr; }
        /// Adds the <paramref name="formatFile"/> to the current FormatTable's file list.
        /// The FormatTable will not reflect the change until Update is called.
        /// <param name="formatFile"></param>
        /// <param name="shouldPrepend">
        /// if true, <paramref name="formatFile"/> is prepended to the current FormatTable's file list.
        /// if false, it will be appended.
        internal void Add(string formatFile, bool shouldPrepend)
            _formatDBMgr.Add(formatFile, shouldPrepend);
        /// Removes the <paramref name="formatFile"/> from the current FormatTable's file list.
        internal void Remove(string formatFile)
            _formatDBMgr.Remove(formatFile);
        /// Returns a format table instance with all default
        /// format files loaded.
        public static FormatTable LoadDefaultFormatFiles()
            string psHome = Utils.DefaultPowerShellAppBase;
            List<string> defaultFormatFiles = new List<string>();
            if (!string.IsNullOrEmpty(psHome))
                defaultFormatFiles.AddRange(Platform.FormatFileNames.Select(file => Path.Combine(psHome, file)));
            return new FormatTable(defaultFormatFiles);

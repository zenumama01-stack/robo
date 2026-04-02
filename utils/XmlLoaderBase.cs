 SUMMARY: this file contains a general purpose, reusable framework for
    loading XML files, and do data validation.
    It provides the capability of:
    * logging errors, warnings and traces to a file or in memory
    * managing the XML dom traversal using an add hoc stack frame management scheme
    * validating common error conditions (e.g. missing node or unknown node)
    /// Base exception to be used for all the exceptions that this framework will generate.
    internal abstract class TypeInfoDataBaseLoaderException : SystemException
    /// Exception thrown by the loader when the maximum number of errors is exceeded.
    internal class TooManyErrorsException : TypeInfoDataBaseLoaderException
        /// Error count that triggered the exception.
        internal int errorCount;
    /// Entry logged by the loader and made available to external consumers.
    internal class XmlLoaderLoggerEntry
        internal enum EntryType { Error, Trace }
        /// Type of information being logged.
        internal EntryType entryType;
        /// Path of the file the info refers to.
        internal string filePath = null;
        /// XPath location inside the file.
        internal string xPath = null;
        /// Message to be displayed to the user.
        internal string message = null;
        /// Indicate whether we fail to load the file due to the security reason.
        internal bool failToLoadFile = false;
    /// Logger object used by the loader (class XmlLoaderBase) to write log entries.
    /// It logs to a memory buffer and (optionally) to a text file.
    internal class XmlLoaderLogger : IDisposable
        // PSS/end-user tracer
        [TraceSource("FormatFileLoading", "Loading format files")]
        private static readonly PSTraceSource s_formatFileLoadingtracer = PSTraceSource.GetTracer("FormatFileLoading", "Loading format files", false);
        /// Log an entry.
        /// <param name="entry">Entry to log.</param>
        internal void LogEntry(XmlLoaderLoggerEntry entry)
            if (entry.entryType == XmlLoaderLoggerEntry.EntryType.Error)
                _hasErrors = true;
            if (_saveInMemory)
                _entries.Add(entry);
            if ((s_formatFileLoadingtracer.Options | PSTraceSourceOptions.WriteLine) != 0)
                WriteToTracer(entry);
        private static void WriteToTracer(XmlLoaderLoggerEntry entry)
                s_formatFileLoadingtracer.WriteLine("ERROR:\r\n FilePath: {0}\r\n XPath: {1}\r\n Message = {2}", entry.filePath, entry.xPath, entry.message);
            else if (entry.entryType == XmlLoaderLoggerEntry.EntryType.Trace)
                s_formatFileLoadingtracer.WriteLine("TRACE:\r\n FilePath: {0}\r\n XPath: {1}\r\n Message = {2}", entry.filePath, entry.xPath, entry.message);
        /// IDisposable implementation.
        internal List<XmlLoaderLoggerEntry> LogEntries
                return _entries;
        internal bool HasErrors
                return _hasErrors;
        /// If true, log entries to memory.
        private readonly bool _saveInMemory = true;
        /// List of entries logged if saveInMemory is true.
        private readonly List<XmlLoaderLoggerEntry> _entries = new List<XmlLoaderLoggerEntry>();
        /// True if we ever logged an error.
        private bool _hasErrors = false;
    /// Base class providing XML loading basic functionality (stack management and logging facilities)
    /// NOTE: you need to implement to load an actual XML document and traverse it as see fit.
    internal abstract class XmlLoaderBase : IDisposable
        [TraceSource("XmlLoaderBase", "XmlLoaderBase")]
        private static readonly PSTraceSource s_tracer = PSTraceSource.GetTracer("XmlLoaderBase", "XmlLoaderBase");
        /// Class representing a stack frame for the XML document tree traversal.
        private sealed class XmlLoaderStackFrame : IDisposable
            internal XmlLoaderStackFrame(XmlLoaderBase loader, XmlNode n, int index)
                _loader = loader;
                this.node = n;
                this.index = index;
                if (_loader != null)
                    _loader.RemoveStackFrame();
                    _loader = null;
            /// Back pointer to the loader, used to pop a stack frame.
            private XmlLoaderBase _loader;
            /// Node the stack frame refers to.
            internal XmlNode node;
            /// Node index for enumerations, valid only if != -1
            /// NOTE: this allows to express the XPath construct "foo[0]"
            internal int index = -1;
                if (_logger != null)
                    _logger.Dispose();
                    _logger = null;
        /// Get the list of log entries.
        /// <value>list of entries logged during a load</value>
                return _logger.LogEntries;
        /// Check if there were errors.
        /// <value>true of the log entry list has errors</value>
                return _logger.HasErrors;
        /// To be called when starting a stack frame.
        /// The returned IDisposable should be used in a using(){...} block.
        /// <param name="n">Node to push on the stack.</param>
        /// <returns>Object to dispose when exiting the frame.</returns>
        protected IDisposable StackFrame(XmlNode n)
            return StackFrame(n, -1);
        /// <param name="index">Index of the node of the same name in a collection.</param>
        protected IDisposable StackFrame(XmlNode n, int index)
            XmlLoaderStackFrame sf = new XmlLoaderStackFrame(this, n, index);
            _executionStack.Push(sf);
            if (_logStackActivity)
                WriteStackLocation("Enter");
        /// Called by the Dispose code of the XmlLoaderStackFrame object
        /// to pop a frame off the stack.
                WriteStackLocation("Exit");
            _executionStack.Pop();
        protected void ProcessUnknownNode(XmlNode n)
            if (IsFilteredOutNode(n))
            ReportIllegalXmlNode(n);
        protected void ProcessUnknownAttribute(XmlAttribute a)
            ReportIllegalXmlAttribute(a);
        protected static bool IsFilteredOutNode(XmlNode n)
            return (n is XmlComment || n is XmlWhitespace);
        protected bool VerifyNodeHasNoChildren(XmlNode n)
            if (n.ChildNodes.Count == 0)
            if (n.ChildNodes.Count == 1)
                if (n.ChildNodes[0] is XmlText)
            // Error at XPath {0} in file {1}: Node {2} cannot have children.
            this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoChildrenAllowed, ComputeCurrentXPath(), FilePath, n.Name));
        internal string GetMandatoryInnerText(XmlNode n)
            if (string.IsNullOrEmpty(n.InnerText))
                this.ReportEmptyNode(n);
            return n.InnerText;
        internal string GetMandatoryAttributeValue(XmlAttribute a)
            if (string.IsNullOrEmpty(a.Value))
                this.ReportEmptyAttribute(a);
            return a.Value;
        /// Helper to compare node names, e.g. "foo" in <foo/>
        /// it uses case sensitive, culture invariant compare.
        /// This is because XML tags are case sensitive.
        /// <param name="n">XmlNode whose name is to compare.</param>
        /// <param name="s">String to compare the node name to.</param>
        /// <param name="allowAttributes">If true, accept the presence of attributes on the node.</param>
        private bool MatchNodeNameHelper(XmlNode n, string s, bool allowAttributes)
            if (string.Equals(n.Name, s, StringComparison.Ordinal))
                // we have a case sensitive match
            else if (string.Equals(n.Name, s, StringComparison.OrdinalIgnoreCase))
                // try a case insensitive match
                // we differ only in case: flag this as an ERROR for the time being
                // and accept the comparison
                const string fmtString = "XML tag differ in case only {0} {1}";
                ReportTrace(string.Format(CultureInfo.InvariantCulture, fmtString, n.Name, s));
            if (match && !allowAttributes && n is XmlElement e && e.Attributes.Count > 0)
                // Error at XPath {0} in file {1}: The XML Element {2} does not allow attributes.
                ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.AttributesNotAllowed, ComputeCurrentXPath(), FilePath, n.Name));
        internal bool MatchNodeNameWithAttributes(XmlNode n, string s)
            return MatchNodeNameHelper(n, s, true);
        internal bool MatchNodeName(XmlNode n, string s)
            return MatchNodeNameHelper(n, s, false);
        internal bool MatchAttributeName(XmlAttribute a, string s)
            if (string.Equals(a.Name, s, StringComparison.Ordinal))
            else if (string.Equals(a.Name, s, StringComparison.OrdinalIgnoreCase))
                const string fmtString = "XML attribute differ in case only {0} {1}";
                ReportTrace(string.Format(CultureInfo.InvariantCulture, fmtString, a.Name, s));
        internal void ProcessDuplicateNode(XmlNode n)
            // Error at XPath {0} in file {1}: Duplicated node.
            ReportLogEntryHelper(StringUtil.Format(FormatAndOutXmlLoadingStrings.DuplicatedNode, ComputeCurrentXPath(), FilePath), XmlLoaderLoggerEntry.EntryType.Error);
        internal void ProcessDuplicateAlternateNode(string node1, string node2)
            // Error at XPath {0} in file {1}: {2} and {3} are mutually exclusive.
            ReportLogEntryHelper(StringUtil.Format(FormatAndOutXmlLoadingStrings.MutuallyExclusiveNode, ComputeCurrentXPath(), FilePath, node1, node2), XmlLoaderLoggerEntry.EntryType.Error);
        internal void ProcessDuplicateAlternateNode(XmlNode n, string node1, string node2)
            // Error at XPath {0} in file {1}: {2}, {3} and {4} are mutually exclusive.
            ReportLogEntryHelper(StringUtil.Format(FormatAndOutXmlLoadingStrings.ThreeMutuallyExclusiveNode, ComputeCurrentXPath(), FilePath, n.Name, node1, node2), XmlLoaderLoggerEntry.EntryType.Error);
        private void ReportIllegalXmlNode(XmlNode n)
            // UnknownNode=Error at XPath {0} in file {1}: {2} is an unknown node.
            ReportLogEntryHelper(StringUtil.Format(FormatAndOutXmlLoadingStrings.UnknownNode, ComputeCurrentXPath(), FilePath, n.Name), XmlLoaderLoggerEntry.EntryType.Error);
        private void ReportIllegalXmlAttribute(XmlAttribute a)
            // Error at XPath {0} in file {1}: {2} is an unknown attribute.
            ReportLogEntryHelper(StringUtil.Format(FormatAndOutXmlLoadingStrings.UnknownAttribute, ComputeCurrentXPath(), FilePath, a.Name), XmlLoaderLoggerEntry.EntryType.Error);
        protected void ReportMissingAttribute(string name)
            // Error at XPath {0} in file {1}: {2} is a missing attribute.
            ReportLogEntryHelper(StringUtil.Format(FormatAndOutXmlLoadingStrings.MissingAttribute, ComputeCurrentXPath(), FilePath, name), XmlLoaderLoggerEntry.EntryType.Error);
        protected void ReportMissingNode(string name)
            // Error at XPath {0} in file {1}: Missing Node {2}.
            ReportLogEntryHelper(StringUtil.Format(FormatAndOutXmlLoadingStrings.MissingNode, ComputeCurrentXPath(), FilePath, name), XmlLoaderLoggerEntry.EntryType.Error);
        protected void ReportMissingNodes(string[] names)
            // Error at XPath {0} in file {1}: Missing Node from {2}.
            string namesString = string.Join(", ", names);
            ReportLogEntryHelper(StringUtil.Format(FormatAndOutXmlLoadingStrings.MissingNodeFromList, ComputeCurrentXPath(), FilePath, namesString), XmlLoaderLoggerEntry.EntryType.Error);
        protected void ReportEmptyNode(XmlNode n)
            // Error at XPath {0} in file {1}: {2} is an empty node.
            ReportLogEntryHelper(StringUtil.Format(FormatAndOutXmlLoadingStrings.EmptyNode, ComputeCurrentXPath(), FilePath, n.Name), XmlLoaderLoggerEntry.EntryType.Error);
        protected void ReportEmptyAttribute(XmlAttribute a)
            // EmptyAttribute=Error at XPath {0} in file {1}: {2} is an empty attribute.
            ReportLogEntryHelper(StringUtil.Format(FormatAndOutXmlLoadingStrings.EmptyAttribute, ComputeCurrentXPath(), FilePath, a.Name), XmlLoaderLoggerEntry.EntryType.Error);
        /// For tracing purposes only, don't add to log.
        /// trace message, non-localized string is OK.
        protected void ReportTrace(string message)
            ReportLogEntryHelper(message, XmlLoaderLoggerEntry.EntryType.Trace);
        protected void ReportError(string message)
            ReportLogEntryHelper(message, XmlLoaderLoggerEntry.EntryType.Error);
        private void ReportLogEntryHelper(string message, XmlLoaderLoggerEntry.EntryType entryType, bool failToLoadFile = false)
            string currentPath = ComputeCurrentXPath();
            XmlLoaderLoggerEntry entry = new XmlLoaderLoggerEntry();
            entry.entryType = entryType;
            entry.filePath = this.FilePath;
            entry.xPath = currentPath;
            entry.message = message;
            if (failToLoadFile)
                System.Management.Automation.Diagnostics.Assert(entryType == XmlLoaderLoggerEntry.EntryType.Error, "the entry type should be 'error' when a file cannot be loaded");
                entry.failToLoadFile = true;
            _logger.LogEntry(entry);
            if (entryType == XmlLoaderLoggerEntry.EntryType.Error)
                _currentErrorCount++;
                if (_currentErrorCount >= _maxNumberOfErrors)
                    // we have to log a last error and then bail
                    if (_maxNumberOfErrors > 1)
                        XmlLoaderLoggerEntry lastEntry = new XmlLoaderLoggerEntry();
                        lastEntry.entryType = XmlLoaderLoggerEntry.EntryType.Error;
                        lastEntry.filePath = this.FilePath;
                        lastEntry.xPath = currentPath;
                        lastEntry.message = StringUtil.Format(FormatAndOutXmlLoadingStrings.TooManyErrors, FilePath);
                        _logger.LogEntry(lastEntry);
                    // NOTE: this exception is an internal one, and it is caught
                    // internally by the calling code.
                    TooManyErrorsException e = new TooManyErrorsException();
                    e.errorCount = _currentErrorCount;
        /// Report error when loading formatting data from object model.
        protected void ReportErrorForLoadingFromObjectModel(string message, string typeName)
            entry.entryType = XmlLoaderLoggerEntry.EntryType.Error;
                    lastEntry.message = StringUtil.Format(FormatAndOutXmlLoadingStrings.TooManyErrorsInFormattingData, typeName);
        private void WriteStackLocation(string label)
            ReportTrace(label);
        protected string ComputeCurrentXPath()
            StringBuilder path = new StringBuilder();
            foreach (XmlLoaderStackFrame sf in _executionStack)
                path.Insert(0, "/");
                if (sf.index != -1)
                    path.Insert(1, string.Create(CultureInfo.InvariantCulture, $"{sf.node.Name}[{sf.index + 1}]"));
                    path.Insert(1, sf.node.Name);
            return path.Length > 0 ? path.ToString() : null;
        #region helpers for loading XML documents
        protected XmlDocument LoadXmlDocumentFromFileLoadingInfo(AuthorizationManager authorizationManager, PSHost host, out bool isFullyTrusted)
            // get file contents
            ExternalScriptInfo ps1xmlInfo = new ExternalScriptInfo(FilePath, FilePath);
            string fileContents = ps1xmlInfo.ScriptContents;
            isFullyTrusted = false;
            if (ps1xmlInfo.DefiningLanguageMode == PSLanguageMode.FullLanguage)
                isFullyTrusted = true;
            if (authorizationManager != null)
                    authorizationManager.ShouldRunInternal(ps1xmlInfo, CommandOrigin.Internal, host);
                catch (PSSecurityException reason)
                    string errorMessage = StringUtil.Format(TypesXmlStrings.ValidationException,
                        string.Empty /* TODO/FIXME snapin */,
                    ReportLogEntryHelper(errorMessage, XmlLoaderLoggerEntry.EntryType.Error, failToLoadFile: true);
            // load file into XML document
                    fileContents,
                    null); /* default maxCharacters */
                this.ReportTrace("XmlDocument loaded OK");
            catch (XmlException e)
                this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.ErrorInFile, FilePath, e.Message));
                this.ReportTrace("XmlDocument discarded");
        /// File system path for the file we are loading from.
        protected string FilePath
                return _loadingInfo.filePath;
        protected void SetDatabaseLoadingInfo(XmlFileLoadInfo info)
            _loadingInfo.fileDirectory = info.fileDirectory;
            _loadingInfo.filePath = info.filePath;
        protected void SetLoadingInfoIsFullyTrusted(bool isFullyTrusted)
            _loadingInfo.isFullyTrusted = isFullyTrusted;
        protected void SetLoadingInfoIsProductCode(bool isProductCode)
            _loadingInfo.isProductCode = isProductCode;
        private readonly DatabaseLoadingInfo _loadingInfo = new DatabaseLoadingInfo();
        protected DatabaseLoadingInfo LoadingInfo
                DatabaseLoadingInfo info = new DatabaseLoadingInfo();
                info.filePath = _loadingInfo.filePath;
                info.fileDirectory = _loadingInfo.fileDirectory;
                info.isFullyTrusted = _loadingInfo.isFullyTrusted;
                info.isProductCode = _loadingInfo.isProductCode;
        protected DisplayResourceManagerCache displayResourceManagerCache;
        internal bool VerifyStringResources { get; } = true;
        private readonly int _maxNumberOfErrors = 30;
        private int _currentErrorCount = 0;
        private readonly bool _logStackActivity = false;
        private readonly Stack<XmlLoaderStackFrame> _executionStack = new Stack<XmlLoaderStackFrame>();
        private XmlLoaderLogger _logger = new XmlLoaderLogger();

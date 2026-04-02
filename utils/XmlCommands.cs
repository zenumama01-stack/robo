    /// Implementation for the Export-Clixml command.
    [Cmdlet(VerbsData.Export, "Clixml", SupportsShouldProcess = true, DefaultParameterSetName = "ByPath", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096926")]
    public sealed class ExportClixmlCommand : PSCmdlet, IDisposable
        // If a Passthru parameter is added, the SupportsShouldProcess
        // implementation will need to be modified.
        public int Depth { get; set; }
        [Parameter(Mandatory = true, ParameterSetName = "ByLiteralPath")]
        /// Input object to be exported.
        [Parameter(ValueFromPipeline = true, Mandatory = true)]
            if (_serializer != null)
                _serializer.Serialize(InputObject);
                _xw.Flush();
        EndProcessing()
                _serializer.Done();
            _serializer.Stop();
        private XmlWriter _xw;
        /// Serializer used for serialization.
        private Serializer _serializer;
        /// FileInfo of file to clear read-only flag when operation is complete.
            Dbg.Assert(Path != null, "FileName is mandatory parameter");
            if (!ShouldProcess(Path))
            StreamWriter sw;
                this.Encoding,
                false, // default encoding
                false, // append
                this.Force,
                this.NoClobber,
                out sw,
            // create xml writer
            XmlWriterSettings xmlSettings = new();
            xmlSettings.CloseOutput = true;
            xmlSettings.Encoding = sw.Encoding;
            xmlSettings.Indent = true;
            xmlSettings.OmitXmlDeclaration = true;
            _xw = XmlWriter.Create(sw, xmlSettings);
            if (Depth == 0)
                _serializer = new Serializer(_xw);
                _serializer = new Serializer(_xw, Depth, true);
        private
        CleanUp()
                if (_xw != null)
                    _xw.Dispose();
                    _xw = null;
        Dispose()
    /// Implements Import-Clixml command.
    [Cmdlet(VerbsData.Import, "Clixml", SupportsPaging = true, DefaultParameterSetName = "ByPath", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096618")]
    public sealed class ImportClixmlCommand : PSCmdlet, IDisposable
        /// Mandatory file name to read from.
                if (_helper != null)
                    _helper.Dispose();
                    _helper = null;
        private ImportXmlHelper _helper;
                    _helper = new ImportXmlHelper(path, this, _isLiteralPath);
                    _helper.Import();
            _helper.Stop();
    /// Implementation for the convertto-xml command.
    [Cmdlet(VerbsData.ConvertTo, "Xml", SupportsShouldProcess = false,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096603", RemotingCapability = RemotingCapability.None)]
    [OutputType(typeof(XmlDocument), typeof(string))]
    public sealed class ConvertToXmlCommand : PSCmdlet, IDisposable
        [Parameter(HelpMessage = "Specifies how many levels of contained objects should be included in the XML representation")]
        /// Input Object which is written to XML format.
        [Parameter(Position = 0, ValueFromPipeline = true, Mandatory = true)]
        /// Property that sets NoTypeInformation parameter.
        [Parameter(HelpMessage = "Specifies not to include the Type information in the XML representation")]
        public SwitchParameter NoTypeInformation
                return _notypeinformation;
                _notypeinformation = value;
        private bool _notypeinformation;
        /// Property that sets As parameter.
        [ValidateSet("Stream", "String", "Document")]
        public string As { get; set; } = "Document";
            if (!As.Equals("Stream", StringComparison.OrdinalIgnoreCase))
                CreateMemoryStream();
                WriteObject(string.Create(CultureInfo.InvariantCulture, $"<?xml version=\"1.0\" encoding=\"{Encoding.UTF8.WebName}\"?>"));
                WriteObject("<Objects>");
            if (As.Equals("Stream", StringComparison.OrdinalIgnoreCase))
                _serializer?.SerializeAsStream(InputObject);
                    _serializer.DoneAsStream();
                // Loading to the XML Document
                _ms.Position = 0;
                StreamReader read = new(_ms);
                string data = read.ReadToEnd();
                WriteObject(data);
                // Cleanup
                _serializer?.Serialize(InputObject);
                WriteObject("</Objects>");
                if (As.Equals("Document", StringComparison.OrdinalIgnoreCase))
                    // this is a trusted xml doc - the cmdlet generated the doc into a private memory stream
                    XmlDocument xmldoc = new();
                    xmldoc.Load(_ms);
                    WriteObject(xmldoc);
                else if (As.Equals("String", StringComparison.OrdinalIgnoreCase))
            // Cleaning up
        #region memory
        /// XmlText writer.
        private CustomSerialization _serializer;
        /// Memory Stream used for serialization.
        private MemoryStream _ms;
        private void CreateMemoryStream()
            // Memory Stream
            _ms = new MemoryStream();
            // We use XmlTextWriter originally:
            //     _xw = new XmlTextWriter(_ms, null);
            //     _xw.Formatting = Formatting.Indented;
            // This implies the following settings:
            //  - Encoding is null -> use the default encoding 'UTF-8' when creating the writer from the stream;
            //  - XmlTextWriter closes the underlying stream / writer when 'Close/Dispose' is called on it;
            //  - Use the default indentation setting -- two space characters.
            // We configure the same settings in XmlWriterSettings when refactoring this code to use XmlWriter:
            //  - The default encoding used by XmlWriterSettings is 'UTF-8', but we call it out explicitly anyway;
            //  - Set CloseOutput to true;
            //  - Set Indent to true, and by default, IndentChars is two space characters.
            // We use XmlWriterSettings.OmitXmlDeclaration instead of XmlWriter.WriteStartDocument because the
            // xml writer created by calling XmlWriter.Create(Stream, XmlWriterSettings) will write out the xml
            // declaration even without calling WriteStartDocument().
            var xmlSettings = new XmlWriterSettings();
            xmlSettings.Encoding = Encoding.UTF8;
                // Omit xml declaration in this case because we will write out the declaration string in BeginProcess.
            _xw = XmlWriter.Create(_ms, xmlSettings);
                _serializer = new CustomSerialization(_xw, NoTypeInformation);
                _serializer = new CustomSerialization(_xw, NoTypeInformation, Depth);
        ///Cleaning up the MemoryStream.
            if (_ms != null)
                _ms.Dispose();
                _ms = null;
        #endregion memory
    /// Implements ConvertTo-CliXml command.
    [Cmdlet(VerbsData.ConvertTo, "CliXml", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2280866")]
    public sealed class ConvertToClixmlCommand : PSCmdlet
        /// Gets or sets input objects to be converted to CliXml object.
        /// Gets or sets depth of serialization.
        public int Depth { get; set; } = 2;
        private readonly List<object> _inputObjectBuffer = new();
        #endregion Private Members
            _inputObjectBuffer.Add(InputObject);
        /// End Processing.
            WriteObject(PSSerializer.Serialize(_inputObjectBuffer, Depth, enumerate: true));
    /// Implements ConvertFrom-CliXml command.
    [Cmdlet(VerbsData.ConvertFrom, "CliXml", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2280770")]
    public sealed class ConvertFromClixmlCommand : PSCmdlet
        /// Gets or sets input object which is written in CliXml format.
            WriteObject(PSSerializer.Deserialize(InputObject));
    /// Helper class to import single XML file.
    internal sealed class ImportXmlHelper : IDisposable
        /// XML file to import.
        private readonly string _path;
        private readonly bool _isLiteralPath;
        internal ImportXmlHelper(string fileName, PSCmdlet cmdlet, bool isLiteralPath)
            Dbg.Assert(fileName != null, "filename is mandatory");
            Dbg.Assert(cmdlet != null, "cmdlet is mandatory");
            _path = fileName;
            _isLiteralPath = isLiteralPath;
        internal FileStream _fs;
        /// XmlReader used to read file.
        internal XmlReader _xr;
        private static XmlReader CreateXmlReader(Stream stream)
            TextReader textReader = new StreamReader(stream);
            // skip #< CLIXML directive
            const string cliXmlDirective = "#< CLIXML";
            if (textReader.Peek() == (int)cliXmlDirective[0])
                string line = textReader.ReadLine();
                if (!line.Equals(cliXmlDirective, StringComparison.Ordinal))
                    stream.Seek(0, SeekOrigin.Begin);
            return XmlReader.Create(textReader, InternalDeserializer.XmlReaderSettingsForCliXml);
        internal void CreateFileStream()
            _fs = PathUtils.OpenFileStream(_path, _cmdlet, _isLiteralPath);
            _xr = CreateXmlReader(_fs);
        private Deserializer _deserializer;
        internal void Import()
            _deserializer = new Deserializer(_xr);
            // If total count has been requested, return a dummy object with zero confidence
            if (_cmdlet.PagingParameters.IncludeTotalCount)
                PSObject totalCount = _cmdlet.PagingParameters.NewTotalCount(0, 0);
                _cmdlet.WriteObject(totalCount);
            ulong skip = _cmdlet.PagingParameters.Skip;
            ulong first = _cmdlet.PagingParameters.First;
            // if paging is not specified then keep the old V2 behavior
            if (skip == 0 && first == ulong.MaxValue)
                while (!_deserializer.Done())
                    object result = _deserializer.Deserialize();
            // else try to flatten the output if possible
                ulong skipped = 0;
                ulong count = 0;
                while (!_deserializer.Done() && count < first)
                    if (result is PSObject psObject)
                        if (psObject.BaseObject is ICollection c)
                            foreach (object o in c)
                                if (count >= first)
                                if (skipped++ >= skip)
                                    _cmdlet.WriteObject(o);
                    else if (skipped++ >= skip)
        internal void Stop() => _deserializer?.Stop();
    #region Select-Xml
    /// This cmdlet is used to search an xml document based on the XPath Query.
    [Cmdlet(VerbsCommon.Select, "Xml", DefaultParameterSetName = "Xml", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097031")]
    [OutputType(typeof(SelectXmlInfo))]
    public class SelectXmlCommand : PSCmdlet
        /// Specifies the path which contains the xml files. The default is the current user directory.
        [Parameter(Position = 1, Mandatory = true,
                   ParameterSetName = "Path")]
        /// Specifies the literal path which contains the xml files. The default is the current user directory.
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "LiteralPath")]
        /// The following is the definition of the input parameter "XML".
        /// Specifies the xml Node.
        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, ValueFromPipeline = true,
                   ParameterSetName = "Xml")]
        [Alias("Node")]
        public System.Xml.XmlNode[] Xml { get; set; }
        /// The following is the definition of the input parameter in string format.
        /// Specifies the string format of a fully qualified xml.
        [Parameter(Mandatory = true, ValueFromPipeline = true,
                   ParameterSetName = "Content")]
        public string[] Content { get; set; }
        /// The following is the definition of the input parameter "Xpath".
        /// Specifies the String in XPath language syntax. The xml documents will be
        /// searched for the nodes/values represented by this parameter.
        public string XPath { get; set; }
        /// The following definition used to specify the NameSpace of xml.
        public Hashtable Namespace { get; set; }
        private void WriteResults(XmlNodeList foundXmlNodes, string filePath)
            Dbg.Assert(foundXmlNodes != null, "Caller should verify foundNodes != null");
            foreach (XmlNode foundXmlNode in foundXmlNodes)
                SelectXmlInfo selectXmlInfo = new();
                selectXmlInfo.Node = foundXmlNode;
                selectXmlInfo.Pattern = XPath;
                selectXmlInfo.Path = filePath;
                this.WriteObject(selectXmlInfo);
        private void ProcessXmlNode(XmlNode xmlNode, string filePath)
            Dbg.Assert(xmlNode != null, "Caller should verify xmlNode != null");
            XmlNodeList xList;
            if (Namespace != null)
                XmlNamespaceManager xmlns = AddNameSpaceTable(this.ParameterSetName, xmlNode as XmlDocument, Namespace);
                xList = xmlNode.SelectNodes(XPath, xmlns);
                xList = xmlNode.SelectNodes(XPath);
            this.WriteResults(xList, filePath);
        private void ProcessXmlFile(string filePath)
            // Cannot use ImportXMLHelper because it will throw terminating error which will
            // not be inline with Select-String
            // So doing self processing of the file.
                XmlDocument xmlDocument = InternalDeserializer.LoadUnsafeXmlDocument(
                    new FileInfo(filePath),
                    true, /* preserve whitespace, comments, etc. */
                this.ProcessXmlNode(xmlDocument, filePath);
            catch (NotSupportedException notSupportedException)
                this.WriteFileReadError(filePath, notSupportedException);
                this.WriteFileReadError(filePath, ioException);
                this.WriteFileReadError(filePath, securityException);
                this.WriteFileReadError(filePath, unauthorizedAccessException);
            catch (XmlException xmlException)
                this.WriteFileReadError(filePath, xmlException);
                this.WriteFileReadError(filePath, invalidOperationException);
        private void WriteFileReadError(string filePath, Exception exception)
                // filePath is culture invariant, exception message is to be copied verbatim
                UtilityCommonStrings.FileReadError,
                exception.Message);
            ArgumentException argumentException = new(errorMessage, exception);
            ErrorRecord errorRecord = new(argumentException, "ProcessingFile", ErrorCategory.InvalidArgument, filePath);
        private XmlNamespaceManager AddNameSpaceTable(string parametersetname, XmlDocument xDoc, Hashtable namespacetable)
            XmlNamespaceManager xmlns;
            if (parametersetname.Equals("Xml"))
                XmlNameTable xmlnt = new NameTable();
                xmlns = new XmlNamespaceManager(xmlnt);
                xmlns = new XmlNamespaceManager(xDoc.NameTable);
            foreach (DictionaryEntry row in namespacetable)
                    xmlns.AddNamespace(row.Key.ToString(), row.Value.ToString());
                catch (NullReferenceException)
                    string message = StringUtil.Format(UtilityCommonStrings.SearchXMLPrefixNullError);
                    InvalidOperationException e = new(message);
                    ErrorRecord er = new(e, "PrefixError", ErrorCategory.InvalidOperation, namespacetable);
                catch (ArgumentNullException)
            return xmlns;
            if (ParameterSetName.Equals("Xml", StringComparison.OrdinalIgnoreCase))
                foreach (XmlNode xmlNode in this.Xml)
                    ProcessXmlNode(xmlNode, string.Empty);
            else if (
                (ParameterSetName.Equals("Path", StringComparison.OrdinalIgnoreCase) ||
                (ParameterSetName.Equals("LiteralPath", StringComparison.OrdinalIgnoreCase))))
                // If any file not resolved, execution stops. this is to make consistent with select-string.
                List<string> fullresolvedPaths = new();
                foreach (string fpath in Path)
                    if (_isLiteralPath)
                        string resolvedPath = GetUnresolvedProviderPathFromPSPath(fpath);
                        fullresolvedPaths.Add(resolvedPath);
                        Collection<string> resolvedPaths = GetResolvedProviderPathFromPSPath(fpath, out provider);
                            // Cannot open File error
                            string message = StringUtil.Format(UtilityCommonStrings.FileOpenError, provider.FullName);
                            ErrorRecord er = new(e, "ProcessingFile", ErrorCategory.InvalidOperation, fpath);
                        fullresolvedPaths.AddRange(resolvedPaths);
                foreach (string file in fullresolvedPaths)
                    ProcessXmlFile(file);
            else if (ParameterSetName.Equals("Content", StringComparison.OrdinalIgnoreCase))
                foreach (string xmlstring in Content)
                    XmlDocument xmlDocument;
                        xmlDocument = (XmlDocument)LanguagePrimitives.ConvertTo(xmlstring, typeof(XmlDocument), CultureInfo.InvariantCulture);
                    catch (PSInvalidCastException invalidCastException)
                        this.WriteError(invalidCastException.ErrorRecord);
                    this.ProcessXmlNode(xmlDocument, string.Empty);
                Dbg.Assert(false, "Unrecognized parameterset");
    /// The object returned by Select-Xml representing the result of a match.
    public sealed class SelectXmlInfo
        /// If the object is InputObject, Input Stream is used.
        private const string inputStream = "InputStream";
        private const string SimpleFormat = "{0}";
        /// The XmlNode that matches search.
        public XmlNode Node { get; set; }
        /// The FileName from which the match is found.
                    _path = inputStream;
        /// The pattern used to search.
        /// Return String representation of the object.
        /// <param name="directory"></param>
        private string ToString(string directory)
            return FormatLine(GetNodeText(), displayPath);
        /// Returns the XmlNode Value or InnerXml.
        internal string GetNodeText()
            string nodeText = string.Empty;
            if (Node != null)
                if (Node.Value != null)
                    nodeText = Node.Value.Trim();
                    nodeText = Node.InnerXml.Trim();
            return nodeText;
        /// the routine would return bar\baz.c
        private string RelativePath(string directory)
            if (!relPath.Equals(inputStream))
        /// <param name="text"></param>
        /// <param name="displaypath"></param>
        private string FormatLine(string text, string displaypath)
            if (_path.Equals(inputStream))
                return StringUtil.Format(SimpleFormat, text);
                return StringUtil.Format(MatchFormat, text, displaypath);
    #endregion Select-Xml

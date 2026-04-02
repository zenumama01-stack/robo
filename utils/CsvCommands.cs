#pragma warning disable 1634, 1691 // Stops compiler from warning about unknown warnings
    #region BaseCsvWritingCommand
    /// This class implements the base for exportcsv and converttocsv commands.
    public abstract class BaseCsvWritingCommand : PSCmdlet
        #region Command Line Parameters
        /// Property that sets delimiter.
        [Parameter(Position = 1, ParameterSetName = "Delimiter")]
        public char Delimiter { get; set; }
        /// Culture switch for csv conversion
        [Parameter(ParameterSetName = "UseCulture")]
        public SwitchParameter UseCulture { get; set; }
        /// Abstract Property - Input Object which is written in Csv format.
        /// Derived as Different Attributes.In ConvertTo-CSV, This is a positional parameter. Export-CSV not a Positional behaviour.
        public abstract PSObject InputObject { get; set; }
        /// IncludeTypeInformation : The #TYPE line should be generated. Default is false.
        [Alias("ITI")]
        public SwitchParameter IncludeTypeInformation { get; set; }
        /// Gets or sets a value indicating whether to suppress the #TYPE line.
        /// This parameter is obsolete and has no effect. It is retained for backward compatibility only.
        [Parameter(DontShow = true)]
        [Alias("NTI")]
        [Obsolete("This parameter is obsolete and has no effect. The default behavior is to not include type information. Use -IncludeTypeInformation to include type information.")]
        public SwitchParameter NoTypeInformation { get; set; } = true;
        /// Gets or sets list of fields to quote in output.
        [Alias("QF")]
        public string[] QuoteFields { get; set; }
        /// Gets or sets option to use or suppress quotes in output.
        [Alias("UQ")]
        public QuoteKind UseQuotes { get; set; } = QuoteKind.Always;
        /// Gets or sets property that writes csv file with no headers.
        public SwitchParameter NoHeader { get; set; }
        #endregion Command Line Parameters
        /// Kind of output quoting.
        public enum QuoteKind
            /// Never quote output.
            Never,
            /// Always quote output.
            Always,
            /// Quote output as needed (a field contains used delimiter).
            AsNeeded
        /// Write the string to a file or pipeline.
        public virtual void WriteCsvLine(string line)
        /// BeginProcessing override.
            if (this.MyInvocation.BoundParameters.ContainsKey(nameof(QuoteFields)) && this.MyInvocation.BoundParameters.ContainsKey(nameof(UseQuotes)))
                InvalidOperationException exception = new(CsvCommandStrings.CannotSpecifyQuoteFieldsAndUseQuotes);
                ErrorRecord errorRecord = new(exception, "CannotSpecifyQuoteFieldsAndUseQuotes", ErrorCategory.InvalidData, null);
                this.ThrowTerminatingError(errorRecord);
            Delimiter = ImportExportCSVHelper.SetDelimiter(this, ParameterSetName, Delimiter, UseCulture);
    #region Export-CSV Command
    /// Implementation for the Export-Csv command.
    [Cmdlet(VerbsData.Export, "Csv", SupportsShouldProcess = true, DefaultParameterSetName = "Delimiter", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096608")]
    public sealed class ExportCsvCommand : BaseCsvWritingCommand, IDisposable
        // If a Passthru parameter is added, the ShouldProcess
        // implementation will need to be changed.
        /// Input Object for CSV Writing.
        [Parameter(ValueFromPipeline = true, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public override PSObject InputObject { get; set; }
        /// Mandatory file name to write to.
                _specifiedPath = true;
        private bool _specifiedPath = false;
        /// The literal path of the mandatory file name to write to.
                _isLiteralPath = true;
        private bool _isLiteralPath = false;
        /// Gets or sets property that sets force parameter.
        /// Gets or sets property that prevents file overwrite.
        [Alias("NoOverwrite")]
        public SwitchParameter NoClobber { get; set; }
        /// Gets or sets encoding optional flag.
        [ArgumentToEncodingTransformation]
        [ArgumentEncodingCompletions]
        public Encoding Encoding
                return _encoding;
                EncodingConversion.WarnIfObsolete(this, value);
                _encoding = value;
        private Encoding _encoding = Encoding.Default;
        /// Gets or sets property that sets append parameter.
        // true if Append=true AND the file written was not empty (or nonexistent) when the cmdlet was invoked
        private bool _isActuallyAppending;
        private bool _shouldProcess;
        private IList<string> _propertyNames;
        private IList<string> _preexistingPropertyNames;
        private ExportCsvHelper _helper;
            // Validate that they don't provide both Path and LiteralPath, but have provided at least one.
            if (!(_specifiedPath ^ _isLiteralPath))
                InvalidOperationException exception = new(CsvCommandStrings.CannotSpecifyPathAndLiteralPath);
                ErrorRecord errorRecord = new(exception, "CannotSpecifyPathAndLiteralPath", ErrorCategory.InvalidData, null);
            // Validate that Append and NoHeader are not specified together.
            if (Append && NoHeader)
                InvalidOperationException exception = new(CsvCommandStrings.CannotSpecifyAppendAndNoHeader);
                ErrorRecord errorRecord = new(exception, "CannotSpecifyBothAppendAndNoHeader", ErrorCategory.InvalidData, null);
            _shouldProcess = ShouldProcess(Path);
            if (!_shouldProcess)
            CreateFileStream();
            _helper = new ExportCsvHelper(base.Delimiter, base.UseQuotes, base.QuoteFields);
        /// Convert the current input object to Csv and write to file/WriteObject.
            if (InputObject == null || _sw == null)
            // Process first object
            if (_propertyNames == null)
                // figure out the column names (and lock-in their order)
                _propertyNames = ExportCsvHelper.BuildPropertyNames(InputObject, _propertyNames);
                if (_isActuallyAppending && _preexistingPropertyNames != null)
                    this.ReconcilePreexistingPropertyNames();
                // write headers (row1: typename + row2: column names)
                if (!_isActuallyAppending && !NoHeader.IsPresent)
                    if (IncludeTypeInformation)
                        WriteCsvLine(ExportCsvHelper.GetTypeString(InputObject));
                    WriteCsvLine(_helper.ConvertPropertyNamesCSV(_propertyNames));
            string csv = _helper.ConvertPSObjectToCSV(InputObject, _propertyNames);
            WriteCsvLine(csv);
            CleanUp();
        #region file
        /// Handle to file stream.
        private FileStream _fs;
        /// Stream writer used to write to file.
        private StreamWriter _sw = null;
        /// Handle to file whose read-only attribute should be reset when we are done.
        private FileInfo _readOnlyFileInfo = null;
        private void CreateFileStream()
            if (_path == null)
                throw new InvalidOperationException(CsvCommandStrings.FileNameIsAMandatoryParameter);
            string resolvedFilePath = PathUtils.ResolveFilePath(this.Path, this, _isLiteralPath);
            bool isCsvFileEmpty = true;
            if (this.Append && File.Exists(resolvedFilePath))
                using (StreamReader streamReader = PathUtils.OpenStreamReader(this, this.Path, Encoding, _isLiteralPath))
                    isCsvFileEmpty = streamReader.Peek() == -1;
            // If the csv file is empty then even append is treated as regular export (i.e., both header & values are added to the CSV file).
            _isActuallyAppending = this.Append && File.Exists(resolvedFilePath) && !isCsvFileEmpty;
            if (_isActuallyAppending)
                Encoding encodingObject;
                    ImportCsvHelper readingHelper = new(
                        this, this.Delimiter, null /* header */, null /* typeName */, streamReader);
                    readingHelper.ReadHeader();
                    _preexistingPropertyNames = readingHelper.Header;
                    encodingObject = streamReader.CurrentEncoding;
                PathUtils.MasterStreamOpen(
                    this.Path,
                    encodingObject,
                    defaultEncoding: false,
                    Append,
                    Force,
                    NoClobber,
                    out _fs,
                    out _sw,
                    out _readOnlyFileInfo,
                    _isLiteralPath);
                    Encoding,
        private void CleanUp()
            if (_fs != null)
                if (_sw != null)
                    _sw.Dispose();
                    _sw = null;
                _fs.Dispose();
                _fs = null;
                // reset the read-only attribute
                if (_readOnlyFileInfo != null)
                    _readOnlyFileInfo.Attributes |= FileAttributes.ReadOnly;
            _helper?.Dispose();
        private void ReconcilePreexistingPropertyNames()
            if (!_isActuallyAppending)
                throw new InvalidOperationException(CsvCommandStrings.ReconcilePreexistingPropertyNamesMethodShouldOnlyGetCalledWhenAppending);
            if (_preexistingPropertyNames == null)
                throw new InvalidOperationException(CsvCommandStrings.ReconcilePreexistingPropertyNamesMethodShouldOnlyGetCalledWhenPreexistingPropertyNamesHaveBeenReadSuccessfully);
            HashSet<string> appendedPropertyNames = new(StringComparer.OrdinalIgnoreCase);
            foreach (string appendedPropertyName in _propertyNames)
                appendedPropertyNames.Add(appendedPropertyName);
            foreach (string preexistingPropertyName in _preexistingPropertyNames)
                if (!appendedPropertyNames.Contains(preexistingPropertyName))
                    if (!Force)
                            CultureInfo.InvariantCulture, // property names and file names are culture invariant
                            CsvCommandStrings.CannotAppendCsvWithMismatchedPropertyNames,
                            preexistingPropertyName,
                            this.Path);
                        InvalidOperationException exception = new(errorMessage);
                        ErrorRecord errorRecord = new(exception, "CannotAppendCsvWithMismatchedPropertyNames", ErrorCategory.InvalidData, preexistingPropertyName);
            _propertyNames = _preexistingPropertyNames;
            _preexistingPropertyNames = null;
        /// Write the csv line to file.
        /// <param name="line">Line to write.</param>
        public override void WriteCsvLine(string line)
            // NTRAID#Windows Out Of Band Releases-915851-2005/09/13
            if (_disposed)
                throw PSTraceSource.NewObjectDisposedException("ExportCsvCommand");
            _sw.WriteLine(line);
        #endregion file
        /// Set to true when object is disposed.
        /// Public dispose method.
        #endregion IDisposable Members
    #endregion Export-CSV Command
    #region Import-CSV Command
    /// Implements Import-Csv command.
    [Cmdlet(VerbsData.Import, "Csv", DefaultParameterSetName = "DelimiterPath", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097020")]
    public sealed class ImportCsvCommand : PSCmdlet
        /// Gets or sets property that sets delimiter.
        [Parameter(Position = 1, ParameterSetName = "DelimiterPath")]
        [Parameter(Position = 1, ParameterSetName = "DelimiterLiteralPath")]
        /// Gets or sets mandatory file name to read from.
        [Parameter(Position = 0, ParameterSetName = "DelimiterPath", Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Parameter(Position = 0, ParameterSetName = "CulturePath", Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        /// Gets or sets the literal path of the mandatory file name to read from.
        [Parameter(ParameterSetName = "DelimiterLiteralPath", Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = "CultureLiteralPath", Mandatory = true, ValueFromPipelineByPropertyName = true)]
        /// Gets or sets property that sets UseCulture parameter.
        [Parameter(ParameterSetName = "CulturePath", Mandatory = true)]
        [Parameter(ParameterSetName = "CultureLiteralPath", Mandatory = true)]
        /// Gets or sets header property to customize the names.
        [Parameter(Mandatory = false)]
        public string[] Header { get; set; }
        /// Avoid writing out duplicate warning messages when there are one or more unspecified names.
        private bool _alreadyWarnedUnspecifiedNames = false;
        #region Override Methods
        /// ProcessRecord overload.
            if (_paths != null)
                    using (StreamReader streamReader = PathUtils.OpenStreamReader(this, path, this.Encoding, _isLiteralPath))
                        ImportCsvHelper helper = new(this, Delimiter, Header, null /* typeName */, streamReader);
                            helper.Import(ref _alreadyWarnedUnspecifiedNames);
                        catch (ExtendedTypeSystemException exception)
                            ErrorRecord errorRecord = new(exception, "AlreadyPresentPSMemberInfoInternalCollectionAdd", ErrorCategory.NotSpecified, null);
    #endregion Override Methods
    #endregion Import-CSV Command
    #region ConvertTo-CSV Command
    /// Implements ConvertTo-Csv command.
    [Cmdlet(VerbsData.ConvertTo, "Csv", DefaultParameterSetName = "Delimiter",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096832", RemotingCapability = RemotingCapability.None)]
    public sealed class ConvertToCsvCommand : BaseCsvWritingCommand
        /// Overrides Base InputObject.
        [Parameter(ValueFromPipeline = true, Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        /// Stores Property Names.
        /// Convert the current input object to Csv and write to stream/WriteObject.
                if (!NoHeader.IsPresent)
                    // Write property information
                    string properties = _helper.ConvertPropertyNamesCSV(_propertyNames);
                    if (!properties.Equals(string.Empty))
                        WriteCsvLine(properties);
            // Write to the output stream
            if (csv != string.Empty)
        #region CSV conversion
        /// Write the line to output.
            WriteObject(line);
        #endregion CSV conversion
    #endregion ConvertTo-CSV Command
    #region ConvertFrom-CSV Command
    /// Implements ConvertFrom-Csv command.
    [Cmdlet(VerbsData.ConvertFrom, "Csv", DefaultParameterSetName = "Delimiter",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096830", RemotingCapability = RemotingCapability.None)]
    public sealed class ConvertFromCsvCommand : PSCmdlet
        [Parameter(ParameterSetName = "UseCulture", Mandatory = true)]
        /// Gets or sets input object which is written in Csv format.
        public PSObject[] InputObject { get; set; }
            foreach (PSObject inputObject in InputObject)
                using (MemoryStream memoryStream = new(Encoding.Unicode.GetBytes(inputObject.ToString())))
                using (StreamReader streamReader = new(memoryStream, System.Text.Encoding.Unicode))
                    ImportCsvHelper helper = new(this, Delimiter, Header, _typeName, streamReader);
                    if ((Header == null) && (helper.Header != null))
                        Header = helper.Header.ToArray();
                    if ((_typeName == null) && (helper.TypeName != null))
                        _typeName = helper.TypeName;
    #endregion ConvertFrom-CSV Command
    #region ExportHelperConversion
    /// Helper class for Export-Csv and ConvertTo-Csv.
    internal sealed class ExportCsvHelper : IDisposable
        private readonly char _delimiter;
        private readonly BaseCsvWritingCommand.QuoteKind _quoteKind;
        private readonly HashSet<string> _quoteFields;
        private readonly StringBuilder _outputString;
        /// Initializes a new instance of the <see cref="ExportCsvHelper"/> class.
        /// <param name="delimiter">Delimiter char.</param>
        /// <param name="quoteKind">Kind of quoting.</param>
        /// <param name="quoteFields">List of fields to quote.</param>
        internal ExportCsvHelper(char delimiter, BaseCsvWritingCommand.QuoteKind quoteKind, string[] quoteFields)
            _delimiter = delimiter;
            _quoteKind = quoteKind;
            _quoteFields = quoteFields == null ? null : new HashSet<string>(quoteFields, StringComparer.OrdinalIgnoreCase);
            _outputString = new StringBuilder(128);
        // Name of properties to be written in CSV format
        /// Get the name of properties from source PSObject and add them to _propertyNames.
        internal static IList<string> BuildPropertyNames(PSObject source, IList<string> propertyNames)
            if (propertyNames != null)
                throw new InvalidOperationException(CsvCommandStrings.BuildPropertyNamesMethodShouldBeCalledOnlyOncePerCmdletInstance);
            propertyNames = new Collection<string>();
            if (source.BaseObject is IDictionary dictionary)
                foreach (var key in dictionary.Keys)
                    propertyNames.Add(LanguagePrimitives.ConvertTo<string>(key));
                // Add additional extended members added to the dictionary object, if any
                var propertiesToSearch = new PSMemberInfoIntegratingCollection<PSPropertyInfo>(
                    source,
                    PSObject.GetPropertyCollection(PSMemberViewTypes.Extended));
                foreach (var prop in propertiesToSearch)
                    propertyNames.Add(prop.Name);
                // serialize only Extended and Adapted properties.
                PSMemberInfoCollection<PSPropertyInfo> srcPropertiesToSearch =
                    new PSMemberInfoIntegratingCollection<PSPropertyInfo>(
                        PSObject.GetPropertyCollection(PSMemberViewTypes.Extended | PSMemberViewTypes.Adapted));
                foreach (PSPropertyInfo prop in srcPropertiesToSearch)
            return propertyNames;
        /// Converts PropertyNames in to a CSV string.
        /// <returns>Converted string.</returns>
        internal string ConvertPropertyNamesCSV(IList<string> propertyNames)
            ArgumentNullException.ThrowIfNull(propertyNames);
            _outputString.Clear();
            foreach (string propertyName in propertyNames)
                    _outputString.Append(_delimiter);
                if (_quoteFields != null)
                    if (_quoteFields.TryGetValue(propertyName, out _))
                        AppendStringWithEscapeAlways(_outputString, propertyName);
                        _outputString.Append(propertyName);
                    switch (_quoteKind)
                        case BaseCsvWritingCommand.QuoteKind.Always:
                        case BaseCsvWritingCommand.QuoteKind.AsNeeded:
                            if (propertyName.AsSpan().IndexOfAny(_delimiter, '\n', '"') != -1)
                        case BaseCsvWritingCommand.QuoteKind.Never:
            return _outputString.ToString();
        /// Convert PSObject to CSV string.
        /// <param name="mshObject">PSObject to convert.</param>
        /// <param name="propertyNames">Property names.</param>
        internal string ConvertPSObjectToCSV(PSObject mshObject, IList<string> propertyNames)
                string value = null;
                if (mshObject.BaseObject is IDictionary dictionary)
                    if (dictionary.Contains(propertyName))
                        value = dictionary[propertyName]?.ToString();
                    else if (mshObject.Properties[propertyName] is PSPropertyInfo property)
                        value = GetToStringValueForProperty(property);
                // If value is null, assume property is not present and skip it.
                            AppendStringWithEscapeAlways(_outputString, value);
                            _outputString.Append(value);
                                if (value != null && value.AsSpan().IndexOfAny(_delimiter, '\n', '"') != -1)
                                Diagnostics.Assert(false, "BaseCsvWritingCommand.QuoteKind has new item.");
        /// Get value from property object.
        /// <param name="property"> Property to convert.</param>
        /// <returns>ToString() value.</returns>
        internal static string GetToStringValueForProperty(PSPropertyInfo property)
            ArgumentNullException.ThrowIfNull(property);
                object temp = property.Value;
                    value = temp.ToString();
                // If we cannot read some value, treat it as null.
        /// Prepares string for writing type information.
        /// <param name="source">PSObject whose type to determine.</param>
        /// <returns>String with type information.</returns>
        internal static string GetTypeString(PSObject source)
            string type = null;
            // get type of source
            Collection<string> tnh = source.TypeNames;
            if (tnh == null || tnh.Count == 0)
                type = "#TYPE";
                if (tnh[0] == null)
                    throw new InvalidOperationException(CsvCommandStrings.TypeHierarchyShouldNotHaveNullValues);
                string temp = tnh[0];
                // If type starts with CSV: remove it. This would happen when you export
                // an imported object. import-csv adds CSV. prefix to the type.
                if (temp.StartsWith(ImportExportCSVHelper.CSVTypePrefix, StringComparison.OrdinalIgnoreCase))
                    temp = temp.Substring(4);
                type = string.Create(System.Globalization.CultureInfo.InvariantCulture, $"#TYPE {temp}");
            return type;
        /// Escapes the " in string if necessary.
        /// Encloses the string in double quotes if necessary.
        internal static void AppendStringWithEscapeAlways(StringBuilder dest, string source)
            if (source == null)
            // Adding Double quote to all strings
            dest.Append('"');
            for (int i = 0; i < source.Length; i++)
                char c = source[i];
                // Double quote in the string is escaped with double quote
                dest.Append(c);
    #endregion ExportHelperConversion
    #region ImportHelperConversion
    /// Helper class to import single CSV file.
    internal sealed class ImportCsvHelper
        /// Reference to cmdlet which is using this helper class.
        private readonly PSCmdlet _cmdlet;
        /// CSV delimiter (default is the "comma" / "," character).
        /// Use "UnspecifiedName" when the name is null or empty.
        private const string UnspecifiedName = "H";
        private bool _alreadyWarnedUnspecifiedName = false;
        /// Gets reference to header values.
        internal IList<string> Header { get; private set; }
        /// Gets ETS type name from the first line / comment in the CSV.
        internal string TypeName { get; private set; }
        /// Reader of the csv content.
        private readonly StreamReader _sr;
        // Initial sizes of the value list and the line stringbuilder.
        // Set to reasonable initial sizes. They may grow beyond these,
        // but this will prevent a few reallocations.
        private const int ValueCountGuestimate = 16;
        private const int LineLengthGuestimate = 256;
        internal ImportCsvHelper(PSCmdlet cmdlet, char delimiter, IList<string> header, string typeName, StreamReader streamReader)
            ArgumentNullException.ThrowIfNull(cmdlet);
            ArgumentNullException.ThrowIfNull(streamReader);
            _cmdlet = cmdlet;
            Header = header;
            TypeName = typeName;
            _sr = streamReader;
        #region reading helpers
        /// This is set to true when end of file is reached.
        private bool EOF => _sr.EndOfStream;
        private char ReadChar()
            if (EOF)
                throw new InvalidOperationException(CsvCommandStrings.EOFIsReached);
            int i = _sr.Read();
            return (char)i;
        /// Peeks the next character in the stream and returns true if it is same as passed in character.
        /// <param name="c"></param>
        private bool PeekNextChar(char c)
            int i = _sr.Peek();
            if (i == -1)
            return c == (char)i;
        /// Reads a line from file. This consumes the end of line.
        /// Only use it when end of line chars are not important.
        /// <returns>Line from file.</returns>
        private string ReadLine() => _sr.ReadLine();
        #endregion reading helpers
        internal void ReadHeader()
            // Read #Type record if available
            if ((TypeName == null) && (!this.EOF))
                TypeName = ReadTypeInformation();
            var values = new List<string>(ValueCountGuestimate);
            var builder = new StringBuilder(LineLengthGuestimate);
            while ((Header == null) && (!this.EOF))
                ParseNextRecord(values, builder);
                // Trim all trailing blankspaces and delimiters ( single/multiple ).
                // If there is only one element in the row and if its a blankspace we dont trim it.
                // A trailing delimiter is represented as a blankspace while being added to result collection
                // which is getting trimmed along with blankspaces supplied through the CSV in the below loop.
                while (values.Count > 1 && values[values.Count - 1].Equals(string.Empty))
                    values.RemoveAt(values.Count - 1);
                // File starts with '#' and contains '#Fields:' is W3C Extended Log File Format
                if (values.Count != 0 && values[0].StartsWith("#Fields: "))
                    values[0] = values[0].Substring(9);
                    Header = values;
                else if (values.Count != 0 && values[0].StartsWith('#'))
                    // Skip all lines starting with '#'
                    // This is not W3C Extended Log File Format
                    // By default first line is Header
            if (Header != null && Header.Count > 0)
                ValidatePropertyNames(Header);
        internal void Import(ref bool alreadyWriteOutWarning)
            _alreadyWarnedUnspecifiedName = alreadyWriteOutWarning;
            ReadHeader();
            var prevalidated = false;
                if (values.Count == 0)
                if (values.Count == 1 && string.IsNullOrEmpty(values[0]))
                    // skip the blank lines
                PSObject result = BuildMshobject(TypeName, Header, values, _delimiter, prevalidated);
                prevalidated = true;
                _cmdlet.WriteObject(result);
            alreadyWriteOutWarning = _alreadyWarnedUnspecifiedName;
        /// Validate the names of properties.
        /// <param name="names"></param>
        private static void ValidatePropertyNames(IList<string> names)
            if (names != null)
                if (names.Count == 0)
                    // If there are no names, it is an error
                    HashSet<string> headers = new(StringComparer.OrdinalIgnoreCase);
                    foreach (string currentHeader in names)
                        if (!string.IsNullOrEmpty(currentHeader))
                            if (!headers.Add(currentHeader))
                                // throw a terminating error as there are duplicate headers in the input.
                                string memberAlreadyPresentMsg =
                                        ExtendedTypeSystem.MemberAlreadyPresent,
                                        currentHeader);
                                ExtendedTypeSystemException exception = new(memberAlreadyPresentMsg);
        /// Read the type information, if present.
        /// <returns>Type string if present else null.</returns>
        private string ReadTypeInformation()
            if (PeekNextChar('#'))
                string temp = ReadLine();
                if (temp.StartsWith("#Type", StringComparison.OrdinalIgnoreCase))
                    type = temp.Substring(5);
                    type = type.Trim();
                    if (type.Length == 0)
                        type = null;
        /// Reads the next record from the file and returns parsed collection of string.
        /// Parsed collection of strings.
        private void ParseNextRecord(List<string> result, StringBuilder current)
            result.Clear();
            // current string
            current.Clear();
            bool seenBeginQuote = false;
            while (!EOF)
                // Read the next character
                char ch = ReadChar();
                if (ch == _delimiter)
                    if (seenBeginQuote)
                        // Delimiter inside double quotes is part of string.
                        // Ex:
                        // "foo, bar"
                        // is parsed as
                        // ->foo, bar<-
                        current.Append(ch);
                        // Delimiter outside quotes is end of current word.
                        result.Add(current.ToString());
                        current.Remove(0, current.Length);
                else if (ch == '"')
                        if (PeekNextChar('"'))
                            // "" inside double quote are single quote
                            // ex: "foo""bar"
                            // is read as
                            // ->foo"bar<-
                            // PeekNextChar only peeks. Read the next char.
                            ReadChar();
                            current.Append('"');
                            // We have seen a matching end quote.
                            seenBeginQuote = false;
                            // Read
                            // everything till we hit next delimiter.
                            // In correct CSV,1) end quote is followed by delimiter
                            // 2)end quote is followed some whitespaces and
                            // then delimiter.
                            // We eat the whitespaces seen after the ending quote.
                            // However if there are other characters, we add all of them
                            // to string.
                            // Ex: ->"foo bar"<- is read as ->foo bar<-
                            // ->"foo bar"  <- is read as ->foo bar<-
                            // ->"foo bar" ab <- is read as ->"foo bar" ab <-
                            bool endofRecord = false;
                            ReadTillNextDelimiter(current, ref endofRecord, true);
                            if (endofRecord)
                    else if (current.Length == 0)
                        // We are at the beginning of a new word.
                        // This quote is the first quote.
                        seenBeginQuote = true;
                        // We are seeing a quote after the start of
                        // the word. This is error, however we will be
                        // lenient here and do what excel does:
                        // Ex: foo "ba,r"
                        // In above example word read is ->foo "ba<-
                        // Basically we read till next delimiter
                        bool endOfRecord = false;
                        ReadTillNextDelimiter(current, ref endOfRecord, false);
                        if (endOfRecord)
                else if (ch == ' ' || ch == '\t')
                        // Spaces in side quote are valid
                        // ignore leading spaces
                        // We are not in quote and we are not at the
                        // beginning of a word. We should not be seeing
                        // spaces here. This is an error condition, however
                        // we will be lenient here and do what excel does,
                        // that is read till next delimiter.
                        // Ex: ->foo <- is read as ->foo<-
                        // Ex: ->foo bar<- is read as ->foo bar<-
                        // Ex: ->foo bar <- is read as ->foo bar <-
                        // Ex: ->foo bar "er,ror"<- is read as ->foo bar "er<-
                        ReadTillNextDelimiter(current, ref endOfRecord, true);
                else if (IsNewLine(ch, out string newLine))
                        // newline inside quote are valid
                        current.Append(newLine);
                        // New line outside quote is end of word and end of record
            if (current.Length != 0)
        // If we detect a newline we return it as a string "\r", "\n" or "\r\n"
        private bool IsNewLine(char ch, out string newLine)
            newLine = string.Empty;
            if (ch == '\r')
                if (PeekNextChar('\n'))
                    newLine = "\r\n";
                    newLine = "\r";
            else if (ch == '\n')
                newLine = "\n";
            return newLine != string.Empty;
        /// This function reads the characters till next delimiter and adds them to current.
        /// <param name="current"></param>
        /// <param name="endOfRecord">
        /// This is true if end of record is reached
        /// when delimiter is hit. This would be true if delimiter is NewLine.
        /// <param name="eatTrailingBlanks">
        /// If this is true, eat the trailing blanks. Note:if there are non
        /// whitespace characters present, then trailing blanks are not consumed.
        private void ReadTillNextDelimiter(StringBuilder current, ref bool endOfRecord, bool eatTrailingBlanks)
            StringBuilder temp = new();
            // Did we see any non-whitespace character
            bool nonWhiteSpace = false;
                    endOfRecord = true;
                    temp.Append(ch);
                    if (ch != ' ' && ch != '\t')
                        nonWhiteSpace = true;
            if (eatTrailingBlanks && !nonWhiteSpace)
                string s = temp.ToString();
                s = s.Trim();
                current.Append(s);
                current.Append(temp);
        private PSObject BuildMshobject(string type, IList<string> names, List<string> values, char delimiter, bool preValidated = false)
            PSObject result = new(names.Count);
            char delimiterlocal = delimiter;
            int unspecifiedNameIndex = 1;
            for (int i = 0; i <= names.Count - 1; i++)
                string name = names[i];
                // if name is null and delimiter is '"', use a default property name 'UnspecifiedName'
                if (name.Length == 0 && delimiterlocal == '"')
                    name = UnspecifiedName + unspecifiedNameIndex;
                    unspecifiedNameIndex++;
                // if name is null and delimiter is not '"', use a default property name 'UnspecifiedName'
                // If no value was present in CSV file, we write null.
                if (i < values.Count)
                    value = values[i];
                result.Properties.Add(new PSNoteProperty(name, value), preValidated);
            if (!_alreadyWarnedUnspecifiedName && unspecifiedNameIndex != 1)
                _cmdlet.WriteWarning(CsvCommandStrings.UseDefaultNameForUnspecifiedHeader);
                _alreadyWarnedUnspecifiedName = true;
            if (!string.IsNullOrEmpty(type))
                result.TypeNames.Clear();
                result.TypeNames.Add(type);
                result.TypeNames.Add(ImportExportCSVHelper.CSVTypePrefix + type);
    #endregion ImportHelperConversion
    #region ExportImport Helper
    /// Helper class for CSV conversion.
    internal static class ImportExportCSVHelper
        internal const char CSVDelimiter = ',';
        internal const string CSVTypePrefix = "CSV:";
        internal static char SetDelimiter(PSCmdlet cmdlet, string parameterSetName, char explicitDelimiter, bool useCulture)
            char delimiter = explicitDelimiter;
                case "Delimiter":
                case "DelimiterPath":
                case "DelimiterLiteralPath":
                    // if delimiter is not given, it should take , as value
                    if (explicitDelimiter == '\0')
                        delimiter = ImportExportCSVHelper.CSVDelimiter;
                case "UseCulture":
                case "CulturePath":
                case "CultureLiteralPath":
                    if (useCulture)
                        // ListSeparator is apparently always a character even though the property returns a string, checked via:
                        // [CultureInfo]::GetCultures("AllCultures") | % { ([CultureInfo]($_.Name)).TextInfo.ListSeparator } | ? Length -ne 1
                        delimiter = CultureInfo.CurrentCulture.TextInfo.ListSeparator[0];
            return delimiter;
    #endregion ExportImport Helper

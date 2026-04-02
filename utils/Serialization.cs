    /// Wraps Hitesh's xml serializer in such a way that it will select the proper serializer based on the data
    /// format.
    internal class Serialization
        /// Describes the format of the data streamed between minishells, e.g. the allowed arguments to the minishell
        /// -outputformat and -inputformat command line parameters.
        internal enum DataFormat
            /// Text format -- i.e. stream text just as out-default would display it.
            Text = 0,
            /// XML-serialized format.
            XML = 1,
            /// Indicates that the data should be discarded instead of processed.
            None = 2
        protected
        Serialization(DataFormat dataFormat, string streamName)
            Dbg.Assert(!string.IsNullOrEmpty(streamName), "stream needs a name");
            format = dataFormat;
            this.streamName = streamName;
        protected static string XmlCliTag = "#< CLIXML";
        protected string streamName;
        protected DataFormat format;
    class WrappedSerializer : Serialization
        WrappedSerializer(DataFormat dataFormat, string streamName, TextWriter output)
            : base(dataFormat, streamName)
            Dbg.Assert(output != null, "output should have a value");
            textWriter = output;
                case DataFormat.XML:
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.CheckCharacters = false;
                    settings.OmitXmlDeclaration = true;
                    _xmlWriter = XmlWriter.Create(textWriter, settings);
                    _xmlSerializer = new Serializer(_xmlWriter);
                case DataFormat.Text:
                    // do nothing; we'll just write to the TextWriter
                    // or discard it.
        Serialize(object o)
            Serialize(o, this.streamName);
        Serialize(object o, string streamName)
                case DataFormat.None:
                        textWriter.WriteLine(Serialization.XmlCliTag);
                    _xmlSerializer.Serialize(o, streamName);
                    textWriter.Write(o.ToString());
        End()
                    _xmlSerializer.Done();
                    _xmlSerializer = null;
        internal TextWriter textWriter;
        private readonly XmlWriter _xmlWriter;
        private Serializer _xmlSerializer;
    class WrappedDeserializer : Serialization
        WrappedDeserializer(DataFormat dataFormat, string streamName, TextReader input)
            Dbg.Assert(input != null, "input should have a value");
            // If the data format is none - do nothing...
            if (dataFormat == DataFormat.None)
            textReader = input;
            _firstLine = textReader.ReadLine();
            if (string.Equals(_firstLine, Serialization.XmlCliTag, StringComparison.OrdinalIgnoreCase))
                // format should be XML
                dataFormat = DataFormat.XML;
                    _xmlReader = XmlReader.Create(textReader, new XmlReaderSettings { XmlResolver = null });
                    _xmlDeserializer = new Deserializer(_xmlReader);
                    // do nothing; we'll just read from the TextReader
        Deserialize()
            object o;
                    _atEnd = true;
                    o = _xmlDeserializer.Deserialize(out _);
                    if (_atEnd)
                    if (_firstLine != null)
                        o = _firstLine;
                        _firstLine = null;
                        o = textReader.ReadLine();
            return o;
        AtEnd
                        result = _xmlDeserializer.Done();
                        result = _atEnd;
        internal TextReader textReader;
        private readonly XmlReader _xmlReader;
        private readonly Deserializer _xmlDeserializer;
        private string _firstLine;
        private bool _atEnd;
    internal enum SerializationOptions
        UseDepthFromTypes = 1,
        NoRootElement = 2,
        NoNamespace = 4,
        NoObjectRefIds = 8,
        PreserveSerializationSettingOfOriginal = 16,
        RemotingOptions = UseDepthFromTypes | NoRootElement | NoNamespace | PreserveSerializationSettingOfOriginal,
    internal class SerializationContext
        private const int DefaultSerializationDepth = 2;
        internal SerializationContext()
            : this(DefaultSerializationDepth, true)
        internal SerializationContext(int depth, bool useDepthFromTypes)
                    depth,
                    (useDepthFromTypes ? SerializationOptions.UseDepthFromTypes : SerializationOptions.None) |
                       SerializationOptions.PreserveSerializationSettingOfOriginal,
                    null)
        internal SerializationContext(int depth, SerializationOptions options, PSRemotingCryptoHelper cryptoHelper)
                throw PSTraceSource.NewArgumentException("writer", Serialization.DepthOfOneRequired);
            this.depth = depth;
            this.options = options;
            this.cryptoHelper = cryptoHelper;
        internal readonly int depth;
        internal readonly SerializationOptions options;
        internal readonly PSRemotingCryptoHelper cryptoHelper;
        internal readonly CimClassSerializationCache<CimClassSerializationId> cimClassSerializationIdCache = new CimClassSerializationCache<CimClassSerializationId>();
    /// This class provides public functionality for serializing a PSObject.
    public static class PSSerializer
        /// Serializes an object into PowerShell CliXml.
        /// <param name="source">The input object to serialize. Serializes to a default depth of 1.</param>
        /// <returns>The serialized object, as CliXml.</returns>
        public static string Serialize(object source)
            return Serialize(source, s_mshDefaultSerializationDepth);
        /// <param name="source">The input object to serialize.</param>
        /// <param name="depth">The depth of the members to serialize.</param>
        public static string Serialize(object source, int depth)
            // Create an xml writer
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Encoding = System.Text.Encoding.Unicode;
            XmlWriter xw = XmlWriter.Create(sb, xmlSettings);
            // Serialize the objects
            Serializer serializer = new Serializer(xw, depth, true);
            serializer.Serialize(source);
            serializer = null;
            // Return the output
        /// Serializes list of objects into PowerShell CliXml.
        /// <param name="source">The input objects to serialize.</param>
        /// <param name="enumerate">Enumerates input objects and serializes one at a time.</param>
        internal static string Serialize(IList<object> source, int depth, bool enumerate)
            XmlWriterSettings xmlSettings = new()
                CloseOutput = true,
                Encoding = Encoding.Unicode,
            Serializer serializer = new(xw, depth, useDepthFromTypes: true);
                foreach (object item in source)
                    serializer.Serialize(item);
        /// Deserializes PowerShell CliXml into an object.
        /// <param name="source">The CliXml the represents the object to deserialize.</param>
        /// <returns>An object that represents the serialized content.</returns>
        public static object Deserialize(string source)
            object[] results = DeserializeAsList(source);
            // Return the results
            if (results.Length == 0)
            else if (results.Length == 1)
                return results[0];
        /// Deserializes PowerShell CliXml into a list of objects.
        /// <returns>An object array represents the serialized content.</returns>
        public static object[] DeserializeAsList(string source)
            // Create the text reader to hold the content
            TextReader textReader = new StringReader(source);
            XmlReader xmlReader = XmlReader.Create(textReader, InternalDeserializer.XmlReaderSettingsForCliXml);
            // Deserialize the content
            Deserializer deserializer = new Deserializer(xmlReader);
            while (!deserializer.Done())
                object result = deserializer.Deserialize();
            return results.ToArray();
        private static readonly int s_mshDefaultSerializationDepth = 1;
    internal class Serializer
        private readonly InternalSerializer _serializer;
        /// Creates a Serializer using default serialization context.
        /// <param name="writer">Writer to be used for serialization.</param>
        internal Serializer(XmlWriter writer)
            : this(writer, new SerializationContext())
        /// Creates a Serializer using specified serialization context.
        /// <param name="depth">Depth of serialization.</param>
        /// <param name="useDepthFromTypes">
        /// if <see langword="true"/> then types.ps1xml can override depth
        /// for a particular types (using SerializationDepth property)
        internal Serializer(XmlWriter writer, int depth, bool useDepthFromTypes)
            : this(writer, new SerializationContext(depth, useDepthFromTypes))
        /// <param name="context">Serialization context.</param>
        internal Serializer(XmlWriter writer, SerializationContext context)
                throw PSTraceSource.NewArgumentException(nameof(context));
            _serializer = new InternalSerializer(writer, context);
            _serializer.Start();
        #region public methods / properties
        /// Used by Remoting infrastructure. This TypeTable instance
        /// will be used by Serializer if ExecutionContext is not
        /// available (to get the ExecutionContext's TypeTable)
            get { return _serializer.TypeTable; }
            set { _serializer.TypeTable = value; }
        /// Serializes the object.
        /// <param name="source">Object to be serialized.</param>
        /// Please note that this method shouldn't throw any exceptions.
        /// If it throws - please open a bug.
            Serialize(source, null);
        /// object to be serialized
        /// <param name="streamName">
        /// Stream to which this object belong. Ex: Output, Error etc.
        internal void Serialize(object source, string streamName)
            _serializer.WriteOneTopLevelObject(source, streamName);
            _serializer.End();
    internal enum DeserializationOptions
        NoRootElement = 256,            // I start at 256 to try not to overlap
        NoNamespace = 512,              // with SerializationOptions and to catch bugs early
        DeserializeScriptBlocks = 1024,
        RemotingOptions = NoRootElement | NoNamespace,
    internal class DeserializationContext
        internal DeserializationContext()
            : this(DeserializationOptions.None, null)
        internal DeserializationContext(DeserializationOptions options, PSRemotingCryptoHelper cryptoHelper)
        /// Limits the total data processed by the deserialization context. Deserialization context
        /// is used by PriorityReceivedDataCollection (remoting) to process incoming data from the
        /// remote end. A value of Null means that the max memory is unlimited.
        internal int? MaximumAllowedMemory { get; set; }
        /// Logs that memory used by deserialized objects is not related to the size of input xml.
        /// Used mainly to account for memory usage of cloned TypeNames when calculating memory quota usage.
        /// <param name="amountOfExtraMemory"></param>
        internal void LogExtraMemoryUsage(int amountOfExtraMemory)
            if (amountOfExtraMemory < 0)
            if (MaximumAllowedMemory.HasValue)
                if (amountOfExtraMemory > (MaximumAllowedMemory.Value - _totalDataProcessedSoFar))
                    string message = StringUtil.Format(Serialization.DeserializationMemoryQuota, ((double)MaximumAllowedMemory.Value) / (1 << 20),
                                                       ConfigurationDataFromXML.MAXRCVDOBJSIZETOKEN_CamelCase,
                                                       ConfigurationDataFromXML.MAXRCVDCMDSIZETOKEN_CamelCase);
                _totalDataProcessedSoFar += amountOfExtraMemory;
        private int _totalDataProcessedSoFar;
        internal readonly DeserializationOptions options;
        internal static readonly int MaxItemsInCimClassCache = 100;
        internal readonly CimClassDeserializationCache<CimClassSerializationId> cimClassSerializationIdCache = new CimClassDeserializationCache<CimClassSerializationId>();
    internal class CimClassDeserializationCache<TKey>
        private readonly Dictionary<TKey, CimClass> _cimClassIdToClass = new Dictionary<TKey, CimClass>();
        internal void AddCimClassToCache(TKey key, CimClass cimClass)
            if (_cimClassIdToClass.Count >= DeserializationContext.MaxItemsInCimClassCache)
                _cimClassIdToClass.Clear();
            _cimClassIdToClass.Add(key, cimClass);
            /* PRINTF DEBUG
            Console.WriteLine("Contents of deserialization cache (after a call to AddCimClassToCache ({0})):", key);
            Console.WriteLine("  Count = {0}", this._cimClassIdToClass.Count);
            foreach (var t in this._cimClassIdToClass.Keys)
                Console.WriteLine("  {0}", t);
        internal CimClass GetCimClassFromCache(TKey key)
            CimClass cimClass;
            if (_cimClassIdToClass.TryGetValue(key, out cimClass))
                Console.WriteLine("GetCimClassFromCache - class found: {0}", key);
            Console.WriteLine("GetCimClassFromCache - class NOT found: {0}", key);
    internal class CimClassSerializationCache<TKey>
        private readonly HashSet<TKey> _cimClassesHeldByDeserializer = new HashSet<TKey>(EqualityComparer<TKey>.Default);
        internal bool DoesDeserializerAlreadyHaveCimClass(TKey key)
            return _cimClassesHeldByDeserializer.Contains(key);
        internal void AddClassToCache(TKey key)
            Dbg.Assert(!_cimClassesHeldByDeserializer.Contains(key), "This method should not be called for classes already in the cache");
            if (_cimClassesHeldByDeserializer.Count >= DeserializationContext.MaxItemsInCimClassCache)
                _cimClassesHeldByDeserializer.Clear();
            _cimClassesHeldByDeserializer.Add(key);
            Console.WriteLine("Contents of serialization cache (after adding {0}):", key);
            Console.WriteLine("  Count = {0}", this._cimClassesHeldByDeserializer.Count);
            foreach (var t in _cimClassesHeldByDeserializer)
    internal class CimClassSerializationId : Tuple<string, string, string, int>
        public CimClassSerializationId(string className, string namespaceName, string computerName, int hashCode)
            : base(className, namespaceName, computerName, hashCode)
        public string ClassName { get { return this.Item1; } }
        public string NamespaceName { get { return this.Item2; } }
        public string ComputerName { get { return this.Item3; } }
        public int ClassHashCode { get { return this.Item4; } }
    /// This class provides functionality for deserializing a PSObject.
    internal class Deserializer
        private readonly InternalDeserializer _deserializer;
        private readonly DeserializationContext _context;
        /// Creates a Deserializer using default deserialization context.
        /// <param name="reader">Reader to be used for deserialization.</param>
        /// <exception cref="XmlException">
        /// Thrown when the xml is in an incorrect format
        internal Deserializer(XmlReader reader)
            : this(reader, new DeserializationContext())
        /// Creates a Deserializer using specified serialization context.
        internal Deserializer(XmlReader reader, DeserializationContext context)
            if (reader == null)
                throw PSTraceSource.NewArgumentNullException(nameof(reader));
            _reader = reader;
            _deserializer = new InternalDeserializer(_reader, _context);
            catch (XmlException exception)
                ReportExceptionForETW(exception);
        #region public method / properties
        private static void ReportExceptionForETW(XmlException exception)
            PSEtwLog.LogAnalyticError(
                PSEventId.Serializer_XmlExceptionWhenDeserializing, PSOpcode.Exception, PSTask.Serialization,
                PSKeyword.Serializer | PSKeyword.UseAlwaysAnalytic,
                exception.LineNumber, exception.LinePosition, exception.ToString());
        private bool _done = false;
        /// will be used by Deserializer if ExecutionContext is not
            get { return _deserializer.TypeTable; }
            set { _deserializer.TypeTable = value; }
        /// Read the root element tag and set the cursor to start tag of
        /// first object.
            Dbg.Assert(_reader.ReadState == ReadState.Initial, "When deserialization starts we should have XmlReader.ReadState == Initial");
            Dbg.Assert(_reader.NodeType == XmlNodeType.None, "When deserialization starts we should have XmlReader.NodeType == None");
            // If version is not provided, we assume it is the default
            string version = InternalSerializer.DefaultVersion;
            if ((_context.options & DeserializationOptions.NoRootElement) == DeserializationOptions.NoRootElement)
                _done = _reader.EOF;
                // Make sure the reader is positioned on the root ( <Objs> ) element (not on XmlDeclaration for example)
                Dbg.Assert(_reader.EOF || (_reader.NodeType == XmlNodeType.Element), "When deserialization starts reading we should have XmlReader.NodeType == Element");
                // Read version attribute and validate it.
                string versionAttribute = _reader.GetAttribute(SerializationStrings.VersionAttribute);
                if (versionAttribute != null)
                    version = versionAttribute;
                // If the root element tag is empty, there are no objects to read.
                if (!_deserializer.ReadStartElementAndHandleEmpty(SerializationStrings.RootElementTag))
                    _done = true;
            _deserializer.ValidateVersion(version);
        internal bool Done()
            if (!_done)
                    if (_reader.NodeType == XmlNodeType.EndElement)
            return _done;
            _deserializer.Stop();
        /// Deserializes next object.
        internal object Deserialize()
            string ignore;
            return Deserialize(out ignore);
        /// <param name="streamName">Stream the object belongs to (i.e. "Error", "Output", etc.).</param>
        internal object Deserialize(out string streamName)
            if (Done())
                throw PSTraceSource.NewInvalidOperationException(Serialization.ReadCalledAfterDone);
                return _deserializer.ReadOneObject(out streamName);
        #region Helper methods for dealing with "Deserialized." prefix
        /// Adds "Deserialized." prefix to passed in argument if not already present.
        internal static void AddDeserializationPrefix(ref string type)
            Dbg.Assert(type != null, "caller should validate the parameter");
            if (!type.StartsWith(Deserializer.DeserializationTypeNamePrefix, StringComparison.OrdinalIgnoreCase))
                type = type.Insert(0, Deserializer.DeserializationTypeNamePrefix);
        /// Checks if an object <paramref name="o"/> is either a live or deserialized instance of class <paramref name="type"/> or one of its subclasses.
        /// <returns><see langword="true"/> if <paramref name="o"/> is either a live or deserialized instance of class <paramref name="type"/> or one of its subclasses;  <see langword="false"/> otherwise.</returns>
        internal static bool IsInstanceOfType(object o, Type type)
            return type.IsInstanceOfType(PSObject.Base(o)) || IsDeserializedInstanceOfType(o, type);
        /// Checks if an object <paramref name="o"/> is a deserialized instance of class <paramref name="type"/> or one of its subclasses.
        /// <returns><see langword="true"/> if <paramref name="o"/> is a deserialized instance of class <paramref name="type"/> or one of its subclasses;  <see langword="false"/> otherwise.</returns>
        internal static bool IsDeserializedInstanceOfType(object o, Type type)
            PSObject pso = o as PSObject;
                IEnumerable<string> typeNames = pso.InternalTypeNames;
                if (typeNames != null)
                    foreach (string typeName in typeNames)
                        if (typeName.Length == Deserializer.DeserializationTypeNamePrefix.Length + type.FullName.Length &&
                            typeName.StartsWith(Deserializer.DeserializationTypeNamePrefix, StringComparison.OrdinalIgnoreCase) &&
                            typeName.EndsWith(type.FullName, StringComparison.OrdinalIgnoreCase))
            // not the right type
        internal static string MaskDeserializationPrefix(string typeName)
            if (typeName.StartsWith(Deserializer.DeserializationTypeNamePrefix, StringComparison.OrdinalIgnoreCase))
                typeName = typeName.Substring(Deserializer.DeserializationTypeNamePrefix.Length);
        /// Gets a new collection of typenames without "Deserialization." prefix
        /// in the typename. This will allow to map type info/format info of the original type
        /// for deserialized objects.
        /// <param name="typeNames"></param>
        /// Null if no type with "Deserialized." prefix is found.
        /// Otherwise <paramref name="typeNames"/> with the prefix removed if any.
        internal static Collection<string> MaskDeserializationPrefix(Collection<string> typeNames)
            Dbg.Assert(typeNames != null, "typeNames cannot be null");
            bool atleastOneDeserializedTypeFound = false;
            Collection<string> typesWithoutPrefix = new Collection<string>();
            foreach (string type in typeNames)
                if (type.StartsWith(Deserializer.DeserializationTypeNamePrefix,
                    atleastOneDeserializedTypeFound = true;
                    // remove *only* the prefix
                    typesWithoutPrefix.Add(type.Substring(Deserializer.DeserializationTypeNamePrefix.Length));
                    typesWithoutPrefix.Add(type);
            if (atleastOneDeserializedTypeFound)
                return typesWithoutPrefix;
        /// Used to prefix a typename for deserialization.
        private const string DeserializationTypeNamePrefix = "Deserialized.";
    /// Types of known type container supported by monad.
    internal enum ContainerType
        Dictionary,
        Queue,
        Stack,
        List,
        Enumerable,
        None
    internal class InternalSerializer
        internal const string DefaultVersion = "1.1.0.1";
        /// Serialization context.
        private readonly SerializationContext _context;
        /// Depth below top level - used to prevent infinitely deep serialization
        /// (without this protection it would be possible i.e. with SerializationDepth and recursion)
        private int _depthBelowTopLevel;
        private const int MaxDepthBelowTopLevel = 50;
        private readonly ReferenceIdHandlerForSerializer<object> _objectRefIdHandler;
        private readonly ReferenceIdHandlerForSerializer<ConsolidatedString> _typeRefIdHandler;
        internal InternalSerializer(XmlWriter writer, SerializationContext context)
            Dbg.Assert(context != null, "caller should validate the parameter");
            IDictionary<object, ulong> objectRefIdDictionary = null;
            if ((_context.options & SerializationOptions.NoObjectRefIds) == 0)
                objectRefIdDictionary = new WeakReferenceDictionary<UInt64>();
            _objectRefIdHandler = new ReferenceIdHandlerForSerializer<object>(objectRefIdDictionary);
            _typeRefIdHandler = new ReferenceIdHandlerForSerializer<ConsolidatedString>(
                new Dictionary<ConsolidatedString, UInt64>(ConsolidatedString.EqualityComparer));
            get { return _typeTable; }
            set { _typeTable = value; }
        internal void Start()
            if ((_context.options & SerializationOptions.NoRootElement) != SerializationOptions.NoRootElement)
                this.WriteStartElement(SerializationStrings.RootElementTag);
                this.WriteAttribute(SerializationStrings.VersionAttribute, InternalSerializer.DefaultVersion);
        /// Writes the end of root element.
        internal void End()
        internal static bool IsPrimitiveKnownType(Type input)
            TypeSerializationInfo pktInfo = KnownTypes.GetTypeSerializationInfo(input);
            return (pktInfo != null);
        /// Stream to which source belongs
        internal void WriteOneTopLevelObject
            string streamName
            Dbg.Assert(_depthBelowTopLevel == 0, "InternalSerializer.depthBelowTopLevel should be 0 at top-level");
            WriteOneObject(source, streamName, null, _context.depth);
        private void WriteOneObject
            string streamName,
            int depth
            this.CheckIfStopping();
                WriteNull(streamName, property);
                _depthBelowTopLevel++;
                Dbg.Assert(_depthBelowTopLevel <= MaxDepthBelowTopLevel, "depthBelowTopLevel should be <= MaxDepthBelowTopLevel");
                if (HandleMaxDepth(source, streamName, property))
                if (HandlePrimitiveKnownTypeByConvertingToPSObject(source, streamName, property, depth))
                // Object is not of primitive known type. Check if this has
                // already been serialized.
                string refId = _objectRefIdHandler.GetRefId(source);
                if (refId != null)
                    WritePSObjectReference(streamName, property, refId);
                if (HandlePrimitiveKnownTypePSObject(source, streamName, property, depth))
                // Note: We do not use containers in depth calculation. i.e even if the
                if (HandleKnownContainerTypes(source, streamName, property, depth))
                    HandlePSObjectAsString(mshSource, streamName, property, depth);
                HandleComplexTypePSObject(source, streamName, property, depth);
                _depthBelowTopLevel--;
                Dbg.Assert(_depthBelowTopLevel >= 0, "depthBelowTopLevel should be >= 0");
        private bool HandleMaxDepth(object source, string streamName, string property)
            if (_depthBelowTopLevel == MaxDepthBelowTopLevel)
                // assert commented out because of clashes with Wei's tests
                // Dbg.Assert(false, "We should never reach MaxDepthBelowTopLevel with non-malicious input");
                PSEtwLog.LogAnalyticError(PSEventId.Serializer_MaxDepthWhenSerializing, PSOpcode.Exception,
                    PSTask.Serialization, PSKeyword.Serializer, source.GetType().AssemblyQualifiedName, property, _depthBelowTopLevel);
                string content = Serialization.DeserializationTooDeep;
                HandlePrimitiveKnownType(content, streamName, property);
        private bool HandlePrimitiveKnownType
            string property
                WriteOnePrimitiveKnownType(this, streamName, property, source, pktInfo);
        /// Handles primitive known type by first converting it to a PSObject.In W8, extended
        /// property data is stored external to PSObject. By converting to PSObject, we will
        /// be able to retrieve and serialize the extended properties. This is tracked by
        /// Win8: 414042.
        /// <param name="streamName"></param>
        private bool HandlePrimitiveKnownTypeByConvertingToPSObject(
                PSObject pktInfoPSObject = PSObject.AsPSObject(source);
                return HandlePrimitiveKnownTypePSObject(pktInfoPSObject, streamName, property, depth);
        private bool HandleSecureString(object source, string streamName, string property)
            SecureString secureString = null;
            secureString = source as SecureString;
            PSObject moSource;
            if (secureString != null)
                moSource = PSObject.AsPSObject(secureString);
                moSource = source as PSObject;
            if (moSource != null && !moSource.ImmediateBaseObjectIsEmpty)
                // check if source is of type secure string
                secureString = moSource.ImmediateBaseObject as SecureString;
                    // the principle used in serialization is that serialization
                    // never throws, and if something can't be serialized nothing
                    // is written. So we write the elements only if encryption succeeds.
                    // However, in the case for non-Windows where secure string encryption
                    // is not yet supported, a PSCryptoException will be thrown.
                    string encryptedString;
                    if (_context.cryptoHelper != null)
                        encryptedString = _context.cryptoHelper.EncryptSecureString(secureString);
                        encryptedString = Microsoft.PowerShell.SecureStringHelper.Protect(secureString);
                        WriteStartElement(SerializationStrings.SecureStringTag);
                        WriteNameAttribute(property);
                        WriteAttribute(SerializationStrings.StreamNameAttribute, streamName);
                    // Note: We do not use WriteRaw for serializing secure string. WriteString
                    // does necessary escaping which may be needed for certain
                    // characters.
                    _writer.WriteString(encryptedString);
        private bool HandlePrimitiveKnownTypePSObject
            PSObject moSource = source as PSObject;
                    WritePrimitiveTypePSObject(moSource, baseObject, pktInfo, streamName, property, depth);
        private bool HandleKnownContainerTypes
            SerializationUtilities.GetKnownContainerTypeInfo(mshSource != null ? mshSource.ImmediateBaseObject : source, out ct,
            string refId = _objectRefIdHandler.SetRefId(source);
            WriteStartOfPSObject(
                mshSource ?? PSObject.AsPSObject(source),
                streamName,
                property,
                refId,
                true, // always write TypeNames information for known container types
                null); // never write ToString information for known container types
                    WriteDictionary(dictionary, SerializationStrings.DictionaryTag, depth);
                    WriteEnumerable(enumerable, SerializationStrings.StackTag, depth);
                    WriteEnumerable(enumerable, SerializationStrings.QueueTag, depth);
                    WriteEnumerable(enumerable, SerializationStrings.ListTag, depth);
                    WriteEnumerable(enumerable, SerializationStrings.CollectionTag, depth);
            if (depth != 0)
                // An object which is original enumerable becomes an PSObject with ArrayList on deserialization.
                // So on roundtrip it will show up as List.
                // We serialize properties of enumerable and on deserialization mark the object as Deserialized.
                // So if object is marked deserialized, we should write properties.
                if (ct == ContainerType.Enumerable || (mshSource != null && mshSource.IsDeserialized))
                    PSObject sourceAsPSObject = PSObject.AsPSObject(source);
                    PSMemberInfoInternalCollection<PSPropertyInfo> specificPropertiesToSerialize = SerializationUtilities.GetSpecificPropertiesToSerialize(sourceAsPSObject, AllPropertiesCollection, _typeTable);
                    WritePSObjectProperties(sourceAsPSObject, depth, specificPropertiesToSerialize);
                    SerializeExtendedProperties(sourceAsPSObject, depth, specificPropertiesToSerialize);
                // always serialize instance properties if there are any
                else if (mshSource != null)
                    SerializeInstanceProperties(mshSource, depth);
        /// Writes PSObject Reference Element.
        private void WritePSObjectReference
            string refId
            Dbg.Assert(!string.IsNullOrEmpty(refId), "caller should validate the parameter");
            WriteStartElement(SerializationStrings.ReferenceTag);
            WriteAttribute(SerializationStrings.ReferenceIdAttribute, refId);
        private static bool PSObjectHasModifiedTypesCollection(PSObject pso)
            ConsolidatedString currentTypes = pso.InternalTypeNames;
            Collection<string> originalTypes = pso.InternalAdapter.BaseGetTypeNameHierarchy(pso.ImmediateBaseObject);
            if (currentTypes.Count != originalTypes.Count)
            IEnumerator<string> currentEnumerator = currentTypes.GetEnumerator();
            IEnumerator<string> originalEnumerator = originalTypes.GetEnumerator();
            while (currentEnumerator.MoveNext() && originalEnumerator.MoveNext())
                if (!currentEnumerator.Current.Equals(originalEnumerator.Current, StringComparison.OrdinalIgnoreCase))
        /// source from which notes are written
        private void WritePrimitiveTypePSObject
            Dbg.Assert(source != null, "Caller should validate source != null");
            string toStringValue = SerializationUtilities.GetToStringForPrimitiveObject(source);
            bool hasModifiedTypesCollection = PSObjectHasModifiedTypesCollection(source);
            bool hasModifiedToString = (toStringValue != null);
            if (hasNotes || hasModifiedTypesCollection || hasModifiedToString)
                WritePrimitiveTypePSObjectWithNotes(
                    primitive,
                    hasModifiedTypesCollection,
                    toStringValue,
                    pktInfo,
                if (primitive != null)
                    WriteOnePrimitiveKnownType(this, streamName, property, primitive, pktInfo);
        /// Serializes an PSObject whose baseobject is of primitive type
        /// <param name="hasModifiedTypesCollection"></param>
        /// <param name="toStringValue"></param>
        private void WritePrimitiveTypePSObjectWithNotes
            bool hasModifiedTypesCollection,
            string toStringValue,
            Dbg.Assert(pktInfo != null, "Caller should validate pktInfo != null");
                hasModifiedTypesCollection, // preserve TypeNames information if different from the primitive type
                toStringValue); // preserve ToString information only if got it from deserialization or overridden by PSObject
                                // (example where preservation of TypeNames and ToString is needed: enums serialized as ints, help string with custom type names (HelpInfoShort))
                WriteOnePrimitiveKnownType(this, streamName, null, primitive, pktInfo);
            // serialize only instance properties - members from type table are
            // always going to be available for known primitive types
            SerializeInstanceProperties(source, depth);
        private void HandleComplexTypePSObject
            bool isErrorRecord = false;
            bool isInformationalRecord = false;
            bool isCimInstance = false;
            if (!mshSource.ImmediateBaseObjectIsEmpty)
                    CimInstance cimInstance = mshSource.ImmediateBaseObject as CimInstance;
                        isCimInstance = true;
                    ErrorRecord errorRecord = mshSource.ImmediateBaseObject as ErrorRecord;
                        errorRecord.ToPSObjectForRemoting(mshSource);
                        isErrorRecord = true;
                    InformationalRecord informationalRecord = mshSource.ImmediateBaseObject as InformationalRecord;
                    if (informationalRecord != null)
                        informationalRecord.ToPSObjectForRemoting(mshSource);
                        isInformationalRecord = true;
                    isEnum = mshSource.ImmediateBaseObject is Enum;
                    isPSObject = mshSource.ImmediateBaseObject is PSObject;
            bool writeToString = true;
            if (mshSource.ToStringFromDeserialization == null) // continue to write ToString from deserialized objects, but...
                if (mshSource.ImmediateBaseObjectIsEmpty) // ... don't write ToString for property bags
                    writeToString = false;
                mshSource,
                true, // always write TypeNames for complex objects
                writeToString ? SerializationUtilities.GetToString(mshSource) : null);
            PSMemberInfoInternalCollection<PSPropertyInfo> specificPropertiesToSerialize = SerializationUtilities.GetSpecificPropertiesToSerialize(mshSource, AllPropertiesCollection, _typeTable);
                object baseObject = mshSource.ImmediateBaseObject;
                WriteOneObject(System.Convert.ChangeType(baseObject, Enum.GetUnderlyingType(baseObject.GetType()), System.Globalization.CultureInfo.InvariantCulture), null, null, depth);
                WriteOneObject(mshSource.ImmediateBaseObject, null, null, depth);
            else if (isErrorRecord || isInformationalRecord)
                // nothing to do
                WritePSObjectProperties(mshSource, depth, specificPropertiesToSerialize);
            if (isCimInstance)
                PrepareCimInstanceForSerialization(mshSource, cimInstance);
            SerializeExtendedProperties(mshSource, depth, specificPropertiesToSerialize);
        private static readonly Lazy<CimSerializer> s_cimSerializer = new Lazy<CimSerializer>(CimSerializer.Create);
        private void PrepareCimInstanceForSerialization(PSObject psObject, CimInstance cimInstance)
            Queue<CimClassSerializationId> serializedClasses = new Queue<CimClassSerializationId>();
            // CREATE SERIALIZED FORM OF THE CLASS METADATA
            ArrayList psoClasses = new ArrayList();
            for (CimClass cimClass = cimInstance.CimClass; cimClass != null; cimClass = cimClass.CimSuperClass)
                PSObject psoClass = new PSObject();
                psoClass.TypeNames.Clear();
                psoClasses.Add(psoClass);
                psoClass.Properties.Add(new PSNoteProperty(InternalDeserializer.CimClassNameProperty, cimClass.CimSystemProperties.ClassName));
                psoClass.Properties.Add(new PSNoteProperty(InternalDeserializer.CimNamespaceProperty, cimClass.CimSystemProperties.Namespace));
                psoClass.Properties.Add(new PSNoteProperty(InternalDeserializer.CimServerNameProperty, cimClass.CimSystemProperties.ServerName));
                psoClass.Properties.Add(new PSNoteProperty(InternalDeserializer.CimHashCodeProperty, cimClass.GetHashCode()));
                CimClassSerializationId cimClassSerializationId = new CimClassSerializationId(
                    cimClass.CimSystemProperties.ClassName,
                    cimClass.CimSystemProperties.ServerName,
                    cimClass.GetHashCode());
                if (_context.cimClassSerializationIdCache.DoesDeserializerAlreadyHaveCimClass(cimClassSerializationId))
                serializedClasses.Enqueue(cimClassSerializationId);
                byte[] miXmlBytes = s_cimSerializer.Value.Serialize(cimClass, ClassSerializationOptions.None);
                string miXmlString = Encoding.Unicode.GetString(miXmlBytes, 0, miXmlBytes.Length);
                psoClass.Properties.Add(new PSNoteProperty(InternalDeserializer.CimMiXmlProperty, miXmlString));
            psoClasses.Reverse();
            // UPDATE CLASSDECL CACHE
            foreach (CimClassSerializationId serializedClassId in serializedClasses)
                _context.cimClassSerializationIdCache.AddClassToCache(serializedClassId);
            // ATTACH CLASS METADATA TO THE OBJECT BEING SERIALIZED
            PSPropertyInfo classMetadataProperty = psObject.Properties[InternalDeserializer.CimClassMetadataProperty];
            if (classMetadataProperty != null)
                classMetadataProperty.Value = psoClasses;
                PSNoteProperty classMetadataNote = new PSNoteProperty(
                    InternalDeserializer.CimClassMetadataProperty,
                    psoClasses);
                classMetadataNote.IsHidden = true;
                psObject.Properties.Add(classMetadataNote);
            // ATTACH INSTANCE METADATA TO THE OBJECT BEING SERIALIZED
            List<string> namesOfModifiedProperties = cimInstance
                .CimInstanceProperties
                .Where(static p => p.IsValueModified)
                .Select(static p => p.Name)
            if (namesOfModifiedProperties.Count != 0)
                PSObject instanceMetadata = new PSObject();
                PSPropertyInfo instanceMetadataProperty = psObject.Properties[InternalDeserializer.CimInstanceMetadataProperty];
                if (instanceMetadataProperty != null)
                    instanceMetadataProperty.Value = instanceMetadata;
                    PSNoteProperty instanceMetadataNote = new PSNoteProperty(InternalDeserializer.CimInstanceMetadataProperty, instanceMetadata);
                    instanceMetadataNote.IsHidden = true;
                    psObject.Properties.Add(instanceMetadataNote);
                instanceMetadata.InternalTypeNames = ConsolidatedString.Empty;
                instanceMetadata.Properties.Add(
                    new PSNoteProperty(
                        InternalDeserializer.CimModifiedProperties,
                        string.Join(' ', namesOfModifiedProperties)));
        /// <param name="refId"></param>
        /// <param name="writeTypeNames">If true, TypeName information is written, else not.</param>
        /// <param name="toStringValue">If not null then ToString information is written.</param>
        private void WriteStartOfPSObject
            string refId,
            bool writeTypeNames,
            string toStringValue
            // Write PSObject start element.
            WriteStartElement(SerializationStrings.PSObjectTag);
            if (writeTypeNames)
                // Write TypeNames
                ConsolidatedString typeNames = mshObject.InternalTypeNames;
                    string typeNameHierarchyReferenceId = _typeRefIdHandler.GetRefId(typeNames);
                    if (typeNameHierarchyReferenceId == null)
                        WriteStartElement(SerializationStrings.TypeNamesTag);
                        // Create a new refId and write it as attribute
                        string tnRefId = _typeRefIdHandler.SetRefId(typeNames);
                        Dbg.Assert(tnRefId != null, "SetRefId should always succeed for strings");
                        WriteAttribute(SerializationStrings.ReferenceIdAttribute, tnRefId);
                            WriteEncodedElementString(SerializationStrings.TypeNamesItemTag, type);
                        WriteStartElement(SerializationStrings.TypeNamesReferenceTag);
                        WriteAttribute(SerializationStrings.ReferenceIdAttribute, typeNameHierarchyReferenceId);
            if (toStringValue != null)
                WriteEncodedElementString(SerializationStrings.ToStringElementTag, toStringValue);
            Dbg.Assert(source != null, "Caller should validate the parameter");
        private bool? _canUseDefaultRunspaceInThreadSafeManner;
        private bool CanUseDefaultRunspaceInThreadSafeManner
                if (!_canUseDefaultRunspaceInThreadSafeManner.HasValue)
                    _canUseDefaultRunspaceInThreadSafeManner = Runspace.CanUseDefaultRunspace;
                return _canUseDefaultRunspaceInThreadSafeManner.Value;
        /// Serialize member set. This method serializes without writing
        /// enumerable containing members
        private void WriteMemberInfoCollection
            IEnumerable<PSMemberInfo> me,
            int depth,
            bool writeEnclosingMemberSetElementTag
            bool enclosingTagWritten = false;
                int depthOfMember = info.IsInstance ? depth : depth - 1;
                if (info.MemberType == (info.MemberType & PSMemberTypes.Properties))
                    bool gotValue;
                    object value = SerializationUtilities.GetPropertyValueInThreadSafeManner((PSPropertyInfo)info, this.CanUseDefaultRunspaceInThreadSafeManner, out gotValue);
                    if (gotValue)
                        if (writeEnclosingMemberSetElementTag && !enclosingTagWritten)
                            enclosingTagWritten = true;
                            WriteStartElement(SerializationStrings.MemberSet);
                        WriteOneObject(value, null, info.Name, depthOfMember);
                else if (info.MemberType == PSMemberTypes.MemberSet)
                    WriteMemberSet((PSMemberSet)info, depthOfMember);
            if (enclosingTagWritten)
        /// Serializes MemberSet.
        private void WriteMemberSet
            PSMemberSet set,
            Dbg.Assert(set != null, "Caller should validate the parameter");
            if (!set.ShouldSerialize)
            WriteNameAttribute(set.Name);
            WriteMemberInfoCollection(set.Members, depth, false);
        private void WritePSObjectProperties
            IEnumerable<PSPropertyInfo> specificPropertiesToSerialize
            if (specificPropertiesToSerialize != null)
                SerializeProperties(specificPropertiesToSerialize, SerializationStrings.AdapterProperties, depth);
                if (source.ShouldSerializeAdapter())
                    IEnumerable<PSPropertyInfo> adapterCollection = null;
                    adapterCollection = source.GetAdaptedProperties();
                    if (adapterCollection != null)
                        SerializeProperties(adapterCollection, SerializationStrings.AdapterProperties, depth);
        private void SerializeInstanceProperties
        private Collection<CollectionEntry<PSMemberInfo>> _extendedMembersCollection;
        private Collection<CollectionEntry<PSMemberInfo>> ExtendedMembersCollection
                return _extendedMembersCollection ??=
                    PSObject.GetMemberCollection(PSMemberViewTypes.Extended, _typeTable);
        private Collection<CollectionEntry<PSPropertyInfo>> _allPropertiesCollection;
        private Collection<CollectionEntry<PSPropertyInfo>> AllPropertiesCollection
                return _allPropertiesCollection ??= PSObject.GetPropertyCollection(PSMemberViewTypes.All, _typeTable);
        private void SerializeExtendedProperties
            IEnumerable<PSMemberInfo> extendedMembersEnumerable = null;
            if (specificPropertiesToSerialize == null)
                // Get only extended members including hidden members from the psobect source.
                    new PSMemberInfoIntegratingCollection<PSMemberInfo>(source, ExtendedMembersCollection);
                extendedMembersEnumerable = membersToSearch.Match(
                        PSMemberTypes.Properties | PSMemberTypes.PropertySet | PSMemberTypes.MemberSet,
                        MshMemberMatchOptions.IncludeHidden | MshMemberMatchOptions.OnlySerializable);
                List<PSMemberInfo> extendedMembersList = new List<PSMemberInfo>(source.InstanceMembers);
                extendedMembersEnumerable = extendedMembersList;
                foreach (PSMemberInfo member in specificPropertiesToSerialize)
                    if (member.IsInstance)
                    if (member is PSProperty)
                    extendedMembersList.Add(member);
            if (extendedMembersEnumerable != null)
                WriteMemberInfoCollection(extendedMembersEnumerable, depth, true);
        /// Collection of properties to serialize
        /// Name for enclosing element tag
        /// depth to which each property should be
        /// serialized
        private void SerializeProperties
            IEnumerable<PSPropertyInfo> propertyCollection,
            bool startElementWritten = false;
                if (info is not PSProperty prop)
                if (!startElementWritten)
                    WriteStartElement(name);
                    startElementWritten = true;
                object value = SerializationUtilities.GetPropertyValueInThreadSafeManner(prop, this.CanUseDefaultRunspaceInThreadSafeManner, out success);
                if (success)
                    WriteOneObject(value, null, prop.Name, depth);
            if (startElementWritten)
        /// enumerable which is serialized
        /// <param name="tag">
        private void WriteEnumerable
            IEnumerable enumerable,
            string tag,
            Dbg.Assert(!string.IsNullOrEmpty(tag), "caller should validate the parameter");
            // Start element
            WriteStartElement(tag);
                catch (System.NotSupportedException)
                    // ignore exceptions thrown when the enumerator doesn't support Reset() method as in win8:948569
                PSEtwLog.LogAnalyticWarning(
                    PSEventId.Serializer_EnumerationFailed, PSOpcode.Exception, PSTask.Serialization,
                    enumerable.GetType().AssemblyQualifiedName,
                    exception.ToString());
                    WriteOneObject(item, null, null, depth);
            // End element
        /// <param name="tag"></param>
        private void WriteDictionary
            IDictionary dictionary,
            Dbg.Assert(dictionary != null, "caller should validate the parameter");
                dictionaryEnum = dictionary.GetEnumerator();
            catch (Exception exception) // ignore non-severe exceptions
                    dictionary.GetType().AssemblyQualifiedName,
                    object key = null;
                        if (!dictionaryEnum.MoveNext())
                            key = dictionaryEnum.Key;
                            value = dictionaryEnum.Value;
                    Dbg.Assert(key != null, "Dictionary keys should never be null");
                    WriteStartElement(SerializationStrings.DictionaryEntryTag);
                    WriteOneObject(key, null, SerializationStrings.DictionaryKey, depth);
                    WriteOneObject(value, null, SerializationStrings.DictionaryValue, depth);
        private void HandlePSObjectAsString(
            string value = GetSerializationString(source);
            TypeSerializationInfo pktInfo = null;
                pktInfo = KnownTypes.GetTypeSerializationInfo(value.GetType());
            WritePrimitiveTypePSObject(source, value, pktInfo, streamName, property, depth);
        /// types.ps1xml.
        /// This string is used for serializing the PSObject at depth 0
        /// or when pso.SerializationMethod == SerializationMethod.String.
        /// PSObject to be converted to string
        private string GetSerializationString(PSObject source)
            PSPropertyInfo serializationProperty = null;
                serializationProperty = source.GetStringSerializationSource(_typeTable);
                    PSEventId.Serializer_ToStringFailed, PSOpcode.Exception, PSTask.Serialization,
                    source.GetType().AssemblyQualifiedName,
                    e.InnerException != null ? e.InnerException.ToString() : e.ToString());
                object val = SerializationUtilities.GetPropertyValueInThreadSafeManner(serializationProperty, this.CanUseDefaultRunspaceInThreadSafeManner, out success);
                if (success && (val != null))
                    result = SerializationUtilities.GetToString(val);
                result = SerializationUtilities.GetToString(source);
        /// and returns true if this object should be serialized as
        private bool SerializeAsString(PSObject source)
            SerializationMethod method = source.GetSerializationMethod(_typeTable);
            if (method == SerializationMethod.String)
                PSEtwLog.LogAnalyticVerbose(
                    PSEventId.Serializer_ModeOverride, PSOpcode.SerializationSettings, PSTask.Serialization,
                    source.InternalTypeNames.Key,
                    (UInt32)(SerializationMethod.String));
        private int GetDepthOfSerialization(object source, int depth)
            Dbg.Assert(source != null, "Caller should verify source != null");
            PSObject pso = PSObject.AsPSObject(source);
            if (pso.BaseObject is CimInstance)
            if (pso.BaseObject is PSCredential)
            if (pso.BaseObject is PSSenderInfo)
            if (pso.BaseObject is SwitchParameter)
            if ((_context.options & SerializationOptions.UseDepthFromTypes) != 0)
                int typesPs1xmlDepth = pso.GetSerializationDepth(_typeTable);
                if (typesPs1xmlDepth > 0)
                    if (typesPs1xmlDepth != depth)
                            PSEventId.Serializer_DepthOverride, PSOpcode.SerializationSettings, PSTask.Serialization,
                            pso.InternalTypeNames.Key, depth, typesPs1xmlDepth, _depthBelowTopLevel);
                        return typesPs1xmlDepth;
            if ((_context.options & SerializationOptions.PreserveSerializationSettingOfOriginal) != 0)
                if ((pso.IsDeserialized) && (depth <= 0))
        private void WriteNull(string streamName, string property)
            WriteStartElement(SerializationStrings.NilTag);
        /// Writes raw string as item or property in Monad namespace.
        /// <param name="serializer">The serializer to which the object is serialized.</param>
        /// <param name="streamName">Name of the stream to write. Do not write if null.</param>
        /// <param name="raw">String to write.</param>
        /// <param name="entry">Serialization information.</param>
        private static void WriteRawString
            InternalSerializer serializer,
            string raw,
            TypeSerializationInfo entry
            Dbg.Assert(serializer != null, "caller should have validated the information");
            Dbg.Assert(raw != null, "caller should have validated the information");
            Dbg.Assert(entry != null, "caller should have validated the information");
                serializer.WriteStartElement(entry.PropertyTag);
                serializer.WriteNameAttribute(property);
                serializer.WriteStartElement(entry.ItemTag);
                serializer.WriteAttribute(SerializationStrings.StreamNameAttribute, streamName);
            serializer._writer.WriteRaw(raw);
            serializer._writer.WriteEndElement();
        private static void WriteOnePrimitiveKnownType
            if (entry.Serializer == null)
                // we are not using GetToString, because we assume that
                // ToString() for primitive types never throws
                string value = Convert.ToString(source, CultureInfo.InvariantCulture);
                Dbg.Assert(value != null, "ToString shouldn't return null for primitive types");
                WriteRawString(serializer, streamName, property, value, entry);
                entry.Serializer(serializer, streamName, property, source, entry);
        /// Writes DateTime as item or property.
        /// <param name="property">Name of property. pass null for item.</param>
        /// <param name="source">DateTime to write.</param>
        internal static void WriteDateTime(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
            WriteRawString(serializer, streamName, property, XmlConvert.ToString((DateTime)source, XmlDateTimeSerializationMode.RoundtripKind), entry);
        /// Writes Version.
        /// <param name="source">Version to write.</param>
        internal static void WriteVersion(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
            Dbg.Assert(source is Version, "Caller should verify that typeof(source) is Version");
            WriteRawString(serializer, streamName, property, Convert.ToString(source, CultureInfo.InvariantCulture), entry);
        /// Writes SemanticVersion.
        internal static void WriteSemanticVersion(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
            Dbg.Assert(source is SemanticVersion, "Caller should verify that typeof(source) is Version");
        /// Serialize scriptblock as item or property.
        /// <param name="source">Scriptblock to write.</param>
        internal static void WriteScriptBlock(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
            Dbg.Assert(source is ScriptBlock, "Caller should verify that typeof(source) is ScriptBlock");
            WriteEncodedString(serializer, streamName, property, Convert.ToString(source, CultureInfo.InvariantCulture), entry);
        /// Serialize URI as item or property.
        /// <param name="source">URI to write.</param>
        internal static void WriteUri(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
            Dbg.Assert(source is Uri, "Caller should verify that typeof(source) is Uri");
        /// Serialize string as item or property.
        /// <param name="source">String to write.</param>
        internal static void WriteEncodedString(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
            Dbg.Assert(source is string, "caller should have validated the information");
            // Note: We do not use WriteRaw for serializing string. WriteString
            Dbg.Assert(source is string, "Caller should verify that typeof(source) is String");
            string s = (string)source;
            string encoded = EncodeString(s);
            serializer._writer.WriteString(encoded);
        /// Writes Double as item or property.
        /// <param name="source">Double to write.</param>
        internal static void WriteDouble(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
            WriteRawString(serializer, streamName, property, XmlConvert.ToString((double)source), entry);
        /// Writes Char as item or property.
        /// <param name="source">Char to write.</param>
        internal static void WriteChar(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
            // Char is defined as unsigned short in schema
            WriteRawString(serializer, streamName, property, XmlConvert.ToString((UInt16)(char)source), entry);
        /// Writes Boolean as item or property.
        /// <param name="source">Boolean to write.</param>
        internal static void WriteBoolean(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
            WriteRawString(serializer, streamName, property, XmlConvert.ToString((bool)source), entry);
        /// Writes Single as item or property.
        /// <param name="source">Single to write.</param>
        internal static void WriteSingle(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
            WriteRawString(serializer, streamName, property, XmlConvert.ToString((Single)source), entry);
        /// Writes TimeSpan as item or property.
        internal static void WriteTimeSpan(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
            WriteRawString(serializer, streamName, property, XmlConvert.ToString((TimeSpan)source), entry);
        /// <param name="source">Bytearray to write.</param>
        internal static void WriteByteArray(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
            byte[] bytes = (byte[])source;
            serializer._writer.WriteBase64(bytes, 0, bytes.Length);
        internal static void WriteXmlDocument(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
            string xml = ((XmlDocument)source).OuterXml;
            WriteEncodedString(serializer, streamName, property, xml, entry);
        internal static void WriteProgressRecord(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
            ProgressRecord rec = (ProgressRecord)source;
            serializer.WriteEncodedElementString(SerializationStrings.ProgressRecordActivity, rec.Activity);
            serializer.WriteEncodedElementString(SerializationStrings.ProgressRecordActivityId, rec.ActivityId.ToString(CultureInfo.InvariantCulture));
            serializer.WriteOneObject(rec.CurrentOperation, null, null, 1);
            serializer.WriteEncodedElementString(SerializationStrings.ProgressRecordParentActivityId, rec.ParentActivityId.ToString(CultureInfo.InvariantCulture));
            serializer.WriteEncodedElementString(SerializationStrings.ProgressRecordPercentComplete, rec.PercentComplete.ToString(CultureInfo.InvariantCulture));
            serializer.WriteEncodedElementString(SerializationStrings.ProgressRecordType, rec.RecordType.ToString());
            serializer.WriteEncodedElementString(SerializationStrings.ProgressRecordSecondsRemaining, rec.SecondsRemaining.ToString(CultureInfo.InvariantCulture));
            serializer.WriteEncodedElementString(SerializationStrings.ProgressRecordStatusDescription, rec.StatusDescription);
        internal static void WriteSecureString(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry)
            serializer.HandleSecureString(source, streamName, property);
        private void WriteStartElement(string elementTag)
            Dbg.Assert(!string.IsNullOrEmpty(elementTag), "Caller should validate the parameter");
            if ((_context.options & SerializationOptions.NoNamespace) == SerializationOptions.NoNamespace)
                _writer.WriteStartElement(elementTag);
                _writer.WriteStartElement(elementTag, SerializationStrings.MonadNamespace);
        private void WriteAttribute(string name, string value)
            Dbg.Assert(!string.IsNullOrEmpty(name), "Caller should validate the parameter");
            Dbg.Assert(value != null, "Caller should validate the parameter");
            _writer.WriteAttributeString(name, value);
        private void WriteNameAttribute(string value)
            Dbg.Assert(!string.IsNullOrEmpty(value), "Caller should validate the parameter");
            WriteAttribute(
                SerializationStrings.NameAttribute,
                EncodeString(value));
        /// Encodes the string to escape characters which would make XmlWriter.WriteString throw an exception.
        /// <param name="s">String to encode.</param>
        /// <returns>Encoded string.</returns>
        /// Output from this method can be reverted using XmlConvert.DecodeName method
        /// (or InternalDeserializer.DecodeString).
        /// This method has been introduced to produce shorter output than XmlConvert.EncodeName
        /// (which escapes everything that can't be part of an xml name - whitespace, punctuation).
        /// This method has been split into 2 parts to optimize its performance:
        /// 1) part1 (this method) checks if there are any encodable characters and
        ///    if there aren't it simply (and efficiently) returns the original string
        /// 2) part2 (EncodeString(string, int)) picks up when part1 detects the first encodable
        ///    character.  It avoids looking at the characters already verified by part1
        ///    and copies those already verified characters and then starts encoding
        ///    the rest of the string.
        internal static string EncodeString(string s)
            Dbg.Assert(s != null, "Caller should validate the parameter");
            int slen = s.Length;
            for (int i = 0; i < slen; ++i)
                // A control character is in ranges 0x00-0x1F or 0x7F-0x9F
                // The escape character is 0x5F ('_') if followed by an 'x'
                // A surrogate character is in range 0xD800-0xDFFF
                if (c <= 0x1F
                        || (c >= 0x7F && c <= 0x9F)
                        || (c >= 0xD800 && c <= 0xDFFF)
                        || (c == 0x5F && (i + 1 < slen) &&
                            ((s[i + 1] == 'x') || (s[i + 1] == 'X'))
                    return EncodeString(s, i);
            // No encodable characters were found above - simply return the original string
        private static readonly char[] s_hexlookup = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        /// This is the real workhorse that encodes strings.
        /// See <see cref="EncodeString(string)"/> for more information.
        /// <param name="indexOfFirstEncodableCharacter">IndexOfFirstEncodableCharacter.</param>
        private static string EncodeString(string s, int indexOfFirstEncodableCharacter)
            Dbg.Assert(s != null, "Caller should validate the 's' parameter");
            Dbg.Assert(indexOfFirstEncodableCharacter >= 0, "Caller should verify validity of indexOfFirstEncodableCharacter");
            Dbg.Assert(indexOfFirstEncodableCharacter < s.Length, "Caller should verify validity of indexOfFirstEncodableCharacter");
            char[] result = new char[indexOfFirstEncodableCharacter + (slen - indexOfFirstEncodableCharacter) * 7];
            s.CopyTo(0, result, 0, indexOfFirstEncodableCharacter);
            int rlen = indexOfFirstEncodableCharacter;
            for (int i = indexOfFirstEncodableCharacter; i < slen; ++i)
                if (c > 0x1F
                        && (c < 0x7F || c > 0x9F)
                        && (c < 0xD800 || c > 0xDFFF)
                        && (c != 0x5F || ((i + 1 >= slen) ||
                            ((s[i + 1] != 'x') && (s[i + 1] != 'X'))
                            )))
                    result[rlen++] = c;
                else if (c == 0x5F)
                    // Special case the escape character, encode _
                    result[rlen + 0] = '_';
                    result[rlen + 1] = 'x';
                    result[rlen + 2] = '0';
                    result[rlen + 3] = '0';
                    result[rlen + 4] = '5';
                    result[rlen + 5] = 'F';
                    result[rlen + 6] = '_';
                    rlen += 7;
                    // It is a control character or a unicode surrogate
                    result[rlen + 2 + 3] = s_hexlookup[c & 0x0F];
                    c >>= 4;
                    result[rlen + 2 + 2] = s_hexlookup[c & 0x0F];
                    result[rlen + 2 + 1] = s_hexlookup[c & 0x0F];
                    result[rlen + 2 + 0] = s_hexlookup[c & 0x0F];
            return new string(result, 0, rlen);
        /// Writes element string in monad namespace.
        private void WriteEncodedElementString(string name, string value)
            value = EncodeString(value);
                _writer.WriteElementString(name, value);
                _writer.WriteElementString(name, SerializationStrings.MonadNamespace, value);
    /// This internal class provides methods for de-serializing mshObject.
    internal class InternalDeserializer
        /// XmlReader from which object is deserialized.
        /// Deserialization context.
        /// If true, unknowntags are allowed inside PSObject.
        private bool UnknownTagsAllowed
                Dbg.Assert(_version.Major <= 1, "Deserializer assumes clixml version is <= 1.1");
                // If minor version is greater than 1, it means that there can be
                // some unknown tags in xml. Deserialization should ignore such element.
                return (_version.Minor > 1);
        private bool DuplicateRefIdsAllowed
                Version boundaryVersion = new Version(1, 1, 0, 1);
                return (_version < boundaryVersion);
                return true; // handle v1 stuff gracefully
        /// Depth below top level - used to prevent stack overflow during deserialization.
        /// Version declared by the clixml being read.
        private readonly ReferenceIdHandlerForDeserializer<object> _objectRefIdHandler;
        private readonly ReferenceIdHandlerForDeserializer<ConsolidatedString> _typeRefIdHandler;
        /// <param name="reader"></param>
        internal InternalDeserializer(XmlReader reader, DeserializationContext context)
            Dbg.Assert(reader != null, "caller should validate the parameter");
            _objectRefIdHandler = new ReferenceIdHandlerForDeserializer<object>();
            _typeRefIdHandler = new ReferenceIdHandlerForDeserializer<ConsolidatedString>();
        #region Known CIMTypes
        private static readonly Lazy<HashSet<Type>> s_knownCimArrayTypes = new Lazy<HashSet<Type>>(
                new HashSet<Type>
                    typeof(Int16),
                    typeof(Int32),
                    typeof(Int64),
                    typeof(TimeSpan),
                    typeof(UInt16),
                    typeof(UInt32),
                    typeof(UInt64),
                    typeof(object),
                    typeof(CimInstance)
        #region deserialization
        /// Validates the version for correctness. Also validates that deserializer
        /// can deserialize this version.
        /// <param name="version">
        /// version in string format
        internal void ValidateVersion(string version)
            Dbg.Assert(version != null, "Caller should validate the parameter");
            _version = null;
                _version = new Version(version);
                throw NewXmlException(Serialization.InvalidVersion, exceptionToRethrow);
            // Versioning Note:Future version of serialization can add new known types.
            // This version will ignore those known types, if they are base object.
            // It is expected that future version will still put information in base
            // and adapter properties which this serializer can read and use.
            // For example, assume the version 2 serialization engine supports a new known
            // type IPAddress. The version 1 deserializer doesn't know IPAddress as known
            // type and it must retrieve it as an PSObject. The version 2 serializer
            // can serialize this as follows:
            // <PSObject Version=1.2 Was=Deserialized.IPAddress >
            //  <TypeNames>...</TypeNames>
            //  <BaseObject>
            //      <IPAddress>120.23.35.53</IPAddress>
            //  </BaseObject>
            //  <Properties>
            //      <string name=Address>120.23.34.53</string>
            //      <string name=class>A</string>
            //  </Properties>
            // </PSObject>
            // In above example, V1 serializer will ignore <IPAddress> element and read
            // properties from <Properties>
            // V2 serializer can read <IPAddress> tag and ignore properties.
            // Read serialization note doc for information.
            // Now validate the major version number is 1
            if (_version.Major != 1)
                throw NewXmlException(Serialization.UnexpectedVersion, null, _version.Major);
        private object ReadOneDeserializedObject(out string streamName, out bool isKnownPrimitiveType)
            if (_reader.NodeType != XmlNodeType.Element)
                throw NewXmlException(Serialization.InvalidNodeType, null,
                                       _reader.NodeType.ToString(), nameof(XmlNodeType.Element));
            s_trace.WriteLine("Processing start node {0}", _reader.LocalName);
            streamName = _reader.GetAttribute(SerializationStrings.StreamNameAttribute);
            isKnownPrimitiveType = false;
            // handle nil node
            if (IsNextElement(SerializationStrings.NilTag))
                Skip();
            // Handle reference to previous deserialized object.
            if (IsNextElement(SerializationStrings.ReferenceTag))
                string refId = _reader.GetAttribute(SerializationStrings.ReferenceIdAttribute);
                if (refId == null)
                    throw NewXmlException(Serialization.AttributeExpected, null, SerializationStrings.ReferenceIdAttribute);
                object duplicate = _objectRefIdHandler.GetReferencedObject(refId);
                if (duplicate == null)
                    throw NewXmlException(Serialization.InvalidReferenceId, null, refId);
                return duplicate;
            // Handle primitive known types
            TypeSerializationInfo pktInfo = KnownTypes.GetTypeSerializationInfoFromItemTag(_reader.LocalName);
                s_trace.WriteLine("Primitive Knowntype Element {0}", pktInfo.ItemTag);
                isKnownPrimitiveType = true;
                return ReadPrimaryKnownType(pktInfo);
            // Handle PSObject
            if (IsNextElement(SerializationStrings.PSObjectTag))
                s_trace.WriteLine("PSObject Element");
                return ReadPSObject();
            // If we are here, we have an unknown node. Unknown nodes may
            // be allowed inside PSObject. We do not allow them at top level.
            s_trace.TraceError("Invalid element {0} tag found", _reader.LocalName);
            throw NewXmlException(Serialization.InvalidElementTag, null, _reader.LocalName);
        internal const string CimInstanceMetadataProperty = "__InstanceMetadata";
        internal const string CimModifiedProperties = "Modified";
        internal const string CimClassMetadataProperty = "__ClassMetadata";
        internal const string CimClassNameProperty = "ClassName";
        internal const string CimNamespaceProperty = "Namespace";
        internal const string CimServerNameProperty = "ServerName";
        internal const string CimHashCodeProperty = "Hash";
        internal const string CimMiXmlProperty = "MiXml";
        private static bool RehydrateCimInstanceProperty(
            PSPropertyInfo deserializedProperty,
            HashSet<string> namesOfModifiedProperties)
            Dbg.Assert(cimInstance != null, "Caller should make sure cimInstance != null");
            Dbg.Assert(deserializedProperty != null, "Caller should make sure deserializedProperty != null");
            if (deserializedProperty.Name.Equals(RemotingConstants.ComputerNameNoteProperty, StringComparison.OrdinalIgnoreCase))
                string psComputerNameValue = deserializedProperty.Value as string;
                if (psComputerNameValue != null)
                    cimInstance.SetCimSessionComputerName(psComputerNameValue);
            CimProperty cimProperty = cimInstance.CimInstanceProperties[deserializedProperty.Name];
            if (cimProperty == null)
            // TODO/FIXME - think if it is possible to do the array handling in a more efficient way
            object propertyValue = deserializedProperty.Value;
                PSObject psoPropertyValue = PSObject.AsPSObject(propertyValue);
                if (psoPropertyValue.BaseObject is ArrayList)
                    if ((psoPropertyValue.InternalTypeNames == null) || (psoPropertyValue.InternalTypeNames.Count == 0))
                    string originalArrayTypeName = Deserializer.MaskDeserializationPrefix(psoPropertyValue.InternalTypeNames[0]);
                    if (originalArrayTypeName == null)
                    Type originalArrayType;
                    if (!LanguagePrimitives.TryConvertTo(originalArrayTypeName, CultureInfo.InvariantCulture, out originalArrayType))
                    if (!originalArrayType.IsArray || !s_knownCimArrayTypes.Value.Contains(originalArrayType.GetElementType()))
                    object newPropertyValue;
                    if (!LanguagePrimitives.TryConvertTo(propertyValue, originalArrayType, CultureInfo.InvariantCulture, out newPropertyValue))
                    psoPropertyValue = PSObject.AsPSObject(newPropertyValue);
                propertyValue = psoPropertyValue.BaseObject;
                cimProperty.Value = propertyValue;
                if (!namesOfModifiedProperties.Contains(deserializedProperty.Name))
                    cimProperty.IsValueModified = false;
                    Dbg.Assert(cimProperty.IsValueModified, "Deserialized CIM properties should by default be marked as 'modified' ");
        private static readonly Lazy<CimDeserializer> s_cimDeserializer = new Lazy<CimDeserializer>(CimDeserializer.Create);
        private CimClass RehydrateCimClass(PSPropertyInfo classMetadataProperty)
            if ((classMetadataProperty == null) || (classMetadataProperty.Value == null))
            IEnumerable deserializedClasses = LanguagePrimitives.GetEnumerable(classMetadataProperty.Value);
            if (deserializedClasses == null)
            Stack<KeyValuePair<CimClassSerializationId, CimClass>> cimClassesToAddToCache = new Stack<KeyValuePair<CimClassSerializationId, CimClass>>();
            // REHYDRATE CLASS METADATA
            CimClass parentClass = null;
            CimClass currentClass = null;
            foreach (var deserializedClass in deserializedClasses)
                parentClass = currentClass;
                if (deserializedClass == null)
                PSObject psoDeserializedClass = PSObject.AsPSObject(deserializedClass);
                if (psoDeserializedClass.InstanceMembers[InternalDeserializer.CimNamespaceProperty] is not PSPropertyInfo namespaceProperty)
                string cimNamespace = namespaceProperty.Value as string;
                if (psoDeserializedClass.InstanceMembers[InternalDeserializer.CimClassNameProperty] is not PSPropertyInfo classNameProperty)
                string cimClassName = classNameProperty.Value as string;
                if (psoDeserializedClass.InstanceMembers[InternalDeserializer.CimServerNameProperty] is not PSPropertyInfo computerNameProperty)
                string computerName = computerNameProperty.Value as string;
                if (psoDeserializedClass.InstanceMembers[InternalDeserializer.CimHashCodeProperty] is not PSPropertyInfo hashCodeProperty)
                var hashCodeObject = hashCodeProperty.Value;
                if (hashCodeObject == null)
                if (hashCodeObject is PSObject)
                    hashCodeObject = ((PSObject)hashCodeObject).BaseObject;
                if (hashCodeObject is not int)
                int hashCode = (int)hashCodeObject;
                CimClassSerializationId cimClassSerializationId = new CimClassSerializationId(cimClassName, cimNamespace, computerName, hashCode);
                currentClass = _context.cimClassSerializationIdCache.GetCimClassFromCache(cimClassSerializationId);
                if (currentClass != null)
                PSPropertyInfo miXmlProperty = psoDeserializedClass.InstanceMembers[InternalDeserializer.CimMiXmlProperty] as PSPropertyInfo;
                if ((miXmlProperty == null) || (miXmlProperty.Value == null))
                string miXmlString = miXmlProperty.Value.ToString();
                byte[] miXmlBytes = Encoding.Unicode.GetBytes(miXmlString);
                    currentClass = s_cimDeserializer.Value.DeserializeClass(
                        miXmlBytes,
                        ref offset,
                        parentClass,
                        computerName: computerName,
                        namespaceName: cimNamespace);
                    cimClassesToAddToCache.Push(new KeyValuePair<CimClassSerializationId, CimClass>(cimClassSerializationId, currentClass));
            // UPDATE CLASSDECL DACHE
            foreach (var cacheEntry in cimClassesToAddToCache)
                _context.cimClassSerializationIdCache.AddCimClassToCache(cacheEntry.Key, cacheEntry.Value);
            return currentClass;
        // NOTE: Win7 change for refid-s that span multiple xml documents: ADMIN: changelist #226414
        private PSObject RehydrateCimInstance(PSObject deserializedObject)
            if (deserializedObject.BaseObject is not PSCustomObject)
                return deserializedObject;
            PSPropertyInfo classMetadataProperty = deserializedObject.InstanceMembers[CimClassMetadataProperty] as PSPropertyInfo;
            CimClass cimClass = RehydrateCimClass(classMetadataProperty);
            if (cimClass == null)
            CimInstance cimInstance;
                cimInstance = new CimInstance(cimClass);
            PSObject psoCimInstance = PSObject.AsPSObject(cimInstance);
            // process __InstanceMetadata
            HashSet<string> namesOfModifiedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            PSPropertyInfo instanceMetadataProperty = deserializedObject.InstanceMembers[CimInstanceMetadataProperty] as PSPropertyInfo;
            if ((instanceMetadataProperty != null) && (instanceMetadataProperty.Value != null))
                PSObject instanceMetadata = PSObject.AsPSObject(instanceMetadataProperty.Value);
                PSPropertyInfo modifiedPropertiesProperty = instanceMetadata.InstanceMembers[CimModifiedProperties] as PSPropertyInfo;
                if ((modifiedPropertiesProperty != null) && (modifiedPropertiesProperty.Value != null))
                    string modifiedPropertiesString = modifiedPropertiesProperty.Value.ToString();
                    foreach (string nameOfModifiedProperty in modifiedPropertiesString.Split(' '))
                        namesOfModifiedProperties.Add(nameOfModifiedProperty);
            // process properties that were originally "adapted" properties
            if (deserializedObject.AdaptedMembers != null)
                foreach (PSMemberInfo deserializedMemberInfo in deserializedObject.AdaptedMembers)
                    if (deserializedMemberInfo is not PSPropertyInfo deserializedProperty)
                    bool propertyHandledSuccessfully = RehydrateCimInstanceProperty(
                        deserializedProperty,
                        namesOfModifiedProperties);
                    if (!propertyHandledSuccessfully)
            // process properties that were originally "extended" properties
            foreach (PSMemberInfo deserializedMemberInfo in deserializedObject.InstanceMembers)
                // skip adapted properties
                if ((deserializedObject.AdaptedMembers != null) && (deserializedObject.AdaptedMembers[deserializedProperty.Name] != null))
                // skip metadata introduced by CliXml/CimInstance serialization
                if (deserializedProperty.Name.Equals(CimClassMetadataProperty, StringComparison.OrdinalIgnoreCase))
                // skip properties re-added by the client (i.e. through types.ps1xml)
                if (psoCimInstance.Properties[deserializedProperty.Name] != null)
                PSNoteProperty noteProperty = new PSNoteProperty(deserializedProperty.Name, deserializedProperty.Value);
                psoCimInstance.Properties.Add(noteProperty);
            return psoCimInstance;
        /// Reads one object. At this point reader should be positioned
        /// at the start tag of object.
        /// Deserialized Object.
        internal object ReadOneObject(out string streamName)
                    throw NewXmlException(Serialization.DeserializationTooDeep, null);
                bool isKnownPrimitiveType;
                object result = ReadOneDeserializedObject(out streamName, out isKnownPrimitiveType);
                if (!isKnownPrimitiveType)
                    PSObject mshSource = PSObject.AsPSObject(result);
                    if (Deserializer.IsDeserializedInstanceOfType(mshSource, typeof(CimInstance)))
                        return RehydrateCimInstance(mshSource);
                    // Convert deserialized object to a user-defined type (specified in a types.ps1xml file)
                    Type targetType = mshSource.GetTargetTypeForDeserialization(_typeTable);
                    if (targetType != null)
                        Exception rehydrationException = null;
                            object rehydratedResult = LanguagePrimitives.ConvertTo(
                                result, targetType, true /* recurse */, CultureInfo.InvariantCulture, _typeTable);
                            PSEtwLog.LogAnalyticVerbose(PSEventId.Serializer_RehydrationSuccess,
                                                        PSOpcode.Rehydration, PSTask.Serialization, PSKeyword.Serializer,
                                                        mshSource.InternalTypeNames.Key, targetType.FullName,
                                                        rehydratedResult.GetType().FullName);
                            return rehydratedResult;
                            rehydrationException = e;
                        Dbg.Assert(rehydrationException != null,
                                   "The only way to get here is with rehydrationException != null");
                        PSEtwLog.LogAnalyticError(PSEventId.Serializer_RehydrationFailure,
                                                  mshSource.InternalTypeNames.Key,
                                                  targetType.FullName,
                                                  rehydrationException.ToString(),
                                                  rehydrationException.InnerException == null
                                                      : rehydrationException.InnerException.ToString());
        private object ReadOneObject()
            return ReadOneObject(out ignore);
        // Reads one PSObject
        private PSObject ReadPSObject()
            PSObject dso = ReadAttributeAndCreatePSObject();
            // Read start element tag
            if (!ReadStartElementAndHandleEmpty(SerializationStrings.PSObjectTag))
                // Empty element.
                return dso;
            bool overrideTypeInfo = true;
            // Process all the child nodes
            while (_reader.NodeType == XmlNodeType.Element)
                if (IsNextElement(SerializationStrings.TypeNamesTag) ||
                    IsNextElement(SerializationStrings.TypeNamesReferenceTag))
                    ReadTypeNames(dso);
                    overrideTypeInfo = false;
                else if (IsNextElement(SerializationStrings.AdapterProperties))
                    ReadProperties(dso);
                else if (IsNextElement(SerializationStrings.MemberSet))
                    ReadMemberSet(dso.InstanceMembers);
                else if (IsNextElement(SerializationStrings.ToStringElementTag))
                    dso.ToStringFromDeserialization = ReadDecodedElementString(SerializationStrings.ToStringElementTag);
                    dso.InstanceMembers.Add(PSObject.DotNetInstanceAdapter.GetDotNetMethod<PSMemberInfo>(dso, "ToString"));
                    PSGetMemberBinder.SetHasInstanceMember("ToString");
                    // Fix for Win8:75437
                    // The TokenText property is used in type conversion and it is not being populated during deserialization
                    // As a result, parameter binding fails in the following case on a remote session
                    // register-psssessionconfiguration -Name foo -psversion 3.0
                    // The value "3.0" is treated as a double and since the TokenText property holds null, the type converter tries to convert
                    // from System.Double to System.Version using Parse method of System.Version and fails
                    dso.TokenText = dso.ToStringFromDeserialization;
                    // Handle BaseObject
                    object baseObject = null;
                    // Check if tag is PrimaryKnownType.
                        baseObject = ReadPrimaryKnownType(pktInfo);
                    else if (IsKnownContainerTag(out ct))
                        s_trace.WriteLine("Found container node {0}", ct);
                        baseObject = ReadKnownContainer(ct, dso.InternalTypeNames);
                    else if (IsNextElement(SerializationStrings.PSObjectTag))
                        s_trace.WriteLine("Found PSObject node");
                        baseObject = ReadOneObject();
                        // We have an unknown tag
                        s_trace.WriteLine("Unknown tag {0} encountered", _reader.LocalName);
                        if (UnknownTagsAllowed)
                        dso.SetCoreOnDeserialization(baseObject, overrideTypeInfo);
            PSObject immediateBasePso = dso.ImmediateBaseObject as PSObject;
            if (immediateBasePso != null)
                PSObject.CopyDeserializerFields(source: immediateBasePso, target: dso);
        /// This function reads the refId attribute and creates a
        /// mshObject for that attribute.
        /// <returns>MshObject which is created for refId.</returns>
        private PSObject ReadAttributeAndCreatePSObject()
            PSObject sh = new PSObject();
            // RefId is not mandatory attribute
                s_trace.WriteLine("Read PSObject with refId: {0}", refId);
                _objectRefIdHandler.SetRefId(sh, refId, this.DuplicateRefIdsAllowed);
            return sh;
        /// Read type names.
        /// <param name="dso">
        /// PSObject to which TypeNames are added
        private void ReadTypeNames(PSObject dso)
            Dbg.Assert(dso != null, "caller should validate the parameter");
            Dbg.Assert(_reader.NodeType == XmlNodeType.Element, "NodeType should be Element");
            if (IsNextElement(SerializationStrings.TypeNamesTag))
                Collection<string> typeNames = new Collection<string>();
                // Read refId attribute if available
                s_trace.WriteLine("Processing TypeNamesTag with refId {0}", refId);
                if (ReadStartElementAndHandleEmpty(SerializationStrings.TypeNamesTag))
                        if (IsNextElement(SerializationStrings.TypeNamesItemTag))
                            string item = ReadDecodedElementString(SerializationStrings.TypeNamesItemTag);
                            if (!string.IsNullOrEmpty(item))
                                Deserializer.AddDeserializationPrefix(ref item);
                                typeNames.Add(item);
                dso.InternalTypeNames = new ConsolidatedString(typeNames);
                    _typeRefIdHandler.SetRefId(dso.InternalTypeNames, refId, this.DuplicateRefIdsAllowed);
            else if (IsNextElement(SerializationStrings.TypeNamesReferenceTag))
                s_trace.WriteLine("Processing TypeNamesReferenceTag with refId {0}", refId);
                ConsolidatedString typeNames = _typeRefIdHandler.GetReferencedObject(refId);
                if (typeNames == null)
                    throw NewXmlException(Serialization.InvalidTypeHierarchyReferenceId, null, refId);
                // At this point we know that we will clone the ConsolidatedString object, so we might end up
                // allocating much more memory than the length of the xml string
                // We have to account for that to limit that to remoting quota and protect against OOM.
                _context.LogExtraMemoryUsage(
                    typeNames.Key.Length * sizeof(char) // Key is shared among the cloned and original object
                                                        // but the list of strings isn't.  The expression to the left
                                                        // is roughly the size of memory the list of strings occupies
                    - 29 // size of <Obj><TNRef RefId="0"/></Obj> in UTF8 encoding
                // Skip the node
                Dbg.Assert(false, "caller should validate that we do no reach here");
        /// Read properties.
        private void ReadProperties(PSObject dso)
            // Since we are adding baseobject properties as propertybag,
            // mark the object as deserialized.
            dso.IsDeserialized = true;
            dso.AdaptedMembers = new PSMemberInfoInternalCollection<PSPropertyInfo>();
            // Add the GetType method to the instance members, so that it works on deserialized psobjects
            dso.InstanceMembers.Add(PSObject.DotNetInstanceAdapter.GetDotNetMethod<PSMemberInfo>(dso, "GetType"));
            PSGetMemberBinder.SetHasInstanceMember("GetType");
            // Set Clr members to a collection which is empty
            dso.ClrMembers = new PSMemberInfoInternalCollection<PSPropertyInfo>();
            if (ReadStartElementAndHandleEmpty(SerializationStrings.AdapterProperties))
                // Read one or more property elements
                    string property = ReadNameAttribute();
                    object value = ReadOneObject();
                    PSProperty prop = new PSProperty(property, value);
                    dso.AdaptedMembers.Add(prop);
        #region memberset
        /// Read memberset.
        /// collection to which members are added
        private void ReadMemberSet(PSMemberInfoCollection<PSMemberInfo> collection)
            Dbg.Assert(collection != null, "caller should validate the value");
            if (ReadStartElementAndHandleEmpty(SerializationStrings.MemberSet))
                    if (IsNextElement(SerializationStrings.MemberSet))
                        string name = ReadNameAttribute();
                        PSMemberSet set = new PSMemberSet(name);
                        collection.Add(set);
                        ReadMemberSet(set.Members);
                        PSGetMemberBinder.SetHasInstanceMember(name);
                        PSNoteProperty note = ReadNoteProperty();
                        collection.Add(note);
                        PSGetMemberBinder.SetHasInstanceMember(note.Name);
        /// Read note.
        private PSNoteProperty ReadNoteProperty()
            PSNoteProperty note = new PSNoteProperty(name, value);
            return note;
        #endregion memberset
        #region known container
        private bool IsKnownContainerTag(out ContainerType ct)
            Dbg.Assert(_reader.NodeType == XmlNodeType.Element, "Expected node type is element");
            if (IsNextElement(SerializationStrings.DictionaryTag))
            else if (IsNextElement(SerializationStrings.QueueTag))
            else if (IsNextElement(SerializationStrings.StackTag))
            else if (IsNextElement(SerializationStrings.ListTag))
            else if (IsNextElement(SerializationStrings.CollectionTag))
            return ct != ContainerType.None;
        private object ReadKnownContainer(ContainerType ct, ConsolidatedString InternalTypeNames)
                    return ReadDictionary(ct, InternalTypeNames);
                    return ReadListContainer(ct);
                    Dbg.Assert(false, "Unrecognized ContainerType enum");
        /// Read List Containers.
        private object ReadListContainer(ContainerType ct)
            Dbg.Assert(ct == ContainerType.Enumerable ||
                       ct == ContainerType.List ||
                       ct == ContainerType.Queue ||
                       ct == ContainerType.Stack, "ct should be queue, stack, enumerable or list");
            ArrayList list = new ArrayList();
            if (ReadStartElementAndHandleEmpty(_reader.LocalName))
                    list.Add(ReadOneObject());
            if (ct == ContainerType.Stack)
                list.Reverse();
                return new Stack(list);
            else if (ct == ContainerType.Queue)
                return new Queue(list);
        /// Utility class for ReadDictionary(), supporting ordered or non-ordered Dictionary methods.
        private class PSDictionary
            private IDictionary dict;
            private readonly bool _isOrdered;
            private int _keyClashFoundIteration = 0;
            public PSDictionary(bool isOrdered) {
                _isOrdered = isOrdered;
                // By default use a non case-sensitive comparer
                if (_isOrdered) {
                    dict = new OrderedDictionary(StringComparer.CurrentCultureIgnoreCase);
                    dict = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
            public object DictionaryObject { get { return dict; } }
            public void Add(object key, object value) {
                // On the first collision, copy the hash table to one that uses the `key` object's default comparer.
                if (_keyClashFoundIteration == 0 && dict.Contains(key))
                    _keyClashFoundIteration++;
                    IDictionary newDict = _isOrdered ? new OrderedDictionary(dict.Count) : new Hashtable(dict.Count);
                    foreach (DictionaryEntry entry in dict) {
                        newDict.Add(entry.Key, entry.Value);
                    dict = newDict;
                // win8: 389060. If there are still collisions even with case-sensitive default comparer,
                // use an IEqualityComparer that does object ref equality.
                if (_keyClashFoundIteration == 1 && dict.Contains(key))
                    IEqualityComparer equalityComparer = new ReferenceEqualityComparer();
                    IDictionary newDict = _isOrdered ?
                                            new OrderedDictionary(dict.Count, equalityComparer) :
                                            new Hashtable(dict.Count, equalityComparer);
                dict.Add(key, value);
        /// Deserialize Dictionary.
        private object ReadDictionary(ContainerType ct, ConsolidatedString InternalTypeNames)
            Dbg.Assert(ct == ContainerType.Dictionary, "Unrecognized ContainerType enum");
            // We assume the hash table is a PowerShell hash table and hence uses
            // a case insensitive string comparer.  If we discover a key collision,
            // we'll revert back to the default comparer.
            // Find whether original directory was ordered
            bool isOrdered = InternalTypeNames.Count > 0 &&
                (Deserializer.MaskDeserializationPrefix(InternalTypeNames[0]) == typeof(OrderedDictionary).FullName);
            PSDictionary dictionary = new PSDictionary(isOrdered);
            if (ReadStartElementAndHandleEmpty(SerializationStrings.DictionaryTag))
                    ReadStartElement(SerializationStrings.DictionaryEntryTag);
                    // Read Key
                        throw NewXmlException(Serialization.DictionaryKeyNotSpecified, null);
                    if (!string.Equals(name, SerializationStrings.DictionaryKey, StringComparison.OrdinalIgnoreCase))
                        throw NewXmlException(Serialization.InvalidDictionaryKeyName, null);
                    object key = ReadOneObject();
                        throw NewXmlException(Serialization.NullAsDictionaryKey, null);
                    // Read Value
                        throw NewXmlException(Serialization.DictionaryValueNotSpecified, null);
                    name = ReadNameAttribute();
                    if (!string.Equals(name, SerializationStrings.DictionaryValue, StringComparison.OrdinalIgnoreCase))
                        throw NewXmlException(Serialization.InvalidDictionaryValueName, null);
                        // Add entry to hashtable
                        dictionary.Add(key, value);
                        throw this.NewXmlException(Serialization.InvalidPrimitiveType, e, typeof(Hashtable));
            return dictionary.DictionaryObject;
        #endregion known containers
        #endregion deserialization
        #region Getting XmlReaderSettings
        internal static XmlReaderSettings XmlReaderSettingsForCliXml { get; } = GetXmlReaderSettingsForCliXml();
        private static XmlReaderSettings GetXmlReaderSettingsForCliXml()
            XmlReaderSettings xrs = new XmlReaderSettings();
            xrs.ConformanceLevel = ConformanceLevel.Document;
            xrs.IgnoreComments = true;
            xrs.IgnoreWhitespace = false;
            xrs.DtdProcessing = DtdProcessing.Prohibit;
            xrs.Schemas = null;
            xrs.ValidationFlags = System.Xml.Schema.XmlSchemaValidationFlags.None;
            xrs.ValidationType = ValidationType.None;
        internal static XmlReaderSettings XmlReaderSettingsForUntrustedXmlDocument { get; } = GetXmlReaderSettingsForUntrustedXmlDocument();
        private static XmlReaderSettings GetXmlReaderSettingsForUntrustedXmlDocument()
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Auto;
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.MaxCharactersFromEntities = 1024;
            settings.MaxCharactersInDocument = 512 * 1024 * 1024; // 512M characters = 1GB
            settings.DtdProcessing = DtdProcessing.Parse;   // Allowing DTD parsing with limits of MaxCharactersFromEntities/MaxCharactersInDocument
            settings.ValidationFlags = System.Xml.Schema.XmlSchemaValidationFlags.None;
            settings.ValidationType = ValidationType.None;
            return settings;
        #region known type deserialization
        internal static object DeserializeBoolean(InternalDeserializer deserializer)
            Dbg.Assert(deserializer != null, "Caller should validate the parameter");
                return XmlConvert.ToBoolean(deserializer._reader.ReadElementContentAsString());
                throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, e, typeof(bool).FullName);
        internal static object DeserializeByte(InternalDeserializer deserializer)
            Exception recognizedException = null;
                return XmlConvert.ToByte(deserializer._reader.ReadElementContentAsString());
                recognizedException = e;
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, recognizedException, typeof(byte).FullName);
        internal static object DeserializeChar(InternalDeserializer deserializer)
                return (char)XmlConvert.ToUInt16(deserializer._reader.ReadElementContentAsString());
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, recognizedException, typeof(char).FullName);
        internal static object DeserializeDateTime(InternalDeserializer deserializer)
                return XmlConvert.ToDateTime(deserializer._reader.ReadElementContentAsString(), XmlDateTimeSerializationMode.RoundtripKind);
                throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, e, typeof(DateTime).FullName);
        internal static object DeserializeDecimal(InternalDeserializer deserializer)
                return XmlConvert.ToDecimal(deserializer._reader.ReadElementContentAsString());
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, recognizedException, typeof(decimal).FullName);
        internal static object DeserializeDouble(InternalDeserializer deserializer)
                return XmlConvert.ToDouble(deserializer._reader.ReadElementContentAsString());
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, recognizedException, typeof(double).FullName);
        internal static object DeserializeGuid(InternalDeserializer deserializer)
                return XmlConvert.ToGuid(deserializer._reader.ReadElementContentAsString());
            // MSDN for XmlConvert.ToGuid doesn't list any exceptions, but
            // Reflector shows that this just calls to new Guid(string)
            // which MSDN documents can throw Format/OverflowException
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, recognizedException, typeof(Guid).FullName);
        internal static object DeserializeVersion(InternalDeserializer deserializer)
                return new Version(deserializer._reader.ReadElementContentAsString());
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, recognizedException, typeof(Version).FullName);
        internal static object DeserializeSemanticVersion(InternalDeserializer deserializer)
                return new SemanticVersion(deserializer._reader.ReadElementContentAsString());
        internal static object DeserializeInt16(InternalDeserializer deserializer)
                return XmlConvert.ToInt16(deserializer._reader.ReadElementContentAsString());
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, recognizedException, typeof(Int16).FullName);
        internal static object DeserializeInt32(InternalDeserializer deserializer)
                return XmlConvert.ToInt32(deserializer._reader.ReadElementContentAsString());
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, recognizedException, typeof(Int32).FullName);
        internal static object DeserializeInt64(InternalDeserializer deserializer)
                return XmlConvert.ToInt64(deserializer._reader.ReadElementContentAsString());
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, recognizedException, typeof(Int64).FullName);
        internal static object DeserializeSByte(InternalDeserializer deserializer)
                return XmlConvert.ToSByte(deserializer._reader.ReadElementContentAsString());
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, recognizedException, typeof(sbyte).FullName);
        internal static object DeserializeSingle(InternalDeserializer deserializer)
                return XmlConvert.ToSingle(deserializer._reader.ReadElementContentAsString());
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, recognizedException, typeof(float).FullName);
        internal static object DeserializeScriptBlock(InternalDeserializer deserializer)
            string scriptBlockBody = deserializer.ReadDecodedElementString(SerializationStrings.ScriptBlockTag);
            if ((deserializer._context.options & DeserializationOptions.DeserializeScriptBlocks) == DeserializationOptions.DeserializeScriptBlocks)
                return ScriptBlock.Create(scriptBlockBody);
                // Scriptblock is deserialized as string
                return scriptBlockBody;
        internal static object DeserializeString(InternalDeserializer deserializer)
            return deserializer.ReadDecodedElementString(SerializationStrings.StringTag);
        internal static object DeserializeTimeSpan(InternalDeserializer deserializer)
                return XmlConvert.ToTimeSpan(deserializer._reader.ReadElementContentAsString());
                throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, e, typeof(TimeSpan).FullName);
        internal static object DeserializeUInt16(InternalDeserializer deserializer)
                return XmlConvert.ToUInt16(deserializer._reader.ReadElementContentAsString());
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, recognizedException, typeof(UInt16).FullName);
        internal static object DeserializeUInt32(InternalDeserializer deserializer)
                return XmlConvert.ToUInt32(deserializer._reader.ReadElementContentAsString());
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, recognizedException, typeof(UInt32).FullName);
        internal static object DeserializeUInt64(InternalDeserializer deserializer)
                return XmlConvert.ToUInt64(deserializer._reader.ReadElementContentAsString());
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, recognizedException, typeof(UInt64).FullName);
        internal static object DeserializeUri(InternalDeserializer deserializer)
                string uriString = deserializer.ReadDecodedElementString(SerializationStrings.AnyUriTag);
                return new Uri(uriString, UriKind.RelativeOrAbsolute);
            catch (UriFormatException e)
                throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, e, typeof(Uri).FullName);
        internal static object DeserializeByteArray(InternalDeserializer deserializer)
                return Convert.FromBase64String(deserializer._reader.ReadElementContentAsString());
                throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, e, typeof(byte[]).FullName);
        /// <exception cref="System.Xml.XmlException"></exception>
        internal static XmlDocument LoadUnsafeXmlDocument(FileInfo xmlPath, bool preserveNonElements, int? maxCharactersInDocument)
            XmlDocument doc = null;
            // same FileStream options as Reflector shows for XmlDocument.Load(path) / XmlDownloadManager.GetStream:
            using (Stream stream = new FileStream(xmlPath.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                doc = LoadUnsafeXmlDocument(stream, preserveNonElements, maxCharactersInDocument);
        internal static XmlDocument LoadUnsafeXmlDocument(string xmlContents, bool preserveNonElements, int? maxCharactersInDocument)
            using (TextReader textReader = new StringReader(xmlContents))
                return LoadUnsafeXmlDocument(textReader, preserveNonElements, maxCharactersInDocument);
        internal static XmlDocument LoadUnsafeXmlDocument(Stream stream, bool preserveNonElements, int? maxCharactersInDocument)
            using (TextReader textReader = new StreamReader(stream))
        internal static XmlDocument LoadUnsafeXmlDocument(TextReader textReader, bool preserveNonElements, int? maxCharactersInDocument)
            XmlReaderSettings settings;
            if (maxCharactersInDocument.HasValue || preserveNonElements)
                settings = InternalDeserializer.XmlReaderSettingsForUntrustedXmlDocument.Clone();
                if (maxCharactersInDocument.HasValue)
                    settings.MaxCharactersInDocument = maxCharactersInDocument.Value;
                if (preserveNonElements)
                    settings.IgnoreWhitespace = false;
                settings = InternalDeserializer.XmlReaderSettingsForUntrustedXmlDocument;
                xmlDocument.PreserveWhitespace = preserveNonElements;
                throw new XmlException(invalidOperationException.Message, invalidOperationException);
        internal static object DeserializeXmlDocument(InternalDeserializer deserializer)
            string docAsString = deserializer.ReadDecodedElementString(SerializationStrings.XmlDocumentTag);
                int? maxCharactersInDocument = null;
                if (deserializer._context.MaximumAllowedMemory.HasValue)
                    maxCharactersInDocument = deserializer._context.MaximumAllowedMemory.Value / sizeof(char);
                    docAsString,
                    maxCharactersInDocument);
                deserializer._context.LogExtraMemoryUsage((docAsString.Length - doc.OuterXml.Length) * sizeof(char));
                throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, e, typeof(XmlDocument).FullName);
        internal static object DeserializeProgressRecord(InternalDeserializer deserializer)
            // read deserialized elements of a progress record
            deserializer.ReadStartElement(SerializationStrings.ProgressRecord);
            string activity = null, currentOperation = null, prt = null, statusDescription = null;
            int activityId = 0, parentActivityId = 0, percentComplete = 0, secondsRemaining = 0;
                activity = deserializer.ReadDecodedElementString(SerializationStrings.ProgressRecordActivity);
                activityId = int.Parse(deserializer.ReadDecodedElementString(SerializationStrings.ProgressRecordActivityId), CultureInfo.InvariantCulture);
                object tmp = deserializer.ReadOneObject();
                currentOperation = tmp?.ToString();
                parentActivityId = int.Parse(deserializer.ReadDecodedElementString(SerializationStrings.ProgressRecordParentActivityId), CultureInfo.InvariantCulture);
                percentComplete = int.Parse(deserializer.ReadDecodedElementString(SerializationStrings.ProgressRecordPercentComplete), CultureInfo.InvariantCulture);
                prt = deserializer.ReadDecodedElementString(SerializationStrings.ProgressRecordType);
                secondsRemaining = int.Parse(deserializer.ReadDecodedElementString(SerializationStrings.ProgressRecordSecondsRemaining), CultureInfo.InvariantCulture);
                statusDescription = deserializer.ReadDecodedElementString(SerializationStrings.ProgressRecordStatusDescription);
            if (recognizedException != null)
            deserializer.ReadEndElement();
            // Build the progress record
            ProgressRecordType type;
                type = (ProgressRecordType)Enum.Parse(typeof(ProgressRecordType), prt, true);
                throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, e, typeof(ProgressRecord).FullName);
                ProgressRecord record = new ProgressRecord(activityId, activity, statusDescription);
                if (!string.IsNullOrEmpty(currentOperation))
                    record.CurrentOperation = currentOperation;
                record.ParentActivityId = parentActivityId;
                record.PercentComplete = percentComplete;
                record.RecordType = type;
                record.SecondsRemaining = secondsRemaining;
        internal static object DeserializeSecureString(InternalDeserializer deserializer)
            // read deserialized elements of a Secure String
            return deserializer.ReadSecureString();
        #endregion known type deserialization
        /// Check if LocalName of next element is "tag"
        private bool IsNextElement(string tag)
            Dbg.Assert(!string.IsNullOrEmpty(tag), "Caller should validate the parameter");
            return (_reader.LocalName == tag) &&
                (((_context.options & DeserializationOptions.NoNamespace) != 0) ||
                 (_reader.NamespaceURI == SerializationStrings.MonadNamespace));
        /// Read start element in monad namespace.
        /// <param name="element">Element tag to read.</param>
        /// <returns>True if not an empty element else false.</returns>
        internal bool ReadStartElementAndHandleEmpty(string element)
            Dbg.Assert(!string.IsNullOrEmpty(element), "Caller should validate the parameter");
            // IsEmpty is set to true when element is of the form <tag/>
            bool isEmpty = _reader.IsEmptyElement;
            this.ReadStartElement(element);
            // This takes care of the case: <tag></tag> or <tag>  </tag>. In
            // this case isEmpty is false.
            if (!isEmpty && _reader.NodeType == XmlNodeType.EndElement)
                isEmpty = true;
            return !isEmpty;
        private void ReadStartElement(string element)
            if ((_context.options & DeserializationOptions.NoNamespace) == DeserializationOptions.NoNamespace)
                _reader.ReadStartElement(element);
                _reader.ReadStartElement(element, SerializationStrings.MonadNamespace);
        private string ReadDecodedElementString(string element)
            string temp = null;
                temp = _reader.ReadElementContentAsString(element, string.Empty);
                temp = _reader.ReadElementContentAsString(element, SerializationStrings.MonadNamespace);
            temp = DecodeString(temp);
            return temp;
        /// Skips an element and all its child elements.
        /// Moves cursor to next content Node.
        private void Skip()
        /// Reads Primary known type.
        /// <param name="pktInfo"></param>
        private object ReadPrimaryKnownType(TypeSerializationInfo pktInfo)
            Dbg.Assert(pktInfo != null, "Deserializer should be available");
            Dbg.Assert(pktInfo.Deserializer != null, "Deserializer should be available");
            object result = pktInfo.Deserializer(this);
        private object ReadSecureString()
            string encryptedString = _reader.ReadElementContentAsString();
                    result = _context.cryptoHelper.DecryptSecureString(encryptedString);
                    result = Microsoft.PowerShell.SecureStringHelper.Unprotect(encryptedString);
            catch (PSCryptoException)
                throw NewXmlException(Serialization.DeserializeSecureStringFailed, null);
        /// Helper function for building XmlException.
        /// resource String
        /// params for format string obtained from resourceId
        private XmlException NewXmlException
            params object[] args
            Dbg.Assert(!string.IsNullOrEmpty(resourceString), "Caller should validate the parameter");
            string message = StringUtil.Format(resourceString, args);
            XmlException ex = null;
            IXmlLineInfo xmlLineInfo = _reader as IXmlLineInfo;
            if (xmlLineInfo != null)
                if (xmlLineInfo.HasLineInfo())
                    ex = new XmlException
                            xmlLineInfo.LineNumber,
                            xmlLineInfo.LinePosition
            return ex ?? new XmlException(message, innerException);
        private string ReadNameAttribute()
            string encodedName = _reader.GetAttribute(SerializationStrings.NameAttribute);
            if (encodedName == null)
                throw NewXmlException(Serialization.AttributeExpected, null, SerializationStrings.NameAttribute);
            return DecodeString(encodedName);
        private static string DecodeString(string s)
            return XmlConvert.DecodeName(s);
        [TraceSource("InternalDeserializer", "InternalDeserializer class")]
        private static readonly PSTraceSource s_trace = PSTraceSource.GetTracer("InternalDeserializer", "InternalDeserializer class");
    /// Helper class for generating reference id.
    internal class ReferenceIdHandlerForSerializer<T> where T : class
        /// Get new reference id.
        /// <returns>New reference id.</returns>
        private UInt64 GetNewReferenceId()
            UInt64 refId = _seed++;
            return refId;
        /// Seed is incremented by one after each reference generation.
        private UInt64 _seed;
        // note:
        // any boxed UInt64 takes 16 bytes on the heap
        // one-character string (i.e. "7") takes 20 bytes on the heap
        private readonly IDictionary<T, UInt64> _object2refId;
        internal ReferenceIdHandlerForSerializer(IDictionary<T, UInt64> dictionary)
            _object2refId = dictionary;
        /// Assigns a RefId to the given object.
        /// <param name="t">Object to assign a RefId to.</param>
        /// <returns>RefId assigned to the object.</returns>
        internal string SetRefId(T t)
            if (_object2refId != null)
                Dbg.Assert(!_object2refId.ContainsKey(t), "SetRefId shouldn't be called when the object is already assigned a ref id");
                UInt64 refId = GetNewReferenceId();
                _object2refId.Add(t, refId);
                return refId.ToString(System.Globalization.CultureInfo.InvariantCulture);
        /// Gets a RefId already assigned for the given object or <see langword="null"/> if there is no associated ref id.
        /// <param name="t"></param>
        internal string GetRefId(T t)
            UInt64 refId;
            if ((_object2refId != null) && (_object2refId.TryGetValue(t, out refId)))
    internal class ReferenceIdHandlerForDeserializer<T> where T : class
        private readonly Dictionary<string, T> _refId2object = new Dictionary<string, T>();
        internal void SetRefId(T o, string refId, bool duplicateRefIdsAllowed)
            if (!duplicateRefIdsAllowed)
                Dbg.Assert(!_refId2object.ContainsKey(refId), "You can't change refId association");
            _refId2object[refId] = o;
        internal T GetReferencedObject(string refId)
            Dbg.Assert(_refId2object.ContainsKey(refId), "Reference id wasn't seen earlier");
            if (_refId2object.TryGetValue(refId, out t))
    /// A delegate for serializing known type.
    internal delegate void TypeSerializerDelegate(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry);
    /// A delegate for deserializing known type.
    internal delegate object TypeDeserializerDelegate(InternalDeserializer deserializer);
    /// This class contains serialization information about a type.
    internal class TypeSerializationInfo
        /// <param name="type">Type for which this entry is created.</param>
        /// <param name="itemTag">ItemTag for the type.</param>
        /// <param name="propertyTag">PropertyTag for the type.</param>
        /// <param name="serializer">TypeSerializerDelegate for serializing the type.</param>
        /// <param name="deserializer">TypeDeserializerDelegate for deserializing the type.</param>
        internal TypeSerializationInfo(Type type, string itemTag, string propertyTag, TypeSerializerDelegate serializer, TypeDeserializerDelegate deserializer)
            Type = type;
            Serializer = serializer;
            Deserializer = deserializer;
            ItemTag = itemTag;
            PropertyTag = propertyTag;
        /// Get the type for which this TypeSerializationInfo is created.
        /// Get the item tag for this type.
        internal string ItemTag { get; }
        /// Get the Property tag for this type.
        internal string PropertyTag { get; }
        /// Gets the delegate to serialize this type.
        internal TypeSerializerDelegate Serializer { get; }
        /// Gets the delegate to deserialize this type.
        internal TypeDeserializerDelegate Deserializer { get; }
    /// A class for identifying types which are treated as KnownType by Monad.
    /// A KnownType is guaranteed to be available on machine on which monad is
    internal static class KnownTypes
        /// Static constructor.
        static KnownTypes()
            for (int i = 0; i < s_typeSerializationInfo.Length; i++)
                s_knownTableKeyType.Add(s_typeSerializationInfo[i].Type.FullName, s_typeSerializationInfo[i]);
                s_knownTableKeyItemTag.Add(s_typeSerializationInfo[i].ItemTag, s_typeSerializationInfo[i]);
        /// Gets the type serialization information about a type.
        /// <param name="type">Type for which information is retrieved.</param>
        /// <returns>TypeSerializationInfo for the type, null if it doesn't exist.</returns>
        internal static TypeSerializationInfo GetTypeSerializationInfo(Type type)
            TypeSerializationInfo temp;
            if (!s_knownTableKeyType.TryGetValue(type.FullName, out temp) && typeof(XmlDocument).IsAssignableFrom(type))
                temp = s_xdInfo;
        /// Get TypeSerializationInfo using ItemTag as key.
        /// <param name="itemTag">ItemTag for which TypeSerializationInfo is to be fetched.</param>
        /// <returns>TypeSerializationInfo entry, null if no entry exist for the tag.</returns>
        internal static TypeSerializationInfo GetTypeSerializationInfoFromItemTag(string itemTag)
            s_knownTableKeyItemTag.TryGetValue(itemTag, out temp);
        #region private_fields
        // TypeSerializationInfo for XmlDocument
        private static readonly TypeSerializationInfo s_xdInfo =
            new TypeSerializationInfo(typeof(XmlDocument),
                SerializationStrings.XmlDocumentTag,
                InternalSerializer.WriteXmlDocument,
                InternalDeserializer.DeserializeXmlDocument);
        /// Array of known types.
        private static readonly TypeSerializationInfo[] s_typeSerializationInfo = new TypeSerializationInfo[]
            new TypeSerializationInfo(typeof(bool),
                                SerializationStrings.BooleanTag,
                                InternalSerializer.WriteBoolean,
                                InternalDeserializer.DeserializeBoolean),
            new TypeSerializationInfo(typeof(byte),
                                SerializationStrings.UnsignedByteTag,
                                InternalDeserializer.DeserializeByte),
            new TypeSerializationInfo(typeof(char),
                                SerializationStrings.CharTag,
                                InternalSerializer.WriteChar,
                                InternalDeserializer.DeserializeChar),
            new TypeSerializationInfo(typeof(DateTime),
                                SerializationStrings.DateTimeTag,
                                InternalSerializer.WriteDateTime,
                                InternalDeserializer.DeserializeDateTime),
            new TypeSerializationInfo(typeof(decimal),
                                SerializationStrings.DecimalTag,
                                InternalDeserializer.DeserializeDecimal),
            new TypeSerializationInfo(typeof(double),
                                SerializationStrings.DoubleTag,
                                InternalSerializer.WriteDouble,
                                InternalDeserializer.DeserializeDouble),
            new TypeSerializationInfo(typeof(Guid),
                                SerializationStrings.GuidTag,
                                InternalDeserializer.DeserializeGuid),
            new TypeSerializationInfo(typeof(Int16),
                                SerializationStrings.ShortTag,
                                InternalDeserializer.DeserializeInt16),
            new TypeSerializationInfo(typeof(Int32),
                                SerializationStrings.IntTag,
                                InternalDeserializer.DeserializeInt32),
            new TypeSerializationInfo(typeof(Int64),
                                SerializationStrings.LongTag,
                                InternalDeserializer.DeserializeInt64),
            new TypeSerializationInfo(typeof(sbyte),
                                SerializationStrings.ByteTag,
                                InternalDeserializer.DeserializeSByte),
            new TypeSerializationInfo(typeof(Single),
                                SerializationStrings.FloatTag,
                                InternalSerializer.WriteSingle,
                                InternalDeserializer.DeserializeSingle),
            new TypeSerializationInfo(typeof(ScriptBlock),
                                SerializationStrings.ScriptBlockTag,
                                InternalSerializer.WriteScriptBlock,
                                InternalDeserializer.DeserializeScriptBlock),
            new TypeSerializationInfo(typeof(string),
                                SerializationStrings.StringTag,
                                InternalSerializer.WriteEncodedString,
                                InternalDeserializer.DeserializeString),
            new TypeSerializationInfo(typeof(TimeSpan),
                                SerializationStrings.DurationTag,
                                InternalSerializer.WriteTimeSpan,
                                InternalDeserializer.DeserializeTimeSpan),
            new TypeSerializationInfo(typeof(UInt16),
                                SerializationStrings.UnsignedShortTag,
                                InternalDeserializer.DeserializeUInt16),
            new TypeSerializationInfo(typeof(UInt32),
                                SerializationStrings.UnsignedIntTag,
                                InternalDeserializer.DeserializeUInt32),
            new TypeSerializationInfo(typeof(UInt64),
                                SerializationStrings.UnsignedLongTag,
                                InternalDeserializer.DeserializeUInt64),
            new TypeSerializationInfo(typeof(Uri),
                                SerializationStrings.AnyUriTag,
                                InternalSerializer.WriteUri,
                                InternalDeserializer.DeserializeUri),
            new TypeSerializationInfo(typeof(byte[]),
                                      SerializationStrings.Base64BinaryTag,
                                      InternalSerializer.WriteByteArray,
                                      InternalDeserializer.DeserializeByteArray),
            new TypeSerializationInfo(typeof(System.Version),
                                      SerializationStrings.VersionTag,
                                      InternalSerializer.WriteVersion,
                                      InternalDeserializer.DeserializeVersion),
            s_xdInfo,
            new TypeSerializationInfo(typeof(ProgressRecord),
                                      SerializationStrings.ProgressRecord,
                                      InternalSerializer.WriteProgressRecord,
                                      InternalDeserializer.DeserializeProgressRecord),
            new TypeSerializationInfo(typeof(SecureString),
                                      SerializationStrings.SecureStringTag,
                                      InternalSerializer.WriteSecureString,
                                      InternalDeserializer.DeserializeSecureString),
        /// Hashtable of knowntypes.
        /// Key is Type.FullName and value is Type object.
        private static readonly Dictionary<string, TypeSerializationInfo> s_knownTableKeyType = new Dictionary<string, TypeSerializationInfo>();
        /// Hashtable of knowntypes. Key is ItemTag.
        private static readonly Dictionary<string, TypeSerializationInfo> s_knownTableKeyItemTag = new Dictionary<string, TypeSerializationInfo>();
        #endregion private_fields
    /// This class contains helper routined for serialization/deserialization.
    internal static class SerializationUtilities
        /// Extracts the value of a note property from a PSObject; returns null if the property does not exist.
        internal static object GetPropertyValue(PSObject psObject, string propertyName)
            PSNoteProperty property = (PSNoteProperty)psObject.Properties[propertyName];
        /// Returns the BaseObject of a note property encoded as a PSObject; returns null if the property does not exist.
        internal static object GetPsObjectPropertyBaseObject(PSObject psObject, string propertyName)
            PSObject propertyPsObject = (PSObject)GetPropertyValue(psObject, propertyName);
            if (propertyPsObject == null)
            return propertyPsObject.BaseObject;
        /// Checks if source is known container type and returns appropriate
        internal static void GetKnownContainerTypeInfo(
            out ContainerType ct,
            out IDictionary dictionary,
            out IEnumerable enumerable)
            else if (source is Stack)
                if (gt.IsGenericType)
            // Check if LanguagePrimitive.GetEnumerable can do some magic to get IEnumerable instance
                    PSEtwLog.LogAnalyticWarning(PSEventId.Serializer_EnumerationFailed, PSOpcode.Exception,
                        PSTask.Serialization, PSKeyword.Serializer, source.GetType().AssemblyQualifiedName,
            // (LanguagePrimitives.GetEnumerable above should be enough - the check below is to preserve
            // backcompatibility in some corner-cases (see bugs in Windows7 - #372562 and #372563))
                enumerable = source as IEnumerable;
                    // WinBlue: 206515 - There are no elements in the source. The source is of type XmlLinkedNode (which derives from XmlNode which implements IEnumerable).
                    // So, adding an additional check to see if this contains any elements
                    IEnumerator enumerator = enumerable.GetEnumerator();
                    if (enumerator != null && enumerator.MoveNext())
                if (derived.IsGenericType)
                derived = derived.BaseType;
        /// Gets the "ToString" from PSObject.
        /// "ToString" value
        internal static string GetToString(object source)
                result = Convert.ToString(source, CultureInfo.InvariantCulture);
                    e.ToString());
        internal static string GetToStringForPrimitiveObject(PSObject pso)
            // if object is not wrapped in a PSObject, then nothing modifies the ToString value of the primitive object
            // preserve ToString throughout deserialization/*re*serialization
            if (pso.ToStringFromDeserialization != null)
                return pso.ToStringFromDeserialization;
            // preserve token text (i.e. double: 0E1517567410;  see Windows 7 bug #694057 for more details)
            string token = pso.TokenText;
                string originalToString = GetToString(pso.BaseObject);
                if (originalToString == null || !string.Equals(token, originalToString, StringComparison.Ordinal))
            // no need to write <ToString> element otherwise - the ToString method of a deserialized, live primitive object will return the right value
        internal static PSMemberInfoInternalCollection<PSPropertyInfo> GetSpecificPropertiesToSerialize(PSObject source, Collection<CollectionEntry<PSPropertyInfo>> allPropertiesCollection, TypeTable typeTable)
            if (source.GetSerializationMethod(typeTable) == SerializationMethod.SpecificProperties)
                    (UInt32)(SerializationMethod.SpecificProperties));
                PSMemberInfoInternalCollection<PSPropertyInfo> specificProperties =
                    new PSMemberInfoInternalCollection<PSPropertyInfo>();
                PSMemberInfoIntegratingCollection<PSPropertyInfo> allProperties =
                        allPropertiesCollection);
                Collection<string> namesOfPropertiesToSerialize = source.GetSpecificPropertiesToSerialize(typeTable);
                foreach (string propertyName in namesOfPropertiesToSerialize)
                    PSPropertyInfo property = allProperties[propertyName];
                            PSEventId.Serializer_SpecificPropertyMissing, PSOpcode.Exception, PSTask.Serialization,
                            propertyName);
                return specificProperties;
        internal static object GetPropertyValueInThreadSafeManner(PSPropertyInfo property, bool canUseDefaultRunspaceInThreadSafeManner, out bool success)
            Dbg.Assert(property != null, "Caller should validate the parameter");
            if (!property.IsGettable)
            PSAliasProperty alias = property as PSAliasProperty;
                property = alias.ReferencedMember as PSPropertyInfo;
            PSScriptProperty script = property as PSScriptProperty;
            Dbg.Assert(script == null || script.GetterScript != null, "scriptProperty.IsGettable => (scriptProperty.GetterScript != null)");
            if ((script != null) && (!canUseDefaultRunspaceInThreadSafeManner))
                    PSEventId.Serializer_ScriptPropertyWithoutRunspace, PSOpcode.Exception, PSTask.Serialization,
                    property.instance == null ? string.Empty : PSObject.GetTypeNames(property.instance).Key,
                    script.GetterScript.ToString());
                object value = property.Value;
                    PSEventId.Serializer_PropertyGetterFailed, PSOpcode.Exception, PSTask.Serialization,
                    e.ToString(),
                    e.InnerException == null ? string.Empty : e.InnerException.ToString());
    /// A dictionary from object to T where
    /// 1) keys are objects,
    /// 2) keys use reference equality,
    /// 3) dictionary keeps only weak references to keys.
    /// <typeparam name="T">type of dictionary values</typeparam>
    internal class WeakReferenceDictionary<T> : IDictionary<object, T>
        private sealed class WeakReferenceEqualityComparer : IEqualityComparer<WeakReference>
            public bool Equals(WeakReference x, WeakReference y)
                object tx = x.Target;
                if (tx == null)
                    return false; // collected object is not equal to anything (object.ReferenceEquals(null, null) == true)
                object ty = y.Target;
                if (ty == null)
                return object.ReferenceEquals(tx, ty);
            public int GetHashCode(WeakReference obj)
                object t = obj.Target;
                if (t == null)
                    // collected object doesn't have a hash code
                    // return an arbitrary hashcode here and fall back on Equal method for comparison
                    return RuntimeHelpers.GetHashCode(obj); // RuntimeHelpers.GetHashCode(null) returns 0 - this would cause many hashtable collisions for WeakReferences to dead objects
                    return RuntimeHelpers.GetHashCode(t);
        private readonly IEqualityComparer<WeakReference> _weakEqualityComparer;
        private Dictionary<WeakReference, T> _dictionary;
        public WeakReferenceDictionary()
            _weakEqualityComparer = new WeakReferenceEqualityComparer();
            _dictionary = new Dictionary<WeakReference, T>(_weakEqualityComparer);
        private const int initialCleanupTriggerSize = 2; // 2 will stress this code more
        private const int initialCleanupTriggerSize = 1000;
        private int _cleanupTriggerSize = initialCleanupTriggerSize;
            if (this.Count > _cleanupTriggerSize)
                Dictionary<WeakReference, T> alive = new Dictionary<WeakReference, T>(_weakEqualityComparer);
                foreach (KeyValuePair<WeakReference, T> weakKeyValuePair in _dictionary)
                    object key = weakKeyValuePair.Key.Target;
                        alive.Add(weakKeyValuePair.Key, weakKeyValuePair.Value);
                _dictionary = alive;
                _cleanupTriggerSize = initialCleanupTriggerSize + this.Count * 2;
        #region IDictionary<object,T> Members
        public void Add(object key, T value)
            _dictionary.Add(new WeakReference(key), value);
            this.CleanUp();
        public bool ContainsKey(object key)
            return _dictionary.ContainsKey(new WeakReference(key));
        public ICollection<object> Keys
                List<object> keys = new List<object>(_dictionary.Keys.Count);
                foreach (WeakReference weakKey in _dictionary.Keys)
                    object key = weakKey.Target;
                        keys.Add(key);
                return keys;
        public bool Remove(object key)
            return _dictionary.Remove(new WeakReference(key));
        public bool TryGetValue(object key, out T value)
            WeakReference weakKey = new WeakReference(key);
            return _dictionary.TryGetValue(weakKey, out value);
        public ICollection<T> Values
                return _dictionary.Values;
        public T this[object key]
                return _dictionary[new WeakReference(key)];
                _dictionary[new WeakReference(key)] = value;
        #region ICollection<KeyValuePair<object,T>> Members
        private ICollection<KeyValuePair<WeakReference, T>> WeakCollection
        private static KeyValuePair<WeakReference, T> WeakKeyValuePair(KeyValuePair<object, T> publicKeyValuePair)
            return new KeyValuePair<WeakReference, T>(new WeakReference(publicKeyValuePair.Key), publicKeyValuePair.Value);
        public void Add(KeyValuePair<object, T> item)
            this.WeakCollection.Add(WeakKeyValuePair(item));
            this.WeakCollection.Clear();
        public bool Contains(KeyValuePair<object, T> item)
            return this.WeakCollection.Contains(WeakKeyValuePair(item));
        public void CopyTo(KeyValuePair<object, T>[] array, int arrayIndex)
            List<KeyValuePair<object, T>> rawList = new List<KeyValuePair<object, T>>(this.WeakCollection.Count);
            foreach (KeyValuePair<object, T> keyValuePair in this)
                rawList.Add(keyValuePair);
            rawList.CopyTo(array, arrayIndex);
                return this.WeakCollection.Count;
        public bool IsReadOnly
                return this.WeakCollection.IsReadOnly;
        public bool Remove(KeyValuePair<object, T> item)
            return this.WeakCollection.Remove(WeakKeyValuePair(item));
        #region IEnumerable<KeyValuePair<object,T>> Members
        public IEnumerator<KeyValuePair<object, T>> GetEnumerator()
            foreach (KeyValuePair<WeakReference, T> weakKeyValuePair in this.WeakCollection)
                    yield return new KeyValuePair<object, T>(key, weakKeyValuePair.Value);
            IEnumerable<KeyValuePair<object, T>> enumerable = this;
            IEnumerator<KeyValuePair<object, T>> enumerator = enumerable.GetEnumerator();
            return enumerator;
    /// <see cref="PSPrimitiveDictionary"/> is a <see cref="Hashtable"/> that is limited to
    /// 1) case-insensitive strings as keys and
    /// 2) values that can be serialized and deserialized during PowerShell remoting handshake
    ///    (in major-version compatible versions of PowerShell remoting)
    public sealed class PSPrimitiveDictionary : Hashtable
        /// Initializes a new empty instance of the <see cref="PSPrimitiveDictionary"/> class.
        public PSPrimitiveDictionary()
        /// Initializes a new instance of the <see cref="PSPrimitiveDictionary"/> class with contents
        /// copied from the <paramref name="other"/> hashtable.
        /// <param name="other">Hashtable to copy into the new instance of <see cref="PSPrimitiveDictionary"/></param>
        /// This constructor will throw if the <paramref name="other"/> hashtable contains keys that are not a strings
        /// or values that are not one of primitive types that will work during PowerShell remoting handshake.
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "The class is sealed")]
        public PSPrimitiveDictionary(Hashtable other)
            foreach (DictionaryEntry entry in other)
                Hashtable valueAsHashtable = PSObject.Base(entry.Value) as Hashtable;
                if (valueAsHashtable != null)
                    this.Add(entry.Key, new PSPrimitiveDictionary(valueAsHashtable));
                    this.Add(entry.Key, entry.Value);
#if !CORECLR // No .NET Serialization In CoreCLR
        /// Support for .NET serialization.
        private PSPrimitiveDictionary(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        #region Plumbing to make Hashtable reject all non-primitive types
        private static string VerifyKey(object key)
            key = PSObject.Base(key);
            string keyAsString = key as string;
            if (keyAsString == null)
                string message = StringUtil.Format(Serialization.PrimitiveHashtableInvalidKey,
                    key.GetType().FullName);
                throw new ArgumentException(message);
                return keyAsString;
        private static readonly Type[] s_handshakeFriendlyTypes = new Type[] {
                typeof(Guid),
                // typeof(ScriptBlock) - don't want ScriptBlocks, because they are deserialized into strings
                typeof(Uri),
                typeof(byte[]),
                typeof(Version),
                typeof(ProgressRecord),
                typeof(XmlDocument),
                typeof(PSPrimitiveDictionary)
        private static void VerifyValue(object value)
            // null is a primitive type
            value = PSObject.Base(value);
            // this list is based on the list inside KnownTypes
            // it is copied here to make sure that a list of "handshake friendly types"
            // won't change even if we add new primitive types in v.next
            // if "value" is a "primitiveType" then we are good
            foreach (Type goodType in s_handshakeFriendlyTypes)
                if (valueType == goodType)
            // if "value" is an array of "primitiveType" items then we are good
            // (note: we could have used IEnumerable<> or ICollection<> (covariance/contravariance concerns),
            //        but it is safer to limit the types to arrays.
            //        (one concern is that in v.next we might allow overriding SerializationMethod for
            //         types [i.e. check SerializationMethod *before* HandleKnownContainerTypes)
            if ((valueType.IsArray) || valueType == typeof(ArrayList))
                foreach (object o in (IEnumerable)value)
                    VerifyValue(o);
            string message = StringUtil.Format(Serialization.PrimitiveHashtableInvalidValue,
                value.GetType().FullName);
        /// Adds an element with the specified key and value into the Hashtable.
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// This method will throw if the <paramref name="key"/> is not a string and the <paramref name="value"/>
        /// is not one of primitive types that will work during PowerShell remoting handshake.
        /// Use of strongly-typed overloads of this method is suggested if throwing an exception is not acceptable.
            string keyAsString = VerifyKey(key);
            VerifyValue(value);
            base.Add(keyAsString, value);
        /// <param name="key">The key whose value to get or set.</param>
        /// <returns>The value associated with the specified key.</returns>
        /// If the specified key is not found, attempting to get it returns <see langword="null"/>
        /// and attempting to set it creates a new element using the specified key.
        /// The setter will throw if the <paramref name="key"/> is not a string and the value
        /// Use of strongly-typed overloads of Add method is suggested if throwing an exception is not acceptable.
                return base[key];
                base[keyAsString] = value;
        /// The setter will throw if the value
        public object this[string key]
                base[key] = value;
        /// Creates a new instance by doing a shallow copy of the current instance.
            return new PSPrimitiveDictionary(this);
        public void Add(string key, bool value)
            this.Add((object)key, (object)value);
        public void Add(string key, bool[] value)
        public void Add(string key, byte value)
        public void Add(string key, byte[] value)
        public void Add(string key, char value)
        public void Add(string key, char[] value)
        public void Add(string key, DateTime value)
        public void Add(string key, DateTime[] value)
        public void Add(string key, Decimal value)
        public void Add(string key, Decimal[] value)
        public void Add(string key, double value)
        public void Add(string key, double[] value)
        public void Add(string key, Guid value)
        public void Add(string key, Guid[] value)
        public void Add(string key, Int32 value)
        public void Add(string key, Int32[] value)
        public void Add(string key, Int64 value)
        public void Add(string key, Int64[] value)
        public void Add(string key, sbyte value)
        public void Add(string key, sbyte[] value)
        public void Add(string key, Single value)
        public void Add(string key, Single[] value)
        public void Add(string key, string value)
        public void Add(string key, string[] value)
        public void Add(string key, TimeSpan value)
        public void Add(string key, TimeSpan[] value)
        public void Add(string key, UInt16 value)
        public void Add(string key, UInt16[] value)
        public void Add(string key, UInt32 value)
        public void Add(string key, UInt32[] value)
        public void Add(string key, UInt64 value)
        public void Add(string key, UInt64[] value)
        public void Add(string key, Uri value)
        public void Add(string key, Uri[] value)
        public void Add(string key, Version value)
        public void Add(string key, Version[] value)
        public void Add(string key, PSPrimitiveDictionary value)
        public void Add(string key, PSPrimitiveDictionary[] value)
        /// If originalHash contains PSVersionTable, then just returns the Cloned copy of
        /// the original hash. Otherwise, creates a clone copy and add PSVersionInfo.GetPSVersionTable
        /// to the clone and returns.
        /// <param name="originalHash"></param>
        internal static PSPrimitiveDictionary CloneAndAddPSVersionTable(PSPrimitiveDictionary originalHash)
            if ((originalHash != null) &&
                (originalHash.ContainsKey(PSVersionInfo.PSVersionTableName)))
                return (PSPrimitiveDictionary)originalHash.Clone();
            PSPrimitiveDictionary result = originalHash;
            if (originalHash != null)
                result = (PSPrimitiveDictionary)originalHash.Clone();
                result = new PSPrimitiveDictionary();
            PSPrimitiveDictionary versionTable = new PSPrimitiveDictionary(PSVersionInfo.GetPSVersionTableForDownLevel())
                {"PSSemanticVersion", PSVersionInfo.PSVersion.ToString()}
            result.Add(PSVersionInfo.PSVersionTableName, versionTable);
        /// Tries to get a value that might be present in a chain of nested PSPrimitiveDictionaries.
        /// For example to get $sessionInfo.ApplicationPrivateData.ImplicitRemoting.Hash you could call
        /// TryPathGet&lt;string&gt;($sessionInfo.ApplicationPrivateData, out myHash, "ImplicitRemoting", "Hash").
        /// <typeparam name="T">Expected type of the value</typeparam>
        /// <param name="data">The root dictionary.</param>
        /// <param name="keys">A chain of keys leading from the root dictionary (<paramref name="data"/>) to the value.</param>
        /// <returns><see langword="true"/> if the value was found and was of the correct type; <see langword="false"/> otherwise.</returns>
        internal static bool TryPathGet<T>(IDictionary data, out T result, params string[] keys)
            Dbg.Assert(keys != null, "Caller should verify that keys != null");
            Dbg.Assert(keys.Length >= 1, "Caller should verify that keys.Length >= 1");
            Dbg.Assert(keys[0] != null, "Caller should verify that keys[i] != null");
            if (data == null || !data.Contains(keys[0]))
            if (keys.Length == 1)
                return LanguagePrimitives.TryConvertTo<T>(data[keys[0]], out result);
                IDictionary subData;
                if (LanguagePrimitives.TryConvertTo<IDictionary>(data[keys[0]], out subData)
                    && subData != null)
                    string[] subKeys = new string[keys.Length - 1];
                    Array.Copy(keys, 1, subKeys, 0, subKeys.Length);
                    return TryPathGet<T>(subData, out result, subKeys);
    /// Rehydrating type converter used during deserialization.
    /// It takes results of serializing some common types
    /// and rehydrates them back from property bags into live objects.
    /// To add a new type for rehydration:
    /// - Add a new T RehydrateT(PSObject pso) method below
    /// - Add this method to converters dictionary in the static constructor below
    /// - If implicit rehydration is required then
    ///   - Add appropriate types.ps1 xml entries for
    ///     - SerializationDepth=X
    ///     - For types depending only on ToString for rehydration set
    ///       - SerializationMethod=SpecificProperties
    ///       - PropertySerializationSet=<empty>
    ///     - TargetTypeForDeserialization=DeserializingTypeConverter
    ///   - Add a field of that type in unit tests / S.M.A.Test.SerializationTest+RehydratedType
    public sealed class DeserializingTypeConverter : PSTypeConverter
        #region Infrastructure
        private static readonly Dictionary<Type, Func<PSObject, object>> s_converter;
        static DeserializingTypeConverter()
            s_converter = new Dictionary<Type, Func<PSObject, object>>();
            s_converter.Add(typeof(PSPrimitiveDictionary), RehydratePrimitiveHashtable);
            s_converter.Add(typeof(SwitchParameter), RehydrateSwitchParameter);
            s_converter.Add(typeof(PSListModifier), RehydratePSListModifier);
            s_converter.Add(typeof(PSCredential), RehydratePSCredential);
            s_converter.Add(typeof(PSSenderInfo), RehydratePSSenderInfo);
            s_converter.Add(typeof(CultureInfo), RehydrateCultureInfo);
            s_converter.Add(typeof(ParameterSetMetadata), RehydrateParameterSetMetadata);
            s_converter.Add(typeof(System.Security.Cryptography.X509Certificates.X509Certificate2), RehydrateX509Certificate2);
            s_converter.Add(typeof(System.Security.Cryptography.X509Certificates.X500DistinguishedName), RehydrateX500DistinguishedName);
            s_converter.Add(typeof(System.Net.IPAddress), RehydrateIPAddress);
            s_converter.Add(typeof(MailAddress), RehydrateMailAddress);
            s_converter.Add(typeof(System.Security.AccessControl.DirectorySecurity), RehydrateObjectSecurity<System.Security.AccessControl.DirectorySecurity>);
            s_converter.Add(typeof(System.Security.AccessControl.FileSecurity), RehydrateObjectSecurity<System.Security.AccessControl.FileSecurity>);
            s_converter.Add(typeof(System.Security.AccessControl.RegistrySecurity), RehydrateObjectSecurity<System.Security.AccessControl.RegistrySecurity>);
            s_converter.Add(typeof(ExtendedTypeDefinition), RehydrateExtendedTypeDefinition);
            s_converter.Add(typeof(FormatViewDefinition), RehydrateFormatViewDefinition);
            s_converter.Add(typeof(PSControl), RehydratePSControl);
            s_converter.Add(typeof(PSControlGroupBy), RehydrateGroupBy);
            s_converter.Add(typeof(DisplayEntry), RehydrateDisplayEntry);
            s_converter.Add(typeof(EntrySelectedBy), RehydrateEntrySelectedBy);
            s_converter.Add(typeof(TableControlColumnHeader), RehydrateTableControlColumnHeader);
            s_converter.Add(typeof(TableControlRow), RehydrateTableControlRow);
            s_converter.Add(typeof(TableControlColumn), RehydrateTableControlColumn);
            s_converter.Add(typeof(ListControlEntry), RehydrateListControlEntry);
            s_converter.Add(typeof(ListControlEntryItem), RehydrateListControlEntryItem);
            s_converter.Add(typeof(WideControlEntryItem), RehydrateWideControlEntryItem);
            s_converter.Add(typeof(CustomControlEntry), RehydrateCustomControlEntry);
            s_converter.Add(typeof(CustomItemBase), RehydrateCustomItemBase);
            s_converter.Add(typeof(CompletionResult), RehydrateCompletionResult);
            s_converter.Add(typeof(ModuleSpecification), RehydrateModuleSpecification);
            s_converter.Add(typeof(CommandCompletion), RehydrateCommandCompletion);
            s_converter.Add(typeof(JobStateInfo), RehydrateJobStateInfo);
            s_converter.Add(typeof(JobStateEventArgs), RehydrateJobStateEventArgs);
            s_converter.Add(typeof(PSSessionOption), RehydratePSSessionOption);
            s_converter.Add(typeof(LineBreakpoint), RehydrateLineBreakpoint);
            s_converter.Add(typeof(CommandBreakpoint), RehydrateCommandBreakpoint);
            s_converter.Add(typeof(VariableBreakpoint), RehydrateVariableBreakpoint);
            s_converter.Add(typeof(BreakpointUpdatedEventArgs), RehydrateBreakpointUpdatedEventArgs);
            s_converter.Add(typeof(DebuggerCommand), RehydrateDebuggerCommand);
            s_converter.Add(typeof(DebuggerCommandResults), RehydrateDebuggerCommandResults);
            s_converter.Add(typeof(DebuggerStopEventArgs), RehydrateDebuggerStopEventArgs);
        /// <param name="destinationType">The type to convert to.</param>
        public override bool CanConvertFrom(PSObject sourceValue, Type destinationType)
            foreach (Type type in s_converter.Keys)
                if (Deserializer.IsDeserializedInstanceOfType(sourceValue, type))
        public override object ConvertFrom(PSObject sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase)
            if (destinationType == null)
                throw PSTraceSource.NewArgumentNullException(nameof(destinationType));
                    "InvalidCastWhenRehydratingFromNull",
                    PSTraceSource.NewArgumentNullException(nameof(sourceValue)),
                    ExtendedTypeSystem.InvalidCastFromNull,
                    destinationType.ToString());
            foreach (KeyValuePair<Type, Func<PSObject, object>> item in s_converter)
                Type type = item.Key;
                Func<PSObject, object> typeConverter = item.Value;
                    return ConvertFrom(sourceValue, typeConverter);
                "InvalidCastEnumFromTypeNotAString",
                destinationType);
        private static object ConvertFrom(PSObject o, Func<PSObject, object> converter)
            // rehydrate
            PSObject dso = o;
            object rehydratedObject = converter(dso);
            // re-add instance properties
            // (dso.InstanceMembers includes both instance and *type table* properties;
            //  therefore this will also re-add type table properties if they are not present when the deserializer runs;
            //  this is ok)
            bool returnPSObject = false;
            PSObject rehydratedPSObject = PSObject.AsPSObject(rehydratedObject);
            foreach (PSMemberInfo member in dso.InstanceMembers)
                if (member.MemberType == (member.MemberType & (PSMemberTypes.Properties | PSMemberTypes.MemberSet | PSMemberTypes.PropertySet)))
                    if (rehydratedPSObject.Members[member.Name] == null)
                        rehydratedPSObject.InstanceMembers.Add(member);
                        returnPSObject = true;
            if (returnPSObject)
                return rehydratedPSObject;
                return rehydratedObject;
        /// This method is not implemented - an overload taking a PSObject is implemented instead.
        public override bool CanConvertTo(PSObject sourceValue, Type destinationType)
        public override object ConvertTo(PSObject sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase)
        #region Rehydration helpers
        internal enum RehydrationFlags
            NullValueBad = 0,
            NullValueOk = 0x1,
            NullValueMeansEmptyList = 0x3,
            MissingPropertyBad = 0,
            MissingPropertyOk = 0x4,
        /// Gets value of a property (has to be present, value has to be non-null).
        /// Can throw any exception (which is ok - LanguagePrimitives.ConvertTo will catch that).
        /// <typeparam name="T">Expected type of the property</typeparam>
        /// <param name="pso">Deserialized object.</param>
        private static T GetPropertyValue<T>(PSObject pso, string propertyName)
            return GetPropertyValue<T>(pso, propertyName, RehydrationFlags.NullValueBad | RehydrationFlags.MissingPropertyBad);
        /// Gets value of a property.  Can throw any exception (which is ok - LanguagePrimitives.ConvertTo will catch that).
        internal static T GetPropertyValue<T>(PSObject pso, string propertyName, RehydrationFlags flags)
            Dbg.Assert(pso != null, "Caller should verify pso != null");
            Dbg.Assert(!string.IsNullOrEmpty(propertyName), "Caller should verify propertyName != null");
            if ((property == null) && ((flags & RehydrationFlags.MissingPropertyOk) == RehydrationFlags.MissingPropertyOk))
                object propertyValue = property.Value;
                if ((propertyValue == null) && ((flags & RehydrationFlags.NullValueOk) == RehydrationFlags.NullValueOk))
                    T t = (T)LanguagePrimitives.ConvertTo(propertyValue, typeof(T), CultureInfo.InvariantCulture);
        private static TList RehydrateList<TList, TItem>(PSObject pso, string propertyName, RehydrationFlags flags)
            where TList : IList, new()
            ArrayList deserializedList = GetPropertyValue<ArrayList>(pso, propertyName, flags);
            if (deserializedList == null)
                if ((flags & RehydrationFlags.NullValueMeansEmptyList) == RehydrationFlags.NullValueMeansEmptyList)
                    return new TList();
                    return default(TList);
                TList newList = new TList();
                foreach (object deserializedItem in deserializedList)
                    TItem item = (TItem)LanguagePrimitives.ConvertTo(deserializedItem, typeof(TItem), CultureInfo.InvariantCulture);
                    newList.Add(item);
        #region Rehydration of miscellaneous types
        private static object RehydratePrimitiveHashtable(PSObject pso)
            Hashtable hashtable = (Hashtable)LanguagePrimitives.ConvertTo(pso, typeof(Hashtable), CultureInfo.InvariantCulture);
            return new PSPrimitiveDictionary(hashtable);
        private static object RehydrateSwitchParameter(PSObject pso)
            return GetPropertyValue<SwitchParameter>(pso, "IsPresent");
        private static CultureInfo RehydrateCultureInfo(PSObject pso)
            string s = pso.ToString();
            return new CultureInfo(s);
        private static PSListModifier RehydratePSListModifier(PSObject pso)
            Hashtable h = new Hashtable();
            PSPropertyInfo addProperty = pso.Properties[PSListModifier.AddKey];
            if ((addProperty != null) && (addProperty.Value != null))
                h.Add(PSListModifier.AddKey, addProperty.Value);
            PSPropertyInfo removeProperty = pso.Properties[PSListModifier.RemoveKey];
            if ((removeProperty != null) && (removeProperty.Value != null))
                h.Add(PSListModifier.RemoveKey, removeProperty.Value);
            PSPropertyInfo replaceProperty = pso.Properties[PSListModifier.ReplaceKey];
            if ((replaceProperty != null) && (replaceProperty.Value != null))
                h.Add(PSListModifier.ReplaceKey, replaceProperty.Value);
            return new PSListModifier(h);
        private static CompletionResult RehydrateCompletionResult(PSObject pso)
            string completionText = GetPropertyValue<string>(pso, "CompletionText");
            string listItemText = GetPropertyValue<string>(pso, "ListItemText");
            string toolTip = GetPropertyValue<string>(pso, "ToolTip");
            CompletionResultType resultType = GetPropertyValue<CompletionResultType>(pso, "ResultType");
            return new CompletionResult(completionText, listItemText, resultType, toolTip);
        private static ModuleSpecification RehydrateModuleSpecification(PSObject pso)
            return new ModuleSpecification
                Name = GetPropertyValue<string>(pso, "Name"),
                Guid = GetPropertyValue<Guid?>(pso, "Guid"),
                Version = GetPropertyValue<Version>(pso, "Version"),
                MaximumVersion = GetPropertyValue<string>(pso, "MaximumVersion"),
                RequiredVersion =
                                                    GetPropertyValue<Version>(pso, "RequiredVersion")
        private static CommandCompletion RehydrateCommandCompletion(PSObject pso)
            var completions = new Collection<CompletionResult>();
            foreach (var match in GetPropertyValue<ArrayList>(pso, "CompletionMatches"))
                completions.Add((CompletionResult)match);
            var currentMatchIndex = GetPropertyValue<int>(pso, "CurrentMatchIndex");
            var replacementIndex = GetPropertyValue<int>(pso, "ReplacementIndex");
            var replacementLength = GetPropertyValue<int>(pso, "ReplacementLength");
            return new CommandCompletion(completions, currentMatchIndex, replacementIndex, replacementLength);
        private static JobStateInfo RehydrateJobStateInfo(PSObject pso)
            var jobState = GetPropertyValue<JobState>(pso, "State");
            Exception reason = null;
            PSPropertyInfo property = pso.Properties["Reason"];
                propertyValue = property.Value;
                if (Deserializer.IsDeserializedInstanceOfType(propertyValue, typeof(Exception)))
                    // if we have a deserialized remote or any other exception, use its message to construct
                    // an exception
                    message = PSObject.AsPSObject(propertyValue).Properties["Message"].Value as string;
                else if (propertyValue is Exception)
                    reason = (Exception)propertyValue;
                    message = propertyValue.ToString();
                        reason = (Exception)LanguagePrimitives.ConvertTo(message, typeof(Exception), CultureInfo.InvariantCulture);
                        // it is ok to eat this exception since we do not want
                        // rehydration to fail
            return new JobStateInfo(jobState, reason);
        internal static JobStateEventArgs RehydrateJobStateEventArgs(PSObject pso)
            var jobStateInfo = RehydrateJobStateInfo(PSObject.AsPSObject(pso.Properties["JobStateInfo"].Value));
            JobStateInfo previousJobStateInfo = null;
            var previousJobStateInfoProperty = pso.Properties["PreviousJobStateInfo"];
            if (previousJobStateInfoProperty != null && previousJobStateInfoProperty.Value != null)
                previousJobStateInfo = RehydrateJobStateInfo(PSObject.AsPSObject(previousJobStateInfoProperty.Value));
            return new JobStateEventArgs(jobStateInfo, previousJobStateInfo);
        internal static PSSessionOption RehydratePSSessionOption(PSObject pso)
            PSSessionOption option = new PSSessionOption();
            option.ApplicationArguments = GetPropertyValue<PSPrimitiveDictionary>(pso, "ApplicationArguments");
            option.CancelTimeout = GetPropertyValue<TimeSpan>(pso, "CancelTimeout");
            option.Culture = GetPropertyValue<CultureInfo>(pso, "Culture");
            option.IdleTimeout = GetPropertyValue<TimeSpan>(pso, "IdleTimeout");
            option.MaximumConnectionRedirectionCount = GetPropertyValue<int>(pso, "MaximumConnectionRedirectionCount");
            option.MaximumReceivedDataSizePerCommand = GetPropertyValue<int?>(pso, "MaximumReceivedDataSizePerCommand");
            option.MaximumReceivedObjectSize = GetPropertyValue<int?>(pso, "MaximumReceivedObjectSize");
            option.NoCompression = GetPropertyValue<bool>(pso, "NoCompression");
            option.NoEncryption = GetPropertyValue<bool>(pso, "NoEncryption");
            option.NoMachineProfile = GetPropertyValue<bool>(pso, "NoMachineProfile");
            option.OpenTimeout = GetPropertyValue<TimeSpan>(pso, "OpenTimeout");
            option.OperationTimeout = GetPropertyValue<TimeSpan>(pso, "OperationTimeout");
            option.OutputBufferingMode = GetPropertyValue<OutputBufferingMode>(pso, "OutputBufferingMode");
            option.MaxConnectionRetryCount = GetPropertyValue<int>(pso, "MaxConnectionRetryCount");
            option.ProxyAccessType = GetPropertyValue<ProxyAccessType>(pso, "ProxyAccessType");
            option.ProxyAuthentication = GetPropertyValue<AuthenticationMechanism>(pso, "ProxyAuthentication");
            option.ProxyCredential = GetPropertyValue<PSCredential>(pso, "ProxyCredential");
            option.SkipCACheck = GetPropertyValue<bool>(pso, "SkipCACheck");
            option.SkipCNCheck = GetPropertyValue<bool>(pso, "SkipCNCheck");
            option.SkipRevocationCheck = GetPropertyValue<bool>(pso, "SkipRevocationCheck");
            option.UICulture = GetPropertyValue<CultureInfo>(pso, "UICulture");
            option.UseUTF16 = GetPropertyValue<bool>(pso, "UseUTF16");
            option.IncludePortInSPN = GetPropertyValue<bool>(pso, "IncludePortInSPN");
        internal static LineBreakpoint RehydrateLineBreakpoint(PSObject pso)
            string script = GetPropertyValue<string>(pso, "Script");
            int line = GetPropertyValue<int>(pso, "Line");
            int column = GetPropertyValue<int>(pso, "Column");
            int id = GetPropertyValue<int>(pso, "Id");
            bool enabled = GetPropertyValue<bool>(pso, "Enabled");
            ScriptBlock action = RehydrateScriptBlock(
                GetPropertyValue<string>(pso, "Action", RehydrationFlags.MissingPropertyOk));
            var bp = new LineBreakpoint(script, line, column, action, id);
            bp.SetEnabled(enabled);
        internal static CommandBreakpoint RehydrateCommandBreakpoint(PSObject pso)
            string script = GetPropertyValue<string>(pso, "Script", RehydrationFlags.MissingPropertyOk | RehydrationFlags.NullValueOk);
            string command = GetPropertyValue<string>(pso, "Command");
            WildcardPattern pattern = WildcardPattern.Get(command, WildcardOptions.Compiled | WildcardOptions.IgnoreCase);
            var bp = new CommandBreakpoint(script, pattern, command, action, id);
        internal static VariableBreakpoint RehydrateVariableBreakpoint(PSObject pso)
            string variableName = GetPropertyValue<string>(pso, "Variable");
            VariableAccessMode access = GetPropertyValue<VariableAccessMode>(pso, "AccessMode");
            var bp = new VariableBreakpoint(script, variableName, access, action, id);
        internal static BreakpointUpdatedEventArgs RehydrateBreakpointUpdatedEventArgs(PSObject pso)
            Breakpoint bp = GetPropertyValue<Breakpoint>(pso, "Breakpoint");
            BreakpointUpdateType bpUpdateType = GetPropertyValue<BreakpointUpdateType>(pso, "UpdateType");
            int bpCount = GetPropertyValue<int>(pso, "BreakpointCount");
            return new BreakpointUpdatedEventArgs(bp, bpUpdateType, bpCount);
        internal static DebuggerCommand RehydrateDebuggerCommand(PSObject pso)
            bool repeatOnEnter = GetPropertyValue<bool>(pso, "RepeatOnEnter");
            bool executedByDebugger = GetPropertyValue<bool>(pso, "ExecutedByDebugger");
            DebuggerResumeAction? resumeAction = GetPropertyValue<DebuggerResumeAction?>(pso, "ResumeAction", RehydrationFlags.NullValueOk);
            return new DebuggerCommand(command, resumeAction, repeatOnEnter, executedByDebugger);
        internal static DebuggerCommandResults RehydrateDebuggerCommandResults(PSObject pso)
            bool evaluatedByDebugger = GetPropertyValue<bool>(pso, "EvaluatedByDebugger");
            return new DebuggerCommandResults(resumeAction, evaluatedByDebugger);
        internal static DebuggerStopEventArgs RehydrateDebuggerStopEventArgs(PSObject pso)
            PSObject psoInvocationInfo = GetPropertyValue<PSObject>(pso, "SerializedInvocationInfo", RehydrationFlags.NullValueOk | RehydrationFlags.MissingPropertyOk);
            InvocationInfo invocationInfo = (psoInvocationInfo != null) ? new InvocationInfo(psoInvocationInfo) : null;
            DebuggerResumeAction resumeAction = GetPropertyValue<DebuggerResumeAction>(pso, "ResumeAction");
            Collection<Breakpoint> breakpoints = new Collection<Breakpoint>();
            foreach (var item in GetPropertyValue<ArrayList>(pso, "Breakpoints"))
                Breakpoint bp = item as Breakpoint;
                if (bp != null)
                    breakpoints.Add(bp);
            return new DebuggerStopEventArgs(invocationInfo, breakpoints, resumeAction);
        private static ScriptBlock RehydrateScriptBlock(string script)
            if (!string.IsNullOrEmpty(script))
                return ScriptBlock.Create(script);
        #region Rehydration of security-related types
        private static PSCredential RehydratePSCredential(PSObject pso)
            string userName = GetPropertyValue<string>(pso, "UserName");
            System.Security.SecureString password = GetPropertyValue<System.Security.SecureString>(pso, "Password");
                return PSCredential.Empty;
        internal static PSSenderInfo RehydratePSSenderInfo(PSObject pso)
            PSObject userInfo = GetPropertyValue<PSObject>(pso, "UserInfo");
            PSObject userIdentity = GetPropertyValue<PSObject>(userInfo, "Identity");
            PSObject certDetails = GetPropertyValue<PSObject>(userIdentity, "CertificateDetails");
            PSCertificateDetails psCertDetails = certDetails == null ? null : new PSCertificateDetails(
                GetPropertyValue<string>(certDetails, "Subject"),
                GetPropertyValue<string>(certDetails, "IssuerName"),
                GetPropertyValue<string>(certDetails, "IssuerThumbprint"));
            PSIdentity psIdentity = new PSIdentity(
                GetPropertyValue<string>(userIdentity, "AuthenticationType"),
                GetPropertyValue<bool>(userIdentity, "IsAuthenticated"),
                GetPropertyValue<string>(userIdentity, "Name"),
                psCertDetails);
            PSPrincipal psPrincipal = new PSPrincipal(psIdentity, WindowsIdentity.GetCurrent());
            PSSenderInfo senderInfo = new PSSenderInfo(psPrincipal, GetPropertyValue<string>(pso, "ConnectionString"));
            senderInfo.ApplicationArguments = GetPropertyValue<PSPrimitiveDictionary>(pso, "ApplicationArguments");
            return senderInfo;
        private static System.Security.Cryptography.X509Certificates.X509Certificate2 RehydrateX509Certificate2(PSObject pso)
            byte[] rawData = GetPropertyValue<byte[]>(pso, "RawData");
            return new System.Security.Cryptography.X509Certificates.X509Certificate2(rawData);
        private static System.Security.Cryptography.X509Certificates.X500DistinguishedName RehydrateX500DistinguishedName(PSObject pso)
            return new System.Security.Cryptography.X509Certificates.X500DistinguishedName(rawData);
        private static System.Net.IPAddress RehydrateIPAddress(PSObject pso)
            return System.Net.IPAddress.Parse(s);
        private static MailAddress RehydrateMailAddress(PSObject pso)
            return new MailAddress(s);
        private static T RehydrateObjectSecurity<T>(PSObject pso)
            where T : System.Security.AccessControl.ObjectSecurity, new()
            string sddl = GetPropertyValue<string>(pso, "SDDL");
            T t = new T();
            t.SetSecurityDescriptorSddlForm(sddl);
        #region Rehydration of types needed by implicit remoting
        /// Gets the boolean properties of ParameterSetMetadata object encoded as an integer.
        /// The PSObject for which to obtain the flags
        /// Boolean properties of ParameterSetMetadata object encoded as an integer
        public static UInt32 GetParameterSetMetadataFlags(PSObject instance)
            if (instance.BaseObject is not ParameterSetMetadata parameterSetMetadata)
            return (UInt32)(parameterSetMetadata.Flags);
        /// Gets the full remoting serialized PSObject for the InvocationInfo property
        /// of the DebuggerStopEventArgs type.
        /// <param name="instance">InvocationInfo instance.</param>
        /// <returns>PSObject containing serialized InvocationInfo.</returns>
        public static PSObject GetInvocationInfo(PSObject instance)
            if (instance.BaseObject is not DebuggerStopEventArgs dbgStopEventArgs)
            if (dbgStopEventArgs.InvocationInfo == null)
            PSObject psoInvocationInfo = new PSObject();
            dbgStopEventArgs.InvocationInfo.ToPSObjectForRemoting(psoInvocationInfo);
            return psoInvocationInfo;
        private static ParameterSetMetadata RehydrateParameterSetMetadata(PSObject pso)
            int position = GetPropertyValue<int>(pso, "Position");
            UInt32 flags = GetPropertyValue<UInt32>(pso, "Flags");
            string helpMessage = GetPropertyValue<string>(pso, "HelpMessage");
            return new ParameterSetMetadata(position, (ParameterSetMetadata.ParameterFlags)flags, helpMessage);
        private static DisplayEntry RehydrateDisplayEntry(PSObject deserializedDisplayEntry)
            var result = new DisplayEntry
                Value = GetPropertyValue<string>(deserializedDisplayEntry, "Value"),
                ValueType = GetPropertyValue<DisplayEntryValueType>(deserializedDisplayEntry, "ValueType")
        private static EntrySelectedBy RehydrateEntrySelectedBy(PSObject deserializedEsb)
            var result = new EntrySelectedBy
                TypeNames = RehydrateList<List<string>, string>(deserializedEsb, "TypeNames", RehydrationFlags.MissingPropertyOk),
                SelectionCondition = RehydrateList<List<DisplayEntry>, DisplayEntry>(deserializedEsb, "SelectionCondition", RehydrationFlags.MissingPropertyOk)
        private static WideControlEntryItem RehydrateWideControlEntryItem(PSObject deserializedEntryItem)
            var entrySelectedBy = GetPropertyValue<EntrySelectedBy>(deserializedEntryItem, "EntrySelectedBy", RehydrationFlags.MissingPropertyOk);
            if (entrySelectedBy == null)
                var selectedBy = RehydrateList<List<string>, string>(deserializedEntryItem, "SelectedBy", RehydrationFlags.MissingPropertyOk);
                entrySelectedBy = EntrySelectedBy.Get(selectedBy, null);
            var result = new WideControlEntryItem
                DisplayEntry = GetPropertyValue<DisplayEntry>(deserializedEntryItem, "DisplayEntry"),
                EntrySelectedBy = entrySelectedBy,
                FormatString = GetPropertyValue<string>(deserializedEntryItem, "FormatString", RehydrationFlags.MissingPropertyOk),
        private static ListControlEntryItem RehydrateListControlEntryItem(PSObject deserializedEntryItem)
            var result = new ListControlEntryItem
                ItemSelectionCondition = GetPropertyValue<DisplayEntry>(deserializedEntryItem, "ItemSelectionCondition", RehydrationFlags.MissingPropertyOk),
                Label = GetPropertyValue<string>(deserializedEntryItem, "Label", RehydrationFlags.NullValueOk)
        private static ListControlEntry RehydrateListControlEntry(PSObject deserializedEntry)
            var entrySelectedBy = GetPropertyValue<EntrySelectedBy>(deserializedEntry, "EntrySelectedBy", RehydrationFlags.MissingPropertyOk);
                var selectedBy = RehydrateList<List<string>, string>(deserializedEntry, "SelectedBy", RehydrationFlags.MissingPropertyOk);
            var result = new ListControlEntry
                Items = RehydrateList<List<ListControlEntryItem>, ListControlEntryItem>(deserializedEntry, "Items", RehydrationFlags.NullValueBad),
                EntrySelectedBy = entrySelectedBy
        private static TableControlColumnHeader RehydrateTableControlColumnHeader(PSObject deserializedHeader)
            var result = new TableControlColumnHeader
                Alignment = GetPropertyValue<Alignment>(deserializedHeader, "Alignment"),
                Label = GetPropertyValue<string>(deserializedHeader, "Label", RehydrationFlags.NullValueOk),
                Width = GetPropertyValue<int>(deserializedHeader, "Width")
        private static TableControlColumn RehydrateTableControlColumn(PSObject deserializedColumn)
            var result = new TableControlColumn
                Alignment = GetPropertyValue<Alignment>(deserializedColumn, "Alignment"),
                DisplayEntry = GetPropertyValue<DisplayEntry>(deserializedColumn, "DisplayEntry"),
                FormatString = GetPropertyValue<string>(deserializedColumn, "FormatString", RehydrationFlags.MissingPropertyOk)
        private static TableControlRow RehydrateTableControlRow(PSObject deserializedRow)
            var result = new TableControlRow
                Wrap = GetPropertyValue<bool>(deserializedRow, "Wrap", RehydrationFlags.MissingPropertyOk),
                SelectedBy = GetPropertyValue<EntrySelectedBy>(deserializedRow, "EntrySelectedBy", RehydrationFlags.MissingPropertyOk),
                Columns = RehydrateList<List<TableControlColumn>, TableControlColumn>(deserializedRow, "Columns", RehydrationFlags.NullValueBad)
        private static CustomControlEntry RehydrateCustomControlEntry(PSObject deserializedEntry)
            var result = new CustomControlEntry
                CustomItems = RehydrateList<List<CustomItemBase>, CustomItemBase>(deserializedEntry, "CustomItems", RehydrationFlags.MissingPropertyBad),
                SelectedBy = GetPropertyValue<EntrySelectedBy>(deserializedEntry, "SelectedBy", RehydrationFlags.MissingPropertyOk)
        private static CustomItemBase RehydrateCustomItemBase(PSObject deserializedItem)
            CustomItemBase result;
            if (Deserializer.IsDeserializedInstanceOfType(deserializedItem, typeof(CustomItemNewline)))
                result = new CustomItemNewline
                    Count = GetPropertyValue<int>(deserializedItem, "Count", RehydrationFlags.MissingPropertyBad)
            else if (Deserializer.IsDeserializedInstanceOfType(deserializedItem, typeof(CustomItemText)))
                result = new CustomItemText
                    Text = GetPropertyValue<string>(deserializedItem, "Text", RehydrationFlags.MissingPropertyBad)
            else if (Deserializer.IsDeserializedInstanceOfType(deserializedItem, typeof(CustomItemFrame)))
                result = new CustomItemFrame
                    FirstLineHanging = GetPropertyValue<uint>(deserializedItem, "FirstLineHanging"),
                    FirstLineIndent = GetPropertyValue<uint>(deserializedItem, "FirstLineIndent"),
                    RightIndent = GetPropertyValue<uint>(deserializedItem, "RightIndent"),
                    LeftIndent = GetPropertyValue<uint>(deserializedItem, "LeftIndent"),
                    CustomItems = RehydrateList<List<CustomItemBase>, CustomItemBase>(deserializedItem, "CustomItems", RehydrationFlags.MissingPropertyBad)
            else if (Deserializer.IsDeserializedInstanceOfType(deserializedItem, typeof(CustomItemExpression)))
                result = new CustomItemExpression
                    EnumerateCollection = GetPropertyValue<bool>(deserializedItem, "EnumerateCollection"),
                    CustomControl = GetPropertyValue<CustomControl>(deserializedItem, "CustomControl", RehydrationFlags.MissingPropertyOk),
                    Expression = GetPropertyValue<DisplayEntry>(deserializedItem, "Expression", RehydrationFlags.MissingPropertyOk),
                    ItemSelectionCondition = GetPropertyValue<DisplayEntry>(deserializedItem, "ItemSelectionCondition", RehydrationFlags.MissingPropertyOk)
                throw PSTraceSource.NewArgumentException(nameof(deserializedItem));
        private static PSControl RehydratePSControl(PSObject deserializedControl)
            // Earlier versions of PowerShell did not have all of the possible properties in a control, so we must
            // use MissingPropertyOk to allow for connections to those older endpoints.
            PSControl result;
            if (Deserializer.IsDeserializedInstanceOfType(deserializedControl, typeof(TableControl)))
                var tableControl = new TableControl
                    AutoSize = GetPropertyValue<bool>(deserializedControl, "AutoSize", RehydrationFlags.MissingPropertyOk),
                    HideTableHeaders = GetPropertyValue<bool>(deserializedControl, "HideTableHeaders", RehydrationFlags.MissingPropertyOk),
                    Headers = RehydrateList<List<TableControlColumnHeader>, TableControlColumnHeader>(deserializedControl, "Headers", RehydrationFlags.NullValueBad),
                    Rows = RehydrateList<List<TableControlRow>, TableControlRow>(deserializedControl, "Rows", RehydrationFlags.NullValueBad)
                result = tableControl;
            else if (Deserializer.IsDeserializedInstanceOfType(deserializedControl, typeof(ListControl)))
                var listControl = new ListControl
                    Entries = RehydrateList<List<ListControlEntry>, ListControlEntry>(deserializedControl, "Entries", RehydrationFlags.NullValueBad)
                result = listControl;
            else if (Deserializer.IsDeserializedInstanceOfType(deserializedControl, typeof(WideControl)))
                var wideControl = new WideControl
                    AutoSize = GetPropertyValue<bool>(deserializedControl, "Alignment", RehydrationFlags.MissingPropertyOk),
                    Columns = GetPropertyValue<uint>(deserializedControl, "Columns"),
                    Entries = RehydrateList<List<WideControlEntryItem>, WideControlEntryItem>(deserializedControl, "Entries", RehydrationFlags.NullValueBad)
                result = wideControl;
            else if (Deserializer.IsDeserializedInstanceOfType(deserializedControl, typeof(CustomControl)))
                var customControl = new CustomControl
                    Entries = RehydrateList<List<CustomControlEntry>, CustomControlEntry>(deserializedControl, "Entries", RehydrationFlags.NullValueBad)
                result = customControl;
                throw PSTraceSource.NewArgumentException("pso");
            result.GroupBy = GetPropertyValue<PSControlGroupBy>(deserializedControl, "GroupBy", RehydrationFlags.MissingPropertyOk);
            result.OutOfBand = GetPropertyValue<bool>(deserializedControl, "OutOfBand", RehydrationFlags.MissingPropertyOk);
        private static PSControlGroupBy RehydrateGroupBy(PSObject deserializedGroupBy)
            var result = new PSControlGroupBy
                CustomControl = GetPropertyValue<CustomControl>(deserializedGroupBy, "CustomControl", RehydrationFlags.MissingPropertyOk),
                Expression = GetPropertyValue<DisplayEntry>(deserializedGroupBy, "Expression", RehydrationFlags.MissingPropertyOk),
                Label = GetPropertyValue<string>(deserializedGroupBy, "Label", RehydrationFlags.NullValueOk)
        public static Guid GetFormatViewDefinitionInstanceId(PSObject instance)
            if (instance.BaseObject is not FormatViewDefinition formatViewDefinition)
            return formatViewDefinition.InstanceId;
        private static FormatViewDefinition RehydrateFormatViewDefinition(PSObject deserializedViewDefinition)
            string name = GetPropertyValue<string>(deserializedViewDefinition, "Name");
            Guid instanceId = GetPropertyValue<Guid>(deserializedViewDefinition, "InstanceId");
            PSControl control = GetPropertyValue<PSControl>(deserializedViewDefinition, "Control");
            return new FormatViewDefinition(name, control, instanceId);
        private static ExtendedTypeDefinition RehydrateExtendedTypeDefinition(PSObject deserializedTypeDefinition)
            // Prefer TypeNames to TypeName - as it was incorrect to create multiple ExtendedTypeDefinitions for a group of types.
            // But if a new PowerShell connects to an old endpoint, TypeNames will be missing, so fall back to TypeName in that case.
            var typeNames = RehydrateList<List<string>, string>(deserializedTypeDefinition, "TypeNames", RehydrationFlags.MissingPropertyOk);
            if (typeNames == null || typeNames.Count == 0)
                typeName = GetPropertyValue<string>(deserializedTypeDefinition, "TypeName");
            List<FormatViewDefinition> viewDefinitions = RehydrateList<List<FormatViewDefinition>, FormatViewDefinition>(
                deserializedTypeDefinition,
                "FormatViewDefinition",
                RehydrationFlags.NullValueBad);
            var result = new ExtendedTypeDefinition(typeName, viewDefinitions);
            if (typeNames != null && typeNames.Count > 1)
                for (var i = 1; i < typeNames.Count; i++)
                    result.TypeNames.Add(typeNames[i]);

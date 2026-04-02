using System.Diagnostics.Eventing;
    /// Class that implements the New-WinEvent cmdlet.
    /// This cmdlet writes a new Etw event using the provider specified in parameter.
    [Cmdlet(VerbsCommon.New, "WinEvent", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096808")]
    public sealed class NewWinEventCommand : PSCmdlet
        private ProviderMetadata _providerMetadata;
        private EventDescriptor? _eventDescriptor;
        private const string TemplateTag = "template";
        private const string DataTag = "data";
        private readonly ResourceManager _resourceMgr = Microsoft.PowerShell.Commands.Diagnostics.Common.CommonUtilities.GetResourceManager();
        /// ProviderName.
            ParameterSetName = ParameterAttribute.AllParameterSets)]
        public string ProviderName { get; set; }
        /// Id (EventId defined in manifest file)
        public int Id
                return _id;
                _id = value;
                _idSpecified = true;
        private int _id;
        private bool _idSpecified = false;
        /// Version (event version)
        public byte Version
                return _version;
                _version = value;
                _versionSpecified = true;
        private byte _version;
        private bool _versionSpecified = false;
        /// Event Payload.
            ParameterSetName = ParameterAttribute.AllParameterSets),
        AllowEmptyCollection,
        SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
            Target = "Microsoft.PowerShell.Commands",
        public object[] Payload { get; set; }
        /// BeginProcessing.
            LoadProvider();
            LoadEventDescriptor();
            base.BeginProcessing();
        private void LoadProvider()
            if (string.IsNullOrEmpty(ProviderName))
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, _resourceMgr.GetString("ProviderNotSpecified")), "ProviderName");
            using (EventLogSession session = new())
                foreach (string providerName in session.GetProviderNames())
                    if (string.Equals(providerName, ProviderName, StringComparison.OrdinalIgnoreCase))
                            _providerMetadata = new ProviderMetadata(providerName);
                        catch (EventLogException exc)
                            string msg = string.Format(CultureInfo.InvariantCulture, _resourceMgr.GetString("ProviderMetadataUnavailable"), providerName, exc.Message);
                            throw new Exception(msg, exc);
            if (_providerMetadata == null)
                string msg = string.Format(CultureInfo.InvariantCulture, _resourceMgr.GetString("NoProviderFound"), ProviderName);
                throw new ArgumentException(msg);
        private void LoadEventDescriptor()
            if (_idSpecified)
                List<EventMetadata> matchedEvents = new();
                foreach (EventMetadata emd in _providerMetadata.Events)
                    if (emd.Id == _id)
                        matchedEvents.Add(emd);
                if (matchedEvents.Count == 0)
                        _resourceMgr.GetString("IncorrectEventId"),
                        _id,
                        ProviderName);
                    throw new EventWriteException(msg);
                EventMetadata matchedEvent = null;
                if (!_versionSpecified && matchedEvents.Count == 1)
                    matchedEvent = matchedEvents[0];
                    if (_versionSpecified)
                        foreach (EventMetadata emd in matchedEvents)
                            if (emd.Version == _version)
                                matchedEvent = emd;
                        if (matchedEvent == null)
                                _resourceMgr.GetString("IncorrectEventVersion"),
                                _version,
                            _resourceMgr.GetString("VersionNotSpecified"),
                VerifyTemplate(matchedEvent);
                _eventDescriptor = CreateEventDescriptor(_providerMetadata, matchedEvent);
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, _resourceMgr.GetString("EventIdNotSpecified")), "Id");
        private bool VerifyTemplate(EventMetadata emd)
            if (emd.Template != null)
                XmlReaderSettings readerSettings = new()
                    CheckCharacters = false,
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true,
                    MaxCharactersInDocument = 0, // no limit
                    ConformanceLevel = ConformanceLevel.Fragment,
                    XmlResolver = null
                int definedParameterCount = 0;
                using (XmlReader reader = XmlReader.Create(new StringReader(emd.Template), readerSettings))
                    if (reader.ReadToFollowing(TemplateTag))
                        bool found = reader.ReadToDescendant(DataTag);
                        while (found)
                            definedParameterCount++;
                            found = reader.ReadToFollowing(DataTag);
                if ((Payload == null && definedParameterCount != 0)
                    || ((Payload != null) && Payload.Length != definedParameterCount))
                    string warning = string.Format(CultureInfo.InvariantCulture, _resourceMgr.GetString("PayloadMismatch"), _id, emd.Template);
                    WriteWarning(warning);
        private static EventDescriptor CreateEventDescriptor(ProviderMetadata providerMetaData, EventMetadata emd)
            long keywords = 0;
            foreach (EventKeyword keyword in emd.Keywords)
                keywords |= keyword.Value;
            byte channel = 0;
            foreach (EventLogLink logLink in providerMetaData.LogLinks)
                if (string.Equals(logLink.LogName, emd.LogLink.LogName, StringComparison.OrdinalIgnoreCase))
                channel++;
            return new EventDescriptor(
                (int)emd.Id,
                emd.Version,
                channel,
                (byte)emd.Level.Value,
                (byte)emd.Opcode.Value,
                emd.Task.Value,
                keywords);
        /// ProcessRecord.
            using (EventProvider provider = new(_providerMetadata.Id))
                EventDescriptor ed = _eventDescriptor.Value;
                if (Payload != null && Payload.Length > 0)
                    for (int i = 0; i < Payload.Length; i++)
                        if (Payload[i] == null)
                            Payload[i] = string.Empty;
                    provider.WriteEvent(in ed, Payload);
                    provider.WriteEvent(in ed);
            base.ProcessRecord();
        /// EndProcessing.
            _providerMetadata?.Dispose();
    internal sealed class EventWriteException : Exception
        internal EventWriteException(string msg, Exception innerException)
            : base(msg, innerException)
        { }
        internal EventWriteException(string msg)
            : base(msg)

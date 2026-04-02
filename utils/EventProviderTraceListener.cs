    public class EventProviderTraceListener : TraceListener
        // The listener uses the EtwProvider base class.
        // Because Listener data is not schematized at the moment the listener will
        // log events using WriteMessageEvent method.
        // Because WriteMessageEvent takes a string as the event payload
        // all the overridden logging methods convert the arguments into strings.
        // Event payload is "delimiter" separated, which can be configured
        private EventProvider _provider;
        private const string s_nullStringValue = "null";
        private const string s_nullStringComaValue = "null,";
        private const string s_nullCStringValue = ": null";
        private string _delimiter = ";";
        private const uint s_keyWordMask = 0xFFFFFF00;
        private const int s_defaultPayloadSize = 512;
        public string Delimiter
                return _delimiter;
                ArgumentNullException.ThrowIfNull(value, nameof(Delimiter));
                if (value.Length == 0)
                    throw new ArgumentException(DotNetEventingStrings.Argument_NeedNonemptyDelimiter);
                _delimiter = value;
        /// This method creates an instance of the ETW provider.
        /// The guid argument must be a valid GUID or a format exception will be
        /// thrown when creating an instance of the ControlGuid.
        /// PlatformNotSupported exception will be thrown by the EventProvider.
        public EventProviderTraceListener(string providerId)
            InitProvider(providerId);
        public EventProviderTraceListener(string providerId, string name)
        public EventProviderTraceListener(string providerId, string name, string delimiter)
            ArgumentNullException.ThrowIfNull(delimiter);
            if (delimiter.Length == 0)
        private void InitProvider(string providerId)
            Guid controlGuid = new(providerId);
            // Create The ETW TraceProvider
            _provider = new EventProvider(controlGuid);
        // override Listener methods
        public sealed override void Flush()
        public sealed override bool IsThreadSafe
                _provider.Close();
        public sealed override void Write(string message)
            if (!_provider.IsEnabled())
            _provider.WriteMessageEvent(message, (byte)TraceEventType.Information, 0);
        public sealed override void WriteLine(string message)
            Write(message);
        // For all the methods below the string to be logged contains:
        // m_delimiter separated data converted to string
        // The source parameter is ignored.
        public sealed override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null))
            StringBuilder dataString = new(s_defaultPayloadSize);
                dataString.Append(data.ToString());
                dataString.Append(s_nullCStringValue);
            _provider.WriteMessageEvent(dataString.ToString(),
                            (byte)eventType,
                            (long)eventType & s_keyWordMask);
        public sealed override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
            if ((data != null) && (data.Length > 0))
                for (index = 0; index < (data.Length - 1); index++)
                    if (data[index] != null)
                        dataString.Append(data[index].ToString());
                        dataString.Append(Delimiter);
                        dataString.Append(s_nullStringComaValue);
                    dataString.Append(s_nullStringValue);
        public sealed override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
            _provider.WriteMessageEvent(string.Empty,
        public sealed override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
            dataString.Append(message);
        public sealed override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
                _provider.WriteMessageEvent(format,
                _provider.WriteMessageEvent(string.Format(CultureInfo.InvariantCulture, format, args),
        public override void Fail(string message, string detailMessage)
            StringBuilder failMessage = new(message);
            if (detailMessage != null)
                failMessage.Append(' ');
                failMessage.Append(detailMessage);
            this.TraceEvent(null, null, TraceEventType.Error, 0, failMessage.ToString());

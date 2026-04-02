    /// This class contains information about the capability of one side of the connection. The client
    /// side and the server side will have their own capabilities. These two sets of capabilities will
    /// be used in a capability negotiation algorithm to determine if it is possible to establish a
    /// connection between the client and the server.
    internal class RemoteSessionCapability
        private readonly Version _psversion;
        private readonly Version _serversion;
        private Version _protocolVersion;
        private readonly RemotingDestination _remotingDestination;
        internal Version ProtocolVersion
                return _protocolVersion;
                _protocolVersion = value;
        internal Version PSVersion { get { return _psversion; } }
        internal Version SerializationVersion { get { return _serversion; } }
        internal RemotingDestination RemotingDestination { get { return _remotingDestination; } }
        /// Constructor for RemoteSessionCapability.
        /// <remarks>should not be called from outside, use create methods instead
        internal RemoteSessionCapability(RemotingDestination remotingDestination)
            _protocolVersion = RemotingConstants.ProtocolVersion;
            // PS Version 3 is fully backward compatible with Version 2
            // In the remoting protocol sense, nothing is changing between PS3 and PS2
            // For negotiation to succeed with old client/servers we have to use 2.
            _psversion = new Version(2, 0); // PSVersionInfo.PSVersion;
            _serversion = PSVersionInfo.SerializationVersion;
            _remotingDestination = remotingDestination;
        internal RemoteSessionCapability(RemotingDestination remotingDestination,
            Version protocolVersion,
            Version psVersion,
            Version serVersion)
            _protocolVersion = protocolVersion;
            _psversion = psVersion;
            _serversion = serVersion;
        /// Create client capability.
        internal static RemoteSessionCapability CreateClientCapability()
            return new RemoteSessionCapability(RemotingDestination.Server);
        /// Create server capability.
        internal static RemoteSessionCapability CreateServerCapability()
            return new RemoteSessionCapability(RemotingDestination.Client);
    /// The HostDefaultDataId enum.
    internal enum HostDefaultDataId
        ForegroundColor,
        BackgroundColor,
        CursorPosition,
        WindowPosition,
        CursorSize,
        BufferSize,
        WindowSize,
        MaxWindowSize,
        MaxPhysicalWindowSize,
        WindowTitle,
    /// The HostDefaultData class.
    internal sealed class HostDefaultData
        /// Data.
        private readonly Dictionary<HostDefaultDataId, object> data;
        /// Private constructor to force use of Create.
        private HostDefaultData()
            data = new Dictionary<HostDefaultDataId, object>();
        /// Indexer to provide clean access to data.
        internal object this[HostDefaultDataId id]
                return this.GetValue(id);
        /// Has value.
        internal bool HasValue(HostDefaultDataId id)
            return data.ContainsKey(id);
        /// Set value.
        internal void SetValue(HostDefaultDataId id, object dataValue)
            data[id] = dataValue;
        /// Get value.
        internal object GetValue(HostDefaultDataId id)
            data.TryGetValue(id, out result);
        /// Returns null if host is null or if reading RawUI fields fails; otherwise returns a valid object.
        internal static HostDefaultData Create(PSHostRawUserInterface hostRawUI)
            if (hostRawUI == null)
            HostDefaultData hostDefaultData = new HostDefaultData();
            // Try to get values from the host. Catch-all okay because of 3rd party call-out.
            // Set ForegroundColor.
                hostDefaultData.SetValue(HostDefaultDataId.ForegroundColor, hostRawUI.ForegroundColor);
            // Set BackgroundColor.
                hostDefaultData.SetValue(HostDefaultDataId.BackgroundColor, hostRawUI.BackgroundColor);
            // Set CursorPosition.
                hostDefaultData.SetValue(HostDefaultDataId.CursorPosition, hostRawUI.CursorPosition);
            // Set WindowPosition.
                hostDefaultData.SetValue(HostDefaultDataId.WindowPosition, hostRawUI.WindowPosition);
            // Set CursorSize.
                hostDefaultData.SetValue(HostDefaultDataId.CursorSize, hostRawUI.CursorSize);
            // Set BufferSize.
                hostDefaultData.SetValue(HostDefaultDataId.BufferSize, hostRawUI.BufferSize);
            // Set WindowSize.
                hostDefaultData.SetValue(HostDefaultDataId.WindowSize, hostRawUI.WindowSize);
            // Set MaxWindowSize.
                hostDefaultData.SetValue(HostDefaultDataId.MaxWindowSize, hostRawUI.MaxWindowSize);
            // Set MaxPhysicalWindowSize.
                hostDefaultData.SetValue(HostDefaultDataId.MaxPhysicalWindowSize, hostRawUI.MaxPhysicalWindowSize);
            // Set WindowTitle.
                hostDefaultData.SetValue(HostDefaultDataId.WindowTitle, hostRawUI.WindowTitle);
            return hostDefaultData;
    /// The HostInfo class.
    internal class HostInfo
        /// Host default data.
        internal HostDefaultData HostDefaultData
            get { return _hostDefaultData; }
        /// Is host null.
        internal bool IsHostNull
            get { return _isHostNull; }
        /// Is host ui null.
        private readonly bool _isHostUINull;
        internal bool IsHostUINull
                return _isHostUINull;
        /// Is host raw ui null.
        private readonly bool _isHostRawUINull;
        private readonly bool _isHostNull;
        private readonly HostDefaultData _hostDefaultData;
        private bool _useRunspaceHost;
        internal bool IsHostRawUINull
                return _isHostRawUINull;
        /// Use runspace host.
        internal bool UseRunspaceHost
            get { return _useRunspaceHost; }
            set { _useRunspaceHost = value; }
        /// Constructor for HostInfo.
        internal HostInfo(PSHost host)
            // Set these flags based on investigating the host.
            CheckHostChain(host, ref _isHostNull, ref _isHostUINull, ref _isHostRawUINull);
            // If raw UI is non-null then get the host-info object.
            if (!_isHostUINull && !_isHostRawUINull)
                _hostDefaultData = HostDefaultData.Create(host.UI.RawUI);
        /// Check host chain.
        private static void CheckHostChain(PSHost host, ref bool isHostNull, ref bool isHostUINull, ref bool isHostRawUINull)
            // Set the defaults.
            isHostNull = true;
            isHostUINull = true;
            isHostRawUINull = true;
            // Unwrap the host: remove outer InternalHost object.
                // If host is null then the bools are correct. Nothing further to do here.
            else if (host is InternalHost)
                // This nesting can only be one level deep.
                host = ((InternalHost)host).ExternalHost;
            // At this point we know for sure that the host is not null.
            isHostNull = false;
            // Verify that the UI is not null.
            if (host.UI == null)
            isHostUINull = false;
            // Verify that the raw UI is not null.
            if (host.UI.RawUI == null)
            isHostRawUINull = false;

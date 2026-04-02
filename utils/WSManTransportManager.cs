 * managers based on WSMan protocol.
using WSManConnectionInfo = System.Management.Automation.Runspaces.WSManConnectionInfo;
using AuthenticationMechanism = System.Management.Automation.Runspaces.AuthenticationMechanism;
    /// WSMan TransportManager related utils.
    internal static class WSManTransportManagerUtils
        // Fully qualified error Id modifiers based on transport (WinRM) error codes.
        private static readonly Dictionary<int, string> s_transportErrorCodeToFQEID = new Dictionary<int, string>()
            {WSManNativeApi.ERROR_WSMAN_ACCESS_DENIED, "AccessDenied"},
            {WSManNativeApi.ERROR_WSMAN_OUTOF_MEMORY, "ServerOutOfMemory"},
            {WSManNativeApi.ERROR_WSMAN_NETWORKPATH_NOTFOUND, "NetworkPathNotFound"},
            {WSManNativeApi.ERROR_WSMAN_COMPUTER_NOTFOUND, "ComputerNotFound"},
            {WSManNativeApi.ERROR_WSMAN_AUTHENTICATION_FAILED, "AuthenticationFailed"},
            {WSManNativeApi.ERROR_WSMAN_LOGON_FAILURE, "LogonFailure"},
            {WSManNativeApi.ERROR_WSMAN_IMPROPER_RESPONSE, "ImproperResponse"},
            {WSManNativeApi.ERROR_WSMAN_INCORRECT_PROTOCOLVERSION, "IncorrectProtocolVersion"},
            {WSManNativeApi.ERROR_WSMAN_SENDDATA_CANNOT_COMPLETE, "WinRMOperationTimeout"},
            {WSManNativeApi.ERROR_WSMAN_URL_NOTAVAILABLE, "URLNotAvailable"},
            {WSManNativeApi.ERROR_WSMAN_SENDDATA_CANNOT_CONNECT, "CannotConnect"},
            {WSManNativeApi.ERROR_WSMAN_INVALID_RESOURCE_URI, "InvalidResourceUri"},
            {WSManNativeApi.ERROR_WSMAN_INUSE_CANNOT_RECONNECT, "CannotConnectAlreadyConnected"},
            {WSManNativeApi.ERROR_WSMAN_INVALID_AUTHENTICATION, "InvalidAuthentication"},
            {WSManNativeApi.ERROR_WSMAN_SHUTDOWN_INPROGRESS, "ShutDownInProgress"},
            {WSManNativeApi.ERROR_WSMAN_CANNOT_CONNECT_INVALID, "CannotConnectInvalidOperation"},
            {WSManNativeApi.ERROR_WSMAN_CANNOT_CONNECT_MISMATCH, "CannotConnectMismatchSessions"},
            {WSManNativeApi.ERROR_WSMAN_CANNOT_CONNECT_RUNASFAILED, "CannotConnectRunAsFailed"},
            {WSManNativeApi.ERROR_WSMAN_CREATEFAILED_INVALIDNAME, "SessionCreateFailedInvalidName"},
            {WSManNativeApi.ERROR_WSMAN_TARGETSESSION_DOESNOTEXIST, "CannotConnectTargetSessionDoesNotExist"},
            {WSManNativeApi.ERROR_WSMAN_REMOTESESSION_DISALLOWED, "RemoteSessionDisallowed"},
            {WSManNativeApi.ERROR_WSMAN_REMOTECONNECTION_DISALLOWED, "RemoteConnectionDisallowed"},
            {WSManNativeApi.ERROR_WSMAN_INVALID_RESOURCE_URI2, "InvalidResourceUri"},
            {WSManNativeApi.ERROR_WSMAN_CORRUPTED_CONFIG, "CorruptedWinRMConfig"},
            {WSManNativeApi.ERROR_WSMAN_OPERATION_ABORTED, "WinRMOperationAborted"},
            {WSManNativeApi.ERROR_WSMAN_URI_LIMIT, "URIExceedsMaxAllowedSize"},
            {WSManNativeApi.ERROR_WSMAN_CLIENT_KERBEROS_DISABLED, "ClientKerberosDisabled"},
            {WSManNativeApi.ERROR_WSMAN_SERVER_NOTTRUSTED, "ServerNotTrusted"},
            {WSManNativeApi.ERROR_WSMAN_WORKGROUP_NO_KERBEROS, "WorkgroupCannotUseKerberos"},
            {WSManNativeApi.ERROR_WSMAN_EXPLICIT_CREDENTIALS_REQUIRED, "ExplicitCredentialsRequired"},
            {WSManNativeApi.ERROR_WSMAN_REDIRECT_LOCATION_INVALID, "RedirectLocationInvalid"},
            {WSManNativeApi.ERROR_WSMAN_REDIRECT_REQUESTED, "RedirectInformationRequired"},
            {WSManNativeApi.ERROR_WSMAN_BAD_METHOD, "WinRMOperationNotSupportedOnServer"},
            {WSManNativeApi.ERROR_WSMAN_HTTP_SERVICE_UNAVAILABLE, "CannotConnectWinRMService"},
            {WSManNativeApi.ERROR_WSMAN_HTTP_SERVICE_ERROR, "WinRMHttpError"},
            {WSManNativeApi.ERROR_WSMAN_TARGET_UNKNOWN, "TargetUnknown"},
            {WSManNativeApi.ERROR_WSMAN_CANNOTUSE_IP, "CannotUseIPAddress"}
        /// Constructs a WSManTransportErrorOccuredEventArgs instance from the supplied data.
        /// <param name="wsmanAPIHandle">
        /// WSMan API handle to use to get error messages from WSMan error id(s)
        /// <param name="wsmanSessionTM">
        /// Session Transportmanager to use to get error messages (for redirect)
        /// <param name="errorStruct">
        /// Error structure supplied by callbacks from WSMan API
        /// <param name="transportMethodReportingError">
        /// The transport method call that reported this error.
        /// resource string that holds the message.
        /// <param name="resourceArgs">
        /// Arguments to pass to the resource
        /// An instance of WSManTransportErrorOccuredEventArgs
        internal static TransportErrorOccuredEventArgs ConstructTransportErrorEventArgs(IntPtr wsmanAPIHandle,
            WSManClientSessionTransportManager wsmanSessionTM,
            WSManNativeApi.WSManError errorStruct,
            TransportMethodEnum transportMethodReportingError,
            params object[] resourceArgs)
            PSRemotingTransportException e;
            // For the first two special error conditions, it is remotely possible that the wsmanSessionTM is null when the failures are returned
            // as part of command TM operations (could be returned because of RC retries under the hood)
            // Not worth to handle these cases separately as there are very corner scenarios, but need to make sure wsmanSessionTM is not referenced
            // Destination server is reporting that URI redirect is required for this user.
            if ((errorStruct.errorCode == WSManNativeApi.ERROR_WSMAN_REDIRECT_REQUESTED) && (wsmanSessionTM != null))
                IntPtr wsmanSessionHandle = wsmanSessionTM.SessionHandle;
                // populate the transport message with the redirection uri..this will
                // allow caller to make a new connection.
                string redirectLocation = WSManNativeApi.WSManGetSessionOptionAsString(wsmanSessionHandle,
                    WSManNativeApi.WSManSessionOption.WSMAN_OPTION_REDIRECT_LOCATION);
                string winrmMessage = ParseEscapeWSManErrorMessage(
                    WSManNativeApi.WSManGetErrorMessage(wsmanAPIHandle, errorStruct.errorCode)).Trim();
                e = new PSRemotingTransportRedirectException(redirectLocation,
                    PSRemotingErrorId.URIEndPointNotResolved,
                    RemotingErrorIdStrings.URIEndPointNotResolved,
                    winrmMessage,
                    redirectLocation);
            else if ((errorStruct.errorCode == WSManNativeApi.ERROR_WSMAN_INVALID_RESOURCE_URI) && (wsmanSessionTM != null))
                string configurationName =
                    wsmanSessionTM.ConnectionInfo.ShellUri.Replace(Remoting.Client.WSManNativeApi.ResourceURIPrefix, string.Empty);
                string errorMessage = PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.InvalidConfigurationName,
                                                   wsmanSessionTM.ConnectionInfo.ComputerName);
                e = new PSRemotingTransportException(PSRemotingErrorId.InvalidConfigurationName,
                                                   RemotingErrorIdStrings.ConnectExCallBackError, wsmanSessionTM.ConnectionInfo.ComputerName, errorMessage);
                e.TransportMessage = ParseEscapeWSManErrorMessage(
                   WSManNativeApi.WSManGetErrorMessage(wsmanAPIHandle, errorStruct.errorCode));
                // Construct specific error message and then append this message pointing to our own
                // help topic. PowerShell's about help topic "about_Remote_Troubleshooting" should
                // contain all the trouble shooting information.
                string wsManErrorMessage = PSRemotingErrorInvariants.FormatResourceString(resourceString, resourceArgs);
                e = new PSRemotingTransportException(PSRemotingErrorId.TroubleShootingHelpTopic,
                    RemotingErrorIdStrings.TroubleShootingHelpTopic,
                    wsManErrorMessage);
            e.ErrorCode = errorStruct.errorCode;
                new TransportErrorOccuredEventArgs(e, transportMethodReportingError);
            return eventargs;
        /// Helper method that escapes powershell parser recognized strings like "@{" from the error message
        /// string. This is needed to make error messages look authentic. Some WSMan error messages provide a
        /// command line to run to fix certain issues. WSMan command line has syntax that allows use of @{}.
        /// PowerShell parser treats them differently..and so when user cut and paste the command line in a
        /// PowerShell console, it wont work. This escape logic works around the issue.
        internal static string ParseEscapeWSManErrorMessage(string errorMessage)
            // currently we do special processing only for "@{" construct.
            if (string.IsNullOrEmpty(errorMessage) || (!errorMessage.Contains("@{")))
            string result = errorMessage.Replace("@{", "'@{").Replace("}", "}'");
             * Use this pattern if we need to escape other characters.
                StringBuilder msgSB = new StringBuilder(errorMessage);
                Collection<PSParseError> parserErrors = new Collection<PSParseError>();
                Collection<PSToken> tokens = PSParser.Tokenize(errorMessage, out parserErrors);
                if (parserErrors.Count > 0)
                    tracer.WriteLine(string.Create(CultureInfo.InvariantCulture, $"There were errors parsing string '{errorMessage}'");
                for (int index = tokens.Count - 1; index > 0; index--)
                    PSToken currentToken = tokens[index];
                    switch(currentToken.Type)
                        case PSTokenType.GroupStart:
                            msgSB.Insert(currentToken.StartColumn - 1, "'", 1);
                        case PSTokenType.GroupEnd:
                            if (msgSB.Length <= currentToken.EndColumn)
                                msgSB.Append("'");
                                msgSB.Insert(currentToken.EndColumn - 1, ",", 1);
                return msgSB.ToString();
            // ignore possible exceptions manipulating the string.
            catch(ArgumentOutOfRangeException)
            catch(RuntimeException)
            return errorMessage;*/
        internal enum tmStartModes
            None = 1, Create = 2, Connect = 3
        /// Helper method to convert a transport error code value
        /// to a fully qualified error Id string.
        /// <param name="transportErrorCode">Transport error code.</param>
        /// <param name="defaultFQEID">Default FQEID.</param>
        /// <returns>Fully qualified error Id string.</returns>
        internal static string GetFQEIDFromTransportError(
            int transportErrorCode,
            string defaultFQEID)
            string specificErrorId;
            if (s_transportErrorCodeToFQEID.TryGetValue(transportErrorCode, out specificErrorId))
                return specificErrorId + "," + defaultFQEID;
            else if (transportErrorCode != 0)
                // Provide error code to uniquely identify the error Id.
                return transportErrorCode.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "," + defaultFQEID;
            return defaultFQEID;
    /// Class that manages a server session. This doesn't implement IDisposable. Use Close method
    /// to clean the resources.
    internal sealed class WSManClientSessionTransportManager : BaseClientSessionTransportManager
        /// Max uri redirection count session variable.
        internal const string MAX_URI_REDIRECTION_COUNT_VARIABLE = "WSManMaxRedirectionCount";
        /// Default max uri redirection count - wsman.
        internal const int MAX_URI_REDIRECTION_COUNT = 5;
        private enum CompletionNotification
            DisconnectCompleted
        #region CompletionEventArgs
        private sealed class CompletionEventArgs : EventArgs
            internal CompletionEventArgs(CompletionNotification notification)
            internal CompletionNotification Notification { get; }
        // operation handles are owned by WSMan
        private IntPtr _wsManSessionHandle;
        private IntPtr _wsManShellOperationHandle;
        private IntPtr _wsManReceiveOperationHandle;
        private IntPtr _wsManSendOperationHandle;
        // this is used with WSMan callbacks to represent a session transport manager.
        private long _sessionContextID;
        private WSManTransportManagerUtils.tmStartModes _startMode = WSManTransportManagerUtils.tmStartModes.None;
        private readonly string _sessionName;
        // callbacks
        // instance callback handlers
        private WSManNativeApi.WSManShellAsync _createSessionCallback;
        private WSManNativeApi.WSManShellAsync _receivedFromRemote;
        private WSManNativeApi.WSManShellAsync _sendToRemoteCompleted;
        private WSManNativeApi.WSManShellAsync _disconnectSessionCompleted;
        private WSManNativeApi.WSManShellAsync _reconnectSessionCompleted;
        private WSManNativeApi.WSManShellAsync _connectSessionCallback;
        // TODO: This GCHandle is required as it seems WSMan is calling create callback
        // after we call Close. This seems wrong. Opened bug on WSMan to track this.
        private GCHandle _createSessionCallbackGCHandle;
        private WSManNativeApi.WSManShellAsync _closeSessionCompleted;
        // used by WSManCreateShell call to send additional data (like negotiation)
        // during shell creation. This is an instance variable to allow for redirection.
        private WSManNativeApi.WSManData_ManToUn _openContent;
        // By default WSMan compresses data sent on the network..use this flag to not do
        // this.
        private bool _noCompression;
        private bool _noMachineProfile;
        private int _connectionRetryCount;
        // Robust connections maximum retry time value in milliseconds.
        private int _maxRetryTime;
        private void ProcessShellData(string data)
                settings.MaxCharactersFromEntities = 1024;      // 1024 is a generous upperbound for shell Xml entries
                settings.MaxCharactersInDocument = 1024 * 30;
                settings.DtdProcessing = System.Xml.DtdProcessing.Prohibit;
                using (XmlReader reader = XmlReader.Create(new StringReader(data), settings))
                        if (reader.NodeType == XmlNodeType.Element)
                            if (reader.LocalName.Equals("IdleTimeOut", StringComparison.OrdinalIgnoreCase) ||
                                reader.LocalName.Equals("MaxIdleTimeOut", StringComparison.OrdinalIgnoreCase))
                                bool settingIdleTimeout =
                                    !reader.LocalName.Equals("MaxIdleTimeOut", StringComparison.OrdinalIgnoreCase);
                                string timeoutString = reader.ReadElementContentAsString();
                                Dbg.Assert(timeoutString.Substring(0, 2).Equals("PT", StringComparison.OrdinalIgnoreCase),
                                    "IdleTimeout is not in expected format");
                                int decimalIndex = timeoutString.IndexOf('.');
                                    int timeout = Convert.ToInt32(timeoutString.Substring(2, decimalIndex - 2), NumberFormatInfo.InvariantInfo) * 1000 + Convert.ToInt32(timeoutString.Substring(decimalIndex + 1, 3), NumberFormatInfo.InvariantInfo);
                                    if (settingIdleTimeout)
                                        ConnectionInfo.IdleTimeout = timeout;
                                        ConnectionInfo.MaxIdleTimeout = timeout;
                                    Dbg.Assert(false, "IdleTimeout is not in expected format");
                            else if (reader.LocalName.Equals("BufferMode", StringComparison.OrdinalIgnoreCase))
                                string bufferMode = reader.ReadElementContentAsString();
                                if (bufferMode.Equals("Block", StringComparison.OrdinalIgnoreCase))
                                    ConnectionInfo.OutputBufferingMode = Runspaces.OutputBufferingMode.Block;
                                else if (bufferMode.Equals("Drop", StringComparison.OrdinalIgnoreCase))
                                    ConnectionInfo.OutputBufferingMode = Runspaces.OutputBufferingMode.Drop;
                                    Dbg.Assert(false, "unexpected buffer mode");
                Dbg.Assert(false, "shell xml is in unexpected format");
        // static callback delegate
        private static WSManNativeApi.WSManShellAsyncCallback s_sessionCreateCallback;
        private static WSManNativeApi.WSManShellAsyncCallback s_sessionCloseCallback;
        private static WSManNativeApi.WSManShellAsyncCallback s_sessionReceiveCallback;
        private static WSManNativeApi.WSManShellAsyncCallback s_sessionSendCallback;
        private static WSManNativeApi.WSManShellAsyncCallback s_sessionDisconnectCallback;
        private static WSManNativeApi.WSManShellAsyncCallback s_sessionReconnectCallback;
        private static WSManNativeApi.WSManShellAsyncCallback s_sessionConnectCallback;
        // This dictionary maintains active session transport managers to be used from various
        // callbacks.
        private static readonly Dictionary<long, WSManClientSessionTransportManager> s_sessionTMHandles =
            new Dictionary<long, WSManClientSessionTransportManager>();
        private static long s_sessionTMSeed;
        // generate unique session id
        private static long GetNextSessionTMHandleId()
            return System.Threading.Interlocked.Increment(ref s_sessionTMSeed);
        // we need a synchronized add and remove so that multiple threads
        // update the data store concurrently
        private static void AddSessionTransportManager(long sessnTMId,
            WSManClientSessionTransportManager sessnTransportManager)
            lock (s_sessionTMHandles)
                s_sessionTMHandles.Add(sessnTMId, sessnTransportManager);
        private static void RemoveSessionTransportManager(long sessnTMId)
                s_sessionTMHandles.Remove(sessnTMId);
        private static bool TryGetSessionTransportManager(IntPtr operationContext,
            out WSManClientSessionTransportManager sessnTransportManager,
            out long sessnTMId)
            sessnTMId = operationContext.ToInt64();
            sessnTransportManager = null;
                return s_sessionTMHandles.TryGetValue(sessnTMId, out sessnTransportManager);
        #region SHIM: Redirection delegates for test purposes
        private static readonly Delegate s_sessionSendRedirect = null;
        private static readonly Delegate s_protocolVersionRedirect = null;
        /// Static constructor to initialize WSMan Client stack.
        static WSManClientSessionTransportManager()
            // Initialize callback delegates
            WSManNativeApi.WSManShellCompletionFunction createDelegate =
                new WSManNativeApi.WSManShellCompletionFunction(OnCreateSessionCallback);
            s_sessionCreateCallback = new WSManNativeApi.WSManShellAsyncCallback(createDelegate);
            WSManNativeApi.WSManShellCompletionFunction closeDelegate =
                new WSManNativeApi.WSManShellCompletionFunction(OnCloseSessionCompleted);
            s_sessionCloseCallback = new WSManNativeApi.WSManShellAsyncCallback(closeDelegate);
            WSManNativeApi.WSManShellCompletionFunction receiveDelegate =
                new WSManNativeApi.WSManShellCompletionFunction(OnRemoteSessionDataReceived);
            s_sessionReceiveCallback = new WSManNativeApi.WSManShellAsyncCallback(receiveDelegate);
            WSManNativeApi.WSManShellCompletionFunction sendDelegate =
                new WSManNativeApi.WSManShellCompletionFunction(OnRemoteSessionSendCompleted);
            s_sessionSendCallback = new WSManNativeApi.WSManShellAsyncCallback(sendDelegate);
            WSManNativeApi.WSManShellCompletionFunction disconnectDelegate =
                new WSManNativeApi.WSManShellCompletionFunction(OnRemoteSessionDisconnectCompleted);
            s_sessionDisconnectCallback = new WSManNativeApi.WSManShellAsyncCallback(disconnectDelegate);
            WSManNativeApi.WSManShellCompletionFunction reconnectDelegate =
                new WSManNativeApi.WSManShellCompletionFunction(OnRemoteSessionReconnectCompleted);
            s_sessionReconnectCallback = new WSManNativeApi.WSManShellAsyncCallback(reconnectDelegate);
            WSManNativeApi.WSManShellCompletionFunction connectDelegate =
                new WSManNativeApi.WSManShellCompletionFunction(OnRemoteSessionConnectCallback);
            s_sessionConnectCallback = new WSManNativeApi.WSManShellAsyncCallback(connectDelegate);
        /// Constructor. This will create a new PrioritySendDataCollection which should be used to
        /// <param name="runspacePoolInstanceId">
        /// This is used for logging trace/operational crimson messages. Having this id in the logs
        /// helps a user to map which transport is created for which runspace.
        /// Connection info to use while connecting to the remote machine.
        /// <param name="cryptoHelper">Crypto helper.</param>
        /// <param name="sessionName">Session friendly name.</param>
        /// 1. Create Session failed with a non-zero error code.
        internal WSManClientSessionTransportManager(
            Guid runspacePoolInstanceId,
            WSManConnectionInfo connectionInfo,
            string sessionName)
            : base(runspacePoolInstanceId, cryptoHelper)
            // Initialize WSMan instance
            WSManAPIData = new WSManAPIDataCommon();
            if (WSManAPIData.WSManAPIHandle == IntPtr.Zero)
                    StringUtil.Format(RemotingErrorIdStrings.WSManInitFailed, WSManAPIData.ErrorCode));
            Dbg.Assert(connectionInfo != null, "connectionInfo cannot be null");
            _sessionName = sessionName;
            ReceivedDataCollection.MaximumReceivedObjectSize = connectionInfo.MaximumReceivedObjectSize;
            Initialize(connectionInfo.ConnectionUri, connectionInfo);
        #region Set Session Options
        /// Sets default timeout for all client operations in milliseconds.
        /// TODO: Sync with WSMan and figure out what the default is if we
        /// dont set.
        /// <param name="milliseconds"></param>
        /// Setting session option failed with a non-zero error code.
        internal void SetDefaultTimeOut(int milliseconds)
            Dbg.Assert(_wsManSessionHandle != IntPtr.Zero, "Session handle cannot be null");
            using (tracer.TraceMethod("Setting Default timeout: {0} milliseconds", milliseconds))
                int result = WSManNativeApi.WSManSetSessionOption(_wsManSessionHandle,
                    WSManNativeApi.WSManSessionOption.WSMAN_OPTION_DEFAULT_OPERATION_TIMEOUTMS,
                    new WSManNativeApi.WSManDataDWord(milliseconds));
                    // Get the error message from WSMan
                    string errorMessage = WSManNativeApi.WSManGetErrorMessage(WSManAPIData.WSManAPIHandle, result);
                    PSInvalidOperationException exception = new PSInvalidOperationException(errorMessage);
        /// Sets timeout for Create operation in milliseconds.
        internal void SetConnectTimeOut(int milliseconds)
            using (tracer.TraceMethod("Setting CreateShell timeout: {0} milliseconds", milliseconds))
                    WSManNativeApi.WSManSessionOption.WSMAN_OPTION_TIMEOUTMS_CREATE_SHELL,
        /// Sets timeout for Close operation in milliseconds.
        internal void SetCloseTimeOut(int milliseconds)
            using (tracer.TraceMethod("Setting CloseShell timeout: {0} milliseconds", milliseconds))
                    WSManNativeApi.WSManSessionOption.WSMAN_OPTION_TIMEOUTMS_CLOSE_SHELL_OPERATION,
        /// Sets timeout for SendShellInput operation in milliseconds.
        internal void SetSendTimeOut(int milliseconds)
            using (tracer.TraceMethod("Setting SendShellInput timeout: {0} milliseconds", milliseconds))
                    WSManNativeApi.WSManSessionOption.WSMAN_OPTION_TIMEOUTMS_SEND_SHELL_INPUT,
        /// Sets timeout for Receive operation in milliseconds.
        internal void SetReceiveTimeOut(int milliseconds)
            using (tracer.TraceMethod("Setting ReceiveShellOutput timeout: {0} milliseconds", milliseconds))
                    WSManNativeApi.WSManSessionOption.WSMAN_OPTION_TIMEOUTMS_RECEIVE_SHELL_OUTPUT,
        /// Sets timeout for Signal operation in milliseconds.
        internal void SetSignalTimeOut(int milliseconds)
            using (tracer.TraceMethod("Setting SignalShell timeout: {0} milliseconds", milliseconds))
                    WSManNativeApi.WSManSessionOption.WSMAN_OPTION_TIMEOUTMS_SIGNAL_SHELL,
        /// Sets a DWORD value for a WSMan Session option.
        /// <param name="dwordData"></param>
        internal void SetWSManSessionOption(WSManNativeApi.WSManSessionOption option, int dwordData)
                option, new WSManNativeApi.WSManDataDWord(dwordData));
        /// Sets a string value for a WSMan Session option.
        /// <param name="stringData"></param>
        internal void SetWSManSessionOption(WSManNativeApi.WSManSessionOption option, string stringData)
            using (WSManNativeApi.WSManData_ManToUn data = new WSManNativeApi.WSManData_ManToUn(stringData))
                      option, data);
        internal WSManAPIDataCommon WSManAPIData { get; private set; }
        internal bool SupportsDisconnect { get; private set; }
            Dbg.Assert(!isClosed, "object already disposed");
            // Pass the WSManConnectionInfo object IdleTimeout value if it is
            // valid.  Otherwise pass the default value that instructs the server
            // to use its default IdleTimeout value.
            uint uIdleTimeout = (ConnectionInfo.IdleTimeout > 0) ?
                (uint)ConnectionInfo.IdleTimeout : UseServerDefaultIdleTimeoutUInt;
            // startup info
            WSManNativeApi.WSManShellDisconnectInfo disconnectInfo = new WSManNativeApi.WSManShellDisconnectInfo(uIdleTimeout);
            // Add ETW traces
            // disconnect Callback
            _disconnectSessionCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(_sessionContextID), s_sessionDisconnectCallback);
                        // the transport is already closed
                        // anymore.
                    flags |= (ConnectionInfo.OutputBufferingMode == Runspaces.OutputBufferingMode.Block) ?
                                    (int)WSManNativeApi.WSManShellFlag.WSMAN_FLAG_SERVER_BUFFERING_MODE_BLOCK : 0;
                    flags |= (ConnectionInfo.OutputBufferingMode == Runspaces.OutputBufferingMode.Drop) ?
                                    (int)WSManNativeApi.WSManShellFlag.WSMAN_FLAG_SERVER_BUFFERING_MODE_DROP : 0;
                    WSManNativeApi.WSManDisconnectShellEx(_wsManShellOperationHandle,
                            disconnectInfo,
                            _disconnectSessionCompleted);
                disconnectInfo.Dispose();
            ReceivedDataCollection.PrepareForStreamConnect();
            // reconnect Callback
            _reconnectSessionCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(_sessionContextID), s_sessionReconnectCallback);
                WSManNativeApi.WSManReconnectShellEx(_wsManShellOperationHandle,
                        _reconnectSessionCompleted);
        /// Starts connecting to an existing remote session. This will result in a WSManConnectShellEx WSMan
        /// async call. Piggy backs available data in input stream as openXml in connect SOAP.
        /// DSHandler will push negotiation related messages through the open content.
        /// WSManConnectShellEx failed.
            Dbg.Assert(!string.IsNullOrEmpty(ConnectionInfo.ShellUri), "shell uri cannot be null or empty.");
            // additional content with connect shell call. Negotiation and connect related messages
            // should be included in payload
            if (_openContent == null)
                DataPriorityType additionalDataType;
                byte[] additionalData = dataToBeSent.ReadOrRegisterCallback(null, out additionalDataType);
                if (additionalData != null)
                    // WSMan expects the data to be in XML format (which is text + xml tags)
                    // so convert byte[] into base64 encoded format
                    string base64EncodedDataInXml = string.Format(
                        WSManNativeApi.PS_CONNECT_XML_TAG,
                        Convert.ToBase64String(additionalData));
                    _openContent = new WSManNativeApi.WSManData_ManToUn(base64EncodedDataInXml);
                // THERE SHOULD BE NO ADDITIONAL DATA. If there is, it means we are not able to push all initial negotiation related data
                // as part of Connect SOAP. The connect algorithm is based on this assumption. So bail out.
                additionalData = dataToBeSent.ReadOrRegisterCallback(null, out additionalDataType);
                    // Negotiation payload does not fit in ConnectShell. bail out.
                    // Assert for now. should be replaced with raising an exception so upper layers can catch.
                    Dbg.Assert(false, "Negotiation payload does not fit in ConnectShell");
            // Create and store context for this shell operation. This context is used from various callbacks
            _sessionContextID = GetNextSessionTMHandleId();
            AddSessionTransportManager(_sessionContextID, this);
            // session is implicitly assumed to support disconnect
            SupportsDisconnect = true;
            // Create Callback
            _connectSessionCallback = new WSManNativeApi.WSManShellAsync(new IntPtr(_sessionContextID), s_sessionConnectCallback);
                    // the transport is already closed..so no need to connect
                Dbg.Assert(_startMode == WSManTransportManagerUtils.tmStartModes.None, "startMode is not in expected state");
                _startMode = WSManTransportManagerUtils.tmStartModes.Connect;
                WSManNativeApi.WSManConnectShellEx(_wsManSessionHandle,
                    ConnectionInfo.ShellUri,
                    RunspacePoolInstanceId.ToString().ToUpperInvariant(),  // wsman is case sensitive wrt shellId. so consistently using upper case
                    _openContent,
                    _connectSessionCallback,
                    ref _wsManShellOperationHandle);
            if (_wsManShellOperationHandle == IntPtr.Zero)
                TransportErrorOccuredEventArgs eventargs = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(WSManAPIData.WSManAPIHandle,
                    new WSManNativeApi.WSManError(),
                    TransportMethodEnum.ConnectShellEx,
                    RemotingErrorIdStrings.ConnectExFailed, this.ConnectionInfo.ComputerName);
                ProcessWSManTransportError(eventargs);
        internal override void StartReceivingData()
                // make sure the transport is not closed.
                if (receiveDataInitiated)
                    tracer.WriteLine("Client Session TM: ReceiveData has already been called.");
                receiveDataInitiated = true;
                tracer.WriteLine("Client Session TM: Placing Receive request using WSManReceiveShellOutputEx");
                    PSEventId.WSManReceiveShellOutputEx,
                    PSOpcode.Receive, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic,
                _receivedFromRemote = new WSManNativeApi.WSManShellAsync(new IntPtr(_sessionContextID), s_sessionReceiveCallback);
                WSManNativeApi.WSManReceiveShellOutputEx(_wsManShellOperationHandle,
                    IntPtr.Zero, 0, WSManAPIData.OutputStreamSet, _receivedFromRemote,
                    ref _wsManReceiveOperationHandle);
        /// Starts connecting to remote end asynchronously. This will result in a WSManCreateShellEx WSMan
        /// async call. By the time this call returns, we will have a valid handle, if the operation
        /// succeeds. Make sure other methods are called only after this method returns. Thread
        /// synchronization is left to the caller.
        /// WSManCreateShellEx failed.
            Dbg.Assert(WSManAPIData != null, "WSManApiData should always be created before session creation.");
            List<WSManNativeApi.WSManOption> shellOptions = new List<WSManNativeApi.WSManOption>(WSManAPIData.CommonOptionSet);
            #region SHIM: Redirection code for protocol version
            if (s_protocolVersionRedirect != null)
                string newProtocolVersion = (string)s_protocolVersionRedirect.DynamicInvoke();
                shellOptions.Clear();
                WSManNativeApi.WSManOption newPrtVOption = new WSManNativeApi.WSManOption();
                newPrtVOption.name = RemoteDataNameStrings.PS_STARTUP_PROTOCOL_VERSION_NAME;
                newPrtVOption.value = newProtocolVersion;
                newPrtVOption.mustComply = true;
                shellOptions.Add(newPrtVOption);
            WSManNativeApi.WSManShellStartupInfo_ManToUn startupInfo =
                new WSManNativeApi.WSManShellStartupInfo_ManToUn(WSManAPIData.InputStreamSet,
                WSManAPIData.OutputStreamSet,
                uIdleTimeout,
                _sessionName);
            // additional content with create shell call. Piggy back first fragment from
            // the dataToBeSent buffer.
                #region SHIM: Redirection code for session data send.
                bool sendContinue = true;
                if (s_sessionSendRedirect != null)
                    object[] arguments = new object[2] { null, additionalData };
                    sendContinue = (bool)s_sessionSendRedirect.DynamicInvoke(arguments);
                    additionalData = (byte[])arguments[0];
                if (!sendContinue)
                        WSManNativeApi.PS_CREATION_XML_TAG,
            // Create the session context information only once.  CreateAsync() can be called multiple
            // times by RetrySessionCreation for flaky networks.
            if (_sessionContextID == 0)
                _createSessionCallback = new WSManNativeApi.WSManShellAsync(new IntPtr(_sessionContextID), s_sessionCreateCallback);
                _createSessionCallbackGCHandle = GCHandle.Alloc(_createSessionCallback);
                        // the transport is already closed..so no need to create a connection
                    _startMode = WSManTransportManagerUtils.tmStartModes.Create;
                    if (_noMachineProfile)
                        WSManNativeApi.WSManOption noProfile = new WSManNativeApi.WSManOption();
                        noProfile.name = WSManNativeApi.NoProfile;
                        noProfile.mustComply = true;
                        noProfile.value = "1";
                        shellOptions.Add(noProfile);
                    int flags = _noCompression ? (int)WSManNativeApi.WSManShellFlag.WSMAN_FLAG_NO_COMPRESSION : 0;
                    using (WSManNativeApi.WSManOptionSet optionSet = new WSManNativeApi.WSManOptionSet(shellOptions.ToArray()))
                        WSManNativeApi.WSManCreateShellEx(_wsManSessionHandle,
                            RunspacePoolInstanceId.ToString().ToUpperInvariant(),
                            startupInfo,
                            optionSet,
                            _createSessionCallback,
                        TransportMethodEnum.CreateShellEx,
                        RemotingErrorIdStrings.ConnectExFailed,
                        this.ConnectionInfo.ComputerName);
                startupInfo.Dispose();
        /// Closes the pending Create,Send,Receive operations and then closes the shell and release all the resources.
        /// The caller should make sure this method is called only after calling ConnectAsync.
            // let other threads release the lock before we clean up the resources.
                if (_startMode == WSManTransportManagerUtils.tmStartModes.None)
                else if (_startMode == WSManTransportManagerUtils.tmStartModes.Create ||
                    _startMode == WSManTransportManagerUtils.tmStartModes.Connect)
                    Dbg.Assert(false, "startMode is in unexpected state");
                // Set boolean indicating that this session is closing.
                    RemoveSessionTransportManager(_sessionContextID);
            // TODO - On unexpected failures on a reconstructed session... we dont want to close server session
                PSEventId.WSManCloseShell,
            _closeSessionCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(_sessionContextID), s_sessionCloseCallback);
            WSManNativeApi.WSManCloseShell(_wsManShellOperationHandle, 0, _closeSessionCompleted);
        /// Adjusts for any variations in different protocol versions. Following changes are considered
        /// - In V2, default max envelope size is 150KB while in V3 it has been changed to 500KB.
        ///   With default configuration remoting from V3 client to V2 server will break as V3 client can send upto 500KB in a single Send packet
        ///   So if server version is known to be V2, we'll downgrade the max env size to 150KB (V2's default) if the current value is 500KB (V3 default)
        /// <param name="serverProtocolVersion">Server negotiated protocol version.</param>
        internal void AdjustForProtocolVariations(Version serverProtocolVersion)
            if (serverProtocolVersion <= RemotingConstants.ProtocolVersion_2_1)
                int maxEnvSize;
                WSManNativeApi.WSManGetSessionOptionAsDword(_wsManSessionHandle,
                    WSManNativeApi.WSManSessionOption.WSMAN_OPTION_MAX_ENVELOPE_SIZE_KB,
                    out maxEnvSize);
                if (maxEnvSize == WSManNativeApi.WSMAN_DEFAULT_MAX_ENVELOPE_SIZE_KB_V3)
                    new WSManNativeApi.WSManDataDWord(WSManNativeApi.WSMAN_DEFAULT_MAX_ENVELOPE_SIZE_KB_V2));
                    // retrieve the packet size again
                    int packetSize;
                        WSManNativeApi.WSManSessionOption.WSMAN_OPTION_SHELL_MAX_DATA_SIZE_PER_MESSAGE_KB,
                        out packetSize);
                    // packet size returned is in KB. Convert this into bytes
                    Fragmentor.FragmentSize = packetSize << 10;
        /// This will close the internal WSMan Session handle. Callers must catch the close
        /// completed event and call Redirect to perform the redirection.
        internal override void PrepareForRedirection()
            Dbg.Assert(!isClosed, "Transport manager must not be closed while preparing for redirection.");
        internal override void Redirect(Uri newUri, RunspaceConnectionInfo connectionInfo)
            CloseSessionAndClearResources();
            tracer.WriteLine("Redirecting to URI: {0}", newUri);
                PSEventId.URIRedirection,
                newUri.ToString());
            Initialize(newUri, (WSManConnectionInfo)connectionInfo);
            // reset startmode
            _startMode = WSManTransportManagerUtils.tmStartModes.None;
            CreateAsync();
        internal override BaseClientCommandTransportManager CreateClientCommandTransportManager(RunspaceConnectionInfo connectionInfo,
            WSManConnectionInfo wsmanConnectionInfo = connectionInfo as WSManConnectionInfo;
            Dbg.Assert(wsmanConnectionInfo != null, "ConnectionInfo must be WSManConnectionInfo");
            WSManClientCommandTransportManager result = new
                WSManClientCommandTransportManager(wsmanConnectionInfo, _wsManShellOperationHandle, cmd, noInput, this);
        /// Initializes the session.
        /// Uri to connect to.
        private void Initialize(Uri connectionUri, WSManConnectionInfo connectionInfo)
            // this will generate: http://ComputerName:port/appname?PSVersion=<version>
            // PSVersion= pattern is needed to make Exchange compatible with PS V2 CTP3
            // release. Using the PSVersion= logic, Exchange R4 server will redirect
            // clients to an R3 endpoint.
            bool isSSLSpecified = false;
            string connectionStr = connectionUri.OriginalString;
            if ((connectionUri == connectionInfo.ConnectionUri) &&
                (connectionInfo.UseDefaultWSManPort))
                connectionStr = WSManConnectionInfo.GetConnectionString(connectionInfo.ConnectionUri,
                    out isSSLSpecified);
            string additionalUriSuffixString = string.Empty;
            if (PSSessionConfigurationData.IsServerManager)
                additionalUriSuffixString = ";MSP=7a83d074-bb86-4e52-aa3e-6cc73cc066c8";
            if (string.IsNullOrEmpty(connectionUri.Query))
                // if there is no query string already, create one..see RFC 3986
                connectionStr = string.Format(
                    "{0}?PSVersion={1}{2}",
                    // Trimming the last '/' as this will allow WSMan to
                    // properly apply URLPrefix.
                    // Ex: http://localhost?PSVersion=2.0 will be converted
                    // to http://localhost:<port>/<urlprefix>?PSVersion=2.0
                    // by WSMan
                    connectionStr.TrimEnd('/'),
                    PSVersionInfo.PSVersion,
                    additionalUriSuffixString);
                // if there is already a query string, append using & .. see RFC 3986
                    "{0};PSVersion={1}{2}",
                    connectionStr,
            WSManNativeApi.BaseWSManAuthenticationCredentials authCredentials;
            // use certificate thumbprint for authentication
            if (connectionInfo.CertificateThumbprint != null)
                authCredentials = new WSManNativeApi.WSManCertificateThumbprintCredentials(connectionInfo.CertificateThumbprint);
                // use credential based authentication
                System.Security.SecureString password = null;
                if ((connectionInfo.Credential != null) && (!string.IsNullOrEmpty(connectionInfo.Credential.UserName)))
                    userName = connectionInfo.Credential.UserName;
                    password = connectionInfo.Credential.Password;
                WSManNativeApi.WSManUserNameAuthenticationCredentials userNameCredentials =
                    new WSManNativeApi.WSManUserNameAuthenticationCredentials(userName,
                        connectionInfo.WSManAuthenticationMechanism);
                authCredentials = userNameCredentials;
            // proxy related data
            WSManNativeApi.WSManUserNameAuthenticationCredentials proxyAuthCredentials = null;
            if (connectionInfo.ProxyCredential != null)
                WSManNativeApi.WSManAuthenticationMechanism authMechanism = WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_NEGOTIATE;
                switch (connectionInfo.ProxyAuthentication)
                        authMechanism = WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_NEGOTIATE;
                        authMechanism = WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_BASIC;
                        authMechanism = WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_DIGEST;
                if (!string.IsNullOrEmpty(connectionInfo.ProxyCredential.UserName))
                    userName = connectionInfo.ProxyCredential.UserName;
                    password = connectionInfo.ProxyCredential.Password;
                proxyAuthCredentials = new WSManNativeApi.WSManUserNameAuthenticationCredentials(userName, password, authMechanism);
            WSManNativeApi.WSManProxyInfo proxyInfo = (connectionInfo.ProxyAccessType == ProxyAccessType.None) ?
                null :
                new WSManNativeApi.WSManProxyInfo(connectionInfo.ProxyAccessType, proxyAuthCredentials);
                result = WSManNativeApi.WSManCreateSession(WSManAPIData.WSManAPIHandle, connectionStr, 0,
                     authCredentials.GetMarshalledObject(),
                     (proxyInfo == null) ? IntPtr.Zero : (IntPtr)proxyInfo,
                     ref _wsManSessionHandle);
                // release resources
                proxyAuthCredentials?.Dispose();
                proxyInfo?.Dispose();
                authCredentials?.Dispose();
            // set the packet size for this session
            // packet size returned is in KB. Convert this into bytes..
            // Get robust connections maximum retries time.
                WSManNativeApi.WSManSessionOption.WSMAN_OPTION_MAX_RETRY_TIME,
                out _maxRetryTime);
            this.dataToBeSent.Fragmentor = base.Fragmentor;
            _noCompression = !connectionInfo.UseCompression;
            _noMachineProfile = connectionInfo.NoMachineProfile;
            // set other WSMan session related defaults
                // WSMan Port DCR related changes - BUG 542726
                // this session option will tell WSMan to use port for HTTPS from
                // config provider.
                SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_USE_SSL, 1);
            // Explicitly disallow Basic auth over HTTP on Unix.
            if (connectionInfo.AuthenticationMechanism == AuthenticationMechanism.Basic && !isSSLSpecified && connectionUri.Scheme != Uri.UriSchemeHttps)
                throw new PSRemotingTransportException(PSRemotingErrorId.ConnectFailed, RemotingErrorIdStrings.BasicAuthOverHttpNotSupported);
            // The OMI client distributed with PowerShell does not support validating server certificates on Unix.
            // Check if third-party psrpclient and MI support the verification.
            // If WSManGetSessionOptionAsDword does not return 0 then it's not supported.
            bool verificationAvailable = WSManNativeApi.WSManGetSessionOptionAsDword(_wsManSessionHandle,
                WSManNativeApi.WSManSessionOption.WSMAN_OPTION_SKIP_CA_CHECK, out _) == 0;
            if (isSSLSpecified && !verificationAvailable && (!connectionInfo.SkipCACheck || !connectionInfo.SkipCNCheck))
                throw new PSRemotingTransportException(PSRemotingErrorId.ConnectSkipCheckFailed, RemotingErrorIdStrings.UnixOnlyHttpsWithoutSkipCACheckNotSupported);
            if (connectionInfo.NoEncryption)
                // send unencrypted messages
                SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_UNENCRYPTED_MESSAGES, 1);
            // check if implicit credentials can be used for Negotiate
            if (connectionInfo.AllowImplicitCredentialForNegotiate)
                result = WSManNativeApi.WSManSetSessionOption(_wsManSessionHandle,
                    WSManNativeApi.WSManSessionOption.WSMAN_OPTION_ALLOW_NEGOTIATE_IMPLICIT_CREDENTIALS,
                    new WSManNativeApi.WSManDataDWord(1));
            if (connectionInfo.UseUTF16)
                SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_UTF16, 1);
            if (connectionInfo.SkipCACheck)
                SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_SKIP_CA_CHECK, 1);
            if (connectionInfo.SkipCNCheck)
                SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_SKIP_CN_CHECK, 1);
            if (connectionInfo.SkipRevocationCheck)
                SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_SKIP_REVOCATION_CHECK, 1);
            if (connectionInfo.IncludePortInSPN)
                SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_ENABLE_SPN_SERVER_PORT, 1);
            // Set use interactive token flag based on EnableNetworkAccess property.
            SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_USE_INTERACTIVE_TOKEN,
                (connectionInfo.EnableNetworkAccess) ? 1 : 0);
            // set UI Culture for this session from current thread's UI Culture
            string currentUICulture = connectionInfo.UICulture.Name;
            if (!string.IsNullOrEmpty(currentUICulture))
                // WSMan API cannot handle empty culture names
                SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_UI_LANGUAGE, currentUICulture);
            // set Culture for this session from current thread's Culture
            string currentCulture = connectionInfo.Culture.Name;
            if (!string.IsNullOrEmpty(currentCulture))
                SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_LOCALE, currentCulture);
            // set the PowerShell specific default client timeouts
            SetDefaultTimeOut(connectionInfo.OperationTimeout);
            SetConnectTimeOut(connectionInfo.OpenTimeout);
            SetCloseTimeOut(connectionInfo.CancelTimeout);
            SetSignalTimeOut(connectionInfo.CancelTimeout);
        /// Handle transport error - calls EnqueueAndStartProcessingThread to process transport exception
        /// in a different thread
        /// Logic in transport callbacks should always use this to process a transport error.
        internal void ProcessWSManTransportError(TransportErrorOccuredEventArgs eventArgs)
            EnqueueAndStartProcessingThread(null, eventArgs, null);
        /// Log the error message in the Crimson logger and raise error handler.
        public override void RaiseErrorHandler(TransportErrorOccuredEventArgs eventArgs)
            // Look for a valid stack trace.
            string stackTrace;
            if (!string.IsNullOrEmpty(eventArgs.Exception.StackTrace))
                stackTrace = eventArgs.Exception.StackTrace;
            else if (eventArgs.Exception.InnerException != null &&
                     !string.IsNullOrEmpty(eventArgs.Exception.InnerException.StackTrace))
                stackTrace = eventArgs.Exception.InnerException.StackTrace;
                stackTrace = string.Empty;
            // Write errors into both Operational and Analytical channels
            PSEtwLog.LogOperationalError(
                PSEventId.TransportError, PSOpcode.Open, PSTask.None, PSKeyword.UseAlwaysOperational,
                eventArgs.Exception.ErrorCode.ToString(CultureInfo.InvariantCulture),
                eventArgs.Exception.Message,
                PSEventId.TransportError_Analytic,
                PSOpcode.Open, PSTask.None,
            base.RaiseErrorHandler(eventArgs);
        /// Receive/send operation handles and callback handles should be released/disposed from
        /// receive/send callback only. Releasing them after CloseOperation() may not cover all
        /// the scenarios, as WSMan does not guarantee that a rcv/send callback is not called after
        /// Close completed callback.
        /// <param name="shouldClearSend"></param>
        internal void ClearReceiveOrSendResources(int flags, bool shouldClearSend)
            if (shouldClearSend)
                if (_sendToRemoteCompleted != null)
                    _sendToRemoteCompleted.Dispose();
                    _sendToRemoteCompleted = null;
                // For send..clear always
                if (_wsManSendOperationHandle != IntPtr.Zero)
                    WSManNativeApi.WSManCloseOperation(_wsManSendOperationHandle, 0);
                    _wsManSendOperationHandle = IntPtr.Zero;
                // clearing for receive..Clear only when the end of operation is reached.
                if (flags == (int)WSManNativeApi.WSManCallbackFlags.WSMAN_FLAG_CALLBACK_END_OF_OPERATION)
                    if (_wsManReceiveOperationHandle != IntPtr.Zero)
                        WSManNativeApi.WSManCloseOperation(_wsManReceiveOperationHandle, 0);
                        _wsManReceiveOperationHandle = IntPtr.Zero;
                    if (_receivedFromRemote != null)
                        _receivedFromRemote.Dispose();
                        _receivedFromRemote = null;
        /// Call back from worker thread / queue to raise Robust Connection notification event.
        /// <param name="privateData">ConnectionStatusEventArgs.</param>
            // Raise the Robust
            ConnectionStatusEventArgs rcArgs = privateData as ConnectionStatusEventArgs;
            if (rcArgs != null)
                RaiseRobustConnectionNotification(rcArgs);
            CompletionEventArgs completionArgs = privateData as CompletionEventArgs;
            if (completionArgs != null)
                switch (completionArgs.Notification)
                    case CompletionNotification.DisconnectCompleted:
                        RaiseDisconnectCompleted();
                        Dbg.Assert(false, "Currently only DisconnectCompleted notification is handled on the worker thread queue.");
            Dbg.Assert(false, "Worker thread callback should always have ConnectionStatusEventArgs or CompletionEventArgs type for privateData.");
        /// Robust connection maximum retry time in milliseconds.
            get { return _maxRetryTime; }
        /// Returns the WSMan's session handle that this Session transportmanager
        /// is proxying.
        internal IntPtr SessionHandle
            get { return _wsManSessionHandle; }
        /// Returns the WSManConnectionInfo used to make the connection.
        internal WSManConnectionInfo ConnectionInfo { get; private set; }
        /// Examine the session create error code and if the error is one where a
        /// session create/connect retry attempt may be beneficial then do the
        /// retry attempt.
        /// <param name="sessionCreateErrorCode">Error code returned from Create response.</param>
        /// <returns>True if a session create retry has been started.</returns>
        private bool RetrySessionCreation(int sessionCreateErrorCode)
            if (_connectionRetryCount >= ConnectionInfo.MaxConnectionRetryCount)
            bool retryConnect;
            switch (sessionCreateErrorCode)
                // Continue with connect retry for these errors.
                case WSManNativeApi.ERROR_WSMAN_SENDDATA_CANNOT_CONNECT:
                case WSManNativeApi.ERROR_WSMAN_OPERATION_ABORTED:
                case WSManNativeApi.ERROR_WSMAN_IMPROPER_RESPONSE:
                case WSManNativeApi.ERROR_WSMAN_URL_NOTAVAILABLE:
                case WSManNativeApi.ERROR_WSMAN_CANNOT_CONNECT_INVALID:
                case WSManNativeApi.ERROR_WSMAN_CANNOT_CONNECT_MISMATCH:
                case WSManNativeApi.ERROR_WSMAN_HTTP_SERVICE_UNAVAILABLE:
                case WSManNativeApi.ERROR_WSMAN_HTTP_SERVICE_ERROR:
                    retryConnect = true;
                // For any other errors don't do connect retry.
                    retryConnect = false;
            if (retryConnect)
                ++_connectionRetryCount;
                // Write trace output
                tracer.WriteLine("Attempting session creation retry {0} for error code {1} on session Id {2}",
                    _connectionRetryCount, sessionCreateErrorCode, RunspacePoolInstanceId);
                // Create ETW log entry
                    PSEventId.RetrySessionCreation, PSOpcode.Open, PSTask.None,
                    _connectionRetryCount.ToString(CultureInfo.InvariantCulture),
                    sessionCreateErrorCode.ToString(CultureInfo.InvariantCulture),
                // Use worker pool thread to initiate retry, since WSMan does not allow method
                // calls on its own call back thread.
                System.Threading.ThreadPool.QueueUserWorkItem(StartCreateRetry);
            return retryConnect;
        private void StartCreateRetry(object state)
            // Begin new session create attempt.
        #region Static Callbacks from WSMan
        // callback that gets called when createshellex returns.
        private static void OnCreateSessionCallback(IntPtr operationContext,
            IntPtr data)
            tracer.WriteLine("Client Session TM: CreateShell callback received");
            long sessionTMHandle = 0;
            WSManClientSessionTransportManager sessionTM = null;
            if (!TryGetSessionTransportManager(operationContext, out sessionTM, out sessionTMHandle))
                // We dont have the session TM handle..just return.
                tracer.WriteLine("Unable to find a transport manager for context {0}.", sessionTMHandle);
            // This callback is also used for robust connection notifications.  Check for and
            // handle these notifications here.
            if (HandleRobustConnectionCallback(flags, sessionTM))
                PSEventId.WSManCreateShellCallbackReceived,
                sessionTM.RunspacePoolInstanceId.ToString());
            // TODO: 188098 wsManShellOperationHandle should be populated by WSManCreateShellEx,
            // but there is a thread timing bug in WSMan layer causing the callback to
            // be called before WSManCreateShellEx returns. since we already validated the
            // shell context exists, safely assigning the shellOperationHandle to shell transport manager.
            // Remove this once WSMan fixes its code.
            sessionTM._wsManShellOperationHandle = shellOperationHandle;
            lock (sessionTM.syncObject)
                // Already close request is made. So return
                if (sessionTM.isClosed)
            if (error != IntPtr.Zero)
                WSManNativeApi.WSManError errorStruct = WSManNativeApi.WSManError.UnMarshal(error);
                if (errorStruct.errorCode != 0)
                    tracer.WriteLine("Got error with error code {0}. Message {1}", errorStruct.errorCode.ToString(), errorStruct.errorDetail);
                    // Test error code for possible session connection retry.
                    if (sessionTM.RetrySessionCreation(errorStruct.errorCode))
                        // If a connection retry is being attempted (on
                        // another thread) then return without processing error.
                    TransportErrorOccuredEventArgs eventargs = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(
                        sessionTM.WSManAPIData.WSManAPIHandle,
                        sessionTM,
                        errorStruct,
                        RemotingErrorIdStrings.ConnectExCallBackError,
                        new object[] { sessionTM.ConnectionInfo.ComputerName, WSManTransportManagerUtils.ParseEscapeWSManErrorMessage(errorStruct.errorDetail) });
                    sessionTM.ProcessWSManTransportError(eventargs);
            // check if the session supports disconnect
            sessionTM.SupportsDisconnect = (flags & (int)WSManNativeApi.WSManCallbackFlags.WSMAN_FLAG_CALLBACK_SHELL_SUPPORTS_DISCONNECT) != 0;
            // openContent is used by redirection ie., while redirecting to
            // a new machine.. this is not needed anymore as the connection
            // is successfully established.
            if (sessionTM._openContent != null)
                sessionTM._openContent.Dispose();
                sessionTM._openContent = null;
                WSManNativeApi.WSManCreateShellDataResult shellData = WSManNativeApi.WSManCreateShellDataResult.UnMarshal(data);
                if (shellData.data != null)
                    string returnXml = shellData.data;
                    sessionTM.ProcessShellData(returnXml);
                // Successfully made a connection. Now report this by raising the CreateCompleted event.
                // Pass updated connection information to event.
                sessionTM.RaiseCreateCompleted(
                    new CreateCompleteEventArgs(sessionTM.ConnectionInfo.Copy()));
                // Since create shell is successful, put a receive request.
                sessionTM.StartReceivingData();
            // Start sending data if any.
            sessionTM.SendOneItem();
        // callback that gets called when closeshellex returns.
        private static void OnCloseSessionCompleted(IntPtr operationContext,
            tracer.WriteLine("Client Session TM: CloseShell callback received");
                PSEventId.WSManCloseShellCallbackReceived,
                sessionTM.RunspacePoolInstanceId.ToString(),
                "OnCloseSessionCompleted");
                        TransportMethodEnum.CloseShellOperationEx,
                        RemotingErrorIdStrings.CloseExCallBackError,
                        new object[] { WSManTransportManagerUtils.ParseEscapeWSManErrorMessage(errorStruct.errorDetail) });
                    sessionTM.RaiseErrorHandler(eventargs);
            sessionTM.RaiseCloseCompleted();
        private static void OnRemoteSessionDisconnectCompleted(IntPtr operationContext,
            // LOG ETW EVENTS
                "OnRemoteSessionDisconnectCompleted");
            // Dispose the OnDisconnect callback as it is not needed anymore
            if (sessionTM._disconnectSessionCompleted != null)
                sessionTM._disconnectSessionCompleted.Dispose();
                sessionTM._disconnectSessionCompleted = null;
                        TransportMethodEnum.DisconnectShellEx,
                        RemotingErrorIdStrings.DisconnectShellExFailed,
                // successfully made a connection. Now report this by raising the ConnectCompleted event.
                sessionTM.EnqueueAndStartProcessingThread(null, null,
                    new CompletionEventArgs(CompletionNotification.DisconnectCompleted));
                // Log ETW traces                
                    "OnRemoteSessionReconnectCompleted: DisconnectCompleted");
        private static void OnRemoteSessionReconnectCompleted(IntPtr operationContext,
            // Add ETW events
                "OnRemoteSessionReconnectCompleted");
            // Dispose the OnCreate callback as it is not needed anymore
            if (sessionTM._reconnectSessionCompleted != null)
                sessionTM._reconnectSessionCompleted.Dispose();
                sessionTM._reconnectSessionCompleted = null;
                        TransportMethodEnum.ReconnectShellEx,
                        RemotingErrorIdStrings.ReconnectShellExCallBackErrr,
                sessionTM.RaiseReconnectCompleted();
        private static bool HandleRobustConnectionCallback(int flags, WSManClientSessionTransportManager sessionTM)
            if (flags != (int)WSManNativeApi.WSManCallbackFlags.WSMAN_FLAG_CALLBACK_SHELL_AUTODISCONNECTED &&
                flags != (int)WSManNativeApi.WSManCallbackFlags.WSMAN_FLAG_CALLBACK_NETWORK_FAILURE_DETECTED &&
                flags != (int)WSManNativeApi.WSManCallbackFlags.WSMAN_FLAG_CALLBACK_RETRYING_AFTER_NETWORK_FAILURE &&
                flags != (int)WSManNativeApi.WSManCallbackFlags.WSMAN_FLAG_CALLBACK_RECONNECTED_AFTER_NETWORK_FAILURE &&
                flags != (int)WSManNativeApi.WSManCallbackFlags.WSMAN_FLAG_CALLBACK_SHELL_AUTODISCONNECTING &&
                flags != (int)WSManNativeApi.WSManCallbackFlags.WSMAN_FLAG_CALLBACK_RETRY_ABORTED_DUE_TO_INTERNAL_ERROR)
            // Raise transport event notifying start of robust connections.
            if (flags == (int)WSManNativeApi.WSManCallbackFlags.WSMAN_FLAG_CALLBACK_NETWORK_FAILURE_DETECTED)
                    sessionTM.RobustConnectionsInitiated.SafeInvoke(sessionTM, EventArgs.Empty);
            // Send robust notification to client.
            sessionTM.QueueRobustConnectionNotification(flags);
            // Raise transport event notifying completion of robust connections.
            if (flags == (int)WSManNativeApi.WSManCallbackFlags.WSMAN_FLAG_CALLBACK_SHELL_AUTODISCONNECTED ||
                flags == (int)WSManNativeApi.WSManCallbackFlags.WSMAN_FLAG_CALLBACK_RECONNECTED_AFTER_NETWORK_FAILURE ||
                flags == (int)WSManNativeApi.WSManCallbackFlags.WSMAN_FLAG_CALLBACK_RETRY_ABORTED_DUE_TO_INTERNAL_ERROR)
                    sessionTM.RobustConnectionsCompleted.SafeInvoke(sessionTM, EventArgs.Empty);
        private static void OnRemoteSessionConnectCallback(IntPtr operationContext,
            tracer.WriteLine("Client Session TM: Connect callback received");
                PSEventId.WSManSendShellInputExCallbackReceived,
                "OnRemoteSessionConnectCallback:Client Session TM: Connect callback received");
            // dispose openContent
            // process returned Xml
            Dbg.Assert(data != IntPtr.Zero, "WSManConnectShell callback returned null data");
            WSManNativeApi.WSManConnectDataResult connectData = WSManNativeApi.WSManConnectDataResult.UnMarshal(data);
            if (connectData.data != null)
                byte[] connectResponse = ServerOperationHelpers.ExtractEncodedXmlElement(connectData.data, WSManNativeApi.PS_CONNECTRESPONSE_XML_TAG);
                sessionTM.ProcessRawData(connectResponse, WSManNativeApi.WSMAN_STREAM_ID_STDOUT);
            // Set up the data-to-send callback.
            // Microsoft's PS 3.0 Server will return all negotiation related data in one shot in connect Data
            // Note that we are not starting to receive data yet. the DSHandlers above will do that when the session
            // gets to an established state.
            sessionTM.RaiseConnectCompleted();
        private static void OnRemoteSessionSendCompleted(IntPtr operationContext,
            tracer.WriteLine("Client Session TM: SendComplete callback received");
            // do the logging for this send
                Guid.Empty.ToString());
            if (!shellOperationHandle.Equals(sessionTM._wsManShellOperationHandle))
                // WSMan returned data from a wrong shell..notify the caller
                // about the same.
                PSRemotingTransportException e = new PSRemotingTransportException(
                    PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.SendExFailed, sessionTM.ConnectionInfo.ComputerName));
                    new TransportErrorOccuredEventArgs(e, TransportMethodEnum.SendShellInputEx);
            sessionTM.ClearReceiveOrSendResources(flags, true);
            // if the session is already closed ignore the errors and return.
                // Ignore operation aborted error. operation aborted is raised by WSMan to
                // notify operation complete. PowerShell protocol has its own
                // way of notifying the same using state change events.
                if ((errorStruct.errorCode != 0) && (errorStruct.errorCode != 995))
                        TransportMethodEnum.SendShellInputEx,
                        RemotingErrorIdStrings.SendExCallBackError,
            // Send the next item, if available
        // WSMan will make sure this callback is synchronously called ie., if 1 callback
        // is active, the callback will not be called from a different thread.
        private static void OnRemoteSessionDataReceived(IntPtr operationContext,
            tracer.WriteLine("Client Session TM: OnRemoteDataReceived callback.");
            sessionTM.ClearReceiveOrSendResources(flags, false);
                    PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.ReceiveExFailed, sessionTM.ConnectionInfo.ComputerName));
                        TransportMethodEnum.ReceiveShellOutputEx,
                        RemotingErrorIdStrings.ReceiveExCallBackError,
            WSManNativeApi.WSManReceiveDataResult dataReceived = WSManNativeApi.WSManReceiveDataResult.UnMarshal(data);
            if (dataReceived.data != null)
                tracer.WriteLine("Session Received Data : {0}", dataReceived.data.Length);
                    dataReceived.data.Length.ToString(CultureInfo.InvariantCulture));
                sessionTM.ProcessRawData(dataReceived.data, dataReceived.stream);
        #region Send Data Handling
            tracer.WriteLine("Session sending data of size : {0}", data.Length);
            byte[] package = data;
                object[] arguments = new object[2] { null, package };
                package = (byte[])arguments[0];
            using (WSManNativeApi.WSManData_ManToUn serializedContent =
                         new WSManNativeApi.WSManData_ManToUn(package))
                    serializedContent.BufferLength.ToString(CultureInfo.InvariantCulture));
                    // send callback
                    _sendToRemoteCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(_sessionContextID), s_sessionSendCallback);
                    WSManNativeApi.WSManSendShellInputEx(_wsManShellOperationHandle, IntPtr.Zero, 0,
                        priorityType == DataPriorityType.Default ?
                            WSManNativeApi.WSMAN_STREAM_ID_STDIN : WSManNativeApi.WSMAN_STREAM_ID_PROMPTRESPONSE,
                        serializedContent,
                        _sendToRemoteCompleted,
                        ref _wsManSendOperationHandle);
        #region Dispose / Destructor pattern
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
            tracer.WriteLine("Disposing session with session context: {0} Operation Context: {1}", _sessionContextID, _wsManShellOperationHandle);
            DisposeWSManAPIDataAsync();
            // a new machine and hence this is cleared only when the session
            // is disposing.
            if (isDisposing && (_openContent != null))
                _openContent.Dispose();
                _openContent = null;
        /// Closes current session handle by calling WSManCloseSession and clears
        /// session related resources.
        private void CloseSessionAndClearResources()
            tracer.WriteLine("Clearing session with session context: {0} Operation Context: {1}", _sessionContextID, _wsManShellOperationHandle);
            // Taking a copy of session handle as we should call WSManCloseSession only once and
            // clear the original value. This will protect us if Dispose() is called twice.
            IntPtr tempWSManSessionHandle = _wsManSessionHandle;
            _wsManSessionHandle = IntPtr.Zero;
            // Call WSManCloseSession on a different thread as Dispose can be called from one of
            // the WSMan callback threads. WSMan does not support closing a session in the callbacks.
                // wsManSessionHandle is passed as parameter to allow the thread to be independent
                // of the rest of the parent object.
                    IntPtr sessionHandle = (IntPtr)state;
                    if (sessionHandle != IntPtr.Zero)
                        WSManNativeApi.WSManCloseSession(sessionHandle, 0);
                }), tempWSManSessionHandle);
            // remove session context from session handles dictionary
            if (_closeSessionCompleted != null)
                _closeSessionCompleted.Dispose();
                _closeSessionCompleted = null;
            // Dispose the create session completed callback here, since it is
            // used for periodic robust connection retry/auto-disconnect
            // notifications while the shell is active.
            if (_createSessionCallback != null)
                _createSessionCallbackGCHandle.Free();
                _createSessionCallback.Dispose();
                _createSessionCallback = null;
            // Dispose the OnConnect callback if one present
            if (_connectSessionCallback != null)
                _connectSessionCallback.Dispose();
                _connectSessionCallback = null;
            // Reset the session context Id to zero so that a new one will be generated for
            // any following redirected session.
            _sessionContextID = 0;
        private void DisposeWSManAPIDataAsync()
            WSManAPIDataCommon tempWSManApiData = WSManAPIData;
            if (tempWSManApiData == null)
            WSManAPIData = null;
            // Dispose and de-initialize the WSManAPIData instance object on separate worker thread to ensure
            // it is not run on a WinRM thread (which will fail).
            // Note that WSManAPIData.Dispose() method is thread safe.
            ThreadPool.QueueUserWorkItem((_) => tempWSManApiData.Dispose());
        #region WSManAPIDataCommon
        /// Class that manages WSManAPI data. Has information like APIHandle which is created
        /// using WSManInitialize, InputStreamSet, OutputStreamSet.
        internal class WSManAPIDataCommon : IDisposable
            private IntPtr _handle;
            // if any
            private WSManNativeApi.WSManStreamIDSet_ManToUn _inputStreamSet;
            private WSManNativeApi.WSManStreamIDSet_ManToUn _outputStreamSet;
            // Dispose
            private readonly WindowsIdentity _identityToImpersonate;
            /// Initializes handle by calling WSManInitialize API.
            internal WSManAPIDataCommon()
                // Check for thread impersonation and save identity for later de-initialization.
                _handle = IntPtr.Zero;
                    ErrorCode = WSManNativeApi.WSManInitialize(WSManNativeApi.WSMAN_FLAG_REQUESTED_API_VERSION_1_1, ref _handle);
                catch (DllNotFoundException ex)
                        PSEventId.TransportError,
                        "WSManAPIDataCommon.ctor",
                        "WSManInitialize",
                        ex.HResult.ToString(CultureInfo.InvariantCulture),
                        ex.Message,
                        ex.StackTrace);
                    throw new PSRemotingTransportException(RemotingErrorIdStrings.WSManClientDllNotAvailable, ex);
                // input / output streams common to all connections
                _inputStreamSet = new WSManNativeApi.WSManStreamIDSet_ManToUn(
                        WSManNativeApi.WSMAN_STREAM_ID_PROMPTRESPONSE
                _outputStreamSet = new WSManNativeApi.WSManStreamIDSet_ManToUn(
                    new string[] { WSManNativeApi.WSMAN_STREAM_ID_STDOUT });
                // startup options common to all connections
                WSManNativeApi.WSManOption protocolStartupOption = new WSManNativeApi.WSManOption();
                protocolStartupOption.name = RemoteDataNameStrings.PS_STARTUP_PROTOCOL_VERSION_NAME;
                protocolStartupOption.value = RemotingConstants.ProtocolVersion.ToString();
                protocolStartupOption.mustComply = true;
                CommonOptionSet = new List<WSManNativeApi.WSManOption>();
                CommonOptionSet.Add(protocolStartupOption);
            internal int ErrorCode { get; }
            internal WSManNativeApi.WSManStreamIDSet_ManToUn InputStreamSet { get { return _inputStreamSet; } }
            internal WSManNativeApi.WSManStreamIDSet_ManToUn OutputStreamSet { get { return _outputStreamSet; } }
            internal List<WSManNativeApi.WSManOption> CommonOptionSet { get; }
            internal IntPtr WSManAPIHandle { get { return _handle; } }
            // Suppress this message. The result is actually used, but only in checked builds....
               MessageId = "System.Management.Automation.Remoting.Client.WSManNativeApi.WSManDeinitialize(System.IntPtr,System.Int32)")]
            [SuppressMessage("Microsoft.Usage", "CA2216:Disposabletypesshoulddeclarefinalizer")]
                _inputStreamSet.Dispose();
                _outputStreamSet.Dispose();
                if (_handle != IntPtr.Zero)
                    // If we initialized with thread impersonation, make sure de-initialize is run with the same.
                        result = WindowsIdentity.RunImpersonated(
                            () => WSManNativeApi.WSManDeinitialize(_handle, 0));
                        result = WSManNativeApi.WSManDeinitialize(_handle, 0);
                    Dbg.Assert(result == 0, "WSManDeinitialize returned non-zero value");
        internal event EventHandler<EventArgs> RobustConnectionsInitiated;
        internal event EventHandler<EventArgs> RobustConnectionsCompleted;
    /// A class maintaining the transport of a command for the shell. Multiple commands will have
    /// multiple transport managers. The Transport manager manages creating / sending /receiving
    /// data and closing (terminating) the command.
    internal sealed class WSManClientCommandTransportManager : BaseClientCommandTransportManager
        internal const string StopSignal = @"powershell/signal/crtl_c";
        // operation handles
        private readonly IntPtr _wsManShellOperationHandle;
        private IntPtr _wsManCmdOperationHandle;
        private IntPtr _cmdSignalOperationHandle;
        // this is used with WSMan callbacks to represent a command transport manager.
        private long _cmdContextId;
        // should be integrated with receiveDataInitiated
        private bool _shouldStartReceivingData;
        // bools used to track and send stop signal only after Create is completed.
        private bool _isCreateCallbackReceived;
        private bool _isStopSignalPending;
        private bool _isDisconnectPending;
        private bool _isSendingInput;
        private bool _isDisconnectedOnInvoke;
        private WSManNativeApi.WSManShellAsync _createCmdCompleted;
        private WSManNativeApi.WSManShellAsync _reconnectCmdCompleted;
        private WSManNativeApi.WSManShellAsync _connectCmdCompleted;
        private GCHandle _createCmdCompletedGCHandle;
        private WSManNativeApi.WSManShellAsync _closeCmdCompleted;
        private WSManNativeApi.WSManShellAsync _signalCmdCompleted;
        // this is the chunk that got delivered on onDataAvailableToSendCallback
        // will be sent during subsequent SendOneItem()
        private SendDataChunk _chunkToSend;
        private readonly string _cmdLine;
        private readonly WSManClientSessionTransportManager _sessnTm;
        private sealed class SendDataChunk
            public SendDataChunk(byte[] data, DataPriorityType type)
            public byte[] Data { get; }
            public DataPriorityType Type { get; }
        private static WSManNativeApi.WSManShellAsyncCallback s_cmdCreateCallback;
        private static WSManNativeApi.WSManShellAsyncCallback s_cmdCloseCallback;
        private static WSManNativeApi.WSManShellAsyncCallback s_cmdReceiveCallback;
        private static WSManNativeApi.WSManShellAsyncCallback s_cmdSendCallback;
        private static WSManNativeApi.WSManShellAsyncCallback s_cmdSignalCallback;
        private static WSManNativeApi.WSManShellAsyncCallback s_cmdReconnectCallback;
        private static WSManNativeApi.WSManShellAsyncCallback s_cmdConnectCallback;
        static WSManClientCommandTransportManager()
                new WSManNativeApi.WSManShellCompletionFunction(OnCreateCmdCompleted);
            s_cmdCreateCallback = new WSManNativeApi.WSManShellAsyncCallback(createDelegate);
                new WSManNativeApi.WSManShellCompletionFunction(OnCloseCmdCompleted);
            s_cmdCloseCallback = new WSManNativeApi.WSManShellAsyncCallback(closeDelegate);
                new WSManNativeApi.WSManShellCompletionFunction(OnRemoteCmdDataReceived);
            s_cmdReceiveCallback = new WSManNativeApi.WSManShellAsyncCallback(receiveDelegate);
                new WSManNativeApi.WSManShellCompletionFunction(OnRemoteCmdSendCompleted);
            s_cmdSendCallback = new WSManNativeApi.WSManShellAsyncCallback(sendDelegate);
            WSManNativeApi.WSManShellCompletionFunction signalDelegate =
                new WSManNativeApi.WSManShellCompletionFunction(OnRemoteCmdSignalCompleted);
            s_cmdSignalCallback = new WSManNativeApi.WSManShellAsyncCallback(signalDelegate);
                new WSManNativeApi.WSManShellCompletionFunction(OnReconnectCmdCompleted);
            s_cmdReconnectCallback = new WSManNativeApi.WSManShellAsyncCallback(reconnectDelegate);
                new WSManNativeApi.WSManShellCompletionFunction(OnConnectCmdCompleted);
            s_cmdConnectCallback = new WSManNativeApi.WSManShellAsyncCallback(connectDelegate);
        /// This is an internal constructor used by WSManClientSessionTransportManager.
        /// connection info to be used for creating the command.
        /// <param name="wsManShellOperationHandle">
        /// Shell operation handle in whose context this transport manager sends/receives
        /// data packets.
        /// <param name="shell">
        /// The command to be sent to the remote end.
        /// true if the command has input, false otherwise.
        /// <param name="sessnTM">
        /// Session transport manager creating this command transport manager instance.
        /// Used by Command TM to apply session specific properties
        internal WSManClientCommandTransportManager(
            IntPtr wsManShellOperationHandle,
            ClientRemotePowerShell shell,
            WSManClientSessionTransportManager sessnTM)
            : base(shell, sessnTM.CryptoHelper, sessnTM)
            Dbg.Assert(wsManShellOperationHandle != IntPtr.Zero, "Shell operation handle cannot be IntPtr.Zero.");
            _wsManShellOperationHandle = wsManShellOperationHandle;
            // Apply quota limits.. allow for data to be unlimited..
            ReceivedDataCollection.MaximumReceivedDataSize = connectionInfo.MaximumReceivedDataSizePerCommand;
            _cmdLine = shell.PowerShell.Commands.Commands.GetCommandStringForHistory();
            _sessnTm = sessnTM;
            // Suspend queue on robust connections initiated event.
            sessnTM.RobustConnectionsInitiated += HandleRobustConnectionsInitiated;
            // Resume queue on robust connections completed event.
            sessnTM.RobustConnectionsCompleted += HandleRobusConnectionsCompleted;
        #region SHIM: Redirection delegate for command code send.
        private static readonly Delegate s_commandCodeSendRedirect = null;
        private void HandleRobustConnectionsInitiated(object sender, EventArgs e)
            SuspendQueue();
        private void HandleRobusConnectionsCompleted(object sender, EventArgs e)
            ResumeQueue();
        /// WSManConnectShellCommandEx failed.
            // Empty the serializedPipeline data that contains PowerShell command information created in the
            // constructor.  We are connecting to an existing command on the server and don't want to send
            // information on a new command.
            serializedPipeline.Read();
            // create cmdContextId
            _cmdContextId = GetNextCmdTMHandleId();
            AddCmdTransportManager(_cmdContextId, this);
            _connectCmdCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(_cmdContextId), s_cmdConnectCallback);
            _reconnectCmdCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(_cmdContextId), s_cmdReconnectCallback);
                WSManNativeApi.WSManConnectShellCommandEx(_wsManShellOperationHandle,
                    PowershellInstanceId.ToString().ToUpperInvariant(),
                    _connectCmdCompleted,
                    ref _wsManCmdOperationHandle);
            if (_wsManCmdOperationHandle == IntPtr.Zero)
                PSRemotingTransportException e = new PSRemotingTransportException(RemotingErrorIdStrings.RunShellCommandExFailed);
                    new TransportErrorOccuredEventArgs(e, TransportMethodEnum.ConnectShellCommandEx);
        /// Begin connection creation.
        /// WSManRunShellCommandEx failed.
            byte[] cmdPart1 = serializedPipeline.ReadOrRegisterCallback(null);
            if (cmdPart1 != null)
                #region SHIM: Redirection code for command code send.
                if (s_commandCodeSendRedirect != null)
                    object[] arguments = new object[2] { null, cmdPart1 };
                    sendContinue = (bool)s_commandCodeSendRedirect.DynamicInvoke(arguments);
                    cmdPart1 = (byte[])arguments[0];
                WSManNativeApi.WSManCommandArgSet argSet = new WSManNativeApi.WSManCommandArgSet(cmdPart1);
                    PSEventId.WSManCreateCommand,
                    powershellInstanceId.ToString());
                _createCmdCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(_cmdContextId), s_cmdCreateCallback);
                _createCmdCompletedGCHandle = GCHandle.Alloc(_createCmdCompleted);
                using (argSet)
                        if (!isClosed)
                            WSManNativeApi.WSManRunShellCommandEx(_wsManShellOperationHandle,
                                // WSManRunsShellCommand doesn't accept empty string "".
                                (_cmdLine == null || _cmdLine.Length == 0) ? " " : (_cmdLine.Length <= 256 ? _cmdLine : _cmdLine.Substring(0, 255)),
                                argSet,
                                _createCmdCompleted,
                            tracer.WriteLine("Started cmd with command context : {0} Operation context: {1}", _cmdContextId, _wsManCmdOperationHandle);
                    new TransportErrorOccuredEventArgs(e, TransportMethodEnum.RunShellCommandEx);
        /// Restores connection on a disconnected command.
                WSManNativeApi.WSManReconnectShellCommandEx(_wsManCmdOperationHandle, 0, _reconnectCmdCompleted);
                // WSMan API do not allow a signal/input/receive be sent until RunShellCommand is
                // successful (ie., callback is received)..so note that a signal is to be sent
                // here and return.
                if (!_isCreateCallbackReceived)
                    _isStopSignalPending = true;
                // we are about to send a signal..so clear pending bit.
                _isStopSignalPending = false;
                tracer.WriteLine("Sending stop signal with command context: {0} Operation Context {1}", _cmdContextId, _wsManCmdOperationHandle);
                    PSEventId.WSManSignal,
                    StopSignal);
                _signalCmdCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(_cmdContextId), s_cmdSignalCallback);
                WSManNativeApi.WSManSignalShellEx(_wsManShellOperationHandle, _wsManCmdOperationHandle, 0,
                    StopSignal, _signalCmdCompleted, ref _cmdSignalOperationHandle);
            tracer.WriteLine("Closing command with command context: {0} Operation Context {1}", _cmdContextId, _wsManCmdOperationHandle);
            // then let other threads release the lock before we cleaning up the resources.
                // There is no valid cmd operation handle..so just
                // raise close completed.
                    RemoveCmdTransportManager(_cmdContextId);
                PSEventId.WSManCloseCommand,
            _closeCmdCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(_cmdContextId), s_cmdCloseCallback);
            Dbg.Assert((IntPtr)_closeCmdCompleted != IntPtr.Zero, "closeCmdCompleted callback is null in cmdTM.CloseAsync()");
            WSManNativeApi.WSManCloseCommand(_wsManCmdOperationHandle, 0, _closeCmdCompleted);
                PSEventId.TransportError, PSOpcode.Open, PSTask.None,
                PSEventId.TransportError_Analytic, PSOpcode.Open, PSTask.None,
        internal override void PrepareForDisconnect()
            _isDisconnectPending = true;
            // If there is not input processing and the command has already been created
            // on the server then this object is ready for Disconnect now.
            // Otherwise let the sending input data call back handle it.
            if (this.isClosed || _isDisconnectedOnInvoke ||
                (_isCreateCallbackReceived &&
                 this.serializedPipeline.Length == 0 &&
                 !_isSendingInput))
                RaiseReadyForDisconnect();
        internal override void PrepareForConnect()
            _isDisconnectPending = false;
        #region Callbacks from WSMan
        // callback that gets called when WSManRunShellCommandEx returns.
        private static void OnCreateCmdCompleted(IntPtr operationContext,
            tracer.WriteLine("OnCreateCmdCompleted callback received");
            long cmdContextId = 0;
            WSManClientCommandTransportManager cmdTM = null;
            if (!TryGetCmdTransportManager(operationContext, out cmdTM, out cmdContextId))
                // We dont have the command TM handle..just return.
                tracer.WriteLine("OnCreateCmdCompleted: Unable to find a transport manager for the command context {0}.", cmdContextId);
                PSEventId.WSManCreateCommandCallbackReceived,
                cmdTM.RunspacePoolInstanceId.ToString(),
                cmdTM.powershellInstanceId.ToString());
            // dispose the cmdCompleted callback as it is not needed any more
            if (cmdTM._createCmdCompleted != null)
                cmdTM._createCmdCompletedGCHandle.Free();
                cmdTM._createCmdCompleted.Dispose();
                cmdTM._createCmdCompleted = null;
            // TODO: 188098 wsManCmdOperationHandle should be populated by WSManRunShellCommandEx,
            // be called before WSManRunShellCommandEx returns. since we already validated the
            // cmd context exists, safely assigning the commandOperationHandle to cmd transport manager.
            cmdTM._wsManCmdOperationHandle = commandOperationHandle;
                    tracer.WriteLine("OnCreateCmdCompleted callback: WSMan reported an error: {0}", errorStruct.errorDetail);
                        cmdTM._sessnTm.WSManAPIData.WSManAPIHandle,
                        TransportMethodEnum.RunShellCommandEx,
                        RemotingErrorIdStrings.RunShellCommandExCallBackError,
                    cmdTM.ProcessWSManTransportError(eventargs);
            // Send remaining cmd / parameter fragments.
            lock (cmdTM.syncObject)
                cmdTM._isCreateCallbackReceived = true;
                if (cmdTM.isClosed)
                    if (cmdTM._isDisconnectPending)
                        cmdTM.RaiseReadyForDisconnect();
                // If a disconnect is pending at this point then we should not start
                // receiving data or sending input and let the disconnect take place.
                if (cmdTM.serializedPipeline.Length == 0)
                    cmdTM._shouldStartReceivingData = true;
                cmdTM.SendOneItem();
                // WSMan API does not allow a signal/input/receive be sent until RunShellCommand is
                // successful (ie., callback is received)
                if (cmdTM._isStopSignalPending)
                    cmdTM.SendStopSignal();
        private static void OnConnectCmdCompleted(IntPtr operationContext,
            tracer.WriteLine("OnConnectCmdCompleted callback received");
                "OnConnectCmdCompleted: OnConnectCmdCompleted callback received");
                tracer.WriteLine("OnConnectCmdCompleted: Unable to find a transport manager for the command context {0}.", cmdContextId);
            if (cmdTM._connectCmdCompleted != null)
                cmdTM._connectCmdCompleted.Dispose();
                cmdTM._connectCmdCompleted = null;
                    tracer.WriteLine("OnConnectCmdCompleted callback: WSMan reported an error: {0}", errorStruct.errorDetail);
                        TransportMethodEnum.ReconnectShellCommandEx,
                        RemotingErrorIdStrings.ReconnectShellCommandExCallBackError,
                // If the transport is already closed then we are done.
                    // Release disconnect pending, if any.
                // Allow SendStopSignal.
                // Send stop signal if it is pending.
            // Establish a client data to server callback so that the client can respond to prompts.
            cmdTM.RaiseConnectCompleted();
            cmdTM.StartReceivingData();
        private static void OnCloseCmdCompleted(IntPtr operationContext,
            tracer.WriteLine("OnCloseCmdCompleted callback received for operation context {0}", commandOperationHandle);
                PSEventId.WSManCloseCommandCallbackReceived,
                "OnCloseCmdCompleted: OnCloseCmdCompleted callback received");
                tracer.WriteLine("OnCloseCmdCompleted: Unable to find a transport manager for the command context {0}.", cmdContextId);
            tracer.WriteLine("Close completed callback received for command: {0}", cmdTM._cmdContextId);
            cmdTM.RaiseCloseCompleted();
        private static void OnRemoteCmdSendCompleted(IntPtr operationContext,
            tracer.WriteLine("SendComplete callback received");
                "OnRemoteCmdSendCompleted: SendComplete callback received");
                tracer.WriteLine("Unable to find a transport manager for the command context {0}.", cmdContextId);
            cmdTM._isSendingInput = false;
            if ((!shellOperationHandle.Equals(cmdTM._wsManShellOperationHandle)) ||
                (!commandOperationHandle.Equals(cmdTM._wsManCmdOperationHandle)))
                tracer.WriteLine("SendShellInputEx callback: ShellOperationHandles are not the same as the Send is initiated with");
                PSRemotingTransportException e = new PSRemotingTransportException(RemotingErrorIdStrings.CommandSendExFailed);
                    new TransportErrorOccuredEventArgs(e, TransportMethodEnum.CommandInputEx);
            // release the resources related to send
            cmdTM.ClearReceiveOrSendResources(flags, true);
            // if the transport manager is already closed..ignore the errors and return
                // Ignore Command aborted error. Command aborted is raised by WSMan to
                // notify command operation complete. PowerShell protocol has its own
                    tracer.WriteLine("CmdSend callback: WSMan reported an error: {0}", errorStruct.errorDetail);
                        TransportMethodEnum.CommandInputEx,
                        RemotingErrorIdStrings.CommandSendExCallBackError,
        private static void OnRemoteCmdDataReceived(IntPtr operationContext,
            tracer.WriteLine("Remote Command DataReceived callback.");
                PSEventId.WSManReceiveShellOutputExCallbackReceived,
                "OnRemoteCmdDataReceived: Remote Command DataReceived callback");
                tracer.WriteLine("Unable to find a transport manager for the given command context {0}.", cmdContextId);
                tracer.WriteLine("CmdReceive callback: ShellOperationHandles are not the same as the Receive is initiated with");
                PSRemotingTransportException e = new PSRemotingTransportException(RemotingErrorIdStrings.CommandReceiveExFailed);
                    new TransportErrorOccuredEventArgs(e, TransportMethodEnum.ReceiveCommandOutputEx);
            // release the resources related to receive
            cmdTM.ClearReceiveOrSendResources(flags, false);
                    tracer.WriteLine("CmdReceive callback: WSMan reported an error: {0}", errorStruct.errorDetail);
                        TransportMethodEnum.ReceiveCommandOutputEx,
                        RemotingErrorIdStrings.CommandReceiveExCallBackError,
                        new object[] { errorStruct.errorDetail });
            if (flags == (int)WSManNativeApi.WSManCallbackFlags.WSMAN_FLAG_RECEIVE_DELAY_STREAM_REQUEST_PROCESSED)
                cmdTM._isDisconnectedOnInvoke = true;
                cmdTM.RaiseDelayStreamProcessedEvent();
                tracer.WriteLine("Cmd Received Data : {0}", dataReceived.data.Length);
                    cmdTM.powershellInstanceId.ToString(),
                cmdTM.ProcessRawData(dataReceived.data, dataReceived.stream);
        private static void OnReconnectCmdCompleted(IntPtr operationContext,
                "OnReconnectCmdCompleted");
                tracer.WriteLine("Cmd Signal callback: ShellOperationHandles are not the same as the signal is initiated with");
                PSRemotingTransportException e = new PSRemotingTransportException(RemotingErrorIdStrings.ReconnectShellCommandExCallBackError);
                    new TransportErrorOccuredEventArgs(e, TransportMethodEnum.ReconnectShellCommandEx);
                    tracer.WriteLine("OnReconnectCmdCompleted callback: WSMan reported an error: {0}", errorStruct.errorDetail);
            // The command may have been disconnected before all input was read or
            // the returned command data started to be received.
            cmdTM.RaiseReconnectCompleted();
        private static void OnRemoteCmdSignalCompleted(IntPtr operationContext,
            tracer.WriteLine("Signal Completed callback received.");
            PSEtwLog.LogAnalyticInformational(PSEventId.WSManSignalCallbackReceived, PSOpcode.Disconnect, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, "OnRemoteCmdSignalCompleted");
                PSEventId.WSManSignalCallbackReceived,
            // release the resources related to signal
            if (cmdTM._cmdSignalOperationHandle != IntPtr.Zero)
                WSManNativeApi.WSManCloseOperation(cmdTM._cmdSignalOperationHandle, 0);
                cmdTM._cmdSignalOperationHandle = IntPtr.Zero;
            if (cmdTM._signalCmdCompleted != null)
                cmdTM._signalCmdCompleted.Dispose();
                cmdTM._signalCmdCompleted = null;
                    tracer.WriteLine("Cmd Signal callback: WSMan reported an error: {0}", errorStruct.errorDetail);
            cmdTM.EnqueueAndStartProcessingThread(null, null, true);
            // If a disconnect is completing then do not send any more data.
            // Also raise the readyfordisconnect event.
            if (_isDisconnectPending)
                // if there are no command / parameter fragments need to be sent
                // start receiving data. Reason: Command will start its execution
                // once command string + parameters are sent.
                if (serializedPipeline.Length == 0)
                    _shouldStartReceivingData = true;
            else if (_chunkToSend != null) // there is a pending chunk to be sent
                data = _chunkToSend.Data;
                priorityType = _chunkToSend.Type;
                _chunkToSend = null;
                _isSendingInput = true;
            if (_shouldStartReceivingData)
                StartReceivingData();
            Dbg.Assert(_chunkToSend == null, "data callback received while a chunk is pending to be sent");
            _chunkToSend = new SendDataChunk(data, priorityType);
        #region SHIM: Redirection delegate for command data send.
        private static readonly Delegate s_commandSendRedirect = null;
            tracer.WriteLine("Command sending data of size : {0}", data.Length);
            #region SHIM: Redirection code for command data send.
            if (s_commandSendRedirect != null)
                sendContinue = (bool)s_commandSendRedirect.DynamicInvoke(arguments);
                    _sendToRemoteCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(_cmdContextId), s_cmdSendCallback);
                    WSManNativeApi.WSManSendShellInputEx(_wsManShellOperationHandle, _wsManCmdOperationHandle, 0,
            // We should call Receive only once.. WSMan will call the callback multiple times.
            _shouldStartReceivingData = false;
                // receive callback
                _receivedFromRemote = new WSManNativeApi.WSManShellAsync(new IntPtr(_cmdContextId), s_cmdReceiveCallback);
                    _wsManCmdOperationHandle, startInDisconnectedMode ? (int)WSManNativeApi.WSManShellFlag.WSMAN_FLAG_RECEIVE_DELAY_OUTPUT_STREAM : 0,
                   _sessnTm.WSManAPIData.OutputStreamSet,
                   _receivedFromRemote, ref _wsManReceiveOperationHandle);
            tracer.WriteLine("Disposing command with command context: {0} Operation Context: {1}", _cmdContextId, _wsManCmdOperationHandle);
            // remove command context from cmd handles dictionary
            // unregister event handlers
            if (_sessnTm != null)
                _sessnTm.RobustConnectionsInitiated -= HandleRobustConnectionsInitiated;
                _sessnTm.RobustConnectionsCompleted -= HandleRobusConnectionsCompleted;
            if (_closeCmdCompleted != null)
                _closeCmdCompleted.Dispose();
                _closeCmdCompleted = null;
            if (_reconnectCmdCompleted != null)
                _reconnectCmdCompleted.Dispose();
                _reconnectCmdCompleted = null;
            _wsManCmdOperationHandle = IntPtr.Zero;
        #region Static Data / Methods
        // This dictionary maintains active command transport managers to be used from various
        private static readonly Dictionary<long, WSManClientCommandTransportManager> s_cmdTMHandles =
            new Dictionary<long, WSManClientCommandTransportManager>();
        private static long s_cmdTMSeed;
        // Generate command transport manager unique id
        private static long GetNextCmdTMHandleId()
            return System.Threading.Interlocked.Increment(ref s_cmdTMSeed);
        private static void AddCmdTransportManager(long cmdTMId,
            WSManClientCommandTransportManager cmdTransportManager)
            lock (s_cmdTMHandles)
                s_cmdTMHandles.Add(cmdTMId, cmdTransportManager);
        private static void RemoveCmdTransportManager(long cmdTMId)
                s_cmdTMHandles.Remove(cmdTMId);
        private static bool TryGetCmdTransportManager(IntPtr operationContext,
            out WSManClientCommandTransportManager cmdTransportManager,
            out long cmdTMId)
            cmdTMId = operationContext.ToInt64();
            cmdTransportManager = null;
                return s_cmdTMHandles.TryGetValue(cmdTMId, out cmdTransportManager);

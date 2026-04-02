// ----------------------------------------------------------------------
//  Contents:  Entry points for managed PowerShell plugin worker used to
//  host powershell in a WSMan service.
using System.Management.Automation.Remoting.WSMan;
    /// Consolidation of constants for uniformity.
    internal static class WSManPluginConstants
        internal const int ExitCodeSuccess = 0x00000000;
        internal const int ExitCodeFailure = 0x00000001;
        internal const string CtrlCSignal = "powershell/signal/crtl_c";
        // The following are the only supported streams in PowerShell remoting.
        // see WSManNativeApi.cs. These are duplicated here to save on
        // Marshalling time.
        internal const string SupportedInputStream = "stdin";
        internal const string SupportedOutputStream = "stdout";
        internal const string SupportedPromptResponseStream = "pr";
        internal const string PowerShellStartupProtocolVersionName = "protocolversion";
        internal const string PowerShellStartupProtocolVersionValue = "2.0";
        internal const string PowerShellOptionPrefix = "PS_";
        internal const int WSManPluginParamsGetRequestedLocale = 5;
        internal const int WSManPluginParamsGetRequestedDataLocale = 6;
    /// Definitions of HRESULT error codes that are passed to the client.
    /// 0x8054.... means that it is a PowerShell HRESULT. The PowerShell facility
    /// is 84 (0x54).
    internal enum WSManPluginErrorCodes : int
        NullPluginContext = -2141976624, // 0x805407D0
        PluginContextNotFound = -2141976623, // 0x805407D1
        NullInvalidInput = -2141975624, // 0x80540BB8
        NullInvalidStreamSets = -2141975623, // 0x80540BB9
        SessionCreationFailed = -2141975622, // 0x80540BBA
        NullShellContext = -2141975621, // 0x80540BBB
        InvalidShellContext = -2141975620, // 0x80540BBC
        InvalidCommandContext = -2141975619, // 0x80540BBD
        InvalidInputStream = -2141975618, // 0x80540BBE
        InvalidInputDatatype = -2141975617, // 0x80540BBF
        InvalidOutputStream = -2141975616, // 0x80540BC0
        InvalidSenderDetails = -2141975615, // 0x80540BC1
        ShutdownRegistrationFailed = -2141975614, // 0x80540BC2
        ReportContextFailed = -2141975613, // 0x80540BC3
        InvalidArgSet = -2141975612, // 0x80540BC4
        ProtocolVersionNotMatch = -2141975611, // 0x80540BC5
        OptionNotUnderstood = -2141975610, // 0x80540BC6
        ProtocolVersionNotFound = -2141975609, // 0x80540BC7
        ManagedException = -2141974624, // 0x80540FA0
        PluginOperationClose = -2141974623, // 0x80540FA1
        PluginConnectNoNegotiationData = -2141974622, // 0x80540FA2
        PluginConnectOperationFailed = -2141974621, // 0x80540FA3
        OutOfMemory = -2147024882  // 0x8007000E
    /// Class that holds plugin + shell context information used to handle
    /// shutdown notifications.
    /// Explicit destruction and release of the IntPtrs is not required because
    /// their lifetime is managed by WinRM.
    internal class WSManPluginOperationShutdownContext // TODO: Rename to OperationShutdownContext when removing the MC++ module.
        internal IntPtr pluginContext;
        internal IntPtr shellContext;
        internal IntPtr commandContext;
        internal bool isReceiveOperation;
        internal bool isShuttingDown;
        internal WSManPluginOperationShutdownContext(
            IntPtr plgContext,
            IntPtr shContext,
            IntPtr cmdContext,
            bool isRcvOp)
            pluginContext = plgContext;
            shellContext = shContext;
            commandContext = cmdContext;
            isReceiveOperation = isRcvOp;
            isShuttingDown = false;
    /// Represents the logical grouping of all actions required to handle the
    /// lifecycle of shell sessions through the WinRM plugin.
    internal class WSManPluginInstance
        private readonly Dictionary<IntPtr, WSManPluginShellSession> _activeShellSessions;
        private static readonly Dictionary<IntPtr, WSManPluginInstance> s_activePlugins = new Dictionary<IntPtr, WSManPluginInstance>();
        /// Enables dependency injection after the static constructor is called.
        /// This may be overridden in unit tests to enable different behavior.
        /// It is static because static instances of this class use the facade. Otherwise,
        /// it would be passed in via a parameterized constructor.
        internal static readonly IWSManNativeApiFacade wsmanPinvokeStatic = new WSManNativeApiFacade();
        #region Constructor and Destructor
        internal WSManPluginInstance()
            _activeShellSessions = new Dictionary<IntPtr, WSManPluginShellSession>();
        /// Static constructor to listen to unhandled exceptions
        /// from the AppDomain and log the errors
        /// Note: It is not necessary to instantiate IWSManNativeApi here because it is not used.
        static WSManPluginInstance()
            // NOTE - the order is important here:
            // because handler from WindowsErrorReporting is going to terminate the process
            // we want it to fire last
            // Register our remoting handler for crashes
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException +=
                new UnhandledExceptionEventHandler(WSManPluginInstance.UnhandledExceptionHandler);
        /// Create a new shell in the plugin context.
        /// <param name="pluginContext"></param>
        /// <param name="requestDetails"></param>
        /// <param name="extraInfo"></param>
        /// <param name="inboundShellInformation"></param>
        internal void CreateShell(
            IntPtr pluginContext,
            WSManNativeApi.WSManPluginRequest requestDetails,
            string extraInfo,
            WSManNativeApi.WSManShellStartupInfo_UnToMan startupInfo,
            WSManNativeApi.WSManData_UnToMan inboundShellInformation)
                PSEventId.ServerCreateRemoteSession,
                PSOpcode.Connect,
                PSKeyword.ManagedPlugin | PSKeyword.UseAlwaysAnalytic,
                "CreateShell: Create a new shell in the plugin context",
            if (requestDetails == null)
                // Nothing can be done because requestDetails are required to report operation complete
                PSEtwLog.LogAnalyticInformational(PSEventId.ReportOperationComplete,
                    PSOpcode.Close, PSTask.None,
                    Convert.ToString(WSManPluginErrorCodes.NullInvalidInput, CultureInfo.InvariantCulture),
                        RemotingErrorIdStrings.WSManPluginNullInvalidInput,
                        "requestDetails",
                        "WSManPluginShell"),
            if ((requestDetails.senderDetails == null) ||
                (requestDetails.operationInfo == null))
                ReportOperationComplete(
                    requestDetails,
                    WSManPluginErrorCodes.NullInvalidInput,
                        "WSManPluginShell"));
            if (startupInfo == null)
                        "startupInfo",
                requestDetails.ToString(),
                "CreateShell: NULL checks being performed",
            if ((startupInfo.inputStreamSet.streamIDsCount == 0) || (startupInfo.outputStreamSet.streamIDsCount == 0))
                    WSManPluginErrorCodes.NullInvalidStreamSets,
                        RemotingErrorIdStrings.WSManPluginNullInvalidStreamSet,
                        WSManPluginConstants.SupportedInputStream,
                        WSManPluginConstants.SupportedOutputStream));
            if (string.IsNullOrEmpty(extraInfo))
                        "extraInfo",
            WSManPluginInstance.SetThreadProperties(requestDetails);
            // check if protocolversion option is honored
            if (!EnsureOptionsComply(requestDetails))
            int result = WSManPluginConstants.ExitCodeSuccess;
            WSManPluginShellSession mgdShellSession;
            WSManPluginOperationShutdownContext context;
            byte[] convertedBase64 = null;
                PSSenderInfo senderInfo = GetPSSenderInfo(requestDetails.senderDetails);
                // inbound shell information is already verified by pwrshplugin.dll.. so no need
                // to verify here.
                WSManPluginServerTransportManager serverTransportMgr;
                    serverTransportMgr = new WSManPluginServerTransportManager(BaseTransportManager.DefaultFragmentSize, new PSRemotingCryptoHelperServer());
                    serverTransportMgr = new WSManPluginServerTransportManager(BaseTransportManager.DefaultFragmentSize, null);
                    requestDetails.ToString(), senderInfo.UserInfo.Identity.Name, requestDetails.resourceUri);
                ServerRemoteSession remoteShellSession = ServerRemoteSession.CreateServerRemoteSession(
                    senderInfo: senderInfo,
                    configurationProviderId: requestDetails.resourceUri,
                    initializationParameters: extraInfo,
                    transportManager: serverTransportMgr,
                    initialCommand: null,       // Not used by WinRM endpoint.
                    configurationName: null,    // Not used by WinRM endpoint, which has its own configuration.
                    configurationFile: null,    // Same.
                    initialLocation: null);     // Same.
                if (remoteShellSession == null)
                    WSManPluginInstance.ReportWSManOperationComplete(
                        WSManPluginErrorCodes.SessionCreationFailed);
                context = new WSManPluginOperationShutdownContext(pluginContext, requestDetails.unmanagedHandle, IntPtr.Zero, false);
                    ReportOperationComplete(requestDetails, WSManPluginErrorCodes.OutOfMemory);
                // Create a shell session wrapper to track and service future interactions.
                mgdShellSession = new WSManPluginShellSession(requestDetails, serverTransportMgr, remoteShellSession, context);
                AddToActiveShellSessions(mgdShellSession);
                mgdShellSession.SessionClosed += HandleShellSessionClosed;
                if (inboundShellInformation != null)
                    if (inboundShellInformation.Type != (uint)WSManNativeApi.WSManDataType.WSMAN_DATA_TYPE_TEXT)
                        // only text data is supported
                            WSManPluginErrorCodes.InvalidInputDatatype,
                                RemotingErrorIdStrings.WSManPluginInvalidInputDataType,
                                "WSMAN_DATA_TYPE_TEXT"));
                        DeleteFromActiveShellSessions(requestDetails.unmanagedHandle);
                        convertedBase64 = ServerOperationHelpers.ExtractEncodedXmlElement(
                            inboundShellInformation.Text,
                            WSManNativeApi.PS_CREATION_XML_TAG);
                // now report the shell context to WSMan.
                    PSEventId.ReportContext,
                    requestDetails.ToString(), requestDetails.ToString());
                result = wsmanPinvokeStatic.WSManPluginReportContext(requestDetails.unmanagedHandle, 0, requestDetails.unmanagedHandle);
                if (result != WSManPluginConstants.ExitCodeSuccess)
                        WSManPluginErrorCodes.ReportContextFailed,
                                RemotingErrorIdStrings.WSManPluginReportContextFailed));
            catch (System.Exception e)
                PSEtwLog.LogOperationalError(PSEventId.TransportError,
                    PSOpcode.Connect, PSTask.None, PSKeyword.UseAlwaysOperational, "00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000",
                    Convert.ToString(WSManPluginErrorCodes.ManagedException, CultureInfo.InvariantCulture), e.Message, e.StackTrace);
                    WSManPluginErrorCodes.ManagedException,
                        RemotingErrorIdStrings.WSManPluginManagedException,
            bool isRegisterWaitForSingleObjectSucceeded = true;
            // always synchronize calls to OperationComplete once notification handle is registered.. else duplicate OperationComplete calls are bound to happen
            lock (mgdShellSession.shellSyncObject)
                mgdShellSession.registeredShutdownNotification = 1;
                // Wrap the provided handle so it can be passed to the registration function
                EventWaitHandle eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                    SafeWaitHandle safeWaitHandle = new SafeWaitHandle(requestDetails.shutdownNotificationHandle, false); // Owned by WinRM
                    eventWaitHandle.SafeWaitHandle = safeWaitHandle;
                    // On non-windows platforms the shutdown notification is done through a callback instead of a windows event handle.
                    // Register the callback and this will then signal the event. Note, the gch object is deleted in the shell shutdown
                    // notification that will always come in to shut down the operation.
                    GCHandle gch = GCHandle.Alloc(eventWaitHandle);
                    IntPtr p = GCHandle.ToIntPtr(gch);
                    wsmanPinvokeStatic.WSManPluginRegisterShutdownCallback(
                                                           requestDetails.unmanagedHandle,
                                                           WSManPluginManagedEntryWrapper.workerPtrs.UnmanagedStruct.wsManPluginShutdownCallbackNative,
                                                           p);
                mgdShellSession.registeredShutDownWaitHandle = ThreadPool.RegisterWaitForSingleObject(
                                 eventWaitHandle,
                                 new WaitOrTimerCallback(WSManPluginManagedEntryWrapper.PSPluginOperationShutdownCallback),
                                 -1, // INFINITE
                                 true); // TODO: Do I need to worry not being able to set missing WT_TRANSFER_IMPERSONATION?
                if (mgdShellSession.registeredShutDownWaitHandle == null)
                    isRegisterWaitForSingleObjectSucceeded = false;
            if (!isRegisterWaitForSingleObjectSucceeded)
                mgdShellSession.registeredShutdownNotification = 0;
                    WSManPluginErrorCodes.ShutdownRegistrationFailed);
                if (convertedBase64 != null)
                    mgdShellSession.SendOneItemToSessionHelper(convertedBase64, WSManPluginConstants.SupportedInputStream);
                if (Interlocked.Exchange(ref mgdShellSession.registeredShutdownNotification, 0) == 1)
                    // unregister callback.. wait for any ongoing callbacks to complete.. nothing much we could do if this fails
                    bool ignore = mgdShellSession.registeredShutDownWaitHandle.Unregister(null);
                    mgdShellSession.registeredShutDownWaitHandle = null;
                    // this will called OperationComplete
                    PerformCloseOperation(context);
                "CreateShell: Completed",
        /// This gets called on a thread pool thread once Shutdown wait handle is notified.
        internal void CloseShellOperation(
            WSManPluginOperationShutdownContext context)
                PSEventId.ServerCloseOperation,
                PSOpcode.Disconnect,
                ((IntPtr)context.shellContext).ToString(),
                ((IntPtr)context.commandContext).ToString(),
                context.isReceiveOperation.ToString());
            WSManPluginShellSession mgdShellSession = GetFromActiveShellSessions(context.shellContext);
            if (mgdShellSession == null)
                // this should never be the case. this will protect the service.
                // Dbg.Assert(false, "context.shellContext not matched");
            // update the internal data store only if this is not receive operation.
            if (!context.isReceiveOperation)
                DeleteFromActiveShellSessions(context.shellContext);
            System.Exception reasonForClose = new System.Exception(RemotingErrorIdStrings.WSManPluginOperationClose);
            mgdShellSession.CloseOperation(context, reasonForClose);
                "CloseShellOperation: Completed",
        internal void CloseCommandOperation(
                context.shellContext.ToString(),
                context.commandContext.ToString(),
            mgdShellSession.CloseCommandOperation(context);
                "CloseCommandOperation: Completed",
        /// Adds shell session to activeShellSessions store and returns the id
        /// at which the session is added.
        /// <param name="newShellSession"></param>
        private void AddToActiveShellSessions(
            WSManPluginShellSession newShellSession)
            int count = -1;
                IntPtr key = newShellSession.creationRequestDetails.unmanagedHandle;
                Dbg.Assert(key != IntPtr.Zero, "NULL handles should not be provided");
                if (!_activeShellSessions.ContainsKey(key))
                    _activeShellSessions.Add(key, newShellSession);
                    // trigger an event outside the lock
                    count = _activeShellSessions.Count;
            if (count != -1)
                // Raise session count changed event
                WSManServerChannelEvents.RaiseActiveSessionsChangedEvent(new ActiveSessionsChangedEventArgs(count));
        /// Retrieves a WSManPluginShellSession if matched.
        /// <param name="key">Shell context (WSManPluginRequest.unmanagedHandle).</param>
        /// <returns>Null WSManPluginShellSession if not matched. The object if matched.</returns>
        private WSManPluginShellSession GetFromActiveShellSessions(
            IntPtr key)
                WSManPluginShellSession result;
                _activeShellSessions.TryGetValue(key, out result);
        /// Removes a WSManPluginShellSession from tracking.
        /// <param name="keyToDelete">IntPtr of a WSManPluginRequest structure.</param>
        private void DeleteFromActiveShellSessions(
            IntPtr keyToDelete)
                if (_activeShellSessions.Remove(keyToDelete))
        /// Triggers a shell close from an event handler.
        /// <param name="source">Shell context.</param>
        private void HandleShellSessionClosed(
            EventArgs e)
            DeleteFromActiveShellSessions((IntPtr)source);
        /// Helper function to validate incoming values.
        /// <param name="shellContext"></param>
        /// <param name="inputFunctionName"></param>
        private static bool validateIncomingContexts(
            IntPtr shellContext,
            string inputFunctionName)
                    PSEventId.ReportOperationComplete,
                        inputFunctionName),
            if (shellContext == IntPtr.Zero)
                    WSManPluginErrorCodes.NullShellContext,
                        RemotingErrorIdStrings.WSManPluginNullShellContext,
                        "ShellContext",
                        inputFunctionName));
        /// Create a new command in the shell context.
        /// <param name="commandLine"></param>
        /// <param name="arguments"></param>
        internal void CreateCommand(
            WSManNativeApi.WSManCommandArgSet arguments)
                PSEventId.ServerCreateCommandSession,
                "CreateCommand: Create a new command in the shell context",
            if (!validateIncomingContexts(requestDetails, shellContext, "WSManRunShellCommandEx"))
            SetThreadProperties(requestDetails);
                ((IntPtr)shellContext).ToString(), requestDetails.ToString());
            WSManPluginShellSession mgdShellSession = GetFromActiveShellSessions(shellContext);
                    WSManPluginErrorCodes.InvalidShellContext,
                        RemotingErrorIdStrings.WSManPluginInvalidShellContext));
            mgdShellSession.CreateCommand(pluginContext, requestDetails, flags, commandLine, arguments);
                "CreateCommand: Create a new command in the shell context completed",
        internal void StopCommand(
            IntPtr commandContext)
                        "StopCommand"),
                PSEventId.ServerStopCommand,
                ((IntPtr)shellContext).ToString(),
                ((IntPtr)commandContext).ToString(),
                requestDetails.ToString());
            WSManPluginCommandSession mgdCommandSession = mgdShellSession.GetCommandSession(commandContext);
            if (mgdCommandSession == null)
                    WSManPluginErrorCodes.InvalidCommandContext,
                        RemotingErrorIdStrings.WSManPluginInvalidCommandContext));
            mgdCommandSession.Stop(requestDetails);
                "StopCommand: completed",
        internal void Shutdown()
                PSEventId.WSManPluginShutdown,
                PSOpcode.ShuttingDown, PSTask.None,
                PSKeyword.ManagedPlugin | PSKeyword.UseAlwaysAnalytic);
            // all active shells should be closed at this point
            Dbg.Assert(_activeShellSessions.Count == 0, "All active shells should be closed");
            // raise shutting down notification
            WSManServerChannelEvents.RaiseShuttingDownEvent();
        /// Connect.
        /// <param name="commandContext"></param>
        /// <param name="inboundConnectInformation"></param>
        internal void ConnectShellOrCommand(
            IntPtr commandContext,
            WSManNativeApi.WSManData_UnToMan inboundConnectInformation)
                PSEventId.ServerReceivedData,
                "ConnectShellOrCommand: Connect",
            if (!validateIncomingContexts(requestDetails, shellContext, "ConnectShellOrCommand"))
            // TODO... What does this mean from a new client that has specified diff locale from original client?
            if (commandContext == IntPtr.Zero)
                mgdShellSession.ExecuteConnect(requestDetails, flags, inboundConnectInformation);
            // this connect is on a command
            WSManPluginCommandSession mgdCmdSession = mgdShellSession.GetCommandSession(commandContext);
            if (mgdCmdSession == null)
            mgdCmdSession.ExecuteConnect(requestDetails, flags, inboundConnectInformation);
                "ConnectShellOrCommand: ExecuteConnect invoked",
        /// Send data to the shell / command specified.
        /// <param name="inboundData"></param>
        internal void SendOneItemToShellOrCommand(
            WSManNativeApi.WSManData_UnToMan inboundData)
                PSOpcode.Open,
                "SendOneItemToShellOrCommand: Send data to the shell / command specified",
            if (!validateIncomingContexts(requestDetails, shellContext, "SendOneItemToShellOrCommand"))
                        RemotingErrorIdStrings.WSManPluginInvalidShellContext)
                // the data is destined for shell (runspace) session. so let shell handle it
                mgdShellSession.SendOneItemToSession(requestDetails, flags, stream, inboundData);
            // the data is destined for command.
            mgdCmdSession.SendOneItemToSession(requestDetails, flags, stream, inboundData);
                "SendOneItemToShellOrCommand: SendOneItemToSession invoked",
        /// Unlock the shell / command specified so that the shell / command
        /// starts sending data to the client.
        /// <param name="streamSet"></param>
        internal void EnableShellOrCommandToSendDataToClient(
            WSManNativeApi.WSManStreamIDSet_UnToMan streamSet)
                PSEventId.ServerClientReceiveRequest,
                "EnableShellOrCommandToSendDataToClient: unlock the shell / command specified so that the shell / command starts sending data to the client.",
            if (!validateIncomingContexts(requestDetails, shellContext, "EnableShellOrCommandToSendDataToClient"))
            WSManPluginOperationShutdownContext ctxtToReport = new WSManPluginOperationShutdownContext(pluginContext, shellContext, IntPtr.Zero, true);
            if (ctxtToReport == null)
                "EnableShellOrCommandToSendDataToClient: Instruction destined to shell or for command",
                // the instruction is destined for shell (runspace) session. so let shell handle it
                if (mgdShellSession.EnableSessionToSendDataToClient(requestDetails, flags, streamSet, ctxtToReport))
                // the instruction is destined for command
                ctxtToReport.commandContext = commandContext;
                if (mgdCmdSession.EnableSessionToSendDataToClient(requestDetails, flags, streamSet, ctxtToReport))
        /// Used to create PSPrincipal object from senderDetails struct.
        /// <param name="senderDetails"></param>
        private static PSSenderInfo GetPSSenderInfo(
            WSManNativeApi.WSManSenderDetails senderDetails)
            // senderDetails will not be null.
            Dbg.Assert(senderDetails != null, "senderDetails cannot be null");
            // Construct PSIdentity
            PSCertificateDetails psCertDetails = null;
            // Construct Certificate Details
            if (senderDetails.certificateDetails != null)
                psCertDetails = new PSCertificateDetails(
                    senderDetails.certificateDetails.subject,
                    senderDetails.certificateDetails.issuerName,
                    senderDetails.certificateDetails.issuerThumbprint);
            // Construct PSPrincipal
            PSIdentity psIdentity = new PSIdentity(senderDetails.authenticationMechanism, true, senderDetails.senderName, psCertDetails);
            // For Virtual and RunAs accounts WSMan specifies the client token via an environment variable and
            // senderDetails.clientToken should not be used.
            IntPtr clientToken = GetRunAsClientToken();
            clientToken = (clientToken != IntPtr.Zero) ? clientToken : senderDetails.clientToken;
            WindowsIdentity windowsIdentity = null;
            if (clientToken != IntPtr.Zero)
                    windowsIdentity = new WindowsIdentity(clientToken, senderDetails.authenticationMechanism);
                // Suppress exceptions..So windowsIdentity = null in these cases
                    // userToken is 0.
                    // -or-
                    // userToken is duplicated and invalid for impersonation.
                    // The caller does not have the correct permissions.
                    // A Win32 error occurred.
            PSPrincipal userPrincipal = new PSPrincipal(psIdentity, windowsIdentity);
            PSSenderInfo result = new PSSenderInfo(userPrincipal, senderDetails.httpUrl);
        private const string WSManRunAsClientTokenName = "__WINRM_RUNAS_CLIENT_TOKEN__";
        /// Helper method to retrieve the WSMan client token from the __WINRM_RUNAS_CLIENT_TOKEN__
        /// environment variable, which is set in the WSMan layer for Virtual or RunAs accounts.
        /// <returns>ClientToken IntPtr.</returns>
        private static IntPtr GetRunAsClientToken()
            string clientTokenStr = System.Environment.GetEnvironmentVariable(WSManRunAsClientTokenName);
            if (clientTokenStr != null)
                // Remove the token value from the environment variable
                System.Environment.SetEnvironmentVariable(WSManRunAsClientTokenName, null);
                int clientTokenInt;
                if (int.TryParse(clientTokenStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out clientTokenInt))
                    return new IntPtr(clientTokenInt);
        /// Was private. Made protected internal for easier testing.
        protected internal bool EnsureOptionsComply(
            WSManNativeApi.WSManPluginRequest requestDetails)
            WSManNativeApi.WSManOption[] options = requestDetails.operationInfo.optionSet.options;
            bool isProtocolVersionDeclared = false;
            for (int i = 0; i < options.Length; i++) // What about requestDetails.operationInfo.optionSet.optionsCount? It is a hold over from the C++ API. Safer is Length.
                WSManNativeApi.WSManOption option = options[i];
                if (string.Equals(option.name, WSManPluginConstants.PowerShellStartupProtocolVersionName, StringComparison.Ordinal))
                    if (!EnsureProtocolVersionComplies(requestDetails, option.value))
                    isProtocolVersionDeclared = true;
                if (string.Compare(option.name, 0, WSManPluginConstants.PowerShellOptionPrefix, 0, WSManPluginConstants.PowerShellOptionPrefix.Length, StringComparison.Ordinal) == 0)
                    if (option.mustComply)
                            WSManPluginErrorCodes.OptionNotUnderstood,
                                RemotingErrorIdStrings.WSManPluginOptionNotUnderstood,
                                option.name,
                                System.Management.Automation.PSVersionInfo.GitCommitId,
                                WSManPluginConstants.PowerShellStartupProtocolVersionValue));
            if (!isProtocolVersionDeclared)
                    WSManPluginErrorCodes.ProtocolVersionNotFound,
                        RemotingErrorIdStrings.WSManPluginProtocolVersionNotFound,
                        WSManPluginConstants.PowerShellStartupProtocolVersionName,
        /// Verifies that the protocol version is in the correct syntax and supported.
        /// <param name="clientVersionString"></param>
        protected internal bool EnsureProtocolVersionComplies(
            string clientVersionString)
            if (string.Equals(clientVersionString, WSManPluginConstants.PowerShellStartupProtocolVersionValue, StringComparison.Ordinal))
            // Check if major versions are equal and server's minor version is smaller..
            // if so client's version is supported by the server. The understanding is
            // that minor version changes do not break the protocol.
            System.Version clientVersion = Utils.StringToVersion(clientVersionString);
            System.Version serverVersion = Utils.StringToVersion(WSManPluginConstants.PowerShellStartupProtocolVersionValue);
            if ((clientVersion != null) && (serverVersion != null) &&
                (clientVersion.Major == serverVersion.Major) &&
                (clientVersion.Minor >= serverVersion.Minor))
                WSManPluginErrorCodes.ProtocolVersionNotMatch,
                    RemotingErrorIdStrings.WSManPluginProtocolVersionNotMatch,
                    WSManPluginConstants.PowerShellStartupProtocolVersionValue,
                    clientVersionString));
        /// Static func to take care of unmanaged to managed transitions.
        internal static void PerformWSManPluginShell(
            IntPtr pluginContext, // PVOID
            IntPtr requestDetails, // WSMAN_PLUGIN_REQUEST*
            IntPtr startupInfo, // WSMAN_SHELL_STARTUP_INFO*
            IntPtr inboundShellInformation) // WSMAN_DATA*
                PSEventId.WSManCreateShell,
                "PerformWSManPluginShell: static func to take care of unmanaged to managed transitions.",
            WSManPluginInstance pluginToUse = GetFromActivePlugins(pluginContext);
            if (pluginToUse == null)
                lock (s_activePlugins)
                    pluginToUse = GetFromActivePlugins(pluginContext);
                        // create a new plugin
                        WSManPluginInstance mgdPlugin = new WSManPluginInstance();
                        AddToActivePlugins(pluginContext, mgdPlugin);
                        pluginToUse = mgdPlugin;
            // Marshal the incoming pointers into managed types prior to the call
            WSManNativeApi.WSManPluginRequest requestDetailsInstance = WSManNativeApi.WSManPluginRequest.UnMarshal(requestDetails);
            WSManNativeApi.WSManShellStartupInfo_UnToMan startupInfoInstance = WSManNativeApi.WSManShellStartupInfo_UnToMan.UnMarshal(startupInfo);
            WSManNativeApi.WSManData_UnToMan inboundShellInfo = WSManNativeApi.WSManData_UnToMan.UnMarshal(inboundShellInformation);
                requestDetailsInstance.ToString(),
                requestDetailsInstance.resourceUri);
            pluginToUse.CreateShell(pluginContext, requestDetailsInstance, flags, extraInfo, startupInfoInstance, inboundShellInfo);
                "PerformWSManPluginShell: Completed",
        internal static void PerformWSManPluginCommand(
            IntPtr shellContext, // PVOID
            [MarshalAs(UnmanagedType.LPWStr)] string commandLine,
            IntPtr arguments) // WSMAN_COMMAND_ARG_SET*
                "PerformWSManPluginCommand: static func to take care of unmanaged to managed transitions.",
                    WSManPluginErrorCodes.PluginContextNotFound,
                        RemotingErrorIdStrings.WSManPluginContextNotFound));
            WSManNativeApi.WSManPluginRequest request = WSManNativeApi.WSManPluginRequest.UnMarshal(requestDetails);
            WSManNativeApi.WSManCommandArgSet argSet = WSManNativeApi.WSManCommandArgSet.UnMarshal(arguments);
                request.ToString(),
                request.resourceUri);
            pluginToUse.CreateCommand(pluginContext, request, flags, shellContext, commandLine, argSet);
                "PerformWSManPluginCommand: Completed",
        internal static void PerformWSManPluginConnect(
            IntPtr inboundConnectInformation)
                "PerformWSManPluginConnect: static func to take care of unmanaged to managed transitions.",
            WSManNativeApi.WSManData_UnToMan connectInformation = WSManNativeApi.WSManData_UnToMan.UnMarshal(inboundConnectInformation);
            pluginToUse.ConnectShellOrCommand(request, flags, shellContext, commandContext, connectInformation);
                "PerformWSManPluginConnect: Completed",
        internal static void PerformWSManPluginSend(
            IntPtr commandContext, // PVOID
            IntPtr inboundData) // WSMAN_DATA*
                "PerformWSManPluginSend: Invoked",
            WSManNativeApi.WSManData_UnToMan data = WSManNativeApi.WSManData_UnToMan.UnMarshal(inboundData);
            pluginToUse.SendOneItemToShellOrCommand(request, flags, shellContext, commandContext, stream, data);
                "PerformWSManPluginSend: Completed",
        internal static void PerformWSManPluginReceive(
            IntPtr streamSet) // WSMAN_STREAM_ID_SET*
                "PerformWSManPluginReceive: Invoked",
            WSManNativeApi.WSManStreamIDSet_UnToMan streamIdSet = WSManNativeApi.WSManStreamIDSet_UnToMan.UnMarshal(streamSet);
            pluginToUse.EnableShellOrCommandToSendDataToClient(pluginContext, request, flags, shellContext, commandContext, streamIdSet);
                "PerformWSManPluginReceive: Completed",
        internal static void PerformWSManPluginSignal(
            string code)
                "PerformWSManPluginSignal: Invoked",
            // Close Command
            if (commandContext != IntPtr.Zero)
                if (!string.Equals(code, WSManPluginConstants.CtrlCSignal, StringComparison.Ordinal))
                    // Close operations associated with this command..
                    WSManPluginOperationShutdownContext cmdCtxt = new WSManPluginOperationShutdownContext(pluginContext, shellContext, commandContext, false);
                    if (cmdCtxt != null)
                        PerformCloseOperation(cmdCtxt);
                        ReportOperationComplete(request, WSManPluginErrorCodes.OutOfMemory);
                    // we got crtl_c (stop) message from client. so stop powershell
                            request,
                    // this will ReportOperationComplete by itself..
                    // so we just here.
                    pluginToUse.StopCommand(request, shellContext, commandContext);
            ReportOperationComplete(request, WSManPluginErrorCodes.NoError);
        /// Close the operation specified by the supplied context.
        internal static void PerformCloseOperation(
                "PerformCloseOperation: Invoked",
            WSManPluginInstance pluginToUse = GetFromActivePlugins(context.pluginContext);
            if (context.commandContext == IntPtr.Zero)
                // this is targeted at shell
                pluginToUse.CloseShellOperation(context);
                // shutdown is targeted at command
                pluginToUse.CloseCommandOperation(context);
        /// Performs deinitialization during shutdown.
        internal static void PerformShutdown(
            IntPtr pluginContext)
            pluginToUse.Shutdown();
        private static WSManPluginInstance GetFromActivePlugins(IntPtr pluginContext)
                WSManPluginInstance result = null;
                s_activePlugins.TryGetValue(pluginContext, out result);
        private static void AddToActivePlugins(IntPtr pluginContext, WSManPluginInstance plugin)
                if (!s_activePlugins.ContainsKey(pluginContext))
                    s_activePlugins.Add(pluginContext, plugin);
        /// Report operation complete to WSMan and supply a reason (if any)
        internal static void ReportWSManOperationComplete(
            WSManPluginErrorCodes errorCode)
            Dbg.Assert(requestDetails != null, "requestDetails cannot be null in operation complete.");
                (requestDetails.unmanagedHandle).ToString(),
                Convert.ToString(errorCode, CultureInfo.InvariantCulture),
            ReportOperationComplete(requestDetails.unmanagedHandle, errorCode);
        /// Extract message from exception (if any) and report operation complete with it to WSMan.
        /// <param name="reasonForClose"></param>
            Exception reasonForClose)
            WSManPluginErrorCodes error = WSManPluginErrorCodes.NoError;
            string errorMessage = string.Empty;
            string stackTrace = string.Empty;
            if (reasonForClose != null)
                error = WSManPluginErrorCodes.ManagedException;
                errorMessage = reasonForClose.Message;
                stackTrace = reasonForClose.StackTrace;
                Convert.ToString(error, CultureInfo.InvariantCulture),
                stackTrace);
                // report operation complete to wsman with the error message (if any).
                        reasonForClose.Message));
                    WSManPluginErrorCodes.NoError);
        /// Sets thread properties like UI Culture, Culture etc..This is needed as code is transitioning from
        /// unmanaged heap to managed heap...and thread properties are not set correctly during this
        /// transition.
        /// Currently WSMan provider supplies only UI Culture related data..so only UI Culture is set.
        internal static void SetThreadProperties(
            // requestDetails cannot not be null.
            Dbg.Assert(requestDetails != null, "requestDetails cannot be null");
            // IntPtr nativeLocaleData = IntPtr.Zero;
            WSManNativeApi.WSManDataStruct outputStruct = new WSManNativeApi.WSManDataStruct();
            int hResult = wsmanPinvokeStatic.WSManPluginGetOperationParameters(
                WSManPluginConstants.WSManPluginParamsGetRequestedLocale,
                outputStruct);
            // ref nativeLocaleData);
            bool retrievingLocaleSucceeded = (hResult == 0);
            WSManNativeApi.WSManData_UnToMan localeData = WSManNativeApi.WSManData_UnToMan.UnMarshal(outputStruct); // nativeLocaleData
            // IntPtr nativeDataLocaleData = IntPtr.Zero;
            hResult = wsmanPinvokeStatic.WSManPluginGetOperationParameters(
                WSManPluginConstants.WSManPluginParamsGetRequestedDataLocale,
            // ref nativeDataLocaleData);
            bool retrievingDataLocaleSucceeded = (hResult == (int)WSManPluginErrorCodes.NoError);
            WSManNativeApi.WSManData_UnToMan dataLocaleData = WSManNativeApi.WSManData_UnToMan.UnMarshal(outputStruct); // nativeDataLocaleData
            // Set the UI Culture
                if (retrievingLocaleSucceeded && (localeData.Type == (uint)WSManNativeApi.WSManDataType.WSMAN_DATA_TYPE_TEXT))
                    CultureInfo uiCultureToUse = new CultureInfo(localeData.Text);
                    Thread.CurrentThread.CurrentUICulture = uiCultureToUse;
            // ignore if there is any exception constructing the culture..
            // Set the Culture
                if (retrievingDataLocaleSucceeded && (dataLocaleData.Type == (uint)WSManNativeApi.WSManDataType.WSMAN_DATA_TYPE_TEXT))
                    CultureInfo cultureToUse = new CultureInfo(dataLocaleData.Text);
                    Thread.CurrentThread.CurrentCulture = cultureToUse;
        /// Handle any unhandled exceptions that get raised in the AppDomain
        /// This will log the exception into Crimson logs.
        internal static void UnhandledExceptionHandler(
            UnhandledExceptionEventArgs args)
            // args can never be null.
            Exception exception = (Exception)args.ExceptionObject;
            PSEtwLog.LogOperationalError(PSEventId.AppDomainUnhandledException,
                    exception.GetType().ToString(), exception.Message,
                    exception.StackTrace);
            PSEtwLog.LogAnalyticError(PSEventId.AppDomainUnhandledException_Analytic,
        /// Alternate wrapper for WSManPluginOperationComplete. TODO: Needed? I could easily use the handle instead and get rid of this? It is only for easier refactoring...
        /// <param name="errorMessage">Pre-formatted localized string.</param>
        internal static void ReportOperationComplete(
            WSManPluginErrorCodes errorCode,
            if (requestDetails != null)
                ReportOperationComplete(requestDetails.unmanagedHandle, errorCode, errorMessage);
            // else cannot report if requestDetails is null.
        /// Wrapper for WSManPluginOperationComplete. It performs validation prior to making the call.
            if (requestDetails != null &&
                requestDetails.unmanagedHandle != IntPtr.Zero)
                wsmanPinvokeStatic.WSManPluginOperationComplete(
                    (int)errorCode,
            string errorMessage = "")
            if (requestDetails == IntPtr.Zero)
                // cannot report if requestDetails is null.

    /// Abstract class that defines common functionality for WinRM Plugin API Server Sessions.
    internal abstract class WSManPluginServerSession : IDisposable
        protected bool isClosed;
        protected bool isContextReported;
        // used to keep track of last error..this will be used
        // for reporting operation complete to WSMan.
        protected Exception lastErrorReported;
        // request context passed by WSMan while creating a shell or command.
        internal WSManNativeApi.WSManPluginRequest creationRequestDetails;
        // request context passed by WSMan while sending Plugin data.
        internal WSManNativeApi.WSManPluginRequest sendRequestDetails;
        internal WSManPluginOperationShutdownContext shutDownContext;
        // tracker used in conjunction with WSMan API to identify a particular
        // shell context.
        internal RegisteredWaitHandle registeredShutDownWaitHandle;
        internal WSManPluginServerTransportManager transportMgr;
        internal int registeredShutdownNotification;
        // event that gets raised when session is closed.."source" will provide
        // IntPtr for "creationRequestDetails" which can be used to free
        // the context.
        internal event EventHandler<EventArgs> SessionClosed;
        // Track whether Dispose has been called.
        protected WSManPluginServerSession(
            WSManNativeApi.WSManPluginRequest creationRequestDetails,
            WSManPluginServerTransportManager transportMgr)
            this.creationRequestDetails = creationRequestDetails;
            this.transportMgr = transportMgr;
            transportMgr.PrepareCalled += this.HandlePrepareFromTransportManager;
            transportMgr.WSManTransportErrorOccured += this.HandleTransportError;
            // Dispose of unmanaged resources.
        /// <param name="disposing"></param> True when called from Dispose(), False when called from Finalize().
                    // Close(false);
                Close(false);
        /// Finalizes an instance of the <see cref="WSManPluginServerSession"/> class.
        ~WSManPluginServerSession()
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
        internal void SendOneItemToSession(
            if ((!string.Equals(stream, WSManPluginConstants.SupportedInputStream, StringComparison.Ordinal)) &&
                (!string.Equals(stream, WSManPluginConstants.SupportedPromptResponseStream, StringComparison.Ordinal)))
                    WSManPluginErrorCodes.InvalidInputStream,
                        RemotingErrorIdStrings.WSManPluginInvalidInputStream,
                        WSManPluginConstants.SupportedInputStream));
            if (inboundData == null)
                // no data is supplied..just ignore.
            if (inboundData.Type != (uint)WSManNativeApi.WSManDataType.WSMAN_DATA_TYPE_BINARY)
                // only binary data is supported
                        "WSMAN_DATA_TYPE_BINARY"));
                    WSManPluginInstance.ReportWSManOperationComplete(requestDetails, lastErrorReported);
                // store the send request details..because the operation complete
                // may happen from a different thread.
                sendRequestDetails = requestDetails;
            SendOneItemToSessionHelper(inboundData.Data, stream);
            // report operation complete.
            ReportSendOperationComplete();
        internal void SendOneItemToSessionHelper(
            byte[] data,
            string stream)
            transportMgr.ProcessRawData(data, stream);
        internal bool EnableSessionToSendDataToClient(
            WSManNativeApi.WSManStreamIDSet_UnToMan streamSet,
            WSManPluginOperationShutdownContext ctxtToReport)
            if ((streamSet == null) ||
                (streamSet.streamIDsCount != 1))
                // only "stdout" is the supported output stream.
                    WSManPluginErrorCodes.InvalidOutputStream,
                        RemotingErrorIdStrings.WSManPluginInvalidOutputStream,
            if (!string.Equals(streamSet.streamIDs[0], WSManPluginConstants.SupportedOutputStream, StringComparison.Ordinal))
            return transportMgr.EnableTransportManagerSendDataToClient(requestDetails, ctxtToReport);
        /// Report session context to WSMan..this will let WSMan send ACK to
        /// client and client can send data.
        internal void ReportContext()
            bool isRegisterWaitForSingleObjectFailed = false;
                if (!isContextReported)
                    isContextReported = true;
                    PSEtwLog.LogAnalyticInformational(PSEventId.ReportContext,
                        creationRequestDetails.ToString(), creationRequestDetails.ToString());
                    // TO BE FIXED - As soon as this API is called, WinRM service will send CommandResponse back and Signal is expected anytime
                    // If Signal comes and executes before registering the notification handle, cleanup will be messed
                    result = WSManNativeApi.WSManPluginReportContext(creationRequestDetails.unmanagedHandle, 0, creationRequestDetails.unmanagedHandle);
                    if (Platform.IsWindows && (result == WSManPluginConstants.ExitCodeSuccess))
                        registeredShutdownNotification = 1;
                        SafeWaitHandle safeWaitHandle = new SafeWaitHandle(creationRequestDetails.shutdownNotificationHandle, false); // Owned by WinRM
                        // Register shutdown notification handle
                        this.registeredShutDownWaitHandle = ThreadPool.RegisterWaitForSingleObject(
                            shutDownContext,
                        if (this.registeredShutDownWaitHandle == null)
                            isRegisterWaitForSingleObjectFailed = true;
                            registeredShutdownNotification = 0;
            if ((result != WSManPluginConstants.ExitCodeSuccess) || (isRegisterWaitForSingleObjectFailed))
                if (isRegisterWaitForSingleObjectFailed)
                    errorMessage = StringUtil.Format(RemotingErrorIdStrings.WSManPluginShutdownRegistrationFailed);
                    errorMessage = StringUtil.Format(RemotingErrorIdStrings.WSManPluginReportContextFailed);
                // Report error and close the session
                Exception mgdException = new InvalidOperationException(errorMessage);
                Close(mgdException);
        /// Added to provide derived classes with the ability to send event notifications.
        protected internal void SafeInvokeSessionClosed(object sender, EventArgs eventArgs)
            SessionClosed.SafeInvoke(sender, eventArgs);
        // handle transport manager related errors
        internal void HandleTransportError(object sender, TransportErrorOccuredEventArgs eventArgs)
            Exception reasonForClose = null;
                reasonForClose = eventArgs.Exception;
            Close(reasonForClose);
        // handle prepare from transport by reporting context to WSMan.
        internal void HandlePrepareFromTransportManager(object sender, EventArgs eventArgs)
            ReportContext();
            transportMgr.PrepareCalled -= this.HandlePrepareFromTransportManager;
        internal void Close(bool isShuttingDown)
            if (Interlocked.Exchange(ref registeredShutdownNotification, 0) == 1)
                // release the shutdown notification handle.
                if (registeredShutDownWaitHandle != null)
                    registeredShutDownWaitHandle.Unregister(null);
                    registeredShutDownWaitHandle = null;
            // Delete the context only if isShuttingDown != true. isShuttingDown will
            // be true only when the method is called from RegisterWaitForSingleObject
            // handler..in which case the context will be freed from the callback.
            if (shutDownContext != null)
                shutDownContext = null;
            transportMgr.WSManTransportErrorOccured -= this.HandleTransportError;
            // We should not use request details again after so releasing the resource.
            // Remember not to free this memory as this memory is allocated and owned by WSMan.
            creationRequestDetails = null;
            // System.GC.SuppressFinalize(this); // TODO: This is already called in Dispose().
        // close current session and transport manager because of an exception
        internal void Close(Exception reasonForClose)
            lastErrorReported = reasonForClose;
            WSManPluginOperationShutdownContext context = new WSManPluginOperationShutdownContext(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, false);
            CloseOperation(context, reasonForClose);
        // Report Operation Complete using the send request details.
        internal void ReportSendOperationComplete()
                if (sendRequestDetails != null)
                    // report and clear the send request details
                    WSManPluginInstance.ReportWSManOperationComplete(sendRequestDetails, lastErrorReported);
                    sendRequestDetails = null;
        #region Pure virtual methods
        internal abstract void CloseOperation(WSManPluginOperationShutdownContext context, Exception reasonForClose);
        internal abstract void ExecuteConnect(
            WSManNativeApi.WSManPluginRequest requestDetails, // in
            int flags, // in
            WSManNativeApi.WSManData_UnToMan inboundConnectInformation); // in optional
    internal class WSManPluginShellSession : WSManPluginServerSession
        private readonly Dictionary<IntPtr, WSManPluginCommandSession> _activeCommandSessions;
        private readonly ServerRemoteSession _remoteSession;
        #region Internally Visible Members
        internal object shellSyncObject;
        internal WSManPluginShellSession(
            WSManPluginServerTransportManager transportMgr,
            ServerRemoteSession remoteSession,
            WSManPluginOperationShutdownContext shutDownContext)
            : base(creationRequestDetails, transportMgr)
            _remoteSession = remoteSession;
            _remoteSession.Closed +=
                new EventHandler<RemoteSessionStateMachineEventArgs>(this.HandleServerRemoteSessionClosed);
            _activeCommandSessions = new Dictionary<IntPtr, WSManPluginCommandSession>();
            this.shellSyncObject = new object();
            this.shutDownContext = shutDownContext;
        /// Main Routine for Connect on a Shell.
        /// Calls in server remotesessions ExecuteConnect to run the Connect algorithm
        /// This call is synchronous. i.e WSManOperationComplete will be called before the routine completes.
        internal override void ExecuteConnect(
            WSManNativeApi.WSManData_UnToMan inboundConnectInformation) // in optional
            if (inboundConnectInformation == null)
                        "inboundConnectInformation",
                        "WSManPluginShellConnect"));
            // not registering shutdown event as this is a synchronous operation.
            IntPtr responseXml = IntPtr.Zero;
                byte[] inputData;
                byte[] outputData;
                // Retrieve the string (Base64 encoded)
                inputData = ServerOperationHelpers.ExtractEncodedXmlElement(
                    inboundConnectInformation.Text,
                    WSManNativeApi.PS_CONNECT_XML_TAG);
                // this will raise exceptions on failure
                    _remoteSession.ExecuteConnect(inputData, out outputData);
                    // construct Xml to send back
                    string responseData = string.Format(
                        "<{0} xmlns=\"{1}\">{2}</{0}>",
                        WSManNativeApi.PS_CONNECTRESPONSE_XML_TAG,
                        WSManNativeApi.PS_XML_NAMESPACE,
                        Convert.ToBase64String(outputData));
                    // TODO: currently using OperationComplete to report back the responseXml. This will need to change to use WSManReportObject
                    // that is currently internal.
                    WSManPluginInstance.ReportOperationComplete(requestDetails, WSManPluginErrorCodes.NoError, responseData);
                catch (PSRemotingDataStructureException ex)
                    WSManPluginInstance.ReportOperationComplete(requestDetails, WSManPluginErrorCodes.PluginConnectOperationFailed, ex.Message);
            catch (OutOfMemoryException)
                WSManPluginInstance.ReportOperationComplete(requestDetails, WSManPluginErrorCodes.OutOfMemory);
                if (responseXml != IntPtr.Zero)
                    Marshal.FreeHGlobal(responseXml);
        // Create a new command in the shell context.
                // inbound cmd information is already verified.. so no need to verify here.
                WSManPluginCommandTransportManager serverCmdTransportMgr = new WSManPluginCommandTransportManager(transportMgr);
                serverCmdTransportMgr.Initialize();
                // Apply quota limits on the command transport manager
                _remoteSession.ApplyQuotaOnCommandTransportManager(serverCmdTransportMgr);
                WSManPluginCommandSession mgdCmdSession = new WSManPluginCommandSession(requestDetails, serverCmdTransportMgr, _remoteSession);
                AddToActiveCmdSessions(mgdCmdSession);
                mgdCmdSession.SessionClosed += this.HandleCommandSessionClosed;
                mgdCmdSession.shutDownContext = new WSManPluginOperationShutdownContext(
                    pluginContext,
                    creationRequestDetails.unmanagedHandle,
                    mgdCmdSession.creationRequestDetails.unmanagedHandle,
                    if (!mgdCmdSession.ProcessArguments(arguments))
                            WSManPluginErrorCodes.InvalidArgSet,
                                RemotingErrorIdStrings.WSManPluginInvalidArgSet,
                                "WSManPluginCommand"));
                    // Report plugin context to WSMan
                    mgdCmdSession.ReportContext();
                // if there is an exception creating remote session send the message to client.
        // Closes the command and clears associated resources
            WSManPluginCommandSession mgdCmdSession = GetCommandSession(context.commandContext);
                DeleteFromActiveCmdSessions(mgdCmdSession.creationRequestDetails.unmanagedHandle);
            mgdCmdSession.CloseOperation(context, null);
        // adds command session to active command Sessions store and returns the id
        // at which the session is added.
        private void AddToActiveCmdSessions(
            WSManPluginCommandSession newCmdSession)
            lock (shellSyncObject)
                IntPtr key = newCmdSession.creationRequestDetails.unmanagedHandle;
                if (!_activeCommandSessions.ContainsKey(key))
                    _activeCommandSessions.Add(key, newCmdSession);
        private void DeleteFromActiveCmdSessions(
                _activeCommandSessions.Remove(keyToDelete);
        // closes all the active command sessions.
        private void CloseAndClearCommandSessions(
            Collection<WSManPluginCommandSession> copyCmdSessions = new Collection<WSManPluginCommandSession>();
                Dictionary<IntPtr, WSManPluginCommandSession>.Enumerator cmdEnumerator = _activeCommandSessions.GetEnumerator();
                while (cmdEnumerator.MoveNext())
                    copyCmdSessions.Add(cmdEnumerator.Current.Value);
                _activeCommandSessions.Clear();
            // close the command sessions outside of the lock
            IEnumerator<WSManPluginCommandSession> cmdSessionEnumerator = copyCmdSessions.GetEnumerator();
            while (cmdSessionEnumerator.MoveNext())
                WSManPluginCommandSession cmdSession = cmdSessionEnumerator.Current;
                // we are not interested in session closed events anymore as we are initiating the close
                // anyway/
                cmdSession.SessionClosed -= this.HandleCommandSessionClosed;
                cmdSession.Close(reasonForClose);
            copyCmdSessions.Clear();
        // returns the command session instance for a given command id.
        // null if not found.
        internal WSManPluginCommandSession GetCommandSession(
            IntPtr cmdContext)
                WSManPluginCommandSession result = null;
                _activeCommandSessions.TryGetValue(cmdContext, out result);
        private void HandleServerRemoteSessionClosed(
            RemoteSessionStateMachineEventArgs eventArgs)
                reasonForClose = eventArgs.Reason;
        private void HandleCommandSessionClosed(
            // command context is passed as "source" parameter
            DeleteFromActiveCmdSessions((IntPtr)source);
        internal override void CloseOperation(
            WSManPluginOperationShutdownContext context,
            // let command sessions to close.
            WSManPluginInstance.SetThreadProperties(creationRequestDetails);
            bool isRcvOpShuttingDown = (context.isShuttingDown) && (context.isReceiveOperation);
            bool isRcvOp = context.isReceiveOperation;
            bool isShuttingDown = context.isShuttingDown;
            // close the pending send operation if any
            // close the shell's transport manager after commands handled the operation
            transportMgr.DoClose(isRcvOpShuttingDown, reasonForClose);
            if (!isRcvOp)
                // Initiate close on the active command sessions and then clear the internal
                // Command Session dictionary
                CloseAndClearCommandSessions(reasonForClose);
                // raise session closed event and let dependent code to release resources.
                // null check is not performed here because the handler will take care of this.
                base.SafeInvokeSessionClosed(creationRequestDetails.unmanagedHandle, EventArgs.Empty);
                // Send Operation Complete to WSMan service
                WSManPluginInstance.ReportWSManOperationComplete(creationRequestDetails, reasonForClose);
                // let base class release its resources
                base.Close(isShuttingDown);
            // TODO: Do this.Dispose(); here?
    internal class WSManPluginCommandSession : WSManPluginServerSession
        internal object cmdSyncObject;
        internal WSManPluginCommandSession(
            ServerRemoteSession remoteSession)
            cmdSyncObject = new object();
        internal bool ProcessArguments(
            if (arguments.argsCount != 1)
            byte[] convertedBase64 = Convert.FromBase64String(arguments.args[0]);
            transportMgr.ProcessRawData(convertedBase64, WSManPluginConstants.SupportedInputStream);
        internal void Stop(
            // stop the command..command will be stopped if we raise ClosingEvent on
            // transport manager.
            transportMgr.PerformStop();
            WSManPluginInstance.ReportWSManOperationComplete(requestDetails, null);
            lock (cmdSyncObject)
            // only one thread will be here.
            bool isRcvOpShuttingDown = (context.isShuttingDown) &&
                (context.isReceiveOperation) &&
                (context.commandContext == creationRequestDetails.unmanagedHandle);
            bool isCmdShuttingDown = (context.isShuttingDown) &&
                (!context.isReceiveOperation) &&
            // close the shell's transport manager first..so we wont send data.
                // null check is not performed here because Managed C++ will take care of this.
                this.Close(isCmdShuttingDown);
        /// Main routine for connect on a command/pipeline.. Currently NO-OP
        /// will be enhanced later to support intelligent connect... like ending input streams on pipelines
        /// that are still waiting for input data.

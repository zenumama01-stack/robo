    internal class WSManPluginServerTransportManager : AbstractServerSessionTransportManager
        private WSManNativeApi.WSManPluginRequest _requestDetails;
        // the following variables are used to block thread from sending
        // data to the client until the client sends a receive request.
        private bool _isRequestPending;
        private readonly ManualResetEvent _waitHandle;
        private readonly Dictionary<Guid, WSManPluginServerTransportManager> _activeCmdTransportManagers;
        private bool _isClosed;
        private Exception _lastErrorReported;
        // used with RegisterWaitForSingleObject. This object needs to be freed
        // upon close
        private WSManPluginOperationShutdownContext _shutDownContext;
        private RegisteredWaitHandle _registeredShutDownWaitHandle;
        // event that gets raised when Prepare is called. Respective Session
        // object can use this callback to ReportContext to client.
        public event EventHandler<EventArgs> PrepareCalled;
        internal WSManPluginServerTransportManager(
            _activeCmdTransportManagers = new Dictionary<Guid, WSManPluginServerTransportManager>();
        #region Inherited_from_AbstractServerSessionTransportManager
        internal override void Close(
            DoClose(false, reasonForClose);
        /// <param name="isShuttingDown">true if the method is called from RegisterWaitForSingleObject
        /// callback. This boolean is used to decide whether to UnregisterWait or
        /// UnregisterWaitEx</param>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", Justification = "The WSManPluginReceiveResult return value is not documented and is not needed in this case.")]
        internal void DoClose(
            bool isShuttingDown,
            if (_isClosed)
                _isClosed = true;
                _lastErrorReported = reasonForClose;
                if (!_isRequestPending)
                    // release threads blocked on the sending data to client if any
            // only one thread will reach here
            // let everyone know that we are about to close
                foreach (var cmdTransportKvp in _activeCmdTransportManagers)
                    cmdTransportKvp.Value.Close(reasonForClose);
                _activeCmdTransportManagers.Clear();
                if (_registeredShutDownWaitHandle != null)
                    // This will not wait for the callback to complete.
                    _registeredShutDownWaitHandle.Unregister(null);
                    _registeredShutDownWaitHandle = null;
                if (_shutDownContext != null)
                    _shutDownContext = null;
                // This might happen when client did not send a receive request
                // but the server is closing
                if (_requestDetails != null)
                    // Notify that no more data is being sent on this transport.
                    WSManNativeApi.WSManPluginReceiveResult(
                        _requestDetails.unmanagedHandle,
                        (int)WSManNativeApi.WSManFlagReceive.WSMAN_FLAG_RECEIVE_RESULT_NO_MORE_DATA,
                        WSManPluginConstants.SupportedOutputStream,
                        WSManNativeApi.WSMAN_COMMAND_STATE_DONE,
                    WSManPluginInstance.ReportWSManOperationComplete(_requestDetails, reasonForClose);
                    // We should not use request details again after reporting operation complete
                    // so releasing the resource. Remember not to free this memory as this memory
                    // is allocated and owned by WSMan.
                    _requestDetails = null;
                // dispose resources
        /// Used by powershell DS handler. notifies transport that powershell is back to running state
        /// no payload.
            int result = (int)WSManPluginErrorCodes.NoError;
            // there should have been a receive request in place already
                if (!_isClosed)
                    result = WSManNativeApi.WSManPluginReceiveResult(
                        WSManNativeApi.WSMAN_COMMAND_STATE_RUNNING,
            if (result != (int)WSManPluginErrorCodes.NoError)
                ReportError(result, "WSManPluginReceiveResult");
        /// If flush is true, data will be sent immediately to the client. This is accomplished
        /// by using WSMAN_FLAG_RECEIVE_FLUSH flag provided by WSMan API.
        /// <param name="flush"></param>
        /// <param name="reportAsPending"></param>
        /// <param name="reportAsDataBoundary"></param>
        protected override void SendDataToClient(
            bool flush,
            bool reportAsPending,
            bool reportAsDataBoundary)
            // double-check locking mechanism is used here to avoid entering into lock
            // every time data is sent..entering/exiting from lock is costly.
                // Dont send data until we have received request from client.
                // The following blocks the calling thread. The thread is
                // unblocked once a request from client arrives.
                _isRequestPending = true;
                // at this point request must be pending..so dispose waitHandle
            // at this point we have pending request from client. so it is safe
            // to send data to client using WSMan API.
            using (WSManNativeApi.WSManData_ManToUn dataToBeSent = new WSManNativeApi.WSManData_ManToUn(data))
                        if (flush)
                            flags |= (int)WSManNativeApi.WSManFlagReceive.WSMAN_FLAG_RECEIVE_FLUSH;
                        if (reportAsDataBoundary)
                            // currently assigning hardcoded value for this flag, this is a new change in wsman.h and needs to be replaced with the actual definition once
                            // modified wsman.h is in public headers
                            flags |= (int)WSManNativeApi.WSManFlagReceive.WSMAN_FLAG_RECEIVE_RESULT_DATA_BOUNDARY;
                            dataToBeSent,
                            reportAsPending ? WSManNativeApi.WSMAN_COMMAND_STATE_PENDING : null,
            // raise PrepareCalled event and let dependent code to ReportContext.
            PrepareCalled(this, EventArgs.Empty);
        internal override AbstractServerTransportManager GetCommandTransportManager(
            Guid powerShellCmdId)
            return _activeCmdTransportManagers[powerShellCmdId];
        // Used by command transport manager to manage cmd transport manager instances by session.
        internal void ReportTransportMgrForCmd(
            Guid cmdId,
            WSManPluginServerTransportManager transportManager)
                if (!_activeCmdTransportManagers.ContainsKey(cmdId))
                    _activeCmdTransportManagers.Add(cmdId, transportManager);
        internal override void RemoveCommandTransportManager(
            Guid cmdId)
                _activeCmdTransportManagers.Remove(cmdId);
        internal bool EnableTransportManagerSendDataToClient(
            _shutDownContext = ctxtToReport;
                if (_isRequestPending)
                    // if a request is already pending..ignore this.
                    WSManPluginInstance.ReportWSManOperationComplete(requestDetails, _lastErrorReported);
                _requestDetails = requestDetails;
                    _registeredShutDownWaitHandle = ThreadPool.RegisterWaitForSingleObject(
                            _shutDownContext,
                    if (_registeredShutDownWaitHandle == null)
                // release thread waiting to send data to the client.
                WSManPluginInstance.PerformCloseOperation(ctxtToReport);
                    WSManPluginErrorCodes.ShutdownRegistrationFailed,
                        RemotingErrorIdStrings.WSManPluginShutdownRegistrationFailed));
        // This will either RaiseClosingEvent or calls DoClose()
        // RaiseClosingEvent will be called if Client has already put a receive request,
        // Otherwise DoClose() is called.
        // This is to make sure server sends all the data it has to Client w.r.t stopping
        // a command like StateChangedInfo etc.
        internal void PerformStop()
                DoClose(false, null);
    internal class WSManPluginCommandTransportManager : WSManPluginServerTransportManager
        private readonly WSManPluginServerTransportManager _serverTransportMgr;
        private System.Guid _cmdId;
        // Create Cmd Transport Manager for this sessn transport manager
        internal WSManPluginCommandTransportManager(WSManPluginServerTransportManager srvrTransportMgr)
            : base(srvrTransportMgr.Fragmentor.FragmentSize, srvrTransportMgr.CryptoHelper)
            _serverTransportMgr = srvrTransportMgr;
            this.TypeTable = srvrTransportMgr.TypeTable;
        internal void Initialize()
            this.PowerShellGuidObserver += OnPowershellGuidReported;
            this.MigrateDataReadyEventHandlers(_serverTransportMgr);
        private void OnPowershellGuidReported(object src, System.EventArgs args)
            _cmdId = (System.Guid)src;
            _serverTransportMgr.ReportTransportMgrForCmd(_cmdId, this);
            this.PowerShellGuidObserver -= this.OnPowershellGuidReported;

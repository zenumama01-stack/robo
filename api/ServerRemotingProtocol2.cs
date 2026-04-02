    /// Handles all data structure handler communication with the client
    internal class ServerRunspacePoolDataStructureHandler
        /// Constructor which takes a server runspace pool driver and
        /// creates an associated ServerRunspacePoolDataStructureHandler.
        /// <param name="driver"></param>
        internal ServerRunspacePoolDataStructureHandler(ServerRunspacePoolDriver driver,
            AbstractServerSessionTransportManager transportManager)
            _clientRunspacePoolId = driver.InstanceId;
        /// Send a message with application private data to the client.
        /// <param name="applicationPrivateData">ApplicationPrivateData to send.</param>
        /// <param name="serverCapability">Server capability negotiated during initial exchange of remoting messages / session capabilities of client and server.</param>
        internal void SendApplicationPrivateDataToClient(PSPrimitiveDictionary applicationPrivateData, RemoteSessionCapability serverCapability)
            // make server's PSVersionTable available to the client using ApplicationPrivateData
            PSPrimitiveDictionary applicationPrivateDataWithVersionTable =
                PSPrimitiveDictionary.CloneAndAddPSVersionTable(applicationPrivateData);
            // override the hardcoded version numbers with the stuff that was reported to the client during negotiation
            PSPrimitiveDictionary versionTable = (PSPrimitiveDictionary)applicationPrivateDataWithVersionTable[PSVersionInfo.PSVersionTableName];
            versionTable[PSVersionInfo.PSRemotingProtocolVersionName] = serverCapability.ProtocolVersion;
            versionTable[PSVersionInfo.SerializationVersionName] = serverCapability.SerializationVersion;
            // Pass back the true PowerShell version to the client via application private data.
            versionTable[PSVersionInfo.PSVersionName] = PSVersionInfo.PSVersion;
            RemoteDataObject data = RemotingEncoder.GenerateApplicationPrivateData(
                _clientRunspacePoolId, applicationPrivateDataWithVersionTable);
            SendDataAsync(data);
        /// Send a message with the RunspacePoolStateInfo to the client.
        /// <param name="stateInfo">State info to send.</param>
        internal void SendStateInfoToClient(RunspacePoolStateInfo stateInfo)
            RemoteDataObject data = RemotingEncoder.GenerateRunspacePoolStateInfo(
                    _clientRunspacePoolId, stateInfo);
        /// Send a message with the PSEventArgs to the client.
        /// <param name="e">Event to send.</param>
        internal void SendPSEventArgsToClient(PSEventArgs e)
            RemoteDataObject data = RemotingEncoder.GeneratePSEventArgs(_clientRunspacePoolId, e);
        /// Called when session is connected from a new client
        /// call into the sessionconnect handlers for each associated powershell dshandler.
        internal void ProcessConnect()
            List<ServerPowerShellDataStructureHandler> dsHandlers;
                dsHandlers = new List<ServerPowerShellDataStructureHandler>(_associatedShells.Values);
            foreach (var dsHandler in dsHandlers)
                dsHandler.ProcessConnect();
        /// Process the data received from the runspace pool on
                "RemotingTargetInterface must be Runspace");
                case RemotingDataType.CreatePowerShell:
                        Dbg.Assert(CreateAndInvokePowerShell != null,
                            "The ServerRunspacePoolDriver should subscribe to all data structure handler events");
                        CreateAndInvokePowerShell.SafeInvoke(this, new RemoteDataEventArgs<RemoteDataObject<PSObject>>(receivedData));
                case RemotingDataType.GetCommandMetadata:
                        Dbg.Assert(GetCommandMetadata != null,
                        GetCommandMetadata.SafeInvoke(this, new RemoteDataEventArgs<RemoteDataObject<PSObject>>(receivedData));
                case RemotingDataType.RemoteRunspaceHostResponseData:
                        Dbg.Assert(HostResponseReceived != null,
                        RemoteHostResponse remoteHostResponse = RemoteHostResponse.Decode(receivedData.Data);
                        // part of host message robustness algo. Now the host response is back, report to transport that
                        // execution status is back to running
                        _transportManager.ReportExecutionStatusAsRunning();
                        HostResponseReceived.SafeInvoke(this, new RemoteDataEventArgs<RemoteHostResponse>(remoteHostResponse));
                case RemotingDataType.SetMaxRunspaces:
                        Dbg.Assert(SetMaxRunspacesReceived != null,
                        SetMaxRunspacesReceived.SafeInvoke(this, new RemoteDataEventArgs<PSObject>(receivedData.Data));
                case RemotingDataType.SetMinRunspaces:
                        Dbg.Assert(SetMinRunspacesReceived != null,
                        SetMinRunspacesReceived.SafeInvoke(this, new RemoteDataEventArgs<PSObject>(receivedData.Data));
                case RemotingDataType.AvailableRunspaces:
                        Dbg.Assert(GetAvailableRunspacesReceived != null,
                        GetAvailableRunspacesReceived.SafeInvoke(this, new RemoteDataEventArgs<PSObject>(receivedData.Data));
                case RemotingDataType.ResetRunspaceState:
                        Dbg.Assert(ResetRunspaceState != null,
                            "The ServerRunspacePoolDriver should subscribe to all data structure handler events.");
                        ResetRunspaceState.SafeInvoke(this, new RemoteDataEventArgs<PSObject>(receivedData.Data));
        /// Creates a powershell data structure handler from this runspace pool.
        /// <param name="instanceId">Powershell instance id.</param>
        /// <param name="remoteStreamOptions">Remote stream options.</param>
        /// <param name="localPowerShell">Local PowerShell object.</param>
        /// <returns>ServerPowerShellDataStructureHandler.</returns>
        internal ServerPowerShellDataStructureHandler CreatePowerShellDataStructureHandler(
            Guid instanceId, Guid runspacePoolId, RemoteStreamOptions remoteStreamOptions, PowerShell localPowerShell)
            // start with pool's transport manager.
            AbstractServerTransportManager cmdTransportManager = _transportManager;
                cmdTransportManager = _transportManager.GetCommandTransportManager(instanceId);
                Dbg.Assert(cmdTransportManager.TypeTable != null, "This should be already set in managed C++ code");
            ServerPowerShellDataStructureHandler dsHandler =
                new ServerPowerShellDataStructureHandler(instanceId, runspacePoolId, remoteStreamOptions, cmdTransportManager, localPowerShell);
                _associatedShells.Add(dsHandler.PowerShellId, dsHandler);
            dsHandler.RemoveAssociation += HandleRemoveAssociation;
        /// Returns the currently active PowerShell datastructure handler.
        /// ServerPowerShellDataStructureHandler if one is present, null otherwise.
        internal ServerPowerShellDataStructureHandler GetPowerShellDataStructureHandler()
                if (_associatedShells.Count > 0)
                    foreach (object o in _associatedShells.Values)
                        ServerPowerShellDataStructureHandler result = o as ServerPowerShellDataStructureHandler;
        /// <param name="rcvdData">Message to dispatch.</param>
            // if data structure handler is not found, then association has already been
            // removed, discard message
        /// Send the specified response to the client. The client call will
        /// be blocked on the same.
        /// <param name="callId">Call id on the client.</param>
        /// <param name="response">Response to send.</param>
        internal void SendResponseToClient(long callId, object response)
                RemotingEncoder.GenerateRunspacePoolOperationResponse(_clientRunspacePoolId, response, callId);
            get { return _transportManager.TypeTable; }
            set { _transportManager.TypeTable = value; }
        /// This event is raised whenever there is a request from the
        /// client to create a powershell on the server and invoke it.
        internal event EventHandler<RemoteDataEventArgs<RemoteDataObject<PSObject>>> CreateAndInvokePowerShell;
        /// client to run command discovery pipeline.
        internal event EventHandler<RemoteDataEventArgs<RemoteDataObject<PSObject>>> GetCommandMetadata;
        /// This event is raised when a host call response is received.
        internal event EventHandler<RemoteDataEventArgs<RemoteHostResponse>> HostResponseReceived;
        /// This event is raised when there is a request to modify the
        /// maximum runspaces in the runspace pool.
        internal event EventHandler<RemoteDataEventArgs<PSObject>> SetMaxRunspacesReceived;
        /// minimum runspaces in the runspace pool.
        internal event EventHandler<RemoteDataEventArgs<PSObject>> SetMinRunspacesReceived;
        /// This event is raised when there is a request to get the
        /// available runspaces in the runspace pool.
        internal event EventHandler<RemoteDataEventArgs<PSObject>> GetAvailableRunspacesReceived;
        /// This event is raised when the client requests the runspace state
        /// to be reset.
        internal event EventHandler<RemoteDataEventArgs<PSObject>> ResetRunspaceState;
        /// <remarks>This overload takes a RemoteDataObject and should
        /// be the one that's used to send data from within this
        /// data structure handler class</remarks>
            Dbg.Assert(data != null, "Cannot send null object.");
            _transportManager.SendDataToClient(data, true);
        /// Get the associated powershell data structure handler for the specified
        /// powershell id.
        /// <param name="clientPowerShellId">powershell id for the
        /// powershell data structure handler</param>
        internal ServerPowerShellDataStructureHandler GetAssociatedPowerShellDataStructureHandler
            ServerPowerShellDataStructureHandler dsHandler = null;
                bool success = _associatedShells.TryGetValue(clientPowerShellId, out dsHandler);
            Dbg.Assert(sender is ServerPowerShellDataStructureHandler, @"sender of the event
                must be ServerPowerShellDataStructureHandler");
            ServerPowerShellDataStructureHandler dsHandler = sender as ServerPowerShellDataStructureHandler;
                _associatedShells.Remove(dsHandler.PowerShellId);
            // let session transport manager remove its association of command transport manager.
        // transport manager using which this
        // runspace pool driver handles all client
        // communication
        private readonly AbstractServerSessionTransportManager _transportManager;
        private readonly Dictionary<Guid, ServerPowerShellDataStructureHandler> _associatedShells
            = new Dictionary<Guid, ServerPowerShellDataStructureHandler>();
        // powershell data structure handlers associated with this
        // runspace pool data structure handler
    /// Handles all PowerShell data structure handler communication
    /// with the client side PowerShell.
    internal class ServerPowerShellDataStructureHandler
        // powershell driver handles all client
        private readonly RemoteStreamOptions _streamSerializationOptions;
        private Runspace _rsUsedToInvokePowerShell;
        /// Default constructor for creating ServerPowerShellDataStructureHandler
        /// <param name="transportManager">Transport manager.</param>
        /// <param name="localPowerShell">Local powershell object.</param>
        internal ServerPowerShellDataStructureHandler(Guid instanceId, Guid runspacePoolId, RemoteStreamOptions remoteStreamOptions,
            AbstractServerTransportManager transportManager, PowerShell localPowerShell)
            _clientPowerShellId = instanceId;
            _clientRunspacePoolId = runspacePoolId;
            _streamSerializationOptions = remoteStreamOptions;
            transportManager.Closing += HandleTransportClosing;
            if (localPowerShell != null)
                localPowerShell.RunspaceAssigned += LocalPowerShell_RunspaceAssigned;
        private void LocalPowerShell_RunspaceAssigned(object sender, PSEventArgs<Runspace> e)
            _rsUsedToInvokePowerShell = e.Args;
        /// Prepare transport manager to send data to client.
        internal void Prepare()
            // When Guid.Empty is used, PowerShell must be using pool's transport manager
            // to send data to client. so we dont need to prepare command transport manager
            if (_clientPowerShellId != Guid.Empty)
                _transportManager.Prepare();
        /// Send the state information to the client.
        /// <param name="stateInfo">state information to be
        /// sent to the client</param>
        internal void SendStateChangedInformationToClient(PSInvocationStateInfo
            stateInfo)
            Dbg.Assert((stateInfo.State == PSInvocationState.Completed) ||
                       (stateInfo.State == PSInvocationState.Failed) ||
                       (stateInfo.State == PSInvocationState.Stopped),
                       "SendStateChangedInformationToClient should be called to notify a termination state");
            SendDataAsync(RemotingEncoder.GeneratePowerShellStateInfo(
                stateInfo, _clientPowerShellId, _clientRunspacePoolId));
            // Close the transport manager only if the PowerShell Guid != Guid.Empty.
            // to send data to client.
                // no need to listen for closing events as we are initiating the close
                _transportManager.Closing -= HandleTransportClosing;
                // if terminal state is reached close the transport manager instead of letting
                // the client initiate the close.
                _transportManager.Close(null);
        /// Send the output data to the client.
        internal void SendOutputDataToClient(PSObject data)
            SendDataAsync(RemotingEncoder.GeneratePowerShellOutput(data,
                _clientPowerShellId, _clientRunspacePoolId));
        /// Send the error record to client.
        /// <param name="errorRecord">Error record to send.</param>
        internal void SendErrorRecordToClient(ErrorRecord errorRecord)
            errorRecord.SerializeExtendedInfo = (_streamSerializationOptions & RemoteStreamOptions.AddInvocationInfoToErrorRecord) != 0;
            SendDataAsync(RemotingEncoder.GeneratePowerShellError(
                errorRecord, _clientRunspacePoolId, _clientPowerShellId));
        /// Send the specified warning record to client.
        /// <param name="record">Warning record.</param>
        internal void SendWarningRecordToClient(WarningRecord record)
            record.SerializeExtendedInfo = (_streamSerializationOptions & RemoteStreamOptions.AddInvocationInfoToWarningRecord) != 0;
            SendDataAsync(RemotingEncoder.GeneratePowerShellInformational(
                record, _clientRunspacePoolId, _clientPowerShellId, RemotingDataType.PowerShellWarning));
        /// Send the specified debug record to client.
        /// <param name="record">Debug record.</param>
        internal void SendDebugRecordToClient(DebugRecord record)
            record.SerializeExtendedInfo = (_streamSerializationOptions & RemoteStreamOptions.AddInvocationInfoToDebugRecord) != 0;
                record, _clientRunspacePoolId, _clientPowerShellId, RemotingDataType.PowerShellDebug));
        /// Send the specified verbose record to client.
        internal void SendVerboseRecordToClient(VerboseRecord record)
            record.SerializeExtendedInfo = (_streamSerializationOptions & RemoteStreamOptions.AddInvocationInfoToVerboseRecord) != 0;
                record, _clientRunspacePoolId, _clientPowerShellId, RemotingDataType.PowerShellVerbose));
        /// Send the specified progress record to client.
        /// <param name="record">Progress record.</param>
        internal void SendProgressRecordToClient(ProgressRecord record)
                record, _clientRunspacePoolId, _clientPowerShellId));
        /// Send the specified information record to client.
        /// <param name="record">Information record.</param>
        internal void SendInformationRecordToClient(InformationRecord record)
        /// calls into observers of this event.
        /// observers include corresponding driver that shutdown
        /// input stream is present.
            OnSessionConnected.SafeInvoke(this, EventArgs.Empty);
        /// Process the data received from the powershell on
        /// the client.
                "RemotingTargetInterface must be PowerShell");
                case RemotingDataType.StopPowerShell:
                        Dbg.Assert(StopPowerShellReceived != null,
                            "ServerPowerShellDriver should subscribe to all data structure handler events");
                        StopPowerShellReceived.SafeInvoke(this, EventArgs.Empty);
                case RemotingDataType.PowerShellInput:
                        Dbg.Assert(InputReceived != null,
                        InputReceived.SafeInvoke(this, new RemoteDataEventArgs<object>(receivedData.Data));
                case RemotingDataType.PowerShellInputEnd:
                        Dbg.Assert(InputEndReceived != null,
                        InputEndReceived.SafeInvoke(this, EventArgs.Empty);
                case RemotingDataType.RemotePowerShellHostResponseData:
            Dbg.Assert(RemoveAssociation != null, @"The ServerRunspacePoolDataStructureHandler should subscribe
                to the RemoveAssociation event of ServerPowerShellDataStructureHandler");
        /// Creates a ServerRemoteHost which is associated with this powershell.
        /// <param name="powerShellHostInfo">Host information about the host associated
        /// PowerShell object on the client.</param>
        /// <param name="runspaceServerRemoteHost">Host associated with the RunspacePool
        /// on the server.</param>
        /// <returns>A new ServerRemoteHost for the PowerShell.</returns>
        internal ServerRemoteHost GetHostAssociatedWithPowerShell(
            HostInfo powerShellHostInfo,
            ServerRemoteHost runspaceServerRemoteHost)
            // If host was null use the runspace's host for this powershell; otherwise,
            // use the HostInfo to create a proxy host of the powershell's host.
            if (powerShellHostInfo.UseRunspaceHost)
                hostInfo = runspaceServerRemoteHost.HostInfo;
                hostInfo = powerShellHostInfo;
            // If the host was not null on the client, then the PowerShell object should
            // get a brand spanking new host.
            return new ServerRemoteHost(_clientRunspacePoolId, _clientPowerShellId, hostInfo,
                _transportManager, runspaceServerRemoteHost.Runspace, runspaceServerRemoteHost as ServerDriverRemoteHost);
        /// This event is raised when the a message to stop the
        /// powershell is received from the client.
        internal event EventHandler StopPowerShellReceived;
        /// This event is raised when an input object is received
        /// from the client.
        internal event EventHandler<RemoteDataEventArgs<object>> InputReceived;
        /// This event is raised when end of input is received from
        internal event EventHandler InputEndReceived;
        /// Raised when server session is connected from a new client.
        internal event EventHandler OnSessionConnected;
        /// This event is raised when a host response is received.
        /// Client powershell id.
        /// Runspace used to invoke PowerShell, this is used by the steppable
        /// pipeline driver.
        internal Runspace RunspaceUsedToInvokePowerShell
            get { return _rsUsedToInvokePowerShell; }
            // this is from a command execution..let transport manager collect
            // as much data as possible and send bigger buffer to client.
            _transportManager.SendDataToClient(data, false);
        /// Handle transport manager's closing event.
        private void HandleTransportClosing(object sender, EventArgs args)
            StopPowerShellReceived.SafeInvoke(this, args);

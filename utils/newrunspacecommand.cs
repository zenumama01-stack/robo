    /// This cmdlet establishes a new Runspace either on the local machine or
    /// on the specified remote machine(s). The runspace established can be used
    /// to invoke expressions remotely.
    /// Open a local runspace
    /// $rs = New-PSSession
    /// Open a runspace to a remote system.
    /// $rs = New-PSSession -Machine PowerShellWorld
    /// Create a runspace specifying that it is globally scoped.
    /// $global:rs = New-PSSession -Machine PowerShellWorld
    /// Create a collection of runspaces
    /// $runspaces = New-PSSession -Machine PowerShellWorld,PowerShellPublish,PowerShellRepo
    /// Create a set of Runspaces using the Secure Socket Layer by specifying the URI form.
    /// This assumes that an shell by the name of E12 exists on the remote server.
    ///     $serverURIs = 1..8 | ForEach-Object { "SSL://server${_}:443/E12" }
    ///     $rs = New-PSSession -URI $serverURIs
    /// Create a runspace by connecting to port 8081 on servers s1, s2 and s3
    /// $rs = New-PSSession -computername s1,s2,s3 -port 8081
    /// Create a runspace by connecting to port 443 using ssl on servers s1, s2 and s3
    /// $rs = New-PSSession -computername s1,s2,s3 -port 443 -useSSL
    /// Create a runspace by connecting to port 8081 on server s1 and run shell named E12.
    /// This assumes that a shell by the name E12 exists on the remote server
    /// $rs = New-PSSession -computername s1 -port 8061 -ShellName E12.
    [Cmdlet(VerbsCommon.New, "PSSession", DefaultParameterSetName = "ComputerName",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096484", RemotingCapability = RemotingCapability.OwnedByCommand)]
    public class NewPSSessionCommand : PSRemotingBaseCmdlet, IDisposable
                   ParameterSetName = NewPSSessionCommand.ComputerNameParameterSet)]
                   ParameterSetName = NewPSSessionCommand.SessionParameterSet)]
        /// Friendly names for the new PSSessions.
        [Parameter(ParameterSetName = NewPSSessionCommand.SessionParameterSet)]
                   ParameterSetName = NewPSSessionCommand.UriParameterSet)]
                   ParameterSetName = NewPSSessionCommand.ContainerIdParameterSet)]
                   ParameterSetName = NewPSSessionCommand.VMIdParameterSet)]
                   ParameterSetName = NewPSSessionCommand.VMNameParameterSet)]
        /// Gets or sets parameter value that creates connection to a Windows PowerShell process.
        [Parameter(Mandatory = true, ParameterSetName = NewPSSessionCommand.UseWindowsPowerShellParameterSet)]
        /// The throttle limit will be set here as it needs to be done
        /// only once per cmdlet and not for every call.
                if ((ParameterSetName == NewPSSessionCommand.ComputerNameParameterSet) ||
                    (ParameterSetName == NewPSSessionCommand.UriParameterSet))
        /// The runspace objects will be created using OpenAsync.
        /// At the end, the method will check if any runspace
        /// opened has already become available. If so, then it
        /// will be written to the pipeline.
            List<RemoteRunspace> remoteRunspaces = null;
            List<IThrottleOperation> operations = new List<IThrottleOperation>();
                case NewPSSessionCommand.SessionParameterSet:
                        remoteRunspaces = CreateRunspacesWhenRunspaceParameterSpecified();
                case "Uri":
                        remoteRunspaces = CreateRunspacesWhenUriParameterSpecified();
                case NewPSSessionCommand.ComputerNameParameterSet:
                        remoteRunspaces = CreateRunspacesWhenComputerNameParameterSpecified();
                case NewPSSessionCommand.VMIdParameterSet:
                case NewPSSessionCommand.VMNameParameterSet:
                        remoteRunspaces = CreateRunspacesWhenVMParameterSpecified();
                case NewPSSessionCommand.ContainerIdParameterSet:
                        remoteRunspaces = CreateRunspacesWhenContainerParameterSpecified();
                case NewPSSessionCommand.SSHHostParameterSet:
                        remoteRunspaces = CreateRunspacesForSSHHostParameterSet();
                case NewPSSessionCommand.SSHHostHashParameterSet:
                        remoteRunspaces = CreateRunspacesForSSHHostHashParameterSet();
                case NewPSSessionCommand.UseWindowsPowerShellParameterSet:
                        if (UseWindowsPowerShell)
                            remoteRunspaces = CreateRunspacesForUseWindowsPowerShellParameterSet();
                            // When -UseWindowsPowerShell:$false is explicitly specified,
                            // fall back to the default ComputerName parameter set behavior
                            goto case NewPSSessionCommand.ComputerNameParameterSet;
                        Dbg.Assert(false, "Missing parameter set in switch statement");
                        remoteRunspaces = new List<RemoteRunspace>(); // added to avoid prefast warning
            foreach (RemoteRunspace remoteRunspace in remoteRunspaces)
                OpenRunspaceOperation operation = new OpenRunspaceOperation(remoteRunspace);
                // HandleRunspaceStateChanged callback is added before ThrottleManager complete
                // callback handlers so HandleRunspaceStateChanged will always be called first.
                operation.OperationComplete += HandleRunspaceStateChanged;
                operations.Add(operation);
            // submit list of operations to throttle manager to start opening
            // runspaces
            // Add to list for clean up.
            _allOperations.Add(operations);
            // If there are any runspaces opened asynchronously
            // that are ready now, check their status and do
            // necessary action. If there are any error records
            // or verbose messages write them as well
            Collection<object> streamObjects =
                _stream.ObjectReader.NonBlockingRead();
        /// OpenAsync would have been called from ProcessRecord. This method
        /// will wait until all runspaces are opened and then write them to
        /// the pipeline as and when they become available.
            // signal to throttle manager end of submit operations
                // Keep reading objects until end of pipeline is encountered
        /// creating all the runspaces (basically the runspaces its
        /// waiting on OpenAsync is made available). However, when a stop
        /// signal is sent, CloseAsyn needs to be called to close all the
        /// pending runspaces.
            // close the outputStream so that further writes to the outputStream
            // are not possible
            // for all the runspaces that have been submitted for opening
            // call StopOperation on each object and quit
        private void OnRunspacePSEventReceived(object sender, PSEventArgs e) => this.Events?.AddForwardedEvent(e);
            Action<Cmdlet> warningWriter = (Cmdlet cmdlet) => cmdlet.WriteWarning(message);
            _stream.Write(warningWriter);
        /// Handles state changes for Runspace.
        /// <param name="stateEventArgs">Event information object which describes
        /// the event which triggered this method</param>
        private void HandleRunspaceStateChanged(object sender, OperationStateEventArgs stateEventArgs)
                throw PSTraceSource.NewArgumentNullException(nameof(sender));
            if (stateEventArgs == null)
                throw PSTraceSource.NewArgumentNullException(nameof(stateEventArgs));
            RunspaceStateEventArgs runspaceStateEventArgs =
                        stateEventArgs.BaseEvent as RunspaceStateEventArgs;
            RunspaceStateInfo stateInfo = runspaceStateEventArgs.RunspaceStateInfo;
            RunspaceState state = stateInfo.State;
            OpenRunspaceOperation operation = sender as OpenRunspaceOperation;
            RemoteRunspace remoteRunspace = operation.OperatedRunspace;
            PipelineWriter writer = _stream.ObjectWriter;
            Exception reason = runspaceStateEventArgs.RunspaceStateInfo.Reason;
                        // Indicates that runspace is successfully opened
                        // Write it to PipelineWriter to be handled in
                        // HandleRemoteRunspace
                        PSSession remoteRunspaceInfo = new PSSession(remoteRunspace);
                        this.RunspaceRepository.Add(remoteRunspaceInfo);
                        Action<Cmdlet> outputWriter = (Cmdlet cmdlet) => cmdlet.WriteObject(remoteRunspaceInfo);
                        if (writer.IsOpen)
                            writer.Write(outputWriter);
                        // Open resulted in a broken state. Extract reason
                        // and write an error record
                            reason as PSRemotingTransportException;
                        int transErrorCode = 0;
                            OpenRunspaceOperation senderAsOp = sender as OpenRunspaceOperation;
                            transErrorCode = transException.ErrorCode;
                            if (senderAsOp != null)
                                string host = senderAsOp.OperatedRunspace.ConnectionInfo.ComputerName;
                                    System.Management.Automation.Remoting.Client.WSManNativeApi.ERROR_WSMAN_REDIRECT_REQUESTED)
                                    errorDetails = "[" + host + "] " + message;
                                    errorDetails = "[" + host + "] ";
                                    if (!string.IsNullOrEmpty(transException.Message))
                        // add host identification information in data structure handler message
                        PSRemotingDataStructureException protoException = reason as PSRemotingDataStructureException;
                        if (protoException != null)
                                errorDetails = "[" + host + "] " + protoException.Message;
                        reason ??= new RuntimeException(this.GetMessage(RemotingErrorIdStrings.RemoteRunspaceOpenUnknownState, state));
                        string fullyQualifiedErrorId = WSManTransportManagerUtils.GetFQEIDFromTransportError(
                            transErrorCode,
                            _defaultFQEID);
                        if (transErrorCode == WSManNativeApi.ERROR_WSMAN_NO_LOGON_SESSION_EXIST)
                            errorDetails += System.Environment.NewLine + string.Format(System.Globalization.CultureInfo.CurrentCulture, RemotingErrorIdStrings.RemotingErrorNoLogonSessionExist);
                        ErrorRecord errorRecord = new ErrorRecord(reason,
                             remoteRunspace, fullyQualifiedErrorId,
                                   ErrorCategory.OpenError, null, null,
                                        null, null, null, errorDetails, null);
                        Action<Cmdlet> errorWriter = (Cmdlet cmdlet) =>
                            if ((errorRecord.Exception != null) &&
                                    errorRecord = new ErrorRecord(errorRecord.Exception.InnerException,
                            writer.Write(errorWriter);
                        _toDispose.Add(remoteRunspace);
                        // The runspace was closed possibly because the user
                        // hit ctrl-C when runspaces were being opened or Dispose has been
                        // called when there are open runspaces
                        Uri connectionUri = WSManConnectionInfo.ExtractPropertyAsWsManConnectionInfo<Uri>(remoteRunspace.ConnectionInfo,
                            GetMessage(RemotingErrorIdStrings.RemoteRunspaceClosed,
                                        (connectionUri != null) ?
                                        connectionUri.AbsoluteUri : string.Empty);
                        Action<Cmdlet> verboseWriter = (Cmdlet cmdlet) => cmdlet.WriteVerbose(message);
                            writer.Write(verboseWriter);
                        // runspace may not have been opened in certain cases
                        // like when the max memory is set to 25MB, in such
                        // cases write an error record
                                 "PSSessionStateClosed",
                                       ErrorCategory.OpenError, remoteRunspace);
        /// Creates the remote runspace objects when PSSession
        /// parameter is specified
        /// It now supports PSSession based on VM/container connection info as well.
        private List<RemoteRunspace> CreateRunspacesWhenRunspaceParameterSpecified()
            List<RemoteRunspace> remoteRunspaces = new List<RemoteRunspace>();
            // validate the runspaces specified before processing them.
            // The function will result in terminating errors, if any
            // validation failure is encountered
            int rsIndex = 0;
                if (remoteRunspaceInfo == null || remoteRunspaceInfo.Runspace == null)
                        new ArgumentNullException("PSSession"), "PSSessionArgumentNull",
                    // clone the object based on what's specified in the input parameter
                        RemoteRunspace remoteRunspace = (RemoteRunspace)remoteRunspaceInfo.Runspace;
                        RunspaceConnectionInfo newConnectionInfo = null;
                        if (remoteRunspace.ConnectionInfo is VMConnectionInfo)
                            newConnectionInfo = remoteRunspace.ConnectionInfo.Clone();
                        else if (remoteRunspace.ConnectionInfo is ContainerConnectionInfo)
                            ContainerConnectionInfo newContainerConnectionInfo = remoteRunspace.ConnectionInfo.Clone() as ContainerConnectionInfo;
                            newContainerConnectionInfo.CreateContainerProcess();
                            newConnectionInfo = newContainerConnectionInfo;
                            // WSMan case
                            WSManConnectionInfo originalWSManConnectionInfo = remoteRunspace.ConnectionInfo as WSManConnectionInfo;
                            WSManConnectionInfo newWSManConnectionInfo = null;
                            if (originalWSManConnectionInfo != null)
                                newWSManConnectionInfo = originalWSManConnectionInfo.Copy();
                                newWSManConnectionInfo.EnableNetworkAccess = newWSManConnectionInfo.EnableNetworkAccess || EnableNetworkAccess;
                                newConnectionInfo = newWSManConnectionInfo;
                                string shellUri = WSManConnectionInfo.ExtractPropertyAsWsManConnectionInfo<string>(remoteRunspace.ConnectionInfo,
                                newWSManConnectionInfo = new WSManConnectionInfo(connectionUri,
                                                                shellUri,
                                                                remoteRunspace.ConnectionInfo.Credential);
                                UpdateConnectionInfo(newWSManConnectionInfo);
                                newWSManConnectionInfo.EnableNetworkAccess = EnableNetworkAccess;
                        RemoteRunspacePoolInternal rrsPool = remoteRunspace.RunspacePool.RemoteRunspacePoolInternal;
                        TypeTable typeTable = null;
                        if ((rrsPool != null) &&
                            (rrsPool.DataStructureHandler != null) &&
                            (rrsPool.DataStructureHandler.TransportManager != null))
                            typeTable = rrsPool.DataStructureHandler.TransportManager.Fragmentor.TypeTable;
                        // Create new remote runspace with name and Id.
                        string rsName = GetRunspaceName(rsIndex, out rsId);
                        RemoteRunspace newRemoteRunspace = new RemoteRunspace(
                            typeTable, newConnectionInfo, this.Host, this.SessionOption.ApplicationArguments,
                            rsName, rsId);
                        remoteRunspaces.Add(newRemoteRunspace);
                                ErrorCategory.InvalidArgument, remoteRunspaceInfo);
                ++rsIndex;
            return remoteRunspaces;
        /// Creates the remote runspace objects when the URI parameter
        /// is specified.
        private List<RemoteRunspace> CreateRunspacesWhenUriParameterSpecified()
            // parse the Uri to obtain information about the runspace
            // required
                    string rsName = GetRunspaceName(i, out rsId);
                        Utils.GetTypeTableFromExecutionContextTLS(), connectionInfo, this.Host,
                        this.SessionOption.ApplicationArguments, rsName, rsId);
                    remoteRunspaces.Add(remoteRunspace);
        /// Creates the remote runspace objects when the ComputerName parameter
        private List<RemoteRunspace> CreateRunspacesWhenComputerNameParameterSpecified()
            List<RemoteRunspace> remoteRunspaces =
                new List<RemoteRunspace>();
            // Resolve all the machine names
            string[] resolvedComputerNames;
            ValidateComputerName(resolvedComputerNames);
            // Do for each machine
                    connectionInfo.ComputerName = resolvedComputerNames[i];
                    RemoteRunspace runspace = new RemoteRunspace(
                    remoteRunspaces.Add(runspace);
                            ErrorCategory.InvalidArgument, resolvedComputerNames[i]);
        /// Creates the remote runspace objects when the VMId or VMName parameter
        private List<RemoteRunspace> CreateRunspacesWhenVMParameterSpecified()
            bool isVMIdSet = false;
            if (ParameterSetName == PSExecutionCmdlet.VMIdParameterSet)
                isVMIdSet = true;
                Dbg.Assert((ParameterSetName == PSExecutionCmdlet.VMNameParameterSet),
                           "Expected ParameterSetName == VMId or VMName");
                        command, false, PipelineResultTypes.None, null,
                        isVMIdSet ? this.VMId[index].ToString() : this.VMName[index]);
                // handle invalid input
                    if (isVMIdSet)
                RemoteRunspace runspace = null;
                string rsName = GetRunspaceName(index, out rsId);
                    runspace = new RemoteRunspace(Utils.GetTypeTableFromExecutionContextTLS(),
                        connectionInfo, this.Host, null, rsName, rsId);
        /// Creates the remote runspace objects when the ContainerId parameter is specified.
        private List<RemoteRunspace> CreateRunspacesWhenContainerParameterSpecified()
            Dbg.Assert((ParameterSetName == PSExecutionCmdlet.ContainerIdParameterSet),
                       "Expected ParameterSetName == ContainerId");
        /// CreateRunspacesForSSHHostParameterSet.
        private List<RemoteRunspace> CreateRunspacesForSSHHostParameterSet()
            var remoteRunspaces = new List<RemoteRunspace>();
            foreach (var computerName in resolvedComputerNames)
                    this.KeyFilePath,
                    port,
                    Subsystem,
                    ConnectingTimeout,
                    Options);
                string rsName = GetRunspaceName(index, out int rsIdUnused);
                remoteRunspaces.Add(RunspaceFactory.CreateRunspace(connectionInfo: sshConnectionInfo,
                                                                   host: this.Host,
                                                                   typeTable: typeTable,
                                                                   applicationArguments: null,
                                                                   name: rsName) as RemoteRunspace);
        private List<RemoteRunspace> CreateRunspacesForSSHHostHashParameterSet()
                    sshConnection.ConnectingTimeout,
                    sshConnection.Options);
        /// Helper method to create remote runspace based on UseWindowsPowerShell parameter set.
        /// <returns>Remote runspace that was created.</returns>
        private List<RemoteRunspace> CreateRunspacesForUseWindowsPowerShellParameterSet()
            connectionInfo.PSVersion = new Version(5, 1);
            string runspaceName = GetRunspaceName(0, out int runspaceIdUnused);
            remoteRunspaces.Add(RunspaceFactory.CreateRunspace(connectionInfo: connectionInfo,
                                                               name: runspaceName) as RemoteRunspace);
        /// Helper method to either get a user supplied runspace/session name
        /// or to generate one along with a unique Id.
        /// <param name="rsIndex">Runspace name array index.</param>
        /// <param name="rsId">Runspace Id.</param>
        /// <returns>Runspace name.</returns>
        private string GetRunspaceName(int rsIndex, out int rsId)
            // Get a unique session/runspace Id and default Name.
            // If there is a friendly name for the runspace, we need to pass it to the
            // runspace pool object, which in turn passes it on to the server during
            // construction.  This way the friendly name can be returned when querying
            // the sever for disconnected sessions/runspaces.
            if (Name != null && rsIndex < Name.Length)
                rsName = Name[rsIndex];
            return rsName;
                // wait for all runspace operations to be complete
                foreach (RemoteRunspace remoteRunspace in _toDispose)
                // Dispose all open operation objects, to remove runspace event callback.
                foreach (List<IThrottleOperation> operationList in _allOperations)
                    foreach (OpenRunspaceOperation operation in operationList)
                        operation.Dispose();
        /// Handles the throttling complete event of the throttle manager.
            // all operations are complete close the stream
                       e is ArgumentException || e is NotSupportedException,
                       "Exception has to be of type UriFormatException or InvalidOperationException or ArgumentException or NotSupportedException");
        // event that signals that all operations are
        // complete (including closing if any)
        // list of runspaces to dispose
        private readonly List<RemoteRunspace> _toDispose = new List<RemoteRunspace>();
        // List of runspace connect operations.  Need to keep for cleanup.
        private readonly Collection<List<IThrottleOperation>> _allOperations = new Collection<List<IThrottleOperation>>();
        // Default FQEID.
        private readonly string _defaultFQEID = "PSSessionOpenFailed";
    /// Class that implements the IThrottleOperation in turn wrapping the
    /// opening of a runspace asynchronously within it.
    internal class OpenRunspaceOperation : IThrottleOperation, IDisposable
        // Member variables to ensure that the ThrottleManager gets StartComplete
        // or StopComplete called only once per Start or Stop operation.
        private bool _startComplete;
        private bool _stopComplete;
        internal RemoteRunspace OperatedRunspace { get; }
        internal OpenRunspaceOperation(RemoteRunspace runspace)
            _startComplete = true;
            _stopComplete = true;
            OperatedRunspace = runspace;
            OperatedRunspace.StateChanged += HandleRunspaceStateChanged;
        /// Opens the runspace asynchronously.
                _startComplete = false;
            OperatedRunspace.OpenAsync();
        /// Closes the runspace already opened asynchronously.
            OperationStateEventArgs operationStateEventArgs = null;
                // Ignore stop operation if start operation has completed.
                if (_startComplete)
                    operationStateEventArgs = new OperationStateEventArgs();
                    operationStateEventArgs.BaseEvent = new RunspaceStateEventArgs(OperatedRunspace.RunspaceStateInfo);
                    _stopComplete = false;
            if (operationStateEventArgs != null)
                FireEvent(operationStateEventArgs);
                OperatedRunspace.CloseAsync();
        // OperationComplete event handler uses an internal collection of event handler
        // callbacks for two reasons:
        //  a) To ensure callbacks are made in list order (first added, first called).
        //  b) To ensure all callbacks are fired by manually invoking callbacks and handling
        //     any exceptions thrown on this thread. (ThrottleManager will not respond if it doesn't
        //     get a start/stop complete callback).
        private readonly List<EventHandler<OperationStateEventArgs>> _internalCallbacks = new List<EventHandler<OperationStateEventArgs>>();
        internal override event EventHandler<OperationStateEventArgs> OperationComplete
                lock (_internalCallbacks)
                    _internalCallbacks.Add(value);
                    _internalCallbacks.Remove(value);
        /// Handler for handling runspace state changed events. This method will be
        /// registered in the StartOperation and StopOperation methods. This handler
        /// will in turn invoke the OperationComplete event for all events that are
        /// necessary - Opened, Closed, Disconnected, Broken. It will ignore all other state
        /// There are two problems that need to be handled.
        /// 1) We need to make sure that the ThrottleManager StartComplete and StopComplete
        ///    operation events are called or the ThrottleManager will never end (will stop responding).
        /// 2) The HandleRunspaceStateChanged event handler remains in the Runspace
        ///    StateChanged event call chain until this object is disposed.  We have to
        ///    disallow the HandleRunspaceStateChanged event from running and throwing
        ///    an exception since this prevents other event handlers in the chain from
        ///    being called.
        /// <param name="source">Source of this event.</param>
        /// <param name="stateEventArgs">object describing state information of the
        /// runspace</param>
        private void HandleRunspaceStateChanged(object source, RunspaceStateEventArgs stateEventArgs)
            // Disregard intermediate states.
            switch (stateEventArgs.RunspaceStateInfo.State)
                // We must call OperationComplete ony *once* for each Start/Stop operation.
                if (!_stopComplete)
                    // Note that the StopComplete callback removes *both* the Start and Stop
                    // operations from their respective queues.  So update the member vars
                    // accordingly.
                    operationStateEventArgs.BaseEvent = stateEventArgs;
                else if (!_startComplete)
                // Fire callbacks in list order.
        private void FireEvent(OperationStateEventArgs operationStateEventArgs)
            EventHandler<OperationStateEventArgs>[] copyCallbacks;
                copyCallbacks = new EventHandler<OperationStateEventArgs>[_internalCallbacks.Count];
                _internalCallbacks.CopyTo(copyCallbacks);
            foreach (var callbackDelegate in copyCallbacks)
                // Ensure all callbacks get called to prevent ThrottleManager from not responding.
                    callbackDelegate.SafeInvoke(this, operationStateEventArgs);
        /// Implements IDisposable.
            // Must remove the event callback from the new runspace or it will block other event
            // handling by throwing an exception on the event thread.
            OperatedRunspace.StateChanged -= HandleRunspaceStateChanged;

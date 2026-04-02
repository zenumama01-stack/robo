using System.Management.Automation.Runspaces;
    #region CimSessionWrapper
    internal class CimSessionWrapper
        /// Id of the cimsession.
        public uint SessionId { get; }
        /// InstanceId of the cimsession.
        public Guid InstanceId { get; }
        /// Name of the cimsession.
        public string Name { get; }
        /// Computer name of the cimsession.
        public string ComputerName { get; }
        /// Wrapped cimsession object.
        public CimSession CimSession { get; }
        public string Protocol
                switch (protocol)
                    case ProtocolType.Dcom:
                        return "DCOM";
                    case ProtocolType.Default:
                    case ProtocolType.Wsman:
                        return "WSMAN";
        internal ProtocolType GetProtocolType()
            return protocol;
        private readonly ProtocolType protocol;
        /// PSObject that wrapped the cimSession.
        private PSObject psObject;
        internal CimSessionWrapper(
            uint theSessionId,
            Guid theInstanceId,
            string theName,
            ProtocolType theProtocol)
            this.SessionId = theSessionId;
            this.InstanceId = theInstanceId;
            this.Name = theName;
            this.ComputerName = theComputerName;
            this.CimSession = theCimSession;
            this.psObject = null;
            this.protocol = theProtocol;
        internal PSObject GetPSObject()
            if (psObject == null)
                psObject = new PSObject(this.CimSession);
                psObject.Properties.Add(new PSNoteProperty(CimSessionState.idPropName, this.SessionId));
                psObject.Properties.Add(new PSNoteProperty(CimSessionState.namePropName, this.Name));
                psObject.Properties.Add(new PSNoteProperty(CimSessionState.instanceidPropName, this.InstanceId));
                psObject.Properties.Add(new PSNoteProperty(CimSessionState.computernamePropName, this.ComputerName));
                psObject.Properties.Add(new PSNoteProperty(CimSessionState.protocolPropName, this.Protocol));
                psObject.Properties[CimSessionState.idPropName].Value = this.SessionId;
                psObject.Properties[CimSessionState.namePropName].Value = this.Name;
                psObject.Properties[CimSessionState.instanceidPropName].Value = this.InstanceId;
                psObject.Properties[CimSessionState.computernamePropName].Value = this.ComputerName;
                psObject.Properties[CimSessionState.protocolPropName].Value = this.Protocol;
            return psObject;
    #region CimSessionState
    /// Class used to hold all cimsession related status data related to a runspace.
    /// Including the CimSession cache, session counters for generating session name.
    internal class CimSessionState : IDisposable
        /// Default session name.
        /// If a name is not passed, then the session is given the name CimSession<int>,
        /// where <int> is the next available session number.
        /// For example, CimSession1, CimSession2, etc...
        internal static readonly string CimSessionClassName = "CimSession";
        /// CimSession object name.
        internal static readonly string CimSessionObject = "{CimSession Object}";
        /// CimSession object path, which is identifying a cimsession object
        internal static readonly string SessionObjectPath = @"CimSession id = {0}, name = {2}, ComputerName = {3}, instance id = {1}";
        /// Id property name of cimsession wrapper object.
        internal static readonly string idPropName = "Id";
        /// Instanceid property name of cimsession wrapper object.
        internal static readonly string instanceidPropName = "InstanceId";
        /// Name property name of cimsession wrapper object.
        internal static readonly string namePropName = "Name";
        /// Computer name property name of cimsession object.
        internal static readonly string computernamePropName = "ComputerName";
        /// Protocol name property name of cimsession object.
        internal static readonly string protocolPropName = "Protocol";
        /// Session counter bound to current runspace.
        private uint sessionNameCounter;
        /// Dictionary used to holds all CimSessions in current runspace by session name.
        private readonly Dictionary<string, HashSet<CimSessionWrapper>> curCimSessionsByName;
        /// Dictionary used to holds all CimSessions in current runspace by computer name.
        private readonly Dictionary<string, HashSet<CimSessionWrapper>> curCimSessionsByComputerName;
        /// Dictionary used to holds all CimSessions in current runspace by instance ID.
        private readonly Dictionary<Guid, CimSessionWrapper> curCimSessionsByInstanceId;
        /// Dictionary used to holds all CimSessions in current runspace by session id.
        private readonly Dictionary<uint, CimSessionWrapper> curCimSessionsById;
        /// Dictionary used to link CimSession object with PSObject.
        private readonly Dictionary<CimSession, CimSessionWrapper> curCimSessionWrapper;
        /// Initializes a new instance of the <see cref="CimSessionState"/> class.
        internal CimSessionState()
            sessionNameCounter = 1;
            curCimSessionsByName = new Dictionary<string, HashSet<CimSessionWrapper>>(
                StringComparer.OrdinalIgnoreCase);
            curCimSessionsByComputerName = new Dictionary<string, HashSet<CimSessionWrapper>>(
            curCimSessionsByInstanceId = new Dictionary<Guid, CimSessionWrapper>();
            curCimSessionsById = new Dictionary<uint, CimSessionWrapper>();
            curCimSessionWrapper = new Dictionary<CimSession, CimSessionWrapper>();
        /// Get sessions count.
        /// <returns>The count of session objects in current runspace.</returns>
        internal int GetSessionsCount()
            return this.curCimSessionsById.Count;
        /// Generates an unique session id.
        /// <returns>Unique session id under current runspace.</returns>
        internal uint GenerateSessionId()
            return this.sessionNameCounter++;
        /// Indicates whether this object was disposed or not.
                    this._disposed = true;
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources.
        public void Cleanup()
            foreach (CimSession session in curCimSessionWrapper.Keys)
                session.Dispose();
            curCimSessionWrapper.Clear();
            curCimSessionsByName.Clear();
            curCimSessionsByComputerName.Clear();
            curCimSessionsByInstanceId.Clear();
            curCimSessionsById.Clear();
        #region Add CimSession to/remove CimSession from cache
        /// Add new CimSession object to cache.
        /// <param name="sessionId"></param>
        /// <param name="instanceId"></param>
        /// <param name="name"></param>
        /// <param name="protocol"></param>
        internal PSObject AddObjectToCache(
            uint sessionId,
            Guid instanceId,
            string name,
            ProtocolType protocol)
            CimSessionWrapper wrapper = new(
                sessionId, instanceId, name, computerName, session, protocol);
            HashSet<CimSessionWrapper> objects;
            if (!this.curCimSessionsByComputerName.TryGetValue(computerName, out objects))
                objects = new HashSet<CimSessionWrapper>();
                this.curCimSessionsByComputerName.Add(computerName, objects);
            objects.Add(wrapper);
            if (!this.curCimSessionsByName.TryGetValue(name, out objects))
                this.curCimSessionsByName.Add(name, objects);
            this.curCimSessionsByInstanceId.Add(instanceId, wrapper);
            this.curCimSessionsById.Add(sessionId, wrapper);
            this.curCimSessionWrapper.Add(session, wrapper);
            return wrapper.GetPSObject();
        /// Generates remove session message by given wrapper object.
        /// <param name="psObject"></param>
        internal string GetRemoveSessionObjectTarget(PSObject psObject)
            string message = string.Empty;
            if (psObject.BaseObject is CimSession)
                uint id = 0x0;
                Guid instanceId = Guid.Empty;
                string name = string.Empty;
                string computerName = string.Empty;
                if (psObject.Properties[idPropName].Value is uint)
                    id = Convert.ToUInt32(psObject.Properties[idPropName].Value, null);
                if (psObject.Properties[instanceidPropName].Value is Guid)
                    instanceId = (Guid)psObject.Properties[instanceidPropName].Value;
                if (psObject.Properties[namePropName].Value is string)
                    name = (string)psObject.Properties[namePropName].Value;
                if (psObject.Properties[computernamePropName].Value is string)
                    computerName = (string)psObject.Properties[computernamePropName].Value;
                message = string.Format(CultureInfo.CurrentUICulture, SessionObjectPath, id, instanceId, name, computerName);
            return message;
        /// Remove given <see cref="PSObject"/> object from cache.
        internal void RemoveOneSessionObjectFromCache(PSObject psObject)
                RemoveOneSessionObjectFromCache(psObject.BaseObject as CimSession);
        /// Remove given <see cref="CimSession"/> object from cache.
        internal void RemoveOneSessionObjectFromCache(CimSession session)
            if (!this.curCimSessionWrapper.ContainsKey(session))
            CimSessionWrapper wrapper = this.curCimSessionWrapper[session];
            string name = wrapper.Name;
            string computerName = wrapper.ComputerName;
            DebugHelper.WriteLog("name {0}, computername {1}, id {2}, instanceId {3}", 1, name, computerName, wrapper.SessionId, wrapper.InstanceId);
            if (this.curCimSessionsByComputerName.TryGetValue(computerName, out objects))
                objects.Remove(wrapper);
            if (this.curCimSessionsByName.TryGetValue(name, out objects))
            RemoveSessionInternal(session, wrapper);
        /// Remove given <see cref="CimSession"/> object from partial of the cache only.
        private void RemoveSessionInternal(CimSession session, CimSessionWrapper wrapper)
            this.curCimSessionsByInstanceId.Remove(wrapper.InstanceId);
            this.curCimSessionsById.Remove(wrapper.SessionId);
            this.curCimSessionWrapper.Remove(session);
        #region Query CimSession from cache
        /// Add ErrorRecord to list.
        /// <param name="errRecords"></param>
        /// <param name="propertyValue"></param>
        private static void AddErrorRecord(
            ref List<ErrorRecord> errRecords,
            string propertyName,
            object propertyValue)
            errRecords.Add(
                new ErrorRecord(
                    new CimException(string.Format(CultureInfo.CurrentUICulture, CimCmdletStrings.CouldNotFindCimsessionObject, propertyName, propertyValue)),
                    string.Empty,
                    ErrorCategory.ObjectNotFound,
                    null));
        /// Query session list by given id array.
        /// <param name="ids"></param>
        /// <returns>List of session wrapper objects.</returns>
        internal IEnumerable<PSObject> QuerySession(
            IEnumerable<uint> ids,
            out IEnumerable<ErrorRecord> errorRecords)
            HashSet<PSObject> sessions = new();
            HashSet<uint> sessionIds = new();
            List<ErrorRecord> errRecords = new();
            errorRecords = errRecords;
            // NOTES: use template function to implement this will save duplicate code
            foreach (uint id in ids)
                if (this.curCimSessionsById.ContainsKey(id))
                    if (sessionIds.Add(id))
                        sessions.Add(this.curCimSessionsById[id].GetPSObject());
                    AddErrorRecord(ref errRecords, idPropName, id);
            return sessions;
        /// Query session list by given instance id array.
        /// <param name="instanceIds"></param>
            IEnumerable<Guid> instanceIds,
            foreach (Guid instanceid in instanceIds)
                if (this.curCimSessionsByInstanceId.ContainsKey(instanceid))
                    CimSessionWrapper wrapper = this.curCimSessionsByInstanceId[instanceid];
                    if (!sessionIds.Contains(wrapper.SessionId))
                        sessionIds.Add(wrapper.SessionId);
                        sessions.Add(wrapper.GetPSObject());
                    AddErrorRecord(ref errRecords, instanceidPropName, instanceid);
        /// Query session list by given name array.
        /// <param name="nameArray"></param>
        internal IEnumerable<PSObject> QuerySession(IEnumerable<string> nameArray,
            foreach (string name in nameArray)
                bool foundSession = false;
                WildcardPattern pattern = new(name, WildcardOptions.IgnoreCase);
                foreach (KeyValuePair<string, HashSet<CimSessionWrapper>> kvp in this.curCimSessionsByName)
                    if (pattern.IsMatch(kvp.Key))
                        HashSet<CimSessionWrapper> wrappers = kvp.Value;
                        foundSession = wrappers.Count > 0;
                        foreach (CimSessionWrapper wrapper in wrappers)
                if (!foundSession && !WildcardPattern.ContainsWildcardCharacters(name))
                    AddErrorRecord(ref errRecords, namePropName, name);
        /// Query session list by given computer name array.
        /// <param name="computernameArray"></param>
        internal IEnumerable<PSObject> QuerySessionByComputerName(
            IEnumerable<string> computernameArray,
            foreach (string computername in computernameArray)
                if (this.curCimSessionsByComputerName.ContainsKey(computername))
                    HashSet<CimSessionWrapper> wrappers = this.curCimSessionsByComputerName[computername];
                if (!foundSession)
                    AddErrorRecord(ref errRecords, computernamePropName, computername);
        /// Query session list by given session objects array.
        /// <param name="cimsessions"></param>
        internal IEnumerable<PSObject> QuerySession(IEnumerable<CimSession> cimsessions,
            foreach (CimSession cimsession in cimsessions)
                if (this.curCimSessionWrapper.ContainsKey(cimsession))
                    CimSessionWrapper wrapper = this.curCimSessionWrapper[cimsession];
                    AddErrorRecord(ref errRecords, CimSessionClassName, CimSessionObject);
        /// Query session wrapper object.
        /// <returns>Session wrapper.</returns>
        internal CimSessionWrapper QuerySession(CimSession cimsession)
            CimSessionWrapper wrapper;
            this.curCimSessionWrapper.TryGetValue(cimsession, out wrapper);
            return wrapper;
        /// Query session object with given CimSessionInstanceID.
        /// <param name="cimSessionInstanceId"></param>
        /// <returns>CimSession object.</returns>
        internal CimSession QuerySession(Guid cimSessionInstanceId)
            if (this.curCimSessionsByInstanceId.ContainsKey(cimSessionInstanceId))
                CimSessionWrapper wrapper = this.curCimSessionsByInstanceId[cimSessionInstanceId];
                return wrapper.CimSession;
    #region CimSessionBase
    /// Base class of all session operation classes.
    /// All sessions created will be held in a ConcurrentDictionary:cimSessions.
    /// It manages the lifecycle of the sessions being created for each
    /// runspace according to the state of the runspace.
    internal class CimSessionBase
        #region constructor
        /// Initializes a new instance of the <see cref="CimSessionBase"/> class.
        public CimSessionBase()
            this.sessionState = cimSessions.GetOrAdd(
                CurrentRunspaceId,
                (Guid instanceId) =>
                    if (Runspace.DefaultRunspace != null)
                        Runspace.DefaultRunspace.StateChanged += DefaultRunspace_StateChanged;
                    return new CimSessionState();
        /// Thread safe static dictionary to store session objects associated
        /// with each runspace, which is identified by a GUID. NOTE: cmdlet
        /// can running parallelly under more than one runspace(s).
        internal static readonly ConcurrentDictionary<Guid, CimSessionState> cimSessions
            = new();
        /// Default runspace Id.
        internal static readonly Guid defaultRunspaceId = Guid.Empty;
        /// Object used to hold all CimSessions and status data bound
        /// to current runspace.
        internal CimSessionState sessionState;
        /// Get current runspace id.
        private static Guid CurrentRunspaceId
                    return Runspace.DefaultRunspace.InstanceId;
                    return CimSessionBase.defaultRunspaceId;
        public static CimSessionState GetCimSessionState()
            CimSessionState state = null;
            cimSessions.TryGetValue(CurrentRunspaceId, out state);
            return state;
        /// Clean up the dictionaries if the runspace is closed or broken.
        /// <param name="sender">Runspace.</param>
        /// <param name="e">Event args.</param>
        private static void DefaultRunspace_StateChanged(object sender, RunspaceStateEventArgs e)
            Runspace runspace = (Runspace)sender;
            switch (e.RunspaceStateInfo.State)
                case RunspaceState.Broken:
                case RunspaceState.Closed:
                    CimSessionState state;
                    if (cimSessions.TryRemove(runspace.InstanceId, out state))
                        DebugHelper.WriteLog(string.Format(CultureInfo.CurrentUICulture, DebugHelper.runspaceStateChanged, runspace.InstanceId, e.RunspaceStateInfo.State));
                        state.Dispose();
                    runspace.StateChanged -= DefaultRunspace_StateChanged;
    #region CimTestConnection
    #region CimNewSession
    /// <c>CimNewSession</c> is the class to create cimSession
    /// based on given <c>NewCimSessionCommand</c>.
    internal class CimNewSession : CimSessionBase, IDisposable
        /// CimTestCimSessionContext.
        internal class CimTestCimSessionContext : XOperationContextBase
            /// Initializes a new instance of the <see cref="CimTestCimSessionContext"/> class.
            /// <param name="wrapper"></param>
            internal CimTestCimSessionContext(
                CimSessionWrapper wrapper)
                this.CimSessionWrapper = wrapper;
                this.nameSpace = null;
            /// <para>Namespace</para>
            internal CimSessionWrapper CimSessionWrapper { get; }
        /// Initializes a new instance of the <see cref="CimNewSession"/> class.
        internal CimNewSession() : base()
            this.cimTestSession = new CimTestSession();
            this.Disposed = false;
        /// Create a new <see cref="CimSession"/> base on given cmdlet
        /// and its parameter.
        /// <param name="sessionOptions"></param>
        /// <param name="credential"></param>
        internal void NewCimSession(NewCimSessionCommand cmdlet,
            CimSessionOptions sessionOptions,
            CimCredential credential)
                CimSessionProxy proxy;
                if (sessionOptions == null)
                    DebugHelper.WriteLog("Create CimSessionOption due to NewCimSessionCommand has null sessionoption", 1);
                    sessionOptions = CimSessionProxy.CreateCimSessionOption(computerName,
                        cmdlet.OperationTimeoutSec, credential);
                proxy = new CimSessionProxyTestConnection(computerName, sessionOptions);
                string computerNameValue = (computerName == ConstValue.NullComputerName) ? ConstValue.LocalhostComputerName : computerName;
                CimSessionWrapper wrapper = new(0, Guid.Empty, cmdlet.Name, computerNameValue, proxy.CimSession, proxy.Protocol);
                CimTestCimSessionContext context = new(proxy, wrapper);
                // Skip test the connection if user intend to
                if (cmdlet.SkipTestConnection.IsPresent)
                    AddSessionToCache(proxy.CimSession, context, new CmdletOperationBase(cmdlet));
                    // CimSession will be returned as part of TestConnection
                    this.cimTestSession.TestCimSession(computerName, proxy);
        /// Add session to global cache,
        internal void AddSessionToCache(CimSession cimSession, XOperationContextBase context, CmdletOperationBase cmdlet)
            CimTestCimSessionContext testCimSessionContext = context as CimTestCimSessionContext;
            uint sessionId = this.sessionState.GenerateSessionId();
            string originalSessionName = testCimSessionContext.CimSessionWrapper.Name;
            string sessionName = originalSessionName ?? string.Create(CultureInfo.CurrentUICulture, $"{CimSessionState.CimSessionClassName}{sessionId}");
            // detach CimSession from the proxy object
            CimSession createdCimSession = testCimSessionContext.Proxy.Detach();
            PSObject psObject = this.sessionState.AddObjectToCache(
                createdCimSession,
                sessionId,
                createdCimSession.InstanceId,
                sessionName,
                testCimSessionContext.CimSessionWrapper.ComputerName,
                testCimSessionContext.Proxy.Protocol);
            cmdlet.WriteObject(psObject, null);
        /// Process all actions in the action queue.
            this.cimTestSession.ProcessActions(cmdletOperation);
            this.cimTestSession.ProcessRemainActions(cmdletOperation);
        /// <see cref="CimTestSession"/> object.
        private readonly CimTestSession cimTestSession;
        #endregion // private members
        protected bool Disposed { get; private set; }
            if (!this.Disposed)
                    this.cimTestSession.Dispose();
                    this.Disposed = true;
    #region CimGetSession
    /// Get CimSession based on given id/instanceid/computername/name.
    internal class CimGetSession : CimSessionBase
        /// Initializes a new instance of the <see cref="CimGetSession"/> class.
        public CimGetSession() : base()
        /// Get <see cref="CimSession"/> objects based on the given cmdlet
        public void GetCimSession(GetCimSessionCommand cmdlet)
            IEnumerable<PSObject> sessionToGet = null;
            IEnumerable<ErrorRecord> errorRecords = null;
                case CimBaseCommand.ComputerNameSet:
                    if (cmdlet.ComputerName == null)
                        sessionToGet = this.sessionState.QuerySession(ConstValue.DefaultSessionName, out errorRecords);
                        sessionToGet = this.sessionState.QuerySessionByComputerName(cmdlet.ComputerName, out errorRecords);
                case CimBaseCommand.SessionIdSet:
                    sessionToGet = this.sessionState.QuerySession(cmdlet.Id, out errorRecords);
                case CimBaseCommand.InstanceIdSet:
                    sessionToGet = this.sessionState.QuerySession(cmdlet.InstanceId, out errorRecords);
                case CimBaseCommand.NameSet:
                    sessionToGet = this.sessionState.QuerySession(cmdlet.Name, out errorRecords);
            if (sessionToGet != null)
                foreach (PSObject psobject in sessionToGet)
                    cmdlet.WriteObject(psobject);
            if (errorRecords != null)
                foreach (ErrorRecord errRecord in errorRecords)
                    cmdlet.WriteError(errRecord);
    #region CimRemoveSession
    internal class CimRemoveSession : CimSessionBase
        /// Remove session action string.
        internal static readonly string RemoveCimSessionActionName = "Remove CimSession";
        /// Initializes a new instance of the <see cref="CimRemoveSession"/> class.
        public CimRemoveSession() : base()
        /// Remove the <see cref="CimSession"/> objects based on given cmdlet
        public void RemoveCimSession(RemoveCimSessionCommand cmdlet)
            IEnumerable<PSObject> sessionToRemove = null;
                case CimBaseCommand.CimSessionSet:
                    sessionToRemove = this.sessionState.QuerySession(cmdlet.CimSession, out errorRecords);
                    sessionToRemove = this.sessionState.QuerySessionByComputerName(cmdlet.ComputerName, out errorRecords);
                    sessionToRemove = this.sessionState.QuerySession(cmdlet.Id, out errorRecords);
                    sessionToRemove = this.sessionState.QuerySession(cmdlet.InstanceId, out errorRecords);
                    sessionToRemove = this.sessionState.QuerySession(cmdlet.Name, out errorRecords);
            if (sessionToRemove != null)
                foreach (PSObject psobject in sessionToRemove)
                    if (cmdlet.ShouldProcess(this.sessionState.GetRemoveSessionObjectTarget(psobject), RemoveCimSessionActionName))
                        this.sessionState.RemoveOneSessionObjectFromCache(psobject);
    #region CimTestSession
    /// Class <see cref="CimTestSession"/>, which is used to
    /// test cimsession and execute async operations.
    internal class CimTestSession : CimAsyncOperation
        /// Initializes a new instance of the <see cref="CimTestSession"/> class.
        internal CimTestSession()
        /// Test the session connection with
        /// given <see cref="CimSessionProxy"/> object.
        internal void TestCimSession(
            CimSessionProxy proxy)
            proxy.TestConnectionAsync();

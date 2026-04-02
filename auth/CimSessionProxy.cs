using System.Runtime.InteropServices;
using Microsoft.Management.Infrastructure.Generic;
    #region Context base class
    /// Context base class for cross operations
    /// for example, some cmdlets need to query instance first and then
    /// remove instance, those scenarios need context object transferred
    /// from one operation to another.
    internal abstract class XOperationContextBase
        internal string Namespace
                return this.nameSpace;
        protected string nameSpace;
        /// Session proxy.
        internal CimSessionProxy Proxy
                return this.proxy;
        protected CimSessionProxy proxy;
    /// Class provides all information regarding the
    /// current invocation to the .NET API.
    internal class InvocationContext
        /// Initializes a new instance of the <see cref="InvocationContext"/> class.
        internal InvocationContext(CimSessionProxy proxy)
            if (proxy != null)
                this.ComputerName = proxy.CimSession.ComputerName;
                this.TargetCimInstance = proxy.TargetCimInstance;
        internal InvocationContext(string computerName, CimInstance targetCimInstance)
            this.ComputerName = computerName;
            this.TargetCimInstance = targetCimInstance;
        /// ComputerName of the session
        /// <remarks>
        /// return value could be null
        /// </remarks>
        internal virtual string ComputerName { get; }
        /// CimInstance on which the current operation against.
        internal virtual CimInstance TargetCimInstance { get; }
    #region Preprocessing of result object interface
    /// Defines a method to preprocessing an result object before sending to
    /// output pipeline.
    [ComVisible(false)]
    internal interface IObjectPreProcess
        /// Performs pre processing of given result object.
        /// <returns>Pre-processed object.</returns>
        object Process(object resultObject);
    #region Eventargs class
    /// CmdletActionEventArgs holds a CimBaseAction object
    internal sealed class CmdletActionEventArgs : EventArgs
        /// Initializes a new instance of the <see cref="CmdletActionEventArgs"/> class.
        /// <param name="action">CimBaseAction object bound to the event.</param>
        public CmdletActionEventArgs(CimBaseAction action)
            this.Action = action;
        public readonly CimBaseAction Action;
    /// OperationEventArgs holds a cancellation object, and an operation.
    internal sealed class OperationEventArgs : EventArgs
        /// Initializes a new instance of the <see cref="OperationEventArgs"/> class.
        /// <param name="operationCancellation">Object used to cancel the operation.</param>
        /// <param name="operation">Async observable operation.</param>
        public OperationEventArgs(IDisposable operationCancellation,
            IObservable<object> operation,
            bool theSuccess)
            this.operationCancellation = operationCancellation;
            this.operation = operation;
            this.success = theSuccess;
        public readonly IDisposable operationCancellation;
        public readonly IObservable<object> operation;
        public readonly bool success;
    /// Wrapper of <see cref="CimSession"/> object.
    /// A CimSessionProxy object can only execute one operation at specific moment.
    internal class CimSessionProxy : IDisposable
        #region static members
        /// global operation counter
        private static long gOperationCounter = 0;
        /// Temporary CimSession cache lock.
        private static readonly object temporarySessionCacheLock = new();
        /// <para>temporary CimSession cache</para>
        /// <para>Temporary CimSession means the session is created by cimcmdlets,
        /// which is not created by <see cref="New-CimSession"/> cmdlet.
        /// Due to some cmdlet, such as <see cref="Remove-CimInstance"/>
        /// might need to split the operation into multiple stages, i.e., query
        /// CimInstance firstly, then remove the CimInstance resulted from query,
        /// such that the temporary CimSession need to be shared between
        /// multiple <see cref="CimSessionProxy"/> objects, introducing a
        /// temporary session cache is necessary to control the lifetime of the
        /// temporary CimSession objects.</para>
        /// Once the reference count of the CimSession is decreased to 0,
        /// then call Dispose on it.
        private static readonly Dictionary<CimSession, uint> temporarySessionCache = new();
        /// Add <see cref="CimSession"/> to temporary cache.
        /// If CimSession already present in cache, then increase the refcount by 1,
        /// otherwise insert it into the cache.
        /// <param name="session">CimSession to be added.</param>
        internal static void AddCimSessionToTemporaryCache(CimSession session)
            if (session != null)
                lock (temporarySessionCacheLock)
                    if (temporarySessionCache.ContainsKey(session))
                        temporarySessionCache[session]++;
                        DebugHelper.WriteLogEx(@"Increase cimsession ref count {0}", 1, temporarySessionCache[session]);
                        temporarySessionCache.Add(session, 1);
                        DebugHelper.WriteLogEx(@"Add cimsession to cache. Ref count {0}", 1, temporarySessionCache[session]);
        /// <para>Wrapper function to remove CimSession from cache</para>
        /// <param name="dispose">Whether need to dispose the <see cref="CimSession"/> object.</param>
        private static void RemoveCimSessionFromTemporaryCache(CimSession session,
            bool dispose)
                bool removed = false;
                        temporarySessionCache[session]--;
                        DebugHelper.WriteLogEx(@"Decrease cimsession ref count {0}", 1, temporarySessionCache[session]);
                        if (temporarySessionCache[session] == 0)
                            removed = true;
                            temporarySessionCache.Remove(session);
                // there is a race condition that if
                // one thread is waiting to add CimSession to cache,
                // while current thread is removing the CimSession,
                // then invalid CimSession may be added to cache.
                // Ignored this scenario in CimCmdlet implementation,
                // since the code inside cimcmdlet will not hit this
                // scenario anyway.
                if (removed && dispose)
                    DebugHelper.WriteLogEx(@"Dispose cimsession ", 1);
        /// Remove <see cref="CimSession"/> from temporary cache.
        /// If CimSession already present in cache, then decrease the refcount by 1,
        /// otherwise ignore.
        /// If refcount became 0, call dispose on the <see cref="CimSession"/> object.
        internal static void RemoveCimSessionFromTemporaryCache(CimSession session)
            RemoveCimSessionFromTemporaryCache(session, true);
        #region Event definitions
        public event EventHandler<CmdletActionEventArgs> OnNewCmdletAction;
        /// Event triggered when a new operation is started.
        public event EventHandler<OperationEventArgs> OnOperationCreated;
        /// Event triggered when a new operation is completed,
        /// either success or failed.
        public event EventHandler<OperationEventArgs> OnOperationDeleted;
        /// Initializes a new instance of the <see cref="CimSessionProxy"/> class.
        /// Then create wrapper object by given CimSessionProxy object.
        public CimSessionProxy(CimSessionProxy proxy)
            DebugHelper.WriteLogEx("protocol = {0}", 1, proxy.Protocol);
            CreateSetSession(null, proxy.CimSession, null, proxy.OperationOptions, proxy.IsTemporaryCimSession);
            this.Protocol = proxy.Protocol;
            this.OperationTimeout = proxy.OperationTimeout;
            this.isDefaultSession = proxy.isDefaultSession;
        /// Create <see cref="CimSession"/> by given computer name.
        /// Then create wrapper object.
        public CimSessionProxy(string computerName)
            CreateSetSession(computerName, null, null, null, false);
            this.isDefaultSession = computerName == ConstValue.NullComputerName;
        /// Create <see cref="CimSession"/> by given computer name
        /// and session options.
        public CimSessionProxy(string computerName, CimSessionOptions sessionOptions)
            CreateSetSession(computerName, null, sessionOptions, null, false);
        /// and cimInstance. Then create wrapper object.
        public CimSessionProxy(string computerName, CimInstance cimInstance)
            DebugHelper.WriteLogEx("ComputerName {0}; cimInstance.CimSessionInstanceID = {1}; cimInstance.CimSessionComputerName = {2}.",
                0,
                computerName,
                cimInstance.GetCimSessionInstanceId(),
                cimInstance.GetCimSessionComputerName());
            if (computerName != ConstValue.NullComputerName)
            Debug.Assert(cimInstance != null, "Caller should verify cimInstance != null");
            // computerName is null, fallback to create session from cimInstance
            CimSessionState state = CimSessionBase.GetCimSessionState();
            if (state != null)
                CimSession session = state.QuerySession(cimInstance.GetCimSessionInstanceId());
                    DebugHelper.WriteLogEx("Found the session from cache with InstanceID={0}.", 0, cimInstance.GetCimSessionInstanceId());
                    CreateSetSession(null, session, null, null, false);
            string cimsessionComputerName = cimInstance.GetCimSessionComputerName();
            CreateSetSession(cimsessionComputerName, null, null, null, false);
            this.isDefaultSession = cimsessionComputerName == ConstValue.NullComputerName;
            DebugHelper.WriteLogEx("Create a temp session with computerName = {0}.", 0, cimsessionComputerName);
        /// Create <see cref="CimSession"/> by given computer name,
        /// session options.
        /// <param name="operOptions">Used when create async operation.</param>
        public CimSessionProxy(string computerName, CimSessionOptions sessionOptions, CimOperationOptions operOptions)
            CreateSetSession(computerName, null, sessionOptions, operOptions, false);
        public CimSessionProxy(string computerName, CimOperationOptions operOptions)
            CreateSetSession(computerName, null, null, operOptions, false);
        /// Create wrapper object by given session object.
        public CimSessionProxy(CimSession session)
        public CimSessionProxy(CimSession session, CimOperationOptions operOptions)
            CreateSetSession(null, session, null, operOptions, false);
        /// Initialize CimSessionProxy object.
        /// <param name="options"></param>
        private void CreateSetSession(
            CimOperationOptions operOptions,
            bool temporaryCimSession)
            DebugHelper.WriteLogEx("computername {0}; cimsession {1}; sessionOptions {2}; operationOptions {3}.", 0, computerName, cimSession, sessionOptions, operOptions);
            lock (this.stateLock)
                this.CancelOperation = null;
                this.operation = null;
            InitOption(operOptions);
            this.Protocol = ProtocolType.Wsman;
            this.IsTemporaryCimSession = temporaryCimSession;
            if (cimSession != null)
                this.CimSession = cimSession;
                    CimSessionWrapper wrapper = state.QuerySession(cimSession);
                    if (wrapper != null)
                        this.Protocol = wrapper.GetProtocolType();
                if (sessionOptions != null)
                    if (sessionOptions is DComSessionOptions)
                        string defaultComputerName = ConstValue.IsDefaultComputerName(computerName) ? ConstValue.NullComputerName : computerName;
                        this.CimSession = CimSession.Create(defaultComputerName, sessionOptions);
                        this.Protocol = ProtocolType.Dcom;
                        this.CimSession = CimSession.Create(computerName, sessionOptions);
                    this.CimSession = CreateCimSessionByComputerName(computerName);
                this.IsTemporaryCimSession = true;
            if (this.IsTemporaryCimSession)
                AddCimSessionToTemporaryCache(this.CimSession);
            this.invocationContextObject = new InvocationContext(this);
            DebugHelper.WriteLog("Protocol {0}, Is temporary session ? {1}", 1, this.Protocol, this.IsTemporaryCimSession);
        #region set operation options
        /// Gets or sets a value indicating whether to retrieve localized information for the CIM class.
        public bool Amended
            get => OperationOptions.Flags.HasFlag(CimOperationFlags.LocalizedQualifiers);
                if (value)
                    OperationOptions.Flags |= CimOperationFlags.LocalizedQualifiers;
                    OperationOptions.Flags &= ~CimOperationFlags.LocalizedQualifiers;
        /// Set timeout value (seconds) of the operation.
        public uint OperationTimeout
                return (uint)this.OperationOptions.Timeout.TotalSeconds;
                DebugHelper.WriteLogEx("OperationTimeout {0},", 0, value);
                this.OperationOptions.Timeout = TimeSpan.FromSeconds((double)value);
        /// Set resource URI of the operation.
        public Uri ResourceUri
                return this.OperationOptions.ResourceUri;
                DebugHelper.WriteLogEx("ResourceUri {0},", 0, value);
                this.OperationOptions.ResourceUri = value;
        /// Enable/Disable the method result streaming,
        /// it is enabled by default.
        public bool EnableMethodResultStreaming
                return this.OperationOptions.EnableMethodResultStreaming;
                DebugHelper.WriteLogEx("EnableMethodResultStreaming {0}", 0, value);
                this.OperationOptions.EnableMethodResultStreaming = value;
        /// Enable/Disable prompt user streaming,
        public bool EnablePromptUser
                DebugHelper.WriteLogEx("EnablePromptUser {0}", 0, value);
                    this.OperationOptions.PromptUser = this.PromptUser;
        /// Enable the pssemantics.
        private void EnablePSSemantics()
            // this.options.PromptUserForceFlag...
            // this.options.WriteErrorMode
            this.OperationOptions.WriteErrorMode = CimCallbackMode.Inquire;
            // !!!NOTES: Does not subscribe to PromptUser for CimCmdlets now
            // since cmdlet does not provider an approach
            // to let user select how to handle prompt message
            // this can be enabled later if needed.
            this.OperationOptions.WriteError = this.WriteError;
            this.OperationOptions.WriteMessage = this.WriteMessage;
            this.OperationOptions.WriteProgress = this.WriteProgress;
        /// Set keyonly property.
        public SwitchParameter KeyOnly
            set { this.OperationOptions.KeysOnly = value.IsPresent; }
        /// Set Shallow flag.
        public SwitchParameter Shallow
                if (value.IsPresent)
                    this.OperationOptions.Flags = CimOperationFlags.PolymorphismShallow;
                    this.OperationOptions.Flags = CimOperationFlags.None;
        /// Initialize the operation option.
        private void InitOption(CimOperationOptions operOptions)
            if (operOptions != null)
                this.OperationOptions = new CimOperationOptions(operOptions);
                this.OperationOptions ??= new CimOperationOptions();
            this.EnableMethodResultStreaming = true;
            this.EnablePSSemantics();
        #region misc operations
        /// Caller call Detach to retrieve the session
        /// object and control the lifecycle of the CimSession object.
        public CimSession Detach()
            // Remove the CimSession from cache but don't dispose it
            RemoveCimSessionFromTemporaryCache(this.CimSession, false);
            CimSession sessionToReturn = this.CimSession;
            this.CimSession = null;
            this.IsTemporaryCimSession = false;
            return sessionToReturn;
        /// Add a new operation to cache.
        /// <param name="operation"></param>
        /// <param name="cancelObject"></param>
        private void AddOperation(IObservable<object> operation)
                Debug.Assert(this.Completed, "Caller should verify that there is no operation in progress");
        /// Remove object from cache.
        private void RemoveOperation(IObservable<object> operation)
                Debug.Assert(this.operation == operation, "Caller should verify that the operation to remove is the operation in progress");
                this.DisposeCancelOperation();
                if (this.operation != null)
                if (this.CimSession != null && this.ContextObject == null)
                    DebugHelper.WriteLog("Dispose this proxy object @ RemoveOperation");
                    this.Dispose();
        /// Trigger an event that new action available
        /// <param name="action"></param>
        protected void FireNewActionEvent(CimBaseAction action)
            CmdletActionEventArgs actionArgs = new(action);
            if (!PreNewActionEvent(actionArgs))
            EventHandler<CmdletActionEventArgs> temp = this.OnNewCmdletAction;
                temp(this.CimSession, actionArgs);
                DebugHelper.WriteLog("Ignore action since OnNewCmdletAction is null.", 5);
            this.PostNewActionEvent(actionArgs);
        /// Trigger an event that new operation is created
        /// <param name="cancelOperation"></param>
        private void FireOperationCreatedEvent(
            IDisposable cancelOperation,
            IObservable<object> operation)
            OperationEventArgs args = new(
                cancelOperation, operation, false);
            this.OnOperationCreated?.Invoke(this.CimSession, args);
            this.PostOperationCreateEvent(args);
        /// Trigger an event that an operation is deleted
        private void FireOperationDeletedEvent(
            bool success)
            this.WriteOperationCompleteMessage(this.operationName);
                null, operation, success);
            PreOperationDeleteEvent(args);
            this.OnOperationDeleted?.Invoke(this.CimSession, args);
            this.PostOperationDeleteEvent(args);
            this.RemoveOperation(operation);
            this.operationName = null;
        #region PSExtension callback functions
        /// WriteMessage callback
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal void WriteMessage(uint channel, string message)
            DebugHelper.WriteLogEx("Channel = {0} message = {1}", 0, channel, message);
                CimWriteMessage action = new(channel, message);
                this.FireNewActionEvent(action);
        /// Write operation start verbose message
        internal void WriteOperationStartMessage(string operation, Hashtable parameterList)
            StringBuilder parameters = new();
            if (parameterList != null)
                foreach (string key in parameterList.Keys)
                    if (parameters.Length > 0)
                        parameters.Append(',');
                    parameters.Append(CultureInfo.CurrentUICulture, $@"'{key}' = {parameterList[key]}");
            string operationStartMessage = string.Format(CultureInfo.CurrentUICulture,
                CimCmdletStrings.CimOperationStart,
                operation,
                (parameters.Length == 0) ? "null" : parameters.ToString());
            WriteMessage((uint)CimWriteMessageChannel.Verbose, operationStartMessage);
        /// Write operation complete verbose message
        internal void WriteOperationCompleteMessage(string operation)
            string operationCompleteMessage = string.Format(CultureInfo.CurrentUICulture,
                CimCmdletStrings.CimOperationCompleted,
                operation);
            WriteMessage((uint)CimWriteMessageChannel.Verbose, operationCompleteMessage);
        /// WriteProgress callback
        /// <param name="activity"></param>
        /// <param name="currentOperation"></param>
        /// <param name="statusDescription"></param>
        /// <param name="percentageCompleted"></param>
        /// <param name="secondsRemaining"></param>
        public void WriteProgress(string activity,
            string currentOperation,
            string statusDescription,
            uint percentageCompleted,
            uint secondsRemaining)
            DebugHelper.WriteLogEx("activity:{0}; currentOperation:{1}; percentageCompleted:{2}; secondsRemaining:{3}",
                0, activity, currentOperation, percentageCompleted, secondsRemaining);
                CimWriteProgress action = new(
                    activity,
                    (int)this.operationID,
                    currentOperation,
                    statusDescription,
                    percentageCompleted,
                    secondsRemaining);
        /// WriteError callback
        /// <param name="instance"></param>
        public CimResponseType WriteError(CimInstance instance)
            DebugHelper.WriteLogEx("Error:{0}", 0, instance);
                CimWriteError action = new(instance, this.invocationContextObject);
                return action.GetResponse();
                return CimResponseType.NoToAll;
        /// PromptUser callback.
        /// <param name="prompt"></param>
        public CimResponseType PromptUser(string message, CimPromptType prompt)
            DebugHelper.WriteLogEx("message:{0} prompt:{1}", 0, message, prompt);
                CimPromptUser action = new(message, prompt);
        #region Async result handler
        /// Handle async event triggered by <see cref="CimResultObserver{T}"/>
        /// <param name="observer">Object triggered the event.</param>
        /// <param name="resultArgs">Async result event argument.</param>
        internal void ResultEventHandler(
            object observer,
            AsyncResultEventArgsBase resultArgs)
            switch (resultArgs.resultType)
                case AsyncResultType.Completion:
                        DebugHelper.WriteLog("ResultEventHandler::Completion", 4);
                        AsyncResultCompleteEventArgs args = resultArgs as AsyncResultCompleteEventArgs;
                        this.FireOperationDeletedEvent(args.observable, true);
                case AsyncResultType.Exception:
                        AsyncResultErrorEventArgs args = resultArgs as AsyncResultErrorEventArgs;
                        DebugHelper.WriteLog("ResultEventHandler::Exception {0}", 4, args.error);
                        using (CimWriteError action = new(args.error, this.invocationContextObject, args.context))
                        this.FireOperationDeletedEvent(args.observable, false);
                case AsyncResultType.Result:
                        AsyncResultObjectEventArgs args = resultArgs as AsyncResultObjectEventArgs;
                        DebugHelper.WriteLog("ResultEventHandler::Result {0}", 4, args.resultObject);
                        object resultObject = args.resultObject;
                        if (!this.isDefaultSession)
                            AddShowComputerNameMarker(resultObject);
                        if (this.ObjectPreProcess != null)
                            resultObject = this.ObjectPreProcess.Process(resultObject);
#if DEBUG
                        resultObject = PostProcessCimInstance(resultObject);
                        CimWriteResultObject action = new(resultObject, this.ContextObject);
        /// This method adds a note property to <paramref name="o"/>,
        /// which will cause the default PowerShell formatting and output
        /// to include PSComputerName column/property in the display.
        /// <param name="o"></param>
        private static void AddShowComputerNameMarker(object o)
            if (o == null)
            PSObject pso = PSObject.AsPSObject(o);
            if (pso.BaseObject is not CimInstance)
            PSNoteProperty psShowComputerNameProperty = new(ConstValue.ShowComputerNameNoteProperty, true);
            pso.Members.Add(psShowComputerNameProperty);
        private static readonly bool isCliXmlTestabilityHookActive = GetIsCliXmlTestabilityHookActive();
        private static bool GetIsCliXmlTestabilityHookActive()
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CDXML_CLIXML_TEST"));
        private static object PostProcessCimInstance(object resultObject)
            if (isCliXmlTestabilityHookActive && (resultObject is CimInstance))
                string serializedForm = PSSerializer.Serialize(resultObject as CimInstance, depth: 1);
                object deserializedObject = PSSerializer.Deserialize(serializedForm);
                object returnObject = (deserializedObject is PSObject) ? (deserializedObject as PSObject).BaseObject : deserializedObject;
                DebugHelper.WriteLogEx("Deserialized object is {0}, type {1}", 1, returnObject, returnObject.GetType());
                return returnObject;
        #region Async operations
        /// create a cim instance asynchronously
        /// <param name="namespaceName"></param>
        public void CreateInstanceAsync(string namespaceName, CimInstance instance)
            Debug.Assert(instance != null, "Caller should verify that instance != NULL.");
            DebugHelper.WriteLogEx("EnableMethodResultStreaming = {0}", 0, this.OperationOptions.EnableMethodResultStreaming);
            this.CheckAvailability();
            this.TargetCimInstance = instance;
            this.operationName = CimCmdletStrings.CimOperationNameCreateInstance;
            this.operationParameters.Clear();
            this.operationParameters.Add(@"namespaceName", namespaceName);
            this.operationParameters.Add(@"instance", instance);
            this.WriteOperationStartMessage(this.operationName, this.operationParameters);
            CimAsyncResult<CimInstance> asyncResult = this.CimSession.CreateInstanceAsync(namespaceName, instance, this.OperationOptions);
            ConsumeCimInstanceAsync(asyncResult, new CimResultContext(instance));
        /// Delete a cim instance asynchronously.
        public void DeleteInstanceAsync(string namespaceName, CimInstance instance)
            DebugHelper.WriteLogEx("namespace = {0}; classname = {1};", 0, namespaceName, instance.CimSystemProperties.ClassName);
            this.operationName = CimCmdletStrings.CimOperationNameDeleteInstance;
            CimAsyncStatus asyncResult = this.CimSession.DeleteInstanceAsync(namespaceName, instance, this.OperationOptions);
            ConsumeObjectAsync(asyncResult, new CimResultContext(instance));
        /// Get cim instance asynchronously.
        public void GetInstanceAsync(string namespaceName, CimInstance instance)
            DebugHelper.WriteLogEx("namespace = {0}; classname = {1}; keyonly = {2}", 0, namespaceName, instance.CimSystemProperties.ClassName, this.OperationOptions.KeysOnly);
            this.operationName = CimCmdletStrings.CimOperationNameGetInstance;
            CimAsyncResult<CimInstance> asyncResult = this.CimSession.GetInstanceAsync(namespaceName, instance, this.OperationOptions);
        /// Modify cim instance asynchronously.
        public void ModifyInstanceAsync(string namespaceName, CimInstance instance)
            DebugHelper.WriteLogEx("namespace = {0}; classname = {1}", 0, namespaceName, instance.CimSystemProperties.ClassName);
            this.operationName = CimCmdletStrings.CimOperationNameModifyInstance;
            CimAsyncResult<CimInstance> asyncResult = this.CimSession.ModifyInstanceAsync(namespaceName, instance, this.OperationOptions);
        /// Enumerate cim instance associated with the
        /// given instance asynchronously.
        /// <param name="sourceInstance"></param>
        /// <param name="associationClassName"></param>
        /// <param name="resultClassName"></param>
        /// <param name="sourceRole"></param>
        /// <param name="resultRole"></param>
        public void EnumerateAssociatedInstancesAsync(
            string namespaceName,
            CimInstance sourceInstance,
            string associationClassName,
            string resultClassName,
            string sourceRole,
            string resultRole)
            Debug.Assert(sourceInstance != null, "Caller should verify that sourceInstance != NULL.");
            DebugHelper.WriteLogEx("Instance class {0}, association class {1}", 0, sourceInstance.CimSystemProperties.ClassName, associationClassName);
            this.TargetCimInstance = sourceInstance;
            this.operationName = CimCmdletStrings.CimOperationNameEnumerateAssociatedInstances;
            this.operationParameters.Add(@"sourceInstance", sourceInstance);
            this.operationParameters.Add(@"associationClassName", associationClassName);
            this.operationParameters.Add(@"resultClassName", resultClassName);
            this.operationParameters.Add(@"sourceRole", sourceRole);
            this.operationParameters.Add(@"resultRole", resultRole);
            CimAsyncMultipleResults<CimInstance> asyncResult = this.CimSession.EnumerateAssociatedInstancesAsync(namespaceName, sourceInstance, associationClassName, resultClassName, sourceRole, resultRole, this.OperationOptions);
            ConsumeCimInstanceAsync(asyncResult, new CimResultContext(sourceInstance));
        /// Enumerate cim instance asynchronously.
        public void EnumerateInstancesAsync(string namespaceName, string className)
            DebugHelper.WriteLogEx("KeyOnly {0}", 0, this.OperationOptions.KeysOnly);
            this.TargetCimInstance = null;
            this.operationName = CimCmdletStrings.CimOperationNameEnumerateInstances;
            this.operationParameters.Add(@"className", className);
            CimAsyncMultipleResults<CimInstance> asyncResult = this.CimSession.EnumerateInstancesAsync(namespaceName, className, this.OperationOptions);
            string errorSource = string.Create(CultureInfo.CurrentUICulture, $"{namespaceName}:{className}");
            ConsumeCimInstanceAsync(asyncResult, new CimResultContext(errorSource));
        /// Enumerate referencing instance associated with
        /// the given instance asynchronously
        public void EnumerateReferencingInstancesAsync(
            string sourceRole)
        /// Query cim instance asynchronously
        public void QueryInstancesAsync(
            string queryExpression)
            DebugHelper.WriteLogEx("KeyOnly = {0}", 0, this.OperationOptions.KeysOnly);
            this.operationName = CimCmdletStrings.CimOperationNameQueryInstances;
            this.operationParameters.Add(@"queryDialect", queryDialect);
            this.operationParameters.Add(@"queryExpression", queryExpression);
            CimAsyncMultipleResults<CimInstance> asyncResult = this.CimSession.QueryInstancesAsync(namespaceName, queryDialect, queryExpression, this.OperationOptions);
            ConsumeCimInstanceAsync(asyncResult, null);
        /// Enumerate cim class asynchronously.
        public void EnumerateClassesAsync(string namespaceName)
            DebugHelper.WriteLogEx("namespace {0}", 0, namespaceName);
            this.operationName = CimCmdletStrings.CimOperationNameEnumerateClasses;
            CimAsyncMultipleResults<CimClass> asyncResult = this.CimSession.EnumerateClassesAsync(namespaceName, null, this.OperationOptions);
            ConsumeCimClassAsync(asyncResult, null);
        public void EnumerateClassesAsync(string namespaceName, string className)
            CimAsyncMultipleResults<CimClass> asyncResult = this.CimSession.EnumerateClassesAsync(namespaceName, className, this.OperationOptions);
            ConsumeCimClassAsync(asyncResult, new CimResultContext(errorSource));
        /// Get cim class asynchronously.
        public void GetClassAsync(string namespaceName, string className)
            DebugHelper.WriteLogEx("namespace = {0}, className = {1}", 0, namespaceName, className);
            this.operationName = CimCmdletStrings.CimOperationNameGetClass;
            CimAsyncResult<CimClass> asyncResult = this.CimSession.GetClassAsync(namespaceName, className, this.OperationOptions);
        /// Invoke method of a given cim instance asynchronously.
        /// <param name="methodParameters"></param>
        public void InvokeMethodAsync(
            CimInstance instance,
            string methodName,
            CimMethodParametersCollection methodParameters)
            this.operationName = CimCmdletStrings.CimOperationNameInvokeMethod;
            this.operationParameters.Add(@"methodName", methodName);
            CimAsyncMultipleResults<CimMethodResultBase> asyncResult = this.CimSession.InvokeMethodAsync(namespaceName, instance, methodName, methodParameters, this.OperationOptions);
            ConsumeCimInvokeMethodResultAsync(asyncResult, instance.CimSystemProperties.ClassName, methodName, new CimResultContext(instance));
        /// Invoke static method of a given class asynchronously.
            CimAsyncMultipleResults<CimMethodResultBase> asyncResult = this.CimSession.InvokeMethodAsync(namespaceName, className, methodName, methodParameters, this.OperationOptions);
            ConsumeCimInvokeMethodResultAsync(asyncResult, className, methodName, new CimResultContext(errorSource));
        /// Subscribe to cim indication asynchronously
        public void SubscribeAsync(
            DebugHelper.WriteLogEx("QueryDialect = '{0}'; queryExpression = '{1}'", 0, queryDialect, queryExpression);
            this.operationName = CimCmdletStrings.CimOperationNameSubscribeIndication;
            this.OperationOptions.Flags |= CimOperationFlags.ReportOperationStarted;
            CimAsyncMultipleResults<CimSubscriptionResult> asyncResult = this.CimSession.SubscribeAsync(namespaceName, queryDialect, queryExpression, this.OperationOptions);
            ConsumeCimSubscriptionResultAsync(asyncResult, null);
        /// Test connection asynchronously
        public void TestConnectionAsync()
            DebugHelper.WriteLogEx("Start test connection", 0);
            CimAsyncResult<CimInstance> asyncResult = this.CimSession.TestConnectionAsync();
            // ignore the test connection result objects
            ConsumeCimInstanceAsync(asyncResult, true, null);
        #region pre action APIs
        /// Called before new action event.
        protected virtual bool PreNewActionEvent(CmdletActionEventArgs args)
        /// Called before operation delete event.
        protected virtual void PreOperationDeleteEvent(OperationEventArgs args)
        #region post action APIs
        /// Called after new action event.
        protected virtual void PostNewActionEvent(CmdletActionEventArgs args)
        /// Called after operation create event.
        protected virtual void PostOperationCreateEvent(OperationEventArgs args)
        /// Called after operation delete event.
        protected virtual void PostOperationDeleteEvent(OperationEventArgs args)
        /// Unique operation ID
        private long operationID;
        /// The CimSession object managed by this proxy object,
        /// which is either created by constructor OR passed in by caller.
        /// The session will be closed while disposing this proxy object
        /// if it is created by constructor.
        internal CimSession CimSession { get; private set; }
        /// The current CimInstance object, against which issued
        /// current operation, it could be null.
        internal CimInstance TargetCimInstance { get; private set; }
        internal bool IsTemporaryCimSession { get; private set; }
        /// The CimOperationOptions object, which specifies the options
        /// of the operation against the session object.
        /// Caller can control the timeout, method streaming support, and
        /// extended ps semantics support, etc.
        /// The setting MUST be set before start new operation on the
        /// this proxy object.
        internal CimOperationOptions OperationOptions { get; private set; }
        /// All operations completed.
        private bool Completed
            get { return this.operation == null; }
        /// Lock object used to lock
        /// operation & cancelOperation members.
        private readonly object stateLock = new();
        /// The operation issued by cimSession.
        private IObservable<object> operation;
        /// The current operation name.
        private string operationName;
        /// The current operation parameters.
        private readonly Hashtable operationParameters = new();
        /// Handler used to cancel operation.
        private IDisposable _cancelOperation;
        /// CancelOperation disposed flag.
        private int _cancelOperationDisposed = 0;
        /// Dispose the cancel operation.
        private void DisposeCancelOperation()
            DebugHelper.WriteLogEx("CancelOperation Disposed = {0}", 0, this._cancelOperationDisposed);
            if (Interlocked.CompareExchange(ref this._cancelOperationDisposed, 1, 0) == 0)
                if (this._cancelOperation != null)
                    DebugHelper.WriteLog("CimSessionProxy::Dispose async operation.", 4);
                    this._cancelOperation.Dispose();
                    this._cancelOperation = null;
        /// Set the cancel operation.
        private IDisposable CancelOperation
                return this._cancelOperation;
                this._cancelOperation = value;
                Interlocked.Exchange(ref this._cancelOperationDisposed, 0);
        /// Current protocol name
        /// DCOM or WSMAN.
        internal ProtocolType Protocol { get; private set; }
        /// Cross operation context object.
        internal XOperationContextBase ContextObject { get; set; }
        /// Invocation context object.
        private InvocationContext invocationContextObject;
        /// A preprocess object to pre-processing the result object,
        /// for example, adding PSTypeName, etc.
        internal IObjectPreProcess ObjectPreProcess { get; set; }
        /// <see cref="isDefaultSession"/> is <see langword="true"/> if this <see cref="CimSessionProxy"/> was
        /// created to handle the "default" session, in cases where cmdlets are invoked without
        /// ComputerName and/or CimSession parameters.
        private readonly bool isDefaultSession;
        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
            DebugHelper.WriteLogEx("Disposed = {0}", 0, this.IsDisposed);
                    if (this.OperationOptions != null)
                        this.OperationOptions.Dispose();
                        this.OperationOptions = null;
                    DisposeTemporaryCimSession();
        public bool IsDisposed
        /// Dispose temporary <see cref="CimSession"/>.
        private void DisposeTemporaryCimSession()
            if (this.IsTemporaryCimSession && this.CimSession != null)
                // remove the cimsession from temporary cache
                RemoveCimSessionFromTemporaryCache(this.CimSession);
        /// Consume the results of async operations
        /// <param name="asyncResult"></param>
        /// <param name="cimResultContext"></param>
        protected void ConsumeCimInstanceAsync(IObservable<CimInstance> asyncResult,
            ConsumeCimInstanceAsync(asyncResult, false, cimResultContext);
        /// Consume the CimInstance results of async operations.
        /// <param name="ignoreResultObjects"></param>
        protected void ConsumeCimInstanceAsync(
            IObservable<CimInstance> asyncResult,
            bool ignoreResultObjects,
            CimResultObserver<CimInstance> observer;
            if (ignoreResultObjects)
                observer = new IgnoreResultObserver(this.CimSession, asyncResult);
                observer = new CimResultObserver<CimInstance>(this.CimSession, asyncResult, cimResultContext);
            observer.OnNewResult += this.ResultEventHandler;
            this.operationID = Interlocked.Increment(ref gOperationCounter);
            this.AddOperation(asyncResult);
            this.CancelOperation = asyncResult.Subscribe(observer);
            this.FireOperationCreatedEvent(this.CancelOperation, asyncResult);
        protected void ConsumeObjectAsync(IObservable<object> asyncResult,
            CimResultObserver<object> observer = new(
                this.CimSession, asyncResult, cimResultContext);
            DebugHelper.WriteLog("FireOperationCreatedEvent");
        /// Consume the <see cref="CimClass"/> of async operations
        protected void ConsumeCimClassAsync(IObservable<CimClass> asyncResult,
            CimResultObserver<CimClass> observer = new(
        /// Consume the <see cref="CimSubscriptionResult"/> of async operations
        protected void ConsumeCimSubscriptionResultAsync(
            IObservable<CimSubscriptionResult> asyncResult,
            CimSubscriptionResultObserver observer = new(
        /// Consume the <see cref="CimMethodResultBase"/> of async operations
        protected void ConsumeCimInvokeMethodResultAsync(
            IObservable<CimMethodResultBase> asyncResult,
            CimMethodResultObserver observer = new(this.CimSession, asyncResult, cimResultContext)
                ClassName = className,
                MethodName = methodName
        /// Check whether current proxy object is available
        private void CheckAvailability()
            AssertSession();
                if (!this.Completed)
                    throw new InvalidOperationException(CimCmdletStrings.OperationInProgress);
            DebugHelper.WriteLog("KeyOnly {0},", 1, this.OperationOptions.KeysOnly);
        /// Check the wrapped <see cref="CimSession"/> object
        private void AssertSession()
            if (this.IsDisposed || (this.CimSession == null))
                DebugHelper.WriteLogEx("Invalid CimSessionProxy object, disposed? {0}; session object {1}", 1, this.IsDisposed, this.CimSession);
                throw new ObjectDisposedException(this.ToString());
        /// Create <see cref="CimSessionOptions"/> based on the given computerName
        private CimSession CreateCimSessionByComputerName(string computerName)
            DebugHelper.WriteLogEx("ComputerName {0}", 0, computerName);
            CimSessionOptions option = CreateCimSessionOption(computerName, 0, null);
            if (option is DComSessionOptions)
                DebugHelper.WriteLog("Create dcom cimSession");
                return CimSession.Create(ConstValue.NullComputerName, option);
                DebugHelper.WriteLog("Create wsman cimSession");
                return CimSession.Create(computerName, option);
        /// Create <see cref="CimSessionOptions"/> based on the given computerName,
        /// timeout and credential
        internal static CimSessionOptions CreateCimSessionOption(string computerName,
            uint timeout, CimCredential credential)
            CimSessionOptions option;
            if (ConstValue.IsDefaultComputerName(computerName))
                DebugHelper.WriteLog("<<<<<<<<<< Use protocol DCOM  {0}", 1, computerName);
                option = new DComSessionOptions();
                DebugHelper.WriteLog("<<<<<<<<<< Use protocol WSMAN {0}", 1, computerName);
                option = new WSManSessionOptions();
            if (timeout != 0)
                option.Timeout = TimeSpan.FromSeconds((double)timeout);
            if (credential != null)
                option.AddDestinationCredentials(credential);
            DebugHelper.WriteLogEx("returned option :{0}.", 1, option);
            return option;
    #region class CimSessionProxyTestConnection
    /// Write session to pipeline after test connection success
    internal class CimSessionProxyTestConnection : CimSessionProxy
        /// Initializes a new instance of the <see cref="CimSessionProxyTestConnection"/> class.
        public CimSessionProxyTestConnection(string computerName, CimSessionOptions sessionOptions)
            : base(computerName, sessionOptions)
        protected override void PreOperationDeleteEvent(OperationEventArgs args)
            DebugHelper.WriteLogEx("test connection result {0}", 0, args.success);
            if (args.success)
                // test connection success, write session object to pipeline
                CimWriteResultObject result = new(this.CimSession, this.ContextObject);
                this.FireNewActionEvent(result);
    #region class CimSessionProxyGetCimClass
    /// Write CimClass to pipeline if the CimClass satisfied
    /// the given conditions
    internal class CimSessionProxyGetCimClass : CimSessionProxy
        /// Initializes a new instance of the <see cref="CimSessionProxyGetCimClass"/> class.
        public CimSessionProxyGetCimClass(string computerName)
            : base(computerName)
        public CimSessionProxyGetCimClass(CimSession session)
            : base(session)
        protected override bool PreNewActionEvent(CmdletActionEventArgs args)
            if (args.Action is not CimWriteResultObject)
                // allow all other actions
            CimWriteResultObject writeResultObject = args.Action as CimWriteResultObject;
            if (writeResultObject.Result is not CimClass cimClass)
            DebugHelper.WriteLog("class name = {0}", 1, cimClass.CimSystemProperties.ClassName);
            CimGetCimClassContext context = this.ContextObject as CimGetCimClassContext;
            Debug.Assert(context != null, "Caller should verify that CimGetCimClassContext != NULL.");
            WildcardPattern pattern;
            if (WildcardPattern.ContainsWildcardCharacters(context.ClassName))
                pattern = new WildcardPattern(context.ClassName, WildcardOptions.IgnoreCase);
                if (!pattern.IsMatch(cimClass.CimSystemProperties.ClassName))
            if (context.PropertyName != null)
                bool match = false;
                if (cimClass.CimClassProperties != null)
                    pattern = new WildcardPattern(context.PropertyName, WildcardOptions.IgnoreCase);
                    foreach (CimPropertyDeclaration decl in cimClass.CimClassProperties)
                        DebugHelper.WriteLog("--- property name : {0}", 1, decl.Name);
                        if (pattern.IsMatch(decl.Name))
                            match = true;
                    DebugHelper.WriteLog("Property name does not match: {0}", 1, context.PropertyName);
                    return match;
            if (context.MethodName != null)
                if (cimClass.CimClassMethods != null)
                    pattern = new WildcardPattern(context.MethodName, WildcardOptions.IgnoreCase);
                    foreach (CimMethodDeclaration decl in cimClass.CimClassMethods)
                        DebugHelper.WriteLog("--- method name : {0}", 1, decl.Name);
                    DebugHelper.WriteLog("Method name does not match: {0}", 1, context.MethodName);
            if (context.QualifierName != null)
                if (cimClass.CimClassQualifiers != null)
                    pattern = new WildcardPattern(context.QualifierName, WildcardOptions.IgnoreCase);
                    foreach (CimQualifier qualifier in cimClass.CimClassQualifiers)
                        DebugHelper.WriteLog("--- qualifier name : {0}", 1, qualifier.Name);
                        if (pattern.IsMatch(qualifier.Name))
                    DebugHelper.WriteLog("Qualifier name does not match: {0}", 1, context.QualifierName);
            DebugHelper.WriteLog("CimClass '{0}' is qualified.", 1, cimClass.CimSystemProperties.ClassName);
    #region class CimSessionProxyNewCimInstance
    /// Get full <see cref="CimInstance"/> if create successfully.
    internal class CimSessionProxyNewCimInstance : CimSessionProxy
        /// Initializes a new instance of the <see cref="CimSessionProxyNewCimInstance"/> class.
        public CimSessionProxyNewCimInstance(string computerName, CimNewCimInstance operation)
            this.NewCimInstanceOperation = operation;
        public CimSessionProxyNewCimInstance(CimSession session, CimNewCimInstance operation)
            if (writeResultObject.Result is not CimInstance cimInstance)
            DebugHelper.WriteLog("Going to read CimInstance classname = {0}; namespace = {1}", 1, cimInstance.CimSystemProperties.ClassName, cimInstance.CimSystemProperties.Namespace);
            this.NewCimInstanceOperation.GetCimInstance(cimInstance, this.ContextObject);
        internal CimNewCimInstance NewCimInstanceOperation { get; }
    /// Support PassThru for set-ciminstance.
    internal class CimSessionProxySetCimInstance : CimSessionProxy
        /// Initializes a new instance of the <see cref="CimSessionProxySetCimInstance"/> class.
        /// Create <see cref="CimSession"/> by given <see cref="CimSessionProxy"/> object.
        /// <param name="originalProxy"><see cref="CimSessionProxy"/> object to clone.</param>
        /// <param name="passThru">PassThru, true means output the modified instance; otherwise does not output.</param>
        public CimSessionProxySetCimInstance(CimSessionProxy originalProxy, bool passThru)
            : base(originalProxy)
            this.passThru = passThru;
        public CimSessionProxySetCimInstance(string computerName,
            : base(computerName, cimInstance)
        public CimSessionProxySetCimInstance(CimSession session, bool passThru)
            if ((!this.passThru) && (args.Action is CimWriteResultObject))
                // filter out any output object
        /// Ture indicates need to output the modified result.
        private readonly bool passThru = false;

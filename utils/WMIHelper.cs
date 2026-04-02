    /// Base class for all WMI helper classes. This is an abstract class
    /// and the helpers need to derive from this.
    internal abstract class AsyncCmdletHelper : IThrottleOperation
        /// Exception raised internally when any method of this class
        /// is executed.
        internal Exception InternalException
                return internalException;
        protected Exception internalException = null;
    /// This class is responsible for creating WMI connection for getting objects and notifications
    /// from WMI asynchronously. This spawns a new thread to connect to WMI on remote machine.
    /// This allows the main thread to return faster and not blocked on network hops.
    internal class WmiAsyncCmdletHelper : AsyncCmdletHelper
        /// Internal Constructor.
        /// <param name="childJob">Job associated with this operation.</param>
        /// <param name="wmiObject">Object associated with this operation.</param>
        /// <param name="computerName">Computer on which the operation is invoked.</param>
        /// <param name="results">Sink to get wmi objects.</param>
        internal WmiAsyncCmdletHelper(PSWmiChildJob childJob, Cmdlet wmiObject, string computerName, ManagementOperationObserver results)
            _wmiObject = wmiObject;
            _computerName = computerName;
            _results = results;
            this.State = WmiState.NotStarted;
            _job = childJob;
        /// Internal Constructor.  This variant takes a count parameter that determines how many times
        /// the WMI command is executed.
        /// <param name="results">Sink to return wmi objects.</param>
        /// <param name="count">Number of times the WMI command is executed.</param>
        internal WmiAsyncCmdletHelper(PSWmiChildJob childJob, Cmdlet wmiObject, string computerName, ManagementOperationObserver results, int count)
            : this(childJob, wmiObject, computerName, results)
            _cmdCount = count;
        private string _computerName;
        internal event EventHandler<WmiJobStateEventArgs> WmiOperationState;
        internal event EventHandler<EventArgs> ShutdownComplete;
        private ManagementOperationObserver _results;
        private int _cmdCount = 1;
        private PSWmiChildJob _job;
        /// Current operation state.
        internal WmiState State
            get { return _state; }
            set { _state = value; }
        private WmiState _state;
        /// Cancel WMI connection.
        internal override void StopOperation()
            _results.Cancel();
            _state = WmiState.Stopped;
            RaiseOperationCompleteEvent(null, OperationState.StopComplete);
        private string GetWmiQueryString()
            GetWmiObjectCommand getObject = (GetWmiObjectCommand)_wmiObject;
            returnValue.Append(string.Join(", ", getObject.Property));
            returnValue.Append(getObject.Class);
            if (!string.IsNullOrEmpty(getObject.Filter))
                returnValue.Append(getObject.Filter);
        /// Do WMI connection by creating another thread based on type of request and return immediately.
        internal override void StartOperation()
            Thread thread;
            if (_wmiObject.GetType() == typeof(GetWmiObjectCommand))
                thread = new Thread(new ThreadStart(ConnectGetWMI));
            else if (_wmiObject.GetType() == typeof(RemoveWmiObject))
                thread = new Thread(new ThreadStart(ConnectRemoveWmi));
            else if (_wmiObject is InvokeWmiMethod)
                thread = new Thread(new ThreadStart(ConnectInvokeWmi));
            else if (_wmiObject is SetWmiInstance)
                thread = new Thread(new ThreadStart(ConnectSetWmi));
                InvalidOperationException exception = new InvalidOperationException("This operation is not supported for this cmdlet.");
                internalException = exception;
                _state = WmiState.Failed;
            thread.IsBackground = true;
        internal override event EventHandler<OperationStateEventArgs> OperationComplete;
        private Cmdlet _wmiObject;
        /// Raise operation completion event.
        internal void RaiseOperationCompleteEvent(EventArgs baseEventArgs, OperationState state)
            OperationStateEventArgs operationStateEventArgs = new OperationStateEventArgs();
            operationStateEventArgs.OperationState = state;
            OperationComplete.SafeInvoke(this, operationStateEventArgs);
        /// Raise WMI state changed event
        internal void RaiseWmiOperationState(EventArgs baseEventArgs, WmiState state)
            WmiJobStateEventArgs wmiJobStateEventArgs = new WmiJobStateEventArgs();
            wmiJobStateEventArgs.WmiState = state;
            WmiOperationState.SafeInvoke(this, wmiJobStateEventArgs);
        /// Do the actual connection to remote machine for Set-WMIInstance cmdlet and raise operation complete event.
        private void ConnectSetWmi()
            SetWmiInstance setObject = (SetWmiInstance)_wmiObject;
            _state = WmiState.Running;
            RaiseWmiOperationState(null, WmiState.Running);
            if (setObject.InputObject != null)
                    // Extra check
                    if (setObject.InputObject.GetType() == typeof(ManagementClass))
                        // Check if Flag specified is CreateOnly or not
                        if (setObject.flagSpecified && setObject.PutType != PutType.CreateOnly)
                            InvalidOperationException e = new InvalidOperationException("CreateOnlyFlagNotSpecifiedWithClassPath");
                            internalException = e;
                        mObj = ((ManagementClass)setObject.InputObject).CreateInstance();
                        setObject.PutType = PutType.CreateOnly;
                        // Check if Flag specified is Updateonly or UpdateOrCreateOnly or not
                        if (setObject.flagSpecified)
                            if (!(setObject.PutType == PutType.UpdateOnly || setObject.PutType == PutType.UpdateOrCreate))
                                InvalidOperationException e = new InvalidOperationException("NonUpdateFlagSpecifiedWithInstancePath");
                            setObject.PutType = PutType.UpdateOrCreate;
                        mObj = (ManagementObject)setObject.InputObject.Clone();
                    if (setObject.Arguments != null)
                        IDictionaryEnumerator en = setObject.Arguments.GetEnumerator();
                        while (en.MoveNext())
                            mObj[en.Key as string] = en.Value;
                    pOptions.Type = setObject.PutType;
                        mObj.Put(_results, pOptions);
                        internalException = exp;
                if (setObject.Class != null)
                        InvalidOperationException exp = new InvalidOperationException("CreateOnlyFlagNotSpecifiedWithClassPath");
                    mPath = new ManagementPath(setObject.Path);
                        mPath.NamespacePath = setObject.Namespace;
                    else if (setObject.namespaceSpecified)
                        InvalidOperationException exp = new InvalidOperationException("NamespaceSpecifiedWithPath");
                    if (mPath.Server != "." && setObject.serverNameSpecified)
                        InvalidOperationException exp = new InvalidOperationException("ComputerNameSpecifiedWithPath");
                                InvalidOperationException exp = new InvalidOperationException("NonUpdateFlagSpecifiedWithInstancePath");
                    if (!(mPath.Server == "." && setObject.serverNameSpecified))
                        _computerName = mPath.Server;
                ConnectionOptions options = setObject.GetConnectionOption();
                    if (setObject.Path != null)
                        mPath.Server = _computerName;
                            mClass.Scope = mScope;
                            mObject = mClass.CreateInstance();
                            // This can throw if path does not exist caller should catch it.
                            mInstance.Scope = mScope;
                                mInstance.Get();
                                if (e.ErrorCode != ManagementStatus.NotFound)
                                int namespaceIndex = setObject.Path.IndexOf(':');
                                if (namespaceIndex == -1)
                                int classIndex = (setObject.Path.Substring(namespaceIndex)).IndexOf('.');
                                if (classIndex == -1)
                                // Get class object and create instance.
                                string newPath = setObject.Path.Substring(0, classIndex + namespaceIndex);
                                ManagementPath classPath = new ManagementPath(newPath);
                                ManagementClass mClass = new ManagementClass(classPath);
                                mInstance = mClass.CreateInstance();
                        ManagementScope scope = new ManagementScope(WMIHelper.GetScopeString(_computerName, setObject.Namespace), options);
                        ManagementClass mClass = new ManagementClass(setObject.Class);
                        mClass.Scope = scope;
                            mObject[en.Key as string] = en.Value;
                        mObject.Put(_results, pOptions);
        /// Do the actual connection to remote machine for Invoke-WMIMethod cmdlet and raise operation complete event.
        private void ConnectInvokeWmi()
            InvokeWmiMethod invokeObject = (InvokeWmiMethod)_wmiObject;
            if (invokeObject.InputObject != null)
                    inputParameters = invokeObject.InputObject.GetMethodParameters(invokeObject.Name);
                    if (invokeObject.ArgumentList != null)
                        int inParamCount = invokeObject.ArgumentList.Length;
                            property.Value = invokeObject.ArgumentList[invokeObject.ArgumentList.Length - inParamCount];
                    invokeObject.InputObject.InvokeMethod(_results, invokeObject.Name, inputParameters, null);
                ConnectionOptions options = invokeObject.GetConnectionOption();
                if (invokeObject.Path != null)
                    mPath = new ManagementPath(invokeObject.Path);
                        mPath.NamespacePath = invokeObject.Namespace;
                    else if (invokeObject.namespaceSpecified)
                        InvalidOperationException e = new InvalidOperationException("NamespaceSpecifiedWithPath");
                    if (mPath.Server != "." && invokeObject.serverNameSpecified)
                        InvalidOperationException e = new InvalidOperationException("ComputerNameSpecifiedWithPath");
                    if (!(mPath.Server == "." && invokeObject.serverNameSpecified))
                bool isLocal = false, needToEnablePrivilege = false;
                PlatformInvokes.TOKEN_PRIVILEGE currentPrivilegeState = new PlatformInvokes.TOKEN_PRIVILEGE();
                    needToEnablePrivilege = NeedToEnablePrivilege(_computerName, invokeObject.Name, ref isLocal);
                    if (needToEnablePrivilege)
                        if (!(isLocal && PlatformInvokes.EnableTokenPrivilege(ComputerWMIHelper.SE_SHUTDOWN_NAME, ref currentPrivilegeState)) &&
                            !(!isLocal && PlatformInvokes.EnableTokenPrivilege(ComputerWMIHelper.SE_REMOTE_SHUTDOWN_NAME, ref currentPrivilegeState)))
                                StringUtil.Format(ComputerResources.PrivilegeNotEnabled, _computerName,
                                isLocal ? ComputerWMIHelper.SE_SHUTDOWN_NAME : ComputerWMIHelper.SE_REMOTE_SHUTDOWN_NAME);
                            InvalidOperationException e = new InvalidOperationException(message);
                        ManagementScope scope = new ManagementScope(WMIHelper.GetScopeString(_computerName, invokeObject.Namespace), options);
                        ManagementClass mClass = new ManagementClass(invokeObject.Class);
                    ManagementBaseObject inputParameters = mObject.GetMethodParameters(invokeObject.Name);
                        ManagementBaseObject result = mObject.InvokeMethod(invokeObject.Name, inputParameters, null);
                        Dbg.Diagnostics.Assert(result != null, "result cannot be null if the Join method is invoked");
                        int returnCode = Convert.ToInt32(result["ReturnValue"], CultureInfo.CurrentCulture);
                            var e = new Win32Exception(returnCode);
                            ShutdownComplete.SafeInvoke(this, null);
                        mObject.InvokeMethod(_results, invokeObject.Name, inputParameters, null);
                            isLocal ? ComputerWMIHelper.SE_SHUTDOWN_NAME : ComputerWMIHelper.SE_REMOTE_SHUTDOWN_NAME, ref currentPrivilegeState);
        /// Check if we need to enable the shutdown privilege.
        /// <param name="isLocal"></param>
        private bool NeedToEnablePrivilege(string computer, string methodName, ref bool isLocal)
            if (methodName.Equals("Win32Shutdown", StringComparison.OrdinalIgnoreCase))
                string localName = System.Net.Dns.GetHostName();
                string localFullName = System.Net.Dns.GetHostEntry(string.Empty).HostName;
                if (computer.Equals(".") || computer.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                    computer.Equals(localName, StringComparison.OrdinalIgnoreCase) ||
                    computer.Equals(localFullName, StringComparison.OrdinalIgnoreCase))
        /// Do the actual connection to remote machine for Remove-WMIObject cmdlet and raise operation complete event.
        private void ConnectRemoveWmi()
            RemoveWmiObject removeObject = (RemoveWmiObject)_wmiObject;
            if (removeObject.InputObject != null)
                    removeObject.InputObject.Delete(_results);
                ConnectionOptions options = removeObject.GetConnectionOption();
                if (removeObject.Path != null)
                    mPath = new ManagementPath(removeObject.Path);
                        mPath.NamespacePath = removeObject.Namespace;
                    else if (removeObject.namespaceSpecified)
                    if (mPath.Server != "." && removeObject.serverNameSpecified)
                    if (!(mPath.Server == "." && removeObject.serverNameSpecified))
                        ManagementScope scope = new ManagementScope(WMIHelper.GetScopeString(_computerName, removeObject.Namespace), options);
                        ManagementClass mClass = new ManagementClass(removeObject.Class);
                    mObject.Delete(_results);
        /// Do the actual connection to remote machine for Get-WMIObject cmdlet and raise operation complete event.
        private void ConnectGetWMI()
            ConnectionOptions options = getObject.GetConnectionOption();
            if (getObject.List.IsPresent)
                if (!getObject.ValidateClassFormat())
                    ArgumentException e = new ArgumentException(
                            "Class", getObject.Class));
                    if (getObject.Recurse.IsPresent)
                        ArrayList namespaceArray = new ArrayList();
                        ArrayList sinkArray = new ArrayList();
                        ArrayList connectArray = new ArrayList(); // Optimization for remote namespace
                        int currentNamespaceCount = 0;
                        namespaceArray.Add(getObject.Namespace);
                        bool topNamespace = true;
                        while (currentNamespaceCount < namespaceArray.Count)
                            string connectNamespace = (string)namespaceArray[currentNamespaceCount];
                            ManagementScope scope = new ManagementScope(WMIHelper.GetScopeString(_computerName, connectNamespace), options);
                                if (!getObject.IsLocalizedNamespace((string)obj["Name"]))
                                    namespaceArray.Add(connectNamespace + "\\" + obj["Name"]);
                            if (topNamespace)
                                topNamespace = false;
                                sinkArray.Add(_results);
                                sinkArray.Add(_job.GetNewSink());
                            connectArray.Add(scope);
                            currentNamespaceCount++;
                        if ((sinkArray.Count != namespaceArray.Count) || (connectArray.Count != namespaceArray.Count)) // not expected throw exception
                            internalException = new InvalidOperationException();
                        currentNamespaceCount = 0;
                            ManagementObjectSearcher searcher = getObject.GetObjectList((ManagementScope)connectArray[currentNamespaceCount]);
                                searcher.Get(_results);
                                searcher.Get((ManagementOperationObserver)sinkArray[currentNamespaceCount]);
                        ManagementScope scope = new ManagementScope(WMIHelper.GetScopeString(_computerName, getObject.Namespace), options);
                        ManagementObjectSearcher searcher = getObject.GetObjectList(scope);
                            throw new ManagementException();
            string queryString = string.IsNullOrEmpty(getObject.Query) ? GetWmiQueryString() : getObject.Query;
                enumOptions.UseAmendedQualifiers = getObject.Amended;
                enumOptions.DirectRead = getObject.DirectRead;
                // Execute the WMI command for each count value.
                for (int i = 0; i < _cmdCount; ++i)
    /// Event which will be triggered when WMI state is changed.
    /// Currently it is to notify Jobs that state has changed to running.
    /// Other states are notified via OperationComplete.
    internal sealed class WmiJobStateEventArgs : EventArgs
        /// WMI state
        internal WmiState WmiState { get; set; }
    /// Enumerated type defining the state of the WMI operation.
    public enum WmiState
        /// The operation has not been started.
        NotStarted = 0,
        /// The operation is executing.
        Running = 1,
        /// The operation is stoping execution.
        Stopping = 2,
        /// The operation is completed due to a stop request.
        Stopped = 3,
        /// The operation has completed.
        Completed = 4,
        /// The operation completed abnormally due to an error.
        Failed = 5,
    internal static class WMIHelper
    /// A class to set WMI connection options.
    public class WmiBaseCmdlet : Cmdlet
        /// Perform Async operation.
        public SwitchParameter AsJob { get; set; } = false;
        /// The Impersonation level to use.
        [Parameter(ParameterSetName = "class")]
        public ImpersonationLevel Impersonation { get; set; } = ImpersonationLevel.Impersonate;
        /// The Authentication level to use.
        public AuthenticationLevel Authentication { get; set; } = AuthenticationLevel.PacketPrivacy;
        /// The Locale to use.
        public string Locale { get; set; } = null;
        /// If all Privileges are enabled.
        public SwitchParameter EnableAllPrivileges { get; set; }
        /// The Authority to use.
        public string Authority { get; set; } = null;
        public Int32 ThrottleLimit { get; set; } = s_DEFAULT_THROTTLE_LIMIT;
            get { return _computerName; }
            set { _computerName = value; serverNameSpecified = true; }
            get { return _nameSpace; }
            set { _nameSpace = value; namespaceSpecified = true; }
        /// The computer to query.
        private string[] _computerName = new string[] { "localhost" };
        /// WMI namespace.
        private string _nameSpace = "root\\cimv2";
        /// Specify if namespace was specified or not.
        internal bool namespaceSpecified = false;
        /// Specify if server name was specified or not.
        internal bool serverNameSpecified = false;
        private static int s_DEFAULT_THROTTLE_LIMIT = 32;    // maximum number of items to be processed at a time
        /// Get connection options.
        internal ConnectionOptions GetConnectionOption()
            ConnectionOptions options;
            options = new ConnectionOptions();
            options.Authentication = this.Authentication;
            options.Locale = this.Locale;
            options.Authority = this.Authority;
            options.EnablePrivileges = this.EnableAllPrivileges;
            options.Impersonation = this.Impersonation;
                if (!(this.Credential.UserName == null && this.Credential.Password == null)) // Empty credential, use implicit credential
                    options.Username = this.Credential.UserName;
                    options.SecurePassword = this.Credential.Password;
        /// Set wmi instance helper.
        internal ManagementObject SetWmiInstanceGetObject(ManagementPath mPath, string serverName)
            var setObject = this as SetWmiInstance;
            if (setObject != null)
                    mPath.Server = serverName;
                    ManagementScope scope = new ManagementScope(WMIHelper.GetScopeString(serverName, setObject.Namespace), options);
        /// Set wmi instance helper for building management path.
        internal ManagementPath SetWmiInstanceBuildManagementPath()
            var wmiInstance = this as SetWmiInstance;
            if (wmiInstance != null)
                if (wmiInstance.Class != null)
                    if (wmiInstance.flagSpecified && wmiInstance.PutType != PutType.CreateOnly)
                        // Throw Terminating error
                         "CreateOnlyFlagNotSpecifiedWithClassPath",
                         wmiInstance.PutType));
                    wmiInstance.PutType = PutType.CreateOnly;
                    mPath = new ManagementPath(wmiInstance.Path);
                        mPath.NamespacePath = wmiInstance.Namespace;
                    else if (wmiInstance.namespaceSpecified)
                            wmiInstance.Namespace));
                    if (mPath.Server != "." && wmiInstance.serverNameSpecified)
                            wmiInstance.ComputerName));
                        if (wmiInstance.flagSpecified)
                            if (!(wmiInstance.PutType == PutType.UpdateOnly || wmiInstance.PutType == PutType.UpdateOrCreate))
                                // Throw terminating error
                                "NonUpdateFlagSpecifiedWithInstancePath",
                            wmiInstance.PutType = PutType.UpdateOrCreate;
            return mPath;
        /// Set wmi instance helper for pipeline input.
        internal ManagementObject SetWmiInstanceGetPipelineObject()
            // Should only be called from Set-WMIInstance cmdlet
                if (wmiInstance.InputObject != null)
                    if (wmiInstance.InputObject.GetType() == typeof(ManagementClass))
                        mObj = ((ManagementClass)wmiInstance.InputObject).CreateInstance();
                        mObj = (ManagementObject)wmiInstance.InputObject.Clone();
                    if (wmiInstance.Arguments != null)
                        IDictionaryEnumerator en = wmiInstance.Arguments.GetEnumerator();
            return mObj;
        /// Start this cmdlet as a WMI job...
        internal void RunAsJob(string cmdletName)
            PSWmiJob wmiJob = new PSWmiJob(this, ComputerName, this.ThrottleLimit, Job.GetCommandTextFromInvocationInfo(this.MyInvocation));
            if (_context != null)
                ((System.Management.Automation.Runspaces.LocalRunspace)_context.CurrentRunspace).JobRepository.Add(wmiJob);
            WriteObject(wmiJob);
        // Get the PowerShell execution context if it's available at cmdlet creation time...
        private System.Management.Automation.ExecutionContext _context = System.Management.Automation.Runspaces.LocalPipeline.GetExecutionContextFromTLS();
    /// A class to perform async operations for WMI cmdlets.
    internal class PSWmiJob : Job
        #region internal constructor
        ///Internal constructor for initializing WMI jobs.
        internal PSWmiJob(Cmdlet cmds, string[] computerName, int throttleLimt, string command)
        : base(command, null)
            PSJobTypeName = WMIJobType;
            _throttleManager.ThrottleLimit = throttleLimt;
            for (int i = 0; i < computerName.Length; i++)
                PSWmiChildJob job = new PSWmiChildJob(cmds, computerName[i], _throttleManager);
                job.StateChanged += new EventHandler<JobStateEventArgs>(HandleChildJobStateChanged);
                job.JobUnblocked += new EventHandler(HandleJobUnblocked);
                ChildJobs.Add(job);
            CommonInit(throttleLimt);
        /// Internal constructor for initializing WMI jobs, where WMI command is executed a variable
        /// number of times.
        internal PSWmiJob(Cmdlet cmds, string[] computerName, int throttleLimit, string command, int count)
            _throttleManager.ThrottleLimit = throttleLimit;
            for (int i = 0; i < computerName.Length; ++i)
                PSWmiChildJob childJob = new PSWmiChildJob(cmds, computerName[i], _throttleManager, count);
                childJob.StateChanged += new EventHandler<JobStateEventArgs>(HandleChildJobStateChanged);
                childJob.JobUnblocked += new EventHandler(HandleJobUnblocked);
                ChildJobs.Add(childJob);
            CommonInit(throttleLimit);
        #endregion internal constructor
        // Set to true when at least one chil job failed
        private bool _atleastOneChildJobFailed = false;
        // Count the number of childs which have finished
        private int _finishedChildJobsCount = 0;
        // Count of number of child jobs which are blocked
        private int _blockedChildJobsCount = 0;
        // WMI Job type name.
        private const string WMIJobType = "WmiJob";
        /// Handles the StateChanged event from each of the child job objects.
        private void HandleChildJobStateChanged(object sender, JobStateEventArgs e)
            if (e.JobStateInfo.State == JobState.Blocked)
                // increment count of blocked child jobs
                lock (_syncObject)
                    _blockedChildJobsCount++;
                // if any of the child job is blocked, we set state to blocked
                SetJobState(JobState.Blocked, null);
            // Ignore state changes which are not resulting in state change to finished.
            if ((!IsFinishedState(e.JobStateInfo.State)) || (e.JobStateInfo.State == JobState.NotStarted))
            if (e.JobStateInfo.State == JobState.Failed)
                // If any of the child job failed, we set status to failed
                _atleastOneChildJobFailed = true;
            bool allChildJobsFinished = false;
                _finishedChildJobsCount++;
                // We are done
                if (_finishedChildJobsCount == ChildJobs.Count)
                    allChildJobsFinished = true;
            if (allChildJobsFinished)
                // if any child job failed, set status to failed
                // If stop was called set, status to stopped
                // else completed
                if (_atleastOneChildJobFailed)
                    SetJobState(JobState.Failed);
                else if (_stopIsCalled == true)
                    SetJobState(JobState.Stopped);
                    SetJobState(JobState.Completed);
        private bool _stopIsCalled = false;
        private string _statusMessage;
        /// Message indicating status of the job.
                return _statusMessage;
        // ISSUE: Implement StatusMessage
        /// Checks the status of remote command execution.
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void SetStatusMessage()
            _statusMessage = "test";
        private bool _moreData = false;
        /// Indicates if more data is available.
        /// This has more data if any of the child jobs have more data.
                // moreData is set to false and will be set to true
                // if at least one child is has more data.
                // if ( (!moreData))
                bool atleastOneChildHasMoreData = false;
                for (int i = 0; i < ChildJobs.Count; i++)
                    if (ChildJobs[i].HasMoreData)
                        atleastOneChildHasMoreData = true;
                _moreData = atleastOneChildHasMoreData;
                return _moreData;
        /// Computers on which this job is running.
                return ConstructLocation();
        private string ConstructLocation()
            StringBuilder location = new StringBuilder();
            foreach (PSWmiChildJob job in ChildJobs)
                location.Append(job.Location);
                location.Append(',');
            location.Remove(location.Length - 1, 1);
            return location.ToString();
        /// Stop Job.
            // AssertNotDisposed();
            if (!IsFinishedState(JobStateInfo.State))
                _stopIsCalled = true;
                _throttleManager.StopAllOperations();
                Finished.WaitOne();
        /// Release all the resources.
        /// if true, release all the managed objects.
                if (!_isDisposed)
                    _isDisposed = true;
                            StopJob();
                        _throttleManager.Dispose();
                        foreach (Job job in ChildJobs)
                            job.Dispose();
        private bool _isDisposed = false;
        /// Initialization common to both constructors.
        private void CommonInit(int throttleLimit)
            // Since no results are produced by any streams. We should
            // close all the streams
            base.CloseAllStreams();
            // set status to "in progress"
            SetJobState(JobState.Running);
            // submit operations to the throttle manager
            _throttleManager.EndSubmitOperations();
        /// Handles JobUnblocked event from a child job and decrements
        /// count of blocked child jobs. When count reaches 0, sets the
        /// state of the parent job to running.
        /// <param name="sender">Sender of this event, unused.</param>
        /// <param name="eventArgs">event arguments, should be empty in this
        /// case</param>
        private void HandleJobUnblocked(object sender, EventArgs eventArgs)
            bool unblockjob = false;
                _blockedChildJobsCount--;
                if (_blockedChildJobsCount == 0)
                    unblockjob = true;
            if (unblockjob)
                SetJobState(JobState.Running, null);
        private ThrottleManager _throttleManager = new ThrottleManager();
        private object _syncObject = new object();           // sync object
    /// Class for WmiChildJob object. This job object Execute wmi cmdlet.
    internal class PSWmiChildJob : Job
        /// Internal constructor for initializing WMI jobs.
        internal PSWmiChildJob(Cmdlet cmds, string computerName, ThrottleManager throttleManager)
        : base(null, null)
            Location = computerName;
            _throttleManager = throttleManager;
            _wmiSinkArray = new ArrayList();
            ManagementOperationObserver wmiSink = new ManagementOperationObserver();
            _wmiSinkArray.Add(wmiSink);
            _sinkCompleted++;
            wmiSink.ObjectReady += new ObjectReadyEventHandler(this.NewObject);
            wmiSink.Completed += new CompletedEventHandler(this.JobDone);
            _helper = new WmiAsyncCmdletHelper(this, cmds, computerName, wmiSink);
            _helper.WmiOperationState += new EventHandler<WmiJobStateEventArgs>(HandleWMIState);
            _helper.ShutdownComplete += new EventHandler<EventArgs>(JobDoneForWin32Shutdown);
            SetJobState(JobState.NotStarted);
            IThrottleOperation operation = _helper;
            operation.OperationComplete += new EventHandler<OperationStateEventArgs>(HandleOperationComplete);
            throttleManager.ThrottleComplete += new EventHandler<EventArgs>(HandleThrottleComplete);
            throttleManager.AddOperation(operation);
        internal PSWmiChildJob(Cmdlet cmds, string computerName, ThrottleManager throttleManager, int count)
            _sinkCompleted += count;
            _helper = new WmiAsyncCmdletHelper(this, cmds, computerName, wmiSink, count);
        private WmiAsyncCmdletHelper _helper;
        // bool _bFinished;
        private ThrottleManager _throttleManager;
        private int _sinkCompleted;
        private bool _bJobFailed;
        private bool _bAtLeastOneObject;
        private ArrayList _wmiSinkArray;
        /// Event raised by this job to indicate to its parent that
        /// its now unblocked by the user.
        internal event EventHandler JobUnblocked;
        /// Set the state of the current job from blocked to
        /// running and raise an event indicating to this
        /// parent job that this job is unblocked.
        internal void UnblockJob()
            JobUnblocked.SafeInvoke(this, EventArgs.Empty);
        internal ManagementOperationObserver GetNewSink()
            return wmiSink;
        /// It receives Management objects.
        private void NewObject(object sender, ObjectReadyEventArgs obj)
            if (!_bAtLeastOneObject)
                _bAtLeastOneObject = true;
            this.WriteObject(obj.NewObject);
        /// It is called when WMI job is done.
        private void JobDone(object sender, CompletedEventArgs obj)
                _sinkCompleted--;
            if (obj.Status != ManagementStatus.NoError)
                _bJobFailed = true;
            if (_sinkCompleted == 0)
                // Notify throttle manager and change the state to complete
                // Two cases where _bFinished should be set to false.
                // 1) Invalid class or some other condition so that after making a connection WMI is throwing an error
                // 2) We could not get any instance for the class.
                /*if(bAtLeastOneObject )
                    _bFinished = true;*/
                _helper.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
                if (!_bJobFailed)
                    _helper.State = WmiState.Completed;
                    _helper.State = WmiState.Failed;
        /// It is called when the call to Win32shutdown is successfully completed.
        private void JobDoneForWin32Shutdown(object sender, EventArgs arg)
        public override string StatusMessage { get; } = "test";
        /// Indicates if there is more data available in
        /// this Job.
        /// Returns the computer on which this command is
        /// running.
        public override string Location { get; }
        /// Stops the job.
            AssertNotDisposed();
            _throttleManager.StopOperation(_helper);
            // if IgnoreStop is set, then StopOperation will
            // return immediately, but StopJob should only
            // return when job is complete. Waiting on the
            // wait handle will ensure that its blocked
            // until the job reaches a terminal state
        private bool _isDisposed;
        /// Handles operation complete event.
        private void HandleOperationComplete(object sender, OperationStateEventArgs stateEventArgs)
            WmiAsyncCmdletHelper helper = (WmiAsyncCmdletHelper)sender;
            if (helper.State == WmiState.NotStarted)
                // This is a case WMI operation was not started.
                SetJobState(JobState.Stopped, helper.InternalException);
            else if (helper.State == WmiState.Running)
                SetJobState(JobState.Running, helper.InternalException);
            else if (helper.State == WmiState.Completed)
                SetJobState(JobState.Completed, helper.InternalException);
            else if (helper.State == WmiState.Failed)
                SetJobState(JobState.Failed, helper.InternalException);
        /// Handles WMI state changed.
        private void HandleWMIState(object sender, WmiJobStateEventArgs stateEventArgs)
            if (stateEventArgs.WmiState == WmiState.Running)
                SetJobState(JobState.Running, _helper.InternalException);
            else if (stateEventArgs.WmiState == WmiState.NotStarted)
                SetJobState(JobState.NotStarted, _helper.InternalException);
            else if (stateEventArgs.WmiState == WmiState.Completed)
            else if (stateEventArgs.WmiState == WmiState.Failed)
                SetJobState(JobState.Failed, _helper.InternalException);
                SetJobState(JobState.Stopped, _helper.InternalException);
        /// Handle a throttle complete event.
        /// <param name="sender">Sender of this event.</param>
        /// <param name="eventArgs">Not used in this method.</param>
        private void HandleThrottleComplete(object sender, EventArgs eventArgs)
            if (_helper.State == WmiState.NotStarted)
            else if (_helper.State == WmiState.Running)
            else if (_helper.State == WmiState.Completed)
                SetJobState(JobState.Completed, _helper.InternalException);
            else if (_helper.State == WmiState.Failed)
            // Do Nothing

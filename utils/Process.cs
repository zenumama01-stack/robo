using System.Diagnostics; // Process class
    #region ProcessBaseCommand
    /// This class implements the base for process commands.
    public abstract class ProcessBaseCommand : Cmdlet
        /// The various process selection modes.
        internal enum MatchMode
            /// Select all processes.
            All,
            /// Select processes matching the supplied names.
            ByName,
            /// Select the processes matching the id.
            ById,
            /// Select the processes specified as input.
            ByInput
        /// The current process selection mode.
        internal MatchMode myMode = MatchMode.All;
        /// The Name parameter is declared in subclasses,
        /// since it is optional for GetProcess and mandatory for StopProcess.
        internal string[] processNames = null;
        // The Id parameter is declared in subclasses,
        // since it is positional for StopProcess but not for GetProcess.
        internal int[] processIds = null;
        /// If the input is a stream of [collections of]
        /// Process objects, we bypass the Name and
        /// Id parameters and read the Process objects
        /// directly.  This allows us to deal with processes which
        /// have wildcard characters in their name.
        /// <value>Process objects</value>
            ParameterSetName = "InputObject",
        public virtual Process[] InputObject
                return _input;
                myMode = MatchMode.ByInput;
                _input = value;
        private Process[] _input = null;
        #region Internal
        // We use a Dictionary to optimize the check whether the object
        // is already in the list.
        private List<Process> _matchingProcesses = new();
        private readonly Dictionary<int, Process> _keys = new();
        /// Retrieve the list of all processes matching the Name, Id
        /// and InputObject parameters, sorted by Id.
        internal List<Process> MatchingProcesses()
            _matchingProcesses.Clear();
            switch (myMode)
                case MatchMode.ById:
                    RetrieveMatchingProcessesById();
                case MatchMode.ByInput:
                    RetrieveProcessesByInput();
                    // Default is "Name":
                    RetrieveMatchingProcessesByProcessName();
            // 2004/12/16 Note that the processes will be sorted
            //  before being stopped.  PM confirms that this is fine.
            _matchingProcesses.Sort(ProcessComparison);
            return _matchingProcesses;
        /// Sort function to sort by Name first, then Id.
        /// <param name="x">First Process object.</param>
        /// <param name="y">Second Process object.</param>
        /// As string.Compare: returns less than zero if x less than y,
        /// greater than 0 if x greater than y, 0 if x == y.
        private static int ProcessComparison(Process x, Process y)
            int diff = string.Compare(
                SafeGetProcessName(x),
                SafeGetProcessName(y),
            if (diff != 0)
            return SafeGetProcessId(x) - SafeGetProcessId(y);
        /// Retrieves the list of all processes matching the Name
        /// parameter.
        /// Generates a non-terminating error for each specified
        /// process name which is not found even though it contains
        /// no wildcards.
        private void RetrieveMatchingProcessesByProcessName()
            if (processNames == null)
                _matchingProcesses = new List<Process>(AllProcesses);
            foreach (string pattern in processNames)
                WildcardPattern wildcard =
                    WildcardPattern.Get(pattern, WildcardOptions.IgnoreCase);
                foreach (Process process in AllProcesses)
                    if (!wildcard.IsMatch(SafeGetProcessName(process)))
                    AddIdempotent(process);
                if (!found &&
                    !WildcardPattern.ContainsWildcardCharacters(pattern))
                    string errorText = ProcessResources.NoProcessFoundForGivenName;
                    string errorName = nameof(ProcessResources.NoProcessFoundForGivenName);
                    if (int.TryParse(pattern, out int x) && x >= 0)
                        errorText = ProcessResources.RecommendIdTagForGivenName;
                        errorName = nameof(ProcessResources.RecommendIdTagForGivenName);
                    WriteNonTerminatingError(
                        processName: pattern,
                        processId: 0,
                        targetObject: pattern,
                        innerException: null,
                        resourceId: errorText,
                        errorId: errorName,
                        category: ErrorCategory.ObjectNotFound);
        /// Retrieves the list of all processes matching the Id
        /// process ID which is not found.
        private void RetrieveMatchingProcessesById()
            if (processIds == null)
                Diagnostics.Assert(false, "null processIds");
                throw PSTraceSource.NewInvalidOperationException();
            foreach (int processId in processIds)
                Process process;
                    process = Process.GetProcessById(processId);
                        processId,
                        ProcessResources.NoProcessFoundForGivenId,
                        "NoProcessFoundForGivenId",
        /// Retrieves the list of all processes matching the InputObject
        private void RetrieveProcessesByInput()
            if (InputObject == null)
                Diagnostics.Assert(false, "null InputObject");
            foreach (Process process in InputObject)
                SafeRefresh(process);
        /// Gets an array of all processes.
        /// <value>An array of <see cref="Process"/> components that represents all the process resources.</value>
        /// <exception cref="System.Security.SecurityException">
        /// MSDN does not document the list of exceptions,
        /// but it is reasonable to expect that SecurityException is
        /// among them.  Errors here will terminate the cmdlet.
        internal Process[] AllProcesses => _allProcesses ??= Process.GetProcesses();
        private Process[] _allProcesses;
        /// Add <paramref name="process"/> to <see cref="_matchingProcesses"/>,
        /// but only if it is not already on  <see cref="_matchingProcesses"/>.
        /// We use a Dictionary to optimize the check whether the object
        /// is already in the list.
        /// <param name="process">Process to add to list.</param>
        private void AddIdempotent(
            Process process)
            int hashCode = SafeGetProcessName(process).GetHashCode()
                ^ SafeGetProcessId(process); // XOR
            if (!_keys.ContainsKey(hashCode))
                _keys.Add(hashCode, process);
                _matchingProcesses.Add(process);
        /// Writes a non-terminating error.
        /// <param name="process"></param>
        /// <param name="innerException"></param>
        /// <param name="resourceId"></param>
        internal void WriteNonTerminatingError(
            Process process,
            string resourceId, string errorId,
                SafeGetProcessName(process),
                SafeGetProcessId(process),
                process,
                resourceId,
                category);
        /// <param name="processName"></param>
        /// <param name="processId"></param>
        /// <param name="targetObject"></param>
            string processName,
            int processId,
            object targetObject,
            string resourceId,
            string message = StringUtil.Format(resourceId,
                processName,
                (innerException == null) ? string.Empty : innerException.Message);
            ProcessCommandException exception =
                new(message, innerException);
            exception.ProcessName = processName;
                exception, errorId, category, targetObject));
        // The Name property is not always available, even for
        // live processes (such as the Idle process).
        internal static string SafeGetProcessName(Process process)
                return process.ProcessName;
        // 2004/12/17-JonN I saw this fail once too, so we'll play it safe
        internal static int SafeGetProcessId(Process process)
                return process.Id;
                return int.MinValue;
        internal static void SafeRefresh(Process process)
                process.Refresh();
        /// TryHasExited is a helper function used to detect if the process has aready exited or not.
        /// <param name="process">
        /// Process whose exit status has to be checked.
        /// <returns>Tre if the process has exited or else returns false.</returns>
        internal static bool TryHasExited(Process process)
            bool hasExited = true;
                hasExited = process.HasExited;
                hasExited = false;
            return hasExited;
        #endregion Internal
    #endregion ProcessBaseCommand
    #region GetProcessCommand
    /// This class implements the get-process command.
    [Cmdlet(VerbsCommon.Get, "Process", DefaultParameterSetName = NameParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096814", RemotingCapability = RemotingCapability.SupportedByCommand)]
    [OutputType(typeof(ProcessModule), typeof(FileVersionInfo), typeof(Process))]
    public sealed class GetProcessCommand : ProcessBaseCommand
        #region ParameterSetStrings
        private const string IdParameterSet = "Id";
        private const string InputObjectParameterSet = "InputObject";
        private const string NameWithUserNameParameterSet = "NameWithUserName";
        private const string IdWithUserNameParameterSet = "IdWithUserName";
        private const string InputObjectWithUserNameParameterSet = "InputObjectWithUserName";
        #endregion ParameterSetStrings
        /// Has the list of process names on which to this command will work.
        [Parameter(Position = 0, ParameterSetName = NameWithUserNameParameterSet, ValueFromPipelineByPropertyName = true)]
        [Alias("ProcessName")]
                return processNames;
                myMode = MatchMode.ByName;
                processNames = value;
        /// Gets/sets an array of process IDs.
        [Parameter(ParameterSetName = IdParameterSet, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = IdWithUserNameParameterSet, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [Alias("PID")]
        public int[] Id
                return processIds;
                myMode = MatchMode.ById;
                processIds = value;
        /// Input is a stream of [collections of] Process objects.
        [Parameter(ParameterSetName = InputObjectParameterSet, Mandatory = true, ValueFromPipeline = true)]
        [Parameter(ParameterSetName = InputObjectWithUserNameParameterSet, Mandatory = true, ValueFromPipeline = true)]
        public override Process[] InputObject
                return base.InputObject;
                base.InputObject = value;
        /// Include the UserName.
        [Parameter(ParameterSetName = NameWithUserNameParameterSet, Mandatory = true)]
        [Parameter(ParameterSetName = IdWithUserNameParameterSet, Mandatory = true)]
        [Parameter(ParameterSetName = InputObjectWithUserNameParameterSet, Mandatory = true)]
        public SwitchParameter IncludeUserName { get; set; }
        /// To display the modules of a process.
        [Parameter(ParameterSetName = NameParameterSet)]
        [Parameter(ParameterSetName = IdParameterSet)]
        [Parameter(ParameterSetName = InputObjectParameterSet)]
        public SwitchParameter Module { get; set; }
        /// To display the fileversioninfo of the main module of a process.
        [Alias("FV", "FVI")]
        public SwitchParameter FileVersionInfo { get; set; }
        /// Write the process objects.
            foreach (Process process in MatchingProcesses())
                if (Module.IsPresent && FileVersionInfo.IsPresent)
                    ProcessModule tempmodule = null;
                        ProcessModuleCollection modules = process.Modules;
                        foreach (ProcessModule pmodule in modules)
                            // Assigning to tempmodule to rethrow for exceptions on 64 bit machines
                            tempmodule = pmodule;
                            WriteObject(pmodule.FileVersionInfo, true);
                    catch (InvalidOperationException exception)
                        WriteNonTerminatingError(process, exception, ProcessResources.CouldNotEnumerateModuleFileVer, "CouldNotEnumerateModuleFileVer", ErrorCategory.PermissionDenied);
                    catch (ArgumentException exception)
                    catch (Win32Exception exception)
                            if (exception.HResult == 299)
                                WriteObject(tempmodule.FileVersionInfo, true);
                            WriteNonTerminatingError(process, ex, ProcessResources.CouldNotEnumerateModuleFileVer, "CouldNotEnumerateModuleFileVer", ErrorCategory.PermissionDenied);
                    catch (Exception exception)
                else if (Module.IsPresent)
                        WriteObject(process.Modules, true);
                                WriteNonTerminatingError(process, exception, ProcessResources.CouldNotEnumerateModules, "CouldNotEnumerateModules", ErrorCategory.PermissionDenied);
                            WriteNonTerminatingError(process, ex, ProcessResources.CouldNotEnumerateModules, "CouldNotEnumerateModules", ErrorCategory.PermissionDenied);
                else if (FileVersionInfo.IsPresent)
                        ProcessModule mainModule = process.MainModule;
                        if (mainModule != null)
                            WriteObject(mainModule.FileVersionInfo, true);
                        WriteNonTerminatingError(process, exception, ProcessResources.CouldNotEnumerateFileVer, "CouldNotEnumerateFileVer", ErrorCategory.PermissionDenied);
                                WriteObject(process.MainModule?.FileVersionInfo, true);
                            WriteNonTerminatingError(process, ex, ProcessResources.CouldNotEnumerateFileVer, "CouldNotEnumerateFileVer", ErrorCategory.PermissionDenied);
                    WriteObject(IncludeUserName.IsPresent ? AddUserNameToProcess(process) : process);
        #region Privates
        /// New PSTypeName added to the process object.
        private const string TypeNameForProcessWithUserName = "System.Diagnostics.Process#IncludeUserName";
        /// Add the 'UserName' NoteProperty to the Process object.
        private static PSObject AddUserNameToProcess(Process process)
            // Return null if we failed to get the owner information
            string userName = RetrieveProcessUserName(process);
            PSObject processAsPsobj = PSObject.AsPSObject(process);
            PSNoteProperty noteProperty = new("UserName", userName);
            processAsPsobj.Properties.Add(noteProperty, true);
            processAsPsobj.TypeNames.Insert(0, TypeNameForProcessWithUserName);
            return processAsPsobj;
        /// Retrieve the UserName through PInvoke.
        private static string RetrieveProcessUserName(Process process)
            string userName = null;
            userName = Platform.NonWindowsGetUserFromPid(process.Id);
            IntPtr tokenUserInfo = IntPtr.Zero;
            IntPtr processTokenHandler = IntPtr.Zero;
            const uint TOKEN_QUERY = 0x0008;
                int error;
                if (!Win32Native.OpenProcessToken(process.Handle, TOKEN_QUERY, out processTokenHandler))
                // Set the default length to be 256, so it will be sufficient for most cases.
                int tokenInfoLength = 256;
                tokenUserInfo = Marshal.AllocHGlobal(tokenInfoLength);
                if (!Win32Native.GetTokenInformation(processTokenHandler, Win32Native.TOKEN_INFORMATION_CLASS.TokenUser, tokenUserInfo, tokenInfoLength, out tokenInfoLength))
                    error = Marshal.GetLastWin32Error();
                    if (error == Win32Native.ERROR_INSUFFICIENT_BUFFER)
                        Marshal.FreeHGlobal(tokenUserInfo);
                var tokenUser = Marshal.PtrToStructure<Win32Native.TOKEN_USER>(tokenUserInfo);
                SecurityIdentifier sid = new SecurityIdentifier(tokenUser.User.Sid);
                userName = sid.Translate(typeof(System.Security.Principal.NTAccount)).Value;
                // The Process not started yet, or it's a process from a remote machine.
                // SID cannot be mapped to a user
                // The Process has exited, Process.Handle will raise this exception.
                // We might get an AccessDenied error.
                // I don't expect to get other exceptions.
                if (tokenUserInfo != IntPtr.Zero)
                if (processTokenHandler != IntPtr.Zero)
                    Win32Native.CloseHandle(processTokenHandler);
            return userName;
        #endregion Privates
    #endregion GetProcessCommand
    #region WaitProcessCommand
    /// This class implements the Wait-process command.
    [Cmdlet(VerbsLifecycle.Wait, "Process", DefaultParameterSetName = "Name", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097146")]
    [OutputType(typeof(Process))]
    public sealed class WaitProcessCommand : ProcessBaseCommand
        /// Specifies the process IDs of the processes to be waited on.
            ParameterSetName = "Id",
        [Alias("PID", "ProcessId")]
        /// Name of the processes to wait on for termination.
            ParameterSetName = "Name",
        /// If specified, wait for this number of seconds.
        [ValidateRange(0, 32767)]
                _timeOutSpecified = true;
        /// Gets or sets a value indicating whether to return after any one process exits.
        public SwitchParameter Any { get; set; }
        /// Gets or sets a value indicating whether to return the Process objects after waiting.
        private int _timeout = 0;
        private bool _timeOutSpecified;
        private bool _disposed = false;
        /// Dispose method of IDisposable interface.
                if (_waitHandle != null)
                    _waitHandle.Dispose();
                    _waitHandle = null;
        // Handle Exited event and display process information.
        private void myProcess_Exited(object sender, System.EventArgs e)
            if (Any || (Interlocked.Decrement(ref _numberOfProcessesToWaitFor) == 0))
                _waitHandle?.Set();
        private readonly List<Process> _processList = new();
        // Wait handle which is used by thread to sleep.
        private ManualResetEvent _waitHandle;
        private int _numberOfProcessesToWaitFor;
        /// Gets the list of process.
            // adding the processes into the list
                // Idle process has processid zero,so handle that because we cannot wait on it.
                if (process.Id == 0)
                    WriteNonTerminatingError(process, null, ProcessResources.WaitOnIdleProcess, "WaitOnIdleProcess", ErrorCategory.ObjectNotFound);
                // It cannot wait on itself
                if (process.Id.Equals(Environment.ProcessId))
                    WriteNonTerminatingError(process, null, ProcessResources.WaitOnItself, "WaitOnItself", ErrorCategory.ObjectNotFound);
                _processList.Add(process);
        /// Wait for the process to terminate.
            _waitHandle = new ManualResetEvent(false);
            foreach (Process process in _processList)
                    // Check for processes that exit too soon for us to add an event.
                    if (Any && process.HasExited)
                        _waitHandle.Set();
                    else if (!process.HasExited)
                        process.EnableRaisingEvents = true;
                        process.Exited += myProcess_Exited;
                        if (!process.HasExited)
                            System.Threading.Interlocked.Increment(ref _numberOfProcessesToWaitFor);
                    WriteNonTerminatingError(process, exception, ProcessResources.ProcessIsNotTerminated, "ProcessNotTerminated", ErrorCategory.CloseError);
            bool hasTimedOut = false;
            if (_numberOfProcessesToWaitFor > 0)
                if (_timeOutSpecified)
                    hasTimedOut = !_waitHandle.WaitOne(_timeout * 1000);
                    _waitHandle.WaitOne();
            if (hasTimedOut || (!Any && _numberOfProcessesToWaitFor > 0))
                            string message = StringUtil.Format(ProcessResources.ProcessNotTerminated, new object[] { process.ProcessName, process.Id });
                            ErrorRecord errorRecord = new(new TimeoutException(message), "ProcessNotTerminated", ErrorCategory.CloseError, process);
                WriteObject(_processList, enumerateCollection: true);
        /// StopProcessing.
        protected override void StopProcessing() => _waitHandle?.Set();
    #endregion WaitProcessCommand
    #region StopProcessCommand
    /// This class implements the stop-process command.
    /// Processes will be sorted before being stopped.  PM confirms
    /// that this should be fine.
    [Cmdlet(VerbsLifecycle.Stop, "Process",
        DefaultParameterSetName = "Id",
        SupportsShouldProcess = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097058")]
    public sealed class StopProcessCommand : ProcessBaseCommand
        /// Gets/sets an array of objects.
        public new Process[] InputObject
        private bool _passThru;
        /// The updated process object should be passed down the pipeline.
            get { return _passThru; }
            set { _passThru = value; }
        /// Specifies whether to force a process to kill
        /// even if it has dependent services.
        /// Kill the processes.
        /// It is a non-terminating error if the Process.Kill() operation fails.
            if (myMode == MatchMode.All || (myMode == MatchMode.ByName && processNames == null))
                Diagnostics.Assert(false, "trying to kill all processes");
                // confirm the operation first
                // this is always false if WhatIf is set
                // 2005-06-21 Moved this ahead of the hasExited check
                string targetString = StringUtil.Format(
                            ProcessResources.ProcessNameForConfirmation,
                            SafeGetProcessId(process));
                if (!ShouldProcess(targetString))
                    // Many properties including Name are not available if the process has exited.
                    // If this is the case, we skip the process. If the process is from a remote
                    // machine, then we generate a non-terminating error because .NET doesn't support
                    // terminate a remote process.
                    if (process.HasExited)
                            WriteObject(process);
                catch (NotSupportedException ex)
                        process, ex, ProcessResources.CouldNotStopProcess,
                        "CouldNotStopProcess", ErrorCategory.InvalidOperation);
                        "CouldNotStopProcess", ErrorCategory.CloseError);
                    if (Environment.ProcessId == SafeGetProcessId(process))
                        _shouldKillCurrentProcess = true;
                    if (Platform.IsWindows && !Force)
                        if (!IsProcessOwnedByCurrentUser(process))
                            string message = StringUtil.Format(
                                        ProcessResources.ConfirmStopProcess,
                            // caption: null = default caption
                            if (!ShouldContinue(message, null, ref _yesToAll, ref _noToAll))
                    // If the process is svchost stop all the dependent services before killing process
                    if (string.Equals(SafeGetProcessName(process), "SVCHOST", StringComparison.OrdinalIgnoreCase))
                        StopDependentService(process);
                        process.Kill();
                    if (!TryHasExited(process))
                            process, exception, ProcessResources.CouldNotStopProcess,
        /// Kill the current process here.
            if (_shouldKillCurrentProcess)
                StopProcess(Process.GetCurrentProcess());
        /// Should the current powershell process to be killed.
        private bool _shouldKillCurrentProcess;
        /// Boolean variables to display the warning using ShouldContinue.
        private bool _yesToAll, _noToAll;
        /// Current windows user name.
        private string _currentUserName;
        /// Gets the owner of the process.
        /// <returns>Returns the owner.</returns>
        private bool IsProcessOwnedByCurrentUser(Process process)
            IntPtr ph = IntPtr.Zero;
                if (Win32Native.OpenProcessToken(process.Handle, TOKEN_QUERY, out ph))
                    if (_currentUserName == null)
                        using (var currentUser = WindowsIdentity.GetCurrent())
                            _currentUserName = currentUser.Name;
                    using (var processUser = new WindowsIdentity(ph))
                        return string.Equals(processUser.Name, _currentUserName, StringComparison.OrdinalIgnoreCase);
                // Catching IdentityMappedException
                // Need not throw error.
                // Catching ArgumentException. In Win2k3 Token is zero
                if (ph != IntPtr.Zero)
                    Win32Native.CloseHandle(ph);
        /// Stop the service that depends on the process and its child services.
        private void StopDependentService(Process process)
            string queryString = "Select * From Win32_Service Where ProcessId=" + SafeGetProcessId(process) + " And State !='Stopped'";
                using (CimSession cimSession = CimSession.Create(null))
                    IEnumerable<CimInstance> serviceList =
                        cimSession.QueryInstances("root/cimv2", "WQL", queryString);
                    foreach (CimInstance oService in serviceList)
                        string serviceName = oService.CimInstanceProperties["Name"].Value.ToString();
                        using (var service = new System.ServiceProcess.ServiceController(serviceName))
                                service.Stop();
                                // Wait 2 sec for the status to become 'Stopped'
                                service.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 2));
                            catch (Win32Exception) { }
                            catch (InvalidOperationException) { }
                            catch (System.ServiceProcess.TimeoutException) { }
                var errorRecord = new ErrorRecord(ex, "GetCimException", ErrorCategory.InvalidOperation, null);
        /// Stops the given process throws non terminating error if can't.
        /// <param name="process">Process to be stopped.</param>
        /// <returns>True if process stopped successfully else false.</returns>
        private void StopProcess(Process process)
                    // This process could not be stopped,
                    // so write a non-terminating error.
    #endregion StopProcessCommand
    #region DebugProcessCommand
    /// This class implements the Debug-process command.
    [Cmdlet(VerbsDiagnostic.Debug, "Process", DefaultParameterSetName = "Name", SupportsShouldProcess = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096809")]
    public sealed class DebugProcessCommand : ProcessBaseCommand
        /// Gets the list of process and attach the debugger to the processes.
                string targetMessage = StringUtil.Format(
                if (!ShouldProcess(targetMessage))
                // Sometimes Idle process has processid zero,so handle that because we cannot attach debugger to it.
                        process, null, ProcessResources.NoDebuggerFound,
                        "NoDebuggerFound", ErrorCategory.ObjectNotFound);
                    // If the process has exited, we skip it. If the process is from a remote
                    // machine, then we generate a non-terminating error.
                        process, ex, ProcessResources.CouldNotDebugProcess,
                        "CouldNotDebugProcess", ErrorCategory.InvalidOperation);
                    // This process could not be stopped, so write a non-terminating error.
                        "CouldNotDebugProcess", ErrorCategory.CloseError);
                AttachDebuggerToProcess(process);
        /// Attach debugger to the process.
        private void AttachDebuggerToProcess(Process process)
            string searchQuery = "Select * From Win32_Process Where ProcessId=" + SafeGetProcessId(process);
                IEnumerable<CimInstance> processCollection =
                    cimSession.QueryInstances("root/cimv2", "WQL", searchQuery);
                foreach (CimInstance processInstance in processCollection)
                        // Call the AttachDebugger method
                        CimMethodResult result = cimSession.InvokeMethod(processInstance, "AttachDebugger", null);
                        int returnCode = Convert.ToInt32(result.ReturnValue.Value, System.Globalization.CultureInfo.CurrentCulture);
                        if (returnCode != 0)
                            var ex = new InvalidOperationException(MapReturnCodeToErrorMessage(returnCode));
                        string message = e.Message?.Trim();
                        var errorRecord = new ErrorRecord(
                                new InvalidOperationException(StringUtil.Format(ProcessResources.DebuggerError, message)),
                                "GetCimException", ErrorCategory.InvalidOperation, null);
        /// Map the return code from 'AttachDebugger' to error message.
        private static string MapReturnCodeToErrorMessage(int returnCode)
            string errorMessage = returnCode switch
                2 => ProcessResources.AttachDebuggerReturnCode2,
                3 => ProcessResources.AttachDebuggerReturnCode3,
                8 => ProcessResources.AttachDebuggerReturnCode8,
                9 => ProcessResources.AttachDebuggerReturnCode9,
                21 => ProcessResources.AttachDebuggerReturnCode21,
                _ => string.Empty
            Diagnostics.Assert(!string.IsNullOrEmpty(errorMessage), "Error message should not be null or empty.");
            return errorMessage;
    #endregion DebugProcessCommand
    #region StartProcessCommand
    /// This class implements the Start-process command.
    [Cmdlet(VerbsLifecycle.Start, "Process", DefaultParameterSetName = "Default", SupportsShouldProcess = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097141")]
    public sealed class StartProcessCommand : PSCmdlet, IDisposable
        private bool _isDefaultSetParameterSpecified = false;
        /// Path/FileName of the process to start.
        [Parameter(Mandatory = true, Position = 0)]
        [Alias("PSPath", "Path")]
        public string FilePath { get; set; }
        /// Arguments for the process.
        public string[] ArgumentList { get; set; }
        /// Credentials for the process.
        [Parameter(ParameterSetName = "Default")]
        [Alias("RunAs")]
        public PSCredential Credential
                return _credential;
                _credential = value;
                _isDefaultSetParameterSpecified = true;
        private PSCredential _credential;
        /// Working directory of the process.
        public string WorkingDirectory { get; set; }
        /// Load user profile from registry.
        [Alias("Lup")]
        public SwitchParameter LoadUserProfile
                return _loaduserprofile;
                _loaduserprofile = value;
        private SwitchParameter _loaduserprofile = SwitchParameter.Present;
        /// Starts process in the current console window.
        [Alias("nnw")]
        public SwitchParameter NoNewWindow
                return _nonewwindow;
                _nonewwindow = value;
        private SwitchParameter _nonewwindow;
        /// PassThru parameter.
        /// Redirect error.
        [Alias("RSE")]
        public string RedirectStandardError
                return _redirectstandarderror;
                _redirectstandarderror = value;
        private string _redirectstandarderror;
        /// Redirect input.
        [Alias("RSI")]
        public string RedirectStandardInput
                return _redirectstandardinput;
                _redirectstandardinput = value;
        private string _redirectstandardinput;
        /// Redirect output.
        [Alias("RSO")]
        public string RedirectStandardOutput
                return _redirectstandardoutput;
                _redirectstandardoutput = value;
        private string _redirectstandardoutput;
        /// Verb.
        /// The 'Verb' parameter is only supported on Windows Desktop.
        [Parameter(ParameterSetName = "UseShellExecute")]
        [ArgumentCompleter(typeof(VerbArgumentCompleter))]
        public string Verb { get; set; }
        /// Window style of the process window.
        public ProcessWindowStyle WindowStyle
                return _windowstyle;
                _windowstyle = value;
                _windowstyleSpecified = true;
        private ProcessWindowStyle _windowstyle = ProcessWindowStyle.Normal;
        private bool _windowstyleSpecified = false;
        /// Default Environment.
        public SwitchParameter UseNewEnvironment
                return _UseNewEnvironment;
                _UseNewEnvironment = value;
        private SwitchParameter _UseNewEnvironment;
        /// Gets or sets the environment variables for the process.
        public Hashtable Environment
                return _environment;
                _environment = value;
        private Hashtable _environment;
        #region overrides
            // -Verb and -WindowStyle are not supported on non-Windows platforms as well as Windows headless SKUs
            if (Platform.IsWindowsDesktop)
                // Parameters '-NoNewWindow' and '-WindowStyle' are both valid on full windows SKUs.
                if (_nonewwindow && _windowstyleSpecified)
                    message = StringUtil.Format(ProcessResources.ContradictParametersSpecified, "-NoNewWindow", "-WindowStyle");
                    ErrorRecord er = new(new InvalidOperationException(message), "InvalidOperationException", ErrorCategory.InvalidOperation, null);
                if (this.ParameterSetName.Equals("UseShellExecute"))
                    message = StringUtil.Format(ProcessResources.ParameterNotSupportedOnPSEdition, "-Verb", "Start-Process");
                else if (_windowstyleSpecified)
                    message = StringUtil.Format(ProcessResources.ParameterNotSupportedOnPSEdition, "-WindowStyle", "Start-Process");
                if (!string.IsNullOrEmpty(message))
                    ErrorRecord er = new(new NotSupportedException(message), "NotSupportedException", ErrorCategory.NotImplemented, null);
            // Use ShellExecute by default if we are running on full windows SKUs
            startInfo.UseShellExecute = Platform.IsWindowsDesktop;
            // Path = Mandatory parameter -> Will not be empty.
                    FilePath, CommandTypes.Application | CommandTypes.ExternalScript,
                startInfo.FileName = cmdinfo.Definition;
            catch (CommandNotFoundException)
                // codeql[cs/microsoft/command-line-injection-shell-execution] - This is expected Poweshell behavior where user inputted paths are supported for the context of this method. The user assumes trust for the file path they are specifying and the process is on the user's system except for remoting in which case restricted remoting security guidelines should be used.
                startInfo.FileName = FilePath;
                // Arguments are passed incorrectly to the executable used for ShellExecute and not to filename https://github.com/dotnet/corefx/issues/30718
                // so don't use ShellExecute if arguments are specified
                // Linux relies on `xdg-open` and macOS relies on `open` which behave differently than Windows ShellExecute when running console commands
                // as a new console will be opened.  So to avoid that, we only use ShellExecute on non-Windows if the filename is not an actual command (like a URI)
                startInfo.UseShellExecute = (ArgumentList == null);
            if (ArgumentList != null)
                startInfo.Arguments = string.Join(' ', ArgumentList);
            if (WorkingDirectory != null)
                // WorkingDirectory -> Not Exist -> Throw Error
                WorkingDirectory = ResolveFilePath(WorkingDirectory);
                if (!Directory.Exists(WorkingDirectory))
                    message = StringUtil.Format(ProcessResources.InvalidInput, "WorkingDirectory");
                    ErrorRecord er = new(new DirectoryNotFoundException(message), "DirectoryNotFoundException", ErrorCategory.InvalidOperation, null);
                startInfo.WorkingDirectory = WorkingDirectory;
                // Working Directory not specified -> Assign Current Path, but only if it still exists
                var currentDirectory = PathUtils.ResolveFilePath(this.SessionState.Path.CurrentFileSystemLocation.Path, this, isLiteralPath: true);
                if (Directory.Exists(currentDirectory))
                    startInfo.WorkingDirectory = currentDirectory;
            if (this.ParameterSetName.Equals("Default"))
                if (_isDefaultSetParameterSpecified)
                if (_UseNewEnvironment)
                    startInfo.EnvironmentVariables.Clear();
                    LoadEnvironmentVariable(startInfo, System.Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine));
                    LoadEnvironmentVariable(startInfo, System.Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User));
                if (_environment != null)
                    LoadEnvironmentVariable(startInfo, _environment);
                startInfo.WindowStyle = _windowstyle;
                // When starting a process as another user, the 'CreateNoWindow' property value is ignored and a new window is created.
                // See details at https://learn.microsoft.com/dotnet/api/system.diagnostics.processstartinfo.createnowindow?view=net-9.0#remarks
                if (_nonewwindow && _credential is null)
                    startInfo.CreateNoWindow = _nonewwindow;
                startInfo.LoadUserProfile = _loaduserprofile;
                if (_credential != null)
                    NetworkCredential nwcredential = _credential.GetNetworkCredential();
                    startInfo.UserName = nwcredential.UserName;
                    if (string.IsNullOrEmpty(nwcredential.Domain))
                        startInfo.Domain = ".";
                        startInfo.Domain = nwcredential.Domain;
                    startInfo.Password = _credential.Password;
                // RedirectionInput File Check -> Not Exist -> Throw Error
                if (_redirectstandardinput != null)
                    _redirectstandardinput = ResolveFilePath(_redirectstandardinput);
                    if (!File.Exists(_redirectstandardinput))
                        message = StringUtil.Format(ProcessResources.InvalidInput, "RedirectStandardInput '" + this.RedirectStandardInput + "'");
                        ErrorRecord er = new(new FileNotFoundException(message), "FileNotFoundException", ErrorCategory.InvalidOperation, null);
                // RedirectionInput == RedirectionOutput -> Throw Error
                if (_redirectstandardinput != null && _redirectstandardoutput != null)
                    _redirectstandardoutput = ResolveFilePath(_redirectstandardoutput);
                    if (_redirectstandardinput.Equals(_redirectstandardoutput, StringComparison.OrdinalIgnoreCase))
                        message = StringUtil.Format(ProcessResources.DuplicateEntry, "RedirectStandardInput", "RedirectStandardOutput");
                // RedirectionInput == RedirectionError -> Throw Error
                if (_redirectstandardinput != null && _redirectstandarderror != null)
                    _redirectstandarderror = ResolveFilePath(_redirectstandarderror);
                    if (_redirectstandardinput.Equals(_redirectstandarderror, StringComparison.OrdinalIgnoreCase))
                        message = StringUtil.Format(ProcessResources.DuplicateEntry, "RedirectStandardInput", "RedirectStandardError");
                // RedirectionOutput == RedirectionError -> Throw Error
                if (_redirectstandardoutput != null && _redirectstandarderror != null)
                    if (_redirectstandardoutput.Equals(_redirectstandarderror, StringComparison.OrdinalIgnoreCase))
                        message = StringUtil.Format(ProcessResources.DuplicateEntry, "RedirectStandardOutput", "RedirectStandardError");
            else if (ParameterSetName.Equals("UseShellExecute"))
                if (Verb != null)
                    startInfo.Verb = Verb;
            string targetMessage = StringUtil.Format(ProcessResources.StartProcessTarget, startInfo.FileName, startInfo.Arguments.Trim());
            Process process = null;
            using JobProcessCollection jobObject = new();
            bool? jobAssigned = null;
            if (startInfo.UseShellExecute)
                process = StartWithShellExecute(startInfo);
                process = new Process() { StartInfo = startInfo };
                SetupInputOutputRedirection(process);
                if (process.StartInfo.RedirectStandardOutput)
                    process.BeginOutputReadLine();
                if (process.StartInfo.RedirectStandardError)
                    process.BeginErrorReadLine();
                if (process.StartInfo.RedirectStandardInput)
                    WriteToStandardInput(process);
                using ProcessInformation processInfo = StartWithCreateProcess(startInfo);
                process = Process.GetProcessById(processInfo.ProcessId);
                // Starting a process as another user might make it impossible
                // to get the process handle from the S.D.Process object. Use
                // the ALL_ACCESS token from CreateProcess here to setup the
                // job object assignment early if -Wait was specified.
                // https://github.com/PowerShell/PowerShell/issues/17033
                    jobAssigned = jobObject.AssignProcessToJobObject(processInfo.Process);
                // Since the process wasn't spawned by .NET, we need to trigger .NET to get a lock on the handle of the process.
                // Otherwise, accessing properties like `ExitCode` will throw the following exception:
                //   "Process was not started by this object, so requested information cannot be determined."
                // Fetching the process handle will trigger the `Process` object to update its internal state by calling `SetProcessHandle`,
                // the result is discarded as it's not used later in this code.
                    _ = process.Handle;
                    // If the caller was not an admin and the process was started with another user's credentials .NET
                    // won't be able to retrieve the process handle. As this is not a critical failure we treat this as
                    // a warning.
                        string msg = StringUtil.Format(ProcessResources.FailedToCreateProcessObject, e.Message);
                        WriteDebug(msg);
                // Resume the process now that is has been set up.
                processInfo.Resume();
            if (PassThru.IsPresent)
                if (process != null)
                    message = StringUtil.Format(ProcessResources.CannotStarttheProcess);
            if (Wait.IsPresent)
                        process.WaitForExitAsync(_cancellationTokenSource.Token).GetAwaiter().GetResult();
                        // Add the process to the job, this may have already
                        // been done in StartWithCreateProcess.
                        if (jobAssigned == true || (jobAssigned is null && jobObject.AssignProcessToJobObject(process.SafeHandle)))
                            // Wait for the job object to finish
                            jobObject.WaitForExit(_cancellationTokenSource.Token);
                            // WinBlue: 27537 Start-Process -Wait doesn't work in a remote session on Windows 7 or lower.
                            // A Remote session is in it's own job and nested job support was only added in Windows 8/Server 2012.
        /// Implements ^c, after creating a process.
        protected override void StopProcessing() => _cancellationTokenSource.Cancel();
        #region IDisposable Overrides
        /// Dispose WaitHandle used to honor -Wait parameter.
        private string ResolveFilePath(string path)
            string filepath = PathUtils.ResolveFilePath(path, this);
            return filepath;
        private static void LoadEnvironmentVariable(ProcessStartInfo startinfo, IDictionary EnvironmentVariables)
            var processEnvironment = startinfo.EnvironmentVariables;
            foreach (DictionaryEntry entry in EnvironmentVariables)
                if (processEnvironment.ContainsKey(entry.Key.ToString()))
                    processEnvironment.Remove(entry.Key.ToString());
                if (entry.Value != null)
                    if (entry.Key.ToString().Equals("PATH"))
                        processEnvironment.Add(entry.Key.ToString(), entry.Value.ToString());
                        processEnvironment.Add(entry.Key.ToString(), entry.Value.ToString() + Path.PathSeparator + System.Environment.GetEnvironmentVariable(entry.Key.ToString(), EnvironmentVariableTarget.Machine) + Path.PathSeparator + System.Environment.GetEnvironmentVariable(entry.Key.ToString(), EnvironmentVariableTarget.User));
        private StreamWriter _outputWriter;
        private StreamWriter _errorWriter;
        private void StdOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
            if (!string.IsNullOrEmpty(outLine.Data))
                _outputWriter.WriteLine(outLine.Data);
                _outputWriter.Flush();
        private void StdErrorHandler(object sendingProcess, DataReceivedEventArgs outLine)
                _errorWriter.WriteLine(outLine.Data);
                _errorWriter.Flush();
        private void ExitHandler(object sendingProcess, System.EventArgs e)
            // To avoid a race condition with Std*Handler, let's wait a bit before closing the streams
            // System.Timer is not supported in CoreCLR, so let's spawn a new thread to do the wait
            Thread delayedStreamClosing = new Thread(StreamClosing);
            delayedStreamClosing.Start();
        private void StreamClosing()
            Thread.Sleep(1000);
            _outputWriter?.Dispose();
            _errorWriter?.Dispose();
        private void SetupInputOutputRedirection(Process p)
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardInput = false;
            if (_redirectstandardoutput != null)
                p.StartInfo.RedirectStandardOutput = true;
                p.OutputDataReceived += new DataReceivedEventHandler(StdOutputHandler);
                // Can't do StreamWriter(string) in coreCLR
                _outputWriter = new StreamWriter(new FileStream(_redirectstandardoutput, FileMode.Create));
                p.StartInfo.RedirectStandardOutput = false;
                _outputWriter = null;
            if (_redirectstandarderror != null)
                p.StartInfo.RedirectStandardError = true;
                p.ErrorDataReceived += new DataReceivedEventHandler(StdErrorHandler);
                _errorWriter = new StreamWriter(new FileStream(_redirectstandarderror, FileMode.Create));
                p.StartInfo.RedirectStandardError = false;
                _errorWriter = null;
            p.EnableRaisingEvents = true;
            p.Exited += new EventHandler(ExitHandler);
        private void WriteToStandardInput(Process p)
            StreamWriter writer = p.StandardInput;
            using (StreamReader reader = new StreamReader(new FileStream(_redirectstandardinput, FileMode.Open)))
                string line = reader.ReadToEnd();
                writer.WriteLine(line);
            writer.Dispose();
        private SafeFileHandle GetSafeFileHandleForRedirection(string RedirectionPath, FileMode mode)
            SafeFileHandle sf = null;
                sf = File.OpenHandle(RedirectionPath, mode, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Inheritable, FileOptions.WriteThrough);
            catch (Win32Exception win32ex)
                sf?.Dispose();
                string message = StringUtil.Format(ProcessResources.InvalidStartProcess, win32ex.Message);
            return sf;
        private static StringBuilder BuildCommandLine(string executableFileName, string arguments)
            StringBuilder builder = new();
            string str = executableFileName.Trim();
            bool flag = str.StartsWith('"') && str.EndsWith('"');
            if (!flag)
                builder.Append('"');
            builder.Append(str);
            if (!string.IsNullOrEmpty(arguments))
                builder.Append(arguments);
            return builder;
        private static byte[] ConvertEnvVarsToByteArray(StringDictionary sd)
            string[] array = new string[sd.Count];
            byte[] bytes = null;
            sd.Keys.CopyTo(array, 0);
            string[] strArray2 = new string[sd.Count];
            sd.Values.CopyTo(strArray2, 0);
            Array.Sort(array, strArray2, StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < sd.Count; i++)
                builder.Append(array[i]);
                builder.Append('=');
                builder.Append(strArray2[i]);
                builder.Append('\0');
            // Use Unicode encoding
            bytes = Encoding.Unicode.GetBytes(builder.ToString());
            return bytes;
        private void SetStartupInfo(ProcessStartInfo startinfo, ref ProcessNativeMethods.STARTUPINFO lpStartupInfo, ref int creationFlags)
            // If we are starting a process using the current console window, we need to set its standard handles
            // explicitly when they are not redirected because otherwise they won't be set and the new process will
            // fail with the "invalid handle" error.
            // However, if we are starting a process with a new console window, we should not explicitly set those
            // standard handles when they are not redirected, but instead let Windows figure out the default to use
            // when creating the process. Otherwise, the standard input handles of the current window and the new
            // window will get weirdly tied together and cause problems.
            bool hasRedirection = startinfo.CreateNoWindow
                || _redirectstandardinput is not null
                || _redirectstandardoutput is not null
                || _redirectstandarderror is not null;
            // RedirectionStandardInput
                startinfo.RedirectStandardInput = true;
                lpStartupInfo.hStdInput = GetSafeFileHandleForRedirection(_redirectstandardinput, FileMode.Open);
            else if (startinfo.CreateNoWindow)
                lpStartupInfo.hStdInput = new SafeFileHandle(
                    ProcessNativeMethods.GetStdHandle(-10),
                    ownsHandle: false);
            // RedirectionStandardOutput
                startinfo.RedirectStandardOutput = true;
                lpStartupInfo.hStdOutput = GetSafeFileHandleForRedirection(_redirectstandardoutput, FileMode.Create);
                lpStartupInfo.hStdOutput = new SafeFileHandle(
                    ProcessNativeMethods.GetStdHandle(-11),
            // RedirectionStandardError
                startinfo.RedirectStandardError = true;
                lpStartupInfo.hStdError = GetSafeFileHandleForRedirection(_redirectstandarderror, FileMode.Create);
                lpStartupInfo.hStdError = new SafeFileHandle(
                    ProcessNativeMethods.GetStdHandle(-12),
            if (hasRedirection)
                // Set STARTF_USESTDHANDLES only if there is redirection.
                lpStartupInfo.dwFlags = 0x100;
            if (startinfo.CreateNoWindow)
                // No new window: Inherit the parent process's console window
                creationFlags = 0x00000000;
                // CREATE_NEW_CONSOLE
                creationFlags |= 0x00000010;
                // STARTF_USESHOWWINDOW
                lpStartupInfo.dwFlags |= 0x00000001;
                // On headless SKUs like NanoServer and IoT, window style can only be the default value 'Normal'.
                switch (startinfo.WindowStyle)
                    case ProcessWindowStyle.Normal:
                        // SW_SHOWNORMAL
                        lpStartupInfo.wShowWindow = 1;
                    case ProcessWindowStyle.Minimized:
                        // SW_SHOWMINIMIZED
                        lpStartupInfo.wShowWindow = 2;
                    case ProcessWindowStyle.Maximized:
                        // SW_SHOWMAXIMIZED
                        lpStartupInfo.wShowWindow = 3;
                    case ProcessWindowStyle.Hidden:
                        // SW_HIDE
                        lpStartupInfo.wShowWindow = 0;
            // Create the new process suspended so we have a chance to get a corresponding Process object in case it terminates quickly.
            creationFlags |= 0x00000004;
        /// This method will be used on all windows platforms, both full desktop and headless SKUs.
        private ProcessInformation StartWithCreateProcess(ProcessStartInfo startinfo)
            ProcessNativeMethods.STARTUPINFO lpStartupInfo = new();
            ProcessNativeMethods.PROCESS_INFORMATION lpProcessInformation = new();
            int error = 0;
            GCHandle pinnedEnvironmentBlock = new();
            IntPtr AddressOfEnvironmentBlock = IntPtr.Zero;
            // building the cmdline with the file name given and it's arguments
            StringBuilder cmdLine = BuildCommandLine(startinfo.FileName, startinfo.Arguments);
                int creationFlags = 0;
                SetStartupInfo(startinfo, ref lpStartupInfo, ref creationFlags);
                // We follow the logic:
                //   - Ignore `UseNewEnvironment` when we run a process as another user.
                //          Setting initial environment variables makes sense only for current user.
                //   - Set environment variables if they present in ProcessStartupInfo.
                if (!UseNewEnvironment)
                    var environmentVars = startinfo.EnvironmentVariables;
                    if (environmentVars != null)
                        // All Windows Operating Systems that we support are Windows NT systems, so we use Unicode for environment.
                        creationFlags |= 0x400;
                        pinnedEnvironmentBlock = GCHandle.Alloc(ConvertEnvVarsToByteArray(environmentVars), GCHandleType.Pinned);
                        AddressOfEnvironmentBlock = pinnedEnvironmentBlock.AddrOfPinnedObject();
                bool flag;
                    // Run process as another user.
                    ProcessNativeMethods.LogonFlags logonFlags = 0;
                    if (startinfo.LoadUserProfile)
                        logonFlags = ProcessNativeMethods.LogonFlags.LOGON_WITH_PROFILE;
                    IntPtr password = IntPtr.Zero;
                        password = (startinfo.Password == null) ? Marshal.StringToCoTaskMemUni(string.Empty) : Marshal.SecureStringToCoTaskMemUnicode(startinfo.Password);
                        flag = ProcessNativeMethods.CreateProcessWithLogonW(startinfo.UserName, startinfo.Domain, password, logonFlags, null, cmdLine, creationFlags, AddressOfEnvironmentBlock, startinfo.WorkingDirectory, lpStartupInfo, ref lpProcessInformation);
                            ErrorRecord er = null;
                            if (error == 0xc1)
                                message = StringUtil.Format(ProcessResources.InvalidApplication, FilePath);
                            else if (error == 0x424)
                                // The API 'CreateProcessWithLogonW' depends on the 'Secondary Logon' service, but the component 'Microsoft-Windows-SecondaryLogonService'
                                // is not installed in OneCoreUAP. We will get error code 0x424 when the service is not available.
                                message = StringUtil.Format(ProcessResources.ParameterNotSupported, "-Credential", "Start-Process");
                                er = new ErrorRecord(new NotSupportedException(message), "NotSupportedException", ErrorCategory.NotInstalled, null);
                                Win32Exception win32ex = new(error);
                                message = StringUtil.Format(ProcessResources.InvalidStartProcess, win32ex.Message);
                            er ??= new ErrorRecord(new InvalidOperationException(message), "InvalidOperationException", ErrorCategory.InvalidOperation, null);
                        goto Label_03AE;
                        if (password != IntPtr.Zero)
                            Marshal.ZeroFreeCoTaskMemUnicode(password);
                // Run process as current user.
                if (UseNewEnvironment)
                    IntPtr token = WindowsIdentity.GetCurrent().Token;
                    if (!ProcessNativeMethods.CreateEnvironmentBlock(out AddressOfEnvironmentBlock, token, false))
                        var errorRecord = new ErrorRecord(new InvalidOperationException(message), "InvalidOperationException", ErrorCategory.InvalidOperation, null);
                        ThrowTerminatingError(errorRecord);
                ProcessNativeMethods.SECURITY_ATTRIBUTES lpProcessAttributes = new();
                ProcessNativeMethods.SECURITY_ATTRIBUTES lpThreadAttributes = new();
                flag = ProcessNativeMethods.CreateProcess(null, cmdLine, lpProcessAttributes, lpThreadAttributes, true, creationFlags, AddressOfEnvironmentBlock, startinfo.WorkingDirectory, lpStartupInfo, ref lpProcessInformation);
            Label_03AE:
                return new ProcessInformation(lpProcessInformation);
                if (pinnedEnvironmentBlock.IsAllocated)
                    pinnedEnvironmentBlock.Free();
                    ProcessNativeMethods.DestroyEnvironmentBlock(AddressOfEnvironmentBlock);
                lpStartupInfo.Dispose();
        /// This method will be used only on Windows full desktop.
        private Process StartWithShellExecute(ProcessStartInfo startInfo)
            Process result = null;
                result = Process.Start(startInfo);
                string message = StringUtil.Format(ProcessResources.InvalidStartProcess, ex.Message);
    /// Provides argument completion for Verb parameter.
    public class VerbArgumentCompleter : IArgumentCompleter
        /// Returns completion results for verb parameter.
            // -Verb is not supported on non-Windows platforms as well as Windows headless SKUs
            if (!Platform.IsWindowsDesktop)
                return Array.Empty<CompletionResult>();
            // Completion: Start-Process -FilePath <path> -Verb <wordToComplete>
            if (commandName.Equals("Start-Process", StringComparison.OrdinalIgnoreCase)
                && fakeBoundParameters.Contains("FilePath"))
                string filePath = fakeBoundParameters["FilePath"].ToString();
                // Complete file verbs if extension exists
                if (Path.HasExtension(filePath))
                    return CompleteFileVerbs(wordToComplete, filePath);
                // Otherwise check if command is an Application to resolve executable full path with extension
                // e.g if powershell was given, resolve to powershell.exe to get verbs
                var commandInfo = new CmdletInfo("Get-Command", typeof(GetCommandCommand));
                ps.AddCommand(commandInfo);
                ps.AddParameter("Name", filePath);
                ps.AddParameter("CommandType", CommandTypes.Application);
                Collection<CommandInfo> commands = ps.Invoke<CommandInfo>();
                // Start-Process & Get-Command select first found application based on PATHEXT environment variable
                if (commands.Count >= 1)
                    return CompleteFileVerbs(wordToComplete, filePath: commands[0].Source);
        /// Completes file verbs.
        /// <param name="filePath">The file path to get verbs.</param>
        /// <returns>List of file verbs to complete.</returns>
        private static IEnumerable<CompletionResult> CompleteFileVerbs(string wordToComplete, string filePath)
            => CompletionHelpers.GetMatchingResults(
                possibleCompletionValues: new ProcessStartInfo(filePath).Verbs);
    /// ProcessInformation is a helper class that wraps the native PROCESS_INFORMATION structure
    /// returned by CreateProcess or CreateProcessWithLogon. It ensures the process and thread
    /// HANDLEs are disposed once it's not needed.
    internal sealed class ProcessInformation : IDisposable
        public SafeProcessHandle Process { get; }
        public SafeProcessHandle Thread { get; }
        public Int32 ProcessId { get; }
        public Int32 ThreadId { get; }
        internal ProcessInformation(ProcessNativeMethods.PROCESS_INFORMATION info)
            Process = new(info.hProcess, true);
            Thread = new(info.hThread, true);
            ProcessId = info.dwProcessId;
            ThreadId = info.dwThreadId;
        public void Resume()
            ProcessNativeMethods.ResumeThread(Thread.DangerousGetHandle());
            Process.Dispose();
            Thread.Dispose();
        ~ProcessInformation() => Dispose();
    internal static class ProcessNativeMethods
        [DllImport(PinvokeDllNames.GetStdHandleDllName, SetLastError = true)]
        public static extern IntPtr GetStdHandle(int whichHandle);
        [DllImport(PinvokeDllNames.CreateProcessWithLogonWDllName, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        internal static extern bool CreateProcessWithLogonW(string userName,
            string domain,
            IntPtr password,
            LogonFlags logonFlags,
            [MarshalAs(UnmanagedType.LPWStr)] string appName,
            StringBuilder cmdLine,
            int creationFlags,
            IntPtr environmentBlock,
            [MarshalAs(UnmanagedType.LPWStr)] string lpCurrentDirectory,
            STARTUPINFO lpStartupInfo,
            ref PROCESS_INFORMATION lpProcessInformation);
        [DllImport(PinvokeDllNames.CreateProcessDllName, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CreateProcess([MarshalAs(UnmanagedType.LPWStr)] string lpApplicationName,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpCommandLine,
            SECURITY_ATTRIBUTES lpProcessAttributes,
            SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            int dwCreationFlags,
            IntPtr lpEnvironment,
        [DllImport(PinvokeDllNames.ResumeThreadDllName, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint ResumeThread(IntPtr threadHandle);
        [DllImport("userenv.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CreateEnvironmentBlock(out IntPtr lpEnvironment, IntPtr hToken, bool bInherit);
        public static extern bool DestroyEnvironmentBlock(IntPtr lpEnvironment);
        internal enum LogonFlags
            LOGON_NETCREDENTIALS_ONLY = 2,
            LOGON_WITH_PROFILE = 1
        internal struct PROCESS_INFORMATION
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        internal sealed class SECURITY_ATTRIBUTES
            public int nLength;
            public SafeLocalMemHandle lpSecurityDescriptor;
            public bool bInheritHandle;
            public SECURITY_ATTRIBUTES()
                this.nLength = 12;
                this.bInheritHandle = true;
                this.lpSecurityDescriptor = new SafeLocalMemHandle(IntPtr.Zero, true);
        internal sealed class SafeLocalMemHandle : SafeHandleZeroOrMinusOneIsInvalid
            // Methods
            internal SafeLocalMemHandle()
                : base(true)
            internal SafeLocalMemHandle(IntPtr existingHandle, bool ownsHandle)
                : base(ownsHandle)
                base.SetHandle(existingHandle);
            [DllImport(PinvokeDllNames.LocalFreeDllName)]
            private static extern IntPtr LocalFree(IntPtr hMem);
                return (LocalFree(base.handle) == IntPtr.Zero);
        internal sealed class STARTUPINFO
            public int cb;
            public IntPtr lpReserved;
            public IntPtr lpDesktop;
            public IntPtr lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public SafeFileHandle hStdInput;
            public SafeFileHandle hStdOutput;
            public SafeFileHandle hStdError;
            public STARTUPINFO()
                this.lpReserved = IntPtr.Zero;
                this.lpDesktop = IntPtr.Zero;
                this.lpTitle = IntPtr.Zero;
                this.lpReserved2 = IntPtr.Zero;
                this.hStdInput = new SafeFileHandle(IntPtr.Zero, false);
                this.hStdOutput = new SafeFileHandle(IntPtr.Zero, false);
                this.hStdError = new SafeFileHandle(IntPtr.Zero, false);
                this.cb = Marshal.SizeOf(this);
                    if ((this.hStdInput != null) && !this.hStdInput.IsInvalid)
                        this.hStdInput.Dispose();
                        this.hStdInput = null;
                    if ((this.hStdOutput != null) && !this.hStdOutput.IsInvalid)
                        this.hStdOutput.Dispose();
                        this.hStdOutput = null;
                    if ((this.hStdError != null) && !this.hStdError.IsInvalid)
                        this.hStdError.Dispose();
                        this.hStdError = null;
    #region ProcessCommandException
    /// Non-terminating errors occurring in the process noun commands.
    public class ProcessCommandException : SystemException
        #region ctors
        /// Unimplemented standard constructor.
        /// <returns>Doesn't return.</returns>
        public ProcessCommandException() : base()
        /// Standard constructor.
        /// <returns>Constructed object.</returns>
        public ProcessCommandException(string message) : base(message)
        public ProcessCommandException(string message, Exception innerException)
            : base(message, innerException)
        #endregion ctors
        #region Serialization
        /// Serialization constructor.
        /// <param name="info"></param>
        protected ProcessCommandException(
        #endregion Serialization
        /// Name of the process which could not be found or operated upon.
        public string ProcessName
            get { return _processName; }
            set { _processName = value; }
        private string _processName = string.Empty;
    #endregion ProcessCommandException

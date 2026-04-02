    /// This cmdlet enters into an interactive session with the specified local process by
    /// creating a remote runspace to the process and pushing it on the current PSHost.
    /// If the selected process does not contain PowerShell then an error message will result.
    /// If the current user does not have sufficient privileges to attach to the selected process
    /// then an error message will result.
    [Cmdlet(VerbsCommon.Enter, "PSHostProcess", DefaultParameterSetName = EnterPSHostProcessCommand.ProcessIdParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2096580")]
    public sealed class EnterPSHostProcessCommand : PSCmdlet
        private IHostSupportsInteractiveSession _interactiveHost;
        private RemoteRunspace _connectingRemoteRunspace;
        private const string ProcessParameterSet = "ProcessParameterSet";
        private const string ProcessNameParameterSet = "ProcessNameParameterSet";
        private const string ProcessIdParameterSet = "ProcessIdParameterSet";
        private const string PipeNameParameterSet = "PipeNameParameterSet";
        private const string PSHostProcessInfoParameterSet = "PSHostProcessInfoParameterSet";
        private const string NamedPipeRunspaceName = "PSAttachRunspace";
        /// Process to enter.
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ParameterSetName = EnterPSHostProcessCommand.ProcessParameterSet)]
        public Process Process
        /// Id of process to enter.
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = EnterPSHostProcessCommand.ProcessIdParameterSet)]
        /// Name of process to enter.  An error will result if more than one such process exists.
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = EnterPSHostProcessCommand.ProcessNameParameterSet)]
        /// Host Process Info object that describes a connectible process.
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ParameterSetName = EnterPSHostProcessCommand.PSHostProcessInfoParameterSet)]
        public PSHostProcessInfo HostProcessInfo
        /// Gets or sets the custom named pipe name to connect to. This is usually used in conjunction with `pwsh -CustomPipeName`.
        [Parameter(Mandatory = true, ParameterSetName = EnterPSHostProcessCommand.PipeNameParameterSet)]
        public string CustomPipeName
        /// Optional name of AppDomain in process to enter.  If not specified then the default AppDomain is used.
        [Parameter(Position = 1, ParameterSetName = EnterPSHostProcessCommand.ProcessParameterSet)]
        [Parameter(Position = 1, ParameterSetName = EnterPSHostProcessCommand.ProcessIdParameterSet)]
        [Parameter(Position = 1, ParameterSetName = EnterPSHostProcessCommand.ProcessNameParameterSet)]
        [Parameter(Position = 1, ParameterSetName = EnterPSHostProcessCommand.PSHostProcessInfoParameterSet)]
        public string AppDomainName
            // Check if system is in locked down mode, in which case this cmdlet is disabled.
                        new PSSecurityException(RemotingErrorIdStrings.EnterPSHostProcessCmdletDisabled),
                        "EnterPSHostProcessCmdletDisabled",
            // Check for host that supports interactive remote sessions.
            _interactiveHost = this.Host as IHostSupportsInteractiveSession;
            if (_interactiveHost == null)
                        new ArgumentException(RemotingErrorIdStrings.HostDoesNotSupportIASession),
                        "EnterPSHostProcessHostDoesNotSupportIASession",
            // Check selected process for existence, and whether it hosts PowerShell.
            Runspace namedPipeRunspace = null;
                case ProcessIdParameterSet:
                    Process = GetProcessById(Id);
                    VerifyProcess(Process);
                    namedPipeRunspace = CreateNamedPipeRunspace(Process.Id, AppDomainName);
                case ProcessNameParameterSet:
                    Process = GetProcessByName(Name);
                case PSHostProcessInfoParameterSet:
                    Process = GetProcessByHostProcessInfo(HostProcessInfo);
                    // Create named pipe runspace for selected process and open.
                case PipeNameParameterSet:
                    VerifyPipeName(CustomPipeName);
                    namedPipeRunspace = CreateNamedPipeRunspace(CustomPipeName);
            // Set runspace prompt.  The runspace is closed on pop so we don't
            // have to reverse this change.
            PrepareRunspace(namedPipeRunspace);
                // Push runspace onto host.
                _interactiveHost.PushRunspace(namedPipeRunspace);
                namedPipeRunspace.Close();
                        "EnterPSHostProcessCannotPushRunspace",
        /// Stop Processing.
            RemoteRunspace connectingRunspace = _connectingRemoteRunspace;
            connectingRunspace?.AbortOpen();
        private Runspace CreateNamedPipeRunspace(string customPipeName)
            NamedPipeConnectionInfo connectionInfo = new NamedPipeConnectionInfo(customPipeName);
            return CreateNamedPipeRunspace(connectionInfo);
        private Runspace CreateNamedPipeRunspace(int procId, string appDomainName)
            NamedPipeConnectionInfo connectionInfo = new NamedPipeConnectionInfo(procId, appDomainName);
        private Runspace CreateNamedPipeRunspace(NamedPipeConnectionInfo connectionInfo)
            RemoteRunspace remoteRunspace = RunspaceFactory.CreateRunspace(connectionInfo, this.Host, typeTable) as RemoteRunspace;
            remoteRunspace.Name = NamedPipeRunspaceName;
            _connectingRemoteRunspace = remoteRunspace;
                remoteRunspace.Debugger?.SetDebugMode(DebugModes.LocalScript | DebugModes.RemoteScript);
                // Unwrap inner exception for original error message, if any.
                string errorMessage = (e.InnerException != null) ? (e.InnerException.Message ?? string.Empty) : string.Empty;
                if (connectionInfo.CustomPipeName != null)
                                    RemotingErrorIdStrings.EnterPSHostProcessCannotConnectToPipe,
                                    connectionInfo.CustomPipeName,
                                    errorMessage),
                                e.InnerException),
                            "EnterPSHostProcessCannotConnectToPipe",
                    string msgAppDomainName = connectionInfo.AppDomainName ?? NamedPipeUtils.DefaultAppDomainName;
                                    RemotingErrorIdStrings.EnterPSHostProcessCannotConnectToProcess,
                                    msgAppDomainName,
                                    connectionInfo.ProcessId,
                            "EnterPSHostProcessCannotConnectToProcess",
                _connectingRemoteRunspace = null;
        private static void PrepareRunspace(Runspace runspace)
            string promptFn = StringUtil.Format(RemotingErrorIdStrings.EnterPSHostProcessPrompt,
                @"function global:prompt { """,
                @"PS $($executionContext.SessionState.Path.CurrentLocation)> "" }"
            // Set prompt in pushed named pipe runspace.
                    // Set pushed runspace prompt.
                    ps.AddScript(promptFn).Invoke();
        private Process GetProcessById(int procId)
            var process = PSHostProcessUtils.GetProcessById(procId);
            if (process is null)
                        new PSArgumentException(StringUtil.Format(RemotingErrorIdStrings.EnterPSHostProcessNoProcessFoundWithId, procId)),
                        "EnterPSHostProcessNoProcessFoundWithId",
            return process;
        private Process GetProcessByHostProcessInfo(PSHostProcessInfo hostProcessInfo)
            return GetProcessById(hostProcessInfo.ProcessId);
        private Process GetProcessByName(string name)
            Collection<Process> foundProcesses;
                ps.AddCommand("Get-Process").AddParameter("Name", name);
                foundProcesses = ps.Invoke<Process>();
            if (foundProcesses.Count == 0)
                            new PSArgumentException(StringUtil.Format(RemotingErrorIdStrings.EnterPSHostProcessNoProcessFoundWithName, name)),
                            "EnterPSHostProcessNoProcessFoundWithName",
            else if (foundProcesses.Count > 1)
                            new PSArgumentException(StringUtil.Format(RemotingErrorIdStrings.EnterPSHostProcessMultipleProcessesFoundWithName, name)),
                            "EnterPSHostProcessMultipleProcessesFoundWithName",
            return foundProcesses[0];
        private void VerifyProcess(Process process)
            if (process.Id == Environment.ProcessId)
                            new PSInvalidOperationException(RemotingErrorIdStrings.EnterPSHostProcessCantEnterSameProcess),
                            "EnterPSHostProcessCantEnterSameProcess",
            bool hostsSMA = false;
            IReadOnlyCollection<PSHostProcessInfo> availableProcInfo = GetPSHostProcessInfoCommand.GetAppDomainNamesFromProcessId(null);
            foreach (var procInfo in availableProcInfo)
                if (process.Id == procInfo.ProcessId)
                    hostsSMA = true;
            if (!hostsSMA)
                            new PSInvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.EnterPSHostProcessNoPowerShell, Process.Id)),
                            "EnterPSHostProcessNoPowerShell",
        private void VerifyPipeName(string customPipeName)
            // Named Pipes are represented differently on Windows vs macOS & Linux
            var sb = new StringBuilder(customPipeName.Length);
                sb.Append(@"\\.\pipe\");
                sb.Append(Path.GetTempPath()).Append("CoreFxPipe_");
            sb.Append(customPipeName);
            string pipePath = sb.ToString();
            if (!File.Exists(pipePath))
                        new PSArgumentException(StringUtil.Format(RemotingErrorIdStrings.EnterPSHostProcessNoNamedPipeFound, customPipeName)),
                        "EnterPSHostProcessNoNamedPipeFound",
    /// This cmdlet exits an interactive session with a local process.
    [Cmdlet(VerbsCommon.Exit, "PSHostProcess",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2096583")]
    public sealed class ExitPSHostProcessCommand : PSCmdlet
            var _interactiveHost = this.Host as IHostSupportsInteractiveSession;
                        "ExitPSHostProcessHostDoesNotSupportIASession",
            _interactiveHost.PopRunspace();
    /// This cmdlet returns a collection of PSHostProcessInfo objects containing
    /// process and AppDomain name information for processes that have PowerShell loaded.
    [Cmdlet(VerbsCommon.Get, "PSHostProcessInfo", DefaultParameterSetName = GetPSHostProcessInfoCommand.ProcessNameParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkId=517012")]
    [OutputType(typeof(PSHostProcessInfo))]
    public sealed class GetPSHostProcessInfoCommand : PSCmdlet
        // CoreFx uses the system temp path to store the file used for named pipes and is not settable.
        // This member is only used by Get-PSHostProcessInfo to know where to look for the named pipe files.
        private static readonly string NamedPipePath = Path.GetTempPath();
        private const string NamedPipePath = @"\\.\pipe\";
        /// Name of Process.
        [Parameter(Position = 0, ParameterSetName = GetPSHostProcessInfoCommand.ProcessNameParameterSet)]
        /// Process.
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ParameterSetName = GetPSHostProcessInfoCommand.ProcessParameterSet)]
        public Process[] Process
        /// Id of process.
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = GetPSHostProcessInfoCommand.ProcessIdParameterSet)]
        /// End bock processing.
            IReadOnlyCollection<PSHostProcessInfo> processAppDomainInfo;
                    processAppDomainInfo = GetAppDomainNamesFromProcessId(GetProcIdsFromNames(Name));
                    processAppDomainInfo = GetAppDomainNamesFromProcessId(Id);
                case ProcessParameterSet:
                    processAppDomainInfo = GetAppDomainNamesFromProcessId(GetProcIdsFromProcs(Process));
                    Debug.Fail("Unknown parameter set.");
                    processAppDomainInfo = new ReadOnlyCollection<PSHostProcessInfo>(new Collection<PSHostProcessInfo>());
            WriteObject(processAppDomainInfo, true);
        private static int[] GetProcIdsFromProcs(Process[] processes)
            List<int> returnIds = new List<int>();
            foreach (Process process in processes)
                returnIds.Add(process.Id);
            return returnIds.ToArray();
        private static int[] GetProcIdsFromNames(string[] names)
            if ((names == null) || (names.Length == 0))
            System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcesses();
                foreach (var proc in processes)
                    // Skip processes that have already terminated.
                    if (proc.HasExited)
                        if (namePattern.IsMatch(proc.ProcessName))
                            returnIds.Add(proc.Id);
                        // Ignore if process has exited in the mean time.
        /// Returns all named pipe AppDomain names for given process Ids or all PowerShell
        /// processes if procIds parameter is null.
        /// PowerShell pipe name example:
        ///     PSHost.130566795082911445.8224.DefaultAppDomain.powershell.
        /// <param name="procIds">Process Ids or null.</param>
        /// <returns>Collection of process AppDomain info.</returns>
        internal static IReadOnlyCollection<PSHostProcessInfo> GetAppDomainNamesFromProcessId(int[] procIds)
            var procAppDomainInfo = new List<PSHostProcessInfo>();
            // Get all named pipe 'files' on local machine.
            List<string> namedPipes = new List<string>();
            var namedPipeDirectory = new DirectoryInfo(NamedPipePath);
            foreach (var pipeFileInfo in namedPipeDirectory.EnumerateFiles(NamedPipeUtils.NamedPipeNamePrefixSearch))
                namedPipes.Add(Path.Combine(pipeFileInfo.DirectoryName, pipeFileInfo.Name));
            // Collect all PowerShell named pipes for given process Ids.
            foreach (string namedPipe in namedPipes)
                int startIndex = namedPipe.IndexOf(NamedPipeUtils.NamedPipeNamePrefix, StringComparison.OrdinalIgnoreCase);
                    int pStartTimeIndex = namedPipe.IndexOf('.', startIndex);
                    if (pStartTimeIndex > -1)
                        int pIdIndex = namedPipe.IndexOf('.', pStartTimeIndex + 1);
                        if (pIdIndex > -1)
                            int pAppDomainIndex = namedPipe.IndexOf('.', pIdIndex + 1);
                            if (pAppDomainIndex > -1)
                                ReadOnlySpan<char> idString = namedPipe.AsSpan(pIdIndex + 1, (pAppDomainIndex - pIdIndex - 1));
                                int id = -1;
                                if (int.TryParse(idString, out id))
                                    // Filter on provided proc Ids.
                                    if (procIds != null)
                                        foreach (int procId in procIds)
                                            if (id == procId)
                                    // Process id is not valid so we'll skip
                                int pNameIndex = namedPipe.IndexOf('.', pAppDomainIndex + 1);
                                if (pNameIndex > -1)
                                    string appDomainName = namedPipe.Substring(pAppDomainIndex + 1, (pNameIndex - pAppDomainIndex - 1));
                                    string pName = namedPipe.Substring(pNameIndex + 1);
                                        process = PSHostProcessUtils.GetProcessById(id);
                                        // Do nothing if the process no longer exists
                                    if (process == null)
                                            // If the process is gone, try removing the PSHost named pipe
                                            var pipeFile = new FileInfo(namedPipe);
                                            pipeFile.Delete();
                                            // best effort to cleanup
                                            if (process.ProcessName.Equals(pName, StringComparison.Ordinal))
                                                // only add if the process name matches
                                                procAppDomainInfo.Add(new PSHostProcessInfo(pName, id, appDomainName, namedPipe));
            if (procAppDomainInfo.Count > 1)
                // Sort list by process name.
                var comparerInfo = CultureInfo.InvariantCulture.CompareInfo;
                procAppDomainInfo.Sort((firstItem, secondItem) => comparerInfo.Compare(firstItem.ProcessName, secondItem.ProcessName, CompareOptions.IgnoreCase));
            return new ReadOnlyCollection<PSHostProcessInfo>(procAppDomainInfo);
    #region PSHostProcessInfo class
    /// PowerShell host process information class.
    public sealed class PSHostProcessInfo
        private readonly string _pipeNameFilePath;
        /// Name of process.
        public string ProcessName { get; }
        /// Name of PowerShell AppDomain in process.
        public string AppDomainName { get; }
        /// Main window title of the process.
        public string MainWindowTitle { get; }
        private PSHostProcessInfo() { }
        /// Initializes a new instance of the PSHostProcessInfo type.
        /// <param name="processName">Name of process.</param>
        /// <param name="processId">Id of process.</param>
        /// <param name="appDomainName">Name of process AppDomain.</param>
        /// <param name="pipeNameFilePath">File path of pipe name.</param>
        internal PSHostProcessInfo(
            string appDomainName,
            string pipeNameFilePath)
            if (string.IsNullOrEmpty(processName))
                throw new PSArgumentNullException(nameof(processName));
            if (string.IsNullOrEmpty(appDomainName))
                throw new PSArgumentNullException(nameof(appDomainName));
            MainWindowTitle = string.Empty;
                var process = PSHostProcessUtils.GetProcessById(processId);
                MainWindowTitle = process?.MainWindowTitle ?? string.Empty;
                // Window title is optional.
            this.ProcessName = processName;
            this.ProcessId = processId;
            this.AppDomainName = appDomainName;
            _pipeNameFilePath = pipeNameFilePath;
        /// Retrieves the pipe name file path.
        /// <returns>Pipe name file path.</returns>
        public string GetPipeNameFilePath()
            return _pipeNameFilePath;
    #region PSHostProcessUtils
    internal static class PSHostProcessUtils
        /// Return a System.Diagnostics.Process object by process Id,
        /// or null if not found or process has exited.
        /// <param name="procId">Process of Id to find.</param>
        /// <returns>Process object or null.</returns>
        public static Process GetProcessById(int procId)
                var process = Process.GetProcessById(procId);
                return process.HasExited ? null : process;
            catch (System.ArgumentException)

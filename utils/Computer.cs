using System.Security.Cryptography;
    #region Restart-Computer
    /// This exception is thrown when the timeout expires before a computer finishes restarting.
    public sealed class RestartComputerTimeoutException : RuntimeException
        /// Name of the computer that is restarting.
        /// The timeout value specified by the user. It indicates the seconds to wait before timeout.
        public int Timeout { get; }
        /// Construct a RestartComputerTimeoutException.
        internal RestartComputerTimeoutException(string computerName, int timeout, string message, string errorId)
            : base(message)
            SetErrorId(errorId);
            SetErrorCategory(ErrorCategory.OperationTimeout);
            ComputerName = computerName;
            Timeout = timeout;
        public RestartComputerTimeoutException() : base() { }
        /// Constructs a RestartComputerTimeoutException.
        /// <param name="message">
        /// The message used in the exception.
        public RestartComputerTimeoutException(string message) : base(message) { }
        /// <param name="innerException">
        /// An exception that led to this exception.
        public RestartComputerTimeoutException(string message, Exception innerException) : base(message, innerException) { }
    /// Defines the services that Restart-Computer can wait on.
    [SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
    public enum WaitForServiceTypes
        /// Wait for the WMI service to be ready.
        Wmi = 0x0,
        /// Wait for the WinRM service to be ready.
        WinRM = 0x1,
        /// Wait for the PowerShell to be ready.
        PowerShell = 0x2,
    /// Restarts the computer.
    [Cmdlet(VerbsLifecycle.Restart, "Computer", SupportsShouldProcess = true, DefaultParameterSetName = DefaultParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097060", RemotingCapability = RemotingCapability.OwnedByCommand)]
    public class RestartComputerCommand : PSCmdlet, IDisposable
        #region "Parameters and PrivateData"
        private const string DefaultParameterSet = "DefaultSet";
        private const int forcedReboot = 6; // see https://msdn.microsoft.com/library/aa394058(v=vs.85).aspx
        /// The authentication options for CIM_WSMan connection.
        [Parameter(ParameterSetName = DefaultParameterSet)]
        [ValidateSet(
            "Default",
            "Basic",
            "Negotiate", // can be used with and without credential (without -> PSRP mapped to NegotiateWithImplicitCredential)
            "CredSSP",
            "Digest",
            "Kerberos")] // can be used with explicit or implicit credential
        public string WsmanAuthentication { get; set; }
        /// Specifies the computer (s)Name on which this command is executed.
        /// When this parameter is omitted, this cmdlet restarts the local computer.
        /// Type the NETBIOS name, IP address, or fully-qualified domain name of one
        /// or more computers in a comma-separated list. To specify the local computer, type the computername or "localhost".
        [Parameter(Position = 0, ValueFromPipeline = true,
        [Alias("CN", "__SERVER", "Server", "IPAddress")]
        public string[] ComputerName { get; set; } = new string[] { "." };
        private List<string> _validatedComputerNames = new();
        private readonly List<string> _waitOnComputers = new();
        private readonly HashSet<string> _uniqueComputerNames = new(StringComparer.OrdinalIgnoreCase);
        /// Specifies a user account that has permission to perform this action. Type a
        /// user-name, such as "User01" or "Domain01\User01", or enter a PSCredential
        /// object, such as one from the Get-Credential cmdlet.
        [Parameter(Position = 1)]
        /// Using Force in conjunction with Reboot on a
        /// remote computer immediately reboots the remote computer.
        [Alias("f")]
        /// Specify the Wait parameter. Prompt will be blocked is the Timeout is not 0.
        public SwitchParameter Wait { get; set; }
        /// Specify the Timeout parameter.
        /// Negative value indicates wait infinitely.
        /// Positive value indicates the seconds to wait before timeout.
        [Alias("TimeoutSec")]
        [ValidateRange(-1, int.MaxValue)]
        public int Timeout
                return _timeout;
                _timeout = value;
                _timeoutSpecified = true;
        private int _timeout = -1;
        private bool _timeoutSpecified = false;
        /// Specify the For parameter.
        /// Wait for the specific service before unblocking the prompt.
        public WaitForServiceTypes For
                return _waitFor;
                _waitFor = value;
                _waitForSpecified = true;
        private WaitForServiceTypes _waitFor = WaitForServiceTypes.PowerShell;
        private bool _waitForSpecified = false;
        /// Specify the Delay parameter.
        /// The specific time interval (in second) to wait between network pings or service queries.
        [ValidateRange(1, short.MaxValue)]
        public short Delay
                return (short)_delay;
                _delay = value;
                _delaySpecified = true;
        private int _delay = 5;
        private bool _delaySpecified = false;
        /// Script to test if the PowerShell is ready.
        private const string TestPowershellScript = @"
$array = @($input)
$result = @{}
foreach ($computerName in $array[1])
    $ret = $null
    $arguments = @{
        ComputerName = $computerName
        ScriptBlock = { $true }
        SessionOption = New-PSSessionOption -NoMachineProfile
        ErrorAction = 'SilentlyContinue'
    if ( $null -ne $array[0] )
        $arguments['Credential'] = $array[0]
    $result[$computerName] = (Invoke-Command @arguments) -as [bool]
$result
";
        /// The indicator to use when show progress.
        private readonly string[] _indicator = { "|", "/", "-", "\\" };
        /// The activity id.
        private int _activityId;
        /// After call 'Shutdown' on the target computer, wait a few
        /// seconds for the restart to begin.
        private const int SecondsToWaitForRestartToBegin = 25;
        /// Actual time out in seconds.
        private int _timeoutInMilliseconds;
        /// Indicate to exit.
        private bool _exit, _timeUp;
        private readonly CancellationTokenSource _cancel = new();
        /// A waithandler to wait on. Current thread will wait on it during the delay interval.
        private readonly ManualResetEventSlim _waitHandler = new(false);
        private readonly Dictionary<string, ComputerInfo> _computerInfos = new(StringComparer.OrdinalIgnoreCase);
        // CLR 4.0 Port note - use https://msdn.microsoft.com/library/system.net.networkinformation.ipglobalproperties.hostname(v=vs.110).aspx
        private readonly string _shortLocalMachineName = Dns.GetHostName();
        // And for this, use PsUtils.GetHostname()
        private readonly string _fullLocalMachineName = Dns.GetHostEntryAsync(string.Empty).Result.HostName;
        private int _percent;
        private string _status;
        private string _activity;
        private Timer _timer;
        private System.Management.Automation.PowerShell _powershell;
        private const string StageVerification = "VerifyStage";
        private const string WmiConnectionTest = "WMI";
        private const string WinrmConnectionTest = "WinRM";
        private const string PowerShellConnectionTest = "PowerShell";
        #endregion "parameters and PrivateData"
        #region "IDisposable Members"
        /// Dispose Method.
            // Use SuppressFinalize in case a subclass
            // of this type implements a finalizer.
        /// <param name="disposing"></param>
        public void Dispose(bool disposing)
                _timer?.Dispose();
                _waitHandler.Dispose();
                _cancel.Dispose();
                _powershell?.Dispose();
        #endregion "IDisposable Members"
        #region "Private Methods"
        /// Validate parameters for 'DefaultSet'
        /// 1. When the Wait is specified, the computername cannot contain the local machine
        /// 2. If the local machine is present, make sure it is at the end of the list (so the remote ones get restarted before the local machine reboot).
        private void ValidateComputerNames()
            bool containLocalhost = false;
            _validatedComputerNames.Clear();
            foreach (string name in ComputerName)
                ErrorRecord error = null;
                string targetComputerName = ComputerWMIHelper.ValidateComputerName(name, _shortLocalMachineName, _fullLocalMachineName, ref error);
                if (targetComputerName == null)
                    if (error != null)
                        WriteError(error);
                if (targetComputerName.Equals(ComputerWMIHelper.localhostStr, StringComparison.OrdinalIgnoreCase))
                    containLocalhost = true;
                else if (!_uniqueComputerNames.Contains(targetComputerName))
                    _validatedComputerNames.Add(targetComputerName);
                    _uniqueComputerNames.Add(targetComputerName);
            // Force wait with a test hook even if we're on the local computer
            if (!InternalTestHooks.TestWaitStopComputer && Wait && containLocalhost)
                // The local machine will be ignored, and an error will be emitted.
                InvalidOperationException ex = new(ComputerResources.CannotWaitLocalComputer);
                WriteError(new ErrorRecord(ex, "CannotWaitLocalComputer", ErrorCategory.InvalidOperation, null));
                containLocalhost = false;
            // Add the localhost to the end of the list, so we will restart remote machines
            // before we restart the local one.
            if (containLocalhost)
                _validatedComputerNames.Add(ComputerWMIHelper.localhostStr);
        /// Write out progress.
        /// <param name="status"></param>
        /// <param name="percent"></param>
        /// <param name="progressRecordType"></param>
        private void WriteProgress(string activity, string status, int percent, ProgressRecordType progressRecordType)
            ProgressRecord progress = new(_activityId, activity, status);
            progress.PercentComplete = percent;
            progress.RecordType = progressRecordType;
        /// Calculate the progress percentage.
        /// <param name="currentStage"></param>
        private int CalculateProgressPercentage(string currentStage)
            switch (currentStage)
                case StageVerification:
                    return _waitFor.Equals(WaitForServiceTypes.Wmi) || _waitFor.Equals(WaitForServiceTypes.WinRM)
                               ? 33
                               : 20;
                case WmiConnectionTest:
                    return _waitFor.Equals(WaitForServiceTypes.Wmi) ? 66 : 40;
                case WinrmConnectionTest:
                    return _waitFor.Equals(WaitForServiceTypes.WinRM) ? 66 : 60;
                case PowerShellConnectionTest:
                    return 80;
            Dbg.Diagnostics.Assert(false, "CalculateProgressPercentage should never hit the default case");
        /// Event handler for the timer.
        /// <param name="s"></param>
        private void OnTimedEvent(object s)
            _exit = _timeUp = true;
            _cancel.Cancel();
            _waitHandler.Set();
            if (_powershell != null)
                _powershell.Stop();
                _powershell.Dispose();
        private sealed class ComputerInfo
            internal string LastBootUpTime;
            internal bool RebootComplete;
        private List<string> TestRestartStageUsingWsman(IEnumerable<string> computerNames, List<string> nextTestList, CancellationToken token)
            var restartStageTestList = new List<string>();
            var operationOptions = new CimOperationOptions
                Timeout = TimeSpan.FromMilliseconds(2000),
                CancellationToken = token
            foreach (var computer in computerNames)
                    if (token.IsCancellationRequested)
                    using (CimSession cimSession = RemoteDiscoveryHelper.CreateCimSession(computer, Credential, WsmanAuthentication, isLocalHost: false, this, token))
                        bool itemRetrieved = false;
                        IEnumerable<CimInstance> mCollection = cimSession.QueryInstances(
                                                                 ComputerWMIHelper.CimOperatingSystemNamespace,
                                                                 ComputerWMIHelper.CimQueryDialect,
                                                                 "Select * from " + ComputerWMIHelper.WMI_Class_OperatingSystem,
                        foreach (CimInstance os in mCollection)
                            itemRetrieved = true;
                            string newLastBootUpTime = os.CimInstanceProperties["LastBootUpTime"].Value.ToString();
                            string oldLastBootUpTime = _computerInfos[computer].LastBootUpTime;
                            if (!string.Equals(newLastBootUpTime, oldLastBootUpTime, StringComparison.OrdinalIgnoreCase))
                                _computerInfos[computer].RebootComplete = true;
                                nextTestList.Add(computer);
                                restartStageTestList.Add(computer);
                        if (!itemRetrieved)
                catch (CimException)
            return restartStageTestList;
        private List<string> SetUpComputerInfoUsingWsman(IEnumerable<string> computerNames, CancellationToken token)
            var validComputerNameList = new List<string>();
                            if (!_computerInfos.ContainsKey(computer))
                                var info = new ComputerInfo
                                    LastBootUpTime = os.CimInstanceProperties["LastBootUpTime"].Value.ToString(),
                                    RebootComplete = false
                                _computerInfos.Add(computer, info);
                                validComputerNameList.Add(computer);
                            string errMsg = StringUtil.Format(ComputerResources.RestartComputerSkipped, computer, ComputerResources.CannotGetOperatingSystemObject);
                            var error = new ErrorRecord(new InvalidOperationException(errMsg), "RestartComputerSkipped",
                                                            ErrorCategory.OperationStopped, computer);
                            this.WriteError(error);
                catch (CimException ex)
                    string errMsg = StringUtil.Format(ComputerResources.RestartComputerSkipped, computer, ex.Message);
            return validComputerNameList;
        private void WriteOutTimeoutError(IEnumerable<string> computerNames)
            const string errorId = "RestartComputerTimeout";
            foreach (string computer in computerNames)
                string errorMsg = StringUtil.Format(ComputerResources.RestartcomputerFailed, computer, ComputerResources.TimeoutError);
                var exception = new RestartComputerTimeoutException(computer, Timeout, errorMsg, errorId);
                var error = new ErrorRecord(exception, errorId, ErrorCategory.OperationTimeout, computer);
                if (!InternalTestHooks.TestWaitStopComputer)
        #endregion "Private Methods"
        #region "Internal Methods"
        internal static List<string> TestWmiConnectionUsingWsman(List<string> computerNames, List<string> nextTestList, PSCredential credential, string wsmanAuthentication, PSCmdlet cmdlet, CancellationToken token)
            // Check if the WMI service "Winmgmt" is started
            const string wmiServiceQuery = "Select * from " + ComputerWMIHelper.WMI_Class_Service + " Where name = 'Winmgmt'";
            var wmiTestList = new List<string>();
                    using (CimSession cimSession = RemoteDiscoveryHelper.CreateCimSession(computer, credential, wsmanAuthentication, isLocalHost: false, cmdlet, token))
                                                                 wmiServiceQuery,
                        foreach (CimInstance service in mCollection)
                            if (LanguagePrimitives.IsTrue(service.CimInstanceProperties["Started"].Value))
                                wmiTestList.Add(computer);
            return wmiTestList;
        /// Test the PowerShell state for the restarting computer.
        /// <param name="nextTestList"></param>
        /// <param name="powershell"></param>
        internal static List<string> TestPowerShell(List<string> computerNames, List<string> nextTestList, System.Management.Automation.PowerShell powershell, PSCredential credential)
            List<string> psList = new();
                Collection<PSObject> psObjectCollection = powershell.Invoke(new object[] { credential, computerNames.ToArray() });
                if (psObjectCollection == null)
                    Dbg.Diagnostics.Assert(false, "This should never happen. Invoke should never return null.");
                // If ^C or timeout happens when we are in powershell.Invoke(), the psObjectCollection might be empty
                if (psObjectCollection.Count == 0)
                    return computerNames;
                object result = PSObject.Base(psObjectCollection[0]);
                Hashtable data = result as Hashtable;
                Dbg.Diagnostics.Assert(data != null, "data should never be null");
                Dbg.Diagnostics.Assert(data.Count == computerNames.Count, "data should contain results for all computers in computerNames");
                    if (LanguagePrimitives.IsTrue(data[computer]))
                        psList.Add(computer);
            catch (PipelineStoppedException)
                // powershell.Stop() is invoked because timeout expires, or Ctrl+C is pressed
                // powershell.dispose() is invoked because timeout expires, or Ctrl+C is pressed
            return psList;
        #endregion "Internal Methods"
        #region "Overrides"
            // Timeout, For, Delay, Progress cannot be present if Wait is not present
            if ((_timeoutSpecified || _waitForSpecified || _delaySpecified) && !Wait)
                InvalidOperationException ex = new(ComputerResources.RestartComputerInvalidParameter);
                ThrowTerminatingError(new ErrorRecord(ex, "RestartComputerInvalidParameter", ErrorCategory.InvalidOperation, null));
            if (Wait)
                _activityId = Random.Shared.Next();
                if (_timeout == -1 || _timeout >= int.MaxValue / 1000)
                    _timeoutInMilliseconds = int.MaxValue;
                    _timeoutInMilliseconds = _timeout * 1000;
                // We don't support combined service types for now
                switch (_waitFor)
                    case WaitForServiceTypes.Wmi:
                    case WaitForServiceTypes.WinRM:
                    case WaitForServiceTypes.PowerShell:
                        _powershell = System.Management.Automation.PowerShell.Create();
                        _powershell.AddScript(TestPowershellScript);
                        InvalidOperationException ex = new(ComputerResources.NoSupportForCombinedServiceType);
                        ErrorRecord error = new(ex, "NoSupportForCombinedServiceType", ErrorCategory.InvalidOperation, (int)_waitFor);
                        ThrowTerminatingError(error);
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
            // Validate parameters
            ValidateComputerNames();
            object[] flags = new object[] { 2, 0 };
            if (Force)
                flags[0] = forcedReboot;
            if (ParameterSetName.Equals(DefaultParameterSet, StringComparison.OrdinalIgnoreCase))
                if (Wait && _timeout != 0)
                    _validatedComputerNames =
                        SetUpComputerInfoUsingWsman(_validatedComputerNames, _cancel.Token);
                foreach (string computer in _validatedComputerNames)
                    bool isLocal = false;
                    string compname;
                    if (computer.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                        compname = _shortLocalMachineName;
                        isLocal = true;
                        compname = computer;
                    // Generate target and action strings
                    string action =
                        StringUtil.Format(
                            ComputerResources.RestartComputerAction,
                            isLocal ? ComputerResources.LocalShutdownPrivilege : ComputerResources.RemoteShutdownPrivilege);
                    string target =
                        isLocal ? StringUtil.Format(ComputerResources.DoubleComputerName, "localhost", compname) : compname;
                    if (!ShouldProcess(target, action))
                    bool isSuccess =
                        ComputerWMIHelper.InvokeWin32ShutdownUsingWsman(this, isLocal, compname, flags, Credential, WsmanAuthentication, ComputerResources.RestartcomputerFailed, "RestartcomputerFailed", _cancel.Token);
                    if (isSuccess && Wait && _timeout != 0)
                        _waitOnComputers.Add(computer);
                if (_waitOnComputers.Count > 0)
                    var restartStageTestList = new List<string>(_waitOnComputers);
                    var winrmTestList = new List<string>();
                    var psTestList = new List<string>();
                    var allDoneList = new List<string>();
                    bool isForWmi = _waitFor.Equals(WaitForServiceTypes.Wmi);
                    bool isForWinRm = _waitFor.Equals(WaitForServiceTypes.WinRM);
                    bool isForPowershell = _waitFor.Equals(WaitForServiceTypes.PowerShell);
                    int indicatorIndex = 0;
                    int machineCompleteRestart = 0;
                    int actualDelay = SecondsToWaitForRestartToBegin;
                    bool first = true;
                    bool waitComplete = false;
                    _percent = 0;
                    _status = ComputerResources.WaitForRestartToBegin;
                    _activity = _waitOnComputers.Count == 1 ?
                        StringUtil.Format(ComputerResources.RestartSingleComputerActivity, _waitOnComputers[0]) :
                        ComputerResources.RestartMultipleComputersActivity;
                    _timer = new Timer(OnTimedEvent, null, _timeoutInMilliseconds, System.Threading.Timeout.Infinite);
                        // (delay * 1000)/250ms
                        int loopCount = actualDelay * 4;
                        while (loopCount > 0)
                            WriteProgress(_indicator[(indicatorIndex++) % 4] + _activity, _status, _percent, ProgressRecordType.Processing);
                            loopCount--;
                            _waitHandler.Wait(250);
                            if (_exit)
                        if (first)
                            actualDelay = _delay;
                            first = false;
                            if (_waitOnComputers.Count > 1)
                                _status = StringUtil.Format(ComputerResources.WaitForMultipleComputers, machineCompleteRestart, _waitOnComputers.Count);
                            // Test restart stage.
                            // We check if the target machine has already rebooted by querying the LastBootUpTime from the Win32_OperatingSystem object.
                            // So after this step, we are sure that both the Network and the WMI or WinRM service have already come up.
                            if (restartStageTestList.Count > 0)
                                if (_waitOnComputers.Count == 1)
                                    _status = ComputerResources.VerifyRebootStage;
                                    _percent = CalculateProgressPercentage(StageVerification);
                                List<string> nextTestList = (isForWmi || isForPowershell) ? wmiTestList : winrmTestList;
                                restartStageTestList = TestRestartStageUsingWsman(restartStageTestList, nextTestList, _cancel.Token);
                            // Test WMI service
                            if (wmiTestList.Count > 0)
                                // This statement block executes for both CLRs.
                                // In the "full" CLR, it serves as the else case.
                                        _status = ComputerResources.WaitForWMI;
                                        _percent = CalculateProgressPercentage(WmiConnectionTest);
                                    wmiTestList = TestWmiConnectionUsingWsman(wmiTestList, winrmTestList, Credential, WsmanAuthentication, this, _cancel.Token);
                            if (isForWmi)
                            // Test WinRM service
                            if (winrmTestList.Count > 0)
                                    // CIM-WSMan in use. In this case, restart stage checking is done by using WMIv2,
                                    // so the WinRM service on the target machine is already up at this point.
                                    psTestList.AddRange(winrmTestList);
                                    winrmTestList.Clear();
                                        // This is to simulate the test for WinRM service
                                        _status = ComputerResources.WaitForWinRM;
                                        _percent = CalculateProgressPercentage(WinrmConnectionTest);
                                        loopCount = actualDelay * 4; // (delay * 1000)/250ms
                            if (isForWinRm)
                            // Test PowerShell
                            if (psTestList.Count > 0)
                                    _status = ComputerResources.WaitForPowerShell;
                                    _percent = CalculateProgressPercentage(PowerShellConnectionTest);
                                psTestList = TestPowerShell(psTestList, allDoneList, _powershell, this.Credential);
                        } while (false);
                        // if time is up or Ctrl+c is typed, break out
                        // Check if the restart completes
                                waitComplete = (winrmTestList.Count == _waitOnComputers.Count);
                                machineCompleteRestart = winrmTestList.Count;
                                waitComplete = (psTestList.Count == _waitOnComputers.Count);
                                machineCompleteRestart = psTestList.Count;
                                waitComplete = (allDoneList.Count == _waitOnComputers.Count);
                                machineCompleteRestart = allDoneList.Count;
                        // Wait is done or time is up
                        if (waitComplete || _exit)
                            if (waitComplete)
                                _status = ComputerResources.RestartComplete;
                                WriteProgress(_indicator[indicatorIndex % 4] + _activity, _status, 100, ProgressRecordType.Completed);
                                _timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                            _percent = machineCompleteRestart * 100 / _waitOnComputers.Count;
                    if (_timeUp)
                        // The timeout expires. Write out timeout error messages for the computers that haven't finished restarting
                                WriteOutTimeoutError(restartStageTestList);
                                WriteOutTimeoutError(wmiTestList);
                            // Wait for WMI. All computers that finished restarting are put in "winrmTestList"
                            // Wait for WinRM. All computers that finished restarting are put in "psTestList"
                                WriteOutTimeoutError(winrmTestList);
                                WriteOutTimeoutError(psTestList);
                            // Wait for PowerShell. All computers that finished restarting are put in "allDoneList"
        /// To implement ^C.
            _exit = true;
            _timer?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        #endregion "Overrides"
    #endregion Restart-Computer
    #region Stop-Computer
    /// Cmdlet to stop computer.
    [Cmdlet(VerbsLifecycle.Stop, "Computer", SupportsShouldProcess = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097151", RemotingCapability = RemotingCapability.SupportedByCommand)]
    public sealed class StopComputerCommand : PSCmdlet, IDisposable
        #region Private Members
        private const int forcedShutdown = 5; // See https://msdn.microsoft.com/library/aa394058(v=vs.85).aspx
        #region "Parameters"
        public string WsmanAuthentication { get; set; } = "Default";
        /// Value of the address requested. The form of the value can be either the
        /// computer name ("wxyz1234"), IPv4 address ("192.168.177.124"), or IPv6
        /// address ("2010:836B:4179::836B:4179").
        /// Force the operation to take place if possible.
        public SwitchParameter Force { get; set; } = false;
        #endregion "parameters"
            object[] flags = new object[] { 1, 0 };
            if (Force.IsPresent)
                flags[0] = forcedShutdown;
            ProcessWSManProtocol(flags);
            catch (ObjectDisposedException) { }
            catch (AggregateException) { }
        private void ProcessWSManProtocol(object[] flags)
            foreach (string computer in ComputerName)
                string compname = string.Empty;
                string strLocal = string.Empty;
                bool isLocalHost = false;
                if (_cancel.Token.IsCancellationRequested)
                if ((computer.Equals("localhost", StringComparison.OrdinalIgnoreCase)) || (computer.Equals(".", StringComparison.OrdinalIgnoreCase)))
                    compname = Dns.GetHostName();
                    strLocal = "localhost";
                    isLocalHost = true;
                if (!ShouldProcess(StringUtil.Format(ComputerResources.DoubleComputerName, strLocal, compname)))
                    ComputerWMIHelper.InvokeWin32ShutdownUsingWsman(
                        isLocalHost,
                        compname,
                        flags,
                        Credential,
                        WsmanAuthentication,
                        ComputerResources.StopcomputerFailed,
                        "StopComputerException",
                        _cancel.Token);
    #region Rename-Computer
    /// Renames a domain computer and its corresponding domain account or a
    /// workgroup computer. Use this command to rename domain workstations and local
    /// machines only. It cannot be used to rename Domain Controllers.
    [Cmdlet(VerbsCommon.Rename, "Computer", SupportsShouldProcess = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097054", RemotingCapability = RemotingCapability.SupportedByCommand)]
    [OutputType(typeof(RenameComputerChangeInfo))]
    public class RenameComputerCommand : PSCmdlet
        private bool _containsLocalHost = false;
        private string _newNameForLocalHost = null;
        /// Target computers to rename.
        public string ComputerName { get; set; } = "localhost";
        /// Emit the output.
        // [Alias("Restart")]
        /// The domain credential of the domain the target computer joined.
        public PSCredential DomainCredential { get; set; }
        /// The administrator credential of the target computer.
        public PSCredential LocalCredential { get; set; }
        /// New names for the target computers.
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string NewName { get; set; }
        /// Suppress the ShouldContinue.
        /// To restart the target computer after rename it.
        public SwitchParameter Restart
            get { return _restart; }
            set { _restart = value; }
        private bool _restart;
            "Kerberos")] // can be used with implicit or explicit credential
        /// Check to see if the target computer is the local machine.
        private string ValidateComputerName()
            // Validate target name.
            ErrorRecord targetError = null;
            string targetComputer = ComputerWMIHelper.ValidateComputerName(ComputerName, _shortLocalMachineName, _fullLocalMachineName, ref targetError);
            if (targetComputer == null)
                if (targetError != null)
                    WriteError(targetError);
            // Validate *new* name. Validate the format of the new name. Check if the old name is the same as the
            // new name later.
            if (!ComputerWMIHelper.IsComputerNameValid(NewName))
                bool isLocalhost = targetComputer.Equals(ComputerWMIHelper.localhostStr, StringComparison.OrdinalIgnoreCase);
                string errMsg = StringUtil.Format(ComputerResources.InvalidNewName, isLocalhost ? _shortLocalMachineName : targetComputer, NewName);
                ErrorRecord error = new(
                        new InvalidOperationException(errMsg), "InvalidNewName",
                        ErrorCategory.InvalidArgument, NewName);
            return targetComputer;
        private void DoRenameComputerAction(string computer, string newName, bool isLocalhost)
            string computerName = isLocalhost ? _shortLocalMachineName : computer;
            if (!ShouldProcess(computerName))
            // Check the length of the new name
            if (newName != null && newName.Length > ComputerWMIHelper.NetBIOSNameMaxLength)
                string truncatedName = newName.Substring(0, ComputerWMIHelper.NetBIOSNameMaxLength);
                string query = StringUtil.Format(ComputerResources.TruncateNetBIOSName, truncatedName);
                string caption = ComputerResources.TruncateNetBIOSNameCaption;
                if (!Force && !ShouldContinue(query, caption))
            DoRenameComputerWsman(computer, computerName, newName, isLocalhost);
        private void DoRenameComputerWsman(string computer, string computerName, string newName, bool isLocalhost)
            int retVal;
            PSCredential credToUse = isLocalhost ? null : (LocalCredential ?? DomainCredential);
                using (CancellationTokenSource cancelTokenSource = new())
                using (CimSession cimSession = RemoteDiscoveryHelper.CreateCimSession(computer, credToUse, WsmanAuthentication, isLocalhost, this, cancelTokenSource.Token))
                        Timeout = TimeSpan.FromMilliseconds(10000),
                        CancellationToken = cancelTokenSource.Token,
                        // This prefix works against all versions of the WinRM server stack, both win8 and win7
                        ResourceUriPrefix = new Uri(ComputerWMIHelper.CimUriPrefix)
                                                             "Select * from " + ComputerWMIHelper.WMI_Class_ComputerSystem,
                    foreach (CimInstance cimInstance in mCollection)
                        var oldName = cimInstance.CimInstanceProperties["DNSHostName"].Value.ToString();
                        if (oldName.Equals(newName, StringComparison.OrdinalIgnoreCase))
                            string errMsg = StringUtil.Format(ComputerResources.NewNameIsOldName, computerName, newName);
                                    new InvalidOperationException(errMsg), "NewNameIsOldName",
                                    ErrorCategory.InvalidArgument, newName);
                        // If the target computer is in a domain, always use the DomainCred. If the DomainCred is not given,
                        // use null for UserName and Password, so the context of the caller will be used.
                        // If the target computer is not in a domain, just use null for the UserName and Password
                        string dUserName = null;
                        string dPassword = null;
                        if (((bool)cimInstance.CimInstanceProperties["PartOfDomain"].Value) && (DomainCredential != null))
                            dUserName = DomainCredential.UserName;
                            dPassword = Utils.GetStringFromSecureString(DomainCredential.Password);
                        methodParameters.Add(CimMethodParameter.Create(
                            "Name",
                            newName,
                            Microsoft.Management.Infrastructure.CimType.String,
                            CimFlags.None));
                            "UserName",
                            dUserName,
                            (dUserName == null) ? CimFlags.NullValue : CimFlags.None));
                        methodParameters.Add(
                            CimMethodParameter.Create(
                            "Password",
                            dPassword,
                            (dPassword == null) ? CimFlags.NullValue : CimFlags.None));
                        if (!InternalTestHooks.TestRenameComputer)
                            CimMethodResult result = cimSession.InvokeMethod(
                                "Rename",
                            retVal = Convert.ToInt32(result.ReturnValue.Value, CultureInfo.CurrentCulture);
                            retVal = InternalTestHooks.TestRenameComputerResults;
                        if (retVal != 0)
                            var ex = new Win32Exception(retVal);
                            string errMsg = StringUtil.Format(ComputerResources.FailToRename, computerName, newName, ex.Message);
                            ErrorRecord error = new(new InvalidOperationException(errMsg), "FailToRenameComputer", ErrorCategory.OperationStopped, computerName);
                            successful = true;
                        if (PassThru)
                            WriteObject(ComputerWMIHelper.GetRenameComputerStatusObject(retVal, newName, computerName));
                        if (successful)
                            if (_restart)
                                // If successful and the Restart parameter is specified, restart the computer
                                object[] flags = new object[] { 6, 0 };
                                    isLocalhost,
                                    credToUse,
                                    ComputerResources.RestartcomputerFailed,
                                    "RestartcomputerFailed",
                                    cancelTokenSource.Token);
                                WriteWarning(StringUtil.Format(ComputerResources.RestartNeeded, null, computerName));
                string errMsg = StringUtil.Format(ComputerResources.FailToConnectToComputer, computerName, ex.Message);
                ErrorRecord error = new(new InvalidOperationException(errMsg), "RenameComputerException",
                                                    ErrorCategory.OperationStopped, computerName);
        #region "Override Methods"
            string targetComputer = ValidateComputerName();
            bool isLocalhost = targetComputer.Equals("localhost", StringComparison.OrdinalIgnoreCase);
            if (isLocalhost)
                if (!_containsLocalHost)
                    _containsLocalHost = true;
                _newNameForLocalHost = NewName;
            DoRenameComputerAction(targetComputer, NewName, false);
            DoRenameComputerAction("localhost", _newNameForLocalHost, true);
        #endregion "Override Methods"
    #endregion Rename-Computer
    #region "Public API"
    /// The object returned by SAM Computer cmdlets representing the status of the target machine.
    public sealed class ComputerChangeInfo
        private const string MatchFormat = "{0}:{1}";
        /// The HasSucceeded which shows the operation was success or not.
        public bool HasSucceeded { get; set; }
        /// The ComputerName on which the operation is done.
        public string ComputerName { get; set; }
        /// Returns the string representation of this object.
            return FormatLine(this.HasSucceeded.ToString(), this.ComputerName);
        /// Formats a line for use in ToString.
        /// <param name="HasSucceeded"></param>
        /// <param name="computername"></param>
        private static string FormatLine(string HasSucceeded, string computername)
            return StringUtil.Format(MatchFormat, HasSucceeded, computername);
    /// The object returned by Rename-Computer cmdlet representing the status of the target machine.
    public sealed class RenameComputerChangeInfo
        private const string MatchFormat = "{0}:{1}:{2}";
        /// The status which shows the operation was success or failure.
        /// The NewComputerName which represents the target machine.
        public string NewComputerName { get; set; }
        /// The OldComputerName which represented the target machine.
        public string OldComputerName { get; set; }
            return FormatLine(this.HasSucceeded.ToString(), this.NewComputerName, this.OldComputerName);
        /// <param name="newcomputername"></param>
        /// <param name="oldcomputername"></param>
        private static string FormatLine(string HasSucceeded, string newcomputername, string oldcomputername)
            return StringUtil.Format(MatchFormat, HasSucceeded, newcomputername, oldcomputername);
    #endregion "Public API"
    #region Helper
    /// Helper Class used by Stop-Computer,Restart-Computer and Test-Connection
    /// Also Contain constants used by System Restore related Cmdlets.
    internal static class ComputerWMIHelper
        /// The maximum length of a valid NetBIOS name.
        internal const int NetBIOSNameMaxLength = 15;
        /// System Restore Class used by Cmdlets.
        internal const string WMI_Class_SystemRestore = "SystemRestore";
        /// OperatingSystem WMI class used by Cmdlets.
        internal const string WMI_Class_OperatingSystem = "Win32_OperatingSystem";
        /// Service WMI class used by Cmdlets.
        internal const string WMI_Class_Service = "Win32_Service";
        /// Win32_ComputerSystem WMI class used by Cmdlets.
        internal const string WMI_Class_ComputerSystem = "Win32_ComputerSystem";
        /// Ping Class used by Cmdlet.
        internal const string WMI_Class_PingStatus = "Win32_PingStatus";
        /// CIMV2 path.
        internal const string WMI_Path_CIM = "\\root\\cimv2";
        /// Default path.
        internal const string WMI_Path_Default = "\\root\\default";
        /// The error says The interface is unknown.
        internal const int ErrorCode_Interface = 1717;
        /// This error says An instance of the service is already running.
        internal const int ErrorCode_Service = 1056;
        /// The name of the privilege to shutdown a local system.
        internal const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        /// The name of the privilege to shutdown a remote system.
        internal const string SE_REMOTE_SHUTDOWN_NAME = "SeRemoteShutdownPrivilege";
        /// CimUriPrefix.
        internal const string CimUriPrefix = "http://schemas.microsoft.com/wbem/wsman/1/wmi/root/cimv2";
        /// CimOperatingSystemNamespace.
        internal const string CimOperatingSystemNamespace = "root/cimv2";
        /// CimOperatingSystemShutdownMethod.
        internal const string CimOperatingSystemShutdownMethod = "Win32shutdown";
        /// CimQueryDialect.
        internal const string CimQueryDialect = "WQL";
        /// Local host name.
        internal const string localhostStr = "localhost";
        /// Get the local admin user name from a local NetworkCredential.
        /// <param name="psLocalCredential"></param>
        internal static string GetLocalAdminUserName(string computerName, PSCredential psLocalCredential)
            string localUserName = null;
            // The format of local admin username should be "ComputerName\AdminName"
            if (psLocalCredential.UserName.Contains('\\'))
                localUserName = psLocalCredential.UserName;
                int dotIndex = computerName.IndexOf('.');
                if (dotIndex == -1)
                    localUserName = computerName + "\\" + psLocalCredential.UserName;
                    localUserName = string.Concat(computerName.AsSpan(0, dotIndex), "\\", psLocalCredential.UserName);
            return localUserName;
        /// Generate a random password.
        /// <param name="passwordLength"></param>
        internal static string GetRandomPassword(int passwordLength)
            const int charMin = 32, charMax = 122;
            const int allowedCharsCount = charMax - charMin + 1;
            byte[] randomBytes = new byte[passwordLength];
            char[] chars = new char[passwordLength];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            for (int i = 0; i < passwordLength; i++)
                chars[i] = (char)(randomBytes[i] % allowedCharsCount + charMin);
            return new string(chars);
        /// Gets the Scope.
        /// <param name="computer"></param>
        /// <param name="namespaceParameter"></param>
        internal static string GetScopeString(string computer, string namespaceParameter)
            StringBuilder returnValue = new("\\\\");
            if (computer.Equals("::1", StringComparison.OrdinalIgnoreCase) || computer.Equals("[::1]", StringComparison.OrdinalIgnoreCase))
                returnValue.Append("localhost");
                returnValue.Append(computer);
            returnValue.Append(namespaceParameter);
        /// Returns true if it is a valid drive on the system.
        /// <param name="drive"></param>
        internal static bool IsValidDrive(string drive)
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo logicalDrive in drives)
                if (logicalDrive.DriveType.Equals(DriveType.Fixed))
                    if (drive.Equals(logicalDrive.Name, System.StringComparison.OrdinalIgnoreCase))
        /// Checks whether string[] contains System Drive.
        /// <param name="drives"></param>
        /// <param name="sysdrive"></param>
        internal static bool ContainsSystemDrive(string[] drives, string sysdrive)
            string driveApp;
            foreach (string drive in drives)
                if (!drive.EndsWith('\\'))
                    driveApp = string.Concat(drive, "\\");
                    driveApp = drive;
                if (driveApp.Equals(sysdrive, StringComparison.OrdinalIgnoreCase))
        /// Returns the given computernames in a string.
        internal static string GetMachineNames(string[] computerNames)
            string separator = ",";
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Control Panel\International");
            if (regKey != null)
                object sListValue = regKey.GetValue("sList");
                if (sListValue != null)
                    separator = sListValue.ToString();
            StringBuilder strComputers = new();
                    strComputers.Append(separator);
                strComputers.Append(compname);
            return strComputers.ToString();
        internal static ComputerChangeInfo GetComputerStatusObject(int errorcode, string computername)
            ComputerChangeInfo computerchangeinfo = new();
            computerchangeinfo.ComputerName = computername;
                computerchangeinfo.HasSucceeded = false;
                computerchangeinfo.HasSucceeded = true;
            return computerchangeinfo;
        internal static RenameComputerChangeInfo GetRenameComputerStatusObject(int errorcode, string newcomputername, string oldcomputername)
            RenameComputerChangeInfo renamecomputerchangeinfo = new();
            renamecomputerchangeinfo.OldComputerName = oldcomputername;
            renamecomputerchangeinfo.NewComputerName = newcomputername;
                renamecomputerchangeinfo.HasSucceeded = false;
                renamecomputerchangeinfo.HasSucceeded = true;
            return renamecomputerchangeinfo;
        internal static void WriteNonTerminatingError(int errorcode, PSCmdlet cmdlet, string computername)
            Win32Exception ex = new(errorcode);
            string additionalmessage = string.Empty;
            if (ex.NativeErrorCode.Equals(0x00000035))
                additionalmessage = StringUtil.Format(ComputerResources.NetworkPathNotFound, computername);
            string message = StringUtil.Format(ComputerResources.OperationFailed, ex.Message, computername, additionalmessage);
            ErrorRecord er = new(new InvalidOperationException(message), "InvalidOperationException", ErrorCategory.InvalidOperation, computername);
            cmdlet.WriteError(er);
        /// Check whether the new computer name is valid.
        internal static bool IsComputerNameValid(string computerName)
            bool hasAsciiLetterOrHyphen = false;
            if (computerName.Length >= 64)
            foreach (char t in computerName)
                if (char.IsAsciiLetter(t) || t is '-')
                    hasAsciiLetterOrHyphen = true;
                else if (!char.IsAsciiDigit(t))
            return hasAsciiLetterOrHyphen;
        /// Invokes the Win32Shutdown command on provided target computer using WSMan
        /// over a CIMSession.  The flags parameter determines the type of shutdown operation
        /// such as shutdown, reboot, force etc.
        /// <param name="cmdlet">Cmdlet host for reporting errors.</param>
        /// <param name="isLocalhost">True if local host computer.</param>
        /// <param name="computerName">Target computer.</param>
        /// <param name="flags">Win32Shutdown flags.</param>
        /// <param name="credential">Optional credential.</param>
        /// <param name="authentication">Optional authentication.</param>
        /// <param name="formatErrorMessage">Error message format string that takes two parameters.</param>
        /// <param name="ErrorFQEID">Fully qualified error Id.</param>
        /// <param name="cancelToken">Cancel token.</param>
        /// <returns>True on success.</returns>
        internal static bool InvokeWin32ShutdownUsingWsman(
            PSCmdlet cmdlet,
            bool isLocalhost,
            object[] flags,
            PSCredential credential,
            string authentication,
            string formatErrorMessage,
            string ErrorFQEID,
            CancellationToken cancelToken)
            Dbg.Diagnostics.Assert(flags.Length == 2, "Caller need to verify the flags passed in");
            bool isSuccess = false;
            string targetMachine = isLocalhost ? "localhost" : computerName;
            string authInUse = isLocalhost ? null : authentication;
            PSCredential credInUse = isLocalhost ? null : credential;
            var currentPrivilegeState = new PlatformInvokes.TOKEN_PRIVILEGE();
                CancellationToken = cancelToken,
                if (!(isLocalhost && PlatformInvokes.EnableTokenPrivilege(ComputerWMIHelper.SE_SHUTDOWN_NAME, ref currentPrivilegeState)) &&
                    !(!isLocalhost && PlatformInvokes.EnableTokenPrivilege(ComputerWMIHelper.SE_REMOTE_SHUTDOWN_NAME, ref currentPrivilegeState)))
                    string message =
                        StringUtil.Format(ComputerResources.PrivilegeNotEnabled, computerName,
                            isLocalhost ? ComputerWMIHelper.SE_SHUTDOWN_NAME : ComputerWMIHelper.SE_REMOTE_SHUTDOWN_NAME);
                    ErrorRecord errorRecord = new(new InvalidOperationException(message), "PrivilegeNotEnabled", ErrorCategory.InvalidOperation, null);
                using (CimSession cimSession = RemoteDiscoveryHelper.CreateCimSession(targetMachine, credInUse, authInUse, isLocalhost, cmdlet, cancelToken))
                        "Flags",
                        flags[0],
                        Microsoft.Management.Infrastructure.CimType.SInt32,
                        "Reserved",
                        flags[1],
                    if (!InternalTestHooks.TestStopComputer)
                        CimMethodResult result = null;
                            // Win32_ComputerSystem is a singleton hence FirstOrDefault() return the only instance returned by EnumerateInstances.
                            var computerSystem = cimSession.EnumerateInstances(ComputerWMIHelper.CimOperatingSystemNamespace, ComputerWMIHelper.WMI_Class_OperatingSystem).FirstOrDefault();
                            result = cimSession.InvokeMethod(
                                computerSystem,
                                ComputerWMIHelper.CimOperatingSystemShutdownMethod,
                                ComputerWMIHelper.WMI_Class_OperatingSystem,
                        retVal = InternalTestHooks.TestStopComputerResults;
                        string errMsg = StringUtil.Format(formatErrorMessage, computerName, ex.Message);
                            new InvalidOperationException(errMsg), ErrorFQEID, ErrorCategory.OperationStopped, computerName);
                        cmdlet.WriteError(error);
                        isSuccess = true;
                ErrorRecord error = new(new InvalidOperationException(errMsg), ErrorFQEID,
                // Restore the previous privilege state if something unexpected happened
                PlatformInvokes.RestoreTokenPrivilege(
                    isLocalhost ? ComputerWMIHelper.SE_SHUTDOWN_NAME : ComputerWMIHelper.SE_REMOTE_SHUTDOWN_NAME, ref currentPrivilegeState);
            return isSuccess;
        /// Returns valid computer name or null on failure.
        /// <param name="nameToCheck">Computer name to validate.</param>
        /// <param name="shortLocalMachineName"></param>
        /// <param name="fullLocalMachineName"></param>
        /// <returns>Valid computer name.</returns>
        internal static string ValidateComputerName(
            string nameToCheck,
            string shortLocalMachineName,
            string fullLocalMachineName,
            ref ErrorRecord error)
            string validatedComputerName = null;
            if (nameToCheck.Equals(".", StringComparison.OrdinalIgnoreCase) ||
                nameToCheck.Equals(localhostStr, StringComparison.OrdinalIgnoreCase) ||
                nameToCheck.Equals(shortLocalMachineName, StringComparison.OrdinalIgnoreCase) ||
                nameToCheck.Equals(fullLocalMachineName, StringComparison.OrdinalIgnoreCase))
                validatedComputerName = localhostStr;
                bool isIPAddress = false;
                    isIPAddress = IPAddress.TryParse(nameToCheck, out _);
                    string fqcn = Dns.GetHostEntryAsync(nameToCheck).Result.HostName;
                    if (fqcn.Equals(shortLocalMachineName, StringComparison.OrdinalIgnoreCase) ||
                        fqcn.Equals(fullLocalMachineName, StringComparison.OrdinalIgnoreCase))
                        // The IPv4 or IPv6 of the local machine is specified
                        validatedComputerName = nameToCheck;
                    // If GetHostEntry() throw exception, then the target should not be the local machine
                    if (!isIPAddress)
                        // Return error if the computer name is not an IP address. Dns.GetHostEntry() may not work on IP addresses.
                        string errMsg = StringUtil.Format(ComputerResources.CannotResolveComputerName, nameToCheck, e.Message);
                        error = new ErrorRecord(
                            new InvalidOperationException(errMsg), "AddressResolutionException",
                            ErrorCategory.InvalidArgument, nameToCheck);
            return validatedComputerName;
    #endregion Helper

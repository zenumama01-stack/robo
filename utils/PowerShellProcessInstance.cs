    /// This class represents a PowerShell process that is used for an out-of-process remote Runspace.
    public sealed class PowerShellProcessInstance : IDisposable
        private readonly ProcessStartInfo _startInfo;
        private RunspacePool _runspacePool;
        private bool _started;
        private bool _processExited;
        internal static readonly string PwshExePath;
        internal static readonly string WinPwshExePath;
        static PowerShellProcessInstance()
            PwshExePath = Path.Combine(Utils.DefaultPowerShellAppBase, "pwsh");
            PwshExePath = Path.Combine(Utils.DefaultPowerShellAppBase, "pwsh.exe");
            var winPowerShellDir = Utils.GetApplicationBaseFromRegistry(Utils.DefaultPowerShellShellID);
            WinPwshExePath = string.IsNullOrEmpty(winPowerShellDir) ? null : Path.Combine(winPowerShellDir, "powershell.exe");
        /// Initializes a new instance of the <see cref="PowerShellProcessInstance"/> class. Initializes the underlying dotnet process class.
        /// <param name="powerShellVersion">Specifies the version of powershell.</param>
        /// <param name="credential">Specifies a user account credentials.</param>
        /// <param name="initializationScript">Specifies a script that will be executed when the powershell process is initialized.</param>
        /// <param name="useWow64">Specifies if the powershell process will be 32-bit.</param>
        /// <param name="workingDirectory">Specifies the initial working directory for the new powershell process.</param>
        public PowerShellProcessInstance(Version powerShellVersion, PSCredential credential, ScriptBlock initializationScript, bool useWow64, string workingDirectory)
            string exePath = PwshExePath;
            bool startingWindowsPowerShell51 = false;
            // if requested PS version was "5.1" then we start Windows PS instead of PS Core
            startingWindowsPowerShell51 = (powerShellVersion != null) && (powerShellVersion.Major == 5) && (powerShellVersion.Minor == 1);
            if (startingWindowsPowerShell51)
                if (WinPwshExePath == null)
                    throw new PSInvalidOperationException(RemotingErrorIdStrings.WindowsPowerShellNotPresent);
                exePath = WinPwshExePath;
                if (useWow64)
                    string procArch = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
                    if ((!string.IsNullOrEmpty(procArch)) && (procArch.Equals("amd64", StringComparison.OrdinalIgnoreCase) ||
                        procArch.Equals("ia64", StringComparison.OrdinalIgnoreCase)))
                        exePath = WinPwshExePath.ToLowerInvariant().Replace("\\system32\\", "\\syswow64\\");
                        if (!File.Exists(exePath))
                            string message = PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.WowComponentNotPresent, exePath);
            // 'WindowStyle' is used only if 'UseShellExecute' is 'true'. Since 'UseShellExecute' is set
            // to 'false' in our use, we can ignore the 'WindowStyle' setting in the initialization below.
            _startInfo = new ProcessStartInfo
                FileName = exePath,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                LoadUserProfile = true,
                _startInfo.ArgumentList.Add("-Version");
                _startInfo.ArgumentList.Add("5.1");
                _startInfo.Environment["PSModulePath"] = ModuleIntrinsics.GetWindowsPowerShellModulePath();
            _startInfo.ArgumentList.Add("-s");
            _startInfo.ArgumentList.Add("-NoLogo");
            _startInfo.ArgumentList.Add("-NoProfile");
            if (!string.IsNullOrWhiteSpace(workingDirectory) && !startingWindowsPowerShell51)
                _startInfo.ArgumentList.Add("-wd");
                _startInfo.ArgumentList.Add(workingDirectory);
            if (initializationScript != null)
                var scriptBlockString = initializationScript.ToString();
                if (!string.IsNullOrEmpty(scriptBlockString))
                    var encodedCommand = Convert.ToBase64String(Encoding.Unicode.GetBytes(scriptBlockString));
                    _startInfo.ArgumentList.Add("-EncodedCommand");
                    _startInfo.ArgumentList.Add(encodedCommand);
                Net.NetworkCredential netCredential = credential.GetNetworkCredential();
                _startInfo.UserName = netCredential.UserName;
                _startInfo.Domain = string.IsNullOrEmpty(netCredential.Domain) ? "." : netCredential.Domain;
                _startInfo.Password = credential.Password;
            Process = new Process { StartInfo = _startInfo, EnableRaisingEvents = true };
        public PowerShellProcessInstance(Version powerShellVersion, PSCredential credential, ScriptBlock initializationScript, bool useWow64) : this(powerShellVersion, credential, initializationScript, useWow64, workingDirectory: null)
        /// Initializes a new instance of the <see cref="PowerShellProcessInstance"/> class. Default initializes the underlying dotnet process class.
        public PowerShellProcessInstance() : this(powerShellVersion: null, credential: null, initializationScript: null, useWow64: false, workingDirectory: null)
        /// Gets a value indicating whether the associated process has been terminated.
        /// true if the operating system process referenced by the Process component has terminated; otherwise, false.
        public bool HasExited
                // When process is exited, there is some delay in receiving ProcessExited event and HasExited property on process object.
                // Using HasExited property on started process object to determine if powershell process has exited.
                return _processExited || (_started && Process != null && Process.HasExited);
                if (Process != null && !Process.HasExited)
                    Process.Kill();
        #endregion Dispose
        /// Gets the process object of the remote target.
        public Process Process { get; }
        internal RunspacePool RunspacePool
        internal OutOfProcessTextWriter StdInWriter { get; set; }
            // To fix the deadlock, we should not call Process.HasExited by holding the sync lock as Process.HasExited can raise ProcessExited event
            if (HasExited)
                if (_started)
                _started = true;
                Process.Exited += ProcessExited;
            Process.Start();
        private void ProcessExited(object sender, EventArgs e)
                _processExited = true;

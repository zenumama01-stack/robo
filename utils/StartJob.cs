    /// This cmdlet start invocation of jobs in background.
    [Cmdlet(VerbsLifecycle.Start, "Job", DefaultParameterSetName = StartJobCommand.ComputerNameParameterSet, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096796")]
    [OutputType(typeof(PSRemotingJob))]
    public class StartJobCommand : PSExecutionCmdlet, IDisposable
        private static readonly string s_startJobType = "BackgroundJob";
        private const string DefinitionNameParameterSet = "DefinitionName";
        /// JobDefinition Name.
                   ParameterSetName = StartJobCommand.DefinitionNameParameterSet)]
        public string DefinitionName
            get { return _definitionName; }
            set { _definitionName = value; }
        private string _definitionName;
        /// JobDefinition file path.
        public string DefinitionPath
            get { return _definitionPath; }
            set { _definitionPath = value; }
        private string _definitionPath;
        /// Job SourceAdapter type for this job definition.
            get { return _definitionType; }
            set { _definitionType = value; }
        private string _definitionType;
        /// Friendly name for this job object.
                   ParameterSetName = StartJobCommand.FilePathComputerNameParameterSet)]
                   ParameterSetName = StartJobCommand.ComputerNameParameterSet)]
                   ParameterSetName = StartJobCommand.LiteralFilePathComputerNameParameterSet)]
        public virtual string Name
        /// <remarks>This is used in the in process case with a
        /// "ValueFromPipelineProperty" enabled in order to maintain
        /// compatibility with v1.0</remarks>
        #region Suppress PSRemotingBaseCmdlet parameters
        // suppress all the parameters from PSRemotingBaseCmdlet
        // which should not be part of Start-PSJob
        /// Not used for OutOfProc jobs.  Suppressing this parameter.
        /// Suppress SSHTransport.
        /// Suppress SSHConnection.
        /// Suppress UserName.
        /// Suppress KeyFilePath.
        /// Suppress HostName.
        /// Suppress Subsystem.
        /// Credential to use for this job.
        [Parameter(ParameterSetName = StartJobCommand.FilePathComputerNameParameterSet)]
        [Parameter(ParameterSetName = StartJobCommand.ComputerNameParameterSet)]
        [Parameter(ParameterSetName = StartJobCommand.LiteralFilePathComputerNameParameterSet)]
        public override Int32 ThrottleLimit
        /// Filepath to execute as a script.
        /// Literal Filepath to execute as a script.
                base.IsLiteralPath = true;
        public override string CertificateThumbprint
                return base.CertificateThumbprint;
                base.CertificateThumbprint = value;
        /// This is not declared as a Parameter for Start-PSJob as this is not
        /// used for background jobs.
        /// Script that is used to initialize the background job.
        public virtual ScriptBlock InitializationScript
            get { return _initScript; }
            set { _initScript = value; }
        private ScriptBlock _initScript;
        /// Gets or sets an initial working directory for the powershell background job.
        [ValidateNotNullOrWhiteSpace]
        /// Launches the background job as a 32-bit process. This can be used on
        /// 64-bit systems to launch a 32-bit wow process for the background job.
        public virtual SwitchParameter RunAs32 { get; set; }
        /// Powershell Version to execute the background job.
        public virtual Version PSVersion
                return _psVersion;
                // PSVersion value can only be 5.1 for Start-Job.
                if (!(value.Major == 5 && value.Minor == 1))
                        StringUtil.Format(RemotingErrorIdStrings.PSVersionParameterOutOfRange, value, "PSVersion"));
                _psVersion = value;
        private Version _psVersion;
        /// InputObject.
        public override PSObject InputObject
            get { return base.InputObject; }
            set { base.InputObject = value; }
        /// ArgumentList.
        public override object[] ArgumentList
            get { return base.ArgumentList; }
            set { base.ArgumentList = value; }
        /// 1. Set the throttling limit and reset operations complete
        /// 2. Create helper objects
        /// 3. For async case, write the async result object down the
        ///    pipeline.
            if (!File.Exists(PowerShellProcessInstance.PwshExePath))
                // The pwsh executable file is not found under $PSHOME.
                // This means that PowerShell is currently being hosted in another application,
                // and 'Start-Job' is not supported by design in that scenario.
                    RemotingErrorIdStrings.IPCPwshExecutableNotFound,
                    PowerShellProcessInstance.PwshExePath);
                    new PSNotSupportedException(message),
                    "IPCPwshExecutableNotFound",
            if (RunAs32.IsPresent && Environment.Is64BitProcess)
                // We cannot start a 32-bit 'pwsh' process from a 64-bit 'pwsh' installation.
                string message = RemotingErrorIdStrings.RunAs32NotSupported;
                    "RunAs32NotSupported",
            if (WorkingDirectory != null && !InvokeProvider.Item.IsContainer(WorkingDirectory))
                string message = StringUtil.Format(RemotingErrorIdStrings.StartJobWorkingDirectoryNotFound, WorkingDirectory);
                    new DirectoryNotFoundException(message),
                    "DirectoryNotFoundException",
            if (WorkingDirectory == null)
                    WorkingDirectory = SessionState.Internal.CurrentLocation.Path;
            if (ParameterSetName == DefinitionNameParameterSet)
            // since jobs no more depend on WinRM
            // we will have to skip the check for the same
        /// Create a throttle operation using NewProcessConnectionInfo
        /// ie., Out-Of-Process runspace.
        protected override void CreateHelpersForSpecifiedComputerNames()
            // If we're in ConstrainedLanguage mode and the system is in lockdown mode,
            // ensure that they haven't specified a ScriptBlock or InitScript - as
            // we can't protect that boundary.
                (SystemPolicy.GetSystemLockdownPolicy() != SystemEnforcementMode.Enforce) &&
                ((ScriptBlock != null) || (InitializationScript != null)))
                        new PSNotSupportedException(RemotingErrorIdStrings.CannotStartJobInconsistentLanguageMode),
                            "CannotStartJobInconsistentLanguageMode",
            NewProcessConnectionInfo connectionInfo = new NewProcessConnectionInfo(this.Credential);
            connectionInfo.InitializationScript = _initScript;
            connectionInfo.AuthenticationMechanism = this.Authentication;
            connectionInfo.PSVersion = this.PSVersion;
            connectionInfo.WorkingDirectory = this.WorkingDirectory;
            RemoteRunspace remoteRunspace = (RemoteRunspace)RunspaceFactory.CreateRunspace(connectionInfo, this.Host,
                        Utils.GetTypeTableFromExecutionContextTLS());
                new ExecutionCmdletHelperComputerName(remoteRunspace, pipeline);
        /// remote runspace parameter or computer name is specified. If
        /// none other than command parameter is specified, then it
        /// just executes the command locally without creating a new
        /// remote runspace object.
                // Get the Job2 object from the Job Manager for this definition name and start the job.
                string resolvedPath = null;
                if (!string.IsNullOrEmpty(_definitionPath))
                    System.Collections.ObjectModel.Collection<string> paths =
                        this.Context.SessionState.Path.GetResolvedProviderPathFromPSPath(_definitionPath, out provider);
                    // Only file system paths are allowed.
                        string message = StringUtil.Format(RemotingErrorIdStrings.StartJobDefinitionPathInvalidNotFSProvider,
                            _definitionName,
                            _definitionPath,
                        WriteError(new ErrorRecord(new RuntimeException(message), "StartJobFromDefinitionNamePathInvalidNotFileSystemProvider",
                    // Only a single file path is allowed.
                        string message = StringUtil.Format(RemotingErrorIdStrings.StartJobDefinitionPathInvalidNotSingle,
                            _definitionPath);
                        WriteError(new ErrorRecord(new RuntimeException(message), "StartJobFromDefinitionNamePathInvalidNotSingle",
                    resolvedPath = paths[0];
                List<Job2> jobs = JobManager.GetJobToStart(_definitionName, resolvedPath, _definitionType, this, false);
                if (jobs.Count == 0)
                    string message = (_definitionType != null) ?
                        StringUtil.Format(RemotingErrorIdStrings.StartJobDefinitionNotFound2, _definitionType, _definitionName) :
                        StringUtil.Format(RemotingErrorIdStrings.StartJobDefinitionNotFound1, _definitionName);
                    WriteError(new ErrorRecord(new RuntimeException(message), "StartJobFromDefinitionNameNotFound",
                if (jobs.Count > 1)
                    string message = StringUtil.Format(RemotingErrorIdStrings.StartJobManyDefNameMatches, _definitionName);
                    WriteError(new ErrorRecord(new RuntimeException(message), "StartJobFromDefinitionNameMoreThanOneMatch",
                        ErrorCategory.InvalidResult, null));
                // Start job.
                Job2 job = jobs[0];
                job.StartJob();
                // Write job object to host.
            if (_firstProcessRecord)
                _firstProcessRecord = false;
                job.PSJobTypeName = s_startJobType;
            // inject input
                    helper.Pipeline.Input.Write(InputObject);
        private bool _firstProcessRecord = true;

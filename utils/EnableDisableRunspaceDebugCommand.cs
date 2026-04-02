    #region PSRunspaceDebug class
    /// Runspace Debug Options class.
    public sealed class PSRunspaceDebug
        /// When true this property will cause any breakpoints set in a Runspace to stop
        /// the running command or script when the breakpoint is hit, regardless of whether a
        /// debugger is currently attached.  The script or command will remain stopped until
        /// a debugger is attached to debug the breakpoint.
        public bool Enabled { get; }
        /// When true this property will cause any running command or script in the Runspace
        /// to stop in step mode, regardless of whether a debugger is currently attached.  The
        /// script or command will remain stopped until a debugger is attached to debug the
        /// current stop point.
        public bool BreakAll { get; }
        /// Name of runspace for which the options apply.
        public string RunspaceName { get; }
        /// Local Id of runspace for which the options apply.
        public int RunspaceId { get; }
        /// Initializes a new instance of the <see cref="PSRunspaceDebug"/> class.
        /// <param name="enabled">Enable debugger option.</param>
        /// <param name="breakAll">BreakAll option.</param>
        /// <param name="runspaceName">Runspace name.</param>
        /// <param name="runspaceId">Runspace local Id.</param>
        public PSRunspaceDebug(bool enabled, bool breakAll, string runspaceName, int runspaceId)
            if (string.IsNullOrEmpty(runspaceName))
                throw new PSArgumentNullException(nameof(runspaceName));
            this.Enabled = enabled;
            this.BreakAll = breakAll;
            this.RunspaceName = runspaceName;
            this.RunspaceId = runspaceId;
    #region CommonRunspaceCommandBase class
    /// Abstract class that defines common Runspace Command parameters.
    public abstract class CommonRunspaceCommandBase : PSCmdlet
        /// RunspaceParameterSet.
        protected const string RunspaceParameterSet = "RunspaceParameterSet";
        /// RunspaceNameParameterSet.
        protected const string RunspaceNameParameterSet = "RunspaceNameParameterSet";
        /// RunspaceIdParameterSet.
        protected const string RunspaceIdParameterSet = "RunspaceIdParameterSet";
        /// RunspaceInstanceIdParameterSet.
        protected const string RunspaceInstanceIdParameterSet = "RunspaceInstanceIdParameterSet";
        /// ProcessNameParameterSet.
        protected const string ProcessNameParameterSet = "ProcessNameParameterSet";
        /// Runspace Name.
                   ParameterSetName = CommonRunspaceCommandBase.RunspaceNameParameterSet)]
        public string[] RunspaceName
        /// Runspace.
                   ParameterSetName = CommonRunspaceCommandBase.RunspaceParameterSet)]
        public Runspace[] Runspace
        /// Runspace Id.
                   ParameterSetName = CommonRunspaceCommandBase.RunspaceIdParameterSet)]
        public int[] RunspaceId
        /// RunspaceInstanceId.
                   ParameterSetName = CommonRunspaceCommandBase.RunspaceInstanceIdParameterSet)]
        public System.Guid[] RunspaceInstanceId
        /// Gets or Sets the ProcessName for which runspace debugging has to be enabled or disabled.
        [Parameter(Position = 0, ParameterSetName = CommonRunspaceCommandBase.ProcessNameParameterSet)]
        /// Gets or Sets the AppDomain Names for which runspace debugging has to be enabled or disabled.
        [Parameter(Position = 1, ParameterSetName = CommonRunspaceCommandBase.ProcessNameParameterSet)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope = "member",
            Target = "Microsoft.PowerShell.Commands.CommonRunspaceCommandBase.#AppDomainName")]
        public string[] AppDomainName
        /// Returns a list of valid runspaces based on current parameter set.
        /// <returns>IReadOnlyList.</returns>
        protected IReadOnlyList<Runspace> GetRunspaces()
            IReadOnlyList<Runspace> results = null;
            if ((ParameterSetName == CommonRunspaceCommandBase.RunspaceNameParameterSet) && ((RunspaceName == null) || RunspaceName.Length == 0))
                results = GetRunspaceUtils.GetAllRunspaces();
                    case CommonRunspaceCommandBase.RunspaceNameParameterSet:
                        results = GetRunspaceUtils.GetRunspacesByName(RunspaceName);
                    case CommonRunspaceCommandBase.RunspaceIdParameterSet:
                        results = GetRunspaceUtils.GetRunspacesById(RunspaceId);
                    case CommonRunspaceCommandBase.RunspaceParameterSet:
                        results = new ReadOnlyCollection<Runspace>(new List<Runspace>(Runspace));
                    case CommonRunspaceCommandBase.RunspaceInstanceIdParameterSet:
                        results = GetRunspaceUtils.GetRunspacesByInstanceId(RunspaceInstanceId);
        /// Returns Runspace Debugger.
        /// <param name="runspace">Runspace.</param>
        /// <returns>Debugger.</returns>
        protected System.Management.Automation.Debugger GetDebuggerFromRunspace(Runspace runspace)
            System.Management.Automation.Debugger debugger = null;
                debugger = runspace.Debugger;
            if (debugger == null)
                        new PSInvalidOperationException(string.Format(CultureInfo.InvariantCulture, Debugger.RunspaceOptionNoDebugger, runspace.Name)),
                        "RunspaceDebugOptionNoDebugger",
            return debugger;
        /// SetDebugPreferenceHelper is a helper method used to enable/disable debug preference.
        /// <param name="processName">Process Name.</param>
        /// <param name="appDomainName">App Domain Name.</param>
        /// <param name="enable">Indicates if debug preference has to be enabled or disabled.</param>
        /// <param name="fullyQualifiedErrorId">FullyQualifiedErrorId to be used on error.</param>
        protected void SetDebugPreferenceHelper(string processName, string[] appDomainName, bool enable, string fullyQualifiedErrorId)
            List<string> appDomainNames = null;
            if (appDomainName != null)
                foreach (string currentAppDomainName in appDomainName)
                    if (!string.IsNullOrEmpty(currentAppDomainName))
                        appDomainNames ??= new List<string>();
                        appDomainNames.Add(currentAppDomainName.ToLowerInvariant());
                System.Management.Automation.Runspaces.LocalRunspace.SetDebugPreference(processName.ToLowerInvariant(), appDomainNames, enable);
                new PSInvalidOperationException(string.Format(CultureInfo.InvariantCulture, Debugger.PersistDebugPreferenceFailure, processName), ex),
                fullyQualifiedErrorId,
                this);
    #region EnableRunspaceDebugCommand Cmdlet
    /// This cmdlet enables debugging for selected runspaces in the current or specified process.
    [Cmdlet(VerbsLifecycle.Enable, "RunspaceDebug", DefaultParameterSetName = CommonRunspaceCommandBase.RunspaceNameParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2096831")]
    public sealed class EnableRunspaceDebugCommand : CommonRunspaceCommandBase
        [Parameter(Position = 1,
        public SwitchParameter BreakAll
        /// Process Record.
            if (this.ParameterSetName.Equals(CommonRunspaceCommandBase.ProcessNameParameterSet))
                SetDebugPreferenceHelper(ProcessName, AppDomainName, true, "EnableRunspaceDebugCommandPersistDebugPreferenceFailure");
            IReadOnlyList<Runspace> results = GetRunspaces();
            foreach (var runspace in results)
                if (runspace.RunspaceStateInfo.State != RunspaceState.Opened)
                        new ErrorRecord(new PSInvalidOperationException(string.Format(CultureInfo.InvariantCulture, Debugger.RunspaceOptionInvalidRunspaceState, runspace.Name)),
                        "SetRunspaceDebugOptionCommandInvalidRunspaceState",
                        this));
                System.Management.Automation.Debugger debugger = GetDebuggerFromRunspace(runspace);
                // Enable debugging by preserving debug stop events.
                debugger.UnhandledBreakpointMode = UnhandledBreakpointProcessingMode.Wait;
                if (this.MyInvocation.BoundParameters.ContainsKey(nameof(BreakAll)))
                    if (BreakAll)
                            debugger.SetDebuggerStepMode(true);
                        catch (PSInvalidOperationException e)
                                "SetRunspaceDebugOptionCommandCannotEnableDebuggerStepping",
                        debugger.SetDebuggerStepMode(false);
    #region DisableRunspaceDebugCommand Cmdlet
    /// This cmdlet disables Runspace debugging in selected Runspaces.
    [Cmdlet(VerbsLifecycle.Disable, "RunspaceDebug", DefaultParameterSetName = CommonRunspaceCommandBase.RunspaceNameParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2096924")]
    public sealed class DisableRunspaceDebugCommand : CommonRunspaceCommandBase
                SetDebugPreferenceHelper(ProcessName.ToLowerInvariant(), AppDomainName, false, "DisableRunspaceDebugCommandPersistDebugPreferenceFailure");
                                new PSInvalidOperationException(string.Format(CultureInfo.InvariantCulture, Debugger.RunspaceOptionInvalidRunspaceState, runspace.Name)),
                    debugger.UnhandledBreakpointMode = UnhandledBreakpointProcessingMode.Ignore;
    #region GetRunspaceDebugCommand Cmdlet
    /// This cmdlet returns a PSRunspaceDebug object for each found Runspace object.
    [Cmdlet(VerbsCommon.Get, "RunspaceDebug", DefaultParameterSetName = CommonRunspaceCommandBase.RunspaceNameParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2097015")]
    [OutputType(typeof(PSRunspaceDebug))]
    public sealed class GetRunspaceDebugCommand : CommonRunspaceCommandBase
                if (debugger != null)
                        new PSRunspaceDebug((debugger.UnhandledBreakpointMode == UnhandledBreakpointProcessingMode.Wait),
                            debugger.IsDebuggerSteppingEnabled,
                            runspace.Name,
                            runspace.Id)
    #region WaitDebuggerCommand Cmdlet
    /// This cmdlet causes a running script or command to stop in the debugger at the next execution point.
    [Cmdlet(VerbsLifecycle.Wait, "Debugger",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2097035")]
    public sealed class WaitDebuggerCommand : PSCmdlet
            Runspace currentRunspace = this.Context.CurrentRunspace;
            if (currentRunspace != null && currentRunspace.Debugger != null)
                WriteVerbose(string.Format(CultureInfo.InvariantCulture, Debugger.DebugBreakMessage, MyInvocation.ScriptLineNumber, MyInvocation.ScriptName));
                currentRunspace.Debugger.Break();

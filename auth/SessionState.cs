    /// Holds the state of a Monad Shell session.
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is a bridge class between internal classes and a public interface. It requires this much coupling.")]
    internal sealed partial class SessionStateInternal
             "SessionState",
             "SessionState Class")]
            Dbg.PSTraceSource.GetTracer("SessionState",
             "SessionState Class");
        /// Constructor for session state object.
        /// The context for the runspace to which this session state object belongs.
        /// if <paramref name="context"/> is null.
        internal SessionStateInternal(ExecutionContext context) : this(null, false, context)
        internal SessionStateInternal(SessionStateInternal parent, bool linkToGlobal, ExecutionContext context)
            ExecutionContext = context;
            // Create the working directory stack. This
            // is used for the pushd and popd commands
            _workingLocationStack = new Dictionary<string, Stack<PathInfo>>(StringComparer.OrdinalIgnoreCase);
            // Conservative choice to limit the Set-Location history in order to limit memory impact in case of a regression.
            const int locationHistoryLimit = 20;
            _setLocationHistory = new HistoryStack<PathInfo>(locationHistoryLimit);
            GlobalScope = new SessionStateScope(null);
            ModuleScope = GlobalScope;
            _currentScope = GlobalScope;
            InitializeSessionStateInternalSpecialVariables(false);
            // Create the push the global scope on as
            // the starting script scope.  That way, if you dot-source a script
            // that uses variables qualified by script: it works.
            GlobalScope.ScriptScope = GlobalScope;
                GlobalScope.Parent = parent.GlobalScope;
                // Copy the drives and providers from the parent...
                CopyProviders(parent);
                // During loading of core modules, providers are not populated.
                // We set the drive information later
                if (Providers != null && Providers.Count > 0)
                    CurrentDrive = parent.CurrentDrive;
                // Link it to the global scope...
                if (linkToGlobal)
                    GlobalScope = parent.GlobalScope;
                _currentScope.LocalsTuple = MutableTuple.MakeTuple(Compiler.DottedLocalsTupleType, Compiler.DottedLocalsNameIndexMap);
        /// Add any special variables to the session state variable table. This routine
        /// must be called at construction time or if the variable table is reset.
        internal void InitializeSessionStateInternalSpecialVariables(bool clearVariablesTable)
            if (clearVariablesTable)
                // Clear the variable table
                GlobalScope.Variables.Clear();
                // Add in the per-scope default variables.
                GlobalScope.AddSessionStateScopeDefaultVariables();
            // Set variable $Error
            PSVariable errorvariable = new PSVariable("Error", new ArrayList(), ScopedItemOptions.Constant);
            GlobalScope.SetVariable(errorvariable.Name, errorvariable, false, false, this, fastPath: true);
            // Set variable $PSDefaultParameterValues
            Collection<Attribute> attributes = new Collection<Attribute>();
            attributes.Add(new ArgumentTypeConverterAttribute(typeof(System.Management.Automation.DefaultParameterDictionary)));
            PSVariable psDefaultParameterValuesVariable = new PSVariable(SpecialVariables.PSDefaultParameterValues,
                                                                         new DefaultParameterDictionary(),
                                                                         ScopedItemOptions.None, attributes,
                                                                         RunspaceInit.PSDefaultParameterValuesDescription);
            GlobalScope.SetVariable(psDefaultParameterValuesVariable.Name, psDefaultParameterValuesVariable, false, false, this, fastPath: true);
        #region Private data
        /// Provides all the path manipulation and globbing for Monad paths.
        internal LocationGlobber Globber
            get { return _globberPrivate ??= ExecutionContext.LocationGlobber; }
        private LocationGlobber _globberPrivate;
        /// The context of the runspace to which this session state object belongs.
        internal ExecutionContext ExecutionContext { get; }
        /// Returns the public session state facade object for this session state instance.
        internal SessionState PublicSessionState
            get { return _publicSessionState ??= new SessionState(this); }
            set { _publicSessionState = value; }
        private SessionState _publicSessionState;
        internal ProviderIntrinsics InvokeProvider
            get { return _invokeProvider ??= new ProviderIntrinsics(this); }
        private ProviderIntrinsics _invokeProvider;
        /// The module info object associated with this session state.
        internal PSModuleInfo Module { get; set; } = null;
        // This is used to maintain the order in which modules were imported.
        // This is used by Get-Command -All to order by last imported
        internal List<string> ModuleTableKeys = new List<string>();
        /// The private module table for this session state object...
        internal Dictionary<string, PSModuleInfo> ModuleTable { get; } = new Dictionary<string, PSModuleInfo>(StringComparer.OrdinalIgnoreCase);
                return ExecutionContext.LanguageMode;
                ExecutionContext.LanguageMode = value;
                return ExecutionContext.UseFullLanguageModeInDebugger;
        /// The list of scripts that are allowed to be run. If the name "*"
        /// is in the list, then all scripts can be run. (This is the default.)
        public List<string> Scripts { get; } = new List<string>(new string[] { "*" });
        /// See if a script is allowed to be run.
        /// <param name="scriptPath">Path to check.</param>
        /// <returns>True if script is allowed.</returns>
        internal SessionStateEntryVisibility CheckScriptVisibility(string scriptPath)
            return checkPathVisibility(Scripts, scriptPath);
        /// The list of applications that are allowed to be run. If the name "*"
        /// is in the list, then all applications can be run. (This is the default.)
        public List<string> Applications { get; } = new List<string>(new string[] { "*" });
        /// List of functions/filters to export from this session state object...
        internal List<CmdletInfo> ExportedCmdlets { get; } = new List<CmdletInfo>();
        /// Defines the default command visibility for this session state. Binding an InitialSessionState instance
        /// with private members will set this to Private.
        internal SessionStateEntryVisibility DefaultCommandVisibility = SessionStateEntryVisibility.Public;
        /// Add an new SessionState cmdlet entry to this session state object...
        /// <param name="entry">The entry to add.</param>
        internal void AddSessionStateEntry(SessionStateCmdletEntry entry)
            AddSessionStateEntry(entry, /*local*/false);
        /// <param name="local">If local, add cmdlet to current scope. Else, add to module scope.</param>
        internal void AddSessionStateEntry(SessionStateCmdletEntry entry, bool local)
            ExecutionContext.CommandDiscovery.AddSessionStateCmdletEntryToCache(entry, local);
        internal void AddSessionStateEntry(SessionStateApplicationEntry entry)
            this.Applications.Add(entry.Path);
        internal void AddSessionStateEntry(SessionStateScriptEntry entry)
            this.Scripts.Add(entry.Path);
        /// Add the variables that must always be present in a SessionState instance...
        internal void InitializeFixedVariables()
            // String resources for aliases are currently associated with Runspace init
            // $Host
                    SpecialVariables.Host,
                    ExecutionContext.EngineHostInterface,
                    ScopedItemOptions.Constant | ScopedItemOptions.AllScope,
                    RunspaceInit.PSHostDescription);
            this.GlobalScope.SetVariable(v.Name, v, asValue: false, force: true, this, CommandOrigin.Internal, fastPath: true);
            // $HOME - indicate where a user's home directory is located in the file system.
            //    -- %USERPROFILE% on windows
            //    -- %HOME% on unix
            string home = Environment.GetEnvironmentVariable(Platform.CommonEnvVariableNames.Home) ?? string.Empty;
            v = new PSVariable(SpecialVariables.Home,
                    home,
                    ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope,
                    RunspaceInit.HOMEDescription);
            // $ExecutionContext
            v = new PSVariable(SpecialVariables.ExecutionContext,
                    ExecutionContext.EngineIntrinsics,
                    RunspaceInit.ExecutionContextDescription);
            // $PSVersionTable
            v = new PSVariable(SpecialVariables.PSVersionTable,
                    PSVersionInfo.GetPSVersionTable(),
                    RunspaceInit.PSVersionTableDescription);
            // $PSEdition
            v = new PSVariable(SpecialVariables.PSEdition,
                    PSVersionInfo.PSEditionValue,
                    RunspaceInit.PSEditionDescription);
            // $PID
            v = new PSVariable(
                    SpecialVariables.PID,
                    Environment.ProcessId,
                    RunspaceInit.PIDDescription);
            // $PSCulture
            v = new PSCultureVariable();
            this.GlobalScope.SetVariableForce(v, this);
            // $PSUICulture
            v = new PSUICultureVariable();
            // $?
            v = new QuestionMarkVariable(this.ExecutionContext);
            // $ShellId - if there is no runspace config, use the default string
            string shellId = ExecutionContext.ShellID;
            v = new PSVariable(SpecialVariables.ShellId, shellId,
                    RunspaceInit.MshShellIdDescription);
            // $PSHOME
            string applicationBase = Utils.DefaultPowerShellAppBase;
            v = new PSVariable(SpecialVariables.PSHome, applicationBase,
                    RunspaceInit.PSHOMEDescription);
            // $EnabledExperimentalFeatures
            v = new PSVariable(SpecialVariables.EnabledExperimentalFeatures,
                               ExperimentalFeature.EnabledExperimentalFeatureNames,
                               RunspaceInit.EnabledExperimentalFeatures);
        /// Check to see if an application is allowed to be run.
        /// <param name="applicationPath">The path to the application to check.</param>
        /// <returns>True if application is permitted.</returns>
        internal SessionStateEntryVisibility CheckApplicationVisibility(string applicationPath)
            return checkPathVisibility(Applications, applicationPath);
        private static SessionStateEntryVisibility checkPathVisibility(List<string> list, string path)
            if (list == null || list.Count == 0)
                return SessionStateEntryVisibility.Private;
            if (list.Contains("*"))
            foreach (string p in list)
                if (string.Equals(p, path, StringComparison.OrdinalIgnoreCase))
                if (WildcardPattern.ContainsWildcardCharacters(p))
                    WildcardPattern pattern = WildcardPattern.Get(p, WildcardOptions.IgnoreCase);
                    if (pattern.IsMatch(path))
        #endregion Private data
        /// Notification for SessionState to do cleanup
        /// before runspace is closed.
            if (this != ExecutionContext.TopLevelSessionState && Providers.Count > 0)
                // Remove all providers at the top level...
                CmdletProviderContext context = new CmdletProviderContext(this.ExecutionContext);
                foreach (string providerName in Providers.Keys)
                    // All errors are ignored.
                    RemoveProvider(providerName, true, context);
        #region Errors
        /// Constructs a new instance of a ProviderInvocationException
        /// using the specified data.
        /// The resource ID to use as the format message for the error.
        /// <param name="resourceStr">
        /// This is the message template string.
        /// The provider information used when formatting the error message.
        /// The path used when formatting the error message.
        /// The exception that was thrown by the provider. This will be set as
        /// the ProviderInvocationException's InnerException and the message will
        /// be used when formatting the error message.
        /// A new instance of a ProviderInvocationException.
        /// Wraps <paramref name="e"/> in a ProviderInvocationException
        /// and then throws it.
        internal ProviderInvocationException NewProviderInvocationException(
            string resourceStr,
            Exception e)
            return NewProviderInvocationException(resourceId, resourceStr, provider, path, e, true);
        /// <param name="useInnerExceptionErrorMessage">
        /// If true, the error record from the inner exception will be used if it contains one.
        /// If false, the error message specified by the resourceId will be used.
            bool useInnerExceptionErrorMessage)
            //  If the innerException was itself thrown by
            //  ProviderBase.ThrowTerminatingError, it is already a
            //  ProviderInvocationException, and we don't want to
            //  re-wrap it.
            ProviderInvocationException pie = e as ProviderInvocationException;
            if (pie != null)
                pie._providerInfo = provider;
                return pie;
            pie = new ProviderInvocationException(resourceId, resourceStr, provider, path, e, useInnerExceptionErrorMessage);
                ExecutionContext,
                provider.Name,
                pie,
        #endregion Errors

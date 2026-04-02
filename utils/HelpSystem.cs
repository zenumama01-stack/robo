    /// Monad help is an architecture made up of three layers:
    ///     1. At the top is get-help commandlet from where help functionality is accessed.
    ///     2. At the middle is the help system which collects help objects based on user's request.
    ///     3. At the bottom are different help providers which provide help contents for different kinds of information requested.
    /// Class HelpSystem implements the middle layer of Monad Help.
    /// HelpSystem will provide functionalities in following areas,
    ///     1. Initialization and management of help providers
    ///     2. Help engine: this will invoke different providers based on user's request.
    ///     3. Help API: this is the API HelpSystem provide to get-help commandlet.
    /// Initialization:
    ///     Initialization of different help providers needs some context information like "ExecutionContext"
    /// Help engine:
    ///     By default, HelpInfo will be retrieved in two phases: exact-match phase and search phase.
    ///     Exact-match phase: help providers will be called in appropriate order to retrieve HelpInfo.
    ///         If a match is found, help engine will stop and return the one and only HelpInfo retrieved.
    ///     Search phase: all relevant help providers will be called to retrieve HelpInfo. (Order doesn't
    ///         matter in this case) Help engine will not stop until all help providers are called.
    ///     Behavior of the help engine can be modified based on Help API parameters in the following ways:
    ///         1. limit the number of HelpInfo to be returned.
    ///         2. specify which providers will be used.
    ///         3. general help info returned in case the search target is empty.
    ///         4. default help info (or hint) returned in case no match is found.
    /// Help API:
    ///     The Help API is the function to be called by the Get-Help cmdlet.
    ///     The following information shall be provided in Help API parameters:
    ///         1. search target: (which can be one or multiple strings)
    ///         2. help type: to limit the type of help to be searched
    ///         3. included fields: the fields to be included in the help info
    ///         4. excluded fields: the fields to be excluded in the help info
    ///         5. max number of results to be returned
    ///         6. scoring algorithm for help results
    ///         7. help reason: help can be directly invoked by end user or as a result of
    ///             some command syntax error.
    ///     [gxie, 7-25-04]: included fields, excluded fields and help reason will be handled in
    ///         get-help commandlet.
    ///     Help API's are internal. The only way to access help is by
    ///     invoking the get-help command.
    ///     To support the scenario of multiple monad engines running in one process, each
    ///     monad engine is required to have its one help system instance.
    ///     Currently each ExecutionContext has a help system instance as its member.
    ///     The result of a help provider invocation can be three things:
    ///         a. Full help info (in case of an exact-match and a single search result)
    ///         b. Short help info (in case of multiple search results)
    ///         c. Partial help info (in case of some cmdlet help info, which
    ///         d. Help forwarding info (in the case of an alias, which will return the target
    ///                                   for the alias)
    ///     Help providers may need to provide functionality in the following two areas:
    ///         b. localization.
    internal class HelpSystem
        /// Constructor for HelpSystem.
        /// <param name="context">Execution context for this help system.</param>
        internal HelpSystem(ExecutionContext context)
                throw PSTraceSource.NewArgumentNullException("ExecutionContext");
            _executionContext = context;
        /// ExecutionContext for the help system. Different help providers
        /// will depend on this to retrieve session-related information like
        /// session state and command discovery objects.
                return _executionContext;
        #region Progress Callback
        internal event EventHandler<HelpProgressEventArgs> OnProgress;
        #region Initialization
        /// Initialize the help system with an execution context. If the execution context
        /// matches the execution context of current singleton HelpSystem object, nothing
        /// needs to be done. Otherwise, a new singleton HelpSystem object will be created
        /// with the new execution context.
            _verboseHelpErrors = LanguagePrimitives.IsTrue(
                _executionContext.GetVariableValue(SpecialVariables.VerboseHelpErrorsVarPath, false));
            _helpErrorTracer = new HelpErrorTracer(this);
            InitializeHelpProviders();
        #endregion Initialization
        #region Help API
        /// Get Help API function. This is the basic form of the Help API using help
        /// request.
        /// Variants of this function are defined below, which will create help request
        /// object on the fly.
        /// <param name="helpRequest">HelpRequest object</param>
        /// <returns>An array of HelpInfo objects</returns>
        internal IEnumerable<HelpInfo> GetHelp(HelpRequest helpRequest)
            helpRequest.Validate();
            ValidateHelpCulture();
            return this.DoGetHelp(helpRequest);
        #endregion Help API
        #region Error Handling
        private readonly Collection<ErrorRecord> _lastErrors = new Collection<ErrorRecord>();
        /// This is for tracking the last set of errors happened during the help
        /// search.
        internal Collection<ErrorRecord> LastErrors
                return _lastErrors;
        private HelpCategory _lastHelpCategory = HelpCategory.None;
        /// This is the help category to search for help for the last command.
        /// <value>help category to search for help</value>
        internal HelpCategory LastHelpCategory
                return _lastHelpCategory;
        #region Configuration
        private bool _verboseHelpErrors = false;
        /// VerboseHelpErrors is used in the case when end users are interested
        /// to know all errors that happened during a help search. This property
        /// is false by default.
        /// If this property is turned on (by setting session variable "VerboseHelpError"),
        /// following two behaviours will be different,
        ///     a. Help errors will be written to the error pipeline regardless of the situation.
        ///        (Normally, help errors will be written to error pipeline if there is no
        ///         help found and there is no wildcard in help search target).
        ///     b. Some additional warnings, including MAML processing warnings, will be
        ///        written to the error pipeline.
        internal bool VerboseHelpErrors
                return _verboseHelpErrors;
        #region Help Engine
        // Cache of search paths that are currently active.
        // This will save a lot time when help providers do their searching
        private Collection<string> _searchPaths = null;
        /// Gets the search paths for external snapins/modules that are currently loaded.
        /// If the current shell is single-shell-based, then the returned
        /// search path contains all the directories of currently active PSSnapIns/modules.
        /// <returns>A collection of strings representing locations.</returns>
            // return the cache if already present.
            if (_searchPaths != null)
                return _searchPaths;
            _searchPaths = new Collection<string>();
            // add loaded modules paths to the search path
            if (ExecutionContext.Modules != null)
                foreach (PSModuleInfo loadedModule in ExecutionContext.Modules.ModuleTable.Values)
                    if (!_searchPaths.Contains(loadedModule.ModuleBase))
                        _searchPaths.Add(loadedModule.ModuleBase);
        /// Get help based on the target, help type, etc
        /// Help engine retrieve help based on following schemes:
        ///     1. if the help target is empty, get default help
        ///     2. if the help target is not a search pattern, try to retrieve exact help
        ///     3. if help target is a search pattern or step 2 returns no helpInfo, try to search for help
        ///        (Search for pattern in command name followed by pattern match in help content)
        ///     4. if step 3 returns exactly one helpInfo object, try to retrieve exact help.
        private IEnumerable<HelpInfo> DoGetHelp(HelpRequest helpRequest)
            _lastErrors.Clear();
            // Reset SearchPaths
            _searchPaths = null;
            _lastHelpCategory = helpRequest.HelpCategory;
            if (string.IsNullOrEmpty(helpRequest.Target))
                HelpInfo helpInfo = GetDefaultHelp();
                bool isMatchFound = false;
                    foreach (HelpInfo helpInfo in ExactMatchHelp(helpRequest))
                        isMatchFound = true;
                if (!isMatchFound)
                    foreach (HelpInfo helpInfo in SearchHelp(helpRequest))
                        // Throwing an exception here may not be the
                        // best thing to do. Instead we can choose to
                        //    a. give a hint
                        //    b. just silently return an empty search result.
                        // Solution:
                        //    If it is an exact help target, throw an exception.
                        //    Otherwise, return empty result set.
                        if (!WildcardPattern.ContainsWildcardCharacters(helpRequest.Target) && this.LastErrors.Count == 0)
                            Exception e = new HelpNotFoundException(helpRequest.Target);
                            ErrorRecord errorRecord = new ErrorRecord(e, "HelpNotFound", ErrorCategory.ResourceUnavailable, null);
                            this.LastErrors.Add(errorRecord);
        /// Get help that exactly matches the target.
        /// If the helpInfo returned is not complete, we shall forward the
        /// helpInfo object to the appropriate help provider for further processing.
        /// (this is implemented by ForwardHelp)
        /// <param name="helpRequest">Help request object</param>
        /// <returns>HelpInfo object retrieved (can be null)</returns>
        internal IEnumerable<HelpInfo> ExactMatchHelp(HelpRequest helpRequest)
            bool isHelpInfoFound = false;
            for (int i = 0; i < this.HelpProviders.Count; i++)
                HelpProvider helpProvider = (HelpProvider)this.HelpProviders[i];
                if ((helpProvider.HelpCategory & helpRequest.HelpCategory) > 0)
                    foreach (HelpInfo helpInfo in helpProvider.ExactMatchHelp(helpRequest))
                        isHelpInfoFound = true;
                        foreach (HelpInfo fwdHelpInfo in ForwardHelp(helpInfo, helpRequest))
                            yield return fwdHelpInfo;
                // Bug Win7 737383: Win7 RTM shows both function and cmdlet help when there is
                // function and cmdlet with the same name. So, ignoring the ScriptCommandHelpProvider's
                // results and going to the CommandHelpProvider for further evaluation.
                if (isHelpInfoFound && helpProvider is not ScriptCommandHelpProvider)
                    // once helpInfo found from a provider..no need to traverse other providers.
        /// Forward help to the help provider with type forwardHelpCategory.
        /// This is used in the following known scenarios so far
        ///     1. Alias: helpInfo returned by Alias is not what end user needed.
        ///               The real help can be retrieved from Command help provider.
        /// <returns>Never returns null.</returns>
        /// <remarks>helpInfos is not null or empty.</remarks>
        private IEnumerable<HelpInfo> ForwardHelp(HelpInfo helpInfo, HelpRequest helpRequest)
            // findout if this helpInfo needs to be processed further..
            if (helpInfo.ForwardHelpCategory == HelpCategory.None && string.IsNullOrEmpty(helpInfo.ForwardTarget))
                // this helpInfo is final...so store this in result
                // and move on..
                // Find out a capable provider to process this request...
                HelpCategory forwardHelpCategory = helpInfo.ForwardHelpCategory;
                bool isHelpInfoProcessed = false;
                    if ((helpProvider.HelpCategory & forwardHelpCategory) != HelpCategory.None)
                        isHelpInfoProcessed = true;
                        // If this help info is processed by this provider already, break
                        // out of the provider loop...
                        foreach (HelpInfo fwdResult in helpProvider.ProcessForwardedHelp(helpInfo, helpRequest))
                            // Add each helpinfo to our repository
                            foreach (HelpInfo fHelpInfo in ForwardHelp(fwdResult, helpRequest))
                                yield return fHelpInfo;
                            // get out of the provider loop..
                if (!isHelpInfoProcessed)
                    // we are here because no help provider processed the helpinfo..
                    // so add this to our repository..
        /// Get the default help info (normally when help target is empty).
        private HelpInfo GetDefaultHelp()
            HelpRequest helpRequest = new HelpRequest("default", HelpCategory.DefaultHelp);
                // return just the first helpInfo object
        /// Get help that exactly match the target.
        /// <returns>An IEnumerable of HelpInfo object.</returns>
        private IEnumerable<HelpInfo> SearchHelp(HelpRequest helpRequest)
            int countOfHelpInfosFound = 0;
            bool searchInHelpContent = false;
            bool shouldBreak = false;
            HelpProgressEventArgs progress = new HelpProgressEventArgs();
            progress.Activity = StringUtil.Format(HelpDisplayStrings.SearchingForHelpContent, helpRequest.Target);
            progress.Completed = false;
            progress.PercentComplete = 0;
                OnProgress(this, progress);
                // algorithm:
                // 1. Search for pattern (helpRequest.Target) in command name
                // 2. If Step 1 fails then search for pattern in help content
                    // we should not continue the search loop if we are
                    // searching in the help content (as this is the last step
                    // in our search algorithm).
                    if (searchInHelpContent)
                        shouldBreak = true;
                            foreach (HelpInfo helpInfo in helpProvider.SearchHelp(helpRequest, searchInHelpContent))
                                if (_executionContext.CurrentPipelineStopping)
                                countOfHelpInfosFound++;
                                if ((countOfHelpInfosFound >= helpRequest.MaxResults) && (helpRequest.MaxResults > 0))
                    // no need to do help content search once we have some help topics
                    // with command name search.
                    if (countOfHelpInfosFound > 0)
                    // appears that we did not find any help matching command names..look for
                    // pattern in help content.
                    searchInHelpContent = true;
                    if (this.HelpProviders.Count > 0)
                        progress.PercentComplete += (100 / this.HelpProviders.Count);
                } while (!shouldBreak);
                progress.Completed = true;
        #endregion Help Engine
        #region Help Provider Manager
        private readonly ArrayList _helpProviders = new ArrayList();
        /// Return the list of help providers initialized.
        /// <value>a list of help providers</value>
        internal ArrayList HelpProviders
                return _helpProviders;
        /// Initialize help providers.
        /// Currently we hardcode the sequence of help provider initialization.
        /// In the longer run, we probably will load help providers based on some provider catalog. That
        /// will allow new providers to be defined by customer.
        private void InitializeHelpProviders()
            HelpProvider helpProvider = null;
            helpProvider = new AliasHelpProvider(this);
            _helpProviders.Add(helpProvider);
            helpProvider = new ScriptCommandHelpProvider(this);
            helpProvider = new CommandHelpProvider(this);
            helpProvider = new ProviderHelpProvider(this);
            helpProvider = new PSClassHelpProvider(this);
            /* TH Bug#3141590 - Disable DscResourceHelp for ClientRTM due to perf issue.
#if !CORECLR // TODO:CORECLR Add this back in once we support Get-DscResource
            helpProvider = new DscResourceHelpProvider(this);
            helpProvider = new HelpFileHelpProvider(this);
            helpProvider = new DefaultHelpProvider(this);
#if _HelpProviderReflection
        // Eventually we will publicize the provider api and initialize
        // help providers using reflection. This is not in v1 right now.
        private static HelpProviderInfo[] _providerInfos = new HelpProviderInfo[]
                            { new HelpProviderInfo(string.Empty, "AliasHelpProvider", HelpCategory.Alias),
                              new HelpProviderInfo(string.Empty, "CommandHelpProvider", HelpCategory.Command),
                              new HelpProviderInfo(string.Empty, "ProviderHelpProvider", HelpCategory.Provider),
                              new HelpProviderInfo(string.Empty, "OverviewHelpProvider", HelpCategory.Overview),
                              new HelpProviderInfo(string.Empty, "GeneralHelpProvider", HelpCategory.General),
                              new HelpProviderInfo(string.Empty, "FAQHelpProvider", HelpCategory.FAQ),
                              new HelpProviderInfo(string.Empty, "GlossaryHelpProvider", HelpCategory.Glossary),
                              new HelpProviderInfo(string.Empty, "HelpFileHelpProvider", HelpCategory.HelpFile),
                              new HelpProviderInfo(string.Empty, "DefaultHelpHelpProvider", HelpCategory.DefaultHelp)
            for (int i = 0; i < _providerInfos.Length; i++)
                HelpProvider helpProvider = GetHelpProvider(_providerInfos[i]);
                if (helpProvider != null)
                    helpProvider.Initialize(this._executionContext);
        private HelpProvider GetHelpProvider(HelpProviderInfo providerInfo)
            Assembly providerAssembly = null;
            if (string.IsNullOrEmpty(providerInfo.AssemblyName))
                providerAssembly = Assembly.GetExecutingAssembly();
                providerAssembly = Assembly.Load(providerInfo.AssemblyName);
                if (providerAssembly != null)
                    HelpProvider helpProvider =
                        (HelpProvider)providerAssembly.CreateInstance(providerInfo.ClassName,
                                                                     false, // don't ignore case
                                                                     BindingFlags.CreateInstance,
                                                                     null, // use default binder
                                                                     null, // use current culture
                                                                     null // no special activation attributes
                    return helpProvider;
                System.Console.WriteLine(e.Message);
                    System.Console.WriteLine(e.InnerException.Message);
                    System.Console.WriteLine(e.InnerException.StackTrace);
        #endregion Help Provider Manager
        #region Help Error Tracer
        private HelpErrorTracer _helpErrorTracer;
        /// The error tracer for this help system.
        internal HelpErrorTracer HelpErrorTracer
                return _helpErrorTracer;
        /// Start a trace frame for a help file.
            if (_helpErrorTracer == null)
            return _helpErrorTracer.Trace(helpFile);
        /// Trace an error within a help frame, which is tracked by help tracer itself.
            _helpErrorTracer.TraceError(errorRecord);
        /// Trace a collection of errors within a help frame, which is tracked by
        /// help tracer itself.
            if (_helpErrorTracer == null || errorRecords == null)
            _helpErrorTracer.TraceErrors(errorRecords);
        #region Help MUI
        private CultureInfo _culture;
        /// Before each help request is serviced, current thread culture will validate
        /// against the current culture of help system. If there is a miss match, each
        /// help provider will be notified of the culture change.
        private void ValidateHelpCulture()
            CultureInfo culture = CultureInfo.CurrentUICulture;
            if (_culture == null)
                _culture = culture;
            if (_culture.Equals(culture))
            ResetHelpProviders();
        /// Reset help providers providers. This normally corresponds to help culture change.
        /// Normally help providers will remove cached help content to make sure new help
        /// requests will be served with content of right culture.
        internal void ResetHelpProviders()
            if (_helpProviders == null)
            for (int i = 0; i < _helpProviders.Count; i++)
                HelpProvider helpProvider = (HelpProvider)_helpProviders[i];
                helpProvider.Reset();
        #region ScriptBlock Parse Tokens Caching/Clearing Functionality
        private readonly Lazy<Dictionary<Ast, Token[]>> _scriptBlockTokenCache = new Lazy<Dictionary<Ast, Token[]>>(isThreadSafe: true);
        internal Dictionary<Ast, Token[]> ScriptBlockTokenCache
            get { return _scriptBlockTokenCache.Value; }
        internal void ClearScriptBlockTokenCache()
            if (_scriptBlockTokenCache.IsValueCreated)
                _scriptBlockTokenCache.Value.Clear();
    /// Help progress info.
    internal class HelpProgressEventArgs : EventArgs
        internal bool Completed { get; set; }
        internal string Activity { get; set; }
        internal int PercentComplete { get; set; }
    /// This is the structure to keep track of HelpProvider Info.
    internal class HelpProviderInfo
        internal string AssemblyName = string.Empty;
        internal string ClassName = string.Empty;
        internal HelpCategory HelpCategory = HelpCategory.None;
        /// <param name="assemblyName">Assembly that contains this help provider.</param>
        /// <param name="className">The class that implements this help provider.</param>
        /// <param name="helpCategory">Help category of this help provider.</param>
        internal HelpProviderInfo(string assemblyName, string className, HelpCategory helpCategory)
            this.AssemblyName = assemblyName;
            this.ClassName = className;
            this.HelpCategory = helpCategory;
    /// Help categories.
    internal enum HelpCategory
        /// Undefined help category.
        /// Alias help.
        Alias = 0x01,
        /// Cmdlet help.
        Cmdlet = 0x02,
        /// Provider help.
        Provider = 0x04,
        /// General keyword help.
        General = 0x10,
        /// FAQ's.
        FAQ = 0x20,
        /// Glossary and term definitions.
        Glossary = 0x40,
        /// Help that is contained in help file.
        HelpFile = 0x80,
        /// Help from a script block.
        ScriptCommand = 0x100,
        /// Help for a function.
        Function = 0x200,
        /// Help for a filter.
        Filter = 0x400,
        /// Help for an external script (i.e. for a *.ps1 file)
        ExternalScript = 0x800,
        /// All help categories.
        All = 0xFFFFF,
        /// Default Help.
        DefaultHelp = 0x1000,
        /// Help for a Configuration.
        Configuration = 0x4000,
        /// Help for DSC Resource.
        DscResource = 0x8000,
        /// Help for PS Classes.
        Class = 0x10000

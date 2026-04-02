    #region CoreCommandBase
    /// The base command for the core commands.
    public abstract class CoreCommandBase : PSCmdlet, IDynamicParameters
        #region Tracer
        /// An instance of the PSTraceSource class used for trace output
        /// using "NavigationCommands" as the category.
        [Dbg.TraceSource("NavigationCommands", "The namespace navigation tracer")]
        internal static readonly Dbg.PSTraceSource tracer = Dbg.PSTraceSource.GetTracer("NavigationCommands", "The namespace navigation tracer");
        #endregion Tracer
        #region Protected members
        /// The context for the command that is passed to the core command providers.
        internal virtual CmdletProviderContext CmdletProviderContext
                CmdletProviderContext coreCommandContext = new(this);
                coreCommandContext.Force = Force;
                Collection<string> includeFilter =
                    SessionStateUtilities.ConvertArrayToCollection<string>(Include);
                Collection<string> excludeFilter =
                    SessionStateUtilities.ConvertArrayToCollection<string>(Exclude);
                coreCommandContext.SetFilters(includeFilter, excludeFilter, Filter);
                coreCommandContext.SuppressWildcardExpansion = SuppressWildcardExpansion;
                coreCommandContext.DynamicParameters = RetrievedDynamicParameters;
                stopContextCollection.Add(coreCommandContext);
                return coreCommandContext;
        internal virtual SwitchParameter SuppressWildcardExpansion
            get => _suppressWildcardExpansion;
            set => _suppressWildcardExpansion = value;
        private bool _suppressWildcardExpansion;
        internal virtual object GetDynamicParameters(CmdletProviderContext context) => null;
        /// Called by the base implementation that checks the SupportShouldProcess provider
        /// capability. This virtual method gives the
        /// derived cmdlet a chance query the CmdletProvider capabilities to determine
        /// if the provider supports ShouldProcess.
        protected virtual bool ProviderSupportsShouldProcess => true;
        /// A helper for derived classes to call to determine if the paths specified
        /// are for a provider that supports ShouldProcess.
        /// <param name="paths">
        /// The paths to check to see if the providers support ShouldProcess.
        /// If the paths are to different providers, and any don't support
        /// ShouldProcess, then the return value is false. If they all
        /// support ShouldProcess then the return value is true.
        protected bool DoesProviderSupportShouldProcess(string[] paths)
            // If no paths are specified, then default to true as the paths
            // may be getting piped in.
            bool result = true;
            if (paths != null)
                    // I don't really care about the returned path, just the provider name
                        this.CmdletProviderContext,
                    // Check the provider's capabilities
                    if (!CmdletProviderManagementIntrinsics.CheckProviderCapabilities(
                            ProviderCapabilities.ShouldProcess,
                            provider))
                        result = false;
        /// The dynamic parameters which have already been retrieved from the provider
        /// and bound by the command processor.
        protected internal object RetrievedDynamicParameters => _dynamicParameters;
        /// The dynamic parameters for the command. They are retrieved using the
        /// GetDynamicParameters virtual method.
        private object _dynamicParameters;
        #endregion Protected members
        #region Public members
        /// Stops the processing of the provider by using the
        /// CmdletProviderContext to tunnel the stop message to
        /// the provider instance.
            foreach (CmdletProviderContext stopContext in stopContextCollection)
                stopContext.StopProcessing();
        internal Collection<CmdletProviderContext> stopContextCollection =
        /// This is meant to be overridden by derived classes if
        /// they support the Filter parameter. This property is on
        /// the base class to simplify the creation of the CmdletProviderContext.
        public virtual string Filter { get; set; }
        /// they support the Include parameter. This property is on
        public virtual string[] Include
        } = Array.Empty<string>();
        /// they support the Exclude parameter. This property is on
        public virtual string[] Exclude
        /// they support the Force parameter. This property is on
        public virtual SwitchParameter Force
            get => _force;
            set => _force = value;
        /// Retrieves the dynamic parameters for the command from
        /// the provider.
        public object GetDynamicParameters()
            // Don't stream errors or Write* to the pipeline.
            CmdletProviderContext context = CmdletProviderContext;
            context.PassThru = false;
                _dynamicParameters = GetDynamicParameters(context);
            catch (ItemNotFoundException)
                _dynamicParameters = null;
            catch (ProviderNotFoundException)
            catch (DriveNotFoundException)
        /// Determines if the cmdlet and CmdletProvider supports ShouldProcess.
        public bool SupportsShouldProcess => ProviderSupportsShouldProcess;
        #endregion Public members
    #endregion CoreCommandBase
    #region CoreCommandWithCredentialsBase
    /// The base class for core commands to extend when they require credentials
    /// to be passed as parameters.
    public class CoreCommandWithCredentialsBase : CoreCommandBase
        /// Gets or sets the credential parameter.
        internal override CmdletProviderContext CmdletProviderContext
                CmdletProviderContext coreCommandContext = new(this, Credential);
    #endregion CoreCommandWithCredentialsBase
    #region GetLocationCommand
    /// The get-location command class.
    /// This command does things like list the contents of a container, get
    /// an item at a given path, get the current working directory, etc.
    [Cmdlet(VerbsCommon.Get, "Location", DefaultParameterSetName = LocationParameterSet, SupportsTransactions = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096495")]
    [OutputType(typeof(PathInfo), ParameterSetName = new string[] { LocationParameterSet })]
    [OutputType(typeof(PathInfoStack), ParameterSetName = new string[] { StackParameterSet })]
    public class GetLocationCommand : DriveMatchingCoreCommandBase
        private const string LocationParameterSet = "Location";
        private const string StackParameterSet = "Stack";
        #region Location parameter set parameters
        /// Gets or sets the provider from which to get the current location.
        [Parameter(ParameterSetName = LocationParameterSet, ValueFromPipelineByPropertyName = true)]
        public string[] PSProvider
            get => _provider;
            set => _provider = value ?? Array.Empty<string>();
        /// Gets or sets the drive from which to get the current location.
        public string[] PSDrive { get; set; }
        #endregion Location parameter set parameters
        #region Stack parameter set parameters
        /// Gets or sets the Stack switch parameter which is used
        /// to disambiguate parameter sets.
        [Parameter(ParameterSetName = StackParameterSet)]
        public SwitchParameter Stack
            get => _stackSwitch;
            set => _stackSwitch = value;
        private bool _stackSwitch;
        /// Gets or sets the stack ID for the location stack that will
        /// be retrieved.
        [Parameter(ParameterSetName = StackParameterSet, ValueFromPipelineByPropertyName = true)]
        public string[] StackName
            get => _stackNames;
            set => _stackNames = value;
        #endregion Stack parameter set parameters
        #region Location parameter set data
        /// The name of the provider from which to return the current location.
        private string[] _provider = Array.Empty<string>();
        #endregion Location parameter set data
        #region Stack parameter set data
        /// The name of the location stack from which to return the stack.
        private string[] _stackNames;
        #endregion Stack parameter set data
        /// The main execution method for the get-location command. Depending on
        /// the parameter set that is specified, the command can do many things.
        ///     -locationSet gets the current working directory as a Monad path
        ///     -stackSet gets the directory stack of directories that have been
        ///               pushed by the push-location command.
            // It is OK to use a switch for string comparison here because we
            // want a case sensitive comparison in the current culture.
                case LocationParameterSet:
                    PathInfo result = null;
                    if (PSDrive != null && PSDrive.Length > 0)
                        foreach (string drive in PSDrive)
                            List<PSDriveInfo> foundDrives = null;
                                foundDrives = GetMatchingDrives(drive, PSProvider, null);
                            catch (DriveNotFoundException e)
                                ErrorRecord errorRecord =
                                        "GetLocationNoMatchingDrive",
                                        drive);
                            catch (ProviderNotFoundException e)
                                        "GetLocationNoMatchingProvider",
                                        PSProvider);
                            catch (ArgumentException argException)
                                        argException,
                            // Get the current location for a specific drive and provider
                            foreach (PSDriveInfo workingDrive in foundDrives)
                                    string path =
                                        LocationGlobber.GetDriveQualifiedPath(
                                            workingDrive.CurrentLocation,
                                            workingDrive);
                                    result = new PathInfo(workingDrive, workingDrive.Provider, path, SessionState);
                    // If the drive wasn't specified but the provider was
                    else if ((PSDrive == null || PSDrive.Length == 0) &&
                             (PSProvider != null && PSProvider.Length > 0))
                        foreach (string providerName in PSProvider)
                            bool providerContainsWildcard = WildcardPattern.ContainsWildcardCharacters(providerName);
                            if (!providerContainsWildcard)
                                // Since the Provider was specified and doesn't contain
                                // wildcard characters, make sure it exists.
                                    SessionState.Provider.GetOne(providerName);
                                            providerName);
                            // Match the providers
                            foreach (ProviderInfo providerInfo in SessionState.Provider.GetAll())
                                if (providerInfo.IsMatch(providerName))
                                        WriteObject(SessionState.Path.CurrentProviderLocation(providerInfo.FullName));
                                        if (providerContainsWildcard)
                                            // NTRAID#Windows Out Of Band Releases-923607-2005/11/02-JeffJon
                                            // This exception is ignored, because it just means we didn't find
                                            // an active drive for the provider.
                        // Get the current working directory using the core command API.
                        WriteObject(SessionState.Path.CurrentLocation);
                case StackParameterSet:
                    if (_stackNames != null)
                        foreach (string stackName in _stackNames)
                                // Get the directory stack. This is similar to the "dirs" command
                                WriteObject(SessionState.Path.LocationStack(stackName), false);
                            catch (PSArgumentException argException)
                                        argException.ErrorRecord,
                                        argException));
                            WriteObject(SessionState.Path.LocationStack(null), false);
                    Dbg.Diagnostics.Assert(false, string.Create(System.Globalization.CultureInfo.InvariantCulture, $"One of the predefined parameter sets should have been specified, instead we got: {ParameterSetName}"));
    #endregion GetLocationCommand
    #region SetLocationCommand
    /// The core command for setting/changing location.
    /// This is the equivalent of cd command.
    [Cmdlet(VerbsCommon.Set, "Location", DefaultParameterSetName = PathParameterSet, SupportsTransactions = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097049")]
    [OutputType(typeof(PathInfo), typeof(PathInfoStack))]
    public class SetLocationCommand : CoreCommandBase
        private const string PathParameterSet = "Path";
        private const string LiteralPathParameterSet = "LiteralPath";
        /// Gets or sets the path property.
        [Parameter(Position = 0, ParameterSetName = PathParameterSet,
            get => _path;
            set => _path = value;
        /// Gets or sets the path property, when bound from the pipeline.
        [Parameter(ParameterSetName = LiteralPathParameterSet,
        public string LiteralPath
                _path = value;
        /// Gets or sets the parameter -passThru which states output from
        /// the command should be placed in the pipeline.
        public SwitchParameter PassThru
            get => _passThrough;
            set => _passThrough = value;
        /// Gets or sets the StackName parameter which determines which location stack
        /// to use for the push. If the parameter is missing or empty the default
        /// location stack is used.
        public string StackName { get; set; }
        #region Command data
        /// The filter used when doing a dir.
        private string _path = string.Empty;
        /// Determines if output should be passed through for
        /// set-location.
        private bool _passThrough;
        #endregion Command data
        /// The functional part of the code that does the changing of the current
        /// working directory.
                case PathParameterSet:
                case LiteralPathParameterSet:
                        // Change the current working directory
                        if (string.IsNullOrEmpty(Path))
                            // If user just typed 'cd', go to FileSystem provider home directory
                            Path = SessionState.Internal.GetSingleProvider(Commands.FileSystemProvider.ProviderName).Home;
                        result = SessionState.Path.SetLocation(Path, CmdletProviderContext, ParameterSetName == LiteralPathParameterSet);
                        // Change the default location stack
                        result = SessionState.Path.SetDefaultLocationStack(StackName);
                    catch (ItemNotFoundException itemNotFound)
                                itemNotFound.ErrorRecord,
                                itemNotFound));
                        "One of the specified parameter sets should have been called");
            if (_passThrough && result != null)
    #endregion SetLocationCommand
    #region PushLocationCommand
    /// The core command for setting/changing location and pushing it onto a location stack.
    /// This is the equivalent of the pushd command.
    [Cmdlet(VerbsCommon.Push, "Location", DefaultParameterSetName = PathParameterSet, SupportsTransactions = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097105")]
    public class PushLocationCommand : CoreCommandBase
                   ValueFromPipeline = false, ValueFromPipelineByPropertyName = true)]
        public string StackName
            get => _stackName;
            set => _stackName = value;
        /// push-location.
        /// The ID of the stack to use for the pop.
        private string _stackName;
        /// working directory and pushes the container onto the stack.
            // Push the current working directory onto the
            // working directory stack
            SessionState.Path.PushCurrentLocation(_stackName);
            if (Path != null)
                    // Now change the directory to the one specified
                    // in the command
                    PathInfo result = SessionState.Path.SetLocation(Path, CmdletProviderContext);
    #endregion PushLocationCommand
    #region PopLocationCommand
    /// The core command for pop-location.  This is the equivalent of the popd command.
    /// It pops a container from the stack and sets the current location to that container.
    [Cmdlet(VerbsCommon.Pop, "Location", SupportsTransactions = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096907")]
    public class PopLocationCommand : CoreCommandBase
        /// to use for the pop. If the parameter is missing or empty the default
        /// pop-location.
        /// Gets the top container from the location stack and sets the
        /// location to it.
                // Pop the top of the location stack.
                PathInfo result = SessionState.Path.PopLocation(_stackName);
    #endregion PopLocationCommand
    #region Drive commands
    #region NewPSDriveCommand
    /// Mounts a drive in PowerShell runspace.
    [Cmdlet(VerbsCommon.New, "PSDrive", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Low,
        SupportsTransactions = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096815")]
    public class NewPSDriveCommand : CoreCommandWithCredentialsBase
        /// Gets or sets the name of the drive.
            get => _name;
            set => _name = value ?? throw PSTraceSource.NewArgumentNullException(nameof(value));
        /// Gets or sets the provider ID.
        public string PSProvider
            set => _provider = value ?? throw PSTraceSource.NewArgumentNullException(nameof(value));
        /// Gets or sets the root of the drive. This path should be
        /// a namespace specific path.
        public string Root
            get => _root;
            set => _root = value ?? throw PSTraceSource.NewArgumentNullException(nameof(value));
        /// Gets or sets the description of the drive.
            get => _description;
            set => _description = value ?? throw PSTraceSource.NewArgumentNullException(nameof(value));
        /// Gets or sets the scope identifier for the drive being created.
        [ArgumentCompleter(typeof(ScopeArgumentCompleter))]
        public string Scope { get; set; }
        /// Gets or sets the Persist Switch parameter.
        /// If this switch parameter is set then the created PSDrive
        /// would be persisted across PowerShell sessions.
        public SwitchParameter Persist
            get => _persist;
            set => _persist = value;
        private bool _persist = false;
        /// Gets the dynamic parameters for the new-psdrive cmdlet.
            return SessionState.Drive.NewDriveDynamicParameters(PSProvider, context);
        /// New-psdrive always supports ShouldProcess.
        protected override bool ProviderSupportsShouldProcess => true;
        /// The name of the drive.
        private string _name;
        /// The provider ID for the drive.
        private string _provider;
        /// The namespace specific path of the root of the drive.
        private string _root;
        /// A description for the drive.
        private string _description;
        /// Adds a new drive to the Monad namespace.
                provider = SessionState.Internal.GetSingleProvider(PSProvider);
            // Check to see if the provider exists
            if (provider != null)
                // Get the confirmation strings
                string action = NavigationResources.NewDriveConfirmAction;
                string resourceTemplate = NavigationResources.NewDriveConfirmResourceTemplate;
                string resource =
                       System.Globalization.CultureInfo.CurrentCulture,
                       resourceTemplate,
                       provider.FullName,
                       Root);
                if (ShouldProcess(resource, action))
                    // -Persist switch parameter is supported only for FileSystem provider.
                    if (Persist && !provider.Name.Equals(FileSystemProvider.ProviderName, StringComparison.OrdinalIgnoreCase))
                        ErrorRecord er = new(new NotSupportedException(FileSystemProviderStrings.PersistNotSupported), "DriveRootNotNetworkPath", ErrorCategory.InvalidArgument, this);
                        ThrowTerminatingError(er);
                    // Trimming forward and backward slash for FileSystem provider when -Persist is used.
                    if (Persist && provider.Name.Equals(FileSystemProvider.ProviderName, StringComparison.OrdinalIgnoreCase))
                        Root = Root.TrimEnd('/', '\\');
                    // Create the new drive
                    PSDriveInfo newDrive =
                            Root,
                            Description,
                            Persist);
                        new PSDriveInfo(
                            persist: false);
                        SessionState.Drive.New(newDrive, Scope, CmdletProviderContext);
                    catch (SessionStateException sessionStateException)
                                sessionStateException.ErrorRecord,
                                sessionStateException));
    #endregion NewPSDriveCommand
    #region DriveMatchingCoreCommandBase
    /// Base class for Drive commands that need to glob drives on both the drive name
    /// and the provider name.
    public class DriveMatchingCoreCommandBase : CoreCommandBase
        /// Globs on both the drive name and the provider name to get a list of Drives
        /// that match the glob filters.
        /// <param name="driveName">
        /// The name of the drive(s) to returned. The name can contain glob characters.
        /// <param name="providerNames">
        /// The name of the provider(s) to return. The name can contain glob characters.
        /// <param name="scope">
        /// The scope to get the drives from. If this parameter is null or empty all drives
        /// will be retrieved.
        /// A collection of the drives that match the filters.
        /// <exception cref="DriveNotFoundException"></exception>
        /// <exception cref="ProviderNotFoundException"></exception>
        /// If <paramref name="scope"/> is less than zero, or not
        /// a number and not "script", "global", "local", or "private"
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="scope"/> is less than zero or greater than the number of currently
        /// active scopes.
        internal List<PSDriveInfo> GetMatchingDrives(
             string driveName,
            string[] providerNames,
            string scope)
            List<PSDriveInfo> results = new();
            if (providerNames == null || providerNames.Length == 0)
                providerNames = new string[] { "*" };
            foreach (string providerName in providerNames)
                tracer.WriteLine("ProviderName: {0}", providerName);
                bool providerNameEmpty = string.IsNullOrEmpty(providerName);
                bool providerNameContainsWildcardCharacters =
                    WildcardPattern.ContainsWildcardCharacters(providerName);
                bool driveNameEmpty = string.IsNullOrEmpty(driveName);
                bool driveNameContainsWildcardCharacters =
                    WildcardPattern.ContainsWildcardCharacters(driveName);
                // This is just a simple check to see if the provider exists
                // if the provider name is specified without glob characters.
                // The call will throw an exception if the provider doesn't
                // exist.
                if (!providerNameEmpty && !providerNameContainsWildcardCharacters)
                    SessionState.Provider.Get(providerName);
                // This is just a simple check to see if the drive exists
                // if the drive name is specified without glob characters.
                // The call will throw an exception if the drive doesn't
                if (!driveNameEmpty && !driveNameContainsWildcardCharacters)
                    if (string.IsNullOrEmpty(scope))
                        SessionState.Drive.Get(driveName);
                        SessionState.Drive.GetAtScope(driveName, scope);
                WildcardPattern providerMatcher = null;
                PSSnapinQualifiedName pssnapinQualifiedProviderName = null;
                if (!providerNameEmpty)
                    pssnapinQualifiedProviderName = PSSnapinQualifiedName.GetInstance(providerName);
                    if (pssnapinQualifiedProviderName == null)
                        // This is a malformed pssnapin-qualified name so there is no chances for a match.
                    providerMatcher =
                        WildcardPattern.Get(
                            pssnapinQualifiedProviderName.ShortName,
                            WildcardOptions.IgnoreCase);
                WildcardPattern nameMatcher = null;
                if (!driveNameEmpty)
                    nameMatcher =
                            driveName,
                foreach (PSDriveInfo drive in SessionState.Drive.GetAllAtScope(scope))
                    bool addDrive = driveNameEmpty;
                    if (base.SuppressWildcardExpansion)
                        if (string.Equals(drive.Name, driveName, StringComparison.OrdinalIgnoreCase))
                            addDrive = true;
                        if (nameMatcher != null && nameMatcher.IsMatch(drive.Name))
                    if (addDrive)
                        // Now check to see if it matches the provider
                        if (providerNameEmpty || drive.Provider.IsMatch(providerMatcher, pssnapinQualifiedProviderName))
                            results.Add(drive);
            results.Sort();
    #endregion DriveMatchingCoreCommandBase
    #region RemovePSDriveCommand
    /// Removes a drive that is mounted in the PowerShell runspace.
    [Cmdlet(VerbsCommon.Remove, "PSDrive", DefaultParameterSetName = NameParameterSet, SupportsShouldProcess = true, SupportsTransactions = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097050")]
    public class RemovePSDriveCommand : DriveMatchingCoreCommandBase
        private const string NameParameterSet = "Name";
        private const string LiteralNameParameterSet = "LiteralName";
        /// Gets or sets the name of the drive to remove.
        [Parameter(Position = 0, ParameterSetName = NameParameterSet,
            get => _names;
            set => _names = value;
        /// Gets or sets the literal name parameter to the command.
        [Parameter(Position = 0, ParameterSetName = LiteralNameParameterSet,
        public string[] LiteralName
                _names = value;
        /// Gets or sets the name provider(s) for which the drives should be removed.
        /// Gets or sets the scope identifier from which to remove the drive.
        /// If the scope is null or empty, the scope hierarchy will be searched
        /// starting at the current scope through all the parent scopes to the
        /// global scope until a drive of the given name is found to remove.
        /// Gets or sets the force property which determines if the drive
        /// should be removed even if there were errors.
        /// The name of the drive to remove.
        private string[] _names;
        /// The name of the provider(s) for which to remove all drives.
        /// Removes the specified drive from the Monad namespace using the name
        /// of the drive.
            string action = NavigationResources.RemoveDriveConfirmAction;
            string resourceTemplate = NavigationResources.RemoveDriveConfirmResourceTemplate;
            bool verifyMatch = true;
            if (_names == null)
                _names = new string[] { string.Empty };
                verifyMatch = false;
            foreach (string driveName in _names)
                bool foundMatch = false;
                    foreach (PSDriveInfo drive in GetMatchingDrives(driveName, PSProvider, Scope))
                                drive.Name,
                                drive.Provider,
                                drive.Root);
                        foundMatch = true;
                            if (!Force && drive == SessionState.Drive.Current)
                                PSInvalidOperationException invalidOperation =
                                    (PSInvalidOperationException)PSTraceSource.NewInvalidOperationException(
                                        NavigationResources.RemoveDriveInUse,
                                        drive.Name);
                                        invalidOperation.ErrorRecord,
                                        invalidOperation));
                            SessionState.Drive.Remove(drive.Name, Force, Scope, CmdletProviderContext);
                // If a name was specified explicitly write an error if the drive wasn't
                // found
                if (verifyMatch && !foundMatch)
                    DriveNotFoundException e = new(
                        SessionStateStrings.DriveNotFound);
                    WriteError(new ErrorRecord(e.ErrorRecord, e));
    #endregion RemovePSDriveCommand
    #region GetPSDriveCommand
    /// Gets a specified or listing of drives that are mounted in PowerShell
    /// namespace.
    [Cmdlet(VerbsCommon.Get, "PSDrive", DefaultParameterSetName = NameParameterSet, SupportsTransactions = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096494")]
    [OutputType(typeof(PSDriveInfo))]
    public class GetPSDriveCommand : DriveMatchingCoreCommandBase
        /// Gets or sets the drive name the user is looking for.
        /// If the drive name is left empty, all drives will be
        /// returned. A globing or regular expression can also be
        /// supplied and any drive names that match the expression
        /// will be returned.
        [Parameter(Position = 0, ParameterSetName = NameParameterSet, ValueFromPipelineByPropertyName = true)]
            set => _name = value ?? new string[] { "*" };
                _name = value;
        /// Gets or sets the scope parameter to the command.
        /// Gets or sets the provider name for the
        /// drives that should be retrieved.
        /// If the provider is left empty, all drives will be
        /// supplied and any drive with providers that match the expression
        /// The name of the drive to be retrieved.
        private string[] _name = new string[] { "*" };
        /// The provider ID for the drives you want to see.
        /// Prepare the session for the Get-PSDrive command.
        /// Currently, auto-loads the core modules that define drives. Ideally,
        /// we could discover fake PSDriveInfo objects here based on drives exported
        /// from modules.
            SessionStateInternal.MountDefaultDrive("Cert", Context);
            SessionStateInternal.MountDefaultDrive("WSMan", Context);
        /// Retrieves the drives specified by the parameters. If the name is empty, all drives
        /// will be retrieved. If the provider is specified, only drives for that provider
            foreach (string driveName in Name)
                    List<PSDriveInfo> foundDrives = GetMatchingDrives(driveName, PSProvider, Scope);
                    if (foundDrives.Count > 0)
                        WriteObject(foundDrives, true);
                        // If no drives were found and the user was asking for a specific
                        // drive (no wildcards) then write an error
                        if (!WildcardPattern.ContainsWildcardCharacters(driveName))
                            DriveNotFoundException driveNotFound =
                                    driveNotFound,
                                    "GetDriveNoMatchingDrive",
                                    driveName));
                            driveName);
                            providerNotFound,
                catch (PSArgumentOutOfRangeException outOfRange)
                            outOfRange.ErrorRecord,
                            outOfRange));
    #endregion GetPSDriveCommand
    #endregion Drive commands
    #region Item commands
    #region GetItemCommand
    /// Gets the specified item using the namespace providers.
    [Cmdlet(VerbsCommon.Get, "Item", DefaultParameterSetName = PathParameterSet, SupportsTransactions = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096812")]
    public class GetItemCommand : CoreCommandWithCredentialsBase
        /// Gets or sets the path to item to get.
            get => _paths;
            set => _paths = value;
            get => base.Filter;
            set => base.Filter = value;
            get => base.Include;
            set => base.Include = value;
            get => base.Exclude;
            set => base.Exclude = value;
        /// Gets the dynamic parameters for the get-item cmdlet.
                return InvokeProvider.Item.GetItemDynamicParameters(Path[0], context);
            return InvokeProvider.Item.GetItemDynamicParameters(".", context);
        /// The path of the item to get.
        /// Gets the specified item.
                    InvokeProvider.Item.Get(path, CmdletProviderContext);
    #endregion GetItemCommand
    #region NewItemCommand
    /// Creates the specified item using the namespace providers.
    [Cmdlet(VerbsCommon.New, "Item", DefaultParameterSetName = PathParameterSet, SupportsShouldProcess = true, SupportsTransactions = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096592")]
    public class NewItemCommand : CoreCommandWithCredentialsBase
        private const string NameParameterSet = "nameSet";
        private const string PathParameterSet = "pathSet";
        /// Gets or sets the container path to create the item in.
        [Parameter(Position = 0, ParameterSetName = PathParameterSet, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [Parameter(Position = 0, ParameterSetName = NameParameterSet, Mandatory = false, ValueFromPipelineByPropertyName = true)]
        /// Gets or sets the name of the item to create.
        [Parameter(ParameterSetName = NameParameterSet, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        /// Gets or sets the type of the item to create.
        [Alias("Type")]
        public string ItemType { get; set; }
        /// Gets or sets the content of the item to create.
        [Parameter(ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Alias("Target")]
        public object Value { get; set; }
        /// Gets the dynamic parameters for the new-item cmdlet.
                // Path is only globbed if Name is specified.
                if (string.IsNullOrEmpty(Name))
                    return InvokeProvider.Item.NewItemDynamicParameters(WildcardPattern.Escape(Path[0]), ItemType, Value, context);
                    return InvokeProvider.Item.NewItemDynamicParameters(Path[0], ItemType, Value, context);
            return InvokeProvider.Item.NewItemDynamicParameters(".", ItemType, Value, context);
        protected override bool ProviderSupportsShouldProcess => DoesProviderSupportShouldProcess(Path);
        #endregion Command Data
        /// Creates the specified item.
                Path = new string[] { string.Empty };
                    InvokeProvider.Item.New(path, Name, ItemType, Value, CmdletProviderContext);
    #endregion NewItemCommand
    #region SetItemCommand
    /// Sets the specified item using the namespace providers.
    [Cmdlet(VerbsCommon.Set, "Item", SupportsShouldProcess = true, DefaultParameterSetName = PathParameterSet, SupportsTransactions = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097055")]
    public class SetItemCommand : CoreCommandWithCredentialsBase
        /// Gets or sets the path to item to set.
        /// Gets or sets the value of the item to be set.
        [Parameter(Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        /// Gets or sets the pass through property which determines
        /// if the object that is set should be written to the pipeline.
        /// Defaults to false.
        /// Gets the dynamic parameters for the set-item cmdlet.
                return InvokeProvider.Item.SetItemDynamicParameters(Path[0], Value, context);
            return InvokeProvider.Item.SetItemDynamicParameters(".", Value, context);
        protected override bool ProviderSupportsShouldProcess => DoesProviderSupportShouldProcess(_paths);
        /// The path of the item to set.
        /// Determines if the object being set should be written to the pipeline.
        /// Sets the specified item.
            currentCommandContext.PassThru = _passThrough;
                    InvokeProvider.Item.Set(path, Value, currentCommandContext);
    #endregion SetItemCommand
    #region RemoveItemCommand
    /// Removes the specified item using the namespace providers.
    [Cmdlet(VerbsCommon.Remove, "Item", SupportsShouldProcess = true, DefaultParameterSetName = PathParameterSet, SupportsTransactions = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097103")]
    public class RemoveItemCommand : CoreCommandWithCredentialsBase
        /// Gets or sets the recurse property.
            get => _recurse;
            set => _recurse = value;
        /// Gets the dynamic parameters for the remove-item cmdlet.
                return InvokeProvider.Item.RemoveItemDynamicParameters(Path[0], Recurse, context);
            return InvokeProvider.Item.RemoveItemDynamicParameters(".", Recurse, context);
        /// The path used when doing a delete.
        /// Determines if the remove command should recurse into
        /// sub-containers.
        /// Removes the specified items.
            bool yesToAll = false;
            bool noToAll = false;
                // Resolve the path in case it contains any glob characters
                Collection<PathInfo> resolvedPSPaths = null;
                    // Save the include and exclude filters so that we can ignore
                    // them when doing recursion
                    Collection<string> include = currentContext.Include;
                    Collection<string> exclude = currentContext.Exclude;
                    string filter = currentContext.Filter;
                    if (_recurse)
                        currentContext.SetFilters(
                            new Collection<string>(),
                        resolvedPSPaths = SessionState.Path.GetResolvedPSPathFromPSPath(path, currentContext);
                        if (SuppressWildcardExpansion == true && resolvedPSPaths.Count == 0)
                            ItemNotFoundException pathNotFound =
                                    "PathNotFound",
                                    SessionStateStrings.PathNotFound);
                        // Reset the include and exclude filters
                            include,
                            exclude,
                            filter);
                foreach (PathInfo resolvedPath in resolvedPSPaths)
                    // Check each path to make sure it isn't a parent of the current working location
                    bool isCurrentLocationOrAncestor = false;
                        isCurrentLocationOrAncestor = SessionState.Path.IsCurrentLocationOrAncestor(resolvedPath.Path, currentContext);
                    if (isCurrentLocationOrAncestor)
                                NavigationResources.RemoveItemInUse,
                                resolvedPath.Path);
                    bool hasChildren = false;
                    string providerPath = GetUnresolvedProviderPathFromPSPath(resolvedPath.Path);
                        hasChildren = SessionState.Internal.HasChildItems(resolvedPath.Provider.Name, providerPath, currentContext);
                        currentContext.ThrowFirstErrorOrDoNothing();
                    bool shouldRecurse = Recurse;
                    bool treatAsFile = false;
                    // only check if path is a directory using DirectoryInfo if using FileSystemProvider
                    if (resolvedPath.Provider.Name.Equals(FileSystemProvider.ProviderName, StringComparison.OrdinalIgnoreCase))
                            System.IO.DirectoryInfo di = new(providerPath);
                            if (InternalSymbolicLinkLinkCodeMethods.IsReparsePointLikeSymlink(di))
                                shouldRecurse = false;
                                treatAsFile = true;
                        catch (System.IO.FileNotFoundException)
                            // not a directory
                    if (!treatAsFile && !Recurse && hasChildren)
                        // Get the localized prompt string
                        string prompt = StringUtil.Format(NavigationResources.RemoveItemWithChildren, resolvedPath.Path);
                        // Confirm the user wants to remove all children and the item even if
                        // they did not specify -recurse
                        if (!ShouldContinue(prompt, null, ref yesToAll, ref noToAll))
                        shouldRecurse = true;
                    // Now do the delete
                    // This calls the internal method since it is more efficient
                    // than trying to glob again.  It also will prevent problems
                    // where globbing a second time may not have properly escaped
                    // wildcard characters in the path.
                        SessionState.Internal.RemoveItem(
                            resolvedPath.Provider.Name,
                            providerPath,
                            shouldRecurse,
    #endregion RemoveItemCommand
    #region MoveItemCommand
    /// Moves an item from the specified location to the specified destination using
    /// the namespace providers.
    [Cmdlet(VerbsCommon.Move, "Item", DefaultParameterSetName = PathParameterSet, SupportsShouldProcess = true, SupportsTransactions = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096591")]
    public class MoveItemCommand : CoreCommandWithCredentialsBase
        /// Gets or sets the destination property.
        public string Destination { get; set; } = ".";
        /// Gets the dynamic parameters for the move-item cmdlet.
                return InvokeProvider.Item.MoveItemDynamicParameters(Path[0], Destination, context);
            return InvokeProvider.Item.MoveItemDynamicParameters(".", Destination, context);
        /// The path of the item to move. It is set or retrieved via
        /// the Path property.
        private Collection<PathInfo> GetResolvedPaths(string path)
                results = SessionState.Path.GetResolvedPSPathFromPSPath(path, CmdletProviderContext);
        /// Moves the specified item to the specified destination.
                    MoveItem(path, literalPath: true);
                    Collection<PathInfo> resolvedPaths = GetResolvedPaths(path);
                    foreach (PathInfo resolvedPathInfo in resolvedPaths)
                        string resolvedPath = resolvedPathInfo.Path;
                        MoveItem(resolvedPath, literalPath: true);
        private void MoveItem(string path, bool literalPath = false)
            currentContext.SuppressWildcardExpansion = literalPath;
                if (!InvokeProvider.Item.Exists(path, currentContext))
                            NavigationResources.MoveItemDoesntExist,
                            path);
            // See if the item to be moved is in use.
                isCurrentLocationOrAncestor = SessionState.Path.IsCurrentLocationOrAncestor(path, currentContext);
                        NavigationResources.MoveItemInUse,
            tracer.WriteLine("Moving {0} to {1}", path, Destination);
                // Now do the move
                InvokeProvider.Item.Move(path, Destination, currentContext);
    #endregion MoveItemCommand
    #region RenameItemCommand
    /// Renames a specified item to a new name using the namespace providers.
    [Cmdlet(VerbsCommon.Rename, "Item", SupportsShouldProcess = true, SupportsTransactions = true, DefaultParameterSetName = ByPathParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097153")]
    public class RenameItemCommand : CoreCommandWithCredentialsBase
        private const string ByPathParameterSet = "ByPath";
        private const string ByLiteralPathParameterSet = "ByLiteralPath";
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = ByPathParameterSet)]
        /// Gets or sets the literal path property.
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = ByLiteralPathParameterSet)]
        /// Gets or sets the newName property.
        /// Gets the dynamic parameters for the rename-item cmdlet.
            return InvokeProvider.Item.RenameItemDynamicParameters(Path, NewName, context);
        protected override bool ProviderSupportsShouldProcess => DoesProviderSupportShouldProcess(new string[] { _path });
        /// The path of the item to rename. It is set or retrieved via
            Collection<PathInfo> results = null;
            if (SuppressWildcardExpansion)
                RenameItem(Path, literalPath: true);
            Collection<PathInfo> resolvedPaths = GetResolvedPaths(Path);
            if (resolvedPaths == null)
            if (resolvedPaths.Count == 1)
                RenameItem(resolvedPaths[0].Path, literalPath: true);
                RenameItem(WildcardPattern.Unescape(Path), literalPath: true);
        private void RenameItem(string path, bool literalPath = false)
                            NavigationResources.RenameItemDoesntExist,
                        NavigationResources.RenamedItemInUse,
            tracer.WriteLine("Rename {0} to {1}", path, NewName);
                // Now do the rename
                InvokeProvider.Item.Rename(path, NewName, currentContext);
    #endregion RenameItemCommand
    #region CopyItemCommand
    /// Copies a specified item to a new location using the namespace providers.
    [Cmdlet(VerbsCommon.Copy, "Item", DefaultParameterSetName = PathParameterSet, SupportsShouldProcess = true, SupportsTransactions = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096990")]
    public class CopyItemCommand : CoreCommandWithCredentialsBase
        /// Gets or sets the container property.
        public SwitchParameter Container
            get => _container;
                _containerSpecified = true;
                _container = value;
                // is, then -Container takes on the same value
                // as -Recurse
                if (!_containerSpecified)
                    _container = _recurse;
        /// Gets the dynamic parameters for the copy-item cmdlet.
                return InvokeProvider.Item.CopyItemDynamicParameters(Path[0], Destination, Recurse, context);
            return InvokeProvider.Item.CopyItemDynamicParameters(".", Destination, Recurse, context);
        /// The path of the item to copy. It is set or retrieved via
        /// Determines if the containers should be copied with the items or not.
        private bool _container = true;
        private bool _containerSpecified;
        /// Determines if the copy command should recurse into
        /// Copies the specified item(s) to the specified destination.
            currentCommandContext.PassThru = PassThru;
                tracer.WriteLine("Copy {0} to {1}", path, Destination);
                    CopyContainers copyContainers = (Container) ? CopyContainers.CopyTargetContainer : CopyContainers.CopyChildrenOfTargetContainer;
                    // Now do the copy
                    InvokeProvider.Item.Copy(path, Destination, Recurse, copyContainers, currentCommandContext);
    #endregion CopyItemCommand
    #region ClearItemCommand
    /// Clears an item at the specified location.
    [Cmdlet(VerbsCommon.Clear, "Item", DefaultParameterSetName = PathParameterSet, SupportsShouldProcess = true, SupportsTransactions = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096491")]
    public class ClearItemCommand : CoreCommandWithCredentialsBase
        /// Gets the dynamic parameters for the clear-item cmdlet.
                return InvokeProvider.Item.ClearItemDynamicParameters(Path[0], context);
            return InvokeProvider.Item.ClearItemDynamicParameters(".", context);
        /// Clears the specified item.
                tracer.WriteLine("Clearing {0}", path);
                    InvokeProvider.Item.Clear(path, currentCommandContext);
    #endregion ClearItemCommand
    #region InvokeItemCommand
    /// Invokes an item at the specified location.
    [Cmdlet(VerbsLifecycle.Invoke, "Item", DefaultParameterSetName = PathParameterSet, SupportsShouldProcess = true, SupportsTransactions = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096590")]
    public class InvokeItemCommand : CoreCommandWithCredentialsBase
        /// Gets the dynamic parameters for the invoke-item cmdlet.
                return InvokeProvider.Item.InvokeItemDynamicParameters(Path[0], context);
            return InvokeProvider.Item.InvokeItemDynamicParameters(".", context);
        /// Invokes the specified item.
                tracer.WriteLine("Invoking {0}", path);
                    // Now invoke the action
                    InvokeProvider.Item.Invoke(path, CmdletProviderContext);
    #endregion InvokeItemCommand
    #endregion Item commands
    #region Provider commands
    #region GetProviderCommand
    /// Gets a core command provider by name.
    [Cmdlet(VerbsCommon.Get, "PSProvider", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096816")]
    [OutputType(typeof(ProviderInfo))]
    public class GetPSProviderCommand : CoreCommandBase
        /// Gets or sets the provider that will be removed.
        /// The string ID of the provider to remove.
        /// Gets a provider from the core command namespace.
            if (PSProvider == null || PSProvider.Length == 0)
                // Get all the providers
                WriteObject(SessionState.Provider.GetAll(), true);
                foreach (string requestedProvider in PSProvider)
                    PSSnapinQualifiedName pssnapinQualifiedProvider = PSSnapinQualifiedName.GetInstance(requestedProvider);
                    if (pssnapinQualifiedProvider != null && WildcardPattern.ContainsWildcardCharacters(pssnapinQualifiedProvider.ShortName))
                        // The user entered a glob string so use the WildcardPattern to
                        // compare the glob string to the provider names that exist
                        // and write out any that match
                        WildcardPattern matcher =
                                pssnapinQualifiedProvider.ShortName,
                        foreach (ProviderInfo enumeratedProvider in SessionState.Provider.GetAll())
                                enumeratedProvider != null,
                                "SessionState.Providers should return only ProviderInfo objects");
                            if (enumeratedProvider.IsMatch(matcher, pssnapinQualifiedProvider))
                                // A match was found
                                WriteObject(enumeratedProvider);
                            Collection<ProviderInfo> matchingProviders =
                                SessionState.Provider.Get(requestedProvider);
                            // The provider was found
                            WriteObject(matchingProviders, true);
                                    e.ErrorRecord,
                                    e));
    #endregion GetProviderCommand
    #endregion Provider commands

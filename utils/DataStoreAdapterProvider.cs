    /// Information about a loaded Cmdlet Provider.
    /// own public members to expose to the user or to cache information related to the provider.
    public class ProviderInfo
        /// Gets the System.Type of the class that implements the provider.
        public Type ImplementingType { get; }
        /// Gets the help file path for the provider.
        public string HelpFile { get; } = string.Empty;
        /// The instance of session state the provider belongs to.
        private readonly SessionState _sessionState;
        private string _fullName;
        private string _cachedModuleName;
        /// Gets the name of the provider.
        /// Gets the full name of the provider including the module name if available.
                static string GetFullName(string name, string psSnapInName, string moduleName)
                    string result = name;
                    if (!string.IsNullOrEmpty(psSnapInName))
                                "{0}\\{1}",
                                psSnapInName,
                    // After converting core snapins to load as modules, the providers will have Module property populated
                    else if (!string.IsNullOrEmpty(moduleName))
                                moduleName,
                if (_fullName != null && ModuleName.Equals(_cachedModuleName, StringComparison.Ordinal))
                    return _fullName;
                _cachedModuleName = ModuleName;
                return _fullName = GetFullName(Name, PSSnapInName, ModuleName);
        /// Gets the Snap-in in which the provider is implemented.
        public PSSnapInInfo PSSnapIn { get; }
        /// Gets the pssnapin name that the provider is implemented in.
                if (PSSnapIn != null)
                    result = PSSnapIn.Name;
        internal string ApplicationBase
                string psHome = null;
                    psHome = Utils.DefaultPowerShellAppBase;
                    psHome = null;
                return psHome;
        /// Get the name of the module exporting this provider.
                    return PSSnapIn.Name;
                    return Module.Name;
        /// Gets the module the defined this provider.
        public PSModuleInfo Module { get; private set; }
        internal void SetModule(PSModuleInfo module)
            Module = module;
            _fullName = null;
        /// Gets or sets the description for the provider.
        /// Gets the capabilities that are implemented by the provider.
        public Provider.ProviderCapabilities Capabilities
                if (!_capabilitiesRead)
                        // Get the CmdletProvider declaration attribute
                        Type providerType = this.ImplementingType;
                        var attrs = providerType.GetCustomAttributes<CmdletProviderAttribute>(false);
                        var cmdletProviderAttributes = attrs as CmdletProviderAttribute[] ?? attrs.ToArray();
                        if (cmdletProviderAttributes.Length == 1)
                            _capabilities = cmdletProviderAttributes[0].ProviderCapabilities;
                            _capabilitiesRead = true;
                    catch (Exception) // Catch-all OK, 3rd party callout
                        // Assume no capabilities for now
                return _capabilities;
        private ProviderCapabilities _capabilities = ProviderCapabilities.None;
        private bool _capabilitiesRead;
        /// Gets or sets the home for the provider.
        /// The location can be either a fully qualified provider path
        /// or a PowerShell path. This is the location that is substituted for the ~.
        public string Home { get; set; }
        /// Gets an enumeration of drives that are available for
        /// this provider.
        public Collection<PSDriveInfo> Drives
                return _sessionState.Drive.GetAllForProvider(FullName);
        /// A hidden drive for the provider that is used for setting
        /// the location to a provider-qualified path.
        private readonly PSDriveInfo _hiddenDrive;
        /// Gets the hidden drive for the provider that is used
        /// for setting a location to a provider-qualified path.
        internal PSDriveInfo HiddenDrive
                return _hiddenDrive;
        /// Gets the string representation of the instance which is the name of the provider.
        /// The name of the provider. If single-shell, the name is pssnapin-qualified. If custom-shell,
        /// the name is just the provider name.
            return FullName;
#if USE_TLS
        /// Allocates some thread local storage to an instance of the
        /// provider. We don't want to cache a single instance of the
        /// provider because that could lead to problems in a multi-threaded
        /// environment.
        private LocalDataStoreSlot instance =
            Thread.AllocateDataSlot();
        /// Gets or sets if the drive-root relative paths on drives of this provider
        ///  are separated by a colon or not.
        /// non-windows platforms.
        /// Gets the default item separator character for this provider.
        public char ItemSeparator { get; private set; }
        /// Gets the alternate item separator character for this provider.
        public char AltItemSeparator { get; private set; }
        /// Constructs an instance of the class using an existing reference
        /// <param name="providerInfo">
        /// The provider information to copy to this instance.
        /// This constructor should be used by derived types to easily copying
        /// the base class members from an existing ProviderInfo.
        /// This is designed for use by a <see cref="System.Management.Automation.Provider.CmdletProvider"/>
        /// during calls to their <see cref="System.Management.Automation.Provider.CmdletProvider.Start(ProviderInfo)"/> method.
        /// If <paramref name="providerInfo"/> is null.
        protected ProviderInfo(ProviderInfo providerInfo)
            if (providerInfo == null)
                throw PSTraceSource.NewArgumentNullException(nameof(providerInfo));
            Name = providerInfo.Name;
            ImplementingType = providerInfo.ImplementingType;
            _capabilities = providerInfo._capabilities;
            Description = providerInfo.Description;
            _hiddenDrive = providerInfo._hiddenDrive;
            Home = providerInfo.Home;
            HelpFile = providerInfo.HelpFile;
            PSSnapIn = providerInfo.PSSnapIn;
            _sessionState = providerInfo._sessionState;
            VolumeSeparatedByColon = providerInfo.VolumeSeparatedByColon;
            ItemSeparator = providerInfo.ItemSeparator;
            AltItemSeparator = providerInfo.AltItemSeparator;
        /// Constructor for the ProviderInfo class.
        /// The instance of session state that the provider is being added to.
        /// The type that implements the provider
        /// The name of the provider.
        /// The help file for the provider.
        /// <param name="psSnapIn">
        /// The Snap-In name for the provider.
        /// If <paramref name="name"/> is null or empty.
        /// If <paramref name="implementingType"/> is null.
        internal ProviderInfo(
            SessionState sessionState,
            PSSnapInInfo psSnapIn)
            : this(sessionState, implementingType, name, string.Empty, string.Empty, helpFile, psSnapIn)
        /// The alternate name to use for the provider instead of the one specified
        /// in the .cmdletprovider file.
        /// The description of the provider.
        /// <param name="home">
        /// The home path for the provider. This must be a PowerShell path.
        /// The Snap-In for the provider.
        /// If <paramref name="implementingType"/> or <paramref name="sessionState"/> is null.
            string home,
            // Verify parameters
                throw PSTraceSource.NewArgumentNullException(nameof(implementingType));
            Home = home;
            ImplementingType = implementingType;
            HelpFile = helpFile;
            PSSnapIn = psSnapIn;
            // Create the hidden drive. The name doesn't really
            // matter since we are not adding this drive to a scope.
            _hiddenDrive =
                    this.FullName,
            _hiddenDrive.Hidden = true;
            // TODO:PSL
            // this is probably not right here
            if (implementingType == typeof(Microsoft.PowerShell.Commands.FileSystemProvider) && !Platform.IsWindows)
                VolumeSeparatedByColon = false;
        /// Determines if the passed in name is either the fully-qualified pssnapin name or
        /// short name of the provider.
        /// <param name="providerName">
        /// The name to compare with the provider name.
        /// True if the name is the fully-qualified pssnapin name or the short name of the provider.
        internal bool NameEquals(string providerName)
            PSSnapinQualifiedName qualifiedProviderName = PSSnapinQualifiedName.GetInstance(providerName);
            if (qualifiedProviderName != null)
                // If the pssnapin name and provider name are specified, then both must match
                    if (!string.IsNullOrEmpty(qualifiedProviderName.PSSnapInName))
                        if (!string.Equals(qualifiedProviderName.PSSnapInName, this.PSSnapInName, StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(qualifiedProviderName.PSSnapInName, this.ModuleName, StringComparison.OrdinalIgnoreCase))
                    result = string.Equals(qualifiedProviderName.ShortName, this.Name, StringComparison.OrdinalIgnoreCase);
                // If only the provider name is specified, then only the name must match
                result = string.Equals(providerName, Name, StringComparison.OrdinalIgnoreCase);
        internal bool IsMatch(string providerName)
            PSSnapinQualifiedName psSnapinQualifiedName = PSSnapinQualifiedName.GetInstance(providerName);
            WildcardPattern namePattern = null;
            if (psSnapinQualifiedName != null && WildcardPattern.ContainsWildcardCharacters(psSnapinQualifiedName.ShortName))
                namePattern = WildcardPattern.Get(psSnapinQualifiedName.ShortName, WildcardOptions.IgnoreCase);
            return IsMatch(namePattern, psSnapinQualifiedName);
        internal bool IsMatch(WildcardPattern namePattern, PSSnapinQualifiedName psSnapinQualifiedName)
            if (psSnapinQualifiedName == null)
                if (namePattern == null)
                    if (string.Equals(Name, psSnapinQualifiedName.ShortName, StringComparison.OrdinalIgnoreCase) &&
                        IsPSSnapinNameMatch(psSnapinQualifiedName))
                else if (namePattern.IsMatch(Name) && IsPSSnapinNameMatch(psSnapinQualifiedName))
        private bool IsPSSnapinNameMatch(PSSnapinQualifiedName psSnapinQualifiedName)
            if (string.IsNullOrEmpty(psSnapinQualifiedName.PSSnapInName) ||
                string.Equals(psSnapinQualifiedName.PSSnapInName, PSSnapInName, StringComparison.OrdinalIgnoreCase))
        /// Creates an instance of the provider.
        /// An instance of the provider or null if one could not be created.
        /// If an instance of the provider could not be created because the
        /// type could not be found in the assembly.
        internal Provider.CmdletProvider CreateInstance()
            // It doesn't really seem that using thread local storage to store an
            // instance of the provider is really much of a performance gain and it
            // still causes problems with the CmdletProviderContext when piping two
            // commands together that use the same provider.
            // get-child -filter a*.txt | get-content
            // This pipeline causes problems when using a cached provider instance because
            // the CmdletProviderContext gets changed when get-content gets called.
            // When get-content finishes writing content from the first output of get-child
            // get-child gets control back and writes out a FileInfo but the WriteObject
            // from get-content gets used because the CmdletProviderContext is still from
            // that cmdlet.
            // Possible solutions are to not cache the provider instance, or to maintain
            // a CmdletProviderContext stack in ProviderBase.  Each method invocation pushes
            // the current context and the last action of the method pops back to the
            // previous context.
            // Next see if we already have an instance in thread local storage
            object providerInstance = Thread.GetData(instance);
            if (providerInstance == null)
            object providerInstance = null;
            // Finally create an instance of the class
            Exception invocationException = null;
                providerInstance =
                    Activator.CreateInstance(this.ImplementingType);
            catch (TargetInvocationException targetException)
                invocationException = targetException.InnerException;
            catch (MissingMethodException)
            catch (MemberAccessException)
                // cache the instance in thread local storage
                Thread.SetData(instance, providerInstance);
                ProviderNotFoundException e = null;
                if (invocationException != null)
                    e =
                        new ProviderNotFoundException(
                            SessionStateCategory.CmdletProvider,
                            "ProviderCtorException",
                            SessionStateStrings.ProviderCtorException,
                            invocationException.Message);
                            "ProviderNotFoundInAssembly",
                            SessionStateStrings.ProviderNotFoundInAssembly);
            Provider.CmdletProvider result = providerInstance as Provider.CmdletProvider;
            ItemSeparator = result.ItemSeparator;
            AltItemSeparator = result.AltItemSeparator;
                "DiscoverProvider should verify that the class is derived from CmdletProvider so this is just validation of that");
            result.SetProviderInformation(this);
        /// Get the output types specified on this provider for the cmdlet requested.
        internal void GetOutputTypes(string cmdletname, List<PSTypeName> listToAppend)
            if (_providerOutputType == null)
                _providerOutputType = new Dictionary<string, List<PSTypeName>>();
                foreach (OutputTypeAttribute outputType in ImplementingType.GetCustomAttributes<OutputTypeAttribute>(false))
                    if (string.IsNullOrEmpty(outputType.ProviderCmdlet))
                    List<PSTypeName> l;
                    if (!_providerOutputType.TryGetValue(outputType.ProviderCmdlet, out l))
                        l = new List<PSTypeName>();
                        _providerOutputType[outputType.ProviderCmdlet] = l;
                    l.AddRange(outputType.Type);
            List<PSTypeName> cmdletOutputType = null;
            if (_providerOutputType.TryGetValue(cmdletname, out cmdletOutputType))
                listToAppend.AddRange(cmdletOutputType);
        private Dictionary<string, List<PSTypeName>> _providerOutputType;

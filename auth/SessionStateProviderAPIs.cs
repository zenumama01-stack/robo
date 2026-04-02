    /// Holds the state of a PowerShell session.
        /// A collection of the providers. Any provider in this collection can
        /// have drives in any scope in session state.
        internal Dictionary<string, List<ProviderInfo>> Providers
                if (this == ExecutionContext.TopLevelSessionState)
                return ExecutionContext.TopLevelSessionState.Providers;
        private Dictionary<string, List<ProviderInfo>> _providers =
            new Dictionary<string, List<ProviderInfo>>(
                    SessionStateConstants.DefaultDictionaryCapacity, StringComparer.OrdinalIgnoreCase);
        /// Stores the current working drive for each provider. This
        /// allows for retrieving the current working directory for each
        /// individual provider.
        internal Dictionary<ProviderInfo, PSDriveInfo> ProvidersCurrentWorkingDrive
                    return _providersCurrentWorkingDrive;
                return ExecutionContext.TopLevelSessionState.ProvidersCurrentWorkingDrive;
        private readonly Dictionary<ProviderInfo, PSDriveInfo> _providersCurrentWorkingDrive = new Dictionary<ProviderInfo, PSDriveInfo>();
        /// Entrypoint used by to add a provider to the current session state
        /// based on a SessionStateProviderEntry.
        /// <param name="providerEntry"></param>
        internal void AddSessionStateEntry(SessionStateProviderEntry providerEntry)
            AddProvider(providerEntry.ImplementingType,
                        providerEntry.Name,
                        providerEntry.HelpFileName,
                        providerEntry.PSSnapIn,
                        providerEntry.Module);
        private ProviderInfo AddProvider(Type implementingType, string name, string helpFileName, PSSnapInInfo psSnapIn, PSModuleInfo module)
                provider =
                    new ProviderInfo(
                        new SessionState(this),
                        implementingType,
                        helpFileName,
                        psSnapIn);
                provider.SetModule(module);
                NewProvider(provider);
                // Log the provider start event
                MshLog.LogProviderLifecycleEvent(
                    ProviderState.Started);
                if (sessionStateException.GetType() == typeof(SessionStateException))
                    // NTRAID#Windows OS Bugs-1009281-2004/02/11-JeffJon
                    this.ExecutionContext.ReportEngineStartupError(sessionStateException);
                this.ExecutionContext.ReportEngineStartupError(e);
            return provider;
        /// Determines the appropriate provider for the drive and then calls the NewDrive
        /// method of that provider.
        /// The drive to have the provider verify.
        /// The command context under which the drive is being added.
        /// <param name="resolvePathIfPossible">
        /// If true, the drive root will be resolved as an MSH path before verifying with
        /// the provider. If false, the path is assumed to be a provider-internal path.
        /// The instance of the drive to be added as approved by the provider.
        /// If the provider throws an exception while validating the drive.
        private PSDriveInfo ValidateDriveWithProvider(PSDriveInfo drive, CmdletProviderContext context, bool resolvePathIfPossible)
                drive != null,
                "drive should have been validated by the caller");
            DriveCmdletProvider namespaceProvider =
            return ValidateDriveWithProvider(namespaceProvider, drive, context, resolvePathIfPossible);
        private PSDriveInfo ValidateDriveWithProvider(
            DriveCmdletProvider driveProvider,
            bool resolvePathIfPossible)
                driveProvider != null,
                "driveProvider should have been validated by the caller");
            // Mark the drive as being created so that the provider can modify the
            // root if necessary
            drive.DriveBeingCreated = true;
            // Only try to resolve the root as an MSH path if there is a current drive.
            if (CurrentDrive != null && resolvePathIfPossible)
                string newRoot = GetProviderRootFromSpecifiedRoot(drive.Root, drive.Provider);
                if (newRoot != null)
                    drive.SetRoot(newRoot);
                result = driveProvider.NewDrive(drive, context);
                ProviderInvocationException pie =
                        "NewDriveProviderException",
                        SessionStateStrings.NewDriveProviderException,
                        pie.ErrorRecord,
                        pie));
                drive.DriveBeingCreated = false;
        /// Gets an instance of a provider given the provider ID.
        /// The identifier for the provider to return an instance of.
        /// An instance of the specified provider.
        /// If <paramref name="providerId"/> is null.
        /// If the <paramref name="providerId"/> refers to a provider that doesn't exist or
        /// the name passed matched multiple providers.
        internal Provider.CmdletProvider GetProviderInstance(string providerId)
            ProviderInfo provider = GetSingleProvider(providerId);
            return GetProviderInstance(provider);
        /// Gets an instance of a provider given the provider information.
        /// The provider to return an instance of.
        /// If <paramref name="provider"/> is null.
        internal Provider.CmdletProvider GetProviderInstance(ProviderInfo provider)
            return provider.CreateInstance();
        /// Creates an exception for the case where the provider name matched multiple providers.
        /// <param name="matchingProviders">
        /// The ProviderInfo of the possible matches.
        /// An exception representing the error with a message stating which providers are possible matches.
        internal static ProviderNameAmbiguousException NewAmbiguousProviderName(string name, Collection<ProviderInfo> matchingProviders)
            string possibleMatches = GetPossibleMatches(matchingProviders);
            ProviderNameAmbiguousException e =
                new ProviderNameAmbiguousException(
                    "ProviderNameAmbiguous",
                    SessionStateStrings.ProviderNameAmbiguous,
                    matchingProviders,
        private static string GetPossibleMatches(Collection<ProviderInfo> matchingProviders)
            foreach (ProviderInfo matchingProvider in matchingProviders)
                possibleMatches.Append(' ');
                possibleMatches.Append(matchingProvider.FullName);
            return possibleMatches.ToString();
        /// Gets an instance of an DriveCmdletProvider given the provider ID.
        /// The provider ID of the provider to get an instance of.
        /// An instance of a DriveCmdletProvider for the specified provider ID.
        /// if <paramref name="providerId"/> is null.
        /// if the <paramref name="providerId"/> is not for a provider
        /// that is derived from NavigationCmdletProvider.
        /// If the <paramref name="providerId"/> refers to a provider that doesn't exist.
        internal DriveCmdletProvider GetDriveProviderInstance(string providerId)
            if (GetProviderInstance(providerId) is not DriveCmdletProvider driveCmdletProvider)
            return driveCmdletProvider;
        /// Gets an instance of an DriveCmdletProvider given the provider information.
        /// The provider to get an instance of.
        /// An instance of a DriveCmdletProvider for the specified provider.
        /// if <paramref name="provider"/> is null.
        /// if the <paramref name="provider"/> is not for a provider
        internal DriveCmdletProvider GetDriveProviderInstance(ProviderInfo provider)
            if (GetProviderInstance(provider) is not DriveCmdletProvider driveCmdletProvider)
        /// if <paramref name="providerInstance"/> is null.
        /// if the <paramref name="providerInstance"/> is not for a provider
        /// that is derived from DriveCmdletProvider.
        private static DriveCmdletProvider GetDriveProviderInstance(CmdletProvider providerInstance)
                throw PSTraceSource.NewArgumentNullException(nameof(providerInstance));
            if (providerInstance is not DriveCmdletProvider driveCmdletProvider)
        /// Gets an instance of an ItemCmdletProvider given the provider ID.
        /// An instance of a ItemCmdletProvider for the specified provider ID.
        internal ItemCmdletProvider GetItemProviderInstance(string providerId)
            if (GetProviderInstance(providerId) is not ItemCmdletProvider itemCmdletProvider)
                    PSTraceSource.NewNotSupportedException(SessionStateStrings.ItemCmdletProvider_NotSupported);
            return itemCmdletProvider;
        /// Gets an instance of an ItemCmdletProvider given the provider.
        /// An instance of a ItemCmdletProvider for the specified provider.
        internal ItemCmdletProvider GetItemProviderInstance(ProviderInfo provider)
            if (GetProviderInstance(provider) is not ItemCmdletProvider itemCmdletProvider)
        /// that is derived from ItemCmdletProvider.
        private static ItemCmdletProvider GetItemProviderInstance(CmdletProvider providerInstance)
            if (providerInstance is not ItemCmdletProvider itemCmdletProvider)
        /// Gets an instance of an ContainerCmdletProvider given the provider ID.
        /// An instance of a ContainerCmdletProvider for the specified provider ID.
        internal ContainerCmdletProvider GetContainerProviderInstance(string providerId)
            if (GetProviderInstance(providerId) is not ContainerCmdletProvider containerCmdletProvider)
                    PSTraceSource.NewNotSupportedException(SessionStateStrings.ContainerCmdletProvider_NotSupported);
            return containerCmdletProvider;
        /// Gets an instance of an ContainerCmdletProvider given the provider.
        /// An instance of a ContainerCmdletProvider for the specified provider.
        internal ContainerCmdletProvider GetContainerProviderInstance(ProviderInfo provider)
            if (GetProviderInstance(provider) is not ContainerCmdletProvider containerCmdletProvider)
        /// that is derived from ContainerCmdletProvider.
        private static ContainerCmdletProvider GetContainerProviderInstance(CmdletProvider providerInstance)
            if (providerInstance is not ContainerCmdletProvider containerCmdletProvider)
        /// Gets an instance of an NavigationCmdletProvider given the provider.
        /// An instance of a NavigationCmdletProvider for the specified provider ID.
        internal NavigationCmdletProvider GetNavigationProviderInstance(ProviderInfo provider)
            if (GetProviderInstance(provider) is not NavigationCmdletProvider navigationCmdletProvider)
                    PSTraceSource.NewNotSupportedException(SessionStateStrings.NavigationCmdletProvider_NotSupported);
            return navigationCmdletProvider;
        /// Gets an instance of an NavigationCmdletProvider given the provider ID.
        private static NavigationCmdletProvider GetNavigationProviderInstance(CmdletProvider providerInstance, bool acceptNonContainerProviders)
                providerInstance as NavigationCmdletProvider;
            if ((navigationCmdletProvider == null) && (!acceptNonContainerProviders))
        #region GetProvider
        /// Determines if the specified CmdletProvider is loaded.
        /// The name of the CmdletProvider.
        /// true if the CmdletProvider is loaded, or false otherwise.
        internal bool IsProviderLoaded(string name)
            // Get the provider from the providers container
                ProviderInfo providerInfo = GetSingleProvider(name);
                result = providerInfo != null;
        /// Gets the provider of the specified name.
        /// The name of the provider to retrieve
        /// The provider of the given name
        /// The provider with the specified <paramref name="name"/>
        internal Collection<ProviderInfo> GetProvider(string name)
            PSSnapinQualifiedName providerName = PSSnapinQualifiedName.GetInstance(name);
            if (providerName == null)
                ProviderNotFoundException e =
                     "ProviderNotFoundBadFormat",
                     SessionStateStrings.ProviderNotFoundBadFormat);
            return GetProvider(providerName);
        /// could not be found or the name was ambiguous.
        /// If the name is ambiguous then the PSSnapin qualified name must
        /// be specified.
        internal ProviderInfo GetSingleProvider(string name)
            Collection<ProviderInfo> matchingProviders = GetProvider(name);
            if (matchingProviders.Count != 1)
                if (matchingProviders.Count == 0)
                            "ProviderNotFound",
                            SessionStateStrings.ProviderNotFound);
                    throw NewAmbiguousProviderName(name, matchingProviders);
            return matchingProviders[0];
        internal Collection<ProviderInfo> GetProvider(PSSnapinQualifiedName providerName)
            Collection<ProviderInfo> result = new Collection<ProviderInfo>();
            List<ProviderInfo> matchingProviders = null;
            if (!Providers.TryGetValue(providerName.ShortName, out matchingProviders))
                // If the provider was not found, we may need to auto-mount it.
                SessionStateInternal.MountDefaultDrive(providerName.ShortName, ExecutionContext);
                            providerName.ToString(),
            if (!string.IsNullOrEmpty(providerName.PSSnapInName))
                // Be sure the PSSnapin/Module name matches
                foreach (ProviderInfo provider in matchingProviders)
                            provider.PSSnapInName,
                            providerName.PSSnapInName,
                           StringComparison.OrdinalIgnoreCase) ||
                            provider.ModuleName,
                        result.Add(provider);
        /// Gets all the CoreCommandProviders.
        internal IEnumerable<ProviderInfo> ProviderList
                foreach (List<ProviderInfo> providerValues in Providers.Values)
                    foreach (ProviderInfo provider in providerValues)
        /// Copy the Providers from another session state instance...
        /// <param name="ss">The session state instance to copy from...</param>
        internal void CopyProviders(SessionStateInternal ss)
            if (ss == null || ss.Providers == null)
            // private Dictionary<string, List<ProviderInfo>> providers;
            _providers = new Dictionary<string, List<ProviderInfo>>();
            foreach (KeyValuePair<string, List<ProviderInfo>> e in ss._providers)
                _providers.Add(e.Key, e.Value);
        #endregion GetProvider
        #region NewProvider
        /// Initializes a provider by loading the assembly, creating an instance of the
        /// provider, calling its start method followed by the InitializeDefaultDrives method. The
        /// Drives that are returned from the InitializeDefaultDrives method are then mounted.
        /// An instance of the provider to use for the initialization.
        /// The provider to be initialized.
        /// The context under which the initialization is occurring. If this parameter is not
        /// null, errors will be written to the WriteError method of the context.
        /// If <paramref name="provider"/> or <paramref name="context"/> is null.
        /// <exception cref="SessionStateException">
        /// If a drive already exists for the name of one of the drives the
        /// provider tries to add.
        internal void InitializeProvider(
            Provider.CmdletProvider providerInstance,
            // Initialize the provider so that it can add any drives
            // that it needs.
            List<PSDriveInfo> newDrives = new List<PSDriveInfo>();
                GetDriveProviderInstance(providerInstance);
                    Collection<PSDriveInfo> drives = driveProvider.InitializeDefaultDrives(context);
                    if (drives != null && drives.Count > 0)
                        newDrives.AddRange(drives);
                        ProvidersCurrentWorkingDrive[provider] = drives[0];
                            "InitializeDefaultDrivesException",
                            SessionStateStrings.InitializeDefaultDrivesException,
                            provider));
            if (newDrives != null && newDrives.Count > 0)
                // Add the drives.
                foreach (PSDriveInfo newDrive in newDrives)
                    if (newDrive == null)
                    // Only mount drives for the current provider
                    if (!provider.NameEquals(newDrive.Provider.FullName))
                        PSDriveInfo validatedNewDrive = ValidateDriveWithProvider(driveProvider, newDrive, context, false);
                        if (validatedNewDrive != null)
                            // Since providers are global then the drives created
                            // through InitializeDefaultDrives should also be global.
                            GlobalScope.NewDrive(validatedNewDrive);
                    catch (SessionStateException exception)
                        context.WriteError(exception.ErrorRecord);
        /// Creates and adds a provider to the provider container.
        /// The provider that was added or null if the provider failed to be added.
        /// If the provider already exists.
        /// If there was a failure to load the provider or the provider
        /// threw an exception.
        internal ProviderInfo NewProvider(ProviderInfo provider)
            // Check to see if the provider already exists.
            // We do the check instead of allowing the hashtable to
            // throw the exception so that we give a better error
            // message.
            ProviderInfo existingProvider = ProviderExists(provider);
            if (existingProvider != null)
                // If it's an already loaded provider, don't return an error...
                if (existingProvider.ImplementingType == provider.ImplementingType)
                    return existingProvider;
                        "CmdletProviderAlreadyExists",
                        SessionStateStrings.CmdletProviderAlreadyExists,
                throw sessionStateException;
            // Make sure we are able to create an instance of the provider.
            // Note, this will also set the friendly name if the user didn't
            // specify one.
            // Now call start to let the provider initialize itself
            ProviderInfo newProviderInfo = null;
                newProviderInfo = providerInstance.Start(provider, context);
                // Set the new provider info in the instance in case the provider
                // derived a new one
                providerInstance.SetProviderInformation(newProviderInfo);
            catch (Exception e) // Catch-call OK, 3rd party callout
                        "ProviderStartException",
                        SessionStateStrings.ProviderStartException,
            if (newProviderInfo == null)
                        SessionStateStrings.InvalidProviderInfoNull);
            if (newProviderInfo != provider)
                // Since the references are not the same, ensure that the provider
                // name is the same.
                if (!string.Equals(newProviderInfo.Name, provider.Name, StringComparison.OrdinalIgnoreCase))
                            SessionStateStrings.InvalidProviderInfo);
                // Use the new provider info instead
                provider = newProviderInfo;
            // Add the newly create provider to the providers container
                NewProviderEntry(provider);
            // Add the provider to the provider current working
            // drive hashtable so that we can associate a current working
            // drive with it.
            ProvidersCurrentWorkingDrive.Add(provider, null);
            bool initializeProviderError = false;
                // Initialize the provider and give it a chance to
                // mount some drives.
                InitializeProvider(providerInstance, provider, context);
                initializeProviderError = true;
                // We can safely ignore NotSupportedExceptions because
                // it just means that the provider doesn't support
                // drives.
                initializeProviderError = false;
                if (initializeProviderError)
                    // An exception during initialization should remove the provider from
                    // session state.
                    Providers.Remove(provider.Name);
                    ProvidersCurrentWorkingDrive.Remove(provider);
            // Now write out the result
        private ProviderInfo ProviderExists(ProviderInfo provider)
            if (Providers.TryGetValue(provider.Name, out matchingProviders))
                foreach (ProviderInfo possibleMatch in matchingProviders)
                    if (provider.NameEquals(possibleMatch.FullName))
                        return possibleMatch;
        /// Creates an entry in the providers hashtable for the new provider.
        /// The provider being added.
        /// If a provider with the same name and PSSnapIn name already exists.
        private void NewProviderEntry(ProviderInfo provider)
            bool isDuplicateProvider = false;
            // Add the entry to the list of providers with that name
            if (!Providers.ContainsKey(provider.Name))
                Providers.Add(provider.Name, new List<ProviderInfo>());
                // be sure the same provider from the same PSSnapin doesn't already exist
                List<ProviderInfo> existingProviders = Providers[provider.Name];
                foreach (ProviderInfo existingProvider in existingProviders)
                    // making sure that we are not trying to add the same provider by checking the provider name & type of the new and existing providers.
                    if (string.IsNullOrEmpty(provider.PSSnapInName) && (string.Equals(existingProvider.Name, provider.Name, StringComparison.OrdinalIgnoreCase) &&
                        (existingProvider.GetType().Equals(provider.GetType()))))
                        isDuplicateProvider = true;
                    // making sure that we are not trying to add the same provider by checking the PSSnapinName of the new and existing providers.
                    else if (string.Equals(existingProvider.PSSnapInName, provider.PSSnapInName, StringComparison.OrdinalIgnoreCase))
            if (!isDuplicateProvider)
                Providers[provider.Name].Add(provider);
        #endregion NewProvider
        #region Remove Provider
        /// Removes the provider of the given name.
        /// The name of the provider to remove.
        /// Determines if the provider should be removed forcefully even if there were
        /// drives present or errors.
        /// The context under which the command is being run.
        /// <error cref="ArgumentNullException">
        /// </error>
        /// <error cref="SessionStateException">
        /// There are still drives associated with this provider,
        /// and the "force" option was not specified.
        /// <error cref="ProviderNotFoundException">
        /// A provider with name <paramref name="providerName"/> could not be found.
        /// <error>
        /// If a provider throws an exception it gets written to the <paramref name="context"/>.
        /// If <paramref name="providerName"/> is null or empty.
        /// All drives associated with the provider must be removed before the provider
        /// can be removed. Call SessionState.GetDrivesForProvider() to determine if there
        /// are any drives associated with the provider. A SessionStateException
        /// will be written to the context if any such drives do exist.
        internal void RemoveProvider(
            string providerName,
            if (string.IsNullOrEmpty(providerName))
                throw PSTraceSource.NewArgumentException(nameof(providerName));
            bool errors = false;
                provider = GetSingleProvider(providerName);
                // First get an instance of the provider to make sure it exists
                Provider.CmdletProvider providerBase = GetProviderInstance(provider);
                if (providerBase == null)
                    ProviderNotFoundException e = new ProviderNotFoundException(
                        providerName,
                    errors = true;
                    // See if there are any drives present for the provider
                    int driveCount = 0;
                    foreach (PSDriveInfo drive in GetDrivesForProvider(providerName))
                            ++driveCount;
                    if (driveCount > 0)
                        if (force)
                            // Forcefully remove all the drives
                                    RemoveDrive(drive, true, null);
                            // Since there are still drives associated with the provider
                            // the provider cannot be removed
                            SessionStateException e = new SessionStateException(
                                "RemoveDrivesBeforeRemovingProvider",
                                SessionStateStrings.RemoveDrivesBeforeRemovingProvider,
                    // Now tell the provider that they are going to be removed by
                    // calling the Stop method
                        providerBase.Stop(context);
                        "RemoveProviderUnexpectedException",
                        providerName));
                if (force || !errors)
                    // Log the provider stopped event
                        ProviderState.Stopped);
                    RemoveProviderFromCollection(provider);
        /// Removes the provider from the providers dictionary.
        /// The provider to be removed.
        /// If there are multiple providers with the same name, then only the provider
        /// from the matching PSSnapin is removed.
        /// If the last provider of that name is removed the entry is removed from the dictionary.
        private void RemoveProviderFromCollection(ProviderInfo provider)
            List<ProviderInfo> matchingProviders;
                if (matchingProviders.Count == 1 &&
                    matchingProviders[0].NameEquals(provider.FullName))
                    matchingProviders.Remove(provider);
        #endregion RemoveProvider
        internal int ProviderCount
                foreach (List<ProviderInfo> matchingProviders in Providers.Values)
                    count += matchingProviders.Count;

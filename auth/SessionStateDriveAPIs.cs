        /// The currently active drive. It determines the current working directory.
        private PSDriveInfo _currentDrive;
        #region NewDrive
        /// Adds the specified drive to the current scope.
        /// The drive to be added to the current scope.
        /// The ID for the scope to add the drive to. The scope ID can be any of the
        /// "special" scope identifiers like "global", "local", or "private" or it
        /// can be a numeric identifier that is a count of the number of parent
        /// scopes up from the current scope to put the drive in.
        /// If this parameter is null or empty the drive will be placed in the
        /// current scope.
        /// The drive that was added, if any.
        internal PSDriveInfo NewDrive(PSDriveInfo drive, string scopeID)
            PSDriveInfo result = null;
            // Construct a CmdletProviderContext and call the override
            NewDrive(drive, scopeID, context);
            Collection<PSObject> successObjects = context.GetAccumulatedObjects();
            if (successObjects != null &&
                successObjects.Count > 0)
                    successObjects.Count == 1,
                    "NewDrive should only add one PSDriveInfo object to the pipeline");
                // set the return value to the first drive (should only be one).
                if (!successObjects[0].ImmediateBaseObjectIsEmpty)
                    result = (PSDriveInfo)successObjects[0].BaseObject;
        /// Adds a drive to the PowerShell namespace.
        /// The new drive to be added.
        internal void NewDrive(PSDriveInfo drive, string scopeID, CmdletProviderContext context)
            if (!IsValidDriveName(drive.Name))
                        "drive.Name",
                        SessionStateStrings.DriveNameIllegalCharacters);
            // Allow the provider a chance to approve the drive and set
            // provider specific data
            PSDriveInfo result = ValidateDriveWithProvider(drive, context, true);
            // We assume that the provider wrote the error message as they
            // are suppose to.
            if (string.Equals(result.Name, drive.Name, StringComparison.OrdinalIgnoreCase))
                // Set the drive in the current scope.
                    SessionStateScope scope = _currentScope;
                    if (!string.IsNullOrEmpty(scopeID))
                        scope = GetScopeByID(scopeID);
                    scope.NewDrive(result);
                    // Wrap up the exception and write it to the error stream
                            "NewDriveError",
                            result));
                catch (SessionStateException)
                    // This should be a pipeline terminating condition
                if (ProvidersCurrentWorkingDrive[drive.Provider] == null)
                    // Set the new drive as the current
                    // drive for the provider since there isn't one set.
                    ProvidersCurrentWorkingDrive[drive.Provider] = drive;
                // Upon success, write the drive to the pipeline
                context.WriteObject(result);
                ProviderInvocationException e =
                    NewProviderInvocationException(
                        "NewDriveProviderFailed",
                        SessionStateStrings.NewDriveProviderFailed,
                        drive.Root,
                        PSTraceSource.NewArgumentException("root"));
        private static bool IsValidDriveName(string name)
            const string CharactersInvalidInDriveName = ":/\\.~";
            return !string.IsNullOrEmpty(name)
                && name.AsSpan().IndexOfAny(CharactersInvalidInDriveName) < 0;
        /// Tries to resolve the drive root as an MSH path. If it successfully resolves
        /// to a single path then the resolved provider internal path is returned. If it
        /// does not resolve to a single MSH path the root is returned as it was passed.
        /// The root path of the drive to be resolved.
        /// The provider that should be used when resolving the path.
        /// The new root path of the drive.
        private string GetProviderRootFromSpecifiedRoot(string root, ProviderInfo provider)
                root != null,
                "Caller should have verified the root");
                provider != null,
                "Caller should have verified the provider");
            string result = root;
            SessionState sessionState = new SessionState(ExecutionContext.TopLevelSessionState);
            Collection<string> resolvedPaths = null;
            ProviderInfo resolvedProvider = null;
                // First try to resolve the root as an MSH path
                    sessionState.Path.GetResolvedProviderPathFromPSPath(root, out resolvedProvider);
                // If a single path was resolved...
                if (resolvedPaths != null &&
                    resolvedPaths.Count == 1)
                    // and the provider used to resolve the path,
                    // matches the one specified by the drive...
                    if (provider.NameEquals(resolvedProvider.FullName))
                        // and the item exists
                        ProviderIntrinsics providerIntrinsics =
                            new ProviderIntrinsics(this);
                        if (providerIntrinsics.Item.Exists(root))
                            // then use the resolved path as the root of the drive
                            result = resolvedPaths[0];
            // If any of the following exceptions are thrown we assume that
            // the path is a file system path not an MSH path and try
            // to create the drive with that root.
        internal object NewDriveDynamicParameters(string providerId, CmdletProviderContext context)
                // If the provider hasn't been specified yet, just return null.
                // The provider can be specified as pipeline input.
            DriveCmdletProvider provider = GetDriveProviderInstance(providerId);
                result = provider.NewDriveDynamicParameters(context);
                        "NewDriveDynamicParametersProviderException",
                        SessionStateStrings.NewDriveDynamicParametersProviderException,
                        provider.ProviderInfo,
        #endregion NewDrive
        #region GetDrive
        /// Searches through the session state scopes to find a drive.
        /// The name of a drive to find.
        /// The drive information if the drive is found.
        /// If there is no drive with <paramref name="name"/>.
        internal PSDriveInfo GetDrive(string name)
            return GetDrive(name, true);
        private PSDriveInfo GetDrive(string name, bool automount)
            // Start searching through the scopes for the drive until the drive
            // is found or the global scope is reached.
            SessionStateScopeEnumerator scopeEnumerator = new SessionStateScopeEnumerator(CurrentScope);
            int scopeID = 0;
            foreach (SessionStateScope processingScope in scopeEnumerator)
                result = processingScope.GetDrive(name);
                    if (result.IsAutoMounted)
                        // Validate or remove the auto-mounted drive
                        if (!ValidateOrRemoveAutoMountedDrive(result, processingScope))
                        s_tracer.WriteLine("Drive found in scope {0}", scopeID);
                // Increment the scope ID
                ++scopeID;
            if (result == null && automount)
                // Attempt to automount as a file system drive
                // or as a BuiltIn drive (e.g. "Cert"/"Certificate"/"WSMan")
                result = AutomountFileSystemDrive(name) ?? AutomountBuiltInDrive(name);
                    new DriveNotFoundException(
                throw driveNotFound;
        /// Searches through the session state scopes looking
        /// for a drive of the specified name.
        /// The name of the drive to return.
        /// The scope ID of the scope to look in for the drive.
        /// If this parameter is null or empty the drive will be
        /// found by searching the scopes using the dynamic scoping
        /// rules.
        /// The drive for the given name in the given scope or null if
        /// the drive was not found.
        internal PSDriveInfo GetDrive(string name, string scopeID)
            // The scope ID wasn't defined or wasn't recognizable
            // so do a search through the scopes looking for the
            // drive.
            if (string.IsNullOrEmpty(scopeID))
                    new SessionStateScopeEnumerator(CurrentScope);
                    result = scope.GetDrive(name);
                            if (!ValidateOrRemoveAutoMountedDrive(result, scope))
                    result = AutomountFileSystemDrive(name);
                    if (scope == GlobalScope)
        private PSDriveInfo AutomountFileSystemDrive(string name)
            // Check to see if it could be a "auto-mounted"
            // file system drive.  If so, add the new drive
            // to the global scope and return it
            if (name.Length == 1)
                    System.IO.DriveInfo driveInfo = new System.IO.DriveInfo(name);
                    result = AutomountFileSystemDrive(driveInfo);
                    // Catch all exceptions and continue since the drive does not exist
                    // This action wasn't requested by the user and as such we don't
                    // want to expose the user to any error conditions other than
                    // DriveNotFoundException which will be thrown by the caller
        private PSDriveInfo AutomountFileSystemDrive(System.IO.DriveInfo systemDriveInfo)
            if (!IsProviderLoaded(this.ExecutionContext.ProviderNames.FileSystem))
                s_tracer.WriteLine("The {0} provider is not loaded", this.ExecutionContext.ProviderNames.FileSystem);
            // Since the drive does exist, add it.
                // Get the FS provider
                DriveCmdletProvider driveProvider =
                    GetDriveProviderInstance(this.ExecutionContext.ProviderNames.FileSystem);
                if (driveProvider != null)
                    // Create a new drive
                    string systemDriveName = systemDriveInfo.Name.Substring(0, 1);
                    string volumeLabel = string.Empty;
                    string displayRoot = null;
                        // When run in an AppContainer, we may not have access to the volume label.
                        volumeLabel = systemDriveInfo.VolumeLabel;
                    // Get the actual root path for Network type drives
                    if (systemDriveInfo.DriveType == DriveType.Network)
                            displayRoot = Microsoft.PowerShell.Commands.FileSystemProvider
                                            .GetRootPathForNetworkDriveOrDosDevice(systemDriveInfo);
                        // We want to get root path of the network drive as extra information to display to the user.
                        // It's okay we failed to get the root path for some reason. We don't want to throw exception
                        // here as it would break the current behavior.
                    PSDriveInfo newPSDriveInfo =
                            systemDriveName,
                            driveProvider.ProviderInfo,
                            systemDriveInfo.RootDirectory.FullName,
                            volumeLabel,
                            displayRoot);
                    newPSDriveInfo.IsAutoMounted = true;
                    newPSDriveInfo.DriveBeingCreated = true;
                    // Validate the drive with the provider
                    result = ValidateDriveWithProvider(driveProvider, newPSDriveInfo, context, false);
                    newPSDriveInfo.DriveBeingCreated = false;
                    if (result != null && !context.HasErrors())
                        // Create the drive in the global scope.
                        GlobalScope.NewDrive(result);
                // Since the user isn't expecting this behavior, we don't
                // want to let errors find their way out. If there are any
                // failures we just don't mount the drive.
                    this.ExecutionContext,
                    this.ExecutionContext.ProviderNames.FileSystem,
        /// Auto-mounts a built-in drive.
        /// Calls GetDrive(name, false) internally.
        /// <param name="name">The name of the drive to load.</param>
        internal PSDriveInfo AutomountBuiltInDrive(string name)
            MountDefaultDrive(name, ExecutionContext);
            PSDriveInfo result = GetDrive(name, false);
        /// Automatically mount the specified drive.
        /// Neither 'WSMan' nor 'Certificate' provider works in UNIX PS today.
        /// So this method currently does nothing on UNIX.
        internal static void MountDefaultDrive(string name, ExecutionContext context)
            PSModuleAutoLoadingPreference moduleAutoLoadingPreference =
                CommandDiscovery.GetCommandDiscoveryPreference(context, SpecialVariables.PSModuleAutoLoadingPreferenceVarPath, "PSModuleAutoLoadingPreference");
            if (moduleAutoLoadingPreference == PSModuleAutoLoadingPreference.None)
            // Note: For the certificate provider, we actually support the provider name as an alternative to
            // mount the default drive, since the provider names can be used for provider-qualified paths.
            // The WSMAN drive is the same as the provider name.
                string.Equals("Cert", name, StringComparison.OrdinalIgnoreCase) ||
                string.Equals("Certificate", name, StringComparison.OrdinalIgnoreCase)
                moduleName = "Microsoft.PowerShell.Security";
            else if (string.Equals("WSMan", name, StringComparison.OrdinalIgnoreCase))
                moduleName = "Microsoft.WSMan.Management";
                s_tracer.WriteLine("Auto-mounting built-in drive: {0}", name);
                CommandInfo commandInfo = new CmdletInfo("Import-Module", typeof(Microsoft.PowerShell.Commands.ImportModuleCommand), null, null, context);
                CommandDiscovery.AutoloadSpecifiedModule(moduleName, context, commandInfo.Visibility, out exception);
        /// Determines if the specified automounted drive still exists. If not,
        /// the drive is removed.
        /// The drive to validate or remove.
        /// The scope the drive is in.  This will be used to remove the drive
        /// if necessary.
        /// True if the drive is still valid, false if the drive was removed.
        private bool ValidateOrRemoveAutoMountedDrive(PSDriveInfo drive, SessionStateScope scope)
                System.IO.DriveInfo systemDriveInfo = new System.IO.DriveInfo(drive.Name);
                result = systemDriveInfo.DriveType != DriveType.NoRootDirectory;
                // Assume any exception means the drive is no longer valid and needs
                // to be removed.
                DriveCmdletProvider driveProvider = null;
                    driveProvider =
                        // Give the provider a chance to cleanup
                        driveProvider.RemoveDrive(drive, context);
                    // Ignore any exceptions the provider throws because we
                    // are doing this without an explicit request from the
                    // user. Since the provider can throw any exception
                    // we must catch all exceptions here.
                    scope.RemoveDrive(drive);
        /// If a VHD is mounted to a drive prior to the PowerShell session being launched,
        /// then such a drive has to be validated for its existence before performing
        /// any operations on that drive to make sure that the drive is not unmounted.
        /// <returns>Absence of mounted drive for FileSystem provider or False for other provider types.</returns>
        private bool IsAStaleVhdMountedDrive(PSDriveInfo drive)
            // check that drive's provider type is FileSystem
            if ((drive.Provider != null) && (!drive.Provider.NameEquals(this.ExecutionContext.ProviderNames.FileSystem)))
            // A VHD mounted drive gets detected with a DriveType of DriveType.Fixed
            // when the VHD is mounted, however if the drive is unmounted, such a
            // stale drive is no longer valid and gets detected with DriveType.NoRootDirectory.
            // We would hit this situation in the following scenario:
            //  1. Launch Powershell session 'A' and mount the VHD.
            //  2. Launch different powershell session 'B'.
            //  3. Unmount the VHD in session 'A'.
            // The drive pointing to VHD in session 'B' gets detected as DriveType.NoRootDirectory
            // after the VHD is removed in session 'A'.
            if (drive != null && !string.IsNullOrEmpty(drive.Name) && drive.Name.Length == 1)
                    char driveChar = Convert.ToChar(drive.Name, CultureInfo.InvariantCulture);
                    if (char.ToUpperInvariant(driveChar) >= 'A' && char.ToUpperInvariant(driveChar) <= 'Z')
                        DriveInfo systemDriveInfo = new DriveInfo(drive.Name);
                        if (systemDriveInfo.DriveType == DriveType.NoRootDirectory)
                            if (!Directory.Exists(drive.Root))
                    // At this point, We dont care if the drive is not a valid drive that does not host the VHD.
        /// Gets all the drives for a specific provider.
        /// The identifier for the provider to retrieve the drives for.
        /// An IEnumerable that contains the drives for the specified provider.
        internal Collection<PSDriveInfo> GetDrivesForProvider(string providerId)
                return Drives(null);
            // Ensure that the provider name resolves to a single provider
            GetSingleProvider(providerId);
            foreach (PSDriveInfo drive in Drives(null))
                if (drive != null &&
                    drive.Provider.NameEquals(providerId))
        #region RemoveDrive
        /// Removes the drive with the specified name.
        /// The ID of the scope from which to remove the drive.
        /// If the scope ID is null or empty, the scope hierarchy will be searched
        internal void RemoveDrive(string driveName, bool force, string scopeID)
            if (driveName == null)
                throw PSTraceSource.NewArgumentNullException(nameof(driveName));
            PSDriveInfo drive = GetDrive(driveName, scopeID);
                DriveNotFoundException e = new DriveNotFoundException(
            RemoveDrive(drive, force, scopeID);
        /// The context of the command.
        internal void RemoveDrive(
            string scopeID,
                "The caller should verify the context");
                context.WriteError(new ErrorRecord(e.ErrorRecord, e));
                RemoveDrive(drive, force, scopeID, context);
        /// The drive to be removed.
        internal void RemoveDrive(PSDriveInfo drive, bool force, string scopeID)
            if (context.HasErrors() && !force)
            // Make sure that the CanRemoveDrive is called even if we are forcing
            // the removal because we want the provider to have a chance to
            // cleanup.
            bool canRemove = false;
                canRemove = CanRemoveDrive(drive, context);
            // Now remove the drive if there was no error or we are forcing the removal
            if (canRemove || force)
                            PSDriveInfo result = scope.GetDrive(drive.Name);
                                // If the drive is the current drive for the provider, remove
                                // it from the current drive list.
                                if (ProvidersCurrentWorkingDrive[drive.Provider] == result)
                                    ProvidersCurrentWorkingDrive[drive.Provider] = null;
                    if (ProvidersCurrentWorkingDrive[drive.Provider] == drive)
                PSInvalidOperationException e =
                        SessionStateStrings.DriveRemovalPreventedByProvider,
                        drive.Provider);
        /// Determines if the drive can be removed by calling the provider
        /// for the drive.
        /// The drive to test for removal.
        /// True if the drive can be removed, false otherwise.
        /// If the provider threw an exception when RemoveDrive was called.
        private bool CanRemoveDrive(PSDriveInfo drive, CmdletProviderContext context)
            s_tracer.WriteLine("Drive name = {0}", drive.Name);
            // First set the drive data
            // Now see if the provider will let us remove the drive
            DriveCmdletProvider driveCmdletProvider =
                GetDriveProviderInstance(drive.Provider);
            bool driveRemovable = false;
                result = driveCmdletProvider.RemoveDrive(drive, context);
                    "RemoveDriveProviderException",
                    SessionStateStrings.RemoveDriveProviderException,
                    driveCmdletProvider.ProviderInfo,
                // Make sure the provider didn't try to pull a fast one on us
                // and substitute a different drive.
                    driveRemovable = true;
            return driveRemovable;
        #endregion RemoveDrive
        #region Drives
        /// Gets an enumerable list of the drives that are mounted in
        /// the specified scope.
        /// The scope to retrieve the drives from. If null or empty,
        /// all drives from all scopes will be retrieved.
        internal Collection<PSDriveInfo> Drives(string scope)
            Dictionary<string, PSDriveInfo> driveTable = new Dictionary<string, PSDriveInfo>();
            SessionStateScope startingScope = _currentScope;
            if (!string.IsNullOrEmpty(scope))
                startingScope = GetScopeByID(scope);
                new SessionStateScopeEnumerator(startingScope);
            DriveInfo[] alldrives = DriveInfo.GetDrives();
            Collection<string> driveNames = new Collection<string>();
            foreach (DriveInfo drive in alldrives)
                driveNames.Add(drive.Name.Substring(0, 1));
            foreach (SessionStateScope lookupScope in scopeEnumerator)
                foreach (PSDriveInfo drive in lookupScope.Drives)
                    // It is the correct behavior for child scope
                    // drives to overwrite parent scope drives of
                    // the same name.
                        bool driveIsValid = true;
                        // If the drive is auto-mounted, ensure that it still exists, or remove the drive.
                        if (drive.IsAutoMounted || IsAStaleVhdMountedDrive(drive))
                            driveIsValid = ValidateOrRemoveAutoMountedDrive(drive, lookupScope);
                        if (drive.Name.Length == 1)
                            if (!(driveNames.Contains(drive.Name)))
                                driveTable.Remove(drive.Name);
                        if (driveIsValid && !driveTable.ContainsKey(drive.Name))
                            driveTable[drive.Name] = drive;
                // If the scope was specified then don't loop
                // through the other scopes
                if (scope != null && scope.Length > 0)
            // Now lookup all the file system drives and automount any that are not
            // present
                foreach (System.IO.DriveInfo fsDriveInfo in alldrives)
                    if (fsDriveInfo != null)
                        string fsDriveName = fsDriveInfo.Name.Substring(0, 1);
                        if (!driveTable.ContainsKey(fsDriveName))
                            PSDriveInfo automountedDrive = AutomountFileSystemDrive(fsDriveInfo);
                            if (automountedDrive != null)
                                driveTable[automountedDrive.Name] = automountedDrive;
            // We don't want to have automounting cause an exception. We
            // rather it just fail silently as it wasn't a result of an
            // explicit request by the user anyway.
            Collection<PSDriveInfo> results = new Collection<PSDriveInfo>();
            foreach (PSDriveInfo drive in driveTable.Values)
        #endregion Drives
        /// Gets or sets the current working drive.
        internal PSDriveInfo CurrentDrive
                if (this != ExecutionContext.TopLevelSessionState)
                    return ExecutionContext.TopLevelSessionState.CurrentDrive;
                    return _currentDrive;
                    ExecutionContext.TopLevelSessionState.CurrentDrive = value;
                    _currentDrive = value;

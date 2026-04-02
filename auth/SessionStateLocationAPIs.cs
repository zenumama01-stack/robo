        #region Current working directory/drive
        /// Gets the current monad namespace specific working location. If
        /// you want to change the current working directory use the SetLocation
        internal PathInfo CurrentLocation
                if (CurrentDrive == null)
                    // We need the error handling, and moving to a method would be
                    // a breaking change
                PathInfo result =
                    new PathInfo(
                        CurrentDrive,
                        CurrentDrive.Provider,
                        CurrentDrive.CurrentLocation,
                        new SessionState(this));
        /// Gets the namespace specific path of the current working directory
        /// for the specified namespace.
        /// <param name="namespaceID">
        /// An identifier that uniquely identifies the namespace to get the
        /// current working directory for.
        /// The namespace specific path of the current working directory for
        /// the specified namespace.
        /// If <paramref name="namespaceID"/> is null.
        /// If a current drive cannot be found for the provider <paramref name="namespaceID"/>
        internal PathInfo GetNamespaceCurrentLocation(string namespaceID)
            if (namespaceID == null)
                throw PSTraceSource.NewArgumentNullException(nameof(namespaceID));
            // If namespace ID is empty, we will use the current working drive
            if (namespaceID.Length == 0)
                ProvidersCurrentWorkingDrive.TryGetValue(CurrentDrive.Provider, out drive);
                // First check to see if the provider exists
                ProvidersCurrentWorkingDrive.TryGetValue(GetSingleProvider(namespaceID), out drive);
                DriveNotFoundException e =
                        namespaceID,
            // Now make the namespace specific path
            if (drive.Hidden)
                if (LocationGlobber.IsProviderDirectPath(drive.CurrentLocation))
                    path = drive.CurrentLocation;
                    path = LocationGlobber.GetProviderQualifiedPath(drive.CurrentLocation, drive.Provider);
                path = LocationGlobber.GetDriveQualifiedPath(drive.CurrentLocation, drive);
            return new PathInfo(drive, drive.Provider, path, new SessionState(this));
        /// Changes the current working directory to the path specified.
        /// The path of the new current working directory.
        /// The PathInfo object representing the path of the location
        /// that was set.
        internal PathInfo SetLocation(string path)
            return SetLocation(path, null);
        /// The path of the new current working directory
        /// The context the provider uses when performing the operation.
        /// If the <paramref name="path"/> could not be resolved.
            return SetLocation(path, context, literalPath: false);
        /// Indicate if the path is a literal path.
            PathInfo current = CurrentLocation;
            string driveName = null;
            string providerId = null;
            switch (originalPath)
                case string originalPathSwitch when !literalPath && originalPathSwitch.Equals("-", StringComparison.Ordinal):
                    if (_setLocationHistory.UndoCount <= 0)
                        throw new InvalidOperationException(SessionStateStrings.LocationUndoStackIsEmpty);
                    path = _setLocationHistory.Undo(this.CurrentLocation).Path;
                case string originalPathSwitch when !literalPath && originalPathSwitch.Equals("+", StringComparison.Ordinal):
                    if (_setLocationHistory.RedoCount <= 0)
                        throw new InvalidOperationException(SessionStateStrings.LocationRedoStackIsEmpty);
                    path = _setLocationHistory.Redo(this.CurrentLocation).Path;
                    var pushPathInfo = GetNewPushPathInfo();
                    _setLocationHistory.Push(pushPathInfo);
            PSDriveInfo previousWorkingDrive = CurrentDrive;
            // First check to see if the path is a home path
            if (LocationGlobber.IsHomePath(path))
                path = Globber.GetHomeRelativePath(path);
            if (LocationGlobber.IsProviderDirectPath(path))
                // The path is a provider-direct path so use the current
                // provider and its hidden drive but don't modify the path
                // at all.
                provider = CurrentLocation.Provider;
                CurrentDrive = provider.HiddenDrive;
            else if (LocationGlobber.IsProviderQualifiedPath(path, out providerId))
                provider = GetSingleProvider(providerId);
                // See if the path is a relative or absolute
                if (Globber.IsAbsolutePath(path, out driveName))
                    // Since the path is an absolute path
                    // we need to change the current working
                    // drive
                    PSDriveInfo newWorkingDrive = GetDrive(driveName);
                    CurrentDrive = newWorkingDrive;
                    // If the path is simply a colon-terminated drive,
                    // not a slash-terminated path to the root of a drive,
                    // set the path to the current working directory of that drive.
                    string colonTerminatedVolume = CurrentDrive.Name + ':';
                    if (CurrentDrive.VolumeSeparatedByColon && (path.Length == colonTerminatedVolume.Length))
                        path = Path.Combine(colonTerminatedVolume + Path.DirectorySeparatorChar, CurrentDrive.CurrentLocation);
                    // Now that the current working drive is set,
                    // process the rest of the path as a relative path.
            context ??= new CmdletProviderContext(this.ExecutionContext);
            if (CurrentDrive != null)
                context.Drive = CurrentDrive;
            Collection<PathInfo> workingPath = null;
                workingPath =
                    Globber.GetGlobbedMonadPathsFromMonadPath(
                // Reset the drive to the previous drive and
                // then rethrow the error
                CurrentDrive = previousWorkingDrive;
            if (workingPath.Count == 0)
                // Set the current working drive back to the previous
                // one in case it was changed.
            // We allow globbing the location as long as it only resolves a single container.
            bool foundContainer = false;
            bool pathIsContainer = false;
            bool pathIsProviderQualifiedPath = false;
            bool currentPathisProviderQualifiedPath = false;
            for (int index = 0; index < workingPath.Count; ++index)
                CmdletProviderContext normalizePathContext =
                PathInfo resolvedPath = workingPath[index];
                string currentPath = path;
                    string providerName = null;
                    currentPathisProviderQualifiedPath = LocationGlobber.IsProviderQualifiedPath(resolvedPath.Path, out providerName);
                    if (currentPathisProviderQualifiedPath)
                        // The path should be the provider-qualified path without the provider ID
                        // or ::
                        string providerInternalPath = LocationGlobber.RemoveProviderQualifier(resolvedPath.Path);
                            currentPath = NormalizeRelativePath(GetSingleProvider(providerName), providerInternalPath, string.Empty, normalizePathContext);
                            // Since the provider does not support normalizing the path, just
                            // use the path we currently have.
                            currentPath = NormalizeRelativePath(resolvedPath.Path, CurrentDrive.Root, normalizePathContext);
                    // Now see if there was errors while normalizing the path
                    if (normalizePathContext.HasErrors())
                        normalizePathContext.ThrowFirstErrorOrDoNothing();
                    normalizePathContext.RemoveStopReferral();
                CmdletProviderContext itemContainerContext =
                itemContainerContext.SuppressWildcardExpansion = true;
                    isContainer =
                            resolvedPath.Path,
                            itemContainerContext);
                    if (itemContainerContext.HasErrors())
                        itemContainerContext.ThrowFirstErrorOrDoNothing();
                    if (currentPath.Length == 0)
                        // Treat this as a container because providers that only
                        // support the ContainerCmdletProvider interface are really
                        // containers at their root.
                    itemContainerContext.RemoveStopReferral();
                    if (foundContainer)
                        // The path resolved to more than one container
                                SessionStateStrings.PathResolvedToMultiple,
                                originalPath);
                        // Set the path to use
                        path = currentPath;
                        // Mark it as a container
                        pathIsContainer = true;
                        // Mark whether or not it was provider-qualified
                        pathIsProviderQualifiedPath = currentPathisProviderQualifiedPath;
                        // Mark that we have already found one container. Finding additional
                        // should be an error
                        foundContainer = true;
            if (pathIsContainer)
                // Remove the root slash since it is implied that the
                // current working directory is relative to the root.
                if (!LocationGlobber.IsProviderDirectPath(path) &&
                    path.StartsWith(StringLiterals.DefaultPathSeparator) &&
                    !pathIsProviderQualifiedPath)
                    path = path.Substring(1);
                    "New working path = {0}",
                CurrentDrive.CurrentLocation = path;
                        originalPath,
            // Now make sure the current drive is set in the provider's
            // current working drive hashtable
            ProvidersCurrentWorkingDrive[CurrentDrive.Provider] =
                CurrentDrive;
            // Set the $PWD variable to the new location
            this.SetVariable(SpecialVariables.PWDVarPath, this.CurrentLocation, false, true, CommandOrigin.Internal);
            // If an action has been defined for location changes, invoke it now.
            if (PublicSessionState.InvokeCommand.LocationChangedAction != null)
                var eventArgs = new LocationChangedEventArgs(PublicSessionState, current, CurrentLocation);
                PublicSessionState.InvokeCommand.LocationChangedAction.Invoke(ExecutionContext.CurrentRunspace, eventArgs);
                s_tracer.WriteLine("Invoked LocationChangedAction");
            return this.CurrentLocation;
        /// Determines if the specified path is the current working directory
        /// or a parent of the current working directory.
        /// A monad namespace absolute or relative path.
        /// true, if the path is the current working directory or a parent of the current
        /// working directory. false, otherwise.
            string providerSpecificPath =
                s_tracer.WriteLine("Tracing drive");
                drive.Trace();
                providerSpecificPath != null,
                "There should always be a way to generate a provider path for a " +
                "given path");
            // Check to see if the path that was specified is within the current
            // working drive
            if (drive == CurrentDrive)
                // The path needs to be normalized to get rid of relative path tokens
                // so they don't interfere with our path comparisons below
                CmdletProviderContext normalizePathContext
                    = new CmdletProviderContext(context);
                    providerSpecificPath = NormalizeRelativePath(path, null, normalizePathContext);
                s_tracer.WriteLine("Provider path = {0}", providerSpecificPath);
                // Get the current working directory provider specific path
                PSDriveInfo currentWorkingDrive = null;
                ProviderInfo currentDriveProvider = null;
                string currentWorkingPath =
                        out currentDriveProvider,
                        out currentWorkingDrive);
                    currentWorkingDrive == CurrentDrive,
                    "The current working drive should be the CurrentDrive.");
                    "Current working path = {0}",
                    currentWorkingPath);
                // See if the path is the current working directory or a parent
                // of the current working directory
                    "Comparing {0} to {1}",
                    providerSpecificPath,
                if (string.Equals(providerSpecificPath, currentWorkingPath, StringComparison.OrdinalIgnoreCase))
                    // The path is the current working directory so
                    // return true
                    s_tracer.WriteLine("The path is the current working directory");
                    // Check to see if the specified path is a parent
                    string lockedDirectory = currentWorkingPath;
                    while (lockedDirectory.Length > 0)
                        // We need to allow the provider to go as far up the tree
                        // as it can even if that means it has to traverse higher
                        // than the mount point for this drive. That is
                        // why we are passing the empty string as the root here.
                        lockedDirectory =
                            GetParentPath(
                                lockedDirectory,
                            providerSpecificPath);
                        if (string.Equals(lockedDirectory, providerSpecificPath, StringComparison.OrdinalIgnoreCase))
                            // The path is a parent of the current working
                            // directory
                                "The path is a parent of the current working directory: {0}",
                                lockedDirectory);
                s_tracer.WriteLine("Drives are not the same");
        #endregion Current working directory/drive
        #region push-Pop current working directory
        /// Location history for Set-Location that supports Undo/Redo using bounded stacks.
        private readonly HistoryStack<PathInfo> _setLocationHistory;
        /// A stack of the most recently pushed locations.
        private readonly Dictionary<string, Stack<PathInfo>> _workingLocationStack;
        private const string startingDefaultStackName = "default";
        /// The name of the default location stack.
        private string _defaultStackName = startingDefaultStackName;
        /// Pushes the current location onto the working
        /// location stack so that it can be retrieved later.
        /// The ID of the stack to push the location on. If
        /// it is null or empty the default stack is used.
        internal void PushCurrentLocation(string stackName)
            if (string.IsNullOrEmpty(stackName))
                stackName = _defaultStackName;
            // Get the location stack from the hashtable
            Stack<PathInfo> locationStack = null;
            if (!_workingLocationStack.TryGetValue(stackName, out locationStack))
                locationStack = new Stack<PathInfo>();
                _workingLocationStack[stackName] = locationStack;
            // Push the directory/drive pair onto the stack
            locationStack.Push(pushPathInfo);
        private PathInfo GetNewPushPathInfo()
            // Create a new instance of the directory/drive pair
            ProviderInfo provider = CurrentDrive.Provider;
            string mshQualifiedPath =
                LocationGlobber.GetMshQualifiedPath(CurrentDrive.CurrentLocation, CurrentDrive);
            PathInfo newPushLocation =
                    mshQualifiedPath,
                "Pushing drive: {0} directory: {1}",
                CurrentDrive.Name,
                mshQualifiedPath);
            return newPushLocation;
        /// Resets the current working drive and directory to the first
        /// entry on the working directory stack and removes that entry
        /// from the stack.
        /// The ID of the stack to pop the location from. If it is null or
        /// empty the default stack is used.
        /// A PathInfo object representing the location that was popped
        /// from the location stack and set as the new location.
        internal PathInfo PopLocation(string stackName)
            if (WildcardPattern.ContainsWildcardCharacters(stackName))
                // Need to glob the stack name, but it can only glob to a single.
                bool haveMatch = false;
                WildcardPattern stackNamePattern =
                    WildcardPattern.Get(stackName, WildcardOptions.IgnoreCase);
                foreach (string key in _workingLocationStack.Keys)
                    if (stackNamePattern.IsMatch(key))
                        if (haveMatch)
                                    nameof(stackName),
                                    SessionStateStrings.StackNameResolvedToMultiple,
                                    stackName);
                        haveMatch = true;
                        stackName = key;
            PathInfo result = CurrentLocation;
                    if (!string.Equals(stackName, startingDefaultStackName, StringComparison.OrdinalIgnoreCase))
                                SessionStateStrings.StackNotFound,
                PathInfo poppedWorkingDirectory = locationStack.Pop();
                    poppedWorkingDirectory != null,
                    "All items in the workingLocationStack should be " +
                    "of type PathInfo");
                string newPath =
                    LocationGlobber.GetMshQualifiedPath(
                        WildcardPattern.Escape(poppedWorkingDirectory.Path),
                        poppedWorkingDirectory.GetDrive());
                result = SetLocation(newPath);
                if (locationStack.Count == 0 &&
                    !string.Equals(stackName, startingDefaultStackName, StringComparison.OrdinalIgnoreCase))
                    // Remove the stack from the stack list if it
                    // no longer contains any paths.
                    _workingLocationStack.Remove(stackName);
                // This is a no-op. We stay with the current working
        /// Gets the monad namespace paths for all the directories that are
        /// pushed on the working directory stack.
        /// The stack of the ID of the location stack to retrieve. If it is
        /// null or empty the default stack is used.
        /// The PathInfoStack representing the location stack for the specified
        /// stack ID.
        /// If no location stack <paramref name="stackName"/> exists except if
        /// the default stack is requested.
        internal PathInfoStack LocationStack(string stackName)
                // If the request was for the default stack, but it doesn't
                // yet exist, create a dummy stack and return it.
                        stackName,
                        startingDefaultStackName,
                    throw PSTraceSource.NewArgumentException(nameof(stackName));
            PathInfoStack result = new PathInfoStack(stackName, locationStack);
        /// Sets the default stack ID to the specified stack ID.
        /// The stack ID to be used as the default.
        /// The PathInfoStack for the new default stack or null if the
        /// stack does not exist yet.
        internal PathInfoStack SetDefaultLocationStack(string stackName)
                stackName = startingDefaultStackName;
            if (!_workingLocationStack.ContainsKey(stackName))
                if (string.Equals(stackName, startingDefaultStackName, StringComparison.OrdinalIgnoreCase))
                    // Since the "default" stack must always exist, create it here
                    return new PathInfoStack(startingDefaultStackName, new Stack<PathInfo>());
                        "StackNotFound",
                throw itemNotFound;
            _defaultStackName = stackName;
            Stack<PathInfo> locationStack = _workingLocationStack[_defaultStackName];
            if (locationStack != null)
                return new PathInfoStack(_defaultStackName, locationStack);
        #endregion push-Pop current working directory
    /// Event argument for the LocationChangedAction containing
    /// information about the old location we were in and the new
    /// location we changed to.
    public class LocationChangedEventArgs : EventArgs
        /// Initializes a new instance of the LocationChangedEventArgs class.
        /// The public session state instance associated with this runspace.
        /// <param name="oldPath">
        /// The path we changed locations from.
        /// <param name="newPath">
        /// The path we change locations to.
        internal LocationChangedEventArgs(SessionState sessionState, PathInfo oldPath, PathInfo newPath)
            SessionState = sessionState;
            OldPath = oldPath;
            NewPath = newPath;
        /// Gets the path we changed location from.
        public PathInfo OldPath { get; internal set; }
        /// Gets the path we changed location to.
        public PathInfo NewPath { get; internal set; }
        /// Gets the session state instance for the current runspace.
        public SessionState SessionState { get; internal set; }

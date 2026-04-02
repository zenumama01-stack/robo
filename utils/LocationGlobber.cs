    /// Implements the interfaces used by navigation commands to work with
    /// the virtual drive system.
    internal sealed class LocationGlobber
        /// using "LocationGlobber" as the category.
             "LocationGlobber",
             "The location globber converts PowerShell paths with glob characters to zero or more paths.")]
            Dbg.PSTraceSource.GetTracer("LocationGlobber",
             "The location globber converts PowerShell paths with glob characters to zero or more paths.");
        /// User level tracing for path resolution.
             "PathResolution",
             "Traces the path resolution algorithm.")]
        private static readonly Dbg.PSTraceSource s_pathResolutionTracer =
            Dbg.PSTraceSource.GetTracer(
                "Traces the path resolution algorithm.",
        /// Constructs an instance of the LocationGlobber from the current SessionState.
        /// The instance of session state on which this location globber acts.
        internal LocationGlobber(SessionState sessionState)
        #region PowerShell paths from PowerShell path globbing
        /// Converts a PowerShell path containing glob characters to PowerShell paths that match
        /// the glob string.
        /// A PowerShell path containing glob characters.
        /// If true, a ItemNotFoundException will not be thrown for non-existing
        /// paths. Instead an appropriate path will be returned as if it did exist.
        /// The provider instance used to resolve the path.
        /// The PowerShell paths that match the glob string.
        internal Collection<PathInfo> GetGlobbedMonadPathsFromMonadPath(
            out CmdletProvider providerInstance)
            CmdletProviderContext context =
                new CmdletProviderContext(_sessionState.Internal.ExecutionContext);
            return GetGlobbedMonadPathsFromMonadPath(path, allowNonexistingPaths, context, out providerInstance);
        /// The instance of the provider used to resolve the path.
        /// If <paramref name="context"/> has been signaled for
            providerInstance = null;
            Collection<PathInfo> result;
            using (s_pathResolutionTracer.TraceScope("Resolving MSH path \"{0}\" to MSH path", path))
                TraceFilters(context);
                // First check to see if the path starts with a ~ (home)
                if (IsHomePath(path))
                    using (s_pathResolutionTracer.TraceScope("Resolving HOME relative path."))
                        path = GetHomeRelativePath(path);
                // Now determine how to parse the path
                bool isProviderDirectPath = IsProviderDirectPath(path);
                bool isProviderQualifiedPath = IsProviderQualifiedPath(path);
                if (isProviderDirectPath || isProviderQualifiedPath)
                        ResolvePSPathFromProviderPath(
                            allowNonexistingPaths,
                            isProviderDirectPath,
                            isProviderQualifiedPath,
                        ResolveDriveQualifiedPath(
                if (!allowNonexistingPaths &&
                    result.Count < 1 &&
                    (!WildcardPattern.ContainsWildcardCharacters(path) || context.SuppressWildcardExpansion) &&
                    (context.Include == null || context.Include.Count == 0) &&
                    (context.Exclude == null || context.Exclude.Count == 0))
                    // Since we are not globbing, throw an exception since
                    // the path doesn't exist
                    s_pathResolutionTracer.TraceError("Item does not exist: {0}", path);
        private Collection<string> ResolveProviderPathFromProviderPath(
            out CmdletProvider providerInstance
            // Check the provider capabilities before globbing
            providerInstance = _sessionState.Internal.GetProviderInstance(providerId);
            ContainerCmdletProvider containerCmdletProvider = providerInstance as ContainerCmdletProvider;
            ItemCmdletProvider itemProvider = providerInstance as ItemCmdletProvider;
            Collection<string> stringResult = new Collection<string>();
            if (!context.SuppressWildcardExpansion)
                // See if the provider will expand the wildcard
                if (CmdletProviderManagementIntrinsics.CheckProviderCapabilities(
                        ProviderCapabilities.ExpandWildcards,
                        providerInstance.ProviderInfo))
                    s_pathResolutionTracer.WriteLine("Wildcard matching is being performed by the provider.");
                    // Only do the expansion if the path actually contains wildcard
                    if ((itemProvider != null) &&
                        (WildcardPattern.ContainsWildcardCharacters(providerPath)))
                        stringResult = new Collection<string>(itemProvider.ExpandPath(providerPath, context));
                        stringResult.Add(providerPath);
                    s_pathResolutionTracer.WriteLine("Wildcard matching is being performed by the engine.");
                    if (containerCmdletProvider != null)
                        // Since it is really a provider-internal path, use provider-to-provider globbing
                        // and then add back on the provider ID.
                        stringResult =
                            GetGlobbedProviderPathsFromProviderPath(
                                containerCmdletProvider,
                        // For simple CmdletProvider instances, we can't resolve the paths any
                        // further, so just return the providerPath
            // They are suppressing wildcard expansion
                if (itemProvider != null)
                    if (allowNonexistingPaths || itemProvider.ItemExists(providerPath, context))
            // Make sure this resolved to something
            if ((!allowNonexistingPaths) &&
                stringResult.Count < 1 &&
                !WildcardPattern.ContainsWildcardCharacters(providerPath) &&
                s_pathResolutionTracer.TraceError("Item does not exist: {0}", providerPath);
            return stringResult;
        private Collection<PathInfo> ResolvePSPathFromProviderPath(
            bool isProviderDirectPath,
            bool isProviderQualifiedPath,
            Collection<PathInfo> result = new Collection<PathInfo>();
            // The path is a provide direct path so use the current
            // provider and don't modify the path.
            if (isProviderDirectPath)
                s_pathResolutionTracer.WriteLine("Path is PROVIDER-DIRECT");
                providerId = _sessionState.Path.CurrentLocation.Provider.Name;
            else if (isProviderQualifiedPath)
                s_pathResolutionTracer.WriteLine("Path is PROVIDER-QUALIFIED");
                providerPath = ParseProviderPath(path, out providerId);
            s_pathResolutionTracer.WriteLine("PROVIDER-INTERNAL path: {0}", providerPath);
            s_pathResolutionTracer.WriteLine("Provider: {0}", providerId);
            Collection<string> stringResult = ResolveProviderPathFromProviderPath(
                providerId,
                out providerInstance
            // Get the hidden drive for the provider
            drive = providerInstance.ProviderInfo.HiddenDrive;
            // Now fix the paths
            foreach (string globbedPath in stringResult)
                string escapedPath = globbedPath;
                string constructedProviderPath = null;
                if (IsProviderDirectPath(escapedPath))
                    constructedProviderPath = escapedPath;
                    constructedProviderPath =
                            "{0}::{1}",
                            escapedPath);
                result.Add(new PathInfo(drive, providerInstance.ProviderInfo, constructedProviderPath, _sessionState));
                s_pathResolutionTracer.WriteLine("RESOLVED PATH: {0}", constructedProviderPath);
        private Collection<PathInfo> ResolveDriveQualifiedPath(
            s_pathResolutionTracer.WriteLine("Path is DRIVE-QUALIFIED");
            string relativePath =
                GetDriveRootRelativePathFromPSPath(
                    !context.SuppressWildcardExpansion,
                    out drive,
                "GetDriveRootRelativePathFromPSPath should always return a valid drive");
                relativePath != null,
            s_pathResolutionTracer.WriteLine("DRIVE-RELATIVE path: {0}", relativePath);
            s_pathResolutionTracer.WriteLine("Drive: {0}", drive.Name);
            s_pathResolutionTracer.WriteLine("Provider: {0}", drive.Provider);
            // Associate the drive with the context
            providerInstance = _sessionState.Internal.GetContainerProviderInstance(drive.Provider);
            ProviderInfo provider = providerInstance.ProviderInfo;
            string userPath = null;
            string itemPath = null;
                userPath = GetProviderQualifiedPath(relativePath, provider);
                itemPath = relativePath;
                userPath = GetDriveQualifiedPath(relativePath, drive);
                itemPath = GetProviderPath(path, context);
            s_pathResolutionTracer.WriteLine("PROVIDER path: {0}", itemPath);
                        (WildcardPattern.ContainsWildcardCharacters(relativePath)))
                        foreach (string pathResult in itemProvider.ExpandPath(itemPath, context))
                            stringResult.Add(
                                GetDriveRootRelativePathFromProviderPath(pathResult, drive, context));
                        stringResult.Add(GetDriveRootRelativePathFromProviderPath(itemPath, drive, context));
                    // Now perform the globbing
                        ExpandMshGlobPath(
                    if (allowNonexistingPaths || itemProvider.ItemExists(itemPath, context))
                        stringResult.Add(userPath);
                !WildcardPattern.ContainsWildcardCharacters(path) &&
            foreach (string expandedPath in stringResult)
                // Make sure to obey StopProcessing
                // Add the drive back into the path
                userPath = null;
                    if (IsProviderDirectPath(expandedPath))
                        userPath = expandedPath;
                        userPath =
                            LocationGlobber.GetProviderQualifiedPath(
                                expandedPath,
                result.Add(new PathInfo(drive, provider, userPath, _sessionState));
                s_pathResolutionTracer.WriteLine("RESOLVED PATH: {0}", userPath);
        #endregion PowerShell paths from PowerShell path globbing
        #region Provider paths from PowerShell path globbing
        /// Converts a PowerShell path containing glob characters to the provider
        /// specific paths matching the glob strings.
        /// Returns the information of the provider that was used to do the globbing.
        /// An array of provider specific paths that matched the PowerShell glob path.
        /// Any exception can be thrown by the provider that is called to build
        /// the provider path.
        internal Collection<string> GetGlobbedProviderPathsFromMonadPath(
            return GetGlobbedProviderPathsFromMonadPath(path, allowNonexistingPaths, context, out provider, out providerInstance);
            using (s_pathResolutionTracer.TraceScope("Resolving MSH path \"{0}\" to PROVIDER-INTERNAL path", path))
                // Remove the drive from the context if this path is not associated with a drive
                if (IsProviderQualifiedPath(path))
                    context.Drive = null;
                string providerPath = GetProviderPath(path, context, out provider, out drive);
                if (providerPath == null)
                    s_tracer.WriteLine("provider returned a null path so return an empty array");
                    s_pathResolutionTracer.WriteLine("Provider '{0}' returned null", provider);
                Collection<string> paths = new Collection<string>();
                foreach (PathInfo currentPath in
                    GetGlobbedMonadPathsFromMonadPath(
                        out providerInstance))
                    paths.Add(currentPath.ProviderPath);
        #endregion Provider paths from Monad path globbing
        #region Provider paths from provider path globbing
        /// Given a provider specific path that contains glob characters, this method
        /// will perform the globbing using the specified provider and return the
        /// matching provider specific paths.
        /// The path containing the glob characters to resolve.
        /// The ID of the provider to use to do the resolution.
        /// The instance of the provider that was used to resolve the path.
        /// An array of provider specific paths that match the glob path.
        internal Collection<string> GetGlobbedProviderPathsFromProviderPath(
            if (context.HasErrors())
                // Throw the first error
                ErrorRecord errorRecord = context.GetAccumulatedErrorObjects()[0];
        /// The path containing the glob characters to resolve. The path must be in the
        /// form providerId::providerPath.
        /// The provider identifier for the provider to use to do the globbing.
        /// The context under which the command is occurring.
        /// An instance of the provider that was used to perform the globbing.
            using (s_pathResolutionTracer.TraceScope("Resolving PROVIDER-INTERNAL path \"{0}\" to PROVIDER-INTERNAL path", path))
                return ResolveProviderPathFromProviderPath(
        #endregion Provider path to provider paths globbing
        #region Path manipulation
        /// Gets a provider specific path when given an Msh path without resolving the
        /// An Msh path.
        /// A provider specific path that the Msh path represents.
        internal string GetProviderPath(string path)
            return GetProviderPath(path, out provider);
        /// The information of the provider that was used to resolve the path.
        internal string GetProviderPath(string path, out ProviderInfo provider)
            string result = GetProviderPath(path, context, out provider, out drive);
                Collection<ErrorRecord> errors = context.GetAccumulatedErrorObjects();
                if (errors != null &&
                    errors.Count > 0)
        internal string GetProviderPath(string path, CmdletProviderContext context)
        /// Returns a provider specific path for given PowerShell path.
        /// Either a PowerShell path or a provider path in the form providerId::providerPath
        /// The command context under which this operation is occurring.
        /// This parameter is filled with the provider information for the given path.
        /// This parameter is filled with the PowerShell drive that represents the given path. If a
        /// provider path is given drive will be null.
        /// The provider specific path generated from the given path.
        internal string GetProviderPath(
            return GetProviderPath(
        /// <param name="context">Cmdlet context.</param>
        /// <param name="isTrusted">When true bypass trust check.</param>
        /// <param name="provider">Provider.</param>
        /// <param name="drive">Drive.</param>
            bool isTrusted,
            drive = null;
            // Now check to see if it is a provider-direct path (starts with // or \\)
            if (IsProviderDirectPath(path))
                // just return the path directly using the current provider
                provider = _sessionState.Path.CurrentLocation.Provider;
                s_pathResolutionTracer.WriteLine("PROVIDER-INTERNAL path: {0}", result);
                s_pathResolutionTracer.WriteLine("Provider: {0}", provider);
            else if (IsProviderQualifiedPath(path))
                result = ParseProviderPath(path, out providerId);
                // Get the provider info
                provider = _sessionState.Internal.GetSingleProvider(providerId);
                string relativePath = GetDriveRootRelativePathFromPSPath(path, context, false, out drive, out providerInstance);
                    result = relativePath;
                    result = GetProviderSpecificPath(drive, relativePath, context);
                provider = drive.Provider;
            s_pathResolutionTracer.WriteLine("RESOLVED PATH: {0}", result);
            // If this is a private provider, don't allow access to it directly from the runspace.
            if ((provider != null) &&
                (context != null) &&
                (context.MyInvocation != null) &&
                (context.ExecutionContext != null) &&
                (context.ExecutionContext.InitialSessionState != null))
                foreach (Runspaces.SessionStateProviderEntry sessionStateProvider in context.ExecutionContext.InitialSessionState.Providers[provider.Name])
                    if (!isTrusted &&
                        (sessionStateProvider.Visibility == SessionStateEntryVisibility.Private) &&
                        (context.MyInvocation.CommandOrigin == CommandOrigin.Runspace))
                        s_pathResolutionTracer.WriteLine("Provider is private: {0}", provider.Name);
                        throw new ProviderNotFoundException(
        /// Determines if the specified path is a provider. This is done by looking for
        /// two colons in a row. Anything before the colons is considered the provider ID,
        /// and everything after is considered a namespace specific path.
        /// The path to check to see if it is a provider path.
        /// True if the path is a provider path, false otherwise.
        internal static bool IsProviderQualifiedPath(string path)
            return IsProviderQualifiedPath(path, out providerId);
        /// The name of the provider if the path is a provider qualified path.
        internal static bool IsProviderQualifiedPath(string path, out string providerId)
            providerId = null;
                    // The current working directory is specified
                if (path.StartsWith(@".\", StringComparison.Ordinal) ||
                    path.StartsWith(@"./", StringComparison.Ordinal))
                    // The .\ prefix basically escapes anything that follows
                    // so treat it as a relative path no matter what comes
                    // after it.
                if (index == -1 || index + 1 >= path.Length || path[index + 1] != ':')
                    // If there is no : then the path is relative to the
                    // current working drive
                // If the :: is the first two character in the path then we
                // must assume that it is part of the path, and not
                // delimiting the drive name.
                    // Get the provider ID
                    providerId = path.Substring(0, index);
                    s_tracer.WriteLine("providerId = {0}", providerId);
        /// Determines if the given path is absolute while on a single root filesystem.
        /// Porting notes: absolute paths on non-Windows filesystems start with a '/' (no "C:" drive
        /// prefix, the slash is the prefix). We compare against both '/' and '\' (default and
        /// alternate path separator) in order for PowerShell to be slash agnostic.
        /// The path used in the determination
        /// Returns true if we're on a single root filesystem and the path is absolute.
        internal static bool IsSingleFileSystemAbsolutePath(string path)
            return path.StartsWith(StringLiterals.DefaultPathSeparator)
                || path.StartsWith(StringLiterals.AlternatePathSeparator);
        /// Determines if the given path is relative or absolute.
        /// true if the path is an absolute path, false otherwise.
        internal static bool IsAbsolutePath(string path)
                // compare both to \ and / here
                if (IsSingleFileSystemAbsolutePath(path))
                // If the : is the first character in the path then we
                    // see if there are any path separators before the colon which would mean the
                    // colon is part of a file or folder name and not a drive: ./foo:bar vs foo:bar
                    int separator = path.IndexOf(StringLiterals.DefaultPathSeparator, 0, index - 1);
                    if (separator == -1)
                        separator = path.IndexOf(StringLiterals.AlternatePathSeparator, 0, index - 1);
                    if (separator == -1 || index < separator)
                        // We must have a drive specified
        /// If the path is absolute, this out parameter will be the
        /// drive name of the drive that is referenced.
        internal bool IsAbsolutePath(string path, out string driveName)
            if (_sessionState.Drive.Current != null)
                driveName = _sessionState.Drive.Current.Name;
                driveName = null;
                    driveName = StringLiterals.DefaultPathSeparatorString;
                    driveName = path.Substring(0, index);
                    driveName != null,
                    "The drive name should always have a value, " +
                    "the default is the current working drive");
                    "driveName = {0}",
        #endregion Path manipulation
        #region private fields and methods
        /// The instance of session state on which this globber acts.
        /// Removes the back tick "`" from any of the glob characters in the path.
        /// The path to remove the glob escaping from.
        /// The path with the glob characters unescaped.
        private static string RemoveGlobEscaping(string path)
            string result = WildcardPattern.Unescape(path);
        #region Path manipulation methods
        /// Determines if the given drive name is a "special" name defined
        /// by the shell. For instance, "default", "current", "global", and "scope[##]" are scopes
        /// for variables and are considered shell virtual drives.
        /// The name of the drive to check to see if it is a shell virtual drive.
        /// This out parameter is filled with the scope that the drive name represents.
        /// It will be null if the driveName does not represent a scope.
        /// true, if the drive name is a shell virtual drive like "Default" or "global",
        /// The comparison is done using a case-insensitive comparison using the
        /// Invariant culture.
        /// This is internal so that it is accessible to SessionState.
        internal bool IsShellVirtualDrive(string driveName, out SessionStateScope scope)
                // It's the global scope.
                s_tracer.WriteLine("match found: {0}", StringLiterals.Global);
                scope = _sessionState.Internal.GlobalScope;
                // It's the local scope.
                s_tracer.WriteLine("match found: {0}", driveName);
                scope = _sessionState.Internal.CurrentScope;
        /// Gets a provider specific path that represents the specified path and is relative
        /// to the root of the PowerShell drive.
        /// Can be a relative or absolute path.
        /// <param name="escapeCurrentLocation">
        /// Escape the wildcards in the current location.  Use when this path will be
        /// passed through globbing.
        /// <param name="workingDriveForPath">
        /// This out parameter returns the drive that was specified
        /// by the <paramref name="path"/>. If <paramref name="path"/> is
        /// an absolute path this value may be something other than
        /// the current working drive.
        /// If the path refers to a non-existent drive, this parameter is set to null, and an exception is thrown.
        /// The provider instance that was used.
        /// A provider specific relative path to the root of the drive.
        /// The path is parsed to determine if it is a relative path to the
        /// current working drive or if it is an absolute path. If
        /// it is a relative path the provider specific path is generated using the current
        /// working directory, the drive root, and the path specified.
        /// If the path is an absolute path the provider specific path is generated by stripping
        /// of anything before the : and using that to find the appropriate
        /// drive. The provider specific path is then generated the same as the
        /// relative path using the specified drive instead of the
        /// current working drive.
        /// This is internal so that it can be called from SessionState
        /// If the provider is not a NavigationCmdletProvider.
        internal string GetDriveRootRelativePathFromPSPath(
            bool escapeCurrentLocation,
            out PSDriveInfo workingDriveForPath,
            workingDriveForPath = null;
            // Check to see if the path is relative or absolute
            bool isPathForCurrentDrive = false;
            if (IsAbsolutePath(path, out driveName))
                    "IsAbsolutePath should be returning the drive name");
                    "Drive Name: {0}",
                // This will resolve $GLOBAL, and $LOCAL as needed.
                // This throws DriveNotFoundException if a drive of the specified
                // name does not exist. Just let the exception propagate out.
                    workingDriveForPath = _sessionState.Drive.Get(driveName);
                    // Check to see if it is a path relative to the
                    // current drive's root. This is true when a drive root
                    // appears to be a drive (like HTTP://). The drive will not
                    // actually exist, but this is not an absolute path.
                    if (_sessionState.Drive.Current == null)
                    string normalizedRoot = _sessionState.Drive.Current.Root.Replace(
                        StringLiterals.AlternatePathSeparator, StringLiterals.DefaultPathSeparator);
                    if (normalizedRoot.Contains(':'))
                        string normalizedPath = path.Replace(StringLiterals.AlternatePathSeparator, StringLiterals.DefaultPathSeparator);
                        if (normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
                            isPathForCurrentDrive = true;
                            path = string.Concat(StringLiterals.DefaultPathSeparatorString, path.AsSpan(normalizedRoot.Length).TrimStart(StringLiterals.DefaultPathSeparator));
                            workingDriveForPath = _sessionState.Drive.Current;
                    if (!isPathForCurrentDrive)
                // Now hack off the drive component of the path
                    // This functionality needs to respect if a drive uses a colon to separate the path
                    // what happens here is this:
                    // - path is assumed to be drive root relative, so on Windows it would start with a
                    //   \
                    // - on Linux, there is no difference between drive root relative, and absolute, they
                    //   are both the same, so we have to preserve the drive here in order to make
                    //   sure the path will continue being drive root relative
                    if (workingDriveForPath.VolumeSeparatedByColon)
                        // this is the default behavior for all windows drives, and all non-filesystem
                        // drives on non-windows
                        path = path.Substring(driveName.Length + 1);
                // it's a relative path, so the working drive is the current drive
            if (workingDriveForPath == null)
                    _sessionState.Internal.GetContainerProviderInstance(workingDriveForPath.Provider);
                // Add the drive info to the context so that downstream methods
                // have access to it.
                context.Drive = workingDriveForPath;
                string relativePath = string.Empty;
                relativePath =
                    GenerateRelativePath(
                        workingDriveForPath,
                        escapeCurrentLocation,
                return relativePath;
                // If it's really not a container provider, the relative path will
                // always be empty
        private string GetDriveRootRelativePathFromProviderPath(
            CmdletProviderContext context
            string childPath = string.Empty;
            CmdletProvider providerInstance =
                _sessionState.Internal.GetContainerProviderInstance(drive.Provider);
            NavigationCmdletProvider navigationProvider = providerInstance as NavigationCmdletProvider;
            // Normalize the paths
            providerPath = providerPath.Replace(StringLiterals.AlternatePathSeparator, StringLiterals.DefaultPathSeparator);
            providerPath = providerPath.TrimEnd(StringLiterals.DefaultPathSeparator);
            string driveRoot = drive.Root.Replace(StringLiterals.AlternatePathSeparator, StringLiterals.DefaultPathSeparator);
            driveRoot = driveRoot.TrimEnd(StringLiterals.DefaultPathSeparator);
            // Keep on lopping off children until the remaining path
            // is the drive root.
            while ((!string.IsNullOrEmpty(providerPath)) &&
                (!providerPath.Equals(driveRoot, StringComparison.OrdinalIgnoreCase)))
                if (!string.IsNullOrEmpty(childPath))
                    childPath = _sessionState.Internal.MakePath(
                        navigationProvider.GetChildName(providerPath, context),
                    childPath = navigationProvider.GetChildName(providerPath, context);
                providerPath = _sessionState.Internal.GetParentPath(
            return childPath;
        /// Builds a provider specific path from the current working
        /// directory using the specified relative path.
        /// The drive to generate the provider specific path from.
        /// The relative path to add to the absolute path in the drive.
        /// An instance of the provider to use if MakePath or GetParentPath
        /// need to be called.
        /// A string with the joined current working path and relative
        /// If <paramref name="path"/> or <paramref name="drive"/> is null.
        internal string GenerateRelativePath(
            // This string will be filled in with the
            // new root relative working directory as we process
            // the supplied path
            string driveRootRelativeWorkingPath = drive.CurrentLocation;
            if ((!string.IsNullOrEmpty(driveRootRelativeWorkingPath) &&
                (driveRootRelativeWorkingPath.StartsWith(drive.Root, StringComparison.Ordinal))))
                driveRootRelativeWorkingPath = driveRootRelativeWorkingPath.Substring(drive.Root.Length);
            if (escapeCurrentLocation)
                driveRootRelativeWorkingPath = WildcardPattern.Escape(driveRootRelativeWorkingPath);
            // These are static strings that we will parse and
            // interpret if they are leading the path. Otherwise
            // we will just pass them on to the provider.
            const char monadRelativePathSeparatorBackslash = '\\';
            const char monadRelativePathSeparatorForwardslash = '/';
            const string currentDirSymbol = ".";
            const string parentDirSymbol = "..";
            const int parentDirSymbolLength = 2;
            const string currentDirRelativeSymbolBackslash = ".\\";
            const string currentDirRelativeSymbolForwardslash = "./";
            // If the path starts with the "\" then it is
            // relative to the root of the drive.
            // We don't want to process other relative path
            // symbols in this case
                // Just fall-through
            else if (path[0] == monadRelativePathSeparatorBackslash ||
                     path[0] == monadRelativePathSeparatorForwardslash)
                // The root relative path was given so empty the current working path.
                // Porting notes: This can happen on non-Windows, because the assumption
                // is that for file paths a path that is already relative to the drive
                // root is the same thing as an absolute path (both start with /).
                driveRootRelativeWorkingPath = string.Empty;
                // Remove the \ or / from the drive relative
                // path
                    "path = {0}",
                // Now process all other relative path symbols like
                // ".." and "."
                while ((path.Length > 0) && HasRelativePathTokens(path))
                    bool processedSomething = false;
                    // Process the parent directory symbol ".."
                    bool pathStartsWithDirSymbol = path.StartsWith(parentDirSymbol, StringComparison.Ordinal);
                    bool pathLengthEqualsParentDirSymbol = path.Length == parentDirSymbolLength;
                    bool pathDirSymbolFollowedBySeparator =
                        (path.Length > parentDirSymbolLength) &&
                        ((path[parentDirSymbolLength] == monadRelativePathSeparatorBackslash) ||
                         (path[parentDirSymbolLength] == monadRelativePathSeparatorForwardslash));
                    if (pathStartsWithDirSymbol &&
                        (pathLengthEqualsParentDirSymbol ||
                         pathDirSymbolFollowedBySeparator))
                        if (!string.IsNullOrEmpty(driveRootRelativeWorkingPath))
                            // Use the provider to get the current path
                            driveRootRelativeWorkingPath =
                                _sessionState.Internal.GetParentPath(
                                    driveRootRelativeWorkingPath,
                            "Parent path = {0}",
                            driveRootRelativeWorkingPath);
                        // remove the parent path symbol from the
                        // relative path
                            path.Substring(
                            parentDirSymbolLength);
                        processedSomething = true;
                        // If the ".." was followed by a "\" or "/" then
                        // strip that off as well
                        if (path[0] == monadRelativePathSeparatorBackslash ||
                        // no more relative path to work with so break
                        // continue the loop instead of trying to process
                        // ".\". This makes the code easier for ".\" by
                        // not having to check for ".."
                    // Process the current directory symbol "."
                    if (path.Equals(currentDirSymbol, StringComparison.OrdinalIgnoreCase))
                        path = string.Empty;
                    if (path.StartsWith(currentDirRelativeSymbolBackslash, StringComparison.Ordinal) ||
                        path.StartsWith(currentDirRelativeSymbolForwardslash, StringComparison.Ordinal))
                        path = path.Substring(currentDirRelativeSymbolBackslash.Length);
                    // If there is no more path to work with break
                    // out of the loop
                    if (!processedSomething)
                        // Since that path wasn't modified, break
                        // the loop.
            // If more relative path remains add that to
            // the known absolute path
                    _sessionState.Internal.MakePath(
            if (navigationProvider != null)
                string rootedPath = _sessionState.Internal.MakePath(context.Drive.Root, driveRootRelativeWorkingPath, context);
                string normalizedRelativePath = navigationProvider.ContractRelativePath(rootedPath, context.Drive.Root, false, context);
                if (!string.IsNullOrEmpty(normalizedRelativePath))
                    if (normalizedRelativePath.StartsWith(context.Drive.Root, StringComparison.Ordinal))
                        driveRootRelativeWorkingPath = normalizedRelativePath.Substring(context.Drive.Root.Length);
                        driveRootRelativeWorkingPath = normalizedRelativePath;
                "result = {0}",
            return driveRootRelativeWorkingPath;
        private static bool HasRelativePathTokens(string path)
            string comparePath = path.Replace('/', '\\');
            return (
                comparePath.Equals(".", StringComparison.OrdinalIgnoreCase) ||
                comparePath.Equals("..", StringComparison.OrdinalIgnoreCase) ||
                comparePath.Contains("\\.\\") ||
                comparePath.Contains("\\..\\") ||
                comparePath.EndsWith("\\..", StringComparison.OrdinalIgnoreCase) ||
                comparePath.EndsWith("\\.", StringComparison.OrdinalIgnoreCase) ||
                comparePath.StartsWith("..\\", StringComparison.OrdinalIgnoreCase) ||
                comparePath.StartsWith(".\\", StringComparison.OrdinalIgnoreCase) ||
                comparePath.StartsWith('~'));
        /// Uses the drive and a relative working path to construct
        /// a string which has a fully qualified provider specific path.
        /// The drive to use as the root of the path.
        /// <param name="workingPath">
        /// The relative working directory to the specified drive.
        /// A string which is contains the fully qualified path in provider
        /// specific form.
        /// If <paramref name="drive"/> or <paramref name="workingPath"/> is null.
        private string GetProviderSpecificPath(
            string workingPath,
            if (workingPath == null)
                throw PSTraceSource.NewArgumentNullException(nameof(workingPath));
            // Trace the inputs
                "workingPath = {0}",
                workingPath);
            string result = drive.Root;
                        workingPath,
                // This is valid if the provider doesn't support MakePath.  The
                // drive should be enough.
        /// Parses the provider-qualified path into the provider name and
        /// the provider-internal path.
        /// The provider-qualified path to parse.
        /// The name of the provider specified by the path is returned through
        /// this out parameter.
        /// The provider-internal path.
        /// If <paramref name="path"/> is not in the correct format.
        private static string ParseProviderPath(string path, out string providerId)
            int providerIdSeparatorIndex = path.IndexOf(StringLiterals.ProviderPathSeparator, StringComparison.Ordinal);
            if (providerIdSeparatorIndex <= 0)
                        SessionStateStrings.NotProviderQualifiedPath);
            providerId = path.Substring(0, providerIdSeparatorIndex);
            string result = path.Substring(providerIdSeparatorIndex + StringLiterals.ProviderPathSeparator.Length);
        #endregion Path manipulation methods
        #endregion private fields and methods
        /// <param name="containerProvider">
        /// The provider that will be used to glob the <paramref name="path"/>.
        /// An array of provider specific paths that match the glob path and
        /// filter (if supplied via the context).
        /// This method is internal because we don't want to expose the
        /// provider instances outside the engine.
        /// If <paramref name="path"/>, <paramref name="containerProvider"/>, or
            ContainerCmdletProvider containerProvider,
            if (containerProvider == null)
                throw PSTraceSource.NewArgumentNullException(nameof(containerProvider));
            Collection<string> expandedPaths =
                ExpandGlobPath(
                    containerProvider,
            return expandedPaths;
        /// Determines if the specified path contains any globing characters. These
        /// characters are defined as '?' and '*'.
        /// The path to search for globing characters.
        /// True if the path contains any of the globing characters, false otherwise.
        internal static bool StringContainsGlobCharacters(string path)
            return WildcardPattern.ContainsWildcardCharacters(path);
        /// Determines if the path and context are such that we need to run through
        /// the globbing algorithm.
        /// The path to check for glob characters.
        /// The context to check for filter, include, or exclude expressions.
        /// True if globbing should be performed (the path has glob characters, or the context
        /// has either a an include, or an exclude expression). False otherwise.
        internal static bool ShouldPerformGlobbing(string path, CmdletProviderContext context)
            bool pathContainsGlobCharacters = false;
                pathContainsGlobCharacters = StringContainsGlobCharacters(path);
            bool contextContainsIncludeExclude = false;
            bool contextContainsNoGlob = false;
                bool includePresent = context.Include != null && context.Include.Count > 0;
                s_pathResolutionTracer.WriteLine("INCLUDE filter present: {0}", includePresent);
                bool excludePresent = context.Exclude != null && context.Exclude.Count > 0;
                s_pathResolutionTracer.WriteLine("EXCLUDE filter present: {0}", excludePresent);
                contextContainsIncludeExclude = includePresent || excludePresent;
                contextContainsNoGlob = context.SuppressWildcardExpansion;
                s_pathResolutionTracer.WriteLine("NOGLOB parameter present: {0}", contextContainsNoGlob);
            s_pathResolutionTracer.WriteLine("Path contains wildcard characters: {0}", pathContainsGlobCharacters);
            return (pathContainsGlobCharacters || contextContainsIncludeExclude) && (!contextContainsNoGlob);
        /// Generates an array of provider specific paths from the single provider specific
        /// path using globing rules.
        /// A path that may or may not contain globing characters.
        /// The drive that the path is relative to.
        /// The provider that implements the namespace for the path that we are globing over.
        /// An array of path strings that match the globing rules applied to the path parameter.
        /// First the path is checked to see if it contains any globing characters ('?' or '*').
        /// If it doesn't then the path is returned as the only element in the array.
        /// If it does, GetParentPath and GetLeafPathName is called on the path and each element
        /// is stored until the path doesn't contain any globing characters. At that point
        /// GetChildNames() is called on the provider with the last parent path that doesn't
        /// contain a globing character. All the results are then matched against leaf element
        /// of that parent path (which did contain a glob character). We then walk out of the
        /// recursion and apply the same procedure to each leaf element that contained globing
        /// characters.
        /// The procedure above allows us to match globing strings in multiple sub-containers
        /// in the namespace without having to have knowledge of the namespace paths, or
        /// their syntax.
        /// Example:
        /// dir c:\foo\*\bar\*a??.cs
        /// Calling this method for the path above would return all files that end in 'a' and
        /// any other two characters followed by ".cs" in all the subdirectories of
        /// foo that have a bar subdirectory.
        /// If <paramref name="path"/>, <paramref name="provider"/>, or
        /// <paramref name="provider"/> is null.
        private Collection<string> ExpandMshGlobPath(
            ContainerCmdletProvider provider,
            NavigationCmdletProvider navigationProvider = provider as NavigationCmdletProvider;
            using (s_pathResolutionTracer.TraceScope("EXPANDING WILDCARDS"))
                if (ShouldPerformGlobbing(path, context))
                    // This collection contains the directories for which a leaf is being added.
                    // If the directories are being globed over as well, then there will be
                    // many directories in this collection which will have to be iterated over
                    // every time there is a child being added
                    List<string> dirs = new List<string>();
                    // Each leaf element that is pulled off the path is pushed on the stack in
                    // order such that we can generate the path again.
                    Stack<string> leafElements = new Stack<string>();
                    using (s_pathResolutionTracer.TraceScope("Tokenizing path"))
                        // If the path contains glob characters then iterate through pulling the
                        // leaf elements off and pushing them on to the leafElements stack until
                        // there are no longer any glob characters in the path.
                        while (StringContainsGlobCharacters(path))
                            // Use the provider to get the leaf element string
                            string leafElement = path;
                                leafElement = navigationProvider.GetChildName(path, context);
                            s_tracer.WriteLine("Pushing leaf element: {0}", leafElement);
                            s_pathResolutionTracer.WriteLine("Leaf element: {0}", leafElement);
                            // Push the leaf element onto the leaf element stack for future use
                            leafElements.Push(leafElement);
                            // Now use the parent path for the next iteration
                                // Now call GetParentPath with the root
                                string newParentPath = navigationProvider.GetParentPath(path, drive.Root, context);
                                        newParentPath,
                                    // The provider is implemented in an inconsistent way.
                                    // GetChildName returned a non-empty/non-null result but
                                    // GetParentPath with the same path returns the same path.
                                    // This would cause the globber to go into an infinite loop,
                                    // so instead an exception is thrown.
                                            SessionStateStrings.ProviderImplementationInconsistent,
                                            provider.ProviderInfo.Name,
                                path = newParentPath;
                                // If the provider doesn't implement NavigationCmdletProvider then at most
                                // it can have only one segment in its path. So after removing
                                // the leaf all we have left is the empty string.
                            s_tracer.WriteLine("New path: {0}", path);
                            s_pathResolutionTracer.WriteLine("Parent path: {0}", path);
                        s_tracer.WriteLine("Base container path: {0}", path);
                        // If no glob elements were found there must be an include and/or
                        // exclude specified. Use the parent path to iterate over to
                        // resolve the include/exclude filters
                        if (leafElements.Count == 0)
                                if (!string.IsNullOrEmpty(leafElement))
                                    path = navigationProvider.GetParentPath(path, null, context);
                        s_pathResolutionTracer.WriteLine("Root path of resolution: {0}", path);
                    // Once the container path with no glob characters are found store it
                    // so that it's children can be iterated over.
                    dirs.Add(path);
                    // Reconstruct the path one leaf element at a time, expanding wherever
                    // we encounter glob characters
                    while (leafElements.Count > 0)
                        string leafElement = leafElements.Pop();
                            leafElement != null,
                            "I am only pushing strings onto this stack so I should be able " +
                            "to cast any Pop to a string without failure.");
                        dirs =
                            GenerateNewPSPathsWithGlobLeaf(
                                dirs,
                                leafElement,
                                leafElements.Count == 0,
                        // If there are more leaf elements in the stack we need
                        // to make sure that only containers where added to dirs
                        // in GenerateNewPathsWithGlobLeaf
                        if (leafElements.Count > 0)
                            using (s_pathResolutionTracer.TraceScope("Checking matches to ensure they are containers"))
                                while (index < dirs.Count)
                                    string resolvedPath =
                                        GetMshQualifiedPath(dirs[index], drive);
                                    // Check to see if the matching item is a container
                                    if (navigationProvider != null &&
                                        !_sessionState.Internal.IsItemContainer(
                                            resolvedPath,
                                            context))
                                        // If not, remove it from the collection
                                            "Removing {0} because it is not a container",
                                            dirs[index]);
                                        s_pathResolutionTracer.WriteLine("{0} is not a container", dirs[index]);
                                        dirs.RemoveAt(index);
                                    else if (navigationProvider == null)
                                            navigationProvider != null,
                                            "The path in the dirs should never be a container unless " +
                                            "the provider implements the NavigationCmdletProvider interface. If it " +
                                            "doesn't, there should be no more leafElements in the stack " +
                                            "when this check is done");
                                        s_pathResolutionTracer.WriteLine("{0} is a container", dirs[index]);
                                        // If so, leave it and move on to the next one
                        dirs != null,
                        "GenerateNewPathsWithGlobLeaf() should return the base path as an element " +
                        "even if there are no globing characters");
                    foreach (string dir in dirs)
                        s_pathResolutionTracer.WriteLine("RESOLVED PATH: {0}", dir);
                        result.Add(dir);
                        dirs.Count == result.Count,
                        "The result of copying the globed strings should be the same " +
                        "as from the collection");
                    string unescapedPath = context.SuppressWildcardExpansion ? path : RemoveGlobEscaping(path);
                    string formatString = "{0}:" + StringLiterals.DefaultPathSeparator + "{1}";
                    // Check to see if its a hidden provider drive.
                        if (IsProviderDirectPath(unescapedPath))
                            formatString = "{1}";
                        if (path.StartsWith(StringLiterals.DefaultPathSeparator))
                    // Porting note: if the volume is not separated by a colon (non-Windows filesystems), don't add it.
                    if (!drive.VolumeSeparatedByColon)
                            unescapedPath);
                    // Since we didn't do globbing, be sure the path exists
                    if (allowNonexistingPaths ||
                        provider.ItemExists(GetProviderPath(resolvedPath, context), context))
                        s_pathResolutionTracer.WriteLine("RESOLVED PATH: {0}", resolvedPath);
                        result.Add(resolvedPath);
                "This method should at least return the path or more if it has glob characters");
        /// Gets either a drive-qualified or provider-qualified path based on the drive
        /// The path to create a qualified path from.
        /// The drive used to qualify the path.
        /// Either a drive-qualified or provider-qualified Msh path.
        /// The drive's Hidden property is used to determine if the path returned
        /// should be provider (hidden=true) or drive (hidden=false) qualified.
        internal static string GetMshQualifiedPath(string path, PSDriveInfo drive)
                "The caller should verify drive before calling this method");
                    result = GetProviderQualifiedPath(path, drive.Provider);
                result = GetDriveQualifiedPath(path, drive);
        /// Removes the provider or drive qualifier from a Msh path.
        /// The path to remove the qualifier from.
        /// The drive information used to determine if a provider qualifier
        /// or drive qualifier should be removed from the path.
        /// The path with the Msh qualifier removed.
        internal static string RemoveMshQualifier(string path, PSDriveInfo drive)
                "The caller should verify path before calling this method");
                result = RemoveProviderQualifier(path);
                result = RemoveDriveQualifier(path);
        /// Given an Msh relative or absolute path, returns a drive-qualified absolute path.
        /// No globbing or relative path character expansion is done.
        /// The path to get the drive qualified path from.
        /// The drive the path should be qualified with.
        /// A drive-qualified absolute Msh path.
        internal static string GetDriveQualifiedPath(string path, PSDriveInfo drive)
            bool treatAsRelative = true;
            if (drive.VolumeSeparatedByColon)
                // Ensure the drive name is the same as the portion of the path before
                // :. If not add the drive name and colon as if it was a relative path
                        treatAsRelative = false;
                        if (path.AsSpan(0, index).Equals(drive.Name, StringComparison.OrdinalIgnoreCase))
                    // Check if the path begins with "\" or "/" (UNC Path or Path in Unix).
                    // Ignore if the path resolves to a drive path, this will happen when path is equal to "\" or "/".
                    // Drive path still need formatting, so treat them as relative.
                    if (path.Length > 1 && (path.StartsWith(StringLiterals.DefaultPathSeparator) ||
                        path.StartsWith(StringLiterals.AlternatePathSeparator)))
                if (IsAbsolutePath(path))
            if (treatAsRelative)
                string formatString;
                    formatString = "{0}:" + StringLiterals.DefaultPathSeparator + "{1}";
        /// Removes the drive qualifier from a drive qualified MSH path.
        /// The path to remove the drive qualifier from.
        /// The path without the drive qualifier.
        private static string RemoveDriveQualifier(string path)
                "Caller should verify path");
            // Find the drive separator only if it's before a path separator
                int separator = path.IndexOf(StringLiterals.DefaultPathSeparator, 0, index);
                    separator = path.IndexOf(StringLiterals.AlternatePathSeparator, 0, index);
                    // Remove the \ or / if it follows the drive indicator
                    if (path[index + 1] == '\\' ||
                        path[index + 1] == '/')
                    result = path.Substring(index + 1);
        /// Given an Msh path, returns a provider-qualified path.
        /// The provider the path should be qualified with.
        /// If <paramref name="path"/> or <paramref name="provider"/> is null.
        internal static string GetProviderQualifiedPath(string path, ProviderInfo provider)
            bool pathResolved = false;
            // Check to see if the path is already provider qualified
            int providerSeparatorIndex = path.IndexOf("::", StringComparison.Ordinal);
                string possibleProvider = path.Substring(0, providerSeparatorIndex);
                if (provider.NameEquals(possibleProvider))
                    pathResolved = true;
            if (!pathResolved)
                        StringLiterals.ProviderPathSeparator,
        /// Removes the provider qualifier from a provider-qualified MSH path.
        /// The path to remove the provider qualifier from.
        /// The path without the provider qualifier.
        internal static string RemoveProviderQualifier(string path)
            int index = path.IndexOf(StringLiterals.ProviderPathSeparator, StringComparison.Ordinal);
                // The +2 removes the ::
                result = path.Substring(index + StringLiterals.ProviderPathSeparator.Length);
        /// Generates a collection of containers and/or leaves that are children of the containers
        /// in the currentDirs parameter and match the glob expression in the
        /// <paramref name="leafElement"/> parameter.
        /// <param name="currentDirs">
        /// A collection of paths that should be searched for leaves that match the
        /// <paramref name="leafElement"/> expression.
        /// The drive the Msh path is relative to.
        /// <param name="leafElement">
        /// A single element of a path that may or may not contain a glob expression. This parameter
        /// is used to search the containers in <paramref name="currentDirs"/> for children that
        /// match the glob expression.
        /// <param name="isLastLeaf">
        /// True if the <paramref name="leafElement"/> is the last element to glob over. If false, we
        /// need to get all container names from the provider even if they don't match the filter.
        /// The provider associated with the paths that are being passed in the
        /// <paramref name="currentDirs"/> and <paramref name="leafElement"/> parameters.
        /// The provider must derive from ContainerCmdletProvider or NavigationCmdletProvider
        /// in order to get globbing.
        /// A collection of fully qualified namespace paths whose leaf element matches the
        /// If <paramref name="currentDirs"/> or <paramref name="provider"/>
        private List<string> GenerateNewPSPathsWithGlobLeaf(
            List<string> currentDirs,
            string leafElement,
            bool isLastLeaf,
            if (currentDirs == null)
                throw PSTraceSource.NewArgumentNullException(nameof(currentDirs));
            List<string> newDirs = new List<string>();
            // Only loop through the child names if the leafElement contains a glob character
            if (!string.IsNullOrEmpty(leafElement) &&
                StringContainsGlobCharacters(leafElement) ||
                isLastLeaf)
                string regexEscapedLeafElement = ConvertMshEscapeToRegexEscape(leafElement);
                // Construct the glob filter
                WildcardPattern stringMatcher =
                        regexEscapedLeafElement,
                // Loop through the current dirs and add the appropriate children
                foreach (string dir in currentDirs)
                    using (s_pathResolutionTracer.TraceScope("Expanding wildcards for items under '{0}'", dir))
                        // Now continue on with the names that were returned
                        string mshQualifiedParentPath = string.Empty;
                        Collection<PSObject> childNamesObjectArray =
                            GetChildNamesInDir(
                                dir,
                                !isLastLeaf,
                                out mshQualifiedParentPath);
                        if (childNamesObjectArray == null)
                            s_tracer.TraceError("GetChildNames returned a null array");
                            s_pathResolutionTracer.WriteLine("No child names returned for '{0}'", dir);
                        // Loop through each child to see if they match the glob expression
                        foreach (PSObject childObject in childNamesObjectArray)
                            string child = string.Empty;
                            if (IsChildNameAMatch(
                                    childObject,
                                    stringMatcher,
                                    out child))
                                string childPath = child;
                                    string parentPath = RemoveMshQualifier(mshQualifiedParentPath, drive);
                                    childPath = _sessionState.Internal.MakePath(parentPath, child, context);
                                    childPath = GetMshQualifiedPath(childPath, drive);
                                s_tracer.WriteLine("Adding child path to dirs {0}", childPath);
                                // -- If there are more leafElements, the current childPath will be treated as a container path later,
                                //    we should escape the childPath in case the actual childPath contains wildcard characters such as '[' or ']'.
                                // -- If there is no more leafElement, the childPath will not be further processed, and we don't need to
                                //    escape it.
                                childPath = isLastLeaf ? childPath : WildcardPattern.Escape(childPath);
                                newDirs.Add(childPath);
                    "LeafElement does not contain any glob characters so do a MakePath");
                // Loop through the current dirs and add the leafElement to each of
                // the dirs
                    using (s_pathResolutionTracer.TraceScope("Expanding intermediate containers under '{0}'", dir))
                        string backslashEscapedLeafElement = ConvertMshEscapeToRegexEscape(leafElement);
                        string unescapedDir = context.SuppressWildcardExpansion ? dir : RemoveGlobEscaping(dir);
                        string resolvedPath = GetMshQualifiedPath(unescapedDir, drive);
                        string childPath = backslashEscapedLeafElement;
                            string parentPath = RemoveMshQualifier(resolvedPath, drive);
                            childPath = _sessionState.Internal.MakePath(parentPath, backslashEscapedLeafElement, context);
                        if (_sessionState.Internal.ItemExists(childPath, context))
                            s_pathResolutionTracer.WriteLine("Valid intermediate container: {0}", childPath);
            return newDirs;
        /// GetChildPathNames() is called on the provider with the last parent path that doesn't
        /// or if the provider is implemented in such a way as to cause the globber to go
        /// into an infinite loop.
        internal Collection<string> ExpandGlobPath(
            // See if the provider wants to convert the path and filter
            string convertedPath = null;
            string convertedFilter = null;
            string originalFilter = context.Filter;
            bool changedPathOrFilter = provider.ConvertPath(path, context.Filter, ref convertedPath, ref convertedFilter, context);
            if (changedPathOrFilter)
                    s_tracer.WriteLine("Provider converted path and filter.");
                    s_tracer.WriteLine("Original path: {0}", path);
                    s_tracer.WriteLine("Converted path: {0}", convertedPath);
                    s_tracer.WriteLine("Original filter: {0}", context.Filter);
                    s_tracer.WriteLine("Converted filter: {0}", convertedFilter);
                path = convertedPath;
                originalFilter = context.Filter;
                                // See if we can get the root from the context
                                    PSDriveInfo drive = context.Drive;
                                        root = drive.Root;
                                string newParentPath = navigationProvider.GetParentPath(path, root, context);
                    // Reconstruct the path one leaf element at a time, expanding where-ever
                            GenerateNewPathsWithGlobLeaf(
                                        !navigationProvider.IsItemContainer(
                                            dirs[index],
                        provider.ItemExists(unescapedPath, context))
                        s_pathResolutionTracer.WriteLine("RESOLVED PATH: {0}", unescapedPath);
                        result.Add(unescapedPath);
                context.Filter = originalFilter;
        internal List<string> GenerateNewPathsWithGlobLeaf(
                (StringContainsGlobCharacters(leafElement) ||
                 isLastLeaf))
                        string unescapedDir = null;
                            GetChildNamesInDir(dir, leafElement, !isLastLeaf, context, true, null, provider, out unescapedDir);
                            if (IsChildNameAMatch(childObject, stringMatcher, includeMatcher, excludeMatcher, out child))
                                    childPath = navigationProvider.MakePath(unescapedDir, child, context);
                            childPath =
                                navigationProvider.
                                        unescapedDir,
                                        backslashEscapedLeafElement,
                        if (provider.ItemExists(childPath, context))
        /// Gets the child names in the specified path by using the provider.
        /// <param name="dir">
        /// The path of the directory to get the child names from. If this is an Msh Path,
        /// dirIsProviderPath must be false, If this is a provider-internal path,
        /// dirIsProviderPath must be true.
        /// The element that we are ultimately looking for. Used to set filters on the context
        /// if desired.
        /// <param name="getAllContainers">
        /// Determines if the GetChildNames call should get all containers even if they don't
        /// match the filter.
        /// The context to be used for the command. The context is copied to a new context, the
        /// results are accumulated and then returned.
        /// <param name="dirIsProviderPath">
        /// Specifies whether the dir parameter is a provider-internal path (true) or Msh Path (false).
        /// The drive to use to qualify the Msh path if dirIsProviderPath is false.
        /// The provider to use to get the child names.
        /// <param name="modifiedDirPath">
        /// Returns the modified dir path. If dirIsProviderPath is true, this is the unescaped dir path.
        /// If dirIsProviderPath is false, this is the unescaped resolved provider path.
        /// A collection of PSObjects whose BaseObject is a string that contains the name of the child.
        /// If <paramref name="dir"/> or <paramref name="drive"/> is null.
        private Collection<PSObject> GetChildNamesInDir(
            string dir,
            bool getAllContainers,
            bool dirIsProviderPath,
            out string modifiedDirPath)
            bool changedPathOrFilter = provider.ConvertPath(leafElement, context.Filter, ref convertedPath, ref convertedFilter, context);
                    s_tracer.WriteLine("Original path: {0}", leafElement);
                leafElement = convertedPath;
                context.Filter = convertedFilter;
            ReturnContainers returnContainers = ReturnContainers.ReturnAllContainers;
            if (!getAllContainers)
                returnContainers = ReturnContainers.ReturnMatchingContainers;
            CmdletProviderContext getChildNamesContext =
            // Remove the include/exclude filters from the new context
            getChildNamesContext.SetFilters(
                context.Filter);
                // Use the provider to get the children
                modifiedDirPath = null;
                if (dirIsProviderPath)
                    modifiedDirPath = unescapedDir = context.SuppressWildcardExpansion ? dir : RemoveGlobEscaping(dir);
                        "Caller should verify that drive is not null when dirIsProviderPath is false");
                    // If the directory is an MSH path we must resolve it before calling GetChildNames()
                    // -- If the path is passed in by LiteralPath (context.SuppressWildcardExpansion == false), we surely should use 'dir' unchanged.
                    // -- If the path is passed in by Path (context.SuppressWildcardExpansion == true), we still should use 'dir' unchanged, in case that the special character
                    //    in 'dir' is escaped
                    modifiedDirPath = GetMshQualifiedPath(dir, drive);
                    ProviderInfo providerIgnored = null;
                    CmdletProvider providerInstanceIgnored = null;
                    Collection<string> resolvedPaths =
                        GetGlobbedProviderPathsFromMonadPath(
                            modifiedDirPath,
                            getChildNamesContext,
                            out providerIgnored,
                            out providerInstanceIgnored);
                    // After resolving the path, we unescape the modifiedDirPath if necessary.
                    modifiedDirPath = context.SuppressWildcardExpansion
                                          ? modifiedDirPath
                                          : RemoveGlobEscaping(modifiedDirPath);
                        unescapedDir = resolvedPaths[0];
                        // If there were no results from globbing but no
                        // exception was thrown, that means there was filtering.
                        // So return an empty collection and let the caller deal
                        // with it.
                if (provider.HasChildItems(unescapedDir, getChildNamesContext))
                    provider.GetChildNames(
                        getChildNamesContext);
                // First check to see if there were any errors, and write them
                // to the real context if there are.
                if (getChildNamesContext.HasErrors())
                    Collection<ErrorRecord> errors = getChildNamesContext.GetAccumulatedErrorObjects();
                        foreach (ErrorRecord errorRecord in errors)
                            context.WriteError(errorRecord);
                Collection<PSObject> childNamesObjectArray = getChildNamesContext.GetAccumulatedObjects();
                return childNamesObjectArray;
                getChildNamesContext.RemoveStopReferral();
        /// Determines if the specified PSObject contains a string that matches the specified
        /// wildcard patterns.
        /// <param name="childObject">
        /// The PSObject that contains the child names.
        /// <param name="stringMatcher">
        /// The glob matcher.
        /// The include matcher wildcard patterns.
        /// The exclude matcher wildcard patterns.
        /// <param name="childName">
        /// The name of the child which was extracted from the childObject and used for the matches.
        /// True if the string in the childObject matches the stringMatcher and includeMatcher wildcard patterns,
        /// and does not match the exclude wildcard patterns. False otherwise.
        private static bool IsChildNameAMatch(
            PSObject childObject,
            WildcardPattern stringMatcher,
            out string childName)
                childName = null;
                object baseObject = childObject.BaseObject;
                if (baseObject is PSCustomObject)
                    s_tracer.TraceError("GetChildNames returned a null object");
                childName = baseObject as string;
                if (childName == null)
                    s_tracer.TraceError("GetChildNames returned an object that wasn't a string");
                s_pathResolutionTracer.WriteLine("Name returned from provider: {0}", childName);
                // Check the glob expression
                // First see if the child matches the glob expression
                bool isGlobbed = WildcardPattern.ContainsWildcardCharacters(stringMatcher.Pattern);
                bool isChildMatch = stringMatcher.IsMatch(childName);
                s_tracer.WriteLine("isChildMatch = {0}", isChildMatch);
                bool isIncludeSpecified = (includeMatcher.Count > 0);
                bool isExcludeSpecified = (excludeMatcher.Count > 0);
                s_tracer.WriteLine("isIncludeMatch = {0}", isIncludeMatch);
                // Check if the child name matches, or the include matches
                if (isChildMatch || (isGlobbed && isIncludeSpecified && isIncludeMatch))
                    s_pathResolutionTracer.WriteLine("Path wildcard match: {0}", childName);
                    // See if it should not be included
                    if (isIncludeSpecified && !isIncludeMatch)
                        s_pathResolutionTracer.WriteLine("Not included match: {0}", childName);
                    // See if it should be excluded
                    if (isExcludeSpecified &&
                        SessionStateUtilities.MatchesAnyWildcardPattern(childName, excludeMatcher, false))
                        s_pathResolutionTracer.WriteLine("Excluded match: {0}", childName);
                    s_pathResolutionTracer.WriteLine("NOT path wildcard match: {0}", childName);
            s_tracer.WriteLine("result = {0}; childName = {1}", result.ToString(), childName);
        /// Converts a back tick '`' escape into back slash escape for
        /// all occurrences in the string.
        /// A string that may or may not have back ticks as escape characters.
        /// A string that has the back ticks replaced with back slashes except
        /// in the case where there are two back ticks in a row. In that case a single
        /// back tick is returned.
        /// The following rules apply to the conversion:
        /// 1. All \ characters are expanded to be \\
        /// 2. Any ` not followed by a ` is converted to a \
        /// 3. Any ` that is followed by a ` collapses the two into a single `
        /// 4. Any other character is immediately appended to the result.
        private static string ConvertMshEscapeToRegexEscape(string path)
            const char mshEscapeChar = '`';
            const char regexEscapeChar = '\\';
            ReadOnlySpan<char> workerArray = path;
            for (int index = 0; index < workerArray.Length; ++index)
                // look for an escape character
                if (workerArray[index] == mshEscapeChar)
                    if (index + 1 < workerArray.Length)
                        if (workerArray[index + 1] == mshEscapeChar)
                            // Since there are two escape characters in a row,
                            // the string really wanted a back tick so add that to
                            // the result and continue.
                            result.Append(mshEscapeChar);
                            // Skip the next character since it has already been processed.
                            // Since the escape character wasn't followed by another
                            // escape character, convert it to a back slash and continue.
                            result.Append(regexEscapeChar);
                        // Since the escape character was the last character in the string
                        // just convert it. Most likely this is an error condition in the
                        // Regex class but I will let that fail instead of pretending to
                        // know what the user meant.
                else if (workerArray[index] == regexEscapeChar)
                    // For backslashes we need to append two back slashes so that
                    // the regex processor doesn't think its an escape character
                    result.Append("\\\\");
                    // The character is not an escape character so add it to the result
                    // and continue.
                    result.Append(workerArray[index]);
                "Original path: {0} Converted to: {1}",
                result.ToString());
        /// Determines if the path is relative to a provider home based on
        /// the ~ character.
        /// The path to determine if it is a home path.
        /// True if the path contains a ~ at the beginning of the path or immediately
        /// following a provider designator ("provider::")
        /// Is <paramref name="path"/> is null.
        internal static bool IsHomePath(string path)
                // Strip off the provider portion of the path
                    path = path.Substring(index + StringLiterals.ProviderPathSeparator.Length);
            if (path.StartsWith(StringLiterals.HomePath, StringComparison.Ordinal))
                // Support the single "~"
                if (path.Length == 1)
                // Support "~/" or "~\"
                else if ((path.Length > 1) &&
                        (path[1] == '\\' ||
                         path[1] == '/'))
        /// Determines if the specified path looks like a remote path. (starts with
        /// // or \\.
        /// The path to check to determine if it is a remote path.
        /// True if the path starts with // or \\, or false otherwise.
        internal static bool IsProviderDirectPath(string path)
            return path.StartsWith(StringLiterals.DefaultRemotePathPrefix, StringComparison.Ordinal) ||
                   path.StartsWith(StringLiterals.AlternateRemotePathPrefix, StringComparison.Ordinal);
        /// Generates the path for the home location for a provider when given a
        /// path starting with ~ or "provider:~" followed by a relative path.
        /// The path to generate into a home path.
        /// The path representing the path to the home location for a provider. This
        /// may be either a fully qualified provider path or a PowerShell path.
        internal string GetHomeRelativePath(string path)
            if (IsHomePath(path) && _sessionState.Drive.Current != null)
                ProviderInfo provider = _sessionState.Drive.Current.Provider;
                        // Since the provider was specified store it and remove it
                        // from the path.
                        string providerName = path.Substring(0, index);
                        provider = _sessionState.Internal.GetSingleProvider(providerName);
                    // Strip of the ~ and the \ or / if present
                    if (path.Length > 1 &&
                        path = path.Substring(2);
                    // Now piece together the provider's home path and the remaining
                    // portion of the passed in path
                    if (provider.Home != null &&
                        provider.Home.Length > 0)
                        s_pathResolutionTracer.WriteLine("Getting home path for provider: {0}", provider.Name);
                        s_pathResolutionTracer.WriteLine("Provider HOME path: {0}", provider.Home);
                            path = provider.Home;
                            path = _sessionState.Internal.MakePath(provider, provider.Home, path, context);
                        s_pathResolutionTracer.WriteLine("HOME relative path: {0}", path);
                                SessionStateStrings.HomePathNotSet,
                                provider.Name);
                        s_pathResolutionTracer.TraceError("HOME path not set for provider: {0}", provider.Name);
        private static void TraceFilters(CmdletProviderContext context)
            if ((s_pathResolutionTracer.Options & PSTraceSourceOptions.WriteLine) != 0)
                // Trace the filter
                s_pathResolutionTracer.WriteLine("Filter: {0}", context.Filter ?? string.Empty);
                    // Trace the include filters
                    StringBuilder includeString = new StringBuilder();
                    foreach (string includeFilter in context.Include)
                        includeString.Append($"{includeFilter} ");
                    s_pathResolutionTracer.WriteLine("Include: {0}", includeString.ToString());
                if (context.Exclude != null)
                    // Trace the exclude filters
                    StringBuilder excludeString = new StringBuilder();
                    foreach (string excludeFilter in context.Exclude)
                        excludeString.Append($"{excludeFilter} ");
                    s_pathResolutionTracer.WriteLine("Exclude: {0}", excludeString.ToString());

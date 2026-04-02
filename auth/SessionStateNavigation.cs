        #region NavigationCmdletProvider accessors
        #region GetParentPath
        /// Gets the path to the parent object for the given object.
        /// The path to the object to get the parent path from
        /// The path to the parent object
        internal string GetParentPath(string path, string root)
            string result = GetParentPath(path, root, context);
        /// The root of the drive. Namespace providers should
        /// return the root if GetParentPath is called for the root.
        internal string GetParentPath(
            return GetParentPath(path, root, context, false);
        /// Specify whether to use default provider when needed.
            CmdletProviderContext getProviderPathContext =
                        getProviderPathContext,
                    // the path is sure to be drive_qualified and it is absolute path, otherwise the
                    // drive would be set to the current drive and the DriveNotFoundException will not happen
                    if (useDefaultProvider)
                        // the default provider is FileSystem
                        provider = PublicSessionState.Internal.GetSingleProvider(Microsoft.PowerShell.Commands.FileSystemProvider.ProviderName);
                if (getProviderPathContext.HasErrors())
                    getProviderPathContext.WriteErrorsToContext(context);
                bool isProviderQualified = false;
                bool isDriveQualified = false;
                string qualifier = null;
                string pathNoQualifier = RemoveQualifier(path, provider, out qualifier, out isProviderQualified, out isDriveQualified);
                string result = GetParentPath(provider, pathNoQualifier, root, context);
                if (!string.IsNullOrEmpty(qualifier) && !string.IsNullOrEmpty(result))
                    result = AddQualifier(result, provider, qualifier, isProviderQualified, isDriveQualified);
                getProviderPathContext.RemoveStopReferral();
        private static string AddQualifier(string path, ProviderInfo provider, string qualifier, bool isProviderQualified, bool isDriveQualified)
            string formatString = "{1}";
            if (isProviderQualified)
                formatString = "{0}::{1}";
            else if (isDriveQualified)
                // Porting note: on non-windows filesystem paths, there should be no colon in the path
                if (provider.VolumeSeparatedByColon)
                    formatString = "{0}:{1}";
                    formatString = "{0}{1}";
                    qualifier,
        /// The provider that should handle the RemoveQualifier call.
        /// <param name="qualifier">
        /// Returns the qualifier of the path.
        /// <param name="isProviderQualified">
        /// Returns true if the path is a provider-qualified path.
        /// <param name="isDriveQualified">
        /// Returns true if the path is a drive-qualified path.
        private string RemoveQualifier(string path, ProviderInfo provider, out string qualifier, out bool isProviderQualified, out bool isDriveQualified)
            qualifier = null;
            isProviderQualified = false;
            isDriveQualified = false;
            if (LocationGlobber.IsProviderQualifiedPath(path, out qualifier))
                isProviderQualified = true;
                if (Globber.IsAbsolutePath(path, out qualifier))
                    isDriveQualified = true;
                    // Remove the drive name and colon, or just the drive name
                    // Porting note: on non-windows there is no colon for qualified paths
                        result = path.Substring(qualifier.Length + 1);
                        result = path.Substring(qualifier.Length);
        /// The provider that should handle the GetParentPath call.
        /// This is internal so that it can be called from the LocationGlobber.
                "Caller should validate provider before calling this method");
                "Caller should validate root before calling this method");
            CmdletProvider providerInstance = GetProviderInstance(provider);
            return GetParentPath(providerInstance, path, root, context);
        /// The instance of the provider that should handle the GetParentPath call.
            NavigationCmdletProvider navigationCmdletProvider =
                result = navigationCmdletProvider.GetParentPath(path, root, context);
                    "GetParentPathProviderException",
                    SessionStateStrings.GetParentPathProviderException,
        #endregion GetParentPath
        internal string NormalizeRelativePath(string path, string basePath)
            string result = NormalizeRelativePath(path, basePath, context);
                string workingPath = Globber.GetProviderPath(
                if (workingPath == null ||
                    provider == null)
                    // Since the provider didn't write an error, and we didn't get any
                    // results ourselves, we need to write out our own error.
                    Exception e = PSTraceSource.NewArgumentException(nameof(path));
                    context.WriteError(new ErrorRecord(e, "NormalizePathNullResult", ErrorCategory.InvalidArgument, path));
                if (basePath != null)
                    PSDriveInfo baseDrive = null;
                    ProviderInfo baseProvider = null;
                         basePath,
                         out baseProvider,
                         out baseDrive);
                    if (drive != null && baseDrive != null)
                        if (!drive.Name.Equals(baseDrive.Name, StringComparison.OrdinalIgnoreCase))
                            // Make sure they are from physically different drives
                            // Doing StartsWith from both directions covers the following cases
                            // C:\ and C:\Temp
                            // C:\Temp and C:\
                            if (!(drive.Root.StartsWith(baseDrive.Root, StringComparison.OrdinalIgnoreCase) ||
                                (baseDrive.Root.StartsWith(drive.Root, StringComparison.OrdinalIgnoreCase))))
                                // In this case, no normalization is necessary
                    // Detect if the original path was already a
                    // provider path. This happens when a drive doesn't
                    // have a rooted root -- such as HKEY_LOCAL_MACHINE instead of
                    // \\HKEY_LOCAL_MACHINE
                        (GetProviderInstance(provider) is NavigationCmdletProvider) &&
                        (!string.IsNullOrEmpty(drive.Root)) &&
                        (path.StartsWith(drive.Root, StringComparison.OrdinalIgnoreCase)))
                        // If the drive root doesn't end with a path separator then there is a chance the
                        // path starts with the drive root name but doesn't actually refer to it.  For example,
                        // (see Win8 bug 922001) consider drive with root HKEY_LOCAL_MACHINE named
                        // HKEY_LOCAL_MACHINE_foo.  The path would start with the drive root but is not a provider
                        // We will remediate this by only considering this a provider path if
                        // 1.  The drive root ends with a path separator.
                        // OR
                        // 2.  The path starts with the drive root followed by a path separator
                        // 3.  The path exactly matches the drive root.
                        // 1. Test for the drive root ending with a path separator.
                        bool driveRootEndsWithPathSeparator = IsPathSeparator(drive.Root[drive.Root.Length - 1]);
                        // 2. Test for the path starting with the drive root followed by a path separator
                        int indexAfterDriveRoot = drive.Root.Length;
                        bool pathStartsWithDriveRootAndPathSeparator = indexAfterDriveRoot < path.Length && IsPathSeparator(path[indexAfterDriveRoot]);
                        // 3. Test for the drive root exactly matching the path.
                        //    Since we know the path starts with the drive root then they are equal if the lengths are equal.
                        bool pathEqualsDriveRoot = drive.Root.Length == path.Length;
                        if (driveRootEndsWithPathSeparator || pathStartsWithDriveRootAndPathSeparator || pathEqualsDriveRoot)
                            workingPath = path;
                return NormalizeRelativePath(provider, workingPath, basePath, context);
        /// Tests the specified character for equality with one of the powershell path separators and
        /// returns true if it matches.
        /// <param name="c">The character to test.</param>
        /// <returns>True if the character is a path separator.</returns>
        private static bool IsPathSeparator(char c)
            return c == StringLiterals.DefaultPathSeparator || c == StringLiterals.AlternatePathSeparator;
        /// The provider to use to normalize the path.
        /// An provider internal path to normalize.
            // Get an instance of the provider
            Provider.CmdletProvider providerInstance = GetProviderInstance(provider);
            NavigationCmdletProvider navigationCmdletProvider = providerInstance as NavigationCmdletProvider;
            if (navigationCmdletProvider != null)
                    path = navigationCmdletProvider.NormalizeRelativePath(path, basePath, context);
                        "NormalizeRelativePathProviderException",
                    SessionStateStrings.NormalizeRelativePathProviderException,
            else if (providerInstance is ContainerCmdletProvider)
                // Do nothing and return the path as-is
        #region MakePath
        /// Generates a path from the given parts.
        /// The parent segment of the path to be joined with the child.
        /// The child segment of the ath to be joined with the parent.
        /// The generated path.
        internal string MakePath(
            string parent,
            string child)
            return MakePath(parent, child, context);
            string child,
            if (parent == null &&
                child == null)
                throw PSTraceSource.NewArgumentException(nameof(parent));
            // Set the drive data for the context
                provider = CurrentDrive.Provider;
            if (context.Drive == null)
                bool isProviderQualified = LocationGlobber.IsProviderQualifiedPath(parent);
                bool isAbsolute = LocationGlobber.IsAbsolutePath(parent);
                if (isProviderQualified || isAbsolute)
                    // Ignore the result. Just using this to get the providerId and drive
                    Globber.GetProviderPath(parent, context, out provider, out drive);
                    if (drive == null && isProviderQualified)
                        drive = provider.HiddenDrive;
                result = MakePath(provider, parent, child, context);
                if (isAbsolute)
                    result = LocationGlobber.GetDriveQualifiedPath(result, context.Drive);
                else if (isProviderQualified)
                    result = LocationGlobber.GetProviderQualifiedPath(result, provider);
                provider = context.Drive.Provider;
        /// Uses the specified provider to put the two parts of a path together.
        /// The parent part of the path to join with the child.
        /// The child part of the path to join with the parent.
        /// The combined path.
            Provider.CmdletProvider providerInstance = provider.CreateInstance();
            return MakePath(providerInstance, parent, child, context);
                    result = navigationCmdletProvider.MakePath(parent, child, context);
                        "MakePathProviderException",
                        SessionStateStrings.MakePathProviderException,
                        parent,
                result = child;
        #endregion MakePath
        #region GetChildName
        /// Gets the name of the leaf element in the specified path.
        /// The fully qualified path to the item
        /// The leaf element in the path.
        internal string GetChildName(string path)
            string result = GetChildName(path, context);
        internal string GetChildName(
            return GetChildName(path, context, false);
            string workingPath = null;
                workingPath = Globber.GetProviderPath(path, context, out provider, out drive);
                // the path is sure to be drive_qualified and it is an absolute path, otherwise the
                // drive would be set to the current drive and the DriveNotFoundException will not happen.
                    workingPath = path.Replace(StringLiterals.AlternatePathSeparator, StringLiterals.DefaultPathSeparator);
                    workingPath = workingPath.TrimEnd(StringLiterals.DefaultPathSeparator);
                workingPath != null,
                "There should always be a way to generate a UniversalResourceName for a " +
            "There should always be a way to get the provider ID for a given path or else GetProviderPath should have thrown an exception");
            return GetChildName(provider, workingPath, context);
        /// Gets the leaf element of the specified path.
        private string GetChildName(
                "Caller should validate path before callin g this method");
            CmdletProvider providerInstance = provider.CreateInstance();
            return GetChildName(providerInstance, path, context, true);
        /// <param name="acceptNonContainerProviders">
        /// Specify True if the method should just return the Path if the
        /// provider doesn't support container overloads.
            bool acceptNonContainerProviders
                GetNavigationProviderInstance(providerInstance, acceptNonContainerProviders);
            if (navigationCmdletProvider == null)
                result = navigationCmdletProvider.GetChildName(path, context);
                    "GetChildNameProviderException",
                    SessionStateStrings.GetChildNameProviderException,
        #endregion GetChildName
        /// Moves the item specified by path to the specified destination.
        /// The path(s) to the item(s) to be moved.
        /// The path of the destination container.
        internal Collection<PSObject> MoveItem(string[] paths, string destination, bool force, bool literalPath)
            MoveItem(paths, destination, context);
        /// Nothing. All items that are moved are written into the context object.
        internal void MoveItem(
            if (destination == null)
                throw PSTraceSource.NewArgumentNullException(nameof(destination));
            Collection<PathInfo> providerDestinationPaths =
                    destination,
            if (providerDestinationPaths.Count > 1)
                        nameof(destination),
                        SessionStateStrings.MoveItemOneDestination);
                context.WriteError(new ErrorRecord(argException, argException.GetType().FullName, ErrorCategory.InvalidArgument, destination));
                    // Check to be sure we resolved at least one item to move and that the
                    // destination is a container.
                    if (providerPaths.Count > 1 &&
                        providerDestinationPaths.Count > 0 &&
                        !IsItemContainer(providerDestinationPaths[0].Path))
                                SessionStateStrings.MoveItemPathMultipleDestinationNotContainer);
                        context.WriteError(new ErrorRecord(argException, argException.GetType().FullName, ErrorCategory.InvalidArgument, providerDestinationPaths[0]));
                        CmdletProviderContext destinationContext = new CmdletProviderContext(this.ExecutionContext);
                        string destinationProviderInternalPath = null;
                        if (providerDestinationPaths.Count > 0)
                            destinationProviderInternalPath =
                                    providerDestinationPaths[0].Path,
                            // Since the path doesn't exist, just convert it to a
                            // provider path and continue.
                        // Now verify the providers are the same.
                                destinationProvider.FullName,
                                    SessionStateStrings.MoveItemSourceAndDestinationNotSameProvider);
                            context.WriteError(new ErrorRecord(argException, argException.GetType().FullName, ErrorCategory.InvalidArgument, providerPaths));
                                MoveItemPrivate(providerInstance, providerPath, destinationProviderInternalPath, context);
        /// Moves the item at the specified path to the destination path.
        /// The path to where the item should be moved.
        private void MoveItemPrivate(
                navigationCmdletProvider.MoveItem(path, destination, context);
                    "MoveItemProviderException",
                    SessionStateStrings.MoveItemProviderException,
                return MoveItemDynamicParameters(providerInstance, providerPaths[0], destination, newContext);
        private object MoveItemDynamicParameters(
                result = navigationCmdletProvider.MoveItemDynamicParameters(path, destination, context);
                    "MoveItemDynamicParametersProviderException",
                    SessionStateStrings.MoveItemDynamicParametersProviderException,
        #endregion NavigationCmdletProvider accessors

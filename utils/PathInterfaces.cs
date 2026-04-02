    /// Exposes the path manipulation and location APIs to the Cmdlet base class.
    public sealed class PathIntrinsics
        private PathIntrinsics()
        /// Internal constructor for the PathIntrinsics facade.
        /// The session for which this is a facade.
        /// This is only public for testing purposes.
        internal PathIntrinsics(SessionStateInternal sessionState)
        /// Gets the current location.
        /// If a location has not been set yet.
        public PathInfo CurrentLocation
                return _sessionState.CurrentLocation;
        /// Gets the current location for a specific provider.
        /// The name of the provider to get the current location for.
        /// If <paramref name="providerName"/> is null.
        /// If <paramref name="namespacesID"/> refers to a provider that does not exist.
        /// If a current drive cannot be found for the provider <paramref name="providerName"/>
        public PathInfo CurrentProviderLocation(string providerName)
            return _sessionState.GetNamespaceCurrentLocation(providerName);
        /// Gets the current location for the file system provider.
        /// If a current drive cannot be found for the FileSystem provider
        public PathInfo CurrentFileSystemLocation
                return CurrentProviderLocation(_sessionState.ExecutionContext.ProviderNames.FileSystem);
        /// Changes the current location to the specified path.
        /// The path to change the location to. This can be either a drive-relative or provider-relative
        /// path. It cannot be a provider-internal path.
        /// The path of the new current location.
        /// If <paramref name="path"/> does not exist, is not a container, or
        /// resolved to multiple containers.
        /// If <paramref name="path"/> refers to a provider that does not exist.
        /// If <paramref name="path"/> refers to a drive that does not exist.
        /// If the provider associated with <paramref name="path"/> threw an
        /// exception.
        public PathInfo SetLocation(string path)
            return _sessionState.SetLocation(path);
        internal PathInfo SetLocation(string path, CmdletProviderContext context)
            return _sessionState.SetLocation(path, context);
        /// Indicates if the path is a literal path.
        internal PathInfo SetLocation(string path, CmdletProviderContext context, bool literalPath)
            return _sessionState.SetLocation(path, context, literalPath);
        /// Determines if the specified path is the current location or a parent of the current location.
        /// A drive or provider-qualified path to be compared against the current location.
        /// True if the path is the current location or a parent of the current location. False otherwise.
        /// If the path is a provider-qualified path for a provider that is
        /// not loaded into the system.
        /// If the provider used to build the path threw an exception.
        /// If the provider that the <paramref name="path"/> represents is not a NavigationCmdletProvider
        /// or ContainerCmdletProvider.
        /// If the <paramref name="path"/> starts with "~" and the home location is not set for
        /// If the provider specified by <paramref name="providerId"/> threw an
        /// exception when its GetParentPath or MakePath was called while
        /// processing the <paramref name="path"/>.
        internal bool IsCurrentLocationOrAncestor(string path, CmdletProviderContext context)
            return _sessionState.IsCurrentLocationOrAncestor(path, context);
        /// Pushes the current location onto the location stack so that it can be retrieved later.
        /// <param name="stackName">
        /// The ID of the stack to push the location onto.
        public void PushCurrentLocation(string stackName)
            _sessionState.PushCurrentLocation(stackName);
        /// Gets the location off the top of the location stack.
        /// The ID of the stack to pop the location from. If stackName is null or empty
        /// the default stack is used.
        /// The path information for the location that was on the top of the location stack.
        /// If the path on the stack does not exist, is not a container, or
        /// If <paramref name="stackName"/> contains wildcard characters and resolves
        /// to multiple location stacks.
        /// A stack was not found with the specified name.
        /// If the path on the stack refers to a provider that does not exist.
        /// If the path on the stack refers to a drive that does not exist.
        /// If the provider associated with the path on the stack threw an
        public PathInfo PopLocation(string stackName)
            return _sessionState.PopLocation(stackName);
        /// Gets the location stack and all the locations on it.
        /// The stack ID of the stack to get the stack info for.
        public PathInfoStack LocationStack(string stackName)
            return _sessionState.LocationStack(stackName);
        /// Sets the default location stack to that specified by the stack ID.
        /// The stack ID of the stack to use as the default location stack.
        /// If <paramref name="stackName"/> does not exist as a location stack.
        public PathInfoStack SetDefaultLocationStack(string stackName)
            return _sessionState.SetDefaultLocationStack(stackName);
        /// Resolves a drive or provider qualified absolute or relative path that may contain
        /// wildcard characters into one or more absolute drive or provider qualified paths.
        /// The drive or provider qualified path to be resolved. This path may contain wildcard
        /// characters which will get resolved.
        /// An array of PowerShell paths that resolved from the given path.
        /// If <paramref name="path"/> is a provider-qualified path
        /// and the specified provider does not exist.
        /// If <paramref name="path"/> is a drive-qualified path and
        /// the specified drive does not exist.
        /// If the provider throws an exception when its MakePath gets
        /// called.
        /// If the provider does not support multiple items.
        /// If the home location for the provider is not set and
        /// <paramref name="path"/> starts with a "~".
        /// If <paramref name="path"/> does not contain wildcard characters and
        public Collection<PathInfo> GetResolvedPSPathFromPSPath(string path)
            // The parameters will be verified by the path resolver
            Provider.CmdletProvider providerInstance = null;
            return PathResolver.GetGlobbedMonadPathsFromMonadPath(path, false, out providerInstance);
        /// An array of Msh paths that resolved from the given path.
        internal Collection<PathInfo> GetResolvedPSPathFromPSPath(
            return PathResolver.GetGlobbedMonadPathsFromMonadPath(path, false, context, out providerInstance);
        /// wildcard characters into one or more provider-internal paths.
        /// The provider for which the returned paths should be used.
        /// An array of provider-internal paths that resolved from the given path.
        /// If the provider associated with the <paramref name="path"/> threw an
        /// exception when building its path.
        public Collection<string> GetResolvedProviderPathFromPSPath(
            out ProviderInfo provider)
            return PathResolver.GetGlobbedProviderPathsFromMonadPath(path, false, out provider, out providerInstance);
        internal Collection<string> GetResolvedProviderPathFromPSPath(
            return PathResolver.GetGlobbedProviderPathsFromMonadPath(path, allowNonexistingPaths, out provider, out providerInstance);
            CmdletProviderContext context,
            return PathResolver.GetGlobbedProviderPathsFromMonadPath(path, false, context, out provider, out providerInstance);
        /// If <paramref name="providerId"/> references a provider that does not exist.
        /// If the <paramref name="providerId"/> references a provider that is not
        /// a ContainerCmdletProvider.
        public Collection<string> GetResolvedProviderPathFromProviderPath(
            string providerId)
            return PathResolver.GetGlobbedProviderPathsFromProviderPath(path, false, providerId, out providerInstance);
        /// If <paramref name="path"/>, <paramref name="providerId"/>, or
        /// <paramref name="context"/> is null.
        ///  </exception>
        internal Collection<string> GetResolvedProviderPathFromProviderPath(
            return PathResolver.GetGlobbedProviderPathsFromProviderPath(path, false, providerId, context, out providerInstance);
        /// Converts a drive or provider qualified absolute or relative path that may contain
        /// wildcard characters into one a provider-internal path still containing the wildcard characters.
        /// The drive or provider qualified path to be converted. This path may contain wildcard
        /// characters which will not get resolved.
        /// A provider-internal path that does not have the wildcard characters resolved.
        /// If the provider specified by <paramref name="path"/> threw an
            return PathResolver.GetProviderPath(path);
        /// The information for the provider for which the returned path should be used.
        /// The drive of the PowerShell path that was used to convert the path. Note, this may be null
        /// if the <paramref name="path"/> was a provider-qualified path.
        /// If the provider specified by <paramref name="provider"/> threw an
        public string GetUnresolvedProviderPathFromPSPath(
            out ProviderInfo provider,
            out PSDriveInfo drive)
            CmdletProviderContext context = new CmdletProviderContext(_sessionState.ExecutionContext);
            string result = PathResolver.GetProviderPath(path, context, out provider, out drive);
            context.ThrowFirstErrorOrDoNothing();
        /// The drive of the Msh path that was used to convert the path.
        internal string GetUnresolvedProviderPathFromPSPath(
            return PathResolver.GetProviderPath(path, context, out provider, out drive);
        /// Determines if the give path is a PowerShell provider-qualified path.
        /// The path to check.
        /// True if the specified path is provider-qualified, false otherwise.
        /// A provider-qualified path is a path in the following form:
        /// providerId::provider-internal-path
        public bool IsProviderQualified(string path)
            return LocationGlobber.IsProviderQualifiedPath(path);
        /// Determines if the given path is a drive-qualified absolute path.
        /// If the path is an absolute path then the returned value is
        /// the name of the drive that the path is absolute to.
        /// True if the specified path is an absolute drive-qualified path.
        /// A path is an absolute drive-qualified path if it has the following
        /// form:
        /// drive-name:drive-relative-path
        public bool IsPSAbsolute(string path, out string driveName)
            return PathResolver.IsAbsolutePath(path, out driveName);
        #region Combine
        /// Combines two strings with a provider specific path separator.
        /// The parent path to be joined with the child.
        /// <param name="child">
        /// The child path to be joined with the parent.
        /// The combined path of the parent and child with the provider
        /// specific path separator between them.
        /// If both <paramref name="parent"/> and <paramref name="child"/> is null.
        /// If the <paramref name="providerId"/> does not support this operation.
        /// If the pipeline is being stopped while executing the command.
        public string Combine(string parent, string child)
            return _sessionState.MakePath(parent, child);
        internal string Combine(string parent, string child, CmdletProviderContext context)
            return _sessionState.MakePath(parent, child, context);
        #endregion Combine
        #region ParseParent
        /// Gets the parent path of the specified path.
        /// The path to get the parent path from.
        /// If the root is specified the path returned will not be any higher than the root.
        /// The parent path of the specified path.
        /// If the <paramref name="providerInstance"/> does not support this operation.
        public string ParseParent(string path, string root)
            return _sessionState.GetParentPath(path, root);
        internal string ParseParent(
            return _sessionState.GetParentPath(path, root, context, false);
        /// Allow to use FileSystem as the default provider when the
        /// given path is drive-qualified and the drive cannot be found.
        /// <param name="useDefaultProvider">
        /// to use default provider when needed.
            bool useDefaultProvider)
            return _sessionState.GetParentPath(path, root, context, useDefaultProvider);
        #endregion ParseParent
        #region ParseChildName
        /// Gets the child name of the specified path.
        /// The path to get the child name from.
        /// The last element of the path.
        public string ParseChildName(string path)
            return _sessionState.GetChildName(path);
        internal string ParseChildName(
            return _sessionState.GetChildName(path, context, false);
            return _sessionState.GetChildName(path, context, useDefaultProvider);
        #endregion ParseChildName
        #region NormalizeRelativePath
        /// Normalizes the path that was passed in and returns the normalized path
        /// as a relative path to the basePath that was passed.
        /// A PowerShell path to an item. The item should exist
        /// or the provider should write out an error.
        /// <param name="basePath">
        /// The path that the return value should be relative to.
        /// A normalized path that is relative to the basePath that was passed.
        public string NormalizeRelativePath(string path, string basePath)
            return _sessionState.NormalizeRelativePath(path, basePath);
        /// An MSH path to an item. The item should exist
        internal string NormalizeRelativePath(
            string basePath,
            return _sessionState.NormalizeRelativePath(path, basePath, context);
        #endregion NormalizeRelativePath
        /// Determines if the path is a syntactically and semantically valid path for the provider.
        /// The path to validate.
        /// true if the object specified by path is syntactically and semantically valid, false otherwise.
        public bool IsValid(string path)
            return _sessionState.IsValidPath(path);
        /// Determines if the MSH path is a syntactically and semantically valid path for the provider.
        /// The context under which the call is being made.
        internal bool IsValid(
            return _sessionState.IsValidPath(path, context);
        private LocationGlobber PathResolver
                return _pathResolver ??= _sessionState.ExecutionContext.LocationGlobber;
        private LocationGlobber _pathResolver;

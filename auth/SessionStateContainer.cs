        #region ContainerCmdletProvider accessors
        #region ItemExists
        /// Determines if the monad virtual namespace path exists.
        /// The path to the object to determine if it exists.
        /// true if the object specified by path exists, false otherwise.
        internal bool ItemExists(string path, bool force, bool literalPath)
            context.Force = force;
            context.SuppressWildcardExpansion = literalPath;
            bool result = ItemExists(path, context);
        internal bool ItemExists(
            CmdletProvider providerInstance = null;
                Collection<string> providerPaths =
                    Globber.GetGlobbedProviderPathsFromMonadPath(
                        out providerInstance);
                    result = ItemExists(providerInstance, providerPath, context);
        /// Determines if the item at the specified path exists.
        /// <param name="providerInstance">
        /// The provider instance to use.
            CmdletProvider providerInstance,
            // All parameters should have been validated by caller
                providerInstance != null,
                "Caller should validate providerId before calling this method");
                "Caller should validate path before calling this method");
                "Caller should validate context before calling this method");
            ItemCmdletProvider itemCmdletProvider =
                GetItemProviderInstance(providerInstance);
                result = itemCmdletProvider.ItemExists(path, context);
            catch (ActionPreferenceStopException)
                throw NewProviderInvocationException(
                    "ItemExistsProviderException",
                    SessionStateStrings.ItemExistsProviderException,
                    itemCmdletProvider.ProviderInfo,
        internal object ItemExistsDynamicParameters(string path, CmdletProviderContext context)
            CmdletProviderContext newContext =
                new CmdletProviderContext(context);
            newContext.SetFilters(
                    newContext,
            if (providerPaths.Count > 0)
                // Get the dynamic parameters for the first resolved path
                return ItemExistsDynamicParameters(providerInstance, providerPaths[0], newContext);
        /// The instance of the provider to use.
        private object ItemExistsDynamicParameters(
                "Caller should validate providerInstance before calling this method");
            ContainerCmdletProvider containerCmdletProvider =
                GetContainerProviderInstance(providerInstance);
                result = containerCmdletProvider.ItemExistsDynamicParameters(path, context);
                    "ItemExistsDynamicParametersProviderException",
                    SessionStateStrings.ItemExistsDynamicParametersProviderException,
                    containerCmdletProvider.ProviderInfo,
        #endregion Exists
        #region IsValidPath
        internal bool IsValidPath(string path)
            bool result = IsValidPath(path, context);
        internal bool IsValidPath(
            string providerPath =
                Globber.GetProviderPath(
            ItemCmdletProvider providerInstance = GetItemProviderInstance(provider);
            return IsValidPath(providerInstance, providerPath, context);
        /// Determines if the specified path is valid.
        private bool IsValidPath(
                result = itemCmdletProvider.IsValidPath(path, context);
                    "IsValidPathProviderException",
                    SessionStateStrings.IsValidPathProviderException,
        #endregion IsValidPath
        #region IsItemContainer
        /// Determines if the monad virtual namespace path is a container.
        /// The path to the object to determine if it is a container.
        /// true if the object specified by path is a container, false otherwise.
        internal bool IsItemContainer(string path)
            bool result = IsItemContainer(path, context);
        internal bool IsItemContainer(
                    result = IsItemContainer(providerInstance, providerPath, context);
        /// Determines if the item at the specified path is a container.
        private bool IsItemContainer(
            NavigationCmdletProvider navigationCmdletProvider = null;
                // If it is a NavigationProvider instance then we
                // can ask the provider if the path is to a container.
                navigationCmdletProvider =
                    GetNavigationProviderInstance(providerInstance, false);
                    result = navigationCmdletProvider.IsItemContainer(path, context);
                        "IsItemContainerProviderException",
                    SessionStateStrings.IsItemContainerProviderException,
                        navigationCmdletProvider.ProviderInfo,
                    // If it is a ContainerProvider instance then the
                    // root (empty path) is considered a container
                    // but everything else is not.
                    // If the provider is neither a NavigationProvider nor a
                    // ContainerProvider then the path cannot be a container
        /// Deletes the specified object.
        /// A relative or absolute path to the object to be deleted.
        /// The delete should occur in all sub-containers of the specified path.
        internal void RemoveItem(string[] paths, bool recurse, bool force, bool literalPath)
            if (paths == null)
            RemoveItem(paths, recurse, context);
        internal void RemoveItem(
            string[] paths,
                    RemoveItem(providerInstance, providerPath, recurse, context);
        /// Internal remove item method that just calls the provider directly without globbing.
        /// The name of the provider to use.
        /// True if all items should be removed recursively.
                providerId != null,
            CmdletProvider providerInstance = GetProviderInstance(providerId);
            RemoveItem(providerInstance, path, recurse, context);
                if (context.HasIncludeOrExclude)
                    int childrenNotMatchingFilterCriteria = 0;
                    // Use the new code path only if either Include or Exclude is specified
                    // This will take care of all the child items.
                    // This will also take care of the case where "path" is not a container.
                    ProcessPathItems(providerInstance, path, recurse, context, out childrenNotMatchingFilterCriteria, ProcessMode.Delete, skipIsItemContainerCheck: false);
                    // Now delete the container if it matches the filter(s)
                    // and the container does not have any children.
                    if (IsItemContainer(providerInstance, path, context))
                        string item = GetChildName(path, context, false);
                                item,
                                    context.Include, WildcardOptions.IgnoreCase),
                        if (isIncludeMatch)
                            if (!SessionStateUtilities.MatchesAnyWildcardPattern(
                                    context.Exclude, WildcardOptions.IgnoreCase),
                                false))
                                // Earlier, we used to check if "path" has any child items remaining. If not, we remove "path".
                                // This does not work for some providers (for e.g. IIS provider) which do not support removing the child items
                                // So, instead of checking for any child items remaining (which are applicable to only those providers which support them - like File, Registry),
                                // we check if there are any items that were never intended to be deleted. If there are no such items, then, we can remove "path".
                                // WinBlue: 289907
                                if (childrenNotMatchingFilterCriteria == 0)
                                    containerCmdletProvider.RemoveItem(path, false, context);
                    containerCmdletProvider.RemoveItem(path, recurse, context);
                    "RemoveItemProviderException",
                    SessionStateStrings.RemoveItemProviderException,
                return RemoveItemDynamicParameters(providerInstance, providerPaths[0], recurse, newContext);
        /// If true, all items in the subtree should be removed.
        private object RemoveItemDynamicParameters(
                result = containerCmdletProvider.RemoveItemDynamicParameters(path, recurse, context);
        /// Gets the children of the specified item.
        /// An array of relative or absolute paths to the object to get the children of.
        /// This method does not provider streaming of the results. If you want streaming
        /// then you must call the overload that takes a CmdletProviderContext.
        internal Collection<PSObject> GetChildItems(string[] paths, bool recurse, uint depth, bool force, bool literalPath)
                GetChildItems(path, recurse, depth, context);
            return context.GetAccumulatedObjects();
        /// A relative or absolute path to the object to get the children of.
        internal void GetChildItems(
            if ((recurse && !context.SuppressWildcardExpansion) || LocationGlobber.ShouldPerformGlobbing(path, context))
                bool modifiedInclude = false;
                    // If we're recursing, do some path fixups to match user
                    // expectations, but only if the last part is a file and not a directory:
                    if (recurse && !path.EndsWith(Path.DirectorySeparatorChar) && !path.EndsWith(Path.AltDirectorySeparatorChar))
                        string childName = GetChildName(path, context);
                        // If -File or -Directory is specified and path is ended with '*', we should include the parent path as search path
                        bool isFileOrDirectoryPresent = false;
                        if (context.DynamicParameters is Microsoft.PowerShell.Commands.GetChildDynamicParameters dynParam)
                            isFileOrDirectoryPresent = dynParam.File.IsPresent || dynParam.Directory.IsPresent;
                        if (string.Equals(childName, "*", StringComparison.OrdinalIgnoreCase) && isFileOrDirectoryPresent)
                            string parentName = path.Substring(0, path.Length - childName.Length);
                            path = parentName;
                        // dir c:\tem* -include *.ps1 -rec => No change
                        if ((context.Include == null) || (context.Include.Count == 0))
                            // dir c:\tem* -rec => dir c:\ -include tem* -rec
                            // dir tem* -rec => dir -include tem* -rec
                            // dir temp -rec
                            // Should glob paths and files that match tem*, but then
                            // recurse into all subdirectories and do the same for
                            // those directories.
                            if (!string.IsNullOrEmpty(path) && !IsItemContainer(path))
                                if (!string.Equals(childName, "*", StringComparison.OrdinalIgnoreCase))
                                    if (context.Include != null)
                                        context.Include.Add(childName);
                                        modifiedInclude = true;
                    Collection<string> include = context.Include;
                    Collection<string> exclude = context.Exclude;
                    string filter = context.Filter;
                        context.SetFilters(
                    Collection<string> providerPaths = null;
                        providerPaths = Globber.GetGlobbedProviderPathsFromMonadPath(
                    // Ensure this is a container provider before recursing.
                    // GetContainerProviderInstance throws otherwise (as was done in V2.)
                        ContainerCmdletProvider unused = GetContainerProviderInstance(provider);
                    bool getChildrenBecauseNoGlob = !LocationGlobber.StringContainsGlobCharacters(path);
                    // If we are doing recursion and we have include or exclude
                    // filters the recursion must be done manually.
                    // Win8: 279933 Perf degradation: recursive get-childitem is 100% slower than win7
                    // To fix this Perf regression, using getChildrenBecauseNoGlob (+recurse) variables
                    // to decide when to use ManualGetChildItems and when not to.
                        (recurse && !getChildrenBecauseNoGlob && (include != null) && (include.Count == 0)) ||
                        (include != null && include.Count > 0) ||
                        (exclude != null && exclude.Count > 0))
                        // Do the recursion manually so that we can apply the
                        // include and exclude filters
                            // Making sure to obey the StopProcessing.
                            if (context.Stopping)
                            ProcessPathItems(providerInstance, providerPath, recurse, depth, context, out _, ProcessMode.Enumerate);
                        // If the path wasn't globbed or we are recursing then we want to get the
                        // children of the path. If we were globbing and we are not recursing
                        // then we just want to get the item for the resolved paths.
                            if ((getChildrenBecauseNoGlob || recurse) && IsItemContainer(providerInstance, providerPath, context))
                                GetChildItems(providerInstance, providerPath, recurse, depth, context);
                                GetItemPrivate(providerInstance, providerPath, context);
                    if (modifiedInclude)
                        context.Include.Clear();
                string originalPath = path;
                        context.SuppressWildcardExpansion ? path : WildcardPattern.Unescape(path),
                if (drive != null)
                    context.Drive = drive;
                ContainerCmdletProvider providerInstance = GetContainerProviderInstance(provider);
                    (context.Include != null && context.Include.Count > 0) ||
                    (context.Exclude != null && context.Exclude.Count > 0))
                        // Temporary set literal path as false to apply filter
                        context.SuppressWildcardExpansion = false;
                        ProcessPathItems(providerInstance, path, recurse, depth, context, out _, ProcessMode.Enumerate);
                else if (path != null && this.ItemExists(providerInstance, path, context))
                        GetChildItems(providerInstance, path, recurse, depth, context);
                        GetItemPrivate(providerInstance, path, context);
                        new ItemNotFoundException(
                    throw pathNotFound;
        /// Gets the child items of the item at the specified path.
        /// If true, all the child items in the subtree are returned.
        private void GetChildItems(
                containerCmdletProvider.GetChildItems(path, recurse, depth, context);
                    "GetChildrenProviderException",
                    SessionStateStrings.GetChildrenProviderException,
        private bool IsPathContainer(
            bool itemContainer = false;
                itemContainer = IsItemContainer(providerInstance, path, context);
            catch (UnauthorizedAccessException accessException)
                context.WriteError(new ErrorRecord(accessException, "GetItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
            catch (ProviderInvocationException accessException)
                // if providerinvocationexception is wrapping access denied error, it is ok to not terminate the pipeline
                if (accessException.InnerException != null &&
                    accessException.InnerException.GetType().Equals(typeof(System.UnauthorizedAccessException)))
            return itemContainer;
        /// Since we can't do include and exclude filtering on items we have to
        /// do the recursion ourselves. We get each child name and see if it matches
        /// the include and exclude filters. If the child is a container we recurse
        /// into that container.
        /// The path to the item to get the children from.
        /// Recurse into sub-containers when getting children.
        /// <param name="childrenNotMatchingFilterCriteria">
        /// The count of items that do not match any include/exclude criteria.
        /// <param name="processMode">Indicates if this is a Enumerate/Remove operation.</param>
        /// <param name="skipIsItemContainerCheck">A hint used to skip IsItemContainer checks.</param>
        private void ProcessPathItems(
            out int childrenNotMatchingFilterCriteria,
            ProcessMode processMode = ProcessMode.Enumerate,
            bool skipIsItemContainerCheck = false)
            // Call ProcessPathItems with 'depth' set to maximum value for infinite recursion when needed.
            ProcessPathItems(providerInstance, path, recurse, uint.MaxValue, context, out childrenNotMatchingFilterCriteria, processMode, skipIsItemContainerCheck);
            ContainerCmdletProvider containerCmdletProvider = GetContainerProviderInstance(providerInstance);
            childrenNotMatchingFilterCriteria = 0;
                "The caller should have verified the providerInstance");
                "The caller should have verified the path");
                "The caller should have verified the context");
            // Construct the include filter
            Collection<WildcardPattern> includeMatcher =
                    context.Include,
            // Construct the exclude filter
            Collection<WildcardPattern> excludeMatcher =
                    context.Exclude,
            // If the item is a container we have to filter its children
            // Use a hint + lazy evaluation to skip a container check
            if (skipIsItemContainerCheck || IsPathContainer(providerInstance, path, context))
                Collection<PSObject> childNameObjects = null;
                System.Collections.Generic.Dictionary<string, bool> filteredChildNameDictionary = null;
                    // Get all the child names
                    GetChildNames(
                        providerInstance,
                        (recurse) ? ReturnContainers.ReturnAllContainers : ReturnContainers.ReturnMatchingContainers,
                        newContext);
                    newContext.WriteErrorsToContext(context);
                    childNameObjects = newContext.GetAccumulatedObjects();
                    // The code above initially retrieves all of the containers so that it doesn't limit the recursion,
                    // but then emits the non-matching container further down. The public API doesn't support a way to
                    // differentiate the two, so we need to do a diff.
                    // So if there was a filter, do it again to get the fully filtered items.
                    if (recurse && (providerInstance.IsFilterSet()))
                        newContext.RemoveStopReferral();
                        newContext = new CmdletProviderContext(context);
                        filteredChildNameDictionary = new System.Collections.Generic.Dictionary<string, bool>();
                            ReturnContainers.ReturnMatchingContainers,
                        var filteredChildNameObjects = newContext.GetAccumulatedObjects();
                        foreach (PSObject filteredChildName in filteredChildNameObjects)
                            string filteredName = filteredChildName.BaseObject as string;
                            if (filteredName != null)
                                filteredChildNameDictionary[filteredName] = true;
                // Now loop through all the child objects matching the filters and recursing
                // into containers
                for (int index = 0; index < childNameObjects.Count; ++index)
                    if (childNameObjects[index].BaseObject is not string childName)
                    // Generate the provider path for the child
                    string qualifiedPath = MakePath(providerInstance, path, childName, context);
                    if (qualifiedPath == null)
                    bool isIncludeMatch = !context.SuppressWildcardExpansion &&
                            childName,
                            includeMatcher,
                            excludeMatcher,
                            bool emitItem = true;
                            if (filteredChildNameDictionary != null)
                                bool isChildNameInDictionary = false;
                                emitItem = filteredChildNameDictionary.TryGetValue(childName, out isChildNameInDictionary);
                            if (emitItem)
                                if (processMode == ProcessMode.Delete)
                                    containerCmdletProvider.RemoveItem(qualifiedPath, false, context);
                                else if (processMode != ProcessMode.Delete)
                                    // The object is a match so get it and write it out.
                                    GetItemPrivate(providerInstance, qualifiedPath, context);
                            childrenNotMatchingFilterCriteria++;
                    // Now recurse if it is a container
                    if (recurse && IsPathContainer(providerInstance, qualifiedPath, context) && depth > 0)
                        // The item is a container so recurse into it.
                        ProcessPathItems(providerInstance, qualifiedPath, recurse, depth - 1, context, out childrenNotMatchingFilterCriteria, processMode, skipIsItemContainerCheck: true);
                // The path is not a container so write it out if its name
                // matches the filter
                string childName = path;
                childName = GetChildName(providerInstance, path, context, true);
                // Write out the object if it is a match
                        if (processMode != ProcessMode.Delete)
                            // The object is a match so, remove it.
            // Get the provider that will handle this path
            Globber.GetProviderPath(path, out provider);
            // See if it supports dynamic parameters. If not, we don't need to
            // glob the path.
            if (!HasGetChildItemDynamicParameters(provider))
                providerPaths =
                // If the provided path is like c:\fakepath\aa*, since we cannot resolve c:\fakepath, an
                // ItemNotFoundException will be thrown out. In this case, we catch the exception
                // and check if the "providerInstance" is identified. If providerInstance is not null,
                // we can carry on with the get-dynamic-parameters method.
            if (providerPaths != null && providerPaths.Count > 0)
                return GetChildItemsDynamicParameters(providerInstance, providerPaths[0], recurse, newContext);
                if (providerInstance != null)
                    // If we get here, the GetProviderPath should always succeed. This method was already invoked
                    // in the call to GetGlobbedProviderPathsFromMonadPath, and since "providerInstance" is not null,
                    // the invocation in method GetGlobbedProviderPathsFromMonadPath should succeed.
                    string providerPath = Globber.GetProviderPath(path, context, out provider, out drive);
                    if (providerPath != null)
                        return GetChildItemsDynamicParameters(providerInstance, providerPath, recurse, newContext);
        // Detect if the GetChildItemDynamicParameters has been overridden.
        private static bool HasGetChildItemDynamicParameters(ProviderInfo providerInfo)
            Type providerType = providerInfo.ImplementingType;
            MethodInfo mi = null;
                mi = providerType.GetMethod("GetChildItemsDynamicParameters",
                 BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                providerType = providerType.BaseType;
            } while (
                (mi == null) &&
                (providerType != null) &&
                (providerType != typeof(ContainerCmdletProvider))
            return (mi != null);
        /// If true, all child items in the subtree should be returned.
        private object GetChildItemsDynamicParameters(
                result = containerCmdletProvider.GetChildItemsDynamicParameters(path, recurse, context);
                    "GetChildrenDynamicParametersProviderException",
                    SessionStateStrings.GetChildrenDynamicParametersProviderException,
        /// Gets names of the children of the specified path.
        /// The paths to the items from which to retrieve the child names.
        /// An array of strings that contains the names of the children of the specified
        /// The child names are the leaf portion of the path. Example, for the file system
        /// the name for the path c:\windows\system32\foo.dll would be foo.dll or for
        /// the directory c:\windows\system32 would be system32. For Active Directory the
        /// child names would be RDN values of the child objects of the container.
        internal Collection<string> GetChildNames(
                GetChildNames(path, returnContainers, recurse, depth, context);
            Collection<PSObject> objectResults = context.GetAccumulatedObjects();
            Collection<string> results = new Collection<string>();
            foreach (PSObject resultObject in objectResults)
                results.Add(resultObject.BaseObject as string);
        /// The path to the item from which to retrieve the child names.
        /// Nothing is returned, but all names should be written to the context object.
        internal void GetChildNames(
            if (LocationGlobber.ShouldPerformGlobbing(path, context))
                // We don't want to process include/exclude filters
                // when globbing the targets of the operation, so
                // copy the context without the filters.
                CmdletProviderContext resolvePathContext =
                resolvePathContext.SetFilters(
                // Resolve the path
                        resolvePathContext,
                if (resolvePathContext.Drive != null)
                    context.Drive = resolvePathContext.Drive;
                bool pathContainsGlobCharacters = LocationGlobber.StringContainsGlobCharacters(path);
                    if ((!pathContainsGlobCharacters || recurse) && IsItemContainer(providerInstance, providerPath, context))
                        // Since the path contained glob characters or we are recursing and the
                        // path is a container, do the name enumeration manually
                        DoGetChildNamesManually(
                            depth);
                        // Since the original path did not contain glob characters,
                        // if the provider is a NavigationCmdletProvider, write
                        // out the child name, else write out the name as it
                        // was resolved.
                        if (providerInstance is NavigationCmdletProvider)
                            string childName =
                                GetChildName(
                                    context, false);
                                context.WriteObject(childName);
                            context.WriteObject(providerPath);
                // Figure out which provider to use
                if (!providerInstance.ItemExists(providerPath, context))
                    // The path did not contain glob characters but recurse was specified
                    // so do the enumeration manually
                    // Since the path did not contain glob characters and recurse wasn't
                    // specified, we can have the provider write out the child names directly
        /// Gets the child names of the item at the specified path by
        /// manually recursing through all the containers instead of
        /// allowing the provider to do the recursion.
        /// <param name="providerPath">
        /// <param name="relativePath">
        /// The path the name is relative to.
        /// If true all names in the subtree should be returned.
        /// Current depth of recursion; special case uint.MaxValue performs full recursion.
        /// <param name="includeMatcher">
        /// A set of filters that the names must match to be returned.
        /// <param name="excludeMatcher">
        /// A set of filters that the names cannot match to be returned.
        private void DoGetChildNamesManually(
            string providerPath,
            string relativePath,
            Collection<WildcardPattern> includeMatcher,
            Collection<WildcardPattern> excludeMatcher,
            uint depth)
                "The providerInstance should have been verified by the caller");
                providerPath != null,
                "The paths should have been verified by the caller");
                "The context should have been verified by the caller");
            string newProviderPath =
                MakePath(
                    relativePath,
            CmdletProviderContext childNamesContext =
                // First get all children that match the filters and write them out
                    newProviderPath,
                    childNamesContext);
                Collection<PSObject> results = childNamesContext.GetAccumulatedObjects();
                    if (result.BaseObject is not string name)
                            string resultPath = MakePath(providerInstance, relativePath, name, context);
                            context.WriteObject(resultPath);
                    // Now get all the children that are containers and recurse into them
                    // Limiter for recursion
                    if (depth > 0) // this includes special case 'depth == uint.MaxValue' for unlimited recursion
                            ReturnContainers.ReturnAllContainers,
                        results = childNamesContext.GetAccumulatedObjects();
                            // Generate the relative path from the provider path
                            string resultRelativePath =
                            // Generate the provider path for the child item to see
                            // if it is a container
                            string resultProviderPath =
                                        resultRelativePath,
                            // If the item is a container recurse into it and output its
                            // child names
                            if (IsItemContainer(providerInstance, resultProviderPath, context))
                                    depth - 1);
                childNamesContext.RemoveStopReferral();
        /// Gets the names of the children of the item at the specified path.
        private void GetChildNames(
                containerCmdletProvider.GetChildNames(path, returnContainers, context);
                    "GetChildNamesProviderException",
                    SessionStateStrings.GetChildNamesProviderException,
                result = GetChildNamesDynamicParameters(providerInstance, providerPaths[0], newContext);
                        result = GetChildNamesDynamicParameters(providerInstance, providerPath, newContext);
        /// Gets the dynamic parameters for the get-childitem -names cmdlet.
        private object GetChildNamesDynamicParameters(
                result = containerCmdletProvider.GetChildNamesDynamicParameters(path, context);
                    "GetChildNamesDynamicParametersProviderException",
                    SessionStateStrings.GetChildNamesDynamicParametersProviderException,
        /// Renames the item at the specified path to the new name provided.
        /// The path to the item to rename.
        /// The name to which the item should be renamed. This name should always be
        /// relative to the parent container.
        /// The item that was renamed at the specified path.
        internal Collection<PSObject> RenameItem(string path, string newName, bool force)
            RenameItem(path, newName, context);
            // Since there was no errors return the accumulated objects
        /// Nothing. All items that are renamed are written into the context object.
        internal void RenameItem(
            // Can only rename one item at a time, so if we glob more than
            // one item write out an error.
            if (providerPaths.Count == 1)
                RenameItem(providerInstance, providerPaths[0], newName, context);
                ArgumentException argException =
                        nameof(path),
                        SessionStateStrings.RenameMultipleItemError);
                context.WriteError(
                        "RenameMultipleItemError",
                        providerPaths));
        /// Renames the item at the specified path.
        private void RenameItem(
                containerCmdletProvider.RenameItem(path, newName, context);
                    "RenameItemProviderException",
                    SessionStateStrings.RenameItemProviderException,
                return RenameItemDynamicParameters(providerInstance, providerPaths[0], newName, newContext);
        private object RenameItemDynamicParameters(
                result = containerCmdletProvider.RenameItemDynamicParameters(path, newName, context);
                    "RenameItemDynamicParametersProviderException",
                    SessionStateStrings.RenameItemDynamicParametersProviderException,
        /// Creates a new item at the specified path.
        /// The path(s) to the container(s) to create the item in.
        /// The name of the item to create.
        /// The provider specific type of the object to be created.
        internal Collection<PSObject> NewItem(string[] paths, string name, string type, object content, bool force)
            NewItem(paths, name, type, content, context);
        /// The path(s) to the item(s) to create.
        /// The provider specific type of the item to be created.
        /// The content to create the new item with.
        /// Nothing. The item created is written to the context object.
        internal void NewItem(
                string resolvePath = null;
                    PSTraceSource.NewArgumentNullException(nameof(paths));
                else if (path.EndsWith((":" + Path.DirectorySeparatorChar), StringComparison.Ordinal) ||
                         path.EndsWith((":" + Path.AltDirectorySeparatorChar), StringComparison.Ordinal))
                    // path is Windows root
                    resolvePath = path;
                    // To be compatible with Linux OS. Which will be either '/' or '\' depends on the OS type.
                    char[] charsToTrim = { ' ', Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar };
                    resolvePath = path.TrimEnd(charsToTrim);
                PSDriveInfo driveInfo;
                Collection<string> providerPaths = new Collection<string>();
                // Only glob the path if the name is specified
                        Globber.GetProviderPath(resolvePath, context, out provider, out driveInfo);
                    providerInstance = GetProviderInstance(provider);
                    providerPaths.Add(providerPath);
                                resolvePath,
                    // Compose the globbed container and the name together to get a path
                    // to pass on to the provider.
                    string composedPath = providerPath;
                        composedPath = MakePath(providerInstance, providerPath, name, context);
                    // Don't support 'New-Item -Type Directory' on the Function provider
                    // if the runspace has ever been in constrained language mode, as the mkdir
                    // function can be abused
                    if (context.ExecutionContext.HasRunspaceEverUsedConstrainedLanguageMode &&
                        (providerInstance is Microsoft.PowerShell.Commands.FunctionProvider) &&
                        (string.Equals(type, "Directory", StringComparison.OrdinalIgnoreCase)))
                            PSTraceSource.NewNotSupportedException(SessionStateStrings.DriveCmdletProvider_NotSupported);
                    bool isSymbolicJunctionOrHardLink = false;
                    // Symbolic link targets are allowed to not exist on both Windows and Linux
                    bool allowNonexistingPath = false;
                        WildcardPattern typeEvaluator = WildcardPattern.Get(type + "*", WildcardOptions.IgnoreCase | WildcardOptions.Compiled);
                        if (typeEvaluator.IsMatch("symboliclink") || typeEvaluator.IsMatch("junction") || typeEvaluator.IsMatch("hardlink"))
                            isSymbolicJunctionOrHardLink = true;
                            allowNonexistingPath = typeEvaluator.IsMatch("symboliclink");
                    if (isSymbolicJunctionOrHardLink)
                        string targetPath;
                        if (content is null || string.IsNullOrEmpty(targetPath = content.ToString()))
                            throw PSTraceSource.NewArgumentNullException(nameof(content), SessionStateStrings.NewLinkTargetNotSpecified, path);
                        content = targetPath;
                    NewItemPrivate(providerInstance, composedPath, type, content, context);
        /// The type of the item to create.
        /// The content of the item to create.
        private void NewItemPrivate(
                containerCmdletProvider.NewItem(path, type, content, context);
                    "NewItemProviderException",
                    SessionStateStrings.NewItemProviderException,
        /// <param name="newItemValue">
            object newItemValue,
                return NewItemDynamicParameters(providerInstance, providerPaths[0], type, newItemValue, newContext);
        /// The type of the new item.
        /// The value of the new item
        private object NewItemDynamicParameters(
                result = containerCmdletProvider.NewItemDynamicParameters(path, type, newItemValue, context);
                    "NewItemDynamicParametersProviderException",
                    SessionStateStrings.NewItemDynamicParametersProviderException,
        /// Determines if the item at the specified path has children.
        /// The path to the item to see if it has children.
        /// True if the item has children, false otherwise.
        internal bool HasChildItems(string path, bool force, bool literalPath)
            bool result = HasChildItems(path, context);
        internal bool HasChildItems(
                result = HasChildItems(providerInstance, providerPath, context);
        /// The provider to use.
            if (string.IsNullOrEmpty(providerId))
                throw PSTraceSource.NewArgumentException(nameof(providerId));
            result = HasChildItems(providerId, path, context);
            ContainerCmdletProvider providerInstance = GetContainerProviderInstance(providerId);
            return HasChildItems(providerInstance, path, context);
        private bool HasChildItems(
                result = containerCmdletProvider.HasChildItems(path, context);
                    "HasChildItemsProviderException",
                    SessionStateStrings.HasChildItemsProviderException,
        /// Copies an item at the specified path to an item at the <paramref name="copyPath"/>.
        /// The path(s) of the item(s) to copy.
        /// <param name="copyPath">
        /// The path of the item to copy to.
        /// Tells the provider to recurse sub-containers when copying.
        /// The objects that were copied.
        internal Collection<PSObject> CopyItem(string[] paths,
                                               string copyPath,
            copyPath ??= string.Empty;
            CopyItem(paths, copyPath, recurse, copyContainers, context);
        internal void CopyItem(
            // Get the provider specific path for the destination
            ProviderInfo destinationProvider = null;
            Microsoft.PowerShell.Commands.CopyItemDynamicParameters dynamicParams = context.DynamicParameters as Microsoft.PowerShell.Commands.CopyItemDynamicParameters;
            bool destinationIsRemote = false;
            bool sourceIsRemote = false;
            string providerDestinationPath;
            Runspaces.PSSession session = null;
            if (dynamicParams != null)
                if (dynamicParams.FromSession != null)
                    sourceIsRemote = true;
                    session = dynamicParams.FromSession;
                if (dynamicParams.ToSession != null)
                    destinationIsRemote = true;
                    session = dynamicParams.ToSession;
            if (sourceIsRemote && destinationIsRemote)
                context.WriteError(new ErrorRecord(
                               string.Format(System.Globalization.CultureInfo.InvariantCulture, SessionStateStrings.CopyItemFromSessionToSession, "FromSession", "ToSession")),
                               "InvalidInput",
                                dynamicParams));
            // Validate that the session is available and the language mode is not set to ConstrainedLanguage or NoLanguage.
            PSLanguageMode? remoteLanguageMode = null;
            if (sourceIsRemote || destinationIsRemote)
                if (!isValidSession(session, context, out remoteLanguageMode))
            if (!destinationIsRemote)
                providerDestinationPath =
                       copyPath,
                       out destinationProvider,
                // Validate remote destination path
                providerDestinationPath = copyPath;
                if (string.IsNullOrEmpty(providerDestinationPath))
                                            new ArgumentNullException(
                                                    SessionStateStrings.CopyItemRemotelyPathIsNullOrEmpty,
                                                    "Destination")),
                                            "CopyItemRemoteDestinationIsNullOrEmpty",
                                            providerDestinationPath));
                string root = ValidateRemotePathAndGetRoot(providerDestinationPath, session, context, remoteLanguageMode, false);
            s_tracer.WriteLine("providerDestinationPath = {0}", providerDestinationPath);
                if (sourceIsRemote)
                    // Get the root for the remote path
                    string root = ValidateRemotePathAndGetRoot(path, session, context, remoteLanguageMode, true);
                    providerInstance = ExecutionContext.SessionState.Internal.GetProviderInstance("FileSystem");
                    providerPaths = new Collection<string>();
                    providerPaths.Add(path);
                // Ensure that the source and destination are the same provider. Copying between
                // providers is not supported
                if (!sourceIsRemote && !destinationIsRemote && provider != destinationProvider)
                            "path",
                            SessionStateStrings.CopyItemSourceAndDestinationNotSameProvider);
                            "CopyItemSourceAndDestinationNotSameProvider",
                bool destinationIsContainer = false;
                    destinationIsContainer = IsItemContainer(
                        providerDestinationPath,
                    s_tracer.WriteLine("destinationIsContainer = {0}", destinationIsContainer);
                        CopyItem(
                    bool sourceIsContainer =
                         IsItemContainer(
                    s_tracer.WriteLine("sourceIsContainer = {0}", sourceIsContainer);
                    if (sourceIsContainer)
                        if (destinationIsContainer)
                            if (!recurse && copyContainers == CopyContainers.CopyChildrenOfTargetContainer)
                                // Copying a container to a container without the -container or -recurse
                                // switch is an error.
                                Exception e =
                                        SessionStateStrings.CopyContainerToContainerWithoutRecurseOrContainer);
                                    "CopyContainerToContainerWithoutRecurseOrContainer",
                                    providerPath));
                            else if (recurse && copyContainers == CopyContainers.CopyChildrenOfTargetContainer)
                                // Copy all the leaf items to a single container
                                CopyRecurseToSingleContainer(
                                // Call the provider to do a recurse copy of all the items
                            // Since we know the destination isn't a container, check to
                            // see if it exists.
                            if (ItemExists(providerInstance, providerDestinationPath, context))
                                // Since the item exists and is not a container it must
                                // be a leaf. Copying a container to a leaf is an error
                                        SessionStateStrings.CopyContainerItemToLeafError);
                                    "CopyContainerItemToLeafError",
                                // Copy the container to a non-existing path
                        // Copy a leaf to the destination
        /// The path to copy the item(s) to.
        /// If true all sub-containers and their children should be copied.
        private void CopyItem(
                containerCmdletProvider.CopyItem(path, copyPath, recurse, context);
                    "CopyItemProviderException",
                    SessionStateStrings.CopyItemProviderException,
        /// Recursively copies many items to a single container.
        private void CopyRecurseToSingleContainer(
                !string.IsNullOrEmpty(sourcePath),
                "The sourcePath should have been verified by the caller");
                !string.IsNullOrEmpty(destinationPath),
                "The destinationPath should have been verified by the caller");
            ContainerCmdletProvider containerProviderInstance =
            // Use GetChildNames to get the relative paths of all the children
            // to be copied
            Collection<string> children =
                    true, uint.MaxValue, false, false);
            foreach (string childName in children)
                // Now convert each relative path into a provider-internal path
                string childPath = MakePath(providerInstance.ProviderInfo, sourcePath, childName, context);
                // And then copy the item to the destination
                CopyItem(containerProviderInstance, childPath, destinationPath, false, context);
            string providerPath = null;
            bool pathNotFoundOnClient = false;
                    providerPath = providerPaths[0];
                // This exception is expected for remote sessions where drives exist in a remote session but not
                // on the client.
                pathNotFoundOnClient = true;
                // This exception is expected for remote sessions where item exist in a remote session but not
            if (pathNotFoundOnClient)
                // At this point, we don't know if this is a remote use of copy-item because the FromSession
                // and ToSession dynamic parameters have not been retrieved yet.
                // Ignore these exceptions and use the FileSystem provider as default.  If this is a real drive
                // issue, or if the item does not exist, it will be caught later when the drive or item path is used.
                var fileSystemProviders = Providers["FileSystem"];
                if (fileSystemProviders.Count > 0)
                    providerPath = path;
                    providerInstance = ExecutionContext.EngineSessionState.GetProviderInstance(
                        fileSystemProviders[0]);
                return CopyItemDynamicParameters(providerInstance, providerPath, destination, recurse, newContext);
        /// If true, subcontainers and their children should be copied.
        private object CopyItemDynamicParameters(
                result = containerCmdletProvider.CopyItemDynamicParameters(path, destination, recurse, context);
                    "CopyItemDynamicParametersProviderException",
                    SessionStateStrings.CopyItemDynamicParametersProviderException,
        // This function validates a remote path, and if it exists, it returns the root path.
        private static string ValidateRemotePathAndGetRoot(string path, Runspaces.PSSession session, CmdletProviderContext context, PSLanguageMode? languageMode, bool sourceIsRemote)
            Hashtable op = null;
                ps.Runspace = session.Runspace;
                // Check to see if the remote PSSession is running in constrained or no language mode and if so
                // then also if the path validation function already exists in the session (for the User drive
                // custom endpoint case).  Otherwise error out.
                if (languageMode.HasValue &&
                    (languageMode.Value == PSLanguageMode.ConstrainedLanguage || languageMode.Value == PSLanguageMode.NoLanguage))
                    ps.AddCommand("Get-Command").AddArgument(CopyFileRemoteUtils.PSCopyRemoteUtilsName);
                    var result = ps.Invoke<bool>();
                                    SessionStateStrings.CopyItemSessionProperties,
                                    "LanguageMode",
                                    session.Runspace.SessionStateProxy.LanguageMode)
                                ),
                                "SessionIsNotInFullLanguageMode",
                                session.Availability
                    ps.Streams.ClearStreams();
                    ps.AddCommand(CopyFileRemoteUtils.PSCopyRemoteUtilsName);
                    ps.AddScript(CopyFileRemoteUtils.PSValidatePathDefinition);
                ps.AddParameter("pathToValidate", path);
                    ps.AddParameter(nameof(sourceIsRemote), true);
                op = Microsoft.PowerShell.Commands.SafeInvokeCommand.Invoke(ps, null, context);
            if (op == null)
                                    System.Globalization.CultureInfo.InvariantCulture, SessionStateStrings.CopyItemValidateRemotePath, path)),
                                    "FailedToValidateRemotePath",
            // If the remote path is not absolute, display an error to the user.
            if (op["IsAbsolute"] != null)
                bool isAbsolute = (bool)op["IsAbsolute"];
                if (!isAbsolute)
                                            System.Globalization.CultureInfo.InvariantCulture, SessionStateStrings.CopyItemRemotelyPathIsNotAbsolute, path)),
                                            "RemotePathIsNotAbsolute",
            bool pathExist = false;
            string root = null;
            if (op["Exists"] != null)
                pathExist = (bool)op["Exists"];
            if (op["Root"] != null)
                root = (string)op["Root"];
            // Here there are two scenarios:
            // 1) If the source is remote and the path does not exist, error out.
            bool invalidRemoteSource = (sourceIsRemote && (!pathExist));
            // 2) For a remote destination, if the root does not exist, error out.
            bool invalidRemoteDestination = (root == null);
            if (invalidRemoteSource || invalidRemoteDestination)
                                                System.Globalization.CultureInfo.InvariantCulture, SessionStateStrings.PathNotFound, path)),
                                                "RemotePathNotFound",
            return root;
        private static bool isValidSession(PSSession session, CmdletProviderContext context, out PSLanguageMode? languageMode)
            // session == null is validated by the parameter binding
            if (session.Availability != RunspaceAvailability.Available)
                                        string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                            "Availability", session.Availability)),
                                            "SessionIsNotAvailable",
                                        session.Availability));
                languageMode = null;
            languageMode = session.Runspace.SessionStateProxy.LanguageMode;
        #endregion ContainerCmdletProvider accessors
    /// Defines the action to be taken for Navigation cmdlets.
    internal enum ProcessMode
        /// Write out the details.
        Enumerate = 1,
        /// Delete the item.
        Delete = 2

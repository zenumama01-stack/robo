    #region NavigationCmdletProvider
    /// The base class for a Cmdlet provider that expose a hierarchy of items and containers.
    /// The NavigationCmdletProvider class is a base class that provider can derive from
    /// to implement a set of methods that allow
    /// the recursive commands, nested containers, and relative paths.
    public abstract class NavigationCmdletProvider : ContainerCmdletProvider
        /// Internal wrapper for the MakePath protected method. It is called instead
        /// The parent segment of a path to be joined with the child.
        /// The child segment of a path to be joined with the parent.
        /// A string that represents the parent and child segments of the path
        /// joined by a path separator.
        /// This method should use lexical joining of two path segments with a path
        /// separator character. It should not validate the path as a legal fully
        /// qualified path in the provider namespace as each parameter could be only
        /// partial segments of a path and joined they may not generate a fully
        /// qualified path.
        /// Example: the file system provider may get "windows\system32" as the parent
        /// parameter and "foo.dll" as the child parameter. The method should join these
        /// with the "\" separator and return "windows\system32\foo.dll". Note that
        /// the returned path is not a fully qualified file system path.
        /// Also beware that the path segments may contain characters that are illegal
        /// in the provider namespace. These characters are most likely being used
        /// for globbing and should not be removed by the implementation of this method.
            return MakePath(parent, child);
        /// Internal wrapper for the GetParentPath protected method. It is called instead
        /// A fully qualified provider specific path to an item. The item may or
        /// may not exist.
        /// The fully qualified path to the root of a drive. This parameter may be null
        /// or empty if a mounted drive is not in use for this operation. If this parameter
        /// is not null or empty the result of the method should not be a path to a container
        /// that is a parent or in a different tree than the root.
        /// The path of the parent of the path parameter.
        /// This should be a lexical splitting of the path on the path separator character
        /// for the provider namespace. For example, the file system provider should look
        /// for the last "\" and return everything to the left of the "\".
            return GetParentPath(path, root);
        /// Internal wrapper for the NormalizeRelativePath method. It is called instead
            return NormalizeRelativePath(path, basePath);
        /// Internal wrapper for the GetChildName protected method. It is called instead
        /// This should be implemented as a split on the path separator. The characters
        /// in the fullPath may not be legal characters in the namespace but may be
        /// used in globing or regular expression matching. The provider should not error
        /// unless there are no path separators in the fully qualified path.
        /// Internal wrapper for the IsItemContainer protected method. It is called instead
        /// true if the item specified by path is a container, false otherwise.
            return IsItemContainer(path);
        /// Internal wrapper for the MoveItem protected method. It is called instead
        /// The path to the item to be moved.
        /// Nothing. All objects that are moved should be written to the WriteObject method.
            MoveItem(path, destination);
        /// the move-item cmdlet.
            return MoveItemDynamicParameters(path, destination);
        /// Joins two strings with a path a provider specific path separator.
        protected virtual string MakePath(string parent, string child)
            return MakePath(parent, child, childIsLeaf: false);
        /// <param name="childIsLeaf">
        /// Indicate that the <paramref name="child"/> is the name of a child item that's guaranteed to exist
        /// If the <paramref name="childIsLeaf"/> is True, then we don't normalize the child path, and would do
        /// some checks to decide whether to normalize the parent path.
        /// <returns>New path string.</returns>
        protected string MakePath(string parent, string child, bool childIsLeaf)
                if (string.IsNullOrEmpty(parent) &&
                    string.IsNullOrEmpty(child))
                else if (string.IsNullOrEmpty(parent) &&
                         !string.IsNullOrEmpty(child))
                    result = NormalizePath(child);
                else if (!string.IsNullOrEmpty(parent) &&
                         (string.IsNullOrEmpty(child) ||
                          child.Equals(StringLiterals.DefaultPathSeparatorString, StringComparison.Ordinal) ||
                          child.Equals(StringLiterals.AlternatePathSeparatorString, StringComparison.Ordinal)))
                    if (parent.EndsWith(StringLiterals.DefaultPathSeparator))
                        result = parent;
                        result = parent + StringLiterals.DefaultPathSeparator;
                    // Both parts are not empty so join them
                    // 'childIsLeaf == true' indicates that 'child' is actually the name of a child item and
                    // guaranteed to exist. In this case, we don't normalize the child path.
                    if (childIsLeaf)
                        parent = NormalizePath(parent);
                        // Normalize the path so that only the default path separator is used as a
                        // separator even if the user types the alternate slash.
                        child = NormalizePath(child);
                    ReadOnlySpan<char> appendChild = child.AsSpan();
                    if (child.StartsWith(StringLiterals.DefaultPathSeparator))
                        appendChild = appendChild.Slice(1);
                    result = IO.Path.Join(parent.AsSpan(), appendChild);
        /// Removes the child segment of a path and returns the remaining parent
        /// portion.
        protected virtual string GetParentPath(string path, string root)
                string rootPath = string.Empty;
                if (root != null)
                    rootPath = NormalizePath(root);
                // Check to see if the path is equal to the root
                // of the virtual drive
                    rootPath,
                    parentPath = string.Empty;
                    int lastIndex = path.LastIndexOf(StringLiterals.DefaultPathSeparator);
                    if (lastIndex != -1)
                        if (lastIndex == 0)
                            ++lastIndex;
                        // Get the parent directory
                        parentPath = path.Substring(0, lastIndex);
        protected virtual string NormalizeRelativePath(
                return ContractRelativePath(path, basePath, false, Context);
        internal string ContractRelativePath(
            bool allowNonExistingPaths,
            providerBaseTracer.WriteLine("basePath = {0}", basePath);
            bool originalPathHadTrailingSlash = false;
            string normalizedPath = path;
            string normalizedBasePath = basePath;
            // NTRAID#Windows 7-697922-2009/06/29-leeholm
            // WORKAROUND WORKAROUND WORKAROUND WORKAROUND WORKAROUND WORKAROUND WORKAROUND WORKAROUND WORKAROUND
            // This path normalization got moved here from the MakePath override in V2 to prevent
            // over-normalization of paths. This was a net-improvement for providers that use the default
            // implementations, but now incorrectly replaces forward slashes with back slashes during the call to
            // GetParentPath and GetChildName. This breaks providers that are sensitive to slash direction, the only
            // one we are aware of being the Active Directory provider. This change prevents this over-normalization
            // from being done on AD paths.
            // For more information, see Win7:695292. Do not change this code without closely working with the
            // Active Directory team.
            if (!string.Equals(context.ProviderInstance.ProviderInfo.FullName,
                @"Microsoft.ActiveDirectory.Management\ActiveDirectory", StringComparison.OrdinalIgnoreCase))
                normalizedPath = NormalizePath(path);
                normalizedBasePath = NormalizePath(basePath);
                if (path.EndsWith(StringLiterals.DefaultPathSeparator))
                    originalPathHadTrailingSlash = true;
                if (string.Equals(normalizedPath, normalizedBasePath, StringComparison.OrdinalIgnoreCase) &&
                if (!normalizedPath.StartsWith(normalizedBasePath, StringComparison.OrdinalIgnoreCase) &&
                    (basePath.Length > 0))
                    string commonBase = GetCommonBase(normalizedPath, normalizedBasePath);
                    Stack<string> parentNavigationStack = TokenizePathToStack(normalizedBasePath, commonBase);
                        if (string.Equals(normalizedPath, commonBase, StringComparison.OrdinalIgnoreCase) &&
                            (!normalizedPath.EndsWith(StringLiterals.DefaultPathSeparator)))
                            string[] childNavigationItems = TokenizePathToStack(normalizedPath, commonBase).ToArray();
                        normalizedPathStack = NormalizeThePath(tokenizedPathStack, path, basePath, allowNonExistingPaths);
                        WriteError(new ErrorRecord(argumentException, argumentException.GetType().FullName, ErrorCategory.InvalidArgument, null));
            if (originalPathHadTrailingSlash)
                result += StringLiterals.DefaultPathSeparator;
        protected virtual string GetChildName(string path)
                // If the full path existed, we must semantically evaluate the parent path
                else if (ItemExists(path, Context))
                    string parentPath = GetParentPath(path, null);
                    // No parent, return the entire path
                    if (string.IsNullOrEmpty(parentPath))
                    // If the parent path ends with the path separator, we can't split
                    // the path based on that
                    else if (parentPath.IndexOf(StringLiterals.DefaultPathSeparator) == (parentPath.Length - 1))
                        separatorIndex = path.IndexOf(parentPath, StringComparison.OrdinalIgnoreCase) + parentPath.Length;
                        result = path.Substring(separatorIndex);
                // Otherwise, use lexical parsing
        /// Determines if the item specified by the path is a container.
        /// Providers override this method to give the user the ability to check
        /// to see if a provider object is a container using the test-path -container cmdlet.
        protected virtual bool IsItemContainer(string path)
        /// Nothing is returned, but all the objects that were moved should be written to the WriteItemObject method.
        /// Providers override this method to give the user the ability to move provider objects using
        /// of ExpandWildcards, Filter, Include, or Exclude should ensure that the path and items being moved
        /// By default overrides of this method should not move objects over existing items unless the Force
        /// property is set to true. For instance, the FileSystem provider should not move c:\temp\foo.txt over
        /// If <paramref name="destination"/> exists and is a container then Force isn't required and <paramref name="path"/>
        /// should be moved into the <paramref name="destination"/> container as a child.
        protected virtual void MoveItem(
        protected virtual object MoveItemDynamicParameters(
        /// When a path contains both forward slash and backslash, we may introduce some errors by
        /// normalizing the path. This method does some smart checks to reduce the chances of making
        /// those errors.
        /// Normalized path or the original path.
        private string NormalizePath(string path)
            // If we have a mix of slashes, then we may introduce an error by normalizing the path.
            // For example: path HKCU:\Test\/ is pointing to a subkey '/' of 'HKCU:\Test', if we
            // normalize it, then we will get a wrong path.
            // Fast return if nothing to normalize.
            if (!path.Contains(StringLiterals.AlternatePathSeparator))
            bool pathHasBackSlash = path.Contains(StringLiterals.DefaultPathSeparator);
            string normalizedPath;
            // There is a mix of slashes & the path is rooted & the path exists without normalization.
            // In this case, we might want to skip the normalization to the path.
            if (pathHasBackSlash && IsAbsolutePath(path) && ItemExists(path))
                // 1. The path exists and ends with a forward slash, in this case, it's very possible the ending forward slash
                //    make sense to the underlying provider, so we skip normalization
                // 2. The path exists, but not anymore after normalization, then we skip normalization
                if (path.EndsWith(StringLiterals.AlternatePathSeparator))
                normalizedPath = path.Replace(StringLiterals.AlternatePathSeparator, StringLiterals.DefaultPathSeparator);
                if (!ItemExists(normalizedPath))
                    return normalizedPath;
        /// Test if the path is an absolute path.
        private bool IsAbsolutePath(string path)
            if (LocationGlobber.IsAbsolutePath(path))
            else if (this.PSDriveInfo != null && !string.IsNullOrEmpty(this.PSDriveInfo.Root) &&
                     path.StartsWith(this.PSDriveInfo.Root, StringComparison.OrdinalIgnoreCase))
                providerBaseTracer.WriteLine("tokenizedPathStack.Push({0})", childName);
                if (tempPath.Length >= previousParent.Length)
        /// The path being normalized. Just used for error reporting.
        /// The base path to make the path relative to. Just used for error reporting.
        /// <param name="allowNonExistingPaths">
        /// Determines whether to throw an exception on non-existing paths.
        private static Stack<string> NormalizeThePath(
            Stack<string> tokenizedPathStack, string path,
            string basePath, bool allowNonExistingPaths)
                providerBaseTracer.WriteLine("childName = {0}", childName);
                // Make sure we don't have
                if (childName.Equals("..", StringComparison.OrdinalIgnoreCase))
                        providerBaseTracer.WriteLine("normalizedPathStack.Pop() : {0}", poppedName);
                        if (!allowNonExistingPaths)
                            PSArgumentException e =
                                (PSArgumentException)PSTraceSource.NewArgumentException(
                                    SessionStateStrings.NormalizeRelativePathOutsideBase,
                                    basePath);
                providerBaseTracer.WriteLine("normalizedPathStack.Push({0})", childName);
    #endregion NavigationCmdletProvider

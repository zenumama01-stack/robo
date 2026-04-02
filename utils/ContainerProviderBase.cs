namespace System.Management.Automation.Provider
    /// The base class for Cmdlet providers that expose a single level of items.
    /// The ContainerCmdletProvider class is base class that a provider derives from
    /// to implement methods that allow
    /// the use of a set of core commands against the objects that the provider
    /// gives access to. By deriving from this class users can take advantage of
    /// all the features of the <see cref="ItemCmdletProvider"/> as well as
    /// globbing and the following commands when targeting this provider:
    ///     get-childitem
    ///     rename-item
    ///     new-item
    ///     remove-item
    ///     set-location
    ///     push-location
    ///     pop-location
    ///     get-location -stack
    public abstract class ContainerCmdletProvider : ItemCmdletProvider
        /// Internal wrapper for the GetChildItems protected method. It is called instead
        /// of the protected method that is overridden by derived classes so that the
        /// context of the command can be set.
        /// The path (or name in a flat namespace) to the item from which to retrieve the children.
        /// True if all children in a subtree should be retrieved, false if only a single
        /// level of children should be retrieved. This parameter should only be true for
        /// the NavigationCmdletProvider derived class.
        /// Nothing is returned, but all children should be written to the Write*Object or
        /// Write*Objects method.
            // Call virtual method
            GetChildItems(path, recurse, depth);
        /// Gives the provider to attach additional parameters to
        /// the get-childitem cmdlet.
        /// to the item to get the dynamic parameters for.
        /// Overrides of this method should return an object that has properties and fields decorated with
        /// parsing attributes similar to a cmdlet class or a
        /// <see cref="System.Management.Automation.RuntimeDefinedParameterDictionary"/>.
        /// The default implementation returns null. (no additional parameters)
            return GetChildItemsDynamicParameters(path, recurse);
        /// Internal wrapper for the GetChildNames protected method. It is called instead
        /// Nothing is returned, but all names should be written to the Write*Object or
            GetChildNames(path, returnContainers);
        /// Gets a new provider-specific path and filter (if any) that corresponds to the given
        /// path.
        /// The path to the item. Unlike most other provider APIs, this path is likely to
        /// contain PowerShell wildcards.
        /// The provider-specific filter currently applied.
        /// <param name="updatedPath">
        /// The new path to the item.
        /// <param name="updatedFilter">
        /// The new filter.
        /// True if the path or filter were altered. False otherwise.
        /// Providers override this method if they support a native filtering syntax that
        /// can offer performance improvements over wildcard matching done by the PowerShell
        /// If the provider can handle a portion (or all) of the PowerShell wildcard with
        /// semantics equivalent to the PowerShell wildcard, it may adjust the path to exclude
        /// the PowerShell wildcard.
        /// If the provider can augment the PowerShell wildcard with an approximate filter (but
        /// not replace it entirely,) it may simply return a filter without modifying the path.
        /// In this situation, PowerShell's wildcarding will still be applied to a smaller result
        /// set, resulting in improved performance.
        /// The default implementation of this method leaves both Path and Filter unmodified.
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#")]
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "3#")]
        internal virtual bool ConvertPath(
            string filter,
            ref string updatedPath,
            ref string updatedFilter,
            return ConvertPath(path, filter, ref updatedPath, ref updatedFilter);
        /// the get-childitem -name cmdlet.
            return GetChildNamesDynamicParameters(path);
        /// Internal wrapper for the RenameItem protected method. It is called instead
        /// Nothing is returned, but all renamed items should be written to the Write*Object or
        /// Write*Objects.
            RenameItem(path, newName);
        /// the rename-item cmdlet.
            return RenameItemDynamicParameters(path, newName);
        /// Internal wrapper for the New protected method. It is called instead
        /// The path to the item to create.
        /// The provider defined type of the item to create.
        /// This is a provider specific type that the provider can use to create a new
        /// instance of an item at the specified path.
        /// Nothing is returned, but all new items should be written to the Write*Object or
            NewItem(path, type, newItemValue);
        /// the new-item cmdlet.
            return NewItemDynamicParameters(path, type, newItemValue);
        /// Internal wrapper for the Remove protected method. It is called instead
        /// The path to the item to remove.
        /// True if all children in a subtree should be removed, false if only a single
        /// level of children should be removed. This parameter should only be true for
        /// NavigationCmdletProvider and its derived classes.
            RemoveItem(path, recurse);
        /// the remove-item cmdlet.
            return RemoveItemDynamicParameters(path, recurse);
        /// Internal wrapper for the HasChildItems protected method. It is called instead
        /// For implementers of ContainerCmdletProvider classes and those derived from it,
        /// if a null or empty path is passed,
        /// the provider should consider any items in the data store to be children
        /// and return true.
        internal bool HasChildItems(string path, CmdletProviderContext context)
            return HasChildItems(path);
        /// Internal wrapper for the Copy protected method. It is called instead
        /// The path of the item to copy.
        /// Nothing. All objects that are copied should be written to the Write*Object or
        /// Write*Objects methods.
            CopyItem(path, copyPath, recurse);
        /// the copy-item cmdlet.
            return CopyItemDynamicParameters(path, destination, recurse);
        #region Protected methods
        /// Gets the children of the item at the specified path.
        /// Nothing is returned, but all objects should be written to the WriteItemObject method.
        /// Providers override this method to give the user access to the provider objects using
        /// the get-childitem cmdlets.
        /// Providers that declare <see cref="System.Management.Automation.Provider.ProviderCapabilities"/>
        /// of ExpandWildcards, Filter, Include, or Exclude should ensure that the path passed meets those
        /// requirements by accessing the appropriate property from the base class.
        /// By default overrides of this method should not write objects that are generally hidden from
        /// the user unless the Force property is set to true. For instance, the FileSystem provider should
        /// not call WriteItemObject for hidden or system files unless the Force property is set to true.
        /// The provider implementation is responsible for preventing infinite recursion when there are
        /// circular links and the like. An appropriate terminating exception should be thrown if this
        /// situation occurs.
        /// The default implementation of this method throws an <see cref="System.Management.Automation.PSNotSupportedException"/>.
        protected virtual void GetChildItems(
                        SessionStateStrings.CmdletProvider_NotSupported);
                if (depth == uint.MaxValue)
                    this.GetChildItems(path, recurse);
                            SessionStateStrings.CmdletProvider_NotSupportedRecursionDepth);
        /// Gives the provider an opportunity to attach additional parameters to
        protected virtual object GetChildItemsDynamicParameters(string path, bool recurse)
        /// the get-childitem  -name cmdlet.
        /// requirements by accessing the appropriate property from the base class. The exception to this
        /// is if <paramref name="returnAllContainers"/> is true, then any child name for a container should
        /// be returned even if it doesn't match the Filter, Include, or Exclude.
        /// By default overrides of this method should not write the names of objects that are generally hidden from
        protected virtual void GetChildNames(
        /// PowerShell wildcarding semantics are handled by the System.Management.Automation.Wildcardpattern
        protected virtual bool ConvertPath(
            ref string updatedFilter)
        protected virtual object GetChildNamesDynamicParameters(string path)
        /// Nothing is returned, but the renamed items should be written to the WriteItemObject method.
        /// Providers override this method to give the user the ability to rename provider objects using
        /// By default overrides of this method should not allow renaming objects that are generally hidden from
        /// not allow renaming of a hidden or system file unless the Force property is set to true.
        /// This method is intended for the modification of the item's name only and not for Move operations.
        /// An error should be written to <see cref="CmdletProvider.WriteError"/> if the <paramref name="newName"/>
        /// parameter contains path separators or would cause the item to change its parent location.
        protected virtual void RenameItem(
            string newName)
        protected virtual object RenameItemDynamicParameters(string path, string newName)
        /// The provider defined type for the object to create.
        /// Providers override this method to give the user the ability to create new provider objects using
        /// The <paramref name="itemTypeName"/> parameter is a provider specific string that the user specifies to tell
        /// the provider what type of object to create.  For instance, in the FileSystem provider the <paramref name="type"/>
        /// parameter can take a value of "file" or "directory". The comparison of this string should be
        /// case-insensitive and you should also allow for least ambiguous matches. So if the provider allows
        /// for the types "file" and "directory", only the first letter is required to disambiguate.
        /// If <paramref name="itemTypeName"/> refers to a type the provider cannot create, the provider should produce
        /// an <see cref="ArgumentException"/> with a message indicating the types the provider can create.
        /// The <paramref name="newItemValue"/> parameter can be any type of object that the provider can use
        /// to create the item. It is recommended that the provider accept at a minimum strings, and an instance
        /// of the type of object that would be returned from GetItem() for this path. <see cref="LanguagePrimitives.ConvertTo(object, System.Type)"/>
        /// can be used to convert some types to the desired type.
        protected virtual void NewItem(
            object newItemValue)
        protected virtual object NewItemDynamicParameters(
        /// Removes (deletes) the item at the specified path.
        /// Nothing should be returned or written from this method.
        /// Providers override this method to allow the user the ability to remove provider objects using
        /// By default overrides of this method should not remove objects that are generally hidden from
        /// not remove a hidden or system file unless the Force property is set to true.
        protected virtual void RemoveItem(
        protected virtual object RemoveItemDynamicParameters(
        /// Providers override this method to give the provider infrastructure the ability to determine
        /// if a particular provider object has children without having to retrieve all the child items.
        /// For implementers of <see cref="ContainerCmdletProvider"/> classes and those derived from it,
        protected virtual bool HasChildItems(string path)
        /// Nothing is returned, but all the objects that were copied should be written to the WriteItemObject method.
        /// Providers override this method to give the user the ability to copy provider objects using
        /// of ExpandWildcards, Filter, Include, or Exclude should ensure that the path and items being copied
        /// meets those requirements by accessing the appropriate property from the base class.
        /// By default overrides of this method should not copy objects over existing items unless the Force
        /// property is set to true. For instance, the FileSystem provider should not copy c:\temp\foo.txt over
        /// c:\bar.txt if c:\bar.txt already exists unless the Force parameter is true.
        /// If <paramref name="copyPath"/> exists and is a container then Force isn't required and <paramref name="path"/>
        /// should be copied into the <paramref name="copyPath"/> container as a child.
        /// If <paramref name="recurse"/> is true, the provider implementation is responsible for
        /// preventing infinite recursion when there are circular links and the like. An appropriate
        /// terminating exception should be thrown if this situation occurs.
        protected virtual void CopyItem(
        protected virtual object CopyItemDynamicParameters(
    #endregion ContainerCmdletProvider

    /// Exposes the Item noun of the Cmdlet Providers to the Cmdlet base class. The methods of this class
    public sealed class ItemCmdletProviderIntrinsics
        private ItemCmdletProviderIntrinsics()
        internal ItemCmdletProviderIntrinsics(Cmdlet cmdlet)
        /// An instance of the "real" session state class.
        internal ItemCmdletProviderIntrinsics(SessionStateInternal sessionState)
        #region GetItem
        /// Gets the item at the specified path.
        /// The object(s) at the specified path.
        public Collection<PSObject> Get(string path)
            return _sessionState.GetItem(new string[] { path }, false, false);
        /// The path(s) to the item(s) to retrieve. They may be a drive or provider-qualified path(s) and may include
        public Collection<PSObject> Get(string[] path, bool force, bool literalPath)
            return _sessionState.GetItem(path, force, literalPath);
        /// Nothing. The object(s) at the specified path are written to the context.
        internal void Get(string path, CmdletProviderContext context)
            _sessionState.GetItem(new string[] { path }, context);
        internal object GetItemDynamicParameters(string path, CmdletProviderContext context)
            return _sessionState.GetItemDynamicParameters(path, context);
        #endregion GetItem
        #region SetItem
        /// Sets the item at the specified path.
        /// The path to the item to set. It may be a drive or provider-qualified path and may include
        /// The new value to set the item to.
        /// The object(s) set at the specified path.
        public Collection<PSObject> Set(string path, object value)
            return _sessionState.SetItem(new string[] { path }, value, false, false);
        /// The path(s) to the item(s) to set. They may be drive or provider-qualified paths and may include
        public Collection<PSObject> Set(string[] path, object value, bool force, bool literalPath)
            return _sessionState.SetItem(path, value, force, literalPath);
        /// Nothing. The object(s) set at the specified path are written to the context.
        internal void Set(string path, object value, CmdletProviderContext context)
            _sessionState.SetItem(new string[] { path }, value, context);
        /// The new value of the item at the specified path.
        internal object SetItemDynamicParameters(
            return _sessionState.SetItemDynamicParameters(path, value, context);
        #endregion SetItem
        #region ClearItem
        /// Clears the item at the specified path.
        /// The path to the item to clear. It may be a drive or provider-qualified path and may include
        /// The object(s) cleared at the specified path.
        public Collection<PSObject> Clear(string path)
            return _sessionState.ClearItem(new string[] { path }, false, false);
        /// The path(s) to the item to clear. It may be a drive or provider-qualified path and may include
        public Collection<PSObject> Clear(string[] path, bool force, bool literalPath)
            return _sessionState.ClearItem(path, force, literalPath);
        /// The path to the item to be cleared. It may be a drive or provider-qualified path and may include
        /// Nothing. The object(s) cleared at the specified path are written to the context.
            _sessionState.ClearItem(new string[] { path }, context);
        internal object ClearItemDynamicParameters(string path, CmdletProviderContext context)
            return _sessionState.ClearItemDynamicParameters(path, context);
        #endregion ClearItem
        #region InvokeDefaultAction
        /// Invokes the default action of the item at the specified path.
        /// The path to the item to invoke. It may be a drive or provider-qualified path and may include
        public void Invoke(string path)
            _sessionState.InvokeDefaultAction(new string[] { path }, false);
        /// Invokes the default action of the item(s) at the specified path(s).
        /// The path(s) to the item(s) to invoke. They may be drive or provider-qualified paths and may include
        public void Invoke(string[] path, bool literalPath)
            _sessionState.InvokeDefaultAction(path, literalPath);
        /// Invokes the default action for the item at the specified path.
        /// The path to the item to be invoked. It may be a drive or provider-qualified path and may include
        internal void Invoke(string path, CmdletProviderContext context)
            _sessionState.InvokeDefaultAction(new string[] { path }, context);
        internal object InvokeItemDynamicParameters(string path, CmdletProviderContext context)
            return _sessionState.InvokeDefaultActionDynamicParameters(path, context);
        #endregion InvokeDefaultAction
        #region RenameItem
        /// Renames the item at the given path.
        /// The path to the item to rename. It may be a drive or provider-qualified path and may include
        /// The new name of the item.
        /// The item(s) that were renamed.
        public Collection<PSObject> Rename(string path, string newName)
            return _sessionState.RenameItem(path, newName, false);
        public Collection<PSObject> Rename(string path, string newName, bool force)
            return _sessionState.RenameItem(path, newName, force);
        /// Nothing.  The item(s) that get renamed are written to the context.
        internal void Rename(
            string newName,
            _sessionState.RenameItem(path, newName, context);
        internal object RenameItemDynamicParameters(
            return _sessionState.RenameItemDynamicParameters(path, newName, context);
        #endregion RenameItem
        #region NewItem
        /// Creates a new item at the given path.
        /// The path to the container to create item in. It may be a drive or provider-qualified path and may include
        /// The name of the new item to create.
        /// <param name="itemTypeName">
        /// The type of the new item to create.
        /// The content of the new item to create.
        /// The item that was created.
        public Collection<PSObject> New(
            string itemTypeName,
            object content)
            return _sessionState.NewItem(new string[] { path }, name, itemTypeName, content, false);
        /// The path(s) to the container to create item in. They may be drive or provider-qualified path and may include
        /// The item(s) that was created.
            object content,
            bool force)
            return _sessionState.NewItem(path, name, itemTypeName, content, force);
        /// Nothing.  The new item is written to the context.
            _sessionState.NewItem(new string[] { path }, name, type, content, context);
        internal object NewItemDynamicParameters(
            return _sessionState.NewItemDynamicParameters(path, type, content, context);
        #endregion NewItem
        #region RemoveItem
        /// Removes the items at the given path.
        /// The path to the item to remove. It may be a drive or provider-qualified path and may include
        /// If true, removes all the children in all the sub-containers of the specified
        /// container. If false, only removes the immediate children of the specified
        public void Remove(string path, bool recurse)
            _sessionState.RemoveItem(new string[] { path }, recurse, false, false);
        /// The path(s) to the item(s) to remove. They may be drive or provider-qualified paths and may include
        public void Remove(string[] path, bool recurse, bool force, bool literalPath)
            _sessionState.RemoveItem(path, recurse, force, literalPath);
            _sessionState.RemoveItem(new string[] { path }, recurse, context);
        internal object RemoveItemDynamicParameters(
            return _sessionState.RemoveItemDynamicParameters(path, recurse, context);
        #endregion RemoveItem
        #region CopyItem
        /// Copy item at the specified path.
        /// The path to the item to copy. It may be a drive or provider-qualified path and may include
        /// <param name="destinationPath">
        /// The path to copy the item to.
        /// If true, copies all the children in all the sub-containers of the specified
        /// container. If false, only copies the specified item.
        /// <param name="copyContainers">
        /// Determines how the source container is used in the copy operation.
        /// The item(s) that were copied.
        public Collection<PSObject> Copy(
            string destinationPath,
            CopyContainers copyContainers)
            return _sessionState.CopyItem(new string[] { path }, destinationPath, recurse, copyContainers, false, false);
        /// The path(s) to the item(s) to copy. They may be a drive or provider-qualified path and may include
            CopyContainers copyContainers,
            return _sessionState.CopyItem(path, destinationPath, recurse, copyContainers, force, literalPath);
        /// Nothing. The item(s) that were copied are written to the context.
        internal void Copy(
            _sessionState.CopyItem(new string[] { path }, destinationPath, recurse, copyContainers, context);
        internal object CopyItemDynamicParameters(
            return _sessionState.CopyItemDynamicParameters(path, destination, recurse, context);
        #endregion CopyItem
        #region MoveItem
        /// Moves the item at the specified path to the specified destination.
        /// The path to the item to move.
        /// The path to the location that the item will be moved.
        /// The item(s) that were moved.
        /// If <paramref name="destination"/> resolves to multiple paths.
        /// If <paramref name="destination"/> and <paramref name="path"/> don't resolve
        /// to the same provider.
        /// If <paramref name="path"/> resolves to multiple paths and <paramref name="destination"/>
        /// is not a container.
        public Collection<PSObject> Move(string path, string destination)
            return _sessionState.MoveItem(new string[] { path }, destination, false, false);
        /// The path(s) to the item to move.
        public Collection<PSObject> Move(string[] path, string destination, bool force, bool literalPath)
            return _sessionState.MoveItem(path, destination, force, literalPath);
        /// Nothing. The object that is moved is written to the context.
        internal void Move(
            _sessionState.MoveItem(new string[] { path }, destination, context);
        /// The path to move the item to.
        internal object MoveItemDynamicParameters(
            return _sessionState.MoveItemDynamicParameters(path, destination, context);
        #endregion MoveItem
        #region Exists
        /// Determines if an item at the given path exits.
        /// The path to the item to determine if it exists. It may be a drive or provider-qualified path and may include
        /// True if the item at the specified path exists. False otherwise.
        public bool Exists(string path)
            return _sessionState.ItemExists(path, false, false);
        public bool Exists(string path, bool force, bool literalPath)
            return _sessionState.ItemExists(path, force, literalPath);
        internal bool Exists(
            return _sessionState.ItemExists(path, context);
        /// Gets the dynamic parameters for the test-path cmdlet.
        internal object ItemExistsDynamicParameters(
            return _sessionState.ItemExistsDynamicParameters(path, context);
        #endregion ItemExists
        #region IsContainer
        /// Determines if the specified path is to an item that is a container.
        /// The path to the item to determine if it is a container.
        /// True if the path is to an item that is a container. False otherwise.
        public bool IsContainer(string path)
            return _sessionState.IsItemContainer(path);
        internal bool IsContainer(
            return _sessionState.IsItemContainer(path, context);
        #endregion IsItemContainer
    /// Determines how the source container of a copy operation
    /// will be used.
    public enum CopyContainers
        /// The source container is copied.
        CopyTargetContainer,
        /// The children of the source container are copied.
        CopyChildrenOfTargetContainer

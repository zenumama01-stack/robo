    /// Exposes the Property noun of the Cmdlet Providers to the Cmdlet base class. The methods of this class
    public sealed class PropertyCmdletProviderIntrinsics
        private PropertyCmdletProviderIntrinsics()
        internal PropertyCmdletProviderIntrinsics(Cmdlet cmdlet)
        internal PropertyCmdletProviderIntrinsics(SessionStateInternal sessionState)
        #region GetProperty
        /// Gets the specified properties from the specified item(s)
        /// The path to the item to get the properties from.
        /// <param name="providerSpecificPickList">
        /// The properties to get from the item(s). If this is empty, null, or "*" all
        /// properties should be returned.
        /// A PSObject for each item that the path represents. Each PSObject should
        /// contain a property for those in the providerSpecificPickList.
        public Collection<PSObject> Get(
            Collection<string> providerSpecificPickList)
            return _sessionState.GetProperty(new string[] { path }, providerSpecificPickList, false);
        /// The path(s) to the item(s) to get the properties from.
            Collection<string> providerSpecificPickList,
            return _sessionState.GetProperty(path, providerSpecificPickList, literalPath);
        /// Nothing. A PSObject for each item that the path represents is written
        /// to the context. Each PSObject should
            _sessionState.GetProperty(new string[] { path }, providerSpecificPickList, context);
        /// Gets the dynamic parameters for the get-itemproperty cmdlet.
        internal object GetPropertyDynamicParameters(
            return _sessionState.GetPropertyDynamicParameters(path, providerSpecificPickList, context);
        #endregion GetProperty
        #region SetProperty
        /// Sets the specified properties on the specified item(s)
        /// The path to the item to set the properties on.
        /// <param name="propertyValue">
        /// The properties that are to be set on the item
        /// A PSObject for each item that had the property set on it.
        /// If <paramref name="path"/> or <paramref name="property"/> is null.
        public Collection<PSObject> Set(
            PSObject propertyValue)
            return _sessionState.SetProperty(new string[] { path }, propertyValue, false, false);
        /// The path(s) to the item(s) to set the properties on.
            PSObject propertyValue,
            return _sessionState.SetProperty(path, propertyValue, force, literalPath);
        /// Nothing. A PSObject for the property that was set is written to the context.
        internal void Set(
            _sessionState.SetProperty(new string[] { path }, propertyValue, context);
        /// Gets the dynamic parameters for the set-itemproperty cmdlet.
        internal object SetPropertyDynamicParameters(
            return _sessionState.SetPropertyDynamicParameters(path, propertyValue, context);
        #endregion SetProperty
        #region ClearProperty
        /// Clear the specified properties from the specified item(s)
        /// The path to the item to clear the properties from.
        /// <param name="propertyToClear">
        /// The properties to clear from the item(s).
        public void Clear(
            Collection<string> propertyToClear)
            _sessionState.ClearProperty(new string[] { path }, propertyToClear, false, false);
        /// The path(s) to the item(s) to clear the properties from.
            Collection<string> propertyToClear,
            _sessionState.ClearProperty(path, propertyToClear, force, literalPath);
        /// Clears the specified properties from the specified item(s)
        internal void Clear(
            _sessionState.ClearProperty(new string[] { path }, propertyToClear, context);
        /// Gets the dynamic parameters for the clear-itemproperty cmdlet.
        internal object ClearPropertyDynamicParameters(
            return _sessionState.ClearPropertyDynamicParameters(path, propertyToClear, context);
        #endregion ClearProperty
        #region NewProperty
        /// Creates a new property on the specified item.
        /// The path to the item on which the new property should be created.
        /// The name of the property that should be created.
        /// <param name="propertyTypeName">
        /// The type of the property that should be created.
        /// The new value of the property that should be created.
        /// A PSObject for each item that the property was created on. The PSObject
        /// contains the properties that were created.
            string propertyTypeName,
            return _sessionState.NewProperty(new string[] { path }, propertyName, propertyTypeName, value, false, false);
        /// The path(s) to the item(s0 on which the new property should be created.
            return _sessionState.NewProperty(path, propertyName, propertyTypeName, value, force, literalPath);
        /// Nothing. A PSObject for each item that the property was created on
        /// is written to the context. Each PSObject
            _sessionState.NewProperty(new string[] { path }, propertyName, type, value, context);
        /// Gets the dynamic parameters for the new-itemproperty cmdlet.
        internal object NewPropertyDynamicParameters(
            return _sessionState.NewPropertyDynamicParameters(path, propertyName, type, value, context);
        #endregion NewProperty
        #region RemoveProperty
        /// Removes a property from the specified item(s)
        /// The path to the item(s) on which the property should be removed.
        /// The property name that should be removed.
        public void Remove(string path, string propertyName)
            _sessionState.RemoveProperty(new string[] { path }, propertyName, false, false);
        /// The path(s) to the item(s) on which the property should be removed.
        public void Remove(string[] path, string propertyName, bool force, bool literalPath)
            _sessionState.RemoveProperty(path, propertyName, force, literalPath);
            _sessionState.RemoveProperty(new string[] { path }, propertyName, context);
        /// Gets the dynamic parameters for the remove-itemproperty cmdlet.
        /// The name of the property that should be removed.
        internal object RemovePropertyDynamicParameters(
            return _sessionState.RemovePropertyDynamicParameters(path, propertyName, context);
        #endregion RemoveProperty
        #region RenameProperty
        /// Renames a property on the specified item(s)
        /// The path to the item(s) on which the property should be renamed.
        /// <param name="sourceProperty">
        /// The source name of the property to be renamed.
        /// <param name="destinationProperty">
        /// The new name of the property.
        /// A PSObject for each item that is the new property after the rename.
        /// If <paramref name="path"/>, <paramref name="sourceProperty"/>,
        /// or <paramref name="destinationProperty"/> is null.
        public Collection<PSObject> Rename(
            string sourceProperty,
            string destinationProperty)
            return _sessionState.RenameProperty(new string[] { path }, sourceProperty, destinationProperty, false, false);
        /// The path(s) to the item(s) on which the property should be renamed.
            string destinationProperty,
            return _sessionState.RenameProperty(path, sourceProperty, destinationProperty, force, literalPath);
        /// Nothing. A PSObject for each item that the property is renamed on is
        /// written to the context. The Shellobject contains the new property after the rename.
            _sessionState.RenameProperty(new string[] { path }, sourceProperty, destinationProperty, context);
        /// Gets the dynamic parameters for the rename-itemproperty cmdlet.
        internal object RenamePropertyDynamicParameters(
            return _sessionState.RenamePropertyDynamicParameters(path, sourceProperty, destinationProperty, context);
        #endregion RenameProperty
        #region CopyProperty
        /// Copies a property on the specified item(s)
        /// <param name="sourcePath">
        /// The path to the item(s) on which the property should be copied.
        /// The source name of the property to be copied.
        /// The path to the item(s) to copy the property to. It can be the same
        /// as the sourcePath as long as the destinationProperty is different.
        /// A PSObject for each item that is the new property after the copy.
        /// If <paramref name="sourcePath"/>, <paramref name="sourceProperty"/>,
        /// <paramref name="destinationPath"/>, or <paramref name="destinationProperty"/>
        ///  is null.
        /// If the <paramref name="sourcePath"/> refers to a provider that could not be found.
        /// If the <paramref name="sourcePath"/> refers to a drive that could not be found.
        /// If the provider that the <paramref name="sourcePath"/> refers to does
            string sourcePath,
                _sessionState.CopyProperty(
                    new string[] { sourcePath },
                    sourceProperty,
                    destinationPath,
                    destinationProperty,
                    false, false);
        /// The path(s) to the item(s) on which the property should be copied.
            string[] sourcePath,
                    sourcePath,
                    literalPath);
        /// Nothing. A PSObject for each item that the new property was copied to is
        /// written to the context.
        /// Gets the dynamic parameters for the copy-itemproperty cmdlet.
        internal object CopyPropertyDynamicParameters(
            return _sessionState.CopyPropertyDynamicParameters(path, sourceProperty, destinationPath, destinationProperty, context);
        #endregion CopyProperty
        #region MoveProperty
        /// Moves a property on the specified item(s)
        /// The path to the item(s) on which the property should be moved.
        /// The source name of the property to be moved.
        /// The path to the item(s) to move the property to. It can be the same
        /// A PSObject for each item that is the new property after the move.
        /// If <paramref name="destinationPath"/> resolves to more than one item.
        public Collection<PSObject> Move(
                _sessionState.MoveProperty(
        /// The path(s) to the item(s) on which the property should be moved.
        /// Nothing. A PSObject for each item that the property was moved to is written
        /// to the context.
        internal object MovePropertyDynamicParameters(
            return _sessionState.MovePropertyDynamicParameters(path, sourceProperty, destinationPath, destinationProperty, context);
        #endregion MoveProperty

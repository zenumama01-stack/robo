        #region IDynamicPropertyCmdletProvider accessors
        /// The path(s) to the item(s) on which the new property should be created.
        /// A property table containing the properties and their values.
        internal Collection<PSObject> NewProperty(
                throw PSTraceSource.NewArgumentNullException(nameof(property));
            NewProperty(paths, property, type, value, context);
            Collection<PSObject> results = context.GetAccumulatedObjects();
        /// Nothing. The property should be passed to the context as a PSObject.
        internal void NewProperty(
                    NewProperty(providerInstance, providerPath, property, type, value, context);
        /// Creates a new property on the item at the specified path.
        /// The name of the property to create.
        /// The type of the property to create.
        /// The value of the property to create.
        private void NewProperty(
                property != null,
                providerInstance.NewProperty(path, property, type, value, context);
                    "NewPropertyProviderException",
                    SessionStateStrings.NewPropertyProviderException,
                return NewPropertyDynamicParameters(providerInstance, providerPaths[0], propertyName, type, value, newContext);
        /// The value of the property.
        private object NewPropertyDynamicParameters(
                result = providerInstance.NewPropertyDynamicParameters(path, propertyName, type, value, context);
                    "NewPropertyDynamicParametersProviderException",
                    SessionStateStrings.NewPropertyDynamicParametersProviderException,
        /// Removes the specified property from the specified item.
        /// The path(s) to the item(s) to remove the property from.
        /// The name of the property to remove
        internal void RemoveProperty(string[] paths, string property, bool force, bool literalPath)
            RemoveProperty(paths, property, context);
        /// Removes the specified properties from the specified item.
        /// The path(s) to the item(s) to remove the properties from.
        internal void RemoveProperty(
                    RemoveProperty(providerInstance, providerPath, property, context);
        /// Removes the property from the item at the specified path.
        /// The name of the property to remove.
        private void RemoveProperty(
                "Caller should validate property before calling this method");
                providerInstance.RemoveProperty(path, property, context);
                    "RemovePropertyProviderException",
                    SessionStateStrings.RemovePropertyProviderException,
                return RemovePropertyDynamicParameters(providerInstance, providerPaths[0], propertyName, newContext);
        private object RemovePropertyDynamicParameters(
                result = providerInstance.RemovePropertyDynamicParameters(path, propertyName, context);
                    "RemovePropertyDynamicParametersProviderException",
                    SessionStateStrings.RemovePropertyDynamicParametersProviderException,
        /// Copies the specified property on the specified item to the specified property
        /// on the destination item.  The source and destination items can be the same item.
        /// <param name="sourcePaths">
        /// The path(s) to the item(s) to copy the property from.
        /// The name of the property to be copied.
        /// The path to the item to copy the property to.
        /// The name of the property to copy the property to.
        internal Collection<PSObject> CopyProperty(
            string[] sourcePaths,
            if (sourcePaths == null)
                throw PSTraceSource.NewArgumentNullException(nameof(sourcePaths));
            if (sourceProperty == null)
                throw PSTraceSource.NewArgumentNullException(nameof(sourceProperty));
            if (destinationPath == null)
                throw PSTraceSource.NewArgumentNullException(nameof(destinationPath));
            if (destinationProperty == null)
                throw PSTraceSource.NewArgumentNullException(nameof(destinationProperty));
            CopyProperty(sourcePaths, sourceProperty, destinationPath, destinationProperty, context);
        internal void CopyProperty(
            foreach (string sourcePath in sourcePaths)
                if (sourcePath == null)
                    // Save off the original filters
                    Collection<string> includeFilters = context.Include;
                    Collection<string> excludeFilters = context.Exclude;
                    string filterString = context.Filter;
                    // now modify the filters so that the destination isn't filtered
                    Collection<string> providerDestinationPaths =
                    // Now reapply the filters
                        filterString);
                        foreach (string providerDestinationPath in providerDestinationPaths)
                            CopyProperty(providerInstance, providerPath, sourceProperty, providerDestinationPath, destinationProperty, context);
        /// Copies the property.
        private void CopyProperty(
                sourcePath != null,
                "Caller should validate sourcePath before calling this method");
                sourceProperty != null,
                "Caller should validate sourceProperty before calling this method");
                destinationPath != null,
                "Caller should validate destinationPath before calling this method");
                destinationProperty != null,
                "Caller should validate destinationProperty before calling this method");
                providerInstance.CopyProperty(sourcePath, sourceProperty, destinationPath, destinationProperty, context);
                    "CopyPropertyProviderException",
                    SessionStateStrings.CopyPropertyProviderException,
                return CopyPropertyDynamicParameters(
                    providerPaths[0],
        /// The name of the property to copy.
        /// The name of the property to copy the property to on the destination item.
        private object CopyPropertyDynamicParameters(
                result = providerInstance.CopyPropertyDynamicParameters(
                    "CopyPropertyDynamicParametersProviderException",
                    SessionStateStrings.CopyPropertyDynamicParametersProviderException,
        /// Moves the specified property on the specified item to the specified property
        /// The path(s) to the item(s) to move the property from.
        /// The name of the property to be moved.
        /// The path to the item to move the property to.
        /// The name of the property to move the property to.
        internal Collection<PSObject> MoveProperty(
            MoveProperty(sourcePaths, sourceProperty, destinationPath, destinationProperty, context);
        internal void MoveProperty(
            // We shouldn't be filtering the destination path
            CmdletProviderContext destinationContext = new CmdletProviderContext(context);
            destinationContext.SetFilters(
            Collection<string> destinationProviderPaths =
                    destinationContext,
            if (destinationProviderPaths.Count > 1)
                        nameof(destinationPath),
                        SessionStateStrings.MovePropertyDestinationResolveToSingle);
                context.WriteError(new ErrorRecord(argException, argException.GetType().FullName, ErrorCategory.InvalidArgument, destinationProviderPaths));
                        MoveProperty(providerInstance, providerPath, sourceProperty, destinationProviderPaths[0], destinationProperty, context);
        /// Moves the property from one item to another.
        /// The property to be moved.
        /// The path of the item to move the property to.
        private void MoveProperty(
                providerInstance.MoveProperty(sourcePath, sourceProperty, destinationPath, destinationProperty, context);
                    "MovePropertyProviderException",
                    SessionStateStrings.MovePropertyProviderException,
        /// Gets the dynamic parameters for the move-itemproperty cmdlet.
                return MovePropertyDynamicParameters(
        /// The name of the property to move.
        /// The name of the property on the destination item to move the property to.
        private object MovePropertyDynamicParameters(
                result = providerInstance.MovePropertyDynamicParameters(
                    "MovePropertyDynamicParametersProviderException",
                    SessionStateStrings.MovePropertyDynamicParametersProviderException,
        /// Renames the specified property on the specified item to the specified property.
        /// The path(s) to the item(s) to rename the property on.
        /// The name of the property to be renamed.
        /// The name of the property to rename the property to.
        internal Collection<PSObject> RenameProperty(
            RenameProperty(sourcePaths, sourceProperty, destinationProperty, context);
        internal void RenameProperty(
                    RenameProperty(providerInstance, providerPath, sourceProperty, destinationProperty, context);
        /// Renames the property of the item at the specified path.
        /// The name of the property to rename.
        private void RenameProperty(
                providerInstance.RenameProperty(sourcePath, sourceProperty, destinationProperty, context);
                    "RenamePropertyProviderException",
                    SessionStateStrings.RenamePropertyProviderException,
                return RenamePropertyDynamicParameters(
        /// The new name for the property.
        private object RenamePropertyDynamicParameters(
                result = providerInstance.RenamePropertyDynamicParameters(
                    "RenamePropertyDynamicParametersProviderException",
                    SessionStateStrings.RenamePropertyDynamicParametersProviderException,
        #endregion IDynamicPropertyCmdletProvider accessors

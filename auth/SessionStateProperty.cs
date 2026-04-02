        #region IPropertyCmdletProvider accessors
        /// Gets the specified properties from the specified item.
        /// A list of the properties that the provider should return.
        /// A property table container the properties and their values.
        internal Collection<PSObject> GetProperty(
                throw PSTraceSource.NewArgumentNullException("path");
            GetProperty(paths, providerSpecificPickList, context);
        /// Nothing. A PSObject representing the properties should be written to the
        /// context.
        internal void GetProperty(
                    GetPropertyPrivate(
                        providerSpecificPickList,
        /// Gets the property from the item at the specified path.
        /// The names of the properties to get.
        private void GetPropertyPrivate(
                providerInstance.GetProperty(path, providerSpecificPickList, context);
                    "GetPropertyProviderException",
                    SessionStateStrings.GetPropertyProviderException,
                return GetPropertyDynamicParameters(providerInstance, providerPaths[0], providerSpecificPickList, newContext);
        private object GetPropertyDynamicParameters(
                result = providerInstance.GetPropertyDynamicParameters(path, providerSpecificPickList, context);
                    "GetPropertyDynamicParametersProviderException",
                    SessionStateStrings.GetPropertyDynamicParametersProviderException,
        /// Sets the specified properties on the specified item.
        /// A PSObject containing the properties to be changed.
        /// An array of PSObjects representing the properties that were set on each item.
        internal Collection<PSObject> SetProperty(string[] paths, PSObject property, bool force, bool literalPath)
                throw PSTraceSource.NewArgumentNullException("properties");
            SetProperty(paths, property, context);
        /// Sets the specified properties on specified item.
        /// A property table containing the properties and values to be set on the object.
        /// Nothing. A PSObject is passed to the context for the properties on each item
        /// that were modified.
        internal void SetProperty(
            PSObject property,
                        SetPropertyPrivate(providerInstance, providerPath, property, context);
        /// Sets the property of the item at the specified path.
        private void SetPropertyPrivate(
                "Caller should validate properties before calling this method");
                providerInstance.SetProperty(path, property, context);
                    "SetPropertyProviderException",
                    SessionStateStrings.SetPropertyProviderException,
                return SetPropertyDynamicParameters(providerInstance, providerPaths[0], propertyValue, newContext);
        private object SetPropertyDynamicParameters(
                result = providerInstance.SetPropertyDynamicParameters(path, propertyValue, context);
                    "SetPropertyDynamicParametersProviderException",
                    SessionStateStrings.SetPropertyDynamicParametersProviderException,
        /// Clears the specified property on the specified item.
        /// The path(s) to the item(s) to clear the property on.
        /// The name of the property to clear.
        internal void ClearProperty(
            if (propertyToClear == null)
                throw PSTraceSource.NewArgumentNullException(nameof(propertyToClear));
            ClearProperty(paths, propertyToClear, context);
        /// Clears the specified property in the specified item.
        /// A property table containing the property to clear.
                    ClearPropertyPrivate(providerInstance, providerPath, propertyToClear, context);
        /// Clears the value of the property from the item at the specified path.
        private void ClearPropertyPrivate(
                propertyToClear != null,
                "Caller should validate propertyToClear before calling this method");
                providerInstance.ClearProperty(path, propertyToClear, context);
                    "ClearPropertyProviderException",
                    SessionStateStrings.ClearPropertyProviderException,
                return ClearPropertyDynamicParameters(providerInstance, providerPaths[0], propertyToClear, newContext);
        private object ClearPropertyDynamicParameters(
                result = providerInstance.ClearPropertyDynamicParameters(path, propertyToClear, context);
                    "ClearPropertyDynamicParametersProviderException",
                    SessionStateStrings.ClearPropertyDynamicParametersProviderException,
        #endregion IPropertyCmdletProvider accessors

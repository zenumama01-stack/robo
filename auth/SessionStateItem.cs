        #region ItemCmdletProvider accessors
        /// Gets the specified object.
        /// The path(s) to the object(s). They can be either a relative (most common)
        /// or absolute path.
        /// The item at the specified path.
        internal Collection<PSObject> GetItem(string[] paths, bool force, bool literalPath)
            GetItem(paths, context);
            // Since there was not errors return the accumulated objects
        /// Nothing is returned, but all objects should be written to the WriteObject
        /// method of the <paramref name="context"/> parameter.
        internal void GetItem(
        private void GetItemPrivate(
                itemCmdletProvider.GetItem(path, context);
                    "GetItemProviderException",
                    SessionStateStrings.GetItemProviderException,
                return GetItemDynamicParameters(providerInstance, providerPaths[0], newContext);
        private object GetItemDynamicParameters(
                result = itemCmdletProvider.GetItemDynamicParameters(path, context);
                    "GetItemDynamicParametersProviderException",
                    SessionStateStrings.GetItemDynamicParametersProviderException,
        /// The path(s) to the object. It can be either a relative (most common)
        /// The new value for the item at the specified path.
        /// The item that was modified at the specified path.
        internal Collection<PSObject> SetItem(string[] paths, object value, bool force, bool literalPath)
            SetItem(paths, value, context);
        /// Sets the specified object to the specified value.
        internal void SetItem(
                if (providerPaths != null)
                        SetItem(providerInstance, providerPath, value, context);
        /// Sets item at the specified path.
        /// The value of the item.
        private void SetItem(
                itemCmdletProvider.SetItem(path, value, context);
                    "SetItemProviderException",
                    SessionStateStrings.SetItemProviderException,
        internal object SetItemDynamicParameters(string path, object value, CmdletProviderContext context)
                return SetItemDynamicParameters(providerInstance, providerPaths[0], value, newContext);
        /// The value to be set.
        private object SetItemDynamicParameters(
                result = itemCmdletProvider.SetItemDynamicParameters(path, value, context);
                    "SetItemDynamicParametersProviderException",
                    SessionStateStrings.SetItemDynamicParametersProviderException,
        /// Clears the specified object. Depending on the provider that the path
        /// maps to, this could mean the properties and/or content and/or value is
        /// cleared.
        /// The items that were cleared.
        /// If an error occurs that error will be thrown.
        internal Collection<PSObject> ClearItem(string[] paths, bool force, bool literalPath)
            ClearItem(paths, context);
        /// Clears the specified item. Depending on the provider that the path
        internal void ClearItem(
                        ClearItemPrivate(providerInstance, providerPath, context);
        private void ClearItemPrivate(
                itemCmdletProvider.ClearItem(path, context);
                    "ClearItemProviderException",
                    SessionStateStrings.ClearItemProviderException,
                return ClearItemDynamicParameters(providerInstance, providerPaths[0], newContext);
        private object ClearItemDynamicParameters(
                result = itemCmdletProvider.ClearItemDynamicParameters(path, context);
        /// Performs the default action on the specified item. The default action is
        /// determined by the provider.
        /// or absolute path(s).
        internal void InvokeDefaultAction(string[] paths, bool literalPath)
            InvokeDefaultAction(paths, context);
        /// Performs the default action on the specified item. The default action
        /// is determined by the provider.
        /// or absolute paths.
        internal void InvokeDefaultAction(
                        InvokeDefaultActionPrivate(providerInstance, providerPath, context);
        /// Invokes the default action on the item at the specified path.
        private void InvokeDefaultActionPrivate(
                itemCmdletProvider.InvokeDefaultAction(path, context);
                    "InvokeDefaultActionProviderException",
                    SessionStateStrings.InvokeDefaultActionProviderException,
        internal object InvokeDefaultActionDynamicParameters(string path, CmdletProviderContext context)
                return InvokeDefaultActionDynamicParameters(providerInstance, providerPaths[0], newContext);
        private object InvokeDefaultActionDynamicParameters(
                result = itemCmdletProvider.InvokeDefaultActionDynamicParameters(path, context);
                    "InvokeDefaultActionDynamicParametersProviderException",
                    SessionStateStrings.InvokeDefaultActionDynamicParametersProviderException,
        #endregion ItemCmdletProvider accessors

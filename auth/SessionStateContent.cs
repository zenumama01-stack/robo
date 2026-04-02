        #region IContentCmdletProvider accessors
        /// Gets the content reader for the specified item.
        /// The content readers for all items that the path resolves to.
        internal Collection<IContentReader> GetContentReader(string[] paths, bool force, bool literalPath)
            Collection<IContentReader> results = GetContentReader(paths, context);
        /// The path(s) to the item(s) to get the content reader from.
        internal Collection<IContentReader> GetContentReader(
            Collection<IContentReader> results = new Collection<IContentReader>();
                    IContentReader reader = GetContentReaderPrivate(providerInstance, providerPath, context);
                    if (reader != null)
                        results.Add(reader);
        private IContentReader GetContentReaderPrivate(
            IContentReader result = null;
                result = providerInstance.GetContentReader(path, context);
                    "GetContentReaderProviderException",
                    SessionStateStrings.GetContentReaderProviderException,
                    providerInstance.ProviderInfo,
            return GetContentReaderDynamicParameters(providerInstance, path, newContext);
        private object GetContentReaderDynamicParameters(
                result = providerInstance.GetContentReaderDynamicParameters(path, context);
                    "GetContentReaderDynamicParametersProviderException",
                    SessionStateStrings.GetContentReaderDynamicParametersProviderException,
        /// Gets the content writer for the specified item.
        /// The content writers for all items that the path resolves to.
        internal Collection<IContentWriter> GetContentWriter(string[] paths, bool force, bool literalPath)
            Collection<IContentWriter> results = GetContentWriter(paths, context);
        /// The path(s) to the item(s) to get the content writer from.
        internal Collection<IContentWriter> GetContentWriter(
            Collection<IContentWriter> results = new Collection<IContentWriter>();
                    IContentWriter result =
                        GetContentWriterPrivate(providerInstance, providerPath, context);
                        results.Add(result);
        /// Gets the content writer for the item at the specified path.
        private IContentWriter GetContentWriterPrivate(
            IContentWriter result = null;
                result = providerInstance.GetContentWriter(path, context);
                    "GetContentWriterProviderException",
                    SessionStateStrings.GetContentWriterProviderException,
                return GetContentWriterDynamicParameters(providerInstance, providerPaths[0], newContext);
        private object GetContentWriterDynamicParameters(
                result = providerInstance.GetContentWriterDynamicParameters(path, context);
                    "GetContentWriterDynamicParametersProviderException",
                    SessionStateStrings.GetContentWriterDynamicParametersProviderException,
        /// Clears all the content from the specified item.
        internal void ClearContent(string[] paths, bool force, bool literalPath)
            ClearContent(paths, context);
        /// Clears all of the content from the specified item.
        /// The path to the item to clear the content from.
        internal void ClearContent(
                    ClearContentPrivate(providerInstance, providerPath, context);
        /// Clears the content from the item at the specified path.
        private void ClearContentPrivate(
                providerInstance.ClearContent(path, context);
                    "ClearContentProviderException",
                    SessionStateStrings.ClearContentProviderException,
        internal object ClearContentDynamicParameters(
                return ClearContentDynamicParameters(providerInstance, providerPaths[0], newContext);
        /// Calls the provider to get the clear-content dynamic parameters.
        /// The instance of the provider to call
        /// The path to pass to the provider.
        /// The context the command is executing under.
        /// The dynamic parameter object returned by the provider.
        private object ClearContentDynamicParameters(
                result = providerInstance.ClearContentDynamicParameters(path, context);
                    "ClearContentDynamicParametersProviderException",
                    SessionStateStrings.ClearContentDynamicParametersProviderException,
        #endregion IContentCmdletProvider accessors

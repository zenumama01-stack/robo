    /// Exposes the Content nouns to the Cmdlet Providers to the Cmdlet base class. The methods of this class
    public sealed class ContentCmdletProviderIntrinsics
        private ContentCmdletProviderIntrinsics()
        internal ContentCmdletProviderIntrinsics(Cmdlet cmdlet)
        /// An instance of the sessionState.
        internal ContentCmdletProviderIntrinsics(SessionStateInternal sessionState)
        #region GetContentReader
        /// Gets the content reader for the item at the specified path.
        /// The path to the item to get the content reader for.
        /// The IContentReader for the item(s) at the specified path.
        public Collection<IContentReader> GetReader(string path)
            return _sessionState.GetContentReader(new string[] { path }, false, false);
        /// The path(s) to the item(s) to get the content reader for.
        public Collection<IContentReader> GetReader(string[] path, bool force, bool literalPath)
            return _sessionState.GetContentReader(path, force, literalPath);
        internal Collection<IContentReader> GetReader(
            return _sessionState.GetContentReader(new string[] { path }, context);
        /// Gets the dynamic parameters for the get-content cmdlet.
        internal object GetContentReaderDynamicParameters(
            return _sessionState.GetContentReaderDynamicParameters(path, context);
        #endregion GetContentReader
        #region GetContentWriter
        /// Gets the content writer for the item(s) at the specified path.
        /// The path to the item(s) to get the content writer for.
        /// The IContentWriter for the item(s) at the specified path.
        public Collection<IContentWriter> GetWriter(string path)
            return _sessionState.GetContentWriter(new string[] { path }, false, false);
        /// The path(s) to the item(s) to get the content writer for.
        public Collection<IContentWriter> GetWriter(string[] path, bool force, bool literalPath)
            return _sessionState.GetContentWriter(path, force, literalPath);
        internal Collection<IContentWriter> GetWriter(
            return _sessionState.GetContentWriter(new string[] { path }, context);
        /// Gets the dynamic parameters for the set-content and add-content cmdlet.
        internal object GetContentWriterDynamicParameters(
            return _sessionState.GetContentWriterDynamicParameters(path, context);
        #endregion GetContentWriter
        #region ClearContent
        /// Clears the content from the item(s) specified by the path.
        /// The path to the item(s) to clear the content from.
        public void Clear(string path)
            _sessionState.ClearContent(new string[] { path }, false, false);
        /// The path(s) to the item(s) to clear the content from.
        public void Clear(string[] path, bool force, bool literalPath)
            _sessionState.ClearContent(path, force, literalPath);
        /// Clears the content from the specified item(s)
        internal void Clear(string path, CmdletProviderContext context)
            _sessionState.ClearContent(new string[] { path }, context);
        /// Gets the dynamic parameters for the clear-content cmdlet.
        internal object ClearContentDynamicParameters(string path, CmdletProviderContext context)
            return _sessionState.ClearContentDynamicParameters(path, context);
        #endregion ClearContent

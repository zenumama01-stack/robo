    /// This is the base class for all the providers that produce a view
    /// on session state data (Variables, Aliases, and Functions)
    public abstract class SessionStateProviderBase : ContainerCmdletProvider, IContentCmdletProvider
             "SessionStateProvider",
             "Providers that produce a view of session state data.")]
            Dbg.PSTraceSource.GetTracer("SessionStateProvider",
             "Providers that produce a view of session state data.");
        /// Derived classes must override to get items from session state.
        /// The name of the item to get.
        /// The item of the given name in the appropriate session state table.
        internal abstract object GetSessionStateItem(string name);
        /// Sets a session state item in the appropriate session state table.
        /// Derived classes must override this method to set the item in the
        /// proper table.
        /// The new value for the item.
        internal abstract void SetSessionStateItem(string name, object value, bool writeItem);
        /// Removes a session state item from the appropriate session state table.
        /// Derived classes must override this method to remove items from the
        /// The name of the item to remove.
        internal abstract void RemoveSessionStateItem(string name);
        /// Gets all the items in the appropriate session state table.
        /// An IDictionary representing the items in the session state table.
        /// The key is the name of the item and the value is the value.
        internal abstract IDictionary GetSessionStateTable();
        internal virtual object GetValueOfItem(object item)
        internal virtual bool CanRenameItem(object item)
        /// Gets an item from session state.
        /// Name of the item to get.
        /// The item instance is written to the WriteObject
        protected override void GetItem(string name)
            IDictionary table = GetSessionStateTable();
                    item = table.Values;
                    item = table[name];
                if (SessionState.IsVisible(this.Context.Origin, item))
                    WriteItemObject(item, name, isContainer);
        /// Sets a session state item to a given value.
        /// Name of the item to set
        /// The value to which to set the item
        /// Nothing. The item that was set is written to the
        /// WriteObject method.
        protected override void SetItem(
                    PSTraceSource.NewArgumentNullException(nameof(name)),
                    "SetItemNullName",
                    name));
                string action = SessionStateProviderBaseStrings.SetItemAction;
                string resourceTemplate = SessionStateProviderBaseStrings.SetItemResourceTemplate;
                    SetSessionStateItem(name, value, true);
                    PSTraceSource.NewArgumentNullException(nameof(path)),
                    "ClearItemNullPath",
                string action = SessionStateProviderBaseStrings.ClearItemAction;
                string resourceTemplate = SessionStateProviderBaseStrings.ClearItemResourceTemplate;
                    SetSessionStateItem(path, null, false);
        /// Gets the item(s) at the given path.
        /// The name of the item to retrieve, or all if empty or null.
            CommandOrigin origin = this.Context.Origin;
                    dictionary = GetSessionStateTable();
                            "GetTableSecurityException",
                // bug Windows7 #300974 says that we should sort
                List<DictionaryEntry> sortedEntries = new List<DictionaryEntry>(dictionary.Count + 1);
                    sortedEntries.Add(entry);
                sortedEntries.Sort(
                    (DictionaryEntry left, DictionaryEntry right) =>
                        string leftKey = (string)left.Key;
                        string rightKey = (string)right.Key;
                        IComparer<string> stringComparer = StringComparer.CurrentCultureIgnoreCase;
                        return stringComparer.Compare(leftKey, rightKey);
                // Now write out each object
                foreach (DictionaryEntry entry in sortedEntries)
                        if (SessionState.IsVisible(origin, entry.Value))
                            WriteItemObject(entry.Value, (string)entry.Key, false);
                                "GetItemSecurityException",
                                (string)entry.Key));
                    item = GetSessionStateItem(path);
                    if (SessionState.IsVisible(origin, item))
                        WriteItemObject(item, path, false);
        /// Gets the name(s) of the item(s) at the given path.
                            "GetChildNamesSecurityException",
                // Now write out each object's key...
                            WriteItemObject(entry.Key, (string)entry.Key, false);
                        WriteItemObject(path, path, false);
        /// Determines if there are any items.
        /// The container to check to see if there are any children.
        /// True if path is empty or null, false otherwise.
                    if (GetSessionStateTable().Count > 0)
                            "HasChildItemsSecurityException",
        /// Determines if the specified item exists.
        /// The path to the item to check. If this is null or empty, the item
        /// container is used (and always exists).
                            "ItemExistsSecurityException",
        /// True if the path is valid, or false otherwise.
        /// The path may not contain the following characters:
        /// . ( ) :
            return !string.IsNullOrEmpty(path);
        /// Removes the item at the specified path.
        /// The name of the item to be removed.
                    PSTraceSource.NewArgumentException(nameof(path));
                    "RemoveItemNullPath",
                string action = SessionStateProviderBaseStrings.RemoveItemAction;
                string resourceTemplate = SessionStateProviderBaseStrings.RemoveItemResourceTemplate;
                        RemoveSessionStateItem(path);
                                "RemoveItemSecurityException",
        /// Creates a new item if one of the same name doesn't already exist.
        /// The value of the new item.
        protected override void NewItem(string path, string type, object newItem)
                    "NewItemNullPath",
                ArgumentNullException argException =
                    PSTraceSource.NewArgumentNullException("value");
                        "NewItemValueNotSpecified",
            if (ItemExists(path) && !Force)
                        SessionStateStrings.NewItemAlreadyExists,
                string action = SessionStateProviderBaseStrings.NewItemAction;
                string resourceTemplate = SessionStateProviderBaseStrings.NewItemResourceTemplate;
                        newItem);
                    SetItem(path, newItem);
        /// Copies the specified item.
        /// The name of the item to copy.
        protected override void CopyItem(string path, string copyPath, bool recurse)
                    "CopyItemNullPath",
            // If copyPath is null or empty, that means we are trying to copy
            // the item to itself so it should be a no-op.
            if (string.IsNullOrEmpty(copyPath))
                // Just get the item for -passthru
                        "CopyItemSecurityException",
                string action = SessionStateProviderBaseStrings.CopyItemAction;
                string resourceTemplate = SessionStateProviderBaseStrings.CopyItemResourceTemplate;
                        copyPath);
                        SetSessionStateItem(copyPath, GetValueOfItem(item), true);
                        SessionStateStrings.CopyItemDoesntExist,
        protected override void RenameItem(string name, string newName)
                    "RenameItemNullPath",
                item = GetSessionStateItem(name);
                        "RenameItemSecurityException",
                if (ItemExists(newName) && !Force)
                            nameof(newName),
                            newName);
                        if (CanRenameItem(item))
                            string action = SessionStateProviderBaseStrings.RenameItemAction;
                            string resourceTemplate = SessionStateProviderBaseStrings.RenameItemResourceTemplate;
                                if (string.Equals(name, newName, StringComparison.OrdinalIgnoreCase))
                                    // This is a no-op. Just get the item for -passthru
                                    GetItem(newName);
                                    SetSessionStateItem(newName, item, true);
                        SessionStateStrings.RenameItemDoesntExist,
        #region IContentCmdletProvider methods
        /// Gets an instance of the content reader for this provider for the
        /// The path to get the content reader for.
        /// An instance of an IContentReader for the given path.
            return new SessionStateProviderBaseContentReaderWriter(path, this);
        /// Gets an instance of the content writer for this provider for the
        /// The path to get the content writer for.
        /// An instance of an IContentWriter for the given path.
        /// Always throws a NotSupportedException.
        /// ignored.
        /// This exception is always thrown.
                    SessionStateStrings.IContent_Clear_NotSupported);
        #region dynamic parameters
        // For now, none of the derived providers need dynamic parameters
        // so these methods just return null
        /// Always returns null.
        /// <returns>Null.</returns>
        public object GetContentReaderDynamicParameters(string path) { return null; }
        public object GetContentWriterDynamicParameters(string path) { return null; }
        public object ClearContentDynamicParameters(string path) { return null; }
    /// The content reader/writer for all providers deriving from SessionStateProviderBase.
    public class SessionStateProviderBaseContentReaderWriter : IContentReader, IContentWriter
        /// Constructs a content reader/writer for the specified provider using the specified
        /// path to read or write the content.
        /// The path to the session state item which the content will be read or written.
        /// The SessionStateProviderBase derived provider that the content will be read or written
        /// from/to.
        /// if <paramref name="path"/> is null or empty.
        internal SessionStateProviderBaseContentReaderWriter(string path, SessionStateProviderBase provider)
        private readonly SessionStateProviderBase _provider;
            if (!_contentRead)
                object item = _provider.GetSessionStateItem(_path);
                    object getItemValueResult = _provider.GetValueOfItem(item);
                    if (getItemValueResult != null)
                        result = getItemValueResult as IList ?? new object[] { getItemValueResult };
                    _contentRead = true;
        private bool _contentRead;
                throw PSTraceSource.NewArgumentNullException(nameof(content));
            // Unravel the IList if there is only one value
            object valueToSet = content;
            if (content.Count == 1)
                valueToSet = content[0];
            _provider.SetSessionStateItem(_path, valueToSet, false);
        /// None of the derived providers supports seeking for V1 so this
        /// always throws a NotSupportedException.
        /// ignored
                    SessionStateStrings.IContent_Seek_NotSupported);
        /// Closes the reader. None of the derived providers need to
        /// close their reader so do nothing.
        public void Close() { }
        public void Dispose() { Close(); GC.SuppressFinalize(this); }

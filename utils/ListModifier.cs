    /// PSListModifier is a simple helper class created by the update-list cmdlet.
    /// The update-list cmdlet will either return an instance of this class, or
    /// it will internally use an instance of this class to implement the updates.
    /// Cmdlets can also take a PSListModifier as a parameter.  Usage might look like:
    ///     Get-Mailbox | Set-Mailbox -Alias @{Add='jim'}
    /// Alias would take a PSListModifier and the Cmdlet code would be responsible
    /// for apply updates (possibly using PSListModifier.ApplyTo or else using custom logic).
    public class PSListModifier
        /// Create a new PSListModifier with empty lists for Add/Remove.
        public PSListModifier()
            _itemsToAdd = new Collection<object>();
            _itemsToRemove = new Collection<object>();
            _replacementItems = new Collection<object>();
        /// Create a new PSListModifier with the specified add and remove lists.
        /// <param name="removeItems">The items to remove.</param>
        /// <param name="addItems">The items to add.</param>
        public PSListModifier(Collection<object> removeItems, Collection<object> addItems)
            _itemsToAdd = addItems ?? new Collection<object>();
            _itemsToRemove = removeItems ?? new Collection<object>();
        /// Create a new PSListModifier to replace a given list with replaceItems.
        /// <param name="replacementItems">The item(s) to replace an existing list with.</param>
        public PSListModifier(object replacementItems)
            if (replacementItems == null)
            else if (replacementItems is Collection<object>)
                _replacementItems = (Collection<object>)replacementItems;
            else if (replacementItems is IList<object>)
                _replacementItems = new Collection<object>((IList<object>)replacementItems);
            else if (replacementItems is IList)
                foreach (object item in (IList)replacementItems)
                    _replacementItems.Add(item);
                _replacementItems.Add(replacementItems);
        /// Create a new PSListModifier with the specified add and remove lists (in the hash.)
        /// <param name="hash">A hashtable, where the value for key Add is the list to add
        /// and the value for Remove is the list to remove.</param>
        public PSListModifier(Hashtable hash)
            if (hash == null)
                throw PSTraceSource.NewArgumentNullException(nameof(hash));
            foreach (DictionaryEntry entry in hash)
                if (entry.Key is string)
                    string key = entry.Key as string;
                    bool isAdd = key.Equals(AddKey, StringComparison.OrdinalIgnoreCase);
                    bool isRemove = key.Equals(RemoveKey, StringComparison.OrdinalIgnoreCase);
                    bool isReplace = key.Equals(ReplaceKey, StringComparison.OrdinalIgnoreCase);
                    if (!isAdd && !isRemove && !isReplace)
                        throw PSTraceSource.NewArgumentException(nameof(hash), PSListModifierStrings.ListModifierDisallowedKey, key);
                    Collection<object> collection;
                    if (isRemove)
                        collection = _itemsToRemove;
                    else if (isAdd)
                        collection = _itemsToAdd;
                        collection = _replacementItems;
                    IEnumerable enumerable = LanguagePrimitives.GetEnumerable(entry.Value);
                            collection.Add(obj);
                        collection.Add(entry.Value);
                    throw PSTraceSource.NewArgumentException(nameof(hash), PSListModifierStrings.ListModifierDisallowedKey, entry.Key);
        /// The list of items to add when ApplyTo is called.
        public Collection<object> Add
            get { return _itemsToAdd; }
        private readonly Collection<object> _itemsToAdd;
        /// The list of items to remove when AppyTo is called.
        public Collection<object> Remove
            get { return _itemsToRemove; }
        private readonly Collection<object> _itemsToRemove;
        /// The list of items to replace an existing list with.
        public Collection<object> Replace
            get { return _replacementItems; }
        private readonly Collection<object> _replacementItems;
        /// Update the given collection with the items in Add and Remove.
        /// <param name="collectionToUpdate">The collection to update.</param>
        public void ApplyTo(IList collectionToUpdate)
            if (collectionToUpdate == null)
                throw PSTraceSource.NewArgumentNullException(nameof(collectionToUpdate));
            if (_replacementItems.Count > 0)
                collectionToUpdate.Clear();
                foreach (object obj in _replacementItems)
                    collectionToUpdate.Add(PSObject.Base(obj));
                foreach (object obj in _itemsToRemove)
                    collectionToUpdate.Remove(PSObject.Base(obj));
                foreach (object obj in _itemsToAdd)
        public void ApplyTo(object collectionToUpdate)
            ArgumentNullException.ThrowIfNull(collectionToUpdate);
            collectionToUpdate = PSObject.Base(collectionToUpdate);
            if (collectionToUpdate is not IList list)
                throw PSTraceSource.NewInvalidOperationException(PSListModifierStrings.UpdateFailed);
            ApplyTo(list);
        internal Hashtable ToHashtable()
            Hashtable result = new Hashtable(2);
            if (_itemsToAdd.Count > 0)
                result.Add(AddKey, _itemsToAdd);
            if (_itemsToRemove.Count > 0)
                result.Add(RemoveKey, _itemsToRemove);
                result.Add(ReplaceKey, _replacementItems);
        internal const string AddKey = "Add";
        internal const string RemoveKey = "Remove";
        internal const string ReplaceKey = "Replace";
    /// A generic version of PSListModifier that exists for the sole purpose of making
    /// cmdlets that accept a PSListModifier more usable.  Users that look at the syntax
    /// of the command will see something like PSListModifier[Mailbox] and know they need
    /// to pass in Mailboxes.
    /// <typeparam name="T">The list element type</typeparam>
    public class PSListModifier<T> : PSListModifier
            : base(removeItems, addItems)
        /// <param name="replacementItems">The items to replace an existing list with.</param>
            : base(replacementItems)
            : base(hash)

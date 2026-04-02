    /// OrderedHashtable is a hashtable that preserves the order of the keys.
    public sealed class OrderedHashtable : Hashtable, IEnumerable
        private readonly OrderedDictionary _orderedDictionary;
        /// Initializes a new instance of the <see cref="OrderedHashtable"/> class.
        public OrderedHashtable()
            _orderedDictionary = new OrderedDictionary();
        /// <param name="capacity">The capacity.</param>
        public OrderedHashtable(int capacity) : base(capacity)
            _orderedDictionary = new OrderedDictionary(capacity);
        /// <param name="dictionary">The dictionary to use for initialization.</param>
        public OrderedHashtable(IDictionary dictionary)
            _orderedDictionary = new OrderedDictionary(dictionary.Count);
                _orderedDictionary.Add(entry.Key, entry.Value);
        /// Get the number of items in the hashtable.
                return _orderedDictionary.Count;
        /// Get if the hashtable is a fixed size.
        public override bool IsFixedSize
        /// Get if the hashtable is read-only.
        public override bool IsReadOnly
        /// Get if the hashtable is synchronized.
        public override bool IsSynchronized
        /// Gets the keys in the hashtable.
        public override ICollection Keys
                return _orderedDictionary.Keys;
        /// Gets the values in the hashtable.
        public override ICollection Values
                return _orderedDictionary.Values;
        /// Gets or sets the value associated with the specified key.
        /// <param name="key">The key.</param>
        /// <returns>The value associated with the key.</returns>
        public override object? this[object key]
                return _orderedDictionary[key];
                _orderedDictionary[key] = value;
        /// Adds the specified key and value to the hashtable.
        /// <param name="value">The value.</param>
        public override void Add(object key, object? value)
            _orderedDictionary.Add(key, value);
        /// Removes all keys and values from the hashtable.
            _orderedDictionary.Clear();
        /// Get a shallow clone of the hashtable.
        /// <returns>A shallow clone of the hashtable.</returns>
        public override object Clone()
            return new OrderedHashtable(_orderedDictionary);
        /// Determines whether the hashtable contains a specific key.
        /// <param name="key">The key to locate in the hashtable.</param>
        /// <returns>true if the hashtable contains an element with the specified key; otherwise, false.</returns>
            return _orderedDictionary.Contains(key);
        /// Determines whether the hashtable contains a specific value.
        /// <param name="value">The value to locate in the hashtable.</param>
        /// <returns>true if the hashtable contains an element with the specified value; otherwise, false.</returns>
        public override bool ContainsValue(object? value)
            foreach (DictionaryEntry entry in _orderedDictionary)
                if (Equals(entry.Value, value))
        /// Copies the elements of the hashtable to an array of type object, starting at the specified array index.
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the hashtable. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public override void CopyTo(Array array, int arrayIndex)
            _orderedDictionary.CopyTo(array, arrayIndex);
        /// Get the enumerator.
        /// <returns>The enumerator.</returns>
        public override IDictionaryEnumerator GetEnumerator()
            return _orderedDictionary.GetEnumerator();
        /// Removes the specified key from the hashtable.
        /// <param name="key">The key to remove.</param>
            _orderedDictionary.Remove(key);

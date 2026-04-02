    #region DataAddedEventArgs
    /// Event arguments passed to PSDataCollection DataAdded handlers.
    public sealed class DataAddedEventArgs : EventArgs
        /// <param name="psInstanceId">
        /// PowerShell InstanceId which added this data.
        /// Guid.Empty, if the data is not added by a PowerShell
        /// Index at which the data is added.
        internal DataAddedEventArgs(Guid psInstanceId, int index)
            PowerShellInstanceId = psInstanceId;
            Index = index;
        public int Index { get; }
        public Guid PowerShellInstanceId { get; }
    /// Event arguments passed to PSDataCollection DataAdding handlers.
    public sealed class DataAddingEventArgs : EventArgs
        /// <param name="itemAdded">
        /// The actual item about to be added.
        internal DataAddingEventArgs(Guid psInstanceId, object itemAdded)
            ItemAdded = itemAdded;
        /// The item about to be added.
        public object ItemAdded { get; }
    #region PSDataCollection
    /// <summary>build
    /// Thread Safe buffer used with PowerShell Hosting interfaces.
    public class PSDataCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable, IDisposable, ISerializable
        private readonly IList<T> _data;
        private ManualResetEvent _readWaitHandle;
        private bool _releaseOnEnumeration;
        private bool _isEnumerated;
        // a counter to keep track of active PowerShell instances
        // using this buffer.
        /// Whether the enumerator needs to be blocking
        /// by default.
        private bool _blockingEnumerator = false;
        /// Whether the ref count was incremented when
        /// BlockingEnumerator was updated.
        private bool _refCountIncrementedForBlockingEnumerator = false;
        private int _countNewData = 0;
        private int _dataAddedFrequency = 1;
        private Guid _sourceGuid = Guid.Empty;
        #region Public Constructors
        public PSDataCollection() : this(new List<T>())
        /// Creates a PSDataCollection that includes all the items in the IEnumerable and invokes Complete().
        /// <param name="items">
        /// Items used to initialize the collection
        /// This constructor is useful when the user wants to use an IEnumerable as an input to one of the PowerShell.BeginInvoke overloads.
        /// The invocation doesn't complete until Complete() is called on the PSDataCollection; this constructor does the Complete() on
        /// behalf of the user.
        public PSDataCollection(IEnumerable<T> items) : this(new List<T>(items))
            this.Complete();
        /// Initializes a new instance with the specified capacity
        /// <paramref name="capacity"/>
        /// <param name="capacity">
        /// The number of elements that the new buffer can initially
        /// Capacity is the number of elements that the PSDataCollection can
        /// store before resizing is required.
        public PSDataCollection(int capacity) : this(new List<T>(capacity))
        #region type converters
        /// Wrap the argument in a PSDataCollection.
        /// <param name="valueToConvert">The value to convert.</param>
        /// <returns>New collection of value, marked as Complete.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates",
            Justification = "There are already alternates to the implicit casts, ToXXX and FromXXX methods are unnecessary and redundant")]
        public static implicit operator PSDataCollection<T>(bool valueToConvert)
            return CreateAndInitializeFromExplicitValue(valueToConvert);
        public static implicit operator PSDataCollection<T>(string valueToConvert)
        public static implicit operator PSDataCollection<T>(int valueToConvert)
        public static implicit operator PSDataCollection<T>(byte valueToConvert)
        private static PSDataCollection<T> CreateAndInitializeFromExplicitValue(object valueToConvert)
            PSDataCollection<T> psdc = new PSDataCollection<T>();
            psdc.Add(LanguagePrimitives.ConvertTo<T>(valueToConvert));
            psdc.Complete();
            return psdc;
        public static implicit operator PSDataCollection<T>(Hashtable valueToConvert)
        public static implicit operator PSDataCollection<T>(T valueToConvert)
        /// <param name="arrayToConvert">The value to convert.</param>
        public static implicit operator PSDataCollection<T>(object[] arrayToConvert)
            if (arrayToConvert != null)
                foreach (var ae in arrayToConvert)
                    psdc.Add(LanguagePrimitives.ConvertTo<T>(ae));
        #region Internal Constructor
        /// Construct the DataBuffer using the supplied <paramref name="listToUse"/>
        /// as the data buffer.
        /// <param name="listToUse">
        /// buffer where the elements are stored
        /// Using this constructor will make the data buffer a wrapper on
        /// top of the <paramref name="listToUse"/>, which provides synchronized
        internal PSDataCollection(IList<T> listToUse)
            _data = listToUse;
        /// Creates a PSDataCollection from an ISerializable context.
        protected PSDataCollection(SerializationInfo info, StreamingContext context)
            if (info.GetValue("Data", typeof(IList<T>)) is not IList<T> listToUse)
            _blockingEnumerator = info.GetBoolean("BlockingEnumerator");
            _dataAddedFrequency = info.GetInt32("DataAddedCount");
            EnumeratorNeverBlocks = info.GetBoolean("EnumeratorNeverBlocks");
            _isOpen = info.GetBoolean("IsOpen");
        #region PSDataCollection Specific Public Methods / Properties
        /// Event fired when objects are being added to the underlying buffer.
        public event EventHandler<DataAddingEventArgs> DataAdding;
        /// Event fired when objects are done being added to the underlying buffer.
        public event EventHandler<DataAddedEventArgs> DataAdded;
        /// Event fired when the buffer is completed.
        public event EventHandler Completed;
        /// A boolean which determines if the buffer is open.
        /// An int that tells the frequency of Data Added events fired.
        /// Raises the DataAdded event only when data has been added a multiple of this many times,
        /// or when collection can receive no more data, if further data is added past the last event
        /// prior to completion.
        public int DataAddedCount
                return _dataAddedFrequency;
                bool raiseDataAdded = false;
                    _dataAddedFrequency = value;
                    if (_countNewData >= _dataAddedFrequency)
                        raiseDataAdded = true;
                        _countNewData = 0;
                if (raiseDataAdded)
                    // We should raise the event outside of the lock
                    // as the call is made into 3rd party code
                    RaiseDataAddedEvent(_lastPsInstanceId, _lastIndex);
        /// Serializes all input by default.
        /// This is supported only for PSDataCollections of PSObject.
        public bool SerializeInput
                return _serializeInput;
                if (typeof(T) != typeof(PSObject))
                    // If you drop this constraint, GetSerializedInput must be updated.
                    throw new NotSupportedException(PSDataBufferStrings.SerializationNotSupported);
                _serializeInput = value;
        private bool _serializeInput = false;
        /// Determines whether this PSDataCollection was created implicitly in support of
        /// data collection (for example, a workflow that wants to capture output but hasn't
        /// provided an instance of the PSDataCollection to capture it with.)
        public bool IsAutoGenerated
        /// Internal tag for indicating a source object identifier for this collection.
        internal Guid SourceId
                    return _sourceGuid;
                    _sourceGuid = value;
        /// If this flag is set to true, the items in the collection will be set to null when it is
        /// traversed using a PSDataCollectionEnumerator.
        internal bool ReleaseOnEnumeration
                    return _releaseOnEnumeration;
                    _releaseOnEnumeration = value;
        /// This flag is true when the collection has been enumerated at least once by a PSDataCollectionEnumerator.
        internal bool IsEnumerated
                    return _isEnumerated;
                    _isEnumerated = value;
        /// Completes insertions to the buffer.
        /// Subsequent Inserts to the buffer will result in an InvalidOperationException.
        public void Complete()
            bool raiseEvents = false;
                // Close the buffer
                        raiseEvents = true;
                        // release any threads to notify an event. Enumerator
                        // blocks on this syncObject.
                        Monitor.PulseAll(SyncObject);
                        if (_countNewData > 0)
                // raise the events outside of the lock.
                if (raiseEvents)
                    // unblock any readers waiting on the handle
                    _readWaitHandle?.Set();
                    // A temporary variable is used as the Completed may
                    // reach null (because of -='s) after the null check
                    Completed?.Invoke(this, EventArgs.Empty);
        /// Indicates whether the data collection should
        /// have a blocking enumerator by default. Currently
        /// only when a PowerShell object is associated with
        /// the data collection, a reference count is added
        /// which causes the enumerator to be blocking. This
        /// prevents the use of PSDataCollection without a
        /// PowerShell object. This property fixes the same.
        public bool BlockingEnumerator
                    return _blockingEnumerator;
                    _blockingEnumerator = value;
                    if (_blockingEnumerator)
                        if (!_refCountIncrementedForBlockingEnumerator)
                            _refCountIncrementedForBlockingEnumerator = true;
                        // TODO: false doesn't always leading to non-blocking
                        // behavior in an intuitive way. Need to follow up
                        // and fix this
                        if (_refCountIncrementedForBlockingEnumerator)
                            _refCountIncrementedForBlockingEnumerator = false;
                            DecrementRef();
        /// If this is set to true, then the enumerator returned from
        /// GetEnumerator() will never block.
        public bool EnumeratorNeverBlocks { get; set; }
        #region IList Generic Overrides
        /// Gets or sets the element at the specified index.
        /// The zero-based index of the element to get or set.
        /// Objects cannot be added to a closed buffer.
        /// Make sure the buffer is open for Add and Insert
        /// operations to succeed.
        /// index is less than 0.
        /// (or)
        /// index is equal to or greater than Count.
                    return _data[index];
                    if ((index < 0) || (index >= _data.Count))
                        throw PSTraceSource.NewArgumentOutOfRangeException(nameof(index), index,
                            PSDataBufferStrings.IndexOutOfRange, 0, _data.Count - 1);
                    if (_serializeInput)
                        value = (T)(object)GetSerializedObject(value);
                    _data[index] = value;
        /// Determines the index of a specific item in the buffer.
        /// The object to locate in the buffer.
        /// The index of item if found in the buffer; otherwise, -1.
        public int IndexOf(T item)
                return InternalIndexOf(item);
        /// Inserts an item to the buffer at the specified index.
        /// The zero-based index at which item should be inserted.
        /// The object to insert into the buffer.
        /// The index specified is less than zero or greater
        /// than Count.
        public void Insert(int index, T item)
                InternalInsertItem(Guid.Empty, index, item);
            RaiseEvents(Guid.Empty, index);
        /// Removes the item at the specified index.
        /// The zero-based index of the item to remove.
        /// index is not a valid index in the buffer.
                RemoveItem(index);
        #region ICollection Generic Overrides
        /// Gets the number of elements contained in the buffer.
                    if (_data == null)
                        return _data.Count;
        /// Gets a value indicating whether the buffer is read-only.
        /// Adds an item to the thread-safe buffer.
        /// item to add
            InternalAdd(Guid.Empty, item);
        /// Removes all items from the buffer.
                _data?.Clear();
        /// Determines whether the buffer contains an element with a specific value.
        /// true if the element value is found in the buffer; otherwise false.
                    item = (T)(object)GetSerializedObject(item);
                return _data.Contains(item);
        /// Copies the elements of the buffer to a specified array, starting at a particular index.
        /// The destination Array for the elements of type T copied from the buffer.
        /// <param name="arrayIndex">
        /// The zero-based index in the array at which copying begins.
        /// array is multidimensional.
        /// arrayIndex is equal to or greater than the length of array.
        /// The number of elements in the source buffer is greater than the
        /// available space from arrayIndex to the end of the destination array.
        /// Type T cannot be cast automatically to the type of the destination array.
        /// array is a null reference
        /// arrayIndex is less than 0.
                _data.CopyTo(array, arrayIndex);
        /// Removes the first occurrence of a specified item from the buffer.
        /// The object to remove from the buffer.
        /// true if item was successfully removed from the buffer; otherwise, false.
                int index = InternalIndexOf(item);
        #region IEnumerable Generic Overrides
        /// Returns an enumerator that iterates through the
        /// elements of the buffer.
        /// An IEnumerator for objects of the type stored in the buffer.
            return new PSDataCollectionEnumerator<T>(this, EnumeratorNeverBlocks);
        #region IList Overrides
        /// Adds an element to the buffer.
        /// The object to add to the buffer.
        /// The position into which the new element was inserted.
        /// value reference is null.
        /// value is not of the correct generic type T for the buffer.
        int IList.Add(object value)
            PSDataCollection<T>.VerifyValueType(value);
            int index = _data.Count;
            InternalAdd(Guid.Empty, (T)value);
        /// Determines whether the collection contains an
        /// element with a specific value.
        /// The object to locate in the collection
        /// true if the element value is found in the collection;
        /// otherwise false.
        bool IList.Contains(object value)
            return Contains((T)value);
        /// Determines the zero-based index of an element in the buffer.
        /// The element in the buffer whose index is being determined.
        /// The index of the value if found in the buffer; otherwise, -1.
        int IList.IndexOf(object value)
            return IndexOf((T)value);
        /// Inserts an object into the buffer at a specified index.
        /// The zero-based index at which value is to be inserted.
        void IList.Insert(int index, object value)
            Insert(index, (T)value);
        /// Removes the first occurrence of a specified object
        /// as an element from the buffer.
        /// The object to be removed from the buffer.
        void IList.Remove(object value)
            Remove((T)value);
        /// Gets a value that indicates whether the buffer is fixed in size.
        bool IList.IsFixedSize
        /// Gets a value that indicates whether the buffer is read-only.
        bool IList.IsReadOnly
        /// <exception cref="IndexOutOfRangeException">
        object IList.this[int index]
                return this[index];
                this[index] = (T)value;
        #region ICollection Overrides
        /// Gets a value that indicates whether the buffer is synchronized.
        bool ICollection.IsSynchronized
        /// Gets the object used to synchronize access to the thread-safe buffer.
        object ICollection.SyncRoot
                return SyncObject;
        /// Copies the elements of the collection to a specified array,
        /// starting at a particular index.
        /// The destination Array for the elements of type T copied
        /// from the buffer.
        void ICollection.CopyTo(Array array, int index)
                _data.CopyTo((T[])array, index);
        #region IEnumerable Overrides
        /// Returns an enumerator that iterates through the buffer.
        #region Streaming Behavior
        /// Makes a shallow copy of all the elements currently in this collection
        /// and clears them from this collection. This will not result in a blocking call.
        /// Calling this method might have side effects on the enumerator. When this
        /// method is called, the behavior of the enumerator is not defined.
        /// A new collection with a copy of all the elements in the current collection.
        public Collection<T> ReadAll()
            return ReadAndRemove(0);
        /// <param name="readCount">Maximum number of elements to read.</param>
        internal Collection<T> ReadAndRemove(int readCount)
            Dbg.Assert(_data != null, "Collection cannot be null");
            Dbg.Assert(readCount >= 0, "ReadCount cannot be negative");
            int resolvedReadCount = (readCount > 0 ? readCount : Int32.MaxValue);
                // Copy the elements into a new collection
                // and clear.
                for (int i = 0; i < resolvedReadCount; i++)
                    if (_data.Count > 0)
                        result.Add(_data[0]);
                        _data.RemoveAt(0);
                if (_readWaitHandle != null)
                    if (_data.Count > 0 || !_isOpen)
                        // release all the waiting threads.
                        _readWaitHandle.Set();
                        // reset the handle so that future
                        // threads will block
                        _readWaitHandle.Reset();
        internal T ReadAndRemoveAt0()
            T value = default(T);
                if (_data != null && _data.Count > 0)
                    value = _data[0];
        #region Protected Virtual Methods
        /// Inserts an item into the buffer at a specified index.
        /// InstanceId of PowerShell instance adding this data.
        /// Guid.Empty if not initiated by a PowerShell instance.
        /// The zero-based index of the buffer where the object is to be inserted
        /// The object to be inserted into the buffer.
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "ps", Justification = "PS signifies PowerShell and is used at many places in the product.")]
        protected virtual void InsertItem(Guid psInstanceId, int index, T item)
            RaiseDataAddingEvent(psInstanceId, item);
            _data.Insert(index, item);
        /// Removes the item at a specified index.
        /// The zero-based index of the buffer where the object is to be removed.
        /// than the number of items in the buffer.
        protected virtual void RemoveItem(int index)
            _data.RemoveAt(index);
        #region Serializable
        /// Implements the ISerializable contract for serializing a PSDataCollection.
            info.AddValue("Data", _data);
            info.AddValue("BlockingEnumerator", _blockingEnumerator);
            info.AddValue("DataAddedCount", _dataAddedFrequency);
            info.AddValue("EnumeratorNeverBlocks", EnumeratorNeverBlocks);
            info.AddValue("IsOpen", _isOpen);
        #region Internal/Private Methods and Properties
        /// Waitable handle for caller's to block until new data
        /// is added to the underlying buffer.
        internal WaitHandle WaitHandle
                if (_readWaitHandle == null)
                        // Create the handle signaled if there are objects in the buffer
                        // or the buffer has been closed.
                        _readWaitHandle ??= new ManualResetEvent(_data.Count > 0 || !_isOpen);
                return _readWaitHandle;
        /// Utility method to signal handles and raise events
        /// in the consistent order.
        private void RaiseEvents(Guid psInstanceId, int index)
                    // TODO: Should ObjectDisposedException be caught.
                _countNewData++;
                if (_countNewData >= _dataAddedFrequency || (_countNewData > 0 && !_isOpen))
                    // store information in case _dataAddedFrequency is updated or collection completes
                    // so that event may be raised using last added data.
                    _lastPsInstanceId = psInstanceId;
                    _lastIndex = index;
                // as the call is made into 3rd party code.
                RaiseDataAddedEvent(psInstanceId, index);
        private Guid _lastPsInstanceId;
        private int _lastIndex;
        private void RaiseDataAddingEvent(Guid psInstanceId, object itemAdded)
            // A temporary variable is used as the DataAdding may
            DataAdding?.Invoke(this, new DataAddingEventArgs(psInstanceId, itemAdded));
        private void RaiseDataAddedEvent(Guid psInstanceId, int index)
            // A temporary variable is used as the DataAdded may
            DataAdded?.Invoke(this, new DataAddedEventArgs(psInstanceId, index));
        /// The caller should make sure the method call is
        /// synchronized.
        /// Guid.Empty if this is not initiated by a PowerShell instance.
        /// The zero-based index of the buffer where the object is
        /// to be inserted.
        private void InternalInsertItem(Guid psInstanceId, int index, T item)
            if (!_isOpen)
                throw PSTraceSource.NewInvalidOperationException(PSDataBufferStrings.WriteToClosedBuffer);
            InsertItem(psInstanceId, index, item);
        internal void InternalAdd(Guid psInstanceId, T item)
            // should not rely on data.Count in "finally"
            // as another thread might add data
                // Add the item and set to raise events
                // so that events are raised outside of
                // lock.
                index = _data.Count;
                InternalInsertItem(psInstanceId, index, item);
                RaiseEvents(psInstanceId, index);
        /// Adds the elements of an ICollection to the end of the buffer.
        /// The ICollection whose elements should be added to the end of
        /// the buffer.
        /// <paramref name="collection"/> is null.
        internal void InternalAddRange(Guid psInstanceId, ICollection collection)
            if (collection == null)
                throw PSTraceSource.NewArgumentNullException(nameof(collection));
                foreach (object o in collection)
                    InsertItem(psInstanceId, _data.Count, (T)o);
                    // set raise events if at least one item is
                    // added.
        /// Increment counter to keep track of active PowerShell instances
        /// using this buffer. This is used only internally.
        internal void AddRef()
        /// Decrement counter to keep track of active PowerShell instances
        internal void DecrementRef()
                Dbg.Assert(_refCount > 0, "RefCount cannot be <= 0");
                _refCount--;
                if (_refCount != 0 && (!_blockingEnumerator || _refCount != 1))
                // release threads blocked on waithandle
                // release any threads to notify refCount is 0. Enumerator
                // blocks on this syncObject and it needs to be notified
                // when the count becomes 0.
        /// Returns the index of first occurrence of <paramref name="item"/>
        /// in the buffer.
        /// 0 based index of item if found,
        /// -1 otherwise.
        private int InternalIndexOf(T item)
            int count = _data.Count;
            for (int index = 0; index < count; index++)
                if (object.Equals(_data[index], item))
        /// Checks if the <paramref name="value"/> is of type T.
        /// Value to verify.
        private static void VerifyValueType(object value)
                if (typeof(T).IsValueType)
                    throw PSTraceSource.NewArgumentNullException(nameof(value), PSDataBufferStrings.ValueNullReference);
            else if (value is not T)
                throw PSTraceSource.NewArgumentException(nameof(value), PSDataBufferStrings.CannotConvertToGenericType,
                                                         value.GetType().FullName,
                                                         typeof(T).FullName);
        // Serializes an object, as long as it's not serialized.
        private static PSObject GetSerializedObject(object value)
            // This is a safe cast, as this method is only called with "SerializeInput" is set,
            // and that method throws if the collection type is not PSObject.
            PSObject result = value as PSObject;
            // Check if serialization would be idempotent
            if (SerializationWouldHaveNoEffect(result))
                object deserialized = PSSerializer.Deserialize(PSSerializer.Serialize(value));
                if (deserialized == null)
                    return PSObject.AsPSObject(deserialized);
        private static bool SerializationWouldHaveNoEffect(PSObject result)
            object baseObject = PSObject.Base(result);
            // Check if it's a primitive known type
            if (InternalSerializer.IsPrimitiveKnownType(baseObject.GetType()))
            // Check if it's a CIM type
            if (baseObject is Microsoft.Management.Infrastructure.CimInstance)
            // Check if it's got "Deserialized" in its type name
            if (result.TypeNames[0].StartsWith("Deserialized", StringComparison.OrdinalIgnoreCase))
        /// Sync object for this collection.
        /// Reference count variable.
        internal int RefCount
                return _refCount;
                    _refCount = value;
        #region Idle event
        /// Indicates whether or not the collection should pulse idle events.
        internal bool PulseIdleEvent
            get { return (IdleEvent != null); }
        internal event EventHandler<EventArgs> IdleEvent;
        /// Fires an idle event.
        internal void FireIdleEvent()
            IdleEvent.SafeInvoke(this, null);
        /// Pulses the collection.
        /// <param name="disposing">If true, release all managed resources.</param>
                if (_isDisposed)
                        _readWaitHandle.Dispose();
                        _readWaitHandle = null;
        #endregion IDisposable Overrides
    /// Interface to support PSDataCollectionEnumerator.
    /// Needed to provide a way to get to the non-blocking
    /// MoveNext implementation.
    internal interface IBlockingEnumerator<out T> : IEnumerator<T>
        bool MoveNext(bool block);
    #region PSDataCollectionEnumerator
    /// Enumerator for PSDataCollection. This enumerator blocks until
    /// either all the PowerShell operations are completed or the
    /// PSDataCollection is closed.
    internal sealed class PSDataCollectionEnumerator<T> : IBlockingEnumerator<T>
        private T _currentElement;
        private readonly PSDataCollection<T> _collToEnumerate;
        private readonly bool _neverBlock;
        /// PSDataCollection to enumerate.
        /// <param name="neverBlock">
        /// Controls if the enumerator is blocking by default or not.
        internal PSDataCollectionEnumerator(PSDataCollection<T> collection, bool neverBlock)
            Dbg.Assert(collection != null,
                "Collection cannot be null");
            Dbg.Assert(!collection.ReleaseOnEnumeration || !collection.IsEnumerated,
                "shouldn't enumerate more than once if ReleaseOnEnumeration is true");
            _collToEnumerate = collection;
            _index = 0;
            _currentElement = default(T);
            _collToEnumerate.IsEnumerated = true;
            _neverBlock = neverBlock;
        #region IEnumerator Overrides
        /// Gets the element in the collection at the current position
        /// of the enumerator.
        /// For better performance, this property does not throw an exception
        /// if the enumerator is positioned before the first element or after
        /// the last element; the value of the property is undefined.
                return _currentElement;
        /// Advances the enumerator to the next element in the collection.
        /// true if the enumerator successfully advanced to the next element;
        /// otherwise, false.
        /// This will block if the original collection is attached to any
        /// active PowerShell instances and the original collection is not
        /// closed.
            return MoveNext(!_neverBlock);
        /// <param name="block">True - to block when no elements are available.</param>
        public bool MoveNext(bool block)
            lock (_collToEnumerate.SyncObject)
                    if (_index < _collToEnumerate.Count)
                        _currentElement = _collToEnumerate[_index];
                        if (_collToEnumerate.ReleaseOnEnumeration)
                            _collToEnumerate[_index] = default(T);
                        _index++;
                    // we have reached the end if either the collection is closed
                    // or no powershell instance is bound to this collection.
                    if ((_collToEnumerate.RefCount == 0) || (!_collToEnumerate.IsOpen))
                    if (block)
                        if (_collToEnumerate.PulseIdleEvent)
                            _collToEnumerate.FireIdleEvent();
                            Monitor.Wait(_collToEnumerate.SyncObject);
                            // using light-weight monitor to block the current thread instead
                            // of AutoResetEvent. This saves using Kernel objects.
        /// Resets the enumerator to its initial position,
        /// which is before the first element in the collection.
        void IDisposable.Dispose()
    /// Class that represents various informational buffers like
    /// verbose, debug, warning, progress, information used with command invocation.
    internal sealed class PSInformationalBuffers
        private readonly Guid _psInstanceId;
        /// Guid of Powershell instance creating this buffers.
        /// Whenever an item is added to one of the buffers, this id is
        /// used to notify the buffer about the PowerShell instance adding
        /// this data.
        internal PSInformationalBuffers(Guid psInstanceId)
            Dbg.Assert(psInstanceId != Guid.Empty,
                "PowerShell instance id cannot be Guid.Empty");
            _psInstanceId = psInstanceId;
            progress = new PSDataCollection<ProgressRecord>();
            verbose = new PSDataCollection<VerboseRecord>();
            debug = new PSDataCollection<DebugRecord>();
            Warning = new PSDataCollection<WarningRecord>();
            Information = new PSDataCollection<InformationRecord>();
        /// A buffer representing Progress record objects of a PowerShell command invocation.
        /// Can be null.
        internal PSDataCollection<ProgressRecord> Progress
                return progress;
                progress = value;
        internal PSDataCollection<ProgressRecord> progress;
        /// A buffer representing Verbose objects of a PowerShell command invocation.
        internal PSDataCollection<VerboseRecord> Verbose
                return verbose;
                verbose = value;
        internal PSDataCollection<VerboseRecord> verbose;
        /// A buffer representing Debug objects of a PowerShell command invocation.
        internal PSDataCollection<DebugRecord> Debug
                return debug;
                debug = value;
        internal PSDataCollection<DebugRecord> debug;
        /// A buffer representing Warning objects of a PowerShell command invocation.
        internal PSDataCollection<WarningRecord> Warning { get; set; }
        /// A buffer representing Information objects of a PowerShell command invocation.
        internal PSDataCollection<InformationRecord> Information { get; set; }
        /// Adds item to the progress buffer.
        /// The item is added to the buffer along with PowerShell InstanceId.
        internal void AddProgress(ProgressRecord item) => progress?.InternalAdd(_psInstanceId, item);
        /// Adds item to the verbose buffer.
        internal void AddVerbose(VerboseRecord item) => verbose?.InternalAdd(_psInstanceId, item);
        /// Adds item to the debug buffer.
        internal void AddDebug(DebugRecord item) => debug?.InternalAdd(_psInstanceId, item);
        /// Adds item to the warning buffer.
        internal void AddWarning(WarningRecord item) => Warning?.InternalAdd(_psInstanceId, item);
        /// Adds item to the information buffer.
        internal void AddInformation(InformationRecord item) => Information?.InternalAdd(_psInstanceId, item);

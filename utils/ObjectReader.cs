    /// A PipelineReader for an ObjectStream.
    /// This class is not safe for multi-threaded operations.
    internal abstract class ObjectReaderBase<T> : PipelineReader<T>, IDisposable
        /// Construct with an existing ObjectStream.
        /// <param name="stream">The stream to read.</param>
        /// <exception cref="ArgumentNullException">Thrown if the specified stream is null.</exception>
        protected ObjectReaderBase([In, Out] ObjectStreamBase stream)
            ArgumentNullException.ThrowIfNull(stream);
        /// Event fired when objects are added to the underlying stream.
        public override event EventHandler DataReady
                lock (_monitorObject)
                    bool firstRegistrant = (InternalDataReady == null);
                    InternalDataReady += value;
                    if (firstRegistrant)
                        _stream.DataReady += this.OnDataReady;
                    InternalDataReady -= value;
                    if (InternalDataReady == null)
                        _stream.DataReady -= this.OnDataReady;
        public event EventHandler InternalDataReady = null;
        /// Waitable handle for caller's to block until data is ready to read from the underlying stream.
                return _stream.ReadHandle;
        /// <value>True if the stream is closed and contains no data, otherwise; false.</value>
        public override bool EndOfPipeline
                return _stream.EndOfPipeline;
                return _stream.IsOpen;
                return _stream.Count;
                return _stream.MaxCapacity;
            // 2003/09/02-JonN added call to close underlying stream
            _stream.Close();
        /// Handle DataReady events from the underlying stream.
        /// <param name="sender">The stream raising the event.</param>
        /// <param name="args">Standard event args.</param>
        private void OnDataReady(object sender, EventArgs args)
            // ObjectStream sender with 'this' since receivers
            // are expecting a PipelineReader<object>
            InternalDataReady.SafeInvoke(this, args);
        /// The underlying stream.
        /// <remarks>Can never be null</remarks>
        protected ObjectStreamBase _stream;
        /// This object is used to acquire an exclusive lock
        /// on event handler registration.
        /// Note that we lock _monitorObject rather than "this" so that
        /// we are protected from outside code interfering in our
        /// critical section.  Thanks to Wintellect for the hint.
        private readonly object _monitorObject = new object();
        protected abstract void Dispose(bool disposing);
    /// A PipelineReader reading objects from an ObjectStream.
    internal class ObjectReader : ObjectReaderBase<object>
        public ObjectReader([In, Out] ObjectStream stream)
            : base(stream)
        public override Collection<object> Read(int count)
            return _stream.Read(count);
        public override object Read()
            return _stream.Read();
        public override Collection<object> ReadToEnd()
            return _stream.ReadToEnd();
        /// stream. The method will block until exclusive access to the
        /// stream is acquired.  If there are no objects in the stream,
        public override Collection<object> NonBlockingRead()
            return _stream.NonBlockingRead(Int32.MaxValue);
        public override Collection<object> NonBlockingRead(int maxRequested)
            return _stream.NonBlockingRead(maxRequested);
        /// Peek the next object.
        /// <returns>The next object in the stream or ObjectStream.EmptyObject if the stream is empty.</returns>
        public override object Peek()
            return _stream.Peek();
    /// A PipelineReader reading PSObjects from an ObjectStream.
    internal class PSObjectReader : ObjectReaderBase<PSObject>
        public PSObjectReader([In, Out] ObjectStream stream)
        public override Collection<PSObject> Read(int count)
            return MakePSObjectCollection(_stream.Read(count));
        /// Read a single PSObject from the stream.
        /// <returns>The next PSObject in the stream.</returns>
        public override PSObject Read()
            return MakePSObject(_stream.Read());
        public override Collection<PSObject> ReadToEnd()
            return MakePSObjectCollection(_stream.ReadToEnd());
        public override Collection<PSObject> NonBlockingRead()
            return MakePSObjectCollection(_stream.NonBlockingRead(Int32.MaxValue));
        public override Collection<PSObject> NonBlockingRead(int maxRequested)
            return MakePSObjectCollection(_stream.NonBlockingRead(maxRequested));
        /// Peek the next PSObject.
        /// <returns>The next PSObject in the stream or ObjectStream.EmptyObject if the stream is empty.</returns>
        public override PSObject Peek()
            return MakePSObject(_stream.Peek());
        private static PSObject MakePSObject(object o)
            return PSObject.AsPSObject(o);
        // It might ultimately be more efficient to
        // make ObjectStream generic and convert the objects to PSObject
        // before inserting them into the initial Collection, so that we
        // don't have to convert the collection later.
        private static Collection<PSObject> MakePSObjectCollection(
            Collection<object> coll)
            if (coll == null)
            Collection<PSObject> retval = new Collection<PSObject>();
            foreach (object o in coll)
                retval.Add(MakePSObject(o));
    /// A ObjectReader for a PSDataCollection ObjectStream.
    /// PSDataCollection is introduced after 1.0. PSDataCollection is
    /// used to store data which can be used with different
    /// commands concurrently.
    /// Only Read() operation is supported currently.
    internal class PSDataCollectionReader<T, TResult>
        : ObjectReaderBase<TResult>
        private readonly PSDataCollectionEnumerator<T> _enumerator;
        public PSDataCollectionReader(PSDataCollectionStream<T> stream)
            System.Management.Automation.Diagnostics.Assert(stream.ObjectStore != null,
                "Stream should have a valid data store");
            _enumerator = (PSDataCollectionEnumerator<T>)stream.ObjectStore.GetEnumerator();
        public override Collection<TResult> Read(int count)
        /// The next object in the buffer or AutomationNull if buffer is closed
        /// and data is not available.
        /// This method blocks if the buffer is empty.
        public override TResult Read()
            if (_enumerator.MoveNext())
                result = _enumerator.Current;
            return ConvertToReturnType(result);
        public override Collection<TResult> ReadToEnd()
        public override Collection<TResult> NonBlockingRead()
            return NonBlockingRead(Int32.MaxValue);
        public override Collection<TResult> NonBlockingRead(int maxRequested)
            if (maxRequested < 0)
                throw PSTraceSource.NewArgumentOutOfRangeException(nameof(maxRequested), maxRequested);
            if (maxRequested == 0)
                return new Collection<TResult>();
            Collection<TResult> result = new Collection<TResult>();
            int readCount = maxRequested;
            while (readCount > 0)
                if (_enumerator.MoveNext(false))
                    result.Add(ConvertToReturnType(_enumerator.Current));
        public override TResult Peek()
        private static TResult ConvertToReturnType(object inputObject)
            Type resultType = typeof(TResult);
            if (typeof(PSObject) == resultType || typeof(object) == resultType)
                TResult result;
                LanguagePrimitives.TryConvertTo(inputObject, out result);
                "ReturnType should be either object or PSObject only");
    internal class PSDataCollectionPipelineReader<T, TReturn>
        : ObjectReaderBase<TReturn>
        private readonly PSDataCollection<T> _datastore;
        /// <param name="runspaceId"></param>
        internal PSDataCollectionPipelineReader(PSDataCollectionStream<T> stream,
            string computerName, Guid runspaceId)
            _datastore = stream.ObjectStore;
        /// Computer name passed in by the pipeline which
        /// created this reader.
        /// Runspace Id passed in by the pipeline which
        internal Guid RunspaceId { get; }
        public override Collection<TReturn> Read(int count)
        public override TReturn Read()
            if (_datastore.Count > 0)
                Collection<T> resultCollection = _datastore.ReadAndRemove(1);
                // ReadAndRemove returns a Collection<T> type but we
                // just want the single object contained in the collection.
                if (resultCollection.Count == 1)
                    result = resultCollection[0];
        public override Collection<TReturn> ReadToEnd()
        public override Collection<TReturn> NonBlockingRead()
        public override Collection<TReturn> NonBlockingRead(int maxRequested)
                return new Collection<TReturn>();
            Collection<TReturn> results = new Collection<TReturn>();
                    results.Add(ConvertToReturnType((_datastore.ReadAndRemove(1))[0]));
                    readCount--;
        public override TReturn Peek()
        /// Converts to the return type based on language primitives.
        /// <param name="inputObject">Input object to convert.</param>
        /// <returns>Input object converted to the specified return type.</returns>
        private static TReturn ConvertToReturnType(object inputObject)
            Type resultType = typeof(TReturn);
                TReturn result;
                _datastore.Dispose();

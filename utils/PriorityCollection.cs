    internal enum DataPriorityType : int
        /// This indicate that the data will be sent without priority consideration.
        /// Large data objects will be fragmented so that each fragmented piece can
        /// fit into one message.
        /// PromptResponse may be sent with or without priority considerations.
        PromptResponse = 1,
    #region Sending Data
    /// DataStructure used by different remoting protocol /
    /// DataStructures to pass data to transport manager.
    /// This class holds the responsibility of fragmenting.
    /// This allows to fragment an object only once and
    /// send the fragments to various machines thus saving
    /// fragmentation time.
    internal class PrioritySendDataCollection
        // actual data store(s) to store priority based data and its
        // corresponding sync objects to provide thread safety.
        private SerializedDataStream[] _dataToBeSent;
        // array of sync objects, one for each element in _dataToBeSent
        private object[] _dataSyncObjects;
        // fragmentor used to serialize & fragment objects added to this collection.
        private Fragmentor _fragmentor;
        // callbacks used if no data is available at any time.
        // these callbacks are used to notify when data becomes available under
        // suc circumstances.
        private readonly SerializedDataStream.OnDataAvailableCallback _onSendCollectionDataAvailable;
        private bool _isHandlingCallback;
        private readonly object _readSyncObject = new object();
        /// Callback that is called once a fragmented data is available to send.
        /// Fragmented object that can be sent to the remote end.
        /// <param name="priorityType">
        /// Priority stream to which <paramref name="data"/> belongs to.
        internal delegate void OnDataAvailableCallback(byte[] data, DataPriorityType priorityType);
        /// Constructs a PrioritySendDataCollection object.
        internal PrioritySendDataCollection()
            _onSendCollectionDataAvailable = new SerializedDataStream.OnDataAvailableCallback(OnDataAvailable);
        internal Fragmentor Fragmentor
                return _fragmentor;
                Dbg.Assert(value != null, "Fragmentor cannot be null.");
                _fragmentor = value;
                // create serialized streams using fragment size.
                string[] names = Enum.GetNames<DataPriorityType>();
                _dataToBeSent = new SerializedDataStream[names.Length];
                _dataSyncObjects = new object[names.Length];
                    _dataToBeSent[i] = new SerializedDataStream(_fragmentor.FragmentSize);
                    _dataSyncObjects[i] = new object();
        /// Adds data to this collection. The data is fragmented in this method
        /// before being stored into the collection. So the calling thread
        /// will get affected, if it tries to add a huge object.
        /// data to be added to the collection. Caller should make sure this is not
        /// <param name="priority">
        /// Priority of the data.
        internal void Add<T>(RemoteDataObject<T> data, DataPriorityType priority)
            Dbg.Assert(data != null, "Cannot send null data object");
            Dbg.Assert(_fragmentor != null, "Fragmentor cannot be null while adding objects");
            Dbg.Assert(_dataToBeSent != null, "Serialized streams are not initialized");
            // make sure the only one object is fragmented and added to the collection
            // at any give time. This way the order of fragment is maintained
            // in the SendDataCollection(s).
            lock (_dataSyncObjects[(int)priority])
                _fragmentor.Fragment<T>(data, _dataToBeSent[(int)priority]);
        /// The data is added with Default priority.
        internal void Add<T>(RemoteDataObject<T> data)
            Add<T>(data, DataPriorityType.Default);
        /// Clears fragmented objects stored so far in this collection.
                NOTE: Error paths during initialization can cause _dataSyncObjects to be null
                causing an unhandled exception in finalize and a process crash.
                Verify arrays and dataToBeSent objects before referencing.
            if (_dataSyncObjects != null && _dataToBeSent != null)
                const int promptResponseIndex = (int)DataPriorityType.PromptResponse;
                const int defaultIndex = (int)DataPriorityType.Default;
                lock (_dataSyncObjects[promptResponseIndex])
                    if (_dataToBeSent[promptResponseIndex] != null)
                        _dataToBeSent[promptResponseIndex].Dispose();
                        _dataToBeSent[promptResponseIndex] = null;
                lock (_dataSyncObjects[defaultIndex])
                    if (_dataToBeSent[defaultIndex] != null)
                        _dataToBeSent[defaultIndex].Dispose();
                        _dataToBeSent[defaultIndex] = null;
        /// Gets the fragment or if no fragment is available registers the callback which
        /// gets called once a fragment is available. These 2 steps are performed in a
        /// synchronized way.
        /// While getting a fragment the following algorithm is used:
        /// 1. If this is the first time or if the last fragment read is an EndFragment,
        ///    then a new set of fragments is chosen based on the implicit priority.
        ///    PromptResponse is higher in priority order than default.
        /// 2. If last fragment read is not an EndFragment, then next fragment is chosen from
        ///    the priority collection as the last fragment. This will ensure fragments
        ///    are sent in order.
        /// Callback to call once data is available. (This will be used if no data is currently
        /// available).
        /// Priority stream to which the returned object belongs to, if any.
        /// If the call does not return any data, the value of this "out" parameter
        /// is undefined.
        /// A FragmentRemoteObject if available, otherwise null.
        internal byte[] ReadOrRegisterCallback(OnDataAvailableCallback callback,
            out DataPriorityType priorityType)
            lock (_readSyncObject)
                priorityType = DataPriorityType.Default;
                // Send data from which ever stream that has data directly.
                SerializedDataStream promptDataToBeSent = _dataToBeSent[(int)DataPriorityType.PromptResponse];
                if (promptDataToBeSent is not null)
                    result = promptDataToBeSent.ReadOrRegisterCallback(_onSendCollectionDataAvailable);
                    priorityType = DataPriorityType.PromptResponse;
                    SerializedDataStream defaultDataToBeSent = _dataToBeSent[(int)DataPriorityType.Default];
                    if (defaultDataToBeSent is not null)
                        result = defaultDataToBeSent.ReadOrRegisterCallback(_onSendCollectionDataAvailable);
                // No data to return..so register the callback.
                    // Register callback.
        private void OnDataAvailable(byte[] data, bool isEndFragment)
                // PromptResponse and Default priority collection can both raise at the
                // same time. This will take care of the situation.
                if (_isHandlingCallback)
                _isHandlingCallback = true;
            if (_onDataAvailableCallback != null)
                DataPriorityType prType;
                // now get the fragment and call the callback..
                byte[] result = ReadOrRegisterCallback(_onDataAvailableCallback, out prType);
                    // reset the onDataAvailableCallback so that we dont notify
                    // multiple times. we are resetting before actually calling
                    // the callback to make sure the caller calls ReadOrRegisterCallback
                    // at a later point and we dont loose the callback handle.
                    OnDataAvailableCallback realCallback = _onDataAvailableCallback;
                    _onDataAvailableCallback = null;
                    realCallback(result, prType);
            _isHandlingCallback = false;
    #region Receiving Data
    /// DataStructure used by remoting transport layer to store
    /// data being received from the wire for a particular priority
    /// stream.
    internal class ReceiveDataCollection : IDisposable
        // fragmentor used to defragment objects added to this collection.
        private readonly Fragmentor _defragmentor;
        // this stream holds incoming data..this stream doesn't know anything
        // about fragment boundaries.
        private MemoryStream _pendingDataStream;
        // the idea is to maintain 1 whole object.
        // 1 whole object may contain any number of fragments. blob from
        // each fragment is written to this stream.
        private MemoryStream _dataToProcessStream;
        private long _currentObjectId;
        private long _currentFrgId;
        // max deserialized object size in bytes
        private int? _maxReceivedObjectSize;
        private int _totalReceivedObjectSizeSoFar;
        private readonly bool _isCreateByClientTM;
        // this indicates if any off sync fragments can be ignored
        // this gets reset (to false) upon receiving the next "start" fragment along the stream
        private bool _canIgnoreOffSyncFragments = false;
        // objects need to cleanly release resources without
        // locking entire processing logic.
        // holds the number of threads that are currently in
        // ProcessRawData method. This might happen only for
        // ServerCommandTransportManager case where the command
        // is run in the same thread that runs ProcessRawData (to avoid
        // thread context switch).
        private int _numberOfThreadsProcessing;
        // limits the numberOfThreadsProcessing variable.
        private int _maxNumberOfThreadsToAllowForProcessing = 1;
        /// Callback that is called once a deserialized object is available.
        /// Deserialized object that can be processed.
        internal delegate void OnDataAvailableCallback(RemoteDataObject<PSObject> data);
        /// <param name="createdByClientTM">
        /// True if a client transport manager created this collection.
        /// This is used to generate custom messages for server and client.
        internal ReceiveDataCollection(Fragmentor defragmentor, bool createdByClientTM)
            Dbg.Assert(defragmentor != null, "ReceiveDataCollection needs a defragmentor to work with");
            // Memory streams created with an unsigned byte array provide a non-resizable stream view
            // of the data, and can only be written to. When using a byte array, you can neither append
            // to nor shrink the stream, although you might be able to modify the existing contents
            // depending on the parameters passed into the constructor. Empty memory streams are
            // resizable, and can be written to and read from.
            _pendingDataStream = new MemoryStream();
            _defragmentor = defragmentor;
            _isCreateByClientTM = createdByClientTM;
        /// Limits the deserialized object size received from a remote machine.
        internal int? MaximumReceivedObjectSize
            set { _maxReceivedObjectSize = value; }
        /// This might be needed only for ServerCommandTransportManager case
        /// where the command is run in the same thread that runs ProcessRawData
        /// (to avoid thread context switch). By default this class supports
        /// only one thread in ProcessRawData.
        internal void AllowTwoThreadsToProcessRawData()
            _maxNumberOfThreadsToAllowForProcessing = 2;
        /// Prepares the collection for a stream connect
        ///     When reconnecting from same client, its possible that fragment stream get interrupted if server is dropping data
        ///     When connecting from a new client, its possible to get trailing fragments of a previously partially transmitted object
        ///     Logic based on this flag, ensures such offsync/trailing fragments get ignored until the next full object starts flowing.
        internal void PrepareForStreamConnect()
            _canIgnoreOffSyncFragments = true;
        /// Process data coming from the transport. This method analyses the data
        /// and if an object can be created, it creates one and calls the
        /// <paramref name="callback"/> with the deserialized object. This method
        /// does not assume all fragments to be available. So if not enough fragments are
        /// available it will simply return..
        /// Data to process.
        /// Callback to call once a complete deserialized object is available.
        /// Defragmented Object if any, otherwise null.
        /// 1. Fragment Ids not in sequence
        /// 2. Object Ids does not match
        /// 3. The current deserialized object size of the received data exceeded
        /// allowed maximum object size. The current deserialized object size is {0}.
        /// Allowed maximum object size is {1}.
        /// Might throw other exceptions as the deserialized object is handled here.
        internal void ProcessRawData(byte[] data, OnDataAvailableCallback callback)
            Dbg.Assert(callback != null, "Callback cannot be null");
                _numberOfThreadsProcessing++;
                if (_numberOfThreadsProcessing > _maxNumberOfThreadsToAllowForProcessing)
                    Dbg.Assert(false, "Multiple threads are not allowed in ProcessRawData.");
                _pendingDataStream.Write(data, 0, data.Length);
                // this do loop will process one deserialized object.
                // using a loop allows to process multiple objects within
                // the same packet
                    if (_pendingDataStream.Length <= FragmentedRemoteObject.HeaderLength)
                        // there is not enough data to be processed.
                        s_baseTracer.WriteLine("Not enough data to process. Data is less than header length. Data length is {0}. Header Length {1}.",
                            _pendingDataStream.Length, FragmentedRemoteObject.HeaderLength);
                    byte[] dataRead = _pendingDataStream.ToArray();
                    // there is enough data to process here. get the fragment header
                    long objectId = FragmentedRemoteObject.GetObjectId(dataRead, 0);
                    if (objectId <= 0)
                        throw new PSRemotingTransportException(RemotingErrorIdStrings.ObjectIdCannotBeLessThanZero);
                    long fragmentId = FragmentedRemoteObject.GetFragmentId(dataRead, 0);
                    bool sFlag = FragmentedRemoteObject.GetIsStartFragment(dataRead, 0);
                    bool eFlag = FragmentedRemoteObject.GetIsEndFragment(dataRead, 0);
                    int blobLength = FragmentedRemoteObject.GetBlobLength(dataRead, 0);
                    if ((s_baseTracer.Options & PSTraceSourceOptions.WriteLine) != PSTraceSourceOptions.None)
                        s_baseTracer.WriteLine("Object Id: {0}", objectId);
                        s_baseTracer.WriteLine("Fragment Id: {0}", fragmentId);
                        s_baseTracer.WriteLine("Start Flag: {0}", sFlag);
                        s_baseTracer.WriteLine("End Flag: {0}", eFlag);
                        s_baseTracer.WriteLine("Blob Length: {0}", blobLength);
                    int totalLengthOfFragment = 0;
                        totalLengthOfFragment = checked(FragmentedRemoteObject.HeaderLength + blobLength);
                    catch (System.OverflowException)
                        s_baseTracer.WriteLine("Fragment too big.");
                        ResetReceiveData();
                        PSRemotingTransportException e = new PSRemotingTransportException(RemotingErrorIdStrings.ObjectIsTooBig);
                    if (_pendingDataStream.Length < totalLengthOfFragment)
                        s_baseTracer.WriteLine("Not enough data to process packet. Data is less than expected blob length. Data length {0}. Expected Length {1}.",
                            _pendingDataStream.Length, totalLengthOfFragment);
                    // ensure object size limit is not reached
                    if (_maxReceivedObjectSize.HasValue)
                        _totalReceivedObjectSizeSoFar = unchecked(_totalReceivedObjectSizeSoFar + totalLengthOfFragment);
                        if ((_totalReceivedObjectSizeSoFar < 0) || (_totalReceivedObjectSizeSoFar > _maxReceivedObjectSize.Value))
                            s_baseTracer.WriteLine("ObjectSize > MaxReceivedObjectSize. ObjectSize is {0}. MaxReceivedObjectSize is {1}",
                                _totalReceivedObjectSizeSoFar, _maxReceivedObjectSize);
                            PSRemotingTransportException e = null;
                            if (_isCreateByClientTM)
                                e = new PSRemotingTransportException(PSRemotingErrorId.ReceivedObjectSizeExceededMaximumClient,
                                    RemotingErrorIdStrings.ReceivedObjectSizeExceededMaximumClient,
                                e = new PSRemotingTransportException(PSRemotingErrorId.ReceivedObjectSizeExceededMaximumServer,
                                    RemotingErrorIdStrings.ReceivedObjectSizeExceededMaximumServer,
                    // appears like stream doesn't have individual position marker for read and write
                    // since we are going to read from now...
                    _pendingDataStream.Seek(0, SeekOrigin.Begin);
                    // we have enough data to process..so read the data from the stream and process.
                    byte[] oneFragment = new byte[totalLengthOfFragment];
                    // this will change position back to totalLengthOfFragment
                    int dataCount = _pendingDataStream.Read(oneFragment, 0, totalLengthOfFragment);
                    Dbg.Assert(dataCount == totalLengthOfFragment, "Unable to read enough data from the stream. Read failed");
                        PSEventId.ReceivedRemotingFragment, PSOpcode.Receive, PSTask.None,
                        (Int64)objectId,
                        (Int64)fragmentId,
                        sFlag ? 1 : 0,
                        eFlag ? 1 : 0,
                        (UInt32)blobLength,
                        new PSETWBinaryBlob(oneFragment, FragmentedRemoteObject.HeaderLength, blobLength));
                    byte[] extraData = null;
                    if (totalLengthOfFragment < _pendingDataStream.Length)
                        // there is more data in the stream than fragment size..so save that data
                        extraData = new byte[_pendingDataStream.Length - totalLengthOfFragment];
                        _pendingDataStream.Read(extraData, 0, (int)(_pendingDataStream.Length - totalLengthOfFragment));
                    // reset incoming stream.
                    _pendingDataStream.Dispose();
                    if (extraData != null)
                        _pendingDataStream.Write(extraData, 0, extraData.Length);
                    if (sFlag)
                        _canIgnoreOffSyncFragments = false; // reset this upon receiving a start fragment of a fresh object
                        _currentObjectId = objectId;
                        _dataToProcessStream = new MemoryStream();
                        // check if the data belongs to the same object as the start fragment
                        if (objectId != _currentObjectId)
                            s_baseTracer.WriteLine("ObjectId != CurrentObjectId");
                            // TODO - drop an ETW event
                            if (!_canIgnoreOffSyncFragments)
                                PSRemotingTransportException e = new PSRemotingTransportException(RemotingErrorIdStrings.ObjectIdsNotMatching);
                                s_baseTracer.WriteLine("Ignoring ObjectId != CurrentObjectId");
                        if (fragmentId != (_currentFrgId + 1))
                            s_baseTracer.WriteLine("Fragment Id is not in sequence.");
                                PSRemotingTransportException e = new PSRemotingTransportException(RemotingErrorIdStrings.FragmentIdsNotInSequence);
                                s_baseTracer.WriteLine("Ignoring Fragment Id is not in sequence.");
                    // make fragment id from this packet as the current fragment id
                    _currentFrgId = fragmentId;
                    // store the blob in a separate stream
                    _dataToProcessStream.Write(oneFragment, FragmentedRemoteObject.HeaderLength, blobLength);
                    if (eFlag)
                            // appears like stream doesn't individual position marker for read and write
                            // since we are going to read from now..i am resetting position to 0.
                            _dataToProcessStream.Seek(0, SeekOrigin.Begin);
                            RemoteDataObject<PSObject> remoteObject = RemoteDataObject<PSObject>.CreateFrom(_dataToProcessStream, _defragmentor);
                            s_baseTracer.WriteLine("Runspace Id: {0}", remoteObject.RunspacePoolId);
                            s_baseTracer.WriteLine("PowerShell Id: {0}", remoteObject.PowerShellId);
                            // notify the caller that a deserialized object is available.
                            callback(remoteObject);
                            // Reset the receive data buffers and start the process again.
                    if (_isDisposed && (_numberOfThreadsProcessing == 1))
                        ReleaseResources();
                    _numberOfThreadsProcessing--;
        /// Resets the store(s) holding received data.
        private void ResetReceiveData()
            // reset resources used to store incoming data (for a single object)
            _dataToProcessStream?.Dispose();
            _currentObjectId = 0;
            _currentFrgId = 0;
            _totalReceivedObjectSizeSoFar = 0;
        private void ReleaseResources()
            if (_pendingDataStream != null)
                _pendingDataStream = null;
            if (_dataToProcessStream != null)
                _dataToProcessStream.Dispose();
                _dataToProcessStream = null;
        /// Dispose and release resources.
        internal virtual void Dispose(bool isDisposing)
                if (_numberOfThreadsProcessing == 0)
    /// DataStructures to receive data from transport manager.
    /// This class holds the responsibility of defragmenting and
    /// deserializing.
    internal class PriorityReceiveDataCollection : IDisposable
        private readonly ReceiveDataCollection[] _recvdData;
        /// Construct a priority receive data collection.
        /// <param name="defragmentor">Defragmentor used to deserialize an object.</param>
        internal PriorityReceiveDataCollection(Fragmentor defragmentor, bool createdByClientTM)
            _recvdData = new ReceiveDataCollection[names.Length];
            for (int index = 0; index < names.Length; index++)
                _recvdData[index] = new ReceiveDataCollection(defragmentor, createdByClientTM);
        /// Limits the total data received from a remote machine.
        internal int? MaximumReceivedDataSize
                _defragmentor.DeserializationContext.MaximumAllowedMemory = value;
                foreach (ReceiveDataCollection recvdDataBuffer in _recvdData)
                    recvdDataBuffer.MaximumReceivedObjectSize = value;
        /// Prepares receive data streams for a reconnection.
            for (int index = 0; index < _recvdData.Length; index++)
                _recvdData[index].PrepareForStreamConnect();
                _recvdData[index].AllowTwoThreadsToProcessRawData();
        /// Priority stream this data belongs to.
        /// 4.The total data received from the remote machine exceeded allowed maximum.
        /// The total data received from remote machine is {0}. Allowed maximum is {1}.
            DataPriorityType priorityType,
            ReceiveDataCollection.OnDataAvailableCallback callback)
                _defragmentor.DeserializationContext.LogExtraMemoryUsage(data.Length);
                    e = new PSRemotingTransportException(PSRemotingErrorId.ReceivedDataSizeExceededMaximumClient,
                        RemotingErrorIdStrings.ReceivedDataSizeExceededMaximumClient,
                            _defragmentor.DeserializationContext.MaximumAllowedMemory.Value);
                    e = new PSRemotingTransportException(PSRemotingErrorId.ReceivedDataSizeExceededMaximumServer,
                        RemotingErrorIdStrings.ReceivedDataSizeExceededMaximumServer,
            _recvdData[(int)priorityType].ProcessRawData(data, callback);
            if (_recvdData != null)
                    _recvdData[index].Dispose();

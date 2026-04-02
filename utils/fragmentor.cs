    /// This class is used to hold a fragment of remoting PSObject for transporting to remote computer.
    /// A large remoting PSObject will be broken into fragments. Each fragment has a ObjectId and a FragmentId.
    /// The first fragment has a StartFragment marker. The last fragment also an EndFragment marker.
    /// These fragments can be reassembled on the receiving
    /// end by sequencing the fragment ids.
    /// Currently control objects (Control-C for stopping a pipeline execution) is not
    /// really fragmented. These objects are small. They are just wrapped into a single
    /// fragment.
    internal class FragmentedRemoteObject
        private byte[] _blob;
        private int _blobLength;
        /// SFlag stands for the IsStartFragment. It is the bit value in the binary encoding.
        internal const byte SFlag = 0x1;
        /// EFlag stands for the IsEndFragment. It is the bit value in the binary encoding.
        internal const byte EFlag = 0x2;
        /// HeaderLength is the total number of bytes in the binary encoding header.
        internal const int HeaderLength = 8 + 8 + 1 + 4;
        /// _objectIdOffset is the offset of the ObjectId in the binary encoding.
        private const int _objectIdOffset = 0;
        /// _fragmentIdOffset is the offset of the FragmentId in the binary encoding.
        private const int _fragmentIdOffset = 8;
        /// _flagsOffset is the offset of the byte in the binary encoding that contains the SFlag, EFlag and CFlag.
        private const int _flagsOffset = 16;
        /// _blobLengthOffset is the offset of the BlobLength in the binary encoding.
        private const int _blobLengthOffset = 17;
        /// _blobOffset is the offset of the Blob in the binary encoding.
        private const int _blobOffset = 21;
        internal FragmentedRemoteObject()
        /// Used to construct a fragment of PSObject to be sent to remote computer.
        /// <param name="blob"></param>
        /// <param name="objectId">
        /// ObjectId of the fragment.
        /// Caller should make sure this is not less than 0.
        /// <param name="fragmentId">
        /// FragmentId within the object.
        /// <param name="isEndFragment">
        /// true if this is a EndFragment.
        internal FragmentedRemoteObject(byte[] blob, long objectId, long fragmentId,
            bool isEndFragment)
            Dbg.Assert((blob != null) && (blob.Length != 0), "Cannot create a fragment for null or empty data.");
            Dbg.Assert(objectId >= 0, "Object Id cannot be < 0");
            Dbg.Assert(fragmentId >= 0, "Fragment Id cannot be < 0");
            ObjectId = objectId;
            FragmentId = fragmentId;
            IsStartFragment = fragmentId == 0;
            IsEndFragment = isEndFragment;
            _blob = blob;
            _blobLength = _blob.Length;
        #region Data Fields being sent
        /// All fragments of the same PSObject have the same ObjectId.
        internal long ObjectId { get; set; }
        /// FragmentId starts from 0. It increases sequentially by an increment of 1.
        internal long FragmentId { get; set; }
        /// The first fragment of a PSObject.
        internal bool IsStartFragment { get; set; }
        /// The last fragment of a PSObject.
        internal bool IsEndFragment { get; set; }
        /// Blob length. This enables scenarios where entire byte[] is
        /// not filled for the fragment.
        internal int BlobLength
                return _blobLength;
                Dbg.Assert(value >= 0, "BlobLength cannot be less than 0.");
                _blobLength = value;
        /// This is the actual data in bytes form.
        internal byte[] Blob
                return _blob;
                Dbg.Assert(value != null, "Blob cannot be null");
                _blob = value;
        #endregion Data Fields being sent
        /// This method generate a binary encoding of the FragmentedRemoteObject as follows:
        /// ObjectId: 8 bytes as long, byte order is big-endian. this value can only be non-negative.
        /// FragmentId: 8 bytes as long, byte order is big-endian. this value can only be non-negative.
        /// FlagsByte: 1 byte:
        ///       0x1 if IsStartOfFragment is true: This is called S-flag.
        ///       0x2 if IsEndOfFragment is true: This is called the E-flag.
        ///       0x4 if IsControl is true: This is called the C-flag.
        ///       The other bits are reserved for future use.
        ///       Now they must be zero when sending,
        ///       and they are ignored when receiving.
        /// BlobLength: 4 bytes as int, byte order is big-endian. this value can only be non-negative.
        /// Blob: BlobLength number of bytes.
        ///     0                   1                   2                   3
        ///     0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
        ///     +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ///     |                                                               |
        ///     +-+-+-+-+-+-+-+-         ObjectId               +-+-+-+-+-+-+-+-+
        ///     +-+-+-+-+-+-+-+-        FragmentId              +-+-+-+-+-+-+-+-+
        ///     |reserved |C|E|S|
        ///     |                        BlobLength                             |
        ///     |     Blob ...
        ///     +-+-+-+-+-+-+-+-
        /// The binary encoded FragmentedRemoteObject to be ready to pass to WinRS Send API.
        internal byte[] GetBytes()
            const int objectIdSize = 8; // number of bytes of long
            const int fragmentIdSize = 8; // number of bytes of long
            const int flagsSize = 1; // 1 byte for IsEndOfFrag and IsControl
            const int blobLengthSize = 4; // number of bytes of int
            int totalLength = objectIdSize + fragmentIdSize + flagsSize + blobLengthSize + BlobLength;
            byte[] result = new byte[totalLength];
            int idx = 0;
            // release build will optimize the calculation of the constants
            // ObjectId
            idx = _objectIdOffset;
            result[idx++] = (byte)((ObjectId >> (7 * 8)) & 0x7F); // sign bit is 0
            result[idx++] = (byte)((ObjectId >> (6 * 8)) & 0xFF);
            result[idx++] = (byte)((ObjectId >> (5 * 8)) & 0xFF);
            result[idx++] = (byte)((ObjectId >> (4 * 8)) & 0xFF);
            result[idx++] = (byte)((ObjectId >> (3 * 8)) & 0xFF);
            result[idx++] = (byte)((ObjectId >> (2 * 8)) & 0xFF);
            result[idx++] = (byte)((ObjectId >> 8) & 0xFF);
            result[idx++] = (byte)(ObjectId & 0xFF);
            // FragmentId
            idx = _fragmentIdOffset;
            result[idx++] = (byte)((FragmentId >> (7 * 8)) & 0x7F); // sign bit is 0
            result[idx++] = (byte)((FragmentId >> (6 * 8)) & 0xFF);
            result[idx++] = (byte)((FragmentId >> (5 * 8)) & 0xFF);
            result[idx++] = (byte)((FragmentId >> (4 * 8)) & 0xFF);
            result[idx++] = (byte)((FragmentId >> (3 * 8)) & 0xFF);
            result[idx++] = (byte)((FragmentId >> (2 * 8)) & 0xFF);
            result[idx++] = (byte)((FragmentId >> 8) & 0xFF);
            result[idx++] = (byte)(FragmentId & 0xFF);
            // E-flag and S-Flag
            idx = _flagsOffset;
            byte s_flag = IsStartFragment ? SFlag : (byte)0;
            byte e_flag = IsEndFragment ? EFlag : (byte)0;
            result[idx++] = (byte)(s_flag | e_flag);
            // BlobLength
            idx = _blobLengthOffset;
            result[idx++] = (byte)((BlobLength >> (3 * 8)) & 0xFF);
            result[idx++] = (byte)((BlobLength >> (2 * 8)) & 0xFF);
            result[idx++] = (byte)((BlobLength >> 8) & 0xFF);
            result[idx++] = (byte)(BlobLength & 0xFF);
            Array.Copy(_blob, 0, result, _blobOffset, BlobLength);
        /// Extract the objectId from a byte array, starting at the index indicated by
        /// startIndex parameter.
        /// <param name="fragmentBytes"></param>
        /// <param name="startIndex"></param>
        /// The objectId.
        /// If fragmentBytes is null.
        /// If startIndex is negative or fragmentBytes is not large enough to hold the entire header of
        /// a binary encoded FragmentedRemoteObject.
        internal static long GetObjectId(byte[] fragmentBytes, int startIndex)
            Dbg.Assert(fragmentBytes != null, "fragmentBytes cannot be null");
            Dbg.Assert(fragmentBytes.Length >= HeaderLength, "not enough data to decode object id");
            long objectId = 0;
            int idx = startIndex + _objectIdOffset;
            objectId = (((long)fragmentBytes[idx++]) << (7 * 8)) & 0x7F00000000000000;
            objectId += (((long)fragmentBytes[idx++]) << (6 * 8)) & 0xFF000000000000;
            objectId += (((long)fragmentBytes[idx++]) << (5 * 8)) & 0xFF0000000000;
            objectId += (((long)fragmentBytes[idx++]) << (4 * 8)) & 0xFF00000000;
            objectId += (((long)fragmentBytes[idx++]) << (3 * 8)) & 0xFF000000;
            objectId += (((long)fragmentBytes[idx++]) << (2 * 8)) & 0xFF0000;
            objectId += (((long)fragmentBytes[idx++]) << 8) & 0xFF00;
            objectId += ((long)fragmentBytes[idx++]) & 0xFF;
            return objectId;
        /// Extract the FragmentId from the byte array, starting at the index indicated by
        internal static long GetFragmentId(byte[] fragmentBytes, int startIndex)
            Dbg.Assert(fragmentBytes.Length >= HeaderLength, "not enough data to decode fragment id");
            long fragmentId = 0;
            int idx = startIndex + _fragmentIdOffset;
            fragmentId = (((long)fragmentBytes[idx++]) << (7 * 8)) & 0x7F00000000000000;
            fragmentId += (((long)fragmentBytes[idx++]) << (6 * 8)) & 0xFF000000000000;
            fragmentId += (((long)fragmentBytes[idx++]) << (5 * 8)) & 0xFF0000000000;
            fragmentId += (((long)fragmentBytes[idx++]) << (4 * 8)) & 0xFF00000000;
            fragmentId += (((long)fragmentBytes[idx++]) << (3 * 8)) & 0xFF000000;
            fragmentId += (((long)fragmentBytes[idx++]) << (2 * 8)) & 0xFF0000;
            fragmentId += (((long)fragmentBytes[idx++]) << 8) & 0xFF00;
            fragmentId += ((long)fragmentBytes[idx++]) & 0xFF;
            return fragmentId;
        /// Extract the IsStartFragment value from the byte array, starting at the index indicated by
        /// True is the S-flag is set in the encoding. Otherwise false.
        internal static bool GetIsStartFragment(byte[] fragmentBytes, int startIndex)
            Dbg.Assert(fragmentBytes != null, "fragment cannot be null");
            Dbg.Assert(fragmentBytes.Length >= HeaderLength, "not enough data to decode if it is a start fragment.");
            if ((fragmentBytes[startIndex + _flagsOffset] & SFlag) != 0)
        /// Extract the IsEndFragment value from the byte array, starting at the index indicated by
        /// True if the E-flag is set in the encoding. Otherwise false.
        internal static bool GetIsEndFragment(byte[] fragmentBytes, int startIndex)
            Dbg.Assert(fragmentBytes.Length >= HeaderLength, "not enough data to decode if it is an end fragment.");
            if ((fragmentBytes[startIndex + _flagsOffset] & EFlag) != 0)
        /// Extract the BlobLength value from the byte array, starting at the index indicated by
        /// The BlobLength value.
        internal static int GetBlobLength(byte[] fragmentBytes, int startIndex)
            Dbg.Assert(fragmentBytes.Length >= HeaderLength, "not enough data to decode blob length.");
            int blobLength = 0;
            int idx = startIndex + _blobLengthOffset;
            blobLength += (((int)fragmentBytes[idx++]) << (3 * 8)) & 0x7F000000;
            blobLength += (((int)fragmentBytes[idx++]) << (2 * 8)) & 0xFF0000;
            blobLength += (((int)fragmentBytes[idx++]) << 8) & 0xFF00;
            blobLength += ((int)fragmentBytes[idx++]) & 0xFF;
            return blobLength;
    /// A stream used to store serialized data. This stream holds serialized data in the
    /// form of fragments. Every "fragment size" data will hold a blob identifying the fragment.
    /// The blob has "ObjectId","FragmentId","Properties like Start,End","BlobLength"
    internal class SerializedDataStream : Stream, IDisposable
        [TraceSource("SerializedDataStream", "SerializedDataStream")]
        private static readonly PSTraceSource s_trace = PSTraceSource.GetTracer("SerializedDataStream", "SerializedDataStream");
        #region Global Constants
        private static long s_objectIdSequenceNumber = 0;
        private bool _isEntered;
        private readonly FragmentedRemoteObject _currentFragment;
        private long _fragmentId;
        private readonly int _fragmentSize;
        private readonly bool _notifyOnWriteFragmentImmediately;
        // MemoryStream does not dynamically resize as data is read. This will waste
        // lot of memory as data sent on the network will still be there in memory.
        // To avoid this a queue of memory streams (each stream is of fragmentsize)
        // is created..so after data is sent the MemoryStream is disposed there by
        // clearing resources.
        private readonly Queue<MemoryStream> _queuedStreams;
        private MemoryStream _writeStream;
        private MemoryStream _readStream;
        private int _writeOffset;
        private int _readOffSet;
        /// Callback that is called once a fragmented data is available.
        /// Data that resulted in this callback.
        /// true if data represents EndFragment of an object.
        internal delegate void OnDataAvailableCallback(byte[] data, bool isEndFragment);
        private OnDataAvailableCallback _onDataAvailableCallback;
        /// Creates a stream to hold serialized data.
        /// <param name="fragmentSize">
        /// fragmentSize to be used while creating fragment boundaries.
        internal SerializedDataStream(int fragmentSize)
            s_trace.WriteLine("Creating SerializedDataStream with fragmentsize : {0}", fragmentSize);
            Dbg.Assert(fragmentSize > 0, "fragmentsize should be greater than 0.");
            _currentFragment = new FragmentedRemoteObject();
            _queuedStreams = new Queue<MemoryStream>();
            _fragmentSize = fragmentSize;
        /// Use this constructor carefully. This will not write data into internal
        /// streams. Instead this will make the SerializedDataStream call the
        /// callback whenever a fragmented data is available. It is upto the caller
        /// to figure out what to do with the data.
        /// <param name="callbackToNotify">
        /// If this is not null, then callback will get notified whenever fragmented
        /// data is available. Read() will return null in this case always.
        internal SerializedDataStream(int fragmentSize,
            OnDataAvailableCallback callbackToNotify) : this(fragmentSize)
            if (callbackToNotify != null)
                _notifyOnWriteFragmentImmediately = true;
                _onDataAvailableCallback = callbackToNotify;
        #region Internal methods / Protected overrides
        /// Start using the stream exclusively (to write data). The stream can be entered only once.
        /// If you want to Enter again, first Exit and then Enter.
        internal void Enter()
            Dbg.Assert(!_isEntered, "Stream is already entered. You cannot enter into stream again.");
            _isEntered = true;
            _fragmentId = 0;
            // Initialize the current fragment
            _currentFragment.ObjectId = GetObjectId();
            _currentFragment.FragmentId = _fragmentId;
            _currentFragment.IsStartFragment = true;
            _currentFragment.BlobLength = 0;
            _currentFragment.Blob = new byte[_fragmentSize];
        /// Notify that the stream is not used to write anymore.
        internal void Exit()
            _isEntered = false;
            // write left over data
            if (_currentFragment.BlobLength > 0)
                // this is endfragment...as we are in Exit
                _currentFragment.IsEndFragment = true;
                WriteCurrentFragmentAndReset();
        /// Writes a block of bytes to the current stream using data read from buffer.
        /// The base MemoryStream is written to only if "FragmentSize" is reached.
        /// The buffer to read data from.
        /// The byte offset in buffer at which to begin writing from.
        /// <param name="count">
        /// The maximum number of bytes to write.
            Dbg.Assert(_isEntered, "Stream should be Entered before writing into.");
            int offsetToReadFrom = offset;
            int amountLeft = count;
            while (amountLeft > 0)
                int dataLeftInTheFragment = _fragmentSize - FragmentedRemoteObject.HeaderLength - _currentFragment.BlobLength;
                if (dataLeftInTheFragment > 0)
                    int amountToWriteIntoFragment = (amountLeft > dataLeftInTheFragment) ? dataLeftInTheFragment : amountLeft;
                    amountLeft -= amountToWriteIntoFragment;
                    // Write data into fragment
                    Array.Copy(buffer, offsetToReadFrom, _currentFragment.Blob, _currentFragment.BlobLength, amountToWriteIntoFragment);
                    _currentFragment.BlobLength += amountToWriteIntoFragment;
                    offsetToReadFrom += amountToWriteIntoFragment;
                    // write only if amountLeft is more than 0. I dont write if amountLeft is 0 as we are not
                    // sure if the fragment is EndFragment..we will know this only in Exit.
                    if (amountLeft > 0)
        /// Writes a byte to the current stream.
            byte[] buffer = new byte[1];
            buffer[0] = value;
            Write(buffer, 0, 1);
        /// Returns a byte[] which holds data of fragment size (or) serialized data of
        /// one object, which ever is greater. If data is not currently available, then
        /// the callback is registered and called whenever the data is available.
        /// callback to call once the data becomes available.
        /// a byte[] holding data read from the stream
        internal byte[] ReadOrRegisterCallback(OnDataAvailableCallback callback)
                if (_length <= 0)
                    _onDataAvailableCallback = callback;
                int bytesToRead = _length > _fragmentSize ? _fragmentSize : (int)_length;
                byte[] result = new byte[bytesToRead];
                Read(result, 0, bytesToRead);
        /// Read the currently accumulated data in queued memory streams.
        internal byte[] Read()
                if (bytesToRead > 0)
            int offSetToWriteTo = offset;
            int dataWritten = 0;
            Collection<MemoryStream> memoryStreamsToDispose = new Collection<MemoryStream>();
            MemoryStream prevReadStream = null;
                // technically this should throw an exception..but remoting callstack
                // is optimized ie., we are not locking in every layer (in powershell)
                // to save on performance..as a result there may be cases where
                // upper layer is trying to add stuff and stream is disposed while
                // adding stuff.
                while (dataWritten < count)
                    if (_readStream == null)
                        if (_queuedStreams.Count > 0)
                            _readStream = _queuedStreams.Dequeue();
                            if ((!_readStream.CanRead) || (prevReadStream == _readStream))
                                // if the stream is disposed CanRead returns false
                                // this will happen if a Write enqueues the stream
                                // and a Read reads the data without dequeuing
                                _readStream = null;
                            _readStream = _writeStream;
                        Dbg.Assert(_readStream.Length > 0, "Not enough data to read.");
                        _readOffSet = 0;
                    _readStream.Position = _readOffSet;
                    int result = _readStream.Read(buffer, offSetToWriteTo, count - dataWritten);
                    s_trace.WriteLine("Read {0} data from readstream: {1}", result, _readStream.GetHashCode());
                    dataWritten += result;
                    offSetToWriteTo += result;
                    _readOffSet += result;
                    _length -= result;
                    // dispose only if we dont read from the current write stream.
                    if ((_readStream.Capacity == _readOffSet) && (_readStream != _writeStream))
                        s_trace.WriteLine("Adding readstream {0} to dispose collection.", _readStream.GetHashCode());
                        memoryStreamsToDispose.Add(_readStream);
                        prevReadStream = _readStream;
            // Dispose the memory streams outside of the lock
            foreach (MemoryStream streamToDispose in memoryStreamsToDispose)
                s_trace.WriteLine("Disposing stream: {0}", streamToDispose.GetHashCode());
                streamToDispose.Dispose();
            return dataWritten;
        private void WriteCurrentFragmentAndReset()
            // log trace of the fragment
                PSEventId.SentRemotingFragment, PSOpcode.Send, PSTask.None,
                PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic,
                (Int64)(_currentFragment.ObjectId),
                (Int64)(_currentFragment.FragmentId),
                _currentFragment.IsStartFragment ? 1 : 0,
                _currentFragment.IsEndFragment ? 1 : 0,
                (UInt32)(_currentFragment.BlobLength),
                new PSETWBinaryBlob(_currentFragment.Blob, 0, _currentFragment.BlobLength));
            // finally write into memory stream
            byte[] data = _currentFragment.GetBytes();
            int amountLeft = data.Length;
            int offSetToReadFrom = 0;
            // user asked us to notify immediately..so no need
            // to write into memory stream..instead give the
            // data directly to user and let him figure out what to do.
            // This will save write + read + dispose!!
            if (!_notifyOnWriteFragmentImmediately)
                    if (_writeStream == null)
                        _writeStream = new MemoryStream(_fragmentSize);
                        s_trace.WriteLine("Created write stream: {0}", _writeStream.GetHashCode());
                        _writeOffset = 0;
                        int dataLeftInWriteStream = _writeStream.Capacity - _writeOffset;
                        if (dataLeftInWriteStream == 0)
                            // enqueue the current write stream and create a new one.
                            EnqueueWriteStream();
                            dataLeftInWriteStream = _writeStream.Capacity - _writeOffset;
                        int amountToWriteIntoStream = (amountLeft > dataLeftInWriteStream) ? dataLeftInWriteStream : amountLeft;
                        amountLeft -= amountToWriteIntoStream;
                        // write data
                        _writeStream.Position = _writeOffset;
                        _writeStream.Write(data, offSetToReadFrom, amountToWriteIntoStream);
                        offSetToReadFrom += amountToWriteIntoStream;
                        _writeOffset += amountToWriteIntoStream;
                        _length += amountToWriteIntoStream;
            // call the callback since we have data available
            _onDataAvailableCallback?.Invoke(data, _currentFragment.IsEndFragment);
            // prepare a new fragment
            _currentFragment.FragmentId = ++_fragmentId;
            _currentFragment.IsStartFragment = false;
            _currentFragment.IsEndFragment = false;
        private void EnqueueWriteStream()
            s_trace.WriteLine("Queuing write stream: {0} Length: {1} Capacity: {2}",
                _writeStream.GetHashCode(), _writeStream.Length, _writeStream.Capacity);
            _queuedStreams.Enqueue(_writeStream);
        /// This method provides a thread safe way to get an object id.
        /// An object Id in integer.
        private static long GetObjectId()
            return System.Threading.Interlocked.Increment(ref s_objectIdSequenceNumber);
        #region Disposable Overrides
                    foreach (MemoryStream streamToDispose in _queuedStreams)
                        // make sure we dispose only once.
                        if (streamToDispose.CanRead)
                    if ((_readStream != null) && (_readStream.CanRead))
                        _readStream.Dispose();
                    if ((_writeStream != null) && (_writeStream.CanRead))
                        _writeStream.Dispose();
        #region Stream Overrides
        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return true; } }
        /// Gets the length of the stream in bytes.
        public override long Length { get { return _length; } }
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        /// This is a No-Op intentionally as there is nothing
        /// to flush.
        public new void Dispose()
    /// This class performs the fragmentation as well as defragmentation operations of large objects to be sent
    /// to the other side. A large remoting PSObject will be broken into fragments. Each fragment has a ObjectId
    /// and a FragmentId. The last fragment also has an end of fragment marker. These fragments can be reassembled
    /// on the receiving end by sequencing the fragment ids.
    internal class Fragmentor
        private static readonly UTF8Encoding s_utf8Encoding = new UTF8Encoding();
        // This const defines the default depth to be used for serializing objects for remoting.
        private const int SerializationDepthForRemoting = 1;
        private int _fragmentSize;
        private readonly SerializationContext _serializationContext;
        /// Constructor which initializes fragmentor with FragmentSize.
        /// size of each fragment
        /// <param name="cryptoHelper"></param>
        internal Fragmentor(int fragmentSize, PSRemotingCryptoHelper cryptoHelper)
            Dbg.Assert(fragmentSize > 0, "fragment size cannot be less than 0.");
            _serializationContext = new SerializationContext(
                SerializationDepthForRemoting,
                SerializationOptions.RemotingOptions,
            DeserializationContext = new DeserializationContext(
                DeserializationOptions.RemotingOptions,
        /// The method performs the fragmentation operation.
        /// All fragments of the same object have the same ObjectId.
        /// Each fragment has its own Fragment Id. Fragment Id always starts from zero (0),
        /// and increments sequentially with an increment of 1.
        /// The last fragment is indicated by an End of Fragment marker.
        /// The object to be fragmented. Caller should make sure this is not null.
        /// <param name="dataToBeSent">
        /// Caller specified dataToStore to which the fragments are added
        /// one-by-one
        internal void Fragment<T>(RemoteDataObject<T> obj, SerializedDataStream dataToBeSent)
            Dbg.Assert(obj != null, "Cannot fragment a null object");
            Dbg.Assert(dataToBeSent != null, "SendDataCollection cannot be null");
            dataToBeSent.Enter();
                obj.Serialize(dataToBeSent, this);
                dataToBeSent.Exit();
        /// The deserialization context used by this fragmentor. DeserializationContext
        /// controls the amount of memory a deserializer can use and other things.
        internal DeserializationContext DeserializationContext { get; }
        /// The size limit of the fragmented object.
        internal int FragmentSize
                return _fragmentSize;
                Dbg.Assert(value > 0, "FragmentSize cannot be less than 0.");
                _fragmentSize = value;
        /// TypeTable used for Serialization/Deserialization.
        internal TypeTable TypeTable { get; set; }
        /// Serialize an PSObject into a byte array.
        internal void SerializeToBytes(object obj, Stream streamToWriteTo)
            Dbg.Assert(obj != null, "Cannot serialize a null object");
            Dbg.Assert(streamToWriteTo != null, "Stream to write to cannot be null");
            xmlSettings.CheckCharacters = false;
            xmlSettings.Indent = false;
            // we dont want the underlying stream to be closed as we expect
            // the stream to be usable after this call.
            xmlSettings.CloseOutput = false;
            xmlSettings.Encoding = UTF8Encoding.UTF8;
            xmlSettings.NewLineHandling = NewLineHandling.None;
            xmlSettings.ConformanceLevel = ConformanceLevel.Fragment;
            using (XmlWriter xmlWriter = XmlWriter.Create(streamToWriteTo, xmlSettings))
                Serializer serializer = new Serializer(xmlWriter, _serializationContext);
                serializer.TypeTable = TypeTable;
                serializer.Serialize(obj);
        /// Converts the bytes back to PSObject.
        /// <param name="serializedDataStream">
        /// The bytes to be deserialized.
        /// The deserialized object.
        /// If the deserialized object is null.
        internal PSObject DeserializeToPSObject(Stream serializedDataStream)
            Dbg.Assert(serializedDataStream != null, "Cannot Deserialize null data");
            Dbg.Assert(serializedDataStream.Length != 0, "Cannot Deserialize empty data");
            using (XmlReader xmlReader = XmlReader.Create(serializedDataStream, InternalDeserializer.XmlReaderSettingsForCliXml))
                Deserializer deserializer = new Deserializer(xmlReader, DeserializationContext);
                deserializer.TypeTable = TypeTable;
                result = deserializer.Deserialize();
                deserializer.Done();
                // cannot be null.
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.DeserializedObjectIsNull);
            return PSObject.AsPSObject(result);

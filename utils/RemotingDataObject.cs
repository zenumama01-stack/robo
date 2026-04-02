    /// This is the object used by Runspace,pipeline,host to send data
    /// to remote end. Transport layer owns breaking this into fragments
    /// and sending to other end
    internal class RemoteDataObject<T>
        private const int destinationOffset = 0;
        private const int dataTypeOffset = 4;
        private const int rsPoolIdOffset = 8;
        private const int psIdOffset = 24;
        private const int headerLength = 4 + 4 + 16 + 16;
        private const int SessionMask = 0x00010000;
        private const int RunspacePoolMask = 0x00021000;
        private const int PowerShellMask = 0x00041000;
        /// Constructs a RemoteDataObject from its
        /// individual components.
        /// Destination this object is going to.
        /// <param name="dataType">
        /// Payload type this object represents.
        /// <param name="runspacePoolId">
        /// Runspace id this object belongs to.
        /// <param name="powerShellId">
        /// PowerShell (pipeline) id this object belongs to.
        /// This may be null if the payload belongs to runspace.
        /// Actual payload.
        protected RemoteDataObject(RemotingDestination destination,
            RemotingDataType dataType,
            Guid powerShellId,
            T data)
            RunspacePoolId = runspacePoolId;
            PowerShellId = powerShellId;
        internal RemotingDestination Destination { get; }
        /// Gets the target (Runspace / Pipeline / Powershell / Host)
        /// the payload belongs to.
        internal RemotingTargetInterface TargetInterface
                int dt = (int)DataType;
                // get the most used ones in the top.
                if ((dt & PowerShellMask) == PowerShellMask)
                    return RemotingTargetInterface.PowerShell;
                if ((dt & RunspacePoolMask) == RunspacePoolMask)
                    return RemotingTargetInterface.RunspacePool;
                if ((dt & SessionMask) == SessionMask)
                    return RemotingTargetInterface.Session;
                return RemotingTargetInterface.InvalidTargetInterface;
        internal Guid RunspacePoolId { get; }
        internal Guid PowerShellId { get; }
        /// <param name="dataType"></param>
        /// <param name="powerShellId"></param>
        internal static RemoteDataObject<T> CreateFrom(RemotingDestination destination,
            return new RemoteDataObject<T>(destination, dataType, runspacePoolId, powerShellId, data);
        /// Creates a RemoteDataObject by deserializing <paramref name="data"/>.
        /// <param name="serializedDataStream"></param>
        /// <param name="defragmentor">
        /// Defragmentor used to deserialize an object.
        internal static RemoteDataObject<T> CreateFrom(Stream serializedDataStream, Fragmentor defragmentor)
            Dbg.Assert(serializedDataStream != null, "cannot construct a RemoteDataObject from null data");
            Dbg.Assert(defragmentor != null, "defragmentor cannot be null.");
            if ((serializedDataStream.Length - serializedDataStream.Position) < headerLength)
                PSRemotingTransportException e =
                    new PSRemotingTransportException(PSRemotingErrorId.NotEnoughHeaderForRemoteDataObject,
                        RemotingErrorIdStrings.NotEnoughHeaderForRemoteDataObject,
                    headerLength + FragmentedRemoteObject.HeaderLength);
            RemotingDestination destination = (RemotingDestination)DeserializeUInt(serializedDataStream);
            RemotingDataType dataType = (RemotingDataType)DeserializeUInt(serializedDataStream);
            Guid runspacePoolId = DeserializeGuid(serializedDataStream);
            Guid powerShellId = DeserializeGuid(serializedDataStream);
            object actualData = null;
            if ((serializedDataStream.Length - headerLength) > 0)
                actualData = defragmentor.DeserializeToPSObject(serializedDataStream);
            T deserializedObject = (T)LanguagePrimitives.ConvertTo(actualData, typeof(T),
                System.Globalization.CultureInfo.CurrentCulture);
            return new RemoteDataObject<T>(destination, dataType, runspacePoolId, powerShellId, deserializedObject);
        #region Serialize / Deserialize
        /// Serializes the object into the stream specified. The serialization mechanism uses
        /// UTF8 encoding to encode data.
        /// <param name="streamToWriteTo"></param>
        /// <param name="fragmentor">
        /// fragmentor used to serialize and fragment the object.
        internal virtual void Serialize(Stream streamToWriteTo, Fragmentor fragmentor)
            Dbg.Assert(streamToWriteTo != null, "Stream to write to cannot be null.");
            Dbg.Assert(fragmentor != null, "Fragmentor cannot be null.");
            SerializeHeader(streamToWriteTo);
            if (Data != null)
                fragmentor.SerializeToBytes(Data, streamToWriteTo);
        /// Serializes only the header portion of the object. ie., runspaceId,
        /// powerShellId, destination and dataType.
        /// <param name="streamToWriteTo">
        /// place where the serialized data is stored into.
        private void SerializeHeader(Stream streamToWriteTo)
            Dbg.Assert(streamToWriteTo != null, "stream to write to cannot be null");
            // Serialize destination
            SerializeUInt((uint)Destination, streamToWriteTo);
            // Serialize data type
            SerializeUInt((uint)DataType, streamToWriteTo);
            // Serialize runspace guid
            SerializeGuid(RunspacePoolId, streamToWriteTo);
            // Serialize powershell guid
            SerializeGuid(PowerShellId, streamToWriteTo);
        private static void SerializeUInt(uint data, Stream streamToWriteTo)
            byte[] result = new byte[4]; // size of int
            result[idx++] = (byte)(data & 0xFF);
            result[idx++] = (byte)((data >> 8) & 0xFF);
            result[idx++] = (byte)((data >> (2 * 8)) & 0xFF);
            result[idx++] = (byte)((data >> (3 * 8)) & 0xFF);
            streamToWriteTo.Write(result, 0, 4);
        private static uint DeserializeUInt(Stream serializedDataStream)
            Dbg.Assert(serializedDataStream.Length >= 4, "Not enough data to get Int.");
            uint result = 0;
            result |= (((uint)(serializedDataStream.ReadByte())) & 0xFF);
            result |= (((uint)(serializedDataStream.ReadByte() << 8)) & 0xFF00);
            result |= (((uint)(serializedDataStream.ReadByte() << (2 * 8))) & 0xFF0000);
            result |= (((uint)(serializedDataStream.ReadByte() << (3 * 8))) & 0xFF000000);
        private static void SerializeGuid(Guid guid, Stream streamToWriteTo)
            byte[] guidArray = guid.ToByteArray();
            streamToWriteTo.Write(guidArray, 0, guidArray.Length);
        private static Guid DeserializeGuid(Stream serializedDataStream)
            Dbg.Assert(serializedDataStream.Length >= 16, "Not enough data to get Guid.");
            byte[] guidarray = new byte[16]; // Size of GUID.
            for (int idx = 0; idx < 16; idx++)
                guidarray[idx] = (byte)serializedDataStream.ReadByte();
            return new Guid(guidarray);
    internal sealed class RemoteDataObject : RemoteDataObject<object>
        #region Constructors / Factory
        private RemoteDataObject(RemotingDestination destination,
            object data) : base(destination, dataType, runspacePoolId, powerShellId, data)
        internal static new RemoteDataObject CreateFrom(RemotingDestination destination,
            object data)
            return new RemoteDataObject(destination, dataType, runspacePoolId,
                powerShellId, data);

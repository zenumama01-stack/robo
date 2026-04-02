    /// The ServerDispatchTable class.
    internal class ServerDispatchTable : DispatchTable<RemoteHostResponse>
        // DispatchTable specialized for RemoteHostResponse.
    /// Provides a thread-safe dictionary that maps call-ids to AsyncData objects.
    /// When a thread tries to do a get on a hashtable key (callId) that has not been
    /// set it is blocked. Once the key's value is set the thread is released. This is
    /// used to synchronize server calls with their responses.
    /// This code needs to be thread-safe. The locking convention is that only the
    /// internal or public methods use locks and are thread-safe. The private methods
    /// do not use locks and are not thread-safe (unless called by the internal and
    /// public methods). If the private methods becomes internal or public
    /// please review the locking.
    internal class DispatchTable<T> where T : class
        /// Response async objects.
        private readonly Dictionary<long, AsyncObject<T>> _responseAsyncObjects = new Dictionary<long, AsyncObject<T>>();
        /// Next call id.
        private long _nextCallId = 0;
        /// Void call id.
        internal const long VoidCallId = -100;
        /// Create new call id.
        internal long CreateNewCallId()
            // Note: Only CreateNewCallId adds new records.
            long callId = Interlocked.Increment(ref _nextCallId);
            AsyncObject<T> responseAsyncObject = new AsyncObject<T>();
            lock (_responseAsyncObjects)
                _responseAsyncObjects[callId] = responseAsyncObject;
            return callId;
        /// Get response async object.
        private AsyncObject<T> GetResponseAsyncObject(long callId)
            AsyncObject<T> responseAsyncObject = null;
            Dbg.Assert(_responseAsyncObjects.ContainsKey(callId), "Expected _responseAsyncObjects.ContainsKey(callId)");
            responseAsyncObject = _responseAsyncObjects[callId];
            Dbg.Assert(responseAsyncObject != null, "Expected responseAsyncObject != null");
            return responseAsyncObject;
        /// Waits for response PSObject to be set and then returns it. Returns null
        /// if wait was aborted.
        /// <param name="callId">
        /// default return value (in case the remote end did not send response).
        internal T GetResponse(long callId, T defaultValue)
            // Note: Only GetResponse removes records.
                responseAsyncObject = GetResponseAsyncObject(callId);
            // This will block until Value is set on this AsyncObject.
            T remoteHostResponse = responseAsyncObject.Value;
            // Remove table entry to conserve memory: this table could be alive for a long time.
                _responseAsyncObjects.Remove(callId);
            // return caller specified value in case there is no response
            // from remote end.
            if (remoteHostResponse == null)
            return remoteHostResponse;
        /// Set response.
        internal void SetResponse(long callId, T remoteHostResponse)
            Dbg.Assert(remoteHostResponse != null, "Expected remoteHostResponse != null");
                // The response-async-object might not exist if the call was aborted by Ctrl-C or if
                // the call had a void return and no return value was expected.
                if (!_responseAsyncObjects.ContainsKey(callId))
                // Unblock the AsyncObject by setting its value.
                AsyncObject<T> responseAsyncObject = GetResponseAsyncObject(callId);
                responseAsyncObject.Value = remoteHostResponse;
        /// Abort call.
        private void AbortCall(long callId)
            // The response-async-object might not exist if the call was already aborted.
            // Releases blocked thread by setting null as return value, which should be detected by caller of GetResponse.
            responseAsyncObject.Value = null;
        /// Abort calls.
        private void AbortCalls(List<long> callIds)
            foreach (long callId in callIds)
                AbortCall(callId);
        /// Get all calls.
        private List<long> GetAllCalls()
            // Gets all the callIds that are waiting on calls.
            List<long> callIds = new List<long>();
            foreach (KeyValuePair<long, AsyncObject<T>> callIdResponseAsyncObjectPair in _responseAsyncObjects)
                callIds.Add(callIdResponseAsyncObjectPair.Key);
            return callIds;
        /// Abort all calls.
        internal void AbortAllCalls()
                List<long> callIds = GetAllCalls();
                AbortCalls(callIds);

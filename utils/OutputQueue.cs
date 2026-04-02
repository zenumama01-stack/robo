    /// Queue to provide sliding window capabilities for auto size functionality
    /// It provides caching capabilities (either the first N objects in a group
    /// or all the objects in a group)
    internal sealed class OutputGroupQueue
        /// Create a grouping cache.
        /// <param name="callBack">Notification callback to be called when the desired number of objects is reached.</param>
        /// <param name="objectCount">Max number of objects to be cached.</param>
        internal OutputGroupQueue(FormattedObjectsCache.ProcessCachedGroupNotification callBack, int objectCount)
            _notificationCallBack = callBack;
            _objectCount = objectCount;
        /// Create a time-bounded grouping cache.
        /// <param name="groupingDuration">Max amount of time to cache of objects.</param>
        internal OutputGroupQueue(FormattedObjectsCache.ProcessCachedGroupNotification callBack, TimeSpan groupingDuration)
            _groupingDuration = groupingDuration;
        /// Add an object to the cache.
        /// <param name="o">Object to add.</param>
        /// <returns>Objects the cache needs to return. It can be null.</returns>
        internal List<PacketInfoData> Add(PacketInfoData o)
            if (o is FormatStartData fsd)
                // just cache the reference (used during the notification call)
                _formatStartData = fsd;
            UpdateObjectCount(o);
            // STATE TRANSITION: we are not processing and we start
            if (!_processingGroup && (o is GroupStartData))
                // just set the flag and start caching
                _processingGroup = true;
                _currentObjectCount = 0;
                if (_groupingDuration > TimeSpan.MinValue)
                    _groupingTimer = Stopwatch.StartNew();
                _queue.Enqueue(o);
            // STATE TRANSITION: we are processing and we stop
            if (_processingGroup &&
                ((o is GroupEndData) ||
                (_objectCount > 0) && (_currentObjectCount >= _objectCount)) ||
                ((_groupingTimer != null) && (_groupingTimer.Elapsed > _groupingDuration))
                // reset the object count
                if (_groupingTimer != null)
                    _groupingTimer.Stop();
                    _groupingTimer = null;
                // add object to queue, to be picked up
                // we are at the end of a group, drain the queue
                Notify();
                _processingGroup = false;
                List<PacketInfoData> retVal = new List<PacketInfoData>();
                while (_queue.Count > 0)
                    retVal.Add(_queue.Dequeue());
            // NO STATE TRANSITION: check the state we are in
            if (_processingGroup)
                // we are in the caching state
            // we are not processing, so just return it
            List<PacketInfoData> ret = new List<PacketInfoData>();
            ret.Add(o);
        private void UpdateObjectCount(PacketInfoData o)
            // add only of it's not a control message
            // and it's not out of band
            if (o is FormatEntryData fed && !fed.outOfBand)
                _currentObjectCount++;
        private void Notify()
            if (_notificationCallBack == null)
            // filter out the out of band data, since they do not participate in the
            // auto resize algorithm
            List<PacketInfoData> validObjects = new List<PacketInfoData>();
            foreach (PacketInfoData x in _queue)
                if (x is FormatEntryData fed && fed.outOfBand)
                validObjects.Add(x);
            _notificationCallBack(_formatStartData, validObjects);
        /// Remove a single object from the queue.
        /// <returns>Object retrieved, null if queue is empty.</returns>
        internal PacketInfoData Dequeue()
            if (_queue.Count == 0)
            return _queue.Dequeue();
        /// Queue to store the currently cached objects.
        private readonly Queue<PacketInfoData> _queue = new Queue<PacketInfoData>();
        private readonly int _objectCount = 0;
        /// Maximum amount of time for record processing to compute the best fit.
        /// MaxValue: all the objects.
        /// A positive timespan: use all objects that have been processed within the timeframe.
        private readonly TimeSpan _groupingDuration = TimeSpan.MinValue;
        private Stopwatch _groupingTimer = null;
        /// Notification callback to be called when we have accumulated enough
        /// data to compute a hint.
        private readonly FormattedObjectsCache.ProcessCachedGroupNotification _notificationCallBack = null;
        /// Reference kept to be used during notification.
        private FormatStartData _formatStartData = null;
        /// State flag to signal we are queuing.
        private bool _processingGroup = false;
        /// Current object count.
        private int _currentObjectCount = 0;
    /// Facade class managing the front end and the autosize cache.
    internal sealed class FormattedObjectsCache
        /// Delegate to allow notifications when the autosize queue is about to be drained.
        /// <param name="formatStartData">Current Fs control message.</param>
        /// <param name="objects">Enumeration of PacketInfoData objects.</param>
        internal delegate void ProcessCachedGroupNotification(FormatStartData formatStartData, List<PacketInfoData> objects);
        /// Decide right away if we need a front end cache (e.g. printing)
        /// <param name="cacheFrontEnd">If true, create a front end cache object.</param>
        internal FormattedObjectsCache(bool cacheFrontEnd)
            if (cacheFrontEnd)
                _frontEndQueue = new Queue<PacketInfoData>();
        /// If needed, add a back end autosize (grouping) cache.
        internal void EnableGroupCaching(ProcessCachedGroupNotification callBack, int objectCount)
            if (callBack != null)
                _groupQueue = new OutputGroupQueue(callBack, objectCount);
        internal void EnableGroupCaching(ProcessCachedGroupNotification callBack, TimeSpan groupingDuration)
                _groupQueue = new OutputGroupQueue(callBack, groupingDuration);
        /// Add an object to the cache. the behavior depends on the object added, the
        /// objects already in the cache and the cache settings.
        /// <returns>List of objects the cache is flushing.</returns>
            // if neither there, pass thru
            if (_frontEndQueue == null && _groupQueue == null)
                retVal.Add(o);
            // if front present, add to front
            if (_frontEndQueue != null)
                _frontEndQueue.Enqueue(o);
            // if back only, add to back
            return _groupQueue.Add(o);
        /// Remove all the objects from the cache.
        /// <returns>All the objects that were in the cache.</returns>
        internal List<PacketInfoData> Drain()
            // if neither there,we did not cache at all
                if (_groupQueue == null)
                    // drain the front queue and return the data
                    while (_frontEndQueue.Count > 0)
                        retVal.Add(_frontEndQueue.Dequeue());
                // move from the front to the back queue
                    List<PacketInfoData> groupQueueOut = _groupQueue.Add(_frontEndQueue.Dequeue());
                    if (groupQueueOut != null)
                        foreach (PacketInfoData x in groupQueueOut)
                            retVal.Add(x);
            // drain the back queue
                PacketInfoData obj = _groupQueue.Dequeue();
                retVal.Add(obj);
        /// Front end queue (if present, cache ALL, if not, bypass)
        private readonly Queue<PacketInfoData> _frontEndQueue;
        /// Back end grouping queue.
        private OutputGroupQueue _groupQueue = null;

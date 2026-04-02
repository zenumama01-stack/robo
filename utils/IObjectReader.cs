    /// PipelineReader provides asynchronous access to the stream of objects emitted by
    /// a <see cref="System.Management.Automation.Runspaces.Pipeline"/>.
    /// <seealso cref="System.Management.Automation.Runspaces.Pipeline.Output"/>
    /// <seealso cref="System.Management.Automation.Runspaces.Pipeline.Error"/>
    public abstract class PipelineReader<T>
        /// Event fired when data is added to the buffer.
        public abstract event EventHandler DataReady;
        /// Signaled when data is available.
        public abstract WaitHandle WaitHandle
        /// Check if the stream is closed and contains no data.
        /// <value>True if the stream is closed and contains no data, otherwise false</value>
        /// Attempting to read from the underlying stream if EndOfPipeline is true returns
        /// zero objects.
        public abstract bool EndOfPipeline
        /// <value>true if the underlying stream is open, otherwise false</value>
        /// The underlying stream may be readable after it is closed if data remains in the
        /// internal buffer. Check <see cref="EndOfPipeline"/> to determine if
        /// the underlying stream is closed and contains no data.
        public abstract bool IsOpen
        /// Returns the number of objects currently available in the underlying stream.
        public abstract int Count
        public abstract int MaxCapacity
        /// a write operation to throw an PipelineClosedException.
        /// The stream is already disposed
        /// Read at most <paramref name="count"/> objects.
        /// <param name="count">The maximum number of objects to read.</param>
        /// <returns>The objects read.</returns>
        /// This method blocks if the number of objects in the stream is less than <paramref name="count"/>
        /// and the stream is not closed.
        public abstract Collection<T> Read(int count);
        /// Read a single object from the stream.
        /// <returns>The next object in the stream.</returns>
        /// <remarks>This method blocks if the stream is empty</remarks>
        public abstract T Read();
        /// Blocks until the pipeline closes and reads all objects.
        /// <returns>A collection of zero or more objects.</returns>
        /// If the stream is empty, an empty collection is returned.
        public abstract Collection<T> ReadToEnd();
        /// Reads all objects currently in the stream, but does not block.
        /// This method performs a read of all objects currently in the
        /// stream.  If there are no objects in the stream,
        /// an empty collection is returned.
        public abstract Collection<T> NonBlockingRead();
        // 892370-2003/10/29-JonN added this method
        /// Reads objects currently in the stream, but does not block.
        /// This method performs a read of objects currently in the
        /// <param name="maxRequested">
        /// Return no more than maxRequested objects.
        public abstract Collection<T> NonBlockingRead(int maxRequested);
        /// Peek the next object, but do not remove it from the stream.  Non-blocking.
        /// The next object in the stream or AutomationNull.Value if the stream is empty
        /// <exception cref="PipelineClosedException">The stream is closed.</exception>
        public abstract T Peek();
        #region IEnumerable<T> Members
        /// Returns an enumerator that reads the items in the pipeline.
        internal IEnumerator<T> GetReadEnumerator()
            while (!this.EndOfPipeline)
                T t = this.Read();
                if (object.Equals(t, System.Management.Automation.Internal.AutomationNull.Value))

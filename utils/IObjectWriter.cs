    /// PipelineWriter allows the caller to provide an asynchronous stream of objects
    /// as input to a <see cref="System.Management.Automation.Runspaces.Pipeline"/>.
    public abstract class PipelineWriter
        /// Signaled when buffer space is available in the underlying stream.
        /// a <see cref="PipelineClosedException"/>.
        /// Returns the number of objects currently in the underlying stream.
        /// Flush the buffered data from the stream.  Closed streams may be flushed,
        public abstract void Flush();
        /// The underlying stream is already closed
        public abstract int Write(object obj);
        /// Write multiple objects to the underlying stream.
        /// If the enumeration contains elements equal to
        /// AutomationNull.Value, they are ignored.
        /// This can cause the return value to be less than the size of
        public abstract int Write(object obj, bool enumerateCollection);
    internal class DiscardingPipelineWriter : PipelineWriter
        private readonly ManualResetEvent _waitHandle = new ManualResetEvent(true);
            get { return _waitHandle; }
            get { return _isOpen; }
        private int _count = 0;
            get { return _count; }
            const int numberOfObjectsWritten = 1;
            _count += numberOfObjectsWritten;
            return numberOfObjectsWritten;
                return this.Write(obj);
            int numberOfObjectsWritten = 0;
                    numberOfObjectsWritten++;

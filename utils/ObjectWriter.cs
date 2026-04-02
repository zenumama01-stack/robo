    /// A ObjectWriter for an ObjectStream.
    internal class ObjectWriter : PipelineWriter
        /// <param name="stream">The stream to write.</param>
        public ObjectWriter([In, Out] ObjectStreamBase stream)
            stream.WriteReady += new EventHandler (this.OnWriteReady);
        /// Waitable handle for caller's to block until buffer space is available in the underlying stream.
                return _stream.WriteHandle;
            // 2003/09/02-JonN I removed setting _stream
            // to null, now all of the tests for null can come out.
        /// The underlying stream is disposed
            return _stream.Write(obj);
            return _stream.Write(obj, enumerateCollection);
        /// Handle WriteReady events from the underlying stream.
        private void OnWriteReady (object sender, EventArgs args)
            if (WriteReady != null)
                // are expecting an PipelineWriter
                WriteReady (this, args);
        private readonly ObjectStreamBase _stream;
    /// A ObjectWriter for a PSDataCollection ObjectStream.
    /// PSDataCollection is introduced after 1.0. PSDataCollection
    /// is used to store data from the last command in
    /// the pipeline and hence the writer will not
    /// support certain features like Flush().
    internal class PSDataCollectionWriter<T> : ObjectWriter
        /// Construct with an existing PSDataCollectionStream.
        /// Thrown if the specified stream is null
        public PSDataCollectionWriter(PSDataCollectionStream<T> stream)

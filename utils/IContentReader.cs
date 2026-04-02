    #region IContentReader
    /// A Cmdlet provider that implements the IContentCmdletProvider interface must provide an
    /// object that implements this interface when GetContentReader() is called.
    /// The interface allows for reading content from an item.
    public interface IContentReader : IDisposable
        /// Reads the content from the item.
        /// The number of "blocks" of data to be read from the item.
        /// An array of the blocks of data read from the item.
        /// A "block" of content is provider specific.  For the file system
        /// a "block" may be considered a line of text, a byte, a character, or delimited string.
        /// The implementation of this method should break the content down into meaningful blocks
        /// that the user may want to manipulate individually. The number of blocks to return is
        /// indicated by the <paramref name="readCount"/> parameter.
        IList Read(long readCount);
        /// Moves the current "block" to be read to a position relative to a place
        /// in the reader.
        /// An offset of the number of blocks to seek from the origin.
        /// The place in the stream to start the seek from.
        /// The implementation of this method moves the content reader <paramref name="offset"/>
        /// number of blocks from the specified <paramref name="origin"/>. See <see cref="IContentReader.Read"/>
        /// for a description of what a block is.
        void Seek(long offset, SeekOrigin origin);
        /// Closes the reader. Further reads should fail if the reader
        /// has been closed.
        /// The implementation of this method should close any resources held open by the
        /// reader.
        void Close();
    #endregion IContentReader

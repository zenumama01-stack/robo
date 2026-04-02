    #region IContentWriter
    /// object that implements this interface when GetContentWriter() is called.
    /// The interface allows for writing content to an item.
    public interface IContentWriter : IDisposable
        /// Writes content to the item.
        /// An array of content "blocks" to be written to the item.
        /// The blocks of content that were successfully written to the item.
        /// a "block" may be considered a byte, a character, or delimited string.
        /// The implementation of this method should treat each element in the
        /// <paramref name="content"/> parameter as a block. Each additional
        /// call to this method should append any new values to the content
        /// writer's current location until <see cref="IContentWriter.Close"/> is called.
        IList Write(IList content);
        /// Moves the current "block" to be written to a position relative to a place
        /// in the writer.
        /// The implementation of this method moves the content writer <paramref name="offset"/>
        /// number of blocks from the specified <paramref name="origin"/>. See <see cref="System.Management.Automation.Provider.IContentWriter.Write(IList)"/>
        /// Closes the writer. Further writes should fail if the writer
        /// writer.
    #endregion IContentWriter

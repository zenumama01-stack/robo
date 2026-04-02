    /// A base class for the commands that write content (set-content, add-content)
    public class WriteContentCommandBase : PassThroughContentCommandBase
        /// The value of the content to set.
        [Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public object[] Value
                return _content;
                _content = value;
        /// The value of the content to be set.
        private object[] _content;
        #region private Data
        /// This bool is used to determine if the path
        /// parameter was specified on the command line or via the pipeline.
        private bool _pipingPaths;
        /// True if the content writers have been open.
        /// This is used in conjunction with pipingPaths
        /// to determine if the content writers need to
        /// be closed each time ProgressRecord is called.
        private bool _contentWritersOpen;
        #endregion private Data
        /// Determines if the paths are specified on the command line
        /// or being piped in.
                _pipingPaths = false;
                _pipingPaths = true;
        /// Appends the content to the specified item.
            CmdletProviderContext currentContext = GetCurrentContext();
            // Initialize the content
            _content ??= Array.Empty<object>();
            if (_pipingPaths)
                // Make sure to clean up the content writers that are already there
                if (contentStreams != null && contentStreams.Count > 0)
                    _contentWritersOpen = false;
            if (!_contentWritersOpen)
                // Since the paths are being pipelined in, we have
                // to get new content writers for the new paths
                string[] paths = GetAcceptedPaths(Path, currentContext);
                if (paths.Length > 0)
                    BeforeOpenStreams(paths);
                    contentStreams = GetContentWriters(paths, currentContext);
                    SeekContentPosition(contentStreams);
                _contentWritersOpen = true;
            // Now write the content to the item
                        IList result = null;
                            result = holder.Writer.Write(_content);
                                   "ProviderContentWriteError",
                                   SessionStateStrings.ProviderContentWriteError,
                        if (result != null && result.Count > 0 && PassThru)
                            WriteContentObject(result, result.Count, holder.PathInfo, currentContext);
                // Need to close all the writers if the paths are being pipelined
        /// Closes all the content writers.
        /// This method is called by the base class after getting the content writer
        /// from the provider. If the current position needs to be changed before writing
        /// the content, this method should be overridden to do that.
        internal virtual void SeekContentPosition(List<ContentHolder> contentHolders)
            // default does nothing.
        internal virtual void BeforeOpenStreams(string[] paths)
                return InvokeProvider.Content.GetContentWriterDynamicParameters(Path[0], context);
            return InvokeProvider.Content.GetContentWriterDynamicParameters(".", context);
        /// Gets the IContentWriters for the current path(s)
        /// An array of IContentWriters for the current path(s)
        internal List<ContentHolder> GetContentWriters(
            string[] writerPaths,
            Collection<PathInfo> pathInfos = ResolvePaths(writerPaths, true, false, currentCommandContext);
                Collection<IContentWriter> writers = null;
                    writers =
                        InvokeProvider.Content.GetWriter(
                if (writers != null && writers.Count > 0)
                    if (writers.Count == 1 && writers[0] != null)
                            new(pathInfo, null, writers[0]);
        /// Gets the list of paths accepted by the user.
        /// <param name="unfilteredPaths">The list of unfiltered paths.</param>
        /// <param name="currentContext">The current context.</param>
        /// <returns>The list of paths accepted by the user.</returns>
        private string[] GetAcceptedPaths(string[] unfilteredPaths, CmdletProviderContext currentContext)
            Collection<PathInfo> pathInfos = ResolvePaths(unfilteredPaths, true, false, currentContext);
            var paths = new List<string>();
                if (CallShouldProcess(pathInfo.Path))
                    paths.Add(pathInfo.Path);
            return paths.ToArray();

using System.Management.Automation.Provider;
    /// The base class for the */content commands.
    public class ContentCommandBase : CoreCommandWithCredentialsBase, IDisposable
                   Mandatory = true, ValueFromPipelineByPropertyName = true)]
                return Path;
                Path = value;
        /// Gets or sets the filter property.
        public override string Filter
            get { return base.Filter; }
            set { base.Filter = value; }
        /// Gets or sets the include property.
        public override string[] Include
            get { return base.Include; }
            set { base.Include = value; }
        /// Gets or sets the exclude property.
        public override string[] Exclude
            get { return base.Exclude; }
            set { base.Exclude = value; }
        /// Gets or sets the force property.
        /// Gives the provider guidance on how vigorous it should be about performing
        /// the operation. If true, the provider should do everything possible to perform
        /// the operation. If false, the provider should attempt the operation but allow
        /// even simple errors to terminate the operation.
        /// For example, if the user tries to copy a file to a path that already exists and
        /// the destination is read-only, if force is true, the provider should copy over
        /// the existing read-only file. If force is false, the provider should write an error.
        public override SwitchParameter Force
            get { return base.Force; }
            set { base.Force = value; }
        /// An array of content holder objects that contain the path information
        /// and content readers/writers for the item represented by the path information.
        internal List<ContentHolder> contentStreams = new();
        /// Wraps the content into a PSObject and adds context information as notes.
        /// <param name="content">
        /// The content being written out.
        /// <param name="readCount">
        /// The number of blocks that have been read so far.
        /// <param name="pathInfo">
        /// The context the content was retrieved from.
        /// The context the command is being run under.
        internal void WriteContentObject(object content, long readCount, PathInfo pathInfo, CmdletProviderContext context)
                content != null,
                "The caller should verify the content.");
                pathInfo != null,
                "The caller should verify the pathInfo.");
                context != null,
                "The caller should verify the context.");
            PSObject result = PSObject.AsPSObject(content);
                result != null,
                "A PSObject should always be constructed.");
            // Use the cached notes if the cache exists and the path is still the same
            PSNoteProperty note;
            if (_currentContentItem != null &&
                ((_currentContentItem.PathInfo == pathInfo) ||
                    string.Equals(
                        pathInfo.Path,
                        _currentContentItem.PathInfo.Path,
                        StringComparison.OrdinalIgnoreCase)))
                result = _currentContentItem.AttachNotes(result);
                // Generate a new cache item and cache the notes
                _currentContentItem = new ContentPathsCache(pathInfo);
                // Construct a provider qualified path as the Path note
                string psPath = pathInfo.Path;
                note = new PSNoteProperty("PSPath", psPath);
                result.Properties.Add(note, true);
                tracer.WriteLine("Attaching {0} = {1}", "PSPath", psPath);
                _currentContentItem.PSPath = psPath;
                    // Now get the parent path and child name
                    string parentPath = null;
                    if (pathInfo.Drive != null)
                        parentPath = SessionState.Path.ParseParent(pathInfo.Path, pathInfo.Drive.Root, context);
                        parentPath = SessionState.Path.ParseParent(pathInfo.Path, string.Empty, context);
                    note = new PSNoteProperty("PSParentPath", parentPath);
                    tracer.WriteLine("Attaching {0} = {1}", "PSParentPath", parentPath);
                    _currentContentItem.ParentPath = parentPath;
                    // Get the child name
                    string childName = SessionState.Path.ParseChildName(pathInfo.Path, context);
                    note = new PSNoteProperty("PSChildName", childName);
                    tracer.WriteLine("Attaching {0} = {1}", "PSChildName", childName);
                    _currentContentItem.ChildName = childName;
                    // Ignore. The object just won't have ParentPath or ChildName set.
                // PSDriveInfo
                    PSDriveInfo drive = pathInfo.Drive;
                    note = new PSNoteProperty("PSDrive", drive);
                    tracer.WriteLine("Attaching {0} = {1}", "PSDrive", drive);
                    _currentContentItem.Drive = drive;
                // ProviderInfo
                ProviderInfo provider = pathInfo.Provider;
                note = new PSNoteProperty("PSProvider", provider);
                tracer.WriteLine("Attaching {0} = {1}", "PSProvider", provider);
                _currentContentItem.Provider = provider;
            // Add the ReadCount note
            note = new PSNoteProperty("ReadCount", readCount);
            WriteObject(result);
        /// A cache of the notes that get added to the content items as they are written
        /// to the pipeline.
        private ContentPathsCache _currentContentItem;
        /// A class that stores a cache of the notes that get attached to content items
        /// as they get written to the pipeline. An instance of this cache class is
        /// only valid for a single path.
        internal sealed class ContentPathsCache
            /// Constructs a content cache item.
            /// The path information for which the cache will be bound.
            public ContentPathsCache(PathInfo pathInfo)
                PathInfo = pathInfo;
            /// The path information for the cached item.
            public PathInfo PathInfo { get; }
            /// The cached PSPath of the item.
            public string PSPath { get; set; }
            /// The cached parent path of the item.
            public string ParentPath { get; set; }
            /// The cached drive for the item.
            public PSDriveInfo Drive { get; set; }
            /// The cached provider of the item.
            public ProviderInfo Provider { get; set; }
            /// The cached child name of the item.
            public string ChildName { get; set; }
            /// Attaches the cached notes to the specified PSObject.
            /// The PSObject to attached the cached notes to.
            /// The PSObject that was passed in with the cached notes added.
            public PSObject AttachNotes(PSObject content)
                PSNoteProperty note = new("PSPath", PSPath);
                content.Properties.Add(note, true);
                tracer.WriteLine("Attaching {0} = {1}", "PSPath", PSPath);
                // Now attach the parent path and child name
                note = new PSNoteProperty("PSParentPath", ParentPath);
                tracer.WriteLine("Attaching {0} = {1}", "PSParentPath", ParentPath);
                // Attach the child name
                note = new PSNoteProperty("PSChildName", ChildName);
                tracer.WriteLine("Attaching {0} = {1}", "PSChildName", ChildName);
                if (PathInfo.Drive != null)
                    note = new PSNoteProperty("PSDrive", Drive);
                    tracer.WriteLine("Attaching {0} = {1}", "PSDrive", Drive);
                note = new PSNoteProperty("PSProvider", Provider);
                tracer.WriteLine("Attaching {0} = {1}", "PSProvider", Provider);
                return content;
        /// A struct to hold the path information and the content readers/writers
        /// for an item.
        internal readonly struct ContentHolder
            internal ContentHolder(
                PathInfo pathInfo,
                IContentReader reader,
                IContentWriter writer)
                if (pathInfo == null)
                    throw PSTraceSource.NewArgumentNullException(nameof(pathInfo));
                Reader = reader;
                Writer = writer;
            internal PathInfo PathInfo { get; }
            internal IContentReader Reader { get; }
            internal IContentWriter Writer { get; }
        /// Closes the content readers and writers in the content holder array.
        internal void CloseContent(List<ContentHolder> contentHolders, bool disposing)
            if (contentHolders == null)
                throw PSTraceSource.NewArgumentNullException(nameof(contentHolders));
                    holder.Writer?.Close();
                catch (Exception e) // Catch-all OK. 3rd party callout
                    // Catch all the exceptions caused by closing the writer
                    // and write out an error.
                            "ProviderContentCloseError",
                            SessionStateStrings.ProviderContentCloseError,
                    if (!disposing)
                                providerException.ErrorRecord,
                                providerException));
                    holder.Reader?.Close();
        /// Overridden by derived classes to support ShouldProcess with
        /// the appropriate information.
        /// The path to the item from which the content writer will be
        /// retrieved.
        internal virtual bool CallShouldProcess(string path)
        /// Gets the IContentReaders for the current path(s)
        /// An array of IContentReaders for the current path(s)
        internal List<ContentHolder> GetContentReaders(
            string[] readerPaths,
            CmdletProviderContext currentCommandContext)
            // Resolve all the paths into PathInfo objects
            Collection<PathInfo> pathInfos = ResolvePaths(readerPaths, false, true, currentCommandContext);
            // Create the results array
            List<ContentHolder> results = new();
            foreach (PathInfo pathInfo in pathInfos)
                // For each path, get the content writer
                Collection<IContentReader> readers = null;
                    string pathToProcess = WildcardPattern.Escape(pathInfo.Path);
                    if (currentCommandContext.SuppressWildcardExpansion)
                        pathToProcess = pathInfo.Path;
                    readers =
                        InvokeProvider.Content.GetReader(pathToProcess, currentCommandContext);
                if (readers != null && readers.Count > 0)
                    if (readers.Count == 1 && readers[0] != null)
                        ContentHolder holder =
                            new(pathInfo, readers[0], null);
                        results.Add(holder);
            return results;
        /// Resolves the specified paths to PathInfo objects.
        /// <param name="pathsToResolve">
        /// The paths to be resolved. Each path may contain glob characters.
        /// <param name="allowNonexistingPaths">
        /// If true, resolves the path even if it doesn't exist.
        /// <param name="allowEmptyResult">
        /// If true, allows a wildcard that returns no results.
        /// <param name="currentCommandContext">
        /// An array of PathInfo objects that are the resolved paths for the
        /// <paramref name="pathsToResolve"/> parameter.
        internal Collection<PathInfo> ResolvePaths(
            string[] pathsToResolve,
            bool allowNonexistingPaths,
            bool allowEmptyResult,
            Collection<PathInfo> results = new();
            foreach (string path in pathsToResolve)
                bool pathNotFound = false;
                bool filtersHidPath = false;
                ErrorRecord pathNotFoundErrorRecord = null;
                    // First resolve each of the paths
                    Collection<PathInfo> pathInfos =
                        SessionState.Path.GetResolvedPSPathFromPSPath(
                            currentCommandContext);
                    if (pathInfos.Count == 0)
                        pathNotFound = true;
                        // If the item simply did not exist,
                        // we would have got an ItemNotFoundException.
                        // If we get here, it's because the filters
                        // excluded the file.
                        if (!currentCommandContext.SuppressWildcardExpansion)
                            filtersHidPath = true;
                        results.Add(pathInfo);
                catch (ItemNotFoundException pathNotFoundException)
                    pathNotFoundErrorRecord = new ErrorRecord(pathNotFoundException.ErrorRecord, pathNotFoundException);
                if (pathNotFound)
                    if (allowNonexistingPaths &&
                        (!filtersHidPath) &&
                        (currentCommandContext.SuppressWildcardExpansion ||
                        (!WildcardPattern.ContainsWildcardCharacters(path))))
                        ProviderInfo provider = null;
                        PSDriveInfo drive = null;
                        string unresolvedPath =
                            SessionState.Path.GetUnresolvedProviderPathFromPSPath(
                                currentCommandContext,
                                out provider,
                                out drive);
                        PathInfo pathInfo =
                                drive,
                                provider,
                                unresolvedPath,
                                SessionState);
                        if (pathNotFoundErrorRecord == null)
                            // Detect if the path resolution failed to resolve to a file.
                            string error = StringUtil.Format(NavigationResources.ItemNotFound, Path);
                            Exception e = new(error);
                            pathNotFoundErrorRecord = new ErrorRecord(
                                "ItemNotFound",
                                Path);
                        WriteError(pathNotFoundErrorRecord);
        internal void Dispose(bool isDisposing)
                CloseContent(contentStreams, true);
                contentStreams = new List<ContentHolder>();
        /// Dispose method in IDisposable.
        #endregion IDisposable

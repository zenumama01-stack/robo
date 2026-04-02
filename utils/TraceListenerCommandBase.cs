    /// A base class for the trace cmdlets that allow you to specify
    /// which trace listeners to add to a TraceSource.
    public class TraceListenerCommandBase : TraceCommandBase
        internal string[] NameInternal { get; set; } = Array.Empty<string>();
        internal PSTraceSourceOptions OptionsInternal
                return _options;
                optionsSpecified = true;
        private PSTraceSourceOptions _options = PSTraceSourceOptions.All;
        /// True if the Options parameter has been set, or false otherwise.
        internal bool optionsSpecified;
        internal TraceOptions ListenerOptionsInternal
                return _traceOptions;
                traceOptionsSpecified = true;
                _traceOptions = value;
        private TraceOptions _traceOptions = TraceOptions.None;
        /// True if the TraceOptions parameter was specified, or false otherwise.
        internal bool traceOptionsSpecified;
        internal string FileListener { get; set; }
        /// Property that sets force parameter.  This will clear the
        /// read-only attribute on an existing file if present.
        /// Note that we do not attempt to reset the read-only attribute.
        public bool ForceWrite { get; set; }
        internal bool DebuggerListener { get; set; }
        internal SwitchParameter PSHostListener
            get { return _host; }
            set { _host = value; }
        private bool _host = false;
        internal Collection<PSTraceSource> ConfigureTraceSource(
            string[] sourceNames,
            bool preConfigure,
            out Collection<PSTraceSource> preconfiguredSources)
            preconfiguredSources = new Collection<PSTraceSource>();
            // Find the matching and unmatched trace sources.
            Collection<string> notMatched = null;
            Collection<PSTraceSource> matchingSources = GetMatchingTraceSource(sourceNames, false, out notMatched);
            if (preConfigure)
                // Set the flags if they were specified
                if (optionsSpecified)
                    SetFlags(matchingSources);
                AddTraceListenersToSources(matchingSources);
                SetTraceListenerOptions(matchingSources);
            // Now try to preset options for sources which have not yet been
            // constructed.
            foreach (string notMatchedName in notMatched)
                if (string.IsNullOrEmpty(notMatchedName))
                if (WildcardPattern.ContainsWildcardCharacters(notMatchedName))
                PSTraceSource newTraceSource =
                    PSTraceSource.GetNewTraceSource(
                        notMatchedName,
                preconfiguredSources.Add(newTraceSource);
            // Preconfigure any trace sources that were not already present
            if (preconfiguredSources.Count > 0)
                        SetFlags(preconfiguredSources);
                    AddTraceListenersToSources(preconfiguredSources);
                    SetTraceListenerOptions(preconfiguredSources);
                // Add the sources to the preconfigured table so that they are found
                // when the trace source finally gets created by the system.
                foreach (PSTraceSource sourceToPreconfigure in preconfiguredSources)
                    if (!PSTraceSource.PreConfiguredTraceSource.ContainsKey(sourceToPreconfigure.Name))
                        PSTraceSource.PreConfiguredTraceSource.Add(sourceToPreconfigure.Name, sourceToPreconfigure);
            return matchingSources;
        #region AddTraceListeners
        /// Adds the console, debugger, file, or host listener if requested.
        internal void AddTraceListenersToSources(Collection<PSTraceSource> matchingSources)
            if (DebuggerListener)
                if (_defaultListener == null)
                    _defaultListener =
                        new DefaultTraceListener();
                    // Note, this is not meant to be localized.
                    _defaultListener.Name = "Debug";
                AddListenerToSources(matchingSources, _defaultListener);
            if (PSHostListener)
                if (_hostListener == null)
                    ((MshCommandRuntime)this.CommandRuntime).DebugPreference = ActionPreference.Continue;
                    _hostListener = new PSHostTraceListener(this);
                    _hostListener.Name = "Host";
                AddListenerToSources(matchingSources, _hostListener);
            if (FileListener != null)
                if (_fileListeners == null)
                    _fileListeners = new Collection<TextWriterTraceListener>();
                    FileStreams = new Collection<FileStream>();
                    Exception error = null;
                        Collection<string> resolvedPaths = new();
                            // Resolve the file path
                            resolvedPaths = this.SessionState.Path.GetResolvedProviderPathFromPSPath(FileListener, out provider);
                                        StringUtil.Format(TraceCommandStrings.TraceFileOnly,
                                            FileListener,
                                            provider.FullName));
                            // Since the file wasn't found, just make a provider-qualified path out if it
                            // and use that.
                            PSDriveInfo driveInfo = null;
                                this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(
                                    new CmdletProviderContext(this.Context),
                                    out driveInfo);
                        if (resolvedPaths.Count > 1)
                                new PSNotSupportedException(StringUtil.Format(TraceCommandStrings.TraceSingleFileOnly, FileListener));
                        string resolvedPath = resolvedPaths[0];
                        Exception fileOpenError = null;
                            if (ForceWrite && System.IO.File.Exists(resolvedPath))
                                // remove readonly attributes on the file
                                System.IO.FileInfo fInfo = new(resolvedPath);
                                if (fInfo != null)
                                    // Save some disk write time by checking whether file is readonly..
                                    if ((fInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                                        // Make sure the file is not read only
                                        fInfo.Attributes &= ~(FileAttributes.ReadOnly);
                            // Trace commands always append..So there is no need to set overwrite with force..
                            FileStream fileStream = new(resolvedPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                            FileStreams.Add(fileStream);
                            // Open the file stream
                            TextWriterTraceListener fileListener =
                                    new(fileStream, resolvedPath);
                            fileListener.Name = FileListener;
                            _fileListeners.Add(fileListener);
                            fileOpenError = ioException;
                            fileOpenError = securityException;
                        catch (UnauthorizedAccessException unauthorized)
                            fileOpenError = unauthorized;
                        if (fileOpenError != null)
                                    fileOpenError,
                                    "FileListenerPathResolutionFailed",
                                    resolvedPath);
                        error = providerNotFound;
                        error = driveNotFound;
                        error = notSupported;
                                error,
                                FileListener);
                foreach (TraceListener listener in _fileListeners)
                    AddListenerToSources(matchingSources, listener);
        private DefaultTraceListener _defaultListener;
        private PSHostTraceListener _hostListener;
        private Collection<TextWriterTraceListener> _fileListeners;
        /// The file streams that were open by this command.
        internal Collection<FileStream> FileStreams { get; private set; }
        private static void AddListenerToSources(Collection<PSTraceSource> matchingSources, TraceListener listener)
            // Now add the listener to all the sources
            foreach (PSTraceSource source in matchingSources)
                source.Listeners.Add(listener);
        #endregion AddTraceListeners
        #region RemoveTraceListeners
        /// Removes the tracelisteners from the specified trace sources.
        internal static void RemoveListenersByName(
            Collection<PSTraceSource> matchingSources,
            string[] listenerNames,
            bool fileListenersOnly)
            Collection<WildcardPattern> listenerMatcher =
                    listenerNames,
            // Loop through all the matching sources and remove the matching listeners
                // Get the indexes of the listeners that need to be removed.
                // This is done because we cannot remove the listeners while
                // we are enumerating them.
                for (int index = source.Listeners.Count - 1; index >= 0; --index)
                    TraceListener listenerToRemove = source.Listeners[index];
                    if (fileListenersOnly && listenerToRemove is not TextWriterTraceListener)
                        // Since we only want to remove file listeners, skip any that
                        // aren't file listeners
                    // Now match the names
                    if (SessionStateUtilities.MatchesAnyWildcardPattern(
                            listenerToRemove.Name,
                            listenerMatcher,
                            true))
                        listenerToRemove.Flush();
                        listenerToRemove.Dispose();
                        source.Listeners.RemoveAt(index);
        #endregion RemoveTraceListeners
        #region SetTraceListenerOptions
        /// Sets the trace listener options based on the ListenerOptions parameter.
        internal void SetTraceListenerOptions(Collection<PSTraceSource> matchingSources)
            // Set the trace options if they were specified
            if (traceOptionsSpecified)
                    foreach (TraceListener listener in source.Listeners)
                        listener.TraceOutputOptions = this.ListenerOptionsInternal;
        #endregion SetTraceListenerOptions
        #region SetFlags
        /// Sets the flags for all the specified TraceSources.
        internal void SetFlags(Collection<PSTraceSource> matchingSources)
            foreach (PSTraceSource structuredSource in matchingSources)
                structuredSource.Options = this.OptionsInternal;
        #endregion SetFlags
        #region TurnOnTracing
        /// Turns on tracing for the TraceSources, flags, and listeners defined by the parameters.
        internal void TurnOnTracing(Collection<PSTraceSource> matchingSources, bool preConfigured)
                // Store the current state of the TraceSource
                if (!_storedTraceSourceState.ContainsKey(source))
                    // Copy the listeners into a different collection
                    Collection<TraceListener> listenerCollection = new();
                        listenerCollection.Add(listener);
                    if (preConfigured)
                        // If the source is a preconfigured source, then the default options
                        // and listeners should be stored as the existing state.
                        _storedTraceSourceState[source] =
                            new KeyValuePair<PSTraceSourceOptions, Collection<TraceListener>>(
                                PSTraceSourceOptions.None,
                                new Collection<TraceListener>());
                                source.Options,
                                listenerCollection);
                // Now set the new flags
                source.Options = this.OptionsInternal;
            // Now turn on the listeners
        #endregion TurnOnTracing
        #region ResetTracing
        /// Resets tracing to the previous level for the TraceSources defined by the parameters.
        /// Note, TurnOnTracing must be called before calling ResetTracing or else all
        /// TraceSources will be turned off.
        internal void ResetTracing(Collection<PSTraceSource> matchingSources)
                // First flush all the existing trace listeners
                    listener.Flush();
                if (_storedTraceSourceState.ContainsKey(source))
                    // Restore the TraceSource to its original state
                    KeyValuePair<PSTraceSourceOptions, Collection<TraceListener>> storedState =
                        _storedTraceSourceState[source];
                    source.Listeners.Clear();
                    foreach (TraceListener listener in storedState.Value)
                    source.Options = storedState.Key;
                    // Since we don't have any stored state for this TraceSource,
                    // just turn it off.
                    source.Options = PSTraceSourceOptions.None;
        #endregion ResetTracing
        #region stored state
        /// Clears the store TraceSource state.
        protected void ClearStoredState()
            // First close all listeners
            foreach (KeyValuePair<PSTraceSourceOptions, Collection<TraceListener>> pair in _storedTraceSourceState.Values)
                foreach (TraceListener listener in pair.Value)
                    listener.Dispose();
            _storedTraceSourceState.Clear();
        private readonly Dictionary<PSTraceSource, KeyValuePair<PSTraceSourceOptions, Collection<TraceListener>>> _storedTraceSourceState =
        #endregion stored state

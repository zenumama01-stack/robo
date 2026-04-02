    /// The ProviderContext class.
    internal class ProviderContext
        /// Requested path.
        private readonly string _requestedPath;
        private readonly PathIntrinsics _pathIntrinsics;
        internal string RequestedPath
                return _requestedPath;
        /// Create a new instance of ProviderContext.
        internal ProviderContext(
            string requestedPath,
            PathIntrinsics pathIntrinsics)
            Dbg.Assert(executionContext != null, "ExecutionContext cannot be null.");
            _requestedPath = requestedPath;
            _executionContext = executionContext;
            _pathIntrinsics = pathIntrinsics;
        /// Get provider specific help info.
        internal MamlCommandHelpInfo GetProviderSpecificHelpInfo(string helpItemName)
                // By returning null, we force get-help to return generic help
            // Get the provider.
            string resolvedProviderPath = null;
            CmdletProviderContext cmdletProviderContext = new CmdletProviderContext(_executionContext);
                string psPath = _requestedPath;
                if (string.IsNullOrEmpty(_requestedPath))
                    psPath = _pathIntrinsics.CurrentLocation.Path;
                resolvedProviderPath = _executionContext.LocationGlobber.GetProviderPath(
                     psPath,
                     cmdletProviderContext,
                     out providerInfo,
            // ignore exceptions caused by provider resolution
            // Does the provider know how to generate MAML.
            CmdletProvider cmdletProvider = providerInfo.CreateInstance();
            if (!(cmdletProvider is ICmdletProviderSupportsHelp provider))
                // Under JEA sessions the resolvedProviderPath will be null, we should allow get-help to continue.
            bool isJEASession = false;
            if (this._executionContext.InitialSessionState != null && this._executionContext.InitialSessionState.Providers != null && providerInfo != null)
                    Runspaces.SessionStateProviderEntry sessionStateProvider in
                        this._executionContext.InitialSessionState.Providers[providerInfo.Name])
                    if (sessionStateProvider.Visibility == SessionStateEntryVisibility.Private)
                        isJEASession = true;
                if (isJEASession)
                    throw new ItemNotFoundException(_requestedPath, "PathNotFound", SessionStateStrings.PathNotFound);
            // ok we have path and valid provider that supplies content..initialize the provider
            // and get the help content for the path.
            cmdletProvider.Start(providerInfo, cmdletProviderContext);
            // There should be exactly one resolved path.
            string providerPath = resolvedProviderPath;
            // Get the MAML help info. Don't catch exceptions thrown by provider.
            string mamlXmlString = provider.GetHelpMaml(helpItemName, providerPath);
            if (string.IsNullOrEmpty(mamlXmlString))
            // process the MAML content only if it is non-empty.
            XmlDocument mamlDoc = InternalDeserializer.LoadUnsafeXmlDocument(
                mamlXmlString,
            MamlCommandHelpInfo providerSpecificHelpInfo = MamlCommandHelpInfo.Load(mamlDoc.DocumentElement, HelpCategory.Provider);
            return providerSpecificHelpInfo;

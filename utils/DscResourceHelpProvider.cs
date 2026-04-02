    internal class DscResourceHelpProvider : HelpProviderWithCache
        /// Constructor for DscResourceHelpProvider.
        internal DscResourceHelpProvider(HelpSystem helpSystem)
        /// Execution context of the HelpSystem.
        [TraceSource("DscResourceHelpProvider", "DscResourceHelpProvider")]
        private static readonly PSTraceSource s_tracer = PSTraceSource.GetTracer("DscResourceHelpProvider", "DscResourceHelpProvider");
        #region common properties
        /// Name of the Help Provider.
            get { return "Dsc Resource Help Provider"; }
        /// Supported Help Categories.
            get { return Automation.HelpCategory.DscResource; }
        /// Override SearchHelp to find a dsc resource help matching a pattern.
        /// <param name="helpRequest">Help request.</param>
        /// <param name="searchOnlyContent">Not used.</param>
            Debug.Assert(helpRequest != null, "helpRequest cannot be null.");
                DscResourceSearcher searcher = new DscResourceSearcher(pattern, _context);
                foreach (var helpInfo in GetHelpInfo(searcher))
        /// Override ExactMatchHelp to find the matching DscResource matching help request.
        /// <param name="helpRequest">Help Request for the search.</param>
        /// <returns>Enumerable of HelpInfo objects.</returns>
            if ((helpRequest.HelpCategory & Automation.HelpCategory.DscResource) == 0)
                yield return null;
            DscResourceSearcher searcher = new DscResourceSearcher(target, _context);
        /// Get the help in for the DscResource Info.        ///
        /// <param name="searcher">Searcher for DscResources.</param>
        /// <returns>Next HelpInfo object.</returns>
        private IEnumerable<HelpInfo> GetHelpInfo(DscResourceSearcher searcher)
                DscResourceInfo current = ((IEnumerator<DscResourceInfo>)searcher).Current;
                string moduleDir = current.ParentPath;
                // for binary modules, current.Module is empty.
                // in such cases use the leaf folder of ParentPath as filename.
                if (current.Module != null)
                    moduleName = current.Module.Name;
                else if (!string.IsNullOrEmpty(moduleDir))
                    string[] splitPath = moduleDir.Split('\\');
                    moduleName = splitPath[splitPath.Length - 1];
                    string helpFileName = null;
                    HelpInfo helpInfo = GetHelpInfoFromHelpFile(current, helpFileToFind, searchPaths, true, out helpFileName);
        /// <param name="helpFile">File name.</param>
        /// <param name="helpItemsNode">Nodes to check.</param>
            Debug.Assert(!string.IsNullOrEmpty(helpFile), "helpFile cannot be null.");
        private HelpInfo GetHelpInfoFromHelpFile(DscResourceInfo resourceInfo, string helpFileToFind, Collection<string> searchPaths, bool reportErrors, out string helpFile)
            Dbg.Assert(resourceInfo != null, "Caller should verify that resourceInfo != null");
            if (!File.Exists(helpFile))
                // Load the help file only once. Then use it from the cache.
                    LoadHelpFile(helpFile, helpFile, resourceInfo.Name, reportErrors);
                return GetFromResourceHelpCache(helpFile, Automation.HelpCategory.DscResource);
        /// <param name="helpCategory">Help Category for search.</param>
        private HelpInfo GetFromResourceHelpCache(string helpFileIdentifier, HelpCategory helpCategory)
            Debug.Assert(!string.IsNullOrEmpty(helpFileIdentifier), "helpFileIdentifier should not be null or empty.");
            HelpInfo result = GetCache(helpFileIdentifier);
                s_tracer.WriteLine("Error occurred in DscResourceHelpProvider {0}", e.Message);
            Dbg.Assert(!string.IsNullOrEmpty(helpFile), "HelpFile cannot be null or empty.");
            Dbg.Assert(!string.IsNullOrEmpty(helpFileIdentifier), "helpFileIdentifier cannot be null or empty.");
                        string nodeLocalName = node.LocalName;
                        bool isDscResource = (string.Equals(nodeLocalName, "dscResource", StringComparison.OrdinalIgnoreCase));
                        if (node.NodeType == XmlNodeType.Element && isDscResource)
                                if (isDscResource)
                                    helpInfo = MamlCommandHelpInfo.Load(node, HelpCategory.DscResource);
                                AddCache(helpFileIdentifier, helpInfo);

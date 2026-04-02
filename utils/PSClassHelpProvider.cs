    internal class PSClassHelpProvider : HelpProviderWithCache
        /// Constructor for PSClassHelpProvider.
        internal PSClassHelpProvider(HelpSystem helpSystem)
        [TraceSource("PSClassHelpProvider", "PSClassHelpProvider")]
        private static readonly PSTraceSource s_tracer = PSTraceSource.GetTracer("PSClassHelpProvider", "PSClassHelpProvider");
            get { return "Powershell Class Help Provider"; }
            get { return Automation.HelpCategory.Class; }
        /// Override SearchHelp to find a class module with help matching a pattern.
                PSClassSearcher searcher = new PSClassSearcher(pattern, useWildCards: true, _context);
        /// Override ExactMatchHelp to find the matching class module matching help request.
            if ((helpRequest.HelpCategory & Automation.HelpCategory.Class) == 0)
            PSClassSearcher searcher = new PSClassSearcher(helpRequest.Target, useWildCards: false, _context);
        /// Get the help in for the PS Class Info.        ///
        /// <param name="searcher">Searcher for PS Classes.</param>
        private IEnumerable<HelpInfo> GetHelpInfo(PSClassSearcher searcher)
                PSClassInfo current = ((IEnumerator<PSClassInfo>)searcher).Current;
                string moduleName = current.Module.Name;
                string moduleDir = current.Module.ModuleBase;
                    string externalHelpFile = current.HelpFile;
                    if (!string.IsNullOrEmpty(externalHelpFile))
                        FileInfo helpFileInfo = new FileInfo(externalHelpFile);
                        DirectoryInfo dirToSearch = helpFileInfo.Directory;
                        if (dirToSearch.Exists)
                            searchPaths.Add(dirToSearch.FullName);
                            helpFileToFind = helpFileInfo.Name; // If external help file is specified. Then use it.
        private HelpInfo GetHelpInfoFromHelpFile(PSClassInfo classInfo, string helpFileToFind, Collection<string> searchPaths, bool reportErrors, out string helpFile)
            Dbg.Assert(classInfo != null, "Caller should verify that classInfo != null");
                    LoadHelpFile(helpFile, helpFile, classInfo.Name, reportErrors);
                return GetFromPSClassHelpCache(helpFile, Automation.HelpCategory.Class);
        private HelpInfo GetFromPSClassHelpCache(string helpFileIdentifier, HelpCategory helpCategory)
                MamlClassHelpInfo original = (MamlClassHelpInfo)result;
                s_tracer.WriteLine("Error occurred in PSClassHelpProvider {0}", e.Message);
                        bool isClass = (string.Equals(nodeLocalName, "class", StringComparison.OrdinalIgnoreCase));
                        if (node.NodeType == XmlNodeType.Element && isClass)
                            MamlClassHelpInfo helpInfo = null;
                                if (isClass)
                                    helpInfo = MamlClassHelpInfo.Load(node, HelpCategory.Class);

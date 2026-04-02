    /// Searcher class for finding PS classes on the system.
    internal class PSClassSearcher : IEnumerable<PSClassInfo>, IEnumerator<PSClassInfo>
        internal PSClassSearcher(
            bool useWildCards,
            Diagnostics.Assert(className != null, "caller to verify className is not null");
            _useWildCards = useWildCards;
            _moduleInfoCache = new Dictionary<string, PSModuleInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly string _className = null;
        private PSClassInfo _currentMatch = null;
        private IEnumerator<PSClassInfo> _matchingClass = null;
        private Collection<PSClassInfo> _matchingClassList = null;
        private readonly bool _useWildCards = false;
        private readonly Dictionary<string, PSModuleInfo> _moduleInfoCache = null;
            _matchingClass = null;
        IEnumerator<PSClassInfo> IEnumerable<PSClassInfo>.GetEnumerator()
            _currentMatch = GetNextClass();
        /// Return the current PSClassInfo.
        PSClassInfo IEnumerator<PSClassInfo>.Current
        /// Return the current PSClassInfo as object.
                return ((IEnumerator<PSClassInfo>)this).Current;
        /// Get all modules and find the matching type
        /// <returns>Next PSClassInfo object or null if none are found.</returns>
        private PSClassInfo GetNextClass()
            PSClassInfo returnValue = null;
            WildcardPattern classNameMatcher = WildcardPattern.Get(_className, WildcardOptions.IgnoreCase);
            if (_matchingClassList == null)
                _matchingClassList = new Collection<PSClassInfo>();
                if (FindTypeByModulePath(classNameMatcher))
                    _matchingClass = _matchingClassList.GetEnumerator();
            if (!_matchingClass.MoveNext())
                returnValue = _matchingClass.Current;
        private bool FindTypeByModulePath(WildcardPattern classNameMatcher)
            var moduleList = ModuleUtils.GetDefaultAvailableModuleFiles(isForAutoDiscovery: false, _context);
            foreach (var modulePath in moduleList)
                var cachedClasses = AnalysisCache.GetExportedClasses(expandedModulePath, _context);
                if (cachedClasses != null)
                    // Exact match
                    if (!_useWildCards)
                        if (cachedClasses.ContainsKey(_className))
                            var classInfo = CachedItemToPSClassInfo(classNameMatcher, modulePath);
                            if (classInfo != null)
                                _matchingClassList.Add(classInfo);
                        foreach (var className in cachedClasses.Keys)
                            if (classNameMatcher.IsMatch(className))
            return matchFound;
        /// Convert the cacheItem to a PSClassInfo object.
        /// For this, we call Get-Module -List with module name.
        /// <param name="classNameMatcher">Wildcard pattern matcher for comparing class name.</param>
        /// <param name="modulePath">Path to the module where the class is defined.</param>
        /// <returns>Converted PSClassInfo object.</returns>
        private PSClassInfo CachedItemToPSClassInfo(WildcardPattern classNameMatcher, string modulePath)
            foreach (var module in GetPSModuleInfo(modulePath))
                var exportedTypes = module.GetExportedTypeDefinitions();
                ScriptBlockAst ast = null;
                TypeDefinitionAst typeAst = null;
                    if (exportedTypes.TryGetValue(_className, out typeAst))
                        ast = typeAst.Parent.Parent as ScriptBlockAst;
                        if (ast != null)
                            return ConvertToClassInfo(module, ast, typeAst);
                    foreach (var exportedType in exportedTypes)
                        if (exportedType.Value != null &&
                            classNameMatcher.IsMatch(exportedType.Value.Name) &&
                            exportedType.Value.IsClass)
                            ast = exportedType.Value.Parent.Parent as ScriptBlockAst;
                                return ConvertToClassInfo(module, ast, exportedType.Value);
        private Collection<PSModuleInfo> GetPSModuleInfo(string modulePath)
            PSModuleInfo moduleInfo = null;
                _moduleInfoCache.TryGetValue(modulePath, out moduleInfo);
                var returnValue = new Collection<PSModuleInfo>();
                returnValue.Add(moduleInfo);
            CommandInfo commandInfo = new CmdletInfo("Get-Module", typeof(Microsoft.PowerShell.Commands.GetModuleCommand), null, null, _context);
            System.Management.Automation.Runspaces.Command getModuleCommand = new System.Management.Automation.Runspaces.Command(commandInfo);
            string moduleName = Path.GetFileNameWithoutExtension(modulePath);
            var modules = System.Management.Automation.PowerShell.Create(RunspaceMode.CurrentRunspace)
                .AddCommand(getModuleCommand)
                    .AddParameter("List", true)
                    .AddParameter("Debug", false)
                    .Invoke<PSModuleInfo>();
                foreach (var module in modules)
                    _moduleInfoCache.Add(module.Path, module);
            return modules;
        private static PSClassInfo ConvertToClassInfo(PSModuleInfo module, ScriptBlockAst ast, TypeDefinitionAst statement)
            PSClassInfo classInfo = new PSClassInfo(statement.Name);
            Dbg.Assert(statement.Name != null, "statement should have a name.");
            classInfo.Module = module;
            Collection<PSClassMemberInfo> properties = new Collection<PSClassMemberInfo>();
            foreach (var member in statement.Members)
                if (member is PropertyMemberAst propAst && !propAst.PropertyAttributes.HasFlag(PropertyAttributes.Hidden))
                    Dbg.Assert(propAst.Name != null, "PropName cannot be null");
                    Dbg.Assert(propAst.PropertyType != null, "PropertyType cannot be null");
                    Dbg.Assert(propAst.PropertyType.TypeName != null, "Property TypeName cannot be null");
                    Dbg.Assert(propAst.Extent != null, "Property Extent cannot be null");
                    Dbg.Assert(propAst.Extent.Text != null, "Property ExtentText cannot be null");
                    PSClassMemberInfo classProperty = new PSClassMemberInfo(propAst.Name,
                                                                          propAst.PropertyType.TypeName.FullName,
                                                                          propAst.Extent.Text);
                    properties.Add(classProperty);
            classInfo.UpdateMembers(properties);
            string mamlHelpFile = null;
            if (ast.GetHelpContent() != null)
                mamlHelpFile = ast.GetHelpContent().MamlHelpFile;
            if (!string.IsNullOrEmpty(mamlHelpFile))
                classInfo.HelpFile = mamlHelpFile;
            return classInfo;

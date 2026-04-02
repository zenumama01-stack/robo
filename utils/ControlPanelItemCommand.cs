    /// Represent a control panel item.
    public sealed class ControlPanelItem
        /// Control panel applet name.
        /// Control panel applet canonical name.
        public string CanonicalName { get; }
        /// Control panel applet category.
        public string[] Category { get; }
        /// Control panel applet description.
        public string Description { get; }
        /// Control panel applet path.
        internal string Path { get; }
        /// Internal constructor for ControlPanelItem.
        /// <param name="canonicalName"></param>
        /// <param name="category"></param>
        /// <param name="description"></param>
        /// <param name="path"></param>
        internal ControlPanelItem(string name, string canonicalName, string[] category, string description, string path)
            Name = name;
            CanonicalName = canonicalName;
            Category = category;
            Description = description;
        /// ToString method.
    /// This class implements the base for ControlPanelItem commands.
    public abstract class ControlPanelItemBaseCommand : PSCmdlet
        /// Locale specific verb action Open string exposed by the control panel item.
        private static string s_verbActionOpenName = null;
        /// Canonical name of the control panel item used as a reference to fetch the verb
        /// action Open string. This control panel item exists on all SKU's.
        private const string RegionCanonicalName = "Microsoft.RegionAndLanguage";
        private const string ControlPanelShellFolder = "shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}";
        private static readonly string[] s_controlPanelItemFilterList = new string[] { "Folder Options", "Taskbar and Start Menu" };
        private const string TestHeadlessServerScript = @"
$result = $false
$serverManagerModule = Get-Module -ListAvailable | Where-Object {$_.Name -eq 'ServerManager'}
if ($serverManagerModule -ne $null)
    Import-Module ServerManager
    $Gui = (Get-WindowsFeature Server-Gui-Shell).Installed
    if ($Gui -eq $false)
        $result = $true
        internal readonly Dictionary<string, string> CategoryMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        internal string[] CategoryNames = { "*" };
        internal string[] RegularNames = { "*" };
        internal string[] CanonicalNames = { "*" };
        internal ControlPanelItem[] ControlPanelItems = new ControlPanelItem[0];
        /// Get all executable control panel items.
        internal List<ShellFolderItem> AllControlPanelItems
                if (_allControlPanelItems == null)
                    _allControlPanelItems = new List<ShellFolderItem>();
                    string allItemFolderPath = ControlPanelShellFolder + "\\0";
                    IShellDispatch4 shell2 = (IShellDispatch4)new Shell();
                    Folder2 allItemFolder = (Folder2)shell2.NameSpace(allItemFolderPath);
                    FolderItems3 allItems = (FolderItems3)allItemFolder.Items();
                    bool applyControlPanelItemFilterList = IsServerCoreOrHeadLessServer();
                    foreach (ShellFolderItem item in allItems)
                        if (applyControlPanelItemFilterList)
                            foreach (string name in s_controlPanelItemFilterList)
                                if (name.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
                            if (match)
                        if (ContainVerbOpen(item))
                            _allControlPanelItems.Add(item);
                return _allControlPanelItems;
        private List<ShellFolderItem> _allControlPanelItems;
        #region Cmdlet Overrides
        /// Does the preprocessing for ControlPanelItem cmdlets.
            System.OperatingSystem osInfo = System.Environment.OSVersion;
            PlatformID platform = osInfo.Platform;
            Version version = osInfo.Version;
            if (platform.Equals(PlatformID.Win32NT) &&
                ((version.Major < 6) ||
                 ((version.Major == 6) && (version.Minor < 2))
                // Below Win8, this cmdlet is not supported because of Win8:794135
                // throw terminating
                string message = string.Format(CultureInfo.InvariantCulture,
                                               ControlPanelResources.ControlPanelItemCmdletNotSupported,
                                               this.CommandInfo.Name);
                throw new PSNotSupportedException(message);
        /// Test if an item can be invoked.
        /// <param name="item"></param>
        private bool ContainVerbOpen(ShellFolderItem item)
            FolderItemVerbs verbs = item.Verbs();
            foreach (FolderItemVerb verb in verbs)
                if (!string.IsNullOrEmpty(verb.Name) &&
                    (verb.Name.Equals(ControlPanelResources.VerbActionOpen, StringComparison.OrdinalIgnoreCase) ||
                     CompareVerbActionOpen(verb.Name)))
        /// CompareVerbActionOpen is a helper function used to perform locale specific
        /// comparison of the verb action Open exposed by various control panel items.
        /// <param name="verbActionName">Locale specific verb action exposed by the control panel item.</param>
        /// <returns>True if the control panel item supports verb action open or else returns false.</returns>
        private static bool CompareVerbActionOpen(string verbActionName)
            if (s_verbActionOpenName == null)
                const string allItemFolderPath = ControlPanelShellFolder + "\\0";
                    string canonicalName = (string)item.ExtendedProperty("System.ApplicationName");
                    canonicalName = !string.IsNullOrEmpty(canonicalName)
                                        ? canonicalName.Substring(0, canonicalName.IndexOf('\0'))
                                        : null;
                    if (canonicalName != null && canonicalName.Equals(RegionCanonicalName, StringComparison.OrdinalIgnoreCase))
                        // The 'Region' control panel item always has '&Open' (english or other locale) as the first verb name
                        s_verbActionOpenName = item.Verbs().Item(0).Name;
                Dbg.Assert(s_verbActionOpenName != null, "The 'Region' control panel item is available on all SKUs and it always "
                                                       + "has '&Open' as the first verb item, so VerbActionOpenName should never be null at this point");
            return s_verbActionOpenName.Equals(verbActionName, StringComparison.OrdinalIgnoreCase);
        /// IsServerCoreORHeadLessServer is a helper function that checks if the current SKU is a
        /// Server Core machine or if the Server-GUI-Shell feature is removed on the machine.
        /// <returns>True if the current SKU is a Server Core machine or if the Server-GUI-Shell
        /// feature is removed on the machine or else returns false.</returns>
        private bool IsServerCoreOrHeadLessServer()
            using (RegistryKey installation = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion"))
                Dbg.Assert(installation != null, "the CurrentVersion subkey should exist");
                string installationType = (string)installation.GetValue("InstallationType", string.Empty);
                if (installationType.Equals("Server Core"))
                else if (installationType.Equals("Server"))
                    using (System.Management.Automation.PowerShell ps = System.Management.Automation.PowerShell.Create())
                        ps.AddScript(TestHeadlessServerScript);
                        Collection<PSObject> psObjectCollection = ps.Invoke(Array.Empty<object>());
                        Dbg.Assert(psObjectCollection != null && psObjectCollection.Count == 1, "invoke should never return null, there should be only one return item");
                        if (LanguagePrimitives.IsTrue(PSObject.Base(psObjectCollection[0])))
        /// Get the category number and name map.
        internal void GetCategoryMap()
            if (CategoryMap.Count != 0)
            Folder2 categoryFolder = (Folder2)shell2.NameSpace(ControlPanelShellFolder);
            FolderItems3 catItems = (FolderItems3)categoryFolder.Items();
            foreach (ShellFolderItem category in catItems)
                string path = category.Path;
                string catNum = path.Substring(path.LastIndexOf('\\') + 1);
                CategoryMap.Add(catNum, category.Name);
        /// Get control panel item by the category.
        /// <param name="controlPanelItems"></param>
        internal List<ShellFolderItem> GetControlPanelItemByCategory(List<ShellFolderItem> controlPanelItems)
            List<ShellFolderItem> list = new List<ShellFolderItem>();
            HashSet<string> itemSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string pattern in CategoryNames)
                bool found = false;
                WildcardPattern wildcard = WildcardPattern.Get(pattern, WildcardOptions.IgnoreCase);
                foreach (ShellFolderItem item in controlPanelItems)
                    string path = item.Path;
                    int[] categories = (int[])item.ExtendedProperty("System.ControlPanel.Category");
                    foreach (int cat in categories)
                        string catStr = (string)LanguagePrimitives.ConvertTo(cat, typeof(string), CultureInfo.InvariantCulture);
                        Dbg.Assert(CategoryMap.ContainsKey(catStr), "the category should be contained in _categoryMap");
                        string catName = CategoryMap[catStr];
                        if (!wildcard.IsMatch(catName))
                        if (itemSet.Contains(path))
                        itemSet.Add(path);
                        list.Add(item);
                if (!found && !WildcardPattern.ContainsWildcardCharacters(pattern))
                    string errMsg = StringUtil.Format(ControlPanelResources.NoControlPanelItemFoundForGivenCategory, pattern);
                    ErrorRecord error = new ErrorRecord(new InvalidOperationException(errMsg),
                                                        "NoControlPanelItemFoundForGivenCategory",
                                                        ErrorCategory.InvalidArgument, pattern);
            return list;
        /// Get control panel item by the regular name.
        /// <param name="withCategoryFilter"></param>
        internal List<ShellFolderItem> GetControlPanelItemByName(List<ShellFolderItem> controlPanelItems, bool withCategoryFilter)
            foreach (string pattern in RegularNames)
                    string name = item.Name;
                    if (!wildcard.IsMatch(name))
                    string formatString = withCategoryFilter
                                              ? ControlPanelResources.NoControlPanelItemFoundForGivenNameWithCategory
                                              : ControlPanelResources.NoControlPanelItemFoundForGivenName;
                    string errMsg = StringUtil.Format(formatString, pattern);
                                                        "NoControlPanelItemFoundForGivenName",
        /// Get control panel item by the canonical name.
        internal List<ShellFolderItem> GetControlPanelItemByCanonicalName(List<ShellFolderItem> controlPanelItems, bool withCategoryFilter)
            if (CanonicalNames == null)
                    if (canonicalName == null)
                    string errMsg = withCategoryFilter
                                        ? ControlPanelResources.NoControlPanelItemFoundWithNullCanonicalNameWithCategory
                                        : ControlPanelResources.NoControlPanelItemFoundWithNullCanonicalName;
                    ErrorRecord error = new ErrorRecord(new InvalidOperationException(errMsg), string.Empty,
                                                        ErrorCategory.InvalidArgument, CanonicalNames);
            foreach (string pattern in CanonicalNames)
                    canonicalName = canonicalName != null
                        if (pattern.Equals("*", StringComparison.OrdinalIgnoreCase))
                            if (!itemSet.Contains(path))
                        if (!wildcard.IsMatch(canonicalName))
                                              ? ControlPanelResources.NoControlPanelItemFoundForGivenCanonicalNameWithCategory
                                              : ControlPanelResources.NoControlPanelItemFoundForGivenCanonicalName;
                                                        "NoControlPanelItemFoundForGivenCanonicalName",
        /// Get control panel item by the ControlPanelItem instances.
        internal List<ShellFolderItem> GetControlPanelItemsByInstance(List<ShellFolderItem> controlPanelItems)
            foreach (ControlPanelItem controlPanelItem in ControlPanelItems)
                    if (!controlPanelItem.Path.Equals(path, StringComparison.OrdinalIgnoreCase))
                    string errMsg = StringUtil.Format(ControlPanelResources.NoControlPanelItemFoundForGivenInstance,
                                                      controlPanelItem.GetType().Name);
                                                        "NoControlPanelItemFoundForGivenInstance",
                                                        ErrorCategory.InvalidArgument, controlPanelItem);
    /// Get all control panel items that is available in the "All Control Panel Items" category.
    [Cmdlet(VerbsCommon.Get, "ControlPanelItem", DefaultParameterSetName = RegularNameParameterSet, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=219982")]
    [OutputType(typeof(ControlPanelItem))]
    public sealed class GetControlPanelItemCommand : ControlPanelItemBaseCommand
        private const string RegularNameParameterSet = "RegularName";
        private const string CanonicalNameParameterSet = "CanonicalName";
        /// Control panel item names.
        [Parameter(Position = 0, ParameterSetName = RegularNameParameterSet, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
            get { return RegularNames; }
                RegularNames = value;
                _nameSpecified = true;
        private bool _nameSpecified = false;
        /// Canonical names of control panel items.
        [Parameter(Mandatory = true, ParameterSetName = CanonicalNameParameterSet)]
        public string[] CanonicalName
            get { return CanonicalNames; }
                CanonicalNames = value;
                _canonicalNameSpecified = true;
        private bool _canonicalNameSpecified = false;
        /// Category of control panel items.
        public string[] Category
            get { return CategoryNames; }
                CategoryNames = value;
                _categorySpecified = true;
        private bool _categorySpecified = false;
        #endregion "Parameters"
            GetCategoryMap();
            List<ShellFolderItem> items = GetControlPanelItemByCategory(AllControlPanelItems);
            if (_nameSpecified)
                items = GetControlPanelItemByName(items, _categorySpecified);
            else if (_canonicalNameSpecified)
                items = GetControlPanelItemByCanonicalName(items, _categorySpecified);
            List<ControlPanelItem> results = new List<ControlPanelItem>();
            foreach (ShellFolderItem item in items)
                string description = (string)item.ExtendedProperty("InfoTip");
                string[] cateStrings = new string[categories.Length];
                for (int i = 0; i < categories.Length; i++)
                    string catStr = (string)LanguagePrimitives.ConvertTo(categories[i], typeof(string), CultureInfo.InvariantCulture);
                    Dbg.Assert(CategoryMap.ContainsKey(catStr), "the category should be contained in CategoryMap");
                    cateStrings[i] = CategoryMap[catStr];
                ControlPanelItem controlPanelItem = new ControlPanelItem(name, canonicalName, cateStrings, description, path);
                results.Add(controlPanelItem);
            // Sort the results by Canonical Name
            results.Sort(CompareControlPanelItems);
            foreach (ControlPanelItem controlPanelItem in results)
                WriteObject(controlPanelItem);
        private static int CompareControlPanelItems(ControlPanelItem x, ControlPanelItem y)
            // In the case that at least one of them is null
            if (x.CanonicalName == null && y.CanonicalName == null)
            if (x.CanonicalName == null)
            if (y.CanonicalName == null)
            // In the case that both are not null
            return string.Compare(x.CanonicalName, y.CanonicalName, StringComparison.OrdinalIgnoreCase);
    /// Show the specified control panel applet.
    [Cmdlet(VerbsCommon.Show, "ControlPanelItem", DefaultParameterSetName = RegularNameParameterSet, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=219983")]
    public sealed class ShowControlPanelItemCommand : ControlPanelItemBaseCommand
        private const string ControlPanelItemParameterSet = "ControlPanelItem";
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = RegularNameParameterSet, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
            set { RegularNames = value; }
            set { CanonicalNames = value; }
        /// Control panel items returned by Get-ControlPanelItem.
        [Parameter(Position = 0, ParameterSetName = ControlPanelItemParameterSet, ValueFromPipeline = true)]
        public ControlPanelItem[] InputObject
            get { return ControlPanelItems; }
            set { ControlPanelItems = value; }
            List<ShellFolderItem> items;
            if (ParameterSetName == RegularNameParameterSet)
                items = GetControlPanelItemByName(AllControlPanelItems, false);
            else if (ParameterSetName == CanonicalNameParameterSet)
                items = GetControlPanelItemByCanonicalName(AllControlPanelItems, false);
                items = GetControlPanelItemsByInstance(AllControlPanelItems);
                item.InvokeVerb();

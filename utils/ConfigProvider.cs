namespace Microsoft.WSMan.Management
    /// WsMan Provider.
    [CmdletProvider(WSManStringLiterals.ProviderName, ProviderCapabilities.Credentials)]
    public sealed class WSManConfigProvider : NavigationCmdletProvider, ICmdletProviderSupportsHelp
        // Plugin Name Storage
        private PSObject objPluginNames = null;
        private ServiceController winrmServiceController;
        /// Determines if Set-Item user input type validation is required or not.
        /// It is True by default, Clear-Item will set it to false so that it can
        /// pass Empty string as value for Set-Item.
        private bool clearItemIsCalled = false;
        private WSManHelper helper = new WSManHelper();
        /// Object contains the cache of the enumerate results for the cmdlet to execute.
        private readonly Dictionary<string, XmlDocument> enumerateMapping = new Dictionary<string, XmlDocument>();
        /// Mapping of ResourceURI with the XML returned by the Get call.
        private readonly Dictionary<string, string> getMapping = new Dictionary<string, string>();
        ///   This implements Get-Help for config provider custom path.
        ///   When user calls "Get-Help new-item" in our config provider path, this function will get called.
        /// <param name="helpItemName"></param>
            // Get the leaf node from the path for which help is requested.
            int ChildIndex = path.LastIndexOf('\\');
            if (ChildIndex == -1)
                // Means we are at host level, where no new-item is supported. Return empty string.
            string child = path.Substring(ChildIndex + 1);
            // We only return help for the below set of 5 commands, not for any other case.
            switch (helpItemName)
                case "New-Item":
                case "Get-Item":
                case "Set-Item":
                case "Clear-Item":
                case "Remove-Item":
            XmlDocument document = new XmlDocument();
            CultureInfo culture = Host.CurrentUICulture;
            string providerBase = this.ProviderInfo.PSSnapIn != null ? this.ProviderInfo.PSSnapIn.ApplicationBase : this.ProviderInfo.Module.ModuleBase;  // "\windows\system32\WindowsPowerShell\v1.0"
            string helpFile = null;
                string muiDirectory = Path.Combine(providerBase, culture.Name);
                if (Directory.Exists(muiDirectory))
                    string supposedHelpFile = Path.Combine(muiDirectory, this.ProviderInfo.HelpFile);
                    if (File.Exists(supposedHelpFile))
                        helpFile = supposedHelpFile;
                culture = culture.Parent;
            } while (culture != culture.Parent);
            if (helpFile == null)
                // Can't find help file. Return empty string
                XmlReaderSettings readerSettings = new XmlReaderSettings();
                readerSettings.XmlResolver = null;
                using (XmlReader reader = XmlReader.Create(helpFile, readerSettings))
            // Add the "msh" and "command" namespaces from the MAML schema
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(document.NameTable);
            // XPath 1.0 associates empty prefix with "null" namespace; must use non-empty prefix for default namespace.
            // This will not work: nsMgr.AddNamespace(string.Empty, "http://msh");
            nsMgr.AddNamespace("msh", "http://msh");
            nsMgr.AddNamespace("command", "http://schemas.microsoft.com/maml/dev/command/2004/10");
            // Split the help item name into verb and noun
            string verb = helpItemName.Split('-')[0];
            string noun = helpItemName.Substring(helpItemName.IndexOf('-') + 1);
            // Compose XPath query to select the appropriate node based on the verb, noun and id
            string xpathQuery = "/msh:helpItems/msh:providerHelp/msh:CmdletHelpPaths/msh:CmdletHelpPath[@id='" + child + "' or @ID='" + child + "']/command:command/command:details[command:verb='" + verb + "' and command:noun='" + noun + "']";
            // Execute the XPath query and if the command was found, return its MAML snippet
            XmlNode result = null;
                result = document.SelectSingleNode(xpathQuery, nsMgr);
                return result.ParentNode.OuterXml;
        #region DriveCmdletProvider
        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
            if (drive == null)
            if (!string.IsNullOrEmpty(drive.Root))
                AssertError(helper.GetResourceMsgFromResourcetext("NewDriveRootDoesNotExist"), false);
            return drive;
        /// Adds the required drive.
            Collection<PSDriveInfo> drives = new Collection<PSDriveInfo>();
            drives.Add(new PSDriveInfo(WSManStringLiterals.rootpath, ProviderInfo, string.Empty,
                        helper.GetResourceMsgFromResourcetext("ConfigStorage"), null));
        /// Removes the required drive.
        protected override PSDriveInfo RemoveDrive(PSDriveInfo drive)
            WSManHelper.ReleaseSessions();
        #region ItemCmdletProvider
        /// Get a Child Name. This method is called from MakePath method.
        /// This Method helps in getting the correct case of particular element in the provider path.
        /// XML is case sensitive but Powershell is not.
            string result = string.Empty;
            int separatorIndex = path.LastIndexOf(WSManStringLiterals.DefaultPathSeparator);
            string hostname = string.Empty;
                hostname = path;
                hostname = GetHostName(path);
            return GetCorrectCaseOfName(result, hostname, path);
        /// This method is provided by the Provider infrastructure. This method is called in all actions done
        /// by the provider to get the resolved path. Internally Resolve-Path is called.
        /// Since Root is empty for WsMan Provider the default path generated by Makepath is not correct.
        /// So we have made the tweaks in this method to return the correct resolved path.
        /// <param name="child"></param>
        protected override string MakePath(string parent, string child)
            if (child.EndsWith(WSManStringLiterals.DefaultPathSeparator.ToString(), StringComparison.OrdinalIgnoreCase))
                child = child.Remove(child.LastIndexOf(WSManStringLiterals.DefaultPathSeparator));
            // For Listeners only ... should remove Listener from listener\listener but not from listener_[Hashcode]
            if (parent.Equals(WSManStringLiterals.containerListener, StringComparison.OrdinalIgnoreCase) && child.StartsWith(parent, StringComparison.OrdinalIgnoreCase))
                if (!child.StartsWith(parent + "_", StringComparison.OrdinalIgnoreCase))
                    child = child.Remove(0, parent.Length);
            string ChildName = string.Empty;
            string CorrectCaseChildName = string.Empty;
            if (parent.Length != 0)
                path = parent + WSManStringLiterals.DefaultPathSeparator + child;
                path = child;
            if (path.Length != 0)
                ChildName = path.Substring(path.LastIndexOf(WSManStringLiterals.DefaultPathSeparator) + 1);
                CorrectCaseChildName = GetChildName(path);
            if (ChildName.Equals(CorrectCaseChildName, StringComparison.OrdinalIgnoreCase))
                if (child.Contains(WSManStringLiterals.DefaultPathSeparator.ToString()))
                    child = child.Substring(0, child.LastIndexOf(WSManStringLiterals.DefaultPathSeparator));
                    child = child + WSManStringLiterals.DefaultPathSeparator + CorrectCaseChildName;
                    child = CorrectCaseChildName;
            string basepath = base.MakePath(parent, child);
            return GetCorrectCaseOfPath(basepath);
        /// Checks whether the path is Valid.
        /// eg. winrm/config/client.
            result = CheckValidContainerOrPath(path);
        /// Check whether an Item Exist in the winrm configuration.
        /// Checks whether the given path has got child items.
        /// e.g: This is called by Provider infrastructure when we do a Remove-Item and prompts user
        /// if child items are present.
            string childname = string.Empty;
            string strPathCheck = string.Empty;
            if (path.Length == 0 && string.IsNullOrEmpty(childname))
            // if endswith '\', removes it.
            if (path.EndsWith(WSManStringLiterals.DefaultPathSeparator.ToString(), StringComparison.OrdinalIgnoreCase))
                path = path.Remove(path.LastIndexOf(WSManStringLiterals.DefaultPathSeparator));
            if (path.Contains(WSManStringLiterals.DefaultPathSeparator.ToString()))
                // Get the ChildName
                childname = path.Substring(path.LastIndexOf(WSManStringLiterals.DefaultPathSeparator) + 1);
            Dictionary<string, object> SessionObjCache = WSManHelper.GetSessionObjCache();
            if (SessionObjCache.ContainsKey(path))
            // Get the wsman host name to find the session object
            string host = GetHostName(path);
            // Chks the WinRM Service
            if (IsPathLocalMachine(host))
                if (!IsWSManServiceRunning())
                    WSManHelper.ThrowIfNotAdministrator();
                    StartWSManService(Force);
            string WsManURI = NormalizePath(path, host);
            lock (WSManHelper.AutoSession)
                // Gets the session object from the cache.
                object sessionobj;
                SessionObjCache.TryGetValue(host, out sessionobj);
                WsMan Config Can be divided in to Four Fixed Regions to Check Whether it has Child Items.
                 * 1. Branch in to Listeners (winrm/config/listener)
                 * 2. Branch in to CertMapping (winrm/config/service/certmapping)
                 * 3. Branch in to Plugin (winrm/config/plugin) - Plugin is subdivided in Resources,Security & InitParams
                 * 4. Rest all the branches like Client, Shell(WinRS) ,Service
                // 1. Listener Checks
                strPathCheck = host + WSManStringLiterals.DefaultPathSeparator;
                if (WsManURI.Contains(WSManStringLiterals.containerListener))
                    XmlDocument xmlListeners = EnumerateResourceValue(sessionobj, WsManURI);
                    if (xmlListeners != null)
                        Hashtable KeyCache, ListenerObjCache;
                        ProcessListenerObjects(xmlListeners, out ListenerObjCache, out KeyCache);
                        if (ListenerObjCache.Count > 0)
                // 2. Client Certificate Checks
                else if (WsManURI.Contains(WSManStringLiterals.containerCertMapping))
                    XmlDocument xmlCertificates = EnumerateResourceValue(sessionobj, WsManURI);
                    Hashtable KeyCache, CertificatesObjCache;
                    if (xmlCertificates == null)
                    ProcessCertMappingObjects(xmlCertificates, out CertificatesObjCache, out KeyCache);
                    if (CertificatesObjCache.Count > 0)
                // 3. Plugin and its internal structure Checks
                else if (WsManURI.Contains(WSManStringLiterals.containerPlugin))
                    strPathCheck += WSManStringLiterals.containerPlugin;
                    // Check for Plugin path
                    XmlDocument xmlPlugins = FindResourceValue(sessionobj, WsManURI, null);
                    string currentpluginname = string.Empty;
                    int PluginCount = GetPluginNames(xmlPlugins, out objPluginNames, out currentpluginname, path);
                    if (path.Equals(strPathCheck))
                        if (PluginCount > 0)
                    strPathCheck = strPathCheck + WSManStringLiterals.DefaultPathSeparator + currentpluginname;
                    if (path.EndsWith(strPathCheck, StringComparison.OrdinalIgnoreCase))
                        if (objPluginNames != null)
                            if (objPluginNames.Properties.Match(currentpluginname).Count > 0)
                    string filter = WsManURI + "?Name=" + currentpluginname;
                    XmlDocument CurrentPluginXML = GetResourceValue(sessionobj, filter, null);
                    ArrayList arrSecurities = null;
                    ArrayList arrResources = ProcessPluginResourceLevel(CurrentPluginXML, out arrSecurities);
                    ArrayList arrInitParams = ProcessPluginInitParamLevel(CurrentPluginXML);
                    strPathCheck += WSManStringLiterals.DefaultPathSeparator;
                    if (path.EndsWith(strPathCheck + WSManStringLiterals.containerResources, StringComparison.OrdinalIgnoreCase))
                        if (arrResources != null && arrResources.Count > 0)
                    if (path.EndsWith(strPathCheck + WSManStringLiterals.containerInitParameters, StringComparison.OrdinalIgnoreCase))
                        if (arrInitParams != null && arrInitParams.Count > 0)
                    if (path.EndsWith(strPathCheck + WSManStringLiterals.containerQuotasParameters, StringComparison.OrdinalIgnoreCase))
                        XmlNodeList nodeListForQuotas = CurrentPluginXML.GetElementsByTagName(WSManStringLiterals.containerQuotasParameters);
                        if (nodeListForQuotas.Count > 0)
                            XmlNode pluginQuotas = nodeListForQuotas[0];
                            return pluginQuotas.Attributes.Count > 0;
                    if (arrResources != null)
                        foreach (PSObject objresource in arrResources)
                            string sResourceDirName = objresource.Properties["ResourceDir"].Value.ToString();
                            if (path.Contains(sResourceDirName))
                                strPathCheck = strPathCheck + WSManStringLiterals.containerResources + WSManStringLiterals.DefaultPathSeparator;
                                if (path.EndsWith(strPathCheck + sResourceDirName, StringComparison.OrdinalIgnoreCase))
                                strPathCheck = strPathCheck + sResourceDirName + WSManStringLiterals.DefaultPathSeparator;
                                if (path.Contains(strPathCheck + WSManStringLiterals.containerSecurity))
                                    if (path.EndsWith(strPathCheck + WSManStringLiterals.containerSecurity, StringComparison.OrdinalIgnoreCase))
                                        if (arrSecurities != null && arrSecurities.Count > 0)
                                    strPathCheck = strPathCheck + WSManStringLiterals.containerSecurity + WSManStringLiterals.DefaultPathSeparator;
                                    if (path.Contains(strPathCheck + WSManStringLiterals.containerSecurity + "_"))
                                        if (arrSecurities == null)
                                        foreach (PSObject security in arrSecurities)
                                            string sSecurity = security.Properties["SecurityDIR"].Value.ToString();
                                            if (path.EndsWith(sSecurity, StringComparison.OrdinalIgnoreCase))
                // 4. All Other Item Checks
                    string getXml = this.GetResourceValueInXml(sessionobj, WsManURI, null);
                    XmlDocument xmlResourceValues = new XmlDocument();
                    xmlResourceValues.LoadXml(getXml.ToLowerInvariant());
                    XmlNodeList nodes = SearchXml(xmlResourceValues, childname, WsManURI, path, host);
                    if (nodes != null)
                        return IsItemContainer(nodes);
        /// This cmdlet is used to get a particular item.
        /// cd wsman:\localhost\client> Get-Item .\Auth.
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
                WriteItemObject(GetItemPSObjectWithTypeName(WSManStringLiterals.rootpath, WSManStringLiterals.ContainerChildValue, null, null, null, WsManElementObjectTypes.WSManConfigElement), WSManStringLiterals.rootpath, true);
                childname = path;
            if (childname.Equals(path, StringComparison.OrdinalIgnoreCase))
                if (SessionObjCache.ContainsKey(childname))
                    WriteItemObject(GetItemPSObjectWithTypeName(childname, WSManStringLiterals.ContainerChildValue, null, null, "ComputerLevel", WsManElementObjectTypes.WSManConfigContainerElement), WSManStringLiterals.rootpath + WSManStringLiterals.DefaultPathSeparator + childname, true);
            path = path.Substring(0, path.LastIndexOf(childname, StringComparison.OrdinalIgnoreCase));
            string uri = NormalizePath(path, host);
                XmlDocument xmlResource = FindResourceValue(sessionobj, uri, null);
                if (xmlResource == null)
                string strPathChk = host + WSManStringLiterals.DefaultPathSeparator;
                if (path.Contains(strPathChk + WSManStringLiterals.containerListener))
                    GetItemListenerOrCertMapping(path, xmlResource, WSManStringLiterals.containerListener, childname, host);
                else if (path.Contains(strPathChk + WSManStringLiterals.containerClientCertificate))
                    GetItemListenerOrCertMapping(path, xmlResource, WSManStringLiterals.containerClientCertificate, childname, host);
                else if (path.Contains(strPathChk + WSManStringLiterals.containerPlugin))
                    GetPluginNames(xmlResource, out objPluginNames, out currentpluginname, path);
                    if (path.EndsWith(strPathChk + WSManStringLiterals.containerPlugin, StringComparison.OrdinalIgnoreCase))
                            WriteItemObject(GetItemPSObjectWithTypeName(objPluginNames.Properties[childname].Name, objPluginNames.Properties[childname].Value.ToString(), null, new string[] { "Name=" + objPluginNames.Properties[childname].Name }, null, WsManElementObjectTypes.WSManConfigContainerElement), path + WSManStringLiterals.DefaultPathSeparator + childname, true);
                        catch (PSArgumentNullException) { return; }
                        catch (NullReferenceException) { return; }
                        strPathChk = strPathChk + WSManStringLiterals.containerPlugin + WSManStringLiterals.DefaultPathSeparator;
                        string filter = uri + "?Name=" + currentpluginname;
                        if (CurrentPluginXML == null)
                        PSObject objPluginlevel = ProcessPluginConfigurationLevel(CurrentPluginXML, true);
                        ArrayList arrSecurity = null;
                        ArrayList arrResources = ProcessPluginResourceLevel(CurrentPluginXML, out arrSecurity);
                            if (path.Contains(strPathChk + currentpluginname))
                                if (path.EndsWith(strPathChk + currentpluginname, StringComparison.OrdinalIgnoreCase))
                                    if (!objPluginlevel.Properties[childname].Value.ToString().Equals(WSManStringLiterals.ContainerChildValue))
                                        WriteItemObject(GetItemPSObjectWithTypeName(objPluginlevel.Properties[childname].Name, objPluginlevel.Properties[childname].TypeNameOfValue, objPluginlevel.Properties[childname].Value, null, null, WsManElementObjectTypes.WSManConfigLeafElement), path + WSManStringLiterals.DefaultPathSeparator + objPluginlevel.Properties[childname].Name, false);
                                        WriteItemObject(GetItemPSObjectWithTypeName(objPluginlevel.Properties[childname].Name, objPluginlevel.Properties[childname].Value.ToString(), null, null, null, WsManElementObjectTypes.WSManConfigLeafElement), path + WSManStringLiterals.DefaultPathSeparator + objPluginlevel.Properties[childname].Name, true);
                                strPathChk = strPathChk + currentpluginname + WSManStringLiterals.DefaultPathSeparator;
                                if (path.Contains(strPathChk + WSManStringLiterals.containerResources))
                                    if (arrResources == null)
                                    if (path.EndsWith(strPathChk + WSManStringLiterals.containerResources, StringComparison.OrdinalIgnoreCase))
                                        foreach (PSObject p in arrResources)
                                            if (p.Properties["ResourceDir"].Value.ToString().Equals(childname))
                                                WriteItemObject(GetItemPSObjectWithTypeName(childname, WSManStringLiterals.ContainerChildValue, null, new string[] { "ResourceURI=" + p.Properties["ResourceUri"].Value.ToString() }, null, WsManElementObjectTypes.WSManConfigContainerElement), path + WSManStringLiterals.DefaultPathSeparator + childname, true);
                                    strPathChk = strPathChk + WSManStringLiterals.containerResources + WSManStringLiterals.DefaultPathSeparator;
                                    int Sepindex = path.IndexOf(WSManStringLiterals.DefaultPathSeparator, strPathChk.Length);
                                    string sResourceDirName = string.Empty;
                                    if (Sepindex == -1)
                                        sResourceDirName = path.Substring(strPathChk.Length);
                                        sResourceDirName = path.Substring(strPathChk.Length, path.IndexOf(WSManStringLiterals.DefaultPathSeparator, strPathChk.Length) - (strPathChk.Length));
                                    if (path.Contains(strPathChk + sResourceDirName))
                                        if (path.EndsWith(strPathChk + sResourceDirName, StringComparison.OrdinalIgnoreCase))
                                                if (sResourceDirName.Equals(p.Properties["ResourceDir"].Value.ToString(), StringComparison.OrdinalIgnoreCase))
                                                    p.Properties.Remove("ResourceDir");
                                                    if (p.Properties[childname].Value.ToString().Equals(WSManStringLiterals.ContainerChildValue))
                                                        WriteItemObject(GetItemPSObjectWithTypeName(p.Properties[childname].Name, p.Properties[childname].Value.ToString(), null, null, null, WsManElementObjectTypes.WSManConfigLeafElement), path + WSManStringLiterals.DefaultPathSeparator + p.Properties[childname].Name, true);
                                                        WriteItemObject(GetItemPSObjectWithTypeName(p.Properties[childname].Name, p.Properties[childname].TypeNameOfValue, p.Properties[childname].Value, null, null, WsManElementObjectTypes.WSManConfigLeafElement), path + WSManStringLiterals.DefaultPathSeparator + p.Properties[childname].Name, false);
                                        strPathChk = strPathChk + sResourceDirName + WSManStringLiterals.DefaultPathSeparator;
                                        if (path.Contains(strPathChk + WSManStringLiterals.containerSecurity))
                                            if (arrSecurity == null)
                                            foreach (PSObject p in arrSecurity)
                                                if (path.EndsWith(WSManStringLiterals.containerSecurity, StringComparison.OrdinalIgnoreCase))
                                                    WriteItemObject(GetItemPSObjectWithTypeName(p.Properties["SecurityDIR"].Value.ToString(), WSManStringLiterals.ContainerChildValue, null, new string[] { "Uri=" + p.Properties["Uri"].Value.ToString() }, null, WsManElementObjectTypes.WSManConfigContainerElement), path + WSManStringLiterals.DefaultPathSeparator + p.Properties["SecurityDIR"].Value.ToString(), true);
                                                    string sSecurityDirName = path.Substring(path.LastIndexOf(WSManStringLiterals.DefaultPathSeparator) + 1, path.Length - (path.LastIndexOf(WSManStringLiterals.DefaultPathSeparator) + 1));
                                                    if (sSecurityDirName.Equals(p.Properties["SecurityDIR"].Value.ToString()))
                                                        p.Properties.Remove("SecurityDIR");
                                else if (path.EndsWith(host + WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerPlugin + WSManStringLiterals.DefaultPathSeparator + currentpluginname + WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerInitParameters, StringComparison.OrdinalIgnoreCase))
                                    if (arrInitParams != null)
                                        foreach (PSObject p in arrInitParams)
                                            if (p.Properties.Match(childname, PSMemberTypes.NoteProperty).Count > 0)
                                                WriteItemObject(GetItemPSObjectWithTypeName(p.Properties[childname].Name, p.Properties[childname].TypeNameOfValue, p.Properties[childname].Value, null, "InitParams", WsManElementObjectTypes.WSManConfigLeafElement), path + WSManStringLiterals.DefaultPathSeparator + p.Properties[childname].Name, false);
                                else if (path.EndsWith(WSManStringLiterals.containerQuotasParameters, StringComparison.OrdinalIgnoreCase))
                                    // Get the Quotas element from the config XML.
                                        foreach (XmlAttribute attrOfQuotas in pluginQuotas.Attributes)
                                            if (childname.Equals(attrOfQuotas.Name, StringComparison.OrdinalIgnoreCase))
                                                PSObject objectToAdd =
                                                    GetItemPSObjectWithTypeName(
                                                        attrOfQuotas.Name,
                                                        attrOfQuotas.Value.GetType().ToString(),
                                                        attrOfQuotas.Value,
                                                        WsManElementObjectTypes.WSManConfigLeafElement);
                                                string pathToAdd =
                                                        "{0}{1}{2}",
                                                        WSManStringLiterals.DefaultPathSeparator,
                                                        attrOfQuotas.Name);
                                                WriteItemObject(objectToAdd, pathToAdd, false);
                        if (!uri.Equals(WinrmRootName[0], StringComparison.OrdinalIgnoreCase))
                            foreach (XmlNode innerResourceNodes in xmlResource.ChildNodes)
                                mshObject = ConvertToPSObject(innerResourceNodes);
                            mshObject = BuildHostLevelPSObjectArrayList(sessionobj, uri, false);
                        if (mshObject != null)
                            if (mshObject.Properties[childname].Value.ToString().Equals(WSManStringLiterals.ContainerChildValue))
                                WriteItemObject(GetItemPSObjectWithTypeName(mshObject.Properties[childname].Name, mshObject.Properties[childname].Value.ToString(), null, null, null, WsManElementObjectTypes.WSManConfigLeafElement), path + WSManStringLiterals.DefaultPathSeparator + mshObject.Properties[childname].Name, true);
                                WriteItemObject(
                                        mshObject.Properties[childname].Name,
                                        mshObject.Properties[childname].TypeNameOfValue,
                                        mshObject.Properties[childname].Value,
                                        null, null,
                                        WsManElementObjectTypes.WSManConfigLeafElement,
                                        mshObject),
                                    path + WSManStringLiterals.DefaultPathSeparator + mshObject.Properties[childname].Name,
                    catch (PSArgumentNullException) { return; /*Leaving this known exception for no value found. Not Throwing error.*/}
                    catch (NullReferenceException) { return; /*Leaving this known exception for no value found. Not Throwing error.*/}
        /// This cmdlet is used to set the value of a particular item.
        /// cd wsman:\localhost\client> Set-Item .\TrustedHosts -value "*"
        /// This has one dynamic parameter. It is used with TrustedHost only.
        /// The parameter is -Concatenate.
        protected override void SetItem(string path, object value)
                throw new ArgumentException(helper.GetResourceMsgFromResourcetext("value"));
            if (path.Length == 0 && string.IsNullOrEmpty(ChildName))
                AssertError(helper.GetResourceMsgFromResourcetext("SetItemNotSupported"), false);
                ChildName = path;
            if (ChildName.Equals(path, StringComparison.OrdinalIgnoreCase))
            if (!this.clearItemIsCalled)
                value = this.ValidateAndGetUserObject(ChildName, value);
                // The value will be Null only if the object provided by User is not of accepted type.
                // As of now, this can only happen in case of RunAsUserName and RunAsPassword
                // If validation is not required, that means Clear-Item cmdlet is called.
                // Clear-Item is not allowed on RunAsPassword, Admin should call Clear-Item RunAsUser
                // if he intends to disable RunAs on the Plugin.
                if (string.Equals(ChildName, WSManStringLiterals.ConfigRunAsPasswordName, StringComparison.OrdinalIgnoreCase))
                    AssertError(helper.GetResourceMsgFromResourcetext("ClearItemOnRunAsPassword"), false);
            string whatIfMessage = string.Format(CultureInfo.CurrentUICulture, helper.GetResourceMsgFromResourcetext("SetItemWhatIfAndConfirmText"), path, value);
            if (!ShouldProcess(whatIfMessage, string.Empty, string.Empty))
            path = path.Substring(0, path.LastIndexOf(ChildName, StringComparison.OrdinalIgnoreCase));
            // Chk for Winrm Service
                    StartWSManService(this.Force);
            bool settingPickedUpDynamically = false;
                List<string> warningMessage = new List<string>();
                    SetItemListenerOrClientCertificate(sessionobj, uri, PKeyListener, ChildName, value, path, WSManStringLiterals.containerListener, host);
                    SetItemListenerOrClientCertificate(sessionobj, uri, PKeyCertMapping, ChildName, value, path, WSManStringLiterals.containerClientCertificate, host);
                        XmlDocument xmlPlugins = FindResourceValue(sessionobj, uri, null);
                        GetPluginNames(xmlPlugins, out objPluginNames, out currentpluginname, path);
                        if (string.IsNullOrEmpty(currentpluginname))
                                // Don't need an error if ClearItem is called.
                                AssertError(helper.GetResourceMsgFromResourcetext("ItemDoesNotExist"), false);
                        CurrentConfigurations pluginConfiguration = new CurrentConfigurations((IWSManSession)sessionobj);
                        string pluginXML = this.GetResourceValueInXml((IWSManSession)sessionobj, filter, null);
                        pluginConfiguration.RefreshCurrentConfiguration(pluginXML);
                        XmlDocument CurrentPluginXML = pluginConfiguration.RootDocument;
                            // Remove XML:LANG attribute if present.
                            // If not present ignore the exception.
                            pluginConfiguration.RemoveOneConfiguration("./attribute::xml:lang");
                            if (WSManStringLiterals.ConfigRunAsUserName.Equals(ChildName, StringComparison.OrdinalIgnoreCase)  && value is PSCredential runAsCredentials)
                                // UserName
                                value = runAsCredentials.UserName;
                                pluginConfiguration.UpdateOneConfiguration(
                                    WSManStringLiterals.ConfigRunAsPasswordName,
                                    GetStringFromSecureString(runAsCredentials.Password));
                            if (WSManStringLiterals.ConfigRunAsPasswordName.Equals(ChildName, StringComparison.OrdinalIgnoreCase))
                                if (string.IsNullOrEmpty(
                                    pluginConfiguration.GetOneConfiguration(
                                            "./attribute::{0}",
                                            WSManStringLiterals.ConfigRunAsUserName))))
                                    // User Cannot set RunAsPassword if, RunAsUser is not present.
                                    AssertError(helper.GetResourceMsgFromResourcetext("SetItemOnRunAsPasswordNoRunAsUser"), false);
                                value = GetStringFromSecureString(value);
                            pluginConfiguration.UpdateOneConfiguration(".", ChildName, value.ToString());
                                    if (ChildName.Equals(WSManStringLiterals.ConfigResourceUriName, StringComparison.OrdinalIgnoreCase))
                                        AssertError(helper.GetResourceMsgFromResourcetext("NoChangeValue"), false);
                                            string xpathToUse = string.Format(
                                                "{0}:{1}/{0}:{2}[attribute::{3}='{4}']",
                                                CurrentConfigurations.DefaultNameSpacePrefix,
                                                WSManStringLiterals.containerResources,
                                                WSManStringLiterals.containerSingleResource,
                                                WSManStringLiterals.ConfigResourceUriName,
                                                p.Properties[WSManStringLiterals.ConfigResourceUriName].Value.ToString());
                                            if (!p.Properties[ChildName].Value.ToString().Equals(WSManStringLiterals.ContainerChildValue))
                                                // Allow Set-Item on non-container values only.
                                                pluginConfiguration.UpdateOneConfiguration(xpathToUse, ChildName, value.ToString());
                                    if (path.EndsWith(strPathChk + WSManStringLiterals.containerSecurity, StringComparison.OrdinalIgnoreCase))
                                    if (path.Contains(strPathChk + WSManStringLiterals.containerSecurity + WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerSecurity + "_"))
                                        // Any setting inside security for plugin is picked up by service dynamically
                                        settingPickedUpDynamically = true;
                                            if (sSecurityDirName.Equals(p.Properties["SecurityDIR"].Value.ToString(), StringComparison.OrdinalIgnoreCase))
                                                    string query = helper.GetResourceMsgFromResourcetext("ShouldContinueSecurityQuery");
                                                    query = string.Format(CultureInfo.CurrentCulture, query, currentpluginname);
                                                    if (!ShouldContinue(query, helper.GetResourceMsgFromResourcetext("ShouldContinueSecurityCaption")))
                                                // NameSpace:Resources/NameSpace:Resource[@ResourceUri={''}]/NameSpace:Security[@Uri='{2}']
                                                    "{0}:{1}/{0}:{2}[@{6}='{7}']/{0}:{3}[@{4}='{5}']",
                                                    WSManStringLiterals.containerSecurity,
                                                    WSManStringLiterals.ConfigSecurityUri,
                                                    p.Properties[WSManStringLiterals.ConfigSecurityUri].Value.ToString(),
                                                    p.Properties["ParentResourceUri"].Value.ToString());
                        else if (path.EndsWith(strPathChk + WSManStringLiterals.containerInitParameters, StringComparison.OrdinalIgnoreCase))
                                if (p.Properties[ChildName] != null)
                                        "{0}:{1}/{0}:{2}[@{3}='{4}']",
                                        WSManStringLiterals.containerInitParameters,
                                        WSManStringLiterals.ConfigInitializeParameterTag,
                                        WSManStringLiterals.ConfigInitializeParameterName,
                                        p.Properties[ChildName].Name);
                                    pluginConfiguration.UpdateOneConfiguration(xpathToUse, WSManStringLiterals.ConfigInitializeParameterValue, value.ToString());
                        else if (path.EndsWith(strPathChk + WSManStringLiterals.containerQuotasParameters, StringComparison.OrdinalIgnoreCase))
                                "{0}:{1}",
                                WSManStringLiterals.containerQuotasParameters);
                            if (ppqWarningConfigurations.Contains(ChildName.ToLowerInvariant()))
                                string adjustedChileName = ChildName;
                                if (ChildName.Equals("IdleTimeoutms", StringComparison.OrdinalIgnoreCase))
                                    adjustedChileName = "IdleTimeout";
                                string pathForGlobalQuota =
                                        @"{0}:\{1}\{2}\{3}",
                                        WSManStringLiterals.rootpath,
                                        host,
                                        WSManStringLiterals.containerShell,
                                        adjustedChileName);
                                warningMessage.Add(string.Format(helper.GetResourceMsgFromResourcetext("SetItemWarnigForPPQ"), pathForGlobalQuota));
                        string resourceUri = string.Format(
                            "{0}?Name={1}",
                            uri,
                            currentpluginname);
                            pluginConfiguration.PutConfigurationOnServer(resourceUri);
                            // Show Win RM service restart warning only when the changed setting is not picked up dynamically
                            if (!settingPickedUpDynamically)
                                    warningMessage.Add(helper.GetResourceMsgFromResourcetext("SetItemServiceRestartWarning"));
                                    warningMessage.Add(string.Format(helper.GetResourceMsgFromResourcetext("SetItemServiceRestartWarningRemote"), host));
                            if (!string.IsNullOrEmpty(pluginConfiguration.ServerSession.Error))
                                AssertError(pluginConfiguration.ServerSession.Error, true);
                    catch (PSArgumentException)
                    catch (PSArgumentNullException)
                        Hashtable cmdlinevalue = new Hashtable();
                        if (ChildName.Equals("TrustedHosts", StringComparison.OrdinalIgnoreCase) || ChildName.Equals("RootSDDL", StringComparison.OrdinalIgnoreCase))
                            if (value.GetType().FullName.Equals("System.String"))
                                    string query = string.Empty;
                                    string caption = helper.GetResourceMsgFromResourcetext("SetItemGeneralSecurityCaption");
                                    if (ChildName.Equals("TrustedHosts", StringComparison.OrdinalIgnoreCase))
                                        query = helper.GetResourceMsgFromResourcetext("SetItemTrustedHostsWarningQuery");
                                    else if (ChildName.Equals("RootSDDL", StringComparison.OrdinalIgnoreCase))
                                        query = helper.GetResourceMsgFromResourcetext("SetItemRootSDDLWarningQuery");
                                    if (!ShouldContinue(query, caption))
                                if (DynamicParameters is WSManProviderSetItemDynamicParameters dynParams)
                                    if (dynParams.Concatenate)
                                        if (!string.IsNullOrEmpty(value.ToString()))
                                            // ',' is used as the delimiter in WSMan for TrustedHosts.
                                            value = SplitAndUpdateStringUsingDelimiter(sessionobj, uri, ChildName, value.ToString(), ",");
                                cmdlinevalue.Add(ChildName, value);
                                AssertError(helper.GetResourceMsgFromResourcetext("TrustedHostValueTypeError"), false);
                            if (globalWarningUris.Contains(uri) && globalWarningConfigurations.Contains(ChildName.ToLowerInvariant()))
                                warningMessage.Add(string.Format(helper.GetResourceMsgFromResourcetext("SetItemWarningForGlobalQuota"), value));
                        PutResourceValue(sessionobj, uri, cmdlinevalue, host);
                        AssertError(e.Message, false);
                foreach (string warnings in warningMessage)
                    WriteWarning(warnings);
        /// This command is used to clear the value of a item.
        protected override void ClearItem(string path)
            this.clearItemIsCalled = true;
            SetItem(path, string.Empty);
            this.clearItemIsCalled = false;
        /// This is method which create the dynamic or runtime parameter for set-item.
        protected override object SetItemDynamicParameters(string path, object value)
                string hostname = GetHostName(path);
                if (path.EndsWith(hostname + WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerClient, StringComparison.OrdinalIgnoreCase))
                    // To have Tab completion.
                    return new WSManProviderSetItemDynamicParameters();
                else if (path.EndsWith(hostname + WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerClient + WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerTrustedHosts, StringComparison.OrdinalIgnoreCase))
                    // To Support Concatenate parameter for trustedhosts.
        #region ContainerCmdletProvider
        /// Gets the Child items. dir functionality
        /// wsman:\localhost\client> dir.
        /// <param name="recurse"></param>
            GetChildItemsOrNames(path, ProviderMethods.GetChildItems, recurse);
        /// This method gives the names of child items. this is used for Tab completion.
        /// <param name="returnContainers"></param>
        protected override void GetChildNames(string path, ReturnContainers returnContainers)
            GetChildItemsOrNames(path, ProviderMethods.GetChildNames, false);
        #region NavigationalCmdletProvider
        /// Checks whether the specified path is a container path.
            if (string.IsNullOrEmpty(host))
                WsMan Config Can be divided in to Four Fixed Regions to Check Whether Item is Container
                    strPathCheck += WSManStringLiterals.containerListener;
                        if (KeyCache.Contains(childname))
                    strPathCheck += WSManStringLiterals.containerClientCertificate;
                    if (xmlCertificates != null)
                    strPathCheck += currentpluginname;
                    if (path.EndsWith(currentpluginname, StringComparison.OrdinalIgnoreCase))
                    if (arrResources == null || arrResources.Count == 0)
                    bool result = IsItemContainer(nodes);
        /// Removes a particular item.
        protected override void RemoveItem(string path, bool recurse)
            bool throwerror = true;
                AssertError(helper.GetResourceMsgFromResourcetext("RemoveItemNotSupported"), false);
                if (ChildName.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                    AssertError(helper.GetResourceMsgFromResourcetext("LocalHost"), false);
                helper.RemoveFromDictionary(ChildName);
                string inputStr = string.Empty;
                if (path.Contains(strPathCheck + WSManStringLiterals.containerPlugin))//(path.Contains(@"\plugin"))
                    if (path.EndsWith(strPathCheck + WSManStringLiterals.containerPlugin, StringComparison.OrdinalIgnoreCase))
                        // Deletes all the Plugin when user is in WsMan:\Localhost\Plugin.
                        string pluginUri = uri + "?Name=" + ChildName;
                        DeleteResourceValue(sessionobj, pluginUri, null, recurse);
                    int pos = 0; string pName = null;
                    pos = path.LastIndexOf(strPathCheck + WSManStringLiterals.DefaultPathSeparator, StringComparison.OrdinalIgnoreCase) + strPathCheck.Length + 1;
                    int pindex = path.IndexOf(WSManStringLiterals.DefaultPathSeparator, pos);
                    if (pindex != -1)
                        pName = path.Substring(pos, path.IndexOf(WSManStringLiterals.DefaultPathSeparator, pos) - pos);
                        pName = path.Substring(pos);
                    string filter1 = uri + "?Name=" + pName;
                    XmlDocument pxml = GetResourceValue(sessionobj, filter1, null);
                    PSObject ps = ProcessPluginConfigurationLevel(pxml);
                    ArrayList SecurityArray = null;
                    ArrayList ResourceArray = ProcessPluginResourceLevel(pxml, out SecurityArray);
                    ArrayList InitParamArray = ProcessPluginInitParamLevel(pxml);
                    strPathCheck = strPathCheck + WSManStringLiterals.DefaultPathSeparator + pName + WSManStringLiterals.DefaultPathSeparator;
                    if (path.Contains(strPathCheck + WSManStringLiterals.containerResources))
                        // Remove-Item is called for one of the resources.
                            throwerror = false;
                            ResourceArray = RemoveItemfromResourceArray(ResourceArray, ChildName, string.Empty, "ResourceDir");
                        if (throwerror)
                            int Sepindex = path.IndexOf(WSManStringLiterals.DefaultPathSeparator, strPathCheck.Length);
                                sResourceDirName = path.Substring(strPathCheck.Length);
                                sResourceDirName = path.Substring(strPathCheck.Length, path.IndexOf(WSManStringLiterals.DefaultPathSeparator, strPathCheck.Length) - (strPathCheck.Length));
                                SecurityArray = RemoveItemfromResourceArray(SecurityArray, ChildName, string.Empty, "SecurityDIR");
                    else if (path.EndsWith(strPathCheck + WSManStringLiterals.containerInitParameters, StringComparison.OrdinalIgnoreCase))
                        // Remove-Item is called for one of the initialization Parameters.
                        InitParamArray = RemoveItemfromResourceArray(InitParamArray, ChildName, "InitParams", string.Empty);
                    inputStr = ConstructPluginXml(ps, uri, host, "Set", ResourceArray, SecurityArray, InitParamArray);
                        ((IWSManSession)sessionobj).Put(uri + "?Name=" + pName, inputStr, 0);
                        if (!string.IsNullOrEmpty(((IWSManSession)sessionobj).Error))
                            AssertError(((IWSManSession)sessionobj).Error, true);
                else if (path.EndsWith(strPathCheck + WSManStringLiterals.containerListener, StringComparison.OrdinalIgnoreCase))
                    RemoveListenerOrCertMapping(sessionobj, uri, ChildName, PKeyListener, true);
                else if (path.EndsWith(strPathCheck + WSManStringLiterals.containerClientCertificate, StringComparison.OrdinalIgnoreCase))
                    RemoveListenerOrCertMapping(sessionobj, uri, ChildName, PKeyCertMapping, false);
        /// This method creates a new item of listener,clientcertificate etc.
        /// <param name="itemTypeName"></param>
        /// <param name="newItemValue"></param>
        protected override void NewItem(string path, string itemTypeName, object newItemValue)
            if (path.Length == 0 || !path.Contains(WSManStringLiterals.DefaultPathSeparator.ToString()))
                NewItemCreateComputerConnection(path);
                throw new ArgumentException(helper.GetResourceMsgFromResourcetext("InvalidPath"));
                if (path.Contains(strPathChk + WSManStringLiterals.containerPlugin))//(path.Contains(@"\plugin"))
                    NewItemPluginOrPluginChild(sessionobj, path, host, uri);
                else if (path.EndsWith(strPathChk + WSManStringLiterals.containerListener, StringComparison.OrdinalIgnoreCase))
                    WSManProvidersListenerParameters niParams = DynamicParameters as WSManProvidersListenerParameters;
                    Hashtable listenerparams = new Hashtable();
                    listenerparams.Add("Address", niParams.Address);
                    listenerparams.Add("Transport", niParams.Transport);
                    listenerparams.Add("Enabled", niParams.Enabled);
                    if (niParams.HostName != null)
                        listenerparams.Add("Hostname", niParams.HostName);
                    if (niParams.URLPrefix != null)
                        listenerparams.Add("URLPrefix", niParams.URLPrefix);
                    if (niParams.IsPortSpecified)
                        listenerparams.Add("Port", niParams.Port);
                    if (niParams.CertificateThumbPrint != null)
                        listenerparams.Add("CertificateThumbPrint", niParams.CertificateThumbPrint);
                    NewItemContainerListenerOrCertMapping(sessionobj, path, uri, host, listenerparams, WSManStringLiterals.containerListener, helper.GetResourceMsgFromResourcetext("NewItemShouldContinueListenerQuery"), helper.GetResourceMsgFromResourcetext("NewItemShouldContinueListenerCaption"));
                else if (path.EndsWith(strPathChk + WSManStringLiterals.containerClientCertificate, StringComparison.OrdinalIgnoreCase))
                    WSManProviderClientCertificateParameters dynParams = DynamicParameters as WSManProviderClientCertificateParameters;
                    Hashtable Certparams = new Hashtable();
                    Certparams.Add("Issuer", dynParams.Issuer);
                    Certparams.Add("Subject", dynParams.Subject);
                    Certparams.Add("Uri", dynParams.URI);
                    if (this.Credential.UserName != null)
                        System.Net.NetworkCredential nwCredentials = this.Credential.GetNetworkCredential();
                        Certparams.Add("UserName", nwCredentials.UserName);
                        Certparams.Add("Password", nwCredentials.Password);
                    Certparams.Add("Enabled", dynParams.Enabled);
                    NewItemContainerListenerOrCertMapping(sessionobj, path, uri, host, Certparams, WSManStringLiterals.containerClientCertificate, helper.GetResourceMsgFromResourcetext("NewItemShouldContinueClientCertQuery"), helper.GetResourceMsgFromResourcetext("NewItemShouldContinueClientCertCaption"));
                    AssertError(helper.GetResourceMsgFromResourcetext("NewItemNotSupported"), false);
        /// Dynamic parameter used by New-Item. According to different path. This method return the
        /// required dynamic parameters.
        protected override object NewItemDynamicParameters(string path, string itemTypeName, object newItemValue)
                return new WSManProviderNewItemComputerParameters();
            string strpathchk = hostname + WSManStringLiterals.DefaultPathSeparator;
            if (path.EndsWith(strpathchk + WSManStringLiterals.containerPlugin, StringComparison.OrdinalIgnoreCase))
                return new WSManProviderNewItemPluginParameters();
            if (path.EndsWith(WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerInitParameters, StringComparison.OrdinalIgnoreCase))
                return new WSManProviderInitializeParameters();
            if (path.EndsWith(WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerResources, StringComparison.OrdinalIgnoreCase))
                return new WSManProviderNewItemResourceParameters();
            if (path.EndsWith(WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerSecurity, StringComparison.OrdinalIgnoreCase))
                return new WSManProviderNewItemSecurityParameters();
            if (path.EndsWith(strpathchk + WSManStringLiterals.containerListener, StringComparison.OrdinalIgnoreCase))
                return new WSManProvidersListenerParameters();
            if (path.EndsWith(strpathchk + WSManStringLiterals.containerClientCertificate, StringComparison.OrdinalIgnoreCase))
                return new WSManProviderClientCertificateParameters();
        /// This method creates the connection to new machine in wsman provider.
        /// This is called from New-Item.
        /// <param name="Name"></param>
        private void NewItemCreateComputerConnection(string Name)
            helper = new WSManHelper(this);
            string parametersetName = "ComputerName";
            if (DynamicParameters is WSManProviderNewItemComputerParameters dynParams)
                if (dynParams.ConnectionURI != null)
                    parametersetName = "URI";
                helper.CreateWsManConnection(parametersetName, dynParams.ConnectionURI, dynParams.Port, Name, dynParams.ApplicationName, dynParams.UseSSL, dynParams.Authentication, dynParams.SessionOption, this.Credential, dynParams.CertificateThumbprint);
                    string[] constrsplit = dynParams.ConnectionURI.OriginalString.Split(":" + dynParams.Port + "/" + dynParams.ApplicationName, StringSplitOptions.None);
                    string[] constrsplit1 = constrsplit[0].Split("//", StringSplitOptions.None);
                    Name = constrsplit1[1].Trim();
                WriteItemObject(GetItemPSObjectWithTypeName(Name, WSManStringLiterals.ContainerChildValue, null, null, "ComputerLevel", WsManElementObjectTypes.WSManConfigContainerElement), WSManStringLiterals.rootpath + WSManStringLiterals.DefaultPathSeparator + Name, true);
                dynParams = new WSManProviderNewItemComputerParameters();
        /// This method creates the Listener or ClientCertificate in wsman provider.
        /// <param name="sessionobj"></param>
        /// <param name="uri"></param>
        /// <param name="host"></param>
        /// <param name="InputParams"></param>
        /// <param name="ContainerListenerOrCertMapping"></param>
        /// <param name="ShouldContinueQuery"></param>
        /// <param name="ShouldContinueCaption"></param>
        private void NewItemContainerListenerOrCertMapping(object sessionobj, string path, string uri, string host, Hashtable InputParams, string ContainerListenerOrCertMapping, string ShouldContinueQuery, string ShouldContinueCaption)
                if (!ShouldContinue(ShouldContinueQuery, ShouldContinueCaption))
            string inputstr = GetInputStringForCreate(uri, InputParams, host);
            CreateResourceValue(sessionobj, uri, inputstr, InputParams);
            XmlDocument xmlResource = GetResourceValue(sessionobj, uri, InputParams);
            Hashtable CCache = null;
            Hashtable kCache = null;
            if (ContainerListenerOrCertMapping.Equals(WSManStringLiterals.containerClientCertificate))
                ProcessCertMappingObjects(xmlResource, out CCache, out kCache);
            else if (ContainerListenerOrCertMapping.Equals(WSManStringLiterals.containerListener))
                ProcessListenerObjects(xmlResource, out CCache, out kCache);
            if (CCache != null && CCache.Count > 0)
                foreach (DictionaryEntry resource in CCache)
                    WriteItemObject(GetItemPSObjectWithTypeName(resource.Key.ToString(), WSManStringLiterals.ContainerChildValue, null, (string[])kCache[resource.Key], string.Empty, WsManElementObjectTypes.WSManConfigContainerElement), path + WSManStringLiterals.DefaultPathSeparator + resource.Key.ToString(), true);
        /// This method creates the Plugin and its child items in wsman provider.
        private void NewItemPluginOrPluginChild(object sessionobj, string path, string host, string uri)
            PSObject mshObj = new PSObject();
            string PluginName = string.Empty;
            string strPathChk = host + WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerPlugin;
            if (!path.Equals(strPathChk))
                int separatorindex = path.IndexOf(WSManStringLiterals.DefaultPathSeparator, strPathChk.Length + 1);
                if (separatorindex == -1)
                    PluginName = path.Substring(path.LastIndexOf(WSManStringLiterals.DefaultPathSeparator) + 1);
            // to create a new plugin
            if (path.EndsWith(strPathChk, StringComparison.OrdinalIgnoreCase))
                if (DynamicParameters is WSManProviderNewItemPluginParameters niParams)
                    if (string.IsNullOrEmpty(niParams.File))
                        mshObj.Properties.Add(new PSNoteProperty("Name", niParams.Plugin));
                        mshObj.Properties.Add(new PSNoteProperty("Filename", niParams.FileName));
                        mshObj.Properties.Add(new PSNoteProperty("Resource", niParams.Resource));
                        mshObj.Properties.Add(new PSNoteProperty("SDKVersion", niParams.SDKVersion));
                        mshObj.Properties.Add(new PSNoteProperty("Capability", niParams.Capability));
                        if (niParams.RunAsCredential != null)
                            mshObj.Properties.Add(new PSNoteProperty(WSManStringLiterals.ConfigRunAsUserName, niParams.RunAsCredential.UserName));
                            mshObj.Properties.Add(new PSNoteProperty(WSManStringLiterals.ConfigRunAsPasswordName, niParams.RunAsCredential.Password));
                        if (niParams.AutoRestart)
                            mshObj.Properties.Add(new PSNoteProperty(WSManStringLiterals.ConfigAutoRestart, niParams.AutoRestart));
                        if (niParams.ProcessIdleTimeoutSec.HasValue)
                            mshObj.Properties.Add(new PSNoteProperty(WSManStringLiterals.ConfigProcessIdleTimeoutSec, niParams.ProcessIdleTimeoutSec.Value));
                        if (niParams.UseSharedProcess)
                            mshObj.Properties.Add(new PSNoteProperty(WSManStringLiterals.ConfigUseSharedProcess, niParams.UseSharedProcess));
                        if (niParams.XMLRenderingType != null)
                            mshObj.Properties.Add(new PSNoteProperty("XmlRenderingType", niParams.XMLRenderingType));
                            mshObj.Properties.Add(new PSNoteProperty("XmlRenderingType", "Text"));
                        inputStr = ConstructPluginXml(mshObj, uri, host, "New", null, null, null);
                        PluginName = niParams.Plugin;
                        inputStr = ReadFile(niParams.File);
                    ErrorRecord er = new ErrorRecord(new InvalidOperationException(helper.GetResourceMsgFromResourcetext("NewItemNotSupported")), "InvalidOperationException", ErrorCategory.InvalidOperation, null);
                string filter = uri + "?Name=" + PluginName;
                CreateResourceValue(sessionobj, filter, inputStr, null);
                WriteItemObject(GetItemPSObjectWithTypeName(PluginName, WSManStringLiterals.ContainerChildValue, null, new string[] { "Name=" + PluginName }, string.Empty, WsManElementObjectTypes.WSManConfigContainerElement), path + WSManStringLiterals.DefaultPathSeparator + PluginName, true);
            {// to create an internal item of as plugin
                string pName = string.Empty;
                string NewItem = string.Empty;
                string[] Keys = null;
                int pos = path.LastIndexOf(strPathChk + WSManStringLiterals.DefaultPathSeparator, StringComparison.OrdinalIgnoreCase) + strPathChk.Length + 1;
                string filter = uri + "?Name=" + pName;
                XmlDocument pxml = GetResourceValue(sessionobj, filter, null);
                strPathChk = strPathChk + WSManStringLiterals.DefaultPathSeparator + pName + WSManStringLiterals.DefaultPathSeparator;
                    strPathChk += WSManStringLiterals.containerResources;
                        if (DynamicParameters is WSManProviderNewItemResourceParameters niParams)
                            mshObj.Properties.Add(new PSNoteProperty("Resource", niParams.ResourceUri));
                            inputStr = ConstructResourceXml(mshObj, null, null);
                            XmlDocument xdoc = new XmlDocument();
                            xdoc.LoadXml(inputStr);
                            ArrayList arrList = null;
                            ArrayList NewResource = ProcessPluginResourceLevel(xdoc, out arrList);
                            NewItem = ((PSObject)NewResource[0]).Properties["ResourceDir"].Value.ToString();
                            Keys = new string[] { "Uri=" + ((PSObject)NewResource[0]).Properties["ResourceURI"].Value.ToString() };
                            if (ResourceArray != null)
                                ResourceArray.Add(NewResource[0]);
                                ResourceArray = NewResource;
                    if (Sepindex != -1)
                        sResourceDirName = path.Substring(strPathChk.Length + 1, path.IndexOf(WSManStringLiterals.DefaultPathSeparator, strPathChk.Length + 1) - (strPathChk.Length + 1));
                    strPathChk = strPathChk + WSManStringLiterals.DefaultPathSeparator + sResourceDirName;
                    if (path.EndsWith(strPathChk + WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerSecurity, StringComparison.OrdinalIgnoreCase))
                            query = string.Format(CultureInfo.CurrentCulture, query, pName);
                        // Construct the Uri from Resource_XXXX resource dir.
                        PSObject resourceDirProperties = GetItemValue(strPathChk);
                        if ((resourceDirProperties == null) || (resourceDirProperties.Properties["ResourceUri"] == null))
                            string message = helper.FormatResourceMsgFromResourcetext("ResourceURIMissingInResourceDir",
                                "ResourceUri", strPathChk);
                            AssertError(message, false);
                            return; // AssertError is going to throw - return silences some static analysis tools
                        WSManProviderNewItemSecurityParameters niParams = DynamicParameters as WSManProviderNewItemSecurityParameters;
                        mshObj.Properties.Add(new PSNoteProperty("Uri", resourceDirProperties.Properties["ResourceUri"].Value));
                        mshObj.Properties.Add(new PSNoteProperty("Sddl", niParams.Sddl));
                        inputStr = ConstructSecurityXml(mshObj, null, string.Empty);
                        ArrayList newSecurity = new ArrayList();
                        newSecurity = ProcessPluginSecurityLevel(newSecurity, xdoc, sResourceDirName, resourceDirProperties.Properties["ResourceUri"].Value.ToString());
                        NewItem = ((PSObject)newSecurity[0]).Properties["SecurityDIR"].Value.ToString();
                        Keys = new string[] { "Uri=" + ((PSObject)newSecurity[0]).Properties["Uri"].Value.ToString() };
                        if (SecurityArray != null)
                            SecurityArray.Add(newSecurity[0]);
                            SecurityArray = newSecurity;
                if (path.EndsWith(strPathChk + WSManStringLiterals.containerInitParameters, StringComparison.OrdinalIgnoreCase))
                    WSManProviderInitializeParameters niParams = DynamicParameters as WSManProviderInitializeParameters;
                    mshObj.Properties.Add(new PSNoteProperty(niParams.ParamName, niParams.ParamValue));
                    inputStr = ConstructInitParamsXml(mshObj, null);
                    ArrayList newInitParam = ProcessPluginInitParamLevel(xdoc);
                    NewItem = niParams.ParamName;
                    if (InitParamArray != null)
                        InitParamArray.Add(newInitParam[0]);
                        InitParamArray = ProcessPluginInitParamLevel(xdoc);
                    ((IWSManSession)sessionobj).Put(uri + "?" + "Name=" + pName, inputStr, 0);
                        WriteItemObject(GetItemPSObjectWithTypeName(mshObj.Properties[NewItem].Name, mshObj.Properties[NewItem].TypeNameOfValue, mshObj.Properties[NewItem].Value, null, "InitParams", WsManElementObjectTypes.WSManConfigLeafElement), path + WSManStringLiterals.DefaultPathSeparator + mshObj.Properties[NewItem].Name, false);
                        WriteItemObject(GetItemPSObjectWithTypeName(NewItem, WSManStringLiterals.ContainerChildValue, null, Keys, string.Empty, WsManElementObjectTypes.WSManConfigContainerElement), path + WSManStringLiterals.DefaultPathSeparator + NewItem, true);
        /// Returns the object to be written to the console.
        /// <param name="TypeNameOfElement"></param>
        /// <param name="Value"></param>
        /// <param name="keys"></param>
        /// <param name="ExtendedTypeName"></param>
        /// <param name="WSManElementObjectType"></param>
        private static PSObject GetItemPSObjectWithTypeName(string Name, string TypeNameOfElement, object Value, string[] keys, string ExtendedTypeName, WsManElementObjectTypes WSManElementObjectType, PSObject input = null)
            if (WSManElementObjectType.Equals(WsManElementObjectTypes.WSManConfigElement))
                WSManConfigElement element = new WSManConfigElement(Name, TypeNameOfElement);
                mshObject = new PSObject(element);
            if (WSManElementObjectType.Equals(WsManElementObjectTypes.WSManConfigContainerElement))
                WSManConfigContainerElement element = new WSManConfigContainerElement(Name, TypeNameOfElement, keys);
            if (WSManElementObjectType.Equals(WsManElementObjectTypes.WSManConfigLeafElement))
                object source = null;
                if (input != null)
                    string sourceProp = Name + WSManStringLiterals.HiddenSuffixForSourceOfValue;
                    if (input.Properties[sourceProp] != null)
                        source = input.Properties[sourceProp].Value;
                WSManConfigLeafElement element = new WSManConfigLeafElement(Name, Value, TypeNameOfElement, source);
            if (!string.IsNullOrEmpty(ExtendedTypeName))
                StringBuilder types = new StringBuilder(string.Empty);
                    types.Append(mshObject.ImmediateBaseObject.GetType().FullName);
                    types.Append('#');
                    types.Append(ExtendedTypeName);
                    mshObject.TypeNames.Insert(0, types.ToString());
            return mshObject;
        /// Called from setitem. This set the value in Listener and Client Certificate container.
        /// <param name="sessionObj"></param>
        /// <param name="ResourceURI"></param>
        /// <param name="PrimaryKeys"></param>
        /// <param name="childName"></param>
        private void SetItemListenerOrClientCertificate(object sessionObj, string ResourceURI, string[] PrimaryKeys, string childName, object value, string path, string parent, string host)
            Hashtable objcache = null;
            Hashtable Keyscache = null;
            XmlDocument xmlResource = EnumerateResourceValue(sessionObj, ResourceURI);
                AssertError(helper.GetResourceMsgFromResourcetext("InvalidPath"), false);
            if (ResourceURI.EndsWith(WSManStringLiterals.containerListener, StringComparison.OrdinalIgnoreCase))
                ProcessListenerObjects(xmlResource, out objcache, out Keyscache);
            else if (ResourceURI.EndsWith(WSManStringLiterals.containerCertMapping, StringComparison.OrdinalIgnoreCase))
                ProcessCertMappingObjects(xmlResource, out objcache, out Keyscache);
            if (path.EndsWith(host + WSManStringLiterals.DefaultPathSeparator + parent, StringComparison.OrdinalIgnoreCase))
                    if (!ShouldContinue(helper.GetResourceMsgFromResourcetext("SetItemShouldContinueQuery"), helper.GetResourceMsgFromResourcetext("SetItemShouldContinueCaption")))
                string item = path.Substring(path.LastIndexOf(WSManStringLiterals.DefaultPathSeparator) + 1);
                    cmdlinevalue.Add(childName, value);
                    foreach (string key in PrimaryKeys)
                        cmdlinevalue.Add(key, ((PSObject)objcache[item]).Properties[key].Value);
                    PutResourceValue(sessionObj, ResourceURI, cmdlinevalue, host);
                    ErrorRecord er = new ErrorRecord(e, "COMException", ErrorCategory.InvalidOperation, null);
        /// Get the input string for create.
        private static string GetInputStringForCreate(string ResourceURI, Hashtable value, string host)
            string putstr = string.Empty;
            string nilns = string.Empty;
            StringBuilder sbvalues = new StringBuilder();
            if (value.Count > 0)
                foreach (string key in value.Keys)
                    if (!IsPKey(key, ResourceURI))
                        sbvalues.Append("<p:");
                        sbvalues.Append(key);
                        if (value[key] == null)
                            sbvalues.Append(' ');
                            sbvalues.Append(WSManStringLiterals.ATTR_NIL);
                            nilns = " " + WSManStringLiterals.NS_XSI;
                        sbvalues.Append('>');
                        sbvalues.Append(EscapeValuesForXML(((Hashtable)value)[key].ToString()));
                        sbvalues.Append("</p:");
            string root = GetRootNodeName(ResourceURI);
            putstr = "<p:" + root + " " + "xmlns:p=\"" + SetSchemaPath(ResourceURI) + ".xsd\"" + nilns + ">" + sbvalues.ToString() + "</p:" + root + ">";
            return putstr;
        /// Reads the file. used by New-Item for Plugin creation from file.
        private string ReadFile(string path)
                string filePath = this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(path);
                putstr = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            // known exceptions
                ErrorRecord er = new ErrorRecord(e, "ArgumentNullException", ErrorCategory.InvalidOperation, null);
                ErrorRecord er = new ErrorRecord(e, "UnauthorizedAccessException", ErrorCategory.InvalidOperation, null);
                ErrorRecord er = new ErrorRecord(e, "NotSupportedException", ErrorCategory.InvalidOperation, null);
            catch (FileNotFoundException e)
                ErrorRecord er = new ErrorRecord(e, "FileNotFoundException", ErrorCategory.InvalidOperation, null);
            catch (DirectoryNotFoundException e)
                ErrorRecord er = new ErrorRecord(e, "DirectoryNotFoundException", ErrorCategory.InvalidOperation, null);
                ErrorRecord er = new ErrorRecord(e, "SecurityException", ErrorCategory.InvalidOperation, null);
        /// Get the host name or the machine to which connected.
        /// This is used by most of the methods like GetItem,GetChildItem, NewItem, SetItem,RemoveItem etc...
        private string GetHostName(string path)
            string sHostname = path;
                // HostName is always followed by root name
                    sHostname = path.Substring(0, path.IndexOf(WSManStringLiterals.DefaultPathSeparator));
                if (!SessionObjCache.ContainsKey(sHostname))
                    sHostname = null;
                ErrorRecord er = new ErrorRecord(e, "ArgumentNullException", ErrorCategory.InvalidArgument, null);
            return sHostname;
        private static string GetRootNodeName(string ResourceURI)
            string tempuri = string.Empty;
            if (ResourceURI.Contains('?'))
                ResourceURI = ResourceURI.Split('?').GetValue(0).ToString();
            const string PTRN_URI_LAST = "([a-z_][-a-z0-9._]*)$";
            Regex objregex = new Regex(PTRN_URI_LAST, RegexOptions.IgnoreCase);
            MatchCollection regexmatch = objregex.Matches(ResourceURI);
            if (regexmatch.Count > 0)
                tempuri = regexmatch[0].Value;
            return tempuri;
        private static string EscapeValuesForXML(string value)
            StringBuilder esc_str = new StringBuilder();
            for (int i = 0; i <= value.Length - 1; i++)
                switch (value[i])
                            esc_str.Append("&amp;");
                            esc_str.Append("&lt;");
                            esc_str.Append("&gt;");
                            esc_str.Append("&quot;");
                            esc_str.Append("&apos;");
                            esc_str.Append(value[i]);
            return esc_str.ToString();
        private static bool IsItemContainer(XmlNodeList nodes)
            if (nodes.Count != 0)
                if (nodes[0].ChildNodes.Count != 0)
                    if (!nodes[0].FirstChild.Name.Equals("#text", StringComparison.OrdinalIgnoreCase))
        private XmlNodeList SearchXml(XmlDocument resourcexmldocument, string searchitem, string ResourceURI, string path, string host)
            XmlNodeList nodes = null;
                if (ResourceURI.EndsWith(WSManStringLiterals.containerWinrs, StringComparison.OrdinalIgnoreCase))
                    if (path.Equals(host + WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerShell, StringComparison.OrdinalIgnoreCase))
                        searchitem = WSManStringLiterals.containerWinrs.ToLowerInvariant();
                if (ResourceURI.EndsWith("Config", StringComparison.OrdinalIgnoreCase) || !ResourceURI.EndsWith(searchitem, StringComparison.OrdinalIgnoreCase))
                    xpathString = @"/*/*[local-name()=""" + searchitem.ToLowerInvariant() + @"""]";
                    xpathString = @"/*[local-name()=""" + searchitem.ToLowerInvariant() + @"""]";
                nodes = resourcexmldocument.SelectNodes(xpathString);
            catch (System.Xml.XPath.XPathException ex)
                ErrorRecord er = new ErrorRecord(ex, "XPathException", ErrorCategory.InvalidArgument, null);
            return nodes;
        #region "WsMan linking Operations"
        /// To put a resource value. Wsman put operation.
        private void PutResourceValue(object sessionobj, string ResourceURI, Hashtable value, string host)
            XmlDocument inputxml = null;
                inputxml = GetResourceValue(sessionobj, ResourceURI, value);
                if (inputxml != null)
                    bool Itemfound = false;
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(inputxml.NameTable);
                    string uri_schema = SetSchemaPath(ResourceURI);
                    nsmgr.AddNamespace("cfg", uri_schema);
                    string xpath = SetXPathString(ResourceURI);
                    XmlNodeList nodelist = inputxml.SelectNodes(xpath, nsmgr);
                    if (nodelist != null && nodelist.Count == 1)
                        XmlNode node = (XmlNode)nodelist.Item(0);
                        if (node.HasChildNodes)
                            for (int i = 0; i < node.ChildNodes.Count; i++)
                                if ((node.ChildNodes[i].ChildNodes.Count == 0) || node.ChildNodes[i].FirstChild.Name.Equals("#text", StringComparison.OrdinalIgnoreCase))
                                        // to make sure we dont set values at inner level.
                                        // for eg: when set-item at winrm/config we dont take input at below level
                                            if (node.ChildNodes[i].LocalName.Equals(key, StringComparison.OrdinalIgnoreCase))
                                                node.ChildNodes[i].InnerText = value[key].ToString();
                                                Itemfound = true;
                            if (Itemfound)
                                ResourceURI = GetURIWithFilter(ResourceURI, value);
                                ((IWSManSession)sessionobj).Put(ResourceURI, node.OuterXml, 0);
        private string GetResourceValueInXml(object sessionobj, string ResourceURI, Hashtable cmdlinevalues)
                ResourceURI = GetURIWithFilter(ResourceURI, cmdlinevalues);
                string returnValue = string.Empty;
                if (!this.getMapping.TryGetValue(ResourceURI, out returnValue))
                    returnValue = ((IWSManSession)sessionobj).Get(ResourceURI, 0);
                    this.getMapping.Add(ResourceURI, returnValue);
        /// WSMan Get operation.
        /// <param name="cmdlinevalues"></param>
        private XmlDocument GetResourceValue(object sessionobj, string ResourceURI, Hashtable cmdlinevalues)
            XmlDocument xmlResource = null;
            string strValueXml = this.GetResourceValueInXml(sessionobj, ResourceURI, cmdlinevalues);
            xmlResource = new XmlDocument();
            xmlResource.LoadXml(strValueXml);
            return xmlResource;
        /// WsMan Enumerate operation.
        private XmlDocument EnumerateResourceValue(object sessionobj, string ResourceURI)
            XmlDocument xmlEnumResources = null;
            if (!this.enumerateMapping.TryGetValue(ResourceURI, out xmlEnumResources))
                    object value = ((IWSManSession)sessionobj).Enumerate(ResourceURI, string.Empty, string.Empty, 0);
                    string strXmlValue = string.Empty;
                    while (!((IWSManEnumerator)value).AtEndOfStream)
                        strXmlValue += ((IWSManEnumerator)value).ReadItem();
                    Marshal.ReleaseComObject(value);
                    if (!string.IsNullOrEmpty(strXmlValue))
                        xmlEnumResources = new XmlDocument();
                        strXmlValue = "<WsManResults>" + strXmlValue + "</WsManResults>";
                        xmlEnumResources.LoadXml(strXmlValue);
                        this.enumerateMapping.Add(ResourceURI, xmlEnumResources);
            return xmlEnumResources;
        /// WsMan Delete Operation.
        private void DeleteResourceValue(object sessionobj, string ResourceURI, Hashtable cmdlinevalues, bool recurse)
                // Support only for Listener,plugin and ClientCertificate.
                if (ResourceURI.Contains(WSManStringLiterals.containerPlugin) || ResourceURI.Contains(WSManStringLiterals.containerListener) || ResourceURI.Contains(WSManStringLiterals.containerCertMapping))
                    if (cmdlinevalues != null)
                    ((IWSManSession)sessionobj).Delete(ResourceURI, 0);
        /// WsMan Create Operation.
        /// <param name="resource"></param>
        private void CreateResourceValue(object sessionobj, string ResourceURI, string resource, Hashtable cmdlinevalues)
                if (ResourceURI.Contains(WSManStringLiterals.containerListener) || ResourceURI.Contains(WSManStringLiterals.containerPlugin) || ResourceURI.Contains(WSManStringLiterals.containerCertMapping))
                    ((IWSManSession)sessionobj).Create(ResourceURI, resource, 0);
        /// To find a resource value. Both Get and Enumerate works here.
        private XmlDocument FindResourceValue(object sessionobj, string ResourceURI, Hashtable cmdlinevalues)
            XmlDocument outval = null;
                if (cmdlinevalues == null || cmdlinevalues.Count == 0)
                    outval = EnumerateResourceValue(sessionobj, ResourceURI);
                    outval = GetResourceValue(sessionobj, ResourceURI, cmdlinevalues);
            return outval;
        /// Checks whether a value is present in Wsman config.
        /// <param name="childname"></param>
        private bool ContainResourceValue(object sessionobj, string ResourceURI, string childname, string path, string host)
            string valuexml = string.Empty;
                        valuexml += ((IWSManEnumerator)value).ReadItem();
                    if ((valuexml != string.Empty) && !(string.IsNullOrEmpty(valuexml)))
                        valuexml = "<WsManResults>" + valuexml + "</WsManResults>";
                    valuexml = this.GetResourceValueInXml(((IWSManSession)sessionobj), ResourceURI, null);
                if (string.IsNullOrEmpty(valuexml))
                xmlResourceValues.LoadXml(valuexml.ToLowerInvariant());
                XmlNodeList nodes = SearchXml(xmlResourceValues, childname, ResourceURI, path, host);
                if (nodes.Count > 0)
            catch (COMException) { result = false; }
        #endregion "WsMan linking Operations"
        private static string GetURIWithFilter(string uri, Hashtable cmdlinevalues)
            StringBuilder sburi = new StringBuilder(uri);
                if (uri.Contains("Config/Listener"))
                    sburi.Append('?');
                    sburi.Append(GetFilterString(cmdlinevalues, PKeyListener));
                else if (uri.Contains("Config/Service/certmapping"))
                    sburi.Append(GetFilterString(cmdlinevalues, PKeyCertMapping));
                else if (uri.Contains("Config/Plugin"))
                    sburi.Append(GetFilterString(cmdlinevalues, PKeyPlugin));
            return sburi.ToString();
        private static string GetFilterString(Hashtable cmdlinevalues, string[] pkey)
            StringBuilder filter = new StringBuilder();
            foreach (string key in pkey)
                if (cmdlinevalues.Contains(key))
                    filter.Append(key);
                    filter.Append('=');
                    filter.Append(cmdlinevalues[key].ToString());
                    filter.Append('+');
            if (filter.ToString().EndsWith('+'))
                filter.Remove(filter.ToString().Length - 1, 1);
            return filter.ToString();
        private static bool IsPKey(string value, string ResourceURI)
            if (ResourceURI.Contains(WSManStringLiterals.containerListener))
                result = CheckPkeysArray(null, value, PKeyListener);
            else if (ResourceURI.Contains(WSManStringLiterals.containerPlugin))
                result = CheckPkeysArray(null, value, PKeyPlugin);
            else if (ResourceURI.Contains(WSManStringLiterals.containerCertMapping))
                result = CheckPkeysArray(null, value, PKeyCertMapping);
        private static bool CheckPkeysArray(Hashtable values, string value, string[] pkeys)
            if (values != null)
                foreach (string key in pkeys)
                    if (values.Contains(key))
            else if (!string.IsNullOrEmpty(value))
                    if (key.Equals(value, StringComparison.OrdinalIgnoreCase))
        private void WritePSObjectPropertyNames(PSObject psobject, string path)
            foreach (PSPropertyInfo prop in psobject.Properties)
                if (!prop.Value.ToString().Equals(WSManStringLiterals.ContainerChildValue))
                    WriteItemObject(prop.Name, path + WSManStringLiterals.DefaultPathSeparator + prop.Name, true);
                    WriteItemObject(prop.Name, path + WSManStringLiterals.DefaultPathSeparator + prop.Name, false);
        /// Used to Write WSMan objects to the output console. Used by GetChildItem, GetItem and NewItem.
        /// <param name="psobject"></param>
        private void WritePSObjectPropertiesAsWSManElementObjects(PSObject psobject, string path, string[] keys, string ExtendedTypeName, WsManElementObjectTypes WSManElementObjectType, bool recurse)
            Collection<string> directory = new Collection<string>();
                if (prop.Name.EndsWith(WSManStringLiterals.HiddenSuffixForSourceOfValue))
                    WSManConfigElement element = new WSManConfigElement(prop.Name, prop.Value.ToString());
                    WSManConfigContainerElement element = new WSManConfigContainerElement(prop.Name, prop.Value.ToString(), keys);
                    string sourceProp = prop.Name + WSManStringLiterals.HiddenSuffixForSourceOfValue;
                    if (psobject.Properties[sourceProp] != null)
                        source = psobject.Properties[sourceProp].Value;
                    WSManConfigLeafElement element = null;
                        element = new WSManConfigLeafElement(prop.Name, prop.Value, prop.TypeNameOfValue, source);
                        element = new WSManConfigLeafElement(prop.Name, null, prop.Value.ToString());
                    if (element != null)
                    // This path is used by WriteItemObject to construct PSPath.
                    // PSPath is a provider qualified path and we dont need to specify
                    // provider root in this path..So I am trying to eliminate provider root
                    // in this case.
                    string pathToUse = WSManStringLiterals.rootpath.Equals(path, StringComparison.OrdinalIgnoreCase) ?
                        prop.Name :
                        (path + WSManStringLiterals.DefaultPathSeparator + prop.Name);
                    WriteItemObject(mshObject, pathToUse, false);
                    WriteItemObject(mshObject, pathToUse, true);
                        directory.Add(prop.Name);
                foreach (string dir in directory)
                    GetChildItemsRecurse(path, dir, ProviderMethods.GetChildItems, recurse);
        private string SplitAndUpdateStringUsingDelimiter(object sessionobj, string uri, string childname, string value, string Delimiter)
            XmlDocument xmlResource = GetResourceValue(sessionobj, uri, null);
            string existingvalue = string.Empty;
                    existingvalue = mshObject.Properties[childname].Value.ToString();
                if (!string.IsNullOrEmpty(existingvalue))
                    string[] existingsplitvalues = existingvalue.Split(Delimiter, StringSplitOptions.None);
                    string[] newvalues = value.Split(Delimiter, StringSplitOptions.None);
                    foreach (string val in newvalues)
                        if (Array.IndexOf(existingsplitvalues, val) == -1)
                            existingvalue += Delimiter + val;
                    existingvalue = value;
            return existingvalue;
        /// For Host Level or WsMan level
        /// WsMan\Localhost:\>
        /// WsMan:\>
        /// <param name="objSessionObject"></param>
        /// <param name="IsWsManLevel"></param>
        private PSObject BuildHostLevelPSObjectArrayList(object objSessionObject, string uri, bool IsWsManLevel)
            PSObject mshobject = new PSObject();
            if (IsWsManLevel)
                foreach (string key in SessionObjCache.Keys)
                    mshobject.Properties.Add(new PSNoteProperty(key, WSManStringLiterals.ContainerChildValue));
                if (objSessionObject != null)
                    XmlDocument ConfigXml = GetResourceValue(objSessionObject, uri, null);
                    // Moving in to <Config>
                    foreach (XmlNode node in ConfigXml.ChildNodes)
                        foreach (XmlNode node1 in node.ChildNodes)
                            // Getting Top Element in <Config>
                            if ((node1.ChildNodes.Count == 0) || node1.FirstChild.Name.Equals("#text", StringComparison.OrdinalIgnoreCase))
                                mshobject.Properties.Add(new PSNoteProperty(node1.LocalName, node1.InnerText));
                // Getting the Fixed root nodes.
                foreach (string root in WinRmRootConfigs)
                    mshobject.Properties.Add(new PSNoteProperty(root, WSManStringLiterals.ContainerChildValue));
            return mshobject;
        /// Converts XmlNodes ChildNodes to Properties of PSObject.
        /// <param name="xmlnode"></param>
        private static PSObject ConvertToPSObject(XmlNode xmlnode)
            PSObject mshObject = new PSObject();
            foreach (XmlNode node in xmlnode.ChildNodes)
                // If node contains 0 child-nodes, it is empty node, if it's name = "#text" then it's a simple node.
                if ((node.ChildNodes.Count == 0) || node.FirstChild.Name.Equals("#text", StringComparison.OrdinalIgnoreCase))
                    XmlAttribute attrSource = null;
                    foreach (XmlAttribute attr in node.Attributes)
                        if (attr.LocalName.Equals("Source", StringComparison.OrdinalIgnoreCase))
                            attrSource = attr;
                    mshObject.Properties.Add(new PSNoteProperty(node.LocalName, node.InnerText));
                    if (attrSource != null)
                        string propName = node.LocalName + WSManStringLiterals.HiddenSuffixForSourceOfValue;
                        mshObject.Properties.Remove(propName);
                        mshObject.Properties.Add(new PSNoteProperty(propName, attrSource.Value));
                    mshObject.Properties.Add(new PSNoteProperty(node.LocalName, WSManStringLiterals.ContainerChildValue));
        private static string SetXPathString(string uri)
            string parent = uri.Substring(uri.LastIndexOf(WSManStringLiterals.WinrmPathSeparator.ToString(), StringComparison.OrdinalIgnoreCase) + 1);
            if (parent.Equals(WSManStringLiterals.containerWinrs, StringComparison.OrdinalIgnoreCase))
                parent = WSManStringLiterals.containerWinrs;
            else if (parent.Equals(WSManStringLiterals.containerAuth, StringComparison.OrdinalIgnoreCase))
                parent = WSManStringLiterals.containerAuth;
            else if (parent.Equals("certmapping", StringComparison.OrdinalIgnoreCase))
                parent = "CertMapping";
            else if (parent.Equals(WSManStringLiterals.containerService, StringComparison.OrdinalIgnoreCase))
                parent = WSManStringLiterals.containerService;
            else if (parent.Equals(WSManStringLiterals.containerDefaultPorts, StringComparison.OrdinalIgnoreCase))
                parent = WSManStringLiterals.containerDefaultPorts;
            else if (parent.Equals(WSManStringLiterals.containerPlugin, StringComparison.OrdinalIgnoreCase))
                parent = WSManStringLiterals.containerPlugin;
            parent = "/cfg:" + parent;
            return parent;
        private static string SetSchemaPath(string uri)
            string schemapath = string.Empty;
            uri = uri.Remove(0, WinrmRootName[0].Length);
            if (uri.Contains(WSManStringLiterals.containerPlugin))
                schemapath = WSManStringLiterals.WsMan_Schema + "/plugin";
            else if (uri.Contains(WSManStringLiterals.containerClientCertificate))
                uri = uri.Replace(WSManStringLiterals.containerClientCertificate, "/service/certmapping");
                schemapath = WSManStringLiterals.WsMan_Schema + uri.ToLowerInvariant();
            return schemapath;
        /// Get the Uri for the operation from a path.
        private static string NormalizePath(string path, string host)
            string uri = string.Empty;
            if (path.StartsWith(host, StringComparison.OrdinalIgnoreCase))
                    path = path.TrimEnd(WSManStringLiterals.DefaultPathSeparator);
                if (path.Equals(host, StringComparison.OrdinalIgnoreCase))
                    uri = WinrmRootName[0];
                uri = path.Substring(host.Length);
                uri = uri.Replace(WSManStringLiterals.DefaultPathSeparator, WSManStringLiterals.WinrmPathSeparator);
                string host_prefix = host + WSManStringLiterals.DefaultPathSeparator;
                if (path.StartsWith(host_prefix + WSManStringLiterals.containerClientCertificate, StringComparison.OrdinalIgnoreCase))
                    uri = WinrmRootName[0] + WSManStringLiterals.WinrmPathSeparator + WSManStringLiterals.containerCertMapping;
                else if (path.StartsWith(host_prefix + WSManStringLiterals.containerPlugin, StringComparison.OrdinalIgnoreCase))
                    uri = WinrmRootName[0] + WSManStringLiterals.WinrmPathSeparator + WSManStringLiterals.containerPlugin;
                else if (path.StartsWith(host_prefix + WSManStringLiterals.containerShell, StringComparison.OrdinalIgnoreCase))
                    uri = WinrmRootName[0] + WSManStringLiterals.WinrmPathSeparator + WSManStringLiterals.containerWinrs;
                else if (path.StartsWith(host_prefix + WSManStringLiterals.containerListener, StringComparison.OrdinalIgnoreCase))
                    uri = WinrmRootName[0] + WSManStringLiterals.WinrmPathSeparator + WSManStringLiterals.containerListener;
                    if (!(path.Equals(host_prefix + WSManStringLiterals.containerService, StringComparison.OrdinalIgnoreCase)
                        || path.Equals(host_prefix + WSManStringLiterals.containerClient, StringComparison.OrdinalIgnoreCase) || path.EndsWith(WSManStringLiterals.containerDefaultPorts, StringComparison.OrdinalIgnoreCase)
                           || path.EndsWith(WSManStringLiterals.containerAuth, StringComparison.OrdinalIgnoreCase)))
                        int index = uri.LastIndexOf(WSManStringLiterals.WinrmPathSeparator);
                            uri = uri.Remove(index);
                    uri = WinrmRootName[0] + uri;
        /// Given wsman config path, gets the value of the leaf present.
        /// If path is not valid or not present throws an exception.
        /// Currently this supports only retrieving Resource_XXXX dir contents.
        /// if you need support at other levels implement them.
        /// Example resource dir: WSMan:\localhost\Plugin\someplugin\Resources\Resource_XXXXXXX.
        /// A PSObject representing the contents of the path if successful,
        /// Otherwise null.
        /// 1. path is null or empty.
        /// <exception cref="NotSupportedException">
        /// 1. <paramref name="path"/> should be of form
        ///    WSMan:\localhost\Plugin\someplugin\Resources\Resource_XXXXXXX
        ///    Other paths are not supported currently. If you want you can
        ///    add them later.
        private PSObject GetItemValue(string path)
            ArgumentException.ThrowIfNullOrEmpty(path);
                throw new InvalidOperationException("InvalidPath");
            if (IsPathLocalMachine(host) && (!IsWSManServiceRunning()))
                AssertError("WinRMServiceError", false);
                // gets the sessionobject
                // Normalize to the required uri
                string strPathchk = host + WSManStringLiterals.DefaultPathSeparator;
                if (path.EndsWith(host, StringComparison.OrdinalIgnoreCase))
                    PSObject result = BuildHostLevelPSObjectArrayList(sessionobj, uri, false);
                // Get the XML for the resource path we are looking for.
                XmlDocument outxml = FindResourceValue(sessionobj, uri, null);
                if (outxml == null || !outxml.HasChildNodes)
                if (path.Contains(strPathchk + WSManStringLiterals.containerListener))
                    // Implement the necessary functionality here when needed.
                else if (path.Contains(strPathchk + WSManStringLiterals.containerClientCertificate))
                else if (path.Contains(strPathchk + WSManStringLiterals.containerPlugin))
                    GetPluginNames(outxml, out objPluginNames, out currentpluginname, path);
                    if (path.EndsWith(strPathchk + WSManStringLiterals.containerPlugin, StringComparison.OrdinalIgnoreCase))
                        return objPluginNames;
                        // Currently this supports only retrieving Resource_XXXX dir contents.
                        // if you need support at other levels implement them.
                        // Example resource dir: WSMan:\localhost\Plugin\someplugin\Resources\Resource_67830040
                        PSObject objPluginlevel = ProcessPluginConfigurationLevel(CurrentPluginXML);
                        strPathchk = strPathchk + WSManStringLiterals.containerPlugin + WSManStringLiterals.DefaultPathSeparator;
                        strPathchk = strPathchk + currentpluginname + WSManStringLiterals.DefaultPathSeparator;
                        // We support only retrieving Resource_XXX dir properties only.
                        // other directory support can be added as needed.
                        strPathchk = strPathchk + WSManStringLiterals.containerResources + WSManStringLiterals.DefaultPathSeparator;
                        int Sepindex = path.IndexOf(WSManStringLiterals.DefaultPathSeparator, strPathchk.Length);
                            sResourceDirName = path.Substring(strPathchk.Length);
                            sResourceDirName = path.Substring(strPathchk.Length,
                                path.IndexOf(WSManStringLiterals.DefaultPathSeparator, strPathchk.Length) - (strPathchk.Length));
                        if (path.EndsWith(strPathchk + sResourceDirName, StringComparison.OrdinalIgnoreCase))
                                if (sResourceDirName.Equals(p.Properties["ResourceDir"].Value.ToString()))
        private string GetCorrectCaseOfPath(string path)
            if (!path.Contains(WSManStringLiterals.DefaultPathSeparator.ToString()))
                return GetChildName(path);
            string[] splitpath = path.Split(WSManStringLiterals.DefaultPathSeparator);
            StringBuilder sbPath = new StringBuilder();
            StringBuilder tempPath = new StringBuilder();
            foreach (string strpath in splitpath)
                    tempPath.Append(GetChildName(strpath));
                    sbPath.Append(tempPath);
                    tempPath.Append(WSManStringLiterals.DefaultPathSeparator);
                    tempPath.Append(strpath);
                    sbPath.Append(WSManStringLiterals.DefaultPathSeparator);
                    sbPath.Append(GetChildName(tempPath.ToString()));
            return sbPath.ToString();
        private string GetCorrectCaseOfName(string ChildName, string hostname, string path)
            string result = ChildName;
            if (ChildName != null)
                if (!ChildName.Contains('_'))
                    if (ChildName.Equals(WSManStringLiterals.containerQuotasParameters, StringComparison.OrdinalIgnoreCase))
                        result = WSManStringLiterals.containerQuotasParameters;
                    else if (ChildName.Equals(WSManStringLiterals.containerPlugin, StringComparison.OrdinalIgnoreCase))
                        result = WSManStringLiterals.containerPlugin;
                    else if (ChildName.Equals(WSManStringLiterals.containerResources, StringComparison.OrdinalIgnoreCase))
                        result = WSManStringLiterals.containerResources;
                    else if (ChildName.Equals(WSManStringLiterals.containerSecurity, StringComparison.OrdinalIgnoreCase))
                        result = WSManStringLiterals.containerSecurity;
                    else if (ChildName.Equals(WSManStringLiterals.containerService, StringComparison.OrdinalIgnoreCase))
                        result = WSManStringLiterals.containerService;
                    else if (ChildName.Equals(WSManStringLiterals.containerShell, StringComparison.OrdinalIgnoreCase))
                        result = WSManStringLiterals.containerShell;
                    else if (ChildName.Equals(WSManStringLiterals.containerTrustedHosts, StringComparison.OrdinalIgnoreCase))
                        result = WSManStringLiterals.containerTrustedHosts;
                    else if (ChildName.Equals(WSManStringLiterals.containerAuth, StringComparison.OrdinalIgnoreCase))
                        result = WSManStringLiterals.containerAuth;
                    else if (ChildName.Equals(WSManStringLiterals.containerClient, StringComparison.OrdinalIgnoreCase))
                        result = WSManStringLiterals.containerClient;
                    else if (ChildName.Equals(WSManStringLiterals.containerClientCertificate, StringComparison.OrdinalIgnoreCase))
                        result = WSManStringLiterals.containerClientCertificate;
                    else if (ChildName.Equals(WSManStringLiterals.containerDefaultPorts, StringComparison.OrdinalIgnoreCase))
                        result = WSManStringLiterals.containerDefaultPorts;
                    else if (ChildName.Equals(WSManStringLiterals.containerInitParameters, StringComparison.OrdinalIgnoreCase))
                        result = WSManStringLiterals.containerInitParameters;
                    else if (ChildName.Equals(WSManStringLiterals.containerListener, StringComparison.OrdinalIgnoreCase))
                        result = WSManStringLiterals.containerListener;
                        if (!string.IsNullOrEmpty(hostname))
                            if (ChildName.Equals(hostname, StringComparison.OrdinalIgnoreCase))
                                    if (ChildName.Equals(key, StringComparison.OrdinalIgnoreCase))
                                        result = key;
                            else if (path.Contains(hostname + WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerPlugin))
                                if (IsPathLocalMachine(hostname))
                                string uri = NormalizePath(path, hostname);
                                    SessionObjCache.TryGetValue(hostname, out sessionobj);
                                    if (outxml != null)
                                        string currentPluginName = string.Empty;
                                        GetPluginNames(outxml, out objPluginNames, out currentPluginName, path);
                                        if (path.EndsWith(hostname + WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerPlugin + WSManStringLiterals.DefaultPathSeparator + currentPluginName, StringComparison.OrdinalIgnoreCase))
                                            result = currentPluginName;
                    if (ChildName.StartsWith(WSManStringLiterals.containerListener, StringComparison.OrdinalIgnoreCase))
                        result = string.Concat(WSManStringLiterals.containerListener, "_", ChildName.AsSpan(ChildName.IndexOf('_') + 1));
                    if (ChildName.StartsWith(WSManStringLiterals.containerSingleResource, StringComparison.OrdinalIgnoreCase))
                        result = string.Concat(WSManStringLiterals.containerSingleResource, "_", ChildName.AsSpan(ChildName.IndexOf('_') + 1));
                    if (ChildName.StartsWith(WSManStringLiterals.containerSecurity, StringComparison.OrdinalIgnoreCase))
                        result = string.Concat(WSManStringLiterals.containerSecurity, "_", ChildName.AsSpan(ChildName.IndexOf('_') + 1));
                    if (ChildName.StartsWith(WSManStringLiterals.containerClientCertificate, StringComparison.OrdinalIgnoreCase))
                        result = string.Concat(WSManStringLiterals.containerClientCertificate, "_", ChildName.AsSpan(ChildName.IndexOf('_') + 1));
        private static ArrayList RemoveItemfromResourceArray(ArrayList resourceArray, string ChildName, string type, string property)
            if (resourceArray != null)
                bool itemfound = false;
                foreach (PSObject obj in resourceArray)
                    if (type.Equals("InitParams"))
                        if (obj.Properties.Match(ChildName).Count > 0)
                            itemfound = true;
                        if (obj.Properties[property].Value.ToString().Equals(ChildName, StringComparison.OrdinalIgnoreCase))
                if (itemfound)
                    resourceArray.RemoveAt(index);
            return resourceArray;
        /// Get Child Items of Listener and Client Certificate. Used by Getchilditem or getchildname.
        /// <param name="xmlResource"></param>
        /// <param name="ListenerOrCerMapping"></param>
        /// <param name="methodname"></param>
        private void GetChildItemOrNamesForListenerOrCertMapping(XmlDocument xmlResource, string ListenerOrCerMapping, string path, string host, ProviderMethods methodname, bool recurse)
            Hashtable Objcache, Keyscache;
            string PathEnd = string.Empty;
            if (ListenerOrCerMapping.Equals(WSManStringLiterals.containerClientCertificate))
                ProcessCertMappingObjects(xmlResource, out Objcache, out Keyscache);
            else if (ListenerOrCerMapping.Equals(WSManStringLiterals.containerListener))
                ProcessListenerObjects(xmlResource, out Objcache, out Keyscache);
            if (Objcache == null || Keyscache == null)
            if (path.EndsWith(host + WSManStringLiterals.DefaultPathSeparator + ListenerOrCerMapping, StringComparison.OrdinalIgnoreCase))
                foreach (string key in Keyscache.Keys)
                    switch (methodname)
                        // Get the items at Config level
                        case ProviderMethods.GetChildItems:
                            obj.Properties.Add(new PSNoteProperty(key, WSManStringLiterals.ContainerChildValue));
                            WritePSObjectPropertiesAsWSManElementObjects(obj, path, (string[])Keyscache[key], null, WsManElementObjectTypes.WSManConfigContainerElement, recurse);
                            // WriteItemObject(new WSManConfigContainerElement(key, WSManStringLiterals.ContainerChildValue, (string[])Keyscache[key]), path, true);
                        // Get the names of container at config level
                        case ProviderMethods.GetChildNames:
                            WriteItemObject(key, path, true);
                if (methodname.Equals(ProviderMethods.GetChildItems))
                    WritePSObjectPropertiesAsWSManElementObjects((PSObject)Objcache[item], path, null, null, WsManElementObjectTypes.WSManConfigLeafElement, recurse);
                else if (methodname.Equals(ProviderMethods.GetChildNames))
                    foreach (PSPropertyInfo prop in ((PSObject)Objcache[item]).Properties)
        /// Get a Listener or ClientCertificate Item. Used by GEt-Item.
        /// <param name="ContainerListenerOrClientCert"></param>
        private void GetItemListenerOrCertMapping(string path, XmlDocument xmlResource, string ContainerListenerOrClientCert, string childname, string host)
            if (ContainerListenerOrClientCert.Equals(WSManStringLiterals.containerListener, StringComparison.OrdinalIgnoreCase))
            else if (ContainerListenerOrClientCert.Equals(WSManStringLiterals.containerClientCertificate, StringComparison.OrdinalIgnoreCase))
            if (path.EndsWith(host + WSManStringLiterals.DefaultPathSeparator + ContainerListenerOrClientCert, StringComparison.OrdinalIgnoreCase))
                if (Objcache.ContainsKey(childname))
                    WriteItemObject(GetItemPSObjectWithTypeName(childname, WSManStringLiterals.ContainerChildValue, null, (string[])Keyscache[childname], null, WsManElementObjectTypes.WSManConfigContainerElement), path + WSManStringLiterals.DefaultPathSeparator + childname, true);
                    WriteItemObject(GetItemPSObjectWithTypeName(((PSObject)Objcache[item]).Properties[childname].Name, ((PSObject)Objcache[item]).Properties[childname].TypeNameOfValue, ((PSObject)Objcache[item]).Properties[childname].Value, null, null, WsManElementObjectTypes.WSManConfigLeafElement), path + WSManStringLiterals.DefaultPathSeparator + childname, false);
                catch (PSArgumentException) { return; }
        /// Removes a Listener or ClientCertificate object. Used by Remove-Item cmdlets.
        /// <param name="WsManUri"></param>
        /// <param name="primarykeys"></param>
        /// <param name="IsListener"></param>
        private void RemoveListenerOrCertMapping(object sessionobj, string WsManUri, string childname, string[] primarykeys, bool IsListener)
            XmlDocument xmlresources = EnumerateResourceValue(sessionobj, WsManUri);
            if (xmlresources != null)
                Hashtable KeysCache, ResourcesCache;
                if (!IsListener)
                    ProcessCertMappingObjects(xmlresources, out ResourcesCache, out KeysCache);
                    ProcessListenerObjects(xmlresources, out ResourcesCache, out KeysCache);
                if (KeysCache.Contains(childname))
                    PSObject objResource = (PSObject)ResourcesCache[childname];
                    Hashtable SelectorParams = new Hashtable();
                    foreach (string pKey in primarykeys)
                        SelectorParams.Add(pKey, objResource.Properties[pKey].Value);
                    DeleteResourceValue(sessionobj, WsManUri, SelectorParams, false);
        /// Used By ItemExists, HasChildItem,IsValidPath, IsItemContainer.
        private bool CheckValidContainerOrPath(string path)
            string strpathChk = string.Empty;
            // Get URI to pass to WsMan Automation API
            if (string.IsNullOrEmpty(uri))
                object sessionobj = null;
                strpathChk = host + WSManStringLiterals.DefaultPathSeparator;
                // Check for host path
                if (path.StartsWith(strpathChk + WSManStringLiterals.containerPlugin, StringComparison.OrdinalIgnoreCase))
                    if (path.Equals(strpathChk + WSManStringLiterals.containerPlugin, StringComparison.OrdinalIgnoreCase))
                    PSObject mshPluginLvl = ProcessPluginConfigurationLevel(CurrentPluginXML);
                    strpathChk = strpathChk + WSManStringLiterals.containerPlugin + WSManStringLiterals.DefaultPathSeparator + currentpluginname;
                    if (path.Equals(strpathChk, StringComparison.OrdinalIgnoreCase))
                    if (path.StartsWith(strpathChk + WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerQuotasParameters, StringComparison.OrdinalIgnoreCase))
                    if (path.StartsWith(strpathChk + WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerResources, StringComparison.OrdinalIgnoreCase))
                        if (path.Equals(strpathChk + WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerResources, StringComparison.OrdinalIgnoreCase))
                            strpathChk = strpathChk + WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerResources;
                                if (path.StartsWith(strpathChk + WSManStringLiterals.DefaultPathSeparator + sResourceDirName, StringComparison.OrdinalIgnoreCase))
                                    if (path.Equals(strpathChk + WSManStringLiterals.DefaultPathSeparator + sResourceDirName, StringComparison.OrdinalIgnoreCase))
                                        if (objresource.Properties.Match(ChildName).Count > 0)
                                    if (path.StartsWith(strpathChk + WSManStringLiterals.DefaultPathSeparator + sResourceDirName + WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerSecurity, StringComparison.OrdinalIgnoreCase))
                                        if (path.Equals(strpathChk + WSManStringLiterals.DefaultPathSeparator + sResourceDirName + WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerSecurity, StringComparison.OrdinalIgnoreCase))
                                        strpathChk = strpathChk + WSManStringLiterals.DefaultPathSeparator + sResourceDirName + WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerSecurity + WSManStringLiterals.DefaultPathSeparator;
                                            if (path.Equals(strpathChk + sSecurity, StringComparison.OrdinalIgnoreCase))
                                                if (security.Properties.Match(ChildName).Count > 0)
                    else if (path.StartsWith(strpathChk + WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerInitParameters, StringComparison.OrdinalIgnoreCase))
                        if (path.Equals(strpathChk + WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerInitParameters, StringComparison.OrdinalIgnoreCase))
                                foreach (PSObject obj in arrInitParams)
                        if (mshPluginLvl.Properties.Match(ChildName).Count > 0)
                else if (path.StartsWith(strpathChk + WSManStringLiterals.containerListener, StringComparison.OrdinalIgnoreCase))
                    return ItemExistListenerOrClientCertificate(sessionobj, uri, path, WSManStringLiterals.containerListener, host);
                else if (path.StartsWith(strpathChk + WSManStringLiterals.containerClientCertificate, StringComparison.OrdinalIgnoreCase))
                    return ItemExistListenerOrClientCertificate(sessionobj, uri, path, WSManStringLiterals.containerClientCertificate, host);
                    return (ContainResourceValue(sessionobj, uri, ChildName, path, host));
        private bool ItemExistListenerOrClientCertificate(object sessionobj, string ResourceURI, string path, string parentListenerOrCert, string host)
            XmlDocument outxml = EnumerateResourceValue(sessionobj, ResourceURI);
            if (path.Equals(host + WSManStringLiterals.DefaultPathSeparator + parentListenerOrCert, StringComparison.OrdinalIgnoreCase))
            if (outxml == null)
            Hashtable KeysCache = null, objcache = null;
            if (parentListenerOrCert.Equals(WSManStringLiterals.containerClientCertificate, StringComparison.OrdinalIgnoreCase))
                ProcessCertMappingObjects(outxml, out objcache, out KeysCache);
                ProcessListenerObjects(outxml, out objcache, out KeysCache);
            string PathChecked = host + WSManStringLiterals.DefaultPathSeparator + parentListenerOrCert;
            int pos = PathChecked.Length + 1;
            string RemainingPath = path.Substring(pos);
            string CurrentNode = null;
            pos = RemainingPath.IndexOf(WSManStringLiterals.DefaultPathSeparator);
            if (pos == -1)
                CurrentNode = RemainingPath;
                CurrentNode = RemainingPath.Substring(0, pos);
            if (!objcache.Contains(CurrentNode))
                // means the path was only till the CurrentNode. Nothing ahead
            // Get the object cache from the listener object
            PSObject obj = (PSObject)objcache[CurrentNode];
            CurrentNode = RemainingPath.Substring(pos + 1);
            if (CurrentNode.Contains(WSManStringLiterals.DefaultPathSeparator))
                // No more directories allowed after listeners objects
            if (obj.Properties.Match(CurrentNode).Count > 0)
        /// For the recurse operation of Get-ChildItems.
        private void GetChildItemsRecurse(string path, string childname, ProviderMethods methodname, bool recurse)
            if (path.Equals(WSManStringLiterals.rootpath))
                path = childname;
                path = path + WSManStringLiterals.DefaultPathSeparator + childname;
            if (HasChildItems(path))
        /// Get the child items or Names. Used by GetChildItems and GetChildNames.
        private void GetChildItemsOrNames(string path, ProviderMethods methodname, bool recurse)
                        PSObject obj = BuildHostLevelPSObjectArrayList(null, string.Empty, true);
                        WritePSObjectPropertiesAsWSManElementObjects(obj, WSManStringLiterals.rootpath, null,
                            "ComputerLevel", WsManElementObjectTypes.WSManConfigContainerElement, recurse);
                        foreach (string hostname in SessionObjCache.Keys)
                            WriteItemObject(hostname, WSManStringLiterals.rootpath, true);
            // Checks the WinRM Service
                    PSObject obj = BuildHostLevelPSObjectArrayList(sessionobj, uri, false);
                            WritePSObjectPropertiesAsWSManElementObjects(obj, path, null, null, WsManElementObjectTypes.WSManConfigLeafElement, recurse);
                            WritePSObjectPropertyNames(obj, path);
                    GetChildItemOrNamesForListenerOrCertMapping(outxml, WSManStringLiterals.containerListener, path, host, methodname, recurse);
                    GetChildItemOrNamesForListenerOrCertMapping(outxml, WSManStringLiterals.containerClientCertificate, path, host, methodname, recurse);
                            // Get the items at Plugin level
                                foreach (PSPropertyInfo p in objPluginNames.Properties)
                                    obj.Properties.Add(new PSNoteProperty(p.Name, p.Value));
                                    WritePSObjectPropertiesAsWSManElementObjects(obj, path, new string[] { "Name=" + p.Name }, null, WsManElementObjectTypes.WSManConfigContainerElement, recurse);
                                    // WriteItemObject(new PSObject(new WSManConfigContainerElement(p.Name, p.Value.ToString(), new string[] { "Name=" + p.Name })), path + WSManStringLiterals.DefaultPathSeparator + p.Name, true);
                            // Get the names of container at Plugin level
                                WritePSObjectPropertyNames(objPluginNames, path);
                        if (path.EndsWith(strPathchk + currentpluginname, StringComparison.OrdinalIgnoreCase))
                                    WritePSObjectPropertiesAsWSManElementObjects(objPluginlevel, path, null, null, WsManElementObjectTypes.WSManConfigLeafElement, recurse);
                                    WritePSObjectPropertyNames(objPluginlevel, path);
                                    if (methodname == ProviderMethods.GetChildNames)
                                        WriteItemObject(attrOfQuotas.Name, pathToAdd, false);
                        else if (path.Contains(strPathchk + currentpluginname + WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerResources))
                            if (path.EndsWith(strPathchk + WSManStringLiterals.containerResources, StringComparison.OrdinalIgnoreCase))
                                                string[] key = new string[] { "Uri" + WSManStringLiterals.Equalto + p.Properties["ResourceURI"].Value.ToString() };
                                                obj.Properties.Add(new PSNoteProperty(p.Properties["ResourceDir"].Value.ToString(), WSManStringLiterals.ContainerChildValue));
                                                WritePSObjectPropertiesAsWSManElementObjects(obj, path, key, null, WsManElementObjectTypes.WSManConfigContainerElement, recurse);
                                                // WriteItemObject(new WSManConfigContainerElement(p.Properties["ResourceDir"].Value.ToString(), WSManStringLiterals.ContainerChildValue, key), path + WSManStringLiterals.DefaultPathSeparator + p.Properties["ResourceDir"].Value.ToString(), true);
                                                WriteItemObject(p.Properties["ResourceDir"].Value.ToString(), path, true);
                                sResourceDirName = path.Substring(strPathchk.Length, path.IndexOf(WSManStringLiterals.DefaultPathSeparator, strPathchk.Length) - (strPathchk.Length));
                            if (path.Contains(strPathchk + sResourceDirName))
                                                // Get the items at Initparams level
                                                    WritePSObjectPropertiesAsWSManElementObjects(p, path, null, null, WsManElementObjectTypes.WSManConfigLeafElement, recurse);
                                                    WritePSObjectPropertyNames(p, path);
                                strPathchk = strPathchk + sResourceDirName + WSManStringLiterals.DefaultPathSeparator;
                                if (path.EndsWith(strPathchk + WSManStringLiterals.containerSecurity, StringComparison.OrdinalIgnoreCase) || path.Contains(WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerSecurity + "_"))
                                    if (arrSecurity != null)
                                        foreach (PSObject objsecurity in arrSecurity)
                                            if (sResourceDirName.Equals(objsecurity.Properties["ResourceDir"].Value.ToString(), StringComparison.OrdinalIgnoreCase))
                                                if (path.EndsWith(strPathchk + WSManStringLiterals.containerSecurity, StringComparison.OrdinalIgnoreCase))
                                                    objsecurity.Properties.Remove("ResourceDir");
                                                        // Get the items at Security level
                                                            string key = "Uri" + WSManStringLiterals.Equalto + objsecurity.Properties["Uri"].Value.ToString();
                                                            obj.Properties.Add(new PSNoteProperty(objsecurity.Properties["SecurityDIR"].Value.ToString(), WSManStringLiterals.ContainerChildValue));
                                                            WritePSObjectPropertiesAsWSManElementObjects(obj, path, new string[] { key }, null, WsManElementObjectTypes.WSManConfigContainerElement, recurse);
                                                            // WriteItemObject(new WSManConfigContainerElement(objsecurity.Properties["SecurityDIR"].Value.ToString(), WSManStringLiterals.ContainerChildValue, new string[] { key }), path + WSManStringLiterals.DefaultPathSeparator + objsecurity.Properties["SecurityDIR"].Value.ToString(), true);
                                                            WriteItemObject(objsecurity.Properties["SecurityDIR"].Value.ToString(), path, true);
                                                    if (sSecurityDirName.Equals(objsecurity.Properties["SecurityDIR"].Value.ToString(), StringComparison.OrdinalIgnoreCase))
                                                        objsecurity.Properties.Remove("SecurityDIR");
                                                                WritePSObjectPropertiesAsWSManElementObjects(objsecurity, path, null, null, WsManElementObjectTypes.WSManConfigLeafElement, recurse);
                                                                WritePSObjectPropertyNames(objsecurity, path);
                            if (arrInitParams == null)
                                        WritePSObjectPropertiesAsWSManElementObjects(p, path, null, "InitParams", WsManElementObjectTypes.WSManConfigLeafElement, recurse);
                    if ((path.EndsWith(WSManStringLiterals.containerService, StringComparison.OrdinalIgnoreCase) || path.EndsWith(WSManStringLiterals.containerTrustedHosts, StringComparison.OrdinalIgnoreCase)
                           || path.EndsWith(WSManStringLiterals.containerClient, StringComparison.OrdinalIgnoreCase) || path.EndsWith(WSManStringLiterals.containerDefaultPorts, StringComparison.OrdinalIgnoreCase)
                              || path.EndsWith(WSManStringLiterals.containerAuth, StringComparison.OrdinalIgnoreCase) || path.EndsWith(WSManStringLiterals.containerShell, StringComparison.OrdinalIgnoreCase)))
                        foreach (XmlNode node in outxml.ChildNodes)
                            PSObject mshObject = ConvertToPSObject(node);
                                    WritePSObjectPropertiesAsWSManElementObjects(mshObject, path, null, null, WsManElementObjectTypes.WSManConfigLeafElement, recurse);
                                    WritePSObjectPropertyNames(mshObject, path);
        /// Get the Plugin names from the WsMan Config file.
        /// <param name="xmlPlugins"></param>
        /// <param name="PluginNames"></param>
        /// <param name="CurrentPluginName"></param>
        private static int GetPluginNames(XmlDocument xmlPlugins, out PSObject PluginNames, out string CurrentPluginName, string path)
            PluginNames = new PSObject();
            CurrentPluginName = string.Empty;
            // If the execution is reached this point ... that means the path should for plugins directory (..\Plugins...).
            if (!path.Contains(WSManStringLiterals.DefaultPathSeparator + WSManStringLiterals.containerPlugin))
            // The path will be something like <serverName>\Plugin\<Name of the plugin>\...
            string[] splitPath = path.Split(WSManStringLiterals.DefaultPathSeparator);
            XmlNodeList pluginNodeList = xmlPlugins.GetElementsByTagName("PlugInConfiguration");
            foreach (XmlElement e in pluginNodeList)
                for (int i = 0; i <= e.Attributes.Count - 1; i++)
                    if (e.Attributes[i].LocalName.Equals("Name"))
                        PluginNames.Properties.Add(new PSNoteProperty(e.Attributes[i].Value, WSManStringLiterals.ContainerChildValue));
                        // If the path contains \plugin and splitLength is greater than 3 then splitLength[2] will be plugin Name.
                        if (splitPath.Length >= 3 && splitPath[2].Equals(e.Attributes[i].Value, StringComparison.OrdinalIgnoreCase))
                            CurrentPluginName = e.Attributes[i].Value;
            return pluginNodeList.Count;
        /// All Error are thrown using this method.
        /// <param name="ErrorMessage"></param>
        /// <param name="IsWSManError"></param>
        private void AssertError(string ErrorMessage, bool IsWSManError)
            if (IsWSManError)
                XmlDocument ErrorDoc = new XmlDocument();
                ErrorDoc.LoadXml(ErrorMessage);
                XmlNodeList errornodelist = ErrorDoc.GetElementsByTagName("f:Message");
                foreach (XmlNode node in errornodelist)
                    InvalidOperationException ex = new InvalidOperationException(node.InnerText);
                    ErrorRecord er = new ErrorRecord(ex, "WsManError", ErrorCategory.InvalidOperation, null);
                InvalidOperationException ex = new InvalidOperationException(ErrorMessage);
        /// Checks whether WsMan Service is running.
        private bool IsWSManServiceRunning()
            if (winrmServiceController == null)
                winrmServiceController = new ServiceController("WinRM");
                winrmServiceController.Refresh();
            return (winrmServiceController.Status.Equals(ServiceControllerStatus.Running));
        /// Starts the WsMan service.
        /// <param name="force"></param>
        private void StartWSManService(bool force)
                string startserviceScript = string.Format(CultureInfo.InvariantCulture, WSManStringLiterals.StartWinrmServiceSBFormat);
                ScriptBlock startserviceSb = ScriptBlock.Create(startserviceScript);
                Collection<PSObject> result = startserviceSb.Invoke(force, helper.GetResourceMsgFromResourcetext("WSManServiceStartCaption"), helper.GetResourceMsgFromResourcetext("WSManServiceStartQuery"));
                if (!(bool)result[0].ImmediateBaseObject)
                    AssertError(helper.GetResourceMsgFromResourcetext("WinRMServiceError"), false);
            catch (CmdletInvocationException)
                // Eating cmdlet invocation exception. The exception is thrown when No is given.
        /// Checks whether localmachine or not. If this returns true only we start the service.
        private static bool IsPathLocalMachine(string host)
            bool hostfound = false;
            // Check is Localhost
            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                hostfound = true;
            // Check is TestMac
            if (!hostfound)
                if (host.Equals(System.Net.Dns.GetHostName(), StringComparison.OrdinalIgnoreCase))
            // Check is TestMac.redmond.microsoft.corp.com
                System.Net.IPHostEntry hostentry = System.Net.Dns.GetHostEntry("localhost");
                if (host.Equals(hostentry.HostName, StringComparison.OrdinalIgnoreCase))
                // Check is 127.0.0.1 or ::1
                    foreach (System.Net.IPAddress ipaddress in hostentry.AddressList)
                        if (ipaddress.ToString().Equals(host, StringComparison.OrdinalIgnoreCase))
            // check if any IPAddress.
                foreach (System.Net.IPAddress ipaddress in System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName()))
            return hostfound;
        #region Plugin private functions
        private static void GenerateObjectNameAndKeys(Hashtable InputAttributes, string ResourceURI, string ContainerItem, out string ItemName, out string[] keys)
            StringBuilder sbHashKey = new StringBuilder();
            string keysColumns = string.Empty;
            foreach (DictionaryEntry attribute in InputAttributes)
                if (IsPKey(attribute.Key.ToString(), ResourceURI))
                    sbHashKey.Append(attribute.Key.ToString());
                    sbHashKey.Append(WSManStringLiterals.Equalto);
                    sbHashKey.Append(attribute.Value.ToString());
                    keysColumns = keysColumns + attribute.Key.ToString() + WSManStringLiterals.Equalto + attribute.Value.ToString() + "|";
                    if (ContainerItem.Equals("Listener", StringComparison.OrdinalIgnoreCase)
                        && attribute.Key.ToString().Equals("Port", StringComparison.OrdinalIgnoreCase))
                        // we add the Port number when generating the name in order
                        // be distinguish compatibility listeners which might have the same
                        // real key (address and port) as a real listener
            keysColumns = keysColumns.Substring(0, keysColumns.LastIndexOf('|'));
            ItemName = ContainerItem + "_" + Math.Abs(sbHashKey.ToString().GetHashCode());
            keys = keysColumns.Split('|');
        private static void ProcessCertMappingObjects(XmlDocument xmlCerts, out Hashtable Certcache, out Hashtable Keyscache)
            Hashtable lCache = new Hashtable();
            Hashtable kCache = new Hashtable();
            XmlNodeList xmlnodesCerts = xmlCerts.GetElementsByTagName("cfg:" + "CertMapping");
            if (xmlnodesCerts == null)
                Certcache = null;
                Keyscache = null;
            foreach (XmlNode node in xmlnodesCerts)
                Hashtable InputAttributes = new Hashtable();
                PSObject objCerts = new PSObject();
                string[] keys = null;
                string ItemName = string.Empty;
                foreach (XmlNode childnode in node.ChildNodes)
                    //    if (childnode.LocalName.Equals("URI"))
                    //        // sbCerts.Append(childnode.LocalName);
                    //        // sbCerts.Append(WSManStringLiterals.Equalto);
                    //        // sbCerts.Append(childnode.InnerText);
                    //        // keys[0] = childnode.LocalName + WSManStringLiterals.Equalto + childnode.InnerText;
                    //    else if (childnode.LocalName.Equals("Subject"))
                    //        // keys[1] = childnode.LocalName + WSManStringLiterals.Equalto + childnode.InnerText;
                    //    else if (childnode.LocalName.Equals("Issuer"))
                    //        // keys[2] = childnode.LocalName + WSManStringLiterals.Equalto + childnode.InnerText;
                    InputAttributes.Add(childnode.LocalName, childnode.InnerText);
                    objCerts.Properties.Add(new PSNoteProperty(childnode.LocalName, childnode.InnerText));
                GenerateObjectNameAndKeys(InputAttributes, WSManStringLiterals.containerCertMapping, WSManStringLiterals.containerClientCertificate, out ItemName, out keys);
                // lCache.Add(WSManStringLiterals.containerClientCertificate + "_" + Math.Abs(sbCerts.ToString().GetHashCode()), objCerts);
                lCache.Add(ItemName, objCerts);
                kCache.Add(ItemName, keys);
                // kCache.Add(WSManStringLiterals.containerClientCertificate + "_" + Math.Abs(sbCerts.ToString().GetHashCode()), keys);
            Certcache = lCache;
            Keyscache = kCache;
        private static void ProcessListenerObjects(XmlDocument xmlListeners, out Hashtable listenercache, out Hashtable Keyscache)
            XmlNodeList xmlnodesListeners = xmlListeners.GetElementsByTagName("cfg:" + WSManStringLiterals.containerListener);
            if (xmlnodesListeners == null)
                listenercache = null;
            foreach (XmlNode node in xmlnodesListeners)
                PSObject objListener = new PSObject();
                    // if (childnode.LocalName.Equals("Address"))
                    //    sbListener.Append(childnode.LocalName);
                    //    sbListener.Append(WSManStringLiterals.Equalto);
                    //    sbListener.Append(childnode.InnerText);
                    //    Keys[0] = childnode.LocalName + WSManStringLiterals.Equalto + childnode.InnerText;
                    //    objListener.Properties.Add(new PSNoteProperty(childnode.LocalName, childnode.InnerText));
                    // else if (childnode.LocalName.Equals("Transport"))
                    //    Keys[1] = childnode.LocalName + WSManStringLiterals.Equalto + childnode.InnerText;
                    if (childnode.LocalName.Equals("ListeningOn"))
                        string ListeningOnItem = childnode.LocalName + "_" + Math.Abs(childnode.InnerText.GetHashCode());
                        objListener.Properties.Add(new PSNoteProperty(ListeningOnItem, childnode.InnerText));
                        InputAttributes.Add(ListeningOnItem, childnode.InnerText);
                        objListener.Properties.Add(new PSNoteProperty(childnode.LocalName, childnode.InnerText));
                GenerateObjectNameAndKeys(InputAttributes, WSManStringLiterals.containerListener, WSManStringLiterals.containerListener, out ItemName, out Keys);
                // lCache.Add(WSManStringLiterals.containerListener + "_" + Math.Abs(sbListener.ToString().GetHashCode()), objListener);
                lCache.Add(ItemName, objListener);
                kCache.Add(ItemName, Keys);
                // kCache.Add(WSManStringLiterals.containerListener + "_" + Math.Abs(sbListener.ToString().GetHashCode()), Keys);
            listenercache = lCache;
        private static PSObject ProcessPluginConfigurationLevel(XmlDocument xmldoc, bool setRunasPasswordAsSecureString = false)
            PSObject objConfiglvl = null;
            if (xmldoc != null)
                XmlNodeList nodelistPlugin = xmldoc.GetElementsByTagName("PlugInConfiguration");
                if (nodelistPlugin.Count > 0)
                    objConfiglvl = new PSObject();
                    XmlAttributeCollection attributecol = nodelistPlugin.Item(0).Attributes;
                    XmlNode runAsUserNode = attributecol.GetNamedItem(WSManStringLiterals.ConfigRunAsUserName);
                    bool runAsUserPresent = runAsUserNode != null && !string.IsNullOrEmpty(runAsUserNode.Value);
                    for (int i = 0; i <= attributecol.Count - 1; i++)
                        if (string.Equals(attributecol[i].LocalName, WSManStringLiterals.ConfigRunAsPasswordName, StringComparison.OrdinalIgnoreCase)
                            && runAsUserPresent
                            && setRunasPasswordAsSecureString)
                            objConfiglvl.Properties.Add(new PSNoteProperty(attributecol[i].LocalName, new SecureString()));
                            objConfiglvl.Properties.Add(new PSNoteProperty(attributecol[i].LocalName, attributecol[i].Value));
                // Containers in Plugin Level Configs
                if (objConfiglvl != null)
                    objConfiglvl.Properties.Add(new PSNoteProperty("InitializationParameters", WSManStringLiterals.ContainerChildValue));
                    objConfiglvl.Properties.Add(new PSNoteProperty("Resources", WSManStringLiterals.ContainerChildValue));
                    objConfiglvl.Properties.Add(new PSNoteProperty(WSManStringLiterals.containerQuotasParameters, WSManStringLiterals.ContainerChildValue));
            return objConfiglvl;
        private static ArrayList ProcessPluginResourceLevel(XmlDocument xmldoc, out ArrayList arrSecurity)
            ArrayList Resources = null;
            ArrayList nSecurity = null;
                XmlNodeList xmlpluginResource = xmldoc.GetElementsByTagName("Resource");
                if (xmlpluginResource.Count > 0)
                    Resources = new ArrayList();
                    nSecurity = new ArrayList();
                    foreach (XmlElement xe in xmlpluginResource)
                        PSObject objResource = new PSObject();
                        string strUniqueResourceId = string.Empty;
                        XmlAttributeCollection attributecol = xe.Attributes;
                        bool ExactMatchFound = false;
                        bool SupportsOptionsFound = false;
                        string resourceUri = string.Empty;
                            if (attributecol[i].LocalName.Equals("ResourceUri", StringComparison.OrdinalIgnoreCase))
                                resourceUri = attributecol[i].Value;
                                strUniqueResourceId = "Resource_" + Convert.ToString(Math.Abs(attributecol[i].Value.GetHashCode()), CultureInfo.InvariantCulture);
                                objResource.Properties.Add(new PSNoteProperty("ResourceDir", strUniqueResourceId));
                            if (attributecol[i].LocalName.Equals("ExactMatch", StringComparison.OrdinalIgnoreCase))
                                objResource.Properties.Add(new PSNoteProperty(attributecol[i].LocalName, attributecol[i].Value));
                                ExactMatchFound = true;
                            if (attributecol[i].LocalName.Equals("SupportsOptions", StringComparison.OrdinalIgnoreCase))
                                SupportsOptionsFound = true;
                        if (!ExactMatchFound)
                            objResource.Properties.Add(new PSNoteProperty("ExactMatch", false));
                        if (!SupportsOptionsFound)
                            objResource.Properties.Add(new PSNoteProperty("SupportsOptions", false));
                        // Processing capabilities
                        XmlDocument xmlCapabilities = new XmlDocument();
                        xmlCapabilities.LoadXml("<Capabilities>" + xe.InnerXml + "</Capabilities>");
                        XmlNodeList nodeCapabilities = xmlCapabilities.GetElementsByTagName("Capability");
                        object[] enumcapability = null;
                        if (nodeCapabilities.Count > 0)
                            enumcapability = new object[nodeCapabilities.Count];
                            for (int i = 0; i < nodeCapabilities.Count; i++)
                                enumcapability.SetValue(nodeCapabilities[i].Attributes["Type"].Value, i);
                        objResource.Properties.Add(new PSNoteProperty("Capability", enumcapability));
                        objResource.Properties.Add(new PSNoteProperty(WSManStringLiterals.containerSecurity, WSManStringLiterals.ContainerChildValue));
                        // Process Security in Resources. We add the resource Unique ID in to each security to
                        // identify in the Provider methods.
                        nSecurity = ProcessPluginSecurityLevel(nSecurity, xmlCapabilities, strUniqueResourceId, resourceUri);
                        Resources.Add(objResource);
            arrSecurity = nSecurity;
            return Resources;
        private static ArrayList ProcessPluginInitParamLevel(XmlDocument xmldoc)
            ArrayList InitParamLvl = null;
                XmlNodeList nodelistInitParam = xmldoc.GetElementsByTagName("Param");
                if (nodelistInitParam.Count > 0)
                    InitParamLvl = new ArrayList();
                    foreach (XmlElement xe in nodelistInitParam)
                        PSObject objInitParam = new PSObject();
                        string Name = string.Empty;
                        string Value = string.Empty;
                            if (attributecol[i].LocalName.Equals("Name", StringComparison.OrdinalIgnoreCase))
                                Name = attributecol[i].Value;
                            if (attributecol[i].LocalName.Equals("Value", StringComparison.OrdinalIgnoreCase))
                                string ValueAsXML = attributecol[i].Value;
                                Value = SecurityElement.Escape(ValueAsXML);
                        objInitParam.Properties.Add(new PSNoteProperty(Name, Value));
                        InitParamLvl.Add(objInitParam);
            return InitParamLvl;
        private static ArrayList ProcessPluginSecurityLevel(ArrayList arrSecurity, XmlDocument xmlSecurity, string UniqueResourceID, string ParentResourceUri)
            // ArrayList SecurityLvl = null;
            if (xmlSecurity != null)
                XmlNodeList nodelistSecurity = xmlSecurity.GetElementsByTagName(WSManStringLiterals.containerSecurity);
                if (nodelistSecurity.Count > 0)
                    // SecurityLvl = new ArrayList();
                    foreach (XmlElement xe in nodelistSecurity)
                        PSObject objSecurity = new PSObject();
                            if (attributecol[i].LocalName.Equals("Uri", StringComparison.OrdinalIgnoreCase))
                                objSecurity.Properties.Add(new PSNoteProperty("SecurityDIR", "Security_" + Math.Abs(UniqueResourceID.GetHashCode())));
                                objSecurity.Properties.Add(new PSNoteProperty(attributecol[i].LocalName, attributecol[i].Value));
                            objSecurity.Properties.Add(new PSNoteProperty("ExactMatch", false));
                        objSecurity.Properties.Add(new PSNoteProperty("ResourceDir", UniqueResourceID));
                        objSecurity.Properties.Add(new PSNoteProperty("ParentResourceUri", ParentResourceUri));
                        arrSecurity.Add(objSecurity);
            return arrSecurity;
        /// This method constructs the Configuration XML from the PSObject.
        /// For RunAsPassword, if the value is not of type SecureString or the value is not present
        /// then an Empty string is added as value. The caller of this method MUST make sure that
        /// the RunAsPassword (if updated) is of type SecureString.
        /// <param name="objinputparam">PSObject, from which XML will be produced.</param>
        /// <param name="ResourceURI">Resource URI for the XML.</param>
        /// <param name="host">Name of the Host.</param>
        /// <param name="Operation">Type of Operation.</param>
        /// <param name="resources">List of Resources.</param>
        /// <param name="securities">List of Securities</param>
        /// <param name="initParams">List of initialization parameters.</param>
        /// <returns>An Configuration XML, ready to send to server.</returns>
        private static string ConstructPluginXml(PSObject objinputparam, string ResourceURI, string host, string Operation, ArrayList resources, ArrayList securities, ArrayList initParams)
            sbvalues.Append("<PlugInConfiguration ");
            sbvalues.Append("xmlns=");
            sbvalues.Append(
                string.Concat(
                    WSManStringLiterals.EnclosingDoubleQuotes,
                    @"http://schemas.microsoft.com/wbem/wsman/1/config/PluginConfiguration",
                    WSManStringLiterals.EnclosingDoubleQuotes));
            if (objinputparam != null)
                foreach (PSPropertyInfo prop in objinputparam.Properties)
                    sbvalues.Append(WSManStringLiterals.SingleWhiteSpace);
                    if (IsValueOfParamList(prop.Name, WSManStringLiterals.NewItemPluginConfigParams))
                        // ... Name
                        sbvalues.Append(prop.Name);
                        // ... Name=
                        sbvalues.Append(WSManStringLiterals.Equalto);
                        if (WSManStringLiterals.ConfigRunAsPasswordName.Equals(prop.Name, StringComparison.OrdinalIgnoreCase))
                            prop.Value = GetStringFromSecureString(prop.Value);
                        // ... Name="Value"
                                prop.Value.ToString(),
            // < ...Name="value" ... >
            sbvalues.Append(WSManStringLiterals.GreaterThan);
            if (Operation.Equals("New"))
                    sbvalues.Append(ConstructResourceXml(objinputparam, null, null));
                    sbvalues.Append(ConstructResourceXml(null, resources, null));
            else if (Operation.Equals("Set"))
                if (initParams != null)
                    sbvalues.Append(ConstructInitParamsXml(null, initParams));
                if (resources != null)
                    sbvalues.Append(ConstructResourceXml(null, resources, securities));
            sbvalues.Append("</PlugInConfiguration>");
            return sbvalues.ToString();
        /// PS wraps most of the parameters to Item cmdlets in PSObject. These values are passed as 'Object'.
        /// This method unwraps the PSObjects and returns the base object as the required Type for the given
        /// configuration name as follows:
        /// RunAsUser - PSCredential
        /// RunAsPassword - SecureString
        /// If the object provided by user is not of required type the method return Null.
        /// If the object is not PSObject, the method returns the 'value'.
        /// <param name="configurationName">Name of the configuration setting.</param>
        /// <param name="value">Object provided by User.</param>
        /// <returns>Object of the required type or Null.</returns>
        private object ValidateAndGetUserObject(string configurationName, object value)
            PSObject basePsObject = value as PSObject;
            PSCredential psCredential = null;
            if (basePsObject == null)
                psCredential = value as PSCredential;
            if (configurationName.Equals(WSManStringLiterals.ConfigRunAsPasswordName, StringComparison.OrdinalIgnoreCase))
                if (basePsObject != null && basePsObject.BaseObject is SecureString)
                    return basePsObject.BaseObject as SecureString;
                    string error = string.Format(
                        helper.GetResourceMsgFromResourcetext("InvalidValueType"),
                        typeof(SecureString).FullName);
                    AssertError(error, false);
            else if (configurationName.Equals(WSManStringLiterals.ConfigRunAsUserName, StringComparison.OrdinalIgnoreCase))
                if (basePsObject != null && basePsObject.BaseObject is PSCredential)
                    return basePsObject.BaseObject as PSCredential;
                else if (psCredential != null)
                    return psCredential;
                        WSManStringLiterals.ConfigRunAsUserName,
                        typeof(PSCredential).FullName);
        /// Appends the plain text value of a SecureString variable to the StringBuilder.
        /// if the propertyValue provided is not SecureString appends empty string.
        /// <param name="propertyValue">Value to append.</param>
        private static string GetStringFromSecureString(object propertyValue)
            string passwordValueToAdd = string.Empty;
            if (propertyValue is SecureString value)
                IntPtr ptr = Marshal.SecureStringToBSTR(value);
                passwordValueToAdd = Marshal.PtrToStringAuto(ptr);
                Marshal.ZeroFreeBSTR(ptr);
            return passwordValueToAdd;
        private static string ConstructResourceXml(PSObject objinputparams, ArrayList resources, ArrayList securities)
            StringBuilder sbvalues = new StringBuilder(string.Empty);
            if (objinputparams == null && resources == null)
            object[] capability = null;
            sbvalues.Append("<Resources>");
            if (objinputparams != null)
                sbvalues.Append("<Resource");
                foreach (PSPropertyInfo prop in objinputparams.Properties)
                    if (IsValueOfParamList(prop.Name, WSManStringLiterals.NewItemResourceParams))
                        if (prop.Name.Equals("Resource") || prop.Name.Equals("ResourceUri"))
                            sbvalues.Append("ResourceUri" + WSManStringLiterals.Equalto + WSManStringLiterals.EnclosingDoubleQuotes + prop.Value.ToString() + WSManStringLiterals.EnclosingDoubleQuotes);
                        else if (prop.Name.Equals("Capability"))
                            capability = (object[])prop.Value;
                            sbvalues.Append(WSManStringLiterals.EnclosingDoubleQuotes + prop.Value.ToString() + WSManStringLiterals.EnclosingDoubleQuotes);
                if (securities != null)
                    sbvalues.Append(ConstructSecurityXml(null, securities, string.Empty));
                sbvalues.Append(ConstructCapabilityXml(capability));
                sbvalues.Append("</Resource>");
                foreach (PSObject p in resources)
                    foreach (PSPropertyInfo prop in p.Properties)
                                if (prop.Value.GetType().FullName.Equals("System.String"))
                                    capability = new object[] { prop.Value };
                        sbvalues.Append(ConstructSecurityXml(null, securities, p.Properties["ResourceDir"].Value.ToString()));
            sbvalues.Append("</Resources>");
        private static string ConstructSecurityXml(PSObject objinputparams, ArrayList securities, string strResourceIdentity)
            // <Security Uri="" ExactMatch="false" Sddl="O:NSG:BAD:P(A;;GA;;;BA)(A;;GR;;;ER)S:P(AU;FA;GA;;;WD)(AU;SA;GXGW;;;WD)"/>
            if (objinputparams == null && securities == null)
                AddSecurityProperties(objinputparams.Properties, sbvalues);
                foreach (PSObject p in securities)
                    if (p.Properties["ResourceDir"].Value.ToString().Equals(strResourceIdentity))
                        AddSecurityProperties(p.Properties, sbvalues);
        private static void AddSecurityProperties(
            PSMemberInfoCollection<PSPropertyInfo> properties,
            StringBuilder sbValues)
            sbValues.Append("<Security");
            foreach (var prop in properties)
                sbValues.Append(WSManStringLiterals.SingleWhiteSpace);
                if (IsValueOfParamList(prop.Name, WSManStringLiterals.NewItemSecurityParams))
                    // Ensure SDDL, which can contain invalid XML characters such as '&', is escaped.
                    string propValueStr = (prop.Name.Equals("SDDL", StringComparison.OrdinalIgnoreCase)) ?
                        EscapeValuesForXML(prop.Value.ToString()) :
                        prop.Value.ToString();
                    sbValues.Append(prop.Name);
                    sbValues.Append(WSManStringLiterals.Equalto);
                    sbValues.Append(WSManStringLiterals.EnclosingDoubleQuotes + propValueStr + WSManStringLiterals.EnclosingDoubleQuotes);
            sbValues.Append(WSManStringLiterals.GreaterThan);
            sbValues.Append("</Security>");
        private static string ConstructInitParamsXml(PSObject objinputparams, ArrayList initparams)
            // <InitializationParameters>
            // <Param Name="Param1" Value="Value1" />
            // </InitializationParameters>
            if (objinputparams == null && initparams == null)
            sbvalues.Append("<InitializationParameters>");
                    sbvalues.Append("<Param");
                    sbvalues.Append("Name");
                    sbvalues.Append(WSManStringLiterals.EnclosingDoubleQuotes + prop.Name + WSManStringLiterals.EnclosingDoubleQuotes);
                    sbvalues.Append("Value");
                    sbvalues.Append(WSManStringLiterals.XmlClosingTag);
                foreach (PSObject p in initparams)
            sbvalues.Append("</InitializationParameters>");
        private static string ConstructCapabilityXml(object[] capabilities)
            foreach (object cap in capabilities)
                sbvalues.Append("<Capability");
                sbvalues.Append("Type" + WSManStringLiterals.Equalto);
                sbvalues.Append(WSManStringLiterals.EnclosingDoubleQuotes + cap.ToString() + WSManStringLiterals.EnclosingDoubleQuotes);
                sbvalues.Append("</Capability>");
        private static bool IsValueOfParamList(string name, string[] paramcontainer)
            foreach (string value in paramcontainer)
                if (value.Equals(name, StringComparison.OrdinalIgnoreCase))
        #endregion Plugin private functions
        private enum ProviderMethods
            GetChildItems,
            GetChildNames
        private enum WsManElementObjectTypes
            WSManConfigElement,
            WSManConfigContainerElement,
            WSManConfigLeafElement
        #region def
        private static readonly string[] WinrmRootName = new string[] { "winrm/Config" };
        private static readonly string[] WinRmRootConfigs = new string[] {
            "Client",
            "Service",
            "Shell",
            "Listener",
            "Plugin",
            "ClientCertificate"
        // Defining Primarykeys for resource uri's
        private static readonly string[] PKeyListener = new string[] { "Address", "Transport" };
        private static readonly string[] PKeyPlugin = new string[] { "Name" };
        private static readonly string[] PKeyCertMapping = new string[] { "Issuer", "Subject", "Uri" };
        /// In PPQ display warnings for these configurations.
        private static readonly List<string> ppqWarningConfigurations = new List<string>
            "idletimeoutms",
            "maxprocessespershell",
            "maxmemorypershellmb",
            "maxshellsperuser",
            "maxconcurrentusers"
        /// Display warning for these configurations.
        private static readonly List<string> globalWarningConfigurations = new List<string>
            "maxconcurrentoperationsperuser",
            "idletimeout",
        /// Display warning for these URIs.
        private static readonly List<string> globalWarningUris =
            new List<string> {
                WinrmRootName[0] + WSManStringLiterals.WinrmPathSeparator + WSManStringLiterals.containerWinrs,
                WinrmRootName[0] + WSManStringLiterals.WinrmPathSeparator + WSManStringLiterals.containerService};
        #endregion def
    #region "Dynamic Parameter Classes"
    #region "New-Item Dynamic Parameters"
    /// Computer dynamic parameters. This is similar to connect-wsman parameters.
    /// Available path wsman:\>
    public class WSManProviderNewItemComputerParameters
        /// The following is the definition of the input parameter "OptionSet".
        /// OptionSet is a hash table and is used to pass a set of switches to the
        /// service to modify or refine the nature of the request.
        [Alias("OS")]
        public Hashtable OptionSet
            get { return optionset; }
            set { optionset = value; }
        private Hashtable optionset;
        /// This parameter takes a set of authentication methods the user can select
        /// from. The available method are an enum called Authentication in the
        /// System.Management.Automation.Runspaces namespace. The available options
        /// should be as follows:
        /// - Default : Use the default authentication (ad defined by the underlying
        /// protocol) for establishing a remote connection.
        /// - Negotiate
        /// - Kerberos
        /// - Basic:  Use basic authentication for establishing a remote connection.
        /// -CredSSP: Use CredSSP authentication for establishing a remote connection
        /// which will enable the user to perform credential delegation. (i.e. second
        /// hop)
        public AuthenticationMechanism Authentication
            get { return authentication; }
            set { authentication = value; }
        private AuthenticationMechanism authentication = AuthenticationMechanism.Default;
        /// Specifies the certificate thumbprint to be used to impersonate the user on the
        /// remote machine.
        public string CertificateThumbprint
            get { return thumbPrint; }
            set { thumbPrint = value; }
        private string thumbPrint = null;
        /// Defines a set of extended options for the WSMan session.  This hashtable can
        /// be created using New-WSManSessionOption.
        [Alias("SO")]
        public SessionOption SessionOption
            get { return sessionoption; }
            set { sessionoption = value; }
        private SessionOption sessionoption;
        /// The following is the definition of the input parameter "ApplicationName".
        /// ApplicationName identifies the remote endpoint.
        [Parameter(ParameterSetName = "nameSet")]
        public string ApplicationName
            get { return applicationname; }
            set { applicationname = value; }
        private string applicationname = "wsman";
        /// Specifies the port to be used when connecting to the ws management service.
        public int Port
            get { return port; }
            set { port = value; }
        private int port = 0;
        /// Uses the Secure Sockets Layer (SSL) protocol to establish a connection to
        /// the remote computer. If SSL is not available on the port specified by the
        /// Port parameter, the command fails.
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SSL")]
        public SwitchParameter UseSSL
            get { return usessl; }
            set { usessl = value; }
        /// The following is the definition of the input parameter "ConnectionURI".
        /// Specifies the transport, server, port, and ApplicationName of the new
        /// runspace. The format of this string is:
        /// transport://server:port/ApplicationName.
        [Parameter(ParameterSetName = "pathSet", Mandatory = true)]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "URI")]
        public Uri ConnectionURI
            get { return connectionuri; }
            set { connectionuri = value; }
        private Uri connectionuri;
    /// Plugin Dynamic parameter. There are 2 parameter sets.
    /// Path - WSMan:\Localhost\Plugin>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Plugin")]
    public class WSManProviderNewItemPluginParameters
        /// Parameter Plugin.
        [Parameter(Mandatory = true, ParameterSetName = "pathSet")]
        public string Plugin
            get { return _plugin; }
            set { _plugin = value; }
        private string _plugin;
        /// Parameter FileName.
            get { return _filename; }
            set { _filename = value; }
        /// Parameter SDKVersion.
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SDK")]
        public string SDKVersion
            get { return _sdkversion; }
            set { _sdkversion = value; }
        private string _sdkversion;
        /// Parameter Resource.
        public System.Uri Resource
            get { return _resourceuri; }
            set { _resourceuri = value; }
        private System.Uri _resourceuri;
        /// Parameter Capability.
        public object[] Capability
            get { return _capability; }
            set { _capability = value; }
        private object[] _capability;
        /// Parameter XMLRenderingType.
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "XML")]
        public string XMLRenderingType
            get { return _xmlRenderingtype; }
            set { _xmlRenderingtype = value; }
        private string _xmlRenderingtype;
        /// Parameter File.
        [Parameter(Mandatory = true, ParameterSetName = "nameSet")]
        public string File
            get { return _file; }
            set { _file = value; }
        private string _file;
        /// Parameter for RunAs credentials for a Plugin.
        public PSCredential RunAsCredential
            get { return this.runAsCredentials; }
            set { this.runAsCredentials = value; }
        private PSCredential runAsCredentials;
        /// Parameter for Plugin Host Process configuration (Shared or Separate).
        public SwitchParameter UseSharedProcess
            get { return this.sharedHost; }
            set { this.sharedHost = value; }
        private bool sharedHost;
        /// Parameter for Auto Restart configuration for Plugin.
        public SwitchParameter AutoRestart
            get { return this.autoRestart; }
            set { this.autoRestart = value; }
        private bool autoRestart;
        /// Parameter for Idle timeout for HostProcess.
        public uint? ProcessIdleTimeoutSec
                return this.processIdleTimeoutSeconds;
                this.processIdleTimeoutSeconds = value;
        private uint? processIdleTimeoutSeconds;
    /// Initparameters dynamic parameters
    /// Path - wsman:\localhost\plugin\[specified plugin]\Initializationparameters>
    public class WSManProviderInitializeParameters
        /// Parameter ParamName.
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Param")]
        public string ParamName
            get { return _paramname; }
            set { _paramname = value; }
        private string _paramname;
        /// Parameter ParamValue.
        public string ParamValue
            get { return _paramvalue; }
            set { _paramvalue = value; }
        private string _paramvalue;
    /// Dynamic parameter for Resource Item.
    /// Path - WsMAn:\localhost\Plugin\[Specified Plugin]\Resources>
    public class WSManProviderNewItemResourceParameters
        /// Parameter ResourceUri.
        public System.Uri ResourceUri
    /// Security Dynamic Parameters
    /// Path - WsMan:\Localhost\Plugin\[Specified Plugin]\Resources\[Specified Resource]\Security>
    public class WSManProviderNewItemSecurityParameters
        /// Parameter Sddl.
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sddl")]
        public string Sddl
            get { return _sddl; }
            set { _sddl = value; }
        private string _sddl;
    #region "ClientCertificate Dynamic Parameters"
    /// Client Certificate Dynamic Parameters
    /// Path - WsMan:\Localhost\ClientCertificate.
    public class WSManProviderClientCertificateParameters
        /// Parameter Issuer.
        public string Issuer
                return _issuer;
                _issuer = value;
        private string _issuer;
        /// Parameter Subject.
        public string Subject
                return _subject;
                _subject = value;
        private string _subject = "*";
        /// Parameter URI.
        public System.Uri URI
                return _uri;
        private System.Uri _uri = new Uri("*", UriKind.RelativeOrAbsolute);
        /// Parameter Enabled.
        public bool Enabled
                return _enabled;
                _enabled = value;
        private bool _enabled = true;
    #region Listener Dynamic Parameters
    /// Listener Dynamic parameters
    /// Path - WsMan:\Localhost\Listener>
    public class WSManProvidersListenerParameters
        /// Parameter Address.
        public string Address
                return _address;
                _address = value;
        private string _address;
        /// Parameter Transport.
        public string Transport
                return _transport;
                _transport = value;
        private string _transport = "http";
        /// Parameter Port.
                return _port;
                _port = value;
                _IsPortSpecified = true;
        private int _port = 0;
        /// Parameter HostName.
        public string HostName
                return _hostName;
                _hostName = value;
        private string _hostName;
        /// Parameter URLPrefix.
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "URL")]
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
        public string URLPrefix
                return _urlprefix;
                _urlprefix = value;
        private string _urlprefix = "wsman";
        /// Parameter CertificateThumbPrint.
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ThumbPrint")]
        public string CertificateThumbPrint
                return _certificatethumbprint;
                _certificatethumbprint = value;
        private string _certificatethumbprint;
        /// Variable IsPortSpecified.
        public bool IsPortSpecified
                return _IsPortSpecified;
                _IsPortSpecified = value;
        private bool _IsPortSpecified = false;
    #region SetItemDynamicParameters
    /// Set-Item Dynamic parameters
    /// Path - WsMan:\Localhost\Client> Set-Item .\TrustedHosts.
    public class WSManProviderSetItemDynamicParameters
        /// Parameter Concatenate.
        public SwitchParameter Concatenate
            get { return _concatenate; }
            set { _concatenate = value; }
        private SwitchParameter _concatenate = false;
    #endregion SetItemDynamicParameters
    #region "String Literals"
    internal static class WSManStringLiterals
        // constants
        /// The default path separator used by the base implementation of the providers.
        internal const char DefaultPathSeparator = '\\';
        /// The alternate path separator used by the base implementation of the providers.
        internal const char AlternatePathSeparator = '/';
        /// Double Quotes used while constructing XML.
        internal const char EnclosingDoubleQuotes = '\"';
        /// Equalto Used while constructing XML.
        internal const char Equalto = '=';
        /// For XML Construction.
        internal const char GreaterThan = '>';
        /// XML Closing Tag.
        internal const string XmlClosingTag = "/>";
        /// White space used while constructing XML.
        internal const char SingleWhiteSpace = ' ';
        /// Root node of WsMan.
        internal const string ProviderName = "WSMan";
        internal const string WsMan_Schema = "http://schemas.microsoft.com/wbem/wsman/1/config";
        internal const string NS_XSI = "xmlns:xsi=" + "\"http://www.w3.org/2001/XMLSchema-instance\"";
        internal const string ATTR_NIL = "xsi:nil=" + "\"true\"";
        internal const string ATTR_NIL_NAME = "xsi:nil";
        internal const char WinrmPathSeparator = '/';
        internal const string rootpath = "WSMan";
        internal const string ContainerChildValue = "Container";
        #region WsMan Containers
        /// Plugin Container.
        internal const string containerPlugin = "Plugin";
        /// Client Container.
        internal const string containerClient = "Client";
        /// Shell Container.
        internal const string containerShell = "Shell";
        /// ClientCertificate Container.
        internal const string containerClientCertificate = "ClientCertificate";
        /// Listener Container.
        internal const string containerListener = "Listener";
        /// Service Container.
        internal const string containerService = "Service";
        /// Auth Container - Under Client,Service.
        internal const string containerAuth = "Auth";
        /// DefaultPorts Container - Under Client,Service.
        internal const string containerDefaultPorts = "DefaultPorts";
        /// TrustedHosts Container - Under Client,Service.
        internal const string containerTrustedHosts = "TrustedHosts";
        /// Security Container - Under Plugin.
        internal const string containerSecurity = "Security";
        /// Resources Container - Under Plugin.
        internal const string containerResources = "Resources";
        /// Resource in Resources Container - Under Plugin.
        internal const string containerSingleResource = "Resource";
        /// InitParameters Container - Under Plugin.
        internal const string containerInitParameters = "InitializationParameters";
        /// Quotas Container - Under Plugin.
        internal const string containerQuotasParameters = "Quotas";
        /// Winrs Container - Exposed as Shell.
        internal const string containerWinrs = "Winrs";
        /// Certmapping Container - Exposed as ClientCertificate in the provider.
        internal const string containerCertMapping = "Service/certmapping";
        /// Possible Values in Plugin Top Level XML.
        internal static readonly string[] NewItemPluginConfigParams =
            new string[] {
                "Filename",
                "SDKVersion",
                "XmlRenderingType",
                "Enabled",
                "Architecture",
                WSManStringLiterals.ConfigAutoRestart,
                WSManStringLiterals.ConfigProcessIdleTimeoutSec,
                WSManStringLiterals.ConfigUseSharedProcess,
        /// Possible Values in Plugin Top Resources XML
        internal static readonly string[] NewItemResourceParams = new string[] { "Resource", "ResourceUri", "Capability", "ExactMatch", "SupportsOptions" };
        /// Possible Values in Plugin Top Init Param XML
        internal static readonly string[] NewItemInitParamsParams = new string[] { "Name", "Value" };
        /// Possible Values in Plugin Top Security XML
        internal static readonly string[] NewItemSecurityParams = new string[] { "Uri", "Sddl", "ExactMatch" };
        #endregion WsMan Containers
        #region WSMAN Config Names
        /// Name of the configuration which represents RunAs Password.
        internal const string ConfigRunAsPasswordName = "RunAsPassword";
        /// Name of the configuration which represents RunAs Name.
        internal const string ConfigRunAsUserName = "RunAsUser";
        /// Name of the configuration which represents if HostProcess is shared or separate.
        internal const string ConfigUseSharedProcess = "UseSharedProcess";
        /// Name of the configuration which represents if AutoRestart is enabled or not.
        internal const string ConfigAutoRestart = "AutoRestart";
        /// Name of the configuration which represents the Host Idle Timeout in seconds.
        internal const string ConfigProcessIdleTimeoutSec = "ProcessIdleTimeoutSec";
        /// Name of the configuration which represents the Resource URI for a Resource.
        internal const string ConfigResourceUriName = "ResourceUri";
        /// Name of the tag which represents a initialization parameter.
        internal const string ConfigInitializeParameterTag = "Param";
        /// Name of the tag which represents Name of the parameter.
        internal const string ConfigInitializeParameterName = "Name";
        /// Name of the tag which represents Value of the parameter.
        internal const string ConfigInitializeParameterValue = "Value";
        /// Name of the tag which represents a Security URI.
        internal const string ConfigSecurityUri = "Uri";
        internal const string HiddenSuffixForSourceOfValue = "___Source";
        /// This is used to start the service. return a bool value. if false we throw error.
        public const string StartWinrmServiceSBFormat = @"
function Start-WSManServiceD15A7957836142a18627D7E1D342DD82
[CmdletBinding()]
    [bool]
    $Force,
    $captionForStart,
    $queryForStart)
    begin
        if ($force -or $pscmdlet.ShouldContinue($queryForStart, $captionForStart))
            Restart-Service  WinRM -Force -Confirm:$false
            return $true
        return $false
    }} #end of Begin block
$_ | Start-WSManServiceD15A7957836142a18627D7E1D342DD82 -force $args[0] -captionForStart $args[1] -queryForStart $args[2]
    #endregion "String Literals"
    #region "WsMan Output Objects"
    /// Base Output object.
    public class WSManConfigElement
        internal WSManConfigElement()
        internal WSManConfigElement(string name, string typenameofelement)
            _name = name;
            _typenameofelement = typenameofelement;
        /// Variable Name.
            get { return _name; }
            set { _name = value; }
        /// Variable TypeNameOfElement.
        public string TypeNameOfElement
            get { return _typenameofelement; }
            set { _typenameofelement = value; }
        private string _typenameofelement;
        /// Gets or Sets the type if the object.
        public string Type
    /// Leaf Element.
    public class WSManConfigLeafElement : WSManConfigElement
        internal WSManConfigLeafElement()
        internal WSManConfigLeafElement(string Name, object Value, string TypeNameOfElement, object SourceOfValue = null)
            _value = Value;
            _SourceOfValue = SourceOfValue;
            base.Name = Name;
            base.Type = TypeNameOfElement;
        /// Variable Value.
        public object SourceOfValue
            get { return _SourceOfValue; }
            set { _SourceOfValue = value; }
        private object _SourceOfValue;
            get { return _value; }
            set { _value = value; }
        private object _value;
    /// Container Element.
    public class WSManConfigContainerElement : WSManConfigElement
        internal WSManConfigContainerElement(string Name, string TypeNameOfElement, string[] keys)
            _keys = keys;
        /// Variable Keys.
        public string[] Keys
            get { return _keys; }
            set { _keys = value; }
        private string[] _keys;
    #endregion "WsMan Output Objects"

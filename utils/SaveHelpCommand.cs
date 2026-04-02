    /// This class implements the Save-Help cmdlet.
    [Cmdlet(VerbsData.Save, "Help", DefaultParameterSetName = SaveHelpCommand.PathParameterSetName,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096794")]
    public sealed class SaveHelpCommand : UpdatableHelpCommandBase
        public SaveHelpCommand() : base(UpdatableHelpCommandType.SaveHelpCommand)
        private bool _alreadyCheckedOncePerDayPerModule = false;
        /// Specifies the paths to save updates to.
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = PathParameterSetName)]
        public string[] DestinationPath
        /// Specifies the literal path to save updates to.
        [Parameter(Mandatory = true, ParameterSetName = LiteralPathParameterSetName)]
        /// Specifies the modules to update.
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, ValueFromPipeline = true, ParameterSetName = PathParameterSetName)]
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, ValueFromPipeline = true, ParameterSetName = LiteralPathParameterSetName)]
        [ArgumentToModuleTransformation]
        public PSModuleInfo[] Module { get; set; }
        /// Specifies the Module Specifications to update.
        [Parameter(ParameterSetName = PathParameterSetName, ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = LiteralPathParameterSetName, ValueFromPipelineByPropertyName = true)]
        public ModuleSpecification[] FullyQualifiedModule { get; set; }
                if (Module != null && FullyQualifiedModule != null)
                    string errMsg = StringUtil.Format(SessionStateStrings.GetContent_TailAndHeadCannotCoexist, "Module", "FullyQualifiedModule");
                List<string> moduleNames = null;
                List<PSModuleInfo> moduleInfos = null;
                    moduleNames = new List<string>();
                    moduleInfos = new List<PSModuleInfo>();
                    foreach (PSModuleInfo moduleInfo in Module)
                        // WinBlue: 268863
                        // this check will cover the cases where
                        // user supplied just name with -Module parameter.
                        // In other cases, user must have supplied either
                        // PSModuleInfo or Deserialized PSModuleInfo
                        if (string.IsNullOrEmpty(moduleInfo.ModuleBase))
                            moduleNames.Add(moduleInfo.Name);
                            moduleInfos.Add(moduleInfo);
                base.Process(moduleNames, FullyQualifiedModule);
                base.Process(moduleInfos);
                ProgressRecord progress = new ProgressRecord(activityId, HelpDisplayStrings.SaveProgressActivityForModule, HelpDisplayStrings.UpdateProgressInstalling);
        /// Process a single module with a given culture.
        /// <param name="module">Module to process.</param>
        /// <param name="culture">Culture to use.</param>
        /// <returns>True if the module has been processed, false if not.</returns>
        internal override bool ProcessModuleWithCulture(UpdatableHelpModuleInfo module, string culture)
            // Search for the HelpInfo XML
            foreach (string path in _path)
                UpdatableHelpSystemDrive helpInfoDrive = null;
                        PSArgumentException e = new PSArgumentException(StringUtil.Format(HelpDisplayStrings.PathNullOrEmpty));
                    string destPath = path;
                        if (path.Contains('*'))
                            // Deal with wildcards
                            int index = path.IndexOf('*');
                                throw new UpdatableHelpSystemException("PathMustBeValidContainers",
                                    StringUtil.Format(HelpDisplayStrings.PathMustBeValidContainers, path), ErrorCategory.InvalidArgument,
                                    null, new ItemNotFoundException());
                                int i = index;
                                    if (path[i].Equals('/') || path[i].Equals('\\'))
                                helpInfoDrive = new UpdatableHelpSystemDrive(this, path.Substring(0, i), _credential);
                                destPath = Path.Combine(helpInfoDrive.DriveName, path.Substring(i + 1, path.Length - (i + 1)));
                            helpInfoDrive = new UpdatableHelpSystemDrive(this, path, _credential);
                            destPath = helpInfoDrive.DriveName;
                        string destinationPath = GetUnresolvedProviderPathFromPSPath(destPath);
                        if (!Directory.Exists(destinationPath))
                        resolvedPaths.Add(destinationPath);
                            foreach (string tempPath in ResolvePath(destPath, false, false))
                                resolvedPaths.Add(tempPath);
                                StringUtil.Format(HelpDisplayStrings.PathMustBeValidContainers, path), ErrorCategory.InvalidArgument, null, e);
                    helpInfoDrive?.Dispose();
            bool installed = false;
                UpdatableHelpInfo currentHelpInfo = null;
                UpdatableHelpInfo newHelpInfo = null;
                // if -force is specified, no need to read from the current HelpInfo.xml file
                // because it won't be used for checking "IsUpdateNecessary"
                string xml = _force
                                 : UpdatableHelpSystem.LoadStringFromPath(this,
                                                                          SessionState.Path.Combine(path, module.GetHelpInfoName()),
                                                                          _credential);
                if (xml != null)
                    // constructing the helpinfo object from previous update help log xml..
                    // no need to resolve the uri's in this case.
                    currentHelpInfo = _helpSystem.CreateHelpInfo(xml, module.ModuleName, module.ModuleGuid,
                                                                 currentCulture: null, pathOverride: null, verbose: false,
                                                                 shouldResolveUri: false, ignoreValidationException: false);
                // Don't update too frequently
                if (!_alreadyCheckedOncePerDayPerModule && !CheckOncePerDayPerModule(module.ModuleName, path, module.GetHelpInfoName(), DateTime.UtcNow, _force))
                _alreadyCheckedOncePerDayPerModule = true;
                // Form the actual HelpInfo.xml uri
                helpInfoUri = _helpSystem.GetHelpInfoUri(module, null).ResolvedUri;
                string uri = helpInfoUri + module.GetHelpInfoName();
                newHelpInfo = _helpSystem.GetHelpInfo(_commandType, uri, module.ModuleName, module.ModuleGuid, culture);
                if (newHelpInfo == null)
                    throw new UpdatableHelpSystemException("UnableToRetrieveHelpInfoXml",
                        StringUtil.Format(HelpDisplayStrings.UnableToRetrieveHelpInfoXml, culture), ErrorCategory.ResourceUnavailable,
                        null, null);
                string tempPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetTempFileName()));
                foreach (UpdatableHelpUri contentUri in newHelpInfo.HelpContentUriCollection)
                    if (!IsUpdateNecessary(module, currentHelpInfo, newHelpInfo, contentUri.Culture, _force))
                        WriteVerbose(StringUtil.Format(HelpDisplayStrings.SuccessfullyUpdatedHelpContent, module.ModuleName, HelpDisplayStrings.NewestContentAlreadyDownloaded,
                            contentUri.Culture.Name, newHelpInfo.GetCultureVersion(contentUri.Culture)));
                        installed = true;
                        Debug.Assert(helpInfoUri != null, "If we are here, helpInfoUri must not be null");
                        string helpContentUri = contentUri.ResolvedUri;
                        string helpContentName = module.GetHelpContentName(contentUri.Culture);
                        UpdatableHelpSystemDrive helpContentDrive = null;
                            if (Directory.Exists(helpContentUri))
                                File.Copy(SessionState.Path.Combine(helpContentUri, helpContentName),
                                    SessionState.Path.Combine(path, helpContentName), true);
                                // Remote
                                        helpContentDrive = new UpdatableHelpSystemDrive(this, path, _credential);
                                        if (!_helpSystem.DownloadHelpContent(_commandType, tempPath, helpContentUri, helpContentName, culture))
                                            installed = false;
                                        InvokeProvider.Item.Copy(new string[1] { tempPath }, helpContentDrive.DriveName, true, CopyContainers.CopyChildrenOfTargetContainer,
                                            true, true);
                                        ProcessException(module.ModuleName, contentUri.Culture.Name, e);
                                    if (!_helpSystem.DownloadHelpContent(_commandType, path, helpContentUri, helpContentName, culture))
                                _helpSystem.GenerateHelpInfo(module.ModuleName, module.ModuleGuid, newHelpInfo.UnresolvedUri, contentUri.Culture.Name, newHelpInfo.GetCultureVersion(contentUri.Culture), tempPath,
                                    module.GetHelpInfoName(), _force);
                                InvokeProvider.Item.Copy(new string[1] { Path.Combine(tempPath, module.GetHelpInfoName()) }, Path.Combine(helpContentDrive.DriveName, module.GetHelpInfoName()), false,
                                    CopyContainers.CopyTargetContainer, true, true);
                                _helpSystem.GenerateHelpInfo(module.ModuleName, module.ModuleGuid, newHelpInfo.UnresolvedUri, contentUri.Culture.Name, newHelpInfo.GetCultureVersion(contentUri.Culture), path,
                            WriteVerbose(StringUtil.Format(HelpDisplayStrings.SuccessfullyUpdatedHelpContent, module.ModuleName,
                                StringUtil.Format(HelpDisplayStrings.SavedHelpContent, System.IO.Path.Combine(path, helpContentName)), contentUri.Culture.Name,
                                newHelpInfo.GetCultureVersion(contentUri.Culture)));
                            LogMessage(StringUtil.Format(HelpDisplayStrings.SaveHelpCompleted, path));
                            helpContentDrive?.Dispose();
            return installed;
    internal sealed class ArgumentToModuleTransformationAttribute : ArgumentTransformationAttribute
            object argument = PSObject.Base(inputData);
            // deal with scalar string argument
            var strArg = argument as string;
            if (strArg != null)
                return new PSModuleInfo(name: strArg, path: null, context: null, sessionState: null);
            // deal with IList argument
            IList iListArg = ParameterBinderBase.GetIList(argument);
            if (iListArg != null && iListArg.Count > 0)
                int elementCount = iListArg.Count;
                int targetIndex = 0;
                var target = Array.CreateInstance(typeof(object), elementCount);
                foreach (object element in iListArg)
                    var elementValue = PSObject.Base(element);
                    if (elementValue is PSModuleInfo)
                        target.SetValue(elementValue, targetIndex++);
                    else if (elementValue is string)
                        var elementAsModuleObj = new PSModuleInfo(name: (string)elementValue, path: null, context: null, sessionState: null);
                        target.SetValue(elementAsModuleObj, targetIndex++);
                        PSModuleInfo elementValueModuleInfo = null;
                        if (TryConvertFromDeserializedModuleInfo(elementValue, out elementValueModuleInfo))
                            target.SetValue(elementValueModuleInfo, targetIndex++);
                            target.SetValue(element, targetIndex++);
            if (TryConvertFromDeserializedModuleInfo(inputData, out moduleInfo))
        private static bool TryConvertFromDeserializedModuleInfo(object inputData, out PSModuleInfo moduleInfo)
            PSObject pso = inputData as PSObject;
            if (Deserializer.IsDeserializedInstanceOfType(pso, typeof(PSModuleInfo)))
                LanguagePrimitives.TryConvertTo<string>(pso.Properties["Name"].Value, out moduleName);
                Guid moduleGuid;
                LanguagePrimitives.TryConvertTo<Guid>(pso.Properties["Guid"].Value, out moduleGuid);
                LanguagePrimitives.TryConvertTo<Version>(pso.Properties["Version"].Value, out moduleVersion);
                string helpInfoUri;
                LanguagePrimitives.TryConvertTo<string>(pso.Properties["HelpInfoUri"].Value, out helpInfoUri);
                moduleInfo = new PSModuleInfo(name: moduleName, path: null, context: null, sessionState: null);
                moduleInfo.SetGuid(moduleGuid);
                moduleInfo.SetVersion(moduleVersion);
                moduleInfo.SetHelpInfoUri(helpInfoUri);
                // setting the base to temp directory as this is a deserialized
                // module info.
                moduleInfo.SetModuleBase(System.IO.Path.GetTempPath());

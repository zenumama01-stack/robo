    /// This class implements the Update-Help cmdlet.
    [Cmdlet(VerbsData.Update, "Help", DefaultParameterSetName = PathParameterSetName,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096805")]
    public sealed class UpdateHelpCommand : UpdatableHelpCommandBase
        public UpdateHelpCommand() : base(UpdatableHelpCommandType.UpdateHelpCommand)
        [Parameter(Position = 0, ParameterSetName = PathParameterSetName, ValueFromPipelineByPropertyName = true)]
        [Parameter(Position = 0, ParameterSetName = LiteralPathParameterSetName, ValueFromPipelineByPropertyName = true)]
                return _module;
                _module = value;
        private string[] _module;
        /// Specifies the paths to update from.
        [Parameter(Position = 1, ParameterSetName = PathParameterSetName)]
        public string[] SourcePath
        /// Scans paths recursively.
        /// Begin processing.
                // Pull default source path from GP
                string defaultSourcePath = _helpSystem.GetDefaultSourcePath();
                if (defaultSourcePath != null)
                    _path = new string[1] { defaultSourcePath };
        /// Main cmdlet logic.
                    if (_path == null && Recurse.IsPresent)
                        PSArgumentException e = new PSArgumentException(StringUtil.Format(HelpDisplayStrings.CannotSpecifyRecurseWithoutPath));
                // check if there is an UI, if not Throw out terminating error. 
                if (!cultures.Any()) 
                    string cultureString = string.IsNullOrEmpty(CultureInfo.CurrentCulture.Name) ? CultureInfo.CurrentCulture.DisplayName : CultureInfo.CurrentCulture.Name;
                    string errMsg = StringUtil.Format(HelpDisplayStrings.FailedToUpdateHelpWithLocaleNoUICulture, cultureString);
                    ErrorRecord error = new ErrorRecord(new InvalidOperationException(errMsg), "FailedToUpdateHelpWithLocaleNoUICulture", ErrorCategory.InvalidOperation, targetObject: null);                    
                base.Process(_module, FullyQualifiedModule);
                // Reset the per-runspace help cache
                foreach (HelpProvider provider in Context.HelpSystem.HelpProviders)
                    provider.Reset();
                ProgressRecord progress = new ProgressRecord(activityId, HelpDisplayStrings.UpdateProgressActivityForModule, HelpDisplayStrings.UpdateProgressInstalling);
            // Simulate culture not found
            if (InternalTestHooks.ThrowHelpCultureNotSupported)
                    StringUtil.Format(HelpDisplayStrings.HelpCultureNotSupported, culture, "en-US"),
            string moduleBase = module.ModuleBase;
            if (this.Scope == UpdateHelpScope.CurrentUser)
                moduleBase = HelpUtils.GetModuleBaseForUserHelp(moduleBase, module.ModuleName);
            // reading the xml file even if force is specified
            // Reason: we need the current version for ShouldProcess
            string xml = UpdatableHelpSystem.LoadStringFromPath(this,
                 SessionState.Path.Combine(moduleBase, module.GetHelpInfoName()),
                                                             shouldResolveUri: false,
                                                             // ignore validation exception if _force is true
                                                             ignoreValidationException: _force);
            if (!_alreadyCheckedOncePerDayPerModule && !CheckOncePerDayPerModule(module.ModuleName, moduleBase, module.GetHelpInfoName(), DateTime.UtcNow, _force))
                            string sourcePath = path;
                                UpdatableHelpSystemDrive drive = new UpdatableHelpSystemDrive(this, path, _credential);
                                sourcePath = drive.DriveName;
                            foreach (string tempPath in ResolvePath(sourcePath, _recurse, _isLiteralPath))
                        catch (System.Management.Automation.DriveNotFoundException e)
                            ThrowPathMustBeValidContainersException(path, e);
                    // Everything in resolvedPaths is a container
                        string literalPath = SessionState.Path.Combine(resolvedPath, module.GetHelpInfoName());
                        xml = UpdatableHelpSystem.LoadStringFromPath(this, literalPath, _credential);
                            newHelpInfo = _helpSystem.CreateHelpInfo(xml, module.ModuleName, module.ModuleGuid, culture, resolvedPath,
                                                                     verbose: false, shouldResolveUri: true, ignoreValidationException: false);
                            helpInfoUri = resolvedPath;
                        null, e);
                newHelpInfo = _helpSystem.GetHelpInfo(UpdatableHelpCommandType.UpdateHelpCommand, uri, module.ModuleName, module.ModuleGuid, culture);
                Version currentHelpVersion = currentHelpInfo?.GetCultureVersion(contentUri.Culture);
                string updateHelpShouldProcessAction = string.Format(CultureInfo.InvariantCulture,
                    HelpDisplayStrings.UpdateHelpShouldProcessActionMessage,
                    module.ModuleName,
                    (currentHelpVersion != null) ? currentHelpVersion.ToString() : "0.0.0.0",
                    newHelpInfo.GetCultureVersion(contentUri.Culture),
                    contentUri.Culture);
                if (!this.ShouldProcess(updateHelpShouldProcessAction, "Update-Help"))
                if (Utils.IsUnderProductFolder(moduleBase) && (!Utils.IsAdministrator()))
                    string message = StringUtil.Format(HelpErrors.UpdatableHelpRequiresElevation);
                    ProcessException(module.ModuleName, null, new UpdatableHelpSystemException("UpdatableHelpSystemRequiresElevation",
                            message, ErrorCategory.InvalidOperation, null, null));
                if (!IsUpdateNecessary(module, _force ? null : currentHelpInfo, newHelpInfo, contentUri.Culture, _force))
                    WriteVerbose(StringUtil.Format(HelpDisplayStrings.SuccessfullyUpdatedHelpContent, module.ModuleName, HelpDisplayStrings.NewestContentAlreadyInstalled,
                        string xsdPath = SessionState.Path.Combine(Utils.GetApplicationBase(Context.ShellID), "Schemas\\PSMaml\\maml.xsd"); // TODO: Edit the maml XSDs and change this
                        // Gather destination paths
                        Collection<string> destPaths = new Collection<string>();
                        if (!Directory.Exists(moduleBase))
                            Directory.CreateDirectory(moduleBase);
                        destPaths.Add(moduleBase);
#if !CORECLR // Side-By-Side directories are not present in OneCore environments.
                        if (IsSystemModule(module.ModuleName) && Environment.Is64BitOperatingSystem)
                            string path = Utils.DefaultPowerShellAppBase.Replace("System32", "SysWOW64");
                            destPaths.Add(path);
                        Collection<string> filesInstalled;
                                string tempContentPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
                                    using (UpdatableHelpSystemDrive drive = new UpdatableHelpSystemDrive(this, helpContentUri, _credential))
                                        if (!Directory.Exists(tempContentPath))
                                            Directory.CreateDirectory(tempContentPath);
                                        InvokeProvider.Item.Copy(new string[1] { Path.Combine(drive.DriveName, helpContentName) },
                                            Path.Combine(tempContentPath, helpContentName), false, CopyContainers.CopyTargetContainer, true, true);
                                        // Local
                                        _helpSystem.InstallHelpContent(UpdatableHelpCommandType.UpdateHelpCommand, Context, tempContentPath,
                                            destPaths, module.GetHelpContentName(contentUri.Culture),
                                            Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName())),
                                            contentUri.Culture, xsdPath, out filesInstalled);
                                        ErrorCategory.ResourceUnavailable, null, e);
                                _helpSystem.InstallHelpContent(UpdatableHelpCommandType.UpdateHelpCommand, Context, helpContentUri,
                            // Download and install help content
                            if (!_helpSystem.DownloadAndInstallHelpContent(UpdatableHelpCommandType.UpdateHelpCommand, Context,
                                destPaths, module.GetHelpContentName(contentUri.Culture), contentUri.Culture, helpContentUri, xsdPath, out filesInstalled))
                        _helpSystem.GenerateHelpInfo(module.ModuleName, module.ModuleGuid, newHelpInfo.UnresolvedUri, contentUri.Culture.Name, newHelpInfo.GetCultureVersion(contentUri.Culture),
                            moduleBase, module.GetHelpInfoName(), _force);
                        foreach (string fileInstalled in filesInstalled)
                                StringUtil.Format(HelpDisplayStrings.UpdatedHelpContent, fileInstalled), contentUri.Culture.Name,
                        LogMessage(StringUtil.Format(HelpDisplayStrings.UpdateHelpCompleted));
        /// Throws PathMustBeValidContainers exception.
        private static void ThrowPathMustBeValidContainersException(string path, Exception e)

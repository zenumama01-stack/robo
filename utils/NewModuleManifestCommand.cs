    #region New-ModuleManifest
    /// Cmdlet to create a new module manifest file.
    [Cmdlet(VerbsCommon.New, "ModuleManifest", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Low,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096487")]
    public sealed class NewModuleManifestCommand : PSCmdlet
        /// Gets or sets the output path for the generated file.
        /// Gets or sets the list of files to load by default.
        public object[] NestedModules
            get { return _nestedModules; }
            set { _nestedModules = value; }
        private object[] _nestedModules;
        /// Gets or sets the GUID in the manifest file.
        public Guid Guid
            get { return _guid; }
            set { _guid = value; }
        private Guid _guid = Guid.NewGuid();
        /// Gets or sets the author string in the manifest.
        public string Author
            get { return _author; }
            set { _author = value; }
        private string _author;
        /// Gets or sets the company name in the manifest.
        public string CompanyName
            get { return _companyName; }
            set { _companyName = value; }
        private string _companyName = string.Empty;
        /// Gets or sets the copyright string in the module manifest.
        public string Copyright
            get { return _copyright; }
            set { _copyright = value; }
        private string _copyright;
        /// Gets or sets the root module.
        [Alias("ModuleToProcess")]
        public string RootModule
            get { return _rootModule; }
            set { _rootModule = value; }
        private string _rootModule = null;
        /// Gets or sets the module version.
            get { return _moduleVersion; }
            set { _moduleVersion = value; }
        private Version _moduleVersion = new Version(0, 0, 1);
        /// Gets or sets the module description.
            get { return _description; }
            set { _description = value; }
        /// Gets or sets the ProcessorArchitecture required by this module.
        public ProcessorArchitecture ProcessorArchitecture
            get { return _processorArchitecture ?? ProcessorArchitecture.None; }
            set { _processorArchitecture = value; }
        private ProcessorArchitecture? _processorArchitecture = null;
        /// Gets or sets the PowerShell version required by this module.
        public Version PowerShellVersion
            get { return _powerShellVersion; }
            set { _powerShellVersion = value; }
        private Version _powerShellVersion = null;
        /// Gets or sets the CLR version required by the module.
        public Version ClrVersion
            get { return _ClrVersion; }
            set { _ClrVersion = value; }
        private Version _ClrVersion = null;
        /// Gets or sets the version of .NET Framework required by the module.
        public Version DotNetFrameworkVersion
            get { return _DotNetFrameworkVersion; }
            set { _DotNetFrameworkVersion = value; }
        private Version _DotNetFrameworkVersion = null;
        /// Gets or sets the name of PowerShell host required by the module.
        public string PowerShellHostName
            get { return _PowerShellHostName; }
            set { _PowerShellHostName = value; }
        private string _PowerShellHostName = null;
        /// Gets or sets the version of PowerShell host required by the module.
        public Version PowerShellHostVersion
            get { return _PowerShellHostVersion; }
            set { _PowerShellHostVersion = value; }
        private Version _PowerShellHostVersion = null;
        /// Gets or sets the list of Dependencies for the module.
        [ArgumentTypeConverter(typeof(ModuleSpecification[]))]
        public object[] RequiredModules
            get { return _requiredModules; }
            set { _requiredModules = value; }
        private object[] _requiredModules;
        /// Gets or sets the list of types files for the module.
        public string[] TypesToProcess
            get { return _types; }
            set { _types = value; }
        private string[] _types;
        /// Gets or sets the list of formats files for the module.
        public string[] FormatsToProcess
            get { return _formats; }
            set { _formats = value; }
        private string[] _formats;
        /// Gets or sets the list of ps1 scripts to run in the session state of the import-module invocation.
        public string[] ScriptsToProcess
            get { return _scripts; }
            set { _scripts = value; }
        private string[] _scripts;
        /// Gets or sets the list of assemblies to load for this module.
        public string[] RequiredAssemblies
            get { return _requiredAssemblies; }
            set { _requiredAssemblies = value; }
        private string[] _requiredAssemblies;
        /// Gets or sets the additional files used by this module.
        public string[] FileList
            get { return _miscFiles; }
            set { _miscFiles = value; }
        private string[] _miscFiles;
        /// Gets or sets the list of other modules included with this module.
        /// Like the RequiredModules key, this list can be a simple list of module names or a complex list of module hashtables.
        public object[] ModuleList
            get { return _moduleList; }
            set { _moduleList = value; }
        private object[] _moduleList;
        /// Gets or sets the functions to export from this manifest.
        public string[] FunctionsToExport
            get { return _exportedFunctions; }
            set { _exportedFunctions = value; }
        private string[] _exportedFunctions;
        /// Gets or sets the aliases to export from this manifest.
        public string[] AliasesToExport
            get { return _exportedAliases; }
            set { _exportedAliases = value; }
        private string[] _exportedAliases;
        /// Gets or sets the variables to export from this manifest.
        public string[] VariablesToExport
            get { return _exportedVariables; }
            set { _exportedVariables = value; }
        private string[] _exportedVariables = new string[] { "*" };
        /// Gets or sets the cmdlets to export from this manifest.
        public string[] CmdletsToExport
            get { return _exportedCmdlets; }
            set { _exportedCmdlets = value; }
        private string[] _exportedCmdlets;
        /// Gets or sets the dsc resources to export from this manifest.
        public string[] DscResourcesToExport
            get { return _dscResourcesToExport; }
            set { _dscResourcesToExport = value; }
        private string[] _dscResourcesToExport;
        /// Gets or sets the compatible PSEditions of this module.
        [ValidateSet("Desktop", "Core")]
        public string[] CompatiblePSEditions
            get { return _compatiblePSEditions; }
            set { _compatiblePSEditions = value; }
        private string[] _compatiblePSEditions;
        /// Gets or sets the module-specific private data here.
        public object PrivateData
            get { return _privateData; }
            set { _privateData = value; }
        private object _privateData;
        /// Gets or sets the Tags.
        /// Gets or sets the ProjectUri.
        public Uri ProjectUri { get; set; }
        /// Gets or sets the LicenseUri.
        public Uri LicenseUri { get; set; }
        /// Gets or sets the IconUri.
        public Uri IconUri { get; set; }
        /// Gets or sets the ReleaseNotes.
        public string ReleaseNotes { get; set; }
        /// Gets or sets whether or not the module is a prerelease.
        public string Prerelease { get; set; }
        /// Gets or sets whether or not the module requires explicit user acceptance for install/update/save.
        public SwitchParameter RequireLicenseAcceptance { get; set; }
        /// Gets or sets the external module dependencies.
        public string[] ExternalModuleDependencies { get; set; }
        /// Gets or sets the HelpInfo URI.
        public string HelpInfoUri
            get { return _helpInfoUri; }
            set { _helpInfoUri = value; }
        private string _helpInfoUri;
        /// Gets or sets whether the module manifest string should go to the output stream.
            get { return (SwitchParameter)_passThru; }
        /// Gets or sets the Default Command Prefix.
        public string DefaultCommandPrefix
            get { return _defaultCommandPrefix; }
            set { _defaultCommandPrefix = value; }
        private string _defaultCommandPrefix;
        private string _indent = string.Empty;
        /// Return a single-quoted string. Any embedded single quotes will be doubled.
        /// <param name="name">The string to quote.</param>
        /// <returns>The quoted string.</returns>
        private static string QuoteName(string name)
                return "''";
            return ("'" + name.Replace("'", "''") + "'");
        /// Return a single-quoted string using the AbsoluteUri member to ensure it is escaped correctly.
        /// <param name="name">The Uri to quote.</param>
        /// <returns>The quoted AbsoluteUri.</returns>
        private static string QuoteName(Uri name)
            return QuoteName(name.AbsoluteUri);
        /// Return a single-quoted string from a Version object.
        /// <param name="name">The Version object to quote.</param>
        /// <returns>The quoted Version string.</returns>
        private static string QuoteName(Version name)
            return QuoteName(name.ToString());
        /// Takes a collection of strings and returns the collection
        /// quoted.
        /// <param name="names">The list to quote.</param>
        /// <param name="streamWriter">Streamwriter to get end of line character from.</param>
        /// <returns>The quoted list.</returns>
        private static string QuoteNames(IEnumerable names, StreamWriter streamWriter)
            if (names == null)
                return "@()";
            int offset = 15;
                        result.Append(", ");
                    string quotedString = QuoteName(name);
                    offset += quotedString.Length;
                    if (offset > 80)
                        result.Append(streamWriter.NewLine);
                        result.Append("               ");
                        offset = 15 + quotedString.Length;
                    result.Append(quotedString);
            if (result.Length == 0)
        /// This function is created to PreProcess -NestedModules in Win8.
        /// In Win7, -NestedModules is of type string[]. In Win8, we changed
        /// this to object[] to support module specification using hashtable.
        /// To be backward compatible, this function calls ToString() on any
        /// object that is not of type hashtable or string.
        /// <param name="moduleSpecs"></param>
        private static IEnumerable PreProcessModuleSpec(IEnumerable moduleSpecs)
            if (moduleSpecs != null)
                foreach (object spec in moduleSpecs)
                    if (spec is not Hashtable)
                        yield return spec.ToString();
                        yield return spec;
        /// Takes a collection of "module specifications" (string or hashtable)
        /// and returns the collection as a string that can be inserted into a module manifest.
        /// <param name="moduleSpecs">The list to quote.</param>
        private static string QuoteModules(IEnumerable moduleSpecs, StreamWriter streamWriter)
            result.Append("@(");
                bool firstModule = true;
                    if (spec == null)
                    ModuleSpecification moduleSpecification = (ModuleSpecification)LanguagePrimitives.ConvertTo(
                            spec,
                            typeof(ModuleSpecification),
                    if (!firstModule)
                    firstModule = false;
                    if ((moduleSpecification.Guid == null) && (moduleSpecification.Version == null) && (moduleSpecification.MaximumVersion == null) && (moduleSpecification.RequiredVersion == null))
                        result.Append(QuoteName(moduleSpecification.Name));
                        result.Append("@{");
                        result.Append("ModuleName = ");
                        result.Append("; ");
                        if (moduleSpecification.Guid != null)
                            result.Append("GUID = ");
                            result.Append(QuoteName(moduleSpecification.Guid.ToString()));
                        if (moduleSpecification.Version != null)
                            result.Append("ModuleVersion = ");
                            result.Append(QuoteName(moduleSpecification.Version.ToString()));
                            result.Append("MaximumVersion = ");
                            result.Append(QuoteName(moduleSpecification.MaximumVersion));
                            result.Append("RequiredVersion = ");
                            result.Append(QuoteName(moduleSpecification.RequiredVersion.ToString()));
                        result.Append('}');
        /// Takes a collection of file names and returns the collection
        private string QuoteFiles(IEnumerable names, StreamWriter streamWriter)
            List<string> resolvedPaths = new List<string>();
                        foreach (string path in TryResolveFilePath(name))
            return QuoteNames(resolvedPaths, streamWriter);
        ///// Takes a collection of file names and returns the collection
        ///// quoted.  It does not expand wildcard to actual files (as QuoteFiles does).
        ///// It throws an error when the entered filename is different than the allowedExtension.
        ///// If any file name falls outside the directory tree basPath a warning is issued.
        ///// <param name="basePath">This is the path which will be used to determine whether a warning is to be displayed.</param>
        ///// <param name="names">The list to quote</param>
        ///// <param name="allowedExtension">This is the allowed file extension, any other extension will give an error.</param>
        ///// <param name="streamWriter">Streamwriter to get end of line character from</param>
        ///// <param name="item">The item of the manifest file for which names are being resolved.</param>
        ///// <returns>The quoted list.</returns>
        // private string QuoteFilesWithWildcard(string basePath, IEnumerable names, string allowedExtension, StreamWriter streamWriter, string item)
        //    if (names != null)
        //        foreach (string name in names)
        //            if (string.IsNullOrEmpty(name))
        //                continue;
        //            string fileName = name;
        //            string extension = System.IO.Path.GetExtension(fileName);
        //            if (string.Equals(extension, allowedExtension, StringComparison.OrdinalIgnoreCase))
        //            {
        //                string drive = string.Empty;
        //                if (!SessionState.Path.IsPSAbsolute(fileName, out drive) && !System.IO.Path.IsPathRooted(fileName))
        //                {
        //                    fileName = SessionState.Path.Combine(SessionState.Path.CurrentLocation.ProviderPath, fileName);
        //                }
        //                string basePathDir = System.IO.Path.GetDirectoryName(SessionState.Path.GetUnresolvedProviderPathFromPSPath(basePath));
        //                if (basePathDir[basePathDir.Length - 1] != StringLiterals.DefaultPathSeparator)
        //                    basePathDir += StringLiterals.DefaultPathSeparator;
        //                string fileDir = null;
        //                // Call to SessionState.Path.GetUnresolvedProviderPathFromPSPath throws an exception
        //                // when the drive in the path does not exist.
        //                // Based on the exception it is obvious that the path is outside the basePath, because
        //                // basePath must always exist.
        //                try
        //                    fileDir = System.IO.Path.GetDirectoryName(SessionState.Path.GetUnresolvedProviderPathFromPSPath(fileName));
        //                    if (fileDir[fileDir.Length - 1] != StringLiterals.DefaultPathSeparator)
        //                    {
        //                        fileDir += StringLiterals.DefaultPathSeparator;
        //                    }
        //                catch
        //                if (fileDir == null
        //                    || !fileDir.StartsWith(basePathDir, StringComparison.OrdinalIgnoreCase))
        //                    WriteWarning(StringUtil.Format(Modules.IncludedItemPathFallsOutsideSaveTree, name,
        //                        fileDir ?? name, item));
        //            }
        //            else
        //                string message = StringUtil.Format(Modules.InvalidWorkflowExtension);
        //                InvalidOperationException invalidOp = new InvalidOperationException(message);
        //                ErrorRecord er = new ErrorRecord(invalidOp, "Modules_InvalidWorkflowExtension",
        //                    ErrorCategory.InvalidOperation, null);
        //                ThrowTerminatingError(er);
        //    return QuoteNames(names, streamWriter);
        /// Glob a set of files then resolve them to relative paths.
        private List<string> TryResolveFilePath(string filePath)
            SessionState sessionState = Context.SessionState;
                    sessionState.Path.GetResolvedProviderPathFromPSPath(filePath, out provider);
                // If the name doesn't resolve to something we can use, just return the unresolved name...
                if (!provider.NameEquals(this.Context.ProviderNames.FileSystem) || filePaths == null || filePaths.Count < 1)
                    result.Add(filePath);
                // Otherwise get the relative resolved path and trim the .\ or ./ because
                // modules are always loaded relative to the manifest base directory.
                    string adjustedPath = SessionState.Path.NormalizeRelativePath(path,
                        SessionState.Path.CurrentLocation.ProviderPath);
                    if (adjustedPath.StartsWith(".\\", StringComparison.OrdinalIgnoreCase) ||
                        adjustedPath.StartsWith("./", StringComparison.OrdinalIgnoreCase))
                        adjustedPath = adjustedPath.Substring(2);
                    result.Add(adjustedPath);
        /// This routine builds a fragment of the module manifest file
        /// for a particular key. It returns a formatted string that includes
        /// a comment describing the key as well as the key and its value.
        /// <param name="key">The manifest key to use.</param>
        /// <param name="resourceString">ResourceString that holds the message.</param>
        /// <param name="value">The formatted manifest fragment.</param>
        private string ManifestFragment(string key, string resourceString, string value, StreamWriter streamWriter)
            return string.Format(CultureInfo.InvariantCulture, "{0}# {1}{2}{0}{3:19} = {4}{2}{2}", _indent, resourceString, streamWriter.NewLine, key, value);
        private string ManifestFragmentForNonSpecifiedManifestMember(string key, string resourceString, string value, StreamWriter streamWriter)
            return string.Format(CultureInfo.InvariantCulture, "{0}# {1}{2}{0}# {3:19} = {4}{2}{2}", _indent, resourceString, streamWriter.NewLine, key, value);
        private static string ManifestComment(string insert, StreamWriter streamWriter)
            // Prefix a non-empty string with a space for formatting reasons...
            if (!string.IsNullOrEmpty(insert))
                insert = " " + insert;
            return string.Format(CultureInfo.InvariantCulture, "#{0}{1}", insert, streamWriter.NewLine);
        /// Generate the module manifest...
            // Win8: 264471 - Error message for New-ModuleManifest -ProcessorArchitecture is obsolete.
            // If an undefined value is passed for the ProcessorArchitecture parameter, the error message from parameter binder includes all the values from the enum.
            // The value 'IA64' for ProcessorArchitecture is not supported. But since we do not own the enum System.Reflection.ProcessorArchitecture, we cannot control the values in it.
            // So, we add a separate check in our code to give an error if user specifies IA64
            if (ProcessorArchitecture == ProcessorArchitecture.IA64)
                string message = StringUtil.Format(Modules.InvalidProcessorArchitectureInManifest, ProcessorArchitecture);
                ErrorRecord er = new ErrorRecord(ioe, "Modules_InvalidProcessorArchitectureInManifest",
                    ErrorCategory.InvalidArgument, ProcessorArchitecture);
            string filePath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(_path, out provider, out drive);
            if (!provider.NameEquals(Context.ProviderNames.FileSystem) || !filePath.EndsWith(StringLiterals.PowerShellDataFileExtension, StringComparison.OrdinalIgnoreCase))
                string message = StringUtil.Format(Modules.InvalidModuleManifestPath, _path);
                ErrorRecord er = new ErrorRecord(ioe, "Modules_InvalidModuleManifestPath",
                    ErrorCategory.InvalidArgument, _path);
            // By default, we want to generate a module manifest the encourages the best practice of explicitly specifying
            // the commands exported (even if it's an empty array.) Unfortunately, changing the default breaks automation
            // (however unlikely, this cmdlet isn't really meant for automation). Instead of trying to detect interactive
            // use (which is quite hard), we infer interactive use if none of RootModule/NestedModules/RequiredModules is
            // specified - because the manifest needs to be edited to actually be of use in those cases.
            // If one of these parameters has been specified, default back to the old behavior by specifying
            // wildcards for exported commands that weren't specified on the command line.
            if (_rootModule != null || _nestedModules != null || _requiredModules != null)
                _exportedAliases ??= new string[] { "*" };
                _exportedCmdlets ??= new string[] { "*" };
                _exportedFunctions ??= new string[] { "*" };
            ValidateUriParameterValue(ProjectUri, "ProjectUri");
            ValidateUriParameterValue(LicenseUri, "LicenseUri");
            ValidateUriParameterValue(IconUri, "IconUri");
            if (_helpInfoUri != null)
                ValidateUriParameterValue(new Uri(_helpInfoUri), "HelpInfoUri");
            if (CompatiblePSEditions != null && (CompatiblePSEditions.Distinct(StringComparer.OrdinalIgnoreCase).Count() != CompatiblePSEditions.Length))
                string message = StringUtil.Format(Modules.DuplicateEntriesInCompatiblePSEditions, string.Join(',', CompatiblePSEditions));
                var er = new ErrorRecord(ioe, "Modules_DuplicateEntriesInCompatiblePSEditions", ErrorCategory.InvalidArgument, CompatiblePSEditions);
            string action = StringUtil.Format(Modules.CreatingModuleManifestFile, filePath);
            if (ShouldProcess(filePath, action))
                if (string.IsNullOrEmpty(_author))
                    _author = Environment.UserName;
                if (string.IsNullOrEmpty(_companyName))
                    _companyName = Modules.DefaultCompanyName;
                if (string.IsNullOrEmpty(_copyright))
                    _copyright = StringUtil.Format(Modules.DefaultCopyrightMessage, _author);
                FileInfo readOnlyFileInfo;
                // Now open the output file...
                    filePath: filePath,
                    resolvedEncoding: new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                    Append: false,
                    Force: false,
                    fileStream: out fileStream,
                    streamWriter: out streamWriter,
                    readOnlyFileInfo: out readOnlyFileInfo,
                    isLiteralPath: false
                    // Insert the formatted manifest header...
                    result.Append(ManifestComment(string.Empty, streamWriter));
                    result.Append(ManifestComment(StringUtil.Format(Modules.ManifestHeaderLine1, System.IO.Path.GetFileNameWithoutExtension(filePath)),
                            streamWriter));
                    result.Append(ManifestComment(StringUtil.Format(Modules.ManifestHeaderLine2, _author),
                    result.Append(ManifestComment(StringUtil.Format(Modules.ManifestHeaderLine3, DateTime.Now.ToString("d", CultureInfo.CurrentCulture)),
                    _rootModule ??= string.Empty;
                    BuildModuleManifest(result, nameof(RootModule), Modules.RootModule, !string.IsNullOrEmpty(_rootModule), () => QuoteName(_rootModule), streamWriter);
                    BuildModuleManifest(result, nameof(ModuleVersion), Modules.ModuleVersion, _moduleVersion != null && !string.IsNullOrEmpty(_moduleVersion.ToString()), () => QuoteName(_moduleVersion), streamWriter);
                    BuildModuleManifest(result, nameof(CompatiblePSEditions), Modules.CompatiblePSEditions, _compatiblePSEditions != null && _compatiblePSEditions.Length > 0, () => QuoteNames(_compatiblePSEditions, streamWriter), streamWriter);
                    BuildModuleManifest(result, nameof(Modules.GUID), Modules.GUID, !string.IsNullOrEmpty(_guid.ToString()), () => QuoteName(_guid.ToString()), streamWriter);
                    BuildModuleManifest(result, nameof(Author), Modules.Author, !string.IsNullOrEmpty(_author), () => QuoteName(Author), streamWriter);
                    BuildModuleManifest(result, nameof(CompanyName), Modules.CompanyName, !string.IsNullOrEmpty(_companyName), () => QuoteName(_companyName), streamWriter);
                    BuildModuleManifest(result, nameof(Copyright), Modules.Copyright, !string.IsNullOrEmpty(_copyright), () => QuoteName(_copyright), streamWriter);
                    BuildModuleManifest(result, nameof(Description), Modules.Description, !string.IsNullOrEmpty(_description), () => QuoteName(_description), streamWriter);
                    BuildModuleManifest(result, nameof(PowerShellVersion), Modules.PowerShellVersion, _powerShellVersion != null && !string.IsNullOrEmpty(_powerShellVersion.ToString()), () => QuoteName(_powerShellVersion), streamWriter);
                    BuildModuleManifest(result, nameof(PowerShellHostName), Modules.PowerShellHostName, !string.IsNullOrEmpty(_PowerShellHostName), () => QuoteName(_PowerShellHostName), streamWriter);
                    BuildModuleManifest(result, nameof(PowerShellHostVersion), Modules.PowerShellHostVersion, _PowerShellHostVersion != null && !string.IsNullOrEmpty(_PowerShellHostVersion.ToString()), () => QuoteName(_PowerShellHostVersion), streamWriter);
                    BuildModuleManifest(result, nameof(DotNetFrameworkVersion), StringUtil.Format(Modules.DotNetFrameworkVersion, Modules.PrerequisiteForDesktopEditionOnly), _DotNetFrameworkVersion != null && !string.IsNullOrEmpty(_DotNetFrameworkVersion.ToString()), () => QuoteName(_DotNetFrameworkVersion), streamWriter);
                    BuildModuleManifest(result, nameof(ClrVersion), StringUtil.Format(Modules.CLRVersion, Modules.PrerequisiteForDesktopEditionOnly), _ClrVersion != null && !string.IsNullOrEmpty(_ClrVersion.ToString()), () => QuoteName(_ClrVersion), streamWriter);
                    BuildModuleManifest(result, nameof(ProcessorArchitecture), Modules.ProcessorArchitecture, _processorArchitecture.HasValue, () => QuoteName(_processorArchitecture.ToString()), streamWriter);
                    BuildModuleManifest(result, nameof(RequiredModules), Modules.RequiredModules, _requiredModules != null && _requiredModules.Length > 0, () => QuoteModules(_requiredModules, streamWriter), streamWriter);
                    BuildModuleManifest(result, nameof(RequiredAssemblies), Modules.RequiredAssemblies, _requiredAssemblies != null, () => QuoteFiles(_requiredAssemblies, streamWriter), streamWriter);
                    BuildModuleManifest(result, nameof(ScriptsToProcess), Modules.ScriptsToProcess, _scripts != null, () => QuoteFiles(_scripts, streamWriter), streamWriter);
                    BuildModuleManifest(result, nameof(TypesToProcess), Modules.TypesToProcess, _types != null, () => QuoteFiles(_types, streamWriter), streamWriter);
                    BuildModuleManifest(result, nameof(FormatsToProcess), Modules.FormatsToProcess, _formats != null, () => QuoteFiles(_formats, streamWriter), streamWriter);
                    BuildModuleManifest(result, nameof(NestedModules), Modules.NestedModules, _nestedModules != null, () => QuoteModules(PreProcessModuleSpec(_nestedModules), streamWriter), streamWriter);
                    BuildModuleManifest(result, nameof(FunctionsToExport), Modules.FunctionsToExport, true, () => QuoteNames(_exportedFunctions, streamWriter), streamWriter);
                    BuildModuleManifest(result, nameof(CmdletsToExport), Modules.CmdletsToExport, true, () => QuoteNames(_exportedCmdlets, streamWriter), streamWriter);
                    BuildModuleManifest(result, nameof(VariablesToExport), Modules.VariablesToExport, _exportedVariables != null && _exportedVariables.Length > 0, () => QuoteNames(_exportedVariables, streamWriter), streamWriter);
                    BuildModuleManifest(result, nameof(AliasesToExport), Modules.AliasesToExport, true, () => QuoteNames(_exportedAliases, streamWriter), streamWriter);
                    BuildModuleManifest(result, nameof(DscResourcesToExport), Modules.DscResourcesToExport, _dscResourcesToExport != null && _dscResourcesToExport.Length > 0, () => QuoteNames(_dscResourcesToExport, streamWriter), streamWriter);
                    BuildModuleManifest(result, nameof(ModuleList), Modules.ModuleList, _moduleList != null, () => QuoteModules(_moduleList, streamWriter), streamWriter);
                    BuildModuleManifest(result, nameof(FileList), Modules.FileList, _miscFiles != null, () => QuoteFiles(_miscFiles, streamWriter), streamWriter);
                    BuildPrivateDataInModuleManifest(result, streamWriter);
                    BuildModuleManifest(result, nameof(Modules.HelpInfoURI), Modules.HelpInfoURI, !string.IsNullOrEmpty(_helpInfoUri), () => QuoteName((_helpInfoUri != null) ? new Uri(_helpInfoUri) : null), streamWriter);
                    BuildModuleManifest(result, nameof(DefaultCommandPrefix), Modules.DefaultCommandPrefix, !string.IsNullOrEmpty(_defaultCommandPrefix), () => QuoteName(_defaultCommandPrefix), streamWriter);
                    string strResult = result.ToString();
                        WriteObject(strResult);
                    streamWriter.Write(strResult);
        private void BuildModuleManifest(StringBuilder result, string key, string keyDescription, bool hasValue, Func<string> action, StreamWriter streamWriter)
            if (hasValue)
                result.Append(ManifestFragment(key, keyDescription, action(), streamWriter));
                result.Append(ManifestFragmentForNonSpecifiedManifestMember(key, keyDescription, action(), streamWriter));
        // PrivateData format in manifest file when PrivateData value is a HashTable or not specified.
        // <#
        // # Private data to pass to the module specified in RootModule/ModuleToProcess
        // PrivateData = @{
        // PSData = @{
        // # Tags of this module
        // Tags = @()
        // # LicenseUri of this module
        // LicenseUri = ''
        // # ProjectUri of this module
        // ProjectUri = ''
        // # IconUri of this module
        // IconUri = ''
        // # ReleaseNotes of this module
        // ReleaseNotes = ''
        // }# end of PSData hashtable
        // # User's private data keys
        // }# end of PrivateData hashtable
        // #>
        private void BuildPrivateDataInModuleManifest(StringBuilder result, StreamWriter streamWriter)
            var privateDataHashTable = PrivateData as Hashtable;
            bool specifiedPSDataProperties = !(Tags == null && ReleaseNotes == null && ProjectUri == null && IconUri == null && LicenseUri == null);
            if (_privateData != null && privateDataHashTable == null)
                if (specifiedPSDataProperties)
                    var ioe = new InvalidOperationException(Modules.PrivateDataValueTypeShouldBeHashTableError);
                    var er = new ErrorRecord(ioe, "PrivateDataValueTypeShouldBeHashTable", ErrorCategory.InvalidArgument, _privateData);
                    WriteWarning(Modules.PrivateDataValueTypeShouldBeHashTableWarning);
                    BuildModuleManifest(result, nameof(PrivateData), Modules.PrivateData, _privateData != null,
                        () => QuoteName((string)LanguagePrimitives.ConvertTo(_privateData, typeof(string), CultureInfo.InvariantCulture)),
                        streamWriter);
                result.Append(ManifestComment(Modules.PrivateData, streamWriter));
                result.Append("PrivateData = @{");
                result.Append("    PSData = @{");
                _indent = "        ";
                BuildModuleManifest(result, nameof(Tags), Modules.Tags, Tags != null && Tags.Length > 0, () => QuoteNames(Tags, streamWriter), streamWriter);
                BuildModuleManifest(result, nameof(LicenseUri), Modules.LicenseUri, LicenseUri != null, () => QuoteName(LicenseUri), streamWriter);
                BuildModuleManifest(result, nameof(ProjectUri), Modules.ProjectUri, ProjectUri != null, () => QuoteName(ProjectUri), streamWriter);
                BuildModuleManifest(result, nameof(IconUri), Modules.IconUri, IconUri != null, () => QuoteName(IconUri), streamWriter);
                BuildModuleManifest(result, nameof(ReleaseNotes), Modules.ReleaseNotes, !string.IsNullOrEmpty(ReleaseNotes), () => QuoteName(ReleaseNotes), streamWriter);
                BuildModuleManifest(result, nameof(Prerelease), Modules.Prerelease, !string.IsNullOrEmpty(Prerelease), () => QuoteName(Prerelease), streamWriter);
                BuildModuleManifest(result, nameof(RequireLicenseAcceptance), Modules.RequireLicenseAcceptance, RequireLicenseAcceptance.IsPresent, () => RequireLicenseAcceptance.IsPresent ? "$true" : "$false", streamWriter);
                BuildModuleManifest(result, nameof(ExternalModuleDependencies), Modules.ExternalModuleDependencies, ExternalModuleDependencies != null && ExternalModuleDependencies.Length > 0, () => QuoteNames(ExternalModuleDependencies, streamWriter), streamWriter);
                result.Append("    } ");
                result.Append(ManifestComment(StringUtil.Format(Modules.EndOfManifestHashTable, "PSData"), streamWriter));
                _indent = "    ";
                if (privateDataHashTable != null)
                    foreach (DictionaryEntry entry in privateDataHashTable)
                        result.Append(ManifestFragment(entry.Key.ToString(), entry.Key.ToString(), QuoteName((string)LanguagePrimitives.ConvertTo(entry.Value, typeof(string), CultureInfo.InvariantCulture)), streamWriter));
                result.Append("} ");
                result.Append(ManifestComment(StringUtil.Format(Modules.EndOfManifestHashTable, "PrivateData"), streamWriter));
                _indent = string.Empty;
        private void ValidateUriParameterValue(Uri uri, string parameterName)
            Dbg.Assert(!string.IsNullOrWhiteSpace(parameterName), "parameterName should not be null or whitespace");
            if (uri != null && !Uri.IsWellFormedUriString(uri.AbsoluteUri, UriKind.Absolute))
                var message = StringUtil.Format(Modules.InvalidParameterValue, uri);
                var er = new ErrorRecord(ioe, "Modules_InvalidUri",
                    ErrorCategory.InvalidArgument, parameterName);

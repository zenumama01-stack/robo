    /// New-PSSessionConfigurationFile command implementation
    /// See Declarative Initial Session State (DISC)
    [Cmdlet(VerbsCommon.New, "PSSessionConfigurationFile", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096791")]
    public class NewPSSessionConfigurationFileCommand : PSCmdlet
        /// Destination path.
        /// Configuration file schema version.
        public Version SchemaVersion
                return _schemaVersion;
                _schemaVersion = value;
        private Version _schemaVersion = new Version("2.0.0.0");
        /// Configuration file GUID.
                return _guid;
                _guid = value;
        /// Author of the configuration file.
                return _author;
                _author = value;
        /// Description.
        /// Company name.
                return _companyName;
                _companyName = value;
        private string _companyName;
        /// Copyright information.
                return _copyright;
                _copyright = value;
        /// Specifies type of initial session state to use.
        public SessionType SessionType
                return _sessionType;
                _sessionType = value;
        private SessionType _sessionType = SessionType.Default;
        /// Specifies the directory for transcripts to be placed.
        public string TranscriptDirectory
                return _transcriptDirectory;
                _transcriptDirectory = value;
        private string _transcriptDirectory = null;
        /// Specifies whether to run this configuration under a virtual account.
        public SwitchParameter RunAsVirtualAccount { get; set; }
        /// Specifies groups a virtual account is part of.
        public string[] RunAsVirtualAccountGroups { get; set; }
        /// Creates a User PSDrive in the session.
        /// The User drive is used with Copy-Item for file transfer when the FileSystem provider is
        /// not visible in the session.
        public SwitchParameter MountUserDrive
        /// Optional parameter that specifies a maximum size in bytes for the User: drive created with the
        /// MountUserDrive parameter.
        /// If no maximum size is specified then the default drive maximum size is 50MB.
        public long UserDriveMaximumSize { get; set; }
        // Temporarily removed until script input parameter validation is implemented.
        /// Optional parameter that enforces script input parameter validation.  When specified all scripts
        /// run in the PSSession must have validation attributes to validate input data or an error is generated.
        /// If a MountUserDrive is specified for the PSSession then input parameter validation will be
        /// enabled automatically.
        public SwitchParameter EnforceInputParameterValidation { get; set; }
        /// Optional parameter that specifies a Group Managed Service Account name in which the configuration
        /// is run.
        public string GroupManagedServiceAccount { get; set; }
        /// Scripts to process.
                return _scriptsToProcess;
                _scriptsToProcess = value;
        private string[] _scriptsToProcess = Array.Empty<string>();
        /// Role definitions for this session configuration (Role name -> Role capability)
        public IDictionary RoleDefinitions
                return _roleDefinitions;
                _roleDefinitions = value;
        private IDictionary _roleDefinitions;
        /// Specifies account groups that are membership requirements for this session.
        public IDictionary RequiredGroups
            get { return _requiredGroups; }
            set { _requiredGroups = value; }
        private IDictionary _requiredGroups;
        /// Language mode.
                _isLanguageModeSpecified = true;
        private PSLanguageMode _languageMode = PSLanguageMode.NoLanguage;
        private bool _isLanguageModeSpecified;
        /// Execution policy.
        private ExecutionPolicy _executionPolicy = ExecutionPolicy.Restricted;
        /// PowerShell version.
                return _powerShellVersion;
                _powerShellVersion = value;
        private Version _powerShellVersion;
        /// A list of modules to import.
                return _modulesToImport;
                _modulesToImport = value;
        private object[] _modulesToImport;
        /// A list of visible aliases.
        public string[] VisibleAliases
                return _visibleAliases;
                _visibleAliases = value;
        private string[] _visibleAliases = Array.Empty<string>();
        /// A list of visible cmdlets.
        public object[] VisibleCmdlets
                return _visibleCmdlets;
                _visibleCmdlets = value;
        private object[] _visibleCmdlets = null;
        /// A list of visible functions.
        public object[] VisibleFunctions
                return _visibleFunctions;
                _visibleFunctions = value;
        private object[] _visibleFunctions = null;
        /// A list of visible external commands (scripts and applications)
        public string[] VisibleExternalCommands
                return _visibleExternalCommands;
                _visibleExternalCommands = value;
        private string[] _visibleExternalCommands = Array.Empty<string>();
        /// A list of providers.
        public string[] VisibleProviders
                return _visibleProviders;
                _visibleProviders = value;
        private string[] _visibleProviders = Array.Empty<string>();
        /// A list of aliases.
        public IDictionary[] AliasDefinitions
                return _aliasDefinitions;
                _aliasDefinitions = value;
        private IDictionary[] _aliasDefinitions;
        /// A list of functions.
        public IDictionary[] FunctionDefinitions
                return _functionDefinitions;
                _functionDefinitions = value;
        private IDictionary[] _functionDefinitions;
        /// A list of variables.
        public object VariableDefinitions
                return _variableDefinitions;
                _variableDefinitions = value;
        private object _variableDefinitions;
        /// A list of environment variables.
        public IDictionary EnvironmentVariables
                _environmentVariables = value;
        private IDictionary _environmentVariables;
        /// A list of types to process.
                return _typesToProcess;
                _typesToProcess = value;
        private string[] _typesToProcess = Array.Empty<string>();
        /// A list of format data to process.
                return _formatsToProcess;
                _formatsToProcess = value;
        private string[] _formatsToProcess = Array.Empty<string>();
        /// A list of assemblies to load.
        public string[] AssembliesToLoad
                return _assembliesToLoad;
                _assembliesToLoad = value;
        private string[] _assembliesToLoad;
        /// Gets or sets whether to include a full expansion of all possible session configuration
        /// keys as comments when creating the session configuration file.
            Debug.Assert(!string.IsNullOrEmpty(_path));
                string message = StringUtil.Format(RemotingErrorIdStrings.InvalidPSSessionConfigurationFilePath, _path);
                /* defaultEncoding */ false,
                /* Append */ false,
                /* Force */ false,
                /* NoClobber */ false,
                out streamWriter,
                // Schema version
                result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.SchemaVersion, RemotingErrorIdStrings.DISCSchemaVersionComment,
                    SessionConfigurationUtils.QuoteName(_schemaVersion), streamWriter, false));
                // Guid
                result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.Guid, RemotingErrorIdStrings.DISCGUIDComment, SessionConfigurationUtils.QuoteName(_guid), streamWriter, false));
                // Author
                result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.Author, RemotingErrorIdStrings.DISCAuthorComment,
                    SessionConfigurationUtils.QuoteName(_author), streamWriter, false));
                // Description
                result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.Description, RemotingErrorIdStrings.DISCDescriptionComment,
                    SessionConfigurationUtils.QuoteName(_description), streamWriter, string.IsNullOrEmpty(_description)));
                // Company name
                if (ShouldGenerateConfigurationSnippet("CompanyName"))
                    result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.CompanyName, RemotingErrorIdStrings.DISCCompanyNameComment,
                        SessionConfigurationUtils.QuoteName(_companyName), streamWriter, false));
                // Copyright
                if (ShouldGenerateConfigurationSnippet("Copyright"))
                    result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.Copyright, RemotingErrorIdStrings.DISCCopyrightComment,
                        SessionConfigurationUtils.QuoteName(_copyright), streamWriter, false));
                // Session type
                result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.SessionType, RemotingErrorIdStrings.DISCInitialSessionStateComment,
                    SessionConfigurationUtils.QuoteName(_sessionType), streamWriter, false));
                string resultData = null;
                // Transcript directory
                resultData = string.IsNullOrEmpty(_transcriptDirectory) ? "'C:\\Transcripts\\'" : SessionConfigurationUtils.QuoteName(_transcriptDirectory);
                result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.TranscriptDirectory, RemotingErrorIdStrings.DISCTranscriptDirectoryComment,
                    resultData, streamWriter, string.IsNullOrEmpty(_transcriptDirectory)));
                // Run as virtual account
                result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.RunAsVirtualAccount, RemotingErrorIdStrings.DISCRunAsVirtualAccountComment,
                    SessionConfigurationUtils.WriteBoolean(true), streamWriter, RunAsVirtualAccount == false));
                // Run as virtual account groups
                if (ShouldGenerateConfigurationSnippet("RunAsVirtualAccountGroups"))
                    bool haveVirtualAccountGroups = (RunAsVirtualAccountGroups != null) && (RunAsVirtualAccountGroups.Length > 0);
                    resultData = (haveVirtualAccountGroups) ? SessionConfigurationUtils.CombineStringArray(RunAsVirtualAccountGroups) : "'Remote Desktop Users', 'Remote Management Users'";
                    result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.RunAsVirtualAccountGroups, RemotingErrorIdStrings.DISCRunAsVirtualAccountGroupsComment,
                        resultData, streamWriter, !haveVirtualAccountGroups));
                // Mount user drive
                if (ShouldGenerateConfigurationSnippet("MountUserDrive"))
                    result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.MountUserDrive, RemotingErrorIdStrings.DISCMountUserDriveComment,
                        SessionConfigurationUtils.WriteBoolean(true), streamWriter, MountUserDrive == false));
                // User drive maximum size
                if (ShouldGenerateConfigurationSnippet("UserDriveMaximumSize"))
                    long userDriveMaxSize = (UserDriveMaximumSize > 0) ? UserDriveMaximumSize : 50000000;
                    result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.UserDriveMaxSize, RemotingErrorIdStrings.DISCUserDriveMaxSizeComment,
                        SessionConfigurationUtils.WriteLong(userDriveMaxSize), streamWriter, (UserDriveMaximumSize <= 0)));
                // Enforce input parameter validation
                result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.EnforceInputParameterValidation, RemotingErrorIdStrings.DISCEnforceInputParameterValidation,
                    SessionConfigurationUtils.WriteBoolean(true), streamWriter, EnforceInputParameterValidation == false));
                // Group Managed Service Account Name
                if (ShouldGenerateConfigurationSnippet("GroupManagedServiceAccount"))
                    bool haveGMSAAccountName = !string.IsNullOrEmpty(GroupManagedServiceAccount);
                    resultData = (!haveGMSAAccountName) ? "'CONTOSO\\GroupManagedServiceAccount'" : SessionConfigurationUtils.QuoteName(GroupManagedServiceAccount);
                    result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.GMSAAccount, RemotingErrorIdStrings.DISCGMSAComment,
                        resultData, streamWriter, !haveGMSAAccountName));
                // Scripts to process
                resultData = (_scriptsToProcess.Length > 0) ? SessionConfigurationUtils.CombineStringArray(_scriptsToProcess) : "'C:\\ConfigData\\InitScript1.ps1', 'C:\\ConfigData\\InitScript2.ps1'";
                result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.ScriptsToProcess, RemotingErrorIdStrings.DISCScriptsToProcessComment,
                    resultData, streamWriter, (_scriptsToProcess.Length == 0)));
                // Role definitions
                if (_roleDefinitions == null)
                    result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.RoleDefinitions, RemotingErrorIdStrings.DISCRoleDefinitionsComment,
                        "@{ 'CONTOSO\\SqlAdmins' = @{ RoleCapabilities = 'SqlAdministration' }; 'CONTOSO\\SqlManaged' = @{ RoleCapabilityFiles = 'C:\\RoleCapability\\SqlManaged.psrc' }; 'CONTOSO\\ServerMonitors' = @{ VisibleCmdlets = 'Get-Process' } } ", streamWriter, true));
                    DISCUtils.ValidateRoleDefinitions(_roleDefinitions);
                        SessionConfigurationUtils.CombineHashtable(_roleDefinitions, streamWriter), streamWriter, false));
                // Required groups
                if (ShouldGenerateConfigurationSnippet("RequiredGroups"))
                    if (_requiredGroups == null)
                        result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.RequiredGroups, RemotingErrorIdStrings.DISCRequiredGroupsComment,
                            "@{ And = @{ Or = 'CONTOSO\\SmartCard-Logon1', 'CONTOSO\\SmartCard-Logon2' }, 'Administrators' }", streamWriter, true));
                            SessionConfigurationUtils.CombineRequiredGroupsHash(_requiredGroups), streamWriter, false));
                // PSLanguageMode languageMode
                if (ShouldGenerateConfigurationSnippet("LanguageMode"))
                    if (!_isLanguageModeSpecified)
                        if (_sessionType == SessionType.Default)
                            _languageMode = PSLanguageMode.FullLanguage;
                    result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.LanguageMode, RemotingErrorIdStrings.DISCLanguageModeComment,
                        SessionConfigurationUtils.QuoteName(_languageMode), streamWriter, false));
                // ExecutionPolicy executionPolicy
                if (ShouldGenerateConfigurationSnippet("ExecutionPolicy"))
                    result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.ExecutionPolicy, RemotingErrorIdStrings.DISCExecutionPolicyComment,
                        SessionConfigurationUtils.QuoteName(_executionPolicy), streamWriter, false));
                // PowerShell version
                bool isExample = false;
                if (ShouldGenerateConfigurationSnippet("PowerShellVersion"))
                    if (_powerShellVersion == null)
                        isExample = true;
                        _powerShellVersion = PSVersionInfo.PSVersion;
                    result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.PowerShellVersion, RemotingErrorIdStrings.DISCPowerShellVersionComment,
                        SessionConfigurationUtils.QuoteName(_powerShellVersion), streamWriter, isExample));
                // Modules to import
                if (_modulesToImport == null)
                    if (Full)
                        const string exampleModulesToImport = "'MyCustomModule', @{ ModuleName = 'MyCustomModule'; ModuleVersion = '1.0.0.0'; GUID = '4d30d5f0-cb16-4898-812d-f20a6c596bdf' }";
                        result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.ModulesToImport, RemotingErrorIdStrings.DISCModulesToImportComment, exampleModulesToImport, streamWriter, true));
                    result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.ModulesToImport, RemotingErrorIdStrings.DISCModulesToImportComment,
                        SessionConfigurationUtils.CombineHashTableOrStringArray(_modulesToImport, streamWriter, this), streamWriter, false));
                // Visible aliases
                if (ShouldGenerateConfigurationSnippet("VisibleAliases"))
                    result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.VisibleAliases, RemotingErrorIdStrings.DISCVisibleAliasesComment,
                        SessionConfigurationUtils.GetVisibilityDefault(_visibleAliases, streamWriter, this), streamWriter, _visibleAliases.Length == 0));
                // Visible cmdlets
                if ((_visibleCmdlets == null) || (_visibleCmdlets.Length == 0))
                        result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.VisibleCmdlets, RemotingErrorIdStrings.DISCVisibleCmdletsComment,
                            "'Invoke-Cmdlet1', @{ Name = 'Invoke-Cmdlet2'; Parameters = @{ Name = 'Parameter1'; ValidateSet = 'Item1', 'Item2' }, @{ Name = 'Parameter2'; ValidatePattern = 'L*' } }", streamWriter, true));
                        SessionConfigurationUtils.GetVisibilityDefault(_visibleCmdlets, streamWriter, this), streamWriter, false));
                // Visible functions
                if ((_visibleFunctions == null) || (_visibleFunctions.Length == 0))
                        result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.VisibleFunctions, RemotingErrorIdStrings.DISCVisibleFunctionsComment,
                            "'Invoke-Function1', @{ Name = 'Invoke-Function2'; Parameters = @{ Name = 'Parameter1'; ValidateSet = 'Item1', 'Item2' }, @{ Name = 'Parameter2'; ValidatePattern = 'L*' } }", streamWriter, true));
                        SessionConfigurationUtils.GetVisibilityDefault(_visibleFunctions, streamWriter, this), streamWriter, _visibleFunctions.Length == 0));
                // Visible external commands (scripts, executables)
                if (ShouldGenerateConfigurationSnippet("VisibleExternalCommands"))
                    result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.VisibleExternalCommands, RemotingErrorIdStrings.DISCVisibleExternalCommandsComment,
                        SessionConfigurationUtils.GetVisibilityDefault(_visibleExternalCommands, streamWriter, this), streamWriter, _visibleExternalCommands.Length == 0));
                // Visible providers
                if (ShouldGenerateConfigurationSnippet("VisibleProviders"))
                    result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.VisibleProviders, RemotingErrorIdStrings.DISCVisibleProvidersComment,
                        SessionConfigurationUtils.GetVisibilityDefault(_visibleProviders, streamWriter, this), streamWriter, _visibleProviders.Length == 0));
                // Alias definitions
                if ((_aliasDefinitions == null) || (_aliasDefinitions.Length == 0))
                        result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.AliasDefinitions, RemotingErrorIdStrings.DISCAliasDefinitionsComment,
                           "@{ Name = 'Alias1'; Value = 'Invoke-Alias1'}, @{ Name = 'Alias2'; Value = 'Invoke-Alias2'}", streamWriter, true));
                        SessionConfigurationUtils.CombineHashtableArray(_aliasDefinitions, streamWriter), streamWriter, false));
                // Function definitions
                if (_functionDefinitions == null)
                        result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.FunctionDefinitions, RemotingErrorIdStrings.DISCFunctionDefinitionsComment,
                            "@{ Name = 'MyFunction'; ScriptBlock = { param($MyInput) $MyInput } }", streamWriter, true));
                    Hashtable[] funcHash = DISCPowerShellConfiguration.TryGetHashtableArray(_functionDefinitions);
                    if (funcHash != null)
                            SessionConfigurationUtils.CombineHashtableArray(funcHash, streamWriter), streamWriter, false));
                        foreach (Hashtable hashtable in funcHash)
                            if (!hashtable.ContainsKey(ConfigFileConstants.FunctionNameToken))
                                PSArgumentException e = new PSArgumentException(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustContainKey,
                                    ConfigFileConstants.FunctionDefinitions, ConfigFileConstants.FunctionNameToken, _path));
                            if (!hashtable.ContainsKey(ConfigFileConstants.FunctionValueToken))
                                    ConfigFileConstants.FunctionDefinitions, ConfigFileConstants.FunctionValueToken, _path));
                            if (hashtable[ConfigFileConstants.FunctionValueToken] is not ScriptBlock)
                                PSArgumentException e = new PSArgumentException(StringUtil.Format(RemotingErrorIdStrings.DISCKeyMustBeScriptBlock,
                                    ConfigFileConstants.FunctionValueToken, ConfigFileConstants.FunctionDefinitions, _path));
                            foreach (string functionKey in hashtable.Keys)
                                if (!string.Equals(functionKey, ConfigFileConstants.FunctionNameToken, StringComparison.OrdinalIgnoreCase) &&
                                    !string.Equals(functionKey, ConfigFileConstants.FunctionValueToken, StringComparison.OrdinalIgnoreCase) &&
                                    !string.Equals(functionKey, ConfigFileConstants.FunctionOptionsToken, StringComparison.OrdinalIgnoreCase))
                                    PSArgumentException e = new PSArgumentException(StringUtil.Format(RemotingErrorIdStrings.DISCTypeContainsInvalidKey,
                                        functionKey, ConfigFileConstants.FunctionDefinitions, _path));
                        PSArgumentException e = new PSArgumentException(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeHashtableArray,
                            ConfigFileConstants.FunctionDefinitions, filePath));
                // Variable definitions
                if (_variableDefinitions == null)
                        result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.VariableDefinitions, RemotingErrorIdStrings.DISCVariableDefinitionsComment,
                            "@{ Name = 'Variable1'; Value = { 'Dynamic' + 'InitialValue' } }, @{ Name = 'Variable2'; Value = 'StaticInitialValue' }", streamWriter, true));
                    string varString = _variableDefinitions as string;
                    if (varString != null)
                            varString, streamWriter, false));
                        Hashtable[] varHash = DISCPowerShellConfiguration.TryGetHashtableArray(_variableDefinitions);
                        if (varHash != null)
                                SessionConfigurationUtils.CombineHashtableArray(varHash, streamWriter), streamWriter, false));
                            foreach (Hashtable hashtable in varHash)
                                if (!hashtable.ContainsKey(ConfigFileConstants.VariableNameToken))
                                        ConfigFileConstants.VariableDefinitions, ConfigFileConstants.VariableNameToken, _path));
                                if (!hashtable.ContainsKey(ConfigFileConstants.VariableValueToken))
                                        ConfigFileConstants.VariableDefinitions, ConfigFileConstants.VariableValueToken, _path));
                                foreach (string variableKey in hashtable.Keys)
                                    if (!string.Equals(variableKey, ConfigFileConstants.VariableNameToken, StringComparison.OrdinalIgnoreCase) &&
                                        !string.Equals(variableKey, ConfigFileConstants.VariableValueToken, StringComparison.OrdinalIgnoreCase))
                                            variableKey, ConfigFileConstants.VariableDefinitions, _path));
                                ConfigFileConstants.VariableDefinitions, filePath));
                // Environment variables
                        result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.EnvironmentVariables, RemotingErrorIdStrings.DISCEnvironmentVariablesComment,
                            "@{ Variable1 = 'Value1'; Variable2 = 'Value2' }",
                            streamWriter, true));
                        SessionConfigurationUtils.CombineHashtable(_environmentVariables, streamWriter), streamWriter, false));
                // Types to process
                if (ShouldGenerateConfigurationSnippet("TypesToProcess"))
                    resultData = (_typesToProcess.Length > 0) ? SessionConfigurationUtils.CombineStringArray(_typesToProcess) : "'C:\\ConfigData\\MyTypes.ps1xml', 'C:\\ConfigData\\OtherTypes.ps1xml'";
                    result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.TypesToProcess, RemotingErrorIdStrings.DISCTypesToProcessComment,
                        resultData, streamWriter, (_typesToProcess.Length == 0)));
                // Formats to process
                if (ShouldGenerateConfigurationSnippet("FormatsToProcess"))
                    resultData = (_formatsToProcess.Length > 0) ? SessionConfigurationUtils.CombineStringArray(_formatsToProcess) : "'C:\\ConfigData\\MyFormats.ps1xml', 'C:\\ConfigData\\OtherFormats.ps1xml'";
                    result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.FormatsToProcess, RemotingErrorIdStrings.DISCFormatsToProcessComment,
                        resultData, streamWriter, (_formatsToProcess.Length == 0)));
                // Assemblies to load
                if (ShouldGenerateConfigurationSnippet("AssembliesToLoad"))
                    isExample = false;
                    if ((_assembliesToLoad == null) || (_assembliesToLoad.Length == 0))
                        _assembliesToLoad = new string[] { "System.Web", "System.OtherAssembly, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" };
                    result.Append(SessionConfigurationUtils.ConfigFragment(ConfigFileConstants.AssembliesToLoad, RemotingErrorIdStrings.DISCAssembliesToLoadComment,
                        SessionConfigurationUtils.CombineStringArray(_assembliesToLoad), streamWriter, isExample));
                streamWriter.Write(result.ToString());
        private bool ShouldGenerateConfigurationSnippet(string parameterName)
            return Full || MyInvocation.BoundParameters.ContainsKey(parameterName);
    /// New-PSRoleCapabilityFile command implementation
    /// Creates a role capability file suitable for use in a Role Capability (which can be referenced in a Session Configuration file)
    [Cmdlet(VerbsCommon.New, "PSRoleCapabilityFile", HelpUri = "https://go.microsoft.com/fwlink/?LinkId=623708")]
    public class NewPSRoleCapabilityFileCommand : PSCmdlet
            if (!provider.NameEquals(Context.ProviderNames.FileSystem) || !filePath.EndsWith(StringLiterals.PowerShellRoleCapabilityFileExtension, StringComparison.OrdinalIgnoreCase))
                string message = StringUtil.Format(RemotingErrorIdStrings.InvalidRoleCapabilityFilePath, _path);
                ErrorRecord er = new ErrorRecord(ioe, "InvalidRoleCapabilityFilePath",
                string resultData = (_scriptsToProcess.Length > 0) ? SessionConfigurationUtils.CombineStringArray(_scriptsToProcess) : "'C:\\ConfigData\\InitScript1.ps1', 'C:\\ConfigData\\InitScript2.ps1'";
    #region SessionConfigurationUtils
    /// Utility methods for configuration file commands.
    internal static class SessionConfigurationUtils
        /// This routine builds a fragment of the config file
        internal static string ConfigFragment(string key, string resourceString, string value, StreamWriter streamWriter, bool isExample)
            string nl = streamWriter.NewLine;
            if (isExample)
                return string.Format(CultureInfo.InvariantCulture, "# {0}{1}# {2:19} = {3}{4}{5}", resourceString, nl, key, value, nl, nl);
            return string.Format(CultureInfo.InvariantCulture, "# {0}{1}{2:19} = {3}{4}{5}", resourceString, nl, key, value, nl, nl);
        internal static string QuoteName(object name)
            return "'" + System.Management.Automation.Language.CodeGeneration.EscapeSingleQuotedStringContent(name.ToString()) + "'";
        /// Return a script block string wrapped in curly braces.
        /// <param name="sb">The string to wrap.</param>
        /// <returns>The wrapped string.</returns>
        internal static string WrapScriptBlock(object sb)
                return "{}";
            return "{" + sb.ToString() + "}";
        internal static string WriteBoolean(bool booleanToEmit)
            if (booleanToEmit)
                return "$true";
                return "$false";
        internal static string WriteLong(long longToEmit)
            return longToEmit.ToString(CultureInfo.InvariantCulture);
        /// Gets the visibility default value.
        internal static string GetVisibilityDefault(object[] values, StreamWriter writer, PSCmdlet caller)
            if ((values != null) && (values.Length > 0))
                return CombineHashTableOrStringArray(values, writer, caller);
            // Default Visibility is Empty which gets commented
            // out in the session config file
            return "'Item1', 'Item2'";
        /// Combines a hashtable into a single string block.
        internal static string CombineHashtable(IDictionary table, StreamWriter writer, int? indent = 0)
            sb.Append("@{");
            var keys = table.Keys.Cast<string>().Order();
                sb.Append(writer.NewLine);
                sb.AppendFormat("{0," + (4 * (indent + 1)) + "}", string.Empty);
                sb.Append(QuoteName(key));
                sb.Append(" = ");
                if (table[key] is ScriptBlock)
                    sb.Append(WrapScriptBlock(table[key].ToString()));
                IDictionary tableValue = table[key] as IDictionary;
                if (tableValue != null)
                    sb.Append(CombineHashtable(tableValue, writer, indent + 1));
                IDictionary[] tableValues = DISCPowerShellConfiguration.TryGetHashtableArray(table[key]);
                if (tableValues != null)
                    sb.Append(CombineHashtableArray(tableValues, writer, indent + 1));
                string[] stringValues = DISCPowerShellConfiguration.TryGetStringArray(table[key]);
                if (stringValues != null)
                    sb.Append(CombineStringArray(stringValues));
                sb.Append(QuoteName(table[key]));
            sb.Append(" }");
        /// Combines RequireGroups logic operator hash tables / lists
        /// e.g.,
        /// -RequiredGroups @{ Or = 'TrustedGroup1', 'MFAGroup2' }
        /// -RequiredGroups @{ And = 'Administrators', @{ Or = 'MFAGroup1', 'MFAGroup2' } }
        /// -RequiredGroups @{ Or = @{ And = 'Administrators', 'TrustedGroup1' }, @{ And = 'Power Users', 'TrustedGroup1' } }
        internal static string CombineRequiredGroupsHash(IDictionary table)
            if (table.Count != 1)
                throw new PSInvalidOperationException(RemotingErrorIdStrings.RequiredGroupsHashMultipleKeys);
            var keyEnumerator = table.Keys.GetEnumerator();
            keyEnumerator.MoveNext();
            string key = keyEnumerator.Current as string;
            object keyObject = table[key];
            sb.Append("@{ ");
            object[] values = keyObject as object[];
                for (int i = 0; i < values.Length;)
                    WriteRequiredGroup(values[i++], sb);
                    if (i < values.Length)
                WriteRequiredGroup(keyObject, sb);
        private static void WriteRequiredGroup(object value, StringBuilder sb)
                sb.Append(QuoteName(strValue));
                Hashtable subTable = value as Hashtable;
                if (subTable != null)
                    sb.Append(CombineRequiredGroupsHash(subTable));
        /// Combines an array of hashtables into a single string block.
        internal static string CombineHashtableArray(IDictionary[] tables, StreamWriter writer, int? indent = 0)
            for (int i = 0; i < tables.Length; i++)
                sb.Append(CombineHashtable(tables[i], writer, indent));
                if (i < (tables.Length - 1))
        /// Combines an array of strings into a single string block.
        /// <param name="values">String values.</param>
        /// <returns>String block.</returns>
        internal static string CombineStringArray(string[] values)
                if (!string.IsNullOrEmpty(values[i]))
                    sb.Append(QuoteName(values[i]));
                    if (i < (values.Length - 1))
        /// Combines an array of strings or hashtables into a single string block.
        internal static string CombineHashTableOrStringArray(object[] values, StreamWriter writer, PSCmdlet caller)
                string strVal = values[i] as string;
                if (!string.IsNullOrEmpty(strVal))
                    sb.Append(QuoteName(strVal));
                    Hashtable hashVal = values[i] as Hashtable;
                    if (hashVal == null)
                        string message = StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeStringOrHashtableArray,
                                                           ConfigFileConstants.ModulesToImport);
                        PSArgumentException e = new PSArgumentException(message);
                        caller.ThrowTerminatingError(e.ErrorRecord);
                    sb.Append(CombineHashtable(hashVal, writer));

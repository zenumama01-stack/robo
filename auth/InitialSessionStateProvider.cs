    #region WSMan endpoint configuration
    /// This struct is used to represent contents from configuration xml. The
    /// XML is passed to plugins by WSMan API.
    /// This helper does not validate XML content as it is already validated
    /// by WSMan.
    internal class ConfigurationDataFromXML
        #region Config XML Constants
        internal const string INITPARAMETERSTOKEN = "InitializationParameters";
        internal const string PARAMTOKEN = "Param";
        internal const string NAMETOKEN = "Name";
        internal const string VALUETOKEN = "Value";
        internal const string APPBASETOKEN = "applicationbase";
        internal const string ASSEMBLYTOKEN = "assemblyname";
        internal const string SHELLCONFIGTYPETOKEN = "pssessionconfigurationtypename";
        internal const string STARTUPSCRIPTTOKEN = "startupscript";
        internal const string MAXRCVDOBJSIZETOKEN = "psmaximumreceivedobjectsizemb";
        internal const string MAXRCVDOBJSIZETOKEN_CamelCase = "PSMaximumReceivedObjectSizeMB";
        internal const string MAXRCVDCMDSIZETOKEN = "psmaximumreceiveddatasizepercommandmb";
        internal const string MAXRCVDCMDSIZETOKEN_CamelCase = "PSMaximumReceivedDataSizePerCommandMB";
        internal const string THREADOPTIONSTOKEN = "pssessionthreadoptions";
        internal const string THREADAPTSTATETOKEN = "pssessionthreadapartmentstate";
        internal const string SESSIONCONFIGTOKEN = "sessionconfigurationdata";
        internal const string PSVERSIONTOKEN = "PSVersion";
        internal const string MAXPSVERSIONTOKEN = "MaxPSVersion";
        internal const string MODULESTOIMPORT = "ModulesToImport";
        internal const string HOSTMODE = "hostmode";
        internal const string CONFIGFILEPATH = "configfilepath";
        internal const string CONFIGFILEPATH_CamelCase = "ConfigFilePath";
        internal string StartupScript;
        // this field is used only by an Out-Of-Process (IPC) server process
        internal string InitializationScriptForOutOfProcessRunspace;
        internal string ApplicationBase;
        internal string AssemblyName;
        internal string EndPointConfigurationTypeName;
        internal Type EndPointConfigurationType;
        internal int? MaxReceivedObjectSizeMB;
        internal int? MaxReceivedCommandSizeMB;
        // Used to set properties on the RunspacePool created for this shell.
        internal PSThreadOptions? ShellThreadOptions;
        internal ApartmentState? ShellThreadApartmentState;
        internal PSSessionConfigurationData SessionConfigurationData;
        internal string ConfigFilePath;
        /// Using optionName and optionValue updates the current object.
        /// 1. "optionName" is not valid in "InitializationParameters" section.
        /// 2. "startupscript" must specify a PowerShell script file that ends with extension ".ps1".
        private void Update(string optionName, string optionValue)
            switch (optionName.ToLowerInvariant())
                case APPBASETOKEN:
                    AssertValueNotAssigned(APPBASETOKEN, ApplicationBase);
                    // this is a folder pointing to application base of the plugin shell
                    // allow the folder path to use environment variables.
                    ApplicationBase = Environment.ExpandEnvironmentVariables(optionValue);
                case ASSEMBLYTOKEN:
                    AssertValueNotAssigned(ASSEMBLYTOKEN, AssemblyName);
                    AssemblyName = optionValue;
                case SHELLCONFIGTYPETOKEN:
                    AssertValueNotAssigned(SHELLCONFIGTYPETOKEN, EndPointConfigurationTypeName);
                    EndPointConfigurationTypeName = optionValue;
                case STARTUPSCRIPTTOKEN:
                    AssertValueNotAssigned(STARTUPSCRIPTTOKEN, StartupScript);
                    if (!optionValue.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
                        throw PSTraceSource.NewArgumentException(STARTUPSCRIPTTOKEN,
                            RemotingErrorIdStrings.StartupScriptNotCorrect,
                            STARTUPSCRIPTTOKEN);
                    // allow the script file to exist in any path..and support
                    // environment variable expansion.
                    StartupScript = Environment.ExpandEnvironmentVariables(optionValue);
                case MAXRCVDOBJSIZETOKEN:
                    AssertValueNotAssigned(MAXRCVDOBJSIZETOKEN, MaxReceivedObjectSizeMB);
                    MaxReceivedObjectSizeMB = GetIntValueInBytes(optionValue);
                case MAXRCVDCMDSIZETOKEN:
                    AssertValueNotAssigned(MAXRCVDCMDSIZETOKEN, MaxReceivedCommandSizeMB);
                    MaxReceivedCommandSizeMB = GetIntValueInBytes(optionValue);
                case THREADOPTIONSTOKEN:
                    AssertValueNotAssigned(THREADOPTIONSTOKEN, ShellThreadOptions);
                    ShellThreadOptions = (PSThreadOptions)LanguagePrimitives.ConvertTo(
                        optionValue, typeof(PSThreadOptions), CultureInfo.InvariantCulture);
                case THREADAPTSTATETOKEN:
                    AssertValueNotAssigned(THREADAPTSTATETOKEN, ShellThreadApartmentState);
                    ShellThreadApartmentState = (ApartmentState)LanguagePrimitives.ConvertTo(
                        optionValue, typeof(ApartmentState), CultureInfo.InvariantCulture);
                case SESSIONCONFIGTOKEN:
                        AssertValueNotAssigned(SESSIONCONFIGTOKEN, SessionConfigurationData);
                        SessionConfigurationData = PSSessionConfigurationData.Create(optionValue);
                case CONFIGFILEPATH:
                        AssertValueNotAssigned(CONFIGFILEPATH, ConfigFilePath);
                        ConfigFilePath = optionValue;
                    // we dont need to evaluate PSVersion and other custom authz
                    // related tokens
        /// Checks if the originalValue is empty. If not throws an exception.
        /// <param name="originalValue"></param>
        /// 1. "optionName" is already defined
        private static void AssertValueNotAssigned(string optionName, object originalValue)
                throw PSTraceSource.NewArgumentException(optionName,
                    RemotingErrorIdStrings.DuplicateInitializationParameterFound, optionName, INITPARAMETERSTOKEN);
        /// Converts the value specified by <paramref name="optionValue"/> to int.
        /// Multiplies the value by 1MB (1024*1024) to get the number in bytes.
        /// <param name="optionValueInMB"></param>
        /// If value is specified, specified value as int . otherwise null.
        private static int? GetIntValueInBytes(string optionValueInMB)
            int? result = null;
                double variableValue = (double)LanguagePrimitives.ConvertTo(optionValueInMB,
                    typeof(double), System.Globalization.CultureInfo.InvariantCulture);
                result = unchecked((int)(variableValue * 1024 * 1024)); // Multiply by 1MB
        /// Creates the struct from initialization parameters xml.
        /// <param name="initializationParameters">
        /// Initialization Parameters xml passed by WSMan API. This data is read from the config
        /// xml and is in the following format:
                    <Param Name="PSVersion" Value="2.0" />
                    <Param Name="ApplicationBase" Value="<folder path>" />
                    ...
        /* The following extensions have been added in V3 providing the user
         * the ability to pass data to the session configuration for initialization
                <Param Name="SessionConfigurationData" Value="<SessionConfigurationData with XML escaping>" />
         * The session configuration data blob can be defined as under
                    <SessionConfigurationData>
                        <Param Name="ModulesToImport" Value="<folder path>" />
                        <Param Name="PrivateData" />
                            <PrivateData>
                            </PrivateData>
                        </Param>
                    </SessionConfigurationData>
        internal static ConfigurationDataFromXML Create(string initializationParameters)
            ConfigurationDataFromXML result = new ConfigurationDataFromXML();
            if (string.IsNullOrEmpty(initializationParameters))
            readerSettings.MaxCharactersInDocument = 10000;
            using (XmlReader reader = XmlReader.Create(new StringReader(initializationParameters), readerSettings))
                // read the header <InitializationParameters>
                if (reader.ReadToFollowing(INITPARAMETERSTOKEN))
                    bool isParamFound = reader.ReadToDescendant(PARAMTOKEN);
                    while (isParamFound)
                        if (!reader.MoveToAttribute(NAMETOKEN))
                            throw PSTraceSource.NewArgumentException(initializationParameters,
                                RemotingErrorIdStrings.NoAttributesFoundForParamElement,
                                NAMETOKEN, VALUETOKEN, PARAMTOKEN);
                        string optionName = reader.Value;
                        if (!reader.MoveToAttribute(VALUETOKEN))
                        string optionValue = reader.Value;
                        result.Update(optionName, optionValue);
                        // move to next Param token.
                        isParamFound = reader.ReadToFollowing(PARAMTOKEN);
            // assign defaults after parsing the xml content.
            result.MaxReceivedObjectSizeMB ??= BaseTransportManager.MaximumReceivedObjectSize;
            result.MaxReceivedCommandSizeMB ??= BaseTransportManager.MaximumReceivedDataSize;
        /// 1. Unable to load type "{0}" specified in "InitializationParameters" section.
        internal PSSessionConfiguration CreateEndPointConfigurationInstance()
                return (PSSessionConfiguration)Activator.CreateInstance(EndPointConfigurationType);
            catch (TypeLoadException)
            // if we are here, that means we are unable to load the type specified
            // in the config xml.. notify the same.
            throw PSTraceSource.NewArgumentException("typeToLoad", RemotingErrorIdStrings.UnableToLoadType,
                    EndPointConfigurationTypeName, ConfigurationDataFromXML.INITPARAMETERSTOKEN);
    /// InitialSessionStateProvider is used by 3rd parties to provide shell configuration
    /// on the remote server.
    public abstract class PSSessionConfiguration : IDisposable
        /// Tracer for Server Remote session.
        [TraceSource("ServerRemoteSession", "ServerRemoteSession")]
        private static readonly PSTraceSource s_tracer = PSTraceSource.GetTracer("ServerRemoteSession", "ServerRemoteSession");
        #region public interfaces
        /// Derived classes must override this to supply an InitialSessionState
        /// to be used to construct a Runspace for the user.
        /// <param name="senderInfo">
        /// User Identity for which this information is requested
        public abstract InitialSessionState GetInitialSessionState(PSSenderInfo senderInfo);
        /// <param name="sessionConfigurationData"></param>
        /// <param name="senderInfo"></param>
        /// <param name="configProviderId"></param>
        public virtual InitialSessionState GetInitialSessionState(PSSessionConfigurationData sessionConfigurationData,
            PSSenderInfo senderInfo, string configProviderId)
        /// If null, then the size is unlimited. Default is 10MB.
        public virtual int? GetMaximumReceivedObjectSize(PSSenderInfo senderInfo)
            return BaseTransportManager.MaximumReceivedObjectSize;
        /// Default is 50MB.
        public virtual int? GetMaximumReceivedDataSizePerCommand(PSSenderInfo senderInfo)
            return BaseTransportManager.MaximumReceivedDataSize;
        /// Derived classes can override this method to provide application private data
        /// that is going to be sent to the client and exposed via
        /// <see cref="System.Management.Automation.Runspaces.PSSession.ApplicationPrivateData"/>,
        /// <see cref="System.Management.Automation.Runspaces.Runspace.GetApplicationPrivateData"/> and
        /// <see cref="System.Management.Automation.Runspaces.RunspacePool.GetApplicationPrivateData"/>
        /// <returns>Application private data or <see langword="null"/></returns>
        public virtual PSPrimitiveDictionary GetApplicationPrivateData(PSSenderInfo senderInfo)
        /// Dispose this configuration object. This will be called when a Runspace/RunspacePool
        /// created using InitialSessionState from this object is Closed.
        /// <param name="isDisposing"></param>
        #region GetInitialSessionState from 3rd party shell ids
        /// <param name="shellId"></param>
        /// 1. Non existent InitialSessionState provider for the shellID
        internal static ConfigurationDataFromXML LoadEndPointConfiguration(
            string shellId,
            string initializationParameters)
            ConfigurationDataFromXML configData = null;
            if (!s_ssnStateProviders.ContainsKey(initializationParameters))
                LoadRSConfigProvider(shellId, initializationParameters);
                if (!s_ssnStateProviders.TryGetValue(initializationParameters, out configData))
                    throw PSTraceSource.NewInvalidOperationException(RemotingErrorIdStrings.NonExistentInitialSessionStateProvider, shellId);
            return configData;
        private static void LoadRSConfigProvider(string shellId, string initializationParameters)
            ConfigurationDataFromXML configData = ConfigurationDataFromXML.Create(initializationParameters);
            Type endPointConfigType = LoadAndAnalyzeAssembly(shellId,
                configData.ApplicationBase,
                configData.AssemblyName,
                configData.EndPointConfigurationTypeName);
            Dbg.Assert(endPointConfigType != null, "EndPointConfiguration type cannot be null");
            configData.EndPointConfigurationType = endPointConfigType;
                    s_ssnStateProviders.Add(initializationParameters, configData);
        /// shellId for which the assembly is getting loaded
        /// <param name="applicationBase"></param>
        /// <param name="typeToLoad">
        /// type which is supplying the configuration.
        /// Type instance representing the EndPointConfiguration to load.
        /// This Type can be instantiated when needed.
        private static Type LoadAndAnalyzeAssembly(string shellId, string applicationBase,
            string assemblyName, string typeToLoad)
            if ((string.IsNullOrEmpty(assemblyName) && !string.IsNullOrEmpty(typeToLoad)) ||
                 (!string.IsNullOrEmpty(assemblyName) && string.IsNullOrEmpty(typeToLoad)))
                throw PSTraceSource.NewInvalidOperationException(RemotingErrorIdStrings.TypeNeedsAssembly,
                    ConfigurationDataFromXML.INITPARAMETERSTOKEN);
                PSEtwLog.LogAnalyticVerbose(PSEventId.LoadingPSCustomShellAssembly,
                    PSOpcode.Connect, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic,
                    assemblyName, shellId);
                assembly = LoadSsnStateProviderAssembly(applicationBase, assemblyName);
                    throw PSTraceSource.NewArgumentException(nameof(assemblyName), RemotingErrorIdStrings.UnableToLoadAssembly,
                        assemblyName, ConfigurationDataFromXML.INITPARAMETERSTOKEN);
            // configuration xml specified an assembly and typetoload.
                    PSEtwLog.LogAnalyticVerbose(PSEventId.LoadingPSCustomShellType,
                        typeToLoad, shellId);
                    Type type = assembly.GetType(typeToLoad, true, true);
                        throw PSTraceSource.NewArgumentException(nameof(typeToLoad), RemotingErrorIdStrings.UnableToLoadType,
                            typeToLoad, ConfigurationDataFromXML.INITPARAMETERSTOKEN);
            // load the default PowerShell since plugin config
            // did not specify a typename to load.
            return typeof(DefaultRemotePowerShellConfiguration);
        /// Sets the application's current working directory to <paramref name="applicationBase"/> and
        /// loads the assembly <paramref name="assemblyName"/>. Once the assembly is loaded, the application's
        /// current working directory is set back to the original value.
        // TODO: Send the exception message back to the client.
        private static Assembly LoadSsnStateProviderAssembly(string applicationBase, string assemblyName)
            Dbg.Assert(!string.IsNullOrEmpty(assemblyName), "AssemblyName cannot be null.");
            string originalDirectory = string.Empty;
                // changing current working directory allows CLR loader to load dependent assemblies
                    originalDirectory = Directory.GetCurrentDirectory();
                    Directory.SetCurrentDirectory(applicationBase);
                    s_tracer.TraceWarning("Not able to change current working directory to {0}: {1}",
                        applicationBase, e.Message);
                catch (PathTooLongException e)
            // Even if there is error changing current working directory..try to load the assembly
            // This is to allow assembly loading from GAC
                    result = Assembly.Load(new AssemblyName(assemblyName));
                    s_tracer.TraceWarning("Not able to load assembly {0}: {1}", assemblyName, e.Message);
                s_tracer.WriteLine("Loading assembly from path {0}", applicationBase);
                    string assemblyPath;
                    if (!Path.IsPathRooted(assemblyName))
                        if (!string.IsNullOrEmpty(applicationBase) && Directory.Exists(applicationBase))
                            assemblyPath = Path.Combine(applicationBase, assemblyName);
                            assemblyPath = Path.Combine(Directory.GetCurrentDirectory(), assemblyName);
                        // Rooted path of dll is provided.
                        assemblyPath = assemblyName;
                    result = Assembly.LoadFrom(assemblyPath);
                    // set the application's directory back to the original directory
                    Directory.SetCurrentDirectory(originalDirectory);
        // TODO: I think this should be moved to Utils..this way all versioning related
        // logic will be in one place.
        private static RegistryKey GetConfigurationProvidersRegistryKey()
                RegistryKey monadRootKey = PSSnapInReader.GetMonadRootKey();
                RegistryKey versionRoot = PSSnapInReader.GetVersionRootKey(monadRootKey, Utils.GetCurrentMajorVersion());
                RegistryKey configProviderKey = versionRoot.OpenSubKey(configProvidersKeyName);
                return configProviderKey;
        /// Read value from the property <paramref name="name"/> for registry <paramref name="registryKey"/>
        /// as string.
        /// <param name="registryKey">
        /// Registry key from which the value is read.
        /// Caller should make sure this is not null.
        /// Name of the property.
        /// <param name="mandatory">
        /// True, if the property should exist.
        /// False, otherwise.
        /// Value of the property.
        private static string
        ReadStringValue(RegistryKey registryKey, string name, bool mandatory)
            Dbg.Assert(!string.IsNullOrEmpty(name), "caller should validate the name parameter");
            Dbg.Assert(registryKey != null, "Caller should validate the registryKey parameter");
            object value = registryKey.GetValue(name);
            if (value == null && mandatory)
                s_tracer.TraceError("Mandatory property {0} not specified for registry key {1}",
                        name, registryKey.Name);
                throw PSTraceSource.NewArgumentException(nameof(name), RemotingErrorIdStrings.MandatoryValueNotPresent, name, registryKey.Name);
            string s = value as string;
            if (string.IsNullOrEmpty(s) && mandatory)
                s_tracer.TraceError("Value is null or empty for mandatory property {0} in {1}",
                throw PSTraceSource.NewArgumentException(nameof(name), RemotingErrorIdStrings.MandatoryValueNotInCorrectFormat, name, registryKey.Name);
        private const string configProvidersKeyName = "PSConfigurationProviders";
        private const string configProviderApplicationBaseKeyName = "ApplicationBase";
        private const string configProviderAssemblyNameKeyName = "AssemblyName";
        private static readonly Dictionary<string, ConfigurationDataFromXML> s_ssnStateProviders =
            new Dictionary<string, ConfigurationDataFromXML>(StringComparer.OrdinalIgnoreCase);
    /// Provides Default InitialSessionState.
    internal sealed class DefaultRemotePowerShellConfiguration : PSSessionConfiguration
        #region Method overrides
        public override InitialSessionState GetInitialSessionState(PSSenderInfo senderInfo)
            InitialSessionState result = InitialSessionState.CreateDefault2();
            // TODO: Remove this after RDS moved to $using
            if (senderInfo.ConnectionString != null && senderInfo.ConnectionString.Contains("MSP=7a83d074-bb86-4e52-aa3e-6cc73cc066c8"))
                PSSessionConfigurationData.IsServerManager = true;
        public override InitialSessionState GetInitialSessionState(PSSessionConfigurationData sessionConfigurationData, PSSenderInfo senderInfo, string configProviderId)
            ArgumentNullException.ThrowIfNull(sessionConfigurationData);
            ArgumentNullException.ThrowIfNull(senderInfo);
            ArgumentNullException.ThrowIfNull(configProviderId);
            InitialSessionState sessionState = InitialSessionState.CreateDefault2();
            // now get all the modules in the specified path and import the same
            if (sessionConfigurationData != null && sessionConfigurationData.ModulesToImportInternal != null)
                foreach (var module in sessionConfigurationData.ModulesToImportInternal)
                    var moduleName = module as string;
                        moduleName = Environment.ExpandEnvironmentVariables(moduleName);
                        sessionState.ImportPSModule(new[] { moduleName });
                        var moduleSpec = module as ModuleSpecification;
                            var modulesToImport = new Collection<ModuleSpecification> { moduleSpec };
                            sessionState.ImportPSModule(modulesToImport);
            return sessionState;
    #region Declarative InitialSession Configuration
    #region Supporting types
    /// Specifies type of initial session state to use. Valid values are Empty and Default.
    public enum SessionType
        /// Empty session state.
        Empty,
        /// Restricted remote server.
        RestrictedRemoteServer,
        /// Default session state.
        Default
    /// Configuration type entry.
    internal class ConfigTypeEntry
        internal delegate bool TypeValidationCallback(string key, object obj, PSCmdlet cmdlet, string path);
        internal TypeValidationCallback ValidationCallback;
        internal ConfigTypeEntry(string key, TypeValidationCallback callback)
            this.Key = key;
            this.ValidationCallback = callback;
    #region ConfigFileConstants
    /// Configuration file constants.
    internal static class ConfigFileConstants
        internal static readonly string AliasDefinitions = "AliasDefinitions";
        internal static readonly string AliasDescriptionToken = "Description";
        internal static readonly string AliasNameToken = "Name";
        internal static readonly string AliasOptionsToken = "Options";
        internal static readonly string AliasValueToken = "Value";
        internal static readonly string AssembliesToLoad = "AssembliesToLoad";
        internal static readonly string Author = "Author";
        internal static readonly string CompanyName = "CompanyName";
        internal static readonly string Copyright = "Copyright";
        internal static readonly string Description = "Description";
        internal static readonly string EnforceInputParameterValidation = "EnforceInputParameterValidation";
        internal static readonly string EnvironmentVariables = "EnvironmentVariables";
        internal static readonly string ExecutionPolicy = "ExecutionPolicy";
        internal static readonly string FormatsToProcess = "FormatsToProcess";
        internal static readonly string FunctionDefinitions = "FunctionDefinitions";
        internal static readonly string FunctionNameToken = "Name";
        internal static readonly string FunctionOptionsToken = "Options";
        internal static readonly string FunctionValueToken = "ScriptBlock";
        internal static readonly string GMSAAccount = "GroupManagedServiceAccount";
        internal static readonly string Guid = "GUID";
        internal static readonly string LanguageMode = "LanguageMode";
        internal static readonly string ModulesToImport = "ModulesToImport";
        internal static readonly string MountUserDrive = "MountUserDrive";
        internal static readonly string PowerShellVersion = "PowerShellVersion";
        internal static readonly string RequiredGroups = "RequiredGroups";
        internal static readonly string RoleDefinitions = "RoleDefinitions";
        internal static readonly string SchemaVersion = "SchemaVersion";
        internal static readonly string ScriptsToProcess = "ScriptsToProcess";
        internal static readonly string SessionType = "SessionType";
        internal static readonly string RoleCapabilities = "RoleCapabilities";
        internal static readonly string RoleCapabilityFiles = "RoleCapabilityFiles";
        internal static readonly string RunAsVirtualAccount = "RunAsVirtualAccount";
        internal static readonly string RunAsVirtualAccountGroups = "RunAsVirtualAccountGroups";
        internal static readonly string TranscriptDirectory = "TranscriptDirectory";
        internal static readonly string TypesToProcess = "TypesToProcess";
        internal static readonly string UserDriveMaxSize = "UserDriveMaximumSize";
        internal static readonly string VariableDefinitions = "VariableDefinitions";
        internal static readonly string VariableNameToken = "Name";
        internal static readonly string VariableValueToken = "Value";
        internal static readonly string VisibleAliases = "VisibleAliases";
        internal static readonly string VisibleCmdlets = "VisibleCmdlets";
        internal static readonly string VisibleFunctions = "VisibleFunctions";
        internal static readonly string VisibleProviders = "VisibleProviders";
        internal static readonly string VisibleExternalCommands = "VisibleExternalCommands";
        internal static readonly ConfigTypeEntry[] ConfigFileKeys = new ConfigTypeEntry[] {
            new ConfigTypeEntry(AliasDefinitions,                new ConfigTypeEntry.TypeValidationCallback(AliasDefinitionsTypeValidationCallback)),
            new ConfigTypeEntry(AssembliesToLoad,                new ConfigTypeEntry.TypeValidationCallback(StringArrayTypeValidationCallback)),
            new ConfigTypeEntry(Author,                          new ConfigTypeEntry.TypeValidationCallback(StringTypeValidationCallback)),
            new ConfigTypeEntry(CompanyName,                     new ConfigTypeEntry.TypeValidationCallback(StringTypeValidationCallback)),
            new ConfigTypeEntry(Copyright,                       new ConfigTypeEntry.TypeValidationCallback(StringTypeValidationCallback)),
            new ConfigTypeEntry(Description,                     new ConfigTypeEntry.TypeValidationCallback(StringTypeValidationCallback)),
            new ConfigTypeEntry(EnforceInputParameterValidation, new ConfigTypeEntry.TypeValidationCallback(BooleanTypeValidationCallback)),
            new ConfigTypeEntry(EnvironmentVariables,            new ConfigTypeEntry.TypeValidationCallback(HashtableTypeValidationCallback)),
            new ConfigTypeEntry(ExecutionPolicy,                 new ConfigTypeEntry.TypeValidationCallback(ExecutionPolicyValidationCallback)),
            new ConfigTypeEntry(FormatsToProcess,                new ConfigTypeEntry.TypeValidationCallback(StringArrayTypeValidationCallback)),
            new ConfigTypeEntry(FunctionDefinitions,             new ConfigTypeEntry.TypeValidationCallback(FunctionDefinitionsTypeValidationCallback)),
            new ConfigTypeEntry(GMSAAccount,                     new ConfigTypeEntry.TypeValidationCallback(StringTypeValidationCallback)),
            new ConfigTypeEntry(Guid,                            new ConfigTypeEntry.TypeValidationCallback(StringTypeValidationCallback)),
            new ConfigTypeEntry(LanguageMode,                    new ConfigTypeEntry.TypeValidationCallback(LanguageModeValidationCallback)),
            new ConfigTypeEntry(ModulesToImport,                 new ConfigTypeEntry.TypeValidationCallback(StringOrHashtableArrayTypeValidationCallback)),
            new ConfigTypeEntry(MountUserDrive,                  new ConfigTypeEntry.TypeValidationCallback(BooleanTypeValidationCallback)),
            new ConfigTypeEntry(PowerShellVersion,               new ConfigTypeEntry.TypeValidationCallback(StringTypeValidationCallback)),
            new ConfigTypeEntry(RequiredGroups,                  new ConfigTypeEntry.TypeValidationCallback(HashtableTypeValidationCallback)),
            new ConfigTypeEntry(RoleCapabilities,                new ConfigTypeEntry.TypeValidationCallback(StringArrayTypeValidationCallback)),
            new ConfigTypeEntry(RoleCapabilityFiles,             new ConfigTypeEntry.TypeValidationCallback(StringArrayTypeValidationCallback)),
            new ConfigTypeEntry(RoleDefinitions,                 new ConfigTypeEntry.TypeValidationCallback(HashtableTypeValidationCallback)),
            new ConfigTypeEntry(RunAsVirtualAccount,             new ConfigTypeEntry.TypeValidationCallback(BooleanTypeValidationCallback)),
            new ConfigTypeEntry(RunAsVirtualAccountGroups,       new ConfigTypeEntry.TypeValidationCallback(StringArrayTypeValidationCallback)),
            new ConfigTypeEntry(SchemaVersion,                   new ConfigTypeEntry.TypeValidationCallback(StringTypeValidationCallback)),
            new ConfigTypeEntry(ScriptsToProcess,                new ConfigTypeEntry.TypeValidationCallback(StringArrayTypeValidationCallback)),
            new ConfigTypeEntry(SessionType,                     new ConfigTypeEntry.TypeValidationCallback(ISSValidationCallback)),
            new ConfigTypeEntry(TranscriptDirectory,             new ConfigTypeEntry.TypeValidationCallback(StringTypeValidationCallback)),
            new ConfigTypeEntry(TypesToProcess,                  new ConfigTypeEntry.TypeValidationCallback(StringArrayTypeValidationCallback)),
            new ConfigTypeEntry(UserDriveMaxSize,                new ConfigTypeEntry.TypeValidationCallback(IntegerTypeValidationCallback)),
            new ConfigTypeEntry(VariableDefinitions,             new ConfigTypeEntry.TypeValidationCallback(VariableDefinitionsTypeValidationCallback)),
            new ConfigTypeEntry(VisibleAliases,                  new ConfigTypeEntry.TypeValidationCallback(StringArrayTypeValidationCallback)),
            new ConfigTypeEntry(VisibleCmdlets,                  new ConfigTypeEntry.TypeValidationCallback(StringArrayTypeValidationCallback)),
            new ConfigTypeEntry(VisibleFunctions,                new ConfigTypeEntry.TypeValidationCallback(StringArrayTypeValidationCallback)),
            new ConfigTypeEntry(VisibleProviders,                new ConfigTypeEntry.TypeValidationCallback(StringArrayTypeValidationCallback)),
            new ConfigTypeEntry(VisibleExternalCommands,         new ConfigTypeEntry.TypeValidationCallback(StringArrayTypeValidationCallback)),
        /// Checks if the given key is a valid key.
        /// <param name="de"></param>
        internal static bool IsValidKey(DictionaryEntry de, PSCmdlet cmdlet, string path)
            bool validKey = false;
            foreach (ConfigTypeEntry configEntry in ConfigFileKeys)
                if (string.Equals(configEntry.Key, de.Key.ToString(), StringComparison.OrdinalIgnoreCase))
                    validKey = true;
                    if (configEntry.ValidationCallback(de.Key.ToString(), de.Value, cmdlet, path))
            if (!validKey)
                cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCInvalidKey, de.Key.ToString(), path));
        private static bool ISSValidationCallback(string key, object obj, PSCmdlet cmdlet, string path)
            string value = obj as string;
                    Enum.Parse(typeof(SessionType), value, true);
                    // Do nothing here
            cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeValidEnum, key, typeof(SessionType).FullName,
                LanguagePrimitives.EnumSingleTypeConverter.EnumValues(typeof(SessionType)), path));
        private static bool LanguageModeValidationCallback(string key, object obj, PSCmdlet cmdlet, string path)
                    Enum.Parse(typeof(PSLanguageMode), value, true);
            cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeValidEnum, key, typeof(PSLanguageMode).FullName,
                LanguagePrimitives.EnumSingleTypeConverter.EnumValues(typeof(PSLanguageMode)), path));
        private static bool ExecutionPolicyValidationCallback(string key, object obj, PSCmdlet cmdlet, string path)
                    Enum.Parse(DISCUtils.ExecutionPolicyType, value, true);
            cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeValidEnum, key, DISCUtils.ExecutionPolicyType.FullName,
                LanguagePrimitives.EnumSingleTypeConverter.EnumValues(DISCUtils.ExecutionPolicyType), path));
        private static bool HashtableTypeValidationCallback(string key, object obj, PSCmdlet cmdlet, string path)
            Hashtable hash = obj as Hashtable;
                cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeHashtable, key, path));
        private static bool AliasDefinitionsTypeValidationCallback(string key, object obj, PSCmdlet cmdlet, string path)
            Hashtable[] hashtables = DISCPowerShellConfiguration.TryGetHashtableArray(obj);
            if (hashtables == null)
                cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeHashtableArray, key, path));
            foreach (Hashtable hashtable in hashtables)
                if (!hashtable.ContainsKey(AliasNameToken))
                    cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustContainKey, key, AliasNameToken, path));
                if (!hashtable.ContainsKey(AliasValueToken))
                    cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustContainKey, key, AliasValueToken, path));
                foreach (string aliasKey in hashtable.Keys)
                    if (!string.Equals(aliasKey, AliasNameToken, StringComparison.OrdinalIgnoreCase) &&
                       !string.Equals(aliasKey, AliasValueToken, StringComparison.OrdinalIgnoreCase) &&
                       !string.Equals(aliasKey, AliasDescriptionToken, StringComparison.OrdinalIgnoreCase) &&
                       !string.Equals(aliasKey, AliasOptionsToken, StringComparison.OrdinalIgnoreCase))
                        cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeContainsInvalidKey, aliasKey, key, path));
        private static bool FunctionDefinitionsTypeValidationCallback(string key, object obj, PSCmdlet cmdlet, string path)
                if (!hashtable.ContainsKey(FunctionNameToken))
                    cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustContainKey, key, FunctionNameToken, path));
                if (!hashtable.ContainsKey(FunctionValueToken))
                    cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustContainKey, key, FunctionValueToken, path));
                if (hashtable[FunctionValueToken] is not ScriptBlock)
                    cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCKeyMustBeScriptBlock, FunctionValueToken, key, path));
                    if (!string.Equals(functionKey, FunctionNameToken, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(functionKey, FunctionValueToken, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(functionKey, FunctionOptionsToken, StringComparison.OrdinalIgnoreCase))
                        cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeContainsInvalidKey, functionKey, key, path));
        private static bool VariableDefinitionsTypeValidationCallback(string key, object obj, PSCmdlet cmdlet, string path)
                if (!hashtable.ContainsKey(VariableNameToken))
                    cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustContainKey, key, VariableNameToken, path));
                if (!hashtable.ContainsKey(VariableValueToken))
                    cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustContainKey, key, VariableValueToken, path));
                    if (!string.Equals(variableKey, VariableNameToken, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(variableKey, VariableValueToken, StringComparison.OrdinalIgnoreCase))
                        cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeContainsInvalidKey, variableKey, key, path));
        /// Verifies a string type.
        private static bool StringTypeValidationCallback(string key, object obj, PSCmdlet cmdlet, string path)
            if (obj is not string)
                cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeString, key, path));
        /// Verifies a string array type.
        private static bool StringArrayTypeValidationCallback(string key, object obj, PSCmdlet cmdlet, string path)
            if (DISCPowerShellConfiguration.TryGetStringArray(obj) == null)
                cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeStringArray, key, path));
        private static bool BooleanTypeValidationCallback(string key, object obj, PSCmdlet cmdlet, string path)
            if (obj is not bool)
                cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeBoolean, key, path));
        private static bool IntegerTypeValidationCallback(string key, object obj, PSCmdlet cmdlet, string path)
            if (obj is not int && obj is not long)
                cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeInteger, key, path));
        /// Verifies that an array contains only string or hashtable elements.
        private static bool StringOrHashtableArrayTypeValidationCallback(string key, object obj, PSCmdlet cmdlet, string path)
            if (DISCPowerShellConfiguration.TryGetObjectsOfType<object>(obj, new Type[] { typeof(string), typeof(Hashtable) }) == null)
                cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeStringOrHashtableArrayInFile, key, path));
    #region DISC Utilities
    /// DISC utilities.
    internal static class DISCUtils
        internal static Type ExecutionPolicyType = null;
        /// !! NOTE that this list MUST be updated when new capability session configuration properties are added.
        private static readonly HashSet<string> s_allowedRoleCapabilityKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            "RoleCapabilities",
            "RoleCapabilityFiles",
            "ModulesToImport",
            "VisibleAliases",
            "VisibleCmdlets",
            "VisibleFunctions",
            "VisibleExternalCommands",
            "VisibleProviders",
            "AliasDefinitions",
            "FunctionDefinitions",
            "VariableDefinitions",
            "EnvironmentVariables",
            "AssembliesToLoad"
        internal static ExternalScriptInfo GetScriptInfoForFile(ExecutionContext context, string fileName, out string scriptName)
            ExternalScriptInfo scriptInfo = new ExternalScriptInfo(scriptName, fileName, context);
                context.AuthorizationManager.ShouldRunInternal(scriptInfo, CommandOrigin.Internal,
                    context.EngineHostInterface);
                // Verify that the PSversion is correct...
                CommandDiscovery.VerifyPSVersion(scriptInfo);
        /// Loads the configuration file into a hashtable.
        /// <param name="scriptInfo">The ExternalScriptInfo object.</param>
        /// <returns>Configuration hashtable.</returns>
        internal static Hashtable LoadConfigFile(ExecutionContext context, ExternalScriptInfo scriptInfo)
            object oldPSScriptRoot = context.GetVariableValue(SpecialVariables.PSScriptRootVarPath);
            object oldPSCommandPath = context.GetVariableValue(SpecialVariables.PSCommandPathVarPath);
                context.SetVariable(SpecialVariables.PSScriptRootVarPath, Path.GetDirectoryName(scriptInfo.Definition));
                context.SetVariable(SpecialVariables.PSCommandPathVarPath, scriptInfo.Definition);
                result = PSObject.Base(scriptInfo.ScriptBlock.InvokeReturnAsIs());
                context.SetVariable(SpecialVariables.PSScriptRootVarPath, oldPSScriptRoot);
                context.SetVariable(SpecialVariables.PSCommandPathVarPath, oldPSCommandPath);
            return result as Hashtable;
        /// Verifies the configuration hashtable.
        /// <param name="table">Configuration hashtable.</param>
        /// <returns>True if valid, false otherwise.</returns>
        internal static bool VerifyConfigTable(Hashtable table, PSCmdlet cmdlet, string path)
            bool hasSchemaVersion = false;
            foreach (DictionaryEntry de in table)
                if (!ConfigFileConstants.IsValidKey(de, cmdlet, path))
                if (de.Key.ToString().Equals(ConfigFileConstants.SchemaVersion, StringComparison.OrdinalIgnoreCase))
                    hasSchemaVersion = true;
            if (!hasSchemaVersion)
                cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCMissingSchemaVersion, path));
                ValidateAbsolutePaths(cmdlet.SessionState, table, path);
                ValidateExtensions(table, path);
                cmdlet.WriteVerbose(e.Message);
        private static void ValidatePS1XMLExtension(string key, string[] paths, string filePath)
                    string ext = System.IO.Path.GetExtension(path);
                    if (!ext.Equals(".ps1xml", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.DISCInvalidExtension, key, ext, ".ps1xml"));
                    throw new InvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.ErrorParsingTheKeyInPSSessionConfigurationFile, key, filePath), argumentException);
        private static void ValidatePS1OrPSM1Extension(string key, string[] paths, string filePath)
                    if (!ext.Equals(StringLiterals.PowerShellScriptFileExtension, StringComparison.OrdinalIgnoreCase) &&
                        !ext.Equals(StringLiterals.PowerShellModuleFileExtension, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.DISCInvalidExtension, key, ext,
                            string.Join(", ", StringLiterals.PowerShellScriptFileExtension, StringLiterals.PowerShellModuleFileExtension)));
        internal static void ValidateExtensions(Hashtable table, string filePath)
            if (table.ContainsKey(ConfigFileConstants.TypesToProcess))
                ValidatePS1XMLExtension(ConfigFileConstants.TypesToProcess, DISCPowerShellConfiguration.TryGetStringArray(table[ConfigFileConstants.TypesToProcess]), filePath);
            if (table.ContainsKey(ConfigFileConstants.FormatsToProcess))
                ValidatePS1XMLExtension(ConfigFileConstants.FormatsToProcess, DISCPowerShellConfiguration.TryGetStringArray(table[ConfigFileConstants.FormatsToProcess]), filePath);
            if (table.ContainsKey(ConfigFileConstants.ScriptsToProcess))
                ValidatePS1OrPSM1Extension(ConfigFileConstants.ScriptsToProcess, DISCPowerShellConfiguration.TryGetStringArray(table[ConfigFileConstants.ScriptsToProcess]), filePath);
        /// Checks if all paths are absolute paths.
        internal static void ValidateAbsolutePaths(SessionState state, Hashtable table, string filePath)
                ValidateAbsolutePath(state, ConfigFileConstants.TypesToProcess, DISCPowerShellConfiguration.TryGetStringArray(table[ConfigFileConstants.TypesToProcess]), filePath);
                ValidateAbsolutePath(state, ConfigFileConstants.FormatsToProcess, DISCPowerShellConfiguration.TryGetStringArray(table[ConfigFileConstants.FormatsToProcess]), filePath);
                ValidateAbsolutePath(state, ConfigFileConstants.ScriptsToProcess, DISCPowerShellConfiguration.TryGetStringArray(table[ConfigFileConstants.ScriptsToProcess]), filePath);
        /// Checks if a path is an absolute path.
        /// <param name="paths"></param>
        internal static void ValidateAbsolutePath(SessionState state, string key, string[] paths, string filePath)
            string driveName;
                if (!state.Path.IsPSAbsolute(path, out driveName))
                    throw new InvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.DISCPathsMustBeAbsolute, key, path, filePath));
        /// Validates Role Definition hash entries
        /// RoleDefinitions = @{
        ///     'Everyone' = @{
        ///         'RoIeCapabilities' = 'Basic' };
        ///     'Administrators' = @{
        ///         'VisibleCmdlets' = 'Get-Process','Get-Location'; 'VisibleFunctions = 'TabExpansion2' } }
        /// <param name="roleDefinitions"></param>
        internal static void ValidateRoleDefinitions(IDictionary roleDefinitions)
            foreach (var roleKey in roleDefinitions.Keys)
                if (roleKey is not string)
                    var invalidOperationEx = new PSInvalidOperationException(
                        string.Format(RemotingErrorIdStrings.InvalidRoleKeyType, roleKey.GetType().FullName));
                    invalidOperationEx.SetErrorId("InvalidRoleKeyType");
                    throw invalidOperationEx;
                // Each role capability in the role definition item should contain a hash table with allowed role capability key.
                IDictionary roleDefinition = roleDefinitions[roleKey] as IDictionary;
                if (roleDefinition == null)
                        StringUtil.Format(RemotingErrorIdStrings.InvalidRoleValue, roleKey));
                    invalidOperationEx.SetErrorId("InvalidRoleEntryNotHashtable");
                foreach (var key in roleDefinition.Keys)
                    // Ensure each role capability key is valid.
                    string roleCapabilityKey = key as string;
                    if (roleCapabilityKey == null)
                            string.Format(RemotingErrorIdStrings.InvalidRoleCapabilityKeyType, key.GetType().FullName));
                        invalidOperationEx.SetErrorId("InvalidRoleCapabilityKeyType");
                    if (!s_allowedRoleCapabilityKeys.Contains(roleCapabilityKey))
                            string.Format(RemotingErrorIdStrings.InvalidRoleCapabilityKey, roleCapabilityKey));
                        invalidOperationEx.SetErrorId("InvalidRoleCapabilityKey");
    #region DISCPowerShellConfiguration
    /// Creates an initial session state based on the configuration language for PSSC files.
    internal sealed class DISCPowerShellConfiguration : PSSessionConfiguration
        private readonly string _configFile;
        private readonly Hashtable _configHash;
        /// Gets the configuration hashtable that results from parsing the specified configuration file.
        internal Hashtable ConfigHash
            get { return _configHash; }
        /// Creates a new instance of a Declarative Initial Session State Configuration.
        /// <param name="configFile">The path to the .pssc file representing the initial session state.</param>
        /// <param name="validateFile">Validate file for supported configuration options.</param>
        internal DISCPowerShellConfiguration(
            string configFile,
            bool validateFile = false)
            _configFile = configFile;
            roleVerifier ??= static (role) => false;
            Runspace backupRunspace = Runspace.DefaultRunspace;
                Runspace.DefaultRunspace = RunspaceFactory.CreateRunspace();
                Runspace.DefaultRunspace.Open();
                ExternalScriptInfo script = DISCUtils.GetScriptInfoForFile(Runspace.DefaultRunspace.ExecutionContext,
                    configFile, out scriptName);
                _configHash = DISCUtils.LoadConfigFile(Runspace.DefaultRunspace.ExecutionContext, script);
                if (validateFile)
                    DISCFileValidation.ValidateContents(_configHash);
                MergeRoleRulesIntoConfigHash(roleVerifier);
                MergeRoleCapabilitiesIntoConfigHash();
                Runspace.DefaultRunspace.Close();
            catch (PSSecurityException e)
                string message = StringUtil.Format(RemotingErrorIdStrings.InvalidPSSessionConfigurationFilePath, configFile);
                PSInvalidOperationException ioe = new PSInvalidOperationException(message, e);
                ioe.SetErrorId("InvalidPSSessionConfigurationFilePath");
                Runspace.DefaultRunspace = backupRunspace;
        // Takes the "Roles" node in the config hash, and merges all that apply into the base configuration.
        private void MergeRoleRulesIntoConfigHash(Func<string, bool> roleVerifier)
            if (_configHash.ContainsKey(ConfigFileConstants.RoleDefinitions))
                // Extract the 'Roles' hashtable
                IDictionary roleEntry = _configHash[ConfigFileConstants.RoleDefinitions] as IDictionary;
                if (roleEntry == null)
                    string message = StringUtil.Format(RemotingErrorIdStrings.InvalidRoleEntry, _configHash["Roles"].GetType().FullName);
                    PSInvalidOperationException ioe = new PSInvalidOperationException(message);
                    ioe.SetErrorId("InvalidRoleDefinitionNotHashtable");
                // Ensure that role definitions contain valid entries.
                DISCUtils.ValidateRoleDefinitions(roleEntry);
                // Go through the Roles hashtable
                foreach (object role in roleEntry.Keys)
                    // Check if this role applies to the connected user
                    if (roleVerifier(role.ToString()))
                        // Extract their specific configuration
                        IDictionary roleCustomizations = roleEntry[role] as IDictionary;
                        if (roleCustomizations == null)
                            string message = StringUtil.Format(RemotingErrorIdStrings.InvalidRoleValue, role.ToString());
                            ioe.SetErrorId("InvalidRoleValueNotHashtable");
                        MergeConfigHashIntoConfigHash(roleCustomizations);
        // Takes the "RoleCapabilities" node in the config hash, and merges its values into the base configuration.
        private const string PSRCExtension = ".psrc";
        private void MergeRoleCapabilitiesIntoConfigHash()
            List<string> psrcFiles = new List<string>();
            if (_configHash.ContainsKey(ConfigFileConstants.RoleCapabilities))
                string[] roleCapabilities = TryGetStringArray(_configHash[ConfigFileConstants.RoleCapabilities]);
                if (roleCapabilities != null)
                    foreach (string roleCapability in roleCapabilities)
                        string roleCapabilityPath = GetRoleCapabilityPath(roleCapability);
                        if (string.IsNullOrEmpty(roleCapabilityPath))
                            string message = StringUtil.Format(RemotingErrorIdStrings.CouldNotFindRoleCapability, roleCapability, roleCapability + PSRCExtension);
                            ioe.SetErrorId("CouldNotFindRoleCapability");
                        psrcFiles.Add(roleCapabilityPath);
            if (ConfigHash.ContainsKey(ConfigFileConstants.RoleCapabilityFiles))
                string[] roleCapabilityFiles = TryGetStringArray(ConfigHash[ConfigFileConstants.RoleCapabilityFiles]);
                if (roleCapabilityFiles != null)
                    foreach (var roleCapabilityFilePath in roleCapabilityFiles)
                        if (!Path.GetExtension(roleCapabilityFilePath).Equals(PSRCExtension, StringComparison.OrdinalIgnoreCase))
                            string message = StringUtil.Format(RemotingErrorIdStrings.InvalidRoleCapabilityFileExtension, roleCapabilityFilePath);
                            ioe.SetErrorId("InvalidRoleCapabilityFileExtension");
                        if (!File.Exists(roleCapabilityFilePath))
                            string message = StringUtil.Format(RemotingErrorIdStrings.CouldNotFindRoleCapabilityFile, roleCapabilityFilePath);
                            ioe.SetErrorId("CouldNotFindRoleCapabilityFile");
                        psrcFiles.Add(roleCapabilityFilePath);
            foreach (var roleCapabilityFile in psrcFiles)
                DISCPowerShellConfiguration roleCapabilityConfiguration = new DISCPowerShellConfiguration(roleCapabilityFile, null);
                IDictionary roleCapabilityConfigurationItems = roleCapabilityConfiguration.ConfigHash;
                MergeConfigHashIntoConfigHash(roleCapabilityConfigurationItems);
        // Merge a role / role capability hashtable into the master configuration hashtable
        private void MergeConfigHashIntoConfigHash(IDictionary childConfigHash)
            foreach (object customization in childConfigHash.Keys)
                string customizationString = customization.ToString();
                var customizationValue = new List<object>();
                // First, take all values from the master config table
                if (_configHash.ContainsKey(customizationString))
                    IEnumerable existingValueAsCollection = LanguagePrimitives.GetEnumerable(_configHash[customization]);
                    if (existingValueAsCollection != null)
                        foreach (object value in existingValueAsCollection)
                            customizationValue.Add(value);
                        customizationValue.Add(_configHash[customization]);
                // Then add the current role's values
                IEnumerable newValueAsCollection = LanguagePrimitives.GetEnumerable(childConfigHash[customization]);
                if (newValueAsCollection != null)
                    foreach (object value in newValueAsCollection)
                    customizationValue.Add(childConfigHash[customization]);
                // Now update the config table for this role.
                _configHash[customization] = customizationValue.ToArray();
        private static string GetRoleCapabilityPath(string roleCapability)
            string moduleName = "*";
            if (roleCapability.Contains('\\'))
                string[] components = roleCapability.Split('\\', 2);
                moduleName = components[0];
                roleCapability = components[1];
            // Go through each directory in the module path
            string[] modulePaths = ModuleIntrinsics.GetModulePath().Split(Path.PathSeparator);
            foreach (string path in modulePaths)
                    // And then each module in that directory
                    foreach (string directory in Directory.EnumerateDirectories(path, moduleName))
                        string roleCapabilitiesPath = Path.Combine(directory, "RoleCapabilities");
                        if (Directory.Exists(roleCapabilitiesPath))
                            // If the role capabilities directory exists, look for .psrc files with the role capability name
                            foreach (string roleCapabilityPath in Directory.EnumerateFiles(roleCapabilitiesPath, roleCapability + ".psrc"))
                                return roleCapabilityPath;
                    // Could not enumerate the directories for a broken module path element. Just try the next.
        /// Creates an initial session state from a configuration file (DISC)
            // Create the initial session state
            string initialSessionState = TryGetValue(_configHash, ConfigFileConstants.SessionType);
            SessionType sessionType = SessionType.Default;
            bool cmdletVisibilityApplied = IsNonDefaultVisibilitySpecified(ConfigFileConstants.VisibleCmdlets);
            bool functionVisibilityApplied = IsNonDefaultVisibilitySpecified(ConfigFileConstants.VisibleFunctions);
            bool aliasVisibilityApplied = IsNonDefaultVisibilitySpecified(ConfigFileConstants.VisibleAliases);
            bool providerVisibilityApplied = IsNonDefaultVisibilitySpecified(ConfigFileConstants.VisibleProviders);
            bool processDefaultSessionStateVisibility = false;
            if (!string.IsNullOrEmpty(initialSessionState))
                sessionType = (SessionType)Enum.Parse(typeof(SessionType), initialSessionState, true);
                if (sessionType == SessionType.Empty)
                else if (sessionType == SessionType.RestrictedRemoteServer)
                    iss = InitialSessionState.CreateRestricted(SessionCapabilities.RemoteServer);
                    iss = InitialSessionState.CreateDefault2();
                    processDefaultSessionStateVisibility = true;
            if (cmdletVisibilityApplied || functionVisibilityApplied || aliasVisibilityApplied || providerVisibilityApplied ||
                IsNonDefaultVisibilitySpecified(ConfigFileConstants.VisibleExternalCommands))
                iss.DefaultCommandVisibility = SessionStateEntryVisibility.Private;
                // If visibility is applied on a default runspace then set initial ISS
                // commands visibility to private.
                if (processDefaultSessionStateVisibility)
                    foreach (var cmd in iss.Commands)
                        cmd.Visibility = iss.DefaultCommandVisibility;
            // Add providers
            if (providerVisibilityApplied)
                string[] providers = TryGetStringArray(_configHash[ConfigFileConstants.VisibleProviders]);
                    System.Collections.Generic.HashSet<string> addedProviders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (string provider in providers)
                        if (!string.IsNullOrEmpty(provider))
                            // Look up providers from provider name including wildcards.
                            var providersFound = iss.Providers.LookUpByName(provider);
                            foreach (var providerFound in providersFound)
                                if (!addedProviders.Contains(providerFound.Name))
                                    addedProviders.Add(providerFound.Name);
                                    providerFound.Visibility = SessionStateEntryVisibility.Public;
            // Add assemblies and modules);
            if (_configHash.ContainsKey(ConfigFileConstants.AssembliesToLoad))
                string[] assemblies = TryGetStringArray(_configHash[ConfigFileConstants.AssembliesToLoad]);
                if (assemblies != null)
                    foreach (string assembly in assemblies)
                        iss.Assemblies.Add(new SessionStateAssemblyEntry(assembly));
            if (_configHash.ContainsKey(ConfigFileConstants.ModulesToImport))
                object[] modules = TryGetObjectsOfType<object>(_configHash[ConfigFileConstants.ModulesToImport],
                                                               new Type[] { typeof(string), typeof(Hashtable) });
                if ((_configHash[ConfigFileConstants.ModulesToImport] != null) && (modules == null))
                    ioe.SetErrorId("InvalidModulesToImportKeyEntries");
                if (modules != null)
                    Collection<ModuleSpecification> modulesToImport = new Collection<ModuleSpecification>();
                    foreach (object module in modules)
                        ModuleSpecification moduleSpec = null;
                            moduleSpec = new ModuleSpecification(moduleName);
                            Hashtable moduleHash = module as Hashtable;
                            if (moduleHash != null)
                                moduleSpec = new ModuleSpecification(moduleHash);
                        // Now add the moduleSpec to modulesToImport
                            if (string.Equals(InitialSessionState.CoreModule, moduleSpec.Name,
                                    // Win8: 627752 Cannot load microsoft.powershell.core module as part of DISC
                                    // Convert Microsoft.PowerShell.Core module -> Microsoft.PowerShell.Core snapin.
                                    // Doing this Import only in SessionType.Empty case, because other cases already do this.
                                    // In V3, Microsoft.PowerShell.Core module is not installed externally.
                                    iss.ImportCorePSSnapIn();
                                // silently ignore Microsoft.PowerShell.Core for other cases ie., SessionType.RestrictedRemoteServer && SessionType.Default
                                modulesToImport.Add(moduleSpec);
                    iss.ImportPSModule(modulesToImport);
            // Define members
            if (_configHash.ContainsKey(ConfigFileConstants.VisibleCmdlets))
                object[] cmdlets = TryGetObjectsOfType<object>(_configHash[ConfigFileConstants.VisibleCmdlets],
                if (cmdlets == null)
                        ConfigFileConstants.VisibleCmdlets);
                    ioe.SetErrorId("InvalidVisibleCmdletsKeyEntries");
                ProcessVisibleCommands(iss, cmdlets);
            if (_configHash.ContainsKey(ConfigFileConstants.AliasDefinitions))
                Hashtable[] aliases = TryGetHashtableArray(_configHash[ConfigFileConstants.AliasDefinitions]);
                    foreach (Hashtable alias in aliases)
                        SessionStateAliasEntry entry = CreateSessionStateAliasEntry(alias, aliasVisibilityApplied);
                            // Indexing iss.Commands with a command that does not exist returns 'null', rather
                            // than some sort of KeyNotFound exception.
                            if (iss.Commands[entry.Name] != null)
                                iss.Commands.Remove(entry.Name, typeof(SessionStateAliasEntry));
                            iss.Commands.Add(entry);
            if (_configHash.ContainsKey(ConfigFileConstants.VisibleAliases))
                string[] aliases = DISCPowerShellConfiguration.TryGetStringArray(_configHash[ConfigFileConstants.VisibleAliases]);
                    foreach (string alias in aliases)
                            // Look up aliases using alias name including wildcards.
                            Collection<SessionStateCommandEntry> existingEntries = iss.Commands.LookUpByName(alias);
                            foreach (SessionStateCommandEntry existingEntry in existingEntries)
                                if (existingEntry.CommandType == CommandTypes.Alias)
                                    existingEntry.Visibility = SessionStateEntryVisibility.Public;
                            if (!found || WildcardPattern.ContainsWildcardCharacters(alias))
                                iss.UnresolvedCommandsToExpose.Add(alias);
            if (_configHash.ContainsKey(ConfigFileConstants.FunctionDefinitions))
                Hashtable[] functions = TryGetHashtableArray(_configHash[ConfigFileConstants.FunctionDefinitions]);
                    foreach (Hashtable function in functions)
                        SessionStateFunctionEntry entry = CreateSessionStateFunctionEntry(function, functionVisibilityApplied);
            if (_configHash.ContainsKey(ConfigFileConstants.VisibleFunctions))
                object[] functions = TryGetObjectsOfType<object>(_configHash[ConfigFileConstants.VisibleFunctions],
                if (functions == null)
                        ConfigFileConstants.VisibleFunctions);
                    ioe.SetErrorId("InvalidVisibleFunctionsKeyEntries");
                ProcessVisibleCommands(iss, functions);
            if (_configHash.ContainsKey(ConfigFileConstants.VariableDefinitions))
                Hashtable[] variables = TryGetHashtableArray(_configHash[ConfigFileConstants.VariableDefinitions]);
                    foreach (Hashtable variable in variables)
                        if (variable.ContainsKey(ConfigFileConstants.VariableValueToken) &&
                            variable[ConfigFileConstants.VariableValueToken] is ScriptBlock)
                            iss.DynamicVariablesToDefine.Add(variable);
                        SessionStateVariableEntry entry = CreateSessionStateVariableEntry(variable, iss.LanguageMode);
                            iss.Variables.Add(entry);
            if (_configHash.ContainsKey(ConfigFileConstants.EnvironmentVariables))
                Hashtable[] variablesList = TryGetHashtableArray(_configHash[ConfigFileConstants.EnvironmentVariables]);
                if (variablesList != null)
                    foreach (Hashtable variables in variablesList)
                        foreach (DictionaryEntry variable in variables)
                            SessionStateVariableEntry entry = new SessionStateVariableEntry(variable.Key.ToString(), variable.Value.ToString(), null);
                            iss.EnvironmentVariables.Add(entry);
            // Update type data
            if (_configHash.ContainsKey(ConfigFileConstants.TypesToProcess))
                string[] types = DISCPowerShellConfiguration.TryGetStringArray(_configHash[ConfigFileConstants.TypesToProcess]);
                            iss.Types.Add(new SessionStateTypeEntry(type));
            // Update format data
            if (_configHash.ContainsKey(ConfigFileConstants.FormatsToProcess))
                string[] formats = DISCPowerShellConfiguration.TryGetStringArray(_configHash[ConfigFileConstants.FormatsToProcess]);
                if (formats != null)
                    foreach (string format in formats)
                        if (!string.IsNullOrEmpty(format))
                            iss.Formats.Add(new SessionStateFormatEntry(format));
            // Add external commands
            if (_configHash.ContainsKey(ConfigFileConstants.VisibleExternalCommands))
                string[] externalCommands = TryGetStringArray(_configHash[ConfigFileConstants.VisibleExternalCommands]);
                if (externalCommands != null)
                    foreach (string command in externalCommands)
                        if (command.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
                            iss.Commands.Add(
                                new SessionStateScriptEntry(command, SessionStateEntryVisibility.Public));
                            if (command == "*")
                                new SessionStateApplicationEntry(command, SessionStateEntryVisibility.Public));
            // Register startup scripts
            if (_configHash.ContainsKey(ConfigFileConstants.ScriptsToProcess))
                string[] startupScripts = DISCPowerShellConfiguration.TryGetStringArray(_configHash[ConfigFileConstants.ScriptsToProcess]);
                if (startupScripts != null)
                    foreach (string script in startupScripts)
                            iss.StartupScripts.Add(script);
            // Now apply visibility logic
            if (cmdletVisibilityApplied || functionVisibilityApplied || aliasVisibilityApplied || providerVisibilityApplied)
                if (sessionType == SessionType.Default)
                    // autoloading preference is none, so modules cannot be autoloaded. Since the session type is default,
                    // load PowerShell default modules
                    iss.ImportPSCoreModule(InitialSessionState.EngineModules.ToArray());
                if (cmdletVisibilityApplied)
                    // Import-Module is needed for internal *required modules* processing, so make visibility private.
                    var importModuleEntry = iss.Commands["Import-Module"];
                    if (importModuleEntry.Count == 1)
                        importModuleEntry[0].Visibility = SessionStateEntryVisibility.Private;
                if (aliasVisibilityApplied)
                    var importModuleAliasEntry = iss.Commands["ipmo"];
                    if (importModuleAliasEntry.Count == 1)
                        importModuleAliasEntry[0].Visibility = SessionStateEntryVisibility.Private;
                iss.Variables.Add(new SessionStateVariableEntry(SpecialVariables.PSModuleAutoLoading, PSModuleAutoLoadingPreference.None, string.Empty, ScopedItemOptions.None));
            if (_configHash.ContainsKey(ConfigFileConstants.ExecutionPolicy))
                Microsoft.PowerShell.ExecutionPolicy executionPolicy = (Microsoft.PowerShell.ExecutionPolicy)Enum.Parse(
                    typeof(Microsoft.PowerShell.ExecutionPolicy), _configHash[ConfigFileConstants.ExecutionPolicy].ToString(), true);
                iss.ExecutionPolicy = executionPolicy;
            if (_configHash.ContainsKey(ConfigFileConstants.LanguageMode))
                System.Management.Automation.PSLanguageMode languageMode = (System.Management.Automation.PSLanguageMode)Enum.Parse(
                    typeof(System.Management.Automation.PSLanguageMode), _configHash[ConfigFileConstants.LanguageMode].ToString(), true);
                iss.LanguageMode = languageMode;
            // Set the transcript directory
            if (_configHash.ContainsKey(ConfigFileConstants.TranscriptDirectory))
                iss.TranscriptDirectory = _configHash[ConfigFileConstants.TranscriptDirectory].ToString();
            // Process User Drive
            if (_configHash.ContainsKey(ConfigFileConstants.MountUserDrive))
                if (Convert.ToBoolean(_configHash[ConfigFileConstants.MountUserDrive], CultureInfo.InvariantCulture))
                    iss.UserDriveEnabled = true;
                    iss.UserDriveUserName = senderInfo?.UserInfo.Identity.Name;
                    // Set user drive max drive if provided.
                    if (_configHash.ContainsKey(ConfigFileConstants.UserDriveMaxSize))
                        long userDriveMaxSize = Convert.ToInt64(_configHash[ConfigFileConstants.UserDriveMaxSize]);
                        if (userDriveMaxSize > 0)
                            iss.UserDriveMaximumSize = userDriveMaxSize;
                    // Input parameter validation enforcement is always true when a User drive is created.
                    iss.EnforceInputParameterValidation = true;
                    // Add function definitions for Copy-Item support.
                    ProcessCopyItemFunctionDefinitions(iss);
        // Adds Copy-Item remote session helper functions to the ISS.
        private static void ProcessCopyItemFunctionDefinitions(InitialSessionState iss)
            // Copy file to remote helper functions.
            foreach (var copyToRemoteFn in CopyFileRemoteUtils.GetAllCopyToRemoteScriptFunctions())
                var functionEntry = new SessionStateFunctionEntry(
                    copyToRemoteFn["Name"] as string,
                    copyToRemoteFn["Definition"] as string);
                iss.Commands.Add(functionEntry);
            // Copy file from remote helper functions.
            foreach (var copyFromRemoteFn in CopyFileRemoteUtils.GetAllCopyFromRemoteScriptFunctions())
                    copyFromRemoteFn["Name"] as string,
                    copyFromRemoteFn["Definition"] as string);
        private static void ProcessVisibleCommands(InitialSessionState iss, object[] commands)
            // A dictionary of: function name -> Parameters
            // Parameters = A dictionary of parameter names -> Modifications
            // Modifications = A dictionary of modification types (ValidatePattern, ValidateSet) to the interim value
            //  for that attribute, as a HashSet of strings. For ValidateSet, this will be used as a collection of strings
            //  directly during proxy generation. For ValidatePattern, it will be combined into a regex
            //  like: '^(Pattern1|Pattern2|Pattern3)$' during proxy generation.
            Dictionary<string, Hashtable> commandModifications = new Dictionary<string, Hashtable>(StringComparer.OrdinalIgnoreCase);
            // Create a hash set of current modules to import so that fully qualified commands can include their
            // module if needed.
            HashSet<string> commandModuleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var moduleSpec in iss.ModuleSpecificationsToImport)
                commandModuleNames.Add(moduleSpec.Name);
            foreach (object commandObject in commands)
                if (commandObject == null)
                // If it's just a string, this is a visible command
                string command = commandObject as string;
                if (!string.IsNullOrEmpty(command))
                    ProcessVisibleCommand(iss, command, commandModuleNames);
                    // If it's a hashtable, it represents a customization to a cmdlet.
                    // (I.e.: Exposed parameter with ValidateSet and / or ValidatePattern)
                    // Collect these so that we can post-process them.
                    IDictionary commandModification = commandObject as IDictionary;
                    if (commandModification != null)
                        ProcessCommandModification(commandModifications, commandModification);
            // Now, save the commandModifications table for post-processing during runspace creation,
            // where we have the command info
            foreach (var pair in commandModifications)
                iss.CommandModifications.Add(pair.Key, pair.Value);
        private static void ProcessCommandModification(Dictionary<string, Hashtable> commandModifications, IDictionary commandModification)
            string commandName = commandModification["Name"] as string;
            Hashtable[] parameters = null;
            if (commandModification.Contains("Parameters"))
                parameters = TryGetHashtableArray(commandModification["Parameters"]);
                    // Validate that the parameter restriction has the right keys
                    foreach (Hashtable parameter in parameters)
                        if (!parameter.ContainsKey("Name"))
                            parameters = null;
            // Validate that we got the Name and Parameters keys
            if ((commandName == null) || (parameters == null))
                string hashtableKey = commandName;
                if (string.IsNullOrEmpty(hashtableKey))
                    IEnumerator errorKey = commandModification.Keys.GetEnumerator();
                    errorKey.MoveNext();
                    hashtableKey = errorKey.Current.ToString();
                string message = StringUtil.Format(RemotingErrorIdStrings.DISCCommandModificationSyntax, hashtableKey);
                ioe.SetErrorId("InvalidVisibleCommandKeyEntries");
            // Ensure we have the hashtable representing the current command being modified
            Hashtable parameterModifications;
            if (!commandModifications.TryGetValue(commandName, out parameterModifications))
                parameterModifications = new Hashtable(StringComparer.OrdinalIgnoreCase);
                commandModifications[commandName] = parameterModifications;
            foreach (IDictionary parameter in parameters)
                // Ensure we have the hashtable representing the current parameter being modified
                string parameterName = parameter["Name"].ToString();
                Hashtable currentParameterModification = parameterModifications[parameterName] as Hashtable;
                if (currentParameterModification == null)
                    currentParameterModification = new Hashtable(StringComparer.OrdinalIgnoreCase);
                    parameterModifications[parameterName] = currentParameterModification;
                foreach (string parameterModification in parameter.Keys)
                    if (string.Equals("Name", parameterModification, StringComparison.OrdinalIgnoreCase))
                    // Go through the keys, adding them to the current parameter modification
                    if (!currentParameterModification.Contains(parameterModification))
                        currentParameterModification[parameterModification] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    HashSet<string> currentParameterModificationValue = (HashSet<string>)currentParameterModification[parameterModification];
                    foreach (string parameterModificationValue in TryGetStringArray(parameter[parameterModification]))
                        if (!string.IsNullOrEmpty(parameterModificationValue))
                            currentParameterModificationValue.Add(parameterModificationValue);
        private static void ProcessVisibleCommand(InitialSessionState iss, string command, HashSet<string> moduleNames)
            // Defer module restricted command processing to runspace bind time.
            // Module restricted command names are fully qualified with the module name:
            // <ModuleName>\<CommandName>, e.g., PSScheduledJob\*JobTrigger*
            if (command.IndexOf('\\') < 0)
                // Look up commands from command name including wildcards.
                Collection<SessionStateCommandEntry> existingEntries = iss.Commands.LookUpByName(command);
                    if ((existingEntry.CommandType == CommandTypes.Cmdlet) || (existingEntry.CommandType == CommandTypes.Function))
                // Extract the module name and ensure it is part of the ISS modules to process list.
                Utils.ParseCommandName(command, out moduleName);
                if (!string.IsNullOrEmpty(moduleName) && !moduleNames.Contains(moduleName))
                    moduleNames.Add(moduleName);
                    iss.ImportPSModule(new string[] { moduleName });
            if (!found || WildcardPattern.ContainsWildcardCharacters(command))
                iss.UnresolvedCommandsToExpose.Add(command);
        /// Creates an alias entry.
        private static SessionStateAliasEntry CreateSessionStateAliasEntry(Hashtable alias, bool isAliasVisibilityDefined)
            string name = TryGetValue(alias, ConfigFileConstants.AliasNameToken);
            string value = TryGetValue(alias, ConfigFileConstants.AliasValueToken);
            string description = TryGetValue(alias, ConfigFileConstants.AliasDescriptionToken);
            string optionsString = TryGetValue(alias, ConfigFileConstants.AliasOptionsToken);
            if (!string.IsNullOrEmpty(optionsString))
                options = (ScopedItemOptions)Enum.Parse(typeof(ScopedItemOptions), optionsString, true);
            SessionStateEntryVisibility visibility = SessionStateEntryVisibility.Private;
            if (!isAliasVisibilityDefined)
                visibility = SessionStateEntryVisibility.Public;
            return new SessionStateAliasEntry(name, value, description, options, visibility);
        /// Creates a function entry.
        private static SessionStateFunctionEntry CreateSessionStateFunctionEntry(Hashtable function, bool isFunctionVisibilityDefined)
            string name = TryGetValue(function, ConfigFileConstants.FunctionNameToken);
            string value = TryGetValue(function, ConfigFileConstants.FunctionValueToken);
            string optionsString = TryGetValue(function, ConfigFileConstants.FunctionOptionsToken);
            ScriptBlock newFunction = ScriptBlock.Create(value);
            newFunction.LanguageMode = PSLanguageMode.FullLanguage;
            SessionStateEntryVisibility functionVisibility = SessionStateEntryVisibility.Private;
            if (!isFunctionVisibilityDefined)
                functionVisibility = SessionStateEntryVisibility.Public;
            return new SessionStateFunctionEntry(name, value, options, functionVisibility, newFunction, null);
        /// Creates a variable entry.
        private static SessionStateVariableEntry CreateSessionStateVariableEntry(Hashtable variable, PSLanguageMode languageMode)
            string name = TryGetValue(variable, ConfigFileConstants.VariableNameToken);
            string value = TryGetValue(variable, ConfigFileConstants.VariableValueToken);
            string description = TryGetValue(variable, ConfigFileConstants.AliasDescriptionToken);
            string optionsString = TryGetValue(variable, ConfigFileConstants.AliasOptionsToken);
            if (languageMode == PSLanguageMode.FullLanguage)
            return new SessionStateVariableEntry(name, value, description, options, new Collections.ObjectModel.Collection<Attribute>(), visibility);
        /// Applies the command (cmdlet/function/alias) visibility settings to the <paramref name="iss"/>
        /// <param name="configFileKey"></param>
        private bool IsNonDefaultVisibilitySpecified(string configFileKey)
            if (_configHash.ContainsKey(configFileKey))
                string[] commands =
                    DISCPowerShellConfiguration.TryGetStringArray(_configHash[configFileKey]);
                if ((commands == null) || (commands.Length == 0))
        /// Attempts to get a value from a hashtable.
        internal static string TryGetValue(Hashtable table, string key)
            if (table.ContainsKey(key))
                return table[key].ToString();
        /// Attempts to get a hashtable array from an object.
        /// <param name="hashObj"></param>
        internal static Hashtable[] TryGetHashtableArray(object hashObj)
            // Scalar case
            Hashtable hashtable = hashObj as Hashtable;
                return new[] { hashtable };
            // 1. Direct conversion
            Hashtable[] hashArray = hashObj as Hashtable[];
            if (hashArray == null)
                // 2. Convert from object array
                object[] objArray = hashObj as object[];
                if (objArray != null)
                    hashArray = new Hashtable[objArray.Length];
                    for (int i = 0; i < hashArray.Length; i++)
                        if (objArray[i] is not Hashtable hash)
                        hashArray[i] = hash;
            return hashArray;
        /// Attempts to get a string array from a hashtable.
        internal static string[] TryGetStringArray(object hashObj)
            object[] objs = hashObj as object[];
            if (objs == null)
                object obj = hashObj as object;
                    return new string[] { obj.ToString() };
            string[] result = new string[objs.Length];
            for (int i = 0; i < objs.Length; i++)
                result[i] = objs[i].ToString();
        internal static T[] TryGetObjectsOfType<T>(object hashObj, IEnumerable<Type> types) where T : class
                object obj = hashObj;
                    foreach (Type type in types)
                        if (obj.GetType().Equals(type))
                            return new T[] { obj as T };
            T[] result = new T[objs.Length];
                int i1 = i;
                if (types.Any(type => objs[i1].GetType().Equals(type)))
                    result[i] = objs[i] as T;
    #region DISCFileValidation
    internal static class DISCFileValidation
        // Set of supported configuration options for a PowerShell InitialSessionState.
        private static readonly HashSet<string> SupportedConfigOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            "AssembliesToLoad",
            "MountUserDrive",
            "SchemaVersion",
            "SessionType",
            "TranscriptDirectory",
            "UserDriveMaximumSize",
            "VisibleProviders"
            "ExecutionPolicy",
        // These are configuration options for WSMan (WinRM) endpoint configurations, that
        // appear in .pssc files, but are not part of PowerShell InitialSessionState.
        private static readonly HashSet<string> UnsupportedConfigOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            "GroupManagedServiceAccount",
            "RequiredGroups",
            "RoleDefinitions",
            "RunAsVirtualAccount",
            "RunAsVirtualAccountGroups"
        internal static void ValidateContents(Hashtable configHash)
            foreach (var key in configHash.Keys)
                if (key is not string keyName)
                    throw new PSInvalidOperationException(RemotingErrorIdStrings.DISCInvalidConfigKeyType);
                if (UnsupportedConfigOptions.Contains(keyName))
                        StringUtil.Format(RemotingErrorIdStrings.DISCUnsupportedConfigName, keyName));
                if (!SupportedConfigOptions.Contains(keyName))
                        StringUtil.Format(RemotingErrorIdStrings.DISCUnknownConfigName, keyName));

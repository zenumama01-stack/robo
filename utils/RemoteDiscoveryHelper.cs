    internal static class RemoteDiscoveryHelper
        #region PSRP
        private static Collection<string> RehydrateHashtableKeys(PSObject pso, string propertyName)
            const DeserializingTypeConverter.RehydrationFlags rehydrationFlags = DeserializingTypeConverter.RehydrationFlags.NullValueOk |
                                   DeserializingTypeConverter.RehydrationFlags.MissingPropertyOk;
            Hashtable hashtable = DeserializingTypeConverter.GetPropertyValue<Hashtable>(pso, propertyName, rehydrationFlags);
            if (hashtable == null)
                List<string> list = hashtable
                    .Keys
                    .Where(static k => k != null)
                    .Select(static k => k.ToString())
                    .Where(static s => s != null)
                return new Collection<string>(list);
        internal static PSModuleInfo RehydratePSModuleInfo(PSObject deserializedModuleInfo)
            string name = DeserializingTypeConverter.GetPropertyValue<string>(deserializedModuleInfo, "Name", rehydrationFlags);
            string path = DeserializingTypeConverter.GetPropertyValue<string>(deserializedModuleInfo, "Path", rehydrationFlags);
            PSModuleInfo moduleInfo = new PSModuleInfo(name, path, context: null, sessionState: null);
            moduleInfo.SetGuid(DeserializingTypeConverter.GetPropertyValue<Guid>(deserializedModuleInfo, "Guid", rehydrationFlags));
            moduleInfo.SetModuleType(DeserializingTypeConverter.GetPropertyValue<ModuleType>(deserializedModuleInfo, "ModuleType", rehydrationFlags));
            moduleInfo.SetVersion(DeserializingTypeConverter.GetPropertyValue<Version>(deserializedModuleInfo, "Version", rehydrationFlags));
            moduleInfo.SetHelpInfoUri(DeserializingTypeConverter.GetPropertyValue<string>(deserializedModuleInfo, "HelpInfoUri", rehydrationFlags));
            moduleInfo.AccessMode = DeserializingTypeConverter.GetPropertyValue<ModuleAccessMode>(deserializedModuleInfo, "AccessMode", rehydrationFlags);
            moduleInfo.Author = DeserializingTypeConverter.GetPropertyValue<string>(deserializedModuleInfo, "Author", rehydrationFlags);
            moduleInfo.ClrVersion = DeserializingTypeConverter.GetPropertyValue<Version>(deserializedModuleInfo, "ClrVersion", rehydrationFlags);
            moduleInfo.CompanyName = DeserializingTypeConverter.GetPropertyValue<string>(deserializedModuleInfo, "CompanyName", rehydrationFlags);
            moduleInfo.Copyright = DeserializingTypeConverter.GetPropertyValue<string>(deserializedModuleInfo, "Copyright", rehydrationFlags);
            moduleInfo.Description = DeserializingTypeConverter.GetPropertyValue<string>(deserializedModuleInfo, "Description", rehydrationFlags);
            moduleInfo.DotNetFrameworkVersion = DeserializingTypeConverter.GetPropertyValue<Version>(deserializedModuleInfo, "DotNetFrameworkVersion", rehydrationFlags);
            moduleInfo.PowerShellHostName = DeserializingTypeConverter.GetPropertyValue<string>(deserializedModuleInfo, "PowerShellHostName", rehydrationFlags);
            moduleInfo.PowerShellHostVersion = DeserializingTypeConverter.GetPropertyValue<Version>(deserializedModuleInfo, "PowerShellHostVersion", rehydrationFlags);
            moduleInfo.PowerShellVersion = DeserializingTypeConverter.GetPropertyValue<Version>(deserializedModuleInfo, "PowerShellVersion", rehydrationFlags);
            moduleInfo.ProcessorArchitecture = DeserializingTypeConverter.GetPropertyValue<Reflection.ProcessorArchitecture>(deserializedModuleInfo, "ProcessorArchitecture", rehydrationFlags);
            moduleInfo.DeclaredAliasExports = RehydrateHashtableKeys(deserializedModuleInfo, "ExportedAliases");
            moduleInfo.DeclaredCmdletExports = RehydrateHashtableKeys(deserializedModuleInfo, "ExportedCmdlets");
            moduleInfo.DeclaredFunctionExports = RehydrateHashtableKeys(deserializedModuleInfo, "ExportedFunctions");
            moduleInfo.DeclaredVariableExports = RehydrateHashtableKeys(deserializedModuleInfo, "ExportedVariables");
            var compatiblePSEditions = DeserializingTypeConverter.GetPropertyValue<string[]>(deserializedModuleInfo, "CompatiblePSEditions", rehydrationFlags);
            if (compatiblePSEditions != null && compatiblePSEditions.Length > 0)
                foreach (var edition in compatiblePSEditions)
                    moduleInfo.AddToCompatiblePSEditions(edition);
            // PowerShellGet related properties
            var tags = DeserializingTypeConverter.GetPropertyValue<string[]>(deserializedModuleInfo, "Tags", rehydrationFlags);
            if (tags != null && tags.Length > 0)
                foreach (var tag in tags)
                    moduleInfo.AddToTags(tag);
            moduleInfo.ReleaseNotes = DeserializingTypeConverter.GetPropertyValue<string>(deserializedModuleInfo, "ReleaseNotes", rehydrationFlags);
            moduleInfo.ProjectUri = DeserializingTypeConverter.GetPropertyValue<Uri>(deserializedModuleInfo, "ProjectUri", rehydrationFlags);
            moduleInfo.LicenseUri = DeserializingTypeConverter.GetPropertyValue<Uri>(deserializedModuleInfo, "LicenseUri", rehydrationFlags);
            moduleInfo.IconUri = DeserializingTypeConverter.GetPropertyValue<Uri>(deserializedModuleInfo, "IconUri", rehydrationFlags);
            moduleInfo.RepositorySourceLocation = DeserializingTypeConverter.GetPropertyValue<Uri>(deserializedModuleInfo, "RepositorySourceLocation", rehydrationFlags);
        private static EventHandler<DataAddedEventArgs> GetStreamForwarder<T>(Action<T> forwardingAction, bool swallowInvalidOperationExceptions = false)
            // TODO/FIXME: ETW event for extended semantics streams
            return (object sender, DataAddedEventArgs eventArgs) =>
                var psDataCollection = (PSDataCollection<T>)sender;
                foreach (T t in psDataCollection.ReadAll())
                        forwardingAction(t);
                        if (!swallowInvalidOperationExceptions)
        // This is a static field (instead of a constant) to make it possible to set through tests (and/or by customers if needed for a workaround)
        private static readonly int s_blockingCollectionCapacity = 1000;
        private static IEnumerable<PSObject> InvokeTopLevelPowerShell(
            PowerShell powerShell,
            PSInvocationSettings invocationSettings,
            string errorMessageTemplate,
            using (var mergedOutput = new BlockingCollection<Func<PSCmdlet, IEnumerable<PSObject>>>(s_blockingCollectionCapacity))
                var asyncOutput = new PSDataCollection<PSObject>();
                EventHandler<DataAddedEventArgs> outputHandler = GetStreamForwarder<PSObject>(
                    output => mergedOutput.Add(_ => new[] { output }),
                    swallowInvalidOperationExceptions: true);
                EventHandler<DataAddedEventArgs> errorHandler = GetStreamForwarder<ErrorRecord>(
                    errorRecord => mergedOutput.Add(
                        (PSCmdlet c) =>
                            errorRecord = GetErrorRecordForRemotePipelineInvocation(errorRecord, errorMessageTemplate);
                            HandleErrorFromPipeline(c, errorRecord, powerShell);
                            return Enumerable.Empty<PSObject>();
                EventHandler<DataAddedEventArgs> warningHandler = GetStreamForwarder<WarningRecord>(
                    warningRecord => mergedOutput.Add(
                            c.WriteWarning(warningRecord.Message);
                EventHandler<DataAddedEventArgs> verboseHandler = GetStreamForwarder<VerboseRecord>(
                    verboseRecord => mergedOutput.Add(
                            c.WriteVerbose(verboseRecord.Message);
                EventHandler<DataAddedEventArgs> debugHandler = GetStreamForwarder<DebugRecord>(
                    debugRecord => mergedOutput.Add(
                            c.WriteDebug(debugRecord.Message);
                EventHandler<DataAddedEventArgs> informationHandler = GetStreamForwarder<InformationRecord>(
                    informationRecord => mergedOutput.Add(
                            c.WriteInformation(informationRecord);
                asyncOutput.DataAdded += outputHandler;
                powerShell.Streams.Error.DataAdded += errorHandler;
                powerShell.Streams.Warning.DataAdded += warningHandler;
                powerShell.Streams.Verbose.DataAdded += verboseHandler;
                powerShell.Streams.Debug.DataAdded += debugHandler;
                powerShell.Streams.Information.DataAdded += informationHandler;
                    // TODO/FIXME: ETW event for PowerShell invocation
                    var asyncResult = powerShell.BeginInvoke<PSObject, PSObject>(
                        output: asyncOutput,
                        settings: invocationSettings,
                        callback: delegate
                                          mergedOutput.CompleteAdding();
                                      // ignore exceptions thrown because mergedOutput.CompleteAdding was called
                        state: null);
                    using (cancellationToken.Register(powerShell.Stop))
                            foreach (Func<PSCmdlet, IEnumerable<PSObject>> mergedOutputItem in mergedOutput.GetConsumingEnumerable())
                                foreach (PSObject outputObject in mergedOutputItem(cmdlet))
                                    yield return outputObject;
                    asyncOutput.DataAdded -= outputHandler;
                    powerShell.Streams.Error.DataAdded -= errorHandler;
                    powerShell.Streams.Warning.DataAdded -= warningHandler;
                    powerShell.Streams.Verbose.DataAdded -= verboseHandler;
                    powerShell.Streams.Debug.DataAdded -= debugHandler;
                    powerShell.Streams.Information.DataAdded -= informationHandler;
        private static IEnumerable<PSObject> InvokeNestedPowerShell(
                (ErrorRecord errorRecord) =>
                    HandleErrorFromPipeline(cmdlet, errorRecord, powerShell);
                    foreach (PSObject outputObject in powerShell.Invoke<PSObject>(null, invocationSettings))
        private static void CopyParameterFromCmdletToPowerShell(Cmdlet cmdlet, PowerShell powerShell, string parameterName)
            object parameterValue;
            if (!cmdlet.MyInvocation.BoundParameters.TryGetValue(parameterName, out parameterValue))
            var commandParameter = new CommandParameter(parameterName, parameterValue);
            foreach (var command in powerShell.Commands.Commands)
                if (command.Parameters.Any(existingParameter => existingParameter.Name.Equals(parameterName, StringComparison.OrdinalIgnoreCase)))
                command.Parameters.Add(commandParameter);
        internal static ErrorRecord GetErrorRecordForProcessingOfCimModule(Exception innerException, string moduleName)
                Modules.RemoteDiscoveryFailedToProcessRemoteModule,
            Exception outerException = new InvalidOperationException(errorMessage, innerException);
            ErrorRecord errorRecord = new ErrorRecord(outerException, innerException.GetType().Name, ErrorCategory.NotSpecified, moduleName);
        private const string DiscoveryProviderNotFoundErrorId = "DiscoveryProviderNotFound";
        private static ErrorRecord GetErrorRecordForRemoteDiscoveryProvider(Exception innerException)
            CimException cimException = innerException as CimException;
            if ((cimException != null) &&
                ((cimException.NativeErrorCode == NativeErrorCode.InvalidNamespace) ||
                 (cimException.NativeErrorCode == NativeErrorCode.InvalidClass) ||
                 (cimException.NativeErrorCode == NativeErrorCode.MethodNotFound) ||
                 (cimException.NativeErrorCode == NativeErrorCode.MethodNotAvailable)))
                    Modules.RemoteDiscoveryProviderNotFound,
                ErrorRecord errorRecord = new ErrorRecord(outerException, DiscoveryProviderNotFoundErrorId, ErrorCategory.NotImplemented, null);
                    Modules.RemoteDiscoveryFailureFromDiscoveryProvider,
                ErrorRecord errorRecord = new ErrorRecord(outerException, "DiscoveryProviderFailure", ErrorCategory.NotSpecified, null);
        private static ErrorRecord GetErrorRecordForRemotePipelineInvocation(Exception innerException, string errorMessageTemplate)
            RemoteException remoteException = innerException as RemoteException;
            ErrorRecord remoteErrorRecord = remoteException?.ErrorRecord;
            string errorId = remoteErrorRecord != null ? remoteErrorRecord.FullyQualifiedErrorId : innerException.GetType().Name;
            ErrorCategory errorCategory = remoteErrorRecord != null ? remoteErrorRecord.CategoryInfo.Category : ErrorCategory.NotSpecified;
            ErrorRecord errorRecord = new ErrorRecord(outerException, errorId, errorCategory, null);
        private static ErrorRecord GetErrorRecordForRemotePipelineInvocation(ErrorRecord innerErrorRecord, string errorMessageTemplate)
            string innerErrorMessage;
            if (innerErrorRecord.ErrorDetails != null && innerErrorRecord.ErrorDetails.Message != null)
                innerErrorMessage = innerErrorRecord.ErrorDetails.Message;
            else if (innerErrorRecord.Exception != null && innerErrorRecord.Exception.Message != null)
                innerErrorMessage = innerErrorRecord.Exception.Message;
                innerErrorMessage = innerErrorRecord.ToString();
                innerErrorMessage);
            ErrorRecord outerErrorRecord = new ErrorRecord(innerErrorRecord, null /* null means: do not replace the exception */);
            ErrorDetails outerErrorDetails = new ErrorDetails(errorMessage);
            outerErrorRecord.ErrorDetails = outerErrorDetails;
            return outerErrorRecord;
        private static IEnumerable<T> EnumerateWithCatch<T>(IEnumerable<T> enumerable, Action<Exception> exceptionHandler)
            IEnumerator<T> enumerator = null;
                exceptionHandler(e);
                using (enumerator)
                    bool gotResults = false;
                            gotResults = false;
                            gotResults = enumerator.MoveNext();
                        if (gotResults)
                            T currentItem = default(T);
                            bool gotCurrentItem = false;
                                currentItem = enumerator.Current;
                                gotCurrentItem = true;
                            if (gotCurrentItem)
                                yield return currentItem;
                    } while (gotResults);
        private static void HandleErrorFromPipeline(Cmdlet cmdlet, ErrorRecord errorRecord, PowerShell powerShell)
            if (!cmdlet.MyInvocation.ExpectingInput)
                if (((powerShell.Runspace != null) && (powerShell.Runspace.RunspaceStateInfo.State != RunspaceState.Opened)) ||
                    ((powerShell.RunspacePool != null) && (powerShell.RunspacePool.RunspacePoolStateInfo.State != RunspacePoolState.Opened)))
        internal static IEnumerable<PSObject> InvokePowerShell(
            CopyParameterFromCmdletToPowerShell(cmdlet, powerShell, "ErrorAction");
            CopyParameterFromCmdletToPowerShell(cmdlet, powerShell, "WarningAction");
            CopyParameterFromCmdletToPowerShell(cmdlet, powerShell, "InformationAction");
            CopyParameterFromCmdletToPowerShell(cmdlet, powerShell, "Verbose");
            CopyParameterFromCmdletToPowerShell(cmdlet, powerShell, "Debug");
            var invocationSettings = new PSInvocationSettings { Host = cmdlet.Host };
            // TODO/FIXME: ETW events for the output stream
            IEnumerable<PSObject> outputStream = powerShell.IsNested
                ? InvokeNestedPowerShell(powerShell, cmdlet, invocationSettings, errorMessageTemplate, cancellationToken)
                : InvokeTopLevelPowerShell(powerShell, cmdlet, invocationSettings, errorMessageTemplate, cancellationToken);
            return EnumerateWithCatch(
                outputStream,
                (Exception exception) =>
                    ErrorRecord errorRecord = GetErrorRecordForRemotePipelineInvocation(exception, errorMessageTemplate);
        #endregion PSRP
        #region CIM
        private const string DiscoveryProviderNamespace = "root/Microsoft/Windows/Powershellv3";
        private const string DiscoveryProviderModuleClass = "PS_Module";
        private const string DiscoveryProviderFileClass = "PS_ModuleFile";
        private const string DiscoveryProviderAssociationClass = "PS_ModuleToModuleFile";
        private static T GetPropertyValue<T>(CimInstance cimInstance, string propertyName, T defaultValue)
            object propertyValue = cimProperty.Value;
            if (propertyValue is T)
                return (T)propertyValue;
            if (propertyValue is string)
                string stringValue = (string)propertyValue;
                    if (typeof(T) == typeof(bool))
                        return (T)(object)XmlConvert.ToBoolean(stringValue);
                    else if (typeof(T) == typeof(UInt16))
                        return (T)(object)UInt16.Parse(stringValue, CultureInfo.InvariantCulture);
                    else if (typeof(T) == typeof(byte[]))
                        byte[] contentBytes = Convert.FromBase64String(stringValue);
                        byte[] lengthBytes = BitConverter.GetBytes(contentBytes.Length + 4);
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(lengthBytes);
                        return (T)(object)(lengthBytes.Concat(contentBytes).ToArray());
        internal enum CimFileCode
            PsdV1,
            TypesV1,
            FormatV1,
            CmdletizationV1,
        internal abstract class CimModuleFile
            public CimFileCode FileCode
                    if (this.FileName.EndsWith(".psd1", StringComparison.OrdinalIgnoreCase))
                        return CimFileCode.PsdV1;
                    if (this.FileName.EndsWith(".cdxml", StringComparison.OrdinalIgnoreCase))
                        return CimFileCode.CmdletizationV1;
                    if (this.FileName.EndsWith(".types.ps1xml", StringComparison.OrdinalIgnoreCase))
                        return CimFileCode.TypesV1;
                    if (this.FileName.EndsWith(".format.ps1xml", StringComparison.OrdinalIgnoreCase))
                        return CimFileCode.FormatV1;
                    return CimFileCode.Unknown;
            public abstract string FileName { get; }
            internal abstract byte[] RawFileDataCore { get; }
            public byte[] RawFileData
                get { return this.RawFileDataCore.Skip(4).ToArray(); }
            public string FileData
                    if (_fileData == null)
                        using (var ms = new MemoryStream(this.RawFileData))
                        using (var sr = new StreamReader(ms, detectEncodingFromByteOrderMarks: true))
                            _fileData = sr.ReadToEnd();
                    return _fileData;
            private string _fileData;
        internal class CimModule
            private readonly CimInstance _baseObject;
            internal CimModule(CimInstance baseObject)
                Dbg.Assert(baseObject != null, "Caller should make sure baseObject != null");
                    baseObject.CimSystemProperties.ClassName.Equals(DiscoveryProviderModuleClass, StringComparison.OrdinalIgnoreCase),
                    "Caller should make sure baseObject is an instance of the right CIM class");
                _baseObject = baseObject;
                    var rawModuleName = GetPropertyValue<string>(_baseObject, "ModuleName", string.Empty);
                    return Path.GetFileName(rawModuleName);
            private enum DiscoveredModuleType : ushort
                Cim = 1,
            public bool IsPsCimModule
                    UInt16 moduleTypeInt = GetPropertyValue<UInt16>(_baseObject, "ModuleType", 0);
                    DiscoveredModuleType moduleType = (DiscoveredModuleType)moduleTypeInt;
                    bool isPsCimModule = (moduleType == DiscoveredModuleType.Cim);
                    return isPsCimModule;
            public CimModuleFile MainManifest
                    byte[] rawFileData = GetPropertyValue<byte[]>(_baseObject, "moduleManifestFileData", Array.Empty<byte>());
                    return new CimModuleManifestFile(this.ModuleName + ".psd1", rawFileData);
            public IEnumerable<CimModuleFile> ModuleFiles
                get { return _moduleFiles; }
            internal void FetchAllModuleFiles(CimSession cimSession, string cimNamespace, CimOperationOptions operationOptions)
                IEnumerable<CimInstance> associatedInstances = cimSession.EnumerateAssociatedInstances(
                    _baseObject,
                    DiscoveryProviderAssociationClass,
                    DiscoveryProviderFileClass,
                    "Antecedent",
                    "Dependent",
                IEnumerable<CimModuleFile> associatedFiles = associatedInstances.Select(static i => new CimModuleImplementationFile(i));
                _moduleFiles = associatedFiles.ToList();
            private List<CimModuleFile> _moduleFiles;
            private sealed class CimModuleManifestFile : CimModuleFile
                internal CimModuleManifestFile(string fileName, byte[] rawFileData)
                    Dbg.Assert(fileName != null, "Caller should make sure fileName != null");
                    Dbg.Assert(rawFileData != null, "Caller should make sure rawFileData != null");
                    RawFileDataCore = rawFileData;
                public override string FileName { get; }
                internal override byte[] RawFileDataCore { get; }
            private sealed class CimModuleImplementationFile : CimModuleFile
                internal CimModuleImplementationFile(CimInstance baseObject)
                        baseObject.CimSystemProperties.ClassName.Equals(DiscoveryProviderFileClass, StringComparison.OrdinalIgnoreCase),
                public override string FileName
                        string rawFileName = GetPropertyValue<string>(_baseObject, "FileName", string.Empty);
                        return Path.GetFileName(rawFileName);
                internal override byte[] RawFileDataCore
                    get { return GetPropertyValue<byte[]>(_baseObject, "FileData", Array.Empty<byte>()); }
        internal static IEnumerable<CimModule> GetCimModules(
            IEnumerable<string> moduleNamePatterns,
            bool onlyManifests,
            moduleNamePatterns ??= new[] { "*" };
            HashSet<string> alreadyEmittedNamesOfCimModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            IEnumerable<CimModule> remoteModules = moduleNamePatterns
                .SelectMany(moduleNamePattern =>
                    RemoteDiscoveryHelper.GetCimModules(cimSession, resourceUri, cimNamespace, moduleNamePattern, onlyManifests, cmdlet, cancellationToken));
            foreach (CimModule remoteModule in remoteModules)
                if (!alreadyEmittedNamesOfCimModules.Contains(remoteModule.ModuleName))
                    alreadyEmittedNamesOfCimModules.Add(remoteModule.ModuleName);
                    yield return remoteModule;
        private static IEnumerable<CimModule> GetCimModules(
            string moduleNamePattern,
            Dbg.Assert(cimSession != null, "Caller should verify cimSession != null");
            Dbg.Assert(moduleNamePattern != null, "Caller should verify that moduleNamePattern != null");
            var wildcardPattern = WildcardPattern.Get(moduleNamePattern, wildcardOptions);
            string dosWildcard = WildcardPatternToDosWildcardParser.Parse(wildcardPattern);
            var options = new CimOperationOptions { CancellationToken = cancellationToken };
            options.SetCustomOption("PS_ModuleNamePattern", dosWildcard, mustComply: false);
                options.ResourceUri = resourceUri;
            if (string.IsNullOrEmpty(cimNamespace) && (resourceUri == null))
                cimNamespace = DiscoveryProviderNamespace;
            // TODO/FIXME: ETW for method invocation
            IEnumerable<CimInstance> syncResults = cimSession.EnumerateInstances(
                DiscoveryProviderModuleClass,
            // TODO/FIXME: ETW for method results
            IEnumerable<CimModule> cimModules = syncResults
                .Select(static cimInstance => new CimModule(cimInstance))
                .Where(cimModule => wildcardPattern.IsMatch(cimModule.ModuleName));
            if (!onlyManifests)
                cimModules = cimModules.Select(
                    (CimModule cimModule) =>
                        cimModule.FetchAllModuleFiles(cimSession, cimNamespace, options);
                        return cimModule;
                cimModules,
                    ErrorRecord errorRecord = GetErrorRecordForRemoteDiscoveryProvider(exception);
                        if (errorRecord.FullyQualifiedErrorId.Contains(DiscoveryProviderNotFoundErrorId, StringComparison.OrdinalIgnoreCase)
                            || cancellationToken.IsCancellationRequested
                            || exception is OperationCanceledException
                            || !cimSession.TestConnection())
        internal static Hashtable RewriteManifest(Hashtable originalManifest)
            return RewriteManifest(originalManifest, null, null, null);
        private static readonly string[] s_manifestEntriesToKeepAsString = new[] {
        private static readonly string[] s_manifestEntriesToKeepAsStringArray = new[] {
        internal static Hashtable RewriteManifest(
            Hashtable originalManifest,
            IEnumerable<string> nestedModules,
            IEnumerable<string> typesToProcess,
            IEnumerable<string> formatsToProcess)
            nestedModules ??= Array.Empty<string>();
            typesToProcess ??= Array.Empty<string>();
            formatsToProcess ??= Array.Empty<string>();
            var newManifest = new Hashtable(StringComparer.OrdinalIgnoreCase);
            newManifest["NestedModules"] = nestedModules;
            newManifest["TypesToProcess"] = typesToProcess;
            newManifest["FormatsToProcess"] = formatsToProcess;
            newManifest["PrivateData"] = originalManifest["PrivateData"];
            foreach (DictionaryEntry entry in originalManifest)
                if (s_manifestEntriesToKeepAsString.Contains(entry.Key as string, StringComparer.OrdinalIgnoreCase))
                    var value = (string)LanguagePrimitives.ConvertTo(entry.Value, typeof(string), CultureInfo.InvariantCulture);
                    newManifest[entry.Key] = value;
                else if (s_manifestEntriesToKeepAsStringArray.Contains(entry.Key as string, StringComparer.OrdinalIgnoreCase))
                    var values = (string[])LanguagePrimitives.ConvertTo(entry.Value, typeof(string[]), CultureInfo.InvariantCulture);
                    newManifest[entry.Key] = values;
            return newManifest;
        private static CimCredential GetCimCredentials(PasswordAuthenticationMechanism authenticationMechanism, PSCredential credential)
            NetworkCredential networkCredential = credential.GetNetworkCredential();
            return new CimCredential(authenticationMechanism, networkCredential.Domain, networkCredential.UserName, credential.Password);
        private static Exception GetExceptionWhenAuthenticationRequiresCredential(string authentication)
                RemotingErrorIdStrings.AuthenticationMechanismRequiresCredential,
                authentication);
            throw new ArgumentException(errorMessage);
        private static CimCredential GetCimCredentials(string authentication, PSCredential credential)
            if (authentication == null || (authentication.Equals("Default", StringComparison.OrdinalIgnoreCase)))
                    return GetCimCredentials(PasswordAuthenticationMechanism.Default, credential);
            if (authentication.Equals("Basic", StringComparison.OrdinalIgnoreCase))
                    throw GetExceptionWhenAuthenticationRequiresCredential(authentication);
                    return GetCimCredentials(PasswordAuthenticationMechanism.Basic, credential);
            if (authentication.Equals("Negotiate", StringComparison.OrdinalIgnoreCase))
                    return new CimCredential(ImpersonatedAuthenticationMechanism.Negotiate);
                    return GetCimCredentials(PasswordAuthenticationMechanism.Negotiate, credential);
            if (authentication.Equals("CredSSP", StringComparison.OrdinalIgnoreCase))
                    return GetCimCredentials(PasswordAuthenticationMechanism.CredSsp, credential);
            if (authentication.Equals("Digest", StringComparison.OrdinalIgnoreCase))
                    return GetCimCredentials(PasswordAuthenticationMechanism.Digest, credential);
            if (authentication.Equals("Kerberos", StringComparison.OrdinalIgnoreCase))
                    return new CimCredential(ImpersonatedAuthenticationMechanism.Kerberos);
                    return GetCimCredentials(PasswordAuthenticationMechanism.Kerberos, credential);
            Dbg.Assert(false, "Unrecognized authentication mechanism [ValidateSet should prevent that from happening]");
            throw new ArgumentOutOfRangeException(nameof(authentication));
        internal static CimSession CreateCimSession(
            bool isLocalHost,
            if (isLocalHost)
            var sessionOptions = new CimSessionOptions();
            CimCredential cimCredentials = GetCimCredentials(authentication, credential);
            if (cimCredentials != null)
                sessionOptions.AddDestinationCredentials(cimCredentials);
            CimSession cimSession = CimSession.Create(computerName, sessionOptions);
        internal static Hashtable ConvertCimModuleFileToManifestHashtable(RemoteDiscoveryHelper.CimModuleFile cimModuleFile, string temporaryModuleManifestPath, ModuleCmdletBase cmdlet, ref bool containedErrors)
            Dbg.Assert(cimModuleFile.FileCode == RemoteDiscoveryHelper.CimFileCode.PsdV1, "Caller should verify the file is of the right type");
                System.Management.Automation.Language.Token[] throwAwayTokens;
                scriptBlockAst = System.Management.Automation.Language.Parser.ParseInput(cimModuleFile.FileData, temporaryModuleManifestPath, out throwAwayTokens, out parseErrors);
                if ((scriptBlockAst == null) || (parseErrors != null && parseErrors.Length > 0))
                data = cmdlet.LoadModuleManifestData(
                    ModuleCmdletBase.ModuleManifestMembers,
        #endregion CIM
        #region Protocol/transport agnostic functionality
        internal static string GetModulePath(string remoteModuleName, Version remoteModuleVersion, string computerName, Runspace localRunspace)
            computerName ??= string.Empty;
            string sanitizedRemoteModuleName = Regex.Replace(remoteModuleName, "[^a-zA-Z0-9]", string.Empty);
            string sanitizedComputerName = Regex.Replace(computerName, "[^a-zA-Z0-9]", string.Empty);
            string moduleName = string.Format(
                "remoteIpMoProxy_{0}_{1}_{2}_{3}",
                sanitizedRemoteModuleName.Substring(0, Math.Min(sanitizedRemoteModuleName.Length, 100)),
                sanitizedComputerName.Substring(0, Math.Min(sanitizedComputerName.Length, 100)),
                localRunspace.InstanceId);
            string modulePath = Path.Combine(Path.GetTempPath(), moduleName);
        internal static void AssociatePSModuleInfoWithSession(PSModuleInfo moduleInfo, CimSession cimSession, Uri resourceUri, string cimNamespace)
            AssociatePSModuleInfoWithSession(moduleInfo, (object)new Tuple<CimSession, Uri, string>(cimSession, resourceUri, cimNamespace));
        internal static void AssociatePSModuleInfoWithSession(PSModuleInfo moduleInfo, PSSession psSession)
            AssociatePSModuleInfoWithSession(moduleInfo, (object)psSession);
        private static void AssociatePSModuleInfoWithSession(PSModuleInfo moduleInfo, object weaklyTypedSession)
            s_moduleInfoToSession.Add(moduleInfo, weaklyTypedSession);
        private static readonly ConditionalWeakTable<PSModuleInfo, object> s_moduleInfoToSession = new ConditionalWeakTable<PSModuleInfo, object>();
        internal static void DispatchModuleInfoProcessing(
            Action localAction,
            Action<CimSession, Uri, string> cimSessionAction,
            Action<PSSession> psSessionAction)
            object weaklyTypeSession;
            if (!s_moduleInfoToSession.TryGetValue(moduleInfo, out weaklyTypeSession))
                localAction();
            Tuple<CimSession, Uri, string> cimSessionInfo = weaklyTypeSession as Tuple<CimSession, Uri, string>;
            if (cimSessionInfo != null)
                cimSessionAction(cimSessionInfo.Item1, cimSessionInfo.Item2, cimSessionInfo.Item3);
            PSSession psSession = weaklyTypeSession as PSSession;
            if (psSession != null)
                psSessionAction(psSession);
            Dbg.Assert(false, "PSModuleInfo was associated with an unrecognized session type");

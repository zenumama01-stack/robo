using System.Diagnostics.Tracing;
    /// EventArgs for the ScriptCmdletVariableUpdate event.
    public class CommandLookupEventArgs : EventArgs
        /// Constructor for event args object.
        /// <param name="commandName">The name of the command we're searching for.</param>
        /// <param name="commandOrigin">The origin of the command internal or runspace (external).</param>
        /// <param name="context">The execution context for this command.</param>
        internal CommandLookupEventArgs(string commandName, CommandOrigin commandOrigin, ExecutionContext context)
            CommandName = commandName;
            CommandOrigin = commandOrigin;
        /// The name of the command we're looking for.
        public string CommandName { get; }
        /// The origin of the command internal or runspace (external)
        public CommandOrigin CommandOrigin { get; }
        /// If true on return from event handler, the search is stopped.
        public bool StopSearch { get; set; }
        /// The CommandInfo object for the command that was found.
        public CommandInfo Command { get; set; }
        /// Scriptblock to be returned as the found command. If it is set to
        /// null, then the command to return and the StopSearch flag will be reset.
        public ScriptBlock CommandScriptBlock
                return _scriptBlock;
                _scriptBlock = value;
                if (_scriptBlock != null)
                    string dynamicName = "LookupHandlerReplacementFor<<" + CommandName + ">>";
                    Command = new FunctionInfo(dynamicName, _scriptBlock, _context);
                    StopSearch = true;
                    Command = null;
                    StopSearch = false;
        private ScriptBlock _scriptBlock;
    /// Defines the preference options for the Module Auto-loading feature.
    public enum PSModuleAutoLoadingPreference
        /// Do not auto-load modules when a command is not found.
        /// Only auto-load modules when a command is not found, and the command
        /// is module-qualified.
        ModuleQualified = 1,
        /// Auto-load modules when a command is not found.
        All = 2
    /// CommandDiscovery...
    internal class CommandDiscovery
        [TraceSource("CommandDiscovery", "Traces the discovery of cmdlets, scripts, functions, applications, etc.")]
        internal static readonly PSTraceSource discoveryTracer =
            PSTraceSource.GetTracer(
                "CommandDiscovery",
                "Traces the discovery of cmdlets, scripts, functions, applications, etc.",
        /// Default constructor...
        internal CommandDiscovery(ExecutionContext context)
            Context = context;
            discoveryTracer.ShowHeaders = false;
        /// Determines if the cmdlet is a cmdlet that shouldn't be in the discovery list.
        /// Type implementing the cmdlet
        /// True if the cmdlet is a special cmdlet that shouldn't be part of the discovery list. Or false otherwise.
        private static bool IsSpecialCmdlet(Type implementingType)
            // These commands should never be put in the discovery list.  They are an internal implementation
            // detail of the formatting and output component. That component uses these cmdlets by creating
            // an instance of the CommandProcessor class directly.
            return implementingType == typeof(OutLineOutputCommand) || implementingType == typeof(FormatDefaultCommand);
        private CmdletInfo NewCmdletInfo(SessionStateCmdletEntry entry)
            return NewCmdletInfo(entry, Context);
        internal static CmdletInfo NewCmdletInfo(SessionStateCmdletEntry entry, ExecutionContext context)
            CmdletInfo ci = new CmdletInfo(entry.Name, entry.ImplementingType, entry.HelpFileName, entry.PSSnapIn, context)
                Visibility = entry.Visibility,
                Module = entry.Module
            return ci;
        internal static AliasInfo NewAliasInfo(SessionStateAliasEntry entry, ExecutionContext context)
            AliasInfo ci = new AliasInfo(entry.Name, entry.Definition, context, entry.Options)
        /// Adds the CmdletInfo to the cmdlet cache in the current scope object.
        /// The name of the cmdlet to add.
        /// <param name="newCmdletInfo">
        /// The CmdletInfo to add.
        /// <param name="isGlobal">
        /// If true, the cmdlet is added to the Module Scope of the session state.
        /// <exception cref="PSNotSupportedException">
        /// If a cmdlet with the same module and cmdlet name already exists
        /// but has a different implementing type.
        internal CmdletInfo AddCmdletInfoToCache(string name, CmdletInfo newCmdletInfo, bool isGlobal)
            if (newCmdletInfo == null)
                throw PSTraceSource.NewArgumentNullException("cmdlet");
            if (isGlobal)
                // When cmdlet cache was not scope-based, we used to import cmdlets to the module scope.
                // We need to do the same as the default action (setting "isGlobal" is done as a default action in the caller)
                return Context.EngineSessionState.ModuleScope.AddCmdletToCache(newCmdletInfo.Name, newCmdletInfo, CommandOrigin.Internal, Context);
            return Context.EngineSessionState.CurrentScope.AddCmdletToCache(newCmdletInfo.Name, newCmdletInfo, CommandOrigin.Internal, Context);
        /// Add a SessionStateCmdletEntry to the cmdlet cache...
        internal void AddSessionStateCmdletEntryToCache(SessionStateCmdletEntry entry)
            AddSessionStateCmdletEntryToCache(entry, /*local*/false);
        /// <param name="local"></param>
        internal void AddSessionStateCmdletEntryToCache(SessionStateCmdletEntry entry, bool local)
            if (!IsSpecialCmdlet(entry.ImplementingType))
                CmdletInfo nci = NewCmdletInfo(entry);
                AddCmdletInfoToCache(nci.Name, nci, !local);
        /// Look up a command named by the argument string and return its CommandProcessorBase.
        /// <param name="commandName">
        /// The command name to lookup.
        /// <param name="commandOrigin">Location where the command was dispatched from.</param>
        /// <param name="useLocalScope">
        /// True if command processor should use local scope to execute the command,
        /// False if not.  Null if command discovery should default to something reasonable
        /// for the command discovered.
        /// <param name="forCompletion">
        /// True if this for parameter completion and script requirements should be ignored.
        /// <exception cref="CommandNotFoundException">
        /// If the command, <paramref name="commandName"/>, could not be found.
        /// If the security manager is preventing the command from running.
        internal CommandProcessorBase LookupCommandProcessor(string commandName,
            CommandOrigin commandOrigin, bool? useLocalScope, bool forCompletion = false)
            CommandProcessorBase processor = null;
            CommandInfo commandInfo = LookupCommandInfo(commandName, commandOrigin);
            if (commandInfo != null)
                processor = LookupCommandProcessor(commandInfo, commandOrigin, useLocalScope, null, forCompletion);
                // commandInfo.Name might be different than commandName - restore the original invocation name
                processor.Command.MyInvocation.InvocationName = commandName;
            return processor;
        internal static void VerifyRequiredModules(ExternalScriptInfo scriptInfo, ExecutionContext context, bool forCompletion = false)
            // Check Required Modules
            if (scriptInfo.RequiresModules != null)
                foreach (var requiredModule in scriptInfo.RequiresModules)
                    ModuleCmdletBase.LoadRequiredModule(
                        context: context,
                        currentModule: null,
                        requiredModuleSpecification: requiredModule,
                        moduleManifestPath: null,
                        manifestProcessingFlags: ModuleCmdletBase.ManifestProcessingFlags.LoadElements | ModuleCmdletBase.ManifestProcessingFlags.WriteErrors,
                        error: out error);
                    if (!forCompletion && error is not null)
                        ScriptRequiresException scriptRequiresException =
                            new ScriptRequiresException(
                                scriptInfo.Name,
                                new Collection<string> { requiredModule.GetRequiredModuleNotFoundVersionMessage() },
                                "ScriptRequiresMissingModules",
                        throw scriptRequiresException;
        private CommandProcessorBase CreateScriptProcessorForSingleShell(ExternalScriptInfo scriptInfo, ExecutionContext context, bool useLocalScope, SessionStateInternal sessionState, bool forCompletion = false)
            VerifyScriptRequirements(scriptInfo, Context, forCompletion);
            if (!string.IsNullOrEmpty(scriptInfo.RequiresApplicationID))
                ScriptRequiresException sre =
                        "RequiresShellIDInvalidForSingleShell");
                throw sre;
            return CreateCommandProcessorForScript(scriptInfo, Context, useLocalScope, sessionState);
        // This method verifies the following 3 elements of #Requires statement
        // #Requires -RunAsAdministrator
        // #Requires -PSVersion
        // #Requires -PSEdition
        // #Requires -Module
        internal static void VerifyScriptRequirements(ExternalScriptInfo scriptInfo, ExecutionContext context, bool forCompletion = false)
            // When completing script parameters we don't care if these requirements are met.
            // VerifyRequiredModules will attempt to load the required modules which is useful for completion (so the correct types are loaded).
            if (!forCompletion)
                VerifyElevatedPrivileges(scriptInfo);
                VerifyPSVersion(scriptInfo);
                VerifyPSEdition(scriptInfo);
            VerifyRequiredModules(scriptInfo, context, forCompletion);
        internal static void VerifyPSVersion(ExternalScriptInfo scriptInfo)
            Version requiresPSVersion = scriptInfo.RequiresPSVersion;
            // in single shell mode
            if (requiresPSVersion != null)
                if (!PSVersionInfo.IsValidPSVersion(requiresPSVersion))
                            requiresPSVersion,
                            PSVersionInfo.PSVersion.ToString(),
                            "ScriptRequiresUnmatchedPSVersion");
        internal static void VerifyPSEdition(ExternalScriptInfo scriptInfo)
            if (scriptInfo.RequiresPSEditions != null)
                var isCurrentEditionListed = false;
                var isRequiresPSEditionSpecified = false;
                foreach (var edition in scriptInfo.RequiresPSEditions)
                    isRequiresPSEditionSpecified = true;
                    isCurrentEditionListed = Utils.IsPSEditionSupported(edition);
                    if (isCurrentEditionListed)
                // Throw an error if required PowerShell editions are specified and without the current PowerShell Edition.
                if (isRequiresPSEditionSpecified && !isCurrentEditionListed)
                    var specifiedEditionsString = string.Join(',', scriptInfo.RequiresPSEditions);
                    var message = StringUtil.Format(DiscoveryExceptions.RequiresPSEditionNotCompatible,
                        specifiedEditionsString,
                        PSVersionInfo.PSEditionValue);
                    var ex = new RuntimeException(message);
                    ex.SetErrorId("ScriptRequiresUnmatchedPSEdition");
                    ex.SetTargetObject(scriptInfo.Name);
        internal static void VerifyElevatedPrivileges(ExternalScriptInfo scriptInfo)
            bool requiresElevation = scriptInfo.RequiresElevation;
            bool isAdministrator = Utils.IsAdministrator();
            if (requiresElevation && !isAdministrator)
                            "ScriptRequiresElevation");
        /// Look up a command using a CommandInfo object and return its CommandProcessorBase.
        /// <param name="commandInfo">
        /// The commandInfo for the command to lookup.
        /// <param name="sessionState">The session state the commandInfo should be run in.</param>
        /// <exception cref="System.Management.Automation.PSSecurityException">
        internal CommandProcessorBase LookupCommandProcessor(CommandInfo commandInfo,
            CommandOrigin commandOrigin, bool? useLocalScope, SessionStateInternal sessionState, bool forCompletion = false)
            HashSet<string> processedAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            while (commandInfo.CommandType == CommandTypes.Alias &&
                (!processedAliases.Contains(commandInfo.Name)) &&
                (commandOrigin == CommandOrigin.Internal || commandInfo.Visibility == SessionStateEntryVisibility.Public))
                processedAliases.Add(commandInfo.Name);
                AliasInfo aliasCommandInfo = (AliasInfo)commandInfo;
                commandInfo = aliasCommandInfo.ResolvedCommand ??
                              LookupCommandInfo(aliasCommandInfo.Definition, commandOrigin, Context);
                // If we didn't have the alias target already resolved, see if it can be loaded.
                if (commandInfo == null)
                    CommandNotFoundException e =
                        new CommandNotFoundException(
                            aliasCommandInfo.Name,
                            "AliasNotResolvedException",
                            DiscoveryExceptions.AliasNotResolvedException,
                            aliasCommandInfo.UnresolvedCommandName);
            ShouldRun(Context, Context.EngineHostInterface, commandInfo, commandOrigin);
            switch (commandInfo.CommandType)
                    processor = new NativeCommandProcessor((ApplicationInfo)commandInfo, Context);
                    processor = new CommandProcessor((CmdletInfo)commandInfo, Context);
                    ExternalScriptInfo scriptInfo = (ExternalScriptInfo)commandInfo;
                    scriptInfo.SignatureChecked = true;
                        processor = CreateScriptProcessorForSingleShell(scriptInfo, Context, useLocalScope ?? true, sessionState, forCompletion);
                    catch (ScriptRequiresSyntaxException reqSyntaxException)
                            new CommandNotFoundException(reqSyntaxException.Message, reqSyntaxException);
                case CommandTypes.Configuration:
                    FunctionInfo functionInfo = (FunctionInfo)commandInfo;
                    processor = CreateCommandProcessorForScript(functionInfo, Context, useLocalScope ?? true, sessionState);
                    processor = CreateCommandProcessorForScript((ScriptInfo)commandInfo, Context, useLocalScope ?? true, sessionState);
                                commandInfo.Name,
                                "CommandNotFoundException",
                                DiscoveryExceptions.CommandNotFoundException);
            // Set the internal command origin member on the command object at this point...
            processor.Command.CommandOriginInternal = commandOrigin;
            processor.Command.MyInvocation.InvocationName = commandInfo.Name;
        internal static void ShouldRun(ExecutionContext context, PSHost host, CommandInfo commandInfo, CommandOrigin commandOrigin)
            // ShouldRunInternal throws PSSecurityException if run is not allowed
                if (commandOrigin == CommandOrigin.Runspace && commandInfo.Visibility != SessionStateEntryVisibility.Public)
                    CommandNotFoundException e = new CommandNotFoundException(
                        commandInfo.Name, null, "CommandNotFoundException", DiscoveryExceptions.CommandNotFoundException);
                context.AuthorizationManager.ShouldRunInternal(commandInfo, commandOrigin, host);
                MshLog.LogCommandHealthEvent(context,
                                reason,
                MshLog.LogCommandLifecycleEvent(context,
                                CommandState.Terminated,
                                commandInfo.Name);
        private static CommandProcessorBase CreateCommandProcessorForScript(ScriptInfo scriptInfo, ExecutionContext context, bool useNewScope, SessionStateInternal sessionState)
            sessionState ??= scriptInfo.ScriptBlock.SessionStateInternal ?? context.EngineSessionState;
            CommandProcessorBase scriptAsCmdletProcessor = GetScriptAsCmdletProcessor(scriptInfo, context, useNewScope, true, sessionState);
            if (scriptAsCmdletProcessor != null)
                return scriptAsCmdletProcessor;
            return new DlrScriptCommandProcessor(scriptInfo, context, useNewScope, sessionState);
        private static CommandProcessorBase CreateCommandProcessorForScript(ExternalScriptInfo scriptInfo, ExecutionContext context, bool useNewScope, SessionStateInternal sessionState)
        internal static CommandProcessorBase CreateCommandProcessorForScript(FunctionInfo functionInfo, ExecutionContext context, bool useNewScope, SessionStateInternal sessionState)
            sessionState ??= functionInfo.ScriptBlock.SessionStateInternal ?? context.EngineSessionState;
            CommandProcessorBase scriptAsCmdletProcessor = GetScriptAsCmdletProcessor(functionInfo, context, useNewScope, false, sessionState);
            return new DlrScriptCommandProcessor(functionInfo, context, useNewScope, sessionState);
        internal static CommandProcessorBase CreateCommandProcessorForScript(ScriptBlock scriptblock, ExecutionContext context, bool useNewScope, SessionStateInternal sessionState)
            sessionState ??= scriptblock.SessionStateInternal ?? context.EngineSessionState;
            if (scriptblock.UsesCmdletBinding)
                FunctionInfo fi = new FunctionInfo(string.Empty, scriptblock, context);
                return GetScriptAsCmdletProcessor(fi, context, useNewScope, false, sessionState);
            return new DlrScriptCommandProcessor(scriptblock, context, useNewScope, CommandOrigin.Internal, sessionState);
        private static CommandProcessorBase GetScriptAsCmdletProcessor(IScriptCommandInfo scriptCommandInfo, ExecutionContext context, bool useNewScope, bool fromScriptFile, SessionStateInternal sessionState)
            if (scriptCommandInfo.ScriptBlock == null || !scriptCommandInfo.ScriptBlock.UsesCmdletBinding)
            sessionState ??= scriptCommandInfo.ScriptBlock.SessionStateInternal ?? context.EngineSessionState;
            return new CommandProcessor(scriptCommandInfo, context, useNewScope, fromScriptFile, sessionState);
        /// Look up a command and return its CommandInfo.
        /// An instance of a CommandInfo object that represents the
        /// command. If the command is resolved as an alias, an AliasInfo
        /// is returned with the ReferencedCommand info intact.
        internal CommandInfo LookupCommandInfo(string commandName)
            return LookupCommandInfo(commandName, CommandOrigin.Internal);
        internal CommandInfo LookupCommandInfo(string commandName, CommandOrigin commandOrigin)
            return LookupCommandInfo(commandName, commandOrigin, Context);
        internal static CommandInfo LookupCommandInfo(string commandName, CommandOrigin commandOrigin, ExecutionContext context)
            return LookupCommandInfo(commandName, CommandTypes.All, SearchResolutionOptions.ResolveLiteralThenPathPatterns, commandOrigin, context);
        internal static CommandInfo LookupCommandInfo(
            CommandTypes commandTypes,
            SearchResolutionOptions searchResolutionOptions,
            CommandOrigin commandOrigin,
            bool etwEnabled = CommandDiscoveryEventSource.Log.IsEnabled();
            if (etwEnabled) CommandDiscoveryEventSource.Log.CommandLookupStart(commandName);
            string originalCommandName = commandName;
            Exception lastError = null;
            // Check to see if there is a pre-search look-up event handler...
            CommandLookupEventArgs eventArgs = null;
            EventHandler<CommandLookupEventArgs> preCommandLookupEvent = context.EngineIntrinsics.InvokeCommand.PreCommandLookupAction;
            if (preCommandLookupEvent != null)
                discoveryTracer.WriteLine("Executing PreCommandLookupAction: {0}", commandName);
                    context.CommandDiscovery.RegisterLookupCommandInfoAction("ActivePreLookup", originalCommandName);
                    eventArgs = new CommandLookupEventArgs(originalCommandName, commandOrigin, context);
                    preCommandLookupEvent.Invoke(originalCommandName, eventArgs);
                    discoveryTracer.WriteLine("PreCommandLookupAction returned: {0}", eventArgs.Command);
                finally { context.CommandDiscovery.UnregisterLookupCommandInfoAction("ActivePreLookup", commandName); }
            // Check the module auto-loading preference
            PSModuleAutoLoadingPreference moduleAutoLoadingPreference = GetCommandDiscoveryPreference(context, SpecialVariables.PSModuleAutoLoadingPreferenceVarPath, "PSModuleAutoLoadingPreference");
            if (eventArgs == null || !eventArgs.StopSearch)
                    discoveryTracer.WriteLine("Looking up command: {0}", commandName);
                    // Use the CommandSearcher to find the first command. If there are duplicate
                    // command names, then take the first one...
                    result = TryNormalSearch(commandName, context, commandOrigin, searchResolutionOptions, commandTypes, ref lastError);
                    // Try the module-qualified auto-loading (unless module auto-loading has been entirely disabled)
                    if (moduleAutoLoadingPreference != PSModuleAutoLoadingPreference.None)
                        result = TryModuleAutoLoading(commandName, context, originalCommandName, commandOrigin, ref lastError);
                    // See if the this was not module-qualified. In that case, we should look for the first module
                    // that contains the command and load that.
                    if (moduleAutoLoadingPreference == PSModuleAutoLoadingPreference.All)
                        result = TryModuleAutoDiscovery(commandName, context, originalCommandName, commandOrigin,
                                                        searchResolutionOptions, commandTypes, ref lastError);
                    // Otherwise, invoke the CommandNotFound handler
                    result ??= InvokeCommandNotFoundHandler(commandName, context, originalCommandName, commandOrigin);
                if (eventArgs.Command != null)
                    result = eventArgs.Command;
            // If we resolved a command, give the PostCommandLookup a chance to change it
                System.EventHandler<CommandLookupEventArgs> postAction = context.EngineIntrinsics.InvokeCommand.PostCommandLookupAction;
                if (postAction != null)
                    discoveryTracer.WriteLine("Executing PostCommandLookupAction: {0}", originalCommandName);
                        context.CommandDiscovery.RegisterLookupCommandInfoAction("ActivePostCommand", originalCommandName);
                        eventArgs.Command = result;
                        postAction.Invoke(originalCommandName, eventArgs);
                        if (eventArgs != null)
                        context.CommandDiscovery.UnregisterLookupCommandInfoAction("ActivePostCommand", originalCommandName);
            // After all command lookup is done, throw a CommandNotFound if we have no result.
                discoveryTracer.TraceError(
                    "'{0}' is not recognized as a cmdlet, function, executable program or script file.",
                    commandName);
                        originalCommandName,
                        lastError,
            if (etwEnabled) CommandDiscoveryEventSource.Log.CommandLookupStop(commandName);
        internal static void AutoloadModulesWithJobSourceAdapters(System.Management.Automation.ExecutionContext context, CommandOrigin commandOrigin)
            /* This function is used by *-Job cmdlets (JobCmdletBase.BeginProcessing(), StartJobCommand.BeginProcessing())
            It attempts to load modules from a fixed ModulesWithJobSourceAdapters list that currently has only `PSScheduledJob` module that is not PS-Core compatible.
            Because this function does not check the result of a (currently failing) `PSScheduledJob` module autoload, it provides no value.
            After discussion it was decided to comment out this code as it may be useful if ModulesWithJobSourceAdapters list changes in the future.
            if (!context.IsModuleWithJobSourceAdapterLoaded)
                    CmdletInfo cmdletInfo = context.SessionState.InvokeCommand.GetCmdlet("Microsoft.PowerShell.Core\\Import-Module");
                    if ((commandOrigin == CommandOrigin.Internal) ||
                        ((cmdletInfo != null) && (cmdletInfo.Visibility == SessionStateEntryVisibility.Public)))
                        foreach (var module in System.Management.Automation.ExecutionContext.ModulesWithJobSourceAdapters)
                            List<PSModuleInfo> existingModule = context.Modules.GetModules(new string[] { module }, false);
                            if (existingModule == null || existingModule.Count == 0)
                                Exception unUsedException = null;
                                AutoloadSpecifiedModule(module, context, cmdletInfo.Visibility, out unUsedException);
                        context.IsModuleWithJobSourceAdapterLoaded = true;
        internal static Collection<PSModuleInfo> AutoloadSpecifiedModule(string moduleName, ExecutionContext context, SessionStateEntryVisibility visibility, out Exception exception)
            exception = null;
            Collection<PSModuleInfo> matchingModules = null;
            CommandInfo commandInfo = new CmdletInfo("Import-Module", typeof(ImportModuleCommand), null, null, context);
            commandInfo.Visibility = visibility;
            Command importModuleCommand = new Command(commandInfo);
            discoveryTracer.WriteLine("Attempting to load module: {0}", moduleName);
            PowerShell ps = null;
                ps = PowerShell.Create(RunspaceMode.CurrentRunspace)
                     .AddParameter("PassThru")
                matchingModules = (Collection<PSModuleInfo>)ps.Invoke<PSModuleInfo>();
                discoveryTracer.WriteLine("Encountered error importing module: {0}", e.Message);
                // Call-out to user code, catch-all OK
            return matchingModules;
        private static CommandInfo InvokeCommandNotFoundHandler(string commandName, ExecutionContext context, string originalCommandName, CommandOrigin commandOrigin)
            CommandLookupEventArgs eventArgs;
            System.EventHandler<CommandLookupEventArgs> cmdNotFoundHandler = context.EngineIntrinsics.InvokeCommand.CommandNotFoundAction;
            if (cmdNotFoundHandler != null)
                discoveryTracer.WriteLine("Executing CommandNotFoundAction: {0}", commandName);
                    context.CommandDiscovery.RegisterLookupCommandInfoAction("ActiveCommandNotFound", originalCommandName);
                    cmdNotFoundHandler.Invoke(originalCommandName, eventArgs);
                finally { context.CommandDiscovery.UnregisterLookupCommandInfoAction("ActiveCommandNotFound", originalCommandName); }
        private static CommandInfo TryNormalSearch(string commandName,
                                                   ref Exception lastError)
            CommandSearcher searcher =
                    commandName,
                    searchResolutionOptions,
                    commandTypes,
            searcher.CommandOrigin = commandOrigin;
                if (!searcher.MoveNext())
                    if (!commandName.Contains('-') && !commandName.Contains('\\'))
                        discoveryTracer.WriteLine(
                            "The command [{0}] was not found, trying again with get- prepended",
                        commandName = StringLiterals.DefaultCommandVerb + StringLiterals.CommandVerbNounSeparator + commandName;
                            result = LookupCommandInfo(commandName, commandTypes, searchResolutionOptions, commandOrigin, context);
                        catch (CommandNotFoundException) { }
                    result = ((IEnumerator<CommandInfo>)searcher).Current;
                lastError = argException;
            catch (PathTooLongException pathTooLong)
                lastError = pathTooLong;
            catch (FileLoadException fileLoadException)
                lastError = fileLoadException;
            catch (FormatException formatException)
                lastError = formatException;
            catch (MetadataException metadataException)
                lastError = metadataException;
        private static CommandInfo TryModuleAutoDiscovery(string commandName,
                                                          string originalCommandName,
            if (etwEnabled) CommandDiscoveryEventSource.Log.ModuleAutoDiscoveryStart(commandName);
                // If commandName had a slash, it was module-qualified or path-qualified.
                // In that case, we should not return anything (module-qualified is handled
                // by the previous call to TryModuleAutoLoading().
                int colonOrBackslash = commandName.AsSpan().IndexOfAny('\\', ':');
                if (colonOrBackslash != -1)
                CmdletInfo cmdletInfo = context.SessionState.InvokeCommand.GetCmdlet("Microsoft.PowerShell.Core\\Get-Module");
                if (commandOrigin == CommandOrigin.Internal || cmdletInfo?.Visibility == SessionStateEntryVisibility.Public)
                    // Search for a module with a matching command, as long as the user would have the ability to
                    // import the module.
                    cmdletInfo = context.SessionState.InvokeCommand.GetCmdlet("Microsoft.PowerShell.Core\\Import-Module");
                        discoveryTracer.WriteLine("Executing non module-qualified search: {0}", commandName);
                        context.CommandDiscovery.RegisterLookupCommandInfoAction("ActiveModuleSearch", commandName);
                        // Get the available module files, preferring modules from $PSHOME so that user modules don't
                        // override system modules during auto-loading
                        if (etwEnabled) CommandDiscoveryEventSource.Log.SearchingForModuleFilesStart();
                        var defaultAvailableModuleFiles = ModuleUtils.GetDefaultAvailableModuleFiles(isForAutoDiscovery: true, context);
                        if (etwEnabled) CommandDiscoveryEventSource.Log.SearchingForModuleFilesStop();
                        foreach (string modulePath in defaultAvailableModuleFiles)
                            // WinBlue:69141 - We need to get the full path here because the module path might be C:\Users\User1\DOCUME~1
                            // While the exportedCommands are cached, they are cached with the full path
                            string expandedModulePath = Path.GetFullPath(modulePath);
                            string moduleShortName = Path.GetFileNameWithoutExtension(expandedModulePath);
                            var exportedCommands = AnalysisCache.GetExportedCommands(expandedModulePath, false, context);
                            if (exportedCommands == null) { continue; }
                            // Skip if module only has class or other types and no commands.
                            if (exportedCommands.TryGetValue(commandName, out CommandTypes exportedCommandTypes))
                                discoveryTracer.WriteLine("Found in module: {0}", expandedModulePath);
                                Collection<PSModuleInfo> matchingModule = AutoloadSpecifiedModule(
                                    expandedModulePath,
                                    cmdletInfo != null ? cmdletInfo.Visibility : SessionStateEntryVisibility.Private,
                                    out lastError);
                                if (matchingModule is null || matchingModule.Count == 0)
                                    string errorMessage = lastError is null
                                        ? StringUtil.Format(DiscoveryExceptions.CouldNotAutoImportMatchingModule, commandName, moduleShortName)
                                        : StringUtil.Format(DiscoveryExceptions.CouldNotAutoImportMatchingModuleWithErrorMessage, commandName, moduleShortName, lastError.Message);
                                    throw new CommandNotFoundException(
                                        "CouldNotAutoloadMatchingModule",
                        // Close the progress pane that may have popped up from analyzing UNC paths.
                        if (context.CurrentCommandProcessor != null)
                            ProgressRecord analysisProgress = new ProgressRecord(0, Modules.ScriptAnalysisPreparing, " ");
                            analysisProgress.RecordType = ProgressRecordType.Completed;
                            context.CurrentCommandProcessor.CommandRuntime.WriteProgress(analysisProgress);
            catch (CommandNotFoundException) { throw; }
                context.CommandDiscovery.UnregisterLookupCommandInfoAction("ActiveModuleSearch", commandName);
            if (etwEnabled) CommandDiscoveryEventSource.Log.ModuleAutoDiscoveryStop(commandName);
        private static CommandInfo TryModuleAutoLoading(string commandName, ExecutionContext context, string originalCommandName, CommandOrigin commandOrigin, ref Exception lastError)
            // If commandName was module-qualified. In that case, we should load the module.
            var colonOrBackslash = commandName.AsSpan().IndexOfAny('\\', ':');
            // If we don't see '\', there is no module specified, so no module to load.
            // If we see ':' before '\', then we probably have a drive qualified path, not a module name
            if (colonOrBackslash == -1 || commandName[colonOrBackslash] == ':')
            string moduleCommandName = commandName.Substring(colonOrBackslash + 1, commandName.Length - colonOrBackslash - 1);
            string moduleName;
            // Now we check if there exists the second '\'
            var secondBackslash = moduleCommandName.IndexOf('\\');
            if (secondBackslash == -1)
                moduleName = commandName.Substring(0, colonOrBackslash);
                string versionString = moduleCommandName.Substring(0, secondBackslash);
                // The second '\' could be version specified. eg: "Microsoft.PowerShell.Archive\1.0.0.0\Compress-Archive", we need to support this scenario
                Version version;
                if (Version.TryParse(versionString, out version))
                    moduleCommandName = moduleCommandName.Substring(secondBackslash + 1, moduleCommandName.Length - secondBackslash - 1);
                    moduleName = commandName.Substring(0, colonOrBackslash) + "\\" + versionString + "\\" + commandName.Substring(0, colonOrBackslash) + ".psd1";
            if (string.IsNullOrEmpty(moduleName) || string.IsNullOrEmpty(moduleCommandName) || moduleName.EndsWith('.'))
            if (etwEnabled) CommandDiscoveryEventSource.Log.ModuleAutoLoadingStart(commandName);
                discoveryTracer.WriteLine("Executing module-qualified search: {0}", commandName);
                // Verify that auto-loading is only done on for internal commands if it's not public
                    List<PSModuleInfo> existingModule = context.Modules.GetModules(new string[] { moduleName }, false);
                    PSModuleInfo discoveredModule = null;
                        Collection<PSModuleInfo> importedModule = AutoloadSpecifiedModule(moduleName, context, cmdletInfo.Visibility, out exception);
                        lastError = exception;
                        if ((importedModule == null) || (importedModule.Count == 0))
                            string error = StringUtil.Format(DiscoveryExceptions.CouldNotAutoImportModule, moduleName);
                            CommandNotFoundException commandNotFound = new CommandNotFoundException(
                                "CouldNotAutoLoadModule",
                            throw commandNotFound;
                        discoveredModule = importedModule[0];
                        discoveredModule = existingModule[0];
                    CommandInfo exportedResult;
                    if (discoveredModule.ExportedCommands.TryGetValue(moduleCommandName, out exportedResult))
                        // Return the command if we found a module
                        result = exportedResult;
            finally { context.CommandDiscovery.UnregisterLookupCommandInfoAction("ActiveModuleSearch", commandName); }
            if (etwEnabled) CommandDiscoveryEventSource.Log.ModuleAutoLoadingStop(commandName);
        internal void RegisterLookupCommandInfoAction(string currentAction, string command)
            HashSet<string> currentActionSet = null;
            switch (currentAction)
                case "ActivePreLookup": currentActionSet = _activePreLookup; break;
                case "ActiveModuleSearch": currentActionSet = _activeModuleSearch; break;
                case "ActiveCommandNotFound": currentActionSet = _activeCommandNotFound; break;
                case "ActivePostCommand": currentActionSet = _activePostCommand; break;
            if (!currentActionSet.Add(command))
        internal void UnregisterLookupCommandInfoAction(string currentAction, string command)
            currentActionSet.Remove(command);
        private readonly HashSet<string> _activePreLookup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _activeModuleSearch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _activeCommandNotFound = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _activePostCommand = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        /// Gets the resolved paths contained in the PATH environment
        /// variable.
        /// The contents of the PATH environment variable split on System.IO.Path.PathSeparator.
        /// The result is an ordered list of paths with paths starting with "." unresolved until lookup time.
        internal LookupPathCollection GetLookupDirectoryPaths()
            LookupPathCollection result = new LookupPathCollection();
                "PATH: {0}",
            bool isPathCacheValid =
                path != null &&
                string.Equals(_pathCacheKey, path, StringComparison.OrdinalIgnoreCase) &&
                _cachedPath != null;
            if (!isPathCacheValid)
                // Reset the cached lookup paths
                _cachedLookupPaths = null;
                // Tokenize the path and cache it
                _pathCacheKey = path;
                if (_pathCacheKey != null)
                    string[] tokenizedPath = _pathCacheKey.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
                    _cachedPath = new Collection<string>();
                    foreach (string directory in tokenizedPath)
                        string tempDir = directory.TrimStart();
                        if (tempDir.EqualsOrdinalIgnoreCase("~"))
                            tempDir = Environment.GetFolderPath(
                                Environment.SpecialFolder.UserProfile,
                                Environment.SpecialFolderOption.DoNotVerify);
                        else if (tempDir.StartsWith("~" + Path.DirectorySeparatorChar))
                                Environment.SpecialFolderOption.DoNotVerify)
                                + Path.DirectorySeparatorChar
                                + tempDir.Substring(2);
                        _cachedPath.Add(tempDir);
                        result.Add(tempDir);
                result.AddRange(_cachedPath);
            // Cache the new lookup paths
            return _cachedLookupPaths ??= result;
        /// The cached list of lookup paths. It can be invalidated by
        /// the PATH changing.
        private LookupPathCollection _cachedLookupPaths;
        /// The key that determines if the cached PATH can be used.
        private string _pathCacheKey;
        /// The cache of the tokenized PATH directories.
        private Collection<string> _cachedPath;
        #endregion internal members
        #region environment variable helpers
        /// Gets the PATHEXT environment variable extensions and tokenizes them.
        internal static string[] PathExtensionsWithPs1Prepended
                var pathExt = Environment.GetEnvironmentVariable("PATHEXT");
                if (!string.Equals(pathExt, s_pathExtCacheKey, StringComparison.OrdinalIgnoreCase) ||
                    s_cachedPathExtCollection == null)
                    InitPathExtCache(pathExt);
                return s_cachedPathExtCollectionWithPs1;
        internal static string[] PathExtensions
                return s_cachedPathExtCollection;
        private static void InitPathExtCache(string pathExt)
            lock (s_lockObject)
                s_cachedPathExtCollection = pathExt != null
                    ? pathExt.ToLower().Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                    : Array.Empty<string>();
                s_cachedPathExtCollectionWithPs1 = new string[s_cachedPathExtCollection.Length + 1];
                s_cachedPathExtCollectionWithPs1[0] = StringLiterals.PowerShellScriptFileExtension;
                Array.Copy(s_cachedPathExtCollection, 0, s_cachedPathExtCollectionWithPs1, 1, s_cachedPathExtCollection.Length);
                s_pathExtCacheKey = pathExt;
        #endregion environment variable helpers
        private static readonly object s_lockObject = new object();
        private static string s_pathExtCacheKey;
        private static string[] s_cachedPathExtCollection;
        private static string[] s_cachedPathExtCollectionWithPs1;
        /// Gets the cmdlet information for the specified name.
        /// <param name="cmdletName">
        /// The name of the cmdlet to return the information for.
        /// <param name="searchAllScopes">
        /// True if we should search all scopes, false if we should stop after finding the first.
        /// The CmdletInfo for the cmdlet for all the cmdlets with the specified name.
        /// If <paramref name="cmdletName"/> is null or empty.
        internal IEnumerator<CmdletInfo> GetCmdletInfo(string cmdletName, bool searchAllScopes)
            Dbg.Assert(!string.IsNullOrEmpty(cmdletName), "Caller should verify the cmdletName");
            PSSnapinQualifiedName commandName = PSSnapinQualifiedName.GetInstance(cmdletName);
            if (commandName == null)
                yield break;
            // Check the current cmdlet cache then check the top level
            // if we aren't already at the top level.
            SessionStateScopeEnumerator scopeEnumerator =
                new SessionStateScopeEnumerator(Context.EngineSessionState.CurrentScope);
            foreach (SessionStateScope scope in scopeEnumerator)
                List<CmdletInfo> cmdlets;
                if (!scope.CmdletTable.TryGetValue(commandName.ShortName, out cmdlets))
                foreach (var cmdletInfo in cmdlets)
                    if (!string.IsNullOrEmpty(commandName.PSSnapInName))
                        if (string.Equals(cmdletInfo.ModuleName, commandName.PSSnapInName, StringComparison.OrdinalIgnoreCase))
                            yield return cmdletInfo;
                            if (!searchAllScopes)
                        // The engine cmdlets get imported (via Import-Module) once when PowerShell starts and the cmdletInfo is added to PSSnapinHelpers._cmdletcache(static) with ModuleName
                        // as "System.Management.Automation.dll" instead of the actual snapin name. The next time we load something in an InitialSessionState, we look at this _cmdletcache and
                        // if the assembly is already loaded, we just return the cmdlets back. So, the CmdletInfo has moduleName has "System.Management.Automation.dll". So, when M3P Activity
                        // tries to access Microsoft.PowerShell.Core\\Get-Command, it cannot. So, adding an additional check to return the correct cmdletInfo for cmdlets from core modules.
                        else if (InitialSessionState.IsEngineModule(cmdletInfo.ModuleName))
                                cmdletInfo.ModuleName,
                                InitialSessionState.GetNestedModuleDllName(commandName.PSSnapInName),
        internal static PSModuleAutoLoadingPreference GetCommandDiscoveryPreference(ExecutionContext context, VariablePath variablePath, string environmentVariable)
            Dbg.Assert(context != null, "context cannot be Null");
            Dbg.Assert(variablePath != null, "variablePath must be non empty");
            Dbg.Assert(!string.IsNullOrEmpty(environmentVariable), "environmentVariable must be non empty");
            // check the PSVariable
            object result = context.GetVariableValue(variablePath);
                    return LanguagePrimitives.ConvertTo<PSModuleAutoLoadingPreference>(result);
                // check the environment variable
                string psEnvironmentVariable = Environment.GetEnvironmentVariable(environmentVariable);
                if (!string.IsNullOrEmpty(psEnvironmentVariable))
                    return LanguagePrimitives.ConvertTo<PSModuleAutoLoadingPreference>(psEnvironmentVariable);
                return PSModuleAutoLoadingPreference.All;
    /// A helper collection of strings that doesn't allow duplicate strings. Comparison
    /// is case-insensitive and done in the invariant culture.
    internal class LookupPathCollection : Collection<string>
        internal LookupPathCollection() : base() { }
        /// Constructs a LookupPathCollection object and adds all the items
        /// in the supplied collection to it.
        /// <param name="collection">
        /// A set of items to be added to the collection.
        internal LookupPathCollection(IEnumerable<string> collection) : base()
            foreach (string item in collection)
                Add(item);
        /// Adds the specified string to the collection if its not already
        /// a member of the collection.
        /// The string to add to the collection.
        /// The index at which the string was added or -1 if it was not added.
        public new int Add(string item)
            int result = -1;
            if (!Contains(item))
                base.Add(item);
                result = base.IndexOf(item);
        /// Adds all the strings in the specified collection to this collection.
        /// The collection of strings to add.
        /// Only the strings that are not already in the collection will be added.
        internal void AddRange(ICollection<string> collection)
            foreach (string name in collection)
                Add(name);
        /// Determines if the string already exists in the collection
        /// using a invariant culture case insensitive comparison.
        /// The string to check for existence.
        ///  </param>
        /// True if the string already exists in the collection.
        public new bool Contains(string item)
            foreach (string name in this)
                if (string.Equals(item, name, StringComparison.OrdinalIgnoreCase))
        /// Returns a collection of all the indexes that are relative paths.
        /// A collection of all the indexes that are relative paths.
        internal Collection<int> IndexOfRelativePath()
            for (int index = 0; index < this.Count; ++index)
                string path = this[index];
                if (!string.IsNullOrEmpty(path) &&
                    path.StartsWith('.'))
                    result.Add(index);
        /// Finds the first index of the specified string. The string
        /// is compared in the invariant culture using a case-insensitive comparison.
        /// The string to look for.
        /// The index of the string in the collection or -1 if it was not found.
        /// If <paramref name="item"/> is null or empty.
        public new int IndexOf(string item)
            if (string.IsNullOrEmpty(item))
                throw PSTraceSource.NewArgumentException(nameof(item));
                if (string.Equals(this[index], item, StringComparison.OrdinalIgnoreCase))
                    result = index;
    // Guid is {ea9e8155-5042-5537-0b73-8c0e6b53f398}
    [EventSource(Name = "Microsoft-PowerShell-CommandDiscovery")]
    internal class CommandDiscoveryEventSource : EventSource
        internal static readonly CommandDiscoveryEventSource Log = new CommandDiscoveryEventSource();
        public void CommandLookupStart(string CommandName) { WriteEvent(1, CommandName); }
        public void CommandLookupStop(string CommandName) { WriteEvent(2, CommandName); }
        public void ModuleAutoLoadingStart(string CommandName) { WriteEvent(3, CommandName); }
        public void ModuleAutoLoadingStop(string CommandName) { WriteEvent(4, CommandName); }
        public void ModuleAutoDiscoveryStart(string CommandName) { WriteEvent(5, CommandName); }
        public void ModuleAutoDiscoveryStop(string CommandName) { WriteEvent(6, CommandName); }
        public void SearchingForModuleFilesStart() { WriteEvent(7); }
        public void SearchingForModuleFilesStop() { WriteEvent(8); }
        public void GetModuleExportedCommandsStart(string ModulePath) { WriteEvent(9, ModulePath); }
        public void GetModuleExportedCommandsStop(string ModulePath) { WriteEvent(10, ModulePath); }
        public void ModuleManifestAnalysisResult(string ModulePath, bool Success) { WriteEvent(11, ModulePath, Success); }
        public void ModuleManifestAnalysisException(string ModulePath, string Exception) { WriteEvent(12, ModulePath, Exception); }

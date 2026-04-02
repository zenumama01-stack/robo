    /// Class to manage the caching of analysis data.
    /// For performance, module command caching is flattened after discovery. Many modules have nested
    /// modules that can only be resolved at runtime - for example,
    /// script modules that declare: $env:PATH += "; $psScriptRoot". When
    /// doing initial analysis, we include these in 'ExportedCommands'.
    /// Changes to these type of modules will not be re-analyzed, unless the user re-imports the module,
    /// or runs Get-Module -List.
    internal static class AnalysisCache
        private static readonly AnalysisCacheData s_cacheData = AnalysisCacheData.Get();
        // This dictionary shouldn't see much use, so low concurrency and capacity
        private static readonly ConcurrentDictionary<string, string> s_modulesBeingAnalyzed =
            new(concurrencyLevel: 1, capacity: 2, StringComparer.OrdinalIgnoreCase);
        internal static readonly SearchValues<char> InvalidCommandNameCharacters = SearchValues.Create("#,(){}[]&/\\$^;:\"'<>|?@`*%+=~");
        internal static bool ContainsInvalidCommandNameCharacters(ReadOnlySpan<char> text)
            => text.ContainsAny(InvalidCommandNameCharacters);
        internal static ConcurrentDictionary<string, CommandTypes> GetExportedCommands(string modulePath, bool testOnly, ExecutionContext context)
            if (etwEnabled) CommandDiscoveryEventSource.Log.GetModuleExportedCommandsStart(modulePath);
            DateTime lastWriteTime;
            ModuleCacheEntry moduleCacheEntry;
            if (GetModuleEntryFromCache(modulePath, out lastWriteTime, out moduleCacheEntry))
                if (etwEnabled) CommandDiscoveryEventSource.Log.GetModuleExportedCommandsStop(modulePath);
                return moduleCacheEntry.Commands;
            ConcurrentDictionary<string, CommandTypes> result = null;
            if (!testOnly)
                var extension = Path.GetExtension(modulePath);
                if (extension.Equals(StringLiterals.PowerShellDataFileExtension, StringComparison.OrdinalIgnoreCase))
                    result = AnalyzeManifestModule(modulePath, context, lastWriteTime, etwEnabled);
                else if (extension.Equals(StringLiterals.PowerShellModuleFileExtension, StringComparison.OrdinalIgnoreCase))
                    result = AnalyzeScriptModule(modulePath, context, lastWriteTime);
                else if (extension.Equals(StringLiterals.PowerShellCmdletizationFileExtension, StringComparison.OrdinalIgnoreCase))
                    result = AnalyzeCdxmlModule(modulePath, context, lastWriteTime);
                else if (extension.Equals(StringLiterals.PowerShellILAssemblyExtension, StringComparison.OrdinalIgnoreCase))
                    result = AnalyzeDllModule(modulePath, context, lastWriteTime);
                else if (extension.Equals(StringLiterals.PowerShellILExecutableExtension, StringComparison.OrdinalIgnoreCase))
                s_cacheData.QueueSerialization();
                ModuleIntrinsics.Tracer.WriteLine("Returning {0} exported commands.", result.Count);
                ModuleIntrinsics.Tracer.WriteLine("Returning NULL for exported commands.");
        private static ConcurrentDictionary<string, CommandTypes> AnalyzeManifestModule(string modulePath, ExecutionContext context, DateTime lastWriteTime, bool etwEnabled)
                var moduleManifestProperties = PsUtils.GetModuleManifestProperties(modulePath, PsUtils.FastModuleManifestAnalysisPropertyNames);
                if (moduleManifestProperties != null)
                    if (!Configuration.PowerShellConfig.Instance.IsImplicitWinCompatEnabled() && ModuleIsEditionIncompatible(modulePath, moduleManifestProperties))
                        ModuleIntrinsics.Tracer.WriteLine($"Module lies on the Windows System32 legacy module path and is incompatible with current PowerShell edition, skipping module: {modulePath}");
                    if (ModuleUtils.IsModuleInVersionSubdirectory(modulePath, out version))
                        var versionInManifest = LanguagePrimitives.ConvertTo<Version>(moduleManifestProperties["ModuleVersion"]);
                        if (version != versionInManifest)
                            ModuleIntrinsics.Tracer.WriteLine("ModuleVersion in manifest does not match versioned module directory, skipping module: {0}", modulePath);
                    result = new ConcurrentDictionary<string, CommandTypes>(3, moduleManifestProperties.Count, StringComparer.OrdinalIgnoreCase);
                    var sawWildcard = false;
                    var hadCmdlets = AddPsd1EntryToResult(result, moduleManifestProperties["CmdletsToExport"], CommandTypes.Cmdlet, ref sawWildcard);
                    var hadFunctions = AddPsd1EntryToResult(result, moduleManifestProperties["FunctionsToExport"], CommandTypes.Function, ref sawWildcard);
                    var hadAliases = AddPsd1EntryToResult(result, moduleManifestProperties["AliasesToExport"], CommandTypes.Alias, ref sawWildcard);
                    var analysisSucceeded = hadCmdlets && hadFunctions && hadAliases;
                    if (!analysisSucceeded && !sawWildcard && (hadCmdlets || hadFunctions))
                        // If we're missing CmdletsToExport, that might still be OK, but only if we have a script module.
                        // Likewise, if we're missing FunctionsToExport, that might be OK, but only if we have a binary module.
                        analysisSucceeded = !CheckModulesTypesInManifestAgainstExportedCommands(moduleManifestProperties, hadCmdlets, hadFunctions, hadAliases);
                    if (analysisSucceeded)
                        var moduleCacheEntry = new ModuleCacheEntry
                            ModulePath = modulePath,
                            LastWriteTime = lastWriteTime,
                            Commands = result,
                            TypesAnalyzed = false,
                            Types = new ConcurrentDictionary<string, TypeAttributes>(1, 8, StringComparer.OrdinalIgnoreCase)
                        s_cacheData.Entries[modulePath] = moduleCacheEntry;
                if (etwEnabled) CommandDiscoveryEventSource.Log.ModuleManifestAnalysisException(modulePath, e.Message);
                // Ignore the errors, proceed with the usual module analysis
                ModuleIntrinsics.Tracer.WriteLine("Exception on fast-path analysis of module {0}", modulePath);
            if (etwEnabled) CommandDiscoveryEventSource.Log.ModuleManifestAnalysisResult(modulePath, result != null);
            return result ?? AnalyzeTheOldWay(modulePath, context, lastWriteTime);
        /// Check if a module is compatible with the current PSEdition given its path and its manifest properties.
        /// <param name="modulePath">The path to the module.</param>
        /// <param name="moduleManifestProperties">The properties of the module's manifest.</param>
        internal static bool ModuleIsEditionIncompatible(string modulePath, Hashtable moduleManifestProperties)
            if (!ModuleUtils.IsOnSystem32ModulePath(modulePath))
            if (!moduleManifestProperties.ContainsKey("CompatiblePSEditions"))
            return !Utils.IsPSEditionSupported(LanguagePrimitives.ConvertTo<string[]>(moduleManifestProperties["CompatiblePSEditions"]));
        internal static bool ModuleAnalysisViaGetModuleRequired(object modulePathObj, bool hadCmdlets, bool hadFunctions, bool hadAliases)
            if (modulePathObj is not string modulePath)
            if (modulePath.EndsWith(StringLiterals.PowerShellModuleFileExtension, StringComparison.OrdinalIgnoreCase))
                // A script module can't exactly define cmdlets, but it can import a binary module (as nested), so
                // it can indirectly define cmdlets.  And obviously a script module can define functions and aliases.
                // If we got here, one of those is missing, so analysis is required.
            if (modulePath.EndsWith(StringLiterals.PowerShellCmdletizationFileExtension, StringComparison.OrdinalIgnoreCase))
                // A cdxml module can only define functions and aliases, so if we have both, no more analysis is required.
                return !hadFunctions || !hadAliases;
            if (modulePath.EndsWith(StringLiterals.PowerShellILAssemblyExtension, StringComparison.OrdinalIgnoreCase))
                // A dll just exports cmdlets, so if the manifest doesn't explicitly export any cmdlets,
                // more analysis is required. If the module exports aliases, we can't discover that analyzing
                // the binary, so aliases are always required to be explicit (no wildcards) in the manifest.
                return !hadCmdlets;
            if (modulePath.EndsWith(StringLiterals.PowerShellILExecutableExtension, StringComparison.OrdinalIgnoreCase))
            // Any other extension (or no extension), just assume the worst and analyze the module
        // Returns true if we need to analyze the manifest module in Get-Module because
        // our quick and dirty module manifest analysis is missing something not easily
        // discovered.
        // TODO - psm1 modules are actually easily handled, so if we only saw a psm1 here,
        // we should just analyze it and not fall back on Get-Module -List.
        private static bool CheckModulesTypesInManifestAgainstExportedCommands(Hashtable moduleManifestProperties, bool hadCmdlets, bool hadFunctions, bool hadAliases)
            var rootModule = moduleManifestProperties["RootModule"];
            if (rootModule != null && ModuleAnalysisViaGetModuleRequired(rootModule, hadCmdlets, hadFunctions, hadAliases))
            var moduleToProcess = moduleManifestProperties["ModuleToProcess"];
            if (moduleToProcess != null && ModuleAnalysisViaGetModuleRequired(moduleToProcess, hadCmdlets, hadFunctions, hadAliases))
            var nestedModules = moduleManifestProperties["NestedModules"];
            if (nestedModules != null)
                var nestedModule = nestedModules as string;
                if (nestedModule != null)
                    return ModuleAnalysisViaGetModuleRequired(nestedModule, hadCmdlets, hadFunctions, hadAliases);
                if (nestedModules is not object[] nestedModuleArray)
                foreach (var element in nestedModuleArray)
                    if (ModuleAnalysisViaGetModuleRequired(element, hadCmdlets, hadFunctions, hadAliases))
        private static bool AddPsd1EntryToResult(ConcurrentDictionary<string, CommandTypes> result, string command, CommandTypes commandTypeToAdd, ref bool sawWildcard)
                sawWildcard = true;
            // An empty string is one way of saying "no exported commands".
            if (command.Length != 0)
                CommandTypes commandTypes;
                if (result.TryGetValue(command, out commandTypes))
                    commandTypes |= commandTypeToAdd;
                    commandTypes = commandTypeToAdd;
                result[command] = commandTypes;
        private static bool AddPsd1EntryToResult(ConcurrentDictionary<string, CommandTypes> result, object value, CommandTypes commandTypeToAdd, ref bool sawWildcard)
            string command = value as string;
                return AddPsd1EntryToResult(result, command, commandTypeToAdd, ref sawWildcard);
            object[] commands = value as object[];
            if (commands != null)
                foreach (var o in commands)
                    if (!AddPsd1EntryToResult(result, o, commandTypeToAdd, ref sawWildcard))
                // An empty array is still success, that's how a manifest declares that
                // no entries are exported (unlike the lack of an entry, or $null).
            // Unknown type, let Get-Module -List deal with this manifest
        private static ConcurrentDictionary<string, CommandTypes> AnalyzeScriptModule(string modulePath, ExecutionContext context, DateTime lastWriteTime)
            var scriptAnalysis = ScriptAnalysis.Analyze(modulePath, context);
            if (scriptAnalysis == null)
            List<WildcardPattern> scriptAnalysisPatterns = new List<WildcardPattern>();
            foreach (string discoveredCommandFilter in scriptAnalysis.DiscoveredCommandFilters)
                scriptAnalysisPatterns.Add(new WildcardPattern(discoveredCommandFilter));
            var result = new ConcurrentDictionary<string, CommandTypes>(3,
                scriptAnalysis.DiscoveredExports.Count + scriptAnalysis.DiscoveredAliases.Count,
            // Add any directly discovered exports
            foreach (var command in scriptAnalysis.DiscoveredExports)
                if (SessionStateUtilities.MatchesAnyWildcardPattern(command, scriptAnalysisPatterns, true))
                    if (!ContainsInvalidCommandNameCharacters(command))
                        result[command] = CommandTypes.Function;
            // Add the discovered aliases
            foreach (var pair in scriptAnalysis.DiscoveredAliases)
                var commandName = pair.Key;
                // These are already filtered
                if (!ContainsInvalidCommandNameCharacters(commandName))
                    result.AddOrUpdate(commandName, CommandTypes.Alias,
                        static (_, existingCommandType) => existingCommandType | CommandTypes.Alias);
            // Add any files in PsScriptRoot if it added itself to the path
            if (scriptAnalysis.AddsSelfToPath)
                string baseDirectory = Path.GetDirectoryName(modulePath);
                    foreach (string item in Directory.EnumerateFiles(baseDirectory, "*.ps1"))
                        var command = Path.GetFileNameWithoutExtension(item);
                        result.AddOrUpdate(command, CommandTypes.ExternalScript,
                            static (_, existingCommandType) => existingCommandType | CommandTypes.ExternalScript);
                    // Consume this exception here
            ConcurrentDictionary<string, TypeAttributes> exportedClasses = new(
                concurrencyLevel: 1,
                capacity: scriptAnalysis.DiscoveredClasses.Count,
            foreach (var exportedClass in scriptAnalysis.DiscoveredClasses)
                exportedClasses[exportedClass.Name] = exportedClass.TypeAttributes;
                TypesAnalyzed = true,
                Types = exportedClasses
        private static ConcurrentDictionary<string, CommandTypes> AnalyzeCdxmlModule(string modulePath, ExecutionContext context, DateTime lastWriteTime)
            return AnalyzeTheOldWay(modulePath, context, lastWriteTime);
        private static ConcurrentDictionary<string, CommandTypes> AnalyzeDllModule(string modulePath, ExecutionContext context, DateTime lastWriteTime)
        private static ConcurrentDictionary<string, CommandTypes> AnalyzeTheOldWay(string modulePath, ExecutionContext context, DateTime lastWriteTime)
                // If we're already analyzing this module, let the recursion bottom out.
                if (!s_modulesBeingAnalyzed.TryAdd(modulePath, modulePath))
                    ModuleIntrinsics.Tracer.WriteLine("{0} is already being analyzed. Exiting.", modulePath);
                // Record that we're analyzing this specific module so that we don't get stuck in recursion
                ModuleIntrinsics.Tracer.WriteLine("Started analysis: {0}", modulePath);
                CallGetModuleDashList(context, modulePath);
                ModuleIntrinsics.Tracer.WriteLine("Module analysis generated an exception: {0}", e);
                // Catch-all OK, third-party call-out.
                ModuleIntrinsics.Tracer.WriteLine("Finished analysis: {0}", modulePath);
                s_modulesBeingAnalyzed.TryRemove(modulePath, out modulePath);
        /// Return the exported types for a specific module.
        /// If the module is already cache, return from cache, else cache the module.
        /// Also re-cache the module if the cached item is stale.
        /// <param name="modulePath">Path to the module to get exported types from.</param>
        /// <param name="context">Current Context.</param>
        internal static ConcurrentDictionary<string, TypeAttributes> GetExportedClasses(string modulePath, ExecutionContext context)
            if (GetModuleEntryFromCache(modulePath, out lastWriteTime, out moduleCacheEntry) && moduleCacheEntry.TypesAnalyzed)
                return moduleCacheEntry.Types;
        internal static void CacheModuleExports(PSModuleInfo module, ExecutionContext context)
            ModuleIntrinsics.Tracer.WriteLine("Requested caching for {0}", module.Name);
            // Don't cache incompatible modules on the system32 module path even if loaded with
            // -SkipEditionCheck, since it will break subsequent sessions
            if (!Configuration.PowerShellConfig.Instance.IsImplicitWinCompatEnabled() && !module.IsConsideredEditionCompatible)
                ModuleIntrinsics.Tracer.WriteLine($"Module '{module.Name}' not edition compatible and not cached.");
            GetModuleEntryFromCache(module.Path, out lastWriteTime, out moduleCacheEntry);
            var realExportedCommands = module.ExportedCommands;
            var realExportedClasses = module.GetExportedTypeDefinitions();
            ConcurrentDictionary<string, CommandTypes> exportedCommands;
            ConcurrentDictionary<string, TypeAttributes> exportedClasses;
            // First see if the existing module info is sufficient. GetModuleEntryFromCache does LastWriteTime
            // verification, so this will also return nothing if the cache is out of date or corrupt.
            if (moduleCacheEntry != null)
                bool needToUpdate = false;
                // We need to iterate and check as exportedCommands will have more item as it can have aliases as well.
                exportedCommands = moduleCacheEntry.Commands;
                foreach (var pair in realExportedCommands)
                    var realCommandType = pair.Value.CommandType;
                    CommandTypes commandType;
                    if (!exportedCommands.TryGetValue(commandName, out commandType) || commandType != realCommandType)
                        needToUpdate = true;
                exportedClasses = moduleCacheEntry.Types;
                foreach (var pair in realExportedClasses)
                    var className = pair.Key;
                    var realTypeAttributes = pair.Value.TypeAttributes;
                    TypeAttributes typeAttributes;
                    if (!exportedClasses.TryGetValue(className, out typeAttributes) ||
                        typeAttributes != realTypeAttributes)
                // Update or not, we've analyzed commands and types now.
                moduleCacheEntry.TypesAnalyzed = true;
                if (!needToUpdate)
                    ModuleIntrinsics.Tracer.WriteLine("Existing cached info up-to-date. Skipping.");
                exportedCommands.Clear();
                exportedClasses.Clear();
                exportedCommands = new ConcurrentDictionary<string, CommandTypes>(3, realExportedCommands.Count, StringComparer.OrdinalIgnoreCase);
                exportedClasses = new ConcurrentDictionary<string, TypeAttributes>(1, realExportedClasses.Count, StringComparer.OrdinalIgnoreCase);
                moduleCacheEntry = new ModuleCacheEntry
                    ModulePath = module.Path,
                    Commands = exportedCommands,
                moduleCacheEntry = s_cacheData.Entries.GetOrAdd(module.Path, moduleCacheEntry);
            // We need to update the cache
            foreach (var exportedCommand in realExportedCommands.Values)
                ModuleIntrinsics.Tracer.WriteLine("Caching command: {0}", exportedCommand.Name);
                exportedCommands.GetOrAdd(exportedCommand.Name, exportedCommand.CommandType);
                ModuleIntrinsics.Tracer.WriteLine("Caching command: {0}", className);
                moduleCacheEntry.Types.AddOrUpdate(className, pair.Value.TypeAttributes, (k, t) => t);
        private static void CallGetModuleDashList(ExecutionContext context, string modulePath)
            CommandInfo commandInfo = new CmdletInfo("Get-Module", typeof(GetModuleCommand), null, null, context);
            Command getModuleCommand = new Command(commandInfo);
                PowerShell.Create(RunspaceMode.CurrentRunspace)
                        .AddParameter("Name", modulePath)
                    .Invoke();
        private static bool GetModuleEntryFromCache(string modulePath, out DateTime lastWriteTime, out ModuleCacheEntry moduleCacheEntry)
                lastWriteTime = new FileInfo(modulePath).LastWriteTime;
                ModuleIntrinsics.Tracer.WriteLine("Exception checking LastWriteTime on module {0}: {1}", modulePath, e.Message);
                lastWriteTime = DateTime.MinValue;
            if (s_cacheData.Entries.TryGetValue(modulePath, out moduleCacheEntry))
                if (lastWriteTime == moduleCacheEntry.LastWriteTime)
                ModuleIntrinsics.Tracer.WriteLine("{0}: cache entry out of date, cached on {1}, last updated on {2}",
                    modulePath, moduleCacheEntry.LastWriteTime, lastWriteTime);
                s_cacheData.Entries.TryRemove(modulePath, out moduleCacheEntry);
            moduleCacheEntry = null;
    internal sealed class AnalysisCacheData
        private static byte[] GetHeader()
            return new byte[]
                0x50, 0x53, 0x4d, 0x4f, 0x44, 0x55, 0x4c, 0x45, 0x43, 0x41, 0x43, 0x48, 0x45, // PSMODULECACHE
                0x01 // version #
        // The last time the index was maintained.
        public DateTime LastReadTime { get; set; }
        public ConcurrentDictionary<string, ModuleCacheEntry> Entries { get; set; }
        private int _saveCacheToDiskQueued;
        private bool _saveCacheToDisk = true;
        public void QueueSerialization()
            if (string.IsNullOrEmpty(s_cacheStoreLocation))
            // We expect many modules to rapidly call for serialization.
            // Instead of doing it right away, we'll queue a task that starts writing
            // after it seems like we've stopped adding stuff to write out.  This is
            // avoids blocking the pipeline thread waiting for the write to finish.
            // We want to make sure we only queue one task.
            if (_saveCacheToDisk && Interlocked.Increment(ref _saveCacheToDiskQueued) == 1)
                Task.Run(async delegate
                    // Wait a while before assuming we've finished the updates,
                    // writing the cache out in a timely matter isn't too important
                    // now anyway.
                    await Task.Delay(10000).ConfigureAwait(false);
                    int counter1, counter2;
                        // Check the counter a couple times with a delay,
                        // if it's stable, then proceed with writing.
                        counter1 = _saveCacheToDiskQueued;
                        await Task.Delay(3000).ConfigureAwait(false);
                        counter2 = _saveCacheToDiskQueued;
                    } while (counter1 != counter2);
                    Serialize(s_cacheStoreLocation);
        // Remove entries that are not needed anymore, e.g. if a module was removed.
        // If anything is removed, save the cache.
            Diagnostics.Assert(Environment.GetEnvironmentVariable("PSDisableModuleAnalysisCacheCleanup") == null,
                "Caller to check environment variable before calling");
            bool removedSomething = false;
            var keys = Entries.Keys;
                if (!File.Exists(key))
                    removedSomething |= Entries.TryRemove(key, out ModuleCacheEntry _);
            if (removedSomething)
                QueueSerialization();
        private static unsafe void Write(int val, byte[] bytes, FileStream stream)
            Diagnostics.Assert(bytes.Length >= 4, "Must pass a large enough byte array");
            fixed (byte* b = bytes) *((int*)b) = val;
            stream.Write(bytes, 0, 4);
        private static unsafe void Write(long val, byte[] bytes, FileStream stream)
            Diagnostics.Assert(bytes.Length >= 8, "Must pass a large enough byte array");
            fixed (byte* b = bytes) *((long*)b) = val;
            stream.Write(bytes, 0, 8);
        private static void Write(string val, byte[] bytes, FileStream stream)
            Write(val.Length, bytes, stream);
            bytes = Encoding.UTF8.GetBytes(val);
            stream.Write(bytes, 0, bytes.Length);
        private void Serialize(string filename)
            AnalysisCacheData fromOtherProcess = null;
            Diagnostics.Assert(_saveCacheToDisk, "Serialize should never be called without going through QueueSerialization which has a check");
                if (File.Exists(filename))
                    var fileLastWriteTime = new FileInfo(filename).LastWriteTime;
                    if (fileLastWriteTime > this.LastReadTime)
                        fromOtherProcess = Deserialize(filename);
                    // Make sure the folder exists
                    var folder = Path.GetDirectoryName(filename);
                    if (!Directory.Exists(folder))
                            Directory.CreateDirectory(folder);
                            // service accounts won't be able to create directory
                            _saveCacheToDisk = false;
                ModuleIntrinsics.Tracer.WriteLine("Exception checking module analysis cache {0}: {1} ", filename, e.Message);
            if (fromOtherProcess != null)
                // We should merge with what another process wrote so we don't clobber useful analysis
                foreach (var otherEntryPair in fromOtherProcess.Entries)
                    var otherModuleName = otherEntryPair.Key;
                    var otherEntry = otherEntryPair.Value;
                    ModuleCacheEntry thisEntry;
                    if (Entries.TryGetValue(otherModuleName, out thisEntry))
                        if (otherEntry.LastWriteTime > thisEntry.LastWriteTime)
                            // The other entry is newer, take it over ours
                            Entries[otherModuleName] = otherEntry;
            // "PSMODULECACHE"     -> 13 bytes
            // byte     ( 1 byte)  -> version
            // int      ( 4 bytes) -> count of entries
            // entries  (?? bytes) -> all entries
            // each entry is
            //   DateTime ( 8 bytes) -> last write time for module file
            //   int      ( 4 bytes) -> path length
            //   string   (?? bytes) -> utf8 encoded path
            //   int      ( 4 bytes) -> count of commands
            //   commands (?? bytes) -> all commands
            //   int      ( 4 bytes) -> count of types, -1 means unanalyzed (and 0 items serialized)
            //   types    (?? bytes) -> all types
            // each command is
            //   int      ( 4 bytes) -> command name length
            //   string   (?? bytes) -> utf8 encoded command name
            //   int      ( 4 bytes) -> CommandTypes enum
            // each type is
            //   int     ( 4 bytes) -> type name length
            //   string  (?? bytes) -> utf8 encoded type name
            //   int     ( 4 bytes) -> type attributes
                var bytes = new byte[8];
                using (var stream = File.Create(filename))
                    var headerBytes = GetHeader();
                    stream.Write(headerBytes, 0, headerBytes.Length);
                    // Count of entries
                    Write(Entries.Count, bytes, stream);
                    foreach (var pair in Entries.ToArray())
                        var path = pair.Key;
                        // Module last write time
                        Write(entry.LastWriteTime.Ticks, bytes, stream);
                        // Module path
                        Write(path, bytes, stream);
                        // Commands
                        var commandPairs = entry.Commands.ToArray();
                        Write(commandPairs.Length, bytes, stream);
                        foreach (var command in commandPairs)
                            Write(command.Key, bytes, stream);
                            Write((int)command.Value, bytes, stream);
                        // Types
                        var typePairs = entry.Types.ToArray();
                        Write(entry.TypesAnalyzed ? typePairs.Length : -1, bytes, stream);
                        foreach (var type in typePairs)
                            Write(type.Key, bytes, stream);
                            Write((int)type.Value, bytes, stream);
                // We just wrote the file, note this so we can detect writes from another process
                LastReadTime = new FileInfo(filename).LastWriteTime;
                ModuleIntrinsics.Tracer.WriteLine("Exception writing module analysis cache {0}: {1} ", filename, e.Message);
            // Reset our counter so we can write again if asked.
            Interlocked.Exchange(ref _saveCacheToDiskQueued, 0);
        private const string TruncatedErrorMessage = "module cache file appears truncated";
        private const string InvalidSignatureErrorMessage = "module cache signature not valid";
        private const string PossibleCorruptionErrorMessage = "possible corruption in module cache";
        private static unsafe long ReadLong(FileStream stream, byte[] bytes)
            if (stream.Read(bytes, 0, 8) != 8)
                throw new Exception(TruncatedErrorMessage);
            fixed (byte* b = bytes)
                return *(long*)b;
        private static unsafe int ReadInt(FileStream stream, byte[] bytes)
            if (stream.Read(bytes, 0, 4) != 4)
                return *(int*)b;
        private static string ReadString(FileStream stream, ref byte[] bytes)
            int length = ReadInt(stream, bytes);
            if (length > 10 * 1024)
                throw new Exception(PossibleCorruptionErrorMessage);
            if (length > bytes.Length)
                bytes = new byte[length];
            if (stream.Read(bytes, 0, length) != length)
            return Encoding.UTF8.GetString(bytes, 0, length);
        private static void ReadHeader(FileStream stream, byte[] bytes)
            var length = headerBytes.Length;
            Diagnostics.Assert(bytes.Length >= length, "must pass a large enough byte array");
                if (bytes[i] != headerBytes[i])
                    throw new Exception(InvalidSignatureErrorMessage);
            // No need to return - we don't use it other than to detect the correct file format
        public static AnalysisCacheData Deserialize(string filename)
            using (var stream = File.OpenRead(filename))
                var result = new AnalysisCacheData { LastReadTime = DateTime.Now };
                var bytes = new byte[1024];
                // Header
                ReadHeader(stream, bytes);
                int entries = ReadInt(stream, bytes);
                if (entries > 20 * 1024)
                result.Entries = new ConcurrentDictionary<string, ModuleCacheEntry>(/*concurrency*/3, entries, StringComparer.OrdinalIgnoreCase);
                while (entries > 0)
                    var lastWriteTime = new DateTime(ReadLong(stream, bytes));
                    var path = ReadString(stream, ref bytes);
                    var countItems = ReadInt(stream, bytes);
                    if (countItems > 20 * 1024)
                    var commands = new ConcurrentDictionary<string, CommandTypes>(/*concurrency*/3, countItems, StringComparer.OrdinalIgnoreCase);
                    while (countItems > 0)
                        var commandName = ReadString(stream, ref bytes);
                        var commandTypes = (CommandTypes)ReadInt(stream, bytes);
                        // Ignore empty entries (possible corruption in the cache or bug?)
                        if (!string.IsNullOrWhiteSpace(commandName))
                            commands[commandName] = commandTypes;
                        countItems -= 1;
                    //   int      ( 4 bytes) -> count of types
                    countItems = ReadInt(stream, bytes);
                    bool typesAnalyzed = countItems != -1;
                    if (!typesAnalyzed)
                        countItems = 0;
                    var types = new ConcurrentDictionary<string, TypeAttributes>(1, countItems, StringComparer.OrdinalIgnoreCase);
                        var typeName = ReadString(stream, ref bytes);
                        var typeAttributes = (TypeAttributes)ReadInt(stream, bytes);
                        if (!string.IsNullOrWhiteSpace(typeName))
                            types[typeName] = typeAttributes;
                    var entry = new ModuleCacheEntry
                        ModulePath = path,
                        Commands = commands,
                        TypesAnalyzed = typesAnalyzed,
                        Types = types
                    result.Entries[path] = entry;
                    entries -= 1;
                if (Environment.GetEnvironmentVariable("PSDisableModuleAnalysisCacheCleanup") == null)
                    Task.Delay(10000).ContinueWith(_ => result.Cleanup());
        internal static AnalysisCacheData Get()
            int retryCount = 3;
                    if (File.Exists(s_cacheStoreLocation))
                        return Deserialize(s_cacheStoreLocation);
                    ModuleIntrinsics.Tracer.WriteLine("Exception checking module analysis cache: " + e.Message);
                    if ((object)e.Message == (object)TruncatedErrorMessage
                        || (object)e.Message == (object)InvalidSignatureErrorMessage
                        || (object)e.Message == (object)PossibleCorruptionErrorMessage)
                        // Don't retry if we detected something is wrong with the file
                        // (as opposed to the file being locked or something else)
                retryCount -= 1;
                Thread.Sleep(25); // Sleep a bit to give time for another process to finish writing the cache
            } while (retryCount > 0);
            return new AnalysisCacheData
                LastReadTime = DateTime.Now,
                // Capacity set to 100 - a bit bigger than the # of modules on a default Win10 client machine
                // Concurrency=3 to not create too many locks, contention is unclear, but the old code had a single lock
                Entries = new ConcurrentDictionary<string, ModuleCacheEntry>(/*concurrency*/3, /*capacity*/100, StringComparer.OrdinalIgnoreCase)
        private AnalysisCacheData()
        private static readonly string s_cacheStoreLocation;
        static AnalysisCacheData()
            // If user defines a custom cache path, then use that.
            string userDefinedCachePath = Environment.GetEnvironmentVariable("PSModuleAnalysisCachePath");
            if (!string.IsNullOrEmpty(userDefinedCachePath))
                s_cacheStoreLocation = userDefinedCachePath;
            string cacheFileName = "ModuleAnalysisCache";
            // When multiple copies of pwsh are on the system, they should use their own copy of the cache.
            // Append hash of `$PSHOME` to cacheFileName.
            string hashString = CRC32Hash.ComputeHash(Utils.DefaultPowerShellAppBase);
            cacheFileName = string.Create(CultureInfo.InvariantCulture, $"{cacheFileName}-{hashString}");
            if (ExperimentalFeature.EnabledExperimentalFeatureNames.Count > 0)
                // If any experimental features are enabled, we cannot use the default cache file because those
                // features may expose commands that are not available in a regular powershell session, and we
                // should not cache those commands in the default cache file because that will result in wrong
                // auto-completion suggestions when the default cache file is used in another powershell session.
                // Here we will generate a cache file name that represent the combination of enabled feature names.
                // We first convert enabled feature names to lower case, then we sort the feature names, and then
                // compute an CRC32 hash from the sorted feature names. We will use the CRC32 hash to generate the
                // cache file name.
                string[] featureNames = new string[ExperimentalFeature.EnabledExperimentalFeatureNames.Count];
                foreach (string featureName in ExperimentalFeature.EnabledExperimentalFeatureNames)
                    featureNames[index++] = featureName.ToLowerInvariant();
                Array.Sort(featureNames);
                string allNames = string.Join(Environment.NewLine, featureNames);
                // Use CRC32 because it's faster.
                // It's very unlikely to get collision from hashing the combinations of enabled features names.
                hashString = CRC32Hash.ComputeHash(allNames);
            Platform.TryDeriveFromCache(cacheFileName, out s_cacheStoreLocation);
    [DebuggerDisplay("ModulePath = {ModulePath}")]
    internal class ModuleCacheEntry
        public DateTime LastWriteTime;
        public string ModulePath;
        public bool TypesAnalyzed;
        public ConcurrentDictionary<string, CommandTypes> Commands;
        public ConcurrentDictionary<string, TypeAttributes> Types;

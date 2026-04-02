    internal static class EarlyStartup
        internal static void Init()
            // Code added here should:
            //   * run every time we start PowerSHell
            //   * have high CPU cost
            //   * be ordered from most expensive to least expensive, or at least needed earliest
            //   * this method should return quickly, so all work should be run in one or more tasks.
            //   * code called from here should correctly handle being called twice, in case initialization
            //     is needed in the main code path before the task completes.
            // Code added here should not:
            //   * count on running - not all hosts will call this method
            //   * have high disk cost
            // We shouldn't create too many tasks.
            // Amsi initialize can be a little slow.
            Task.Run(() => AmsiUtils.WinScanContent(content: string.Empty, sourceMetadata: string.Empty, warmUp: true));
            // Initialize the types 'Compiler', 'CachedReflectionInfo', and 'ExpressionCache'.
            // Their type initializers do a lot of reflection operations.
            // We will access 'Compiler' members when creating the first session state.
            Task.Run(() => _ = Compiler.DottedLocalsTupleType);
            // One other task for other stuff that's faster, but still a little slow.
                // Loading the resources for System.Management.Automation can be expensive,
                // so force that to happen early on a background thread.
                _ = RunspaceInit.OutputEncodingDescription;
                // This will init some tables and could load some assemblies.
                // We will access 'LanguagePrimitives' when binding built-in variables for the Runspace.
                LanguagePrimitives.GetEnumerator(null);
                // We will access 'TypeAccelerators' when auto-loading the PSReadLine module, which happens last.
                _ = TypeAccelerators.builtinTypeAccelerators;
    /// Baseclass for defining elements that can be added
    /// to an InitialSessionState object.
    public abstract class InitialSessionStateEntry
        /// The ctor so that each derived class has a name.
        protected InitialSessionStateEntry(string name)
        /// The name of this entry.
        /// The SnapIn to load from initially.
        public PSSnapInInfo PSSnapIn { get; private set; }
        internal void SetPSSnapIn(PSSnapInInfo psSnapIn)
        /// Shallow-clone this object.
        /// <returns>The cloned object...</returns>
        public abstract InitialSessionStateEntry Clone();
    /// Class to constrain session state entries.
    public abstract class ConstrainedSessionStateEntry : InitialSessionStateEntry
        /// <param name="visibility"></param>
        protected ConstrainedSessionStateEntry(string name, SessionStateEntryVisibility visibility)
            Visibility = visibility;
        public SessionStateEntryVisibility Visibility { get; set; }
    /// Command class so that all the commands can derive off this one.
    /// Adds the flexibility of adding additional derived class,
    /// such as ProxyCommand for Exchange.
    /// Derived classes - Alias, Application, Cmdlet, Function, Script.
    public abstract class SessionStateCommandEntry : ConstrainedSessionStateEntry
        /// Base constructor for all SessionState commands.
        protected SessionStateCommandEntry(string name)
            : base(name, SessionStateEntryVisibility.Public)
        protected internal SessionStateCommandEntry(string name, SessionStateEntryVisibility visibility)
            : base(name, visibility)
        /// Returns the type of the command using an enum
        /// instead of requiring a full reflection type check.
        public CommandTypes CommandType { get; internal set; }
        /// Is internal so it can be set by the engine code...
        /// This is used to specify whether this command was imported or not
        /// If noClobber is specified during Import-Module, it is set to false.
        internal bool _isImported = true;
    /// Type file configuration entry...
    public sealed class SessionStateTypeEntry : InitialSessionStateEntry
        /// Loads all entries from the types file.
        public SessionStateTypeEntry(string fileName)
            : base(fileName)
                throw PSTraceSource.NewArgumentException(nameof(fileName));
            FileName = fileName.Trim();
        /// Loads all the types specified in the typeTable.
        /// <param name="typeTable"></param>
        public SessionStateTypeEntry(TypeTable typeTable)
            : base("*")
                throw PSTraceSource.NewArgumentNullException(nameof(typeTable));
            TypeTable = typeTable;
        /// Loads all entries from the typeData.
        /// <param name="isRemove"></param>
        public SessionStateTypeEntry(TypeData typeData, bool isRemove)
            if (typeData == null)
                throw PSTraceSource.NewArgumentNullException(nameof(typeData));
            TypeData = typeData;
            IsRemove = isRemove;
        /// <returns>The cloned object.</returns>
        public override InitialSessionStateEntry Clone()
            SessionStateTypeEntry entry;
            if (FileName != null)
                entry = new SessionStateTypeEntry(FileName);
            else if (TypeTable != null)
                entry = new SessionStateTypeEntry(TypeTable);
                entry = new SessionStateTypeEntry(TypeData, IsRemove);
            entry.SetPSSnapIn(this.PSSnapIn);
            entry.SetModule(this.Module);
        /// The pathname of the types.ps1xml file. This can be null if
        /// TypeTable constructor or TypeData constructor is used.
        public string FileName { get; }
        /// The TypeTable specified with constructor. This can be null if
        /// FileName constructor or TypeData constructor is used.
        public TypeTable TypeTable { get; }
        /// The TypeData we want to update with. This can be null if
        /// FileName constructor or TypeTable constructor is used.
        public TypeData TypeData { get; }
        /// The operation will be done on the typedata. This is only
        /// meaningful when the TypeData constructor is used.
        public bool IsRemove { get; }
        // So that we can specify the type information on the fly,
        // without using Types.ps1xml file
        // public SessionStateTypeEntry(string name, xmlreader definition);
        // public string Definition { get; }
    /// Format file configuration entry...
    public sealed class SessionStateFormatEntry : InitialSessionStateEntry
        /// Loads the entire formats file.
        public SessionStateFormatEntry(string fileName)
        /// Loads all the format data specified in the formatTable.
        /// <param name="formattable"></param>
        public SessionStateFormatEntry(FormatTable formattable)
            if (formattable == null)
                throw PSTraceSource.NewArgumentNullException(nameof(formattable));
            Formattable = formattable;
        /// Loads all the format data specified in the typeDefinition.
        /// <param name="typeDefinition"></param>
        public SessionStateFormatEntry(ExtendedTypeDefinition typeDefinition)
            FormatData = typeDefinition;
        /// Shallow-clone this object...
            SessionStateFormatEntry entry;
                entry = new SessionStateFormatEntry(FileName);
            else if (Formattable != null)
                entry = new SessionStateFormatEntry(Formattable);
                entry = new SessionStateFormatEntry(FormatData);
        /// The name of the format file referenced by this entry...
        /// The FormatTable specified with constructor. This can be null if
        /// FileName constructor is used.
        public FormatTable Formattable { get; }
        /// The FormatData specified with constructor.
        /// This can be null if the FileName or FormatTable constructors are used.
        public ExtendedTypeDefinition FormatData { get; }
        // So that we can specify the format information on the fly,
        // without using Format.ps1xml file
        // public SessionStateFormatEntry(string name, xmlreader definition);
    /// An assembly to load for this sessionstate...
    public sealed class SessionStateAssemblyEntry : InitialSessionStateEntry
        /// Create a named entry for the assembly to load with both the
        /// name and the path to the assembly as a backup.
        /// <param name="name">The name of the assembly to load.</param>
        /// <param name="fileName">The path to the assembly to use as an alternative.</param>
        public SessionStateAssemblyEntry(string name, string fileName)
            FileName = fileName;
        /// Create a named entry for the assembly to load, specifying
        /// just the name.
        public SessionStateAssemblyEntry(string name)
            var entry = new SessionStateAssemblyEntry(Name, FileName);
        /// Return the assembly file name...
    /// List a cmdlet to add to this session state entry.
    public sealed class SessionStateCmdletEntry : SessionStateCommandEntry
        /// <param name="implementingType"></param>
        /// <param name="helpFileName"></param>
        public SessionStateCmdletEntry(string name, Type implementingType, string helpFileName)
            HelpFileName = helpFileName;
            CommandType = CommandTypes.Cmdlet;
        internal SessionStateCmdletEntry(string name, Type implementingType, string helpFileName, SessionStateEntryVisibility visibility)
            SessionStateCmdletEntry entry = new SessionStateCmdletEntry(Name, ImplementingType, HelpFileName, Visibility);
        public string HelpFileName { get; }
    public sealed class SessionStateProviderEntry : ConstrainedSessionStateEntry
        public SessionStateProviderEntry(string name, Type implementingType, string helpFileName)
        internal SessionStateProviderEntry(string name, Type implementingType, string helpFileName, SessionStateEntryVisibility visibility)
            SessionStateProviderEntry entry = new SessionStateProviderEntry(Name, ImplementingType, HelpFileName, this.Visibility);
    public sealed class SessionStateScriptEntry : SessionStateCommandEntry
        /// Create a session state command entry instance.
        /// <param name="path">The path to the script.</param>
        public SessionStateScriptEntry(string path)
            : base(path, SessionStateEntryVisibility.Public)
            CommandType = CommandTypes.ExternalScript;
        /// Create a session state command entry instance with the specified visibility.
        /// <param name="visibility">Visibility of the script.</param>
        internal SessionStateScriptEntry(string path, SessionStateEntryVisibility visibility)
            : base(path, visibility)
            SessionStateScriptEntry entry = new SessionStateScriptEntry(Path, Visibility);
    public sealed class SessionStateAliasEntry : SessionStateCommandEntry
        /// Define an alias entry to add to the initial session state.
        /// <param name="name">The name of the alias entry to add.</param>
        /// <param name="definition">The name of the command it resolves to.</param>
        public SessionStateAliasEntry(string name, string definition)
            CommandType = CommandTypes.Alias;
        /// <param name="description">A description of the purpose of the alias.</param>
        public SessionStateAliasEntry(string name, string definition, string description)
        /// <param name="options">Options defining the scope visibility, readonly and constant.</param>
        public SessionStateAliasEntry(string name, string definition, string description, ScopedItemOptions options)
            Options = options;
        internal SessionStateAliasEntry(string name, string definition, string description, ScopedItemOptions options, SessionStateEntryVisibility visibility)
            SessionStateAliasEntry entry = new SessionStateAliasEntry(Name, Definition, Description, Options, Visibility);
        /// The string defining the body of this alias...
        /// A string describing this alias...
        /// Options controlling scope visibility and setability for this entry.
        public ScopedItemOptions Options { get; } = ScopedItemOptions.None;
    public sealed class SessionStateApplicationEntry : SessionStateCommandEntry
        /// Used to define a permitted script in this session state. If the path is
        /// "*", then any path is permitted.
        /// <param name="path">The full path to the application.</param>
        public SessionStateApplicationEntry(string path)
            CommandType = CommandTypes.Application;
        /// <param name="visibility">Sets the external visibility of the path.</param>
        internal SessionStateApplicationEntry(string path, SessionStateEntryVisibility visibility)
            SessionStateApplicationEntry entry = new SessionStateApplicationEntry(Path, Visibility);
        /// The path to this application...
    public sealed class SessionStateFunctionEntry : SessionStateCommandEntry
        /// Represents a function definition in an Initial session state object.
        /// <param name="name">The name of the function.</param>
        /// <param name="definition">The definition of the function.</param>
        /// <param name="options">Options controlling scope-related elements of this object.</param>
        /// <param name="helpFile">The name of the help file associated with the function.</param>
        public SessionStateFunctionEntry(string name, string definition, ScopedItemOptions options, string helpFile)
            CommandType = CommandTypes.Function;
            ScriptBlock = ScriptBlock.Create(Definition);
            ScriptBlock.LanguageMode = PSLanguageMode.FullLanguage;
        public SessionStateFunctionEntry(string name, string definition, string helpFile)
            : this(name, definition, ScopedItemOptions.None, helpFile)
        public SessionStateFunctionEntry(string name, string definition)
            : this(name, definition, ScopedItemOptions.None, null)
        /// This is an internal copy constructor.
        internal SessionStateFunctionEntry(string name, string definition, ScopedItemOptions options,
            SessionStateEntryVisibility visibility, ScriptBlock scriptBlock, string helpFile)
        internal static SessionStateFunctionEntry GetDelayParsedFunctionEntry(string name, string definition, bool isProductCode, PSLanguageMode languageMode)
            var fnEntry = GetDelayParsedFunctionEntry(name, definition, isProductCode);
            fnEntry.ScriptBlock.LanguageMode = languageMode;
            return fnEntry;
        internal static SessionStateFunctionEntry GetDelayParsedFunctionEntry(string name, string definition, bool isProductCode)
            var sb = ScriptBlock.CreateDelayParsedScriptBlock(definition, isProductCode);
            return new SessionStateFunctionEntry(name, definition, ScopedItemOptions.None, SessionStateEntryVisibility.Public, sb, null);
        internal static SessionStateFunctionEntry GetDelayParsedFunctionEntry(string name, string definition, ScriptBlock sb)
            SessionStateFunctionEntry entry = new SessionStateFunctionEntry(Name, Definition, Options, Visibility, ScriptBlock, HelpFile);
        /// Sets the name of the help file associated with the function.
        internal void SetHelpFile(string help)
            HelpFile = help;
        /// The string to use to define this function...
        /// The script block for this function.
        internal ScriptBlock ScriptBlock { get; set; }
        public string HelpFile { get; private set; }
    public sealed class SessionStateVariableEntry : ConstrainedSessionStateEntry
        /// Is used to define a variable that should be created when
        /// the runspace is opened. Note - if this object is cloned,
        /// then the clone will contain a reference to the original object
        /// not a clone of it.
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The value to set the variable to.</param>
        /// <param name="description">A descriptive string to attach to the variable.</param>
        public SessionStateVariableEntry(string name, object value, string description)
        /// <param name="options">Options like readonly, constant, allscope, etc.</param>
        public SessionStateVariableEntry(string name, object value, string description, ScopedItemOptions options)
        /// <param name="attributes">A list of attributes to attach to the variable.</param>
        public SessionStateVariableEntry(string name, object value, string description, ScopedItemOptions options, Collection<Attribute> attributes)
            _attributes = attributes;
        /// <param name="attribute">A single attribute to attach to the variable.</param>
        public SessionStateVariableEntry(string name, object value, string description, ScopedItemOptions options, Attribute attribute)
            _attributes = new Collection<Attribute>();
            _attributes.Add(attribute);
        /// <param name="attributes">A single attribute to attach to the variable.</param>
        internal SessionStateVariableEntry(string name, object value, string description, ScopedItemOptions options, Collection<Attribute> attributes, SessionStateEntryVisibility visibility)
            // Copy the attribute collection if necessary...
            Collection<Attribute> attrs = null;
            if (_attributes != null && _attributes.Count > 0)
                attrs = new Collection<Attribute>(_attributes);
            return new SessionStateVariableEntry(Name, Value, Description, Options, attrs, Visibility);
        /// The value to bind to this variable.
        public object Value { get; }
        /// The description associated with this variable.
        /// The options associated with this variable (e.g. readonly, allscope, etc.)
        /// The attributes that will be attached to this object.
        public Collection<Attribute> Attributes
            get { return _attributes ??= new Collection<Attribute>(); }
        private Collection<Attribute> _attributes;
    /// <typeparam name="T"></typeparam>
    public sealed class InitialSessionStateEntryCollection<T> : IEnumerable<T> where T : InitialSessionStateEntry
        /// Create an empty collection...
        public InitialSessionStateEntryCollection()
            _internalCollection = new Collection<T>();
        /// Create an new collection, copying in the passed items...
        /// <param name="items"></param>
        public InitialSessionStateEntryCollection(IEnumerable<T> items)
            foreach (T item in items)
                _internalCollection.Add(item);
        /// Clone this collection.
        /// <returns>The cloned collection.</returns>
        public InitialSessionStateEntryCollection<T> Clone()
            InitialSessionStateEntryCollection<T> result;
                result = new InitialSessionStateEntryCollection<T>();
                foreach (T item in _internalCollection)
                    result.Add((T)item.Clone());
        /// Reset the collection.
                _internalCollection.Clear();
        /// Returns a count of the number of items in the collection...
            get { return _internalCollection.Count; }
        /// <param name="index"></param>
                T result;
                    result = _internalCollection[index];
        /// To find the entries based on name.
        /// Why collection - Different SnapIn/modules and same entity names.
        /// If used on command collection entry, then for the same name, one can have multiple output.
        public Collection<T> this[string name]
                Collection<T> result = new Collection<T>();
                    foreach (T element in _internalCollection)
                        if (element.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                            result.Add(element);
        /// Find entries based on string name which can include wildcards.
        internal Collection<T> LookUpByName(string name)
                throw new PSArgumentNullException(nameof(name));
                    if (namePattern.IsMatch(element.Name))
        public void RemoveItem(int index)
                _internalCollection.RemoveAt(index);
        /// Remove a number of items starting at the specified index.
        public void RemoveItem(int index, int count)
                while (count-- > 0)
        /// Clears the collection...
        /// This overload exists so that we can remove items based on the item name, rather than
        /// its position in the collection. The type argument can be null but we'll throw an error if
        /// we can't distinguish between multiple entries of the same name but different types
        /// and the type hasn't been specified.
        /// BUGBUG - brucepay - the throw thing is not implemented yet...
        /// <param name="name">The name of the element to remove.</param>
        /// <param name="type">The type of object to remove, can be null to remove any type.</param>
        public void Remove(string name, object type)
                Type objType = null;
                    objType = type as Type ?? type.GetType();
                // Work backwards through the collection...
                for (int i = _internalCollection.Count - 1; i >= 0; i--)
                    T element = _internalCollection[i];
                    if ((objType == null || element.GetType() == objType) &&
                        string.Equals(element.Name, name, StringComparison.OrdinalIgnoreCase))
                        _internalCollection.RemoveAt(i);
        /// Add an item to this collection.
        /// <param name="item">The item to add...</param>
        /// Add items to this collection.
        public void Add(IEnumerable<T> items)
                foreach (T element in items)
                    _internalCollection.Add(element);
        /// Get enumerator for this collection.
        /// Enumerator work is not thread safe by default. Any code trying
        /// to do enumeration on this collection should lock it first.
        /// Need to document this.
        IEnumerator System.Collections.IEnumerable.GetEnumerator()
            return _internalCollection.GetEnumerator();
        IEnumerator<T> System.Collections.Generic.IEnumerable<T>.GetEnumerator()
        private readonly Collection<T> _internalCollection;
        // object to use for locking
    /// Allows you to define the set of elements that should be
    /// present when Session State is created.
    public class InitialSessionState
        private static void RemoveDisallowedEntries<T>(InitialSessionStateEntryCollection<T> list, List<string> allowedNames, Func<T, string> nameGetter)
            where T : InitialSessionStateEntry
            List<string> namesToRemove = new List<string>();
            foreach (T entry in list)
                string entryName = nameGetter(entry);
                // if entryName is not present in allowedNames list, then remove it
                if (!allowedNames.Exists(allowedName => allowedName.Equals(entryName, StringComparison.OrdinalIgnoreCase)))
                    namesToRemove.Add(entry.Name);
            foreach (string nameToRemove in namesToRemove)
                list.Remove(nameToRemove, null /* remove any type with this name */);
        private static void MakeDisallowedEntriesPrivate<T>(InitialSessionStateEntryCollection<T> list, List<string> allowedNames, Func<T, string> nameGetter)
            where T : ConstrainedSessionStateEntry
                // Aliases to allowed commands are OK
                SessionStateAliasEntry aliasEntry = entry as SessionStateAliasEntry;
                if (aliasEntry != null)
                    if (allowedNames.Exists(allowedName => allowedName.Equals(aliasEntry.Definition, StringComparison.OrdinalIgnoreCase)))
                        aliasEntry.Visibility = SessionStateEntryVisibility.Public;
                    entry.Visibility = SessionStateEntryVisibility.Private;
        /// Creates an initial session state from a PSSC configuration file.
        /// <param name="path">The path to the PSSC session configuration file.</param>
        /// <returns>InitialSessionState object.</returns>
        public static InitialSessionState CreateFromSessionConfigurationFile(string path)
            return CreateFromSessionConfigurationFile(path, null);
        /// <param name="roleVerifier">
        /// The verifier that PowerShell should call to determine if groups in the Role entry apply to the
        /// target session. If you have a WindowsPrincipal for a user, for example, create a Function that
        /// checks windowsPrincipal.IsInRole().
        public static InitialSessionState CreateFromSessionConfigurationFile(
            Func<string, bool> roleVerifier)
            return CreateFromSessionConfigurationFile(path, roleVerifier, validateFile: false);
        /// <param name="validateFile">Validates the file contents for supported SessionState options.</param>
            Func<string, bool> roleVerifier,
            bool validateFile)
                throw new PSArgumentNullException(nameof(path));
                throw new PSInvalidOperationException(
                    StringUtil.Format(ConsoleInfoErrorStrings.ConfigurationFileDoesNotExist, path));
            if (!path.EndsWith(".pssc", StringComparison.OrdinalIgnoreCase))
                    StringUtil.Format(ConsoleInfoErrorStrings.NotConfigurationFile, path));
            Remoting.DISCPowerShellConfiguration discConfiguration = new Remoting.DISCPowerShellConfiguration(path, roleVerifier, validateFile);
            return discConfiguration.GetInitialSessionState(null);
        /// Creates an <see cref="InitialSessionState"/> instance that exposes only the minimal
        /// set of commands needed by give set of <paramref name="sessionCapabilities"/>.
        /// All commands that are not needed are made private in order to minimize the attack surface.
        /// <param name="sessionCapabilities">
        /// What capabilities the session should have.
        public static InitialSessionState CreateRestricted(SessionCapabilities sessionCapabilities)
            // only remote server has been requested
            if (sessionCapabilities == SessionCapabilities.RemoteServer)
                return CreateRestrictedForRemoteServer();
            return Create();
        private static InitialSessionState CreateRestrictedForRemoteServer()
            InitialSessionState iss = Create();
            iss.LanguageMode = PSLanguageMode.NoLanguage;
            iss.ThrowOnRunspaceOpenError = true;
            iss.UseFullLanguageModeInDebugger = false;
            iss.Commands.Add(BuiltInFunctions);
            iss.Commands.Add(BuiltInAliases);
            // Load the default snapins - all commands will be private by default.
            Collection<PSSnapInInfo> defaultSnapins = PSSnapInReader.ReadEnginePSSnapIns();
            foreach (PSSnapInInfo si in defaultSnapins)
                // ImportPSSnapIn always sets "out warning" to "null";  all our internal calls ignore/discard "out warning"
                PSSnapInException warning;
                iss.ImportPSSnapIn(si, out warning);
            // restrict what gets exposed
            List<string> allowedCommands = new List<string>();
            // required by implicit remoting and interactive remoting
            allowedCommands.Add("Get-Command");
            allowedCommands.Add("Get-FormatData");
            allowedCommands.Add("Clear-Host");
            allowedCommands.Add("Select-Object"); // used to serialize exactly the properties that we need (+ at the right depth)
            // used if available by implicit remoting
            allowedCommands.Add("Get-Help"); // used when displaying help for implicit remoting proxy functions
            allowedCommands.Add("Measure-Object"); // used to have nice progress bars when import/export-pssession is running
            // required by interactive remoting
            allowedCommands.Add("Out-Default"); // appended to every command line
            allowedCommands.Add("Exit-PSSession"); // used by the user to exit the session
            // We don't remove these entries so that they can be called by commands in the runspace.
            // Setting them to 'Private' ensures that the user can't call them.
            MakeDisallowedEntriesPrivate(
                iss.Commands,
                allowedCommands,
                commandEntry => commandEntry.Name);
            // Ensure that only PowerShell core formats are included in the restricted session.
            IncludePowerShellCoreFormats(iss);
            List<string> allowedTypes = new List<string>();
            allowedTypes.Add("types.ps1xml");
            allowedTypes.Add("typesV3.ps1xml");
            RemoveDisallowedEntries(
                iss.Types,
                allowedTypes,
                typeEntry => IO.Path.GetFileName(typeEntry.FileName));
            // No providers are visible by default
            foreach (SessionStateProviderEntry provider in iss.Providers)
                provider.Visibility = SessionStateEntryVisibility.Private;
            // Add built-in variables.
            iss.Variables.Add(BuiltInVariables);
            // wrap some commands in a proxy function to restrict their parameters
            foreach (KeyValuePair<string, CommandMetadata> proxyFunction in CommandMetadata.GetRestrictedCommands(SessionCapabilities.RemoteServer))
                string commandName = proxyFunction.Key;
                // make the cmdlet private
                Collection<SessionStateCommandEntry> originalCmdlet = iss.Commands[commandName];
                Diagnostics.Assert(originalCmdlet != null, "Proxied cmdlets should be imported at this point");
                Diagnostics.Assert(originalCmdlet.Count == 1, "Exactly one cmdlet with a given name");
                originalCmdlet[0].Visibility = SessionStateEntryVisibility.Private;
                // and add a public proxy function
                string proxyBody = ProxyCommand.Create(proxyFunction.Value, string.Empty, false);
                iss.Commands.Add(new SessionStateFunctionEntry(commandName, proxyBody));
            return iss;
        private static void IncludePowerShellCoreFormats(InitialSessionState iss)
            if (string.IsNullOrEmpty(psHome))
            iss.Formats.Clear();
            foreach (var coreFormat in Platform.FormatFileNames)
                iss.Formats.Add(new SessionStateFormatEntry(Path.Combine(psHome, coreFormat)));
        /// Ctor for Custom-Shell - Do we need this?
        protected InitialSessionState()
        // Creates an empty EE
        /// Creates an empty InitialSessionState object...
        public static InitialSessionState Create()
            InitialSessionState iss = new InitialSessionState();
            // TODO: the following code is probably needed for the hosted constrained runspace
            // There are too many things that depend on the built-in variables. At the same time,
            // these variables can't be public or they become a security issue.
            // This change still needs to be spec-reviewed before turning it on. It also seems to
            // be causing test failures - i suspect due to lack test isolation - brucepay Mar 06/2008
            // Add the default variables and make them private...
            iss.AddVariables(BuiltInVariables);
            foreach (SessionStateVariableEntry v in iss.Variables)
                v.Visibility = SessionStateEntryVisibility.Private;
        /// Creates the default PowerShell one with default cmdlets, provider etc.
        /// BuiltIn functions, aliases need to be available through default
        /// InitialSessionstate constructor. Need to have this discussion for packaging as well.
        public static InitialSessionState CreateDefault()
            // Read all of the registered PSSnapins
            Collection<PSSnapInInfo> defaultSnapins;
            InitialSessionState ss = new InitialSessionState();
            ss.Variables.Add(BuiltInVariables);
            ss.Commands.Add(new SessionStateApplicationEntry("*"));
            ss.Commands.Add(new SessionStateScriptEntry("*"));
            ss.Commands.Add(BuiltInFunctions);
            ss.Commands.Add(BuiltInAliases);
            defaultSnapins = PSSnapInReader.ReadEnginePSSnapIns();
                    ss.ImportPSSnapIn(si, out warning);
                catch (PSSnapInException)
                // This code is for testing a module-based shell. It is only available when the shell is complied
                // in debug mode and is not intended to be a feature.
                // July 31 2008 - brucepay
                // Only load the core snapins at this point...
                if (Environment.GetEnvironmentVariable("PowerShellMinimal") != null)
                    if (si.Name.Equals("Microsoft.PowerShell.Host", StringComparison.OrdinalIgnoreCase))
            // Remove duplicated assemblies
            HashSet<string> assemblyList = new HashSet<string>();
            for (int i = ss.Assemblies.Count - 1; i >= 0; i--)
                string assembly = ss.Assemblies[i].FileName;
                if (!string.IsNullOrEmpty(assembly))
                    if (!assemblyList.Add(assembly))
                        ss.Assemblies.RemoveItem(i);
            ss.LanguageMode = PSLanguageMode.FullLanguage;
            ss.AuthorizationManager = new Microsoft.PowerShell.PSAuthorizationManager(Utils.DefaultPowerShellShellID);
            return ss.Clone();
        /// The default cmdlets, provider, etc are loaded via Modules.
        /// For loading Microsoft.PowerShell.Core module only.
        public static InitialSessionState CreateDefault2()
            ss.ImportCorePSSnapIn();
        internal static bool IsEngineModule(string moduleName)
            return EngineModules.Contains(moduleName) || NestedEngineModules.Contains(moduleName);
        internal static bool IsNestedEngineModule(string moduleName)
            return NestedEngineModules.Contains(moduleName);
        internal static bool IsConstantEngineModule(string moduleName)
            return ConstantEngineModules.Contains(moduleName) || ConstantEngineNestedModules.Contains(moduleName);
        /// Clone this InitialSessionState object. The collections are
        /// recursively cloned as well as the elements in the collections.
        /// Note however, that the contents of the individual entries
        /// are not deep-cloned. This is only an issue for variable
        /// entries which may have reference types. These objects
        /// will be added by reference rather than by value.
        public InitialSessionState Clone()
            ss.Variables.Add(this.Variables.Clone());
            ss.EnvironmentVariables.Add(this.EnvironmentVariables.Clone());
            ss.Commands.Add(this.Commands.Clone());
            ss.Assemblies.Add(this.Assemblies.Clone());
            ss.Providers.Add(this.Providers.Clone());
            ss.Types.Add(this.Types.Clone());
            ss.Formats.Add(this.Formats.Clone());
            foreach (string startupScript in this.StartupScripts)
                ss.StartupScripts.Add(startupScript);
            foreach (string unresolvedCommandsToExpose in this.UnresolvedCommandsToExpose)
                ss.UnresolvedCommandsToExpose.Add(unresolvedCommandsToExpose);
            foreach (Hashtable dynamicVariableToDefine in this.DynamicVariablesToDefine)
                ss.DynamicVariablesToDefine.Add(dynamicVariableToDefine);
            foreach (var pair in this.CommandModifications)
                ss.CommandModifications.Add(pair.Key, pair.Value);
            ss.DefaultCommandVisibility = this.DefaultCommandVisibility;
            ss.AuthorizationManager = this.AuthorizationManager;
            ss.LanguageMode = this.LanguageMode;
            ss.TranscriptDirectory = this.TranscriptDirectory;
            ss.UserDriveEnabled = this.UserDriveEnabled;
            ss.UserDriveUserName = this.UserDriveUserName;
            ss.UserDriveMaximumSize = this.UserDriveMaximumSize;
            if (_wasExecutionPolicySet)
                ss.ExecutionPolicy = this.ExecutionPolicy;
            ss.UseFullLanguageModeInDebugger = this.UseFullLanguageModeInDebugger;
            ss.ThreadOptions = this.ThreadOptions;
            ss.ThrowOnRunspaceOpenError = this.ThrowOnRunspaceOpenError;
            ss.ApartmentState = this.ApartmentState;
            foreach (ModuleSpecification modSpec in this.ModuleSpecificationsToImport)
                ss.ModuleSpecificationsToImport.Add(modSpec);
            foreach (string mod in this.CoreModulesToImport)
                ss.CoreModulesToImport.Add(mod);
            ss.DisableFormatUpdates = this.DisableFormatUpdates;
            foreach (var s in ImportedSnapins)
                ss.ImportedSnapins.Add(s.Key, s.Value);
        /// Want to get away from SnapIn and console file. Have modules and assemblies instead.
        /// Specify the registered SnapIn name or name collection.
        /// <param name="snapInName"></param>
        public static InitialSessionState Create(string snapInName)
            return new InitialSessionState();
        /// <param name="snapInNameCollection"></param>
        /// <param name="warning"></param>
        public static InitialSessionState Create(string[] snapInNameCollection, out PSConsoleLoadException warning)
            warning = null;
        /// <param name="snapInPath"></param>
        /// <param name="warnings"></param>
        public static InitialSessionState CreateFrom(string snapInPath, out PSConsoleLoadException warnings)
            warnings = null;
        /// <param name="snapInPathCollection"></param>
        public static InitialSessionState CreateFrom(string[] snapInPathCollection, out PSConsoleLoadException warnings)
        /// Specifies the language mode to be used for this session state instance.
        public PSLanguageMode LanguageMode { get; set; } = PSLanguageMode.NoLanguage;
        /// Specifies the directory to be used for collection session transcripts.
        public string TranscriptDirectory { get; set; } = null;
        /// True when session opted for a User PSDrive.
        internal bool UserDriveEnabled
        /// User name for the user drive.  This will be part of the root path for the User PSDrive.
        internal string UserDriveUserName
        /// Optional maximum size value for User drive (in bytes).
        internal long UserDriveMaximumSize
        /// Forces all session script input parameters to have validation.
        internal bool EnforceInputParameterValidation
        /// Specifies the execution policy to be used for this session state instance.
        public Microsoft.PowerShell.ExecutionPolicy ExecutionPolicy
                _executionPolicy = value;
                _wasExecutionPolicySet = true;
        private Microsoft.PowerShell.ExecutionPolicy _executionPolicy = Microsoft.PowerShell.ExecutionPolicy.Default;
        private bool _wasExecutionPolicySet = false;
        public bool UseFullLanguageModeInDebugger { get; set; } = false;
        /// ApartmentState of the thread used to execute commands.
        public ApartmentState ApartmentState { get; set; } = Runspace.DefaultApartmentState;
        /// This property determines whether a new thread is created for each invocation of a command.
        public PSThreadOptions ThreadOptions { get; set; } = PSThreadOptions.Default;
        /// If this property is set and there was a runspace creation error, then
        /// throw an exception, otherwise just continue creating the runspace even though it may
        /// be in an inconsistent state.
        public bool ThrowOnRunspaceOpenError { get; set; } = false;
        /// This property will be set only if we are refreshing the Type/Format settings by calling UpdateTypes/UpdateFormats directly.
        /// In this case, we should wait until all type/format entries get processed. After that, if there were errors
        /// generated, we throw them as an exception.
        internal bool RefreshTypeAndFormatSetting = false;
        /// Specifies the authorization manager to be used for this session state instance.
        /// If no authorization manager is specified, then the default authorization manager
        /// for PowerShell will be used which checks the ExecutionPolicy before running a command.
        public virtual AuthorizationManager AuthorizationManager { get; set; } = new Microsoft.PowerShell.PSAuthorizationManager(Utils.DefaultPowerShellShellID);
        internal PSHost Host = null;
        /// Add a list of modules to import when the runspace is created.
        /// <param name="name">The modules to add.</param>
        public void ImportPSModule(params string[] name)
            foreach (string n in name)
                ModuleSpecificationsToImport.Add(new ModuleSpecification(n));
        /// Clears ImportPSModule list.
        internal void ClearPSModules()
            ModuleSpecificationsToImport.Clear();
        /// <param name="modules">
        /// The modules, whose specifications are specified by <paramref name="modules"/>,
        /// to add.
        public void ImportPSModule(IEnumerable<ModuleSpecification> modules)
            ArgumentNullException.ThrowIfNull(modules);
            foreach (var moduleSpecification in modules)
                ModuleSpecificationsToImport.Add(moduleSpecification);
        /// Imports all the modules from the specified module path by default.
        /// Path from which all modules need to be imported.
        public void ImportPSModulesFromPath(string path)
            string expandedpath = Environment.ExpandEnvironmentVariables(path);
            var availableModuleFiles = ModuleUtils.GetDefaultAvailableModuleFiles(expandedpath);
            ImportPSModule(availableModuleFiles.ToArray());
        /// Add a list of core modules to import when the runspace is created.
        internal void ImportPSCoreModule(string[] name)
                CoreModulesToImport.Add(n);
        /// Imported modules.
        public ReadOnlyCollection<ModuleSpecification> Modules
            get { return new ReadOnlyCollection<ModuleSpecification>(ModuleSpecificationsToImport); }
        internal Collection<ModuleSpecification> ModuleSpecificationsToImport { get; } = new Collection<ModuleSpecification>();
        internal Dictionary<string, PSSnapInInfo> ImportedSnapins { get; } = new Dictionary<string, PSSnapInInfo>(StringComparer.OrdinalIgnoreCase);
        /// Gets the dictionary of core modules to import on runspace creation...
        internal HashSet<string> CoreModulesToImport { get; } = new HashSet<string>();
        /// The list of assemblies to load...
        public virtual InitialSessionStateEntryCollection<SessionStateAssemblyEntry> Assemblies
                if (_assemblies == null)
                    Interlocked.CompareExchange(ref _assemblies, new InitialSessionStateEntryCollection<SessionStateAssemblyEntry>(), null);
                return _assemblies;
        private InitialSessionStateEntryCollection<SessionStateAssemblyEntry> _assemblies;
        /// List of types to use for this session state instance...
        public virtual InitialSessionStateEntryCollection<SessionStateTypeEntry> Types
                if (_types == null)
                    Interlocked.CompareExchange(ref _types, new InitialSessionStateEntryCollection<SessionStateTypeEntry>(), null);
        private InitialSessionStateEntryCollection<SessionStateTypeEntry> _types;
        public virtual InitialSessionStateEntryCollection<SessionStateFormatEntry> Formats
                if (_formats == null)
                    Interlocked.CompareExchange(ref _formats, new InitialSessionStateEntryCollection<SessionStateFormatEntry>(), null);
        private InitialSessionStateEntryCollection<SessionStateFormatEntry> _formats;
        /// If set to true, disables any updates to format table. This includes disabling
        /// format table updates through Update-FormatData, Import-Module etc.
        /// All the disabling happens silently ie., the user will not get any exception.
        /// By default, this is set to False.
        public bool DisableFormatUpdates { get; set; }
        public virtual InitialSessionStateEntryCollection<SessionStateProviderEntry> Providers
                if (_providers == null)
                    Interlocked.CompareExchange(ref _providers, new InitialSessionStateEntryCollection<SessionStateProviderEntry>(), null);
                return _providers;
        private InitialSessionStateEntryCollection<SessionStateProviderEntry> _providers;
        /// List of commands (Alias, Application, Cmdlets, Function, Script) for this entry.
        public virtual InitialSessionStateEntryCollection<SessionStateCommandEntry> Commands
                if (_commands == null)
                    Interlocked.CompareExchange(ref _commands, new InitialSessionStateEntryCollection<SessionStateCommandEntry>(), null);
                return _commands;
        private InitialSessionStateEntryCollection<SessionStateCommandEntry> _commands;
        internal SessionStateEntryVisibility DefaultCommandVisibility { get; set; }
        internal HashSet<string> UnresolvedCommandsToExpose
                if (_unresolvedCommandsToExpose == null)
                    Interlocked.CompareExchange(ref _unresolvedCommandsToExpose, new HashSet<string>(StringComparer.OrdinalIgnoreCase), null);
                return _unresolvedCommandsToExpose;
        private HashSet<string> _unresolvedCommandsToExpose;
        internal Dictionary<string, Hashtable> CommandModifications
                if (_commandModifications == null)
                    Interlocked.CompareExchange(ref _commandModifications, new Dictionary<string, Hashtable>(StringComparer.OrdinalIgnoreCase), null);
                return _commandModifications;
        private Dictionary<string, Hashtable> _commandModifications;
        internal List<Hashtable> DynamicVariablesToDefine
                if (_dynamicVariablesToDefine == null)
                    Interlocked.CompareExchange(ref _dynamicVariablesToDefine, new List<Hashtable>(), null);
                return _dynamicVariablesToDefine;
        private List<Hashtable> _dynamicVariablesToDefine;
        public virtual InitialSessionStateEntryCollection<SessionStateVariableEntry> Variables
                if (_variables == null)
                    Interlocked.CompareExchange(ref _variables, new InitialSessionStateEntryCollection<SessionStateVariableEntry>(), null);
                return _variables;
        private InitialSessionStateEntryCollection<SessionStateVariableEntry> _variables;
        public virtual InitialSessionStateEntryCollection<SessionStateVariableEntry> EnvironmentVariables
                if (_environmentVariables == null)
                    Interlocked.CompareExchange(ref _environmentVariables, new InitialSessionStateEntryCollection<SessionStateVariableEntry>(), null);
                return _environmentVariables;
        private InitialSessionStateEntryCollection<SessionStateVariableEntry> _environmentVariables;
        public virtual HashSet<string> StartupScripts
                if (_startupScripts == null)
                    Interlocked.CompareExchange(ref _startupScripts, new HashSet<string>(), null);
                return _startupScripts;
        private HashSet<string> _startupScripts = new HashSet<string>();
        internal void Bind(ExecutionContext context, bool updateOnly, PSModuleInfo module, bool noClobber, bool local, bool setLocation)
            Host = context.EngineHostInterface;
                SessionStateInternal ss = context.EngineSessionState;
                // Clear the application and script collections...
                if (!updateOnly)
                    ss.Applications.Clear();
                    ss.Scripts.Clear();
                // If the initial session state made some commands private by way of
                // VisibleCmdlets / etc., then change the default command visibility for
                // the session state so that newly imported modules aren't exposed accidentally.
                if (DefaultCommandVisibility == SessionStateEntryVisibility.Private)
                    ss.DefaultCommandVisibility = SessionStateEntryVisibility.Private;
                    // Load assemblies before anything else - we may need to resolve types in the loaded
                    // assemblies as part of loading formats or types, and that can't be done in parallel.
                    Bind_LoadAssemblies(context);
                    var actions = new Action[]
                        () => Bind_UpdateTypes(context, updateOnly),
                        () => Bind_UpdateFormats(context, updateOnly),
                        () => Bind_BindCommands(module, noClobber, local, ss),
                        () => Bind_LoadProviders(ss),
                        () => Bind_SetVariables(ss),
                        Bind_SetEnvironment
                    if (updateOnly)
                        // We're typically called to import a module. It seems like this could
                        // still happen in parallel, but calls to WriteError on the wrong thread
                        // get silently swallowed, so instead just run the actions serially on this thread.
                        foreach (var action in actions)
                        Parallel.Invoke(actions);
                catch (AggregateException e)
                    e = e.Flatten();
                    foreach (var exception in e.InnerExceptions)
                        MshLog.LogEngineHealthEvent(
                            MshLog.EVENT_ID_CONFIGURATION_FAILURE,
                    if (this.ThrowOnRunspaceOpenError)
                        // Just throw the first error
                        throw e.InnerExceptions[0];
                    context.ReportEngineStartupError(e.Message);
                // Set the language mode
                    ss.LanguageMode = LanguageMode;
                // Set the execution policy
                    string shellId = context.ShellID;
                    SecuritySupport.SetExecutionPolicy(Microsoft.PowerShell.ExecutionPolicyScope.Process, ExecutionPolicy, shellId);
            SetSessionStateDrive(context, setLocation: setLocation);
        private void Bind_SetVariables(SessionStateInternal ss)
            bool etwEnabled = RunspaceEventSource.Log.IsEnabled();
                RunspaceEventSource.Log.LoadVariablesStart();
            // Add all of the variables to session state...
            foreach (SessionStateVariableEntry var in Variables)
                ss.AddSessionStateEntry(var);
                RunspaceEventSource.Log.LoadVariablesStop();
        private void Bind_SetEnvironment()
                RunspaceEventSource.Log.LoadEnvironmentVariablesStart();
            foreach (SessionStateVariableEntry var in EnvironmentVariables)
                Environment.SetEnvironmentVariable(var.Name, var.Value.ToString());
                RunspaceEventSource.Log.LoadEnvironmentVariablesStop();
        private void Bind_UpdateTypes(ExecutionContext context, bool updateOnly)
                RunspaceEventSource.Log.UpdateTypeTableStart();
            this.UpdateTypes(context, updateOnly);
                RunspaceEventSource.Log.UpdateTypeTableStop();
        private void Bind_UpdateFormats(ExecutionContext context, bool updateOnly)
                RunspaceEventSource.Log.UpdateFormatTableStart();
            this.UpdateFormats(context, updateOnly);
                RunspaceEventSource.Log.UpdateFormatTableStop();
        private void Bind_LoadProviders(SessionStateInternal ss)
                RunspaceEventSource.Log.LoadProvidersStart();
            // Add all of the providers to session state...
            foreach (SessionStateProviderEntry provider in Providers)
                    RunspaceEventSource.Log.LoadProviderStart(provider.Name);
                ss.AddSessionStateEntry(provider);
                    RunspaceEventSource.Log.LoadProviderStop(provider.Name);
                RunspaceEventSource.Log.LoadProvidersStop();
        private void Bind_BindCommands(PSModuleInfo module, bool noClobber, bool local, SessionStateInternal ss)
                RunspaceEventSource.Log.LoadCommandsStart();
            foreach (SessionStateCommandEntry cmd in Commands)
                    RunspaceEventSource.Log.LoadCommandStart(cmd.Name);
                SessionStateCmdletEntry ssce = cmd as SessionStateCmdletEntry;
                if (ssce != null)
                    if (noClobber && ModuleCmdletBase.CommandFound(ssce.Name, ss))
                        ssce._isImported = false;
                    ss.AddSessionStateEntry(ssce, local);
                    cmd.SetModule(module);
                SessionStateFunctionEntry ssfe = cmd as SessionStateFunctionEntry;
                if (ssfe != null)
                    ss.AddSessionStateEntry(ssfe);
                SessionStateAliasEntry ssae = cmd as SessionStateAliasEntry;
                if (ssae != null)
                    ss.AddSessionStateEntry(ssae, StringLiterals.Local);
                SessionStateApplicationEntry ssappe = cmd as SessionStateApplicationEntry;
                if (ssappe != null)
                    if (ssappe.Visibility == SessionStateEntryVisibility.Public)
                        ss.AddSessionStateEntry(ssappe);
                SessionStateScriptEntry ssse = cmd as SessionStateScriptEntry;
                if (ssse != null)
                    if (ssse.Visibility == SessionStateEntryVisibility.Public)
                        ss.AddSessionStateEntry(ssse);
                    RunspaceEventSource.Log.LoadCommandStop(cmd.Name);
                RunspaceEventSource.Log.LoadCommandsStop();
        private void Bind_LoadAssemblies(ExecutionContext context)
                RunspaceEventSource.Log.LoadAssembliesStart();
            // Load the assemblies and initialize the assembly cache...
            foreach (SessionStateAssemblyEntry ssae in Assemblies)
                    RunspaceEventSource.Log.LoadAssemblyStart(ssae.Name, ssae.FileName);
                // Specify the source only if this is for module loading.
                // The source is used for proper cleaning of the assembly cache when a module is unloaded.
                Assembly asm = context.AddAssembly(ssae.Module?.Name, ssae.Name, ssae.FileName, out Exception error);
                if (asm == null || error != null)
                    // If no module was found but there was no specific error, then
                    // create a not found error.
                    if (error == null)
                        string msg = StringUtil.Format(global::Modules.ModuleAssemblyFound, ssae.Name);
                        error = new DllNotFoundException(msg);
                    // If this occurs while loading a module manifest, just
                    // throw the exception instead of writing it out...
                    if ((!string.IsNullOrEmpty(context.ModuleBeingProcessed) &&
                         Path.GetExtension(context.ModuleBeingProcessed)
                             .Equals(
                                 StringLiterals.PowerShellDataFileExtension,
                                 StringComparison.OrdinalIgnoreCase)) ||
                        ThrowOnRunspaceOpenError)
                        throw error;
                        context.ReportEngineStartupError(error.Message);
                    RunspaceEventSource.Log.LoadAssemblyStop(ssae.Name, ssae.FileName);
                RunspaceEventSource.Log.LoadAssembliesStop();
        internal Exception BindRunspace(Runspace initializedRunspace, PSTraceSource runspaceInitTracer)
            // Get the initial list of public commands from session in a lazy way, so that we can defer
            // the work until it's actually needed.
            // We could use Lazy<> with an initializer for the same purpose, but we can save allocations
            // by using the local function. It avoids allocating the delegate, and it's more efficient on
            // capturing variables from the enclosing scope by using a struct.
            HashSet<CommandInfo> publicCommands = null;
            HashSet<CommandInfo> GetPublicCommands()
                if (publicCommands != null)
                    return publicCommands;
                publicCommands = new HashSet<CommandInfo>();
                foreach (CommandInfo sessionCommand in initializedRunspace.ExecutionContext.SessionState.InvokeCommand.GetCommands(
                            name: "*",
                            CommandTypes.Alias | CommandTypes.Function | CommandTypes.Filter | CommandTypes.Cmdlet,
                            nameIsPattern: true))
                    if (sessionCommand.Visibility == SessionStateEntryVisibility.Public)
                        publicCommands.Add(sessionCommand);
            var unresolvedCmdsToExpose = new HashSet<string>(this.UnresolvedCommandsToExpose, StringComparer.OrdinalIgnoreCase);
            if (CoreModulesToImport.Count > 0 || unresolvedCmdsToExpose.Count > 0)
                // If a user has any module with the same name as that of the core module( or nested module inside the core module)
                // in his module path, then that will get loaded instead of the actual nested module (from the GAC - in our case)
                // Hence, searching only from the system module path while loading the core modules
                ProcessModulesToImport(initializedRunspace, CoreModulesToImport, ModuleIntrinsics.GetPSHomeModulePath(), GetPublicCommands(), unresolvedCmdsToExpose);
            // Win8:328748 - functions defined in global scope end up in a module
            // Since we import the core modules, EngineSessionState's module is set to the last imported module. So, if a function is defined in global scope, it ends up in that module.
            // Setting the module to null fixes that.
            initializedRunspace.ExecutionContext.EngineSessionState.Module = null;
            if (ModuleSpecificationsToImport.Count > 0 || unresolvedCmdsToExpose.Count > 0)
                Exception moduleImportException = ProcessModulesToImport(initializedRunspace, ModuleSpecificationsToImport, string.Empty, GetPublicCommands(), unresolvedCmdsToExpose);
                if (moduleImportException != null)
                    runspaceInitTracer.WriteLine(
                        "Runspace open failed while loading module: First error {1}",
                        moduleImportException);
                    return moduleImportException;
            // If we still have unresolved commands after importing specified modules, then try finding associated module for
            // each unresolved command and import that module.
            if (unresolvedCmdsToExpose.Count > 0)
                string[] foundModuleList = GetModulesForUnResolvedCommands(unresolvedCmdsToExpose, initializedRunspace.ExecutionContext);
                if (foundModuleList.Length > 0)
                    ProcessModulesToImport(initializedRunspace, foundModuleList, string.Empty, GetPublicCommands(), unresolvedCmdsToExpose);
            // Process dynamic variables if any are defined.
            if (DynamicVariablesToDefine.Count > 0)
                ProcessDynamicVariables(initializedRunspace);
            // Process command modifications if any are defined.
            if (CommandModifications.Count > 0)
                ProcessCommandModifications(initializedRunspace);
            // Process the 'User:' drive if 'UserDriveEnabled' is set.
            if (UserDriveEnabled)
                Exception userDriveException = ProcessUserDrive(initializedRunspace);
                if (userDriveException != null)
                        "Runspace open failed while processing user drive with error {1}",
                        userDriveException);
                    Exception result = PSTraceSource.NewInvalidOperationException(userDriveException, RemotingErrorIdStrings.UserDriveProcessingThrewTerminatingError, userDriveException.Message);
            // Process startup scripts
            if (StartupScripts.Count > 0)
                Exception startupScriptException = ProcessStartupScripts(initializedRunspace);
                if (startupScriptException != null)
                        "Runspace open failed while running startup script: First error {1}",
                        startupScriptException);
                    Exception result = PSTraceSource.NewInvalidOperationException(startupScriptException, RemotingErrorIdStrings.StartupScriptThrewTerminatingError, startupScriptException.Message);
            // Start transcribing
            if (!string.IsNullOrEmpty(TranscriptDirectory))
                using (PowerShell psToInvoke = PowerShell.Create())
                    psToInvoke.AddCommand(new Command("Start-Transcript")).AddParameter("OutputDirectory", TranscriptDirectory);
                    Exception exceptionToReturn = ProcessPowerShellCommand(psToInvoke, initializedRunspace);
                    if (exceptionToReturn != null)
                        // ThrowOnRunspaceOpenError handling is done by ProcessPowerShellCommand
                        return exceptionToReturn;
        private static string[] GetModulesForUnResolvedCommands(IEnumerable<string> unresolvedCommands, ExecutionContext context)
            Collection<string> modulesToImport = new Collection<string>();
            HashSet<string> commandsToResolve = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var unresolvedCommand in unresolvedCommands)
                string command = Utils.ParseCommandName(unresolvedCommand, out moduleName);
                    // Skip fully qualified module names since they are already processed.
                if (WildcardPattern.ContainsWildcardCharacters(command))
                    // Skip names with wild cards.
                commandsToResolve.Add(command);
            if (commandsToResolve.Count > 0)
                Runspace restoreRunspace = Runspace.DefaultRunspace;
                    // Create a temporary default runspace for the analysis cache to use.
                    using (Runspace tempRunspace = RunspaceFactory.CreateRunspace())
                        tempRunspace.Open();
                        Runspace.DefaultRunspace = tempRunspace;
                        foreach (var unresolvedCommand in commandsToResolve)
                            // Use the analysis cache to find the first module containing the unresolved command.
                            foreach (string modulePath in ModuleUtils.GetDefaultAvailableModuleFiles(isForAutoDiscovery: true, context))
                                string expandedModulePath = IO.Path.GetFullPath(modulePath);
                                if (exportedCommands != null && exportedCommands.ContainsKey(unresolvedCommand))
                                    modulesToImport.Add(System.IO.Path.GetFileNameWithoutExtension(expandedModulePath));
                    Runspace.DefaultRunspace = restoreRunspace;
            return modulesToImport.ToArray<string>();
        private void ProcessCommandModifications(Runspace initializedRunspace)
            foreach (var pair in CommandModifications)
                string commandName = pair.Key;
                Hashtable commandModification = pair.Value;
                CommandInfo existingCommand = initializedRunspace.SessionStateProxy.InvokeCommand.GetCommand(commandName, CommandTypes.Cmdlet | CommandTypes.Function);
                if (existingCommand == null)
                    // Could not find the command - just continue, rather than generating an error. This could just be a missing module
                    // or something similar.
                // If we are wrapping a function, rename it.
                FunctionInfo commandAsFunction = existingCommand as FunctionInfo;
                if (commandAsFunction != null)
                    string newCommandName = commandAsFunction.Name + "_" + Guid.NewGuid().ToString("N");
                    commandAsFunction.Rename(newCommandName);
                    initializedRunspace.ExecutionContext.EngineSessionState.GlobalScope.FunctionTable.Add(newCommandName, commandAsFunction);
                    initializedRunspace.ExecutionContext.EngineSessionState.GlobalScope.FunctionTable.Remove(commandName);
                    existingCommand = initializedRunspace.SessionStateProxy.InvokeCommand.GetCommand(newCommandName, CommandTypes.Function);
                CommandMetadata metadata = new CommandMetadata(existingCommand);
                List<string> unprocessedCommandModifications = new List<string>();
                foreach (string commandModificationParameter in commandModification.Keys)
                    unprocessedCommandModifications.Add(commandModificationParameter);
                // Visit all parameters of the command we're wrapping
                foreach (string existingParameter in metadata.Parameters.Keys.ToArray<string>())
                    // If it's not allowed, remove it
                    if (!commandModification.ContainsKey(existingParameter))
                        metadata.Parameters.Remove(existingParameter);
                        // Remember that we've processed this parameter, so that we can add the remainder
                        // as virtual command modifications over (what we must assume to be) dynamic parameters.
                        unprocessedCommandModifications.Remove(existingParameter);
                        ProcessCommandModification(commandModification, metadata, existingParameter);
                // Now, process the command modifications that the user requested (but there was no parameter
                // in the cmdlet that matched the requested parameter)
                foreach (string unprocessedCommandModification in unprocessedCommandModifications)
                    ProcessCommandModification(commandModification, metadata, unprocessedCommandModification);
                string proxyBody = ProxyCommand.Create(metadata, string.Empty, false);
                ScriptBlock proxyScriptBlock = ScriptBlock.Create(proxyBody);
                proxyScriptBlock.LanguageMode = PSLanguageMode.FullLanguage;
                initializedRunspace.ExecutionContext.EngineSessionState.GlobalScope.FunctionTable.Add(
                    commandName, new FunctionInfo(commandName, proxyScriptBlock, initializedRunspace.ExecutionContext));
        /// Process a command modification for a specific parameter.
        /// <param name="commandModification">The hashtable of command modifications for this command.</param>
        /// <param name="metadata">The metadata for the command being processed.</param>
        /// <param name="parameterName">The parameter being modified.</param>
        private static void ProcessCommandModification(Hashtable commandModification, CommandMetadata metadata, string parameterName)
            // If the metadata doesn't actually contain the parameter, then we need to create one.
            if (!metadata.Parameters.ContainsKey(parameterName))
                metadata.Parameters[parameterName] = new ParameterMetadata(parameterName);
            // Add validation attributes
            Hashtable parameterValidations = (Hashtable)commandModification[parameterName];
            foreach (object parameterValidation in parameterValidations.Keys)
                string[] parameterValidationValues =
                    ((HashSet<string>)parameterValidations[parameterValidation]).ToList<string>().ToArray();
                switch (parameterValidation.ToString())
                    case "ValidateSet":
                        ValidateSetAttribute validateSet = new ValidateSetAttribute(parameterValidationValues);
                        metadata.Parameters[parameterName].Attributes.Add(validateSet);
                    case "ValidatePattern":
                        string pattern = "^(" + string.Join('|', parameterValidationValues) + ")$";
                        ValidatePatternAttribute validatePattern = new ValidatePatternAttribute(pattern);
                        metadata.Parameters[parameterName].Attributes.Add(validatePattern);
        private Exception ProcessDynamicVariables(Runspace initializedRunspace)
            foreach (Hashtable variable in DynamicVariablesToDefine)
                if (variable.ContainsKey("Name"))
                    string name = variable["Name"].ToString();
                    ScriptBlock sb = variable["Value"] as ScriptBlock;
                    if (!string.IsNullOrEmpty(name) && (sb != null))
                        sb.SessionStateInternal = initializedRunspace.ExecutionContext.EngineSessionState;
                            psToInvoke.AddCommand(new Command("Invoke-Command")).AddParameter("ScriptBlock", sb).AddParameter("NoNewScope");
                            psToInvoke.AddCommand(new Command("Set-Variable")).AddParameter("Name", name);
        private Exception ProcessUserDrive(Runspace initializedRunspace)
                List<ProviderInfo> fileSystemProviders = initializedRunspace.ExecutionContext.EngineSessionState.Providers["FileSystem"];
                if (fileSystemProviders.Count == 0)
                    throw new PSInvalidOperationException(RemotingErrorIdStrings.UserDriveCannotGetFileSystemProvider);
                // Create the User drive path directory in current user local appdata location:
                // SystemDrive\Users\[user]\AppData\Local\Microsoft\PowerShell\DriveRoots\[UserName]
                // Or for virtual accounts
                // WinDir\System32\Microsoft\PowerShell\DriveRoots\[UserName]
                string directoryName = MakeUserNamePath();
                string userDrivePath = Path.Combine(Platform.CacheDirectory, "DriveRoots", directoryName);
                // Create directory if it doesn't exist.
                if (!System.IO.Directory.Exists(userDrivePath))
                    System.IO.Directory.CreateDirectory(userDrivePath);
                // Create the PSDrive.
                var newDriveInfo = new PSDriveInfo(
                    "User",
                    fileSystemProviders[0],
                    userDrivePath,
                var userDriveInfo = initializedRunspace.ExecutionContext.SessionState.Drive.New(newDriveInfo, null);
                // Set User drive maximum size.  Default maximum size is 50MB
                userDriveInfo.MaximumSize = (UserDriveMaximumSize > 0) ? UserDriveMaximumSize : 50000000;
            catch (ArgumentNullException e) { ex = e; }
            catch (ArgumentException e) { ex = e; }
            catch (NotSupportedException e) { ex = e; }
            catch (ProviderNotFoundException e) { ex = e; }
            catch (ProviderInvocationException e) { ex = e; }
            catch (KeyNotFoundException e) { ex = e; }
            catch (IOException e) { ex = e; }
            catch (UnauthorizedAccessException e) { ex = e; }
        private string MakeUserNamePath()
            // Use the user name passed to initial session state if available, or
            // otherwise use the current user name.
            var userName = !string.IsNullOrEmpty(this.UserDriveUserName)
                ? this.UserDriveUserName
                : Environment.UserName;
                : Environment.UserDomainName + "_" + Environment.UserName;
            // Ensure that user name contains no invalid path characters.
            // MSDN indicates that logon names cannot contain any of these invalid characters,
            // but this check will ensure safety.
            if (PathUtils.ContainsInvalidPathChars(userName))
                throw new PSInvalidOperationException(RemotingErrorIdStrings.InvalidUserDriveName);
        private Exception ProcessStartupScripts(Runspace initializedRunspace)
            foreach (string startupScript in StartupScripts)
                    psToInvoke.AddCommand(new Command(startupScript, false, false));
        private Exception ProcessPowerShellCommand(PowerShell psToInvoke, Runspace initializedRunspace)
            PSLanguageMode originalLanguageMode = initializedRunspace.SessionStateProxy.LanguageMode;
                initializedRunspace.SessionStateProxy.LanguageMode = PSLanguageMode.FullLanguage;
                psToInvoke.Runspace = initializedRunspace;
                foreach (Command command in psToInvoke.Commands.Commands)
                    command.CommandOrigin = CommandOrigin.Internal;
                    psToInvoke.Invoke();
                    if (ThrowOnRunspaceOpenError)
                // Restore the language mode, but not if it was altered by the startup script itself.
                if (initializedRunspace.SessionStateProxy.LanguageMode == PSLanguageMode.FullLanguage)
                    initializedRunspace.SessionStateProxy.LanguageMode = originalLanguageMode;
                // find out if there are any error records reported. If there is one, report the error..
                // this will result in the runspace getting closed/broken.
                ArrayList errorList = (ArrayList)initializedRunspace.GetExecutionContext.DollarErrorVariable;
                if (errorList.Count > 0)
                    ErrorRecord lastErrorRecord = errorList[0] as ErrorRecord;
                    if (lastErrorRecord != null)
                        return new Exception(lastErrorRecord.ToString());
                        Exception lastException = errorList[0] as Exception;
                        if (lastException != null)
                            return lastException;
                            return new Exception(errorList[0].ToString());
        private RunspaceOpenModuleLoadException ProcessModulesToImport(
            Runspace initializedRunspace,
            IEnumerable moduleList,
            HashSet<CommandInfo> publicCommands,
            HashSet<string> unresolvedCmdsToExpose)
            RunspaceOpenModuleLoadException exceptionToReturn = null;
            List<PSModuleInfo> processedModules = new List<PSModuleInfo>();
            foreach (object module in moduleList)
                string moduleName = module as string;
                if (moduleName != null)
                    exceptionToReturn = ProcessOneModule(
                        initializedRunspace: initializedRunspace,
                        name: moduleName,
                        moduleInfoToLoad: null,
                        publicCommands: publicCommands,
                        processedModules: processedModules);
                    ModuleSpecification moduleSpecification = module as ModuleSpecification;
                    if (moduleSpecification != null)
                        if ((moduleSpecification.RequiredVersion == null) && (moduleSpecification.Version == null) && (moduleSpecification.MaximumVersion == null) && (moduleSpecification.Guid == null))
                            // if only name is specified in the module spec, just try import the module
                            // ie., don't take the performance overhead of calling GetModule.
                                name: moduleSpecification.Name,
                            Collection<PSModuleInfo> moduleInfos = ModuleCmdletBase.GetModuleIfAvailable(moduleSpecification, initializedRunspace);
                            if (moduleInfos != null && moduleInfos.Count > 0)
                                    moduleInfoToLoad: moduleInfos[0],
                                var version = "0.0.0.0";
                                if (moduleSpecification.RequiredVersion != null)
                                    version = moduleSpecification.RequiredVersion.ToString();
                                else if (moduleSpecification.Version != null)
                                    version = moduleSpecification.Version.ToString();
                                    if (moduleSpecification.MaximumVersion != null)
                                        version = version + " - " + moduleSpecification.MaximumVersion;
                                else if (moduleSpecification.MaximumVersion != null)
                                    version = moduleSpecification.MaximumVersion;
                                    global::Modules.RequiredModuleNotFoundWrongGuidVersion,
                                    moduleSpecification.Name,
                                    moduleSpecification.Guid,
                                    version);
                                RunspaceOpenModuleLoadException rome = new RunspaceOpenModuleLoadException(message);
                                exceptionToReturn = ValidateAndReturnRunspaceOpenModuleLoadException(null, moduleSpecification.Name, rome);
                        Debug.Assert(false, "ProcessImportModule can import a module using name or module specification.");
            if (exceptionToReturn == null)
                // Now go through the list of commands not yet resolved to ensure they are public if requested
                foreach (string unresolvedCommand in unresolvedCmdsToExpose.ToArray<string>())
                    string commandToMakeVisible = Utils.ParseCommandName(unresolvedCommand, out moduleName);
                    foreach (CommandInfo cmd in LookupCommands(
                        commandPattern: commandToMakeVisible,
                        moduleName: moduleName,
                        context: initializedRunspace.ExecutionContext,
                        processedModules: processedModules))
                            // Special case for wild card lookups.
                            // "Import-Module" or "ipmo" cannot be visible when exposing commands via VisibleCmdlets, etc.
                            if ((cmd.Name.Equals("Import-Module", StringComparison.OrdinalIgnoreCase) &&
                                 (!string.IsNullOrEmpty(cmd.ModuleName) && cmd.ModuleName.Equals("Microsoft.PowerShell.Core", StringComparison.OrdinalIgnoreCase))) ||
                                 cmd.Name.Equals("ipmo", StringComparison.OrdinalIgnoreCase)
                                cmd.Visibility = SessionStateEntryVisibility.Private;
                                cmd.Visibility = SessionStateEntryVisibility.Public;
                                publicCommands.Add(cmd);
                            // Some CommandInfo derivations throw on the Visibility setter.
                    if (found && !WildcardPattern.ContainsWildcardCharacters(commandToMakeVisible))
                        unresolvedCmdsToExpose.Remove(unresolvedCommand);
        /// Helper method to search for commands matching the provided commandPattern.
        /// Supports wild cards and if the commandPattern contains wildcard characters then multiple
        /// results can be returned.  Otherwise a single (and first) match will be returned.
        /// If a moduleName is provided then only commands associated with that module will be returned.
        /// Only public commands are searched to start with.  If no results are found then a search on
        /// internal commands is performed.
        /// <param name="commandPattern"></param>
        /// <param name="processedModules"></param>
        private static IEnumerable<CommandInfo> LookupCommands(
            string commandPattern,
            List<PSModuleInfo> processedModules)
            bool isWildCardPattern = WildcardPattern.ContainsWildcardCharacters(commandPattern);
            var searchOptions = isWildCardPattern ?
                SearchResolutionOptions.CommandNameIsPattern | SearchResolutionOptions.ResolveFunctionPatterns | SearchResolutionOptions.SearchAllScopes :
                SearchResolutionOptions.ResolveFunctionPatterns | SearchResolutionOptions.SearchAllScopes;
            bool haveModuleName = !string.IsNullOrEmpty(moduleName);
            // Start with public search
            CommandOrigin cmdOrigin = CommandOrigin.Runspace;
                foreach (CommandInfo commandInfo in context.SessionState.InvokeCommand.GetCommands(
                    name: commandPattern,
                    commandTypes: CommandTypes.All,
                    options: searchOptions,
                    commandOrigin: cmdOrigin))
                    // If module name is provided then use it to restrict returned results.
                    if (haveModuleName && !moduleName.Equals(commandInfo.ModuleName, StringComparison.OrdinalIgnoreCase))
                    yield return commandInfo;
                    // Return first match unless a wild card pattern is submitted.
                    if (!isWildCardPattern)
                if (found || (cmdOrigin == CommandOrigin.Internal))
                // Next try internal search.
                cmdOrigin = CommandOrigin.Internal;
            // If the command is associated with a module, try finding the command in the imported module list.
            // The SessionState function table holds only one command name, and if two or more modules contain
            // a command with the same name, only one of them will appear in the function table search above.
            if (!found && haveModuleName)
                var pattern = new WildcardPattern(commandPattern);
                foreach (PSModuleInfo moduleInfo in processedModules)
                    if (moduleInfo.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                        foreach (var cmd in moduleInfo.ExportedCommands.Values)
                            if (pattern.IsMatch(cmd.Name))
                                yield return cmd;
        /// If <paramref name="moduleInfoToLoad"/> is null, import module using <paramref name="name"/>. Otherwise,
        /// import module using <paramref name="moduleInfoToLoad"/>
        private RunspaceOpenModuleLoadException ProcessOneModule(
            PSModuleInfo moduleInfoToLoad,
            using (PowerShell pse = PowerShell.Create())
                CommandInfo c = new CmdletInfo("Import-Module", typeof(ImportModuleCommand), null, null, initializedRunspace.ExecutionContext);
                Command cmd = new Command(c);
                if (moduleInfoToLoad != null)
                    cmd.Parameters.Add("ModuleInfo", moduleInfoToLoad);
                    name = moduleInfoToLoad.Name;
                    // If FullyQualifiedPath is supplied then use it.
                    // In this scenario, the FullyQualifiedPath would
                    // refer to $pshome\Modules location where core
                    // modules are deployed.
                        name = Path.Combine(path, name);
                    cmd.Parameters.Add("Name", name);
                if (!ThrowOnRunspaceOpenError)
                    cmd.MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
                pse.AddCommand(cmd);
                    c = new CmdletInfo("Out-Default", typeof(OutDefaultCommand), null, null, initializedRunspace.ExecutionContext);
                    pse.AddCommand(new Command(c));
                    // For runspace init module processing, pass back the PSModuleInfo to the output pipeline.
                    cmd.Parameters.Add("PassThru");
                pse.Runspace = initializedRunspace;
                // Module import should be run in FullLanguage mode since it is running in
                // a trusted context.
                var savedLanguageMode = pse.Runspace.ExecutionContext.LanguageMode;
                pse.Runspace.ExecutionContext.LanguageMode = PSLanguageMode.FullLanguage;
                    // For runspace init module processing, collect the imported PSModuleInfo returned in the output pipeline.
                    // In other cases, this collection will be empty.
                    Collection<PSModuleInfo> moduleInfos = pse.Invoke<PSModuleInfo>();
                    processedModules.AddRange(moduleInfos);
                    pse.Runspace.ExecutionContext.LanguageMode = savedLanguageMode;
                // Lock down the command visibility to respect default command visibility
                if (this.DefaultCommandVisibility != SessionStateEntryVisibility.Public)
                    foreach (CommandInfo importedCommand in initializedRunspace.ExecutionContext.SessionState.InvokeCommand.GetCommands(
                            // All commands except for the initial session public commands should be made private.
                            if ((importedCommand.Visibility != this.DefaultCommandVisibility) &&
                                !publicCommands.Contains(importedCommand))
                                importedCommand.Visibility = this.DefaultCommandVisibility;
                // Now see if there were any errors. Because the only way we have to
                // return an error at this point is a single exception, we'll take the first
                // error and throw it...
                return ValidateAndReturnRunspaceOpenModuleLoadException(pse, name, null);
        private RunspaceOpenModuleLoadException ValidateAndReturnRunspaceOpenModuleLoadException(PowerShell pse, string moduleName, RunspaceOpenModuleLoadException exception)
            // Only throw the exception if ThrowOnRunspaceOpenError is set.
                RunspaceOpenModuleLoadException rome = null;
                    rome = exception;
                else if (pse.Streams.Error.Count > 0)
                    ErrorRecord er;
                    Exception firstError;
                    // Merge errors from pse.Streams and errors
                    PSDataCollection<ErrorRecord> mergedErrors = new PSDataCollection<ErrorRecord>();
                    er = pse.Streams.Error[0];
                    firstError = er.Exception;
                    foreach (var e in pse.Streams.Error)
                        mergedErrors.Add(e);
                    rome = new RunspaceOpenModuleLoadException(moduleName, mergedErrors);
                if (rome != null)
                    return rome;
        /// Reinitializes elements of the associated runspace to their initial values.
        /// This allows for runspace reuse with minimal chance for contamination.
        internal void ResetRunspaceState(ExecutionContext context)
                // Reset the global variable table
                ss.InitializeSessionStateInternalSpecialVariables(true);
                // Add the built-in variables
                foreach (SessionStateVariableEntry e in InitialSessionState.BuiltInVariables)
                    PSVariable v = new PSVariable(
                        e.Name,
                        e.Value,
                        e.Options, e.Attributes,
                        e.Description)
                    { Visibility = e.Visibility };
                    ss.GlobalScope.SetVariable(e.Name, v, false, true, ss, fastPath: true);
                ss.InitializeFixedVariables();
                // Then re-initialize it with variables to session state...
                foreach (SessionStateVariableEntry e in Variables)
                        e.Options,
                        e.Attributes,
                InitialSessionState.CreateQuestionVariable(context);
                // Reset the path for this runspace.
                SetSessionStateDrive(context, true);
                // Reset the event, transaction and debug managers.
                context.ResetManagers();
                // Reset tracing/debugging to the off state.
                context.PSDebugTraceLevel = 0;
                context.PSDebugTraceStep = false;
        internal static void SetSessionStateDrive(ExecutionContext context, bool setLocation)
            // Set the starting location to the current process working directory
            // Ignore any errors as the file system provider may not be loaded or
            // a drive with the same name as the real file system drive may not have
            // been mounted.
                bool proceedWithSetLocation = true;
                if (context.EngineSessionState.ProviderCount > 0)
                    // NTRAID#Windows Out Of Band Releases-908481-2005/07/01-JeffJon
                    // Make sure we have a CurrentDrive set so that we can deal with
                    // UNC paths
                    if (context.EngineSessionState.CurrentDrive == null)
                        bool fsDriveSet = false;
                            // Set the current drive to the first FileSystem drive if it exists.
                            ProviderInfo fsProvider = context.EngineSessionState.GetSingleProvider(context.ProviderNames.FileSystem);
                            Collection<PSDriveInfo> fsDrives = fsProvider.Drives;
                            if (fsDrives != null && fsDrives.Count > 0)
                                context.EngineSessionState.CurrentDrive = fsDrives[0];
                                fsDriveSet = true;
                        if (!fsDriveSet)
                            Collection<PSDriveInfo> allDrives = context.EngineSessionState.Drives(null);
                            if (allDrives != null && allDrives.Count > 0)
                                context.EngineSessionState.CurrentDrive = allDrives[0];
                                    new ItemNotFoundException(Directory.GetCurrentDirectory(), "PathNotFound", SessionStateStrings.PathNotFound);
                                context.ReportEngineStartupError(itemNotFound);
                                proceedWithSetLocation = false;
                    if (proceedWithSetLocation && setLocation)
                        CmdletProviderContext providerContext = new CmdletProviderContext(context);
                            providerContext.SuppressWildcardExpansion = true;
                            context.EngineSessionState.SetLocation(Directory.GetCurrentDirectory(), providerContext);
                            // If we can't access the Environment.CurrentDirectory, we may be in an AppContainer. Set the
                            // default drive to $pshome
                            string defaultPath = System.IO.Path.GetDirectoryName(Environment.ProcessPath);
                            context.EngineSessionState.SetLocation(defaultPath, providerContext);
        internal static void CreateQuestionVariable(ExecutionContext context)
            QuestionMarkVariable qv = new QuestionMarkVariable(context);
            context.EngineSessionState.SetVariableAtScope(qv, "global", true, CommandOrigin.Internal);
        internal static void RemoveTypesAndFormats(ExecutionContext context, IList<string> formatFilesToRemove, IList<string> typeFilesToRemove)
            // The formats and types tables are implemented in such a way that
            // we can't simply remove an entry. We need to edit the list, clear the
            // exiting composed table and then rebuild the entire table.
            if (formatFilesToRemove != null && formatFilesToRemove.Count > 0)
                var newFormats = new InitialSessionStateEntryCollection<SessionStateFormatEntry>();
                HashSet<string> formatFilesToRemoveSet = new HashSet<string>(formatFilesToRemove, StringComparer.OrdinalIgnoreCase);
                foreach (SessionStateFormatEntry entry in context.InitialSessionState.Formats)
                    if (!formatFilesToRemoveSet.Contains(entry.FileName))
                context.InitialSessionState.Formats.Clear();
                context.InitialSessionState.Formats.Add(newFormats);
                context.InitialSessionState.UpdateFormats(context, false);
            if (typeFilesToRemove != null && typeFilesToRemove.Count > 0)
                // The types table has the same issue as the format table - we need to rebuild the entire table.
                var newTypes = new InitialSessionStateEntryCollection<SessionStateTypeEntry>();
                List<string> resolvedTypeFilesToRemove = new List<string>();
                foreach (var typeFile in typeFilesToRemove)
                    resolvedTypeFilesToRemove.Add(ModuleCmdletBase.ResolveRootedFilePath(typeFile, context) ?? typeFile);
                foreach (SessionStateTypeEntry entry in context.InitialSessionState.Types)
                    if (entry.FileName == null)
                        // The entry is associated with a TypeData instance
                        string filePath = ModuleCmdletBase.ResolveRootedFilePath(entry.FileName, context) ?? entry.FileName;
                        if (!resolvedTypeFilesToRemove.Contains(filePath))
                // If there are any types that need to be added to the typetable, update them.
                // Else, clear the typetable
                if (newTypes.Count > 0)
                    context.InitialSessionState.Types.Clear();
                    context.InitialSessionState.Types.Add(newTypes);
                    context.InitialSessionState.UpdateTypes(context, false);
                    context.TypeTable.Clear();
        /// Update the type metadata loaded into this runspace.
        /// <param name="context">The execution context for the runspace to update.</param>
        /// <param name="updateOnly">If true, re-initialize the metadata collection...</param>
        internal void UpdateTypes(ExecutionContext context, bool updateOnly)
            if (Types.Count == 1)
                TypeTable typeTable = Types[0].TypeTable;
                if (typeTable != null)
                    // reuse the TypeTable instance specified in the sste.
                    // this essentially allows for TypeTable sharing across
                    // multiple runspaces.
                    context.TypeTable = typeTable;
                    Types.Clear();
                    Types.Add(typeTable.typesInfo);
            // Use at most 3 locks (we don't expect contention on that many cores anyways,
            // and typically we'll be processing just 2 or 3 files anyway, hence capacity=3.
            ConcurrentDictionary<string, string> filesProcessed = new ConcurrentDictionary<string, string>(
                    concurrencyLevel: 3,
                    capacity: 3,
            Parallel.ForEach(
                Types,
                sste =>
                // foreach (var sste in Types)
                if (sste.FileName != null)
                    if (filesProcessed.TryAdd(sste.FileName, null))
                        string moduleName = string.Empty;
                        if (sste.PSSnapIn != null && !string.IsNullOrEmpty(sste.PSSnapIn.Name))
                            moduleName = sste.PSSnapIn.Name;
                        context.TypeTable.Update(moduleName, sste.FileName, errors, context.AuthorizationManager, context.EngineHostInterface, out _);
                else if (sste.TypeTable != null)
                    // We get here only if it's NOT updating the existing type table
                    // because we cannot do the update with a type table instance
                    errors.Add(TypesXmlStrings.TypeTableCannotCoExist);
                    context.TypeTable.Update(sste.TypeData, errors, sste.IsRemove);
            context.TypeTable.ClearConsolidatedMembers();
                // Put the SessionStateTypeEntry into the cache if we are updating the type table
                foreach (var sste in Types)
                    context.InitialSessionState.Types.Add(sste);
                var allErrors = new StringBuilder();
                allErrors.Append('\n');
                foreach (string error in errors)
                    if (!string.IsNullOrEmpty(error))
                        if (this.ThrowOnRunspaceOpenError || this.RefreshTypeAndFormatSetting)
                            allErrors.Append(error);
                            context.ReportEngineStartupError(ExtendedTypeSystem.TypesXmlError, error);
                    string resource = ExtendedTypeSystem.TypesXmlError;
                    ThrowTypeOrFormatErrors(resource, allErrors.ToString(), "ErrorsUpdatingTypes");
                if (this.RefreshTypeAndFormatSetting)
        /// Update the formatting information for a runspace.
        /// <param name="context">The execution context for the runspace to be updated.</param>
        /// <param name="update">True if we only want to add stuff, false if we want to reinitialize.</param>
        internal void UpdateFormats(ExecutionContext context, bool update)
            if (DisableFormatUpdates || this.Formats.Count == 0)
            Collection<PSSnapInTypeAndFormatErrors> entries = new Collection<PSSnapInTypeAndFormatErrors>();
            InitialSessionStateEntryCollection<SessionStateFormatEntry> formatsToLoad;
            // If we're just updating the current runspace, then we'll add our entries
            // to the current list otherwise, we'll build a new list...
            if (update && context.InitialSessionState != null)
                formatsToLoad = context.InitialSessionState.Formats;
                formatsToLoad.Add(this.Formats);
                formatsToLoad = this.Formats;
            HashSet<string> filesProcessed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (SessionStateFormatEntry ssfe in formatsToLoad)
                    if (formatsToLoad.Count == 1)
                        context.FormatDBManager = ssfe.Formattable.FormatDBManager;
                        // if a SharedFormatTable is allowed then only one
                        // entry can be specified.
                        throw PSTraceSource.NewInvalidOperationException(FormatAndOutXmlLoadingStrings.FormatTableCannotCoExist);
                    if (!filesProcessed.Contains(ssfe.FileName))
                        filesProcessed.Add(ssfe.FileName);
                context.FormatDBManager.UpdateDataBase(entries, context.AuthorizationManager, context.EngineHostInterface, true);
                var allErrors = new StringBuilder("\n");
                bool hasErrors = false;
                // Now see if there were any errors in the format files and report them
                // if this is the case...
                foreach (PSSnapInTypeAndFormatErrors entry in entries)
                    if (entry.Errors != null && !entry.Errors.IsEmpty)
                        foreach (string error in entry.Errors)
                                hasErrors = true;
                                    context.ReportEngineStartupError(FormatAndOutXmlLoadingStrings.FormatLoadingErrors, error);
                if ((this.ThrowOnRunspaceOpenError || this.RefreshTypeAndFormatSetting) && hasErrors)
                    string resource = FormatAndOutXmlLoadingStrings.FormatLoadingErrors;
                    ThrowTypeOrFormatErrors(resource, allErrors.ToString(), "ErrorsUpdatingFormats");
        private static void ThrowTypeOrFormatErrors(string resourceString, string errorMsg, string errorId)
            string message = StringUtil.Format(resourceString, errorMsg);
            ex.SetErrorId(errorId);
        /// Need to have SnapIn support till we move to modules.
        [Obsolete("Custom PSSnapIn is deprecated. Please use a module instead.", true)]
        public PSSnapInInfo ImportPSSnapIn(string name, out PSSnapInException warning)
                PSTraceSource.NewArgumentException(nameof(name));
            // Check whether the mshsnapin is present in the registry.
            // TODO: Note the hard-coded version number here, this was part of the SingleShell
            // implementation and should be refactored.
            PSSnapInInfo newPSSnapIn = PSSnapInReader.Read("2", name);
            if (!PSVersionInfo.IsValidPSVersion(newPSSnapIn.PSVersion))
                s_PSSnapInTracer.TraceError("MshSnapin {0} and current monad engine's versions don't match.", name);
                throw PSTraceSource.NewArgumentException(
                    "mshSnapInID",
                    ConsoleInfoErrorStrings.AddPSSnapInBadMonadVersion,
                    newPSSnapIn.PSVersion.ToString(),
                    "2.0");
            // Now actually load the snapin...
            PSSnapInInfo snapin = ImportPSSnapIn(newPSSnapIn, out warning);
            return snapin;
        internal PSSnapInInfo ImportCorePSSnapIn()
            // Load Microsoft.PowerShell.Core as a snapin.
            PSSnapInInfo coreSnapin = PSSnapInReader.ReadCoreEngineSnapIn();
            ImportPSSnapIn(coreSnapin, out _);
            return coreSnapin;
        internal PSSnapInInfo ImportPSSnapIn(PSSnapInInfo psSnapInInfo, out PSSnapInException warning)
            ArgumentNullException.ThrowIfNull(psSnapInInfo);
            // See if the snapin is already loaded. If has been then there will be an entry in the
            // Assemblies list for it already...
            bool reload = true;
            foreach (SessionStateAssemblyEntry ae in this.Assemblies)
                PSSnapInInfo loadedPSSnapInInfo = ae.PSSnapIn;
                if (loadedPSSnapInInfo != null)
                    // See if the assembly-qualified names match and return the existing PSSnapInInfo
                    // if they do.
                    string loadedSnapInName = ae.PSSnapIn.AssemblyName;
                    if (!string.IsNullOrEmpty(loadedSnapInName)
                        && string.Equals(loadedSnapInName, psSnapInInfo.AssemblyName, System.StringComparison.OrdinalIgnoreCase))
                        // the previous implementation used to return the
                        // same loaded snap-in value. This results in the
                        // commands/types/formats exposed in the snap-in
                        // to be not populated in the InitialSessionState
                        // object. This is being fixed
                        reload = false;
            Dictionary<string, SessionStateCmdletEntry> cmdlets = null;
            Dictionary<string, List<SessionStateAliasEntry>> aliases = null;
            Dictionary<string, SessionStateProviderEntry> providers = null;
            Assembly assembly = null;
            if (reload)
                s_PSSnapInTracer.WriteLine("Loading assembly for psSnapIn {0}", psSnapInInfo.Name);
                assembly = PSSnapInHelpers.LoadPSSnapInAssembly(psSnapInInfo);
                    s_PSSnapInTracer.TraceError("Loading assembly for psSnapIn {0} failed", psSnapInInfo.Name);
                    return null; // BUGBUG - should add something to the warnings list here instead of quitting...
                s_PSSnapInTracer.WriteLine("Loading assembly for psSnapIn {0} succeeded", psSnapInInfo.Name);
                PSSnapInHelpers.AnalyzePSSnapInAssembly(assembly, psSnapInInfo.Name, psSnapInInfo, moduleInfo: null, out cmdlets, out aliases, out providers, out helpFile);
            // We skip checking if the file exists when it's in $PSHOME because of magic
            // where we have the former contents of those files built into the engine directly.
            foreach (string file in psSnapInInfo.Types)
                string path = Path.Combine(psSnapInInfo.ApplicationBase, file);
                if (!string.Equals(psHome, psSnapInInfo.ApplicationBase, StringComparison.OrdinalIgnoreCase)
                    && !File.Exists(path))
                    // Remove the application base directory if assembly doesn't exist in it.
                    path = file;
                SessionStateTypeEntry typeEntry = new SessionStateTypeEntry(path);
                typeEntry.SetPSSnapIn(psSnapInInfo);
                this.Types.Add(typeEntry);
            foreach (string file in psSnapInInfo.Formats)
                SessionStateFormatEntry formatEntry = new SessionStateFormatEntry(path);
                formatEntry.SetPSSnapIn(psSnapInInfo);
                this.Formats.Add(formatEntry);
            var assemblyEntry = new SessionStateAssemblyEntry(psSnapInInfo.AssemblyName, psSnapInInfo.AbsoluteModulePath);
            assemblyEntry.SetPSSnapIn(psSnapInInfo);
            Assemblies.Add(assemblyEntry);
            if (cmdlets != null)
                foreach (SessionStateCmdletEntry cmdlet in cmdlets.Values)
                    SessionStateCmdletEntry newEntry = (SessionStateCmdletEntry)cmdlet.Clone();
                    newEntry.Visibility = this.DefaultCommandVisibility;
                    this.Commands.Add(newEntry);
            if (aliases != null)
                foreach (var cmdletAliasesEntry in aliases.Values)
                    foreach (var sessionStateAliasEntry in cmdletAliasesEntry)
                        sessionStateAliasEntry.Visibility = this.DefaultCommandVisibility;
                        this.Commands.Add(sessionStateAliasEntry);
            if (providers != null)
                foreach (SessionStateProviderEntry provider in providers.Values)
                    this.Providers.Add(provider);
            // Add help file information for built-in functions
            if (psSnapInInfo.Name.Equals(CoreSnapin, StringComparison.OrdinalIgnoreCase))
                foreach (var f in BuiltInFunctions)
                    Collection<SessionStateCommandEntry> funcList = Commands[f.Name];
                    foreach (var func in funcList)
                        if (func is SessionStateFunctionEntry)
                            ((SessionStateFunctionEntry)func).SetHelpFile(helpFile);
            ImportedSnapins.Add(psSnapInInfo.Name, psSnapInInfo);
            return psSnapInInfo;
        internal PSSnapInInfo GetPSSnapIn(string psSnapinName)
            if (ImportedSnapins.TryGetValue(psSnapinName, out PSSnapInInfo importedSnapin))
                return importedSnapin;
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom")]
        internal static Assembly LoadAssemblyFromFile(string fileName)
            s_PSSnapInTracer.WriteLine("Loading assembly for psSnapIn {0}", fileName);
            Assembly assembly = Assembly.LoadFrom(fileName);
                s_PSSnapInTracer.TraceError("Loading assembly for psSnapIn {0} failed", fileName);
            s_PSSnapInTracer.WriteLine("Loading assembly for psSnapIn {0} succeeded", fileName);
        internal void ImportCmdletsFromAssembly(Assembly assembly, PSModuleInfo module)
            ArgumentNullException.ThrowIfNull(assembly);
            string assemblyPath = assembly.Location;
            PSSnapInHelpers.AnalyzePSSnapInAssembly(
                assembly,
                assemblyPath,
                psSnapInInfo: null,
                out Dictionary<string, SessionStateCmdletEntry> cmdlets,
                out Dictionary<string, List<SessionStateAliasEntry>> aliases,
                out Dictionary<string, SessionStateProviderEntry> providers,
                helpFile: out _);
                    this.Commands.Add(cmdlet);
        // Now define a bunch of functions that describe the rest of the default session state...
        internal const string FormatEnumerationLimit = "FormatEnumerationLimit";
        internal const int DefaultFormatEnumerationLimit = 4;
        /// This is the default function to use for tab expansion.
        private static readonly string s_tabExpansionFunctionText = @"
<# Options include:
     RelativeFilePaths - [bool]
         Always resolve file paths using Resolve-Path -Relative.
         The default is to use some heuristics to guess if relative or absolute is better.
   To customize your own custom options, pass a hashtable to CompleteInput, e.g.
         return [System.Management.Automation.CommandCompletion]::CompleteInput($inputScript, $cursorColumn,
             @{ RelativeFilePaths=$false }
[CmdletBinding(DefaultParameterSetName = 'ScriptInputSet')]
[OutputType([System.Management.Automation.CommandCompletion])]
Param(
    [Parameter(ParameterSetName = 'ScriptInputSet', Mandatory = $true, Position = 0)]
    [AllowEmptyString()]
    [string] $inputScript,
    [Parameter(ParameterSetName = 'ScriptInputSet', Position = 1)]
    [int] $cursorColumn = $inputScript.Length,
    [Parameter(ParameterSetName = 'AstInputSet', Mandatory = $true, Position = 0)]
    [System.Management.Automation.Language.Ast] $ast,
    [Parameter(ParameterSetName = 'AstInputSet', Mandatory = $true, Position = 1)]
    [System.Management.Automation.Language.Token[]] $tokens,
    [Parameter(ParameterSetName = 'AstInputSet', Mandatory = $true, Position = 2)]
    [System.Management.Automation.Language.IScriptPosition] $positionOfCursor,
    [Parameter(ParameterSetName = 'ScriptInputSet', Position = 2)]
    [Parameter(ParameterSetName = 'AstInputSet', Position = 3)]
    [Hashtable] $options = $null
End
    if ($psCmdlet.ParameterSetName -eq 'ScriptInputSet')
        return [System.Management.Automation.CommandCompletion]::CompleteInput(
            <#inputScript#>  $inputScript,
            <#cursorColumn#> $cursorColumn,
            <#options#>      $options)
            <#ast#>              $ast,
            <#tokens#>           $tokens,
            <#positionOfCursor#> $positionOfCursor,
            <#options#>          $options)
        /// This is the default function to use for clear-host.
        internal static string GetClearHostFunctionText()
                // use $RawUI so this works over remoting where there isn't a physical console
$RawUI = $Host.UI.RawUI
$RawUI.CursorPosition = @{X=0;Y=0}
$RawUI.SetBufferContents(
    @{Top = -1; Bottom = -1; Right = -1; Left = -1},
    @{Character = ' '; ForegroundColor = $rawui.ForegroundColor; BackgroundColor = $rawui.BackgroundColor})
# .Link
# https://go.microsoft.com/fwlink/?LinkID=2096480
# .ExternalHelp System.Management.Automation.dll-help.xml
                // Porting note: non-Windows platforms use `clear`
[Console]::Write((
    & (Get-Command -CommandType Application clear | Select-Object -First 1).Definition
        internal static string GetExecFunctionText()
Switch-Process -WithCommand $args
        internal const string WindowsHelpFunctionText = @"
.FORWARDHELPTARGETNAME Get-Help
.FORWARDHELPCATEGORY Cmdlet
[CmdletBinding(DefaultParameterSetName='AllUsersView', HelpUri='https://go.microsoft.com/fwlink/?LinkID=113316')]
    [Parameter(Position=0, ValueFromPipelineByPropertyName=$true)]
    ${Name},
    ${Path},
    [ValidateSet('Alias','Cmdlet','Provider','General','FAQ','Glossary','HelpFile','ScriptCommand','Function','Filter','ExternalScript','All','DefaultHelp','DscResource','Class','Configuration')]
    [string[]]
    ${Category},
    [Parameter(ParameterSetName='DetailedView', Mandatory=$true)]
    ${Detailed},
    [Parameter(ParameterSetName='AllUsersView')]
    ${Full},
    [Parameter(ParameterSetName='Examples', Mandatory=$true)]
    ${Examples},
    [Parameter(ParameterSetName='Parameters', Mandatory=$true)]
    ${Parameter},
    ${Component},
    ${Functionality},
    ${Role},
    [Parameter(ParameterSetName='Online', Mandatory=$true)]
    ${Online},
    [Parameter(ParameterSetName='ShowWindow', Mandatory=$true)]
    ${ShowWindow})
    # Display the full help topic by default but only for the AllUsersView parameter set.
    if (($psCmdlet.ParameterSetName -eq 'AllUsersView') -and !$Full) {
        $PSBoundParameters['Full'] = $true
    # Nano needs to use Unicode, but Windows and Linux need the default
    $OutputEncoding = if ([System.Management.Automation.Platform]::IsNanoServer -or [System.Management.Automation.Platform]::IsIoT) {
        [System.Text.Encoding]::Unicode
        [System.Console]::OutputEncoding
    $help = Get-Help @PSBoundParameters
    # If a list of help is returned or AliasHelpInfo (because it is small), don't pipe to more
    $psTypeNames = ($help | Select-Object -First 1).PSTypeNames
    if ($psTypeNames -Contains 'HelpInfoShort' -Or $psTypeNames -Contains 'AliasHelpInfo')
        $help
    elseif ($help -ne $null)
        # By default use more on Windows and less on Linux.
        $pagerCommand = 'more.com'
        $pagerArgs = $null
        # Respect PAGER environment variable which allows user to specify a custom pager.
        # Ignore a pure whitespace PAGER value as that would cause the tokenizer to return 0 tokens.
        if (![string]::IsNullOrWhitespace($env:PAGER)) {
            if (Get-Command $env:PAGER -ErrorAction Ignore) {
                # Entire PAGER value corresponds to a single command.
                $pagerCommand = $env:PAGER
                # PAGER value is not a valid command, check if PAGER command and arguments have been specified.
                # Tokenize the specified $env:PAGER value. Ignore tokenizing errors since any errors may be valid
                # argument syntax for the paging utility.
                $errs = $null
                $tokens = [System.Management.Automation.PSParser]::Tokenize($env:PAGER, [ref]$errs)
                $customPagerCommand = $tokens[0].Content
                if (!(Get-Command $customPagerCommand -ErrorAction Ignore)) {
                    # Custom pager command is invalid, issue a warning.
                    Write-Warning ""Custom-paging utility command not found. Ignoring command specified in `$env:PAGER: $env:PAGER""
                    # This approach will preserve all the pagers args.
                    $pagerCommand = $customPagerCommand
                    $pagerArgs = if ($tokens.Count -gt 1) {
                        $env:PAGER.Substring($tokens[1].Start)
                        $null
        $pagerCommandInfo = Get-Command -Name $pagerCommand -ErrorAction Ignore
        if ($pagerCommandInfo -eq $null) {
        elseif ($pagerCommandInfo.CommandType -eq 'Application') {
            # If the pager is an application, format the output width before sending to the app.
            $consoleWidth = [System.Math]::Max([System.Console]::WindowWidth, 20)
            if ($pagerArgs) {
                $help | Out-String -Stream -Width ($consoleWidth - 1) | & $pagerCommand $pagerArgs
                $help | Out-String -Stream -Width ($consoleWidth - 1) | & $pagerCommand
            # The pager command is a PowerShell function, script or alias, so pipe directly into it.
            $help | & $pagerCommand $pagerArgs
        internal const string UnixHelpFunctionText = @"
    ${Online})
    # Linux need the default
    $OutputEncoding = [System.Console]::OutputEncoding
        $pagerCommand = 'less'
        $pagerArgs = '-s','-P','Page %db?B of %D:.\. Press h for help or q to quit\.'
                    $pagerArgs = if ($tokens.Count -gt 1) {$env:PAGER.Substring($tokens[1].Start)} else {$null}
        /// This is the default function to use for man/help. It uses
        /// splatting to pass in the parameters.
        internal static string GetHelpPagingFunctionText()
            // We used to generate the text for this function so you could add a parameter
            // to Get-Help and not worry about adding it here.  That was a little slow at
            // startup, so it's hard coded, with a test to make sure the parameters match.
            return WindowsHelpFunctionText;
            // This version removes the -ShowWindow parameter since it is not supported on Linux.
            return UnixHelpFunctionText;
        internal static string GetMkdirFunctionText()
.FORWARDHELPTARGETNAME New-Item
[CmdletBinding(DefaultParameterSetName='pathSet',
    SupportsShouldProcess=$true,
    SupportsTransactions=$true,
    ConfirmImpact='Medium')]
    [OutputType([System.IO.DirectoryInfo])]
    [Parameter(ParameterSetName='nameSet', Position=0, ValueFromPipelineByPropertyName=$true)]
    [Parameter(ParameterSetName='pathSet', Mandatory=$true, Position=0, ValueFromPipelineByPropertyName=$true)]
    [System.String[]]
    [Parameter(ParameterSetName='nameSet', Mandatory=$true, ValueFromPipelineByPropertyName=$true)]
    [System.String]
    [Parameter(ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]
    [System.Object]
    ${Value},
    [Switch]
    ${Force},
    [Parameter(ValueFromPipelineByPropertyName=$true)]
    [System.Management.Automation.PSCredential]
    ${Credential}
begin {
    $wrappedCmd = $ExecutionContext.InvokeCommand.GetCommand('New-Item', [System.Management.Automation.CommandTypes]::Cmdlet)
    $scriptCmd = {& $wrappedCmd -Type Directory @PSBoundParameters }
    $steppablePipeline = $scriptCmd.GetSteppablePipeline()
process {
end {
        internal static string GetOSTFunctionText()
    [ValidateRange(2, 2147483647)]
    [int]
    ${Width},
    [Parameter(ValueFromPipeline=$true)]
    [psobject]
    ${InputObject})
    $PSBoundParameters['Stream'] = $true
    $wrappedCmd = $ExecutionContext.InvokeCommand.GetCommand('Out-String',[System.Management.Automation.CommandTypes]::Cmdlet)
    $scriptCmd = {& $wrappedCmd @PSBoundParameters }
.ForwardHelpTargetName Out-String
.ForwardHelpCategory Cmdlet
        internal const ActionPreference DefaultDebugPreference = ActionPreference.SilentlyContinue;
        internal const ActionPreference DefaultErrorActionPreference = ActionPreference.Continue;
        internal const ActionPreference DefaultProgressPreference = ActionPreference.Continue;
        internal const ActionPreference DefaultVerbosePreference = ActionPreference.SilentlyContinue;
        internal const ActionPreference DefaultWarningPreference = ActionPreference.Continue;
        internal const ActionPreference DefaultInformationPreference = ActionPreference.SilentlyContinue;
        internal const ErrorView DefaultErrorView = ErrorView.ConciseView;
        internal const bool DefaultWhatIfPreference = false;
        internal const ConfirmImpact DefaultConfirmPreference = ConfirmImpact.High;
        static InitialSessionState()
            var builtinVariables = new List<SessionStateVariableEntry>()
                // Engine variables that should be precreated before running profile
                // Bug fix for Win7:2202228 Engine halts if initial command fulls up variable table
                // Anytime a new variable that the engine depends on to run is added, this table
                // must be updated...
                new SessionStateVariableEntry(SpecialVariables.LastToken, null, string.Empty),
                new SessionStateVariableEntry(SpecialVariables.FirstToken, null, string.Empty),
                new SessionStateVariableEntry(SpecialVariables.StackTrace, null, string.Empty),
                // Variable which controls the output rendering
                new SessionStateVariableEntry(
                    SpecialVariables.PSStyle,
                    PSStyle.Instance,
                    RunspaceInit.PSStyleDescription,
                    ScopedItemOptions.Constant),
                // Variable which controls the encoding for piping data to a NativeCommand
                    SpecialVariables.OutputEncoding,
                    Encoding.Default,
                    RunspaceInit.OutputEncodingDescription,
                    ScopedItemOptions.None,
                    new ArgumentTypeConverterAttribute(typeof(System.Text.Encoding))),
                // Variable which controls the encoding for decoding data from a NativeCommand
                    SpecialVariables.PSApplicationOutputEncoding,
                    RunspaceInit.PSApplicationOutputEncodingDescription,
                    new ArgumentTypeConverterAttribute(typeof(Encoding))),
                // Preferences
                // NTRAID#Windows Out Of Band Releases-931461-2006/03/13
                // ArgumentTypeConverterAttribute is applied to these variables,
                // but this only reaches the global variable.  If these are
                // redefined in script scope etc, the type conversion
                // is not applicable.
                // Variables typed to ActionPreference
                    SpecialVariables.ConfirmPreference,
                    DefaultConfirmPreference,
                    RunspaceInit.ConfirmPreferenceDescription,
                    new ArgumentTypeConverterAttribute(typeof(ConfirmImpact))),
                    SpecialVariables.DebugPreference,
                    DefaultDebugPreference,
                    RunspaceInit.DebugPreferenceDescription,
                    new ArgumentTypeConverterAttribute(typeof(ActionPreference))),
                    SpecialVariables.ErrorActionPreference,
                    DefaultErrorActionPreference,
                    RunspaceInit.ErrorActionPreferenceDescription,
                    SpecialVariables.ProgressPreference,
                    DefaultProgressPreference,
                    RunspaceInit.ProgressPreferenceDescription,
                    SpecialVariables.VerbosePreference,
                    DefaultVerbosePreference,
                    RunspaceInit.VerbosePreferenceDescription,
                    SpecialVariables.WarningPreference,
                    DefaultWarningPreference,
                    RunspaceInit.WarningPreferenceDescription,
                    SpecialVariables.InformationPreference,
                    DefaultInformationPreference,
                    RunspaceInit.InformationPreferenceDescription,
                    SpecialVariables.ErrorView,
                    DefaultErrorView,
                    RunspaceInit.ErrorViewDescription,
                    new ArgumentTypeConverterAttribute(typeof(ErrorView))),
                    SpecialVariables.NestedPromptLevel,
                    RunspaceInit.NestedPromptLevelDescription),
                    SpecialVariables.WhatIfPreference,
                    DefaultWhatIfPreference,
                    RunspaceInit.WhatIfPreferenceDescription),
                    FormatEnumerationLimit,
                    DefaultFormatEnumerationLimit,
                    RunspaceInit.FormatEnumerationLimitDescription),
                // variable for PSEmailServer
                    SpecialVariables.PSEmailServer,
                    RunspaceInit.PSEmailServerDescription),
                // Start: Variables which control remoting behavior
                    Microsoft.PowerShell.Commands.PSRemotingBaseCmdlet.DEFAULT_SESSION_OPTION,
                    new System.Management.Automation.Remoting.PSSessionOption(),
                    RemotingErrorIdStrings.PSDefaultSessionOptionDescription,
                    ScopedItemOptions.None),
                    SpecialVariables.PSSessionConfigurationName,
                    "http://schemas.microsoft.com/powershell/Microsoft.PowerShell",
                    RemotingErrorIdStrings.PSSessionConfigurationName,
                    SpecialVariables.PSSessionApplicationName,
                    "wsman",
                    RemotingErrorIdStrings.PSSessionAppName,
                // End: Variables which control remoting behavior
                #region Platform
                    SpecialVariables.IsLinux,
                    Platform.IsLinux,
                    ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
                    SpecialVariables.IsMacOS,
                    Platform.IsMacOS,
                    SpecialVariables.IsWindows,
                    Platform.IsWindows,
                    SpecialVariables.IsCoreCLR,
                    Platform.IsCoreCLR,
            builtinVariables.Add(
                    SpecialVariables.PSNativeCommandUseErrorActionPreference,
                    value: false,
                    RunspaceInit.PSNativeCommandUseErrorActionPreferenceDescription,
                    new ArgumentTypeConverterAttribute(typeof(bool))));
                    SpecialVariables.NativeArgumentPassing,
                    GetPassingStyle(),
                    RunspaceInit.NativeCommandArgumentPassingDescription,
                    new ArgumentTypeConverterAttribute(typeof(NativeArgumentPassingStyle))));
            BuiltInVariables = builtinVariables.ToArray();
        /// Assigns the default behavior for native argument passing.
        /// If the system is non-Windows, we will return Standard.
        /// Otherwise, we will return Windows.
        private static NativeArgumentPassingStyle GetPassingStyle()
            return NativeArgumentPassingStyle.Standard;
            return NativeArgumentPassingStyle.Windows;
        internal static readonly SessionStateVariableEntry[] BuiltInVariables;
        /// Returns a new array of alias entries every time it's called. This
        /// can't be static because the elements may be mutated in different session
        /// state objects so each session state must have a copy of the entry.
        internal static SessionStateAliasEntry[] BuiltInAliases
                // Too many AllScope entries hurts performance because an entry is
                // created in each new scope, so we limit the use of AllScope to the
                // most commonly used commands - primarily so command lookup is faster,
                // though if we speed up command lookup significantly, then removing
                // AllScope for all of these aliases makes sense.
                const ScopedItemOptions AllScope = ScopedItemOptions.AllScope;
                const ScopedItemOptions ReadOnly_AllScope = ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope;
                const ScopedItemOptions ReadOnly = ScopedItemOptions.ReadOnly;
                var builtInAliases = new List<SessionStateAliasEntry> {
                    new SessionStateAliasEntry("foreach", "ForEach-Object", string.Empty, ReadOnly_AllScope),
                    new SessionStateAliasEntry("%", "ForEach-Object", string.Empty, ReadOnly_AllScope),
                    new SessionStateAliasEntry("where", "Where-Object", string.Empty, ReadOnly_AllScope),
                    new SessionStateAliasEntry("?", "Where-Object", string.Empty, ReadOnly_AllScope),
                    new SessionStateAliasEntry("clc", "Clear-Content", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("cli", "Clear-Item", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("clp", "Clear-ItemProperty", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("clv", "Clear-Variable", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("cpi", "Copy-Item", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("cvpa", "Convert-Path", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("dbp", "Disable-PSBreakpoint", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("ebp", "Enable-PSBreakpoint", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("epal", "Export-Alias", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("epcsv", "Export-Csv", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("fl", "Format-List", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("ft", "Format-Table", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("fw", "Format-Wide", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("gal", "Get-Alias", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("gbp", "Get-PSBreakpoint", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("gc", "Get-Content", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("gci", "Get-ChildItem", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("gcm", "Get-Command", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("gdr", "Get-PSDrive", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("gcs", "Get-PSCallStack", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("ghy", "Get-History", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("gi", "Get-Item", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("gl", "Get-Location", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("gm", "Get-Member", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("gmo", "Get-Module", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("gp", "Get-ItemProperty", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("gpv", "Get-ItemPropertyValue", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("gps", "Get-Process", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("group", "Group-Object", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("gu", "Get-Unique", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("gv", "Get-Variable", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("iex", "Invoke-Expression", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("ihy", "Invoke-History", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("ii", "Invoke-Item", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("ipmo", "Import-Module", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("ipal", "Import-Alias", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("ipcsv", "Import-Csv", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("measure", "Measure-Object", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("mi", "Move-Item", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("mp", "Move-ItemProperty", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("nal", "New-Alias", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("ndr", "New-PSDrive", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("ni", "New-Item", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("nv", "New-Variable", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("nmo", "New-Module", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("oh", "Out-Host", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("rbp", "Remove-PSBreakpoint", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("rdr", "Remove-PSDrive", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("ri", "Remove-Item", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("rni", "Rename-Item", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("rnp", "Rename-ItemProperty", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("rp", "Remove-ItemProperty", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("rmo", "Remove-Module", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("rv", "Remove-Variable", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("gerr", "Get-Error", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("rvpa", "Resolve-Path", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("sal", "Set-Alias", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("sbp", "Set-PSBreakpoint", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("select", "Select-Object", string.Empty, ReadOnly_AllScope),
                    new SessionStateAliasEntry("si", "Set-Item", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("sl", "Set-Location", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("sp", "Set-ItemProperty", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("saps", "Start-Process", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("spps", "Stop-Process", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("sv", "Set-Variable", string.Empty, ReadOnly),
                    // Web cmdlets aliases
                    new SessionStateAliasEntry("irm", "Invoke-RestMethod", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("iwr", "Invoke-WebRequest", string.Empty, ReadOnly),
// Porting note: #if !UNIX is used to disable aliases for cmdlets which conflict with Linux / macOS
                    // ac is a native command on macOS
                    new SessionStateAliasEntry("ac", "Add-Content", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("clear", "Clear-Host"),
                    new SessionStateAliasEntry("compare", "Compare-Object", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("cpp", "Copy-ItemProperty", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("diff", "Compare-Object", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("gsv", "Get-Service", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("sleep", "Start-Sleep", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("sort", "Sort-Object", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("start", "Start-Process", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("sasv", "Start-Service", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("spsv", "Stop-Service", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("tee", "Tee-Object", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("write", "Write-Output", string.Empty, ReadOnly),
                    // These were transferred from the "transferred from the profile" section
                    new SessionStateAliasEntry("cat", "Get-Content"),
                    new SessionStateAliasEntry("cp", "Copy-Item", string.Empty, AllScope),
                    new SessionStateAliasEntry("ls", "Get-ChildItem"),
                    new SessionStateAliasEntry("man", "help"),
                    new SessionStateAliasEntry("mount", "New-PSDrive"),
                    new SessionStateAliasEntry("mv", "Move-Item"),
                    new SessionStateAliasEntry("ps", "Get-Process"),
                    new SessionStateAliasEntry("rm", "Remove-Item"),
                    new SessionStateAliasEntry("rmdir", "Remove-Item"),
                    new SessionStateAliasEntry("cnsn", "Connect-PSSession", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("dnsn", "Disconnect-PSSession", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("ogv", "Out-GridView", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("shcm", "Show-Command", string.Empty, ReadOnly),
                    // Bash built-ins we purposefully keep even if they override native commands
                    new SessionStateAliasEntry("cd", "Set-Location", string.Empty, AllScope),
                    new SessionStateAliasEntry("dir", "Get-ChildItem", string.Empty, AllScope),
                    new SessionStateAliasEntry("echo", "Write-Output", string.Empty, AllScope),
                    new SessionStateAliasEntry("fc", "Format-Custom", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("kill", "Stop-Process"),
                    new SessionStateAliasEntry("pwd", "Get-Location"),
                    new SessionStateAliasEntry("type", "Get-Content"),
// #if !CORECLR is used to disable aliases for cmdlets which are not available on OneCore or not appropriate for PSCore6 due to conflicts
                    new SessionStateAliasEntry("gwmi", "Get-WmiObject", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("iwmi", "Invoke-WMIMethod", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("ise", "powershell_ise.exe", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("rwmi", "Remove-WMIObject", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("sc", "Set-Content", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("swmi", "Set-WMIInstance", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("trcm", "Trace-Command", string.Empty, ReadOnly),
                    // Aliases transferred from the profile
                    new SessionStateAliasEntry("h", "Get-History"),
                    new SessionStateAliasEntry("history", "Get-History"),
                    new SessionStateAliasEntry("md", "mkdir", string.Empty, AllScope),
                    new SessionStateAliasEntry("popd", "Pop-Location", string.Empty, AllScope),
                    new SessionStateAliasEntry("pushd", "Push-Location", string.Empty, AllScope),
                    new SessionStateAliasEntry("r", "Invoke-History"),
                    new SessionStateAliasEntry("cls", "Clear-Host"),
                    new SessionStateAliasEntry("chdir", "Set-Location"),
                    new SessionStateAliasEntry("copy", "Copy-Item", string.Empty, AllScope),
                    new SessionStateAliasEntry("del", "Remove-Item", string.Empty, AllScope),
                    new SessionStateAliasEntry("erase", "Remove-Item"),
                    new SessionStateAliasEntry("move", "Move-Item", string.Empty, AllScope),
                    new SessionStateAliasEntry("rd", "Remove-Item"),
                    new SessionStateAliasEntry("ren", "Rename-Item"),
                    new SessionStateAliasEntry("set", "Set-Variable"),
                    new SessionStateAliasEntry("icm", "Invoke-Command"),
                    new SessionStateAliasEntry("clhy", "Clear-History", string.Empty, ReadOnly),
                    // Job Specific aliases
                    new SessionStateAliasEntry("gjb", "Get-Job"),
                    new SessionStateAliasEntry("rcjb", "Receive-Job"),
                    new SessionStateAliasEntry("rjb", "Remove-Job"),
                    new SessionStateAliasEntry("sajb", "Start-Job"),
                    new SessionStateAliasEntry("spjb", "Stop-Job"),
                    new SessionStateAliasEntry("wjb", "Wait-Job"),
                    new SessionStateAliasEntry("sujb", "Suspend-Job"),
                    new SessionStateAliasEntry("rujb", "Resume-Job"),
                    // Remoting Cmdlets Specific aliases
                    new SessionStateAliasEntry("npssc", "New-PSSessionConfigurationFile", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("ipsn", "Import-PSSession"),
                    new SessionStateAliasEntry("epsn", "Export-PSSession"),
                    new SessionStateAliasEntry("nsn", "New-PSSession"),
                    new SessionStateAliasEntry("gsn", "Get-PSSession"),
                    new SessionStateAliasEntry("rsn", "Remove-PSSession"),
                    new SessionStateAliasEntry("etsn", "Enter-PSSession"),
                    new SessionStateAliasEntry("rcsn", "Receive-PSSession", string.Empty, ReadOnly),
                    new SessionStateAliasEntry("exsn", "Exit-PSSession"),
                    // Win8: 121662/169179 Add "sls" alias for Select-String cmdlet
                    //   - do not use AllScope - this causes errors in profiles that set this somewhat commonly used alias.
                    new SessionStateAliasEntry("sls", "Select-String"),
                return builtInAliases.ToArray();
        internal const string DefaultPromptFunctionText = @"
""PS $($executionContext.SessionState.Path.CurrentLocation)$('>' * ($nestedPromptLevel + 1)) "";
# https://go.microsoft.com/fwlink/?LinkID=225750
        internal const string DefaultSetDriveFunctionText = "Set-Location $MyInvocation.MyCommand.Name";
        internal static readonly ScriptBlock SetDriveScriptBlock = ScriptBlock.CreateDelayParsedScriptBlock(DefaultSetDriveFunctionText, isProductCode: true);
        private static readonly PSLanguageMode systemLanguageMode = (SystemPolicy.GetSystemLockdownPolicy() == SystemEnforcementMode.Enforce) ? PSLanguageMode.ConstrainedLanguage : PSLanguageMode.FullLanguage;
        internal static readonly SessionStateFunctionEntry[] BuiltInFunctions = new SessionStateFunctionEntry[]
           // Functions that don't require full language mode
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("cd..", "Set-Location ..", isProductCode: true, languageMode: systemLanguageMode),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("cd\\", "Set-Location \\", isProductCode: true, languageMode: systemLanguageMode),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("cd~", "Set-Location ~", isProductCode: true, languageMode: systemLanguageMode),
            // Win8: 320909. Retaining the original definition to ensure backward compatibility.
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("Pause",
                string.Concat("$null = Read-Host '", CodeGeneration.EscapeSingleQuotedStringContent(RunspaceInit.PauseDefinitionString), "'"), isProductCode: true, languageMode: systemLanguageMode),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("help", GetHelpPagingFunctionText(), isProductCode: true, languageMode: systemLanguageMode),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("prompt", DefaultPromptFunctionText, isProductCode: true, languageMode: systemLanguageMode),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("exec", GetExecFunctionText(), isProductCode: true, languageMode: systemLanguageMode),
            // Functions that require full language mode and are trusted
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("Clear-Host", GetClearHostFunctionText(), isProductCode: true, languageMode: PSLanguageMode.FullLanguage),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("TabExpansion2", s_tabExpansionFunctionText, isProductCode: true, languageMode: PSLanguageMode.FullLanguage),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("oss", GetOSTFunctionText(), isProductCode: true, languageMode: PSLanguageMode.FullLanguage),
            // Porting note: we remove mkdir on Linux because of a conflict
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("mkdir", GetMkdirFunctionText(), isProductCode: true, languageMode: PSLanguageMode.FullLanguage),
            // Porting note: we remove the drive functions from Linux because they make no sense in that environment
            // Default drives
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("A:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("B:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("C:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("D:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("E:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("F:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("G:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("H:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("I:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("J:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("K:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("L:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("M:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("N:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("O:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("P:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("Q:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("R:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("S:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("T:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("U:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("V:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("W:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("X:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("Y:", DefaultSetDriveFunctionText, SetDriveScriptBlock),
            SessionStateFunctionEntry.GetDelayParsedFunctionEntry("Z:", DefaultSetDriveFunctionText, SetDriveScriptBlock)
        internal static void RemoveAllDrivesForProvider(ProviderInfo pi, SessionStateInternal ssi)
            foreach (PSDriveInfo di in ssi.GetDrivesForProvider(pi.FullName))
                    ssi.RemoveDrive(di, true, null);
        private static readonly PSTraceSource s_PSSnapInTracer = PSTraceSource.GetTracer("PSSnapInLoadUnload", "Loading and unloading mshsnapins", false);
        internal static readonly string CoreSnapin = "Microsoft.PowerShell.Core";
        internal static readonly string CoreModule = "Microsoft.PowerShell.Core";
        // The list of engine modules to create warnings when you try to remove them
        internal static readonly HashSet<string> EngineModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                "Microsoft.PowerShell.Utility",
                "Microsoft.PowerShell.Management",
                "Microsoft.PowerShell.Diagnostics",
                "Microsoft.PowerShell.Host",
                "Microsoft.PowerShell.Security",
                "Microsoft.WSMan.Management"
        internal static readonly HashSet<string> NestedEngineModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                "Microsoft.PowerShell.Commands.Utility",
                "Microsoft.PowerShell.Commands.Management",
                "Microsoft.PowerShell.Commands.Diagnostics",
                "Microsoft.PowerShell.ConsoleHost"
        internal static readonly Dictionary<string, string> EngineModuleNestedModuleMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                { "Microsoft.PowerShell.Utility", "Microsoft.PowerShell.Commands.Utility"},
                { "Microsoft.PowerShell.Management", "Microsoft.PowerShell.Commands.Management"},
                { "Microsoft.PowerShell.Diagnostics", "Microsoft.PowerShell.Commands.Diagnostics"},
                { "Microsoft.PowerShell.Host", "Microsoft.PowerShell.ConsoleHost"},
        internal static readonly Dictionary<string, string> NestedModuleEngineModuleMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                { "Microsoft.PowerShell.Commands.Utility", "Microsoft.PowerShell.Utility"},
                { "Microsoft.PowerShell.Commands.Management", "Microsoft.PowerShell.Management"},
                { "Microsoft.PowerShell.Commands.Diagnostics", "Microsoft.PowerShell.Diagnostics"},
                { "Microsoft.PowerShell.ConsoleHost", "Microsoft.PowerShell.Host"},
                { "Microsoft.PowerShell.Security", "Microsoft.PowerShell.Security"},
                { "Microsoft.WSMan.Management", "Microsoft.WSMan.Management"},
        // The list of engine modules that we will not allow users to remove
        internal static readonly HashSet<string> ConstantEngineModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                CoreModule,
        // The list of nested engine modules that we will not allow users to remove
        internal static readonly HashSet<string> ConstantEngineNestedModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                "System.Management.Automation",
        internal static string GetNestedModuleDllName(string moduleName)
            if (!EngineModuleNestedModuleMapping.TryGetValue(moduleName, out result))
    /// Set of helper methods fro loading assemblies containing cmdlets...
    internal static class PSSnapInHelpers
        internal static Assembly LoadPSSnapInAssembly(PSSnapInInfo psSnapInInfo)
            s_PSSnapInTracer.WriteLine("Loading assembly from GAC. Assembly Name: {0}", psSnapInInfo.AssemblyName);
                assembly = Assembly.Load(new AssemblyName(psSnapInInfo.AssemblyName));
            catch (BadImageFormatException e)
                s_PSSnapInTracer.TraceWarning("Not able to load assembly {0}: {1}", psSnapInInfo.AssemblyName, e.Message);
            catch (FileLoadException e)
            s_PSSnapInTracer.WriteLine("Loading assembly from path: {0}", psSnapInInfo.AssemblyName);
                AssemblyName assemblyName = AssemblyName.GetAssemblyName(psSnapInInfo.AbsoluteModulePath);
                if (!string.Equals(assemblyName.FullName, psSnapInInfo.AssemblyName, StringComparison.OrdinalIgnoreCase))
                    string message = StringUtil.Format(ConsoleInfoErrorStrings.PSSnapInAssemblyNameMismatch, psSnapInInfo.AbsoluteModulePath, psSnapInInfo.AssemblyName);
                    s_PSSnapInTracer.TraceError(message);
                    throw new PSSnapInException(psSnapInInfo.Name, message);
                assembly = Assembly.LoadFrom(psSnapInInfo.AbsoluteModulePath);
                s_PSSnapInTracer.TraceError("Not able to load assembly {0}: {1}", psSnapInInfo.AssemblyName, e.Message);
                throw new PSSnapInException(psSnapInInfo.Name, e.Message);
        private static bool TryGetCustomAttribute<T>(Type decoratedType, out T attribute) where T : Attribute
            var attributes = decoratedType.GetCustomAttributes<T>(inherit: false);
            attribute = attributes.FirstOrDefault();
            return attribute != null;
        internal static void AnalyzePSSnapInAssembly(
            Assembly assembly,
            PSSnapInInfo psSnapInInfo,
            PSModuleInfo moduleInfo,
            out string helpFile)
            helpFile = null;
            cmdlets = null;
            aliases = null;
            providers = null;
            // See if this assembly has already been scanned...
            Dictionary<string, Tuple<SessionStateCmdletEntry, List<SessionStateAliasEntry>>> cachedCmdlets;
            if (s_cmdletCache.Value.TryGetValue(assembly, out cachedCmdlets))
                cmdlets = new Dictionary<string, SessionStateCmdletEntry>(cachedCmdlets.Count, StringComparer.OrdinalIgnoreCase);
                aliases = new Dictionary<string, List<SessionStateAliasEntry>>(cachedCmdlets.Count, StringComparer.OrdinalIgnoreCase);
                foreach (var pair in cachedCmdlets)
                    var key = pair.Key;
                    var entry = pair.Value;
                    if (entry.Item1.PSSnapIn == null && psSnapInInfo != null)
                        entry.Item1.SetPSSnapIn(psSnapInInfo);
                    var newEntry = (SessionStateCmdletEntry)entry.Item1.Clone();
                    if (newEntry.PSSnapIn != null && psSnapInInfo == null)
                        newEntry.SetPSSnapIn(null);
                    cmdlets[key] = newEntry;
                    if (entry.Item2 != null)
                        var aliasList = new List<SessionStateAliasEntry>();
                        foreach (var alias in entry.Item2)
                            if (alias.PSSnapIn == null && psSnapInInfo != null)
                                alias.SetPSSnapIn(psSnapInInfo);
                            var newAliasEntry = (SessionStateAliasEntry)alias.Clone();
                            if (newAliasEntry.PSSnapIn != null && psSnapInInfo == null)
                                newAliasEntry.SetPSSnapIn(null);
                            aliasList.Add(newAliasEntry);
                        aliases[key] = aliasList;
            Dictionary<string, SessionStateProviderEntry> cachedProviders;
            if (s_providerCache.Value.TryGetValue(assembly, out cachedProviders))
                providers = new Dictionary<string, SessionStateProviderEntry>(s_providerCache.Value.Count, StringComparer.OrdinalIgnoreCase);
                foreach (var pair in cachedProviders)
                    if (entry.PSSnapIn == null && psSnapInInfo != null)
                        entry.SetPSSnapIn(psSnapInInfo);
                    var newEntry = (SessionStateProviderEntry)entry.Clone();
                    providers[key] = newEntry;
            if (cmdlets != null || providers != null)
                if (!s_assembliesWithModuleInitializerCache.Value.ContainsKey(assembly))
                    s_PSSnapInTracer.WriteLine("Returning cached cmdlet and provider entries for {0}", assemblyPath);
                    s_PSSnapInTracer.WriteLine("Executing IModuleAssemblyInitializer.Import for {0}", assemblyPath);
                    var assemblyTypes = GetAssemblyTypes(assembly, name);
                    ExecuteModuleInitializer(assembly, assemblyTypes);
            s_PSSnapInTracer.WriteLine("Analyzing assembly {0} for cmdlet and providers", assemblyPath);
            helpFile = GetHelpFile(assemblyPath);
            if (psSnapInInfo != null && psSnapInInfo.Name.Equals(InitialSessionState.CoreSnapin, StringComparison.OrdinalIgnoreCase))
                InitializeCoreCmdletsAndProviders(psSnapInInfo, out cmdlets, out providers, helpFile);
                // Make sure the pre-built cmdlet and provider tables match what reflection finds.  This will help
                // avoid issues where you add a cmdlet but forget to update the table in InitializeCoreCmdletsAndProviders.
                Dictionary<string, SessionStateCmdletEntry> cmdletsCheck = null;
                Dictionary<string, SessionStateProviderEntry> providersCheck = null;
                Dictionary<string, List<SessionStateAliasEntry>> aliasesCheck = null;
                AnalyzeModuleAssemblyWithReflection(assembly, name, psSnapInInfo, moduleInfo, helpFile, ref cmdletsCheck, ref aliasesCheck, ref providersCheck);
                Diagnostics.Assert(aliasesCheck == null, "InitializeCoreCmdletsAndProviders assumes no aliases are defined in System.Management.Automation.dll");
                Diagnostics.Assert(providersCheck.Count == providers.Count, "new Provider added to System.Management.Automation.dll - update InitializeCoreCmdletsAndProviders");
                foreach (var pair in providersCheck)
                    SessionStateProviderEntry other;
                    if (providers.TryGetValue(pair.Key, out other))
                        Diagnostics.Assert((object)pair.Value.HelpFileName == (object)other.HelpFileName, "Pre-generated Provider help file incorrect");
                        Diagnostics.Assert(pair.Value.ImplementingType == other.ImplementingType, "Pre-generated Provider implementing type incorrect");
                        Diagnostics.Assert(string.Equals(pair.Value.Name, other.Name, StringComparison.Ordinal), "Pre-generated Provider name incorrect");
                        Diagnostics.Assert(pair.Value.PSSnapIn == other.PSSnapIn, "Pre-generated Provider snapin type incorrect");
                        Diagnostics.Assert(pair.Value.Module == other.Module, "Pre-generated Provider module incorrect");
                        Diagnostics.Assert(pair.Value.Visibility == other.Visibility, "Pre-generated Provider visibility incorrect");
                        Diagnostics.Assert(false, "Missing provider: " + pair.Key);
                Diagnostics.Assert(cmdletsCheck.Count == cmdlets.Count, "new Cmdlet added to System.Management.Automation.dll - update InitializeCoreCmdletsAndProviders");
                foreach (var pair in cmdletsCheck)
                    SessionStateCmdletEntry other;
                    if (cmdlets.TryGetValue(pair.Key, out other))
                        Diagnostics.Assert(false, "Pre-generated Cmdlet missing: " + pair.Key);
                AnalyzeModuleAssemblyWithReflection(assembly, name, psSnapInInfo, moduleInfo, helpFile, ref cmdlets, ref aliases, ref providers);
            // Cache the cmdlet and provider info for this assembly...
            // We need to cache a clone of this data *before* the
            // module info is set on it since module info can't be shared
            // across runspaces. When these entries are hit in the cache,
            // copies will be returned to ensure that the cache is never tied to a runspace.
                var clone = new Dictionary<string, Tuple<SessionStateCmdletEntry, List<SessionStateAliasEntry>>>(cmdlets.Count, StringComparer.OrdinalIgnoreCase);
                List<SessionStateAliasEntry> aliasesCloneList = null;
                foreach (var entry in cmdlets)
                    List<SessionStateAliasEntry> aliasEntries;
                    if (aliases != null && aliases.TryGetValue(entry.Key, out aliasEntries))
                        aliasesCloneList = new List<SessionStateAliasEntry>(aliases.Count);
                        foreach (var aliasEntry in aliasEntries)
                            aliasesCloneList.Add((SessionStateAliasEntry)aliasEntry.Clone());
                    clone[entry.Key] = new Tuple<SessionStateCmdletEntry, List<SessionStateAliasEntry>>((SessionStateCmdletEntry)entry.Value.Clone(), aliasesCloneList);
                s_cmdletCache.Value[assembly] = clone;
                var clone = new Dictionary<string, SessionStateProviderEntry>(providers.Count, StringComparer.OrdinalIgnoreCase);
                foreach (var entry in providers)
                    clone[entry.Key] = (SessionStateProviderEntry)entry.Value.Clone();
                s_providerCache.Value[assembly] = clone;
        private static void AnalyzeModuleAssemblyWithReflection(
            ref Dictionary<string, SessionStateCmdletEntry> cmdlets,
            ref Dictionary<string, List<SessionStateAliasEntry>> aliases,
            ref Dictionary<string, SessionStateProviderEntry> providers)
            foreach (Type type in assemblyTypes)
                if (!HasDefaultConstructor(type))
                // Check for cmdlets
                if (IsCmdletClass(type) && TryGetCustomAttribute(type, out CmdletAttribute cmdletAttribute))
                    if (TryGetCustomAttribute(type, out ExperimentalAttribute expAttribute) && expAttribute.ToHide)
                        // If 'ExperimentalAttribute' is specified on the cmdlet type and the
                        // effective action at run time is 'Hide', then we ignore the type.
                    string cmdletName = cmdletAttribute.VerbName + "-" + cmdletAttribute.NounName;
                    if (cmdlets != null && cmdlets.ContainsKey(cmdletName))
                        string message = StringUtil.Format(ConsoleInfoErrorStrings.PSSnapInDuplicateCmdlets, cmdletName, name);
                        throw new PSSnapInException(name, message);
                    SessionStateCmdletEntry cmdlet = new SessionStateCmdletEntry(cmdletName, type, helpFile);
                    if (psSnapInInfo != null)
                        cmdlet.SetPSSnapIn(psSnapInInfo);
                    if (moduleInfo != null)
                        cmdlet.SetModule(moduleInfo);
                    cmdlets ??= new Dictionary<string, SessionStateCmdletEntry>(StringComparer.OrdinalIgnoreCase);
                    cmdlets.Add(cmdletName, cmdlet);
                    if (TryGetCustomAttribute(type, out AliasAttribute aliasAttribute))
                        aliases ??= new Dictionary<string, List<SessionStateAliasEntry>>(StringComparer.OrdinalIgnoreCase);
                        foreach (var alias in aliasAttribute.AliasNames)
                            // Alias declared by 'AliasAttribute' is set with the option 'ScopedItemOptions.None', because we believe
                            // the users of the cmdlet, instead of the author, should have control of what options applied to an alias
                            // ('ScopedItemOptions.ReadOnly' and/or 'ScopedItemOptions.AllScopes').
                            var aliasEntry = new SessionStateAliasEntry(alias, cmdletName, description: string.Empty, ScopedItemOptions.None);
                                aliasEntry.SetPSSnapIn(psSnapInInfo);
                                aliasEntry.SetModule(moduleInfo);
                            aliasList.Add(aliasEntry);
                        aliases.Add(cmdletName, aliasList);
                    s_PSSnapInTracer.WriteLine("{0} from type {1} is added as a cmdlet. ", cmdletName, type.FullName);
                // Check for providers
                else if (IsProviderClass(type) && TryGetCustomAttribute(type, out CmdletProviderAttribute providerAttribute))
                        // If 'ExperimentalAttribute' is specified on the provider type and
                        // the effective action at run time is 'Hide', then we ignore the type.
                    string providerName = providerAttribute.ProviderName;
                    if (providers != null && providers.ContainsKey(providerName))
                        string message = StringUtil.Format(ConsoleInfoErrorStrings.PSSnapInDuplicateProviders, providerName, psSnapInInfo.Name);
                    SessionStateProviderEntry provider = new SessionStateProviderEntry(providerName, type, helpFile);
                        provider.SetPSSnapIn(psSnapInInfo);
                        provider.SetModule(moduleInfo);
                    providers ??= new Dictionary<string, SessionStateProviderEntry>(StringComparer.OrdinalIgnoreCase);
                    providers.Add(providerName, provider);
                    s_PSSnapInTracer.WriteLine("{0} from type {1} is added as a provider. ", providerName, type.FullName);
        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1001:CommasMustBeSpacedCorrectly", Justification = "Reviewed.")]
        private static void InitializeCoreCmdletsAndProviders(
            string helpFile)
            cmdlets = new Dictionary<string, SessionStateCmdletEntry>(StringComparer.OrdinalIgnoreCase)
                { "Add-History",                       new SessionStateCmdletEntry("Add-History", typeof(AddHistoryCommand), helpFile) },
                { "Clear-History",                     new SessionStateCmdletEntry("Clear-History", typeof(ClearHistoryCommand), helpFile) },
                { "Debug-Job",                         new SessionStateCmdletEntry("Debug-Job", typeof(DebugJobCommand), helpFile) },
                { "Disable-PSRemoting",                new SessionStateCmdletEntry("Disable-PSRemoting", typeof(DisablePSRemotingCommand), helpFile) },
                { "Enable-PSRemoting",                 new SessionStateCmdletEntry("Enable-PSRemoting", typeof(EnablePSRemotingCommand), helpFile) },
                { "Disable-PSSessionConfiguration",    new SessionStateCmdletEntry("Disable-PSSessionConfiguration", typeof(DisablePSSessionConfigurationCommand), helpFile) },
                { "Enable-PSSessionConfiguration",     new SessionStateCmdletEntry("Enable-PSSessionConfiguration", typeof(EnablePSSessionConfigurationCommand), helpFile) },
                { "Get-PSSessionCapability",           new SessionStateCmdletEntry("Get-PSSessionCapability", typeof(GetPSSessionCapabilityCommand), helpFile) },
                { "Get-PSSessionConfiguration",        new SessionStateCmdletEntry("Get-PSSessionConfiguration", typeof(GetPSSessionConfigurationCommand), helpFile) },
                { "Receive-PSSession",                 new SessionStateCmdletEntry("Receive-PSSession", typeof(ReceivePSSessionCommand), helpFile) },
                { "Register-PSSessionConfiguration",   new SessionStateCmdletEntry("Register-PSSessionConfiguration", typeof(RegisterPSSessionConfigurationCommand), helpFile) },
                { "Unregister-PSSessionConfiguration", new SessionStateCmdletEntry("Unregister-PSSessionConfiguration", typeof(UnregisterPSSessionConfigurationCommand), helpFile) },
                { "Set-PSSessionConfiguration",        new SessionStateCmdletEntry("Set-PSSessionConfiguration", typeof(SetPSSessionConfigurationCommand), helpFile) },
                { "Test-PSSessionConfigurationFile",   new SessionStateCmdletEntry("Test-PSSessionConfigurationFile", typeof(TestPSSessionConfigurationFileCommand), helpFile) },
                { "Connect-PSSession",                 new SessionStateCmdletEntry("Connect-PSSession", typeof(ConnectPSSessionCommand), helpFile) },
                { "Disconnect-PSSession",              new SessionStateCmdletEntry("Disconnect-PSSession", typeof(DisconnectPSSessionCommand), helpFile) },
                { "Disable-ExperimentalFeature",       new SessionStateCmdletEntry("Disable-ExperimentalFeature", typeof(DisableExperimentalFeatureCommand), helpFile) },
                { "Enable-ExperimentalFeature",        new SessionStateCmdletEntry("Enable-ExperimentalFeature", typeof(EnableExperimentalFeatureCommand), helpFile) },
                { "Enter-PSHostProcess",               new SessionStateCmdletEntry("Enter-PSHostProcess", typeof(EnterPSHostProcessCommand), helpFile) },
                { "Enter-PSSession",                   new SessionStateCmdletEntry("Enter-PSSession", typeof(EnterPSSessionCommand), helpFile) },
                { "Exit-PSHostProcess",                new SessionStateCmdletEntry("Exit-PSHostProcess", typeof(ExitPSHostProcessCommand), helpFile) },
                { "Exit-PSSession",                    new SessionStateCmdletEntry("Exit-PSSession", typeof(ExitPSSessionCommand), helpFile) },
                { "Export-ModuleMember",               new SessionStateCmdletEntry("Export-ModuleMember", typeof(ExportModuleMemberCommand), helpFile) },
                { "ForEach-Object",                    new SessionStateCmdletEntry("ForEach-Object", typeof(ForEachObjectCommand), helpFile) },
                { "Get-Command",                       new SessionStateCmdletEntry("Get-Command", typeof(GetCommandCommand), helpFile) },
                { "Get-ExperimentalFeature",           new SessionStateCmdletEntry("Get-ExperimentalFeature", typeof(GetExperimentalFeatureCommand), helpFile) },
                { "Get-Help",                          new SessionStateCmdletEntry("Get-Help", typeof(GetHelpCommand), helpFile) },
                { "Get-History",                       new SessionStateCmdletEntry("Get-History", typeof(GetHistoryCommand), helpFile) },
                { "Get-Job",                           new SessionStateCmdletEntry("Get-Job", typeof(GetJobCommand), helpFile) },
                { "Get-Module",                        new SessionStateCmdletEntry("Get-Module", typeof(GetModuleCommand), helpFile) },
                { "Get-PSHostProcessInfo",             new SessionStateCmdletEntry("Get-PSHostProcessInfo", typeof(GetPSHostProcessInfoCommand), helpFile) },
                { "Get-PSSession",                     new SessionStateCmdletEntry("Get-PSSession", typeof(GetPSSessionCommand), helpFile) },
                { "Get-PSSubsystem",                   new SessionStateCmdletEntry("Get-PSSubsystem", typeof(Subsystem.GetPSSubsystemCommand), helpFile) },
                { "Import-Module",                     new SessionStateCmdletEntry("Import-Module", typeof(ImportModuleCommand), helpFile) },
                { "Invoke-Command",                    new SessionStateCmdletEntry("Invoke-Command", typeof(InvokeCommandCommand), helpFile) },
                { "Invoke-History",                    new SessionStateCmdletEntry("Invoke-History", typeof(InvokeHistoryCommand), helpFile) },
                { "New-Module",                        new SessionStateCmdletEntry("New-Module", typeof(NewModuleCommand), helpFile) },
                { "New-ModuleManifest",                new SessionStateCmdletEntry("New-ModuleManifest", typeof(NewModuleManifestCommand), helpFile) },
                { "New-PSRoleCapabilityFile",          new SessionStateCmdletEntry("New-PSRoleCapabilityFile", typeof(NewPSRoleCapabilityFileCommand), helpFile) },
                { "New-PSSession",                     new SessionStateCmdletEntry("New-PSSession", typeof(NewPSSessionCommand), helpFile) },
                { "New-PSSessionConfigurationFile",    new SessionStateCmdletEntry("New-PSSessionConfigurationFile", typeof(NewPSSessionConfigurationFileCommand), helpFile) },
                { "New-PSSessionOption",               new SessionStateCmdletEntry("New-PSSessionOption", typeof(NewPSSessionOptionCommand), helpFile) },
                { "New-PSTransportOption",             new SessionStateCmdletEntry("New-PSTransportOption", typeof(NewPSTransportOptionCommand), helpFile) },
                { "Out-Default",                       new SessionStateCmdletEntry("Out-Default", typeof(OutDefaultCommand), helpFile) },
                { "Out-Host",                          new SessionStateCmdletEntry("Out-Host", typeof(OutHostCommand), helpFile) },
                { "Out-Null",                          new SessionStateCmdletEntry("Out-Null", typeof(OutNullCommand), helpFile) },
                { "Receive-Job",                       new SessionStateCmdletEntry("Receive-Job", typeof(ReceiveJobCommand), helpFile) },
                { "Register-ArgumentCompleter",        new SessionStateCmdletEntry("Register-ArgumentCompleter", typeof(RegisterArgumentCompleterCommand), helpFile) },
                { "Remove-Job",                        new SessionStateCmdletEntry("Remove-Job", typeof(RemoveJobCommand), helpFile) },
                { "Remove-Module",                     new SessionStateCmdletEntry("Remove-Module", typeof(RemoveModuleCommand), helpFile) },
                { "Remove-PSSession",                  new SessionStateCmdletEntry("Remove-PSSession", typeof(RemovePSSessionCommand), helpFile) },
                { "Save-Help",                         new SessionStateCmdletEntry("Save-Help", typeof(SaveHelpCommand), helpFile) },
                { "Set-PSDebug",                       new SessionStateCmdletEntry("Set-PSDebug", typeof(SetPSDebugCommand), helpFile) },
                { "Set-StrictMode",                    new SessionStateCmdletEntry("Set-StrictMode", typeof(SetStrictModeCommand), helpFile) },
                { "Start-Job",                         new SessionStateCmdletEntry("Start-Job", typeof(StartJobCommand), helpFile) },
                { "Stop-Job",                          new SessionStateCmdletEntry("Stop-Job", typeof(StopJobCommand), helpFile) },
                { "Test-ModuleManifest",               new SessionStateCmdletEntry("Test-ModuleManifest", typeof(TestModuleManifestCommand), helpFile) },
                { "Update-Help",                       new SessionStateCmdletEntry("Update-Help", typeof(UpdateHelpCommand), helpFile) },
                { "Wait-Job",                          new SessionStateCmdletEntry("Wait-Job", typeof(WaitJobCommand), helpFile) },
                { "Where-Object",                      new SessionStateCmdletEntry("Where-Object", typeof(WhereObjectCommand), helpFile) },
                { "Add-PSSnapin",                      new SessionStateCmdletEntry("Add-PSSnapin", typeof(AddPSSnapinCommand), helpFile) },
                { "Export-Console",                    new SessionStateCmdletEntry("Export-Console", typeof(ExportConsoleCommand), helpFile) },
                { "Get-PSSnapin",                      new SessionStateCmdletEntry("Get-PSSnapin", typeof(GetPSSnapinCommand), helpFile) },
                { "Remove-PSSnapin",                   new SessionStateCmdletEntry("Remove-PSSnapin", typeof(RemovePSSnapinCommand), helpFile) },
                { "Resume-Job",                        new SessionStateCmdletEntry("Resume-Job", typeof(ResumeJobCommand), helpFile) },
                { "Suspend-Job",                       new SessionStateCmdletEntry("Suspend-Job", typeof(SuspendJobCommand), helpFile) },
                // Not exported, but are added via reflection so added here as well, though maybe they shouldn't be
                { "Out-LineOutput",                    new SessionStateCmdletEntry("Out-LineOutput", typeof(OutLineOutputCommand), helpFile) },
                { "Format-Default",                    new SessionStateCmdletEntry("Format-Default", typeof(FormatDefaultCommand), helpFile) },
            cmdlets.Add("Switch-Process", new SessionStateCmdletEntry("Switch-Process", typeof(SwitchProcessCommand), helpFile));
            foreach (var val in cmdlets.Values)
                val.SetPSSnapIn(psSnapInInfo);
            providers = new Dictionary<string, SessionStateProviderEntry>(StringComparer.OrdinalIgnoreCase)
                { "Registry",    new SessionStateProviderEntry("Registry", typeof(RegistryProvider), helpFile) },
                { "Alias",       new SessionStateProviderEntry("Alias", typeof(AliasProvider), helpFile) },
                { "Environment", new SessionStateProviderEntry("Environment", typeof(EnvironmentProvider), helpFile) },
                { "FileSystem" , new SessionStateProviderEntry("FileSystem", typeof(FileSystemProvider), helpFile) },
                { "Function",    new SessionStateProviderEntry("Function", typeof(FunctionProvider), helpFile) },
                { "Variable",    new SessionStateProviderEntry("Variable", typeof(VariableProvider), helpFile) },
            foreach (var val in providers.Values)
        private static void ExecuteModuleInitializer(Assembly assembly, IEnumerable<Type> assemblyTypes)
                if (typeof(IModuleAssemblyInitializer).IsAssignableFrom(type))
                    s_assembliesWithModuleInitializerCache.Value[assembly] = true;
                    var moduleInitializer = (IModuleAssemblyInitializer)Activator.CreateInstance(type, true);
                    moduleInitializer.OnImport();
        internal static IEnumerable<Type> GetAssemblyTypes(Assembly assembly, string name)
                // Return types that are public, non-abstract, non-interface and non-valueType.
                return assembly.ExportedTypes.Where(static t => !t.IsAbstract && !t.IsInterface && !t.IsValueType);
            catch (ReflectionTypeLoadException e)
                string message = e.Message + "\nLoader Exceptions: \n";
                if (e.LoaderExceptions != null)
                    foreach (Exception exception in e.LoaderExceptions)
                        message += "\n" + exception.Message;
        // cmdletCache holds the list of cmdlets along with its aliases per each assembly.
        private static readonly Lazy<ConcurrentDictionary<Assembly, Dictionary<string, Tuple<SessionStateCmdletEntry, List<SessionStateAliasEntry>>>>> s_cmdletCache =
            new Lazy<ConcurrentDictionary<Assembly, Dictionary<string, Tuple<SessionStateCmdletEntry, List<SessionStateAliasEntry>>>>>();
        private static readonly Lazy<ConcurrentDictionary<Assembly, Dictionary<string, SessionStateProviderEntry>>> s_providerCache =
            new Lazy<ConcurrentDictionary<Assembly, Dictionary<string, SessionStateProviderEntry>>>();
        // Using a ConcurrentDictionary for this so that we can avoid having a private lock variable. We use only the keys for checking.
        private static readonly Lazy<ConcurrentDictionary<Assembly, bool>> s_assembliesWithModuleInitializerCache = new Lazy<ConcurrentDictionary<Assembly, bool>>();
        private static bool IsCmdletClass(Type type)
            return type.IsSubclassOf(typeof(System.Management.Automation.Cmdlet));
        private static bool IsProviderClass(Type type)
            return type.IsSubclassOf(typeof(System.Management.Automation.Provider.CmdletProvider));
        private static bool HasDefaultConstructor(Type type)
            return type.GetConstructor(Type.EmptyTypes) is not null;
        private static string GetHelpFile(string assemblyPath)
            // Help files exist only for original module assemblies, not for generated Ngen binaries
            return Path.GetFileName(assemblyPath).Replace(".ni.dll", ".dll") + StringLiterals.HelpFileExtension;
    // Guid is {15d4c170-2f29-5689-a0e2-d95b0c7b4ea0}
    [EventSource(Name = "Microsoft-PowerShell-Runspaces")]
    internal class RunspaceEventSource : EventSource
        internal static readonly RunspaceEventSource Log = new RunspaceEventSource();
        public void OpenRunspaceStart() { WriteEvent(1); }
        public void OpenRunspaceStop() { WriteEvent(2); }
        public void LoadAssembliesStart() { WriteEvent(3); }
        public void LoadAssembliesStop() { WriteEvent(4); }
        public void UpdateFormatTableStart() { WriteEvent(5); }
        public void UpdateFormatTableStop() { WriteEvent(6); }
        public void UpdateTypeTableStart() { WriteEvent(7); }
        public void UpdateTypeTableStop() { WriteEvent(8); }
        public void LoadProvidersStart() { WriteEvent(9); }
        public void LoadProvidersStop() { WriteEvent(10); }
        public void LoadCommandsStart() { WriteEvent(11); }
        public void LoadCommandsStop() { WriteEvent(12); }
        public void LoadVariablesStart() { WriteEvent(13); }
        public void LoadVariablesStop() { WriteEvent(14); }
        public void LoadEnvironmentVariablesStart() { WriteEvent(15); }
        public void LoadEnvironmentVariablesStop() { WriteEvent(16); }
        public void LoadAssemblyStart(string Name, string FileName) { WriteEvent(17, Name, FileName); }
        public void LoadAssemblyStop(string Name, string FileName) { WriteEvent(18, Name, FileName); }
        public void ProcessFormatFileStart(string FileName) { WriteEvent(19, FileName); }
        public void ProcessFormatFileStop(string FileName) { WriteEvent(20, FileName); }
        public void ProcessTypeFileStart(string FileName) { WriteEvent(21, FileName); }
        public void ProcessTypeFileStop(string FileName) { WriteEvent(22, FileName); }
        public void LoadProviderStart(string Name) { WriteEvent(23, Name); }
        public void LoadProviderStop(string Name) { WriteEvent(24, Name); }
        public void LoadCommandStart(string Name) { WriteEvent(25, Name); }
        public void LoadCommandStop(string Name) { WriteEvent(26, Name); }

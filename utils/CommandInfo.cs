using System.Runtime.ExceptionServices;
    /// Defines the types of commands that PowerShell can execute.
    public enum CommandTypes
        /// Aliases create a name that refers to other command types.
        /// Aliases are only persisted within the execution of a single engine.
        Alias = 0x0001,
        /// Script functions that are defined by a script block.
        /// Functions are only persisted within the execution of a single engine.
        Function = 0x0002,
        /// Script filters that are defined by a script block.
        /// Filters are only persisted within the execution of a single engine.
        Filter = 0x0004,
        /// A cmdlet.
        Cmdlet = 0x0008,
        /// An PowerShell script (*.ps1 file)
        ExternalScript = 0x0010,
        /// Any existing application (can be console or GUI).
        /// An application can have any extension that can be executed either directly through CreateProcess
        /// or indirectly through ShellExecute.
        Application = 0x0020,
        /// A script that is built into the runspace configuration.
        Script = 0x0040,
        /// A Configuration.
        Configuration = 0x0100,
        /// All possible command types.
        /// NOTE: a CommandInfo instance will never specify All as its CommandType
        /// but All can be used when filtering the CommandTypes.
        All = Alias | Function | Filter | Cmdlet | Script | ExternalScript | Application | Configuration,
    /// The base class for the information about commands. Contains the basic information about
    /// the command, like name and type.
    public abstract class CommandInfo : IHasSessionStateEntryVisibility
        /// Creates an instance of the CommandInfo class with the specified name and type.
        /// The type of the command.
        /// If <paramref name="name"/> is null.
        internal CommandInfo(string name, CommandTypes type)
            // The name can be empty for functions and filters but it
            // can't be null
            CommandType = type;
        /// The execution context for the command.
        internal CommandInfo(string name, CommandTypes type, ExecutionContext context)
            : this(name, type)
        internal CommandInfo(CommandInfo other)
            // Computed fields not copied:
            // this._externalCommandMetadata = other._externalCommandMetadata;
            // this._moduleName = other._moduleName;
            // this.parameterSets = other.parameterSets;
            this.Module = other.Module;
            _visibility = other._visibility;
            Arguments = other.Arguments;
            this.Context = other.Context;
            Name = other.Name;
            CommandType = other.CommandType;
            CopiedCommand = other;
            this.DefiningLanguageMode = other.DefiningLanguageMode;
        internal CommandInfo(string name, CommandInfo other)
            : this(other)
        /// Gets the name of the command.
        public string Name { get; private set; } = string.Empty;
        // Name
        /// Gets the type of the command.
        public CommandTypes CommandType { get; private set; } = CommandTypes.Application;
        // CommandType
        /// Gets the source of the command (shown by default in Get-Command)
        public virtual string Source { get { return this.ModuleName; } }
        /// Gets the source version (shown by default in Get-Command)
        public virtual Version Version
                        if (Module.Version.Equals(new Version(0, 0)))
                            if (Module.Path.EndsWith(StringLiterals.PowerShellDataFileExtension, StringComparison.OrdinalIgnoreCase))
                                // Manifest module (.psd1)
                                Module.SetVersion(ModuleIntrinsics.GetManifestModuleVersion(Module.Path));
                            else if (Module.Path.EndsWith(StringLiterals.PowerShellILAssemblyExtension, StringComparison.OrdinalIgnoreCase) ||
                                     Module.Path.EndsWith(StringLiterals.PowerShellILExecutableExtension, StringComparison.OrdinalIgnoreCase))
                                // Binary module (.dll or .exe)
                                Module.SetVersion(AssemblyName.GetAssemblyName(Module.Path).Version);
                        _version = Module.Version;
        /// The execution context this command will run in.
                if ((value != null) && !this.DefiningLanguageMode.HasValue)
                    this.DefiningLanguageMode = value.LanguageMode;
        /// The language mode that was in effect when this alias was defined.
        internal PSLanguageMode? DefiningLanguageMode { get; set; }
        internal virtual HelpCategory HelpCategory
            get { return HelpCategory.None; }
        internal CommandInfo CopiedCommand { get; set; }
        /// Internal interface to change the type of a CommandInfo object.
        /// <param name="newType"></param>
        internal void SetCommandType(CommandTypes newType)
            CommandType = newType;
        /// This is overridden by derived classes to return specific
        /// information for the command type.
        public abstract string Definition { get; }
        /// This is required for renaming aliases, functions, and filters.
        /// The new name for the command.
        /// If <paramref name="newName"/> is null or empty.
        internal void Rename(string newName)
            ArgumentException.ThrowIfNullOrEmpty(newName);
            Name = newName;
        /// For diagnostic purposes.
            return ModuleCmdletBase.AddPrefixToCommandName(Name, Prefix);
        /// Indicates if the command is to be allowed to be executed by a request
        /// external to the runspace.
        public virtual SessionStateEntryVisibility Visibility
                return CopiedCommand == null ? _visibility : CopiedCommand.Visibility;
                if (CopiedCommand == null)
                    CopiedCommand.Visibility = value;
                if (value == SessionStateEntryVisibility.Private && Module != null)
                    Module.ModuleHasPrivateMembers = true;
        private SessionStateEntryVisibility _visibility = SessionStateEntryVisibility.Public;
        /// Return a CommandMetadata instance that is never exposed publicly.
        internal virtual CommandMetadata CommandMetadata
        /// Returns the syntax of a command.
        internal virtual string Syntax
            get { return Definition; }
        /// The module name of this command. It will be empty for commands
        /// not imported from either a module or snapin.
                string moduleName = null;
                if (Module != null && !string.IsNullOrEmpty(Module.Name))
                    moduleName = Module.Name;
                    CmdletInfo cmdlet = this as CmdletInfo;
                    if (cmdlet != null && cmdlet.PSSnapIn != null)
                        moduleName = cmdlet.PSSnapInName;
                if (moduleName == null)
                return moduleName;
        /// The module that defines this cmdlet. This will be null for commands
        /// that are not defined in the context of a module.
        public PSModuleInfo Module { get; internal set; }
        /// The remoting capabilities of this cmdlet, when exposed in a context
        public RemotingCapability RemotingCapability
                    return ExternalCommandMetadata.RemotingCapability;
                    // Thrown on an alias that hasn't been resolved yet (i.e.: in a module that
                    // hasn't been loaded.) Assume the default.
                    return RemotingCapability.PowerShell;
        /// True if the command has dynamic parameters, false otherwise.
        internal virtual bool ImplementsDynamicParameters
            get { return false; }
        /// Constructs the MergedCommandParameterMetadata, using any arguments that
        /// may have been specified so that dynamic parameters can be determined, if any.
        private MergedCommandParameterMetadata GetMergedCommandParameterMetadataSafely()
            if (_context == null)
            MergedCommandParameterMetadata result;
            if (_context != LocalPipeline.GetExecutionContextFromTLS())
                // In the normal case, _context is from the thread we're on, and we won't get here.
                // But, if it's not, we can't safely get the parameter metadata without running on
                // on the correct thread, because that thread may be busy doing something else.
                // One of the things we do here is change the current scope in execution context,
                // that can mess up the runspace our CommandInfo object came from.
                var runspace = (RunspaceBase)_context.CurrentRunspace;
                if (runspace.CanRunActionInCurrentPipeline())
                    GetMergedCommandParameterMetadata(out result);
                    _context.Events.SubscribeEvent(
                            source: null,
                            eventName: PSEngineEvent.GetCommandInfoParameterMetadata,
                            sourceIdentifier: PSEngineEvent.GetCommandInfoParameterMetadata,
                            data: null,
                            handlerDelegate: new PSEventReceivedEventHandler(OnGetMergedCommandParameterMetadataSafelyEventHandler),
                            supportEvent: true,
                            forwardEvent: false,
                            shouldQueueAndProcessInExecutionThread: true,
                            maxTriggerCount: 1);
                    var eventArgs = new GetMergedCommandParameterMetadataSafelyEventArgs();
                    _context.Events.GenerateEvent(
                        args: new[] { eventArgs },
                        waitForCompletionInCurrentThread: true);
                    // An exception happened on a different thread, rethrow it here on the correct thread.
                    eventArgs.Exception?.Throw();
                    return eventArgs.Result;
        private sealed class GetMergedCommandParameterMetadataSafelyEventArgs : EventArgs
            public MergedCommandParameterMetadata Result;
            public ExceptionDispatchInfo Exception;
        private void OnGetMergedCommandParameterMetadataSafelyEventHandler(object sender, PSEventArgs args)
            var eventArgs = args.SourceEventArgs as GetMergedCommandParameterMetadataSafelyEventArgs;
                    // Save the result in our event args as the return value.
                    GetMergedCommandParameterMetadata(out eventArgs.Result);
                    // Save the exception so we can throw it on the correct thread.
                    eventArgs.Exception = ExceptionDispatchInfo.Capture(e);
        private void GetMergedCommandParameterMetadata(out MergedCommandParameterMetadata result)
            // MSFT:652277 - When invoking cmdlets or advanced functions, MyInvocation.MyCommand.Parameters do not contain the dynamic parameters
            // When trying to get parameter metadata for a CommandInfo that has dynamic parameters, a new CommandProcessor will be
            // created out of this CommandInfo and the parameter binding algorithm will be invoked. However, when this happens via
            // 'MyInvocation.MyCommand.Parameter', it's actually retrieving the parameter metadata of the same cmdlet that is currently
            // running. In this case, information about the specified parameters are not kept around in 'MyInvocation.MyCommand', so
            // going through the binding algorithm again won't give us the metadata about the dynamic parameters that should have been
            // discovered already.
            // The fix is to check if the CommandInfo is actually representing the currently running cmdlet. If so, the retrieval of parameter
            // metadata actually stems from the running of the same cmdlet. In this case, we can just use the current CommandProcessor to
            // retrieve all bindable parameters, which should include the dynamic parameters that have been discovered already.
            CommandProcessor processor;
            if (Context.CurrentCommandProcessor != null && Context.CurrentCommandProcessor.CommandInfo == this)
                // Accessing the parameters within the invocation of the same cmdlet/advanced function.
                processor = (CommandProcessor)Context.CurrentCommandProcessor;
                IScriptCommandInfo scriptCommand = this as IScriptCommandInfo;
                processor = scriptCommand != null
                    ? new CommandProcessor(scriptCommand, _context, useLocalScope: true, fromScriptFile: false,
                        sessionState: scriptCommand.ScriptBlock.SessionStateInternal ?? Context.EngineSessionState)
                    : new CommandProcessor((CmdletInfo)this, _context);
                ParameterBinderController.AddArgumentsToCommandProcessor(processor, Arguments);
                CommandProcessorBase oldCurrentCommandProcessor = Context.CurrentCommandProcessor;
                    Context.CurrentCommandProcessor = processor;
                    processor.SetCurrentScopeToExecutionScope();
                    processor.CmdletParameterBinderController.BindCommandLineParametersNoValidation(processor.arguments);
                    // Ignore the binding exception if no argument is specified
                    if (processor.arguments.Count > 0)
                    Context.CurrentCommandProcessor = oldCurrentCommandProcessor;
                    processor.RestorePreviousScope();
            result = processor.CmdletParameterBinderController.BindableParameters;
        /// Return the parameters for this command.
        public virtual Dictionary<string, ParameterMetadata> Parameters
                Dictionary<string, ParameterMetadata> result = new Dictionary<string, ParameterMetadata>(StringComparer.OrdinalIgnoreCase);
                if (ImplementsDynamicParameters && Context != null)
                    MergedCommandParameterMetadata merged = GetMergedCommandParameterMetadataSafely();
                    foreach (KeyValuePair<string, MergedCompiledCommandParameter> pair in merged.BindableParameters)
                        result.Add(pair.Key, new ParameterMetadata(pair.Value.Parameter));
                    // Don't cache this data...
                return ExternalCommandMetadata.Parameters;
        internal CommandMetadata ExternalCommandMetadata
            get { return _externalCommandMetadata ??= new CommandMetadata(this, true); }
            set { _externalCommandMetadata = value; }
        private CommandMetadata _externalCommandMetadata;
        /// Resolves a full, shortened, or aliased parameter name to the actual
        /// cmdlet parameter name, using PowerShell's standard parameter resolution
        /// algorithm.
        /// <param name="name">The name of the parameter to resolve.</param>
        /// <returns>The parameter that matches this name.</returns>
        public ParameterMetadata ResolveParameter(string name)
            MergedCompiledCommandParameter result = merged.GetMatchingParameter(name, true, true, null);
            return this.Parameters[result.Parameter.Name];
        /// Gets the information about the parameters and parameter sets for
        /// this command.
        public ReadOnlyCollection<CommandParameterSetInfo> ParameterSets
                if (_parameterSets == null)
                    Collection<CommandParameterSetInfo> parameterSetInfo =
                        GenerateCommandParameterSetInfo();
                    _parameterSets = new ReadOnlyCollection<CommandParameterSetInfo>(parameterSetInfo);
                return _parameterSets;
        internal ReadOnlyCollection<CommandParameterSetInfo> _parameterSets;
        /// A possibly incomplete or even incorrect list of types the command could return.
        public abstract ReadOnlyCollection<PSTypeName> OutputType { get; }
        /// Specifies whether this command was imported from a module or not.
        /// This is used in Get-Command to figure out which of the commands in module session state were imported.
        internal bool IsImported { get; set; } = false;
        /// The prefix that was used when importing this command.
        internal virtual CommandInfo CreateGetCommandCopy(object[] argumentList)
        /// Generates the parameter and parameter set info from the cmdlet metadata.
        /// A collection of CommandParameterSetInfo representing the cmdlet metadata.
        internal Collection<CommandParameterSetInfo> GenerateCommandParameterSetInfo()
            Collection<CommandParameterSetInfo> result;
            if (IsGetCommandCopy && ImplementsDynamicParameters)
                result = GetParameterMetadata(CommandMetadata, GetMergedCommandParameterMetadataSafely());
                result = GetCacheableMetadata(CommandMetadata);
        /// Gets or sets whether this CmdletInfo instance is a copy used for get-command.
        /// If true, and the cmdlet supports dynamic parameters, it means that the dynamic
        /// parameter metadata will be merged into the parameter set information.
        internal bool IsGetCommandCopy { get; set; }
        /// Gets or sets the command line arguments/parameters that were specified
        /// which will allow for the dynamic parameters to be retrieved and their
        /// metadata merged into the parameter set information.
        internal object[] Arguments { get; set; }
        internal static Collection<CommandParameterSetInfo> GetCacheableMetadata(CommandMetadata metadata)
            return GetParameterMetadata(metadata, metadata.StaticCommandParameterMetadata);
        internal static Collection<CommandParameterSetInfo> GetParameterMetadata(CommandMetadata metadata, MergedCommandParameterMetadata parameterMetadata)
            Collection<CommandParameterSetInfo> result = new Collection<CommandParameterSetInfo>();
            if (parameterMetadata != null)
                if (parameterMetadata.ParameterSetCount == 0)
                    const string parameterSetName = ParameterAttribute.AllParameterSets;
                        new CommandParameterSetInfo(
                            uint.MaxValue,
                            parameterMetadata));
                    int parameterSetCount = parameterMetadata.ParameterSetCount;
                    for (int index = 0; index < parameterSetCount; ++index)
                        uint currentFlagPosition = (uint)0x1 << index;
                        // Get the parameter set name
                        string parameterSetName = parameterMetadata.GetParameterSetName(currentFlagPosition);
                        // Is the parameter set the default?
                        bool isDefaultParameterSet = (currentFlagPosition & metadata.DefaultParameterSetFlag) != 0;
                                isDefaultParameterSet,
                                currentFlagPosition,
    /// Represents <see cref="System.Type"/>, but can be used where a real type
    /// might not be available, in which case the name of the type can be used.
    public class PSTypeName
        /// This constructor is used when the type exists and is currently loaded.
        /// <param name="type">The type.</param>
        public PSTypeName(Type type)
            if (_type != null)
                Name = _type.FullName;
        /// This constructor is used when the type may not exist, or is not loaded.
        /// <param name="name">The name of the type.</param>
        public PSTypeName(string name)
            _type = null;
        /// This constructor is used when the creating a PSObject with a custom typename.
        /// <param name="type">The real type.</param>
        public PSTypeName(string name, Type type)
        /// This constructor is used when the type is defined in PowerShell.
        /// <param name="typeDefinitionAst">The type definition from the ast.</param>
        public PSTypeName(TypeDefinitionAst typeDefinitionAst)
            if (typeDefinitionAst == null)
                throw PSTraceSource.NewArgumentNullException(nameof(typeDefinitionAst));
            TypeDefinitionAst = typeDefinitionAst;
            Name = typeDefinitionAst.Name;
        /// This constructor creates a type from a ITypeName.
        public PSTypeName(ITypeName typeName)
            if (typeName == null)
            _type = typeName.GetReflectionType();
                var t = typeName as TypeName;
                if (t != null && t._typeDefinitionAst != null)
                    TypeDefinitionAst = t._typeDefinitionAst;
                    Name = TypeDefinitionAst.Name;
                    Name = typeName.FullName;
        /// Return the name of the type.
        /// Return the type with metadata, or null if the type is not loaded.
        public Type Type
                if (!_typeWasCalculated)
                    if (_type == null)
                        if (TypeDefinitionAst != null)
                            _type = TypeDefinitionAst.Type;
                            TypeResolver.TryResolveType(Name, out _type);
                        // We ignore the exception.
                        if (Name != null &&
                            Name.StartsWith('[') &&
                            Name.EndsWith(']'))
                            string tmp = Name.Substring(1, Name.Length - 2);
                            TypeResolver.TryResolveType(tmp, out _type);
                    _typeWasCalculated = true;
        private Type _type;
        /// When a type is defined by PowerShell, the ast for that type.
        public TypeDefinitionAst TypeDefinitionAst { get; }
        private bool _typeWasCalculated;
        /// Returns a String that represents the current PSTypeName.
        /// <returns>String that represents the current PSTypeName.</returns>
            return Name ?? string.Empty;
    [DebuggerDisplay("{PSTypeName} {Name}")]
    internal readonly struct PSMemberNameAndType
        public readonly string Name;
        public readonly PSTypeName PSTypeName;
        public readonly object Value;
        public PSMemberNameAndType(string name, PSTypeName typeName, object value = null)
            PSTypeName = typeName;
    /// Represents dynamic types such as <see cref="System.Management.Automation.PSObject"/>,
    /// but can be used where a real type might not be available, in which case the name of the type can be used.
    /// The type encodes the members of dynamic objects in the type name.
    internal sealed class PSSyntheticTypeName : PSTypeName
        internal static PSSyntheticTypeName Create(string typename, IList<PSMemberNameAndType> membersTypes) => Create(new PSTypeName(typename), membersTypes);
        internal static PSSyntheticTypeName Create(Type type, IList<PSMemberNameAndType> membersTypes) => Create(new PSTypeName(type), membersTypes);
        internal static PSSyntheticTypeName Create(PSTypeName typename, IList<PSMemberNameAndType> membersTypes)
            var typeName = GetMemberTypeProjection(typename.Name, membersTypes);
            var members = new List<PSMemberNameAndType>();
            members.AddRange(membersTypes);
            members.Sort(static (c1, c2) => string.Compare(c1.Name, c2.Name, StringComparison.OrdinalIgnoreCase));
            return new PSSyntheticTypeName(typeName, typename.Type, members);
        private PSSyntheticTypeName(string typeName, Type type, IList<PSMemberNameAndType> membersTypes)
        : base(typeName, type)
            Members = membersTypes;
            if (type != typeof(PSObject))
            for (int i = 0; i < Members.Count; i++)
                var psMemberNameAndType = Members[i];
                if (IsPSTypeName(psMemberNameAndType))
                    Members.RemoveAt(i);
        private static bool IsPSTypeName(in PSMemberNameAndType member) => member.Name.Equals(nameof(PSTypeName), StringComparison.OrdinalIgnoreCase);
        private static string GetMemberTypeProjection(string typename, IList<PSMemberNameAndType> members)
            if (typename == typeof(PSObject).FullName)
                foreach (var mem in members)
                    if (IsPSTypeName(mem))
                        typename = mem.Value.ToString();
            var builder = new StringBuilder(typename, members.Count * 7);
            builder.Append('#');
            foreach (var m in members.OrderBy(static m => m.Name))
                if (!IsPSTypeName(m))
                    builder.Append(m.Name).Append(':');
            builder.Length--;
            return builder.ToString();
        public IList<PSMemberNameAndType> Members { get; }
    internal interface IScriptCommandInfo
        ScriptBlock ScriptBlock { get; }

using PipelineResultTypes = System.Management.Automation.Runspaces.PipelineResultTypes;
    #region Auxiliary
    /// An interface that a
    /// <see cref="Cmdlet"/> or <see cref="Provider.CmdletProvider"/>
    /// must implement to indicate that it has dynamic parameters.
    /// Dynamic parameters allow a
    /// to define additional parameters based on the value of
    /// the formal arguments.  For example, the parameters of
    /// "set-itemproperty" for the file system provider vary
    /// depending on whether the target object is a file or directory.
    /// <seealso cref="Cmdlet"/>
    /// <seealso cref="PSCmdlet"/>
    /// <seealso cref="RuntimeDefinedParameter"/>
    /// <seealso cref="RuntimeDefinedParameterDictionary"/>
    public interface IDynamicParameters
        /// Returns an instance of an object that defines the
        /// dynamic parameters for this
        /// <see cref="Cmdlet"/> or <see cref="Provider.CmdletProvider"/>.
        /// This method should return an object that has properties and fields
        /// decorated with parameter attributes similar to a
        /// These attributes include <see cref="ParameterAttribute"/>,
        /// <see cref="AliasAttribute"/>, argument transformation and
        /// validation attributes, etc.
        /// Alternately, it can return a
        /// <see cref="System.Management.Automation.RuntimeDefinedParameterDictionary"/>
        /// instead.
        /// The <see cref="Cmdlet"/> or <see cref="Provider.CmdletProvider"/>
        /// should hold on to a reference to the object which it returns from
        /// this method, since the argument values for the dynamic parameters
        /// specified by that object will be set in that object.
        /// This method will be called after all formal (command-line)
        /// parameters are set, but before <see cref="Cmdlet.BeginProcessing"/>
        /// is called and before any incoming pipeline objects are read.
        /// Therefore, parameters which allow input from the pipeline
        /// may not be set at the time this method is called,
        /// even if the parameters are mandatory.
        object? GetDynamicParameters();
    /// Type used to define a parameter on a cmdlet script of function that
    /// can only be used as a switch.
    public readonly struct SwitchParameter
        private readonly bool _isPresent;
        /// Returns true if the parameter was specified on the command line, false otherwise.
        /// <value>True if the parameter was specified, false otherwise</value>
        public bool IsPresent
            get { return _isPresent; }
        /// Implicit cast operator for casting SwitchParameter to bool.
        /// <param name="switchParameter">The SwitchParameter object to convert to bool.</param>
        /// <returns>The corresponding boolean value.</returns>
        public static implicit operator bool(SwitchParameter switchParameter)
            return switchParameter.IsPresent;
        /// Implicit cast operator for casting bool to SwitchParameter.
        /// <param name="value">The bool to convert to SwitchParameter.</param>
        public static implicit operator SwitchParameter(bool value)
            return new SwitchParameter(value);
        /// Explicit method to convert a SwitchParameter to a boolean value.
        /// <returns>The boolean equivalent of the SwitchParameter.</returns>
        public bool ToBool()
            return _isPresent;
        /// Construct a SwitchParameter instance with a particular value.
        /// <param name="isPresent">
        /// If true, it indicates that the switch is present, false otherwise.
        public SwitchParameter(bool isPresent)
            _isPresent = isPresent;
        /// Static method that returns a instance of SwitchParameter that indicates that it is present.
        /// <value>An instance of a switch parameter that will convert to true in a boolean context</value>
        public static SwitchParameter Present
            get { return new SwitchParameter(true); }
        /// Compare this switch parameter to another object.
        /// <param name="obj">An object to compare against.</param>
        /// <returns>True if the objects are the same value.</returns>
            if (obj is bool)
                return _isPresent == (bool)obj;
            else if (obj is SwitchParameter)
                return _isPresent == ((SwitchParameter)obj).IsPresent;
        /// Returns the hash code for this switch parameter.
        /// <returns>The hash code for this cobject.</returns>
            return _isPresent.GetHashCode();
        /// Implement the == operator for switch parameters objects.
        /// <param name="first">First object to compare.</param>
        /// <param name="second">Second object to compare.</param>
        /// <returns>True if they are the same.</returns>
        public static bool operator ==(SwitchParameter first, SwitchParameter second)
            return first.Equals(second);
        /// Implement the != operator for switch parameters.
        /// <returns>True if they are different.</returns>
        public static bool operator !=(SwitchParameter first, SwitchParameter second)
            return !first.Equals(second);
        /// Implement the == operator for switch parameters and booleans.
        public static bool operator ==(SwitchParameter first, bool second)
        /// Implement the != operator for switch parameters and booleans.
        public static bool operator !=(SwitchParameter first, bool second)
        /// Implement the == operator for bool and switch parameters.
        public static bool operator ==(bool first, SwitchParameter second)
        /// Implement the != operator for bool and switch parameters.
        public static bool operator !=(bool first, SwitchParameter second)
        /// Returns the string representation for this object.
        /// <returns>The string for this object.</returns>
            return _isPresent.ToString();
    /// Interfaces that cmdlets can use to build script blocks and execute scripts.
    public class CommandInvocationIntrinsics
        internal CommandInvocationIntrinsics(ExecutionContext context, PSCmdlet cmdlet)
                _commandRuntime = cmdlet.CommandRuntime as MshCommandRuntime;
        internal CommandInvocationIntrinsics(ExecutionContext context)
            : this(context, null)
        /// If an error occurred while executing the cmdlet, this will be set to true.
        public bool HasErrors
                return _commandRuntime.PipelineProcessor.ExecutionFailed;
                _commandRuntime.PipelineProcessor.ExecutionFailed = value;
        /// Returns a string with all of the variable and expression substitutions done.
        /// <param name="source">The string to expand.
        /// <returns>The expanded string.</returns>
        /// <exception cref="ParseException">
        /// Thrown if a parse exception occurred during subexpression substitution.
        public string ExpandString(string source)
            _cmdlet?.ThrowIfStopping();
            return _context.Engine.Expand(source);
        public CommandInfo GetCommand(string commandName, CommandTypes type)
            return GetCommand(commandName, type, null);
        /// Returns a command info for a given command name and type, using the specified arguments
        /// to resolve dynamic parameters.
        /// <param name="commandName">The command name to search for.</param>
        /// <param name="type">The command type to search for.</param>
        /// <param name="arguments">The command arguments used to resolve dynamic parameters.</param>
        /// <returns>A CommandInfo result that represents the resolved command.</returns>
        public CommandInfo GetCommand(string commandName, CommandTypes type, object[] arguments)
                CommandOrigin commandOrigin = CommandOrigin.Runspace;
                if (_cmdlet != null)
                    commandOrigin = _cmdlet.CommandOrigin;
                else if (_context != null)
                    commandOrigin = _context.EngineSessionState.CurrentScope.ScopeOrigin;
                result = CommandDiscovery.LookupCommandInfo(commandName, type, SearchResolutionOptions.None, commandOrigin, _context);
                if ((result != null) && (arguments != null) && (arguments.Length > 0))
                    // We've been asked to retrieve dynamic parameters
                    if (result.ImplementsDynamicParameters)
                        result = result.CreateGetCommandCopy(arguments);
        /// This event handler is called when a command is not found.
        /// If should have a single string parameter that is the name
        /// of the command and should return a CommandInfo object or null. By default
        /// it will search the module path looking for a module that exports the
        /// desired command.
        public System.EventHandler<CommandLookupEventArgs> CommandNotFoundAction { get; set; }
        /// This event handler is called before the command lookup is done.
        /// of the command and should return a CommandInfo object or null.
        public System.EventHandler<CommandLookupEventArgs> PreCommandLookupAction { get; set; }
        /// This event handler is after the command lookup is done but before the event object is
        /// returned to the caller. This allows things like interning scripts to work.
        public System.EventHandler<CommandLookupEventArgs> PostCommandLookupAction { get; set; }
        /// Gets or sets the action that is invoked every time the runspace location (cwd) is changed.
        public System.EventHandler<LocationChangedEventArgs> LocationChangedAction { get; set; }
        /// Returns the CmdletInfo object that corresponds to the name argument.
        /// <param name="commandName">The name of the cmdlet to look for.</param>
        /// <returns>The cmdletInfo object if found, null otherwise.</returns>
        public CmdletInfo GetCmdlet(string commandName)
            return GetCmdlet(commandName, _context);
        /// <param name="context">The execution context instance to use for lookup.</param>
        internal static CmdletInfo GetCmdlet(string commandName, ExecutionContext context)
            CmdletInfo current = null;
            CommandSearcher searcher = new CommandSearcher(
                    CommandTypes.Cmdlet,
                current = ((IEnumerator)searcher).Current as CmdletInfo;
            return current;
        /// Get the cmdlet info using the name of the cmdlet's implementing type. This bypasses
        /// session state and retrieves the command directly. Note that the help file and snapin/module
        /// info will both be null on returned object.
        /// <param name="cmdletTypeName">The type name of the class implementing this cmdlet.</param>
        /// <returns>CmdletInfo for the cmdlet if found, null otherwise.</returns>
        public CmdletInfo GetCmdletByTypeName(string cmdletTypeName)
            if (string.IsNullOrEmpty(cmdletTypeName))
                throw PSTraceSource.NewArgumentNullException(nameof(cmdletTypeName));
            Type cmdletType = TypeResolver.ResolveType(cmdletTypeName, out e);
            if (cmdletType == null)
            CmdletAttribute ca = null;
            foreach (var attr in cmdletType.GetCustomAttributes(true))
                ca = attr as CmdletAttribute;
                if (ca != null)
            if (ca == null)
            string noun = ca.NounName;
            string verb = ca.VerbName;
            string cmdletName = verb + "-" + noun;
            return new CmdletInfo(cmdletName, cmdletType, null, null, _context);
        /// Returns a list of all cmdlets...
        public List<CmdletInfo> GetCmdlets()
            return GetCmdlets("*");
        /// Returns all cmdlets whose names match the pattern...
        /// <returns>A list of CmdletInfo objects...</returns>
        public List<CmdletInfo> GetCmdlets(string pattern)
            if (pattern == null)
                throw PSTraceSource.NewArgumentNullException(nameof(pattern));
            List<CmdletInfo> cmdlets = new List<CmdletInfo>();
                    SearchResolutionOptions.CommandNameIsPattern,
                    _context);
                if (current != null)
                    cmdlets.Add(current);
            return cmdlets;
        /// Searches for PowerShell commands, optionally using wildcard patterns
        /// and optionally return the full path to applications and scripts rather than
        /// the simple command name.
        /// <param name="name">The name of the command to use.</param>
        /// <param name="nameIsPattern">If true treat the name as a pattern to search for.</param>
        /// <param name="returnFullName">If true, return the full path to scripts and applications.</param>
        /// <returns>A list of command names...</returns>
        public List<string> GetCommandName(string name, bool nameIsPattern, bool returnFullName)
            List<string> commands = new List<string>();
            foreach (CommandInfo current in this.GetCommands(name, CommandTypes.All, nameIsPattern))
                if (current.CommandType == CommandTypes.Application)
                    string cmdExtension = System.IO.Path.GetExtension(current.Name);
                    if (!string.IsNullOrEmpty(cmdExtension))
                        // Only add the application in PATHEXT...
                            if (extension.Equals(cmdExtension, StringComparison.OrdinalIgnoreCase))
                                if (returnFullName)
                                    commands.Add(current.Definition);
                                    commands.Add(current.Name);
                else if (current.CommandType == CommandTypes.ExternalScript)
            return commands;
        /// Searches for PowerShell commands, optionally using wildcard patterns.
        /// <param name="commandTypes">Type of commands to support.</param>
        /// <returns>Collection of command names...</returns>
        public IEnumerable<CommandInfo> GetCommands(string name, CommandTypes commandTypes, bool nameIsPattern)
            SearchResolutionOptions options = nameIsPattern ?
                (SearchResolutionOptions.CommandNameIsPattern | SearchResolutionOptions.ResolveFunctionPatterns | SearchResolutionOptions.ResolveAliasPatterns)
                : SearchResolutionOptions.None;
            return GetCommands(name, commandTypes, options);
        internal IEnumerable<CommandInfo> GetCommands(string name, CommandTypes commandTypes, SearchResolutionOptions options, CommandOrigin? commandOrigin = null)
            if (commandOrigin != null)
                searcher.CommandOrigin = commandOrigin.Value;
                CommandInfo commandInfo = ((IEnumerator)searcher).Current as CommandInfo;
        /// Executes a piece of text as a script synchronously in the caller's session state.
        /// The given text will be executed in a child scope rather than dot-sourced.
        /// <param name="script">The script text to evaluate.</param>
        /// <returns>A collection of PSObjects generated by the script. Never null, but may be empty.</returns>
        /// <exception cref="ParseException">Thrown if there was a parsing error in the script.</exception>
        /// <exception cref="RuntimeException">Represents a script-level exception.</exception>
        /// <exception cref="FlowControlException"></exception>
        public Collection<PSObject> InvokeScript(string script)
            return InvokeScript(script, useNewScope: true, PipelineResultTypes.None, input: null);
        /// <param name="args">The arguments to the script, available as $args.</param>
        public Collection<PSObject> InvokeScript(string script, params object[] args)
            return InvokeScript(script, useNewScope: true, PipelineResultTypes.None, input: null, args);
        /// Executes a given scriptblock synchronously in the given session state.
        /// The scriptblock will be executed in the calling scope (dot-sourced) rather than in a new child scope.
        /// <param name="sessionState">The session state in which to execute the scriptblock.</param>
        /// <param name="scriptBlock">The scriptblock to execute.</param>
        /// <param name="args">The arguments to the scriptblock, available as $args.</param>
        /// <returns>A collection of the PSObjects emitted by the executing scriptblock. Never null, but may be empty.</returns>
        public Collection<PSObject> InvokeScript(
            SessionStateInternal _oldSessionState = _context.EngineSessionState;
                _context.EngineSessionState = sessionState.Internal;
                return InvokeScript(
                    sb: scriptBlock,
                    useNewScope: false,
                    writeToPipeline: PipelineResultTypes.None,
                    input: null,
                    args: args);
                _context.EngineSessionState = _oldSessionState;
        /// Invoke a scriptblock in the current runspace, controlling if it gets a new scope.
        /// <param name="useLocalScope">If true, executes the scriptblock in a new child scope, otherwise the scriptblock is dot-sourced into the calling scope.</param>
        /// <param name="input">Optional input to the command.</param>
        /// <param name="args">Arguments to pass to the scriptblock.</param>
        /// A collection of the PSObjects generated by executing the script. Never null, but may be empty.
            bool useLocalScope,
            IList input,
            // Force the current runspace onto the callers thread - this is needed
            // if this API is going to be callable through the SessionStateProxy on the runspace.
            var old = System.Management.Automation.Runspaces.Runspace.DefaultRunspace;
            System.Management.Automation.Runspaces.Runspace.DefaultRunspace = _context.CurrentRunspace;
                return InvokeScript(scriptBlock, useLocalScope, PipelineResultTypes.None, input, args);
                System.Management.Automation.Runspaces.Runspace.DefaultRunspace = old;
        /// Executes a piece of text as a script synchronously using the options provided.
        /// <param name="script">The script to evaluate.</param>
        /// <param name="useNewScope">If true, evaluate the script in its own scope.
        /// If false, the script will be evaluated in the current scope i.e. it will be dot-sourced.</param>
        /// <param name="writeToPipeline">If set to Output, all output will be streamed
        /// to the output pipe of the calling cmdlet. If set to None, the result will be returned
        /// to the caller as a collection of PSObjects. No other flags are supported at this time and
        /// will result in an exception if used.</param>
        /// <param name="input">The list of objects to use as input to the script.</param>
        /// <param name="args">The array of arguments to the command, available as $args.</param>
        /// <returns>A collection of PSObjects generated by the script. This will be
        /// empty if output was redirected. Never null.</returns>
        /// <exception cref="NotImplementedException">Thrown if any redirect other than output is attempted.</exception>
            string script,
            bool useNewScope,
            PipelineResultTypes writeToPipeline,
            ArgumentNullException.ThrowIfNull(script);
            // Compile the script text into an executable script block.
            ScriptBlock sb = ScriptBlock.Create(_context, script);
            return InvokeScript(sb, useNewScope, writeToPipeline, input, args);
        private Collection<PSObject> InvokeScript(
            ScriptBlock sb,
            Cmdlet cmdletToUse = null;
            ScriptBlock.ErrorHandlingBehavior errorHandlingBehavior = ScriptBlock.ErrorHandlingBehavior.WriteToExternalErrorPipe;
            // Check if they want output
            if ((writeToPipeline & PipelineResultTypes.Output) == PipelineResultTypes.Output)
                cmdletToUse = _cmdlet;
                writeToPipeline &= (~PipelineResultTypes.Output);
            // Check if they want error
            if ((writeToPipeline & PipelineResultTypes.Error) == PipelineResultTypes.Error)
                errorHandlingBehavior = ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe;
                writeToPipeline &= (~PipelineResultTypes.Error);
            if (writeToPipeline != PipelineResultTypes.None)
                // The only output types are Output and Error.
            // If the cmdletToUse is not null, then the result of the evaluation will be
            // streamed out the output pipe of the cmdlet.
            object rawResult;
            if (cmdletToUse != null)
                sb.InvokeUsingCmdlet(
                    contextCmdlet: cmdletToUse,
                    useLocalScope: useNewScope,
                    errorHandlingBehavior: errorHandlingBehavior,
                    input: input,
                rawResult = AutomationNull.Value;
                rawResult = sb.DoInvokeReturnAsIs(
            if (rawResult == AutomationNull.Value)
                return new Collection<PSObject>();
            // If the result is already a collection of PSObjects, just return it...
            Collection<PSObject> result = rawResult as Collection<PSObject>;
            result = new Collection<PSObject>();
            IEnumerator list = null;
            list = LanguagePrimitives.GetEnumerator(rawResult);
                    object val = list.Current;
                    result.Add(LanguagePrimitives.AsPSObjectOrNull(val));
                result.Add(LanguagePrimitives.AsPSObjectOrNull(rawResult));
        /// Compile a string into a script block.
        /// <param name="scriptText">The source text to compile.</param>
        /// <returns>The compiled script block.</returns>
        public ScriptBlock NewScriptBlock(string scriptText)
            _commandRuntime?.ThrowIfStopping();
            ScriptBlock result = ScriptBlock.Create(_context, scriptText);
    #endregion Auxiliary
    /// <see cref="System.Management.Automation.Cmdlet"/>
    /// or its subclasses.
    /// Instead, derive your own subclasses and mark them with
    /// <see cref="System.Management.Automation.CmdletAttribute"/>,
    /// and when your assembly is included in a shell, the Engine will
    /// take care of instantiating your subclass.
        internal bool HasDynamicParameters
            get { return this is IDynamicParameters; }
        /// The name of the parameter set in effect.
        /// <value>the parameter set name</value>
                    return _ParameterSetName;
        /// Contains information about the identity of this cmdlet
        /// and how it was invoked.
        public new InvocationInfo MyInvocation
                    return base.MyInvocation;
        /// If the cmdlet declares paging support (via <see cref="CmdletCommonMetadataAttribute.SupportsPaging"/>),
        /// then <see cref="PagingParameters"/> property contains arguments of the paging parameters.
        /// Otherwise <see cref="PagingParameters"/> property is <see langword="null"/>.
        public PagingParameters PagingParameters
                    if (!this.CommandInfo.CommandMetadata.SupportsPaging)
                    if (_pagingParameters == null)
                        if (mshCommandRuntime != null)
                            _pagingParameters = mshCommandRuntime.PagingParameters ?? new PagingParameters(mshCommandRuntime);
                    return _pagingParameters;
        private PagingParameters _pagingParameters;
        #region InvokeCommand
        /// Provides access to utility routines for executing scripts
        /// and creating script blocks.
        /// <value>Returns an object exposing the utility routines.</value>
                    return _invokeCommand ??= new CommandInvocationIntrinsics(Context, this);
        #endregion InvokeCommand

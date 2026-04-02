    /// The parameter binder for runtime-defined parameters which are declared through the RuntimeDefinedParameterDictionary.
    internal class RuntimeDefinedParameterBinder : ParameterBinderBase
        /// for a single instance of a bindable runtime-defined parameter collection and only for the duration of a command.
        /// The target runtime-defined parameter collection that the parameter values will be bound to.
        /// An instance of the command so that attributes can access the context.
        /// <param name="commandLineParameters">
        /// The Command line parameter collection to update...
        internal RuntimeDefinedParameterBinder(
            RuntimeDefinedParameterDictionary target,
            InternalCommand command,
            CommandLineParameters commandLineParameters)
            : base(target, command.MyInvocation, command.Context, command)
            // NTRAID#Windows Out Of Band Releases-927103-2006/01/25-JonN
            foreach (var pair in target)
                string key = pair.Key;
                RuntimeDefinedParameter pp = pair.Value;
                string ppName = pp?.Name;
                if (pp == null || key != ppName)
                            command.MyInvocation,
                            ppName,
                            ParameterBinderStrings.RuntimeDefinedParameterNameMismatch,
                            "RuntimeDefinedParameterNameMismatch",
                            key);
            this.CommandLineParameters = commandLineParameters;
        /// Hides the base class Target property to ensure the target
        /// is always a RuntimeDefinedParameterDictionary.
        internal new RuntimeDefinedParameterDictionary Target
                return base.Target as RuntimeDefinedParameterDictionary;
                base.Target = value;
        /// Gets the default value for the specified parameter.
        /// The name of the parameter to get the value for.
        /// The value of the specified parameter
            RuntimeDefinedParameter parameter;
            if (this.Target.TryGetValue(name, out parameter) && parameter != null)
                result = parameter.Value;
        /// Uses ETS to set the property specified by name to the value on
        /// the target bindable object.
            Target[name].Value = value;
            this.CommandLineParameters.Add(name, value);
    #region "AstArgumentPair"
    /// The types for AstParameterArgumentPair.
    internal enum AstParameterArgumentType
        AstPair = 0,
        Switch = 1,
        Fake = 2,
        AstArray = 3,
        PipeObject = 4
    /// The base class for parameter argument pair.
    internal abstract class AstParameterArgumentPair
        /// The parameter Ast.
        public CommandParameterAst Parameter { get; protected set; }
        /// The argument type.
        public AstParameterArgumentType ParameterArgumentType { get; protected set; }
        /// Indicate if the parameter is specified.
        public bool ParameterSpecified { get; protected set; } = false;
        public bool ArgumentSpecified { get; protected set; } = false;
        /// The parameter name.
        public string ParameterName { get; protected set; }
        /// The parameter text.
        public string ParameterText { get; protected set; }
        public Type ArgumentType { get; protected set; }
    /// Represent a parameter argument pair. The argument is a pipeline input object.
    internal sealed class PipeObjectPair : AstParameterArgumentPair
        internal PipeObjectPair(string parameterName, Type pipeObjType)
            if (parameterName == null)
                throw PSTraceSource.NewArgumentNullException(nameof(parameterName));
            Parameter = null;
            ParameterArgumentType = AstParameterArgumentType.PipeObject;
            ParameterSpecified = true;
            ArgumentSpecified = true;
            ParameterName = parameterName;
            ParameterText = parameterName;
            ArgumentType = pipeObjType;
    /// Represent a parameter argument pair. The argument is an array of ExpressionAst (remaining
    /// arguments)
    internal sealed class AstArrayPair : AstParameterArgumentPair
        internal AstArrayPair(string parameterName, ICollection<ExpressionAst> arguments)
            if (arguments == null || arguments.Count == 0)
            ParameterArgumentType = AstParameterArgumentType.AstArray;
            ArgumentType = typeof(Array);
            Argument = arguments.ToArray();
        /// Get the argument.
        public ExpressionAst[] Argument { get; } = null;
    /// Represent a parameter argument pair. The argument is a fake object.
    internal sealed class FakePair : AstParameterArgumentPair
        internal FakePair(CommandParameterAst parameterAst)
            if (parameterAst == null)
                throw PSTraceSource.NewArgumentNullException(nameof(parameterAst));
            Parameter = parameterAst;
            ParameterArgumentType = AstParameterArgumentType.Fake;
            ParameterName = parameterAst.ParameterName;
            ParameterText = parameterAst.ParameterName;
            ArgumentType = typeof(object);
    /// Represent a parameter argument pair. The parameter is a switch parameter.
    internal sealed class SwitchPair : AstParameterArgumentPair
        internal SwitchPair(CommandParameterAst parameterAst)
            ParameterArgumentType = AstParameterArgumentType.Switch;
            ArgumentType = typeof(bool);
        public bool Argument
            get { return true; }
    /// Represent a parameter argument pair. It could be a pure argument (no parameter, only argument available);
    /// it could be a CommandParameterAst that contains its argument; it also could be a CommandParameterAst with
    /// another CommandParameterAst as the argument.
    internal sealed class AstPair : AstParameterArgumentPair
        internal AstPair(CommandParameterAst parameterAst)
            if (parameterAst == null || parameterAst.Argument == null)
                throw PSTraceSource.NewArgumentException(nameof(parameterAst));
            ParameterArgumentType = AstParameterArgumentType.AstPair;
            ParameterText = "-" + ParameterName + ":";
            ArgumentType = parameterAst.Argument.StaticType;
            ParameterContainsArgument = true;
            Argument = parameterAst.Argument;
        internal AstPair(CommandParameterAst parameterAst, ExpressionAst argumentAst)
            if (parameterAst != null && parameterAst.Argument != null)
            if (parameterAst == null && argumentAst == null)
                throw PSTraceSource.NewArgumentNullException(nameof(argumentAst));
            ParameterSpecified = parameterAst != null;
            ArgumentSpecified = argumentAst != null;
            ParameterName = parameterAst?.ParameterName;
            ParameterText = parameterAst?.ParameterName;
            ArgumentType = argumentAst?.StaticType;
            ParameterContainsArgument = false;
            Argument = argumentAst;
        internal AstPair(CommandParameterAst parameterAst, CommandElementAst argumentAst)
            if (parameterAst == null || argumentAst == null)
            ArgumentType = typeof(string);
            ArgumentIsCommandParameterAst = true;
        /// Indicate if the argument is contained in the CommandParameterAst.
        public bool ParameterContainsArgument { get; } = false;
        /// Indicate if the argument is of type CommandParameterAst.
        public bool ArgumentIsCommandParameterAst { get; } = false;
        public CommandElementAst Argument { get; } = null;
    #endregion "AstArgumentPair"
    /// Runs the PowerShell parameter binding algorithm against a CommandAst,
    /// returning information about which parameters were bound.
    public static class StaticParameterBinder
        /// Bind a CommandAst to one of PowerShell's built-in commands.
        /// <param name="commandAst">The CommandAst that represents the command invocation.</param>
        /// <returns>The StaticBindingResult that represents the binding.</returns>
        public static StaticBindingResult BindCommand(CommandAst commandAst)
            return BindCommand(commandAst, resolve: true);
        /// Bind a CommandAst to the specified command.
        /// <param name="resolve">Boolean to determine whether binding should be syntactic, or should attempt
        /// to resolve against an existing command.
        public static StaticBindingResult BindCommand(CommandAst commandAst, bool resolve)
            return BindCommand(commandAst, resolve, null);
        /// <param name="desiredParameters">
        ///     A string array that represents parameter names of interest. If any of these are specified,
        ///     then full binding is done.
        public static StaticBindingResult BindCommand(CommandAst commandAst, bool resolve, string[] desiredParameters)
            // If they specified any desired parameters, first quickly check if they are found
            if ((desiredParameters != null) && (desiredParameters.Length > 0))
                bool possiblyHadDesiredParameter = false;
                foreach (CommandParameterAst commandParameter in commandAst.CommandElements.OfType<CommandParameterAst>())
                    string actualParameterName = commandParameter.ParameterName;
                    foreach (string actualParameter in desiredParameters)
                        if (actualParameter.StartsWith(actualParameterName, StringComparison.OrdinalIgnoreCase))
                            possiblyHadDesiredParameter = true;
                    if (possiblyHadDesiredParameter)
                // Quick exit if the desired parameter was not present
                if (!possiblyHadDesiredParameter)
            if (!resolve)
                return new StaticBindingResult(commandAst, null);
            PseudoBindingInfo pseudoBinding = null;
            if (Runspace.DefaultRunspace == null)
                // Handle static binding from a non-PowerShell / C# application
                // DefaultRunspace is a thread static field, so race condition will not happen because different threads will access different instances of "DefaultRunspace"
                if (t_bindCommandRunspace == null)
                    // Create a mini runspace by remove the types and formats
                    InitialSessionState minimalState = InitialSessionState.CreateDefault2();
                    minimalState.Types.Clear();
                    minimalState.Formats.Clear();
                    t_bindCommandRunspace = RunspaceFactory.CreateRunspace(minimalState);
                    t_bindCommandRunspace.Open();
                Runspace.DefaultRunspace = t_bindCommandRunspace;
                // Static binding always does argument binding (not argument or parameter completion).
                pseudoBinding = new PseudoParameterBinder().DoPseudoParameterBinding(commandAst, null, null, PseudoParameterBinder.BindingType.ArgumentBinding);
                Runspace.DefaultRunspace = null;
            return new StaticBindingResult(commandAst, pseudoBinding);
        private static Runspace t_bindCommandRunspace = null;
    /// Represents the results of the PowerShell parameter binding process.
    public class StaticBindingResult
        internal StaticBindingResult(CommandAst commandAst, PseudoBindingInfo bindingInfo)
            BoundParameters = new Dictionary<string, ParameterBindingResult>(StringComparer.OrdinalIgnoreCase);
            BindingExceptions = new Dictionary<string, StaticBindingError>(StringComparer.OrdinalIgnoreCase);
            if (bindingInfo == null)
                CreateBindingResultForSyntacticBind(commandAst);
                CreateBindingResultForSuccessfulBind(commandAst, bindingInfo);
        private void CreateBindingResultForSuccessfulBind(CommandAst commandAst, PseudoBindingInfo bindingInfo)
            _bindingInfo = bindingInfo;
            // Check if there is exactly one parameter set valid. In that case,
            // ValidParameterSetFlags is exactly a power of two. Otherwise,
            // add to the binding exceptions.
            bool parameterSetSpecified = bindingInfo.ValidParameterSetsFlags != UInt32.MaxValue;
            bool remainingParameterSetIncludesDefault =
                (bindingInfo.DefaultParameterSetFlag != 0) &&
                ((bindingInfo.ValidParameterSetsFlags & bindingInfo.DefaultParameterSetFlag) ==
                bindingInfo.DefaultParameterSetFlag);
            // (x & (x -1 ) == 0) is a bit hack to determine if something is
            // exactly a power of two.
            bool onlyOneRemainingParameterSet =
                (bindingInfo.ValidParameterSetsFlags != 0) &&
                (bindingInfo.ValidParameterSetsFlags &
                        (bindingInfo.ValidParameterSetsFlags - 1)) == 0;
            if (parameterSetSpecified &&
                (!remainingParameterSetIncludesDefault) &&
                (!onlyOneRemainingParameterSet))
                BindingExceptions.Add(commandAst.CommandElements[0].Extent.Text,
                    new StaticBindingError(commandAst.CommandElements[0], bindingException));
            // Add error for duplicate parameters
            if (bindingInfo.DuplicateParameters != null)
                foreach (AstParameterArgumentPair duplicateParameter in bindingInfo.DuplicateParameters)
                    AddDuplicateParameterBindingException(duplicateParameter.Parameter);
            // Add error for parameters not found
            if (bindingInfo.ParametersNotFound != null)
                foreach (CommandParameterAst parameterNotFound in bindingInfo.ParametersNotFound)
                            parameterNotFound.ErrorPosition,
                            parameterNotFound.ParameterName,
                    BindingExceptions.Add(parameterNotFound.ParameterName, new StaticBindingError(parameterNotFound, bindingException));
            // Add error for ambiguous parameters
            if (bindingInfo.AmbiguousParameters != null)
                foreach (CommandParameterAst ambiguousParameter in bindingInfo.AmbiguousParameters)
                    ParameterBindingException bindingException = bindingInfo.BindingExceptions[ambiguousParameter];
                    BindingExceptions.Add(ambiguousParameter.ParameterName, new StaticBindingError(ambiguousParameter, bindingException));
            // Add error for unbound positional parameters
            if (bindingInfo.UnboundArguments != null)
                foreach (AstParameterArgumentPair unboundArgument in bindingInfo.UnboundArguments)
                    AstPair argument = unboundArgument as AstPair;
                            argument.Argument.Extent,
                            argument.Argument.Extent.Text,
                    BindingExceptions.Add(argument.Argument.Extent.Text, new StaticBindingError(argument.Argument, bindingException));
            // Process the bound parameters
            if (bindingInfo.BoundParameters != null)
                foreach (KeyValuePair<string, MergedCompiledCommandParameter> item in bindingInfo.BoundParameters)
                    CompiledCommandParameter parameter = item.Value.Parameter;
                    CommandElementAst value = null;
                    object constantValue = null;
                    // This is a single argument
                    AstPair argumentAstPair = bindingInfo.BoundArguments[item.Key] as AstPair;
                    if (argumentAstPair != null)
                        value = argumentAstPair.Argument;
                    // This is a parameter that took an argument, as well as ValueFromRemainingArguments.
                    // Merge the arguments into a single fake argument.
                    AstArrayPair argumentAstArrayPair = bindingInfo.BoundArguments[item.Key] as AstArrayPair;
                    if (argumentAstArrayPair != null)
                        List<ExpressionAst> arguments = new List<ExpressionAst>();
                        foreach (ExpressionAst expression in argumentAstArrayPair.Argument)
                            ArrayLiteralAst expressionArray = expression as ArrayLiteralAst;
                            if (expressionArray != null)
                                foreach (ExpressionAst newExpression in expressionArray.Elements)
                                    arguments.Add((ExpressionAst)newExpression.Copy());
                                arguments.Add((ExpressionAst)expression.Copy());
                        // Define the virtual extent and virtual ArrayLiteral.
                        IScriptExtent fakeExtent = arguments[0].Extent;
                        ArrayLiteralAst fakeArguments = new ArrayLiteralAst(fakeExtent, arguments);
                        value = fakeArguments;
                    // Special handling of switch parameters
                    if (parameter.Type == typeof(SwitchParameter))
                        if ((value != null) &&
                            (string.Equals("$false", value.Extent.Text, StringComparison.OrdinalIgnoreCase)))
                        constantValue = true;
                    // We got a parameter and a value
                    if ((value != null) || (constantValue != null))
                        BoundParameters.Add(item.Key, new ParameterBindingResult(parameter, value, constantValue));
                        bool takesValueFromPipeline = false;
                        foreach (ParameterSetSpecificMetadata parameterSet in parameter.GetMatchingParameterSetData(bindingInfo.ValidParameterSetsFlags))
                            if (parameterSet.ValueFromPipeline)
                                takesValueFromPipeline = true;
                        if (!takesValueFromPipeline)
                            // We have a parameter with no value that isn't a switch parameter, or input parameter
                                    commandAst.CommandElements[0].Extent,
                                    parameter.Type,
        private void AddDuplicateParameterBindingException(CommandParameterAst duplicateParameter)
            if (duplicateParameter == null)
                    duplicateParameter.ErrorPosition,
                    duplicateParameter.ParameterName,
            // if the duplicated Parameter Name appears more than twice, we will ignore as we already have similar bindingException.
            if (!BindingExceptions.ContainsKey(duplicateParameter.ParameterName))
                BindingExceptions.Add(duplicateParameter.ParameterName, new StaticBindingError(duplicateParameter, bindingException));
        private PseudoBindingInfo _bindingInfo = null;
        private void CreateBindingResultForSyntacticBind(CommandAst commandAst)
            bool foundCommand = false;
            CommandParameterAst currentParameter = null;
            ParameterBindingResult bindingResult = new ParameterBindingResult();
            foreach (CommandElementAst commandElement in commandAst.CommandElements)
                // Skip the command name
                if (!foundCommand)
                    foundCommand = true;
                CommandParameterAst parameter = commandElement as CommandParameterAst;
                    if (currentParameter != null)
                        // Assume it was a switch
                        AddSwitch(currentParameter.ParameterName, bindingResult);
                        ResetCurrentParameter(ref currentParameter, ref bindingResult);
                    // If this is an actual parameter, get its name.
                    string parameterName = parameter.ParameterName;
                    bindingResult.Value = parameter;
                    // If it's a parameter with argument, add them both to the dictionary
                    if (parameter.Argument != null)
                        bindingResult.Value = parameter.Argument;
                        AddBoundParameter(parameter, parameterName, bindingResult);
                    // Otherwise, it's just a parameter and the argument is to follow.
                        // Store our current parameter
                        currentParameter = parameter;
                    // This isn't a parameter, it's a value for the previous parameter
                        bindingResult.Value = commandElement;
                        AddBoundParameter(currentParameter, currentParameter.ParameterName, bindingResult);
                        // Assume positional
                        AddBoundParameter(null, position.ToString(CultureInfo.InvariantCulture), bindingResult);
            // Catch any extra parameters at the end of the command
        private void AddBoundParameter(CommandParameterAst parameter, string parameterName, ParameterBindingResult bindingResult)
            if (BoundParameters.ContainsKey(parameterName))
                AddDuplicateParameterBindingException(parameter);
                BoundParameters.Add(parameterName, bindingResult);
        private static void ResetCurrentParameter(ref CommandParameterAst currentParameter, ref ParameterBindingResult bindingResult)
            currentParameter = null;
            bindingResult = new ParameterBindingResult();
        private void AddSwitch(string currentParameter, ParameterBindingResult bindingResult)
            bindingResult.ConstantValue = true;
            AddBoundParameter(null, currentParameter, bindingResult);
        public Dictionary<string, ParameterBindingResult> BoundParameters { get; }
        public Dictionary<string, StaticBindingError> BindingExceptions { get; }
    /// Represents the binding of a parameter to its argument.
    public class ParameterBindingResult
        internal ParameterBindingResult(CompiledCommandParameter parameter, CommandElementAst value, object constantValue)
            this.Parameter = new ParameterMetadata(parameter);
            this.Value = value;
            this.ConstantValue = constantValue;
        internal ParameterBindingResult()
        public ParameterMetadata Parameter { get; internal set; }
        public object ConstantValue
                return _constantValue;
                    _constantValue = value;
        private object _constantValue;
        public CommandElementAst Value
                ConstantExpressionAst constantValueAst = value as ConstantExpressionAst;
                if (constantValueAst != null)
                    this.ConstantValue = constantValueAst.Value;
        private CommandElementAst _value;
    /// Represents the exception generated by the static parameter binding process.
    public class StaticBindingError
        /// Creates a StaticBindingException.
        /// <param name="commandElement">The element associated with the exception.</param>
        /// <param name="exception">The parameter binding exception that got raised.</param>
        internal StaticBindingError(CommandElementAst commandElement, ParameterBindingException exception)
            this.CommandElement = commandElement;
            this.BindingException = exception;
        /// The command element associated with the exception.
        public CommandElementAst CommandElement { get; }
        /// The ParameterBindingException that this command element caused.
        public ParameterBindingException BindingException { get; }
    #region "PseudoBindingInfo"
    internal enum PseudoBindingInfoType
        PseudoBindingFail = 0,
        PseudoBindingSucceed = 1,
    internal sealed class PseudoBindingInfo
        /// The pseudo binding succeeded.
        /// <param name="validParameterSetsFlags"></param>
        /// <param name="defaultParameterSetFlag"></param>
        /// <param name="boundParameters"></param>
        /// <param name="unboundParameters"></param>
        /// <param name="boundArguments"></param>
        /// <param name="boundPositionalParameter"></param>
        /// <param name="allParsedArguments"></param>
        /// <param name="parametersNotFound"></param>
        /// <param name="ambiguousParameters"></param>
        /// <param name="bindingExceptions"></param>
        /// <param name="duplicateParameters"></param>
        /// <param name="unboundArguments"></param>
        internal PseudoBindingInfo(
            uint validParameterSetsFlags,
            List<MergedCompiledCommandParameter> unboundParameters,
            Collection<string> boundPositionalParameter,
            Collection<AstParameterArgumentPair> allParsedArguments,
            Collection<CommandParameterAst> parametersNotFound,
            Collection<CommandParameterAst> ambiguousParameters,
            Dictionary<CommandParameterAst, ParameterBindingException> bindingExceptions,
            Collection<AstParameterArgumentPair> duplicateParameters,
            Collection<AstParameterArgumentPair> unboundArguments)
            InfoType = PseudoBindingInfoType.PseudoBindingSucceed;
            ValidParameterSetsFlags = validParameterSetsFlags;
            DefaultParameterSetFlag = defaultParameterSetFlag;
            BoundParameters = boundParameters;
            UnboundParameters = unboundParameters;
            BoundArguments = boundArguments;
            BoundPositionalParameter = boundPositionalParameter;
            AllParsedArguments = allParsedArguments;
            ParametersNotFound = parametersNotFound;
            AmbiguousParameters = ambiguousParameters;
            BindingExceptions = bindingExceptions;
            DuplicateParameters = duplicateParameters;
            UnboundArguments = unboundArguments;
        /// The pseudo binding failed with parameter set confliction.
            List<MergedCompiledCommandParameter> unboundParameters)
            InfoType = PseudoBindingInfoType.PseudoBindingFail;
        internal string CommandName
            get { return CommandInfo.Name; }
        internal CommandInfo CommandInfo { get; }
        internal PseudoBindingInfoType InfoType { get; }
        internal uint ValidParameterSetsFlags { get; }
        internal uint DefaultParameterSetFlag { get; }
        internal Dictionary<string, MergedCompiledCommandParameter> BoundParameters { get; }
        internal List<MergedCompiledCommandParameter> UnboundParameters { get; }
        internal Dictionary<string, AstParameterArgumentPair> BoundArguments { get; }
        internal Collection<AstParameterArgumentPair> UnboundArguments { get; }
        internal Collection<string> BoundPositionalParameter { get; }
        internal Collection<AstParameterArgumentPair> AllParsedArguments { get; }
        internal Collection<CommandParameterAst> ParametersNotFound { get; }
        internal Collection<CommandParameterAst> AmbiguousParameters { get; }
        internal Dictionary<CommandParameterAst, ParameterBindingException> BindingExceptions { get; }
        internal Collection<AstParameterArgumentPair> DuplicateParameters { get; }
    #endregion "PseudoBindingInfo"
    internal class PseudoParameterBinder
        /// Get the parameter binding metadata.
        /// <param name="possibleParameterSets"></param>
        public Dictionary<ParameterMetadata, ExpressionAst> GetPseudoParameterBinding(out Collection<ParameterSetMetadata> possibleParameterSets)
            ExecutionContext contextFromTls =
                System.Management.Automation.Runspaces.LocalPipeline.GetExecutionContextFromTLS();
            return GetPseudoParameterBinding(out possibleParameterSets, contextFromTls, null);
        internal enum BindingType
            /// Caller is binding a parameter argument.
            ArgumentBinding = 0,
            /// Caller is performing completion on a parameter argument.
            ArgumentCompletion,
            /// Caller is performing completion on a parameter name.
            ParameterCompletion
        /// <param name="pipeArgumentType">Indicate the type of the piped-in argument.</param>
        /// <param name="paramAstAtCursor">The CommandParameterAst the cursor is pointing at.</param>
        /// <param name="bindingType">Indicates whether pseudo binding is for argument binding, argument completion, or parameter completion.</param>
        /// <param name="bindPositional">Indicates if the pseudo binding should bind positional parameters</param>
        /// <returns>PseudoBindingInfo.</returns>
        internal PseudoBindingInfo DoPseudoParameterBinding(CommandAst command, Type pipeArgumentType, CommandParameterAst paramAstAtCursor, BindingType bindingType, bool bindPositional = true)
            // initialize/reset the private members
            InitializeMembers();
            _commandAst = command;
            _commandElements = command.CommandElements;
            Collection<AstParameterArgumentPair> unboundArguments = new Collection<AstParameterArgumentPair>();
            // analyze the command and reparse the arguments
                ExecutionContext executionContext = LocalPipeline.GetExecutionContextFromTLS();
                if (executionContext != null)
                    // WinBlue: 324316. This limits the interaction of pseudoparameterbinder with the actual host.
                    SetTemporaryDefaultHost(executionContext);
                        if (executionContext.HasRunspaceEverUsedConstrainedLanguageMode)
                            previousLanguageMode = executionContext.LanguageMode;
                            executionContext.LanguageMode = PSLanguageMode.ConstrainedLanguage;
                        _bindingEffective = PrepareCommandElements(executionContext, paramAstAtCursor);
                            executionContext.LanguageMode = previousLanguageMode.Value;
                        RestoreHost(executionContext);
            if (_bindingEffective && (_isPipelineInputExpected || pipeArgumentType != null))
                _pipelineInputType = pipeArgumentType;
            _bindingEffective = ParseParameterArguments(paramAstAtCursor);
            if (_bindingEffective)
                // named binding
                unboundArguments = BindNamedParameters();
                _bindingEffective = _currentParameterSetFlag != 0;
                if (bindPositional)
                    // positional binding
                    unboundArguments = BindPositionalParameter(
                        unboundArguments,
                        _defaultParameterSetFlag,
                        bindingType);
                // VFRA/pipeline binding if the given command is a binary cmdlet or a script cmdlet
                if (!_function)
                    unboundArguments = BindRemainingParameters(unboundArguments);
                    BindPipelineParameters();
                // Update available parameter sets based on bound arguments
                bool parameterSetSpecified = (_currentParameterSetFlag != 0) &&
                    (_currentParameterSetFlag != UInt32.MaxValue);
                bool onlyOneRemainingParameterSet = (_currentParameterSetFlag != 0) &&
                    (_currentParameterSetFlag & (_currentParameterSetFlag - 1)) == 0;
                if ((bindingType != BindingType.ParameterCompletion) && parameterSetSpecified && (!onlyOneRemainingParameterSet))
                    CmdletParameterBinderController.ResolveParameterSetAmbiguityBasedOnMandatoryParameters(
                        _boundParameters,
                        _unboundParameters,
                        ref _currentParameterSetFlag,
            // Binding failed
            if (!_bindingEffective)
                // The command is not a cmdlet, not a script cmdlet, and not a function
                if (_bindableParameters == null)
                // get all bindable parameters
                _unboundParameters.Clear();
                _unboundParameters.AddRange(_bindableParameters.BindableParameters.Values);
                return new PseudoBindingInfo(
                    _commandInfo,
                    _arguments,
                    _unboundParameters);
                _boundArguments,
                _boundPositionalParameter,
                _parametersNotFound,
                _ambiguousParameters,
                _bindingExceptions,
                _duplicateParameters,
                unboundArguments
        /// Sets a temporary default host on the ExecutionContext.
        /// <param name="executionContext">ExecutionContext.</param>
        private void SetTemporaryDefaultHost(ExecutionContext executionContext)
            if (executionContext.EngineHostInterface.IsHostRefSet)
                // A temporary host is already set so we need to track and restore here, because
                // setting the host again will overwrite the current one.
                _restoreHost = executionContext.EngineHostInterface.ExternalHost;
                // Revert host back to its original state.
                executionContext.EngineHostInterface.RevertHostRef();
            // Temporarily set host to default.
            executionContext.EngineHostInterface.SetHostRef(new Microsoft.PowerShell.DefaultHost(
                CultureInfo.CurrentUICulture));
        /// Restores original ExecutionContext host state.
        private void RestoreHost(ExecutionContext executionContext)
            // Remove temporary host and revert to original.
            // Re-apply saved host if any.
            if (_restoreHost != null)
                executionContext.EngineHostInterface.SetHostRef(_restoreHost);
                _restoreHost = null;
        // Host to restore.
        private PSHost _restoreHost;
        // command ast related states
        private CommandAst _commandAst;
        private ReadOnlyCollection<CommandElementAst> _commandElements;
        // binding related states
        private bool _function = false;
        private CommandInfo _commandInfo = null;
        private uint _currentParameterSetFlag = uint.MaxValue;
        private uint _defaultParameterSetFlag = 0;
        private MergedCommandParameterMetadata _bindableParameters = null;
        private Dictionary<string, MergedCompiledCommandParameter> _boundParameters;
        private Dictionary<string, AstParameterArgumentPair> _boundArguments;
        private Collection<AstParameterArgumentPair> _arguments;
        private Collection<string> _boundPositionalParameter;
        private List<MergedCompiledCommandParameter> _unboundParameters;
        // tab expansion related states
        private Type _pipelineInputType = null;
        private bool _bindingEffective = true;
        private bool _isPipelineInputExpected = false;
        private Collection<CommandParameterAst> _parametersNotFound;
        private Collection<CommandParameterAst> _ambiguousParameters;
        private Collection<AstParameterArgumentPair> _duplicateParameters;
        private Dictionary<CommandParameterAst, ParameterBindingException> _bindingExceptions;
        /// Initialize collection/dictionary members when it's necessary.
        private void InitializeMembers()
            // Initializing binding related members
            _function = false;
            _commandName = null;
            _currentParameterSetFlag = uint.MaxValue;
            _defaultParameterSetFlag = 0;
            _bindableParameters = null;
            // reuse the collections/dictionaries
            _arguments ??= new Collection<AstParameterArgumentPair>();
            _boundParameters ??= new Dictionary<string, MergedCompiledCommandParameter>(StringComparer.OrdinalIgnoreCase);
            _boundArguments ??= new Dictionary<string, AstParameterArgumentPair>(StringComparer.OrdinalIgnoreCase);
            _unboundParameters ??= new List<MergedCompiledCommandParameter>();
            _boundPositionalParameter ??= new Collection<string>();
            _bindingExceptions ??= new Dictionary<CommandParameterAst, ParameterBindingException>();
            _arguments.Clear();
            _boundParameters.Clear();
            _boundArguments.Clear();
            _boundPositionalParameter.Clear();
            _bindingExceptions.Clear();
            // Initializing tab expansion related members
            _pipelineInputType = null;
            _bindingEffective = true;
            _isPipelineInputExpected = false;
            // reuse the collections
            _parametersNotFound ??= new Collection<CommandParameterAst>();
            _ambiguousParameters ??= new Collection<CommandParameterAst>();
            _duplicateParameters ??= new Collection<AstParameterArgumentPair>();
            _parametersNotFound.Clear();
            _ambiguousParameters.Clear();
            _duplicateParameters.Clear();
        private bool PrepareCommandElements(ExecutionContext context, CommandParameterAst paramAtCursor)
            int commandIndex = 0;
            bool dotSource = _commandAst.InvocationOperator == TokenKind.Dot;
            string commandName = null;
                processor = PrepareFromAst(context, out commandName) ?? context.CreateCommand(commandName, dotSource, forCompletion:true);
                // Failed to create the CommandProcessor;
            var commandProcessor = processor as CommandProcessor;
            var scriptProcessor = processor as ScriptCommandProcessorBase;
            bool implementsDynamicParameters = commandProcessor != null &&
                                               commandProcessor.CommandInfo.ImplementsDynamicParameters;
            if (commandProcessor != null || scriptProcessor != null)
                // Pre-processing the arguments -- command arguments
                for (commandIndex++; commandIndex < _commandElements.Count; commandIndex++)
                    if (implementsDynamicParameters && _commandElements[commandIndex] == paramAtCursor)
                        // Commands with dynamic parameters will try to bind the command elements.
                        // A partially complete parameter will most likely cause a binding error and negatively affect the results.
                    var parameter = _commandElements[commandIndex] as CommandParameterAst;
                        if (implementsDynamicParameters)
                            CommandParameterInternal paramToAdd;
                            if (parameter.Argument is null)
                                paramToAdd = CommandParameterInternal.CreateParameter(parameter.ParameterName, parameter.Extent.Text);
                                if (!SafeExprEvaluator.TrySafeEval(parameter.Argument, context, out value))
                                    value = parameter.Argument.Extent.Text;
                                paramToAdd = CommandParameterInternal.CreateParameterWithArgument(
                                    parameterAst: null,
                                    parameterName: parameter.ParameterName,
                                    parameterText: parameter.Extent.Text,
                                    argumentAst: null,
                                    value: value,
                                    spaceAfterParameter: false);
                            commandProcessor.AddParameter(paramToAdd);
                        AstPair parameterArg = parameter.Argument != null
                            ? new AstPair(parameter)
                            : new AstPair(parameter, (ExpressionAst)null);
                        _arguments.Add(parameterArg);
                        object valueToAdd;
                        ExpressionAst expressionToAdd;
                        if (_commandElements[commandIndex] is ConstantExpressionAst constant)
                            if (constant.Extent.Text.Equals("-", StringComparison.Ordinal))
                                // A value of "-" is most likely the user trying to tab here,
                                // and we don't want it be treated as an argument
                            valueToAdd = constant.Value;
                            expressionToAdd = constant;
                        else if (_commandElements[commandIndex] is ExpressionAst expression)
                            if (!SafeExprEvaluator.TrySafeEval(expression, context, out valueToAdd))
                                valueToAdd = expression.Extent.Text;
                            expressionToAdd = expression;
                            commandProcessor.AddParameter(CommandParameterInternal.CreateArgument(valueToAdd));
                        _arguments.Add(new AstPair(null, expressionToAdd));
            if (commandProcessor != null)
                    bool retryWithNoArgs = false, alreadyRetried = false;
                        CommandProcessorBase oldCurrentCommandProcessor = context.CurrentCommandProcessor;
                            context.CurrentCommandProcessor = commandProcessor;
                            commandProcessor.SetCurrentScopeToExecutionScope();
                            // Run method "BindCommandLineParametersNoValidation" to get all available parameters, including the dynamic
                            // parameters (some of them, not necessarily all. Since we don't do the actual binding, some dynamic parameters
                            // might not be retrieved).
                            if (!retryWithNoArgs)
                                // Win8 345299: First try with all unbounded arguments
                                commandProcessor.CmdletParameterBinderController.BindCommandLineParametersNoValidation(commandProcessor.arguments);
                                // Win8 345299: If the first try ended with ParameterBindingException, try again with no arguments
                                alreadyRetried = true;
                                commandProcessor.CmdletParameterBinderController.ClearUnboundArguments();
                                commandProcessor.CmdletParameterBinderController.BindCommandLineParametersNoValidation(new Collection<CommandParameterInternal>());
                            // Catch the parameter binding exception thrown when Reparsing the argument.
                            //   "MissingArgument" - a single parameter is matched, but no argument is present
                            //   "AmbiguousParameter" - multiple parameters are matched
                            // When such exceptions are caught, retry again without arguments, so as to get dynamic parameters
                            // based on the current provider
                            if (e.ErrorId == "MissingArgument" || e.ErrorId == "AmbiguousParameter")
                                retryWithNoArgs = true;
                            context.CurrentCommandProcessor = oldCurrentCommandProcessor;
                            commandProcessor.RestorePreviousScope();
                    } while (retryWithNoArgs && !alreadyRetried);
                // Get all bindable parameters and initialize the _unboundParameters
                _commandInfo = commandProcessor.CommandInfo;
                _commandName = commandProcessor.CommandInfo.Name;
                _bindableParameters = commandProcessor.CmdletParameterBinderController.BindableParameters;
                _defaultParameterSetFlag = commandProcessor.CommandInfo.CommandMetadata.DefaultParameterSetFlag;
            else if (scriptProcessor != null)
                _function = true;
                _commandInfo = scriptProcessor.CommandInfo;
                _commandName = scriptProcessor.CommandInfo.Name;
                _bindableParameters = scriptProcessor.ScriptParameterBinderController.BindableParameters;
                // The command is not a function, cmdlet and script cmdlet
            // Pre-processing the arguments -- pipeline input
            // Check if there is pipeline input
            CommandBaseAst preCmdBaseAst = null;
            var pipe = _commandAst.Parent as PipelineAst;
            Diagnostics.Assert(pipe != null, "CommandAst should has a PipelineAst parent");
            if (pipe.PipelineElements.Count > 1)
                foreach (CommandBaseAst cmdBase in pipe.PipelineElements)
                    if (cmdBase.GetHashCode() == _commandAst.GetHashCode())
                        _isPipelineInputExpected = preCmdBaseAst != null;
                        if (_isPipelineInputExpected)
                            _pipelineInputType = typeof(object);
                    preCmdBaseAst = cmdBase;
        private CommandProcessorBase PrepareFromAst(ExecutionContext context, out string resolvedCommandName)
            // Analyze the Ast
            var exportVisitor = new ExportVisitor(forCompletion: true);
            Ast ast = _commandAst;
            while (ast.Parent != null)
                ast = ast.Parent;
            ast.Visit(exportVisitor);
            CommandProcessorBase commandProcessor = null;
            resolvedCommandName = _commandAst.GetCommandName();
                string alias;
                int resolvedAliasCount = 0;
                while (exportVisitor.DiscoveredAliases.TryGetValue(resolvedCommandName, out alias))
                    resolvedAliasCount += 1;
                    if (resolvedAliasCount > 5)
                        break;  // give up, assume it's recursive
                    resolvedCommandName = alias;
                FunctionDefinitionAst functionDefinitionAst;
                if (exportVisitor.DiscoveredFunctions.TryGetValue(resolvedCommandName, out functionDefinitionAst))
                    var scriptBlock = new ScriptBlock(functionDefinitionAst, functionDefinitionAst.IsFilter);
                    commandProcessor = CommandDiscovery.CreateCommandProcessorForScript(scriptBlock, context, true, context.EngineSessionState);
        /// Parse the arguments to process switch parameters and parameters without a value
        /// specified. We always eat the error (such as parameter without value) and continue
        /// to do the binding.
        /// <param name="paramAstAtCursor">
        /// For parameter completion, if the cursor is pointing at a CommandParameterAst, we
        /// should not try exact matching for that CommandParameterAst. This is to handle the
        /// following case:
        ///     Add-Computer -domain(tab)
        /// Add-Computer has an alias "Domain" that can exactly match this partial input, but
        /// since the user is typing 'tab', the partial input 'domain' should not be considered
        /// as an exact match. In this case, we don't try exact matching when calling
        /// GetMatchingParameter(..) so as to preserve other possibilities.
        private bool ParseParameterArguments(CommandParameterAst paramAstAtCursor)
                return _bindingEffective;
            var result = new Collection<AstParameterArgumentPair>();
            for (int index = 0; index < _arguments.Count; index++)
                AstParameterArgumentPair argument = _arguments[index];
                if (!argument.ParameterSpecified || argument.ArgumentSpecified)
                    // Add the positional/named arguments back
                Diagnostics.Assert(argument.ParameterSpecified && !argument.ArgumentSpecified,
                    "At this point, the parameters should have no arguments");
                // Now check the parameter name with the bindable parameters
                MergedCompiledCommandParameter matchingParameter = null;
                    bool tryExactMatching = argument.Parameter != paramAstAtCursor;
                    matchingParameter = _bindableParameters.GetMatchingParameter(parameterName, false, tryExactMatching, null);
                    // The parameterName is resolved to multiple parameters. The most possible scenario for this
                    // would be the user typing tab to complete a parameter. In this case, we can ignore this
                    // parameter safely.
                    // If the next item is a pure argument, we skip it so that it doesn't get bound
                    // positionally.
                    if (index < _arguments.Count - 1)
                        AstParameterArgumentPair nextArg = _arguments[index + 1];
                        if (!nextArg.ParameterSpecified && nextArg.ArgumentSpecified)
                    _ambiguousParameters.Add(argument.Parameter);
                    _bindingExceptions[argument.Parameter] = e;
                    // The parameter cannot be found. The reason could be:
                    // 1. It's a bynamic parameter, and we cannot retrieve the ParameterMetadata for it
                    //    at this point, since it's pseudo binding.
                    // 2. The spelling of this parameter is wrong.
                    // We can simply ignore this parameter, but the issue is what to do with the argument
                    // following this parameter (if there is an argument following it). There are two cases:
                    // 1. This parameter is supposed to be a switch parameter. Then the argument following it
                    //    should NOT be ignored.
                    // 2. This parameter is supposed to take an argument. Then the following argument should
                    //    also be ignored
                    // We check the next item. If it's a pure argument, we give up the binding, because we don't
                    // know how to deal with it (ignore it? keep it?), and it will affect the accuracy of our
                    // parameter set resolution.
                        // If the next item is a pure argument, we give up the pseudo binding.
                            // Testing paramsAstAtCursor ensures we only give up during tab completion,
                            // otherwise we know this is a missing parameter.
                            if (paramAstAtCursor != null)
                                // Do not use the parsed arguments
                                _arguments = null;
                                // Otherwise, skip the next argument
                                _parametersNotFound.Add(argument.Parameter);
                    // If the next item is not a pure argument, or the current parameter is the last item,
                    // ignore this parameter and carry on with the binding
                // Check if it's SwitchParameter
                if (matchingParameter.Parameter.Type == typeof(SwitchParameter))
                    SwitchPair newArg = new SwitchPair(argument.Parameter);
                    result.Add(newArg);
                // It's not a switch parameter, we need to check the next argument
                    if (nextArg.ParameterSpecified)
                                _bindableParameters.GetMatchingParameter(nextArg.ParameterName, false, true, null);
                            // The next parameter doesn't exist. We use it as an argument
                            if (nextMatchingParameter == null)
                                AstPair newArg = new AstPair(argument.Parameter, nextArg.Parameter);
                                // It's possible the user is typing tab for argument completion.
                                // We set a fake argument for the current parameter in this case.
                                FakePair newArg = new FakePair(argument.Parameter);
                            // The next parameter name is ambiguous. We just set
                            // a fake argument for the current parameter.
                        // The next item is a pure argument.
                        AstPair nextArgument = nextArg as AstPair;
                        Diagnostics.Assert(nextArgument != null, "the next item should be a pure argument here");
                        Diagnostics.Assert(nextArgument.ArgumentSpecified && !nextArgument.ArgumentIsCommandParameterAst, "the next item should be a pure argument here");
                        AstPair newArg = new AstPair(argument.Parameter, (ExpressionAst)nextArgument.Argument);
                    // The current parameter is the last item. Set a fake argument for it
            _arguments = result;
        private Collection<AstParameterArgumentPair> BindNamedParameters()
            Collection<AstParameterArgumentPair> result = new Collection<AstParameterArgumentPair>();
            foreach (AstParameterArgumentPair argument in _arguments)
                if (!argument.ParameterSpecified)
                MergedCompiledCommandParameter parameter = null;
                    parameter = _bindableParameters.GetMatchingParameter(argument.ParameterName, false, true, null);
                    // The parameter name is ambiguous. It's not processed in ParseParameterArguments. Otherwise we
                    // should detect it early. So this argument comes from a CommandParameterAst with argument. We
                    // ignore it and carry on with our binding
                    // Cannot find a matching parameter. It's not processed in ParseParameterArguments. It comes from
                    // a CommandParameterAst with argument. We ignore it and carry on with our binding
                if (_boundParameters.ContainsKey(parameter.Parameter.Name))
                    // This parameter is already bound. We ignore it and carry on with the binding.
                    _duplicateParameters.Add(argument);
                // The parameter exists and is not bound yet. We assume the binding will always succeed.
                _unboundParameters.Remove(parameter);
                if (!_boundParameters.ContainsKey(parameter.Parameter.Name))
                    _boundParameters.Add(parameter.Parameter.Name, parameter);
                if (!_boundArguments.ContainsKey(parameter.Parameter.Name))
                    _boundArguments.Add(parameter.Parameter.Name, argument);
        private Collection<AstParameterArgumentPair> BindPositionalParameter(
            Collection<AstParameterArgumentPair> unboundArguments,
            BindingType bindingType)
            if (_bindingEffective && unboundArguments.Count > 0)
                List<AstParameterArgumentPair> unboundArgumentsCollection = new List<AstParameterArgumentPair>(unboundArguments);
                // Get the unbound positional parameters
                        ParameterBinderController.EvaluateUnboundPositionalParameters(_unboundParameters, validParameterSetFlags);
                    // from the same parameter set with the same position defined. The parameter definition
                    // is ambiguous. We give up binding in this case
                    _bindingEffective = false;
                // No positional parameter available
                if (positionalParameterDictionary.Count == 0)
                    return unboundArguments;
                    AstParameterArgumentPair argument = GetNextPositionalArgument(
                    // The positional pseudo binding is processed in two different approaches for parameter completion and parameter argument completion.
                    // - For parameter completion, we do NOT honor the default parameter set, so we can preserve potential parameters as many as possible.
                    //   Example:
                    //           Where-Object PropertyA -<tab>
                    //   If the default parameter is honored, the completion results only contain EQ, because it's locked to the default set
                    // - For parameter argument completion, however, we want to honor the default parameter set some times, especially when the argument
                    //   can be bound to the positional parameter from the default set WITHOUT type coercion.
                    //           Set-Location c:\win<tab>
                    //   In this scenario, the user actually intends to use -Path implicitly, and we should not preserve the -LiteralPath. But if we fail
                    //   on the attempt with the (default set + no coercion), we should fall back to the (all valid set + with coercion) to preserve possibilities.
                    //           Add-Member notep<tab>
                    //   We need presever the -MemberType along with the -NotePropertyName in this case.
                    // So the algorithm for positional binding is:
                    // - With bindingType == ParameterCompletion
                    //   Skip the attempt with the default set, as well as the attempt with all sets but no coercion.
                    //   Do the positional binding with the (all valid set + with coercion) directly.
                    // - With bindingType == ArgumentCompletion  (parameter argument completion)
                    //   First try to do positional binding with (default set + no coercion)
                    //   If the first attempt fails, do positional binding with (all valid set + with coercion)
                    // - With bindingType == ArgumentBinding (parameter argument binding, no completion)
                    //   If the first attempt fails, do positional binding with (all valid set + without coercion)
                    //   If the second attempt fails, do positional binding with (all valid set + with coercion)
                    bool aParameterGetBound = false;
                    if ((bindingType != BindingType.ParameterCompletion) && ((validParameterSetFlags & defaultParameterSetFlag) != 0))
                        // Default set, no coercion.
                        aParameterGetBound =
                            BindPseudoPositionalParameterInSet(
                                defaultParameterSetFlag,
                    if (!aParameterGetBound && (bindingType == BindingType.ArgumentBinding))
                        // All valid sets, no coercion.
                                validParameterSetFlags,
                    if (!aParameterGetBound)
                        // All valid sets, with coercion.
                        if (validParameterSetFlags != _currentParameterSetFlag)
                            validParameterSetFlags = _currentParameterSetFlag;
                            ParameterBinderController.UpdatePositionalDictionary(positionalParameterDictionary, validParameterSetFlags);
        private bool BindPseudoPositionalParameterInSet(
            uint validParameterSetFlag,
            AstParameterArgumentPair argument,
            bool typeConversion)
            bool bindingSuccessful = false;
            uint localParameterSetFlag = 0;
                    // Skip it if it's not in the specified parameter set
                    if ((validParameterSetFlag & parameterSetData.ParameterSetFlag) == 0 &&
                    Type parameterType = parameter.Parameter.Parameter.Type;
                    Type argumentType = argument.ArgumentType;
                    // 1. the argument type is not known(typeof(object)). we assume the binding always succeeds
                    // 2. the argument type is the same as parameter type, we assume the binding succeeds
                    // 3. the types are not the same, but we allow conversion, we assume the binding succeeds
                    // 4. the types are not the same, and conversion is not allowed, we assume the binding fails
                    if (argumentType == typeof(object))
                        bindingSuccessful = result = true;
                    else if (IsTypeEquivalent(argumentType, parameterType))
                    else if (typeConversion)
                        localParameterSetFlag |= parameter.Parameter.Parameter.ParameterSetFlags;
                        _unboundParameters.Remove(parameter.Parameter);
                        if (!_boundParameters.ContainsKey(parameterName))
                            _boundParameters.Add(parameterName, parameter.Parameter);
                            _boundPositionalParameter.Add(parameterName);
                        if (!_boundArguments.ContainsKey(parameterName))
                            _boundArguments.Add(parameterName, argument);
            // We preserve all possibilities
            if (bindingSuccessful && localParameterSetFlag != 0)
                _currentParameterSetFlag &= localParameterSetFlag;
            return bindingSuccessful;
        private static bool IsTypeEquivalent(Type argType, Type paramType)
            if (argType == paramType)
            else if (argType.IsSubclassOf(paramType))
            else if (argType == paramType.GetElementType())
            else if (argType.IsSubclassOf(typeof(Array)) && paramType.IsSubclassOf(typeof(Array)))
        private static AstParameterArgumentPair GetNextPositionalArgument(
            List<AstParameterArgumentPair> unboundArgumentsCollection,
            Collection<AstParameterArgumentPair> nonPositionalArguments,
            // Find the next positional parameter. An argument without the parameter being
            // specified is considered to be a positional argument
            AstParameterArgumentPair result = null;
                AstParameterArgumentPair argument = unboundArgumentsCollection[unboundArgumentsIndex++];
                if (argument is AstPair astPair
                    && astPair.Argument is VariableExpressionAst argumentVariable
                    && argumentVariable.Splatted)
        private Collection<AstParameterArgumentPair> BindRemainingParameters(Collection<AstParameterArgumentPair> unboundArguments)
            if (!_bindingEffective || unboundArguments.Count == 0)
            Collection<ExpressionAst> argList = new Collection<ExpressionAst>();
            foreach (AstParameterArgumentPair arg in unboundArguments)
                AstPair realArg = arg as AstPair;
                Diagnostics.Assert(realArg != null && !realArg.ParameterSpecified && !realArg.ArgumentIsCommandParameterAst,
                    "all unbound arguments left should be pure ExpressionAst arguments");
                argList.Add((ExpressionAst)realArg.Argument);
            var unboundParametersCopy = new List<MergedCompiledCommandParameter>(_unboundParameters);
            foreach (MergedCompiledCommandParameter unboundParam in unboundParametersCopy)
                bool isInParameterSet = (unboundParam.Parameter.ParameterSetFlags & _currentParameterSetFlag) != 0 ||
                                        unboundParam.Parameter.IsInAllSets;
                var parameterSetDataCollection = unboundParam.Parameter.GetMatchingParameterSetData(_currentParameterSetFlag);
                    if (!parameterSetData.ValueFromRemainingArguments)
                    localParameterSetFlag |= unboundParam.Parameter.ParameterSetFlags;
                    string parameterName = unboundParam.Parameter.Name;
                    _unboundParameters.Remove(unboundParam);
                        _boundParameters.Add(parameterName, unboundParam);
                        _boundArguments.Add(parameterName, new AstArrayPair(parameterName, argList));
                        unboundArguments.Clear();
            if (result && localParameterSetFlag != 0)
        private void BindPipelineParameters()
            if (!_bindingEffective || !_isPipelineInputExpected)
                if (!unboundParam.Parameter.IsPipelineParameterInSomeParameterSet)
                    // We don't assume the 'ValueFromPipelineByPropertyName' parameters get bound
                    if (!parameterSetData.ValueFromPipeline)
                        _boundArguments.Add(parameterName, new PipeObjectPair(parameterName, _pipelineInputType));

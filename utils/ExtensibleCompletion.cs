    /// This attribute is used to specify an argument completer for a parameter to a cmdlet or function.
    ///     [Parameter()]
    ///     [ArgumentCompleter(typeof(NounArgumentCompleter))]
    ///     public string Noun { get; set; }
    public class ArgumentCompleterAttribute : Attribute
        public Type Type { get; }
        /// <param name="type">The type must implement <see cref="IArgumentCompleter"/> and have a default constructor.</param>
        public ArgumentCompleterAttribute(Type type)
            if (type == null || (type.GetInterfaces().All(static t => t != typeof(IArgumentCompleter))))
                throw PSTraceSource.NewArgumentException(nameof(type));
        /// Initializes a new instance of the <see cref="ArgumentCompleterAttribute"/> class.
        /// This constructor is used by derived attributes implementing <see cref="IArgumentCompleterFactory"/>.
        protected ArgumentCompleterAttribute()
            if (this is not IArgumentCompleterFactory)
        /// This constructor is used primarily via PowerShell scripts.
        /// <param name="scriptBlock"></param>
        public ArgumentCompleterAttribute(ScriptBlock scriptBlock)
            if (scriptBlock is null)
        internal IArgumentCompleter CreateArgumentCompleter()
            return Type != null
                ? Activator.CreateInstance(Type) as IArgumentCompleter
                : this is IArgumentCompleterFactory factory
                    ? factory.Create()
    /// A type specified by the <see cref="ArgumentCompleterAttribute"/> must implement this interface.
    public interface IArgumentCompleter
        /// Implementations of this function are called by PowerShell to complete arguments.
        /// <param name="commandName">The name of the command that needs argument completion.</param>
        /// <param name="parameterName">The name of the parameter that needs argument completion.</param>
        /// <param name="wordToComplete">The (possibly empty) word being completed.</param>
        /// <param name="commandAst">The command ast in case it is needed for completion.</param>
        /// <param name="fakeBoundParameters">
        /// This parameter is similar to $PSBoundParameters, except that sometimes PowerShell cannot or
        /// will not attempt to evaluate an argument, in which case you may need to use <paramref name="commandAst"/>.
        /// A collection of completion results, most like with <see cref="CompletionResult.ResultType"/> set to
        /// <see cref="CompletionResultType.ParameterValue"/>.
        IEnumerable<CompletionResult> CompleteArgument(
            IDictionary fakeBoundParameters);
    /// Creates a new argument completer.
    /// If an attribute that derives from <see cref="ArgumentCompleterAttribute"/> implements this interface,
    /// it will be used to create the <see cref="IArgumentCompleter"/>, thus giving a way to parameterize a completer.
    /// The derived attribute can have properties or constructor arguments that are used when creating the completer.
    /// This example shows the intended usage of <see cref="IArgumentCompleterFactory"/> to pass arguments to an argument completer.
    /// public class NumberCompleterAttribute : ArgumentCompleterAttribute, IArgumentCompleterFactory {
    ///    private readonly int _from;
    ///    private readonly int _to;
    ///    public NumberCompleterAttribute(int from, int to){
    ///       _from = from;
    ///       _to = to;
    ///    }
    ///    // use the attribute parameters to create a parameterized completer
    ///    IArgumentCompleter Create() => new NumberCompleter(_from, _to);
    /// class NumberCompleter : IArgumentCompleter {
    ///    public NumberCompleter(int from, int to){
    ///    IEnumerable{CompletionResult} CompleteArgument(string commandName, string parameterName, string wordToComplete,
    ///       CommandAst commandAst, IDictionary fakeBoundParameters) {
    ///       for(int i = _from; i &lt; _to; i++) {
    ///           yield return new CompletionResult(i.ToString());
    ///       }
    public interface IArgumentCompleterFactory
        /// Creates an instance of a class implementing the <see cref="IArgumentCompleter"/> interface.
        /// <returns>An IArgumentCompleter instance.</returns>
        IArgumentCompleter Create();
    /// Base class for parameterized argument completer attributes.
    public abstract class ArgumentCompleterFactoryAttribute : ArgumentCompleterAttribute, IArgumentCompleterFactory
        /// <inheritdoc />
        public abstract IArgumentCompleter Create();
    [Cmdlet(VerbsLifecycle.Register, "ArgumentCompleter", HelpUri = "https://go.microsoft.com/fwlink/?LinkId=528576")]
    public class RegisterArgumentCompleterCommand : PSCmdlet
        private const string PowerShellSetName = "PowerShellSet";
        private const string NativeCommandSetName = "NativeCommandSet";
        private const string NativeFallbackSetName = "NativeFallbackSet";
        // Use a key that is unlikely to be a file name or path to indicate the fallback completer for native commands.
        internal const string FallbackCompleterKey = "___ps::<native_fallback_key>@@___";
        /// Gets or sets the command names for which the argument completer is registered.
        [Parameter(ParameterSetName = NativeCommandSetName, Mandatory = true)]
        [Parameter(ParameterSetName = PowerShellSetName)]
        public string[] CommandName { get; set; }
        /// Gets or sets the name of the parameter for which the argument completer is registered.
        [Parameter(ParameterSetName = PowerShellSetName, Mandatory = true)]
        public string ParameterName { get; set; }
        /// Gets or sets the script block that will be executed to provide argument completions.
        public ScriptBlock ScriptBlock { get; set; }
        /// Indicates the argument completer is for native commands.
        [Parameter(ParameterSetName = NativeCommandSetName)]
        public SwitchParameter Native { get; set; }
        /// Indicates the argument completer is a fallback for any native commands that don't have a completer registered.
        [Parameter(ParameterSetName = NativeFallbackSetName)]
        public SwitchParameter NativeFallback { get; set; }
            Dictionary<string, ScriptBlock> completerDictionary;
            if (ParameterSetName is NativeFallbackSetName)
                completerDictionary = Context.NativeArgumentCompleters ??= new(StringComparer.OrdinalIgnoreCase);
                SetKeyValue(completerDictionary, FallbackCompleterKey, ScriptBlock);
            else if (ParameterSetName is NativeCommandSetName)
                foreach (string command in CommandName)
                    var key = command?.Trim();
                    if (string.IsNullOrEmpty(key))
                    SetKeyValue(completerDictionary, key, ScriptBlock);
            else if (ParameterSetName is PowerShellSetName)
                completerDictionary = Context.CustomArgumentCompleters ??= new(StringComparer.OrdinalIgnoreCase);
                string paramName = ParameterName.Trim();
                if (paramName.Length is 0)
                if (CommandName is null || CommandName.Length is 0)
                    SetKeyValue(completerDictionary, paramName, ScriptBlock);
                    key = string.IsNullOrEmpty(key)
                        ? paramName
                        : $"{key}:{paramName}";
            static void SetKeyValue(Dictionary<string, ScriptBlock> table, string key, ScriptBlock value)
                    table.Remove(key);
                    table[key] = value;
    /// This attribute is used to specify an argument completions for a parameter of a cmdlet or function
    /// based on string array.
    ///     [ArgumentCompletions("Option1","Option2","Option3")]
    public class ArgumentCompletionsAttribute : Attribute
        private readonly string[] _completions;
        /// Initializes a new instance of the ArgumentCompletionsAttribute class.
        /// <param name="completions">List of complete values.</param>
        public ArgumentCompletionsAttribute(params string[] completions)
            if (completions == null)
                throw PSTraceSource.NewArgumentNullException(nameof(completions));
            if (completions.Length == 0)
                throw PSTraceSource.NewArgumentOutOfRangeException(nameof(completions), completions);
            _completions = completions;
        /// The function returns completions for arguments.
            var wordToCompletePattern = WildcardPattern.Get(string.IsNullOrWhiteSpace(wordToComplete) ? "*" : wordToComplete + "*", WildcardOptions.IgnoreCase);
            foreach (var str in _completions)
                if (wordToCompletePattern.IsMatch(str))
                    yield return new CompletionResult(str, str, CompletionResultType.ParameterValue, str);

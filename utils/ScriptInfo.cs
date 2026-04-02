    /// The command information for scripts that are directly executable by PowerShell.
    public class ScriptInfo : CommandInfo, IScriptCommandInfo
        /// Creates an instance of the ScriptInfo class with the specified name, and script.
        /// The script definition
        /// The execution context for the script.
        /// If <paramref name="script"/> is null.
        internal ScriptInfo(string name, ScriptBlock script, ExecutionContext context)
            : base(name, CommandTypes.Script, context)
                throw PSTraceSource.NewArgumentException(nameof(script));
            this.ScriptBlock = script;
        internal ScriptInfo(ScriptInfo other)
            this.ScriptBlock = other.ScriptBlock;
            ScriptInfo copy = new ScriptInfo(this) { IsGetCommandCopy = true, Arguments = argumentList };
            get { return HelpCategory.ScriptCommand; }
        /// Gets the ScriptBlock that represents the implementation of the script.
        // Path
        /// Gets the definition of the ScriptBlock for the script. This is the ToString() of
        /// the ScriptBlock.
                return ScriptBlock.ToString();

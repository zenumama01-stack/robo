    /// Holds the information for a given breakpoint.
    public abstract class Breakpoint
        /// The action to take when the breakpoint is hit.
        public ScriptBlock Action { get; }
        /// Gets whether this breakpoint is enabled.
        internal void SetEnabled(bool value)
            Enabled = value;
        /// Records how many times this breakpoint has been triggered.
        public int HitCount { get; private set; }
        /// This breakpoint's Id.
        public int Id { get; }
        /// True if breakpoint is set on a script, false if the breakpoint is not scoped.
        internal bool IsScriptBreakpoint
            get { return Script != null; }
        /// The script this breakpoint is on, or null if the breakpoint is not scoped.
        public string Script { get; }
        /// Creates a new instance of a <see cref="Breakpoint"/>
        protected Breakpoint(string script)
            : this(script, null)
        protected Breakpoint(string script, ScriptBlock action)
            Enabled = true;
            Script = string.IsNullOrEmpty(script) ? null : script;
            Id = Interlocked.Increment(ref s_lastID);
            HitCount = 0;
        protected Breakpoint(string script, int id)
            : this(script, null, id)
        protected Breakpoint(string script, ScriptBlock action, int id)
        internal BreakpointAction Trigger()
            ++HitCount;
            if (Action == null)
                return BreakpointAction.Break;
                // Pass this to the action so the breakpoint.  This could be used to
                // implement a "trigger once" breakpoint that disables itself after first hit.
                // One could also share an action across many breakpoints - and hence needs
                // to know something about the breakpoint that is hit, e.g. in a poor mans code coverage tool.
                Action.DoInvoke(dollarUnder: this, input: null, args: Array.Empty<object>());
            catch (BreakException)
            return BreakpointAction.Continue;
        internal virtual bool RemoveSelf(ScriptDebugger debugger) => false;
        #region enums
        internal enum BreakpointAction
            Continue = 0x0,
            Break = 0x1
        #endregion enums
        private static int s_lastID;
    /// A breakpoint on a command.
    public class CommandBreakpoint : Breakpoint
        /// Creates a new instance of a <see cref="CommandBreakpoint"/>
        public CommandBreakpoint(string script, WildcardPattern command, string commandString)
            : this(script, command, commandString, null)
        public CommandBreakpoint(string script, WildcardPattern command, string commandString, ScriptBlock action)
            : base(script, action)
            CommandPattern = command;
            Command = commandString;
        public CommandBreakpoint(string script, WildcardPattern command, string commandString, int id)
            : this(script, command, commandString, null, id)
        public CommandBreakpoint(string script, WildcardPattern command, string commandString, ScriptBlock action, int id)
            : base(script, action, id)
        /// Which command this breakpoint is on.
        public string Command { get; }
        internal WildcardPattern CommandPattern { get; }
        /// Gets a string representation of this breakpoint.
        /// <returns>A string representation of this breakpoint.</returns>
            return IsScriptBreakpoint
                       ? StringUtil.Format(DebuggerStrings.CommandScriptBreakpointString, Script, Command)
                       : StringUtil.Format(DebuggerStrings.CommandBreakpointString, Command);
        internal override bool RemoveSelf(ScriptDebugger debugger) =>
            debugger.RemoveCommandBreakpoint(this);
        private bool CommandInfoMatches(CommandInfo commandInfo)
            if (CommandPattern.IsMatch(commandInfo.Name))
            // If the breakpoint looks like it might have specified a module name and the command
            // we're checking is in a module, try matching the module\command against the pattern
            // in the breakpoint.
            if (!string.IsNullOrEmpty(commandInfo.ModuleName) && Command.Contains('\\'))
                if (CommandPattern.IsMatch(commandInfo.ModuleName + "\\" + commandInfo.Name))
            var externalScript = commandInfo as ExternalScriptInfo;
            if (externalScript != null)
                if (externalScript.Path.Equals(Command, StringComparison.OrdinalIgnoreCase))
                if (CommandPattern.IsMatch(Path.GetFileNameWithoutExtension(externalScript.Path)))
        internal bool Trigger(InvocationInfo invocationInfo)
            // invocationInfo.MyCommand can be null when invoked via ScriptBlock.Invoke()
            if (CommandPattern.IsMatch(invocationInfo.InvocationName) || CommandInfoMatches(invocationInfo.MyCommand))
                return (Script == null || Script.Equals(invocationInfo.ScriptName, StringComparison.OrdinalIgnoreCase));
    /// The access type for variable breakpoints to break on.
    public enum VariableAccessMode
        /// Break on read access only.
        Read,
        /// Break on write access only (default).
        Write,
        /// Breakon read or write access.
        ReadWrite
    /// A breakpoint on a variable.
    public class VariableBreakpoint : Breakpoint
        /// Creates a new instance of a <see cref="VariableBreakpoint"/>.
        public VariableBreakpoint(string script, string variable, VariableAccessMode accessMode)
            : this(script, variable, accessMode, null)
        public VariableBreakpoint(string script, string variable, VariableAccessMode accessMode, ScriptBlock action)
            Variable = variable;
            AccessMode = accessMode;
        public VariableBreakpoint(string script, string variable, VariableAccessMode accessMode, int id)
            : this(script, variable, accessMode, null, id)
        public VariableBreakpoint(string script, string variable, VariableAccessMode accessMode, ScriptBlock action, int id)
        /// The access mode to trigger this variable breakpoint on.
        public VariableAccessMode AccessMode { get; }
        /// Which variable this breakpoint is on.
        public string Variable { get; }
        /// Gets the string representation of this breakpoint.
        /// <returns>The string representation of this breakpoint.</returns>
                       ? StringUtil.Format(DebuggerStrings.VariableScriptBreakpointString, Script, Variable, AccessMode)
                       : StringUtil.Format(DebuggerStrings.VariableBreakpointString, Variable, AccessMode);
        internal bool Trigger(string currentScriptFile, bool read)
            if (!Enabled)
            if (AccessMode != VariableAccessMode.ReadWrite && AccessMode != (read ? VariableAccessMode.Read : VariableAccessMode.Write))
            if (Script == null || Script.Equals(currentScriptFile, StringComparison.OrdinalIgnoreCase))
                return Trigger() == BreakpointAction.Break;
            debugger.RemoveVariableBreakpoint(this);
    /// A breakpoint on a line or statement.
    public class LineBreakpoint : Breakpoint
        /// Creates a new instance of a <see cref="LineBreakpoint"/>
        public LineBreakpoint(string script, int line)
            : this(script, line, null)
        public LineBreakpoint(string script, int line, ScriptBlock action)
            Diagnostics.Assert(!string.IsNullOrEmpty(script), "Caller to verify script parameter is not null or empty.");
            Column = 0;
            SequencePointIndex = -1;
        public LineBreakpoint(string script, int line, int column)
            : this(script, line, column, null)
        public LineBreakpoint(string script, int line, int column, ScriptBlock action)
            Column = column;
        public LineBreakpoint(string script, int line, int column, int id)
            : this(script, line, column, null, id)
        public LineBreakpoint(string script, int line, int column, ScriptBlock action, int id)
        /// Which column this breakpoint is on.
        public int Column { get; }
        /// Which line this breakpoint is on.
        public int Line { get; }
            return Column == 0
                       ? StringUtil.Format(DebuggerStrings.LineBreakpointString, Script, Line)
                       : StringUtil.Format(DebuggerStrings.StatementBreakpointString, Script, Line, Column);
        internal int SequencePointIndex { get; set; }
        internal IScriptExtent[] SequencePoints { get; set; }
        internal BitArray BreakpointBitArray { get; set; }
        private sealed class CheckBreakpointInScript : AstVisitor
            public static bool IsInNestedScriptBlock(Ast ast, LineBreakpoint breakpoint)
                var visitor = new CheckBreakpointInScript { _breakpoint = breakpoint };
                ast.InternalVisit(visitor);
                return visitor._result;
            private LineBreakpoint _breakpoint;
            private bool _result;
                if (functionDefinitionAst.Extent.ContainsLineAndColumn(_breakpoint.Line, _breakpoint.Column))
                    _result = true;
                // We don't need to visit the body, we're just checking extents of the topmost functions.
                // We'll visit the bodies eventually, but only when the nested function/script is executed.
                if (scriptBlockExpressionAst.Extent.ContainsLineAndColumn(_breakpoint.Line, _breakpoint.Column))
        internal bool TrySetBreakpoint(string scriptFile, FunctionContext functionContext)
            Diagnostics.Assert(SequencePointIndex == -1, "shouldn't be trying to set on a pending breakpoint");
            // A quick check to see if the breakpoint is within the scriptblock.
            bool couldBeInNestedScriptBlock;
            var scriptBlock = functionContext._scriptBlock;
                var ast = scriptBlock.Ast;
                if (!ast.Extent.ContainsLineAndColumn(Line, Column))
                var sequencePoints = functionContext._sequencePoints;
                if (sequencePoints.Length == 1 && sequencePoints[0] == scriptBlock.Ast.Extent)
                    // If there was no real executable code in the function (e.g. only function definitions),
                    // we added the entire scriptblock as a sequence point, but it shouldn't be allowed as a breakpoint.
                couldBeInNestedScriptBlock = CheckBreakpointInScript.IsInNestedScriptBlock(((IParameterMetadataProvider)ast).Body, this);
                couldBeInNestedScriptBlock = false;
            int sequencePointIndex;
            var sequencePoint = FindSequencePoint(functionContext, Line, Column, out sequencePointIndex);
            if (sequencePoint != null)
                // If the bp could be in a nested script block, we want to be careful and get the bp in the correct script block.
                // If it's a simple line bp (no column specified), then the start line must match the bp line exactly, otherwise
                // we assume the bp is in the nested script block.
                if (!couldBeInNestedScriptBlock || (sequencePoint.StartLineNumber == Line && Column == 0))
                    SetBreakpoint(functionContext, sequencePointIndex);
            // Before using heuristics, make sure the breakpoint isn't in a nested function/script block.
            if (couldBeInNestedScriptBlock)
            // Not found.  First, we check if the line/column is before any real code.  If so, we'll
            // move the breakpoint to the first interesting sequence point (could be a dynamicparam,
            // begin, process, end, or clean block.)
                var bodyAst = ((IParameterMetadataProvider)ast).Body;
                if ((bodyAst.DynamicParamBlock == null || bodyAst.DynamicParamBlock.Extent.IsAfter(Line, Column))
                    && (bodyAst.BeginBlock == null || bodyAst.BeginBlock.Extent.IsAfter(Line, Column))
                    && (bodyAst.ProcessBlock == null || bodyAst.ProcessBlock.Extent.IsAfter(Line, Column))
                    && (bodyAst.EndBlock == null || bodyAst.EndBlock.Extent.IsAfter(Line, Column))
                    && (bodyAst.CleanBlock == null || bodyAst.CleanBlock.Extent.IsAfter(Line, Column)))
                    SetBreakpoint(functionContext, 0);
            // Still not found.  Try fudging a bit, but only if it's a simple line breakpoint.
            if (Column == 0 && FindSequencePoint(functionContext, Line + 1, 0, out sequencePointIndex) != null)
        private static IScriptExtent FindSequencePoint(FunctionContext functionContext, int line, int column, out int sequencePointIndex)
            for (int i = 0; i < sequencePoints.Length; ++i)
                var extent = sequencePoints[i];
                if (extent.ContainsLineAndColumn(line, column))
                    sequencePointIndex = i;
                    return extent;
            sequencePointIndex = -1;
        private void SetBreakpoint(FunctionContext functionContext, int sequencePointIndex)
            // Remember the bitarray so we when the last breakpoint is removed, we can avoid
            // stopping at the sequence point.
            this.BreakpointBitArray = functionContext._breakPoints;
            this.SequencePoints = functionContext._sequencePoints;
            SequencePointIndex = sequencePointIndex;
            this.BreakpointBitArray.Set(SequencePointIndex, true);
        internal override bool RemoveSelf(ScriptDebugger debugger)
            if (this.SequencePoints != null)
                // Remove ourselves from the list of bound breakpoints in this script.  It's possible the breakpoint was never
                // bound, in which case there is nothing to do.
                var boundBreakPoints = debugger.GetBoundBreakpoints(this.SequencePoints);
                if (boundBreakPoints != null)
                    Diagnostics.Assert(boundBreakPoints[this.SequencePointIndex].Contains(this),
                                       "If we set _scriptBlock, we should have also added the breakpoint to the bound breakpoint list");
                    boundBreakPoints[this.SequencePointIndex].Remove(this);
                    if (boundBreakPoints[this.SequencePointIndex].All(breakpoint => breakpoint.SequencePointIndex != this.SequencePointIndex))
                        // No other line breakpoints are at the same sequence point, so disable the breakpoint so
                        // we don't go looking for breakpoints the next time we hit the sequence point.
                        // This isn't strictly necessary, but script execution will be faster.
                        this.BreakpointBitArray.Set(SequencePointIndex, false);
            return debugger.RemoveLineBreakpoint(this);

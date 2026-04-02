    /// Types of breakpoints.
    public enum BreakpointType
        /// <summary>Breakpoint on a line within a script</summary>
        Line,
        /// <summary>Breakpoint on a variable</summary>
        Variable,
        /// <summary>Breakpoint on a command</summary>
        Command
    /// This class implements Get-PSBreakpoint.
    [Cmdlet(VerbsCommon.Get, "PSBreakpoint", DefaultParameterSetName = LineParameterSetName, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097108")]
    [OutputType(typeof(CommandBreakpoint), ParameterSetName = new[] { CommandParameterSetName })]
    [OutputType(typeof(LineBreakpoint), ParameterSetName = new[] { LineParameterSetName })]
    [OutputType(typeof(VariableBreakpoint), ParameterSetName = new[] { VariableParameterSetName })]
    [OutputType(typeof(Breakpoint), ParameterSetName = new[] { TypeParameterSetName, IdParameterSetName })]
    public class GetPSBreakpointCommand : PSBreakpointAccessorCommandBase
        #region strings
        internal const string TypeParameterSetName = "Type";
        internal const string IdParameterSetName = "Id";
        #endregion strings
        /// Scripts of the breakpoints to output.
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "It's OK to use arrays for cmdlet parameters")]
        [Parameter(ParameterSetName = LineParameterSetName, Position = 0, ValueFromPipeline = true)]
        [Parameter(ParameterSetName = CommandParameterSetName)]
        [Parameter(ParameterSetName = VariableParameterSetName)]
        [Parameter(ParameterSetName = TypeParameterSetName)]
        public string[] Script { get; set; }
        /// IDs of the breakpoints to output.
        [Parameter(ParameterSetName = IdParameterSetName, Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public int[] Id { get; set; }
        /// Variables of the breakpoints to output.
        [Parameter(ParameterSetName = VariableParameterSetName, Mandatory = true)]
        public string[] Variable { get; set; }
        /// Commands of the breakpoints to output.
        [Parameter(ParameterSetName = CommandParameterSetName, Mandatory = true)]
        public string[] Command { get; set; }
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Type is OK for a cmdlet parameter")]
        [Parameter(ParameterSetName = TypeParameterSetName, Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public BreakpointType[] Type { get; set; }
        /// Remove breakpoints.
            List<Breakpoint> breakpoints = Runspace.Debugger.GetBreakpoints();
            // Filter by parameter set
            if (ParameterSetName.Equals(LineParameterSetName, StringComparison.OrdinalIgnoreCase))
                // no filter
            else if (ParameterSetName.Equals(IdParameterSetName, StringComparison.OrdinalIgnoreCase))
                breakpoints = Filter(
                    breakpoints,
                    Id,
                    static (Breakpoint breakpoint, int id) => breakpoint.Id == id);
            else if (ParameterSetName.Equals(CommandParameterSetName, StringComparison.OrdinalIgnoreCase))
                    Command,
                    (Breakpoint breakpoint, string command) =>
                        if (breakpoint is not CommandBreakpoint commandBreakpoint)
                        return commandBreakpoint.Command.Equals(command, StringComparison.OrdinalIgnoreCase);
            else if (ParameterSetName.Equals(VariableParameterSetName, StringComparison.OrdinalIgnoreCase))
                    (Breakpoint breakpoint, string variable) =>
                        if (breakpoint is not VariableBreakpoint variableBreakpoint)
                        return variableBreakpoint.Variable.Equals(variable, StringComparison.OrdinalIgnoreCase);
            else if (ParameterSetName.Equals(TypeParameterSetName, StringComparison.OrdinalIgnoreCase))
                    Type,
                    (Breakpoint breakpoint, BreakpointType type) =>
                            case BreakpointType.Line:
                                if (breakpoint is LineBreakpoint)
                            case BreakpointType.Command:
                                if (breakpoint is CommandBreakpoint)
                            case BreakpointType.Variable:
                                if (breakpoint is VariableBreakpoint)
            // Filter by script
            if (Script != null)
                    Script,
                    (Breakpoint breakpoint, string script) =>
                        if (breakpoint.Script == null)
                        return string.Equals(
                            SessionState.Path.GetUnresolvedProviderPathFromPSPath(breakpoint.Script),
                            SessionState.Path.GetUnresolvedProviderPathFromPSPath(script),
                            StringComparison.OrdinalIgnoreCase
            // Output results
            foreach (Breakpoint b in breakpoints)
                ProcessBreakpoint(b);
        /// Gives the criteria to filter breakpoints.
        private delegate bool FilterSelector<T>(Breakpoint breakpoint, T target);
        /// Returns the items in the input list that match an item in the filter array according to
        /// the given selection criterion.
        private static List<Breakpoint> Filter<T>(List<Breakpoint> input, T[] filter, FilterSelector<T> selector)
            List<Breakpoint> output = new();
            for (int i = 0; i < input.Count; i++)
                for (int j = 0; j < filter.Length; j++)
                    if (selector(input[i], filter[j]))
                        output.Add(input[i]);

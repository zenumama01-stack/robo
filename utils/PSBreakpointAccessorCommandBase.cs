    /// Base class for Get/Set-PSBreakpoint.
    public abstract class PSBreakpointAccessorCommandBase : PSBreakpointCommandBase
        internal const string CommandParameterSetName = "Command";
        internal const string LineParameterSetName = "Line";
        internal const string VariableParameterSetName = "Variable";

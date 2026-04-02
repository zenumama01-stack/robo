    /// This class implements Remove-PSBreakpoint.
    [Cmdlet(VerbsCommon.Remove, "PSBreakpoint", SupportsShouldProcess = true, DefaultParameterSetName = BreakpointParameterSetName,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097134")]
    public class RemovePSBreakpointCommand : PSBreakpointUpdaterCommandBase
        /// Removes the given breakpoint.
            Runspace.Debugger.RemoveBreakpoint(breakpoint);

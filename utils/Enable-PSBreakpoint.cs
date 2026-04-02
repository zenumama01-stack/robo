    /// This class implements Enable-PSBreakpoint.
    [Cmdlet(VerbsLifecycle.Enable, "PSBreakpoint", SupportsShouldProcess = true, DefaultParameterSetName = BreakpointParameterSetName, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096700")]
    public class EnablePSBreakpointCommand : PSBreakpointUpdaterCommandBase
        /// Enables the given breakpoint.
            breakpoint = Runspace.Debugger.EnableBreakpoint(breakpoint);

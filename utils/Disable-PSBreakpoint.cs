    /// This class implements Disable-PSBreakpoint.
    [Cmdlet(VerbsLifecycle.Disable, "PSBreakpoint", SupportsShouldProcess = true, DefaultParameterSetName = BreakpointParameterSetName, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096498")]
    [OutputType(typeof(Breakpoint))]
    public class DisablePSBreakpointCommand : PSBreakpointUpdaterCommandBase
        /// Gets or sets the parameter -passThru which states whether the
        /// command should place the breakpoints it processes in the pipeline.
        /// Disables the given breakpoint.
        protected override void ProcessBreakpoint(Breakpoint breakpoint)
            breakpoint = Runspace.Debugger.DisableBreakpoint(breakpoint);
                base.ProcessBreakpoint(breakpoint);
        #endregion overrides

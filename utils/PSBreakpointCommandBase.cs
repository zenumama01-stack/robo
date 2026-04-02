    /// Base class for PSBreakpoint cmdlets.
    public abstract class PSBreakpointCommandBase : PSCmdlet
        /// Gets or sets the runspace where the breakpoints will be used.
        [Runspace]
        public virtual Runspace Runspace { get; set; }
        /// Identifies the default runspace.
            Runspace ??= Context.CurrentRunspace;
        /// Write the given breakpoint out to the pipeline, decorated with the runspace instance id if appropriate.
        /// <param name="breakpoint">The breakpoint to write to the pipeline.</param>
        protected virtual void ProcessBreakpoint(Breakpoint breakpoint)
            if (Runspace != Context.CurrentRunspace)
                var pso = new PSObject(breakpoint);
                pso.Properties.Add(new PSNoteProperty(RemotingConstants.RunspaceIdNoteProperty, Runspace.InstanceId));
                WriteObject(pso);
                WriteObject(breakpoint);

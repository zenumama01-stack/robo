    /// Base class for Enable/Disable/Remove-PSBreakpoint.
    public abstract class PSBreakpointUpdaterCommandBase : PSBreakpointCommandBase
        internal const string BreakpointParameterSetName = "Breakpoint";
        /// Gets or sets the breakpoint to enable.
        [Parameter(ParameterSetName = BreakpointParameterSetName, ValueFromPipeline = true, Position = 0, Mandatory = true)]
        public Breakpoint[] Breakpoint { get; set; }
        /// Gets or sets the Id of the breakpoint to enable.
        [Parameter(ParameterSetName = IdParameterSetName, ValueFromPipelineByPropertyName = true, Position = 0, Mandatory = true)]
        [Parameter(ParameterSetName = IdParameterSetName, ValueFromPipelineByPropertyName = true)]
        [Alias("RunspaceId")]
        public override Runspace Runspace { get; set; }
        /// Gathers the list of breakpoints to process and calls ProcessBreakpoints.
            if (ParameterSetName.Equals(BreakpointParameterSetName, StringComparison.OrdinalIgnoreCase))
                foreach (Breakpoint breakpoint in Breakpoint)
                    if (ShouldProcessInternal(breakpoint.ToString()) &&
                        TryGetRunspace(breakpoint))
                        ProcessBreakpoint(breakpoint);
                    ParameterSetName.Equals(IdParameterSetName, StringComparison.OrdinalIgnoreCase),
                    $"There should be no other parameter sets besides '{BreakpointParameterSetName}' and '{IdParameterSetName}'.");
                foreach (int id in Id)
                    Breakpoint breakpoint;
                    if (TryGetBreakpoint(id, out breakpoint) &&
                        ShouldProcessInternal(breakpoint.ToString()))
        #region private data
        private readonly Dictionary<Guid, Runspace> runspaces = new();
        #endregion private data
        private bool TryGetRunspace(Breakpoint breakpoint)
            // Breakpoints retrieved from another runspace will have a RunspaceId note property of type Guid on them.
            var runspaceInstanceIdProperty = pso.Properties[RemotingConstants.RunspaceIdNoteProperty];
            if (runspaceInstanceIdProperty == null)
                Runspace = Context.CurrentRunspace;
            Debug.Assert(runspaceInstanceIdProperty.TypeNameOfValue.Equals("System.Guid", StringComparison.OrdinalIgnoreCase), "Instance ids must be GUIDs.");
            var runspaceInstanceId = (Guid)runspaceInstanceIdProperty.Value;
            if (runspaces.ContainsKey(runspaceInstanceId))
                Runspace = runspaces[runspaceInstanceId];
            var matchingRunspaces = GetRunspaceUtils.GetRunspacesByInstanceId(new[] { runspaceInstanceId });
            if (matchingRunspaces.Count != 1)
                        new ArgumentException(StringUtil.Format(Debugger.RunspaceInstanceIdNotFound, runspaceInstanceId)),
                        "PSBreakpoint:RunspaceInstanceIdNotFound",
            Runspace = runspaces[runspaceInstanceId] = matchingRunspaces[0];
        private bool TryGetBreakpoint(int id, out Breakpoint breakpoint)
            breakpoint = Runspace.Debugger.GetBreakpoint(id);
            if (breakpoint == null)
                        new ArgumentException(StringUtil.Format(Debugger.BreakpointIdNotFound, id)),
                        "PSBreakpoint:BreakpointIdNotFound",
        private bool ShouldProcessInternal(string target)
            // ShouldProcess should be called only if the WhatIf or Confirm parameters are passed in explicitly.
            // It should *not* be called if we are in a nested debug prompt and the current running command was
            // run with -WhatIf or -Confirm, because this prevents the user from adding/removing breakpoints inside
            // a debugger stop.
            if (MyInvocation.BoundParameters.ContainsKey("WhatIf") ||
                MyInvocation.BoundParameters.ContainsKey("Confirm"))
                return ShouldProcess(target);

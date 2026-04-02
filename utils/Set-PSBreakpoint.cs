    /// This class implements Set-PSBreakpoint command.
    [Cmdlet(VerbsCommon.Set, "PSBreakpoint", DefaultParameterSetName = LineParameterSetName, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096623")]
    [OutputType(typeof(CommandBreakpoint), ParameterSetName = new string[] { CommandParameterSetName })]
    [OutputType(typeof(LineBreakpoint), ParameterSetName = new string[] { LineParameterSetName })]
    [OutputType(typeof(VariableBreakpoint), ParameterSetName = new string[] { VariableParameterSetName })]
    public class SetPSBreakpointCommand : PSBreakpointAccessorCommandBase
        /// Gets or sets the action to take when hitting this breakpoint.
        [Parameter(ParameterSetName = LineParameterSetName)]
        public ScriptBlock Action { get; set; }
        /// Gets or sets the column to set the breakpoint on.
        [Parameter(Position = 2, ParameterSetName = LineParameterSetName)]
        public int Column { get; set; }
        /// Gets or sets the command(s) to set the breakpoint on.
        [Alias("C")]
        /// Gets or sets the line to set the breakpoint on.
        [Parameter(Position = 1, ParameterSetName = LineParameterSetName, Mandatory = true)]
        public int[] Line { get; set; }
        /// Gets or sets the script to set the breakpoint on.
        [Parameter(ParameterSetName = CommandParameterSetName, Position = 0)]
        [Parameter(ParameterSetName = LineParameterSetName, Mandatory = true, Position = 0)]
        [Parameter(ParameterSetName = VariableParameterSetName, Position = 0)]
        /// Gets or sets the variables to set the breakpoint(s) on.
        [Alias("V")]
        /// Gets or sets the access type for variable breakpoints to break on.
        public VariableAccessMode Mode { get; set; } = VariableAccessMode.Write;
        /// Verifies that debugging is supported.
            // Call the base method to ensure Runspace is initialized properly.
            // Check whether we are executing on a remote session and if so
            // whether the RemoteScript debug option is selected.
            if (this.Context.InternalHost.ExternalHost is System.Management.Automation.Remoting.ServerRemoteHost &&
                ((this.Context.CurrentRunspace == null) || (this.Context.CurrentRunspace.Debugger == null) ||
                 ((this.Context.CurrentRunspace.Debugger.DebugMode & DebugModes.RemoteScript) != DebugModes.RemoteScript) &&
                  (this.Context.CurrentRunspace.Debugger.DebugMode != DebugModes.None)))
                        new PSNotSupportedException(Debugger.RemoteDebuggerNotSupportedInHost),
                        "SetPSBreakpoint:RemoteDebuggerNotSupported",
            // If we're in ConstrainedLanguage mode and the system is not in lockdown mode,
            // don't allow breakpoints as we can't protect that boundary.
            // This covers the case where the debugger could modify variables in a trusted
            // script block.  So debugging is supported in Constrained language mode only if
            // the system is also in lock down mode.
            if ((Context.LanguageMode == PSLanguageMode.ConstrainedLanguage) &&
                (System.Management.Automation.Security.SystemPolicy.GetSystemLockdownPolicy() !=
                 System.Management.Automation.Security.SystemEnforcementMode.Enforce))
                        new PSNotSupportedException(Debugger.RemoteDebuggerNotSupported),
                            "CannotSetBreakpointInconsistentLanguageMode",
                            Context.LanguageMode));
        /// Set a new breakpoint.
            // If there is a script, resolve its path
            Collection<string> scripts = new();
                foreach (string script in Script)
                    Collection<PathInfo> scriptPaths = SessionState.Path.GetResolvedPSPathFromPSPath(script);
                    for (int i = 0; i < scriptPaths.Count; i++)
                        string providerPath = scriptPaths[i].ProviderPath;
                        if (!File.Exists(providerPath))
                                    new ArgumentException(StringUtil.Format(Debugger.FileDoesNotExist, providerPath)),
                                    "NewPSBreakpoint:FileDoesNotExist",
                        string extension = Path.GetExtension(providerPath);
                        if (!extension.Equals(".ps1", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".psm1", StringComparison.OrdinalIgnoreCase))
                                    new ArgumentException(StringUtil.Format(Debugger.WrongExtension, providerPath)),
                                    "NewPSBreakpoint:WrongExtension",
                        scripts.Add(Path.GetFullPath(providerPath));
            // If it is a command breakpoint...
            if (ParameterSetName.Equals(CommandParameterSetName, StringComparison.OrdinalIgnoreCase))
                for (int i = 0; i < Command.Length; i++)
                    if (scripts.Count > 0)
                        foreach (string path in scripts)
                            ProcessBreakpoint(
                                Runspace.Debugger.SetCommandBreakpoint(Command[i], Action, path));
                            Runspace.Debugger.SetCommandBreakpoint(Command[i], Action, path: null));
            // If it is a variable breakpoint...
                for (int i = 0; i < Variable.Length; i++)
                                Runspace.Debugger.SetVariableBreakpoint(Variable[i], Mode, Action, path));
                            Runspace.Debugger.SetVariableBreakpoint(Variable[i], Mode, Action, path: null));
            // Else it is the default parameter set (Line breakpoint)...
                Debug.Assert(ParameterSetName.Equals(LineParameterSetName, StringComparison.OrdinalIgnoreCase));
                for (int i = 0; i < Line.Length; i++)
                    if (Line[i] < 1)
                                new ArgumentException(Debugger.LineLessThanOne),
                                "SetPSBreakpoint:LineLessThanOne",
                            Runspace.Debugger.SetLineBreakpoint(path, Line[i], Column, Action));

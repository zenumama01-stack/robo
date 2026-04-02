    /// A cmdlet that sets the properties of the TraceSwitch instances that are instantiated in the process.
    [Cmdlet(VerbsCommon.Set, "TraceSource", DefaultParameterSetName = "optionsSet", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097129")]
    public class SetTraceSourceCommand : TraceListenerCommandBase
        /// The TraceSource parameter determines which TraceSource categories the
        /// operation will take place on.
            get { return base.NameInternal; }
            set { base.NameInternal = value; }
        /// The flags to be set on the TraceSource.
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, ParameterSetName = "optionsSet")]
        public PSTraceSourceOptions Option
                return base.OptionsInternal;
                base.OptionsInternal = value;
        /// The parameter which determines the options for output from the trace listeners.
        [Parameter(ParameterSetName = "optionsSet")]
        public TraceOptions ListenerOption
                return base.ListenerOptionsInternal;
                base.ListenerOptionsInternal = value;
        /// Adds the file trace listener using the specified file.
            get { return base.FileListener; }
            set { base.FileListener = value; }
        /// Force parameter to control read-only files.
            get { return base.ForceWrite; }
            set { base.ForceWrite = value; }
        /// If this parameter is specified the Debugger trace listener will be added.
        public SwitchParameter Debugger
            get { return base.DebuggerListener; }
            set { base.DebuggerListener = value; }
        /// If this parameter is specified the PSHost trace listener will be added.
        public SwitchParameter PSHost
            get { return base.PSHostListener; }
            set { base.PSHostListener = value; }
        /// If set, the specified listeners will be removed regardless of their type.
        [Parameter(ParameterSetName = "removeAllListenersSet")]
        public string[] RemoveListener { get; set; } = new string[] { "*" };
        /// If set, the specified file trace listeners will be removed.
        [Parameter(ParameterSetName = "removeFileListenersSet")]
        public string[] RemoveFileListener { get; set; } = new string[] { "*" };
        /// Determines if the modified PSTraceSource should be written out.
        /// Default is false.
        /// Sets the TraceSource properties.
            Collection<PSTraceSource> matchingSources = null;
                case "optionsSet":
                    Collection<PSTraceSource> preconfiguredTraceSources = null;
                    matchingSources = ConfigureTraceSource(Name, true, out preconfiguredTraceSources);
                        WriteObject(matchingSources, true);
                        WriteObject(preconfiguredTraceSources, true);
                case "removeAllListenersSet":
                    matchingSources = GetMatchingTraceSource(Name, true);
                    RemoveListenersByName(matchingSources, RemoveListener, false);
                case "removeFileListenersSet":
                    RemoveListenersByName(matchingSources, RemoveFileListener, true);

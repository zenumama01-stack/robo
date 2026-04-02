    /// Null sink to absorb pipeline output.
    [Cmdlet("Out", "Null", SupportsShouldProcess = false,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096792", RemotingCapability = RemotingCapability.None)]
    public class OutNullCommand : PSCmdlet
        /// Do nothing.
            // explicitely overridden:
            // do not do any processing
    /// Implementation for the out-default command
    /// this command it implicitly inject by the
    /// powershell host at the end of the pipeline as the
    /// default sink (display to console screen)
    [Cmdlet(VerbsData.Out, "Default", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096486", RemotingCapability = RemotingCapability.None)]
    public class OutDefaultCommand : FrontEndCommandBase
        /// Determines whether objects should be sent to API consumers.
        /// This command is automatically added to the pipeline when PowerShell is transcribing and
        /// invoked via API. This ensures that the objects pass through the formatting and output
        /// system, but can still make it to the API consumer.
        public SwitchParameter Transcript { get; set; }
        /// Set inner command.
        public OutDefaultCommand()
        /// Just hook up the LineOutput interface.
            var lineOutput = new ConsoleLineOutput(Host, false, new TerminatingErrorContext(this));
            ((OutputManagerInner)this.implementation).LineOutput = lineOutput;
            if (this.CommandRuntime is MshCommandRuntime mrt)
                mrt.MergeUnclaimedPreviousErrorResults = true;
            if (Transcript)
                _transcribeOnlyCookie = Host.UI.SetTranscribeOnly();
            // This needs to be done directly through the command runtime, as Out-Default
            // doesn't actually write pipeline objects.
            if (Context.CurrentCommandProcessor.CommandRuntime.OutVarList != null)
                _outVarResults = new List<PSObject>();
        /// Process the OutVar, if set.
            if (_outVarResults != null)
                object inputObjectBase = PSObject.Base(InputObject);
                // Ignore errors and formatting records, as those can't be captured
                if (inputObjectBase != null &&
                    inputObjectBase is not ErrorRecord &&
                    !inputObjectBase.GetType().FullName.StartsWith(
                        "Microsoft.PowerShell.Commands.Internal.Format", StringComparison.OrdinalIgnoreCase))
                    _outVarResults.Add(InputObject);
        /// Swap the outVar with what we've processed, if OutVariable is set.
            if ((_outVarResults != null) && (_outVarResults.Count > 0))
                Context.CurrentCommandProcessor.CommandRuntime.OutVarList.Clear();
                foreach (object item in _outVarResults)
                    Context.CurrentCommandProcessor.CommandRuntime.OutVarList.Add(item);
                _outVarResults = null;
        /// Revert transcription state on Dispose.
                if (_transcribeOnlyCookie != null)
                    _transcribeOnlyCookie.Dispose();
                    _transcribeOnlyCookie = null;
        private List<PSObject> _outVarResults = null;
        private IDisposable _transcribeOnlyCookie = null;
    /// Implementation for the out-host command.
    [Cmdlet(VerbsData.Out, "Host", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096863", RemotingCapability = RemotingCapability.None)]
    public class OutHostCommand : FrontEndCommandBase
        /// Non positional parameter to specify paging.
        private bool _paging;
        /// Constructor of OutHostCommand.
        public OutHostCommand()
        /// Optional, non positional parameter to specify paging
        /// FALSE: names only
        /// TRUE: full info.
        public SwitchParameter Paging
            get { return _paging; }
            set { _paging = value; }
            var lineOutput = new ConsoleLineOutput(Host, _paging, new TerminatingErrorContext(this));
